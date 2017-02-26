using System;
using System.Collections.Concurrent;
using Kontur.GameStats.Server.Util;

// ReSharper disable PossibleInvalidOperationException
namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс со статистикой одного игрока: публичная - отдаётся по запросу,
    /// остальная - приватная - нужна для обновлений публичной
    /// При добавлении нового поля в данную статистику, его нужно пометить аттрибутом [OptionalField],
    /// и добавить callback для [OnDeserializing], в котором нужно задать значение по-умолчанию.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        /// <summary>
        /// Публичная статистика
        /// </summary>
        public PublicPlayerStats PublicStats { get; set; }
        public double TotalScoreboard { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public ConcurrentDictionary<DateTime, int> MatchesPerDay { get; set; } = new ConcurrentDictionary<DateTime, int>();
        public ConcurrentDictionary<string, int> ServerFrequency { get; set; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> GameModeFrequency { get; set; } = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Вычисляет новое значение статистики, основываясь на старой статистике и новых данных
        /// </summary>
        /// <param name="time">Время окончания матча</param>
        /// <param name="place">Место игрока</param>
        /// <param name="matchInfo">Информация о матче</param>
        /// <param name="info">Результаты игрока в матче</param>
        /// <returns>
        /// Новый экземпляр данного класса с обновлённой статистикой,
        /// старый экземпляр не меняется из-за необходимости блокировок в многопоточной среде
        /// </returns>
        public PlayerStats CalcNew(DateTime time, int place, MatchInfo matchInfo, PlayerMatchInfo info)
        {
            var totalPlayers = matchInfo.Scoreboard.Count;
            var playersBelowCurrent = totalPlayers - place;
            double scoreboardPercent;
            if (totalPlayers == 1)
            {
                scoreboardPercent = 100;
            }
            else
            {
                scoreboardPercent = (double)playersBelowCurrent / (totalPlayers - 1) * 100;
            }
            MatchesPerDay[time.Date] = MatchesPerDay.Get(time.Date) + 1;
            return new PlayerStats
            {
                TotalDeaths = TotalDeaths + info.Deaths.Value,
                TotalKills = TotalKills + info.Kills.Value,
                TotalScoreboard = TotalScoreboard + scoreboardPercent,
                MatchesPerDay = MatchesPerDay,
                ServerFrequency = ServerFrequency,
                GameModeFrequency = GameModeFrequency
            };
        }
    }
}