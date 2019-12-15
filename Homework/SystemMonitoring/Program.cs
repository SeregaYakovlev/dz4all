using static ClassLibrary.Global;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace SystemMonitoring
{
    class Program
    {
        private static string workDir = @"/root";
        private static string topFileName = @"top.txt";
        private static string pathToTopFile = Path.Combine(workDir, topFileName);
        static async Task Main()
        {
            //await ConfigureLogger();
            string command = @$"cd {workDir}; ";
            command += @$"top -c -o RES -b -n 1 > {topFileName}";
            await ExecuteCommand(command);
        }

        private static async Task ConfigureLogger()
        {
            var logConfig = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.Console()
                   .WriteTo.Seq(ConfigJson.Seq);
            Log.Logger = logConfig.CreateLogger();
        }

        private static async Task ExecuteCommand(string command)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();
                SendFile();
            }
        }

        private static void SendFile()
        {
            var file = new ClassLibrary.File_Manager().OpenFile(pathToTopFile, "Read", null).fileData;
            new ClassLibrary.File_Manager().OpenFile(@"/root/monitor.txt", "Append", file);
            //Log.CloseAndFlush();
        }
    }
}
