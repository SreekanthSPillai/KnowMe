using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace KofaxTimerService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.KofaxTimerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.User;
            this.KofaxTimerServiceProcessInstaller.Username = "canadalifeuk\\bio187";
            this.KofaxTimerServiceProcessInstaller.Password = "Jan@2015$";

        }

        private void KofaxTimerServiceProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void KofaxTimerServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
