using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Fclp;
using Microsoft.Owin.Hosting;

namespace Kontur.GameStats.Server
{
    /// <summary>
    /// Точка входа в приложение
    /// </summary>
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            var commandLineParser = new FluentCommandLineParser<Options>();

            commandLineParser
                .Setup(options => options.Prefix)
                .As("prefix")
                .SetDefault("http://+:8080/")
                .WithDescription("HTTP prefix to listen on");

            commandLineParser
                .SetupHelp("h", "help")
                .WithHeader($"{AppDomain.CurrentDomain.FriendlyName} [--prefix <prefix>]")
                .Callback(text => Console.WriteLine(text));

            if (commandLineParser.Parse(args).HelpCalled)
                return;

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            RunServer(commandLineParser.Object);
        }

        /// <summary>
        /// Метод вызываемый при перехвате необработанного исключения
        /// </summary>
        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine("Unexepected error occured");
            Console.Error.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        /// <summary>
        /// Запуск REST-сервера
        /// </summary>
        private static void RunServer(Options options)
        {
            try
            {
                using (WebApp.Start<Startup>(options.Prefix))
                {
	                var p =Process.Start(new ProcessStartInfo(@"cmd.exe")
	                {
		                Arguments = @"/C node E:\swap\t\node_modules\.bin\bench-rest -n 100 -c 20 E:\swap\t\i.js",
						RedirectStandardOutput = true,
						UseShellExecute = false
					});
	               p.StandardOutput.BaseStream.CopyToAsync(Console.OpenStandardOutput());
	                p.WaitForExit();
	               // Console.WriteLine(p.StandardOutput.ReadToEnd());
                }
            }
            catch (TargetInvocationException e)
            {
                Console.Error.WriteLine("Error starting server");
                Console.Error.WriteLine(e.InnerException?.Message ?? e.Message);
                Console.Error.WriteLine(e.InnerException?.StackTrace ?? e.StackTrace);
            }
        }

        private class Options
        {
            public string Prefix { get; set; }
        }
    }
}
