using System;
using NLog;
using NLog.Targets;
using NLog.Config;
namespace Usher
{
    static class Utilities
    {
        public static Logger Logger;

        static Utilities()
        {
            // Set up logging
            var logConfig = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget();
            consoleTarget.Name = "Console";
            consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
            consoleTarget.DetectConsoleAvailable = false;
            logConfig.LoggingRules.Add(new LoggingRule("Usher", LogLevel.Trace, consoleTarget));
            logConfig.LoggingRules.Add(new LoggingRule("ZWaveLib.*", LogLevel.Error, consoleTarget));

            logConfig.AddTarget(consoleTarget);
            
            LogManager.Configuration = logConfig;
            Logger = LogManager.GetLogger("Usher");
        }

        public static int[] ColorTemperatureToRgb(int temperature)
        {
            var fTemp = ((float)temperature)/100;
        
            double r = 255;
            double g = 255;
            double b = 255;
    
            // Red 
            if (fTemp > 66) {
                r = fTemp-60;
                r = 329.698727446 * Math.Pow(r,-0.1332047592);
            }
    
            // Green 
            if (fTemp <= 66) {
                g = fTemp;
                g = 99.4708025861 * Math.Log(g) - 161.1195681661;
            } else {
                g = fTemp - 60; 
                g = 288.1221695283 * Math.Pow(g,-0.0755148492);
            }
    
            // Blue 
            if (fTemp <= 19) {
                b = 0;
            } else if (fTemp < 66) {
                b = fTemp - 10;
                b = 138.5177312231 * Math.Log(b) - 305.0447927307;
            }

            r = Math.Max(0, Math.Min(r, 255));
            g = Math.Max(0, Math.Min(g, 255));
            b = Math.Max(0, Math.Min(b, 255));
    
            return new int[]{(int)r, (int)g, (int)b};
        }
    }
}