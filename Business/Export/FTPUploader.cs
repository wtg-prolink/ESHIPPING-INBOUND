using Business.Utils;
using Prolink.Integrate.Ftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Business.Export
{
    public abstract class FTPUploader : IDisposable
    {
        public FTPUploader()
        {
        }

        protected abstract FTPConfig CreateConfig();

        protected string CreateFile(string txt, string fileName)
        {
            if (_ftpConfig == null) InitUploader();
            if (string.IsNullOrEmpty(fileName)) throw new Exception("file name conn't be null;");
            string path = Path.Combine(_ftpConfig.BackupPath, fileName);
            if (!Directory.Exists(_ftpConfig.BackupPath))
                Directory.CreateDirectory(_ftpConfig.BackupPath);
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, path);
            permissionSet.AddPermission(writePermission);
            if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                UTF8Encoding utf8 = new UTF8Encoding(false);
                using (FileStream stream = new FileStream(path, FileMode.CreateNew))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(txt);
                }
            }
            return path;
        }

        FTPConfig _ftpConfig;
        FtpFileOperator _uploader;
        protected virtual void InitUploader()
        {
            _ftpConfig = CreateConfig();
            _uploader = new FtpFileOperator(_ftpConfig.Server, _ftpConfig.Port, _ftpConfig.User, _ftpConfig.Psw, _ftpConfig.Path);
        }

        string _type = string.Empty;
        public string Upload(string txt, string fileName, string type = "", Boolean isActive = false, Boolean rootFolder = false)
        {
            _type = type;
            string filePath = CreateFile(txt, fileName);
            Upload(filePath, isActive, rootFolder);
            return filePath;
        }

        public void Upload(string filePath, Boolean isActive = false, Boolean rootFolder = false)
        {
            _uploader.ConnectMode(isActive);
            _uploader.Connect();

            string fileName = Path.GetFileName(filePath);
            //Prolink.V6.Persistence.DatabaseFactory.Logger.WriteLog("Loction:" + _type + "IP:" + _uploader.Connection.ServerAddress);

            _uploader.UploadFile(filePath, fileName, rootFolder);
            _uploader.Close();
        }

        //public void Upload(string filePath,Boolean isActive = true)
        //{
        //    _uploader.Connect();
        //    string fileName = Path.GetFileName(filePath);
        //    //Prolink.V6.Persistence.DatabaseFactory.Logger.WriteLog("Loction:" + _type + "IP:" + _uploader.Connection.ServerAddress);
        //    _uploader.UploadFile(filePath, fileName);
        //    _uploader.Close();
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_uploader != null)
                {
                    _uploader.Close();
                    _uploader = null;
                }
            }
        }
    }
}
