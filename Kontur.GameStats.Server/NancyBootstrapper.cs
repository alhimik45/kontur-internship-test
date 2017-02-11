using System;
using System.IO;
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
        private readonly string _dbFile;
        private const int MaxReportSize = 50;

        public NancyBootstrapper(string dbName = "data")
        {
            _dbFile = $"{dbName}.db";
            File.Delete(_dbFile);
            File.Delete($"{dbName}-journal.db");
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register(new LiteDatabase(_dbFile));
            container.Register((c,p) => new ServerStatistics(container.Resolve<LiteDatabase>(), MaxReportSize));
            container.Register((c,p) => new PlayerStatistics(container.Resolve<LiteDatabase>(), MaxReportSize));
            container.Register<StatisticsManager>().AsSingleton();
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