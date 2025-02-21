using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;

namespace NotifyService.Task
{
    /// <summary>
    /// 报价相关任务
    /// </summary>
    public class QuotTask : IPlanTask
    {
        public void Run(IPlanTaskMessenger messenger)
        {
             //OperationUtils.ExecuteUpdate("UPDATE SMDN SET SEAl_QTY=QTY WHERE CARGO_TYPE='A' AND TRAN_TYPE IN ('D','E') AND SEAl_QTY<>QTY AND (SELECT COUNT(1) FROM SMRV WHERE SMRV.DN_NO=SMDN.DN_NO)>0", Prolink.Web.WebContext.GetInstance().GetConnection());

            //string id = string.Empty;
            //string sql = "SELECT * FROM SMRQM WHERE STATUS='B'";
            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //EditInstruct ei = null;
            //MixedList ml = new MixedList();

            //DateTime now = DateTime.Now;
            //now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            //foreach (DataRow dr in dt.Rows)
            //{
            //    if (dr["RFQ_TO"] == null || dr["RFQ_TO"] == DBNull.Value)
            //        continue;
            //    DateTime rfq_to = (DateTime)dr["RFQ_TO"];
            //    rfq_to = new DateTime(rfq_to.Year, rfq_to.Month, rfq_to.Day, 0, 0, 0);

            //    id = Prolink.Math.GetValueAsString(dr["U_ID"]);
            //    ei = new EditInstruct("SMRQM", EditInstruct.UPDATE_OPERATION);
            //    ei.PutKey("U_ID", id);
            //    ei.PutKey("STATUS", "B");
            //    ei.Put("STATUS", "C");

            //    if (now.CompareTo(rfq_to) > 0)
            //        ml.Add(ei);
            //    //ml.Add(string.Format("UPDATE SMRQD SET STATUS='B' WEHRE U_FID={0} AND (STATUS='Q' OR STATUS IS NULL)", SQLUtils.QuotedStr(id)));
            //}
            //if (ml.Count > 0)
            //    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());


            //sql = "SELECT * FROM SMQTM WHERE QUOT_TYPE='Q'";//报价
            //dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //foreach (DataRow dr in dt.Rows)
            //{
            //    if (dr["RFQ_TO"] == null || dr["RFQ_TO"] == DBNull.Value)
            //        continue;
            //    DateTime rfq_to = (DateTime)dr["RFQ_TO"];
            //    rfq_to = new DateTime(rfq_to.Year, rfq_to.Month, rfq_to.Day, 0, 0, 0);

            //    id = Prolink.Math.GetValueAsString(dr["U_ID"]);
            //    ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            //    ei.PutKey("U_ID", id);
            //    ei.PutKey("QUOT_TYPE", "Q");
            //    ei.Put("QUOT_TYPE", "B");//投标

            //    if (now.CompareTo(rfq_to) > 0)
            //    {
            //        ml.Add(ei);
            //        ml.Add(string.Format("UPDATE SMQTD SET QUOT_TYPE='B' WHERE U_FID={0} AND (QUOT_TYPE='Q' OR QUOT_TYPE IS NULL)", SQLUtils.QuotedStr(id)));
            //    }
            //}
        }
    }
}
