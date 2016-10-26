namespace Usher.Platforms.Generic.Abilities
{
    public delegate void ToggledEventHandler(bool IsOn);
    public interface IToggles
    {
        event ToggledEventHandler Toggled;
    }
}