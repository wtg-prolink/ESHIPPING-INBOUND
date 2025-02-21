using Business.TPV.Base;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Business.TPV.Import
{
    public class ATCCargoReceiveManager : ManagerBase
    {
        public string Import(ATCCargoReceiveInfo info, string json)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            try
            {
                info = js.Deserialize<ATCCargoReceiveInfo>(json);
            }
            catch (Exception ex)
            {
                return "JSON格式异常";
            }
            string returnMsg = Save(info);
            ATCCargoReceiveEDILog log = new ATCCargoReceiveEDILog(info, "System");
            var v = log.CreateSucceed(info.ShipmentID, json, "Successfully!");
            if (!string.IsNullOrEmpty(returnMsg))
                v = log.CreateEx(returnMsg, info.ShipmentID, json);

            Helper.WriteEdiLog(v, null);

            return returnMsg;
        }

        private string Save(ATCCargoReceiveInfo info)
        {
            string sql = $"SELECT TOP(1)CMP FROM SMORD WHERE SHIPMENT_ID = {SQLUtils.QuotedStr(info.ShipmentID)} AND CNTR_NO = {SQLUtils.QuotedStr(info.ContainerNumber)}";
            
            string cmp = string.Empty;
            try
            {
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                info.cmp = cmp;
            }
            catch
            {
                return "No Find Data!";
            }
            EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", info.ShipmentID);
            ei.PutKey("CNTR_NO", info.ContainerNumber);
            ei.PutDate("ETA", Prolink.Math.GetValueAsDateTime(info.ETA));
            ei.PutDate("ATA", Prolink.Math.GetValueAsDateTime(info.ATA));
            ei.PutDate("DISCHARGE_DATE", Prolink.Math.GetValueAsDateTime(info.DischargeDate));
            ei.PutDate("PICKUP_CDATE", Prolink.Math.GetValueAsDateTime(info.PickupDate));
            ei.PutDate("EMPTY_TIME", Prolink.Math.GetValueAsDateTime(info.EmptyReturnDate));
            ei.Put("BACK_LOCATION", info.EmptyReturnPlace);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
            
            return "";
        }
    }
}
