using ZWaveLib;
using System;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.ZWave.Devices
{
    public class RgbBulb : ZWaveEndDevice<RgbBulb>, IRgbBulb
    {
        protected static int[] RequiredClasses = new int[] { 51 };

        private decimal _dim;
        public decimal Dim {
            get { return _dim; }
            set
            {
                _dim = value;
                var level = (int)(value*99);
                SendRequest(ZWave.Proto.CommandClass.SwitchMultilevel,
                            0x01,
                            new byte[] {(byte)level, 0x00}
                );
            }
        }

        private readonly Dictionary<string, float[]> _gamuts = new Dictionary<string, float[]>{
            {"0086-0062", new float[]{1.0F, 0.94F, 0.17F}}
        };
        private int[] _color = new int[] {0,0,0,0,0};
        public void SetRgb(int r, int g, int b, int ww, int cw)
        {
            // Update the cached color value
            _color = new int[]{r,g,b,ww,cw};

            // Get and apply the gamut
            var gamutLookup =
                $"{ZWaveNode.ManufacturerSpecific.ManufacturerId}-{ZWaveNode.ManufacturerSpecific.ProductId}";
            var gamut = new float[]{1.0F, 1.0F, 1.0F};
            if (_gamuts.ContainsKey(gamutLookup)) {
                gamut = _gamuts[gamutLookup];
            }
            r = (int)(r * gamut[0]);
            g = (int)(g * gamut[1]);
            b = (int)(b * gamut[2]);

            SendRequest(ZWave.Proto.CommandClass.Color,
                        (byte)ZWave.Proto.Commands.Color.Set,
                        new byte[]{
                            0x05, // Number of colors being sent, in this case Red, Green, Blue, Warm-White, and Cool-White
                            0x02, (byte)r,
                            0x03, (byte)g,
                            0x04, (byte)b,
                            0x00, (byte)ww,
                            0x01, (byte)cw
                        });
        }

#region "Accessing Getters/Setters"
        public bool IsOn
        {
            get
            {
                return Dim > 0;
            }
            set
            {
                Dim = (value ? 1 : 0);
            }
        }

        public int Temperature
        {
            set
            {
                var rgb = Usher.Utilities.ColorTemperatureToRgb(value);
                _color = new int[]{rgb[0], rgb[1], rgb[2], 0, 0};
                SetRgb();
            }
        }

        public int R
        {
            get
            {
                return _color[1];
            }
            set
            {
                _color[1] = value;
                SetRgb();
            }
        }
        public int G
        {
            get
            {
                return _color[2];
            }
            set
            {
                _color[2] = value;
                SetRgb();
            }
        }
        public int B
        {
            get
            {
                return _color[3];
            }
            set
            {
                _color[3] = value;
                SetRgb();
            }
        }
        public int Ww
        {
            get
            {
                return _color[4];
            }
            set
            {
                _color[4] = value;
                SetRgb();
            }
        }
        public int Cw
        {
            get
            {
                return _color[5];
            }
            set
            {
                _color[5] = value;
                SetRgb();
            }
        }

        private void SetRgb()
        {
            SetRgb(_color[0], _color[1], _color[2], _color[3], _color[4]);
        }

        public void SetRgb(int r, int g, int b)
        {
            SetRgb(r, g, b, 0, 0);
        }
#endregion

    }
}