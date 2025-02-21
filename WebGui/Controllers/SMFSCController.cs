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

namespace WebGui.Controllers
{
    public class SMFSCController : BaseController
    {

        #region View
        public ActionResult SMFSCQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("QT015");
            return View();
        }

        public ActionResult SMFSCSetupView(string id = null, string uid = null)
        {
            SetSchema("SMFSCSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("QT015");
            ViewBag.Uid = id;
            string CmpNm = "";
            if (IOFlag == "O")
            {
                string sql = "SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE STATUS='U' AND PARTY_NO=" + SQLUtils.QuotedStr(CompanyId);
                CmpNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            ViewBag.CmpNm = HttpUtility.HtmlDecode(CmpNm);
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult SMFSCQuery()
        {
            string condition = "";
            if (IOFlag == "O")
            {
                condition = " GROUP_ID='TPV' AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            }
            return GetBootstrapData("SMFSC", condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetSmfscDetail()
        {
            string u_id = Request["UId"];
            string Carrier = "";
            string Area = "";
            string sql = string.Format("SELECT * FROM SMFSC WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (mainDt.Rows.Count > 0)
            {
                Carrier = Prolink.Math.GetValueAsString(mainDt.Rows[0]["CARRIER"]);
                Area = Prolink.Math.GetValueAsString(mainDt.Rows[0]["AREA"]);
            }
            sql = string.Format("SELECT * FROM BSCAA WHERE CARRIER={0} AND AREA={1}", SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(Area));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SMFSCModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "BSCAAModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult SmfscUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string Carrier = Prolink.Math.GetValueAsString(Request.Params["Carrier"]);
            string EffectDate = Prolink.Math.GetValueAsString(Request.Params["EffectDate"]);
            string Area = Prolink.Math.GetValueAsString(Request.Params["Area"]);
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
            if (!string.IsNullOrEmpty(Carrier))
                Carrier = HttpUtility.UrlDecode(Carrier);
            if (!string.IsNullOrEmpty(EffectDate))
                EffectDate = HttpUtility.UrlDecode(EffectDate);
            if (!string.IsNullOrEmpty(Area))
                Area = HttpUtility.UrlDecode(Area);
            if (!string.IsNullOrEmpty(Cmp))
                Cmp = HttpUtility.UrlDecode(Cmp);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMFSCModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            //bool chk = ChkKey(Carrier,EffectDate,Area);
                            //if (chk == false)
                            //{
                            //    return Json(new { message = @Resources.Locale.L_SMFSCController_Controllers_210 });
                            //}
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            Cmp = ei.Get("CMP");
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
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BSCAAModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", Cmp);
                            ei.Put("AREA", Area);
                            ei.Put("CARRIER", Carrier);
                            ei.Put("U_FID", UId);
                        }
                        else
                            ei.AddKey("U_ID");
                        //string test_id = ei.Get("U_ID");
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
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            string sql = string.Format("SELECT * FROM SMFSC WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM BSCAA WHERE CARRIER={0} AND AREA={1}", SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(Area));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmrqmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmrqdModel");
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
                    case "SMFSCSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMFSC WHERE 1=0";
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

        #region 检查唯一值
        public bool ChkKey(string Carrier, string EffectDate, string Area)
        {
            string sql = "SELECT COUNT(*) FROM SMFSC WHERE CARRIER=" + SQLUtils.QuotedStr(Carrier) + " AND EFFECT_DATE=" + SQLUtils.QuotedStr(EffectDate) + " AND AREA=" + SQLUtils.QuotedStr(Area);
            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (num > 0)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Excel Download
        public ActionResult downloadSampleExcel()
        {
            string resultType = Request.Params["resultType"];
            string sql = "SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE STATUS='U' AND PARTY_NO=" + SQLUtils.QuotedStr(CompanyId);
            string CmpNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dt = null;
            sql = @"SELECT * FROM SMFSC WHERE U_ID='xxx'";
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow workRow = dt.NewRow();
            if (dt.Rows.Count == 0)
            {
                workRow["CMP"] = CompanyId;
                workRow["CMP_NM"] = CmpNm;
                dt.Rows.Add(workRow);
            }
            if (resultType == "excel")
                return ExportExcelFile(dt);

            return ExportExcelFile(dt);
        }
        #endregion

        #region Excel Upload
        
        [HttpPost]
        public ActionResult UploadSmfsc(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList mx = new MixedList();
            string returnMessage = "success";
            string sql = "";
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

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
                int[] resulst;
                /*
                 * dr[0]: CARRIER
                 * dr[1]: FAREA
                 * dr[2]: AREA
                 * dr[3]: AREA_NM
                 * dr[4]: EFFECT DATE
                 * dr[5]: 20GP
                 * dr[6]: 40GP
                 * dr[7]: 40HQ
                 */
                try
                {

                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];

                        ei = new EditInstruct("SMFSC", EditInstruct.INSERT_OPERATION);

                        //string CMP = Prolink.Math.GetValueAsString(dr[0]);
                        //string CMP_NM = Prolink.Math.GetValueAsString(dr[1]);
                        string CARRIER = Prolink.Math.GetValueAsString(dr[0]);
                        sql = "SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TCAR' AND CD=" + SQLUtils.QuotedStr(CARRIER);
                        string CARRIER_NM = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string FAREA = Prolink.Math.GetValueAsString(dr[1]);
                        string AREA = Prolink.Math.GetValueAsString(dr[2]);
                        string AREA_NM = Prolink.Math.GetValueAsString(dr[3]);
                        DateTime EFFECT_DATE = Prolink.Math.GetValueAsDateTime(dr[4]);
                        decimal GP20 = Prolink.Math.GetValueAsDecimal(dr[5]);
                        decimal GP40 = Prolink.Math.GetValueAsDecimal(dr[6]);
                        decimal HQ40 = Prolink.Math.GetValueAsDecimal(dr[7]);

                        sql = "SELECT COUNT(*) FROM BSCODE WHERE CD_TYPE='TCAR' AND CD=" + SQLUtils.QuotedStr(CARRIER);
                        int bNum = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if(bNum > 0)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            //ei.Put("CMP_NM", CMP_NM);
                            ei.Put("CARRIER", CARRIER);
                            ei.Put("CARRIER_NM", CARRIER_NM);
                            ei.PutDate("EFFECT_DATE", EFFECT_DATE);
                            ei.Put("AREA", AREA);
                            ei.Put("AREA_NM", AREA_NM);
                            ei.Put("FAREA", FAREA);
                            ei.Put("GP20", GP20);
                            ei.Put("GP40", GP40);
                            ei.Put("HQ40", HQ40);
                            //ei.Put("OTH_RATE", OTHER);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
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
        #endregion

        [HttpPost]
        public ActionResult UploadBSCAA(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList mx = new MixedList();
            string returnMessage = "success";
            string sql = "";
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

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
                int[] resulst;
                /*
                 * dr[0]: CARRIER
                 * dr[1]: AREA
                 * dr[2]: CNTRY
                 * dr[3]: PORT
                 */
                try
                {

                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];

                        ei = new EditInstruct("BSCAA", EditInstruct.INSERT_OPERATION);

                        string CARRIER = Prolink.Math.GetValueAsString(dr[0]);
                        sql = "SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TCAR' AND CD=" + SQLUtils.QuotedStr(CARRIER);
                        string CARRIER_NM = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string AREA = Prolink.Math.GetValueAsString(dr[1]);
                        string CNTRY = Prolink.Math.GetValueAsString(dr[2]);
                        string PORT = Prolink.Math.GetValueAsString(dr[3]);
                        string PORT_CD = CNTRY + PORT;
                        sql = "SELECT PORT_NM FROM BSCITY WHERE CNTRY_CD="+SQLUtils.QuotedStr(CNTRY)+" AND PORT_CD=" + SQLUtils.QuotedStr(PORT);
                        string PORT_DESCP = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        sql = "SELECT COUNT(*) FROM BSCODE WHERE CD_TYPE='TCAR' AND CD=" + SQLUtils.QuotedStr(CARRIER);
                        int bNum = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (bNum > 0)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CARRIER", CARRIER);
                            ei.Put("AREA", AREA);
                            ei.Put("PORT", PORT_CD);
                            ei.Put("PORT_DESCP", PORT_DESCP);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
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

    }
}
