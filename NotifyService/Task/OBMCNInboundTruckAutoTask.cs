using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Data;
using TrackingEDI.InboundBusiness;


namespace NotifyService.Task
{
    public class OBMCNInboundTruckAutoTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            //#region 計算due Date
            SetTruckInfo();
        }

        private void SetTruckInfo()
        {
            string shipmentid = string.Empty;
            string userid= string.Empty;
            string companyid = string.Empty;
            string sql = "SELECT SHIPMENT_ID,CREATE_BY,CMP FROM SMSMI WHERE STATUS='A' AND TRAN_TYPE='T' AND CMP='CNI' AND LSP_CONFIRM_DATE IS NULL ";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                userid = Prolink.Math.GetValueAsString(dr["CREATE_BY"]);
                companyid = Prolink.Math.GetValueAsString(dr["CMP"]);

                MixedList mllist = new MixedList();

                mllist.Add(string.Format("DELETE FROM SMORD WHERE SHiPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid)));
                mllist.Add(string.Format("DELETE FROM SMRDN WHERE SHiPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid)));
                mllist.Add(string.Format("DELETE FROM SMRCNTR WHERE SHiPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid)));
                mllist.Add(string.Format("DELETE FROM SMIRV WHERE shipment_info ={0}", SQLUtils.QuotedStr(shipmentid)));
                try
                {
                    OperationUtils.ExecuteUpdate(mllist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch(Exception ex)
                {
                    continue;
                }

                TrackingEDI.InboundBusiness.InboundHelper.SetTruckToAutoInfo(shipmentid, userid, companyid);
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("STATUS", "A");
                try
                {
                    OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
