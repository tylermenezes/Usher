using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms
{
    class Supervisor
    {
        public static readonly Supervisor Instance = new Supervisor();

        protected List<IManager> PlatformManagers = new List<IManager>();

        protected Supervisor()
        {
            // Enumerate platform managers in the namespace.
            var pManagerTypes = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(s => s.GetTypes())
                                .Where(p => p.IsClass)
                                .Where(p => typeof(IManager).IsAssignableFrom(p))
                                .Where(p => p.GetCustomAttributes(typeof(ManagerAttribute), true).Length > 0);

            // Instantiate platform managers from the config file
            foreach (var t in pManagerTypes) {
                var ctor = t.GetConstructors().First();
                var attr = (ManagerAttribute)t.GetCustomAttributes(typeof(ManagerAttribute), true).First();

                foreach (var config
                            in Config.Devices.Instance.Platforms.Where(p => p.Platform == attr.Id).ToList()) {

                    var manager = (IManager)ctor.Invoke(new object[]{config.Instance, config.Config});
                    manager.OnReady += OnManagerReady;
                    manager.OnStop += OnManagerStopped;
                    manager.OnError += OnManagerError;

                    Utilities.Logger.Info("Loaded manager {0}", manager.Uri);

                    PlatformManagers.Add(manager);
                }
            }
        }

        protected void OnManagerReady(IManager manager)
        {
            Utilities.Logger.Debug("{0} is ready.", manager.Uri);
        }

        protected void OnManagerStopped(IManager manager)
        {
            Utilities.Logger.Debug("{0} has stopped.", manager.Uri);
        }

        protected void OnManagerError(IManager manager)
        {
            Utilities.Logger.Debug("{0} experienced an error.", manager.Uri);
        }

        public Task Start()
        {
            return Task.WhenAll(PlatformManagers.Select(p => p.Start()));
        }

        public Task Stop()
        {
            return Task.WhenAll(PlatformManagers.Select(p => p.Stop()));
        }

        public IEnumerable<IDevice> Devices
        {
            get
            {
                return PlatformManagers
                        .Select(p => p.Devices)
                        .SelectMany(d => d);
            }
        }
    }
}