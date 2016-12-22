using System;
using System.Collections.Generic;

namespace Usher.Platforms.Generic.Devices
{
    public abstract class GenericDevice
    {
        public abstract string Provider { get; }
        public abstract string Instance { get; }
        public virtual string Id { get; protected set; }

        public string Uri => $"{Provider}://{Instance}/{Id}";

        public string Name { get { return Config.Name; } set { Config.Name = value; }}
        public string Location { get { return Config.Location; } set { Config.Location = value; }}
        public string Description { get { return Config.Description; } set { Config.Location = value; }}
        public List<string> Tags => Config.Tags;

        protected Config.Entities.Node Config => Usher.Config.Devices.Instance.NodeFromUri(Uri);
    }
}