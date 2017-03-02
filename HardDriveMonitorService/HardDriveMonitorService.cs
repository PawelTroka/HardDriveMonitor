using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceProcess;
using HardDriveMonitorService.Properties;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

/*
 * Usluga systemowa monitorujaca dysk
 
 * Wymagania:
     * Usluga systemowa (serwis) powinna monitorowac liste katalogow (z ew. podkatalogami) pod katem wybranych typów zmian.
     * Usludze powinno towarzyszyc GUI (oddzielny proces) w WPF pozwalajacy na zmine parametrów dzialania uslugi.
     * Usluga powinna logowac aktywnosc na monitorowanym dysku przy wykorzystaniu Log4Net lub podobnego rozwiazania
     * wymagane sa rozne poziomy szczegolowosci logowania (debug/info) 
     * Wymagana mozliwosc instalacji/deinstalacji uslugi

 * Termin oddawania: koniec semestru.
 */

namespace HardDriveMonitorService
{
    public partial class HardDriveMonitorService : ServiceBase
    {
        public const string Name = "HardDriveMonitorService";
        private readonly FileSystemWatcher configChangeWatcher;

        private bool _isDebug = true;
        private List<FileSystemWatcherExtended> systemWatchers;

        public HardDriveMonitorService()
        {
            InitializeComponent();
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            XmlConfigurator.Configure();


            Log.Info("HardDriveMonitorService is being create");
            systemWatchers = LoadFileSystemWatchersExtended(Log);


            configChangeWatcher = new FileSystemWatcher();
            configChangeWatcher.EnableRaisingEvents = false;
            configChangeWatcher.Changed += configChangeWatcher_Changed;


            ServiceController ctl =
                ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "HardDriveMonitorService");
            if (ctl == null)
            {
                _canStart = _canStop = false;
                IsInstalled = false;
            }
            else
            {
                IsInstalled = true;
                setupConfigChangeWatcher();

                if (ctl.Status == ServiceControllerStatus.Running)
                {
                    _canStart = false;
                    _canStop = true;
                }
                else
                {
                    _canStart = true;
                    _canStop = false;
                }
            }

            IsDebug = Settings.Default.IsDebug;

            Settings.Default.something = "chugo";
            Settings.Default.Save();
        }


        private bool _canStart, _canStop;

        public bool CanStart
        {
            get
            {
                ServiceController ctl =
    ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "HardDriveMonitorService");
                if (ctl == null) return false;

               // return ctl.Status == ServiceControllerStatus.Stopped ||
                      // ctl.Status == ServiceControllerStatus.Paused;
                return _canStart;
            }
        }

        public bool CanStop
        {
            get
            {
                ServiceController ctl =
ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "HardDriveMonitorService");
                if (ctl == null) return false;

                //return ctl.Status == ServiceControllerStatus.Running;
                return _canStop;
            }
        }





        public bool IsInstalled { get; set; }

        public bool IsDebug
        {
            get { return _isDebug; }
            set
            {
                if (value != _isDebug)
                {
                    _isDebug = value;

                    if (IsDebug)
                    {
                        ((Hierarchy) LogManager.GetRepository()).Root.Level = Level.All;
                        ((Hierarchy) LogManager.GetRepository()).RaiseConfigurationChanged(
                            EventArgs.Empty);
                    }
                    else
                    {
                        ((Hierarchy) LogManager.GetRepository()).Root.Level = Level.Info;
                        ((Hierarchy) LogManager.GetRepository()).RaiseConfigurationChanged(
                            EventArgs.Empty);
                    }
                    Settings.Default.IsDebug = IsDebug;
                    Settings.Default.Save();
                }
            }
        }

        public ILog Log { get; set; }

        private void setupConfigChangeWatcher()
        {
            const ConfigurationUserLevel configUserLevel = ConfigurationUserLevel.PerUserRoamingAndLocal;
            
            //this is just a dummy for creating settings path
            Settings.Default.something = "dasdsadsa";
            Settings.Default.Save();


            string path = ConfigurationManager.OpenExeConfiguration(configUserLevel).FilePath;


            configChangeWatcher.Path =
                Path.GetDirectoryName(path);
            configChangeWatcher.Filter =
                Path.GetFileName(path);
            configChangeWatcher.EnableRaisingEvents = true;
        }

        private void configChangeWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            foreach (FileSystemWatcherExtended fileSystemWatcherExtended in systemWatchers)
            {
                fileSystemWatcherExtended.EnableRaisingEvents = false;
            }
            systemWatchers = LoadFileSystemWatchersExtended(Log);
        }

        public void Run()
        {
            setupConfigChangeWatcher();
            foreach (var fileSystemWatcherExtended in systemWatchers)
            {
                fileSystemWatcherExtended.EnableRaisingEvents = true;
            }

            _canStart = false;
            _canStop = true;
          //  Run(this);
        }

        public void Stop()
        {
            configChangeWatcher.EnableRaisingEvents = false;
            //setupConfigChangeWatcher();
            foreach (var fileSystemWatcherExtended in systemWatchers)
            {
                fileSystemWatcherExtended.EnableRaisingEvents = false;
            }

            _canStart = true;
            _canStop = false;
            //  Run(this);
        }

        protected override void OnStart(string[] args)
        {
            Log.Info("HardDriveMonitorService is starting");
            Debugger.Break();
            // System.Diagnostics.Debugger.Launch();
        }

        protected override void OnStop()
        {
            Log.Info("HardDriveMonitorService is stopping");
            SaveFileSystemWatchersExtended(systemWatchers);
        }


        public static void SaveFileSystemWatchersExtended(List<FileSystemWatcherExtended> fswe)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, fswe);
                ms.Position = 0;
                var buffer = new byte[(int) ms.Length];
                ms.Read(buffer, 0, buffer.Length);

                Settings.Default.FileSystemWatchersExtended = Convert.ToBase64String(buffer);
            }
            Settings.Default.Save();
        }

        public static List<FileSystemWatcherExtended> LoadFileSystemWatchersExtended(ILog Log)
        {
            ConfigurationManager.RefreshSection("HardDriveMonitorService.Properties.Settings");
            //HardDriveMonitorService.Properties.Settings
            List<FileSystemWatcherExtended> sw = null;

            if (!string.IsNullOrEmpty(Settings.Default.FileSystemWatchersExtended))
                using (var ms = new MemoryStream(Convert.FromBase64String(Settings.Default.FileSystemWatchersExtended)))
                {
                    var bf = new BinaryFormatter();
                    sw = (List<FileSystemWatcherExtended>) bf.Deserialize(ms);
                }
            else
            {
                sw = new List<FileSystemWatcherExtended>
                {
                    new FileSystemWatcherExtended(Log, Settings.Default.BasicPath)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = true
                    }
                };
            }

            foreach (FileSystemWatcherExtended item in sw)
            {
                item.Log = Log;
                item.setSerializedFields();
            }

            return sw;
        }


        public void Install()
        {
            // ExecuteCommand(File.ReadAllText("installHardDriveMonitor.bat"));
            ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
            IsInstalled = true;

        }


        public void Uninstall()
        {
            //ExecuteCommand(File.ReadAllText("uninstallHardDriveMonitor.bat"));
            ManagedInstallerClass.InstallHelper(new[] {"/u", Assembly.GetExecutingAssembly().Location});
            IsInstalled = false;
        }

        private static void ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode, "ExecuteCommand");
            process.Close();
        }
    }
}