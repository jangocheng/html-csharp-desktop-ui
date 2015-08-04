using System.IO;
using System.Reflection;
using CefSharp;
using HCDU.API;
using HCDU.API.Http;

namespace HCDU.Windows
{
    public class ContentPackageResourceHandlerFactory : IResourceHandlerFactory
    {
        private readonly string baseUrl;
        private readonly ContentPackage contentPackage;
        private readonly MethodInfo statusCodeSetter;
        private readonly MethodInfo statusTextSetter;

        public ContentPackageResourceHandlerFactory(string baseUrl, ContentPackage contentPackage)
        {
            this.baseUrl = baseUrl;
            this.contentPackage = contentPackage;

            //todo: create patch for CefSharp instead?
            statusCodeSetter = typeof (ResourceHandler).GetProperty("StatusCode").GetSetMethod(true);
            statusTextSetter = typeof (ResourceHandler).GetProperty("StatusText").GetSetMethod(true);
        }

        public void RegisterHandler(string url, ResourceHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public void UnregisterHandler(string url)
        {
            throw new System.NotImplementedException();
        }

        public ResourceHandler GetResourceHandler(IWebBrowser browser, IRequest request)
        {
            if (request.Url == null || !request.Url.StartsWith(baseUrl))
            {
                return null;
            }

            string contentLocation = request.Url.Substring(baseUrl.Length).TrimStart('/');

            IContentProvider contentProvider = contentPackage.GetContentProvider(contentLocation);
            if (contentProvider == null)
            {
                return null;
            }

            HttpResponse response = contentProvider.GetContent();
            MemoryStream mem = new MemoryStream(response.Content);

            ResourceHandler resourceHandler = ResourceHandler.FromStream(mem, response.MimeType);
            SetStatus(resourceHandler, response.StatusCode, response.StatusText);
            return resourceHandler;
        }

        public bool HasHandlers
        {
            get { return true; }
        }

        private void SetStatus(ResourceHandler resourceHandler, int statusCode, string statusText)
        {
            statusCodeSetter.Invoke(resourceHandler, new object[] {statusCode});
            statusTextSetter.Invoke(resourceHandler, new object[] {statusText});
        }
    }
}