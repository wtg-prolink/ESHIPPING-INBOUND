using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
    public class SplitSmbidHelper : BaseParser
    {
        public string SMTableName { get; set; }
        public string DNTableName { get; set; }
        public string DNPTableName { get; set; }
        public string BankType { get; set; }
        public string BillUid { get; set; }
        public string Cmp { get; set; }
        public string BillTo { get; set; }
        public string LspNo { get; set; }
        public string Bu { get; set; }
        public string DnType { get; set; }
        public string Distribute { get; set; }
        public DataTable Smbid { get; set; }
        public string ColName { get; set; }
        string Mapping = "SMBIDtoSMBIDDN";
        static string[] DecimalColList = { "NAAMT", "ATAMT", "NEAMT", "QAMT", "BAMT", "LAMT", "QLAMT", "EAMT" };
        public SplitSmbidHelper(string billUid)
        {
            DataTable smbim = GetSMBIM(billUid);
            if (smbim.Rows.Count > 0)
            {
                BankType = Prolink.Math.GetValueAsString(smbim.Rows[0]["BANK_TYPE"]);
                Cmp = Prolink.Math.GetValueAsString(smbim.Rows[0]["CMP"]);
                BillTo = Prolink.Math.GetValueAsString(smbim.Rows[0]["BILL_TO"]);
                LspNo = Prolink.Math.GetValueAsString(smbim.Rows[0]["LSP_NO"]);
                BillUid = billUid;
            }
            SMTableName = "SMSMI";
            DNTableName = "SMIDN";
            DNPTableName = "SMIDNP";
            ColName = "ISNULL(COMBINE_INFO,DN_NO) COMBINE_INFO,CBM,GVALUE";
            SetCodeInfo();
            SetDistribute();
            Smbid = GetSMBID(billUid);
        }

        public void DoSplitSMBID()
        {

            List<string> shipmetIdList = new List<string>();
            foreach (DataRow dr in Smbid.Rows)
            {
                string shipment = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (shipmetIdList.Contains(shipment) || string.IsNullOrEmpty(shipment))
                    continue;
                shipmetIdList.Add(shipment);
            }
            if (shipmetIdList.Count <= 0)
                return;
            MixedList ml = new MixedList();
            EditInstruct delei = new EditInstruct("SMBID_DN", EditInstruct.DELETE_OPERATION);
            delei.PutKey("U_FID", BillUid);
            ml.Add(delei);
            string sql = "";
            foreach (string shipment in shipmetIdList)
            {
                sql = string.Format(@"SELECT {2} FROM {1} WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment), SMTableName, ColName);
                DataTable smsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smsmdt.Rows.Count <= 0) continue;

                string combineInfo = Prolink.Math.GetValueAsString(smsmdt.Rows[0]["COMBINE_INFO"]);
                string[] dnNoList = combineInfo.Split(',');
                DataTable dnDt = GetDnInfoByDnList(dnNoList);
                if (dnDt.Rows.Count > 0)
                {
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    parm["combineInfo"] = combineInfo;
                    decimal minCbm = Prolink.Math.GetValueAsDecimal(dnDt.Rows[0]["CBM"]);
                    for (int i = 0; i < dnDt.Rows.Count; i++)
                    {
                        DataRow[] rows = Smbid.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment)));
                        if (rows == null || rows.Length == 0) continue;

                        DataTable newTable = rows[0].Table.Clone();
                        foreach (DataRow row in rows)
                        {
                            newTable.ImportRow(row);
                        }
                        decimal percent = dnNoList.Length > 1 ? GetDNPercent(dnDt.Rows[i], smsmdt, minCbm) : 1;
                        parm["percent"] = percent;
                        parm["dnDataRow"] = dnDt.Rows[i];
                        parm["lastDn"] = i == dnDt.Rows.Count - 1;
                        parm["distribute"] = Distribute;
                        parm["dnType"] = DnType;
                        parm["bu"] = Bu;
                        parm["bankType"] = BankType;
                        RegisterEditInstructFunc(Mapping, SmbiddnEditInstruct);
                        ParseEditInstruct(newTable, Mapping, ml, parm);
                    }
                }
            }
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

        }

        string SmbiddnEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            decimal percent = Prolink.Math.GetValueAsDecimal(parm["percent"]);
            string combineInfo = Prolink.Math.GetValueAsString(parm["combineInfo"]);
            string bankType = Prolink.Math.GetValueAsString(parm["bankType"]);
            string bu = Prolink.Math.GetValueAsString(parm["bu"]);
            string dnType = Prolink.Math.GetValueAsString(parm["dnType"]);
            string distribute = Prolink.Math.GetValueAsString(parm["distribute"]);
            int floatintLen = 2;
            DataRow dnDr = (DataRow)parm["dnDataRow"];
            string dnNo = Prolink.Math.GetValueAsString(dnDr["DN_NO"]);
            DateTime postFlagDate = Prolink.Math.GetValueAsDateTime(dnDr["POST_FLAG_DATE"]);
            if (postFlagDate <= DateTime.MinValue)
                postFlagDate = Prolink.Math.GetValueAsDateTime(dnDr["SM_ETD"]);
            if(postFlagDate <= DateTime.MinValue)
                postFlagDate = Prolink.Math.GetValueAsDateTime(dr["CREATE_DATE"]);
            ei.Put("U_ID", Guid.NewGuid().ToString());
            ei.Put("BANK_TYPE", bankType);//GetBankType()
            ei.Put("BU", bu);
            ei.Put("DN_TYPE", dnType);
            ei.Put("DN_NO", dnNo);
            ei.Put("COMBINE_INFO", combineInfo);
            ei.Put("COST_CENTER", "");
            ei.Put("PROFIT_CENTER", "");
            ei.Put("DISTRIBUTE", distribute);
            ei.Put("REFKEY_TWO", "");
            ei.Put("POST_MONTH", postFlagDate.ToString("yyyyMM"));
            ei.Put("FSSP_ESTNO", "");

            decimal bamt = Prolink.Math.GetValueAsDecimal(ei.Get("BAMT"));
            decimal tax = Prolink.Math.GetValueAsDecimal(ei.Get("TAX"));
            tax = 0;
            decimal naamt = GetNaValue(bamt, tax, floatintLen); //Math.Round(bamt / (1 + tax / 100), 2, MidpointRounding.AwayFromZero);
            decimal atamt = GetAtValue(bamt, tax, floatintLen);// Math.Round(bamt / (1 + tax / 100) * (tax / 100), 2, MidpointRounding.AwayFromZero);
            ei.Put("NAAMT", naamt);
            ei.Put("ATAMT", atamt);
            ei.Put("ALEVEL", GetTxcd(tax));

            decimal eamt = Prolink.Math.GetValueAsDecimal(ei.Get("EAMT"));
            decimal eTax = Prolink.Math.GetValueAsDecimal(ei.Get("ETAX"));
            ei.Put("ELEVEL", GetTxcd(eTax));
            //decimal etamt = GetAtValue(bamt, eTax, floatintLen);
            decimal enamat = GetNaValue(eamt, eTax, floatintLen); //Math.Round(eamt / (1 + eTax / 100), 2, MidpointRounding.AwayFromZero);
            decimal eatamt = GetAtValue(eamt, eTax, floatintLen); //Math.Round(eamt / (1 + eTax / 100) * (eTax / 100), 2, MidpointRounding.AwayFromZero);
            ei.Put("NEAMT", enamat);

            if (percent < 1)
            {
                string bidUid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                SetAmtDt(parm, ei, bidUid);
                naamt = GetNaValue(naamt * percent, 0, floatintLen);
                atamt = GetNaValue(atamt * percent, 0, floatintLen);
                bamt = GetNaValue(bamt * percent, 0, floatintLen);
                eamt = GetNaValue(eamt * percent, 0, floatintLen);
                enamat = GetNaValue(enamat * percent, 0, floatintLen);
                eatamt = GetNaValue(eatamt * percent, 0, floatintLen);
                ei.Put("NAAMT", naamt);//未税请款=请款未税金额
                ei.Put("ATAMT", atamt);//税额

                ei.Put("NEAMT", enamat);

                ei.Put("QAMT", GetNaValue(Prolink.Math.GetValueAsDecimal(ei.Get("QAMT")) * percent, 0, floatintLen));//预提金额   報價金額
                ei.Put("BAMT", bamt);//请款金额
                ei.Put("LAMT", GetNaValue(Prolink.Math.GetValueAsDecimal(ei.Get("LAMT")) * percent, 0, floatintLen));//本地请款金额
                ei.Put("QLAMT", GetNaValue(Prolink.Math.GetValueAsDecimal(ei.Get("QLAMT")) * percent, 0, floatintLen));//本地预提金额
                ei.Put("EAMT", eamt);//预估费用
                ResetAmtDt(parm, ei, bidUid);
            }
            ei.Put("PRE_NAAMT", naamt - enamat); //O.预估与请款差异未税金额(SMBID_DN.PRE_NAAMT)”栏位。计算逻辑 = 未税请款 - 未税预估。
            ei.Put("PRE_AAMT", atamt - eatamt);//P.	预估与请款差异税额(SMBID_DN. PRE_AAMT)”栏位。计算逻辑=请款税额-预估税额。
            ei.Put("PRD_SUM", bamt - eamt); //Q.	预估与请款差异价税合计(SMBID_DN. PRD_SUM)”栏位。计算逻辑=请款金额-原币预估（预估单里对应请款金额栏位名称为原币请款）。
            ei.Put("ETAMT", eamt - enamat);//预估税额=预估费用-未税预估=预估费用/(1+预估税率)*预估税率

            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="tax"></param>
        /// <param name="floatintLen"></param>
        /// <returns></returns>
        public static decimal GetNaValue(decimal val, decimal tax, int floatintLen)
        {
            return Math.Round(val / (1 + tax / 100), floatintLen, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="tax"></param>
        /// <param name="floatintLen"></param>
        /// <returns></returns>
        public static decimal GetAtValue(decimal val, decimal tax, int floatintLen)
        {
            return Math.Round(val / (1 + tax / 100) * (tax / 100), 2, MidpointRounding.AwayFromZero);
        }

        public static DataTable GetSMBID(string billUid)
        {
            string sql = string.Format(@"SELECT * FROM SMBID WHERE U_FID={0} AND EXISTS (SELECT 1 FROM SMBIM WHERE U_ID={0})",
                SQLUtils.QuotedStr(billUid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static DataTable GetSMBIM(string billUid)
        {
            string sql = string.Format(@"SELECT LSP_NO,BANK_INFO,TPV_DEBIT_NO,CMP,BANK_TYPE,BILL_TO FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(billUid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }


        public static decimal GetDNPercent(DataRow dnDr, DataTable smsmdt, decimal minCbm)
        {
            if (smsmdt.Rows.Count <= 0) return 0;
            decimal smsmCbm = Prolink.Math.GetValueAsDecimal(smsmdt.Rows[0]["CBM"]);
            decimal smsmGvalue = Prolink.Math.GetValueAsDecimal(smsmdt.Rows[0]["GVALUE"]);
            decimal amount1 = Prolink.Math.GetValueAsDecimal(dnDr["AMOUNT1"]);
            decimal dnCbm = Prolink.Math.GetValueAsDecimal(dnDr["CBM"]);

            return minCbm <= 0 ? amount1 / smsmGvalue : dnCbm / smsmCbm;
        }

        public void SetAmtDt(Dictionary<string, object> parm, EditInstruct ei, string uid)
        {
            if (!parm.ContainsKey("amtDt"))
                parm.Add("amtDt", InitAmtDataTable());
            DataTable amtDt = (DataTable)parm["amtDt"];
            DataRow[] amtDrs = amtDt.Select("U_ID=" + SQLUtils.QuotedStr(uid));
            if (amtDrs.Length > 0)
                return;
            DataRow dr = amtDt.NewRow();
            dr["U_ID"] = uid;
            foreach (string colName in DecimalColList)
            {
                dr[colName] = Prolink.Math.GetValueAsDecimal(ei.Get(colName));
            }
            amtDt.Rows.Add(dr.ItemArray);
        }

        public DataTable InitAmtDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("U_ID", typeof(string));
            foreach (string colName in DecimalColList)
            {
                dt.Columns.Add(colName, typeof(decimal));
            }
            return dt;
        }

        public void ResetAmtDt(Dictionary<string, object> parm, EditInstruct ei, string uid)
        {
            DataTable amtDt = (DataTable)parm["amtDt"];
            bool lastDn = (bool)parm["lastDn"];
            DataRow[] amtDrs = amtDt.Select("U_ID=" + SQLUtils.QuotedStr(uid));
            if (amtDrs.Length <= 0)
                return;
            foreach (string colName in DecimalColList)
            {
                decimal colVal = Prolink.Math.GetValueAsDecimal(amtDrs[0][colName]);
                amtDrs[0][colName] = colVal - Prolink.Math.GetValueAsDecimal(ei.Get(colName));
                if (lastDn)
                    ei.Put(colName, colVal);
            }
        }

        public DataTable GetDnInfoByDnList(string[] dnList)
        {
            string sql = string.Format(@"SELECT INV_NO AS DN_NO,NULL AS POST_FLAG_DATE,CBM,AMOUNT AS AMOUNT1,
(SELECT TOP 1 ISNULL(ATA,ETA) FROM {2} WHERE {2}.SHIPMENT_ID={1}.SHIPMENT_ID) AS SM_ETD FROM {1} WHERE DN_NO IN {0}  ORDER BY CBM ASC",
SQLUtils.Quoted(dnList.ToArray()), DNTableName, SMTableName);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }


        public string GetSMCS(string dnno,string shipmentId)
        {
            string sql = string.Format(@"SELECT TOP 1 FSSP_ESTNO FROM SMCS WHERE SHIPMENT_ID={0} AND DN_NO={1} order by POST_DATE desc",
                SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(dnno));
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static string GetTxcd(decimal tax)
        {
            string cd = tax.ToString();
            if (cd.Contains("."))
                cd = cd.TrimEnd('0');
            cd = cd.TrimEnd('.');
            string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TXCD' AND CD={0}", SQLUtils.QuotedStr(cd));
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public void SetCodeInfo()
        {
            string sql = string.Format("SELECT INTTRA,AP_CD FROM BSCODE WHERE CD_TYPE ='TDLT' AND CD={0} AND CMP={1}", SQLUtils.QuotedStr(BillTo),
                SQLUtils.QuotedStr(Cmp));
            DataTable bscode = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Bu = string.Empty;
            DnType = string.Empty;
            if (bscode.Rows.Count > 0)
            {
                Bu = Prolink.Math.GetValueAsString(bscode.Rows[0]["AP_CD"]);
                DnType = Prolink.Math.GetValueAsString(bscode.Rows[0]["INTTRA"]);
            }
        }

        public void SetDistribute()
        {
            string sql = string.Format("SELECT ABBR FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(LspNo));
            DataTable ptyDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Distribute = string.Empty;
            if (ptyDt.Rows.Count > 0)
                Distribute = Prolink.Math.GetValueAsString(ptyDt.Rows[0]["ABBR"]);

        }
    }   
}
