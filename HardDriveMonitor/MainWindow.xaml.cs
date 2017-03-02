using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using HardDriveMonitor.Annotations;
using HardDriveMonitor.Properties;
using HardDriveMonitorService;

namespace HardDriveMonitor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private FileSystemWatcher logFileChangWatcher;

        public MainWindow()
        {
            hardDriveMonitorService = new HardDriveMonitorService.HardDriveMonitorService();
            hardDriveMonitorServiceController =
                ServiceController.GetServices()
                    .FirstOrDefault(s => s.ServiceName == hardDriveMonitorService.ServiceName);


            FileSystemWatchersExtended = hardDriveMonitorService.IsInstalled
                ? new ObservableCollectionWithItemNotify<FileSystemWatcherExtended>(
                    HardDriveMonitorService.HardDriveMonitorService.LoadFileSystemWatchersExtended(
                        hardDriveMonitorService.Log))
                : new ObservableCollectionWithItemNotify<FileSystemWatcherExtended>();

            FileSystemWatchersExtended.CollectionChanged += FileSystemWatchersExtended_CollectionChanged;


            hardDriveMonitorServiceList = new List<HardDriveMonitorService.HardDriveMonitorService>
            {
                hardDriveMonitorService
            };
            hardDriveMonitorServiceControllerList = new List<ServiceController>();


            if (hardDriveMonitorServiceController != null)
                hardDriveMonitorServiceControllerList.Add(hardDriveMonitorServiceController);


            logFileChangWatcher = new FileSystemWatcher(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            logFileChangWatcher.Filter = "log-file.txt";
            logFileChangWatcher.Changed += (o, e) => { OnPropertyChanged("LoggedText"); };

            InitializeComponent();
        }

        public string LoggedText
        {
            get
            {
                string pathOfLoggedText = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "log-file.txt");

                using (
                    var fileStream = new FileStream(pathOfLoggedText, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite))
                using (var textReader = new StreamReader(fileStream))
                {
                    return textReader.ReadToEnd();
                }
            }
        }

        public bool CanRemove
        {
            get { return MonitoredPathsListView != null && MonitoredPathsListView.SelectedItem != null; }
        }

        public bool CanInstall
        {
            get { return !hardDriveMonitorService.IsInstalled; }
        }

        public bool CanUninstall
        {
            get { return hardDriveMonitorService.IsInstalled; }
        }

        public bool CanStart
        {
            get
            {
                return hardDriveMonitorService.CanStart;
            }
        }

        public bool CanStop
        {
            get { return hardDriveMonitorService.CanStop; }
        }


        public List<HardDriveMonitorService.HardDriveMonitorService> hardDriveMonitorServiceList { get; set; }
        public List<ServiceController> hardDriveMonitorServiceControllerList { get; set; }


        public HardDriveMonitorService.HardDriveMonitorService hardDriveMonitorService { get; set; }
        public ServiceController hardDriveMonitorServiceController { get; set; }


        public ObservableCollectionWithItemNotify<FileSystemWatcherExtended> FileSystemWatchersExtended { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void FileSystemWatchersExtended_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HardDriveMonitorService.HardDriveMonitorService.SaveFileSystemWatchersExtended(
                FileSystemWatchersExtended.ToList());
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            //add

            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FileSystemWatchersExtended.Add(new FileSystemWatcherExtended(hardDriveMonitorService.Log,
                    dialog.SelectedPath));
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MonitoredPathsListView.SelectedItem != null)
            {
                //  var item = MonitoredPathsListView.SelectedItem as FileSystemWatcherExtended;
                FileSystemWatchersExtended.Remove((FileSystemWatcherExtended) MonitoredPathsListView.SelectedItem);
            }
            //remove
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //start
           // hardDriveMonitorServiceController.Start();

            hardDriveMonitorServiceControllerList.Clear();

            if (hardDriveMonitorServiceController != null)
                hardDriveMonitorServiceControllerList.Add(hardDriveMonitorServiceController);

            OnPropertyChanged("hardDriveMonitorServiceControllerList");
            
            hardDriveMonitorService.Run();

            hardDriveMonitorServiceList.Clear();
            hardDriveMonitorServiceList.Add(hardDriveMonitorService);
            OnPropertyChanged("hardDriveMonitorServiceList");
           

            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanStop");


            MonitoredPathsListView.Items.Refresh();

            ServiceInfoDataGrid.Items.Refresh();
            ServiceInfoDataGrid2.Items.Refresh();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
           // hardDriveMonitorServiceController.Stop();
            //stop

            hardDriveMonitorServiceControllerList.Clear();

            if (hardDriveMonitorServiceController != null)
                hardDriveMonitorServiceControllerList.Add(hardDriveMonitorServiceController);

            hardDriveMonitorService.Stop();

            hardDriveMonitorServiceList.Clear();
            hardDriveMonitorServiceList.Add(hardDriveMonitorService);
            OnPropertyChanged("hardDriveMonitorServiceList");

            OnPropertyChanged("hardDriveMonitorServiceControllerList");

            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanStop");


            MonitoredPathsListView.Items.Refresh();
            ServiceInfoDataGrid.Items.Refresh();
            ServiceInfoDataGrid2.Items.Refresh();
        }

        private void installButton_Click(object sender, RoutedEventArgs e)
        {
            //install
            hardDriveMonitorService.Install();

            hardDriveMonitorServiceList.Clear();
            hardDriveMonitorServiceList.Add(hardDriveMonitorService);


            hardDriveMonitorServiceController =
                ServiceController.GetServices()
                    .FirstOrDefault(s => s.ServiceName == hardDriveMonitorService.ServiceName);

            hardDriveMonitorServiceControllerList.Clear();

            if (hardDriveMonitorServiceController != null)
                hardDriveMonitorServiceControllerList.Add(hardDriveMonitorServiceController);

            OnPropertyChanged("hardDriveMonitorServiceList");
            OnPropertyChanged("hardDriveMonitorServiceControllerList");
            OnPropertyChanged("CanInstall");
            OnPropertyChanged("CanUninstall");

            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanStop");


            ServiceInfoDataGrid.Items.Refresh();
            ServiceInfoDataGrid2.Items.Refresh();
        }

        private void uninstallButton_Click(object sender, RoutedEventArgs e)
        {
            //uninstall

            hardDriveMonitorService.Uninstall();


            hardDriveMonitorServiceList.Clear();
            hardDriveMonitorServiceList.Add(hardDriveMonitorService);

            hardDriveMonitorServiceControllerList.Clear();
            hardDriveMonitorServiceController = null;

            OnPropertyChanged("hardDriveMonitorServiceList");

            OnPropertyChanged("hardDriveMonitorServiceController");
            OnPropertyChanged("CanInstall");
            OnPropertyChanged("CanUninstall");

            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanStop");


            ServiceInfoDataGrid.Items.Refresh();
            ServiceInfoDataGrid2.Items.Refresh();
        }

        private void MonitoredPathsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged("CanRemove");
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void refreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            LogListBox.Text = LoggedText;
            OnPropertyChanged("LoggedText");
        }

        private void clearButton_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}