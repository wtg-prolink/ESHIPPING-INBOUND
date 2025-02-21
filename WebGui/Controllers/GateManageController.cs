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
using System.Threading;
using System.Collections.Specialized;
using Business.Service;
namespace WebGui.Controllers
{
    public class GateManageController : BaseController
    {
        #region View
        public ActionResult InOutManage()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult GateReserve()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM010");
            return View();
        }
        public ActionResult GateReserveSetup(string id = null, string uid = null)
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            SetSchema("GateReserveSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("SM010");
            ViewBag.RelationId = ids;
            return View();
        }

        public ActionResult GateAnalysis()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM040");
            return View();
        }

        public ActionResult ConfirmReserve()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM060");
            return View();
        }

        public ActionResult ConfirmReserveSetup(string id = null, string uid = null)
        {
            SetSchema("ConfirmReserveSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("SM060");
            return View();
        }

        public ActionResult GateSetup(string id = null, string uid = null)
        {
            //string sql = "SELECT NAME FROM SYS_SITE WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND STN=" + SQLUtils.QuotedStr(Station);
            //string CmpNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            //ViewBag.Cmp = CompanyId;
            //ViewBag.CmpNm = CmpNm;
            SetSchema("GateSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("SM050");
            return View();
        }

        public ActionResult ContainerManage()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM070");
            return View();
        }

        public ActionResult SmrvSetup(string id = null, string uid = null)
        {
            SetSchema("ConfirmReserveSetup");
            if (uid == null)
            {
                uid = id;
            }
            if (string.IsNullOrEmpty(id))
                id = Request["id"];
            string flag = Request["flag"];

            ViewBag.Uid = id;
            ViewBag.Flag = flag;
            if (!string.IsNullOrEmpty(flag))//进厂出厂确认
                ViewBag.pmsList = GetBtnPms("SMRV01");
            else
                ViewBag.pmsList = GetBtnPms("SM070");
            return View();
        }

        public ActionResult GateStatus()
        {
            string sql = "SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            string CmpNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.MenuBar = false;
            ViewBag.Cmp = CompanyId;
            ViewBag.CmpNm = CmpNm;
            ViewBag.pmsList = GetBtnPms("SM030");
            return View();
        }

        public ActionResult ReserveQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM080");
            return View();
        }

        public ActionResult OrderCarQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("CAR110");
            string sql = "SELECT WS_CD, WS_NM FROM SMWH WHERE CMP=" + SQLUtils.QuotedStr(CompanyId) + " ORDER BY WS_CD ASC";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string WsSelect = "";
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                { 
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    string WsNm = Prolink.Math.GetValueAsString(item["WS_NM"]);
                    WsSelect += WsCd + ")*(" + WsNm + "(*)";
                }
            }
            ViewBag.WsSelect = WsSelect;
            ViewBag.CargoType = GetBscodeByMode("TCGT");
            return View();
        }

            /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*", string funcName="",NameValueCollection namevaluecollection=null)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            string statusField = Request.Params["statusField"];

            DataTable dt = null;
            if (resultType == "count")
            {
                
                string basecondtion=GetDecodeBase64ToString(Request.Params["basecondition"]);
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
                dt = GetStatusCountData(statusField, table, condition, Request.Params, funcName);
                pageSize = 1;
            }
            else
            {

                if (namevaluecollection == null) namevaluecollection = Request.Params;
                //if (namevaluecollection["conditions"].IndexOf("Status=R") == -1)
                //{
                //    condition += " AND STATUS != 'R' ";
                //}
                
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

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues, string funcName = "")
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);
            //king filter archived status
            condition += " AND (" + col + " !='R' AND " + col + " !='ARCH' )";

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";



            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dtAll = dt.Copy();

            sql = "  SELECT '' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
            if (!string.IsNullOrEmpty(funcName))
            {
                if (funcName == "GetSMDNPData")
                {
                    sql = "  SELECT '' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
                }

                DataTable dtsum = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in dtsum.Rows)
                {
                    dtAll.ImportRow(dr);
                }

            }

            return dtAll;
        }



        public ActionResult GateQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM050");
            return View();
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
                    case "GateReserveSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMRV WHERE 1=0";
                        break;
                    case "ConfirmReserveSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMRV WHERE 1=0";
                        break;
                    case "GateSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMWH WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }
        /*
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
        }*/

        private ActionResult GetBaseData(string table, string condition, string colNames = "*", string orderBy = "", string qType = "", int pageSize=20)
        {
            int recordsCount = 0, pageIndex = 1;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            switch (qType)
            { 
                case "1":
                    dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                case "2":
                    dt = ModelFactory.InquiryData("*", table, condition, orderBy,  pageIndex,  pageSize, ref  recordsCount);
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

        #region 純查詢
        public ActionResult GetSmRvByDnNo()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string condition = " DN_NO=" + SQLUtils.QuotedStr(DnNo);
            return GetBaseData("SMRV", condition, "*", "RESERVE_NO", "2");
        }
        public ActionResult GetSmRvByShipmentId()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            int pagesize=Prolink.Math.GetValueAsInt(Request.Params["Pagesize"]);
            if(pagesize<=0){
                pagesize=20;
            }
            string condition = " SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);

            return GetBaseData("SMRV", condition, "*", "RESERVE_NO", "2", pagesize);
        }
        #endregion

        #region Export/Upload Excel
        public JsonResult chkSmdnpCount()
        {
            string returnMsg = "success";
            string Dns = Prolink.Math.GetValueAsString(Request.Params["Dns"]);
            string sql = "SELECT * FROM V_CMEXCEL WHERE DOWN_FLAG IS NULL OR DOWN_FLAG='N'";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count == 0)
            {
                returnMsg = "Fail";
            }

            return Json(new { msg=returnMsg});
        }

        public ActionResult exportSmdnpToExcel()
        {
            //int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = " (DOWN_FLAG IS NULL OR DOWN_FLAG='N') AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            string resultType = Request.Params["resultType"];
            string Dns = Prolink.Math.GetValueAsString(Request.Params["Dns"]);
            string con = "";
            if (Dns != "")
            {
                con += " WHERE DN_NO IN (" + Dns + ")";
            }
            DataTable dt = null;
            /*
            string sql = @"SELECT 'U_ID' AS U_ID, 'SHIPMENT_INFO' AS SHIPMENT_INFO, 'DN_NO' AS DN_NO, 'IPART_NO' AS IPART_NO, 'JOB_NO' AS JOB_NO, 'PRODUCT_LINE' AS PRODUCT_LINE, 'OPART_NO' AS OPART_NO 
                                   UNION
                                    SELECT convert(nvarchar(50), U_ID) AS U_ID,  SHIPMENT_INFO, DN_NO, IPART_NO, JOB_NO, PRODUCT_LINE, OPART_NO 
                                   FROM  V_CMSUMMARY WHERE (DOWN_FLAG IS  NULL OR DOWN_FLAG = 'N') ORDER BY DN_NO DESC";
             */
            string sql = @"SELECT * FROM V_CMEXCEL WHERE " + condition + " ORDER BY IPART_NO DESC";
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                sql = "";
                foreach (DataRow item in dt.Rows)
                {
                    string UId = Prolink.Math.GetValueAsString(item["U_ID"]);

                    if (UId != "")
                    {
                        sql += string.Format("UPDATE SMDNP SET DOWN_FLAG='Y', DOWN_USER={0},DOWN_DATE=getdate() WHERE U_ID={1}",
                            SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(UId))+";";
                        //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
                Thread tr = new Thread(() => executeUpdateSql(sql));

                try
                {
                    tr.Start();
                }
                catch (Exception ex)
                {

                }
                
            }
            //dt = ModelFactory.InquiryData("*", "V_CMSUMMARY", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            /*
            DataRow toInsert = dt.NewRow();
            toInsert["U_ID"] = "U_ID";
            toInsert["SHIPMENT_INFO"] = "SHIPMENT_INFO";
            toInsert["DN_NO"] = "DN_NO";
            toInsert["IPART_NO"] = "IPART_NO";
            toInsert["JOB_NO"] = "JOB_NO";
            toInsert["JQTY"] = Convert.ToDecimal("JQTY");
            toInsert["PQTY"] = Convert.ToDecimal("PQTY");
            toInsert["CNTR_STD_QTY"] = "CNTR_STD_QTY";
            toInsert["CNT_NUMBER"] = "CNT_NUMBER";
            toInsert["PRODUCT_LINE"] = "PRODUCT_LINE";
            toInsert["OPART_NO"] = "OPART_NO";
            dt.Rows.InsertAt(toInsert, 0);
             * */

                if (resultType == "excel")
                    return ExportExcelFile(dt);

            return ExportExcelFile(dt);
        }

        public void executeUpdateSql(string sql)
        {
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public ActionResult exportOrderCarToExcel()
        {
            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            string orderBy = Prolink.Math.GetValueAsString(Request.Params["orderBy"]);
            string sql = @"SELECT * FROM V_IPART" + orderBy;
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (resultType == "excel")
                return ExportExcelFile(dt);

            return ExportExcelFile(dt);
        }
        [HttpPost]
        public ActionResult UploadJobNo(FormCollection form)
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
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

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, 2);
                
                EditInstruct ei;
                int[] resulst;

                try 
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ei = new EditInstruct("SMDNP", EditInstruct.UPDATE_OPERATION);
                        //string UId = Prolink.Math.GetValueAsString(dr["U_ID"]);
                        string DnNo = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                        string IpartNo = Prolink.Math.GetValueAsString(dr["IPART_NO"]);
                        string JobNo = Prolink.Math.GetValueAsString(dr["JOB_NO"]);
                        string ProductLine = Prolink.Math.GetValueAsString(dr["PRODUCT_LINE"]);
                        ei.PutKey("DN_NO", DnNo);
                        ei.PutKey("IPART_NO", IpartNo);
                        ei.Put("JOB_NO", JobNo);
                        ei.Put("PRODUCT_LINE", ProductLine);
                        ei.Put("DOWN_FLAG", "Y");
                        if (DnNo != "" && JobNo != "" && IpartNo !="")
                        {
                            ml.Add(ei);
                        }

                        if (i + 1 % 100 == 0)
                         {
                             resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                             ml = new MixedList();
                         }
                    }
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch(Exception ex)
                {
                    returnMessage = ex.Message;
                    return Json(new { message = returnMessage });
                }
            }

            return Json(new { message = returnMessage });
        }

        [HttpPost]
        public ActionResult updateSi(FormCollection form)
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
            List<Dictionary<string, object>> smdnpData = new List<Dictionary<string, object>>();
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);
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

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);

                EditInstruct ei;
                int[] resulst;

                try
                { 
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ei = new EditInstruct("SMSIM", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("PROFILE", Prolink.Math.GetValueAsString(dr["PROFILE"]));
                        ei.Put("BL_TYPE", Prolink.Math.GetValueAsString(dr["BL_TYPE"]));
                        ei.Put("BL_REMARK1", Prolink.Math.GetValueAsString(dr["BL_REMARK1"]));
                        ei.Put("BL_REMARK2", Prolink.Math.GetValueAsString(dr["BL_REMARK2"]));
                        ei.Put("BL_REMARK3", Prolink.Math.GetValueAsString(dr["BL_REMARK3"]));
                        ei.Put("BL_REMARK4", Prolink.Math.GetValueAsString(dr["BL_REMARK4"]));
                        ei.Put("BL_REMARK5", Prolink.Math.GetValueAsString(dr["BL_REMARK5"]));
                        ei.Put("BL_REMARK6", Prolink.Math.GetValueAsString(dr["BL_REMARK6"]));
                        ei.Put("BL_SPC_REQ", Prolink.Math.GetValueAsString(dr["BL_SPC_REQ"]));
                        ei.Put("COMMODITY", Prolink.Math.GetValueAsString(dr["COMMODITY"]));
                        ei.Put("PKG_DESCP", Prolink.Math.GetValueAsString(dr["PKG_DESCP"]));
                        ei.Put("SHPR_NM", Prolink.Math.GetValueAsString(dr["SHPR_NM"]));
                        ml.Add(ei);

                        if (i + 1 % 100 == 0)
                        {
                            resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            ml = new MixedList();
                        }
                    }
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch(Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = returnMessage });
                }
            }

            return Json(new { message = returnMessage });
        }

        [HttpPost]
        public ActionResult UploadSI(FormCollection form)
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
            List<Dictionary<string, object>> smdnpData = new List<Dictionary<string, object>>();
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);
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

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);

                EditInstruct ei;
                int[] resulst;

                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        string profileId = Prolink.Math.GetValueAsString(dr["PROFILE"]);
                        string sql = "SELECT COUNT(*) FROM SMSIM WHERE PROFILE=" + SQLUtils.QuotedStr(profileId);
                        int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (num == 0)
                        {
                            ei = new EditInstruct("SMSIM", EditInstruct.INSERT_OPERATION);

                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("PROFILE", Prolink.Math.GetValueAsString(dr["PROFILE"]));
                            ei.Put("CMP", CompanyId);
                            ei.Put("PROFILE_NM", Prolink.Math.GetValueAsString(dr["PROFILE_NM"]));
                            ei.Put("INV_FLOW", Prolink.Math.GetValueAsString(dr["INV_FLOW"]));
                            ei.Put("MODEL_NAME", Prolink.Math.GetValueAsString(dr["MODEL_NAME"]));
                            string CustCd = Prolink.Math.GetValueAsString(dr["CUST_CD"]);
                            if (CustCd != "/" && CustCd != "")
                                CustCd = CustCd.PadLeft(10,'0');
                            ei.Put("CUST_CD", CustCd);
                            ei.Put("CUST_NM", Prolink.Math.GetValueAsString(dr["CUST_NM"]));

                            string Seller = Prolink.Math.GetValueAsString(dr["SELLER"]);
                            if (Seller != "/" && Seller != "")
                                Seller = Seller.PadLeft(10, '0');
                            ei.Put("SELLER", Seller);
                            ei.Put("SELLER_NM", Prolink.Math.GetValueAsString(dr["SELLER_NM"]));

                            string Buyer1 = Prolink.Math.GetValueAsString(dr["BUYER1"]);
                            if (Buyer1 != "/" && Buyer1 != "")
                                Buyer1 = Buyer1.PadLeft(10, '0');
                            ei.Put("BUYER1", Buyer1);
                            ei.Put("BUYER1_NM", Prolink.Math.GetValueAsString(dr["BUYER1_NM"]));
                            ei.Put("INCOTERM1", Prolink.Math.GetValueAsString(dr["INCOTERM1"]));
                            ei.Put("BUYER2_NM", Prolink.Math.GetValueAsString(dr["BUYER2_NM"]));

                            string Buyer2 = Prolink.Math.GetValueAsString(dr["BUYER2"]);
                            if (Buyer2 != "/" && Buyer2 != "")
                                Buyer2 = Buyer2.PadLeft(10, '0');
                            ei.Put("BUYER2", Buyer2);
                            ei.Put("INCOTERM2", Prolink.Math.GetValueAsString(dr["INCOTERM2"]));

                            string CneeCd = Prolink.Math.GetValueAsString(dr["CNEE_CD"]);
                            if (CneeCd != "/" && CneeCd != "")
                                CneeCd = CneeCd.PadLeft(10, '0');
                            ei.Put("CNEE_CD", CneeCd);
                            ei.Put("CNEE_NM", Prolink.Math.GetValueAsString(dr["CNEE_NM"]));
                            ei.Put("FCL_LCL", Prolink.Math.GetValueAsString(dr["FCL_LCL"]));

                            ei.Put("COMMODITY", Prolink.Math.GetValueAsString(dr["COMMODITY"]));
                            ei.Put("BL_TYPE", Prolink.Math.GetValueAsString(dr["BL_TYPE"]));
                            ei.Put("BL_REMARK1", Prolink.Math.GetValueAsString(dr["BL_REMARK1"]));
                            ei.Put("BL_REMARK2", Prolink.Math.GetValueAsString(dr["BL_REMARK2"]));
                            ei.Put("BL_REMARK3", Prolink.Math.GetValueAsString(dr["BL_REMARK3"]));
                            ei.Put("BL_REMARK4", Prolink.Math.GetValueAsString(dr["BL_REMARK4"]));
                            ei.Put("BL_REMARK5", Prolink.Math.GetValueAsString(dr["BL_REMARK5"]));
                            ei.Put("BL_REMARK6", Prolink.Math.GetValueAsString(dr["BL_REMARK6"]));
                            ei.Put("INV_SPEC", Prolink.Math.GetValueAsString(dr["INV_SPEC"]));
                            ei.Put("BL_SPC_REQ", Prolink.Math.GetValueAsString(dr["BL_SPC_REQ"]));
                            string com = Prolink.Math.GetValueAsString(dr["COMBINE"]);

                            if (com == "YES")
                            {
                                com = "Y";
                            }
                            else if (com == "NO")
                            {
                                com = "N";
                            }
                            else
                            {
                                com = "N";
                            }
                            ei.Put("COMBINE", com);
                            ei.Put("INV_REMARK1", Prolink.Math.GetValueAsString(dr["INV_REMARK1"]));
                            ei.Put("INV_REMARK2", Prolink.Math.GetValueAsString(dr["INV_REMARK2"]));
                            ei.Put("INV_REMARK3", Prolink.Math.GetValueAsString(dr["INV_REMARK3"]));
                            ei.Put("INV_REMARK4", Prolink.Math.GetValueAsString(dr["INV_REMARK4"]));
                            ei.Put("INV_REMARK5", Prolink.Math.GetValueAsString(dr["INV_REMARK5"]));
                            ei.Put("INV_SPEC", Prolink.Math.GetValueAsString(dr["INV_SPEC"]));
                            ei.Put("PK_REMARK1", Prolink.Math.GetValueAsString(dr["PK_REMARK1"]));
                            ei.Put("PK_REMARK2", Prolink.Math.GetValueAsString(dr["PK_REMARK2"]));
                            ei.Put("PK_REMARK3", Prolink.Math.GetValueAsString(dr["PK_REMARK3"]));
                            ei.Put("PK_REMARK4", Prolink.Math.GetValueAsString(dr["PK_REMARK4"]));
                            ei.Put("PK_REMARK5", Prolink.Math.GetValueAsString(dr["PK_REMARK5"]));
                            ei.Put("PK_SPEC", Prolink.Math.GetValueAsString(dr["PK_SPEC"]));
                            string ShprCd = Prolink.Math.GetValueAsString(dr["SHPR_CD"]);
                            if (ShprCd != "/" && ShprCd != "")
                                ShprCd = ShprCd.PadLeft(10, '0');
                            ei.Put("SHPR_CD", ShprCd);
                            ei.Put("SHPR_NM", Prolink.Math.GetValueAsString(dr["SHPR_NM"]));
                            ei.Put("SHIPPING_MODE", Prolink.Math.GetValueAsString(dr["SHIPPING_MODE"]));

                            string Carrier1 = Prolink.Math.GetValueAsString(dr["CARRIER1"]);
                            if (Carrier1 != "/" && Carrier1 != "")
                                Carrier1 = Carrier1.PadLeft(10, '0');
                            ei.Put("CARRIER1", Carrier1);
                            //ei.Put("CARRIER_NM1", Prolink.Math.GetValueAsString(dr["CARRIER_NM1"]));
                            ei.Put("CONTACT_NO1", Prolink.Math.GetValueAsString(dr["CONTACT_NO1"]));

                            string LspNo1 = Prolink.Math.GetValueAsString(dr["LSP_NO1"]);
                            if (LspNo1 != "/" && LspNo1 != "")
                                LspNo1 = LspNo1.PadLeft(10, '0');
                            ei.Put("LSP_NO1", LspNo1);
                            ei.Put("LSP_INFO1", Prolink.Math.GetValueAsString(dr["LSP_INFO1"]));

                            string Carrier2 = Prolink.Math.GetValueAsString(dr["CARRIER2"]);
                            if (Carrier2 != "/" && Carrier2 != "")
                                Carrier2 = Carrier2.PadLeft(10, '0');
                            ei.Put("CARRIER2", Prolink.Math.GetValueAsString(dr["CARRIER2"]));
                            ei.Put("CONTACT_NO2", Prolink.Math.GetValueAsString(dr["CONTACT_NO2"]));

                            string LspNo2 = Prolink.Math.GetValueAsString(dr["LSP_NO2"]);
                            if (LspNo2 != "/" && LspNo2 != "")
                                LspNo2 = LspNo2.PadLeft(10, '0');
                            ei.Put("LSP_NO2", LspNo2);
                            ei.Put("LSP_INFO2", Prolink.Math.GetValueAsString(dr["LSP_INFO2"]));

                            string Carrier3 = Prolink.Math.GetValueAsString(dr["CARRIER3"]);
                            if (Carrier3 != "/" && Carrier3 != "")
                                Carrier3 = Carrier3.PadLeft(10, '0');
                            ei.Put("CARRIER3", Prolink.Math.GetValueAsString(dr["CARRIER3"]));
                            ei.Put("CONTACT_NO3", Prolink.Math.GetValueAsString(dr["CONTACT_NO3"]));

                            string LspNo3 = Prolink.Math.GetValueAsString(dr["LSP_NO3"]);
                            if (LspNo3 != "/" && LspNo3 != "")
                                LspNo3 = LspNo3.PadLeft(10, '0');
                            ei.Put("LSP_NO3", Prolink.Math.GetValueAsString(dr["LSP_NO3"]));
                            ei.Put("LSP_INFO3", Prolink.Math.GetValueAsString(dr["LSP_INFO3"]));

                            ei.Put("DOC1", Prolink.Math.GetValueAsString(dr["DOC1"]));
                            ei.Put("MAIL_GROUP1", Prolink.Math.GetValueAsString(dr["MAIL_GROUP1"]));
                            ei.Put("DOC2", Prolink.Math.GetValueAsString(dr["DOC2"]));
                            ei.Put("MAIL_GROUP2", Prolink.Math.GetValueAsString(dr["MAIL_GROUP2"]));
                            ei.Put("DOC3", Prolink.Math.GetValueAsString(dr["DOC3"]));
                            ei.Put("MAIL_GROUP3", Prolink.Math.GetValueAsString(dr["MAIL_GROUP3"]));
                            ei.Put("DOC4", Prolink.Math.GetValueAsString(dr["DOC4"]));
                            ei.Put("MAIL_GROUP4", Prolink.Math.GetValueAsString(dr["MAIL_GROUP4"]));
                            ei.Put("DOC5", Prolink.Math.GetValueAsString(dr["DOC5"]));
                            ei.Put("MAIL_GROUP5", Prolink.Math.GetValueAsString(dr["MAIL_GROUP5"]));
                            string PoHeader = Prolink.Math.GetValueAsString(dr["PO_HEADER"]);
                            string ModelHeader = Prolink.Math.GetValueAsString(dr["MODEL_HEADER"]);
                            string PartHeader = Prolink.Math.GetValueAsString(dr["PART_HEADER"]);
                            if (PoHeader == "√")
                            {
                                PoHeader = "Customer PO#";
                            }
                            else
                            {
                                PoHeader = "";
                            }
                            if (ModelHeader == "√")
                            {
                                ModelHeader = "Model#";
                            }
                            else
                            {
                                ModelHeader = "";
                            }
                            if (PartHeader == "√")
                            {
                                PartHeader = "P/N#";
                            }
                            else
                            {
                                PartHeader = "";
                            }
                            ei.Put("PO_HEADER", Prolink.Math.GetValueAsString(PoHeader));
                            ei.Put("MODEL_HEADER", Prolink.Math.GetValueAsString(ModelHeader));
                            ei.Put("PART_HEADER", Prolink.Math.GetValueAsString(PartHeader));

                            StringBuilder msg = new StringBuilder(); XmlParser.ParseDbEditInstruct(ei, msg);
                            if (msg.Length > 0)
                            {
                                string a = "";
                            }
                            ml.Add(ei);
                        }
                        else
                        { 
                            ei = new EditInstruct("SMSIM", EditInstruct.UPDATE_OPERATION);

                            ei.PutKey("PROFILE", Prolink.Math.GetValueAsString(dr["PROFILE"]));
                            ei.Put("CMP", CompanyId);
                            ei.Put("PROFILE_NM", Prolink.Math.GetValueAsString(dr["PROFILE_NM"]));
                            ei.Put("INV_FLOW", Prolink.Math.GetValueAsString(dr["INV_FLOW"]));
                            ei.Put("MODEL_NAME", Prolink.Math.GetValueAsString(dr["MODEL_NAME"]));
                            string CustCd = Prolink.Math.GetValueAsString(dr["CUST_CD"]);
                            if (CustCd != "/" && CustCd != "")
                                CustCd = CustCd.PadLeft(10, '0');
                            ei.Put("CUST_CD", CustCd);
                            ei.Put("CUST_NM", Prolink.Math.GetValueAsString(dr["CUST_NM"]));

                            string Seller = Prolink.Math.GetValueAsString(dr["SELLER"]);
                            if (Seller != "/" && Seller != "")
                                Seller = Seller.PadLeft(10, '0');
                            ei.Put("SELLER", Seller);
                            ei.Put("SELLER_NM", Prolink.Math.GetValueAsString(dr["SELLER_NM"]));

                            string Buyer1 = Prolink.Math.GetValueAsString(dr["BUYER1"]);
                            if (Buyer1 != "/" && Buyer1 != "")
                                Buyer1 = Buyer1.PadLeft(10, '0');
                            ei.Put("BUYER1", Buyer1);
                            ei.Put("BUYER1_NM", Prolink.Math.GetValueAsString(dr["BUYER1_NM"]));
                            ei.Put("INCOTERM1", Prolink.Math.GetValueAsString(dr["INCOTERM1"]));
                            ei.Put("BUYER2_NM", Prolink.Math.GetValueAsString(dr["BUYER2_NM"]));

                            string Buyer2 = Prolink.Math.GetValueAsString(dr["BUYER2"]);
                            if (Buyer2 != "/" && Buyer2 != "")
                                Buyer2 = Buyer2.PadLeft(10, '0');
                            ei.Put("BUYER2", Buyer2);
                            ei.Put("INCOTERM2", Prolink.Math.GetValueAsString(dr["INCOTERM2"]));

                            string CneeCd = Prolink.Math.GetValueAsString(dr["CNEE_CD"]);
                            if (CneeCd != "/" && CneeCd != "")
                                CneeCd = CneeCd.PadLeft(10, '0');
                            ei.Put("CNEE_CD", CneeCd);
                            ei.Put("CNEE_NM", Prolink.Math.GetValueAsString(dr["CNEE_NM"]));
                            ei.Put("FCL_LCL", Prolink.Math.GetValueAsString(dr["FCL_LCL"]));

                            ei.Put("COMMODITY", Prolink.Math.GetValueAsString(dr["COMMODITY"]));
                            ei.Put("BL_TYPE", Prolink.Math.GetValueAsString(dr["BL_TYPE"]));
                            ei.Put("BL_REMARK1", Prolink.Math.GetValueAsString(dr["BL_REMARK1"]));
                            ei.Put("BL_REMARK2", Prolink.Math.GetValueAsString(dr["BL_REMARK2"]));
                            ei.Put("BL_REMARK3", Prolink.Math.GetValueAsString(dr["BL_REMARK3"]));
                            ei.Put("BL_REMARK4", Prolink.Math.GetValueAsString(dr["BL_REMARK4"]));
                            ei.Put("BL_REMARK5", Prolink.Math.GetValueAsString(dr["BL_REMARK5"]));
                            ei.Put("BL_REMARK6", Prolink.Math.GetValueAsString(dr["BL_REMARK6"]));
                            ei.Put("INV_SPEC", Prolink.Math.GetValueAsString(dr["INV_SPEC"]));
                            ei.Put("BL_SPC_REQ", Prolink.Math.GetValueAsString(dr["BL_SPC_REQ"]));
                            string com = Prolink.Math.GetValueAsString(dr["COMBINE"]);

                            string ShprCd = Prolink.Math.GetValueAsString(dr["SHPR_CD"]);
                            if (ShprCd != "/" && ShprCd != "")
                                ShprCd = ShprCd.PadLeft(10, '0');
                            ei.Put("SHPR_CD", ShprCd);
                            ei.Put("SHPR_NM", Prolink.Math.GetValueAsString(dr["SHPR_NM"]));
                            ei.Put("SHIPPING_MODE", Prolink.Math.GetValueAsString(dr["SHIPPING_MODE"]));

                            if (com == "YES")
                            {
                                com = "Y";
                            }
                            else if (com == "NO")
                            {
                                com = "N";
                            }
                            else
                            {
                                com = "N";
                            }
                            ei.Put("COMBINE", com);
                            ei.Put("INV_REMARK1", Prolink.Math.GetValueAsString(dr["INV_REMARK1"]));
                            ei.Put("INV_REMARK2", Prolink.Math.GetValueAsString(dr["INV_REMARK2"]));
                            ei.Put("INV_REMARK3", Prolink.Math.GetValueAsString(dr["INV_REMARK3"]));
                            ei.Put("INV_REMARK4", Prolink.Math.GetValueAsString(dr["INV_REMARK4"]));
                            ei.Put("INV_REMARK5", Prolink.Math.GetValueAsString(dr["INV_REMARK5"]));
                            ei.Put("INV_SPEC", Prolink.Math.GetValueAsString(dr["INV_SPEC"]));
                            ei.Put("PK_REMARK1", Prolink.Math.GetValueAsString(dr["PK_REMARK1"]));
                            ei.Put("PK_REMARK2", Prolink.Math.GetValueAsString(dr["PK_REMARK2"]));
                            ei.Put("PK_REMARK3", Prolink.Math.GetValueAsString(dr["PK_REMARK3"]));
                            ei.Put("PK_REMARK4", Prolink.Math.GetValueAsString(dr["PK_REMARK4"]));
                            ei.Put("PK_REMARK5", Prolink.Math.GetValueAsString(dr["PK_REMARK5"]));
                            ei.Put("PK_SPEC", Prolink.Math.GetValueAsString(dr["PK_SPEC"]));
                            ei.Put("SHPR_NM", Prolink.Math.GetValueAsString(dr["SHPR_NM"]));

                            string Carrier1 = Prolink.Math.GetValueAsString(dr["CARRIER1"]);
                            if (Carrier1 != "/" && Carrier1 != "")
                                Carrier1 = Carrier1.PadLeft(10, '0');
                            ei.Put("CARRIER1", Carrier1);
                            //ei.Put("CARRIER_NM1", Prolink.Math.GetValueAsString(dr["CARRIER_NM1"]));
                            ei.Put("CONTACT_NO1", Prolink.Math.GetValueAsString(dr["CONTACT_NO1"]));

                            string LspNo1 = Prolink.Math.GetValueAsString(dr["LSP_NO1"]);
                            if (LspNo1 != "/" && LspNo1 != "")
                                LspNo1 = LspNo1.PadLeft(10, '0');
                            ei.Put("LSP_NO1", LspNo1);
                            ei.Put("LSP_INFO1", Prolink.Math.GetValueAsString(dr["LSP_INFO1"]));

                            string Carrier2 = Prolink.Math.GetValueAsString(dr["CARRIER2"]);
                            if (Carrier2 != "/" && Carrier2 != "")
                                Carrier2 = Carrier2.PadLeft(10, '0');
                            ei.Put("CARRIER2", Prolink.Math.GetValueAsString(dr["CARRIER2"]));
                            ei.Put("CONTACT_NO2", Prolink.Math.GetValueAsString(dr["CONTACT_NO2"]));

                            string LspNo2 = Prolink.Math.GetValueAsString(dr["LSP_NO2"]);
                            if (LspNo2 != "/" && LspNo2 != "")
                                LspNo2 = LspNo2.PadLeft(10, '0');
                            ei.Put("LSP_NO2", LspNo2);
                            ei.Put("LSP_INFO2", Prolink.Math.GetValueAsString(dr["LSP_INFO2"]));

                            string Carrier3 = Prolink.Math.GetValueAsString(dr["CARRIER3"]);
                            if (Carrier3 != "/" && Carrier3 != "")
                                Carrier3 = Carrier3.PadLeft(10, '0');
                            ei.Put("CARRIER3", Prolink.Math.GetValueAsString(dr["CARRIER3"]));
                            ei.Put("CONTACT_NO3", Prolink.Math.GetValueAsString(dr["CONTACT_NO3"]));

                            string LspNo3 = Prolink.Math.GetValueAsString(dr["LSP_NO3"]);
                            if (LspNo3 != "/" && LspNo3 != "")
                                LspNo3 = LspNo3.PadLeft(10, '0');
                            ei.Put("LSP_NO3", Prolink.Math.GetValueAsString(dr["LSP_NO3"]));
                            ei.Put("LSP_INFO3", Prolink.Math.GetValueAsString(dr["LSP_INFO3"]));

                            ei.Put("DOC1", Prolink.Math.GetValueAsString(dr["DOC1"]));
                            ei.Put("MAIL_GROUP1", Prolink.Math.GetValueAsString(dr["MAIL_GROUP1"]));
                            ei.Put("DOC2", Prolink.Math.GetValueAsString(dr["DOC2"]));
                            ei.Put("MAIL_GROUP2", Prolink.Math.GetValueAsString(dr["MAIL_GROUP2"]));
                            ei.Put("DOC3", Prolink.Math.GetValueAsString(dr["DOC3"]));
                            ei.Put("MAIL_GROUP3", Prolink.Math.GetValueAsString(dr["MAIL_GROUP3"]));
                            ei.Put("DOC4", Prolink.Math.GetValueAsString(dr["DOC4"]));
                            ei.Put("MAIL_GROUP4", Prolink.Math.GetValueAsString(dr["MAIL_GROUP4"]));
                            ei.Put("DOC5", Prolink.Math.GetValueAsString(dr["DOC5"]));
                            ei.Put("MAIL_GROUP5", Prolink.Math.GetValueAsString(dr["MAIL_GROUP5"]));
                            string PoHeader = Prolink.Math.GetValueAsString(dr["PO_HEADER"]);
                            string ModelHeader = Prolink.Math.GetValueAsString(dr["MODEL_HEADER"]);
                            string PartHeader = Prolink.Math.GetValueAsString(dr["PART_HEADER"]);
                            if (PoHeader == "√")
                            {
                                PoHeader = "Customer PO#";
                            }
                            else
                            {
                                PoHeader = "";
                            }
                            if (ModelHeader == "√")
                            {
                                ModelHeader = "Model#";
                            }
                            else
                            {
                                ModelHeader = "";
                            }
                            if (PartHeader == "√")
                            {
                                PartHeader = "P/N#";
                            }
                            else
                            {
                                PartHeader = "";
                            }
                            ei.Put("PO_HEADER", Prolink.Math.GetValueAsString(PoHeader));
                            ei.Put("MODEL_HEADER", Prolink.Math.GetValueAsString(ModelHeader));
                            ei.Put("PART_HEADER", Prolink.Math.GetValueAsString(PartHeader));

                            StringBuilder msg = new StringBuilder(); XmlParser.ParseDbEditInstruct(ei, msg);
                            if (msg.Length > 0)
                            {
                                string a = "";
                            }
                            ml.Add(ei);
                        }

                        if (i + 1 % 100 == 0)
                        {
                            resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            ml = new MixedList();
                        }
                    }
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                    return Json(new { message = returnMessage });
                }
            }

            return Json(new { message = returnMessage });
        }

        [HttpPost]
        public ActionResult UploadPacking(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList mx = new MixedList();
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string InvoiceType = Prolink.Math.GetValueAsString(Request.Params["InvoiceType"]);
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string InvNo = Prolink.Math.GetValueAsString(Request.Params["InvNo"]);
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

            if (UId == "")
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_298;
                return Json(new { message = returnMessage });
            }

            if (StartRow == 0)
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_299;
                return Json(new { message = returnMessage });
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

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);
                
                EditInstruct ei;
                EditInstruct ei1;
                EditInstruct ei2;
                int[] resulst;
                /*
                 * dr[0]:CNTR_NO
                 * dr[1]: 資料庫無對應欄位
                 * dr[2]: CASE_NO
                 * dr[3]: PO_NO
                 * dr[4]: PART_NO
                 * dr[5]: OPART_NO
                 * dr[6]: IPART_NO
                 * dr[7]: GOODS_DESCP
                 * dr[8]: QTY
                 * dr[9]: NW
                 * dr[10]: GW
                 * dr[11]: CBM
                 * dr[12]: UNIT_PRICE
                 * dr[13]: AMT
                 */
                try 
                {
                    ei1 = new EditInstruct("SMINP", EditInstruct.DELETE_OPERATION);
                    ei1.PutKey("U_FId", UId);
                    ml.Add(ei1);
                    string OldCaseNo = "";
                    string OldCntrNo = "";
                    string olddbCaseNo = "";
                    int SeqNo = 1;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];

                        ei = new EditInstruct("SMINP", EditInstruct.INSERT_OPERATION);
                        string PoNo = Prolink.Math.GetValueAsString(dr[3]);
                        PoNo = PoNo.Replace(" ", "");
                        if (PoNo == "")
                        {
                            break; // 沒有PoNo就結束匯入
                        }
                        string CntrNo = Prolink.Math.GetValueAsString(dr[0]);
                        CntrNo = CntrNo.Replace(" ", "");
                        string NewCaseNo = Prolink.Math.GetValueAsString(dr[2]);
                        NewCaseNo = NewCaseNo.Replace(" ", "");
                        string dbCaseNo = NewCaseNo;
                        decimal CaseNum = 0;

                        if (CntrNo != "")
                        {
                            OldCntrNo = CntrNo;
                        }
                        else
                        {
                            CntrNo = OldCntrNo;
                        }

                        if (NewCaseNo != "")
                        {
                            olddbCaseNo = NewCaseNo;
                            NewCaseNo = NewCaseNo.Replace("0", "");
                            NewCaseNo = NewCaseNo.Replace("-", ";;");
                            NewCaseNo = NewCaseNo.Replace(@"[^A-Za-z]+", "");
                            string[] CaseArray = NewCaseNo.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                            CaseNum = (Convert.ToDecimal(CaseArray[1]) - Convert.ToDecimal(CaseArray[0])) + 1;
                            OldCaseNo = NewCaseNo;
                        }
                        else
                        {
                            dbCaseNo = olddbCaseNo;
                            string[] CaseArray = OldCaseNo.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                            CaseNum = Convert.ToDecimal(CaseArray[1]) - Convert.ToDecimal(CaseArray[0]) + 1;
                            NewCaseNo = OldCaseNo;
                        }
                        

                         //decimal CaseNum = Prolink.Math.GetValueAsDecimal(dr["CASE_NUM"]);
                         decimal Qty = Prolink.Math.GetValueAsDecimal(dr[8]);
                         string GoodsDescp = Prolink.Math.GetValueAsString(dr[7]);
                         string IpartNo = Prolink.Math.GetValueAsString(dr[6]);
                         string PartNo = Prolink.Math.GetValueAsString(dr[4]);
                        string OpartNo = Prolink.Math.GetValueAsString(dr[5]);
                        decimal Nw = Prolink.Math.GetValueAsDecimal(dr[9]);
                        decimal Gw = Prolink.Math.GetValueAsDecimal(dr[10]);
                        decimal Cbm = Prolink.Math.GetValueAsDecimal(dr[11]);
                        decimal UnitPrice = Prolink.Math.GetValueAsDecimal(dr[12]);
                        decimal Amt = Prolink.Math.GetValueAsDecimal(dr[13]);
                        decimal TtlQty = CaseNum * Qty;

                        decimal TtlNw = CaseNum * Nw;
                        decimal TtlGw = CaseNum * Gw;
                        decimal TtlCbm = CaseNum * Cbm;

                        ei.Put("U_ID", System.Guid.NewGuid().ToString());
                        ei.Put("U_FID", UId);
                        ei.Put("SEQ_NO", SeqNo ++);
                        ei.Put("CNTR_NO", CntrNo);
                        ei.Put("PO_NO", PoNo);
                        ei.Put("INVOICE_TYPE", InvoiceType);
                        ei.Put("SHIPMENT_ID", ShipmentId);
                        ei.Put("DN_NO", DnNo);
                        ei.Put("INV_NO", InvNo);
                        ei.Put("CASE_NO", dbCaseNo);
                        ei.Put("CASE_NUM", CaseNum);
                        ei.Put("QTY", Qty);
                        ei.Put("QTYU", "PCS");
                        ei.Put("TTL_QTY", TtlQty);
                        ei.Put("GOODS_DESCP", GoodsDescp);
                        ei.Put("TTL_NW", TtlNw);
                        ei.Put("TTL_GW", TtlGw);
                        ei.Put("TTL_CBM", TtlCbm);
                        ei.Put("OPART_NO", OpartNo.Replace(" ", ""));
                        ei.Put("IPART_NO", IpartNo.Replace(" ", ""));
                        ei.Put("PART_NO", PartNo.Replace(" ", ""));
                        ei.Put("UNIT_PRICE", UnitPrice);
                        ei.Put("AMT", Amt);
                        ei.Put("NW", Nw);
                        ei.Put("GW", Gw);
                        ei.Put("CBM", Cbm);
                        
                        ml.Add(ei);

                        if (i + 1 % 100 == 0)
                         {
                             resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                             ml = new MixedList();
                         }
                    }
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                    string sql = "SELECT IPART_NO, OPART_NO, PO_NO, QTYU, UNIT_PRICE, SUM(QTY) AS TTL_QTY, AVG(AMT) AS AMT FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(UId) + " GROUP BY IPART_NO, OPART_NO, PO_NO, QTYU, UNIT_PRICE";
                    DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (dt1.Rows.Count > 0)
                    {
                        ei2 = new EditInstruct("SMIND", EditInstruct.DELETE_OPERATION);
                        ei2.PutKey("U_FId", UId);
                        mx.Add(ei2);
                        foreach (DataRow item in dt1.Rows)
                        {
                            string IpartNo = Prolink.Math.GetValueAsString(item["IPART_NO"]);
                            string OpartNo = Prolink.Math.GetValueAsString(item["OPART_NO"]);
                            string PoNo = Prolink.Math.GetValueAsString(item["PO_NO"]);
                            string Qtyu = Prolink.Math.GetValueAsString(item["QTYU"]);
                            decimal TtlQty = Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]);
                            decimal UnitPrice = Prolink.Math.GetValueAsDecimal(item["UNIT_PRICE"]);
                            decimal Amt = Prolink.Math.GetValueAsDecimal(item["AMT"]);
                            
                            ei2 = new EditInstruct("SMIND", EditInstruct.INSERT_OPERATION); ;
                            ei2.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei2.Put("U_FID", UId);
                            ei2.Put("INVOICE_TYPE", InvoiceType);
                            ei2.Put("SHIPMENT_ID", ShipmentId);
                            ei2.Put("DN_NO", DnNo);
                            ei2.Put("INV_NO", InvNo);
                            ei2.Put("IPART_NO", IpartNo);
                            ei2.Put("QTY", TtlQty);
                            ei2.Put("QTYU", Qtyu);
                            ei2.Put("UNIT_PRICE1", UnitPrice);
                            ei2.Put("AMT", Amt);
                            ei2.Put("OPART_NO", OpartNo);
                            ei2.Put("PO_NO", PoNo);

                            mx.Add(ei2);

                        }
                        resulst = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
                catch(Exception ex)
                {
                    returnMessage = ex.Message;
                    return Json(new { message = returnMessage });
                }
            }

            return Json(new { message = returnMessage });
        }

        public string CreateDirectorys(string logDir, int logFileDays, string strExt)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.AddDays(-logFileDays);
            if (!System.IO.Directory.Exists(logDir))
                System.IO.Directory.CreateDirectory(logDir);
            string[] dirs = System.IO.Directory.GetDirectories(logDir);
            for (int i = 0; i < dirs.Length; i++)
            {
                try
                {
                    System.IO.DirectoryInfo dirinf = new System.IO.DirectoryInfo(dirs[i]);
                    if (dirinf.CreationTime < from)
                    {
                        System.IO.Directory.Delete(dirs[i], true);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            string path = System.IO.Path.Combine(logDir, to.ToString("yyyy-MM-dd"));
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            string filename = System.IO.Path.Combine(path, to.ToString("yyyyMMddHHmmss"));
            return string.Format("{0}.{1}", filename, strExt);
        }

        private List<string> GetRefSminmOrDN(string DnNo, bool isdn = false)
        {
            string sql = string.Format(@";with REF_DATA AS(
select SMDN.DN_NO,SMDN.REF_NO,SMDN.DN_NO_CMP_REF from SMDN where DN_NO ={0}
union all
select SMDN.DN_NO,SMDN.REF_NO,SMDN.DN_NO_CMP_REF from SMDN inner join REF_DATA r on   r.REF_NO = SMDN.DN_NO 
)
,COM_REF_DATA AS(
select SMDN.DN_NO,SMDN.REF_NO,SMDN.DN_NO_CMP_REF from SMDN where DN_NO ={0}
union all
select SMDN.DN_NO,SMDN.REF_NO,SMDN.DN_NO_CMP_REF from SMDN inner join COM_REF_DATA r on  r.DN_NO_CMP_REF = SMDN.DN_NO and 
SMDN.DN_NO <> r.DN_NO
)
select  * from REF_DATA  where DN_NO !={0}
union all
select  * from COM_REF_DATA  where DN_NO !={0}", SQLUtils.QuotedStr(DnNo));
            DataTable refdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> refUid = new List<string>();
            if (refdndt.Rows.Count > 0)
            {
                List<string> refdn = new List<string>();
                foreach (DataRow refdr in refdndt.Rows)
                {
                    if (!refdn.Contains(Prolink.Math.GetValueAsString(refdr["DN_NO"])))
                    {
                        refdn.Add(Prolink.Math.GetValueAsString(refdr["DN_NO"]));
                    }
                }
                if (isdn)
                    return refdn;
                sql = string.Format("SELECT U_ID FROM SMINM WHERE DN_NO IN {0}", SQLUtils.Quoted(refdn.ToArray()));
                DataTable sminmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow smindr in sminmdt.Rows)
                {
                    if (!refUid.Contains(Prolink.Math.GetValueAsString(smindr["U_ID"])))
                    {
                        refUid.Add(Prolink.Math.GetValueAsString(smindr["U_ID"]));
                    }
                }
            }
            return refUid;
        }

        [HttpPost]
        public ActionResult UploadPackingNew(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList mx = new MixedList();
            string returnMessage = "success";
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);
            string TtlPlt = string.Empty;
            string Pltu = string.Empty;
            string PkgUnitDesc = string.Empty;
            string ShippingMark = string.Empty;
            string SupplierInvNo = string.Empty;
            string UId = string.Empty;
            string InvoiceType = string.Empty;
            string ShipmentId = string.Empty;
            string InvNo = string.Empty;

            if (StartRow <= 0)
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_299;
                return Json(new { message = returnMessage });
            }
            StartRow--;
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
                string strExt = file.FileName.Split('.')[1].ToUpper();
                //string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                string excelFileName = CreateDirectorys(Server.MapPath("~/FileUploads/UploadPackingNew/"), DEFAULT_FILE_SAVE_DATE, strExt);

                file.SaveAs(excelFileName);


                DnNo = Prolink.Math.GetValueAsString(ImportExcelCellValue(excelFileName, strExt, 3, "C"));

                string sql = string.Format("SELECT CD FROM BSCODE WHERE CD_TYPE='TSAP' AND CMP={0}", SQLUtils.QuotedStr(CompanyId));
                string sapid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                Business.TPV.Import.DNManager m = new Business.TPV.Import.DNManager();
                ResultInfo v = m.ImportDNForSAP(sapid, DnNo, "");
                if (!v.IsSucceed)
                {
                    return Json(new { message = "Get DN information from sap error:" + v.Description });
                }

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, 0, 1);
                sql = string.Format(@"SELECT COMBINE_INFO,DN_NO,SHIPMENT_ID,(SELECT TOP 1 INVOICE_TYPE FROM SMINM WHERE SMINM.DN_NO=SMDN.DN_NO) AS INVOICE_TYPE,
                        (SELECT TOP 1 U_ID FROM SMINM WHERE SMINM.DN_NO=SMDN.DN_NO) AS INV_ID,
                        (SELECT TOP 1 INV_NO FROM SMINM WHERE SMINM.DN_NO=SMDN.DN_NO) AS INV_NO
                         FROM SMDN WHERE DN_NO ={0}",SQLUtils.QuotedStr(DnNo));
                DataTable smdndt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smdndt.Rows.Count > 0)
                {
                    UId = Prolink.Math.GetValueAsString(smdndt.Rows[0]["INV_ID"]);
                    InvoiceType = Prolink.Math.GetValueAsString(smdndt.Rows[0]["INVOICE_TYPE"]);
                    ShipmentId = Prolink.Math.GetValueAsString(smdndt.Rows[0]["SHIPMENT_ID"]);
                    InvNo = Prolink.Math.GetValueAsString(smdndt.Rows[0]["INV_NO"]);

                    sql = string.Format("SELECT CMP,SHIPMENT_ID,DN_NO AS COMBINE_INFO FROM SMIDN WHERE DN_NO={0}", SQLUtils.QuotedStr(DnNo));
                    DataTable dndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dndt.Rows.Count > 0)
                    {
                        Business.TPV.Helper.SendICACargoInfoByInbound(dndt.Rows[0]);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    ShippingMark = Prolink.Math.GetValueAsString(dr[0]);
                }

                TtlPlt = Prolink.Math.GetValueAsString(ImportExcelCellValue(excelFileName, strExt, 2, "F"));
                Pltu = Prolink.Math.GetValueAsString(ImportExcelCellValue(excelFileName, strExt, 2, "H"));
                PkgUnitDesc = Prolink.Math.GetValueAsString(ImportExcelCellValue(excelFileName, strExt, 2, "G"));
                SupplierInvNo = Prolink.Math.GetValueAsString(ImportExcelCellValue(excelFileName, strExt, 3, "D"));

                dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);

                //var Cntrnodt = GetCntrno(dt);
                //foreach (var cntr in Cntrnodt)
                //{
                //    string val = Prolink.Math.GetValueAsString(cntr.Value);
                //    if (val == "") return Json(new { message = @Resources.Locale.L_GateManageController_Controllers_133 });

                //    sql = string.Format("SELECT * FROM SMRV WHERE CNTR_NO={0} AND DN_NO LIKE '%{1}%'", SQLUtils.QuotedStr(val), DnNo);
                //    DataTable smrvdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //    if (smrvdt.Rows.Count <= 0)
                //        return Json(new { message = "CONTAINER NO# (" + cntr + ") " + @Resources.Locale.L_GateManageController_Controllers_134 });
                //}

                EditInstruct ei;
                EditInstruct ei1;
                EditInstruct ei2;
                int[] resulst;
                /*
                 * dr[0]:資料庫無對應欄位
                 * dr[1]: PLA_NO
                 * dr[2]: PLA_SIZE
                 * dr[3]: CASE_FROM
                 * dr[4]: CASE_TO
                 * dr[5]: IPART_NO
                 * dr[6]: GOODS_DESCP
                 * dr[7]: QTY
                 * dr[8]: QTYU
                 * dr[9]: NW / UNIT
                 * dr[10]: GW /UNIT
                 * dr[11]: CBM /UNIT
                 * dr[12]: NCM_NO
                 * dr[13]: O_CNTRY
                 * dr[14]: VEN_CD
                 * dr[15]: VEN_NM
                 * dr[16]: VEN_ADDR
                 */
                try
                {
                    List<string> refUid = GetRefSminmOrDN(DnNo);
                    List<string> refDnNo = GetRefSminmOrDN(DnNo, true);
                    ei1 = new EditInstruct("SMINP", EditInstruct.DELETE_OPERATION);
                    ei1.PutKey("U_FId", UId);
                    ml.Add(ei1);
                    string OldCaseNoFrom = "";
                    string OldCaseNoTo = "";
                    string OldPlaNo = "";
                    string OldPlaSize = "";
                    string olddbCaseNoFrom = "";
                    string olddbCaseNoTo = "";
                    //decimal OldGw = 0;
                    int SeqNo = 1;
                    string Nwu = Prolink.Math.GetValueAsString(dt.Rows[0][9]);
                    string Gwu = Prolink.Math.GetValueAsString(dt.Rows[0][10]);
                    string Cbmu = Prolink.Math.GetValueAsString(dt.Rows[0][11]);

                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];

                        ei = new EditInstruct("SMINP", EditInstruct.INSERT_OPERATION);
                        string IpartNo = Prolink.Math.GetValueAsString(dr[5]);
                        if (IpartNo == "")
                        {
                            break; // 沒有PoNo就結束匯入
                        }
                        string PlaNo = Prolink.Math.GetValueAsString(dr[1]);
                        string PlaSize = Prolink.Math.GetValueAsString(dr[2]);
                        decimal Gw = Prolink.Math.GetValueAsDecimal(dr[10]);

                        if (PlaNo != "")
                        {
                            OldPlaNo = PlaNo;
                        }
                        else
                        {
                            if (PlaSize == "")
                                PlaNo = OldPlaNo;
                            //PlaNo = "";
                        }

                        if (PlaSize != "")
                        {
                            OldPlaSize = PlaSize;
                        }
                        else
                        {
                            PlaSize = OldPlaSize;
                            //PlaSize = "";
                        }

                        string NewCaseNoFrom = Prolink.Math.GetValueAsString(dr[3]);
                        NewCaseNoFrom = NewCaseNoFrom.Replace(" ", "");
                        string NewCaseNoTo = Prolink.Math.GetValueAsString(dr[4]);
                        NewCaseNoTo = NewCaseNoTo.Replace(" ", "");

                        string dbCaseNoFrom = NewCaseNoFrom;
                        string dbCaseNoTo = NewCaseNoTo;
                        decimal CaseNum = 0;


                        if (NewCaseNoFrom != "")
                        {
                            olddbCaseNoFrom = NewCaseNoFrom;
                            olddbCaseNoTo = NewCaseNoTo;
                            NewCaseNoFrom = NewCaseNoFrom.TrimStart('0');
                            NewCaseNoTo = NewCaseNoTo.TrimStart('0');
                            string r = @"[a-zA-Z]*";
                            //NewCaseNoFrom = NewCaseNoFrom.Replace(@"[a-zA-Z]*", "");
                            NewCaseNoFrom = Regex.Replace(NewCaseNoFrom, r, "");
                            NewCaseNoTo = Regex.Replace(NewCaseNoTo, r, "");
                            //string[] CaseArray = NewCaseNoFrom.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                            //CaseNum = (Convert.ToDecimal(CaseArray[1]) - Convert.ToDecimal(CaseArray[0])) + 1;
                            CaseNum = (Convert.ToDecimal(NewCaseNoTo) - Convert.ToDecimal(NewCaseNoFrom)) + 1;
                            OldCaseNoFrom = NewCaseNoFrom;
                            OldCaseNoTo = NewCaseNoTo;
                        }
                        else
                        {
                            dbCaseNoFrom = olddbCaseNoFrom;
                            dbCaseNoTo = olddbCaseNoTo;
                            //dbCaseNoFrom = "";
                            //dbCaseNoTo = "";

                            //CaseNum = (Convert.ToDecimal(OldCaseNoTo) - Convert.ToDecimal(OldCaseNoFrom)) + 1;
                            CaseNum = 0;
                            NewCaseNoFrom = OldCaseNoFrom;
                            NewCaseNoTo = OldCaseNoTo;

                        }

                        decimal Qty = Prolink.Math.GetValueAsDecimal(dr[7]);
                        string Qtyu = Prolink.Math.GetValueAsString(dr[8]);
                        string GoodsDescp = Prolink.Math.GetValueAsString(dr[6]);
                        string PartNo = Prolink.Math.GetValueAsString(dr[5]);
                        decimal Nw = Prolink.Math.GetValueAsDecimal(dr[9]);

                        decimal Cbm = Prolink.Math.GetValueAsDecimal(dr[11]);
                        string VenCd = Prolink.Math.GetValueAsString(dr[14]);
                        string VenNm = Prolink.Math.GetValueAsString(dr[15]);
                        string VenAddr = Prolink.Math.GetValueAsString(dr[16]);
                        string NcmNo = Prolink.Math.GetValueAsString(dr[12]);
                        string CntryOrn = Prolink.Math.GetValueAsString(dr[13]);
                        decimal TtlQty = CaseNum * Qty;
                        decimal n = 0;
                        if (CaseNum == 0)
                        {
                            n = 1;
                        }
                        else
                        {
                            n = CaseNum;
                        }
                        //decimal TtlNw = n * Nw;
                        //decimal TtlGw = n * Gw;
                        //decimal TtlCbm = n * Cbm;

                        string dbCaseNo = "";

                        if (dbCaseNoFrom != "")
                        {
                            dbCaseNo = dbCaseNoFrom + "-" + dbCaseNoTo;
                        }

                        string cntrno = Prolink.Math.GetValueAsString(dr[0]);

                        ei.Put("U_ID", System.Guid.NewGuid().ToString());
                        ei.Put("U_FID", UId);
                        ei.Put("SEQ_NO", SeqNo++);
                        ei.Put("PLA_NO", PlaNo);
                        ei.Put("PLA_SIZE", PlaSize);
                        ei.Put("INVOICE_TYPE", InvoiceType);
                        ei.Put("SHIPMENT_ID", ShipmentId);
                        ei.Put("DN_NO", DnNo);
                        ei.Put("INV_NO", InvNo);
                        ei.Put("CASE_NO", dbCaseNo);
                        ei.Put("CASE_NUM", CaseNum);
                        ei.Put("CASE_FROM", dbCaseNoFrom);//NewCaseNoFrom
                        ei.Put("CASE_TO", dbCaseNoTo);//NewCaseNoTo
                        ei.Put("QTY", 0);
                        ei.Put("QTYU", Qtyu);
                        ei.Put("TTL_QTY", Qty);
                        ei.Put("GOODS_DESCP", GoodsDescp);
                        ei.Put("TTL_NW", Nw);
                        ei.Put("TTL_GW", Gw);
                        ei.Put("TTL_CBM", Cbm);
                        ei.Put("IPART_NO", PartNo.Trim());
                        ei.Put("PART_NO", PartNo.Trim());
                        ei.Put("NW", 0);
                        ei.Put("GW", 0);
                        ei.Put("CBM", 0);
                        ei.Put("NWU", Nwu);
                        ei.Put("GWU", Gwu);
                        ei.Put("CBMU", Cbmu);
                        ei.Put("VEN_CD", VenCd);
                        ei.Put("VEN_NM", VenNm);
                        ei.Put("VEN_ADDR", VenAddr);
                        ei.Put("NCM_NO", NcmNo);
                        ei.Put("CNTRY_ORN", CntryOrn);
                        ei.Put("CNTR_NO", cntrno);
                        ml.Add(ei);

                        if (i + 1 % 100 == 0)
                        {
                            resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            ml = new MixedList();
                        }
                    }
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                    EditInstruct ei3 = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
                    ei3.Put("PACKING_FROM", "P");
                    ei3.Put("TTL_PLT", TtlPlt);
                    ei3.Put("PLTU", Pltu);
                    ei3.Put("PKG_UNIT_DESC", PkgUnitDesc);
                    ei3.Put("SUPPLIER_INV_NO", SupplierInvNo);
                    ei3.Put("UPLOAD_BY", UserId);
                    ei3.PutKey("U_ID", UId);
                    if (!string.IsNullOrEmpty(ShippingMark))
                        ei3.Put("MARKS", ShippingMark);
                    MixedList ml3 = new MixedList();
                    ml3.Add(ei3);
                    foreach (string refuid in refUid)
                    {
                        ei3 = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
                        ei3.Put("PACKING_FROM", "P");
                        ei3.Put("TTL_PLT", TtlPlt);
                        ei3.Put("PLTU", Pltu);
                        ei3.Put("PKG_UNIT_DESC", PkgUnitDesc);
                        ei3.PutKey("U_ID", refuid);
                        ei3.Put("SUPPLIER_INV_NO", SupplierInvNo);
                        if (!string.IsNullOrEmpty(ShippingMark))
                            ei3.Put("MARKS", ShippingMark);
                        ml3.Add(ei3);
                    }
                    EditInstruct ei4 = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    ei4.Put("PKG_NUM", TtlPlt);
                    ei4.Put("PKG_UNIT", Pltu);
                    ei4.Put("PKG_UNIT_DESC", PkgUnitDesc);
                    ei4.PutKey("DN_NO", DnNo);
                    ml3.Add(ei4);
                    foreach (string refdn in refDnNo)
                    {
                        ei4 = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                        ei4.Put("PKG_NUM", TtlPlt);
                        ei4.Put("PKG_UNIT", Pltu);
                        ei4.Put("PKG_UNIT_DESC", PkgUnitDesc);
                        ei4.PutKey("DN_NO", refdn);
                        ml3.Add(ei4);
                    }
                    sql = string.Format("SELECT * FROM SMINP WHERE U_FID={0}", SQLUtils.QuotedStr(UId));
                    DataTable sminpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (string refuid in refUid)
                    {
                        SMHandle.ToEi(sminpdt, "SMINP", ml3, "", refuid, null);
                    }
                    //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    resulst = OperationUtils.ExecuteUpdate(ml3, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                    return Json(new { message = returnMessage });
                }
            }
            Business.Service.ResultInfo reserveinfo = null;
            Business.Service.ResultInfo result = SendPackingToSAP(UId, out reserveinfo);
            if (!result.IsSucceed)
            {
                returnMessage = " Update Detail Packing Succeed，But Send Packing To SAP failure，SAP return notice：" + result.Description;
            }
            if (reserveinfo != null && !reserveinfo.IsSucceed && !string.IsNullOrEmpty(reserveinfo.Description))
            {
                returnMessage = " Update Detail Packing successful, Send Packing To SAP successful，But receive Packing info. from SAP failure,SAP return notice：" + reserveinfo.Description;
            }
            return Json(new { message = returnMessage, TtlPlt = TtlPlt, Pltu = Pltu, PkgUnitDesc = PkgUnitDesc, UploadBy = UserId, Marks = ShippingMark, supplierNo = SupplierInvNo });
        }

        public ActionResult HandSendPackingToSAP()
        {
            string returnMsg = "Success!";
            string isok = "Y";
            string uid = Request.Params["uids"];

            string[] uids = uid.Split(',');
            foreach (string uidindex in uids)
            {
                if (string.IsNullOrEmpty(uidindex))
                    continue;

                string sqlisok = string.Format("SELECT IS_OK FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO IN (SELECT DN_NO FROM SMINM WHERE U_ID={0}))", SQLUtils.QuotedStr(uidindex));
                string issendtosap = OperationUtils.GetValueAsString(sqlisok, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (issendtosap == "Y")
                    return Json(new { IsOk = "N", message = "This Invoice is transfer to Inbound，so can not upload Detail Packing！" });

                Business.Service.ResultInfo reserveinfo = null;
                Business.Service.ResultInfo rf = SendPackingToSAP(uidindex, out reserveinfo);
                if (!rf.IsSucceed)
                {
                    returnMsg = "Send To SAP Error!";
                    isok = "N";
                }
                if (reserveinfo != null && !reserveinfo.IsSucceed)
                {
                    returnMsg = " Update Detail Packing successful, Send Packing To SAP successful，But receive Packing info. from SAP failure,SAP return notice：" + reserveinfo.Description;
                }
            }
            return Json(new { message = returnMsg, IsOk = isok });
        }

        public ResultInfo SendPackingToSAP(string uid, out Business.Service.ResultInfo reserveinfo)
        {
            reserveinfo = new Business.Service.ResultInfo();
            string sapId = Business.TPV.Helper.GetSapId(CompanyId);
            Business.TPV.Export.PackingManager m = new Business.TPV.Export.PackingManager();

            List<Business.TPV.Export.PackingInfo> items = m.GetPackingInfo(uid);
            Business.Service.ResultInfo result = m.TryPostPackingInfo(sapId, items, "",out reserveinfo);
            return result;
        }


        private Dictionary<int, object> GetCntrno(DataTable dt)
        {
            Dictionary<int, object> cntrno = new Dictionary<int, object>();
            int i = 1;
            foreach (DataRow dr in dt.Rows)
            {
                if (i == 1) { i++; continue; }
                string IpartNo = Prolink.Math.GetValueAsString(dr[5]);
                if (IpartNo == "")
                {
                    break; // 沒有PoNo就結束匯入
                }
                string cntr = Prolink.Math.GetValueAsString(dr[0]);
                if (!cntrno.ContainsValue(cntr))
                {
                    cntrno.Add(cntrno.Count, cntr);
                }
            }
            return cntrno;
        }
       
        //无用
        public void ResetDNSM(string dnno)
        {
            string sql = string.Format("SELECT * FROM SMINM WHERE DN_NO='{0}' AND CMP='{1}' AND GROUP_ID='{2}'", dnno, CompanyId, GroupId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) throw new Exception(@Resources.Locale.L_GateManageController_Controllers_135);
            double gw = 0;
            double cbm = 0;
            string invoicetype = string.Empty;
            string shipid = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                gw = Prolink.Math.GetValueAsDouble(dr["TTL_GW"]);
                cbm = Prolink.Math.GetValueAsDouble(dr["TTL_CBM"]);
                invoicetype = Prolink.Math.GetValueAsString(dr["INVOICE_TYPE"]);
                shipid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            }

            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("DN_NO", dnno);
            ei.PutKey("CMP", CompanyId);
            ei.PutKey("GROUP_ID", GroupId);
            ei.Put("GW", gw);
            ei.Put("CBM", cbm);
            ml.Add(ei);
            sql = string.Format("SELECT * FROM SMINM WHERE SHIPMENT_ID='{0}' AND CMP='{1}' AND GROUP_ID='{2}' AND INVOICE_TYPE='{3}'", shipid, CompanyId, GroupId, invoicetype);
            DataTable cmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            double ttlgw = 0;
            double ttlcbm = 0;
            foreach (DataRow dr in cmdt.Rows)
            {
                ttlgw += Prolink.Math.GetValueAsDouble(dr["TTL_GW"]);
                ttlcbm += Prolink.Math.GetValueAsDouble(dr["TTL_CBM"]);
            }
            EditInstruct ei1 = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei1.PutKey("SHIPMENT_ID", shipid);
            ei1.PutKey("CMP", CompanyId);
            ei1.PutKey("GROUP_ID", GroupId);
            ei1.Put("GW", ttlgw);
            ei1.Put("CBM", ttlcbm);
            ml.Add(ei1);
            sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID='{0}' AND CMP='{1}' AND GROUP_ID='{2}'", shipid, CompanyId, GroupId);
            DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string CombinShipment = string.Empty;
            if (smdt.Rows.Count > 0)
            {
                CombinShipment = Prolink.Math.GetValueAsString(smdt.Rows[0]["COMBIN_SHIPMENT"]);
                ttlgw = 0;
                ttlcbm = 0;
                if (!string.IsNullOrEmpty(CombinShipment))
                {
                    sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID='{0}' AND CMP='{1}' AND GROUP_ID='{2}'", CombinShipment, CompanyId, GroupId);
                    DataTable allsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow dr in allsmdt.Rows)
                    {
                        string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        if (shipmentid.Equals(CombinShipment)) continue;
                        ttlgw += Prolink.Math.GetValueAsDouble(dr["GW"]);
                        ttlcbm += Prolink.Math.GetValueAsDouble(dr["CBM"]);
                    }
                    EditInstruct ei2 = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                    ei2.PutKey("SHIPMENT_ID", CombinShipment);
                    ei2.PutKey("CMP", CompanyId);
                    ei2.PutKey("GROUP_ID", GroupId);
                    ei2.Put("GW", ttlgw);
                    ei2.Put("CBM", ttlcbm);
                    ml.Add(ei2);
                }
            }
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {

            }
        }
        public bool CheckDN(string filepath, string strExt, int star, string Odn)
        {
            bool result = false;
            DataTable dt = ImportExcelToDataTable(filepath, strExt,star-2,star-1);
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string dnno = Prolink.Math.GetValueAsString(dr[2]);
                if (dnno != "" && dnno == Odn) return !result;
            }
            return result;
        }
        [HttpPost]
        public ActionResult UploadSmrv(FormCollection form)
        {
            List<string> idList = new List<string>();
            MixedList ml = new MixedList();
            string returnMessage = "success";
            string ermsg = "";
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

            if (StartRow == 0)
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_299;
                return Json(new { message = returnMessage });
            }

            //List<Dictionary<string, object>> smdnpData = new List<Dictionary<string, object>>();
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

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);

                /*
                 * dr[0]:用櫃日期
                 * dr[1]: 線別
                 * dr[2]: 機種
                 * dr[3]: 工單量
                 * dr[4]: 工單號
                 * dr[5]: 用櫃時間點
                 * dr[6]: 備註
                 * dr[7]: 裝櫃量
                 * dr[8]: 船公司
                 * dr[9]: 拖車公司
                 * dr[10]: 櫃型
                 * dr[11]: 櫃量
                 * dr[12]: 出貨單號
                 */
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string DnNo = "";
                    try
                    {
                        DataRow dr = dt.Rows[i];

                        DnNo = Prolink.Math.GetValueAsString(dr[12]);
                        string IpartNo = Prolink.Math.GetValueAsString(dr[2]);
                        string JobNo = Prolink.Math.GetValueAsString(dr[4]);
                        string ProductLine = Prolink.Math.GetValueAsString(dr[1]);
                        //string rvremark = Prolink.Math.GetValueAsString(dr[26]);
                        if (DnNo != "")
                        {
                            //string b = Prolink.Math.GetValueAsString(dr[0]);
                            //double dd = double.Parse(b);
                            //DateTime conv = DateTime.FromOADate(dd);
                            if (Prolink.Math.GetValueAsString(dr[0]) == "")
                            {
                                ermsg += "Dn No: " + DnNo + ": " + @Resources.Locale.L_GateManageController_Controllers_136 + "\n";
                                continue;
                            }
                            DateTime conv = Prolink.Math.GetValueAsDateTime(dr[0]);
                            string UseDate = conv.ToString("yyyy-MM-dd");

                            string t = Prolink.Math.GetValueAsString(dr[5]);

                            DateTime convTime = Convert.ToDateTime(t);
                            string UseTime = convTime.ToString("HH:mm");

                            string CombineDate = UseDate + " " + UseTime;
                            decimal CntNumber = Prolink.Math.GetValueAsDecimal(dr[11]);
                            if (CntNumber == 0)
                            {
                                CntNumber = 1;
                            }

                            string sql = "SELECT TOP 1 * FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
                            DataTable dt1 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                            if (dt1.Rows.Count > 0)
                            {
                                foreach (DataRow item in dt1.Rows)
                                {
                                    string ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                                    string returnMsg = SMHandle.QAHoldBlMessage(ShipmentId);
                                    if (!string.IsNullOrEmpty(returnMsg))
                                    {
                                        ermsg += "Dn No: " + DnNo + ":" + returnMsg + "\n";
                                        continue;
                                    }
                                    string g = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                                    string c = Prolink.Math.GetValueAsString(item["CMP"]);
                                    string d = Prolink.Math.GetValueAsString(item["DEP"]);
                                    string exsit = Business.ReserveManage.ChkSmrv(ShipmentId);
                                    string Torder = getTorder(ShipmentId);
                                    if (exsit != "success")
                                    {
                                        //問user要不要重新叫車，如果重新叫車，會把原來的預約記錄刪掉再新增
                                    }

                                    if (Torder == "N" || Torder == "")
                                    {
                                        ermsg += ShipmentId + ":" + @Resources.Locale.L_GateManageController_Controllers_147 + "\n";
                                        continue;
                                    }

                                    //if (!string.IsNullOrEmpty(rvremark))
                                    //{
                                    //    string rvsql = string.Format("UPDATE SMSM SET RV_REMARK={0} WHERE SHIPMENT_ID={1}", SQLUtils.QuotedStr(rvremark), SQLUtils.QuotedStr(ShipmentId));
                                    //    OperationUtils.GetDataTable(rvsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    //}
                                    if (DnNo != "" && IpartNo != "" && JobNo != "")
                                    {
                                        string updateJobNo = Business.ReserveManage.updateJobNo(DnNo, IpartNo, JobNo, ProductLine);

                                        if (updateJobNo == "success")
                                        {
                                            sql = "SELECT COUNT(*) FROM SMRV WHERE SHIPMENT_ID=+" + SQLUtils.QuotedStr(ShipmentId);
                                            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                            string result = "success";
                                            if (num == 0)
                                            {
                                                result = Business.ReserveManage.OrderTrucker(ShipmentId, g, c, d, Ext, UserId,idList, 0, 0, CombineDate, "N", "", DnNo);
                                            }
                                            else
                                            {
                                                ermsg += "Dn No: " + DnNo + ": " + @Resources.Locale.L_GateManageController_Controllers_137 + "\n";
                                            }

                                            if (result != "success")
                                            {
                                                ermsg += "Dn No: " + DnNo + "," + result + "\n";
                                            }
                                            else if (result == "success")
                                            {
                                                sql = "SELECT COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                                string combine_info = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                                string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                                                string updateMsg = "success";
                                                if (dns.Length > 0)
                                                {
                                                    for (int j = 0; j < dns.Length; j++)
                                                    {
                                                        updateMsg = Business.DNApproveLoop.UpdateApproveRecord(dns[j], CompanyId, "FPW", UserId,GetBaseCmp());
                                                    }
                                                }
                                                if (updateMsg != "success")
                                                {
                                                    ermsg += "Dn No: " + DnNo + ":" + updateMsg + "\n";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ermsg += "Dn No: " + DnNo + ": " + @Resources.Locale.L_GateManageController_Controllers_138 + "\n";
                                        }
                                    }
                                    else
                                    {
                                        ermsg += "Dn No: " + DnNo + ": " + @Resources.Locale.L_GateManageController_Controllers_139 + "\n";
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ermsg += DnNo + ":" + ex.Message + "\n";
                    }
                        
                }
                
            }

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            return Json(new { message = returnMessage, errorMsg = ermsg });
        }
        #endregion  

        #region 月台動態action
        /*離開月台*/
        public JsonResult setLeaveGate()
        {
            string returnMessage = "success";
            string ReserveNo = Prolink.Math.GetValueAsString(Request.Params["ReserveNo"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string GateNo = Prolink.Math.GetValueAsString(Request.Params["GateNo"]);
            string now = DateTime.Now.ToString();
            MixedList mixList = new MixedList();
            EditInstruct ei;
            EditInstruct ei2;
            if (ReserveNo == "")
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_310;
                return Json(new { msg = returnMessage });
            }

            string sql = "SELECT TOP 1 * FROM SMRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string UId = Prolink.Math.GetValueAsString(item["U_ID"]);
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);

                    sql = "UPDATE SMWHGT SET CNTR_NO='',RESERVE_NO='' WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo);
                    mixList.Add(sql);
                    ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", UId);
                    ei.Put("STATUS", "G");
                    //ei.PutDate("OUT_DATE", DateTime.Now);
                    mixList.Add(ei);

                    //sql = "SELECT DN_NO FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                    //string DnNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //ei2 = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    //ei2.PutKey("DN_NO", DnNo);
                    //ei2.Put("STATUS", "O");
                    //ei2.PutDate("OUT_DATE", DateTime.Now);
                    //mixList.Add(ei2);

                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.Message;
                        return Json(new { msg = returnMessage });
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_140;
            }

            return Json(new { msg = returnMessage });
        }
        /*移動貨櫃*/
        public JsonResult setMoveGate()
        {
            string returnMessage = "success";
            string ReserveNo = Prolink.Math.GetValueAsString(Request.Params["ReserveNo"]);
            string oldWsCd = Prolink.Math.GetValueAsString(Request.Params["oldWsCd"]);
            string newWsCd = Prolink.Math.GetValueAsString(Request.Params["newWsCd"]);
            string oldGateNo = Prolink.Math.GetValueAsString(Request.Params["oldGateNo"]);
            string newGateNo = Prolink.Math.GetValueAsString(Request.Params["newGateNo"]);
            MixedList mixList = new MixedList();
            string sql = "SELECT TOP 1 * FROM SMRV WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei;

            if (ReserveNo == "")
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_310;
                return Json(new { msg = returnMessage });
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string UFid = Prolink.Math.GetValueAsString(item["U_ID"]);
                    string CntrNo = Prolink.Math.GetValueAsString(item["CNTR_NO"]);
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string Trucker = Prolink.Math.GetValueAsString(item["TRUCKER"]);

                    string TruckCntrno = Prolink.Math.GetValueAsString(item["TRUCK_CNTRNO"]);
                    string TruckNo = Prolink.Math.GetValueAsString(item["TRUCK_NO"]);

                    if (CntrNo == "" && TruckCntrno == "")
                    {
                        CntrNo = TruckNo;
                    }
                    else if (CntrNo == "")
                    {
                        CntrNo = TruckCntrno;
                    }

                    sql = "UPDATE SMWHGT SET CNTR_NO=" + SQLUtils.QuotedStr(CntrNo) + ",RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo) + " WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(newWsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(newGateNo);
                    mixList.Add(sql);
                    sql = "UPDATE SMWHGT SET CNTR_NO='',RESERVE_NO='' WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(oldWsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(oldGateNo);
                    mixList.Add(sql);
                    sql = "UPDATE SMRV SET WS_CD=" + SQLUtils.QuotedStr(newWsCd) + ", GATE_NO=" + SQLUtils.QuotedStr(newGateNo) + " WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    mixList.Add(sql);
                    sql = "SELECT COUNT(*) AS SEQ_NO FROM SMRVM WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    int SeqNo = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    ei = new EditInstruct("SMRVM", EditInstruct.INSERT_OPERATION);
                    ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    ei.Put("U_FID", UFid);
                    ei.Put("SEQ_NO", SeqNo + 1);
                    ei.Put("CNTR_NO", CntrNo);
                    ei.Put("TRUCKER", Trucker);
                    ei.Put("TRUCK_NO", TruckNo);
                    DateTime odt = DateTime.Now;                   
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    ei.PutDate("MOVING_DATE", odt);
                    ei.PutDate("MOVING_DATE_L", ndt);
                    ei.Put("OWS_CD", oldWsCd);
                    ei.Put("OGATE_NO", oldGateNo);
                    ei.Put("NWS_CD", newWsCd);
                    ei.Put("NGATE_NO", newGateNo);
                    ei.Put("RESERVE_NO", ReserveNo);
                    ei.Put("MODIFY_BY", UserId);
                    ei.PutDate("MODIFY_DATE", odt);
                    ei.PutDate("MODIFY_DATE_L", ndt);
                    ei.Put("BAT_NO", ReserveNo);

                    mixList.Add(ei);

                    sql = "UPDATE SMRV SET MOVE_NUMBER=" + (SeqNo + 1) + " WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    mixList.Add(sql);
                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //Business.ReserveManage.syncBatNo(UFid);
                        SetSmrvNightRemove(ReserveNo);
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.Message;
                        return Json(new { msg = returnMessage });
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_141;
            }



            return Json(new { msg = returnMessage });
        }
        public void SetSmrvNightRemove(string ReserveNo)
        {
            string sql = string.Format("SELECT *  FROM SMRVM WHERE BAT_NO='{0}'", ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int count = 0;
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    DateTime movingdate = Prolink.Math.GetValueAsDateTime(dr["MOVING_DATE"]);
                    int time = movingdate.Hour;
                    if (time >= 21 || time <= 3)
                        count++;
                }
            }
            sql = "UPDATE SMRV SET NIGHT_MOVE=" + count + " WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        /*進入月台*/
        public JsonResult setEnterToGate()
        {
            string returnMessage = "success";
            string ReserveNo = Prolink.Math.GetValueAsString(Request.Params["ReserveNo"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string GateNo = Prolink.Math.GetValueAsString(Request.Params["GateNo"]);
            string sql = "SELECT TOP 1 * FROM SMRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (ReserveNo == "")
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_141;
                return Json(new { msg = returnMessage });
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string CntrNo = Prolink.Math.GetValueAsString(item["CNTR_NO"]);
                    string TruckCntrno = Prolink.Math.GetValueAsString(item["TRUCK_CNTRNO"]);
                    string TruckNo = Prolink.Math.GetValueAsString(item["TRUCK_NO"]);
                    //string GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string UId = Prolink.Math.GetValueAsString(item["U_ID"]);
                    string BatNo = Prolink.Math.GetValueAsString(item["BAT_NO"]);

                    if (CntrNo == "" && TruckCntrno == "")
                    {
                        CntrNo = TruckNo;
                    }
                    else if (CntrNo == "")
                    {
                        CntrNo = TruckCntrno;
                    }

                    sql = "UPDATE SMWHGT SET CNTR_NO=" + SQLUtils.QuotedStr(CntrNo) + ",RESERVE_NO=" + SQLUtils.QuotedStr(BatNo) + " WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo);
                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        sql = "UPDATE SMRV SET STATUS='G', WS_CD={0}, GATE_NO={1}, TEMP_WSCD={0},TEMP_GATENO={1} WHERE BAT_NO={2}" ;
                        sql = string.Format(sql, SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(GateNo),SQLUtils.QuotedStr(BatNo));
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        //Business.ReserveManage.syncBatNo(UId);
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.Message;
                        return Json(new { msg = returnMessage });
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_141;
            }

            return Json(new { msg = returnMessage });
        }
        #endregion

        #region 月台管理分析
        /*月台管理分析*/
        public ActionResult gateAnalysisQuery()
        {
            string condition = GetBookingCondition();

            if (PlantPri != "''")
            {
                if (string.IsNullOrEmpty(PlantPri))
                {
                    condition += " AND PLANT in ( '' ) ";
                }
                else
                {
                    condition += string.Format(" AND PLANT in ( {0} ) ", PlantPri);
                }
            }
            condition += " AND (RV_TYPE<>'I' OR RV_TYPE IS NULL)";
            //return GetBaseData("SMRV", condition, "*", " WS_CD ASC", "2");
            string table = @"(SELECT SMRV.*, SMSM.ETD,SMSM.INCOTERM_CD,SMSM.FC_CD,SMSM.FC_NM,SMSM.POL_CD,SMSM.POL_NAME FROM SMRV LEFT JOIN SMSM 
                             ON SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID) SMRV ";
            return GetBootstrapData(table, condition);
        }
        /*移動貨櫃資訊*/
        public ActionResult getMoveInfo()
        {
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            string BatNo = Prolink.Math.GetValueAsString(Request.Params["BatNo"]);

            string condition = " BAT_NO=" + SQLUtils.QuotedStr(BatNo);
            return GetBaseData("SMRVM", condition, "*", " SEQ_NO ASC", "2");
        }
        #endregion

        #region 月台設定
            #region 纯查询
            public ActionResult QueryData()
            {
                string condition = GetBookingCondition();
                return GetBootstrapData("SMWH", condition);
            }
            #endregion

            #region 取得資料
            public ActionResult GetDetail()
            {
                string u_id = Request["UId"];
                string sql = string.Format("SELECT * FROM SMWH WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
                DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMWHGT WHERE U_FID={0} ORDER BY GATE_NO ASC", SQLUtils.QuotedStr(u_id));
                DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                Dictionary<string, object> data = new Dictionary<string, object>();
                data["main"] = ModelFactory.ToTableJson(mainDt, "SmwhModel");
                data["sub"] = ModelFactory.ToTableJson(subDt, "SmwhgtModel");
                return ToContent(data);
            }
            #endregion
       
            #region 保存
            public ActionResult UpdateData()
            {
                string changeData = Request.Params["changedData"];
                string UId = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
                string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
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

                if (!string.IsNullOrEmpty(UId))
                    UId = HttpUtility.UrlDecode(UId);
                if (!string.IsNullOrEmpty(WsCd))
                    WsCd = HttpUtility.UrlDecode(WsCd);
                if (!string.IsNullOrEmpty(Cmp))
                    Cmp = HttpUtility.UrlDecode(Cmp);


                Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
                MixedList mixList = new MixedList();
                foreach (var item in dict)
                {
                    if (item.Key == "mt")
                    {
                        ArrayList objList = item.Value as ArrayList;

                        MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmwhModel");
                        for (int i = 0; i < list.Count; i++)
                        {
                            EditInstruct ei = (EditInstruct)list[i];
                            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                            {
                                UId = System.Guid.NewGuid().ToString();
                                ei.Put("U_ID", UId);
                                ei.Put("GROUP_ID", GroupId);
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
                                EditInstruct ei2 = new EditInstruct("SMWHGT", EditInstruct.DELETE_OPERATION);
                                if (ei.Get("U_ID") == null)
                                {
                                    continue;
                                }

                                ei2.PutKey("U_FID", UId);
                                mixList.Add(ei2);
                            }
                            mixList.Add(ei);
                        }
                    }
                    else if (item.Key == "sub")
                    {
                        ArrayList objList = item.Value as ArrayList;

                        MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmwhgtModel");
                        for (int i = 0; i < list.Count; i++)
                        {
                            EditInstruct ei = (EditInstruct)list[i];
                            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                            {
                                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                                ei.Put("CMP", Cmp);
                                ei.Put("WS_CD", WsCd);
                                ei.Put("U_FID", UId);
                            }
                            else
                                ei.AddKey("U_ID");
                            //string test_id = ei.Get("U_ID");
                            System.Guid test_id = System.Guid.NewGuid();
                            if (!System.Guid.TryParse(ei.Get("U_ID"), out test_id))
                                continue;

                            ei.Put("CMP", Cmp);
                            ei.Put("WS_CD", WsCd);
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

                string sql = string.Format("SELECT * FROM SMWH WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = string.Format("SELECT * FROM SMWHGT WHERE U_FID={0} ORDER BY GATE_NO ASC", SQLUtils.QuotedStr(UId));
                DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                Dictionary<string, object> data = new Dictionary<string, object>();
                data["main"] = ModelFactory.ToTableJson(mainDt, "SmwhModel");
                data["sub"] = ModelFactory.ToTableJson(subDt, "SmwhgtModel");
                return ToContent(data);
            }
            #endregion
        #endregion

            /*保存月台*/
        public ActionResult SaveGateReseve()
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
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string RelationId = Prolink.Math.GetValueAsString(Request.Params["RelationId"]);
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmrvModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;                            
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            //ei.Put("ORDER_BY", UserId);
                            //ei.PutDate("ORDER_DATE", DateTime.Now);
                            string GateNo = Prolink.Math.GetValueAsString(ei.Get("GATE_NO"));
                            string TempWscd = Prolink.Math.GetValueAsString(ei.Get("WS_CD"));
                            string TempRdate = Prolink.Math.GetValueAsString(ei.Get("RESERVE_DATE"));
                            string TempRfrom = Prolink.Math.GetValueAsString(ei.Get("RESERVE_FROM"));
                            string TempRh = Prolink.Math.GetValueAsString(ei.Get("RESERVE_HOUR"));
                            string TruckNo = Prolink.Math.GetValueAsString(ei.Get("TRUCK_NO"));
                            string Driver = Prolink.Math.GetValueAsString(ei.Get("DRIVER"));
                            string Tel = Prolink.Math.GetValueAsString(ei.Get("TEL"));
                            string DriverId = Prolink.Math.GetValueAsString(ei.Get("DRIVER_ID"));
                            if (GateNo != "")
                            {
                                ei.Put("TEMP_GATENO", GateNo);
                            }
                            if (TempWscd != "")
                            {
                                ei.Put("TEMP_WSCD", TempWscd);
                            }
                            if (TempRdate != "")
                            {
                                ei.PutDate("TEMP_RDATE", TempRdate);
                            }
                            if (TempRfrom != "")
                            {
                                ei.Put("TEMP_RFROM", TempRfrom);
                            }
                            if (TempRh != "")
                            {
                                ei.Put("TEMP_RH", TempRh);
                            }
                            //string SealNo1 = Prolink.Math.GetValueAsString(ei.Get("SEAL_NO1"));
                            string Status = Prolink.Math.GetValueAsString(ei.Get("STATUS"));

                            //if (SealNo1 != "")
                            //{
                            //    ei.Put("STATUS", "P");
                            //}

                            if (Status == "I")
                            {
                                DateTime odt = DateTime.Now;                               
                                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                                
                                ei.PutDate("IN_DATE", odt);
                                ei.PutDate("IN_DATE_L", ndt);
                            }

                            sql = @"SELECT TOP 1 SMSM.TRAN_TYPE FROM SMSM INNER JOIN SMRV ON SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID WHERE SMRV.U_ID=" + SQLUtils.QuotedStr(UId);
                            string TranType = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                            if (TranType != "F")
                            {
                                if (TruckNo != "")
                                {
                                    ei.Put("LTRUCK_NO", TruckNo);
                                }

                                if (Driver != "")
                                {
                                    ei.Put("LDRIVER", Driver);
                                }

                                if (Tel != "")
                                {
                                    ei.Put("LTEL", Tel);
                                }

                                if (DriverId != "")
                                {
                                    ei.Put("LDRIVER_ID", DriverId);
                                }
                            }

                            if (RelationId != "")
                            {
                                sql = "SELECT BAT_NO, GROUP_ID, CMP FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                                DataTable rvDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                                if (rvDt.Rows.Count > 0)
                                {
                                    string group_id = Prolink.Math.GetValueAsString(rvDt.Rows[0]["GROUP_ID"]);
                                    string cmp = Prolink.Math.GetValueAsString(rvDt.Rows[0]["CMP"]);
                                    string BatNo = Prolink.Math.GetValueAsString(rvDt.Rows[0]["BAT_NO"]);

                                    if (BatNo == "")
                                    {
                                        BatNo = Business.ReserveManage.getAutoNo("BAT_NO", group_id, cmp);
                                        ei.Put("BAT_NO", BatNo);
                                    }
                                }
                            }
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
                    if (RelationId != "")
                    {
                        bool copy = copyToOtherReserve(UId, RelationId);
                        if (copy == false)
                        {
                            returnMessage = @Resources.Locale.L_GateManageController_Controllers_142;
                        }
                    }
                    else
                    {
                        bool sync = Business.ReserveManage.syncBatNo(UId);

                        if (sync == false)
                        {
                            returnMessage = @Resources.Locale.L_GateManageController_Controllers_142;
                        }

                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            List<Dictionary<string, object>> rvData = new List<Dictionary<string, object>>();
            sql = string.Format("SELECT * FROM SMRV WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            rvData = ModelFactory.ToTableJson(groupDt, "SmrvModel");

            return Json(new { message = returnMessage, mainData = rvData });
        }

        /*卡車公司預約*/
        public ActionResult ReverseGate()
        {
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = false;
            string BatNo = string.Empty;
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            { 
                foreach(DataRow item in dt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    string GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    //string ReserveDate = Prolink.Math.GetValueAsString(item["RESERVE_DATE"]);
                    string ReserveDate = ((DateTime)item["RESERVE_DATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    int ReserveFrom = Prolink.Math.GetValueAsInt(item["RESERVE_FROM"]);
                    int Hour = Prolink.Math.GetValueAsInt(item["RESERVE_HOUR"]);
                    
                    if (WsCd != "" && GateNo != "" && ReserveDate != "")
                    {
                        chk = InsertRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour, UId);
                        if (chk == true)
                        {
                            DateTime odt = DateTime.Now;                          
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            sql = "UPDATE SMRV SET STATUS='R', ORDER_BY=" + SQLUtils.QuotedStr(UserId) + ", ORDER_DATE=" + SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")) + ", ORDER_DATE_L=" + SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")) + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                            
                            try
                            {
                                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                sql = "SELECT BAT_NO, GROUP_ID, CMP FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                                DataTable rvDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                                if (rvDt.Rows.Count > 0)
                                {
                                    string group_id = Prolink.Math.GetValueAsString(rvDt.Rows[0]["GROUP_ID"]);
                                    string cmp = Prolink.Math.GetValueAsString(rvDt.Rows[0]["CMP"]);
                                    BatNo = Prolink.Math.GetValueAsString(rvDt.Rows[0]["BAT_NO"]);

                                    if (BatNo == "")
                                    {
                                        BatNo = Business.ReserveManage.getAutoNo("BAT_NO", group_id, cmp);
                                        sql = "UPDATE SMRV SET BAT_NO={0} WHERE U_ID={1}";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(BatNo), SQLUtils.QuotedStr(UId));
                                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    }
                                }
                                chk = true;
                            }
                            catch (Exception ex)
                            {
                                chk = false;
                            }
                        }
                        bool sysc = Business.ReserveManage.syncBatNo(UId);
                        if(sysc == false)
                        {
                            chk = false;
                        }

                    }
                    else
                    {
                        /*
                        MixedList ml = new MixedList();
                        EditInstruct ei;
                        int[] resulst;
                        ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("U_ID", UId);
                        ei.Put("TEMP_WSCD", WsCd);
                        ei.Put("TEMP_GATENO", GateNo);
                        ei.PutDate("TEMP_RDATE", ReserveDate);
                        ei.Put("TEMP_RFROM", ReserveFrom);
                        ei.Put("TEMP_RH", Hour);
                        ei.Put("STATUS", "R");
                        ei.Put("ORDER_BY", UserId);
                        ei.PutDate("ORDER_DATE", DateTime.Now);
                        ml.Add(ei);
                        //sql = "UPDATE SMRV SET STATUS='R', TEMP_WSCD="+SQLUtils.QuotedStr(WsCd)+", WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                        try
                        {
                            //Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            chk = true;
                            chk = Business.ReserveManage.syncBatNo(UId);
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.Message;
                        }
                        */
                        returnMessage = @Resources.Locale.L_GateManageController_Controllers_143;
                    }
                    
                }
            }

            if (chk == false)
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_144;
            }

            return Json(new { message = returnMessage, BatNo = BatNo });
        }

        /*預約確認*/
        public ActionResult ConfirmReverseGate()
        { 
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = false, updateData=false;
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string WsCd = "", GateNo = "", ReserveDate = "";
            string TempWscd = "", TempGateno = "", TempRdate = "";
            int TempRfrom = 0, Hour = 0;
            int ReserveFrom = 0, TempRh = 0;

            if (dt.Rows.Count > 0)
            { 
                foreach(DataRow item in dt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    ReserveDate = ((DateTime)item["RESERVE_DATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    ReserveFrom = Prolink.Math.GetValueAsInt(item["RESERVE_FROM"]);
                    Hour = Prolink.Math.GetValueAsInt(item["RESERVE_HOUR"]);

                    TempWscd = Prolink.Math.GetValueAsString(item["TEMP_WSCD"]);
                    TempGateno = Prolink.Math.GetValueAsString(item["TEMP_GATENO"]);
                    TempRdate = ((DateTime)item["TEMP_RDATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    TempRfrom = Prolink.Math.GetValueAsInt(item["TEMP_RFROM"]);
                    TempRh = Prolink.Math.GetValueAsInt(item["TEMP_RH"]);
                    
                    if ((TempGateno == "" || GateNo == TempGateno) && (TempWscd == "" || WsCd == TempWscd) && (TempRdate == "" || ReserveDate == TempRdate) && (ReserveFrom == TempRfrom) && (Hour == TempRh))
                    {
                        chk = true;
                        break;
                    }
                    else
                    {
                        if (GateNo != TempGateno || WsCd != TempWscd || ReserveDate != TempRdate || ReserveFrom != TempRfrom || Hour != TempRh)
                        {
                            updateData = UpdateRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour, UId);

                            if (updateData == true)
                            {
                                chk = InsertRVDRecode(Cmp, TempWscd, TempGateno, TempRdate, TempRfrom, TempRh, UId);
                            }
                            else
                            {
                                chk = false;
                            }
                        }
                    }
                    //chk = InsertRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour, UId);
                }
            }

            if (chk == false)
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_145;
            }

            if (chk == true)
            {
                //sql = "UPDATE SMRV SET STATUS='C', GATE_NO=" + SQLUtils.QuotedStr(TempGateno) + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                MixedList ml = new MixedList();
                EditInstruct ei;
                int[] resulst;
                ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", UId);
                ei.Put("WS_CD", TempWscd);
                ei.Put("GATE_NO", TempGateno);
                ei.PutDate("RESERVE_DATE", TempRdate);
                ei.Put("RESERVE_FROM", TempRfrom);
                ei.Put("RESERVE_HOUR", TempRh);
                ei.Put("CONFIRM_BY", UserId);
                DateTime odt = DateTime.Now;                
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("CONFIRM_DATE", odt);
                ei.PutDate("CONFIRM_DATE_L", ndt);
                ei.Put("STATUS", "C");
                ml.Add(ei);
                try
                {
                    //Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    chk = true;
                    chk = Business.ReserveManage.syncBatNo(UId);
                }
                catch (Exception ex)
                {
                    chk = false;
                    returnMessage = ex.Message;
                }
            }

            return Json(new { message = returnMessage });
        }

        /*修改確認*/
        public ActionResult ModifyReverseGate()
        { 
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = false, updateData=false;
            //string TempGateno = "";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string TempWscd = "", TempGateno = "", TempRdate = "";
            string WsCd = "", GateNo = "", ReserveDate = "";
            int TempRfrom = 0, TempRh = 0;
            int ReserveFrom = 0, Hour = 0;

            if (dt.Rows.Count > 0)
            { 
                foreach(DataRow item in dt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    TempWscd = Prolink.Math.GetValueAsString(item["TEMP_WSCD"]);
                    TempGateno = Prolink.Math.GetValueAsString(item["TEMP_GATENO"]);
                    TempRdate = ((DateTime)item["TEMP_RDATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    TempRfrom = Prolink.Math.GetValueAsInt(item["TEMP_RFROM"]);
                    TempRh = Prolink.Math.GetValueAsInt(item["TEMP_RH"]);

                    WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    ReserveDate = ((DateTime)item["RESERVE_DATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    ReserveFrom = Prolink.Math.GetValueAsInt(item["RESERVE_FROM"]);
                    Hour = Prolink.Math.GetValueAsInt(item["RESERVE_HOUR"]);

                    updateData = UpdateRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour, UId);

                    if (updateData == true)
                    {
                        chk = InsertRVDRecode(Cmp, TempWscd, TempGateno, TempRdate, TempRfrom, TempRh, UId);
                    }
                    else
                    {
                        chk = false;
                    }
                }
            }

            if (chk == false)
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_145;
            }

            if (chk == true)
            {
                //sql = "UPDATE SMRV SET STATUS='C', GATE_NO=" + SQLUtils.QuotedStr(TempGateno) + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                MixedList ml = new MixedList();
                EditInstruct ei;
                int[] resulst;
                ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", UId);
                ei.Put("WS_CD", TempWscd);
                ei.Put("GATE_NO", TempGateno);
                ei.PutDate("RESERVE_DATE", TempRdate);
                ei.Put("RESERVE_FROM", TempRfrom);
                ei.Put("RESERVE_HOUR", TempRh);
                ei.Put("CONFIRM_BY", UserId);
                DateTime odt = DateTime.Now;                
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("CONFIRM_DATE", odt);
                ei.PutDate("CONFIRM_DATE_L", ndt);
                ei.Put("STATUS", "C");
                ml.Add(ei);
                try
                {
                    //Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    chk = true;
                    chk = Business.ReserveManage.syncBatNo(UId);
                }
                catch (Exception ex)
                {
                    chk = false;
                    returnMessage = ex.Message;
                }
            }

            return Json(new { message = returnMessage });
        }

        [ValidateInput(false)]
        public ActionResult SaveWhData()
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmwhModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            ei.Put("GROUP_ID", GroupId);
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
                            EditInstruct ei2 = new EditInstruct("SMWHGT", EditInstruct.DELETE_OPERATION);
                            string UId = Prolink.Math.GetValueAsString(ei.Get("U_ID"));

                            if (UId == "")
                            {
                                continue;
                            }
                            ei2.PutKey("U_FID", UId);
                            mixList.Add(ei2);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmwhgtModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION) 
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
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
            return Json(new { message = returnMessage });
        }

        public ActionResult GetGateData()
        {
            string sql = "SELECT TOP 1 IO_FLAG FROM SYS_ACCT WHERE U_ID=" + SQLUtils.QuotedStr(UserId);
            string IoFlag = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string condition = "";

            if (IoFlag == "O")
            {
                condition = "GROUP_ID="+SQLUtils.QuotedStr(GroupId)+" AND TRUCKER=" + SQLUtils.QuotedStr(CompanyId);
            }
            else
            {
                condition = GetBookingCondition();
            }
            condition += " AND (RV_TYPE<>'I' OR RV_TYPE IS NULL)";
            string table = @"(SELECT SMRV.*,(SELECT TOP 1 ETD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID)AS ETD,
                (SELECT TOP 1 FC_CD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID)AS FC_CD,
                (SELECT TOP 1 FC_NM FROM SMSM WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID)AS FC_NM,
                (SELECT TOP 1 RELEASE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID)AS RELEASE_NO FROM SMRV) SMRV ";
            return GetBootstrapData(table, condition);
        }

        public ActionResult GetWareHouseData()
        {
            string condition = " CMP=" + SQLUtils.QuotedStr(CompanyId);
            return GetBootstrapData("SMWH", condition);
        }

        public ActionResult __getTodayWaitTrucker() 
        {
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            int GateNo = Prolink.Math.GetValueAsInt(Request.Params["GateNo"]);
            string Today = Prolink.Math.GetValueAsString(Request.Params["Today"]);

            string condition = " WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + GateNo + " AND RESERVE_DATE=" + SQLUtils.QuotedStr(Today);
            return GetBaseData("SMRV", condition, "*", " RESERVE_NO ASC", "2");
        }

        /*取得今日入廠車輛by gate no*/
        public ActionResult getTodayWaitTrucker() 
        {
            string Today = Prolink.Math.GetValueAsString(Request.Params["Today"]);

            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["Shipmentid"]);
            string dnno = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);

            string condition = " STATUS='I'" + " AND " + GetBaseCmp();
            if (!string.IsNullOrEmpty(shipmentid))
            {
                condition += string.Format(" AND (SHIPMENT_ID LIKE '%{0}%' OR RESERVE_NO LIKE '%{0}%')", shipmentid);
            }
            if (!string.IsNullOrEmpty(dnno))
            {
                condition += string.Format(" AND DN_NO LIKE '%{0}%'", dnno);
            }
            condition += " AND (RV_TYPE<>'I' OR RV_TYPE IS NULL)";
            return GetBaseData("SMRV", condition, "*", " RESERVE_NO ASC", "2",200);
        }

        [ValidateInput(false)]
        public ActionResult GetWhGateData()
        {
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string condition = " WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            //return GetBootstrapData("SMWHGT", condition);
            return GetBaseData("SMWHGT", condition, "*", "GATE_NO", "2");
        }

        public ActionResult GetGateItem()//GetDRuleItem
        {
            string u_id = Request["UId"];
            string sql = string.Empty;
            sql = string.Format("SELECT * FROM SMRV WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string s_uid = "", gp_id = "", cmp = "", BatNo = string.Empty;
            List<string> shipmentid_array = new List<string>();
            List<string> smuid_array = new List<string>();

            string InUrl = "";
            string OutUrl = "";
            string GW = string.Empty;
            string GWU = string.Empty;
            string CBM = string.Empty;
            string TCBM = string.Empty;
            string FTsql = string.Format("SELECT * FROM  SMCUFT WHERE U_FID IN (SELECT SMDN.U_ID FROM SMDN ,SMRV WHERE SMDN.SHIPMENT_ID=SMRV.SHIPMENT_ID AND SMRV.U_ID='{0}')", u_id);
            DataTable FTsdt = OperationUtils.GetDataTable(FTsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (groupDt.Rows.Count > 0)
            {
                BatNo = Prolink.Math.GetValueAsString(groupDt.Rows[0]["BAT_NO"]);
                string _sm_id = Prolink.Math.GetValueAsString(groupDt.Rows[0]["SHIPMENT_ID"]);
                sql = "SELECT B.U_ID,B.SHIPMENT_ID,B.GROUP_ID,B.CMP FROM SMRV A, SMSM B WHERE A.SHIPMENT_ID=B.SHIPMENT_ID AND A.BAT_NO=" + SQLUtils.QuotedStr(BatNo);
                DataTable bDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                data["SmData"] = ModelFactory.ToTableJson(bDt, "SmsmModel");
                sql = "SELECT U_ID, GROUP_ID, CMP,GW,GWU,CBM,TCBM FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(_sm_id);
                DataTable _sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (_sdt.Rows.Count > 0)
                {
                    foreach (DataRow item1 in _sdt.Rows) {
                        GW = Prolink.Math.GetValueAsString(item1["GW"]);
                        GWU = Prolink.Math.GetValueAsString(item1["GWU"]);
                        CBM = Prolink.Math.GetValueAsString(item1["CBM"]);
                        TCBM = Prolink.Math.GetValueAsString(item1["TCBM"]);
                    }
                }
                foreach (DataRow item in bDt.Rows)
                {
                    string sm_id = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                    sql = "SELECT U_ID, GROUP_ID, CMP,GW,GWU,CBM,TCBM FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (sdt.Rows.Count > 0)
                    {
                        foreach (DataRow item1 in sdt.Rows)
                        {
                            s_uid = Prolink.Math.GetValueAsString(item1["U_ID"]);
                            gp_id = Prolink.Math.GetValueAsString(item1["GROUP_ID"]);
                            cmp = Prolink.Math.GetValueAsString(item1["CMP"]);

                            #region 獲取照片
                            try
                            {
                                List<Dictionary<string, string>> list = MailTemplate.FileQueryDownlodInfo(s_uid, gp_id, cmp, "*", "", "SECPIC");
                                string urls = string.Empty;
                                if (list.Count > 0)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        string url = list[i]["FileUrl"];
                                        string Remark = list[i]["Remark"];
                                        string[] str = Remark.Split('-');

                                        if (str.Length >= 2)
                                        {
                                            if (str[1] == BatNo && str[0] == "IN")
                                            {
                                                InUrl = url;
                                            }
                                            else if (str[0] == "OUT" && str[1] == BatNo)
                                            {
                                                OutUrl = url;
                                            }
                                        }
                                        else
                                        {
                                            //king 20160705 增加相容舊的模式
                                            if (str[0] == "IN" && InUrl == "")
                                            {
                                                InUrl = url;
                                            }
                                            else if (str[0] == "OUT" && OutUrl == "")
                                            {
                                                OutUrl = url;
                                            }
                                        }


                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string s = ex.ToString();
                            }
                            #endregion
                        }
                    }

                }
            }
            string dnsql = string.Format("SELECT DN_NO,SEAL_SATAUE FROM SMDN WHERE SHIPMENT_ID IN( SELECT SHIPMENT_ID FROM SMRV WHERE BAT_NO in(select BAT_NO from SMRV WHERE U_ID={0}))", SQLUtils.QuotedStr(u_id));
            DataTable dndt = OperationUtils.GetDataTable(dnsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            data["main"] = ModelFactory.ToTableJson(groupDt, "SmrvModel");
            data["DnData"] = ModelFactory.ToTableJson(dndt);
            data["InImg"] = InUrl;
            data["OutImg"] = OutUrl;
            data["GW"] = GW;
            data["GWU"] = GWU;
            data["CBM"] = CBM;
            data["TCBM"] = TCBM;
            data["Scuft"] = ModelFactory.ToTableJson(FTsdt, "SmcuftModel");
            return ToContent(data);

        }

        public ActionResult GetWareHouseByCmp()
        {
            string cmp = Prolink.Math.GetValueAsString(Request["cmp"]);
            string condition = " CMP=" + SQLUtils.QuotedStr(cmp);
            return GetBootstrapData("SMWH", condition);
        }

        public JsonResult GetRVD()
        {
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["SearchCmp"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["SearchWsCd"]);
            string ReserveDate = Prolink.Math.GetValueAsString(Request.Params["SearchRDate"]);
            string returnMsg = "success";

            string sql = @"SELECT SMWHGT.CMP, 
                                                SMWHGT.WS_CD, 
                                                SMWHGT.GATE_NO,
                                                SMWHGT.LIFT,
                                                SMRVD.H_0, 
                                                SMRVD.H_1, 
                                                SMRVD.H_2, 
                                                SMRVD.H_3, 
                                                SMRVD.H_4, 
                                                SMRVD.H_5, 
                                                SMRVD.H_6,
                                                SMRVD.H_7, 
                                                SMRVD.H_8, 
                                                SMRVD.H_9, 
                                                SMRVD.H_10, 
                                                SMRVD.H_11, 
                                                SMRVD.H_12, 
                                                SMRVD.H_13,
                                                SMRVD.H_14,
                                                SMRVD.H_15, 
                                                SMRVD.H_16, 
                                                SMRVD.H_17, 
                                                SMRVD.H_18, 
                                                SMRVD.H_19, 
                                                SMRVD.H_20, 
                                                SMRVD.H_21,
                                                SMRVD.H_22, 
                                                SMRVD.H_23,
                                                SMRVD.C_0, 
                                                SMRVD.C_1, 
                                                SMRVD.C_2, 
                                                SMRVD.C_3, 
                                                SMRVD.C_4, 
                                                SMRVD.C_5, 
                                                SMRVD.C_6,
                                                SMRVD.C_7, 
                                                SMRVD.C_8, 
                                                SMRVD.C_9, 
                                                SMRVD.C_10, 
                                                SMRVD.C_11, 
                                                SMRVD.C_12, 
                                                SMRVD.C_13,
                                                SMRVD.C_14,
                                                SMRVD.C_15, 
                                                SMRVD.C_16, 
                                                SMRVD.C_17, 
                                                SMRVD.C_18, 
                                                SMRVD.C_19, 
                                                SMRVD.C_20, 
                                                SMRVD.C_21,
                                                SMRVD.C_22,
                                                SMRVD.C_23
                                  FROM   SMWHGT 
                                  LEFT JOIN SMRVD  ON SMRVD.WS_CD = SMWHGT.WS_CD  AND SMRVD.GATE_NO = SMWHGT.GATE_NO AND SMRVD.RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate)
                                  + "WHERE  SMWHGT.CMP = " + SQLUtils.QuotedStr(Cmp) + "  AND SMWHGT.WS_CD = " + SQLUtils.QuotedStr(WsCd) + " ORDER BY GATE_NO ASC";

            List<Dictionary<string, object>> returnData = new List<Dictionary<string, object>>();
            try
            {
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnData = ModelFactory.ToTableJson(dt);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }

            return Json(new { message = returnMsg, data = returnData });
        }

        /*寫入預約明細到SMRVD*/
        private Boolean InsertRVDRecode(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour, string UId)
        {
            string sql = "";
            bool success = false;
            string con = "CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo) + " AND RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate);
            sql = "SELECT COUNT(*) FROM SMRVD WHERE " + con;
            string comma = ",";
            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dt = new DataTable();
            if (num > 0)
            {
                string sql1 = "SELECT TOP 1 * FROM SMRVD WHERE " + con;
                dt = OperationUtils.GetDataTable(sql1, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        int h = Prolink.Math.GetValueAsInt(item["H_" + Hour.ToString()]);

                        if (h == 1)
                        {
                            return false;
                        }
                        else
                        {
                            bool tag = true;

                            /*從預約的時間往後加，檢查位子是都沒人預約*/
                            for (int i = 1; i <= Hour; i++)
                            {
                                int after = ReserveFrom + 1;
                                h = Prolink.Math.GetValueAsInt(item["H_" + after.ToString()]);
                                if (h == 1)
                                { 
                                    tag = false;
                                    break;
                                }
                            }
                                
                            if(tag == true)
                            {
                                string str_h = "", str_c = "";
                                for (int i = 0; i < Hour; i++)
                                {
                                    if (ReserveFrom + i > 23)
                                    {
                                        break; //超過23要換天
                                    }
                                    if (i + 1 == Hour)
                                    {
                                        str_h += " H_" + (ReserveFrom + i).ToString() + "=1 ";
                                        str_c += " C_" + (ReserveFrom + i).ToString() + "=" + SQLUtils.QuotedStr(CompanyId);
                                    }
                                    else
                                    {
                                        if (ReserveFrom + i == 23)
                                        {
                                            comma = "";
                                        }
                                        str_h += " H_" + (ReserveFrom + i).ToString() + "=1" + comma;
                                        str_c += " C_" + (ReserveFrom + i).ToString() + "=" + SQLUtils.QuotedStr(CompanyId) + comma;
                                    }
                                }
                                sql = @"UPDATE SMRVD SET " + str_h +","+ str_c  +" WHERE "+ con;
                            }
                            else
                            {
                                return false;
                            }

                        }
                    }
                }
            }
            else
            {
                string str_h = "", str_c = "", str_hv = "", str_cv = "";
                for (int i = 0; i < Hour; i++)
                {
                    if (ReserveFrom + i > 23)
                    {
                        break;//超過23要換天
                    }
                    if (i + 1 == Hour)
                    {
                        str_h += " H_" + (ReserveFrom + i).ToString() + " ";
                        str_c += " C_" + (ReserveFrom + i).ToString() + " ";
                        str_hv += " 1 ";
                        str_cv += SQLUtils.QuotedStr(CompanyId) + " ";
                    }
                    else
                    {
                        if (ReserveFrom + i == 23)
                        {
                            comma = "";
                        }
                        str_h += " H_" + (ReserveFrom + i).ToString() + comma;
                        str_c += " C_" + (ReserveFrom + i).ToString() + comma;
                        str_hv += " 1" + comma;
                        str_cv += SQLUtils.QuotedStr(CompanyId) + comma;
                    }
                }
                sql = "INSERT INTO SMRVD(CMP, WS_CD, GATE_NO, RESERVE_DATE, " + str_h +","+ str_c + ") VALUES(" + SQLUtils.QuotedStr(Cmp) + "," + SQLUtils.QuotedStr(WsCd) + "," + SQLUtils.QuotedStr(GateNo) + "," + SQLUtils.QuotedStr(ReserveDate) + "," + str_hv +","+ str_cv + ")";
            }

            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }

        /*修改預約明細到SMRVD*/
        private Boolean UpdateRVDRecode(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour, string UId)
        {
            bool success = false;
            string str_h = "", str_c = "";
            string comma = ",";
            string con = "CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo) + " AND RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate);
            for (int i = 0; i < Hour; i++)
            {
                if (ReserveFrom + i > 23)
                {
                    break;//超過23要換天
                }
                if (i + 1 == Hour)
                {
                    str_h += " H_" + (ReserveFrom + i).ToString() + "=null ";
                    str_c += " C_" + (ReserveFrom + i).ToString() + "=null";
                }
                else
                {
                    if (ReserveFrom + i == 23)
                    {
                        comma = "";
                    }
                    str_h += " H_" + (ReserveFrom + i).ToString() + "=null" + comma;
                    str_c += " C_" + (ReserveFrom + i).ToString() + "=null" + comma;
                }
            }
            string sql = "UPDATE  SMRVD SET " + str_h + "," + str_c + " WHERE " + con;

            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }

        /*取得貨櫃管理Grid Data*/
        public ActionResult GetSMDNPData()
        {
            //APPROVE_TO > 'D' AND (STATUS = 'A' OR STATUS='B' OR STATUS='C' OR STATUS='D' OR STATUS='E')
            //string condition = "  (STATUS = 'A' OR STATUS='B' OR STATUS='C' OR STATUS='D' OR STATUS='E')";
            string condition = GetBaseCmp() + " AND (TORDER = 'S' OR TORDER='C') ";
            if (PlantPri != "''")
            {
                if (string.IsNullOrEmpty(PlantPri))
                {
                    condition += " AND PLANT in ( '' ) ";
                }
                else
                {
                    condition += string.Format(" AND PLANT in ( {0} ) ", PlantPri);
                }
            }

            return GetBootstrapData("V_CMSUMMARY", condition, null, "GetSMDNPData");
        }

        /*取得未下載SMDNP*/
        public ActionResult GetNonDownloadSMDNPData()
        {
            //APPROVE_TO > 'D' AND (STATUS = 'A' OR STATUS='B' OR STATUS='C' OR STATUS='D' OR STATUS='E')
            string condition = GetBaseCmp() + " AND (DOWN_FLAG='N' OR DOWN_FLAG IS NULL) AND TORDER='S'";

            if ((UPri != "G" && UPri != "U") && TCmp != "")
            {
                condition += string.Format(" AND STN in ( {0} ) ", TCmp);
            }
            return GetBootstrapData("V_CMSUMMARY", condition);
        }

        /*叫車檢查*/
        public string ChkTruck(string [] myArray)
        {
            string msg="ok";
            string DnpUid = "";
            string DnNo = "";
            for (int i = 0; i < myArray.Count(); i++)
            {
                DnpUid = myArray[i];
                string sql = @"SELECT COUNT(*) FROM SMSMPT INNER JOIN SMSM ON SMSM.U_ID=SMSMPT.U_FID INNER JOIN SMDN ON SMDN.SHIPMENT_ID=SMSM.SHIPMENT_ID INNER JOIN SMDNP ON SMDN.U_ID=SMDNP.U_FID WHERE SMDNP.U_ID=" + SQLUtils.QuotedStr(DnpUid) + " AND SMSMPT.PARTY_TYPE='PT'";
                int n = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (n == 0)
                {
                    sql = @"SELECT DN_NO FROM SMDNP WHERE U_ID=" + SQLUtils.QuotedStr(DnpUid);
                    DnNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    return "DN NO: " + DnNo +  @Resources.Locale.L_GateManageController_Controllers_146;
                }
            }

            return msg;
        }

        /*叫車新方法，目前没有用*/
        public JsonResult NewOrderTruck()
        {
            string returnMessage = "success";
            List<string> idList = new List<string>();
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DN_NO"]);

            if (DnNo != "")
            {
                string sql = "SELECT * FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        string ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                        string g = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                        string c = Prolink.Math.GetValueAsString(item["CMP"]);
                        string d = Prolink.Math.GetValueAsString(item["DEP"]);
                        string exsit = Business.ReserveManage.ChkSmrv(ShipmentId);
                        if (exsit != "success")
                        {
                            //問user要不要重新叫車，如果重新叫車，會把原來的預約記錄刪掉再新增
                        }

                        string result = Business.ReserveManage.OrderTrucker(ShipmentId, g, c, d, Ext, UserId,idList, 0);

                        if (result != "success")
                        {
                            returnMessage = "Shipment ID: " + ShipmentId + "," + result;
                        }
                    }
                }
            }
            else
            {
                returnMessage = "DN NO "+@Resources.Locale.L_ActManage_Controllers_30;    
            }

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");

            return Json(new { message = returnMessage });
        }
        public JsonResult ShipmentOrderTruckN()
        {
            List<string> idList = new List<string>();
            //string UidArray = Prolink.Math.GetValueAsString(Request.Params["UidArray"]);
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            int CntNumber = Prolink.Math.GetValueAsInt(Request.Params["CntNumber"]);
            string useDatetime = Prolink.Math.GetValueAsString(Request.Params["useDatetime"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string CntType = Prolink.Math.GetValueAsString(Request.Params["CntType"]);
            string[] myArray = uid.Split(';');
            string msg = string.Empty;
            string ShipmentId = string.Empty;

            string etd = string.Format("SELECT DISTINCT  ETD FROM SMSM WHERE U_ID IN {0} ", SQLUtils.Quoted(myArray));
            DataTable etddt = OperationUtils.GetDataTable(etd, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (etddt != null && etddt.Rows.Count >= 2)
            {
                return Json(new { message = @Resources.Locale.L_GateManage_edisome });
            }

            foreach (var Uid in myArray)
            {

                string returnMessage = string.Empty;
                string sql = "";
                sql = @"SELECT TOP 1 * FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(Uid);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                        returnMessage = SMHandle.QAHoldBlMessage(ShipmentId);
                        if (!string.IsNullOrEmpty(returnMessage))
                            return Json(new { message = returnMessage });
                        string DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                        string Torder = Prolink.Math.GetValueAsString(item["TORDER"]);
                        string g = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                        string c = Prolink.Math.GetValueAsString(item["CMP"]);
                        string d = Prolink.Math.GetValueAsString(item["DEP"]);
                        string frtterm = Prolink.Math.GetValueAsString(item["FRT_TERM"]);
                        string exsit = Business.ReserveManage.ChkSmrv(ShipmentId);
                        string trantype = Prolink.Math.GetValueAsString(item["TRAN_TYPE"]);

                        if (Torder == "N" || Torder == "")
                        {
                            returnMessage = @Resources.Locale.L_GateManageController_Controllers_147;
                            return Json(new { message = returnMessage });
                        }


                        if (exsit != "success")
                        {
                            //問user要不要重新叫車，如果重新叫車，會把原來的預約記錄刪掉再新增
                            //叫车管理画面，针对到付货物(Feight_term=C )允许仓库增加叫车
                            if ("F".Equals(trantype))
                            {
                                returnMessage = @Resources.Locale.L_GateManageController_Controllers_148;
                                return Json(new { message = returnMessage });
                            }
                        }

                        string result = Business.ReserveManage.OrderTrucker(ShipmentId, g, c, d, Ext, UserId, idList,0, CntNumber, useDatetime, "N", WsCd, null, CntType);

                        if (result != "success")
                        {
                            if (returnMessage == "success")
                            {
                                returnMessage = "";
                            }
                            returnMessage += "Shipment ID: " + ShipmentId + "," + result + "\n";
                        }
                        else
                        {
                            sql = "SELECT COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            string combine_info = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                            string updateMsg = "success";
                            if (dns.Length > 0)
                            {
                                for (int i = 0; i < dns.Length; i++)
                                {
                                    updateMsg = Business.DNApproveLoop.UpdateApproveRecord(dns[i], CompanyId, "FPW", UserId, GetBaseCmp());
                                }

                            }

                            if (updateMsg != "success")
                            {
                                returnMessage = @Resources.Locale.L_GateManage_Controllers_343;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(returnMessage))
                    continue;
                msg += ShipmentId+":"+returnMessage + "\n";
            }

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            return Json(new { message = msg });
        }
        public JsonResult ShipmentOrderTruck()
        {
            List<string> idList = new List<string>();
            string returnMessage = "success";
            //string UidArray = Prolink.Math.GetValueAsString(Request.Params["UidArray"]);
            string Uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            int CntNumber = Prolink.Math.GetValueAsInt(Request.Params["CntNumber"]);
            string useDatetime = Prolink.Math.GetValueAsString(Request.Params["useDatetime"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string CntType = Prolink.Math.GetValueAsString(Request.Params["CntType"]);
            //Uid string[] myArray = UidArray.Split(',');
            string sql = "";
            sql = @"SELECT TOP 1 * FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(Uid);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                    returnMessage = SMHandle.QAHoldBlMessage(ShipmentId);
                    if (!string.IsNullOrEmpty(returnMessage))
                        return Json(new { message = returnMessage });
                    string DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                    string Torder = Prolink.Math.GetValueAsString(item["TORDER"]);
                    string g = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                    string c = Prolink.Math.GetValueAsString(item["CMP"]);
                    string d = Prolink.Math.GetValueAsString(item["DEP"]);
                    string frtterm = Prolink.Math.GetValueAsString(item["FRT_TERM"]);
                    string exsit = Business.ReserveManage.ChkSmrv(ShipmentId);
                    string trantype= Prolink.Math.GetValueAsString(item["TRAN_TYPE"]);

                    if (Torder == "N" || Torder == "")
                    {
                        returnMessage = @Resources.Locale.L_GateManageController_Controllers_147;
                        return Json(new { message = returnMessage });
                    }


                    if (exsit != "success")
                    {
                        //問user要不要重新叫車，如果重新叫車，會把原來的預約記錄刪掉再新增
                        //叫车管理画面，针对到付货物(Feight_term=C )允许仓库增加叫车
                        if ("F".Equals(trantype))
                        {
                            returnMessage = @Resources.Locale.L_GateManageController_Controllers_148;
                            return Json(new { message = returnMessage });
                        }
                    }

                    string result = Business.ReserveManage.OrderTrucker(ShipmentId, g, c, d, Ext, UserId,idList, 0, CntNumber, useDatetime, "N", WsCd,null,CntType);

                    if (result != "success")
                    {
                        if (returnMessage == "success")
                        {
                            returnMessage = "";
                        }
                        returnMessage += "Shipment ID: " + ShipmentId + "," + result + "\n";
                    }
                    else
                    {
                        sql = "SELECT COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        string combine_info = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string[] dns = combine_info.Split(new string[] { ";","," }, StringSplitOptions.RemoveEmptyEntries);
                        string updateMsg = "success";
                        if (dns.Length > 0)
                        {
                            for (int i = 0; i < dns.Length; i++)
                            {
                                updateMsg = Business.DNApproveLoop.UpdateApproveRecord(dns[i], CompanyId, "FPW", UserId, GetBaseCmp());
                            }
                                
                        }
                        
                        if (updateMsg != "success")
                        {
                            returnMessage = @Resources.Locale.L_GateManage_Controllers_343;
                        }
                    }
                }
            }
            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            return Json(new { message = returnMessage });
        }

        /*取消叫车*/
        public JsonResult CancelOrderTruck()
        {
            string returnMessage = "success";
            string Uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);

            string result = Business.ReserveManage.CancelTrucker(Uid,UserId);

            if (result != "success")
            {
                returnMessage = result;
            }
            return Json(new { message = returnMessage });
        }

        /*叫車*/
        public JsonResult OrderTruck()
        {
            string returnMessage = "success";
            string DnpUidArray = Prolink.Math.GetValueAsString(Request.Params["DnpUidArray"]);
            string[] myArray = DnpUidArray.Split(',');
            string DnpUid = "", sql = "";
            MixedList mixList = new MixedList();
            DataTable dt = new DataTable();
            string chk = ChkTruck(myArray);

            if (chk != "ok")
            {
                return Json(new { message = chk });
            }

            for (int i = 0; i < myArray.Count(); i++)
            {
                DnpUid = myArray[i];

                sql = @"SELECT SMDN.GROUP_ID, SMDN.CMP, SMDN.DN_NO, SMWH.WS_CD, SMDNP.PRODUCT_LINE, SHIPMENT_INFO, REF_GATE, SMDN.SHIPMENT_ID 
                             FROM SMDN
                             INNER JOIN SMDNP ON SMDN.U_ID=SMDNP.U_FID 
                             INNER JOIN SMSM ON SMDN.SHIPMENT_ID=SMSM.SHIPMENT_ID  
                             INNER JOIN SMWH ON SMDNP.PRODUCT_LINE = SMWH.PRODUCT_LINE 
                             WHERE  SMDNP.U_ID=" + SQLUtils.QuotedStr(DnpUid);
                dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dt.Rows.Count > 0)
                {
                    string UId = "";
                    foreach (DataRow item in dt.Rows)
                    {
                        bool success = false;
                        string ruleCode = "SMRV_NO";
                        string group = GroupId;//Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                        string cmp = CompanyId;//Prolink.Math.GetValueAsString(item["CMP"]);
                        string ext = Ext;
                        string dep = Dep;
                        //string stn = Prolink.Math.GetValueAsString(item["STN"]);
                        string DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                        System.Collections.Hashtable hash = new System.Collections.Hashtable();
                        hash.Add(cmp, ruleCode);
                        string ReverseNo = AutoNo.GetNo(ruleCode, hash, group, cmp, Station);
                        string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                        string ProductLine = Prolink.Math.GetValueAsString(item["PRODUCT_LINE"]);
                        string ShipmentInfo = Prolink.Math.GetValueAsString(item["SHIPMENT_INFO"]);
                        string RefGate = Prolink.Math.GetValueAsString(item["REF_GATE"]);
                        string ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                        UId = System.Guid.NewGuid().ToString();

                        sql = "SELECT PARTY_NO FROM SMSM, SMSMPT WHERE SMSM.U_ID=SMSMPT.U_FID AND SMSMPT.PARTY_TYPE='PT' AND SMSM.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        string Trucker = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        DateTime odt = DateTime.Now;                       
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        sql = @"INSERT INTO SMRV (U_ID, RESERVE_NO, STATUS, GROUP_ID, CMP, WS_CD, DN_NO, PRODUCT_LINE, SHIPMENT_INFO, DEP, CREATE_BY, CREATE_DATE, CREATE_DATE_L, CREATE_CMP, CREATE_DEP, CREATE_EXT, CALL_DATE, CALL_DATE_L, REF_GATE, TRUCKER) VALUES 
                                   (" + SQLUtils.QuotedStr(UId) + "," + SQLUtils.QuotedStr(ReverseNo) + ",'D'," + SQLUtils.QuotedStr(group) + "," + SQLUtils.QuotedStr(cmp) + "," + SQLUtils.QuotedStr(WsCd) + "," + SQLUtils.QuotedStr(DnNo) + "," + SQLUtils.QuotedStr(ProductLine) + "," + SQLUtils.QuotedStr(ShipmentInfo) + "," + SQLUtils.QuotedStr(dep) + "," + SQLUtils.QuotedStr(UserId) + "," + SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm")) + "," + SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm")) + "," + SQLUtils.QuotedStr(cmp) + "," + SQLUtils.QuotedStr(dep) + "," + SQLUtils.QuotedStr(ext) + "," + SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm")) + "," + SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm")) + "," + SQLUtils.QuotedStr(RefGate) + "," + SQLUtils.QuotedStr(Trucker) + ")";
                        //mixList.Add(sql);

                        try
                        {
                            //OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            returnMessage = @Resources.Locale.L_GateManageController_Controllers_149;
                            break;
                        }

                        if (success == true)
                        {
                            EvenFactory.AddEven(UId + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), UId, MailManager.RVNotify, null, 1, 0, "tim@pllink.com", @Resources.Locale.L_DNManage_CallCar, "");
                        }
                    }
                }
                else
                {
                    returnMessage = @Resources.Locale.L_GateManageController_Controllers_150;
                }
            }

            return Json(new { message = returnMessage });
        }

        /*取得倉庫月台所有預約狀況*/
        public JsonResult getWhGateReserve()
        {
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string ReserveDate = Prolink.Math.GetValueAsString(Request.Params["ReserveDate"]);
            string sql = "SELECT WS_CD FROM SMWH WHERE GROUP_ID='TPV' AND CMP=" + SQLUtils.QuotedStr(Cmp);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            smwhTable p = new smwhTable();
            
            List<object> returnData = new List<object>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    sql = "SELECT * FROM SMWHGT WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " ORDER BY GATE_NO ASC";
                    DataTable dt1 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    whItem g = new whItem();
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow item1 in dt1.Rows)
                        {
                            string GateNo = Prolink.Math.GetValueAsString(item1["GATE_NO"]);
                            string CntrNo = Prolink.Math.GetValueAsString(item1["CNTR_NO"]);
                            string gStatus = Prolink.Math.GetValueAsString(item1["STATUS"]);
                            string ReserveNo = Prolink.Math.GetValueAsString(item1["RESERVE_NO"]);
                            string status = "0";
                            sql = "SELECT COUNT(*) FROM SMRVD WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo) + " AND RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate);
                            int n = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (CntrNo != "")
                            {
                                status = "1";
                            }
                            else if (n > 0)
                            {
                                status = "2";
                            }
                            else if (gStatus != "Y")
                            {
                                status = "0";
                            }
                            else
                            {
                                status = "3";
                            }

                            g.Gates.Add(new gtItem
                            {
                                GateNo = GateNo,
                                CntrNo = CntrNo,
                                GateStatus = status,
                                ReserveNo = ReserveNo
                            });
                            
                        }
                    }

                    p.wh.Add(new whItem
                    {
                        WsCd = WsCd,
                        Gates = g.Gates
                    });
                    
                }
            }

            return Json(new { returnData = p });
        }

        
        /*預約修改*/
        public ActionResult modifySmrvConfirm()
        {
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = false, updateData=false;
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            { 
                foreach(DataRow item in dt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    string GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    string ReserveDate = ((DateTime)item["RESERVE_DATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    int ReserveFrom = Prolink.Math.GetValueAsInt(item["RESERVE_FROM"]);
                    int Hour = Prolink.Math.GetValueAsInt(item["RESERVE_HOUR"]);

                    updateData = UpdateRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour, UId);

                    if (updateData == true)
                    {
                        chk = InsertRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour, UId);
                    }
                    else
                    {
                        chk = false;
                    }
                }
            }

            return Json(new { message = returnMessage });
        }

        /*測試MAIL*/
        public ActionResult testMail()
        {
            string returnMessage = "success";
            TrackingEDI.Business.EvenFactory.RegisterSendMail("IB_BILL", MailManager.SendInboundBillResult);
            EvenFactory.ExecuteMailEven("BILL");
            return Json(new { message = returnMessage });
        }

        /*測試MAIL*/
        public ActionResult testUserMail()
        {
            string returnMessage = "success";
            TrackingEDI.Business.EvenFactory.RegisterSendMail("UInfo", MailManager.SendUserInfoMail);
            EvenFactory.ExecuteMailEven("UInfo");
            return Json(new { message = returnMessage });
        }

        /*調撥叫車*/
        public ActionResult smdndOrderCar()
        {
            List<string> idList = new List<string>();
            string id = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string dnuid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            string returnMessage = "success";
            TrackingEDI.Business.TransferBooking tb = new TransferBooking();

            string ShipmentId = tb.SmdndToBooking(id);
            string sql = "SELECT TOP 1 U_ID FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            string Suid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (ShipmentId == "")
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_151;
            }
            else
            {
                sql = @"SELECT TOP 1 * FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(Suid);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        //ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                        string g = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                        string c = Prolink.Math.GetValueAsString(item["CMP"]);
                        string d = Prolink.Math.GetValueAsString(item["DEP"]);
                        string exsit = Business.ReserveManage.ChkSmrv(ShipmentId);

                        sql = "SELECT TOP 1 CALL_DATE FROM SMDND WHERE U_ID=" + SQLUtils.QuotedStr(id);
                        //string useDatetime = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        DataTable dt1 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string useDatetime = "";
                        if (dt1.Rows.Count > 0)
                        {
                            foreach (DataRow item1 in dt1.Rows)
                            {
                                useDatetime = ((DateTime)item1["CALL_DATE"]).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            }
                        }
                        else
                        {
                            returnMessage = @Resources.Locale.L_GateManage_Controllers_353 ;
                        }
                         

                        if (exsit != "success")
                        {
                            //問user要不要重新叫車，如果重新叫車，會把原來的預約記錄刪掉再新增
                            returnMessage = @Resources.Locale.L_GateManageController_Controllers_152;
                        }
                        else
                        {
                            string result = Business.ReserveManage.OrderTrucker(ShipmentId, g, c, d, Ext, UserId,idList, 0, 1, useDatetime, "Y");

                            if (result != "success")
                            {
                                if (returnMessage == "success")
                                {
                                    returnMessage = "";
                                }
                                returnMessage += "Shipment ID: " + ShipmentId + "," + result + "\n";
                            }
                        }
                    }
                }
            }

            sql = "UPDATE SMDND SET STATUS='B' WHERE U_ID=" + SQLUtils.QuotedStr(id);
            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<Dictionary<string, object>> smdndData = new List<Dictionary<string, object>>();
            int recordsCount = 0, pageIndex = 1, pageSize = 20;
            DataTable dt2 = ModelFactory.InquiryData("*", "SMDND", "U_FID='" + dnuid + "'", "CALL_DATE ASC", pageIndex, pageSize, ref recordsCount);
            smdndData = ModelFactory.ToTableJson(dt2, "SmdndModel");

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            return Json(new { message = returnMessage, smdndData = smdndData });
        }

        /*產生colmodel*/
        public ActionResult genColModel()
        {
            string table = Request.Params["table"];
            string name = Request.Params["name"];
            string keyValue = Request.Params["keys"];
            table = GetDecodeBase64ToString(table);
            name = GetDecodeBase64ToString(name);
            List<string> kyes = null;
            kyes = new List<string> { keyValue };
            string sql = "SELECT * FROM " + table + " WHERE 1=1";
            SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));

            return ToContent(SchemasCache[name]);
        }

        public ActionResult getSmrvForBat()
        { 
            string returnMessage = "success";
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string con = "", mf_no = string.Empty, sql = string.Empty;
            for (int i = 0; i < uidArray.Length; i++)
            {
                if (i == 0)
                {
                    sql = "SELECT MF_NO FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(uidArray[i]);
                    mf_no = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    con += SQLUtils.QuotedStr(uidArray[i]);
                }
                else
                {
                    con += "," + SQLUtils.QuotedStr(uidArray[i]);
                }
            }

            sql = "SELECT TOP 1 IO_FLAG FROM SYS_ACCT WHERE U_ID=" + SQLUtils.QuotedStr(UserId);
            string IoFlag = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string condition = "";

            if (IoFlag == "O")
            {
                condition = " TRUCKER=" + SQLUtils.QuotedStr(CompanyId) + " AND  ";
            }

            ActionResult result = null;
            result = GetBootstrapData("SMRV", string.Format(condition + " BAT_NO IS NOT NULL AND U_ID NOT IN({0}) AND STATUS NOT IN('P','O') AND MF_NO={1}", con, SQLUtils.QuotedStr(mf_no)));
            return Json(new { message = returnMessage, returnData = result });
        }

        /*檢查要合併的單號是否為同一卡車公司或是否沒有合併過*/
        public JsonResult chkBat()
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string Trucker = "", BatNo = "", Trucker1="", BatNo1="";
            string con = "";
            string returnMessage = "success";
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

            string sql = "SELECT * FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    Trucker = Prolink.Math.GetValueAsString(item["TRUCKER"]);
                    BatNo = Prolink.Math.GetValueAsString(item["BAT_NO"]);
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_ForecastQueryData_Views_243;
            }

            sql = "SELECT * FROM SMRV WHERE U_ID IN(" + con + ")";
            DataTable dt1 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt1.Rows.Count > 0)
            {
                foreach (DataRow item in dt1.Rows)
                {
                    Trucker1 = Prolink.Math.GetValueAsString(item["TRUCKER"]);
                    BatNo1 = Prolink.Math.GetValueAsString(item["BAT_NO"]);
                    string ReserveNo = Prolink.Math.GetValueAsString(item["RESERVE_NO"]);

                    if (Trucker != Trucker1)
                    {
                        return Json(new { message = @Resources.Locale.L_GateManageController_Controllers_153});
                    }

                    if (BatNo1 != "")
                    {
                        return Json(new { message = ReserveNo + @Resources.Locale.L_GateManageController_Controllers_154});
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_ForecastQueryData_Views_243;
            }

            return Json(new { message = returnMessage }); 
        }

        public JsonResult removeBatNo()
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string returnMessage = "success";
            MixedList mixlist = new MixedList();
            EditInstruct ei;

            for (int i = 0; i < uidArray.Length; i++)
            {
                string sql = "SELECT BAT_NO FROM SMRV WHERE U_ID={0}";
                sql = string.Format(sql, SQLUtils.QuotedStr(uidArray[i]));
                string BatNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (BatNo != "")
                {
                    sql = "SELECT COUNT(*) FROM SMRV WHERE BAT_NO=" + SQLUtils.QuotedStr(BatNo);
                    int n = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (n == 1)
                    {
                        string s = @Resources.Locale.L_GateManageController_Controllers_155;
                        returnMessage += string.Format(s, BatNo) + "\n";
                        continue;
                    }

                    ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", uidArray[i]);
                    ei.Put("BAT_NO", "");
                    //ei.Put("WS_CD", "");
                    ei.Put("GATE_NO", "");
                    ei.Put("STATUS", "D");
                    ei.Put("RESERVE_DATE", "");
                    ei.Put("RESERVE_FROM", "");
                    ei.Put("RESERVE_HOUR", "");
                    ei.Put("TRUCK_CNTRNO", "");
                    ei.Put("TRUCK_SEALNO", "");
                    ei.Put("TRUCK_NO", "");
                    ei.Put("DRIVER_ID", "");
                    ei.Put("TEL", "");
                    ei.Put("DRIVER", "");
                    //ei.Put("TEMP_WSCD", "");
                    ei.Put("TEMP_GATENO", "");
                    ei.Put("TEMP_WSCD", "");
                    ei.Put("TEMP_RDATE", "");
                    ei.Put("TEMP_RH", "");
                    ei.Put("LTRUCK_NO", "");
                    ei.Put("LDRIVER", "");
                    ei.Put("LTEL", "");
                    ei.Put("LDRIVER_ID", "");
                    ei.Put("IN_DATE", null);
                    ei.Put("SEAL_DATE", null);
                    ei.Put("PICK_DATE", null);
                    ei.Put("PUT_DATE", null);
                    ei.Put("EPT_IDATE", null);
                    ei.Put("EPT_ODATE", null);
                    ei.Put("REMARK", null);
                    ei.Put("ORDER_BY", null);
                    ei.Put("ORDER_DATE", null);
                    ei.Put("CONFIRM_BY", null);
                    ei.Put("CONFIRM_DATE", null);
                    mixlist.Add(ei);

                    string sql1 = string.Format(@"UPDATE SMSM
                                       SET SMSM.STATUS = 'D'
                                     WHERE EXISTS (SELECT *
                                              FROM SMRV
                                             WHERE SMRV.U_ID = '{0}'
                                               AND SMRV.SHIPMENT_ID = SMSM.SHIPMENT_ID) ", uidArray[i]);//

                    string sql2 = string.Format(@"UPDATE SMDN
                                       SET SMDN.SEAL_SATAUE = 'N'
                                     WHERE EXISTS (SELECT *
                                              FROM SMRV
                                             WHERE SMRV.U_ID = '{0}'
                                               AND SMRV.SHIPMENT_ID = SMDN.SHIPMENT_ID) ", uidArray[i]);//

                    mixlist.Add(sql1);
                    mixlist.Add(sql2);
                    try
                    {
                        OperationUtils.ExecuteUpdate(mixlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage += ex.ToString() + "\n";
                    }
                }
            }

            return Json(new { message = returnMessage });
        }

        public JsonResult cancelReserve()
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string returnMessage  = "success";
            MixedList mixlist = new MixedList();
            EditInstruct ei;

            for (int i = 0; i < uidArray.Length; i++)
            {
                ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uidArray[i]);
                ei.Put("BAT_NO", "");
                //ei.Put("WS_CD", "");
                ei.Put("GATE_NO", "");
                ei.Put("STATUS", "D");
                ei.Put("RESERVE_DATE", "");
                ei.Put("RESERVE_FROM", "");
                ei.Put("RESERVE_HOUR", "");
                ei.Put("TRUCK_CNTRNO", "");
                ei.Put("TRUCK_SEALNO", "");
                ei.Put("TRUCK_NO", "");
                ei.Put("DRIVER_ID", "");
                ei.Put("TEL", "");
                ei.Put("DRIVER", "");
                //ei.Put("TEMP_WSCD", "");
                ei.Put("TEMP_GATENO", "");
                ei.Put("TEMP_WSCD", "");
                ei.Put("TEMP_RDATE", "");
                ei.Put("TEMP_RH", "");
                ei.Put("LTRUCK_NO", "");
                ei.Put("LDRIVER", "");
                ei.Put("LTEL", "");
                ei.Put("LDRIVER_ID", "");
                ei.Put("IN_DATE", null);
                ei.Put("SEAL_DATE", null);
                ei.Put("PICK_DATE", null);
                ei.Put("PUT_DATE", null);
                ei.Put("EPT_IDATE", null);
                ei.Put("EPT_ODATE", null);
                ei.Put("REMARK", null);
                ei.Put("ORDER_BY", null);
                ei.Put("ORDER_DATE", null);
                ei.Put("CONFIRM_BY", null);
                ei.Put("CONFIRM_DATE", null);
                mixlist.Add(ei);

                try
                {
                    OperationUtils.ExecuteUpdate(mixlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage += ex.ToString() + "\n";
                }
            }

            return Json(new { message = returnMessage });
        }

        /*複制主單到其它合併單*/
        public bool copyToOtherReserve(string UId, string ids)
        {

            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string sql = "";
            sql = "SELECT BAT_NO FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string BatNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (BatNo == "")
            {
                return false;
            }
            MixedList mixlist = new MixedList();
            for (int i = 0; i < uidArray.Length; i++)
            {
                sql = @"UPDATE SMRV
	                        SET
                            SMRV.STATUS=S2.STATUS,
	                        SMRV.BAT_NO=S2.BAT_NO,
	                        SMRV.WS_CD = S2.WS_CD,
	                        SMRV.GATE_NO = S2.GATE_NO,
	                        SMRV.RESERVE_DATE = S2.RESERVE_DATE,
	                        SMRV.RESERVE_FROM = S2.RESERVE_FROM,
	                        SMRV.RESERVE_HOUR = S2.RESERVE_HOUR,
	                        SMRV.TRUCK_CNTRNO = S2.TRUCK_CNTRNO,
	                        SMRV.TRUCK_SEALNO = S2.TRUCK_SEALNO,
	                        SMRV.TRUCK_NO = S2.TRUCK_NO,
	                        SMRV.DRIVER_ID = S2.DRIVER_ID,
	                        SMRV.TEL = S2.TEL,
	                        SMRV.DRIVER = S2.DRIVER,
	                        SMRV.TEMP_WSCD = S2.TEMP_WSCD,
                            SMRV.TEMP_GATENO=S2.TEMP_GATENO,
	                        SMRV.TEMP_RDATE = S2.TEMP_RDATE,
	                        SMRV.TEMP_RFROM = S2.TEMP_RFROM,
	                        SMRV.TEMP_RH = S2.TEMP_RH,
	                        SMRV.LTRUCK_NO = S2.LTRUCK_NO,
	                        SMRV.LDRIVER = S2.LDRIVER,
	                        SMRV.LTEL = S2.LTEL,
	                        SMRV.LDRIVER_ID = S2.LDRIVER_ID,
                            SMRV.IN_BY = S2.IN_BY,
                            SMRV.IN_DATE = S2.IN_DATE,
                            SMRV.OUT_DATE = S2.OUT_DATE,
                            SMRV.OUT_BY = S2.OUT_BY,
                            SMRV.SEAL_DATE = S2.SEAL_DATE 
                        FROM SMRV, SMRV S2
                        WHERE SMRV.U_ID='{0}' AND S2.U_ID='{1}'";
                sql = string.Format(sql, uidArray[i], UId);

                mixlist.Add(sql);

                string sql1 = string.Format(@"UPDATE SMSM
                                SET SMSM.STATUS = S2.STATUS FROM SMSM, SMSM S2
                                WHERE EXISTS (SELECT SHIPMENT_ID FROM SMRV WHERE SMRV.U_ID='{0}'  AND SMSM.SHIPMENT_ID =SMRV.SHIPMENT_ID)
                                AND EXISTS (SELECT SHIPMENT_ID FROM SMRV WHERE SMRV.U_ID='{1}' AND S2.SHIPMENT_ID =SMRV.SHIPMENT_ID)", uidArray[i], UId);
                mixlist.Add(sql1);
            }

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixlist, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public JsonResult setBatNo()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            string returnMessage = "success";
            string[] uidArray = ids.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            string sql = "SELECT MF_NO FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string mf_no = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            for (int i = 0; i < uidArray.Length; i++)
            {
                string uuid = uidArray[i];
                sql = "SELECT MF_NO FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(uuid);
                string umf_no = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (umf_no != mf_no)
                {
                    returnMessage = @Resources.Locale.L_GateManageController_Controllers_156;
                    return Json(new { message = returnMessage });
                }
            }
                try
                {
                    bool copy = copyToOtherReserve(UId, ids);
                    if (copy == false)
                    {
                        returnMessage = @Resources.Locale.L_GateManageController_Controllers_157;
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }

            return Json(new { message = returnMessage });
        }

        public ActionResult test123()
        {
            string msg = "success";
            string UId = Request.Params["UId"];
            string PartyName = Request.Params["PartyName"];
            string UName = Request.Params["UName"];
            string UPassword = Request.Params["UPassword"];
            string mailStr = Request.Params["mailStr"];
            EvenFactory.AddEven(UId + "#" +PartyName+ "#" + UName + "#" + UPassword + "#" +  GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), UId, MailManager.UserInfo, null, 1, 0, mailStr, "User Infomation", "");

            return ToContent(msg);
        }

        public ActionResult WHouseDelivery()
        {
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            string sm_id = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
            string reserveno = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
            string bat_no = Prolink.Math.GetValueAsString(dt.Rows[0]["BAT_NO"]);
            string qaholdmsg = Business.SMHandle.QAHoldBlMessage(bat_no, "QAH",true);
            if (!string.IsNullOrEmpty(qaholdmsg))
            {
                return Json(new { message = qaholdmsg, IsOk = "N" });
            }

            sql = "SELECT POL_CD, POL_NAME, CMP FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (sdt.Rows.Count <= 0)
            {
                return Json(new { message = @Resources.Locale.L_GateManageController_Controllers_158, IsOk = "N" });
            }
            string pol_cd = Prolink.Math.GetValueAsString(sdt.Rows[0]["POL_CD"]);
            string pol_name = Prolink.Math.GetValueAsString(sdt.Rows[0]["POL_NAME"]);
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", UId);
            ei.Put("STATUS", "O");
            ml.Add(ei);
            TmexpHandler th = new TmexpHandler();
            TmexpInfo tpi = new TmexpInfo();
            tpi.UFid = UId;
            tpi.WrId = UserId;
            tpi.WrDate = DateTime.Now;
            tpi.Cmp = CompanyId;
            tpi.GroupId = GroupId;
            tpi.JobNo = reserveno;
            tpi.ExpType = "RV";
            tpi.ExpReason = "RVO";
            tpi.ExpText = @Resources.Locale.L_GateManageController_Controllers_159 + UserId + @Resources.Locale.L_GateManage_Controllers_365;
            tpi.ExpObj = UserId;
            ml.Add(th.SetTmexpEi(tpi));

            EditInstruct smei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            smei.PutKey("SHIPMENT_ID", sm_id);
            smei.Put("STATUS", "O");
            ml.Add(smei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ShipmentId = sm_id, StsCd = "100", StsDescp = "Leave Factory", Location = pol_cd, LocationName = pol_name, Sender = this.UserId });
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
                return Json(new { message = ex.Message, IsOk = "N" });
            }
            return Json(new { message = returnMessage, IsOk = "Y" });
        }

        public string getTorder(string shipmentId) 
        {
            string sql = string.Format("SELECT TORDER FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            string torder = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return torder;
        }
    }

    public class smwhTable {
        public List<whItem> wh = new List<whItem>();
    }

    public class whItem {
        public string WsCd { get; set; }
        //public List<object> Gates { get; set; }
        public List<gtItem> Gates = new List<gtItem>();
    }

    public class gtItem {
        public string GateNo { get; set; }
        public string GateStatus { get; set; }
        public string CntrNo { get; set; }
        public string ReserveNo { get; set; }
    }
}
