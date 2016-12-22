using System;
using System.Linq;
using System.Threading;
using Usher.Platforms.Generic.Devices;
using Usher.PluginFramework.Base.Residential;
namespace Usher.Plugins.TylerMenezes
{
    public static class RoomScene
    {
        public static void Rainbow(this Room room)
        {
            room.RunScene(() => {
                int r, g, b;
                while (true) {
                    for (double i = 0; i < 360; i += 5 ) {
                        HsvToRgb(i, 1.0, 1.0, out r, out g, out b);
                        room.SetRgb(r, g, b, 0, 0);
                        Thread.Sleep(2000);
                    }
                }
            });
        }

        public static int TempStart = 1200;
        public static int TempTransition = 2300;
        public static int TempEnd = 6000;
        public static decimal DimTransition = 0.3M;
        public static int StepInterval = 2;


        public static void Sunrise(this Room room, int duration)
        {
            room.Unlock();
            room.RunScene(() => {
                room.SunPhaseHorizon(duration/3);
                room.SunPhaseRise(duration/3);
                room.SunPhaseDay(duration/3);
            });
        }

        public static void Sunset(this Room room, int duration)
        {
            room.RunScene(() => {
                room.SunPhaseDay(duration/3, true);
                room.SunPhaseRise(duration/3, true);
                room.SunPhaseHorizon(duration/3, true);
            });
        }
        
        public static void SunPhaseHorizon(this Room room, int duration, bool isSetting = false)
        {
            var start = TempStart;
            var end = TempTransition;
            decimal unitsPerSec = (end-start)/duration;

            TimeInterpolate(duration, TempStart, TempTransition, isSetting, (val, progress) => {
                room.Temperature = (int)val;
                room.Dim = progress;
            });
        }

        public static void SunPhaseRise(this Room room, int duration, bool isSetting = false)
        {
            if (isSetting) {
                room.Devices.OfType<IRgbBulb>().ToList().ForEach(b => {
                    var zb = (Platforms.ZWave.Devices.RgbBulb)b;
                    b.Temperature = TempEnd;
                    zb.SendRequest(Platforms.ZWave.Proto.CommandClass.SwitchMultilevel, 0x01,
                        new byte[]{(byte)(99), 0x01});
                });
                Thread.Sleep(1000);
            }

            TimeInterpolate(duration, TempTransition, TempEnd, isSetting, (val, progress) => {
                room.Temperature = (int)val;
            });
        }

        public static void SunPhaseDay(this Room room, int duration, bool isSetting = false)
        {
            if (!isSetting) {
                room.Devices.OfType<IRgbBulb>().ToList().ForEach(b => {
                    var zb = (Platforms.ZWave.Devices.RgbBulb)b;
                    zb.SendRequest(Platforms.ZWave.Proto.CommandClass.SwitchMultilevel, 0x01,
                        new byte[]{(byte)(DimTransition*99), 0x01});
                    b.SetRgb(0, 0, 0, 255, 0);
                });
            } else {
                room.SetRgb(0, 0, 0, 255, 255);
            }
            Thread.Sleep(1000);

            TimeInterpolate(duration, DimTransition, 1.0M, isSetting, (val, progress) => {
                room.Dim = val;
            });
        }

        public delegate void InterpolationDelegate(decimal val, decimal progress);
        public static void TimeInterpolate(int duration, decimal start, decimal end, bool reverse, InterpolationDelegate step)
        {
            var stepDelta = (end - start)/duration;
            if (reverse){
                var tmp = start;
                start = end;
                end = tmp;
                stepDelta *= -1;
            }

            var val = start;
            for (var sec = 0; sec <= duration; sec+=StepInterval) {
                step(val, ((decimal)(reverse ? (duration-sec) : sec)/duration));
                val += stepDelta*StepInterval;
                Thread.Sleep(StepInterval*1000);
            }
        }
        public static void HsvToRgb(double h, double s, double v, out int r, out int g, out int b)
        {    
            var H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (v <= 0)
                { R = G = B = 0; }
            else if (s <= 0)
            {
                R = G = B = v;
            }
            else
            {
                var hf = H / 60.0;
                var i = (int)Math.Floor(hf);
                var f = hf - i;
                var pv = v * (1 - s);
                var qv = v * (1 - s * f);
                var tv = v * (1 - s * (1 - f));
                switch (i)
                {

                // Red is the dominant color

                case 0:
                    R = v;
                    G = tv;
                    B = pv;
                    break;

                // Green is the dominant color

                case 1:
                    R = qv;
                    G = v;
                    B = pv;
                    break;
                case 2:
                    R = pv;
                    G = v;
                    B = tv;
                    break;

                // Blue is the dominant color

                case 3:
                    R = pv;
                    G = qv;
                    B = v;
                    break;
                case 4:
                    R = tv;
                    G = pv;
                    B = v;
                    break;

                // Red is the dominant color

                case 5:
                    R = v;
                    G = pv;
                    B = qv;
                    break;

                // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                case 6:
                    R = v;
                    G = tv;
                    B = pv;
                    break;
                case -1:
                    R = v;
                    G = pv;
                    B = qv;
                    break;

                // The color is not defined, we should throw an error.

                default:
                    //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                    R = G = B = v; // Just pretend its black/white
                    break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        public static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }
}