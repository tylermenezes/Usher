using Usher.Platforms.Generic.Abilities;

namespace Usher.Platforms.Generic.Devices
{
    public interface IRgbBulb : IRgb, IWhiteBalance, IDimmableBulb, ISimpleBulb, IDevice
    {
        int WW { get; set; }
        int CW { get; set; }
        void SetRgb(int r, int g, int b, int ww, int cw);
    }
}