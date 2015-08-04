using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                using (NetworkStream stream = client.GetStream())
                {
                    HttpRequest request = ReadRequest(stream);
                    HttpResponse response = ProcessRequest(request);
                    WriteResponse(stream, response);
                }
                client.Close();
            }
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

        //HTTP message is desribed here: http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4
        private HttpRequest ReadRequest(NetworkStream stream)
        {
            //todo: use BufferedStream ?

            HttpRequest request = new HttpRequest();

            string requestLine = ReadLine(stream);
            while(requestLine == "")
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
            if(contentLengthHeader != null)
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
                    sb.Append((char)c);
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
