using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveRecordSharp.LiveSites
{
    public abstract class LiveSite : IDisposable
    {
        public abstract Regex SiteRegex { get; }
        public abstract string LiveUrl { get; set; }
        public abstract string LiveRoomName { get; }
        public abstract Task<bool> IsLiveAsync();
        public abstract Task<string> GetLiveStreamUrlAsync();
        public abstract void Dispose();
    }
}
