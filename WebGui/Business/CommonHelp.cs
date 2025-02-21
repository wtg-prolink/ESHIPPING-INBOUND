using Business.Mail;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using WebGui.Models;
using Prolink.V3;
using Prolink;
using System.Globalization;
using System.Xml;
using System.Threading;
using System.Web.Configuration;
namespace Business
{
    public class CommonHelp
    {

        public static Dictionary<string, object> siteInfosStn = new Dictionary<string, object>();
        public static Dictionary<string, object> siteInfosDep = new Dictionary<string, object>();

        public static string getBscodeForCheckbox(string CdType, string pmsCon, string fieldName)
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
                    sel += "<label class='chx'>";
                    sel += string.Format("<input type='checkbox' dt='mt' chxName='{0}' name='{0}' value='{1}'>{2}", fieldName, Cd, CdDescp); 
                    sel += "</label>";
                    if (Cd == "FORK") 
                    {
                        sel += "<label class='chx'>";
                        sel += "<input type='text' class='form-control input-sm' dt='mt' id='Fork' name='Fork' fieldname='Fork' isnumber='true' />";
                        sel += "</label>";
                    }
                }
            }

            return sel;
        }

        public static string getBscodeForCheckbox1(string CdType, string pmsCon, string fieldName)
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
                    sel += "<label class='chx'>";
                    sel += string.Format("<input type='checkbox' dt='mt' chxName='{0}' name='{0}' value='{1}'>{2}", fieldName, CdDescp, CdDescp);
                    sel += "</label>";
                }
            }

            return sel;
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
                    sel += "<option value='" + Cd + "'>" + Cd+":"+CdDescp + "</option>";
                }
            }

            return sel;
        }


        public static string GetBscodeSelect(string Bscode, string column, string Apcd = "")
        {
            string sql = string.Format("SELECT CD,CD_DESCP,AR_CD FROM BSCODE WHERE CD_TYPE={0} ", SQLUtils.QuotedStr(Bscode));
            if (!string.IsNullOrEmpty(Apcd))
            {
                sql += string.Format(" AND AP_CD={0}", SQLUtils.QuotedStr(Apcd));
            }

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]);
                select += ":" + Prolink.Math.GetValueAsString(dr[column]);
            }
            return select;
        }

        public static string getReffeeForColSelect(string pmsCon)
        {
            string sel = string.Empty;
            string sql = "SELECT CHG_CD, CHG_DESCP FROM ECREFFEE WHERE {0} AND CHG_CD <> 'F1' AND CHG_CD <> 'F2' AND CHG_CD <> 'F3'";
            sql = string.Format(sql, pmsCon);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string ChgCd = Prolink.Math.GetValueAsString(item["CHG_CD"]);
                    string ChgDescp = Prolink.Math.GetValueAsString(item["CHG_DESCP"]);
                    sel += ChgCd + ":" + ChgDescp + ";";
                }
                sel = sel.Remove(sel.Length - 1);
            }
            return sel;
        }

        public static string getReffeeForSelect(string pmsCon)
        {
            string sel = string.Empty;
            string sql = "SELECT CHG_CD, CHG_DESCP FROM ECREFFEE WHERE {0} AND FEE_TYPE='O' AND CHG_CD NOT IN ('F1','F2','F3') order by SEQ_NO";
            sql = string.Format(sql, pmsCon);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string ChgCd = Prolink.Math.GetValueAsString(item["CHG_CD"]);
                    string ChgDescp = Prolink.Math.GetValueAsString(item["CHG_DESCP"]);
                    sel += "<option value='" + ChgCd + "'>" + ChgCd + ":" + ChgDescp + "</option>";
                }
                //sel = sel.Remove(sel.Length - 1);
            }
            return sel;
        }

        public static string getDnNoList(string combineInfo)
        {
            if (string.IsNullOrEmpty(combineInfo)) return "";
            string sel = string.Empty;
            string sql = string.Format("SELECT Name FROM dbo.splitstring(({0}))", SQLUtils.QuotedStr(combineInfo));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string name = Prolink.Math.GetValueAsString(item["Name"]); 
                    sel += name + ":" + name + ";";
                }
                sel = sel.Remove(sel.Length - 1);
            }
            return sel;
        }
        
        public static string getBscodeForColModel(string CdType, string pmsCon)
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
                    sel += Cd + ":" + CdDescp + ";";
                }
                sel = sel.Remove(sel.Length - 1);
            }           
            return sel;
        }

        public static string getBscodeForColModelDescp(string CdType, string pmsCon)
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
                    sel += CdDescp + ":" + CdDescp + ";";
                }
                sel = sel.Remove(sel.Length - 1);
            }
            return sel;
        }

        public static Dictionary<string, object> parseArrayJson(System.Collections.ArrayList dict)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            foreach (Dictionary<string, object> item in dict)
            {
                foreach (var item2 in item.Keys)
                {

                }

            }

            return obj;
        }

        #region 匯率轉換
        public static decimal changeExrate(string Fcur, string Tcur, decimal Amt)
        {
            decimal result = 0;
            string sql = string.Empty;
            double exRate = 0;
            sql = "SELECT TOP 1 EX_RATE FROM BSERATE WHERE GROUP_ID='TPV' AND FCUR={0} AND TCUR={1} ORDER BY EDATE DESC";
            sql = string.Format(sql, SQLUtils.QuotedStr(Fcur), SQLUtils.QuotedStr(Tcur));
            exRate = WebGui.Controllers.BaseController.getOneValueAsDoubleFromSql(sql);

            if(exRate > 0)
            {
                result = Amt * Convert.ToDecimal(exRate);
            }
            else
            {
                result = Amt;
            }
            
            return result;
        }
        #endregion

        #region 汇率转换
        public static decimal changeExrate_new(string Fcur, string Tcur, decimal Amt, out double exRate)
        {
            decimal result = 0;
            string sql = string.Empty;
            exRate = 0;
            sql = "SELECT TOP 1 EX_RATE FROM BSERATE WHERE GROUP_ID='TPV' AND FCUR={0} AND TCUR={1} ORDER BY EDATE DESC";
            sql = string.Format(sql, SQLUtils.QuotedStr(Fcur), SQLUtils.QuotedStr(Tcur));
            exRate = WebGui.Controllers.BaseController.getOneValueAsDoubleFromSql(sql);

            if (exRate > 0)
            {
                result = Amt * Convert.ToDecimal(exRate);
            }
            else
            {
                result = Amt;
            }

            return result;
        }
        #endregion

        #region 幣別小數
        public static double formatCur(string Cur, double num)
        {
            string sql = string.Empty;

            sql = "SELECT * FROM BSCUR WHERE CUR=" + SQLUtils.QuotedStr(Cur);
            DataTable dt = getDataTableFromSql(sql);
            int RoundType = 0;
            int DecimalPoint = 0;
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    RoundType = Prolink.Math.GetValueAsInt(item["ROUND_TYPE"]);
                    DecimalPoint = Prolink.Math.GetValueAsInt(item["DECIMAL_POINT"]);

                }
            }

            switch (RoundType)
            {
                case 0:
                    num = System.Math.Floor(num);
                    break;
                case 1:
                    num = System.Math.Ceiling(num);
                    break;
                case 2:
                    num = System.Math.Round(num, DecimalPoint);
                    break;
                default:
                    num = System.Math.Floor(num);
                    break;
            }


            return num;
        }

        public static decimal formatCur(string Cur, decimal num)
        {
            string sql = string.Empty;

            sql = "SELECT * FROM BSCUR WHERE CUR=" + SQLUtils.QuotedStr(Cur);
            DataTable dt = getDataTableFromSql(sql);
            int RoundType = 0;
            int DecimalPoint = 0;
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    RoundType = Prolink.Math.GetValueAsInt(item["ROUND_TYPE"]);
                    DecimalPoint = Prolink.Math.GetValueAsInt(item["DECIMAL_POINT"]);

                }
            }

            switch (RoundType)
            {
                case 0:
                    num = System.Math.Floor(num);
                    break;
                case 1:
                    num = System.Math.Ceiling(num);
                    break;
                case 2:
                    num = System.Math.Round(num, DecimalPoint);
                    break;
                default:
                    num = System.Math.Floor(num);
                    break;
            }


            return num;
        }
        #endregion

        public static DataTable GetChargeCodesByVender(string pms, string VenderCd)
        {
            string sql = string.Empty;
            sql = "SELECT CHG_CD, CHG_DESCP, GW, CBM, REMARK FROM ECREFFEE WHERE " + pms + " AND VENDER_CD='" + VenderCd + "' ORDER BY SEQ_NO ASC";
            DataTable dt = getDataTableFromSql(sql);

            return dt;
        }

        public static DataTable GetChargeCodes(string pms, int type = 1)
        {
            string sql = string.Empty;
            if (type == 1)
                sql = "SELECT CHG_CD, CHG_DESCP, GW, CBM, REMARK FROM ECREFFEE WHERE " + pms + " AND FEE_TYPE='O' ORDER BY SEQ_NO ASC";
            else if(type==4)
                sql = "SELECT CHG_CD, CHG_DESCP, GW, CBM, REMARK FROM ECREFFEE WHERE " + pms + " AND FEE_TYPE='I' ORDER BY SEQ_NO ASC";
            else
                sql = "SELECT CHG_CD, CHG_DESCP, GW, CBM, REMARK FROM ECREFFEE WHERE " + pms + " AND FEE_TYPE='O' AND CHG_CD NOT IN('F1', 'F2', 'F3') ORDER BY SEQ_NO ASC";
            DataTable dt = getDataTableFromSql(sql);

            return dt;
        }


        #region sql 执行
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

        public static void AddRoles(string roleId, string groupId, string cmp, string stn, List<Role> roleList, string fdescp = "")
        {
            Role role = new Role() { RoleId = roleId, GroupId = groupId, Cmp = cmp, Stn = stn, RoleDesc = fdescp };
            role.Id = string.Format("{0}|{1}|{2}|{3}", role.RoleId, role.GroupId, role.Cmp, role.Stn);
            bool add = true;
            foreach (var r in roleList)
            {
                if (role.Id.Equals(r.Id))
                {
                    add = false;
                    break;
                }
            }
            if (add) roleList.Add(role);
        }

        public static void RebuildPermission(string GroupId, string CompanyId, string Station)
        {
            try
            {
                string sys_version = WebConfigurationManager.AppSettings["SYS_VERSION"];
                DataTable dt = OperationUtils.GetDataTable("SELECT DISTINCT FROLE_ID FROM ROLE_REBUID WITH(NOLOCK) WHERE IO_TYPE='I'", null, Prolink.Web.WebContext.GetInstance().GetConnection());

                bool buid = false;
                foreach (DataRow dr in dt.Rows)
                {
                    string roleId = Prolink.Math.GetValueAsString(dr["FROLE_ID"]);
                    if (CheckRebuildPermission(roleId, "", GroupId, CompanyId, Station))
                        buid = true;

                }
                if (buid)
                {
                    Prolink.V3.PermissionManager.GetEdocPermission();
                }
            }
            catch { }
        }

        /// <summary>
        /// 验证是否需要重新抓取权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="pmsId"></param>
        /// <param name="GroupId"></param>
        /// <param name="CompanyId"></param>
        /// <param name="Station"></param>
        public static bool CheckRebuildPermission(string roleId, string pmsId, string GroupId, string CompanyId, string Station)
        {
            try
            {
                if ("EDOC".Equals(pmsId))
                    roleId = "EDOC";
                string sys_version = WebConfigurationManager.AppSettings["SYS_VERSION"];
                DataTable dt = OperationUtils.GetDataTable("SELECT SYS_VERSION,V_COUNT FROM ROLE_REBUID WHERE IO_TYPE='I' AND FROLE_ID=" + SQLUtils.QuotedStr(roleId) + " ORDER BY V_COUNT", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                bool buid = false;
                int max = dt.Rows.Count > 0 ? Prolink.Math.GetValueAsInt(dt.Rows[dt.Rows.Count - 1]["V_COUNT"]) : 0;
                if (max <= 0)
                    return false;

                DataRow[] drs = dt.Select(string.Format("SYS_VERSION={0}", SQLUtils.QuotedStr(sys_version)));
                int count = drs.Length > 0 ? Prolink.Math.GetValueAsInt(drs[0]["V_COUNT"]) : 0;
                buid = count < max;

                if (!buid)
                    return false;

                if ("EDOC".Equals(pmsId))
                    Prolink.V3.PermissionManager.GetEdocPermission();
                else
                {
                    List<Role> roleList = new List<Role>();
                    Business.CommonHelp.AddRoles(roleId, GroupId, CompanyId, Station, roleList);
                    Prolink.V3.PermissionManager.SetRolePermission(roleList);
                }
                EditInstruct ei = new EditInstruct("ROLE_REBUID", EditInstruct.INSERT_OPERATION);
                ei.Put("SYS_VERSION", sys_version);
                ei.Put("FROLE_ID", roleId);
                ei.Put("V_COUNT", max);
                ei.Put("IO_TYPE", "I");
                if (drs.Length > 0)
                {
                    ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    ei.AddKey("SYS_VERSION");
                    ei.AddKey("FROLE_ID");
                    ei.AddKey("IO_TYPE");
                }
                ei.PutDate("CREATE_DATE", DateTime.Now);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 新增权限变更通知
        /// </summary>
        /// <param name="roles"></param>
        public static void NotifyRebuidPermission(List<Role> roles, bool isEdoc = false)
        {
            try
            {
                List<string> roleList = new List<string>();
                if (roles != null)
                {
                    foreach (var r in roles)
                    {
                        if (!string.IsNullOrEmpty(r.Id) && !roleList.Contains(r.RoleId))
                            roleList.Add(r.RoleId);
                    }
                }
                if (isEdoc)
                    roleList.Add("EDOC");
                if (roleList.Count <= 0)
                    return;
                string sys_version = WebConfigurationManager.AppSettings["SYS_VERSION"];
                DataTable dt = OperationUtils.GetDataTable("SELECT SYS_VERSION,V_COUNT,FROLE_ID FROM ROLE_REBUID WHERE IO_TYPE='I' AND FROLE_ID IN " + SQLUtils.Quoted(roleList.ToArray()) + " ORDER BY V_COUNT", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                MixedList ml = new MixedList();
                foreach (var roleId in roleList)
                {
                    DataRow[] drs = dt.Select(string.Format("FROLE_ID={0} AND SYS_VERSION={1}", SQLUtils.QuotedStr(roleId), SQLUtils.QuotedStr(sys_version)), "V_COUNT DESC");
                    int count = drs.Length > 0 ? Prolink.Math.GetValueAsInt(drs[0]["V_COUNT"]) : 0;

                    EditInstruct ei = new EditInstruct("ROLE_REBUID", EditInstruct.INSERT_OPERATION);
                    ei.Put("SYS_VERSION", sys_version);
                    ei.Put("FROLE_ID", roleId);
                    ei.Put("V_COUNT", count + 1);
                    ei.Put("IO_TYPE", "I");
                    if (drs.Length > 0)
                    {
                        ei.OperationType = EditInstruct.UPDATE_OPERATION;
                        ei.AddKey("SYS_VERSION");
                        ei.AddKey("FROLE_ID");
                        ei.AddKey("IO_TYPE");
                    }
                    ei.PutDate("CREATE_DATE", DateTime.Now);
                    ml.Add(ei);
                }
                if (ml.Count > 0)
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { }
        }

        /// <summary>
        /// 获取shipToParty
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static string getShipToParty(string uid)
        {
            string shipToParty = string.Empty;
            string sql = string.Format("SELECT PARTY_NO FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE='WE'", SQLUtils.QuotedStr(uid));
            DataTable dt = getDataTableFromSql(sql);
            if (dt.Rows.Count <= 0) return shipToParty;
            shipToParty = Prolink.Math.GetValueAsString(dt.Rows[0]["PARTY_NO"]);
            return shipToParty;
        }
    }
}