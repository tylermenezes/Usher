namespace Usher.Platforms.Generic.Abilities
{
    public delegate void PressedEventHandler(int buttonIndex);
    public interface IPresses
    {
        event PressedEventHandler Pressed;
    }
}