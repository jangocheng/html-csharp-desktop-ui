using System;
using System.Windows.Forms;
using HCDU.API;
using HCDU.Content;

namespace HCDU.Windows
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ApplicationContainer container = new ApplicationContainer();
            container.Platform = new WinFormsPlatformAdapter();
            container.ApplicationPackage = new HcduContent();
            
            container.Start();

            //todo: use ApplicationContext instead ?
            Application.Run((Form) container.CreateMainWindow());
        }
    }
}
