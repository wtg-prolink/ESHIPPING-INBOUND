using Business;
using Business.Service;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using WebGui.Models;
using WebGui.Controllers;

namespace Business
{
    public class ContractApproveHelper : BaseApprove
    {
        #region 發起簽核
        protected const string CONTRACT_ATTR = "Contract";

        public static string ApproveBillItem(string uid, string jobno, UserInfo userinfo)
        {
            MixedList mixList = new MixedList();
            string returnMsg = string.Empty;
            string NextDnType = "";
            string ffid = "";
            string nowLevel = string.Empty;
            string status = string.Empty;
            string ApproveType = string.Empty;
            string ApproveTo = string.Empty;
            string ApproveBack = string.Empty;

            #region 取得SMCTM資料
            DataTable nowdata = GetDataByUId(uid);
            ApproveType = "SC_Approve";// nowdata.Rows[0]["APPROVE_TYPE"].ToString();
            ApproveTo = nowdata.Rows[0]["APPROVE_TO"].ToString();
            ApproveBack = nowdata.Rows[0]["APPROVE_BACK"].ToString();
            string groupid = nowdata.Rows[0]["GROUP_ID"].ToString();
            string cmp = nowdata.Rows[0]["CMP"].ToString();
            string shipmentid = nowdata.Rows[0]["RFQ_NO"].ToString();
            string approveuser = nowdata.Rows[0]["APPROVE_USER"].ToString();
            #endregion
            string noticeto = string.Empty;
            string notice_mail = string.Empty;
            if (string.IsNullOrEmpty(ApproveTo))
                ApproveTo = "A";

            bool IsOK = CheckLastOK(jobno, ApproveType, ApproveTo, approveuser);
            if (IsOK)
            {
                return returnMsg = @Resources.Locale.L_ContractApproveHelper_Business_44 + jobno + @Resources.Locale.L_BillApproveHelper_Business_35;
            }

            IsOK = CheckApproveLevel(ApproveTo, userinfo, approveuser, CONTRACT_ATTR);
            if (!IsOK)
            {
                return returnMsg = string.Format(@Resources.Locale.L_ContractApproveHelper_Business_45, jobno);
            }

            //检查ApproveRecord是否有值
            string alsql = string.Format("SELECT APPROVE_LEVEL FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(jobno),
                SQLUtils.QuotedStr(ApproveType),
                SQLUtils.QuotedStr(ApproveTo));
            if (!string.IsNullOrEmpty(approveuser))
            {
                alsql += NoticeToSql(approveuser);
            }
            DataTable recorddt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql = string.Empty;
            string NextLevel = string.Empty;
            #region 產生第一次簽核資料
            if (ApproveTo == "A" ||string.IsNullOrEmpty(ApproveTo))
            {
                returnMsg = createApproveRecord(jobno, ref NextDnType, ApproveType, ref noticeto, ref notice_mail, userinfo, 0, cmp);
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
                alsql = string.Format(sql, SQLUtils.QuotedStr(nowLevel), SQLUtils.QuotedStr(jobno), SQLUtils.QuotedStr(ApproveType));
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
                }
                NextDnType = OperationUtils.GetValueAsString(alsql, Prolink.Web.WebContext.GetInstance().GetConnection());

                EditInstruct apei = GetApproveEi(uid, ApproveType, ApproveTo, userinfo.UserId, nowLevel, cmp);
                mixList.Add(apei);

                sql = GetUpActualTimeSql(uid, ApproveType, ApproveTo);
                mixList.Add(sql);
                if (string.IsNullOrEmpty(NextDnType))
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        ChangeFinish(jobno,uid);
                        returnMsg = "success";
                    }
                    catch (Exception ex)
                    {
                        returnMsg = jobno + ":"+@Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
                    }
                    return returnMsg;
                }

                EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                newEi.PutKey("REF_NO", jobno);
                newEi.PutKey("APPROVE_CODE", ApproveType);
                newEi.PutKey("ROLE", NextDnType);
                newEi.PutKey("APPROVE_LEVEL", NextLevel);
                DateTime odt = DateTime.Now;              
                string CompanyId = cmp;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                newEi.PutDate("NOTIFY_DATE", odt);
                newEi.PutDate("NOTIFY_DATE_L", ndt);
                mixList.Add(newEi);
            }
            #endregion
            #region 查無簽核流程設定
            else if (recorddt.Rows.Count <= 0)
            {
                return returnMsg = string.Format(@Resources.Locale.L_ContractApproveHelper_Business_46, jobno);
            }
            #endregion

            ResultInfo resultinfo = GetAPAttrDUi(NextDnType, notice_mail, cmp, CONTRACT_ATTR);
            if (resultinfo.IsSucceed)
                ffid = resultinfo.Description;
            else
                return resultinfo.Description;
           

            //更新SMDN的状态
            EditInstruct ei = new EditInstruct("SMCTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_USER", noticeto);
            ei.Put("APPROVE_TO", NextDnType);
            mixList.Add(ei);


            IMailTemplateParse parse = new DefaultMailParse();
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = jobno + ":"+@Resources.Locale.L_BillApproveHelper_Business_38;

                string subject = @Resources.Locale.L_BillApproveHelper_Business_39 + jobno + @Resources.Locale.L_BillApproveHelper_Business_40;

                returnMsg += AddToNoticeBill(ffid, userinfo, subject, uid, noticeto, notice_mail);
                //returnMsg = "success";

            }
            catch (Exception ex)
            {
                returnMsg = jobno + ":"+@Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
            }
            return returnMsg;

        }
        #endregion

        
        #region 創建approve record
        public static string createApproveRecord(string jobno, ref string NextDnType, string ApproveType, ref string noticeto, ref string notice_mail, UserInfo userinfo, Decimal amt, string CompanyId)
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
            sql = string.Format(sql, SQLUtils.QuotedStr(jobno), SQLUtils.QuotedStr(ApproveType));

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
                int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(jobno)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;
                for (int i = 0; i < dtapprove.Rows.Count; i++)
                {
                    EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
                    string role = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["ROLE"]);
                    approvecode = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["APPROVE_CODE"]);
                    approvename = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["GROUP_DESCP"]);
                    approvetime = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["APPROVE_TIME"]);
                    apei.Put("AFD_ROLE", Prolink.Math.GetValueAsString(dtapprove.Rows[i]["ROLE"]));
                    apei.Put("STATUS", "0");
                    apei.Put("REF_NO", jobno);
                    apei.Put("VOID_LOOP", max);
                    apei.Put("APPROVE_CODE", approvecode);
                    apei.Put("APPROVE_NAME", approvename);

                    if (i == 0)
                    {
                        apei.Put("STATUS", "1");
                        apei.Put("APPROVE_BY", userinfo.UserId);
                        DateTime odt = DateTime.Now;
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        apei.PutDate("APPROVE_DATE", odt);
                        apei.PutDate("NOTIFY_DATE", odt);
                        apei.PutDate("APPROVE_DATE_L", ndt);
                        apei.PutDate("NOTIFY_DATE_L", ndt);
                        apei.Put("REMARK", @Resources.Locale.L_ContractQuery_Views_478);
                        role = userinfo.Dep;
                    }
                    else if (i == 1)
                    {
                        DateTime odt = DateTime.Now;
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        apei.PutDate("NOTIFY_DATE", odt);
                        apei.PutDate("NOTIFY_DATE_L", ndt);
                        apei.Put("STATUS", "0");
                        if (role == "AM")
                        {
                            role = userinfo.Dep + "M";
                            NextDnType = role;
                            continue;
                        }
                    }
                    if (role.Equals("50LEVEL"))
                    {
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
                        if (next_role.Equals("AM"))
                        {
                            Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(userinfo.UserId);
                            //Business.TPV.Employe employe1 = GetSuperior(UserId);
                            if (employe1 == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                            email = employe1.EMail;
                            userid = employe1.EmployeCode;
                            noticeto = userid;
                            notice_mail = email;
                            j++;
                            mixList.Add(ApproveEdit(jobno, userinfo.Dep + "M", approvecode, approvename, approvetime, j, userid, email, next_role, max, userinfo.CompanyId, isOkTime));
                            isOkTime = false;
                            if (dtapprove.Rows.Count > (i + 2))
                            {
                                next_role = Prolink.Math.GetValueAsString(dtapprove.Rows[i + 2]["ROLE"]);
                            }
                            if (next_role.Equals("50LEVEL"))
                            {
                                returnMsg = GetFiftyApprove(jobno, userinfo, mixList, approvecode, approvename, approvetime, ref j, ref userid, ref isOkTime, next_role, max, amt);
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

        private static string GetFiftyApprove(string DnNo, UserInfo userinfo, MixedList mixList, string approvecode, string approvename, string approvetime, ref int j, ref string userid, ref bool isOkTime, string afdrole, int max, decimal amt = 0)
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
                EditInstruct levelei = ApproveEdit(DnNo, userinfo.Dep + "MM", approvecode, approvename, approvetime, j, userid, employe.EMail, afdrole, max, userinfo.CompanyId, isOkTime);
                isOkTime = true;
                mixList.Add(levelei);
            }
            string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE,AP_CD FROM BSCODE WHERE GROUP_ID={0} AND (CMP='*' OR CMP={1}) AND CD_TYPE ='ATST' ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.CompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                decimal amount = Prolink.Math.GetValueAsDecimal(dr["CD"]);

                if (amt > amount)
                {
                    int level = Prolink.Math.GetValueAsInt(dr["AP_CD"]);
                    string nextrole = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                    nextrole = nextrole.Replace("A", userinfo.Dep);
                    GetLevelApprove(DnNo, userinfo.UserId, userinfo.Dep, mixList, approvecode, approvename, approvetime, ref j, ref userid, ref isOkTime, nextrole, max, userinfo.CompanyId, level, nextrole);
                }
            }
            return string.Empty;
        }

        private static string GetLevelApprove(string DnNo, string UserId, string Dep, MixedList mixList, string approvecode, string approvename, string approvetime, ref int j, ref string userid, ref bool isOkTime, string afdrole, int max, string CompanyId, int level = 50, string Role = "")
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
                EditInstruct levelei = ApproveEdit(DnNo, Role, approvecode, approvename, approvetime, j, userid, employe.EMail, afdrole, max, CompanyId, isOkTime);
                isOkTime = true;
                mixList.Add(levelei);
            }
            return string.Empty;
        }



        private static EditInstruct ApproveEdit(string RefNo, string role, string approvecode, string approvename, string approvetime, int j, string userid, string noticemail, string afdrole, int voidtime, string CompanyId, bool noticetime = false)
        {
            EditInstruct levelei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
            levelei.Put("NOTICE_TO", userid);
            levelei.Put("NOTICE_MAIL", noticemail);//employe.EMail
            levelei.Put("STATUS", "0");
            levelei.Put("ROLE", role);//Dep + "MM"
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

        public static string GetApprove(string UserId, string CompanyId, string GroupId, string upri, string Dep, string ContractAttr="")
        {
            ContractAttr = string.IsNullOrEmpty(ContractAttr) ? CONTRACT_ATTR : ContractAttr;
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR={3}",//AND DP.STN={2}
             SQLUtils.QuotedStr(UserId),
             SQLUtils.QuotedStr(CompanyId),
             SQLUtils.QuotedStr(GroupId),
            SQLUtils.QuotedStr(ContractAttr));
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



        public static void ChangeFinish(string jobno,string uid)
        {
            string sql = string.Format(@"SELECT TOP 1 STATUS FROM APPROVE_RECORD WHERE REF_NO={0} ORDER BY APPROVE_LEVEL DESC", SQLUtils.QuotedStr(jobno));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow[] drs = dt.Select("STATUS='0'");
                if (drs.Length <= 0)
                {
                    MixedList mxlist = new MixedList();
                    mxlist.Add(UpdateBillFinish(uid));
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

        public static EditInstruct UpdateBillFinish(string uid)
        {
            EditInstruct ei = new EditInstruct("SMCTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", "Finish");
            return ei;
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
        public static string AddToNoticeBill(string uuid ,UserInfo userinfo, string subject,  string billUid, string noticeUser = null, string notice_mail = null)
        {
            string baseCondition = userinfo.basecondtions;
            string userId = userinfo.UserId;

            string msg = string.Empty;
            if (!string.IsNullOrEmpty(notice_mail))
            {
                EvenFactory.AddEven(billUid + "#" + userinfo.GroupId + "#" + userinfo.CompanyId + "#" + Guid.NewGuid().ToString(), billUid, MailManager.ContractAp, null, 1, 0, notice_mail, subject, "", 3, 0);
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
                EditInstruct ei = AddMessage(md, userinfo, receiveUser);
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


        

        public static EditInstruct GetApproveEi(string debitno, string approvetype, string Approveto, string UserId, string NextLevel, string CompanyId)
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

        public static DataTable GetDataByUId(string uid)
        {
            string sql = string.Format("SELECT * FROM SMCTM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static string ContApproveBack(string uid, string backremark, UserInfo userinfo)
        {
            string returnMsg = "";
            DataTable nowdata = GetDataByUId(uid);
            if (nowdata.Rows.Count <= 0) return @Resources.Locale.L_ContractApproveHelper_Business_47;

            string aptype = "SC_Approve";
            string DebitNo = nowdata.Rows[0]["JOB_NO"].ToString();
            string ApproveTo = nowdata.Rows[0]["APPROVE_TO"].ToString();
            string jobno = nowdata.Rows[0]["JOB_NO"].ToString();
            string noticeuser = string.Empty;
            string noticemail = string.Empty;
            MixedList mixList = new MixedList();

            string conditions = string.Format("REF_NO={0} AND APPROVE_CODE={1} ", SQLUtils.QuotedStr(jobno), SQLUtils.QuotedStr(aptype));

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
                return returnMsg = @Resources.Locale.L_BillApproveHelper_Business_23 + DebitNo + @Resources.Locale.L_BillApproveHelper_Business_24;
            }

            sql = string.Format(@"SELECT AFD_ROLE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2} ", SQLUtils.QuotedStr(jobno),
                    SQLUtils.QuotedStr(aptype), SQLUtils.QuotedStr(ApproveTo));
            string afdrole = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if ("A".Equals(afdrole))    //针对退回申请者状态
            {

                afdrole = "A";
                mixList.Add(Business.BillApproveHelper.UpApproveStatus(uid, afdrole, userinfo));
                sql = string.Format("UPDATE APPROVE_RECORD SET STATUS='0' WHERE REF_NO={0} AND APPROVE_CODE={1}",
                    SQLUtils.QuotedStr(jobno), SQLUtils.QuotedStr(aptype));
                mixList.Add(sql);
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                return @Resources.Locale.L_BillApproveHelper_Business_25;
            }

            sql = string.Format(@"SELECT TOP 1 * FROM APPROVE_RECORD WHERE {0}  AND STATUS='1' AND CAST(APPROVE_LEVEL AS INT)
                <(SELECT TOP 1 CAST(APPROVE_LEVEL AS INT) FROM APPROVE_RECORD WHERE  {0} AND ROLE={1})
                 ORDER BY CAST(APPROVE_LEVEL AS INT) DESC", conditions, SQLUtils.QuotedStr(ApproveTo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count == 0)
            {
                return @Resources.Locale.L_BillApproveHelper_Business_27;
            }

            string NextDnType = Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);
            noticeuser = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_BY"]);   //抓取签核过的人
            //noticemail = Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);
            sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(noticeuser));
            noticemail = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (string.IsNullOrEmpty(noticemail)) return @Resources.Locale.L_BillApproveHelper_Business_28;

            string uuid = string.Empty;
            //更新SMDN的状态
            EditInstruct ei = new EditInstruct("SMCTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", NextDnType);
            ei.Put("APPROVE_USER", noticeuser); //更新此笔签核人员到上一层
            mixList.Add(ei);

            //写入笔资料到签核明细档
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", jobno);
            apei.PutKey("APPROVE_CODE", aptype);
            apei.PutKey("ROLE", NextDnType);
            //apei.Put("ROLE", UserRole);
            apei.Put("APPROVE_BY", userinfo.UserId);        //变更签核明细里面的签核人员
            DateTime odt = DateTime.Now;          
            string CompanyId = userinfo.CompanyId;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            apei.PutDate("APPROVE_DATE", odt);
            apei.PutDate("APPROVE_DATE_L", ndt);
            apei.Put("NOTICE_TO", noticeuser);
            apei.Put("NOTICE_MAIL", noticemail);
            apei.Put("STATUS", "0");
            backremark = @Resources.Locale.L_BillApproveHelper_Business_29 + userinfo.UserId +  @Resources.Locale.L_BillApproveHelper_Business_30 + backremark;
            apei.Put("REMARK", backremark);
            mixList.Add(apei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = @Resources.Locale.L_BillApproveHelper_Business_31;
                string subject = @Resources.Locale.L_ContractApproveHelper_Business_48 + DebitNo +  @Resources.Locale.L_BillApproveHelper_Business_32 + backremark;

                returnMsg += AddToNoticeBill(uuid, userinfo, subject, uid, noticeuser, noticemail);
                //returnMsg += AddToNoticeBill(uuid, userinfo.basecondtions, subject, userinfo.UserId, userinfo.GroupId, userinfo.CompanyId, userinfo.Dep, uid, noticeuser, noticemail);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
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
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR='BILLING'",//AND DP.STN={2}
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
    }
}