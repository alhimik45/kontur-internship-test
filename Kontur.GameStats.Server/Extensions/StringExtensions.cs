using System;

namespace Kontur.GameStats.Server.Extensions
{
    public static class StringExtensions
    {
        public static DateTime ToUtc(this string str)
        {
            return DateTime.Parse(str).ToUniversalTime();
        }
    }
}