using Newtonsoft.Json;
using PuppeteerSharp;
using Serilog;
using System;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static ClassLibrary.Pathes;
using System.Threading.Tasks;

namespace WebScraping
{
    class GetContentAndDiffs
    {
        private static Cookie cookie;
        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            string pathToCookieFile = Path.Combine(pathToAuthorizationDataDirectory, "AuthorizationCookie.json");
            EnsureDirectoryExists(pathToDataDirectory);
            EnsureDirectoryExists(pathToAuthorizationDataDirectory);
            ConfigureLogger();
            await InstallBrowserAsync();

            string cookieFile;
            if (File.Exists(pathToCookieFile))
            {
                using (var reader = new StreamReader(pathToCookieFile))
                {
                    cookieFile = reader.ReadToEnd();
                }
                var json = JObject.Parse(cookieFile);
                var cookieName = json["Name"].ToString();
                var cookieValue = json["Value"].ToString();
                cookie = new Cookie(cookieName, cookieValue);
            }
            else
            {
                Log.Information("Файл cookie отсутствует. Авторизация");
                cookie = await Authorization.GetCookieByAuthorizationAsync(args, pathToCookieFile, Log.Logger);
            }

            // Соединяемся с электронным дневником и получаем JSON

            DateTime next = DateTime.MinValue;
            DateTime last = DateTime.MinValue;

            DateTime currentNext = DateTime.Now;
            while (currentNext.DayOfWeek != DayOfWeek.Sunday)
            {
                currentNext = currentNext.AddDays(1);
            }
            next = currentNext.Date;

            DateTime currentLast = DateTime.Now;
            while (currentLast.DayOfWeek != DayOfWeek.Monday)
            {
                currentLast = currentLast.AddDays(-1);
            }
            last = currentLast.Date;

            string k = args[2]; // На сколько недель нужна домашка?
            int count = Convert.ToInt32(k);

            var newWeekEffect = CheckIfNewWeekEffect(pathToDataDirectory);
            int i = 0;
            if (newWeekEffect.isEffect)
            {
                for (int j = 0; j < newWeekEffect.numberOfPastWeek; j++)
                {
                    var newWeekJson = GetDataFromServer(last, next, pathToCookieFile, args).Result;
                    WriteToFile(Path.Combine(pathToDataDirectory, "0"), newWeekJson);
                }
                i = Convert.ToInt32(newWeekEffect.numberOfPastWeek);
            }

            for (; i < count; i++)
            {
                Log.Information(i.ToString());
                if (i != 0)
                {
                    last = last.AddDays(-7);
                    next = next.AddDays(-7);
                }
                string jsonContentAsString = GetDataFromServer(last, next, pathToCookieFile, args).Result;
                string currentWeekPath = Path.Combine(pathToDataDirectory, i.ToString());
                EnsureDirectoryExists(currentWeekPath);

                var lastFile = new DirectoryInfo(currentWeekPath)
                                    .GetFiles()
                                    .OrderByDescending(fi => fi.CreationTime)
                                    .Where(file => file.Name == "0.json")
                                    .FirstOrDefault();

                GetDiffsContent(lastFile, currentWeekPath, jsonContentAsString);

                string fileName = "0.json";
                var path = Path.Combine(currentWeekPath, fileName);
                WriteToFile(path, jsonContentAsString);
                Log.Information($"Файл создан: {fileName}");
            }
            Log.Information("Скрипт выполнен успешно!");
            Log.CloseAndFlush(); /*отправляем логи на сервер логов*/
        }

        private static async void GetDiffsContent(FileInfo lastFile, string currentWeekPath, string jsonContentAsString)
        {
            if (lastFile != null && jsonContentAsString != "")
            {
                string readedFile;
                using (var reader = new StreamReader(lastFile.FullName))
                {
                    readedFile = reader.ReadToEnd();
                }
                var oldTree = JsonConvert.DeserializeObject<Rootobject>(readedFile);
                var newTree = JsonConvert.DeserializeObject<Rootobject>(jsonContentAsString);
                var result = oldTree.GetDiffs(newTree);
                // Item1(old) - файлы, Item2(@new) - электронный дневник

                string currentDiffsFileToWrite = Path.Combine(currentWeekPath, "diffs.json");
                if (result.Any())
                {
                    var currentDiffsFileForReading = Directory.GetFiles(currentWeekPath, "diffs.json");
                    if (!currentDiffsFileForReading.Any())
                    {
                        var convertToJson = JsonConvert.SerializeObject(result);
                        await File.WriteAllTextAsync(currentDiffsFileToWrite, convertToJson);
                    }
                    else
                    {
                        string readedDiffsFile;
                        using (var reader = new StreamReader(currentDiffsFileForReading.Single()))
                        {
                            readedDiffsFile = await reader.ReadToEndAsync();
                        }
                        var parsedFile = JsonConvert.DeserializeObject<IEnumerable<(Item old, Item @new)>>(readedDiffsFile);

                        var concatedObj = parsedFile.Concat(result);

                        var dateTimeList = new List<DateTime>();
                        foreach (var homework in concatedObj)
                        {
                            string dateTime;
                            DateTime dateTimeAsDateTime;
                            var notEmptyItem = (homework.old ?? homework.@new);
                            if ((notEmptyItem) != null)
                            {
                                dateTime = notEmptyItem.datetime_from;
                                dateTimeAsDateTime = dateTimeAsDateTime = DateTime.ParseExact(dateTime, "dd.MM.yyyy HH:mm:ss", null);
                                dateTimeList.Add(dateTimeAsDateTime);
                            }
                        }
                        var maxDateTimeSaved = dateTimeList.Max().AddDays(-7);

                        var recentItemsOnly = concatedObj.Where(changedHomework =>
                        {
                            var timeAsDateTime = DateTime.ParseExact((changedHomework.old ?? changedHomework.@new).datetime_from, "dd.MM.yyyy HH:mm:ss", null);
                            return timeAsDateTime >= maxDateTimeSaved;
                        });

                        var newData = JsonConvert.SerializeObject(recentItemsOnly);

                        if (newData.Any())
                        {
                            WriteToFile(currentDiffsFileToWrite, newData);
                        }
                    }
                }
            }
        }
        private static async void WriteToFile(string path, string content)
        {
            await File.WriteAllTextAsync(path, content);
        }
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        private static NewWeekEffectProperties CheckIfNewWeekEffect(string pathToDataDirectory)
        {
            var newWeekEffectObj = new NewWeekEffectProperties();
            newWeekEffectObj.numberOfPastWeek = 0;
            newWeekEffectObj.isEffect = false;

            var today = DateTime.Now;
            var monday = today;
            while (monday.DayOfWeek != DayOfWeek.Monday)
            {
                monday = monday.AddDays(-1).Date;
            }

            var diffToMonday = (today - monday).TotalDays;
            var dirs = Directory.GetDirectories(pathToDataDirectory);
            foreach (var dir in dirs)
            {
                var curFiles = new DirectoryInfo(dir).GetFiles();
                var file = curFiles.Where(file => file.Name == "0.json").SingleOrDefault();
                if (file != null)
                {
                    var fileModifyDate = new FileInfo(file.FullName).LastWriteTime.Date;
                    var diffToFile = (today - fileModifyDate).TotalDays;
                    if (diffToFile > diffToMonday)
                    {
                        var diffFromFileToMonday = diffToFile - diffToMonday;
                        var numberOfPastWeek = Math.Ceiling(diffFromFileToMonday / 7);
                        if(newWeekEffectObj.numberOfPastWeek != 0 && numberOfPastWeek != newWeekEffectObj.numberOfPastWeek)
                        {
                            throw new Exception($"The program can not be runned because the last write time of files is earlier than 1 week ago. Delete the '0.json' files from {pathToDataDirectory} and start the program again.");
                        }
                        newWeekEffectObj.isEffect = true;
                        newWeekEffectObj.numberOfPastWeek = numberOfPastWeek;
                    }
                }
            }
            Log.Information($"Эффект новой недели: {newWeekEffectObj.isEffect}");
            return newWeekEffectObj;
        }
        private static async Task<string> GetDataFromServer(DateTime last, DateTime next, string pathToCookieFile, string[] args)
        {
            string jsonContentAsString = "";
            string lastStr = last.ToString("dd.MM.yyyy");
            string nextStr = next.ToString("dd.MM.yyyy");
            HttpResponseMessage response;
            int connectionCount = 0;
            bool success = false;
            string link = $"/api/journal/lesson/list-by-education?p_limit=100&p_page=1&p_datetime_from={lastStr}&p_datetime_to={nextStr}&p_groups%5B%5D=5881&p_educations%5B%5D=15622";
            var baseAddress = new Uri("https://dnevnik2.petersburgedu.ru");
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(baseAddress, cookie);
            while (!success && connectionCount < 10)
            {
                try
                {
                    using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                    using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                    {
                        response = await client.GetAsync(link);
                        response.EnsureSuccessStatusCode();
                        jsonContentAsString = await response.Content.ReadAsStringAsync();
                    }
                    success = true;
                    return jsonContentAsString;
                }
                catch (HttpRequestException err) when (err.Message.IndexOf("401") > -1)
                {

                    connectionCount++;
                    Log.Information("Файл cookie устарел. Авторизация.");
                    cookie = await Authorization.GetCookieByAuthorizationAsync(args, pathToCookieFile, Log.Logger);
                    cookieContainer = new CookieContainer();
                    cookieContainer.Add(baseAddress, cookie);
                }
            }
            return jsonContentAsString; // will returns "";
        }
        private static void ConfigureLogger()
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341");
            Log.Logger = logConfig.CreateLogger();
        }
        private static async System.Threading.Tasks.Task InstallBrowserAsync()
        {
            // Установка и обновление браузера chromium
            var browserFetcher = new BrowserFetcher();
            var localVersions = browserFetcher.LocalRevisions();

            if (!localVersions.Any() || BrowserFetcher.DefaultRevision != localVersions.Max())
            {
                Log.Information("Downloading chromium...");
                browserFetcher.DownloadProgressChanged += (_, e) => { Console.Write("\r" + e.ProgressPercentage + "%"); };
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                Console.WriteLine(); // Перевод курсора на следующую строку(чтобы небыло "100%Успешно")
            }
        }
    }

    public class NewWeekEffectProperties
    {
        public double numberOfPastWeek;
        public bool isEffect;
    }
}
