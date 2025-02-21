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
using Business.TPV.Standard;
using System.Text.RegularExpressions;
using System.IO;
using TrackingEDI.InboundBusiness;
using Business.Service;
using Business.TPV;

namespace WebGui.Controllers
{
    public class ActManageController : BaseController
    {
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
                    case "ActSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMBIM WHERE 1=0";
                        break;
                    case "ActCheckSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMBIM WHERE 1=0";
                        break;
                    case "SMBID":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMBID WHERE 1=0";
                        break;
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

            string sql = " SELECT  '" + statusField + "' col," + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";

            if (!string.IsNullOrEmpty(dnapprove))
            {
                if (dnapprove == "DNAPPROVE")
                {
                    string personsql = "SELECT  '" + statusField + "' col, 'Person' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE APPROVE_USER='" + UserId + "' UNION";
                    string localsql = " SELECT   '" + statusField + "' col," + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " AND APPROVE_TO NOT IN('A') GROUP BY " + col + " UNION";
                    string asql = " SELECT  '" + statusField + "' col," + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " AND APPROVE_TO='A' GROUP BY " + col;
                    sql = personsql + localsql + asql;
                }
            }

            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "  SELECT  '" + statusField + "' col,'' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
            DataTable dtsum = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dtAll = dt.Copy();
            foreach (DataRow dr in dtsum.Rows)
            {
                dtAll.ImportRow(dr);
            }

            return dtAll;
        }
        #endregion

        #region view
        /// <summary>
        /// 账单费用审核
        /// </summary>
        /// <returns></returns>
        public ActionResult ChgApproveManage()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC090");
            SetTranModeSelect();
            SetLocationSelect();
            return View();
        }

        public ActionResult ActQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC010");
            SetRoleSelect();
            SetTranModeSelect();
            return View();
        }

        public ActionResult ActSetup(string id = null, string uid = null)
        {
            SetSchema("ActSetup");
            ViewBag.pmsList = GetBtnPms("AC010");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.IOFlag = IOFlag;
            ViewBag.LspNo = CompanyId;
            string sql = "SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(CompanyId);
            string LspNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.LspNm = LspNm;
            ViewBag.UserId = UserId;
            ViewBag.Type = ActHelper.GetOperType(ViewBag.Uid);
            Business.TPV.EDIConfig config = Business.TPV.Context.GetEDIConfigFromList("FSSP", "FSSP-IMG-IFRAME");
            ViewBag.FSSPURL = config == null ? "" : config.Server;
            sql = string.Format("SELECT DISTINCT CD+';' FROM BSCODE WHERE CD_TYPE='FSSP' FOR XML PATH('') ", SQLUtils.QuotedStr(CompanyId));
            string fsspCmps = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.FSSPCmp = fsspCmps.Trim(';');
            return View();
        }

        public ActionResult ActCheck()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult ActCheckSetup(string id = null, string uid = null)
        {
            SetSchema("ActCheckSetup");
            ViewBag.pmsList = GetBtnPms("AC030");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            string cmp = OperationUtils.GetValueAsString(string.Format("SELECT CMP FROM SMBIM WHERE U_ID={0}",
                SQLUtils.QuotedStr(ViewBag.Uid)), Prolink.Web.WebContext.GetInstance().GetConnection());
            cmp = string.IsNullOrEmpty(cmp) ? CompanyId : cmp;
            ViewBag.tvMntSelect = GetTVMNTSelect(cmp);
            ViewBag.UserId = UserId;
            ViewBag.Type = ActHelper.GetOperType(ViewBag.Uid);
            Business.TPV.EDIConfig config = Business.TPV.Context.GetEDIConfigFromList("FSSP", "FSSP-IMG-IFRAME");
            ViewBag.FSSPURL = config == null ? "" : config.Server;
            return View();
        }

        /*
         * 帐单比对汇总
         */
        public ActionResult ActCheckQueryView()
        {
            string tranType = Request["tranType"];
            if (string.IsNullOrEmpty(tranType)) tranType = string.Empty;
            ViewBag.TranType = tranType;
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC020" + tranType);
            return View();
        }

        /*
         * 帐单比对明细
         */
        public ActionResult ActCheckSetupView(string id = null, string uid = null)
        {
            SetSchema("ActCheckSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("AC020");

            return View();
        }

        //请款审核
        public ActionResult ActApproveQueryView()
        {
            string tranType = Request["tranType"];
            if (string.IsNullOrEmpty(tranType)) tranType = string.Empty;
            ViewBag.TranType = tranType;

            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC030");
            ViewBag.approveGroup = GetApproveGroup("BILLING");
            SetRoleSelect();
            SetApproveSelect();
            GetUseAcct();
            SetApproveRole();
            SetApproveType();
            ViewBag.tvMntSelect = GetTVMNTSelect(CompanyId);
            return View();
        }

        //帳單管理

        public ActionResult ActApproveHighQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC999");
            ViewBag.approveGroup = GetApproveGroup("BILLING");


            ViewBag.SelectRole = "";
            ViewBag.DefaultRole = "";

            string sql = @"SELECT distinct  APPROVE_GROUP,GROUP_DESCP,SEQ_NO FROM APPROVE_ATTR_D WHERE U_FID in (select U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR='BILLING' ) 
            ORDER BY SEQ_NO,APPROVE_GROUP ASC";

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            string approvegroup = string.Empty;
            List<string> approvelist = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                approvegroup = Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                if ("AM".Equals(approvegroup) || "AMM".Equals(approvegroup) || "AMMM".Equals(approvegroup))
                    continue;

                if (approvelist.Contains(approvegroup))
                    continue;
                approvelist.Add(approvegroup);

                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultRole = approvegroup;
                }
                select += approvegroup;
                select += ":" + Prolink.Math.GetValueAsString(dr["GROUP_DESCP"]);
            }
            select += ";Finish:Finish";
            ViewBag.SelectRole = select;
            SetApproveSelect();
            GetUseAcct();
            SetApproveRole();
            return View();
        }

        private void SetApproveType()
        {
            ViewBag.SelectApproveType = "";
            ViewBag.DefaultApproveType = "";

            #region Approve
            string sql = string.Format(@"SELECT DISTINCT APPROVE_CODE,APPROVE_NAME FROM APPROVE_FLOW_M WHERE  GROUP_ID={0} AND CMP_ID={1} AND AU_ID IN(
                SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE  GROUP_ID={0} AND CMP={1} AND APPROVE_ATTR='BILLING')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultApproveType = Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
                }
                select += Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["APPROVE_NAME"]);
            }
            ViewBag.SelectApproveType = select;
            #endregion
        }

        public ActionResult ActManage()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC040");
            SetRoleSelect();
            return View();
        }

        public ActionResult ActDeatilManage()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC060");
            SetTranModeSelect();
            SetLocationSelect();
            return View();
        }
       
        public ActionResult ActUpdate()
        {
            SetSchema("SMBID");
            string uid = Request["uid"];
            string add = Request["add"];
            ViewBag.add = string.IsNullOrEmpty(add) ? "N" : add;
            ViewBag.Uid = string.IsNullOrEmpty(uid) ? string.Empty : uid;
            return View();
        }

        #endregion

        #region 账单输入
        /// <summary>
        /// 账单保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveData()
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
            string DebitNo = Request["DebitNo"];
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.IsSucceed = true;
            bool isDel = false;
            string TpvDebitNo = Request["TpvDebitNo"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);

            if (!string.IsNullOrEmpty(DebitNo))
                DebitNo = HttpUtility.UrlDecode(DebitNo);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            string upDebitNo = string.Empty;
            List<string> bidList = new List<string>();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbimModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;                          
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("APPROVE_TYPE", "STD_BILL");
                            ei.Put("APPROVE_TO", "A");
                            ei.Put("GROUP_ID", "TPV");
                            string cmp = Prolink.Math.GetValueAsString(ei.Get("CMP"));
                            string billTo = Prolink.Math.GetValueAsString(ei.Get("BILL_TO"));
                            string billToNm = getBillToNm(billTo, cmp, "TPV");
                            if (!string.IsNullOrEmpty(billToNm))
                                ei.Put("BILL_TO_NAME", billToNm); 
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;                           
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("MODIFY_DATE", odt);

                            ei.PutDate("MODIFY_DATE_L", ndt);
                            string cmp = Prolink.Math.GetValueAsString(ei.Get("CMP"));
                            string billTo = Prolink.Math.GetValueAsString(ei.Get("BILL_TO"));
                            string billToNm = getBillToNm(billTo, cmp, "TPV");
                            if (!string.IsNullOrEmpty(billToNm))
                                ei.Put("BILL_TO_NAME", billToNm);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            sql = string.Format(@"INSERT INTO SMBIM_FSSP (U_ID,SHIPMENT_ID,STATUS,INV_PAY_CHECK,TPV_DEBIT_NO,DEBIT_DATE,GROUP_ID,CMP,STN)
SELECT U_ID,SHIPMENT_ID,STATUS,INV_PAY_CHECK,TPV_DEBIT_NO,DEBIT_DATE,GROUP_ID,CMP,STN FROM SMBIM  WHERE U_ID ={0}", SQLUtils.QuotedStr(ei.Get("U_ID")));
                            mixList.Add(sql);
                        }
                        mixList.Add(ei);

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            isDel = true;
                            u_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (u_id != "")
                            {
                                string smbidSql = string.Format(@"SELECT * FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
                                DataTable dt = OperationUtils.GetDataTable(smbidSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                EditInstruct updateEi;
                                foreach (DataRow dr in dt.Rows)
                                {
                                    string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                                    updateEi = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                                    updateEi.PutKey("U_ID", uid);
                                    updateEi.Put("U_FID", null);
                                    updateEi.Put("DEBIT_NO", null);
                                    updateEi.Put("VOID_USER", UserId);
                                    VoidSmbid(updateEi, uid);
                                    mixList.Add(updateEi);
                                }
                                resultInfo = SendDel2Fssp(u_id);
                            }
                        }
                        else if (!string.IsNullOrEmpty(u_id) && !string.IsNullOrEmpty(DebitNo))
                        {
                            upDebitNo = string.Format("UPDATE SMBID SET DEBIT_NO={1} WHERE U_FID={0}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(DebitNo));
                        }
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbidModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        string d_uid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string QtData = Prolink.Math.GetValueAsString(ei.Get("QT_DATA"));
                            
                            if(QtData == "Y")
                            {
                                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                                ei.PutKey("U_ID", d_uid);
                            }
                            else
                            {
                                d_uid = System.Guid.NewGuid().ToString();
                                ei.Put("U_ID", d_uid);
                            }
                            
                            ei.Put("U_FID", u_id);
                            ei.Put("DEBIT_NO", DebitNo);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (ei.Get("U_ID") == "")
                            {
                                continue;
                            } 
                            ei.OperationType = EditInstruct.UPDATE_OPERATION;
                            ei.AddKey("U_ID");
                            ei.Put("DEBIT_NO", "");
                            ei.Put("U_FID", "");
                            ei.Put("U_FID", null);
                            ei.Put("VOID_USER", this.UserId);
                            VoidSmbid(ei, ei.Get("U_ID"));
                        }
                        if (!bidList.Contains(d_uid)&&!string.IsNullOrEmpty(d_uid))
                            bidList.Add(d_uid);
                        mixList.Add(ei);
                    }
                }
            }

            if (bidList.Count > 0)
            {
                string bidSql = string.Format("SELECT U_FID,SHIPMENT_ID,CHG_CD FROM SMBID WHERE U_ID IN {0}", SQLUtils.Quoted(bidList.ToArray()));
                DataTable bDt = OperationUtils.GetDataTable(bidSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                foreach (DataRow bDr in bDt.Rows)
                {
                    string ufid = Prolink.Math.GetValueAsString(bDr["U_FID"]);
                    string shipmentId = Prolink.Math.GetValueAsString(bDr["SHIPMENT_ID"]);
                    string chgCd = Prolink.Math.GetValueAsString(bDr["CHG_CD"]);
                    if (ufid == u_id || string.IsNullOrEmpty(ufid))
                        continue;
                    return Json(new { message = "Shipment ID:" + shipmentId + ";Charge CD:" + chgCd + ";" + Resources.Locale.L_ActManage_BillNoExists, message1 = returnMessage });
                }
            }
            if (!resultInfo.IsSucceed)
                return Json(new { message = Resources.Locale.L_FSSP_SaveFail + "\n" + resultInfo.Description, IsDel = false });

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = string.Format("SELECT SHIPMENT_ID,CNTR_NO,CNTR_INFO FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
                    DataTable bidDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    List<string> shipmentList = new List<string>();
                    List<string> cntrList = new List<string>();
                    foreach (DataRow dr in bidDt.Rows)
                    {
                        string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        string cntrNo = Prolink.Math.GetValueAsString(dr["CNTR_INFO"]);
                        if (!string.IsNullOrEmpty(shipmentId) && !shipmentList.Contains(shipmentId))
                            shipmentList.Add(shipmentId);
                        if (!string.IsNullOrEmpty(cntrNo) && !cntrList.Contains(cntrNo))
                            cntrList.Add(cntrNo);
                    }
                    if (shipmentList.Count > 0 || cntrList.Count > 0)
                    {
                        string shipmentInfo = string.Join(",", shipmentList);
                        string cntrInfo = string.Join(",", cntrList);
                        if (shipmentInfo.Length > 1000)
                            shipmentInfo = shipmentInfo.Substring(0, 1000);
                        if (cntrInfo.Length > 1000)
                            cntrInfo = cntrInfo.Substring(0, 1000);
                        string upInfoSQL = string.Format("UPDATE SMBIM SET SHIPMENT_INFO={0},CNTR_INFO={1} WHERE U_ID={2}", SQLUtils.QuotedStr(shipmentInfo), SQLUtils.QuotedStr(cntrInfo), SQLUtils.QuotedStr(u_id));
                        OperationUtils.ExecuteUpdate(upInfoSQL, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    foreach (string shipment in shipmentList)
                        InboundTransfer.UpdateBillInfoToSMORD(shipment, "", null);
                    #region 修正账单号码
                    try
                    {
                        if (!string.IsNullOrEmpty(upDebitNo))
                            OperationUtils.ExecuteUpdate(upDebitNo, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch { }
                    #endregion

                    if ("Y".Equals(Request["recal_amt"]))
                        Business.TPV.Financial.Bill.SumAmt(u_id, "");
                }
                catch (Exception ex)
                {
                    resultInfo.Description += ex.ToString();
                    resultInfo.IsSucceed = false;
                }
            }

            sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");
            data["IsDel"] = true;
            if (isDel)
                data["message"] = Resources.Locale.L_BSCSSetup_DelSuccess + "\n" + resultInfo.Description;
            return ToContent(data);
        }

        public ResultInfo SendDel2Fssp(string uid)
        {
            ResultInfo returnResult = new ResultInfo();
            returnResult.IsSucceed = true;
            string sql = string.Format("SELECT STATUS,INV_PAY_CHECK,TPV_DEBIT_NO,CMP,STN FROM SMBIM  WHERE U_ID ={0}", SQLUtils.QuotedStr(uid));
            DataTable bimDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow dr in bimDt.Rows)
            {
                string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                string invPayCheck = Prolink.Math.GetValueAsString(dr["INV_PAY_CHECK"]);
                string tpvDebitNo = Prolink.Math.GetValueAsString(dr["TPV_DEBIT_NO"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string stn = Prolink.Math.GetValueAsString(dr["STN"]);
                if (TrackingEDI.Manager.CheckFSSPSite(cmp))
                {
                    ResultInfo result = WebApiEdiHandle.SendVoidINVEdi(tpvDebitNo, UserId, cmp, stn);
                    if (!result.IsSucceed)
                        returnResult.IsSucceed = false;
                    returnResult.Description = Resources.Locale.L_FSSP_VoidINVEdiMsg + result.Description + "\n";
                    if ("R".Equals(status))
                    {
                        result = WebApiEdiHandle.SendRejectINVEdi(tpvDebitNo, UserId, cmp, stn);
                        if (!result.IsSucceed)
                            returnResult.IsSucceed = false;
                        returnResult.Description += Resources.Locale.L_FSSP_RejectINVEdiMsg + result.Description + "\n";
                    }
                    sql = string.Format("DELETE FROM SMBIM_FSSP WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            return returnResult;
        }


        public void VoidSmbid(EditInstruct ei,string uid)
        {
            string sql = string.Format(@"SELECT BAMT,QCUR,QAMT,GROUP_ID,CMP,DEBIT_DATE FROM SMBID WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count < 0) return;
            DataRow dr = dt.Rows[0];
            string groupId = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
            string localCur = Business.TPV.Standard.BillingManager.GetLocalCur(groupId, cmp);
            string qCur = Prolink.Math.GetValueAsString(dr["QCUR"]);
            decimal qamt = Prolink.Math.GetValueAsDecimal(dr["QAMT"]);
            decimal bamt = Prolink.Math.GetValueAsDecimal(dr["BAMT"]);
            DateTime debitDate = Prolink.Math.GetValueAsDateTime(dr["DEBIT_DATE"]);
            sql = string.Format(@"SELECT EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND FCUR={0} AND TCUR={1} AND EDATE<={2} ORDER BY EDATE DESC",
                SQLUtils.QuotedStr(qCur), SQLUtils.QuotedStr(localCur), SQLUtils.QuotedStr(debitDate.ToString("yyyy-MM-dd")));
            DataTable bserate = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            decimal exrate = 1;
            if (bserate.Rows.Count > 0)
            {
                exrate = Prolink.Math.GetValueAsDecimal(bserate.Rows[0]["EX_RATE"]);
            }
            decimal qlamt = qamt * exrate;

            ei.Put("EX_RATE", 1);
            ei.Put("QEX_RATE", exrate);
            ei.Put("LAMT", bamt);
            ei.Put("QLAMT", qlamt);
        }

        public string getBillToNm(string billTo,string cmp,string groupId)
        {
            string billToNm = "";
            if (!string.IsNullOrEmpty(billTo))
            {
                string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TDLT' AND GROUP_ID={1} AND (CMP='*' OR CMP={2}) AND CD={0}",SQLUtils.QuotedStr(billTo),
                    SQLUtils.QuotedStr(string.IsNullOrEmpty(groupId) ? GroupId : groupId), SQLUtils.QuotedStr(string.IsNullOrEmpty(cmp) ? CompanyId : cmp));
                billToNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            return billToNm;
        }
        public ActionResult GetHighActApproveData()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string baseCondition = Request.Params["baseCondition"];
            baseCondition = GetDecodeBase64ToString(baseCondition);
            string subSql = "";
            string fiftySql = string.Empty;
            bool IsPerson = false;
            if (virCondition != "")
            {
                subSql = " AND DEBIT_NO IN ( SELECT REF_NO FROM APPROVE_RECORD WHERE APPROVE_CODE <> 'VOID' AND " + virCondition + " )";
            }
            string sql = string.Format("SELECT QTM_PRI FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            string userQtmPri = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            string basecmp = GetBaseCmp();
            if (!string.IsNullOrEmpty(userQtmPri))
            {
                string[] cmps = userQtmPri.TrimEnd(';').Split(';');
                basecmp = GetBaseGroup() + string.Format(" AND CMP IN {0}", SQLUtils.Quoted(cmps));
            }
            fiftySql += " AND " + basecmp;

            if (!string.IsNullOrEmpty(baseCondition))
            {
                subSql += " AND " + baseCondition;
            }

            string table = string.Format(@"(SELECT *,'A' AS APPROVED FROM SMBIM WHERE U_ID IN(
   SELECT DISTINCT APPROVE_RECORD.REF_NO FROM APPROVE_RECORD, SMBIM WHERE SMBIM.APPROVE_TO = APPROVE_RECORD.ROLE AND SMBIM.U_ID = APPROVE_RECORD.REF_NO AND
                 APPROVE_RECORD.STATUS = '0' AND APPROVE_CODE IN ('STD_BILL','INV','INVL50K','INVL5K') AND APPROVE_RECORD.NOTICE_TO = {0})
UNION
SELECT *,'B' AS APPROVED FROM SMBIM WHERE U_ID IN(
  SELECT DISTINCT APPROVE_RECORD.REF_NO FROM APPROVE_RECORD, SMBIM WHERE SMBIM.U_ID = APPROVE_RECORD.REF_NO AND
                APPROVE_RECORD.STATUS = '1' AND APPROVE_CODE IN ('STD_BILL','INV','INVL50K','INVL5K') AND APPROVE_RECORD.APPROVE_BY = {0})) SMBIM", SQLUtils.QuotedStr(UserId));

            NameValueCollection namevaluecollection = null;
            if (IsPerson)
            {
                namevaluecollection = new NameValueCollection();
                for (int i = 0; i < Request.Params.Count; i++)
                {
                    if ("conditions".Equals(Request.Params.GetKey(i))) continue;
                    namevaluecollection.Add(Request.Params.GetKey(i), Request.Params[i]);
                }
            }

            return GetBootstrapData(table, "1=1 " + subSql + fiftySql, "*", "", namevaluecollection);
        }


        public ActionResult GetHighActApproveDataNew()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string baseCondition = Request.Params["baseCondition"];
            baseCondition = GetDecodeBase64ToString(baseCondition);
            string subSql = "";
            string fiftySql = string.Empty;
            bool IsPerson = false;
            if (virCondition != "")
            {
                subSql = " AND DEBIT_NO IN ( SELECT REF_NO FROM APPROVE_RECORD WHERE APPROVE_CODE <> 'VOID' AND " + virCondition + " )";
            }

            string sql = string.Format("SELECT USER_ID FROM APPROVE_HA WHERE AGENT_USER={0} AND CONVERT(datetime,CONVERT(char(20),GETDATE(),110)) BETWEEN AGENT_FROM AND AGENT_TO", SQLUtils.QuotedStr(UserId));
            DataTable agentDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> agentList = new List<string>();
            foreach (DataRow dr in agentDt.Rows)
            {
                string userId = Prolink.Math.GetValueAsString(dr["USER_ID"]);
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userId))
                    agentList.Add(userId);
            }
            sql = string.Empty;
            if (agentList.Count > 0)
            {
                sql = string.Format("SELECT QTM_PRI,U_ID,CMP FROM SYS_ACCT WHERE U_ID IN {0}", SQLUtils.Quoted(agentList.ToArray()));
                sql += " UNION ";
            }

            sql += string.Format("SELECT QTM_PRI,U_ID,CMP FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            DataTable userDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, List<string>> userDic = new Dictionary<string, List<string>>();
            foreach (DataRow udr in userDt.Rows)
            {
                string qtmPri = Prolink.Math.GetValueAsString(udr["QTM_PRI"]);
                string uCmp = Prolink.Math.GetValueAsString(udr["CMP"]);
                string uid = Prolink.Math.GetValueAsString(udr["U_ID"]);
                qtmPri = uCmp + ";" + qtmPri;
                List<string> cmpList = new List<string>();
                if (!userDic.ContainsKey(uid))
                {
                    userDic.Add(uid, cmpList);
                }
                else
                    cmpList = userDic[uid];
                string[] cmps = qtmPri.TrimEnd(';').Split(';');
                foreach (string cmp in cmps)
                {
                    if (!cmpList.Contains(cmp) && !string.IsNullOrEmpty(cmp))
                        cmpList.Add(cmp);
                }
                userDic[uid] = cmpList;
            }
            string noticeBaseCon = string.Empty, approveBaseCon = string.Empty;
            string basefilter = @" AND(APPROVE_RECORD.SAP_LEVEL > 50  OR APPROVE_NAME LIKE '%LEVEL Counter Sign%' OR APPROVE_NAME LIKE '%会签%' OR 
            APPROVE_RECORD.ROLE IN(SELECT ROLE FROM APPROVE_FLOW_D  WHERE APPROVE_CODE IN ('STD_BILL','INV','INVL50K','INVL5K') AND SENIOR_STAFF='Y'))";
            foreach (string key in userDic.Keys)
            {
                string noticeCon = string.Format("(APPROVE_RECORD.NOTICE_TO={0} AND SMBIM.CMP IN {1} {2})", SQLUtils.QuotedStr(key), SQLUtils.Quoted(userDic[key].ToArray()), basefilter);
                string approveCon = noticeCon;
                if (!key.Equals(UserId))
                {
                    noticeCon = string.Format("(APPROVE_RECORD.NOTICE_TO={0} AND SMBIM.CMP IN {1} {2})", SQLUtils.QuotedStr(key), SQLUtils.Quoted(userDic[key].ToArray()), basefilter);
                    approveCon = string.Format("(APPROVE_RECORD.APPROVE_BY={0} AND SMBIM.CMP IN {1} {2})", SQLUtils.QuotedStr(UserId), SQLUtils.Quoted(userDic[key].ToArray()), basefilter);
                }
                if (!string.IsNullOrEmpty(noticeBaseCon))
                    noticeBaseCon += " OR ";
                if (!string.IsNullOrEmpty(approveBaseCon))
                    approveBaseCon += " OR ";
                noticeBaseCon += noticeCon;
                approveBaseCon += approveCon;
            }
            if (!string.IsNullOrEmpty(baseCondition))
            {
                subSql += " AND " + baseCondition;
            }

            string table = string.Format(@"(SELECT *,'A' AS APPROVED FROM SMBIM WHERE APPROVE_USER IN {2} AND CONVERT(nvarchar(50), U_ID) IN(
   SELECT DISTINCT APPROVE_RECORD.REF_NO FROM APPROVE_RECORD, SMBIM WHERE SMBIM.APPROVE_TO = APPROVE_RECORD.ROLE AND CONVERT(nvarchar(50), SMBIM.U_ID) = APPROVE_RECORD.REF_NO AND
                 APPROVE_RECORD.STATUS = '0' AND APPROVE_CODE IN ('STD_BILL','INV','INVL50K','INVL5K') AND ({0}))
UNION
SELECT *,'B' AS APPROVED FROM SMBIM WHERE CONVERT(nvarchar(50), U_ID) IN(
  SELECT DISTINCT APPROVE_RECORD.REF_NO FROM APPROVE_RECORD, SMBIM WHERE CONVERT(nvarchar(50), SMBIM.U_ID) = APPROVE_RECORD.REF_NO AND
                APPROVE_RECORD.STATUS = '1' AND APPROVE_CODE IN ('STD_BILL','INV','INVL50K','INVL5K') AND ({1}))) SMBIM", noticeBaseCon, approveBaseCon, SQLUtils.Quoted(userDic.Keys.ToArray()));

            NameValueCollection namevaluecollection = null;
            if (IsPerson)
            {
                namevaluecollection = new NameValueCollection();
                for (int i = 0; i < Request.Params.Count; i++)
                {
                    if ("conditions".Equals(Request.Params.GetKey(i))) continue;
                    namevaluecollection.Add(Request.Params.GetKey(i), Request.Params[i]);
                }
            }

            string condition = "1=1 " + subSql;
            condition = GetCreateDateCondition("SMBIM", condition);

            return GetBootstrapData(table, condition, "*", "", namevaluecollection);
        }

        /// <summary>
        /// 获取分页查询账单数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetActApproveQueryData()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string baseCondition = GetDecodeBase64ToString(Request.Params["baseCondition"]);
            string subSql = "";
            string fiftySql = string.Empty;
            bool IsPerson = false;
            if (virCondition != "")
            {
                subSql = " AND DEBIT_NO IN ( SELECT REF_NO FROM APPROVE_RECORD WHERE APPROVE_CODE <> 'VOID' AND " + virCondition + " )";
            }
            string approveto = Prolink.Math.GetValueAsString(Request.Params["conditions"]);
            if (!string.IsNullOrEmpty(approveto))
            {
                if (approveto.Contains("ApproveTo=Person"))
                {
                    fiftySql = string.Format(" AND APPROVE_USER={0}", SQLUtils.QuotedStr(UserId));
                    IsPerson = true;
                }
                else if (approveto.Contains("ApproveTo=A"))
                {
                    fiftySql = string.Format(" AND DEP={0}", SQLUtils.QuotedStr(Dep));
                    //fiftySql += " AND " + GetPMSByUrpi();
                }
                else
                {
                    //fiftySql += " AND " + GetPMSByUrpi();
                }
            }
            else
            {
                //fiftySql += " AND " + GetPMSByUrpi();
            }

            if (!string.IsNullOrEmpty(baseCondition))
            {
                subSql += " AND " + baseCondition;
            }

            NameValueCollection namevaluecollection = null;
            if (IsPerson)
            {
                namevaluecollection = new NameValueCollection();
                for (int i = 0; i < Request.Params.Count; i++)
                {
                    if ("conditions".Equals(Request.Params.GetKey(i))) continue;
                    namevaluecollection.Add(Request.Params.GetKey(i), Request.Params[i]);
                }
            }
            return GetBootstrapData("SMBIM", "1=1 " + subSql + fiftySql, "*", "DNAPPROVE", namevaluecollection);
        }

        public ActionResult GetActQueryData()
        {
            string con = " 1=1 ";
            if (IOFlag == "O")
            {
                con += " AND LSP_NO=" + SQLUtils.QuotedStr(CompanyId);
            }
            else
            {
                con += " AND " + GetBaseCmp();
            }

//            string table = @"(SELECT  
//(SELECT DN_NO+',' FROM SMDN  WITH (NOLOCK) WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID  WITH (NOLOCK) WHERE SMBID.U_FID=SMBIM.U_ID AND DN_NO IS NOT NULL) FOR XML PATH('')) AS COMBINE_INFO1,
//SMBIM.* FROM SMBIM) T";
            string table = @" (SELECT  
(SELECT DN_NO+',' FROM SMDN  WITH (NOLOCK) WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID  WITH (NOLOCK) 
WHERE SMBID.U_FID=SMBIM.U_ID AND DN_NO IS NOT NULL) FOR XML PATH('')) AS COMBINE_INFO1,(SELECT TOP 1 Bl_Win FROM SMSM
WITH(NOLOCK) WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMBID  WITH (NOLOCK) 
WHERE SMBID.U_FID=SMBIM.U_ID AND DN_NO IS NOT NULL)) AS BL_WIN,
(SELECT DISTINCT INVOICE_INFO+',' FROM SMBID WHERE SMBID.U_FID=SMBIM.U_ID AND INVOICE_INFO IS NOT NULL FOR XML PATH('')) AS INVOICE_INFO,
SMBIM.* FROM SMBIM) T";
            con = GetCreateDateCondition("SMBIM", con);
            return GetBootstrapData(table, con, "*");
        }

        public ActionResult GetActAppproveData()
        {
            if (IOFlag == "O")
            {
                string con = " 1=1 AND LSP_NO=" + SQLUtils.QuotedStr(CompanyId);
                return GetBootstrapData("SMBIM", con, "*");
            }
            else
            {
                string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
                string baseCondition = GetDecodeBase64ToString(Request.Params["baseCondition"]);
                string subSql = "";
                string fiftySql = string.Empty;
                bool IsPerson = false;
                if (virCondition != "")
                {
                    subSql = " AND DEBIT_NO IN ( SELECT REF_NO FROM APPROVE_RECORD WHERE APPROVE_CODE <> 'VOID' AND " + virCondition + " )";
                }
                string approveto = Prolink.Math.GetValueAsString(Request.Params["conditions"]);

                string sql = string.Format("SELECT QTM_PRI FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
                string userQtmPri = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                string basecmp = GetBaseCmp();
                if (!string.IsNullOrEmpty(userQtmPri))
                {
                    userQtmPri = CompanyId + ";" + userQtmPri;
                    string[] cmps = userQtmPri.TrimEnd(';').Split(';');
                    basecmp = GetBaseGroup() + string.Format(" AND CMP IN {0}", SQLUtils.Quoted(cmps));
                }
                if (!string.IsNullOrEmpty(approveto)&& approveto.Contains("ApproveTo=Person"))
                {
                    fiftySql = string.Format(" AND APPROVE_USER={0}", SQLUtils.QuotedStr(UserId));
                    IsPerson = true;
                }
                else
                {
                    fiftySql += " AND " + basecmp;
                }

                if (!string.IsNullOrEmpty(baseCondition))
                {
                    subSql += " AND " + baseCondition;
                }

                NameValueCollection namevaluecollection = null;
                if (IsPerson)
                {
                    namevaluecollection = new NameValueCollection();
                    for (int i = 0; i < Request.Params.Count; i++)
                    {
                        if ("conditions".Equals(Request.Params.GetKey(i))) continue;
                        namevaluecollection.Add(Request.Params.GetKey(i), Request.Params[i]);
                    }
                }
                string table = @"SMBIM";
                string condition = "1=1 " + subSql + fiftySql;
                condition = GetCreateDateCondition("SMBIM", condition);
                return GetBootstrapData(table, condition, "*", "DNAPPROVE", namevaluecollection);
            }
        }

        /// <summary>
        /// 获取帐单比对数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetActCheckQueryData()
        {
            string table = @" (SELECT  (SELECT TOP 1 Bl_Win FROM SMSM
WITH(NOLOCK) WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMBID  WITH (NOLOCK) 
WHERE SMBID.U_FID=SMBIM.U_ID AND DN_NO IS NOT NULL)) AS BL_WIN,
SMBIM.* FROM SMBIM) T";
            string condition = " STATUS IN ('B', 'C', 'G') AND " + GetBaseCmp();
            return GetBootstrapData(table, condition);
        }

        /// <summary>
        /// 获取单笔账单查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetActSetupDataItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");
            return ToContent(data);
        }
        #endregion

        #region 帐单发送
        public JsonResult CheckInvoice()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string InvNo = Prolink.Math.GetValueAsString(Request.Params["TpvInvNo"]);
            string cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            bool result = false;
            string msg = string.Empty;
            if (string.IsNullOrEmpty(UId))
            {
                msg = @Resources.Locale.L_ActManageController_Controllers_0;
            }
            else
            {
                if (TrackingEDI.Manager.CheckFSSPSite(cmp))
                {
                    ResultInfo EdiResult = Business.TPV.WebApiEdiHandle.SendInvoiceCheckEdi(InvNo, UserId, cmp);
                    if (EdiResult.IsSucceed)
                    {
                        ActHelper.UpdateInvPayCheck(UId, "Y");
                        result = true;
                        if (EdiResult.Description.StartsWith("5:"))
                            msg = EdiResult.Description.Substring(2, EdiResult.Description.Length - 2);
                    }
                    else
                    {
                        ActHelper.UpdateInvPayCheck(UId, "N");
                        msg = EdiResult.Description;
                    }
                }
                else
                    result = true;
            }
            return Json(new { result, msg });
        }

        public JsonResult sendAct()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string msg = "success";
            string sql = "";
            if (string.IsNullOrEmpty( UId ))
            {
                msg = @Resources.Locale.L_ActManageController_Controllers_0;
            }
            else
            {
                Business.ActHelper.ChkAct(UId);//无论符合不符合都变通过  因为带入费用时是只抓取通过的
                sql = "SELECT TOP 1 APPROVE_BY FROM SMBID WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                DataTable dt = getDataTableFromSql(sql);
                string ApproveBy = string.Empty;
                if(dt.Rows.Count > 0)
                {
                    ApproveBy = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_BY"]);
                }
                DateTime odt = DateTime.Now;                
                DateTime ndt =Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                sql = "UPDATE SMBIM SET STATUS='D', VERIFY_BY={0}, VERIFY_DATE={1}, VERIFY_DATE_L={2} WHERE U_ID={3}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ApproveBy), SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(UId));
                try
                {
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }

                sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(UId));
                DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                Dictionary<string, object> data = new Dictionary<string, object>();
                data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
                data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");
                return Json(new { message = msg, returnData = ToContent(data) });
            }

            return Json(new { message = msg });
        }

        /// <summary>
        /// 作废了  做法已改变
        /// </summary>
        /// <returns></returns>
        public JsonResult sendAct1()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string msg = "success";
            string sql = "";
            if (UId == "")
            {
                msg = @Resources.Locale.L_ActManageController_Controllers_0;
            }
            else
            {
                msg = Business.ActHelper.ChkAct(UId); 
                if (msg == "success")
                {
                    sql = "UPDATE SMBIM SET STATUS='G' WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                    try
                    {
                        OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        //若全数符合则自动通过
                        /*
                        sql = "SELECT COUNT(*) FROM SMBID WHERE STATUS='N' AND U_FID=" + SQLUtils.QuotedStr(UId);
                        int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (num == 0)
                        {
                            msg = Business.ActHelper.CreateApprove(UId, CompanyId, UserId, Dep, GetBaseCmp(), UPri);
                        }
                         */
                    }
                    catch (Exception ex)
                    {
                        msg = ex.ToString();
                    }
                }
                else if (msg == "no pass")
                {
                    sql = "UPDATE SMBIM SET STATUS='B' WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                    try
                    {
                        OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        msg = ex.ToString();
                    }
                }

                sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = string.Format("SELECT * FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(UId));
                DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                Dictionary<string, object> data = new Dictionary<string, object>();
                data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
                data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");
                return Json(new { message = msg, returnData = ToContent(data) });
            }

            return Json(new { message = msg});
        }
        #endregion

        public  Dictionary<string,string>GetLang(){
            Dictionary<string,string> lang = new Dictionary<string,string>();
            lang.Add("L_ActCheck_Views_0", Resources.Locale.L_ActCheck_Views_0);
            lang.Add("L_ActManage_Controllers_70", Resources.Locale.L_ActManage_Controllers_70);
            lang.Add("L_ActManage_Controllers_69", Resources.Locale.L_ActManage_Controllers_69);
            lang.Add("L_ActManage_Controllers_85", Resources.Locale.L_ActManage_Controllers_85);
            lang.Add("L_ActManage_Controllers_86", Resources.Locale.L_ActManage_Controllers_86);
            lang.Add("L_ActManage_Controllers_87", Resources.Locale.L_ActManage_Controllers_87);
            lang.Add("L_ActManageController_Controllers_13", Resources.Locale.L_ActManageController_Controllers_13);
            lang.Add("L_ActManageController_Controllers_14", Resources.Locale.L_ActManageController_Controllers_14);
            lang.Add("L_ActManageController_Currency", Resources.Locale.L_ActManageController_Currency);
            lang.Add("L_ActManageController_ChargeCode", Resources.Locale.L_ActManageController_ChargeCode);
            lang.Add("L_ActManageController_ChargeDesc", Resources.Locale.L_ActManageController_ChargeDesc);
            lang.Add("L_ActManageController_ChargeNotExist", Resources.Locale.L_ActManageController_ChargeNotExist);
            lang.Add("L_SMSMI_CntrNoCheck", Resources.Locale.L_SMSMI_CntrNoCheck);
            lang.Add("L_SMSMI_CntrCountCheck", Resources.Locale.L_SMSMI_CntrCountCheck);
            lang.Add("L_DNManage_RefNlCtEt", Resources.Locale.L_DNManage_RefNlCtEt);
            lang.Add("L_ActUpdate_Scripts_80", Resources.Locale.L_ActUpdate_Scripts_80);
            return lang;
        }

        #region 通过与拒绝 action
        public JsonResult ActPass()
        {
            string msg = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                Upri = UPri,
                Dep = Dep,
                basecondtions = GetBaseCmp()
            };
            msg = Business.ActHelper.CreateApprove(UId, userinfo, CompanyId);

            string sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(UId));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SendOpenActMail(UId, mainDt);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");
            return Json(new { message = msg, returnData = ToContent(data) });
        }
        public JsonResult ActPassMuit()
        {
            string msg = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string[] uid = UId.Split(',');
            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                Upri = UPri,
                Dep = Dep,
                basecondtions = GetBaseCmp()
            };
            foreach (string id in uid)
            {
                string _msg = string.Empty;
                _msg = Business.ActHelper.CreateApprove(id, userinfo, CompanyId);

                string sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(id));
                DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (!_msg.Equals("success"))
                {
                    if (mainDt != null && mainDt.Rows.Count > 0)
                        msg += mainDt.Rows[0]["JOB_NO"] + ":" + _msg;
                    else
                        msg += _msg;
                }
                SendOpenActMail(id, mainDt);
            }
            return Json(new { message = msg });
        }
        /// <summary>
        /// 发送开票通知mail
        /// </summary>
        /// <param name="UId"></param>
        /// <param name="mainDt"></param>
        private void SendOpenActMail(string UId, DataTable mainDt)
        {
            string LspNo = string.Empty;
            string DebitNo = string.Empty;
            if (mainDt.Rows.Count > 0)
            {
                LspNo = Prolink.Math.GetValueAsString(mainDt.Rows[0]["LSP_NO"]);
                DebitNo = Prolink.Math.GetValueAsString(mainDt.Rows[0]["DEBIT_NO"]);
            }

            DataTable mailGroupDt = MailTemplate.GetMailGroup(LspNo, GroupId, "IG");
            if (mailGroupDt.Rows.Count > 0)
            {
                foreach (DataRow item1 in mailGroupDt.Rows)
                {
                    string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                    if (mailStr != "")
                    {
                        EvenFactory.AddEven(UId + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString() + "#" + MailManager.InboundBILLPassNotify, UId, MailManager.InboundBILLPassNotify, null, 1, 0, mailStr, @Resources.Locale.L_ActManageController_Controllers_1 + DebitNo + @Resources.Locale.L_ActManageController_Controllers_2, "");
                    }
                }
            }
        }

        public JsonResult ActReject()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string CheckDescp = Prolink.Math.GetValueAsString(Request.Params["CheckDescp"]);
            string Remark = Prolink.Math.GetValueAsString(Request.Params["Remark"]);
            string msg = "success";

            DateTime odt = DateTime.Now;            
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            string sql = "UPDATE SMBIM SET STATUS='C',APPROVE_USER=NULL, CHECK_DESCP={0}, REMARK={1}, VERIFY_CMP={2}, VERIFY_BY={3}, VERIFY_DATE={4}, VERIFY_DATE_L={5} WHERE U_ID={6}";
            sql = string.Format(sql, SQLUtils.QuotedStr(CheckDescp), SQLUtils.QuotedStr(Remark), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(UId));
            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                BillApproveHelper.SendBillRJMail(UId, GroupId, CompanyId);
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }

            sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMBID WHERE U_FID={0}", SQLUtils.QuotedStr(UId));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");

            return Json(new { message = msg, returnData = ToContent(data) });
        }
        #endregion

        #region 取得签核角色
        private void SetRoleSelect()
        {
            ViewBag.SelectRole = "";
            ViewBag.DefaultRole = "";

            #region Approve
            //string sql = @"SELECT APPROVE_GROUP,GROUP_DESCP FROM APPROVE_ATTR_D WHERE APPROVE_ATTR='BILLING' AND CMP={0} ";
            //sql = string.Format(sql, SQLUtils.QuotedStr(CompanyId));
            string sql = string.Format(@"SELECT APPROVE_GROUP,GROUP_DESCP FROM APPROVE_ATTR_D WHERE U_FID=(select U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR='BILLING' AND {0}) AND {1}
            ORDER BY SEQ_NO ASC", GetBaseCmp(), GetBaseCmp());


            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            string approvegroup = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                approvegroup = Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                if ("AM".Equals(approvegroup) || "AMM".Equals(approvegroup) || "AMMM".Equals(approvegroup))
                    continue;
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultRole = approvegroup;
                }
                /*
                string ap = Prolink.Math.GetValueAsString(dr["ROLE"]);
                if (ap == "A")
                {
                    ap = "LST";
                }
                else if (ap == "AM")
                {
                    ap = "LSTM";
                }
                 */
                //select += ap;
                select += approvegroup;
                select += ":" + Prolink.Math.GetValueAsString(dr["GROUP_DESCP"]);
            }
            select += ";Finish:Finish";
            ViewBag.SelectRole = select;
            #endregion
        }

        private void SetApproveSelect()
        {
            ViewBag.SelectApprove = "";
            ViewBag.DefaultApprove = "";

            string approveroles = BillApproveHelper.GetApproveBack(UserId, CompanyId, GroupId, UPri, Dep);
            ViewBag.SelectApprove = approveroles;

            //#region Approve
            //string sql = string.Format("SELECT APPROVE_CODE,APPROVE_NAME FROM APPROVE_FLOW_M WHERE AU_ID='0F3C2DFC-9286-474B-80A4-204C0E155016' AND GROUP_ID={0} AND CMP_ID={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //string select = string.Empty;
            //foreach (DataRow dr in dt.Rows)
            //{
            //    if (select.Length > 0)
            //    {
            //        select += ";";
            //    }
            //    else
            //    {
            //        ViewBag.DefaultApprove = Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
            //    }
            //    select += Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
            //    select += ":" + Prolink.Math.GetValueAsString(dr["APPROVE_NAME"]);
            //}
            //ViewBag.SelectApprove = select;
            //#endregion
        }
        #endregion

        #region 取得user权限
        private void GetUseAcct()
        {
            string sql = string.Format("SELECT U_PRI FROM SYS_ACCT WHERE U_ID={0} AND {1}", SQLUtils.QuotedStr(UserId), GetBaseDep());
            string upri = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.Upri = upri;
        }
        #endregion

        #region 签核明细
        //签核明细
        public ActionResult GetApproveInfo()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string sql = string.Format("SELECT  APPROVE_RECORD.*, ISNULL((SELECT TOP 1 APPROVE_NAME FROM APPROVE_FLOW_M WHERE APPROVE_FLOW_M.APPROVE_CODE=APPROVE_RECORD.APPROVE_CODE AND APPROVE_FLOW_M.CMP_ID={0}),APPROVE_RECORD.APPROVE_CODE) AS APPROVE_CODENAME," +
                "ISNULL((SELECT TOP 1 GROUP_DESCP FROM APPROVE_FLOW_D WHERE APPROVE_FLOW_D.APPROVE_CODE=APPROVE_RECORD.APPROVE_CODE AND APPROVE_FLOW_D.ROLE= APPROVE_RECORD.ROLE AND APPROVE_FLOW_D.CMP_ID={0}),APPROVE_RECORD.ROLE) AS APPROVE_ROLE " +
                " FROM APPROVE_RECORD WHERE REF_NO={1} ORDER BY VOID_LOOP DESC, APPROVE_CODE DESC, CAST(APPROVE_LEVEL AS INT) ASC",
                SQLUtils.QuotedStr(Cmp),SQLUtils.QuotedStr(UId));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(groupDt, "ApproveRecordModel"));
        }
        #endregion

        #region Approve签核通过操作
        static object _lockObj = new object();
        public ActionResult ApproveBill()
        {
            string returnMsg = "";
            string isOk = "Y";
            string approveRemark = Prolink.Math.GetValueAsString(Request.Params["ApproveRemark"]);
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string tvMnt = Prolink.Math.GetValueAsString(Request.Params["TvMnt"]);
            if (string.IsNullOrEmpty(uid))
            {
                returnMsg = @Resources.Locale.L_ActManageController_Controllers_4;
            }
            else
            {
                string[] uids = uid.Split(',');
                string sql = string.Format("SELECT AMT,QAMT,TPV_DEBIT_NO,REMARK_S,APPROVE_TO,CMP,U_ID FROM SMBIM WHERE U_ID IN {0}", SQLUtils.Quoted(uids));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, string> cmpdic = new Dictionary<string, string>();
                foreach (DataRow dr in dt.Rows)
                {
                    float amt = Prolink.Math.GetValueAsFloat(dr["AMT"]);
                    float qamt = Prolink.Math.GetValueAsFloat(dr["QAMT"]);
                    string tpvDebitNo = Prolink.Math.GetValueAsString(dr["TPV_DEBIT_NO"]);
                    string remarks = Prolink.Math.GetValueAsString(dr["REMARK_S"]);
                    string approveTo = Prolink.Math.GetValueAsString(dr["APPROVE_TO"]);
                    string CMP = Prolink.Math.GetValueAsString(dr["CMP"]);
                    string uidtemp = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    if (amt != qamt && string.IsNullOrEmpty(remarks) && "A".Equals(approveTo))
                    {
                        returnMsg += tpvDebitNo + ":" + @Resources.Locale.L_ActSetup_CheckSubAmt + "\n";
                        isOk = "N";
                    }
                    if (!cmpdic.ContainsKey(uidtemp))
                        cmpdic.Add(uidtemp, CMP);
                }
                if (string.IsNullOrEmpty(returnMsg))
                {
                    for (int i = 0; i < uids.Length; i++)
                    {
                        lock (_lockObj)
                        {
                            uid = uids[i].ToString();
                            UserInfo userinfo = new UserInfo
                            {
                                UserId = UserId,
                                CompanyId = CompanyId,
                                GroupId = GroupId,
                                Upri = UPri,
                                Dep = Dep,
                                basecondtions = GetBaseCmp(),
                                DataCmp = cmpdic.ContainsKey(uid) ? cmpdic[uid] : CompanyId
                            };

                            //returnMsg += Business.BillApproveHelper.ApproveBillItem(uid, DebitNo,userinfo, approveRemark) + "\n";
                            Business.BillApproveNew billApproveNew = new BillApproveNew(userinfo, uid, tvMnt);
                            returnMsg += billApproveNew.ApproveBillItem(approveRemark);
                        }
                    }
                }
                SetBimTask(uids);
            }
            return Json(new { message = returnMsg,IsOk = isOk });
        }
        #endregion

        #region 签核退回操作
        public ActionResult ApproveBackBill()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DebitNo"]);
            string ApproveType = Prolink.Math.GetValueAsString(Request.Params["ApproveType"]);
            string ApproveTo = Prolink.Math.GetValueAsString(Request.Params["ApproveTo"]);
            string backremark = Prolink.Math.GetValueAsString(Request.Params["BackRemark"]);
            string cmp = OperationUtils.GetValueAsString("SELECT TOP 1 CMP FROM SMBIM WHERE U_ID =" + SQLUtils.QuotedStr(uid), Prolink.Web.WebContext.GetInstance().GetConnection());

            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                Upri = UPri,
                Dep = Dep,
                basecondtions = GetBaseCmp(),
                DataCmp = string.IsNullOrEmpty(cmp) ? CompanyId : cmp
            };
            string message = BillApproveHelper.BillApproveBack(uid, backremark, userinfo);
            string sql = string.Format("SELECT DISTINCT SHIPMENT_ID FROM SMBID WHERE U_FID={0} AND DEC_NO IS NOT NULL", SQLUtils.QuotedStr(uid));
            DataTable bidDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in bidDt.Rows)
            {
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                InboundTransfer.UpdateBillInfoToSMORD(shipmentId, "", null);
            }
            SetBimTask(new string[] { uid });
            return Json(new { message = message });
        }
        #endregion

        #region 作废
        public JsonResult doInvalid()
        {
            string uid = Request.Params["UId"];
            string msg = "success";
            string sql = "";

            if(uid != "")
            {
                sql = "SELECT STATUS FROM SMBIM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                string status = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                if ("D".Equals(status) || "E".Equals(status) || "F".Equals(status))
                {
                    msg = @Resources.Locale.L_ActManageController_Controllers_5;
                }
                else
                {
                    sql = "UPDATE SMBIM SET STATUS='V' WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                    try
                    {
                        OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        msg = ex.ToString();
                    }
                }
            }
            else
            {
                msg = @Resources.Locale.L_ActManageController_Controllers_6;
            }

            return Json(new { message = msg});

        }
        #endregion

        #region Statement & Un-Statement
        public JsonResult doStatement()
        {
            string s_no = Prolink.Math.GetValueAsString(Request.Params["StatementNo"]);
            int month = Prolink.Math.GetValueAsInt(Request.Params["Month"]);
            string v_no = Prolink.Math.GetValueAsString(Request.Params["VatNo"]);

            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string con = "";
            for (int i = 0; i < uidArray.Length; i++)
            {
                if (i == 0)
                {
                    con += SQLUtils.QuotedStr(uidArray[i]);
                }
                else
                {
                    con += "," + SQLUtils.QuotedStr(uidArray[i]);
                }
            }

            string msg = "success";
            string sql = string.Empty;
            if (con != "")
            {
                sql = "UPDATE SMBIM SET STATEMENT_NO={0}, B_MONTH={1}, VAT_NO={2} WHERE U_ID IN ({3})";
                sql = string.Format(sql, SQLUtils.QuotedStr(s_no), month, SQLUtils.QuotedStr(v_no), con);
                try
                {
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }

            return Json(new {message = msg });
        }

        public JsonResult unStatement()
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string con = "";
            for (int i = 0; i < uidArray.Length; i++)
            {
                if (i == 0)
                {
                    con += SQLUtils.QuotedStr(uidArray[i]);
                }
                else
                {
                    con += "," + SQLUtils.QuotedStr(uidArray[i]);
                }
            }

            string msg = "success";
            string sql = string.Empty;
            if (con != "")
            {
                sql = "UPDATE SMBIM SET STATEMENT_NO='', B_MONTH=NULL, VAT_NO='' WHERE U_ID IN ({0})";
                sql = string.Format(sql, con);
                try
                {
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }

            return Json(new { message = msg });
        }

        #endregion

        #region 付款確認
        public JsonResult doPayed()
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            ids=ids.Trim(',');
            string[] uidArray = ids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            string paydate = Prolink.Math.GetValueAsString(Request["PayDate"]);
            string sql = string.Empty;
            string msg = "success";
            for (int i = 0; i < uidArray.Length; i++)
            {
                if (uidArray[i] != "")
                {
                    sql = "SELECT APPROVE_TO FROM SMBIM WHERE U_ID=" + SQLUtils.QuotedStr(uidArray[i]);
                    string ap_status = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = "SELECT DEBIT_NO FROM SMBIM WHERE U_ID=" + SQLUtils.QuotedStr(uidArray[i]);
                    string debit_no = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (ap_status == "Finish")
                    {
                        sql = string.Format("UPDATE SMBIM SET STATUS='F',PAID_DATE={0} WHERE U_ID={1}",
                            SQLUtils.QuotedStr(paydate), SQLUtils.QuotedStr(uidArray[i]));
                        try
                        {
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            msg = ex.ToString();
                        }
                    }
                    else
                    {
                        msg = @Resources.Locale.L_ActManageController_Controllers_7 + debit_no + @Resources.Locale.L_ActManageController_Controllers_8 + "\n";
                    }
                }
                else
                {
                    msg = @Resources.Locale.L_ActManageController_Controllers_9 + "\n";
                    break;
                }
            }
            

            return Json(new { message = msg});
        }
        #endregion

        #region 帐单excel转入
        [HttpPost]
        public ActionResult UploadChg(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList mx = new MixedList();
            string returnMessage = "success";
            string msg = string.Empty;
            string sql = string.Empty;
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);
            string autoChk = Prolink.Math.GetValueAsString(Request.Params["autoChk"]);
            string LspNo = CompanyId;
            sql = "SELECT CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(LspNo);
            string LspNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            

            if(IOFlag == "I")
            {
                return Json(new { message = @Resources.Locale.L_ActManageController_Controllers_10 });
            }

            List<Dictionary<string, object>> smdnpData = new List<Dictionary<string, object>>();
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            else
            {
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                #region 舊方法
                /*
                Dictionary<string, object> parm = new Dictionary<string, object>();
                string mapping = "ImportBill";
                ExcelParser.RegisterEditInstructFunc(mapping, ActHelper.HandleBillDetail);
                ExcelParser ep = new ExcelParser();

                //DataTable srcDt = ExcelHelper.ImportExcelToDataTable(excelFileName);
                if (autoChk == "")
                    autoChk = "false";
                parm["autoChk"] = autoChk;
                parm["LspNo"] = UserId;
                string sql = "SELECT TOP 1 CONCAT(PARTY_NAME, ' ', PARTY_NAME2) AS PARTY_NAME FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(UserId);
                string LspNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                parm["LspNm"] = LspNm;
                parm["msg"] = string.Empty;
                parm["smbimUid"] = string.Empty;
                parm["newDebitNo"] = string.Empty;
                parm["seq"] = 0;
                ep.Save(mapping, excelFileName, ml, parm, StartRow);
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                string smbimUid = Prolink.Math.GetValueAsString(parm["smbimUid"]);
                if (autoChk == "Y")
                {
                    string chk = ActHelper.ChkAct(smbimUid); //帐单比对
                    #region 比對後更新主檔狀態
                    if (chk == "success")
                    {
                        sql = "UPDATE SMBIM SET STATUS='G' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                        try
                        {
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            chk = ex.ToString();
                        }
                    }
                    else if (chk == "no pass")
                    {
                        sql = "UPDATE SMBIM SET STATUS='B' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                        try
                        {
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            chk = ex.ToString();
                        }
                    }
                    #endregion
                }
                 */
                #endregion
                
                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);

                EditInstruct ei;
                EditInstruct ei1;
                EditInstruct ei2;
                int[] resulst;
                
                /*
                 * dr[0]: CMP
                 * dr[1]: SHIPMENT_ID
                 * dr[2]: COMBINE_INFO
                 * dr[3]: BL_NO
                 * dr[4]: DEBIT_DATE
                 * dr[5]: DEBIT_NO
                 * dr[6]: Cur
                 * dr[7~.....]: 一堆費用代碼
                 */
                try
                {
                    string o_dbno = string.Empty;
                    string smbimUid = string.Empty;
                    string new_debitno = string.Empty;
                    string Cur = string.Empty;
                    ArrayList ChgCdArray = new ArrayList();
                    ArrayList ChgDescpArry = new ArrayList();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        if (i == 0)
                        {
                            for (int j = 0; j < dr.ItemArray.Length; j++)
                            {

                                if (j < 7)
                                {
                                    ChgCdArray.Add("xxx");
                                    ChgDescpArry.Add("xxx");
                                }
                                else
                                {
                                    string chg = Prolink.Math.GetValueAsString(dr[j]);
                                    if (chg == "")
                                    {
                                        break;
                                    }
                                    string[] chgArray = chg.Split('-');
                                    string ChgCd = chgArray[0];
                                    string ChgDescp = chgArray[1];
                                    ChgCdArray.Add(ChgCd);
                                    ChgDescpArry.Add(ChgDescp);
                                }
                            }
                            continue;
                        }
                        else
                        {
                            string Cmp = Prolink.Math.GetValueAsString(dr[0]);
                            string ShipmentId = Prolink.Math.GetValueAsString(dr[1]);
                            string CombineInfo = Prolink.Math.GetValueAsString(dr[2]);
                            string BlNo = Prolink.Math.GetValueAsString(dr[3]);
                            DateTime DebitDate = Prolink.Math.GetValueAsDateTime(dr[4]);
                            string DebitNo = Prolink.Math.GetValueAsString(dr[5]);
                            if (i == 1)
                            {
                                Cur = Prolink.Math.GetValueAsString(dr[6]);
                            }

                            #region 检查必填栏位Shipment ID, Debit Date
                            if (ShipmentId == "")
                            {
                                string localMsg = @Resources.Locale.L_ActManageController_Controllers_11;
                                localMsg = string.Format(localMsg, i);
                                msg += localMsg;
                                continue;
                            }

                            if (DebitDate == null)
                            {
                                string localMsg = @Resources.Locale.L_ActManageController_Controllers_13;
                                localMsg = string.Format(localMsg, i);
                                msg += localMsg;
                                continue;
                            }

                            if (Cmp == "")
                            {
                                string localMsg = @Resources.Locale.L_ActManageController_Controllers_14;
                                localMsg = string.Format(localMsg, i);
                                msg += localMsg;
                                continue;
                            }
                            #endregion

                            #region 创建表头
                            if (o_dbno != DebitNo || (DebitNo == "" && i == 1))
                            {
                                if (i > 1 && autoChk == "Y")
                                {
                                    string chk = ActHelper.ChkAct(smbimUid); //帐单比对
                                    #region 比對後更新主檔狀態
                                    if (chk == "success")
                                    {
                                        sql = "UPDATE SMBIM SET STATUS='G' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                                        try
                                        {
                                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        }
                                        catch (Exception ex)
                                        {
                                            
                                        }
                                    }
                                    else if (chk == "no pass")
                                    {
                                        sql = "UPDATE SMBIM SET STATUS='B' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                                        try
                                        {
                                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    #endregion
                                }

                                if (i > 1)
                                {
                                    sql = "SELECT SUM(QAMT) AS TTL_QAMT, SUM(BAMT) AS TTL_BAMT FROM SMBID WHERE U_FID=" + SQLUtils.QuotedStr(smbimUid);
                                    DataTable sumDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    if (sumDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow item in sumDt.Rows)
                                        {
                                            decimal TtlQamt = Prolink.Math.GetValueAsDecimal(item["TTL_QAMT"]);
                                            decimal TtlBamt = Prolink.Math.GetValueAsDecimal(item["TTL_BAMT"]);
                                            decimal SubAmt = TtlBamt - TtlQamt;

                                            sql = "SELECT TOP 1 QCUR FROM SMBID WHERE U_FID=" + SQLUtils.QuotedStr(smbimUid);
                                            string cur = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                                            sql = "UPDATE SMBIM SET QAMT={0}, AMT={1}, SUB_AMT={2}, CUR={3} WHERE U_ID={4}";
                                            sql = string.Format(sql, TtlQamt, TtlBamt, SubAmt, SQLUtils.QuotedStr(cur), SQLUtils.QuotedStr(smbimUid));
                                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        }
                                    }
                                }

                                if (DebitNo == "")
                                {
                                    smbimUid = System.Guid.NewGuid().ToString();
                                    new_debitno = Business.ReserveManage.getAutoNo("DEBIT_NO", "TPV", Cmp);
                                    DebitNo = new_debitno;
                                    EditInstruct m_ei = ActHelper.CreateBillEditInstruct(smbimUid, LspNo, LspNm, new_debitno, "TPV", Cmp, "");
                                    try
                                    {
                                        int[] result = OperationUtils.ExecuteUpdate(m_ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    }
                                    catch (Exception ex)
                                    {
                                        msg += @Resources.Locale.L_ActManage_Controllers_33 + "\n";
                                        break;
                                    }
                                }
                                else
                                {
                                    string smbim_isexist = ActHelper.easyCheckSmbim(DebitNo, LspNo, Cmp);
                                    if (smbim_isexist != "")
                                    {
                                        smbimUid = smbim_isexist;
                                    }
                                    else
                                    {
                                        smbimUid = System.Guid.NewGuid().ToString();
                                        EditInstruct m_ei = ActHelper.CreateBillEditInstruct(smbimUid, LspNo, LspNm, DebitNo, "TPV", Cmp, "");
                                        try
                                        {
                                            int[] result = OperationUtils.ExecuteUpdate(m_ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        }
                                        catch (Exception ex)
                                        {
                                            msg += @Resources.Locale.L_ActManage_Controllers_33 + "\n";
                                            break;
                                        }
                                    }
                                }
                            }
                            #endregion
                            mx = new MixedList();
                            for (int j = 6; j < dr.ItemArray.Length - 1; j++)
                            {
                                decimal Bamt = Prolink.Math.GetValueAsDecimal(dr[j]);
                                string ChgCd = Prolink.Math.GetValueAsString(ChgCdArray[j]);
                                string ChgDescp = Prolink.Math.GetValueAsString(ChgDescpArry[j]);
                                /*請款金額大於0，才需要update or insert到SMBID帳單明細*/
                                if (Bamt !=0)
                                {
                                    #region 檢查費用代碼是否存在e-shipping
                                    bool chgIsExist = ActHelper.chkChgCdIsExist(ChgCd);
                                    if(chgIsExist == false)
                                    {
                                        msg += @Resources.Locale.L_ActManageController_Controllers_15 + "\n";
                                        continue;
                                    }
                                    #endregion

                                    #region 檢查shipment ID存在否
                                    sql = "SELECT COUNT(*) FROM SMSM WHERE SHIPMENT_ID={0} AND CMP={1}";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(Cmp));
                                    int sm_exist = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    if (sm_exist == 0)
                                    {
                                        msg += @Resources.Locale.L_ActManageController_Controllers_16+"\n";
                                        continue;
                                    }
                                    #endregion

                                    #region 檢查該費用是否存在費用明細，若不在則新增
                                    string smbidUid = ActHelper.chkSmbidIsExist(ShipmentId, LspNo, ChgCd);
                                    string smbidUFid = ActHelper.chkUfidIsExist(ShipmentId, LspNo, ChgCd);
                                    
                                    if (!String.IsNullOrEmpty(smbidUid) && String.IsNullOrEmpty(smbidUFid))
                                    {
                                        ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                                        ei.PutKey("U_ID", smbidUid);
                                        ei.Put("QT_DATA", "Y");
                                    }
                                    else
                                    {
                                        smbidUid = System.Guid.NewGuid().ToString();
                                        ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                                        ei.Put("U_ID", smbidUid);
                                        if (Cur != "")
                                        {
                                            ei.Put("QCUR", Cur);
                                        }
                                        ei.Put("CHG_CD", ChgCd);
                                        ei.Put("CHG_DESCP", ChgDescp);
                                    }
                                    ei.Put("U_FID", smbimUid);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.Put("BL_NO", BlNo);
                                    ei.PutDate("DEBIT_DATE", DebitDate);
                                    ei.Put("DEBIT_NO", DebitNo);
                                    ei.Put("LSP_NO", LspNo);
                                    ei.Put("LSP_NM", LspNm);
                                    ei.Put("BAMT", Bamt);
                                    mx.Add(ei);
                                    #endregion
                                }
                            }

                            try
                            {
                                int[] result = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch (Exception ex)
                            {
                                msg += @Resources.Locale.L_ActManage_Controllers_44 + "\n";
                                continue;
                            }

                            #region 最後一筆還是要判斷 表頭要不要更新
                            if (i == (dt.Rows.Count - 1))
                            {
                                if (autoChk == "Y")
                                {
                                    string chk = ActHelper.ChkAct(smbimUid); //帐单比对
                                    #region 比對後更新主檔狀態
                                    if (chk == "success")
                                    {
                                        sql = "UPDATE SMBIM SET STATUS='G' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                                        try
                                        {
                                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    else if (chk == "no pass")
                                    {
                                        sql = "UPDATE SMBIM SET STATUS='B' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                                        try
                                        {
                                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    #endregion
                                }


                                sql = "SELECT SUM(QAMT) AS TTL_QAMT, SUM(BAMT) AS TTL_BAMT FROM SMBID WHERE U_FID=" + SQLUtils.QuotedStr(smbimUid);
                                DataTable sumDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (sumDt.Rows.Count > 0)
                                {
                                    foreach (DataRow item in sumDt.Rows)
                                    {
                                        decimal TtlQamt = Prolink.Math.GetValueAsDecimal(item["TTL_QAMT"]);
                                        decimal TtlBamt = Prolink.Math.GetValueAsDecimal(item["TTL_BAMT"]);
                                        decimal SubAmt = TtlBamt - TtlQamt;

                                        sql = "SELECT TOP 1 QCUR FROM SMBID WHERE U_FID=" + SQLUtils.QuotedStr(smbimUid);
                                        string cur = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                                        sql = "UPDATE SMBIM SET QAMT={0}, AMT={1}, SUB_AMT={2}, CUR={3} WHERE U_ID={4}";
                                        sql = string.Format(sql, TtlQamt, TtlBamt, SubAmt, SQLUtils.QuotedStr(cur), SQLUtils.QuotedStr(smbimUid));
                                        OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    }
                                }
                            }
                            #endregion
                        }
                        
                    }
                }
                catch (Exception ex)
                { 
                
                }
            }

            return Json(new { message = returnMessage, excelMsg = msg });
        }
        #endregion

        #region 費用下載查詢
        public ActionResult ActDoQuery()
        {
            string ioflag = this.IOFlag;
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string bidCondition = ModelFactory.GetInquiryCondition("SMBID", "", Request.Params);
            #region 过滤条件
            string[] ccs = bidCondition.Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
            string filter = string.Empty;
            foreach (string c in ccs)
            {
                string c1 = c.Trim().ToUpper();
                if (c1.StartsWith("POL_CD") || c1.StartsWith("SCAC_CD") || c1.StartsWith("HOUSE_NO") || c1.StartsWith("SM_STATUS") || c1.StartsWith("TPV_DEBIT_NO") || c1.StartsWith("OP"))
                    continue;
                filter += " AND " + c;
            }
            #endregion
            string subSql = "";
            if (virCondition != "")
            {
                subSql = " AND SHIPMENT_ID IN ( SELECT SHIPMENT_ID FROM SMICNTR WHERE 1=1 AND " + virCondition + " )";
            }
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string table = string.Format(@"(SELECT SMBID.*,'' AS OP,(SELECT TOP 1 POL_CD FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
                (SELECT TOP 1 SCAC_CD FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID AND SMSMI.TRAN_TYPE='E') AS SCAC_CD ,
                (SELECT TOP 1 HOUSE_NO FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS HOUSE_NO ,
          (SELECT TOP 1 STATUS
          FROM SMSMI WITH(NOLOCK)
                  WHERE SMSMI.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS  ,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.U_ID=SMBID.U_FID WHERE SMBID.GROUP_ID={0} AND SMBID.LSP_NO={1} )T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));
            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                if ("G".Equals(upri))
                    innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND SMBID.CMP=" + SQLUtils.QuotedStr(this.CompanyId);
                table = string.Format(@"(SELECT SMBID.*,'' AS OP,(SELECT TOP 1 POL_CD FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
                (SELECT TOP 1 SCAC_CD FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID AND SMSMI.TRAN_TYPE='E') AS SCAC_CD ,
                (SELECT TOP 1 HOUSE_NO FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS HOUSE_NO ,
          (SELECT TOP 1 STATUS
          FROM SMSMI WITH(NOLOCK)
         WHERE SMSMI.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS  ,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.DEBIT_NO=SMBID.DEBIT_NO WHERE {0} UNION SELECT SMBID.*,'' AS OP,(SELECT TOP 1 POL_CD FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD,
                (SELECT TOP 1 SCAC_CD FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID AND SMSMI.TRAN_TYPE='E') AS SCAC_CD ,
                (SELECT TOP 1 HOUSE_NO FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS HOUSE_NO ,
          (SELECT TOP 1 STATUS
          FROM SMSMI WITH(NOLOCK)
         WHERE SMSMI.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.U_ID=SMBID.U_FID WHERE SMBID.GROUP_ID={1} AND SMBID.LSP_NO={2})T", innerCondition, SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));
            }
            string condition = "1=1";
            condition += subSql;
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            NameValueCollection namevaluecollection = null;
            if (namevaluecollection == null) namevaluecollection = Request.Params;
            DataTable dt = ModelFactory.InquiryData("DISTINCT SHIPMENT_ID", table, condition, namevaluecollection, ref recordsCount, ref pageIndex, ref pageSize);
            string str = string.Empty;
            string virtualCol = Prolink.Math.GetValueAsString(Request.Params["virtualCol"]);
            if (!string.IsNullOrEmpty(virtualCol))
                virtualCol = HttpUtility.UrlDecode(virtualCol);
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(virtualCol);
            //virtualCol = Prolink.Math.GetValueAsString(dict["virtualCol"]);
            string byContainerChgCd = Prolink.Math.GetValueAsString(dict["byContainerChgCd"]);
            //string byContainerCol = Prolink.Math.GetValueAsString(dict["byContainerCol"]);
            string transType = Prolink.Math.GetValueAsString(dict["transType"]);
            string btnType = Prolink.Math.GetValueAsString(dict["btnType"]);
            string LspNo = Prolink.Math.GetValueAsString(dict["TargetCmp"]);
            string allContainers = Prolink.Math.GetValueAsString(dict["allContainers"]);
            string chgTypeStr = Prolink.Math.GetValueAsString(dict["chgTypeStr"]);
            string[] chgType = chgTypeStr.Split(';');
            string[] byContainer = !string.IsNullOrEmpty(byContainerChgCd) ? byContainerChgCd.Split(';') : null;

            virtualCol = ", ISNULL(ATA,ETA) AS DEBIT_DATE, '' AS DEBIT_NO,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID) AS PARTY_NAME, '" + LspNo + "' AS LSP_NO1";
            string byContainerCol = virtualCol + ", T.CNTR_NO";
            virtualCol = virtualCol + ",'' AS CNTR_NO";

            string con = "";
            foreach (string val in chgType)
            {
                string[] chgCdType = val.Split('-');
                var thisChgCd = chgCdType[0];
                string cur = "";
                if (chgCdType.Length > 1)
                {
                    string chg = chgCdType[chgCdType.Length - 1];
                    int index = chg.LastIndexOf('(');
                    cur = chg.Substring(index + 1, chg.Length - index - 1);
                    cur = cur.Trim(')');
                }
                if (byContainer!=null&& byContainer.Contains(thisChgCd) && btnType == "L" && (transType == "F" || transType == "R"))
                {
                    con = " AND (CNTR_NO IS NULL OR CNTR_NO ='')";
                }
                byContainerCol += ", (SELECT TOP 1 QAMT FROM SMBID WHERE CHG_CD='" + thisChgCd + "' AND QCUR='" + cur + "' AND SHIPMENT_ID=T.SHIPMENT_ID AND CNTR_NO=T.CNTR_NO AND LSP_NO='" + LspNo + "') AS '" + thisChgCd + "_" + cur + "'";
                virtualCol += ", (SELECT  TOP 1 QAMT  FROM SMBID WHERE SMBID.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND (SMBID.CNTR_NO IS NULL OR SMBID.CNTR_NO='') AND SMBID.CMP=SMSMI.CMP AND CHG_CD='" + thisChgCd + "' AND QCUR='" + cur + "'" + con + " AND LSP_NO ='" + LspNo + "') AS '" + thisChgCd + "_" + cur + "'";

                con = "";
            } 


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);

                    str += ShipmentId + ",";
                }
                str = str.Remove(str.Length - 1);
                string[] sa = str.Split(',');
                List<string> smlist = null;
                string sql = string.Format("SELECT DISTINCT SHIPMENT_ID FROM SMBID WHERE SHIPMENT_ID IN {0} AND LSP_NO ={1} AND (CNTR_NO IS NULL OR CNTR_NO='')", SQLUtils.Quoted(sa), SQLUtils.QuotedStr(LspNo));
                DataTable smdt = getDataTableFromSql(sql);
                condition += "AND (CNTR_NO IS NULL OR CNTR_NO='')";
                smdt =ModelFactory.InquiryData("DISTINCT SHIPMENT_ID", table, condition, namevaluecollection, ref recordsCount, ref pageIndex, ref pageSize);
                if (smdt.Rows.Count > 0)
                    smlist = smdt.AsEnumerable().Select(r => r["SHIPMENT_ID"].ToString()).ToList();
                string byContainerSql = string.Empty;
                if (byContainer != null)
                {
                    byContainerSql = string.Format("SELECT SMSMI.*" + byContainerCol + " FROM (SELECT DISTINCT SHIPMENT_ID,CNTR_NO FROM SMBID WHERE " +
                            "SHIPMENT_ID IN {0} AND CNTR_NO IS NOT NULL AND CHG_CD IN {1} {2})T LEFT JOIN SMSMI ON T.SHIPMENT_ID=SMSMI.SHIPMENT_ID",
                            SQLUtils.Quoted(sa), SQLUtils.Quoted(byContainer), filter);
                    if ("O".Equals(ioflag))
                    {
                        byContainerSql = string.Format("SELECT SMSMI.*" + byContainerCol + " FROM (SELECT DISTINCT SHIPMENT_ID,CNTR_NO FROM SMBID WHERE " +
                            "SHIPMENT_ID IN {0} AND CNTR_NO IS NOT NULL AND CHG_CD IN {1} AND LSP_NO ={2} {3})T LEFT JOIN SMSMI ON T.SHIPMENT_ID=SMSMI.SHIPMENT_ID",
                            SQLUtils.Quoted(sa), SQLUtils.Quoted(byContainer), SQLUtils.QuotedStr(LspNo), filter);
                    }
                }
                byContainerSql = string.Format("SELECT SMSMI.*" + byContainerCol + " FROM (SELECT DISTINCT SHIPMENT_ID,CNTR_NO FROM SMBID WHERE " +
                          "SHIPMENT_ID IN {0} AND CNTR_NO IS NOT NULL  {1})T LEFT JOIN SMSMI ON T.SHIPMENT_ID=SMSMI.SHIPMENT_ID",
                          SQLUtils.Quoted(sa), filter);

                sql = string.Empty;
                if ("L".Equals(btnType) && ("F".Equals(transType) || "R".Equals(transType)))
                {
                    if (!string.IsNullOrEmpty(byContainerSql) || smlist == null)
                        sql = byContainerSql;
                }
                string allSql = "";
                if ("false".Equals(allContainers) || string.IsNullOrEmpty(sql))
                {
                    if (smlist != null)
                    {
                        string[] smids = smlist.ToArray();
                        allSql = "SELECT *" + virtualCol + " FROM SMSMI WHERE SMSMI.SHIPMENT_ID IN " + SQLUtils.Quoted(smids);
                        if (!string.IsNullOrEmpty(sql))
                            allSql = "SELECT *" + virtualCol + " FROM SMSMI WHERE SMSMI.SHIPMENT_ID IN " + SQLUtils.Quoted(smids) + " UNION ALL " + sql;
                    }
                    else
                    {
                        //allSql = "SELECT *" + virtualCol + " FROM SMSMI WHERE SMSMI.SHIPMENT_ID IN " + SQLUtils.Quoted(sa);
                        //if (!string.IsNullOrEmpty(sql))
                        //    allSql = "SELECT *" + virtualCol + " FROM SMSMI WHERE SMSMI.SHIPMENT_ID IN " + SQLUtils.Quoted(sa) + " UNION ALL " + sql;
                        allSql = sql;
                    }
                }
                else if (!string.IsNullOrEmpty(sql))
                    allSql = sql;
                dt = new DataTable();
                if (!string.IsNullOrEmpty(allSql))
                    dt = getDataTableFromSql(allSql);
            }
            else
            {
                dt = new DataTable();
            }
            if (dt.Rows.Count > 0)
            {
                dt.DefaultView.Sort = "SHIPMENT_ID ASC";
                dt = dt.DefaultView.ToTable();
            }

            return ExportExcelFile(dt);
        }
        #endregion

        private void SetApproveRole()
        {
            ViewBag.ApproveRole = "";
            string approveroles = BillApproveHelper.GetApprove(UserId, CompanyId, GroupId, UPri, Dep);
            ViewBag.ApproveRole = approveroles;
        }

        //权限方法  
        public string GetPMSByUrpi()
        {
            string condtion = GetPMSByUPri();
            string sql = string.Format("SELECT TRAN_TYPE FROM SYS_ACCT WHERE U_ID={0} AND {1}", SQLUtils.QuotedStr(UserId), GetBaseCmp());
            string actrantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string actrancondition = string.Empty;
            if (!string.IsNullOrEmpty(actrantype))
            {
                if (Dep.Equals("SFI")) //财务才区分是否是内销还是外销
                {
                    actrantype = actrantype.Trim(';');
                    string[] actrantypes = actrantype.Split(';');
                    actrancondition = string.Format("AND ATRAN_TYPE IN {0}", SQLUtils.Quoted(actrantypes));
                }
            }
            if (!string.IsNullOrEmpty(actrancondition))
            {
                condtion += actrancondition;
            }
            return condtion;
        }

        public string GetPMSByUPri()
        {
            string condtion = GetBaseGroup();
            switch (UPri)
            {
                case "G":
                    condtion = GetBaseGroup();
                    break;
                case "C":
                    condtion = GetBaseCmp();
                    if (TCmp != "")
                    {
                        condtion += string.Format(" AND STN in ( {0} ) ", TCmp);
                    }
                    break;
                case "S":
                    condtion = GetBaseStn();
                    if (TCmp != "")
                    {
                        condtion += string.Format(" AND STN in ( {0} ) ", TCmp);
                    }
                    break;
                case "D":
                    condtion = string.Format("GROUP_ID={0} AND CMP={1} AND DEP={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Dep));
                    if (TCmp != "")
                    {
                        condtion += string.Format(" AND STN in ( {0} ) ", TCmp);
                    }
                    break;
                case "U":
                    condtion = string.Format("GROUP_ID={0} AND CMP={1} AND DEP={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Dep)) + string.Format(" AND CREATE_BY={0}", SQLUtils.QuotedStr(UserId));
                    break;
                default:
                    if (TCmp != "")
                    {
                        condtion += string.Format(" AND STN in ( {0} ) ", TCmp);
                    }
                    break;
            }
            return condtion;
        }

        #region 账单相关
        public ActionResult GetTransTypeInfoForIb()
        {
            string condition = ModelFactory.GetInquiryCondition("SMBID", "", Request.Params);
            string returnMsg = "success";
            string TransType = Request.Params["TransType"];
            string Cmp = Request.Params["Cmp"];
            if (Cmp == "")
            {
                Cmp = CompanyId;
            }
            string Type = Request.Params["Type"];
            string chgTypeStr = "";
            string chgTypeColsStr = "";
            string byContainerChgCd = "";
            string allContainers = "true";
            try
            {
                DataTable dt = Business.TPV.Financial.Bill.GetChargeCodesForIb(Cmp, TransType, Type, condition);
                if (dt.Rows.Count == 0)
                {
                    returnMsg = "fail";
                    return Json(new { message = @Resources.Locale.L_ActManage_Controllers_49 });
                }

                string[] filters = new string[] { "CUR='CNY'", "CUR='USD'", "CUR NOT IN ('CNY','USD')" };
                
                List<string> testList = new List<string>();
                foreach (string filter in filters)
                {
                    DataRow[] drs = string.IsNullOrEmpty(filter) ? dt.Select("", "CUR DESC,REPAY") : dt.Select(filter, "CUR DESC,REPAY");
                    foreach (DataRow dr in drs)
                    {
                        string chg_cd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                        string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                        string TranMode = Prolink.Math.GetValueAsString(dr["TRAN_MODE"]);
                        string key = chg_cd + "_" + cur;
                        if (testList.Contains(key))
                            continue;
                        if (chg_cd.Contains("DTRF"))
                            byContainerChgCd = "DTRF";
                        else
                            allContainers = "false";
                        testList.Add(key);
                        chgTypeStr += chg_cd + "-" + Prolink.Math.GetValueAsString(dr["CHG_DESCP"]).Replace("\n", "") + string.Format("({0})", cur) + ";";
                        chgTypeColsStr += WebGui.Models.BaseModel.GetModelFiledName(key) + ";";
                    }
                }

                foreach (DataRow dr in dt.Rows)
                {
                    string chg_cd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                    string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                    string key = chg_cd + "_" + cur;
                    string TranMode = Prolink.Math.GetValueAsString(dr["TRAN_MODE"]);
                    if (testList.Contains(key))
                        continue;
                    if (chg_cd.Contains("DTRF"))
                        byContainerChgCd = "DTRF";
                    else
                        allContainers = "false";
                    testList.Add(key);
                    chgTypeStr += chg_cd + "-" + Prolink.Math.GetValueAsString(dr["CHG_DESCP"]).Replace("\n", "") + string.Format("({0})", cur) + ";";
                    chgTypeColsStr += WebGui.Models.BaseModel.GetModelFiledName(key) + ";";
                }
                
            }
            catch (Exception ex)
            {
                returnMsg = "fail";
                return Json(new { message = returnMsg });
            }

            //if (byContainerChgCd.Length > 0)
            //    byContainerChgCd = byContainerChgCd.Substring(0, byContainerChgCd.Length - 1);
            return Json(new { message = returnMsg, chgTypeStr = chgTypeStr.Substring(0, chgTypeStr.Length - 1), chgTypeColsStr = chgTypeColsStr.Substring(0, chgTypeColsStr.Length - 1), cmp = CompanyId, user = UserId, group = GroupId, createDate = DateTime.Now.ToString("yyyy/MM/dd HH:ss"), byContainerChgCd = byContainerChgCd, allContainers = allContainers });

        }

        /// <summary>
        /// 导出请款账单
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportBBillExcel1()
        {
            string tranType = Request["virtualCol"];
            string uid = Request["conditions"];
            string smid = Request["baseCondition"];
            //string condition = ids.Length > 0 ? string.Format("U_ID IN ({0})", string.Join(",", ids)) : "1=0";
            string sql = "SELECT * FROM (SELECT * FROM SMBID WITH (NOLOCK) WHERE U_FID=" + SQLUtils.QuotedStr(uid) + ")A OUTER APPLY (SELECT TOP 1 TRAN_TYPE FROM SMSM M WITH (NOLOCK) WHERE M.SHIPMENT_ID=A.SHIPMENT_ID) B ORDER BY SHIPMENT_ID,IPART_NO DESC,CNTR_STD_QTY DESC";
            DataTable bidDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            string col = @",(ISNULL(COMBINE_INFO, '')) AS COMBINE_INFO1
,(SELECT CONVERT(varchar,IN_DATE, 120 )+',' FROM SMRV WHERE SMRV.DN_NO LIKE '%'+ISNULL(SMSM.DN_NO, '------------')+'%' AND CNTR_NO IS NOT NULL AND IN_DATE IS NOT NULL FOR XML PATH('')) AS IN_DATE
,(SELECT CONVERT(varchar,OUT_DATE, 120 )+',' FROM SMRV WHERE SMRV.DN_NO LIKE '%'+ISNULL(SMSM.DN_NO, '------------')+'%' AND CNTR_NO IS NOT NULL AND OUT_DATE IS NOT NULL FOR XML PATH('')) AS OUT_DATE
,(SELECT CONVERT(varchar,CALL_DATE, 120 )+',' FROM SMRV WHERE SMRV.DN_NO LIKE '%'+ISNULL(SMSM.DN_NO, '------------')+'%' AND CNTR_NO IS NOT NULL AND CALL_DATE IS NOT NULL FOR XML PATH('')) AS CALL_DATE

,(SELECT DISTINCT CNTR_NO+',' FROM SMRV WHERE SMRV.DN_NO LIKE '%'+ISNULL(SMSM.DN_NO, '------------')+'%' AND CNTR_NO IS NOT NULL FOR XML PATH('')) AS EX_CNTR_NOS
,'EX_CNT_TYPE'=CASE 
     WHEN (SMSM.PCNT20 > 0) THEN
          '20GP*' + convert(varchar(10), SMSM.CNT20) + ','
         ELSE
          ''
       END + CASE
         WHEN (SMSM.PCNT40 > 0) THEN
          '40GP*' + convert(varchar(10), SMSM.CNT40) + ','
         ELSE
          ''
       END + CASE
         WHEN (SMSM.PCNT40HQ > 0) THEN
          '40HQ*' + convert(varchar(10), SMSM.CNT40HQ) + ','
         ELSE
          '' END ";

            sql = string.Format(@"SELECT *{1} FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WITH (NOLOCK) WHERE U_FID={0})", SQLUtils.QuotedStr(uid), col);
            switch (tranType)
            {
                case "TT"://内贸零担
                    sql = string.Format("SELECT * FROM (SELECT SMSM.*{1},(SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_CD,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_NM FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WITH (NOLOCK) WHERE U_FID={0}))A OUTER APPLY (SELECT TOP 1 DN_NO AS DN_NO1,AMOUNT1,QTY AS DN_QTY,QTYU AS DN_QTYU FROM SMDN WITH (NOLOCK) WHERE SMDN.SHIPMENT_ID=A.SHIPMENT_ID) B OUTER APPLY (SELECT TOP 1 IPART_NO,CNTR_STD_QTY FROM SMDNP WITH (NOLOCK) WHERE A.COMBINE_INFO1 LIKE '%'+SMDNP.DN_NO+'%' AND CNTR_STD_QTY>0 AND (NEW_CATEGORY<>'TANN' OR NEW_CATEGORY IS NULL)) C ORDER BY SHIPMENT_ID,DN_NO1 DESC,IPART_NO DESC,CNTR_STD_QTY DESC", SQLUtils.QuotedStr(uid), col);

                    //if (Prolink.Math.GetValueAsDecimal(bidDt.Compute("SUM(CNTR_STD_QTY)", "")) <= 0)
                    //{
                    //    sql = string.Format("SELECT * FROM (SELECT *{1} FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WITH (NOLOCK) WHERE U_FID={0}))A OUTER APPLY (SELECT TOP 1 DN_NO AS DN_NO1,AMOUNT1,QTY AS DN_QTY,QTYU AS DN_QTYU FROM SMDN WITH (NOLOCK) WHERE SMDN.SHIPMENT_ID=A.SHIPMENT_ID) B ORDER BY SHIPMENT_ID,DN_NO1 DESC", SQLUtils.QuotedStr(uid), col);
                    //    //sql = string.Format("SELECT * FROM (SELECT * FROM SMSM WHERE SHIPMENT_ID={0})A OUTER APPLY (SELECT TOP 1 DN_NO AS DN_NO1,AMOUNT1,QTY AS DN_QTY,QTYU AS DN_QTYU FROM SMDN WITH (NOLOCK) WHERE SMDN.SHIPMENT_ID=A.SHIPMENT_ID) B OUTER APPLY (SELECT TOP 1 TRAN_TYPE FROM SMBID WITH (NOLOCK) WHERE SMBID.SHIPMENT_ID=A.SHIPMENT_ID AND U_FID={1}) C ORDER BY SHIPMENT_ID,DN_NO1 DESC", SQLUtils.QuotedStr(smid), SQLUtils.QuotedStr(uid));
                    //}
                    //else
                    //{
                    //    sql = string.Format("SELECT * FROM (SELECT SMSM.*{1},(SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_CD,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_NM FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WITH (NOLOCK) WHERE U_FID={0}))A OUTER APPLY (SELECT TOP 1 DN_NO AS DN_NO1,AMOUNT1,QTY AS DN_QTY,QTYU AS DN_QTYU FROM SMDN WITH (NOLOCK) WHERE SMDN.SHIPMENT_ID=A.SHIPMENT_ID) B OUTER APPLY (SELECT TOP 1 IPART_NO,CNTR_STD_QTY FROM SMDNP WITH (NOLOCK) WHERE SMDNP.DN_NO=B.DN_NO1) C ORDER BY SHIPMENT_ID,DN_NO1 DESC,IPART_NO DESC,CNTR_STD_QTY DESC", SQLUtils.QuotedStr(uid), col);
                    //}
                    break;
            }

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            
            FileResult result = ExportAmtExcel(bidDt, dt, "BAMT");
            //if (!"TT".Equals(tranType))
            //    result = ExportAmtExcel(bidDt, dt, "BAMT");
            //else
            //    result = ExportAmtExcel1(bidDt, dt);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transTypeCols"></param>
        /// <param name="bidDt"></param>
        /// <param name="smDt"></param>
        /// <param name="update_condition">更新条件</param>
        /// <returns></returns>
        private FileResult ExportAmtExcel(DataTable bidDt, DataTable smDt, string amtField = "QLAMT", string[] transTypeCols = null, string update_condition = "", string excelType = "", bool autoAddCol = false, string tranType = "")
        
        {
            string columnListStr = string.Empty;
            #region 自动添加费用栏位
            if (autoAddCol)//判断是否自动添加栏位
            {
                columnListStr = Request["columnList"];
                if (!string.IsNullOrEmpty(columnListStr))
                    columnListStr = System.Web.HttpUtility.UrlDecode(columnListStr).Replace("null,", "");
                JavaScriptSerializer js = new JavaScriptSerializer();
                ArrayList arraylist = new ArrayList();
                object[] items = js.DeserializeObject(columnListStr) as object[];
                arraylist.AddRange(items);

                List<string> test = new List<string>();
                List<string> colList = new List<string>();
                List<string> usd_colList = new List<string>();
                List<string> oth_colList = new List<string>();
                foreach (DataRow dr in bidDt.Rows)
                {
                    string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                    if (string.IsNullOrEmpty(cur))
                        cur = Prolink.Math.GetValueAsString(dr["QCUR"]);
                    string chg_cd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                    string chg_descp = Prolink.Math.GetValueAsString(dr["CHG_DESCP"]).Replace("\n", "");
                    string key = GetCurKey(chg_cd, cur);
                    string name = WebGui.Models.BaseModel.GetModelFiledName(key);
                    string title = string.Format("{0}-{1}({2})", chg_cd, chg_descp, cur);

                    if (test.Contains(name))
                        continue;
                    test.Add(name);
                    if ("CNY".Equals(cur))
                        colList.Add(name+"#######"+title);
                    else if ("USD".Equals(cur))
                        usd_colList.Add(name + "#######" + title);
                    else
                        oth_colList.Add(name + "#######" + title);
                }
                if (usd_colList.Count>0)
                    colList.AddRange(usd_colList);
                if (oth_colList.Count > 0)
                    colList.AddRange(oth_colList);

                List<string> tempList = new List<string>();
                foreach (string col in colList)
                {
                    var cols = col.Split(new string[] { "#######" }, StringSplitOptions.None);
                    if (cols.Length < 1)
                        continue;
                    AddBillExcelItem(arraylist, cols[0], cols[1]);
                    tempList.Add(cols[1]);
                }
                transTypeCols = tempList.ToArray();
                columnListStr = js.Serialize(arraylist);
            }
            #endregion

            bool hasCur = false;
            #region 构建table
            if (transTypeCols != null && transTypeCols.Length > 0)
            {
                foreach (string col in transTypeCols)
                {
                    if (string.IsNullOrEmpty(col))
                        continue;
                    var cols = col.Split(new string[] { "-" }, StringSplitOptions.None);
                    if (cols.Length < 1)
                        continue;
                    if (smDt.Columns.Contains(cols[0]))
                        continue;
                    Regex regex = new Regex("[(](?<cur>[\\s\\S]*?)[)]", RegexOptions.IgnoreCase);
                    string cur = string.Empty;
                    MatchCollection MS = regex.Matches(col);
                    if (MS.Count > 0)
                        cur = MS[MS.Count - 1].Groups["cur"].Value;
                    string key = GetCurKey(cols[0], cur);
                    if (!string.IsNullOrEmpty(cur)) hasCur = true;
                    smDt.Columns.Add(key, typeof(decimal));
                }
            }

            string[] d_fileds = new string[] { "CNTR_STD_QTY", "DN_QTY", "RATE" };
            foreach (string field in d_fileds)
            {
                if (!smDt.Columns.Contains(field))
                {
                    smDt.Columns.Add(field, typeof(decimal));
                }
            }

            string[] str_fileds = new string[] { "TRACK_WAY", "CUR", "LOCATION", "DEBIT_NO", "VV", "DN_CMP", "DN_QTYU", "CAR_TYPE", "DN_NO", "RG_CD", "RG_NM", "DN_NO1", "IPART_NO", "DN_TYPE", "EX_REGION", "EX_PREGION", "DN_NO2","LCL" };
            foreach (string field in str_fileds)
            {
                if (!smDt.Columns.Contains(field))
                {
                    smDt.Columns.Add(field, typeof(string));
                    smDt.Columns[field].MaxLength = 999;
                }
            }

            if (!smDt.Columns.Contains("DEBIT_DATE"))
                smDt.Columns.Add("DEBIT_DATE", typeof(DateTime));

            if (!smDt.Columns.Contains("AMOUNT1"))
                smDt.Columns.Add("AMOUNT1", typeof(decimal));
            #endregion

            if (!smDt.Columns.Contains("TT_SHIPMENTID"))
                smDt.Columns.Add("TT_SHIPMENTID", typeof(decimal));
            if (!smDt.Columns.Contains("TT_DNNO"))
                smDt.Columns.Add("TT_DNNO", typeof(decimal));

            if (!smDt.Columns.Contains("EXT_TOTAL"))
                smDt.Columns.Add("EXT_TOTAL", typeof(decimal));

            if (smDt.Columns.Contains("COMBINE_INFO"))
                smDt.Columns["COMBINE_INFO"].MaxLength = 9999;

            foreach (DataRow bi in smDt.Rows)
            {
                bi["CUR"] = string.Empty;
                string dn_no = Prolink.Math.GetValueAsString(bi["DN_NO2"]);
                if (string.IsNullOrEmpty(dn_no))
                    dn_no = Prolink.Math.GetValueAsString(bi["DN_NO1"]);

                if (dn_no.Length >= 4)
                    bi["DN_CMP"] = dn_no.Substring(0, 4);
                if (!string.IsNullOrEmpty(dn_no))
                    bi["DN_NO"] = dn_no;

                string ipart_no = Prolink.Math.GetValueAsString(bi["IPART_NO"]);
                if (string.IsNullOrEmpty(ipart_no) && bi.Table.Columns.Contains("IPART_NO1"))
                {
                    ipart_no = Prolink.Math.GetValueAsString(bi["IPART_NO1"]);
                    bi["IPART_NO"] = ipart_no;
                }

                decimal cntr_std_qty = Prolink.Math.GetValueAsDecimal(bi["CNTR_STD_QTY"]);
                if (cntr_std_qty == 0 && bi.Table.Columns.Contains("CNTR_STD_QTY1"))
                {
                    cntr_std_qty = Prolink.Math.GetValueAsDecimal(bi["CNTR_STD_QTY1"]);
                    bi["CNTR_STD_QTY"] = cntr_std_qty;
                }
                decimal cnt_number = Prolink.Math.GetValueAsDecimal(bi["CNT_NUMBER"]);

                //续117626： FCL、LCL的下载格式，订舱上如果LCL栏位数值是空的，下载下来的excel格式中，LCL一列为空就好，不要显示0。 请修正。
                if (cnt_number != 0)
                    bi["LCL"] = cnt_number.ToString();
                else
                    bi["LCL"] = string.Empty;
                //if ("LCL".Equals(Prolink.Math.GetValueAsString(bi["CNT_TYPE"])))
                //{
                //    bi["LCL"] = Prolink.Math.GetValueAsDecimal(bi["CNT_NUMBER"]).ToString();
                //}
                //if (cntr_std_qty <= 0)
                //    bi["IPART_NO"] = dn_no;

                //bi["CUR"] = "CNY";
                #region 设置国家区域
                if (bi.Table.Columns.Contains("EX_REGION"))
                {
                    string pod = Prolink.Math.GetValueAsString(bi["POD_CD"]);
                    string region = string.Empty;
                    if (pod.Length > 2)
                        region = pod.Substring(0, 2);
                    bi["EX_REGION"] = region;
                }

                if (bi.Table.Columns.Contains("EX_PREGION"))
                {
                    string pod = Prolink.Math.GetValueAsString(bi["PPOD_CD"]);
                    string region = string.Empty;
                    if (pod.Length > 2)
                        region = pod.Substring(0, 2);
                    bi["EX_PREGION"] = region;
                }
                #endregion

                bi["LOCATION"] = bi["CMP"];
                if (bi.Table.Columns.Contains("COMBINE_INFO1"))
                {
                    bi["COMBINE_INFO"] = Prolink.Math.GetValueAsString(bi["COMBINE_INFO1"]) + System.Environment.NewLine + Prolink.Math.GetValueAsString(bi["EX_CNTR_NOS"]) + System.Environment.NewLine + Prolink.Math.GetValueAsString(bi["EX_CNT_TYPE"]);
                }

                string voyage1 = Prolink.Math.GetValueAsString(bi["VOYAGE1"]);
                bi["VV"] = Prolink.Math.GetValueAsString(bi["VESSEL1"]) + (string.IsNullOrEmpty(voyage1) ? string.Empty : "-" + voyage1);
                bi["REMARK"] = string.Empty;
                bi["DEBIT_DATE"] = Business.TPV.Financial.Bill.GetBillDate(DateTime.Now, bi, false);
                //if (bi["ETD"] != null && bi["ETD"] != DBNull.Value)
                //    bi["DEBIT_DATE"] = bi["ETD"];
                //else if (bi["DN_ETD"] != null && bi["DN_ETD"] != DBNull.Value)
                //    bi["DEBIT_DATE"] = bi["DN_ETD"];
            }

            foreach (DataRow bi in bidDt.Rows)
            {
                string chg_cd = Prolink.Math.GetValueAsString(bi["CHG_CD"]);
                string cur = amtField.StartsWith("Q") ? Prolink.Math.GetValueAsString(bi["QCUR"]) : Prolink.Math.GetValueAsString(bi["CUR"]);
                string key = hasCur ? chg_cd + "_" + cur : chg_cd;
                //if (string.IsNullOrEmpty(key))
                //    continue;
                if (!smDt.Columns.Contains(key))
                    smDt.Columns.Add(key, typeof(decimal));

                string shipment_id = Prolink.Math.GetValueAsString(bi["SHIPMENT_ID"]);
                string filter = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id));
                string ipart_no = Prolink.Math.GetValueAsString(bi["IPART_NO"]);
                string dn_no = bi.Table.Columns.Contains("DN_NO") ? Prolink.Math.GetValueAsString(bi["DN_NO"]) : string.Empty;
                decimal cntr_std_qty = Prolink.Math.GetValueAsDecimal(bi["CNTR_STD_QTY"]);
                string dn_filter = string.Empty;
                if (!string.IsNullOrEmpty(ipart_no))
                {
                    if (cntr_std_qty > 0M)
                        dn_filter += string.Format(" AND IPART_NO={0} AND DN_NO={1}", SQLUtils.QuotedStr(ipart_no), SQLUtils.QuotedStr(dn_no));
                    else
                        dn_filter += string.Format(" AND DN_NO={0}", SQLUtils.QuotedStr(ipart_no));
                }

                DataRow[] drs = smDt.Select(filter + dn_filter);
                if (drs.Length <= 0)
                {
                    drs = smDt.Select(filter);
                    if (drs.Length <= 0)
                        continue;
                }
                drs[0]["CUR"] = cur;
                if (Prolink.Math.GetValueAsDecimal(drs[0]["CNTR_STD_QTY"]) <= 0)
                    drs[0]["IPART_NO"] = ipart_no;
             
                //drs[0][chg_cd] = bi[amtField];
                if ("F".Equals(excelType) && "QLAMT".Equals(amtField) && "OF".Equals(chg_cd))
                {
                    drs[0]["CUR"] = bi["QCUR"];
                    SetChgAmt(drs[0], bi, chg_cd, cur, "QAMT");
                }
                //else if ("F".Equals(excelType) && "BAMT".Equals(amtField))
                //{
                //    drs[0]["CUR"] = bi["CUR"];
                //    drs[0][chg_cd] = bi["BAMT"];
                //}
                else if ("BAMT".Equals(amtField))
                {
                    if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(bi["CUR"])))
                        bi["CUR"] = "CNY";
                    drs[0]["CUR"] = bi["CUR"];
                    if ("CNY".Equals(Prolink.Math.GetValueAsString(bi["CUR"])) && Prolink.Math.GetValueAsDecimal(bi["LAMT"]) > 0)
                        SetChgAmt(drs[0], bi, chg_cd, Prolink.Math.GetValueAsString(drs[0]["CUR"]), "LAMT");
                    //drs[0][chg_cd] = bi["LAMT"];
                    else
                        SetChgAmt(drs[0], bi, chg_cd, cur, amtField);
                    //drs[0][chg_cd] = bi[amtField];
                }
                else
                    SetChgAmt(drs[0], bi, chg_cd, cur, amtField);

                if (!smDt.Columns.Contains(key + "_Status"))
                    smDt.Columns.Add(key + "_Status", typeof(string));
                drs[0][key + "_Status"] = bi["STATUS"];

                if (smDt.Columns.Contains("EXT_TOTAL"))
                    drs[0]["EXT_TOTAL"] = Prolink.Math.GetValueAsDecimal(drs[0]["EXT_TOTAL"]) + Prolink.Math.GetValueAsDecimal(drs[0][key]);
            }

            //问题单:115621  请款审核 => 内贸下载请款Excel 的格式将包含by Shipment ID & by DN累加计算相关费用的栏位。 格式：见转发的邮件。 ADD BY FISH 2017/02/14
            if (autoAddCol == false && ("T".Equals(tranType) || "TT".Equals(tranType)))
            {
                DataTable dt2 = smDt.Clone();
                foreach (DataRow dr in smDt.Rows)
                {
                    if(string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["CUR"])))
                        continue;
                    dt2.ImportRow(dr);
                }
                smDt = dt2;
                dt2 = smDt.Clone();
                List<string> smList = new List<string>();
                foreach (DataRow dr in smDt.Rows)
                {
                    string shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    if (smList.Contains(shipment_id))
                        continue;
                    if (string.IsNullOrEmpty(shipment_id))
                        continue;
                    smList.Add(shipment_id);
                    string filter =string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id));
                    DataRow[] drs = smDt.Select(filter, "SHIPMENT_ID,DN_NO,IPART_NO");
                    List<string> dnList = new List<string>();
                    ImportRow(smDt.Compute("SUM(EXT_TOTAL)", filter), dt2, drs, "TT_SHIPMENTID", string.Empty);

                    foreach (DataRow dr0 in drs)
                    {
                        string dn_no = Prolink.Math.GetValueAsString(dr0["DN_NO"]);
                        if (dnList.Contains(dn_no))
                            continue;
                        if (string.IsNullOrEmpty(dn_no))
                            continue;
                        dnList.Add(dn_no);
                        filter = string.Format("SHIPMENT_ID={0} AND DN_NO={1}", SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(dn_no));
                        DataRow[] drs1 = smDt.Select(filter, "SHIPMENT_ID,DN_NO,IPART_NO");
                        ImportRow(smDt.Compute("SUM(EXT_TOTAL)", filter), dt2, drs1, "TT_DNNO", dn_no);
                    }

                    foreach (DataRow dr0 in drs)
                    {
                        dt2.ImportRow(dr0);
                    }
                }
                smDt = dt2;
            }
            FileResult result = null;
            try
            {
                if ("BAMT".Equals(amtField))
                {
                    string old_action = Request["old_action"];//是否启用旧方法
                    if (string.IsNullOrEmpty(old_action))
                        result = ExportExcelFile1(smDt,columnListStr);
                    else
                        result = ExportExcelFile(smDt, columnListStr);
                }
                else
                    result = ExportExcelFile(smDt, columnListStr);

                if (!string.IsNullOrEmpty(update_condition))
                {
                    EditInstruct ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                    ei.Condition = update_condition + " AND  FSTATUS='A'";
                    ei.Put("FSTATUS", "B");
                    ei.Put("DOWNLOAD_USER", this.UserId);
                    DateTime odt = DateTime.Now;                  
                    DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    ei.PutDate("DOWNLOAD_TIME", odt);
                    ei.PutDate("DOWNLOAD_TIME_L", ndt);
                    OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        private static void ImportRow(object val, DataTable dt2, DataRow[] drs,string name,string dn_no)
        {
            DataRow drq = dt2.NewRow();
            //drq.ItemArray = drs[0].ItemArray;//这是加入的是第一行
            List<string> cols = new List<string>() { "DN_QTY", "DN_QTYU", "IPART_NO", "CNTR_STD_QTY", "DEBIT_DATE" };
            foreach (DataColumn col in dt2.Columns)
            {
                if (cols.Contains(col.ColumnName))
                {
                    drq[col.ColumnName] = DBNull.Value;
                    continue;
                }
                if (col.DataType == typeof(decimal))
                    drq[col.ColumnName] = DBNull.Value;
                else
                    drq[col.ColumnName] = drs[0][col.ColumnName];
            }

            drq["DN_NO"] = dn_no;
            drq[name] = val;
            dt2.Rows.Add(drq);
        }

        private static string GetCurKey(string col, string cur)
        {
            string key = string.IsNullOrEmpty(cur) ? col : col + "_" + cur;
            if (!string.IsNullOrEmpty(cur) && cur.Length == 1)
            {
                key = col + cur.ToUpper();
            }
            return key;
        }

        private static void SetChgAmt(DataRow sm, DataRow bid, string chg_cd, string cur, string amtFiled)
        {
            string key = string.Format("{0}_{1}", chg_cd, cur);
            if (sm.Table.Columns.Contains(key))
                sm[key] = Prolink.Math.GetValueAsDecimal(sm[key]) + Prolink.Math.GetValueAsDecimal(bid[amtFiled]);
            else if (sm.Table.Columns.Contains(chg_cd))
                sm[chg_cd] = Prolink.Math.GetValueAsDecimal(sm[chg_cd]) + Prolink.Math.GetValueAsDecimal(bid[amtFiled]);
        }

    
        public ActionResult ImportBillExcelForIb()
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
            string msg = string.Empty;
            string sql = string.Empty;
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);
            string autoChk = Prolink.Math.GetValueAsString(Request.Params["autoChk"]);
            string LspNo = Request["lspNo"];
            if (LspNo != null)
                LspNo = HttpUtility.UrlDecode(LspNo);
            if (string.IsNullOrEmpty(LspNo))
                LspNo = CompanyId;


            #region 生成上传的excel数据
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            string strExt = System.IO.Path.GetExtension(file.FileName);
            string excelFileName = string.Format("{0}{1}", Server.MapPath("~/FileUploads/") + this.GetType().Name + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
            file.SaveAs(excelFileName);
            DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);
            #endregion

            List<string> checkList = new List<string>();
            try
            {

                BillingManager bill = new BillingManager(GetLang());
                msg+=bill.ImportDatatableToBillForIb(dt, LspNo, ml, "", this.UserId, checkList);
            }
            catch (Exception e)
            {
                return Json(new { message = e.Message, excelMsg = e.Message });
            }
            try
            {
                if (checkList.Count <= 0)
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                else
                {
                    msg += string.Join(";", checkList) + System.Environment.NewLine + "请修正后重新上传!!!!";
                }
            }
            catch (Exception e)
            {
                msg += e.Message;
            }
            return Json(new { message = returnMessage, excelMsg = msg });
        }


        private static string ChkAct(string uid, decimal Bamt, decimal qamt, EditInstruct ei = null)
        {
            string CheckDescp = "";
            string Status = "Y";
            string sql = "UPDATE SMBID SET CHECK_DESCP={0}, STATUS={1} WHERE U_ID={2}";
            if (qamt > Bamt)
            {
                CheckDescp = @Resources.Locale.L_ActManage_Controllers_69;
                Status = "N";
            }
            else if (qamt < Bamt)
            {
                CheckDescp = @Resources.Locale.L_ActManage_Controllers_70;
                Status = "N";
            }
            if (ei != null)
            {
                ei.Put("STATUS", Status);
                ei.Put("CHECK_DESCP", CheckDescp);
            }
            sql = string.Format(sql, SQLUtils.QuotedStr(CheckDescp), SQLUtils.QuotedStr(Status), SQLUtils.QuotedStr(uid));
            return sql;
        }

        

        public ActionResult Get2CNYRate()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            bool error = false;
            try
            {
                string cur = Request["Cur"];
                string debit_date = Request["debit_date"];
                string cmp= Request["Cmp"];
                string localCur= Business.TPV.Standard.BillingManager.GetLocalCur(this.GroupId, cmp);
                DateTime debitDate = Prolink.Math.GetValueAsDateTime(debit_date.Replace("/", "").Replace("-", "").Replace(" ", "").Replace(":", ""));

                DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND EDATE<={0} ORDER BY EDATE", SQLUtils.QuotedStr(debitDate.ToString("yyyy-MM-dd"))), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                decimal bamt = 0m;
                decimal Blamt = 0m;
                List<string> msg = new List<string>();
                decimal rate = Business.TPV.Financial.Helper.GetTotal(rateDt, msg, bamt, cur, ref Blamt, ref error, localCur);
              
                data["rate"] = rate;
                if (error)
                    data["msg"] = msg.Last();
            }
            catch(Exception e) {
                data["msg"] = e.Message;
            }
            data["error"] = error;
            return ToContent(data);
        }

        /// <summary>
        /// 获取单笔账单查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetActDataItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format(@"SELECT * ,(SELECT TOP 1 STATUS
          FROM SMSM WITH(NOLOCK)
         WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS  FROM SMBID WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbidModel");
            return ToContent(data);
        }

        public ActionResult SaveActItemData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }

            string u_id = Request["u_id"];

            string ChgCd = Request["ChgCd"];
            string LspNo = Request["LspNo"];
            string ipart_no = Request["IpartNo"];
            string ShipmentId = Request["ShipmentId"];

            string debit_date = Request["debit_date"];
            string cur = Request["Cur"];
            decimal unitprice = Prolink.Math.GetValueAsDecimal(Request["UnitPrice"]);
            decimal qty = Prolink.Math.GetValueAsDecimal(Request["Qty"]);
            decimal bamt = Prolink.Math.GetValueAsDecimal(Request["Bamt"]);



            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string cmp = string.Empty;
            string name = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbidModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        decimal qlamt = 0;
                        decimal qamt = 0M;
                        string qcur = string.Empty;

                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = string.Empty;
                            //ei.Put("U_ID", u_id);
                            ei.Put("FSTATUS", "A");
                            ei.Put("GROUP_ID", this.GroupId);
                            ei.Put("CMP", this.BaseCompanyId);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            u_id = ei.Get("U_ID");
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            DataTable smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.*,0 AS EX_UPDATE FROM SMBID WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND DEBIT_NO IS NULL AND (BAMT=0 OR BAMT IS NULL) ORDER BY SHIPMENT_ID,CHG_CD,DEBIT_NO DESC,IPART_NO DESC", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

                            DataTable qtTempDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,LSP_NO,QUOT_NO,CHG_CD,CHG_TYPE,CHG_DESCP,CUR,QAMT,QLAMT,DEBIT_NM,DEBIT_TO FROM SMBID_TEMP WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                            DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                            //2016/8/18新增
                            cmp = string.Empty;
                            string tran_type = string.Empty;
                            string PodCd = string.Empty, MasterNo = string.Empty, CntrInfo = string.Empty;
                            if (smDt.Rows.Count > 0)
                            {
                                cmp = Prolink.Math.GetValueAsString(smDt.Rows[0]["CMP"]);
                                tran_type = Prolink.Math.GetValueAsString(smDt.Rows[0]["TRAN_TYPE"]);
                                PodCd = Prolink.Math.GetValueAsString(smDt.Rows[0]["POD_CD"]);
                                MasterNo = Prolink.Math.GetValueAsString(smDt.Rows[0]["MASTER_NO"]);
                                CntrInfo = Prolink.Math.GetValueAsString(smDt.Rows[0]["CNTR_INFO"]);
                                #region 设置订舱者
                                string bl_win = Prolink.Math.GetValueAsString(smDt.Rows[0]["BL_WIN"]).Trim();
                                if (!string.IsNullOrEmpty(bl_win) && bl_win.LastIndexOf(' ') > 0)
                                    bl_win = bl_win.Substring(0, bl_win.LastIndexOf(' '));
                                ei.Put("BOOKING_BY", bl_win);
                                #endregion
                            }
                            if (string.IsNullOrEmpty(cmp))
                                cmp = this.BaseCompanyId;
                            ei.Put("CMP", cmp);
                            ei.Put("TRAN_TYPE", tran_type);
                            ei.Put("POD_CD", PodCd);
                            ei.Put("MASTER_NO", MasterNo);
                            ei.Put("CNTR_INFO", CntrInfo);

                            DataTable smptDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,PARTY_NO,PARTY_NAME,PARTY_TYPE FROM SMSMIPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                            DataTable chgDt = OperationUtils.GetDataTable(string.Format("SELECT CHG_CD,CHG_TYPE FROM SMCHG WHERE CHG_CD={0}", SQLUtils.QuotedStr(ChgCd)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                            ei.Put("QCUR", cur);

                            string filter = string.Format("SHIPMENT_ID={0} AND CHG_CD={1} AND QCUR={2}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(cur));
                            //string filter = string.Format("SHIPMENT_ID={0} AND CHG_CD={1}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd));
                            string filter1 = filter;
                            DataRow[] drs = null;
                            if (!string.IsNullOrEmpty(ipart_no))
                            {
                                filter1 += string.Format(" AND IPART_NO={0}", SQLUtils.QuotedStr(ipart_no));
                                drs = smbidDt.Select(filter1);
                            }
                            if (drs == null || drs.Length <= 0)
                                drs = smbidDt.Select(filter);
                            string chg_type = string.Empty;

                            foreach (DataRow obi in drs)
                            {
                                if (Prolink.Math.GetValueAsInt(obi["EX_UPDATE"]) != 0)
                                    continue;
                                chg_type = Prolink.Math.GetValueAsString(obi["CHG_TYPE"]);
                                obi["EX_UPDATE"] = 1;
                                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                                u_id = Prolink.Math.GetValueAsString(obi["U_ID"]);
                                ei.PutKey("U_ID", u_id);
                                if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(obi["QUOT_NO"])))
                                    ei.Put("QT_DATA", "Y");
                                qcur = Prolink.Math.GetValueAsString(obi["QCUR"]);
                                qamt = Prolink.Math.GetValueAsDecimal(obi["QAMT"]);
                                qlamt = Prolink.Math.GetValueAsDecimal(obi["QLAMT"]);
                                ei.Put("QCUR", qcur);
                                break;
                            }

                            if (string.IsNullOrEmpty(u_id))
                            {
                                u_id = System.Guid.NewGuid().ToString();
                                ei.Put("U_ID", u_id);
                                ei.OperationType = EditInstruct.INSERT_OPERATION;

                                if (chgDt.Rows.Count > 0)
                                {
                                    chg_type = Prolink.Math.GetValueAsString(chgDt.Rows[0]["CHG_TYPE"]);
                                    if (string.IsNullOrEmpty(ei.Get("CHG_TYPE")))
                                        ei.Put("CHG_TYPE", chg_type);
                                    chg_type = ei.Get("CHG_TYPE");
                                }

                                #region 设置debit to
                                string my_debit_no = string.Empty;
                                string my_debit_nm = string.Empty;
                                DataRow[] partySH = Business.TPV.Financial.Helper.GetParty(ShipmentId, smptDt, new string[] { "SH" });
                                if (string.IsNullOrEmpty(my_debit_no) && partySH != null && partySH.Length > 0)
                                {
                                    my_debit_no = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NO"]);
                                    my_debit_nm = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NAME"]);
                                    ei.Put("DEBIT_TO", my_debit_no);
                                    ei.Put("DEBIT_NM", my_debit_nm);
                                }
                                #endregion

                                DataRow[] temps = qtTempDt.Select(string.Format("SHIPMENT_ID={0} AND CHG_CD={1} AND LSP_NO={2} AND CUR={3}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(cur)));
                                //DataRow[] temps = qtTempDt.Select(string.Format("SHIPMENT_ID={0} AND CHG_CD={1} AND LSP_NO={2}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(LspNo)));
                                if (temps.Length > 0)
                                {
                                    qcur = Prolink.Math.GetValueAsString(temps[0]["CUR"]);
                                    qamt = Prolink.Math.GetValueAsDecimal(temps[0]["QAMT"]);
                                    qlamt = Prolink.Math.GetValueAsDecimal(temps[0]["QLAMT"]);

                                    //QUOT_NO,CHG_CD,CHG_TYPE,CHG_DESCP,CUR,QAMT,QLAMT
                                    ei.Put("QUOT_NO", temps[0]["QUOT_NO"]);
                                    ei.Put("CHG_TYPE", temps[0]["CHG_TYPE"]);
                                    ei.Put("QCUR", qcur);
                                    ei.Put("QAMT", 0);
                                    ei.Put("QLAMT", 0);
                                    ei.Put("DEBIT_NM", temps[0]["DEBIT_NM"]);
                                    ei.Put("DEBIT_TO", temps[0]["DEBIT_TO"]);
                                    ei.Put("REMARK", string.Format(@Resources.Locale.L_ActManageController_Controllers_20, qamt, qcur, qlamt));
                                }

                            }
                        }

                        #region 获取旧数据
                        try
                        {
                            if (!string.IsNullOrEmpty(u_id))
                            {
                                DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBID WHERE U_ID={0}", SQLUtils.QuotedStr(u_id)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                                if (dt.Rows.Count > 0)
                                {
                                    string u_fid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_FID"]);
                                    qlamt = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["QLAMT"]);
                                    qamt = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["QAMT"]);
                                    qcur = Prolink.Math.GetValueAsString(dt.Rows[0]["QCUR"]);
                                    if (!string.IsNullOrEmpty(u_fid))
                                    {
                                        DataTable smbim = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(u_fid)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        if (smbim.Rows.Count > 0)
                                        {
                                            string Status = Prolink.Math.GetValueAsString(smbim.Rows[0]["STATUS"]);
                                            string tag = "";
                                            switch (Status)
                                            {
                                                case "V":
                                                    tag = @Resources.Locale.L_MenuBar_Audit;
                                                    break;
                                                case "F":
                                                    tag = @Resources.Locale.L_ActManage_bePay;
                                                    break;
                                                case "E":
                                                    tag = @Resources.Locale.L_ActManage_Invoice;
                                                    break;
                                                case "D":
                                                    tag = @Resources.Locale.L_Pass;
                                                    break;
                                            }
                                            if (!string.IsNullOrEmpty(tag))
                                                return Json(new { message = string.Format(@Resources.Locale.L_ActManageController_Controllers_21, tag) });
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                        #endregion

                        if (string.IsNullOrEmpty(u_id))
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (qamt != 0)
                            {
                                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                                ei.PutKey("U_ID", u_id);
                                ei.Put("CUR", "");
                                ei.Put("UNIT_PRICE", null);
                                ei.Put("CHG_UNIT", "");
                                ei.Put("BAMT", null);
                                ei.Put("LAMT", null);
                                ei.Put("QTY", null);
                                ei.Put("TAX", null);
                                ei.Put("EX_RATE", null);
                                ei.Put("STATUS", "");
                            }
                        }
                        else
                        {
                            if(string.IsNullOrEmpty(cmp))
                                cmp = ei.Get("CMP");
                            string localCur = Business.TPV.Standard.BillingManager.GetLocalCur(this.GroupId, cmp);
                            ei.Put("BAMT", bamt);
                            decimal Blamt = 0M;
                            bool error = false;
                            DateTime debitDate = Prolink.Math.GetValueAsDateTime(debit_date.Replace("/", "").Replace("-", "").Replace(" ", "").Replace(":", ""));

                            DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND EDATE<={0} ORDER BY EDATE", SQLUtils.QuotedStr(debitDate.ToString("yyyy-MM-dd"))), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                            decimal rate = Business.TPV.Financial.Helper.GetTotal(rateDt, null, bamt, cur, ref Blamt, ref error, localCur);


                            ei.Put("LAMT", Blamt);
                            ei.Put("EX_RATE", rate);
                            if (cur.Equals(qcur))
                                ChkAct(u_id, bamt, qamt, ei);
                            else
                            {
                                ChkAct(u_id, Blamt, qlamt, ei);
                            }
                        }

                        Business.TPV.Financial.InboundBill.SetEamt(ei);
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if ("Y".Equals(Request["recal_amt"]))
                        Business.TPV.Financial.Bill.SumAmt("", u_id);
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = returnMessage });
                }
            }
            string sql = string.Format("SELECT * FROM SMBID WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            try
            {
                if (!string.IsNullOrEmpty(u_id) && mainDt.Rows.Count > 0 && "N".Equals(Prolink.Math.GetValueAsString(mainDt.Rows[0]["APPROVE_STATUS"])))
                {
                    int result = OperationUtils.ExecuteUpdate(string.Format("UPDATE SMBID SET APPROVE_STATUS='E' WHERE U_ID={0} AND APPROVE_STATUS='N'", SQLUtils.QuotedStr(u_id)), Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
                return Json(new { message = returnMessage });
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbidModel");
            return ToContent(data);
        }
        #endregion

        public ActionResult BankInfoSetup()
        {
            ViewBag.pmsList = GetBtnPms("AC070");
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

        public ActionResult SaveBankInfoData()
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
            string cmp = string.Empty;
            string name = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbkinfoModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        cmp = ei.Get("CMP");

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("GROUP_ID", GroupId);
                            //ei.Put("NAME", name);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        else
                            ei.Remove("CMP");

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            //name = ei.Get("NAME").Trim();
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.Put("NAME", name);
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
                    return Json(new { message = returnMessage });
                }
            }
            string sql = string.Format("SELECT *  FROM SMBKINFO WHERE {0} AND CMP={1}", GetBaseGroup(), SQLUtils.QuotedStr(cmp));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "SmbkinfoModel"));
        }

        public ActionResult GetBankInfoData()
        {
            string table = "(SELECT * FROM (SELECT GROUP_ID,LSP_NO,CMP FROM SMBKINFO WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " GROUP BY GROUP_ID,LSP_NO,CMP) S OUTER APPLY (SELECT TOP 1 PARTY_NAME AS NAME FROM SMPTY PT WITH (NOLOCK) WHERE PT.PARTY_NO = S.CMP AND PT.GROUP_ID = S.GROUP_ID) B) MD";
            return GetBootstrapData(table, "1=1");
        }

        public ActionResult GetMailGroup()
        {
            string cmp = Request["cmp"];
            string sql = string.Format("SELECT *  FROM SMBKINFO WHERE {0} AND CMP={1}", GetBaseGroup(), SQLUtils.QuotedStr(cmp));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "SmbkinfoModel"));
        }

        public ActionResult PutCheckNo()
        {
            string id = Request["id"];
            string no = HttpUtility.UrlDecode(Request["no"]);
            string sql = string.Format("UPDATE SMBIM SET VAT_NO={0} WHERE U_ID={1}", SQLUtils.QuotedStr(no), SQLUtils.QuotedStr(id));
            string returnMessage = @Resources.Locale.L_ActManage_Controllers_68;
            try
            {
                int result = OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (result <= 0)
                    returnMessage = @Resources.Locale.L_ActManage_Controllers_64;
            }
            catch (Exception e)
            {
                returnMessage = e.Message;
            }
            return Json(new { message = returnMessage });
        }

        /// <summary>
        /// location选择
        /// </summary>
        private void SetLocationSelect()
        {
            ViewBag.SelectLocation = "";
            string sql = string.Format("SELECT CMP,NAME FROM SYS_SITE WHERE TYPE='1' AND GROUP_ID={0}", SQLUtils.QuotedStr(GroupId));
            //string sql = "SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TTRN'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            List<string> test = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]).Trim();
                if (test.Contains(cmp))
                    continue;
                test.Add(cmp);

                if (select.Length > 0)
                {
                    select += ";";
                }
                select += cmp;
                select += ":" + cmp + "." + Prolink.Math.GetValueAsString(dr["NAME"]).Trim();
            }
            ViewBag.SelectLocation = select;
        }

        public FileResult ExportExcelFile1(DataTable dt,string columnListStr="")
        {
            string xlsFile = MyTableExcelRenderer.GetExcelRenderer(dt, Request.Params, columnListStr);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", regex.Match(xlsFile).Value);
        }

        #region 出險管理
        public ActionResult InsurancePayQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AC080");
            return View();
        }
        public ActionResult InsurancePaySetupView(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string condition = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND (CMP=" + SQLUtils.QuotedStr(BaseCompanyId) + " OR CMP='*') AND (STN=" + SQLUtils.QuotedStr(BaseStation) + " OR STN='*')";
            string sql = "SELECT * FROM SMIPM WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> Schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.Schemas = ToContent(Schemas);
            ViewBag.UId = id;
            ViewBag.Reason = getBscodeForSelect("TIP", condition);
            ViewBag.pmsList = GetBtnPms("AC080");
            ViewBag.Cmp = CompanyId;
            ViewBag.CmpNm = getOneValueAsStringFromSql("SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE STATUS='U' AND PARTY_NO=" + SQLUtils.QuotedStr(CompanyId));
            ViewBag.ChgDescp = CommonHelp.getBscodeForColModel("TIC", GetDataPmsCondition("S"));
            return View();
        }
        public ActionResult InsurancePayDetailView(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string JobType = Prolink.Math.GetValueAsString(Request.Params["JobType"]);
            string JobNo = "";
            if (!string.IsNullOrEmpty(JobType))
            {
                JobNo = OperationUtils.GetValueAsString(string.Format("SELECT JOB_NO FROM SMIPM WHERE U_ID={0}", SQLUtils.QuotedStr(uid)), Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            string sql = "SELECT * FROM SMIPC WHERE 1=0 AND JOB_TYPE=" + SQLUtils.QuotedStr(JobType);
            //string sql = "SELECT * FROM SMIPC WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> Schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.Schemas = ToContent(Schemas);
            ViewBag.UId = uid;
            ViewBag.JobNo = JobNo;
            ViewBag.JobType = JobType;
            ViewBag.pmsList = GetBtnPms("AC080");
            return View();
        }
        public ActionResult SMIPMQueryData()
        {
            string con = " INS_CD=" + SQLUtils.QuotedStr(CompanyId);
            if(IOFlag == "I")
            {
                con = GetDataPmsCondition("C");
            }
            con = GetCreateDateCondition("SMIPM", con);
            return GetBootstrapData("SMIPM", con);
        }
        public ActionResult GetSMIPMDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMIPM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql2 = string.Format("SELECT * FROM SMIPR WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            if (IOFlag.Equals("O"))
            {
                sql2 = string.Format("SELECT * FROM SMIPR WHERE U_FID={0} AND CREATE_BY={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(UserId));
            }
            DataTable subDt = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
//            string sql3 = string.Format(@"SELECT U_Id,Group_Id,Cmp FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIPM WHERE U_ID={0})
//UNION SELECT U_Id,Group_Id,Cmp FROM SMDN WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIPM WHERE U_ID={0}) UNION SELECT U_Id,Group_Id,Cmp FROM SMINM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIPM WHERE U_ID={0})", SQLUtils.QuotedStr(u_id));
            string sql3 = string.Format(@";with REF_DATA AS(
select SMDN.U_Id,SMDN.Group_Id,SMDN.Cmp,SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN where SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIPM WHERE U_ID={0})
union all
select SMDN.U_Id,SMDN.Group_Id,SMDN.Cmp,SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN inner join REF_DATA r on   r.REF_NO = SMDN.DN_NO 
)
select U_Id,Group_Id,Cmp from REF_DATA
union
SELECT U_Id,Group_Id,Cmp FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIPM WHERE U_ID={0})
UNION select U_ID,GROUP_ID,CMP from SMINM WHERE  SHIPMENT_ID  IN (SELECT SHIPMENT_ID FROM SMIPM WHERE U_ID={0}) ", SQLUtils.QuotedStr(u_id));
            
            DataTable doc = OperationUtils.GetDataTable(sql3, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SMIPMModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SMIPRModel");
            data["doc"] = ModelFactory.ToTableJson(doc);
            return ToContent(data);
        }
        public ActionResult GetSMIPCDetail()
        {
            string u_id = Request["UId"];
            string JobType = Request["JobType"];
            string sql = string.Format("SELECT * FROM SMIPC WHERE U_FID={0} AND JOB_TYPE={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(JobType));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sub2"] = ModelFactory.ToTableJson(subDt, "SMIPCModel");
            return ToContent(data);
        }
        public ActionResult SaveInsuranceData()
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
            string rfq_no = Request["rfq_no"];
            string u_id = Request["u_id"];
            string JobNo = Request["Job_No"];
            string JobType = Request["JobType"];
            string Iflag = Request["Iflag"];
            string Cflag = Request["Cflag"];
            string Fflag = Request["Fflag"];
            string Tflag = Request["Tflag"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            if (!string.IsNullOrEmpty(rfq_no))
                rfq_no = HttpUtility.UrlDecode(rfq_no);
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            string sql2 = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMIPMModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            //ei.Put("GROUP_ID", GroupId);
                            //ei.Put("CMP", CompanyId);
                            ei.Put("STATUS", "N");//ei.Put("STATUS", "Y");
                            ei.Put("IFLAG", Iflag);
                            ei.Put("CFLAG", Cflag);
                            ei.Put("FFLAG", Fflag);
                            ei.Put("TFLAG", Tflag);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;                            
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("GROUP_ID", GroupId);

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("IFLAG", Iflag);
                            ei.Put("CFLAG", Cflag);
                            ei.Put("FFLAG", Fflag);
                            ei.Put("TFLAG", Tflag);
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;                           
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMIPRModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string uid = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("U_FID", u_id);
                            //ei.Put("GROUP_ID", GroupId);
                            //ei.Put("CMP", CompanyId);
                            //ei.Put("CREATE_BY", UserId);
                            //ei.PutDate("CREATE_DATE", DateTime.Now);
                            //数据验证
                            //sql = string.Format("SELECT * FROM SMRQM WHERE RFQ_NO={0} "
                            //  , SQLUtils.QuotedStr(ei.Get("RFQ_NO")));

                            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //if (dt.Rows.Count > 0)
                            //    return Json(new { message = "已存在该组数据" });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            //ei.Put("MODIFY_BY", UserId);
                            //ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);
                    }          
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMIPCModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string uid = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("U_FID", u_id);
                            ei.Put("JOB_NO", JobNo);
                            ei.Put("JOB_TYPE", JobType);

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            //ei.Put("MODIFY_BY", UserId);
                            //ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
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
            //計算費用
            /*
            sql = string.Format("SELECT * FROM SMIPM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (Dt.Rows.Count > 0) 
            {
                decimal LossUamt = Prolink.Math.GetValueAsDecimal(Dt.Rows[0]["LOSS_AMT"]) * GetExRate(Dt.Rows[0]["LOSS_CUR"].ToString());
                decimal IUamt = Prolink.Math.GetValueAsDecimal(Dt.Rows[0]["IAMT"]) * GetExRate(Dt.Rows[0]["ICUR"].ToString());
                decimal CUamt = Prolink.Math.GetValueAsDecimal(Dt.Rows[0]["CAMT"]) * GetExRate(Dt.Rows[0]["CCUR"].ToString());
                decimal FUamt = Prolink.Math.GetValueAsDecimal(Dt.Rows[0]["FAMT"]) * GetExRate(Dt.Rows[0]["FCUR"].ToString());
                decimal TUamt = Prolink.Math.GetValueAsDecimal(Dt.Rows[0]["TAMT"]) * GetExRate(Dt.Rows[0]["TCUR"].ToString());
                decimal TPVLamt = LossUamt - (IUamt + CUamt + FUamt + TUamt);

                EditInstruct ei = new EditInstruct("SMIPM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", u_id);
                ei.Put("LOSS_UAMT", LossUamt);
                ei.Put("IUAMT", IUamt);
                ei.Put("CUAMT", CUamt);
                ei.Put("FUAMT", FUamt);
                ei.Put("TUAMT", TUamt);
                ei.Put("TPV_LAMT", TPVLamt);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }*/
            sql = string.Format("SELECT * FROM SMIPM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql2 = string.Format("SELECT * FROM SMIPR WHERE U_FID={0} AND CREATE_BY={1}",SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(UserId));
            DataTable subDt = OperationUtils.GetDataTable(sql2,null,Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SMIPMModel");
            data["sub"] = ModelFactory.ToTableJson(subDt,"SMIPRModel");
            return ToContent(data);
        }

        public ActionResult SaveInsuranceDetailData()
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
            string JobNo = Request["Job_No"];
            string JobType = Request["Job_Type"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMIPCModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string uid = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("U_FID", u_id);
                            ei.Put("JOB_NO",JobNo);
                            ei.Put("JOB_TYPE", JobType);
                            
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            //ei.Put("MODIFY_BY", UserId);
                            //ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
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
            
            sql = string.Format("SELECT * FROM SMIPC WHERE U_FID={0} AND JOB_TYPE={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(JobType));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //計算費用
            if (mainDt.Rows.Count > 0) 
            {
                decimal totalAmt = 0; string AmtColumn = ""; string UAmtColumn = "";               
                switch (JobType) 
                {
                    case "I":
                        AmtColumn = "IAMT";UAmtColumn="IUAMT";
                        break;
                    case "C":
                        AmtColumn = "CAMT";UAmtColumn="CUAMT";
                        break;
                    case "F":
                        AmtColumn = "FAMT";UAmtColumn="FUAMT";
                        break;
                    case "T":
                        AmtColumn = "TAMT";UAmtColumn="TUAMT";
                        break;
                }
                foreach (DataRow item in mainDt.Rows) 
                {
                    decimal IpAmt = Prolink.Math.GetValueAsDecimal(item["IP_AMT"]);
                    totalAmt += IpAmt;
                }
                sql = string.Format("UPDATE SMIPM SET "+AmtColumn+"={0} WHERE U_ID={1}",totalAmt,SQLUtils.QuotedStr(u_id));
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sub2"] = ModelFactory.ToTableJson(mainDt, "SMIPCModel");
            data["message"] = returnMessage;
            return ToContent(data);
        }

        public ActionResult ChangeInsuranceStatus()
        {
            string returnMsg = "success";
            string status = Prolink.Math.GetValueAsString(Request.Params["Status"]);
            string Uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            string sql = "";
            string sql2 = "";
            MixedList mixList = new MixedList();
            EditInstruct ei = new EditInstruct("SMIPM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", Uid);
            ei.Put("STATUS", status);
            if (status == "C") 
            {
                ei.Put("CONFIRM_BY", UserId);
                DateTime odt = DateTime.Now;                
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("CONFIRM_DATE", odt);
                ei.PutDate("CONFIRM_DATE_L", ndt);
            }
            else if (status == "A") 
            {
                ei.Put("AC_BY", UserId);
                DateTime odt = DateTime.Now;               
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("AC_DATE", odt);
                ei.PutDate("AC_DATE_L", ndt);
            }
            else if (status == "F") 
            {
                ei.Put("ENDING_BY", UserId);
                DateTime odt = DateTime.Now;               
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("ENDING_DATE", odt);
                ei.PutDate("ENDING_DATE_L", ndt);
            }
            else if (status == "Y")
            {
                ei.Put("APPLICATION_BY", UserId);
                DateTime odt = DateTime.Now;                
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("APPLICATION_DATE", odt);
                ei.PutDate("APPLICATION_DATE_L", ndt);
            }      

            mixList.Add(ei);

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception e)
                {
                    returnMsg = e.ToString();
                }
                sql = string.Format("SELECT * FROM SMIPM WHERE U_ID={0}", SQLUtils.QuotedStr(Uid));
                DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql2 = string.Format("SELECT * FROM SMIPR WHERE U_FID={0}", SQLUtils.QuotedStr(Uid));
                DataTable subDt = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, object> data = new Dictionary<string, object>();
                data["main"] = ModelFactory.ToTableJson(mainDt, "SMIPMModel");
                data["sub"] = ModelFactory.ToTableJson(subDt, "SMIPRModel");
                return Json(new { message = returnMsg, returnData = ToContent(data) });
            }
            return Json(new { message = returnMsg });
        }
        public ActionResult GetInvNo() 
        {
            string BlNo = Prolink.Math.GetValueAsString(Request.Params["BlNo"]);
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string sql = string.Format("SELECT * FROM SMINM WHERE BL_NO={0}", SQLUtils.QuotedStr(BlNo));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string InvNoList = "", CntNoList = string.Empty;
            if (mainDt.Rows.Count > 0) 
            {
                for (int i = 0; i < mainDt.Rows.Count; i++) 
                {
                    string InvNo = mainDt.Rows[i]["INV_NO"].ToString();
                    InvNoList = InvNoList + InvNo + ",";
                }
            }
            InvNoList = InvNoList.TrimEnd(',');

            sql = string.Format("SELECT CNTR_NO FROM SMRV WHERE SHIPMENT_ID={0} AND CNTR_NO IS NOT NULL", SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count > 0)
            {
                foreach(DataRow item in dt.Rows)
                {
                    string CntrNo = Prolink.Math.GetValueAsString(item["CNTR_NO"]);
                    CntNoList = CntNoList + CntrNo + ",";
                }
            }

            CntNoList = CntNoList.TrimEnd(',');
            return Json(new{InvNoList=InvNoList, CntNoList=CntNoList}); 
        }

        public decimal GetExRate(string Cur) 
        {
            string sql = string.Format("SELECT EX_RATE FROM BSERATE WHERE GROUP_ID={0} AND CMP = {1} AND FCUR = {2} AND TCUR='USD' AND ETYPE='R' ORDER BY EDATE", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(BaseCompanyId), SQLUtils.QuotedStr(Cur));
            decimal rate = Prolink.Math.GetValueAsDecimal(OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection()));
            return rate;
        }
        public ActionResult GetInsFeelookup() 
        {
            string Uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMIPR", "", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        public ActionResult GetSmsmForSMIPMLookup()
        {
            string con = "";
            if (IOFlag == "O")
            {
                con = " SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMSMPT WHERE PARTY_NO={0})";
                con = string.Format(con, SQLUtils.QuotedStr(CompanyId));
            }
            else
            {
                con = " CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND (HOUSE_NO is not null OR MASTER_NO is not null)";
            }

            return GetBootstrapData(@" (SELECT *  FROM SMSM LEFT JOIN (select distinct SHIPMENT_ID AS RV_SHIPMENT_ID,stuff((select ','+CNTR_NO from SMRV t where 
 t.SHIPMENT_ID= SMRV.SHIPMENT_ID  AND (t.RV_TYPE<>'I' OR t.RV_TYPE IS NULL)
 for xml path('')), 1, 1, '') AS CNTR_NO from SMRV )B ON B.RV_SHIPMENT_ID=SMSM.SHIPMENT_ID) V_SMSM", con, "*");
        }

        public ActionResult GetSmsmIForSMIPMLookup()
        {
            string con = "";
            if (IOFlag == "O")
            {
                con = " SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMSMIPT WHERE PARTY_NO={0})";
                con = string.Format(con, SQLUtils.QuotedStr(CompanyId));
            }
            else
            {
                con = " CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND (HOUSE_NO is not null OR MASTER_NO is not null)";
            }

            return GetBootstrapData(@" (SELECT *  FROM SMSMI LEFT JOIN (select distinct SHIPMENT_ID AS RV_SHIPMENT_ID,stuff((select ','+CNTR_NO from SMIRV t where 
 t.SHIPMENT_ID= SMIRV.SHIPMENT_ID  AND t.RV_TYPE ='I'
 for xml path('')), 1, 1, '') AS CNTR_NO from SMIRV )B ON B.RV_SHIPMENT_ID=SMSMI.SHIPMENT_ID) V_SMSM", con, "*");
        }
        public static string getBscodeForSelect(string CdType, string pmsCon)
        {
            string sel = string.Empty;
            string sql = "SELECT CD, CD_DESCP FROM BSCODE WHERE CD_TYPE={0} AND {1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(CdType), pmsCon);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string Cd = Prolink.Math.GetValueAsString(item["CD"]);
                    string CdDescp = Prolink.Math.GetValueAsString(item["CD_DESCP"]);
                    sel += "<option value='" + Cd + "'>" + CdDescp + "</option>";
                }
            }

            return sel;
        }
        #endregion

        /// <summary>
        /// 签核账单费用
        /// </summary>
        /// <returns></returns>
        public ActionResult ApproveSMBID()
        {
            string ids = Request["ids"];//通知人数组
            string[] list = ids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            EditInstruct ei = null;
            MixedList ml = new MixedList();
            for (int i = 0; i < list.Length; i++)
            {
                ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", list[i]);
                ei.Condition = "U_FID IS NULL";
                ei.Put("APPROVE_BY", this.UserId);
                ei.Put("APPROVE_STATUS", "Y");
                DateTime odt = DateTime.Now;               
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("APPROVE_DATE", odt);
                ei.PutDate("APPROVE_DATE_L", ndt);
                Business.TPV.Financial.InboundBill.SetEamt(ei);
                SetCStask(list[i], ml);
                ml.Add(ei);
            }

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message });
                }
            }
            return Json(new { message = @Resources.Locale.L_ActManage_Controllers_75, flag = true });
        }

        public ActionResult UnApproveSMBIDs()
        {
            string msg = Request["msg"] != null ? HttpUtility.UrlDecode(Request["msg"]) : string.Empty;

            string ids = Request["ids"];//通知人数组
            string[] list = ids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            EditInstruct ei = null;
            MixedList ml = new MixedList();
            for (int i = 0; i < list.Length; i++)
            {
                ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", list[i]);
                ei.Condition = "U_FID IS NULL";
                ei.Put("APPROVE_BY", this.UserId);
                ei.Put("APPROVE_STATUS", "N");
                if (!string.IsNullOrEmpty(msg))
                    ei.Put("UNAPPROVE_DESCP", msg);
                DateTime odt = DateTime.Now;                
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("APPROVE_DATE", odt);
                ei.PutDate("APPROVE_DATE_L", ndt);
                Business.TPV.Financial.InboundBill.SetEamt(ei);
                SetCStask(list[i], ml);
                ml.Add(ei);
            }

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message });
                }
            }
            return Json(new { message = @Resources.Locale.L_ActManage_Controllers_75, flag = true });
        }

        public void SetBimTask(string[] uids)
        {
            string sql1 = string.Format("SELECT DISTINCT SHIPMENT_ID,GROUP_ID,CMP FROM SMBID WHERE U_FID IN {0}", SQLUtils.Quoted(uids));
            DataTable biddt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (biddt != null && biddt.Rows.Count > 0)
            {
                MixedList ml = new MixedList();
                foreach (DataRow dr in biddt.Rows)
                {
                    string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    string groupid = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                    string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                    TrackingEDI.Business.CostStatistics.SetCStask(ShipmentId, groupid, cmp, UserId, ml);
                }
                if (ml.Count > 0)
                {
                    try
                    {
                        int[] c = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public void SetCStask(string uid, MixedList ml)
        {
            if (string.IsNullOrEmpty(uid)) return;
            string sql = "SELECT SHIPMENT_ID,GROUP_ID,CMP FROM SMBID WITH (NOLOCK) WHERE U_ID =" + SQLUtils.QuotedStr(uid);
            DataTable biDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in biDt.Rows)
            {
                string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string groupid = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                TrackingEDI.Business.CostStatistics.SetCStask(ShipmentId, groupid, cmp, UserId, ml);
            }
        }

        public ActionResult UnApproveSMBID()
        {
            string id = Request["id"];
            string msg = Request["msg"] != null ? HttpUtility.UrlDecode(Request["msg"]) : string.Empty;

            MixedList ml = new MixedList();
            EditInstruct ei = ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", id);
            ei.Condition = "U_FID IS NULL";
            ei.Put("APPROVE_BY", this.UserId);
            ei.Put("APPROVE_STATUS", "N");
            if (!string.IsNullOrEmpty(msg))
                ei.Put("UNAPPROVE_DESCP", msg);
            DateTime odt = DateTime.Now;           
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("APPROVE_DATE", odt);
            ei.PutDate("APPROVE_DATE_L", ndt);
            SetCStask(id, ml);
            ml.Add(ei);

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message });
                }
            }
            return Json(new { message = @Resources.Locale.L_ActManage_Controllers_75, flag = true });
        }

        public ActionResult GetExportData()
        {
            string uid = Prolink.Math.GetValueAsString(Request["uid"]);
            if (String.IsNullOrEmpty(uid)) return ToContent(null);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');

          
            string sql = string.Format(@" 
                SELECT U_ID,GROUP_ID,CMP,DEBIT_NO FROM SMBIM WHERE U_ID IN {0}
                UNION
                SELECT SMSMI.U_ID,SMSMI.GROUP_ID,SMSMI.CMP,SMBIM.DEBIT_NO FROM SMSMI,SMBIM
                WHERE SMSMI.SHIPMENT_ID=SMBIM.SHIPMENT_ID AND SMBIM.U_ID IN {0}", SQLUtils.Quoted(uids));
            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["dn"] = ModelFactory.ToTableJson(Dt, "SmbimModel");
            return ToContent(data);
        }


        /// <summary>
        /// 设置操作人和操作日期  如果为空就是系统产生的
        /// </summary>
        /// <param name="ei"></param>
        private void SetUserAndDate(EditInstruct ei)
        {
            if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
            {
                ei.Put("MODIFY_BY", this.UserId);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            else if (ei.OperationType == EditInstruct.INSERT_OPERATION)
            {
                ei.Put("CREATE_BY", this.UserId);
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
        }

        #region db优化版导出excel


        public string DeleteTempDB(string id)
        {
            return string.Format(@"if object_id('tempdb..#SM_DATA{0}') is not null
    begin
        drop table #SM_DATA{0} 
    end
	if object_id('tempdb..#DN_DATA{0}') is not null
    begin
        drop table #DN_DATA{0} 
    end
    if object_id('tempdb..#DN_DATA1{0}') is not null
    begin
        drop table #DN_DATA1{0} 
    end
	if object_id('tempdb..#IPART_DATA{0}') is not null
    begin
        drop table #IPART_DATA{0} 
    end
    if object_id('tempdb..#IPART_DATA1{0}') is not null
    begin
        drop table #IPART_DATA1{0} 
    end
	if object_id('tempdb..#CNTR_DATA{0}') is not null
    begin
        drop table #CNTR_DATA{0} 
    end", id) + System.Environment.NewLine;
        }

        /// <summary>
        /// 导出请款账单
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportBBillExcel()
        {
            string tranType = Request["virtualCol"];
            string uid = Request["conditions"];
            string smid = Request["baseCondition"];

            string sql = "SELECT * FROM (SELECT * FROM SMBID WITH (NOLOCK) WHERE U_FID=" + SQLUtils.QuotedStr(uid) + ")A OUTER APPLY (SELECT TOP 1 TRAN_TYPE FROM SMSM M WITH (NOLOCK) WHERE M.SHIPMENT_ID=A.SHIPMENT_ID) B ORDER BY SHIPMENT_ID,IPART_NO DESC,CNTR_STD_QTY DESC";
            DataTable bidDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string ticks = DateTime.Now.Ticks.ToString();
            if (ticks.Contains(".")) ticks = ticks.Substring(0, ticks.IndexOf("."));

            string s1 = string.Format("SELECT DISTINCT SHIPMENT_ID into #SM_DATA{1} FROM SMBID WITH (NOLOCK) WHERE U_FID={0}", SQLUtils.QuotedStr(uid), ticks) + System.Environment.NewLine;
            string s2 = string.Format(@"SELECT COST_CENTER,COST_CENTERDESCP,DN_NO AS DN_NO1,SHIPMENT_ID,AMOUNT1,QTY AS DN_QTY,QTYU AS DN_QTYU into #DN_DATA{0} FROM SMDN WITH (NOLOCK) WHERE
                  SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{0})", ticks) + System.Environment.NewLine;
            string s3 = string.Format(@"SELECT CNTR_NO,DN_NO,SHIPMENT_ID,IN_DATE,OUT_DATE,CALL_DATE into #CNTR_DATA{0} FROM SMRV WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{0})", ticks) + System.Environment.NewLine;
            string s4 = string.Empty;
            string s5 = string.Empty;
            string s6 = string.Empty;
            string col =string.Format(@",(ISNULL(COMBINE_INFO, '')) AS COMBINE_INFO1
,(SELECT COST_CENTER+';' FROM #DN_DATA{0} WHERE #DN_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID FOR XML PATH('')) AS EX_COST_CENTERS
,(SELECT COST_CENTERDESCP+';' FROM #DN_DATA{0} WHERE #DN_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID FOR XML PATH('')) AS EX_COST_CENTERDESCPS

,(SELECT CONVERT(varchar, IN_DATE, 120 )+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID AND IN_DATE IS NOT NULL FOR XML PATH('')) AS IN_DATE
,(SELECT CONVERT(varchar, OUT_DATE, 120 )+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID AND OUT_DATE IS NOT NULL FOR XML PATH('')) AS OUT_DATE
,(SELECT CONVERT(varchar, CALL_DATE, 120 )+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID AND CALL_DATE IS NOT NULL FOR XML PATH('')) AS CALL_DATE
,(SELECT DISTINCT CNTR_NO+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID  AND CNTR_NO IS NOT NULL FOR XML PATH('')) AS EX_CNTR_NOS
,'EX_CNT_TYPE'=CASE 
     WHEN (SMSM.PCNT20 > 0) THEN
          '20GP*' + convert(varchar(10), SMSM.CNT20) + ','
         ELSE
          ''
       END + CASE
         WHEN (SMSM.PCNT40 > 0) THEN
          '40GP*' + convert(varchar(10), SMSM.CNT40) + ','
         ELSE
          ''
       END + CASE
         WHEN (SMSM.PCNT40HQ > 0) THEN
          '40HQ*' + convert(varchar(10), SMSM.CNT40HQ) + ','
         ELSE
          '' END ", ticks);

            sql = string.Format(@"SELECT *{1} FROM SMSM WHERE SHIPMENT_ID IN ({0})", "SELECT SHIPMENT_ID FROM #SM_DATA" + ticks, col);
            switch (tranType)
            {
                case "T"://内贸零担
                case "TT"://内贸零担
                    s4 = string.Format(@"SELECT AMOUNT1,DN_QTY,DN_QTYU,IPART_NO,CNTR_STD_QTY,A.DN_NO AS DN_NO2,#DN_DATA{0}.SHIPMENT_ID AS SHIPMENT_ID1 INTO #IPART_DATA{0} FROM (SELECT  IPART_NO,CNTR_STD_QTY,DN_NO FROM SMDNP WITH (NOLOCK) WHERE 
 DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{0})) AND CNTR_STD_QTY>0 AND (NEW_CATEGORY<> N'TANN' OR NEW_CATEGORY IS NULL)) A 
 LEFT JOIN #DN_DATA{0} ON A.DN_NO=#DN_DATA{0}.DN_NO1", ticks) + System.Environment.NewLine;

                    s5 = string.Format(@"SELECT IPART_NO AS IPART_NO1,CNTR_STD_QTY AS CNTR_STD_QTY1,SHIPMENT_ID INTO #IPART_DATA1{0} FROM SMBDD WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{0}) AND CNTR_STD_QTY>0 AND IPART_NO IS NOT NULL", ticks) + System.Environment.NewLine;

                    s6 = string.Format(@"SELECT * into #DN_DATA1{0} FROM #DN_DATA{0} WHERE
                  ((SELECT SUM(CNTR_STD_QTY) FROM #IPART_DATA{0} WHERE #IPART_DATA{0}.SHIPMENT_ID1=#DN_DATA{0}.SHIPMENT_ID)<=0 AND (SELECT SUM(CNTR_STD_QTY1) FROM #IPART_DATA1{0} WHERE #IPART_DATA1{0}.SHIPMENT_ID=#DN_DATA{0}.SHIPMENT_ID)<=0)", ticks) + System.Environment.NewLine;

                    sql = string.Format("SELECT A.*,#DN_DATA1{2}.*,IPART_NO,CNTR_STD_QTY,IPART_NO1,CNTR_STD_QTY1,DN_NO2 FROM (SELECT SMSM.*{1},(SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_CD,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_NM FROM SMSM WHERE SHIPMENT_ID IN ({0}))A LEFT JOIN #IPART_DATA{2} ON A.SHIPMENT_ID=#IPART_DATA{2}.SHIPMENT_ID1 LEFT JOIN #IPART_DATA1{2} ON A.SHIPMENT_ID=#IPART_DATA1{2}.SHIPMENT_ID LEFT JOIN #DN_DATA1{2} ON A.SHIPMENT_ID=#DN_DATA1{2}.SHIPMENT_ID ORDER BY A.SHIPMENT_ID,#DN_DATA1{2}.DN_NO1 DESC,IPART_NO DESC,IPART_NO1 DESC,CNTR_STD_QTY DESC", "SELECT SHIPMENT_ID FROM #SM_DATA" + ticks, col, ticks);
                    break;
            }
            sql = DeleteTempDB(ticks) + s1 + s2 + s3 + s4 + s5 + s6 + sql + System.Environment.NewLine + DeleteTempDB(ticks);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            FileResult result = ExportAmtExcel(bidDt, dt, "BAMT",null,"","",false,tranType);
            return result;
        }


        /// <summary>
        /// 导出预提账单
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportAllBillExcel()
        {
            #region 设置查询条件
            string virtualCol = Request["virtualCol"];
            if (!string.IsNullOrEmpty(virtualCol)) virtualCol = System.Web.HttpUtility.UrlDecode(virtualCol);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dictionary<string, object> types = jss.Deserialize(virtualCol, typeof(Dictionary<string, object>)) as Dictionary<string, object>;
            string tranType = Prolink.Math.GetValueAsString(types["transType"]);
            string haveTransType = (types != null && types.ContainsKey("haveTransType")) ? Prolink.Math.GetValueAsString(types["transType"]) : string.Empty;
            string location = (types != null && types.ContainsKey("location")) ? Prolink.Math.GetValueAsString(types["location"]) : string.Empty;
            string excelType = (types != null && types.ContainsKey("excelType")) ? Prolink.Math.GetValueAsString(types["excelType"]) : string.Empty;
            string lspNo = (types != null && types.ContainsKey("lspNo")) ? Prolink.Math.GetValueAsString(types["lspNo"]) : string.Empty;

            string condition = ModelFactory.GetInquiryCondition("SMBID", "", Request.Params);
            if (string.IsNullOrEmpty(condition) || string.IsNullOrEmpty(condition.Trim()))
                condition = "1=1";
            condition += " AND BAMT IS NOT NULL AND BAMT<>0 AND U_FID IS NULL";

            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();

            string table = string.Format(@"(SELECT SMBID.*,(SELECT TOP 1 POL_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
          (SELECT TOP 1 STATUS
          FROM SMSM WITH(NOLOCK)
         WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS ,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.DEBIT_NO=SMBID.DEBIT_NO WHERE SMBID.GROUP_ID={0} AND SMBID.LSP_NO={1} )T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));

            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                //if ("G".Equals(upri))
                //    innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                //else
                //    innerCondition = "SMBID.GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND SMBID.CMP=" + SQLUtils.QuotedStr(this.CompanyId);
                table = string.Format(@"(SELECT SMBID.*,(SELECT TOP 1 POL_CD FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID) AS POL_CD ,
          (SELECT TOP 1 STATUS
          FROM SMSM WITH(NOLOCK)
         WHERE SMSM.SHIPMENT_ID = SMBID.SHIPMENT_ID) AS SM_STATUS ,SMBIM.TPV_DEBIT_NO FROM SMBID  LEFT JOIN SMBIM  ON SMBIM.DEBIT_NO=SMBID.DEBIT_NO WHERE {0})T", innerCondition);//AND SMBID.LSP_NO={2}
            }
            location = this.CompanyId;
            if (!string.IsNullOrEmpty(location))
                condition += string.Format(" AND CMP={0}", SQLUtils.QuotedStr(location));
            string sql = "SELECT * FROM " + table + " WHERE " + condition;

            //string sql = "SELECT * FROM (SELECT * FROM SMBID WITH (NOLOCK) WHERE " + condition + ")A OUTER APPLY (SELECT TOP 1 TRAN_TYPE FROM SMSM M WITH (NOLOCK) WHERE M.SHIPMENT_ID=A.SHIPMENT_ID) B ORDER BY SHIPMENT_ID,IPART_NO DESC,CNTR_STD_QTY DESC";
            DataTable biDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            #region 数据预查
            string ticks = DateTime.Now.Ticks.ToString();
            if (ticks.Contains(".")) ticks = ticks.Substring(0, ticks.IndexOf("."));
            string s1 = string.Format("SELECT DISTINCT SHIPMENT_ID into #SM_DATA{1} FROM {0} WHERE {2}", table, ticks, condition) + System.Environment.NewLine;
            string s2 = string.Format(@"SELECT COST_CENTER,COST_CENTERDESCP,DN_NO AS DN_NO1,SHIPMENT_ID,AMOUNT1,QTY AS DN_QTY,QTYU AS DN_QTYU into #DN_DATA{0} FROM SMDN WITH (NOLOCK) WHERE
                  SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{0})", ticks) + System.Environment.NewLine;
            string s3 = string.Format(@"SELECT CNTR_NO,DN_NO,SHIPMENT_ID,IN_DATE,OUT_DATE,CALL_DATE into #CNTR_DATA{0} FROM SMRV WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{0})", ticks) + System.Environment.NewLine;
            string s4 = string.Empty;

            string col = string.Format(@",(ISNULL(COMBINE_INFO, '')) AS COMBINE_INFO1
,(SELECT CONVERT(varchar, IN_DATE, 120 )+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID AND IN_DATE IS NOT NULL FOR XML PATH('')) AS IN_DATE
,(SELECT CONVERT(varchar, OUT_DATE, 120 )+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID AND OUT_DATE IS NOT NULL FOR XML PATH('')) AS OUT_DATE
,(SELECT CONVERT(varchar, CALL_DATE, 120 )+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID AND CALL_DATE IS NOT NULL FOR XML PATH('')) AS CALL_DATE

,(SELECT COST_CENTER+';' FROM #DN_DATA{0} WHERE #DN_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID FOR XML PATH('')) AS EX_COST_CENTERS
,(SELECT COST_CENTERDESCP+';' FROM #DN_DATA{0} WHERE #DN_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID FOR XML PATH('')) AS EX_COST_CENTERDESCPS
,(SELECT DISTINCT CNTR_NO+',' FROM #CNTR_DATA{0} WHERE #CNTR_DATA{0}.SHIPMENT_ID=SMSM.SHIPMENT_ID AND CNTR_NO IS NOT NULL FOR XML PATH('')) AS EX_CNTR_NOS
,'EX_CNT_TYPE'=CASE 
     WHEN (SMSM.PCNT20 > 0) THEN
          '20GP*' + convert(varchar(10), SMSM.CNT20) + ','
         ELSE
          ''
       END + CASE
         WHEN (SMSM.PCNT40 > 0) THEN
          '40GP*' + convert(varchar(10), SMSM.CNT40) + ','
         ELSE
          ''
       END + CASE
         WHEN (SMSM.PCNT40HQ > 0) THEN
          '40HQ*' + convert(varchar(10), SMSM.CNT40HQ) + ','
         ELSE
          '' END ", ticks);
            #endregion

            sql = string.Format(@"SELECT *{1} FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{3}{0}){2}", string.Empty, col, "", ticks);

            switch (tranType)
            {
                case "TTT"://内贸零担
                    s4 = string.Format(@"SELECT AMOUNT1,DN_QTY,DN_QTYU,IPART_NO,CNTR_STD_QTY,A.DN_NO AS DN_NO1,#DN_DATA{0}.SHIPMENT_ID AS SHIPMENT_ID1 INTO #IPART_DATA{0} FROM (SELECT  IPART_NO,CNTR_STD_QTY,DN_NO FROM SMDNP WITH (NOLOCK) WHERE 
 DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{0})) AND CNTR_STD_QTY>0 AND (NEW_CATEGORY<> N'TANN' OR NEW_CATEGORY IS NULL)) A 
 LEFT JOIN #DN_DATA{0} ON A.DN_NO=#DN_DATA{0}.DN_NO1", ticks) + System.Environment.NewLine;

                    sql = string.Format("SELECT * FROM (SELECT SMSM.*{1},(SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_CD,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE PARTY_TYPE='RG' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS RG_NM FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM #SM_DATA{3}{0}){2})A LEFT JOIN #IPART_DATA{3} ON A.SHIPMENT_ID=#IPART_DATA{3}.SHIPMENT_ID1 ORDER BY SHIPMENT_ID,DN_NO1 DESC,IPART_NO DESC,CNTR_STD_QTY DESC", string.Empty, col, "", ticks);
                    break;

            }
            sql = DeleteTempDB(ticks) + s1 + s2 + s3 + s4 + sql + System.Environment.NewLine + DeleteTempDB(ticks);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            //FileResult result = ExportAmtExcel(biDt, dt, "QLAMT", transTypeCols, condition);
            FileResult result = ExportAmtExcel(biDt, dt, "BAMT", null,"","",true);
            return result;
        }

        private static void AddBillExcelItem(ArrayList arraylist, string name, string title)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["name"] = name;
            item["title"] = title;
            item["index"] = name;
            item["sorttype"] = 0;
            item["width"] = 100;
            item["align"] = "right";
            item["formatter"] = "number";
            item["formatoptions"] = new Dictionary<string, object> { { "decimalSeparator", "." }, { "thousandsSeparator", "," }, { "decimalPlaces", 2 }, { "defaultValue", "0.00" } };
            item["hidden"] = false;
            item["caption"] = title;
            arraylist.Add(item);
        }
        #endregion

        #region 匯率換算
        public ActionResult changeExrate()
        {
            string Fcur = Prolink.Math.GetValueAsString(Request.Params["Fcur"]);
            string Tcur = Prolink.Math.GetValueAsString(Request.Params["Tcur"]);
            decimal Famt = Prolink.Math.GetValueAsDecimal(Request.Params["Famt"]);
            decimal amt = 0;
            string samt = string.Empty;
            double exRate;
            amt = CommonHelp.changeExrate_new(Fcur, Tcur, Famt, out exRate);

            amt = System.Math.Round(amt, 2);

            samt = string.Format("{0:N}", amt);

            return Json(new { amt = samt,exRate=exRate });
        }
        #endregion

        public ActionResult ExportExcel()
        {
            string msg = string.Empty;
            bool result = false;

            string uid = Prolink.Math.GetValueAsString(Request["UId"]);
            string table = string.Format(@"( SELECT SMBIM.U_ID,SMSMI.CMP,SMSMI.SHIPMENT_ID,SMSMI.COMBINE_INFO,STUFF((SELECT ','+CNTR_NO FROM SMICNTR T WHERE 
 T.SHIPMENT_ID= SMSMI.SHIPMENT_ID 
 FOR XML PATH('')), 1, 1, '') CNTR_NO,
 STUFF((SELECT ','+DEC_NO FROM SMIDN T WHERE 
 T.SHIPMENT_ID= SMSMI.SHIPMENT_ID 
 FOR XML PATH('')), 1, 1, '') DEC_NO, SMSMI.MASTER_NO MASTER_NO1,
 SMSMI.HOUSE_NO,SMSMI.MASTER_NO,
 (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE  SMSMIPT.SHIPMENT_ID= SMSMI.SHIPMENT_ID  AND PARTY_TYPE='FC') FC_NM,INCOTERM_CD,DEST_NAME, 
 POL_NAME,'' TCBM,SMSMI.CBM,SMIRV.CALL_DATE,SMIRV.IN_DATE,SMIRV.OUT_DATE,STUFF((SELECT ','+PARTY_NAME FROM SMSMIPT T WHERE 
 T.SHIPMENT_ID= SMSMI.SHIPMENT_ID  AND PARTY_TYPE='IBCR'
 FOR XML PATH('')), 1, 1, '') CR_NM,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE  SMSMIPT.SHIPMENT_ID= SMSMI.SHIPMENT_ID  AND PARTY_TYPE='IBSP') SP_NM,SMSMI.CNT_NUMBER
 ,SMSMI.ETD,SMSMI.VESSEL1,SMSMI.BL_WIN SALES_WIN,SMSMI.IB_WINDOW,SMSMI.INSTRUCTION,SMBIM.DEBIT_DATE,SMBIM.DEBIT_NO,SMBIM.CUR,SMBIM.AMT FROM SMBIM 
 LEFT JOIN SMSMI ON SMSMI.SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WHERE U_FID={0}) 
LEFT JOIN SMIRV ON SMSMI.SHIPMENT_ID=SMIRV.SHIPMENT_ID) M", SQLUtils.QuotedStr(uid));

            string shipmentid = Prolink.Math.GetValueAsString(Request["shipmentid"]);
            string excelType = Prolink.Math.GetValueAsString(Request["type"]);
            switch (excelType)
            {
                case "A":
                case "E": table = string.Format(@" ( SELECT SMBIM.U_ID,SMSMI.CMP,SMSMI.SHIPMENT_ID,SMSMI.COMBINE_INFO,SMSMI.ETD,
 STUFF((SELECT ','+DEC_NO FROM SMIDN T WHERE 
 T.SHIPMENT_ID= SMSMI.SHIPMENT_ID 
 FOR XML PATH('')), 1, 1, '') DEC_NO, SMSMI.MASTER_NO,
 SMSMI.HOUSE_NO,
 (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE  SMSMIPT.SHIPMENT_ID= SMSMI.SHIPMENT_ID  AND PARTY_TYPE='FC') FC_NM,POL_NAME,DEST_NAME,INCOTERM_CD, SMSMI.QTY,
 SMSMI.CW,SMBIM.DEBIT_DATE,SMBIM.DEBIT_NO,SMBIM.CUR,SMBIM.AMT FROM SMBIM
 LEFT JOIN SMSMI ON SMSMI.SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WHERE U_FID={0})) M", SQLUtils.QuotedStr(uid)); break;
                case "R":
                case "F": table = string.Format(@" (SELECT SMBIM.U_ID,SMSMI.CMP,SMSMI.SHIPMENT_ID,SMSMI.COMBINE_INFO,STUFF((SELECT ','+CNTR_NO FROM SMICNTR T WHERE 
 T.SHIPMENT_ID= SMSMI.SHIPMENT_ID 
 FOR XML PATH('')), 1, 1, '') CNTR_NO,
 STUFF((SELECT ','+DEC_NO FROM SMICNTR T WHERE 
 T.SHIPMENT_ID= SMSMI.SHIPMENT_ID 
 FOR XML PATH('')), 1, 1, '') DEC_NO, SMSMI.MASTER_NO MASTER_NO1,
 SMSMI.HOUSE_NO,SMSMI.MASTER_NO,
 (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE  SMSMIPT.SHIPMENT_ID= SMSMI.SHIPMENT_ID  AND PARTY_TYPE='FC') FC_NM,INCOTERM_CD,DEST_NAME, 
 POL_NAME,SMSMI.CARRIER,SMSMI.CNT20,SMSMI.CNT40,SMSMI.CNT40HQ,SMSMI.CNT_NUMBER,STUFF((SELECT ','+PARTY_NAME FROM SMSMIPT T WHERE 
 T.SHIPMENT_ID= SMSMI.SHIPMENT_ID   AND PARTY_TYPE='IBCR'
 FOR XML PATH('')), 1, 1, '') CR_NM,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE  SMSMIPT.SHIPMENT_ID= SMSMI.SHIPMENT_ID  AND PARTY_TYPE='IBSP') SP_NM,SMSMI.CBM,'' TCBM,
 SMSMI.ETD,SMSMI.VESSEL1,SMSMI.BL_WIN SALES_WIN,SMSMI.IB_WINDOW,SMSMI.INSTRUCTION,SMBIM.DEBIT_DATE,SMBIM.DEBIT_NO,SMBIM.CUR,SMBIM.AMT FROM SMBIM 
 LEFT JOIN SMSMI ON SMSMI.SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WHERE U_FID={0})) M", SQLUtils.QuotedStr(uid)); break;
                case "L": break;
            }
            string sql = string.Format("SELECT * FROM {0} where U_ID={1}", table, SQLUtils.QuotedStr(uid));
            string _sql = "SELECT * FROM (SELECT * FROM SMBID WITH (NOLOCK) WHERE U_FID=" + SQLUtils.QuotedStr(uid) + ")A OUTER APPLY (SELECT TOP 1 TRAN_TYPE FROM SMSM M WITH (NOLOCK) WHERE M.SHIPMENT_ID=A.SHIPMENT_ID) B ORDER BY SHIPMENT_ID,IPART_NO DESC,CNTR_STD_QTY DESC";
            DataTable bidDt = OperationUtils.GetDataTable(_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable smDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string amtField = "BAMT";
            foreach (DataRow bi in bidDt.Rows)
            {
                string chg_cd = Prolink.Math.GetValueAsString(bi["CHG_CD"]);
                string Chg_Descp = Prolink.Math.GetValueAsString(bi["CHG_DESCP"]);
                //string cur = amtField.StartsWith("Q") ? Prolink.Math.GetValueAsString(bi["QCUR"]) : Prolink.Math.GetValueAsString(bi["CUR"]);
                string key = chg_cd + "_" + Chg_Descp;

                if (!smDt.Columns.Contains(key))
                    smDt.Columns.Add(key, typeof(decimal));

                string shipment_id = Prolink.Math.GetValueAsString(bi["SHIPMENT_ID"]);
                string filter = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id));
                string ipart_no = Prolink.Math.GetValueAsString(bi["IPART_NO"]);
                string dn_no = bi.Table.Columns.Contains("DN_NO") ? Prolink.Math.GetValueAsString(bi["DN_NO"]) : string.Empty;
                decimal cntr_std_qty = Prolink.Math.GetValueAsDecimal(bi["CNTR_STD_QTY"]);
                string dn_filter = string.Empty;
                DataRow[] drs = smDt.Select(filter);
                if (drs.Length <= 0)
                    continue;
                if ("F".Equals(excelType))
                {
                    drs[0][key] = Prolink.Math.GetValueAsDecimal(drs[0][key]) + Prolink.Math.GetValueAsDecimal(bi[amtField]);
                }
                else
                {
                    drs[0][key] =  Prolink.Math.GetValueAsDecimal(bi[amtField]);
                }



            }
            string[] delcol = { "U_ID" };
            string[] col = { "LOCATION", "Shipment ID", "DN", "Container number", "Custom clerance number", "MBL NO", "House NO", "BOOKING Bill of lading No.", "FC Financial customers", "DLV TERM", "Destination(Port of destination)", "POL", "TPV Volume", "Total volume", "Arrival Date", "Gate In Date", "Gate Out Date", "Truck Company", "Service Provider", "LCL", "Sailing ETD", "Vessel name", "Sales Window", "Booking Window", "Remark", "DEBIT DATE", "DEBIT NO", "Currency", "total" };
            string[] Ocol = { "CMP", "SHIPMENT_ID", "COMBINE_INFO", "CNTR_NO", "DEC_NO", "MASTER_NO1", "HOUSE_NO", "MASTER_NO", "FC_NM", "INCOTERM_CD", "DEST_NAME", "POL_NAME", "TCBM", "CBM", "CALL_DATE", "IN_DATE", "OUT_DATE", "CR_NM", "SP_NM", "CNT_NUMBER", "ETD", "VESSEL1", "SALES_WIN", "IB_WINDOW", "INSTRUCTION", "DEBIT_DATE", "DEBIT_NO", "CUR", "AMT" };
            switch (excelType)
            {
                case "A":
                case "E":
                    Ocol = new string[] { "CMP|LOCATION", "SHIPMENT_ID|Shipment ID", "COMBINE_INFO|DN INFO", "ETD|Sailing ETD", "DEC_NO|Custom clerance number", "MASTER_NO|MAWB", "HOUSE_NO|HAWB", "FC_NM|FC Financial customers", "POL_NAME|POL", "DEST_NAME|Destination(Port of destination)", "INSTRUCTION|DLV TERM", "	QTY|Quantity", "CW|Chargeable Weight", "DEBIT_DATE|DEBIT_DATE", "DEBIT_NO|DEBIT_NO", "CUR|CUR", "AMT|AMT" };
                    ChangeCol(Ocol, ref smDt, false, delcol);
                    break;
                case "L":
                    ChangeCol(Ocol, col, ref smDt, false, delcol); break;
                case "R":
                case "F": Ocol = new string[] { "CMP|LOCATION", "SHIPMENT_ID|Shipment ID", "COMBINE_INFO|DN", "CNTR_NO|Container number", "DEC_NO|Custom clerance number", "MASTER_NO1|MBL NO", "HOUSE_NO|House NO", "MASTER_NO|BOOKING Bill of lading No.", "FC_NM|FC Financial customers", "INCOTERM_CD|DLV TERM", "DEST_NAME|Destination(Port of destination)", "POL_NAME|POL", "CARRIER|CARRIER", "CNT20|20GP", "CNT40|40GP", "CNT40HQ|HQ", "CNT_NUMBER|LCL", "CR_NM|Truck Company", "SP_NM|Service Provider", "CBM|TPV Volume", "TCBM|Total volume", "ETD|Sailing ETD", "VESSEL1|Vessel name", "SALES_WIN|Sales Window", "IB_WINDOW|Booking Window", "INSTRUCTION|Remark", "DEBIT_DATE|DEBIT DATE", "DEBIT_NO|DEBIT NO", "CUR|Currency", "AMT|total" };
                    ChangeCol(Ocol, ref smDt, false, delcol); break;
            }


            string strName = "StatementOfAccount.xlsx";
            string strPath = Server.MapPath("~/download");
            string strFile = string.Format(@"{0}\{1}", strPath, strName);
            Business.NPOIExcelHelp exhelp = new Business.NPOIExcelHelp();
            if (!exhelp.Connect_NOPI(strFile))
            {
                msg = @Resources.Locale.L_DNManage_Controllers_292;
            }
            NPOI.SS.UserModel.IWorkbook book = exhelp.DataTableToExcel(smDt, true, 3, 1);
            exhelp.MergedRegion(new int[] { 0, 1 }, new int[] { 0, smDt.Columns.Count - 1 }, book.GetSheetAt(0));
            string FilePath = strFile.Replace(strName, "backup\\" + excelType + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\");
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            strName = DateTime.Now.ToString("yyyyMMddHHmmss") + strName;
            strFile = FilePath + strName;
            using (FileStream file = new FileStream(strFile, FileMode.Create))
            {
                book.Write(file);
                file.Close();
                result = true;
            }
            return Json(new { IsOk = result ? "Y" : "N", file = strFile.Replace(strPath, ""), msg = msg });
        }
        public void DownLoadXls()
        {
            string strResult = string.Empty;
            string strPath = Server.MapPath("~/download");//D:\U_Disk\V3Tracking\WebGui\Config\excel\AIRBSMapping.xml
            string trantype = Prolink.Math.GetValueAsString(Request.Params["type"]);
            string filename = Prolink.Math.GetValueAsString(Request.Params["filename"]);
            string strName = "Excel.xls";
            if (!string.IsNullOrEmpty(filename))
                strName = filename;
            string strFile = string.Format(@"{0}\{1}", strPath, strName);

            string file = string.Empty;
            switch (trantype)
            {
                case "F": file = "F-Statement Of Account.xlsx"; break;
                case "L": file = "L-Statement Of Account.xlsx"; break;
                case "E": file = "E-Statement Of Account.xlsx"; break;
                case "R": file = "R-Statement Of Account.xlsx"; break;
                case "A": file = "A-Statement Of Account.xlsx"; break;
            }
            using (FileStream fs = new FileStream(strFile, FileMode.Open))
            {
                byte[] bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(file, System.Text.Encoding.UTF8));
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
            }
        }
        #region
        /// <summary>
        /// 表名替换
        /// </summary>
        /// <param name="headstr">源列名</param>
        /// <param name="headdb">替换列名</param>
        /// <param name="dt"></param>
        public void ChangeCol(string[] headstr, string[] headdb, ref DataTable dt, bool del = false, string[] delcol = null)
        {
            List<string> rem = new List<string>();
            foreach (DataColumn n in dt.Columns)
            {
                string colname = n.ColumnName.Trim();
                if (!string.IsNullOrEmpty(colname) && headstr.Contains(colname))
                {
                    int Pointer = Array.IndexOf(headstr, colname);
                    n.ColumnName = headdb[Pointer];
                }
                else
                {
                    rem.Add(colname);
                }
            }
            if (rem.Count > 0)
            {
                if (del)
                {
                    foreach (string name in rem)
                    {
                        dt.Columns.Remove(name);
                    }
                }
            }
            if (delcol != null && delcol.Count() > 0)
            {
                foreach (string name in delcol)
                {
                    dt.Columns.Remove(name);
                }
            }
        }

        public void ChangeCol(string[] headstr, ref DataTable dt, bool del = false, string[] delcol = null)
        {
            List<string> rem = new List<string>();
            foreach (DataColumn n in dt.Columns)
            {
                string colname = n.ColumnName.Trim();
                if (!string.IsNullOrEmpty(colname))
                {
                    foreach (string m in headstr)
                    {
                        if (!string.IsNullOrEmpty(m))
                        {
                            string[] cols = m.Split('|');
                            if (cols.Count() == 2)
                            {
                                if (colname.Equals(cols[0]))
                                    n.ColumnName = cols[1];
                            }
                        }
                    }
                }
                else
                {
                    rem.Add(colname);
                }
            }
            if (rem.Count > 0)
            {
                if (del)
                {
                    foreach (string name in rem)
                    {
                        dt.Columns.Remove(name);
                    }
                }
            }
            if (delcol != null && delcol.Count() > 0)
            {
                foreach (string name in delcol)
                {
                    dt.Columns.Remove(name);
                }
            }
        }
        #endregion

        public ActionResult UpdateLstInvDescpInfo()
        {
            string uids = Request.Params["Uids"];
            string[] uidlist = uids.Split(',');
            string lstinvdesp = Request.Params["LstInvDescp"];
            string sql = "";
            MixedList mlist = new MixedList();
            foreach (string index in uidlist)
            {
                if (string.IsNullOrEmpty(index))
                    continue;
                sql = string.Format("UPDATE SMBIM SET LST_INV_DESCP={0}  WHERE U_ID={1}", SQLUtils.QuotedStr(lstinvdesp), SQLUtils.QuotedStr(index));
                mlist.Add(sql);
            }

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message });
                }
            }
            return Json(new { message = "success" });
        }

        public JsonResult GetBscodeArcd()
        {
            string cdType = Request.Params["cdType"];
            string cd = Request.Params["cd"];
            string cmp = Request.Params["cmp"];
            if (string.IsNullOrEmpty(cmp))
                cmp = BaseCompanyId;
            string sql = string.Format(@"SELECT AR_CD FROM BSCODE WHERE CD_TYPE={0} AND CD={1} AND GROUP_ID={2} AND (CMP={3} OR CMP='*')",
                SQLUtils.QuotedStr(cdType), SQLUtils.QuotedStr(cd), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(cmp));
            string arcd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = arcd });
        }

        public ActionResult FSSPSaveData()
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
            string DebitNo = Request["DebitNo"];
            string TpvDebitNo = Request["TpvDebitNo"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);

            if (!string.IsNullOrEmpty(DebitNo))
                DebitNo = HttpUtility.UrlDecode(DebitNo);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            string upDebitNo = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbimModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", Business.TimeZoneHelper.GetTimeZoneDate(CompanyId));
                            ei.Put("APPROVE_TYPE", "STD_BILL");
                            ei.Put("APPROVE_TO", "A");
                            ei.Put("GROUP_ID", "TPV");
                            string sm_id = Prolink.Math.GetValueAsString(ei.Get("SHIPMENT_ID"));
                            string cmp = string.Empty, gid = string.Empty;
                            if (sm_id != "")
                            {
                                sql = "SELECT CMP, GROUP_ID FROM SMBIM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
                                DataTable sDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (sDt.Rows.Count > 0)
                                {
                                    cmp = Prolink.Math.GetValueAsString(sDt.Rows[0]["CMP"]);
                                    gid = Prolink.Math.GetValueAsString(sDt.Rows[0]["GROUP_ID"]);

                                    ei.Put("GROUP_ID", gid);
                                    ei.Put("CMP", cmp);
                                }
                            }

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", Business.TimeZoneHelper.GetTimeZoneDate(CompanyId));
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            u_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (u_id != "")
                            {
                                sql = "UPDATE SMBID SET U_FID=NULL,DEBIT_NO=NULL,FSTATUS='C',VOID_USER={1} WHERE U_FID={0}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(UserId));
                                mixList.Add(sql);

                                sql = "UPDATE SMBID_DN SET U_FID=NULL,DEBIT_NO=NULL,FSTATUS='C',VOID_USER={1} WHERE U_FID={0}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(UserId));
                                mixList.Add(sql);
                            }
                        }
                        else if (!string.IsNullOrEmpty(u_id) && !string.IsNullOrEmpty(DebitNo))
                        {
                            upDebitNo = string.Format("UPDATE SMBID SET DEBIT_NO={1} WHERE U_FID={0}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(DebitNo));
                            upDebitNo = string.Format("UPDATE SMBID_DN SET DEBIT_NO={1} WHERE U_FID={0}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(DebitNo));
                        }
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbidDnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string QtData = Prolink.Math.GetValueAsString(ei.Get("QT_DATA"));
                            string d_uid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (QtData == "Y")
                            {
                                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                                ei.PutKey("U_ID", d_uid);
                                ei.Put("FSTATUS", "D");
                            }
                            else
                            {
                                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            }

                            ei.Put("U_FID", u_id);
                            ei.Put("DEBIT_NO", DebitNo);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (ei.Get("U_ID") == "")
                            {
                                continue;
                            }
                            ei.OperationType = EditInstruct.UPDATE_OPERATION;
                            ei.AddKey("U_ID");
                            ei.Put("DEBIT_NO", "");
                            ei.Put("FSTATUS", "C");
                            ei.Put("U_FID", "");
                            ei.Put("U_FID", null);
                            ei.Put("VOID_USER", this.UserId);
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
                    #region 修正账单号码
                    try
                    {
                        if (!string.IsNullOrEmpty(upDebitNo))
                            OperationUtils.ExecuteUpdate(upDebitNo, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch { }
                    #endregion

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMBID_DN WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidDnModel");
            return ToContent(data);
        }

        public ActionResult TransferFSSPSetup(string id = null, string uid = null)
        {
            SetSchema("ActCheckSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.selects = TKBLController.GetSelectsToString("ActSetup", Request["location"], this.GroupId, this.BaseCompanyId, Request["dn"], Request["code"]);
            string sql = string.Format("SELECT DISTINCT TV_MNT+';' FROM APPROVE_FLOW_M WHERE CMP_ID={0} FOR XML PATH('') ", SQLUtils.QuotedStr(CompanyId));
            string tvMnttypes = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.tvMntSelect = tvMnttypes.Trim(';');
            ViewBag.pmsList = "Send2FSSP|" + GetBtnPms("AC030");
            return View();
        }

        public ActionResult GetFSSPSetupDataItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMBID_DN WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbimModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidDnModel");
            return ToContent(data);
        }

        public ActionResult GetFSSPActDetail()
        {
            return GetBootstrapData("SMBID_DN", "1=1");
        }

        public ActionResult Send2FSSP()
        {
            string uid = Prolink.Math.GetValueAsString(Request["UId"]);
            string cmp = Prolink.Math.GetValueAsString(Request["Cmp"]);
            string tpvDebitNo = Prolink.Math.GetValueAsString(Request["TpvDebitNo"]);
            string recalculate = Prolink.Math.GetValueAsString(Request["recalculate"]);
            ResultInfo resultinfo = new ResultInfo();
            resultinfo.IsSucceed = TrackingEDI.Manager.CheckFSSPSite(cmp);
            if (!resultinfo.IsSucceed)
                resultinfo.Description = "该站点未启用，请联系系统管理员";
            if (resultinfo.IsSucceed) {
                if (recalculate == "true")
                {
                    SplitSmbidHelper splitSmbidHelper = new SplitSmbidHelper(uid);
                    splitSmbidHelper.DoSplitSMBID();
                }
                resultinfo = WebApiEdiHandle.SendSplitSMBIDEdi(tpvDebitNo, UserId, cmp, uid);
            }
            return Json(new { message = resultinfo.Description, IsOk = resultinfo.IsSucceed });
        }

        private string GetTVMNTSelect(string cmp)
        {
            string sql = string.Format("SELECT DISTINCT TV_MNT+';' FROM APPROVE_FLOW_M WHERE CMP_ID={0} FOR XML PATH('') ", SQLUtils.QuotedStr(cmp));
            string tvMnttypes = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return tvMnttypes.Trim(';');
        }

        public ActionResult GetTVMNTSelectValue()
        {
            string cmp = Request.Params["Cmp"];
            return Json(new { message = GetTVMNTSelect(cmp) });
        }
    }
}
