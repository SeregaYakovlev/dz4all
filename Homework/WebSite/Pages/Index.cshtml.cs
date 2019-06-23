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
            var user = Request.Cookies["user"];
            if (user != null)
            {
                CookieWriter(user);
            }
        }
        public void OnPost()
        {
            var user = Request.Cookies["user"];
            if (user != null)
            {
                CookieWriter(user);
            }
        }
        private static void CookieWriter(string user)
        {
            string pathToVisits = @"C:\Users\Serega\Desktop\Publish\HomeworkVisits";

            var currentTime = DateTime.Now.ToString();
            string fileName = $"Visits.txt";
            var path = Path.Combine(pathToVisits, fileName);
            var file = new FileInfo(path);
            System.IO.File.AppendAllText(file.FullName, currentTime + " "
                + user + ";" + Environment.NewLine + Environment.NewLine
                );
        }
    }
}
