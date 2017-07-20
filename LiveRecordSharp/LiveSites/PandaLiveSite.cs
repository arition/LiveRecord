using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiveRecordSharp.Properties;
using Newtonsoft.Json.Linq;

namespace LiveRecordSharp.LiveSites
{
    public sealed class PandaLiveSite : LiveSite
    {
        private HttpClient HttpClient { get; } = new HttpClient();
        public override Regex SiteRegex { get; } = new Regex(@"http(s|)://(www\.|)panda(tv|)\.(com|tv)/(?<roomName>.+)");

        public override string LiveRoomName
        {
            get
            {
                GetLiveJsonAsync().Wait();
                return LiveInfoJson["roominfo"]["name"].ToString();
            }
        }

        private JObject LiveInfoJson { get; set; }

        private string _liveUrl;

        public override string LiveUrl
        {
            get => _liveUrl;
            set
            {
                if (SiteRegex.IsMatch(value)) _liveUrl = value;
                else throw new ArgumentException(Resources.LiveUrl_Url_does_not_match_this_site_);
            }
        }

        public PandaLiveSite()
        {
        }

        public PandaLiveSite(string liveUrl)
        {
            LiveUrl = liveUrl;
        }

        public override async Task<bool> IsLiveAsync()
        {
            await GetLiveJsonAsync(true);
            if (LiveInfoJson["videoinfo"]["status"].ToString() == "2") return true;
            return false;
        }

        // Code from https://github.com/soimort/you-get/blob/develop/src/you_get/extractors/panda.py
        public override async Task<string> GetLiveStreamUrlAsync()
        {
            await GetLiveJsonAsync();
            var plflag = LiveInfoJson["videoinfo"]["plflag"].ToString().Split('_');
            var roomKey = LiveInfoJson["videoinfo"]["room_key"].ToString();
            var data2 = JObject.Parse(LiveInfoJson["videoinfo"]["plflag_list"].ToString());
            var rid = data2["auth"]["rid"].ToString();
            var sign = data2["auth"]["sign"].ToString();
            var ts = data2["auth"]["time"].ToString();
            return $"http://pl{plflag[1]}.live.panda.tv/live_panda/{roomKey}.flv?sign={sign}&ts={ts}&rid={rid}";
        }

        private async Task<JObject> GetLiveJsonAsync(bool refresh = false)
        {
            if (!refresh && LiveInfoJson != null) return LiveInfoJson;
            var roomId = SiteRegex.Match(LiveUrl).Groups["roomName"].Value;
            var content = await HttpClient.GetStringAsync(
                $"http://www.panda.tv/api_room_v2?roomid={roomId}&__plat=pc_web&_={DateTime.UtcNow.ToUnixTimeStamp()}");
            LiveInfoJson = JObject.Parse(content)["data"] as JObject;
            return LiveInfoJson;
        }

        public override void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
