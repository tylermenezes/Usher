using System;
using System.Linq;
using System.Threading;
using Usher.Platforms;
using Usher.Plugins.TylerMenezes;
using Usher.Platforms.Generic.Devices;
using Usher.PluginFramework.Base.Residential;
namespace Usher.Plugins.TylerMenezes
{
    public class Plugin : HomePlugin, PluginFramework.IPlugin
    {
        protected Room roomLiving = new Room(
            Supervisor.Instance.Devices.Where(d => d.Location == "livingroom").ToList()
        );
        protected ICommandSource webSource = Supervisor.Instance.Devices
                                                .OfType<ICommandSource>()
                                                .First();

        protected IRemote remoteSource = Supervisor.Instance.Devices
                                                .OfType<IRemote>()
                                                .First();

        protected TimeSpan timeEvening = new TimeSpan(19, 00, 00);
        protected TimeSpan timeNight = new TimeSpan(22, 30, 00);

        public void Main()
        {
            // Set alarms
            wakeUpAt(new TimeSpan(06, 20, 00));
            setAlarm(timeEvening, true, () => {
                var duration = (int)(timeNight - timeEvening).TotalSeconds;
                roomLiving.Sunset(duration);
            }).Start();


            // Register for command listeners
            webSource.RegisterCommandListener("on", (command, argv) => {roomLiving.LightsOn();});
            remoteSource.RegisterButtonPressHandler(1, (button) => {roomLiving.LightsOn();});
            webSource.RegisterCommandListener("off", (command, argv) => {roomLiving.LightsOff();});
            remoteSource.RegisterButtonPressHandler(2, (button) => {roomLiving.LightsOff();});

            webSource.RegisterCommandListener("theater", (command, argv) => {roomLiving.Theater();});
            remoteSource.RegisterButtonPressHandler(3, (button) => {roomLiving.Theater();});
            remoteSource.RegisterButtonPressHandler(4, (button) => {roomLiving.Rainbow();});

            webSource.RegisterCommandListener("sun", (command, argv) => {
                roomLiving.StopRunningScene();
                if (argv.Length < 1) return;
                var isSetting = argv[0] == "set" ? true : false;
                var duration = argv.Length > 2 ? int.Parse(argv[1]) : 600;

                if (isSetting) {
                    roomLiving.Sunset(duration);
                } else {
                    roomLiving.Sunrise(duration);
                }
            });
            
            remoteSource.RegisterButtonPressHandler(5, (button) => {roomLiving.Sunrise(600);});
            remoteSource.RegisterButtonPressHandler(6, (button) => {roomLiving.Sunset(600);});
            remoteSource.RegisterButtonPressHandler(8, (button) => {
                roomLiving.StopRunningScene();
                roomLiving.LightsOn();
            });
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
                roomLiving.Sunrise(15*60);
            });
            wakeUpAlarm.Start();
        }
    }
}