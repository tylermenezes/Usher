using Usher.Platforms.Generic.Abilities;

namespace Usher.Platforms.Generic.Devices
{
    public interface IRgbBulb : IRgb, IWhiteBalance, IDimmableBulb, ISimpleBulb, IDevice
    {
        int Ww { get; set; }
        int Cw { get; set; }
        void SetRgb(int r, int g, int b, int ww, int cw);
    }
}