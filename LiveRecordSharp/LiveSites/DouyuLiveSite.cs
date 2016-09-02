using System;
using System.Collections.Generic;
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
        public override Regex SiteRegex { get; } = new Regex("http(s|)://www.douyu(tv|).com/", RegexOptions.Compiled);
        private Regex RoomInfoJsonRegex { get; } = new Regex(@"(?<=var \$ROOM = ).+(?=;)", RegexOptions.Compiled);
        private HttpClient HttpClient { get; } = new HttpClient();
        private string LiveInfoJson { get; set; }
        private string UserAgent { get; } = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.22 Safari/537.36";

        private string _liveUrl;

        public override string LiveUrl
        {
            get { return _liveUrl; }
            set
            {
                if (SiteRegex.IsMatch(value)) _liveUrl = value;
                else throw new ArgumentException(Resources.LiveUrl_Url_does_not_match_this_site_);
            }
        }

        public override string LiveRoomName => JObject.Parse(GetLiveInfoJsonAsync().Result)["room_name"].ToString();

        public DouyuLiveSite()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public DouyuLiveSite(string liveUrl)
        {
            LiveUrl = liveUrl;
            HttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
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
            var did = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            var tt = (DateTime.UtcNow.ToUnixTimeStamp()/60).ToString();
            var signContent = $"{roomId}{did}A12Svb&%1UUmf@hC{tt}";
            var signBytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(signContent));
            var sign = BitConverter.ToString(signBytes).Replace("-", string.Empty).ToLower();
            var jsonRequestUrl = $"http://www.douyu.com/lapi/live/getPlay/{roomId}";
            var postContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"cdn", "ws2"},
                {"rate", "0"},
                {"tt", tt},
                {"did", did},
                {"sign", sign}
            });
            var response = await HttpClient.PostAsync(jsonRequestUrl, postContent);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
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