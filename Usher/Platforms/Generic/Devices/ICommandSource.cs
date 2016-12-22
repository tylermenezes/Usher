namespace Usher.Platforms.Generic.Devices
{
    public delegate void CommandReceivedHandler(string command, string[] argv);
    public interface ICommandSource : IDevice
    {
        event CommandReceivedHandler OnCommand;
        void RegisterCommandListener(string command, CommandReceivedHandler onCommand);
    }
}
