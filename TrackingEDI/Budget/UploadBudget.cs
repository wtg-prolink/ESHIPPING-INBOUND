using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;
using TrackingEDI.Business;

namespace TrackingEDI.Budget
{
    public class UploadBudget
    {
        public static bool CheckPortGroup(string portvalue, ref string regiongroup)
        {
            if (portvalue.Length < 5) return false;
            Tuple<string, string> port = GetPortCode(portvalue);
            string sql = string.Format("SELECT TOP 1 REGION_GROUP FROM BSCITY WHERE PORT_CD={0} AND CNTRY_CD={1}", SQLUtils.QuotedStr(port.Item2), SQLUtils.QuotedStr(port.Item1));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt != null && dt.Rows.Count > 0)
            {
                regiongroup = Prolink.Math.GetValueAsString(dt.Rows[0]["REGION_GROUP"]);
                return true;
            }
            return false; 
        }

        public static Tuple<string, string> GetPortCode(string prolinkCD)
        {
            if (string.IsNullOrEmpty(prolinkCD)) return null;
            if (prolinkCD.Length < 5) return null;
            string country = prolinkCD.Substring(0, 2);
            string portCode = prolinkCD.Substring(2, 3);
            return new Tuple<string, string>(country, portCode);
        }
    }
}
