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

            //Process currentProc = Process.GetCurrentProcess();
            //Console.WriteLine("before " + GC.GetTotalMemory(true));
            //Console.WriteLine("before " + currentProc.PrivateMemorySize64);

            //var mat = new ConcurrentDictionary<string, ConcurrentDictionary<string, MatchInfo>>();
            //var pl = new ConcurrentDictionary<string, PlayerStats>();
            //for (int i = 0; i < 10000; i++)
            //{
            //    var tt = new PlayerStats
            //    {
            //        PublicStats = new PublicPlayerStats
            //        {
            //            KillToDeathRatio = 353445,
            //            AverageMatchesPerDay = 43543,
            //            FavoriteServer = "123456784901234567890123456789012345678901234567890" + i,
            //            FavoriteGameMode = "1223",
            //            TotalMatchesPlayed = 2334,
            //            LastMatchPlayed = "123152312312312312",
            //            AverageScoreboardPercent = 5353,
            //            UniqueServers = 4534,
            //            MaximumMatchesPerDay = 635,
            //            TotalMatchesWon = 435635
            //        },
            //        TotalDeaths = 5634,
            //        TotalKills = 534,
            //        TotalScoreboard = 453,
            //        GameModeFrequency = new ConcurrentDictionary<string, int>
            //        {
            //            ["wer"] = 435,
            //            ["1wer"] = 435,
            //            ["w2er"] = 435,
            //            ["we3r"] = 435,
            //            ["wer4"] = 435,
            //            ["5wer"] = 435,
            //            ["w6er"] = 435,
            //            ["we7r"] = 435,
            //            ["8wer"] = 435,
            //            ["w9er"] = 435,
            //        },
            //        MatchesPerDay = new ConcurrentDictionary<DateTime, int>
            //        {
            //            [DateTime.Now] = 3454,
            //            [DateTime.Now.AddDays(-1)] = 3454+i,
            //            [DateTime.Now.AddDays(-2)] = 3454,
            //            [DateTime.Now.AddDays(-3)] = 3454,
            //            [DateTime.Now.AddDays(-4)] = 3454,
            //            [DateTime.Now.AddDays(-5)] = 3454,
            //            [DateTime.Now.AddDays(-6)] = 3454,
            //            [DateTime.Now.AddDays(-7)] = 3454,
            //            [DateTime.Now.AddDays(-8)] = 3454,
            //            [DateTime.Now.AddDays(-9)] = 3454,
            //            [DateTime.Now.AddDays(-10)] = 3454,
            //            [DateTime.Now.AddDays(-11)] = 3454,
            //            [DateTime.Now.AddDays(-12)] = 3454,
            //            [DateTime.Now.AddDays(-13)] = 3454,
            //            [DateTime.Now.AddDays(-14)] = 3454,
            //        },
            //        ServerFrequency = new ConcurrentDictionary<string, int>()
            //    };
            //    for (int j = 0; j < 5000; j++)
            //    {
            //        tt.ServerFrequency[i.ToString()] = 34635+j;
            //    }
            //    pl[i.ToString()] = tt;
            //}
            //Console.WriteLine("koe");
            //for (int i = 0; i < 140000; i++)
            //{
            //    var mm = new MatchInfo
            //    {
            //        GameMode = "dfg",
            //        Scoreboard = new List<PlayerMatchInfo>(),
            //        Map = "12345678901234534567890123456789012345678901234567890" + i,
            //        FragLimit = 355,
            //        TimeLimit = 4354,
            //        TimeElapsed = 35.652
            //    };
            //    for (int j = 0; j < 100; j++)
            //    {
            //        mm.Scoreboard.Add(new PlayerMatchInfo
            //        {
            //            Name = "123443545678901234567890123456789012345678901234567890" + i,
            //            Deaths = 2345,
            //            Kills = i,
            //            Frags = i+i
            //        });
            //    }
            //    mat.GetOrAdd(i.ToString(), _ => new ConcurrentDictionary<string, MatchInfo>())[i.ToString()] = mm;
            //}

            //Console.WriteLine("after  " + GC.GetTotalMemory(true));
            //Console.WriteLine("after  " + currentProc.PrivateMemorySize64);


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
