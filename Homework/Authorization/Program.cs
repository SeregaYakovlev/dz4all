using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static ClassLibrary.Global;
using HtmlAgilityPack;

namespace Authorization
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            ConfigureLogger();

            Log.CloseAndFlush();
        }

        private static void ConfigureLogger()
        {
            var logConfig = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.Console()
                   .WriteTo.Seq(ConfigJson.Seq);
            Log.Logger = logConfig.CreateLogger();
        }
    }
}
