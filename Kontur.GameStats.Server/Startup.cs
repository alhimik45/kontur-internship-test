using Owin;

namespace Kontur.GameStats.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(options => options.Bootstrapper = new NancyBootstrapper());
        }
    }
}