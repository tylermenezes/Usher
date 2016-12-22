namespace Usher.Platforms.Generic.Abilities
{
    public delegate void ToggledEventHandler(bool isOn);
    public interface IToggles
    {
        event ToggledEventHandler Toggled;
    }
}