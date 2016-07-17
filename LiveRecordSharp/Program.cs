using System;
using System.Collections.Generic;
using System.Linq;
using LiveRecordSharp.LiveSites;

namespace LiveRecordSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var siteList = new List<LiveSite> {new DouyuLiveSite(), new HuomaoLiveSite()};
            LiveSite l = null;
            foreach (var liveSite in siteList)
            {
                if (liveSite.SiteRegex.IsMatch(args[0]))
                {
                    l = liveSite;
                    break;
                }
                liveSite.Dispose();
            }
            var record = new Record(l);
            record.StartRecordAsync().Wait();
        }
    }
}
