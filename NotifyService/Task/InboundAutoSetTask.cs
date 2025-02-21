using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Data;
using TrackingEDI.InboundBusiness;

namespace NotifyService.Task
{
    public class InboundAutoSetTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            //#region 計算due Date
            CalDueDate();

            SetSmStatus();
        }

        public void SetSmStatus()
        {
            DelegateConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection();
            string sql = "";

            DateTime Today = DateTime.Now;
            string StrToday_F = Today.ToString("yyyy-MM-dd 00:00:00");
            string StrToday_T = Today.ToString("yyyy-MM-dd 23:59:59");

            #region 掃進口DN檔是否可放行的shipment
            sql = "SELECT SHIPMENT_ID, REL_DATE FROM SMIDN WHERE REL_DATE >= {0} AND REL_DATE <= {1} GROUP BY SHIPMENT_ID, REL_DATE";
            sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T));
            DataTable dt = OperationUtils.GetDataTable(sql, null, conn);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    int n = OperationUtils.GetValueAsInt(sql, conn);

                    if (n > 0)
                    {
                        sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                        int m = OperationUtils.GetValueAsInt(sql, conn);

                        if (n == m)
                        {
                            sql = "UPDATE SMSMI SET STATUS='F' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            catch (Exception ex)
                            {
                                Prolink.DataOperation.OperationUtils.Logger.WriteLog("掃進口DN檔是否可放行的shipment:" + ex.ToString());
                            }
                        }
                    }

                }
            }
            #endregion

            #region 掃進口貨櫃檔是否可放行的shipment
            sql = "SELECT SHIPMENT_ID, REL_DATE FROM SMICNTR WHERE REL_DATE >= {0} AND REL_DATE <= {1} GROUP BY SHIPMENT_ID, REL_DATE";
            sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T));
            dt = OperationUtils.GetDataTable(sql, null, conn);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    int n = OperationUtils.GetValueAsInt(sql, conn);

                    if (n > 0)
                    {
                        sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                        int m = OperationUtils.GetValueAsInt(sql, conn);

                        if (n == m)
                        {
                            sql = "UPDATE SMSMI SET STATUS='F' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            catch (Exception ex)
                            {
                                Prolink.DataOperation.OperationUtils.Logger.WriteLog("掃進口貨櫃檔是否可放行的shipment:" + ex.ToString());
                            }
                        }
                    }

                }
            }
            #endregion
        }

        public void CalDueDate(){
            string ShipmentId = string.Empty;
            string sql = "SELECT SHIPMENT_ID FROM SMSMI WHERE STATUS NOT IN ('P','O')";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                TrackingEDI.InboundBusiness.SMSMIHelper.CalDueDate(ShipmentId);
            }
        }

        public void UpdateDB(string uid, string message, bool success = true)
        {
            EditInstruct ei = new EditInstruct("ASD_TASK", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            if (success)
            {
                ei.Put("RESULT_STATUS", "S");
                //ei.Put("FILE_NAMES", message);
                ei.Put("FILEPATH", message);
            }
            else
            {
                ei.Put("RESULT_STATUS", "F");
                ei.Put("REMARK", message);
            }
            MixedList ml = new MixedList();

            ml.Add(ei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
            }
        }

    }
}
