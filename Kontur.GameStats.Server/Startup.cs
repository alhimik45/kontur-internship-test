using Owin;

namespace Kontur.GameStats.Server
{
    /// <summary>
    /// Указываем, как конфигурировать сервер при запуске
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(options => options.Bootstrapper = new NancyBootstrapper());
        }
    }
}