using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс публичной статистики сервера - именно она отправляется в ответ на запрос
    /// </summary>
    [Serializable]
    public class PublicServerStats
    {
        public int TotalMatchesPlayed { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public int MaximumPopulation { get; set; }
        public double AveragePopulation { get; set; }
        public List<string> Top5GameModes { get; set; } = new List<string>();
        public List<string> Top5Maps { get; set; } = new List<string>();

        /// <summary>
        /// Вычисляет новое значение статистики, основываясь на старой статистике и новых данных
        /// </summary>
        /// <param name="info">Информация о матче</param>
        /// <param name="stats">Приватная статистика сервера</param>
        /// <returns>
        /// Новый экземпляр данного класса с обновлённой статистикой,
        /// старый экземпляр не меняется из-за необходимости блокировок в многопоточной среде
        /// </returns>
        public PublicServerStats CalcNew(MatchInfo info, ServerStats stats)
        {
            return new PublicServerStats
            {
                TotalMatchesPlayed = TotalMatchesPlayed + 1,
                Top5Maps = GetTop5Maps(info, stats),
                Top5GameModes = GetTop5Modes(info, stats),
                MaximumMatchesPerDay = Math.Max(MaximumMatchesPerDay, stats.MatchesInLastDay),
                AverageMatchesPerDay = (double)(TotalMatchesPlayed + 1) / stats.DaysWithMatches,
                MaximumPopulation = Math.Max(MaximumPopulation, info.Scoreboard.Count),
                AveragePopulation = (double)stats.TotalPopulation / (TotalMatchesPlayed + 1)
            };
        }

        /// <summary>
        /// Обновляет топ-5 самых используемых карт
        /// </summary>
        /// <param name="info">Информация о матче</param>
        /// <param name="stats">Статистика сервера</param>
        /// <returns>Новый список топ-5</returns>
        private List<string> GetTop5Maps(MatchInfo info, ServerStats stats)
        {
            return Top5Maps.ToList().UpdateTop(5,
                m => stats.MapFrequency[m],
                m => m,
                info.Map).ToList();
        }

        /// <summary>
        /// Обновляет топ-5 самых используемых игровых режимов
        /// </summary>
        /// <param name="info">Информация о матче</param>
        /// <param name="stats">Статистика сервера</param>
        /// <returns>Новый список топ-5</returns>
        private List<string> GetTop5Modes(MatchInfo info, ServerStats stats)
        {
            return Top5GameModes.ToList().UpdateTop(5,
                m => stats.GameModeFrequency[m],
                m => m,
                info.GameMode).ToList();
        }
    }
}