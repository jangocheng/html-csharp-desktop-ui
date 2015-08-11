using System;
using Gtk;
using HCDU.API;
using HCDU.Content;
using WebKit;
using HCDU.API.Server;
using HCDU.Linux.Gtk;
using System.Threading;

public partial class MainWindow: Gtk.Window
{
	private const string BaseUrl = "http://localhost:8899/";
	private const string HomePageUrl = BaseUrl + "index.html";

	private WebServer webServer;
	private WebView webBrowser;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();

		//todo: bug, window cannot be resized to smaller size (seems related to https://bugs.webkit.org/show_bug.cgi?id=17154)
		this.Resize (1024, 640);

		ContentPackage contentPackage = new ContentPackage();
		HcduContent.AppendTo(contentPackage);
		DebugPages.AppendTo(contentPackage);
		Platform.SetAdapter(new GtkPlatformAdapter(this));

        SocketPackage socketPackage = new SocketPackage();
        HcduContent.AppendTo(socketPackage);

        StartServer(contentPackage, socketPackage);

		InitBrowser (contentPackage);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

    void StartServer (ContentPackage contentPackage, SocketPackage socketPackage)
    {
        webServer = new WebServer(contentPackage, socketPackage, 8899);
        webServer.Start();
    }

	void InitBrowser (ContentPackage contentPackage)
	{
		webBrowser = new WebView ();
		webBrowser.Name = "webBrowser";

		vbox1.Add (webBrowser);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1 [webBrowser]));
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
