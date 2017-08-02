﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public override Regex SiteRegex { get; } = new Regex("http(s|)://www.douyu(tv|).com/(?<roomName>.+)", RegexOptions.Compiled);
        private Regex RoomInfoJsonRegex { get; } = new Regex(@"(?<=var \$ROOM = ).+(?=;)", RegexOptions.Compiled);
        private HttpClient HttpClient { get; } = new HttpClient();
        private JObject LiveInfoJson { get; set; }
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

        public override string LiveRoomName => GetLiveInfoJsonAsync().Result["room_name"].ToString();

        public DouyuLiveSite()
        {
            //HttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public DouyuLiveSite(string liveUrl)
        {
            LiveUrl = liveUrl;
            //HttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public override async Task<bool> IsLiveAsync()
        {
            var json = await GetLiveInfoJsonAsync(true);
            return json["show_status"].ToString() == "1";
        }

        public override async Task<string> GetLiveStreamUrlAsync()
        {
            // code from https://github.com/soimort/you-get/blob/develop/src/you_get/extractors/douyutv.py
            // Some new code from https://gist.github.com/spacemeowx2/629b1d131bd7e240a7d28742048e80fc and
            // https://github.com/soimort/you-get/issues/1720
            var json = await GetLiveInfoJsonAsync();
            var roomId = json["room_id"].ToString();
            var tt = DateTime.UtcNow.ToUnixTimeStamp().ToString();
            var signContent = $"room/{roomId}?aid=androidhd1&cdn=ws&client_sys=android&time={tt}Y237pxTx2In5ayGz";
            var signBytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(signContent));
            var sign = BitConverter.ToString(signBytes).Replace("-", string.Empty).ToLower();
            var jsonRequestUrl =
                $"https://capi.douyucdn.cn/api/v1/room/{roomId}?aid=androidhd1&cdn=ws&client_sys=android&time={tt}&auth={sign}";
            var response = await HttpClient.GetAsync(jsonRequestUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(content) as JToken;
            if (data["error"].ToString() != "0") throw new HttpRequestException($"Error: {data["error"]}");
            data = data["data"];
            return data["rtmp_url"] + "/" + data["rtmp_live"];
        }

        public override void Dispose()
        {
            HttpClient.Dispose();
        }

        private async Task<JObject> GetLiveInfoJsonAsync(bool refresh = false)
        {
            if (!refresh && LiveInfoJson!=null) return LiveInfoJson;
            if (SiteRegex.Match(LiveUrl).Groups["roomName"].Value.All(char.IsDigit))
            {
                var url = "http://m.douyu.com/html5/live?roomId=" + SiteRegex.Match(LiveUrl).Groups["roomName"].Value;
                var content = await HttpClient.GetStringAsync(url);
                if (JObject.Parse(content)["error"].ToString() == "0")
                {
                    LiveInfoJson = new JObject
                    {
                        ["room_id"] = SiteRegex.Match(LiveUrl).Groups["roomName"].Value,
                        ["show_status"] = "1"
                    };
                    LiveInfoJson["room_name"] = LiveInfoJson["room_id"];
                }
                else
                {
                    LiveInfoJson = new JObject {["show_status"] = "0"};
                }
            }
            else
            {
                var content = await HttpClient.GetStringAsync(LiveUrl);
                LiveInfoJson = JObject.Parse(RoomInfoJsonRegex.Match(content).Value);
            }
            return LiveInfoJson;
        }
    }
}