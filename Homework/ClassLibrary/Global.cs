using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public static class Global
    {
        public static ConfigJson ConfigJson = Config.Instance.ConfigJson;
        public static Pathes Pathes = ConfigJson.Pathes;
        public static DateTimesFormats DateTimesFormats = ConfigJson.DateTimesFormats;
    }

    public class File_Manager
    {
        public async Task<FileData> OpenFile(string path, string type, string content)
        {
            var file = new FileInfo(path);
            int attempts = 0;
            bool success = false;
            while (!success && attempts < 10)
            {
                try
                {
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
                    else if(type == "Append")
                    {
                        await File.AppendAllTextAsync(path, content);
                    }
                    success = true;
                }
                catch (IOException)
                {
                    await Task.Delay(1000);
                    attempts++;
                }
            }
            if (attempts >= 10) throw new Exception("Getting the doctype to file failed"); 
            return null;
        }
    }

    public class FileData
    {
        public string fileData;
    }
}
