using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;

namespace Kontur.GameStats.Server
{
    public class NancyBootstrapper : DefaultNancyBootstrapper
    {
        private const int MaxReportSize = 50;

        public NancyBootstrapper():base()
        {
            Console.WriteLine("flkdf");
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register((c,p) => new ServerStatistics(MaxReportSize));
            container.Register((c,p) => new PlayerStatistics(MaxReportSize));
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
                    (c) =>
                    {
                        c.ResponseProcessors.Clear();
                        c.ResponseProcessors.Add(typeof(JsonProcessor));
                    });
            }
        }

    }
}