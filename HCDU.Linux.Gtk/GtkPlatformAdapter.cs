using HCDU.API;
using Gtk;
using System;
using System.Threading;
using WebKit;
using System.Collections.Generic;
using System.Linq;

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
			BrowserWindow window = new BrowserWindow (prototype);
			WindowHandle handle = new WindowHandle (window, window.WebBrowser);
			return handle;
        }

        public WindowHandle ShowDialog(WindowHandle parent, WindowPrototype prototype)
        {
            return InvokeSync(() => ShowDialogHandler((Window) parent.NativeWindow, prototype));
        }

        public void CloseDialog(WindowHandle win)
        {
            Window dlg = (Window) win.NativeWindow;
			InvokeSync (() => {
				Gdk.Event ev = Gdk.EventHelper.New(Gdk.EventType.Delete);
				if(!dlg.ProcessEvent (ev))
				{
					dlg.Destroy();
				}
				//todo: this is a fake value
				return true;
			});
        }

        public void NavigateTo(WindowHandle window, string url)
        {
            WebView browser = (WebView) window.NativeBrowser;
            browser.LoadUri(url);
        }

        public void ReloadPage(WindowHandle window)
        {
            WebView browser = (WebView) window.NativeBrowser;
            browser.Reload();
        }

        public void ShowDevTools(WindowHandle window)
        {
            Window parent = (Window) window.NativeWindow;
            using (MessageDialog dlg = new MessageDialog(parent, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "Developoer tools are not supported on this platform."))
            {
                dlg.Run();
                dlg.Destroy();
            }
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
			//todo: use CreateWindow instead
			BrowserWindow win = new BrowserWindow(prototype);

            win.TransientFor = parent;
            win.Modal = true;
            win.SkipTaskbarHint = true;
            win.TypeHint = Gdk.WindowTypeHint.Dialog;

			win.Show();

            return new WindowHandle(win, win.WebBrowser);
        }

        private string OpenFolderBrowserDialogHandler(Window parent, bool allowCreateFolder)
        {
            FileChooserAction action = allowCreateFolder ? FileChooserAction.CreateFolder : FileChooserAction.SelectFolder;

            using (FileChooserDialog dlg = new FileChooserDialog(
                "Select a folder",
                parent,
                action,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept
                ))
            {
                string result = null;

                if (dlg.Run() == (int) ResponseType.Accept)
                {
                    result = dlg.Filename;
                }

                dlg.Destroy();

                return result;
            }
        }
    }

    public class BrowserWindow : Window
    {
        private WebView webBrowser;

        public WebView WebBrowser
        {
            get { return webBrowser; }
        }

		public BrowserWindow(WindowPrototype prototype) : base(WindowType.Toplevel)
        {
			Construct(prototype);
        }

		private void Construct(WindowPrototype prot)
        {
			string errorMessage = null;
			try
			{
				webBrowser = new WebView();
			}
			catch(Exception)
			{
				errorMessage = "Failed to create a WebKit.WebView widget.\nMake sure that libwebkitgtk-1.0 is installed.";
			}

			//todo: remove windowHandle creation from here
			WindowHandle handle = new WindowHandle (this, this.webBrowser);
			this.Destroyed += (o, args) =>
			{
				prot.OnClose (handle);
			};

			VBox vbox = new VBox(false, 0);
            this.Add(vbox);

			if (prot.Menu != null && prot.Menu.Any ())
			{
				MenuBar menuBar = CreateMenu (handle, prot.Menu);

				vbox.PackStart(menuBar, false, false, 0);
			}

			if (webBrowser == null)
			{
				Label errorLabel = new Label (errorMessage);
				vbox.PackEnd(errorLabel, true, true, 0);
			}
			else
			{
				vbox.PackEnd(webBrowser, true, true, 0);

				webBrowser.TitleChanged += (o, args) => 
				{
					string title = webBrowser.Title;
					Application.Invoke(delegate { this.Title = title; });
				};

				webBrowser.LoadUri(prot.Url);
			}

			//todo: bug, window cannot be resized to smaller size (seems related to https://bugs.webkit.org/show_bug.cgi?id=17154)
			Resize (prot.Width, prot.Height);

			vbox.ShowAll ();
       }

		private MenuBar CreateMenu (WindowHandle handle, List<MenuPrototype> menuItemsProt)
		{
			MenuBar menuBar = new MenuBar ();

			foreach (MenuPrototype menuItemProt in menuItemsProt)
			{
				menuBar.Append (CreateMenuItem(handle, menuItemProt));
			}

			return menuBar;
		}

		private MenuItem CreateMenuItem (WindowHandle handle, MenuPrototype menuItemProt)
		{
			MenuItem menuItem = new MenuItem(menuItemProt.Text);

			if (menuItemProt.OnAction != null)
			{
				menuItem.Activated += (sender, e) => { menuItemProt.OnAction(handle); };
			}	

			if (menuItemProt.Items != null && menuItemProt.Items.Any ())
			{
				Menu subMenu = new Menu();
				menuItem.Submenu = subMenu;

				foreach (MenuPrototype menuSubItemProt in menuItemProt.Items)
				{
					subMenu.Append(CreateMenuItem(handle, menuSubItemProt));
				}
			}

			return menuItem;
		}
    }
}