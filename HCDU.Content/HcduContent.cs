using HCDU.API;

namespace HCDU.Content
{
    public static class HcduContent
    {
        public static void AppendTo(ContentPackage contentPackage)
        {
            contentPackage.AddContent(typeof (HcduContent).Assembly, "HCDU.Content.Web");
        }
    }
}