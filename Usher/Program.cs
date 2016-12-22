using System.Threading;
namespace Usher{
    public class Program {
        public static void Main(string[] argv)
        {
            Utilities.Logger.Info("Starting up!");
            Platforms.Supervisor.Instance.Start().Wait();
            Thread.Sleep(15000);
            PluginFramework.Supervisor.Instance.Start();
            Config.Devices.Instance.Save();
        }
    }
}