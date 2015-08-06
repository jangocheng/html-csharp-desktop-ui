using System;
using System.Drawing;
using System.Windows.Forms;
using CefSharp.WinForms;
using HCDU.API;
using HCDU.API.Server;
using HCDU.Content;

namespace HCDU.Windows
{
    public partial class MainWindow : Form
    {
        private const string BaseUrl = "http://localhost:8899/";
        private const string HomePageUrl = BaseUrl + "index.html";

        private WebServer webServer;
        private ChromiumWebBrowser webBrowser;

        public MainWindow()
        {
            InitializeComponent();

            ContentPackage contentPackage = new ContentPackage();
            HcduContent.AppendTo(contentPackage);
            DebugPages.AppendTo(contentPackage);
            Platform.SetAdapter(new WinFormsPlatformAdapter(this));

            StartServer(contentPackage);

            InitBrowser(contentPackage);
        }

        void StartServer(ContentPackage contentPackage)
        {
            webServer = new WebServer(contentPackage, 8899);
            webServer.Start();
        }

        private void InitBrowser(ContentPackage contentPackage)
        {
            webBrowser = new ChromiumWebBrowser("about:blank");
            webBrowser.Name = "webBrowser";
            webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser.Location = new Point(0, 0);
            webBrowser.Size = webBrowserPanel.ClientSize;
            webBrowser.TabIndex = 1;

            webBrowserPanel.Controls.Add(webBrowser);

            webBrowser.ResourceHandlerFactory = new ContentPackageResourceHandlerFactory(BaseUrl, contentPackage);
            webBrowser.Load(HomePageUrl);
        }

        private void MenuActionShowHomePage(object sender, EventArgs e)
        {
            webBrowser.Load(HomePageUrl);
        }

        private void MenuActionShowAngularMaterial(object sender, EventArgs e)
        {
            webBrowser.Load("https://material.angularjs.org");
        }

        private void MenuActionReload(object sender, EventArgs e)
        {
            webBrowser.Reload(true);
        }

        private void MenuActionOpenDevTools(object sender, EventArgs e)
        {
            webBrowser.ShowDevTools();
        }

        private void MenuActionShowResources(object sender, EventArgs e)
        {
            webBrowser.Load(BaseUrl + "debug/resources.html");
        }
    }
}
