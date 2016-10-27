using ZWaveLib;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;

namespace Usher.Platforms.ZWave.Devices
{
   public class Gateway : GenericDevice, IGateway
    {
        public override string Provider { get { return "zwave"; }}
        public override string Instance { get { return InstanceId; }}

        public string InstanceId { get; private set;}

        public event GatewayDisconnectedHandler OnDisconnected;
        public event GatewayErrorHandler OnError;
        public event GatewayReadyHandler OnReady;

        public IEnumerable<IDevice> Devices { get; private set; }

        public ZWaveController controller;
        public Gateway(string instance, string serialPort)
        {
            InstanceId = instance;
            controller = new ZWaveController(serialPort);
            controller.ControllerStatusChanged += OnControllerStatusChanged;
            controller.DiscoveryProgress += OnDiscoveryStatusChanged;
        }

        SemaphoreSlim startingSemaphor;
        public Task Start()
        {
            if (startingSemaphor == null)
            {
                startingSemaphor = new SemaphoreSlim(0, 1);
                controller.Connect();
            }
            return startingSemaphor.WaitAsync();
        }

        SemaphoreSlim stoppingSemaphor;
        public Task Stop()
        {
            if (stoppingSemaphor == null)
            {
                stoppingSemaphor = new SemaphoreSlim(0, 1);
                controller.Disconnect();
            }
            return stoppingSemaphor.WaitAsync();
        }

        public void Heal()
        {
            controller.HealNetwork();
        }

        protected void updateNodeCache()
        {
            var devices = new List<IDevice>();
            foreach (ZWaveNode node in controller.Nodes)
            {
                if (RgbBulb.IsNodeInstance(node))
                    devices.Add(new RgbBulb {
                        gateway = this,
                        node = node
                    });
            }
            Devices = devices;
            devices
                .ForEach(d => {
                    if (string.IsNullOrEmpty(d.Name)) d.Name = "Untitled";
                });
            Config.Devices.Instance.Save();

            Thread.Sleep(4000);
            OnReady(this);
        }

        /// <summary>
        /// Delegate to be called whenever the Z-Wave controller's status changes - prepares the library.
        /// </summary>
        /// <param name="sender">The Z-Wave controller which sent the change.</param>
        /// <param name="args"></param>
        protected void OnControllerStatusChanged(object sender, ControllerStatusEventArgs args)
        {
            switch (args.Status)
            {
                case ControllerStatus.Connected:
                    controller.Initialize();
                    break;
                case ControllerStatus.Disconnected:
                    if (stoppingSemaphor != null) {
                        stoppingSemaphor.Release();
                        stoppingSemaphor = null;
                    }
                    OnDisconnected(this);
                    break;
                case ControllerStatus.Error:
                    OnError(this);
                    break;
                case ControllerStatus.Ready:
                    controller.Discovery();
                    break;
                case ControllerStatus.Initializing:
                default:
                    break;
            }
        }

        protected void OnDiscoveryStatusChanged(object sender, DiscoveryProgressEventArgs args)
        {
            switch (args.Status)
            {
                case DiscoveryStatus.DiscoveryEnd:
                    updateNodeCache();
                    if (startingSemaphor != null) {
                        startingSemaphor.Release();
                        startingSemaphor = null;
                    }
                    break;
                case DiscoveryStatus.DiscoveryError:
                    OnError(this);
                    break;
                case DiscoveryStatus.DiscoveryStart:
                default:
                    break;
            }
        }
    }
}