using System.Drawing;
using System.Windows.Forms;
using CefSharp.WinForms;
using HCDU.API;
using HCDU.Content;

namespace HCDU.Windows
{
    public partial class MainWindow : Form
    {
        private ChromiumWebBrowser webBrowser;

        public MainWindow()
        {
            InitializeComponent();

            ContentPackage contentPackage = new ContentPackage();
            HcduContent.AppendTo(contentPackage);

            InitBrowser(contentPackage);
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

            webBrowser.ResourceHandlerFactory = new ContentPackageResourceHandlerFactory("http://localhost", contentPackage);
            webBrowser.Load("http://localhost");
        }

        private void MenuActionGoToGoogle(object sender, System.EventArgs e)
        {
            webBrowser.Load("http://www.google.com");
        }

        private void MenuActionOpenDevTools(object sender, System.EventArgs e)
        {
            webBrowser.ShowDevTools();
        }

        private void MenuActionRefresh(object sender, System.EventArgs e)
        {
            webBrowser.Reload(true);
        }
    }
}
