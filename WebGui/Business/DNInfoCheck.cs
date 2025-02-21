using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Business
{
    public class DNInfoCheck
    {
        #region DN 状态类型
        /// <summary>
        /// D -Default
        /// </summary>
        public static string STA_D = "D";   //Default
        /// <summary>
        /// B -订舱发起
        /// </summary>
        public static string STA_B = "B";   //订舱发起
        /// <summary>
        /// V -取消
        /// </summary>
        public static string STA_V = "V";   //取消
        /// <summary>
        /// H -暂缓出货
        /// </summary>
        public static string STA_H = "H";   //暂缓出货
        #endregion

        public static EditInstruct GetApproveRdVoidEI(string refno, string approvecode)
        {
              EditInstruct ei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
              ei.PutKey("REF_NO", refno);
              ei.PutKey("APPROVE_CODE", approvecode);
              ei.Put("APPROVE_CODE", "VOID");
              ei.Put("STATUS", "0");
              return ei;
        }

        public static string CheckDNApproveStatus(string status)
        {
            string revalue = string.Empty;
            switch (status)
            {
                case "V":
                    revalue = @Resources.Locale.L_DNManage_Controllers_233;
                    break;
                case "H":
                    revalue = @Resources.Locale.L_DNInfoCheck_Business_61;
                    break;
            }
            return revalue;
        }

        public static string CheckSMStatus(string dnno,string shipmentid=null)
        {
            string msg = string.Empty;
            string sql=string.Format("SELECT STATUS FROM SMSM WHERE SHIPMENT_ID=(SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO={0})",SQLUtils.QuotedStr(dnno));
            if(!string.IsNullOrEmpty(shipmentid)){
                sql = string.Format("SELECT STATUS FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            string status = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            switch (status)
            {
                case "A":
                    msg = @Resources.Locale.L_DNManage_HasBkCtEt;
                    break;
                case "B":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_62;
                    break;
                case "C":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_63;
                    break;
                case "D":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_64;
                    break;
                case "I":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_65;
                    break;
                case "P":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_66;
                    break;
                case "F":
                case "R":
                case "O":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_67;
                    break;
                case "G":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_68;
                    break;
                case "H":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_69;
                    break;
                case "Z":
                    msg = @Resources.Locale.L_DNInfoCheck_Business_70;
                    break;
            }
            return msg;
        }

        public static DataTable GetDNDataByDnNo(string dnno)
        {
            string sql = string.Format("SELECT * FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static DataTable GetDNDataByUId(string uid)
        {
            string sql = string.Format("SELECT * FROM SMDN WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        
    }
}