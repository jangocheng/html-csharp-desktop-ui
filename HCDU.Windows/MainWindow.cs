using System;
using System.Drawing;
using System.Windows.Forms;
using CefSharp.WinForms;

namespace HCDU.Windows
{
    public partial class MainWindow : Form
    {
        private const string BaseUrl = "http://localhost:8899/";
        private const string HomePageUrl = BaseUrl + "index.html";

        private ChromiumWebBrowser webBrowser;

        public ChromiumWebBrowser WebBrowser
        {
            get { return webBrowser; }
        }

        public MainWindow()
        {
            InitializeComponent();

            InitBrowser();
        }

        private void InitBrowser()
        {
            webBrowser = new ChromiumWebBrowser("about:blank");
            webBrowser.Name = "webBrowser";
            webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser.Location = new Point(0, 0);
            webBrowser.Size = webBrowserPanel.ClientSize;
            webBrowser.TabIndex = 1;

            webBrowserPanel.Controls.Add(webBrowser);

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
