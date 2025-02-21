using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prolink.Data;
using Prolink.Utils;
using System.Data;
using System.Collections.Specialized;
using Prolink.DataOperation;
using System.Web.Caching;

namespace WebGui.Models
{
    public class User : Prolink.V6.Core.User
    {
        public string updatePriDate
        {
            get;
            set;
        }
        public string PwdTimeOut
        {
            get;
            set;
        }
        /// <summary>
        /// 获得部門
        /// </summary>
        /// <returns></returns>
        public virtual string dep
        {
            get;
            set;
        }

        /// <summary>
        /// 获得業別
        /// </summary>
        /// <returns></returns>
        public virtual string bu
        {
            get;
            set;
        }
        public virtual string ext
        {
            get;
            set;
        }

        public virtual string uPri
        {
            get;
            set;
        }

        public virtual string cmpPri
        {
            get;
            set;
        }

        public virtual string plantPri
        {
            get;
            set;
        }

        public virtual string IOFlag
        {
            get;
            set;
        }
        public virtual string UType
        {
            get;
            set;
        }

        public virtual string TCmp
        {
            get;
            set;
        }

        public virtual string Lang
        {
            get;
            set;
        }

        public virtual string BaseCmp
        {
            get;
            set;
        }

        /// <summary>
        /// ip锁
        /// </summary>
        public string IPLocker
        {
            get;
            set;
        }

        private string[] _underling = new string[0];//获得下属员工
        public User(string user, string password, string company, string station)
            : base(user, password, company, station)
        {

        }

        protected override Prolink.V6.Core.User GetSelf(string user, string company, string station, string password)
        {
            return NewUser(user, company, station, password);
        }

        public static User NewUser(string user, string company, string station, string password)
        {
            station = "XXX";
            company = "XX";
            string sql = "SELECT * FROM SYS_ACCT WHERE U_STATUS=0 AND [U_ID]=" + SQLUtils.QuotedStr(user)
                + " AND [U_PASSWORD]=" + SQLUtils.QuotedStr(password);

            System.Data.DataTable result = Prolink.DataOperation.OperationUtils.GetDataTable(sql, new string[0], GetConnection(company, station));
            if (result.Rows.Count > 0)
            {
                user = result.Rows[0]["U_ID"] == null ? "" : result.Rows[0]["U_ID"].ToString();
                company = result.Rows[0]["CMP"] == null ? "" : result.Rows[0]["CMP"].ToString();
                User userObj = new User(user, password, company, station);
                userObj.localName = result.Rows[0]["U_NAME"] == null ? "" : result.Rows[0]["U_NAME"].ToString();
                userObj.englishName = result.Rows[0]["U_NAME"] == null ? "" : result.Rows[0]["U_NAME"].ToString();
                userObj.updatePriDate = result.Rows[0]["UPDATE_PRI_DATE"] == null ? "" : result.Rows[0]["UPDATE_PRI_DATE"].ToString();
                userObj.dep = result.Rows[0]["DEP"] == null ? "" : result.Rows[0]["DEP"].ToString();
                userObj.bu = result.Rows[0]["BU"] == null ? "" : result.Rows[0]["BU"].ToString();
                userObj.uPri = result.Rows[0]["U_PRI"] == null ? "" : result.Rows[0]["U_PRI"].ToString();
                userObj.cmpPri = result.Rows[0]["CMP_PRI"] == null ? "" : result.Rows[0]["CMP_PRI"].ToString();
                userObj.plantPri = result.Rows[0]["PLANT_PRI"] == null ? "" : SQLUtils.QuotedStr(result.Rows[0]["PLANT_PRI"].ToString()).Replace(";", SQLUtils.QuotedStr(","));
                userObj.ext = result.Rows[0]["U_EXT"] == null ? "" : result.Rows[0]["U_EXT"].ToString();
                userObj.IOFlag = result.Rows[0]["IO_FLAG"] == null ? "" : result.Rows[0]["IO_FLAG"].ToString();
                userObj.UType = result.Rows[0]["U_TYPE"] == null ? "" : result.Rows[0]["U_TYPE"].ToString();
                userObj.TCmp = result.Rows[0]["TCMP"] == null ? "" : SQLUtils.QuotedStr(result.Rows[0]["TCMP"].ToString()).Replace(";", SQLUtils.QuotedStr(","));
                userObj.IPLocker = result.Columns.Contains("IP_LOCKER") ? Prolink.Math.GetValueAsString(result.Rows[0]["IP_LOCKER"]) : string.Empty;
                userObj.BaseCmp = result.Rows[0]["BASE_CMP"] == null ? "" : result.Rows[0]["BASE_CMP"].ToString();
                userObj.Lang = string.IsNullOrEmpty(result.Rows[0]["LANG"].ToString()) || result.Rows[0]["LANG"] == null ? "en-US" : result.Rows[0]["LANG"].ToString();
                if (string.IsNullOrEmpty(userObj.GetLanguageType()))
                {
                    userObj.SetLanguageType("BIG5");
                }
                return userObj;
            }
            return null;
        }

        public static User GetUserByCookie(string user, string company, string station)
        {
            string sql = "";
            sql = "SELECT * FROM SYS_ACCT WHERE U_STATUS=0 AND Upper(U_ID)=Upper(" + SQLUtils.QuotedStr(user) + ")" +
                " AND CMP=" + SQLUtils.QuotedStr(company);
            System.Data.DataTable result = Prolink.DataOperation.OperationUtils.GetDataTable(sql, new string[0], Prolink.Web.WebContext.GetInstance().GetConnection());
            if (result.Rows.Count > 0)
            {
                user = Prolink.Math.GetValueAsString(result.Rows[0]["U_ID"]);
                company = Prolink.Math.GetValueAsString(result.Rows[0]["CMP"]);
                station = Prolink.Math.GetValueAsString(result.Rows[0]["STN"]);
                string password = Prolink.Math.GetValueAsString(result.Rows[0]["U_PASSWORD"]);
                if (string.IsNullOrEmpty(station))
                    station = "XX";
                User userObj = new User(user, password, company, station);
                userObj.SetLanguageType("BIG5");
                userObj.group_Id = Prolink.Math.GetValueAsString(result.Rows[0]["GROUP_ID"]);
                userObj.updatePriDate = Prolink.Math.GetValueAsString(result.Rows[0]["UPDATE_PRI_DATE"]);
                userObj.dep = result.Rows[0]["DEP"] == null ? "" : result.Rows[0]["DEP"].ToString();
                userObj.bu = result.Rows[0]["BU"] == null ? "" : result.Rows[0]["BU"].ToString();
                userObj.uPri = result.Rows[0]["U_PRI"] == null ? "" : result.Rows[0]["U_PRI"].ToString();
                userObj.cmpPri = result.Rows[0]["CMP_PRI"] == null ? "" : result.Rows[0]["CMP_PRI"].ToString();
                userObj.plantPri = result.Rows[0]["PLANT_PRI"] == null ? "" : SQLUtils.QuotedStr(result.Rows[0]["PLANT_PRI"].ToString()).Replace(";", SQLUtils.QuotedStr(","));
                userObj.ext = result.Rows[0]["U_EXT"] == null ? "" : result.Rows[0]["U_EXT"].ToString();
                userObj.IOFlag = result.Rows[0]["IO_FLAG"] == null ? "" : result.Rows[0]["IO_FLAG"].ToString();
                userObj.UType = result.Rows[0]["U_TYPE"] == null ? "" : result.Rows[0]["U_TYPE"].ToString();
                userObj.TCmp = result.Rows[0]["TCMP"] == null ? "" : SQLUtils.QuotedStr(result.Rows[0]["TCMP"].ToString()).Replace(";", SQLUtils.QuotedStr(","));
                userObj.IPLocker = result.Columns.Contains("IP_LOCKER") ? Prolink.Math.GetValueAsString(result.Rows[0]["IP_LOCKER"]) : string.Empty;
                userObj.BaseCmp = result.Rows[0]["BASE_CMP"] == null ? "" : result.Rows[0]["BASE_CMP"].ToString();
                userObj.Lang = string.IsNullOrEmpty(result.Rows[0]["LANG"].ToString()) || result.Rows[0]["LANG"] == null ? "en-US" : result.Rows[0]["LANG"].ToString();
                //string updateSQL = "UPDATE SYS_ACCT SET FAIL_COUNT = " + SQLUtils.QuotedStr("0") + " WHERE U_ID =" + SQLUtils.QuotedStr(user) + " AND CMP =" + SQLUtils.QuotedStr(company);
                //OperationUtils.ExecuteUpdate(updateSQL, Prolink.Web.WebContext.GetInstance().GetConnection());

                return userObj;
            }
            return null;
        }


        public static User GetUser(string user, string company, string station, string password,HttpRequestBase request=null)
        {
            string sql = "";
            sql = "SELECT * FROM SYS_ACCT WHERE U_STATUS=0 AND Upper(U_ID)=Upper(" + SQLUtils.QuotedStr(user) + ")" +
                " AND U_PASSWORD=" + SQLUtils.QuotedStr(password) + " AND CMP=" + SQLUtils.QuotedStr(company);
            System.Data.DataTable result = Prolink.DataOperation.OperationUtils.GetDataTable(sql, new string[0], Prolink.Web.WebContext.GetInstance().GetConnection());
            if (result.Rows.Count > 0)
            {
                user = Prolink.Math.GetValueAsString(result.Rows[0]["U_ID"]);
                company = Prolink.Math.GetValueAsString(result.Rows[0]["CMP"]);
                station = Prolink.Math.GetValueAsString(result.Rows[0]["STN"]);
                if (string.IsNullOrEmpty(station))
                    station = "XX";
                User userObj = new User(user, password, company, station);
                userObj.SetLanguageType("BIG5");
                userObj.group_Id = Prolink.Math.GetValueAsString(result.Rows[0]["GROUP_ID"]);
                userObj.updatePriDate = Prolink.Math.GetValueAsString(result.Rows[0]["UPDATE_PRI_DATE"]);
                userObj.dep = result.Rows[0]["DEP"] == null ? "" : result.Rows[0]["DEP"].ToString();
                userObj.bu = result.Rows[0]["BU"] == null ? "" : result.Rows[0]["BU"].ToString();
                userObj.uPri = result.Rows[0]["U_PRI"] == null ? "" : result.Rows[0]["U_PRI"].ToString();
                userObj.cmpPri = result.Rows[0]["CMP_PRI"] == null ? "" : result.Rows[0]["CMP_PRI"].ToString();
                userObj.plantPri = result.Rows[0]["PLANT_PRI"] == null ? "" : SQLUtils.QuotedStr(result.Rows[0]["PLANT_PRI"].ToString()).Replace(";", SQLUtils.QuotedStr(","));
                userObj.ext = result.Rows[0]["U_EXT"] == null ? "" : result.Rows[0]["U_EXT"].ToString();
                userObj.IOFlag = result.Rows[0]["IO_FLAG"] == null ? "" : result.Rows[0]["IO_FLAG"].ToString();
                userObj.UType = result.Rows[0]["U_TYPE"] == null ? "" : result.Rows[0]["U_TYPE"].ToString();
                userObj.TCmp = result.Rows[0]["TCMP"] == null ? "" : SQLUtils.QuotedStr(result.Rows[0]["TCMP"].ToString()).Replace(";",SQLUtils.QuotedStr(","));
                userObj.IPLocker =result.Columns.Contains("IP_LOCKER")?Prolink.Math.GetValueAsString(result.Rows[0]["IP_LOCKER"]):string.Empty;
                userObj.Lang = string.IsNullOrEmpty(result.Rows[0]["LANG"].ToString()) || result.Rows[0]["LANG"] == null ? "en-US" : result.Rows[0]["LANG"].ToString();
                userObj.BaseCmp = result.Rows[0]["BASE_CMP"] == null ? "" : result.Rows[0]["BASE_CMP"].ToString();
                //string updateSQL = "UPDATE SYS_ACCT SET FAIL_COUNT = " + SQLUtils.QuotedStr("0") + " WHERE U_ID =" + SQLUtils.QuotedStr(user) + " AND CMP =" + SQLUtils.QuotedStr(company);
                //OperationUtils.ExecuteUpdate(updateSQL, Prolink.Web.WebContext.GetInstance().GetConnection());

                return userObj;
            }
            return null;
        }

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        /// <param name="user"></param>
        /// <param name="stn"></param>
        /// <param name="cmp"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<string> GetUserRoles(string user, string stn, string cmp, string groupId)
        {
            //string user = Params["plv3.passport.user"];
            //string cmp = Params["plv3.passport.companyid"];
            //string groupId = Params["plv3.passport.groupid"];
            //string stn = Params["plv3.passport.station"];
            DataTable dt = OperationUtils.GetDataTable("SELECT AR.FROLE_ID,R.GROUP_ID,R.CMP,R.STN FROM SYS_ACCT_ROLE AR LEFT JOIN SYS_ROLE R ON AR.FROLE_ID = R.FID AND AR.RCMP = R.CMP AND AR.RSTN = R.STN AND AR.GROUP_ID = R.GROUP_ID WHERE AR.FACCT_ID=" + SQLUtils.QuotedStr(user) + " AND (AR.CMP=" + SQLUtils.QuotedStr(cmp) + "  OR AR.CMP = '*') AND (AR.STN=" + SQLUtils.QuotedStr(stn) + " OR AR.STN = '*') AND AR.GROUP_ID=" + SQLUtils.QuotedStr(groupId), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> roles = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                roles.Add(Prolink.Math.GetValueAsString(dr["FROLE_ID"]) + "|" + Prolink.Math.GetValueAsString(dr["GROUP_ID"]) + "|" + Prolink.Math.GetValueAsString(dr["CMP"]) + "|" + Prolink.Math.GetValueAsString(dr["STN"]));
            }
            return roles;
        }

        /// <summary>
        /// 获取用户的審核權限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="stn"></param>
        /// <param name="cmp"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetUserApproveGroup(string user, string stn, string cmp, string groupId)
        {
            //string user = Params["plv3.passport.user"];
            //string cmp = Params["plv3.passport.companyid"];
            //string groupId = Params["plv3.passport.groupid"];
            //string stn = Params["plv3.passport.station"];
            DataTable dt = OperationUtils.GetDataTable("SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP ON D.U_ID = DP.U_FID WHERE DP.USER_ID=" + SQLUtils.QuotedStr(user) + " AND (DP.CMP=" + SQLUtils.QuotedStr(cmp) + " OR DP.CMP = '*') AND (DP.STN=" + SQLUtils.QuotedStr(stn) + " OR DP.STN = '*') AND DP.GROUP_ID=" + SQLUtils.QuotedStr(groupId), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> groups = new Dictionary<string, object>();
            string roleItem = "";
            string groupName = "";
            foreach (DataRow dr in dt.Rows)
            {
                if (groupName == "" || Prolink.Math.GetValueAsString(dr["APPROVE_ATTR"]) != groupName)
                {
                    if (groupName != "")
                    {
                        groups[Prolink.Math.GetValueAsString(dr["APPROVE_ATTR"])] = roleItem;
                    }
                    groupName = Prolink.Math.GetValueAsString(dr["APPROVE_ATTR"]);
                    roleItem = Prolink.Math.GetValueAsString(dr["APPROVE_ATTR"]) + "|" + Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                }
                else
                {
                    roleItem += "|" + Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                }

            }
            if (roleItem != "")
            {
                groups[groupName] = roleItem;
            }

            return groups;
        }

        #region 检查登入ip和锁定
        public static bool CheckIPByCache(string userId, string cmp, string stn, HttpRequestBase request, User token = null,string unLock="N")
        {
            if (string.IsNullOrEmpty(userId) || "Y".Equals(unLock))
                return true;
            string id = string.Format("Token_{0}#{1}", userId, cmp);
            try
            {
                Cache cache = HttpContext.Current.Cache;
                if (token != null)
                {
                    cache.Insert(id, token, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
                }
                else
                {
                    token = cache.Get(id) as User;
                    if (token == null)
                    {
                        token = WebGui.Models.User.GetUserByCookie(userId, cmp, stn);
                        cache.Insert(id, token, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
                    }
                }
            }
            catch { }
            return CheckIP(request, token);
        }

        /// <summary>
        /// 移除IP缓存
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cmp"></param>
        public static void RemoveIPyCache(string userId, string cmp)
        {
            if (string.IsNullOrEmpty(userId))
                return;
            try
            {
                string id = string.Format("Token_{0}#{1}", userId, cmp);
                Cache cache = HttpContext.Current.Cache;
                cache.Remove(id);
            }
            catch { }
        }

        public static bool CheckIP(HttpRequestBase request, User userObj, string lockStr = "")
        {
            if (string.IsNullOrEmpty(lockStr) && userObj != null)
                lockStr = userObj.IPLocker;
            if (request == null || string.IsNullOrEmpty(lockStr))
            {
                return true;
            }

            string[] locks = lockStr.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < locks.Length; i++)
            {
                locks[i] = locks[i].Trim();
            }
            if (locks.Length > 0)
            {
                lockStr = string.Join(";", locks);
                locks = lockStr.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (locks.Length <= 0)
                return true;
            List<string> iplist = new List<string>() { };
            iplist.AddRange(locks);
            //iplist.Add("218.66.59.12");//查问题需要
            string ip = request.UserHostAddress;
            string name = request.UserHostName;
            if (string.IsNullOrEmpty(ip))//传入IP为空 说明前端有人做了手脚
                return false;
            if (iplist.Contains(ip))
                return true;
            if (string.IsNullOrEmpty(name))//传入主机为空 说明前端有人做了手脚
                return false;
            if (iplist.Contains(name))
                return true;
            return false;
        }
        #endregion
    }
}