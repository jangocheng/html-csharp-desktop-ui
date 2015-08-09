using System;
using System.Threading;
using HCDU.API;
using HCDU.API.Server;

namespace HCDU.Content
{
    public static class HcduContent
    {
        private static int backendEventsCounter = 0;

        public static void AppendTo(ContentPackage contentPackage)
        {
            contentPackage.AddContent(typeof (HcduContent).Assembly, "HCDU.Content.Web");
            contentPackage.AddMethod("rest/cars/boxter", () => new Car {Model = "Porsche Boxster", Year = 1996});
            contentPackage.AddMethod<Car>("rest/exception", () => { throw new Exception("Test Exception"); });
            contentPackage.AddMethod("rest/selectFolder", () => SelectFolder(false));
            contentPackage.AddMethod("rest/selectNewFolder", () => SelectFolder(true));
            contentPackage.AddMethod("rest/backend-events/increment", BackendEventsIncrement);
            contentPackage.AddMethod("rest/backend-events/increment5Sec", BackendEventsIncrement5Sec);
        }

        private static string BackendEventsIncrement()
        {
            Thread thread = new Thread(() =>
                                       {
                                           Interlocked.Increment(ref backendEventsCounter);
                                           WebServer.SendMessage(backendEventsCounter.ToString());
                                       });
            thread.IsBackground = true;
            thread.Start();

            return null;
        }

        private static string BackendEventsIncrement5Sec()
        {
            Thread thread = new Thread(() =>
                                       {
                                           for (int i = 0; i < 10; i++)
                                           {
                                               Thread.Sleep(500);
                                               Interlocked.Increment(ref backendEventsCounter);
                                               WebServer.SendMessage(backendEventsCounter.ToString());
                                           }
                                       });
            thread.IsBackground = true;
            thread.Start();

            return null;
        }

        private static string SelectFolder(bool allowCreateFolder)
        {
            return Platform.OpenFolderBrowserDialog(allowCreateFolder);
        }
    }

    public class Car
    {
        public string Model { get; set; }
        public int Year { get; set; }
    }
}
