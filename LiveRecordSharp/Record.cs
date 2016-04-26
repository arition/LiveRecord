using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using log4net;
using LiveRecordSharp.LiveSites;

namespace LiveRecordSharp
{
    public class Record
    {
        public LiveSite LiveSite { get; private set; }
        private ILog Log { get; } = LiveRecordSharp.Log.GetLogger(typeof (Record));

        public Record(LiveSite liveSite)
        {
            LiveSite = liveSite;
        }

        public async Task StartRecordAsync()
        {
            Log.Info("Link Start");
            while (true)
            {
                try
                {
                    if (await LiveSite.IsLiveAsync())
                    {
                        Log.Info($"{LiveSite.LiveRoomName} is live.");
                        var dirName = LiveSite.LiveRoomName.KeepAlpha();
                        if (string.IsNullOrWhiteSpace(dirName))
                            dirName = LiveSite.LiveUrl.Substring(LiveSite.LiveUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        if (string.IsNullOrWhiteSpace(dirName))
                            dirName = LiveSite.LiveUrl.Substring(LiveSite.LiveUrl.Substring(0, LiveSite.LiveUrl.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
                        var fileName = Path.Combine("record", dirName, $"{DateTime.UtcNow.ToUnixTimeStamp()}.mp4");
                        var directoryName = new FileInfo(fileName).DirectoryName;
                        if (directoryName != null) Directory.CreateDirectory(directoryName);
                        var url = await LiveSite.GetLiveStreamUrlAsync();
                        var p = new Process
                        {
                            StartInfo =
                            {
                                Arguments = $"-i {url} -acodec copy -vcodec copy {fileName}",
                                FileName = "ffmpeg",
                                RedirectStandardError = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false
                            }
                        };
                        Log.Info($"File save path: {fileName}");
                        Log.Debug($"Process args: {p.StartInfo.Arguments}");
                        p.ErrorDataReceived += (o, e) => Log.Info(e.Data);
                        p.OutputDataReceived += (o, e) => Log.Info(e.Data);
                        p.Start();
                        p.BeginErrorReadLine();
                        p.BeginOutputReadLine();
                        p.WaitForExit();
                    }
                    await Task.Delay(10000);
                }
                catch (Exception e)
                {
                    Log.Fatal(e);
                }
            }
        }
    }
}
