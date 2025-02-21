using Business.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Business.EDI
{
    public class XmlWebRequst : ManagerBase
    {
        CookieContainer _currentCookie;
        public CookieContainer CurrentCookie
        {
            get
            {
                if (_currentCookie == null)
                    _currentCookie = new CookieContainer();
                return _currentCookie;
            }
        }

        public void CheckTemplate(EntityEDITemplate template)
        {
            if (template == null) throw new Exception("template is null!");
            EntityValidationResult result = null;
            if (!template.Check(out result))
                throw new EntityValidationResultException(result, true);
        }

        protected dynamic OnRequstJason(string url, string content, out string result)
        {
            WebResponse response = OnRequst(HttpContentType.Json, url, content);
            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                result = sr.ReadToEnd();
                if (!string.IsNullOrEmpty(result))
                    return Assistant.ToJsonDynamicObj(result);
                return null;
            }
        }

        enum HttpContentType { XML, Json }
        HttpWebRequest CreateHttpWebRequest(HttpContentType t, string url)
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.CookieContainer = CurrentCookie;
            wr.ServicePoint.Expect100Continue = false;
            wr.Method = "POST";
            wr.ContentType = "application/xml;charset=utf-8";
            wr.Timeout = 1200000;
            wr.KeepAlive = false;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
            return wr;
        }
        WebResponse OnRequst(HttpContentType t, string url, string content)
        {
            HttpWebRequest wr = CreateHttpWebRequest(t, url);
            if (!string.IsNullOrEmpty(content))
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(content);
                wr.ContentLength = byteArray.Length;
                if (byteArray != null && byteArray.Length > 0)
                    using (Stream rs = wr.GetRequestStream())
                    {
                        rs.Write(byteArray, 0, byteArray.Length);
                    }
            }
            return wr.GetResponse();
        }

        string GetContentType(HttpContentType t)
        {
            switch (t)
            {
                case HttpContentType.XML: return "application/xml;charset=utf-8";
                case HttpContentType.Json: return "application/json";
            }
            return string.Empty;
        }

        protected RequstResult OnRequst(XmlDocument doc, string url)
        {
            string postData = doc.InnerXml;
            string requltBackupFile = Backup(new List<string> { BackupDirName_Requst }, doc);
            WebResponse response = OnRequst(HttpContentType.XML, url, postData);
            XmlDocument result = ReadXmlResponse(response);
            string resultBackupFile = null;
            if (result != null)
            {
                resultBackupFile = Backup(new List<string> { BackupDirName_Response }, result);
            }
            return new RequstResult { ResultDoc = result, RequstBackupFile = requltBackupFile, ResultBackupFile = resultBackupFile };
        }

        protected const string BackupDirName_Response = "Response";
        protected const string BackupDirName_Requst = "Requst";
        protected const string BackupDirNameRoot = "Export";
        protected virtual string Backup(IEnumerable<string> dirConcatenates, XmlDocument doc, string fileName = "")
        {
            return Backup(dirConcatenates, doc.InnerXml, fileName);
        }

        protected virtual string Backup(IEnumerable<string> dirConcatenates, string xml, string fileName = "")
        {
            List<string> items = new List<string>();
            items.Add(BackupDirNameRoot);
            items.AddRange(dirConcatenates);
            return BackupXml(items, xml, fileName);
        }

        static XmlDocument ReadXmlResponse(WebResponse response)
        {
            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                String retXml = sr.ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(retXml);
                return doc;
            }
        }

    }

    public class RequstResult
    {
        public XmlDocument ResultDoc { get; set; }
        public string RequstBackupFile { get; set; }
        public string ResultBackupFile { get; set; }
    }
}
