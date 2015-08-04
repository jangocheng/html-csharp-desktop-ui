using System.Collections.Generic;

namespace HCDU.API.Http
{
    public class HttpRequest
    {
        public HttpRequest()
        {
            Headers = new List<HttpHeader>();
        }

        public string Method { get; set; }
        public string Uri { get; set; }
        public string HttpVersion { get; set; }
        public List<HttpHeader> Headers { get; private set; }
        public byte[] Body { get; set; }
    }
}