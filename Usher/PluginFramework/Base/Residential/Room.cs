using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.PluginFramework.Base.Residential
{
    public class Room
    {
        public IEnumerable<IDevice> Devices;
        public Room(IEnumerable<IDevice> roomDevices)
        {
            Devices = roomDevices;
        }

        protected Thread runningScene;
        public void StopRunningScene()
        {
            if (runningScene != null && runningScene.IsAlive) {
                runningScene.Abort();
            }
        }

        public delegate void sceneDelegate();
        public void RunScene(sceneDelegate toRun)
        {
            StopRunningScene();
            runningScene = (new Thread(() => {
                toRun();
            }));
            runningScene.Start();
        }

        protected int[] desiredRgbw = new int[]{0, 0, 0, 255, 0};
        public void SetRgb(int r, int g, int b, int ww, int cw)
        {
            desiredRgbw = new int[]{r, g, b, ww, cw};
            if (!isLocked) {
                Devices
                    .OfType<IRgbBulb>()
                    .ToList()
                    .ForEach(bulb => bulb.SetRgb(r, g, b, ww, cw));
            }
        }

        protected decimal desiredBrightness = 0.0M;
        public decimal Dim
        {
            get
            {
                return desiredBrightness;
            }
            set
            {
                desiredBrightness = value;
                if (!isLocked) {
                    Devices
                        .OfType<IDimmableBulb>()
                        .ToList()
                        .ForEach(bulb => bulb.Dim = value);
                }
            }
        }

        protected bool isLocked = false;
        public void Lock(decimal brightness, int r, int g, int b, int ww, int cw)
        {
            isLocked = true;
            Devices
                .OfType<IDimmableBulb>()
                .ToList()
                .ForEach(bulb => bulb.Dim = brightness);
            Devices
                .OfType<IRgbBulb>()
                .ToList()
                .ForEach(bulb => bulb.SetRgb(r, g, b, ww, cw));
        }

        public void Unlock()
        {
            isLocked = false;
            SetRgb();
            Dim = Dim;
        }

        public void restoreBeforeForce()
        {
            SetRgb();
            Dim = Dim;
        }

#region "Accessing Getters/Setters"
        public int R
        {
            get
            {
                return desiredRgbw[1];
            }
            set
            {
                desiredRgbw[1] = value;
                SetRgb();
            }
        }
        public int G
        {
            get
            {
                return desiredRgbw[2];
            }
            set
            {
                desiredRgbw[2] = value;
                SetRgb();
            }
        }
        public int B
        {
            get
            {
                return desiredRgbw[3];
            }
            set
            {
                desiredRgbw[3] = value;
                SetRgb();
            }
        }
        public int WW
        {
            get
            {
                return desiredRgbw[4];
            }
            set
            {
                desiredRgbw[4] = value;
                SetRgb();
            }
        }
        public int CW
        {
            get
            {
                return desiredRgbw[5];
            }
            set
            {
                desiredRgbw[5] = value;
                SetRgb();
            }
        }

        public bool IsOn
        {
            get
            {
                return Dim > 0M;
            }
            set
            {
                Dim = value ? 1M : 0M;
            }
        }

        
        public void SetRgb(int r, int g, int b)
        {
            SetRgb(r, g, b, 0, 0);
        }

        public void SetRgb()
        {
            SetRgb(desiredRgbw[0], desiredRgbw[1], desiredRgbw[2], desiredRgbw[3], desiredRgbw[4]);
        }

        public int Temperature
        {
            set
            {
                int[] rgb = Usher.Utilities.ColorTemperatureToRgb(value);
                desiredRgbw = new int[]{rgb[0], rgb[1], rgb[2], 0, 0};
                SetRgb();
            }
        }
#endregion
    }
}