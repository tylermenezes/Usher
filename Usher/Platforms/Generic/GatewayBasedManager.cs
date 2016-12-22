using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;

// ReSharper disable VirtualMemberCallInConstructor
namespace Usher.Platforms.Generic
{
    abstract class GatewayBasedManager : IManager
    {
        public abstract string Provider { get; }
        public virtual string Instance { get; protected set; }
        public virtual string Uri => $"{Provider}://{Instance}";

        public event ManagerErrorHandler OnError;
        public event ManagerReadyHandler OnReady;
        public event ManagerStoppedHandler OnStop;

        public IEnumerable<IDevice> Devices => Gateway.Devices;
        protected IGateway Gateway;

        public GatewayBasedManager(string instance, Dictionary<string, string> config)
        {
            Instance = instance;
            Gateway = GetGateway(instance, config);
            Gateway.OnDisconnected += delegate(IGateway sender) { OnStop?.Invoke(this); };
            Gateway.OnError += delegate(IGateway sender) { OnError?.Invoke(this); };
            Gateway.OnReady += delegate(IGateway sender) { OnReady?.Invoke(this); };
        }

        protected abstract IGateway GetGateway(string instance, Dictionary<string, string> config);

        public Task Start()
        {
            return Gateway.Start();
        }

        public Task Stop()
        {
            return Gateway.Stop();
        }
    }
}