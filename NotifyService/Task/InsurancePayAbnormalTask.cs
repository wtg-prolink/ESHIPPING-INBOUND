using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NotifyService.Task
{
    public class InsurancePayAbnormalTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            string Cmpsql = "SELECT DISTINCT CMP FROM SYS_SITE WHERE GROUP_ID='TPV' AND CMP !='*'";
            DataTable Cmpdt = OperationUtils.GetDataTable(Cmpsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow Cmpdr in Cmpdt.Rows)
            {
                string Cmp = Prolink.Math.GetValueAsString(Cmpdr["CMP"]);
                if (string.IsNullOrEmpty(Cmp)) continue;
                string datesql = string.Format("SELECT * FROM BSCODE WHERE CD_TYPE='IPAC' AND (CMP={0}  OR CMP='*')", SQLUtils.QuotedStr(Cmp));
                DataTable datedt = OperationUtils.GetDataTable(datesql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                int A = 0, B = 0;
                if (datedt != null && datedt.Rows.Count > 0)
                {
                    foreach (DataRow dr in datedt.Rows)
                    {
                        string cd = Prolink.Math.GetValueAsString(dr["CD"]);
                        switch (cd)
                        {
                            case "A": A = Prolink.Math.GetValueAsInt(dr["AP_CD"]); break;
                            case "B": B = Prolink.Math.GetValueAsInt(dr["AP_CD"]); break;
                            //case "C": C = Prolink.Math.GetValueAsInt(dr["AP_CD"]); break;
                        }
                    }

                    MixedList ml = new MixedList();
                    //申請中(申請日期目前相比超過60天)
                    if (A != 0)
                    {
                        string sql = string.Format("SELECT * FROM SMIPM WHERE APPLICATION_DATE<(GETDATE()-{0}) AND STATUS ='Y' AND CMP={1}", A, SQLUtils.QuotedStr(Cmp));
                        DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string Desc = string.Format("申請日期目前相比超过{0}天", A);
                        Ergodic(dt, Desc, ml);
                    }
                    //財務審核(索賠確認日期與目前相比超過30天)
                    if (B != 0)
                    {
                        string sql = string.Format("SELECT * FROM SMIPM WHERE CONFIRM_DATE<(GETDATE()-{0}) AND STATUS ='C' AND CMP={1}", B, SQLUtils.QuotedStr(Cmp));
                        DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string Desc = string.Format("索赔确认日期与目前相比超过{0}天", B);
                        Ergodic(dt, Desc, ml);
                    }

                    //if (C != 0)
                    //{
                    //    string sql = string.Format("SELECT * FROM SMIPM WHERE APPLICATION_DATE<(GETDATE()-{0}) AND STATUS ='Y'", C);
                    //    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //    string Desc = string.Format("金额确认时间超过系统当前日期{0}天", C);
                    //    Ergodic(dt, Desc, ml);
                    //}

                    try
                    {
                        if (ml.Count > 0)
                        {
                            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                    }
                    catch (Exception e)
                    {
                        _messenger.GetLogger().WriteLog(e);
                    }
                }
            }
        }
        public void Ergodic(DataTable dt, string Desc, MixedList ml)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string UID = Prolink.Math.GetValueAsString(dr["U_ID"]);
                EditInstruct ei = new EditInstruct("SMIPM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID",UID);
                ei.Put("EXCEPTION_DESC",Desc);
                ei.Put("STATUS", "I");
                ml.Add(ei);
            }
        }
    }
}
