using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
namespace Usher.PluginFramework
{
    class Supervisor
    {
        public static readonly Supervisor Instance = new Supervisor();

        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        public IEnumerable<IPlugin> Plugins => _plugins;

        protected Supervisor()
        {
            // Enumerate plugins in the namespace.
            var pManagerTypes = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(s => s.GetTypes())
                                .Where(p => p.IsClass)
                                .Where(p => typeof(IPlugin).IsAssignableFrom(p));

            foreach (var t in pManagerTypes) {
                 _plugins.Add((IPlugin)t.GetConstructors().First().Invoke(new object[]{}));
                Utilities.Logger.Info("Loaded plugin {0}", t.Name);
            }
        }

        public void Start()
        {
            foreach (var plugin in Plugins) {
                (new Thread((object threadData) => {
                    Utilities.Logger.Info("Starting plugin {0}", plugin.GetType().Name);
                    ((IPlugin)threadData).Main();
                })).Start(plugin);
            }
        }
    }
}