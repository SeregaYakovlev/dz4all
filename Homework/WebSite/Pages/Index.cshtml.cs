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
        public async Task OnPost()
        {
            var body = Request.Body;
            using (var reader = new StreamReader(body))
            {
                var bodyStr = await reader.ReadToEndAsync();

                if (!Directory.Exists(Pathes.pathToReports))
                {
                    Directory.CreateDirectory(Pathes.pathToReports);
                }

                await UsersCounter.Start(bodyStr);
            }
        }
    }
}
