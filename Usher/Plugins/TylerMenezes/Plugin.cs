using System;
using System.Linq;
using System.Threading;
using Usher.Platforms;
using Usher.Platforms.Generic.Devices;
using Usher.PluginFramework.Base.Residential;
namespace Usher.Plugins.TylerMenezes
{
    public class Plugin : HomePlugin, PluginFramework.IPlugin
    {
        protected Room RoomLiving;
        protected ICommandSource WebSource;
        protected IRemote RemoteSource;
        protected IPresenceSource UnifiSource;

        protected TimeSpan TimeEvening = new TimeSpan(21, 00, 00);
        protected TimeSpan TimeNight = new TimeSpan(23, 30, 00);

        public void Main()
        {
            UnifiSource = Supervisor.Instance.Devices
                            .OfType<IPresenceSource>()
                            .First();

            RoomLiving = new Room(
                Supervisor.Instance.Devices.Where(d => d.Location == "livingroom").ToList()
            );

            WebSource = Supervisor.Instance.Devices
                    .OfType<ICommandSource>()
                    .First();
            RemoteSource = Supervisor.Instance.Devices
                    .OfType<IRemote>()
                    .First();

            // Set alarms
            WakeUpAt(new TimeSpan(07, 00, 00));
            SetAlarm(TimeEvening, true, () =>
            {
                var duration = (int)(TimeNight - TimeEvening).TotalSeconds;
                RoomLiving.Sunset(duration);
            }).Start();

            // Set presence detector
            // TODO: Tied to one specific device. Should load multiple from config and detect presence from any present.
            //       (fix this before I start dating anyone seriously or let friends stay over or they will get annoyed)
            UnifiSource.OnLeave += (string id, string[] all) =>
            {
                if (id == "58:f1:02:0f:46:3c")
                {
                    RoomLiving.Lock(0, 0, 0, 0, 255, 0);
                }
            };

            UnifiSource.OnEnter += (string id, string[] all) =>
            {
                if (id == "58:f1:02:0f:46:3c")
                {
                    RoomLiving.Unlock();
                }
            };


            // Register for command listeners
            WebSource.RegisterCommandListener("on", (command, argv) => {RoomLiving.LightsOn();});
            RemoteSource.RegisterButtonPressHandler(1, (button) => {RoomLiving.LightsOn();});
            WebSource.RegisterCommandListener("off", (command, argv) => {RoomLiving.LightsOff();});
            RemoteSource.RegisterButtonPressHandler(2, (button) => {RoomLiving.LightsOff();});

            WebSource.RegisterCommandListener("theater", (command, argv) => {RoomLiving.Theater();});
            RemoteSource.RegisterButtonPressHandler(3, (button) => {RoomLiving.Theater();});
            RemoteSource.RegisterButtonPressHandler(4, (button) => {RoomLiving.Evening();});

            WebSource.RegisterCommandListener("sun", (command, argv) =>
            {
                RoomLiving.StopRunningScene();
                if (argv.Length < 1) return;
                var isSetting = argv[0] == "set";
                var duration = argv.Length > 2 ? int.Parse(argv[1]) : 600;

                if (isSetting) {
                    RoomLiving.Sunset(duration);
                } else {
                    RoomLiving.Sunrise(duration);
                }
            });
            
            RemoteSource.RegisterButtonPressHandler(5, (button) => {RoomLiving.Sunrise(600);});
            RemoteSource.RegisterButtonPressHandler(6, (button) => {RoomLiving.Sunset(600);});
            RemoteSource.RegisterButtonPressHandler(7, (button) =>
            {
                RoomLiving.Unlock();
            });
            RemoteSource.RegisterButtonPressHandler(8, (button) =>
            {
                RoomLiving.Lock(0, 0, 0, 0, 255, 0);
            });
        }

        protected Thread WakeUpAlarm;
        protected TimeSpan WakeUpTime;
        protected void WakeUpAt(TimeSpan at)
        {
            // Don't kill current thread if the new wakeup time hasn't really changed.
            if (Math.Abs((WakeUpTime - at).TotalSeconds) < 60) {
                return;
            }

            // Kill the current alarm thread.
            if (WakeUpAlarm != null && WakeUpAlarm.IsAlive) {
                WakeUpAlarm.Abort();
            }

            // Set/start the new alarm thread.
            WakeUpAlarm = SetAlarm(at, true, () => {
                RoomLiving.Sunrise(15*60);
            });
            WakeUpAlarm.Start();
        }
    }
}