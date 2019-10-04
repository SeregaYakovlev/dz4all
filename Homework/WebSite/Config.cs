using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebSite
{
    public class Config
    {
        public readonly static string SETTINGS_FILE =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");

        private static Config instance;

        public static Config Instance
        {
            get { return instance ?? ReadConfig(); }
            private set { instance = value; }
        }

        private static Config ReadConfig()
        {
            var context = File.ReadAllText(SETTINGS_FILE);
            return JsonConvert.DeserializeObject<Config>(context);
        }

        public string Seq { get; set; }
        public WebServerConfig WebServer { get; set; } = new WebServerConfig();
    }

    public class WebServerConfig
    {
        public int Port { get; set; }
    }
}
