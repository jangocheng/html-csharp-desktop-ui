using System.Collections.Generic;
using System.IO;

namespace HCDU.Web.Api
{
    public static class MimeTypes
    {
        public const string Html = "text/html";
        public const string Css = "text/css";
        public const string JavaScript = "application/javascript";
        public const string Json = "application/json";
        public const string PlainText = "text/plain";
        
        public const string Data = "application/octet-stream";

        private static readonly Dictionary<string, string> extensionsMap = new Dictionary<string, string>();

        static MimeTypes()
        {
            extensionsMap.Add(".css", Css);
            extensionsMap.Add(".html", Html);
            extensionsMap.Add(".js", JavaScript);
            extensionsMap.Add(".json", Json);
            extensionsMap.Add(".txt", PlainText);
        }

        public static string GetMimeType(string filename)
        {
            string ext = Path.GetExtension(filename);

            if (string.IsNullOrEmpty(ext))
            {
                return Data;
            }

            ext = ext.ToLower();

            string mimeType;
            if (!extensionsMap.TryGetValue(ext, out mimeType))
            {
                return Data;
            }

            return mimeType;
        }
    }
}