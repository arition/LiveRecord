using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveRecordSharp
{
    public static class Utils
    {
        public static int ToUnixTimeStamp(this DateTime dateTime)
        {
            var t = dateTime - new DateTime(1970, 1, 1);
            return (int) t.TotalSeconds;
        }

        public static string Replace(this string _string, char[] oldChars, string newString)
        {
            var stringBuilder = new StringBuilder(_string);
            foreach (var oldChar in oldChars.Select(t => t.ToString()))
            {
                stringBuilder.Replace(oldChar, newString);
            }
            return stringBuilder.ToString();
        }
    }
}
