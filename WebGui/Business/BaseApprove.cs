using Business.Service;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebGui.Models;

namespace Business
{
    public class BaseApprove
    {
        /// <summary>
        /// 获取签核群组中对应的uid
        /// </summary>
        /// <param name="nextdntype"></param>
        /// <param name="notice_mail"></param>
        /// <returns></returns>
        public static ResultInfo GetAPAttrDUi(string apgroup, string notice_mail, string company, string ApAttr)
        {
            ResultInfo resultinfo = new ResultInfo();
            resultinfo.IsSucceed = true;
            string sql = string.Format(@"SELECT APPROVE_GROUP,U_ID FROM APPROVE_ATTR_D WHERE U_FID=(
                SELECT TOP 1 U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR={0} AND CMP={1}) AND CMP={1} AND APPROVE_GROUP={2}",
                SQLUtils.QuotedStr(ApAttr), SQLUtils.QuotedStr(company), SQLUtils.QuotedStr(apgroup));
            DataTable nextdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (nextdt.Rows.Count <= 0 && !string.IsNullOrEmpty(apgroup))
            {
                if (string.IsNullOrEmpty(notice_mail))
                {
                    resultinfo.IsSucceed = false;
                    resultinfo.ResultCode = ResultCode.DataIsNull;
                    resultinfo.Description = @Resources.Locale.L_BaseApprove_Business_20 + apgroup + @Resources.Locale.L_BaseApprove_Business_21;
                }
            }
            else
            {
                resultinfo.IsSucceed = true;
                resultinfo.ResultCode = ResultCode.Succeed;
                resultinfo.Description = Prolink.Math.GetValueAsString(nextdt.Rows[0]["U_ID"]);
            }
            return resultinfo;
        }

        public static string GetApproveBack(UserInfo userinfo, string ApAttr)
        {
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR={3}",
             SQLUtils.QuotedStr(userinfo.UserId),
             SQLUtils.QuotedStr(userinfo.CompanyId),
             SQLUtils.QuotedStr(userinfo.GroupId),
             SQLUtils.QuotedStr(ApAttr));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string approveroles = "Person;A;";//申请者的状态下也可以出现  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approveroles += Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_GROUP"]) + ";";
            }
            approveroles = approveroles.Trim(';');
            return approveroles;
        }

        public static bool CheckApproveLevel(string apprvesto, UserInfo userinfo, string approveuser,string ApAttr)
        {
            bool check = false;
            string UserId = userinfo.UserId;
            if (userinfo.UserId == approveuser) return true;
            if ("MANA10".Equals(UserId)) return true;
            if ("ADMIN".Equals(UserId)) return true;
            string approvelists = GetApproveBack(userinfo, ApAttr);
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

        public static bool CheckLastOK(string refno, string apcode, string role, string noticeto)
        {
            string sql = string.Format("SELECT STATUS FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND ROLE={2}",
                SQLUtils.QuotedStr(refno), SQLUtils.QuotedStr(apcode), SQLUtils.QuotedStr(role));
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

        public static string NoticeToSql(string approveuser)
        {
            return string.Format(" AND NOTICE_TO={0}", SQLUtils.QuotedStr(approveuser));
        }

        public static string GetUpActualTimeSql(string refno, string approvetype, string Approveto)
        {
            string sql = "update APPROVE_RECORD SET ACTUAL_TIME=DateDiff(mi,NOTIFY_DATE ,APPROVE_DATE) WHERE REF_NO={0} AND APPROVE_CODE={1} AND APPROVE_LEVEL={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(refno), SQLUtils.QuotedStr(approvetype), SQLUtils.QuotedStr(Approveto));
            return sql;
        }

        public static EditInstruct AddMessage(MessageData md, UserInfo userinfo, string receiveUser)
        {
            EditInstruct ei = new EditInstruct("GFMESSAGE", EditInstruct.INSERT_OPERATION);
            ei.Put("GROUP_ID", userinfo.GroupId);
            ei.Put("MSG_ID", Guid.NewGuid().ToString());
            ei.Put("RCV_CD", receiveUser);
            ei.Put("STATUS", MessageData.HAS_RECEVIE);
            ei.Put("CREATE_BY", userinfo.UserId);
            ei.Put("MSG_TYPE", md.Type);
            ei.Put("JOB_NO", md.JobNo);
            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            ei.PutClob("CONTENT", md.ToXml());
            return ei;
        }

        public static string GetRoleSelect(string condition, string ApAttr,bool isremoveA=false)
        {
            string sql = string.Format(@"SELECT APPROVE_GROUP,GROUP_DESCP,SEQ_NO FROM APPROVE_ATTR_D WHERE U_FID=(select U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR={0} AND {1}) AND {1}
            ORDER BY SEQ_NO,APPROVE_GROUP ASC", SQLUtils.QuotedStr(ApAttr), condition);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                string approvergoup = Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                if (isremoveA)
                {
                    if(approvergoup.Equals("A"))
                    continue;
                    if(approvergoup.Equals("AM"))
                        continue;
                    if(approvergoup.Equals("AMM"))
                        continue;
                }
                if (select.Length > 0)
                {
                    select += ";";
                }
                select += Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["GROUP_DESCP"]);
            }
            select += ";Finish:Finish";
            return select;
        }
    }
}