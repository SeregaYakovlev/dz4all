using Newtonsoft.Json;
using PuppeteerSharp;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebScraping
{
    class Authorization
    {
        internal static async Task<Cookie> GetCookieByAuthorizationAsync(string[] args, string pathToCookieFile)
        {
            /* ОЧЕНЬ ВАЖНО!
             * В DefaultViewPort стоит параметр null, при котором в развернутом виде КОМПЬЮТЕРНАЯ ВЕРСИЯ,
             * а в окне достаточно маленького размера у некоторых сайтов МОБИЛЬНАЯ ВЕРСИЯ!
             * Следовательно, испольнование CSS-селекторов может быть удачным, а может и нет,
             * в зависимости от размера браузера.
             * В headless-режиме окно маленького размера. */
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false, DefaultViewport = null }))
            {   
                var p = await browser.NewPageAsync();
                /* 10 попыток подключения нужны, если сервер плохой, или интернета вдруг нету.*/

                bool success = false;
                int connectCount = 0;
                while (connectCount < 10 && !success)
                {
                    try
                    {
                        await p.GoToAsync("https://dnevnik2.petersburgedu.ru");
                        success = true;
                        Log.Information("Подключение к сайту {Site}: успешно!", "https://dnevnik2.petersburgedu.ru");
                    }
                    catch (PuppeteerException e)
                    {
                        Log.Error(e, "GoToAsync({Site} failed", "https://dnevnik2.petersburgedu.ru");
                        Log.Information("Подключение к сайту {Site}: Ошибка, повторяю...", "https://dnevnik2.petersburgedu.ru");
                        Log.Information("Попытка № {attempt}", connectCount + 1);
                        await System.Threading.Tasks.Task.Delay(3000);
                        connectCount++;
                    }
                }

                // Кликаем по кнопкам и переходим на страницы

                const string button = "body > app-root > n3-grid > app-login > div > div.notice > div > app-login-form > div > button";
                await p.WaitForSelectorAsync(button);
                await p.ClickAsync(button);

                Log.Information("Первый клик {Button}: успешно!", "Войти с ЕСИА");

                p = await GetPage(browser, "https://esia.gosuslugi.ru");

                // Авторизация
                await p.WaitForSelectorAsync("#mobileOrEmail");
                await p.FocusAsync("#mobileOrEmail");
                await p.Keyboard.TypeAsync(args[0]/*"+79219305001"*/);

                await p.WaitForSelectorAsync("#password");
                await p.FocusAsync("#password");
                await p.Keyboard.TypeAsync(args[1]/*"hereHERE1978!(&"*/);

                await p.WaitForSelectorAsync("#loginByPwdButton > span");
                await p.ClickAsync("#loginByPwdButton > span");

                Log.Information("Авторизация: успешно!");

                /* Куки нужны для того, чтобы сайт меня опознал
                 * при отправке http-запроса на сервер эл. дневника */

                // 10 попыток получения cookie.
                Cookie cookie;
                int forCount = 0;
                do
                {
                    if (forCount > 10) throw new Exception("cookie X-JMT-Token is not present");
                    await System.Threading.Tasks.Task.Delay(1000);
                    var cookies = await p.GetCookiesAsync();
                    cookie = cookies.Where(c => c.Name == "X-JWT-Token").Select(c => new Cookie(c.Name, c.Value)).Single();
                    forCount++;
                }
                while (cookie.Value == "" || cookie == null);

                //Здесь и далее безголовый браузер уже не нужен
                await browser.CloseAsync();

                var cookieAsJson = JsonConvert.SerializeObject(cookie);
                await File.WriteAllTextAsync(pathToCookieFile, cookieAsJson);
                return cookie;
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
            page.DefaultTimeout = 120 * 1000;
            page.DefaultNavigationTimeout = 120 * 1000;
            return page;
        }
    }
}
