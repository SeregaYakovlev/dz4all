using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ClassLibrary.Global;

namespace WebSite
{
    public static class UsersCounter
    {
        public static async Task Start(string user)
        {
            var usersFilePath = Path.Combine(Pathes.pathToReports, "UsersCounter.json");
            bool fileExists = File.Exists(usersFilePath);
            if (fileExists) await AnalizeJson(user, usersFilePath);
            else
            {
                await CreateJson(usersFilePath);
                await AnalizeJson(user, usersFilePath);
            }
        }

        private static async Task AnalizeJson(string user, string usersFilePath)
        {
            var readedFile = new ClassLibrary.File_Manager().OpenFile(usersFilePath, "Read", null).fileData;
            var json = JObject.Parse(readedFile);
            bool UserExists = json.TryGetValue(user, out JToken currentUser);
            if (UserExists)
            {
                int visits = currentUser.Value<int>();
                visits++;
                json[user.ToString()] = visits;
            }
            else
            {
                int numberOfVisits = 1;
                json.Add(new JProperty(user, numberOfVisits));
            }

            int total = json["Total"].Value<int>();
            total++;
            json["Total"] = total;

            var sortedObj = new JObject(
                json.Properties().OrderByDescending(p => (int)p.Value)
            );
            var JSON = JsonConvert.SerializeObject(sortedObj, Formatting.Indented);
            new ClassLibrary.File_Manager().OpenFile(usersFilePath, "Write", JSON);
        }

        private static async Task CreateJson(string usersFilePath)
        {
            var jobj = new JObject();
            jobj["Total"] = 0;
            var json = JsonConvert.SerializeObject(jobj, Formatting.Indented);
            new ClassLibrary.File_Manager().OpenFile(usersFilePath, "Write", json);
        }
    }
}
