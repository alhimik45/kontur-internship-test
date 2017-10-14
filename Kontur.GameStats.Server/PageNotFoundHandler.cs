using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses;

namespace Kontur.GameStats.Server
{
    /// <summary>
    /// Клаяя для Nancy, чтобы он отправлял пустой ответ при 404 статусе
    /// </summary>
    public class PageNotFoundHandler : IStatusCodeHandler
    {
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.NotFound;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.Response = new TextResponse(HttpStatusCode.NotFound, "");
        }
    }
}