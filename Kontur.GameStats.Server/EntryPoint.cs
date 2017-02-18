using System;
using System.Collections.Concurrent;
using System.Reflection;
using Fclp;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Util;
using Microsoft.Owin.Hosting;

namespace Kontur.GameStats.Server
{
    public class EntryPoint
    {
        class S
        {
            public int Id { get; set; }
            public string V { get; set; }
        }

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

            //var db = new LiteDatabase("test.db");
            //var c = new PersistentDictionary<int, int>(db, "kek");
            //var tt = new List<Thread>();
            //for (int i = 0; i < 50; i++)
            //{
            //    var ii = i;
            //    var t = new Thread(() =>
            //    {
            //        using (var trans = db.BeginTrans())
            //        {
            //            var r = new Random();
            //            for (int j = 0; j < 5000; j++)
            //            {
            //                c[r.Next(500)] = r.Next();
            //                //coll.Upsert(new S() { Id = r.Next(100), V = "str" });
            //                //Thread.Sleep(r.Next(1500));
            //            }
            //            trans.Commit();
            //        }
            //    });
            //    t.Start();
            //    tt.Add(t);
            //}
            //foreach (var thread in tt)
            //{
            //    thread.Join();
            //}

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
            }
        }

        private class Options
        {
            public string Prefix { get; set; }
        }
    }
}
