using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using TrackingEDI.Mail;
using TrackingEDI.Utils;

namespace TrackingEDI.Business
{
    public class RouteNotify
    {
        static TrackingEDI.Mail.MailSender Mail = null;
        public static void Run()
        {
            if (Mail == null)
                Mail = new TrackingEDI.Mail.MailSender(Prolink.Web.WebContext.GetInstance().GetProperty("mail-server"));
            IMailTemplateParse parse = new DefaultMailParse();
            string sql = "SELECT * FROM TKRUM M LEFT JOIN TKRUD D ON  M.U_ID=D.U_ID";
            DataTable routeDt = Database.GetDataTable(sql, null);

            Dictionary<string, DataTable> mailBody = new Dictionary<string, DataTable>();
            List<string> testList = new List<string>();
            string nsts_cd = string.Empty, blno = string.Empty, sts_cd = string.Empty, group_id = string.Empty, body = string.Empty, mail_to = string.Empty;
            int working_hour = 0;
            foreach (DataRow route in routeDt.Rows)
            {
                string routeId = Prolink.Math.GetValueAsString(route["U_ID"]);
                if (testList.Contains(routeId))
                    continue;
                testList.Add(routeId);
                string tran_mode = Prolink.Math.GetValueAsString(route["TRAN_MODE"]);
                string pol = Prolink.Math.GetValueAsString(route["POL"]);
                string via = Prolink.Math.GetValueAsString(route["VIA"]);
                string pod = Prolink.Math.GetValueAsString(route["POD"]);
                string dlv = Prolink.Math.GetValueAsString(route["DLV"]);
                string carrier = Prolink.Math.GetValueAsString(route["CARRIER"]);
                string cmp = Prolink.Math.GetValueAsString(route["CMP"]);
                group_id = Prolink.Math.GetValueAsString(route["GROUP_ID"]);
                //DataTable routeDt = Database.GetDataTable(sql, null);
                sql = "SELECT * FROM TKBL WHERE TRAN_TYPE=" + SQLUtils.QuotedStr(tran_mode);
                sql += GetPort(pol, "POL");
                sql += GetPort(via, "VIA");
                sql += GetPort(pod, "POD");
                sql += GetPort(dlv, "DLV");
                sql += " AND GROUP_ID=" + SQLUtils.QuotedStr(group_id);
                sql += " AND CMP=" + SQLUtils.QuotedStr(cmp);
                DataTable blDt = Database.GetDataTable(sql, null);

                foreach (DataRow bl in blDt.Rows)
                {
                    blno = Prolink.Math.GetValueAsString(bl["U_ID"]);
                    //group_id = Prolink.Math.GetValueAsString(bl["GROUP_ID"]);
                    DataTable statusDt = Database.GetDataTable(string.Format("SELECT * FROM TKBLST WHERE U_ID={0}", SQLUtils.QuotedStr(blno)), null);

                    //获取路线的货况设定
                    DataRow[] statuss = routeDt.Select(string.Format("U_ID={0}", SQLUtils.QuotedStr(routeId)));
                    DataRow[] blstatus=null;
                    foreach (DataRow status in statuss)
                    {
                        sts_cd = Prolink.Math.GetValueAsString(status["STS_CD"]);
                        mail_to = Prolink.Math.GetValueAsString(status["MAIL_TO"]);
                        working_hour = Prolink.Math.GetValueAsInt(status["WORKING_HOUR"]);
                        blstatus = statusDt.Select(string.Format("STS_CD={0}", SQLUtils.QuotedStr(sts_cd)));
                        if (blstatus.Length <= 0)
                            continue;
                        nsts_cd = Prolink.Math.GetValueAsString(status["NSTS_CD"]);
                        if (statusDt.Select(string.Format("STS_CD={0}", SQLUtils.QuotedStr(nsts_cd))).Length <= 0)
                        {
                            DateTime even_date = DateTime.Now.AddMinutes(-1);
                            if (blstatus[0]["EVEN_DATE"] != null && blstatus[0]["EVEN_DATE"] != DBNull.Value)
                            {
                                even_date = (DateTime)blstatus[0]["EVEN_DATE"];
                                even_date = even_date.AddHours(working_hour);
                            }
                            if (even_date.CompareTo(DateTime.Now) > 0)
                                continue;
                            body = string.Empty;
                            if (!mailBody.ContainsKey(group_id))
                                mailBody[group_id] = GetMailBody(group_id);
                            if (mailBody.ContainsKey(group_id)&&mailBody[group_id] != null && mailBody[group_id].Rows.Count > 0)
                            {
                                body = Prolink.Math.GetValueAsString(mailBody[group_id].Rows[0]["MT_CONTENT"]);
                                body = Prolink.Web.Utils.WebUtils.ReFormatHtmlText(body);
                            }

                            #region 发送mail
                            //StringBuilder sb = new StringBuilder();
                            Dictionary<string, string> map = new Dictionary<string, string>();
                            DataTable bl1 = blDt.Clone();
                            bl1.ImportRow(bl);
                            EvenFactory.AddEven(blno + "#" + nsts_cd, blno, "RN", null, 1, 0, mail_to, "Route Status Notify", parse.Parse(bl1, null, body, map));
                            #endregion
                        }
                    }
                }
            }
        }

        public static string SendMail(MailData data, MixedList ml)
        {
            try
            {
                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.Message, EvenFactory.Fail,ml);
                return e.Message;
            }
            return string.Empty;
        }

        public static DataTable GetMailBody(string group_id, string notify_format = "STT")
        {
            string sql = string.Format("SELECT MT_NAME,MT_CONTENT FROM TKPMT WHERE GROUP_ID={0} AND MT_TYPE={1}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(notify_format));
            return Database.GetDataTable(sql, null);
        }

        static string GetPort(string port, string name)
        {
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(port))
            {
                if (port.Length == 3)
                    sql += string.Format(" AND {0}_CD=", name) + SQLUtils.QuotedStr(port);
                else
                {
                    if (port.Length >= 2)
                        sql += string.Format(" AND {0}_CNTY=", name) + SQLUtils.QuotedStr(port.Substring(0, 2));
                    if (port.Length >= 5)
                        sql += string.Format(" AND {0}_CD=", name) + SQLUtils.QuotedStr(port.Substring(2, 3));
                }
            }
            return sql;
        }
    }
}
