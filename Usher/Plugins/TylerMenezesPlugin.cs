using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Usher.Platforms;
using Usher.Platforms.Generic.Devices;
namespace Usher.Plugins
{
    public class TylerMenezesPlugin : PluginFramework.IPlugin
    {
        protected List<IRgbBulb> livingRoomBulbs = Supervisor.Instance.Devices
                                                    .OfType<IRgbBulb>()
                                                   .ToList();
        public void Main()
        {
            // On startup, turn off lights
            livingRoomBulbs.ForEach(b => b.SetRgb(0, 0, 0, 255, 0));
            livingRoomBulbs.ForEach(b => b.IsOn = false);

            // WAKE THE FUCK UP
            alarm(new TimeSpan(06, 30, 00), true, () => {
                sunrise(10*60*1000);
            }).Start();

            // Turn off lights during the day (until we have a lightswitch)
            alarm(new TimeSpan(09, 00, 00), true, () => {
                livingRoomBulbs.ForEach(b => b.IsOn = false);
            }).Start();

            // Turn on lights at night (until we have a lightswitch)
            alarm(new TimeSpan(16, 30, 00), true, () => {
                livingRoomBulbs.ForEach(b => b.IsOn = true);
            }).Start();

            // GO THE FUCK TO SLEEP
            alarm(new TimeSpan(22, 00, 00), true, () => {
                sunset(60*60*1000);
            }).Start();
        }

        protected delegate void alarmActivatedDelegate();
        protected Thread alarm(TimeSpan time, bool includeWeekends, alarmActivatedDelegate onAlarm)
        {
            return new Thread(() => {
                while(true) {
                    DateTime nextEndTime = DateTime.Today.Add(time);
                    if (nextEndTime < DateTime.Now) {
                        nextEndTime = nextEndTime.AddDays(1);
                    }

                    if (!includeWeekends) {
                        while (nextEndTime.DayOfWeek == DayOfWeek.Saturday
                                || nextEndTime.DayOfWeek == DayOfWeek.Sunday) {
                            nextEndTime = nextEndTime.AddDays(1);
                        }
                    }

                    TimeSpan delta = nextEndTime - DateTime.Now;

                    if (delta.TotalHours > 1) {
                        Thread.Sleep(45*60*1000);
                    } else if (delta.TotalMinutes > 30) {
                        Thread.Sleep(15*60*1000);
                    } else if (delta.TotalMinutes > 2) {
                        Thread.Sleep(60*1000);
                    } else if (delta.TotalSeconds > 2) {
                        Thread.Sleep(1000);
                    } else {
                        onAlarm();
                        Thread.Sleep(60*60*1000);
                    }
                }
            });
        }

        protected int tempStart = 1200;
        protected int tempTransition = 2300;
        protected int tempEnd = 6000;
        protected decimal dimTransition = 0.3M;

        protected void sunrise(int duration)
        {
            sunPhaseHorizon(duration/3);
            sunPhaseRise(duration/3);
            sunPhaseDay(duration/3);
        }

        protected void sunset(int duration)
        {
            sunPhaseDay(duration/3, true);
            sunPhaseRise(duration/3, true);
            sunPhaseHorizon(duration/3, true);
        }
        
        protected void sunPhaseHorizon(int duration, bool isSetting = false)
        {
            var start = tempStart;
            var end = tempTransition;
            decimal unitsPerSec = (end-start)/duration;

            timeInterpolate(duration, tempStart, tempTransition, isSetting, (val, progress) => {
                livingRoomBulbs.ForEach(b => {
                    b.Temperature = (int)val;
                    b.Dim = progress;
                });
            });
        }

        protected void sunPhaseRise(int duration, bool isSetting = false)
        {
            if (isSetting) {
                livingRoomBulbs.ForEach(b => {
                    var zb = (Platforms.ZWave.Devices.RgbBulb)b;
                    b.Temperature = tempEnd;
                    zb.SendRequest(Platforms.ZWave.Proto.CommandClass.SwitchMultilevel, 0x01,
                        new byte[]{(byte)(99), 0x01});
                });
                Thread.Sleep(1000);
            }

            timeInterpolate(duration, tempTransition, tempEnd, isSetting, (val, progress) => {
                livingRoomBulbs.ForEach(b => b.Temperature = (int)val);
            });
        }

        protected void sunPhaseDay(int duration, bool isSetting = false)
        {
            if (!isSetting) {
                livingRoomBulbs.ForEach(b => {
                    var zb = (Platforms.ZWave.Devices.RgbBulb)b;
                    zb.SendRequest(Platforms.ZWave.Proto.CommandClass.SwitchMultilevel, 0x01,
                        new byte[]{(byte)(dimTransition*99), 0x01});
                    b.SetRgb(0, 0, 0, 255, 0);
                });
            } else {
                livingRoomBulbs.ForEach(b => b.SetRgb(0, 0, 0, 255, 255));
            }
            Thread.Sleep(1000);

            timeInterpolate(duration, dimTransition, 1.0M, isSetting, (val, progress) => {
                livingRoomBulbs.ForEach(b => b.Dim = val);
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
            for (int sec = 0; sec < duration; sec++) {
                step(val, ((decimal)(reverse ? (duration-sec) : sec)/duration));
                val += stepDelta;
                Thread.Sleep(1000);
            }
        }
    }
}