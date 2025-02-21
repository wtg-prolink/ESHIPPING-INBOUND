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
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json;
using System.Globalization;

namespace WebGui.Controllers
{
    public class CostInfoController : BaseController
    {
        //
        // GET: /IPCTM/

        public ActionResult ShowCostInfo()
        {
            //string uid = Prolink.Math.GetValueAsString(Request.Params["uid"]),
            string poNo = Prolink.Math.GetValueAsString(Request.Params["pono"]);
            string postdata = System.Web.HttpUtility.UrlDecode(Prolink.Math.GetValueAsString(Request.Params["postdata"]));
            string[] dataArray = postdata.Split('&');
            foreach (string item in dataArray)
            {
                int eqChar = item.IndexOf("=");
                string fieldName = item.Substring(0, eqChar);
                string fieldValue = item.Substring(eqChar + 1);
                switch (fieldName)
                {
                    case "PoNo":
                        {
                            ViewBag.PoNo = fieldValue;
                            break;
                        }
                    case "CtNo":
                        {
                            ViewBag.CtNo = fieldValue;
                            break;
                        }
                    case "BlNo":
                        {
                            ViewBag.BlNo = fieldValue;
                            break;
                        }
                    case "CHMUId":
                        {
                            ViewBag.CHMUId = fieldValue;
                            break;
                        }
                    case "CHMPlamt":
                        {
                            ViewBag.CHMPlamt = fieldValue;
                            break;
                        }
                    case "CHMFlamt":
                        {
                            ViewBag.CHMFlamt = fieldValue;
                            break;
                        }
                    case "CHMSewqNo":
                        {
                            ViewBag.CHMSewqNo = fieldValue;
                            break;
                        }
                    case "WiNo":
                        {
                            ViewBag.WiNo = fieldValue;
                            break;
                        }
                    case "Dep":
                        {
                            ViewBag.Dep = fieldValue;
                            break;
                        }
                    case "WicUId":
                        {
                            ViewBag.WicUId = fieldValue;
                            break;
                        }
                    case "PomcUid":
                        {
                            ViewBag.PomcUid = fieldValue;
                            break;
                        }
                    case "PomUid":
                        {
                            ViewBag.PomUid = fieldValue;
                            break;
                        }
                    case "SomUid":
                        {
                            ViewBag.SomUid = fieldValue;
                            break;
                        }
                    case "LcUid":
                        {
                            ViewBag.LcUid = fieldValue;
                            break;
                        }
                    case "CmpStn":
                        {
                            ViewBag.CmpStn = fieldValue;
                            break;
                        }
                    case "JobNo":
                        {
                            ViewBag.JobNo = fieldValue;
                            break;
                        }
                    case "Bu":
                        {
                            ViewBag.ChargeDep = fieldValue == "A" ? "TD" : "HU";
                            break;
                        } 
                }
            }

            //ViewBag.uId = uid;
            //ViewBag.poNo = poNo;
            return View();
        }


        public ActionResult ChargeRequiry()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;

            DataTable dt = ModelFactory.InquiryData("GfBlchgModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "GfBlchgModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult ChargeUpdateData()
        {
            string changeData = Request.Params["changedData"];
            int CHMSewqNo = Prolink.Math.GetValueAsInt(Request.Params["CHMSewqNo"]);
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
            string uid = string.Empty;
            string jobNo = string.Empty;
            List<Dictionary<string, object>> chargeData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> costData = new List<Dictionary<string, object>>();
            foreach (var item in dict)
            {
                if (item.Key == "JobNo")
                    jobNo = Prolink.Math.GetValueAsString(item.Value);
                if (item.Key == "chargeDt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "GfBlchgModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (ei.Get("U_ID") == null)
                            {
                                continue;
                            }
                            uid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", System.DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            //if (CHMSewqNo == 0)
                            //{
                            //    ei.Put("SEQ", i + 1);
                            //}
                            //else
                            //{
                            //    ei.Put("SEQ", CHMSewqNo);
                            //}
                            ei.PutKey("U_ID", Guid.NewGuid().ToString().Replace("-", ""));
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", System.DateTime.Now);
                            ei.Put("PC", "L");
                            //ei.Put("CHG_TYPE", "D");
                            ei.Put("VAT_FLAG", "Y");
                            ei.Put("PKG", 1);
                            ei.Put("PRICE", ei.Get("ACTUAL_AMT"));
                            ei.Put("TTL_AMT", ei.Get("ACTUAL_AMT"));
                            ei.Put("TTL_LAMT", ei.Get("LOCAL_AMT"));
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "costDt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "GfBlchgModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (ei.Get("U_ID") == null)
                            {
                                continue;
                            }
                            uid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", System.DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            //if (CHMSewqNo == 0)
                            //{
                            //    ei.Put("SEQ", i + 1);
                            //}
                            //else
                            //{
                            //    ei.Put("SEQ", CHMSewqNo);
                            //}
                            ei.PutKey("U_ID", Guid.NewGuid().ToString().Replace("-", ""));
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", System.DateTime.Now);
                            ei.Put("PC", "L");
                            //ei.Put("CHG_TYPE", "C");
                            ei.Put("VAT_FLAG", "Y");
                            ei.Put("PKG", 1);
                            ei.Put("PRICE", ei.Get("ACTUAL_AMT"));
                            ei.Put("TTL_AMT", ei.Get("ACTUAL_AMT"));
                            ei.Put("TTL_LAMT", ei.Get("LOCAL_AMT"));
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
                    DataTable dt = new DataTable();
                    //if (CHMSewqNo == 0)
                    //{
                    dt = ModelFactory.InquiryData("*", "GFBLCHG", "CHG_TYPE='D' AND JOB_NO='" + jobNo + "'", "SEQ ASC", pageIndex, pageSize, ref recordsCount);
                    //}
                    //else
                    //{
                    //    dt = ModelFactory.InquiryData("*", "GFBLCHG", "CHG_TYPE='D' AND JOB_NO='" + jobNo + "' AND SEQ='"+CHMSewqNo+"'", "", pageIndex, pageSize, ref recordsCount);
                    //}
                    chargeData = ModelFactory.ToTableJson(dt, "GfBlchgModel");
                    //if (CHMSewqNo == 0)
                    //{
                        dt = ModelFactory.InquiryData("*", "GFBLCHG", "CHG_TYPE='C' AND JOB_NO='" + jobNo + "'", "SEQ ASC", pageIndex, pageSize, ref recordsCount);
                    //}
                    //else
                    //{
                    //    dt = ModelFactory.InquiryData("*", "GFBLCHG", "CHG_TYPE='C' AND JOB_NO='" + jobNo + "' AND SEQ='" + CHMSewqNo + "'", "", pageIndex, pageSize, ref recordsCount);
                    //}
                    costData = ModelFactory.ToTableJson(dt, "GfBlchgModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, chargeData = chargeData, costData = costData });
        }

        public ActionResult CHMSetupInquiryData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            //获取采购明细档，所以这边应该使用采购号码CT_NO来进行关联查找出
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string tempGrid = Prolink.Math.GetValueAsString(Request.Params["tempGrid"]);
            string condtions = " WHERE U_ID='" + UId + "'";
            DataTable dt = new DataTable();
            if (tempGrid == "#CopeGrid")
            {
                dt = ModelFactory.InquiryData("U_ID AS Job_No,AC_NO AS Chg_Cd,AC_DESCP AS Chg_Descp,'RMB' AS Cur,ABS(PLAMT) AS Actual_Amt,ABS(FLAMT) AS Actual_Amt2,'1' AS Exchrt,ABS(PLAMT) AS Local_Amt,RCUST_CD AS Bill_Cd,DN_NO AS Inv_No,REMARK AS Rmk,CMP AS Cmp,STN AS Stn,DEP AS Dep", "IPCHM", condtions, "", pageIndex, pageSize, ref recordsCount);
            }
            if (tempGrid == "#AccountsGrid")
            {
                dt = ModelFactory.InquiryData("U_ID AS Job_No,AC_NO AS Chg_Cd,AC_DESCP AS Chg_Descp,'RMB' AS Cur,ABS(FLAMT) AS Actual_Amt,'1' AS Exchrt,ABS(FLAMT) AS Local_Amt,PCUST_CD AS Bill_Cd,DN_NO AS Inv_No,REMARK AS Rmk,CMP AS Cmp,STN AS Stn,DEP AS Dep", "IPCHM", condtions, "", pageIndex, pageSize, ref recordsCount);
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

        /// <summary>
        /// 过账单 by milo 20150825
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateBill()
        {
            string invnos = HttpUtility.UrlDecode(Prolink.Math.GetValueAsString(Request["invnos"]));
            string[] invNoArr = invnos.Split(';');
            string message = "";
            MixedList list = new MixedList();
            string format = "yyyyMMdd";
            for (int i = 0; i < invNoArr.Length; i++)
            {
                if (invNoArr[i].Trim() == "")
                    continue;
                //string[] combStr = invNoArr[i].Split(',');
                string uid = invNoArr[i];
                //string sql = "SELECT * FROM GFBLCHG WHERE JOB_NO=" + SQLUtils.QuotedStr(combStr[0])
                //    + " AND CHG_CD=" + SQLUtils.QuotedStr(combStr[1])
                //    + " AND CHG_TYPE=" + SQLUtils.QuotedStr(combStr[2])
                //    + " AND SEQ=" + SQLUtils.QuotedStr(combStr[3]);
                string sql = "SELECT * FROM GFBLCHG WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];
                    DateTime rdt = DateTime.Now;
                    string invNo = Prolink.Math.GetValueAsString(dr["INV_NO"]);
                    string billCd = Prolink.Math.GetValueAsString(dr["BILL_CD"]);
                    string dnDate = Prolink.Math.GetValueAsString(dr["DN_DATE"]);
                    string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                    string stn = Prolink.Math.GetValueAsString(dr["STN"]);
                    if (!string.IsNullOrEmpty(invNo))
                    {
                        message = string.Format("{0}已经过账单。", Prolink.Math.GetValueAsString(dr["CHG_DESCP"]));
                        return Json(new { result = false, message = message });
                    }
                    //ei.Put("INV_NO", invNo);
                    if (dnDate != "")
                    {

                        dnDate = dnDate.Substring(0, 8);
                    }
                    else
                    {
                        dnDate = DateTime.Now.ToString("yyyyMMdd");
                    }

                   /* WebGui.ColdChainReference.CheckMonthlyCloseRequest service = new WebGui.ColdChainReference.CheckMonthlyCloseRequest();
                    string result = "true";
                    using (var client = new WebGui.ColdChainReference.ColdChainSoapClient())
                    {
                        WebGui.ColdChainReference.ArrayOfString data = new WebGui.ColdChainReference.ArrayOfString();
                        data.Add(dnDate);
                        data.Add(cmp);
                        data.Add(stn);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(Prolink.IO.CompressUtils.DecompressFromString(client.CheckMonthlyClose(data)));
                        result = JsonConvert.SerializeXmlNode(doc);

                    }*/

                    JObject obj = JsonConvert.DeserializeObject<JObject>(result);
                    result = obj["WSData"]["result"].ToString();

                    if (result == "True")
                    {
                        EditInstruct ei = new EditInstruct("GFINV", EditInstruct.INSERT_OPERATION);
                        bool isCost = Prolink.Math.GetValueAsString(dr["CHG_TYPE"]) == "C" ? true : false;

                        string ruleCode = isCost ? "COST_INVOICE_NO" : "CHARGE_INVOICE_NO";
                        System.Collections.Hashtable hash = new System.Collections.Hashtable();
                        hash.Add("CMP", CompanyId);
                        hash.Add("STN", Station);
                        invNo = AutoNo.GetNo(ruleCode, hash, GroupId, CompanyId, Station);

                        ei.Put("INV_NO", invNo);
                        ei.Put("BILL_CD", dr["BILL_CD"]);
                        ei.Put("BILL_NM", dr["BILL_NM"]);
                        ei.Put("DEP", dr["DEP"]);
                        ei.Put("GROUP_ID", dr["GROUP_ID"]);
                        ei.Put("CMP", dr["CMP"]);
                        ei.Put("STN", dr["STN"]);
                        ei.Put("JOB_NO", dr["JOB_NO"]);
                        ei.Put("ARAP", isCost ? "AP" : "AR");
                        ei.Put("DC", dr["CHG_TYPE"]);
                        ei.Put("INV_TYPE", "L");
                        ei.Put("BL_NO", dr["BL_NO"]);
                        ei.Put("INV_AMT", dr["ACTUAL_AMT"]);
                        ei.Put("OFFSET_AMT", 0);
                        ei.Put("BALANCE_AMT", dr["ACTUAL_AMT"]);
                        ei.Put("VOID_FLAG", "N");
                        ei.Put("EXCH_RATE", dr["EXCHRT"]);
                        ei.PutDate("INV_DATE", dr["DN_DATE"]);
                        ei.PutDate("PAY_DATE", dr["DN_DATE"]);
                        ei.PutDate("CREATE_DATE", System.DateTime.Now);
                        ei.Put("CREATE_BY", UserId);
                        ei.Put("INV_TAX_AMT", dr["TAX_AMT"]);
                        ei.Put("INV_NTAX_AMT", dr["NTAX_AMT"]);
                        ei.Put("LOCAL_AMT", dr["LOCAL_AMT"]);
                        ei.Put("ROWID", Guid.NewGuid().ToString());
                        ei.Put("TEMP", "N");
                        ei.Put("CUR", dr["CUR"]);
                        list.Add(ei);

                        //sql = "UPDATE GFBLCHG SET INV_NO=" + SQLUtils.QuotedStr(invNo) + " WHERE JOB_NO=" + SQLUtils.QuotedStr(combStr[0])
                        //+ " AND CHG_CD=" + SQLUtils.QuotedStr(combStr[1])
                        //+ " AND CHG_TYPE=" + SQLUtils.QuotedStr(combStr[2])
                        // + " AND SEQ=" + SQLUtils.QuotedStr(combStr[3]);
                        sql = "UPDATE GFBLCHG SET INV_NO=" + SQLUtils.QuotedStr(invNo) + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                        list.Add(sql);
                    }
                    else
                    {
                        message += "收款对象" + billCd + "该月份已经关账，请修改账单日期";
                    }
                }
            }
            if (list.Count > 0)
            {
                int[] result = OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            return Json(new { result = true, message = message });
        }
        //判斷LOCK_DATE
        public ActionResult checkLockDate()
        {
            string invnos = HttpUtility.UrlDecode(Prolink.Math.GetValueAsString(Request["invnos"]));
            string[] invNoArr = invnos.Split(';');
            string uids = HttpUtility.UrlDecode(Prolink.Math.GetValueAsString(Request["uids"]));
            string[] uIdArr = uids.Split(';');
            string message = "";
            MixedList list = new MixedList();
           
            for (int i = 0; i < invNoArr.Length; i++)
            {
                string uid = uIdArr[i];
                string InvNo = invNoArr[i];
                string sql = "SELECT * FROM GFBLCHG WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];
                    string invNo = Prolink.Math.GetValueAsString(dr["INV_NO"]);
                    string groupId = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                    string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                    string stn = Prolink.Math.GetValueAsString(dr["STN"]);
                    string dep = Prolink.Math.GetValueAsString(dr["DEP"]);

                    sql = "SELECT * FROM GFINV WHERE INV_NO=" + SQLUtils.QuotedStr(InvNo) + " AND GROUP_ID=" + SQLUtils.QuotedStr(groupId) + " AND CMP=" + SQLUtils.QuotedStr(cmp) + " AND STN=" + SQLUtils.QuotedStr(stn) + " AND DEP=" + SQLUtils.QuotedStr(dep);
                    DataTable dx = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count == 1)
                    {
                        DataRow dl = dx.Rows[0];
                        string lockDate = Prolink.Math.GetValueAsString(dl["LOCK_DATE"]);
                        if (!string.IsNullOrEmpty(lockDate))
                        {
                            message += string.Format("账单{0}已经审核，请商务还原。", Prolink.Math.GetValueAsString(dl["INV_NO"]));
                            //return Json(new { result = false, message = message });
                        }
                        else
                        {
                            sql = "DELETE FROM GFINV WHERE INV_NO=" + SQLUtils.QuotedStr(InvNo) + " AND GROUP_ID=" + SQLUtils.QuotedStr(groupId) + " AND CMP=" + SQLUtils.QuotedStr(cmp) + " AND STN=" + SQLUtils.QuotedStr(stn) + " AND DEP=" + SQLUtils.QuotedStr(dep);
                            list.Add(sql);
                            sql = "UPDATE GFBLCHG SET INV_NO=NULL WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                            list.Add(sql);
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                int[] result = OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            return Json(new {  message = message });
        }
    }
}
