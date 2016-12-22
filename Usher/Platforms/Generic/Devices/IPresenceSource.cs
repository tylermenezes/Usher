namespace Usher.Platforms.Generic.Devices
{
    public delegate void PresenceEnteredHandler(string id, string[] presentIds);
    public delegate void PresenceLeftHandler(string id, string[] presentIds);
    public delegate void PresenceChangedHandler(string[] presentIds);

    public interface IPresenceSource : IDevice
    {
        event PresenceEnteredHandler OnEnter;
        event PresenceLeftHandler OnLeave;
        event PresenceChangedHandler OnChange;
    }
}
