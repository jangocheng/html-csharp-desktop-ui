using System.Collections.Generic;
using HCDU.API;
using Gtk;
using System;
using System.Threading;

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
            //todo: implement ShowDialog in GtkPlatformAdapter
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
}