using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Services;

namespace Business.Service
{
    /// <summary>
    /// BaseWebService 的摘要说明
    /// </summary>
    [WebService]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class BaseWebService : System.Web.Services.WebService
    {
        protected const string LoginSessionKey = "@login_session_key";
        public CredentialSoapHeader soapHeader = new CredentialSoapHeader();

        [WebMethod(Description = "登入验证", EnableSession = true)]
        public bool Login(string user, string psw)
        {
            if (Business.Utils.Context.SecurityHandler == null) throw new Exception("系统安全配置未处理!");
            ResultInfo result = null;
            CertificationInfo c = CreateCertificationInfo(user, psw);
            if (!Business.Utils.Context.SecurityHandler.Check(c, out result))
                return false;
            Session[LoginSessionKey] = true;
            return true;
        }

        protected virtual CertificationInfo CreateCertificationInfo(string user, string psw)
        {
            return new CertificationInfo(user, psw);
        }

        protected bool CheckLogin(out ResultInfo result)
        {
            result = null;
            CertificationInfo c = CreateCertificationInfo(null, null);
            if (soapHeader.CheckLogin(c, out result)) return true;
            if (Session != null && Prolink.Math.GetValueAsBool(Session[LoginSessionKey], false)) return true;
            return false;
        }

        protected bool CheckLogin(string user, string psw, out ResultInfo result)
        {
            result = null;
            CertificationInfo c = CreateCertificationInfo(user, psw);
            if (Business.Utils.Context.SecurityHandler == null) throw new Exception("系统安全配置未处理!");
            return Business.Utils.Context.SecurityHandler.Check(c, out result);
        }
    }

    public class CredentialSoapHeader : System.Web.Services.Protocols.SoapHeader
    {
        public void Initial(string userID, string password)
        {
            UserID = userID;
            PassWord = password;
        }

        public string UserID { get; set; }
        public string PassWord { get; set; }

        public bool CheckLogin(CertificationInfo c, out ResultInfo result)
        {
            try
            {
                if (Business.Utils.Context.SecurityHandler == null) throw new Exception("系统安全配置未处理!");
                if (c == null)
                    c = new CertificationInfo();
                c.User = UserID;
                c.Password = PassWord;
                if (!Business.Utils.Context.SecurityHandler.Check(c, out result))
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                result = new ResultInfo()
                {
                    ResultCode = "ValidationException",
                    Description = string.Format("sorry, you have no access authority for current web service!{0}{1}", Environment.NewLine, ex.Message)
                };
                return false;
            }
        }
    }

    //class Encrypt
    //{
    //    private static string ms_Key = System.Configuration.ConfigurationManager.AppSettings["EncryptKey"];
    //    private static string ms_IV = System.Configuration.ConfigurationManager.AppSettings["EncryptIV"];

    //    /// <summary>
    //    /// Encrypt a string
    //    /// </summary>
    //    /// <param name="ecryptString">string needs to be encrypted</param>
    //    /// <returns>the encrypted string</returns>
    //    public static string EncryptClient(string ecryptString)
    //    {
    //        if (ecryptString != "")
    //        {
    //            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
    //            cryptoProvider.Key = ASCIIEncoding.ASCII.GetBytes(ms_Key);
    //            cryptoProvider.IV = ASCIIEncoding.ASCII.GetBytes(ms_IV);
    //            MemoryStream memoryStream = new MemoryStream();
    //            CryptoStream cryptoStream = new CryptoStream(memoryStream,
    //                cryptoProvider.CreateEncryptor(), CryptoStreamMode.Write);
    //            StreamWriter streamWriter = new StreamWriter(cryptoStream);
    //            streamWriter.Write(ecryptString);
    //            streamWriter.Flush();
    //            cryptoStream.FlushFinalBlock();
    //            memoryStream.Flush();
    //            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, Int32.Parse(memoryStream.Length.ToString()));
    //        }
    //        else
    //        {
    //            return "";
    //        }
    //    }


    //    /// <summary>
    //    /// Decrypt a string
    //    /// </summary>
    //    /// <param name="decryptString">string needs to be decrypted</param>
    //    /// <returns>the decrypted string</returns>
    //    public static string DecryptClient(string decryptString)
    //    {
    //        if (decryptString != "")
    //        {
    //            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
    //            cryptoProvider.Key = ASCIIEncoding.ASCII.GetBytes(ms_Key);
    //            cryptoProvider.IV = ASCIIEncoding.ASCII.GetBytes(ms_IV);
    //            Byte[] buffer = Convert.FromBase64String(decryptString);
    //            MemoryStream memoryStream = new MemoryStream(buffer);
    //            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(), CryptoStreamMode.Read);
    //            StreamReader streamReader = new StreamReader(cryptoStream);
    //            return streamReader.ReadToEnd();
    //        }
    //        else
    //        {
    //            return "";
    //        }
    //    }

    //}
}