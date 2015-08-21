namespace HCDU.Web.Server
{
    public class WebSocketFrame
    {
        public WebSocketFrameHeader Header { get; set; }
        public byte[] Content { get; set; }
    }
}