namespace HCDU.API.Server
{
    public interface IWebSocketHandler
    {
        void OnConnent();

        void OnDisconnect();
        
        void ProcessMessage(string message);

        void ProcessMessage(byte[] message);
    }
}