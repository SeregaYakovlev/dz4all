using System;
using System.Collections.Generic;
namespace WebScraping
{
    public class MainObject
    {
        public Array[] array { get; set; }
        // array у текущей переменной result in GetData.cs
        internal bool ifEquals(MainObject diffsFile)
        {
            bool equals;
            foreach(var index1 in array) // in result
            {
                foreach(var index2 in diffsFile.array) // in Diffs.json file
                {
                    equals = Equals(index1, index2);
                    if (equals == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class Array
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
        public string SubjectStatus { get; set; }
        public object HomeworkStatus { get; set; }
        //public string content_name { get; set; }
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
        public object HomeworkStatus { get; set; }
        //public string content_name { get; set; }
        public Task2[] tasks { get; set; }
    }

    public class Task2
    {
        public string task_name { get; set; }
    }
}
