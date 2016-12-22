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

        protected Thread RunningScene;
        public void StopRunningScene()
        {
            if (RunningScene != null && RunningScene.IsAlive) {
                RunningScene.Abort();
            }
        }

        public delegate void SceneDelegate();
        public void RunScene(SceneDelegate toRun)
        {
            StopRunningScene();
            RunningScene = (new Thread(() => {
                toRun();
            }));
            RunningScene.Start();
        }

        protected int[] DesiredRgbw = new int[]{0, 0, 0, 255, 0};
        public void SetRgb(int r, int g, int b, int ww, int cw)
        {
            DesiredRgbw = new int[]{r, g, b, ww, cw};
            if (!IsLocked) {
                Devices
                    .OfType<IRgbBulb>()
                    .ToList()
                    .ForEach(bulb => bulb.SetRgb(r, g, b, ww, cw));
            }
        }

        protected decimal DesiredBrightness = 0.0M;
        public decimal Dim
        {
            get
            {
                return DesiredBrightness;
            }
            set
            {
                DesiredBrightness = value;
                if (!IsLocked) {
                    Devices
                        .OfType<IDimmableBulb>()
                        .ToList()
                        .ForEach(bulb => bulb.Dim = value);
                }
            }
        }

        public bool IsLocked { get; protected set; }
        public void Lock(decimal brightness, int r, int g, int b, int ww, int cw)
        {
            IsLocked = true;
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
            IsLocked = false;
            SetRgb();
            Dim = Dim;
        }

        public void RestoreBeforeForce()
        {
            SetRgb();
            Dim = Dim;
        }

#region "Accessing Getters/Setters"
        public int R
        {
            get
            {
                return DesiredRgbw[1];
            }
            set
            {
                DesiredRgbw[1] = value;
                SetRgb();
            }
        }
        public int G
        {
            get
            {
                return DesiredRgbw[2];
            }
            set
            {
                DesiredRgbw[2] = value;
                SetRgb();
            }
        }
        public int B
        {
            get
            {
                return DesiredRgbw[3];
            }
            set
            {
                DesiredRgbw[3] = value;
                SetRgb();
            }
        }
        public int Ww
        {
            get
            {
                return DesiredRgbw[4];
            }
            set
            {
                DesiredRgbw[4] = value;
                SetRgb();
            }
        }
        public int Cw
        {
            get
            {
                return DesiredRgbw[5];
            }
            set
            {
                DesiredRgbw[5] = value;
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
            SetRgb(DesiredRgbw[0], DesiredRgbw[1], DesiredRgbw[2], DesiredRgbw[3], DesiredRgbw[4]);
        }

        public int Temperature
        {
            set
            {
                var rgb = Usher.Utilities.ColorTemperatureToRgb(value);
                DesiredRgbw = new int[]{rgb[0], rgb[1], rgb[2], 0, 0};
                SetRgb();
            }
        }
#endregion
    }
}