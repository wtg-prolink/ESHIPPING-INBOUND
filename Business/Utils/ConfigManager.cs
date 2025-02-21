using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.Utils
{
    public class ConfigManager
    {
        public static IEnumerable<FTPConfig> GetFTPConfig(string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath)) throw new Exception("程式未配置！");
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);
            XmlNodeList nodeList = doc.GetElementsByTagName("item");
            return GetFTPConfig(nodeList);
        }

        public static IEnumerable<FTPConfig> GetFTPConfig(XmlNodeList nodeList, Action<FTPConfig> handler = null)
        {
            if (nodeList == null || nodeList.Count <= 0) yield break;
            foreach (XmlElement node in nodeList)
            {
                if (node is System.Xml.XmlComment) continue;
                var c = new FTPConfig
                {
                    Node = node,
                    Port = Prolink.Math.GetValueAsInt(node.GetAttribute("port")),
                    Psw = node.GetAttribute("psw"),
                    Server = node.GetAttribute("server"),
                    User = node.GetAttribute("user"),
                    MailTo = node.GetAttribute("MailTo"),
                    MailCC = node.GetAttribute("MailCC"),
                    FtpBackupPath = node.GetAttribute("FtpBackupPath"),
                    Path = node.GetAttribute("path"),
                    BackupPath = node.GetAttribute("backupPath"),
                    DownloadPath = node.GetAttribute("downloadPath"),
                    LogPath = node.GetAttribute("logPath"),
                    SearchDir = node.GetAttribute("SearchDir"),
                    ConnectMode = Prolink.Math.GetValueAsInt(node.GetAttribute("ConnectMode")),
                    FtpBackupFlag = node.GetAttribute("FtpBackupFlag")
                };
                if (handler != null)
                    handler(c);
                c.Path = DecoratePath(c.Path);
                c.BackupPath = DecoratePath(c.BackupPath);
                c.DownloadPath = DecoratePath(c.DownloadPath);
                c.LogPath = DecoratePath(c.LogPath);
                yield return c;
            }
        }

        static string DecoratePath(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var v = Prolink.Web.WebContext.GetInstance();
            if (v == null)
                value = value.Replace("$APP_PATH", Utils.Context.XmlStorePath);
            else
                value = value.Replace("$APP_PATH", Prolink.Web.WebContext.GetInstance().GetAppPath());
            CreateDirectory(value);
            return value;
        }

        static void CreateDirectory(string dir)
        {
            if (string.IsNullOrEmpty(dir)) return;
            if (Directory.Exists(dir)) return;
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch
            {

            }
        }
    }
    public enum FtpType
    {
        FTP = 0,
        SFTP = 1,
        FTPS = 2
    }

    public enum KeyType
    {
        Sftp_NoKey = 0,
        Sftp_KeyKnownHost = 1,
        Sftp_PublicKey = 2,
        Sftp_PrivateKey = 3
    }

    public class FTPConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Psw { get; set; }
        public string Path { get; set; }
        public string LogPath { get; set; }
        public string FtpBackupPath { get; set; }
        public string DownloadPath { get; set; }
        public string BackupPath { get; set; }
        public string MailTo { get; set; }
        public string MailCC { get; set; }
        public XmlNode Node { get; set; }
        public string SearchDir { get; set; }
        public int ConnectMode { get; set; }
        public FtpType ftpType { get; set; }
        public KeyType KeyType { get; set; }
        public string sftpKeyName { get; set; }
        public string FtpBackupFlag { get; set; }
        public int keyType { get; set; }
    }
}