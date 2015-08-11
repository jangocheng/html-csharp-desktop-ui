namespace HCDU.API
{
    public class Platform
    {
        private static IPlatformAdapter adapter;

        public static void SetAdapter(IPlatformAdapter platformAdapter)
        {
            adapter = platformAdapter;
        }

        public static string OpenFolderBrowserDialog(bool allowCreateFolder)
        {
            return adapter.OpenFolderBrowserDialog(allowCreateFolder);
        }

        public static void ShowDialog(string url)
        {
            adapter.ShowDialog(url);
        }
    }
}