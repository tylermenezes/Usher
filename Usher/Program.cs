namespace Usher{
    public class Program {
        public static void Main(string[] argv)
        {
            Utilities.Logger.Info("Starting up!");
            Platforms.Supervisor.Instance.Start().Wait();
            PluginFramework.Supervisor.Instance.Start();
        }
    }
}