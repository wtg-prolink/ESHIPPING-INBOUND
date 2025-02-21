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
    public class BSTRUCKController : BaseController
    {
        //
        // GET: /BSTRUCK/

        public ActionResult BSTRUCKQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BSTRUCK");
            return View();
        }

        public ActionResult BSTRUCKSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string PartyNo = "", PartyName = "", Cmp="";
            string sql = "SELECT * FROM SMPTY WHERE U_ID=" + SQLUtils.QuotedStr(id);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    PartyNo = Prolink.Math.GetValueAsString(item["PARTY_NO"]);
                    PartyName = Prolink.Math.GetValueAsString(item["PARTY_NAME"]);
                    Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                }
            }
            sql = "SELECT NAME FROM SYS_SITE WHERE TYPE=1 AND CMP="+SQLUtils.QuotedStr(Cmp);
            string CmpNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            ViewBag.UFid = id;
            ViewBag.PartyName = PartyName;
            ViewBag.PartyNo = PartyNo;
            ViewBag.CmpNm = CmpNm;
            ViewBag.Cmp = Cmp;
            ViewBag.pmsList = GetBtnPms("BSTRUCK");
            return View();
        }

        public ActionResult GetTruckByParty()
        {
            string sql = "SELECT IO_FLAG FROM SYS_ACCT WHERE U_ID=" + SQLUtils.QuotedStr(UserId);
            string IoFlag = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string con = " (PARTY_TYPE LIKE '%CR%' OR PARTY_TYPE LIKE '%IBCR%')";
            if (IoFlag == "O")
            {
                con += " AND (PARTY_NO=" + SQLUtils.QuotedStr(UserId) + " OR PARTY_NO=" + SQLUtils.QuotedStr(CompanyId)+")";
            }
            con = GetCreateDateCondition("SMPTY", con);
            return GetBootstrapData("SMPTY", con, " PARTY_NO ASC");
        }

        public ActionResult TruckQuery()
        {
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["CMP"]);
            string PartyNo = Prolink.Math.GetValueAsString(Request.Params["PartyNo"]);
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            //string condition = " CMP=" + SQLUtils.QuotedStr(Cmp) + " AND PARTY_NO=" + SQLUtils.QuotedStr(PartyNo);
            string condition = " U_FID=" + SQLUtils.QuotedStr(UFid);
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSTRUCKD", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BsTruckdModel")
            };

            DataTable dt1 = ModelFactory.InquiryData("*", "BSTRUCKC", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result1 = null;
            result1 = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt1, "BsTruckcModel")
            };

            return Json(new { mainGridData = result.rows, subGridData = result1.rows });
        }

        public ActionResult SaveBstruckData()
        {
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["CMP"]);
            string PartyNo = Prolink.Math.GetValueAsString(Request.Params["PartyNo"]);
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
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
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsTruckdModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("U_FID", UFid);
                            ei.Put("CMP", Cmp);
                            ei.Put("PARTY_NO", PartyNo);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                            
                        mixList.Add(ei);
                    }
                }
                else if(item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsTruckcModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("U_FID", UFid);
                            ei.Put("CMP", Cmp);
                            ei.Put("PARTY_NO", PartyNo);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
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

            List<Dictionary<string, object>> cData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> dData = new List<Dictionary<string, object>>();
            sql = string.Format("SELECT * FROM BSTRUCKC WHERE U_FID={0}", SQLUtils.QuotedStr(UFid));
            DataTable cDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            cData = ModelFactory.ToTableJson(cDt, "BsTruckcModel");

            sql = string.Format("SELECT * FROM BSTRUCKD WHERE U_FID={0}", SQLUtils.QuotedStr(UFid));
            DataTable dDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            dData = ModelFactory.ToTableJson(dDt, "BsTruckdModel");


            return Json(new { message = returnMessage, mainData = dData, subData = cData });
        }

        private ActionResult GetBootstrapData(string table, string condition, string orderBy = "", string colNames = "*")
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
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize, orderBy);
                //dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
