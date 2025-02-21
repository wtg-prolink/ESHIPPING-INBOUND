using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;

namespace TrackingEDI
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
            string sql = string.Format("SELECT * FROM (SELECT POL_CD,POD_CD,QUOT_TYPE,F3,L3,CARRIER,LSP_CD,RFQ_NO FROM SMQTD WHERE  QUOT_TYPE='F' AND  POL_CD={0} AND POD_CD={1} AND REGION={3} AND F3>0 AND EXISTS (SELECT 1 FROM SMQTM M WITH (NOLOCK) WHERE M.U_ID=SMQTD.U_FID AND M.TRAN_MODE='F' AND M.EFFECT_FROM<={2} AND M.EFFECT_TO>={2}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM,EFFECT_TO FROM SMQTM WHERE SMQTM.RFQ_NO=A.RFQ_NO)B ORDER BY F3,EFFECT_FROM DESC", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(pod), SQLUtils.QuotedStr(weekDay.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(region));
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
            UpdatePartys("SMSM", "SMSMPT", new string[] { "SH", "CS", "PT", "DT", "FS", "BR", "CR", "BO", "SP" }, key);
        }

        public static void UpdateSMSM(string shipmentid)
        {
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.PutExpress("POL_CD", "PPOL_CD");
            ei.PutExpress("POL_NAME", "PPOL_NAME");
            ei.PutExpress("POR_CD", "PPOR_CD");
            ei.PutExpress("POR_NAME", "PPOR_NAME");
            ei.PutExpress("POD_CD", "PPOD_CD");
            ei.PutExpress("POD_NAME", "PPOD_NAME");
            ei.PutExpress("DEST_CD", "PDEST_CD");
            ei.PutExpress("DEST_NAME", "PDEST_NAME");
            ei.PutExpress("GW", "PGW");
            ei.PutExpress("CBM", "PCBM");
            ei.PutExpress("VW", "PVW");
            ml.Add(ei);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        //public static void UpdateSMDNPartys(string key)         
        //{
        //    UpdatePartys("SMDN", "SMDNPT", new string[] { "AG", "WE", "ZE", "FC", "RE", "RO" }, key);
        //}

        public static void UpdatePartys(string table, string partyTable, string[] partyTypes, string key, string keyField = "SHIPMENT_ID", string ptField = "PARTY_TYPE", string pnField = "PARTY_NO", string pnmField = "PARTY_NAME", string pattrField = "PART_ADDR1")
        {
            MixedList ml = new MixedList();
            DataRow[] drs = null;
            string sql = string.Format("SELECT * FROM {0} WHERE {1}={2}", partyTable, keyField, SQLUtils.QuotedStr(key));
            DataTable ptDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM {0} WHERE {1}={2}", table, keyField, SQLUtils.QuotedStr(key));
            DataTable mdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string trantype = mdt.Rows[0]["TRAN_TYPE"].ToString();
            EditInstruct ei = new EditInstruct(table, EditInstruct.UPDATE_OPERATION);
            foreach (string pt in partyTypes)
            {
                string filter = string.Format("{0}={1}", ptField, SQLUtils.QuotedStr(pt));
                if ("BO".Equals(pt) || "SP".Equals(pt))
                    filter = string.Format("{0} IN ('SP','BO')", ptField);
                drs = ptDt.Select(filter);
                if (drs.Length <= 0)
                {
                    drs = new DataRow[] { ptDt.NewRow() };
                }
                switch (pt)
                {
                    case "FS":
                        if (!string.IsNullOrEmpty(drs[0][pnField].ToString()))
                        {
                            ei.Put("CARRIER", drs[0][pnField]);
                            ei.Put("CARRIER_NM", drs[0][pnmField]);
                        }
                        DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT * FROM BSCODE WHERE CD_TYPE='TCAR' AND AR_CD={0}",
                        SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(drs[0][pnField]))),
                        null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (dt.Rows.Count > 0)
                        {
                            ei.Put("CARRIER", Prolink.Math.GetValueAsString(dt.Rows[0]["CD"]));
                            ei.Put("CARRIER_NM", Prolink.Math.GetValueAsString(dt.Rows[0]["CD_DESCP"]));
                        }
                        break;
                    case "SH":
                        ei.Put("OEXPORTER", drs[0][pnField]);
                        ei.Put("OEXPORTER_NM", drs[0][pnmField]);
                        ei.Put("OEXPORTER_ADDR", drs[0][pattrField]);
                        ei.Put(pt + "_CD", drs[0][pnField]);
                        ei.Put(pt + "_NM", drs[0][pnmField]);
                        break;
                    case "CS":
                        ei.Put("OIMPORTER", drs[0][pnField]);
                        ei.Put("OIMPORTER_NM", drs[0][pnmField]);
                        ei.Put("OIMPORTER_ADDR", drs[0][pattrField]);
                        ei.Put(pt + "_CD", drs[0][pnField]);
                        ei.Put(pt + "_NM", drs[0][pnmField]);
                        break;
                    case "BO":
                    case "SP":
                        ei.Put("LSP_NO", drs[0][pnField]);
                        ei.Put("LSP_NM", drs[0][pnmField]);
                        break;
                    case "CR"://trantype
                        if ("T".Equals(trantype) || "D".Equals(trantype))
                        {
                            ei.Put("LSP_NO", drs[0][pnField]);
                            ei.Put("LSP_NM", drs[0][pnmField]);
                        }
                        ei.Put(pt + "_CD", drs[0][pnField]);
                        ei.Put(pt + "_NM", drs[0][pnmField]);
                        break;
                    default:
                        ei.Put(pt + "_CD", drs[0][pnField]);
                        ei.Put(pt + "_NM", drs[0][pnmField]);
                        break;
                }
                ei.PutKey(keyField, key);
            }
            string iorder = Prolink.Math.GetValueAsString( mdt.Rows[0]["IORDER"]);
            if (string.IsNullOrEmpty(iorder))
            {
                ei.Put("IORDER", "N");
            }
            ml.Add(ei);
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// 获取本币金额，返回汇率
        /// </summary>
        /// <param name="rateDt"></param>
        /// <param name="msg"></param>
        /// <param name="val"></param>
        /// <param name="cur"></param>
        /// <param name="total"></param>
        /// <param name="error"></param>
        public static decimal GetTotal(DataTable rateDt, List<string> msg, decimal val, string cur, ref decimal total, ref bool error, string to_cur = "")
        {
            decimal rate = 0M;
            if (string.IsNullOrEmpty(to_cur))
                to_cur = "CNY";

            if ((("RMB".Equals(cur) || "CNY".Equals(cur)) && "CNY".Equals(to_cur))
                || to_cur.Equals(cur))
            {
                rate = 1;
                total += val;
                return rate;
            }
            int type = 1;
            string msgStr = string.Empty;
            DataRow[] drs = null;
            if ("CNY".Equals(to_cur))
                drs = rateDt.Select(string.Format("FCUR={0} AND (TCUR='RMB' OR TCUR='CNY')", SQLUtils.QuotedStr(cur)), "EDATE DESC");
            else
                drs = rateDt.Select(string.Format("FCUR={0} AND TCUR={1}", SQLUtils.QuotedStr(cur), SQLUtils.QuotedStr(to_cur)), "EDATE DESC");

            if (drs.Length <= 0)
            {
                type = 2;
                if ("CNY".Equals(to_cur))
                    drs = rateDt.Select(string.Format("TCUR={0} AND (FCUR='RMB' OR FCUR='CNY')", SQLUtils.QuotedStr(cur)), "EDATE DESC");
                else
                    drs = rateDt.Select(string.Format("TCUR={0} AND FCUR={1}", SQLUtils.QuotedStr(cur), SQLUtils.QuotedStr(to_cur)), "EDATE DESC");
            }

            if (drs.Length <= 0)
            {
                error = true;
                msgStr = string.Format("无{0}对应{1}的费率", cur, to_cur);
                if (msg != null && !msg.Contains(msgStr))
                    msg.Add(msgStr);
            }
            else
            {
                rate = Prolink.Math.GetValueAsDecimal(drs[0]["EX_RATE"]);
                msgStr = string.Format("{0}对应{2}的费率{1}", cur, Get45AmtValue(rate), to_cur);
                if (msg != null && !msg.Contains(msgStr))
                    msg.Add(msgStr);
                if (type != 1)
                    rate = 1M / rate;
                total += rate * val;
            }
            if (rate == 0)
            {
                rate = 1M;
                total += rate * val;
            }
            return rate;
        }

        /// <summary>
        /// 获取金额的四舍五入的值
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal Get45AmtValue(decimal val, int decimals = 2)
        {
            return System.Math.Round(val, decimals, MidpointRounding.AwayFromZero);
        }

        public static DataTable GetRate(DataTable rateDt, DateTime billDate)
        {
            //DateTime billDate = Prolink.Math.GetValueAsDateTime(billDateStr);
            string rateFilter = string.Format("EDATE<={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            DataRow[] rateDr = rateDt.Select(rateFilter);
            DataTable rateDt1 = rateDt.Clone();
            foreach (DataRow r in rateDr)
            {
                rateDt1.ImportRow(r);
            }
            return rateDt1;
        }
    }
}
