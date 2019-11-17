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
        public static async Task<SortedList<(DateTime, string), List<JToken>>> ParseDiffsFile(FileInfo diffsFile)
        {
            var sortedList = new SortedList<(DateTime, string), List<JToken>>();

            if (diffsFile != null)
            {
                var fileManager = new ClassLibrary.File_Manager();
                var result = fileManager.OpenFile(diffsFile.FullName, "Read", null);
                var readedDiffsFile = result.fileData;
                var diffsJson = JArray.Parse(readedDiffsFile);

                foreach (var obj in diffsJson)
                {
                    var item = GetNotNullItem(obj);
                    var datetime = DateTime.ParseExact(item["datetime_from"].ToString(), DateTimesFormats.FullDateTime, null);
                    var subject = item["subject_name"].ToString();
                    bool keyContains = sortedList.ContainsKey((datetime, subject));
                    if (!keyContains)
                    {
                        var list = new List<JToken>();
                        list.Add(obj);
                        sortedList.Add((datetime, subject), list);
                    }
                    else
                    {
                        List<JToken> @object = sortedList[(datetime, subject)];
                        @object.Add(obj);
                    }
                }
            }
            return sortedList;
        }

        private static JToken GetNotNullItem(JToken obj)
        {
            foreach (var item in obj)
            {
                if (item.Any(t => t.HasValues)) return item.Single();
            }
            throw new InvalidOperationException($"unexpected case. obj.Count={obj.Count()}");
        }
    }
}
