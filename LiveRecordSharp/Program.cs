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
            var l = new DouyuLiveSite(args[0]);
            var record = new Record(l);
            record.StartRecordAsync().Wait();
            Console.ReadKey();
        }
    }
}
