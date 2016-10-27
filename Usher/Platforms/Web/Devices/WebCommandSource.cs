using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.Web.Devices
{
    public class WebCommandSource : GenericDevice, ICommandSource
    {
        public override string Provider { get { return "web"; }}
        public override string Instance { get { return manager.Instance; }}
        public override string Id { get { return "source"; }}

        private Manager manager;
        public WebCommandSource(Manager manager)
        {
            this.manager = manager;
        }

        public event CommandReceivedHandler OnCommand;
        protected Dictionary<string, List<CommandReceivedHandler>> commandHandlers
                                = new Dictionary<string, List<CommandReceivedHandler>>();
        
        public void RegisterCommandListener(string command, CommandReceivedHandler onCommand)
        {
            if (!commandHandlers.ContainsKey(command)) commandHandlers.Add(command, new List<CommandReceivedHandler>());
            commandHandlers[command].Add(onCommand);
        }

        public void dispatchCommand(string command, string[] argv)
        {
            if (OnCommand != null) OnCommand(command, argv);
            if (commandHandlers.ContainsKey(command)) {
                commandHandlers[command].ForEach(h => h(command, argv));
            }
        }
    }
}