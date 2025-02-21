using Business;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;

namespace NotifyService.Task
{
    public class InboundCalculcostTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public InboundCalculcostTask() {
        }
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            //. 资料来源于当 SMSM 订舱确认后的ETD更改就要重新计价。但是前提是先要将ETD更新到账单那边的账单日期ETD
            //账单日期ETD是在未生成账单签，即没有账单号码的前提下
            string sql = "SELECT TOP 100 * FROM AUTO_IBCALCUL_TASK WHERE DONE !='Y' OR DONE IS NULL";

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                Business.TPV.Financial.CalculateFee cf = new Business.TPV.Financial.CalculateFee(shipmentId);
                List<string> emptyMessage = new List<string>();

                string reserveNo = Prolink.Math.GetValueAsString(dr["RESERVE_NO"]);
                try
                {
                    Business.TPV.Financial.Bill.WriteLogTagStart("排程计算费用", shipmentId);
                    string autosql = string.Format("UPDATE AUTO_IBCALCUL_TASK SET DONE='Y', DONE_DATE =GETDATE() WHERE SHIPMENT_ID={0} AND (RESERVE_NO IS NULL OR RESERVE_NO='')"
                        , SQLUtils.QuotedStr(shipmentId));
                    if (!string.IsNullOrEmpty(reserveNo))
                    {
                        cf.FindTrailerQuote(reserveNo, shipmentId, emptyMessage);
                        autosql = string.Format("UPDATE AUTO_IBCALCUL_TASK SET DONE='Y', DONE_DATE =GETDATE() WHERE SHIPMENT_ID={0} AND RESERVE_NO={1}"
                        , SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(reserveNo));
                    }
                    cf.CalLocalFee(shipmentId, emptyMessage);
                    cf.CalFreightCalculat(shipmentId);
                    InboundTransfer.UpdateBillInfoToSMORD(shipmentId, "", null);
                    Business.TPV.Financial.Bill.WriteLogTagStart("结束计算", shipmentId);
                    DataTable smsmiDt = GetSmsmiTable(shipmentId);
                    if (smsmiDt.Rows.Count > 0)
                    {
                        string groupid = Prolink.Math.GetValueAsString(smsmiDt.Rows[0]["GROUP_ID"]);
                        string cmp = Prolink.Math.GetValueAsString(smsmiDt.Rows[0]["CMP"]);
                        TrackingEDI.Business.CostStatistics.SetCStask(shipmentId, groupid, cmp, "SYS");
                    }
                    OperationUtils.ExecuteUpdate(autosql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception e)
                {
                    int count = 0;
                    if (dr["DONE_FREQUENCY"] == System.DBNull.Value)
                    {
                        count = 1;
                    }
                    else
                    {
                        count = Prolink.Math.GetValueAsInt(dr["DONE_FREQUENCY"]) + 1;
                    }
                    string FAIL_MSG = Prolink.Math.GetValueAsString(dr["FAIL_MSG"]);
                    string DONE = Prolink.Math.GetValueAsString(dr["DONE"]);
                    if (count > 3)
                    {
                        FAIL_MSG = "超过3次，自动停止重新计价";
                        DONE = "Y";
                    }
                    string autosql = string.Format("UPDATE AUTO_IBCALCUL_TASK SET DONE_FREQUENCY={1},FAIL_MSG={2},DONE={3}, DONE_DATE =GETDATE() WHERE SHIPMENT_ID={0} AND (RESERVE_NO IS NULL OR RESERVE_NO='')"
                        , SQLUtils.QuotedStr(shipmentId), count, SQLUtils.QuotedStr(FAIL_MSG), SQLUtils.QuotedStr(DONE));
                    if (!string.IsNullOrEmpty(reserveNo))
                    {
                        autosql = string.Format("UPDATE AUTO_IBCALCUL_TASK SET DONE_FREQUENCY={2},FAIL_MSG={3},DONE={4}, DONE_DATE =GETDATE() WHERE SHIPMENT_ID={0} AND RESERVE_NO={1}"
                        , SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(reserveNo), count, SQLUtils.QuotedStr(FAIL_MSG), SQLUtils.QuotedStr(DONE));
                    }
                    OperationUtils.ExecuteUpdate(autosql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    _messenger.GetLogger().WriteLog(e);
                    string subject = "订舱ID:" + shipmentId + " 自动计价异常 ";
                    string body = subject + e.Message.ToString();
                    SendMail(shipmentId, subject, body);
                }
            }
        }

        public void SendMail(string dnno, string subject, string body)
        {
            string mailto = "Will.Wan@wisetechglobal.com";
            EvenFactory.AddEven(Guid.NewGuid().ToString(), dnno, TrackingEDI.Business.MailManager.DNNotify, null, 1, 0, mailto, subject, body);
        }

        public DataTable GetSmsmiTable(string shipmentId)
        {
            string sql = string.Format(@"SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

    }
}
