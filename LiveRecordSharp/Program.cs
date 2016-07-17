using System;
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
        }
    }
}
