using Usher.PluginFramework.Base.Residential;
namespace Usher.Plugins.TylerMenezes
{
    static class RoomGeneric
    {
        public static void LightsOn(this Room room)
        {
            room.StopRunningScene();
            room.SetRgb(0, 0, 0, 255, 0);
            room.Dim = 1.0M;
            room.Unlock();
        }

        public static void LightsOff(this Room room)
        {
            room.StopRunningScene();
            room.SetRgb(0, 0, 0, 255, 0);
            room.Dim = 0M;
            room.Unlock();
        }

        public static void Theater(this Room room)
        {
            room.Lock(0.25M, 0, 0, 255, 0, 0);
        }

        public static void Evening(this Room room)
        {
            var rgb = Utilities.ColorTemperatureToRgb(2500);
            room.Lock(1M, rgb[0], rgb[1], rgb[2], 0, 0);
        }
    }
}