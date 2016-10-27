using System;
using System.Threading;
namespace Usher.PluginFramework.Base.Residential
{
    public abstract class HomePlugin
    {
        protected delegate void alarmActivatedDelegate();
        protected Thread setAlarm(TimeSpan time, bool includeWeekends, alarmActivatedDelegate onAlarm)
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
                    Utilities.Logger.Debug("Alarm handler woke up with {0} left.", delta);

                    if (delta.TotalSeconds > 5) {
                        Thread.Sleep((int)delta.TotalMilliseconds-2000); // n.b. Thread.Sleep is fairly inaccurate.
                    } else {
                        onAlarm();
                        Thread.Sleep(60*60*1000);
                    }
                }
            });
        }

    }
}