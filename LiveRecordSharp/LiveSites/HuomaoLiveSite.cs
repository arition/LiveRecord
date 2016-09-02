using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiveRecordSharp.Properties;
using Newtonsoft.Json.Linq;

namespace LiveRecordSharp.LiveSites
{
    public sealed class HuomaoLiveSite :LiveSite
    {
        public override Regex SiteRegex { get; } = new Regex("http://www.huomao.com/", RegexOptions.Compiled);
        private Regex LiveRoomNameRegex { get; } = new Regex("(?<=<title>).*?(?=</title>)", RegexOptions.Compiled);
        private Regex VideoIdRegex { get; } = new Regex(@"(?<=getFlash\(""\d+"","").*?(?=""\);)", RegexOptions.Compiled);
        public HttpClient HttpClient { get; } = new HttpClient();
        private string Html { get; set; }
        private string LiveData { get; set; }

        public override string LiveRoomName => LiveRoomNameRegex.Match(GetHtmlAsync().Result).Value;

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

        public HuomaoLiveSite() { }

        public HuomaoLiveSite(string liveUrl)
        {
            LiveUrl = liveUrl;
        }

        public override async Task<bool> IsLiveAsync()
        {
            return JObject.Parse(await GetLiveDataAsync(true))["roomStatus"].ToString() == "1";
        }

        public override async Task<string> GetLiveStreamUrlAsync()
        {
            return JObject.Parse(await GetLiveDataAsync())["streamList"].First()["list"]
                .First(t => t["type"].ToString() == "TD")["url"].ToString();
        }

        public override void Dispose()
        {
            HttpClient.Dispose();
        }
        
        private async Task<string> GetHtmlAsync(bool refresh = false)
        {
            if (!refresh && !string.IsNullOrEmpty(Html)) return Html;
            Html = await HttpClient.GetStringAsync(LiveUrl);
            return Html;
        }

        private async Task<string> GetLiveDataAsync(bool refresh = false)
        {
            if (!refresh && !string.IsNullOrEmpty(LiveData)) return LiveData;
            var videoId = VideoIdRegex.Match(await GetHtmlAsync(refresh)).Value;
            var parameterDictionary = new Dictionary<string, string>
            {
                {"cdns", "1"},
                {"VideoIDS", videoId},
                {"streamtype", "live"}
            };
            var responseMessage = await HttpClient.PostAsync("http://www.huomao.com/swf/live_data",
                new FormUrlEncodedContent(parameterDictionary));
            responseMessage.EnsureSuccessStatusCode();
            LiveData = await responseMessage.Content.ReadAsStringAsync();
            return LiveData;
        }
    }
}
