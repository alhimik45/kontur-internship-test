using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Logic;
using Nancy;
using Nancy.ModelBinding;

namespace Kontur.GameStats.Server
{
    /// <summary>
    /// Класс REST-сервера
    /// </summary>
    public class StatModule : NancyModule
    {
        private readonly StatisticsManager _statisticsManager;

        /// <summary>
        /// Вешаем обработчики http запросов и указываем, как отвечать на возвращаемые данные
        /// (данные отдавать, а на null возвращать другой http статус)
        /// </summary>
        /// <param name="statisticsManager"></param>
        public StatModule(StatisticsManager statisticsManager)
        {
            _statisticsManager = statisticsManager;

            Put["/servers/{endpoint}/info"] = args =>
            {
                var info = this.Bind<AdvertiseInfo>();
                return _statisticsManager.PutServerInfo(args.endpoint, info) ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            };

            Put["/servers/{endpoint}/matches/{timestamp}"] = args =>
            {
                var info = this.Bind<MatchInfo>();
                return _statisticsManager.PutMatchInfo(args.endpoint, args.timestamp, info)
                    ? HttpStatusCode.OK
                    : HttpStatusCode.BadRequest;
            };

            Get["/servers/{endpoint}/info"] = args =>
            {
                var result = _statisticsManager.GetServerInfo(args.endpoint);
                return result ?? HttpStatusCode.NotFound;
            };

            Get["/servers/{endpoint}/matches/{timestamp}"] = args =>
            {
                var result = _statisticsManager.GetMatchInfo(args.endpoint, args.timestamp);
                return result ?? HttpStatusCode.NotFound;
            };

            Get["/servers/info"] = args => _statisticsManager.GetAllServersInfo();

            Get["/servers/{endpoint}/stats"] = args =>
            {
                var stats = _statisticsManager.GetServerStats(args.endpoint);
                return stats ?? HttpStatusCode.NotFound;
            };

            Get["/players/{name}/stats"] = args =>
            {
                var stats = _statisticsManager.GetPlayerStats(args.name);
                return stats ?? HttpStatusCode.NotFound;
            };

            Get["/reports/recent-matches/{count?5}"] = args => _statisticsManager.GetRecentMatches(args.count);

            Get["/reports/best-players/{count?5}"] = args => _statisticsManager.GetBestPlayers(args.count);

            Get["/reports/popular-servers/{count?5}"] = args => _statisticsManager.GetPopularServers(args.count);
        }
    }
}