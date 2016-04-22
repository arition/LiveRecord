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
    }
}
