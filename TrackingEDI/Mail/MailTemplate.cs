using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;

namespace TrackingEDI.Mail
{
    public class MailTemplate
    {
        public static readonly string Document = "DOC";
        public static readonly string AskStatus = "A";
        public static readonly string StatusNotify = "N";

        public static string GetBillingFailMailSubject(string notify_format, string group_id, string cmp)
        {
            string sql = string.Format("SELECT MT_NAME,MT_CONTENT FROM TKPMT WHERE GROUP_ID={0} AND CMP={1} AND MT_TYPE={2}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(notify_format));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
                return Prolink.Math.GetValueAsString(dt.Rows[0]["MT_NAME"]);
            return "Billing Post to FSSP Failure";
        }

        /// <summary>
        /// 获取mail内容标题主体
        /// </summary>
        /// <param name="notify_format"></param>
        /// <param name="group_id"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static string GetMailBody(string mail_format, string group_id,string cmp)
        {
            string sql = string.Format("SELECT MT_NAME,MT_CONTENT FROM TKPMT WHERE GROUP_ID={0} AND MT_TYPE={1} AND CMP={2}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(mail_format), SQLUtils.QuotedStr(cmp));
           
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string html = string.Empty;
            if (dt.Rows.Count > 0)
            {
                html = Prolink.Web.Utils.WebUtils.ReFormatHtmlText(Prolink.Math.GetValueAsString(dt.Rows[0]["MT_CONTENT"]));
                if (!string.IsNullOrEmpty(html))
                    html = html.Replace("&quot;", "\"").Replace("&amp;", "&");
            }
            return html;
        }

        /// <summary>
        /// 获取集团mail设定
        /// Q:询报价，F:FLC,L:LC,A:AIR,R:RAILREAD,E:EXPRESS,T:内贸，B:报关，D:国内快递,C:叫车，U:tracking
        /// </summary>
        /// <param name="custCd"></param>
        /// <param name="group_id"></param>
        /// <param name="mailType">groupType的类型</param>
        /// <returns></returns>
        public static DataTable GetMailGroup(string custCd, string group_id,string groupType)
        {
            string sql = string.Format(@"SELECT * FROM TKPMG WHERE GROUP_ID={0} AND CMP={1} AND (NAME IS NULL OR NAME='') {3}
            UNION SELECT * FROM TKPMG WHERE GROUP_ID={0} AND CMP={1} AND NAME={2} {3}", SQLUtils.QuotedStr(group_id),
            SQLUtils.QuotedStr(custCd), SQLUtils.QuotedStr(groupType), GetMailGroupBaseCondition());
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static string GetMailGroupBaseCondition()
        {
            return " AND (SH_CD IS NULL OR SH_CD='') AND (FC_CD IS NULL OR FC_CD='') AND (POL_CD IS NULL OR POL_CD='')";
        }

        public static List<Dictionary<string, string>> FileQueryDownlodInfo(string jobNo, string groupId, string cmp, string stn, string dep, string type)
        {
            int serverNum = 0;
            List<EDOCFileItem> edocList = GetEdoList(jobNo, groupId, cmp, stn, dep, type, ref serverNum);
            List<Dictionary<string, string>> filesInfo = new List<Dictionary<string, string>>();
            if (edocList == null)
                return filesInfo;
            var sortedList = (from a in edocList orderby a.SendTime select a).ToList();
            string path = ConfigurationManager.AppSettings["EDOC_URL1"];
            //根据Sendtime排序，最新的日期排最后面，这样后期货柜管理抓取的时候可以把最新显示出来
            if (serverNum > 0 && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["EDOC_URL_" + serverNum]))
                path = ConfigurationManager.AppSettings["EDOC_URL_" + serverNum];
            foreach (EDOCFileItem edoc in sortedList)
            {
                Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                fileInfo.Add("FileName", edoc.DummyName);
                fileInfo.Add("FileSize", edoc.Size);
                fileInfo.Add("FileExt", edoc.Ext);
                fileInfo.Add("FileUrl", path + "apis/apilaunchfile.ashx?token=" + edoc.Token + "&i=" + edoc.FileID);
                fileInfo.Add("Remark", edoc.Remark);
                filesInfo.Add(fileInfo);
            }
            return filesInfo;
        }

        public static List<EDOCFileItem> GetEdoList(string jobNo, string groupId, string cmp, string stn, string dep, string type, ref int serverNum)
        {
            Prolink.EDOC_API api = new Prolink.EDOC_API();
            //api.Login();
            string guid = api.GetFolderGUID(jobNo, groupId, cmp, stn, dep, ref serverNum, "FOLDER_GUID,SERVER_NUM");
            var result = api.Inquery(guid, type, serverNum);
            if (result == null) result = new List<EDOCFileItem>();
            return result;
        }

        /// <summary>
        /// 改变日期类型为字符串
        /// </summary>
        /// <param name="MainDt"></param>
        /// <param name="dateFields"></param>
        public static void ChageDateToString(DataTable MainDt, string[] dateFields, string format = "yyyy/MM/dd")
        {
            string name = string.Empty;
            for (int i = 0; i < dateFields.Length; i++)
            {
                MainDt.Columns.Add(dateFields[i] + "_TEMP", typeof(string));
                MainDt.Columns[dateFields[i] + "_TEMP"].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    if (dr[name] == null || dr[name] == DBNull.Value)
                        break;
                    DateTime date = (DateTime)dr[name];
                    dr[name + "_TEMP"] = date.ToString(format);
                }
            }
            for (int i = 0; i < dateFields.Length; i++)
            {
                name = dateFields[i];
                MainDt.Columns.Remove(name);
                MainDt.Columns.Add(name, typeof(string));
                MainDt.Columns[name].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    dr[name] = dr[name + "_TEMP"];
                }
            }
        }

        public static void ChageNumToFormat(DataTable MainDt, string[] dateFields, int format = 0)
        {
            string name = string.Empty;
            for (int i = 0; i < dateFields.Length; i++)
            {
                MainDt.Columns.Add(dateFields[i] + "_TEMP", typeof(string));
                MainDt.Columns[dateFields[i] + "_TEMP"].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    if (dr[name] == null || dr[name] == DBNull.Value)
                        break;
                    var val = dr[name];
                    if (format == 0) dr[name + "_TEMP"] = Prolink.Math.GetValueAsInt(val);
                    else dr[name + "_TEMP"] = Math.Round(Prolink.Math.GetValueAsDecimal(val), format);
                }
            }
            for (int i = 0; i < dateFields.Length; i++)
            {
                name = dateFields[i];
                MainDt.Columns.Remove(name);
                MainDt.Columns.Add(name, typeof(string));
                MainDt.Columns[name].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    dr[name] = dr[name + "_TEMP"];
                }
            }
            for (int i = 0; i < dateFields.Length; i++)
            {
                name = dateFields[i];
                MainDt.Columns.Remove(name + "_TEMP");
            }
        }

        /// <summary>
        /// 替换选项内容表达
        /// </summary>
        /// <param name="MainDt"></param>
        /// <param name="dateFields"></param>
        /// <param name="format"></param>
        public static void ChageExpression(DataTable MainDt, string[] dateFields, List<Dictionary<string,object>> list)
        {
            string name = string.Empty;
            for (int i = 0; i < dateFields.Length; i++)
            {
                MainDt.Columns.Add(dateFields[i] + "_TEMP", typeof(string));
                MainDt.Columns[dateFields[i] + "_TEMP"].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    if (dr[name] == null || dr[name] == DBNull.Value)
                        break;
                    string str = Prolink.Math.GetValueAsString(dr[name]);

                    Dictionary<string, object> dataset = list[i];
                    var resutl = ReturnStr(str, dataset);
                    dr[name + "_TEMP"] = resutl;
                }
            }
            for (int i = 0; i < dateFields.Length; i++)
            {
                name = dateFields[i];
                MainDt.Columns.Remove(name);
                MainDt.Columns.Add(name, typeof(string));
                MainDt.Columns[name].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    dr[name] = dr[name + "_TEMP"];
                }
            }
        }

        public static string ReturnStr(string str, Dictionary<string, object> dataset)
        {
            foreach (var list in dataset)
            {
                if (list.Key.Equals(str))
                {
                    return Prolink.Math.GetValueAsString(list.Value);
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取货况明细的mail格式
        /// </summary>
        /// <param name="statusDt"></param>
        /// <returns></returns>
        public static string GetStatusHtml(DataTable statusDt)
        {
            StringBuilder statusSb = new StringBuilder();
            statusSb.Append("<table class=\"GeneratedTable\"><tbody><tr><td class=\"title\">Status Code</td><td class=\"title\">Status Description</td><td class=\"title\">Location</td><td class=\"title\">Event Date</td></tr>");
            foreach (DataRow status in statusDt.Rows)
            {
                statusSb.Append(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                    Prolink.Math.GetValueAsString(status["STS_CD"]),
                    Prolink.Math.GetValueAsString(status["STS_DESCP"]),
                    Prolink.Math.GetValueAsString(status["LOCATION"]),
                    Prolink.Math.GetValueAsString(status["EVEN_DATE"])));
            }
            statusSb.Append("</tbody></table>");
            return statusSb.ToString();
        }

        public static string GetBaseCodeValue(DataTable baseDt, string type, string code)
        {
            DataRow[] drs = baseDt.Select(string.Format("CD_TYPE={0} AND CD={1}", SQLUtils.QuotedStr(type), SQLUtils.QuotedStr(code)));
            if (drs.Length > 0)
                return Prolink.Math.GetValueAsString(drs[0]["CD_DESCP"]);
            return code;
        }

        public static string GetBaseCodeValueEmpty(DataTable baseDt, string type, string code)
        {
            DataRow[] drs = baseDt.Select(string.Format("CD_TYPE={0} AND CD={1}", SQLUtils.QuotedStr(type), SQLUtils.QuotedStr(code)));
            if (drs.Length > 0)
                return Prolink.Math.GetValueAsString(drs[0]["CD_DESCP"]);
            return string.Empty;
        }

        public static DataRow GetBCValueByCondtions(DataTable baseDt, string type, string conditions)
        {
            if (string.IsNullOrEmpty(conditions))
                conditions = "1=0";
            DataRow[] drs = baseDt.Select(string.Format("CD_TYPE={0} AND {1}", SQLUtils.QuotedStr(type), conditions));
            if (drs.Length > 0)
                return drs[0];
            return null;
        } 

        public static string GetBaseCodeApCdValue(DataTable baseDt, string type, string code)
        {
            DataRow[] drs = baseDt.Select(string.Format("CD_TYPE={0} AND CD={1}", SQLUtils.QuotedStr(type), SQLUtils.QuotedStr(code)));
            if (drs.Length > 0)
                return Prolink.Math.GetValueAsString(drs[0]["AP_CD"]);
            return code;
        }

        public static string GetBaseCodeArCdValue(DataTable baseDt, string type, string code)
        {
            DataRow[] drs = baseDt.Select(string.Format("CD_TYPE={0} AND CD={1}", SQLUtils.QuotedStr(type), SQLUtils.QuotedStr(code)));
            if (drs.Length > 0)
                return Prolink.Math.GetValueAsString(drs[0]["AR_CD"]);
            return code;
        }

        /// <summary>
        /// 获取需要的基本建档
        /// </summary>
        /// <param name="types"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static DataTable GetBaseData(string types, string groupId,string cmp)
        {
            string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE,AP_CD,AR_CD FROM BSCODE WHERE GROUP_ID={0} AND CMP={1} AND CD_TYPE IN({2})", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp), types);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        /// <summary>
        /// 创建报价mail模板数据源 只调用一次
        /// </summary>
        /// <param name="mailDt"></param>
        public static void CreateQuotData(DataTable mailDt,string groupId,string cmp)
        {
            MailTemplate.ChageDateToString(mailDt, new string[] { "RFQ_DATE", "RFQ_FROM", "RFQ_TO", "EFFECT_FROM", "EFFECT_TO" });
            mailDt.Columns["OUT_IN"].MaxLength = 50;
            mailDt.Columns["PERIOD"].MaxLength = 50;
            mailDt.Columns["SERVICE_LOADING"].MaxLength = 50;
            mailDt.Columns["TRAN_MODE"].MaxLength = 50;
            DataTable baseDt = MailTemplate.GetBaseData("'PK','TNT'", groupId,cmp);
            foreach (DataRow dr in mailDt.Rows)
            {
                string out_in = Prolink.Math.GetValueAsString(dr["OUT_IN"]);
                if ("O".Equals(out_in))
                {
                    dr["OUT_IN"] = "O.OutBound";
                }
                else if ("I".Equals(out_in))
                {
                    dr["OUT_IN"] = "I.INBound";
                }

                string period = Prolink.Math.GetValueAsString(dr["PERIOD"]);
                if ("R".Equals(period))
                {
                    dr["PERIOD"] = "R.RFQ";
                }
                else if ("B".Equals(period))
                {
                    dr["PERIOD"] = "B.BID";
                }
                string service_mode = Prolink.Math.GetValueAsString(dr["SERVICE_MODE"]);
                dr["SERVICE_LOADING"] = MailTemplate.GetBaseCodeValue(baseDt, "PK", service_mode);
                dr["TRAN_MODE"] = MailTemplate.GetBaseCodeValue(baseDt, "TNT", Prolink.Math.GetValueAsString(dr["TRAN_MODE"]));
            }
            mailDt.Columns.Add("LSP_CD", typeof(string));
            mailDt.Columns["LSP_CD"].MaxLength = 50;
            mailDt.Columns.Add("LSP_NM", typeof(string));
            mailDt.Columns["LSP_NM"].MaxLength = 100;
        }

        public static string GetMailToByUID(string uid,Dictionary<string,string> map)
        {
            string sql = string.Format("SELECT APPROVE_BY FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_LEVEL='1'",
                SQLUtils.QuotedStr(uid));
            string userID = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (string.IsNullOrEmpty(userID))
                return "";
            map["USER"] = userID;
            sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID={0}", SQLUtils.QuotedStr(userID));
            string email = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return email;
        }
    }
}
