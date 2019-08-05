using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace WebScraping
{
    public class MainObject
    {
        public Array[] array { get; set; }
        // array у текущей переменной result in GetData.cs
        internal IEnumerable<(Item, Item)> DeleteTheSameElements(MainObject diffsFile)
        {
            bool equals;
            int count = 0;
            var listTrue = new List<object>();
            foreach (var index1 in array) // in result
            {
                byte[] index1Byte = ObjectToByteArray(index1);
                foreach (var index2 in diffsFile.array) // in Diffs.json file
                {
                    byte[] index2Byte = ObjectToByteArray(index2);
                    equals = StructuralComparisons.StructuralEqualityComparer.Equals(index1Byte, index2Byte);
                    if (equals == true)
                    {
                        listTrue.Add(array[count]);
                    }
                }
                count++;
            }
            var listTrueAsStr = JsonConvert.SerializeObject(listTrue);
            var listTrueToItemItem = JsonConvert.DeserializeObject<IEnumerable<(Item, Item)>>(listTrueAsStr);
            var newArray = array.Where(arr =>
            {
                return arr != listTrueToItemItem;
            });

            var newArrayAsStr = JsonConvert.SerializeObject(newArray);
            var newArrayAsItemItem = JsonConvert.DeserializeObject<IEnumerable<(Item, Item)>>(newArrayAsStr);
            return newArrayAsItemItem;
        }

        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
    [Serializable]
    public class Array
    {
        public Item1 Item1 { get; set; }
        public Item2 Item2 { get; set; }
    }
    [Serializable]
    public class Item1
    {
        //public int number { get; set; }
        public string datetime_from { get; set; }
        public string subject_name { get; set; }
        //public string updateTime { get; set; }
        public string SubjectStatus { get; set; }
        public object HomeworkStatus { get; set; }
        public string content_name { get; set; }
        public Task1[] tasks { get; set; }
    }
    [Serializable]
    public class Task1
    {
        public string task_name { get; set; }
    }
    [Serializable]
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
    [Serializable]
    public class Task2
    {
        public string task_name { get; set; }
    }
}