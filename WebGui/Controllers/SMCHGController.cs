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
namespace WebGui.Controllers
{
    public class SMCHGController : BaseController
    {
        //
        // GET: /SMCHG/

        public ActionResult SMCHGSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM SMCHG WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> mtSchemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.mtSchemas = ToContent(mtSchemas);
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("SMCHG");
            return View();
        }

        public ActionResult SMCHGQuery()
        {
            ViewBag.MenuBar = false;
            SetTranModeSelect();
            return View();
        }

        /*匯總查詢*/
        public ActionResult SMCHGQueryData()
        {
            string condition = GetDataPmsCondition("C");
            condition = GetCreateDateCondition("SMCHG", condition);
            return GetBootstrapData("SMCHG", condition);
        }

        public ActionResult GetSmchgDetail()
        {
            string u_id = Request["UId"];
            //string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            //string sql = string.Format("SELECT * FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable dt = new DataTable();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();

            string sql = "SELECT TOP 1 * FROM SMCHG WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());


            //DataTable dt = ModelFactory.InquiryData("*", "SMINM", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            return Json(new { mainTable = result.ToContent() });
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        [ValidateInput(false)]
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

        public ActionResult SMCHGUpdate()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            List<Dictionary<string, object>> simData = new List<Dictionary<string, object>>();
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMCHGModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            UId = Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
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
                }
            }
            string sql = string.Format("SELECT * FROM SMCHG WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            simData = ModelFactory.ToTableJson(mainDt, "SMCHGModel");
            return Json(new { message = returnMessage, mainData = simData });
        }

        public ActionResult SMCHGSetupInquiryData()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            DataTable dt = ModelFactory.InquiryData("*", "SMCHG", Request.Params, ref recordsCount, ref pageIndex, ref pageSize, "");

            string resultType = Request.Params["resultType"];
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

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues)
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
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
    }
}
