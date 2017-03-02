using System.ComponentModel;
using System.Configuration.Install;

namespace HardDriveMonitorService
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }
    }
}