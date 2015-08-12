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
		private readonly Stack<Window> windowStack = new Stack<Window>();

		public GtkPlatformAdapter(Window mainWindow)
		{
			windowStack.Push(mainWindow);
		}

		public string OpenFolderBrowserDialog(bool allowCreateFolder)
		{
			return InvokeSync(() => OpenFolderBrowserDialogHandler(allowCreateFolder));
		}

	    public void ShowDialog(string url)
	    {
			InvokeSync(() => ShowDialogHandler(url));
	    }

	    public void CloseDialog()
	    {
	        //todo: implement
            throw new NotImplementedException();
	    }

	    //todo: review this approach, it does not look safe
		private TResult InvokeSync<TResult>(Func<TResult> func)
		{
			TResult result = default(TResult);
			ManualResetEvent ev = new ManualResetEvent(false);
			Application.Invoke (delegate {
				try{
					result = func();
				}
				finally{
					ev.Set();
				}
			});
			ev.WaitOne ();
			return result;
		}

		private bool ShowDialogHandler(string url)
		{
			//todo: remove base URL from here
			//url = "http://localhost:8899/" + url;
			url = "http://localhost:8899/index.html";

			//todo: is this a worng place to pick window ? (seems not threadsafe)
			Window parent = windowStack.Peek();
			BrowserWindow win = new BrowserWindow (url);

			win.TransientFor = parent;
			win.Modal = true;
			win.SkipTaskbarHint = true;
			win.TypeHint = Gdk.WindowTypeHint.Dialog;
			//todo: bug, window cannot be resized to smaller size (seems related to https://bugs.webkit.org/show_bug.cgi?id=17154)
			win.Resize (1024, 640);
			win.ShowAll ();

			//todo: this is fake result
			return true;
		}

		private string OpenFolderBrowserDialogHandler(bool allowCreateFolder)
		{
			//todo: is this a worng place to pick window ? (seems not threadsafe)
			Window parent = windowStack.Peek();
			FileChooserAction action = allowCreateFolder ? FileChooserAction.CreateFolder: FileChooserAction.SelectFolder;

			FileChooserDialog dlg = new FileChooserDialog(
				"Select a folder",
				parent,
				action, 
				"Cancel", ResponseType.Cancel,
				"Open", ResponseType.Accept
			);

			string result = null;

			if (dlg.Run () == (int)ResponseType.Accept)
			{
				result = dlg.Filename;
			}

			dlg.Destroy ();

			return result;
		}
	}

	public class BrowserWindow : Window
	{
		private WebView webBrowser;

		public BrowserWindow(string url) : base (WindowType.Toplevel)
		{
			InitBrowser (url);
		}

		private void InitBrowser(string url)
		{
			webBrowser = new WebView ();
			webBrowser.Name = "webBrowser";

			VBox vbox = new VBox ();
			vbox.Name = "vbox";
			this.Add (vbox);

			vbox.Add (webBrowser);
			global::Gtk.Box.BoxChild webBrowserBox = ((global::Gtk.Box.BoxChild)(vbox [webBrowser]));
			webBrowserBox.Position = 0;

			webBrowser.LoadUri(url);
		}
	}
}