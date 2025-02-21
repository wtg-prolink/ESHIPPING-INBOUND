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
    public class FCL03DataGetter : Prolink.WebReport.Business.IDataGetter
    {

        public DataSet GetDataSet(NameValueCollection nameValues)
        {
            nameValues.Add("RPT_DESCP_PARAMTER", "Booking From1");
            //string uId = Prolink.Math.GetValueAsString(nameValues["uid"]);
           /* DataSet ds = new DataSet();
            string sql = "SELECT * FROM V_CHM02";
            DataTable dt = GetDataTable("V_CHM02", sql);
            */
            string UId = Prolink.Math.GetValueAsString(nameValues["UId"]);
            DataSet ds = new DataSet();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = "";
            DataTable dt = ModelFactory.InquiryData("V_FCL03.*", "V_FCL03", condition, nameValues, ref recordsCount, ref pageIndex, ref pageSize);
            dt.TableName = "FCL01";
            var relKeyArray = dt.AsEnumerable().Select(r => r.Field<string>("SHIPMENT_ID")).Distinct().ToArray();
            string relFilter = SQLUtils.QuotedStr(string.Join(",", relKeyArray)).Replace(",", SQLUtils.QuotedStr(","));
            string sql = "SELECT SMDNP.*,SMDN.SHIPMENT_ID, SM.* FROM SMDN LEFT JOIN SMDNP ON SMDN.DN_NO=SMDNP.DN_NO LEFT JOIN Smcuft SM ON SMDN.DN_NO=SM.DN_NO WHERE ISNULL(SMDN.SHIPMENT_ID,'') <> '' AND SMDN.SHIPMENT_ID IN (" + relFilter + ") ";
            DataTable SMDNP = GetDataTable("SMDNP", sql);
            DataColumn[] parentColumns = new DataColumn[] { dt.Columns["SHIPMENT_ID"] };
            DataColumn[] childrenColumns = new DataColumn[] { SMDNP.Columns["SHIPMENT_ID"] };
            try
            {
                ds.Tables.Add(dt);
                ds.Tables.Add(SMDNP);
                ds.Relations.Add(parentColumns, childrenColumns);
            }
            catch (Exception ex)
            {
            }
            return ds;
        }

        public DataTable GetDataTable(string name, string sql)
        {
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, ReporterRegister.DelegateConnection);
            dt.TableName = name;
            return dt;
        }
    }
}