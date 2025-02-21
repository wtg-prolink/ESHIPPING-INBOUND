using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Utils;

namespace TrackingEDI.Business
{
    public class Parser
    {
        /// <summary>
        /// table数据缓存
        /// </summary>
        public Dictionary<string, DataTable> _dtCache = new Dictionary<string, DataTable>();
        public DataTable GetData(string sql, bool fromCache = true)
        {
            if (fromCache && _dtCache.ContainsKey(sql))
                return _dtCache[sql];
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                _dtCache[sql] = dt;
            }
            return dt;
        }

        public DataTable GetContainerByJobNo(string job_no, bool fromCache = true)
        {
            //DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT CNTR_NO FROM TKBLCNTR WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(job_no)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql = string.Format("SELECT CNTR_NO FROM TKBLCNTR WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(job_no));
            DataTable dt = GetData(sql, fromCache);
            return dt;
        }

        /// <summary>
        /// 获取货况信息  STS_CD,EDESCP,LOCATION,ISSINGLE
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public DataTable GetStatusInfo(string code, string senderType = "PROLINK")
        {
            DataTable statusDt = GetData(string.Format("SELECT STS_CD,EDESCP,LDESCP,LOCATION,ISSINGLE FROM (SELECT STS_CD FROM TKSTMP P WHERE CUST_CD={1} AND CSTS_CD={0})A OUTER APPLY (SELECT EDESCP,LDESCP,LOCATION,ISSINGLE FROM TKSTSCD S WHERE S.STS_CD=A.STS_CD)B UNION SELECT STS_CD,EDESCP,LDESCP,LOCATION,ISSINGLE FROM TKSTSCD WHERE STS_CD={0}", SQLUtils.QuotedStr(code), SQLUtils.QuotedStr(senderType)));
            return statusDt;
        }

        /// <summary>
        /// 获取提单数据
        /// </summary>
        /// <param name="MASTER_NO"></param>
        /// <returns></returns>
        public DataTable GetTkblRow(string MASTER_NO)
        {
            DataTable dt = Database.GetDataTable(string.Format("SELECT  SHIPMENT_ID,HOUSE_NO,TRAN_TYPE,U_ID,TRANS_FLAG,MASTER_NO,(SELECT TOP 1 SP.PARTY_NAME FROM TKBLPT SP WHERE SP.U_ID=TKBL.U_ID AND SP.PARTY_TYPE = 'OAG') AS ORIGIN_FORWARDER FROM TKBL WHERE MASTER_NO={0}", Prolink.Data.SQLUtils.QuotedStr(MASTER_NO)), null);
            return dt;
        }

        public DataTable GetTkblRow(string master_no, string house_no, string shipment_id, string job_no,string cmp="")
        {
            string sql = "SELECT SHIPMENT_ID,HOUSE_NO,TRAN_TYPE,U_ID,TRANS_FLAG,MASTER_NO,(SELECT TOP 1 SP.PARTY_NAME FROM TKBLPT SP WHERE SP.U_ID=TKBL.U_ID AND SP.PARTY_TYPE = 'OAG') AS ORIGIN_FORWARDER FROM TKBL WHERE 1=1";
            if (!string.IsNullOrEmpty(master_no))
                sql += " AND MASTER_NO=" + SQLUtils.QuotedStr(master_no);
            else if (!string.IsNullOrEmpty(house_no))
                sql += " AND HOUSE_NO=" + SQLUtils.QuotedStr(house_no);

            if (!string.IsNullOrEmpty(job_no))
                sql += " AND U_ID=" + SQLUtils.QuotedStr(job_no);
            if (!string.IsNullOrEmpty(shipment_id))
                sql += " AND SHIPMENT_ID=" + SQLUtils.QuotedStr(shipment_id);
            if (!string.IsNullOrEmpty(cmp))
                sql += " AND CMP=" + SQLUtils.QuotedStr(cmp);
            DataTable dt = Database.GetDataTable(sql, null);
            return dt;
        }

        public string GetBoolValue(string issingle)
        {
            if (string.IsNullOrEmpty(issingle))
                issingle = "N";
            else if ("1".Equals(issingle.ToUpper()))
                issingle = "Y";
            else if (!"Y".Equals(issingle.ToUpper()))
                issingle = "N";
            else
                issingle = "Y";
            return issingle;
        }
    }
}
