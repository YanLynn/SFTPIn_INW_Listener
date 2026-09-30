using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SFTPIn_INW_Listener
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller1.Username = null;
            serviceProcessInstaller1.Password = null;

            serviceInstaller1.ServiceName = "SFTPIn_INW_Listener";
            serviceInstaller1.DisplayName = "SFTPIn INW Listener";
            serviceInstaller1.Description = "Pulls the bank's inward cheque files via SFTP";
            serviceInstaller1.StartType = ServiceStartMode.Automatic;
        }
    }
}