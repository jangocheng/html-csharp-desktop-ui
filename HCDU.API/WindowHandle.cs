namespace HCDU.API
{
    public class WindowHandle
    {
        private readonly object nativeWindow;
        private readonly object nativeBrowser;

        public WindowHandle(object nativeWindow, object nativeBrowser)
        {
            this.nativeWindow = nativeWindow;
            this.nativeBrowser = nativeBrowser;
        }

        public object NativeWindow
        {
            get { return nativeWindow; }
        }

        //todo: is this field useful ?
        public object NativeBrowser
        {
            get { return nativeBrowser; }
        }
    }
}