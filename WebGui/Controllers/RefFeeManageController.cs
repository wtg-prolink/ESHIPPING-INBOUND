using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Text.RegularExpressions;


namespace WebGui.Controllers
{
    public class RefFeeManageController : BaseController
    {
        #region View
        public ActionResult ReffeeQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("ReffeeManage");
            return View();
        }

        public ActionResult ReffeeSetupView(string id = null, string uid = null)
        {
            SetSchema("Reffee");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("ReffeeManage");
            ViewBag.Uid = id;
            return View();
        }


        public ActionResult WeightQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("QT018");
            return View();
        }

        public ActionResult WeightSetupView()
        {
            SetSchema("Reffee");
            ViewBag.pmsList = GetBtnPms("QT018");
            ViewBag.pmsList = GetBtnPms("ReffeeManage");
            ViewBag.VenderCd = Prolink.Math.GetValueAsString(Request.Params["VenderCd"]);
            return View();
        }

        public ActionResult ExpressQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("QT019");
            return View();
        }

        public ActionResult ExpressSetupView()
        {
            SetSchema("Reffee");
            ViewBag.pmsList = GetBtnPms("QT019");
            ViewBag.pmsList = GetBtnPms("ReffeeManage");
            ViewBag.VenderCd = Prolink.Math.GetValueAsString(Request.Params["VenderCd"]);
            return View();
        }

        public ActionResult InchQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IQT020");
            return View();
        }

        public ActionResult InchSetupView(string id = null, string uid = null)
        {
            SetSchema("Reffee");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("IQT020");
            ViewBag.pmsList = GetBtnPms("ReffeeManage");
            ViewBag.VenderCd = Prolink.Math.GetValueAsString(Request.Params["VenderCd"]);
            ViewBag.Uid = id;
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string con = GetDataPmsCondition("C") + " AND FEE_TYPE='O'";
            con = GetCreateDateCondition("ECREFFEE", con);
            return GetBootstrapData("ECREFFEE", con);
        }

        public ActionResult WeightQueryData()
        {
            string con = GetDataPmsCondition("C") + " AND FEE_TYPE='V'";
            con = GetCreateDateCondition("ECREFFEE", con);
            return GetBootstrapData("ECREFFEE", con);
        }

        public ActionResult ExpressQueryData()
        {
            string con = GetDataPmsCondition("G") + " AND FEE_TYPE='D'";
            con = GetCreateDateCondition("ECREFFEE", con);
            return GetBootstrapData("ECREFFEE", con );
        }

        public ActionResult InchQueryData()
        {
            string con = GetDataPmsCondition("C") + " AND FEE_TYPE='I'";
            con = GetCreateDateCondition("ECREFFEE", con);
            return GetBootstrapData("ECREFFEE", con);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM ECREFFEE WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EcreffeeModel");
            return ToContent(data);
        }

        public ActionResult WeightGetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM ECREFFEE WHERE VENDER_CD={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EcreffeeModel");
            return ToContent(data);
        }

        public ActionResult ExpressGetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM ECREFFEE WHERE VENDER_CD={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EcreffeeModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string FeeType = Prolink.Math.GetValueAsString(Request.Params["FeeType"]);

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
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "EcreffeeModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            string ChgCd = Prolink.Math.GetValueAsString(ei.Get("CHG_CD"));
                            if(string.IsNullOrEmpty(FeeType))
                                FeeType = Prolink.Math.GetValueAsString(ei.Get("CHG_CD"));
                            string[,] unikey = new string[3, 2] { { "CHG_CD", ChgCd }, { "CMP", CompanyId }, { "FEE_TYPE", FeeType } };

                            if (chkKeyIsExist("ECREFFEE", unikey) == true)
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

            string sql = string.Format("SELECT * FROM ECREFFEE WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EcreffeeModel");
            return ToContent(data);
        }

        public ActionResult WeightUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string VenderCd = Prolink.Math.GetValueAsString(Request.Params["VenderCd"]);
            string VenderNm = Prolink.Math.GetValueAsString(Request.Params["VenderNm"]);

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(VenderCd))
                VenderCd = HttpUtility.UrlDecode(VenderCd);

            if (!string.IsNullOrEmpty(VenderNm))
                VenderNm = HttpUtility.UrlDecode(VenderNm);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "EcreffeeModel");
                    string sql = "DELETE FROM ECREFFEE WHERE VENDER_CD=" + SQLUtils.QuotedStr(VenderCd) + " AND STN=" + SQLUtils.QuotedStr(Station);

                    if(VenderCd != "")
                    {
                        try
                        {
                            exeSql(sql);
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                    else
                    {
                        break;
                    }

                    int num = 30;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            num = num + 1;
                            string ChgCd = "F" + num.ToString();

                            string u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            ei.Put("VENDER_CD", VenderCd);
                            ei.Put("VENDER_NM", VenderNm);
                            ei.Put("CHG_CD", ChgCd);
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
                            ei.PutKey("VENDER_CD", VenderCd);
                            ei.PutKey("STN", Station);
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

            string sql1 = string.Format("SELECT * FROM ECREFFEE WHERE VENDER_CD={0}", SQLUtils.QuotedStr(VenderCd)) + " AND STN=" + SQLUtils.QuotedStr(Station);
            DataTable mainDt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "EcreffeeModel");
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
                    case "Reffee":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM ECREFFEE WHERE 1=0";
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

        #region 取得新代碼
        public JsonResult getChgCd()
        {
            string returnMessage = "success";
            string feetype = Prolink.Math.GetValueAsString(Request.Params["FeeType"]);

            string sql = string.Format("SELECT CHG_CD FROM ECREFFEE WHERE FEE_TYPE={0} AND CMP={1}", SQLUtils.QuotedStr(feetype), SQLUtils.QuotedStr(CompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int max = "I".Equals(feetype) ? 9 : 0;
            foreach (DataRow dr in dt.Rows)
            {
                string chgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                int num = Prolink.Math.GetValueAsInt(chgCd.Substring(1));
                if (max < num)
                    max = num;
            }
            max = max + 1;
            string ChgCd = "F" + max.ToString();

            if(max > 50)
            {
                returnMessage = @Resources.Locale.L_Refee_message;
            }

            return Json(new {message = returnMessage, ChgCd = ChgCd });
        }
        #endregion

    }
}
