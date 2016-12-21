using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var cwd = new DirectoryInfo(Environment.CurrentDirectory);
            var dd = cwd.GetFiles();
            var flvList = cwd.GetFiles().Where(t => t.Extension == ".flv");
            var mp4List = cwd.GetFiles().Where(t => t.Extension == ".mp4");
            foreach (var flvFile in flvList.Where(t=> mp4List.All(d => t.Name.Replace(".flv", "") != d.Name.Replace(".mp4", ""))))
            {
                var tempMp4FilePath = flvFile.Name.Replace(".flv", ".mp4");
                var finalMp4FilePath = flvFile.Name.Replace(".flv", "_final.mp4");
                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i {flvFile.Name} -c:v libx264 -preset slower -profile:v high -level 4.0 -crf 23 -c:a aac -b:a 128k {tempMp4FilePath}",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };

                p.ErrorDataReceived += (o, e) => Console.WriteLine(e.Data);
                p.OutputDataReceived += (o, e) => Console.WriteLine(e.Data);
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();

                var concatFile = new FileInfo("concat.txt");
                if(concatFile.Exists) concatFile.Delete();
                using (var stream = concatFile.CreateText())
                {
                    stream.WriteLine($"file '{tempMp4FilePath}'");
                    for (var i = 1; i <= 20; i++)
                    {
                        stream.WriteLine("file 'out_level.mp4'");
                    }
                }

                p = new Process
                {
                    StartInfo =
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-f concat -i concat.txt -c copy {finalMp4FilePath}",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };

                p.ErrorDataReceived += (o, e) => Console.WriteLine(e.Data);
                p.OutputDataReceived += (o, e) => Console.WriteLine(e.Data);
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
        }
    }
}
