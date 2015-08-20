namespace HCDU.Web.Api
{
    public class HttpHeader
    {
        public const string Connection = "Connection";
        public const string ConnectionClose = "close";
        public const string ConnectionUpgrade = "Upgrade";

        public const string ContentType = "Content-Type";
        public const string ContentLength = "Content-Length";

        public const string Upgrade = "Upgrade";
        public const string UpgradeWebsocket = "websocket";

        public const string SecWebsocketKey = "Sec-WebSocket-Key";
        public const string SecWebSocketAccept = "Sec-WebSocket-Accept";

        public string Name { get; set; }
        public string Value { get; set; }
    }
}