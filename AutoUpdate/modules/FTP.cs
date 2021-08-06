using System.IO;
using System.Net;
using System.Collections.Generic;
using System;

namespace AutoUpdate.modules
{
    public class FTP
    {
        public string Url { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
        public string Path { set; get; }

        NetworkCredential Network_Credential => new NetworkCredential(UserName, Password);
        FtpWebRequest FtpWebRequest_Init(string RequestPath)
        {
            FtpWebRequest result;

            result = (FtpWebRequest)WebRequest.Create(Url + RequestPath);
            result.KeepAlive = false;
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
