using Gtk;
using HCDU.API;
using HCDU.Content;

namespace HCDU.Linux.Gtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();

            ApplicationContainer container = new ApplicationContainer();
            container.Platform = new GtkPlatformAdapter();
            container.ApplicationPackage = new HcduContent();

            //todo: use generic window creation approach
            //Window mainWindow = container.CreateMainWindow();

            MainWindow mainWindow = new MainWindow();
            container.Start(mainWindow, mainWindow.WebBrowser);

            mainWindow.Show();
			Application.Run ();
		}
	}
}
