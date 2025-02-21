using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
    public class RdnParser:BaseParser
    {
        public void saveToSmrdn(string ShipmentId, MixedList ml, string Pol, string PolNm, string OrdNo,bool isFirst)
        {
            string sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return;
            foreach (DataRow row in dt.Rows)
            {
                DataTable clDT = dt.Clone();
                clDT.ImportRow(row);
                saveToSmrdn(clDT, ml, Pol, PolNm, OrdNo, isFirst);
            }
        }
        public void saveToSmrdn(DataTable dt, MixedList ml, string Pol, string PolNm, string OrdNo,bool isFirst)
        {
            if (dt == null || dt.Rows.Count <= 0) return;
            Dictionary<string, object> parm = new Dictionary<string, object>();
            string shipmentId = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
            string sql = string.Format("SELECT TOP 1 TRAN_TYPE1,POL_NM2,DEP_ADDR2,PRODUCTION_DATE,CMP FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable SMIdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (SMIdt.Rows.Count <= 0 || SMIdt == null)
                return;
            string tranType1 = Prolink.Math.GetValueAsString(SMIdt.Rows[0]["TRAN_TYPE1"]);
            string PolNm2 = Prolink.Math.GetValueAsString(SMIdt.Rows[0]["POL_NM2"]);
            string DepAddr2 = Prolink.Math.GetValueAsString(SMIdt.Rows[0]["DEP_ADDR2"]);
            string arrivalDate = Prolink.Math.GetValueAsString(SMIdt.Rows[0]["PRODUCTION_DATE"]);
            string cmp= Prolink.Math.GetValueAsString(SMIdt.Rows[0]["CMP"]);
            if (!("T").Equals(tranType1) && isFirst)
            {
                parm["WS_CD"] = "";
                parm["DLV_AREA_NM"] = PolNm2;
                parm["DLV_ADDR"] = DepAddr2;
            }
            else
            {
                string WsCd = Prolink.Math.GetValueAsString(dt.Rows[0]["WS_CD"]);
                string AddrCode = Prolink.Math.GetValueAsString(dt.Rows[0]["ADDR_CODE"]);
                if (string.IsNullOrEmpty(WsCd) && !string.IsNullOrEmpty(AddrCode))
                {
                    sql = string.Format("SELECT TOP 1 WS_CD FROM SMWH WHERE DLV_ADDR={0} AND CMP={1}", SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(cmp));
                    WsCd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                parm["WS_CD"] = WsCd;
                parm["DLV_AREA_NM"] = Prolink.Math.GetValueAsString(dt.Rows[0]["DLV_AREA_NM"]);
                parm["DLV_ADDR"] = Prolink.Math.GetValueAsString(dt.Rows[0]["DLV_ADDR"]);
            }
            parm["PICK_AREA"] = Pol;
            parm["PICK_AREA_NM"] = PolNm;
            parm["ORD_NO"] = OrdNo;
            parm["ARRIVAL_DATE"] = arrivalDate;
            RegisterEditInstructFunc("InboundSMRDNMapping", RdnEditInstruct);
            ParseEditInstruct(dt, "InboundSMRDNMapping", ml, parm);
        }
        static string RdnEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.Put("ORD_NO", parm["ORD_NO"]);
            ei.Put("PICK_AREA_NM", parm["PICK_AREA_NM"]);
            ei.Put("PICK_AREA", parm["PICK_AREA"]);
            ei.Put("DLV_ADDR", parm["DLV_ADDR"]);
            ei.Put("PICK_AREA", parm["PICK_AREA"]);
            ei.Put("DLV_ADDR", parm["DLV_ADDR"]);
            ei.Put("DLV_AREA_NM", parm["DLV_AREA_NM"]);
            ei.Put("WS_CD", parm["WS_CD"]);
            ei.PutDate("ARRIVAL_DATE", parm["ARRIVAL_DATE"]);
            return string.Empty;
        }
    }
}
