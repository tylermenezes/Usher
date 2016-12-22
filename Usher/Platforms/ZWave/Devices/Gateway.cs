using ZWaveLib;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;

namespace Usher.Platforms.ZWave.Devices
{
   public class Gateway : GenericDevice, IGateway
    {
        public override string Provider => "zwave";
        public override string Instance => InstanceId;

        public string InstanceId { get; private set;}

        public event GatewayDisconnectedHandler OnDisconnected;
        public event GatewayErrorHandler OnError;
        public event GatewayReadyHandler OnReady;

        public IEnumerable<IDevice> Devices { get; private set; }

        public ZWaveController Controller;
        public Gateway(string instance, string serialPort)
        {
            InstanceId = instance;
            Controller = new ZWaveController(serialPort);
            Controller.ControllerStatusChanged += OnControllerStatusChanged;
            Controller.DiscoveryProgress += OnDiscoveryStatusChanged;
        }

        SemaphoreSlim _startingSemaphor;
        public Task Start()
        {
            if (_startingSemaphor == null)
            {
                _startingSemaphor = new SemaphoreSlim(0, 1);
                Controller.Connect();
            }
            return _startingSemaphor.WaitAsync();
        }

        SemaphoreSlim _stoppingSemaphor;
        public Task Stop()
        {
            if (_stoppingSemaphor == null)
            {
                _stoppingSemaphor = new SemaphoreSlim(0, 1);
                Controller.Disconnect();
            }
            return _stoppingSemaphor.WaitAsync();
        }

        public void Heal()
        {
            Controller.HealNetwork();
        }

        protected void UpdateNodeCache()
        {
            var devices = new List<IDevice>();
            foreach (var node in Controller.Nodes)
            {
                Utilities.Logger.Debug("Node {0} discovered.", node.Id);
                if (RgbBulb.IsNodeInstance(node))
                    devices.Add(new RgbBulb {
                        Gateway = this,
                        ZWaveNode = node
                    });
                else if (Remote.IsNodeInstance(node))
                    devices.Add(new Remote(this, node));
            }
            Devices = devices;
            devices
                .ForEach(d => {
                    if (string.IsNullOrEmpty(d.Name)) d.Name = "Untitled";
                });
            Usher.Config.Devices.Instance.Save();

            if (devices.Count > 0) { Thread.Sleep(8000); OnReady?.Invoke(this); }
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
                    Controller.Initialize();
                    break;
                case ControllerStatus.Disconnected:
                    if (_stoppingSemaphor != null) {
                        _stoppingSemaphor.Release();
                        _stoppingSemaphor = null;
                    }
                    OnDisconnected?.Invoke(this);
                    break;
                case ControllerStatus.Error:
                    OnError?.Invoke(this);
                    break;
                case ControllerStatus.Ready:
                    Controller.Discovery();
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
                    UpdateNodeCache();
                    if (_startingSemaphor != null) {
                        _startingSemaphor.Release();
                        _startingSemaphor = null;
                    }
                    break;
                case DiscoveryStatus.DiscoveryError:
                    OnError?.Invoke(this);
                    break;
                case DiscoveryStatus.DiscoveryStart:
                default:
                    break;
            }
        }
    }
}