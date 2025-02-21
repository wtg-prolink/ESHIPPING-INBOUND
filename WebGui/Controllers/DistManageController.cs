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
using TrackingEDI.Business;

namespace WebGui.Controllers
{
    public class DistManageController : BaseController
    {
        //
        // GET: /DistManage/
        #region view
        public ActionResult ContainUtili()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult ContainUtiliSetup()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT * FROM SMCNP WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.UId = uid;
            return View();
        }

        public ActionResult ContainUtiliUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string uid = Request.Params["UId"];
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmcnpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            uid = Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                            string sql = string.Format("SELECT * FROM SMCNP WHERE OD_TYPE={0} AND YEAR={1} AND MONTH={2} AND WEEK={3} AND CMP={4} AND REGION={5} AND POD={6}"
                               , SQLUtils.QuotedStr(ei.Get("OD_TYPE"))
                               , SQLUtils.QuotedStr(ei.Get("YEAR"))
                               , SQLUtils.QuotedStr(ei.Get("MONTH"))
                               , SQLUtils.QuotedStr(ei.Get("WEEK"))
                               , SQLUtils.QuotedStr(ei.Get("CMP"))
                               , SQLUtils.QuotedStr(ei.Get("REGION"))
                               , SQLUtils.QuotedStr(ei.Get("POD")));

                            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (dt.Rows.Count > 0)
                                return Json(new { message = @Resources.Locale.L_DistManage_Controllers_229 });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                DataTable dt = new DataTable();
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { UId = uid });
        }

        public ActionResult ContainUtiliInquiryData()
        {
            return GetBootstrapData("SMCNP", "");
        }

        public ActionResult ContainUsage()
        { 
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult ContainUsageSetup()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT * FROM SMCNU WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.UId = uid;
            return View();
        }

        public ActionResult DistributMana()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult ForecastQueryData()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult ForecastSetup()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT * FROM SMFCM WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.UId = uid;
            return View();
        }

        public ActionResult DRule() {
            ViewBag.MenuBar = false;
            SetTermSelect();
            SetTrackWaySelect();
            return View();
        }

        private void SetTrackWaySelect()
        {
            ViewBag.SelectTrack = "";
            ViewBag.DefaultTrack = "";

            string types = "'TDTK'";
            string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE {0} AND CD_TYPE IN({1})", GetBaseGroup(), types);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultTrack = Prolink.Math.GetValueAsString(dr["CD"]);
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
            }
            ViewBag.SelectTrack = select;
        }

        private void SetTermSelect()
        {
            ViewBag.SelectTerm = "";
            ViewBag.DefaultTerm = "";

            string types = "'TCGT'";
            string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE {0} AND CD_TYPE IN({1})", GetBaseGroup(), types);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultTerm = Prolink.Math.GetValueAsString(dr["CD"]);
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
            }
            select += ";Finish:Finish";
            ViewBag.SelectTerm = select;
        }

        public ActionResult DRuleSetup(string id = null, string uid = null)
        {
            SetSchema("DRuleSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            return View();
        }

        #endregion

        #region 基础方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "DRuleSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMDLSPR WHERE 1=0";
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
            if (resultType == "count")
            {
                //string statusField = Request.Params["statusField"];
                //dt = GetStatusCountData(statusField, table, condition, Request.Params);
                //pageSize = 1;
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

        public ActionResult ContainUsageUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string uid = Request.Params["UId"];
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmcnuModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            uid = Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("CMP", CompanyId);

                            string sql = string.Format("SELECT * FROM SMCNU WHERE YEAR={0} AND MONTH={1} AND WEEK={2} AND CARRIER={3} AND REGION={4}"
                               , SQLUtils.QuotedStr(ei.Get("YEAR"))
                               , SQLUtils.QuotedStr(ei.Get("MONTH"))
                               , SQLUtils.QuotedStr(ei.Get("WEEK"))
                               , SQLUtils.QuotedStr(ei.Get("CARRIER"))
                               , SQLUtils.QuotedStr(ei.Get("REGION")));

                            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (dt.Rows.Count > 0)
                                return Json(new { message = @Resources.Locale.L_DistManage_Controllers_229 });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("U_ID", uid);
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                DataTable dt = new DataTable();
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { UId = uid });
        }

        public ActionResult ContainUsageInquiryData()
        {
            return GetBootstrapData("SMCNU", "");
        }


        public ActionResult DisTributInquiryData()
        {
            return GetBootstrapData("SMFCC", "");
        }

        public ActionResult LogisticsRule()
        { 
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult LogisticsRuleSetup()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT * FROM SMLSPR WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.UId = uid;
            return View();
        }

        public ActionResult LogisticsRuleUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmlsprModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        uid = ei.Get("U_ID");
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                DataTable dt = new DataTable();
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    dt = ModelFactory.InquiryData("*", "SMLSPR", "U_ID='" + uid + "'", "", pageIndex, pageSize, ref recordsCount);
                    userData = ModelFactory.ToTableJson(dt, "SmlsprModel");
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, rows = userData });
        }

        public ActionResult LogisticsRuleInquiryData()
        {
            return GetBootstrapData("SMLSPR", "");
        }

        public ActionResult ForecastInquiryData()
        {
            return GetBootstrapData("SMFCM", "");
        }

        public ActionResult GetForecastItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMFCM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmfcmModel");
            return ToContent(data);
        }

        public ActionResult GetContainUtiliItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMCNP WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmcnpModel");
            return ToContent(data);
        }

        public ActionResult GetContainUsageItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMCNU WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmcnuModel");
            return ToContent(data);
        }

        public ActionResult ForecastUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string uid = Request.Params["UId"];
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmfcmModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            uid=Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("CMP", CompanyId);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                DataTable dt = new DataTable();
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { UId = uid });
        }

        #endregion

        public ActionResult TurnAllocation()
        {
            string uid = Request.Params["UId"];     
            AllocationHelper ah=new AllocationHelper();
            string returnMessage=ah.ChangeAllocation(uid);
            if(string.IsNullOrEmpty(returnMessage))
            {
                returnMessage = @Resources.Locale.L_DNManage_Allocation_EntS;
            }
            return Json(new { message = returnMessage });
        }

        public ActionResult BatchRemove()
        {
            string uid = Request.Params["UId"];
            string[] uidArray = uid.Split(',');
            string returnMessage = string.Empty;
            string sql = string.Format("DELETE FROM SMFCM WHERE U_ID IN {0}", SQLUtils.Quoted(uidArray));
            try
            {
                int result = OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                return Json(new { message = returnMessage, IsOk = "N" });
            }
            returnMessage = @Resources.Locale.L_MailFormatSetup_DelS;
            return Json(new { message = returnMessage, IsOk = "Y" });
        }


        public JsonResult SingleLineChart()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string sql="SELECT YEAR, MONTH,SUM(AFEU)AFEU FROM SMFCM WHERE 1=1 GROUP BY YEAR,MONTH ORDER BY MONTH ASC";
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<Dictionary<string, object>> retrunData = new List<Dictionary<string, object>>();
            List<List<object>> list = new List<List<object>>();
            Dictionary<string, object> seriesOptions = null;
            
            for(int i=2014;i<2016;i++)
            {
                seriesOptions = new Dictionary<string, object>();
                DataRow []yeardata=mainDt.Select("YEAR='"+i+"'","MONTH ASC");
                seriesOptions["name"] =Prolink.Math.GetValueAsString(i);
                decimal[] monthdata = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                for(int j=0;j<yeardata.Length;j++)
                {
                    DataRow dr=yeardata[j];
                    int month=Prolink.Math.GetValueAsInt(dr["MONTH"]);
                    monthdata[month - 1] = Prolink.Math.GetValueAsDecimal(dr["AFEU"]); ;
                }
                seriesOptions["data"] = monthdata;
                retrunData.Add(seriesOptions);
            }

            return Json(new { data = new { data = retrunData } }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Upload(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                return Json(new { message = "error" });
            }
            else
            {
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                string mapping = "ForecastMapping";
                Dictionary<string, object> parm = new Dictionary<string, object>();
                parm["USER_ID"] = UserId;
                MixedList ml = new MixedList();
                ExcelParser.RegisterEditInstructFunc(mapping, HandleForeaset);
                ExcelParser ep = new ExcelParser();
                ep.Save(mapping, excelFileName, ml,parm);

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                    return Json(new { message = returnMessage });
                }
            }
            return Json(new { message = returnMessage });
        }

        public static string HandleForeaset(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("OD_TYPE", "O");
            string cmp = ei.Get("CMP");
            cmp = cmp.Replace("TPV", "");
            ei.Put("CMP", cmp);

            if(string.IsNullOrEmpty(ei.Get("BU"))&&string.IsNullOrEmpty(ei.Get("POR"))&&string.IsNullOrEmpty(ei.Get("POL"))&&
                string.IsNullOrEmpty(ei.Get("POD"))){
                    return BaseParser.ERROR;
            }

            if ("(范例)TV".Equals(ei.Get("MODEL")))
            {
                return BaseParser.ERROR;
            }

            if(parm["USER_ID"]!=null)
            {
                ei.Put("CREATE_BY", parm["USER_ID"]);
            }
            ei.PutDate("CREATE_DATE", DateTime.Now);
            return string.Empty;
        }

        public ActionResult SaveDRule()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData == "{}")
            {
                return Json(new { message = "No valid Data!" });
            }
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = string.Empty;
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdlsprModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
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

            sql = string.Format("SELECT * FROM SMDLSPR WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(groupDt, "SmdlsprModel"));
        }

        public ActionResult GetDRuleData()
        {
            return GetBootstrapData("SMDLSPR", "");
        }

        public ActionResult GetDRuleItem()
        {
            string u_id = Request["uId"];
            string sql = string.Empty;
            //if (!string.IsNullOrEmpty(u_id))
                sql = string.Format("SELECT * FROM SMDLSPR WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(groupDt, "SmdlsprModel"));
        }

        public ActionResult CatchActuail()
        {
            string odtype = Request.Params["odtype"];
            if(odtype.Equals("O"))
            {
                odtype="F;L";
            }else{
                odtype="A;D;E;R;T";
            }
            string[] odtypes = odtype.Split(';');
            string year = Request.Params["year"];
            string week = Request.Params["week"];
            string sql = string.Format("SELECT COUNT() FROM SMSM WHERE YEAR={0} AND WEEK={1} AND TRAN_TYPE IN {2}",
                SQLUtils.QuotedStr(year),
                SQLUtils.QuotedStr(week),
                SQLUtils.Quoted(odtypes));
            return Json(new { message = @Resources.Locale.L_ActSetup_Scripts_60 });
        }

        /// <summary>
        /// 根据年和周获取月份
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMonth()
        {
            int year = GetIntValue(Request["y"]);
            int week = GetIntValue(Request["w"]);
            DateTime date = TrackingEDI.Business.DateTimeUtils.WeekToDate(year, week);
            if (date.Year != year)
                return Json(new { message = @Resources.Locale.L_DistManage_Controllers_231 });
            return Json(new { m = date.Month, d = date.Day });
        }

        private static int GetIntValue(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;
            int dvalue = 0;
            if (int.TryParse(val, out dvalue))
                return dvalue;
            return 0;
        }
    }
}
