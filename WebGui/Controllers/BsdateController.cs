using System;
using System.Collections.Generic;
using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System.Collections;
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
using System.IO;
namespace WebGui.Controllers
{
    public class BsdateController : BaseController
    {
        //
        // GET: /Bsdate/

        #region 產生整年假日
        public ActionResult genDate()
        {
            DateTime d_day = new DateTime(2017, 1, 1);
            DateTime e_day = new DateTime(2017, 12, 31);
            MixedList mixList = new MixedList();

            for (DateTime date = d_day; date < e_day; date = date.AddDays(1.0))
            {
                string tmp = date.DayOfWeek.ToString("d");

                if (tmp == "0" || tmp == "6")
                {
                    EditInstruct ei;
                    ei = new EditInstruct("BSDATE", EditInstruct.INSERT_OPERATION);
                    string UId = System.Guid.NewGuid().ToString();
                    ei.Put("U_ID", UId);
                    ei.Put("GROUP_ID", GroupId);
                    ei.Put("CMP", CompanyId);
                    ei.Put("STN", "*");
                    ei.Put("DEP", "*");
                    ei.Put("D_DAY", date.ToString("yyyy-MM-dd"));
                    ei.Put("CREATE_BY", UserId);
                    ei.Put("CREATE_DATE", DateTime.Now.ToString("yyyy-MM-dd"));

                    mixList.Add(ei);
                }
            }

            if (mixList.Count > 0)
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }


            return Json(new { });
        }
        #endregion

        #region View
        public ActionResult BsdateQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("Bsdate010");
            return View();
        }

        public ActionResult BsdateSetupView(string id = null, string uid = null)
        {
            SetSchema("BSDATE");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("Bsdate010");
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string condition = GetDataPmsCondition("C");
            condition = GetCreateDateCondition("BSDATE", condition);
            return GetBootstrapData("BSDATE", condition);
        }
        #endregion

        #region 匯入行事曆
        [HttpPost]
        public ActionResult ImportCalender(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList ml2 = new MixedList();
            string returnMessage = "success";
            string ermsg = "";
            string Sql = "";
            string UId = "", Remark = "";
            EditInstruct ei;
            int StartRow = 0; //Excel 從第2排開始讀
            //int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

            //if (StartRow == 0)
            //{
            //    returnMessage = @Resources.Locale.L_GateManage_Controllers_299;
            //    return Json(new { message = returnMessage });
            //}
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
                 * dr[0]: Date 
                 * dr[1]: Remark 
                 */

                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        //必填欄位                        
                        DateTime Date = Prolink.Math.GetValueAsDateTime(dr[0]);
                        Remark = Prolink.Math.GetValueAsString(dr[1]);
                        string Country = Prolink.Math.GetValueAsString(dr[2]);
                        if (Date.ToString() == null || Date.ToString() == "")
                        {
                            ermsg = "Error";
                            return Json(new { message = ermsg }); ;
                            break;
                        }
                        Sql = "SELECT * FROM BSDATE WHERE GROUP_ID = {0} AND CMP = {1} AND D_DAY={2} AND CNTRY_CD={3} ";
                        Sql = string.Format(Sql, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Date.ToString(("yyyy/MM/dd"))), SQLUtils.QuotedStr(Country));////
                        DataTable Datedt = getDataTableFromSql(Sql);
                        if (Datedt.Rows.Count > 0)
                        {
                            UId = Datedt.Rows[0]["U_ID"].ToString();
                            ei = new EditInstruct("BSDATE", EditInstruct.UPDATE_OPERATION);
                            ei.PutKey("U_ID", UId);
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
                            //ei.Put("D_DAY", Date);
                            ei.Put("REMARK", Remark);
                            ei.Put("MODIFY_BY", UserId);
                            ei.Put("CNTRY_CD", Country);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ml.Add(ei);
                        }
                        else
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei = new EditInstruct("BSDATE", EditInstruct.INSERT_OPERATION);
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", "*");
                            ei.Put("DEP", "*");
                            ei.PutDate("D_DAY", Date);
                            ei.Put("REMARK", Remark);
                            ei.Put("CNTRY_CD", Country);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ml.Add(ei);
                        }
                    }
                    if (ml.Count > 0)
                    {
                        int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
                catch (Exception ex)
                {
                    ermsg = ex.ToString();
                    return Json(new { errorMsg = ermsg });
                }
            }
            return Json(new { message = returnMessage, errorMsg = ermsg });
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM BSDATE WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BsdateModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string d_day = Prolink.Math.GetValueAsString(Request.Params["D_Day"]);
            string d_cnty = Prolink.Math.GetValueAsString(Request.Params["D_Cnty"]);
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
            if (!string.IsNullOrEmpty(d_day))
                d_day = HttpUtility.UrlDecode(d_day);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string su_id = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsdateModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            //string[,] unikey = new string[1, 2] { { "D_DAY", ei.Get("D_DAY") } };
                            string[,] unikey = new string[3, 2] { { "D_DAY", d_day }, { "CNTRY_CD", d_cnty }, { "CMP", CompanyId } };

                            if (chkKeyIsExist("BSDATE", unikey) == true)
                            {
                                return Json(new { message = Resources.Locale.M_CtAssignManage_Msg03 }); //訊息：資料重覆，無法保存
                            }

                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
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

            string sql = string.Format("SELECT * FROM BSDATE WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BsdateModel");
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
                    case "BSDATE":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM BSDATE WHERE 1=0";
                        break;
                    case "BSTERM":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM BSTERM WHERE 1=0";
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
                    dt = ModelFactory.InquiryData("*", table, condition, orderBy, pageIndex, pageSize, ref recordsCount);
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

        public ActionResult TermVSChargeQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("TermVSCharges");
            return View();
        }

        public ActionResult TermVSChargeSetup(string id = null, string uid = null)
        {
            SetSchema("BSTERM");
            ViewBag.pmsList = GetBtnPms("TermVSCharges");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = uid;
            return View();
        }

        public ActionResult TermVSChargeQueryData()
        {
            string condition = GetBaseCmp();
            if ("G".Equals(UPri))
                condition = GetBaseGroup();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            condition = GetCreateDateCondition("BSTERM", condition);
            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            dt = ModelFactory.InquiryData("*", "BSTERM", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            SetNeedChgDataTable(dt);
            if (resultType == "excel")
            {
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
            //return GetBootstrapData("BSTERM", condition);
        }

        public void SetNeedChgDataTable(DataTable dt)
        {
            Dictionary<string, string> dic = new Dictionary<string, string> { { "FC", "Freight Charge"}, {"BC", "Inbound Broker Charge"}, {"TC", "Inbound Truck Charge"}, {"LC", "Inbound Local Charge"},
           {"OBC", "Outbound Broker Charge"}, {"OTC", "Outbound Truck Charge"}, {"OLC", "Outbound Local Charge"}};
            dt.Columns["NEED_CHG"].MaxLength = 200;
            foreach (DataRow dr in dt.Rows)
            {
                string val = "";
                string needChg = Prolink.Math.GetValueAsString(dr["NEED_CHG"]);
                string[] cds = needChg.Split(',');
                foreach (string cd in cds)
                {
                    if (!dic.ContainsKey(cd))
                        continue;
                    if (!string.IsNullOrEmpty(val))
                        val += ",";
                    val += dic[cd];
                } 
                dr["NEED_CHG"] = val;
            }
        }

        public ActionResult TermVSChargeUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = Request.Params["u_id"];
            if (!string.IsNullOrEmpty(uid))
                uid = HttpUtility.UrlDecode(uid);
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BstermModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            uid = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", uid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("U_ID", uid);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            uid = ei.Get("U_ID");
                            ei.PutKey("U_ID", uid);
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
            string sql = string.Format("SELECT * FROM BSTERM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BstermModel");
            return ToContent(data);
        }

        public ActionResult GetTermVSChargeDataItem()
        {
            string u_id = Request["uId"];
            string sql = string.Format("SELECT * FROM BSTERM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BstermModel");
            return ToContent(data);
        }

        public void DownLoadXls()
        {
            string strResult = string.Empty;
            string strPath = Server.MapPath("~/download");
            string trantype = Prolink.Math.GetValueAsString(Request.Params["TranType"]);
            string strName = "Excel.xls";
            switch (trantype)
            {
                case"calender":
                    strName = "BatchCalender_V1_20241124.xlsx";
                    break;
                default:
                    strName = "BatchCalender_V1_20241124.xlsx";
                    break;
            }
            string strFile = string.Format(@"{0}\{1}", strPath, strName);
            using (FileStream fs = new FileStream(strFile, FileMode.Open))
            {
                byte[] bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(strName, System.Text.Encoding.UTF8));
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
            }
        }
    }
}
