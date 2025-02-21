using Prolink;
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
using WebGui.Models;
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json;
using Prolink.Model;
using System.Text;
using Prolink.Persistence;
using Business;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebGui.Controllers
{
    public class BsrptManageController : BaseController
    {
        //
        // GET: /BsrptManage/

        #region View
        public ActionResult BsrptQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BSRPT");
            return View();
        }

        public ActionResult BsrptSetupView(string id = null, string uid = null)
        {
            SetSchema("Bsrpt");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("ReffeeManage");
            ViewBag.Uid = id;
            ViewBag.Menu = GetAllPermission();
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string condition = GetDataPmsCondition();
            condition = GetCreateDateCondition("BSRPT", condition);
            return GetBootstrapData("BSRPT", condition);
        }
        #endregion

        #region 保存
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string ViewId = Prolink.Math.GetValueAsString(Request.Params["ViewId"]);
            string ViewType = Prolink.Math.GetValueAsString(Request.Params["ViewType"]);

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsrptModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            string[,] unikey = new string[2, 2] { { "VIEW_ID", ViewId }, { "VIEW_TYPE", ViewType } };

                            if (chkKeyIsExist("BSRPT", unikey) == true)
                            {
                                return Json(new { message = Resources.Locale.M_CtAssignManage_Msg03 }); //訊息：資料重覆，無法保存
                            }

                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
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

            string sql = string.Format("SELECT * FROM BSRPT WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BsrptModel");
            return ToContent(data);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM BSRPT WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BsrptModel");
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
                    case "Bsrpt":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM BSRPT WHERE 1=0";
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

        #region Permisssion
        /// <summary>
        /// 获取全部的权限菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllPermission()
        {
            //Permission p = PermissionManager.GetFirstPermission();
            //List<Dictionary<string, object>> list = GetPermission(p);
            //return ToContent(list);
            Dictionary<string, object> level = null;
            List<Dictionary<string, object>> firstLevel = null;
            List<Dictionary<string, object>> secondLevel = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;
            string path = MenuManager.GetMenuPath(Prolink.Web.WebContext.GetInstance());
            string menuPath = System.IO.Path.Combine(path, "BIG5_MENU.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(menuPath);
            XmlNodeList xnl = xmlDoc.SelectNodes("/menu/menu");
            firstLevel = new List<Dictionary<string, object>>();
            foreach (XmlNode xn in xnl)
            {
                level = new Dictionary<string, object>();
                level["text"] = "";
                if (xn.FirstChild.Value.Split('@').Length > 0)
                    level["text"] = xn.FirstChild.Value.Split('@')[0].ToString().Replace("\r\n", "");
                level["href"] = "#";
                level["icon"] = "glyphicon glyphicon-folder-open";
                secondLevel = new List<Dictionary<string, object>>();
                XmlNodeList menus = xn.SelectNodes("menu");
                foreach (XmlNode menu in menus)
                {
                    item = new Dictionary<string, object>();
                    item["text"] = menu.FirstChild.Value;
                    item["href"] = GetObjectValue(menu.Attributes, "pmsId");
                    secondLevel.Add(item);
                }
                level["nodes"] = secondLevel;
                firstLevel.Add(level);
            }

            return ToContent(firstLevel);
        }

        public string GetObjectValue(XmlAttributeCollection nObject, string nKey)
        {
            if (nObject[nKey] == null)
            {
                return string.Empty;
            }
            else
            {
                return nObject[nKey].Value;
            }
        }
        #endregion

        #region get report
        public ActionResult getReportList()
        {
            string ViewId = Prolink.Math.GetValueAsString(Request.Params["ViewId"]);
            string sql = string.Empty;

            sql = "SELECT * FROM BSRPT WHERE VIEW_ID={0} AND " + GetDataPmsCondition("S");
            sql = string.Format(sql, SQLUtils.QuotedStr(ViewId));

            DataTable dt = getDataTableFromSql(sql);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(dt, "BsrptModel");
            return ToContent(data);
        }
        #endregion


    }
}
