using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ClassLibrary.Global;

namespace WebSite.Pages.Shared
{
    public class JSErrorsModel : PageModel
    {
        public void OnGet()
        {

        }
        public async Task OnPost()
        {
            var body = Request.Body;
            using(var reader = new StreamReader(body))
            {
                var bodyStr = await reader.ReadToEndAsync();
                ErrorWriter(bodyStr);
            }
        }
        private static void ErrorWriter(string bodyStr)
        {
            if (!Directory.Exists(Pathes.pathToReports))
            {
                Directory.CreateDirectory(Pathes.pathToReports);
            }

            var currentTime = DateTime.Now.ToString(DateTimesFormats.FullDateTime);
            string fileName = $"Errors.txt";
            var path = Path.Combine(Pathes.pathToReports, fileName);
            var content = $"{currentTime}{Environment.NewLine}{bodyStr}{Environment.NewLine + Environment.NewLine}";
            var fileManager = new ClassLibrary.File_Manager();
            fileManager.OpenFile(path, "Append", content);
        }
    }
}