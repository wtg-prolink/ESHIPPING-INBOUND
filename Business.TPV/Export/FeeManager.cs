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
    public class FeeManager : ManagerBase
    {
        public ResultInfo TryPostFeeInfo(string sapId, string shipmentId, string location)
        {
            List<FeeInfo> items = GetFeeInfo(shipmentId);
            return TryPostFeeInfo(sapId, items,null,location);
        }

        public ResultInfo TryPostFeeInfo(string sapId, List<FeeInfo> items, string opUser = null, string location="")
        {
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            CheckIsPost(items);
            items = items.Where(item => item.IsPost).ToList();
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            FeeEDI edi = new FeeEDI();
            var result = edi.TryPostFeeInfo(sapId, items, location);
            foreach (var item in items)
            {
                string fileName = BackupData(item, new List<string> { "Export", "Fee" }, string.Format("{0}_{1}", item.DNNO, GetCurrentTimeString()));
                FeeEDILog log = new FeeEDILog(item, opUser);
                var v = result.IsSucceed ? log.CreateSucceed() : log.CreateEx(result.Description);
                v.DataFolder = fileName;
                Logger.WriteLog(v);
            }
            return result;
        }

        void CheckIsPost(List<FeeInfo> items)
        {
            List<string> bus = items.Select(v => v.BU).Distinct().Where(bu => !string.IsNullOrEmpty(bu)).ToList();
            if (bus == null || bus.Count <= 0) return;
            string sql = "SELECT CD FROM BSCODE WHERE CD_TYPE='TVKO' AND AP_CD='N'";
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return;
            List<string> notPostList = dt.Rows.Cast<DataRow>().Select(row => Prolink.Math.GetValueAsString(row["CD"])).
                Distinct().Where(cd => !string.IsNullOrEmpty(cd)).ToList();
            items.ForEach(item =>
                {
                    if (notPostList.Contains(item.BU))
                        item.IsPost = false;
                });
        }

        List<FeeInfo> GetFeeInfo(string shipmentId)
        {
            string sql = string.Format(@"SELECT SHIPMENT_ID,DN_NO,FREIGHT_FEE,ISSUE_FEE,FOB_VALUE,
             (SELECT TOP 1 BU FROM SMDN WHERE SMDN.DN_NO=SMINM.DN_NO) AS BU FROM SMINM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows.Cast<DataRow>().Select(row => new FeeInfo
                {
                    DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),
                    FreightFee = Prolink.Math.GetValueAsDouble(row["FREIGHT_FEE"]),
                    InsuranceFee = Prolink.Math.GetValueAsDouble(row["ISSUE_FEE"]),
                    FOBFee = Prolink.Math.GetValueAsDouble(row["FOB_VALUE"]),
                    IsPost = true,
                    BU = Prolink.Math.GetValueAsString(row["BU"])
                }).Distinct().Where(item => !string.IsNullOrEmpty(item.DNNO)).ToList();
        }
    }

    public class FeeInfo
    {
        public string DNNO { get; set; }
        public double FreightFee { get; set; }
        public double InsuranceFee { get; set; }
        public double FOBFee { get; set; }
        public string BU { get; set; }
        public bool IsPost { get; set; }
    }
}
