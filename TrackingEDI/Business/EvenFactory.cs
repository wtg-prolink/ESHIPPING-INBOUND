using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Prolink.Data;
using Prolink.DataOperation;
using TrackingEDI.Utils;

namespace  TrackingEDI.Business{
    /// <summary>
    /// 事件工厂
    /// </summary>
    public class EvenFactory
    {
        /// <summary>
        /// 正常执行状态
        /// </summary>
        public static readonly string Normal = "N";

        /// <summary>
        /// 失败
        /// </summary>
        public static readonly string Fail = "F";

        /// <summary>
        /// 成功
        /// </summary>
        public static readonly string Success = "S";

        #region 任务执行
        /// <summary>
        /// 新增EDI排程任务
        /// </summary>
        /// <param name="keyNo">事件主表主键</param>
        /// <param name="u_id">相关主表主键</param>
        /// <param name="even_type">事件类型</param>
        /// <param name="ml">如果需要附加到自己的MixedList,请写自己的</param>
        /// <param name="activate_times">激活次数</param>
        /// <param name="process_minutes">执行时间开始的间隔时间</param>
        /// <param name="to">mail地址</param>
        /// <param name="subject">主旨</param>
        /// <param name="body">内容</param>
        /// <param name="max_error_count">最大允许出错次数</param>
        /// <param name="notify_period">多次执行间隔</param>
        /// <returns></returns>
        public static string AddEven(string keyNo, string u_id, string even_type, MixedList ml = null, int activate_times = 1, int process_minutes = 0, string to = "", string subject = "", string body = "", int max_error_count = 5, int notify_period = 30)
        {
            bool excute = false;
            if (ml == null)
            {
                ml = new MixedList();
                excute = true;
            }
            DataTable dt = Database.GetDataTable(string.Format("SELECT 1 FROM EVEN_TASK WHERE KEY_NO={0}", Prolink.Data.SQLUtils.QuotedStr(even_type + "#" + keyNo)), null);

            DateTime process_date = DateTime.Now.AddMinutes(process_minutes);
            EditInstruct ei = new EditInstruct("EVEN_TASK", EditInstruct.INSERT_OPERATION);
            if (dt.Rows.Count > 0)
            {
                return "存在相同任务";
                //ei.PutKey("KEY_NO", even_type + "#" + keyNo);
                //ei.PutKey("RESULT_STATUS", Normal);
                //ei.OperationType = EditInstruct.UPDATE_OPERATION;
                //ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            else
            {
                ei.Put("KEY_NO", even_type + "#" + keyNo);
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
            ei.Put("MAIL_TO", to);
            ei.Put("MAIL_SUBJECT", subject);
            ei.Put("MAIL_BODY", body);

            ei.Put("NOTIFY_PERIOD", notify_period);
            
            ei.Put("PROCESS_TIMES", 0);
            ei.Put("U_ID", u_id);
            ei.Put("ACTIVATE_TIMES", activate_times);
            ei.Put("MAX_ERROR_COUNT", max_error_count);
            ei.Put("RESULT_STATUS", Normal);
            ei.Put("EVEN_TYPE", even_type);
            ei.PutDate("PROCESS_DATE", process_date);
           
            ml.Add(ei);
            try
            {
                if (excute)
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { return "存在相同任务"; }
            return string.Empty;
        }

        public static string AddOnceEven(string keyNo, string u_id, string even_type, MixedList ml = null)
        {
            bool excute = false;
            if (ml == null)
            {
                ml = new MixedList();
                excute = true;
            }
            DateTime process_date = DateTime.Now;
            EditInstruct ei = new EditInstruct("EVEN_TASK", EditInstruct.INSERT_OPERATION);
            ei.Put("KEY_NO", even_type + "#" + keyNo + "#" + System.Guid.NewGuid().ToString());

            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("NOTIFY_PERIOD", 30);
            ei.Put("PROCESS_TIMES", 0);
            ei.Put("U_ID", u_id);
            ei.Put("ACTIVATE_TIMES", 1);
            ei.Put("MAX_ERROR_COUNT", 1);
            ei.Put("RESULT_STATUS", Normal);
            ei.Put("EVEN_TYPE", even_type);
            ei.PutDate("PROCESS_DATE", process_date);

            ml.Add(ei);
            try
            {
                if (excute)
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { return "存在相同任务"; }
            return string.Empty;
        }

        #region mail事件回调注册
        static TrackingEDI.Mail.MailSender Mail = null;
        static Dictionary<string, Func<MailData, MixedList, string>> _sendMailFuncs = new Dictionary<string, Func<MailData, MixedList, string>>();

        public static void RegisterSendMail(string type, Func<MailData, MixedList, string> func)
        {
            _sendMailFuncs[type] = func;
        }

        public static string[] GetMailType()
        {
            return _sendMailFuncs.Keys.ToArray();
        }
        #endregion

        #region 事件回调方法注册
        static Dictionary<string, Func<MailData, MixedList, string>> _evenFuncs = new Dictionary<string, Func<MailData, MixedList, string>>();

        public static void RegisterEvenTask(string type, Func<MailData, MixedList, string> func)
        {
            _evenFuncs[type] = func;
        }

        public static string[] GetEvenType()
        {
            return _evenFuncs.Keys.ToArray();
        }
        #endregion

        /// <summary>
        /// 执行mail发送任务
        /// </summary>
        /// <param name="even_type"></param>
        public static void ExecuteMailEven(string even_type = "MM")
        {
            if (Mail == null)
                Mail = new TrackingEDI.Mail.MailSender(Prolink.Web.WebContext.GetInstance().GetProperty("mail-server"));
            if (!_sendMailFuncs.ContainsKey(even_type))
            {
                return;
                //throw new Exception(string.Format("不存在{0}的mail发送", even_type));
            }

            Func<MailData,MixedList, string> func = _sendMailFuncs[even_type];
            MailData data = new MailData();
            EditInstruct ei = null;
            DataTable dt = Database.GetDataTable(string.Format("SELECT * FROM EVEN_TASK WHERE EVEN_TYPE={0} AND RESULT_STATUS='N' AND PROCESS_DATE IS NOT NULL ORDER BY CREATE_DATE DESC", SQLUtils.QuotedStr(even_type)), null);
            
            //List<string> testList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                int activate_times = Prolink.Math.GetValueAsInt(dr["ACTIVATE_TIMES"]);
                int process_times = Prolink.Math.GetValueAsInt(dr["PROCESS_TIMES"]);
                int notify_period = Prolink.Math.GetValueAsInt(dr["NOTIFY_PERIOD"]);//多次的执行间隔
                int max_error_count = Prolink.Math.GetValueAsInt(dr["MAX_ERROR_COUNT"]);
                int error_count = Prolink.Math.GetValueAsInt(dr["ERROR_COUNT"]);
                if (activate_times <= process_times)
                    continue;
                if (max_error_count > 0 && error_count >= max_error_count)
                    continue;
                DateTime process_date = (DateTime)dr["PROCESS_DATE"];
                if (process_date.CompareTo(DateTime.Now) > 0)
                    continue;
                string u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                //if (keys == null || keys.Length < 3)
                //    continue;
                //if (testList.Contains(u_id))
                //    continue;
                //testList.Add(u_id);//处理同一笔提单的货况
                MixedList ml = new MixedList();
                ei = new EditInstruct("EVEN_TASK", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("KEY_NO", key_no);
                ml.Add(ei);
                data.EvenNo = u_id;
                data.ProcessTimes = process_times;
                data.ErrorCount = error_count;
                data.MailClient = Mail;
                data.Keys = keys;
                data.To = Prolink.Math.GetValueAsString(dr["MAIL_TO"]);
                data.Body = Prolink.Math.GetValueAsString(dr["MAIL_BODY"]);
                data.Subject = Prolink.Math.GetValueAsString(dr["MAIL_SUBJECT"]);

                string error = func(data, ml);
                if (string.IsNullOrEmpty(error))
                {
                    ei.PutExpress("PROCESS_TIMES", "ISNULL(PROCESS_TIMES,0)+1");
                    ei.PutDate("PROCESS_DATE", process_date.AddMinutes(notify_period));
                    if ((process_times + 1) >= activate_times)
                        ei.Put("RESULT_STATUS", Success);
                }
                else
                {
                    ei.PutExpress("ERROR_COUNT", "ISNULL(ERROR_COUNT,0)+1");
                    ei.PutDate("PROCESS_DATE", process_date.AddMinutes(notify_period > 0 ? notify_period : 20));
                    if ((error_count + 1) >= max_error_count)
                        ei.Put("RESULT_STATUS", Fail);
                }
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch { }
            }
        }

        public static void ExecuteEven(string even_type = "MM")
        {
            if (Mail == null)
                Mail = new TrackingEDI.Mail.MailSender(Prolink.Web.WebContext.GetInstance().GetProperty("mail-server"));
            if (!_evenFuncs.ContainsKey(even_type))
            {
                return;
            }

            Func<MailData, MixedList, string> func = _evenFuncs[even_type];
            MailData data = new MailData();
            EditInstruct ei = null;
            DataTable dt = Database.GetDataTable(string.Format("SELECT * FROM EVEN_TASK WHERE EVEN_TYPE={0} AND RESULT_STATUS='N' AND PROCESS_DATE IS NOT NULL ORDER BY CREATE_DATE DESC", SQLUtils.QuotedStr(even_type)), null);

            //List<string> testList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                int activate_times = Prolink.Math.GetValueAsInt(dr["ACTIVATE_TIMES"]);
                int process_times = Prolink.Math.GetValueAsInt(dr["PROCESS_TIMES"]);
                int notify_period = Prolink.Math.GetValueAsInt(dr["NOTIFY_PERIOD"]);//多次的执行间隔
                int max_error_count = Prolink.Math.GetValueAsInt(dr["MAX_ERROR_COUNT"]);
                int error_count = Prolink.Math.GetValueAsInt(dr["ERROR_COUNT"]);
                if (activate_times <= process_times)
                    continue;
                if (max_error_count > 0 && error_count >= max_error_count)
                    continue;
                DateTime process_date = (DateTime)dr["PROCESS_DATE"];
                if (process_date.CompareTo(DateTime.Now) > 0)
                    continue;
                string u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                //if (keys == null || keys.Length < 3)
                //    continue;
                //if (testList.Contains(u_id))
                //    continue;
                //testList.Add(u_id);//处理同一笔提单的货况
                MixedList ml = new MixedList();
                ei = new EditInstruct("EVEN_TASK", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("KEY_NO", key_no);
                ml.Add(ei);
                data.EvenNo = u_id;
                data.ProcessTimes = process_times;
                data.ErrorCount = error_count;
                data.MailClient = Mail;
                data.Keys = keys;
                data.To = Prolink.Math.GetValueAsString(dr["MAIL_TO"]);
                data.Body = Prolink.Math.GetValueAsString(dr["MAIL_BODY"]);
                data.Subject = Prolink.Math.GetValueAsString(dr["MAIL_SUBJECT"]);
                string error = func(data, ml);
                if (string.IsNullOrEmpty(error))
                {

                    if ((process_times + 1) >= activate_times)
                    {
                        ei.OperationType = EditInstruct.DELETE_OPERATION;
                        //ei.Put("RESULT_STATUS", Success);
                    }
                    else
                    {
                        ei.PutExpress("PROCESS_TIMES", "ISNULL(PROCESS_TIMES,0)+1");
                        ei.PutDate("PROCESS_DATE", process_date.AddMinutes(notify_period));
                    }
                }
                else
                {
                    ei.PutExpress("ERROR_COUNT", "ISNULL(ERROR_COUNT,0)+1");
                    ei.PutDate("PROCESS_DATE", process_date.AddMinutes(notify_period > 0 ? notify_period : 20));
                    if ((error_count + 1) >= max_error_count)
                        ei.Put("RESULT_STATUS", Fail);
                }
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch { }
            }
        }

        public static void SaveMailLog(MailData data, string msg, string status, MixedList ml = null)
        {
            EditInstruct ei = new EditInstruct("MAIL_LOG", EditInstruct.INSERT_OPERATION);
            ei.Put("SEQ_NO", System.Guid.NewGuid().ToString("N"));
            ei.Put("EVEN_NO", data.EvenNo);
            string mailto = data.To;
            if (mailto.Length > 500)
            {
                mailto = mailto.Substring(0, 480);
            }
            ei.Put("MAIL_TO", mailto);
            if (data.Subject != null && data.Subject.Length > 500)
            {
                ei.Put("MAIL_SUBJECT", data.Subject.Substring(0, 500));
            }
            else
            {
                ei.Put("MAIL_SUBJECT", data.Subject);
            }
            ei.Put("MAIL_BODY", data.Body);
            ei.Put("MSG", msg);
            ei.Put("SEND_STATUS", status);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            if (ml == null)
                Database.ExecuteUpdate(ei);
            else
                ml.Add(ei);
        }

        /// <summary>
        /// 执行货况任务
        /// </summary>
        /// <param name="even_type"></param>
        public static void ExecuteStatusEven(string even_type = "ST")
        {
            EditInstruct ei = null;
            DataTable dt = Database.GetDataTable(string.Format("SELECT * FROM EVEN_TASK WITH(NOLOCK) WHERE EVEN_TYPE={0} AND RESULT_STATUS='N' AND PROCESS_DATE IS NOT NULL ORDER BY CREATE_DATE DESC", SQLUtils.QuotedStr(even_type)), null);

            List<string> testList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                int activate_times = Prolink.Math.GetValueAsInt(dr["ACTIVATE_TIMES"]);
                int process_times = Prolink.Math.GetValueAsInt(dr["PROCESS_TIMES"]);
                int notify_period = Prolink.Math.GetValueAsInt(dr["NOTIFY_PERIOD"]);
                int max_error_count = Prolink.Math.GetValueAsInt(dr["MAX_ERROR_COUNT"]);
                int error_count = Prolink.Math.GetValueAsInt(dr["ERROR_COUNT"]);

                if (activate_times <= process_times)
                    continue;
                DateTime process_date = (DateTime)dr["PROCESS_DATE"];
                if (process_date.CompareTo(DateTime.Now) > 0)
                    continue;
                string u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                if (keys == null || keys.Length < 3)
                    continue;
                if (testList.Contains(u_id))
                    continue;
                testList.Add(u_id);//处理同一笔提单的货况

                DataRow bl = GetTkblRow(u_id);
                if (bl == null)
                    continue;

                DataRow[] drs = dt.Select(string.Format("U_ID={0}", SQLUtils.QuotedStr(u_id)));
                DataTable statusDt = GetStatusByJobNo(u_id);

                string master_no = Prolink.Math.GetValueAsString(bl["MASTER_NO"]);
                string house_no = Prolink.Math.GetValueAsString(bl["HOUSE_NO"]);
                string origin_forwarder = Prolink.Math.GetValueAsString(bl["ORIGIN_FORWARDER"]);
                List<EvenData> evenList = GetPostUrl(u_id, even_type, master_no);
                foreach (EvenData data in evenList)
                {
                    switch (data.Type)
                    {
                        case "WebService":
                            SendWebService(dr, u_id, drs, statusDt, master_no, house_no, origin_forwarder, data);
                            break;
                        case "八达通":
                            Send8DaTong(dr, u_id, drs, statusDt, master_no, house_no, origin_forwarder, data);
                            break;
                        case "ftp":
                            SendFtp(dr, u_id, drs, statusDt, master_no, house_no, origin_forwarder, data);
                            break;
                        case "ftp_csv"://威达ftp的数据
                            SendCsvToFtp(dr, u_id, drs, statusDt, master_no, house_no, origin_forwarder, data);
                            break;
                    }
                }
                //ExecuteStatus(keys, u_id);
                ei = new EditInstruct("EVEN_TASK", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", u_id);
                ei.PutKey("EVEN_TYPE", even_type);

                ei.PutDate("PROCESS_DATE", process_date.AddMinutes(notify_period));
                //ei.PutExpress("PROCESS_TIMES", "PROCESS_TIMES+1");
                ei.Put("PROCESS_TIMES", process_times + 1);
                if ((process_times + 1) >= activate_times)
                    ei.Put("RESULT_STATUS", Success);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }
        #endregion

        #region 各种发送任务
        /// <summary>
        /// 消息推送
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="u_id"></param>
        /// <param name="drs"></param>
        /// <param name="statusDt"></param>
        private static void PushApp(DataRow dr, string u_id, DataRow[] drs, DataTable statusDt)
        {
            List<string> checkList = new List<string>();
            DataTable noticeDt = null;
            foreach (DataRow evenDr in drs)
            {
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                string seq_no = keys[2];
                if (checkList.Contains(seq_no))
                    continue;
                checkList.Add(seq_no);
                DataRow[] statusDrs = statusDt.Select(string.Format("SEQ_NO={0}", SQLUtils.QuotedStr(seq_no)));
                if (statusDrs.Length <= 0)
                    continue;
                if (noticeDt == null)
                {
                    //noticeDt = Database.GetDataTable(string.Format("SELECT * FROM TKBL_NOTICE WITH(NOLOCK) WHERE JOB_NO={0}", SQLUtils.QuotedStr(u_id)), null);
                }
            }
        }

        /// <summary>
        /// 发送ftp
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="u_id"></param>
        /// <param name="drs"></param>
        /// <param name="statusDt"></param>
        /// <param name="master_no"></param>
        /// <param name="house_no"></param>
        /// <param name="origin_forwarder"></param>
        /// <param name="data"></param>
        private static void SendFtp(DataRow dr, string u_id, DataRow[] drs, DataTable statusDt, string master_no, string house_no, string origin_forwarder, EvenData data)
        {
            string csv_data = string.Empty;
            string xml_file = string.Empty;
            List<string> checkList = new List<string>();
            foreach (DataRow evenDr in drs)
            {
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                string seq_no = keys[2];
                if (checkList.Contains(seq_no))
                    continue;
                checkList.Add(seq_no);
                DataRow[] statusDrs = statusDt.Select(string.Format("SEQ_NO={0}", SQLUtils.QuotedStr(seq_no)));
                if (statusDrs.Length <= 0)
                    continue;

                string code = Prolink.Math.GetValueAsString(statusDrs[0]["STS_CD"]);
                string event_descp = Prolink.Math.GetValueAsString(statusDrs[0]["STS_DESCP"]);
                string Location = Prolink.Math.GetValueAsString(statusDrs[0]["LOCATION"]);
                string locatioin_Name = Prolink.Math.GetValueAsString(statusDrs[0]["LOCATION_DESCP"]);
                string dateStr = ((DateTime)statusDrs[0]["EVEN_DATE"]).ToString("yyyyMMddHHmmss");
                string sts_remark = !string.IsNullOrEmpty(event_descp) ? event_descp : code;
                string xml_body = GetXmlBody(master_no, house_no, code, event_descp, Location, locatioin_Name, dateStr);

                xml_file = master_no + "_" + code + dateStr + ".xml";
                try
                {
                    UploadFtp(data.User, data.Password, xml_file, data.Url, Encoding.UTF8.GetBytes(xml_body));
                    SaveLog(data.PartyNo + ";ftp:" + data.Url + ";上传路径:" + xml_file, xml_body, "ftp上传成功", data.MasterNo, data.EvenType, data.JobNo);
                }
                catch (Exception e)
                {
                    SaveLog(data.PartyNo + ";ftp:" + data.Url + ";上传路径:" + xml_file, xml_body, "ftp上传异常:" + e.Message, data.MasterNo, data.EvenType, data.JobNo, "N");
                }
            }
        }

        /// <summary>
        /// 发送WebService
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="u_id"></param>
        /// <param name="drs"></param>
        /// <param name="statusDt"></param>
        /// <param name="master_no"></param>
        /// <param name="house_no"></param>
        /// <param name="origin_forwarder"></param>
        /// <param name="data"></param>
        private static void SendWebService(DataRow dr, string u_id, DataRow[] drs, DataTable statusDt, string master_no, string house_no, string origin_forwarder, EvenData data)
        {
            StringBuilder csv_body = new StringBuilder();
            string csv_data = string.Empty;
            List<string> checkList = new List<string>();
            foreach (DataRow evenDr in drs)
            {
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                string seq_no = keys[2];
                if (checkList.Contains(seq_no))
                    continue;
                checkList.Add(seq_no);
                DataRow[] statusDrs = statusDt.Select(string.Format("SEQ_NO={0}", SQLUtils.QuotedStr(seq_no)));
                if (statusDrs.Length <= 0)
                    continue;

                string code = Prolink.Math.GetValueAsString(statusDrs[0]["STS_CD"]);
                string event_descp = Prolink.Math.GetValueAsString(statusDrs[0]["STS_DESCP"]);
                string Location = Prolink.Math.GetValueAsString(statusDrs[0]["LOCATION"]);
                string locatioin_Name = Prolink.Math.GetValueAsString(statusDrs[0]["LOCATION_DESCP"]);
                string dateStr = ((DateTime)statusDrs[0]["EVEN_DATE"]).ToString("yyyyMMddHHmmss");

                string xml_body = GetXmlBody(master_no, house_no, code, event_descp, Location, locatioin_Name, dateStr);
                SendWebService(data, xml_body);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private static void SendWebService(EvenData data, string xml_data)
        {
            string url = data.Url;
            try
            {
                DynamicWebServices.DynamicWebServices.PutWebService(data.Url);
                if (!string.IsNullOrEmpty(data.User))
                {
                    DynamicWebServices.DynamicWebServices.InvokeWebService(data.Url, null, "login", new object[] { data.User, data.Password });
                }
                object result = DynamicWebServices.DynamicWebServices.InvokeWebService(data.Url, null, data.Method, new object[] { xml_data });
                SaveLog(data.PartyNo + ";WebService:" + url, xml_data, "WebService调用结果:" + Prolink.Math.GetValueAsString(result), data.MasterNo, data.EvenType, data.JobNo);
            }
            catch (Exception e)
            {
                SaveLog(data.PartyNo + ";WebService:" + url, xml_data, "WebService调用结果:" + Prolink.Math.GetValueAsString(e.Message), data.MasterNo, data.EvenType, data.JobNo, "N");
            }
        }

        /// <summary>
        /// 发送八达通
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="u_id"></param>
        /// <param name="drs"></param>
        /// <param name="statusDt"></param>
        /// <param name="master_no"></param>
        /// <param name="house_no"></param>
        /// <param name="origin_forwarder"></param>
        /// <param name="data"></param>
        private static void Send8DaTong(DataRow dr, string u_id, DataRow[] drs, DataTable statusDt, string master_no, string house_no, string origin_forwarder, EvenData data)
        {
            StringBuilder csv_body = new StringBuilder();
            string csv_data = string.Empty;
            List<string> checkList = new List<string>();
            foreach (DataRow evenDr in drs)
            {
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                string seq_no = keys[2];
                if (checkList.Contains(seq_no))
                    continue;
                checkList.Add(seq_no);
                DataRow[] statusDrs = statusDt.Select(string.Format("SEQ_NO={0}", SQLUtils.QuotedStr(seq_no)));
                if (statusDrs.Length <= 0)
                    continue;
                string code = Prolink.Math.GetValueAsString(statusDrs[0]["STS_CD"]);
                string event_descp = Prolink.Math.GetValueAsString(statusDrs[0]["STS_DESCP"]);
                string Location = Prolink.Math.GetValueAsString(statusDrs[0]["LOCATION"]);
                string locatioin_Name = Prolink.Math.GetValueAsString(statusDrs[0]["LOCATION_DESCP"]);
                string dateStr = ((DateTime)statusDrs[0]["EVEN_DATE"]).ToString("yyyy-MM-dd HH:mm");
                string sts_remark = !string.IsNullOrEmpty(event_descp) ? event_descp : code;
                string url_data = System.Web.HttpUtility.UrlPathEncode(house_no.Replace("/", ",").Replace("\\", ",")) + "/"
                   + System.Web.HttpUtility.UrlPathEncode(dateStr) + "/"
                   + System.Web.HttpUtility.UrlPathEncode(locatioin_Name.Replace("/", ",").Replace("\\", ",")) + "/"
                   + System.Web.HttpUtility.UrlPathEncode(sts_remark.Replace("/", ",").Replace("\\", ",")) + "/"
                   + System.Web.HttpUtility.UrlPathEncode(code.Replace("/", ",").Replace("\\", ","));
                Send8DaTongUrl(data, url_data);
            }
        }

        /// <summary>
        /// 发送八达通
        /// </summary>
        /// <param name="data"></param>
        /// <param name="url_data"></param>
        private static void Send8DaTongUrl(EvenData data, string url_data)
        {
            string url = data.Url;
            try
            {
                if (!url.EndsWith("/")) url += "/";
                url += data.User + "/";
                url += MD5Hashing.HashString(data.Password) + "/";
                url += url_data;
                WebRequest request = WebRequest.Create(url);
                request.Method = "Get";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string result = reader.ReadToEnd();
                SaveLog(data.PartyNo + ";八达通:" + url, url_data, "八达通处理结果:" + result, data.MasterNo, data.EvenType, data.JobNo);
            }
            catch (Exception e)
            {
                SaveLog(data.PartyNo + ";八达通:" + url, url_data, "八达通处理结果:" + e.Message, data.MasterNo, data.EvenType, data.JobNo, "N");
            }
        }

        /// <summary>
        /// 发送csv给ftp
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="u_id"></param>
        /// <param name="drs"></param>
        /// <param name="statusDt"></param>
        /// <param name="master_no"></param>
        /// <param name="house_no"></param>
        /// <param name="origin_forwarder"></param>
        /// <param name="data"></param>
        private static string SendCsvToFtp(DataRow dr, string u_id, DataRow[] drs, DataTable statusDt, string master_no, string house_no, string origin_forwarder, EvenData data)
        {
            StringBuilder csv_body = new StringBuilder();
            string csv_data = string.Empty;
            List<string> checkList = new List<string>();
            foreach (DataRow evenDr in drs)
            {
                string key_no = Prolink.Math.GetValueAsString(dr["KEY_NO"]);
                string[] keys = key_no.Split(new string[] { "#" }, StringSplitOptions.None);//查询主键
                string seq_no = keys[2];
                if (checkList.Contains(seq_no))
                    continue;
                checkList.Add(seq_no);
                DataRow[] statusDrs = statusDt.Select(string.Format("SEQ_NO={0}", SQLUtils.QuotedStr(seq_no)));
                if (statusDrs.Length <= 0)
                    continue;
                string code = Prolink.Math.GetValueAsString(statusDrs[0]["STS_CD"]);
                csv_data = GetCsvData(master_no, house_no, u_id, code, origin_forwarder, (DateTime)(statusDrs[0]["EVEN_DATE"]));
                if (!string.IsNullOrEmpty(csv_data))
                {
                    if (csv_body.Length > 0)
                        csv_body.Append(System.Environment.NewLine);
                    csv_body.Append(csv_data);
                }
            }
            string path = "ANDASHUN_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            if (csv_body.Length > 0)
            {
                try
                {
                    UploadFtp(data.User, data.Password, path, data.Url, Encoding.UTF8.GetBytes(csv_body.ToString()));
                    SaveLog(data.PartyNo+";ftp:" + data.Url + ";上传路径:" + path, csv_body.ToString(), "ftp上传成功", data.MasterNo, data.EvenType, data.JobNo);
                }
                catch (Exception e)
                {
                    SaveLog(data.PartyNo + ";ftp:" + data.Url + ";上传路径:" + path, csv_body.ToString(), "ftp上传异常:" + e.Message, data.MasterNo, data.EvenType, data.JobNo, "N");
                    return e.Message;
                }
            }
            return string.Empty;
        }
        #endregion

        #region 生成包体数据
        /// <summary>
        /// 创建上传威达ftp的数据
        /// </summary>
        /// <param name="master_no"></param>
        /// <param name="hbl_no"></param>
        /// <param name="jobNO"></param>
        /// <param name="code"></param>
        /// <param name="locatioinTimezone"></param>
        private static string GetCsvData(string master_no, string hbl_no, string jobNO, string code, string origin_forwarder, DateTime locatioinTimezone)
        {
            string csv_body = string.Empty;
            string eventNumber = code;
            switch (eventNumber)
            {
                case "S11":
                    eventNumber = "2010";
                    break;
                case "S14":
                    eventNumber = "2011";
                    break;
                case "S15":
                    eventNumber = "2012";
                    break;
                case "S17":
                    eventNumber = "2013";
                    break;
                default:
                    eventNumber = string.Empty;
                    break;
            }

            if (!string.IsNullOrEmpty(eventNumber))
            {
                string timezome = "+00:00";
                string masterNo = master_no;
                if (!string.IsNullOrEmpty(masterNo))
                    masterNo = masterNo.Replace("-", "");

                csv_body = origin_forwarder + ";" + hbl_no + ";" + masterNo + ";" + locatioinTimezone.ToString("yyyy-MM-ddTHH:mm:ss.fff" + timezome) + ";" + eventNumber;
                csv_body=GetCsvData(csv_body);
            }
            return csv_body;
        }

        private static string GetCsvData(string csv_body)
        {
            if (string.IsNullOrEmpty(csv_body))
                return string.Empty;
            csv_body = csv_body.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
            if (csv_body.Contains(',') || csv_body.Contains('"')
                || csv_body.Contains('\r') || csv_body.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
            {
                csv_body = string.Format("\"{0}\"", csv_body);
            }
            return csv_body;
        }

        private static string GetXmlBody(string master_no, string house_no, string code, string event_descp, string Location, string locatioin_Name, string dateStr)
        {
            string xml_body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
         + "<traceInfo><MSGHEAD><SENDER>Prolink</SENDER><GROUP_ID></GROUP_ID><CMP_ID></CMP_ID><STN_ID></STN_ID><MSGCODE></MSGCODE>"
         + "<MASTER_NO>" + master_no + "</MASTER_NO>"
         + "<HOUSE_NO>" + house_no + "</HOUSE_NO>"
         + "<MSGDETAIL>";
            xml_body += "<EVENT_SEQ></EVENT_SEQ>"
            + "<EVENT_CODE>" + code + "</EVENT_CODE>"
            + "<EVENT_DESCP>" + event_descp + "</EVENT_DESCP>"
            + "<LOCATION>" + Location + "</LOCATION>"
            + "<LOCATIOIN_NAME>" + locatioin_Name + "</LOCATIOIN_NAME>"
            + "<DATETIME>" + dateStr + "</DATETIME>";
            xml_body += "</MSGDETAIL></MSGHEAD></traceInfo>";
            return xml_body;
        }
        #endregion

        /// <summary>
        /// 获取需要抛转的party
        /// </summary>
        /// <param name="jobNO"></param>
        /// <returns></returns>
        private static List<EvenData> GetPostUrl(string jobNO, string even_type, string master_no)
        {
            List<string> postList = new List<string>();
            List<EvenData> evenList = new List<EvenData>();
            string partySql = "SELECT PARTY_NO,(SELECT TOP 1 PUSH_URL FROM TK_CMP WHERE CMP_ID=PARTY_NO AND PUSH_STATUS=1) AS POST_URL FROM TKBLPT WHERE U_ID=" + SQLUtils.QuotedStr(jobNO);
            DataTable partyDt = Database.GetDataTable(partySql, null);
            EvenData data = null;
            foreach (DataRow pt in partyDt.Rows)
            {
                string uri = Prolink.Math.GetValueAsString(pt["POST_URL"]);
                if (string.IsNullOrEmpty(uri))
                    continue;

                data = new EvenData
                {
                    PartyUrl = Prolink.Math.GetValueAsString(pt["POST_URL"]),
                    PartyNo = Prolink.Math.GetValueAsString(pt["PARTY_NO"]),
                    JobNo=jobNO,
                    MasterNo=master_no,
                    EvenType=even_type
                };
                uri = data.PartyUrl + "|" + data.PartyNo;
                if (postList.Contains(uri))
                    continue;

                string[] uris = data.PartyUrl.Split(new char[] { '|' });
                if (uris.Length > 0)
                    data.Url = uris[0];
                if (uris.Length > 1)
                {
                    if (data.Url.ToLower().Contains(".asmx") && data.Url.ToLower().Contains("http://"))
                    {
                        data.Method = uris[1];
                        data.Type = "WebService";
                    }
                    else//ftp 的情况
                    {
                        data.Port = uris[1];
                        data.Type = "ftp";
                        if ("015777".Equals(data.PartyNo))
                            data.Type = "ftp_csv";
                    }
                }
                if ("八达通".Equals(data.Port))
                    data.Type = "八达通";
                if (uris.Length > 2)
                    data.User = uris[2];
                if (uris.Length > 3)
                    data.Password = uris[3];

                evenList.Add(data);
                postList.Add(uri);
            }
            return evenList;
        }

        /// <summary>
        /// 获取提单数据
        /// </summary>
        /// <param name="MASTER_NO"></param>
        /// <returns></returns>
        private static DataRow GetTkblRow(string jobNO)
        {
            DataTable dt = Database.GetDataTable(string.Format("SELECT HOUSE_NO,TRAN_TYPE,U_ID,TRANS_FLAG,MASTER_NO,(SELECT TOP 1 SP.PARTY_NAME FROM TKBLPT SP WHERE SP.U_ID=TKBL.U_ID AND SP.PARTY_TYPE = 'OAG') AS ORIGIN_FORWARDER FROM TKBL WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(jobNO)), null);
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }

        private static DataTable GetStatusByJobNo(string job_no)
        {
            DataTable dt = Database.GetDataTable(string.Format("SELECT EVEN_DATE,CNTR_NO,STS_CD,STS_DESCP,U_ID,LOCATION,SEQ_NO,LOCATION_DESCP FROM TKBLST WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(job_no)), null);
            return dt;
        }

        #region 工具方法
        public static void UploadFtp(string userId, string pwd, string filename, string ftpPath, byte[] buff)
        {
            if (!ftpPath.ToUpper().StartsWith("ftp://"))
                ftpPath = "ftp://" + ftpPath;
            if (!ftpPath.EndsWith("/"))
                ftpPath += "/";
            FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + filename));
            // ftp用户名和密码
            reqFTP.Credentials = new NetworkCredential(userId, pwd);

            reqFTP.UsePassive = false;
            // 默认为true，连接不会被关闭
            // 在一个命令之后被执行
            reqFTP.KeepAlive = false;
            // 指定执行什么命令
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            // 指定数据传输类型
            reqFTP.UseBinary = true;
            // 上传文件时通知服务器文件的大小
            reqFTP.ContentLength = buff.Length;
            // 把上传的文件写入流
            using (Stream strm = reqFTP.GetRequestStream())
            {
                strm.Write(buff, 0, buff.Length);
            }
        }

        /// <summary>
        /// 保存log
        /// </summary>
        /// <param name="url"></param>
        /// <param name="resultdata"></param>
        /// <param name="msg"></param>
        /// <param name="mastorNo"></param>
        /// <param name="reqType"></param>
        /// <param name="port"></param>
        public static void SaveLog(string request_url, string result_data, string msg, string mastorNo, string evenType, string jobNo, string log_status="Y")
        {
            try
            {
                if (!string.IsNullOrEmpty(msg) && msg.Length > 200)
                    msg = msg.Substring(0, 200);
                EditInstruct ei = new EditInstruct("EVEN_LOG", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString("N"));
                ei.Put("EVEN_TYPE", evenType);
                ei.Put("JOB_NO", jobNo);
                ei.Put("REQUEST_URL", request_url);
                ei.Put("REQUEST_DATA", result_data);
                ei.Put("MASTER_NO", mastorNo);
                ei.Put("MSG", msg);
                ei.Put("LOG_STATUS", log_status);
                ei.PutDate("CREATE_DATE", DateTime.Now);
                Database.ExecuteUpdate(ei);
            }
            catch { }
        }
        #endregion
    }

    public class MailData
    {
        public TrackingEDI.Mail.MailSender MailClient=null;
        public string[] Keys
        {
            get;
            set;
        }
        public int ProcessTimes
        {
            get;
            set;
        }
        public int ErrorCount
        {
            get;
            set;
        }

        public string EvenNo
        {
            get;
            set;
        }

        public string To
        {
            get;
            set;
        }

        public string Subject
        {
            get;
            set;
        }

        public string Body
        {
            get;
            set;
        }
    }

    public class EvenData
    {
        /// <summary>
        /// 传送数据
        /// </summary>
        public string XmlData
        {
            get;
            set;
        }

        /// <summary>
        /// 方法
        /// </summary>
        public string Method
        {
            get;
            set;
        }

        /// <summary>
        /// web服务的网址或者是ip
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// 账号
        /// </summary>
        public string User
        {
            get;
            set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// 端口
        /// </summary>
        public string Port
        {
            get;
            set;
        }

        /// <summary>
        /// 上传文件路径
        /// </summary>
        public string UploadFilePath
        {
            get;
            set;
        }

        public string MasterNo
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string PartyNo
        {
            get;
            set;
        }

        public string PartyUrl
        {
            get;
            set;
        }

        public string EvenType
        {
            get;
            set;
        }

        public string JobNo
        {
            get;
            set;
        }
    }
}