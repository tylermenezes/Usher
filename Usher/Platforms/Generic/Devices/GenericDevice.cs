using System;
using System.Collections.Generic;

namespace Usher.Platforms.Generic.Devices
{
    public abstract class GenericDevice
    {
        abstract public string Provider { get; }
        abstract public string Instance { get; }
        virtual public string Id { get; protected set; }

        public string Uri
        {
            get
            {
                return String.Format("{0}://{1}/{2}", Provider, Instance, Id);
            }
        }

        public string Name { get { return config.Name; } set { config.Name = value; }}
        public string Location { get { return config.Location; } set { config.Location = value; }}
        public string Description { get { return config.Description; } set { config.Location = value; }}
        public List<string> Tags { get { return config.Tags; } }

        protected Config.Entities.Node config
        {
            get
            {
                return Config.Devices.Instance.NodeFromUri(Uri);
            }
        }
    }
}