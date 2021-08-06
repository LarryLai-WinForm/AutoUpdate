using System;
using System.IO;

using Newtonsoft.Json;

using AutoUpdate.modules;

namespace AutoUpdate.Directory_Info_Data
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = path = Environment.CurrentDirectory;
            if (args.Length > 0)
            {
                path = args[0];
            }

            Directory_Info directory_Info = new Directory_Info(path);
            string json = JsonConvert.SerializeObject(directory_Info);

            File.WriteAllText(path + "AutoUpdate.Directory_Info_Data.json", json);
        }
    }
}
