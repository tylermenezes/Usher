using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.Web.Devices
{
    public class WebCommandSource : GenericDevice, ICommandSource
    {
        public override string Provider => "web";
        public override string Instance => _manager.Instance;
        public override string Id => "source";

        private readonly Manager _manager;
        public WebCommandSource(Manager manager)
        {
            this._manager = manager;
        }

        public event CommandReceivedHandler OnCommand;
        protected Dictionary<string, List<CommandReceivedHandler>> CommandHandlers
                                = new Dictionary<string, List<CommandReceivedHandler>>();
        
        public void RegisterCommandListener(string command, CommandReceivedHandler onCommand)
        {
            if (!CommandHandlers.ContainsKey(command)) CommandHandlers.Add(command, new List<CommandReceivedHandler>());
            CommandHandlers[command].Add(onCommand);
        }

        public void DispatchCommand(string command, string[] argv)
        {
            if (OnCommand != null) OnCommand(command, argv);
            if (CommandHandlers.ContainsKey(command)) {
                CommandHandlers[command].ForEach(h => h(command, argv));
            }
        }
    }
}