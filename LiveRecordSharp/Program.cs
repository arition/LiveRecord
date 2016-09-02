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
            var log = Log.GetLogger(typeof(Program));
            if (args.Length == 0)
            {
                log.Error("Usage: LiveRecordSharp RecordUrl");
                return;
            }
            var siteList = new List<LiveSite> {new DouyuLiveSite(), new HuomaoLiveSite()};
            LiveSite l = null;
            foreach (var liveSite in siteList)
            {
                if (liveSite.SiteRegex.IsMatch(args[0]))
                {
                    l = liveSite;
                    l.LiveUrl = args[0];
                    break;
                }
                liveSite.Dispose();
            }
            if (l == null)
            {
                log.Error("无法找到可用的录制方法，请确定url输入正确");
                return;
            }
            var record = new Record(l);
            record.StartRecordAsync().Wait();
        }
    }
}
