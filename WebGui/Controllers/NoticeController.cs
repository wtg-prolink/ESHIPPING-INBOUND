using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;

namespace WebGui.Controllers
{
    public class NoticeController : BaseController
    {
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "AddEDM":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM EDM_TPLT WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        public ActionResult NoticeSetup()
        {
            string Id = Prolink.Math.GetValueAsString(Request.Params["Id"]);
            string sql = "SELECT * FROM SYS_NOTICE WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "Id" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.pmsList = GetBtnPms("NOTIFY");
            return View();

        }

        public ActionResult EDMSetup()
        {
            string Id = Prolink.Math.GetValueAsString(Request.Params["Id"]);
            string sql = "SELECT * FROM EDM_TPLT WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "Id" });
            ViewBag.schemas = ToContent(schemas);
            //ViewBag.pmsList = GetBtnPms("NOTIFY");
            return View();

        }

        public ActionResult AddEDM(string id = null, string uid = null)
        {
            //ViewBag.pmsList = GetBtnPms("EDM");
            SetSchema("AddEDM");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult GetEDMDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM EDM_TPLT WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EDMTModel");
            return ToContent(data);
        }

        public ActionResult EDMQueryView()
        {
            string Id = Prolink.Math.GetValueAsString(Request.Params["Id"]);
            string sql = "SELECT * FROM EDM_TPLT WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "Id" });
            ViewBag.schemas = ToContent(schemas);
            //ViewBag.pmsList = GetBtnPms("NOTIFY");
            return View();

        }

        public ActionResult EDMSetupInquiryData()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*,CREATE_DATE AS Cdate,MODIFY_DATE AS Mdate", "EDM_TPLT", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

            string resultType = Request.Params["resultType"];
            if (resultType == "excel")

                //return ExportExcelFile(dt, "test data...");
                return ExportExcelFile(dt);

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

        public ActionResult NoticeSetupInquiryData()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = GetCreateDateCondition("SYS_NOTICE", "");
            DataTable dt = ModelFactory.InquiryData("*,CREATE_DATE AS Cdate,MODIFY_DATE AS Mdate", "SYS_NOTICE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

            string resultType = Request.Params["resultType"];
            if (resultType == "excel")

                //return ExportExcelFile(dt, "test data...");
                return ExportExcelFile(dt);

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

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult NoticeSetUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            MixedList mixList = new MixedList();
            string uid = string.Empty;
            List<Dictionary<string, object>> noticeData = new List<Dictionary<string, object>>();
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);

            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "NoticeModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];                       
                      
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            //系统自动产生Uid
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
                            ei.Put("GROUP_ID", GroupId);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));                            
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {                   
                            string id = Prolink.Math.GetValueAsString(ei.Get("ID"));
                            string sysCode = Prolink.Math.GetValueAsString(ei.Get("SYS_CODE"));
                            string progCode = Prolink.Math.GetValueAsString(ei.Get("PROG_CODE"));
                            string groupId = Prolink.Math.GetValueAsString(ei.Get("GROUP_ID"));
                            string stn = Prolink.Math.GetValueAsString(ei.Get("STN"));
                            string cmp = Prolink.Math.GetValueAsString(ei.Get("CMP"));
                            ei.PutKey("ID", id);
                            ei.PutKey("SYS_CODE", sysCode);
                            ei.PutKey("PROG_CODE", progCode);
                            ei.PutKey("GROUP_ID", groupId);
                            ei.PutKey("STN", stn);
                            ei.PutKey("CMP", cmp);
                        }
                        mixList.Add(ei);
                    }
                    if (mixList.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                            DataTable dt = ModelFactory.InquiryData("*,CREATE_DATE AS Cdate,MODIFY_DATE AS Mdate", "SYS_NOTICE", "1=1", "CREATE_DATE", pageIndex, pageSize, ref recordsCount);
                            noticeData = ModelFactory.ToTableJson(dt, "NoticeModel");
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                        }
                    }
                }
            }
            return Json(new { message = returnMessage, noticeData = noticeData });
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EDMSetUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            MixedList mixList = new MixedList();
            string u_id = Request["u_id"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            Dictionary<string, object> edmData = new Dictionary<string, object>();
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);

            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "EDMTModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            //系统自动产生Uid
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            string id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            string sysCode = Prolink.Math.GetValueAsString(ei.Get("SYS_CODE"));
                            string progCode = Prolink.Math.GetValueAsString(ei.Get("PROG_CODE"));
                            string groupId = Prolink.Math.GetValueAsString(ei.Get("GROUP_ID"));
                            string stn = Prolink.Math.GetValueAsString(ei.Get("STN"));
                            string cmp = Prolink.Math.GetValueAsString(ei.Get("CMP"));
                            ei.PutKey("ID", id);
                            ei.PutKey("SYS_CODE", sysCode);
                            ei.PutKey("PROG_CODE", progCode);
                            ei.PutKey("GROUP_ID", groupId);
                            ei.PutKey("STN", stn);
                            ei.PutKey("CMP", cmp);
                        }
                        mixList.Add(ei);
                    }
                    if (mixList.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                            string sql = string.Format("SELECT * FROM EDM_TPLT WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
                            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            edmData["main"] = ModelFactory.ToTableJson(mainDt, "EDMModel");
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                        }
                    }
                }
            }

            return ToContent(edmData);
 
        }

        

        public JsonResult ChangeNotice()
        {
            string allFields = Request.Params["allFields"];
            string id = Prolink.Math.GetValueAsString(Request["id"]);
            string refNo = Prolink.Math.GetValueAsString(Request["refNo"]);
            string noticeDate = Prolink.Math.GetValueAsString(Request["noticeDate"]);
            string approveCode = Prolink.Math.GetValueAsString(Request["approveCode"]);
            string otherFields = Prolink.Math.GetValueAsString(Request["otherFields"]);
            string boss = Prolink.Math.GetValueAsString(Request["boss"]);
            string isDelete = Prolink.Math.GetValueAsString(Request["isDelete"]);
            string returnMessage = ChangeNoticeContent(id, refNo, allFields, noticeDate, approveCode, boss,isDelete, null);


            
            return Json(new { message = returnMessage });
        }

        //判断通知代碼是否重複
        public ActionResult CheckId()
        {
            string id = Prolink.Math.GetValueAsString(Request["id"]);
            int count = 0;
            string sql = "SELECT count(*) FROM SYS_NOTICE WHERE ID=" + SQLUtils.QuotedStr(id)  ;
            count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(count > 0 ? true : false);
        }


        public ActionResult GetTPLTDetail()
        {
            string uIds = Prolink.Math.GetValueAsString(Request["uIds"]);
            DataTable dt;
            string sql = "SELECT U_ID,TPLT_NAME,TPLT_CONTENT FROM EDM_TPLT WHERE U_ID in ('" + uIds.Replace(";", "','") + "')";
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            return Json(new { data = result.ToContent() });
        }
    }
}
