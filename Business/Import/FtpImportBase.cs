using Business.Utils;
using EnterpriseDT.Net.Ftp;
using EnterpriseDT.Net.Ssh;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Business.Import
{
    public abstract class FtpImportBase : ImportBase
    {
        public event EventHandler<FtpImportEvertArgs> BeforeDownload;
        public event EventHandler<FtpImportEvertArgs> AfterDownload;
        public event EventHandler<FtpImportEvertArgs> AfterBackup;
        public event EventHandler<FtpImportEvertArgs> OccuredError;
        private FtpConnection ftpConnection;

        private SecureFTPConnection secureftpConnection = new SecureFTPConnection();
        SftpClient sftp;
        private bool _Sftp = false;

        public FtpImportBase()
        {
            ftpConnection = new FtpConnection();
        }
        public void AnalyzeFile(bool ftptype = false)
        {
            try
            {
                _Sftp = ftptype;
                OnAnalyzeFile();
            }
            finally
            {
                Close();
            }
        }
        void OnDownLoadException(FTPConfig config, string fileName, Exception ex)
        {
            if (fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0)
                File.Delete(fileName);
            var l = Logger.CreateLog("文件下载异常", this.GetType().Name, "", "", ex.ToString());
            l.Data = config;
            Logger.WriteLog(l);
            if (!string.IsNullOrEmpty(config.MailTo))
            {
                string subject = string.Format("Down Load File Error!:{0} {1}", fileName, DateTime.Now.ToString("g"));
                SendMail(new Business.Mail.MailInfo
                {
                    Subject = subject,
                    Body = string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace)
                });
            }
        }
        void OnOperateFileException(FTPConfig config, string fileName, Exception ex)
        {
            if (fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0)
                File.Delete(fileName);
            var l = Logger.CreateLog("处理文档发生异常", this.GetType().Name, "", "", ex.ToString());
            l.Data = config;
            Logger.WriteLog(l);
            if (!string.IsNullOrEmpty(config.MailTo))
            {
                string subject = string.Format("Operate File Error!:{0}  {1}", fileName, DateTime.Now.ToString("g"));
                SendMail(new Business.Mail.MailInfo
                {
                    Subject = subject,
                    Body = string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace)
                });
            }
        }
        void OnConnectException(FTPConfig config, Exception ex)
        {
            var l = Logger.CreateLog("处理连线发生异常", this.GetType().Name, "", "", ex.ToString());
            l.Data = config;
            Logger.WriteLog(l);
            if (!string.IsNullOrEmpty(config.MailTo))
            {
                string subject = string.Format("Connect FTP Error!:{0}  {1}", config.Server, DateTime.Now.ToString("g"));
                SendMail(new Business.Mail.MailInfo
                {
                    Subject = subject,
                    Body = string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace)
                });
            }
        }

        enum ActionEventModes { BeforeDownload, AfterDownLoad, AfterBackup, Exception }
        void TriggerEvent(FtpImportEvertArgs args, ActionEventModes mode)
        {
            switch (mode)
            {
                case ActionEventModes.BeforeDownload:
                    if (BeforeDownload != null)
                        BeforeDownload(this, args);
                    break;
                case ActionEventModes.AfterDownLoad:
                    if (AfterDownload != null)
                        AfterDownload(this, args);
                    break;
                case ActionEventModes.AfterBackup:
                    if (AfterBackup != null)
                        AfterBackup(this, args);
                    break;
                case ActionEventModes.Exception:
                    if (OccuredError != null)
                        OccuredError(this, args);
                    break;
            }
        }

        private FTPFile[] GetFileForLastModify(string directory)
        {
            if (_Sftp)
            {
                return secureftpConnection.GetFileInfos(directory).AsEnumerable().OrderBy(r => r.LastModified).ToArray();
            }
            else
            {
                return ftpConnection.GetFileInfosForLastModifyLongDateTime(directory);
            }
        }


        void GetFiles(List<FTPFile> list, Dictionary<FTPFile, string> fileMap, string path)
        {


            FTPFile[] fileInfos = GetFileForLastModify(path);
            foreach (var item in fileInfos)
            {
                if (item.Dir)
                {
                    if (item.Name == "." || item.Name == ".." || item.Name.StartsWith("..") || item.Name == "ERROR" || item.Name == "SUCC" || item.Name == "log")
                    {
                    }
                    else
                    {
                        GetFiles(list, fileMap, path + "/" + item.Name);
                    }
                }
                else
                {
                    string toPath = path + "/" + item.Name;
                    int index = toPath.IndexOf("/");
                    //int index = path.Length;
                    fileMap[item] = toPath.Substring(index + 1, toPath.Length - index - 1);
                    list.Add(item);
                }
            }
        }

        //检查FTP目录是否存在
        public static void FtpCheckDirectoryExist(FTPConfig config, string fullDir)
        {
            string[] dirs = fullDir.Split('/');
            string curDir = "/";
            for (int i = 3; i < dirs.Length; i++)
            {
                string dir = dirs[i];
                //如果是以/开始的路径,第一个为空    
                if (dir != null && dir.Length > 0)
                {
                    try
                    {
                        curDir += dir + "/";
                        FtpMakeDir(config, curDir);
                    }
                    catch (Exception)
                    { }
                }
            }
        }

        //创建目录  
        public static Boolean FtpMakeDir(FTPConfig config, string dirs)
        {
            string ftpServerIP = "ftp://" + config.Server;
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpServerIP + dirs);
            req.Credentials = new NetworkCredential(config.User, config.Psw);
            req.Method = WebRequestMethods.Ftp.MakeDirectory;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                response.Close();
            }
            catch (Exception)
            {
                req.Abort();
                return false;
            }
            req.Abort();
            return true;
        }

        //Ftp文件移动到指定目录
        public bool FileRename(FTPConfig config, string oldFileName, string newFileName, string destDir)
        {
            bool success = false;
            FtpWebRequest ftpWebRequest = null;
            FtpWebResponse ftpWebResponse = null;
            Stream ftpResponseStream = null;
            try
            {
                string uri = "ftp://" + config.Server + "/" + config.Path + "/" + oldFileName.Trim('/');
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                ftpWebRequest.Credentials = new NetworkCredential(config.User, config.Psw);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.Rename;
                ftpWebRequest.RenameTo = "./" + destDir.Trim('/') + "/" + newFileName.Trim('/');

                ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                ftpResponseStream = ftpWebResponse.GetResponseStream();

            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (ftpResponseStream != null)
                {
                    ftpResponseStream.Close();
                }
                if (ftpWebResponse != null)
                {
                    ftpWebResponse.Close();
                }
            }
            return success;
        }

        protected virtual void OnAnalyzeFile()
        {
            FTPConfig config = GetFtpConfig();
            if (!Connect(config))
            {
                Logger.WriteLog("ftp Connect error:{0}");
                return;
            }
            Dictionary<FTPFile, string> fileMap = new Dictionary<FTPFile, string>();
            List<FTPFile> list = new List<FTPFile>();
            if (config.SearchDir == "Y")
            {
                GetFiles(list, fileMap, config.Path);
            }
            else
            {
                FTPFile[] ftpfilsInfos = GetFileForLastModify(config.Path);
                foreach (var item in ftpfilsInfos)
                {
                    if (item.Dir) continue;
                    string toPath = config.Path + "/" + item.Name;
                    int index = config.Path.Length;
                    fileMap[item] = toPath.Substring(index + 1, toPath.Length - index - 1);
                    list.Add(item);
                }
            }
            FTPFile[] fileInfos = list.ToArray();

            if (fileInfos == null || fileInfos.Length <= 0)
            {
                Logger.WriteLog("ftp no fils:" + config.Path);
                return;
            }
            fileInfos = fileInfos.OrderBy(item => item.LastModified).ToArray();
            foreach (var item in fileInfos)
            {
                string fileName = fileMap[item];
                string localDownFile = Path.Combine(config.DownloadPath, fileName);
                FtpImportEvertArgs args = new FtpImportEvertArgs(config, item);
                try
                {
                    TriggerEvent(args, ActionEventModes.BeforeDownload);
                    string localDownPath = Path.GetDirectoryName(localDownFile);
                    if (!System.IO.Directory.Exists(localDownPath))
                        Directory.CreateDirectory(localDownPath);

                    DownLoadFile(localDownFile, config.Path + "/" + fileName);
                    args.LocalFileName = localDownFile;
                    TriggerEvent(args, ActionEventModes.AfterDownLoad);
                }
                catch (Exception ex)
                {
                    OnDownLoadException(config, localDownFile, ex);
                    args.EX = ex;
                    TriggerEvent(args, ActionEventModes.Exception);
                    if (args != null && args.ErrToNext)
                        continue;
                    return;
                }
                try
                {
                    bool isDelFtpFile = OperateFile(args);
                    string backupPath = CreateBackupPath(config);
                    BackupFile(backupPath, config, fileName, isDelFtpFile);
                    args.BackupFileName = Path.Combine(backupPath, fileName);
                    TriggerEvent(args, ActionEventModes.AfterBackup);
                }
                catch (Exception ex)
                {
                    OnOperateFileException(config, localDownFile, ex);
                    args.EX = ex;
                    TriggerEvent(args, ActionEventModes.Exception);
                    if (args != null && args.ErrToNext)
                        continue;
                    return;
                }
            }
        }

        private void DownLoadFile(string localFile, string remoteFile)
        {
            if (_Sftp)
            {
                secureftpConnection.DownloadFile(localFile, remoteFile);
            }
            else
            {
                ftpConnection.DownloadFile(localFile, remoteFile);
            }
        }

        protected virtual string CreateBackupPath(FTPConfig config)
        {
            string parentDirectory = config.BackupPath;
            return CreateDirectory(Path.Combine(parentDirectory, DateTime.Now.ToString("yyyyMMdd")));
        }

        private void BackupFile(string destDirectory, FTPConfig config, string fileName, bool isDelFtpFile)
        {
            string destFileName = Path.Combine(destDirectory, fileName);
            if (File.Exists(destFileName))
                File.Delete(destFileName);
            string downFileName = Path.Combine(config.DownloadPath, fileName);
            string destDir = System.IO.Path.GetDirectoryName(destFileName);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            if (File.Exists(downFileName))
                File.Move(downFileName, destFileName);
            if (File.Exists(downFileName))
                File.Delete(downFileName);
            string ftpFileName = Path.Combine(config.Path, fileName);
            try
            {
                if ("Y".Equals(config.FtpBackupFlag))
                {
                    string ftpDestDir = isDelFtpFile ? "SUCC" : "ERROR";
                    BackupFTPFile(config, fileName, ftpDestDir);
                }
                else if ("E".Equals(config.FtpBackupFlag))
                {
                    if (!isDelFtpFile)
                    {
                        string ftpDestDir = "ERROR";
                        BackupFTPFile(config, fileName, ftpDestDir);
                    }
                    else
                    {
                        DeleteFile(ftpFileName);
                    }
                }
                else
                {
                    if (isDelFtpFile)
                    {
                        if (!BackupToFtp(ftpConnection, config, fileName))
                            DeleteFile(ftpFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(string.Format("Delete file error:{0}", ftpFileName), ex);
            }
        }
        private void DeleteFile(string remoteFile)
        {
            if (_Sftp)
            {
                secureftpConnection.DeleteFile(remoteFile);
            }
            else
            {
                ftpConnection.DeleteFile(remoteFile);
            }

        }

        private void BackupFTPFile(FTPConfig config, string fileName, string ftpDestDir)
        {
            try
            {
                if (_Sftp)
                {
                }
                else
                {
                    string newDir = ftpDestDir + "/" + DateTime.Now.ToString("yyyyMMdd");
                    string url = "ftp://" + config.Server + "/" + config.Path + "/" + newDir;
                    string newFileName = fileName;
                    if (fileName.LastIndexOf("/") >= 0)
                    {
                        newFileName = fileName.Substring(fileName.LastIndexOf("/"));
                        url = "ftp://" + config.Server + "/" + config.Path + "/" + fileName.Substring(0, fileName.LastIndexOf("/")) + "/" + newDir;
                    }
                    FtpCheckDirectoryExist(config, url);
                    FileRename(config, fileName, newFileName, newDir);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected bool BackupToFtp(FtpConnection ftpConnection, FTPConfig config, string fileName)
        {
            if (string.IsNullOrEmpty(config.FtpBackupPath)) return false;
            string fromFileName = Path.Combine(config.Path, fileName);
            string toPath = Path.Combine(config.FtpBackupPath, DateTime.Now.ToString("yyyyMMdd"));
            try
            {
                ftpConnection.CreateDirectory(toPath);
            }
            catch
            {

            }
            try
            {
                string toFileName = Path.Combine(toPath, fileName);
                ftpConnection.RenameFile(fromFileName, toFileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected string CreateDirectory(string dir)
        {
            if (string.IsNullOrEmpty(dir)) return string.Empty;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        private void Close()
        {
            if (ftpConnection.IsConnected) ftpConnection.Close();
            if (secureftpConnection.IsConnected) secureftpConnection.Close();
        }

        protected bool Connect(FTPConfig config)
        {
            try
            {
                if (_Sftp)
                {
                    //sftp = new SftpClient(config.Server, config.Port, config.User, config.Psw);
                    //sftp.Connect();
                    if (!secureftpConnection.IsConnected)
                    {
                        secureftpConnection.Protocol = FileTransferProtocol.SFTP;
                        secureftpConnection.ServerAddress = config.Server;
                        secureftpConnection.ServerPort = config.Port;
                        secureftpConnection.UserName = config.User;
                        secureftpConnection.Password = config.Psw;
                        secureftpConnection.AuthenticationMethod = AuthenticationType.Password;
                        secureftpConnection.ServerValidation = SecureFTPServerValidationType.None;
                        secureftpConnection.Timeout = 600000;
                        secureftpConnection.LicenseKey = "042-8283-5817-1624";
                        secureftpConnection.LicenseOwner = "WiseTechGlobal";
                        secureftpConnection.Connect();
                    }
                }
                else
                {
                    ftpConnection.ServerAddress = config.Server;
                    ftpConnection.ServerPort = config.Port;
                    ftpConnection.UserName = config.User;
                    ftpConnection.Password = config.Psw;
                    if (config.ConnectMode == 1)
                    {
                        ftpConnection.ConnectMode = FTPConnectMode.ACTIVE;
                    }
                    ftpConnection.Timeout = 600000;
                    ftpConnection.Connect();
                }
                return true;
            }
            catch (Exception e)
            {
                OnConnectException(config, e);
                return false;
            }
        }

        protected void SendMail(Mail.MailInfo info)
        {
            SendMail(info, null);
        }
        protected void SendMail(Mail.MailInfo info, FTPConfig config)
        {
            Mail.MailServices mail = Mail.MailServices.GetInstance();
            if (config == null)
            {
                mail.SendMail(info); return;
            }
            if (!string.IsNullOrEmpty(config.MailTo))
            {
                if (string.IsNullOrEmpty(info.To))
                    info.To = config.MailTo;
                else
                {
                    info.To = string.Format("{0};{1}", info.To, config.MailTo);
                }
            }
            if (!string.IsNullOrEmpty(config.MailCC))
            {
                if (string.IsNullOrEmpty(info.CC))
                    info.CC = config.MailCC;
                else
                {
                    info.CC = string.Format("{0};{1}", info.CC, config.MailCC);
                }
            }
            mail.SendMail(info);
        }

        protected abstract bool OperateFile(FtpImportEvertArgs args);

        protected abstract FTPConfig GetFtpConfig();
    }

    class SendMailException : Exception
    {
        public SendMailException(string msg)
            : base(msg)
        {

        }
    }



    public class FtpImportEvertArgs : EventArgs
    {
        public FtpImportEvertArgs(FTPConfig config, FTPFile file)
        {
            Config = config;
            File = file;
            ErrToNext = true;
        }

        public FTPConfig Config { get; private set; }
        public FTPFile File { get; private set; }
        public string LocalFileName { get; set; }
        public string BackupFileName { get; set; }
        public Exception EX { get; set; }
        public bool ErrToNext { get; set; }
        public object Data { get; set; }
    }
}
