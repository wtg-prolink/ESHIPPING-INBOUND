using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Prolink;
using Prolink.Web;
using Prolink.Data;
using Prolink.DataOperation;


namespace WebGui.Models
{
    public class BaseModel
    {
       // public static readonly System.Resources.ResourceSet myResourceSet = Resources.Locale.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
        
        /// <summary>
        /// 新增消息
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="type">消息类型</param>
        /// <param name="jobType">工作类型</param>
        /// <param name="jobNo">工作号码</param>
        /// <param name="receiveUser">接收者</param>
        /// <param name="content">消息内容</param>
        /// <param name="groupId">集團</param>
        /// <param name="companyId">公司</param>
        /// <param name="station">站別</param>
        /// <param name="userId">操作用户</param>
        /// <returns>消息的流水号ID</returns>
        public string AddMessage(string title, string type, string jobType, string jobNo, string receiveUser, string content, string groupId, string companyId, string station, string userId, DelegateConnection conn)
        {
            MessageData md = new MessageData();
            md.Type = type;
            md.Title = title;
            md.JobType = jobType;
            md.JobNo = jobNo;
            md.Content = content;
            EditInstruct ei = new EditInstruct("GFMESSAGE", EditInstruct.INSERT_OPERATION);
            ei.Put("GROUP_ID", groupId);
            string msg_id = groupId + companyId + station + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            ei.Put("MSG_ID", msg_id);
            ei.Put("RCV_CD", receiveUser);
            ei.Put("STATUS", MessageData.NOT_RECEVIE);
            ei.Put("CREATE_BY", userId);
            ei.Put("MSG_TYPE", type);
            ei.Put("JOB_NO", jobNo);
            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            ei.PutClob("CONTENT", md.ToXml());
            OperationUtils.ExecuteUpdate(ei, conn);
            return msg_id;
        }
        //sample Admin;Common;[king,milo,tim]
        public DataTable ConvRole2AcctMailList(string roleStr, string groupId, string cmp, string stn, DelegateConnection conn)
        {
            //List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, string> role = new Dictionary<string, string>();
            Dictionary<string, string> accts = new Dictionary<string, string>();
            DataTable myDt = null;
            char[] chr = {';'};
            string[] roles = roleStr.Split(chr);

            for (int i = 0; i < roles.Length; i++)
            {
                if (roles[i] == "")
                {
                    continue;
                }

                if (roles[i].IndexOf("[") < 0)
                {
                    role[roles[i]] = roles[i];
                }
                else
                {
                    string[] acctArr = roles[i].Replace("[","").Replace("]","").Split(new char[]{','});
                    for (int j = 0; j < acctArr.Length; j++)
                    {
                        accts[acctArr[j]] = acctArr[j];
                    }
                }
                
            }
            myDt = getRoleAcctsMail(role, accts, groupId, cmp, stn,conn);
            return myDt;

        }

        public DataTable getRoleAcctsMail(Dictionary<string, string> role, Dictionary<string, string> accts, string groupId, string cmp, string stn, DelegateConnection conn)
        {
            string[] list = null;
            string sql = " SELECT DISTINCT U_ID,U_EMAIL FROM SYS_ACCT WHERE 1=1 ";
            string coditions = " AND (U_ID IN ('FRISTACCT'";
            foreach (KeyValuePair<string, string> item in accts)
            {
                coditions += "," + SQLUtils.QuotedStr(item.Key);
            }
            coditions += @") OR U_ID IN (SELECT FACCT_ID
                      FROM   SYS_ACCT_ROLE
                      WHERE  FROLE_ID IN ('FRISTACCT'";



            foreach (KeyValuePair<string, string> item in role)
            {
                coditions += "," + SQLUtils.QuotedStr(item.Key);
            }
            coditions += ") ) ) AND GROUP_ID = " + SQLUtils.QuotedStr(groupId);
            coditions += " AND CMP = " + SQLUtils.QuotedStr(cmp);
            coditions += " AND STN = " + SQLUtils.QuotedStr(stn);

            DataTable acctDt = OperationUtils.GetDataTable(sql + coditions, new string[] { }, conn);


            return acctDt;
        }


        /// <summary>
        /// 獲取實體的屬性名對應的欄位名
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static string GetModelFiledName(string fn)
        {
            char[] strchar = new char[fn.Replace("_", "").Length];
            System.Text.StringBuilder sbmodel = new System.Text.StringBuilder();
            int first = 0;
            for (int i = 0; i < fn.Length; i++)
            {
                if (fn[i].ToString() == "_")
                {
                    first = 0;
                    continue;
                }
                if (first == 0)
                    sbmodel.Append(fn[i].ToString().ToUpper());
                else
                    sbmodel.Append(fn[i].ToString().ToLower());
                first++;
            }
            return sbmodel.ToString();
        }

    }
}