using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiveRecordSharp.Properties;
using Newtonsoft.Json.Linq;

namespace LiveRecordSharp.LiveSites
{
    public sealed class DouyuLiveSite : LiveSite
    {
        public override Regex SiteRegex { get; } = new Regex("http://www.douyutv.com/", RegexOptions.Compiled);
        private Regex RoomInfoJsonRegex { get; } = new Regex(@"(?<=var \$ROOM = ).+?(?=;)", RegexOptions.Compiled);

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

        public HttpClient HttpClient { get; } = new HttpClient();

        public DouyuLiveSite(string liveUrl)
        {
            LiveUrl = liveUrl;
        }

        public override async Task<bool> IsLive()
        {
            var content = await HttpClient.GetStringAsync(LiveUrl);
            return JObject.Parse(content)["show_status"].ToString() == "1";
        }

        public override Task<string> GetLiveStreamUrl()
        {

            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}