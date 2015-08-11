namespace HCDU.API
{
    public interface IPlatformAdapter
    {
        string OpenFolderBrowserDialog(bool allowCreateFolder);
        void ShowDialog(string url);
    }
}