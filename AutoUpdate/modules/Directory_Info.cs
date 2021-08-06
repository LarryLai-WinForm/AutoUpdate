using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace AutoUpdate.modules
{
    public class Directory_Info
    {
        public Dictionary<string, Directory_Info> Directories { set; get; }
        public List<string> Files { set; get; }

        void GetDirectoryDetailByPath(string path)
        {
            var FilesList = new List<string>();
            var FilesArray = Directory.GetFiles(path);
            foreach (var file in FilesArray)
            {
                FileInfo fileInfo = new FileInfo(file);

                FilesList.Add(fileInfo.Name);
            }
            Files = FilesList;

            var DirectoriesDict = new Dictionary<string, Directory_Info>();
            var DirectoriesArray = Directory.GetDirectories(path);
            foreach (var dir in DirectoriesArray)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dir);

                DirectoriesDict.Add(directoryInfo.Name, new Directory_Info());

                DirectoriesDict[directoryInfo.Name].GetDirectoryDetailByPath(directoryInfo.FullName);
            }
            Directories = DirectoriesDict;
        }

        public Directory_Info(){}
        public Directory_Info(string path) 
        {
            GetDirectoryDetailByPath(path);
        }


        /// <summary>
        /// for old file suffix
        /// </summary>
        string Ext_OLD => ".old";

        /// <summary>
        /// RenameToOldFile
        /// </summary>
        public void RenameToOldFile(string dir = "")
        {
            foreach (var d in Directories.Keys)
            {
                var directory = Directories[d];
                directory.RenameToOldFile(d + "\\");
            }

            foreach (var f in Files)
            {
                string fileName =  dir + f;

                bool result = WinAPI.MoveFileEx(fileName, fileName + Ext_OLD, WinAPI.MoveFileFlags.MOVEFILE_0x01_REPLACE_EXISTING);
                if (!result)
                {
                    throw new Exception($"RenameToOldFile fail!({fileName})");
                }
            }
        }
        /// <summary>
        /// DeleteOldFiles
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteOldFiles(string dir = "")
        {
            foreach (var d in Directories.Keys)
            {
                var directory = Directories[d];
                directory.DeleteOldFiles(d + "\\");
            }

            /// delete old files
            /// Set the delay so that the program can be safely deleted after the program is successfully closed.
            /// 要延遲到程式關閉後執行，否則檔案刪除命令會產生存取被拒錯誤
            string cmd = "/C choice /C Y /N /D Y /T 5 & Del \"" + dir + "*" + Ext_OLD + "\"";
            Process Process_DeleteOldFiles = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe", cmd)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                },
            };
            Process_DeleteOldFiles.Start();
        }
    }
}
