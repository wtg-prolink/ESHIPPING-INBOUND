using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NotifyService.Task
{
    public class AutoDeleteEalertTask : IPlanTask
    {

        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            AutoDelete();
        }
        /// <summary>
        /// OutBound 执行取消 退运动作时，需将inbound的相关资料删除
        /// </summary>
        public void AutoDelete()
        {
            string sql = string.Format("SELECT TOP 100 SHIPMENT_ID FROM SMSM WHERE (STATUS='Z' OR STATUS='V') AND EXISTS(SELECT 1 FROM SMSMI WHERE STATUS='E' AND SHIPMENT_ID=SMSM.SHIPMENT_ID)");
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList ml = new MixedList();
            foreach (DataRow dr in dt.Rows)
            {
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                ml.Add(string.Format("DELETE FROM SMSMI WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentId)));
                ml.Add(string.Format("DELETE FROM SMSMIPT WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentId)));
                ml.Add(string.Format("DELETE FROM SMIDN WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentId)));
                ml.Add(string.Format("DELETE FROM SMIDNP WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentId)));
                ml.Add(string.Format("DELETE FROM SMORD WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentId)));
                ml.Add(string.Format("DELETE FROM SMRDN WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentId)));
                ml.Add(string.Format("DELETE FROM SMRCNTR WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentId)));
                ml.Add(string.Format("DELETE FROM SMIRV WHERE SHIPMENT_INFO = {0}", SQLUtils.QuotedStr(shipmentId)));
            }
            if (ml.Count > 0)
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }
    }
}
