using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ClassLibrary.Pathes;

namespace WebSite.Pages.Shared
{
    public class JSErrorsModel : PageModel
    {
        public void OnGet()
        {

        }
        public void OnPost()
        {
            var body = Request.Body;
            using(var reader = new StreamReader(body))
            {
                var bodyStr = reader.ReadToEnd();
                ErrorWriter(bodyStr);
            }
        }
        private static void ErrorWriter(string bodyStr)
        {
            if (!Directory.Exists(pathToReports))
            {
                Directory.CreateDirectory(pathToReports);
            }

            var currentTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            string fileName = $"Errors.txt";
            var path = Path.Combine(pathToReports, fileName);
            var file = new FileInfo(path);
            bool success = false;
            while (!success)
            {
                try
                {
                    System.IO.File.AppendAllText(file.FullName, currentTime + Environment.NewLine
                        + bodyStr + Environment.NewLine + Environment.NewLine);
                    success = true;
                }
                catch (IOException)
                {
                    Task.Delay(1000);
                }
            }
        }
    }
}