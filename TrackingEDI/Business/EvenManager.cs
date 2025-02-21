using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
    public class EvenManager
    {
        #region 注册类型
        /// <summary>
        /// 订舱转tracing，发送iport：代码E_BL
        /// </summary>
        public static string TrackingEven = "E_BL";
        /// <summary>
        ///  货况通知，更新提单货况状态，电子文档通知:代码E_ST
        /// </summary>
        public static string StatusEven = "E_ST";

        /// <summary>
        /// 货况通知和更新提单货况状态:代码E_ST1
        /// </summary>
        public static string StatusEven1 = "E_ST1";

        /// <summary>
        /// 设置订舱的ATA ATD
        /// </summary>
        public static string StatusDateEven = "E_ST2";
        #endregion

        public static string TrackingNotify(MailData data, MixedList ml)
        {
            try
            {
                string evenType = data.Keys[0];
                string u_id = data.Keys[1];
                BookingParser bp = new BookingParser();
                DataRow dr = EvenNotify.GetBl(u_id);
                EvenNotify.Notify(u_id, ml, dr);
                DataTable dt= OperationUtils.GetDataTable(string.Format("SELECT CORDER,ISCOMBINE_BL FROM SMSM WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id)),
                   null, Prolink.Web.WebContext.GetInstance().GetConnection());
                bool isSendIport=false;
                if (dt.Rows.Count > 0)
                {
                    if ("C".Equals(Prolink.Math.GetValueAsString(dt.Rows[0]["CORDER"])))
                        isSendIport = true;
                    else
                    {
                        if ("C".Equals(Prolink.Math.GetValueAsString(dt.Rows[0]["ISCOMBINE_BL"])))
                            isSendIport = true;
                    }
                }
                else
                {
                    bp.SendIport(u_id, dr);
                }
                if (isSendIport)
                    bp.SendIport(u_id, dr);
            }
            catch { }
            return string.Empty;
        }

        public static string StatusNotify(MailData data, MixedList ml)
        {
            try
            {
                //Prolink.DataOperation.OperationUtils.Logger.WriteLog("EvenNotify/Notify Start");

                string evenType = data.Keys[0];
                string u_id = data.Keys[1];
                string seq_no = data.Keys[2];
                string code = data.Keys[3];
                DataRow dr = EvenNotify.GetBl(u_id);
                EvenNotify.Notify(u_id, ml, dr);
                XmlParser.UpdateProccessStatus(u_id);
                DocSender.Send(u_id, seq_no, code, dr);
            }
            catch(Exception ex) {
               // Prolink.DataOperation.OperationUtils.Logger.WriteLog("EvenNotify/Notify Error:" + ex.ToString());

            }
            return string.Empty;
        }

        public static string StatusNotify1(MailData data, MixedList ml)
        {
            try
            {
                string evenType = data.Keys[0];
                string u_id = data.Keys[1];
                BookingParser bp = new BookingParser();
                DataRow dr = EvenNotify.GetBl(u_id);
                EvenNotify.Notify(u_id, ml, dr);
                XmlParser.UpdateProccessStatus(u_id);
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// 设置 ATA ATD 时间
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SetShipmentStatusTime(MailData data, MixedList ml)
        {
            try
            {
                string evenType = data.Keys[0];
                string mbl_no = data.Keys[1];
                if (string.IsNullOrEmpty(mbl_no))
                    return string.Empty;
                Manager.SetShipmentStatusTime(mbl_no);
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// 状态变化事件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string ChangeStatus(MailData data, MixedList ml)
        {
            try
            {
                string evenType = data.Keys[0];
                string u_id = data.Keys[1];
                DataRow dr = EvenNotify.GetBl(u_id);
                EvenNotify.Notify(u_id, ml, dr);
                XmlParser.UpdateProccessStatus(u_id);

                string mbl_no = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                if (string.IsNullOrEmpty(mbl_no))
                    mbl_no = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);
                if (string.IsNullOrEmpty(mbl_no))
                    return string.Empty;
                Manager.SetShipmentStatusTime(mbl_no);
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// 发送电子文档
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendDoc(MailData data, MixedList ml)
        {
            try
            {
                string evenType = data.Keys[0];
                string u_id = data.Keys[1];
                string seq_no = data.Keys[2];
                string code = data.Keys[3];
                DataRow dr = EvenNotify.GetBl(u_id);
                DocSender.Send(u_id, seq_no, code, dr);
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// 添加状态变更事件
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="statuss"></param>
        public static void AddStatusChangeEven(string uid, Dictionary<string, string> statuss)
        {
            EvenFactory.AddOnceEven(uid, uid, EvenType.E_SC.ToString());
            foreach (var st in statuss)
            {
                EvenFactory.AddOnceEven(string.Format("{0}#{1}#{2}", uid, st.Key, st.Value), uid, EvenType.E_SD.ToString());
            }
        }
    }


    /// <summary>
    /// 事件类型
    /// </summary>
    public enum EvenType
    {
        /// <summary>
        /// 货况状态变更事件
        /// </summary>
        E_SC,

        /// <summary>
        /// 订舱转tracing，发送iport：代码E_BL
        /// </summary>
        E_BL,
        /// <summary>
        ///  货况通知，更新提单货况状态，电子文档通知:代码E_ST
        /// </summary>
        E_ST,

        /// <summary>
        /// 货况通知和更新提单货况状态:代码E_ST1
        /// </summary>
        E_ST1,

        /// <summary>
        /// 设置订舱的ATA ATD
        /// </summary>
        E_ST2,

        /// <summary>
        /// 发送电子文档事件
        /// </summary>
        E_SD
    }

}
