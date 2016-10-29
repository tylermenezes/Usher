using Usher.PluginFramework.Base.Residential;
namespace Usher.Plugins.TylerMenezes
{
    static class RoomGeneric
    {
        public static void LightsOn(this Room room)
        {
            room.Lock(1, 0, 0, 0, 255, 0);
        }

        public static void LightsOff(this Room room)
        {
            room.Lock(0, 0, 0, 0, 255, 0);
        }

        public static void Theater(this Room room)
        {
            room.Lock(0.25M, 0, 0, 255, 0, 0);
        }
    }
}