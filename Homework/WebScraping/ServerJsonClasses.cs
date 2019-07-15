using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WebScraping
{
    public class Rootobject
    {
        public Data data { get; set; }

        internal IEnumerable<(Item, Item)> GetDiffs(Rootobject newTree)
        {
            return data.GetDiffs(newTree.data);
        }
        /*public object[] validations { get; set; }
public object[] messages { get; set; }
public object[] debug { get; set; }*/
    }

    public class Data
    {
        public Item[] items { get; set; }

        internal IEnumerable<(Item, Item)> GetDiffs(Data data)
        {

            var join = items.FullOuterJoin(data.items, old => (old.subject_name, old.datetime_from), @new => (@new.subject_name, @new.datetime_from), (old, @new, key) => (old, @new));
            foreach (var joinEntry in join)
            {
                if (joinEntry.old == null)
                {
                    joinEntry.@new.subject_type("new");
                    yield return joinEntry;// "new: " + joinEntry.@new.subject_name.ToString();
                }
                else if (joinEntry.@new == null)
                {
                    joinEntry.old.subject_type("deleted");
                    yield return joinEntry;// "delete: " + joinEntry.old.subject_name.ToString();
                }
                else
                {

                    var diffs = joinEntry.old.GetDiffs(joinEntry.@new);
                    if (diffs.Any())
                    {
                        foreach (var diff in diffs)
                        {
                            if (diff.task_name.Any())
                            {
                                joinEntry.old.homework_type("changed");
                                joinEntry.@new.homework_type("changed");
                            };
                        }

                        yield return joinEntry;
                    }
                }
            }
        }
        /*public int before { get; set; }
public int current { get; set; }
public int last { get; set; }
public int next { get; set; }
public int total_pages { get; set; }
public int total_items { get; set; }*/


    }

    public class Item
    {
        //public Identity identity { get; set; }
        public int number { get; set; }
        //private string dateTotal;

        public string datetime_from { get; set; }
        //public string datetime_to { get; set; }
        //public int subject_id { get; set; }
        public string subject_name { get; set; }
        public string updateTime
        {
            get
            {
                return DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            }
        }
        private string subjectStatus;
        private string homeworkStatus;
        public string SubjectStatus
        {
            get
            {
                return subjectStatus;
            }
        }
        public string HomeworkStatus
        {
            get
            {
                return homeworkStatus;
            }
        }
        public void subject_type(string subjectSt)
        {
            subjectStatus = subjectSt;
        }
        public void homework_type(string homeworkSt)
        {
            homeworkStatus = homeworkSt;
        }
        //public string content_name { get; set; }
        public Task[] tasks { get; set; }
        //public Estimate[] estimates { get; set; }



        public IEnumerable<Task> GetDiffs(Item other)
        {
            if (!subject_name.Equals(other.subject_name) || !datetime_from.Equals(other.datetime_from))
            {
                throw new InvalidOperationException();
            }

            if (string.Join("", tasks.Select(t => t.task_name)) != string.Join("", other.tasks.Select(t => t.task_name)))
            {
                return other.tasks;
            }
            return Enumerable.Empty<Task>();
        }
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

    }

    /*public class Estimate
    {
        public string estimate_type_code { get; set; }
        public string estimate_type_name { get; set; }
        public string estimate_value_code { get; set; }
        public string estimate_value_name { get; set; }
        public object estimate_comment { get; set; }
    }*/
    internal static class MyExtensions
    {
        internal static IEnumerable<TResult> FullOuterJoin<TA, TB, TKey, TResult>(
        this IEnumerable<TA> a,
        IEnumerable<TB> b,
        Func<TA, TKey> selectKeyA,
        Func<TB, TKey> selectKeyB,
        Func<TA, TB, TKey, TResult> projection,
        TA defaultA = default(TA),
        TB defaultB = default(TB),
        IEqualityComparer<TKey> cmp = null)
        {
            cmp = cmp ?? EqualityComparer<TKey>.Default;
            var alookup = a.ToLookup(selectKeyA, cmp);
            var blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TKey>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                       from xa in alookup[key].DefaultIfEmpty(defaultA)
                       from xb in blookup[key].DefaultIfEmpty(defaultB)
                       select projection(xa, xb, key);

            return join;
        }
    }
}
