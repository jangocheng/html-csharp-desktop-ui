namespace HCDU.API
{
    public interface IPlatformAdapter
    {
        WindowHandle CreateWindow(WindowPrototype prototype);
        //todo: ShowDialog should use WindowHandle instead. Or it should be used as ShowWindow.
        WindowHandle ShowDialog(WindowHandle parent, WindowPrototype prototype);
        void CloseDialog(WindowHandle win);
        string OpenFolderBrowserDialog(WindowHandle parent, bool allowCreateFolder);
    }
}