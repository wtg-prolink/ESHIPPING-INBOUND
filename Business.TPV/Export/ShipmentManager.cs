using Business.EDI;
using Business.Service;
using Business.TPV.Base;
using Business.Utils;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Export
{
    public abstract class ShipmentManager : WebRequstBase
    {
        protected virtual void CheckRuntime(Runtime runtime)
        {
            if (runtime == null) throw new Exception("runtime con't be null!");
            EntityValidationResult result = ValidationHelper.ValidateEntity(runtime);
            if (result.HasError)
                throw new EntityValidationResultException(result);
        }

        protected DataRow QuerySM(Runtime runtime)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows[0];
        }

        protected const string PartyCode_ShipTo = "WE";
        protected const string PartyCode_Shipper = "SH";
        protected const string PartyCode_Track = "CR";
        protected const string PartyCode_Consignee = "CS";
        protected const string PartyCode_Notify1 = "NT";
        protected const string PartyCode_Notify2 = "Z4";
        protected const string PartyCode_Notify3 = "Z5";
        protected const string PartyCode_Carrier = "FS";
        protected const string PartyCode_BookingAgent_BO = "BO";
        protected const string PartyCode_BookingAgent_SP = "SP";

        protected DataTable QueryPartyDT(string shipmentId, IEnumerable<string> partyCodes)
        {
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN({1})", SQLUtils.QuotedStr(shipmentId),
                string.Join(",", partyCodes.Select(c => SQLUtils.QuotedStr(c))));
            return DB.GetDataTable(sql, new string[] { });
        }
    }
}
