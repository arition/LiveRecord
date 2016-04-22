using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveRecordSharp.LiveSites;

namespace LiveRecordSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            LiveSite l = new DouyuLiveSite("http://www.douyu.com/59872");
            l.GetLiveStreamUrlAsync().Wait();
            Console.ReadKey();
        }
    }
}
