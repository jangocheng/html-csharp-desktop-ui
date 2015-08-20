namespace HCDU.Web.Api
{
    public interface IWebSocketHandler
    {
        void OnConnent();

        void OnDisconnect();
        
        void ProcessMessage(string message);

        void ProcessMessage(byte[] message);
    }
}