using System.Collections.Generic;

namespace Usher.Platforms.Generic.Devices
{
    public interface IDevice
    {
        string Provider { get; }
        string Instance { get; }
        string Id { get; }
        string Uri { get; }

        string Name { get; set; }
        string Description { get; set; }
        string Location { get;set; }

        List<string> Tags { get; }
    }
}