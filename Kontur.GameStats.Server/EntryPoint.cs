using System;
using System.Reflection;
using Fclp;
using Microsoft.Owin.Hosting;

namespace Kontur.GameStats.Server
{
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

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine("Unexepected error occured");
            Console.Error.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        private static void RunServer(Options options)
        {
            try
            {
                using (WebApp.Start<Startup>(options.Prefix))
                {
                    Console.Read();
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
