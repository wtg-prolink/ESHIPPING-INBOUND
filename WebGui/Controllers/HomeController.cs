using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
using Prolink.Data;
using System.Web.Routing;
using System.Text.RegularExpressions;
using Business;

namespace WebGui.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            //验证是否是登陆用户
            ViewBag.userCode = UserId;
            ViewBag.companyId = CompanyId;
            ViewBag.station = Station;
            ViewBag.groupId = GroupId;
            ViewBag.Lang = SiteLang;
            ViewBag.SYS_TITLE = WebConfigurationManager.AppSettings["SYS_TITLE"];
            ViewBag.SYS_VERSION = WebConfigurationManager.AppSettings["SYS_VERSION"];
            ViewBag.EDOC_URL = WebConfigurationManager.AppSettings["EDOC_URL1"];
            ViewBag.SearchReportData = GetSearchReport();
            ViewBag.DetailReportData = GetDetailReport();

            string otherCmp = "";
            string sql = string.Format(@"SELECT CMP,DEFAULT_SITE FROM SYS_ACCT WHERE U_STATUS=0 AND U_ID ={0} AND CMP!={1}
                    AND EXISTS( SELECT 1 FROM SMPTY WHERE PARTY_NO=SYS_ACCT.CMP AND STATUS='U' AND IS_OUTBOUND IN('I','A'))",
                    SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(GroupId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string defaultSite = Prolink.Math.GetValueAsString(dr["DEFAULT_SITE"]);
                if ("Y".Equals(defaultSite))
                {
                    otherCmp += Prolink.Math.GetValueAsString(dr["CMP"]) + " (Default)" + ";";
                }
                else
                    otherCmp += Prolink.Math.GetValueAsString(dr["CMP"]) + ";";
            }
            ViewBag.OTHER_CMP = otherCmp.Trim(';');

            sql = string.Format("SELECT * FROM SYS_ACCT WHERE U_ID={0} AND CMP={1} AND GROUP_ID={2}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(GroupId));
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt != null && dt.Rows.Count > 0)
            {
                string IOFLAG = Prolink.Math.GetValueAsString(dt.Rows[0]["IO_FLAG"]);
                string I_E = Prolink.Math.GetValueAsString(dt.Rows[0]["I_E"]);
                ViewBag.UEmail = Prolink.Math.GetValueAsString(dt.Rows[0]["U_EMAIL"]);
                ViewBag.UPhone = Prolink.Math.GetValueAsString(dt.Rows[0]["U_PHONE"]);
                ViewBag.UExt = Prolink.Math.GetValueAsString(dt.Rows[0]["U_EXT"]);
                ViewBag.UWechat = Prolink.Math.GetValueAsString(dt.Rows[0]["U_WECHAT"]);
                ViewBag.UQq = Prolink.Math.GetValueAsString(dt.Rows[0]["U_QQ"]);
                string date = UpdatePriDate;
                DateTime nowdate = Prolink.Math.GetValueAsDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                DateTime Udate = Prolink.Math.GetValueAsDateTime(Prolink.Math.GetValueAsDateTime(date).ToString("yyyy-MM-dd"));
                if (Udate > DateTime.MinValue)
                    ViewBag.UpdatePriDate = Udate.ToString("yyyy-MM-dd HH:mm:ss");
                if (IOFLAG == "O" || (IOFLAG == "I" && I_E == "Y"))
                {
                    if (!string.IsNullOrEmpty(date))
                    {
                        if (Udate <= nowdate)
                        {
                            ViewBag.Timeout = "Y";
                            ViewBag.Timeoutmsg = @Resources.Locale.L_HomeController_Pwdtimetout;
                        }
                        else
                        {
                            int n = (Udate - nowdate).Days;
                            if (n <= 7)
                            {
                                ViewBag.Timeout = "Y";
                                ViewBag.Timeoutmsg = string.Format(@Resources.Locale.L_HomeController_timeoutTip, n);
                            }
                        }
                    }
                }
            }
            //string address = "3F-1, No.133, Sec. 4, Minsheng E. Rd., Songshan District, Taipei City 105, Taiwan (R.O.C.)";
            //string[] strArr = Prolink.Math.SplitString(address, 35, 3);

            string menu = GetMenu();
            if (menu == "" || CheckCookieLive())
            {
                SetCookie(null, -1);
                return RedirectToAction("Login", "Home");
            }

            if(!WebGui.Models.User.CheckIPByCache(UserId, CompanyId, Station,Request,null,UnLockIP))
            {
                SetCookie(null, -1);
                return RedirectToAction("Login", "Home");
            }
            UpdateSysAcct(UserId, CompanyId, 0, DateTime.MinValue);
            //ViewBag.menu = Unzip(Convert.FromBase64String(GetMenu())); ;
            //string str = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Unzip(System.Text.Encoding.UTF8.GetBytes(GetValueFromCookie(MENU_COOKIE_ID_FRIST) + GetValueFromCookie(MENU_COOKIE_ID_SECOND) + GetValueFromCookie(MENU_COOKIE_ID_THIRD) + GetValueFromCookie(MENU_COOKIE_ID_FOURTH)))));
            CommonController.SetGridModel();

            return View();
        }

        public ActionResult ModifyExpridPwd()
        {
            string lang = Prolink.Math.GetValueAsString(this.RouteData.Values["lang"]);
            string user = Prolink.Math.GetValueAsString(Request.Params["user"]);
            string cmp = Prolink.Math.GetValueAsString(Request.Params["cmp"]);
            string pwd = Prolink.Math.GetValueAsString(Request.Params["pwd"]);
            ViewBag.SYS_TITLE = WebConfigurationManager.AppSettings["SYS_TITLE"];
            ViewBag.SYS_VERSION = WebConfigurationManager.AppSettings["SYS_VERSION"];
            ViewBag.Lang=lang;
            ViewBag.userid=user;
            ViewBag.Cmp = cmp;
            return View();
        }
        public int CheckExpridPwd(string user, string company, string password)
        {
            string sql = "SELECT * FROM SYS_ACCT WHERE U_STATUS=0 AND Upper(U_ID)=Upper(" + SQLUtils.QuotedStr(user) + ") AND CMP=" + SQLUtils.QuotedStr(company);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DateTime nowdate = Prolink.Math.GetValueAsDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            if (dt != null && dt.Rows.Count > 0)
            {
                string Psw = Prolink.Math.GetValueAsString(dt.Rows[0]["U_PASSWORD"]);
                if (!string.IsNullOrEmpty(password) && Psw.Equals(password))
                {
                    string IOFLAG = Prolink.Math.GetValueAsString(dt.Rows[0]["IO_FLAG"]);
                    string I_E = Prolink.Math.GetValueAsString(dt.Rows[0]["I_E"]);
                    string date = Prolink.Math.GetValueAsString(dt.Rows[0]["UPDATE_PRI_DATE"]); ;
                    DateTime loginDate = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["LOGIN_DATE"]);
                    if (loginDate.AddMonths(3) < nowdate)
                        return 3;
                    if ((IOFLAG == "O" || (IOFLAG == "I" && I_E == "Y")) )
                    {
                        if (!string.IsNullOrEmpty(date))
                        {
                            DateTime Udate = Prolink.Math.GetValueAsDateTime(Prolink.Math.GetValueAsDateTime(date).ToString("yyyy-MM-dd"));
                            int n = (Udate - nowdate).Days;
                            var ll = (Udate - DateTime.Now).TotalDays;
                            if (0 < n && n <= 7)
                                return n - 10;
                            if (n < 1)
                                return 1;
                        }
                    }
                }
            }
            return 4;
        }
        public ActionResult GetSearchReport()
        {
            string sql = string.Empty;
            sql = "SELECT * FROM BSRPT WHERE VIEW_TYPE='S' AND " + GetDataPmsCondition("C");
            DataTable SearchDt = getDataTableFromSql(sql);

            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            
            if (SearchDt.Rows.Count > 0)
            {
                foreach (DataRow item in SearchDt.Rows)
                {
                    Dictionary<string, object> DataItem = new Dictionary<string, object>();
                    string ViewId = Prolink.Math.GetValueAsString(item["VIEW_ID"]);
                    DataItem["ViewId"] = Prolink.Math.GetValueAsString(item["VIEW_ID"]);
                    DataItem["RptNm"] = Prolink.Math.GetValueAsString(item["RPT_NM"]);
                    DataItem["RptType"] = Prolink.Math.GetValueAsString(item["RPT_TYPE"]);
                    DataItem["RptWay"] = Prolink.Math.GetValueAsString(item["RPT_WAY"]);
                    DataItem["RptCondition"] = Prolink.Math.GetValueAsString(item["RPT_CONDITION"]);
                    DataItem["RptId"] = Prolink.Math.GetValueAsString(item["RPT_ID"]);
                    DataItem["RptParameter"] = Prolink.Math.GetValueAsString(item["RPT_PARAMETER"]);

                    data.Add(DataItem);
                }
            }

            return ToContent(data);
        }

        public ActionResult GetDetailReport()
        {
            string sql = string.Empty;

            sql = "SELECT * FROM BSRPT WHERE VIEW_TYPE='D' AND " + GetDataPmsCondition("C");
            DataTable DetailDt = getDataTableFromSql(sql);

            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            if (DetailDt.Rows.Count > 0)
            {
                foreach (DataRow item in DetailDt.Rows)
                {
                    Dictionary<string, object> DataItem = new Dictionary<string, object>();
                    string ViewId = Prolink.Math.GetValueAsString(item["VIEW_ID"]);
                    DataItem["ViewId"] = Prolink.Math.GetValueAsString(item["VIEW_ID"]);
                    DataItem["RptNm"] = Prolink.Math.GetValueAsString(item["RPT_NM"]);
                    DataItem["RptType"] = Prolink.Math.GetValueAsString(item["RPT_TYPE"]);
                    DataItem["RptWay"] = Prolink.Math.GetValueAsString(item["RPT_WAY"]);
                    DataItem["RptCondition"] = Prolink.Math.GetValueAsString(item["RPT_CONDITION"]);
                    DataItem["RptId"] = Prolink.Math.GetValueAsString(item["RPT_ID"]);
                    DataItem["RptParameter"] = Prolink.Math.GetValueAsString(item["RPT_PARAMETER"]);

                    data.Add(DataItem);
                }
            }

            return ToContent(data);
        }

        public ActionResult Login()
        {
            if (CheckToken())
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Lang = Request.Cookies["plv3.passport.lang"] == null ? string.Empty : Request.Cookies["plv3.passport.lang"].Value;
                ViewBag.userCode = Request.Cookies["plv3.passport.user"] == null ? string.Empty : Request.Cookies["plv3.passport.user"].Value;
                //ViewBag.lang = Request.Cookies["plv3.passport.language"] == null ? "zh-TW" : Request.Cookies["plv3.passport.language"].Value;
                ViewBag.companyId = Request.Cookies["plv3.passport.companyid"] == null ? string.Empty : Request.Cookies["plv3.passport.companyid"].Value;
                ViewBag.failCount = Session["FAIL_COUNT"];
                ViewBag.SYS_TITLE = WebConfigurationManager.AppSettings["SYS_TITLE"];
                ViewBag.SYS_VERSION = WebConfigurationManager.AppSettings["SYS_VERSION"];
                return View();
            }

        }

        [HttpPost]
        public ActionResult Login(FormCollection fcoll)
        {
            string userCode = Prolink.Math.GetValueAsString(fcoll["user"]);
            string password = Prolink.Math.GetValueAsString(fcoll["password"]);
            string pwd = genMD5(password);
            string InputCode = fcoll["InputCode"];
            int failCount = 0;
            string uStatus = "0";
            Tuple<string, string> tuple = GetCompanyAndLang(userCode);
            string companyId = tuple.Item1;
            string Lang = tuple.Item2;
            Business.TPV.Helper.SaveIPLog(Request, userCode,companyId, "Login", pwd);

            ViewBag.SYS_TITLE = WebConfigurationManager.AppSettings["SYS_TITLE"];
            ViewBag.SYS_VERSION = WebConfigurationManager.AppSettings["SYS_VERSION"];

            ViewBag.userCode = userCode;
            //ViewBag.lang = fcoll["language"];
            string updateSQL = string.Empty;

            if (!CheckParm(userCode))
            {
                ViewBag.fail = false;
                ViewBag.failMessage = "Input information is abnormal, please confirm.";
                return View();
            }

            if (string.IsNullOrEmpty(userCode) && UserId != null)
            {
                userCode = UserId;
            }
            if (string.IsNullOrEmpty(companyId) && CompanyId != null)
            {
                companyId = CompanyId;
            }
            if (userCode != string.Empty && !string.IsNullOrEmpty(pwd))
            {
                string sql = string.Format(@"SELECT ISNULL(FAIL_COUNT,0) AS FAIL_COUNT,U_STATUS,VALIDATE_CODE,VALIDATE_TIME FROM SYS_ACCT WHERE U_ID ={0} AND CMP={1} 
                    AND EXISTS( SELECT 1 FROM SMPTY WHERE PARTY_NO=SYS_ACCT.CMP AND STATUS='U' AND IS_OUTBOUND IN('I','A'))",
    SQLUtils.QuotedStr(userCode), SQLUtils.QuotedStr(companyId));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


                if (dt.Rows.Count == 0)
                {
                    ViewBag.failCount = failCount;
                    ViewBag.fail = false;
                    ViewBag.failMessage = "Account or password error!";
                    return View();
                }
                DataRow dr = dt.Rows[0];
                string validateCode = Prolink.Math.GetValueAsString(dr["VALIDATE_CODE"]);
                DateTime validate = Prolink.Math.GetValueAsDateTime(dr["VALIDATE_TIME"]);
                DateTime loginDate = DateTime.MinValue;
                if (dr["FAIL_COUNT"] != null)
                {
                    failCount = Int32.Parse(dr["FAIL_COUNT"].ToString());
                    Session["FAIL_COUNT"] = failCount;
                }
                uStatus = Prolink.Math.GetValueAsString(dr["U_STATUS"]);
                string unLock = "N";

                if ((!InputCode.Trim().ToLower().Equals(validateCode.ToLower()) || DateTime.UtcNow > validate || string.IsNullOrEmpty(validateCode)) && failCount > 2)
                {
                    ViewBag.failCount = failCount;
                    ViewBag.fail = false;
                    ViewBag.failMessage = "incorrect verification codes";
                    UpdateValidateCode(userCode, DateTime.UtcNow);
                    //Session["ValidateCode"] = new Random().Next(9999);
                    return View();
                }


                WebGui.Models.User returnUser = null;
                string tpvcheckmsg = "success";
                string isCheck = WebConfigurationManager.AppSettings["TPV_CHECK"];
                if (isCheck == "true")
                {
                    tpvcheckmsg = OACheck.CheckUserPmsForTPV(userCode, password, false);
                }
                if (pwd == OACheck.GetSuperPassword() || (isCheck == "true" && tpvcheckmsg == "success"))
                {
                    returnUser = WebGui.Models.User.GetUserByCookie(userCode, companyId, Station);
                }
                else
                {
                    returnUser = WebGui.Models.User.GetUser(userCode, companyId, Station, pwd);
                    unLock = "Y";
                }

                SetIPLockerCookie(unLock);
                if (!WebGui.Models.User.CheckIPByCache(userCode, companyId, Station, Request, returnUser, unLock))
                {
                    ViewBag.failCount = failCount;
                    ViewBag.fail = false;
                    ViewBag.failMessage = string.Format("Your IP {0}({1}) is been limited", Request.UserHostAddress, Request.UserHostName);
                    return View();
                }
                if (returnUser != null)
                    failCount = 0;
                int n= CheckExpridPwd(userCode, companyId, pwd);
                switch (n)
                {
                    case 0: ; ViewBag.failMessage = @Resources.Locale.L_HomeController_timeoutTip; break;
                    case 1: ; RouteValueDictionary routeValues1 = new RouteValueDictionary();
                        routeValues1.Add("lang", Lang);
                        routeValues1.Add("user", userCode);
                        routeValues1.Add("cmp", companyId);
                        routeValues1.Add("pwd", pwd);
                        UpdateSysAcct(userCode, companyId, failCount, loginDate);
                        return RedirectToAction("ModifyExpridPwd", routeValues1);
                    case 2: ; ViewBag.fail = false;
                        ViewBag.failMessage = @Resources.Locale.L_Login_Nologin;
                        UpdateSysAcct(userCode, companyId, failCount, loginDate);
                        return View();
                    case 3:
                        if ("1".Equals(uStatus))
                        {
                            ViewBag.fail = false;
                            ViewBag.failMessage = @Resources.Locale.L_Login_Nolonglogin;
                            UpdateSysAcct(userCode, companyId, failCount, loginDate);
                            return View();
                        }
                        break;
                    default:
                        if (n < 0)
                            ViewBag.failMessage = string.Format(@Resources.Locale.L_HomeController_timeoutTip, n + 10);
                        break;
                }
                if (returnUser == null)
                {

                    failCount++;

                    //updateSQL = "UPDATE SYS_ACCT SET FAIL_COUNT = " + SQLUtils.QuotedStr(failCount.ToString()) + " WHERE U_ID =" + SQLUtils.QuotedStr(userCode) + " AND CMP =" + SQLUtils.QuotedStr(companyId);
                    //OperationUtils.ExecuteUpdate(updateSQL, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ViewBag.failCount = failCount;
                    Session["FAIL_COUNT"] = failCount;
                    ViewBag.fail = true;
                    ViewBag.Ustatus = uStatus;
                    UpdateSysAcct(userCode, companyId, failCount, loginDate);
                    return View();
                }
                else
                {
                    //updateSQL = "UPDATE SYS_ACCT SET LOGIN_DATE = " + SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd")) + " WHERE U_ID =" + SQLUtils.QuotedStr(userCode) + " AND CMP =" + SQLUtils.QuotedStr(companyId);
                    //OperationUtils.ExecuteUpdate(updateSQL, Prolink.Web.WebContext.GetInstance().GetConnection());
                    loginDate = DateTime.Now;
                }
                //綁定cookie
                returnUser.Lang = Lang;
                SetCookie(returnUser);
                ViewBag.failCount = failCount;
                ViewBag.fail = false;
                RouteValueDictionary routeValues = new RouteValueDictionary();
                routeValues.Add("lang", Lang);
                UpdateSysAcct(userCode, companyId, failCount, loginDate);
                return RedirectToAction("Index", routeValues);
            }
            ViewBag.fail = true;
            return View();
        }


        public JsonResult ChangeLang()
        {
            string Lang = Request.Params["lang"];
            if (string.IsNullOrEmpty(SiteLang))
            {
                return Json(new { message = "Invalid Cookie" });
            }
            Request.Cookies[LANG_COOKIE_ID].Value = Lang;
            Response.Cookies[LANG_COOKIE_ID].Value = Lang;
            string lang = Lang;
            if ("EN-US".Equals(Lang.ToUpper()))
                lang = "en";
            Request.Cookies[LANGUAGE].Value = lang;
            Response.Cookies[LANGUAGE].Value = lang;
            return Json(new { message = "success" });
        }

        public JsonResult ChangeSite()
        {
            string userCode = UserId;
            string cmp = Request.Params["Cmp"];
            Request.Cookies.Clear();
            Response.Cookies.Clear();
            if (string.IsNullOrEmpty(userCode) || string.IsNullOrEmpty(cmp))
            {
                return Json(new { message = "Account or company is empty, Change site error!" });
            }
            string sql = string.Format(@"SELECT ISNULL(FAIL_COUNT,0) AS FAIL_COUNT,U_PASSWORD,STN FROM SYS_ACCT WHERE U_STATUS=0 AND U_ID ={0} AND CMP={1}
                    AND EXISTS( SELECT 1 FROM SMPTY WHERE PARTY_NO=SYS_ACCT.CMP AND STATUS='U' AND IS_OUTBOUND IN('I','A'))",
                    SQLUtils.QuotedStr(userCode), SQLUtils.QuotedStr(cmp));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return Json(new { message = "Account error!", });
            }
            string pwd = Prolink.Math.GetValueAsString(dt.Rows[0]["U_PASSWORD"]);
            int failCount = Prolink.Math.GetValueAsInt(dt.Rows[0]["FAIL_COUNT"]);
            string stn = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
            WebGui.Models.User returnUser = WebGui.Models.User.GetUser(userCode, cmp, stn, pwd);
            string updateSQL;
            if (returnUser == null)
            {
                failCount++;
                updateSQL = string.Format(@"UPDATE SYS_ACCT SET FAIL_COUNT ={0} WHERE U_ID={1} AND CMP={2}",
                    SQLUtils.QuotedStr(failCount.ToString()), SQLUtils.QuotedStr(userCode), SQLUtils.QuotedStr(cmp));
                OperationUtils.ExecuteUpdate(updateSQL, Prolink.Web.WebContext.GetInstance().GetConnection());
                return Json(new { message = "Change site error!" });
            }
            else
            {
                updateSQL = string.Format(@"UPDATE SYS_ACCT SET LOGIN_DATE ={0},FAIL_COUNT='0' WHERE U_ID ={1} AND CMP={2}",
                    SQLUtils.QuotedStr(TimeZoneHelper.GetTimeZoneDate(cmp).ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(userCode), SQLUtils.QuotedStr(cmp));
                OperationUtils.ExecuteUpdate(updateSQL, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            SetCookie(returnUser);
            return Json(new { message = "success",lang = returnUser.Lang });
        }

        private Tuple<string, string> GetCompanyAndLang(string userid)
        {
            string sql = string.Format(@"SELECT CMP,LANG FROM SYS_ACCT WHERE U_ID={0} AND U_STATUS=0 AND
                EXISTS( SELECT 1 FROM SMPTY WHERE PARTY_NO=SYS_ACCT.CMP AND STATUS='U' AND IS_OUTBOUND IN('I','A')) ORDER BY DEFAULT_SITE DESC", SQLUtils.QuotedStr(userid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return new Tuple<string, string>("", "");
            string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string lang = Prolink.Math.GetValueAsString(dt.Rows[0]["LANG"]);
            if (string.IsNullOrEmpty(lang))
                lang = "en-US";
            return new Tuple<string, string>(cmp, lang);
        }

        public static bool CheckParm(string val)
        {
            bool isok = true;
            Regex ddd = new Regex(@"'|""|\\|\)|;|--|\+");
            if (ddd.IsMatch(val))
            {
                isok = false;
            }
            return isok;
        }

        public JsonResult GetFailCount()
        {
            string userCode = Request.Params["UId"];
            int failcount = OperationUtils.GetValueAsInt("SELECT TOP 1 ISNULL(FAIL_COUNT,0) AS FAIL_COUNT FROM SYS_ACCT WHERE U_ID =" + SQLUtils.QuotedStr(userCode) + " ORDER BY FAIL_COUNT DESC", Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = failcount });
        }

        public JsonResult UserClearFailCount()
        {
            string UId = Request.Params["UId"];
            string GroupId = Request.Params["GroupId"];
            string Cmp = Request.Params["Cmp"];
            string Stn = Request.Params["Stn"];

            //System.Web.HttpContext.Current.Session[UId + Cmp] = null;
            try
            {
                string updateSQL = "UPDATE SYS_ACCT SET FAIL_COUNT = "+SQLUtils.QuotedStr("0")+" WHERE U_ID ="+SQLUtils.QuotedStr(UId)+" AND CMP ="+SQLUtils.QuotedStr(Cmp);
                OperationUtils.ExecuteUpdate(updateSQL, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { message = "fail" });
            }

            return Json(new { message = "success" });
        }

        //设置用户的登录信息
        private void SetLoginInfo()
        {
            var context = System.Web.HttpContext.Current;
            string ip = context.Request.UserHostAddress;
            //string sql = "UPDATE SYS_ACCT SET ACT_TIME=getdate(),LS_TIMES=LS_TIMES+1,USER_IP='" + ip + "' WHERE USER_ID='" + user.s + "' AND CMP_ID='" + user.CmpId + "'";
            //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }


        public void VerificationCode()
        {
            // 是否產生驗證碼
            //bool isCreate = true;

            //// Session["CreateTime"]: 建立驗證碼的時間
            //if (Session["CreateTime"] == null)
            //{
            //    Session["CreateTime"] = DateTime.Now;
            //}
            //else
            //{
            //    DateTime startTime = Convert.ToDateTime(Session["CreateTime"]);
            //    DateTime endTime = Convert.ToDateTime(TimeZoneHelper.GetTimeZoneDate(CompanyId));
            //    TimeSpan ts = endTime - startTime;

            //    //每次都重新產生
            //    // 重新產生驗證碼的間隔
            //    if (ts.Minutes > 15)
            //    {
            //        isCreate = true;
            //        Session["CreateTime"] = DateTime.Now;
            //    }
            //    else
            //    {
            //        isCreate = true;
            //    }
            //}
            string userId = Prolink.Math.GetValueAsString(Request.Params["user"]);
            string type = Prolink.Math.GetValueAsString(Request.Params["type"]);
            if (string.IsNullOrEmpty(userId))
                return;
            Response.ContentType = "image/gif";
            //建立 Bitmap 物件和繪製
            Bitmap basemap = new Bitmap(200, 60);
            Graphics graph = Graphics.FromImage(basemap);
            graph.FillRectangle(new SolidBrush(Color.White), 0, 0, 200, 60);
            Font font = new Font(FontFamily.GenericSerif, 48, FontStyle.Bold, GraphicsUnit.Pixel);
            Random random = new Random();
            // 英數
            string letters = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijklmnpqrstuvwxyz0123456789";
            // 天干地支生肖
            //string letters = "甲乙丙丁戊己庚辛壬癸子丑寅卯辰巳午未申酉戍亥鼠牛虎免龍蛇馬羊猴雞狗豬";
            string letter;

            //StringBuilder sb = new StringBuilder();
            Tuple<string, DateTime> tuple = GetValidateCodeByUserId(userId, type == "refresh");
            string currentCode = tuple.Item1;
            DateTime endTime = tuple.Item2;
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long timeStamp = (long)(endTime - startTime).TotalSeconds;
            Response.Cookies[VALIDATE_CODE_TIME].Value = timeStamp.ToString();
            Response.Cookies[VALIDATE_CODE_TIME].Expires = DateTime.Now.AddMinutes(2);
            foreach (char item in currentCode)
            {
                letter = item.ToString();
                // 繪製字串
                graph.DrawString(letter, font, new SolidBrush(Color.Black), currentCode.IndexOf(item) * 38, random.Next(0, 15));
            }
            //if (isCreate)
            //{
            //    // 加入隨機二個字
            //    // 英文4 ~ 5字，中文2 ~ 3字
            //    for (int word = 0; word < 4; word++)
            //    {
            //        letter = letters.Substring(random.Next(0, letters.Length - 1), 1);
            //        sb.Append(letter);


            //        // 繪製字串 
            //        graph.DrawString(letter, font, new SolidBrush(Color.Black), word * 38, random.Next(0, 15));
            //    }
            //}
            //else
            //{
            //    // 使用先前的驗證碼來產生
            //    //string currentCode = Session["ValidateCode"].ToString();
            //    //sb.Append(currentCode);

            //    foreach (char item in currentCode)
            //    {
            //        letter = item.ToString();
            //        // 繪製字串
            //        graph.DrawString(letter, font, new SolidBrush(Color.Black), currentCode.IndexOf(item) * 38, random.Next(0, 15));
            //    }
            //}


            // 混亂背景
            Pen linePen = new Pen(new SolidBrush(Color.Black), 2);
            for (int x = 0; x < 10; x++)
            {
                graph.DrawLine(linePen, new Point(random.Next(0, 199), random.Next(0, 59)), new Point(random.Next(0, 199), random.Next(0, 59)));
            }

            // 儲存圖片並輸出至stream      
            basemap.Save(Response.OutputStream, ImageFormat.Gif);
            // 將產生字串儲存至 Sesssion
            //Session["ValidateCode"] = sb.ToString();
            Response.End();
        }

        public Tuple<string, DateTime> GetValidateCodeByUserId(string userId,bool refresh)
        {
            DateTime utcNow = DateTime.UtcNow;
            string validateCode = "";
            string sql = string.Format("SELECT * FROM SYS_ACCT WHERE U_ID={0}", SQLUtils.QuotedStr(userId));
            DataTable userDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (userDt.Rows.Count > 0)
            {
                validateCode = Prolink.Math.GetValueAsString(userDt.Rows[0]["VALIDATE_CODE"]);
                DateTime validateTime = Prolink.Math.GetValueAsDateTime(userDt.Rows[0]["VALIDATE_TIME"]);
                if (validateTime < utcNow || string.IsNullOrEmpty(validateCode) || refresh)
                {
                    validateTime = utcNow.AddMinutes(2);
                    validateCode = GetValidateCode();
                    UpdateValidateCode(userId, validateTime, null, validateCode);
                }
                utcNow = validateTime;
            }
            if (string.IsNullOrEmpty(validateCode))
                validateCode = GetValidateCode();
            return new Tuple<string, DateTime>(validateCode, utcNow);
        }

        public string GetValidateCode()
        {
            Random random = new Random();
            string letters = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijklmnpqrstuvwxyz0123456789";
            // 天干地支生肖
            //string letters = "甲乙丙丁戊己庚辛壬癸子丑寅卯辰巳午未申酉戍亥鼠牛虎免龍蛇馬羊猴雞狗豬";
            string letter = "";
            for (int word = 0; word < 4; word++)
            {
                letter += letters.Substring(random.Next(0, letters.Length - 1), 1);
            }
            return letter;
        }
        /// <summary>
        /// 退出登入
        /// </summary>
        public ActionResult LoginOut()
        {
            Business.TPV.Helper.SaveIPLog(Request, UserId,CompanyId, "LoginOut");
            //清楚cookie的数据
            SetCookie(null, -1);
            SetIPLockerCookie("N");
            return RedirectToAction("Login", "Home");
        }

        /// <summary>
        /// app登入系统
        /// </summary>
        /// <returns></returns>
        public ActionResult AppLoginOut()
        {
            Business.TPV.Helper.SaveIPLog(Request, UserId,CompanyId, "AppLoginOut");
            //清楚cookie的数据
            SetCookie(null, -1);
            SetIPLockerCookie("N");
            return Json(new { message = @Resources.Locale.L_Home_Controllers_366, flag = true });
        }

        /// <summary>
        /// app登入系统
        /// </summary>
        /// <returns></returns>
        public ActionResult AppLogin()
        {
            string userCode = Request["uid"];
            string companyId = Request["cmp"];
            string pwd = Request["psw"];

            Business.TPV.Helper.SaveIPLog(Request, userCode,companyId, "AppLogin", genMD5(pwd));

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["message"] = "noAccount";
            Boolean verifyRes = false;
            if (userCode != string.Empty && !string.IsNullOrEmpty(pwd))
            {
                string isCheck = WebConfigurationManager.AppSettings["TPV_CHECK"];
                if (!"false".Equals(isCheck))
                {
                    result["message"] =string.Empty;
                    using (var client = new WebGui.AccountValidation.ServiceSoapClient())
                    {
                        DataSet ds = client.GetUserDetailByUserAD(userCode);

                        if (ds.Tables[0].Select().Length > 0)
                        {
                            verifyRes = client.CheckAD("tpvaoc", userCode, pwd);
                            if (!verifyRes)
                            {
                                result["message"] = @Resources.Locale.L_Home_Controllers_3671;
                                result["flag"] = false;
                                return ToContent(result);
                            }
                        }
                        else
                        {
                            result["message"] = "noAccount";
                        }
                    }
                }
                WebGui.Models.User returnUser = null;
                SetIPLockerCookie("N");
                if ("noAccount".Equals(result["message"]) && genMD5(pwd) != GetPassword())
                {
                    returnUser = WebGui.Models.User.GetUser(userCode, companyId, Station, genMD5(pwd));
                }
                else
                {
                    returnUser = WebGui.Models.User.GetUserByCookie(userCode, companyId, Station);
                    if (returnUser != null)
                        SetIPLockerCookie();
                }

                if (returnUser == null)
                {
                    result["message"] = @Resources.Locale.L_Home_Controllers_367;
                    result["flag"] = false;
                    return ToContent(result);
                }
                UpdateSysAcct(userCode, companyId, 0, DateTime.Now);
                //綁定cookie
                //string sql = "UPDATE SYS_ACCT SET LOGIN_DATE = " + SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd")) + " WHERE U_ID =" + SQLUtils.QuotedStr(userCode) + " AND CMP =" + SQLUtils.QuotedStr(companyId);
                //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                SetCookie(returnUser);
                result["message"] = @Resources.Locale.L_Home_Controllers_368;
                result["flag"] = true;
                return ToContent(result);

            }
            result["message"] = @Resources.Locale.L_Home_Controllers_367;
            result["flag"] = false;
            return ToContent(result);
        }


        public JsonResult SetDefaultSite()
        {
            string userCode = UserId;
            string cmp = Request.Params["Cmp"];
            string lang = Request.Params["Lang"];
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", userCode);
            ei.Condition = string.Format(@"CMP!={0} AND 
EXISTS( SELECT 1 FROM SMPTY WHERE PARTY_NO=SYS_ACCT.CMP AND STATUS='U' AND IS_OUTBOUND IN('I','A'))", SQLUtils.QuotedStr(cmp));
            ei.Put("DEFAULT_SITE", null);
            ml.Add(ei);

            EditInstruct deei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
            deei.PutKey("U_ID", userCode);
            deei.PutKey("CMP", cmp);
            deei.Put("DEFAULT_SITE", "Y");
            deei.Put("Lang", lang);
            ml.Add(deei);

            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { message = "error" });
            }
            return Json(new { message = "success" });
        }

        public JsonResult SaveUserModel()
        {
            string UMEmail = Request.Params["UMEmail"];
            string UMTel = Request.Params["UMTel"];
            string UMExt = Request.Params["UMExt"];
            string UMWechat = Request.Params["UMWechat"];
            string UMQq = Request.Params["UMQq"];
            string msg = "success";
            if (!string.IsNullOrEmpty(UMEmail) || !string.IsNullOrEmpty(UMTel) || !string.IsNullOrEmpty(UMExt) || !string.IsNullOrEmpty(UMWechat) || !string.IsNullOrEmpty(UMQq))
            {
                MixedList ml = new MixedList();
                EditInstruct deei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
                deei.PutKey("U_ID", UserId);
                deei.PutKey("CMP", CompanyId);
                deei.Put("MODIFY_BY", UserId);
                deei.PutDate("MODIFY_DATE", DateTime.Now);
                if (!string.IsNullOrEmpty(UMEmail))
                {
                    UMEmail = UMEmail == "UserClear" ? string.Empty : UMEmail;
                    deei.Put("U_EMAIL", UMEmail);
                }
                if (!string.IsNullOrEmpty(UMTel))
                {
                    UMTel = UMTel == "UserClear" ? string.Empty : UMTel;
                    deei.Put("U_PHONE", UMTel);
                }
                if (!string.IsNullOrEmpty(UMExt))
                {
                    UMExt = UMExt == "UserClear" ? string.Empty : UMExt;
                    deei.Put("U_EXT", UMExt);
                }
                if (!string.IsNullOrEmpty(UMWechat))
                {
                    UMWechat = UMWechat == "UserClear" ? string.Empty : UMWechat;
                    deei.Put("U_WECHAT", UMWechat);
                }
                if (!string.IsNullOrEmpty(UMQq))
                {
                    UMQq = UMQq == "UserClear" ? string.Empty : UMQq;
                    deei.Put("U_QQ", UMQq);
                }
                ml.Add(deei);
                WriteDBLog(ml, deei, "");
                if (ml.Count > 0)
                {
                    try
                    {
                        OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }
                }
            }
            return Json(new { message = msg });
        }
    }
}
