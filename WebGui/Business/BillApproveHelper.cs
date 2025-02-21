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
using WebGui.UICMessageSrv;

namespace Business
{
    public class BillApproveHelper
    {

        #region 获取某资讯by Uid
        public static DataTable GetDataByUId(string uid)
        {
            string sql = string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }
        #endregion

        #region 签核退回
        public static string BillApproveBack(string uid, string backremark, UserInfo userinfo)
        {
            string returnMsg = "";
            DataTable nowdata = Business.BillApproveHelper.GetDataByUId(uid);
            if (nowdata.Rows.Count <= 0) return @Resources.Locale.L_BillApproveHelper_Business_22;

            string aptype = nowdata.Rows[0]["APPROVE_TYPE"].ToString();
            string DebitNo = nowdata.Rows[0]["DEBIT_NO"].ToString();
            string ApproveTo = nowdata.Rows[0]["APPROVE_TO"].ToString();
            string groupid = nowdata.Rows[0]["GROUP_ID"].ToString();
            string cmp = nowdata.Rows[0]["CMP"].ToString();
            string noticeuser = string.Empty;
            string noticemail = string.Empty;
            MixedList mixList = new MixedList();

            string conditions = string.Format("REF_NO={0} AND APPROVE_CODE={1} ", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype));

            string sql = string.Format("SELECT * FROM APPROVE_RECORD WHERE {0} AND ROLE={1}", conditions, SQLUtils.QuotedStr(ApproveTo));
            DataTable dtap = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            bool isok = CheckBackLevel(ApproveTo, userinfo.UserId, userinfo.DataCmp, userinfo.GroupId, "", userinfo.Upri, userinfo.Dep);
            if (dtap.Rows.Count > 0)
            {
                string notice_to = Prolink.Math.GetValueAsString(dtap.Rows[0]["NOTICE_TO"]);
                if (!string.IsNullOrEmpty(notice_to))
                {
                    if (userinfo.UserId.ToUpper() == notice_to.ToUpper())
                    {
                        isok = true;
                    }
                }
            }
            if (!isok)
            {
                return returnMsg = @Resources.Locale.L_BillApproveHelper_Business_23 + DebitNo + @Resources.Locale.L_BillApproveHelper_Business_24;
            }

            sql = string.Format(@"SELECT AFD_ROLE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2} ", SQLUtils.QuotedStr(uid),
                    SQLUtils.QuotedStr(aptype), SQLUtils.QuotedStr(ApproveTo));

            string afdrole = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if ("A".Equals(afdrole) || "AC".Equals(afdrole))    //针对退回申请者状态
            {
                afdrole = "A";
                mixList.Add(Business.BillApproveHelper.UpApproveStatus(uid, afdrole,userinfo));
                sql = string.Format("UPDATE APPROVE_RECORD SET STATUS='0' WHERE REF_NO={0} AND APPROVE_CODE={1}",
                    SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype));
                mixList.Add(sql);
                backremark = @Resources.Locale.L_BillApproveHelper_Business_29 + userinfo.UserId + @Resources.Locale.L_BillApproveHelper_Business_30 + backremark;
                //sql = string.Format("UPDATE APPROVE_RECORD SET APPROVE_BY={2},APPROVE_DATE=GETDATE(),REMARK={3} WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL=1",
                //    SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype),SQLUtils.QuotedStr(userinfo.UserId), SQLUtils.QuotedStr(backremark));
                //mixList.Add(sql);

                EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                apei.PutKey("REF_NO", uid);
                apei.PutKey("APPROVE_CODE", aptype);
                apei.PutKey("APPROVE_LEVEL", 1);
                apei.Put("APPROVE_BY", userinfo.UserId);
                apei.PutDate("APPROVE_DATE", DateTime.Now);
                apei.PutDate("APPROVE_DATE_L", TimeZoneHelper.GetTimeZoneDate(userinfo.CompanyId));
                apei.Put("REMARK", backremark);
                mixList.Add(apei);

                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                return @Resources.Locale.L_BillApproveHelper_Business_25;
            }
            else if ("50LEVEL".Equals(afdrole))
            {
                sql = string.Format("SELECT APPROVE_BY FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                    SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype), SQLUtils.QuotedStr(afdrole));
                noticeuser = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(noticeuser));
                noticemail = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                mixList.Add(Business.BillApproveHelper.UpApproveStatus(uid, afdrole, userinfo, noticeuser));
                sql = string.Format("UPDATE APPROVE_RECORD SET STATUS='0' WHERE REF_NO={0} AND APPROVE_CODE={1} AND AFD_ROLE='50LEVEL'",
                    SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype));
                mixList.Add(sql);

                EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                apei.PutKey("REF_NO", uid);
                apei.PutKey("APPROVE_CODE", aptype);
                apei.PutKey("ROLE", "50LEVEL");
                apei.Put("APPROVE_BY", userinfo.UserId);
                apei.PutDate("APPROVE_DATE", DateTime.Now);
                apei.PutDate("APPROVE_DATE_L", TimeZoneHelper.GetTimeZoneDate(userinfo.CompanyId));
                apei.Put("NOTICE_TO", noticeuser);
                apei.Put("NOTICE_MAIL", noticemail);
                apei.Put("STATUS", "0");
                backremark = @Resources.Locale.L_BillApproveHelper_Business_29 + userinfo.UserId + @Resources.Locale.L_BillApproveHelper_Business_30 + backremark;
                apei.Put("REMARK", backremark);
                mixList.Add(apei);

                UICHandle uic = new UICHandle(uid, "", aptype, userinfo);
                UICReturnMSG msg = uic.SendUICMessage("UICClose");
                if (msg.type.ToLower() == "fail" || (msg.type.ToLower() == "error" && msg.code != "400"))
                {
                    return "UIC Close Fail:" + msg.message;
                }

                string subject = "Debit No:" + DebitNo + @Resources.Locale.L_BillApproveHelper_Business_32 + backremark;
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg += AddToNoticeBill("", userinfo.basecondtions, subject, noticeuser, groupid, cmp, userinfo.Dep, uid, noticeuser, noticemail);
                return @Resources.Locale.L_BillApproveHelper_Business_31;
            }

            sql = string.Format(@"SELECT TOP 1 * FROM APPROVE_RECORD WHERE {0}  AND STATUS='1' AND CAST(APPROVE_LEVEL AS INT)
                <(SELECT TOP 1 CAST(APPROVE_LEVEL AS INT) FROM APPROVE_RECORD WHERE  {0} AND ROLE={1})
                 ORDER BY CAST(APPROVE_LEVEL AS INT) DESC", conditions, SQLUtils.QuotedStr(ApproveTo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count == 0)
            {
                if (ApproveTo == "A")
                {

                    SendBillRJMail(uid, userinfo.GroupId,userinfo.CompanyId);
                    try
                    {
                        EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("U_ID", uid);
                        ei.Put("STATUS", "C");
                        ei.Put("APPROVE_USER", null);
                        ei.Put("CHECK_DESCP", backremark);
                        ei.Put("APPLY_BY", null);
                        OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                        return @Resources.Locale.L_BillApproveHelper_Business_26;
                    }
                    catch (Exception ex)
                    {

                    }
                }

                return @Resources.Locale.L_BillApproveHelper_Business_27;
            }
            return returnMsg;
        }
        #endregion

        public static string Approve_Type = "STD_BILL";

        public static EditInstruct UpApproveStatus(string uid, string NextDnType,UserInfo userInfo, string noticeuser = "")
        {
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", NextDnType);
            ei.Put("APPROVE_USER", noticeuser); //更新此笔签核人员到上一层
            BillApproveNew.SetApprove(ei, userInfo);
            return ei;
        }

        public static string GetReturnRole(MixedList mixList, string uid, string apcode, string afdrole, string dep)
        {
            //            string sql = string.Format(@"SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE={0} AND 
            //                CAST(APPROVE_LEVEL AS INT)<(SELECT TOP 1 CAST(APPROVE_LEVEL AS INT) FROM APPROVE_FLOW_D WHERE APPROVE_CODE={0} AND ROLE={1})
            //                ORDER BY CAST(APPROVE_LEVEL AS INT) DESC",
            //              SQLUtils.QuotedStr(apcode), SQLUtils.QuotedStr(afdrole));

            string sql = string.Format(@"SELECT * FROM APPROVE_RECORD WHERE APPROVE_CODE={0} AND REF_NO={1}
        AND STATUS='1'AND CAST(APPROVE_LEVEL AS INT)<(SELECT TOP 1 CAST(APPROVE_LEVEL AS INT) FROM APPROVE_RECORD
				 WHERE APPROVE_CODE={0} AND REF_NO={1} AND ROLE={2} )
                ORDER BY CAST(APPROVE_LEVEL AS INT) DESC",
              SQLUtils.QuotedStr(apcode), SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(afdrole));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return string.Empty;
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", uid);
            apei.PutKey("APPROVE_CODE", apcode);
            apei.PutKey("AFD_ROLE", afdrole);
            apei.Put("STATUS", "0");
            mixList.Add(apei);
            string looprole = Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);
            foreach (DataRow dr in dt.Rows)
            {
                if ("Y".Equals(Prolink.Math.GetValueAsString(dr["BACK_FLAG"])))
                {
                    looprole = Prolink.Math.GetValueAsString(dr["ROLE"]);
                    break;
                }
                else
                {
                    EditInstruct ei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("REF_NO", uid);
                    ei.PutKey("APPROVE_CODE", apcode);
                    ei.PutKey("AFD_ROLE", Prolink.Math.GetValueAsString(dr["ROLE"]));
                    ei.Put("STATUS", "0");
                    mixList.Add(ei);
                }
            }
            return looprole;
        }

        public static bool CheckLastOK(string uid, string dntype, string role, string noticeto,string userid)
        {
            string sql = string.Format("SELECT STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(dntype), SQLUtils.QuotedStr(role));
            DataTable dt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count == 1)
            {
                if (!string.IsNullOrEmpty(noticeto))
                {
                    sql += NoticeToSql(noticeto);
                }
            }
            else if (dt.Rows.Count > 1)
            {
                if (!string.IsNullOrEmpty(userid))
                {
                    sql += NoticeToSql(userid);
                }
            }
            string value = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (value.Equals("1"))
            {
                return true;
            }
            else if (value.Equals("0"))
            {
                return false;
            }
            return false;
        }

        private static string NoticeToSql(string approveuser)
        {
            return string.Format(" AND NOTICE_TO={0}", SQLUtils.QuotedStr(approveuser));
        }

        public static bool CheckApproveLevel(string apprvesto, string UserId, string CompanyId, string GroupId, string approveuser, string upri, string dep,string uid)
        {
            bool check = false;
            string sql = string.Format("SELECT DISTINCT NOTICE_TO FROM APPROVE_RECORD WHERE REF_NO={0} AND ROLE={1} AND STATUS='0'", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(apprvesto));
            DataTable noticeToDt = OperationUtils.GetDataTable(sql, null,Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach(DataRow dr in noticeToDt.Rows)
            {
                string noticeTo = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                if (!string.IsNullOrEmpty(noticeTo))
                    if (UserId.ToLower() == noticeTo.ToLower()) return true;
            }
            if (!string.IsNullOrEmpty(approveuser))
            {
                if (UserId.ToLower() == approveuser.ToLower()) return true;
            }
            if ("MANA10".Equals(UserId)) return true;
            if ("ADMIN".Equals(UserId)) return true;
            string approvelists = GetApproveBack(UserId, CompanyId, GroupId, upri, dep);
            string[] approvelist = approvelists.Split(';');
            foreach (string item in approvelist)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (item.Equals(apprvesto))
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        public static bool CheckBackLevel(string apprvesto, string UserId, string CompanyId, string GroupId, string approveuser, string upri, string dep)
        {
            bool check = false;
            if (UserId == approveuser) return true;
            if ("MANA10".Equals(UserId)) return true;
            if ("ADMIN".Equals(UserId)) return true;
            string approvelists = GetApproveBack(UserId, CompanyId, GroupId, upri, dep);
            string[] approvelist = approvelists.Split(';');
            foreach (string item in approvelist)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (item.Equals(apprvesto))
                {
                    check = true;
                    break;
                }
            }
            return check;
        }
        public static string GetApprove(string UserId, string CompanyId, string GroupId, string upri, string Dep)
        {
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR='BILLING'",//AND DP.STN={2}
             SQLUtils.QuotedStr(UserId),
             SQLUtils.QuotedStr(CompanyId),
             SQLUtils.QuotedStr(GroupId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<string> approvelist = new List<string>();

            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };

            approvelist.Add("Person");//申请者的状态下也可以出现  
            approvelist.Add("A");

            if ("U" != upri && ("LST".Equals(Dep) || "GLST".Equals(Dep)))
            {
                approvelist.Add(Dep + "M");
                approvelist.Add(Dep + "MM");
                approvelist.Add(Dep + "MMM");
                approvelist.Add(Dep + "MMMM");
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approvelist.Add(Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_GROUP"]));

            }
            return string.Join(";", approvelist);
        }

        public static string GetApproveBack(string UserId, string CompanyId, string GroupId, string upri, string Dep)
        {
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND DP.CMP=D.CMP AND APPROVE_ATTR='BILLING'",//AND DP.STN={2}
             SQLUtils.QuotedStr(UserId),
             SQLUtils.QuotedStr(CompanyId),
             SQLUtils.QuotedStr(GroupId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string approveroles = "Person;A;";//申请者的状态下也可以出现  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approveroles += Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_GROUP"]) + ";";
            }
            approveroles = approveroles.Trim(';');
            return approveroles;
        }

        private static EditInstruct ApproveEdit(string RefNo, string role, string approvecode, string approvename, string rolename, string approvetime, int j,Business.TPV.Employe employe, string afdrole)
        {
            EditInstruct levelei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
            string userid = "";
            if (employe != null)
            {
                userid = employe.EmployeCode;
                levelei.Put("SAP_LEVEL", employe.OrgLevel);
                levelei.Put("NOTICE_TO", employe.EmployeCode);
                levelei.Put("NOTICE_MAIL", employe.EMail);//employe.EMail
            }
            levelei.Put("STATUS", "0");
            levelei.Put("ROLE", role);//Dep + "MM"
            levelei.Put("ROLE_NM", rolename);
            levelei.Put("REF_NO", RefNo);
            levelei.Put("APPROVE_LEVEL", j);
            int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(RefNo)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;
            levelei.Put("VOID_LOOP", max);
            //if (noticetime)
            //{
            //    levelei.PutDate("NOTIFY_DATE", DateTime.Now);
            //}
            levelei.Put("AFD_ROLE", afdrole);
            levelei.Put("APPROVE_CODE", approvecode);
            levelei.Put("APPROVE_NAME", approvename);
            if (string.IsNullOrEmpty(approvename))
                levelei.Put("APPROVE_NAME", rolename);
            levelei.Put("APPROVE_TIME", approvetime);
            //if (IsFirstFifty)
            //{
            //    levelei.Put("STATUS", "1");
            //    levelei.Put("APPROVE_BY", userid);
            //    levelei.PutDate("APPROVE_DATE", DateTime.Now);
            //    levelei.PutDate("NOTIFY_DATE", DateTime.Now);
            //    levelei.Put("REMARK", @Resources.Locale.L_ContractQuery_Views_478);
            //    levelei.Put("NOTICE_TO", "");
            //    levelei.Put("NOTICE_MAIL", "");
            //}
            return levelei;
        }

        private static string NewFiftyApprove(Business.TPV.Employe employe, string DnNo, UserInfo userinfo, MixedList mixList, string approvecode, string approvetime, ref int j, ref string userid, ref bool isOkTime, ref string NextDnType, int place, decimal amt)
        {
            if (string.IsNullOrEmpty(userid)) userid = userinfo.UserId;
            string rolename = string.Empty, role = userinfo.Dep;
            for (int i = 0; i < place; i++)
            {
                role += "M";
            }
            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
            int k = 0;
            while (employe.OrgLevel < 50 && k < 10)
            {
                employe = Business.TPV.Context.OrgService.GetSuperior(employe);
                if (employe != null)
                {
                    rolename = employe.OrgLevel + "LEVEL";
                    if (employe.OrgLevel < 50)
                    {
                        j++;
                        EditInstruct levelbefor = ApproveEdit(DnNo, rolename, approvecode, "", rolename, approvetime, j, employe, "A");
                        mixList.Add(levelbefor);
                    }
                }
                k++;
            }
            rolename = GetApproveName("50LEVEL", "50层级", userinfo);
            if (string.IsNullOrEmpty(NextDnType)) NextDnType = "50LEVEL";
            string afdRole = userinfo.Dep + "M";
            isOkTime = false;
            if (k > 0)
            {
                j++;
                EditInstruct level = ApproveEdit(DnNo, "50LEVEL", approvecode, "", rolename, approvetime, j, employe, "A");
                afdRole = "50LEVEL";
                mixList.Add(level);
            }



            //增加会签人员信息
            string sql = string.Format("SELECT * FROM APPROVE_SIGN WHERE CMP={0} AND SIGN_TYPE='A' ORDER BY SIGN_ORDER", SQLUtils.QuotedStr(userinfo.CompanyId));
            DataTable signTable = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstructList eilist = new EditInstructList();
            foreach (DataRow signrow in signTable.Rows)
            {
                string signid = Prolink.Math.GetValueAsString(signrow["SIGN_ID"]);
                signid = signid.Replace(userid, "");
                if (string.IsNullOrEmpty(signid)) continue;

                List<string> signlist = signid.Split(';').ToList();

                bool isequalle = false;
                for (int i = 0; i < signlist.Count; i++)
                {
                    string sign = signlist[i];
                    if (string.IsNullOrEmpty(sign)) continue;
                    if (!isequalle)
                    {
                        j++;
                        isequalle = true;
                    }
                    Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetEmploye(sign);
                    if (employe1 == null)
                        return "50 level sign Info.：" + sign + "Get failed from OA System！";
                    rolename = employe1.OrgLevel + "LEVEL Counter Sign";
                    EditInstruct fiftyei = ApproveEdit(DnNo, rolename, approvecode, "", rolename, approvetime, j, employe1, afdRole);
                    eilist.Add(fiftyei);
                }
            }

            sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE,AP_CD,AR_CD FROM BSCODE WHERE GROUP_ID={0} AND(CMP={1} OR CMP='*') AND CONVERT(float,CD)<" + amt + " AND CD_TYPE ='ATST' ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.CompanyId));
            DataTable approvedt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (approvedt.Rows.Count > 0)
            {
                for (int i = 0; i < eilist.Count; i++)
                {
                    mixList.Add(eilist[i]);
                }
                List<TPV.Employe> fiftyManagerList = new List<TPV.Employe>();
                foreach (DataRow dr in approvedt.Rows)
                {
                    string apcd = Prolink.Math.GetValueAsString(dr["AP_CD"]);
                    role = apcd + "LEVEL";
                    rolename = Prolink.Math.GetValueAsString(dr["AR_CD"]);

                    if ("55".Equals(apcd))  //55有两层一个是抓直线主管，一个是抓取GLST主管
                    {
                        List<TPV.Employe> employelist = GetPManListByOrgid(employe.OrgID);
                        if (employelist.Count > 0)
                        {
                            foreach (TPV.Employe emp in employelist)
                            {
                                if (emp.OrgLevel == 55)
                                {
                                    j++;
                                    if (emp.IsSapLink)
                                    {
                                        employe = emp;
                                    }
                                    EditInstruct fiftyei = ApproveEdit(DnNo, role, approvecode, "", rolename, approvetime, j, emp, afdRole);
                                    mixList.Add(fiftyei);
                                }
                            }
                        }
                        TPV.Employe emp58level = Get58Level(employe, ref fiftyManagerList);
                        j++;
                        EditInstruct eityei = ApproveEdit(DnNo, "58LEVEL", approvecode, "", "58LEVEL", approvetime, j, emp58level, afdRole);
                        mixList.Add(eityei);

                        string sql1 = string.Format("SELECT * FROM APPROVE_SIGN WHERE CMP={0} AND SIGN_TYPE='B' ORDER BY SIGN_ORDER", SQLUtils.QuotedStr(userinfo.CompanyId));
                        DataTable signTableb = OperationUtils.GetDataTable(sql1, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        foreach (DataRow signrow in signTableb.Rows)
                        {
                            string signid = Prolink.Math.GetValueAsString(signrow["SIGN_ID"]);
                            signid = signid.Replace(userid, "");
                            if (string.IsNullOrEmpty(signid)) continue;

                            List<string> signlist = signid.Split(';').ToList();

                            bool isequalle = false;
                            for (int i = 0; i < signlist.Count; i++)
                            {
                                string sign = signlist[i];
                                if (string.IsNullOrEmpty(sign)) continue;
                                if (!isequalle)
                                {
                                    j++;
                                    isequalle = true;
                                }
                                Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetEmploye(sign);
                                if (employe1 == null)
                                    return "58 level sign Info.：" + sign + "Get failed from OA System！";
                                rolename = employe1.OrgLevel + "LEVEL Counter Sign";
                                EditInstruct fiftyei = ApproveEdit(DnNo, rolename, approvecode, "", rolename, approvetime, j, employe1, afdRole);
                            }
                        }

                    }
                    else if ("60".Equals(apcd))
                    {
                        List<TPV.Employe> sixtylist = Get60Level(fiftyManagerList);
                        for (int i = 0; i < sixtylist.Count; i++)
                        {
                            j++;
                            EditInstruct sixtylevelei = ApproveEdit(DnNo, role, approvecode, "", rolename, approvetime, j, sixtylist[i], afdRole);
                            mixList.Add(sixtylevelei);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(NextDnType)) NextDnType = role;
                        j++;
                        EditInstruct levelei = ApproveEdit(DnNo, role, approvecode, "", rolename, approvetime, j, null, afdRole);
                        mixList.Add(levelei);
                    }
                }
            }
            return string.Empty;
        }

        private static List<TPV.Employe> GetPManListByOrgid(string orgid)
        {
            List<TPV.Employe> employelist = Business.TPV.Context.OrgService.GetOrgPManListByID(orgid);
            if (employelist != null && employelist.Count > 0)
            {
                employelist.Sort(delegate (TPV.Employe a, TPV.Employe b)
                {
                    if (a.IsSapLink == true && b.IsSapLink == false) return -1;
                    if (b.IsSapLink == true && a.IsSapLink == false) return 1;
                    return 1;
                });
            }
            return employelist;
        }

        private static TPV.Employe Get58Level(TPV.Employe emp, ref List<TPV.Employe> employelist)
        {
            if (emp.OrgLevel >= 58) return emp;
            employelist = GetPManListByOrgid(emp.OrgID);
            foreach (TPV.Employe employe in employelist)
            {
                if (employe.OrgLevel == 58)
                    return employe;
            }
            return null;
        }

        private static List<TPV.Employe> Get60Level(List<TPV.Employe> employelist)
        {
            List<TPV.Employe> sixtyLevel = new List<TPV.Employe>();
            string orgid56 = string.Empty;
            foreach (TPV.Employe employe in employelist)
            {
                if (employe.OrgLevel == 60)
                    sixtyLevel.Add(employe);
                if (employe.OrgLevel == 58)
                {
                    orgid56 = employe.OrgID;
                }
            }
            if (sixtyLevel.Count <= 0 && !string.IsNullOrEmpty(orgid56))
            {
                List<TPV.Employe> employelist1 = GetPManListByOrgid(orgid56);
                foreach (TPV.Employe employe in employelist1)
                {
                    if (employe.OrgLevel > 58)
                        sixtyLevel.Add(employe);
                }
            }

            if (sixtyLevel.Count > 0)
            {
                sixtyLevel.Sort(delegate (TPV.Employe a, TPV.Employe b)
                {
                    if (a.IsSapLink == true) return -1;
                    if (b.IsSapLink == true) return 1;
                    return 1;
                });
            }
            return sixtyLevel;
           //return sixtyLevel;
        }




        private static string GetLevelApprove(string DnNo, string UserId, string Dep, MixedList mixList, string approvecode, string approvename, string approvetime, ref int j, ref string userid, ref bool isOkTime, string afdrole, int max, int level = 50, string rolename = "", string Role = "")
        {
            if (string.IsNullOrEmpty(Role))
            {
                Role = Dep + "MM";
            }

            if (string.IsNullOrEmpty(userid)) userid = UserId;
            Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmploye(userid);
            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
            if (employe != null)
            {
                while (employe.OrgLevel < level)
                {
                    employe = Business.TPV.Context.OrgService.GetSuperior(employe);
                    userid = employe.EmployeCode;
                }
                j++;
                EditInstruct levelei = ApproveEdit(DnNo, Role, approvecode, approvename,rolename, approvetime, j, employe, afdrole);
                isOkTime = true;
                mixList.Add(levelei);
            }
            return string.Empty;
        }

        private static string GetLevelApprove(string DnNo,UserInfo userinfo, MixedList mixList, string approvecode, string approvename, string approvetime, ref int j, ref string userid, ref bool isOkTime, string afdrole, int max, int level = 50, string rolename = "", string Role = "")
        {
            if (string.IsNullOrEmpty(Role))
            {
                Role = userinfo.Dep + "MM";
                rolename = GetApproveName(Role, approvename, userinfo);
            }

            if (string.IsNullOrEmpty(userid)) userid = userinfo.UserId;
            Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmploye(userid);
            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
            if (employe != null)
            {
                while (employe.OrgLevel < level)
                {
                    employe = Business.TPV.Context.OrgService.GetSuperior(employe);
                    userid = employe.EmployeCode;
                }
                j++;
                EditInstruct levelei = ApproveEdit(DnNo, Role, approvecode, approvename, rolename, approvetime, j, employe, afdrole);
                isOkTime = true;
                mixList.Add(levelei);
            }
            return string.Empty;
        }
        public static EditInstruct GetApproveEi(string debitno, string Approveto, string UserId, string NextLevel,string cmp)
        {
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, cmp);
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", debitno);
            apei.PutKey("APPROVE_CODE", Approve_Type);
            //apei.PutKey("APPROVE_LEVEL", Approveto);
            if (!string.IsNullOrEmpty(NextLevel))
            {
                apei.PutKey("APPROVE_LEVEL", NextLevel);
            }
            apei.Put("CREATE_BY", UserId);
            apei.PutDate("CREATE_DATE", odt);
            apei.Put("APPROVE_BY", UserId);
            apei.PutDate("APPROVE_DATE", odt);
            apei.PutDate("APPROVE_DATE_L", ndt);
            apei.Put("STATUS", "1");
            apei.Put("REMARK", @Resources.Locale.L_Pass);

            string sql = string.Format("SELECT NOTICE_TO,STATUS,UIC_GUID FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL={2} ",
                SQLUtils.QuotedStr(debitno), SQLUtils.QuotedStr(Approve_Type), SQLUtils.QuotedStr(NextLevel));
            DataTable dt=OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                string uicguid = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                    if ("1".Equals(status)) continue;
                    string noticeto = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                    if (UserId == noticeto)
                    {
                        uicguid = Prolink.Math.GetValueAsString(dt.Rows[0]["UIC_GUID"]);
                    }
                }
                //if (!string.IsNullOrEmpty(uicguid))
                //{
                //    string serviceUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["UIC_ServiceUrl"];
                //    System.ServiceModel.EndpointAddress remoteAddress = new System.ServiceModel.EndpointAddress(serviceUrl);
                //    using (var client = new WebGui.UICMessageSrv.MessageSrvSoapClient("MessageSrvSoap", remoteAddress))
                //    {
                //        ResponseDto resultds = null;
                //        try
                //        {
                //            resultds = client.Recall(uicguid);
                //        }
                //        catch (Exception) { }
                //        if (resultds != null)
                //        {
                //            apei.Put("UIC_STATUS", "Recall " + resultds.StatusCode);
                //        }
                //    }
                //}

            }
            if (dt.Rows.Count > 1)
            {
                bool updatebynoticeto = true;
                string noticeto = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                    if ("1".Equals(status)) continue;
                    noticeto = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                    if(UserId== noticeto)
                    {
                        apei.PutKey("NOTICE_TO", UserId);
                        updatebynoticeto = false;
                    }
                }
                if (updatebynoticeto)
                {
                    apei.PutKey("NOTICE_TO", noticeto);
                }
            }
            return apei;
        }

        public static string GetUpActualTimeSql(string debitno, string Approveto)
        {
            string sql = "update APPROVE_RECORD SET ACTUAL_TIME=DateDiff(mi,NOTIFY_DATE ,APPROVE_DATE) WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(debitno), SQLUtils.QuotedStr(Approve_Type), SQLUtils.QuotedStr(Approveto));
            return sql;
        }

        public static EditInstruct UpdateBillFinish(string uid, UserInfo userinfo)
        {
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", "Finish");
            BillApproveNew.SetApprove(ei, userinfo);
            return ei;
        }

        public static void ChangeFinish(string uid, UserInfo userinfo)
        {
            string sql = string.Format(@"SELECT TOP 1 STATUS FROM APPROVE_RECORD WHERE REF_NO={0} ORDER BY APPROVE_LEVEL DESC", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow[] drs = dt.Select("STATUS='0'");
                if (drs.Length <= 0)
                {
                    MixedList mxlist = new MixedList();
                    mxlist.Add(UpdateBillFinish(uid, userinfo));
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mxlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 增加DN通知记录，发送Mail和增加Msg通知,Mailtype 为DN_AP
        /// </summary>
        /// <param name="uuid">查找通知的 uuid</param>
        /// <param name="baseCondition">基本查询条件 baseCondition</param>
        /// <param name="subject">Mail标题或是Msg的信息</param>
        /// <param name="userId">当前用户</param>
        /// <param name="GroupId">集团</param>
        /// <param name="CompanyId">公司</param>
        /// <param name="Dep">部门</param>
        /// <returns></returns>
        public static string AddToNoticeBill(string uuid, string baseCondition, string subject, string userId, string GroupId, string CompanyId, string Dep, string billUid, string noticeUser = null, string notice_mail = null)
        {
            string msg = string.Empty;
            if (!string.IsNullOrEmpty(notice_mail))
            {
                EvenFactory.AddEven(billUid + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), billUid, MailManager.InboundBILLAp, null, 1, 0, notice_mail, subject, "", 3, 0);
                return msg;
            }
            if (string.IsNullOrEmpty(uuid)) return msg;

            string sql = string.Format(@"SELECT * FROM APPROVE_ATTR_DP WHERE {0} AND U_FID={1}",
                  baseCondition, SQLUtils.QuotedStr(uuid));
            if (!string.IsNullOrEmpty(noticeUser))
            {
                sql += string.Format(" AND USER_ID={0}", SQLUtils.QuotedStr(noticeUser));
            }

            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            #region 处理Msg提醒的
            DataRow[] msgrows = maindt.Select("BY_MSG='Y'");
            MessageData md = null;
            string receiveUser = string.Empty;
            MixedList ml = new MixedList();
            foreach (DataRow dr in msgrows)
            {
                receiveUser = Prolink.Math.GetValueAsString(dr["USER_ID"]);
                //增加通知到Msg中
                md = new MessageData();
                md.Title = "Bill Message";
                md.Type = "1";
                md.JobNo = uuid;
                md.Content = subject;
                EditInstruct ei = AddMessage(md, userId, receiveUser, GroupId, CompanyId, Dep);
                ml.Add(ei);
            }

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    //Log记录
                    msg = ex.Message.ToString();
                }
            }
            return msg;
            #endregion
        }

        public static EditInstruct AddMessage(MessageData md, string UserId, string receiveUser, string GroupId, string CompanyId, string Dep)
        {
            EditInstruct ei = new EditInstruct("GFMESSAGE", EditInstruct.INSERT_OPERATION);
            ei.Put("GROUP_ID", GroupId);
            int iSeed = 6;
            Random ra = new Random(iSeed);
            ei.Put("MSG_ID", Guid.NewGuid().ToString());
            ei.Put("RCV_CD", receiveUser);
            ei.Put("STATUS", MessageData.HAS_RECEVIE);
            ei.Put("CREATE_BY", UserId);
            ei.Put("MSG_TYPE", md.Type);
            ei.Put("JOB_NO", md.JobNo);
            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            ei.PutClob("CONTENT", md.ToXml());
            return ei;
        }

        #region 新方法
        #region 發起簽核
        public static string ApproveBillItem(string uid, string DebitNo, UserInfo userinfo)
        {
            MixedList mixList = new MixedList();
            string returnMsg = string.Empty;
            string NextDnType = string.Empty;
            string ffid = "";
            string nowLevel = string.Empty;
            string status = string.Empty;

            #region 取得SMBIM資料
            DataTable nowdata = Business.BillApproveHelper.GetDataByUId(uid);

            string ApproveTo =  Prolink.Math.GetValueAsString(nowdata.Rows[0]["APPROVE_TO"]);
            string ApproveBack =  Prolink.Math.GetValueAsString(nowdata.Rows[0]["APPROVE_BACK"]);
            string groupid =  Prolink.Math.GetValueAsString(nowdata.Rows[0]["GROUP_ID"]);
            string cmp =  Prolink.Math.GetValueAsString(nowdata.Rows[0]["CMP"]);
            string shipmentid =  Prolink.Math.GetValueAsString(nowdata.Rows[0]["SHIPMENT_ID"]);
            string approveuser =  Prolink.Math.GetValueAsString(nowdata.Rows[0]["APPROVE_USER"]);
            #endregion

            string noticeto = string.Empty;
            string notice_mail = string.Empty;

            bool IsOK = CheckLastOK(uid, Approve_Type, ApproveTo, approveuser,userinfo.UserId);
            if (IsOK)
            {
                return returnMsg = @Resources.Locale.L_BillApproveHelper_Business_34 + DebitNo + @Resources.Locale.L_BillApproveHelper_Business_35;
            }

            IsOK = CheckApproveLevel(ApproveTo, userinfo.UserId, cmp, groupid, approveuser, userinfo.Upri, userinfo.Dep,uid);
            if (!IsOK)
            {
                return returnMsg = string.Format(@Resources.Locale.L_BillApproveHelper_Business_36, DebitNo);
            }

            //检查ApproveRecord是否有值
            string alsql = string.Format("SELECT APPROVE_LEVEL FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(uid),
                SQLUtils.QuotedStr(Approve_Type),
                SQLUtils.QuotedStr(ApproveTo));
            if (!string.IsNullOrEmpty(approveuser))
            {
                alsql += NoticeToSql(approveuser);
            }
            DataTable recorddt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql = string.Empty;
            string NextLevel = string.Empty;
            int saplevel = 0;
            bool noticemailonce = true;
            #region 產生第一次簽核資料

            if (ApproveTo == "A")     //第一笔资料
            {
                returnMsg = createApproveRecord(nowdata, ref NextDnType, ref noticeto, ref notice_mail, userinfo);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    return returnMsg;
                }
                if (string.IsNullOrEmpty(NextDnType)) NextDnType = "Finish";
                NextLevel = "2";
            }
            #endregion
            #region 已有簽核資料
            #region 查無簽核流程設定
            else if (recorddt.Rows.Count <= 0)
            {
                return returnMsg = string.Format(@Resources.Locale.L_BillApproveHelper_Business_36, DebitNo);
            }
            #endregion
            else if (recorddt.Rows.Count > 0 && ApproveTo != "A")
            {
                nowLevel = recorddt.Rows[0]["APPROVE_LEVEL"].ToString();

                sql = "SELECT COUNT(1) FROM APPROVE_RECORD WHERE CAST(APPROVE_LEVEL AS INT)={0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2}";
                sql=string.Format(sql, SQLUtils.QuotedStr(nowLevel), SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(Approve_Type));
                int nowrecordcount=OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (nowrecordcount > 1)
                {
                    sql = @"SELECT TOP 1 ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL,SAP_LEVEL FROM APPROVE_RECORD 
                            WHERE CAST(APPROVE_LEVEL AS INT)>={0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC";
                }
                else
                {
                    sql = @"SELECT TOP 1 ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL,SAP_LEVEL FROM APPROVE_RECORD 
                            WHERE CAST(APPROVE_LEVEL AS INT)>{0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC";
                }
                alsql = string.Format(sql, SQLUtils.QuotedStr(nowLevel), SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(Approve_Type));
                DataTable appdt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (appdt.Rows.Count > 0)
                {
                    NextDnType = Prolink.Math.GetValueAsString(appdt.Rows[0]["ROLE"]);
                    NextLevel = Prolink.Math.GetValueAsString(appdt.Rows[0]["APPROVE_LEVEL"]);
                    noticeto = Prolink.Math.GetValueAsString(appdt.Rows[0]["NOTICE_TO"]);
                    notice_mail = Prolink.Math.GetValueAsString(appdt.Rows[0]["NOTICE_MAIL"]);
                    saplevel = Prolink.Math.GetValueAsInt(appdt.Rows[0]["SAP_LEVEL"]);
                    if (NextLevel == nowLevel)
                        noticemailonce = false;
                }

                EditInstruct apei = GetApproveEi(uid, ApproveTo, userinfo.UserId, nowLevel,cmp);
                mixList.Add(apei);

                sql = GetUpActualTimeSql(uid, ApproveTo);
                mixList.Add(sql);
                if (string.IsNullOrEmpty(NextDnType))
                {
                    try
                    {
                        UICHandle uic = new UICHandle(uid, NextLevel, Approve_Type, userinfo);
                        UICReturnMSG msg = uic.SendUICMessage("UICClose");
                        if (msg.type.ToLower() == "fail" || (msg.type.ToLower() == "error" && msg.code != "400"))
                            return "UIC Close Fail:" + msg.message;
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        ChangeFinish(uid, userinfo);
                        returnMsg = "success";
                    }
                    catch (Exception ex)
                    {
                        returnMsg = DebitNo + ":" + @Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
                    }
                    return returnMsg;
                }
                if (noticemailonce)
                {
                    DateTime odt = DateTime.Now;
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, cmp);
                    EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                    newEi.PutKey("REF_NO", uid);
                    newEi.PutKey("APPROVE_CODE", Approve_Type);
                    newEi.PutKey("ROLE", NextDnType);
                    newEi.PutKey("APPROVE_LEVEL", NextLevel);
                    newEi.PutDate("NOTIFY_DATE", odt);
                    newEi.PutDate("NOTIFY_DATE_L", ndt);
                    mixList.Add(newEi);
                }
            }
            #endregion

            sql = "SELECT APPROVE_GROUP,U_ID FROM APPROVE_ATTR_D WHERE U_FID='7A975E0E-A1CB-4630-8604-89E47B8CA9E5' AND APPROVE_GROUP={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(NextDnType));
            DataTable nextdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (nextdt.Rows.Count <= 0 && !string.IsNullOrEmpty(NextDnType))
            {
                if (string.IsNullOrEmpty(notice_mail))
                {
                    return returnMsg = @Resources.Locale.L_BaseApprove_Business_20 + NextDnType + @Resources.Locale.L_BaseApprove_Business_21;
                }
            }
            else
            {
                ffid = Prolink.Math.GetValueAsString(nextdt.Rows[0]["U_ID"]);
            }

            //更新SMDN的状态
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_USER", noticeto);
            ei.Put("APPROVE_TO", NextDnType);
            BillApproveNew.SetApprove(ei, userinfo);
            mixList.Add(ei);


            IMailTemplateParse parse = new DefaultMailParse();
            try
            {

                UICHandle uic = new UICHandle(uid, NextLevel, Approve_Type, userinfo);
                UICReturnMSG msg = uic.SendUICMessage("UICClose");
                if (saplevel > 50 && noticemailonce)
                {

                    if (msg.type.ToLower() == "success" || msg.code == "400")
                        msg = uic.SendUICMessage("UICCreate");
                    if (msg.type.ToLower() == "fail")
                        return "Send UIC Fail:" + msg.message;
                }
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = DebitNo + ":"+@Resources.Locale.L_BillApproveHelper_Business_38;

                if (saplevel <= 50 && noticemailonce)
                {
                    string subject = @Resources.Locale.L_BillApproveHelper_Business_39 + DebitNo + @Resources.Locale.L_BillApproveHelper_Business_40;
                    returnMsg += AddToNoticeBill(ffid, userinfo.basecondtions, subject, userinfo.UserId, groupid, cmp, userinfo.Dep, uid, noticeto, notice_mail);

                    sql = string.Format(@"SELECT ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL,SAP_LEVEL FROM APPROVE_RECORD 
                            WHERE APPROVE_LEVEL={0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2} AND NOTICE_TO !={3}",
                        SQLUtils.QuotedStr(NextLevel), SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(Approve_Type), SQLUtils.QuotedStr(noticeto));
                    DataTable appdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow dr in appdt.Rows)
                    {
                        noticeto = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                        notice_mail = Prolink.Math.GetValueAsString(dr["NOTICE_MAIL"]);
                        //noticeto = "827737035@qq.com;820839045@qq.com;";
                        //notice_mail = "827737035@qq.com;820839045@qq.com;";
                        returnMsg += AddToNoticeBill(ffid, userinfo.basecondtions, subject, noticeto, groupid, cmp, userinfo.Dep, uid, noticeto, notice_mail);
                    }

                }
                //else
                //{
                //    //如果层级超过了50，则是高层签核，需要将对应的信心发送到TPV的UIC
                //    if (noticemailonce)
                //    {
                //        SendUICMessage(uid, noticeto, notice_mail, NextLevel, result, cmp, userinfo, DebitNo);
                //        sql = string.Format(@"SELECT ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL,SAP_LEVEL FROM APPROVE_RECORD 
                //            WHERE APPROVE_LEVEL={0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2} AND NOTICE_TO !={3}",
                //                SQLUtils.QuotedStr(NextLevel), SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(Approve_Type), SQLUtils.QuotedStr(noticeto));
                //        DataTable appdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //        foreach (DataRow dr in appdt.Rows)
                //        {

                //            noticeto = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                //            notice_mail = Prolink.Math.GetValueAsString(dr["NOTICE_MAIL"]);
                //            SendUICMessage(uid, noticeto, notice_mail, NextLevel, result, cmp, userinfo, DebitNo);
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                returnMsg = DebitNo + ":"+@Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
            }
            return returnMsg;

        }

        private static void SendUICMessage(string uid, string noticeto, string notice_mail, string NextLevel, int[] result, string cmp, UserInfo userinfo,string debitno)
        {
            try
            {
                string sql = string.Format(@"SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID = (SELECT APPROVE_BY FROM APPROVE_RECORD WHERE REF_NO ={0}
                    AND APPROVE_CODE ={1} AND APPROVE_LEVEL = '1') AND CMP = {2}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(Approve_Type), SQLUtils.QuotedStr(cmp));
                string Applicant = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format(@"SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID = {0} AND CMP = {1}", SQLUtils.QuotedStr(userinfo.UserId), SQLUtils.QuotedStr(cmp));
                string Sender = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (string.IsNullOrEmpty(Sender))
                {
                    Sender = userinfo.UserId + "@TPV-TECH.COM";
                }
                string uicguid = Guid.NewGuid().ToString();
                WebGui.UICMessageSrv.TaskMessageRequestDto msg = new WebGui.UICMessageSrv.TaskMessageRequestDto();
                msg.Applicant = Applicant;
                msg.MessageId = uicguid;
                msg.Sender = Sender;
                msg.ContentType = "URL";
                msg.ReceiverList = notice_mail;
                msg.Content = System.Web.Configuration.WebConfigurationManager.AppSettings["UIC_BillMessageUrl"];
                msg.URL = System.Web.Configuration.WebConfigurationManager.AppSettings["UIC_BillMessageUrl"];
                msg.SendingAppKey = System.Web.Configuration.WebConfigurationManager.AppSettings["UIC_AppKey"];
                msg.SendingFunctionKey = System.Web.Configuration.WebConfigurationManager.AppSettings["UIC_FunctionKey"];
                string serviceUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["UIC_ServiceUrl"];

                msg.Subject = "EShipping 物流费用签核(Logistic Billing Approve) " + debitno;
                System.ServiceModel.EndpointAddress remoteAddress = new System.ServiceModel.EndpointAddress(serviceUrl);
                using (var client = new WebGui.UICMessageSrv.MessageSrvSoapClient("MessageSrvSoap", remoteAddress))
                {
                    ResponseDto resultds = null;
                    try
                    {
                        resultds = client.Create(msg);
                    }
                    catch (Exception) { }

                    if (resultds != null)
                    {
                        EditInstruct arei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                        arei.PutKey("REF_NO", uid);
                        arei.PutKey("APPROVE_CODE", Approve_Type);
                        arei.PutKey("NOTICE_TO", noticeto);
                        arei.PutKey("APPROVE_LEVEL", NextLevel);
                        arei.Put("UIC_GUID", uicguid);
                        arei.Put("UIC_STATUS", "SEND" + resultds.StatusCode);
                        result = OperationUtils.ExecuteUpdate(arei, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
            }
            catch (Exception ex) { }
        }
        #endregion

        #region 創建approve record
        public static string createApproveRecord(DataTable nowdata, ref string NextDnType, ref string noticeto, ref string notice_mail, UserInfo userinfo)
        {
            string uid = Prolink.Math.GetValueAsString(nowdata.Rows[0]["U_ID"]);
            string returnMsg = string.Empty;
            string dep = Prolink.Math.GetValueAsString(userinfo.Dep).ToUpper();
            if ("GLST" != dep && "LST" != dep)
            {
                return returnMsg = @Resources.Locale.L_BillApproveHelper_Business_41;
            }
            MixedList mixList = new MixedList();
            string sql = "DELETE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(Approve_Type));
            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            returnMsg = createNewApproveRecord(nowdata, userinfo, ref noticeto, ref notice_mail, mixList,ref NextDnType);
            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }


        public static string createNewApproveRecord(DataTable nowdata, UserInfo user, ref string noticeto, ref string notice_mail, MixedList ml, ref string NextDnType)
        {
            string uid = Prolink.Math.GetValueAsString(nowdata.Rows[0]["U_ID"]);
            string sql = "SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE={0} AND CMP_ID={1} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC";
            sql = string.Format(sql, SQLUtils.QuotedStr(Approve_Type), SQLUtils.QuotedStr(user.CompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Decimal amt = Prolink.Math.GetValueAsDecimal(nowdata.Rows[0]["AMT"]);
            string cur = Prolink.Math.GetValueAsString(nowdata.Rows[0]["CUR"]);
            decimal usdAmt = 0;
            if (!"USD".Equals(cur))
            {
                string rateFilter = string.Format("EDATE<={0}", SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd")));
                DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND {0} ORDER BY EDATE",
                    rateFilter), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                bool error = false;
                List<string> msg = new List<string>();
                Business.TPV.Financial.Helper.GetTotal(rateDt, msg, amt, cur, ref usdAmt, ref error, "USD");
                if (error) return msg.Last();
            }
            else
            { usdAmt = amt; }

            bool exist50 = false;
            int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(uid)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;

            string returnMsg = string.Empty, approvecode = string.Empty, approvename = string.Empty, approvetime = string.Empty, role = string.Empty;
            string userId = user.UserId, email = string.Empty, rolename = string.Empty;
            int j = 0,place=0;
            bool isokTime = true,end50=false;
            sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(userId));
            string noticemail = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmpBaseByMail(noticemail);
            if (employe == null)
            {
                employe = Business.TPV.Context.OrgService.GetEmploye(userId);
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
                role = Prolink.Math.GetValueAsString(dt.Rows[i]["ROLE"]);
                approvecode = Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_CODE"]);
                approvename = Prolink.Math.GetValueAsString(dt.Rows[i]["GROUP_DESCP"]);
                approvetime = Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_TIME"]);
                apei.Put("AFD_ROLE", "A");
                apei.Put("STATUS", "0");
                apei.Put("REF_NO", uid);
                apei.Put("VOID_LOOP", max);
                apei.Put("APPROVE_CODE", approvecode);
                apei.Put("APPROVE_NAME", approvename);
                rolename = GetApproveName(role, approvename, user);
                switch (role.ToUpper())
                { 
                    case "A":
                        if (i == 0)
                        {
                            apei.Put("STATUS", "1");
                            apei.Put("APPROVE_BY", userId);
                            apei.PutDate("APPROVE_DATE", DateTime.Now);
                            apei.Put("REMARK", @Resources.Locale.L_ContractQuery_Views_478);
                            role = user.Dep;
                            apei.Put("ROLE", role);
                            rolename=GetApproveName(role, approvename, user);
                        }
                        else
                            continue;
                        break;
                    case "AM":
                        if (i == 1)
                        {
                            employe = Business.TPV.Context.OrgService.GetSuperior(user.UserId, user.CompanyId);
                            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                            userId = employe.EmployeCode;
                            email = employe.EMail;
                            noticeto = userId;
                            notice_mail = email;
                            isokTime = false;
                            rolename = GetApproveName(role, approvename, user);
                            apei.PutDate("NOTIFY_DATE", DateTime.Now);
                            role = user.Dep + "M";
                            apei.Put("NOTICE_TO", userId);
                            apei.Put("NOTICE_MAIL", email);
                            NextDnType = role;
                            place++;
                        }
                        else
                            continue;
                        break;
                    case "50LEVEL":
                        if (!end50)
                        {
                            exist50 = true;
                        }
                        break;
                }
                apei.Put("ROLE", role);
                apei.Put("ROLE_NM", rolename);
                if (isokTime)
                    apei.PutDate("NOTIFY_DATE", DateTime.Now);
                if (!"50LEVEL".Equals(role.ToUpper()))
                {
                    j++;
                    apei.Put("APPROVE_LEVEL", j);
                    ml.Add(apei);
                }
                if (i >= 1 && exist50)
                {
                    returnMsg = NewFiftyApprove(employe,uid, user, ml, approvecode, approvetime, ref j, ref userId,ref isokTime, ref NextDnType, place, usdAmt);
                    exist50 = false;
                    end50 = true;
                }
            }
            return returnMsg;
        }
        #endregion
        #region
        public static string GetApproveName(string role, string approvename, UserInfo userinfo)
        {
            string sqlDep = string.Format("SELECT TOP 1 GROUP_DESCP FROM APPROVE_ATTR_D WHERE GROUP_ID={0} AND CMP={1} AND APPROVE_GROUP={2} AND U_FID IN(SELECT U_ID FROM APPROVE_ATTRIBUTE C WHERE C.CMP={1} AND APPROVE_ATTR='BILLING')", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.CompanyId), SQLUtils.QuotedStr(role));
            string _approvename =OperationUtils.GetValueAsString(sqlDep, Prolink.Web.WebContext.GetInstance().GetConnection());
            return _approvename == "" ? approvename : _approvename;
        }
        #endregion

        #region 取得下一個通知人
        public static DataTable getNextApproverUser(string ApproveGroup, string GroupId, string Cmp)
        {
            string sql = @"SELECT B.USER_ID, B.U_EMAIL FROM APPROVE_ATTR_D A, APPROVE_ATTR_DP B 
                                WHERE A.U_ID=B.U_FID AND A.APPROVE_GROUP={0} AND A.GROUP_ID={1} AND A.CMP={2} ";
            sql = string.Format(sql, SQLUtils.QuotedStr(ApproveGroup), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Cmp));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            return dt;
        }
        #endregion
        #endregion

        #region 账单拒绝时发送Mail
        public static void SendBillRJMail(string uid, string groupid, string companyid)
        {
            string sql = "SELECT LSP_NO,DEBIT_NO FROM SMBIM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return;
            string LspNo = dt.Rows[0]["LSP_NO"].ToString();
            string DebitNo = dt.Rows[0]["DEBIT_NO"].ToString();

            DataTable mailGroupDt = MailTemplate.GetMailGroup(LspNo, groupid, "G");
            if (mailGroupDt.Rows.Count > 0)
            {
                foreach (DataRow item1 in mailGroupDt.Rows)
                {
                    string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                    if (mailStr != "")
                    {
                        EvenFactory.AddEven(uid + "#" + groupid + "#" + companyid + "#" + Guid.NewGuid().ToString() + "#" + MailManager.InboundBILLRejectNotify, uid, MailManager.InboundBILLRejectNotify, null, 1, 0, mailStr, @Resources.Locale.L_ActManageController_Controllers_3 + DebitNo, "");
                    }
                }
            }
        }
        #endregion
    }
}