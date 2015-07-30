using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HCDU.API
{
    public class ContentPackage
    {
        private readonly Dictionary<string, IContentProvider> contentProviders = new Dictionary<string, IContentProvider>();

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
            string path = string.Join("/", locationParts.Take(locationParts.Length - 2));
            string filename = locationParts[locationParts.Length - 2] + "." + locationParts[locationParts.Length - 1];
            return path + "/" + filename;
        }

        private void AddContentProvider(string contentLocation, IContentProvider contentProvider)
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