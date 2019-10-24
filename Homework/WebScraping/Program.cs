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
using static ClassLibrary.Global;
using System.Threading.Tasks;

namespace WebScraping
{
    class Program
    {
        private static Cookie cookie;
        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            string pathToCookieFile = Path.Combine(Pathes.pathToAuthorizationDataDirectory, "AuthorizationCookie.json");
            EnsureDirectoryExists(Pathes.pathToDataDirectory);
            EnsureDirectoryExists(Pathes.pathToAuthorizationDataDirectory);
            ConfigureLogger();
            await InstallBrowserAsync();

            string cookieFile;
            if (File.Exists(pathToCookieFile))
            {
                var fileManager = new ClassLibrary.File_Manager();
                var result = await fileManager.OpenFile(pathToCookieFile, "Read", null);
                cookieFile = result.fileData;
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

            var monday_sunday = GetMondaySunday();
            var mondayDate = monday_sunday.monday.Date;
            var sundayDate = monday_sunday.sunday.Date;

            string k = args[2]; // На сколько недель нужна домашка?
            int count = Convert.ToInt32(k);

            var dataFromServerList = new SortedList<string, string>();
            var dataFromFileSystemList = new SortedList<string, string>();

            for (int i = 0; i < count; i++)
            {
                Log.Information(i.ToString());
                if (i != 0)
                {
                    mondayDate = mondayDate.AddDays(-7);
                    sundayDate = sundayDate.AddDays(-7);
                }

                string currentWeekPath = Path.Combine(Pathes.pathToDataDirectory, i.ToString());
                EnsureDirectoryExists(currentWeekPath);
                var lastFile = new DirectoryInfo(currentWeekPath)
                                    .GetFiles()
                                    .OrderByDescending(fi => fi.CreationTime)
                                    .Where(file => file.Name == ConfigJson.serverFileName)
                                    .FirstOrDefault();

                if (lastFile != null)
                {
                    var fileManager = new ClassLibrary.File_Manager();
                    var result = await fileManager.OpenFile(pathToCookieFile, "Read", null);
                    var json = result.fileData;
                    var p = JObject.Parse(json);
                    dataFromFileSystemList.Add(p["data"]["Monday"].ToString(), json);
                }

                string jsonContentAsString = GetDataFromServer(mondayDate, sundayDate, pathToCookieFile, args).Result;
                var pa = JObject.Parse(jsonContentAsString);
                dataFromServerList.Add(pa["data"]["Monday"].ToString(), jsonContentAsString);

                string fileName = ConfigJson.serverFileName;
                var path = Path.Combine(currentWeekPath, fileName);
                await WriteToFile(path, jsonContentAsString);
                Log.Information($"Файл создан: {fileName}");
            }
            int dateNumber = 0;
            var orderedDates = dataFromFileSystemList.Keys.Concat(dataFromServerList.Keys).Distinct().OrderByDescending(d => d);
            foreach (var date in orderedDates)
            {
                bool fileExists = dataFromFileSystemList.TryGetValue(date, out var fileJson);
                bool serverContains = dataFromServerList.TryGetValue(date, out var serverJson);
                if (fileExists && serverContains)
                {
                    await GetDiffsContent(fileJson, Path.Combine(Pathes.pathToDataDirectory, dateNumber.ToString()), serverJson);
                }
                dateNumber++;
            }

            Log.Information("Скрипт выполнен успешно!");
            Log.CloseAndFlush(); /*отправляем логи на сервер логов*/
        }

        private static async System.Threading.Tasks.Task GetDiffsContent(string old, string currentWeekPath, string @new)
        {
            if (old != null && @new != null)
            {
                var oldTree = JsonConvert.DeserializeObject<Rootobject>(old);
                var newTree = JsonConvert.DeserializeObject<Rootobject>(@new);
                var result = oldTree.GetDiffs(newTree);
                // Item1(old) - файлы, Item2(@new) - электронный дневник

                string currentDiffsFileToWrite = Path.Combine(currentWeekPath, ConfigJson.diffsFileName);
                if (result.Any())
                {
                    var currentDiffsFileForReading = Directory.GetFiles(currentWeekPath, ConfigJson.diffsFileName);
                    if (!currentDiffsFileForReading.Any())
                    {
                        var convertToJson = JsonConvert.SerializeObject(result);
                        await WriteToFile(currentDiffsFileToWrite, convertToJson);
                    }
                    else
                    {
                        var fileManager = new ClassLibrary.File_Manager();
                        var fileDataObj = await fileManager.OpenFile(currentDiffsFileForReading.Single(), "Read", null);
                        string readedDiffsFile = fileDataObj.fileData;

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
                                dateTimeAsDateTime = dateTimeAsDateTime = DateTime.ParseExact(dateTime, DateTimesFormats.FullDateTime, null);
                                dateTimeList.Add(dateTimeAsDateTime);
                            }
                        }
                        var maxDateTimeSaved = dateTimeList.Max().AddDays(-7);

                        var recentItemsOnly = concatedObj.Where(changedHomework =>
                        {
                            var timeAsDateTime = DateTime.ParseExact((changedHomework.old ?? changedHomework.@new).datetime_from, DateTimesFormats.FullDateTime, null);
                            return timeAsDateTime >= maxDateTimeSaved;
                        });

                        var newData = JsonConvert.SerializeObject(recentItemsOnly);

                        if (newData.Any())
                        {
                            await WriteToFile(currentDiffsFileToWrite, newData);
                        }
                    }
                }
            }
        }
        private static async System.Threading.Tasks.Task WriteToFile(string path, string content)
        {
            var fileManager = new ClassLibrary.File_Manager();
            await fileManager.OpenFile(path, "Write", content);
        }
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        private static async Task<string> GetDataFromServer(DateTime last, DateTime next, string pathToCookieFile, string[] args)
        {
            string jsonContentAsString = null;
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
                    jsonContentAsString = AddJsonTimeSet(jsonContentAsString, last);
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
            return jsonContentAsString; // will returns null;
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
        private static MondaySunday GetMondaySunday()
        {
            DateTime next = DateTime.MinValue;
            DateTime last = DateTime.MinValue;

            DateTime currentNext = DateTime.Now;
            while (currentNext.DayOfWeek != DayOfWeek.Sunday)
            {
                currentNext = currentNext.AddDays(1);
            }
            next = currentNext;

            DateTime currentLast = DateTime.Now;
            while (currentLast.DayOfWeek != DayOfWeek.Monday)
            {
                currentLast = currentLast.AddDays(-1);
            }
            last = currentLast;

            var MondaySundayClass = new MondaySunday();
            MondaySundayClass.monday = last;
            MondaySundayClass.sunday = next;
            return MondaySundayClass;
        }
        private static string AddJsonTimeSet(string jsonContentAsStr, DateTime time)
        {
            var jobj = JObject.Parse(jsonContentAsStr);
            time = ConvertToDate(time);
            jobj["data"][$"{time.DayOfWeek}"] = time;
            var jsonAsStr = JsonConvert.SerializeObject(jobj);
            return jsonAsStr;
        }
        private static DateTime ConvertToDate(DateTime dateTime)
        {
            return dateTime.Date;
        }
    }
    public class MondaySunday
    {
        public DateTime monday;
        public DateTime sunday;
    }

    public class FileModifyTimeInfo
    {
        public bool isSameWeek = false;
        public double weeksOfDiff = 0;
    }

}
