using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;

namespace HardDriveMonitorService
{
    [Serializable]
    public class FileSystemWatcherExtended : INotifyPropertyChanged
    {
        private bool _WatchAttributes;
        private bool _WatchCreationTime;
        private bool _WatchDirectoryName = true;
        private bool _WatchFileName = true;
        private bool _WatchLastAccess;
        private bool _WatchLastWrite = true;
        private bool _WatchSecurity;
        private bool _WatchSize;
        [NonSerialized] private ILog _log;
        private string _path;
        private bool m_EnableRaisingEvents;

        [NonSerialized] private FileSystemWatcher m_FileSystemWatcher;


        private bool m_IncludeSubdirectories;

        public FileSystemWatcherExtended()
        {
            FileSystemWatcher = new FileSystemWatcher();
            addEventHandlers();
            EnableRaisingEvents = true;
            IncludeSubdirectories = true;
        }

        public FileSystemWatcherExtended(ILog log)
        {
            FileSystemWatcher = new FileSystemWatcher();
            _log = log;
            addEventHandlers();
            EnableRaisingEvents = true;
            IncludeSubdirectories = true;
        }

        public FileSystemWatcherExtended(ILog log, string path)
        {
            FileSystemWatcher = new FileSystemWatcher(path);
            _log = log;
            _path = path;
            addEventHandlers();
            EnableRaisingEvents = true;
            IncludeSubdirectories = true;
        }

        private FileSystemWatcher FileSystemWatcher
        {
            get
            {
                if (m_FileSystemWatcher == null)
                {
                    m_FileSystemWatcher = new FileSystemWatcher(Path);
                    addEventHandlers();
                }
                return m_FileSystemWatcher;
            }
            set { m_FileSystemWatcher = value; }
        }


        [Browsable(false)]
        public ILog Log
        {
            get { return _log; }
            set
            {
                _log = value;
                OnPropertyChanged();
            }
        }

        public bool IncludeSubdirectories
        {
            get { return FileSystemWatcher.IncludeSubdirectories; }
            set
            {
                m_IncludeSubdirectories = FileSystemWatcher.IncludeSubdirectories = value;
                OnPropertyChanged();
            }
        }

        public bool EnableRaisingEvents
        {
            get { return FileSystemWatcher.EnableRaisingEvents; }
            set
            {
                m_EnableRaisingEvents = FileSystemWatcher.EnableRaisingEvents = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                FileSystemWatcher.Path = value;
                OnPropertyChanged();
            }
        }

        public bool WatchFileName
        {
            get { return _WatchFileName; }
            set
            {
                _WatchFileName = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }

        public bool WatchDirectoryName
        {
            get { return _WatchDirectoryName; }
            set
            {
                _WatchDirectoryName = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }

        public bool WatchAttributes
        {
            get { return _WatchAttributes; }
            set
            {
                _WatchAttributes = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }

        public bool WatchSize
        {
            get { return _WatchSize; }
            set
            {
                _WatchSize = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }

        public bool WatchLastWrite
        {
            get { return _WatchLastWrite; }
            set
            {
                _WatchLastWrite = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }

        public bool WatchLastAccess
        {
            get { return _WatchLastAccess; }
            set
            {
                _WatchLastAccess = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }

        public bool WatchCreationTime
        {
            get { return _WatchCreationTime; }
            set
            {
                _WatchCreationTime = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }

        public bool WatchSecurity
        {
            get { return _WatchSecurity; }
            set
            {
                _WatchSecurity = value;
                setNotifyFilter();
                OnPropertyChanged();
            }
        }


        /*
        public bool IncludeSubdirectories { get; set; }
		public string Path { get; set; }
		

        FileName = 1,
        DirectoryName = 2,
        Attributes = 4,
        Size = 8,
        LastWrite = 16,
        LastAccess = 32,
        CreationTime = 64,
        Security = 256,
         */

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void setSerializedFields()
        {
            FileSystemWatcher.EnableRaisingEvents = m_EnableRaisingEvents;
            FileSystemWatcher.Path = _path;
            FileSystemWatcher.IncludeSubdirectories = m_IncludeSubdirectories;
            setNotifyFilter();
        }

        private void addEventHandlers()
        {
            FileSystemWatcher.Changed += _Changed;
            FileSystemWatcher.Created += _Created;
            FileSystemWatcher.Deleted += _Deleted;
            FileSystemWatcher.Renamed += _Renamed;

            FileSystemWatcher.Error += _Error;
        }

        private void _Error(object sender, ErrorEventArgs e)
        {
            var fswe = sender as FileSystemWatcher;
            //_log.Error("In "+ fswe.Path+" an error has occured",e.GetException());
            _log.Debug("In " + fswe.Path + " an error has occured", e.GetException());
        }


        private void _Changed(object sender, FileSystemEventArgs e)
        {
            _ChangeEvent(sender as FileSystemWatcher, e);
        }

        private void _Created(object sender, FileSystemEventArgs e)
        {
            _ChangeEvent(sender as FileSystemWatcher, e);
        }

        private void _Deleted(object sender, FileSystemEventArgs e)
        {
            _ChangeEvent(sender as FileSystemWatcher, e);
        }

        private void _Renamed(object sender, RenamedEventArgs e)
        {
            var fswe = sender as FileSystemWatcher;

            if (_log.IsDebugEnabled)
            {
                _log.Debug(Assembly.GetExecutingAssembly().Location);
            }

            _log.Info(fswe.Path + ":" + e.FullPath + " " + e.ChangeType + "from: " + e.OldName + " to " + e.Name);
        }


        private void _ChangeEvent(FileSystemWatcher fsw, FileSystemEventArgs e)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(Assembly.GetExecutingAssembly().Location);
            }

            _log.Info(fsw.Path + ":" + e.FullPath + " " + e.Name + " " + e.ChangeType);
        }

        private void setNotifyFilter()
        {
            FileSystemWatcher.NotifyFilter = 0;

            if (WatchAttributes)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.Attributes;
            if (WatchCreationTime)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.CreationTime;
            if (WatchDirectoryName)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.DirectoryName;
            if (WatchFileName)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.FileName;
            if (WatchLastAccess)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.LastAccess;
            if (WatchLastWrite)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.LastWrite;
            if (WatchSecurity)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.Security;
            if (WatchSize)
                FileSystemWatcher.NotifyFilter |= NotifyFilters.Size;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}