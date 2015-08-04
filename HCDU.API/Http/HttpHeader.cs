namespace HCDU.API.Http
{
    public class HttpHeader
    {
        public const string Connection = "Connection";
        public const string ConnectionClose = "close";

        public const string ContentType = "Content-Type";
        public const string ContentLength = "Content-Length";

        public string Name { get; set; }
        public string Value { get; set; }
    }
}