using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Logic
{
    /// <summary>
    /// Класс, управляющий статистикой игроков
    /// </summary>
    public class PlayerStatistics
    {
        private const string BestPlayersFilename = "Reports/BestPlayers";
        private readonly int _maxReportSize;

        private readonly PersistentDictionary<PlayerStats> _stats;
        private readonly List<BestPlayersItem> _bestPlayers;

        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public PlayerStatistics(int maxReportSize)
        {
            _maxReportSize = maxReportSize;

            _stats = new PersistentDictionary<PlayerStats>("Players", "PlayerStats", noKeyFolder: true);

            Directory.CreateDirectory("Reports");
            _bestPlayers = Collections.Load<BestPlayersItem>(BestPlayersFilename);
        }

        /// <summary>
        /// Пересчитывает статистику на основе информации о новом матче
        /// </summary>
        /// <param name="endpoint">Уникальный идентификатор сервера</param>
        /// <param name="timestamp">Временная метка окончания матча</param>
        /// <param name="info">Информация о матче</param>
        public void AddMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            for (var i = 0; i < info.Scoreboard.Count; ++i)
            {
                CalcPlayerStats(endpoint.ToLower(), timestamp, i, info);
            }
        }

        /// <returns>Статистику игрока или null, если статистики по данному игроку нет</returns>
        public PublicPlayerStats GetStats(string name)
        {
            var playerName = name.ToLower();
            lock (_locks.GetOrAdd(playerName, _ => new object()))
            {
                var stats = _stats[playerName];
                if (stats != null)
                {
                    return stats.PublicStats;
                }
                object _;
                //Удаляем объект блокировки, если такого игрока нет - исключает возможность исчерпания памяти из-за
                //большого количества некорректных запросов
               _locks.TryRemove(playerName, out _);
                return null;
            }
        }

        /// <param name="count">Количество лучшик игроков</param>
        /// <returns>Список лучших игроков</returns>
        public List<BestPlayersItem> GetBestPlayers(int count)
        {
            lock (_bestPlayers)
            {
                return _bestPlayers.Take(count).ToList();
            }
        }

        /// <summary>
        /// Обновляет статистику игрока, основываясь на информации о новом матче
        /// </summary>
        /// <param name="endpoint">Уникальный идентификатор сервера</param>
        /// <param name="timestamp">Временная метка окончания матча</param>
        /// <param name="index">Индекс игрока в списке игроков</param>
        /// <param name="matchInfo">Информация о матче</param>
        private void CalcPlayerStats(string endpoint, string timestamp, int index, MatchInfo matchInfo)
        {
            var info = matchInfo.Scoreboard[index];
            var playerName = info.Name.ToLower();
            var place = index + 1;

            lock (_locks.GetOrAdd(playerName, _ => new object()))
            {
                PlayerStats oldStats;
                if (!_stats.TryGetValue(playerName, out oldStats))
                {
                    oldStats = new PlayerStats { PublicStats = new PublicPlayerStats() };
                }

                var newStats = oldStats.CalcNew(timestamp.ToUtc(), place, matchInfo, info);

                newStats.PublicStats = oldStats.PublicStats.CalcNew(endpoint, timestamp, place, newStats, matchInfo, info);

                if (newStats.TotalDeaths != 0 && newStats.PublicStats.TotalMatchesPlayed >= 10)
                {
                    UpdateBestPlayersReport(playerName, newStats.PublicStats);
                }
                _stats[playerName] = newStats;
            }
        }

        /// <summary>
        /// Обновляет список лучших игроков, основываясь на соотношении убийств к смертям
        /// </summary>
        /// <param name="name">Ник игрока</param>
        /// <param name="info">Статистика игрока</param>
        private void UpdateBestPlayersReport(string name, PublicPlayerStats info)
        {
            lock (_bestPlayers)
            {
                _bestPlayers.UpdateTop(_maxReportSize,
                    bp => bp.KillToDeathRatio,
                    bp => bp.Name,
                    new BestPlayersItem
                    {
                        Name = name,
                        KillToDeathRatio = info.KillToDeathRatio
                    });
                Collections.Save(_bestPlayers, BestPlayersFilename);
            }
        }
    }
}