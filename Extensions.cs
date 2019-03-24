using System;
using MetroStart.Weather.Respnoses;
using Newtonsoft.Json;

namespace MetroStart
{
    public static class Extensions
    {
        public static string Nullable(this string @string) => string.IsNullOrEmpty(@string) ? null : @string;

        // https://stackoverflow.com/a/36191436/24391
        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalSeconds <= 1)
            {
                return $@"{t:s\.ff} seconds";
            }
            if (t.TotalMinutes <= 1)
            {
                return $@"{t:%s} seconds";
            }
            if (t.TotalHours <= 1)
            {
                return $@"{t:%m} minutes";
            }
            if (t.TotalDays <= 1)
            {
                return $@"{t:%h} hours";
            }

            return $@"{t:%d} days";
        }
    }
}