using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CefSharp.WinForms;
using HCDU.API;

namespace HCDU.Windows
{
    public class WinFormsPlatformAdapter : IPlatformAdapter
    {
        private readonly Stack<Form> windowStack = new Stack<Form>();

        public WinFormsPlatformAdapter(Form mainWindow)
        {
            windowStack.Push(mainWindow);
        }

        public string OpenFolderBrowserDialog(bool allowCreateFolder)
        {
            Form parent = windowStack.Peek();

            if (parent.InvokeRequired)
            {
                return (string) parent.Invoke(new Func<bool, string>(OpenFolderBrowserDialog), allowCreateFolder);
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = allowCreateFolder;

            if (dlg.ShowDialog(parent) != DialogResult.OK)
            {
                return null;
            }

            return dlg.SelectedPath;
        }

        public void ShowDialog(string url)
        {
            Form parent = windowStack.Peek();

            if (parent.InvokeRequired)
            {
                parent.Invoke(new Action<string>(ShowDialog), url);
                return;
            }

            //todo: remove base URL from here
            url = "http://localhost:8899/" + url;

            Form window = ConstructDialog(url);
            //todo: this is not very reliable way to forget window
            window.Closed += (sender, args) => windowStack.Pop();
            windowStack.Push(window);

            //todo: use ShowDialog when CefSharp 43 is released (now it freezes the application)
            window.Show(parent);
        }

        public void CloseDialog()
        {
            Form parent = windowStack.Peek();

            if (parent.InvokeRequired)
            {
                parent.Invoke(new Action(CloseDialog));
                return;
            }

            parent.Close();
        }

        private Form ConstructDialog(string url)
        {
            Form form = new Form();

            //todo: is SuspendLayout/ResumeLayout required?
            form.SuspendLayout();
            ChromiumWebBrowser webBrowser = new ChromiumWebBrowser(url);
            form.Controls.Add(webBrowser);
            webBrowser = new ChromiumWebBrowser("about:blank");
            webBrowser.Name = "webBrowser";
            webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser.Location = new Point(0, 0);
            webBrowser.Size = form.ClientSize;
            webBrowser.TabIndex = 1;

            form.Controls.Add(webBrowser);
            //todo: is SuspendLayout/ResumeLayout required?
            form.ResumeLayout();

            return form;
        }
    }
}