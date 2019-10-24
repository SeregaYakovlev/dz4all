using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ClassLibrary.Global;

namespace WebSite.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {

        }
        public async void OnPost()
        {
            var body = Request.Body;
            using (var reader = new StreamReader(body))
            {
                var bodyStr = await reader.ReadToEndAsync();
                UserWriter(bodyStr);
            }
        }
        private static async void UserWriter(string bodyStr)
        {
            if (!Directory.Exists(Pathes.pathToReports))
            {
                Directory.CreateDirectory(Pathes.pathToReports);
            }

            var currentTime = DateTime.Now.ToString(DateTimesFormats.FullDateTime);
            string fileName = $"Visits.txt";
            var path = Path.Combine(Pathes.pathToReports, fileName);
            var content = $"{currentTime}{bodyStr}{Environment.NewLine + Environment.NewLine}";
            var fileManager = new ClassLibrary.File_Manager();
            await fileManager.OpenFile(path, "Append", content);
        }
    }
}
