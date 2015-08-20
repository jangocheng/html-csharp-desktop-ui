using HCDU.API.Http;

namespace HCDU.API.Server
{
    public interface IWebRequestHandler
    {
        HttpResponse ProcessHttpRequest(HttpRequest request);
        
        bool CanCreateWebSocketHandler(HttpRequest request);

        IWebSocketHandler CreateWebSocketHandler(HttpRequest request, WebSocket webSocket);
    }
}