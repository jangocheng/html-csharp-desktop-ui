using System;
using Gtk;
using WebKit;

public partial class MainWindow: Window
{
	private const string BaseUrl = "http://localhost:8899/";
	private const string HomePageUrl = BaseUrl + "index.html";

	private WebView webBrowser;

    public WebView WebBrowser
    {
        get { return webBrowser; }
    }

    public MainWindow () : base (WindowType.Toplevel)
	{
		Build ();

		//todo: bug, window cannot be resized to smaller size (seems related to https://bugs.webkit.org/show_bug.cgi?id=17154)
		Resize (1024, 640);

		InitBrowser ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	void InitBrowser ()
	{
		webBrowser = new WebView ();
		webBrowser.Name = "webBrowser";

		vbox1.Add (webBrowser);
		Box.BoxChild w3 = ((Box.BoxChild)(this.vbox1 [webBrowser]));
		w3.Position = 1;
		this.vbox1.ShowAll();

		webBrowser.LoadUri(HomePageUrl);
	}

	protected void MenuActionShowHomePage (object sender, EventArgs e)
	{
		webBrowser.LoadUri (HomePageUrl);
	}

	private void MenuActionShowAngularMaterial(object sender, EventArgs e)
	{
		webBrowser.LoadUri ("https://material.angularjs.org");
	}

	private void MenuActionReload(object sender, EventArgs e)
	{
		webBrowser.Reload();
	}

	private void MenuActionShowResources(object sender, EventArgs e)
	{
		webBrowser.LoadUri(BaseUrl + "debug/resources.html");
	}
}
