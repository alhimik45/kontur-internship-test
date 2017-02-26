using System;
using Kontur.GameStats.Server.Logic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;

namespace Kontur.GameStats.Server
{
    /// <summary>
    /// Конфигурация REST-сервера
    /// </summary>
    public class NancyBootstrapper : DefaultNancyBootstrapper
    {
        /// <summary>
        /// Максимальный размер отчётов в /reports/*
        /// </summary>
        private const int MaxReportSize = 50;

        /// <summary>
        /// Регистрация зависимостей
        /// </summary>
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register((c, p) => new ServerStatistics(MaxReportSize));
            container.Register((c, p) => new PlayerStatistics(MaxReportSize));
            container.Register<StatisticsManager>().AsSingleton();
        }

        /// <summary>
        /// Действия при запуске сервера: увеличение максимальной длины ответа
        /// </summary>
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;
        }

        /// <summary>
        /// Вешаем обработчик ошибок при обработке запроса
        /// </summary>
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

        /// <summary>
        /// Конфигурируем отдавать json по-умолчанию
        /// </summary>
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