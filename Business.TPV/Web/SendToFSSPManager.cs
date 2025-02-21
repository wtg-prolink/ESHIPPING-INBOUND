using Business.Service;
using Business.TPV.Base;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.TPV.Web
{
    public abstract class SendToFSSPManager<T> : ManagerBase
    {
        public string json = string.Empty;
        public string returnJson = string.Empty;
        public ResultInfo ExportEDI(Runtime runtime, string mode)
        {
            ResultInfo result = null;
            try
            {
                EDIConfig config = Business.TPV.Context.GetEDIConfigFromList("FSSP", runtime.PartyNo);
                if (config == null)
                    result = NoneConfigResult(string.Format("EDI:{0}", runtime.PartyNo));
                if (result == null)
                {
                    T info = CreateEdiInfos(runtime);
                    if (info == null)
                        result = ResultInfo.NullDataResult();

                    if (result == null)
                    {
                        string resultmsg = OperateData(info, runtime, config);
                        if (string.IsNullOrEmpty(resultmsg))
                            resultmsg = "Successfully";
                        result = new ResultInfo
                        {
                            IsSucceed = true,//result.flag,
                            ResultCode = "Succeed",//result.code,
                            Description = resultmsg //result.msg
                        };
                    }
                }
            }
            catch (Exception e)
            {
                result = new ResultInfo
                {
                    IsSucceed = false,
                    ResultCode = ResultCode.UnKnow,
                    Description = e.Message
                };
            }
            if (result.ResultCode != "NullData")
                SetLog(runtime, mode, result.Description, json, result.IsSucceed);
            return result;
        }

        protected abstract string OperateData(T info, Runtime runtime, EDIConfig config);

        protected abstract T CreateEdiInfos(Runtime runtime);

        public void SetLog(Runtime runtime, string mode, string errorMsg, string json, bool isSuccess)
        {
            MixedList ml = new MixedList();
            FSSPEDILog log = new FSSPEDILog(runtime, mode);
            string message = errorMsg;
            if (!string.IsNullOrEmpty(returnJson))
                message = message + ":" + returnJson;
            var v = log.CreateSucceed(runtime.RefNo, json, message);
            if (!isSuccess)
                v = log.CreateEx(message, runtime.RefNo, json);
            if (mode == "BID_D" && !string.IsNullOrEmpty(returnJson))
                ml.Add("UPDATE FSSP_TASK SET EXEC_TYPE='Y',EXEC_DATE=GETDATE() WHERE U_ID=" + SQLUtils.QuotedStr(runtime.Data.ToString()));
            Helper.WriteEdiLog(v, ml);
        }

        public string HttpPost(string url, string appid, string secret, string json)
        {
            DateTime now = DateTime.Now;
            string result = "";
            try
            {
                string time = now.ToString("yyyy-MM-dd HH:mm:ss");
                HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(url);
                Req.Method = "POST";
                Req.Accept = "*/*";
                Req.ContentType = "application/json";
                Req.KeepAlive = false;
                //Req.Timeout = 60000;
                Req.Headers["appid"] = appid;
                Req.Headers["timestamp"] = time;
                Req.Headers["appsecret"] = GetMD5(appid + ":" + time + ":" + secret);
                Req.Headers.Add("cookie", "HttpOnly=true");
                byte[] byteData = UTF8Encoding.UTF8.GetBytes(json);
                Req.ContentLength = byteData.Length;
                Req.Timeout = 60 * 60 * 1000;
                using (Stream reqStream = Req.GetRequestStream())
                {
                    reqStream.Write(byteData, 0, byteData.Length);
                }
                WebResponse response = Req.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader sr = new StreamReader(s, Encoding.UTF8);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        static string GetMD5(string str)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] data = md5Hasher.ComputeHash((new System.Text.UTF8Encoding()).GetBytes(str));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public string GetValueAsString(object value)
        {
            if (value == null)
                return "";

            if (value is DateTime)
                return Prolink.Math.GetValueAsDateTime(value).ToString("yyyy-MM-dd HH:mm:ss");
            return Prolink.Math.GetValueAsString(value);
        }

        public string ConvertToPascalCase(string value)
        {
            string[] words = Regex.Split(value, @"_");
            List<string> valList = new List<string>();
            foreach (string word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    string newWord = char.ToUpper(word[0]) + word.Substring(1).ToLower();
                    valList.Add(newWord);
                }
            }
            string output = string.Join("", valList.ToArray());
            return output;

        }

        public string ZipJson(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    byte[] mbytes = new byte[4096];

                    int cnt;

                    while ((cnt = msi.Read(mbytes, 0, mbytes.Length)) != 0)
                    {
                        gs.Write(mbytes, 0, cnt);
                    }
                }

                return Convert.ToBase64String(mso.ToArray());
            }
        }
    }
}
