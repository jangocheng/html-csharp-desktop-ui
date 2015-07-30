using System.IO;
using CefSharp;
using HCDU.API;

namespace HCDU.Windows
{
    public class ContentPackageResourceHandlerFactory : IResourceHandlerFactory
    {
        private readonly string baseUrl;
        private readonly ContentPackage contentPackage;

        public ContentPackageResourceHandlerFactory(string baseUrl, ContentPackage contentPackage)
        {
            this.baseUrl = baseUrl;
            this.contentPackage = contentPackage;
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
            if (contentLocation == "")
            {
                contentLocation = "index.html";
            }

            IContentProvider contentProvider = contentPackage.GetContentProvider(contentLocation);
            if (contentProvider == null)
            {
                return null;
            }

            HttpResponse response = contentProvider.GetContent();
            MemoryStream mem = new MemoryStream(response.Content);
            return ResourceHandler.FromStream(mem, response.MimeType);
        }

        public bool HasHandlers
        {
            get { return true; }
        }
    }
}