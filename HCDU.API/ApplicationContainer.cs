using System;
using System.Collections.Generic;
using System.Linq;
using HCDU.API.Server;

namespace HCDU.API
{
    //todo: review thread-safety
    public class ApplicationContainer
    {
        private WebServer webServer;
        private WindowHandle mainWindow;
        private readonly List<WindowHandle> windows = new List<WindowHandle>();

        public WebServer WebServer
        {
            get { return webServer; }
        }

        public ApplicationPackage ApplicationPackage { get; set; }

        public IPlatformAdapter Platform { get; set; }

        public void Start()
        {
            ApplicationPackage.Container = this;
            webServer = new WebServer(ApplicationPackage.Content, ApplicationPackage.Sockets, 8899);
            webServer.Start();
            ApplicationPackage.OnStart();
        }

        //todo: remove this temporary method
        public void Start(object mainWindow, object webBrowser)
        {
            this.mainWindow = new WindowHandle(mainWindow, webBrowser);
            windows.Add(this.mainWindow);

            Start();
        }

        public object CreateMainWindow()
        {
            //todo: move CreateMainWindow logic to Start method and use simple getter instead?
            if (mainWindow != null)
            {
                throw new HcduException("Main windows was created already.");
            }
            WindowPrototype prot = (WindowPrototype) ApplicationPackage.MainWindowPrototype.Clone();
            //todo: this is an ugly approach
            prot.Url = MakeAbsoluteUrl(prot.Url);
            mainWindow = Platform.CreateWindow(prot);
            windows.Add(mainWindow);
            return mainWindow.NativeWindow;
        }

        public void ShowDialog(WindowPrototype prot)
        {
            //todo: this is an ugly approach
            prot = (WindowPrototype) prot.Clone();
            prot.Url = MakeAbsoluteUrl(prot.Url);
            
            Action<WindowHandle> oldOnCloseHandler = prot.OnClose;
            prot.OnClose = wh =>
                           {
                               OnWindowClose(wh);
                               if (oldOnCloseHandler != null)
                               {
                                   oldOnCloseHandler(wh);
                               }
                           };

            //todo: should we always use mainWindow as a parent
            WindowHandle handle = Platform.ShowDialog(mainWindow, prot);
            windows.Add(handle);
        }

        //todo: rename to CloseWindow and use WindowHandle or WindowId as parameter
        public void CloseDialog()
        {
            Platform.CloseDialog(windows.Last());
        }

        public string OpenFolderBrowserDialog(bool allowCreateFolder)
        {
            return Platform.OpenFolderBrowserDialog(mainWindow, allowCreateFolder);
        }

        private void OnWindowClose(WindowHandle win)
        {
            //todo: use some other type for window collection ?
            windows.Remove(win);
        }

        private string MakeAbsoluteUrl(string url)
        {
            //todo: use some utils to construct url
            return "http://localhost:" + webServer.Port + "/" + url;
        }
    }
}