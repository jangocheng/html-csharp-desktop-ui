using HCDU.Web.Api;

namespace HCDU.API
{
    public class WebSocketHandler : IWebSocketHandler
    {
        private readonly ISocketProvider socketProvider;
        private readonly ISocket socket;

        public WebSocketHandler(ISocketProvider socketProvider, ISocket socket)
        {
            this.socketProvider = socketProvider;
            this.socket = socket;
        }

        public void OnConnent()
        {
            socket.OnCreate(socketProvider);
        }

        public void OnDisconnect()
        {
            socket.OnDestroy(socketProvider);
        }

        public void ProcessMessage(string message)
        {
            socket.ProcessMessage(message);
        }

        public void ProcessMessage(byte[] message)
        {
            socket.ProcessMessage(message);
        }
    }
}