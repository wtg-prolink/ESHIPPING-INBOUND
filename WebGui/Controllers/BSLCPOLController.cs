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
    public class BSLCPOLController : BaseController
    {
        //
        // GET: /BSLCPOL/

        #region View
        public ActionResult BSLCPOLQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("QT014");
            return View();
        }

        public ActionResult BSLCPOLSetupView(string id = null, string uid = null)
        {
            SetSchema("BSLCPOLSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("QT014");
            ViewBag.Uid = id;
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult BSLCPOLQuery()
        {
            return GetBootstrapData("BSLCPOL", "");
        }
        #endregion

        #region 取得資料
        public JsonResult GetBslcpolDetail()
        {
            string u_id = Request["UId"];
            DataTable dt = new DataTable();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();

            string sql = "SELECT TOP 1 * FROM BSLCPOL WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            return Json(new { mainTable = result.ToContent() });
        }
        #endregion

        #region 保存
        public ActionResult BslcpolUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string Pol = Prolink.Math.GetValueAsString(Request.Params["Pol"]);
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BSLCPOLModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            bool chk = ChkBslcpol(Cmp, Pol);
                            if (chk == false)
                            {
                                return Json(new { message = @Resources.Locale.L_BSLCPOLController_Controllers_101 });
                            }
                            UId = System.Guid.NewGuid().ToString();
                            ei.PutKey("U_ID", UId);
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
                    string sql = string.Format("SELECT * FROM BSLCPOL WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    smexData = ModelFactory.ToTableJson(mainDt, "BSLCPOLModel");
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = smexData });
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
                    case "BSLCPOLSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM BSLCPOL WHERE 1=0";
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
        public bool ChkBslcpol(string Cmp, string Pol)
        {
            string sql = "SELECT COUNT(*) FROM BSLCPOL WHERE CMP="+SQLUtils.QuotedStr(Cmp) + " AND POL="+SQLUtils.QuotedStr(Pol);
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
