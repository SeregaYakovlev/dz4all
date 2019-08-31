using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebSite.Pages.ServicePages
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
            string pathToReports = @"C:\Users\Serega\Desktop\Publish\Reports";
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