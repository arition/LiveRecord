using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveRecordSharp.LiveSites
{
    public abstract class LiveSite : IDisposable
    {
        public abstract Regex SiteRegex { get; }
        public abstract string LiveUrl { get; set; }
        public abstract Task<bool> IsLive();
        public abstract Task<string> GetLiveStreamUrl();
        public abstract void Dispose();
    }
}
