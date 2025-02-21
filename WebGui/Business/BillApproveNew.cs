using Business.TPV;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackingEDI;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;
using TrackingEDI.Mail;
using WebGui.Models;
using WebGui.UICMessageSrv;

namespace Business
{
    public class BillApproveNew
    {
        private readonly UserInfo userinfo;
        private readonly string billUid;
        private readonly string tvMnt;
        private readonly string approveType;
        private readonly string approveTo;
        private readonly string groupid;
        private readonly string cmp;
        private readonly string approveUser;
        private MixedList mixlist;
        private readonly decimal amt;
        private readonly string cur;
        private readonly string debitNo;
        private readonly string tpvDebitNo;

        public BillApproveNew(UserInfo userinfo, string billuid, string tvMnt)
        {
            this.billUid = billuid;
            this.userinfo = userinfo;
            this.mixlist = new MixedList();

            DataTable nowdata = Business.BillApproveHelper.GetDataByUId(billUid);
            string approveto = Prolink.Math.GetValueAsString(nowdata.Rows[0]["APPROVE_TO"]);
            string approvetype = Prolink.Math.GetValueAsString(nowdata.Rows[0]["APPROVE_TYPE"]);
            this.groupid = Prolink.Math.GetValueAsString(nowdata.Rows[0]["GROUP_ID"]);
            this.cmp = Prolink.Math.GetValueAsString(nowdata.Rows[0]["CMP"]);
            this.amt = Prolink.Math.GetValueAsDecimal(nowdata.Rows[0]["AMT"]);
            this.cur = Prolink.Math.GetValueAsString(nowdata.Rows[0]["CUR"]);
            this.approveUser = Prolink.Math.GetValueAsString(nowdata.Rows[0]["APPROVE_USER"]);
            this.debitNo = Prolink.Math.GetValueAsString(nowdata.Rows[0]["DEBIT_NO"]);
            this.tpvDebitNo = Prolink.Math.GetValueAsString(nowdata.Rows[0]["TPV_DEBIT_NO"]);
            if ("A".Equals(approveto))
            {
                this.approveType = ChangeApproveCode(approvetype);
                this.tvMnt = ChangeTvMnt(tvMnt);
            }
            else
            {
                this.approveType = approvetype;
                this.tvMnt = tvMnt;
            }
            this.approveTo = approveto;


        }

        private bool CheckLastOK()
        {
            string sql = string.Format("SELECT STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveType), SQLUtils.QuotedStr(approveTo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return false;
            if (dt.Rows.Count == 1 && !string.IsNullOrEmpty(approveUser))
            {
                sql += NoticeToSql(approveUser);
            }
            else if (!string.IsNullOrEmpty(userinfo.UserId))
            {
                sql += NoticeToSql(userinfo.UserId);
            }
            string value = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (value.Equals("1"))
            {
                return true;
            }
            return false;
        }

        private string ChangeTvMnt(string tvmnt)
        {
            string sql = string.Format("SELECT DISTINCT TV_MNT FROM APPROVE_FLOW_M  WHERE APPROVE_CODE={0} AND CMP_ID={1}",
                SQLUtils.QuotedStr(approveType), SQLUtils.QuotedStr(userinfo.DataCmp));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> tvmntList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                tvmntList.Add(dr["TV_MNT"].ToString());
            }
            if (!tvmntList.Contains(tvmnt))
            {
                if (tvmntList.Contains("ALL"))
                    return "ALL";
            }
            return tvmnt;
        }

        private string ChangeApproveCode(string approvecode)
        {
            decimal usdAmt = 0;
            if (!"USD".Equals(cur))
            {
                string rateFilter = string.Format("EDATE<={0}", SQLUtils.QuotedStr(TimeZoneHelper.GetTimeZoneDate(userinfo.CompanyId).ToString("yyyy-MM-dd")));
                DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND {0} ORDER BY EDATE",
                    rateFilter), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                bool error = false;
                List<string> msg = new List<string>();
                Business.TPV.Financial.Helper.GetTotal(rateDt, msg, amt, cur, ref usdAmt, ref error, "USD");
                //if (error) return approvecode;
            }
            else
            { usdAmt = amt; }

            string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE,AP_CD,AR_CD FROM BSCODE WHERE GROUP_ID={0} AND(CMP={1} OR CMP='*') AND CONVERT(float,CD)<" + usdAmt + " AND CD_TYPE ='ATST' ORDER BY ORDER_BY DESC", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.CompanyId));
            DataTable approvedt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (approvedt.Rows.Count > 0)
            {
                return Prolink.Math.GetValueAsString(approvedt.Rows[0]["AP_CD"]);
            }
            return approvecode;
        }


        public string ApproveBillItem(string ApproveRemark = "")
        {
            string returnMsg = string.Empty;
            string NextDnType = string.Empty;
            string ffid = "";
            string noticeto = string.Empty;
            string notice_mail = string.Empty;

            if (CheckLastOK())
            {
                return @Resources.Locale.L_BillApproveHelper_Business_34 + debitNo + @Resources.Locale.L_BillApproveHelper_Business_35;
            }
            string alsql = string.Format("SELECT APPROVE_LEVEL,SAP_LEVEL,APPROVE_NAME,ROLE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(billUid),
                SQLUtils.QuotedStr(approveType),
                SQLUtils.QuotedStr(approveTo));
            if (!string.IsNullOrEmpty(approveUser))
            {
                alsql += NoticeToSql(approveUser);
            }
            DataTable recorddt = OperationUtils.GetDataTable(alsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> SeniorStaffList = GetSeniorStaff(approveType);
            List<string> agentList = new List<string>();
            if (recorddt.Rows.Count > 0)
            {
                agentList = getAgentList(userinfo.UserId, Prolink.Math.GetValueAsInt(recorddt.Rows[0]["SAP_LEVEL"]), Prolink.Math.GetValueAsString(recorddt.Rows[0]["APPROVE_NAME"]), Prolink.Math.GetValueAsString(recorddt.Rows[0]["ROLE"]), SeniorStaffList);
            }
            bool IsOK = CheckApproveLevel(approveUser, agentList);
            if (!IsOK)
            {
                return string.Format(@Resources.Locale.L_BillApproveHelper_Business_36, debitNo);
            }
            string sql = string.Empty;
            string NextLevel = string.Empty;
            int saplevel = 0;
            bool noticemailonce = true;
            if (approveTo == "A")     //第一笔资料
            {
                returnMsg = createApproveRecord(ref NextDnType, ref noticeto, ref notice_mail, ApproveRemark);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    return returnMsg;
                }
                if (string.IsNullOrEmpty(NextDnType)) NextDnType = "Finish";
                NextLevel = "2";
            }
            #region 已有簽核資料
            else if (recorddt.Rows.Count <= 0)
            {
                return string.Format(@Resources.Locale.L_BillApproveHelper_Business_36, debitNo);
            }
            else if (recorddt.Rows.Count > 0 && approveTo != "A")
            {
                string nowLevel = recorddt.Rows[0]["APPROVE_LEVEL"].ToString();

                sql = "SELECT COUNT(1) FROM APPROVE_RECORD WHERE CAST(APPROVE_LEVEL AS INT)={0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2}";
                sql = string.Format(sql, SQLUtils.QuotedStr(nowLevel), SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveType));
                int nowrecordcount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
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
                alsql = string.Format(sql, SQLUtils.QuotedStr(nowLevel), SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveType));
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

                EditInstruct apei = GetApproveEi(billUid, approveTo, userinfo.UserId, userinfo.CompanyId, nowLevel, ApproveRemark, agentList);
                mixlist.Add(apei);

                sql = GetUpActualTimeSql(billUid, approveTo);
                mixlist.Add(sql);
                if (string.IsNullOrEmpty(NextDnType))
                {
                    try
                    {
                        UICHandle uic = new UICHandle(billUid, NextLevel, approveType, userinfo);
                        UICReturnMSG msg = uic.SendUICMessage("UICClose");
                        if (msg.type.ToLower() == "fail" || (msg.type.ToLower() == "error" && msg.code != "400"))
                        {
                            return "UIC Close Fail:" + msg.message;
                        }
                        int[] result = OperationUtils.ExecuteUpdate(mixlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                        ChangeFinish();
                        returnMsg = "success";
                    }
                    catch (Exception ex)
                    {
                        returnMsg = debitNo + ":" + @Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
                    }
                    return returnMsg;
                }
                if (noticemailonce)
                {
                    EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                    newEi.PutKey("REF_NO", billUid);
                    newEi.PutKey("APPROVE_CODE", approveType);
                    newEi.PutKey("ROLE", NextDnType);
                    newEi.PutKey("APPROVE_LEVEL", NextLevel);
                    newEi.PutDate("NOTIFY_DATE", DateTime.Now);
                    newEi.PutDate("NOTIFY_DATE_L", TimeZoneHelper.GetTimeZoneDate(userinfo.CompanyId));
                    mixlist.Add(newEi);
                }
            }
            #endregion

            sql = "SELECT APPROVE_GROUP,U_ID FROM APPROVE_ATTR_D WHERE U_FID=(SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='PL' AND APPROVE_ATTR='BILLING') AND APPROVE_GROUP={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(NextDnType));
            DataTable nextdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (nextdt.Rows.Count <= 0 && !string.IsNullOrEmpty(NextDnType))
            {
                if (string.IsNullOrEmpty(noticeto))
                {
                    return @Resources.Locale.L_BaseApprove_Business_20 + NextDnType + @Resources.Locale.L_BaseApprove_Business_21;
                }
            }
            else
            {
                ffid = Prolink.Math.GetValueAsString(nextdt.Rows[0]["U_ID"]);
            }

            //更新SMDN的状态
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", billUid);
            ei.Put("APPROVE_USER", noticeto);
            ei.Put("APPROVE_TO", NextDnType);
            SetApprove(ei, userinfo);
            mixlist.Add(ei);
            if ("AC".Equals(NextDnType))
            {
                sql = string.Format("SELECT * FROM FSSP_TASK WHERE U_ID={0}", SQLUtils.QuotedStr(billUid));
                DataTable taskTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                EditInstruct taskei = new EditInstruct("FSSP_TASK", EditInstruct.INSERT_OPERATION);
                taskei.Put("U_ID", billUid);
                if (taskTable.Rows.Count > 0)
                {
                    taskei = new EditInstruct("FSSP_TASK", EditInstruct.UPDATE_OPERATION);
                    taskei.PutKey("U_ID", billUid);
                }
                taskei.Put("MAX_COUNT", 3);
                taskei.PutDate("CREATE_DATE", DateTime.Now);
                taskei.Put("ERROR_COUNT", 0);
                taskei.Put("EXEC_TYPE", "N");
                taskei.Put("REF_NO", tpvDebitNo);
                taskei.Put("CMP", cmp);
                if (TrackingEDI.Manager.CheckFSSPSite(cmp))
                    mixlist.Add(taskei);
            }

            IMailTemplateParse parse = new DefaultMailParse();
            try
            {
                
                if (SeniorStaffList.Contains(NextDnType) && noticemailonce)
                {
                    UICHandle uic = new UICHandle(billUid, NextLevel, approveType, userinfo);
                    UICReturnMSG msg = uic.SendUICMessage("UICClose");
                    if (msg.type.ToLower() == "success" || msg.code == "400")
                        msg = uic.SendUICMessage("UICCreate");
                    if (msg.type.ToLower() == "fail")
                    {
                        return "Send UIC Fail:" + msg.message;
                    }
                }
                int[] result = OperationUtils.ExecuteUpdate(mixlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = debitNo + ":" + @Resources.Locale.L_BillApproveHelper_Business_38;
                if (!SeniorStaffList.Contains(NextDnType) && noticemailonce)
                {
                    string subject = @Resources.Locale.L_BillApproveHelper_Business_39 + debitNo + @Resources.Locale.L_BillApproveHelper_Business_40;
                    returnMsg += AddToNoticeBill(ffid, userinfo.basecondtions, subject, userinfo.UserId, groupid, cmp, userinfo.Dep, billUid, noticeto, notice_mail);

                    sql = string.Format(@"SELECT ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL,SAP_LEVEL FROM APPROVE_RECORD 
                            WHERE APPROVE_LEVEL={0} AND STATUS='0' AND REF_NO={1} AND APPROVE_CODE={2} AND NOTICE_TO !={3}",
                        SQLUtils.QuotedStr(NextLevel), SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveType), SQLUtils.QuotedStr(noticeto));
                    DataTable appdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow dr in appdt.Rows)
                    {
                        noticeto = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                        notice_mail = Prolink.Math.GetValueAsString(dr["NOTICE_MAIL"]);
                        returnMsg += AddToNoticeBill(ffid, userinfo.basecondtions, subject, noticeto, groupid, cmp, userinfo.Dep, billUid, noticeto, notice_mail);
                    }
                }

                sql = string.Format(@"SELECT D.QUOT_NO,D.DEBIT_NO,D.SHIPMENT_ID,D.DEC_NO,M.APPROVE_TO,
M.VERIFY_DATE,D.QAMT,D.QCUR,D.BAMT,D.CUR,D.CHG_CD FROM SMBID D LEFT JOIN SMBIM M ON D.U_FID=M.U_ID 
WHERE M.U_ID={0} AND D.CHG_CD IN ('DSTF','DDET','DDEM')", SQLUtils.QuotedStr(billUid));
                DataTable bidDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                InboundTransfer.UpdateBillInfoToSMORDByBID(bidDt); 

                if ("AC".Equals(NextDnType))
                {
                    SplitSmbidHelper splitSmbidHelper = new SplitSmbidHelper(billUid);
                    splitSmbidHelper.DoSplitSMBID();
                    if (TrackingEDI.Manager.CheckFSSPSite(cmp))
                        WebApiEdiHandle.SendSplitSMBIDEdi(tpvDebitNo, userinfo.UserId, cmp, billUid);
                }
            }
            catch (Exception ex)
            {
                returnMsg = debitNo + ":" + @Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
            }
            return returnMsg;

        }

        public string AddToNoticeBill(string uuid, string baseCondition, string subject, string userId, string GroupId, string CompanyId, string Dep, string billUid, string noticeUser = null, string notice_mail = null)
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

        public EditInstruct AddMessage(MessageData md, string UserId, string receiveUser, string GroupId, string CompanyId, string Dep)
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
            ei.PutDate("CREATE_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmm"));
            ei.PutClob("CONTENT", md.ToXml());
            return ei;
        }



        public EditInstruct GetApproveEi(string debitno, string Approveto, string UserId, string CompanyId, string NextLevel, string ApproveRemark, List<string> agentList)
        {
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", debitno);
            apei.PutKey("APPROVE_CODE", approveType);
            //apei.PutKey("APPROVE_LEVEL", Approveto);
            if (!string.IsNullOrEmpty(NextLevel))
            {
                apei.PutKey("APPROVE_LEVEL", NextLevel);
            }
            apei.Put("CREATE_BY", UserId);
            apei.PutDate("CREATE_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId));
            apei.Put("APPROVE_BY", UserId);
            apei.PutDate("APPROVE_DATE", DateTime.Now);
            apei.PutDate("APPROVE_DATE_L", TimeZoneHelper.GetTimeZoneDate(CompanyId));
            apei.Put("STATUS", "1");
            apei.Put("REMARK", @Resources.Locale.L_Pass);
            apei.Put("APPROVE_REMARK", ApproveRemark);

            string sql = string.Format("SELECT UIC_GUID,NOTICE_TO,STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL={2} ",
                SQLUtils.QuotedStr(debitno), SQLUtils.QuotedStr(approveType), SQLUtils.QuotedStr(NextLevel));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
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
                    if (UserId == noticeto || agentList.Contains(noticeto.ToLower()))
                    {
                        apei.PutKey("NOTICE_TO", noticeto);
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


        public string GetUpActualTimeSql(string debitno, string Approveto)
        {
            string sql = "update APPROVE_RECORD SET ACTUAL_TIME=DateDiff(mi,NOTIFY_DATE ,APPROVE_DATE) WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(debitno), SQLUtils.QuotedStr(approveType), SQLUtils.QuotedStr(Approveto));
            return sql;
        }

        private void SendUICMessage(string uid, string noticeto, string notice_mail, string NextLevel, int[] result, string cmp, UserInfo userinfo, string debitno)
        {
            try
            {
                string sql = string.Format(@"SELECT U_EMAIL,CMP FROM SYS_ACCT WHERE U_ID = (SELECT APPROVE_BY FROM APPROVE_RECORD WHERE REF_NO ={0}
                    AND APPROVE_CODE ={1} AND APPROVE_LEVEL = '1') AND CMP = {2}", SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveType), SQLUtils.QuotedStr(cmp));
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
                string href = Prolink.Web.WebContext.GetInstance().GetProperty("BillManageUrl");
                href = string.Format(href + "&mCmp={0}&mUId={1}", cmp, noticeto);
                if (string.IsNullOrEmpty(cmp))
                    href = string.Format(href + "&mUId={1}", noticeto);
                msg.Content = href;
                msg.URL = href;
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
                    catch (Exception ex)
                    {
                        OperationUtils.Logger.WriteLog("uic message send excption:" + ex.Message);
                    }

                    if (resultds != null)
                    {
                        EditInstruct arei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
                        arei.PutKey("REF_NO", uid);
                        arei.PutKey("APPROVE_CODE", approveType);
                        arei.PutKey("NOTICE_TO", noticeto);
                        arei.PutKey("APPROVE_LEVEL", NextLevel);
                        arei.Put("UIC_GUID", uicguid);
                        arei.Put("UIC_STATUS", "SEND" + resultds.StatusCode);
                        result = OperationUtils.ExecuteUpdate(arei, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
            }
            catch (Exception ex)
            {
                OperationUtils.Logger.WriteLog("SendUICMessage excption:" + ex.Message);
            }
        }


        private void ChangeFinish()
        {
            string sql = string.Format(@"SELECT TOP 1 STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} ORDER BY APPROVE_LEVEL DESC",
                SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveType));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow[] drs = dt.Select("STATUS='0'");
                if (drs.Length <= 0)
                {
                    MixedList mxlist = new MixedList();
                    EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", billUid);
                    ei.Put("APPROVE_TO", "Finish");
                    SetApprove(ei, userinfo);
                    mxlist.Add(ei);
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

        public static void SetApprove(EditInstruct ei, UserInfo userinfo)
        {
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, userinfo.CompanyId);
            ei.Put("VERIFY_CMP", userinfo.CompanyId);
            ei.Put("VERIFY_BY", userinfo.UserId);
            ei.PutDate("VERIFY_DATE", odt);
            ei.PutDate("VERIFY_DATE_L", ndt);
        }

        private string NoticeToSql(string approveuser)
        {
            return string.Format(" AND NOTICE_TO={0}", SQLUtils.QuotedStr(approveuser));
        }

        private List<string> getAgentList(string UserId, int sapLevel, string approveName, string role,List<string> SeniorStaffList)
        {
            List<string> userList = new List<string>();
            if (SeniorStaffList.Contains(role) || approveName.Contains("LEVEL Counter Sign") || approveName.Contains("会签") || sapLevel > 50)
            {
                string sql = string.Format("SELECT USER_ID FROM APPROVE_HA WHERE  CONVERT(datetime,CONVERT(char(20),GETDATE(),110)) BETWEEN AGENT_FROM AND AGENT_TO AND AGENT_USER={0}", SQLUtils.QuotedStr(UserId));
                DataTable haDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in haDt.Rows)
                {
                    string userId = Prolink.Math.GetValueAsString(dr["USER_ID"]).ToLower();
                    if (!string.IsNullOrEmpty(userId) && !userList.Contains(userId))
                        userList.Add(userId);
                }
            }
            return userList;
        }

        public bool CheckApproveLevel(string approveuser, List<string> userAgent)
        {
            bool check = false;
            string UserId = userinfo.UserId;
            string sql = string.Format("SELECT NOTICE_TO FROM APPROVE_RECORD WHERE REF_NO={0} AND ROLE={1} AND STATUS='0' ORDER BY CAST(APPROVE_LEVEL AS INT) ", SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveTo));
            DataTable noticeToDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (noticeToDt.Rows.Count > 0)
            {
                DataRow dr = noticeToDt.Rows[0];
                string noticeTo = Prolink.Math.GetValueAsString(dr["NOTICE_TO"]);
                if (!string.IsNullOrEmpty(noticeTo))
                    if (UserId.ToLower() == noticeTo.ToLower()) return true;
                if (userAgent.Contains(noticeTo.ToLower()))
                    return true;
            }

            if (!string.IsNullOrEmpty(approveuser))
            {
                if (UserId.ToLower() == approveuser.ToLower()) return true;
                if (userAgent.Contains(approveuser.ToLower()))
                    return true;
            }

            if ("MANA10".Equals(UserId)) return true;
            if ("ADMIN".Equals(UserId)) return true;
            string approvelists = GetApproveBack();
            string[] approvelist = approvelists.Split(';');
            foreach (string item in approvelist)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (item.Equals(approveTo))
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        private string GetApproveBack()
        {
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND DP.CMP=D.CMP AND APPROVE_ATTR='BILLING'",//AND DP.STN={2}
             SQLUtils.QuotedStr(userinfo.UserId),
             SQLUtils.QuotedStr(userinfo.DataCmp),
             SQLUtils.QuotedStr(userinfo.GroupId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string approveroles = "Person;A;";//申请者的状态下也可以出现  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approveroles += Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_GROUP"]) + ";";
            }
            approveroles = approveroles.Trim(';');
            return approveroles;
        }


        public string createApproveRecord(ref string NextDnType, ref string noticeto, ref string noticemail, string approveRemark)
        {
            string returnMsg = string.Empty;
            string dep = Prolink.Math.GetValueAsString(userinfo.Dep).ToUpper();
            if ("GLST" != dep && "LST" != dep)
            {
                return @Resources.Locale.L_BillApproveHelper_Business_41;
            }
            MixedList mixList = new MixedList();
            string sql = string.Format(@"DELETE FROM APPROVE_RECORD WHERE REF_NO={0}", SQLUtils.QuotedStr(billUid));
            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            returnMsg = createNewApproveRecord(mixList, ref NextDnType, ref noticeto, ref noticemail, approveRemark);
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return returnMsg;
            }
            try
            {
                EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", billUid);
                ei.Put("APPROVE_TYPE", approveType);
                ei.Put("APPLY_BY", userinfo.UserId);
                mixList.Add(ei);
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }


        private Business.TPV.Employe GetUserEmployeInfo()
        {
            string sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(userinfo.UserId));
            string noticemail = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmpBaseByMail(noticemail);
            if (employe == null)
            {
                employe = Business.TPV.Context.OrgService.GetEmploye(userinfo.UserId);
            }
            return employe;
        }

        private string createNewApproveRecord(MixedList ml, ref string NextDnType, ref string noticeto, ref string noticemail, string ApproveRemark)
        {
            string sql = "SELECT * FROM APPROVE_FLOW_D WHERE U_FID = (SELECT TOP 1 U_ID FROM APPROVE_FLOW_M WHERE APPROVE_CODE={0} AND CMP_ID={1} AND TV_MNT={2}) ORDER BY CAST(APPROVE_LEVEL AS INT) ASC";
            sql = string.Format(sql, SQLUtils.QuotedStr(approveType), SQLUtils.QuotedStr(userinfo.DataCmp), SQLUtils.QuotedStr(tvMnt));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(billUid)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;

            string returnMsg = string.Empty, approvecode = string.Empty, approvename = string.Empty, approvetime = string.Empty, role = string.Empty;
            string userId = userinfo.UserId, email = string.Empty, rolename = string.Empty;
            int j = 0, place = 0;
            Business.TPV.Employe employe = null;// GetUserEmployeInfo();
            string afdrole = "A";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
                role = Prolink.Math.GetValueAsString(dt.Rows[i]["ROLE"]);
                approvecode = Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_CODE"]);
                approvename = Prolink.Math.GetValueAsString(dt.Rows[i]["GROUP_DESCP"]);
                approvetime = Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_TIME"]);
                string SeniorStaff = Prolink.Math.GetValueAsString(dt.Rows[i]["SENIOR_STAFF"]);
                apei.Put("AFD_ROLE", afdrole);
                apei.Put("STATUS", "0");
                apei.Put("REF_NO", billUid);
                apei.Put("VOID_LOOP", max);
                apei.Put("APPROVE_CODE", approvecode);
                apei.Put("APPROVE_NAME", approvename);
                rolename = GetApproveName(role, approvename);
                switch (role.ToUpper())
                {
                    case "A":
                        if (i == 0)
                        {
                            apei.Put("STATUS", "1");
                            apei.Put("APPROVE_REMARK", ApproveRemark);
                            apei.Put("APPROVE_BY", userId);
                            apei.PutDate("APPROVE_DATE", DateTime.Now);
                            apei.PutDate("APPROVE_DATE_L", TimeZoneHelper.GetTimeZoneDate(userinfo.CompanyId));
                            apei.Put("REMARK", @Resources.Locale.L_ContractQuery_Views_478);
                            role = userinfo.Dep;
                            apei.Put("ROLE", role);
                            rolename = GetApproveName(role, approvename);
                        }
                        else
                            continue;
                        break;
                    case "AM":
                        if (i == 1)
                        {
                            employe = Business.TPV.Context.OrgService.GetSuperior(userinfo.UserId, userinfo.CompanyId);
                            if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                            userId = employe.EmployeCode;
                            email = employe.EMail;
                            rolename = GetApproveName(role, approvename);
                            apei.PutDate("NOTIFY_DATE", DateTime.Now);
                            apei.PutDate("NOTIFY_DATE_L", TimeZoneHelper.GetTimeZoneDate(userinfo.CompanyId));
                            role = userinfo.Dep + "M";
                            apei.Put("NOTICE_TO", userId);
                            apei.Put("NOTICE_MAIL", email);
                            NextDnType = role;
                            noticeto = userId;
                            noticemail = email;
                            place++;
                        }
                        else
                            continue;
                        break;
                    case "50LEVEL":
                        returnMsg = NewFiftyApprove(employe, billUid, ml, approvecode, approvetime, ref j, ref afdrole);
                        break;
                    default:
                        if (SeniorStaff == "Y")
                        {
                            List<string> noticlist = GetNoticeMailInfo(role);
                            if (noticlist.Count > 1)
                            {
                                apei.Put("NOTICE_TO", noticlist[0]);
                                apei.Put("NOTICE_MAIL", noticlist[1]);
                            }
                            else
                            {
                                return "签核类型:" + approveType + " " + tvMnt + "对应的签核层级：" + role + " 没有设定人员";
                            }
                        }
                        break;
                }
                apei.Put("ROLE", role);
                apei.Put("ROLE_NM", rolename);
                if (!"50LEVEL".Equals(role.ToUpper()))
                {
                    j++;
                    apei.Put("APPROVE_LEVEL", j);
                    ml.Add(apei);
                }
            }
            return returnMsg;
        }

        private List<string> GetNoticeMailInfo(string role)
        {
            List<string> noticlist = new List<string>();
            string sql = string.Format(@"SELECT* from APPROVE_ATTR_DP where U_FID = (
                SELECT top 1 U_ID FROM APPROVE_ATTR_D WHERE U_FID = (
                SELECT TOP 1 U_ID FROM APPROVE_ATTRIBUTE WHERE CMP = {0} AND APPROVE_ATTR = 'BILLING') 
                AND CMP = {0} AND APPROVE_GROUP = {1})
                and cmp = {0}", SQLUtils.QuotedStr(userinfo.DataCmp), SQLUtils.QuotedStr(role));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow[] drs = dt.Select(string.Format("TV_MNT={0}", SQLUtils.QuotedStr(tvMnt)));
            if (drs.Length == 0)
            {
                drs = dt.Select("TV_MNT='ALL'");
            }
            if (drs.Length >= 1)
            {
                string userid = drs[0]["USER_ID"].ToString();
                string uemail = drs[0]["U_EMAIL"].ToString();
                if (!string.IsNullOrEmpty(userid))
                {
                    noticlist.Add(userid);
                    noticlist.Add(uemail);
                }
            }
            return noticlist;

        }

        private string GetApproveName(string role, string approvename)
        {
            string sqlDep = string.Format("SELECT TOP 1 GROUP_DESCP FROM APPROVE_ATTR_D WHERE GROUP_ID={0} AND CMP={1} AND APPROVE_GROUP={2} AND U_FID IN(SELECT U_ID FROM APPROVE_ATTRIBUTE C WHERE C.CMP={1} AND APPROVE_ATTR='BILLING')", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.DataCmp), SQLUtils.QuotedStr(role));
            string _approvename = OperationUtils.GetValueAsString(sqlDep, Prolink.Web.WebContext.GetInstance().GetConnection());
            return _approvename == "" ? approvename : _approvename;
        }


        private string NewFiftyApprove(Business.TPV.Employe employe, string DnNo, MixedList mixList, string approvecode, string approvetime, ref int j, ref string afdrole)
        {
            string rolename = string.Empty;

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
                        EditInstruct levelbefor = ApproveEdit(DnNo, rolename, approvecode, "", rolename, approvetime, j, employe, "A", "");
                        mixList.Add(levelbefor);
                    }
                }
                k++;
            }
            rolename = GetApproveName("50LEVEL", "50层级");
            afdrole = "A";
            if (k > 0)
            {
                j++;
                afdrole = "50LEVEL";
                EditInstruct level = ApproveEdit(DnNo, "50LEVEL", approvecode, "", rolename, approvetime, j, employe, "A", "");
                mixList.Add(level);
            }
            return string.Empty;
        }


        private static EditInstruct ApproveEdit(string RefNo, string role, string approvecode, string approvename, string rolename, string approvetime, int j, Business.TPV.Employe employe, string afdrole, string ApproveRemark = "")
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
            levelei.Put("AFD_ROLE", afdrole);
            levelei.Put("APPROVE_CODE", approvecode);
            levelei.Put("APPROVE_NAME", approvename);
            if (string.IsNullOrEmpty(approvename))
                levelei.Put("APPROVE_NAME", rolename);
            levelei.Put("APPROVE_TIME", approvetime);
            levelei.Put("APPROVE_REMARK", ApproveRemark);
            return levelei;
        }

        public static List<string> GetSeniorStaff(string ApproveCode)
        {
            return GetSeniorStaff(new string[] { ApproveCode });
        }

        public static List<string> GetSeniorStaff(string[] ApproveCode)
        {
            List<string> SeniorStaff = new List<string>();
            string sql = string.Format(@"SELECT DISTINCT APPROVE_GROUP FROM APPROVE_ATTR_D WHERE U_FID IN (SELECT AU_ID FROM APPROVE_FLOW_M WHERE APPROVE_CODE
            IN {0}) AND SENIOR_STAFF='Y'", SQLUtils.Quoted(ApproveCode));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt != null && dt.Rows.Count > 0)
            {
                SeniorStaff = dt.AsEnumerable().Select(row => row.Field<string>("APPROVE_GROUP")).ToList();
            }
            return SeniorStaff;
        }
    }
}