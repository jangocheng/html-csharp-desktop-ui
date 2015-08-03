namespace HCDU.API
{
    public class HttpResponse
    {
        public int StatusCode { get; private set; }
        public string StatusText { get; private set; }
        public string MimeType { get; private set; }
        public byte[] Content { get; private set; }

        private HttpResponse(int statusCode, string statusText, string mimeType, byte[] content)
        {
            StatusCode = statusCode;
            StatusText = statusText;
            MimeType = mimeType;
            Content = content;
        }

        public static HttpResponse Ok(string mimeType, byte[] content)
        {
            return new HttpResponse(200, "OK", mimeType, content);
        }

        public static HttpResponse InternalServerError(string mimeType, byte[] content)
        {
            return new HttpResponse(500, "Internal Server Error", mimeType, content);
        }
    }
}