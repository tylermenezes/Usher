using System;
using System.Linq;
using System.Collections.Generic;
using Usher.Config.Entities;
using YamlDotNet.Serialization;
namespace Usher.Config
{
    public class Devices : GenericFile<Devices>
    {
        public List<PlatformInstance> Platforms = new List<PlatformInstance>();

        public PlatformInstance PlatformFromUri(string uri)
        {
            var cUri = new Uri(uri);
            var platform = cUri.Scheme;
            var instance = cUri.Host;

            return Platforms
                    .Where(p => p.Platform.ToLower() == platform.ToLower())
                    .First(p => p.Instance.ToLower() == instance.ToLower());
        }

        public Node NodeFromUri(string uri)
        {
            var pf = PlatformFromUri(uri);
            if (pf.Nodes == null) pf.Nodes = new List<Node>();
            var nodes = pf.Nodes
                    .Where(n => n.Id == (new Uri(uri)).AbsolutePath.Substring(1))
                    .ToList();

            Node node;
            if (nodes.Any()) {
                node = nodes.First();
            } else {
                node = new Node {Id = (new Uri(uri)).AbsolutePath.Substring(1)};
                pf.Nodes.Add(node);
            }
            if (node.Tags == null) node.Tags = new List<string>();

            return node;
        }

        protected override string Serialize()
        {
            var sObject = new SerializableDevices{
                Platforms = this.Platforms
            };
            var serializer = (new SerializerBuilder())
                                .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention())
                                .Build();
            return serializer.Serialize(sObject);
        }

        protected override void Deserialize(string s)
        {
           var deserializer = (new DeserializerBuilder())
                                .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.HyphenatedNamingConvention())
                                .Build();
           var newDevices = deserializer.Deserialize<SerializableDevices>(s);
           if (newDevices != null) Platforms = newDevices.Platforms;
        }
    }

    internal class SerializableDevices
    {
        public List<PlatformInstance> Platforms { get; set; }
    }
}