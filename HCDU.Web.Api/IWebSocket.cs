namespace HCDU.Web.Api
{
    public interface IWebSocket
    {
        void SendMessage(string message);
    }
}