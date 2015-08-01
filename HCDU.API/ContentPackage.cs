using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HCDU.API
{
    public class ContentPackage
    {
        private readonly Dictionary<string, IContentProvider> contentProviders = new Dictionary<string, IContentProvider>();

        public IEnumerable<string> Resources
        {
            get { return contentProviders.Keys.ToList(); }
        }

        public void AddMethod<TResult>(string contentLocation, Func<TResult> method)
        {
            AddContentProvider(contentLocation, new MethodContentProvider<TResult>(method));
        }

        public void AddContent(Assembly assembly, string resourcePrefix)
        {
            string[] manifestResourceNames = assembly.GetManifestResourceNames();
            resourcePrefix += ".";
            foreach (string resourceName in manifestResourceNames)
            {
                if (resourceName.StartsWith(resourcePrefix))
                {
                    string contentLocation = ConvertResourceNameToLocation(resourceName.Substring(resourcePrefix.Length));
                    IContentProvider contentProvider = new ResourceContentProvider(assembly, resourceName);
                    AddContentProvider(contentLocation, contentProvider);
                }
            }
        }

        //todo: review this approach
        private string ConvertResourceNameToLocation(string resourceName)
        {
            string[] locationParts = resourceName.Split('.');
            if (locationParts.Length <= 2)
            {
                return resourceName;
            }

            int fileNameLength = 2;
            if (locationParts.Length >= 3 && locationParts[locationParts.Length - 2] == "min")
            {
                fileNameLength = 3;
            }

            string path = string.Join("/", locationParts.Take(locationParts.Length - fileNameLength));
            string filename = string.Join(".", locationParts.Skip(locationParts.Length - fileNameLength));
            return path + "/" + filename;
        }

        public void AddContentProvider(string contentLocation, IContentProvider contentProvider)
        {
            if (contentProviders.ContainsKey(contentLocation))
            {
                throw new HcduException(string.Format("Threre are multiple content providers for location: {0}.", contentLocation));
            }
            contentProviders.Add(contentLocation, contentProvider);
        }

        public IContentProvider GetContentProvider(string contentLocation)
        {
            IContentProvider provider;
            contentProviders.TryGetValue(contentLocation, out provider);
            return provider;
        }
    }
}