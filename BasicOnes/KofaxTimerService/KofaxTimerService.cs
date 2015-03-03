using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KofaxTimerService
{
    public partial class KofaxTimerService : ServiceBase
    {
        DocumentClassificationTask serviceMainTask = new DocumentClassificationTask();
        public KofaxTimerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try { serviceMainTask.Start(); }
            catch { }
            
        }

        protected override void OnStop()
        {
            try { serviceMainTask.Stop();}
            catch { }
            
        }
    }
}
