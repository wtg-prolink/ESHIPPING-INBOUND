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
using Business.TPV;
using System.IO;
using TrackingEDI;
using TrackingEDI.Model;
using System.Web.Configuration;
using System.Threading;
namespace WebGui.Controllers
{
    public class SMALController : BaseController
    {
        //
        // GET: /SMAL/

        #region View
        public ActionResult QueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("AL010");
            ViewBag.TranTypeSel = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult SetupView(string id = null, string uid = null)
        {
            SetSchema("SMAL");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("AL010");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.PartyType = CommonHelp.getBscodeForSelect("PT", GetDataPmsCondition("C"));
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string condition = GetCreateDateCondition("SMAL", GetDataPmsCondition("C"));
            return GetBootstrapData("SMAL", condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMAL WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmalModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);

            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string su_id = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmalModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string PartyType = Prolink.Math.GetValueAsString(ei.Get("PARTY_TYPE"));
                            PartyType = PartyType.Trim(';');
                            string[] p = PartyType.Split(';');
                            EditInstruct[] et = new EditInstruct[p.Length];
                            for (int j = 0; j < p.Length; j++)
			                {
                                et[j] = new EditInstruct("SMAL", EditInstruct.INSERT_OPERATION);
                                string[,] unikey = new string[7, 2] { { "TRAN_TYPE", Prolink.Math.GetValueAsString(ei.Get("TRAN_TYPE")) }, { "FREIGHT_TERM", Prolink.Math.GetValueAsString(ei.Get("FREIGHT_TERM")) }, { "INCOTERM_CD", Prolink.Math.GetValueAsString(ei.Get("INCOTERM_CD")) }, { "CONN_CD", Prolink.Math.GetValueAsString(ei.Get("CONN_CD")) }, { "POD_CD", Prolink.Math.GetValueAsString(ei.Get("POD_CD")) }, { "PARTY_TYPE", p[j] }, { "CARRIER", Prolink.Math.GetValueAsString(ei.Get("CARRIER")) } };

                                if (chkKeyIsExist("SMAL", unikey) == true)
                                {
                                    return Json(new { message = Resources.Locale.M_CtAssignManage_Msg03 }); //訊息：資料重覆，無法保存
                                }
                                
                                u_id = System.Guid.NewGuid().ToString();
                                et[j].Put("U_ID", u_id);
                                et[j].Put("GROUP_ID", GroupId);
                                et[j].Put("CMP", CompanyId);
                                et[j].Put("STN", Station);
                                et[j].Put("DEP", Dep);
                                et[j].Put("CREATE_BY", UserId);
                                et[j].PutDate("CREATE_DATE", DateTime.Now);
                                et[j].Put("PARTY_TYPE", p[j]);
                                et[j].Put("TRAN_TYPE", Prolink.Math.GetValueAsString(ei.Get("TRAN_TYPE")));
                                et[j].Put("FREIGHT_TERM", Prolink.Math.GetValueAsString(ei.Get("FREIGHT_TERM")));
                                et[j].Put("INCOTERM_CD", Prolink.Math.GetValueAsString(ei.Get("INCOTERM_CD")));
                                et[j].Put("INCOTERM_DESCP", Prolink.Math.GetValueAsString(ei.Get("INCOTERM_DESCP")));
                                et[j].Put("CONN_CD", Prolink.Math.GetValueAsString(ei.Get("CONN_CD")));
                                et[j].Put("CONN_NM", Prolink.Math.GetValueAsString(ei.Get("CONN_NM")));
                                et[j].Put("POD_CD", Prolink.Math.GetValueAsString(ei.Get("POD_CD")));
                                et[j].Put("POD_NM", Prolink.Math.GetValueAsString(ei.Get("POD_NM")));
                                et[j].Put("TERMINAL_CD", Prolink.Math.GetValueAsString(ei.Get("TERMINAL_CD")));
                                et[j].Put("TERMINAL_NM", Prolink.Math.GetValueAsString(ei.Get("TERMINAL_NM")));
                                et[j].Put("LSP_CD", Prolink.Math.GetValueAsString(ei.Get("LSP_CD")));
                                et[j].Put("LSP_NM", Prolink.Math.GetValueAsString(ei.Get("LSP_NM")));
                                et[j].Put("DLV_AREA", Prolink.Math.GetValueAsString(ei.Get("DLV_AREA")));
                                et[j].Put("DLV_AREA_NM", Prolink.Math.GetValueAsString(ei.Get("DLV_AREA_NM")));
                                et[j].Put("ADDR_CODE", Prolink.Math.GetValueAsString(ei.Get("ADDR_CODE")));
                                et[j].Put("DLV_ADDR", Prolink.Math.GetValueAsString(ei.Get("DLV_ADDR")));
                                et[j].Put("REMARK", Prolink.Math.GetValueAsString(ei.Get("REMARK")));
                                et[j].Put("WS_CD", Prolink.Math.GetValueAsString(ei.Get("WS_CD")));
                                et[j].Put("WS_NM", Prolink.Math.GetValueAsString(ei.Get("WS_NM")));
                                et[j].Put("FINAL_WH", Prolink.Math.GetValueAsString(ei.Get("FINAL_WH")));
                                et[j].Put("CARRIER", Prolink.Math.GetValueAsString(ei.Get("CARRIER")));
                                et[j].Put("WE_CD", Prolink.Math.GetValueAsString(ei.Get("WE_CD")));
                                et[j].Put("WE_NM", Prolink.Math.GetValueAsString(ei.Get("WE_NM")));
                                mixList.Add(et[j]);
			                }
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            mixList.Add(ei);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            mixList.Add(ei);
                        }
                        
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

            string sql = string.Format("SELECT * FROM SMAL WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmalModel");
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
                    case "SMAL":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMAL WHERE 1=0";
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

    }
}
