using System.Threading.Tasks;
using System.Collections.Generic;
namespace Usher.Platforms.Generic.Devices
{
    public delegate void GatewayDisconnectedHandler(IGateway sender);
    public delegate void GatewayErrorHandler(IGateway sender);
    public delegate void GatewayReadyHandler(IGateway sender);

    public interface IGateway : IDevice
    {
        Task Start();
        Task Stop();

        IEnumerable<IDevice> Devices { get; }

        event GatewayDisconnectedHandler OnDisconnected;
        event GatewayErrorHandler OnError;
        event GatewayReadyHandler OnReady;
    }
}