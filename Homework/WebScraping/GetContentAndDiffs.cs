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
        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            string pathToCookieFile = Path.Combine(pathToAuthorizationDataDirectory, "AuthorizationCookie.json");
            EnsureDirectoryExists(pathToDataDirectory);
            EnsureDirectoryExists(pathToAuthorizationDataDirectory);
            ConfigureLogger();
            await InstallBrowserAsync();

            string cookieFile;
            Cookie cookie;
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

            // Пока этот эффект никак не используется
            //CheckIfNewWeekEffect(pathToDataDirectory);
            for (int i = 0; i < count; i++)
            {
                if (i != 0)
                {
                    last = last.AddDays(-7);
                    next = next.AddDays(-7);
                }
                string jsonContentAsString = GetDataFromServer(cookie, last, next, pathToCookieFile, args).Result;
                string currentWeekPath = Path.Combine(pathToDataDirectory, i.ToString());
                EnsureDirectoryExists(currentWeekPath);

                var lastFile = new DirectoryInfo(currentWeekPath)
                                    .GetFiles()
                                    .OrderByDescending(fi => fi.CreationTime)
                                    .Where(file => file.Name != "diffs.json")
                                    .FirstOrDefault();

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
                            var parsedFile = JsonConvert.DeserializeObject<IEnumerable<(Item, Item)>>(readedDiffsFile);

                            var concatedObj = parsedFile.Concat(result);

                            var dateTimeList = new List<DateTime>();
                            foreach (var items in concatedObj)
                            {
                                string dateTime1;
                                string dateTime2;
                                DateTime dateTime1AsDateTime;
                                DateTime dateTime2AsDateTime;
                                if (items.Item1 != null)
                                {
                                    dateTime1 = items.Item1.datetime_from;
                                    dateTime1AsDateTime = DateTime.ParseExact(dateTime1, "dd.MM.yyyy HH:mm:ss", null);
                                    dateTimeList.Add(dateTime1AsDateTime);
                                }
                                if (items.Item2 != null)
                                {
                                    dateTime2 = items.Item2.datetime_from;
                                    dateTime2AsDateTime = DateTime.ParseExact(dateTime2, "dd.MM.yyyy HH:mm:ss", null);
                                    dateTimeList.Add(dateTime2AsDateTime);
                                }
                            }
                            var maxDateTimeSaved = dateTimeList.Max().AddDays(-7);

                            var item1 = concatedObj.Where(time =>
                            {
                                if (time.Item1 != null)
                                {
                                    var timeAsDateTime = DateTime.ParseExact(time.Item1.datetime_from, "dd.MM.yyyy HH:mm:ss", null);
                                    return timeAsDateTime >= maxDateTimeSaved;
                                }
                                else return false;
                            });
                            var item2 = concatedObj.Where(time =>
                            {
                                if (time.Item2 != null)
                                {
                                    var timeAsDateTime = DateTime.ParseExact(time.Item2.datetime_from, "dd.MM.yyyy HH:mm:ss", null);
                                    return timeAsDateTime >= maxDateTimeSaved;
                                }
                                else return false;
                            });
                            var newData = JsonConvert.SerializeObject(item1.Concat(item2));

                            if (newData.Any())
                            {
                                await File.WriteAllTextAsync(currentDiffsFileToWrite, newData);
                            }
                        }
                    }
                }
                var currentWeekDirectoryFiles = new DirectoryInfo(currentWeekPath)
                    .GetFiles();
                var needToDelete = currentWeekDirectoryFiles.Where(file => file.Name != "diffs.json");
                foreach (var file in needToDelete)
                {
                    File.Delete(file.FullName);
                }
                string fileName = "0.json";
                var path = Path.Combine(currentWeekPath, fileName);

                await File.WriteAllTextAsync(path, jsonContentAsString);
                Log.Information($"Файл создан: {fileName}");
            }
            Log.Information("Скрипт выполнен успешно!");
            Log.CloseAndFlush(); /*отправляем логи на сервер логов*/
        }
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static bool CheckIfNewWeekEffect(string pathToDataDirectory)
        {
            var today = DateTime.Now;
            var monday = today;
            while (monday.DayOfWeek != DayOfWeek.Monday)
            {
                monday = monday.AddDays(-1);
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
                    if(diffToFile > diffToMonday)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static async Task<string> GetDataFromServer(Cookie cookie, DateTime last, DateTime next, string pathToCookieFile, string[] args)
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
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            var client = new HttpClient(handler) { BaseAddress = baseAddress };

            cookieContainer.Add(baseAddress, cookie);
            while (!success && connectionCount < 10)
            {
                try
                {
                    response = await client.GetAsync(link);
                    response.EnsureSuccessStatusCode();
                    jsonContentAsString = await response.Content.ReadAsStringAsync();
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
                    handler = new HttpClientHandler() { CookieContainer = cookieContainer };
                    client = new HttpClient(handler) { BaseAddress = baseAddress };
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
}
