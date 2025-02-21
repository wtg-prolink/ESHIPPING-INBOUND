using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using TrackingEDI.Mail;
using TrackingEDI.Utils;

namespace TrackingEDI.Business
{
    /// <summary>
    /// 文件夹档发送
    /// </summary>
    public class DocSender
    {
        /// <summary>
        /// 发送判断是否发送电子文档
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="seq_no"></param>
        /// <param name="sts_cd"></param>
        /// <param name="bl"></param>
        /// <returns></returns>
        public static string Send(string u_id, string seq_no, string sts_cd, DataRow bl = null)
        {
            if (bl == null)
            {
                DataTable blDt = Database.GetDataTable(string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(u_id)), null);
                if (blDt.Rows.Count <= 0)
                    return "提单不存在";
                bl = blDt.Rows[0];
            }
            string tran_mode = Prolink.Math.GetValueAsString(bl["TRAN_TYPE"]);
            string term = Prolink.Math.GetValueAsString(bl["INCOTERM"]);
            string freight_term = Prolink.Math.GetValueAsString(bl["FRT_PC"]);
            //string freight_term = Prolink.Math.GetValueAsString(bl["FRT_PC"]);
            string group_id = Prolink.Math.GetValueAsString(bl["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(bl["CMP"]);
            string stn = Prolink.Math.GetValueAsString(bl["STN"]);
            //string sql = string.Format("SELECT U_ID,PARTY_TYPE,PARTY_NO FROM TKPDM WHERE GROUP_ID={0} AND CMP={1} AND TRAN_MODE={2} AND TERM={3} AND STS_CD={4}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(tran_mode), SQLUtils.QuotedStr(term), SQLUtils.QuotedStr(sts_cd));
            string sql = string.Format("SELECT U_ID,PARTY_TYPE,PARTY_NO FROM TKPDM WHERE GROUP_ID={0} AND CMP={1} AND TRAN_MODE={2} AND STS_CD={3}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(tran_mode),SQLUtils.QuotedStr(sts_cd));
            DataTable docSetDt = Database.GetDataTable(sql, null);
            MixedList ml = new MixedList();
            sql = "SELECT DISTINCT PARTY_NO,PARTY_TYPE FROM TKBLPT WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            DataTable partyDt = Database.GetDataTable(sql, null);

            foreach (DataRow docSet in docSetDt.Rows)
            {
                string docId = Prolink.Math.GetValueAsString(docSet["U_ID"]);
                string party_type = Prolink.Math.GetValueAsString(docSet["PARTY_TYPE"]);
                string party_no = Prolink.Math.GetValueAsString(docSet["PARTY_NO"]);
                string doc_term = Prolink.Math.GetValueAsString(docSet["TERM"]);
                if (!string.IsNullOrEmpty(doc_term))
                {
                    if (!doc_term.Equals(term))
                        continue;
                }
                string doc_freight_term = Prolink.Math.GetValueAsString(docSet["FREIGHT_TERM"]);
                if (!string.IsNullOrEmpty(doc_freight_term))
                {
                    if (!doc_freight_term.Equals(freight_term))
                        continue;
                }

                //if (!string.IsNullOrEmpty(sql))
                //    sql += " UNION ";
                string filter = "PARTY_TYPE=" + SQLUtils.QuotedStr(party_type);
                if (!string.IsNullOrEmpty(party_no))
                    sql += " AND PARTY_NO=" + SQLUtils.QuotedStr(party_no);

                DataRow[] drs = partyDt.Select(filter);
                List<string> testList = new List<string>();
                foreach (DataRow party in drs)
                {
                    party_no = Prolink.Math.GetValueAsString(party["PARTY_NO"]);
                    if (testList.Contains(party_no))
                        continue;
                    testList.Add(party_no);
                    EvenFactory.AddEven(u_id + "#" + seq_no + "#" + party_no + "#" + docId + "#" + group_id + "#" + cmp + "#" + stn, u_id, MailManager.DocSend, ml, 1, 0);
                }
            }

            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return string.Empty;
        }

        public static string SendMail(MailData data, MixedList ml)
        {
            try
            {
                IMailTemplateParse parse = new DefaultMailParse();
                string u_id = data.Keys[1];
                string seq_no = data.Keys[2];
                string party_no = data.Keys[3];
                string docId = data.Keys[4];
                string groupId = data.Keys[5];
                string cmp = data.Keys[6];
                string stn = data.Keys[7];
                data.Subject = "货况电子文档通知";


                Dictionary<string, string> map = new Dictionary<string, string>();
                map["PROCESS_TIMES"] = data.ProcessTimes + "";
                DataTable blDt = EvenNotify.GetBLTable(u_id);
                MailTemplate.ChageDateToString(blDt, new string[] { "ETD", "ETA" }, "yyyy/MM/dd HH:mm:ss");
                DataTable baseDt = MailTemplate.GetBaseData("'TNT','TKLC'", Prolink.Math.GetValueAsString(blDt.Rows[0]["GROUP_ID"]), Prolink.Math.GetValueAsString(blDt.Rows[0]["CMP"]));

                //string tran_type = string.Empty;
                //if (blDt.Rows.Count > 0)
                //    tran_type = Prolink.Math.GetValueAsString(blDt.Rows[0]["TRAN_TYPE"]);

                blDt.Columns["TRAN_TYPE"].MaxLength = 50;
                blDt.Columns["CSTATUS"].MaxLength = 50;
                foreach (DataRow dr in blDt.Rows)
                {
                    dr["TRAN_TYPE"] = MailTemplate.GetBaseCodeValue(baseDt, "TNT", Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]));
                    dr["CSTATUS"] = MailTemplate.GetBaseCodeValue(baseDt, "TKLC", Prolink.Math.GetValueAsString(dr["CSTATUS"]));
                }

                DataTable statusDt = EvenNotify.GetStatus(u_id, seq_no);//抓取货况
                if (statusDt.Rows.Count > 0)
                {
                    map["STS_CD"] = Prolink.Math.GetValueAsString(statusDt.Rows[0]["STS_CD"]);
                    map["STS_DESCP"] = Prolink.Math.GetValueAsString(statusDt.Rows[0]["STS_DESCP"]);
                    map["LOCATION"] = Prolink.Math.GetValueAsString(statusDt.Rows[0]["LOCATION"]);
                    string even_date = string.Empty;
                    if (statusDt.Rows[0]["EVEN_DATE"] != null && statusDt.Rows[0]["EVEN_DATE"] != DBNull.Value)
                        even_date = ((DateTime)statusDt.Rows[0]["EVEN_DATE"]).ToString("yyyy/MM/dd HH:mm:ss");
                    map["EVEN_DATE"] = even_date;
                    map["STS_REMARK"] = Prolink.Math.GetValueAsString(statusDt.Rows[0]["REMARK"]);
                }


                #region 获取夹档
                string sql = string.Format("SELECT DOC_TYPE FROM TKPDD WHERE U_ID={0}", SQLUtils.QuotedStr(docId));
                string type = "";
                DataTable docTypeDt = Database.GetDataTable(sql, null);
                foreach (DataRow docType in docTypeDt.Rows)
                {
                    string doc_type = Prolink.Math.GetValueAsString(docType["DOC_TYPE"]);
                    if (string.IsNullOrEmpty(doc_type))
                        continue;

                    if (!string.IsNullOrEmpty(type))
                        type += ";";
                    type += doc_type;
                }
                StringBuilder msg = new StringBuilder();

                string body = MailTemplate.GetMailBody(MailTemplate.StatusNotify, groupId, cmp);//MT_NAME,MT_CONTENT
                if (string.IsNullOrEmpty(body))
                {
                    msg.Append(string.Format("类型为{0}的mail模板不存在;", MailTemplate.StatusNotify));
                }
                List<Dictionary<string, string>> list = MailTemplate.FileQueryDownlodInfo(u_id, groupId, cmp, "*", "", type);
                string urls = string.Empty;
                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count;i++ )
                    {
                        string url = list[i]["FileUrl"];
                        if (!string.IsNullOrEmpty(url))
                        {
                            urls += string.Format("<a href=\"{0}\">{1}</a> ", url, list[i]["FileName"]);
                        }
                    }
                    map["DOC_TEXT"] = "文档:";
                    map["DOC_URL"] = urls;
                }
                #endregion

                #region 获取发送mail群组
                DataTable mailGroupDt = MailTemplate.GetMailGroup(party_no, groupId, "U");
                if (mailGroupDt.Rows.Count <= 0)
                    msg.Append(string.Format("在mail group setup 无【{0}】建档", MailTemplate.Document));
                if (msg.Length > 0)
                {
                    EvenFactory.SaveMailLog(data, msg.ToString(), EvenFactory.Fail, ml);
                }
                #endregion
              
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    StringBuilder sb = new StringBuilder();
                    bool result = true;
                    string to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    data.Body = parse.Parse(blDt, statusDt, body, map);
                    data.To = to;
                    sb.Append(to);
                    try
                    {
                        data.MailClient.Send(data.Subject, data.To, data.Body);
                    }
                    catch (Exception e)
                    {
                        sb.Append(string.Format("【{0}】", e.Message));
                        result = false;
                    }
                    sb.Append(result ? "发送成功;" : "发送失败;");
                    EvenFactory.SaveMailLog(data, sb.ToString(), result ? EvenFactory.Success : EvenFactory.Fail, ml);
                }
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.Message, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }
    }
}
