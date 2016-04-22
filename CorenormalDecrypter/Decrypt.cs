using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorenormalDecrypter
{
    public static class Decrypt
    {
        private static string Key { get; } = "dkrltl0%4*@jrky#@$";

        public static byte[] EncryptBytes(byte[] input)
        {
            return ParseBytes(input, 1);
        }

        public static byte[] DecryptBytes(byte[] input)
        {
            return ParseBytes(input, 0);
        }

        private static byte[] ParseBytes(byte[] input, int flag)
        {
            var loc5 = 0;
            var loc6 = 0;
            var loc7 = 0;
            using (var loc4 = new MemoryStream())
            {
                while (loc7 < input.Length)
                {
                    if (loc5 >= Key.Length)
                    {
                        loc5 = 0;
                        loc6++;
                        if (loc6 >= 50)
                        {
                            loc4.Write(input, 50*Key.Length, input.Length - 50*Key.Length);
                            break;
                        }
                    }
                    if (flag == 1)
                    {
                        loc4.WriteByte((byte) (input[loc7] + Key[loc5]));
                    }
                    else if (flag == 0)
                    {
                        loc4.WriteByte((byte) (input[loc7] - Key[loc5]));
                    }
                    loc7++;
                    loc5++;
                }
                return loc4.ToArray();
            }
        }
    }
}
