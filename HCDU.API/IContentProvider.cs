namespace HCDU.API
{
    public interface IContentProvider
    {
        bool IsStatic { get; }
        HttpResponse GetContent();
    }
}