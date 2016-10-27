using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Usher.Platforms;
using Usher.Platforms.Generic.Devices;
using Usher.PluginFramework.Base.Residential;
namespace Usher.Plugins
{
    public class TylerMenezesPlugin : HomePlugin, PluginFramework.IPlugin
    {
        protected Room roomLiving = new Room(
            Supervisor.Instance.Devices.Where(d => d.Location == "livingroom").ToList()
        );
        protected ICommandSource webSource = Supervisor.Instance.Devices
                                                .OfType<ICommandSource>()
                                                .First();

        protected TimeSpan timeEvening = new TimeSpan(19, 00, 00);
        protected TimeSpan timeNight = new TimeSpan(22, 30, 00);

        public void Main()
        {
            // Turn on lights (until switch arrives)
            roomLiving.SetRgb(0, 0, 0, 255, 0);
            roomLiving.Dim = 1M;

            wakeUpAt(new TimeSpan(06, 20, 00));
            setAlarm(timeEvening, true, () => {
                var duration = (int)(timeNight - timeEvening).TotalSeconds;
                sunset(roomLiving, duration);
            }).Start();

            // Register for command listeners
            webSource.RegisterCommandListener("theater", (command, argv) => {
                roomLiving.StopRunningScene();
                roomLiving.SetRgb(0, 0, 255);
                roomLiving.Dim = 0.1M;
            });

            webSource.RegisterCommandListener("on", (command, argv) => {
                roomLiving.StopRunningScene();
                roomLiving.SetRgb(0, 0, 0, 255, 0);
                roomLiving.Dim = 1M;
            });

            webSource.RegisterCommandListener("off", (command, argv) => {
                roomLiving.StopRunningScene();
                roomLiving.SetRgb(0, 0, 0, 255, 0);
                roomLiving.Dim = 0M;
            });

            webSource.RegisterCommandListener("sun", (command, argv) => {
                roomLiving.StopRunningScene();
                if (argv.Length < 1) return;
                var isSetting = argv[0] == "set" ? true : false;
                var duration = argv.Length > 2 ? int.Parse(argv[1]) : 300;

                if (isSetting) {
                    sunset(roomLiving, duration);
                } else {
                    sunrise(roomLiving, duration);
                }
            });
        }

        protected int tempStart = 1200;
        protected int tempTransition = 2300;
        protected int tempEnd = 6000;
        protected decimal dimTransition = 0.3M;
        protected int stepInterval = 2;


        protected void sunrise(Room room, int duration)
        {
            room.RunScene(() => {
                sunPhaseHorizon(room, duration/3);
                sunPhaseRise(room, duration/3);
                sunPhaseDay(room, duration/3);
            });
        }

        protected void sunset(Room room, int duration)
        {
            room.RunScene(() => {
                sunPhaseDay(room, duration/3, true);
                sunPhaseRise(room, duration/3, true);
                sunPhaseHorizon(room, duration/3, true);
            });
        }
        
        protected void sunPhaseHorizon(Room room, int duration, bool isSetting = false)
        {
            var start = tempStart;
            var end = tempTransition;
            decimal unitsPerSec = (end-start)/duration;

            timeInterpolate(duration, tempStart, tempTransition, isSetting, (val, progress) => {
                room.Temperature = (int)val;
                room.Dim = progress;
            });
        }

        protected void sunPhaseRise(Room room, int duration, bool isSetting = false)
        {
            if (isSetting) {
                room.Devices.OfType<IRgbBulb>().ToList().ForEach(b => {
                    var zb = (Platforms.ZWave.Devices.RgbBulb)b;
                    b.Temperature = tempEnd;
                    zb.SendRequest(Platforms.ZWave.Proto.CommandClass.SwitchMultilevel, 0x01,
                        new byte[]{(byte)(99), 0x01});
                });
                Thread.Sleep(1000);
            }

            timeInterpolate(duration, tempTransition, tempEnd, isSetting, (val, progress) => {
                room.Temperature = (int)val;
            });
        }

        protected void sunPhaseDay(Room room, int duration, bool isSetting = false)
        {
            if (!isSetting) {
                room.Devices.OfType<IRgbBulb>().ToList().ForEach(b => {
                    var zb = (Platforms.ZWave.Devices.RgbBulb)b;
                    zb.SendRequest(Platforms.ZWave.Proto.CommandClass.SwitchMultilevel, 0x01,
                        new byte[]{(byte)(dimTransition*99), 0x01});
                    b.SetRgb(0, 0, 0, 255, 0);
                });
            } else {
                room.SetRgb(0, 0, 0, 255, 255);
            }
            Thread.Sleep(1000);

            timeInterpolate(duration, dimTransition, 1.0M, isSetting, (val, progress) => {
                room.Dim = val;
            });
        }

        protected delegate void interpolationDelegate(decimal val, decimal progress);
        protected void timeInterpolate(int duration, decimal start, decimal end, bool reverse, interpolationDelegate step)
        {
            var stepDelta = (end - start)/duration;
            if (reverse){
                var tmp = start;
                start = end;
                end = tmp;
                stepDelta *= -1;
            }

            decimal val = start;
            for (int sec = 0; sec <= duration; sec+=stepInterval) {
                step(val, ((decimal)(reverse ? (duration-sec) : sec)/duration));
                val += stepDelta*stepInterval;
                Thread.Sleep(stepInterval*1000);
            }
        }


        protected Thread wakeUpAlarm;
        protected TimeSpan wakeUpTime;
        protected void wakeUpAt(TimeSpan at)
        {
            // Don't kill current thread if the new wakeup time hasn't really changed.
            if (Math.Abs((wakeUpTime - at).TotalSeconds) < 60) {
                return;
            }

            // Kill the current alarm thread.
            if (wakeUpAlarm != null && wakeUpAlarm.IsAlive) {
                wakeUpAlarm.Abort();
            }

            // Set/start the new alarm thread.
            wakeUpAlarm = setAlarm(at, true, () => {
                sunrise(roomLiving, 1*60);
            });
            wakeUpAlarm.Start();
        }

    }
}