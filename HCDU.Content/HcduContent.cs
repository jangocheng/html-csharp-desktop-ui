using System;
using HCDU.API;

namespace HCDU.Content
{
    public static class HcduContent
    {
        public static void AppendTo(ContentPackage contentPackage)
        {
            contentPackage.AddContent(typeof (HcduContent).Assembly, "HCDU.Content.Web");
            contentPackage.AddMethod("rest/cars/boxter", () => new Car {Model = "Porsche Boxster", Year = 1996});
            contentPackage.AddMethod<Car>("rest/exception", () => { throw new Exception("Test Exception"); });
        }
    }

    public class Car
    {
        public string Model { get; set; }
        public int Year { get; set; }
    }
}