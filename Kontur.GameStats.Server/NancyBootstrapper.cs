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
        private readonly LiteDatabase _database;
        private const int MaxReportSize = 50;

        public NancyBootstrapper(string dbName = "data")
        {
            var dbFile = $"{dbName}.db";
            _database = new LiteDatabase(dbFile);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register(_database);
            container.Register((c, p) => new ServerStatistics(container.Resolve<LiteDatabase>(), MaxReportSize));
            container.Register((c, p) => new PlayerStatistics(container.Resolve<LiteDatabase>(), MaxReportSize));
            container.Register<StatisticsManager>().AsSingleton();
        }

        public new void Dispose()
        {
            base.Dispose();
            _database.Dispose();
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
            {
                Console.Error.WriteLine("Exception occured during processing request");
                Console.Error.WriteLine(ex.Message);
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