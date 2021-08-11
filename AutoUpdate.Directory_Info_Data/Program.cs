using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using Newtonsoft.Json;
using Ionic.Zip;

using AutoUpdate.modules;

namespace AutoUpdate.Directory_Info_Data
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = args.Length > 0 ? args[0] : Environment.CurrentDirectory;

            #region AutoUpdate.Directory_Info_Data
            Directory_Info directory_Info = new Directory_Info(path);
            string json = JsonConvert.SerializeObject(directory_Info);
            File.WriteAllText(Path.Combine(path ,"AutoUpdate.Directory_Info_Data.json"), json);
            #endregion



            #region Save Zip File
            string TargetFileName = File.ReadAllText("TargetFileName").Trim();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(TargetFileName);
            FileInfo fileInfo = new FileInfo(TargetFileName);
            string zipFileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length) + fileVersion.FileVersion + ".zip";

            using ZipFile zip = new ZipFile(Encoding.Default); //Encoding.Default 處理中文問題
            zip.AddDirectory(path);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            string PublishPath = directoryInfo.Parent.FullName + "\\" + directoryInfo.Name + "_Publish\\";
            if(!Directory.Exists(PublishPath)) Directory.CreateDirectory(PublishPath);
            zip.Save(PublishPath + zipFileName);
            #endregion
        }
    }
}
