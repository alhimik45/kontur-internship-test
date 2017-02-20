using System;
using System.Globalization;

namespace Kontur.GameStats.Server.Util
{
    public static class StringExtensions
    {
        public static DateTime ToUtc(this string str)
        {
            return DateTime.Parse(str).ToUniversalTime();
        }

        public static bool IsValidEndpoint(this string str)
        {
            var parts = str.Split("-".ToCharArray());
            if (parts.Length != 2) return false;
            if (Uri.CheckHostName(parts[0]) == UriHostNameType.Unknown) return false;
            int port;
            return int.TryParse(parts[1], out port);
        }

        public static bool IsValidTimestamp(this string str)
        {
            DateTime date;
            return DateTime.TryParseExact(str, "yyyy-MM-ddTHH:mm:ssZ", null,
                DateTimeStyles.None, out date);
        }
    }
}