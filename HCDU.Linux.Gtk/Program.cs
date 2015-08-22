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

			container.Start ();

			Window mainWindow = (Window) container.CreateMainWindow();
			mainWindow.DeleteEvent += (o, a) => 
			{
				Application.Quit();
				a.RetVal = true;
			};

            mainWindow.Show();
			Application.Run ();
		}
	}
}
