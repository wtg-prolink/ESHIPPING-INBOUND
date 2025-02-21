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
using Business.Mail;
using Business;
using TrackingEDI.Business;
using TrackingEDI.Mail;

namespace WebGui.Controllers
{
    public class SMPTYController : BaseController
    {
        //
        // GET: /SMPTY/

        #region 基本操作
        /*匯總查詢*/
        public ActionResult SmsimQueryData()
        {
            string con = string.Empty;
            if (UPri != "G")
            {
                con = " CMP = " + SQLUtils.QuotedStr(CompanyId);
            }
            con = GetCreateDateCondition("SMSIM", con);
            return GetBootstrapData("SMSIM", con);
        }

        public JsonResult GetSmsimDetail()
        {
            string u_id = Request["UId"];
            //string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            //string sql = string.Format("SELECT * FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable dt = new DataTable();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();

            string sql = "SELECT TOP 1 * FROM SMSIM WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());


            //DataTable dt = ModelFactory.InquiryData("*", "SMINM", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            string conditions = " WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
            DataTable detailDt = ModelFactory.InquiryData("*", "SMSID", conditions, " SEQ_NO ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(detailDt)
            };

            return Json(new { mainTable = result.ToContent(), subData1 = resultDetail.ToContent() });
        }

        public JsonResult GetSmsidDetail()
        {
            string u_id = Request["UId"];
            //string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            //string sql = string.Format("SELECT * FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable dt = new DataTable();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();

            string sql = "SELECT TOP 1 * FROM SMSID WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());


            //DataTable dt = ModelFactory.InquiryData("*", "SMINM", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            return Json(new { mainTable = result.ToContent()});
        }

        public ActionResult SmsimUpdateData()
        { 
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            List<Dictionary<string, object>> simData = new List<Dictionary<string, object>>();
            //List<Dictionary<string, object>> sidData = new List<Dictionary<string, object>>();
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsimModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.PutKey("U_ID", UId);
                            ei.Put("CMP", CompanyId);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            /*
                            EditInstruct ei2 = new EditInstruct("SMSID", EditInstruct.UPDATE_OPERATION);
                            if (ei.Get("INV_FLOW") != null)
                            {
                                ei2.PutKey("U_FID", UId);
                                ei2.Put("INV_FLOW", ei.Get("INV_FLOW"));
                                mixList.Add(ei2);
                            }
                              */
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            /*
                            EditInstruct ei2 = new EditInstruct("SMSID", EditInstruct.DELETE_OPERATION);
                             if (ei.Get("U_ID") == null)
                            {
                                continue;
                            }

                             ei2.PutKey("U_FID", UId);
                             mixList.Add(ei2);
                             */
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
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    string sql = string.Format("SELECT * FROM SMSIM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    simData = ModelFactory.ToTableJson(mainDt, "SmsimModel");
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = simData});
        }

        public ActionResult SmsidUpdateData()
        { 
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            List<Dictionary<string, object>> sidData = new List<Dictionary<string, object>>();
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsidModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string sql = "SELECT INV_FLOW FROM SMSIM WHERE U_ID = " + SQLUtils.QuotedStr(UFid);
                            string InvFlow = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                            sql = "SELECT MAX(SEQ_NO) AS SEQ FROM SMSID WHERE U_FID = " + SQLUtils.QuotedStr(UFid);
                            int seq = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                            UId = System.Guid.NewGuid().ToString();
                            ei.PutKey("U_ID", UId);
                            ei.Put("U_FID", UFid);
                            ei.Put("INV_FLOW", InvFlow);
                            ei.Put("SEQ_NO", seq + 1);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
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
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    string sql = string.Format("SELECT * FROM SMSID WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sidData = ModelFactory.ToTableJson(mainDt, "SmsidModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = sidData});
        }

        public ActionResult GetSmsidGridData()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string conditions = " U_FID=" + SQLUtils.QuotedStr(UId);
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();

            DataTable dt = ModelFactory.InquiryData("*", "SMSID", conditions, "SEQ_NO ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return resultDetail.ToContent();
        }

        /*檢查要儲存的資料在smsim有沒有重覆*/
        public JsonResult chkSmsimKey()
        {
            string returnMessage = "success";
            string CustCd = Prolink.Math.GetValueAsString(Request.Params["CustCd"]);
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string InvFlow = Prolink.Math.GetValueAsString(Request.Params["InvFlow"]);

            string sql = "SELECT COUNT(*) FROM SMSIM WHERE CUST_CD=" + SQLUtils.QuotedStr(CustCd) + " AND LOCATION=" + SQLUtils.QuotedStr(Cmp) + " AND INV_FLOW=" + SQLUtils.QuotedStr(InvFlow);
            int n = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (n > 0)
            {
                returnMessage = "fail";
            }

            return Json(new { msg = returnMessage });
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
