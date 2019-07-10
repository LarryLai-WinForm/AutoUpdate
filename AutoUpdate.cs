using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Ionic.Zip;

namespace AutoUpdate
{
    using static WinAPI;

    /// <summary>
    /// AutoUpdate
    /// </summary>
    public class AutoUpdateClass
    {

        /// <summary>
        /// The download directory is the same as the AssemblyName
        /// </summary>
        public static string AssemblyName = Assembly.GetEntryAssembly().GetName().Name;

        /// <summary>
        /// for old file suffix
        /// </summary>
        public const string ext_OLD = ".old";

        #region Message Process

        public delegate void MsgAddEvent(string msg);
        public event MsgAddEvent msgAddEvent;
        void MsgAdd(string msg)
        {
            if (msgAddEvent != null)
            {
                msgAddEvent(msg);
            }
        }

        /// <summary>
        /// Message
        /// </summary>
        string msgAppend;
        /// <summary>
        /// Message Append
        /// </summary>
        string MsgAppend
        {
            set
            {
                const string newLine = "\r\n";
                const string dateTimeFormat = "[yyyy/MM/dd HH:mm:ss] ";

                string msg = DateTime.Now.ToString(dateTimeFormat) + value + newLine;
                MsgAdd(msg);
                msgAppend += msg;
            }
            get
            {
                return msgAppend;
            }
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

        public delegate void ResultEvent(Result res);
        public event ResultEvent resultEvent;
        void result(Result res)
        {

            switch (res)
            {
                case Result.Success:
                    MsgAppend = "Check Update Success!";
                    break;

                case Result.Newest:
                    MsgAppend = "Local Version Is Newest!";
                    break;

                default:
                case Result.Fail:
                    MsgAppend = "Check Update Fail!";
                    break;
            }

            if (resultEvent != null)
            {
                resultEvent(res);
            }
        }

        #endregion

        /// <summary>
        /// for AutoUpdate
        /// </summary>
        public class Info
        {
            /// <summary>
            /// FTP Url, UserName, Password
            /// </summary>
            public struct FtpInfo
            {
                public string UrlString;
                public string UserName;
                public string Password;
            }
            public FtpInfo ftpInfo;

            public class Directory_Info
            {
                public Dictionary<string, Directory_Info> Directories = new Dictionary<string, Directory_Info>();
                public List<string> Files = new List<string>();

                public void ChangeToOldFile()
                {
                    foreach (var d in Directories.Keys)
                    {
                        Directories[d].ChangeToOldFile();
                    }

                    foreach (var f in Files)
                    {
                        MoveFileEx(f, f + ext_OLD, MoveFileFlags.MOVEFILE_0x01_REPLACE_EXISTING);
                    }
                }

                public void ClearOldFiles(string dir = "")
                {
                    foreach (var d in Directories.Keys)
                    {
                        Directories[d].ClearOldFiles(d);
                    }
                    /// Clear old files
                    Process P_CleanOldFiles = new Process();
                    /// Set the delay so that the program can be safely deleted after the program is successfully closed.
                    P_CleanOldFiles.StartInfo = new ProcessStartInfo("cmd.exe",
                        "/C choice /C Y /N /D Y /T 0 & " +
                        "Del "
                        + '"' + dir + "*" + ext_OLD + '"');
                    P_CleanOldFiles.StartInfo.CreateNoWindow = true;
                    P_CleanOldFiles.StartInfo.UseShellExecute = false;
                    P_CleanOldFiles.Start();
                }
            }
            public Directory_Info directory_Info { set; get; }
        }
        /// <summary>
        /// AutoUpdate Info
        /// </summary>
        Info info;

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
        /// <summary>
        /// Main Work Run
        /// </summary>
        public void Run()
        {
            const string slash = "/";
            MsgAppend = "check update";

            #region get local assembly name & version

            MsgAppend = "assembly name : " + AssemblyName;
            FileVersionInfo fver = FileVersion_Custom.GetFileVersionInfoByFilePath();
            Version LocalVer = new Version(fver.FileVersion);
            MsgAppend = "current version : " + LocalVer.ToString();

            #endregion

            #region set FTP

            FTP_Class ftp = new FTP_Class();
            try
            {
                ftp.UrlString = info.ftpInfo.UrlString;
                ftp.UserName = info.ftpInfo.UserName;
                ftp.Password = info.ftpInfo.Password;
            }
            catch (Exception ex)
            {
                MsgAppend = "FTP set fail!";
                MsgAppend = ex.Message;

                result(Result.Fail);
                return;
            }

            #endregion

            #region get remote files list

            string ListDirectoryFirstFile;
            try
            {

                MsgAppend = "get FTP files......";
                List<string> list = new List<string>();
                //Set the assembly name to the remote directory name
                list = ftp.ListDirectory(AssemblyName);

                if (list.Count <= 0)
                {
                    MsgAppend = "FTP has no updateable data!";

                    result(Result.Newest);
                    return;
                }

                list.Sort();
                list.Reverse();

                ListDirectoryFirstFile = list[0];
                MsgAppend = "get FTP files sucees!";
            }
            catch (Exception ex)
            {
                MsgAppend = "get FTP files fail!";
                MsgAppend = ex.Message;

                result(Result.Fail);
                return;
            }

            #endregion

            #region get remote file version

            const string ext_ZIP = ".zip";
            Version UpdateVer = new Version();
            string fileName;
            try
            {
                string[] strArray = ListDirectoryFirstFile.Split(slash.ToCharArray());
                if (strArray.Length < 2)
                {
                    MsgAppend = "get remote files list data process error!";

                    result(Result.Fail);
                    return;
                }
                fileName = strArray[1];
                string ver = fileName;

                if (ver.EndsWith(ext_ZIP))
                {
                    ver = ver.Substring(0, ver.Length - ext_ZIP.Length);
                }

                if (ver.StartsWith(AssemblyName))
                {
                    ver = ver.Substring(AssemblyName.Length);
                }
                
                UpdateVer = new Version(ver);

                MsgAppend = "newest version : " + UpdateVer.ToString();

            }
            catch (Exception ex)
            {
                MsgAppend = "can't parse remote update file data!";
                MsgAppend = ex.Message;

                result(Result.Fail);
                return;
            }

            #endregion

            #region check the remote version is newer

            if (CheckIsNewer(LocalVer,UpdateVer) == false)
            {
                result(Result.Newest);
                return;
            }

            #endregion

            #region get new file size

            int fileSize;
            try
            {
                fileSize = (int)ftp.GetFileSize(AssemblyName + slash + fileName);

                MsgAppend = "get new file size : " + fileSize.ToString() + "bytes";
            }
            catch (Exception ex)
            {
                MsgAppend = "get remote files process error!";
                MsgAppend = ex.Message;

                result(Result.Fail);
                return;
            }

            #endregion

            #region download & update

            try
            {
                MsgAppend = "download update file......";
                ftp.DownloadFile(AssemblyName + slash + fileName, fileSize, fileName);
                MsgAppend = "download update file sucees!";
            }
            catch (Exception ex)
            {
                MsgAppend = "download update file fail!";
                MsgAppend = ex.Message;

                result(Result.Fail);
                return;
            }

            #endregion

            /// <para>
            /// You must first create a ZIP object before moving the old file. 
            /// Otherwise, the problem of not finding the DLL will occur when the file is moving.
            /// </para>
            /// <para>
            /// Parameter System.Text.Encoding.Default to avoid Chinese garbled
            /// </para>
            using (ZipFile zf = new ZipFile(fileName,System.Text.Encoding.Default))
            {

                #region move old files

                try
                {
                    MsgAppend = "process old files......";
                    info.directory_Info.ChangeToOldFile();
                    MsgAppend = "process old files success!";
                }
                catch (Exception ex)
                {
                    MsgAppend = "process old files fail!";
                    MsgAppend = ex.Message;

                    result(Result.Fail);
                    return;
                }

                #endregion

                #region Extract new file

                MsgAppend = "Extract new file......";
                zf.ExtractAll(Application.StartupPath,ExtractExistingFileAction.OverwriteSilently);
                MsgAppend = "Extract new file compelete!";

                #endregion

            }

            /// delete zip file
            File.Delete(fileName);

            result(Result.Success);
            MsgAppend = "The update is successful! The program will restart automatically late!";

            info.directory_Info.ClearOldFiles();

            /// Execute the new program to start automatically
            Process P_new = new Process();
            /// set delay
            P_new.StartInfo = new ProcessStartInfo("cmd.exe", 
                "/C choice /C Y /N /D Y /T 0 & " + 
                "\"" + AssemblyName + ".exe" + "\"");
            P_new.StartInfo.CreateNoWindow = true;
            P_new.StartInfo.UseShellExecute = false;
            P_new.Start();


            Environment.Exit(0);
        }

        public AutoUpdateClass(Info _info)
        {
            info = _info;
        }

    }

    class FileVersion_Custom
    {
        /// <summary>Get FileVersionInfo By File Path</summary>
        /// <param name="filePath">file path</param>
        /// <returns>FileVersionInfo</returns>
        public static FileVersionInfo GetFileVersionInfoByFilePath(string filePath)
        {
            return FileVersionInfo.GetVersionInfo(filePath);
        }

        public static FileVersionInfo GetFileVersionInfoByFilePath()
        {
            return GetFileVersionInfoByFilePath(Assembly.GetEntryAssembly().Location);
        }
    }

    class FTP_Class
    {
        public string UrlString;
        public string UserName;
        public string Password;
        NetworkCredential networkCredential
        {
            get
            {
                return new NetworkCredential(UserName, Password); ;
            }
        }

        public List<string> ListDirectory(string DirectoryPath = "")
        {
            List<string> strList = new List<string>();

            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(UrlString + DirectoryPath);
            ftpWebRequest.KeepAlive = false;
            try
            {
                ftpWebRequest.Credentials = networkCredential;
                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                
                using (WebResponse webResponse = ftpWebRequest.GetResponse())
                {
                    using (Stream s = webResponse.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(s))
                        {
                            string str;
                            do
                            {
                                str = streamReader.ReadLine();
                                if (str != null) strList.Add(str);
                            }
                            while (str != null);
                        }
                    }
                }
            }
            finally
            {
                //ftpWebRequest.Abort();
                //ftpWebRequest.KeepAlive = false;
            }

            return strList;
        }

        public long GetFileSize(string filePath)
        {
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(UrlString + filePath);
            ftpWebRequest.KeepAlive = false;
            try
            {
                ftpWebRequest.Credentials = networkCredential;
                ftpWebRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                using (WebResponse webResponse = ftpWebRequest.GetResponse())
                {
                    long ContentLength = webResponse.ContentLength;
                    return ContentLength;
                }
            }
            finally
            {
                //ftpWebRequest.KeepAlive = false;
                //ftpWebRequest.Abort();
            }
        }

        public void DownloadFile(string DownLoadFilePath, int FileSize, string LocalFilePath)
        {
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(UrlString + DownLoadFilePath);
            ftpWebRequest.KeepAlive = false;
            try
            {
                ftpWebRequest.Credentials = networkCredential;
                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                using (WebResponse webResponse = ftpWebRequest.GetResponse())
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        using (FileStream fsWriter = new FileStream(LocalFilePath, FileMode.Create))
                        {
                            byte[] buffer = new byte[FileSize];

                            while (true)
                            {
                                int bytesRead = responseStream.Read(buffer, 0, FileSize);

                                if (bytesRead <= 0)
                                {
                                    break;
                                }

                                fsWriter.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }
            finally
            {
                //ftpWebRequest.KeepAlive = false;
                //ftpWebRequest.Abort();
            }
        }
    }

    class WinAPI
    {
        #region MoveFileEx
        /// <summary>
        /// MoveFileFlags
        /// </summary>
        public enum MoveFileFlags
        {
            MOVEFILE_0x01_REPLACE_EXISTING = 0x01,
            MOVEFILE_0x02_COPY_ALLOWED = 0x02,
            MOVEFILE_0x04_DELAY_UNTIL_REBOOT = 0x04,
            MOVEFILE_0x08_WRITE_THROUGH = 0x08,
            MOVEFILE_0x10_CREATE_HARDLINK = 0x10,
            MOVEFILE_0x20_FAIL_IF_NOT_TRACKABLE = 0x20,
        }
        /// <summary>
        /// MoveFileEx
        /// </summary>
        /// <param name="lpExistingFileName"></param>
        /// <param name="lpNewFileName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
        #endregion
    }
}
