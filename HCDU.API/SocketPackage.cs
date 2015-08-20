using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using HCDU.Web.Api;

namespace HCDU.API
{
    public class SocketPackage
    {
        private readonly Dictionary<string, ISocketProvider> socketProviders = new Dictionary<string, ISocketProvider>();

        public IEnumerable<string> Sockets
        {
            get { return socketProviders.Keys.ToList(); }
        }

        public void AddSocketProvider(string socketLocation, ISocketProvider socketProvider)
        {
            if (socketProviders.ContainsKey(socketLocation))
            {
                throw new HcduException(string.Format("Threre are multiple socket providers for location: {0}.", socketLocation));
            }
            socketProviders.Add(socketLocation, socketProvider);
        }

        public ISocketProvider GetSocketProvider(string socketLocation)
        {
            ISocketProvider provider;
            socketProviders.TryGetValue(socketLocation, out provider);
            return provider;
        }
    }

    //todo: move to another file
    public interface ISocketProvider
    {
        ISocket CreateSocket(IWebSocket webSocket);
    }

    //todo: move to another file
    public interface ISocket
    {
        void ProcessMessage(string message);

        void ProcessMessage(byte[] message);

        void OnCreate(ISocketProvider provider);

        void OnDestroy(ISocketProvider provider);
    }

    //todo: move to another file
    public class StateSocketProvider<T> : ISocketProvider
    {
        //todo: ensure thread-safe access
        private readonly HashSet<StateSocket<T>> activeSockets = new HashSet<StateSocket<T>>();

        public T State { get; set; }

        public ISocket CreateSocket(IWebSocket webSocket)
        {
            return new StateSocket<T>(this, webSocket);
        }

        public void SendState()
        {
            foreach (StateSocket<T> socket in activeSockets)
            {
                socket.SendState();
            }
        }

        public void Register(StateSocket<T> stateSocket)
        {
            activeSockets.Add(stateSocket);
        }

        public void Unregister(StateSocket<T> stateSocket)
        {
            activeSockets.Remove(stateSocket);
        }
    }

    public class StateSocket<T> : ISocket
    {
        private readonly StateSocketProvider<T> socketProvider;
        private readonly IWebSocket webSocket;

        public StateSocket(StateSocketProvider<T> socketProvider, IWebSocket webSocket)
        {
            this.socketProvider = socketProvider;
            this.webSocket = webSocket;
        }

        public void ProcessMessage(string message)
        {
            //todo: specify protocol
            SendState();
        }

        public void ProcessMessage(byte[] message)
        {
            //todo: specify protocol
            SendState();
        }

        public void OnCreate(ISocketProvider provider)
        {
            socketProvider.Register(this);
        }

        public void OnDestroy(ISocketProvider provider)
        {
            socketProvider.Unregister(this);
        }

        //todo: don't serialize in each socket, serialize in parent (socketProvider)
        public void SendState()
        {
            UTF8Encoding encoding = new UTF8Encoding(false);

            //todo: handle null
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof (T));
            MemoryStream mem = new MemoryStream();
            ser.WriteObject(mem, socketProvider.State);

            //todo: don't convert string to bytes and back
            webSocket.SendMessage(encoding.GetString(mem.ToArray()));
        }
    }
}