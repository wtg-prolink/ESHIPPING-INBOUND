using Business.Service;
using Models.EDI;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace Business.TPV.Utils
{
    class TPVSecurityHandler : Business.Service.ISecurity
    {
        IEnumerable<CertificationInfo> GetCertificationInfo(SecurityModes mode)
        {
            XmlDocument doc = GetSecurityDoc();
            XmlNodeList nodeList = doc.GetElementsByTagName(mode.ToString());
            if (nodeList == null || nodeList.Count <= 0) yield break;
            foreach (XmlElement node in nodeList)
            {
                foreach (XmlElement child in node.ChildNodes)
                {
                    string user = child.GetAttribute("user");
                    string psw = child.GetAttribute("psw");
                    switch (mode)
                    {
                        default:
                            yield return new CertificationInfo(user, psw); break;
                    }
                }
            }
        }

        public XmlDocument GetSecurityDoc()
        {
            string filePath = "edi/Security.xml";
            filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, filePath);
            if (!File.Exists(filePath)) throw new Exception("程式未配置！");
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            return doc;
        }

        string genMD5(string str)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] data = md5Hasher.ComputeHash((new System.Text.ASCIIEncoding()).GetBytes(str));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return "0x" + sBuilder.ToString();
        }

        DataRow QueryUser(string user, string psw)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(psw)) return null;
            string sql = string.Format("SELECT * FROM SYS_ACCT WHERE U_ID={0} AND U_PASSWORD={1}", SQLUtils.QuotedStr(user), SQLUtils.QuotedStr(genMD5(psw)));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows[0];
        }

        bool CheckForDoc(TPVCertificationInfo c)
        {
            IEnumerable<CertificationInfo> infos = GetCertificationInfo(c.Mode);
            int count = infos.Where(item => item.User == c.User && item.Password == c.Password).Count();
            return count > 0;
        }

        public bool Check(CertificationInfo certificationInfo, out ResultInfo result)
        {
            result = new ResultInfo();
            TPVCertificationInfo c = certificationInfo as TPVCertificationInfo;
            if (c == null)
            {
                return false;
            }
            try
            {

                bool r = false;
                r = CheckForDoc(c);
                if (!r)
                {
                    DataRow row = QueryUser(certificationInfo.User, certificationInfo.Password);
                    r = row != null;
                }
                if (r)
                {
                    result.IsSucceed = true;
                    result.ResultCode = ResultCode.Succeed;
                    //var l = Business.Utils.Context.Logger.CreateLog("登入验证通过", this.GetType().Name);
                    //l.Data = certificationInfo;
                    //Business.Utils.Context.Logger.WriteLog(l);
                }
                else
                    result.ResultCode = SOAPResultCode.ValidationNotPass;
                return r;
            }
            catch (Exception ex)
            {
                result.ResultCode = SOAPResultCode.ValidationException;
                result.Description = string.Format("Check Exception!{0}{1}", Environment.NewLine, ex.Message);
                return false;
            }
        }
    }

    public class TPVCertificationInfo : CertificationInfo
    {
        public TPVCertificationInfo(SecurityModes mode, string user, string psw)
            : base(user, psw)
        {
            Mode = mode;
        }

        public SecurityModes Mode { get; private set; }
    }

    public enum SecurityModes
    {
        CPL, SAP, Trace, Billing, Booking, Declaration,Truck,WareHouse
    }
}
