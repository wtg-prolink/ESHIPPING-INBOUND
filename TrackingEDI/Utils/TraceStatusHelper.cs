using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Net;

namespace TrackingEDI.Utils
{
    public class TraceStatusHelper
    {
        public static string SendIport(string u_id, DataRow bl = null,bool throwEx=false)
        {
            Dictionary<string, string> shipmentinfo = new Dictionary<string, string>();
            try
            {
                if (bl == null)
                {
                    string sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count <= 0)
                        return "No record found";
                    bl = dt.Rows[0];
                }
              
                string carrierCd = Prolink.Math.GetValueAsString(bl["CARRIER_CD"]);
                string tran_type = Prolink.Math.GetValueAsString(bl["TRAN_TYPE"]);
                string master_no = Prolink.Math.GetValueAsString(bl["MASTER_NO"]);
                string houseNo = Prolink.Math.GetValueAsString(bl["HOUSE_NO"]);

                if (!"F".Equals(tran_type) && !"L".Equals(tran_type) && !"A".Equals(tran_type))
                    return "运输类型非FCL、LCL、AIR，不发送IPORT";
                #region 获取账号密码
                DataTable accountDt = OperationUtils.GetDataTable("SELECT EDI_ACCOUNT,EDI_PWD,EDI_IP FROM EDI_TARGET WITHOUT(NOLOCK) WHERE CUST_CD='TSMS'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string url = string.Empty;
                string rq_cd = string.Empty;
                string pwd = string.Empty;
                if (accountDt.Rows.Count > 0)
                {
                    url = Prolink.Math.GetValueAsString(accountDt.Rows[0]["EDI_IP"]);
                    rq_cd = Prolink.Math.GetValueAsString(accountDt.Rows[0]["EDI_ACCOUNT"]);
                    pwd = Prolink.Math.GetValueAsString(accountDt.Rows[0]["EDI_PWD"]);
                }
                if (string.IsNullOrEmpty(rq_cd))
                    rq_cd = "TPV";
                if (string.IsNullOrEmpty(pwd))
                    pwd = "TPV12345";
                #endregion

                #region 获取SCAC代码
                string scac = string.Empty;
                if (!"A".Equals(tran_type))
                {
                    scac = Prolink.Math.GetValueAsString(bl["SCAC_CD"]);
                    string myscac = TrackingEDI.Business.BookingParser.GetScac(carrierCd);
                    if (!string.IsNullOrEmpty(myscac))
                        scac = myscac;
                }

                if (string.IsNullOrEmpty(scac))
                {
                    if ("A".Equals(tran_type))
                    {
                        if (master_no != null && master_no.Length > 3)
                            scac = master_no.Substring(0, 3);
                        else
                            scac = master_no;
                    }
                    else
                    {
                        if (master_no != null && master_no.Length > 4)
                            scac = master_no.Substring(0, 4);
                        else
                            scac = master_no;
                    }
                }
                #endregion

                Dictionary<string, object> map = new Dictionary<string, object>();
                map["RqCd"] = rq_cd;
                //map["RqCd"] = "QY";
                map["MblNo"] = master_no;
                map["HblNo"] = houseNo;
                map["TranType"] = tran_type;
                map["Vessel"] = string.Empty;
                map["Voy"] = string.Empty;

                if ("A".Equals(tran_type))
                {
                    map["SvcCd"] = "AIR";
                    map["FlightNo"] = Prolink.Math.GetValueAsString(bl["VESSEL1"]);
                }
                else
                {
                    map["SvcCd"] = "CARRIER";
                    map["Vessel"] = Prolink.Math.GetValueAsString(bl["VESSEL1"]);
                    map["Voy"] = Prolink.Math.GetValueAsString(bl["VOYAGE1"]);
                }

                map["Pol"] = Prolink.Math.GetValueAsString(bl["POL_CNTY"]) + Prolink.Math.GetValueAsString(bl["POL_CD"]);
                map["Pod"] = Prolink.Math.GetValueAsString(bl["POD_CNTY"]) + Prolink.Math.GetValueAsString(bl["POD_CD"]);
                map["Etd"] = Prolink.Math.GetValueAsString(bl["ETD"]);
                map["Eta"] = Prolink.Math.GetValueAsString(bl["ETA"]);
              
                map["Carrier"] = scac;
                map["CarrierCd"] = carrierCd;
                string shipmentId = Prolink.Math.GetValueAsString(bl["SHIPMENT_ID"]);
                string groupid = Prolink.Math.GetValueAsString(bl["GROUP_ID"]);
                string cmp = Prolink.Math.GetValueAsString(bl["CMP"]);
                string stn = Prolink.Math.GetValueAsString(bl["STN"]);
                map["Group"] = groupid;
                map["Cmp"] = cmp;
                map["RefNo"] = shipmentId;
                string smisql = string.Format("SELECT top 1 DEST_CD FROM SMSMI WHERE SHIPMENT_ID={0} AND GROUP_ID={1} AND CMP={2} ", Prolink.Data.SQLUtils.QuotedStr(shipmentId), Prolink.Data.SQLUtils.QuotedStr(groupid), Prolink.Data.SQLUtils.QuotedStr(cmp));
                string Destination = OperationUtils.GetValueAsString(smisql, Prolink.Web.WebContext.GetInstance().GetConnection());
                map["Destination"] = Destination;

                SetToken(map, pwd);
                //SetToken(map, "QY12345");
                string returnValue = string.Empty;
                JavaScriptSerializer jss = new JavaScriptSerializer();
                DataTable cntrDt = OperationUtils.GetDataTable(string.Format("SELECT distinct CNTR_NO FROM TKBLCNTR WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(u_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
               
                TraceEDIService.EDIService ediservice = new TraceEDIService.EDIService();
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                    ediservice.CookieContainer = new System.Net.CookieContainer();
                }
                catch (Exception)
                {
                }
                if (!string.IsNullOrEmpty(url))
                    ediservice.Url = url;
                //if (cntrDt.Rows.Count <= 0)
                //{
                //    map["CntrNo"] = string.Empty;
                //    returnValue = ediservice.Request(jss.Serialize(map));
                //}
                //else
                //{
                //    string cntrNos = string.Empty;
                //    foreach (DataRow dr in cntrDt.Rows)
                //    {
                //        map["CntrNo"] = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                //        returnValue = ediservice.Request(jss.Serialize(map));
                //    }
                //}

                string cntrNos = string.Empty;
                foreach (DataRow dr in cntrDt.Rows)
                {
                    string c = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                    if (string.IsNullOrEmpty(c))
                        continue;
                    if (cntrNos.Length > 0)
                        cntrNos += ";";
                    cntrNos += c;
                }
                string ediMsg = "Sent Iport ";
                map["CntrNo"] = cntrNos;
                shipmentinfo.Add("groupid", groupid);
                shipmentinfo.Add("cmp", cmp);
                shipmentinfo.Add("stn", stn);
                shipmentinfo.Add("msg", ediMsg);
                shipmentinfo.Add("Data", jss.Serialize(map));
                shipmentinfo.Add("shipmentId", shipmentId);

                returnValue = ediservice.Request(jss.Serialize(map));

                Dictionary<string, object> result = jss.Deserialize(returnValue, typeof(Dictionary<string, object>)) as Dictionary<string, object>;

                bool success = false;
                try
                {
                    if (!string.IsNullOrEmpty(returnValue))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(returnValue);
                        if (dict.ContainsKey("flag"))
                        {
                            success = Prolink.Math.GetValueAsBool(dict["flag"]);
                            if (!success)
                            {
                                if (dict.ContainsKey("msg"))
                                {
                                    ediMsg += Prolink.Math.GetValueAsString(dict["msg"]);
                                }
                            }
                        }
                    }
                }
                catch { }
                //"(" + shipmentId + ")" + ediMsg,
                WriteEdilogFunc(success, shipmentinfo);
                if (success)
                    ediMsg = "Successfully sent";
                return ediMsg;
                //return returnValue;
                //return string.Empty;
            }
            catch (Exception e)
            {
                WriteEdilogFunc(false, shipmentinfo);
                if (throwEx)
                    throw e;
                return e.Message;
            }
        }

        public static string SetToken(Dictionary<string, object> parm, string password)
        {
            string token = string.Empty;
            string mblNo = GetParmValueAsString(parm, "MblNo");
            string rqCd = GetParmValueAsString(parm, "RqCd");
            string svcCd = GetParmValueAsString(parm, "SvcCd");
            string timer = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            token = GetMD5(mblNo + rqCd + svcCd + timer + password);
            parm["timer"] = timer;
            parm["token"] = token;
            return token;
        }

        private static string GetParmValueAsString(Dictionary<string, object> parm, string name, string val = "")
        {
            return parm != null && parm.ContainsKey(name) ? Prolink.Math.GetValueAsString(parm[name]) : val;
        }

        /// <summary>
        /// 生成MD5码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static string GetMD5(string str)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] data = md5Hasher.ComputeHash((new System.Text.ASCIIEncoding()).GetBytes(str));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string WriteEdilogFunc(bool isSuccess, Dictionary<string, string> shipmentinfo)
        {
            MixedList ml = new MixedList();
            string remark = shipmentinfo["msg"];
            string groupid = shipmentinfo["groupid"];
            string cmp = shipmentinfo["cmp"];
            string stn = shipmentinfo["stn"];
            string Data = shipmentinfo["Data"];
            string shipmentid = shipmentinfo["shipmentId"];
            string FromCd = "eShipping", ToCd = "Iport", RefNO = shipmentid, ID = Guid.NewGuid().ToString("N"), EdiId = "TSMS", GroupId = groupid, Cmp = cmp, Stn = stn, CreateBy = "Sys", DataFolder = "", Rs = "Send";
            string Status = isSuccess ? "Succeed" : "Exception";
            EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", ID);
            ei.Put("EDI_ID", EdiId);
            ei.Put("SENDER", CreateBy);
            ei.Put("RS", Rs);
            ei.Put("FROM_CD", FromCd);
            ei.Put("TO_CD", ToCd);
            ei.Put("DATA_FOLDER", DataFolder);
            ei.Put("REF_NO", RefNO);
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", Cmp);
            ei.Put("STN", Stn);
            string edidata = Prolink.Math.GetValueAsString(Data);
            if (!string.IsNullOrEmpty(edidata) && ml != null)
            {
                EditInstruct edidataei = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                edidataei.Put("U_ID", ID);
                edidataei.Put("EDI_DATE", edidata);
                edidataei.PutExpress("CREATE_DATE", "getdate()");
                ml.Add(edidataei);
            }

            if (!string.IsNullOrEmpty(remark) && remark.Length > 500)
                remark = remark.Substring(0, 500);
            ei.Put("REMARK", remark);
            ei.PutExpress("EVENT_DATE", "getdate()");
            ei.Put("STATUS", Status);
            ml.Add(ei);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return "";
        }
    }
}
