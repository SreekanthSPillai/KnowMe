using System;
using System.Collections.Generic;
using System.Text;

namespace BgdWorkerApp
{
    class Program
    {
        static void Main(string[] args)
        {

            FeedReadTask readNews = new FeedReadTask();
            readNews.Start();
            Console.ReadLine();
            readNews.Stop();

        }
    }
}
