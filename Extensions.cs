using System;

namespace MetroStart
{
    public static class Extensions
    {
        public static string Nullable(this string @string) => string.IsNullOrEmpty(@string) ? null : @string;
    }
}
