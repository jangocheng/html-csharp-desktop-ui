using System.Collections.Generic;
using HCDU.API;
using Gtk;
using System;
using System.Threading;
using WebKit;

namespace HCDU.Linux.Gtk
{
    public class GtkPlatformAdapter : IPlatformAdapter
    {
        public string OpenFolderBrowserDialog(WindowHandle parent, bool allowCreateFolder)
        {
            return InvokeSync(() => OpenFolderBrowserDialogHandler((Window) parent.NativeWindow, allowCreateFolder));
        }

        public WindowHandle CreateWindow(WindowPrototype prototype)
        {
            throw new NotImplementedException();
        }

        public WindowHandle ShowDialog(WindowHandle parent, WindowPrototype prototype)
        {
            return InvokeSync(() => ShowDialogHandler((Window) parent.NativeWindow, prototype));
        }

        public void CloseDialog(WindowHandle win)
        {
            //todo: implement
            throw new NotImplementedException();
        }

        //todo: review this approach, it does not look safe
        private TResult InvokeSync<TResult>(Func<TResult> func)
        {
            TResult result = default(TResult);
            ManualResetEvent ev = new ManualResetEvent(false);
            Application.Invoke(delegate
                               {
                                   try
                                   {
                                       result = func();
                                   }
                                   finally
                                   {
                                       ev.Set();
                                   }
                               });
            ev.WaitOne();
            return result;
        }

        private WindowHandle ShowDialogHandler(Window parent, WindowPrototype prototype)
        {
            BrowserWindow win = new BrowserWindow(prototype.Url);

            win.TransientFor = parent;
            win.Modal = true;
            win.SkipTaskbarHint = true;
            win.TypeHint = Gdk.WindowTypeHint.Dialog;
            //todo: bug, window cannot be resized to smaller size (seems related to https://bugs.webkit.org/show_bug.cgi?id=17154)
            win.Resize(1024, 640);
            win.ShowAll();

            return new WindowHandle(win, win.WebBrowser);
        }

        private string OpenFolderBrowserDialogHandler(Window parent, bool allowCreateFolder)
        {
            FileChooserAction action = allowCreateFolder ? FileChooserAction.CreateFolder : FileChooserAction.SelectFolder;

            FileChooserDialog dlg = new FileChooserDialog(
                "Select a folder",
                parent,
                action,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept
                );

            string result = null;

            if (dlg.Run() == (int) ResponseType.Accept)
            {
                result = dlg.Filename;
            }

            dlg.Destroy();

            return result;
        }
    }

    public class BrowserWindow : Window
    {
        private WebView webBrowser;

        public WebView WebBrowser
        {
            get { return webBrowser; }
        }

        public BrowserWindow(string url) : base(WindowType.Toplevel)
        {
            InitBrowser(url);
        }

        private void InitBrowser(string url)
        {
            webBrowser = new WebView();
            webBrowser.Name = "webBrowser";

            VBox vbox = new VBox();
            vbox.Name = "vbox";
            this.Add(vbox);

            vbox.Add(webBrowser);
            global::Gtk.Box.BoxChild webBrowserBox = ((global::Gtk.Box.BoxChild) (vbox[webBrowser]));
            webBrowserBox.Position = 0;

            webBrowser.LoadUri(url);
        }
    }
}