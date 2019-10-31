using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace ClassLibrary
{
    public static class Global
    {
        public static Semaphore sem;

        public static ConfigJson ConfigJson = Config.Instance.ConfigJson;
        public static Pathes Pathes = ConfigJson.Pathes;
        public static DateTimesFormats DateTimesFormats = ConfigJson.DateTimesFormats;

        static Global()
        {
            sem = new Semaphore(1,1, "semaphore", out bool createdNew);
            Log.Information($"sema {(createdNew? "created":"existed")}");
        }
    }

    public class File_Manager
    {
        public async Task<FileData> OpenFile(string path, string type, string content)
        {
            if(!Global.sem.WaitOne(TimeSpan.FromSeconds(Debugger.IsAttached ? 60 : 10)))
            {
                throw new InvalidProgramException("Impossible to aquire semaphore");
            }
            try
            {
                var file = new FileInfo(path);

                if (type == "Write")
                {
                    await File.WriteAllTextAsync(path, content);
                }
                else if (type == "Read")
                {
                    string readedFile;
                    using (var reader = new StreamReader(file.FullName))
                    {
                        readedFile = await reader.ReadToEndAsync();
                    }
                    var fileData = new FileData();
                    fileData.fileData = readedFile;
                    return fileData;
                }
                else if (type == "Append")
                {
                    await File.AppendAllTextAsync(path, content);
                }
                return null;
            }
            finally
            {
                Global.sem.Release();
            }
        }
    }

    public class FileData
    {
        public string fileData;
    }
}
