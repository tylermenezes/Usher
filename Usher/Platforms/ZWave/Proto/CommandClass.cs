namespace Usher.Platforms.ZWave.Proto
{
    public enum CommandClass : byte
    {
        Switch = 0x25,
        SwitchMultilevel = 0x26,
        Color = 0x33,

        SensorBinary = 0x30,
        SensorMultilevel = 0x31,
        SensorMeter = 0x32
    }
}