using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Prolink.DataOperation;
using System.Collections.Specialized;
using Prolink.Data;
using Prolink.V3;


namespace ReportGetter
{
    public class IPQ03DataGetter : Prolink.WebReport.Business.IDataGetter
    {

        public DataSet GetDataSet(NameValueCollection nameValues)
        {
            nameValues.Add("RPT_DESCP_PARAMTER", "Picking List");
            string uId = Prolink.Math.GetValueAsString(nameValues["uid"]);
           /* DataSet ds = new DataSet();
            string sql = "SELECT * FROM V_CHM02";
            DataTable dt = GetDataTable("V_CHM02", sql);
            */
            //string lcno = Prolink.Math.GetValueAsString(nameValues["lcno"]);
            string condition = "";
            if (uId != "")
            {
                condition = " PU_FID=" + SQLUtils.QuotedStr(uId);
            }
            
            DataSet ds = new DataSet();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            DataTable dt = ModelFactory.InquiryData("V_IPQ03.*", "V_IPQ03", condition, nameValues, ref recordsCount, ref pageIndex, ref pageSize);
            var relKeyArray = dt.AsEnumerable().Select(r => r.Field<string>("DN_NO")).Distinct().ToArray();
            //string relFilter = SQLUtils.QuotedStr(string.Join(",", relKeyArray)).Replace(",", SQLUtils.QuotedStr(","));
            //string sql = "SELECT CNTR_NO, SEAL_NO1, CNTY_TYPE,DN_NO,GW,NW,CBM FROM SMRV WHERE DN_NO IN (" + relFilter + ") ";

            string con = GetLikeStr("DN_NO", relKeyArray);
            if (string.IsNullOrEmpty(con)) con = "1=2";
            string sql = "SELECT CNTR_NO, SEAL_NO1, CNTY_TYPE,DN_NO,GW,NW,CBM FROM SMRV WHERE "+con ;
            DataTable SMRV = GetDataTable("SMRV", sql);
            DataColumn[] parentColumns = new DataColumn[] { dt.Columns["DN_NO"] };
            DataColumn[] childrenColumns = new DataColumn[] { SMRV.Columns["DN_NO"] };
            try
            {
                ds.Tables.Add(dt);
                ds.Tables.Add(SMRV);
                ds.Relations.Add(parentColumns, childrenColumns);
            }
            catch (Exception ex)
            {
            }
            return ds;
        }

        public string GetLikeStr(string name,string [] str)
        {
            string val = string.Empty;
            foreach (string n in str)
            {
                if (!string.IsNullOrEmpty(val))
                    val += " OR";
                val += string.Format("{0} LIKE {1} ", name, SQLUtils.QuotedStr("%" + n + "%"));
            }
            if (!string.IsNullOrEmpty(val))
                val = "("+ val+")";
            return val;
        }
        public DataTable GetDataTable(string name, string sql)
        {
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, ReporterRegister.DelegateConnection);
            dt.TableName = name;
            return dt;
        }
    }
}