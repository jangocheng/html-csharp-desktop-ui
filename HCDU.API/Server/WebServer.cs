﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using HCDU.API.Http;

namespace HCDU.API.Server
{
    public class WebServer
    {
        private readonly ContentPackage contentPackage;
        private readonly TcpListener tcpListener;

        public WebServer(ContentPackage contentPackage, int port)
        {
            this.contentPackage = contentPackage;
            tcpListener = new TcpListener(IPAddress.Loopback, port);
        }

        public void Start()
        {
            tcpListener.Start();

            Thread thread = new Thread(ListenerLoop);
            thread.IsBackground = true;
            thread.Start();
        }

        private void ListenerLoop()
        {
            //todo: add stop flag
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();

                //todo: use thread pool
                Thread thread = new Thread(HandleClient);
                thread.IsBackground = true;
                thread.Start(client);
            }
        }

        //todo: handle exceptions
        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient) obj;

            using (NetworkStream stream = client.GetStream())
            {
                HttpRequest request = ReadRequest(stream);
                if (IsWebSocketRequest(request))
                {
                    HandleWebSocketRequest(stream, request);
                }
                else
                {
                    HandleHttpRequest(stream, request);
                }
            }

            client.Close();
        }

        private void HandleHttpRequest(NetworkStream stream, HttpRequest request)
        {
            HttpResponse response = ProcessRequest(request);
            WriteResponse(stream, response);
        }

        private void HandleWebSocketRequest(NetworkStream stream, HttpRequest request)
        {
            //todo: ProcessRequest and ProcessWebSocketRequest have different semantics
            ProcessWebSocketRequest(stream, request);

            SendWebSocketMessage(stream, "Hello.");

            Thread thread = new Thread(() =>
                                       {
                                           Thread.Sleep(10000);
                                           SendWebSocketMessage(stream, "Goodbye.");
                                       });
            thread.Start();

            //todo: stop gracefully
            while (true)
            {
                WebSocketMessage message = ReadWebSocketMessage(stream);

                if (message.OpCode == WebSocketOpcodes.ConnectionCloseFrame)
                {
                    //todo: send close message
                    return;
                }
                if (message.OpCode == WebSocketOpcodes.PingFrame)
                {
                    //todo: send pong
                }
                if (message.OpCode == WebSocketOpcodes.PongFrame)
                {
                    //todo: check pong
                }
                if (message.OpCode == WebSocketOpcodes.BinaryFrame || message.OpCode == WebSocketOpcodes.TextFrame)
                {
                    //todo: process message
                }
                //todo: check what RFC recommends is such situation
                throw new HcduException(string.Format("Unecpected Opcode: {0}.", message.OpCode));
            }
        }

        //todo: move to separate file
        public class WebSocketOpcodes
        {
            public const byte ContinuationFrame = 0;
            public const byte TextFrame = 1;
            public const byte BinaryFrame = 2;
            public const byte ConnectionCloseFrame = 8;
            public const byte PingFrame = 9;
            public const byte PongFrame = 10;

            public static bool IsControlFrame(byte opcode)
            {
                return opcode >= 8;
            }
        }

        public class WebSocketMessage
        {
            public byte OpCode { get; set; }
            public byte[] Content { get; set; }
        }

        public class WebSocketFrame
        {
            public WebSocketFrameHeader Header { get; set; }
            public byte[] Content { get; set; }
        }

        public class WebSocketFrameHeader
        {
            public bool IsLast { get; set; }
            public byte OpCode { get; set; }
            public ulong PayloadLength { get; set; }
            public byte[] Mask { get; set; }
        }

        private WebSocketMessage ReadWebSocketMessage(NetworkStream stream)
        {
            List<WebSocketFrame> frames = new List<WebSocketFrame>();
            WebSocketFrame firstFrame = null;
            WebSocketFrame lastFrame = null;
            ulong messageLength = 0;

            while (lastFrame == null || !lastFrame.Header.IsLast)
            {
                lastFrame = ReadWebSocketFrame(stream);
                if (firstFrame == null)
                {
                    firstFrame = lastFrame;
                    if (firstFrame.Header.OpCode == WebSocketOpcodes.ContinuationFrame)
                    {
                        //todo: check what RFC recommends is such situation
                        throw new HcduException(string.Format("Invalid first frame opcode: {0}.", firstFrame.Header.OpCode));
                    }
                }

                if (WebSocketOpcodes.IsControlFrame(lastFrame.Header.OpCode))
                {
                    //todo: return separetely
                }

                frames.Add(lastFrame);
                messageLength += lastFrame.Header.PayloadLength;
                if (messageLength > int.MaxValue)
                {
                    throw new HcduException("Long messages are not supported.");
                }
            }

            byte[] messageContent = new byte[messageLength];
            int ofs = 0;

            foreach (WebSocketFrame frame in frames)
            {
                Array.Copy(frame.Content, 0, messageContent, ofs, frame.Content.Length);
                ofs += frame.Content.Length;
            }

            WebSocketMessage message = new WebSocketMessage();

            message.OpCode = firstFrame.Header.OpCode;
            message.Content = messageContent;

            return message;
        }

        private WebSocketFrame ReadWebSocketFrame(NetworkStream stream)
        {
            WebSocketFrameHeader frameHeader = ReadWebSocketFrameHeader(stream);
            if (frameHeader.PayloadLength > int.MaxValue)
            {
                throw new HcduException("Long messages are not supported.");
            }
            if (frameHeader.Mask == null)
            {
                throw new HcduException("Client messages should be masked.");
            }

            byte[] content = ReadBlock(stream, (int) frameHeader.PayloadLength);

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

        private WebSocketFrameHeader ReadWebSocketFrameHeader(NetworkStream stream)
        {
            byte[] primaryHeader = ReadBlock(stream, 2);

            WebSocketFrameHeader header = new WebSocketFrameHeader();
            header.IsLast = (primaryHeader[0] & 0x80) != 0;
            header.OpCode = (byte) (primaryHeader[0] & 0x0F);
            header.PayloadLength = (byte) (primaryHeader[1] & 0x7F);
            if (header.PayloadLength == 126)
            {
                byte[] extLengthHeader = ReadBlock(stream, 2);
                header.PayloadLength = BitConverter.ToUInt16(extLengthHeader, 0);
            }
            if (header.PayloadLength == 127)
            {
                byte[] extLengthHeader = ReadBlock(stream, 8);
                header.PayloadLength = BitConverter.ToUInt64(extLengthHeader, 0);
            }
            if ((primaryHeader[0] & 0x80) != 0)
            {
                header.Mask = ReadBlock(stream, 4);
            }

            return header;
        }

        private byte[] ReadBlock(NetworkStream stream, int blockLength)
        {
            byte[] buffer = new byte[blockLength];
            int ofs = 0;
            while (ofs < blockLength)
            {
                ofs += stream.Read(buffer, ofs, blockLength - ofs);
            }
            return buffer;
        }

        //WebSocket handshake is described here: https://tools.ietf.org/html/rfc6455#section-4.2.2
        private void ProcessWebSocketRequest(NetworkStream stream, HttpRequest request)
        {
            HttpHeader clientKeyHeader = request.Headers.FirstOrDefault(h => h.Name == HttpHeader.SecWebsocketKey);
            if (clientKeyHeader == null || string.IsNullOrWhiteSpace(clientKeyHeader.Value))
            {
                //todo: send error response instead
                throw new HcduException(string.Format("{0} header is missing.", HttpHeader.SecWebsocketKey));
            }
            string clientKey = clientKeyHeader.Value;
            string serverKey = CreateWebSocketServerKey(clientKey);

            WriteLine(stream, "HTTP/1.1 101 Switching Protocols");
            WriteHeader(stream, HttpHeader.Upgrade, HttpHeader.UpgradeWebsocket);
            WriteHeader(stream, HttpHeader.Connection, HttpHeader.ConnectionUpgrade);
            WriteHeader(stream, HttpHeader.SecWebSocketAccept, serverKey);
            WriteLine(stream, "");
        }

        //WebSocket message is desribed here: https://tools.ietf.org/html/rfc6455#section-5.2
        private void SendWebSocketMessage(NetworkStream stream, string message)
        {
            UTF8Encoding encoding = new UTF8Encoding(false);
            byte[] messageBytes = encoding.GetBytes(message);
            if (messageBytes.Length > 125)
            {
                throw new HcduException("Long messages are not supported.");
            }
            byte[] frameHeader = new byte[2];
            frameHeader[0] = 0x81;
            frameHeader[1] = (byte) messageBytes.Length;
            stream.Write(frameHeader, 0, frameHeader.Length);
            stream.Write(messageBytes, 0, messageBytes.Length);
            //todo: flush ?
        }

        private string CreateWebSocketServerKey(string clientKey)
        {
            string value = clientKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            SHA1 sha1 = SHA1.Create();
            byte[] valueSha1 = sha1.ComputeHash(Encoding.ASCII.GetBytes(value));
            return Convert.ToBase64String(valueSha1);
        }

        private bool IsWebSocketRequest(HttpRequest request)
        {
            return
                request.Headers.Any(h => h.Name == HttpHeader.Connection && h.Value == HttpHeader.ConnectionUpgrade) &&
                request.Headers.Any(h => h.Name == HttpHeader.Upgrade && h.Value == HttpHeader.UpgradeWebsocket);
        }

        private HttpResponse ProcessRequest(HttpRequest request)
        {
            //todo: can URI start with protocol?
            string contentLocation = request.Uri.TrimStart('/');
            IContentProvider contentProvider = contentPackage.GetContentProvider(contentLocation);

            if (contentProvider == null)
            {
                return HttpResponse.NotFound();
            }

            return contentProvider.GetContent();
        }

        //HTTP message is desribed here: https://tools.ietf.org/html/rfc2616#section-4
        private HttpRequest ReadRequest(NetworkStream stream)
        {
            //todo: use BufferedStream ?

            HttpRequest request = new HttpRequest();

            string requestLine = ReadLine(stream);
            while (requestLine == "")
            {
                requestLine = ReadLine(stream);
            }
            ParseRequestLine(request, requestLine);

            string headerLine;
            HttpHeader lastHeader = null;
            while (!string.IsNullOrEmpty(headerLine = ReadLine(stream)))
            {
                if (IsSpOrHt(headerLine[0]))
                {
                    if (lastHeader == null)
                    {
                        throw new HcduException(string.Format("Invalid HTTP-message header: '{0}'.", headerLine));
                    }
                    lastHeader.Value += ' ' + headerLine.TrimStart(' ', '\t');
                }
                else
                {
                    lastHeader = ParserHeader(headerLine);
                    request.Headers.Add(lastHeader);
                }
            }

            HttpHeader contentLengthHeader = request.Headers.FirstOrDefault(h => h.Name == HttpHeader.ContentLength);
            if (contentLengthHeader != null)
            {
                int contentLength;
                if (!int.TryParse(contentLengthHeader.Value, out contentLength) || contentLength < 0)
                {
                    throw new HcduException(string.Format("Invalid Content-Length header value: '{0}'.", contentLengthHeader.Value));
                }
                request.Body = ReadMessageBody(stream, contentLength);
            }
            return request;
        }

        private void WriteResponse(NetworkStream stream, HttpResponse response)
        {
            //todo: use BufferedStream ?

            WriteLine(stream, string.Format("HTTP/1.0 {0} {1}", response.StatusCode, response.StatusText));
            WriteHeader(stream, HttpHeader.Connection, HttpHeader.ConnectionClose);

            if (response.Content != null)
            {
                int contentLength = response.Content.Length;
                WriteHeader(stream, HttpHeader.ContentType, response.MimeType);
                WriteHeader(stream, HttpHeader.ContentLength, contentLength.ToString());
                WriteLine(stream, "");
                stream.Write(response.Content, 0, contentLength);
            }
            else
            {
                WriteLine(stream, "");
            }

            //todo: flush ?
        }

        private void WriteHeader(NetworkStream stream, string headerName, string headerValue)
        {
            WriteLine(stream, string.Format("{0}: {1}", headerName, headerValue));
        }

        private void WriteLine(NetworkStream stream, string line)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(line + "\r\n");
            stream.Write(buffer, 0, buffer.Length);
        }

        private void ParseRequestLine(HttpRequest request, string requestLine)
        {
            string[] parts = requestLine.Split(' ');
            if (parts.Length != 3)
            {
                throw new HcduException(string.Format("Invalid Request-Line: '{0}'.", requestLine));
            }
            request.Method = parts[0];
            request.Uri = parts[1];
            request.HttpVersion = parts[2];
        }

        private HttpHeader ParserHeader(string headerLine)
        {
            int pos = headerLine.IndexOf(':');
            if (pos < 0)
            {
                throw new HcduException(string.Format("Invalid HTTP-message header: '{0}'.", headerLine));
            }

            HttpHeader header = new HttpHeader();
            header.Name = headerLine.Substring(0, pos);
            pos++;
            while (pos < headerLine.Length && IsSpOrHt(headerLine[pos]))
            {
                pos++;
            }
            header.Value = headerLine.Substring(pos);
            return header;
        }

        private string ReadLine(NetworkStream stream)
        {
            //todo: remove debug code
            bool delayedRequest = false;
            StringBuilder sb = new StringBuilder();
            int c;
            while ((c = stream.ReadByte()) != '\n')
            {
                if (c == -1)
                {
                    //todo: research why chrome opens such connections
                    delayedRequest = true;
                    Thread.Sleep(100);
                    continue;
                    //throw new HcduException("Unexpected end of line in HTTP-message.");
                }
                if (c != '\r')
                {
                    sb.Append((char) c);
                }
            }
            if (delayedRequest)
            {
                Console.WriteLine(">>>{0}", sb);
            }
            return sb.ToString();
        }

        private byte[] ReadMessageBody(NetworkStream stream, int contentLength)
        {
            byte[] body = new byte[contentLength];

            int ofs = 0;

            while (ofs < contentLength)
            {
                ofs += stream.Read(body, ofs, contentLength - ofs);
            }

            return body;
        }

        private static bool IsSpOrHt(char c)
        {
            return c == ' ' || c == '\t';
        }
    }
}