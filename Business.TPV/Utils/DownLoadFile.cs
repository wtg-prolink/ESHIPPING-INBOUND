using EDOCApi;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using TrackingEDI.InboundBusiness;

namespace Business.TPV.Utils
{
    public class DownLoadFile
    {
        private static Prolink.EDOC_API _api = new Prolink.EDOC_API();
        private static string cloumns = "FOLDER_GUID,SERVER_NUM";
        public static string DownLoadFileToDir(string jobNo, string fileNo, string cmp, string dep, string type, bool isinbound, IBUserInfo userinfo, string shipmentid = "", string foldername = "", string masterno = "")
        {
            string CompanyId = userinfo.CompanyId;
            string UserId = userinfo.UserId;
            string GroupId = userinfo.GroupId;
            string ftppath = WebConfigurationManager.AppSettings["EDOC_EXPORT_PATH"] + CompanyId + "/" + UserId + "/" + DateTime.Now.ToString("yyyyMMdd") + "/";
            string path = ftppath;
            string folderpath = string.Empty;
            switch (foldername)
            {
                case "Shipment":
                    if (!string.IsNullOrEmpty(shipmentid))
                    {
                        path += shipmentid + "/";
                        folderpath = shipmentid;
                    }
                    break;
                case "BillNo":
                    if (!string.IsNullOrEmpty(masterno))
                    {
                        path += masterno + "/";
                        folderpath = masterno;
                    }
                    break;
            }
            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
                System.IO.Directory.CreateDirectory(path);

            string[] queryList = jobNo.Split(',');
            string[] fileList = fileNo.Split(',');
            string[] cmplist = cmp.Split(',');
            OperationUtils.Logger.WriteLog("shipmentid:" + shipmentid + "queryList =" + queryList.Length + ";fileList=" + fileList.Length + ";cmplist=" + cmplist.Length + ";type" + type);
            if (string.IsNullOrEmpty(type))
            {
                return "0";
            }
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            for (int k = 0; k < queryList.Length; k++)
            {
                if (queryList[k] == "")
                    continue;
                if (k >= fileList.Length)
                    continue;
                if (isinbound)
                {
                    cmp = cmplist[k];
                }
                FileQueryDownlodInfo(queryList[k], fileList[k], GroupId, cmp, "*", dep, type, list);
            }
            OperationUtils.Logger.WriteLog("FileQueryDownlodInfo list=" + list.Count);
            Dictionary<string, int> filetypedic = new Dictionary<string, int>();
            string errorMsg = string.Empty;
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    try
                    {
                        string url = list[i]["FileUrl"].Replace(WebConfigurationManager.AppSettings["EDOC_URL1"], WebConfigurationManager.AppSettings["EDOC_URL"]);
                        string FileExt = list[i]["FileExt"];
                        string FileEdocType = list[i]["FileEdocType"];
                        string dummyName = list[i]["FileName"];
                        string tempExportFile = null;
                        if (isinbound)
                        {
                            //tempExportFile = path + fileList[k] + FileEdocType + FileExt.ToLower();主单号加Edoc Type
                            //檔名+EDOC_TYPE+YYMMDDGGMMSSms，
                            //? * : " < > \ / |
                            dummyName = dummyName.Replace('/', '_');//   System.Web.HttpUtility.UrlEncode(dummyName);
                            dummyName = dummyName.Remove(dummyName.LastIndexOf("."));

                            string unicode = String2Unicode(dummyName);
                            unicode = unicode.Replace("\\u00a0", "\\u0020");//替换全角空格\u00a0
                            dummyName = Unicode2String(unicode);

                            //取文件名称
                            int istr = new Random(i).Next(10, 100);
                            tempExportFile = path + dummyName + FileEdocType + DateTime.Now.ToString("yyyyMMddHHmmss") + istr + FileExt.ToLower();
                        }
                        else
                        {
                            dummyName = list[i]["FiledummyName"];
                            tempExportFile = path + dummyName + DateTime.Now.ToString("HHmmssfff") + "-" + FileEdocType + FileExt.ToLower();
                        }
                        if (filetypedic.ContainsKey(FileEdocType))
                        {
                            int count = filetypedic[FileEdocType];
                            filetypedic[FileEdocType] = count + 1;
                        }
                        else
                        {
                            filetypedic.Add(FileEdocType, 1);
                        }
                        new DownloadToos(CompanyId, UserId, folderpath).DownloadFileAsync(new Uri(url), tempExportFile);
                    }
                    catch (Exception ex)
                    {
                        OperationUtils.Logger.WriteLog("list 循环出错 list【i】=" + i + "Exception:" + ex.ToString());
                        if (!string.IsNullOrEmpty(errorMsg))
                            errorMsg += ";"; 
                        errorMsg += i + ":" + ex.ToString().Substring(0, 80);
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            int sucCount = 0;
            foreach (KeyValuePair<string, int> kp in filetypedic)
            {
                sb.Append(kp.Key);
                sb.Append(":");
                sb.Append(kp.Value);
                sb.Append(",");
                sucCount += kp.Value;
            }
            if (!string.IsNullOrEmpty(errorMsg))
                sb.Append("errorMsg:" + errorMsg);
            if(sucCount != list.Count)
                sb.Append("DownloadAgain");
            DeleteFile(ftppath);
            return list.Count + ";" + sb.ToString();
        }

        public static void DeleteFile(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    LogError("路徑不能為空");
                    return;
                }

                DateTime deleteDate = DateTime.Now.AddDays(-7);
                string targetDate = deleteDate.ToString("yyyyMMdd");
                string parentDir;
                try 
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    if (dirInfo.Parent != null)
                    {
                        parentDir = dirInfo.Parent.FullName;
                    }
                    else 
                    {
                        LogError("無法取得父目錄");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"處理路徑時發生錯誤: {ex.Message}");
                    return;
                }

                foreach (string subDir in Directory.GetDirectories(parentDir))
                {
                    try
                    {
                        string folderName = Path.GetFileName(subDir);
                        
                        // 驗證資料夾名稱是否為有效日期格式
                        if (!DateTime.TryParseExact(folderName, "yyyyMMdd", 
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime folderDate))
                        {
                            //LogWarning($"跳過非日期格式資料夾: {folderName}");
                            continue;
                        }

                        // 使用日期比較而非字串比較
                        if (folderDate < deleteDate)
                        {
                            LogInfo($"正在刪除過期資料夾: {subDir}");
                            Directory.Delete(subDir, true); // 遞迴刪除
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"處理資料夾時發生錯誤 {subDir}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"DeleteFile 執行錯誤: {ex.Message}");
                throw; // 重要錯誤應該往上拋出
            }
        }

        private static void LogError(string message)
        {
            // 實作日誌記錄
            OperationUtils.Logger.WriteLog($"錯誤: {message}");
        }

        private static void LogWarning(string message)
        {
            OperationUtils.Logger.WriteLog($"警告: {message}");
        }

        private static void LogInfo(string message)
        {
            OperationUtils.Logger.WriteLog($"信息: {message}");
        }

        /// <summary>
        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        public static string String2Unicode(string source)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(source);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="source">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        public static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }


        public static List<Dictionary<string, string>> FileQueryDownlodInfo(string jobNo, string filedummy, string groupId, string cmp, string stn, string dep, string type, List<Dictionary<string, string>> filesInfo)
        {
            int serverNum = 0;
            string guid = _api.GetFolderGUID(jobNo, groupId, cmp, stn, dep, ref serverNum, cloumns);
            List<EDOCFileItem> edocList = _api.Inquery(guid, type, serverNum);
            string path = WebConfigurationManager.AppSettings["EDOC_URL"];
            if (serverNum > 0)
                path = WebConfigurationManager.AppSettings["EDOC_URL_" + serverNum];
            if (edocList == null)
                return null;
            foreach (EDOCFileItem edoc in edocList)
            {
                if (!string.IsNullOrEmpty(edoc.EdocType))
                {
                    Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                    fileInfo.Add("FileName", edoc.DummyName);
                    fileInfo.Add("FileSize", edoc.Size);
                    fileInfo.Add("FileExt", edoc.Ext);
                    fileInfo.Add("FileEdocType", edoc.EdocType);
                    fileInfo.Add("FileUrl", path + "apis/apilaunchfile.ashx?token=" + edoc.Token + "&i=" + edoc.FileID);
                    fileInfo.Add("FiledummyName", filedummy);
                    filesInfo.Add(fileInfo);
                }
            }
            return filesInfo;
        }
    }

    public class FtpHelp
    {
        private string ftpServerIP;
        private string ftpRemotePath;
        private string ftpUserID;
        private string ftpPassword;
        private string ftpURI;

        public string FTPURI
        {
            get { return this.ftpURI; }
        }

        /// <summary>
        /// 连接FTP
        /// </summary>
        /// <param name="FtpServerIP">FTP连接地址</param>
        /// <param name="FtpRemotePath">指定FTP连接成功后的当前目录, 如果不指定即默认为根目录</param>
        /// <param name="FtpUserID">用户名</param>
        /// <param name="FtpPassword">密码</param>
        public void FtpWeb(string path)
        {
            XmlDocument doc = GetSecurityDoc();
            var node = doc.SelectSingleNode("/root/ftp/item");
            if (node == null) throw new Exception("请求信息未配置！");

            ftpServerIP = node.Attributes["url"].Value;
            ftpRemotePath = node.Attributes["path"].Value;
            ftpUserID = node.Attributes["user"].Value;
            ftpPassword = node.Attributes["psw"].Value;
            ftpURI = ftpServerIP + "/" + ftpRemotePath + path + "/";
        }
        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="filename"></param>
        public bool Upload(string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            if (fileInf.Length <= 0)
            {
                OperationUtils.Logger.WriteLog("文件大小为0KB，不处理，文件名称：" + filename + "");
                try
                {
                    System.IO.File.Delete(filename);
                }
                catch (Exception ex)
                {
                    OperationUtils.Logger.WriteLog("删除文件异常=" + filename + "Exception:" + ex.ToString());
                }
                return false;
            }
            string uri = ftpURI + fileInf.Name;
            FtpCheckDirectoryExist(uri);
            FtpWebRequest reqFTP = Connect(uri);
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UsePassive = true;
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            try
            {
                //using (FileStream fs = fileInf.OpenRead())
                using (FileStream fs = new FileStream(fileInf.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (Stream stream = reqFTP.GetRequestStream())
                    {
                        //设置缓冲大小
                        int BufferLength = 5120;
                        byte[] b = new byte[BufferLength];
                        int i;
                        while ((i = fs.Read(b, 0, BufferLength)) > 0)
                        {
                            stream.Write(b, 0, i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OperationUtils.Logger.WriteLog("Ftp Upload Exception=" + ex.ToString());
                try
                {
                    reqFTP.Abort();
                    reqFTP = null;
                }
                catch (Exception e)
                {
                    OperationUtils.Logger.WriteLog("Ftp Upload reqFTP Abort=" + e.ToString());
                };
                return false;
            }
            return true;
        }
        public static XmlDocument GetSecurityDoc()
        {
            string filePath = "ftp/FtpConfig.xml";
            filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, filePath);
            if (!File.Exists(filePath)) throw new Exception("程式未配置！");
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            return doc;
        }
        public void FtpCheckDirectoryExist(string destFilePath)
        {
            string fullDir = FtpParseDirectory(destFilePath);
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
                        FtpMakeDir(curDir);
                    }
                    catch (Exception e)
                    {
                        OperationUtils.Logger.WriteLog("Ftp FtpMakeDir=" + e.ToString());
                    }
                }
            }
        }

        public static string FtpParseDirectory(string destFilePath)
        {
            return destFilePath.Substring(0, destFilePath.LastIndexOf("/"));
        }



        private FtpWebRequest Connect(String path)//连接ftp  
        {
            FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            return reqFTP;
        }

        //创建目录  
        public Boolean FtpMakeDir(string localFile)
        {
            try
            {
                string uri = ftpServerIP + localFile;
                FtpWebRequest reqFTP = Connect(uri);
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                //OperationUtils.Logger.WriteLog("Ftp FtpMakeDi FtpWebRequest ftpServerIP=" + ftpServerIP + "localFile=" + localFile + "Exception=" + e.ToString());
                return false;
            }
            return true;
        }
    }

    public class DownloadToos
    {
        private WebClient client;
        private string tempFilePath;
        private string CompanyId;
        private string UserId;
        private string Foldername;

        public DownloadToos(string companyid, string userid,string foldername)
        {
            CompanyId = companyid;
            UserId = userid;
            Foldername = foldername;
            client = new WebClient();
            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
        }

        public void DownloadFileAsync(Uri serveruri, string filepath)
        {
            try
            {
                tempFilePath = filepath;
                client.DownloadFileAsync(serveruri, filepath);
            }
            catch (Exception ex)
            {
                OperationUtils.Logger.WriteLog("client.DownloadFileAsync=" + ex.ToString());
            }
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var responseHeaders = client.ResponseHeaders;
            if (responseHeaders == null)
            {
                if (e.Error != null)
                {
                    if (e.Error.InnerException != null)
                    {
                        OperationUtils.Logger.WriteLog("responseHeaders.e.Error.InnerException.Message=" + e.Error.InnerException.Message);
                    }
                    else
                    {
                        OperationUtils.Logger.WriteLog("e.Error.Message=" + e.Error.Message);
                    }
                }
                client.CancelAsync();
                client.Dispose();
                return;
            }

            if (responseHeaders["ContentError"] != null)
            {
                string error = HttpUtility.UrlDecode(responseHeaders["ContentError"]);
                OperationUtils.Logger.WriteLog("responseHeaders ContentError=" + error);
                File.Delete(tempFilePath);
                //OperationUtils.Logger.WriteLog("responseHeaders ContentError=" + error);
            }
            else
            {
                if (e.Error != null)
                {
                    if (e.Error.InnerException != null)
                    {
                        OperationUtils.Logger.WriteLog("e.Error.InnerException.Message=" + e.Error.InnerException.Message);
                    }
                    else
                    {
                        OperationUtils.Logger.WriteLog("e.Error.Message=" + e.Error.Message);
                    }
                }
                else
                {
                    FtpHelp dd = new FtpHelp();
                    if(string.IsNullOrEmpty(Foldername))
                    {
                        dd.FtpWeb(CompanyId + "/" + UserId + "/" + DateTime.Now.ToString("yyyyMMdd"));
                    }else
                    {
                        dd.FtpWeb(CompanyId + "/" + UserId + "/" + DateTime.Now.ToString("yyyyMMdd")+"/"+Foldername);
                    }

                    OperationUtils.Logger.WriteLog(string.Format("Ftp=开始上传本地文档:{0} 到FTP：{1}", tempFilePath, dd.FTPURI));
                    bool isupload = false;
                    try
                    {
                        isupload = dd.Upload(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        OperationUtils.Logger.WriteLog("Ftp 上传错误，错误异常：" + ex.ToString());
                    }
                    if (isupload)
                    {
                        OperationUtils.Logger.WriteLog(string.Format("Ftp=上传本地文档:{0} 到FTP：{1}，成功", tempFilePath, dd.FTPURI));
                    }
                }
            }
            client.CancelAsync();
            client.Dispose();
        }
    }
}
