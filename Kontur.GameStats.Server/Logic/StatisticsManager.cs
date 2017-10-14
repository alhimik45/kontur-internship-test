using System.Collections.Generic;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Logic
{
    /// <summary>
    /// Класс, принимающий запросы от REST-сервера, валидирующий их и
    /// направляющий либо в класс серверной статистики, либо статистики игрока
    /// </summary>
    public class StatisticsManager
    {
        private readonly ServerStatistics _serverStatistics;
        private readonly PlayerStatistics _playerStatistics;

        public StatisticsManager(ServerStatistics serverStatistics, PlayerStatistics playerStatistics)
        {
            _serverStatistics = serverStatistics;
            _playerStatistics = playerStatistics;
        }

        /// <summary>
        /// Advertise-запрос
        /// </summary>
        /// <param name="endpoint">Уникальный идентификатор сервера</param>
        /// <param name="info">Advertise-запрос</param>
        /// <returns>true - если запрос добавлен, false - если не прошел валидацию</returns>
        public bool PutServerInfo(string endpoint, AdvertiseInfo info)
        {
            if (!endpoint.IsValidEndpoint() || info.IsNotFull()) return false;
            _serverStatistics.PutAdvertise(endpoint, info);
            return true;
        }

        /// <summary>
        /// Запрос добавления информации о матче
        /// </summary>
        /// <param name="endpoint">Уникальный идентификатор сервера</param>
        /// <param name="timestamp">Временная метка окончания матча</param>
        /// <param name="info">Информация о матче</param>
        /// <returns>true - если запрос добавлен, false - если не прошел валидацию</returns>
        public bool PutMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            if (info.IsNotFull() || !endpoint.IsValidEndpoint() ||
                !timestamp.IsValidTimestamp() || !_serverStatistics.HasAdvertise(endpoint) ||
                _serverStatistics.GetMatch(endpoint, timestamp) != null)
            {
                return false;
            }

            var matchAdded = _serverStatistics.PutMatch(endpoint, timestamp, info);
            if (!matchAdded) return false;
            _playerStatistics.AddMatchInfo(endpoint, timestamp, info);
            return true;
        }

        /// <summary>
        /// Запрос на получение advertise информации сервера
        /// </summary>
        /// <param name="endpoint">Уникальный идентификатор сервера</param>
        /// <returns>Advertise-запрос или null, если сервер с таким идентификатором не анонсировал себя</returns>
        public AdvertiseInfo GetServerInfo(string endpoint)
        {
            return _serverStatistics.GetAdvertise(endpoint);
        }

        /// <summary>
        /// Запрос на получение информации о матче
        /// </summary>
        /// <param name="endpoint">Уникальный идентификатор сервера</param>
        /// <param name="timestamp">Временная метка окончания матча</param>
        /// <returns>Информацию о матче или null, если такого матча не было</returns>
        public MatchInfo GetMatchInfo(string endpoint, string timestamp)
        {
            return _serverStatistics.GetMatch(endpoint, timestamp);
        }

        /// <summary>
        /// Возвращает информацию обо всех анонсированных серверах
        /// </summary>
        /// <returns>Список с информацией о всех серверах</returns>
        public List<ServersInfoItem> GetAllServersInfo()
        {
            return _serverStatistics.GetAll();
        }

        /// <summary>
        /// Возвращает статистику сервера
        /// </summary>
        /// <param name="endpoint">Уникальный идентификатор сервера</param>
        /// <returns>Статистика сервера или null, если такого сервера нет, или на нём не было сыграно ни одного матча</returns>
        public PublicServerStats GetServerStats(string endpoint)
        {
            return _serverStatistics.GetStats(endpoint);
        }

        /// <summary>
        /// Возвращает статистику об игроке
        /// </summary>
        /// <param name="name">Ник игрока</param>
        /// <returns>Статистику игрока или null, если статистики по данному игроку нет</returns>
        public PublicPlayerStats GetPlayerStats(string name)
        {
            return _playerStatistics.GetStats(name);
        }

        /// <summary>
        /// Возвращает список недавних матчей
        /// </summary>
        /// <param name="count">Количество недавних матчей</param>
        /// <returns>Список недавних матчей</returns>
        public List<RecentMatchesItem> GetRecentMatches(int count)
        {
            return _serverStatistics.GetRecentMatches(count);
        }

        /// <summary>
        /// Возвращает список лучших игроков
        /// </summary>
        /// <param name="count">Количество лучшик игроков</param>
        /// <returns>Список лучших игроков</returns>
        public List<BestPlayersItem> GetBestPlayers(int count)
        {
            return _playerStatistics.GetBestPlayers(count);
        }

        /// <summary>
        /// Возвращает список популярных серверов
        /// </summary>
        /// <param name="count">Количество популярных серверов</param>
        /// <returns>Список популярных серверов</returns>
        public List<PopularServersItem> GetPopularServers(int count)
        {
            return _serverStatistics.GetPopularServers(count);
        }
    }
}