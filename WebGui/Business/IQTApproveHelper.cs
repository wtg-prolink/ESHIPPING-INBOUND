using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using WebGui.Models;

namespace Business
{
    public class IQTApproveHelper
    {
        private UserInfo UserInfo = new UserInfo();
        private string UId { get; set; }
        private bool Finish { get; set; }
        private bool ApproveCheck { get; set; }
        private QTMData QtmData = new QTMData();
        private List<ApproveRecordData> ARecordList = new List<ApproveRecordData>();
        private ApproveRecordData NextRecord = new ApproveRecordData();
        private ApproveRecordData NowRecord = new ApproveRecordData();
        private class QTMData
        {
            public string QuotType { get; set; }
            public string TranMode { get; set; }
            public string QuotNo { get; set; }
            public string ApproveType { get; set; }
            public string ApproveTo { get; set; }
            public string GroupId { get; set; }
            public string Rlocation { get; set; }
            public string ApproveUser { get; set; }
            public string LspCd { get; set; }
            public string LspNm { get; set; }
        }

        private class ApproveRecordData
        {
            public int Seq { get; set; }
            public string Role { get; set; }
            public string RoleNm { get; set; }
            public int ApproveLevel { get; set; }
            public string NoticeTo { get; set; }
            public string NoticeMail { get; set; }
            public string NoticeDate { get; set; }
            public string Status { get; set; }
            public string Remark { get; set; }
            public int VoidLoop { get; set; }
            public string ApproveCode { get; set; }
            public string ApproveName { get; set; }
            public string ApproveTime { get; set; }
            public string ApproveBy { get; set; }
            public string ApproveDate { get; set; }
            public string AfdRole { get; set; }
        }
        public IQTApproveHelper(UserInfo userInfo, string JobNo,string type)
        {
            this.UserInfo = userInfo;
            this.UId = JobNo;
            InitQtmData();
            SetQuotType();
            if (type == "A")
            {
                InitApproveRecord();
                InitApproveCheck();
            }
            else if (type == "B")
            {
                InitBackRecord();
                InitBackCheck();
            }
        }
        private void InitQtmData()
        {
            string sql = string.Format("SELECT QUOT_TYPE,TRAN_MODE,QUOT_NO,APPROVE_TYPE,APPROVE_TO,GROUP_ID,RLOCATION,APPROVE_USER,LSP_CD,LSP_NM FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            this.QtmData.QuotType = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_TYPE"]);
            this.QtmData.TranMode = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_MODE"]);
            this.QtmData.QuotNo = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_NO"]);
            this.QtmData.ApproveType = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_TYPE"]);
            this.QtmData.ApproveTo = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_TO"]);
            this.QtmData.GroupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            this.QtmData.Rlocation = Prolink.Math.GetValueAsString(dt.Rows[0]["RLOCATION"]);
            this.QtmData.ApproveUser = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_USER"]);
            this.QtmData.LspCd = Prolink.Math.GetValueAsString(dt.Rows[0]["LSP_CD"]);
            this.QtmData.LspNm = Prolink.Math.GetValueAsString(dt.Rows[0]["LSP_NM"]);
        }

        private void SetQuotType()
        {
            if (!string.IsNullOrEmpty(this.QtmData.ApproveType))
                return;
            string approveType = "QUOT_" + this.QtmData.TranMode;
            string sql = string.Format("SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE={0} ORDER BY APPROVE_LEVEL", SQLUtils.QuotedStr(approveType));
            DataTable approvedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (approvedt.Rows.Count <= 0)
            {
                approveType = "QUOT_CON";
                sql = string.Format("SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE={0} ORDER BY APPROVE_LEVEL", SQLUtils.QuotedStr(approveType));
                approvedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            this.QtmData.ApproveType = approvedt.Rows.Count > 0 ? approveType : "";
        }

        private void InitApproveRecord()
        {
            string sql = string.Format("SELECT ROLE,APPROVE_LEVEL,NOTICE_TO,NOTICE_MAIL,STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC",
                SQLUtils.QuotedStr(this.UId), SQLUtils.QuotedStr(this.QtmData.ApproveType));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string condition = string.Format("ROLE={0}", SQLUtils.QuotedStr(this.QtmData.ApproveTo));
            if (!string.IsNullOrEmpty(this.QtmData.ApproveUser))
                condition += NoticeToSql(this.QtmData.ApproveUser);
            DataRow[] drs = dt.Select(condition);
            this.Finish = false;
            if (drs.Length > 0)
            {
                this.Finish = "1".Equals(Prolink.Math.GetValueAsString(drs[0]["STATUS"]));
                ApproveRecordData NowRecordData = new ApproveRecordData();
                NowRecordData.Seq = 0;
                NowRecordData.Role = Prolink.Math.GetValueAsString(drs[0]["ROLE"]);
                NowRecordData.ApproveLevel = Prolink.Math.GetValueAsInt(drs[0]["APPROVE_LEVEL"]);
                NowRecordData.NoticeTo = Prolink.Math.GetValueAsString(drs[0]["NOTICE_TO"]);
                NowRecordData.NoticeMail = Prolink.Math.GetValueAsString(drs[0]["NOTICE_MAIL"]);
                NowRecordData.Status = Prolink.Math.GetValueAsString(drs[0]["STATUS"]);
                ARecordList.Add(NowRecordData);
            }
            NowRecord = SetRecordData(0);
            drs = dt.Select("STATUS='0'");
            int level = -1, j = -1;
            for (int i = 0; i < drs.Length; i++)
            {
                int approveLevel = Prolink.Math.GetValueAsInt(drs[i]["APPROVE_LEVEL"]);
                if (approveLevel <= NowRecord.ApproveLevel)
                    continue;
                if (level < 0 || level > approveLevel)
                {
                    level = approveLevel;
                    j = i;
                }
            }
            if (j >= 0)
            {
                ApproveRecordData NextRecordData = new ApproveRecordData();
                NextRecordData.Seq = 1;
                NextRecordData.Role = Prolink.Math.GetValueAsString(drs[j]["ROLE"]);
                NextRecordData.ApproveLevel = Prolink.Math.GetValueAsInt(drs[j]["APPROVE_LEVEL"]);
                NextRecordData.NoticeTo = Prolink.Math.GetValueAsString(drs[j]["NOTICE_TO"]);
                NextRecordData.NoticeMail = Prolink.Math.GetValueAsString(drs[j]["NOTICE_MAIL"]);
                NextRecordData.Status = Prolink.Math.GetValueAsString(drs[j]["STATUS"]);
                ARecordList.Add(NextRecordData);
            }
        }
        private void InitBackRecord()
        {
            string sql = string.Format("SELECT * FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(this.QtmData.ApproveType));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow[] drs = dt.Select(string.Format("ROLE={0}", SQLUtils.QuotedStr(this.QtmData.ApproveTo)));
            if (drs.Length > 0)
            {
                ApproveRecordData backRecordData = new ApproveRecordData();
                backRecordData.Seq = 3;
                backRecordData.AfdRole = Prolink.Math.GetValueAsString(drs[0]["AFD_ROLE"]);
                backRecordData.NoticeTo = Prolink.Math.GetValueAsString(drs[0]["NOTICE_TO"]);
                ARecordList.Add(backRecordData);
                drs = dt.Select(string.Format("AFD_ROLE={0} AND STATUS='1'", SQLUtils.QuotedStr(backRecordData.AfdRole)));
                foreach (DataRow dr in drs)
                {
                    ApproveRecordData aRecordData = new ApproveRecordData();
                    aRecordData.Seq = 4;
                    aRecordData.AfdRole = Prolink.Math.GetValueAsString(drs[0]["AFD_ROLE"]);
                    aRecordData.NoticeTo = Prolink.Math.GetValueAsString(drs[0]["NOTICE_TO"]);
                    ARecordList.Add(aRecordData);
                }
            }
        }

        private void InitApproveCheck()
        {
            bool check = false;
            if (!string.IsNullOrEmpty(this.QtmData.ApproveUser))
            {
                if (UserInfo.UserId.ToLower() == this.QtmData.ApproveUser.ToLower())
                    check = true;
            }
            if ("MANA10".Equals(UserInfo.UserId)) check = true;
            if ("ADMIN".Equals(UserInfo.UserId)) check = true;
            string approvelists = GetApproveBack(this.QtmData.Rlocation,this.QtmData.GroupId);
            string[] approvelist = approvelists.Split(';');
            foreach (string item in approvelist)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (item.Equals(this.QtmData.ApproveTo))
                {
                    check = true;
                    break;
                }
            }
            ApproveCheck = check;
        }

        private void InitBackCheck()
        {
            bool check = false;
            if ("MANA10".Equals(UserInfo.UserId)) check = true;
            if ("ADMIN".Equals(UserInfo.UserId)) check = true;
            string approvelists = GetApproveBack(UserInfo.CompanyId,UserInfo.GroupId);
            string[] approvelist = approvelists.Split(';');
            foreach (string item in approvelist)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (item.Equals(this.QtmData.ApproveTo))
                {
                    check = true;
                    break;
                }
            }
            ApproveCheck = check;
        }

        private string GetApproveBack(string cmp,string group)
        {
            string sql = string.Format(@"SELECT DISTINCT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR='QUOT'",
             SQLUtils.QuotedStr(UserInfo.UserId),
             SQLUtils.QuotedStr(cmp),
             SQLUtils.QuotedStr(group));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string approveroles = "Person;A;LST;";//申请者的状态下也可以出现  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approveroles += Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_GROUP"]) + ";";
            }
            approveroles = approveroles.Trim(';');
            return approveroles;
        }
        private string NoticeToSql(string approveuser)
        {
            return string.Format(" AND NOTICE_TO={0}", SQLUtils.QuotedStr(approveuser));
        }

        public string ApprovePass()
        {
            MixedList ml = new MixedList();
            NowRecord = SetRecordData(0);
            NextRecord = SetRecordData(1);
            string returnMsg = "";
            returnMsg = CheckApproveStaus(this.QtmData.QuotType, 1);
            if (!string.IsNullOrEmpty(returnMsg))
                return this.QtmData.QuotNo + ":" + returnMsg;
            if (Finish)
                return @Resources.Locale.L_ActManage_Quottip1 + this.QtmData.QuotNo + @Resources.Locale.L_BillApproveHelper_Business_35;
            if(!ApproveCheck|| (!"A".Equals(this.QtmData.ApproveTo) && ARecordList.Count <= 0))
                return string.Format(@Resources.Locale.L_ActManage_Quottip2, this.QtmData.QuotNo);
            if ("A".Equals(this.QtmData.ApproveTo))
                returnMsg = CreateApproveRecord();
            if (!string.IsNullOrEmpty(returnMsg))
                return this.QtmData.QuotNo + ":" + returnMsg;
            if (!"A".Equals(this.QtmData.ApproveTo))
                returnMsg = UpdateApproveRecord(ml);
            if (!string.IsNullOrEmpty(returnMsg))
                return this.QtmData.QuotNo + ":" + returnMsg;
            UpdateSmqtm(ml);
            string ffid = "";
            string sql = string.Format("SELECT APPROVE_GROUP,U_ID FROM APPROVE_ATTR_D WHERE U_FID IN(SELECT U_ID FROM APPROVE_ATTRIBUTE C WHERE C.CMP={1} AND APPROVE_ATTR='QUOT') AND APPROVE_GROUP={0}",
                SQLUtils.QuotedStr(NextRecord.Role), SQLUtils.QuotedStr(this.QtmData.Rlocation));
            DataTable nextdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (nextdt.Rows.Count <= 0 && !string.IsNullOrEmpty(NextRecord.Role))
            {
                if (string.IsNullOrEmpty(NextRecord.NoticeMail))
                {
                    return returnMsg = this.QtmData.QuotNo + ":"+ @Resources.Locale.L_BaseApprove_Business_20 + NextRecord.Role + @Resources.Locale.L_BaseApprove_Business_21;
                }
            }
            else
            {
                ffid = Prolink.Math.GetValueAsString(nextdt.Rows[0]["U_ID"]);
            }
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = this.QtmData.QuotNo + ":" + @Resources.Locale.L_BillApproveHelper_Business_38;
                returnMsg += AddToNoticeQuot(ffid);
            }
            catch (Exception ex)
            {
                returnMsg = this.QtmData.QuotNo + ":" + @Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
            }
            return returnMsg;
        }

        public string ApproveBack(string Remark)
        {
            string returnMsg = "";
            returnMsg = CheckApproveStaus(this.QtmData.QuotType, 0);
            if (!string.IsNullOrEmpty(returnMsg))
                return returnMsg;
            ApproveRecordData backRecordData = SetRecordData(3);
            ApproveRecordData aRecordData = SetRecordData(4);
            if (UserInfo.UserId == backRecordData.NoticeTo)
                ApproveCheck = true;
            if (!ApproveCheck)
                return  @Resources.Locale.L_QTManage_Controllers_tip3 + this.QtmData.QuotNo + @Resources.Locale.L_BillApproveHelper_Business_24;
            string sql = "";
            if ("A".Equals(backRecordData.AfdRole) || "AC".Equals(backRecordData.AfdRole))
            {
                MixedList ml = new MixedList();
                ml.Add(UpApproveStatus());
                sql = string.Format("UPDATE APPROVE_RECORD SET STATUS='0' WHERE REF_NO={0} AND APPROVE_CODE={1}",
                    SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(this.QtmData.ApproveType));
                ml.Add(sql);
                string backremark = @Resources.Locale.L_BillApproveHelper_Business_29 + UserInfo.UserId + @Resources.Locale.L_BillApproveHelper_Business_30 + Remark;
                sql = string.Format("UPDATE APPROVE_RECORD SET APPROVE_BY={2},APPROVE_DATE=GETDATE(),REMARK={3} WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL=1",
                    SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(backRecordData.AfdRole), SQLUtils.QuotedStr(UserInfo.UserId), SQLUtils.QuotedStr(backremark));
                ml.Add(sql);
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
                return @Resources.Locale.L_BillApproveHelper_Business_25;
            }

            if (aRecordData.Seq != 4)
            {
                if (this.QtmData.ApproveTo == "A" || backRecordData.AfdRole == "A")
                {
                    //申請者退回就把帳單狀態改為「拒絕」
                    sql = "UPDATE SMQTM SET QUOT_TYPE='R',APPROVE_USER=NULL, BACK_REMARK={0} WHERE U_ID={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(Remark), SQLUtils.QuotedStr(UId));

                    SendQuotRJMail(Remark);
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
            return "";
        }

        public EditInstruct UpApproveStatus()
        {
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", UId);
            ei.Put("APPROVE_TO", "A");
            ei.PutDate("APPROVE_DATE", DateTime.Now);
            ei.PutDate("APPROVE_DATE_L", TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId));
            ei.Put("APPROVE_USER", ""); //更新此笔签核人员到上一层
            ei.Put("APPROVE_BY", UserInfo.UserId);
            return ei;
        }

        public string CheckApproveStaus(string QuotType, int type)
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

        public string CreateApproveRecord()
        {
            string returnMsg = "";
            if ("GLST" != UserInfo.Dep.ToUpper() && "LST" != UserInfo.Dep.ToUpper())
                return @Resources.Locale.L_BillApproveHelper_Business_41;
            MixedList mixList = new MixedList();
            string sql = "SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE={0} AND CMP_ID={1} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC";
            sql = string.Format(sql, SQLUtils.QuotedStr(this.QtmData.ApproveType), SQLUtils.QuotedStr(UserInfo.CompanyId));
            DataTable dtapprove = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "DELETE FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(this.QtmData.ApproveType));

            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            returnMsg = CreateNewApproveRecord(dtapprove);
            try
            {
                ApproveEdit(mixList);
                if (mixList.Count > 0)
                    OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }

        public string UpdateApproveRecord(MixedList ml)
        {
            string returnMsg = "";
            if (string.IsNullOrEmpty(NextRecord.NoticeTo) && !string.IsNullOrEmpty(NextRecord.Role))
            {
                if (CheckRole(this.QtmData.ApproveTo, NextRecord.Role))
                {
                    Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(UserInfo.UserId, UserInfo.CompanyId);
                    if (employe1 == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                    NextRecord.NoticeTo = employe1.EmployeCode;
                    NextRecord.NoticeMail = employe1.EMail;
                }
                else
                {
                    string sql1 = string.Format("SELECT * FROM APPROVE_ATTR_DP where U_FID IN (select U_ID from APPROVE_ATTR_D WHERE APPROVE_ATTR='QUOT' AND APPROVE_GROUP={0} AND CMP={1} AND GROUP_ID={2})",
                        SQLUtils.QuotedStr(NextRecord.Role), SQLUtils.QuotedStr(UserInfo.CompanyId), SQLUtils.QuotedStr(UserInfo.GroupId));
                    DataTable appdt1 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    NextRecord.NoticeTo = Prolink.Math.GetValueAsString(appdt1.Rows[0]["USER_ID"]);
                    NextRecord.NoticeMail = Prolink.Math.GetValueAsString(appdt1.Rows[0]["U_EMAIL"]);
                }
            }
            EditInstruct apei = GetApproveEi1(Prolink.Math.GetValueAsString(NowRecord.ApproveLevel));
            ml.Add(apei);
            string sql = string.Format("UPDATE APPROVE_RECORD SET ACTUAL_TIME=DATEDIFF(MI,NOTIFY_DATE ,APPROVE_DATE) WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL={2}", 
                SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(this.QtmData.ApproveType), SQLUtils.QuotedStr(this.QtmData.ApproveTo));
            ml.Add(sql);
            if (string.IsNullOrEmpty(NextRecord.Role))
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ChangeFinish();
                    returnMsg = "success";
                }
                catch (Exception ex)
                {
                    returnMsg = @Resources.Locale.L_BillApproveHelper_Business_37 + ex.ToString();
                }
                return returnMsg;
            }
            EditInstruct newEi = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            newEi.PutKey("REF_NO", UId);
            newEi.PutKey("APPROVE_CODE", this.QtmData.ApproveType);
            newEi.PutKey("ROLE", NextRecord.Role);
            newEi.PutKey("APPROVE_LEVEL", NextRecord.ApproveLevel);
            newEi.PutDate("NOTIFY_DATE", TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId));
            newEi.Put("NOTICE_TO", NextRecord.NoticeTo);
            newEi.Put("NOTICE_MAIL", NextRecord.NoticeMail);
            ml.Add(newEi);
            return "";
        }

        public void UpdateSmqtm(MixedList ml)
        {
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", UId);
            ei.Put("APPROVE_USER", NextRecord.NoticeTo);
            ei.Put("APPROVE_TO", NextRecord.Role);
            ei.Put("APPROVE_TYPE", this.QtmData.ApproveType);
            ei.Put("APPROVE_BY", UserInfo.UserId);
            ei.PutDate("APPROVE_DATE", DateTime.Now);
            ei.PutDate("APPROVE_DATE_L", TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId));
            ml.Add(ei);
        }

        public string AddToNoticeQuot(string uuid)
        {
            string subject = @Resources.Locale.L_QTQuery_QuotNo + ":" + this.QtmData.QuotNo + @Resources.Locale.L_BillApproveHelper_Business_40;
            string msg = string.Empty;
            if (!string.IsNullOrEmpty(NextRecord.NoticeTo))
            {
                string type = "X".Equals(this.QtmData.TranMode) ? MailManager.IB_QuotNotify_X : "B".Equals(this.QtmData.TranMode) ? MailManager.IB_QuotNotify_B : "C".Equals(this.QtmData.TranMode) ? MailManager.IB_QuotNotify_C : string.Empty;
                string str = UId + "#" + this.QtmData.GroupId + "#" + this.QtmData.Rlocation + "#" + Guid.NewGuid().ToString() + "#" + type + "#" + NextRecord.NoticeTo + "#";
                EvenFactory.AddEven(str, UId, type, null, 1, 0, NextRecord.NoticeMail, subject, "", 3, 0);
                return msg;
            }
            if (string.IsNullOrEmpty(uuid)) return msg;
            string sql = string.Format(@"SELECT * FROM APPROVE_ATTR_DP WHERE {0} AND U_FID={1}",
                  UserInfo.basecondtions, SQLUtils.QuotedStr(uuid));
            if (!string.IsNullOrEmpty(NextRecord.NoticeTo))
                sql += string.Format(" AND USER_ID={0}", SQLUtils.QuotedStr(NextRecord.NoticeTo));

            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            #region 处理Msg提醒的
            DataRow[] msgrows = maindt.Select("BY_MSG='Y'");
            MessageData md = null;
            string receiveUser = string.Empty;
            MixedList ml = new MixedList();
            foreach (DataRow dr in msgrows)
            {
                receiveUser = Prolink.Math.GetValueAsString(dr["USER_ID"]);
                md = new MessageData();
                md.Title = "Quot Message";
                md.Type = "1";
                md.JobNo = uuid;
                md.Content = subject;
                EditInstruct ei = BillApproveHelper.AddMessage(md, UserInfo.UserId, receiveUser, this.QtmData.GroupId, this.QtmData.Rlocation, UserInfo.Dep);
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
                    msg = this.QtmData.QuotNo + ":" + ex.Message.ToString();
                }
            }
            return msg;
            #endregion
        }

        public void ChangeFinish()
        {
            string sql = string.Format(@"SELECT TOP 1 STATUS FROM APPROVE_RECORD WHERE REF_NO={0} ORDER BY APPROVE_LEVEL DESC", SQLUtils.QuotedStr(UId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow[] drs = dt.Select("STATUS='0'");
                if (drs.Length <= 0)
                {
                    MixedList mxlist = new MixedList();
                    mxlist.Add(UpdateQuotFinish());
                    mxlist.Add(UpdateQuotDFinish());
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
        public EditInstruct UpdateQuotFinish()
        {
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", UId);
            ei.Put("QUOT_TYPE", "A");
            ei.Put("APPROVE_TO", "Finish");
            ei.Put("APPROVE_USER", "");
            ei.Put("APPROVE_BY", UserInfo.UserId);
            ei.PutDate("APPROVE_DATE", DateTime.Now);
            ei.PutDate("APPROVE_DATE_L", TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId));
            return ei;
        }
        public EditInstruct UpdateQuotDFinish()
        {
            EditInstruct ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("U_FID", UId);
            ei.Put("QUOT_TYPE", "F");
            return ei;
        }

        public static bool CheckRole(string role, string nextrole)
        {
            string _role = role.IndexOf(nextrole) == 0 ? nextrole.Replace(role, "") : _role = "";
            Regex re = new Regex("M");
            if (_role.Length > 0 && _role.Length == re.Matches(_role).Count) return true;
            return false;
        }

        public EditInstruct GetApproveEi1(string NowLevel)
        {
            EditInstruct apei = new EditInstruct("APPROVE_RECORD", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("REF_NO", UId);
            apei.PutKey("APPROVE_CODE", this.QtmData.ApproveType);
            if (!string.IsNullOrEmpty(NowLevel))
                apei.PutKey("APPROVE_LEVEL", NowLevel);
            apei.Put("CREATE_BY", UserInfo.UserId);
            apei.PutDate("CREATE_DATE", TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId));
            apei.Put("APPROVE_BY", UserInfo.UserId);
            apei.PutDate("APPROVE_DATE", TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId));
            apei.Put("STATUS", "1");
            apei.Put("REMARK", @Resources.Locale.L_Pass);
            return apei;
        }

        public string CreateNewApproveRecord(DataTable dt)
        {
            int max = OperationUtils.GetValueAsInt(string.Format("SELECT MAX(VOID_LOOP) FROM APPROVE_RECORD WHERE REF_NO={0} ", SQLUtils.QuotedStr(UId)), Prolink.Web.WebContext.GetInstance().GetConnection()) + 1;
            string returnMsg = string.Empty;
            string userId = UserInfo.UserId, email = string.Empty;
            int j = 0, place = 0;
            bool isokTime = true, end50 = false, exist50 = false;
            string NextDnType = "", notice_mail = "", noticeto = "";
            string Now = TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId).ToString("yyyyMMddHHmmss");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ApproveRecordData aRecordData = new ApproveRecordData();
                string role = Prolink.Math.GetValueAsString(dt.Rows[i]["ROLE"]);
                string approvetime = Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_TIME"]);
                aRecordData.Seq = 2;
                aRecordData.Status = "0";
                aRecordData.VoidLoop = max;
                aRecordData.ApproveCode = Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_CODE"]);
                aRecordData.ApproveName = Prolink.Math.GetValueAsString(dt.Rows[i]["GROUP_DESCP"]);
                string rolename = GetApproveName(role, aRecordData.ApproveName);
                switch (role.ToUpper())
                {
                    case "A":
                        if (i == 0)
                        {
                            aRecordData.Status = "1";
                            aRecordData.ApproveBy = userId;
                            aRecordData.ApproveDate = Now;
                            aRecordData.Remark = @Resources.Locale.L_ContractQuery_Views_478;
                            role = UserInfo.Dep;
                            rolename = GetApproveName(role, aRecordData.ApproveName);
                        }
                        else
                            continue;
                        break;
                    case "AM":
                        if (i == 1)
                        {
                            Business.TPV.Employe employe1 = Business.TPV.Context.OrgService.GetSuperior(UserInfo.UserId, UserInfo.CompanyId);
                            if (employe1 == null) return @Resources.Locale.L_BillApproveHelper_Business_43;
                            userId = employe1.EmployeCode;
                            email = employe1.EMail;
                            noticeto = userId;
                            notice_mail = email;
                            isokTime = false;
                            rolename = GetApproveName(role, aRecordData.ApproveName);
                            aRecordData.NoticeDate = Now;
                            role = UserInfo.Dep + "M";
                            aRecordData.NoticeTo = userId;
                            aRecordData.NoticeMail = email;
                            NextDnType = role;
                            place++;
                        }
                        else
                            continue;
                        break;
                    case "50LEVEL":
                        exist50 = end50 ? false : true;
                        break;
                }
                
                aRecordData.Role = role;
                aRecordData.RoleNm = rolename;
                if (isokTime)
                    aRecordData.NoticeDate = Now;
                if (!"50LEVEL".Equals(role.ToUpper()))
                {
                    j++;
                    aRecordData.ApproveLevel = j;
                    ARecordList.Add(aRecordData);
                }
                if (i >= 1 && exist50)
                {
                    if (string.IsNullOrEmpty(userId)) userId = UserInfo.UserId;
                    Business.TPV.Employe employe = Business.TPV.Context.OrgService.GetEmploye(userId);
                    rolename = string.Empty; role = UserInfo.Dep;
                    for (int k = 0; k < place; k++)
                        role += "M";
                    if (employe == null) return @Resources.Locale.L_BillApproveHelper_Business_33;
                    if (employe != null)
                    {
                        while (employe.OrgLevel < 50)
                        {
                            employe = Business.TPV.Context.OrgService.GetSuperior(employe);
                            userId = employe.EmployeCode;
                            email = employe.EMail;
                            role += "M";
                            if (string.IsNullOrEmpty(NextDnType)) NextDnType = role;
                            rolename = GetApproveName(role, "");
                            if (employe.OrgLevel < 50)
                            {
                                j++;
                                AddApprove(role, aRecordData.ApproveCode, rolename, approvetime, j, userId, email, max, isokTime);
                                isokTime = false;
                            }
                        }
                        j++;
                        rolename = GetApproveName("50LEVEL", "50 LEVEL");
                        if (string.IsNullOrEmpty(NextDnType)) NextDnType = "50LEVEL";
                        AddApprove("50LEVEL", aRecordData.ApproveCode, rolename, approvetime, j, userId, email, max, isokTime);
                        isokTime = false;
                    }
                    exist50 = false;
                    end50 = true;
                }
            }

            NextRecord.Seq = 1;
            NextRecord.Role = NextDnType;
            NextRecord.NoticeTo = noticeto;
            NextRecord.NoticeMail = notice_mail;
            ARecordList.Add(NextRecord);

            return returnMsg;
        }

        private void AddApprove(string role, string approvecode, string rolename, string approvetime, int j, string userid, string noticemail, int voidtime, bool noticetime = false)
        {
            ApproveRecordData aRecordData = new ApproveRecordData();
            aRecordData.Seq = 2;
            aRecordData.NoticeTo = userid;
            aRecordData.NoticeMail = noticemail;
            aRecordData.Status= "0";
            aRecordData.Role = role;
            aRecordData.RoleNm = rolename;
            aRecordData.ApproveLevel = j;
            aRecordData.VoidLoop = voidtime;
            if (noticetime)
                aRecordData.NoticeDate = TimeZoneHelper.GetTimeZoneDate(UserInfo.CompanyId).ToString("yyyyMMddHHmmss");
            aRecordData.ApproveCode = approvecode;
            aRecordData.ApproveName = "";
            aRecordData.ApproveTime = approvetime;
            ARecordList.Add(aRecordData);
        }

        private void ApproveEdit(MixedList ml)
        {
            foreach (ApproveRecordData recordData in ARecordList)
            {
                if (recordData.Seq != 2)
                    continue;
                EditInstruct levelei = new EditInstruct("APPROVE_RECORD", EditInstruct.INSERT_OPERATION);
                levelei.Put("NOTICE_TO", recordData.NoticeTo);
                levelei.Put("NOTICE_MAIL", recordData.NoticeMail);
                levelei.Put("STATUS", recordData.Status);
                levelei.Put("ROLE", recordData.Role);
                levelei.Put("ROLE_NM", recordData.RoleNm);
                levelei.Put("REF_NO", UId);
                levelei.Put("APPROVE_LEVEL", recordData.ApproveLevel);
                levelei.Put("VOID_LOOP", recordData.VoidLoop);
                levelei.Put("AFD_ROLE", "A");
                levelei.Put("APPROVE_CODE", recordData.ApproveCode);
                levelei.Put("APPROVE_NAME", recordData.ApproveName);
                levelei.Put("APPROVE_TIME", recordData.ApproveTime);
                levelei.Put("REMARK", recordData.Remark);
                levelei.Put("APPROVE_BY", recordData.ApproveBy);
                levelei.PutDate("APPROVE_DATE", recordData.ApproveDate);
                levelei.PutDate("NOTIFY_DATE", recordData.NoticeDate);
                ml.Add(levelei);
            }
        }

        public string GetApproveName(string role, string approvename)
        {
            string sqlDep = string.Format("SELECT TOP 1 GROUP_DESCP FROM APPROVE_ATTR_D WHERE GROUP_ID={0} AND CMP={1} AND APPROVE_GROUP={2} AND U_FID IN(SELECT U_ID FROM APPROVE_ATTRIBUTE C WHERE C.CMP={1} AND APPROVE_ATTR='QUOT')", SQLUtils.QuotedStr(UserInfo.GroupId), SQLUtils.QuotedStr(UserInfo.CompanyId), SQLUtils.QuotedStr(role));
            string _approvename = OperationUtils.GetValueAsString(sqlDep, Prolink.Web.WebContext.GetInstance().GetConnection());
            return _approvename == "" ? approvename : _approvename;
        }
        public void SendQuotRJMail(string reason = "")
        {
            string _tranmode = "Q" + this.QtmData.TranMode;
            DataTable mailGroupDt = MailTemplate.GetMailGroup(this.QtmData.LspCd, UserInfo.GroupId, _tranmode);
            if (mailGroupDt.Rows.Count > 0)
            {
                foreach (DataRow item1 in mailGroupDt.Rows)
                {
                    string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                    if (mailStr != "")
                    {
                        string type = "X".Equals(this.QtmData.TranMode) ? MailManager.IB_QuotRefuse_X : "B".Equals(this.QtmData.TranMode) ? MailManager.IB_QuotRefuse_B : "C".Equals(this.QtmData.TranMode) ? MailManager.IB_QuotRefuse_C : "O".Equals(this.QtmData.TranMode) ? MailManager.IB_QuotRefuse_O : string.Empty;
                        EvenFactory.AddEven(UId + "#" + UserInfo.GroupId + "#" + UserInfo.CompanyId + "#" + Guid.NewGuid().ToString() + "#" + type + "#" + this.QtmData.LspNm + "#" + reason, UId, type, null, 1, 0, mailStr, @Resources.Locale.L_QTQuery_QuotNo + ":" + this.QtmData.QuotNo + "  " + @Resources.Locale.L_ActSetup_CheckDescp + "：" + reason, "");
                    }
                }
            }
        }

        private ApproveRecordData SetRecordData(int seq)
        {
            ApproveRecordData approveRecord = new ApproveRecordData();
            foreach (ApproveRecordData aRecord in ARecordList)
            {
                if (aRecord.Seq == seq)
                {
                    approveRecord = aRecord;
                    break;
                }
            }
            return approveRecord;
        }
    }
}
