﻿using static ClassLibrary.Global;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace WebSite
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq(ConfigJson.Seq)
            .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                        {
                            // Set properties and call methods on options
                            serverOptions.ListenAnyIP(ConfigJson.WebServer.HTTP_Port);
                            serverOptions.ListenAnyIP(ConfigJson.WebServer.HTTPS_Port, listenOptions =>
                            {
                                var pathToPfxCertificate = ConfigJson.WebServer.HTTPS.PathForPfxFile;
                                listenOptions.UseHttps(pathToPfxCertificate, "secret");
                            });
                        })
                        .UseStartup<Startup>()
                        .UseWebRoot(ConfigJson.WebServer.WebRoot)
                        .UseSerilog();
                });
    }
}