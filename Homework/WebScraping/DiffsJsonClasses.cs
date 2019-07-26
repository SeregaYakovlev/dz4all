using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebScraping
{

    public class Root
    {
        public Items[] items { get; set; }
        
        internal bool ifEquals(IEnumerable newData)
        {
            return newData.Equals(items);
        }
    }

    public class Items
    {
        public Item1 Item1 { get; set; }
        public Item2 Item2 { get; set; }

        
    }

    public class Item1
    {
        //public int number { get; set; }
        public string datetime_from { get; set; }
        public string subject_name { get; set; }
        //public string updateTime { get; set; }
        public object SubjectStatus { get; set; }
        public string HomeworkStatus { get; set; }
        public Task1[] tasks { get; set; }
    }

    public class Task1
    {
        public string task_name { get; set; }
    }

    public class Item2
    {
        //public int number { get; set; }
        public string datetime_from { get; set; }
        public string subject_name { get; set; }
        //public string updateTime { get; set; }
        public string SubjectStatus { get; set; }
        public string HomeworkStatus { get; set; }
        public Task2[] tasks { get; set; }
    }

    public class Task2
    {
        public string task_name { get; set; }
    }

}
