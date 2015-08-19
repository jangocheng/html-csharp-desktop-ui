namespace HCDU.API
{
    public class ApplicationPackage
    {
        private readonly ContentPackage content = new ContentPackage();
        private readonly SocketPackage sockets = new SocketPackage();

        public ContentPackage Content
        {
            get { return content; }
        }

        public SocketPackage Sockets
        {
            get { return sockets; }
        }

        public WindowPrototype MainWindowPrototype { get; set; }

        public ApplicationContainer Container { get; set; }

        public virtual void OnStart()
        {
        }
    }
}