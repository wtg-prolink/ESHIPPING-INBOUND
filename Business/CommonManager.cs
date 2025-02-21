using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business
{
    /// <summary>
    /// 通用管理器
    /// </summary>
    public class CommonManager
    {
        /// <summary>
        /// 获取最低报价
        /// </summary>
        /// <returns></returns>
        public static DataTable GetLowestPrice(int year, int week, string pol, string pod, string region)
        {
            //SELECT * FROM (SELECT POL_CD,POD_CD,QUOT_TYPE,F3,CARRIER,LSP_CD,RFQ_NO FROM SMQTD WHERE  QUOT_TYPE='F' AND  POL_CD='USLAX' AND POD_CD='CNXMN' AND
            //EXISTS (SELECT 1 FROM SMQTM M WITH (NOLOCK) WHERE M.U_ID=SMQTD.U_FID AND M.EFFECT_FROM<='2016-02-16' AND M.EFFECT_TO>='2016-02-16'))A
            // OUTER APPLY (SELECT TOP 1 EFFECT_FROM,EFFECT_TO FROM SMQTM WHERE SMQTM.RFQ_NO=A.RFQ_NO)B ORDER BY F3,EFFECT_TO DESC
            DateTime weekDay = DateTimeUtils.WeekToDate(year, week);
            string sql = string.Format("SELECT * FROM (SELECT POL_CD,POD_CD,QUOT_TYPE,F3,L3,CARRIER,LSP_CD,RFQ_NO FROM SMQTD WHERE  QUOT_TYPE='F' AND  POL_CD={0} AND POD_CD={1} AND REGION={3} AND F3>0 AND EXISTS (SELECT 1 FROM SMQTM M WITH (NOLOCK) WHERE M.U_ID=SMQTD.U_FID AND M.TRAN_MODE='F' AND M.EFFECT_FROM<={2} AND M.EFFECT_TO>={2}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM,EFFECT_TO FROM SMQTM WHERE SMQTM.RFQ_NO=A.RFQ_NO)B ORDER BY F3,EFFECT_TO DESC", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(pod), SQLUtils.QuotedStr(weekDay.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(region));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable clDT = dt.Clone();
            string filter = string.Empty;
            int l3 = 1;
            foreach (DataRow dr in dt.Rows)
            {
                string lsp_cd = Prolink.Math.GetValueAsString(dr["LSP_CD"]);
                string carrier = Prolink.Math.GetValueAsString(dr["CARRIER"]);
                if (string.IsNullOrEmpty(carrier))
                    continue;
                if (string.IsNullOrEmpty(lsp_cd))
                    continue;
                filter = string.Format("LSP_CD={0}", SQLUtils.QuotedStr(lsp_cd));
                //filter = string.Format("LSP_CD={0} AND CARRIER={1}", SQLUtils.QuotedStr(lsp_cd), SQLUtils.QuotedStr(carrier));
                if (clDT.Select(filter).Length > 0)
                    continue;
                clDT.ImportRow(dr);
                clDT.Rows[l3 - 1]["L3"] = l3;
                l3++;
                if (l3 > 5)
                    break;
            }
            return clDT;
        }

        public static void UpdateSMSMPartys(string key)
        {
            UpdatePartys("SMSM", "SMSMPT", new string[] { "SH", "CS", "PT", "DT","FS"}, key);
        }

        //public static void UpdateSMDNPartys(string key)         
        //{
        //    UpdatePartys("SMDN", "SMDNPT", new string[] { "AG", "WE", "ZE", "FC", "RE", "RO" }, key);
        //}

        public static void UpdatePartys(string table, string partyTable, string[] partyTypes, string key, string keyField = "SHIPMENT_ID", string ptField = "PARTY_TYPE", string pnField = "PARTY_NO", string pnmField = "PARTY_NAME",string pattrField="PART_ADDR1")
        {
            MixedList ml = new MixedList();
            DataRow[] drs = null;
            string sql = string.Format("SELECT * FROM {0} WHERE {1}={2}", partyTable, keyField, SQLUtils.QuotedStr(key));
            DataTable ptDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = new EditInstruct(table, EditInstruct.UPDATE_OPERATION);
            foreach (string pt in partyTypes)
            {
                string filter = string.Format("{0}={1}", ptField, SQLUtils.QuotedStr(pt));
                drs = ptDt.Select(filter);
                if (drs.Length <= 0)
                    continue;
              
                if (pt.Equals("FS"))
                {
                    ei.Put("CARRIER", drs[0][pnField]);
                    ei.Put("CARRIER_NM", drs[0][pnmField]);
                }
                else if (pt.Equals("SH"))
                {
                    ei.Put("OEXPORTER", drs[0][pnField]);
                    ei.Put("OEXPORTER_NM", drs[0][pnmField]);
                    ei.Put("OEXPORTER_ADDR", drs[0][pattrField]);
                    ei.Put(pt + "_CD", drs[0][pnField]);
                    ei.Put(pt + "_NM", drs[0][pnmField]);
                }
                else if (pt.Equals("CS"))
                {
                    ei.Put("OIXPORTER", drs[0][pnField]);
                    ei.Put("OIXPORTER_NM", drs[0][pnmField]);
                    ei.Put("OIXPORTER_ADDR", drs[0][pattrField]);
                    ei.Put(pt + "_CD", drs[0][pnField]);
                    ei.Put(pt + "_NM", drs[0][pnmField]);
                }
                else
                {
                    ei.Put(pt + "_CD", drs[0][pnField]);
                    ei.Put(pt + "_NM", drs[0][pnmField]);
                }
                ei.PutKey(keyField, key);
            }
            ml.Add(ei);
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
    }
}
