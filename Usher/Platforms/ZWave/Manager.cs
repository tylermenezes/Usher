using System.Collections.Generic;
using Usher.Platforms.Generic;
using Usher.Platforms.Generic.Devices;
using Usher.Platforms.ZWave.Devices;

namespace Usher.Platforms.ZWave
{
    [ManagerAttribute("zwave")]
    class Manager : GatewayBasedManager, IManager
    {
        public override string Provider { get { return "zwave"; }}

        public Manager(string instance, Dictionary<string, string> args) : base(instance, args)
        {}

        protected override IGateway getGateway(string instance, Dictionary<string, string> config)
        {
            return new Gateway(instance, config["port"]);
        }
    }
}