using EDOCApi;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace Business
{
    public class DownLoadFile
    {
        private static Prolink.EDOC_API _api = new Prolink.EDOC_API();
        private static string cloumns = "FOLDER_GUID,SERVER_NUM";
        public static void DownLoadFileToDir(string jobNo, string fileNo, string cmp , string dep, string type, bool isinbound, UserInfo userinfo,string shipmentid="")
        {
            string CompanyId = userinfo.CompanyId;
            string UserId = userinfo.UserId;
            string GroupId = userinfo.GroupId;
            string path = WebConfigurationManager.AppSettings["EDOC_EXPORT_PATH"] + CompanyId + "/" + UserId + "/" + DateTime.Now.ToString("yyyyMMdd") + "/";

            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
                System.IO.Directory.CreateDirectory(path);

            string[] queryList = jobNo.Split(',');
            string[] fileList = fileNo.Split(',');
            string[] cmplist = cmp.Split(',');
            OperationUtils.Logger.WriteLog("shipmentid:"+ shipmentid+"queryList =" + queryList.Length + ";fileList=" + fileList.Length + ";cmplist=" + cmplist.Length+ ";type"+ type);
            if (string.IsNullOrEmpty(type))
            {
                return;
            }
            List <Dictionary<string, string>> list = new List<Dictionary<string, string>>();
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
                List<Dictionary<string, string>> querylist = FileQueryDownlodInfo(queryList[k], fileList[k], GroupId, cmp, "*", dep, type, list);
            }
            OperationUtils.Logger.WriteLog("FileQueryDownlodInfo list=" + list.Count);
            if (list.Count > 0)
            {
                Random rnd = new Random();
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
                            //取文件名称
                            int istr = rnd.Next(10, 100);
                            tempExportFile = path + dummyName + FileEdocType + DateTime.Now.ToString("yyyyMMddHHmmss") + istr + FileExt.ToLower();
                        }
                        else
                        {
                            dummyName = list[i]["FiledummyName"];
                            tempExportFile = path + dummyName + DateTime.Now.ToString("HHmmssfff") + "-" + FileEdocType + FileExt.ToLower();
                        }
                        new DownloadToos(CompanyId, UserId).DownloadFileAsync(new Uri(url), tempExportFile);
                    }
                    catch (Exception ex)
                    {
                        OperationUtils.Logger.WriteLog("list 循环出错 list【i】=" + i + "Exception:" + ex.ToString());
                    }
                    //dd.FtpWeb("Dean\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    //dd.Upload(@"C:\Users\dean\Desktop\新建文本文档.txt");
                }
            }

        }

        public static List<Dictionary<string, string>> FileQueryDownlodInfo(string jobNo, string filedummy, string groupId, string cmp, string stn, string dep, string type, List<Dictionary<string, string>> filesInfo)
        {
            //_api.Login();
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


        public static void WriteLog(int index, string quot_no, List<string> msg, decimal total)
        {
            try
            {
                //OperationUtils.Logger.WriteLog("FileQueryDownlodInfo list=" + list.Count);

                string path = Prolink.Web.WebContext.GetInstance().GetProperty("EdocLog");
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMM"));
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path,   "0.txt");
                System.IO.File.AppendAllText(path, System.Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Encoding.UTF8);
                System.IO.File.AppendAllText(path, System.Environment.NewLine + string.Format("方案{0}:{1},报价:{2}", index, total, quot_no), Encoding.UTF8);
                System.IO.File.AppendAllText(path, System.Environment.NewLine + string.Join(System.Environment.NewLine, msg), Encoding.UTF8);
                System.IO.File.AppendAllText(path, System.Environment.NewLine + "-------------------------------------", Encoding.UTF8);
            }
            catch { }
        }

    }

    public class FtpHelp
    {
        private static string ftpServerIP;
        private static string ftpRemotePath;
        private static string ftpUserID;
        private static string ftpPassword;
        private static string ftpURI;

        /// <summary>
        /// 连接FTP
        /// </summary>
        /// <param name="FtpServerIP">FTP连接地址</param>
        /// <param name="FtpRemotePath">指定FTP连接成功后的当前目录, 如果不指定即默认为根目录</param>
        /// <param name="FtpUserID">用户名</param>
        /// <param name="FtpPassword">密码</param>
        public void FtpWeb(string path)
        {
           XmlDocument doc=  GetSecurityDoc();
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
                catch(Exception e)
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
                string uri =  ftpServerIP + localFile;
                FtpWebRequest reqFTP=Connect(uri);
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

    public class DownloadToos {
        private WebClient client;
        private string tempFilePath;
        private string CompanyId;
        private string UserId;

        public DownloadToos(string companyid,string userid)
        {
            CompanyId = companyid;
            UserId = userid;
            client = new WebClient();
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
        }

        public void DownloadFileAsync(Uri serveruri,string filepath)
        {
            try
            {
                tempFilePath = filepath;
                client.DownloadFileAsync(serveruri, filepath);
            }
            catch(Exception ex)
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
                    Business.FtpHelp dd = new Business.FtpHelp();
                    dd.FtpWeb(CompanyId + "/" + UserId + "/" + DateTime.Now.ToString("yyyyMMdd"));
                    OperationUtils.Logger.WriteLog("Ftp=开始:"+ tempFilePath);
                    bool isupload = false;
                    try
                    {
                        isupload=dd.Upload(tempFilePath);
                    }catch( Exception ex)
                    {
                        OperationUtils.Logger.WriteLog("Ftp 上传错误，错误异常："+ex.ToString());
                    }
                    if (isupload)
                    {
                        OperationUtils.Logger.WriteLog("上传文档成功=" + tempFilePath);
                        //删除文件
                        try
                        {
                            System.IO.File.Delete(tempFilePath);
                        }
                        catch (Exception  ex) {
                            OperationUtils.Logger.WriteLog("删除文件异常=" + tempFilePath +"Exception:"+ex.ToString());
                        }
                    }
                }
            }
            client.CancelAsync();
            client.Dispose();
        }
    }


}