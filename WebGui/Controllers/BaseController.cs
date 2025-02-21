using System;
using System.Data;
using Prolink.DataOperation;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Prolink.V3;
using System.Web.Script.Serialization;
using Prolink.Data;
using Resources;
using System.Web.Configuration;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Specialized;
using Prolink.Web;
using Prolink.Model;
using System.Text.RegularExpressions;
using Prolink.Tools;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Xml;
using Business;
using TrackingEDI.Business;

namespace WebGui.Controllers
{
    public class BaseController : Controller
    {
        //
        // GET: /Base/
        public static readonly string BASE_COOKIE_ID = "tpv.";
        public static readonly string UPLOCKIP_COOKIE_ID = BASE_COOKIE_ID + "passport.unlockip";
        public static readonly string USER_COOKIE_ID = "plv3.passport.user";
        public static readonly string COMPANYID_COOKIE_ID = "plv3.passport.companyid";
        public static readonly string BASE_COMPANYID_COOKIE_ID = "plv3.passport.basecompanyid";
        public static readonly string GROUPID_COOKIE_ID = "plv3.passport.groupid";
        public static readonly string STATION_COOKIE_ID = "plv3.passport.station";
        public static readonly string DEP_COOKIE_ID = "plv3.passport.dep";
        public static readonly string BU_COOKIE_ID = "plv3.passport.bu";
        public static readonly string UPRI_COOKIE_ID = "plv3.passport.upri";
        public static readonly string CMPPRI_COOKIE_ID = "plv3.passport.cmppri";
        public static readonly string PLANTPRI_COOKIE_ID = "plv3.passport.plantpri";
        public static readonly string EXT_COOKIE_ID = "plv3.passport.ext";
        public static readonly string BASE_STATION_COOKIE_ID = "plv3.passport.basestation";
        public static readonly string UPDATE_DATE_COOKIE_ID = "plv3.passport.update_date";
        public static readonly string ROLES_COOKIE_ID = "plv3.passport.roles";
        public static readonly string APPROVE_COOKIE_ID = "plv3.passport.apgroup";
        public static readonly string CURRCMP_COOKIE_ID = "plv3.passport.currcmp";
        public static readonly string IOFLAG_COOKIE_ID = "plv3.passport.ioflag";
        public static readonly string UTYPE_COOKIE_ID = "plv3.passport.utype";
        public static readonly string TCMP_COOKIE_ID = "plv3.passport.tcmp";

        //public static readonly string MENU_COOKIE_ID_FRIST = "plv3.passport.menu_frist";
        //public static readonly string MENU_COOKIE_ID_SECOND = "plv3.passport.menu_second";
        //public static readonly string MENU_COOKIE_ID_THIRD = "plv3.passport.menu_third";
        //public static readonly string MENU_COOKIE_ID_FOURTH = "plv3.passport.menu_fourth";
        public static readonly string TOKEN_COOKIE_ID = "plv3.passport.token";
        //public static readonly string PMS_COOKIE_ID_FRIST = "plv3.passport.pms_frist";
        //public static readonly string PMS_COOKIE_ID_SECOND = "plv3.passport.pms_second";
        //public static readonly string PMS_COOKIE_ID_THIRD = "plv3.passport.pms_third";
        //public static readonly string PMS_COOKIE_ID_FOURTH = "plv3.passport.pms_fourth";
        //public static readonly string PMS_COOKIE_ID_FIFTH = "plv3.passport.pms_fifth";
        //public static readonly string PMS_COOKIE_ID_SIXTH = "plv3.passport.pms_sixth";
        //public static readonly string PMS_COOKIE_ID_SEVENTH = "plv3.passport.pms_seventh";
        //public static readonly string PMS_COOKIE_ID_EIGHTH = "plv3.passport.pms_eighth";
        public static readonly string EXPRIES_COOKIE_ID = "plv3.passport.expries";
        public static readonly string LANG_COOKIE_ID = "plv3.passport.lang";
        public static readonly int DEFAULT_FILE_SAVE_DATE = 7;
        //public static readonly string EDTPRI_COOKIE_ID = "plv3.passport.edtpri";
        public static readonly string VALIDATE_CODE_TIME = "plv3.passport.vcodetimestamp";
        public static readonly string LANGUAGE = "language";
        public static Dictionary<string, Prolink.V6.Core.PermissionObj> pmsList;


        public static XmlDocument gridColModelXml = new XmlDocument();
       

        public string UserId
        {
            get
            {
                return GetValueFromCookie(USER_COOKIE_ID);
            }
        }

        public string Dep
        {
            get
            {

                return GetValueFromCookie(DEP_COOKIE_ID);
            }
        }

        public string Bu
        {
            get
            {

                return GetValueFromCookie(BU_COOKIE_ID);
            }
        }

        public string CmpPri
        {
            get
            {

                return GetValueFromCookie(CMPPRI_COOKIE_ID);
            }
        }

        public string PlantPri
        {
            get
            {

                return GetValueFromCookie(PLANTPRI_COOKIE_ID);
            }
        }

        public string UPri
        {
            get
            {

                return GetValueFromCookie(UPRI_COOKIE_ID);
            }
        }

        public string Ext
        {
            get
            {

                return GetValueFromCookie(EXT_COOKIE_ID);
            }
        }

        public string Station
        {
            get
            {

                return GetValueFromCookie(STATION_COOKIE_ID);
            }
        }

        public string BaseStation
        {
            get
            {

                return GetValueFromCookie(BASE_STATION_COOKIE_ID);
            }
        }

        public string GroupId
        {
            get
            {

                return GetValueFromCookie(GROUPID_COOKIE_ID);
            }
        }

        public string CompanyId
        {
            get
            {

                return GetValueFromCookie(COMPANYID_COOKIE_ID);
            }
        }
        public string BaseCompanyId
        {
            get
            {
                return GetValueFromCookie(BASE_COMPANYID_COOKIE_ID);
            }
        }

        public string UserRole
        {
            get
            {
                return GetValueFromCookie(ROLES_COOKIE_ID);
            }
        }

        public string ApproveGroup
        {
            get
            {
                return GetValueFromCookie(APPROVE_COOKIE_ID);
            }
        }

        public string CurrCmp
        {
            get
            {
                return GetValueFromCookie(CURRCMP_COOKIE_ID);
            }
        }

        public string IOFlag
        {
            get
            {
                return GetValueFromCookie(IOFLAG_COOKIE_ID);
            }
        }
        public string UType
        {
            get
            {
                return GetValueFromCookie(UTYPE_COOKIE_ID);
            }
        }

        public string TCmp
        {
            get
            {
                return GetValueFromCookie(TCMP_COOKIE_ID);
            }
        }

        public string Expries
        {
            get
            {
                return GetValueFromCookie(EXPRIES_COOKIE_ID);
            }
        }

        public string SiteLang
        {
            get
            {
                return GetValueFromCookie(LANG_COOKIE_ID);
            }
        }

        public string UpdatePriDate
        {
            get
            {
                return GetValueFromCookie(UPDATE_DATE_COOKIE_ID);
            }
        }

        public string UnLockIP
        {
             get
             {

                 return GetValueFromCookie(UPLOCKIP_COOKIE_ID);
             }
         }
        /*public string EDIPri
        {
            get
            {
                return GetValueFromCookie(EDTPRI_COOKIE_ID);
            }
        }*/
        /// <summary>
        /// 获取当前用户所在的集体公司站别的所有角色
        /// </summary>
        /// <returns></returns>
        public ActionResult Roles()
        {
            DataTable dt = OperationUtils.GetDataTable("SELECT FID,FDESCP,GROUP_ID,CMP,STN FROM SYS_ROLE WHERE (CMP=" + SQLUtils.QuotedStr(CompanyId) + " OR CMP = '*') AND (STN=" + SQLUtils.QuotedStr(Station) + " OR STN = '*') AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId)+ " ORDER BY FDESCP ASC", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;
            foreach (DataRow dr in dt.Rows)
            {
                item = new Dictionary<string, object>();
                item["RoleID"] = Prolink.Math.GetValueAsString(dr["FID"]);
                item["RoleName"] = Prolink.Math.GetValueAsString(dr["FDESCP"]);
                item["GroupId"] = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                item["Cmp"] = Prolink.Math.GetValueAsString(dr["CMP"]);
                item["Stn"] = Prolink.Math.GetValueAsString(dr["STN"]);
                list.Add(item);

            }
            return ToContent(list);
        }



        /// <summary>
        /// 获取当前用户所在的集体公司站别的所有用户
        /// </summary>
        /// <returns></returns>
        //public ActionResult Users()
        //{
        //    //if login user was * cmp or * stn , can get cross cmp or stn users
        //    string sql = "SELECT U_ID,U_NAME,GROUP_ID,CMP,STN,U_STATUS FROM SYS_ACCT WHERE IO_FLAG = 'I'  AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);

        //    if (CompanyId != "*")
        //    {
        //        sql += "AND (CMP=" + SQLUtils.QuotedStr(CompanyId) + " OR CMP = '*') ";
        //    }
        //    if (Station != "*")
        //    {
        //        sql += "AND (STN=" + SQLUtils.QuotedStr(Station) + " OR STN = '*')";
        //    }

        //    sql += "UNION ALL SELECT U_ID,U_NAME,GROUP_ID,CMP,STN,U_STATUS FROM SYS_ACCT WHERE IO_FLAG = 'O' AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);

        //    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        //    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        //    Dictionary<string, object> item = null;
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        item = new Dictionary<string, object>();
        //        item["UserID"] = Prolink.Math.GetValueAsString(dr["U_ID"]);
        //        item["UserName"] = Prolink.Math.GetValueAsString(dr["U_NAME"]);
        //        item["GroupId"] = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
        //        item["Cmp"] = Prolink.Math.GetValueAsString(dr["CMP"]);
        //        item["Stn"] = Prolink.Math.GetValueAsString(dr["STN"]);
        //        item["UStatus"] = Prolink.Math.GetValueAsString(dr["U_STATUS"]);
        //        list.Add(item);
        //    }
        //    return ToContent(list);
        //}
        //public ActionResult GetAllUser()
        //{
        //    DataTable dt = OperationUtils.GetDataTable("SELECT U_ID,U_NAME FROM SYS_ACCT", null, Prolink.Web.WebContext.GetInstance().GetConnection());
        //    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        //    Dictionary<string, object> item = null;
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        item = new Dictionary<string, object>();
        //        item["UserID"] = Prolink.Math.GetValueAsString(dr["U_ID"]);
        //        item["UserName"] = Prolink.Math.GetValueAsString(dr["U_NAME"]);
        //        list.Add(item);
        //    }
        //    return ToContent(list);
        //}


        public static ActionResult ToContent(object obj)
        {
            JavaScriptSerializer jsSeri = new JavaScriptSerializer();
            string str = string.Empty;
            try
            {
                jsSeri.MaxJsonLength = 1024 * 1024;
                str = jsSeri.Serialize(obj);
            }
            catch (System.InvalidOperationException e)
            {
                if (e != null && e.Message != null && e.Message.ToLower().Contains("max"))
                {
                    jsSeri.MaxJsonLength = 10 * 1024 * 1024;
                    str = jsSeri.Serialize(obj);
                }
            }
            ContentResult result = new ContentResult();
            result.Content = str;
            return result;
        }

        public void ChangeCurrCmp(string cmp)
        {
            Response.Cookies[CURRCMP_COOKIE_ID].Value = cmp;
            Response.Cookies[CURRCMP_COOKIE_ID].Expires = DateTime.Now.AddDays(7);
        }

        public void SetCookie(WebGui.Models.User returnUser, int days)
        {
            //string str = Regex.Split("11111AA1212QQ1111", @".*[\D](\d*$)").Last();
            bool getValue = true;
            if (returnUser == null)
            {
                returnUser = new Models.User("", "", "", "");
                getValue = false;
            }
            if (days != -1)
            {
                if (Request.Cookies[LANG_COOKIE_ID] != null)
                {
                    Request.Cookies[LANG_COOKIE_ID].Value = returnUser.Lang;
                }
                Response.Cookies[LANG_COOKIE_ID].Value = returnUser.Lang;
                Response.Cookies[LANG_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
                string lang = returnUser.Lang;
                if ("EN-US".Equals(returnUser.Lang.ToUpper()))
                    lang = "en";
                
                if (Request.Cookies[LANGUAGE] != null)
                {
                    Request.Cookies[LANGUAGE].Value = lang;
                }
                Response.Cookies[LANGUAGE].Value = lang;
                Response.Cookies[LANGUAGE].Expires = DateTime.Now.AddDays(days);
            }

            //get base cmp & stn

            Response.Cookies[BASE_COMPANYID_COOKIE_ID].Value = string.IsNullOrEmpty(returnUser.BaseCmp) ? returnUser.GetCompany() : returnUser.BaseCmp;
            Response.Cookies[BASE_COMPANYID_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
            Response.Cookies[BASE_STATION_COOKIE_ID].Value = string.IsNullOrEmpty(returnUser.BaseCmp) ? returnUser.GetStation() : "*";
            Response.Cookies[BASE_STATION_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[CURRCMP_COOKIE_ID].Value = "ALL";
            Response.Cookies[CURRCMP_COOKIE_ID].Expires = DateTime.Now.AddDays(days);


            Response.Cookies[USER_COOKIE_ID].Value = returnUser.GetId();
            Response.Cookies[USER_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
            Response.Cookies[COMPANYID_COOKIE_ID].Value = returnUser.GetCompany();
            Response.Cookies[COMPANYID_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
            Response.Cookies[GROUPID_COOKIE_ID].Value = returnUser.GetGroupId();
            Response.Cookies[GROUPID_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
            Response.Cookies[STATION_COOKIE_ID].Value = returnUser.GetStation();
            Response.Cookies[STATION_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[DEP_COOKIE_ID].Value = returnUser.dep;
            Response.Cookies[DEP_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[BU_COOKIE_ID].Value = returnUser.bu;
            Response.Cookies[BU_COOKIE_ID].Expires = DateTime.Now.AddDays(days);


            Response.Cookies[EXT_COOKIE_ID].Value = returnUser.ext;
            Response.Cookies[EXT_COOKIE_ID].Expires = DateTime.Now.AddDays(days);


            Response.Cookies[UPRI_COOKIE_ID].Value = returnUser.uPri;
            Response.Cookies[UPRI_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[CMPPRI_COOKIE_ID].Value = returnUser.cmpPri;
            Response.Cookies[CMPPRI_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[PLANTPRI_COOKIE_ID].Value = returnUser.plantPri;
            Response.Cookies[PLANTPRI_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[ROLES_COOKIE_ID].Value = JsonConvert.SerializeObject(returnUser.GetRoles());
            Response.Cookies[ROLES_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[IOFLAG_COOKIE_ID].Value = returnUser.IOFlag;
            Response.Cookies[IOFLAG_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[UTYPE_COOKIE_ID].Value = returnUser.UType;
            Response.Cookies[UTYPE_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            Response.Cookies[TCMP_COOKIE_ID].Value = returnUser.TCmp;
            Response.Cookies[TCMP_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
            //for TPV
            /*if (IOFlag == "O")
            {
                Response.Cookies[EDTPRI_COOKIE_ID].Value = GetPartyEdtPri(returnUser.GetCompany());
                Response.Cookies[EDTPRI_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
            }*/

            //TODO SET UPDATE DATE INTO COOKIE
            Response.Cookies[UPDATE_DATE_COOKIE_ID].Value = returnUser.updatePriDate;//returnUser.GetUpdateDate().ToString();
            Response.Cookies[UPDATE_DATE_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            if (getValue)
            {
                Business.CommonHelp.RebuildPermission(returnUser.GetGroupId(), returnUser.GetCompany(), returnUser.GetStation());
                //string menuStr = GenMenu(returnUser.GetId(), returnUser.GetGroupId(), returnUser.GetCompany(), returnUser.GetStation());
                //int menuLength = menuStr.Length;
                //int half = menuLength / 4;
                //Response.Cookies[MENU_COOKIE_ID_FRIST].Value = menuStr.Substring(0, half);
                //Response.Cookies[MENU_COOKIE_ID_SECOND].Value = menuStr.Substring(half, half);
                //Response.Cookies[MENU_COOKIE_ID_THIRD].Value = menuStr.Substring(half * 2, half);
                //Response.Cookies[MENU_COOKIE_ID_FOURTH].Value = menuStr.Substring(half * 3, half);

                string pmsStr = this.GetPermissionObj(returnUser);
                SetDBCookie(UserId, GroupId, CompanyId, pmsStr);
                int pmsLength = pmsStr.Length;
                int pmsHalf = pmsLength / 8;

                //Response.Cookies[PMS_COOKIE_ID_FRIST].Value = pmsStr.Substring(0, pmsHalf);
                //Response.Cookies[PMS_COOKIE_ID_SECOND].Value = pmsStr.Substring(pmsHalf, pmsHalf);
                //Response.Cookies[PMS_COOKIE_ID_THIRD].Value = pmsStr.Substring(pmsHalf * 2, pmsHalf);
                //Response.Cookies[PMS_COOKIE_ID_FOURTH].Value = pmsStr.Substring(pmsHalf * 3, pmsHalf);
                //Response.Cookies[PMS_COOKIE_ID_FIFTH].Value = pmsStr.Substring(pmsHalf * 4, pmsHalf);
                //Response.Cookies[PMS_COOKIE_ID_SIXTH].Value = pmsStr.Substring(pmsHalf * 5, pmsHalf);
                //Response.Cookies[PMS_COOKIE_ID_SEVENTH].Value = pmsStr.Substring(pmsHalf * 6, pmsHalf);
                //Response.Cookies[PMS_COOKIE_ID_EIGHTH].Value = pmsStr.Substring(pmsHalf * 7, pmsHalf);

                Response.Cookies[APPROVE_COOKIE_ID].Value = genApproveGroup();
            }
            else
            {
                //Response.Cookies[MENU_COOKIE_ID_FRIST].Value = null;
                //Response.Cookies[MENU_COOKIE_ID_SECOND].Value = null;
                //Response.Cookies[MENU_COOKIE_ID_THIRD].Value = null;
                //Response.Cookies[MENU_COOKIE_ID_FOURTH].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_FRIST].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_SECOND].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_THIRD].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_FOURTH].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_FIFTH].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_SIXTH].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_SEVENTH].Value = null;
                //Response.Cookies[PMS_COOKIE_ID_EIGHTH].Value = null;
                Response.Cookies[APPROVE_COOKIE_ID].Value = null;
            }

            //Response.Cookies[MENU_COOKIE_ID_FRIST].Expires = DateTime.Now.AddDays(days);
            //Response.Cookies[MENU_COOKIE_ID_SECOND].Expires = DateTime.Now.AddDays(days);
            //Response.Cookies[MENU_COOKIE_ID_THIRD].Expires = DateTime.Now.AddDays(days);
            //Response.Cookies[MENU_COOKIE_ID_FOURTH].Expires = DateTime.Now.AddDays(days);
            //Response.Cookies[PMS_COOKIE_ID_FRIST].Expires = DateTime.Now.AddDays(days);
            //Response.Cookies[PMS_COOKIE_ID_SECOND].Expires = DateTime.Now.AddDays(days);
            //Response.Cookies[PMS_COOKIE_ID_THIRD].Expires = DateTime.Now.AddDays(days);
            //Response.Cookies[PMS_COOKIE_ID_FOURTH].Expires = DateTime.Now.AddDays(days);
            Response.Cookies[APPROVE_COOKIE_ID].Expires = DateTime.Now.AddDays(days);


            string salt = WebConfigurationManager.AppSettings["salt"];
            //string data = Response.Cookies[MENU_COOKIE_ID_FRIST].Value + Response.Cookies[MENU_COOKIE_ID_SECOND].Value + Response.Cookies[MENU_COOKIE_ID_THIRD].Value + Response.Cookies[MENU_COOKIE_ID_FOURTH].Value + Response.Cookies[ROLES_COOKIE_ID].Value + Response.Cookies[USER_COOKIE_ID].Value + Response.Cookies[UPDATE_DATE_COOKIE_ID].Value;// +Response.Cookies[UPDATE_DATE_COOKIE_ID].Value;
            string data = Response.Cookies[ROLES_COOKIE_ID].Value + Response.Cookies[USER_COOKIE_ID].Value + Response.Cookies[UPDATE_DATE_COOKIE_ID].Value;// +Response.Cookies[UPDATE_DATE_COOKIE_ID].Value;

            string token = Prolink.Math.CalculateChecksum(data, salt);
            Response.Cookies[TOKEN_COOKIE_ID].Value = token;
            Response.Cookies[TOKEN_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            String apCd = OperationUtils.GetValueAsString("SELECT ISNULL(AP_CD,1) AS AP_CD FROM BSCODE WHERE CD=" + SQLUtils.QuotedStr("COOKIE") + " AND CD_TYPE='SYS' ", Prolink.Web.WebContext.GetInstance().GetConnection());

            if (String.IsNullOrEmpty(apCd))
            {
                apCd = "1";
            }
            Response.Cookies[EXPRIES_COOKIE_ID].Value = DateTime.Now.AddHours(Double.Parse(apCd)).ToString("yyyyMMddHHmmss");
            Response.Cookies[EXPRIES_COOKIE_ID].Expires = DateTime.Now.AddDays(days);

            SetCookieSecure();
        }

        private void SetCookieSecure()
        {
            bool isset = true;
            bool httpOnly = false;
            string salt = WebConfigurationManager.AppSettings["HTTPSECURE"];
            if ("N".Equals(salt))
                isset = false;

            Response.Cookies[LANG_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[LANG_COOKIE_ID].Secure = isset;
            Response.Cookies[LANGUAGE].HttpOnly = httpOnly;
            Response.Cookies[LANGUAGE].Secure = isset;
            Response.Cookies[BASE_COMPANYID_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[BASE_COMPANYID_COOKIE_ID].Secure = isset;
            Response.Cookies[BASE_STATION_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[BASE_STATION_COOKIE_ID].Secure = isset;
            Response.Cookies[CURRCMP_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[CURRCMP_COOKIE_ID].Secure = isset;
            Response.Cookies[USER_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[USER_COOKIE_ID].Secure = isset;
            Response.Cookies[COMPANYID_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[COMPANYID_COOKIE_ID].Secure = isset;
            Response.Cookies[GROUPID_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[GROUPID_COOKIE_ID].Secure = isset;
            Response.Cookies[STATION_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[STATION_COOKIE_ID].Secure = isset;
            Response.Cookies[DEP_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[DEP_COOKIE_ID].Secure = isset;
            Response.Cookies[BU_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[BU_COOKIE_ID].Secure = isset;
            Response.Cookies[EXT_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[EXT_COOKIE_ID].Secure = isset;
            Response.Cookies[UPRI_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[UPRI_COOKIE_ID].Secure = isset;
            Response.Cookies[CMPPRI_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[CMPPRI_COOKIE_ID].Secure = isset;
            //Response.Cookies[MENU_COOKIE_ID_FRIST].HttpOnly = httpOnly;
            //Response.Cookies[MENU_COOKIE_ID_FRIST].Secure = isset;
            //Response.Cookies[MENU_COOKIE_ID_SECOND].HttpOnly = httpOnly;
            //Response.Cookies[MENU_COOKIE_ID_SECOND].Secure = isset;
            //Response.Cookies[MENU_COOKIE_ID_THIRD].HttpOnly = httpOnly;
            //Response.Cookies[MENU_COOKIE_ID_THIRD].Secure = isset;
            //Response.Cookies[MENU_COOKIE_ID_FOURTH].HttpOnly = httpOnly;
            //Response.Cookies[MENU_COOKIE_ID_FOURTH].Secure = isset;
            Response.Cookies[BU_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[BU_COOKIE_ID].Secure = isset;
            Response.Cookies[ROLES_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[ROLES_COOKIE_ID].Secure = isset;
            Response.Cookies[IOFLAG_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[IOFLAG_COOKIE_ID].Secure = isset;
            Response.Cookies[UTYPE_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[UTYPE_COOKIE_ID].Secure = isset;
            Response.Cookies[TCMP_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[TCMP_COOKIE_ID].Secure = isset;
            Response.Cookies[UPDATE_DATE_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[UPDATE_DATE_COOKIE_ID].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_FRIST].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_FRIST].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_SECOND].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_SECOND].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_THIRD].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_THIRD].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_FOURTH].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_FOURTH].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_FIFTH].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_FIFTH].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_SIXTH].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_SIXTH].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_SEVENTH].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_SEVENTH].Secure = isset;
            //Response.Cookies[PMS_COOKIE_ID_EIGHTH].HttpOnly = httpOnly;
            //Response.Cookies[PMS_COOKIE_ID_EIGHTH].Secure = isset;
            Response.Cookies[APPROVE_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[APPROVE_COOKIE_ID].Secure = isset;
            Response.Cookies[TOKEN_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[TOKEN_COOKIE_ID].Secure = isset;
            Response.Cookies[EXPRIES_COOKIE_ID].HttpOnly = httpOnly;
            Response.Cookies[EXPRIES_COOKIE_ID].Secure = isset;
        }

        public string GetPermissionObj(WebGui.Models.User returnUser)
        {
            string userId = returnUser.GetId();
            string stn = Station;
            string cmp = CompanyId;

            Dictionary<string, object> pmsList = MenuManager.GetUserPermissionObj(Prolink.Web.WebContext.GetInstance(), userId, cmp, stn);
            return Convert.ToBase64String(Zip(HttpUtility.UrlDecode(JsonConvert.SerializeObject(pmsList).Trim())));

        }

        public Boolean CheckToken()
        {
            string oriToken = GetValueFromCookie(TOKEN_COOKIE_ID);
            string salt = WebConfigurationManager.AppSettings["salt"];
            string userId = UserId;
            string stn = Station;
            string cmp = CompanyId;

            WebGui.Models.User cookieUser = WebGui.Models.User.GetUserByCookie(userId, cmp, stn);
            if (cookieUser == null)
            {
                return false;
            }
            
            if (!WebGui.Models.User.CheckIPByCache(userId, cmp, stn,Request, cookieUser,UnLockIP))
            {
                return false;
            }
            UpdateSysAcct(userId, cmp, 0, DateTime.MinValue);
            //string data = GenMenu() + JsonConvert.SerializeObject(cookieUser.GetRoles()) + userId + cookieUser.updatePriDate;// +Request.Cookies[UPDATE_DATE_COOKIE_ID].Value;
            string data = JsonConvert.SerializeObject(cookieUser.GetRoles()) + userId + cookieUser.updatePriDate;// +Request.Cookies[UPDATE_DATE_COOKIE_ID].Value;
            string newToken = Prolink.Math.CalculateChecksum(data, salt);

            if (oriToken.Equals(newToken))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateSysAcct(string uid, string cmp, int failCount, DateTime loginDate)
        {
            MixedList ml = new MixedList();
            EditInstruct deei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
            deei.PutKey("U_ID", uid);
            deei.PutKey("CMP", cmp);
            deei.Put("FAIL_COUNT", failCount);
            if (loginDate > DateTime.MinValue)
                deei.PutDate("LOGIN_DATE", loginDate);
            ml.Add(deei);
            UpdateValidateCode(uid, DateTime.UtcNow, ml);
            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateValidateCode(string uid, DateTime utcNow, MixedList ml = null, string validateCode = "")
        {
            EditInstruct deei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
            deei.PutKey("U_ID", uid);
            if (!string.IsNullOrEmpty(validateCode))
            {
                deei.Put("VALIDATE_CODE", validateCode);
                deei.PutDate("VALIDATE_TIME", utcNow);
            }
            else
            {
                deei.Put("VALIDATE_CODE", DBNull.Value);
                deei.Put("VALIDATE_TIME", DBNull.Value);
            }
            if (ml == null)
            {
                OperationUtils.ExecuteUpdate(deei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else
            {
                ml.Add(deei);
            }
        }


        public Boolean CheckCookieLive()
        {
            if (string.IsNullOrEmpty(Expries))
            {
                return true;
            }

            DateTime expriesTime = DateTime.ParseExact(Expries, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime now = DateTime.Now;
   
            if (now > expriesTime)
            {
                return true;
            }
            else
            {
                String apCd = OperationUtils.GetValueAsString("SELECT ISNULL(AP_CD,1) AS AP_CD FROM BSCODE WHERE CD=" + SQLUtils.QuotedStr("COOKIE") + " AND CD_TYPE='SYS' ", Prolink.Web.WebContext.GetInstance().GetConnection());

                if (String.IsNullOrEmpty(apCd))
                {
                    apCd = "1";
                }
                Response.Cookies[EXPRIES_COOKIE_ID].Value = DateTime.Now.AddHours(Double.Parse(apCd)).ToString("yyyyMMddHHmmss");
                return false;
            }

        }

        /*public string GetPartyEdtPri(string partyNo)
        {
            string sql = "SELECT PARTY_TYPE FROM SMPTY WHERE PARTY_NO = " + SQLUtils.QuotedStr(partyNo);


            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }*/

        public string GetMenu()
        {
            //return Unzip(Convert.FromBase64String(GetValueFromCookie(MENU_COOKIE_ID_FRIST) + GetValueFromCookie(MENU_COOKIE_ID_SECOND) + GetValueFromCookie(MENU_COOKIE_ID_THIRD) + GetValueFromCookie(MENU_COOKIE_ID_FOURTH)));
            return Unzip(Convert.FromBase64String(GenMenu(UserId, GroupId, CompanyId, Station)));
        }

        public string GetPMS()
        {
            //return Unzip(Convert.FromBase64String(GetValueFromCookie(PMS_COOKIE_ID_FRIST) 
            //    + GetValueFromCookie(PMS_COOKIE_ID_SECOND) 
            //    + GetValueFromCookie(PMS_COOKIE_ID_THIRD) 
            //    + GetValueFromCookie(PMS_COOKIE_ID_FOURTH) 
            //    + GetValueFromCookie(PMS_COOKIE_ID_FIFTH) 
            //    + GetValueFromCookie(PMS_COOKIE_ID_SIXTH) 
            //    + GetValueFromCookie(PMS_COOKIE_ID_SEVENTH) 
            //    + GetValueFromCookie(PMS_COOKIE_ID_EIGHTH)));
            string sql = string.Format("SELECT PMS FROM SYS_COOKIE WHERE U_ID={0} AND GROUP_ID={1} AND CMP={2}",
                SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            string pmsstr = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Unzip(Convert.FromBase64String(pmsstr));
        }

        public void SetDBCookie(string user, string group, string cmp, string pms)
        {
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SYS_COOKIE", EditInstruct.DELETE_OPERATION);
            ei.PutKey("U_ID", user);
            ei.PutKey("GROUP_ID", group);
            ei.PutKey("CMP", cmp);

            ml.Add(ei);
            EditInstruct ei2 = new EditInstruct("SYS_COOKIE", EditInstruct.INSERT_OPERATION);
            ei2.Put("U_ID", user);
            ei2.Put("GROUP_ID", group);
            ei2.Put("CMP", cmp);
            ei2.Put("PMS", pms);
            ei2.PutDate("EFFECT_DATE", DateTime.Now);
            ml.Add(ei2);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
            }
        }

        public string GetAPG()
        {
            return Unzip(Convert.FromBase64String(GetValueFromCookie(APPROVE_COOKIE_ID)));
        }

        public string GenMenu(string userId="",string groupId="",string cmp="",string stn="")
        {
            /*return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(HttpUtility.UrlDecode(JsonConvert.SerializeObject(MenuManager.GetPermissionMenu(Prolink.Web.WebContext.GetInstance(),
                WebGui.Models.User.GetUserRoles(UserId, Station, CompanyId, GroupId).ToArray(),
                Resources.Locale.ResourceManager)).Trim())));*/
            /*
            string encodeStr = JsonConvert.SerializeObject(MenuManager.GetPermissionMenu(Prolink.Web.WebContext.GetInstance(),
               WebGui.Models.User.GetUserRoles(UserId, Station, CompanyId, GroupId).ToArray(),
               Resources.Locale.ResourceManager)).Trim();
            encodeStr = Convert.ToBase64String(SmazSharp.Smaz.Compress(encodeStr));
            */
            string menuName = "";
            switch(SiteLang)
            {
                case "zh-TW":
                    menuName = "BIG5_MENU.xml";
                    break;
                case "en-US":
                    menuName = "ENG_MENU.xml";
                    break;
                case "zh-CN":
                    menuName = "GB_MENU.xml";
                    break;
                case "ru-RU":
                    menuName = "RU_MENU.xml";
                    break;
                default:
                    menuName = "GB_MENU.xml";
                    break;
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = UserId;
                groupId = GroupId;
                cmp = CompanyId;
                stn = Station;
            }

            string encodeStr = Convert.ToBase64String(Zip(HttpUtility.UrlDecode(JsonConvert.SerializeObject(MenuManager.GetPermissionMenu(Prolink.Web.WebContext.GetInstance(),
                WebGui.Models.User.GetUserRoles(userId, stn, cmp, groupId).ToArray(),
                Resources.Locale.ResourceManager, menuName)).Trim())));
            return encodeStr;



        }
        public string genApproveGroup()
        {

            Dictionary<string, object> groups = WebGui.Models.User.GetUserApproveGroup(UserId, Station, CompanyId, GroupId);
            return Convert.ToBase64String(Zip(HttpUtility.UrlDecode(JsonConvert.SerializeObject(groups).Trim())));
        }

        public void SetCookie(WebGui.Models.User returnUser)
        {
            SetCookie(returnUser, 7);
        }
        
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
        //实现md5加密算法
        public string MD5Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
            return System.Text.Encoding.Default.GetString(result);
        }

        public string GetValueFromCookie(string cookieKey)
        {
            string value = "";
            if (Request.Cookies[cookieKey] != null)
            {
                value = Request.Cookies[cookieKey].Value;
            }
            return value;
        }

        public ActionResult GetSchemaByModelName(string modelName)
        {
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetModelSchema(modelName);
            return ToContent(schemas);
        }

        public void SetOpitonForSelect(string[] type, string order = "")
        {
            Dictionary<string, object> option = GetSelectsByBscodeToDic(type, "", "", order);
            if (option == null) return;
            foreach (var n in option)
            {
                if (n.Value == null) ViewData[n.Key] = "";
                else
                    ViewData[n.Key + "M"] = ObjectToJson(n.Value);
            }
        }

        public Dictionary<string, object> GetSelectsByBscodeToDic(string[] types, string baseLocation = "", string sql = "", string order = "")
        {
            if (types.Length <= 0) return null;
            if (string.IsNullOrEmpty(sql))
                sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE  FROM BSCODE WHERE {0} AND CD_TYPE IN {1} GROUP BY CD,CD_DESCP,CD_TYPE ", GetBaseCmp(), SQLUtils.Quoted(types));
            if (!string.IsNullOrEmpty(order))
            {
                sql += order;
            }
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return null;
            Dictionary<string, object> option = null;
            Dictionary<string, object> options = new Dictionary<string, object>();
            List<object> list = null;
            string cd, cdDescp, cdType;
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]).Trim();
                option = new Dictionary<string, object>();
                option["cdDescp"] = cdDescp;
                option["cd"] = cd;
                if (!options.ContainsKey(cdType))
                    options[cdType] = new List<object>();
                list = options[cdType] as List<object>;
                list.Add(option);
            }
            return options;
        }


        public static string ObjectToJson(object obj)
        {
            JavaScriptSerializer jsSeri = new JavaScriptSerializer();

            string str = null;
            try
            {
                jsSeri.MaxJsonLength = 1024 * 1024;
                str = jsSeri.Serialize(obj);
            }
            catch (System.InvalidOperationException e)
            {
                if (e != null && e.Message != null && e.Message.ToLower().Contains("max"))
                {
                    jsSeri.MaxJsonLength = 50 * 1024 * 1024;
                    str = jsSeri.Serialize(obj);
                }
            }
            return str;
        }


        public string GetExcelRenderer(DataTable dt, NameValueCollection nameValues, string columnListStr="")
        {
            if (string.IsNullOrEmpty(columnListStr))
            {
                columnListStr = nameValues["columnList"];
                if (!string.IsNullOrEmpty(columnListStr))
                    columnListStr = System.Web.HttpUtility.UrlDecode(columnListStr);
            }
            string title = Prolink.Math.GetValueAsString(nameValues["reportTitle"]);
            string excelName = Prolink.Math.GetValueAsString(nameValues["excelName"]);
            string totalCloumnStr = nameValues["totalColumns"];
            string[] totalColumns = new string[] { };
            if (!string.IsNullOrEmpty(totalCloumnStr))
                totalColumns = totalCloumnStr.Split(';');

            JavaScriptSerializer js = new JavaScriptSerializer();
            //Dictionary<string, ArrayList> columns = js.Deserialize<Dictionary<string, ArrayList>>(columnListStr);
            object[] items = js.DeserializeObject(columnListStr) as object[];
            ColumnList columnList = new ColumnList();
            FieldSet result = new FieldSet("");
            //foreach (var list in columns)
            //{
            //foreach (Dictionary<string, object> item in list.Value)
            Column col;
            foreach (Dictionary<string, object> item in items)
            {

                if (item == null)
                {
                    continue;
                }
                string fieldName = Prolink.Math.GetValueAsString(item["name"]);
                fieldName = ModelFactory.ReplaceFiledToDBName(fieldName);
                string caption = Prolink.Math.GetValueAsString(item["caption"]);
                int width = 100;
                if (item.ContainsKey("width"))
                {
                    width = Prolink.Math.GetValueAsInt(item["width"]);
                }
                string alignment = "right";
                if (item.ContainsKey("align"))
                {
                    alignment = Prolink.Math.GetValueAsString(item["align"]);
                }
                col = new Column(fieldName, caption, alignment, width);
                int thisType = 0;
                int thisLength = 1000;
                int thisScale = 0;
                //添加日期格式以及代码/描述转换功能
                if (item.ContainsKey("formatter"))
                {
                    string formatter = Prolink.Math.GetValueAsString(item["formatter"]);

                    if (formatter == "date")
                    {
                        Dictionary<string, object> newformat = item["formatoptions"] as Dictionary<string, object>;
                        IEnumerator ie = newformat.GetEnumerator();
                        while (ie.MoveNext())
                        {
                            KeyValuePair<string, object> kv = (KeyValuePair<string, object>)ie.Current;
                            string value = Prolink.Math.GetValueAsString(kv.Value);
                            switch (value)
                            {
                                case "Y-m-d":
                                    {
                                        col.DisplayFormat = "yyyy-MM-dd";
                                        break;
                                    }//目前仅mapping一种格式，此处往下可以继续写case种类
                                case "Y-m-d H:i":
                                    {
                                        col.DisplayFormat = "yyyy/MM/dd HH:mm";
                                        col.StoreFormat = "YYYYMMDDHHMMSS";
                                        break;
                                    }//目前仅mapping一种格式，此处往下可以继续写case种类
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                    }
                    else if (formatter == "select")
                    {
                        Dictionary<string, object> editoptions = item["editoptions"] as Dictionary<string, object>;
                        IEnumerator ie = editoptions.GetEnumerator();
                        while (ie.MoveNext())
                        {
                            KeyValuePair<string, object> kv = (KeyValuePair<string, object>)ie.Current;
                            string value = Prolink.Math.GetValueAsString(kv.Value);
                            col.ValueList = value.Replace(":", "=");
                        }
                    }
                    else if (formatter == "number" || formatter == "currency" || formatter == "float")
                    {
                        thisType = Field.FLOAT_TYPE;
                        if (item.ContainsKey("formatoptions"))
                        {
                            Dictionary<string, object> formatoptions = item["formatoptions"] as Dictionary<string, object>;
                            IEnumerator ie = formatoptions.GetEnumerator();
                            while (ie.MoveNext())
                            {
                                KeyValuePair<string, object> kv = (KeyValuePair<string, object>)ie.Current;
                                if (kv.Key == "decimalPlaces")
                                {
                                    thisScale = Prolink.Math.GetValueAsInt(kv.Value);
                                }
                            }
                        }
                    }
                    else if (formatter == "int" || formatter == "integer")
                    {
                        thisType = Field.INTEGER_TYPE;
                    }
                }
                col.TitleGroundColor = item.ContainsKey("titlegroundcolor") ? Prolink.Math.GetValueAsString(item["titlegroundcolor"]) : string.Empty; ;
                columnList.Add(col);

                if (item.ContainsKey("maxLength"))
                {
                    thisLength = Prolink.Math.GetValueAsInt(item["maxLength"]);
                }
                result.Add(new Field(fieldName)
                {
                    Type = thisType,
                    Length = thisLength,
                    Scale = thisScale
                });
            }
            //}


            TableExcelRenderer tableExcelRenderer = new TableExcelRenderer(dt, columnList, totalColumns, title);
            if (excelName == "null")
            {
                tableExcelRenderer.ReportName = System.Guid.NewGuid().ToString();
            }
            else
            {
                tableExcelRenderer.ReportName = excelName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            tableExcelRenderer.FieldSet = result;

            var reportPath = string.Format("{0}{1}.xlsx", System.IO.Path.GetTempPath(), tableExcelRenderer.ReportName);
            string xlsFile = tableExcelRenderer.CreateExcelFile(reportPath);
            return xlsFile;
        }

        public FileResult ExportExcelFile(DataTable dt, string columnListStr = "")
        {
            string xlsFile = GetExcelRenderer(dt, Request.Params, columnListStr);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", regex.Match(xlsFile).Value);
        }

        public string DictionaryToJson(Dictionary<string, object> dict)
        {
            var entries = dict.Select(d =>
                string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));
            return "{" + string.Join(",", entries) + "}";
        }

        public string ChangeNoticeContent(string id, string refNo, string allFields, string noticeDate, string approveCode, string boss, string isDelete, Dictionary<string, object> dict = null)
        {

            string ApproveModifyBy = "";
            string ApproveRole = "";
            string SysCode = "";
            string ProgCode = "";
            int DayNum = 0;
            string Role = "";
            string NoticeType = "";
            string NoticeSubject = "";
            string NoticeContent = "";
            string GroupId = "";
            string Stn = "";
            string Cmp = "";

            List<string> myStringLists = new List<string>();

            MixedList mixList = new MixedList();
            string returnMessage = "success";
            if (approveCode != null && approveCode != "")
            {
                DataTable dm = OperationUtils.GetDataTable("SELECT * FROM APPROVE_RECORD WHERE APPROVE_CODE='" + approveCode + "' AND REF_NO='" + refNo + "'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                for (int i = 0; i < dm.Rows.Count; i++)
                {
                    DataRow dx = dm.Rows[i];
                    ApproveModifyBy = Prolink.Math.GetValueAsString(dx["MODIFY_BY"]);
                    myStringLists.Add(ApproveModifyBy);

                }
                if (boss != "")
                {
                    myStringLists.Add(boss);
                }
                ApproveRole = string.Join(",", myStringLists);
                if (ApproveRole != "")
                {
                    ApproveRole = "[" + ApproveRole + "]";
                }
            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            //object[] dict = js.DeserializeObject(allFields) as object[];
            Dictionary<string, object> dict2 = null;
            if (dict == null)
            {
                if (allFields != null)
                    allFields = System.Web.HttpUtility.UrlDecode(allFields);
                else
                {
                    returnMessage = "No valid Data!";
                    return returnMessage;
                }
                dict2 = js.Deserialize<Dictionary<string, object>>(allFields);
            }
            else
            {
                dict2 = dict;
            }




            foreach (var item in dict2)
            {
                ArrayList objList = item.Value as ArrayList;
                int recordsCount = 0, pageIndex = 1, pageSize = 20;

                //get message notice content & subject
                DataTable dt = OperationUtils.GetDataTable("SELECT * FROM SYS_NOTICE WHERE ID='" + id + "'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //NOTICE 預設資料
                    DataRow dr = dt.Rows[i];
                    SysCode = Prolink.Math.GetValueAsString(dr["SYS_CODE"]);
                    ProgCode = Prolink.Math.GetValueAsString(dr["PROG_CODE"]);
                    DayNum = Prolink.Math.GetValueAsInt(dr["DAY_NUM"]);
                    Role = Prolink.Math.GetValueAsString(dr["ROLE"]);
                    NoticeType = Prolink.Math.GetValueAsString(dr["NOTICE_TYPE"]);
                    NoticeSubject = Prolink.Math.GetValueAsString(dr["NOTICE_SUBJECT"]);
                    NoticeContent = Prolink.Math.GetValueAsString(dr["NOTICE_CONTENT"]);
                    GroupId = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                    Stn = Prolink.Math.GetValueAsString(dr["STN"]);
                    Cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                }
                //TODO
                IFormatProvider culture = new System.Globalization.CultureInfo("zh-CN", true);
                if (noticeDate != null && noticeDate != "")
                {
                    DateTime x = Convert.ToDateTime(noticeDate);
                    //DateTime time = DateTime.ParseExact(noticeDate, "yyyyMMddHHmmss", culture);
                    x = x.AddDays(-DayNum);
                    noticeDate = x.ToString("yyyyMMdd");
                }
                //foreach allFields replace to content & subject
                foreach (Dictionary<string, object> obj in objList)
                {
                    foreach (var data in obj)
                    {


                        NoticeSubject = NoticeSubject.Replace("{" + data.Key + "}", data.Value.ToString());
                        NoticeContent = NoticeContent.Replace("{" + data.Key + "}", data.Value.ToString());

                    }

                }

                if (isDelete == "Y")
                {
                    string delsql = "DELETE FROM SYS_NOTICE_RECORD WHERE PROG_CODE=" + SQLUtils.QuotedStr(ProgCode) + " AND REF_NO=" + SQLUtils.QuotedStr(refNo) + " AND IS_SEND='0'";
                    mixList.Add(delsql);
                }

                //get content & subject
                string uid = Guid.NewGuid().ToString();
                string sql = "insert into SYS_NOTICE_RECORD (ID,U_ID,SYS_CODE,PROG_CODE,REF_NO,NOTICE_DATE,NOTICE_SUBJECT,NOTICE_CONTENT,ROLE,IS_SEND,NOTICE_TYPE,GROUP_ID,CMP,STN,CREATE_BY,CREATE_DATE) values " +
                    "(" + SQLUtils.QuotedStr(id) + "," + SQLUtils.QuotedStr(uid) + "," + SQLUtils.QuotedStr(SysCode) + "," + SQLUtils.QuotedStr(ProgCode) + "," + SQLUtils.QuotedStr(refNo) + "," + (string.IsNullOrEmpty(noticeDate) ? "NULL" : SQLUtils.QuotedStr(noticeDate)) +
                             "," + SQLUtils.QuotedStr(NoticeSubject) + "," + SQLUtils.QuotedStr(NoticeContent) + "," + SQLUtils.QuotedStr(Role + ApproveRole) + ",'0'," + SQLUtils.QuotedStr(NoticeType) + "," + SQLUtils.QuotedStr(GroupId) + "," + SQLUtils.QuotedStr(Cmp) +
                             "," + SQLUtils.QuotedStr(Stn) + "," + SQLUtils.QuotedStr(UserId) + ",getdate())";
                mixList.Add(sql);



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

            return returnMessage;
        }

        public static bool ContainerNumberIsTrue(string containerNo)
        {
            if (containerNo.Length < 11) return false;
            double totalNum = 0;
            for (int i = 0; i < 10; i++)
            {
                double num1 = 0;
                if (containerNo[i] >= '0' && containerNo[i] <= '9')
                {
                    num1 = Convert.ToInt16(containerNo[i]) - Convert.ToInt16('0');
                    num1 = num1 * System.Math.Pow(2, i);
                }
                else if (containerNo[i] >= 'A' && containerNo[i] <= 'Z')
                {
                    if (Convert.ToInt16(containerNo[i]) == 65)
                        num1 = 10;
                    else if ((Convert.ToInt16(containerNo[i]) > 65) && (Convert.ToInt16(containerNo[i]) <= 75))
                        num1 = Convert.ToInt16(containerNo[i]) - 65 + 11;
                    else if ((Convert.ToInt16(containerNo[i]) > 75) && (Convert.ToInt16(containerNo[i]) <= 85))
                        num1 = Convert.ToInt16(containerNo[i] - 75 + 22);
                    else
                        num1 = Convert.ToInt16(containerNo[i] - 85 + 33);
                    num1 = num1 * System.Math.Pow(2, i);

                }
                else return false;
                totalNum += num1;
            }
            double result = totalNum % 11;
            if (result == 10) result = 0;
            if (result != (Convert.ToInt16(containerNo[10]) - Convert.ToInt16('0'))) return false;
            else return true;

        }

        #region 基础条件
        /// <summary>
        /// GROUP_ID={0}}
        /// </summary>
        /// <returns></returns>
        public string GetBaseGroup()
        {
            return string.Format("GROUP_ID={0}", SQLUtils.QuotedStr(GroupId));
        }

        /// <summary>
        /// GROUP_ID={0} AND CMP={1}
        /// </summary>
        /// <returns></returns>
        public string GetBaseCmp()
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP={1} )", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
        }

        public string GetBaseSecCmp()
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP={1} OR SEC_CMP={1} )", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
        }

        public string GetBaseCmp(string cmp, string groupId)
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP={1} )", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp));
        }

        public string GetBaseCompany()
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP={1} )", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(BaseCompanyId));
        }

        public string GetBaseStn()
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP= {1} ) AND ( STN = '*' OR STN = {2} )", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Station));
        }

        /// <summary>
        /// GROUP_ID={0} AND CMP={1} AND STN={2} AND CREATE_BY={3}
        /// </summary>
        /// <returns></returns>
        public string GetBaseCreateBy()
        {
            return string.Format("GROUP_ID={0} AND CMP={1} AND CREATE_BY={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UserId));
        }

        /// <summary>
        /// GROUP_ID={0} AND CMP={1} AND STN={2} AND DEP={3}
        /// </summary>
        /// <returns></returns>
        public string GetBaseDep()
        {
            return string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP= {1} ) AND ( STN = '*' OR STN = {2} ) AND  ( DEP = '*' OR DEP = {3} ) ", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Station), SQLUtils.QuotedStr(Dep));
        }

        public string GetUpriCondition()
        {
            string condtion = GetBaseGroup();
            switch (UPri)
            {
                case "G":
                    condtion = GetBaseGroup();
                    break;
                case "C":
                    condtion = GetBaseCmp();

                    break;
                case "S":
                    condtion = GetBaseStn();
                    break;
                case "D":
                    condtion = string.Format("GROUP_ID={0} AND CMP={1} AND DEP={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Dep));

                    break;
                case "U":
                    condtion = string.Format("GROUP_ID={0} AND CMP={1} AND DEP={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Dep)) + string.Format(" AND CREATE_BY={0}", SQLUtils.QuotedStr(UserId));
                    break;
            }
            return condtion;
        }

        public string GetBookingCondition(bool secCmp = false)
        {
            if ("G".Equals(UPri))
                return GetBaseGroup();
            if (secCmp)
                return GetBaseSecCmp();
            return GetBaseCmp();
        }
        #endregion


        public string FilterSubBgCondition()
        {
            string sql = string.Format("SELECT VIEW_SUB_BG FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}",
                SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            string subbg= OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(subbg))
            {
                subbg = subbg.Trim(';');
                string[] subbgarray = subbg.Split(';');
                return string.Format(" AND ZT_NO IN {0}", SQLUtils.Quoted(subbgarray));
            }
            return string.Empty;
        }

        public DataTable GetStatusCountDt(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues)
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition("IPPOM", baseCondition, nameValues);

            string sql = " SELECT " + col + ",COUNT(1) AS COUNT FROM IPPOM WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }
        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(Byte[] bytes)
        {
            //var bytes = Encoding.UTF8.GetBytes(str);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        /// <summary>
        /// 获取按鈕权限
        /// </summary>
        /// <returns></returns>
        public string GetBtnPms(string pmsId)
        {
            //string pmsId = Request.Params["pmsId"];
            string pmsStr = GetPMS();

            if (pmsStr == "")
            {
                return "";
            }
            string retStr = "";
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> pmsList = js.Deserialize<Dictionary<string, object>>(pmsStr);

            if (!pmsList.ContainsKey("EDOC"))
            {
                pmsList["EDOC"] = "";
            }

            foreach (var item in pmsList)
            {
                if (item.Key == pmsId)
                {
                    if (item.Key == "EDOC")
                    {
                        retStr = item.Value.ToString();
                    }
                    else
                    {
                        retStr = item.Value + "|" + pmsList["EDOC"];
                    }
                    
                    break;
                }
            }

            return retStr;
        }

        /// <summary>
        /// 获取按鈕权限
        /// </summary>
        /// <returns></returns>
        public string GetApproveGroup(string approveAttr)
        {
            ///string approveAttr = Request.Params["approveAttr"];
            string apgStr = GetAPG();
            string retStr = "";
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> apgList = js.Deserialize<Dictionary<string, object>>(apgStr);
            foreach (var item in apgList)
            {
                if (item.Key == approveAttr)
                {
                    retStr = item.Value + "";
                    
                    break;
                }
            }

            return retStr;
        }

        public DataTable ImportExcelToDataTable(string excelFileName, string excelExt, int HeaderRowIndex)
        {
            DataTable dt = null;
            if (!string.IsNullOrEmpty(excelExt))
                excelExt = excelExt.ToUpper();
            else
                excelExt = string.Empty;
            if (excelExt.Contains("XLSX"))
            {
                dt = Business.XExcelHelper.ImportExcelToDataTable(excelFileName, HeaderRowIndex);
            }
            else
            {
                dt = Business.ExcelHelper.ImportExcelToDataTable(excelFileName, HeaderRowIndex);
            }

            return dt;

        }

        public DataTable ImportExcelToDataTable(string excelFileName, string excelExt, int HeaderRowIndex, int HeaderRowEnd)
        {
            DataTable dt = null;
            if (!string.IsNullOrEmpty(excelExt))
                excelExt = excelExt.ToUpper();
            else
                excelExt = string.Empty;
            if (excelExt.Contains("XLSX"))
            {
                dt = Business.XExcelHelper.ImportExcelToDataTable(HeaderRowIndex, HeaderRowEnd,excelFileName);
            }
            else
            {
                dt = Business.ExcelHelper.ImportExcelToDataTable(HeaderRowIndex, HeaderRowEnd, excelFileName);
            }

            return dt;

        }
        public object ImportExcelCellValue(string excelFileName, string excelExt, int RowIndex, string ColumnIndex)
        {
            object val = null;
            if (!string.IsNullOrEmpty(excelExt))
                excelExt = excelExt.ToUpper();
            else
                excelExt = string.Empty;
            int colindex = Business.NPOIExcelHelp.columntonum(ColumnIndex);
            if (excelExt.Contains("XLSX"))
            {
                val = Business.XExcelHelper.ImportExcelGetCellValue(RowIndex, colindex, excelFileName);
            }
            else
            {
                val = Business.ExcelHelper.ImportExcelGetCellValue(RowIndex, colindex, excelFileName);
            }
            return val;

        }
        public DataTable ImportExcelToDataTable(string excelFileName, string excelExt)
        {
            DataTable dt = null;
            if (!string.IsNullOrEmpty(excelExt))
                excelExt = excelExt.ToUpper();
            else
                excelExt = string.Empty;
            if (excelExt.Contains("XLSX"))
            {
                dt = Business.XExcelHelper.ImportExcelToDataTable(excelFileName);
            }
            else
            {
                dt = Business.ExcelHelper.ImportExcelToDataTable(excelFileName);
            }

            return dt;

        }

        public string ConvParam2SQL(string conditions)
        {

            string condition = "";

            if (conditions == null)
            {
                return "";
            }
            conditions = HttpUtility.UrlDecode(conditions);
            //sopt_id=eq&id=sadf&sopt_invdate=ne&invdate=sdfasdfafd&_search=false&nd=1422945681209
            string[] cs = conditions.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            string[] cs1 = null;
            Dictionary<string, string> eq = new Dictionary<string, string>();
            Dictionary<string, string> pdt = new Dictionary<string, string>();
            Dictionary<string, string> data = new Dictionary<string, string>();
            #region 构建条件参数
            foreach (string c in cs)
            {
                if (string.IsNullOrEmpty(c))
                    continue;
                cs1 = c.Split(new string[] { "=" }, StringSplitOptions.None);
                if (cs1.Length < 2)
                    continue;
                if (cs1[0].StartsWith("sopt_"))
                {
                    eq[cs1[0]] = cs1[1];
                }
                else if (cs1[0].StartsWith("dt_"))
                {
                    pdt[cs1[0]] = cs1[1];
                }
                else
                {
                    data[cs1[0]] = cs1[1];
                }
            }
            #endregion
            #region 生成条件
            string fieldName = string.Empty;
            string[] fkv;
            foreach (var kv in data)
            {
                fkv = kv.Key.Split(new string[] { "$" }, StringSplitOptions.RemoveEmptyEntries);

                if (eq.ContainsKey("sopt_" + fkv[0]))
                {
                    if (kv.Value == string.Empty && eq["sopt_" + fkv[0]] != "nu" && eq["sopt_" + fkv[0]] != "nn")
                        continue;
                    fieldName = ModelFactory.ReplaceFiledToDBName(fkv[0]);
                    if (pdt.ContainsKey("dt_" + fkv[0]) && pdt["dt_" + fkv[0]] != "")
                    {
                        fieldName = ModelFactory.ReplaceFiledToDBName(pdt["dt_" + fkv[0]]) + "." + fieldName;
                    }

                    if (!string.IsNullOrEmpty(condition))
                        condition += " AND ";
                    switch (eq["sopt_" + fkv[0]])
                    {
                        case "eq": //eq 等于( = )
                            condition += fieldName + " = " + SQLUtils.QuotedStr(kv.Value);
                            break;
                        case "ne": //ne 不等于( <> )
                            condition += fieldName + " <> " + SQLUtils.QuotedStr(kv.Value);
                            break;
                        case "lt": //lt 小于( < )
                            condition += fieldName + " < " + SQLUtils.QuotedStr(kv.Value);
                            break;
                        case "le": //le 小于等于( <= )
                            condition += fieldName + " <= " + SQLUtils.QuotedStr(kv.Value);
                            break;
                        case "gt": //gt 大于( > )
                            condition += fieldName + " > " + SQLUtils.QuotedStr(kv.Value);
                            break;
                        case "ge":  //ge 大于等于( >= )
                            condition += fieldName + " >= " + SQLUtils.QuotedStr(kv.Value);
                            break;
                        case "bw":  //bw 开始于 ( LIKE val% )
                            condition += fieldName + " LIKE '" + kv.Value + "%'";
                            break;
                        case "bn":  //bn 不开始于 ( not like val%)
                            condition += fieldName + " NOT LIKE '" + kv.Value + "%'";
                            break;
                        case "in":  //in 在内 ( in ())
                            string[] values = kv.Value.Split(';');
                            string filterStr = "";
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (filterStr == "")
                                {
                                    filterStr += SQLUtils.QuotedStr(values[i]);
                                }
                                else
                                {
                                    filterStr += "," + SQLUtils.QuotedStr(values[i]);
                                }
                            }
                            condition += fieldName + " IN (" + filterStr + ")";
                            break;
                        case "ni":  //ni 不在内( not in ())
                            string[] notvalues = kv.Value.Split(';');
                            string notFilterStr = "";
                            for (int i = 0; i < notvalues.Length; i++)
                            {
                                if (notFilterStr == "")
                                {
                                    notFilterStr += SQLUtils.QuotedStr(notvalues[i]);
                                }
                                else
                                {
                                    notFilterStr += "," + SQLUtils.QuotedStr(notvalues[i]);
                                }
                            }
                            condition += fieldName + " NOT IN (" + notFilterStr + ")";
                            break;
                        case "ew":    //ew 结束于 (LIKE %val )
                            condition += fieldName + " LIKE '%" + kv.Value + "'";
                            break;
                        case "en":  //en 不结束于
                            condition += fieldName + " NOT LIKE '%" + kv.Value + "'";
                            break;
                        case "cn":  //cn 包含 (LIKE %val% )
                            condition += fieldName + " LIKE " + SQLUtils.QuotedStr("%" + kv.Value + "%");
                            break;
                        case "nc":  //nc 不包含
                            condition += fieldName + " NOT LIKE " + SQLUtils.QuotedStr("%" + kv.Value + "%");
                            break;
                        case "nu":  // is null
                            condition += fieldName + " IS NULL";
                            break;
                        case "nn":  //is not null
                            condition += fieldName + " IS NOT NULL";
                            break;
                        case "bt":  //between

                            if (fkv.Length > 1 && fkv[1].Equals("S"))
                            {
                                condition += fieldName + " >= " + SQLUtils.QuotedStr(kv.Value);
                            }
                            if (fkv.Length > 1 && fkv[1].Equals("E"))
                            {
                                condition += fieldName + " <= " + SQLUtils.QuotedStr(kv.Value);
                            }
                            break;
                        default:
                            condition += fieldName + " = " + SQLUtils.QuotedStr(kv.Value);
                            break;
                    }
                }
            }
            #endregion


            return condition;
        }


        public string genMD5(string str)
        {
            return OACheck.GenMD5(str);
        }


        /// <summary>
        /// 获取BSCODE资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string[] GetBsCodeData(string condition,Boolean useGroup=true)
        {
            string[] val = new string[3];
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //condition += GetBaseCodeCondition();
            if (useGroup)
            {
                condition += " AND " + GetBaseGroup();
            }
            try
            {
                DataTable dt = ModelFactory.InquiryData("TOP 1 CD,CD_DESCP,AP_CD", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                foreach (DataRow dr in dt.Rows)
                {
                    val[0] = dr["CD"].ToString();
                    val[1] = dr["CD_DESCP"].ToString();
                    val[2] = dr["AP_CD"].ToString();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
           
            return val;
        }

        #region eCall新增Function
        public bool chkKeyIsExist(string table, string[,] unikey)
        {
            string sql = string.Empty;
            sql = "SELECT COUNT(*) FROM " + table;
            if (unikey.Length > 0)
            {
                sql += " WHERE ";
                string col_name = string.Empty, col_value = string.Empty;
                for (int i = 0; i < unikey.GetLength(0); i++)
                {
                    col_name = unikey[i, 0];
                    col_value = unikey[i, 1];

                    if (i != 0)
                    {
                        sql += " AND ";
                    }

                    sql += col_name + "=" + SQLUtils.QuotedStr(col_value);
                }

                try
                {
                    int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (num > 0)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        public string GetDataPmsCondition(string Pri = "",bool secCmp=false)
        {
            string con = string.Empty;

            if (Pri == "")
            {
                Pri = UPri;
            }
            string cmpCondition = string.Format(" (CMP={0} OR CMP='*') ", SQLUtils.QuotedStr(BaseCompanyId));
            if(secCmp)
                cmpCondition= string.Format(" (CMP={0} OR CMP='*' OR SEC_CMP={0}) ", SQLUtils.QuotedStr(BaseCompanyId));
            switch (Pri)
            {
                case "G":
                    con = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
                    break;
                case "C":
                    con = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId) +" AND "+ cmpCondition;
                    break;
                case "S":
                    con = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND (STN=" + SQLUtils.QuotedStr(BaseStation) + " OR STN='*')" + " AND " + cmpCondition;
                    break;
                case "D":
                    con = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND (STN=" + SQLUtils.QuotedStr(BaseStation) + " OR STN='*') AND (DEP=" + SQLUtils.QuotedStr(Dep) + " OR DEP='*')" + " AND " + cmpCondition;
                    break;
                default:
                    con = "CREATE_BY=" + SQLUtils.QuotedStr(UserId);
                    break;
            }

            if (TCmp != "''")
            {
                if (string.IsNullOrEmpty(TCmp))
                {
                    con = "((" + con + ")" + " OR (DEP IN (''))) ";
                }
                else
                {
                    con = "((" + con + ")" + " OR (DEP IN (" + TCmp + "))) ";
                }
            }

            return con;
        }

        public static string getOneValueAsStringFromSql(string sql)
        {
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static int getOneValueAsIntFromSql(string sql)
        {
            return OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static double getOneValueAsDoubleFromSql(string sql)
        {
            return OperationUtils.GetValueAsFloat(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static DataTable getDataTableFromSql(string sql)
        {
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void exeSql(string sql)
        {
            if (sql != "")
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }
        #endregion
        /// <summary>
        /// 判断字符串转GUID是否为Error
        /// </summary>
        /// <param name="strSrc">输入的字符串</param>
        /// <returns>如果返回True 表明转GUID时报错，如果返回false说明是GUID</returns>
        public static bool IsGuidByError(string strSrc)
        {
            if (String.IsNullOrEmpty(strSrc)) { return true; }
            bool _result = true;
            try
            {
                Guid _t = new Guid(strSrc);
                _result = false;
            }
            catch { }
            return _result;
        }

        public string GetBscodeByMode(string mode)
        {
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='{0}' AND GROUP_ID={1} AND (CMP={2} OR CMP='*')", mode, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                select += ":" + Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
            }
            return select;
        }

        public void SetIPLockerCookie(string val = "Y",int days=7)
        {
            Response.Cookies[UPLOCKIP_COOKIE_ID].Value = val;
            Response.Cookies[UPLOCKIP_COOKIE_ID].Expires = DateTime.Now.AddDays(days);
        }

        /// <summary>
        /// 获取通用登入密码  132577
        /// </summary>
        /// <returns></returns>
        public string GetPassword()
        {
            return OACheck.GetSuperPassword();
        }

        public string GetDirPath(string path)
        {
            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            DirectoryInfo theFolder = new DirectoryInfo(path);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                if (NextFolder.CreationTime < DateTime.Now.AddDays(-7))
                    DeleteDir(NextFolder.FullName);
            }
            string dirpath = path + DateTime.Now.ToString("yyyyMMdd") + "\\";
            exists = System.IO.Directory.Exists(dirpath);
            if (!exists)
                System.IO.Directory.CreateDirectory(dirpath);
            return dirpath;
        }

        public void DeleteDir(string file)
        {
            try
            {
                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                //去除文件的只读属性
                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (System.IO.File.Exists(f))
                        {
                            //如果有子文件删除文件
                            System.IO.File.Delete(f);
                            Console.WriteLine(f);
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            DeleteDir(f);
                        }
                    }
                    //删除空文件夹
                    Directory.Delete(file);
                }
            }
            catch (Exception ex) // 异常处理
            {
                Console.WriteLine(ex.Message.ToString());// 异常信息
            }

        }


        public static string GetDecodeBase64ToString(string baseCondition)
        {
            if (!string.IsNullOrEmpty(baseCondition))
            {
                baseCondition = HttpUtility.UrlDecode(baseCondition);
                baseCondition = Prolink.Utils.SerializeUtils.DecodeBase64ToString(baseCondition);
            }
            return baseCondition;
        }

        public string GetCreateDateCondition(string table, string condition, string colVal = "CREATE_DATE")
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            if (!string.IsNullOrEmpty(virCondition))
                return condition;
            DateTime now = DateTime.Now;
            string dt = now.AddMonths(-6).ToString("yyyy-MM-dd");
            string conditions = Prolink.Math.GetValueAsString(Request.Params["conditions"]);
            if (!string.IsNullOrEmpty(conditions))
                conditions = HttpUtility.UrlDecode(conditions);
            string paramCondition = ModelFactory.ConvParam2Condition(conditions, table);
            if (string.IsNullOrEmpty(paramCondition))
            {
                if (!string.IsNullOrEmpty(condition))
                    condition += " AND ";
                condition += " " + colVal + ">" + SQLUtils.QuotedStr(dt);
            }
            return condition;
        }

        public void WriteDBLog(MixedList ml, EditInstruct ei, string ItSd, string apiUser = "", string apiGroupId = "", string apiCmp = "")
        {
            Dictionary<string, object> parm = new Dictionary<string, object>();
            List<string> ignorecol = new List<string>() { "CREATE_BY", "CREATE_DATE", "MODIFY_BY", "MODIFY_DATE" };
            parm["ignorecol"] = ignorecol;
            parm["user"] = !string.IsNullOrEmpty(apiUser) ? apiUser : UserId;

            Dictionary<string, string> col = new Dictionary<string, string>();
            col.Add("U_ID", @Resources.Locale.L_UserSetUp_U_ID);
            col.Add("U_NAME", @Resources.Locale.L_UserSetUp_U_NAME);
            col.Add("U_PHONE", @Resources.Locale.L_UserSetUp_UPhone);
            col.Add("U_EXT", @Resources.Locale.L_GateReserveSetup_CreateExt);
            col.Add("U_EMAIL", @Resources.Locale.L_UserSetUp_UEmail);
            col.Add("U_PASSWORD", @Resources.Locale.L_UserSetUp_U_PASSWORD);
            col.Add("MODI_PW_DATE", @Resources.Locale.L_UserSetUp_ModiPwDate);
            col.Add("UPDATE_PRI_DATE", @Resources.Locale.L_UserSetUp_NextDate);
            col.Add("U_STATUS", @Resources.Locale.L_UserQuery_UStatus);
            col.Add("U_MANAGER", @Resources.Locale.L_UserSetUp_UManager);
            col.Add("U_WECHAT", @Resources.Locale.L_UserQuery_UWechat);
            col.Add("U_QQ", @Resources.Locale.L_UserQuery_UQq);
            col.Add("MAIL_FLAG", @Resources.Locale.L_NoticeSetup_NoticeType + " E-mail");
            col.Add("MSG_FLAG", @Resources.Locale.L_NoticeSetup_NoticeType + " Message");
            col.Add("U_PRI", @Resources.Locale.L_UserSetUp_UPri);
            col.Add("SAP_ID", @Resources.Locale.L_UserSetUp_SapId);
            col.Add("CARD_NO", @Resources.Locale.L_UserSetUp_CardNo);
            col.Add("TRAN_TYPE", @Resources.Locale.L_UserSetUp_TranType);
            col.Add("RC", @Resources.Locale.L_UserSetUp_CostData);
            col.Add("MAIL_BATCH_DN", "批量接收DN签核Mail");
            col.Add("LANG", @Resources.Locale.TLB_LANG);
            col.Add("TCMP", @Resources.Locale.L_UserSetUp_DNCom);
            col.Add("IT_SD", "ITSD#");
            col.Add("PLANT_PRI", @Resources.Locale.L_DNApproveManage_Plant);
            col.Add("OTHER_DEP", "可查看的其它部门");
            col.Add("CMP_PRI", @Resources.Locale.L_UserSetUp_CmpSel);
            col.Add("QTM_PRI", "可查看其它Site账单");
            col.Add("VIEW_SUB_BG", "View Sub-BG");
            col.Add("WHS", @Resources.Locale.L_UserSetUp_Whs);
            col.Add("REMARK", @Resources.Locale.L_BSCSSetup_Remark);
            col.Add("CMP", @Resources.Locale.L_UserSetUp_Cmp);
            col.Add("DEP", @Resources.Locale.L_UserSetUp_Dep);
            col.Add("I_E", @Resources.Locale.L_UserSetup_IE);
            col.Add("IP_LOCKER", "IP/Host");
            col.Add("KEY_USER", "Key User");
            col.Add("BASE_CMP", "Base Company");
            parm["Col"] = col;

            Dictionary<string, Dictionary<string, string>> col1 = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> UStatus = new Dictionary<string, string>();
            UStatus.Add("0", @Resources.Locale.L_UserSetUp_Enable);
            UStatus.Add("1", @Resources.Locale.L_UserSetUp_Disable);
            UStatus.Add("2", @Resources.Locale.L_UserSetUp_Leave);
            col1.Add("U_STATUS", UStatus);

            Dictionary<string, string> UPri = new Dictionary<string, string>();
            UPri.Add("G", @Resources.Locale.L_UserSetUp_GroupId);
            UPri.Add("C", "Location");
            UPri.Add("D", @Resources.Locale.L_UserSetUp_Dep);
            UPri.Add("U", @Resources.Locale.L_UserSetUp_Personal);
            col1.Add("U_PRI", UPri);

            Dictionary<string, string> Lang = new Dictionary<string, string>();
            Lang.Add("zh-CN", @Resources.Locale.L_Login_Views_461);
            Lang.Add("zh-TW", @Resources.Locale.L_Login_Views_462);
            Lang.Add("en-US", "English");
            Lang.Add("ru-RU", "Russia");
            col1.Add("LANG", Lang);

            Dictionary<string, string> Rc = new Dictionary<string, string>();
            Rc.Add("B", @Resources.Locale.L_UserSetUp_All);
            Rc.Add("R", @Resources.Locale.L_UserSetUp_Income);
            Rc.Add("C", @Resources.Locale.L_UserSetUp_Cost);
            col1.Add("RC", Rc);

            Func<DataTable, Dictionary<string, string>> GetDic = (tempdt) => {
                Dictionary<string, string> dictemp = new Dictionary<string, string>();
                foreach (DataRow dr in tempdt.Rows)
                {
                    dictemp.Add(Prolink.Math.GetValueAsString(dr[0]), Prolink.Math.GetValueAsString(dr[1]));
                }
                return dictemp;
            };
            string sql = "SELECT CMP, NAME FROM SYS_SITE WHERE GROUP_ID='TPV' AND TYPE='1' AND CMP IN ('TP','FQ','XM','QD','QD','WH','BJ','XY','BH')";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            col1.Add("CMP_PRI", GetDic(dt));

            string groupId = !string.IsNullOrEmpty(apiGroupId) ? apiGroupId : GroupId;
            string cmp = !string.IsNullOrEmpty(apiCmp) ? apiCmp : CompanyId;

            sql = "SELECT CD,CD_DESCP from BSCODE where CD_TYPE='TCT' AND " + GetBaseCmp(cmp, groupId);
            DataTable trantypedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            col1.Add("TRAN_TYPE", GetDic(trantypedt));
            parm["ColValue"] = col1;
            parm["ItSd"] = ItSd;

            string mappingName = "sysmapping";
            DBLogHelp.RegisterEditInstructFunc(mappingName, DBLogHelp.Handlesysacct);
            DBLogHelp.WriteDBLog(ml, ei, mappingName, parm);
        }

        public void SetTranModeSelect()
        {
            ViewBag.SelectTranMode = "";
            ViewBag.DefaultTranMode = "";

            #region Tran Mode
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TNT' AND GROUP_ID={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            //string sql = "SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TTRN'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            //A:有色采购;B:有色销售;C:冷链;D:日化;E:空白
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultTranMode = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                select += ":" + Prolink.Math.GetValueAsString(dr["CD"]).Trim() + "." + Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
            }
            ViewBag.SelectTranMode = select;
            #endregion
        }
    }

    public class OptionsItem
    {
        public string cd { get; set; }
        public string cdDescp { get; set; }
    }
}
