namespace HCDU.API.Server
{
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
}