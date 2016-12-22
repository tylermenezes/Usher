using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Usher.Platforms.Generic;
using Usher.Platforms.Generic.Devices;

namespace Usher.Plugins.TylerMenezes.Unifi
{
    [ManagerAttribute("unifi")]
    public class Manager : IManager
    {
        public string Provider => "unifi";
        public string Instance { get; protected set; }
        public string Uri => $"{Provider}://{Instance}";

        protected Dictionary<string, string> Config;

        public event ManagerReadyHandler OnReady;
        public event ManagerStoppedHandler OnStop;
        public event ManagerErrorHandler OnError;

        public Manager(string instance, Dictionary<string, string> config)
        {
            Config = config;
            Instance = instance;

            _presenceSource = new WifiClientPresenceSource(this);
            Clients = new List<string>();
        }

        private readonly WifiClientPresenceSource _presenceSource;
        public IEnumerable<IDevice> Devices => new IDevice[]{ _presenceSource };

        public List<string> Clients { get; protected set; }

        protected Thread UnifiPollThread;

        public Task Start()
        {
            var semaphore = new SemaphoreSlim(0, 1);
            UnifiPollThread?.Abort();
            UnifiPollThread = new Thread(async () =>
            {
                var unifiWorker = new UnifiApiWorker(new Uri($"https://{Instance}"), Config["username"],
                    Config["password"]);
                var firstUpdate = true;
                while (true)
                {
                    var oldClients = Clients;
                    Clients = (await unifiWorker.GetClients()).ToList();

                    // Events for new clients
                    Clients
                        .Where(c => !oldClients.Contains(c)).ToList()
                        .ForEach(c => _presenceSource.DispatchEnter(c));

                    // Events for removed clients
                    oldClients
                        .Where(c => !Clients.Contains(c)).ToList()
                        .ForEach(c => _presenceSource.DispatchLeave(c));

                    _presenceSource.DispatchUpdate();

                    if (firstUpdate)
                    {
                        semaphore.Release();
                        firstUpdate = false;
                    }
                    Thread.Sleep(5000);
                }
            });
            UnifiPollThread.Start();

            return semaphore.WaitAsync();
        }

        public Task Stop()
        {
            var task = new Task(() => {
                UnifiPollThread?.Abort();
            });
            task.Start();
            OnStop?.Invoke(this);
            return task;
        }
    }
}
