using System;
using System.Drawing;
using System.Windows.Forms;
using CefSharp.WinForms;
using HCDU.API;

namespace HCDU.Windows
{
    public class WinFormsPlatformAdapter : IPlatformAdapter
    {
        public string OpenFolderBrowserDialog(WindowHandle parent, bool allowCreateFolder)
        {
            Form parentForm = (Form)parent.NativeWindow;

            if (parentForm.InvokeRequired)
            {
                return (string) parentForm.Invoke(new Func<WindowHandle, bool, string>(OpenFolderBrowserDialog), parent, allowCreateFolder);
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = allowCreateFolder;

            if (dlg.ShowDialog(parentForm) != DialogResult.OK)
            {
                return null;
            }

            return dlg.SelectedPath;
        }

        public WindowHandle CreateWindow(WindowPrototype prototype)
        {
            return ConstructDialog(prototype);
        }

        public WindowHandle ShowDialog(WindowHandle parent, WindowPrototype prototype)
        {
            Form parentForm = (Form) parent.NativeWindow;

            if (parentForm.InvokeRequired)
            {
                return (WindowHandle) parentForm.Invoke(new Func<WindowHandle, WindowPrototype, WindowHandle>(ShowDialog), parent, prototype);
            }

            WindowHandle handle = ConstructDialog(prototype);
            Form window = (Form) handle.NativeWindow;
            window.Closed += (sender, args) => prototype.OnClose(handle);

            //todo: use ShowDialog when CefSharp 43 is released (now it freezes the application)
            window.Show(parentForm);

            //todo: this would conflict with window.ShowDialog
            return handle;
        }

        public void CloseDialog(WindowHandle win)
        {
            Form form = (Form) win.NativeWindow;

            if (form.InvokeRequired)
            {
                form.Invoke(new Action<WindowHandle>(CloseDialog), win);
                return;
            }

            form.Close();
        }

        private WindowHandle ConstructDialog(WindowPrototype prototype)
        {
            Form form = new Form();

            //todo: is SuspendLayout/ResumeLayout required?
            form.SuspendLayout();

            ChromiumWebBrowser webBrowser = new ChromiumWebBrowser(prototype.Url);
            form.Controls.Add(webBrowser);

            webBrowser.Name = "webBrowser";
            webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser.Location = new Point(0, 0);
            webBrowser.Size = form.ClientSize;
            webBrowser.TabIndex = 1;

            form.Controls.Add(webBrowser);
            //todo: is SuspendLayout/ResumeLayout required?
            form.ResumeLayout();

            return new WindowHandle(form, webBrowser);
        }
    }
}