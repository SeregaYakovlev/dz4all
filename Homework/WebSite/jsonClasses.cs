using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebSite
{
    public class Rootobject
    {
        public Data data { get; set; }
        /*public object[] validations { get; set; }
        public object[] messages { get; set; }
        public object[] debug { get; set; }*/

        /*public override int GetHashCode()
        {
            return data.GetHashCode();
        }*/

        /*public byte[] GetSHA512()
        {
            var stream = new MemoryStream();
            SHA512 shaM = new SHA512Managed();
            byte[] result;
            result = shaM.ComputeHash(stream);
            return result;
        }
        internal void WriteHashToStream(MemoryStream stream)
        {
            
        }*/

    }

    public class Data
    {
        public Item[] items { get; set; }
        /*public int before { get; set; }
        public int current { get; set; }
        public int last { get; set; }
        public int next { get; set; }
        public int total_pages { get; set; }
        public int total_items { get; set; }*/

        /*public override int GetHashCode()
        {
            var hash = 0;
            foreach (var item in items)
            {
                hash ^= item.GetHashCode();
            }
            return hash;
        }*/
    }

    public class Item
    {
        //public Identity identity { get; set; }
        //public int number { get; set; }
        //public string datetime_from { get; set; }
        //public string datetime_to { get; set; }
        //public int subject_id { get; set; }
        public string subject_name { get; set; }
        //public string content_name { get; set; }
        public Task[] tasks { get; set; }
        //public Estimate[] estimates { get; set; }

        /*public override int GetHashCode()
        {
            int hash = 0;
            //hash ^= number.GetHashCode();
            //hash ^= datetime_from.GetHashCode();
            //hash ^= datetime_to.GetHashCode();
            //hash ^= subject_id.GetHashCode();
            hash ^= subject_name.GetHashCode();
            //hash ^= content_name.GetHashCode();
            foreach (var task in tasks)
            {
                hash ^= task.GetHashCode();
            }
            return hash;
        }*/
        
    }

    /*public class Identity
    {
        public int id { get; set; }
        public object uid { get; set; }
    }*/

    public class Task
    {
        public string task_name { get; set; }
        /*public object task_code { get; set; }
        public string task_kind_code { get; set; }
        public string task_kind_name { get; set; }*/

        /*public override int GetHashCode()
        {
            return task_name.GetHashCode();
        }*/
        
    }

    /*public class Estimate
    {
        public string estimate_type_code { get; set; }
        public string estimate_type_name { get; set; }
        public string estimate_value_code { get; set; }
        public string estimate_value_name { get; set; }
        public object estimate_comment { get; set; }
    }*/

}
