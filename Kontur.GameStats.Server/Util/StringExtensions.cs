using System;
using System.Globalization;

namespace Kontur.GameStats.Server.Util
{
    /// <summary>
    /// Класс с методами расширениями для строк
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Конвертация строки в нужный формат времени
        /// </summary>
        /// <returns>Объект DateTime, соответствующий временной метке</returns>
        public static DateTime ToUtc(this string str)
        {
            return DateTime.Parse(str).ToUniversalTime();
        }

        /// <returns>true - если строка является валидным значением для уникального идентификатора сервера, false - если нет</returns>
        public static bool IsValidEndpoint(this string str)
        {
            var parts = str.Split("-".ToCharArray());
            if (parts.Length != 2) return false;
            if (Uri.CheckHostName(parts[0]) == UriHostNameType.Unknown) return false;
            int port;
            return int.TryParse(parts[1], out port);
        }

        /// <returns>true - если строка является валидной временной меткой, false - если нет</returns>
        public static bool IsValidTimestamp(this string str)
        {
            DateTime date;
            return DateTime.TryParseExact(str, "yyyy-MM-ddTHH:mm:ssZ", null,
                DateTimeStyles.None, out date);
        }
    }
}