using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ClassLibrary.Global;

namespace WebSite
{
    public static class DayOfWeekExtention
    {
        private static string[] russianDaysWeek = new string[]
            {
                "Воскресенье",
                "Понедельник",
                "Вторник",
                "Среда",
                "Четверг",
                "Пятница",
                "Суббота"
            };
        public static string ToRussianString(DayOfWeek dayOfWeek)
        {
            return russianDaysWeek[(int)dayOfWeek];
        }
    }
    public static class Methods
    {
        public static async Task<SortedList<DateTime, List<JToken>>> ParseDiffsFile(FileInfo diffsFile)
        {
            var sortedList = new SortedList<DateTime, List<JToken>>();
            
            if (diffsFile != null)
            {
                var fileManager = new ClassLibrary.File_Manager();
                var result = fileManager.OpenFile(diffsFile.FullName, "Read", null);
                var readedDiffsFile = result.Result.fileData;
                var diffsJson = JArray.Parse(readedDiffsFile);

                foreach (var obj in diffsJson)
                {
                    var item1 = obj["Item1"];
                    var item2 = obj["Item2"];
                    var items = new List<JToken>();
                    if (item1.Any()) items.Add(item1);
                    if (item2.Any()) items.Add(item2);
                    foreach (var item in items)
                    {
                        var datetime = DateTime.ParseExact(item["datetime_from"].ToString(), DateTimesFormats.FullDateTime, null);
                        bool keyContains = sortedList.ContainsKey(datetime);
                        if (!keyContains)
                        {
                            var list = new List<JToken>();
                            list.Add(obj);
                            sortedList.Add(datetime, list);
                        }
                        else
                        {
                            List<JToken> @object = sortedList[datetime];
                            @object.Add(obj);
                        }
                    }
                }
            }
            return sortedList;
        }
    }
}
