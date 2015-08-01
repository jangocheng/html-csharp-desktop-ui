using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HCDU.API
{
    public static class DebugPages
    {
        public static void AppendTo(ContentPackage contentPackage)
        {
            contentPackage.AddContentProvider("debug/resources.html", new ContentPackageResourceListContentProvider(contentPackage));
        }
    }

    public class ContentPackageResourceListContentProvider : IContentProvider
    {
        private readonly ContentPackage contentPackage;

        public ContentPackageResourceListContentProvider(ContentPackage contentPackage)
        {
            this.contentPackage = contentPackage;
        }

        public bool IsStatic
        {
            get { return true; }
        }

        public HttpResponse GetContent()
        {
            string content = LoadResourceAsString("page.template.html");
            content = content.Replace("%PAGE_TITLE%", "Resource List");

            StringBuilder sb = new StringBuilder();
            sb.Append("<ul>\n");

            List<string> resourceNames = contentPackage.Resources.OrderBy(r => r).ToList();
            foreach (string resourceName in resourceNames)
            {
                //todo: use some utility to join URL parts
                string resourceUrl = "../" + resourceName;
                sb.AppendFormat("<li><a href='{0}'>{1}</a></li>\n", resourceUrl, resourceName);
            }

            sb.Append("</ul>\n");

            content = content.Replace("%PAGE_BODY%", sb.ToString());

            HttpResponse response = new HttpResponse();
            response.MimeType = MimeTypes.Html;
            response.Content = Encoding.UTF8.GetBytes(content);
            return response;
        }

        private string LoadResourceAsString(string resourceName)
        {
            Assembly assembly = typeof (ContentPackageResourceListContentProvider).Assembly;
            resourceName = "HCDU.API." + resourceName;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new HcduException(string.Format("Failed to get embedded resource (assembly: '{0}', resource name: '{1}').", assembly.FullName, resourceName));
                }
                using (MemoryStream mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
                    return Encoding.UTF8.GetString(mem.ToArray());
                }
            }
        }
    }
}
