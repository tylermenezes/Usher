using System.Text.RegularExpressions;
using System.Collections.Generic;
using Usher.Platforms.Generic;
using Usher.Platforms.Generic.Devices;
using Usher.Platforms.ZWave.Devices;

namespace Usher.Platforms.ZWave
{
    [ManagerAttribute("zwave")]
    class Manager : GatewayBasedManager, IManager
    {
        public override string Provider => "zwave";

        public Manager(string instance, Dictionary<string, string> args) : base(instance, args)
        {}

        protected override IGateway GetGateway(string instance, Dictionary<string, string> config)
        {
            return new Gateway(this.Instance, config["port"]);
        }
    }
}