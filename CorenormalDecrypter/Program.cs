using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

namespace CorenormalDecrypter
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = null, savepath = null;
            bool showHelp = false;
            var options = new OptionSet
            {
                {"i|input=", "path of the original corenormal.swf", t => path = t},
                {"o|output=", "savepath of the decrypted corenormal.swf", t => savepath = t},
                {"h|help", "show this message and exit", v => showHelp = v != null}
            };
            options.Parse(args);

            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Error: The Path is empty.");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (string.IsNullOrEmpty(savepath))
            {
                Console.WriteLine("Error: The Save Path is empty.");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var decryptedBytes = Decrypt.DecryptBytes(File.ReadAllBytes(path));
            File.WriteAllBytes(savepath, decryptedBytes);
        }
    }
}
