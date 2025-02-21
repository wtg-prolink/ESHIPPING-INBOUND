using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;

namespace NotifyService.Task
{
	public class AutoCsTask : IPlanTask
    {
        public AutoCsTask()
        { 
        }
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            DoCSTask();
        }

        public void DoCSTask()
        {
            try
            {
                string sql1 = "SELECT DISTINCT TOP 100 SHIPMENT_ID FROM(SELECT DISTINCT TOP 200  SHIPMENT_ID,CREATE_DATE FROM CS_ITASK WHERE(DONE IS NULL OR DONE<>'Y') AND (FAIL_COUNT IS NULL OR FAIL_COUNT<3) ORDER BY CREATE_DATE DESC)Z";
                System.Data.DataTable dt1 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                _messenger.GetLogger().WriteLog("开始执行CS TASK,时间：" + DateTime.Now);
                foreach (DataRow dr in dt1.Rows)
                {
                    bool isUpdate = true;
                    string errorMsg = "";
                    string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    try
                    { 
                        if (!string.IsNullOrEmpty(shipmentid))
                           isUpdate= CostStatistics.StatisticsProduce(shipmentid, "SYS", errorMsg);
                        EditInstruct ei = new EditInstruct("CS_ITASK", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", shipmentid);
                        ei.Put("DONE", "Y");
                        ei.PutDate("MODIFY_DATE", DateTime.Now);
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            errorMsg = errorMsg.Length > 1000 ? errorMsg.Substring(0, 1000) : errorMsg;
                            ei.Put("REMARK", errorMsg);
                        }
                        if (!isUpdate)
                            ei.PutExpress("FAIL_COUNT", "CASE WHEN DONE='Y' THEN FAIL_COUNT WHEN FAIL_COUNT IS NULL THEN 1 WHEN FAIL_COUNT >=3 THEN 3 ELSE FAIL_COUNT+1 END");
                        OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        _messenger.GetLogger().WriteLog(shipmentid + ":" + ex);
                    }
                  
                }
                _messenger.GetLogger().WriteLog("结束执行CS TASK,笔数" + dt1.Rows.Count + ",时间：" + DateTime.Now);
                string sql = string.Format("DELETE FROM CS_ITASK WHERE CREATE_DATE<{0}", SQLUtils.QuotedStr(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd")));
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                _messenger.GetLogger().WriteLog(ex);
            }
        }
    }
}
