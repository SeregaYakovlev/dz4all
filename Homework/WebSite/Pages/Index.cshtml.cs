﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
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
                await UsersCounter.Start(bodyStr);
                Log.Information($"User: {bodyStr} {DateTime.Now}");
            }
        }
    }
}
