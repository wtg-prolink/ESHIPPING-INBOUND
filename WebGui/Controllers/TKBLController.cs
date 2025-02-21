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
using TrackingEDI.Business;

namespace WebGui.Controllers
{
    public class TKBLController : BaseController
    {

        #region view
        public ActionResult CustomerStatus()
        {
            ViewBag.pmsList = GetBtnPms("SMGSTM");
            return View();
        }

        public ActionResult Status()
        {
            return View();
        }

        public ActionResult StatueMappingSetup()
        {
            SetSchema("StatueMappingSetup");
            return View();
        }

        public ActionResult ProcessStatus()
        {
            //string SELECT E.*,S.* FROM TKEVM E LEFT JOIN TKBLST 
            string even_no = Request["EvenNo"];
            string TranType = Request["TranType"];
            if (string.IsNullOrEmpty(TranType)) TranType = string.Empty;
            ViewBag.TranType = TranType;

            if (string.IsNullOrEmpty(even_no)) even_no = string.Empty;
            ViewBag.EvenNo = even_no;

            string blNo = Request["blNo"];
            if (string.IsNullOrEmpty(blNo)) blNo = string.Empty;
            ViewBag.BlNo = blNo;
            return View();
        }

        public ActionResult RouteSetup()
        {
            //string sql = "SELECT * FROM SYS_ACCT WHERE 1=0";
            //Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            //ViewBag.schemas = ToContent(schemas);
            SetTranModeSelect();
            SetTermSelect();
            return View();
        }

        /// <summary>
        /// 貨況通知檔
        /// id是路由参数  貌似只有这个能抓到    uid是search参数 真的没抓到过
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public ActionResult NRSSetup(string id = null, string uid = null)
        {
            SetSchema("NRSSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.NotifyGroup = Business.CommonHelp.getBscodeForSelect("TMG", GetDataPmsCondition("C"));
            return View();
        }

        /// <summary>
        /// 貨況通知管理查询画面
        /// </summary>
        /// <returns></returns>
        public ActionResult NRSDataQuery()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult EventSetup()
        {
            ViewBag.MenuBar = false;
            SetTranModeSelect();
            SetNotifyFormatSelect();
            return View();
        }

        public ActionResult MailGroupQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.MailGroup = GetBscodeSelect("TMG");
            return View();
        }

        public ActionResult MailGroupSetup()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            ViewBag.pmsList = GetBtnPms("TKB040");
            ViewBag.MailGroup = GetBscodeSelect("TMG");
            ViewBag.UId = uid;
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            if (!"I".Equals(ioflag))
            {
                if ("G".Equals(this.UPri))
                    ioflag = "I";
            }
            ViewBag.UPri = ioflag;
            return View();
        }

        public ActionResult MailGroupInquiry()
        {
            string table = "(SELECT U_ID,GROUP_ID,CMP,STN,DEP,CASE WHEN (NAME is null OR NAME='') THEN ' ' ELSE NAME END AS NAME,MAIL_ID,REMARK,CREATE_BY,CREATE_DATE,MODIFY_BY,MODIFY_DATE FROM TKPMG) TKPMG";

            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            if (!"I".Equals(ioflag))
            {
                if ("G".Equals(this.UPri))
                    ioflag = "I";
            }
            string condition = string.Format("GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            if (ioflag == "I")
                condition = string.Format("GROUP_ID={0}", SQLUtils.QuotedStr(GroupId));
            return GetBootstrapData(table, condition);
        }

        private string GetBscodeSelect(string Bscode)
        {
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE={0} AND {1} ", SQLUtils.QuotedStr(Bscode), string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP={1} )", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(BaseCompanyId)));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
            }
            return select;
        }

        public ActionResult PartyDocQuery()
        {
            SetTranModeSelect();
            SetTermSelect();
            ViewBag.MenuBar = false;
            return View();
        }

        /// <summary>
        /// 9)	TKB030 Party Document Setup  (電子文檔設定) 
        /// </summary>
        /// <returns></returns>
        public ActionResult PartyDocSetup(string id = null, string uid = null)
        {
            SetSchema("PartyDocSetup");
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            if (string.IsNullOrEmpty(Cmp))
            {
                Cmp = CompanyId;
            }
            SetTranModeSelect();
            SetTermSelect();
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.Cmp = Cmp;
            return View();
        }

        /// <summary>
        /// 货况查询
        /// </summary>
        /// <returns></returns>
        public ActionResult TKBLQuery()
        {
            ViewBag.pmsList = GetBtnPms("TKB010");
            SetTranModeSelect();
            SetCstatus();
            ViewBag.MenuBar = false;
            return View();
        }

        /// <summary>
        /// 货况代碼查询
        /// </summary>
        /// <returns></returns>
        public ActionResult TKQuery()
        { 
            ViewBag.MenuBar = false;
            return View();
        }

        /// <summary>
        /// 货况代碼建檔
        /// </summary>
        /// <returns></returns>
        public ActionResult TKSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM TKSTSCD WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "STS_CD"});
            ViewBag.schemas = ToContent(schemas);
            ViewBag.Uid = id;

            return View();
        }

        public ActionResult POManagementQuery()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult POManagementSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM SMPOM WHERE 1=1";
            Dictionary<string, Dictionary<string, object>> mtSchemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.mtSchemas = ToContent(mtSchemas);
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult POInventoryQuery()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult IFCallPo()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        #endregion

        #region 基础方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        /// <summary>
        /// 设置Schema
        /// </summary>
        /// <param name="name"></param>
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "StatueMappingSetup":
                        kyes = new List<string> { "CUST_CD", "STS_CD" };
                        sql = "SELECT * FROM TKSTMP WHERE 1=0";
                        break;
                    case "PartyDocSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM TKPDM WHERE 1=0";
                        break;
                    case "NRSSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM TKPEM WHERE 1=0";
                        break;
                    default:
                        return;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                dt = GetStatusCountData(statusField, table, condition, Request.Params);
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
        #endregion

        #region 设置下拉选项数据源
        public ActionResult GetSelects()
        {
            Dictionary<string, object> options = GetSelectsToJson(Request["type"], Request["location"], this.GroupId, this.BaseCompanyId, Request["dn"], Request["code"]);
            return Json(options); 
        }

        private void SetNotifyFormatSelect()
        {
            ViewBag.SelectNotifyFormat = "";
            ViewBag.DefaultNotifyFormat = "";

            #region Notify Format
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='MT' AND GROUP_ID={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            //A:有色采购;B:有色销售;C:冷链;D:日化;E:空白
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultNotifyFormat = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                select += ":" + Prolink.Math.GetValueAsString(dr["CD"]).Trim() + "." +Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
            }
            ViewBag.SelectNotifyFormat = select;
            #endregion
        }

        private void SetFreightTermSelect()
        {
            ViewBag.SelectFreightTerm = "";
            ViewBag.DefaultFreightTerm = "";

            #region Freight Term
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='BU' AND GROUP_ID={0} AND (CMP={1}  OR CMP='*')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            //A:有色采购;B:有色销售;C:冷链;D:日化;E:空白
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultFreightTerm = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                select += ":" +Prolink.Math.GetValueAsString(dr["CD"]).Trim()+"."+  Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
            }
            ViewBag.SelectFreightTerm = select;
            #endregion
        }

        private void SetTermSelect()
        {
            ViewBag.SelectTerm = "";
            ViewBag.DefaultTerm = "";

            #region Term
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TD' AND GROUP_ID={0}  AND ( CMP={1} OR CMP='*')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            //string sql = "SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TD'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            //A:有色采购;B:有色销售;C:冷链;D:日化;E:空白
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultTerm = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                select += ":" + Prolink.Math.GetValueAsString(dr["CD"]).Trim() + "." + Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
            }
            ViewBag.SelectTerm = select;
            #endregion
        }

        private void SetCstatus()
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TKLC' AND GROUP_ID={0} AND (CMP={1} OR CMP='*') ORDER BY ORDER_BY", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            //A:有色采购;B:有色销售;C:冷链;D:日化;E:空白
            string statusField=string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                item = new Dictionary<string, string>();
                item["id"] = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                item["label"] = Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                if (string.IsNullOrEmpty(statusField))
                    statusField = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                list.Add(item);
            }
            item = new Dictionary<string, string>();
            item["id"] = "";
            item["label"] = "All";
            list.Add(item);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ViewBag.Cstatuss = jss.Serialize(list);//@MvcHtmlString.Create(@ViewBag.JsonDateMenu)  @Html.Raw(@ViewBag.JsonDateMenu)
            ViewBag.statusField = statusField;
        }
        #endregion

        public static string GetSelectsToString(string code)
        {
            Dictionary<string, object> options = GetSelectsToJson(code);
            if (options == null)
                return "{}";
            return new JavaScriptSerializer().Serialize(options);
        }

        private static Dictionary<string, object> GetSelectsToJson(string code)
        {
            Dictionary<string, object> option = null;
            Dictionary<string, object> options = new Dictionary<string, object>();
            List<object> list = null;
            string cd, cdDescp, cdType;
            string AP_CD;
            if ("Railway".Equals(code))
                AP_CD = "R";
            else if ("Air".Equals(code))
                AP_CD = "A";
            else
                AP_CD = "F";

            string sql = string.Format("SELECT 'DELAY_SOLUTION' AS CD_TYPE_SOLUTION,CD_TYPE,CD,CD_DESCP,AR_CD FROM BSCODE WHERE CD_TYPE='DELY' AND AP_CD={0}", SQLUtils.QuotedStr(AP_CD));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]).Trim();
                option = new Dictionary<string, object>();
                option["cdDescp"] = cdDescp;
                option["cd"] = cd;
                if (!options.ContainsKey(cdType))
                    options[cdType] = new List<object>();
                list = options[cdType] as List<object>;
                list.Add(option);
            }
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                cdDescp = Prolink.Math.GetValueAsString(dr["AR_CD"]).Trim();
                cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE_SOLUTION"]).Trim();
                option = new Dictionary<string, object>();
                option["cdDescp"] = cdDescp;
                option["cd"] = cd;
                if (!options.ContainsKey(cdType))
                    options[cdType] = new List<object>();
                list = options[cdType] as List<object>;
                list.Add(option);
            }
            return options;
        }


        public static string GetSelectsToString(string type, string location, string groupId, string baseLocation, string dn, string code)
        {
            Dictionary<string, object> options = GetSelectsToJson(type, location, groupId, baseLocation, dn, code);
            if (options == null)
                return "{}";
            return new JavaScriptSerializer().Serialize(options);
        }

        private static Dictionary<string, object> GetSelectsToJson(string types, string location, string groupId, string baseLocation, string dn, string code)
        {
            types = HttpUtility.UrlDecode(types);
            location = HttpUtility.UrlDecode(location);
            location = Prolink.Math.GetValueAsString(location);
            groupId = string.IsNullOrEmpty(groupId) ? "TPV" : groupId;
            bool isContract = false;
            bool isDelay = false;
            switch (types)
            {
                case "ActSetup":
                    types = "'TDLT','DTT','VIA','RMK'";
                    break;
                case "PartyDocSetup":
                    types = "'BU','TNT','TD','VIA'";
                    break;
                case "Shipment":
                    types = "'TKLC','TNT','VIA','TMOD','UNLR'";
                    break;
                case "NRSSetup":
                    types = "'TCGT','TDTK','MT','VIA','TNT'";
                    break;
                case "DNDetailView":
                    types = "'TTRN','TDT','TCT','TDTK','TCGT','TNT','TVAK','PK','VIA','TMOD','UNLR','AP','AERE'";
                    break;
                case "FCLBooking":
                    types = "'TNT','TTRN','TDT','TCT','PK','VIA','TMOD','UNLR','AERE'";
                    isContract = true;
                    isDelay = true;
                    break;
                case "DTBooking":
                    types = "'TCGT','TDTK','TDT','TCT','PK','VIA','TNT','TMOD','UNLR'";
                    break;
                case "QT":
                    types = "'TD','TDTK','VIA'";
                    break;
                case "MailGroupSetup":
                    types = "'TMG'";
                    break;
                case "BSTPS":
                    types = "'TNT'";
                    break;
            }
            //string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE {0} AND CD_TYPE IN({1})", "1=1", types);
            //string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE {0} AND CD_TYPE IN({1})", string.Format("GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId)), types);

            //modify by dean  问题由Yvonne提出，解决外代取的选项不是当前操作厂。
            //string sql = string.Format("SELECT * FROM BSCODE WHERE {0} AND CD_TYPE IN({1})", string.Format("GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId)), types);
            string sql = string.Format("SELECT * FROM BSCODE WHERE {0} AND CD_TYPE IN({1}) ORDER BY CD_TYPE,ORDER_BY ASC", string.Format("GROUP_ID={0} AND  (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(location == "" ? baseLocation : location)), types);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            code = HttpUtility.UrlDecode(code);
            dn = Prolink.Math.GetValueAsString(dn);
            if (dn == "" || !HttpUtility.UrlDecode(dn).Equals("Y"))
            {
                try
                {
                    switch (code)
                    {
                        case "Railway":
                            dt = dt.Select("CD_TYPE<>'VIA'  OR  (CD_TYPE='VIA'  AND  AR_CD ='Railway')").CopyToDataTable();
                            break;
                        default:
                            dt = dt.Select("CD_TYPE<>'VIA'  OR  (CD_TYPE='VIA'  AND  ( AR_CD IS  NULL OR AR_CD <> 'Railway'))").CopyToDataTable();
                            break;
                    }
                }
                catch
                {
                    dt = dt.Clone();
                }
            }

            Dictionary<string, object> option = null;
            Dictionary<string, object> options = new Dictionary<string, object>();
            List<object> list = null;
            string cd, cdDescp, cdType;
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]).Trim();
                option = new Dictionary<string, object>();
                if (cd.Equals(cdDescp) || cdType.Equals("AP") || cdType.Equals("RMK"))
                    option["cdDescp"] = cdDescp;
                else
                {
                    option["cdDescp"] = cd + ":" + cdDescp;
                }
                option["cd"] = cd;
                if (!options.ContainsKey(cdType))
                    options[cdType] = new List<object>();
                list = options[cdType] as List<object>;
                list.Add(option);
            }

            if (isContract)
            {
                sql = string.Format("SELECT CASE WHEN TRAN_TYPE='R' THEN 'RCN' WHEN TRAN_TYPE='A' THEN 'ACN' WHEN TRAN_TYPE='F' THEN 'FCN' ELSE 'OCN' END CD_TYPE,CD,CD_DESCP FROM SMCRAT");
                dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in dt.Rows)
                {
                    cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                    cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                    cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]).Trim();
                    option = new Dictionary<string, object>();
                    option["cdDescp"] = cd + ":" + cdDescp;
                    option["cd"] = cd;
                    if (!options.ContainsKey(cdType))
                        options[cdType] = new List<object>();
                    list = options[cdType] as List<object>;
                    list.Add(option);
                }
            }
            if (isDelay)
            {
                string AP_CD;
                if ("Railway".Equals(code))
                    AP_CD = "R";
                else if ("Air".Equals(code))
                    AP_CD = "A";
                else
                    AP_CD = "F";

                sql = string.Format("SELECT 'DELAY_SOLUTION' AS CD_TYPE_SOLUTION,CD_TYPE,CD,CD_DESCP,AR_CD FROM BSCODE WHERE CD_TYPE='DELY' AND AP_CD={0}", SQLUtils.QuotedStr(AP_CD));
                dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in dt.Rows)
                {
                    cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                    cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                    cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]).Trim();
                    option = new Dictionary<string, object>();
                    option["cdDescp"] = cdDescp;
                    option["cd"] = cd;
                    if (!options.ContainsKey(cdType))
                        options[cdType] = new List<object>();
                    list = options[cdType] as List<object>;
                    list.Add(option);
                }
                foreach (DataRow dr in dt.Rows)
                {
                    cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                    cdDescp = Prolink.Math.GetValueAsString(dr["AR_CD"]).Trim();
                    cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE_SOLUTION"]).Trim();
                    option = new Dictionary<string, object>();
                    option["cdDescp"] = cdDescp;
                    option["cd"] = cd;
                    if (!options.ContainsKey(cdType))
                        options[cdType] = new List<object>();
                    list = options[cdType] as List<object>;
                    list.Add(option);
                }
            }
            return options;
        }

        #region 貨況通知操作
        /// <summary>
        /// 保存货况通知数据
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveNotifyData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = Request["uid"];
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "PartyEvenModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString("N");
                            ei.Put("U_ID", u_id);
                            string request_cd = ei.Get("REQUEST_CD");
                            ei.Remove("SEQ_NO");
                            if (string.IsNullOrEmpty(request_cd))
                                ei.Put("REQUEST_CD", string.Empty);
                            ei.Put("GROUP_ID", GroupId);
                            string cmp = ei.Get("CMP");
                            if (string.IsNullOrEmpty(cmp))
                                cmp = CompanyId;
                            ei.Put("CMP", cmp);
                            ei.Put("STN", Station);
                            ei.Put("DEP", "*");
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                            sql = string.Format("SELECT * FROM TKPEM WHERE GROUP_ID={0} AND CMP={1} AND PARTY_TYPE={2} AND NOTIFY_CD={3}"
                               , SQLUtils.QuotedStr(GroupId)
                               , SQLUtils.QuotedStr(cmp)
                               , SQLUtils.QuotedStr(ei.Get("PARTY_TYPE"))
                               , SQLUtils.QuotedStr(ei.Get("NOTIFY_CD")));

                            if (string.IsNullOrEmpty(request_cd))
                                sql += " AND (REQUEST_CD='' OR REQUEST_CD IS NULL) ";
                            else
                                sql += " AND REQUEST_CD=" + SQLUtils.QuotedStr(request_cd);
                            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (dt.Rows.Count > 0)
                                return Json(new { message = @Resources.Locale.L_TKBLController_Controllers_218 });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Remove("SEQ_NO");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                            ei.AddKey("U_ID");
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

            sql = string.Format("SELECT * FROM TKPEM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(groupDt, "PartyEvenModel"));
        }

        /// <summary>
        /// 获取单笔货况通知记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNotifyItem()
        {
            string u_id = Request["uId"];
            string seqNo = Request["seqNo"];
            string group_id = Request["groupId"];
            string cmp = Request["cmp"];
            string stn = Request["stn"];
            string party_type = Request["partyType"];
            string notify_cd = Request["notifyCd"];
            string requestCd = Request["requestCd"];
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(u_id))
                sql = string.Format("SELECT * FROM TKPEM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            else
            {
                sql = string.Format("SELECT * FROM TKPEM WHERE GROUP_ID={0} AND CMP={1} AND PARTY_TYPE={2} AND NOTIFY_CD={3} AND STN={4} AND SEQ_NO={5}"
                   , SQLUtils.QuotedStr(group_id)
                   , SQLUtils.QuotedStr(cmp)
                   , SQLUtils.QuotedStr(party_type)
                   , SQLUtils.QuotedStr(notify_cd)
                   , SQLUtils.QuotedStr(stn)
                   , SQLUtils.QuotedStr(seqNo));

                if (string.IsNullOrEmpty(requestCd))
                    sql += " AND (REQUEST_CD='' OR REQUEST_CD IS NULL) ";
                else
                    sql += " AND REQUEST_CD=" + SQLUtils.QuotedStr(requestCd);
            }

            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(groupDt, "PartyEvenModel"));
        }

        /// <summary>
        /// 分页获取货况通知数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNotifyData()
        {
            string condition = GetCreateDateCondition("TKPEM", GetBaseGroup());
            return GetBootstrapData("TKPEM", condition);
        }

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues)
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);
            if (!string.IsNullOrEmpty(condition))
                condition = " WHERE " + condition;
            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            int count =Prolink.Math.GetValueAsInt( dt.Compute("Sum(COUNT)", ""));
            DataRow dr= dt.NewRow();
            dr["PO_STATUS"] = "";
            dr["COUNT"] = count;
            dt.Rows.Add(dr);
            return dt;
        }
        #endregion

        #region  路线规划操作
        public ActionResult GetRoutGroupData()
        {
            string table = "(SELECT * FROM (SELECT GROUP_ID,CMP FROM TKRUM WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " GROUP BY GROUP_ID,CMP) S OUTER APPLY (SELECT TOP 1 PARTY_NAME AS NAME FROM SMPTY PT WITH (NOLOCK) WHERE PT.PARTY_NO = S.CMP AND PT.GROUP_ID = S.GROUP_ID) B) MD";
            return GetBootstrapData(table, "1=1");
        }

        public ActionResult GetRoutData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "TKRUM", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public JsonResult GetSubRoutData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            string uid = Prolink.Math.GetValueAsString(Request.Params["uid"]);
            string condtions = string.Format(" WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable detailDt = ModelFactory.InquiryData("*", "TKRUD", condtions, "", pageIndex, pageSize, ref recordsCount);
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

        public JsonResult SaveRoutData()
        {
            string changeData = Request.Params["changedData"];
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "RouteModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("STN", "*");
                            ei.Put("DEP", "*");
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            EditInstruct ei2 = new EditInstruct("TKRUD", EditInstruct.DELETE_OPERATION);
                            ei2.PutKey("U_ID", Prolink.Math.GetValueAsString(ei.Get("U_ID")));
                            mixList.Add(ei2);
                        }

                        if (!string.IsNullOrEmpty(ei.Get("U_ID")))
                        {
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SubRouteModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
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
                }
            }
            return Json(new { message = returnMessage });
        }
        #endregion

        #region  Party Document Setup(電子文檔設定)操作
        public ActionResult GetPDocItem()
        {
            string u_id = Request["UId"];
            string cmp = Request["Cmp"];
            string sql = string.Format("SELECT * FROM TKPDM WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(cmp));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM TKPDD WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(cmp));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "PDocModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "PDocSubModel");
            return ToContent(data);
        }

        public ActionResult GetPDocData()
        {
            string condition = GetCreateDateCondition("TKPDM", GetBaseCmp());
            return GetBootstrapData("TKPDM", condition);
        }

        public ActionResult GetSubPDocData()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = string.Format("SELECT * FROM TKPDD WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "PDocSubModel"));
        }

        public ActionResult SavePDocData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = Request["UId"];
            string cmp = Request["Cmp"];
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "PDocModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString("N");
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            //ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", "*");
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                            //数据验证
                            //sql = string.Format("SELECT * FROM TKPDM WHERE GROUP_ID={0} AND CMP={1} AND TRAN_MODE={2} AND TERM={3} AND FREIGHT_TERM={4} AND PARTY_TYPE={5} AND STS_CD={6}"
                            //  , SQLUtils.QuotedStr(GroupId)
                            //  , SQLUtils.QuotedStr(CompanyId)
                            //  , SQLUtils.QuotedStr(ei.Get("TRAN_MODE"))
                            //  , SQLUtils.QuotedStr(ei.Get("TERM"))
                            //  , SQLUtils.QuotedStr(ei.Get("FREIGHT_TERM"))
                            //  , SQLUtils.QuotedStr(ei.Get("PARTY_TYPE"))
                            //  , SQLUtils.QuotedStr(ei.Get("STS_CD")));

                            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //if (dt.Rows.Count > 0)
                            //    return Json(new { message = "已存在该组数据" });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.AddKey("CMP");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.AddKey("CMP");
                        }
                        mixList.Add(ei);

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei = new EditInstruct("TKPDD", EditInstruct.DELETE_OPERATION);
                            ei.PutKey("U_ID", u_id);
                            ei.PutKey("CMP",cmp);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "PDocSubModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", u_id);
                            ei.Put("CMP", cmp);
                            ei.Put("GROUP_ID", GroupId);
                        }
                        string test_id = ei.Get("U_ID");
                        if (string.IsNullOrEmpty(test_id) || test_id.Length < 30)
                            continue;
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

            sql = string.Format("SELECT * FROM TKPDM WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(cmp));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM TKPDD WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(cmp));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "PDocModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "PDocSubModel");
            return ToContent(data);
        }
        #endregion

        #region  货况操作
        public ActionResult GetCargoItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //sql = string.Format("SELECT * FROM TKPDD WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            //DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BillofLadingModel");
            return ToContent(data);
        }

        public ActionResult GetStatus()
        {
            string u_id = Request["UId"];
            string shipment_id = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM TKBL WHERE U_ID=" + SQLUtils.QuotedStr(u_id));
            //string sql = string.Format("SELECT distinct * FROM (SELECT * FROM TKBLST WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMSM WHERE U_ID={0}) UNION SELECT * FROM TKBLST WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMSM WHERE U_ID={0}))T ORDER BY EVEN_DATE DESC,STS_CD DESC", SQLUtils.QuotedStr(u_id));
            string sql = string.Format("SELECT distinct * FROM TKBLST WHERE SHIPMENT_ID={0} ORDER BY EVEN_DATE DESC,STS_CD DESC", SQLUtils.QuotedStr(shipment_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            #region 数据改造
            mainDt.Columns.Add("EVEN_TMG1", typeof(string));
            foreach (DataRow dr in mainDt.Rows)
            {
                if (dr["EVEN_TMG"] == null || dr["EVEN_TMG"] == DBNull.Value)
                    continue;
                dr["EVEN_TMG1"] = ((DateTimeOffset)dr["EVEN_TMG"]).ToString("yyyy-MM-dd HH:mm:ss zzz");
            }
            mainDt.Columns.Remove("EVEN_TMG");
            mainDt.Columns.Add("EVEN_TMG", typeof(string));
            foreach (DataRow dr in mainDt.Rows)
            {
                dr["EVEN_TMG"] = dr["EVEN_TMG1"];
            }
            #endregion

            return ToContent(ModelFactory.ToTableJson(mainDt, "StatusModel"));
        }

        public ActionResult GetContainer()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM TKBLCNTR WHERE JOB_NO={0} ORDER BY INGATE,SEQ_NO", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "ContainerModel"));
        }

         public ActionResult GetTrackingStatus()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM (SELECT M.STS_CD, M.STS_DESCP,M.EVEN_DATE,M.EVEN_TMG,L.EDESCP,L.LDESCP,L.LOCATION AS STS_LOCATION FROM TKBLST M WITH (NOLOCK) OUTER APPLY (SELECT TOP 1 STS_CD,LOCATION,LDESCP,EDESCP FROM TKSTSCD WHERE LOCATION IS NOT NULL AND TKSTSCD.STS_CD=M.STS_CD) AS L WHERE U_ID={0})T WHERE (STS_LOCATION IS NOT NULL AND STS_LOCATION<>'') ORDER BY EVEN_DATE,STS_LOCATION", SQLUtils.QuotedStr(u_id));

            //sql = string.Format("SELECT * FROM (SELECT STS_CD,LOCATION,LDESCP,EDESCP FROM TKSTSCD WITH (NOLOCK) WHERE LOCATION IS NOT NULL AND LOCATION<>'') M OUTER APPLY (SELECT EVEN_DATE,EVEN_TMG,STS_DESCP FROM TKBLST WHERE U_ID={0} AND TKBLST.STS_CD=M.STS_CD) T  ORDER BY LOCATION,EVEN_DATE", SQLUtils.QuotedStr(u_id));

            sql = string.Format("SELECT STS_CD,EVEN_DATE,EVEN_TMG,STS_DESCP, (CASE STS_CD WHEN 'S10' THEN 'A' WHEN 'S11' THEN 'B' WHEN 'S12' THEN 'C' WHEN 'S13' THEN 'D' WHEN 'S14' THEN 'E' ELSE '"+@Resources.Locale.L_TKB_Controllers_l494+"' END) LOCATION  FROM TKBLST WHERE U_ID={0} ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            #region 数据改造
            mainDt.Columns.Add("EVEN_TMG1", typeof(string));
            foreach (DataRow dr in mainDt.Rows)
            {
                if (dr["EVEN_TMG"] == null || dr["EVEN_TMG"] == DBNull.Value)
                    continue;
                dr["EVEN_TMG1"] = ((DateTimeOffset)dr["EVEN_TMG"]).ToString("yyyy-MM-dd HH:mm:ss zzz");
            }
            mainDt.Columns.Remove("EVEN_TMG");
            mainDt.Columns.Add("EVEN_TMG", typeof(string));
            foreach (DataRow dr in mainDt.Rows)
            {
                dr["EVEN_TMG"] = dr["EVEN_TMG1"];
            }
            #endregion

            return ToContent(ModelFactory.ToTableJson(mainDt, "StatusModel"));
        }

         public string GetCustName(string type,string name,string table)
         {
             string cust = string.Format(" OUTER APPLY (SELECT TOP 1 SP.PARTY_NAME AS {0}_NAME,SP.PARTY_NO AS {0}_CODE FROM TKBLPT SP WITH (NOLOCK) WHERE  SP.U_ID = S.U_ID AND SP.PARTY_TYPE = '{1}') AS {2}", name, type, table);
            return cust;
         }
         public ActionResult GetCargoData()
         {
             //string table = "(SELECT * FROM TKBL S WITH (NOLOCK)" + GetCustName("CN", "CONSIGNEE", "C0") + GetCustName("CA", "CARRIER", "C1") + GetCustName("SP", "SHIPPER", "C2") + ") M";
             //string condition=GetBaseGroup()+ string.Format(" AND EXISTS (SELECT 1 FROM TKBLPT WITH (NOLOCK) WHERE TKBLPT.U_ID=M.U_ID AND  TKBLPT.PARTY_NO={0})",SQLUtils.QuotedStr(this.CompanyId));
             //string table = "(SELECT * FROM (SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM TKBLPT WITH (NOLOCK) WHERE TKBLPT.U_ID=TKBL.U_ID AND TKBLPT.PARTY_NO={0})", SQLUtils.QuotedStr(this.CompanyId)) + ") S" + GetCustName("CN", "CONSIGNEE", "C0") + GetCustName("CA", "CARRIER", "C1") + GetCustName("SP", "SHIPPER", "C2");
             //table += " UNION SELECT * FROM (SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" +GetCustName("CN", "CONSIGNEE", "C0") + GetCustName("CA", "CARRIER", "C1") + GetCustName("SP", "SHIPPER", "C2") +") M";

             //string table = "(SELECT * FROM (SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM TKBLPT WITH (NOLOCK) WHERE TKBLPT.U_ID=TKBL.U_ID AND TKBLPT.PARTY_NO={0})", SQLUtils.QuotedStr(this.CompanyId)) + ") S" + GetCustName("CS", "CONSIGNEE", "C0") + GetCustName("CA", "CARRIER", "C1") + GetCustName("SH", "SHIPPER", "C2");
             //table += " UNION SELECT * FROM (SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" + GetCustName("CS", "CONSIGNEE", "C0") + GetCustName("CA", "CARRIER", "C1") + GetCustName("SH", "SHIPPER", "C2") + ") M";
            string table = "(SELECT * FROM (SELECT TKBL.*,(SELECT TOP 1 POST_FLAG_DATE FROM SMSM WHERE SMSM.SHIPMENT_ID=TKBL.SHIPMENT_ID)AS POST_FLAG_DATE FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM TKBLPT WITH (NOLOCK) WHERE TKBLPT.U_ID=TKBL.U_ID AND TKBLPT.PARTY_NO={0})", SQLUtils.QuotedStr(this.CompanyId)) + ") S";
            table += " UNION SELECT TKBL.*,(SELECT TOP 1 POST_FLAG_DATE FROM SMSM WHERE SMSM.SHIPMENT_ID=TKBL.SHIPMENT_ID)AS POST_FLAG_DATE FROM TKBL WITH (NOLOCK) WHERE";
            if (UPri == "G")
                table += " GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
            else
                table += " GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId);
            table += ") M";
            string condition = GetCreateDateCondition("TKBL", "");
            return GetBootstrapData(table, condition);
         }

        public ActionResult SaveCargoData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = string.Empty;
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            Dictionary<string, object> data = new Dictionary<string, object>();
            return ToContent(data);
        }
        #endregion

        #region mail group 操作
        public ActionResult GetMailGroupData()
        {
            string table = "(SELECT * FROM (SELECT GROUP_ID,CMP FROM TKPMG WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " GROUP BY GROUP_ID,CMP) S OUTER APPLY (SELECT TOP 1 PARTY_NAME AS NAME FROM SMPTY PT WITH (NOLOCK) WHERE PT.PARTY_NO = S.CMP AND PT.GROUP_ID = S.GROUP_ID) B) MD";
            return GetBootstrapData(table, "1=1");
        }

        public ActionResult GetMailGroup()
        {
            string cmp = Request["cmp"];
            string sql = string.Format("SELECT U_ID,GROUP_ID,CMP,STN,DEP,CASE WHEN (NAME is null OR NAME='') THEN ' ' ELSE NAME END AS NAME,MAIL_ID,REMARK  FROM TKPMG WHERE {0} AND CMP={1}", GetBaseGroup(), SQLUtils.QuotedStr(cmp));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "PGroupMailModel"));
        }


        public ActionResult GetMailGroupItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM TKPMG WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "PGroupMailModel");
            return ToContent(data);
        }

        public ActionResult SaveMailGroupData()
        {
            string changeData = Request.Params["changedData"];
            string uid = Request.Params["uid"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Dictionary<string, object>> userData = new List<Dictionary<string, object>>();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "PGroupMailModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            uid = Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", TrackingEDI.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId));
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", TrackingEDI.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId));
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (string.IsNullOrEmpty(ei.Get("U_ID")))
                                continue;
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, UId = uid });
        }
        #endregion

        #region 事件管理
        public ActionResult GetEvenRecords()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM TKEVD WHERE EVEN_NO={0} ORDER BY PROCESS_DATE DESC", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "EvenRecordModel"));
        }

        public ActionResult GetEvenData()
        {
            return GetBootstrapData("TKEVM", GetBaseGroup());
        }

        public ActionResult RestEven()
        {
            string u_id = Request["UId"];
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["msg"] = EvenNotify.RestEven(u_id) > 0 ? @Resources.Locale.L_ActManage_Controllers_75 : @Resources.Locale.L_Api_Controllers_100;
            return ToContent(data);
        }

        #endregion

        #region 提单操作
        public ActionResult GetShipmentItemData()
        {
            string u_id = Request["UId"];
            string sql = "SELECT SHIPMENT_ID FROM TKBL WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            string ShipmentId = getOneValueAsStringFromSql(sql);

            sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BillofLadingModel");

            sql = string.Format("SELECT * FROM TKBLPT WHERE U_ID={0} ORDER BY ORDER_BY", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            data["sub"] = ModelFactory.ToTableJson(subDt, "BlPartyModel");


            sql = string.Format("SELECT * FROM TKBLCNTR WHERE JOB_NO={0} ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
            subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            data["con"] = ModelFactory.ToTableJson(subDt, "ContainerModel");

            sql = string.Format("SELECT D.* FROM TKBLST D, TKBL M WHERE M.U_ID=D.U_ID AND M.SHIPMENT_ID={0} ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(ShipmentId));
            subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            #region 数据改造
            subDt.Columns.Add("EVEN_TMG1", typeof(string));
            foreach (DataRow dr in subDt.Rows)
            {
                if (dr["EVEN_TMG"] == null || dr["EVEN_TMG"] == DBNull.Value)
                    continue;
                dr["EVEN_TMG1"] = ((DateTimeOffset)dr["EVEN_TMG"]).ToString("yyyy-MM-dd HH:mm:ss zzz");
            }
            subDt.Columns.Remove("EVEN_TMG");
            subDt.Columns.Add("EVEN_TMG", typeof(string));
            foreach (DataRow dr in subDt.Rows)
            {
                dr["EVEN_TMG"] = dr["EVEN_TMG1"];
            }
            #endregion
            data["status"] = ModelFactory.ToTableJson(subDt, "StatusModel");
            return ToContent(data);
        }
        #endregion

        #region 貨況建檔操作
        public ActionResult GetTkItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM TKSTSCD WHERE STS_CD={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmModel");
            return ToContent(data);
        }
        #endregion


        #region 要求货况时回填数据
        public ActionResult GetProcessStatusData()
        {
            //FQE201601141445034055
            string even_no = Request["EvenNo"];
            string blNo = Request["blNo"];
            if (!string.IsNullOrEmpty(even_no))
                return GetProcessStatusDataByEvenno(even_no);
            else
                return GetProcessStatusDataByBl(blNo);
        }

        private ActionResult GetProcessStatusDataByBl(string blNo)
        {
            DataTable blDt = new DataTable(), statusDt = new DataTable();
            statusDt.Columns.Add("STS_CD", typeof(string));
            statusDt.Columns.Add("STS_DESCP", typeof(string));
            statusDt.Columns.Add("LOCATION", typeof(string));
            statusDt.Columns.Add("Remark", typeof(string));
            statusDt.Columns.Add("EVEN_DATE", typeof(DateTime));
            //string party_type = Prolink.Math.GetValueAsString(dt.Rows[0]["PARTY_TYPE"]);
            string sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(blNo));
            blDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            blDt.Columns.Add("Cnt", typeof(string));
            blDt.Columns["Cnt"].MaxLength = 200;

            sql = string.Format("SELECT 1 FROM SMGSTM WHERE GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //sql = string.Format("SELECT STS_CD,STS_DESCP,UPLOAD_OP,VIEW_OP FROM SMGSTD WHERE GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            DataTable smgstdDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            bool allStatus = false;
            foreach (DataRow dr in blDt.Rows)
            {
                string cnt = string.Empty;
                decimal val = Prolink.Math.GetValueAsDecimal(dr["CNT20"]);
                if (val > 0)
                    cnt += string.Format("20' X {0};", val);

                val = Prolink.Math.GetValueAsDecimal(dr["CNT40"]);
                if (val > 0)
                    cnt += string.Format("40' X {0};", val);

                val = Prolink.Math.GetValueAsDecimal(dr["CNT40HQ"]);
                if (val > 0)
                    cnt += string.Format("40HQ X {0};", val);

                dr["Cnt"] = cnt;
            }
            //string group_id = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            //string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            //sql = string.Format("SELECT STS_CD,STS_DESCP FROM SMGSTD WHERE GROUP_ID={0} AND CMP={1} AND UPLOAD_OP='Y' AND NOT EXISTS(SELECT 1 FROM TKBLST WHERE TKBLST.STS_CD=SMGSTD.STS_CD AND U_ID={2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(blNo));
            if (smgstdDt.Rows.Count > 0)
                sql = string.Format("SELECT STS_CD,STS_DESCP FROM SMGSTD WHERE U_FID IN (SELECT U_ID FROM SMGSTM WHERE GROUP_ID={0} AND CMP={1} AND PARTY_TYPE IN (SELECT PARTY_TYPE FROM TKBLPT WHERE U_ID={2})) AND GROUP_ID={0} AND CMP={1} AND UPLOAD_OP='Y' AND NOT EXISTS(SELECT 1 FROM TKBLST WHERE TKBLST.STS_CD=SMGSTD.STS_CD AND U_ID={2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(blNo));
            else
            {
                sql = "SELECT STS_CD,STS_DESCP FROM SMGSTD WHERE 0=1";
                allStatus = true;
            }

            DataTable tempDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> testList = new List<string>();
            foreach (DataRow dr in tempDt.Rows)
            {
                string sts_cd = Prolink.Math.GetValueAsString(dr["STS_CD"]);
                if (string.IsNullOrEmpty(sts_cd))
                    continue;
                if (testList.Contains(sts_cd))
                    continue;
                testList.Add(sts_cd);
                DataRow st = statusDt.NewRow();
                st["STS_CD"] = sts_cd;
                st["STS_DESCP"] = dr["STS_DESCP"];
                statusDt.Rows.Add(st);
            }

            //sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} AND EXISTS(SELECT 1 FROM SMGSTD WHERE SMGSTD.STS_CD=TKBLST.STS_CD AND SMGSTD.GROUP_ID={1} AND SMGSTD.CMP={2} AND (VIEW_OP='Y' OR UPLOAD_OP='Y'))", SQLUtils.QuotedStr(blNo), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            if (smgstdDt.Rows.Count > 0)
                sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} AND EXISTS(SELECT 1 FROM SMGSTD WHERE SMGSTD.U_FID IN (SELECT U_ID FROM SMGSTM WHERE GROUP_ID={1} AND CMP={2} AND PARTY_TYPE IN (SELECT PARTY_TYPE FROM TKBLPT WHERE U_ID={0})) AND SMGSTD.STS_CD=TKBLST.STS_CD AND SMGSTD.GROUP_ID={1} AND SMGSTD.CMP={2} AND (VIEW_OP='Y' OR UPLOAD_OP='Y')) ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(blNo), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            else
                sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(blNo), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));

            //sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0}", SQLUtils.QuotedStr(blNo));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(blDt, "BillofLadingModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "StatusModel");
            data["status"] = ModelFactory.ToTableJson(statusDt, "StatusModel");
            data["allStatus"] = allStatus;
            return ToContent(data);
        }

        private ActionResult GetProcessStatusDataByEvenno(string even_no)
        {
            string sql = string.Format("SELECT * FROM TKEVM WHERE EVEN_NO={0}", SQLUtils.QuotedStr(even_no));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable blDt = new DataTable(), statusDt = new DataTable();
            DataTable subDt = new DataTable();
            statusDt.Columns.Add("STS_CD", typeof(string));
            statusDt.Columns.Add("STS_DESCP", typeof(string));
            statusDt.Columns.Add("LOCATION", typeof(string));
            statusDt.Columns.Add("Remark", typeof(string));
            statusDt.Columns.Add("EVEN_DATE", typeof(DateTime));
            if (dt.Rows.Count > 0)
            {
                sql = string.Format("SELECT STS_CD,STS_DESCP,UPLOAD_OP,VIEW_OP FROM SMGSTD WHERE GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
                DataTable smgstdDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string blNo = Prolink.Math.GetValueAsString(dt.Rows[0]["BL_NO"]);
                string party_type = Prolink.Math.GetValueAsString(dt.Rows[0]["PARTY_TYPE"]);
                //sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0}", SQLUtils.QuotedStr(blNo));
                //sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} AND EXISTS(SELECT 1 FROM SMGSTD WHERE SMGSTD.STS_CD=TKBLST.STS_CD AND SMGSTD.GROUP_ID={1} AND SMGSTD.CMP={2} AND (VIEW_OP='Y' OR UPLOAD_OP='Y'))", SQLUtils.QuotedStr(blNo), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
                if (smgstdDt.Rows.Count > 0)
                    sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} AND EXISTS(SELECT 1 FROM SMGSTD WHERE SMGSTD.U_FID IN (SELECT U_ID FROM SMGSTM WHERE GROUP_ID={1} AND CMP={2} AND PARTY_TYPE IN (SELECT PARTY_TYPE FROM TKBLPT WHERE U_ID={0})) AND SMGSTD.STS_CD=TKBLST.STS_CD AND SMGSTD.GROUP_ID={1} AND SMGSTD.CMP={2} AND (VIEW_OP='Y' OR UPLOAD_OP='Y')) ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(blNo), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
                else
                    sql = string.Format("SELECT * FROM TKBLST WHERE U_ID={0} ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(blNo), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));

                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(blNo));

                blDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                blDt.Columns.Add("Cnt", typeof(string));
                blDt.Columns["Cnt"].MaxLength = 200;
                foreach (DataRow dr in blDt.Rows)
                {
                    string cnt = string.Empty;
                    decimal val = Prolink.Math.GetValueAsDecimal(dr["CNT20"]);
                    if (val > 0)
                        cnt += string.Format("20' X {0};", val);

                    val = Prolink.Math.GetValueAsDecimal(dr["CNT40"]);
                    if (val > 0)
                        cnt += string.Format("40' X {0};", val);

                    val = Prolink.Math.GetValueAsDecimal(dr["CNT40HQ"]);
                    if (val > 0)
                        cnt += string.Format("40HQ X {0};", val);

                    dr["Cnt"] = cnt;
                }
                string group_id = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
                string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);


                sql = string.Format("SELECT REQUEST_CD,REQUEST_DESCP,GROUP_ID,CMP FROM TKPEM WHERE PARTY_TYPE={0} AND REQUEST_CD IS NOT NULL AND GROUP_ID={1} AND CMP={2} AND NOT EXISTS (SELECT 1 FROM TKBLST WHERE U_ID={3} AND STS_CD=TKPEM.REQUEST_CD)", SQLUtils.QuotedStr(party_type), SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(blNo));
                DataTable subDt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> testList = new List<string>();
                string sts_cd = string.Empty;
                foreach (DataRow dr in subDt1.Rows)
                {
                    sts_cd = Prolink.Math.GetValueAsString(dr["REQUEST_CD"]);
                    if (string.IsNullOrEmpty(sts_cd))
                        continue;
                    if (testList.Contains(sts_cd))
                        continue;
                    testList.Add(sts_cd);
                    DataRow st = statusDt.NewRow();
                    st["STS_CD"] = sts_cd;
                    st["STS_DESCP"] = dr["REQUEST_DESCP"];
                    statusDt.Rows.Add(st);
                }
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(blDt, "BillofLadingModel");
            data["status"] = ModelFactory.ToTableJson(statusDt, "StatusModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "StatusModel");
            return ToContent(data);
        }

        public ActionResult SaveProcessStatus()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = Request["u_id"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            Dictionary<string, string> statuss=new Dictionary<string,string>();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "StatusModel");
                    string ShipmentId = Prolink.Math.GetValueAsString(Request["ShipmentId"]);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        ei.OperationType = EditInstruct.INSERT_OPERATION;
                        //if (string.IsNullOrEmpty(ei.Get("LOCATION")))
                        //    continue;
                        if (string.IsNullOrEmpty(ei.Get("STS_CD")))
                            continue;
                        if (string.IsNullOrEmpty(ei.Get("EVEN_DATE")))
                            continue;
                        ei.Put("U_ID", u_id);
                        ei.Put("SHIPMENT_ID", ShipmentId);
                        ei.Put("SEQ_NO", System.Guid.NewGuid().ToString());
                        ei.Put("CREATE_BY", UserId);
                        ei.PutDate("CREATE_DATE", DateTime.Now);
                        statuss[ei.Get("SEQ_NO")]=ei.Get("STS_CD");
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    EvenManager.AddStatusChangeEven(u_id, statuss);
                    //EvenFactory.RegisterEvenTask(EvenType.E_SD.ToString(), EvenManager.SendDoc);
                    //EvenFactory.RegisterEvenTask(EvenType.E_SC.ToString(), EvenManager.ChangeStatus);
                    //string[] evenTypes = EvenFactory.GetEvenType();
                    //foreach (string et in evenTypes)
                    //{
                    //    EvenFactory.ExecuteEven(et);
                    //}
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            string even_no = Request["EvenNo"];
            if (!string.IsNullOrEmpty(even_no))
                return GetProcessStatusDataByEvenno(even_no);
            else
                return GetProcessStatusDataByBl(u_id);
        }
        #endregion

        #region 货况映射
        public ActionResult SaveStsMappingData()
        {
            string changeData = Request.Params["changedData"];
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
            string cust =Request["Cmp"];
            string custNm = Request["CmpNm"];
            if (!string.IsNullOrEmpty(cust)) cust = HttpUtility.UrlDecode(cust);
            if (!string.IsNullOrEmpty(custNm)) custNm = HttpUtility.UrlDecode(custNm);
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "TkstmpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CUST_CD", cust);
                            ei.Put("CUST_NM", custNm);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }

                        if (ei.OperationType != EditInstruct.DELETE_OPERATION)
                        {
                            string csts_cd = ei.Get("CSTS_CD");
                            string csts_descp = ei.Get("CSTS_DESCP");
                            if (!string.IsNullOrEmpty(csts_cd))
                            {
                                csts_cd = csts_cd.Trim();
                                ei.Put("CSTS_CD", csts_cd);
                            }

                            if (!string.IsNullOrEmpty(csts_descp))
                            {
                                csts_descp = csts_descp.Trim();
                                ei.Put("CSTS_DESCP", csts_descp);
                            }
                        }
                        System.Guid test_id = System.Guid.NewGuid();
                        if (!System.Guid.TryParse(ei.Get("U_ID"), out test_id))
                            continue;
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
                    return Json(new { message = returnMessage });
                }
            }
            return GetStsMappingItemByCmp(cust);
        }

        public ActionResult GetStsMappingItem()
        {
            string cust = Request["Cmp"];
            return GetStsMappingItemByCmp(cust);
        }

        private ActionResult GetStsMappingItemByCmp(string cust)
        {
            string sql = string.Format("SELECT * FROM TKSTMP WHERE CUST_CD={0} AND {1} ORDER BY SEQ_NO", SQLUtils.QuotedStr(cust), GetBaseGroup());
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "TkstmpModel"));
        }

        public ActionResult GetStsMappingGroupData()
        {
            string table = "(SELECT * FROM (SELECT GROUP_ID,CUST_CD AS CMP FROM TKSTMP WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " GROUP BY GROUP_ID,CUST_CD) S OUTER APPLY (SELECT TOP 1 PARTY_NAME AS NAME FROM SMPTY PT WITH (NOLOCK) WHERE PT.PARTY_NO = S.CMP AND PT.GROUP_ID = S.GROUP_ID) B) MD";
            return GetBootstrapData(table, "1=1");
        }
        #endregion



        #region PO Management
        //
        public ActionResult SmpomQueryData()
        {
            //string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            
            string condition = string.Format("CMP={0} AND GROUP_ID={1}", SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(GroupId));

            if (IOFlag == "O")
            {
                condition = "(BUYER_CD = " + SQLUtils.QuotedStr(CompanyId) + " OR  SUPPLIER_CD=" + SQLUtils.QuotedStr(CompanyId) + ")";
            }

            return GetBootstrapData("SMPOM", condition);
        }

        public ActionResult SmpodQueryData()
        {
            
            return GetBootstrapData("SMPOD", "");
        }
     
        public ActionResult GetSMPOMDataItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMPOM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            string sql2 = string.Format("SELECT * FROM SMPOD WHERE U_FID={0} ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string sql3 = string.Format("SELECT HOUSE_NO,MASTER_NO,CARRIER_CD,SCAC_CD,VESSEL1,VOYAGE1,POR_CD,POL_CD,POD_CD,DEST_CD,TKBL.ETD,TKBL.ETA FROM TKBL,TKBLPO,SMPOM where SMPOM.U_ID={0} AND SMPOM.PO_NO = TKBLPO.PO_NO AND SMPOM.Buyer_CD=TKBL.CS_CD", SQLUtils.QuotedStr(u_id));
            DataTable sub2Dt = OperationUtils.GetDataTable(sql3, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmpomModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmpodModel");
            data["sub2"] = ModelFactory.ToTableJson(sub2Dt, "BillofLadingModel");
            //Dictionary<string, object> data = GetPoDatas(u_id, mainDt);
            return ToContent(data);
        }

        

        public ActionResult SmpomUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = Request["u_id"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            string sql2 = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMPOMModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                            ////数据验证
                            //sql = string.Format("SELECT * FROM SMRQM WHERE RFQ_NO={0} "
                            //  , SQLUtils.QuotedStr(ei.Get("RFQ_NO")));

                            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //if (dt.Rows.Count > 0)
                            //    return Json(new { message = "已存在该组数据" });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            
                            ei.AddKey("U_ID");                                
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);

                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMPODModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {

                            ei.AddKey("U_ID");                            
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else
                            ei.AddKey("U_ID");

                        mixList.Add(ei);
                    }
                }
                
            }
            //Dictionary<string, object> data = new Dictionary<string, object>();
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
            sql = string.Format("SELECT * FROM SMPOM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            
            sql2 = string.Format("SELECT * FROM SMPOD WHERE U_FID={0} ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmpomModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmpodModel");
            
            return ToContent(data);
        }

        public JsonResult SetInvalid()
        {
            string returnmessage = "success";
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            EditInstruct ei = new EditInstruct("SMPOM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("STATUS", "I");
            ei.Put("MODIFY_BY", UserId);
            ei.PutDate("MODIFY_DATE", DateTime.Now);
            int[] result = OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (result == null)
            {
                returnmessage = "update errow";
            }
            return Json(new { message = returnmessage });
        }



        #endregion

        #region 客户货况设定
        public ActionResult SaveCustomerStatus()
        {
            string returnMessage = "success";
            string cmp = Request["cmp"];
            if (cmp != null)
                cmp = System.Web.HttpUtility.UrlDecode(cmp);

            #region 获取修改数据
            string changeData = Request.Params["changedData"];
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            #endregion

            #region 创建MixedList
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                ArrayList objList = item.Value as ArrayList;
                MixedList list = null;
                switch (item.Key)
                {
                    case "mt":
                        list = ModelFactory.JsonToEditMixedList(objList, "SmgstmModel");
                        for (int i = 0; i < list.Count; i++)
                        {
                            EditInstruct ei = (EditInstruct)list[i];
                            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                            {
                                //ei.Put("U_ID", Guid.NewGuid().ToString());
                                ei.Put("GROUP_ID", GroupId);
                                ei.Put("CMP", cmp);
                                ei.Put("CREATE_BY", UserId);
                                ei.PutDate("CREATE_DATE", DateTime.Now);
                            }
                            if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                            {
                                ei.Put("MODIFY_BY", UserId);
                                ei.PutDate("MODIFY_DATE", DateTime.Now);
                            }
                            if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                            {
                                EditInstruct ei2 = new EditInstruct("SMGSTD", EditInstruct.DELETE_OPERATION);
                                string UId = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                                if (string.IsNullOrEmpty(UId) || UId.Length < 20)
                                {
                                    continue;
                                }
                                ei2.PutKey("U_FID", UId);
                                mixList.Add(ei2);
                            }
                            mixList.Add(ei);
                        }
                        break;
                    case "sub":
                        list = ModelFactory.JsonToEditMixedList(objList, "SmgstdModel");
                        for (int i = 0; i < list.Count; i++)
                        {
                            EditInstruct ei = (EditInstruct)list[i];
                            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                            {
                                ei.Put("U_ID", Guid.NewGuid().ToString());
                                ei.Put("GROUP_ID", GroupId);
                                ei.Put("CMP", cmp);
                            }

                            Func<string, string> setValue=name =>
                            {
                                string val = ei.Get(name);
                                if (!string.IsNullOrEmpty(val))
                                {
                                    val = val.Trim();
                                    if (val.Length > 1)
                                    {
                                        val = val.ToUpper().Substring(0, 1);
                                        ei.Put(name, val);
                                    }
                                }
                                return val;
                            };

                            setValue("VIEW_OP");
                            setValue("UPLOAD_OP");
                            mixList.Add(ei);
                        }
                        break;
                }
            }
            #endregion

            #region 执行数据变更
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            #endregion

            return Json(new { message = returnMessage });
        }

        public ActionResult GetCustomerStatus()
        {
            string cmp = Request["cmp"];
            string condition = string.Format("CMP={0} AND GROUP_ID={1}", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(GroupId));
            return GetBootstrapData("SMGSTM", condition);
        }

        public ActionResult GetSubCustomerStatus()
        {
            string id = Request["uid"];
            string condition = string.Format("U_FID={0}", SQLUtils.QuotedStr(id));
            return GetBootstrapData("SMGSTD", condition);
        }

        public ActionResult ToTracking()
        {
            string id = Request["uid"];
            BookingParser bp = new BookingParser();
            bp.SaveToTracking(id);
            return ToContent(@Resources.Locale.L_TKB_Controllers_l502);
        }
        #endregion

        public ActionResult SmbidQueryData()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string subSql = "";
            if (virCondition != "")
            {
                subSql = " AND SHIPMENT_ID IN ( SELECT SHIPMENT_ID FROM SMICNTR WHERE 1=1 AND " + virCondition + " )";
            }

            string smbidColumn = string.Format(@"SMBID.U_ID,SMBID.CMP,SMBID.TRAN_TYPE,SMBID.APPROVE_STATUS,SMBID.STATUS,SMBID.FSTATUS,SMBID.CHECK_DESCP,SMBID.UNAPPROVE_DESCP,
                    SMBID.DEBIT_DATE,SMBID.SHIPMENT_ID,SMBID.DEBIT_NO,SMBID.DEBIT_TO,SMBID.DEBIT_NM,SMBID.LSP_NO,SMBID.LSP_NM,SMBID.BL_NO,(CASE WHEN SMBID.RFQ_NO='undefined' THEN NULL ELSE SMBID.RFQ_NO END) RFQ_NO,
                    SMBID.QUOT_NO,SMBID.CHG_CD,SMBID.CHG_DESCP,SMBID.CHG_TYPE,SMBID.QCUR,SMBID.QCHG_UNIT,SMBID.QUNIT_PRICE,SMBID.QQTY,SMBID.QAMT,SMBID.QEX_RATE,SMBID.QLAMT,SMBID.QTAX,SMBID.CUR,SMBID.UNIT_PRICE,
                    SMBID.CHG_UNIT,SMBID.QTY,SMBID.BAMT,SMBID.EX_RATE,SMBID.LAMT,SMBID.TAX,SMBID.BI_REMARK,SMBID.REMARK,SMBID.IPART_NO,SMBID.CNTR_STD_QTY,SMBID.COST_CENTER,SMBID.PROFIT_CENTER,
                    SMBID.MASTER_NO,SMBID.CNTR_INFO,SMBID.POD_CD,SMBID.SEC_CMP,SMBID.DEC_NO,SMBID.INVOICE_INFO,SMBID.CREATE_DATE,SMBID.UPLOAD_USER,SMBID.UPLOAD_TIME,SMBID.APPROVE_DATE");

            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string table = string.Format(@"(SELECT {2},'' AS OP,(SELECT TOP 1 POL_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
                (SELECT TOP 1 SCAC_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID AND SMSM.TRAN_TYPE='E') AS SCAC_CD ,
                (SELECT TOP 1 HOUSE_NO FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS HOUSE_NO ,
                (SELECT TOP 1 ATA FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS ATA,
          (SELECT TOP 1 STATUS
          FROM SMSM WITH(NOLOCK)
          WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS  ,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.U_ID=SMBID.U_FID WHERE SMBID.GROUP_ID={0} AND SMBID.LSP_NO={1} )T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), smbidColumn);
            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                if ("G".Equals(upri))
                    innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition = string.Format("SMBID.GROUP_ID={0} AND (SMBID.CMP={1} OR SMBID.SEC_CMP={1})", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));
                table = string.Format(@"(SELECT {3},'' AS OP,(SELECT TOP 1 POL_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
                (SELECT TOP 1 SCAC_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID AND SMSM.TRAN_TYPE='E') AS SCAC_CD ,
                (SELECT TOP 1 HOUSE_NO FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS HOUSE_NO ,
                (SELECT TOP 1 ATA FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS ATA,
                (SELECT TOP 1 STATUS FROM SMSM WITH(NOLOCK) WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS,SMBIM.TPV_DEBIT_NO FROM SMBID
                LEFT JOIN SMBIM  ON SMBIM.U_ID=SMBID.U_FID WHERE {0}
        UNION SELECT {3},'' AS OP,(SELECT TOP 1 POL_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD,
                (SELECT TOP 1 SCAC_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID AND SMSM.TRAN_TYPE='E') AS SCAC_CD ,
                (SELECT TOP 1 HOUSE_NO FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS HOUSE_NO ,
                (SELECT TOP 1 ATA FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS ATA,
                (SELECT TOP 1 STATUS FROM SMSM WITH(NOLOCK) WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS,SMBIM.TPV_DEBIT_NO FROM SMBID 
                LEFT JOIN SMBIM  ON SMBIM.U_ID=SMBID.U_FID WHERE SMBID.GROUP_ID={1} AND SMBID.LSP_NO={2})T", 
                innerCondition, SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), smbidColumn);
            }
            string condition = "1=1";
            condition = GetCreateDateCondition("SMBID", condition);
            return GetBootstrapData(table, condition + subSql);
        }

        /// <summary>
        /// 查询请款金额不为空的数据
        /// </summary>
        /// <returns></returns>
        public ActionResult SmbidQueryData1()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string table = string.Format(@"(SELECT SMBID.*,'' AS OP,(SELECT TOP 1 POL_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
          (SELECT TOP 1 STATUS
          FROM SMSM WITH(NOLOCK)
         WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS ,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.DEBIT_NO=SMBID.DEBIT_NO WHERE SMBID.GROUP_ID={0} AND SMBID.LSP_NO={1})T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));

            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                if ("G".Equals(upri))
                    innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition =string.Format( "SMBID.GROUP_ID={0} AND (SMBID.CMP={1} OR SMBID.SEC_CMP={1})",SQLUtils.QuotedStr(this.GroupId),SQLUtils.QuotedStr(this.CompanyId));
                table = string.Format(@"(SELECT SMBID.*,'' AS OP,(SELECT TOP 1 POL_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
          (SELECT TOP 1 STATUS
          FROM SMSM WITH(NOLOCK)
         WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS ,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.DEBIT_NO=SMBID.DEBIT_NO  WHERE {0})T", innerCondition);//AND SMBID.LSP_NO={2}
            }
            string condition = "BAMT IS NOT NULL AND BAMT<>0 AND U_FID IS NULL";
            condition = GetCreateDateCondition("SMBID", condition);
            //string location = this.CompanyId;
            //if (!string.IsNullOrEmpty(location))
            //    condition += string.Format(" AND CMP={0}", SQLUtils.QuotedStr(location));
            return GetBootstrapData(table, condition);
        }

        public ActionResult SendProlinkTrace()
        {
            string id = Request["uid"];
            if (string.IsNullOrEmpty(id))
                return Json(new { message = @Resources.Locale.L_DNManage_PleSelcData });
            string msg = TrackingEDI.Utils.TraceStatusHelper.SendIport(id);
            return Json(new { message = msg });
            //return Json(new { message = "已发起货况请求" });
        }

        [HttpPost]
        public ActionResult BatchUploadBLStatus(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    mapping = BookingStatusManager.TKBatchStatusMapping;
                    
                    MixedList ml = new MixedList();
                    MixedList partyml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    parm["mixedlist"] = partyml;
                    ExcelParser.RegisterEditInstructFunc(mapping, BookingStatusManager.HandleTKBLStatus);
                    ExcelParser ep = new ExcelParser();
                    parm.Add("UserId", UserId);
                    ep.Save(mapping, excelFileName, ml, parm);
                    for (int i = 0; i < partyml.Count; i++)
                    {
                        ml.Add((EditInstruct)partyml[i]);
                    }
                    if (ml.Count <= 0)
                    {
                        returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                    }
                    if (ml.Count > 0)
                    {
                        try
                        {
                            //int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                            for (int i = 0; i < ml.Count; i++)
                            {
                                EditInstruct ei = (EditInstruct)ml[i];
                                TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status
                                {
                                    ShipmentId = ei.Get("SHIPMENT_ID").ToString(),
                                    StsCd = ei.Get("STS_CD").ToString(),
                                    Sender = UserId,
                                    StsDescp = ei.Get("STS_DESCP").ToString(),
                                    EventTime=ei.Get("EVEN_DATE").ToString(),
                                    Location=ei.Get("LOCATION").ToString(),
                                    Remark=ei.Get("REMARK").ToString()
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }

    }
}
