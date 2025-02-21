using Models.EDI;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.SI
{
    public class DataGetter
    {
        public virtual SIProfileInfo QuerySIProfileInfo(string profileCode, string dnLevel = "")
        {
            if (string.IsNullOrEmpty(profileCode)) return null;
            string sql = string.Format("SELECT * FROM SMSIM WHERE PROFILE={0}", SQLUtils.QuotedStr(profileCode));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            DataRow row = dt.Rows[0];
            SIProfileInfo info = new SIProfileInfo();
            info.ACRemark = Prolink.Math.GetValueAsString(row["LSP_INFO1"]);
            info.ShippingMark = Prolink.Math.GetValueAsString(row["SHIPPING_MARK"]);
            info.InvoiceIntruction = Prolink.Math.GetValueAsString(row["INV_SPEC"]);
            info.PackingIntruction = Prolink.Math.GetValueAsString(row["PK_SPEC"]);
            info.BLIntruction = Prolink.Math.GetValueAsString(row["BL_SPC_REQ"]);
            info.Goods = Prolink.Math.GetValueAsString(row["COMMODITY"]);
            info.Vendor = Prolink.Math.GetValueAsString(row["VENDOR_CD"]);
            info.ISFSellerCode = Prolink.Math.GetValueAsString(row["ISF_SELLER"]);
            info.BLRemark = GetBLRemark(row);
            info.InvoiceRemark = GetInvRemark(row);
            info.PackingRemark = GetPackingRemark(row);
            info.Parties = GetParties(row, dnLevel);
            return info;
        }
        protected string GetBLRemark(DataRow simRow)
        {
            List<string> items = new List<string>();
            Action<string> add = (txt) =>
            {
                if (string.IsNullOrEmpty(txt)) return;
                if (items.Contains(txt)) return;
                items.Add(txt);
            };
            add(Prolink.Math.GetValueAsString(simRow["BL_REMARK1"]));
            add(Prolink.Math.GetValueAsString(simRow["BL_REMARK2"]));
            add(Prolink.Math.GetValueAsString(simRow["BL_REMARK3"]));
            add(Prolink.Math.GetValueAsString(simRow["BL_REMARK4"]));
            add(Prolink.Math.GetValueAsString(simRow["BL_REMARK5"]));
            add(Prolink.Math.GetValueAsString(simRow["BL_REMARK6"]));
            return string.Join(Environment.NewLine, items);
        }

        protected string GetInvRemark(DataRow simRow)
        {
            List<string> items = new List<string>();
            Action<string> add = (txt) =>
            {
                if (string.IsNullOrEmpty(txt)) return;
                if (items.Contains(txt)) return;
                items.Add(txt);
            };
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK1"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK2"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK3"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK4"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK5"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK6"]));
            return string.Join(Environment.NewLine, items);
        }
        protected string GetPackingRemark(DataRow simRow)
        {
            List<string> items = new List<string>();
            Action<string> add = (txt) =>
            {
                if (string.IsNullOrEmpty(txt)) return;
                if (items.Contains(txt)) return;
                items.Add(txt);
            };
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK1"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK2"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK3"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK4"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK5"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK6"]));
            return string.Join(Environment.NewLine, items);
        }
        protected Dictionary<string, PartnerInfo> GetParties(DataRow simRow, string dnLevel = "")
        {
            Dictionary<string, string> partyCodes = new Dictionary<string, string>();
            Action<string, string> add = (type, code) =>
            {
                if (string.IsNullOrEmpty(code)) return;
                partyCodes[type] = code;
            };
            string column = "SELLER";
            switch (dnLevel)
            {
                case "2st":
                    column = "BUYER1";
                    break;
                case "3rd":
                    column = "BUYER2";
                    break;
            }
            add("SH", Prolink.Math.GetValueAsString(simRow[column]));
            add("FS", Prolink.Math.GetValueAsString(simRow["CARRIER1"]));
            add("SP", Prolink.Math.GetValueAsString(simRow["LSP_NO1"]));
            add("CC", Prolink.Math.GetValueAsString(simRow["CC_CD"]));
            add("DT", Prolink.Math.GetValueAsString(simRow["DT_CD"]));

            DataTable ptDT = Helper.QueryPartyDT(partyCodes.Values);
            Dictionary<string, PartnerInfo> parties = new Dictionary<string, PartnerInfo>();
            foreach (var item in partyCodes)
            {
                DataRow row = Helper.QueryPartyRow(item.Value, ptDT);
                if (row == null) continue;
                parties[item.Key] = Helper.CreatePartnerInfo(item.Key, row);
            }
            return parties;
        }
    }
}
