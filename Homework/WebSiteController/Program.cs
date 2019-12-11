using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ClassLibrary.Global;

namespace WebSiteController
{
    class Program
    {
        private static string WebSiteProcessDataFileName = "WebSiteProcessData.json";
        private static string pathToWorkDirectory = @"/root/ServerLinux";
        private static string ProcessID = "ID";
        private static string SystemRestartTime = "SysRestTime";
        static async Task Main()
        {
            await ConfigureLogger();
            var procData = GetWebSiteProcessData();
            /* Если данных к процессу нет(за исключением случая специального удаления), 
             * или они от прошлого сеанса работы системы,
             * то ясно, что сайт не работает.*/
            if (procData == null)
            {
                await RestartWebSite();
                return;
            }
            else
            {
                string lastSysRestartTime = procData[SystemRestartTime].ToString();
                string currentSysRestartTime = GetLastSystemRestartTime().ToString();
                if (lastSysRestartTime != currentSysRestartTime)
                {
                    DeleteWebSiteProcessDataFile();
                    await RestartWebSite();
                    return;
                };
            }

            int procId = Convert.ToInt32(procData[ProcessID]);
            try
            {
                Process.GetProcessById(procId);
                Log.Information("WebSite is working");
                Log.CloseAndFlush();
            }
            catch (ArgumentException err) when (err.Message.IndexOf($"{procId} is not running") > -1)
            {
                // То есть сайт упал, удаляем файл и запускаем снова.
                DeleteWebSiteProcessDataFile();
                await RestartWebSite();
            }
        }

        private static async Task RestartWebSite()
        {
            Log.Information("Restarting the WebSite");
            var command = $@"cd {pathToWorkDirectory}; ";
            command += @"dotnet ./WebSite/WebSite.dll; ";
            Log.CloseAndFlush();
            await ExecuteCommand(command);
        }

        private static async Task ExecuteCommand(string command)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                var procId = proc.Id.ToString();
                var sysRestartTime = GetLastSystemRestartTime();
                await WriteProcessDataToFile(procId, sysRestartTime);
            }
        }
        private static async Task ConfigureLogger()
        {
            var logConfig = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.Console()
                   .WriteTo.Seq(ConfigJson.Seq);
            Log.Logger = logConfig.CreateLogger();
        }

        private static async Task WriteProcessDataToFile(string id, DateTime systemRestartTime)
        {
            var json = new JObject();
            json.Add(ProcessID, id);
            json.Add(SystemRestartTime, systemRestartTime);
            string jsonStr = JsonConvert.SerializeObject(json);
            var fm = new ClassLibrary.File_Manager();
            var path = Path.Combine(pathToWorkDirectory, WebSiteProcessDataFileName);
            fm.OpenFile(path, "Write", jsonStr);
        }

        private static JObject GetWebSiteProcessData()
        {
            var directoryFiles = new DirectoryInfo(pathToWorkDirectory).GetFiles();
            var file = directoryFiles
            .Where(file => file.Name == WebSiteProcessDataFileName)
            .SingleOrDefault();
            if (file == null) return null;

            var fm = new ClassLibrary.File_Manager();
            var path = Path.Combine(pathToWorkDirectory, WebSiteProcessDataFileName);
            string jsonStr = fm.OpenFile(path, "Read", null).fileData;
            var jobj = JObject.Parse(jsonStr);
            return jobj;
        }

        private static void DeleteWebSiteProcessDataFile()
        {
            var directoryFiles = new DirectoryInfo(pathToWorkDirectory).GetFiles();
            var file = directoryFiles
            .Where(file => file.Name == WebSiteProcessDataFileName)
            .SingleOrDefault();

            if (file != null)
            {
                File.Delete(file.FullName);
            }
        }
        private static DateTime GetLastSystemRestartTime()
        {
            var t = TimeSpan.FromMilliseconds(Environment.TickCount64);
            Console.WriteLine(DateTime.Now.Subtract(t));
            return DateTime.Now.Subtract(t);
        }
    }
}