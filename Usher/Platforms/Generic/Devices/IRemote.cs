using Usher.Platforms.Generic.Abilities;
namespace Usher.Platforms.Generic.Devices
{
    public delegate void ButtonPressedHandler(int id);
    public interface IRemote : IDevice
    {
        event ButtonPressedHandler OnButtonPress;
        void RegisterButtonPressHandler(int id, ButtonPressedHandler onButtonPress);
    }
}