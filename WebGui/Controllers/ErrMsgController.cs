using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;

namespace WebGui.Controllers
{
    public class ErrMsgController : BaseController
    {
        //
        // GET: /IPCTM/

        public ActionResult ErrMsgSetup()
        {
            return View();
        }

        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                //dt = GetStatusCountData(statusField, table, condition, Request.Params);
                pageSize = 1;
            }
            else
            {
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                if (resultType == "excel")
                    return ExportExcelFile(dt);
            }
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        

        public ActionResult getErrManageData()
        {
           
            return GetBootstrapData("TMEXP", GetBaseGroup());
        }

        public JsonResult getErrMsgData()
        {
            string JobNo = Prolink.Math.GetValueAsString(Request.Params["JobNo"]);
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            //int recordsCount = 0, pageIndex = 0, pageSize = 20;
            string coditions = "WHERE A.JOB_NO=" + SQLUtils.QuotedStr(JobNo) +" ORDER BY SEQ_NO";
            DataTable dt = OperationUtils.GetDataTable(@"SELECT (SELECT TOP 1 CD_DESCP FROM BSCODE WHERE 
            CD_TYPE='AST' AND CD=A.EXP_TYPE AND CMP=A.CMP) AS CD_DESCP,A.*,B.EXP_DESCP AS EXP_DESCP1,B.IS_RELIEVE from TMEXP A left join EXPRELA B on A.EXP_TYPE=B.EXP_TYPE and A.EXP_REASON=B.EXP_REASON and A.GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + "and A.cmp=" + SQLUtils.QuotedStr(CompanyId) + coditions, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //DataTable dt = OperationUtils.GetDataTable("SELECT A.*,B.CD_DESCP from TMEXP A left join BSCODE B on A.GROUP_ID=B.GROUP_ID and A.CMP=B.CMP and A.STN=B.STN and A.EXP_TYPE=B.CD  and B.CD_TYPE='AST' and A.GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + "and A.cmp=" + SQLUtils.QuotedStr(CompanyId) + coditions, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            return Json(new { returnData = ModelFactory.ToTableJson(dt) });
        }
     
         public JsonResult iErrMsg()
        {
            string SeqNo = Prolink.Math.GetValueAsString(Request.Params["SeqNo"]);
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            string JobNo = Prolink.Math.GetValueAsString(Request.Params["JobNo"]);
            
            string returnMessage = "success";
           
            MixedList mixedlist = new MixedList();

            string sql = "UPDATE TMEXP SET CANCEL_BY=" + SQLUtils.QuotedStr(UserId) + ",CANCEL_DATE=getdate() WHERE SEQ_NO=" + SQLUtils.QuotedStr(SeqNo) + " AND U_FID=" + SQLUtils.QuotedStr(UFid) + " AND JOB_NO=" + SQLUtils.QuotedStr(JobNo);
                mixedlist.Add(sql);
          
            if (mixedlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage });
        }

         public ActionResult GetSelectOptions()
         {
             return Json(getSelect());
         }

         public TemxpOptions getSelect()
         {
             
             //String sql = "SELECT * FROM BSCODE LEFT JOIN EXPRELA ON EXPRELA.EXP_TYPE = BSCODE.CD WHERE BSCODE.CD_TYPE='AST' AND BSCODE.CD_DESCP=" + SQLUtils.QuotedStr(Options);
             string sql = "SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE GROUP_ID='" + GroupId + "' AND (CMP='" + CompanyId + "' OR CMP='*') AND CD_TYPE IN('AST')";
             DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
             TemxpOptions iOptions = new TemxpOptions();
             string cd, cdDescp, cdType;
             foreach (DataRow dr in dt.Rows)
             {
                 cd = Prolink.Math.GetValueAsString(dr["CD"]);
                 cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                 cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]);
                 
                 switch (cdType)
                 {
                     case "AST":
                         {
                             iOptions.Ex.Add(new OptionsItem
                             {
                                 cd = cd,
                                 cdDescp = cdDescp
                             });
                             break;
                         }
                     //case "ASR":
                     //    {
                     //        iOptions.Ex2.Add(new OptionsItem
                     //        {
                     //            cd = cd,
                     //            cdDescp = cdDescp
                     //        });
                     //        break;
                     //    }
                 }
             }            
             return iOptions;
             
         }

         public ActionResult GetErrReasonOptions(String Options)
         {
             string ExpType = Prolink.Math.GetValueAsString(Request.Params["ExpType"]);
             return Json(getErrSelect(ExpType));
         }
         public TemxpOptions getErrSelect(String Options)
         {
             //int recordsCount = 0, pageIndex = 0, pageSize = 20;

             String sql = "SELECT * FROM EXPRELA  WHERE EXP_TYPE=" + SQLUtils.QuotedStr(Options);
             //string sql = "SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CD_TYPE IN('AST')";
             DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
             TemxpOptions iOptions = new TemxpOptions();
             string  expDescp, expReason;
             foreach (DataRow dr in dt.Rows)
             {
                 //cd = Prolink.Math.GetValueAsString(dr["CD"]);
                 expDescp = Prolink.Math.GetValueAsString(dr["EXP_DESCP"]);
                 expReason = Prolink.Math.GetValueAsString(dr["EXP_REASON"]);


                 iOptions.Ex2.Add(new OptionsItem
                 {
                     cd = expReason,
                     cdDescp = expDescp
                 });
                 
             }
             return iOptions;

         }


         public JsonResult InsrtErrMsg()
         {
             string SeqNo = Prolink.Math.GetValueAsString(Request.Params["SeqNo"]);
             string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
             string JobNo = Prolink.Math.GetValueAsString(Request.Params["JobNo"]);
             string ExpObj = Prolink.Math.GetValueAsString(Request.Params["ExpObj"]);
             string ExpCd = Prolink.Math.GetValueAsString(Request.Params["ExpCd"]);
             string ExpType = Prolink.Math.GetValueAsString(Request.Params["ExpType"]);
             string ExpReason = Prolink.Math.GetValueAsString(Request.Params["ExpReason"]);
             string ExpText = Prolink.Math.GetValueAsString(Request.Params["ExpText"]);
             string Dep = Prolink.Math.GetValueAsString(Request.Params["Dep"]);
             string returnMessage = "success";
             string uid = string.Empty;
             MixedList mixedlist = new MixedList();
             uid = Guid.NewGuid().ToString();
             string sql = "INSERT INTO TMEXP (U_ID,U_FID,JOB_NO,SEQ_NO,DEP,EXP_OBJ,EXP_CD,EXP_TYPE,EXP_REASON,EXP_TEXT,WR_ID,WR_DATE,GROUP_ID,CMP,CREATE_BY,CREATE_DATE) VALUES"
                         + "(" + SQLUtils.QuotedStr(uid) + "," + SQLUtils.QuotedStr(UFid) + ","+SQLUtils.QuotedStr(JobNo)+","+SQLUtils.QuotedStr(SeqNo)+","+SQLUtils.QuotedStr(Dep)+","+SQLUtils.QuotedStr(ExpObj)+","+SQLUtils.QuotedStr(ExpCd)+","+SQLUtils.QuotedStr(ExpType)+","+SQLUtils.QuotedStr(ExpReason)
                         + "," + SQLUtils.QuotedStr(ExpText) + "," + SQLUtils.QuotedStr(UserId) + ",getdate()," + SQLUtils.QuotedStr(GroupId) + "," + SQLUtils.QuotedStr(CompanyId)+","+ SQLUtils.QuotedStr(UserId) + ",getdate())";
             mixedlist.Add(sql);

             if (mixedlist.Count > 0)
             {
                 try
                 {
                     int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                 }
                 catch (Exception ex)
                 {
                     returnMessage = ex.ToString();
                 }
             }
             return Json(new { message = returnMessage });
         }
         public void InsrtErrMsgByDic(Dictionary<string, object> list)
         {
             //string SeqNo = Prolink.Math.GetValueAsString(list["SeqNo"]);
             string UFid = Prolink.Math.GetValueAsString(list["UFid"]);
             string JobNo = Prolink.Math.GetValueAsString(list["JobNo"]);
             string ExpObj = Prolink.Math.GetValueAsString(list["ExpObj"]);
             string ExpCd = Prolink.Math.GetValueAsString(list["ExpCd"]);
             string ExpType = Prolink.Math.GetValueAsString(list["ExpType"]);
             string ExpReason = Prolink.Math.GetValueAsString(list["ExpReason"]);
             string ExpText = Prolink.Math.GetValueAsString(list["ExpText"]);
             string Dep = Prolink.Math.GetValueAsString(list["Dep"]);
             string GroupId = Prolink.Math.GetValueAsString(list["GROUP_ID"]);
             string CompanyId = Prolink.Math.GetValueAsString(list["CMP"]);
             string UserId = Prolink.Math.GetValueAsString(list["UserId"]);
             string returnMessage = "success";
             string uid = string.Empty;
             MixedList mixedlist = new MixedList();
             uid = Guid.NewGuid().ToString();
//             string sql = string.Format(@"(SELECT ISNULL(MAX(SEQ_NO),0)+1 FROM TMEXP WHERE
//   U_FID ='{0}')", UFid);
             EditInstruct ei = new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
             ei.Put("U_ID", uid);
             ei.Put("U_FID", UFid);
             ei.Put("JOB_NO",JobNo);
             ei.PutExpress("SEQ_NO", @"(SELECT 
   (CASE  WHEN MAX(SEQ_NO) IS NULL THEN 1 WHEN MAX(SEQ_NO) = '' THEN 1 ELSE MAX(SEQ_NO)+1 END)
     FROM TMEXP WHERE
   U_FID ='" + UFid + "')");
             ei.Put("DEP", Dep);//超过字符长度
             ei.Put("EXP_OBJ", ExpObj);
             ei.Put("EXP_CD",ExpCd);
             ei.Put("EXP_TYPE", ExpType);
             ei.Put("EXP_REASON", ExpReason);
             ei.Put("EXP_TEXT", ExpText);
             ei.Put("WR_ID",UserId);
             ei.PutDate("WR_DATE", DateTime.Now);;
             ei.Put("GROUP_ID", GroupId);
             ei.Put("CMP", CompanyId);
             ei.Put("CREATE_BY", UserId);
             ei.PutDate("CREATE_DATE", DateTime.Now);
             mixedlist.Add(ei); 

             if (mixedlist.Count > 0)
             {
                 try
                 {
                     int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                 }
                 catch (Exception ex)
                 {
                     returnMessage = ex.ToString();
                 }
             }
         }
         public Boolean DoInsrtErrMsg(string SeqNo, string UFid, string JobNo, string ExpObj, string ExpCd, string ExpType, string ExpReason, string ExpText, string Dep)
         {

             Boolean resType = false;
             if (SeqNo == null)
             {
                 
                 try
                 {
                     string checkSeqNo = " SELECT COUNT(U_ID) FROM TMEXP WHERE U_FID = " + SQLUtils.QuotedStr(UFid);
                     SeqNo = OperationUtils.GetValueAsString(checkSeqNo, Prolink.Web.WebContext.GetInstance().GetConnection());
                 }
                 catch (Exception ex)
                 {
                     resType = false;
                     return resType;
                 }
                 
             }

             
             string returnMessage = "success";
             string uid = string.Empty;
             MixedList mixedlist = new MixedList();
             uid = Guid.NewGuid().ToString();
             string sql = "INSERT INTO TMEXP (U_ID,U_FID,JOB_NO,SEQ_NO,DEP,EXP_OBJ,EXP_CD,EXP_TYPE,EXP_REASON,EXP_TEXT,WR_ID,WR_DATE,GROUP_ID,CMP,CREATE_BY,CREATE_DATE) VALUES"
                         + "(" + SQLUtils.QuotedStr(uid) + "," + SQLUtils.QuotedStr(UFid) + "," + SQLUtils.QuotedStr(JobNo) + "," + SQLUtils.QuotedStr(SeqNo) + "," + SQLUtils.QuotedStr(Dep) + "," + SQLUtils.QuotedStr(ExpObj) + "," + SQLUtils.QuotedStr(ExpCd) + "," + SQLUtils.QuotedStr(ExpType) + "," + SQLUtils.QuotedStr(ExpReason)
                         + "," + SQLUtils.QuotedStr(ExpText) + "," + SQLUtils.QuotedStr(UserId) + ",getdate()," + SQLUtils.QuotedStr(GroupId) + "," + SQLUtils.QuotedStr(CompanyId) + "," + SQLUtils.QuotedStr(UserId) + ",getdate())";
             mixedlist.Add(sql);

             if (mixedlist.Count > 0)
             {
                 try
                 {
                     int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                     resType = true;
                 }
                 catch (Exception ex)
                 {
                     returnMessage = ex.ToString();
                     resType = false;
                 }
             }
             return resType;
         }

         public ActionResult ErrGetSmptyData(string condition)
         {
             int recordsCount = 0, pageIndex = 0, pageSize = 0;
             condition += "STATUS=" + SQLUtils.QuotedStr("U");
             DataTable dt = ModelFactory.InquiryData("*", "SMPTY", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

             BootstrapResult result = null;
             result = new BootstrapResult()
             {
                 records = recordsCount,
                 page = pageIndex,
                 total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                 rows = ModelFactory.ToTableJson(dt)
             };

             return result.ToContent();
         }

         public class TemxpOptions
         {
             public List<OptionsItem> Ex = new List<OptionsItem>();
             public List<OptionsItem> Ex2 = new List<OptionsItem>();
         }

         public class OptionsItem
         {
             public string cd { get; set; }
             public string cdDescp { get; set; }
         }
    }
}
