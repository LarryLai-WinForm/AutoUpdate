using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using Ionic.Zip;
using Newtonsoft.Json;

namespace AutoUpdate
{
    using modules;

    public class AutoUpdate
    {
        FTP Ftp { set; get; }

        public AutoUpdate(FTP _Ftp)
        {
            Ftp = _Ftp;
        }

        #region Message Process

        public delegate void MsgAddEventFunc(string msg);
        public event MsgAddEventFunc MsgAddEvent_Func;
        void MsgAdd(string msg)
        {
            const string dateTimeFormat = "[yyyy/MM/dd HH:mm:ss] ";

            string newLine = Environment.NewLine;
            msg = DateTime.Now.ToString(dateTimeFormat) + msg + newLine;

            MsgAddEvent_Func?.Invoke(msg);
        }

        #endregion

        #region  Process result

        /// <summary>
        /// Result
        /// </summary>
        public enum Result
        {
            /// <summary>
            /// Fail
            /// </summary>
            Fail,
            /// <summary>
            /// Success
            /// </summary>
            Success,
            /// <summary>
            /// Newest
            /// </summary>
            Newest,
        }

        public delegate void ResultEventFunc(Result res);
        public event ResultEventFunc ResultEvent_Func;
        void ProcessResult(Result res)
        {

            switch (res)
            {
                case Result.Success:
                    MsgAdd("Check Update Success!");
                    break;

                case Result.Newest:
                    MsgAdd("Local Version Is Newest!");
                    break;

                default:
                case Result.Fail:
                    MsgAdd("Check Update Fail!");
                    break;
            }

            ResultEvent_Func?.Invoke(res);
        }

        #endregion

        /// <summary>
        /// Main Work Run
        /// </summary>
        public void Run()
        {
            const string Slash = "/";
            string AssemblyName = Assembly.GetEntryAssembly().GetName().Name;

            Directory_Info directory_Info;
            Version LocalVer;
            Version UpdateVer;
            string ZipFileName;

            #region Load_Directory_Info_Data_File

            try
            {
                Directory_Info Load_Directory_Info_Data_File()
                {
                    string Directory_Info_Data_File = "AutoUpdate.Directory_Info_Data.json";
                    string json = File.ReadAllText(Directory_Info_Data_File);
                    return JsonConvert.DeserializeObject<Directory_Info>(json);
                }
                directory_Info = Load_Directory_Info_Data_File();
            }
            catch (Exception ex)
            {
                MsgAdd("Load_Directory_Info_Data_File fail!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

            #region get local assembly name & version

            try
            {
                MsgAdd($"assembly name : {AssemblyName}");
                string getEntryAssembly_Location = Assembly.GetEntryAssembly().Location;
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(getEntryAssembly_Location);
                LocalVer = new Version(fileVersionInfo.FileVersion);
                MsgAdd($"current version : {LocalVer}");
            }
            catch (Exception ex)
            {
                MsgAdd("get local assembly name & version fail!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

            #region set FTP

            try
            {
                Ftp.Url = Ftp.Url;
                Ftp.UserName = Ftp.UserName;
                Ftp.Password = Ftp.Password;
            }
            catch (Exception ex)
            {
                MsgAdd("FTP set fail!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

            #region get remote files

            string ListDirectoryFirstFile;

            try
            {
                MsgAdd($"get {Ftp.Url + Ftp.Path} files......");
                List<string> list = new List<string>();
                //Set the path and assembly name to the remote directory name
                list = Ftp.ListDirectory();

                if (list.Count <= 0)
                {
                    MsgAdd("FTP has no updateable data!");

                    ProcessResult(Result.Newest);
                    return;
                }

                list.Sort();
                list.Reverse();

                ListDirectoryFirstFile = list[0];
                MsgAdd("get FTP files sucees!");
            }
            catch (Exception ex)
            {
                MsgAdd("get FTP files fail!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            try
            {
                const string ext_ZIP = ".zip";
                string[] strArray = ListDirectoryFirstFile.Split(Slash.ToCharArray());

                ZipFileName = strArray[strArray.Length-1];
                string verCheck = ZipFileName;

                //remove .zip
                if (verCheck.EndsWith(ext_ZIP))
                {
                    verCheck = verCheck.Substring(0, verCheck.Length - ext_ZIP.Length);
                }

                //remove AssemblyName
                if (verCheck.StartsWith(AssemblyName))
                {
                    verCheck = verCheck.Substring(AssemblyName.Length);
                }
                
                UpdateVer = new Version(verCheck);

                MsgAdd($"newest version : {UpdateVer}");

            }
            catch (Exception ex)
            {
                MsgAdd("can't parse remote update file data!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

            #region check the remote version is newer

            try
            {
                /// <summary>
                /// Compare newVer with oldVer new
                /// </summary>
                /// <param name="oldVer"></param>
                /// <param name="newVer"></param>
                /// <returns></returns>
                bool CheckIsNewer(Version oldVer, Version newVer)
                {
                    if (newVer.Major > oldVer.Major) return true;
                    if (newVer.Major < oldVer.Major) return false;

                    if (newVer.Minor > oldVer.Minor) return true;
                    if (newVer.Minor < oldVer.Minor) return false;

                    if (newVer.Build > oldVer.Build) return true;
                    if (newVer.Build < oldVer.Build) return false;

                    if (newVer.Revision > oldVer.Revision) return true;
                    if (newVer.Revision < oldVer.Revision) return false;

                    return false;
                }
                if (CheckIsNewer(LocalVer, UpdateVer) == false)
                {
                    ProcessResult(Result.Newest);
                    return;
                }
            }
            catch (Exception ex)
            {
                MsgAdd("check the remote version is newer error!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

            #region get new file size & download

            int ZipFileSize;

            try
            {
                ZipFileSize = (int)Ftp.GetFileSize(Ftp.Path + Slash + ZipFileName);

                MsgAdd($"get new file size : {ZipFileSize}bytes");
            }
            catch (Exception ex)
            {
                MsgAdd("get remote files process error!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            try
            {
                MsgAdd("download update file......");
                Ftp.DownloadFile(Ftp.Path + Slash + ZipFileName, ZipFileSize, ZipFileName);
                MsgAdd("download update file success!");
            }
            catch (Exception ex)
            {
                MsgAdd("download update file fail!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

            #region update files

            try
            {
                /// You must first create a ZIP object before moving the old file. 
                /// Otherwise, the problem of not finding the DLL will occur when the file is moving.
                /// Parameter System.Text.Encoding.Default to avoid Chinese garbled
                /// 先把ZIP物件NEW起來，否則檔案更名後會有找不到dll的問題
                /// 參數System.Text.Encoding.Default避免中文亂碼
                using (ZipFile zf = new ZipFile(ZipFileName, System.Text.Encoding.Default))
                {
                    try
                    {
                        MsgAdd("process old files......");
                        directory_Info.RenameToOldFile();
                        MsgAdd("process old files success!");
                    }
                    catch (Exception ex)
                    {
                        MsgAdd("process old files fail!");
                        MsgAdd(ex.Message);

                        ProcessResult(Result.Fail);
                        return;
                    }

                    MsgAdd("Extract new file......");
                    zf.ExtractAll(Environment.CurrentDirectory, ExtractExistingFileAction.OverwriteSilently);
                    MsgAdd("Extract new file compelete!");
                }
                File.Delete(ZipFileName);
                directory_Info.DeleteOldFiles();
            }
            catch (Exception ex)
            {
                MsgAdd("update files process error!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

            #region restart automatically

            try
            {
                string EXE_Name = AssemblyName + ".exe";
                try
                {
                    EXE_Name = File.ReadAllText("TargetFileName");
                }
                catch{}

                /// Execute the new program to start automatically
                /// set delay
                /// 延遲到程式關閉後再執行開啟
                string cmd = $"/C choice /C Y /N /D Y /T 1 & \"{EXE_Name}\"";
                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo("cmd.exe", cmd)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                    },
                };
                process.Start();

                ProcessResult(Result.Success);
                MsgAdd("The update is successful! The program will restart automatically late!");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MsgAdd("restart automatically fail!");
                MsgAdd(ex.Message);

                ProcessResult(Result.Fail);
                return;
            }

            #endregion

        }
    }

}

#region Example AutoUpdate

//AutoUpdate autoUpdate = new AutoUpdate(Ftp);
//autoUpdate.MsgAddEvent_Func += MsgAdd;
//autoUpdate.ResultEvent_Func += ProcessResult;
//autoUpdate.Run();

#endregion