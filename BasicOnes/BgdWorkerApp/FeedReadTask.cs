using System;
using System.ComponentModel;
using System.IO;
using System.Net;


namespace BgdWorkerApp
{
    public partial class FeedReadTask 
    {
        private BackgroundWorker bw = new BackgroundWorker();

        //http://feeds.bbci.co.uk/news/rss.xml

        public FeedReadTask()
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

            for (int i = 1; (i <= 10); i++)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Perform a time consuming operation and report progress.
                    // Create a request for the URL. 
                    WebRequest request = WebRequest.Create("http://feeds.bbci.co.uk/news/rss.xml");

                    IWebProxy proxy = new WebProxy("cluwcfproxy.canadalifeuk.bz",8080); // port number is of type integer 
                    proxy.Credentials = new NetworkCredential("bio187", "Jan@2015$", "canadalifeuk.bz");

                    // If required by the server, set the credentials.
                    //request.Credentials = CredentialCache.DefaultNetworkCredentials;

                    request.Proxy = proxy;

                    // Get the response.
                    WebResponse response = request.GetResponse();
                    // Display the status.
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.
                    File.WriteAllText("rss.xml", responseFromServer);
                    // Clean up the streams and the response.
                    reader.Close();
                    response.Close();


                    System.Threading.Thread.Sleep(500);
                    worker.ReportProgress((i * 10));
                }
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                Console.WriteLine("Canceled!");
            }

            else if (!(e.Error == null))
            {
                Console.WriteLine("Error: " + e.Error.Message);
            }

            else
            {
                Console.WriteLine("Done!");
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage.ToString() + "%");
        }
    }
}