using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ClassLibrary.Pathes;

namespace WebSite.Pages
{
    public class IndexModel : PageModel
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
                UserWriter(bodyStr);
            }
        }
        private static void UserWriter(string bodyStr)
        {
            if(!Directory.Exists(pathToReports))
            {
                Directory.CreateDirectory(pathToReports);
            }

            var currentTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            string fileName = $"Visits.txt";
            var path = Path.Combine(pathToReports, fileName);
            var file = new FileInfo(path);

            bool success = false;
            while (!success)
            {
                try
                {
                    System.IO.File.AppendAllText(file.FullName, currentTime + " "
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
