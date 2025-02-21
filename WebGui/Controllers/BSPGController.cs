using Prolink;
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
using WebGui.Models;
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json;
using Prolink.Model;
using System.Text;
using Prolink.Persistence;
using Business;
using System.Globalization;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using System.Text.RegularExpressions;

namespace WebGui.Controllers
{
    public class BSPGController : BaseController
    {

        #region View
        public ActionResult EDIQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("MSG01");
            return View();
        }

        public ActionResult EDISetupView(string id = null, string uid = null)
        {
            SetSchema("EDI_TARGET");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("MSG01");
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult EDIDetialSetupView(string id = null, string uid = null)
        {
            SetSchema("EDI_LIST");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("MSG01");
            ViewBag.Uid = id;
            ViewBag.UFid = Prolink.Math.GetValueAsString(Request.Params["muid"]);
            return View();
        }

        public ActionResult EDILogQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("MSG01");
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult EdiQueryData()
        {
            string condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            condition = GetCreateDateCondition("EDI_TARGET", condition);
            return GetBootstrapData("EDI_TARGET", condition);
        }

        public ActionResult EdiLogQueryData()
        {
            string condition = string.Empty;
            if (UPri == "G")
            {
                condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            }
            else
            {
                condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            }
            condition = GetCreateDateCondition("EDI_LOG", condition,"EVENT_DATE");
            return GetBootstrapData("EDI_LOG", condition);
        }

        public ActionResult EdiLogDetail()
        {
            string uid = string.Empty;
            uid = Prolink.Math.GetValueAsString(Request["id"]);
            string content = string.Empty;
            if (!string.IsNullOrEmpty(uid))
            {
                string sql = string.Format("SELECT * FROM EDI_DATA WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
                DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (subDt != null && subDt.Rows.Count > 0)
                {
                    content = Prolink.Math.GetValueAsString(subDt.Rows[0]["EDI_DATE"]);
                    return Json(new { content = content });
                }
            }
            return Json(new { content = content });
        }
        #endregion

        #region 取得資料
        public ActionResult EdiGetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM EDI_TARGET WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM EDI_LIST WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EdiTargetModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "EdiListModel");

            return ToContent(data);
        }

        public ActionResult EdiDetailGetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM EDI_LIST WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EdiListModel");

            return ToContent(data);
        }

        public ActionResult getEdiList()
        {
            string UId = Request.Params["uid"];
            string sql = string.Format("SELECT * FROM EDI_LIST WHERE U_FID={0}", SQLUtils.QuotedStr(UId));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sub"] = ModelFactory.ToTableJson(subDt, "EdiListModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult EdiUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(UId))
                UId = HttpUtility.UrlDecode(UId);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "EdiTargetModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));

                            EditInstruct ei2 = new EditInstruct("EDI_LIST", EditInstruct.DELETE_OPERATION);
                            ei.PutKey("U_FID", UId);
                            mixList.Add(ei2);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "EdiListModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                            
                        mixList.Add(ei);
                    }
                }

            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            string sql = string.Format("SELECT * FROM EDI_TARGET WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM EDI_LIST WHERE U_FID={0}", SQLUtils.QuotedStr(UId));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EdiTargetModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "EdiListModel");
            return ToContent(data);
        }
        public ActionResult EdiDetailUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string UFid = Prolink.Math.GetValueAsString(Request.Params["u_fid"]);
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(UId))
                UId = HttpUtility.UrlDecode(UId);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "EdiListModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("U_FID", UFid);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
                }
                
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            string sql = string.Format("SELECT * FROM EDI_LIST WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EdiListModel");
            return ToContent(data);
        }
        #endregion

        #region 基本方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "EDI_TARGET":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM EDI_TARGET WHERE 1=0";
                        break;
                    case "EDI_LIST":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM EDI_LIST WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            if (resultType == "excel")
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

        private ActionResult GetBaseData(string table, string condition, string colNames = "*", string orderBy = "", string qType = "")
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 20;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            switch (qType)
            {
                case "1":
                    dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                case "2":
                    dt = ModelFactory.InquiryData("*", table, condition, orderBy, pageIndex, pageSize, ref  recordsCount);
                    break;
                case "3":
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                default:
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
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

        #endregion

        #region 检查唯一值
        public bool ChkKey(string Carrier, string EffectDate, string Area)
        {
            string sql = "SELECT COUNT(*) FROM SMFSC WHERE CARRIER=" + SQLUtils.QuotedStr(Carrier) + " AND EFFECT_DATE=" + SQLUtils.QuotedStr(EffectDate) + " AND AREA=" + SQLUtils.QuotedStr(Area);
            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (num > 0)
            {
                return false;
            }

            return true;
        }
        #endregion

    }
}
