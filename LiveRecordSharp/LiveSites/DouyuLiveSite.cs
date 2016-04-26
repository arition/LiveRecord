using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiveRecordSharp.Properties;
using Newtonsoft.Json.Linq;

namespace LiveRecordSharp.LiveSites
{
    public sealed class DouyuLiveSite : LiveSite
    {
        public override Regex SiteRegex { get; } = new Regex("http://www.douyu(tv|).com/", RegexOptions.Compiled);
        private Regex RoomInfoJsonRegex { get; } = new Regex(@"(?<=var \$ROOM = ).+(?=;)", RegexOptions.Compiled);
        public HttpClient HttpClient { get; } = new HttpClient();
        private string LiveInfoJson { get; set; }

        private string _liveUrl;

        public override string LiveUrl
        {
            get { return _liveUrl; }
            set
            {
                if (SiteRegex.IsMatch(value)) _liveUrl = value;
                else throw new ArgumentException(Resources.DouyuLiveSite_LiveUrl_Url_does_not_match_this_site_);
            }
        }

        public override string LiveRoomName => JObject.Parse(GetLiveInfoJsonAsync().Result)["room_name"].ToString();

        public DouyuLiveSite(string liveUrl)
        {
            LiveUrl = liveUrl;
        }

        public override async Task<bool> IsLiveAsync()
        {
            var json = await GetLiveInfoJsonAsync(true);
            return JObject.Parse(json)["show_status"].ToString() == "1";
        }

        public override async Task<string> GetLiveStreamUrlAsync()
        {
            //code from https://github.com/soimort/you-get/blob/develop/src/you_get/extractors/douyutv.py
            var json = await GetLiveInfoJsonAsync();
            var roomId = JObject.Parse(json)["room_id"].ToString();
            var suffix = $"room/{roomId}?aid=android&cdn=ws2&client_sys=android&time={DateTime.UtcNow.ToUnixTimeStamp()}";
            var signBytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(suffix + "1231"));
            var sign = BitConverter.ToString(signBytes).Replace("-", string.Empty).ToLower();
            var jsonRequestUrl = $"http://www.douyu.com/api/v1/{suffix}&auth={sign}";
            var content = await HttpClient.GetStringAsync(jsonRequestUrl);
            var data = JObject.Parse(content)["data"];
            if (data["error"] != null) throw new HttpRequestException($"Error: {data["error"]}");
            return $"{data["rtmp_url"]}/{data["rtmp_live"]}";
        }

        public override void Dispose()
        {
            HttpClient.Dispose();
        }

        private async Task<string> GetLiveInfoJsonAsync(bool refresh = false)
        {
            if (!refresh && !string.IsNullOrEmpty(LiveInfoJson)) return LiveInfoJson;
            var content = await HttpClient.GetStringAsync(LiveUrl);
            LiveInfoJson = RoomInfoJsonRegex.Match(content).Value;
            return LiveInfoJson;
        }
    }
}