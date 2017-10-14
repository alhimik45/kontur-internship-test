using System;
using System.Collections.Generic;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс публичной статистики одного игрока - именно она отправляется в ответ на запрос
    /// </summary>
    [Serializable]
    public class PublicPlayerStats
    {
        public int TotalMatchesPlayed { get; set; }
        public int TotalMatchesWon { get; set; }
        public string FavoriteServer { get; set; }
        public int UniqueServers { get; set; }
        public string FavoriteGameMode { get; set; }
        public double AverageScoreboardPercent { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public string LastMatchPlayed { get; set; }
        public double KillToDeathRatio { get; set; }

        /// <summary>
        /// Вычисляет новое значение статистики, основываясь на старой статистике и новых данных
        /// </summary>
        /// <param name="endpoint">Идентификатор сервера</param>
        /// <param name="timestamp">Временная метка окончания матча</param>
        /// <param name="place">Место игрока</param>
        /// <param name="stats">Приватная статистика игрока</param>
        /// <param name="matchInfo">Информация о матче</param>
        /// <param name="info">Результаты игрока в матче</param>
        /// <returns>
        /// Новый экземпляр данного класса с обновлённой статистикой,
        /// старый экземпляр не меняется из-за необходимости блокировок в многопоточной среде
        /// </returns>
        public PublicPlayerStats CalcNew(string endpoint, string timestamp, int place, PlayerStats stats, MatchInfo matchInfo, PlayerMatchInfo info)
        {
            var time = timestamp.ToUtc();
            var totalMatches = TotalMatchesPlayed + 1;
            var favoriteServer = UpdateFavorite(stats.ServerFrequency, endpoint, FavoriteServer);
            var favoriteGameMode = UpdateFavorite(stats.GameModeFrequency, matchInfo.GameMode, FavoriteGameMode);
            return new PublicPlayerStats
            {
                TotalMatchesPlayed = totalMatches,
                LastMatchPlayed = GetLastTimePlayed(timestamp),
                FavoriteServer = favoriteServer,
                FavoriteGameMode = favoriteGameMode,
                UniqueServers = stats.ServerFrequency.Count,
                TotalMatchesWon = TotalMatchesWon + (place == 1 ? 1 : 0),
                AverageScoreboardPercent = stats.TotalScoreboard / totalMatches,
                MaximumMatchesPerDay = Math.Max(MaximumMatchesPerDay, stats.MatchesPerDay[time.Date]),
                AverageMatchesPerDay = (double)totalMatches / stats.MatchesPerDay.Count,
                KillToDeathRatio = (double)stats.TotalKills / stats.TotalDeaths
            };
        }

        /// <summary>
        /// Возвращает более позднее время, выбирая из предыдущего значения и нового
        /// </summary>
        /// <param name="timestamp">Новая временная метка окончания матча</param>
        /// <returns>Временную метку, соответствующую более позднему времени</returns>
        private string GetLastTimePlayed(string timestamp)
        {
            if (LastMatchPlayed != null)
            {
                return timestamp.ToUtc() > LastMatchPlayed.ToUtc() ? timestamp : LastMatchPlayed;
            }
            return timestamp;
        }

        /// <summary>
        /// Обновляет количество использований значения <paramref name="updatedValue"/>
        /// и выбирает наиболее используемое из обновленного значения и старого значения
        /// </summary>
        /// <param name="frequency">Словарь с количеством использований всех значений</param>
        /// <param name="updatedValue">Значение, количество использований которого увеличивается на 1</param>
        /// <param name="oldValue">Старое самое используемое значение</param>
        /// <returns>Строку - ключ словаря - соответствующую наиболее используемому значению</returns>
        private static string UpdateFavorite(IDictionary<string, int> frequency, string updatedValue, string oldValue)
        {
            var currentUsesCount = frequency.Get(updatedValue) + 1;
            frequency[updatedValue] = currentUsesCount;
            if (oldValue == null)
            {
                return updatedValue;
            }
            var favoriveUsesCount = frequency[oldValue];
            return currentUsesCount > favoriveUsesCount ? updatedValue : oldValue;
        }
    }
}