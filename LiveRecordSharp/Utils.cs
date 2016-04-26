using System;
using System.Linq;
using System.Text;

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

        public static string KeepAlpha(this string _string)
        {
            var stringBuilder = new StringBuilder();
            foreach (var _char in _string.Where(t => (t >= 'a' && t <= 'z') || (t >= 'A' && t <= 'Z')))
            {
                stringBuilder.Append(_char);
            }
            return stringBuilder.ToString();
        }
    }
}
