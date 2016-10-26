using System.Collections.Generic;

namespace Usher.Config.Entities
{
    public class PlatformInstance
    {
        public string Platform { get; set; }
        public string Instance { get; set; }
        public List<string> Config { get; set; }

        public List<Node> Nodes { get; set; }
    }
}