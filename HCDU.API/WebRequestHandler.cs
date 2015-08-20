using HCDU.Web.Api;

namespace HCDU.API
{
    public class WebRequestHandler : IWebRequestHandler
    {
        private readonly ContentPackage contentPackage;
        private readonly SocketPackage socketPackage;

        public WebRequestHandler(ContentPackage contentPackage, SocketPackage socketPackage)
        {
            this.contentPackage = contentPackage;
            this.socketPackage = socketPackage;
        }

        public HttpResponse ProcessHttpRequest(HttpRequest request)
        {
            //todo: can URI start with protocol?
            string contentLocation = request.Uri.TrimStart('/');
            IContentProvider contentProvider = contentPackage.GetContentProvider(contentLocation);

            if (contentProvider == null)
            {
                return HttpResponse.NotFound();
            }

            return contentProvider.GetContent();
        }

        public bool CanCreateWebSocketHandler(HttpRequest request)
        {
            ISocketProvider socketProvider = GetSocketProvider(request.Uri);
            return socketProvider != null;
        }

        public IWebSocketHandler CreateWebSocketHandler(HttpRequest request, IWebSocket webSocket)
        {
            ISocketProvider socketProvider = GetSocketProvider(request.Uri);
            ISocket socket = socketProvider.CreateSocket(webSocket);
            return new WebSocketHandler(socketProvider, socket);
        }

        private ISocketProvider GetSocketProvider(string uri)
        {
            string socketLocation = uri.TrimStart('/');
            return socketPackage.GetSocketProvider(socketLocation);
        }
    }
}