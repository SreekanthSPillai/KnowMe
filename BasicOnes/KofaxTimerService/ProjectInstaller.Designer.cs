namespace KofaxTimerService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.KofaxTimerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.KofaxTimerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // KofaxTimerServiceProcessInstaller
            // 
            this.KofaxTimerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.KofaxTimerServiceProcessInstaller.Password = null;
            this.KofaxTimerServiceProcessInstaller.Username = null;
            this.KofaxTimerServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.KofaxTimerServiceProcessInstaller_AfterInstall);
            // 
            // KofaxTimerServiceInstaller
            // 
            this.KofaxTimerServiceInstaller.ServiceName = "CLUK.KofaxTimerService";
            this.KofaxTimerServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.KofaxTimerServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.KofaxTimerServiceInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.KofaxTimerServiceProcessInstaller,
            this.KofaxTimerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller KofaxTimerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller KofaxTimerServiceInstaller;
    }
}