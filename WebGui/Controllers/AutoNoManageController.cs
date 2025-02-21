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
    public class AutoNoManageController : BaseController
    {
        //
        // GET: /AutoNoManage/

        #region View
        public ActionResult AutoNoQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AUTONO");
            return View();
        }

        public ActionResult AutoNoSetupView()
        {
            SetSchema("AutoNo");
            string RuleCode = Prolink.Math.GetValueAsString(Request.Params["RuleCode"]);
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string Stn = Prolink.Math.GetValueAsString(Request.Params["Stn"]);
            ViewBag.pmsList = GetBtnPms("AUTONO");
            ViewBag.RuleCode = RuleCode;
            ViewBag.Cmp = Cmp;
            ViewBag.Stn = Stn;
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string condition = GetDataPmsCondition("C");
            condition = GetCreateDateCondition("SCS_AUTONO_RULE", condition);
            return GetBootstrapData("SCS_AUTONO_RULE", condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string RuleCode = Request["RuleCode"];
            string Cmp = Request["Cmp"];
            string Stn = Request["Stn"];

            string sql = string.Format("SELECT * FROM SCS_AUTONO_RULE WHERE RULE_CODE={0} AND GROUP_ID={1} AND CMP={2} AND STN={3}", SQLUtils.QuotedStr(RuleCode), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(Stn));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SCS_AUTONO_ITEM WHERE RULE_CODE={0} AND GROUP_ID={1} AND CMP={2} AND STN={3} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(RuleCode), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(Stn));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "AutoNoRuleModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "AutoNoItemModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string RuleCode = Request["RuleCode"];
            string Cmp = Request["Cmp"];
            string Stn = Request["Stn"];

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(RuleCode))
                RuleCode = HttpUtility.UrlDecode(RuleCode);
            if (!string.IsNullOrEmpty(Cmp))
                Cmp = HttpUtility.UrlDecode(Cmp);
            if (!string.IsNullOrEmpty(Stn))
                Stn = HttpUtility.UrlDecode(Stn);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "AutoNoRuleModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string[,] unikey = new string[4, 2] { { "RULE_CODE", RuleCode }, { "GROUP_ID", GroupId }, { "CMP", Cmp }, { "STN", Stn } };

                            if (chkKeyIsExist("SCS_AUTONO_RULE", unikey) == true)
                            {
                                return Json(new { message = Resources.Locale.M_CtAssignManage_Msg03 }); //訊息：資料重覆，無法保存
                            }

                            ei.Put("RULE_CODE", RuleCode);
                            ei.Put("GROUP_ID", GroupId);
                            ei.PutKey("STN", "*");
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("RULE_CODE", ei.Get("RULE_CODE"));
                            ei.PutKey("CMP", ei.Get("CMP"));
                            ei.PutKey("STN", "*");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);

                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("RULE_CODE", ei.Get("RULE_CODE"));
                            ei.PutKey("CMP", ei.Get("CMP"));
                            ei.PutKey("STN", "*");

                            string sql1 = "DELETE FROM SCS_AUTONO_ITEM WHERE RULE_CODE={0} AND CMP={1} AND STN={2}";
                            sql1 = string.Format(sql1, SQLUtils.QuotedStr(ei.Get("RULE_CODE")), SQLUtils.QuotedStr(ei.Get("CMP")), SQLUtils.QuotedStr(ei.Get("STN")));

                            mixList.Add(sql1);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "AutoNoItemModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("RULE_CODE", RuleCode);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", Cmp);
                            ei.Put("STN", "*");
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("RULE_CODE", RuleCode);
                            ei.PutKey("SEQ_NO", ei.Get("SEQ_NO"));
                            ei.PutKey("CMP", Cmp);
                            ei.PutKey("STN", "*");
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("RULE_CODE", ei.Get("REULE_CODE"));
                            ei.PutKey("SEQ_NO", ei.Get("SEQ_NO"));
                            ei.PutKey("CMP", ei.Get("CMP"));
                            ei.PutKey("STN", "*");
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

            string sql = string.Format("SELECT * FROM SCS_AUTONO_RULE WHERE RULE_CODE={0} AND GROUP_ID={1} AND CMP={2} AND STN={3}", SQLUtils.QuotedStr(RuleCode), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr("*"));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SCS_AUTONO_ITEM WHERE RULE_CODE={0} AND GROUP_ID={1} AND CMP={2} AND STN={3} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(RuleCode), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr("*"));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "AutoNoRuleModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "AutoNoItemModel");
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
                    case "AutoNo":
                        kyes = new List<string> { "RULE_CODE" };
                        sql = "SELECT * FROM SCS_AUTONO_RULE WHERE 1=0";
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

    }
}
