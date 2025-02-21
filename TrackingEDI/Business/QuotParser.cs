using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Prolink.DataOperation;

namespace TrackingEDI.Business
{
    public class QuotParser:BaseParser
    {
        static string QuotEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string out_in = ei.Get("OUT_IN");
            if (string.IsNullOrEmpty(out_in))
                out_in = "O";
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("OUT_IN", out_in);
            ei.Put("QUOT_TYPE", "Q");
            ei.Put("CMP", Prolink.Math.GetValueAsString(dr["SYS_CMP"]));
            ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(dr["SYS_GROUP_ID"]));
            ei.Put("CREATE_BY", Prolink.Math.GetValueAsString(dr["SYS_USER_ID"]));
            ei.PutDate("CREATE_DATE", DateTime.Now);
            return string.Empty;
        }

        /// <summary>
        /// 询价信息保存到报价信息中
        /// </summary>
        /// <param name="u_id"></param>
        public void SaveToQuot(string u_id,string groupId,string cmp,string user)
        {
            if (string.IsNullOrEmpty(u_id))
                return;

            string sql = string.Format("SELECT * FROM SMQTM WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
                return;

            MixedList ml = new MixedList();
            sql = string.Format("SELECT SMRQM.*,'{1}' AS SYS_USER_ID,'{2}' AS SYS_GROUP_ID,'{3}' AS SYS_CMP FROM SMRQM WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id), user, groupId, cmp);
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            RegisterEditInstructFunc("QuotMapping", QuotEditInstruct);
            ParseEditInstruct(dt, "QuotMapping",ml,null);
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
    }
}
