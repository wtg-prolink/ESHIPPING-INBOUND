using Business.TPV;
using Business.TPV.Utils;
using Newtonsoft.Json;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using TrackingEDI.Business;
using System.Globalization;

namespace Business
{
    public class UICHandle
    {
        public string billUId { get; set; }
        public string nextLevel { get; set; }
        public string approveType { get; set; }
        public UserInfo User { get; set; }
        public string tpvDebitNo {get;set;}
        public string debitNo { get; set; }
        public string cmp { get; set; }
        public DataTable bimDt { get; set; }
        public DataTable appDt { get; set; }
        public string amt { get; set; }
        public string cur { get; set; }

        public UICHandle(string billUid,string nextLevel,string approveType,UserInfo userInfo)
        {
            this.billUId = billUid;
            this.nextLevel = nextLevel;
            this.approveType = approveType;
            this.User = userInfo;
            string sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(billUId));
            this.bimDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (bimDt.Rows.Count > 0)
            {
                this.cmp = Prolink.Math.GetValueAsString(bimDt.Rows[0]["CMP"]);
                this.debitNo = Prolink.Math.GetValueAsString(bimDt.Rows[0]["DEBIT_NO"]);
                this.tpvDebitNo = Prolink.Math.GetValueAsString(bimDt.Rows[0]["TPV_DEBIT_NO"]);
                decimal bamt = Prolink.Math.GetValueAsDecimal(bimDt.Rows[0]["AMT"]);
                this.amt = bamt.ToString("N", CultureInfo.InvariantCulture);
                this.cur = Prolink.Math.GetValueAsString(bimDt.Rows[0]["CUR"]);
            }
            this.appDt = GetApproveDataTable();
        }

        public static Int64 GetTimestamp()
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan span = (DateTime.UtcNow - dt1970);
            return Convert.ToInt64(span.TotalSeconds);
        }

        public static string GetTokenSign(string api_appid, string auth_appId, string auth_privateKey, Int64 timestamp)
        {
            object data = new object();
            var body = JsonConvert.SerializeObject(data);

            string toSignStr = $"{api_appid}:{auth_appId}:{auth_privateKey}:{timestamp}:{body}";
            var bytes = Encoding.UTF8.GetBytes(toSignStr);
            var hash = SHA256.Create().ComputeHash(bytes);
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public string PostAPI(string type, UICDataInfo pObj, EdiInfo ediInfo)
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            string apiAppid = "MBSUIC";
            string tResult = string.Empty;
            EDIConfig ediConfig = Context.GetEDIConfigFromList("UIC", type);
            Int64 timestamp = GetTimestamp();
            string sign = GetTokenSign(apiAppid, ediConfig.User, ediConfig.Psw, timestamp);
            ediInfo.Cmp = this.cmp;
            ediInfo.RefNO = this.tpvDebitNo;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(pObj, jSetting));
            string tJson = Convert.ToBase64String(bytes);
            var postd = new { Data = tJson };
            string content = JsonConvert.SerializeObject(postd);
            if (type == "UICClose")
                content = System.Text.Encoding.UTF8.GetString(bytes);
            try
            {
                HttpWebRequest tRequest = (HttpWebRequest)HttpWebRequest.Create(ediConfig.Server);
                //指定 request 使用的 http verb
                tRequest.Method = "POST";
                //指定 request 的 content type
                tRequest.ContentType = "application/json; charset=utf-8";
                //指定 request header
                tRequest.Headers.Add("api_appid", apiAppid);
                tRequest.Headers.Add("auth_appId", ediConfig.User);
                tRequest.Headers.Add("sign", sign);    // GetTokenSign的内容
                tRequest.Headers.Add("Timestamp", Prolink.Math.GetValueAsString(timestamp));  // GetTimestamp
                using (StreamWriter tSW = new StreamWriter(tRequest.GetRequestStream()))
                {
                    tSW.Write(content);
                    tSW.Flush();
                }
                using (WebResponse tHttpResponse = (HttpWebResponse)tRequest.GetResponse())
                {
                    using (StreamReader tSR = new StreamReader(tHttpResponse.GetResponseStream()))
                    {
                        tResult = tSR.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                tResult = ex.ToString();
            }
            ediInfo.Data = "PostJson:" + content + ";ReturnJson:" + tResult;
            return tResult;
        }

        private UICDataInfo GetUICDataInfoByType(string type)
        {
            UICDataInfo uicData = new UICDataInfo();
            List<UICApproveInfo> approveInfos = new List<UICApproveInfo>();
            string approveBy = User.UserId;
            List<string> userList = new List<string>() { User.UserId };
            DataTable sysAcct = GetSysAcctDt(userList);
            switch (type)
            {
                case "UICCreate":
                    approveBy = "";
                    foreach (DataRow dr in appDt.Rows)
                    {
                        string approveLevel = Prolink.Math.GetValueAsString(dr["APPROVE_LEVEL"]);
                        string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                        if (approveLevel != "1" && approveLevel != nextLevel)
                            continue;
                        string noticeTo = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                        if (approveLevel == "1")
                        {
                            approveBy = Prolink.Math.GetValueAsString(dr["APPROVE_BY"]);
                            DateTime approveDate = Prolink.Math.GetValueAsDateTime(dr["APPROVE_DATE"]);
                            uicData.ApplyTime = approveDate.ToString("yyyy-MM-dd HH:mm:ss");
                            noticeTo = approveBy;
                            if (!string.IsNullOrEmpty(noticeTo) && !userList.Contains(noticeTo))
                                userList.Add(noticeTo);
                        }
                        else if(status=="0")
                        {
                            string href = Prolink.Web.WebContext.GetInstance().GetProperty("BillManageUrl");
                            string h5Href = Prolink.Web.WebContext.GetInstance().GetProperty("ApproveH5Url");
                            href = string.Format(href + "?mCmp={0}&mUId={1}&MenuId=AC999", cmp, noticeTo);
                            if (string.IsNullOrEmpty(cmp))
                                href = string.Format(href + "?mUId={1}", noticeTo);
                            UICApproveInfo approveInfo = new UICApproveInfo();
                            approveInfo.ApproveH5Url = h5Href + "?uid=" + billUId + "&region=CN";
                            approveInfo.ApproveUrl = href;
                            approveInfo.OwnerUser = Prolink.Math.GetValueAsString(dr["NOTICE_MAIL"]);
                            uicData.DoneUser = Prolink.Math.GetValueAsString(dr["NOTICE_MAIL"]);
                            approveInfos.Add(approveInfo);
                        }
                    }
                    sysAcct = GetSysAcctDt(userList);
                    uicData.AppCode = "eShipping";
                    uicData.AppName = "eShipping";

                    uicData.BusinessName = "物流费用请款";
                    uicData.Sn = "CN|" + billUId;
                    uicData.Urgency = "low";
                    uicData.Title = string.Format("账单号: {0}, 金额: {1}, 币别: {2}", tpvDebitNo, amt, cur);
                    uicData.ApplyBy = GetMailByUser(approveBy, sysAcct);
                    uicData.ApplyName = approveBy;
                    uicData.NextStepUsers = approveInfos;
                    uicData.PreStepUser = GetMailByUser(User.UserId, sysAcct);
                    uicData.Lang = "en-US";
                    uicData.TargetPlatform = "all";
                    uicData.StepInTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    uicData.PushChannel = "TPVPlus";
                    uicData.MType = "Todo";
                    break;
                case "UICClose":
                    uicData.OwnerUser = GetMailByUser(approveBy, sysAcct);
                    uicData.Sn = "CN|" + billUId;
                    uicData.AppCode = "eShipping";
                    break;
            }
            
            return uicData;
        }

        private DataTable GetApproveDataTable()
        {
            string sql = string.Format(@"SELECT ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL,SAP_LEVEL,APPROVE_BY,APPROVE_DATE,UIC_GUID,UIC_STATUS,STATUS FROM APPROVE_RECORD 
                            WHERE REF_NO={0} AND APPROVE_CODE={1} ORDER BY APPROVE_LEVEL ASC",
                            SQLUtils.QuotedStr(billUId), SQLUtils.QuotedStr(approveType));
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }


        public UICReturnMSG SendUICMessage(string type)
        {
            UICReturnMSG result = new UICReturnMSG();
            if (bimDt.Rows.Count <= 0 || appDt.Rows.Count<=0)
            {
                result.type = "fail";
                result.message = "Not any data processing!";
                return result;
            }
            if (type == "UICClose")
            {
                DataRow[] drs = appDt.Select("UIC_GUID IS NOT NULL AND UIC_STATUS='UICCreateSuccess'");
                if (drs.Length <= 0)
                {
                    result.type = "success";
                    result.message = "Close Success";
                    return result;
                }
            }
            EdiInfo ediInfo = new EdiInfo()
            {
                ID = System.Guid.NewGuid().ToString(),
                EdiId = "SendUIC",
                CreateBy = User.UserId,
                FromCd = "eShipping",
                Rs = "Send",
                ToCd = "UIC",
                DataFolder = "",
                Status = "Succeed",
                GroupId = "TPV",
            };
            string returnMsg = PostAPI(type, GetUICDataInfoByType(type), ediInfo);
            
            try
            {
                result = JsonConvert.DeserializeObject<UICReturnMSG>(returnMsg);
            }
            catch
            {
                result.type = "fail";
                result.message = "JSON解析异常";
            }
            ediInfo.Remark = result.message;
            if (result.type == "fail")
                ediInfo.Status = "Exception";
            if (result.type == "success")
                UpdateUICGUID(type);
            Business.TPV.Helper.WriteEdiLog(ediInfo,null);
            return result;
        }

        public DataTable GetSysAcctDt(List<string> uidList)
        {
            string sql = string.Format("SELECT U_EMAIL,CMP,U_ID,U_NAME FROM SYS_ACCT WHERE U_ID IN {0} AND U_EMAIL IS NOT NULL",
                SQLUtils.Quoted(uidList.ToArray()));
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public string GetMailByUser(string user,DataTable dt)
        {
            DataRow[] drs = dt.Select("U_ID=" + SQLUtils.QuotedStr(user));
            if(drs.Length<=0)
                return user + "@TPV-TECH.COM";
            return Prolink.Math.GetValueAsString(drs[0]["U_EMAIL"]);
        }

        public string UpdateUICGUID(string type)
        {
            string message = "";
            EditInstruct arei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            arei.PutKey("REF_NO", billUId);
            arei.PutKey("APPROVE_CODE", approveType);
            switch (type)
            {
                case "UICClose":
                    arei.PutKey("UIC_GUID", "CN|" + billUId);
                    break;
                case "UICCreate":
                    arei.PutKey("APPROVE_LEVEL", nextLevel);
                    arei.Put("UIC_GUID", "CN|" + billUId);
                    break;
            }
            arei.Put("UIC_STATUS", type + "Success");
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(arei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }
            return message;
        }

    }

    public class UICDataInfo
    { 
        public string AppCode { get; set; }
        public string AppName { get; set; }
        public string BusinessName { get; set; }
        public string Sn { get; set; }
        public string Urgency { get; set; }
        public string Title { get; set; }
        public string ApplyBy { get; set; }
        public string ApplyName { get; set; }
        public string ApplyTime { get; set; }
        public string StepInTime { get; set; }
        public string PreStepUser { get; set; }
        public string PushChannel { get; set; }
        public string TargetPlatform { get; set; }
        public string MType { get; set; }
        public string ExpiredTime { get; set; }
        public string Lang { get; set; }
        public string ExtendField1 { get; set; }
        public string ExtendField2 { get; set; }
        public string ExtendField3 { get; set; }
        public List<UICApproveInfo> NextStepUsers { get; set; }
        public string DoneUser { get; set; }
        public string OwnerUser { get; set; }
    }

    public class UICApproveInfo
    {
        public string ApproveUrl { get; set; }
        public string ApproveH5Url { get; set; }
        public string OwnerUser { get; set; }
        public string OwnerName { get; set; }
        public string OwnerNameEnglish { get; set; }
    }

    public class UICReturnMSG {
        public string code { get; set; }
        public string type { get; set; }
        public string message { get; set; }
        public string extras { get; set; }
        public string time { get; set; }
    }
}
