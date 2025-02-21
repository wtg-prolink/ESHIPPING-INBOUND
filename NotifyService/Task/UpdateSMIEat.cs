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
    public class UpdateSMIEat : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            string sql1 = string.Format("DELETE FROM SMETA WHERE CREATE_DATE<{0}", DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"));
            OperationUtils.ExecuteUpdate(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql = "SELECT TOP 500 * FROM SMETA WHERE RESULT_STATUS ='N' ORDER BY CREATE_DATE DESC,SHIPMENT_ID";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<string> shmlist = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (shmlist.Contains(shipmentid))
                {
                    continue;
                }
                shmlist.Add(shipmentid);
                try
                {
                    string msg = UpdateSmsmiEta.SetEta(new List<string> { shipmentid }, cmp);
                    if (string.IsNullOrEmpty(msg))
                    {
                        UpdateDB(shipmentid, "");
                    }
                    else
                    {
                        UpdateDB(shipmentid, msg);
                    }
                }
                catch (Exception ex)
                {
                    UpdateDB(shipmentid, ex.ToString(), false);
                    return;
                }
            }
        }

        public void UpdateDB(string uid, string message, bool success = true)
        {
            EditInstruct ei = new EditInstruct("SMETA", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", uid);
            ei.PutDate("MODIFY_DATE", DateTime.Now);
            if (success)
            {
                ei.Put("RESULT_STATUS", "S");
            }
            else
            {
                ei.Put("RESULT_STATUS", "F");
                ei.Put("FAIL_MSG", message.Length > 99 ? message.Substring(0, 99) : message);
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
