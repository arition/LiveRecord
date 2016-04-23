using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
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
            try
            {
                while (true)
                {
                    if (await LiveSite.IsLiveAsync())
                    {
                        var fileName = Path.Combine("record",
                            LiveSite.LiveRoomName.Replace(Path.GetInvalidPathChars(), "_"),
                            $"{DateTime.UtcNow.ToUnixTimeStamp()}.mp4");
                        var url = await LiveSite.GetLiveStreamUrlAsync();
                        var p = new Process
                        {
                            StartInfo =
                            {
                                Arguments = $"-i '{url}' -acodec copy -vcodec copy '{fileName}'",
                                FileName = "ffmpeg",
                                RedirectStandardError = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false
                            }
                        };
                        Log.Info($"{LiveSite.LiveRoomName} is live.");
                        Log.Info($"File save path: {fileName}");
                        Log.Debug($"Process args: {p.StartInfo.Arguments}");
                        p.ErrorDataReceived += (o, e) => Log.Error(e.Data);
                        p.OutputDataReceived += (o, e) => Log.Info(e.Data);
                        p.Start();
                        p.BeginErrorReadLine();
                        p.BeginOutputReadLine();
                        p.WaitForExit();
                    }
                    await Task.Delay(10000);
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }
    }
}
