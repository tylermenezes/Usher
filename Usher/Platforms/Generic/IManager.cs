using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.Generic
{
    public delegate void ManagerErrorHandler(IManager sender);
    public delegate void ManagerReadyHandler(IManager sender);
    public delegate void ManagerStoppedHandler(IManager sender);

    public interface IManager
    {
        string Provider { get; }
        string Instance { get; }
        string Uri { get; }

        IEnumerable<IDevice> Devices
        {
            get;
        }

        Task Start();
        Task Stop();

        event ManagerErrorHandler OnError;
        event ManagerReadyHandler OnReady;
        event ManagerStoppedHandler OnStop;
    }
}