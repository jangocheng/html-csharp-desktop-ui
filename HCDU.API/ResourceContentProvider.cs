using System.IO;
using System.Reflection;

namespace HCDU.API
{
    public class ResourceContentProvider : IContentProvider
    {
        private readonly Assembly assembly;
        private readonly string resourceName;
        private readonly string mimeType;

        public ResourceContentProvider(Assembly assembly, string resourceName)
        {
            this.assembly = assembly;
            this.resourceName = resourceName;
            this.mimeType = GetMimeType(resourceName);
        }

        private static string GetMimeType(string resourceName)
        {
            resourceName = resourceName.ToLower();
            if (resourceName.EndsWith(".css"))
            {
                return "text/css";
            }
            return "text/html";
        }

        public bool IsStatic
        {
            get { return true; }
        }

        public HttpResponse GetContent()
        {
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new HcduException(string.Format("ResourceContentProvider failed to get embedded resource (assembly: '{0}', resource name: '{1}').", assembly.FullName, resourceName));
                }
                using (MemoryStream mem = new MemoryStream())
                {
                    resourceStream.CopyTo(mem);
                    return new HttpResponse{MimeType = mimeType, Content = mem.ToArray()};
                }
            }
        }
    }
}