namespace Usher.Platforms.ZWave.Proto.Commands
{
    public enum Color : byte
    {
        CapabilityGet = 0x01,
        CapabilityReport = 0x02,
        Get = 0x03,
        Report = 0x04,
        Set = 0x05,

        StartCapabilityLevelChange = 0x06,
        StopStateChange = 0x07
    }
}