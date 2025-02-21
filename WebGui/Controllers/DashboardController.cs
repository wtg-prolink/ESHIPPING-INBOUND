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
using System.Collections.Specialized;
using Business;
using Business.Mail;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using System.Web.Configuration;
using System.Text.RegularExpressions;

namespace WebGui.Controllers
{
    public class DashboardController : BaseController
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
                    case "AddBulletin":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM MOD_BULLETIN WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }
        public ActionResult AddBulletin(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("DHADDBULLETIN");
            SetSchema("AddBulletin");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.EdocUrl = WebConfigurationManager.AppSettings["EDOC_URL1"];
            ViewBag.Cmp = CompanyId;
            return View();
        }

        public ActionResult BulletinQueryView()
        {
            ViewBag.pmsList = GetBtnPms("DHADDBULLETIN");
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult Bulletin()
        {
            ViewBag.MenuBar = false;
            //获取数据库中的数据导入到ViewBag中
            string sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetBaseGroup() + " AND BULL_TYPE='1' ORDER BY BULL_DATE DESC";
            DataTable type1Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable type2Dt = new DataTable();
            DataTable type3Dt = new DataTable();
            DataTable type4Dt = new DataTable();
            DataTable type5Dt = new DataTable();

            //1.外部用户隐藏厂别，部门公告。 2.外部用户的用户别：L.外部物流业者，V.外部供应商 显示对应的公告。
            if ("I".Equals(this.IOFlag))
            {
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardCmp() + " AND BULL_TYPE='2' ORDER BY BULL_DATE DESC";
                type2Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardDep() + " AND BULL_TYPE='3' ORDER BY BULL_DATE DESC";
                type3Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardDep() + " AND BULL_TYPE='4' ORDER BY BULL_DATE DESC";
                type4Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardDep() + " AND BULL_TYPE='5' ORDER BY BULL_DATE DESC";
                type5Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            if ("L".Equals(this.UType))
            {
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetBaseGroup() + " AND BULL_TYPE='4' ORDER BY BULL_DATE DESC";
                type4Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else if ("V".Equals(this.UType))
            {
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetBaseGroup() + " AND BULL_TYPE='5' ORDER BY BULL_DATE DESC";
                type5Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            SetNew(ref type1Dt);
            SetNew(ref type2Dt);
            SetNew(ref type3Dt);
            SetNew(ref type4Dt);
            SetNew(ref type5Dt);
            ViewBag.BulleData = type1Dt;
            ViewBag.Bulle1Data = type2Dt;
            ViewBag.Bulle2Data = type3Dt;
            ViewBag.Bulle3Data = type4Dt;
            ViewBag.Bulle4Data = type5Dt;

            ViewBag.UType = this.UType;
            ViewBag.IOFlag = this.IOFlag;
            ViewBag.pmsList = GetBtnPms("DHBULLETIN");
            return View();
        }

        public void SetNew(ref DataTable dt)
        {
            if (!dt.Columns.Contains("IsNew"))
            {
                dt.Columns.Add("IsNew");
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DateTime date = Prolink.Math.GetValueAsDateTime(dt.Rows[i]["BULL_DATE"]);
                int n = (DateTime.Now - date).Days;

                if (0 <= n && n <= 7)
                {
                    dt.Rows[i]["IsNew"] = "NEW";
                }
            }

        }

        public ActionResult getBulletinDetail()
        {
            //BULL_TYPE: 1.集团2.厂别3.部门4.物流业者5.供应商公告
            string sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetBaseGroup() + " AND BULL_TYPE='1' ORDER BY BULL_DATE DESC";
            DataTable type1Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable type2Dt = new DataTable();
            DataTable type3Dt = new DataTable();
            DataTable type4Dt = new DataTable();
            DataTable type5Dt = new DataTable();

            // 1.外部用户隐藏厂别，部门公告。 2.外部用户的用户别：L.外部物流业者，V.外部供应商 显示对应的公告。
            if ("I".Equals(this.IOFlag))
            {
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardCmp() + " AND BULL_TYPE='2' ORDER BY BULL_DATE DESC";
                type2Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardDep() + " AND BULL_TYPE='3' ORDER BY BULL_DATE DESC";
                type3Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardDep() + " AND BULL_TYPE='4' ORDER BY BULL_DATE DESC";
                type4Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetDashboardDep() + " AND BULL_TYPE='5' ORDER BY BULL_DATE DESC";
                type5Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            if ("L".Equals(this.UType))
            {
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetBaseGroup() + " AND BULL_TYPE='4' ORDER BY BULL_DATE DESC";
                type4Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else if ("V".Equals(this.UType))
            {
                sql = "SELECT TOP(3) * FROM MOD_BULLETIN WHERE " + GetBaseGroup() + " AND BULL_TYPE='5' ORDER BY BULL_DATE DESC";
                type5Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            
            SetNew(ref type1Dt);
            SetNew(ref type2Dt);
            SetNew(ref type3Dt);
            SetNew(ref type4Dt);
            SetNew(ref type5Dt);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["BulleData"] = ModelFactory.ToTableJson(type1Dt, "ModBulletinModel");
            data["Bulle1Data"] = ModelFactory.ToTableJson(type2Dt, "ModBulletinModel");
            data["Bulle2Data"] = ModelFactory.ToTableJson(type3Dt, "ModBulletinModel");
            data["Bulle3Data"] = ModelFactory.ToTableJson(type4Dt, "ModBulletinModel");
            data["Bulle4Data"] = ModelFactory.ToTableJson(type5Dt, "ModBulletinModel");
            return ToContent(data);
        }

        public ActionResult AddBulletinInquiry()
        {
            //string model = Request["model"];
            string hasm = Request["hasm"];//是否包含实体定义
            JavaScriptSerializer js = new JavaScriptSerializer();
            string sql = ModelFactory.GetHeadSql("ModBulletinModel");

            sql = sql + " ORDER BY BULL_DATE DESC";
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("ModBulletinModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "ModBulletinModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult BulletinQueryData()
        {
            string condition = GetDashboardCmp();
            condition = GetCreateDateCondition("MOD_BULLETIN", condition);
            return GetBootstrapData("MOD_BULLETIN", condition);
        }

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM MOD_BULLETIN WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "ModBulletinModel");
            return ToContent(data);
        }
        #endregion

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*", string dnapprove = "", NameValueCollection namevaluecollection = null)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                string basecondtion = GetDecodeBase64ToString(Request.Params["basecondition"]);
                if (!string.IsNullOrEmpty(basecondtion))
                {
                    if (string.IsNullOrEmpty(condition))
                    {
                        condition = basecondtion;
                    }
                    else
                    {
                        condition += " AND " + basecondtion;
                    }
                }
                dt = GetStatusCountData(statusField, table, condition, Request.Params, dnapprove);
                pageSize = 1;
            }
            else
            {
                if (namevaluecollection == null) namevaluecollection = Request.Params;
                dt = ModelFactory.InquiryData(colNames, table, condition, namevaluecollection, ref recordsCount, ref pageIndex, ref pageSize);
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

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues, string dnapprove = "")
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";

            if (!string.IsNullOrEmpty(dnapprove))
            {
                if (dnapprove == "DNAPPROVE")
                {
                    string personsql = "SELECT 'Person' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE APPROVE_USER='" + UserId + "' UNION";
                    string localsql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " AND APPROVE_TO NOT IN('A') GROUP BY " + col + " UNION";
                    string asql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " AND APPROVE_TO='A' AND DEP='" + Dep + "' GROUP BY " + col;
                    sql = personsql + localsql + asql;
                }
            }

            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "  SELECT '' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
            DataTable dtsum = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dtAll = dt.Copy();
            foreach (DataRow dr in dtsum.Rows)
            {
                dtAll.ImportRow(dr);
            }

            return dtAll;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBulletinUpdate()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            string u_id = Request["u_id"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            string BullContent = Request["bullContent"];
            string CmpList = Request["Cmp"];
            string DepList = Request["Dep"];
            if (!string.IsNullOrEmpty(BullContent))
                BullContent = HttpUtility.UrlDecode(BullContent);
            if (!string.IsNullOrEmpty(CmpList))
                CmpList = HttpUtility.UrlDecode(CmpList);
            if (!string.IsNullOrEmpty(DepList))
                DepList = HttpUtility.UrlDecode(DepList);
            Boolean IsSuccess = true;
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixedlist = new MixedList();
            if (string.IsNullOrEmpty(CmpList))
                CmpList = CompanyId;
            if (string.IsNullOrEmpty(DepList))
                DepList = Dep;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ModBulletinModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("CMP", CmpList);
                            ei.Put("STN", Station);
                            ei.Put("DEP", DepList);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;                           
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            if (!string.IsNullOrEmpty(BullContent))
                                ei.Put("BULL_CONTENT", BullContent);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;                           
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                            if (!string.IsNullOrEmpty(BullContent))
                                ei.Put("BULL_CONTENT", BullContent);
                            ei.Put("DEP", DepList);
                            ei.Put("CMP", CmpList);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        /*else
                        {
                            ei.PutKey("BULL_ID", DateTime.Now.ToString("yyyymmddhhssmm"));
                        }*/
                        mixedlist.Add(ei);
                    }
                }
            }
            try
            {

                int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }

            string sql = string.Format("SELECT * FROM MOD_BULLETIN WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "ModBulletinModel");
            return ToContent(data);
        }

        public ActionResult MoreBulletin()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("DHBULLETIN");
            string type = Request["type"];
            string searchInfo = Request["searchInfo"];
            string condition = string.Empty;
            ViewBag.SearchInfo = searchInfo;
            ViewBag.BulleType = type;
            switch (type)
            {
                case "1":
                    ViewBag.BulleTitle = @Resources.Locale.L_Bulletin_Views_228;
                    condition = GetBaseGroup() + " AND BULL_TYPE='1'";
                    break;
                case "2":
                    ViewBag.BulleTitle = @Resources.Locale.L_Bulletin_Factory;
                    condition = GetDashboardCmp() + " AND BULL_TYPE='2'";
                    break;
                case "3":
                    ViewBag.BulleTitle = @Resources.Locale.L_Bulletin_Dep;
                    condition = GetDashboardDep() + " AND BULL_TYPE='3'";
                    break;
                case "4":
                    ViewBag.BulleTitle = @Resources.Locale.L_Bulletin_LSP;
                    condition = "I".Equals(this.IOFlag) ? string.Format(GetDashboardDep() + " AND BULL_TYPE='4'") : string.Format(GetBaseGroup() + " AND BULL_TYPE='4'");
                    break;
                case "5":
                    ViewBag.BulleTitle = @Resources.Locale.L_Bulletin_Vendor;
                    condition = "I".Equals(this.IOFlag) ? string.Format(GetDashboardDep() + " AND BULL_TYPE='5'") : string.Format(GetBaseGroup() + " AND BULL_TYPE='5'");
                    break;
                default:
                    ViewBag.BulleData = new DataTable();
                    return View();
            }
            if (!string.IsNullOrEmpty(searchInfo) && (type.Equals("1") || type.Equals("2") || type.Equals("3") || type.Equals("4") || type.Equals("5")))
                condition += string.Format(" AND BULL_TITLE LIKE {0}", SQLUtils.QuotedStr("%" + searchInfo + "%"));

            string columns = "*";
            string table = "MOD_BULLETIN";
            string orderBy = "BULL_DATE DESC";

            int beginRow = 0, endRow = 0, page = 1, limit = 20;
            string page_str = Request["page"];//第几页  从1开始
            string limit_str = Request["rows"];//每页大小
            if (!int.TryParse(page_str, out page)) page = 1;
            if (!int.TryParse(limit_str, out limit)) limit = 20;

            ViewBag.PrePage = page > 1 ? page - 1 : 0;
            ViewBag.NextPage = page >= 0 ? page + 1 : 0;

            beginRow = (page - 1) * limit;
            endRow = (page) * limit;

            InquiryInstruct ii = new InquiryInstruct(columns, table, orderBy);
            if (!string.IsNullOrEmpty(condition))
                ii.AddFilterBlock(new FilterBlock(condition));
            DataTable dt = OperationUtils.GetDataTable(ii, Prolink.Web.WebContext.GetInstance().GetConnection(), beginRow, endRow);

            if (!dt.Columns.Contains("REP_TITLE"))
            {
                dt.Columns.Add("REP_TITLE", typeof(string));
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string oldTitle = dt.Rows[i]["BULL_TITLE"].ToString();
                if (!string.IsNullOrEmpty(searchInfo))
                {
                    string replace = "<span style='background-color:yellow;'>" + searchInfo + "</span>";
                    string newTitle = Regex.Replace(oldTitle, searchInfo, replace, RegexOptions.IgnoreCase);
                    dt.Rows[i]["REP_TITLE"] = newTitle;
                }
                else
                {
                    dt.Rows[i]["REP_TITLE"] = oldTitle;
                }
            }

            ViewBag.BulleData = dt;
            return View();
        }
        public string GetDashboardDep()
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP LIKE '%{1}%' ) AND  ( DEP = '*' OR DEP LIKE '%;{2}%' OR DEP LIKE '{2}%' ) ", SQLUtils.QuotedStr(GroupId), CompanyId, Dep);
        }

        public string GetDashboardCmp()
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP LIKE '%{1}%' ) ", SQLUtils.QuotedStr(GroupId), CompanyId);
        }
    }
}
