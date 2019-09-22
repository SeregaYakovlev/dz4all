using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ClassLibrary.Pathes;

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
}
