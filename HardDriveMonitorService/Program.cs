using System.ServiceProcess;

namespace HardDriveMonitorService
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new HardDriveMonitorService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}