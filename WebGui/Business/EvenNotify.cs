using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prolink.Data;
using System.Data;
using Prolink.DataOperation;

namespace Business
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
            EditInstruct ei = new EditInstruct("TKEVM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("EVEN_NO", evenNo);//CMP+E+YYMMDD+9999999
            ei.PutDate("NOTIFY_DATE", null);
            ei.Put("PROCESS_TIMES", 0);
            ei.Put("STATUS", NotifyStatus.StandBy);
            int []result=OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            return result[result.Length-1];
        }

        /// <summary>
        /// 提单修改时新增通知
        /// </summary>
        /// <param name="uid">提单主键</param>
        public static void Notify(string uid)
        {
            MixedList ml = new MixedList();
            EditInstruct ei = null;

            DataRow tkbl = GetBl(uid);
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
            string sql = string.Format("SELECT * FROM TKBLPT WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));

            DataTable partyDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow[] notices = null;
            foreach (DataRow dr in partyDt.Rows)
            {
                string party_type = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                notices = evenDt.Select("PARTY_TYPE=" + SQLUtils.QuotedStr(party_type));
                if (notices == null || notices.Length <= 0)//判断有沒有設定要做通知
                    continue;
                foreach (DataRow notice in notices)
                {
                    ei = AddNotify(tkbl, notice,dr, NotifyStatus.StandBy);
                    if (ei == null) continue;
                    ml.Add(ei);
                }
            }
            int[] result = null;
            if (ml.Count > 0)
                result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// 添加通知记录
        /// </summary>
        /// <param name="tkbl"></param>
        /// <param name="notice"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static EditInstruct AddNotify(DataRow tkbl, DataRow notice,DataRow partyDr, string status)
        {
            string shipment_id = Prolink.Math.GetValueAsString(tkbl["SHIPMENT_ID"]);
            string bl_no = Prolink.Math.GetValueAsString(tkbl["U_ID"]);
            //string bl_no = Prolink.Math.GetValueAsString(tkbl["BL_NO"]);
            string cmp = Prolink.Math.GetValueAsString(tkbl["CMP"]);
            string group_id = Prolink.Math.GetValueAsString(tkbl["GROUP_ID"]);
            string dep = Prolink.Math.GetValueAsString(tkbl["DEP"]);
            string stn = Prolink.Math.GetValueAsString(tkbl["STN"]);
            string notify_cd = Prolink.Math.GetValueAsString(notice["NOTIFY_CD"]);
            string request_cd = Prolink.Math.GetValueAsString(notice["REQUEST_CD"]);

            string notify_to = Prolink.Math.GetValueAsString(partyDr["PARTY_NO"]);
            string notify_name = Prolink.Math.GetValueAsString(partyDr["PARTY_NAME"]);
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
            Random rd=new Random();
            string even_no = cmp + "E" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + rd.Next(9);
            //string even_no = System.Guid.NewGuid().ToString("N");//CMP+E+YYMMDD+9999999
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
            //ei.Put("BL_UID", );
            ei.Put("SHIPMENT_ID", shipment_id);
            //ei.Put("TRAN_MODE", tran_mode);
            ei.Put("BL_NO", bl_no);
            ei.Put("NOTIFY_TO", notify_to);//通知人
            ei.Put("NOTIFY_NM", notify_name);

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
            DataTable evenDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return evenDt;
        }

        /// <summary>
        /// 获取提单资料
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private static DataRow GetBl(string uid)
        {
            //获取提单资料
            DataTable tkblDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataRow tkbl = null;
            if (tkblDt != null && tkblDt.Rows.Count > 0)
                tkbl = tkblDt.Rows[0];
            return tkbl;
        }
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

