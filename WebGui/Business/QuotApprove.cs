using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text.RegularExpressions;
using Prolink.Data;
using TrackingEDI.Business;
using WebGui.Models;
using TrackingEDI.Mail;
using WebGui.Controllers;

namespace Business
{
    public class QuotApprove
    {
        public static readonly string Level = "M";
        public static string previousdep = string.Empty;
        public static string _dep = "";
        public static string ApproveType = string.Empty;
        public static string modetype = string.Empty;
        private static string user = "";
        private static string GetApprove(string DnNo, UserInfo userinfo, string userid)
        {
            if (string.IsNullOrEmpty(userid)) userid = userinfo.UserId;
            Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmploye(userid);
            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
            if (employe != null)
            {
                while (employe.OrgLevel < 50)
                {
                    employe = Business.TPV.Context.OrgService.GetSuperior(employe);
                    userid = employe.EmployeCode;
                }
            }
            return "";
        }
        public static void CreateQuotApprove(MixedList ml, DataTable dt, UserInfo userinfo, string uid, string CompanyId)
        {
            EditInstruct ei = null;
            string UserId=userinfo.UserId;
            DataRow nextdr=null;
            _dep = ""; previousdep = "";
            //作废记录
            int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(uid)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;
                
            for (int i = 0; i < dt.Rows.Count; i++)
            { 
                DataRow dr=dt.Rows[i];
                string level=Prolink.Math.GetValueAsString( dr["APPROVE_LEVEL"]);
                ei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
                ei.Put("REF_NO",uid);
                ei.Put("APPROVE_CODE",dr["APPROVE_CODE"]);
                ei.Put("APPROVE_LEVEL", level);
                ei.Put("APPROVE_NAME", dr["APPROVE_NAME"]);
                ei.Put("APPROVE_TIME", dr["APPROVE_TIME"]);
                ei.Put("AFD_ROLE", dr["APPROVE_CODE"]);
                ei.Put("VOID_LOOP", max);
                //ei.Put("NOTICE_MAIL", dr["APPROVE_CODE"]);
                //ei.Put("NOTICE_TO", dr["APPROVE_CODE"]);
                nextdr=dt.Rows.Count>i+1?dt.Rows[i+1]:null;
                SupplyApprove(ei, userinfo, i, dr, nextdr, CompanyId);
                ml.Add(ei);
            }
        }
        public static void SupplyApprove(EditInstruct ei, UserInfo userinfo, int level, DataRow dr, DataRow nextdr, string CompanyId)
        {
            string noticeto = "";
            string noticemail = "";
            string _role = "";
            string dep = "";
            bool isoa = false;
            if (nextdr != null)
            {
                _dep = level == 0 ? Prolink.Math.GetValueAsString(dr["ROLE"]) : _dep;
                dep = Prolink.Math.GetValueAsString(nextdr["ROLE"]);
                _role = dep.IndexOf(_dep) == 0 ? dep.Replace(_dep, "") : _role = ""; 
                Regex re = new Regex(Level);
                if (_role.Length == re.Matches(_role).Count && _role.Length > 0)
                {
                    isoa = true;
                }
                if (!isoa)
                {
                    dep = Prolink.Math.GetValueAsString(dr["ROLE"]);
                    _role = dep.IndexOf(previousdep) == 0 ? dep.Replace(previousdep, "") : _role = "";
                    if (_role.Length == re.Matches(_role).Count && _role.Length > 0)
                    {
                        dep = _dep;
                    }
                }
            }
            else
            {
                dep = Prolink.Math.GetValueAsString(dr["ROLE"]);
                _role=dep.IndexOf(previousdep) == 0?dep.Replace(previousdep, ""):_role = "";
                Regex re = new Regex(Level);
                if (_role.Length == re.Matches(_role).Count && _role.Length > 0)
                {
                    dep = _dep;
                }
            }
            if (level == 0)
            {
                _dep = userinfo.Dep;
                ei.Put("ROLE", _dep);
                ei.Put("ROLE_NM", dr["GROUP_DESCP"]);
                ei.Put("APPROVE_BY", userinfo.UserId);
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("APPROVE_DATE", odt);
                ei.PutDate("NOTIFY_DATE", odt);
                ei.PutDate("APPROVE_DATE_L", ndt);
                ei.PutDate("NOTIFY_DATE_L", ndt);
                ei.Put("CREATE_BY", userinfo.UserId);
                ei.PutDate("CREATE_DATE", odt);
                ei.PutDate("CREATE_DATE_L", ndt);
                ei.Put("STATUS", "1");
                ei.Put("REMARK", @Resources.Locale.L_ContractQuery_Views_478);
                ENoticInfo(dr, ref noticeto, ref noticemail);
                NoticInfo(ei, userinfo, noticeto, noticemail, isoa);
            }
            else
            {
                _dep = dep + _role;
                ei.Put("STATUS", "0");
                ei.Put("ROLE", _dep);
                ei.Put("ROLE_NM", dr["GROUP_DESCP"]);
            }
            previousdep = Prolink.Math.GetValueAsString(dr["ROLE"]);
        }
        public static void Approve(MixedList ml, DataTable dt, UserInfo userinfo,int level)
        {
            DataRow dr = dt.Rows[level];
            string uid = Prolink.Math.GetValueAsString(dr["REF_NO"]);
            string ApproveType = Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
            string nowLevel = Prolink.Math.GetValueAsString(dr["APPROVE_LEVEL"]);
            EditInstruct apei = GetApproveEi(uid, ApproveType, userinfo.UserId, nowLevel, userinfo.CompanyId);
        }
        public static void CheckApprove(UserInfo userinfo,string uid)
        {
           
        }
        public static string GetApproveBack(string UserId, string CompanyId, string GroupId, string upri, string Dep,string type)
        {
            string sql = string.Format(@"SELECT DISTINCT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR='QUOT'",//AND DP.STN={2}
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
        public static bool CheckRole(string role, string nextrole)
        {
            string _role = role.IndexOf(nextrole) == 0 ? nextrole.Replace(role, "") : _role = "";
            Regex re = new Regex(Level);
            if (_role.Length > 0 && _role.Length == re.Matches(_role).Count) return true;
            return false;
        }
        public static EditInstruct GetApproveEi(string refno, string approvetype, string UserId, string NextLevel, string CompanyId)
        {
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", refno);
            apei.PutKey("APPROVE_CODE", approvetype);
            //apei.PutKey("APPROVE_LEVEL", Approveto);
            if (!string.IsNullOrEmpty(NextLevel))
            {
                apei.PutKey("APPROVE_LEVEL", NextLevel);
            }
            apei.Put("CREATE_BY", UserId);
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            apei.PutDate("CREATE_DATE", odt);
            apei.PutDate("CREATE_DATE_L", ndt);
            apei.Put("APPROVE_BY", UserId);
            apei.PutDate("APPROVE_DATE", odt);
            apei.PutDate("APPROVE_DATE_L", ndt);
            apei.Put("STATUS", "1");
            apei.Put("REMARK", @Resources.Locale.L_Pass);
            return apei;
        }
        public static string AddToNoticeQuot(string uuid, string baseCondition, string subject, string userId, string GroupId, string CompanyId, string Dep, string Uid, string noticeUser = null, string notice_mail = null)
        {
            string msg = string.Empty;
            if (!string.IsNullOrEmpty(noticeUser))
            {
                string type = modetype.Equals("X") ? MailManager.QuotNotify_X : modetype.Equals("B") ? MailManager.QuotNotify_B : modetype.Equals("C") ? MailManager.QuotNotify_C : string.Empty;
                string str = Uid + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString() + "#" + type + "#" + noticeUser+"#";
                EvenFactory.AddEven(str, Uid, type, null, 1, 0, notice_mail, subject, "", 3, 0);
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
                md.Title = "Quot Message";
                md.Type = "1";
                md.JobNo = uuid;
                md.Content = subject;
                EditInstruct ei = BillApproveHelper.AddMessage(md, userId, receiveUser, GroupId, CompanyId, Dep);
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
        public static string AddToBackQuot(string uuid, string baseCondition, string subject, string userId, string GroupId, string CompanyId, string Dep, string Uid, string noticeUser = null, string notice_mail = null,string remark="")
        {
            string msg = string.Empty;
            if (!string.IsNullOrEmpty(noticeUser))
            {
                //string type = modetype.Equals("X") ? MailManager.QuotRefuse_X : modetype.Equals("B") ? MailManager.QuotRefuse_B : modetype.Equals("C") ? MailManager.QuotRefuse_C : modetype.Equals("O") ? MailManager.QuotRefuse_O : string.Empty;
                string type = modetype.Equals("X") ? MailManager.QuotNotify_X : modetype.Equals("B") ? MailManager.QuotNotify_B : modetype.Equals("C") ? MailManager.QuotNotify_C : string.Empty;
                string str = Uid + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString() + "#" + type + "#" + noticeUser + "#" + remark;
                EvenFactory.AddEven(str, Uid, type, null, 1, 0, notice_mail, subject, "", 3, 0);
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
                md.Title = "Quot Message";
                md.Type = "1";
                md.JobNo = uuid;
                md.Content = subject;
                EditInstruct ei = BillApproveHelper.AddMessage(md, userId, receiveUser, GroupId, CompanyId, Dep);
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
        public static string GetUpActualTimeSql(string debitno, string approvetype, string Approveto)
        {
            string sql = "update APPROVE_RECORD SET ACTUAL_TIME=DateDiff(mi,NOTIFY_DATE ,APPROVE_DATE) WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(debitno), SQLUtils.QuotedStr(approvetype), SQLUtils.QuotedStr(Approveto));
            return sql;
        }
        public static void NoticInfo(EditInstruct ei, UserInfo userinfo, string noticeto, string noticemail, bool isoa = false)
        {
            if (isoa)
            {
                Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(userinfo.UserId);
                noticeto = employe1.EmployeCode;
                noticemail = employe1.EMail;
            }
            ei.Put("NOTICE_TO", noticeto);
            ei.Put("NOTICE_MAIL", noticemail);
        }
        public static void ENoticInfo(DataRow dr, ref string noticeto, ref string noticemail)
        {
            string AttrDId = "";
            string AttributeId = "";
            string guid= Prolink.Math.GetValueAsString(dr["GU_ID"]);
            AttrDId = guid;
            string Dsql = string.Format("select U_FID from APPROVE_ATTR_D where U_ID='{0}'", guid);
            AttributeId = OperationUtils.GetValueAsString(Dsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql = string.Format("select * from  APPROVE_ATTR_DP WHERE  U_FID='{0}' and U_FFID='{1}'", AttrDId, AttributeId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt != null && dt.Rows.Count > 0)
            {
                noticeto = Prolink.Math.GetValueAsString(dt.Rows[0]["USER_ID"]);
                noticemail = Prolink.Math.GetValueAsString(dt.Rows[0]["U_EMAIL"]);
            }
        }

        #region 發起簽核
        public static string ApproveQuotItem(string uid, UserInfo userinfo)
        {
            MixedList mixList = new MixedList();
            string returnMsg = string.Empty;
            string NextDnType = ""; 
            modetype = string.Empty;
            string ffid = "";
            string nowLevel = string.Empty;
            string status = string.Empty;
            ApproveType = string.Empty;
            string ApproveTo = string.Empty;
            string ApproveBack = string.Empty;
            user = userinfo.UserId;
            #region 取得SMQTM資料
            DataTable nowdata = GetDataByUId(uid);
            string QuotType = Prolink.Math.GetValueAsString(nowdata.Rows[0]["QUOT_TYPE"]);
            returnMsg = CheckApproveStaus(QuotType, 1);
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return returnMsg;
            }
            string tranmode = Prolink.Math.GetValueAsString(nowdata.Rows[0]["TRAN_MODE"]);
            modetype = tranmode;
            string quotno = Prolink.Math.GetValueAsString(nowdata.Rows[0]["QUOT_NO"]); 
            //string ApproveTo = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_TO"]);
            ApproveType = nowdata.Rows[0]["APPROVE_TYPE"].ToString();
            if (string.IsNullOrEmpty(ApproveType))
            {
                ApproveType = "QUOT_" + tranmode;
                string approvesql = string.Format("SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE='{0}' ORDER BY APPROVE_LEVEL", ApproveType);
                DataTable approvedt = OperationUtils.GetDataTable(approvesql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (!(approvedt != null && approvedt.Rows.Count > 0))
                {
                    ApproveType = "QUOT_CON";
                    approvesql = string.Format("select * from APPROVE_FLOW_D where APPROVE_CODE='{0}'  ORDER BY APPROVE_LEVEL", ApproveType);
                    approvedt = OperationUtils.GetDataTable(approvesql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                if (!(approvedt != null && approvedt.Rows.Count > 0))
                    return @Resources.Locale.L_QTManage_Controllers_tip1;
            }
            ApproveTo = nowdata.Rows[0]["APPROVE_TO"].ToString();
            string groupid = nowdata.Rows[0]["GROUP_ID"].ToString();
            string cmp = nowdata.Rows[0]["CMP"].ToString();
            string approveuser = nowdata.Rows[0]["APPROVE_USER"].ToString();

            #endregion

            string noticeto = string.Empty;
            string notice_mail = string.Empty;

            bool IsOK = CheckLastOK(uid, ApproveType, ApproveTo, approveuser);
            if (IsOK)
            {
                return returnMsg = @Resources.Locale.L_ActManage_Quottip1 + quotno + @Resources.Locale.L_BillApproveHelper_Business_35;
            }

            IsOK = CheckApproveLevel(ApproveTo, userinfo.UserId, cmp, groupid, approveuser, userinfo.Upri, userinfo.Dep, ApproveType);
            if (!IsOK)
            {
                return returnMsg = string.Format(@Resources.Locale.L_ActManage_Quottip2, quotno);
            }

            //检查ApproveRecord是否有值
            string alsql = string.Format("SELECT APPROVE_LEVEL FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(uid),
                SQLUtils.QuotedStr(ApproveType),
                SQLUtils.QuotedStr(ApproveTo));
            if (!string.IsNullOrEmpty(approveuser) && !CheckAttrDp(ApproveType,userinfo))
            {
                alsql += NoticeToSql(approveuser);
            }
            DataTable recorddt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql = string.Empty;
            string NextLevel = string.Empty;
            #region 產生第一次簽核資料
            if (ApproveTo == "A" )//|| ApproveTo == "LST")     //第一笔资料
            {
                //returnMsg = ApporveTo_A(uid, UserId, Dep, mixList, ref returnMsg, ref NextDnType, ApproveType, ref sql, ref noticeto, ref notice_mail);
                returnMsg = createApproveRecord(uid, ref NextDnType, ApproveType, ref noticeto, ref notice_mail, userinfo);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    return returnMsg;
                }
                NextLevel = "2";
            }
            #endregion
            #region 已有簽核資料
            else if (recorddt.Rows.Count > 0 && ApproveTo != "A")
            {

                nowLevel = recorddt.Rows[0]["APPROVE_LEVEL"].ToString();
                sql = @"SELECT TOP 1 ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL FROM APPROVE_RECORD 
                            WHERE CAST(APPROVE_LEVEL AS INT)>{0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC";
                alsql = string.Format(sql, SQLUtils.QuotedStr(nowLevel), SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(ApproveType));
                DataTable appdt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (appdt.Rows.Count <= 0)
                {
                    NextDnType = string.Empty;
                }
                else
                {
                    NextDnType = Prolink.Math.GetValueAsString(appdt.Rows[0]["ROLE"]);
                    NextLevel = Prolink.Math.GetValueAsString(appdt.Rows[0]["APPROVE_LEVEL"]);
                    noticeto = Prolink.Math.GetValueAsString(appdt.Rows[0]["NOTICE_TO"]);
                    notice_mail = Prolink.Math.GetValueAsString(appdt.Rows[0]["NOTICE_MAIL"]);
                    //不同部门通知签核
                    if (string.IsNullOrEmpty(noticeto) && !string.IsNullOrEmpty(NextDnType))
                    {
                        if (CheckRole(ApproveTo, NextDnType))
                        {
                            //是主管
                            Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(userinfo.UserId);
                            if (employe1 == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                            noticeto = employe1.EmployeCode;
                            notice_mail = employe1.EMail;
                        }
                        else
                        {
                            string sql1 = string.Format("SELECT * FROM APPROVE_ATTR_DP where U_FID IN (select U_ID from APPROVE_ATTR_D WHERE APPROVE_ATTR='QUOT' AND APPROVE_GROUP={0} AND CMP={1} AND GROUP_ID={2})", SQLUtils.QuotedStr(NextDnType), SQLUtils.QuotedStr(userinfo.CompanyId), SQLUtils.QuotedStr(userinfo.GroupId));
                            DataTable appdt1 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            noticeto = Prolink.Math.GetValueAsString(appdt1.Rows[0]["USER_ID"]);
                            notice_mail = Prolink.Math.GetValueAsString(appdt1.Rows[0]["U_EMAIL"]);
                        }
                    }
                }
                EditInstruct apei = GetApproveEi1(uid, ApproveType, ApproveTo, userinfo.UserId, nowLevel, userinfo.CompanyId);
                mixList.Add(apei);

                sql = GetUpActualTimeSql(uid, ApproveType, ApproveTo);
                mixList.Add(sql);
                if (string.IsNullOrEmpty(NextDnType))
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        ChangeFinish(uid, userinfo.CompanyId);
                        returnMsg = "success";
                    }
                    catch (Exception ex)
                    {
                        returnMsg = quotno + ":" + @Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
                    }
                    return returnMsg;
                }

                EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                newEi.PutKey("REF_NO", uid);
                newEi.PutKey("APPROVE_CODE", ApproveType);
                newEi.PutKey("ROLE", NextDnType);
                newEi.PutKey("APPROVE_LEVEL", NextLevel);
                DateTime odt = DateTime.Now;              
                string CompanyId = userinfo.CompanyId;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                newEi.PutDate("NOTIFY_DATE", odt);
                newEi.PutDate("NOTIFY_DATE_L", ndt);
                newEi.Put("NOTICE_TO", noticeto);
                newEi.Put("NOTICE_MAIL", notice_mail);
                mixList.Add(newEi);
            }
            #endregion
            #region 查無簽核流程設定
            else if (recorddt.Rows.Count <= 0)
            {
                return returnMsg = string.Format(@Resources.Locale.L_ActManage_Quottip2, quotno);
            }
            #endregion

            sql = "SELECT APPROVE_GROUP,U_ID FROM APPROVE_ATTR_D WHERE U_FID IN(SELECT U_ID FROM APPROVE_ATTRIBUTE C WHERE C.CMP={1} AND APPROVE_ATTR='QUOT') AND APPROVE_GROUP={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(NextDnType), SQLUtils.QuotedStr(cmp));
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

            //更新SMQTM的状态
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_USER", noticeto);
            ei.Put("APPROVE_TO", NextDnType);
            ei.Put("APPROVE_TYPE", ApproveType);
            ei.Put("APPROVE_BY", userinfo.UserId);
            DateTime odt1 = DateTime.Now;
            DateTime ndt1 = TimeZoneHelper.GetTimeZoneDate(odt1, cmp);
            ei.PutDate("APPROVE_DATE", odt1);
            ei.PutDate("APPROVE_DATE_L", ndt1);
            mixList.Add(ei);


            IMailTemplateParse parse = new DefaultMailParse();
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = quotno + ":" + @Resources.Locale.L_BillApproveHelper_Business_38;

                string subject = @Resources.Locale.L_QTQuery_QuotNo+":" + quotno + @Resources.Locale.L_BillApproveHelper_Business_40;

                returnMsg += AddToNoticeQuot(ffid, userinfo.basecondtions, subject, userinfo.UserId, groupid, cmp, userinfo.Dep, uid, noticeto, notice_mail);
            }
            catch (Exception ex)
            {
                returnMsg = quotno + ":" + @Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
            }
            return returnMsg;
        }
        #endregion
        public static bool CheckAttrDp(string role, UserInfo user)
        {
            string sql = string.Format(@"SELECT count(*) FROM APPROVE_ATTR_DP where U_FID IN (select U_ID from APPROVE_ATTR_D WHERE APPROVE_ATTR='QUOT'
 AND APPROVE_GROUP={0} AND CMP={1} AND GROUP_ID={2}) 
AND USER_ID={3}", SQLUtils.QuotedStr(role), SQLUtils.QuotedStr(user.CompanyId), SQLUtils.QuotedStr(user.GroupId), SQLUtils.QuotedStr(user.UserId));
            int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (count > 0)
                return true;
            else
                return false;
        }
        public static bool CheckApproveLevel(string apprvesto, string UserId, string CompanyId, string GroupId, string approveuser, string upri, string dep, string approvetype)
        {
            bool check = false;
            if (UserId == approveuser) return true;
            if ("MANA10".Equals(UserId)) return true;
            if ("ADMIN".Equals(UserId)) return true;
            string approvelists = GetApproveBack(UserId, CompanyId, GroupId, upri, dep,approvetype);
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
        public static bool CheckLastOK(string uid, string dntype, string role, string noticeto)
        {
            string sql = string.Format("SELECT STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(dntype), SQLUtils.QuotedStr(role));
            if (!string.IsNullOrEmpty(noticeto))
            {
                sql += NoticeToSql(noticeto);
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
        public static DataTable GetDataByUId(string uid)
        {
            string sql = string.Format("SELECT * FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }
        #region 創建approve record
        public static string createApproveRecord(string uid, ref string NextDnType, string ApproveType, ref string noticeto, ref string notice_mail, UserInfo userinfo)
        {
            string returnMsg = string.Empty;
            string dep = Prolink.Math.GetValueAsString(userinfo.Dep).ToUpper();
            if ("GLST" != dep && "LST" != dep)
            {
                return returnMsg = @Resources.Locale.L_BillApproveHelper_Business_41;
            }
            MixedList mixList = new MixedList();
            string sql = "SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE={0} AND CMP_ID={1} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC";
            sql = string.Format(sql, SQLUtils.QuotedStr(ApproveType), SQLUtils.QuotedStr(userinfo.CompanyId));
            DataTable dtapprove = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "DELETE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(ApproveType));

            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }

            if (dtapprove.Rows.Count <= 0)
            {
                return returnMsg = @Resources.Locale.L_BillApproveHelper_Business_42;
            }
            else
            {
                string approvecode = string.Empty;
                string approvename = string.Empty;
                string approvetime = string.Empty;
                int j = 0;
                int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(uid)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;
                for (int i = 0; i < dtapprove.Rows.Count; i++)
                {
                    EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
                    string role = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["ROLE"]);
                    approvecode = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["APPROVE_CODE"]);
                    approvename = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["GROUP_DESCP"]);
                    approvetime = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["APPROVE_TIME"]);
                    apei.Put("AFD_ROLE", Prolink.Math.GetValueAsString(dtapprove.Rows[i]["ROLE"]));
                    apei.Put("STATUS", "0");
                    apei.Put("REF_NO", uid);
                    apei.Put("VOID_LOOP", max);
                    apei.Put("APPROVE_CODE", approvecode);
                    apei.Put("APPROVE_NAME", approvename);

                    if (i == 0)
                    {
                        apei.Put("STATUS", "1");
                        apei.Put("APPROVE_BY", userinfo.UserId);
                        DateTime odt = DateTime.Now;                       
                        string CompanyId = userinfo.CompanyId;
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        apei.PutDate("APPROVE_DATE", odt);
                        apei.PutDate("NOTIFY_DATE", odt);
                        apei.PutDate("APPROVE_DATE_L", ndt);
                        apei.PutDate("NOTIFY_DATE_L", ndt);
                        apei.Put("CREATE_BY", userinfo.UserId);
                        apei.PutDate("CREATE_DATE", odt);
                        apei.PutDate("CREATE_DATE_L", ndt);
                        apei.Put("REMARK", @Resources.Locale.L_ContractQuery_Views_478);
                        role = userinfo.Dep;
                        apei.Put("ROLE_NM", GetApproveName(role, approvename, userinfo));
                    }
                    else if (i == 1)
                    {
                        DateTime odt = DateTime.Now;                      
                        string CompanyId = userinfo.CompanyId;
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        apei.PutDate("NOTIFY_DATE", odt);
                        apei.PutDate("NOTIFY_DATE_L", ndt);
                        apei.Put("STATUS", "0");
                        string role_0 = Prolink.Math.GetValueAsString(dtapprove.Rows[i-1]["ROLE"]);
                        string _role = role.Replace(role_0, "");
                        Regex m=new Regex("M");
                        int mcount=m.Matches(_role).Count;
                        if (role == "AM" )//|| (_role.Length == mcount && _role.Length > 0))
                        {
                            role = userinfo.Dep + "M";
                            apei.Put("ROLE_NM", GetApproveName(role, approvename, userinfo));
                            NextDnType = role;
                            continue;
                        }
                    }
                    if (role.Equals("50LEVEL"))
                    {
                        apei.Put("ROLE_NM", GetApproveName(role, approvename, userinfo));
                        continue;
                    }
                    apei.Put("ROLE", role);
                    j++;
                    apei.Put("APPROVE_LEVEL", j);
                    mixList.Add(apei);
                    bool isOkTime = true;
                    if (dtapprove.Rows.Count > (i + 1))
                    {
                        string next_role = Prolink.Math.GetValueAsString(dtapprove.Rows[i + 1]["ROLE"]);
                        string email = string.Empty;
                        string userid = string.Empty;
                        string _role= next_role.Replace(role,"");
                        Regex m=new Regex("M");
                        int mcount=m.Matches(_role).Count;
                        if (next_role.Equals("AM") )//|| (_role.Length==mcount&&_role.Length>0))
                        {
                            Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(userinfo.UserId);
                            //Business.TPV.Employe employe1 = GetSuperior(UserId);
                            if (employe1 == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                            email = employe1.EMail;
                            userid = employe1.EmployeCode;
                            noticeto = userid;
                            notice_mail = email;
                            j++;
                            string rolename = GetApproveName(role, approvename, userinfo);
                            mixList.Add(ApproveEdit(uid, userinfo.Dep + "M", approvecode, approvename, rolename, approvetime, j, userid, email, next_role, max, userinfo.CompanyId, isOkTime));
                            isOkTime = false;
                            if (dtapprove.Rows.Count > (i + 2))
                            {
                                next_role = Prolink.Math.GetValueAsString(dtapprove.Rows[i + 2]["ROLE"]);
                            }
                            if (next_role.Equals("50LEVEL"))
                            {
                                returnMsg = GetFiftyApprove(uid, userinfo, mixList, approvecode, approvename, approvetime, ref j, ref userid, ref isOkTime, next_role, max);
                                if (!string.IsNullOrEmpty(returnMsg)) return returnMsg;

                            }
                        }
                    }
                }

                try
                {
                    OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                }
            }

            return returnMsg;
        }
        #endregion
        #region
        public static string GetApproveName(string role, string approvename, UserInfo userinfo)
        {
            string sqlDep = string.Format("SELECT TOP 1 GROUP_DESCP FROM APPROVE_ATTR_D WHERE GROUP_ID={0} AND CMP={1} AND APPROVE_GROUP={2} AND U_FID IN(SELECT U_ID FROM APPROVE_ATTRIBUTE C WHERE C.CMP={1} AND APPROVE_ATTR='QUOT')", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.CompanyId), SQLUtils.QuotedStr(role));
            string _approvename = OperationUtils.GetValueAsString(sqlDep, Prolink.Web.WebContext.GetInstance().GetConnection());
            return _approvename == "" ? approvename : _approvename;
        }
        #endregion
        private static EditInstruct ApproveEdit(string RefNo, string role, string approvecode, string approvename, string rolename, string approvetime, int j, string userid, string noticemail, string afdrole, int voidtime, string CompanyId, bool noticetime = false)
        {
            EditInstruct levelei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
            levelei.Put("NOTICE_TO", userid);
            levelei.Put("NOTICE_MAIL", noticemail);//employe.EMail
            levelei.Put("STATUS", "0");
            levelei.Put("ROLE", role);//Dep + "MM"
            levelei.Put("ROLE_NM", rolename);
            levelei.Put("REF_NO", RefNo);
            levelei.Put("APPROVE_LEVEL", j);
            levelei.Put("VOID_LOOP", voidtime);
            if (noticetime)
            {
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                levelei.PutDate("NOTIFY_DATE", odt);
                levelei.PutDate("NOTIFY_DATE_L", ndt);
            }
            levelei.Put("AFD_ROLE", afdrole);
            levelei.Put("APPROVE_CODE", approvecode);
            levelei.Put("APPROVE_NAME", approvename);
            levelei.Put("APPROVE_TIME", approvetime);
            return levelei;
        }
        private static string GetFiftyApprove(string DnNo, UserInfo userinfo, MixedList mixList, string approvecode, string approvename, string approvetime, ref int j, ref string userid, ref bool isOkTime, string afdrole, int max)
        {
            if (string.IsNullOrEmpty(userid)) userid = userinfo.UserId;
            Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmploye(userid);
            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
            if (employe != null)
            {
                while (employe.OrgLevel < 50)
                {
                    employe = Business.TPV.Context.OrgService.GetSuperior(employe);
                    userid = employe.EmployeCode;
                }

                j++;
                string rolename = GetApproveName(userinfo.Dep + "MM", approvename, userinfo);
                EditInstruct levelei = ApproveEdit(DnNo, userinfo.Dep + "MM", approvecode, approvename, rolename, approvetime, j, userid, employe.EMail, afdrole, max, userinfo.CompanyId, isOkTime);
                isOkTime = true;
                mixList.Add(levelei);
            }
            return string.Empty;
        }
        private static string GetLevelApprove(string DnNo, string UserId, string Dep, MixedList mixList, string approvecode, string approvename, string approvetime, ref int j, ref string userid, ref bool isOkTime, string afdrole, int max, string CompanyId, int level = 50, string rolename = "", string Role = "")
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
                EditInstruct levelei = ApproveEdit(DnNo, Role, approvecode, approvename, rolename, approvetime, j, userid, employe.EMail, afdrole, max, CompanyId, isOkTime);
                isOkTime = true;
                mixList.Add(levelei);
            }
            return string.Empty;
        }
        public static EditInstruct GetApproveEi1(string debitno, string approvetype, string Approveto, string UserId, string NextLevel, string CompanyId)
        {
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", debitno);
            apei.PutKey("APPROVE_CODE", approvetype);
            //apei.PutKey("APPROVE_LEVEL", Approveto);
            if (!string.IsNullOrEmpty(NextLevel))
            {
                apei.PutKey("APPROVE_LEVEL", NextLevel);
            }
            apei.Put("CREATE_BY", UserId);
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            apei.PutDate("CREATE_DATE", odt);
            apei.PutDate("CREATE_DATE_L", ndt);
            apei.Put("APPROVE_BY", UserId);
            apei.PutDate("APPROVE_DATE", odt);
            apei.PutDate("APPROVE_DATE_L", ndt);
            apei.Put("STATUS", "1");
            apei.Put("REMARK", @Resources.Locale.L_Pass);
            return apei;
        }

        public static void ChangeFinish(string uid, string CompanyId)
        {
            string sql = string.Format(@"SELECT TOP 1 STATUS FROM APPROVE_RECORD WHERE REF_NO={0} ORDER BY APPROVE_LEVEL DESC", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow[] drs = dt.Select("STATUS='0'");
                if (drs.Length <= 0)
                {
                    MixedList mxlist = new MixedList();
                    mxlist.Add(UpdateQuotFinish(uid, CompanyId));
                    mxlist.Add(UpdateQuotDFinish(uid));
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
        public static EditInstruct UpdateQuotFinish(string uid, string CompanyId)
        {
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("QUOT_TYPE", "A");
            ei.Put("APPROVE_TO", "Finish");
            ei.Put("APPROVE_USER", "");
            ei.Put("APPROVE_BY", user);
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("APPROVE_DATE", odt);
            ei.PutDate("APPROVE_DATE_L", ndt);
            return ei;
        }
        public static EditInstruct UpdateQuotDFinish(string uid)
        {
            EditInstruct ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("U_FID", uid);
            ei.Put("QUOT_TYPE", "F");
            return ei;
        }
        #region 签核退回
        public static string QuotApproveBack(string uid, string backremark, UserInfo userinfo)
        {
            string returnMsg = "";
            DataTable nowdata = GetDataByUId(uid);
            if (nowdata.Rows.Count <= 0) return @Resources.Locale.L_DNManageController_Controllers_105;

            string aptype = nowdata.Rows[0]["APPROVE_TYPE"].ToString();
            string ApproveTo = nowdata.Rows[0]["APPROVE_TO"].ToString();
            string quotno = Prolink.Math.GetValueAsString(nowdata.Rows[0]["QUOT_NO"]);
            string QuotType = Prolink.Math.GetValueAsString(nowdata.Rows[0]["QUOT_TYPE"]);
            returnMsg = CheckApproveStaus(QuotType, 0);
            modetype = Prolink.Math.GetValueAsString(nowdata.Rows[0]["TRAN_MODE"]);
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return returnMsg;
            }
            string noticeuser = string.Empty;
            string noticemail = string.Empty;
            MixedList mixList = new MixedList();

            string conditions = string.Format("REF_NO={0} AND APPROVE_CODE={1} ", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype));

            string sql = string.Format("SELECT * FROM APPROVE_RECORD WHERE {0} AND ROLE={1}", conditions, SQLUtils.QuotedStr(ApproveTo));
            DataTable dtap = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            bool isok = CheckBackLevel(ApproveTo, userinfo.UserId, userinfo.CompanyId, userinfo.GroupId, "", userinfo.Upri, userinfo.Dep);
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
                return returnMsg = @Resources.Locale.L_QTManage_Controllers_tip3 + quotno + @Resources.Locale.L_BillApproveHelper_Business_24;
            }

            sql = string.Format(@"SELECT AFD_ROLE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2} ", SQLUtils.QuotedStr(uid),
                    SQLUtils.QuotedStr(aptype), SQLUtils.QuotedStr(ApproveTo));
            string afdrole = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            //if ("A".Equals(afdrole))    //针对退回申请者状态
            //{

            //    afdrole = "A";
            //    mixList.Add(UpApproveStatus(uid, afdrole));
            //    sql = string.Format("UPDATE APPROVE_RECORD SET STATUS='0' WHERE REF_NO={0} AND APPROVE_CODE={1}",
            //        SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype));
            //    mixList.Add(sql);
            //    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            //    //string subject = @Resources.Locale.L_QTQuery_QuotNo + ":" + quotno + @Resources.Locale.L_BillApproveHelper_Business_32 + backremark;
            //    //returnMsg += AddToNoticeQuot("", userinfo.basecondtions, subject, userinfo.UserId, userinfo.GroupId, userinfo.CompanyId, userinfo.Dep, uid, noticeuser, noticemail);
            //    return @Resources.Locale.L_BillApproveHelper_Business_31;
            //}
            if (afdrole != "A")
                afdrole = GetReturnRole(mixList, uid, aptype, afdrole, userinfo);
            if (string.IsNullOrEmpty(afdrole) && ApproveTo != "A") return "无法退回上一层!";


            sql = string.Format(@"SELECT TOP 1 * FROM APPROVE_RECORD WHERE REF_NO={0}
  AND APPROVE_CODE={1} AND AFD_ROLE={2} AND STATUS='1' ORDER BY CAST(APPROVE_LEVEL AS INT) DESC",
                 SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(aptype), SQLUtils.QuotedStr(afdrole));

//            sql = string.Format(@"SELECT TOP 1 * FROM APPROVE_RECORD WHERE {0}  AND STATUS='1' AND CAST(APPROVE_LEVEL AS INT)
//                <(SELECT TOP 1 CAST(APPROVE_LEVEL AS INT) FROM APPROVE_RECORD WHERE  {0} AND ROLE={1})
//                 ORDER BY CAST(APPROVE_LEVEL AS INT) DESC", conditions, SQLUtils.QuotedStr(ApproveTo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count == 0)
            {
                if (ApproveTo == "A" || afdrole == "A")
                {
                    string mark = Prolink.Math.GetValueAsString(nowdata.Rows[0]["BACK_REMARK"]);
                    //申請者退回就把帳單狀態改為「拒絕」
                    sql = "UPDATE SMQTM SET QUOT_TYPE='R',APPROVE_USER=NULL, BACK_REMARK={0} WHERE U_ID={1}";
                    //mark += DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm") + ":" + backremark + "\r\n";
                    mark +=  backremark + "\r\n";
                    sql = string.Format(sql, SQLUtils.QuotedStr(backremark), SQLUtils.QuotedStr(uid));

                    SendQuotRJMail(uid, userinfo.GroupId, userinfo.CompanyId, backremark);
                    try
                    {
                        OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        return @Resources.Locale.L_QTManage_Controllers_tip5;
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return @Resources.Locale.L_DNApproveLoop_Business_57;
            }

            string NextDnType = Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);

            //if ("A".Equals(afdrole) || "AM".Equals(afdrole))    //针对退回申请者状态
            //{
            //    NextDnType = "A";
            //}
            noticeuser = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_BY"]);   //抓取签核过的人
            //noticemail = Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);
            sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(noticeuser));
            noticemail = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (string.IsNullOrEmpty(noticemail)) return @Resources.Locale.L_BillApproveHelper_Business_28;

            string uuid = string.Empty;
            //更新SMQTM的状态
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", "A".Equals(afdrole) ? afdrole : NextDnType);
            ei.Put("APPROVE_USER", noticeuser); //更新此笔签核人员到上一层
            ei.Put("APPROVE_BY", userinfo.UserId);
            DateTime odt = DateTime.Now;           
            string CompanyId = userinfo.CompanyId;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("APPROVE_DATE", odt);
            ei.PutDate("APPROVE_DATE_L", ndt);
            mixList.Add(ei);

            //写入笔资料到签核明细档
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", uid);
            apei.PutKey("APPROVE_CODE", aptype);
            apei.PutKey("ROLE", NextDnType);
            //apei.Put("ROLE", UserRole);
            apei.Put("APPROVE_BY", userinfo.UserId);        //变更签核明细里面的签核人员
            apei.PutDate("APPROVE_DATE", odt);
            apei.PutDate("APPROVE_DATE_L", ndt);
            apei.Put("NOTICE_TO", noticeuser);
            apei.Put("NOTICE_MAIL", noticemail);
            apei.Put("STATUS", "0");
            //backremark = @Resources.Locale.L_BillApproveHelper_Business_29 + userinfo.UserId + @Resources.Locale.L_BillApproveHelper_Business_30 + backremark;
            apei.Put("REMARK", @Resources.Locale.L_BillApproveHelper_Business_29 + userinfo.UserId + @Resources.Locale.L_BillApproveHelper_Business_30 + backremark);
            mixList.Add(apei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = @Resources.Locale.L_BillApproveHelper_Business_31;
                string subject = @Resources.Locale.L_QTQuery_QuotNo + ":" + quotno + "  " + @Resources.Locale.L_QTManage_Controllers_tip4 + backremark;
                    //@Resources.Locale.L_QTQuery_QuotNo + ":" + quotno; //+ @Resources.Locale.L_BillApproveHelper_Business_32 + backremark;
                returnMsg += AddToBackQuot(uuid, userinfo.basecondtions, subject, userinfo.UserId, userinfo.GroupId, userinfo.CompanyId, userinfo.Dep, uid, noticeuser, noticemail, backremark);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }
        #endregion
        /// <summary>
        /// 调过主管层级
        /// </summary>
        /// <param name="mixList"></param>
        /// <param name="uid"></param>
        /// <param name="apcode"></param>
        /// <param name="afdrole"></param>
        /// <param name="dep"></param>
        /// <returns></returns>
        public static string GetReturnRole(MixedList mixList, string uid, string apcode, string afdrole, UserInfo userinfo)
        {
            string sql = string.Format(@"SELECT * FROM APPROVE_FLOW_D WHERE {0} AND APPROVE_CODE={1} AND 
                CAST(APPROVE_LEVEL AS INT)<(SELECT TOP 1 CAST(APPROVE_LEVEL AS INT) FROM APPROVE_FLOW_D WHERE {0} AND APPROVE_CODE={1} AND ROLE={2})
                ORDER BY CAST(APPROVE_LEVEL AS INT) DESC",
              APFlowDSql(userinfo), SQLUtils.QuotedStr(apcode), SQLUtils.QuotedStr(afdrole));
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
        private static string APFlowDSql(UserInfo userinfo)
        {
            if (userinfo == null) return string.Empty;
            return string.Format("GROUP_ID={0} AND CMP_ID = {1}", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.CompanyId));
        }
        public static EditInstruct UpApproveStatus(string uid, string NextDnType, string CompanyId, string noticeuser = "")
        {
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", "A");
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("APPROVE_DATE", odt);
            ei.PutDate("APPROVE_DATE_L", ndt);
            ei.Put("APPROVE_USER", noticeuser); //更新此笔签核人员到上一层
            ei.Put("APPROVE_BY", user);
            return ei;
        }
        public static string CheckApproveStaus(string QuotType, int type)
        {
            string returnMsg = string.Empty;
            if (type == 1)
            {
                switch (QuotType)
                {
                    case "P": returnMsg = @Resources.Locale.L_BillApproveHelper_Business_2; break;
                    case "V": returnMsg = @Resources.Locale.L_BillApproveHelper_Business_3; break;
                    case "A": returnMsg = @Resources.Locale.L_BillApproveHelper_Business_35; break;
                }
            }
            if (type == 0)
            {//退回
                switch (QuotType)
                {
                    case "P": returnMsg = @Resources.Locale.L_BillApproveHelper_Business_5; break;
                    case "V": returnMsg = @Resources.Locale.L_BillApproveHelper_Business_6; break;
                    case "A": returnMsg = @Resources.Locale.L_BillApproveHelper_Business_7; break;
                }
            }
            return returnMsg;
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
        public static string GetApproveBack(string UserId, string CompanyId, string GroupId, string upri, string Dep)
        {
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR='QUOT'",//AND DP.STN={2}
             SQLUtils.QuotedStr(UserId),
             SQLUtils.QuotedStr(CompanyId),
             SQLUtils.QuotedStr(GroupId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string approveroles = "Person;A;LST";//申请者的状态下也可以出现  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approveroles += Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_GROUP"]) + ";";
            }
            approveroles = approveroles.Trim(';');
            return approveroles;
        }

        #region 报价拒绝时发送Mail
        public static void SendQuotRJMail(string uid, string groupid, string companyid,string reason="")
        {
            string sql = "SELECT LSP_CD,QUOT_NO,TRAN_MODE,LSP_NM FROM SMQTM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return;
            string LspNo = dt.Rows[0]["LSP_CD"].ToString();
            string quotno = dt.Rows[0]["QUOT_NO"].ToString();
            string tranmode = dt.Rows[0]["TRAN_MODE"].ToString();
            string LspNm = dt.Rows[0]["LSP_NM"].ToString();
            string _tranmode = "Q" + tranmode;
            DataTable mailGroupDt = MailTemplate.GetMailGroup(LspNo, groupid, _tranmode);
            if (mailGroupDt.Rows.Count > 0)
            {
                foreach (DataRow item1 in mailGroupDt.Rows)
                {
                    string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                    if (mailStr != "")
                    {
                        string type = tranmode.Equals("X") ? MailManager.QuotRefuse_X : tranmode.Equals("B") ? MailManager.QuotRefuse_B : tranmode.Equals("C") ? MailManager.QuotRefuse_C : tranmode.Equals("O") ? MailManager.QuotRefuse_O : string.Empty;
                        EvenFactory.AddEven(uid + "#" + groupid + "#" + companyid + "#" + Guid.NewGuid().ToString() + "#" + type + "#" + LspNm + "#" + reason, uid, type, null, 1, 0, mailStr, @Resources.Locale.L_QTQuery_QuotNo + ":" + quotno + "  " + @Resources.Locale.L_ActSetup_CheckDescp+"：" + reason, "");
                    }
                }
            }
        }
        #endregion
        
    }
}