using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Xml;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using TrackingEDI.Utils;

namespace TrackingEDI.Business
{
    public class CostStatistics
    {
        static DataTable rateDt1 = null;

        public static bool StatisticsProduce(string shipmentid, string userid, string errorMsg = "")
        {
            MixedList ml = new MixedList();
            string sql = string.Format(@"SELECT CMP,GROUP_ID,COMBINE_INFO,
            (SELECT TOP 1 ISCOMBINE_BL FROM SMSM WHERE SMSM.SHIPMENT_ID=SMSMI.SHIPMENT_ID) AS ISCOMBINE_BL,
            (SELECT TOP 1 IS_SPLIT_BILL FROM SMSM WHERE SMSM.SHIPMENT_ID=SMSMI.SHIPMENT_ID) AS IS_SPLIT_BILL
            FROM SMSMI WHERE SHIPMENT_ID ={0} ", SQLUtils.QuotedStr(shipmentid));
            DataTable smdt = Database.GetDataTable(sql, null);
            if (smdt == null || smdt.Rows.Count <= 0) return true;
            string cmp = Prolink.Math.GetValueAsString(smdt.Rows[0]["CMP"]);
            string groupid = Prolink.Math.GetValueAsString(smdt.Rows[0]["GROUP_ID"]);
            string combineinfo = Prolink.Math.GetValueAsString(smdt.Rows[0]["COMBINE_INFO"]);
            string iscombinebl = Prolink.Math.GetValueAsString(smdt.Rows[0]["ISCOMBINE_BL"]);
            string issplitbill = Prolink.Math.GetValueAsString(smdt.Rows[0]["IS_SPLIT_BILL"]);

            string[] combindns = combineinfo.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

            string conditions = string.Format("DN_NO IN {0}", SQLUtils.Quoted(combindns));
            string checkposql = string.Format("SELECT COUNT(1) FROM SMDN WHERE {0} AND " +
                "EXISTS (SELECT 1 FROM SMINPO WHERE SMINPO.DN_NO=SMDN.DN_NO)", conditions);
            int pocount=Database.GetValueAsInt(checkposql);
            int dncount = combindns.Length;
            bool bypo = false;
            string costRule = "INVITEM";
            if (pocount >= dncount)
            {
                bypo = true;
                costRule = "POITEM";
            }

            XmlDocument docxml = XmlParser.LoadSQLXml("smcs.xml");
            if (bypo)
            {
                docxml = XmlParser.LoadSQLXml("smcs_po.xml");
            }
            
            string dncol = string.Format(XmlParser.GetXmlDocSql(docxml, "DNcol"), SQLUtils.QuotedStr(shipmentid));
            string smcol = string.Format(XmlParser.GetXmlDocSql(docxml, "SMcol"), SQLUtils.QuotedStr(shipmentid));
            string qcol = string.Format(XmlParser.GetXmlDocSql(docxml, "col"), SQLUtils.QuotedStr(shipmentid), dncol, smcol);
            string qcol1 = string.Format(XmlParser.GetXmlDocSql(docxml, "col1"), SQLUtils.QuotedStr(shipmentid), dncol, smcol);

            string smdnpSql = string.Format(XmlParser.GetXmlDocSql(docxml, "dnpsql"), SQLUtils.QuotedStr(shipmentid), qcol);

            if ("C".Equals(iscombinebl))
            {
                smdnpSql = string.Format(XmlParser.GetXmlDocSql(docxml, "combine"), SQLUtils.QuotedStr(shipmentid), qcol, dncol);
            }
            else if ("S".Equals(issplitbill))
            {
                string[] dnitem = combineinfo.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (dnitem.Length <= 0) return true;
                smdnpSql = string.Format(XmlParser.GetXmlDocSql(docxml, "split"), SQLUtils.QuotedStr(shipmentid), qcol, SQLUtils.Quoted(dnitem), smcol);

            }
            smdnpSql += string.Format(XmlParser.GetXmlDocSql(docxml, "bddsql"), SQLUtils.QuotedStr(shipmentid), qcol1);


            DataTable smdnpDt = Database.GetDataTable(smdnpSql, null);

            DataTable smbidDt = Database.GetDataTable(string.Format(@"SELECT (SELECT TOP 1 STATUS  FROM SMBIM WHERE SMBIM.U_ID =U_FID ) AS BIM_STATUS,
(SELECT TOP 1 APPROVE_TO  FROM SMBIM WHERE SMBIM.U_ID =U_FID ) AS APPROVE_TO,* FROM SMBID WHERE SHIPMENT_ID = {0} AND CMP={1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cmp)), null);

            DataTable chgCodeDt = Database.GetDataTable(string.Format(@"SELECT DISTINCT CHG_CD, CHG_CD+' '+CHG_DESCP CHGNAME FROM SMCHG WHERE IO_TYPE='I' AND GROUP_ID ={0} AND (CMP='*' OR CMP={1})
                            ORDER BY CHG_CD ASC", SQLUtils.QuotedStr(groupid), SQLUtils.QuotedStr(cmp)), null);

            rateDt1 = Database.GetDataTable("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' ORDER BY EDATE", null);

            DataTable LevelDt = Database.GetDataTable(string.Format(@"SELECT * FROM BSCODE WHERE CD_TYPE='TXCD' AND GROUP_ID ={0} 
            AND (CMP='*' OR CMP={1})", SQLUtils.QuotedStr(groupid), SQLUtils.QuotedStr(cmp)), null);
            sql = string.Format("SELECT DNP_ID,BID_ID,U_ID,POST_FLAG FROM SMCSI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable smcsDt = Database.GetDataTable(sql, null);

            sql = string.Format("SELECT CARD_NO,USER_ID,USER_NM,STN,CMP FROM EST_HANDLER WHERE CMP={0}", SQLUtils.QuotedStr(cmp));
            DataTable esthDt = Database.GetDataTable(sql, null);
            List<string> csUidList = new List<string>();
            foreach (DataRow csDr in smcsDt.Rows)
            {
                string csuid = Prolink.Math.GetValueAsString(csDr["U_ID"]);
                if (!csUidList.Contains(csuid))
                    csUidList.Add(csuid);
            }
            string scasql = string.Format("SELECT * FROM SCALLO WHERE CMP={0}", SQLUtils.QuotedStr(cmp));
            DataTable scalloDt = OperationUtils.GetDataTable(scasql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm.Add("bid", smbidDt);
            parm.Add("level", LevelDt);
            parm.Add("userid", userid);
            parm.Add("smcs", smcsDt);
            parm.Add("uidlist", csUidList);
            parm.Add("est_handler", esthDt);
            parm.Add("costRule", costRule);
            parm.Add("scalloDt", scalloDt);
            BaseParser.RegisterEditInstructFunc("CSmapping", CsEditInstruct);
            BaseParser baseparser = new BaseParser();
            baseparser.ParseEditInstruct(smdnpDt, "CSmapping", ml, parm);
            Delete(csUidList, ml);
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    errorMsg = DateTime.Now + "," + ex.ToString();
                    OperationUtils.Logger.WriteLog(ex);
                    return false;
                }
            }
            return true;
        }

        public static string CsEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            DataTable smbidDt = null;
            if (parm.ContainsKey("bid"))
                smbidDt = parm["bid"] as DataTable;
            DataTable LevelDt = null;
            if (parm.ContainsKey("level"))
                LevelDt = parm["level"] as DataTable;
            DataTable estHandler = parm["est_handler"] as DataTable;
            DataTable smcsDt = parm["smcs"] as DataTable;
            List<string> uidList = parm["uidlist"] as List<string>;
            string userid = "";
            if (parm.ContainsKey("userid"))
                userid = Prolink.Math.GetValueAsString(parm["userid"]);

            string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            string chguid = Prolink.Math.GetValueAsString(dr["CHG_U_ID"]);
            string dnpuid = Prolink.Math.GetValueAsString(dr["U_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
            string stn = Prolink.Math.GetValueAsString(dr["STN"]);
            string tranType = dr.Table.Columns.Contains("TRAN_TYPE") ? Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]) : string.Empty;
            string CargoType = dr.Table.Columns.Contains("CARGO_TYPE") ? Prolink.Math.GetValueAsString(dr["CARGO_TYPE"]) : string.Empty;
            string BandType = dr.Table.Columns.Contains("BAND_TYPE") ? Prolink.Math.GetValueAsString(dr["BAND_TYPE"]) : string.Empty;
            ei.Put("LOCATION", cmp);

            DateTime evendate = Prolink.Math.GetValueAsDateTime(dr["SM_ETD"]);
            if (dr["ATD"] != null && dr["ATD"] != DBNull.Value)
            {
                evendate = Prolink.Math.GetValueAsDateTime(dr["ATD"]);
            }
            DataTable rateDt = rateDt1;
            if (evendate.CompareTo(DateTime.MinValue) > 0)
                rateDt = CommonManager.GetRate(rateDt1, evendate);

            DataRow[] smbidrows = smbidDt.Select(string.Format("U_ID={0}", SQLUtils.QuotedStr(chguid)));
            DataTable smbidrowdt = smbidDt.Clone();
            foreach (DataRow dr1 in smbidrows)
            {
                smbidrowdt.ImportRow(dr1);
            }
            DataRow[] estDrs = estHandler.Select(string.Format("STN={0} AND CMP={1} AND CARD_NO IS NOT NULL",
                SQLUtils.QuotedStr(stn), SQLUtils.QuotedStr(cmp)));
            if (estDrs.Length > 0)
            {
                string cardNo = Prolink.Math.GetValueAsString(estDrs[0]["CARD_NO"]);
                ei.Put("CARD_NO", cardNo);
            }
            //string amtfield = "QAMT"; //Bamt实际，Qamt预提，EAMT预估
            string[] amtfields = { "BAMT", "QAMT", "EAMT" };
            DataRow[] csDrs = smcsDt.Select(string.Format("DNP_ID={0} AND BID_ID={1}",
                SQLUtils.QuotedStr(dnpuid), SQLUtils.QuotedStr(chguid)));
            foreach (DataRow csDr in csDrs)
            {
                string uid = Prolink.Math.GetValueAsString(csDr["U_ID"]);
                string postFlag = Prolink.Math.GetValueAsString(csDr["POST_FLAG"]);
                if ("Y".Equals(postFlag))
                    amtfields = new string[] { "BAMT" };
                if (uidList.Contains(uid))
                    uidList.Remove(uid);
                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                ei.PutKey("U_ID", uid);
            }
            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
            {
                ei.Put("CREATE_BY", userid);
                ei.PutDate("CREATE_DATE", DateTime.Now);
                ei.Put("U_ID", Guid.NewGuid().ToString("N"));
                ei.Put("DNP_ID", dnpuid);
                ei.Put("BID_ID", chguid);
                string key = dnpuid + ";" + chguid;
                if (!uidList.Contains(key))
                    uidList.Add(key);
            }
            string cur = "QCUR";
            string tocur = "USD";

            DateTime ata = Prolink.Math.GetValueAsDateTime(dr["ATA"]);
            DateTime eta = Prolink.Math.GetValueAsDateTime(dr["ETA"]);
            DateTime weekDate = ata > DateTime.MinValue ? ata : eta;
            if (weekDate > DateTime.MinValue)
            {
                ei.Put("CS_YEAR", weekDate.Year);
                ei.Put("CS_MONTH", weekDate.Month);
                int quarter = (weekDate.Month - 1) / 3 + 1;
                ei.Put("CS_QUARTER", quarter);
                ei.Put("CS_WEEK", DateTimeUtils.WeekOfYear(weekDate));
            }

            DataTable scalloDt = null;
            if (parm.ContainsKey("scalloDt"))
                scalloDt = parm["scalloDt"] as DataTable;
            

            foreach (string amtfield in amtfields)
            {
                foreach (DataRow dr1 in smbidrows)
                {
                    if (smbidrows.Length <= 0) continue;
                    decimal chgqty1 = Prolink.Math.GetValueAsDecimal(dr1[amtfield]);
                    string approvestatus = Prolink.Math.GetValueAsString(dr1["APPROVE_STATUS"]);
                    string bimstatus = Prolink.Math.GetValueAsString(dr1["BIM_STATUS"]);
                    string approveto = Prolink.Math.GetValueAsString(dr1["APPROVE_TO"]);
                    bool approvecount = false;
                    if ("E".Equals(bimstatus) || "F".Equals(bimstatus) || ("D".Equals(bimstatus) && !"A".Equals(approveto)))
                        approvecount = true;

                    if ("QAMT".Equals(amtfield) && chgqty1 <= 0 && !"Y".Equals(approvestatus))
                        return BaseParser.ERROR;
                    string condition = "1=1";
                    if ("BAMT".Equals(amtfield))
                    {
                        condition = "APPROVE_STATUS='Y'";
                    }

                    chgqty1 = GetSmbidamt(smbidrowdt, amtfield, condition);
                    decimal chgqty = chgqty1;
                    switch (amtfield)
                    {
                        case "BAMT": cur = "CUR"; break;
                        case "EAMT": cur = "ECUR"; break;
                        case "QAMT": cur = "QCUR"; break;
                    }
                    string curreny = Prolink.Math.GetValueAsString(dr1[cur]);

                    //获取DN体积所占的比例
                    Tuple<string,decimal> percentTuple= GetPercentByRule(dr, scalloDt);
                    decimal percent = percentTuple.Item2;
                    string caltype = percentTuple.Item1;
                    ei.Put("RULE_TYPE", caltype);
                    decimal Value = Prolink.Math.GetValueAsDecimal(System.Math.Round(Prolink.Math.GetValueAsDouble(chgqty * percent), 2));
                    bool error = false;
                    decimal Value1 = 0M;
                    CommonManager.GetTotal(rateDt, null, Value, curreny, ref Value1, ref error, tocur);
                    decimal tax = Prolink.Math.GetValueAsDecimal(dr1["TAX"]);
                    decimal qtax = Prolink.Math.GetValueAsDecimal(dr1["QTAX"]);
                    decimal etax = Prolink.Math.GetValueAsDecimal(dr1["ETAX"]);
                    switch (amtfield)
                    {
                        case "BAMT":
                            if (approvecount)
                            {
                                ei.Put("OACOST", Value);
                                ei.Put("ACOST", Value1);
                                decimal naamt = Value / (1 + tax / 100);
                                ei.Put("NAAMT", naamt);
                                ei.Put("ATAMT", Value - naamt);
                                ei.Put("OCRNCY", curreny);
                                ei.Put("ATAX", tax);
                                ei.Put("ALEVEL", GetTxcdValue(LevelDt, tax));
                            }
                            break;
                        case "EAMT":
                            if (DateTime.Now.Year >= weekDate.Year && DateTime.Now.Month > weekDate.Month)
                                break;
                            ei.Put("OECOST", Value);
                            ei.Put("ECOST", Value1);
                            decimal neamt = Value / (1 + etax / 100);
                            ei.Put("NEAMT", neamt);
                            ei.Put("ETAMT", Value - neamt);
                            ei.Put("OECRNCY", curreny);
                            ei.Put("ETAX", etax);
                            ei.Put("ELEVEL", GetTxcdValue(LevelDt, etax));
                            break;
                        case "QAMT":
                            ei.Put("OQCOST", Value);
                            ei.Put("QCOST", Value1);
                            decimal nqamt = Value / (1 + qtax / 100);
                            ei.Put("NQAMT", nqamt);
                            ei.Put("QTAMT", Value - nqamt);
                            ei.Put("OQCRNCY", curreny);
                            ei.Put("QTAX", qtax);
                            ei.Put("QLEVEL", GetTxcdValue(LevelDt, qtax));
                            break;
                    }
                    string repay = Prolink.Math.GetValueAsString(dr1["REPAY"]);
                    string chgCd = Prolink.Math.GetValueAsString(dr1["CHG_CD"]);
                    ei.Put("CHG_CODE", chgCd);
                    ei.Put("CHG_DESCP", dr1["CHG_DESCP"]);
                    ei.Put("LSP_NO", dr1["LSP_NO"]);
                    ei.Put("LSP_NM", dr1["LSP_NM"]);
                    ei.Put("CHG_TYPE", dr1["CHG_TYPE"]);

                    if (string.IsNullOrEmpty(repay))
                    {
                        string sql = string.Format(@"SELECT REPAY FROM SMCHG WHERE CMP={0} AND (TRAN_MODE={1} OR TRAN_MODE='O') AND IO_TYPE='I' AND CHG_CD={2}",
                            SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(tranType), SQLUtils.QuotedStr(chgCd));
                        repay = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    ei.Put("CHG_LEVEL", repay);
                }
            }
            ei.Put("BILL_TO", ei.Get("RE_CD"));

            if (ei.IsExist("SUB_BG_NM"))
            {
                string subbgnm = ei.Get("SUB_BG_NM");
                if (!string.IsNullOrEmpty(subbgnm))
                {
                    string[] bubg = subbgnm.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (bubg.Length > 1)
                    {
                        ei.Put("BU", bubg[0]);
                        ei.Put("BG", bubg[1]);
                    }
                }
            }
            if (ei.IsExist("DEST"))
            {
                string dest = ei.Get("DEST");
                if (!string.IsNullOrEmpty(dest) && dest.Length >= 2)
                {
                    ei.Put("COUNTRY", dest.Substring(0, 2));
                    string region = "";
                    Budget.UploadBudget.CheckPortGroup(dest, ref region);
                    ei.Put("REGION_GROUP", region);
                }
            }
            int floatintLen = 3;

            decimal feu = 0M;
            if ("T".Equals(tranType) && "A".Equals(CargoType) && ("Y".Equals(BandType) || "N".Equals(BandType)))
            {
                feu = GetHtmlFeuByT(dr);
                feu = System.Math.Round(feu, floatintLen);
            }
            else
            {
                feu = GetHtmlFeu(dr, floatintLen, Prolink.Math.GetValueAsString(dr["GROUP_ID"]), Prolink.Math.GetValueAsString(dr["CMP"]));
            }
            ei.Put("FEU", feu);

            if (ei.IsExist("CARRIER_ABBR"))
            {
                string carrierabbr = ei.Get("CARRIER_ABBR");
                if (!string.IsNullOrEmpty(carrierabbr))
                {
                    string sql = string.Format(" SELECT AR_CD FROM BSCODE WHERE CD_TYPE = 'TCAR' AND CD ={0} ", SQLUtils.QuotedStr(carrierabbr));
                    string arcd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ei.Put("CARRIER", arcd);
                }
            }
            string costRule = parm["costRule"] as string;
            ei.Put("COST_RULE", costRule);
            return string.Empty;
        }

        private static Tuple<string, decimal> GetPercentByRule(DataRow dr,DataTable scalloDt)
        {
            if (scalloDt == null || scalloDt.Rows.Count == 0)
            {
                Decimal mincbm = Prolink.Math.GetValueAsDecimal(dr["MINCBM"]);
                return GetPercentByCbm(dr, mincbm);
            }
            DataRow scdr= scalloDt.Rows[0];
            Dictionary<string,int> map = new Dictionary<string,int>();
            int bycbm = Prolink.Math.GetValueAsInt(scdr["BY_CBM"]);
            bycbm = bycbm > 0 ? bycbm : 10000;
            map.Add("CBM",bycbm);
            int bygw = Prolink.Math.GetValueAsInt(scdr["BY_GW"]);
            bygw = bygw > 0 ? bygw : 10000;
            map.Add("GW", bygw);
            int bynm = Prolink.Math.GetValueAsInt(scdr["BY_NW"]);
            bynm = bynm > 0 ? bynm : 10000;
            map.Add("NW", bynm);
            int byvalue = Prolink.Math.GetValueAsInt(scdr["BY_VALUE"]);
            byvalue = byvalue > 0 ? byvalue : 10000;
            map.Add("VALUE", byvalue);
            var sortedDictionary = map.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            decimal percent = 1;
            foreach (KeyValuePair<string, int> pair in sortedDictionary)
            {
                string field = string.Empty;
                string ttlfiled = string.Empty;
                string minfiled = string.Empty;
                switch (pair.Key)
                {
                    case "CBM":
                        field = "CBM";
                        ttlfiled = "SM_CBM";
                        minfiled = "MINCBM";
                        break;
                    case "GW":
                        field = "GW";
                        ttlfiled = "SM_GW";
                        minfiled = "MINGW";
                        break;
                    case "NW":
                        field = "NW";
                        ttlfiled = "SM_NW";
                        minfiled = "MINNW";
                        break;
                    case "VALUE":
                        field = "VALUE1";
                        ttlfiled = "GVALUE";
                        minfiled = "VALUE1";
                        break;
                }
                if (Prolink.Math.GetValueAsDecimal(dr[minfiled]) <= 0)
                {
                    continue;
                }

                decimal ttlvalue = Prolink.Math.GetValueAsDecimal(dr[ttlfiled]);
                decimal value = Prolink.Math.GetValueAsDecimal(dr[field]);
                if (ttlvalue > 0)
                {
                    percent = value / ttlvalue;
                }
                return new Tuple<string, decimal>(pair.Key, percent);
            }
            return new Tuple<string, decimal>("", percent);
        }
        public static Tuple<string, decimal> GetPercentByCbm(DataRow dr, decimal mincbm)
        {
            Decimal ttlcbm = Prolink.Math.GetValueAsDecimal(dr["SM_CBM"]);
            Decimal cbm = Prolink.Math.GetValueAsDecimal(dr["CBM"]);
            string calType = "CBM";
            if (mincbm <= 0)
            {
                ttlcbm = Prolink.Math.GetValueAsDecimal(dr["GVALUE"]);
                cbm = Prolink.Math.GetValueAsDecimal(dr["VALUE1"]);
                calType = "Value";
            }
            Decimal percent = 1;
            if (ttlcbm > 0)
            {
                percent = cbm / ttlcbm;
            }
            return new Tuple<string, decimal>(calType, percent);
        }

        public static string GetTxcdValue(DataTable dt, decimal code)
        {
            string cddescp = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    decimal cd = Prolink.Math.GetValueAsDecimal(dr["CD"]);
                    if (cd == code)
                    {
                        cddescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                        break;
                    }
                }
            }
            return cddescp;
        }

        public static decimal GetSmbidamt(DataTable smbiddt, string fieldname, string condition = "1=1")
        {
            return Prolink.Math.GetValueAsDecimal(smbiddt.Compute("Sum(" + fieldname + ")", condition));
        }

        public static void Delete(List<string> uidList, MixedList ml)
        {
            if (uidList.Count <= 0)
                return;
            List<string> list = new List<string>();
            string sql = "";
            foreach (string uid in uidList)
            {
                if (!uid.Contains(";"))
                {
                    list.Add(uid);
                    continue;
                }
                string[] keys = uid.Split(';');
                sql = string.Format("DELETE FROM SMCSI WHERE DNP_ID={0} AND BID_ID={1}", SQLUtils.QuotedStr(keys[0]), SQLUtils.QuotedStr(keys[1]));
                ml.Insert(0, sql);
            }
            if (list.Count <= 0)
                return;
            sql = string.Format("DELETE FROM SMCSI WHERE U_ID IN {0}", SQLUtils.Quoted(list.ToArray()));
            ml.Insert(0, sql);
        }

        public static decimal TrunCrncy(decimal amt, string cmp, string cur)
        {
            string tocur = "USD";
            string rateFilter = string.Format(" WHERE EDATE<={0} and ETYPE='M' AND TCUR={1}",
                SQLUtils.QuotedStr(TrackingEDI.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, cmp).ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(tocur));
            DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE {0} ORDER BY EDATE", rateFilter), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<string> msglist = new List<string>();
            bool error = false;
            decimal ContactRateTemp = 0M;
            decimal rate = GetTotal(rateDt, msglist, amt, cur, ref ContactRateTemp, ref error, tocur);
            return ContactRateTemp;
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

        public static void SetCStask(string shipmentid, string groupid, string cmp, string userid, MixedList ml = null)
        {
            if (string.IsNullOrEmpty(shipmentid)) return;
            EditInstruct ei = new EditInstruct("CS_ITASK", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", Guid.NewGuid().ToString("N"));
            ei.Put("GROUP_ID", groupid);
            ei.Put("CMP", cmp);
            ei.Put("STN", "*");
            ei.Put("CREATE_BY", userid);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("SHIPMENT_ID", shipmentid);

            if (ml == null)
            {
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else
            {
                ml.Add(ei);
            }
        }
        public static decimal GetHtmlFeuByT(DataRow dr)
        {
            decimal qty = Prolink.Math.GetValueAsDecimal(dr["QTY"]);
            decimal cntrstdqty = Prolink.Math.GetValueAsDecimal(dr["CNTR_STD_QTY"]);
            return cntrstdqty != 0 ? qty / cntrstdqty : 0;
        }

        public static decimal GetHtmlFeu(DataRow dr, int floatintLen, string groupid, string cmp)
        {

            string bsCodeTypes = "'TCGT','TRGN','TNT','TCAR','VERP','CART'";
            DataTable baseCodeDt = MailTemplate.GetBaseData(bsCodeTypes, groupid, cmp);
            string cnttype = Prolink.Math.GetValueAsString(dr["CNT_TYPE"]);
            string ocnttype = Prolink.Math.GetValueAsString(dr["OCNT_TYPE"]);
            decimal othercontainer = 0M;
            if (!string.IsNullOrEmpty(cnttype))
            {
                othercontainer += Prolink.Math.GetValueAsDecimal(MailTemplate.GetBaseCodeArCdValue(baseCodeDt, "VERP", cnttype)) * Prolink.Math.GetValueAsInt(dr["CNT_NUMBER"]);
            }
            if (!string.IsNullOrEmpty(ocnttype))
            {
                othercontainer += Prolink.Math.GetValueAsDecimal(MailTemplate.GetBaseCodeArCdValue(baseCodeDt, "VERP", ocnttype)) * Prolink.Math.GetValueAsInt(dr["OCNT_NUMBER"]);
            }
            var htmlfeu = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT20"]) / 2 + Prolink.Math.GetValueAsDecimal(dr["CNT40"]) + Prolink.Math.GetValueAsDecimal(dr["CNT40HQ"]) + othercontainer) * GetPerCent(dr), floatintLen);
            return htmlfeu;
        }

        public static decimal GetPerCent(DataRow dr)
        {
            string combineother = dr.Table.Columns.Contains("COMBINE_OTHER") ? Prolink.Math.GetValueAsString(dr["COMBINE_OTHER"]) : string.Empty;
            Decimal combincbm = Prolink.Math.GetValueAsDecimal(dr["TCBM"]);
            Decimal ttlcbm = Prolink.Math.GetValueAsDecimal(dr["SM_CBM"]);
            Decimal combinepercent = 1;
            if ("Y".Equals(combineother))
            {
                if (combincbm > 0 && combincbm > ttlcbm)
                {
                    combinepercent = ttlcbm / combincbm;
                }
            }

            Decimal mincbm = Prolink.Math.GetValueAsDecimal(dr["MINCBM"]);
            Decimal cbm = Prolink.Math.GetValueAsDecimal(dr["CBM"]);

            if (mincbm <= 0)
            {
                cbm = dr.Table.Columns.Contains("VALUE1") ? Prolink.Math.GetValueAsDecimal(dr["VALUE1"]) : Prolink.Math.GetValueAsDecimal(dr["AMOUNT1"]);
                ttlcbm = Prolink.Math.GetValueAsDecimal(dr["GVALUE"]);
            }
            if (ttlcbm == 0)
            {
                return 0;
            }
            Decimal percent = cbm / ttlcbm;
            percent = percent * combinepercent;
            return percent;

        }
    }
}
