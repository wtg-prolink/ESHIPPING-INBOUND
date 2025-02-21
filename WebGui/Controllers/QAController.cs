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
using System.Web.Configuration;

namespace WebGui.Controllers
{
    public class QAController : BaseController
    {

        #region View
        public ActionResult QAQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("QA010");
            getSelection();
            return View();
        }

        public ActionResult QASetupView(string id = null, string uid = null)
        {
            SetSchema("SYS_QA");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("QA010");
            ViewBag.Uid = id;
            ViewBag.EdocUrl = WebConfigurationManager.AppSettings["EDOC_URL1"];
            getSelection();
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string condition = string.Empty;
            if (UPri == "G")
            {
                condition += GetBaseGroup();
            }
            else
            {
                condition += GetBaseCmp();
            }
            condition = GetCreateDateCondition("SYS_QA", condition);
            return GetBootstrapData("SYS_QA", condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SYS_QA WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SysQaModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string QaAnswer = Request["QaAnswer"];
            if (!string.IsNullOrEmpty(QaAnswer))
                QaAnswer = HttpUtility.UrlDecode(QaAnswer);
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SysQaModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            if (!string.IsNullOrEmpty(QaAnswer))
                                ei.Put("QA_ANSWER", QaAnswer);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            if (!string.IsNullOrEmpty(QaAnswer))
                                ei.Put("QA_ANSWER", QaAnswer);
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

            string sql = string.Format("SELECT * FROM SYS_QA WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SysQaModel");
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
                    case "SYS_QA":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SYS_QA WHERE 1=0";
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

        #region 创建select
        public void getSelection()
        {
            string sql = string.Empty;
            sql = "SELECT CD, CD_DESCP FROM BSCODE WHERE CD_TYPE='QAT' AND " + GetBaseCmp();
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string col = string.Empty;
            string sel = string.Empty;
            if (dt.Rows.Count > 0)
            {
                
                foreach (DataRow item in dt.Rows)
                {
                    string Cd = Prolink.Math.GetValueAsString(item["CD"]);
                    string CdDescp = Prolink.Math.GetValueAsString(item["CD_DESCP"]);

                    col += Cd + ":" + CdDescp + ";";
                    sel += "<option value='" + Cd + "'>" + CdDescp + "</option>";
                }
            }

            ViewBag.col_sel = col;
            ViewBag.selection = sel;
        }
        #endregion

        public ActionResult byGroupGetDetail()
        {
            string Type = Request["Type"];
            if (Type.Contains("?"))
            {
                Type = Type.Split('?')[0];
            }
            string QAType = getQAType(Type);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sub"] = "Y";
            data["qaType"] = QAType;
            string sql = string.Format("SELECT QA_TITLE,QA_ANSWER,ISNULL(MODIFY_BY,CREATE_BY) AS UPLOADER,ISNULL(MODIFY_DATE,CREATE_DATE) AS UPDATE_DATE " +
                "FROM SYS_QA WHERE QA_TYPE LIKE '%{0}%'  ORDER BY UPDATE_DATE DESC", QAType);
            if (string.IsNullOrEmpty(QAType))
            {
                sql = string.Format("SELECT * FROM SYS_QA WHERE 1=0");
                data["sub"] = "N";
            }
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            data["main"] = ModelFactory.ToTableJson(dt);
            return ToContent(data);
        }

        public ActionResult GetSearchDetail()
        {
            string type = Request["Type"];
            string searchInfo = Request["SearchInfo"];
            string titleClick = Request["titleClick"];
            int clickCount = Prolink.Math.GetValueAsInt(titleClick);
            Dictionary<string, object> data = new Dictionary<string, object>();

            string orderby = "";
            //187216
            if (clickCount == 0)
                orderby = " ORDER BY UPDATE_DATE DESC";
            else if (clickCount % 2 == 1)
                orderby = " ORDER BY QA_TITLE COLLATE CHINESE_PRC_CI_AS ASC";
            else if (clickCount % 2 == 0)
                orderby = " ORDER BY QA_TITLE COLLATE CHINESE_PRC_CI_AS DESC";

            string sql = string.Format("SELECT QA_TITLE,QA_ANSWER,ISNULL(MODIFY_BY,CREATE_BY) AS UPLOADER,ISNULL(MODIFY_DATE,CREATE_DATE) AS UPDATE_DATE " +
                "FROM SYS_QA WHERE QA_TYPE LIKE {0} AND (QA_TITLE LIKE {1} OR QA_ANSWER LIKE {1})", SQLUtils.QuotedStr("%" + type + "%"), SQLUtils.QuotedStr("%" + searchInfo + "%"));

            DataTable dt = OperationUtils.GetDataTable(sql + orderby, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            data["main"] = ModelFactory.ToTableJson(dt);
            return ToContent(data);
        }

        public string getQAType(string type)
        {
            string sql = string.Format("SELECT AP_CD FROM BSCODE WHERE CD_TYPE='IQAM' AND CD={0}", SQLUtils.QuotedStr(type));
            string QAType = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return QAType;
        }

    }
}
