using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;

namespace TrackingEDI.Business
{
    public class ForecastParser : BaseParser
    { 
        string mapping = "SmordForecastMapping";
        public ForecastParser()
        { 
        }
         
        public void CreateForecast(string[] keyList, ref string newUids)
        {
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm["bidUid"] = Guid.NewGuid().ToString();
            MixedList ml = new MixedList(); 
            DataTable dt = GetSmordDt(keyList, ml);
            RegisterEditInstructFunc(mapping, SmordForecastInstruct);
            ParseEditInstruct(dt, mapping, ml, parm);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            newUids = parm["bidUid"].ToString();
        }
         
        public string SmordForecastInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string newUid = Guid.NewGuid().ToString();
            ei.Put("BID_UID", parm["bidUid"]);
            ei.Put("NEW_UID", newUid); 
            ei.PutDate("CREATE_DATE", DateTime.Now);


            string shipmentId = ei.Get("SHIPMENT_ID"); 
            string sql = string.Format("SELECT FOB_AMT,CIF_AMT FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                ei.Put("FOB_AMT", dt.Rows[0]["FOB_AMT"]);
                ei.Put("CIF_AMT", dt.Rows[0]["CIF_AMT"]);
            }

            return string.Empty;
        }

        public DataTable GetSmordDt(string[] keyList, MixedList ml)
        {
            string sql = string.Format("DELETE FROM SMORD_FORECAST WHERE CREATE_DATE<={0}", SQLUtils.QuotedStr(DateTime.Now.AddDays(-7).ToString("yyyyMMdd")));
            ml.Add(sql);

            string condition = "";
            for (int i = 0; i < keyList.Length; i++)
            {
                string[] shipCntr = keyList[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                string shipmentId = shipCntr[0];
                string cntrNo = shipCntr[1];
                if (i != 0)
                {
                    condition += " OR ";
                }
                condition += string.Format("(SHIPMENT_ID={0} AND CNTR_NO={1})", SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(cntrNo));
            }

            sql = string.Format("SELECT * FROM SMORD WHERE {0}", condition);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }
    }
}
