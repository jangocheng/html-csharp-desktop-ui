namespace HCDU.API.Server
{
    //todo: change to struct?
    public class WebSocketFrameHeader
    {
        public bool IsLast { get; set; }
        public byte OpCode { get; set; }
        public ulong PayloadLength { get; set; }
        public byte[] Mask { get; set; }
    }
}