using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Export
{
    public class PackingManager : ManagerBase
    {
        public ResultInfo TryPostPackingInfo(string sapId, List<PackingInfo> items, string location, out  ResultInfo reserveResult)
        {
            reserveResult = null;
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            //CheckIsPost(items);
            //items = items.Where(item => item.IsPost).ToList();
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            PackingEDI edi = new PackingEDI();
            var result = edi.TryPostPackingInfo(sapId, items, location,out reserveResult);
            var item = items[0];
            string fileName = BackupData(item, new List<string> { "Export", "Packing" }, string.Format("{0}_{1}", item.DNNO, GetCurrentTimeString()));
            PackingEDILog log = new PackingEDILog(item, "");
            var v = result.IsSucceed ? log.CreateSucceed() : log.CreateEx(result.Description);
            v.DataFolder = fileName;
            Logger.WriteLog(v);
            return result;
        }

        public List<PackingInfo> GetPackingInfo(string uid)
        {
            string sql = string.Format(@"SELECT SMINP.* FROM SMINP WHERE U_FID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            sql = string.Format(@"SELECT * FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable Sminmdt = DB.GetDataTable(sql, new string[] { });

            if (dt == null || dt.Rows.Count <= 0) return null;
            //string from = Prolink.Math.GetValueAsString(Sminmdt.Rows[0]["FROM_CD"]);
            //if (from.Length > 2)
            //{
            //    from = from.Substring(0, 2);
            //}
            string shipmentid = Prolink.Math.GetValueAsString(Sminmdt.Rows[0]["SHIPMENT_ID"]);
            string cmp = Prolink.Math.GetValueAsString(Sminmdt.Rows[0]["CMP"]);
            string groupid = Prolink.Math.GetValueAsString(Sminmdt.Rows[0]["GROUP_ID"]);
            string stn = "*";
            sql = string.Format(@"SELECT TOP 1 LOCATION FROM(
SELECT PARTY_NO,(SELECT TOP 1 AP_CD FROM BSCODE WHERE CD_TYPE='CULO' 
            AND CD=SMSMPT.PARTY_NO AND (CMP='*' OR CMP={0})) AS LOCATION FROM SMSMPT WHERE SHIPMENT_ID={1}
                AND PARTY_TYPE IN ('CS','NT','WE'))T WHERE  LOCATION IS NOT NULL",SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(shipmentid));
            string receivelocation=DB.GetValueAsString(sql);

            return dt.Rows.Cast<DataRow>().Select(row => new PackingInfo
            {
                GroupID = groupid,
                CMP = cmp,
                STN = stn,
                DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),
                Pallet = Prolink.Math.GetValueAsString(row["PLA_NO"]),
                From = Prolink.Math.GetValueAsString(row["CASE_FROM"]),
                To = Prolink.Math.GetValueAsString(row["CASE_TO"]),
                Material = Prolink.Math.GetValueAsString(row["IPART_NO"]),
                Quantity = Prolink.Math.GetValueAsDouble(row["TTL_QTY"]),
                Unit = Prolink.Math.GetValueAsString(row["QTYU"]),
                NetWeight = Prolink.Math.GetValueAsDouble(row["TTL_NW"]),
                GrossWeight = Prolink.Math.GetValueAsDouble(row["TTL_GW"]),
                Volume = Prolink.Math.GetValueAsDouble(row["TTL_CBM"]),
                SupplierInvoiceNo = Prolink.Math.GetValueAsString(Sminmdt.Rows[0]["SUPPLIER_INV_NO"]),
                OriginalCntry = Prolink.Math.GetValueAsString(row["CNTRY_ORN"]),
                ReceiveLocation = receivelocation
            }).Distinct().Where(item => !string.IsNullOrEmpty(item.DNNO)).ToList();
        }

    }

    public class PackingInfo
    {
        public string DNNO { get; set; }
        public string Pallet { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Material { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public double NetWeight { get; set; }

        public double GrossWeight { get; set; }
        public double Volume { get; set; }
        public string SupplierInvoiceNo { get; set; }
        public string OriginalCntry { get; set; }
        public string ReceiveLocation { get; set; }
        public string GroupID { get; set; }
        public string CMP { get; set; }
        public string STN { get; set; }
    }
}
