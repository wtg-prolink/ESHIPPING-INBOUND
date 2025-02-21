using Business.TPV.Export;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Data;
using TrackingEDI.InboundBusiness;

namespace NotifyService.Task
{
    public class SenASNDateToSAPTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            //#region 計算due Date
            SendAsnDate();
        }

        private void SendAsnDate()
        {
            string sql = @"SELECT SHIPMENT_ID,INV_NO,DN_NO,
            (SELECT TOP 1 ASN_NO FROM SMIDNP WHERE SMIDNP.INV_NO = SMIDN.INV_NO AND ASN_NO IS NOT NULL AND SMIDNP.ASN_NO != '') ASN_NO,
            ASN_DATE,CMP FROM SMIDN
            WHERE  SEND_ASN_STATUS = 'N' AND CMP='AVAIB' AND EXISTS(SELECT 1 FROM SMIDNP WHERE SMIDNP.INV_NO = SMIDN.INV_NO AND
            SMIDNP.ASN_NO IS NOT NULL AND SMIDNP.ASN_NO != '' ) ORDER BY CMP";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string firstcmp = string.Empty;
            int i = 0;
            if(dt.Rows.Count > 0)
            {
                ASNDateManager aSNDateManager = new ASNDateManager();
                aSNDateManager.DsiposeDestination();
            }
            foreach (DataRow dr in dt.Rows)
            {
                bool samecmp = false;
                if(i==0)
                    firstcmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                i++;
                try
                {
                    string dnno = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                    string cmp= Prolink.Math.GetValueAsString(dr["CMP"]); 
                    if (cmp == firstcmp)
                    {
                        samecmp = true;
                    }
                    else
                    {
                        firstcmp = cmp;
                    }
                    EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("DN_NO", dnno);
                    Business.Service.ResultInfo result = Business.TPV.Helper.SendASNDateInfo(dr, samecmp);
                    if (result != null && result.IsSucceed == true)
                    {
                        ei.Put("SEND_ASN_STATUS", "Y");
                    }
                    else
                    {
                       
                        ei.Put("SEND_ASN_STATUS", "F");
                    }
                    OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex) { }
            }

        }
    }
}
