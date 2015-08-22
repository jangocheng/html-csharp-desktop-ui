using System;
using System.Threading;
using HCDU.API;

namespace HCDU.Content
{
    //todo: rename class
    public class HcduContent : ApplicationPackage
    {
        private int backendEventsCounter = 100;
        private readonly StateSocketProvider<int> stateSocket = new StateSocketProvider<int>();

        public HcduContent()
        {
            MainWindowPrototype = new WindowPrototype();
            MainWindowPrototype.Url = "index.html";

            MenuPrototype toolsMenu = new MenuPrototype{Text = "Tools"};
            MainWindowPrototype.Menu.Add(toolsMenu);

            toolsMenu.Items.Add(new MenuPrototype { Text = "Home Page", OnAction = wh => { Container.NavigateTo(wh, MainWindowPrototype.Url); } });
            toolsMenu.Items.Add(new MenuPrototype { Text = "Angular Material Website", OnAction = wh => { Container.NavigateTo(wh, "https://material.angularjs.org"); } });
            toolsMenu.Items.Add(new MenuPrototype { Text = "Developer tools", OnAction = wh => { Container.ShowDevTools(wh); } });
            toolsMenu.Items.Add(new MenuPrototype { Text = "Reload", OnAction = wh => { Container.ReloadPage(wh); } });
            toolsMenu.Items.Add(new MenuPrototype { Text = "Resources List", OnAction = wh => { Container.NavigateTo(wh, DebugPages.ResourcesListUrl); } });

            Content.AddContent(typeof(HcduContent).Assembly, "HCDU.Content.Web");
            Content.AddMethod("rest/cars/boxter", () => new Car { Model = "Porsche Boxster", Year = 1996 });
            Content.AddMethod<Car>("rest/exception", () => { throw new Exception("Test Exception"); });
            Content.AddMethod("rest/selectFolder", () => SelectFolder(false));
            Content.AddMethod("rest/selectNewFolder", () => SelectFolder(true));
            Content.AddMethod("rest/showCustomDialog", ShowCustomDialog);
            Content.AddMethod("rest/closeCustomDialog", CloseCustomDialog);
            Content.AddMethod("rest/backend-events/increment", BackendEventsIncrement);
            Content.AddMethod("rest/backend-events/increment5Sec", BackendEventsIncrement5Sec);

            Sockets.AddSocketProvider("ws/backend-events", stateSocket);
            stateSocket.State = backendEventsCounter;

            //todo: use more generic way of combining packages?
            DebugPages.AppendTo(Content);
        }

        //todo: remove
        public static void AppendTo(ContentPackage contentPackage)
        {
            //throw new NotImplementedException();
        }

        //todo: remove
        public static void AppendTo(SocketPackage socketPackage)
        {
            //throw new NotImplementedException();
        }

        private string BackendEventsIncrement()
        {
            Thread thread = new Thread(() =>
                                       {
                                           Interlocked.Increment(ref backendEventsCounter);
                                           stateSocket.State = backendEventsCounter;
                                           stateSocket.SendState();
                                       });
            thread.IsBackground = true;
            thread.Start();

            return null;
        }

        private string BackendEventsIncrement5Sec()
        {
            Thread thread = new Thread(() =>
                                       {
                                           for (int i = 0; i < 10; i++)
                                           {
                                               Thread.Sleep(500);
                                               Interlocked.Increment(ref backendEventsCounter);
                                               stateSocket.State = backendEventsCounter;
                                               stateSocket.SendState();
                                           }
                                       });
            thread.IsBackground = true;
            thread.Start();

            return null;
        }

        private string SelectFolder(bool allowCreateFolder)
        {
            return Container.OpenFolderBrowserDialog(allowCreateFolder);
        }

        private string ShowCustomDialog()
        {
            WindowPrototype win = new WindowPrototype();
            win.Url = "dialogs/custom-dialog/custom-dialog.html";
            Container.ShowDialog(win);
            //todo: return some value?
            return "todo";
        }

        private string CloseCustomDialog()
        {
            //todo: remove result or add validation
            Container.CloseDialog();
            return "todo";
        }
    }

    //todo: move to other namespace
    public class Car
    {
        public string Model { get; set; }
        public int Year { get; set; }
    }
}
