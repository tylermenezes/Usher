using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
namespace Usher.PluginFramework
{
    class Supervisor
    {
        protected static Supervisor _instance = new Supervisor();
        public static Supervisor Instance
        {
            get {
                return _instance;
            }
        }

        protected List<IPlugin> _plugins = new List<IPlugin>();
        public IEnumerable<IPlugin> Plugins { get { return _plugins; }}

        protected Supervisor()
        {
            // Enumerate plugins in the namespace.
            var pManagerTypes = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(s => s.GetTypes())
                                .Where(p => p.IsClass)
                                .Where(p => typeof(IPlugin).IsAssignableFrom(p));

            foreach (Type t in pManagerTypes) {
                 _plugins.Add((IPlugin)t.GetConstructors().First().Invoke(new object[]{}));
                Utilities.Logger.Info("Loaded plugin {0}", t.Name);
            }
        }

        public void Start()
        {
            foreach (IPlugin plugin in Plugins) {
                (new Thread((object threadData) => {
                    Utilities.Logger.Info("Starting plugin {0}", plugin.GetType().Name);
                    ((IPlugin)threadData).Main();
                })).Start(plugin);
            }
        }
    }
}