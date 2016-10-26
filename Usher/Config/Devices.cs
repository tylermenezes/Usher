using System;
using System.Linq;
using System.Collections.Generic;
using Usher.Config.Entities;
using YamlDotNet.Serialization;
namespace Usher.Config
{
    public class Devices : GenericFile<Devices>
    {
        internal SerializableDevices _devices = new SerializableDevices();
        public List<PlatformInstance> Platforms = new List<PlatformInstance>();

        public PlatformInstance PlatformFromUri(string uri)
        {
            var cUri = new Uri(uri);
            var platform = cUri.Scheme;
            var instance = cUri.Host;

            return Platforms
                    .Where(p => p.Platform == platform)
                    .Where(p => p.Instance == instance)
                    .First();
        }

        public Node NodeFromUri(string uri)
        {
            return PlatformFromUri(uri).Nodes
                    .Where(n => n.Id == (new Uri(uri)).AbsolutePath)
                    .First();
        }

        protected override string serialize()
        {
            _devices.Platforms = Platforms;
            var serializer = (new SerializerBuilder())
                                .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention())
                                .Build();
            return serializer.Serialize(_devices);
        }

        protected override void deserialize(string s)
        {
           var deserializer = (new DeserializerBuilder())
                                .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention())
                                .Build();
           _devices = deserializer.Deserialize<SerializableDevices>(s);
           Platforms = _devices.Platforms;
        }
    }

    internal class SerializableDevices
    {
        public List<PlatformInstance> Platforms { get; set; }
    }
}