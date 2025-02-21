using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prolink.Data;
using System.Data;
using Prolink.DataOperation;
using System.Text;
using TrackingEDI.Mail;

namespace TrackingEDI.Business
{
    /// <summary>
    /// 事件通知类
    /// </summary>
    public class EvenNotify
    {
        /// <summary>
        /// 重置通知事件
        /// </summary>
        /// <param name="evenNo"></param>
        public static int RestEven(string evenNo)
        {
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("TKEVM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("EVEN_NO", evenNo);//CMP+E+YYMMDD+9999999
            ei.PutDate("NOTIFY_DATE", null);
            ei.Put("PROCESS_TIMES", 0);
            ei.Put("STATUS", NotifyStatus.StandBy);
            ml.Add(ei);

            string sql = string.Format("SELECT * FROM TKEVM WHERE EVEN_NO={0}", SQLUtils.QuotedStr(evenNo));
            DataTable evenDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (evenDt.Rows.Count > 0)
                ml.Add(CreateLog(evenDt.Rows[0], "开始重发", "F"));
            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return result[0];
        }

        #region 触发提单事件
        /// <summary>
        /// 提单修改时新增通知
        /// </summary>
        /// <param name="uid">提单主键</param>
        public static void Notify(string uid, MixedList ml = null, DataRow tkbl = null)
        {
            
            bool excute = false;
            if (ml == null)
            {
                ml = new MixedList();
                excute = true;
            }
            EditInstruct ei = null;
            if (tkbl == null) tkbl = GetBl(uid);
            if (tkbl == null)
                return;
            string u_id = Prolink.Math.GetValueAsString(tkbl["U_ID"]);//出货编号
            string shipment_id = Prolink.Math.GetValueAsString(tkbl["SHIPMENT_ID"]);//出货编号
            string bl_no = shipment_id;
            //string bl_no = Prolink.Math.GetValueAsString(tkbl["BL_NO"]);
            string cmp = Prolink.Math.GetValueAsString(tkbl["CMP"]);
            string group_id = Prolink.Math.GetValueAsString(tkbl["GROUP_ID"]);
            //啟動時間:START_HOUR  通知格式:NOTIFY_FORMAT  通知次數:NOTIFY_TIMES  間隔分鐘:NOTIFY_PERIOD  要求貨況:REQUEST_CD

            //2.	提單save 時,先以TKBLM的CMP+TRAN_MODE, 去 TKPEM serach看有沒有資料存在, 如果不存在就離開,表示沒有要通知的紀錄.
            DataTable evenDt = GetNotifySetting(cmp, group_id);
            if (evenDt == null || evenDt.Rows.Count <= 0)
                return;



            //获取提单party
            string sql = string.Format("SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id));
            
            DataTable partyDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM TKBL WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id));
            
            DataTable TkblDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string PodCd = "";
            string PolCd = "";
            if(TkblDt.Rows.Count > 0 )
            {
                PodCd = TkblDt.Rows[0]["POD_CD"].ToString();
                PolCd = TkblDt.Rows[0]["POL_CD"].ToString();
            }
            //Prolink.DataOperation.OperationUtils.Logger.WriteLog("EvenNotify/Notify U_ID:" + u_id + "SHIPMNET_ID:" + shipment_id);
            DataRow[] notices = null;
            foreach (DataRow dr in partyDt.Rows)
            {
                string party_type = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                string party_no = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                //notices = evenDt.Select("PARTY_TYPE=" + SQLUtils.QuotedStr(party_type) + " AND CMP=" + SQLUtils.QuotedStr(party_no));
                notices = evenDt.Select("PARTY_TYPE=" + SQLUtils.QuotedStr(party_type) + " AND POL_CD =" + SQLUtils.QuotedStr(PolCd) + " AND POD_CD =" + SQLUtils.QuotedStr(PodCd));
                if (notices == null || notices.Length <= 0)//判断有沒有設定要做通知
                    continue;                
                foreach (DataRow notice in notices)
                {
                    string notice_partyno = Prolink.Math.GetValueAsString(notice["PARTY_NO"]);
                    if (!string.IsNullOrEmpty(notice_partyno) && !notice_partyno.Equals(party_no))
                        continue;
                    ei = AddNotify(tkbl, notice, dr, NotifyStatus.StandBy);
                    if (ei == null) continue;
                    ml.Add(ei);
                }
            }
            int[] result = null;
            if (excute&&ml.Count > 0)
                result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// 添加通知记录
        /// </summary>
        /// <param name="tkbl"></param>
        /// <param name="notice"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static EditInstruct AddNotify(DataRow tkbl, DataRow notice, DataRow partyDr, string status)
        {
            string shipment_id = Prolink.Math.GetValueAsString(tkbl["SHIPMENT_ID"]);
            string bl_no = Prolink.Math.GetValueAsString(tkbl["U_ID"]);
            //string bl_no = Prolink.Math.GetValueAsString(tkbl["BL_NO"]);
            string cmp = Prolink.Math.GetValueAsString(tkbl["CMP"]);
            string tran_type = Prolink.Math.GetValueAsString(tkbl["TRAN_TYPE"]);
            string group_id = Prolink.Math.GetValueAsString(tkbl["GROUP_ID"]);
            string dep = Prolink.Math.GetValueAsString(tkbl["DEP"]);
            string stn = Prolink.Math.GetValueAsString(tkbl["STN"]);
            string notify_cd = Prolink.Math.GetValueAsString(notice["NOTIFY_CD"]);
            string request_cd = Prolink.Math.GetValueAsString(notice["REQUEST_CD"]);
            
            string party_type = Prolink.Math.GetValueAsString(notice["PARTY_TYPE"]);
            string notify_group = Prolink.Math.GetValueAsString(notice["NOTIFY_GROUP"]);
            string notify_to = Prolink.Math.GetValueAsString(partyDr["PARTY_NO"]);
            string notify_name = Prolink.Math.GetValueAsString(partyDr["PARTY_NAME"]);
            if (string.IsNullOrEmpty(dep))
                dep = "*";
            if (string.IsNullOrEmpty(stn))
                stn = "*";
            //string notify_to = "fish@pllink.com";
            string sql = string.Format("SELECT EVEN_NO,[STATUS],NOTIFY_TIMES,PROCESS_TIMES FROM TKEVM WHERE GROUP_ID={0} AND CMP={1} AND STN={2} AND DEP={3} AND BL_NO={4} AND NOTIFY_TO={5} AND NOTIFY_CD={6}",
                SQLUtils.QuotedStr(group_id),
                SQLUtils.QuotedStr(cmp),
                SQLUtils.QuotedStr(stn),
                SQLUtils.QuotedStr(dep),
                SQLUtils.QuotedStr(bl_no),
                SQLUtils.QuotedStr(notify_to),
                SQLUtils.QuotedStr(notify_cd));//AND ([STATUS]='F' OR NOTIFY_TIMES=PROCESS_TIMES)

            if (string.IsNullOrEmpty(request_cd))
                sql += " AND (REQUEST_CD='' OR REQUEST_CD IS NULL) ";
            else
                sql += " AND REQUEST_CD=" + SQLUtils.QuotedStr(request_cd);



            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            EditInstruct ei = new EditInstruct("TKEVM", EditInstruct.INSERT_OPERATION);
            Random rd = new Random();
            //string even_no = cmp + "E" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + rd.Next(9);
            string even_no = System.Guid.NewGuid().ToString("N");//CMP+E+YYMMDD+9999999
            if (dt != null && dt.Rows.Count > 0)
            {
                return null;
                //if ("F".Equals(Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"])))//已通知完成
                //    return null;
                //if (Prolink.Math.GetValueAsInt(dt.Rows[0]["NOTIFY_TIMES"]) == Prolink.Math.GetValueAsInt(dt.Rows[0]["PROCESS_TIMES"]))//已达到通知测试
                //    return null;
                //else
                //    return null;
                //ei.OperationType = EditInstruct.UPDATE_OPERATION;
                //even_no = Prolink.Math.GetValueAsString(dt.Rows[0]["EVEN_NO"]);
                //ei.PutKey("EVEN_NO", even_no);
            }
            else
            {
                ei.Put("EVEN_NO", even_no);
            }

            ei.Put("GROUP_ID", group_id);
            ei.Put("CMP", cmp);
            ei.Put("STN", stn);
            
            ei.Put("DEP", dep);
            ei.Put("TRAN_MODE", tran_type);
            //ei.Put("BL_UID", );
            ei.Put("SHIPMENT_ID", shipment_id);
            //ei.Put("TRAN_MODE", tran_mode);
            ei.Put("BL_NO", bl_no);
            ei.Put("NOTIFY_TO", notify_to);//通知人
            ei.Put("NOTIFY_NM", notify_name);
            ei.Put("PARTY_TYPE", party_type);
            
            ei.Put("NOTIFY_CD", notify_cd);//通知貨況
            ei.Put("NOTIFY_DESCP", Prolink.Math.GetValueAsString(notice["NOTIFY_DESCP"]));//通知名稱
            ei.Put("START_HOUR", notice["START_HOUR"]);//啟動時間
            ei.Put("NOTIFY_FORMAT", notice["NOTIFY_FORMAT"]);//通知格式
            ei.Put("NOTIFY_TIMES", notice["NOTIFY_TIMES"]);//通知次數
            ei.Put("PROCESS_TIMES", 0);//已通知次數
            ei.Put("NOTIFY_PERIOD", notice["NOTIFY_PERIOD"]);//間隔分鐘
            ei.Put("REQUEST_CD", request_cd);//要求貨況
            ei.Put("REQUEST_DESCP", notice["REQUEST_DESCP"]);//要求說明
            ei.Put("STATUS", status);//狀態  S:Stand By(Default) Ｆ: Finish 
            ei.Put("REMARK", Prolink.Math.GetValueAsString(notice["REMARK"]));//備註
            ei.Put("NOTIFY_GROUP", notify_group); //寄給哪個郵件群組
            ei.Put("CREATE_BY", "SYS");
            ei.PutDate("CREATE_DATE", DateTime.Now);
            //string tran_mode = Prolink.Math.GetValueAsString(tkbl["TRAN_MODE"]);

            return ei;
        }

        /// <summary>
        /// 获取相关事件通知
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="tran_mode"></param>
        /// <returns></returns>
        private static DataTable GetNotifySetting(string cmp, string group)
        {
            string sql = string.Format("SELECT * FROM TKPEM WHERE CMP={0} AND GROUP_ID={1}", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(group));
            //string sql = string.Format("SELECT * FROM TKPEM WHERE GROUP_ID={0}",SQLUtils.QuotedStr(group));
            DataTable evenDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return evenDt;
        }

        /// <summary>
        /// 获取提单资料
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static DataRow GetBl(string uid)
        {
            //获取提单资料
            DataTable tkblDt = GetBLTable(uid);
            DataRow tkbl = null;
            if (tkblDt != null && tkblDt.Rows.Count > 0)
                tkbl = tkblDt.Rows[0];
            return tkbl;
        }
        #endregion

        public static TrackingEDI.Mail.MailSender Mail = null;
        static bool _debug_set = true;
        static bool _debug_send = true;
        /// <summary>
        /// 货况进来时进行货况通知判断 REQUEST_CD
        /// </summary>
        /// <param name="uid"></param>
        public static void StatusNotify(string code = "NOTIFY_CD")
        {
            if (!_debug_set) return;
            //string sql = string.Format("SELECT E.*,S.* FROM TKEVM E LEFT JOIN TKBLST S ON E.{0}=S.STS_CD AND E.STN=S.STN AND E.GROUP_ID=S.GROUP_ID AND E.CMP=S.CMP WHERE E.PROCESS_TIMES=0 AND S.STS_CD IS NOT NULL AND [STATUS]='S'", code);
            string sql = string.Format("SELECT E.*,S.* FROM TKEVM E LEFT JOIN TKBLST S ON E.{0}=S.STS_CD AND E.BL_NO=S.U_ID WHERE E.PROCESS_TIMES=0 AND S.STS_CD IS NOT NULL AND [STATUS]='S' ORDER BY EVEN_DATE", code);
            DataTable evenDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = null;
            MixedList ml = new MixedList();
            foreach (DataRow even in evenDt.Rows)
            {
                ei = new EditInstruct("TKEVM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("GROUP_ID", even["GROUP_ID"]);
                ei.PutKey("CMP", even["CMP"]);
                ei.PutKey("STN", even["STN"]);
                ei.PutKey("DEP", even["DEP"]);
                ei.PutKey("EVEN_NO", even["EVEN_NO"]);
                int start_hour = Prolink.Math.GetValueAsInt(even["START_HOUR"]);
                if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(even["NOTIFY_DATE"])))
                    continue;
                //数据修正
                if (start_hour == 0)
                {
                    ei.PutDate("NOTIFY_DATE", DateTime.Now.AddMilliseconds(-1));//修正1分钟
                }
                else if (even["EVEN_DATE"] == null || even["EVEN_DATE"] == DBNull.Value)
                {
                    ei.PutDate("NOTIFY_DATE", DateTime.Now.AddMilliseconds(-1));//修正1分钟
                }
                else
                {
                    ei.PutDate("NOTIFY_DATE", ((DateTime)even["EVEN_DATE"]).AddHours(start_hour));
                }
                ml.Add(ei);
            }
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        static bool _sendS = false;
        /// <summary>
        /// NOTIFY_CD  REQUEST_CD
        /// </summary>
        /// <param name="code"></param>
        public static void SendNotify(string code)
        {
            #region 发送货况请求去TMSM
            //if (!_sendS)
            //{
            //    _sendS = true;
            //    try
            //    {
            //        DataTable dt1 = OperationUtils.GetDataTable("SELECT * FROM TKBL WHERE ETD>='2016-09-01' AND MASTER_NO IS NOT NULL AND TRAN_TYPE IN ('A','F','L')", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //        foreach (DataRow bl in dt1.Rows)
            //        {
            //            try
            //            {
            //                TrackingEDI.Utils.TraceStatusHelper.SendIport(Prolink.Math.GetValueAsString(bl["U_ID"]), bl);
            //            }
            //            catch { }
            //        }
            //    }
            //    catch { }
            //}
            #endregion

            if (Mail == null)
                Mail = new TrackingEDI.Mail.MailSender(Prolink.Web.WebContext.GetInstance().GetProperty("mail-server"));

            if (!_debug_send) return;
            //string sql = string.Format("SELECT E.*,S.* FROM TKEVM E LEFT JOIN TKBLST S ON E.{0}=S.STS_CD AND E.STN=S.STN AND E.GROUP_ID=S.GROUP_ID AND E.CMP=S.CMP WHERE E.PROCESS_TIMES<E.NOTIFY_TIMES AND S.STS_CD IS NOT NULL AND NOTIFY_DATE IS NOT NULL AND [STATUS]='S'", code);
            string sql = string.Format("SELECT E.*,S.* FROM TKEVM E LEFT JOIN TKBLST S ON E.{0}=S.STS_CD AND E.BL_NO=S.U_ID WHERE E.PROCESS_TIMES<E.NOTIFY_TIMES AND S.STS_CD IS NOT NULL AND NOTIFY_DATE IS NOT NULL AND [STATUS]='S'  ORDER BY EVEN_DATE DESC", code);
            DataTable evenDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = null;
            MixedList ml = null;
            //Prolink.DataOperation.OperationUtils.Logger.WriteLog("EvenNotify/SendNotify");

            List<string> checkList = new List<string>();
            foreach (DataRow even in evenDt.Rows)
            {
                #region 过滤相似事件
                string key = Prolink.Math.GetValueAsString(even["GROUP_ID"])
                    + "#" + Prolink.Math.GetValueAsString(even["CMP"])
                    + "#" + Prolink.Math.GetValueAsString(even["EVEN_NO"])
                    + "#" + Prolink.Math.GetValueAsString(even["STS_CD"])
                    + "#" + Prolink.Math.GetValueAsString(even["U_ID"])
                    + "#" + Prolink.Math.GetValueAsString(even["NOTIFY_TO"])
                    + "#" + Prolink.Math.GetValueAsString(even["REQUEST_CD"]);
                if (checkList.Contains(key))
                    continue;
                checkList.Add(key);
                #endregion

                ml = new MixedList();
                ei = new EditInstruct("TKEVM", EditInstruct.UPDATE_OPERATION);
                string even_no = Prolink.Math.GetValueAsString(even["EVEN_NO"]);
                string group_id = Prolink.Math.GetValueAsString(even["GROUP_ID"]);
                string cmp = Prolink.Math.GetValueAsString(even["CMP"]);
                string notify_format = Prolink.Math.GetValueAsString(even["NOTIFY_FORMAT"]);

                ei.PutKey("GROUP_ID", even["GROUP_ID"]);
                ei.PutKey("CMP", even["CMP"]);
                ei.PutKey("STN", even["STN"]);
                ei.PutKey("DEP", even["DEP"]);
                ei.PutKey("EVEN_NO", even_no);

                int notify_times = Prolink.Math.GetValueAsInt(even["NOTIFY_TIMES"]);
                int notify_period = Prolink.Math.GetValueAsInt(even["NOTIFY_PERIOD"]);
                int sended_count = Prolink.Math.GetValueAsInt(even["PROCESS_TIMES"]);
                DateTime notify_date = (DateTime)even["NOTIFY_DATE"];
                if (DateTime.Now.CompareTo(notify_date) >= 0)//
                {
                    ei.PutDate("NOTIFY_DATE", DateTime.Now.AddMinutes(notify_period));
                    if ((sended_count + 1) == notify_times)
                        ei.Put("STATUS", "F");
                    ei.PutExpress("PROCESS_TIMES", "PROCESS_TIMES+1");
                    ml.Add(ei);

                    SendMail(ml, even, group_id, cmp, notify_format);
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
        }

        #region mail 相关操作
        public static string RuntimePath = string.Empty;
        static Prolink.EDOC_API _api = new Prolink.EDOC_API();
        /// <summary>
        /// 发送mail
        /// </summary>
        /// <param name="ml"></param>
        /// <param name="even"></param>
        /// <param name="group_id"></param>
        /// <param name="cmp"></param>
        /// <param name="notify_format"></param>
        private static void SendMail(MixedList ml, DataRow even, string group_id, string cmp, string notify_format)
        {
            string notify_to = Prolink.Math.GetValueAsString(even["NOTIFY_TO"]);
            //string body = Prolink.Math.GetValueAsString(even["EVEN_DATE"]);
            string shipment_id = Prolink.Math.GetValueAsString(even["SHIPMENT_ID"]);
            string u_id = Prolink.Math.GetValueAsString(even["U_ID"]);//提单主键
            string notify_cd = Prolink.Math.GetValueAsString(even["NOTIFY_CD"]);
            string request_cd = Prolink.Math.GetValueAsString(even["REQUEST_CD"]);
            string notify_descp = Prolink.Math.GetValueAsString(even["NOTIFY_DESCP"]);
            string request_descp = Prolink.Math.GetValueAsString(even["REQUEST_DESCP"]);
            string notify_group = Prolink.Math.GetValueAsString(even["NOTIFY_GROUP"]);

            int sended_count = Prolink.Math.GetValueAsInt(even["PROCESS_TIMES"])+1;
            bool result = false;
            string title = "货况通知";
            Dictionary<string, string> map = new Dictionary<string, string>();

            string th = "th";
            switch (sended_count%10)
            {
                case 1: th = "st";
                    break;
                case 2: th = "nd";
                    break;
                case 3: th = "rd";
                    break;
            }
            map["PROCESS_TIMES"] = sended_count + th;
            map["EVEN_NO"] = Prolink.Math.GetValueAsString(even["EVEN_NO"]);
            
            //DataTable requestStatusDt = null;
            DataTable statusDt = GetStatus(u_id);//抓取货况
            if (!string.IsNullOrEmpty(request_cd))
            {
                title = "要求货况通知";
                //requestStatusDt = GetRequestStatus(u_id, request_cd);
                if (statusDt.Select(string.Format("STS_CD={0}",SQLUtils.QuotedStr(request_cd))).Length> 0)
                {
                    ml.Add(CreateLog(even, string.Format("已有要求货况{0},所以不发送mail", request_cd), result ? "F" : "R"));
                    return;
                }
                map["PROCESS_URL"] = "Go To Process..";
            }

            StringBuilder sb = new StringBuilder();
            DataTable blDt = GetBLTable(u_id);
            DataRow bl = blDt.Rows[0];

            #region 抓取mail模板
            DataTable mailGroupDt = MailTemplate.GetMailGroup(notify_to, group_id, notify_group);
       
            string body = MailTemplate.GetMailBody(notify_format, group_id, cmp);
            //DataRow bodyInfo = GetMailBody(notify_format, group_id, cmp);//MT_NAME,MT_CONTENT
            //string body = Prolink.Math.GetValueAsString(bodyInfo["MT_CONTENT"]);
            //body = Prolink.Web.Utils.WebUtils.ReFormatHtmlText(body);
            if (string.IsNullOrEmpty(body))
            {
                sb.Append(string.Format("类型为{0}的mail模板不存在;", notify_format));
            }
            #endregion

           
            Func<string, string, string> setCnt = (fieldname, filedescp) =>
            {
                int value = Prolink.Math.GetValueAsInt(bl[fieldname]);
                if (value > 0)
                    return filedescp + value + ";";
                return string.Empty;
            };
            map["CNT_ALL"] = setCnt("CNT20", "20' X ") + setCnt("CNT40", "40' X ") + setCnt("CNT40HQ", "40HQ X ");

            //string tran_type = string.Empty;
            //if (blDt != null && blDt.Rows.Count > 0)
            //    tran_type = Prolink.Math.GetValueAsString(blDt.Rows[0]["TRAN_TYPE"]);
            //货况夹档
            //DataTable docDt = OperationUtils.GetDataTable(string.Format("SELECT M.*,D.DOC_TYPE,D.DOC_DESCP,D.REMARK FROM TKPDM M LEFT JOIN TKPDD D ON M.U_ID=D.U_ID WHERE M.TRAN_MODE={0} AND M.PARTY_NO={1}  AND M.STS_CD={2} AND M.GROUP_ID={3} AND M.CMP={4}", SQLUtils.QuotedStr(tran_type), SQLUtils.QuotedStr(notify_to), SQLUtils.QuotedStr(notify_cd), SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MailTemplate.ChageDateToString(blDt, new string[] { "ETD", "ETA" }, "yyyy/MM/dd HH:mm:ss");
            DataTable baseDt = MailTemplate.GetBaseData("'TNT','TKLC'", Prolink.Math.GetValueAsString(blDt.Rows[0]["GROUP_ID"]), Prolink.Math.GetValueAsString(blDt.Rows[0]["CMP"]));
            blDt.Columns["TRAN_TYPE"].MaxLength = 50;
            blDt.Columns["CSTATUS"].MaxLength = 50;
            foreach (DataRow dr in blDt.Rows)
            {
                dr["TRAN_TYPE"] = MailTemplate.GetBaseCodeValue(baseDt, "TNT", Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]));
                dr["CSTATUS"] = MailTemplate.GetBaseCodeValue(baseDt, "TKLC", Prolink.Math.GetValueAsString(dr["CSTATUS"]));
            }
            
            map["STS_CD"] = notify_cd;
            map["STS_DESCP"] = notify_descp;
            map["REQUEST_CD"] = request_cd;
            map["REQUEST_DESCP"] = request_descp;
            DataRow[] statusDr = statusDt.Select(string.Format("STS_CD={0}", SQLUtils.QuotedStr(notify_cd)));
            if (statusDr.Length > 0)
            {
                map["LOCATION"] = Prolink.Math.GetValueAsString(statusDr[0]["LOCATION"]);
                string even_date = string.Empty;
                if (statusDr[0]["EVEN_DATE"] != null && statusDr[0]["EVEN_DATE"] != DBNull.Value)
                    even_date = ((DateTime)statusDr[0]["EVEN_DATE"]).ToString("yyyy/MM/dd HH:mm:ss");
                map["EVEN_DATE"] = even_date;
                map["STS_REMARK"] = Prolink.Math.GetValueAsString(statusDr[0]["REMARK"]);
            }
            #region 抓取party信息
            //DataTable partyDt = GetPartys(u_id);
            ////PARTY_NO PARTY_NAME
            //SetPatyNameMapping(map, partyDt, "SP", "SHIPPER");
            //SetPatyNameMapping(map, partyDt, "CN", "CONSIGNEE");
            //SetPatyNameMapping(map, partyDt, "NF", "NOTIFY");
            #endregion

            //map["STATUS_HTML"] = MailTemplate.GetStatusHtml(statusDt);

            IMailTemplateParse parse = new DefaultMailParse();
            body = parse.Parse(blDt, statusDt, body, map);

            //_api.Login();
            //_api.DownloadFile(System.IO.Path.Combine(RuntimePath,"XXXX.CSS"), "34", "");
            foreach (DataRow mailGroup in mailGroupDt.Rows)
            {
                string to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                sb.Append(to);
                try
                {
                    result = Mail.Send(title, to, body);
                }
                catch (Exception e)
                {
                    sb.Append(string.Format("【{0}】", e.Message));
                    result = false;
                }
                sb.Append(result ? "发送成功;" : "发送失败;");
            }
            if (mailGroupDt.Rows.Count <= 0)
                sb.Append(string.Format("在mail group setup 无【{0}】建档", notify_to));
            string msg = sb.ToString();
            if (msg.Length > 200)
                msg = msg.Substring(0, 200);
            ml.Add(CreateLog(even, msg, result ? "F" : "R"));
        }

        private static void SetPatyNameMapping(Dictionary<string, string> map, DataTable partyDt,string type,string name)
        {
            DataRow[] partys = partyDt.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(type)));
            if (partys.Length > 0)
            {
                string shipper_name = Prolink.Math.GetValueAsString(partys[0]["PARTY_NAME"]);
                if (string.IsNullOrEmpty(shipper_name))
                    shipper_name = Prolink.Math.GetValueAsString(partys[0]["PARTY_NO"]);
                map[name + "_NAME"] = shipper_name;
                map[name + "_ADDR"] = Prolink.Math.GetValueAsString(partys[0]["PARTY_ADDR"]);
            }
        }

        /// <summary>
        /// 获取要求货况
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="request_cd"></param>
        /// <returns></returns>
        private static DataTable GetRequestStatus(string u_id, string request_cd)
        {
            string sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} AND STS_CD={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(request_cd));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        private static DataTable GetPartys(string u_id)
        {
            string sql = string.Format("SELECT * FROM TKBLPT WHERE U_ID={0} ORDER BY ORDER_BY", SQLUtils.QuotedStr(u_id));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static DataTable GetStatus(string u_id, string seq_no = null)
        {
            string sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(u_id));
            if (!string.IsNullOrEmpty(seq_no))
                sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} AND SEQ_NO={1} ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(seq_no));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        /// <summary>
        /// 获取提单
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="request_cd"></param>
        /// <returns></returns>
        public static DataTable GetBLTable(string u_id)
        {
            string sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        /// <summary>
        /// 获取mail内容标题主体
        /// </summary>
        /// <param name="notify_format"></param>
        /// <param name="group_id"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static DataRow GetMailBody(string notify_format, string group_id, string cmp)
        {
            string sql = string.Format("SELECT MT_NAME,MT_CONTENT FROM TKPMT WHERE GROUP_ID={0} AND CMP={1} AND MT_TYPE={2}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(notify_format));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
                return dt.Rows[0];
            return dt.NewRow();
        }

        /// <summary>
        /// 创建mail log
        /// </summary>
        /// <param name="even"></param>
        /// <param name="notify_mail"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static EditInstruct CreateLog(DataRow even, string notify_mail, string status)
        {
            EditInstruct ei = new EditInstruct("TKEVD", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", System.Guid.NewGuid().ToString("N"));
            ei.Put("NOTIFY_MAIL", notify_mail);
            ei.Put("STATUS", status);
            ei.PutDate("PROCESS_DATE", DateTime.Now);

            ei.Put("GROUP_ID", even["GROUP_ID"]);
            ei.Put("CMP", even["CMP"]);
            ei.Put("STN", even["STN"]);
            ei.Put("DEP", even["DEP"]);
            ei.Put("EVEN_NO", even["EVEN_NO"]);
            return ei;
        }
        #endregion
    }

    public class NotifyStatus
    {
        /// <summary>
        /// 标准的默认状态 S:Stand By(Default) 
        /// </summary>
        public static readonly string StandBy = "S";
        /// <summary>
        /// Ｆ: Finish 
        /// </summary>
        public static readonly string Finish = "F";
    }
}

