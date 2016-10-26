using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.Generic
{
    abstract class GatewayBasedManager : IManager
    {
        abstract public string Provider { get; }
        virtual public string Instance { get; protected set; }
        virtual public string Uri
        {
            get
            {
                return string.Format("{0}://{1}", Provider, Instance);
            }
        }

        public event ManagerErrorHandler OnError;
        public event ManagerReadyHandler OnReady;
        public event ManagerStoppedHandler OnStop;

        public IEnumerable<IDevice> Devices { get { return gateway.Devices; }}
        protected IGateway gateway;

        public GatewayBasedManager(string instance, Dictionary<string, string> config)
        {
            Instance = instance;
            gateway = getGateway(instance, config);
            gateway.OnDisconnected += delegate(IGateway sender) { OnStop(this); };
            gateway.OnError += delegate(IGateway sender) { OnError(this); };
            gateway.OnReady += delegate(IGateway sender) { OnReady(this); };
        }

        protected abstract IGateway getGateway(string instance, Dictionary<string, string> config);

        public Task Start()
        {
            return gateway.Start();
        }

        public Task Stop()
        {
            return gateway.Stop();
        }
    }
}