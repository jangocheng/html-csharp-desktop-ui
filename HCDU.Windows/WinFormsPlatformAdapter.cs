using System.Collections.Generic;
using System.Windows.Forms;
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

        private delegate string OpenFolderBrowserDialogDelegate(bool allowCreateFolder);

        public string OpenFolderBrowserDialog(bool allowCreateFolder)
        {
            Form parent = windowStack.Peek();

            if (parent.InvokeRequired)
            {
                return (string)parent.Invoke(new OpenFolderBrowserDialogDelegate(OpenFolderBrowserDialog), allowCreateFolder);
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = allowCreateFolder;

            if (dlg.ShowDialog(parent) != DialogResult.OK)
            {
                return null;
            }

            return dlg.SelectedPath;
        }
    }
}