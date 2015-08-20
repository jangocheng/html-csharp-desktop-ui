using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using HCDU.Web.Api;

namespace HCDU.API.Server
{
    public class WebServer
    {
        private readonly IWebRequestHandler requestHandler;
        private readonly int port;
        private readonly TcpListener tcpListener;

        public WebServer(IWebRequestHandler requestHandler, int port)
        {
            this.requestHandler = requestHandler;
            this.port = port;
            tcpListener = new TcpListener(IPAddress.Loopback, port);
        }

        public int Port
        {
            get { return port; }
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
                //todo: HTTP and WebSocket branches have different semantics
                //todo: handle IOException
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

        //todo: consider joining HandleWebSocketRequest and ProcessRequest to one method
        private void HandleWebSocketRequest(NetworkStream stream, HttpRequest request)
        {
            //todo: can URI start with protocol?
            if (!requestHandler.CanCreateWebSocketHandler(request))
            {
                HttpResponse response = HttpResponse.NotFound();
                WriteResponse(stream, response);
                return;
            }

            ProcessWebSocketHandshake(stream, request);

            WebSocket webSocket = new WebSocket(stream);
            IWebSocketHandler handler = requestHandler.CreateWebSocketHandler(request, webSocket);
            webSocket.Listen(handler);
        }

        private void HandleHttpRequest(NetworkStream stream, HttpRequest request)
        {
            //todo: handle HEAD method
            //todo: handle internal server error
            HttpResponse response = requestHandler.ProcessHttpRequest(request);
            try
            {
                WriteResponse(stream, response);
            }
            catch (IOException)
            {
                //todo: log or just ignore?
            }
        }

        private bool IsWebSocketRequest(HttpRequest request)
        {
            return
                request.Headers.Any(h => h.Name == HttpHeader.Connection && h.Value == HttpHeader.ConnectionUpgrade) &&
                request.Headers.Any(h => h.Name == HttpHeader.Upgrade && h.Value == HttpHeader.UpgradeWebsocket);
        }

        //WebSocket handshake is described here: https://tools.ietf.org/html/rfc6455#section-4.2.2
        private void ProcessWebSocketHandshake(NetworkStream stream, HttpRequest request)
        {
            HttpHeader clientKeyHeader = request.Headers.FirstOrDefault(h => h.Name == HttpHeader.SecWebsocketKey);
            if (clientKeyHeader == null || string.IsNullOrWhiteSpace(clientKeyHeader.Value))
            {
                //todo: send error response instead
                throw new HcduException(string.Format("{0} header is missing.", HttpHeader.SecWebsocketKey));
            }
            string clientKey = clientKeyHeader.Value;
            string serverKey = CreateWebSocketServerKey(clientKey);

            HttpUtils.WriteLine(stream, "HTTP/1.1 101 Switching Protocols");
            HttpUtils.WriteHeader(stream, HttpHeader.Upgrade, HttpHeader.UpgradeWebsocket);
            HttpUtils.WriteHeader(stream, HttpHeader.Connection, HttpHeader.ConnectionUpgrade);
            HttpUtils.WriteHeader(stream, HttpHeader.SecWebSocketAccept, serverKey);
            HttpUtils.WriteLine(stream, "");
        }

        private string CreateWebSocketServerKey(string clientKey)
        {
            string value = clientKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            SHA1 sha1 = SHA1.Create();
            byte[] valueSha1 = sha1.ComputeHash(Encoding.ASCII.GetBytes(value));
            return Convert.ToBase64String(valueSha1);
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

            if (IsWebSocketRequest(request) && request.Method != HttpMethod.Get)
            {
                throw new HcduException(string.Format("WebSocket request has invalid method: '{0}'.", request.Method));
            }

            HttpHeader contentLengthHeader = request.Headers.FirstOrDefault(h => h.Name == HttpHeader.ContentLength);
            if (contentLengthHeader != null)
            {
                //todo: restrict content upload for other methods that does not allow it

                int contentLength;
                if (!int.TryParse(contentLengthHeader.Value, out contentLength) || contentLength < 0)
                {
                    throw new HcduException(string.Format("Invalid Content-Length header value: '{0}'.", contentLengthHeader.Value));
                }
                request.Body = HttpUtils.ReadBlock(stream, contentLength);
            }
            return request;
        }

        private void WriteResponse(NetworkStream stream, HttpResponse response)
        {
            //todo: use BufferedStream ?

            HttpUtils.WriteLine(stream, string.Format("HTTP/1.0 {0} {1}", response.StatusCode, response.StatusText));
            HttpUtils.WriteHeader(stream, HttpHeader.Connection, HttpHeader.ConnectionClose);

            if (response.Content != null)
            {
                int contentLength = response.Content.Length;
                HttpUtils.WriteHeader(stream, HttpHeader.ContentType, response.MimeType);
                HttpUtils.WriteHeader(stream, HttpHeader.ContentLength, contentLength.ToString());
                HttpUtils.WriteLine(stream, "");
                stream.Write(response.Content, 0, contentLength);
            }
            else
            {
                HttpUtils.WriteLine(stream, "");
            }

            //todo: flush ?
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

        private static bool IsSpOrHt(char c)
        {
            return c == ' ' || c == '\t';
        }
    }
}