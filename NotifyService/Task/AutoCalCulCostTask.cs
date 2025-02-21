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
    class AutoCalCulCostTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            //. 资料来源于当 SMSM 订舱确认后的ETD更改就要重新计价。但是前提是先要将ETD更新到账单那边的账单日期ETD
            //账单日期ETD是在未生成账单签，即没有账单号码的前提下
            string sql = "SELECT TOP 1000 * FROM AUTO_VALUATION_TASK WHERE DONE !='Y' OR DONE IS NULL";

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow dr in dt.Rows)
            {
                string uid = Prolink.Math.GetValueAsString(dr["SMU_ID"]);
                if (string.IsNullOrEmpty(uid))
                    continue;
                try
                {
                    Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                    bill.Create(uid, DateTime.Now);
                    string autosql = string.Format("UPDATE AUTO_VALUATION_TASK SET DONE='Y', DONE_DATE =GETDATE() WHERE SMU_ID='{0}'", uid);
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
                    string autosql = string.Format("UPDATE AUTO_VALUATION_TASK SET DONE_FREQUENCY={1},FAIL_MSG='{2}',DONE='{3}', DONE_DATE =GETDATE() WHERE SMU_ID='{0}'", uid, count, FAIL_MSG, DONE);
                    OperationUtils.ExecuteUpdate(autosql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    _messenger.GetLogger().WriteLog(e);
                    string subject = "订舱ID:" + uid + " 自动计价异常 ";
                    string body = subject + e.Message.ToString();
                    SendMail(uid, subject, body);
                }
            }
            AutoCallBoundCost();
        }

        public void AutoCallBoundCost()
        {
            string sql = string.Format("SELECT TOP 1000 SMU_ID,BOUND_TYPE,DONE_COUNT,FAIL_MSG,DONE FROM AUTO_BOUNDCOST_TASK WITH (NOLOCK) WHERE (DONE<>'Y' OR DONE IS NULL) AND (DONE_DATE IS NULL OR DONE_DATE<{0})", SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string uid = Prolink.Math.GetValueAsString(dr["SMU_ID"]);
                if (string.IsNullOrEmpty(uid))
                    continue;
                string bound_type = Prolink.Math.GetValueAsString(dr["BOUND_TYPE"]);
                if (string.IsNullOrEmpty(bound_type))
                    bound_type = "O";
                try
                {
                    Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                    bill.Create(uid, DateTime.Now, true, null, null, bound_type);
                    string autosql = string.Format("UPDATE AUTO_BOUNDCOST_TASK SET DONE='Y',DONE_DATE=GETDATE() WHERE SMU_ID={0}", SQLUtils.QuotedStr(uid));
                    OperationUtils.ExecuteUpdate(autosql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception e)
                {
                    try
                    {
                        if (_messenger != null && _messenger.GetLogger() != null)
                            _messenger.GetLogger().WriteLog(e);

                        int count = Prolink.Math.GetValueAsInt(dr["DONE_COUNT"]) + 1;
                        string FAIL_MSG = Prolink.Math.GetValueAsString(dr["FAIL_MSG"]);
                        string DONE = Prolink.Math.GetValueAsString(dr["DONE"]);
                        if (count > 3)
                        {
                            FAIL_MSG = "超过3次，自动停止重新计价";
                            DONE = "Y";
                        }
                        string exception_msg = e.Message;
                        if (!string.IsNullOrEmpty(exception_msg) && exception_msg.Length > 100)
                            exception_msg = exception_msg.Substring(0, 100);
                        string autosql = string.Format("UPDATE AUTO_BOUNDCOST_TASK SET DONE_COUNT=DONE_COUNT+1,FAIL_MSG={1},DONE={2},EXCEPTION_MSG={3},DONE_DATE =GETDATE() WHERE SMU_ID={0}",SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(FAIL_MSG), SQLUtils.QuotedStr(DONE), SQLUtils.QuotedStr(exception_msg));
                        OperationUtils.ExecuteUpdate(autosql, Prolink.Web.WebContext.GetInstance().GetConnection());
                     
                        string subject = "(" + bound_type + ")订舱ID:" + uid + " 自动计价异常 ";
                        string body = subject + e.Message;
                        SendMail(uid, subject, body);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void SendMail(string dnno, string subject, string body)
        {
            string mailto = "dean@pllink.com";
            EvenFactory.AddEven(Guid.NewGuid().ToString(), dnno, TrackingEDI.Business.MailManager.DNNotify, null, 1, 0, mailto, subject, body);
        }
    }
}
