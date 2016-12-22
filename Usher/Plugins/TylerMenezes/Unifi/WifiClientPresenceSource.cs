using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;

namespace Usher.Plugins.TylerMenezes.Unifi
{
    public class WifiClientPresenceSource : GenericDevice, IPresenceSource
    {
        public override string Provider => "unifi";
        public override string Instance => _manager.Instance;
        public override string Id => "source";

        private readonly Manager _manager;
        public WifiClientPresenceSource(Manager manager)
        {
            this._manager = manager;
        }

        public event PresenceEnteredHandler OnEnter;
        public event PresenceLeftHandler OnLeave;
        public event PresenceChangedHandler OnChange;

        public void DispatchEnter(string id)
        {
            Utilities.Logger.Info($"Client {id} joined {Uri}");
            OnEnter?.Invoke(id, _manager.Clients.ToArray());
        }

        public void DispatchLeave(string id)
        {
            Utilities.Logger.Info($"Client {id} left {Uri}");
            OnLeave?.Invoke(id, _manager.Clients.ToArray());
        }

        public void DispatchUpdate()
        {
            OnChange?.Invoke(_manager.Clients.ToArray());
        }
    }
}
