using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

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
            string pathToReports = @"C:\Users\Serega\Desktop\Publish\Reports";
            if(!Directory.Exists(pathToReports))
            {
                Directory.CreateDirectory(pathToReports);
            }

            var currentTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            string fileName = $"Visits.txt";
            var path = Path.Combine(pathToReports, fileName);
            var file = new FileInfo(path);
            System.IO.File.AppendAllText(file.FullName, currentTime + " "
                + bodyStr + Environment.NewLine + Environment.NewLine);
        }
    }
}
