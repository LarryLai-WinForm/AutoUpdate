using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace AutoUpdate.modules
{
    public class FTP
    {
        public string Url { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
        public string Path { set; get; }

        NetworkCredential Network_Credential => new NetworkCredential(UserName, Password);
        FtpWebRequest FtpWebRequest_Init(string RequestPath,bool KeepAlive = false)
        {
            FtpWebRequest result;

            result = (FtpWebRequest)WebRequest.Create(Url + RequestPath);
            result.KeepAlive = KeepAlive;
            result.Credentials = Network_Credential;

            return result;
        }
        void FtpWebRequest_Finish(FtpWebRequest ftpWebRequest)
        {
            ftpWebRequest?.Abort();
        }

        public List<string> ListDirectory()
        {
            List<string> result = new List<string>();

            FtpWebRequest ftpWebRequest = null;
            try
            {
                ftpWebRequest = FtpWebRequest_Init(Path);

                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                ///這裡KeepAlive設為true後送出屬性"UseBinary = true"效用才會持續，
                ///此修改為了防止後面GetFileSize因"UseBinary = true"未能生效發生錯誤。
                ///因GetFileSize命令本身不會使"UseBinary = true"生效，不知是為BUG或設計問題
                ///而某些站台需"UseBinary = true"才能執行GetFileSize。
                ///上述原因參考自下列網頁：
                ///https://social.msdn.microsoft.com/Forums/en-US/0c38814e-d8e3-49f3-8818-b5306cc100ce/ftpwebrequestusebinary-does-not-work?forum=netfxnetcom
                ftpWebRequest.KeepAlive = true;

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
                                if (str != null) result.Add(str);
                            }
                            while (str != null);
                        }
                    }
                }
            }
            finally
            {
                FtpWebRequest_Finish(ftpWebRequest);
            }

            return result;
        }
        public long GetFileSize(string FilePath)
        {
            FtpWebRequest ftpWebRequest = null;
            try
            {
                ftpWebRequest = FtpWebRequest_Init(FilePath);

                ftpWebRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                using (WebResponse webResponse = ftpWebRequest.GetResponse())
                {
                    long ContentLength = webResponse.ContentLength;
                    return ContentLength;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                FtpWebRequest_Finish(ftpWebRequest);
            }
        }
        public void DownloadFile(string FilePath, int FileSize, string LocalFilePath)
        {
            FtpWebRequest ftpWebRequest = null;
            try
            {
                ftpWebRequest = FtpWebRequest_Init(FilePath);

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
                FtpWebRequest_Finish(ftpWebRequest);
            }
        }
    }
}
