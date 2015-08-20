namespace HCDU.Web.Api
{
    public interface IWebRequestHandler
    {
        HttpResponse ProcessHttpRequest(HttpRequest request);
        
        bool CanCreateWebSocketHandler(HttpRequest request);

        IWebSocketHandler CreateWebSocketHandler(HttpRequest request, IWebSocket webSocket);
    }
}