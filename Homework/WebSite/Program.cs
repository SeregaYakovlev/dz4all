using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .UseUrls("http://*:5000")
                .UseWebRoot(@".\WebSite\wwwroot\")
                .Build()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
