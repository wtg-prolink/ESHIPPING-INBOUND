using Business.TPV.Export;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using TrackingEDI.InboundBusiness;

namespace NotifyService.Task
{
    public class SendASNDateBYMMDTask : IPlanTask
    { 
        private string _hour = null;
        private bool flag = true;

        public SendASNDateBYMMDTask(string hour)
        {
            _hour = hour;
        }
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            if (IsExecute(_hour, ref flag) && !flag)
            {
                SendAsnDate();
            }

        }

        public bool IsExecute(string parmhour, ref bool flag)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }
            string[] hours = parmhour.Split(';');

            int hour = DateTime.Now.Hour;

            bool matchhour = false;
            foreach (string index in hours)
            {
                if (string.IsNullOrEmpty(index)) continue;
                if (Prolink.Math.GetValueAsInt(index) == hour)
                    matchhour = true;
            }

            if (matchhour && flag)
            {
                flag = false;
                return true;
            }
            else if (!matchhour && !flag)
            {
                flag = true;
                return false;
            }
            else
                return false;
        }
        private void SendAsnDate()
        {
            string sql = @"SELECT U_ID,SHIPMENT_ID,INV_NO,DN_NO,
            (SELECT TOP 1 ASN_NO FROM SMIDNP WHERE SMIDNP.INV_NO = SMIDN.INV_NO AND ASN_NO IS NOT NULL AND SMIDNP.ASN_NO != '') ASN_NO,
            (SELECT TOP 1 DELIVERY_DATE FROM SMORD WHERE SHIPMENT_ID=SMIDN.SHIPMENT_ID) DELIVERY_DATE,
            (SELECT TOP 1 ETA FROM SMSMI WHERE SHIPMENT_ID=SMIDN.SHIPMENT_ID) ETA,CMP,
            SMIDN.ASN_DATE AS OLD_ASN_DATE,
            (SELECT TOP 1 CNTR_NO FROM SMIDNP WHERE SMIDNP.INV_NO = SMIDN.INV_NO AND ASN_NO IS NOT NULL AND SMIDNP.ASN_NO != '') CNTR_NO,
            (SELECT TOP 1 BU FROM SMSMI WHERE SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS BU,
            (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS TRAN_TYPE FROM SMIDN
            WHERE  SEND_ASN_STATUS = 'N' AND CMP='MN' AND EXISTS(SELECT 1 FROM SMIDNP WHERE SMIDNP.INV_NO = SMIDN.INV_NO AND
            SMIDNP.ASN_NO IS NOT NULL AND SMIDNP.ASN_NO != '' ) ORDER BY CMP";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string firstcmp = string.Empty;
            int i = 0;
            if(dt.Rows.Count > 0)
            {
                ASNDateManager aSNDateManager = new ASNDateManager();
                aSNDateManager.DsiposeDestination();
            }
            dt.Columns.Add("ASN_DATE", typeof(DateTime));
            List<string> shipmentList = new List<string>();

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
                    DateTime deleveryDate = Prolink.Math.GetValueAsDateTime(dr["DELIVERY_DATE"]);
                    DateTime eta = Prolink.Math.GetValueAsDateTime(dr["ETA"]);
                    string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);

                    DateTime newAsnDate = deleveryDate;

                    if (deleveryDate.Equals(DateTime.MinValue))
                    {
                        switch (tranType)
                        {
                            case "F":
                            case "L":
                                newAsnDate = eta.AddDays(6);
                                break;
                            case "R":
                                newAsnDate = eta.AddDays(3);
                                break;
                            case "A":
                            case "E":
                                newAsnDate = eta.AddDays(1);
                                break;
                            default:
                                newAsnDate = eta.AddDays(6);
                                break;
                        }
                    }

                    dr["ASN_DATE"] = newAsnDate;

                    if (cmp == firstcmp)
                    {
                        samecmp = true;
                    }
                    else
                    {
                        firstcmp = cmp;
                    }
                    MixedList ml = new MixedList();
                    EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("DN_NO", dnno);
                    Business.Service.ResultInfo result = Business.TPV.Helper.SendASNDateInfo(dr, samecmp, true);
                    if (result != null && result.IsSucceed == true)
                    {
                        if (!string.IsNullOrEmpty(shipmentid) && !shipmentList.Contains(shipmentid))
                            shipmentList.Add(shipmentid);
                        ei.Put("SEND_ASN_STATUS", "Y");

                        //EditInstruct smsmei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        //smsmei.PutKey("SHIPMENT_ID", dr["SHIPMENT_ID"]);
                        //smsmei.PutDate("ASN_DATE", newAsnDate);
                        //ml.Add(smsmei);

                        EditInstruct smidnEi = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                        smidnEi.PutKey("SHIPMENT_ID", shipmentid);
                        smidnEi.PutKey("INV_NO", dr["INV_NO"]);
                        smidnEi.PutDate("ASN_DATE", newAsnDate);
                        ml.Add(smidnEi);

                        EditInstruct smidnpEi = new EditInstruct("SMIDNP", EditInstruct.UPDATE_OPERATION);
                        smidnpEi.PutKey("SHIPMENT_ID", shipmentid);
                        smidnpEi.PutKey("INV_NO", dr["INV_NO"]);
                        smidnpEi.PutDate("ASN_DATE", newAsnDate);
                        ml.Add(smidnpEi);
                    }
                    else
                    { 
                        ei.Put("SEND_ASN_STATUS", "T");
                    }
                    ml.Add(ei);


                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                }
                catch (Exception ex) {
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog("SendASNDateBYMMDTask Error:" + ex.Message);
                }
            }
            foreach (string shipmentId in shipmentList)
            {
                TrackingEDI.InboundBusiness.ASNManager.SetAsnByShipmentid(shipmentId);
            }
        }
    }
}
