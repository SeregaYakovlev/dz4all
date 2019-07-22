using HtmlAgilityPack;
using Newtonsoft.Json;
using PuppeteerSharp;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebScraping
{
    class GetData
    {
        //static JsonSerializerSettings jsonSettings;
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            string pathToDataDirectory = @"C:\Users\Serega\Desktop\Publish\HomeworkData";
            EnsureDirectoryExists(pathToDataDirectory);

            #region Конфигурационная и отладочная хрень
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341");
            Log.Logger = logConfig.CreateLogger();

            //var jsonResolver = new IgnorableSerializerContractResolver();
            //ignore single property
            //jsonResolver.Ignore(typeof(Company), "ExitCode");
            //jsonResolver.Ignore(typeof(Company), "ExitTime");
            // ignore single datatype
            //jsonResolver.Ignore(typeof(System.Diagnostics.Process));
            //jsonSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ContractResolver = jsonResolver };
            #endregion

            // Установка и обновление браузера chronium
            var browserFetcher = new BrowserFetcher();
            var localVersions = browserFetcher.LocalRevisions();

            if (!localVersions.Any() || BrowserFetcher.DefaultRevision != localVersions.Max())
            {
                Log.Information("Downloading chromium...");
                browserFetcher.DownloadProgressChanged += (_, e) => { Console.Write("\r" + e.ProgressPercentage + "%"); };
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                Console.WriteLine(); // Перевод курсора на следующую строку(чтобы небыло "100%Успешно")
            }

            /* ОЧЕНЬ ВАЖНО!
             * В DefaultViewPort стоит параметр null, при котором в развернутом виде КОМПЬЮТЕРНАЯ ВЕРСИЯ,
             * а в окне достаточно маленького размера у некоторых сайтов МОБИЛЬНАЯ ВЕРСИЯ!
             * Следовательно, испольнование CSS-селекторов может быть удачным, а может и нет,
             * в зависимости от размера браузера.
             * В headless-режиме окно маленького размера. */

            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false, DefaultViewport = null }))
            {   // Подписка на события для отладки
                /*
                browser.Closed += Browser_Closed;
                browser.Disconnected += Browser_Disconnected;
                browser.TargetChanged += Browser_TargetChanged;
                browser.TargetCreated += Browser_TargetCreated;
                browser.TargetDestroyed += Browser_TargetDestroyed;
                */

                var p = await browser.NewPageAsync();
                p.DefaultTimeout = 120000;
                /* Дальше повторяем заход на сайт, так как
                 * периодически могут повторяться проблемы
                 * с SSL, в результате чего сайт может
                 * не загрузиться. */

                bool success = false;
                int initiatingCount = 0;
                while (initiatingCount < 10 && !success)
                {
                    try
                    {
                        await p.GoToAsync("https://petersburgedu.ru");
                        success = true;
                        Log.Information("Подключение к сайту {Site}: успешно!", "https://petersburgedu.ru");
                    }
                    catch (PuppeteerException e)
                    {
                        Log.Error(e, "GoToAsync({Site} failed", "https://petersburgedu.ru");
                        Log.Information("Подключение к сайту {Site}: Ошибка, повторяю...", "https://petersburgedu.ru");
                        Log.Information("Попытка № {attempt}", initiatingCount + 1);
                        await System.Threading.Tasks.Task.Delay(3000);
                        initiatingCount++;
                    }
                }

                // Кликаем по кнопкам и переходим на страницы
                p = await GetPage(browser, "https://petersburgedu.ru");

                const string button = "body > div.container-fluid.framework.main-page > section > div.header-img.row-fluid.nopadding > div > div > div > div > div.diary-auth > a:nth-child(3)";
                await p.WaitForSelectorAsync(button);
                await p.ClickAsync(button);

                Log.Information("Первый клик {Button}: успешно!", "Новая версия сайта");

                p = await GetPage(browser, "https://dnevnik2.petersburgedu.ru");

                const string button2 = "body > app-root > n3-grid > app-login > div > div.notice > div > app-login-form > div > button";
                await p.WaitForSelectorAsync(button2);
                await p.ClickAsync(button2);

                Log.Information("Второй клик {Button}: успешно!", "Войти с ЕСИА");

                p = await GetPage(browser, "https://esia.gosuslugi.ru");

                // Авторизация
                await p.WaitForSelectorAsync("#mobileOrEmail");
                await p.FocusAsync("#mobileOrEmail");
                await p.Keyboard.TypeAsync("+79219305001");

                await p.WaitForSelectorAsync("#password");
                await p.FocusAsync("#password");
                await p.Keyboard.TypeAsync("hereHERE1978!(&");

                await p.WaitForSelectorAsync("#loginByPwdButton > span");
                await p.ClickAsync("#loginByPwdButton > span");

                Log.Information("Авторизация: успешно!");

                p = await GetPage(browser, "https://dnevnik2.petersburgedu.ru");

                /*WaitForNavigation срабатывает не всегда,
                 * поэтому надеямся, что загрузились и куку получили.
                 */
                try
                {
                    await p.WaitForNavigationAsync(new NavigationOptions
                    {
                        WaitUntil = new[] {
                        WaitUntilNavigation.Networkidle0
                    },
                        Timeout = 60000
                    });
                }
                catch (TimeoutException)
                {
                    Log.Error("WaitForNavigation failed");
                }

                /* Куки нужны для того, чтобы сайт меня опознал
                 * при отправке http-запроса на сервер эл. дневника */
                var cookies = await p.GetCookiesAsync();
                Cookie cookie;
                do
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                    cookie = cookies.Where(c => c.Name == "X-JWT-Token").Select(c => new Cookie(c.Name, c.Value)).Single();
                }
                while (cookie.Value == "");

                //Здесь и далее безголовый браузер уже не нужен
                await browser.CloseAsync();

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

                string k = "12"; // На сколько недель нужна домашка?
                int count = Convert.ToInt32(k);

                for (int i = 0; i < count; i++)
                {
                    if (i != 0)
                    {
                        last = last.AddDays(-7);
                        next = next.AddDays(-7);
                    }
                    string lastStr = last.ToString("dd.MM.yyyy");
                    string nextStr = next.ToString("dd.MM.yyyy");

                    string link = $"/api/journal/lesson/list-by-education?p_limit=100&p_page=1&p_datetime_from={lastStr}&p_datetime_to={nextStr}&p_groups%5B%5D=5881&p_educations%5B%5D=15622";

                    var baseAddress = new Uri("https://dnevnik2.petersburgedu.ru");
                    var cookieContainer = new CookieContainer();
                    string jsonContentAsAString;
                    using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                    using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                    {
                        cookieContainer.Add(baseAddress, cookie);
                        var result = await client.GetAsync(link);
                        result.EnsureSuccessStatusCode();
                        jsonContentAsAString = await result.Content.ReadAsStringAsync();

                    }
                    string currentWeekPath = Path.Combine(pathToDataDirectory, i.ToString());
                    EnsureDirectoryExists(currentWeekPath);
                    var files = new DirectoryInfo(currentWeekPath).GetFiles();
                    string fileName = $"{files.Count()}.json";
                    var path = Path.Combine(currentWeekPath, fileName);

                    var lastFile = new DirectoryInfo(currentWeekPath)
                                        .GetFiles()
                                        .OrderByDescending(fi => fi.CreationTime)
                                        .Where(file => file.Name != "diffs.json")
                                        .FirstOrDefault();

                    if (lastFile != null)
                    {
                        string readedFile;
                        using (var reader = new StreamReader(lastFile.FullName))
                        {
                            readedFile = reader.ReadToEnd();
                        }
                        var oldTree = JsonConvert.DeserializeObject<Rootobject>(readedFile);
                        var newTree = JsonConvert.DeserializeObject<Rootobject>(jsonContentAsAString);
                        var result = oldTree.GetDiffs(newTree);
                        // Item1(old) - файлы, Item2(@new) - электронный дневник

                        string currentDiffsFile = Path.Combine(currentWeekPath, "diffs.json");
                        if (result.Any())
                        {
                            var currentDiffsDirectoryFile = Directory.GetFiles(currentWeekPath, "diffs.json");
                            if (!currentDiffsDirectoryFile.Any())
                            {
                                var convertToJson = JsonConvert.SerializeObject(result);
                                await File.WriteAllTextAsync(currentDiffsFile, convertToJson);
                            }
                            else
                            {
                                string readedDiffsFile;

                                using (var reader = new StreamReader(currentDiffsDirectoryFile.Single()))
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

                                var item1 = parsedFile.Where(time =>
                                {
                                    if (time.Item1 != null)
                                    {
                                        var timeAsDateTime = DateTime.ParseExact(time.Item1.datetime_from, "dd.MM.yyyy HH:mm:ss", null);
                                        return timeAsDateTime >= maxDateTimeSaved;
                                    }
                                    else return false;
                                });
                                var item2 = parsedFile.Where(time =>
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
                                    var newJson = JsonConvert.SerializeObject(newData);
                                    await File.WriteAllTextAsync(currentDiffsFile, newJson);
                                }
                            }
                        }
                    }
                    await File.WriteAllTextAsync(path, jsonContentAsAString);
                    Log.Information($"Файл создан: {fileName}");
                }
                Log.Information("Скрипт выполнен успешно!");
                Log.CloseAndFlush(); /*отправляем логи на сервер логов*/
            }
        }
        private static async Task<Page> GetPage(Browser browser, string url)
        {
            Page[] pages;
            do
            {
                pages = await browser.PagesAsync();
                await System.Threading.Tasks.Task.Delay(1000);
            } while (!pages.Any(p2 => p2.Url.StartsWith(url)));
            var page = pages.Single(p2 => p2.Url.StartsWith(url));
            page.DefaultNavigationTimeout = 120 * 1000;
            return page;
        }

        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        #region Методы для отладочной хрени
        /*
        private static void Browser_TargetDestroyed(object sender, TargetChangedArgs e)
        {
            Log.Debug("{Event} {Args}", MethodBase.GetCurrentMethod().Name, JsonConvert.SerializeObject(e, Formatting.Indented, jsonSettings));
        }

        private static void Browser_TargetCreated(object sender, TargetChangedArgs e)
        {
            Log.Debug("{Event} {Args}", MethodBase.GetCurrentMethod().Name, JsonConvert.SerializeObject(e, Formatting.Indented, jsonSettings));
        }

        private static void Browser_TargetChanged(object sender, TargetChangedArgs e)
        {
            Log.Debug("{Event} {Args}", MethodBase.GetCurrentMethod().Name, JsonConvert.SerializeObject(e, Formatting.Indented, jsonSettings));
        }

        private static void Browser_Disconnected(object sender, EventArgs e)
        {
            Log.Debug("{Event} {Args}", MethodBase.GetCurrentMethod().Name, JsonConvert.SerializeObject(e, Formatting.Indented, jsonSettings));
        }

        private static void Browser_Closed(object sender, EventArgs e)
        {
            Log.Debug("{Event} {Args}", MethodBase.GetCurrentMethod().Name, JsonConvert.SerializeObject(e, Formatting.Indented, jsonSettings));
        }
        */
        #endregion
    }
}
