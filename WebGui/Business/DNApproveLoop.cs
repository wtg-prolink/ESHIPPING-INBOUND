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

namespace Business
{
    public class DNApproveLoop
    {

        public static string GetApprove(string UserId, string CompanyId, string GroupId,string upri,string Dep)
        {
            string approveroles = GetOriginalApprove(UserId, CompanyId, GroupId, upri, Dep);
            approveroles += ";";
            if ("U" != upri && ("SS".Equals(Dep) || "RD".Equals(Dep) || "CS".Equals(Dep) || "MP".Equals(Dep) || "FI".Equals(Dep) || "SFI".Equals(Dep)))
            {
                if ("FI".Equals(Dep) || "SFI".Equals(Dep))
                {
                    approveroles += "FIM;";
                }
                else
                {
                    approveroles += Dep + "M;" + Dep + "MM;";
                }
            }
            approveroles = approveroles.Trim(';');
            return approveroles;
        }

        public static string GetOriginalApprove(string UserId, string CompanyId, string GroupId, string upri, string Dep)
        {
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP
 ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR='DN'",//AND DP.STN={2}
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

        public static bool CheckApproveLevel(string apprvesto, string UserId, string CompanyId, string GroupId,string approveuser,string upri,string dep)
        {
            bool check = false;
            if (!string.IsNullOrEmpty(approveuser) && !string.IsNullOrEmpty(UserId))
            {
                if (UserId.ToUpper() == approveuser.ToUpper()) return true;
            }

            if (UserId == approveuser) return true;
            if ("MANA10".Equals(UserId)) return true;
            //string approvelists = GetApprove(UserId, CompanyId, GroupId, upri, dep);
            string approvelists = GetOriginalApprove(UserId, CompanyId, GroupId, upri, dep);
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

        public static string ApproveDnItem(string uid, string DnNo, string UserId, string Dep, string baseStn,string Upri)
        {
            MixedList mixList = new MixedList();
            string returnMsg = string.Empty;
            string NextDnType = "";
            string ffid = "";
            string nowLevel = string.Empty;
            string status = string.Empty;
            string DnType = string.Empty;
            string ApproveTo = string.Empty;
            string ApproveBack = string.Empty;

            DataTable nowdata = DNInfoCheck.GetDNDataByUId(uid);
            DnType = nowdata.Rows[0]["APPROVE_TYPE"].ToString();
            ApproveTo = nowdata.Rows[0]["APPROVE_TO"].ToString();
            ApproveBack = nowdata.Rows[0]["APPROVE_BACK"].ToString();
            string groupid = nowdata.Rows[0]["GROUP_ID"].ToString();
            string cmp = nowdata.Rows[0]["CMP"].ToString();
            string shipmentid = nowdata.Rows[0]["SHIPMENT_ID"].ToString();
            string combineinfo = nowdata.Rows[0]["COMBINE_INFO"].ToString();
            string approveuser = nowdata.Rows[0]["APPROVE_USER"].ToString();

            string pod = nowdata.Rows[0]["POD"].ToString();
            if(string.IsNullOrEmpty(pod))
                return  @Resources.Locale.L_DNManageController_Controllers_108 + DnNo + @Resources.Locale.L_DNApproveLoop_Business_49; 
            string noticeto = string.Empty;
            string notice_mail = string.Empty;

            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = cmp,
                GroupId = groupid,
                Dep = Dep,
                basecondtions = baseStn
            };

            if (ApproveBack.Equals("Y"))
            {
                return DnApproveBack(uid, @Resources.Locale.L_DNApproveLoop_Business_50, userinfo);  
                //return ApproveBackY(uid, DnNo, DnType, baseStn, groupid, cmp, UserId, Dep);
            }
            status = DNInfoCheck.CheckDNApproveStatus(Prolink.Math.GetValueAsString(nowdata.Rows[0]["STATUS"]));
            if (!string.IsNullOrEmpty(status))
            {
                return returnMsg = status;
            }

            bool IsOK = CheckLastOK(DnNo, DnType, ApproveTo, approveuser);
            if (IsOK)
            {
                return returnMsg = @Resources.Locale.L_DNManageController_Controllers_108 + DnNo + @Resources.Locale.L_BillApproveHelper_Business_35;
            }

            IsOK = CheckApproveLevel(ApproveTo, UserId, cmp, groupid, approveuser, Upri, Dep);
            if (!IsOK)
            {
                return returnMsg = @Resources.Locale.L_DNManageController_Controllers_108 + DnNo + @Resources.Locale.L_DNApproveLoop_Business_51;
            }

            //检查ApproveRecord是否有值
            ApproveHelper ap = new ApproveHelper();
            DataTable recorddt=ap.GetApproveRecordDt(DnNo,DnType,ApproveTo,approveuser);
            string sql = string.Empty;
            if (ApproveTo == "A")     //第一笔资料
            {
                returnMsg = ApporveTo_A(DnNo, UserId, Dep, userinfo, mixList, ref returnMsg, ref NextDnType, DnType, ref sql, ref noticeto, ref notice_mail);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    return returnMsg;
                }
            }
            else if (recorddt.Rows.Count > 0 && ApproveTo != "A")
            {
                if (ApproveTo.Equals("FIM"))
                {
                    ChangeTorder(mixList, DnNo, shipmentid, combineinfo);
                }
                nowLevel = recorddt.Rows[0]["APPROVE_LEVEL"].ToString();
                string NextLevel = string.Empty;
                ApproveInfo apinfo= ap.GetApproveInfo(nowLevel,DnNo,DnType);
                NextDnType = apinfo.NextDnType;
                NextLevel = apinfo.NextLevel;
                noticeto = apinfo.NoticeTo;
                notice_mail = apinfo.NoticeMail;
                EditInstruct apei = GetApproveEi(DnNo, DnType, ApproveTo, UserId,nowLevel, cmp);
                mixList.Add(apei);

                sql = GetUpActualTimeSql(DnNo, DnType, ApproveTo);
                mixList.Add(sql);
                if (string.IsNullOrEmpty(NextDnType))
                {
                    try
                    {
                        mixList.Add(UpdateDNFinish(DnNo));
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        returnMsg = DnNo + ":"+@Resources.Locale.L_BillApproveHelper_Business_38;
                        ChangeFinish(DnNo);
                    }
                    catch (Exception ex)
                    {
                        returnMsg = DnNo + ":"+@Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
                    }
                    return returnMsg;
                }

                EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                newEi.PutKey("REF_NO", DnNo);
                newEi.PutKey("APPROVE_CODE", DnType);
                newEi.PutKey("ROLE", NextDnType);
                newEi.PutKey("APPROVE_LEVEL", NextLevel);
                DateTime odt = DateTime.Now;              
                string CompanyId = cmp;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                newEi.PutDate("NOTIFY_DATE", odt);
                newEi.PutDate("NOTIFY_DATE_L", ndt);
                mixList.Add(newEi);
            }
            else if (recorddt.Rows.Count <= 0)
            {
                return returnMsg = @Resources.Locale.L_DNApproveLoop_Business_52;
            }

            DataTable nextdt=ap.GetApproveAttrDDt(NextDnType,cmp);
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
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            if ("FIM".Equals(NextDnType))
            {
                Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(UserId);
                //Business.TPV.Employe employe1 = GetSuperior(UserId);
                if (employe1 == null) return @Resources.Locale.L_DNApproveLoop_Business_53;
                notice_mail = employe1.EMail;
                noticeto = employe1.EmployeCode;

                EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                newEi.PutKey("REF_NO", DnNo);
                newEi.PutKey("APPROVE_CODE", DnType);
                newEi.PutKey("ROLE", NextDnType);
                newEi.Put("NOTICE_TO", noticeto);
                newEi.Put("NOTICE_MAIL", notice_mail);
                mixList.Add(newEi);
            }
            ei.Put("APPROVE_USER", noticeto);
            ei.Put("APPROVE_TO", NextDnType);
            mixList.Add(ei);
            
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = DnNo + ":"+@Resources.Locale.L_BillApproveHelper_Business_38;

                string subject = "DN No:" + DnNo + @Resources.Locale.L_BillApproveHelper_Business_40;
                returnMsg += AddToNoticeDN(ffid, baseStn, subject, UserId, groupid, cmp, Dep, uid, noticeto,notice_mail);
                ChangeFinish(DnNo);
            }
            catch (Exception ex)
            {
                returnMsg = DnNo + ":"+@Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
            }
            return returnMsg;

        }

        private static string NoticeToSql(string approveuser)
        {
            return string.Format(" AND NOTICE_TO={0}", SQLUtils.QuotedStr(approveuser));
        }

        private static string ApporveTo_A(string DnNo, string UserId, string Dep,UserInfo userinfo, MixedList mixList, ref string returnMsg, ref string NextDnType, string DnType, ref string sql,ref string noticeto,ref string notice_mail)
        {
            if ("SS" != Dep && "RD" != Dep && "CS" != Dep && "MP" != Dep && "MPMFG" != Dep && "QA" != Dep) return @Resources.Locale.L_DNApproveLoop_Business_54;
            sql = string.Format("SELECT * FROM APPROVE_FLOW_D WHERE {0} AND APPROVE_CODE={1} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC", APFlowDSql(userinfo), SQLUtils.QuotedStr(DnType));
            DataTable dtapprove = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("DELETE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1}", SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(DnType));
            mixList.Add(sql);
            if (dtapprove.Rows.Count <= 0)
            {
                return returnMsg = @Resources.Locale.L_BillApproveHelper_Business_42;
            }
            int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(DnNo)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;
            string approvecode=string.Empty;
            string approvename = string.Empty;
            string approvetime = string.Empty;
            for (int i = 0, j = 0; i < dtapprove.Rows.Count; i++)
            {
                EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
                string role = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["ROLE"]);
                approvecode = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["APPROVE_CODE"]);
                approvename = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["GROUP_DESCP"]);
                approvetime = Prolink.Math.GetValueAsString(dtapprove.Rows[i]["APPROVE_TIME"]);
                apei.Put("AFD_ROLE", Prolink.Math.GetValueAsString(dtapprove.Rows[i]["ROLE"]));
                apei.Put("STATUS", "0");
                apei.Put("VOID_LOOP", max);
                if (i == 0)
                {
                    apei.Put("STATUS", "1");
                    apei.Put("APPROVE_BY", UserId);
                    DateTime odt = DateTime.Now;                  
                    string CompanyId = userinfo.CompanyId;
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    apei.PutDate("APPROVE_DATE", odt);
                    apei.PutDate("NOTIFY_DATE", odt);
                    apei.PutDate("APPROVE_DATE_L", ndt);
                    apei.PutDate("NOTIFY_DATE_L", ndt);
                    apei.Put("REMARK", @Resources.Locale.L_ContractQuery_Views_478);
                    role = Dep;
                }
                if (i == 1)
                {
                    DateTime odt = DateTime.Now;                   
                    string CompanyId = userinfo.CompanyId;
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    apei.PutDate("NOTIFY_DATE", odt);
                    apei.PutDate("NOTIFY_DATE_L", ndt);
                    apei.Put("STATUS", "0");
                    if (role == "AM")
                    {
                        role = Dep + "M";
                        NextDnType = role;
                        continue;
                    }
                }
                if (role.Equals("50LEVEL"))
                {
                    //NextDnType = role;
                    continue;
                }
                j++;
                
                apei.Put("ROLE", role);
                apei.Put("REF_NO", DnNo);
                apei.Put("APPROVE_LEVEL", j);
                apei.Put("APPROVE_CODE", approvecode);
                apei.Put("APPROVE_NAME", approvename);
                apei.Put("APPROVE_TIME", approvetime);
                mixList.Add(apei);
                bool isOkTime = true;
                if (dtapprove.Rows.Count > (i + 1))
                {
                    string next_role = Prolink.Math.GetValueAsString(dtapprove.Rows[i + 1]["ROLE"]);
                    string email = string.Empty;
                    string userid = string.Empty;
                    if (next_role.Equals("AM"))
                    {
                        Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(UserId);
                        //Business.TPV.Employe employe1 = GetSuperior(UserId);
                        if (employe1 == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                        email = employe1.EMail;
                        userid = employe1.EmployeCode;
                        noticeto = userid;
                        notice_mail=email;
                        j++;
                        mixList.Add(ApproveEdit(DnNo, Dep + "M", approvecode, approvename, approvetime, j, userid, email,next_role, max, userinfo.CompanyId,isOkTime));
                        isOkTime = false;
                        if (dtapprove.Rows.Count > (i + 2))
                        {
                            next_role = Prolink.Math.GetValueAsString(dtapprove.Rows[i + 2]["ROLE"]);
                        }
                        if (next_role.Equals("50LEVEL"))
                        {
                            returnMsg = GetFiftyApprove(DnNo, UserId, Dep, mixList, approvecode, approvename, approvetime, ref j, ref userid, ref isOkTime, next_role,max, userinfo.CompanyId);
                            if (!string.IsNullOrEmpty(returnMsg)) return returnMsg;  
                        }
                    }
                    else if (next_role.Equals("50LEVEL"))
                    {
                        NextDnType = Dep + "MM";
                        returnMsg = GetFiftyApprove(DnNo, UserId, Dep, mixList, approvecode, approvename, approvetime, ref j, ref userid, ref isOkTime, next_role,max, userinfo.CompanyId);
                        if (!string.IsNullOrEmpty(returnMsg)) return returnMsg;
                    }
                }
            }
            return returnMsg = string.Empty;
        }

        private static string APFlowDSql(UserInfo userinfo)
        {
            if( userinfo==null)return string.Empty;
            return string.Format("GROUP_ID={0} AND CMP_ID = {1}",SQLUtils.QuotedStr(userinfo.GroupId),SQLUtils.QuotedStr(userinfo.CompanyId));
        }
        private static string GetFiftyApprove(string DnNo, string UserId, string Dep, MixedList mixList, string approvecode, string approvename, string approvetime, ref int j, ref string userid, ref bool isOkTime, string afdrole, int max, string CompanyId)
        {
            if (string.IsNullOrEmpty(userid)) userid = UserId;
            Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmploye(userid);
            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
            if (employe != null)
            {
                while (employe.OrgLevel < 50)
                {
                    employe = Business.TPV.Context.OrgService.GetSuperior(employe);
                    if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
                    userid = employe.EmployeCode;
                    //j++;
                    //EditInstruct levelei = ApproveEdit(DnNo, Dep + "MM", approvecode, approvename, approvetime, j, userid, employe.EMail,afdrole,max, isOkTime);
                    //isOkTime = true;
                    //mixList.Add(levelei);
                    //if (j > 20)
                        //return "抓取50层级信息错误";
                }
                j++;
                EditInstruct levelei = ApproveEdit(DnNo, Dep + "MM", approvecode, approvename, approvetime, j, userid, employe.EMail, afdrole, max, CompanyId, isOkTime);
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
            levelei.Put("ROLE",role );//Dep + "MM"
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

        public static string ApproveBackY(string uid, string dnno, string approvecode, string baseStn, string groupid, string cmp, string Userid, string Dep)
        {
            string returnvalue = string.Empty;
            string sql = string.Format("SELECT * FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND STATUS='1'", SQLUtils.QuotedStr(dnno), SQLUtils.QuotedStr(approvecode));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string approverole = "";
            IMailTemplateParse parse = new DefaultMailParse();
            DataTable maindt = DNInfoCheck.GetDNDataByDnNo(dnno);
            foreach (DataRow dr in dt.Rows)
            {
                approverole = Prolink.Math.GetValueAsString(dr["ROLE"]);
                sql = string.Format(@" SELECT * FROM APPROVE_FLOW_D WHERE BACK_FLAG='Y' AND APPROVE_CODE={0} AND ROLE={1}", SQLUtils.QuotedStr(approvecode), SQLUtils.QuotedStr(approverole));
                DataTable dtd = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dtd.Rows.Count == 0)
                {
                    continue;
                }
                string uuid = Prolink.Math.GetValueAsString(dtd.Rows[0]["GU_ID"]);
                try
                {
                    string subject = "DN No:" + dnno + @Resources.Locale.L_GateReserveSetup_Scripts_276;
                    returnvalue += AddToNoticeDN(uuid, baseStn, subject, Userid, groupid, cmp, Dep, uid);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return returnvalue;
        }

        public static EditInstruct GetApproveEi(string dnno, string dntype, string Approveto, string UserId, string NextLevel, string CompanyId)
        {
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", dnno);
            apei.PutKey("APPROVE_CODE", dntype);
            apei.PutKey("ROLE", Approveto);
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

        public static string GetUpActualTimeSql(string dnno, string dntype, string Approveto)
        {
            return string.Format("update APPROVE_RECORD SET ACTUAL_TIME=DateDiff(mi,NOTIFY_DATE ,APPROVE_DATE) WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}", SQLUtils.QuotedStr(dnno), SQLUtils.QuotedStr(dntype), SQLUtils.QuotedStr(Approveto));
        }

        public static bool CheckLastOK(string dnno, string dntype, string role,string noticeto)
        {
            string sql = string.Format("SELECT STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(dnno), SQLUtils.QuotedStr(dntype), SQLUtils.QuotedStr(role));
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
        public static string AddToNoticeDN(string uuid, string baseCondition, string subject, string userId, string GroupId, string CompanyId, string Dep, string dnUid,string noticeUser=null,string notice_mail=null)
        {
            string msg = string.Empty;
            string mailType = "DN_AP";
            if (!string.IsNullOrEmpty(notice_mail))
            {
                EvenFactory.AddEven(mailType + "#" + dnUid + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), dnUid, MailManager.DNNotify, null, 1, 0, notice_mail, subject, "", 3, 0);
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

            #region 处理Mail发送的
            DataRow[] mailrows = maindt.Select("BY_EMAIL='Y'");
            StringBuilder sb = new StringBuilder();
            string mail = "";
            for (int i = 0; i < mailrows.Length; i++)
            {
                mail = Prolink.Math.GetValueAsString(mailrows[i]["U_EMAIL"]);
                if (string.IsNullOrEmpty(mail))
                {
                    continue;
                }
                sb.Append(mail);
                sb.Append(";");
            }
            mail = sb.ToString();
            if (!string.IsNullOrEmpty(mail))
            {
                EvenFactory.AddEven(mailType + "#" + dnUid + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), dnUid, MailManager.DNNotify, null, 1, 0, mail, subject, "", 3, 0);
            }
            #endregion

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
                md.Title = "DN Message";
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

        public static void ChangeTorder(MixedList ml, string dnno, string shipmentid, string combineinfo)
        {
            EditInstruct ei = Business.TPV.Helper.ChangeTorder(dnno, shipmentid, combineinfo);
            if (ei == null) return;
            ml.Add(ei);
        }

        public static void ChangeFinish(string dnno)
        {
            string sql = string.Format(@"SELECT DISTINCT AR.STATUS FROM SMDN DN,APPROVE_RECORD AR WHERE
                DN.APPROVE_TYPE=AR.APPROVE_CODE AND DN.DN_NO=AR.REF_NO AND DN_NO={0}", SQLUtils.QuotedStr(dnno));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow[] drs = dt.Select("STATUS='0'");
                if(drs.Length<=0){
                    MixedList mxlist = new MixedList();
                    mxlist.Add(UpdateDNFinish(dnno));
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

        public static EditInstruct UpdateDNFinish(string dnno)
        {
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("DN_NO", dnno);
            ei.Put("APPROVE_TO", "Finish");
            ei.Put("APPROVE_USER", "");
            return ei;
        }

        /// <summary>
        /// 更新签核明细
        /// </summary>
        /// <param name="dnno">Dn No</param>
        /// <param name="cmp">公司别：cmp...</param>
        /// <param name="ApproveRole">签核对象</param>
        /// <param name="UserId">签核人员</param>
        /// <returns>返回签核信息</returns>
        public static string UpdateApproveRecord(string dnno, string cmp, string ApproveRole, string UserId,string bscondition)
        {
            string returnMsg = "success";
            string sql = "SELECT * FROM SMDN WHERE DN_NO = " + SQLUtils.QuotedStr(dnno) + " AND CMP=" + SQLUtils.QuotedStr(cmp);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count<=0) return "DN NO:" + dnno + @Resources.Locale.L_DNApproveLoop_Business_55;
            DataRow dr =dt.Rows[0];
            string DnNo = dr["DN_NO"].ToString();
            string DnType = dr["APPROVE_TYPE"].ToString();
            string ApproveTo = dr["APPROVE_TO"].ToString();
            if ("Finish".Equals(ApproveTo))
                return returnMsg;
            string approveuser = dr["APPROVE_USER"].ToString();
            if (!string.IsNullOrEmpty(ApproveRole))
            {
                ApproveTo = ApproveRole;
                approveuser = string.Empty;
            }
            string groupid = dr["GROUP_ID"].ToString();
            string uid = dr["U_ID"].ToString();
            string dep = dr["DEP"].ToString();
            string nowLevel=string.Empty;

            MixedList mixList = new MixedList();
            if (DnType != "")
            {
                ApproveHelper ap = new ApproveHelper();
                DataTable approrecord=ap.GetApproveRecordDt(DnNo, DnType, ApproveTo, approveuser);
                if(approrecord.Rows.Count>0)
                    nowLevel=approrecord.Rows[0]["APPROVE_LEVEL"].ToString();
                ApproveInfo apinfo = ap.GetApproveInfo(nowLevel, DnNo, DnType);

                EditInstruct apei = GetApproveEi(DnNo, DnType, ApproveTo, UserId,nowLevel, cmp);
                mixList.Add(apei);

                sql = GetUpActualTimeSql(DnNo, DnType, ApproveTo);
                mixList.Add(sql);
                if (string.IsNullOrEmpty(apinfo.NextDnType))
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //returnMsg = DnNo + ":签核成功";
                        ChangeFinish(DnNo);
                    }
                    catch (Exception ex)
                    {
                        returnMsg = DnNo + ":"+@Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
                    }
                    return returnMsg;
                }

                EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                newEi.PutKey("REF_NO", DnNo);
                newEi.PutKey("APPROVE_CODE", DnType);
                newEi.PutKey("ROLE", apinfo.NextDnType);
                newEi.PutKey("APPROVE_LEVEL", apinfo.NextLevel);
                DateTime odt = DateTime.Now;               
                string CompanyId = cmp;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                newEi.PutDate("NOTIFY_DATE", odt);
                newEi.PutDate("NOTIFY_DATE_L", ndt);
                mixList.Add(newEi);

                string uuid = ap.GetApproveAttrDUid(apinfo.NextDnType,cmp);
                if (string.IsNullOrEmpty(uuid) && !string.IsNullOrEmpty(apinfo.NextDnType))
                {
                    if (string.IsNullOrEmpty(apinfo.NoticeMail))
                    {
                        return returnMsg = @Resources.Locale.L_BaseApprove_Business_20 + apinfo.NextDnType + @Resources.Locale.L_BaseApprove_Business_21;
                    }
                }
              
                //更新SMDN的状态
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("DN_NO", DnNo);
                if ("FIM".Equals(apinfo.NextDnType))
                {
                    Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(UserId);
                    //Business.TPV.Employe employe1 = GetSuperior(UserId);
                    if (employe1 == null) return @Resources.Locale.L_DNApproveLoop_Business_53;
                    apinfo.NoticeMail = employe1.EMail;
                    apinfo.NoticeTo = employe1.EmployeCode;

                    EditInstruct arei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                    arei.PutKey("REF_NO", DnNo);
                    arei.PutKey("APPROVE_CODE", DnType);
                    arei.PutKey("ROLE", apinfo.NextDnType);
                    arei.Put("NOTICE_TO", apinfo.NoticeTo);
                    arei.Put("NOTICE_MAIL", apinfo.NoticeMail);
                    mixList.Add(arei);
                }
                ei.Put("APPROVE_USER", apinfo.NoticeTo);
                ei.Put("APPROVE_TO", apinfo.NextDnType);
                mixList.Add(ei);

                sql = GetUpActualTimeSql(dnno, DnType, ApproveRole);
                mixList.Add(sql);

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string subject = "DN No:" + DnNo + @Resources.Locale.L_BillApproveHelper_Business_40;

                    AddToNoticeDN(uuid, bscondition, subject, UserId, groupid, cmp, dep, uid, apinfo.NoticeTo, apinfo.NoticeMail);
                    ChangeFinish(DnNo);
                }
                catch (Exception ex)
                {
                    returnMsg = "DN NO:" + dnno + @Resources.Locale.L_DNApproveLoop_Business_56 + ex.ToString();
                }
            }
            else {
                returnMsg = "DN NO:" + dnno + @Resources.Locale.L_DNApproveLoop_Business_55 ;
            }

            return returnMsg;
        }

        public static string UpdateApproveRecordToFpw(string dnno, string cmp, string ApproveRole, string UserId)
        {
            string returnMsg = "success";
            string sql = "SELECT APPROVE_TYPE FROM SMDN WHERE DN_NO = " + SQLUtils.QuotedStr(dnno) + " AND CMP=" +SQLUtils.QuotedStr(cmp);
            string ApproveType = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList mixList = new MixedList();
            if (ApproveType != "")
            {
                EditInstruct apei = GetApproveEi(dnno, ApproveType, ApproveRole, UserId, null, cmp);
                mixList.Add(apei);

                sql = GetUpActualTimeSql(dnno, ApproveType, ApproveRole);
                mixList.Add(sql);

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ChangeFinish(dnno);
                }
                catch (Exception ex)
                {
                    returnMsg = "DN NO:" + dnno + @Resources.Locale.L_DNApproveLoop_Business_56 + ex.ToString();
                }
            }
            else {
                returnMsg = "DN NO:" + dnno + @Resources.Locale.L_DNApproveLoop_Business_55;
            }
            return returnMsg;
        }

        public static string DnApproveBack(string uid, string backremark, UserInfo userinfo)
        {
            string returnMsg = "";
            DataTable nowdata = DNInfoCheck.GetDNDataByUId(uid);
            if(nowdata.Rows.Count<=0) return "Dn No"+@Resources.Locale.L_DNManageController_Controllers_105;

            string aptype = nowdata.Rows[0]["APPROVE_TYPE"].ToString();
            string DnNo = nowdata.Rows[0]["DN_NO"].ToString();
            string ApproveTo = nowdata.Rows[0]["APPROVE_TO"].ToString();
            string noticeuser = string.Empty;
            string noticemail = string.Empty;
            MixedList mixList = new MixedList();

            string sql=string.Format(@"SELECT AFD_ROLE FROM APPROVE_RECORD WHERE REF_NO={0} 
                    AND APPROVE_CODE={1} AND ROLE={2} ",SQLUtils.QuotedStr(DnNo),
                    SQLUtils.QuotedStr(aptype), SQLUtils.QuotedStr(ApproveTo));
            string afdrole = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if ("A".Equals(afdrole))    //针对退回申请者状态
            {
                mixList.Add(UpApproveStatus(uid, afdrole));
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                return @Resources.Locale.L_BillApproveHelper_Business_25;
            }

            afdrole = GetReturnRole(mixList, DnNo, aptype, afdrole, userinfo);
            if (string.IsNullOrEmpty(afdrole)) return @Resources.Locale.L_DNApproveLoop_Business_57;
            
            sql = string.Format(@"SELECT TOP 1 * FROM APPROVE_RECORD WHERE REF_NO={0}
  AND APPROVE_CODE={1} AND AFD_ROLE={2} AND STATUS='1' ORDER BY CAST(APPROVE_LEVEL AS INT) DESC",
                    SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(aptype), SQLUtils.QuotedStr(afdrole));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count == 0) return @Resources.Locale.L_BillApproveHelper_Business_27;
            string NextDnType = Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);  
            noticeuser = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_BY"]);   //抓取签核过的人
            //noticemail = Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);
            sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(noticeuser));
            noticemail = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (string.IsNullOrEmpty(noticemail)) return @Resources.Locale.L_BillApproveHelper_Business_28;

            string uuid = string.Empty;
            //更新SMDN的状态
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", NextDnType);
            ei.Put("APPROVE_USER", noticeuser); //更新此笔签核人员到上一层
            mixList.Add(ei);

            //写入笔资料到签核明细档
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", DnNo);
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
            backremark=@Resources.Locale.L_BillApproveHelper_Business_29+userinfo.UserId+@Resources.Locale.L_BillApproveHelper_Business_30+backremark;
            apei.Put("REMARK", backremark);        
            mixList.Add(apei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = @Resources.Locale.L_BillApproveHelper_Business_31;
                string subject = "DN No:" + DnNo + @Resources.Locale.L_BillApproveHelper_Business_32 + backremark;
                returnMsg += DNApproveLoop.AddToNoticeDN(uuid, userinfo.basecondtions, subject, userinfo.UserId, userinfo.GroupId, userinfo.CompanyId, userinfo.Dep, uid, noticeuser, noticemail);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }

        public static EditInstruct UpApproveStatus(string uid, string NextDnType, string noticeuser="")
        {
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("APPROVE_TO", "A");
            ei.Put("APPROVE_USER", noticeuser); //更新此笔签核人员到上一层
            ei.Put("APPROVE_BACK", "N");
            return ei;
        }

        public static string GetReturnRole(MixedList mixList,string dnno, string apcode,string afdrole,UserInfo userinfo){
            string sql = string.Format(@"SELECT * FROM APPROVE_FLOW_D WHERE {0} AND APPROVE_CODE={1} AND 
                CAST(APPROVE_LEVEL AS INT)<(SELECT TOP 1 CAST(APPROVE_LEVEL AS INT) FROM APPROVE_FLOW_D WHERE {0} AND APPROVE_CODE={1} AND ROLE={2})
                ORDER BY CAST(APPROVE_LEVEL AS INT) DESC",
              APFlowDSql(userinfo), SQLUtils.QuotedStr(apcode), SQLUtils.QuotedStr(afdrole));
            DataTable dt=OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return string.Empty;
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", dnno);
            apei.PutKey("APPROVE_CODE", apcode);
            apei.PutKey("AFD_ROLE", afdrole);
            apei.Put("STATUS", "0");
            mixList.Add(apei);
            string looprole= Prolink.Math.GetValueAsString(dt.Rows[0]["ROLE"]);
            foreach(DataRow dr in dt.Rows){
                if ("Y".Equals(Prolink.Math.GetValueAsString(dr["BACK_FLAG"])))
                {
                    looprole = Prolink.Math.GetValueAsString(dr["ROLE"]);
                    break;
                }
                else
                {
                    EditInstruct ei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("REF_NO", dnno);
                    ei.PutKey("APPROVE_CODE", apcode);
                    ei.PutKey("AFD_ROLE", Prolink.Math.GetValueAsString(dr["ROLE"]));
                    ei.Put("STATUS", "0");
                    mixList.Add(ei);
                }
            }
            return looprole;
        }
        
    }

    public class ApproveHelper
    {
        /// <summary>
        /// 抓取下一位签核人
        /// </summary>
        /// <param name="nowLevel">现在签核层级</param>
        /// <param name="DnNo">DN No</param>
        /// <param name="DnType">签核类别</param>
        /// <returns>ApproveInfo 信息</returns>
        public ApproveInfo GetApproveInfo(string nowLevel, string DnNo, string DnType)
        {
            ApproveInfo ai = new ApproveInfo();
            string alsql = string.Format("SELECT TOP 1 ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL FROM APPROVE_RECORD WHERE CAST(APPROVE_LEVEL AS INT)>{0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC",
                SQLUtils.QuotedStr(nowLevel),
                SQLUtils.QuotedStr(DnNo),
                SQLUtils.QuotedStr(DnType));
            DataTable appdt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string NextLevel = string.Empty;
            if (appdt.Rows.Count <= 0)
            {
                ai.NextDnType = string.Empty;
            }
            else
            {
                ai.NextDnType = Prolink.Math.GetValueAsString(appdt.Rows[0]["ROLE"]);
                ai.NextLevel = Prolink.Math.GetValueAsString(appdt.Rows[0]["APPROVE_LEVEL"]);
                ai.NoticeTo = Prolink.Math.GetValueAsString(appdt.Rows[0]["NOTICE_TO"]);
                ai.NoticeMail = Prolink.Math.GetValueAsString(appdt.Rows[0]["NOTICE_MAIL"]);
            }
            return ai;
        }

        /// <summary>
        /// 抓取签核明细内容
        /// </summary>
        /// <param name="DnNo">DN No</param>
        /// <param name="DnType">签核类别</param>
        /// <param name="ApproveTo">签核状态</param>
        /// <param name="approveuser">待签核人</param>
        /// <returns>返回当前签核信息</returns>
        public DataTable GetApproveRecordDt(string DnNo, string DnType, string ApproveTo, string approveuser)
        {
            string alsql = string.Format("SELECT APPROVE_LEVEL FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(DnNo),
                SQLUtils.QuotedStr(DnType),
                SQLUtils.QuotedStr(ApproveTo));
            if (!string.IsNullOrEmpty(approveuser))
            {
                alsql += string.Format(" AND NOTICE_TO={0}", SQLUtils.QuotedStr(approveuser));
            }
            DataTable recorddt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return recorddt;
        }

        /// <summary>
        /// 抓取DN签核部门信息
        /// </summary>
        /// <param name="NextDnType">签核对象</param>
        /// <returns></returns>
        public DataTable GetApproveAttrDDt(string NextDnType,string company)
        {
            string sql = string.Format(@"SELECT APPROVE_GROUP,U_ID FROM APPROVE_ATTR_D WHERE U_FID=(
                SELECT TOP 1 U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR='DN' AND CMP={0}) AND APPROVE_GROUP={1}", SQLUtils.QuotedStr(company), SQLUtils.QuotedStr(NextDnType));

            DataTable nextdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return nextdt;
        }

        /// <summary>
        /// 抓取DN签核部门Uid信息
        /// </summary>
        /// <param name="NextDnType"></param>
        /// <returns></returns>
        public string GetApproveAttrDUid(string NextDnType,string company)
        {
            string uid = string.Empty;
            DataTable nextdt = GetApproveAttrDDt(NextDnType, company);
            if (nextdt.Rows.Count > 0)
                uid = nextdt.Rows[0]["U_ID"].ToString();
            return uid;
        }
    }

    public class ApproveInfo{
        /// <summary>
        /// 下一个签核类别
        /// </summary>
        public string NextDnType { get; set; } 
        /// <summary>
        /// 下一个签核层级
        /// </summary>
        public string NextLevel { get; set; } 
        /// <summary>
        /// 下一位签核人
        /// </summary>
        public string NoticeTo { get; set; } 
        /// <summary>
        /// 下一位签核人Mail
        /// </summary>
        public string NoticeMail { get; set; } 
    }

    public class UserInfo: BaseUserInfo
    {
        public string Upri { get; set; }
        public string Dep { get; set; }
        public string DataCmp { get; set; }
        public string basecondtions { get; set; }

        public string IoFlag { get; set; }
    }

}