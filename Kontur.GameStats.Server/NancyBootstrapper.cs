using System;
using Kontur.GameStats.Server.Logic;
using LiteDB;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;

namespace Kontur.GameStats.Server
{
    public class NancyBootstrapper : DefaultNancyBootstrapper
    {
        private readonly LiteDatabase _dbPlayers;
        private readonly LiteDatabase _dbServers;
        private const int MaxReportSize = 50;

        public NancyBootstrapper(string dbName = "data")
        {
            _dbPlayers = new LiteDatabase($"{dbName}-players.db");
            _dbServers = new LiteDatabase($"{dbName}-servers.db");
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register((c, p) => new ServerStatistics(_dbServers, MaxReportSize));
            container.Register((c, p) => new PlayerStatistics(_dbPlayers, MaxReportSize));
            container.Register<StatisticsManager>().AsSingleton();
        }

        public new void Dispose()
        {
            base.Dispose();
            _dbPlayers.Dispose();
            _dbServers.Dispose();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
            {
                Console.Error.WriteLine("Exception occured during processing request");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return new TextResponse(HttpStatusCode.InternalServerError, "");
            });
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(
                    c =>
                    {
                        c.ResponseProcessors.Clear();
                        c.ResponseProcessors.Add(typeof(JsonProcessor));
                    });
            }
        }

    }
}