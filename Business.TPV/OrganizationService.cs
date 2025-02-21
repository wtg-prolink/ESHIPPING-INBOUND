using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Business.TPV
{
    public class OrgService
    {
        public Employe GetSuperior(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            userId = userId.ToUpper();
            //Employe e = GetEmploye(userId);
            string res = null;
            return CreateEmploye(Agent.GetEmpPOrgManByAD(userId, out res));
            //return GetSuperior(e);
        }

        public Employe GetSuperior(string userId, string Company)
        {
            string sql = string.Format("SELECT CARD_NO FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}",
                SQLUtils.QuotedStr(userId), SQLUtils.QuotedStr(Company));
            string cardno = Business.Utils.DBManager.DefaultDB.GetValueAsString(sql);
            if (string.IsNullOrEmpty(cardno))
                return GetSuperior(userId);
            string res = null;
            return CreateEmploye(Agent.GetEmpPOrgManByID(cardno, out res));
        }

        public Employe GetEmpBaseByMail(string userId)
        {
            string res = null;
            return CreateEmploye(Agent.GetEmpBaseByMail(userId, out res));
        }

        public Employe GetOrgManByID(string OrgId)
        {
            string res = null;
            return CreateEmploye(Agent.GetOrgManByID(OrgId, out res));
        }

        //抓取上一级部门主管信息（P_type=1时为实线主管）
        public Employe GetOrgPManByID(string OrgId)
        {
            string res = null;
            return CreateEmploye(Agent.GetOrgPManByID(OrgId, out res));
        }

        public List<Employe> GetOrgPManListByID(string OrgId)
        {
            string res = null;
            return CreateEmployeList(Agent.GetOrgPManByID(OrgId, out res));
        }

        public Employe GetSuperior(Employe employe)
        {
            if (employe == null) return null;
            if (string.IsNullOrEmpty(employe.OrgID)) return null;
            string res = null;
            Employe superior = CreateEmploye(Agent.GetOrgPManByID(employe.OrgID, out res));
            //while (superior != null && superior.EmployeCode == employe.EmployeCode)
            //    superior = CreateEmploye(Agent.GetOrgPManByID(superior.OrgID, out res));
            return superior;
        }
        public Employe GetEmploye(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            string res = null;
            var v = CreateEmploye(Agent.GetEmpBaseByAD(userId, out res));
            string oRes = null;
            var o = CreateEmploye(Agent.GetEmpOrgByAD(userId, out oRes));
            if (o != null)
                v.OrgLevel = o.OrgLevel;
            return v;
        }

        IEnumerable<Employe> CreateEmployes(DataSet ds)
        {
            if (ds == null || ds.Tables == null || ds.Tables.Count <= 0) yield break;
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count <= 0) yield break;
            foreach (DataRow row in dt.Rows)
            {
                Employe emp = new Employe();
                Func<object, DateTime> getDateV = obj =>
                {
                    if (obj == null || obj == DBNull.Value) return DateTime.MinValue;
                    DateTime time;
                    if (DateTime.TryParse(obj.ToString(), out time))
                        return time;
                    return DateTime.MinValue;
                };
                foreach (DataColumn column in dt.Columns)
                {
                    switch (column.ColumnName)
                    {
                        case "Manid": emp.ID = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "IS_SAP_MAN": emp.IsSap = Prolink.Math.GetValueAsBool(row[column.ColumnName], false); break;
                        case "Name_C": emp.NameC = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Workingrule": emp.WorkingruleCode = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Mailaddr": emp.EMail = Prolink.Math.GetValueAsString(row[column.ColumnName]);
                            if (!string.IsNullOrEmpty(emp.EMail) && string.IsNullOrEmpty(emp.EmployeCode))
                            {
                                int index = emp.EMail.IndexOf("@");
                                if (index != -1)
                                    emp.EmployeCode = emp.EMail.Substring(0, index);
                            }
                            break;
                        case "Org_S_Txt": emp.OrgShortCode = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Org_Level": emp.OrgLevel = Prolink.Math.GetValueAsInt(row[column.ColumnName]); break;
                        case "Org_Cost": emp.OrgCostCode = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "is_sap_org": emp.IsSapOrg = Prolink.Math.GetValueAsBool(row[column.ColumnName], false); break;
                        case "is_sap_link": emp.IsSapLink = Prolink.Math.GetValueAsBool(row[column.ColumnName], false); break;
                        case "Emp_Site": emp.EmployeSite = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Emp_Cost": emp.EmployeCostCode = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Begda": emp.BegingDate = getDateV(row[column.ColumnName]); break;
                        case "Endda": emp.EndDate = getDateV(row[column.ColumnName]); break;
                        case "Ocode":
                        case "Orgid": emp.OrgID = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "AD": emp.EmployeCode = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Pcode_Level": emp.PCodeLevel = Prolink.Math.GetValueAsInt(row[column.ColumnName]); break;
                        case "Org_L_Txt": emp.OrgLocation = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Empygroup": emp.EmployeGroupCode = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Workdate": emp.WordDate = getDateV(row[column.ColumnName]); break;
                        case "org_site": emp.OrgSiteCode = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Ccode_Text": emp.JobTextC = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Jobename": emp.JobTextE = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "Name_E": emp.NameE = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "ManType": emp.EmployeType = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        case "P_Type": emp.Ptype = Prolink.Math.GetValueAsString(row[column.ColumnName]); break;
                        //
                        case "Empyno": break;
                        //case  "Name_C"        : break;
                        //case  "Name_E"        : break;
                        case "Sexnm": break;
                        case "Marriage_F": break;
                        case "Birthdate": break;
                        case "Education": break;
                        case "Educationdate": break;
                        case "Companycode": break;
                        case "Ocode_Text": break;
                        case "Pcode": break;
                        //case  "Ccode_Text"    : break;
                        //case  "Empygroup"     : break;
                        case "Empysubgroup": break;
                        //case  "Pcode_Level"   : break;
                        case "Hireddate": break;
                        case "Leavedate": break;
                        //case  "Workdate"      : break;
                        case "Groupdate": break;
                        case "Lpdate": break;
                        case "Empystatus": break;
                        //case  "Workingrule"   : break;
                        //case  "Mailaddr"      : break;
                        case "Overtime": break;
                        case "Overtime_Yn": break;
                        case "Performance_Yn": break;
                        case "Recordstatus": break;
                        case "Jobid": break;
                        //case  "Jobename"      : break;
                        case "Cost": break;
                        case "Trfst": break;
                        case "Zt01_Ext": break;
                        case "Sitetext": break;
                        //case  "AD"            : break;
                        case "Icnum1": break;
                        case "Icnum2": break;
                        case "Alnam": break;
                        case "Icbegda": break;
                        case "Icendda": break;
                        case "Address": break;
                        case "Cttyp": break;
                        case "Ctbeg": break;
                        case "Ctedt": break;
                        case "EDUCATIONCODE": break;
                        case "EDUCATIONSTATE": break;
                        case "Ownmailaddr": break;
                        case "Func": break;
                    }
                }
                yield return emp;
            }
        }

        Employe CreateEmploye(DataSet ds)
        {
            return CreateEmployes(ds).OrderBy(item => item.Ptype).FirstOrDefault();
        }

        List<Employe> CreateEmployeList(DataSet ds)
        {
            return CreateEmployes(ds).OrderBy(item => item.Ptype).ToList();
        }

        OrganizationService.Service _organizationAgent;
        CookieContainer _currentCookie;

        public OrganizationService.Service Agent
        {
            get
            {
                if (_organizationAgent == null)
                {
                    _currentCookie = new CookieContainer();
                    _organizationAgent = new OrganizationService.Service();
                    string url = GetServiceUrl();
                    if (!string.IsNullOrEmpty(url))
                        _organizationAgent.Url = url;
                    _organizationAgent.CookieContainer = _currentCookie;
                }
                return _organizationAgent;
            }
        }

        internal static string GetServiceUrl()
        {
            XmlDocument doc = Business.TPV.Context.GetSecurityDoc();
            var node = doc.SelectSingleNode("/root/Organization/Service");
            if (node != null)
                return node.Attributes["url"].Value;
            return null;
        }
    }

    public class Employe
    {
        /// <summary>
        /// 员工卡号
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 是否SAP员工
        /// </summary>
        public bool IsSap { get; set; }
        /// <summary>
        /// 是否SAP组织成员
        /// </summary>
        public bool IsSapOrg { get; set; }

        /// <summary>
        /// 是否SAP组织直线主管
        /// </summary>
        public bool IsSapLink { get; set; }

        /// <summary>
        /// 中文名称 
        /// </summary>
        public string NameC { get; set; }
        /// <summary>
        /// 英文名称
        /// </summary>
        public string NameE { get; set; }
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string EMail { get; set; }
        /// <summary>
        /// 费用代码
        /// </summary>
        public string EmployeCostCode { get; set; }
        /// <summary>
        /// 组织简易代码
        /// </summary>
        public string OrgShortCode { get; set; }
        /// <summary>
        /// 组织ID
        /// </summary>
        public string OrgID { get; set; }
        /// <summary>
        /// 组织等级
        /// </summary>
        public int OrgLevel { get; set; }
        /// <summary>
        /// 组织费用代码
        /// </summary>
        public string OrgCostCode { get; set; }
        /// <summary>
        /// 组织站点代码
        /// </summary>
        public string OrgSiteCode { get; set; }
        /// <summary>
        /// 组织地点
        /// </summary>
        public string OrgLocation { get; set; }
        /// <summary>
        /// 起始日期
        /// </summary>
        public DateTime BegingDate { get; set; }
        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 员工代码
        /// </summary>
        public string EmployeCode { get; set; }

        public string Ptype { get; set; }
        public int PCodeLevel { get; set; }
        public string EmployeGroupCode { get; set; }
        public DateTime WordDate { get; set; }
        /// <summary>
        /// 职位代码
        /// </summary>
        public string WorkingruleCode { get; set; }
        /// <summary>
        /// 员工站点
        /// </summary>
        public string EmployeSite { get; set; }
        /// <summary>
        /// 职位描述(中)
        /// </summary>
        public string JobTextC { get; set; }
        /// <summary>
        /// 职位描述(英)
        /// </summary>
        public string JobTextE { get; set; }
        public string EmployeType { get; set; }
    }
}
