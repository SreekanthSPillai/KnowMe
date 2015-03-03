using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KofaxTimerService
{
    public partial class DocumentClassificationTask
    {
            // syncLock object, used to lock the code block
            private static object syncLock = new object();

            private BackgroundWorker bw = new BackgroundWorker();


            public DocumentClassificationTask()
            {

                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            }

            public void Start()
            {
                if (bw.IsBusy != true)
                {
                    bw.RunWorkerAsync();
                }
            }

            public void Stop()
            {
                if (bw.WorkerSupportsCancellation == true)
                {
                    bw.CancelAsync();
                }
            }

            private void bw_DoWork(object sender, DoWorkEventArgs e)
            {
                BackgroundWorker worker = sender as BackgroundWorker;

                while (true)
                {
                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                       
                        //To ensure thread safety
                        lock (syncLock)
                        {
                            /**************************************/
                            /************Kofax API/Batch calls************/
                            // worker.ReportProgress((i * 10)); If required to write update log
                            /**************************************/
                            System.Threading.Thread.Sleep(5000);
                            worker.ReportProgress(10, "Document Classification Task First Stop");
                            System.Threading.Thread.Sleep(5000);
                            worker.ReportProgress(10, "Document Classification Task Second Stop");
                            System.Threading.Thread.Sleep(5000);
                            worker.ReportProgress(10, "Document Classification Task wake up call.");
                        }

                        
                    }
                }
            }

            private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                if ((e.Cancelled == true))
                {
                    WriteLog("Canceled!");
                }

                else if (!(e.Error == null))
                {
                    WriteLog("Error: " + e.Error.Message);
                }

                else
                {
                    WriteLog("Done!");
                }
            }

            private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                WriteLog(e.ProgressPercentage.ToString() + e.UserState.ToString());
            }


            private void WriteLog(string message)
            {
                message = string.Format("{0}: {1}\r\n", DateTime.Now, message);
                File.AppendAllText("C:\\serviceLog.log", message); 
            }
        
    }
}
