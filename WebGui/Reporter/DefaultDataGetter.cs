using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;

namespace WebGui.Reporter
{
    public class DefaultDataGetter : Prolink.WebReport.Business.IDataGetter
    {
        public DataSet GetDataSet(NameValueCollection nameValues)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                dt.TableName = "NUMMST";
                dt.Columns.Add("CUSTOM_NO");
                dt.Columns.Add("CUSTOM_NAME");
                dt.Columns.Add("UI_NO");

                DataRow dr = dt.NewRow();
                dr["CUSTOM_NO"] = "xx1";
                dr["CUSTOM_NAME"] = "xx1a";
                dr["UI_NO"] = "xx1b";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["CUSTOM_NO"] = "xx2";
                dr["CUSTOM_NAME"] = "xx2a";
                dr["UI_NO"] = "xx2b";
                dt.Rows.Add(dr);


                ds.Tables.Add(dt);
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