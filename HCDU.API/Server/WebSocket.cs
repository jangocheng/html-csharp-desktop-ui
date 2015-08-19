using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using HCDU.API.Http;

namespace HCDU.API.Server
{
    public class WebSocket
    {
        private readonly NetworkStream stream;
        private bool isClosed;

        public WebSocket(NetworkStream stream)
        {
            this.stream = stream;
            this.isClosed = false;
        }

        //todo: rename to handler?
        public ISocket Socket { get; set; }

        public void HandleRequest(HttpRequest request)
        {
            //todo: this method belongs to WebServer
            ProcessRequest(request);

            WebSocketFrameHeader messageHeader = null;
            MemoryStream messageContent = new MemoryStream();
            ulong messageLength = 0;

            //todo: stop gracefully
            while (!isClosed)
            {
                WebSocketFrame frame = ReadFrame();

                if (WebSocketOpcodes.IsControlFrame(frame.Header.OpCode))
                {
                    //todo: remove magic number
                    if (!frame.Header.IsLast || frame.Header.PayloadLength > 125)
                    {
                        throw new HcduException(string.Format("Invalid control frame (Opcode: {0}, FIN: {1}, Length: {2}).", frame.Header.OpCode, frame.Header.IsLast, frame.Header.PayloadLength));
                    }
                    HandleControlFrame(frame);
                }

                if (messageHeader == null)
                {
                    if (frame.Header.OpCode == WebSocketOpcodes.ContinuationFrame)
                    {
                        throw new HcduException("Continuation frame cannot start message.");
                    }
                    messageHeader = frame.Header;
                }

                messageLength += frame.Header.PayloadLength;
                if (messageLength > int.MaxValue)
                {
                    throw new HcduException("Long messages are not supported.");
                }

                messageContent.Write(frame.Content, 0, frame.Content.Length);

                if (frame.Header.IsLast)
                {
                    WebSocketMessage message = new WebSocketMessage();
                    message.OpCode = messageHeader.OpCode;
                    message.Content = messageContent.ToArray();

                    messageHeader = null;
                    messageContent = new MemoryStream();

                    HandleMessage(message);
                }
            }
        }

        private void HandleControlFrame(WebSocketFrame frame)
        {
            if (frame.Header.OpCode == WebSocketOpcodes.ConnectionCloseFrame)
            {
                try
                {
                    SendMessage(WebSocketOpcodes.ConnectionCloseFrame, frame.Content);
                }
                catch (IOException)
                {
                    //todo: use more general approach for exception handling
                }
                isClosed = true;
            }
            if (frame.Header.OpCode == WebSocketOpcodes.PingFrame)
            {
                //todo: send pong
            }
            if (frame.Header.OpCode == WebSocketOpcodes.PongFrame)
            {
                //todo: check pong
            }
        }

        private void HandleMessage(WebSocketMessage message)
        {
            //todo: allow Socket to handle opcodes?
            if (message.OpCode == WebSocketOpcodes.TextFrame)
            {
                Socket.ProcessMessage(message.GetText());
            }
            else if (message.OpCode == WebSocketOpcodes.BinaryFrame)
            {
                Socket.ProcessMessage(message.Content);
            }
            //todo: close connection?
        }

        private WebSocketFrame ReadFrame()
        {
            WebSocketFrameHeader frameHeader = ReadFrameHeader();
            if (frameHeader.PayloadLength > int.MaxValue)
            {
                throw new HcduException("Long messages are not supported.");
            }
            if (frameHeader.Mask == null)
            {
                throw new HcduException("Client messages should be masked.");
            }

            byte[] content = HttpUtils.ReadBlock(stream, (int) frameHeader.PayloadLength);

            if (frameHeader.Mask != null)
            {
                for (int i = 0; i < content.Length; i++)
                {
                    content[i] = (byte) (content[i] ^ frameHeader.Mask[i%4]);
                }
            }

            WebSocketFrame frame = new WebSocketFrame();
            frame.Header = frameHeader;
            frame.Content = content;
            return frame;
        }

        private WebSocketFrameHeader ReadFrameHeader()
        {
            byte[] primaryHeader = HttpUtils.ReadBlock(stream, 2);

            WebSocketFrameHeader header = new WebSocketFrameHeader();
            header.IsLast = (primaryHeader[0] & 0x80) != 0;
            header.OpCode = (byte) (primaryHeader[0] & 0x0F);
            header.PayloadLength = (byte) (primaryHeader[1] & 0x7F);
            if (header.PayloadLength == 126)
            {
                byte[] extLengthHeader = HttpUtils.ReadBlock(stream, 2);
                header.PayloadLength = BitConverter.ToUInt16(extLengthHeader, 0);
            }
            if (header.PayloadLength == 127)
            {
                byte[] extLengthHeader = HttpUtils.ReadBlock(stream, 8);
                header.PayloadLength = BitConverter.ToUInt64(extLengthHeader, 0);
            }
            if ((primaryHeader[0] & 0x80) != 0)
            {
                header.Mask = HttpUtils.ReadBlock(stream, 4);
            }

            return header;
        }

        //WebSocket handshake is described here: https://tools.ietf.org/html/rfc6455#section-4.2.2
        private void ProcessRequest(HttpRequest request)
        {
            HttpHeader clientKeyHeader = request.Headers.FirstOrDefault(h => h.Name == HttpHeader.SecWebsocketKey);
            if (clientKeyHeader == null || string.IsNullOrWhiteSpace(clientKeyHeader.Value))
            {
                //todo: send error response instead
                throw new HcduException(string.Format("{0} header is missing.", HttpHeader.SecWebsocketKey));
            }
            string clientKey = clientKeyHeader.Value;
            string serverKey = CreateServerKey(clientKey);

            HttpUtils.WriteLine(stream, "HTTP/1.1 101 Switching Protocols");
            HttpUtils.WriteHeader(stream, HttpHeader.Upgrade, HttpHeader.UpgradeWebsocket);
            HttpUtils.WriteHeader(stream, HttpHeader.Connection, HttpHeader.ConnectionUpgrade);
            HttpUtils.WriteHeader(stream, HttpHeader.SecWebSocketAccept, serverKey);
            HttpUtils.WriteLine(stream, "");
        }

        public void SendMessage(string message)
        {
            UTF8Encoding encoding = new UTF8Encoding(false);
            byte[] content = encoding.GetBytes(message);
            SendMessage(WebSocketOpcodes.TextFrame, content);
        }

        //WebSocket message is desribed here: https://tools.ietf.org/html/rfc6455#section-5.2
        private void SendMessage(byte opcode, byte[] content)
        {
            //todo: add lock and make sure fragmented messages does not intersect
            if (isClosed)
            {
                //todo: throw Exception?
                return;
            }
            if (content.Length > 125)
            {
                throw new HcduException("Long messages are not supported.");
            }
            byte[] frameHeader = new byte[2];
            frameHeader[0] = (byte) (0x80 | opcode);
            frameHeader[1] = (byte) content.Length;
            stream.Write(frameHeader, 0, frameHeader.Length);
            stream.Write(content, 0, content.Length);
        }

        private string CreateServerKey(string clientKey)
        {
            string value = clientKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            SHA1 sha1 = SHA1.Create();
            byte[] valueSha1 = sha1.ComputeHash(Encoding.ASCII.GetBytes(value));
            return Convert.ToBase64String(valueSha1);
        }
    }
}