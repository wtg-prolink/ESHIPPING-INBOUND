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
    public class BscodeNewController : BaseController
    {
        //
        // GET: /IPCTM/
        #region old
        //public ActionResult BSCODESetup()
        //{
        //    ViewBag.pmsList = GetBtnPms("BSCODE");
        //    return View();
        //}

        public ActionResult CodeKindRequiry()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            DataTable dt = ModelFactory.InquiryData("*", "BSCODE_KIND", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BscodeKindModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult bscodeSortQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string CdType = Prolink.Math.GetValueAsString(Request.Params["CdType"]);
            string condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CD_TYPE=" + SQLUtils.QuotedStr(CdType);
            DataTable dt = ModelFactory.InquiryData("*", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BscodeKindModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        [ValidateInput(false)]
        public JsonResult BscodeRequiry()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            string cdType = Prolink.Math.GetValueAsString(Request.Params["CdType"]);
            string condtions = " WHERE CD_TYPE=" + SQLUtils.QuotedStr(cdType);
            condtions += " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            DataTable detailDt = ModelFactory.InquiryData("*", "BSCODE", condtions, " CD ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(detailDt)
            };
            return Json(new { mainTable = resultDetail.ToContent() });
        }
        [ValidateInput(false)]
        public JsonResult BsCodeUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            bool rebuildPms = false;
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
                if (item.Key == "st")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsCodeModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", BaseCompanyId);
                            ei.Put("STN", BaseStation);

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutKey("CD_TYPE", ei.Get("CD_TYPE"));
                            ei.PutKey("CD", ei.Get("CD"));
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", BaseCompanyId);
                            ei.PutKey("STN", BaseStation);
                        }
                        if (ei.Get("CD") != "" && ei.Get("CD_TYPE") != "")
                        {
                            mixList.Add(ei);
                        }

                        if (ei.Get("CD_TYPE") == "EDT")
                        {
                            rebuildPms = true;
                        }

                    }
                }
                else if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BscodeKindModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", BaseCompanyId);
                            ei.Put("STN", BaseStation);
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
                    if (rebuildPms)
                    {
                        Prolink.V3.PermissionManager.GetEdocPermission();
                        Business.CommonHelp.NotifyRebuidPermission(null, true);
                        //PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage });
        }
        #endregion

        #region View
        public ActionResult BscodeQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BscodeNew");
            return View();
        }

        public ActionResult BSCODESetup(string id = null, string uid = null,string cmp = null)
        {
            SetSchema("Bscode");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("BscodeNew");
            ViewBag.Uid = id;
            ViewBag.Cmp = cmp;
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string condition = GetDataPmsCondition("C");
            condition = GetCreateDateCondition("BSCODE_KIND", condition);
            return GetBootstrapData("BSCODE_KIND", condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM BSCODE_KIND WHERE CD_TYPE={0} AND (CMP = {1} OR CMP = '*')", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(CompanyId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM BSCODE WHERE CD_TYPE={0} AND " + GetDataPmsCondition() + " ORDER BY CD DESC", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BscodeKindModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "BsCodeModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            //return Json(new { message = "No valid Data!" });
            string CdType = Prolink.Math.GetValueAsString(Request.Params["CdType"]);
            string sql = string.Empty;
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(CdType))
                CdType = HttpUtility.UrlDecode(CdType);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BscodeKindModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        //Cmp = ei.Get("CMP");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", returncmp());
                            ei.Put("STN", "*");
                            ei.Put("DEP", "*");
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            CdType = Prolink.Math.GetValueAsString(ei.Get("CD_TYPE"));
                            CdType = CdType.Trim();
                            CdType = CdType.Replace(" ", "");
                            ei.Put("CD_TYPE", CdType);   
                            mixList.Add(ei);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            //sql = "UPDATE BSCODE_KIND SET CMP = '*' WHERE GROUP_ID = 'TPV' AND CMP = 'XM' AND CD_TYPE = {0}";
                            //sql = string.Format(sql, SQLUtils.QuotedStr(CdType));
                            //mixList.Add(sql);
                            //sql = "UPDATE BSCODE SET CMP = '*' WHERE GROUP_ID = 'TPV' AND CMP = 'XM' AND CD_TYPE = {0}";
                            //sql = string.Format(sql, SQLUtils.QuotedStr(CdType));
                            //mixList.Add(sql);
                            ei.PutKey("CD_TYPE", CdType);
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", Cmp);
                            //ei.PutKey("STN", "*");
                            //ei.PutKey("DEP", "*");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);

                            mixList.Add(ei);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("CD_TYPE", ei.Get("CD_TYPE"));
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", Cmp);
                            ei.PutKey("STN", "*");
                            ei.PutKey("DEP", "*");

                            if (CdType != "")
                            {
                                sql = "DELETE FROM ECCOSTD WHERE CD_TYPE={0} AND " + GetDataPmsCondition();
                                sql = string.Format(sql, SQLUtils.QuotedStr(CdType));
                                mixList.Add(sql);
                            }
                            mixList.Add(ei);
                        }
                        
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsCodeModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CD_TYPE", CdType);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", returncmp());
                            if ("EDT".Equals(CdType))
                                ei.Put("CMP", "*");
                            ei.Put("STN", "*");
                            ei.Put("DEP", "*");
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            string cmp = ei.Get("CMP");
                            ei.PutKey("CD_TYPE", CdType);
                            ei.PutKey("CD", ei.Get("CD"));
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", Cmp);
                            //ei.PutKey("STN", "*");
                            //ei.PutKey("DEP", "*");
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            string cmp = ei.Get("CMP");
                            ei.PutKey("CD_TYPE", CdType);
                            ei.PutKey("CD", ei.Get("CD"));
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", Cmp);
                            //ei.PutKey("STN", "*");
                            //ei.PutKey("DEP", "*");
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
                    if ("EDT".Equals(CdType))
                    {
                        Prolink.V3.PermissionManager.GetEdocPermission();
                        Business.CommonHelp.NotifyRebuidPermission(null, true);
                        //PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            sql = string.Format("SELECT * FROM BSCODE_KIND WHERE CD_TYPE={0} AND (CMP = {1} OR CMP = '*')", SQLUtils.QuotedStr(CdType), SQLUtils.QuotedStr(CompanyId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM BSCODE WHERE CD_TYPE={0} AND " + GetDataPmsCondition(), SQLUtils.QuotedStr(CdType));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BsdistModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "BsCodeModel");
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
                    case "Bscode":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM BSCODE WHERE 1=0";
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

        public string returncmp() {
            string Cmp = BaseCompanyId;
            string upri = this.UPri;
            if ("G".Equals(upri))
                Cmp = "*";
            return Cmp;
        }
    }
}
