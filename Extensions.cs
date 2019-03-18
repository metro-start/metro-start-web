using MetroStart.Weather.Respnoses;
using Newtonsoft.Json;

namespace MetroStart
{
    public static class Extensions
    {
        public static string Nullable(this string @string) => string.IsNullOrEmpty(@string) ? null : @string;
    }
}
