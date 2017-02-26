using System;
using System.Collections.Concurrent;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс со статистикой одного сервера: публичная - отдаётся по запросу,
    /// остальная - приватная - нужна для обновлений публичной
    /// При добавлении нового поля в данную статистику, его нужно пометить аттрибутом [OptionalField],
    /// и добавить callback для [OnDeserializing], в котором нужно задать значение по-умолчанию.
    /// </summary>
    [Serializable]
    public class ServerStats
    {
        /// <summary>
        /// Публичная статистика
        /// </summary>
        public PublicServerStats PublicStats { get; set; }
        public DateTime LastMatchDay { get; set; }
        public int MatchesInLastDay { get; set; }
        public int TotalPopulation { get; set; }
        public int DaysWithMatches { get; set; } = 1;
        public ConcurrentDictionary<string, int> MapFrequency { get; set; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> GameModeFrequency { get; set; } = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Вычисляет новое значение статистики, основываясь на старой статистике и новых данных
        /// </summary>
        /// <param name="time">Время окончания матча</param>
        /// <param name="info">Информация о матче</param>
        /// <returns>
        /// Новый экземпляр данного класса с обновлённой статистикой,
        /// старый экземпляр не меняется из-за необходимости блокировок в многопоточной среде
        /// </returns>
        public ServerStats CalcNew(DateTime time, MatchInfo info)
        {
            var matchesInLastDay = MatchesInLastDay;
            var daysWithMatches = DaysWithMatches;
            var lastMatchDay = LastMatchDay;

            if (time.Date == LastMatchDay)
            {
                matchesInLastDay += 1;
            }
            else
            {
                daysWithMatches += 1;
                lastMatchDay = time.Date;
                matchesInLastDay = 1;
            }
            GameModeFrequency[info.GameMode] = GameModeFrequency.Get(info.GameMode) + 1;
            MapFrequency[info.Map] = MapFrequency.Get(info.Map) + 1;
            return new ServerStats
            {
                TotalPopulation = TotalPopulation + info.Scoreboard.Count,
                DaysWithMatches = daysWithMatches,
                LastMatchDay = lastMatchDay,
                MatchesInLastDay = matchesInLastDay,
                MapFrequency = MapFrequency,
                GameModeFrequency = GameModeFrequency
            };
        }
    }
}