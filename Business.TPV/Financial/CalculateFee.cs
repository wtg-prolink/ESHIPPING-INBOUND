using Business.TPV.Financial;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Business.TPV.Financial
{
    public class CalculateFee
    {
        Dictionary<string, string> OtherCntTypeMapping;
        private static List<string> ChgType = new List<string>();
        public List<string> CHG_TYPE { get; set; }
        Dictionary<string, string> _cntParm = new Dictionary<string, string> { { "20GP", "F4" }, { "40GP", "F5" }, { "40HQ", "F6" } };
        Dictionary<string, string> _cntParmt = new Dictionary<string, string> { { "20GP", "F1" }, { "40GP", "F2" }, { "40HQ", "F2" } };
        Dictionary<string, string> _cntParm1 = new Dictionary<string, string> { { "20GP", "GP20" }, { "40GP", "GP40" }, { "40HQ", "HQ40" } };
        List<string> _cntParmList = new List<string>();
        const string DESTINATION_CHARGE = "DestinationCharge";
        const string LOCAL_CHARGE = "LocalCharge";
        const string FREIGHT = "Freight";

        OutBoundFreight _freight;

        public CalculateFee(string shipmentId)
        {
            OtherCntTypeMapping = new Dictionary<string, string>();
            string sql = "SELECT distinct CD, CD_DESCP FROM BSCODE WHERE CD_TYPE = 'RN_F' AND CMP = '*'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string cnttype = Prolink.Math.GetValueAsString(dr["CD"]);
                string field = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                OtherCntTypeMapping.Add(field, cnttype);
                _cntParm.Add(field, cnttype);
                _cntParmt.Add(field, cnttype);
                _cntParm1.Add(field, cnttype);
                _cntParmList.Add(field);
            }
            CHG_TYPE = ChgType = CheckBsTerm(shipmentId);
        }

        private string getfieldFromOtherMapping(string cnttype)
        {
            if (OtherCntTypeMapping == null || OtherCntTypeMapping.Count <= 0)
                return string.Empty;
            if (OtherCntTypeMapping.ContainsKey(cnttype))
                return OtherCntTypeMapping[cnttype];
            return string.Empty;
        }

        #region 拖卡车报价，前提是要在Term vs Charge中有包含TC
        public void FindTrailerQuote(string ReserveNo, string ShipmentId, List<string> emptyMessage)
        {
            string quid = string.Empty, QuotNo = string.Empty; 
            if (ChgType == null || !ChgType.Contains("TC"))
            {
                return;
            }
            string sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = CommonHelp.getDataTableFromSql(sql);
            if (dt.Rows.Count <= 0)
            {
                emptyMessage.Add("Shipment no data!");
                return;
            }
            DataRow dr = dt.Rows[0];
            string Location = Prolink.Math.GetValueAsString(dr["CMP"]);
            string TranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            DateTime eta = Prolink.Math.GetValueAsDateTime(dr["ETA"]);
            string Eta = eta.ToString("yyyy-MM-dd");
            sql = string.Format("SELECT * FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(ReserveNo));
            DataTable smrvdt = CommonHelp.getDataTableFromSql(sql);
            if (smrvdt.Rows.Count > 0)
            {
                string LspNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRUCKER"]);
                QuotNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["QUOT_NO"]);
                DataTable mdt = new DataTable();
                if (string.IsNullOrEmpty(QuotNo))
                {
                    sql = @"SELECT TOP 1 * FROM SMQTM WHERE TRAN_MODE='C' AND OUT_IN='I' AND LSP_CD={0} AND RLOCATION={1} AND 
                                EFFECT_FROM <= {2} AND EFFECT_TO >= {2} AND QUOT_TYPE='A' AND TRAN_TYPE={3} ORDER BY EFFECT_FROM DESC, CREATE_BY DESC";
                    sql = string.Format(sql, SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(Eta), SQLUtils.QuotedStr(TranType));
                    mdt = CommonHelp.getDataTableFromSql(sql);
                }
                else
                {
                    sql = string.Format("SELECT * FROM SMQTM WHERE QUOT_NO={0}", SQLUtils.QuotedStr(QuotNo));
                    mdt = CommonHelp.getDataTableFromSql(sql);
                }

                if (mdt.Rows.Count <= 0)
                {
                    Bill.WriteLog(string.Format(ReserveNo + ":无拖卡车费用报价"), ShipmentId);

                    if (!string.IsNullOrEmpty(QuotNo))
                    {
                        emptyMessage.Add(string.Format(ReserveNo + ":Cann't find the Trailer Quotation!" + QuotNo));
                    }
                    else
                    {
                        string tranMode = "FCL";
                        switch (TranType)
                        {
                            case "L":
                                tranMode = "LCL";
                                break;
                            case "A":
                                tranMode = "Air";
                                break;
                            case "R":
                                tranMode = "Rail";
                                break;
                            case "E":
                                tranMode = "INT’L EXPRESS";
                                break;
                            case "T":
                                tranMode = "Truck";
                                break;
                        }
                        emptyMessage.Add(string.Format(ReserveNo + ":Lsp Cd={0},CMP={1},ETA={2},{3}. Cann't find the Trailer Quotation!", LspNo, Location, Eta, tranMode));
                    }
                    return;
                }
                QuotNo = Prolink.Math.GetValueAsString(mdt.Rows[0]["QUOT_NO"]);
                sql = "SELECT SMQTD.* FROM SMQTD WHERE QUOT_NO={0}";
                sql = string.Format(sql, SQLUtils.QuotedStr(QuotNo));
                DataTable smqtdDt = CommonHelp.getDataTableFromSql(sql);
                if (smqtdDt.Rows.Count > 0)
                {
                    Bill.WriteLog(string.Format("拖卡车费用报价:{0}", QuotNo), ShipmentId);
                    TerailerFeeCal(smrvdt, smqtdDt, dt, emptyMessage);
                }
            }
        }
        #endregion

        #region 尋找報關報價
        public string CalBrokerFee(string ShipmentId, List<string> emptyMessage)
        {
            string Msg = "success";
            string sql = string.Empty, quid = string.Empty, QuotNo = string.Empty, RfqNo = string.Empty; 
            if (ChgType == null || !ChgType.Contains("BC"))
            { 
                return Msg;
            }
            if (ChgType.Contains("BC"))
            {
                //當SMSMI.INSPECTION = 'Y' 時，要計算報關費
                sql = @"SELECT * FROM SMSMI M INNER JOIN SMSMIPT D ON D.U_FID=M.U_ID AND PARTY_TYPE IN ('IBBR', 'IBTC') WHERE M.SHIPMENT_ID={0}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId));
                DataTable dt = CommonHelp.getDataTableFromSql(sql);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string LspNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                        string Inspection = Prolink.Math.GetValueAsString(dr["INSPECTION"]);
                        string Location = Prolink.Math.GetValueAsString(dr["CMP"]);
                        string ExtraSrv = Prolink.Math.GetValueAsString(dr["EXTRA_SRV"]);
                        string TranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                        string PodCd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                        DateTime eta = Prolink.Math.GetValueAsDateTime(dr["ETA"]);

                        string Eta = eta.ToString("yyyy-MM-dd");

                        string[] ExtraSrvArray = ExtraSrv.Split(';');

                        sql = "SELECT TOP 1 * FROM SMQTM WHERE TRAN_MODE='B' AND LSP_CD={0} AND POD_CD={4} AND RLOCATION={1} AND EFFECT_FROM <= {2} AND EFFECT_TO >= {2} AND QUOT_TYPE = 'A' AND OUT_IN='I' AND TRAN_TYPE={3} ORDER BY EFFECT_FROM DESC, CREATE_BY DESC";
                        sql = string.Format(sql, SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(Eta), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(PodCd));
                        DataTable mdt = CommonHelp.getDataTableFromSql(sql);

                        if (mdt.Rows.Count == 0)
                        {
                            sql = "SELECT TOP 1 * FROM SMQTM WHERE TRAN_MODE='B' AND LSP_CD={0} AND (POD_CD IS NULL OR POD_CD='') AND RLOCATION={1} AND EFFECT_FROM <= {2} AND EFFECT_TO >= {2} AND QUOT_TYPE = 'A' AND OUT_IN='I' AND TRAN_TYPE={3} ORDER BY EFFECT_FROM DESC, CREATE_BY DESC";
                            sql = string.Format(sql, SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(Eta), SQLUtils.QuotedStr(TranType));
                            mdt = CommonHelp.getDataTableFromSql(sql);
                        }

                        if (mdt.Rows.Count <= 0)
                        {
                            Bill.WriteLog(string.Format(LspNo + ":无报关费用报价"), ShipmentId);
                            string tranMode = "FCL";
                            switch (TranType)
                            {
                                case "L":
                                    tranMode = "LCL";
                                    break;
                                case "A":
                                    tranMode = "Air";
                                    break;
                                case "R":
                                    tranMode = "Rail";
                                    break;
                                case "E":
                                    tranMode = "INT’L EXPRESS";
                                    break;
                                case "T":
                                    tranMode = "Truck";
                                    break;
                            }
                            emptyMessage.Add(string.Format(ShipmentId + ":LSP CD={0},CMP={1},ETA={2},{3} Cann't find the Broker Quotation!", LspNo, Location, Eta, tranMode));
                        }

                        if (mdt.Rows.Count > 0)
                        {
                            QuotNo = Prolink.Math.GetValueAsString(mdt.Rows[0]["QUOT_NO"]);
                            sql = "SELECT COUNT(*) FROM SMQTD WHERE CHG_CD='ACCF' AND PUNIT='CTR' AND QUOT_NO=" + SQLUtils.QuotedStr(QuotNo);
                            int Accf = CommonHelp.getOneValueAsIntFromSql(sql);
                            DataTable fdt = new DataTable();
                            #region 找出C的费用
                            sql = "SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM," +
                                    "(SELECT TOP 1 EFFECT_FROM FROM SMQTM WHERE QUOT_NO=SMQTD.QUOT_NO) AS EFFECT_FROM," +
                                    "(SELECT TOP 1 EFFECT_TO FROM SMQTM WHERE QUOT_NO=SMQTD.QUOT_NO) AS MEFFECT_TO FROM SMQTD WHERE (CHG_CD IN ('CISC','ACCF','ICDF') OR REPAY='M') AND PUNIT NOT IN('20GP','40GP','40HQ') AND QUOT_NO=" + SQLUtils.QuotedStr(QuotNo) + " ORDER BY CHG_CD ASC";
                            fdt = CommonHelp.getDataTableFromSql(sql);
                            if (fdt.Rows.Count == 0)
                            {
                                Bill.WriteLog(string.Format("{0}:无报关费用明细", QuotNo), ShipmentId);
                                emptyMessage.Add(QuotNo + ": Can'nt find the quotation information");
                            }
                            if (fdt.Rows.Count > 0)
                            {
                                BrokerFee2Smbid(dt, fdt, emptyMessage, Accf);
                            }

                            sql = "SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM," +
                                    "(SELECT TOP 1 EFFECT_FROM FROM SMQTM WHERE QUOT_NO=SMQTD.QUOT_NO) AS EFFECT_FROM," +
                                    "(SELECT TOP 1 EFFECT_TO FROM SMQTM WHERE QUOT_NO=SMQTD.QUOT_NO) AS MEFFECT_TO FROM SMQTD WHERE (CHG_CD IN ('CISC','ACCF','ICDF') OR REPAY='M') AND PUNIT IN ('20GP','40GP','40HQ') AND QUOT_NO=" + SQLUtils.QuotedStr(QuotNo) + " ORDER BY CHG_CD ASC";
                            fdt = CommonHelp.getDataTableFromSql(sql);
                            #endregion

                            sql = "SELECT TOP 1 COUNT(TC_DEC_NO) FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            int tcn = CommonHelp.getOneValueAsIntFromSql(sql);

                            if (tcn == 0)
                            {
                                sql = "SELECT TOP 1 COUNT(TC_DEC_NO) FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                tcn = CommonHelp.getOneValueAsIntFromSql(sql);
                            }

                            if (tcn > 0)
                            {
                                #region 找出C的费用
                                sql = "SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM," +
                                    "(SELECT TOP 1 EFFECT_FROM FROM SMQTM WHERE QUOT_NO=SMQTD.QUOT_NO) AS EFFECT_FROM," +
                                    "(SELECT TOP 1 EFFECT_TO FROM SMQTM WHERE QUOT_NO=SMQTD.QUOT_NO) AS MEFFECT_TO FROM SMQTD WHERE CHG_CD IN ('T1_CISC','T1_ACCF','T1_ICDF', 'T1_ICD', 'T1_ICDS') AND PUNIT NOT IN('20GP','40GP','40HQ') AND QUOT_NO=" + SQLUtils.QuotedStr(QuotNo);
                                fdt = CommonHelp.getDataTableFromSql(sql);
                                if (fdt.Rows.Count == 0)
                                {
                                    Bill.WriteLog(string.Format("{0}:无转关费用明细", QuotNo), ShipmentId);
                                    //Msg = QuotNo + ": Can'nt find the quotation information";
                                }
                                if (fdt.Rows.Count > 0)
                                {
                                    BrokerFee2Smbid(dt, fdt, emptyMessage, Accf, true);
                                }
                                #endregion
                            }
                        }
                    }

                }
            }


            return Msg;
        }
        #endregion
        string _localCur = "";
        DataTable _chgDt = null;
        public void CalOutboundFreight(string shipmentId, List<string> emptyMessage)
        {
            if (ChgType == null || (!ChgType.Contains("OBC") && !ChgType.Contains("OLC") && !ChgType.Contains("OTC"))) return;
            _freight = InitData(shipmentId);
            if (ChgType.Contains("OBC"))
            {
                CalOutBoundBrokerFee(emptyMessage);
            }
            if (ChgType.Contains("OLC"))
            {
                CalOutBoundLocalFee(emptyMessage);
            }
            if (ChgType.Contains("OTC"))
            {
                CalOutBoundTrailerFee(emptyMessage);
            }
        }

        public OutBoundFreight InitData(string shipmentId)
        {
           string sql = string.Format(@"SELECT SMSMI.SHIPMENT_ID,
                SMSM.EXTERNAL_WMS,SMSM.ISCOMBINE_BL,SMSM.SHIP_MODE,SMSM.CUSTOMS_CHECK,SMSM.DECL_NUM,SMSM.CONT_DECL_NUM,SMSM.NEXT_NUM,SMSMI.GROUP_ID,SMSMI.TRACK_WAY,
                SMSM.PCNT20,SMSM.PCNT40,SMSM.PCNT40HQ,SMSM.POCNT_NUMBER,SMSM.PCNT_TYPE,SMSM.COMBINE_INFO,SMSM.CBM,SMSM.PCBM,SMSMI.PKG_UNIT,SMSMI.CW,
                SMSMI.GWU,SMSM.GW,SMSM.PGW,SMSMI.GVALUE,SMSM.PCNT_NUMBER,SMSMI.PKG_NUM,SMSMI.MASTER_NO,SMSMI.HOUSE_NO,SMSM.COMBINE_OTHER,SMSMI.INCOTERM_CD,
                SMSM.TELEX_RLS,SMSM.REGION,SMSMI.HORN,SMSMI.BATTERY,SMSM.IS_SPLIT_BILL,SMSM.POCNT_TYPE,SMSMI.INVOICE_INFO,SMSMI.CNTR_INFO,
                SMSMI.CAR_TYPE,SMSMI.CAR_QTY,SMSMI.CAR_TYPE1,SMSMI.CAR_QTY1,SMSMI.CAR_TYPE2,SMSMI.CAR_QTY2,SMSMI.FRT_TERM,
                SMSMI.CMP,SMSMI.POD_CD,SMSMI.PPOL_CD,SMSMI.ATA,SMSMI.ETA,SMSMI.TRAN_TYPE FROM SMSMI,SMSM WHERE SMSM.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMI.SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));

            DataTable dt = CommonHelp.getDataTableFromSql(sql);
            if (dt.Rows.Count <= 0) return null;

            sql = string.Format(@"SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));

            DataTable ptDt = CommonHelp.getDataTableFromSql(sql);   
            string combine_info = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBINE_INFO"]);
            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries); 
            DataRow dr = dt.Rows[0]; 
            OutBoundFreight freight = new OutBoundFreight();
            if (dr["ATA"] != null && dr["ATA"] != DBNull.Value)
            {
                freight.BillDate = Prolink.Math.GetValueAsDateTime(dr["ATA"]).ToString("yyyy-MM-dd");
                freight.BillDescp = "ATA";
            }
            else
            {
                freight.BillDate = Prolink.Math.GetValueAsDateTime(dr["ETA"]).ToString("yyyy-MM-dd");
                freight.BillDescp = "ETA";
            }
            sql = string.Format(@"SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND EDATE<={0} ORDER BY EDATE",
                SQLUtils.QuotedStr(freight.BillDate)); 

            DataTable rateDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            freight.RateDt = rateDt;
            freight.ShipmentId = shipmentId;
            freight.Cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            freight.GroupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            freight.Pod = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
            freight.TranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            freight.Wms= Prolink.Math.GetValueAsString(dr["EXTERNAL_WMS"]);
            freight.IscombineBl = Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"]);
            freight.ShipMode = Prolink.Math.GetValueAsString(dr["SHIP_MODE"]);
            freight.CustomsCheck = Prolink.Math.GetValueAsString(dr["CUSTOMS_CHECK"]);
            freight.DeclNum = Prolink.Math.GetValueAsString(dr["DECL_NUM"]);
            freight.ContDeclNum = Prolink.Math.GetValueAsString(dr["CONT_DECL_NUM"]);
            freight.NextNum = Prolink.Math.GetValueAsString(dr["NEXT_NUM"]);
            freight.SmsmPod = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
            freight.TrackWay = Prolink.Math.GetValueAsString(dr["TRACK_WAY"]);
            decimal Pcnt20 = Prolink.Math.GetValueAsDecimal(dr["PCNT20"]);
            decimal Pcnt40 = Prolink.Math.GetValueAsDecimal(dr["PCNT40"]);
            decimal Pcnt40Hq = Prolink.Math.GetValueAsDecimal(dr["PCNT40HQ"]);
            decimal PocntNumber = Prolink.Math.GetValueAsDecimal(dr["POCNT_NUMBER"]);
            string PcntType = Prolink.Math.GetValueAsString(dr["PCNT_TYPE"]);
            decimal PcntNumber = Prolink.Math.GetValueAsDecimal(dr["PCNT_NUMBER"]);
            freight.Pcnt20 = Pcnt20;
            freight.Pcnt40= Pcnt40;
            freight.Pcnt40Hq = Pcnt40Hq;
            decimal cntNum = Pcnt20 + Pcnt40 + Pcnt40Hq + PocntNumber;
            if (!string.IsNullOrEmpty(PcntType))
            {
                if (!"LCL".Equals(PcntType.ToUpper()))
                {
                    cntNum += PcntNumber;
                }
            }
            freight.CntNum = cntNum;
            freight.DnNum = dns.Length;
            freight.Cbm = Helper.GetValueAsDecimal(dr, new string[] { "CBM", "PCBM" });
            string PkgUnit = Prolink.Math.GetValueAsString(dr["PKG_UNIT"]);
            decimal PkgNum = Prolink.Math.GetValueAsDecimal(dr["PKG_NUM"]); 
            freight.Cnt = "PLT".Equals(PkgUnit) || "CTN".Equals(PkgUnit) ? PkgNum : 0;
            freight.Nw = Prolink.Math.GetValueAsDecimal(dr["CW"]);
            freight.Gwu = Prolink.Math.GetValueAsString(dr["GWU"]);
            freight.Gw = Helper.GetValueAsDecimal(dr, new string[] { "GW", "PGW" });
            freight.Gvalue = Prolink.Math.GetValueAsDecimal(dr["GVALUE"]);
            string masterNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
            string houseNo = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);
            freight.BlNo = string.IsNullOrEmpty(houseNo) ? masterNo : houseNo; 
            freight.Cout = Prolink.Math.GetValueAsString(dr["COMBINE_OTHER"]);
            freight.IncotermCd = Prolink.Math.GetValueAsString(dr["INCOTERM_CD"]);
            freight.TelexRls = Prolink.Math.GetValueAsString(dr["TELEX_RLS"]);
            freight.FrtTerm = Prolink.Math.GetValueAsString(dr["FRT_TERM"]);
            freight.Region = Prolink.Math.GetValueAsString(dr["REGION"]);
            freight.Horn = Prolink.Math.GetValueAsString(dr["HORN"]);
            freight.Battery = Prolink.Math.GetValueAsString(dr["BATTERY"]); 
            freight.OcntType = Prolink.Math.GetValueAsString(dr["POCNT_TYPE"]);
            freight.PocntNumber = Prolink.Math.GetValueAsDecimal(dr["POCNT_NUMBER"]);
            freight.PcntType = Prolink.Math.GetValueAsString(dr["PCNT_TYPE"]);
            freight.IsSplitBill = Prolink.Math.GetValueAsString(dr["IS_SPLIT_BILL"]);
            freight.InvoiceInfo = Prolink.Math.GetValueAsString(dr["INVOICE_INFO"]);
            freight.CntrInfo = Prolink.Math.GetValueAsString(dr["CNTR_INFO"]);
            freight.Current_smsm = dr;  

            Func<string, string> getPartyNo = (partyType) =>
            {
                DataRow[] rows = ptDt.Select($"PARTY_TYPE={SQLUtils.QuotedStr(partyType)}");
                return rows.Length > 0 ? Prolink.Math.GetValueAsString(rows[0]["PARTY_NO"]) : "";
            };
            Func<string, string> getPartyNm = (partyType) =>
            {
                DataRow[] rows = ptDt.Select($"PARTY_TYPE={SQLUtils.QuotedStr(partyType)}");
                return rows.Length > 0 ? Prolink.Math.GetValueAsString(rows[0]["PARTY_NAME"]) : "";
            };
            freight.BrPartyNo = getPartyNo("BR");
            freight.BrPartyNm = getPartyNm("BR");
            freight.CrPartyNo = getPartyNo("CR");
            freight.CrPartyNm = getPartyNm("CR");
            freight.SpPartyNo = getPartyNo("SP");
            freight.SpPartyNm = getPartyNm("SP");
            freight.FcPartyNo = getPartyNo("FC");
            freight.FcPartyNm = getPartyNm("FC");
            freight.BoPartyNo = getPartyNo("BO");
            freight.BoPartyNm = getPartyNm("BO");
            freight.ShPartyNo = getPartyNo("SH");
            freight.ShPartyNm = getPartyNm("SH");
            freight.DebitToList = GetDebitTo(freight);
            return freight;
        }
         
        public List<string> GetDebitTo(OutBoundFreight freight)
        {
            List<string> types = new List<string>();
            switch (freight.FrtTerm)
            {
                case "P":
                    types.Add(LOCAL_CHARGE);
                    types.Add(FREIGHT);
                    break;
                default:
                    if (("R".Equals(freight.TranType) || "L".Equals(freight.TranType) || "A".Equals(freight.TranType)) && !"EXW".Equals(freight.IncotermCd))
                    { 
                        types.Add(LOCAL_CHARGE);
                    }
                    else if ("F".Equals(freight.TranType))
                    { 
                        types.Add(LOCAL_CHARGE);
                    }
                    else if (("T".Equals(freight.TranType) && ("A".Equals(freight.TrackWay) || "S".Equals(freight.TrackWay))) || "D".Equals(freight.TrackWay))
                    { 
                        types.Add(FREIGHT);
                    }
                    break;
            }
            switch (freight.IncotermCd)
            {
                case "CIP":
                case "DAP":
                case "DDP":
                    types.Add(DESTINATION_CHARGE);
                    break; 
            }
            return types;
        }


        public string CalOutBoundBrokerFee(List<string> emptyMessage)
        {
            string Msg = "success";
            if (_freight == null || ChgType == null || !ChgType.Contains("OBC"))
                return Msg;
            _localCur = Standard.BillingManager.GetLocalCur(_freight.GroupId, _freight.Cmp); 
            if (_chgDt == null)
                _chgDt = OperationUtils.GetDataTable(string.Format(@"SELECT * FROM SMCHG WHERE IO_TYPE='O' AND GROUP_ID={1} AND (CMP='*' OR CMP={2})", SQLUtils.QuotedStr(_freight.TranType), SQLUtils.QuotedStr(_freight.GroupId), SQLUtils.QuotedStr(_freight.Cmp)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            string qtdate_condition = string.Format(" AND SMQTM.EFFECT_FROM<={0} AND SMQTM.EFFECT_TO>={0}", SQLUtils.QuotedStr(_freight.BillDate));
            string sql = string.Format(@"SELECT * FROM(SELECT * FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A'
            AND SMQTM.TRAN_MODE='B' AND OUT_IN='I' AND SMQTM.RLOCATION={0} AND (SMQTM.POD_CD IS NULL OR SMQTM.POD_CD={1}) AND SMQTM.LSP_CD={3} {2} AND SMQTM.TRAN_TYPE={4}))A 
            OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,POD_CD AS QT_POD_CD FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B 
            ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(_freight.Cmp), SQLUtils.QuotedStr(_freight.Pod), qtdate_condition, SQLUtils.QuotedStr(_freight.BrPartyNo), SQLUtils.QuotedStr(_freight.TranType));
                
            DataTable brokerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (brokerDt.Rows.Count <= 0)
            {

                Bill.WriteLog(string.Format(_freight.BrPartyNo + ":无OutBound报关费用报价"), _freight.ShipmentId);

                emptyMessage.Add(string.Format(_freight.ShipmentId + ":LSP CD={0},CMP={1},{2}={3},Cann't find the Broker Quotation!", _freight.BrPartyNo, _freight.Cmp, _freight.BillDescp, _freight.BillDate));
            }
            BrokerFreight(brokerDt);
            return Msg;
        }

        private void BrokerFreight(DataTable brokerDt)
        {
            DataTable rateDt =  _freight.RateDt; 
            Helper.AddOthColumns(brokerDt);
            string wms = _freight.Wms;
            string iscombine_bl = _freight.IscombineBl;
            string shipmentId = _freight.ShipmentId;
            decimal total = 0M; 
            int index = 1;
            MixedList ml = new MixedList();
            List<string> msg = new List<string>();
            string tranMode = _freight.TranType;
            List<string> testList = new List<string>();
            string shipMode = _freight.ShipMode;
            string condition = " (SHIP_MODE = '' OR SHIP_MODE IS NULL)";
            if (!string.IsNullOrEmpty(shipMode))
                condition = string.Format(" (SHIP_MODE={0} OR SHIP_MODE='' OR SHIP_MODE IS NULL)", SQLUtils.QuotedStr(shipMode));
            string pod = _freight.SmsmPod;
            DataRow[] drs = new DataRow[] { };
            if (!string.IsNullOrEmpty(pod))
                drs = brokerDt.Select(string.Format(@" {0} AND QT_POD_CD={1}", condition, SQLUtils.QuotedStr(pod)));
            if (drs.Length <= 0)
                drs = brokerDt.Select(string.Format(@" {0} AND (QT_POD_CD IS NULL OR QT_POD_CD='')", condition));
            if (drs.Length <= 0)
                drs = brokerDt.Select(condition);

            foreach (DataRow broker in drs)
            {
                string quot_no = Prolink.Math.GetValueAsString(broker["QUOT_NO"]);
                string repay = Prolink.Math.GetValueAsString(broker["REPAY"]);
                if (string.IsNullOrEmpty(quot_no))
                    continue;
                if (testList.Count <= 0) testList.Add(quot_no);
                if (!testList.Contains(quot_no)) break;
                //ECD
                string chg_cd = Prolink.Math.GetValueAsString(broker["CHG_CD"]);
                string carrier = Prolink.Math.GetValueAsString(broker["CARRIER"]);
                if (!string.IsNullOrEmpty(carrier) && !carrier.Equals(wms))
                    continue;
                string punit = Prolink.Math.GetValueAsString(broker["PUNIT"]);
                if (!"BL".Equals(punit) && "C".Equals(iscombine_bl))
                    continue;
                if ("BL".Equals(punit) && "S".Equals(iscombine_bl))
                    continue;
                
                bool error = false;
                string cur = Prolink.Math.GetValueAsString(broker["CUR"]);
                decimal F1 = Prolink.Math.GetValueAsDecimal(broker["F3"]);

                decimal qty = GetQty(punit);
                decimal cur_total = 0M;
                if (CalCNT(broker, tranMode, ref error, msg, ml,null,_freight.BrPartyNo, _freight.BrPartyNm))
                    continue;
                SetOthCharge(chg_cd, F1, shipmentId, ref punit, ref qty, ref cur_total);
                  
                decimal temp1 = 0M; 

                broker["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                if ("%".Equals(punit))
                {
                    broker["QEX_RATE"] = 1;
                    F1 = temp1;
                    qty = 1;
                    cur_total = temp1;
                    broker["CUR"] = _localCur;
                }

                broker["LOCALE_AMT"] = temp1;
                SetChargeInfo(broker, "", tranMode);

                broker["QCHG_UNIT"] = punit;
                broker["QUNIT_PRICE"] = F1;
                broker["QQTY"] = qty;
                broker["QAMT"] = cur_total;
                broker["C_FLAG"] = "Y";
                ml.Add(CreateBillItem(broker, _freight.BrPartyNo, _freight.BrPartyNm));
            }
            try
            { 
                CalculateLocalAmt(ml, _freight.Current_smsm);
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            { 
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("Shipment Id:" + _freight.ShipmentId + ",CalOutBoundBrokerFee Error:" + ex.ToString());
            }
        }


        public decimal GetQty(string punit, List<string> cntparmlist = null)
        {
            decimal qty = 1;
            string tranMode = _freight.TranType;
            string ocnttype = _freight.OcntType; 
            string pcnttype = _freight.PcntType;
            if (cntparmlist != null && cntparmlist.Contains(punit))
            {
                if (punit == ocnttype) 
                    return _freight.PocntNumber; 
                if (punit == pcnttype) 
                    return _freight.PcntNumber;  
                return 0;
            }
            switch (punit)
            {
                case "20GP":
                    qty = Helper.GetDecimalValue(_freight.Pcnt20);
                    break;
                case "40GP":
                    qty = Helper.GetDecimalValue(_freight.Pcnt40);
                    break;
                case "40HQ":
                    qty = Helper.GetDecimalValue(_freight.Pcnt40Hq);
                    break;
                case "CNT":
                case "CTR":
                    qty = Helper.GetDecimalValue(_freight.CntNum);
                    break;
                case "DN":
                    qty = Helper.GetDecimalValue(_freight.DnNum);
                    break;
                case "BL":
                case "SHT":
                    qty = 1;
                    break;
                case "M3":
                case "CBM":
                    qty = Helper.GetDecimalValue(_freight.Cbm);
                    switch (tranMode)
                    {
                        case "L":
                        case "F":
                            if (qty < 1M)
                                qty = 1M;
                            break;
                    }
                    break;
                case "CTN":
                case "PLT":
                    qty = Helper.GetDecimalValue(_freight.Cnt);
                    break;
                case "CW":
                    qty = Helper.GetDecimalValue(_freight.Nw);
                    break;
                case "L":
                case "G":
                case "LB":
                case "K":
                case "KG":
                case "KGM":
                case "KGS":
                    string gwu = _freight.Gwu.ToUpper();
                    qty = Helper.GetDecimalValue(_freight.Gw);
                    qty = Helper.GetKGWeight(punit, qty, ref gwu, null);
                    break;
                case "%":
                    qty = _freight.Gvalue;
                    break;
                default:
                    qty = 1;
                    break;
            }
            return qty;
        }

        private EditInstruct CreateBillItem(DataRow qt, string lsp_no, string lsp_nm)
        {
            EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
            string shipmentId = _freight.ShipmentId; 
            ei.Put("LSP_NO", lsp_no); 
            ei.Put("LSP_NM", lsp_nm); 
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("QUOT_ID", qt["U_ID"]);
            ei.Put("QUOT_NO", qt["QUOT_NO"]); 
            string rfq_no = Prolink.Math.GetValueAsString(qt["RFQ_NO"]);
            if ("undefined".Equals(rfq_no) || "null".Equals(rfq_no) || string.IsNullOrEmpty(rfq_no))
                rfq_no = string.Empty;
            ei.PutDate("DEBIT_DATE", Prolink.Math.GetValueAsDateTime(_freight.BillDate));
            ei.Put("RFQ_NO", rfq_no);
            ei.Put("SHIPMENT_ID", shipmentId); 
            ei.Put("BILL_TO", _freight.Cmp);  
            ei.Put("CMP", _freight.Cmp);
            ei.Put("TRAN_TYPE", _freight.TranType);
            ei.Put("GROUP_ID", _freight.GroupId);
            ei.Put("BL_NO", _freight.BlNo);

            ei.Put("DEBIT_TO", _freight.ShPartyNo);
            ei.Put("DEBIT_NM", _freight.ShPartyNm);
            ei.Put("POD_CD", _freight.Pod);
            ei.Put("INVOICE_INFO", _freight.InvoiceInfo);
            ei.Put("CNTR_INFO", _freight.CntrInfo);

            string chgCd = GetQTValue(qt, "QCHG_CD", "CHG_CD");
            ei.Put("CHG_CD", chgCd);
            ei.Put("CHG_DESCP", GetQTValue(qt, "QCHG_DESCP", "CHG_DESCP"));
            ei.Put("CHG_TYPE", GetQTValue(qt, "QCHG_TYPE", "CHG_TYPE"));
            ei.Put("REPAY", GetQTValue(qt, "QREPAY", "REPAY"));

            string contractNo = string.Empty;
            if (qt.Table.Columns.Contains("CONTRACT_NO"))
                contractNo = Prolink.Math.GetValueAsString(qt["CONTRACT_NO"]); 
            if (string.IsNullOrEmpty(ei.Get("CHG_DESCP")) && "FRT".Equals(ei.Get("CHG_CD")))
                ei.Put("CHG_DESCP", "Freight charge");
            ei.Put("QCUR", qt["CUR"]); 
            ei.Put("QUNIT_PRICE", qt["QUNIT_PRICE"]); 
            ei.Put("QCHG_UNIT", qt["QCHG_UNIT"]); 
            ei.Put("QQTY", qt["QQTY"]); 
            ei.Put("QAMT", qt["QAMT"]); 
            ei.Put("QLAMT", qt["LOCALE_AMT"]);
            ei.Put("QEX_RATE", qt["QEX_RATE"]);
             
            ei.Put("REMARK", qt["EX_REMARK"]); 
            ei.Put("QTAX", qt["VAT_RATE"]); 
            if (!string.IsNullOrEmpty(contractNo))
                ei.Put("CONTRACT_NO", contractNo);
            if (qt.Table.Columns.Contains("CNTR_STD_QTY"))
                ei.Put("CNTR_STD_QTY", qt["CNTR_STD_QTY"]);
            if (qt.Table.Columns.Contains("IPART_NO"))
                ei.Put("IPART_NO", qt["IPART_NO"]);
            if (qt.Table.Columns.Contains("DN_NO"))
                ei.Put("DN_NO", qt["DN_NO"]);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            return ei;
        }

        private static string GetQTValue(DataRow qt, string qname, string name)
        {
            string val = qt.Table.Columns.Contains(qname) ? Prolink.Math.GetValueAsString(qt[qname]) : string.Empty;
            if (string.IsNullOrEmpty(val))
                val = qt.Table.Columns.Contains(name) ? Prolink.Math.GetValueAsString(qt[name]) : string.Empty;
            return val;
        }


        private void SetOthCharge(string chg_cd, decimal F1,string shipmentId, ref string punit, ref decimal qty, ref decimal cur_total)
        {
            decimal decl_num = Helper.GetDecimalValue(_freight.DeclNum);
            decimal cont_decl_num = Helper.GetDecimalValue(_freight.ContDeclNum);

            switch (chg_cd)
            {
                case "MCO": 
                    string sql = string.Format("SELECT COUNT(1) FROM (SELECT U_ID FROM SMRV WHERE SHIPMENT_ID={0}) A LEFT JOIN SMRVM B ON A.U_ID=B.U_FID WHERE (DATENAME(hour,B.MODIFY_DATE)>=21 OR DATENAME(hour,B.MODIFY_DATE)<3)", SQLUtils.QuotedStr(shipmentId));
                    qty = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    cur_total = Helper.Get45AmtValue(qty * F1); 
                    break;
                case "EDN": 
                    punit = "SET";
                    qty = cont_decl_num;
                    cur_total = Helper.Get45AmtValue(qty * F1);
                    break;
                case "ECD": 
                    punit = "SET";
                    decl_num = 1; 
                    cur_total = Helper.Get45AmtValue(decl_num * F1);
                    qty = decl_num;
                    break;
                case "DDC": 
                    punit = "SET";
                    decimal next_num = Helper.GetDecimalValue(_freight.NextNum);
                    cur_total = Helper.Get45AmtValue(next_num * F1); 
                    qty = next_num;
                    break;
                default:
                    cur_total = Helper.Get45AmtValue(qty * F1);
                    if ("%".Equals(punit))
                    {
                        cur_total = Helper.Get45AmtValue(qty * F1 * 0.01M);
                    }
                    break;
            }
        }
         
        private void SetChargeInfo(DataRow dr, string chgCd, string tranMode, bool setType = false, bool defaultChgCd = false)
        {

            if (_chgDt == null)
                return;
            string chgCd1 = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
            if ("FRT".Equals(chgCd1) || defaultChgCd)
            {
                dr["CHG_CD"] = chgCd;
            }
            else
            {
                chgCd = chgCd1;
            }
            DataRow[] drs = _chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr(tranMode)));
            if (drs.Length <= 0)
                drs = _chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr("O")));
            if (drs.Length <= 0)
                drs = _chgDt.Select(string.Format("CHG_CD={0}", SQLUtils.QuotedStr(chgCd)));
            if (drs.Length > 0)
            {
                dr["QCHG_DESCP"] = drs[0]["CHG_DESCP"];
                dr["QREPAY"] = drs[0]["REPAY"]; 
                if (setType)
                    dr["QCHG_TYPE"] = drs[0]["CHG_TYPE"];
                else if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["CHG_TYPE"])))
                    dr["QCHG_TYPE"] = drs[0]["CHG_TYPE"];
                else
                    dr["QCHG_TYPE"] = dr["CHG_TYPE"];
            }
        }
         
        private bool CalCNT(DataRow local, string tranMode, ref bool error, List<string> msg, MixedList ml, DataRow thc = null, string credit_to = "", string credit_nm = "")
        {
            DataTable rateDt = _freight.RateDt;
            decimal cur_total = 0m;
            string cur = Prolink.Math.GetValueAsString(local["CUR"]);
            List<string> cntMsg = new List<string>();
            DataRow dr = local;
            Dictionary<string, string> cntParm = _cntParm; 
            if (thc != null)
            {
                dr = thc;
                cntParm = _cntParm1;
                cur = Prolink.Math.GetValueAsString(thc["CUR"]);
                if (string.IsNullOrEmpty(cur))
                    cur = Prolink.Math.GetValueAsString(local["CUR"]);
                else
                    local["CUR"] = cur;
            }

            foreach (var kv in cntParm)
            {
                decimal price = Prolink.Math.GetValueAsDecimal(dr[kv.Value]);
                string punit = kv.Key;
                decimal qty = GetQty(punit);
                if (qty <= 0 || price <= 0)
                    continue;
                cntMsg.Add(string.Format("{0}({1}*{2}{3})", kv.Key, qty, price, cur));
                cur_total += Helper.Get45AmtValue(qty * price);
            }

            if (cur_total > 0)
            {
                msg.Add(string.Join("+", cntMsg));
                local["EX_REMARK"] = string.Join("+", cntMsg);
                local["QCHG_UNIT"] = "CTR";
                local["QUNIT_PRICE"] = cur_total;
                local["QQTY"] = 1;
                local["QAMT"] = cur_total;
                local["C_FLAG"] = "Y";
                decimal temp1 = 0M;
                local["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                local["LOCALE_AMT"] = temp1;
                SetChargeInfo(local, "", tranMode);
                ml.Add(CreateBillItem(local, credit_to, credit_nm));
                local["EX_REMARK"] = "";
            }
            return cur_total > 0;
        }
         

        #region 尋找local charge
        public void CalLocalFee(string ShipmentId, List<string> emptyMessage)
        {
            string Msg = "success";
            string sql = string.Empty, QuotNo = string.Empty, QUOT_NO = string.Empty; 

            if (ChgType == null || !ChgType.Contains("LC"))
            { 
                return;
            }

            sql = @"SELECT * FROM SMSMI M LEFT JOIN SMSMIPT D ON D.U_FID=M.U_ID AND PARTY_TYPE IN ('IBSP','IBLP', 'IBGV', 'IBTW','IBIS','IBBO') WHERE  M.SHIPMENT_ID={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = CommonHelp.getDataTableFromSql(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string LspNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                string Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
                string FrtTerm = Prolink.Math.GetValueAsString(dt.Rows[0]["FRT_TERM"]);
                string Location = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
                string TerminalCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TERMINAL_CD"]);
                decimal Cnt20 = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["CNT20"]);
                decimal Cnt40 = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["CNT40"]);
                decimal Cnt40hq = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["CNT40HQ"]);
                string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);

                string Cnt40OtherType = Prolink.Math.GetValueAsString(dt.Rows[0]["OCNT_TYPE"]);
                decimal Cnt40Other = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["OCNT_NUMBER"]);

                sql = "SELECT PARTY_NO FROM SMSMIPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                string CustCd = CommonHelp.getOneValueAsStringFromSql(sql);
                sql = string.Format(@"SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO=
                    (SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE='SP') AND STATUS='U'", SQLUtils.QuotedStr(ShipmentId));
                string headOffice = CommonHelp.getOneValueAsStringFromSql(sql);

                DateTime eta = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETA"]);
                DateTime ata = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ATA"]);
                string ExtraSrv = Prolink.Math.GetValueAsString(dt.Rows[0]["EXTRA_SRV"]);
                string Eta = eta.ToString("yyyy-MM-dd");
                string Ata = ata.ToString("yyyy-MM-dd");

                if (LspNo == "")
                {
                    LspNo = Carrier;
                }

                sql = "SELECT TOP 1 * FROM SMQTM WHERE TRAN_MODE='X' AND LSP_CD={0} AND RLOCATION={1} AND OUT_IN='I' AND EFFECT_FROM <= {2} AND EFFECT_TO >= {2} AND FREIGHT_TERM={3} AND  QUOT_TYPE='A' AND TRAN_TYPE={4} AND CUST_CD={5} AND POD_CD={6} ORDER BY EFFECT_FROM DESC, CREATE_BY DESC";
                sql = string.Format(sql, SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(Eta), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(CustCd), SQLUtils.QuotedStr(PodCd));
                DataTable mdt = CommonHelp.getDataTableFromSql(sql);

                if (mdt.Rows.Count == 0)
                {
                    sql = "SELECT TOP 1 * FROM SMQTM WHERE TRAN_MODE='X' AND LSP_CD={0} AND RLOCATION={1} AND OUT_IN='I' AND EFFECT_FROM <= {2} AND EFFECT_TO >= {2} AND FREIGHT_TERM={3} AND  QUOT_TYPE='A' AND TRAN_TYPE={4} AND POD_CD={5} ORDER BY EFFECT_FROM DESC, CREATE_BY DESC";
                    sql = string.Format(sql, SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(Eta), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(PodCd));
                    mdt = CommonHelp.getDataTableFromSql(sql);
                }

                if (mdt.Rows.Count <= 0)
                {
                    Bill.WriteLog(string.Format(LspNo + ":无Local费用报价"), ShipmentId);
                    string frtTermDesc = "Other";
                    switch (FrtTerm)
                    {
                        case "C":
                            frtTermDesc = "Collect";
                            break;
                        case "P":
                            frtTermDesc = "Prepaid";
                            break;
                    }
                    string tranMode = "FCL";
                    switch (TranType)
                    {
                        case "L":
                            tranMode = "LCL";
                            break;
                        case "A":
                            tranMode = "Air";
                            break;
                        case "R":
                            tranMode = "Rail";
                            break;
                        case "E":
                            tranMode = "INT’L EXPRESS";
                            break;
                        case "T":
                            tranMode = "Truck";
                            break;
                    }
                    emptyMessage.Add(string.Format(ShipmentId + ":LSP CD={0},CMP={1},ETA={2},{3},{4},POD={5} Cann't find the Local Quotation!", LspNo, Location, Eta, frtTermDesc, tranMode, PodCd));
                    continue;
                }
                List<string> spType = new List<string>() { "L", "R", "F" };
                bool ChcekFlag = spType.Contains(TranType) ? true : false;
                if (mdt.Rows.Count > 0)
                {
                    QuotNo = Prolink.Math.GetValueAsString(mdt.Rows[0]["QUOT_NO"]);
                    QUOT_NO = Prolink.Math.GetValueAsString(mdt.Rows[0]["QUOT_NO"]);
                    ///算出必收費用 
                    LocalMustGetFee(ShipmentId, dr, Carrier, QuotNo, emptyMessage, ChcekFlag, headOffice);

                    #region ISPS, Doc費用
                    LocalISPSDocFee(ShipmentId, dr, Location, Eta, QuotNo, emptyMessage, ChcekFlag, headOffice);
                    #endregion

                    #region Storage、Demurage、Detention計價
                    LocalStorageDemurageDetention(ShipmentId, dt, mdt, emptyMessage, ChcekFlag, headOffice);
                    #endregion

                    #region 判斷是否該收ISTF費用
                    sql = LocalISFTFee(ShipmentId, QuotNo, dt, emptyMessage, ChcekFlag, headOffice);
                    #endregion

                    #region 判斷是否該收WSF
                    sql = LocalWSFFee(ShipmentId, QuotNo, dt, emptyMessage, ChcekFlag, headOffice);
                    #endregion

                    #region IBTA計價
                    sql = "SELECT PARTY_NO FROM SMSMI M LEFT JOIN SMSMIPT D ON D.U_FID=M.U_ID AND PARTY_TYPE='IBTA' WHERE ((INCOTERM_CD IN ('EXC','FCA','FAS','FOB') AND FRT_TERM='C') OR (INCOTERM_CD IN ('CPT','CIP','DAT','CFR','CIF','DAP') AND FRT_TERM='P')) AND M.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    string PARTY_NO = CommonHelp.getOneValueAsStringFromSql(sql);
                    QuotNo = Prolink.Math.GetValueAsString(mdt.Rows[0]["QUOT_NO"]);
                    sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='Y' AND QUOT_NO={0} AND TERMINAL_AGENT_NO={1}", SQLUtils.QuotedStr(QUOT_NO), SQLUtils.QuotedStr(PARTY_NO));
                    DataTable fdt = CommonHelp.getDataTableFromSql(sql);

                    if (!string.IsNullOrEmpty(PARTY_NO) && fdt.Rows.Count == 0)
                    {
                        emptyMessage.Add("Inbound Terminal Agent:" + PARTY_NO + "." + QuotNo + " cann't find fee");
                    }

                    DataRow[] fdtRows = GetSmqtdRows(ChcekFlag, fdt, headOffice);

                    if (fdtRows != null && fdtRows.Length > 0)
                    {
                        Bill.WriteLog(string.Format("Local费用报价:{0},IBTA計價", QuotNo), ShipmentId);
                        foreach (DataRow fdr in fdtRows)
                        { 
                            SeaPublicFee2Smbid(dr, fdr, emptyMessage); 
                        }
                    }
                    #endregion

                    #region BHC
                    QuotNo = Prolink.Math.GetValueAsString(mdt.Rows[0]["QUOT_NO"]);
                    sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='M' AND QUOT_NO={0} AND CHG_CD='BHC' AND (PUNIT='SET' OR PUNIT='PCS')",
SQLUtils.QuotedStr(QUOT_NO));
                    fdt = CommonHelp.getDataTableFromSql(sql);

                    DataRow[] fdtdrs = GetSmqtdRows(ChcekFlag, fdt, headOffice);

                    if (fdtdrs != null && fdtdrs.Length > 0)
                    { 
                        //BHC費用单独计算 
                        CalOtherLocal(dr, fdtdrs, ShipmentId, emptyMessage);
                    } 
                    #endregion
                }

                string[] ExtraSrvArray = ExtraSrv.Split(';');

                #region 问题单 164258 
                foreach (string extrasrv in ExtraSrvArray)
                {
                    if (string.IsNullOrEmpty(extrasrv)) continue;
                    string extratype = extrasrv;
                    string feeDescp = "";
                    switch (extrasrv)
                    {
                        case "MH":
                            extratype = "MHC";
                            feeDescp = "Mold Handling";
                            break;
                        case "FORK":
                            extratype = "FKLF";
                            feeDescp = "Forklift";
                            break;
                        case "DMRP":
                            extratype = "DRPS";
                            feeDescp = "DAMAGE REPORT";
                            break;
                    }
                    sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='C' AND QUOT_NO={0} AND CHG_CD={1}", SQLUtils.QuotedStr(QUOT_NO), SQLUtils.QuotedStr(extratype));
                    DataTable fdt = CommonHelp.getDataTableFromSql(sql);

                    DataRow[] fdtRows = GetSmqtdRows(ChcekFlag, fdt, headOffice);
                    
                    if (fdtRows != null && fdtRows.Length > 0)
                    {
                        Bill.WriteLog(string.Format("Local费用报价:{0},额外" + extratype, QuotNo), ShipmentId);
                        foreach (DataRow fdr in fdtRows)
                        { 
                            SeaPublicFee2Smbid(dr, fdr, emptyMessage);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(feeDescp))
                            emptyMessage.Add(QUOT_NO + " cann't find " + feeDescp);
                    }
                }
                #endregion
            }
            return;
        }
         
        private string LocalWSFFee(string ShipmentId, string QuotNo, DataTable dt, List<string> emptyMessage, bool ChcekFlag, string headOffice)
        {
            string Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
            string Location = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string TerminalCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TERMINAL_CD"]);
            string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string sql = "SELECT * FROM SMORD WHERE O_ORDNO IS NOT NULL AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable odt = CommonHelp.getDataTableFromSql(sql);
            if (odt.Rows.Count <= 0) return string.Empty;
            decimal amt = 0;
            decimal WsfAmt = 0;
            string StorageCur = string.Empty;
            if (TranType == "F" || TranType == "R")
            {
                foreach (DataRow odr in odt.Rows)
                {
                    string Oordno = Prolink.Math.GetValueAsString(odr["O_ORDNO"]);
                    DateTime RebulidDate = Prolink.Math.GetValueAsDateTime(odr["CREATE_DATE"]);
                    sql = "SELECT * FROM SMRCNTR WHERE ORD_NO=" + SQLUtils.QuotedStr(Oordno);
                    DataTable cntrDt = CommonHelp.getDataTableFromSql(sql);

                    if (cntrDt.Rows.Count > 0)
                    {
                        foreach (DataRow cntrDr in cntrDt.Rows)
                        {
                            DateTime PodMdate = Prolink.Math.GetValueAsDateTime(cntrDr["POD_MDATE"]);
                            string CNTR_NO = Prolink.Math.GetValueAsString(cntrDr["CNTR_NO"]);
                            string CNTR_TYPE = Prolink.Math.GetValueAsString(cntrDr["CNTR_TYPE"]);
                            string DlvArea = Prolink.Math.GetValueAsString(cntrDr["DLV_AREA"]);

                            string ReserveNo = Prolink.Math.GetValueAsString(cntrDr["RESERVE_NO"]);

                            if (CNTR_TYPE == "20GP")
                            {
                                CNTR_TYPE = "F4";
                            }
                            else if (CNTR_TYPE == "40GP")
                            {
                                CNTR_TYPE = "F5";
                            }
                            else if (CNTR_TYPE == "40HQ")
                            {
                                CNTR_TYPE = "F6";
                            }


                            sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD={2} AND I_TYPE='DSTF' ORDER BY S_DAY DESC";
                            sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TerminalCd));
                            DataTable qdt = CommonHelp.getDataTableFromSql(sql);

                            if (qdt.Rows.Count == 0)
                            {
                                sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD is null AND I_TYPE='DSTF' ORDER BY S_DAY DESC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea));
                                qdt = CommonHelp.getDataTableFromSql(sql);

                                if (qdt.Rows.Count > 0)
                                {
                                    sql = "SELECT * FROM SMQTI WHERE CARRIER_CD={0} AND CNT_TYPE={1} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={2} AND POD_CD={3} AND TERMINAL_CD is null AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea));
                                    qdt = CommonHelp.getDataTableFromSql(sql);

                                    if (qdt.Rows.Count == 0)
                                    {
                                        sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE={0} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={1} AND POD_CD={2} AND TERMINAL_CD is null AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea));
                                        qdt = CommonHelp.getDataTableFromSql(sql);
                                    }

                                    if (qdt.Rows.Count == 0)
                                    {
                                        sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE is null AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD is null AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea));
                                        qdt = CommonHelp.getDataTableFromSql(sql);
                                    }
                                }
                            }
                            else
                            {
                                sql = "SELECT * FROM SMQTI WHERE CARRIER_CD={0} AND CNT_TYPE={1} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={2} AND POD_CD={3} AND TERMINAL_CD={4} AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TerminalCd));
                                qdt = CommonHelp.getDataTableFromSql(sql);

                                if (qdt.Rows.Count == 0)
                                {
                                    sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE={0} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={1} AND POD_CD={2} AND TERMINAL_CD={3} AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TerminalCd));
                                    qdt = CommonHelp.getDataTableFromSql(sql);
                                }

                                if (qdt.Rows.Count == 0)
                                {
                                    sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE is null AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD={2} AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TerminalCd));
                                    qdt = CommonHelp.getDataTableFromSql(sql);
                                }
                            }

                            if (RebulidDate.ToString("yyyy-MM-dd") != "0001-01-01" && PodMdate.ToString("yyyy-MM-dd") != "0001-01-01")
                            {
                                #region Storage
                                int tmpDay = 0;
                                TimeSpan ts = RebulidDate - PodMdate;
                                int Day = ts.Days;
                                Day = Day + 1;
                                DataRow[] qdr = qdt.Select(" I_TYPE='DSTF' AND S_DAY <=" + Day, "S_DAY DESC");
                                if (qdr.Length == 0)
                                {
                                    emptyMessage.Add(string.Format(CNTR_NO + ":Warehouse storage fee. CMP={0},Delivery Area={1},Terminal={2} cann't find port fee date", Location, DlvArea, TerminalCd));
                                }
                                for (int i = 0; i < qdr.Length; i++)
                                {
                                    int Sday = Prolink.Math.GetValueAsInt(qdr[i]["S_DAY"]);
                                    int Eday = Prolink.Math.GetValueAsInt(qdr[i]["E_DAY"]);
                                    decimal FeePerDay = Prolink.Math.GetValueAsDecimal(qdr[i]["FEE_PER_DAY"]);
                                    string CalType = Prolink.Math.GetValueAsString(qdr[i]["CAL_TYPE"]);
                                    decimal Percentage = Prolink.Math.GetValueAsDecimal(qdr[i]["PERCENTAGE"]);
                                    string FobCif = Prolink.Math.GetValueAsString(qdr[i]["FOB_CIF"]);
                                    decimal TermAmt = 0;

                                    if (Percentage == 0)
                                    {
                                        Percentage = 100;
                                    }

                                    if (i == 0)
                                    {
                                        StorageCur = Prolink.Math.GetValueAsString(qdr[i]["CUR"]);
                                    }


                                    if (FeePerDay == 0)
                                    {
                                        break;
                                    }

                                    tmpDay = Day - Sday + 1;
                                    Day = Day - tmpDay;

                                    if (CalType == "C")
                                    {
                                        WsfAmt = WsfAmt + (tmpDay * FeePerDay * System.Math.Round(Percentage / 100, 2));

                                    }
                                    else
                                    {
                                        WsfAmt = WsfAmt + (FeePerDay * System.Math.Round(Percentage / 100, 2));
                                    }

                                }
                                #endregion

                                if (WsfAmt > 0)
                                {
                                    sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='A' AND CHG_CD='WSF' AND QUOT_NO={0}", SQLUtils.QuotedStr(QuotNo));
                                    DataTable fdt = CommonHelp.getDataTableFromSql(sql);

                                    DataRow[] fdtRows = GetSmqtdRows(ChcekFlag, fdt, headOffice);
                                    
                                    if (fdtRows != null && fdtRows.Length > 0)
                                    {
                                        Bill.WriteLog(string.Format("Local费用报价:{0},WSF費用,{1}", QuotNo, CNTR_NO), ShipmentId);
                                        foreach (DataRow fdr in fdtRows)
                                        {  
                                            amt = WsfAmt;
                                            WsfAmt = CalCualteFeeHandle.chkPunit(dt, fdr, WsfAmt);
                                            fdr["F3"] = WsfAmt;
                                            CalCualteFeeHandle.QtiFee2Smbid(dt, fdr, emptyMessage, StorageCur, amt);
                                        }
                                    }
                                }

                            }
                            else
                            {
                                emptyMessage.Add(CNTR_NO + ":Warehouse storage fee. pod date is null");
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (DataRow odr in odt.Rows)
                {
                    string Oordno = Prolink.Math.GetValueAsString(odr["O_ORDNO"]);
                    DateTime RebulidDate = Prolink.Math.GetValueAsDateTime(odr["CREATE_DATE"]);

                    sql = "SELECT * FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(Oordno);
                    DataTable dnDt = CommonHelp.getDataTableFromSql(sql);

                    if (dnDt.Rows.Count > 0)
                    {
                        foreach (DataRow dnDr in dnDt.Rows)
                        {
                            DateTime PodMdate = Prolink.Math.GetValueAsDateTime(dnDr["POD_MDATE"]);
                            string DnNo = Prolink.Math.GetValueAsString(dnDr["DN_NO"]);
                            string DlvArea = Prolink.Math.GetValueAsString(dnDr["DLV_AREA"]);
                            string ReserveNo = Prolink.Math.GetValueAsString(dnDr["RESERVE_NO"]);

                            sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD={2} ORDER BY S_DAY DESC";
                            sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TerminalCd));
                            DataTable qdt = CommonHelp.getDataTableFromSql(sql);

                            if (qdt.Rows.Count == 0)
                            {
                                sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} ORDER BY S_DAY DESC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(DlvArea));
                                qdt = CommonHelp.getDataTableFromSql(sql);
                            }

                            if (RebulidDate.ToString("yyyy-MM-dd") != "0001-01-01" && PodMdate.ToString("yyyy-MM-dd") != "0001-01-01")
                            {
                                #region Storage
                                int tmpDay = 0;
                                TimeSpan ts = RebulidDate - PodMdate;
                                int Day = ts.Days;
                                Day = Day + 1;
                                DataRow[] qdr = qdt.Select(" I_TYPE='DSTF' AND S_DAY <=" + Day, "S_DAY DESC");
                                if (qdr.Length == 0)
                                {
                                    emptyMessage.Add(string.Format(DnNo + ":Warehouse storage fee. CMP={0},Delivery Area={1} cann't find port fee date", Location, DlvArea));
                                }
                                for (int i = 0; i < qdr.Length; i++)
                                {
                                    int Sday = Prolink.Math.GetValueAsInt(qdr[i]["S_DAY"]);
                                    int Eday = Prolink.Math.GetValueAsInt(qdr[i]["E_DAY"]);
                                    decimal FeePerDay = Prolink.Math.GetValueAsDecimal(qdr[i]["FEE_PER_DAY"]);
                                    string CalType = Prolink.Math.GetValueAsString(qdr[i]["CAL_TYPE"]);
                                    decimal Percentage = Prolink.Math.GetValueAsDecimal(qdr[i]["PERCENTAGE"]);
                                    string FobCif = Prolink.Math.GetValueAsString(qdr[i]["FOB_CIF"]);
                                    decimal TermAmt = 0;

                                    if (Percentage == 0)
                                    {
                                        Percentage = 100;
                                    }

                                    if (i == 0)
                                    {
                                        StorageCur = Prolink.Math.GetValueAsString(qdr[i]["CUR"]);
                                    }


                                    if (FeePerDay == 0)
                                    {
                                        break;
                                    }

                                    tmpDay = Day - Sday + 1;
                                    Day = Day - tmpDay;

                                    if (CalType == "C")
                                    {
                                        WsfAmt = WsfAmt + (tmpDay * FeePerDay * System.Math.Round(Percentage / 100, 2));

                                    }
                                    else
                                    {
                                        WsfAmt = WsfAmt + (FeePerDay * System.Math.Round(Percentage / 100, 2));
                                    }

                                }
                                #endregion

                                if (WsfAmt > 0)
                                {
                                    sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='A' AND CHG_CD='WSF' AND QUOT_NO={0}", SQLUtils.QuotedStr(QuotNo));
                                    DataTable fdt = CommonHelp.getDataTableFromSql(sql);

                                    DataRow[] fdtRows = GetSmqtdRows(ChcekFlag, fdt, headOffice); 

                                    if (fdtRows != null && fdtRows.Length > 0)
                                    {
                                        Bill.WriteLog(string.Format("Local费用报价:{0},WSF費用", QuotNo), ShipmentId);
                                        foreach (DataRow fdr in fdtRows)
                                        {  
                                            fdr["F3"] = WsfAmt;
                                            CalCualteFeeHandle.QtiFee2Smbid(dt, fdr, emptyMessage, StorageCur, WsfAmt);
                                        }
                                    }
                                    else
                                    {
                                        emptyMessage.Add(QuotNo + ":cann't find Warehouse storage fee");
                                    }
                                }
                            }
                            else
                            {
                                emptyMessage.Add(DnNo + ":Warehouse storage fee. pod date is null");
                            }

                        }
                    }
                }
            }
            return string.Empty;
        }

        private string LocalISFTFee(string ShipmentId, string QuotNo, DataTable dt, List<string> emptyMessage, bool ChcekFlag, string headOffice)
        {
            string Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
            string Location = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
            string TerminalCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TERMINAL_CD"]);
            string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string sql;
            decimal amt = 0;
            string StorageCur = string.Empty;
            if (TranType == "F" || TranType == "R")
            {
                sql = "SELECT * FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                DataTable cntrDt = CommonHelp.getDataTableFromSql(sql);

                if (cntrDt.Rows.Count > 0)
                {
                    foreach (DataRow cntrDr in cntrDt.Rows)
                    {
                        string TRAN_TYPE1 = Prolink.Math.GetValueAsString(cntrDr["TRAN_TYPE1"]);
                        string TRAN_TYPE2 = Prolink.Math.GetValueAsString(cntrDr["TRAN_TYPE2"]);
                        string CNTR_NO = Prolink.Math.GetValueAsString(cntrDr["CNTR_NO"]);
                        string CNTR_TYPE = Prolink.Math.GetValueAsString(cntrDr["CNTR_TYPE"]);
                        decimal FobAmt = Prolink.Math.GetValueAsDecimal(cntrDr["FOB_AMT"]);
                        decimal CifAmt = Prolink.Math.GetValueAsDecimal(cntrDr["CIF_AMT"]);

                        if (CNTR_TYPE == "20GP")
                        {
                            CNTR_TYPE = "F4";
                        }
                        else if (CNTR_TYPE == "40GP")
                        {
                            CNTR_TYPE = "F5";
                        }
                        else if (CNTR_TYPE == "40HQ")
                        {
                            CNTR_TYPE = "F6";
                        }

                        if ((TRAN_TYPE1 == "R" || TRAN_TYPE1 == "S" || TRAN_TYPE1 == "A" || TRAN_TYPE1 == "I") && TRAN_TYPE2 == "T")
                        {
                            sql = "SELECT * FROM SMRCNTR WHERE CNTR_NO=" + SQLUtils.QuotedStr(CNTR_NO);
                            DataTable rcntrDt = CommonHelp.getDataTableFromSql(sql);

                            DateTime PodMdate = new DateTime();
                            DateTime PickDate = new DateTime();
                            decimal IstfAmt = 0;
                            string RvPodCd = string.Empty;
                            if (rcntrDt.Rows.Count > 0)
                            {
                                foreach (DataRow rcntrDr in rcntrDt.Rows)
                                {
                                    string ReserveNo = Prolink.Math.GetValueAsString(rcntrDr["RESERVE_NO"]);

                                    sql = "SELECT CALL_TYPE FROM SMIRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                    string CallType = CommonHelp.getOneValueAsStringFromSql(sql);

                                    if (CallType == "S")
                                    {
                                        PodMdate = Prolink.Math.GetValueAsDateTime(rcntrDr["POD_MDATE"]);
                                    }

                                    if (CallType == "C")
                                    {
                                        PickDate = Prolink.Math.GetValueAsDateTime(rcntrDr["PICKUP_DATE"]);
                                        sql = "SELECT PICK_AREA FROM SMIRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                        RvPodCd = CommonHelp.getOneValueAsStringFromSql(sql);
                                    }
                                }

                                sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD={2} AND I_TYPE='DSTF' ORDER BY S_DAY DESC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd), SQLUtils.QuotedStr(TerminalCd));
                                DataTable qdt = CommonHelp.getDataTableFromSql(sql);

                                if (qdt.Rows.Count == 0)
                                {
                                    sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD is null AND I_TYPE='DSTF' ORDER BY S_DAY DESC";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd));
                                    qdt = CommonHelp.getDataTableFromSql(sql);

                                    if (qdt.Rows.Count > 0)
                                    {
                                        sql = "SELECT * FROM SMQTI WHERE CARRIER_CD={0} AND CNT_TYPE={1} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={2} AND POD_CD={3} AND TERMINAL_CD is null AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd));
                                        qdt = CommonHelp.getDataTableFromSql(sql);

                                        if (qdt.Rows.Count == 0)
                                        {
                                            sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE={0} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={1} AND POD_CD={2} AND TERMINAL_CD is null AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                            sql = string.Format(sql, SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd));
                                            qdt = CommonHelp.getDataTableFromSql(sql);
                                        }

                                        if (qdt.Rows.Count == 0)
                                        {
                                            sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE is null AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD is null AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                            sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd));
                                            qdt = CommonHelp.getDataTableFromSql(sql);
                                        }
                                    }
                                }
                                else
                                {
                                    sql = "SELECT * FROM SMQTI WHERE CARRIER_CD={0} AND CNT_TYPE={1} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={2} AND POD_CD={3} AND TERMINAL_CD={4} AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd), SQLUtils.QuotedStr(TerminalCd));
                                    qdt = CommonHelp.getDataTableFromSql(sql);

                                    if (qdt.Rows.Count == 0)
                                    {
                                        sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE={0} AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={1} AND POD_CD={2} AND TERMINAL_CD={3} AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(CNTR_TYPE), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd), SQLUtils.QuotedStr(TerminalCd));
                                        qdt = CommonHelp.getDataTableFromSql(sql);
                                    }

                                    if (qdt.Rows.Count == 0)
                                    {
                                        sql = "SELECT * FROM SMQTI WHERE CARRIER_CD is null AND CNT_TYPE is null AND U_ID IN (SELECT U_ID FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD={2} AND I_TYPE='DSTF') ORDER BY S_DAY DESC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(RvPodCd), SQLUtils.QuotedStr(TerminalCd));
                                        qdt = CommonHelp.getDataTableFromSql(sql);
                                    }
                                }

                                if (PickDate.ToString("yyyy-MM-dd") != "0001-01-01" && PodMdate.ToString("yyyy-MM-dd") != "0001-01-01")
                                {
                                    #region Storage
                                    int tmpDay = 0;
                                    TimeSpan ts = PickDate - PodMdate;
                                    int Day = ts.Days;
                                    Day = Day + 1;
                                    DataRow[] qdr = qdt.Select(" I_TYPE='DSTF' AND S_DAY <=" + Day, "S_DAY DESC");
                                    for (int i = 0; i < qdr.Length; i++)
                                    {
                                        int Sday = Prolink.Math.GetValueAsInt(qdr[i]["S_DAY"]);
                                        int Eday = Prolink.Math.GetValueAsInt(qdr[i]["E_DAY"]);
                                        decimal FeePerDay = Prolink.Math.GetValueAsDecimal(qdr[i]["FEE_PER_DAY"]);
                                        string CalType = Prolink.Math.GetValueAsString(qdr[i]["CAL_TYPE"]);
                                        decimal Percentage = Prolink.Math.GetValueAsDecimal(qdr[i]["PERCENTAGE"]);
                                        string FobCif = Prolink.Math.GetValueAsString(qdr[i]["FOB_CIF"]);
                                        decimal TermAmt = 0;

                                        if (Percentage == 0)
                                        {
                                            Percentage = 100;
                                        }

                                        if (i == 0)
                                        {
                                            StorageCur = Prolink.Math.GetValueAsString(qdr[i]["CUR"]);
                                        }


                                        if (FeePerDay == 0)
                                        {
                                            break;
                                        }

                                        tmpDay = Day - Sday + 1;
                                        Day = Day - tmpDay;

                                        if (CalType == "C")
                                        {
                                            IstfAmt = IstfAmt + (tmpDay * FeePerDay * System.Math.Round(Percentage / 100, 2));

                                        }
                                        else
                                        {
                                            IstfAmt = IstfAmt + (FeePerDay * System.Math.Round(Percentage / 100, 2));
                                        }

                                    }
                                    #endregion

                                    if (IstfAmt > 0)
                                    {
                                        sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='A' AND CHG_CD='ISTF' AND QUOT_NO={0}", SQLUtils.QuotedStr(QuotNo));
                                        DataTable fdt = CommonHelp.getDataTableFromSql(sql);

                                        DataRow[] fdtRows = GetSmqtdRows(ChcekFlag, fdt, headOffice);
                                         
                                        if (fdtRows != null && fdtRows.Length > 0)
                                        {
                                            Bill.WriteLog(string.Format("Local费用报价:{0},ISTF費用,{1}", QuotNo, CNTR_NO), ShipmentId);
                                            foreach (DataRow fdr in fdtRows)
                                            { 
                                                fdr["F3"] = IstfAmt;
                                                CalCualteFeeHandle.QtiFee2Smbid(dt, fdr, emptyMessage, StorageCur, IstfAmt); 
                                            }
                                        }
                                        else
                                        {
                                            emptyMessage.Add(QuotNo + ":cann't find Intermodal Storage Fee");
                                        }
                                    }
                                }


                            }


                        }
                    }
                }
            }
            else
            {
                sql = "SELECT * FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                DataTable smiDt = CommonHelp.getDataTableFromSql(sql);

                if (smiDt.Rows.Count > 0)
                {
                    foreach (DataRow smiDr in smiDt.Rows)
                    {
                        string TRAN_TYPE1 = Prolink.Math.GetValueAsString(smiDr["TRAN_TYPE1"]);
                        string TRAN_TYPE2 = Prolink.Math.GetValueAsString(smiDr["TRAN_TYPE2"]);
                        DateTime PodMdate = new DateTime();

                        if ((TRAN_TYPE1 == "R" || TRAN_TYPE1 == "S" || TRAN_TYPE1 == "A") && TRAN_TYPE2 == "T")
                        {
                            sql = "SELECT RESERVE_NO FROM SMIRV WHERE CALL_TYPE='S' AND STATUS <> 'V' AND SHIPMENT_INFO LIKE '%{0}%'";
                            sql = string.Format(sql, ShipmentId);
                            string ReserveNo = CommonHelp.getOneValueAsStringFromSql(sql);

                            sql = "SELECT * FROM SMRCNTR WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                            DataTable cntrDt = CommonHelp.getDataTableFromSql(sql);

                            if (cntrDt.Rows.Count > 0)
                            {
                                PodMdate = Prolink.Math.GetValueAsDateTime(cntrDt.Rows[0]["POD_MDATE"]);
                            }


                            sql = "SELECT RESERVE_NO FROM SMIRV WHERE CALL_TYPE='C' AND STATUS <> 'V' AND SHIPMENT_INFO LIKE '%{0}%'";
                            sql = string.Format(sql, ShipmentId);
                            DataTable rvDt = CommonHelp.getDataTableFromSql(sql);

                            decimal IstfAmt = 0;
                            if (rvDt.Rows.Count > 0)
                            {
                                foreach (DataRow rvDr in rvDt.Rows)
                                {
                                    ReserveNo = Prolink.Math.GetValueAsString(rvDr["RESERVE_NO"]);
                                    DateTime PickDate = Prolink.Math.GetValueAsDateTime(rvDr["USE_DATE"]);

                                    sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD={2} ORDER BY S_DAY DESC";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(TerminalCd));
                                    DataTable qdt = CommonHelp.getDataTableFromSql(sql);

                                    if (qdt.Rows.Count == 0)
                                    {
                                        sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} ORDER BY S_DAY DESC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd));
                                        qdt = CommonHelp.getDataTableFromSql(sql);
                                    }

                                    if (PickDate.ToString("yyyy-MM-dd") != "0001-01-01" && PodMdate.ToString("yyyy-MM-dd") != "0001-01-01")
                                    {
                                        #region Storage
                                        int tmpDay = 0;
                                        TimeSpan ts = PodMdate - PickDate;
                                        int Day = ts.Days;
                                        Day = Day + 1;
                                        DataRow[] qdr = qdt.Select(" I_TYPE='DSTF' AND S_DAY <=" + Day, "S_DAY DESC");
                                        for (int i = 0; i < qdr.Length; i++)
                                        {
                                            int Sday = Prolink.Math.GetValueAsInt(qdr[i]["S_DAY"]);
                                            int Eday = Prolink.Math.GetValueAsInt(qdr[i]["E_DAY"]);
                                            decimal FeePerDay = Prolink.Math.GetValueAsDecimal(qdr[i]["FEE_PER_DAY"]);
                                            string CalType = Prolink.Math.GetValueAsString(qdr[i]["CAL_TYPE"]);
                                            decimal Percentage = Prolink.Math.GetValueAsDecimal(qdr[i]["PERCENTAGE"]);
                                            string FobCif = Prolink.Math.GetValueAsString(qdr[i]["FOB_CIF"]);
                                            decimal TermAmt = 0;

                                            if (Percentage == 0)
                                            {
                                                Percentage = 100;
                                            }

                                            if (i == 0)
                                            {
                                                StorageCur = Prolink.Math.GetValueAsString(qdr[i]["CUR"]);
                                            }


                                            if (FeePerDay == 0)
                                            {
                                                break;
                                            }

                                            tmpDay = Day - Sday + 1;
                                            Day = Day - tmpDay;

                                            if (CalType == "C")
                                            {
                                                IstfAmt = IstfAmt + (tmpDay * FeePerDay * System.Math.Round(Percentage / 100, 2));

                                            }
                                            else
                                            {
                                                IstfAmt = IstfAmt + (FeePerDay * System.Math.Round(Percentage / 100, 2));
                                            }

                                        }
                                        #endregion

                                        if (IstfAmt > 0)
                                        {
                                            sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='A' AND CHG_CD='ISTF' AND QUOT_NO={0}", SQLUtils.QuotedStr(QuotNo));
                                            DataTable fdt = CommonHelp.getDataTableFromSql(sql);
                                            DataRow[] fdtRows = GetSmqtdRows(ChcekFlag, fdt, headOffice);

                                            if (fdtRows != null && fdtRows.Length > 0)
                                            {
                                                Bill.WriteLog(string.Format("Local费用报价:{0},ISTF費用", QuotNo), ShipmentId);
                                                foreach (DataRow fdr in fdtRows)
                                                {
                                                    amt = IstfAmt;
                                                    IstfAmt = CalCualteFeeHandle.chkPunit(dt, fdr, IstfAmt);
                                                    fdr["F3"] = IstfAmt;
                                                    CalCualteFeeHandle.QtiFee2Smbid(dt, fdr, emptyMessage, StorageCur, amt);
                                                }
                                            }
                                            else
                                            {
                                                emptyMessage.Add(QuotNo + ":cann't find Intermodal Storage Fee");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        emptyMessage.Add(ReserveNo + ":Intermodal Storage Fee. pick up date or pod date is null");
                                    }
                                }
                            }


                        }
                    }
                }
            }

            return sql;
        }

        private void LocalStorageDemurageDetention(string ShipmentId, DataTable dt, DataTable mdt, List<string> emptyMessage, bool ChcekFlag, string headOffice)
        {
            CalCualteFeeHandle.LocalStorageDemurageDetention(ShipmentId, dt, mdt, emptyMessage,ChcekFlag, headOffice); 
        }


        private void LocalISPSDocFee(string ShipmentId, DataRow dr, string Location, string Eta, string QuotNo, List<string> emptyMessage, bool ChcekFlag, string headOffice)
        {
            string sql = "SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM FROM SMQTD WHERE REPAY='A' AND CHG_CD IN ('ISPS', 'DDOC') AND QUOT_NO=" + SQLUtils.QuotedStr(QuotNo);
            DataTable fdt = CommonHelp.getDataTableFromSql(sql);
            //if(fdt.Rows.Count)
            if (fdt.Rows.Count > 0)
            {
                foreach (DataRow fdr in fdt.Rows)
                {
                    string Carrier = Prolink.Math.GetValueAsString(fdr["CARRIER"]);
                    if (Carrier != "")
                    {
                        sql = "SELECT TOP 1 * FROM SMQTM WHERE TRAN_MODE='O' AND LSP_CD={0} AND RLOCATION={1} AND OUT_IN='I' AND EFFECT_FROM <= {2} AND EFFECT_TO >= {2} AND  QUOT_TYPE='A' ORDER BY EFFECT_FROM DESC, CREATE_BY DESC";
                        sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(Eta));
                        DataTable mmdt = CommonHelp.getDataTableFromSql(sql);

                        if (mmdt.Rows.Count > 0)
                        {
                            string QuotNo1 = Prolink.Math.GetValueAsString(mmdt.Rows[0]["QUOT_NO"]);
                            Bill.WriteLog(string.Format("Local费用报价:{0}.ISPS, Doc費用", QuotNo1), ShipmentId);

                            string ChgCd = Prolink.Math.GetValueAsString(fdr["CHG_CD"]);
                            sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='M' AND QUOT_NO={0} AND CHG_CD={1}", SQLUtils.QuotedStr(QuotNo1), SQLUtils.QuotedStr(ChgCd));
                            DataTable ffdt = CommonHelp.getDataTableFromSql(sql);


                            DataRow[] ffdtRows = GetSmqtdRows(ChcekFlag, ffdt, headOffice);

                            if (ffdtRows != null && ffdtRows.Length > 0)
                            {
                                foreach (DataRow ffdr in ffdtRows)
                                {
                                    SeaPublicFee2Smbid(dr, ffdr, emptyMessage); 
                                }
                            }
                        }
                    }
                }
            }
        }

        ///算出必收費用
        private void LocalMustGetFee(string ShipmentId, DataRow dr, string Carrier, string QuotNo, List<string> emptyMessage, bool ChcekFlag, string headOffice)
        {
            string sql = "SELECT CHG_CD FROM SMQTD WHERE REPAY='M' AND QUOT_NO=" + SQLUtils.QuotedStr(QuotNo) + " GROUP BY CHG_CD";
            DataTable fdt_c = CommonHelp.getDataTableFromSql(sql);
            if (fdt_c.Rows.Count == 0)
            {
                emptyMessage.Add(QuotNo + ":Cann't find must be charged");
                return;
            }
            if (fdt_c.Rows.Count > 0)
            {
                Bill.WriteLog(string.Format("Local费用报价:{0},必收费用", QuotNo), ShipmentId);
                foreach (DataRow fdr_c in fdt_c.Rows)
                {
                    int i = 0;
                    string FeeCode = Prolink.Math.GetValueAsString(fdr_c["CHG_CD"]);

                    sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='M' AND QUOT_NO={0} AND CHG_CD={1}", SQLUtils.QuotedStr(QuotNo), SQLUtils.QuotedStr(FeeCode));
                    DataTable fdt_m = CommonHelp.getDataTableFromSql(sql);

                    string testCarrier = (Carrier != null ? Carrier.ToUpper().Trim() : Carrier);
                    DataRow[] fdtRows = GetSmqtdRows1(ChcekFlag, fdt_m, headOffice, testCarrier);
                    if (ChcekFlag && (fdtRows == null || fdtRows.Length <= 0)) {
                        emptyMessage.Add(QuotNo + ":" + FeeCode + ":Fee Forwarder is not equal shipment Forwarder and Fee Forwarder is not null");
                        emptyMessage.Add(QuotNo + ":Fee carrier is not equal shipment carrier and Fee carrier is not null");
                    }
                    if (fdtRows != null && fdtRows.Length > 0)
                    {
                        foreach (DataRow fdr in fdtRows)
                        {
                            SeaPublicFee2Smbid(dr, fdr, emptyMessage);
                        }
                    }

                    i++;
                }
            }
        }

        public static DataRow[] GetSmqtdRows(bool ChcekFlag, DataTable dt, string headOffice)
        {
            if (dt == null || dt.Rows.Count <= 0) return null;

            DataRow[] rows = dt.Select("1=1");
            if (ChcekFlag && dt.Rows.Count > 0)
            {
                rows = dt.Select(string.Format("SP_CD={0}", SQLUtils.QuotedStr(headOffice)));
                if (rows.Length <= 0)
                {
                    rows = dt.Select("(SP_CD IS NULL OR SP_CD='')");
                }
            }
            return rows;
        }

        public static DataRow[] GetSmqtdRows1(bool ChcekFlag, DataTable dt, string headOffice, string carrier)
        {
            if (dt == null || dt.Rows.Count <= 0) return null;

            DataRow[] rows = dt.Select("1=1");
            if (ChcekFlag && dt.Rows.Count > 0)
            {
                rows = dt.Select(string.Format("SP_CD={0} AND CARRIER={1}", SQLUtils.QuotedStr(headOffice), SQLUtils.QuotedStr(carrier)));
                if (rows.Length <= 0)
                {
                    rows = dt.Select(string.Format("(SP_CD IS NULL OR SP_CD='') AND CARRIER={0}", SQLUtils.QuotedStr(carrier)));
                }
                if (rows.Length <= 0)
                {
                    rows = dt.Select(string.Format("SP_CD={0} AND (CARRIER IS NULL OR CARRIER='')", SQLUtils.QuotedStr(headOffice)));
                }
                if (rows.Length <= 0)
                {
                    rows = dt.Select("(SP_CD IS NULL OR SP_CD='') AND(CARRIER IS NULL OR CARRIER='')");
                }
            }
            return rows;
        }
        #endregion

        /// <summary>
        /// OutBound Local Freight
        /// </summary>
        /// <param name="emptyMessage"></param>
        /// <returns></returns>
        private string CalOutBoundLocalFee(List<string> emptyMessage)
        {
            string Msg = "success";
            string pod = _freight.Pod;
            string cmp = _freight.Cmp;
            string tranType = _freight.TranType;
            string frtTerm = _freight.FrtTerm;
            string incotermCd = _freight.IncotermCd;
            if (ChgType == null || !ChgType.Contains("OLC")) 
                return Msg; 
            string qtdate_condition = string.Format(" AND SMQTM.EFFECT_FROM<={0} AND SMQTM.EFFECT_TO>={0}", SQLUtils.QuotedStr(_freight.BillDate));
            string local_sql = string.Format(@"SELECT * FROM(SELECT IS_SHARE,[U_ID],[RFQ_NO],[QUOT_NO],
            [QUOT_TYPE],[SEQ_NO],[TRAN_MODE],[OUT_IN],[LSP_CD],[EFFECT_DATE],[EFFECT_TO],[TRAN_TYPE],[REGION],
            [STATE],[POD_CD],[POD_NM],[POL_CD],[POL_NM],[VIA_CD],[VIA_NM],[CARRIER],[ALL_IN],[CUR],
            F1,F2,F3,F4,F5,F6,F7,F8,F9,F10,F11,F12,F13,F14,F15,F16,F17,F18,F19,F20,F21,F22,F23,F24,F25,
            F26,F27,F28,F29,F30,F31,F32,F33,F34,F35,F36,F37,F38,F39,F40,F41,F42,F43,F44,F45,F46,F47,F48,F49,F50,
            [PUNIT],[CHG_CD],[CHG_DESCP],[CHG_TYPE],[SAILING_DAY],[FREE_ODT],[FREE_ODM],[FREE_DDT],[FREE_DDM],
            [TT],[NOTE],[REMARK],[MIN_AMT],[U_FID],[SERVICE_MODE],[LOADING_FROM],[LOADING_TO],[CUT_OFF],[ETD],
            [REPAY],[SHIP_MODE],[INCOTERM_CD],[VAT_RATE] FROM SMQTD WHERE TRAN_MODE='X'  
            AND EXISTS (SELECT RFQ_NO FROM SMQTM WHERE (SMQTM.POD_CD IS NULL OR SMQTM.POD_CD={0}) AND SMQTM.QUOT_NO=SMQTD.QUOT_NO 
            AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='X' AND RLOCATION={1} {2} AND TRAN_TYPE={3}))A 
            OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUST_CD,CUR AS M_CUR,INCOTERM,CREDIT_TO,
            CREDIT_NM,FREIGHT_TERM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B 
            ORDER BY QT_EFFECT_FROM DESC,QUOT_NO",
            SQLUtils.QuotedStr(pod), SQLUtils.QuotedStr(cmp), qtdate_condition, SQLUtils.QuotedStr(tranType));
             
            DataTable dt = OperationUtils.GetDataTable(local_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
             
            string partyNo = _freight.SpPartyNo;
            string partyNm = _freight.SpPartyNm;
            bool localFlag = false;
            DataTable localDt = null;
            if ("F".Equals(tranType))
            {
                localFlag = true;
                if (!"P".Equals(frtTerm))
                {
                    if (!string.IsNullOrEmpty(_freight.BoPartyNo))
                    {
                        partyNo = _freight.BoPartyNo;
                        partyNm = _freight.BoPartyNm;
                    }
                       
                }
                else
                    frtTerm = "C";
            }
            else if (("P".Equals(frtTerm) && "E".Equals(tranType) && "DDP".Equals(incotermCd))
                || (("R".Equals(tranType) || "L".Equals(tranType) || "A".Equals(tranType)) && !"EXW".Equals(incotermCd)))
                localFlag = true;
            if (localFlag) 
                localDt = Helper.CloneQTTable1(dt, partyNo, tranType, frtTerm, true, "", ""); 

            string sm_pod = _freight.Pod;
            string cout = _freight.Cout;

            if (localDt == null) return "";
            Helper.AddOthColumns(localDt);
            decimal total = 0M;
            int index = 1;
             
            List<string> msg = new List<string>();
            string tranMode = _freight.TranType;
            string shipMode = _freight.ShipMode;
            string sm_incotermCd = _freight.IncotermCd;
            string sm_carrier = Helper.GetUrlDecodeValue(_freight.Carrier);
            List<string> charge_type = GetChargeType(_freight.DebitToList, new List<string> { DESTINATION_CHARGE }, new List<string> { "O" });
             
            string filter = string.Empty;
            string filter1 = string.Empty;

            if (!string.IsNullOrEmpty(_freight.FcPartyNo))
                filter = " AND CUST_CD=" + SQLUtils.QuotedStr(_freight.FcPartyNo);
            filter1 = " AND (SHIP_MODE='' OR SHIP_MODE IS NULL)";
            if (!string.IsNullOrEmpty(shipMode))
                filter1 = string.Format(" AND (SHIP_MODE={0} OR SHIP_MODE='' OR SHIP_MODE IS NULL)", SQLUtils.QuotedStr(shipMode));

            DataRow[] drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter + filter1, "QT_EFFECT_FROM DESC,QUOT_NO");
            if (drs.Length <= 0 && !string.IsNullOrEmpty(filter))
            {
                filter = " AND (CUST_CD='' OR CUST_CD IS NULL)";
                drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter + filter1, "QT_EFFECT_FROM DESC,QUOT_NO");
            }
             
            List<string> testList = new List<string>();
            List<string> chgList1 = new List<string>();
            MixedList ml = new MixedList();
            List<string> test = new List<string>();

            if (drs.Length <= 0)
            { 
                Bill.WriteLog(string.Format(_freight.BrPartyNo + ":无OutBound Local费用报价"), _freight.ShipmentId);
                emptyMessage.Add(string.Format(_freight.ShipmentId + ":LSP CD={0},CMP={1},{2}={3},Cann't find the Local Quotation!", partyNo, _freight.Cmp, _freight.BillDescp, _freight.BillDate));
            }

            for (int i = 0; i < drs.Length; i++)
            {
                DataRow local = drs[i];
                string uid = Prolink.Math.GetValueAsString(local["U_ID"]);
                string credit_to = Prolink.Math.GetValueAsString(local["CREDIT_TO"]);
                string credit_nm = Prolink.Math.GetValueAsString(local["CREDIT_NM"]);
                string repay = Prolink.Math.GetValueAsString(local["REPAY"]);
                string chg_cd = Prolink.Math.GetValueAsString(local["CHG_CD"]);
                string quot_no = Prolink.Math.GetValueAsString(local["QUOT_NO"]);
                string carrier = Prolink.Math.GetValueAsString(local["CARRIER"]).Replace("&nbsp;", " ");
                string is_share = Prolink.Math.GetValueAsString(local["IS_SHARE"]);
                string pod_cd = Prolink.Math.GetValueAsString(local["POD_CD"]);
                string cur = Prolink.Math.GetValueAsString(local["CUR"]);
                string incoterm = Prolink.Math.GetValueAsString(local["INCOTERM_CD"]);
                if (string.IsNullOrEmpty(quot_no))
                    continue;
                if (testList.Count <= 0) testList.Add(quot_no);
                if (!testList.Contains(quot_no)) break;

                if (!string.IsNullOrEmpty(pod_cd) && !pod_cd.Equals(sm_pod))
                    continue;

                if (!string.IsNullOrEmpty(carrier) && !sm_carrier.Equals(carrier))//carrier过滤
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(incoterm) && !sm_incotermCd.Equals(incoterm))
                    continue;
                local = ChangeLocal(sm_carrier, drs, local, chg_cd, quot_no, sm_pod, sm_incotermCd);
                if (local == null)
                    continue;
                uid = Prolink.Math.GetValueAsString(local["U_ID"]);
                credit_to = Prolink.Math.GetValueAsString(local["CREDIT_TO"]);
                credit_nm = Prolink.Math.GetValueAsString(local["CREDIT_NM"]);
                repay = Prolink.Math.GetValueAsString(local["REPAY"]);
                chg_cd = Prolink.Math.GetValueAsString(local["CHG_CD"]);
                quot_no = Prolink.Math.GetValueAsString(local["QUOT_NO"]);
                carrier = Prolink.Math.GetValueAsString(local["CARRIER"]).Replace("&nbsp;", " ");
                is_share = Prolink.Math.GetValueAsString(local["IS_SHARE"]);
                pod_cd = Prolink.Math.GetValueAsString(local["POD_CD"]);
                cur = Prolink.Math.GetValueAsString(local["CUR"]);

                if (test.Contains(chg_cd))
                    continue;
                test.Add(chg_cd);

                bool error = false;
                if ("THC".Equals(chg_cd) && "A".Equals(repay))
                {
                    CalCNT(local, tranMode, ref error, msg, ml, null, partyNo, partyNm);
                    continue;
                }

                if (!"M".Equals(repay))
                {
                    switch (repay)
                    {
                        case "C":
                        case "Y":
                            if (!CheckLocalCCharge(chg_cd))
                            {
                                EditInstruct cei = CalCYCNT(local, ref error, msg, ml);
                                if (cei == null)
                                    cei = CreateCEditInstruct(local, credit_to, credit_nm);
                                ml.Add(cei);

                                continue;
                            }
                            break;
                        default:
                            continue;
                    }
                } 


                if (CalCNT(local, tranMode, ref error, msg, ml, null, partyNo, partyNm))
                    continue; 

                decimal F1 = Prolink.Math.GetValueAsDecimal(local["F3"]);
                string punit = Prolink.Math.GetValueAsString(local["PUNIT"]);
                decimal qty = GetQty(punit);

                qty = GetDocQty(chg_cd, punit, qty);

                decimal cur_total = Helper.Get45AmtValue(qty * F1);
                if ("%".Equals(punit))
                {
                    cur_total = Helper.Get45AmtValue(qty * F1 * 0.01M);
                }
               
                decimal temp1 = 0M;
                local["QEX_RATE"] = Helper.GetTotal(_freight.RateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                if ("%".Equals(punit))
                {
                    local["QEX_RATE"] = 1;
                    F1 = temp1;
                    qty = 1;
                    cur_total = temp1;
                    local["CUR"] = _localCur;
                }
                local["LOCALE_AMT"] = temp1;
                local["EX_REMARK"] = "";
                SetChargeInfo(local, "", tranMode);

                local["QCHG_UNIT"] = punit;
                local["QUNIT_PRICE"] = F1;
                local["QQTY"] = qty;
                local["QAMT"] = cur_total;
                local["C_FLAG"] = "Y";
                if (!"Y".Equals(cout)) 
                    local["IS_SHARE"] = "";

                ml.Add(CreateBillItem(local, credit_to, credit_nm));
            }
            try
            { 
                CalculateLocalAmt(ml, _freight.Current_smsm);
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("Shipment Id:" + _freight.ShipmentId + ",CalOutBoundLocalFee Error:" + ex.ToString());
            }
            return Msg;
        }

        private string CalOutBoundTrailerFee(List<string> emptyMessage)
        {
            string Msg = "success";
            string cmp = _freight.Cmp;
            string tranType = _freight.TranType;
            string pol = _freight.Pol;
            if (ChgType == null || !ChgType.Contains("OTC"))
                return Msg;
            string qtdate_condition = string.Format(" AND SMQTM.EFFECT_FROM<={0} AND SMQTM.EFFECT_TO>={0}", SQLUtils.QuotedStr(_freight.BillDate));
            string sql = string.Format(@"SELECT * FROM (SELECT * FROM SMQTD WHERE POL_CD={0} AND POD_CD={1} AND EXISTS 
            (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='C' AND LSP_CD={3} {2} AND SMQTM.TRAN_TYPE={4}))A 
            OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO",
            SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pol), qtdate_condition, SQLUtils.QuotedStr(_freight.CrPartyNo), SQLUtils.QuotedStr(tranType));
            
            DataTable trailerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (trailerDt.Rows.Count <= 0)
            { 
                Bill.WriteLog(string.Format(_freight.BrPartyNo + ":无OutBound Trailer费用报价"), _freight.ShipmentId);
                emptyMessage.Add(string.Format(_freight.ShipmentId + ":LSP CD={0},CMP={1},{2}={3},Cann't find the Trailer Quotation!", _freight.CrPartyNo, _freight.Cmp, _freight.BillDescp, _freight.BillDate));
            }
            
            TrailerFreight(trailerDt);

            return Msg;
        }

        /// <summary>
        /// 拖车费用
        /// </summary>
        /// <param name="brokerDt"></param>
        /// <param name="parm"></param>
        private void TrailerFreight(DataTable trailerDt)
        {
            DataTable rateDt = _freight.RateDt; 

            Helper.AddOthColumns(trailerDt); 
            string tranMode = _freight.TranType;
            string shipmentId = _freight.ShipmentId; 
            decimal total = 0M;
            MixedList ml = new MixedList();

            List<string> msg = new List<string>();
            List<string> testList = new List<string>();
            foreach (DataRow trailer in trailerDt.Rows)
            {
                string quot_no = Prolink.Math.GetValueAsString(trailer["QUOT_NO"]);
                string repay = Prolink.Math.GetValueAsString(trailer["REPAY"]);
                string chg_cd = Prolink.Math.GetValueAsString(trailer["CHG_CD"]);
                string is_share = Prolink.Math.GetValueAsString(trailer["IS_SHARE"]);
                string cur = Prolink.Math.GetValueAsString(trailer["CUR"]);

                if (string.IsNullOrEmpty(quot_no))
                    continue;
                if (testList.Count <= 0) testList.Add(quot_no);
                if (!testList.Contains(quot_no)) break;
                 
                switch (repay)
                {
                    case "C":
                    case "Y":
                        if (!CheckTrailerCCharge(chg_cd))
                        {
                            EditInstruct cei = CreateCEditInstruct(trailer, _freight.CrPartyNo, _freight.CrPartyNm, "F1");
                            ml.Add(cei);
                            continue;
                        }
                        break;
                    case "M":
                        break;
                    default:
                        continue;
                }
      
                bool error = false;

                decimal F1 = Prolink.Math.GetValueAsDecimal(trailer["F1"]);//20'
                decimal F2 = Prolink.Math.GetValueAsDecimal(trailer["F2"]);//40'
                decimal gp20_total = 0m; 
                if ("MCO".Equals(chg_cd))
                {
                    string sql = string.Format(@"SELECT COUNT(1) FROM (SELECT U_ID FROM SMRV WHERE SHIPMENT_ID={0}) A LEFT JOIN SMRVM B ON A.U_ID=B.U_FID WHERE (DATENAME(hour,B.MODIFY_DATE)>=21 OR DATENAME(hour,B.MODIFY_DATE)<3)", SQLUtils.QuotedStr(shipmentId));
                    decimal qty = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    decimal price = 0M;
                    if (F1 > 0) price = F1;
                    else if (F2 > 0) price = F2;
                    gp20_total = price * qty;
                    trailer["QCHG_UNIT"] = "SET";
                    trailer["QUNIT_PRICE"] = price;
                    trailer["QQTY"] = qty;
                    trailer["QAMT"] = Helper.Get45AmtValue(price * qty);
                    trailer["C_FLAG"] = "Y";

                    decimal temp1 = 0M;
                    trailer["QEX_RATE"] = Helper.GetTotal(rateDt, msg, gp20_total, cur, ref temp1, ref error, _localCur);
                    trailer["LOCALE_AMT"] = temp1;
                    total += temp1;
                    SetChargeInfo(trailer, "", tranMode);
                    ml.Add(CreateBillItem(trailer, _freight.CrPartyNo, _freight.CrPartyNm));
                }
                else
                {
                    string punit = Prolink.Math.GetValueAsString(trailer["PUNIT"]);
                    if (CalCNT(trailer, tranMode, ref error, msg, ml, null, _freight.CrPartyNo, _freight.CrPartyNm))
                        continue;
                    decimal F3 = Prolink.Math.GetValueAsDecimal(trailer["F3"]);//费用
                    decimal qty = 0;
                    if (tranMode.Equals("T"))//内贸 获取计费数量
                        qty = GetTruckQty(punit, _freight.Current_smsm);
                    else
                        qty = GetQty(punit); 
                    decimal cur_total = Helper.Get45AmtValue(qty * F3);
                    if ("%".Equals(punit))
                    {
                        cur_total = Helper.Get45AmtValue(qty * F3 * 0.01M);
                    } 
                    decimal f3_l = 0M; 
                    trailer["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref f3_l, ref error, _localCur);
                    if ("%".Equals(punit))
                    {
                        trailer["QEX_RATE"] = 1;
                        F3 = f3_l;
                        qty = 1;
                        cur_total = f3_l;
                        trailer["CUR"] = _localCur;
                    }
                    trailer["LOCALE_AMT"] = f3_l;
                    trailer["EX_REMARK"] = "";
                    SetChargeInfo(trailer, "", tranMode);

                    trailer["QCHG_UNIT"] = punit;
                    trailer["QUNIT_PRICE"] = F3;
                    trailer["QQTY"] = qty;
                    trailer["QAMT"] = cur_total;
                    trailer["C_FLAG"] = "Y";

                    ml.Add(CreateBillItem(trailer, _freight.CrPartyNo, _freight.CrPartyNm)); 
                }
            } 
            try
            {
                CalculateLocalAmt(ml, _freight.Current_smsm);
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("Shipment Id:" + _freight.ShipmentId + ",CalOutBoundTrailerFreight Error:" + ex.ToString());
            }
        }

        private decimal GetTruckQty(string punit, DataRow dr)
        {
            if (dr == null) return 0;  
            decimal qty = 0; 
            string carType = Prolink.Math.GetValueAsString(dr["CAR_TYPE"]);
            string carType1 = Prolink.Math.GetValueAsString(dr["CAR_TYPE1"]);
            string carType2 = Prolink.Math.GetValueAsString(dr["CAR_TYPE2"]);
            decimal carQty = Prolink.Math.GetValueAsDecimal(dr["CAR_QTY"]);
            decimal carQty1 = Prolink.Math.GetValueAsDecimal(dr["CAR_QTY1"]);
            decimal carQty2 = Prolink.Math.GetValueAsDecimal(dr["CAR_QTY2"]);

            #region 单位转换
            if (punit.Equals("20GP"))
                punit = "T20";
            else if (punit.Equals("40GP"))
                punit = "T12";
            #endregion

            #region 获取计费数量
            if (punit.Equals(carType))
            {
                return carQty;
            }
            else if (punit.Equals(carType1))
            {
                return carQty1;
            }
            else if (punit.Equals(carType2))
            {
                return carQty2;
            }
            #endregion
            return qty;
        }

        private bool CheckTrailerCCharge(string chg_cd)
        {
            string tranMode = _freight.TranType;
            bool result = false;
            switch (chg_cd)
            {
                case "MCO "://(夜間移櫃費,) 請到貨櫃管理,看一下該Shipment 下的貨櫃, 是否有移櫃紀錄,且是在晚上 21:00-03:00 之間, 每移一次就一個費用, 移3 次, 要 X 3 . 
                    result = true;
                    break;
            }
            return result;
        }


        private EditInstruct CreateCEditInstruct(DataRow local, string credit_to, string credit_nm, string amtField = "F3")
        {
            DataTable rateDt = _freight.RateDt;
            string tranMode = _freight.TranType;
            bool error = false;
            decimal amt = Prolink.Math.GetValueAsDecimal(local[amtField]);
            local["QCHG_UNIT"] = "SET";
            local["QUNIT_PRICE"] = amt;
            local["QQTY"] = 1;
            local["QAMT"] = amt;
            local["C_FLAG"] = "Y";
            decimal amt1 = 0M;
            local["QEX_RATE"] = Helper.GetTotal(rateDt, null, amt, Prolink.Math.GetValueAsString(local["CUR"]), ref amt1, ref error, _localCur);
            local["LOCALE_AMT"] = amt1;
            SetChargeInfo(local, "", tranMode);
            EditInstruct cei = CreateBillItem(local, credit_to, credit_nm);
            return cei;
        }
        private EditInstruct CalCYCNT(DataRow local, ref bool error, List<string> msg, MixedList ml, DataRow thc = null, bool isFreight = false, string credit_to = "", string credit_nm = "")
        {
            DataTable rateDt = _freight.RateDt;
            string tranMode = _freight.TranType;
            decimal cur_total = 0m;
            string cur = Prolink.Math.GetValueAsString(local["CUR"]);
            List<string> cntMsg = new List<string>();
            DataRow dr = local;
            Dictionary<string, string> cntParm = _cntParm;
            if (thc != null)
            {
                dr = thc;
                cntParm = _cntParm1;
                cur = Prolink.Math.GetValueAsString(thc["CUR"]);
                if (string.IsNullOrEmpty(cur))
                    cur = Prolink.Math.GetValueAsString(local["CUR"]);
                else
                    local["CUR"] = cur;
            }

            foreach (var kv in cntParm)
            {
                decimal price = Prolink.Math.GetValueAsDecimal(dr[kv.Value]);
                string punit = kv.Key;
                decimal qty = GetQty(punit, _cntParmList);
                if (qty <= 0 || price <= 0)
                    continue;
                cntMsg.Add(string.Format("{0}({1}*{2}{3})", kv.Key, qty, price, cur));
                cur_total += Helper.Get45AmtValue(qty * price);
            }

            if (cur_total > 0)
            {
                local["QCHG_UNIT"] = "SET";
                local["QUNIT_PRICE"] = cur_total;
                local["QQTY"] = 1;
                local["QAMT"] = cur_total;
                local["C_FLAG"] = "Y";
                decimal temp1 = 0M;
                local["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                local["LOCALE_AMT"] = temp1;
                SetChargeInfo(local, "", tranMode);
                EditInstruct cei = CreateBillItem(local, credit_to, credit_nm);
                return cei;
            }
            return null;
        }
        public decimal GetDocQty(string chg_cd, string punit, decimal qty)
        {
            string IsSplitBill = _freight.IsSplitBill;
            string shipmentId = _freight.ShipmentId;

            if ("C".Equals(IsSplitBill) &&"DOC".Equals(chg_cd) && "BL".Equals(punit))//DOC单证费 单位BL
            { 
                string sql = string.Format("SELECT COUNT(1) FROM SMSM WHERE SPLIT_SHIPMENT={0} AND STATUS NOT IN('V','Z')", SQLUtils.QuotedStr(shipmentId));
                int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                qty = count;
            }
            return qty;
        }

        private bool CheckLocalCCharge(string chg_cd)
        {
            string tranMode = _freight.TranType;
            string pod = _freight.Pod;
            string telex_rls = _freight.TelexRls;
            string region = _freight.Region;
            string horn = _freight.Horn;
            string battery = _freight.Battery;
            string is_land = _freight.IsLand;
            decimal DnNum = _freight.DnNum;
            string pod_rigion = string.Empty;
            if (!string.IsNullOrEmpty(pod) && pod.Length >= 2)
                pod_rigion = pod.Substring(0, 2);
            bool result = false;
            switch (chg_cd)
            {
                case "ENS"://1.	FCL/LCL/AIR shipment 的Rigion 為 EU 的才會有ENS 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if ("EU".Equals(region))
                                result = true;
                            break;
                    }
                    break;
                case "ACI"://2.	FCL/LCL/AIR shipment 的目的地國別為 CA 的才會有 ACI 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if ("CA".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "AMS"://3.	FCL/LCL/AIR  shipment 的目的地國別為 US 的才會有 AMS 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if ("US".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "AFR"://4.	FCL/LCL shipment 的目的地國別為 JP 的才會有 AFS 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                            if ("JP".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "WEC"://5.	FCL shipment 的目的地國別為 BR 的才會有 WEC 費用
                    switch (tranMode)
                    {
                        case "F":
                            if ("BR".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "TRC"://5.	FCL/LCL shipment的電放?=Y (Telex_RLS) 的才會有 TRC 的費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                            if ("Y".Equals(telex_rls))
                                result = true;
                            break;
                    }
                    break;
                case "CBF"://6.	FCL Shipment上有超過兩個DN 的就要收取CBF 費用
                    if ("F".Equals(tranMode) && DnNum > 2)
                    {
                        result = true;
                    }
                    break;
                case "DGMS"://危險品测磁费--喇叭 DGMS
                    if ("A".Equals(tranMode))
                    {
                        if ("Y".Equals(horn))
                            result = true;
                    }
                    break;
                case "DGMB"://危險品测磁费--电池 DGMB
                    if ("A".Equals(tranMode))
                    {
                        if ("Y".Equals(battery))
                            result = true;
                    }
                    break; 
                case "CTC"://9.	Air 判斷陸運? 如果該欄位=y , 要收取 CTC/SSU 費用.
                case "SSU ":
                    if ("A".Equals(tranMode))
                    {
                        if ("Y".Equals(is_land))
                            result = true;
                    }
                    break;
            }
            return result;
        }

        private static DataRow ChangeLocal(string sm_carrier, DataRow[] drs, DataRow local, string chg_cd, string quot_no, string sm_pod, string sm_incoterm)
        {
            int i = GetIndex(drs, quot_no, chg_cd, sm_pod, sm_carrier, sm_incoterm);
            if (i >= 0)
                return drs[i];
            return local;
        }

        private static int GetIndex(DataRow[] drs, string quot_no, string chg_cd, string sm_pod, string sm_carrier, string sm_incoterm)
        {
            int index = -1, carrierIndex = -1, incotermIndex = -1, podIndex = -1;
            for (int i = 0; i < drs.Length; i++)//满足POD
            {
                string chg_cd1 = Prolink.Math.GetValueAsString(drs[i]["CHG_CD"]);
                string quotNo = Prolink.Math.GetValueAsString(drs[i]["QUOT_NO"]);
                if (!chg_cd1.Equals(chg_cd) || !quot_no.Equals(quotNo))
                    continue;
                string carrier = Prolink.Math.GetValueAsString(drs[i]["CARRIER"]).Replace("&nbsp;", " ");
                string podCd = Prolink.Math.GetValueAsString(drs[i]["POD_CD"]);
                string incotermCd = Prolink.Math.GetValueAsString(drs[i]["INCOTERM_CD"]);
                if (carrier.Equals(sm_carrier) && podCd.Equals(sm_pod) && string.IsNullOrEmpty(incotermCd))
                    index = i;
                if (carrier.Equals(sm_carrier) && incotermCd.Equals(sm_incoterm) && string.IsNullOrEmpty(podCd) && index < 0)
                    index = i;
                if (carrier.Equals(sm_carrier) && string.IsNullOrEmpty(incotermCd) && string.IsNullOrEmpty(podCd))
                    carrierIndex = i;
                if (incotermCd.Equals(sm_incoterm) && string.IsNullOrEmpty(carrier) && string.IsNullOrEmpty(podCd))
                    incotermIndex = i;
                if (podCd.Equals(sm_pod) && string.IsNullOrEmpty(incotermCd) && string.IsNullOrEmpty(carrier))
                    podIndex = i;
            }
            return index >= 0 ? index : carrierIndex >= 0 ? carrierIndex : incotermIndex >= 0 ? incotermIndex : podIndex;
        }

        private static List<string> GetChargeType(List<string> d_to, List<string> types = null, List<string> charge_type = null)
        {
            if (charge_type == null)
                charge_type = new List<string>() { "无" };
            if (d_to == null)
                return charge_type;
            if (types == null)
                types = new List<string>() { LOCAL_CHARGE, DESTINATION_CHARGE, FREIGHT };

            foreach (string type in types)
            {
                if (!d_to.Contains(type))
                    continue;
                switch (type)
                {
                    case LOCAL_CHARGE:
                        if (!charge_type.Contains("O"))
                            charge_type.Add("O");
                        break;
                    case DESTINATION_CHARGE:
                        if (!charge_type.Contains("D"))
                            charge_type.Add("D");
                        break;
                    case FREIGHT:
                        if (!charge_type.Contains("F"))
                            charge_type.Add("F");
                        break;
                }
            }

            return charge_type;
        }


        #region 將拖卡車費用寫入帳單
        public void TerailerFeeCal(DataTable smrvdt, DataTable smqtdDt, DataTable smsmi, List<string> message)
        {
            List<string> msg = new List<string>();
            Dictionary<string, List<string>> ChgRemarkUnit = new Dictionary<string, List<string>>();
            Dictionary<string, string> chgCdDic = new Dictionary<string, string>();
            string shipmentId = Prolink.Math.GetValueAsString(smsmi.Rows[0]["SHIPMENT_ID"]);
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string CallType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CALL_TYPE"]);
            string SmsmiTranType = Prolink.Math.GetValueAsString(smsmi.Rows[0]["TRAN_TYPE"]);
            string QuotNo = Prolink.Math.GetValueAsString(smqtdDt.Rows[0]["QUOT_NO"]);
            string RfqNo = Prolink.Math.GetValueAsString(smqtdDt.Rows[0]["RFQ_NO"]);
            string Location = Prolink.Math.GetValueAsString(smsmi.Rows[0]["CMP"]);
            string carrier = Prolink.Math.GetValueAsString(smsmi.Rows[0]["CARRIER"]);
            string LspNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRUCKER"]);
            string RvStatus = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["STATUS"]);
            string CntrNo = string.Empty;
            MixedList mixList = new MixedList();
            string chgsql = string.Format("SELECT CHG_CD,CHG_DESCP FROM SMCHG WHERE CMP={0}", SQLUtils.QuotedStr(Location));
            DataTable chgDt = OperationUtils.GetDataTable(chgsql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in chgDt.Rows)
            {
                string chgcd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                string chgDescp = Prolink.Math.GetValueAsString(dr["CHG_DESCP"]);
                if (!chgCdDic.ContainsKey(chgcd))
                    chgCdDic.Add(chgcd, chgDescp);
            }
            if (CallType == "S")//一段運輸
            {
                ///内陆运输
                CalculateFeeByCallType(smrvdt, smqtdDt, ChgRemarkUnit, message, QuotNo, chgCdDic);
            }
            else if (CallType == "C")
            {
                /// 货柜叫车
                if (RvStatus != "V")
                {
                    CalculateDTRFFeeByContainer(smrvdt, smqtdDt, ChgRemarkUnit, carrier, message, QuotNo, chgCdDic);
                    /// Unloading Delay費用
                    CalculateUnloadingDelayFee(smrvdt, smqtdDt, ChgRemarkUnit, message, QuotNo, chgCdDic);
                }
                ////取消拖卡車費OCC
                if (RvStatus == "V")
                {
                    CalculateCancelCallTruckerFee(smrvdt, smqtdDt, ChgRemarkUnit, message, QuotNo, chgCdDic);
                }
            }
            else if (CallType == "D")
            {
                if (RvStatus != "V")
                {
                    ////by DN叫车计费
                    CalculateTruckerFeeByDN(smrvdt, smqtdDt, smsmi, ChgRemarkUnit, message, QuotNo, chgCdDic);
                    ///  ADPF 加点费
                    CalculateADPFFee(smrvdt, smqtdDt, ChgRemarkUnit, message, QuotNo, chgCdDic);
                    /// dn
                    CalculateUDFFeeByDn(smrvdt, smqtdDt, ChgRemarkUnit, message, QuotNo, chgCdDic);
                }
                if (RvStatus == "V")
                {
                    ////取消拖卡車費
                    CalculateCancelCallTruckerFeeByDN(smrvdt, smqtdDt, ChgRemarkUnit, message, QuotNo, chgCdDic);
                }
            }

            string sql = "SELECT CHG_TYPE,REPAY,SHIPMENT_ID,CHG_CD,QCUR,SUM(QAMT) AS QAMT FROM TMP_AMT WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentId) + " GROUP BY SHIPMENT_ID,CHG_CD,CHG_TYPE,REPAY,QCUR";
            if (SmsmiTranType.Equals("F") || SmsmiTranType.Equals("R"))
            {
                sql = "SELECT CHG_TYPE,REPAY,CNTR_NO,CHG_CD,QCUR,SUM(QAMT) AS QAMT FROM TMP_AMT WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentId) + " AND RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo) + " GROUP BY CNTR_NO,CHG_CD,CHG_TYPE,REPAY,QCUR";
            }
            DataTable tdt = CommonHelp.getDataTableFromSql(sql);
            if (tdt.Rows.Count > 0)
            {
                foreach (DataRow dr in tdt.Rows)
                {
                    string ChgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                    List<string> RemarkUnit = new List<string>();

                    string remarkchgcd = ChgCd;
                    if (!string.IsNullOrEmpty(ReserveNo))
                    {
                        remarkchgcd += "_" + ReserveNo;
                    }

                    if (ChgRemarkUnit.ContainsKey(remarkchgcd))
                        RemarkUnit = ChgRemarkUnit[remarkchgcd];
                    //string ChgDescp = CommonHelp.getOneValueAsStringFromSql("SELECT TOP 1 CHG_DESCP FROM SMCHG WHERE CHG_CD=" + SQLUtils.QuotedStr(ChgCd) + " AND CMP=" + SQLUtils.QuotedStr(Location));
                    string ChgDescp = "";
                    if (chgCdDic.ContainsKey(ChgCd))
                        ChgDescp = chgCdDic[ChgCd];
                    string Cur = Prolink.Math.GetValueAsString(dr["QCUR"]);
                    if (Cur == "")
                        Cur = CommonHelp.getOneValueAsStringFromSql("SELECT CUR FROM SMQTM WHERE QUOT_NO=" + SQLUtils.QuotedStr(QuotNo));
                    decimal TtlFee = Prolink.Math.GetValueAsDecimal(dr["QAMT"]);
                    if (SmsmiTranType.Equals("F") || SmsmiTranType.Equals("R"))
                        CntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                    TtlFee = System.Math.Round(TtlFee, 2);

                    bool IsOk = CalCualteFeeHandle.ChkSmbid(ChgCd, shipmentId, LspNo, CntrNo);
                    if (!IsOk)
                    {
                        msg.Add(shipmentId + "," + LspNo + "," + ChgCd + ":已开立账单");
                        message.Add(shipmentId + ":" + ChgDescp + "," + "It was a bill already");
                    }
                    //尚未開立帳單的才能寫入
                    if (IsOk == true)
                    {
                        string smbidUid = CalCualteFeeHandle.chkFeeExist(ChgCd, shipmentId, LspNo, Location, CntrNo);
                        msg.Add(ChgCd);
                        msg.Add(Prolink.Math.GetValueAsString(TtlFee));

                        if (smbidUid == "")
                        {
                            EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                            string uid = System.Guid.NewGuid().ToString();
                            //double QtaxAmt = FrtFee * (tax / 100);
                            //double Tqamt = FrtFee + QtaxAmt;
                            //Tqamt = CommonHelp.formatCur(Cur, Tqamt);
                            ei.Put("U_ID", uid);
                            ei.Put("SHIPMENT_ID", shipmentId);
                            ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(smsmi.Rows[0]["GROUP_ID"]));
                            ei.Put("CMP", Location);
                            ei.Put("SEC_CMP", Prolink.Math.GetValueAsString(smsmi.Rows[0]["SEC_CMP"]));
                            ei.Put("STN", Prolink.Math.GetValueAsString(smsmi.Rows[0]["STN"]));
                            //ei.Put("DEP", Prolink.Math.GetValueAsString(smsmi.Rows[0]["DEP"]));
                            ei.Put("QUOT_NO", QuotNo);
                            ei.Put("RFQ_NO", RfqNo);
                            ei.Put("QAMT", TtlFee);
                            //ei.Put("QTAX_AMT", 0);
                            //ei.Put("TQAMT", 0);
                            ei.Put("QTAX", 0);
                            ei.Put("CHG_CD", ChgCd);
                            ei.Put("CHG_DESCP", ChgDescp);
                            ei.Put("QCUR", Cur);
                            ei.Put("LSP_NO", LspNo);
                            ei.Put("LSP_NM", Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRUCKER_NM"]));
                            ei.Put("CNTR_NO", CntrNo);
                            if (!string.IsNullOrEmpty(CntrNo))
                                ei.Put("DEC_NO", CntrNo);
                            ei.Put("TRAN_TYPE", Prolink.Math.GetValueAsString(smsmi.Rows[0]["TRAN_TYPE"]));

                            ei.Put("CNTR_INFO", Prolink.Math.GetValueAsString(smsmi.Rows[0]["CNTR_INFO"]));
                            ei.Put("MASTER_NO", Prolink.Math.GetValueAsString(smsmi.Rows[0]["MASTER_NO"]));
                            ei.Put("BL_NO", Prolink.Math.GetValueAsString(smsmi.Rows[0]["MASTER_NO"]));
                            ei.Put("POD_CD", Prolink.Math.GetValueAsString(smsmi.Rows[0]["POD_CD"]));
                            SetCHGInfo(dr, ei, smsmi.Rows[0], RemarkUnit);
                            TPV.Financial.InboundBill.SetEstInfo(ei);
                            msg.Add("Add");
                            mixList.Add(ei);
                        }
                        else
                        {
                            EditInstruct ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                            string uid = System.Guid.NewGuid().ToString();
                            //double QtaxAmt = FrtFee * (tax / 100);
                            //double Tqamt = FrtFee + QtaxAmt;
                            ei.PutKey("U_ID", smbidUid);
                            ei.Put("QAMT", TtlFee);
                            ei.Put("QCUR", Cur);

                            UpdateSmbidQuotNo(ei, smbidUid, QuotNo, CntrNo);
                            //ei.Put("TQAMT", Tqamt);
                            msg.Add("update");
                            SetCHGInfo(dr, ei, smsmi.Rows[0]);
                            TPV.Financial.InboundBill.SetEstInfo(ei);
                            mixList.Add(ei);
                        }
                    }
                }

                if (mixList.Count > 0)
                {
                    try
                    {
                        CalculateLocalAmt(mixList, smsmi.Rows[0]);
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        msg.Add("success");
                    }
                    catch (Exception ex)
                    {
                        msg = new List<string>();
                        msg.Add(ex.ToString());
                    }
                }
                Bill.WriteLog(string.Join(";", msg), shipmentId);
            }
        }

        private void CalculateUDFFeeByDn(DataTable smrvdt, DataTable smqtdDt, Dictionary<string, List<string>> ChgRemarkUnit, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string Trucker = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRUCKER"]);
            string CarType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CAR_TYPE"]);
            string Cmp = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CMP"]);
            DateTime ArrivalFactDate = Prolink.Math.GetValueAsDateTime(smrvdt.Rows[0]["ARRIVAL_FACT_DATE"]);
            int hours = 0;
            string sql = "SELECT * FROM BSCODE WHERE CD_TYPE='FTU' AND CD={0} AND AP_CD={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(Trucker), SQLUtils.QuotedStr(Cmp));
            DataTable fdt = CommonHelp.getDataTableFromSql(sql);
            if (fdt.Rows.Count > 0)
            {
                foreach (DataRow fdr in fdt.Rows)
                {
                    string ArCd = Prolink.Math.GetValueAsString(fdr["AR_CD"]);
                    hours = Convert.ToInt16(ArCd);
                }
            }

            sql = "SELECT CBM, DN_NO, SHIPMENT_ID, PICK_AREA, DLV_AREA, POD_MDATE FROM SMRDN WHERE RESERVE_NO={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo));
            DataTable rdndt1 = CommonHelp.getDataTableFromSql(sql);
            sql = "SELECT SUM(CBM) AS CBM FROM SMRDN WHERE RESERVE_NO={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo));
            double occ_cbm1 = CommonHelp.getOneValueAsDoubleFromSql(sql);

            if (rdndt1.Rows.Count > 0)
            {
                foreach (DataRow rdndr in rdndt1.Rows)
                {
                    string thisShid = Prolink.Math.GetValueAsString(rdndr["SHIPMENT_ID"]);
                    string DnNo = Prolink.Math.GetValueAsString(rdndr["DN_NO"]);
                    decimal d_cbm = Prolink.Math.GetValueAsDecimal(rdndr["CBM"]);
                    DateTime PodMdate = Prolink.Math.GetValueAsDateTime(rdndr["POD_MDATE"]);
                    string PickArea = Prolink.Math.GetValueAsString(rdndr["PICK_AREA"]);
                    string DlvArea = Prolink.Math.GetValueAsString(rdndr["DLV_AREA"]);

                    TimeSpan ts = PodMdate - ArrivalFactDate;
                    int hour = ts.Hours;
                    hour = hour + 1;
                    if (hour >= hours)
                    {
                        string con2 = "CHG_CD='UDF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2}";
                        con2 = string.Format(con2, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                        DataRow[] occ = smqtdDt.Select(con2);
                        setEmptyRemark(message, quotNo, occ, "UDF", chgcdDic);
                        if (occ.Length > 0)
                        {
                            for (int i = 0; i < occ.Length; i++)
                            {
                                decimal Qamt = Prolink.Math.GetValueAsDecimal(occ[i][CarType]);
                                if (Qamt == 0)
                                {
                                    Qamt = Prolink.Math.GetValueAsDecimal(occ[i]["F70"]);
                                }
                                Qamt = Prolink.Math.GetValueAsDecimal(occ[i][CarType]) * (Convert.ToDecimal(hour) - Convert.ToDecimal(hours));
                                decimal amt = 0;
                                if (occ_cbm1 != 0)
                                    amt = System.Math.Round(Qamt * (d_cbm / Convert.ToDecimal(occ_cbm1)), 3);
                                SetFeeToTMP_AMT(thisShid, DnNo, ReserveNo, amt, ChgRemarkUnit, Qamt, occ[i]);
                            }
                        }
                    }
                }
            }
        }

        private void CalculateCancelCallTruckerFeeByDN(DataTable smrvdt, DataTable smqtdDt, Dictionary<string, List<string>> ChgRemarkUnit, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string Trucker = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRUCKER"]);
            string CarType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CAR_TYPE"]);
            string Cmp = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CMP"]);
            DateTime UseDate = Prolink.Math.GetValueAsDateTime(smrvdt.Rows[0]["USE_DATE"]);
            DateTime CancelDate = Prolink.Math.GetValueAsDateTime(smrvdt.Rows[0]["CANCEL_DATE"]);
            string RvStatus = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["STATUS"]);
            int days = 0;
            string sql = "SELECT * FROM BSCODE WHERE CD_TYPE='OCT' AND CD={0} AND AP_CD={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(Trucker), SQLUtils.QuotedStr(Cmp));
            DataTable fdt = CommonHelp.getDataTableFromSql(sql);
            if (fdt.Rows.Count > 0)
            {
                foreach (DataRow fdr in fdt.Rows)
                {
                    string ArCd = Prolink.Math.GetValueAsString(fdr["AR_CD"]);
                    days = Convert.ToInt16(ArCd);
                }
            }

            TimeSpan ts = CancelDate - UseDate;
            int Day = ts.Days;
            Day = Day + 1;
            if (RvStatus == "V" && Day >= days)
            {
                sql = "SELECT CBM, DN_NO, SHIPMENT_ID, PICK_AREA, DLV_AREA FROM SMRDN WHERE CANCEL_NO={0}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo));
                DataTable rdndt = CommonHelp.getDataTableFromSql(sql);

                sql = "SELECT SUM(CBM) AS CBM FROM SMRDN WHERE CANCEL_NO={0}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo));
                double occ_cbm = CommonHelp.getOneValueAsDoubleFromSql(sql);

                if (rdndt.Rows.Count > 0)
                {
                    foreach (DataRow rdndr in rdndt.Rows)
                    {
                        string thisShid = Prolink.Math.GetValueAsString(rdndr["SHIPMENT_ID"]);
                        string DnNo = Prolink.Math.GetValueAsString(rdndr["DN_NO"]);
                        decimal d_cbm = Prolink.Math.GetValueAsDecimal(rdndr["CBM"]);
                        string PickArea = Prolink.Math.GetValueAsString(rdndr["PICK_AREA"]);
                        string DlvArea = Prolink.Math.GetValueAsString(rdndr["DLV_AREA"]);

                        string con2 = "CHG_CD='OCC' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2}";
                        con2 = string.Format(con2, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                        DataRow[] occ = smqtdDt.Select(con2);
                        setEmptyRemark(message, quotNo, occ, "OCC", chgcdDic);
                        if (occ.Length > 0)
                        {
                            for (int i = 0; i < occ.Length; i++)
                            {
                                decimal Qamt = Prolink.Math.GetValueAsDecimal(occ[i][CarType]) * Convert.ToDecimal(Day);
                                string Cur = Prolink.Math.GetValueAsString(occ[i]["CUR"]);
                                string ChgCd = Prolink.Math.GetValueAsString(occ[i]["CHG_CD"]);
                                string Punit = Prolink.Math.GetValueAsString(occ[i]["PUNIT"]);
                                string remark = Prolink.Math.GetValueAsString(occ[i]["REMARK"]);
                                decimal amt = 0;
                                amt = System.Math.Round(Qamt * (d_cbm / Convert.ToDecimal(occ_cbm)), 3);
                                SetFeeToTMP_AMT(thisShid, DnNo, ReserveNo, amt, ChgRemarkUnit, Qamt, occ[i]);
                            }
                        }
                    }
                }
            }
        }

        private void CalculateTruckerFeeByDN(DataTable smrvdt, DataTable smqtdDt, DataTable smsmi, Dictionary<string, List<string>> ChgRemarkUnit, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string TrsMode = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRS_MODE"]);
            string CarType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CAR_TYPE"]);
            string ShipmentInfo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["SHIPMENT_INFO"]);
            string LspNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRUCKER"]);
            string sql = "SELECT PICK_AREA, DLV_AREA FROM SMRDN WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo) + " GROUP BY PICK_AREA, DLV_AREA";
            DataTable dt = CommonHelp.getDataTableFromSql(sql);

            //smsmi Table
            string Location = Prolink.Math.GetValueAsString(smsmi.Rows[0]["CMP"]);
            string tranType = Prolink.Math.GetValueAsString(smsmi.Rows[0]["TRAN_TYPE"]);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string PickArea = Prolink.Math.GetValueAsString(dr["PICK_AREA"]);
                    string DlvArea = Prolink.Math.GetValueAsString(dr["DLV_AREA"]);
                    #region 卡車費
                    string con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND (BACK_LOCATION='' OR BACK_LOCATION IS NULL)";
                    con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                    DataRow[] qdr = smqtdDt.Select(con);
                    setEmptyRemark(message, quotNo, qdr, "DTRF", chgcdDic);
                    if (qdr.Length > 0)
                    {
                        for (int i = 0; i < qdr.Length; i++)
                        {
                            decimal Qamt = 0;
                            string Cur = string.Empty;
                            string ChgCd = string.Empty;
                            Cur = Prolink.Math.GetValueAsString(qdr[i]["CUR"]);
                            ChgCd = Prolink.Math.GetValueAsString(qdr[i]["CHG_CD"]);
                            string Punit = Prolink.Math.GetValueAsString(qdr[i]["PUNIT"]);
                            string remark = Prolink.Math.GetValueAsString(qdr[i]["REMARK"]);
                            decimal MinAmt = Prolink.Math.GetValueAsDecimal(qdr[0]["MIN_AMT"]);
                            if (TrsMode == "Y")
                            {
                                Qamt = Prolink.Math.GetValueAsDecimal(qdr[i][CarType]);
                            }
                            else
                            {
                                decimal TtlFee = 0, Fee = 0;
                                #region 零擔計價
                                sql = "SELECT * FROM ECREFFEE WHERE FEE_TYPE='V' AND CMP={0} AND VENDER_CD={1} ORDER BY SEQ_NO";
                                sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(LspNo));
                                DataTable edt = CommonHelp.getDataTableFromSql(sql);

                                if (edt.Rows.Count > 0)
                                {
                                    #region 重量區間計算
                                    Punit = Prolink.Math.GetValueAsString(edt.Rows[0]["PUNIT"]);
                                    double w = GetWeightByUnit(Punit, ReserveNo, ShipmentInfo, tranType);
                                    string s = "FEE_FROM <= {0} and FEE_TO >= {0}";
                                    s = string.Format(s, w);
                                    DataRow[] edr = edt.Select(s);

                                    for (int j = 0; j < edr.Length; j++)
                                    {
                                        string CalType = Prolink.Math.GetValueAsString(edr[j]["CAL_TYPE"]);
                                        string FeeChgCd = Prolink.Math.GetValueAsString(edr[j]["CHG_CD"]);
                                        double FeeFrom = Prolink.Math.GetValueAsDouble(edr[j]["FEE_FROM"]);
                                        TtlFee = Prolink.Math.GetValueAsDecimal(qdr[j][FeeChgCd]);

                                        if (CalType == "C")
                                        {
                                            double n = w - FeeFrom + 1;
                                            TtlFee = TtlFee * Convert.ToDecimal(n);
                                        }
                                    }

                                    Qamt = TtlFee;
                                    #endregion
                                }
                                else
                                {
                                    sql = "SELECT * FROM ECREFFEE WHERE FEE_TYPE='D' AND CMP={0} AND VENDER_CD={1} ORDER BY SEQ_NO";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(LspNo));
                                    edt = CommonHelp.getDataTableFromSql(sql);
                                    if (edt.Rows.Count > 0)
                                    {
                                        #region 快遞區間計價
                                        Punit = Prolink.Math.GetValueAsString(edt.Rows[0]["PUNIT"]);
                                        double w = GetWeightByUnit(Punit,ReserveNo,ShipmentInfo, tranType);

                                        string _chgcd = string.Empty;
                                        int SeqNo = 0;
                                        double nextweight = 0;
                                        foreach (DataRow edr in edt.Rows)
                                        {
                                            _chgcd = Prolink.Math.GetValueAsString(edr["CHG_CD"]);
                                            string FeeOp = Prolink.Math.GetValueAsString(edr["FEE_OP"]);
                                            double FeeWeight = Prolink.Math.GetValueAsDouble(edr["FEE_WEIGHT"]);
                                            string CalType = Prolink.Math.GetValueAsString(edr["CAL_TYPE"]);

                                            if (SeqNo == 0)
                                            {
                                                if (FeeOp == "1")
                                                {
                                                    if (w > FeeWeight)
                                                    {
                                                        nextweight = w - FeeWeight + 1;
                                                    }
                                                }

                                                if (FeeOp == "2")
                                                {
                                                    if (w > FeeWeight)
                                                    {
                                                        nextweight = w - FeeWeight;
                                                    }
                                                }

                                                Fee = Prolink.Math.GetValueAsDecimal(qdr[i][_chgcd]);
                                                if (CalType == "C")
                                                {
                                                    TtlFee = Fee * Convert.ToDecimal(FeeWeight);
                                                }
                                                else
                                                {
                                                    TtlFee = Fee;
                                                }

                                            }
                                            else
                                            {
                                                Fee = Prolink.Math.GetValueAsDecimal(qdr[i][_chgcd]);
                                                if (CalType == "C")
                                                {
                                                    TtlFee = TtlFee + (Fee * Convert.ToDecimal(nextweight));
                                                }
                                                else
                                                {
                                                    //TtlFee = TtlFee + Fee;
                                                    if (w > FeeWeight)
                                                    {
                                                        TtlFee = Fee;
                                                    }
                                                }
                                            }

                                            SeqNo++;

                                        }
                                        Qamt = TtlFee;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 一般零擔計價
                                        decimal FrtFee1 = Prolink.Math.GetValueAsDecimal(qdr[i]["F1"]);
                                        decimal FrtFee2 = Prolink.Math.GetValueAsDecimal(qdr[i]["F2"]);
                                        decimal FrtFee3 = Prolink.Math.GetValueAsDecimal(qdr[i]["F3"]);
                                        decimal gw = 0;
                                        sql = "SELECT SUM(GW) AS GW, GWU, PICK_AREA, DLV_AREA FROM SMRDN WHERE RESERVE_NO={0} AND PICK_AREA={1} AND DLV_AREA={2} GROUP BY PICK_AREA, DLV_AREA, GWU";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea));
                                        DataTable wdt = CommonHelp.getDataTableFromSql(sql);
                                        if (wdt.Rows.Count > 0)
                                        {
                                            foreach (DataRow wr in wdt.Rows)
                                            {
                                                string gwu = wr["GWU"].ToString();
                                                decimal w1 = Decimal.Parse(wr["GW"].ToString());
                                                if (gwu == "G" || gwu == "g")
                                                {
                                                    w1 = w1 / 1000;
                                                }
                                                gw = gw + w1;
                                            }
                                        }

                                        sql = "SELECT SUM(CBM) AS CBM, PICK_AREA, DLV_AREA FROM SMRDN WHERE RESERVE_NO={0} AND PICK_AREA={1} AND DLV_AREA={2} GROUP BY PICK_AREA, DLV_AREA";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea));
                                        DataTable wdt1 = CommonHelp.getDataTableFromSql(sql);
                                        decimal cbm = Prolink.Math.GetValueAsDecimal(wdt1.Rows[0]["CBM"]);

                                        sql = "SELECT SUM(VW) AS VW FROM SMSMI WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(ShipmentInfo.Split(','));
                                        decimal vw = Convert.ToDecimal(CommonHelp.getOneValueAsDoubleFromSql(sql));

                                        decimal F1Amt = FrtFee1 * vw;
                                        decimal F2Amt = FrtFee2 * gw;
                                        decimal F3Amt = FrtFee3 * cbm;

                                        decimal[] arry = { F1Amt, F2Amt, F3Amt };
                                        for (int j = 0; j < arry.Length; j++)
                                        {
                                            if (TtlFee < arry[j])
                                            {
                                                TtlFee = arry[j];
                                            }
                                        }

                                        Qamt = TtlFee;
                                        #endregion
                                    }
                                }
                                #endregion
                            }

                            if (MinAmt > Qamt)
                            {
                                Qamt = MinAmt;
                            }

                            sql = "SELECT CBM, DN_NO, SHIPMENT_ID FROM SMRDN WHERE RESERVE_NO={0} AND PICK_AREA={1} AND DLV_AREA={2}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea));
                            DataTable sumdt = CommonHelp.getDataTableFromSql(sql);

                            if (sumdt.Rows.Count > 0)
                            {
                                DataView dataView = sumdt.DefaultView;
                                DataTable shipmentDistinct = dataView.ToTable(true, "SHIPMENT_ID");
                                bool iscombine = false;
                                string combineremark = "";
                                if (shipmentDistinct.Rows.Count > 1)
                                {
                                    iscombine = true;
                                    List<string> shipmetidlist = new List<string>();
                                    foreach (DataRow indexdr in shipmentDistinct.Rows)
                                    {
                                        shipmetidlist.Add(indexdr["SHIPMENT_ID"].ToString());
                                    }
                                    combineremark = string.Join(";", shipmetidlist);
                                }

                                List<string> shipmentlist = new List<string>();
                                List<string> dnlist = new List<string>();
                                foreach (DataRow sumdr in sumdt.Rows)
                                {
                                    string thisShid = Prolink.Math.GetValueAsString(sumdr["SHIPMENT_ID"]);
                                    string DnNo = Prolink.Math.GetValueAsString(sumdr["DN_NO"]);
                                    decimal d_cbm = Prolink.Math.GetValueAsDecimal(sumdr["CBM"]);
                                    decimal amt = 0;
                                    sql = "SELECT SUM(CBM) AS CBM FROM SMRDN WHERE PICK_AREA={0} AND DLV_AREA={1} AND RESERVE_NO={2}";
                                    sql = string.Format(sql, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(ReserveNo));
                                    double cbm = CommonHelp.getOneValueAsDoubleFromSql(sql);

                                    if (cbm > 0)
                                    {
                                        amt = System.Math.Round(Qamt * (d_cbm / Convert.ToDecimal(cbm)), 3);
                                    }
                                    if (iscombine)
                                    {
                                        qdr[i]["REMARK"] = combineremark + "total Fee:" + Qamt;
                                    }
                                    SetFeeToTMP_AMT(thisShid, DnNo, ReserveNo, amt, ChgRemarkUnit, Qamt, qdr[i]);
                                    if (!shipmentlist.Contains(thisShid))
                                    {
                                        shipmentlist.Add(thisShid);
                                    }
                                    if (!dnlist.Contains(DnNo))
                                    {
                                        dnlist.Add(DnNo);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        private void CalculateADPFFee(DataTable smrvdt, DataTable smqtdDt, Dictionary<string, List<string>> ChgRemarkUnit, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string sql = "SELECT * FROM SMRDN WHERE ADD_POINT='Y' AND RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = CommonHelp.getDataTableFromSql(sql);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string PickArea = Prolink.Math.GetValueAsString(dr["PICK_AREA"]);
                    string DlvArea = Prolink.Math.GetValueAsString(dr["DLV_AREA"]);
                    string DnNo = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                    string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);

                    string con = "CHG_CD='ADPF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2}";
                    con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                    DataRow[] qdr = smqtdDt.Select(con);
                    setEmptyRemark(message, quotNo, qdr, "ADPF", chgcdDic);
                    if (qdr.Length > 0)
                    {
                        for (int i = 0; i < qdr.Length; i++)
                        {
                            decimal Qamt = Prolink.Math.GetValueAsDecimal(qdr[i]["F70"]);
                            SetFeeToTMP_AMT(ShipmentId, DnNo, ReserveNo, Qamt, ChgRemarkUnit, Qamt, qdr[i]);
                        }
                    }
                }
            }
        }

        private void CalculateUnloadingDelayFee(DataTable smrvdt, DataTable smqtdDt, Dictionary<string, List<string>> ChgRemarkUnit, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            int hours = 0;
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string CarType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CAR_TYPE"]);
            string Trucker = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRUCKER"]);
            string Cmp = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CMP"]);
            DateTime ArrivalFactDate = Prolink.Math.GetValueAsDateTime(smrvdt.Rows[0]["ARRIVAL_FACT_DATE"]);

            string sql = "SELECT * FROM BSCODE WHERE CD_TYPE='FTU' AND CD={0} AND AP_CD={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(Trucker), SQLUtils.QuotedStr(Cmp));
            DataTable fdt = CommonHelp.getDataTableFromSql(sql);
            if (fdt.Rows.Count > 0)
            {
                foreach (DataRow fdr in fdt.Rows)
                {
                    string ArCd = Prolink.Math.GetValueAsString(fdr["AR_CD"]);
                    hours = Convert.ToInt16(ArCd);
                }
            }

            sql = "SELECT CNTR_NO, SHIPMENT_ID, PICK_AREA, DLV_AREA, POD_MDATE FROM SMRCNTR WHERE RESERVE_NO={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ReserveNo));
            DataTable cntsmrvdt = CommonHelp.getDataTableFromSql(sql);

            if (cntsmrvdt.Rows.Count <= 0) return;

            foreach (DataRow cntrdr in cntsmrvdt.Rows)
            {
                string thisShid = Prolink.Math.GetValueAsString(cntrdr["SHIPMENT_ID"]);
                string CntrNo = Prolink.Math.GetValueAsString(cntrdr["CNTR_NO"]);
                DateTime PodMdate = Prolink.Math.GetValueAsDateTime(cntrdr["POD_MDATE"]);
                string PickArea = Prolink.Math.GetValueAsString(cntrdr["PICK_AREA"]);
                string DlvArea = Prolink.Math.GetValueAsString(cntrdr["DLV_AREA"]);

                TimeSpan ts = PodMdate - ArrivalFactDate;
                int hour = ts.Hours;
                hour = hour + 1;
                if (hour >= hours)
                {
                    string con2 = "CHG_CD='UDF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2}";
                    con2 = string.Format(con2, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                    DataRow[] occ = smqtdDt.Select(con2);
                    setEmptyRemark(message, quotNo, occ, "UDF", chgcdDic);
                    if (occ.Length > 0)
                    {
                        for (int i = 0; i < occ.Length; i++)
                        {
                            decimal Qamt = Prolink.Math.GetValueAsDecimal(occ[i][CarType]);
                            if (Qamt == 0)
                            {
                                Qamt = Prolink.Math.GetValueAsDecimal(occ[i]["F70"]);
                            }
                            Qamt = Qamt * (Convert.ToDecimal(hour) - Convert.ToDecimal(hours));
                            decimal amt = Qamt;
                            SetFeeToTMP_AMT(thisShid, CntrNo, ReserveNo, amt, ChgRemarkUnit, Qamt, occ[i]);
                        }
                    }
                }
            }
        }

        private void CalculateCancelCallTruckerFee(DataTable smrvdt, DataTable smqtdDt, Dictionary<string, List<string>> ChgRemarkUnit, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string CarType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CAR_TYPE"]);
            string ShipmentInfo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["SHIPMENT_INFO"]);
            string sql = "SELECT * FROM SMRCNTR WHERE CANCEL_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable cancel_dt = CommonHelp.getDataTableFromSql(sql);
            if (cancel_dt.Rows.Count <= 0) return;
            foreach (DataRow cancel_dr in cancel_dt.Rows)
            {
                string PickArea = Prolink.Math.GetValueAsString(cancel_dr["PICK_AREA"]);
                string DlvArea = Prolink.Math.GetValueAsString(cancel_dr["DLV_AREA"]);
                string CntrNo = Prolink.Math.GetValueAsString(cancel_dr["CNTR_NO"]);
                string ShipmentId = Prolink.Math.GetValueAsString(cancel_dr["SHIPMENT_ID"]);

                string c_con = "CHG_CD='OCC' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2}";
                c_con = string.Format(c_con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                DataRow[] occ = smqtdDt.Select(c_con);
                setEmptyRemark(message, quotNo, occ, "OCC", chgcdDic);
                if (occ.Length > 0)
                {
                    for (int i = 0; i < occ.Length; i++)
                    {
                        decimal Qamt = Prolink.Math.GetValueAsDecimal(occ[i][CarType]);
                        if (Qamt == 0)
                        {
                            Qamt = Prolink.Math.GetValueAsDecimal(occ[i]["F70"]);
                        }
                        SetFeeToTMP_AMT(ShipmentInfo, CntrNo, ReserveNo, Qamt, ChgRemarkUnit, Qamt, occ[i]);
                    }
                }
            }
        }

        private string CalculateDTRFFeeByContainer(DataTable smrvdt, DataTable smqtdDt, Dictionary<string, List<string>> ChgRemarkUnit, string carrier, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string CarType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CAR_TYPE"]);
            string BackLocation = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["BACK_LOCATION"]);
            string ShipmentInfo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["SHIPMENT_INFO"]);
            string sql = "SELECT * FROM SMRCNTR WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = CommonHelp.getDataTableFromSql(sql);
            if (dt.Rows.Count <= 0) return "";
            foreach (DataRow dr in dt.Rows)
            {
                string PickArea = Prolink.Math.GetValueAsString(dr["PICK_AREA"]);
                string DlvArea = Prolink.Math.GetValueAsString(dr["DLV_AREA"]);
                string CntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);

                #region 卡車費
                string con = "CHG_CD='DTRF'";
                DataRow[] qdr = smqtdDt.Select(con);
                setEmptyRemark(message, quotNo, qdr, "DTRF", chgcdDic);
                if (qdr.Length == 0)
                    return "";
                if (PickArea != "" && DlvArea != "" && BackLocation != "" && carrier != "")
                {
                    con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND BACK_LOCATION={2} AND TRAN_TYPE={3} AND CARRIER_CODE={4}";
                    con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(BackLocation), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(carrier));
                    qdr = smqtdDt.Select(con);
                    if (qdr.Length == 0)
                    {
                        con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND BACK_LOCATION is null AND TRAN_TYPE={2} AND CARRIER_CODE={3}";
                        con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(carrier));
                        qdr = smqtdDt.Select(con);
                        if (qdr.Length == 0)
                        {
                            con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION={3} AND CARRIER_CODE is null";
                            con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(BackLocation));
                            qdr = smqtdDt.Select(con);
                            if (qdr.Length == 0)
                            {
                                con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION is null AND CARRIER_CODE is null";
                                con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                            }
                        }
                    }
                }
                else if (PickArea != "" && DlvArea != "" && BackLocation == "" && carrier != "")
                {
                    con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION is null AND CARRIER_CODE={3}";
                    con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(carrier));
                    qdr = smqtdDt.Select(con);
                    if (qdr.Length == 0)
                    {
                        con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION={3} AND CARRIER_CODE is null";
                        con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(BackLocation));
                        qdr = smqtdDt.Select(con);
                        if (qdr.Length == 0)
                        {
                            con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION is null AND CARRIER_CODE is null";
                            con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                        }
                    }
                }
                else if (PickArea != "" && DlvArea != "" && BackLocation != "" && carrier == "")
                {
                    con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION={3} AND CARRIER_CODE is null";
                    con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(BackLocation));
                    qdr = smqtdDt.Select(con);
                    if (qdr.Length == 0)
                    {
                        con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION is null AND CARRIER_CODE is null";
                        con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                    }
                }
                else if (PickArea != "" && DlvArea != "" && BackLocation == "" && carrier == "")
                {
                    con = "CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2} AND BACK_LOCATION is null AND CARRIER_CODE is null";
                    con = string.Format(con, SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
                }

                qdr = smqtdDt.Select(con);
                setEmptyRemark(message, quotNo, qdr, "DTRF", chgcdDic);
                if (qdr.Length > 0)
                {
                    for (int i = 0; i < qdr.Length; i++)
                    {
                        decimal Qamt = 0M;
                        try
                        {
                            Qamt = Prolink.Math.GetValueAsDecimal(qdr[i][CarType]);
                        }
                        catch (Exception e) { }
                        decimal MinAmt = Prolink.Math.GetValueAsDecimal(qdr[i]["MIN_AMT"]);

                        if (MinAmt > Qamt)
                        {
                            Qamt = MinAmt;
                        }

                        if (Qamt == 0)
                        {
                            Qamt = Prolink.Math.GetValueAsDecimal(qdr[i]["F70"]);
                        }
                        SetFeeToTMP_AMT(ShipmentInfo, CntrNo, ReserveNo, Qamt, ChgRemarkUnit, Qamt, qdr[i]);
                    }
                }
                #endregion
            }
            return "";
        }

        private void CalculateFeeByCallType(DataTable smrvdt, DataTable smqtdDt, Dictionary<string, List<string>> ChgRemarkUnit, List<string> message, string quotNo, Dictionary<string, string> chgcdDic)
        {
            string PickArea = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["PICK_AREA"]);
            string DlvArea = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["DLV_AREA"]);
            string ShipmentInfo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["SHIPMENT_ID"]);
            string CntrNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CNTR_NO"]);
            string ReserveNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["RESERVE_NO"]);
            string TranType = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["TRAN_TYPE"]);
            string con = string.Format("CHG_CD='DTRF' AND POL_CD={0} AND POD_CD={1} AND TRAN_TYPE={2}",
                SQLUtils.QuotedStr(PickArea), SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(TranType));
            DataRow[] qdr = smqtdDt.Select(con);

            setEmptyRemark(message, quotNo, qdr, "DTRF", chgcdDic);

            if (qdr.Length > 0)
            {
                decimal TtlFee = 0;
                string sql = "SELECT * FROM SMRCNTR WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                DataTable smrcntrDt = CommonHelp.getDataTableFromSql(sql);
                decimal Qamt = 0;

                if (smrcntrDt.Rows.Count > 0)
                {
                    CntrNo = Prolink.Math.GetValueAsString(smrcntrDt.Rows[0]["CNTR_NO"]);
                    foreach (DataRow item in smrcntrDt.Rows)
                    {
                        string chg_cd = "F70";
                        string CntrType = Prolink.Math.GetValueAsString(item["CNTR_TYPE"]);
                        Qamt = 0;

                        if (CntrType == "20GP")
                        {
                            chg_cd = "F4";
                        }
                        else if (CntrType == "40GP")
                        {
                            chg_cd = "F5";
                        }
                        else if (CntrType == "40HQ")
                        {
                            chg_cd = "F6";
                        }

                        Qamt = Prolink.Math.GetValueAsDecimal(qdr[0][chg_cd]);
                        if (Qamt == 0)
                        {
                            Qamt = Prolink.Math.GetValueAsDecimal(qdr[0]["F70"]);
                        }
                        TtlFee = TtlFee + Qamt;
                    }
                }
                decimal MinAmt = Prolink.Math.GetValueAsDecimal(qdr[0]["MIN_AMT"]);
                if (ShipmentInfo == "")
                {
                    ShipmentInfo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["SHIPMENT_ID"]);
                }

                if (MinAmt > TtlFee)
                {
                    TtlFee = MinAmt;
                }
                SetFeeToTMP_AMT(ShipmentInfo, CntrNo, ReserveNo, TtlFee, ChgRemarkUnit, Qamt, qdr[0]);
            }
        }

        public void SetFeeToTMP_AMT(string ShipmentInfo, string CntrNo, string ReserveNo, decimal ttlfee, Dictionary<string, List<string>> ChgRemarkUnit, decimal Qamt, DataRow Chargedr)
        {
            string ChgCd = Prolink.Math.GetValueAsString(Chargedr["CHG_CD"]);
            string Cur = Prolink.Math.GetValueAsString(Chargedr["CUR"]);
            string Punit = Prolink.Math.GetValueAsString(Chargedr["PUNIT"]);
            string remark = Prolink.Math.GetValueAsString(Chargedr["REMARK"]);
            bool ex = ChkTmpExist(ShipmentInfo, CntrNo, ReserveNo, ChgCd);
            EditInstruct ei = null;
            if (ex == false)
            {
                ei = new EditInstruct("TMP_AMT", EditInstruct.INSERT_OPERATION);
                ei.Put("SHIPMENT_ID", ShipmentInfo);
                ei.Put("RESERVE_NO", ReserveNo);
                ei.Put("CNTR_NO", CntrNo);
                ei.Put("CHG_CD", ChgCd);
                ei.Put("QCUR", Cur);
                ei.Put("QAMT", ttlfee);
            }
            else
            {
                ei = new EditInstruct("TMP_AMT", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", ShipmentInfo);
                ei.PutKey("RESERVE_NO", ReserveNo);
                ei.PutKey("CNTR_NO", CntrNo);
                ei.PutKey("CHG_CD", ChgCd);
                ei.Put("QAMT", ttlfee);
                ei.Put("QCUR", Cur);
            }
            if (!string.IsNullOrEmpty(ReserveNo))
            {
                ChgCd += "_" + ReserveNo;
            }
            setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, Qamt);
            SetCHGInfo(Chargedr, ei);
            OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public void setEmptyRemark(List<string> message, string quotNo, DataRow[] qdrs, string chgCd, Dictionary<string, string> chgcdDic, string msg = "")
        {
            string chgDescp = chgCd;
            if (chgcdDic.ContainsKey(chgCd))
            {
                chgDescp = chgcdDic[chgCd];
                if (!string.IsNullOrEmpty(msg))
                    message.Add(msg);
                if (qdrs != null && qdrs.Length == 0)
                {
                    message.Add(quotNo + ":can'nt find " + chgDescp);
                }
            }
        }

        private double GetWeightByUnit(string punit,string reserveNo,string ShipmentInfo,string tranType)
        {
            double weight = 0;
            string sql = "";
            switch (punit)
            {
                case "KGS":
                    sql = "SELECT SUM(GW) AS GW, GWU FROM SMRDN WHERE RESERVE_NO={0} GROUP BY GWU";
                    DataTable dt1 = CommonHelp.getDataTableFromSql(string.Format(sql, SQLUtils.QuotedStr(reserveNo)));
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow dr1 in dt1.Rows)
                        {
                            string gwu = dr1["GWU"].ToString();
                            double w1 = Prolink.Math.GetValueAsDouble(dr1["GW"].ToString());
                            if (gwu == "G" || gwu == "g")
                            {
                                w1 = w1 / 1000;
                            }
                            weight = weight + w1;
                        }
                    }
                    break;
                case "CBM":
                    sql = "SELECT SUM(CBM) AS CBM FROM SMRDN WHERE RESERVE_NO={0}";
                    weight = CommonHelp.getOneValueAsDoubleFromSql(string.Format(sql, SQLUtils.QuotedStr(reserveNo)));
                    break;
                case "PLT":
                    sql = "SELECT SUM(PKG_NUM) AS PKG_NUM FROM SMSMI WHERE SHIPMENT_ID IN {0} AND PKG_UNIT='PLT'";
                    weight = CommonHelp.getOneValueAsDoubleFromSql(string.Format(sql, SQLUtils.Quoted(ShipmentInfo.Split(','))));
                    break;
                case "CW":
                    sql = "SELECT SUM(CW) AS CW FROM SMSMI WHERE SHIPMENT_ID IN {0}";
                    weight = CommonHelp.getOneValueAsDoubleFromSql(string.Format(sql, SQLUtils.Quoted(ShipmentInfo.Split(','))));
                    weight = "A".Equals(tranType)? Math.Round(weight, MidpointRounding.AwayFromZero): weight;
                    break;
            }
            return weight;
        }
        #endregion

        #region 將報關費寫入帳單
        public static void BrokerFee2Smbid(DataTable smsmi, DataTable fdt, List<string> Message, int Accf = 0, bool Tc = false)
        {
            
            string sql = string.Empty;
            string ShipmentId = Prolink.Math.GetValueAsString(smsmi.Rows[0]["SHIPMENT_ID"]);
            Func<decimal,string,string, DataRow, bool> ChgCdCheck = (TtlCntNum, ChgCd, ChgDescp, cdr) =>
            {
                string Inspection = Prolink.Math.GetValueAsString(cdr["INSPECTION"]);
                string Icdf = Prolink.Math.GetValueAsString(cdr["ICDF"]);
                bool check = true;
                switch (ChgCd)
                {
                    case "CISC":
                        if (Inspection != "Y")
                        {
                            Message.Add(ChgDescp + ":" + ShipmentId + " CC-Faster Solution is 'no'");
                            check = false;
                        }
                        break;
                    case "ICDF":
                        if (Icdf != "Y")
                        {
                            Message.Add(ChgDescp + ":" + ShipmentId + " CC-Faster Solution is 'no'");
                            check = false;
                        }
                        break;
                    case "ACCF":
                        if (TtlCntNum == 0)
                        {
                            Message.Add(ChgDescp + ":" + ShipmentId + " container quantity 0");
                            check = false;
                        }
                        break;
                }
                return check;
            };
            try
            {
                Dictionary<string, decimal> ChgAmt = new Dictionary<string, decimal>();
                Dictionary<string, List<string>> ChgRemarkUnit = new Dictionary<string, List<string>>();
                Dictionary<string, decimal> decCount = new Dictionary<string, decimal>();
                List<string> DecCntrDn = new List<string>();
                Dictionary<string, decimal> shipmentDecCount = new Dictionary<string, decimal>();
                List<string> shipmentList = new List<string>();
                DataTable allSmDt = new DataTable();
                string TranType = Prolink.Math.GetValueAsString(smsmi.Rows[0]["TRAN_TYPE"]);
                decimal Cnt20 = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["CNT20"]);
                decimal Cnt40 = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["CNT40"]);
                decimal Cnt40hq = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["CNT40HQ"]);
                decimal Cbm = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["CBM"]);
                decimal Gw = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["GW"]);
                decimal Vw = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["VW"]);
                string ExtraSrv = Prolink.Math.GetValueAsString(smsmi.Rows[0]["EXTRA_SRV"]);
                string country = Prolink.Math.GetValueAsString(smsmi.Rows[0]["COUNTRY"]);
                string decDateInfo = Prolink.Math.GetValueAsString(smsmi.Rows[0]["DEC_DATE_INFO"]);
                string productInfo = Prolink.Math.GetValueAsString(smsmi.Rows[0]["PRODUCT_INFO"]);
                string[] productTypes = productInfo.Split(',');
                string product_type = "";
                if (productTypes.Length > 0)
                    product_type = productTypes[0];
                string[] decDates = decDateInfo.Split(',');
                DateTime smDecDate = DateTime.MinValue;
                if (decDateInfo.Length > 0)
                    smDecDate = Prolink.Math.GetValueAsDateTime(decDates[0]);
                string[] ExtraSrvArray = ExtraSrv.Split(';');
                string tableName = "SMIDN";
                if ("F".Equals(TranType) || "R".Equals(TranType))
                    tableName = "SMICNTR";
                string Cnt40OtherType = Prolink.Math.GetValueAsString(smsmi.Rows[0]["OCNT_TYPE"]);
                decimal Cnt40Other = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["OCNT_NUMBER"]);
                string partyNo = Prolink.Math.GetValueAsString(fdt.Rows[0]["LSP_CD"]);
                string pod = Prolink.Math.GetValueAsString(smsmi.Rows[0]["POD_CD"]);
                DateTime effectFrom = Prolink.Math.GetValueAsDateTime(fdt.Rows[0]["EFFECT_FROM"]);
                DateTime effectTo = Prolink.Math.GetValueAsDateTime(fdt.Rows[0]["MEFFECT_TO"]);
                string QuotNo = Prolink.Math.GetValueAsString(fdt.Rows[0]["QUOT_NO"]);
                
                Bill.WriteLog(string.Format("报关费用报价:{0}", QuotNo), ShipmentId);
                string Remark = string.Empty;

                string tableSql = string.Format(@"SELECT DEC_NO,SIS_FEE,SUF_COST FROM {0} WHERE SHIPMENT_ID={1}", tableName, SQLUtils.QuotedStr(ShipmentId));
                DataTable table= CommonHelp.getDataTableFromSql(tableSql);
                decimal TtlCntNum = Cnt20 + Cnt40 + Cnt40hq + Cnt40Other;
                foreach (DataRow qdr in fdt.Rows)
                {
                    string ChgCd = Prolink.Math.GetValueAsString(qdr["CHG_CD"]);
                    string ChgDescp = Prolink.Math.GetValueAsString(qdr["CHG_DESCP"]);
                    string Cur = Prolink.Math.GetValueAsString(qdr["CUR"]);
                    string Punit = Prolink.Math.GetValueAsString(qdr["PUNIT"]);
                    decimal F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);
                    string qHoliDay = Prolink.Math.GetValueAsString(qdr["HOLIDAY"]);
                    string qProdType = Prolink.Math.GetValueAsString(qdr["PRODUCT_TYPE"]);

                    int HsQtyFrom = Prolink.Math.GetValueAsInt(qdr["HS_QTY_FROM"]);
                    int HsQtyTo = Prolink.Math.GetValueAsInt(qdr["HS_QTY_TO"]);
                    int CntrQtyFrom = Prolink.Math.GetValueAsInt(qdr["CNTR_QTY_FROM"]);
                    int CntrQtyTo = Prolink.Math.GetValueAsInt(qdr["CNTR_QTY_TO"]);
                    string remark = Prolink.Math.GetValueAsString(qdr["REMARK"]);
                    int HsQty = 0;
                    decimal TtlFee = 0;
                    DataTable HsQtyDt = new DataTable();
                    if (Tc == false)
                    {
                        #region 報關用
                        if (TranType == "F" || TranType == "R")
                        {
                            sql = "SELECT DEC_DATE,DEC_NO,INSPECTION,ICDF,SUM(HS_QTY) AS HS_QTY,CNTRY_CD,COUNT(CNTR_NO) AS CNTR_QTY FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY DEC_NO,DEC_DATE,INSPECTION,ICDF,CNTRY_CD";
                        }
                        else
                        {
                            sql = "SELECT DEC_DATE,DEC_NO,INSPECTION,ICDF,SUM(HS_QTY) AS HS_QTY,CNTRY_CD FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY DEC_NO,DEC_DATE,INSPECTION,ICDF,CNTRY_CD";
                        }
                        DataTable cdt = CommonHelp.getDataTableFromSql(sql);

                        if (cdt.Rows.Count > 0)
                        {
                            if (ChgCd == "ICD" && Punit == "DECA")
                            {
                                List<string> decList = new List<string>();
                                foreach (DataRow cdr in cdt.Rows)
                                {
                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["DEC_DATE"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["CNTRY_CD"]);
                                    string DecNo = Prolink.Math.GetValueAsString(cdr["DEC_NO"]);
                                    if (!string.IsNullOrEmpty(DecNo) && !decList.Contains(DecNo))
                                        decList.Add(DecNo);
                                    if (string.IsNullOrEmpty(DecNo) && !decCount.ContainsKey(DecNo))
                                    {
                                        decCount.Add(DecNo, 0);
                                        string key = ShipmentId + ";" + DecNo;
                                        if (!shipmentDecCount.ContainsKey(key))
                                            shipmentDecCount.Add(key, 1);
                                    }
                                    if (DecCntrDn.Contains(ChgCd + DecNo))
                                    {
                                        continue;
                                    }
                                    string IsHoliday = GetHoliday(decdate, CntryCd);
                                    decimal amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, "", F3);
                                    if (!ChgAmt.ContainsKey(ChgCd))
                                    {
                                        ChgAmt.Add(ChgCd, 0);
                                    }
                                    ChgAmt[ChgCd] = amt;
                                    setRemarkUnit(ChgRemarkUnit, ChgCd + DecNo, Punit, remark, amt);
                                    if (amt > 0)
                                    {
                                        DecCntrDn.Add(ChgCd + DecNo);
                                    }
                                }
                                if (decList.Count > 0)
                                {
                                    sql = string.Format(@"SELECT DEC_NO,I.SHIPMENT_ID,COUNT(*) CO FROM SMSMI I,{0} T WHERE I.SHIPMENT_ID=T.SHIPMENT_ID AND DEC_NO IN {1} AND POD_CD={2}
AND I.ETA BETWEEN {3} AND {4} AND TRAN_TYPE={6} AND EXISTS (SELECT 1 FROM BSTERM WHERE CMP=I.CMP AND FRT_TERM=I.FRT_TERM AND INCOTERM_CD=I.INCOTERM_CD AND NEED_CHG LIKE '%BC%') AND 
EXISTS (SELECT 1 FROM SMSMIPT WHERE PARTY_TYPE IN ('IBBR', 'IBTC') AND SHIPMENT_ID=I.SHIPMENT_ID AND PARTY_NO={5}) GROUP BY I.SHIPMENT_ID,DEC_NO", tableName,
SQLUtils.Quoted(decList.ToArray()), SQLUtils.QuotedStr(pod), SQLUtils.QuotedStr(effectFrom.ToString("yyyyMMdd")), SQLUtils.QuotedStr(effectTo.ToString("yyyyMMdd")),
SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(TranType));
                                    cdt = CommonHelp.getDataTableFromSql(sql);
                                    foreach (DataRow cdr in cdt.Rows)
                                    {
                                        string mySipmentId = Prolink.Math.GetValueAsString(cdr["SHIPMENT_ID"]);
                                        if (!shipmentList.Contains(mySipmentId) && !mySipmentId.Equals(ShipmentId))
                                            shipmentList.Add(mySipmentId);
                                        string decNo = Prolink.Math.GetValueAsString(cdr["DEC_NO"]);
                                        decimal num = Prolink.Math.GetValueAsInt(cdr["CO"]);
                                        string key = mySipmentId + ";" + decNo;
                                        if (!shipmentDecCount.ContainsKey(key))
                                            shipmentDecCount.Add(key, num);
                                        if (!decCount.ContainsKey(decNo))
                                            decCount.Add(decNo, 0);
                                        decCount[decNo] += num;
                                    }
                                }

                            }
                            else if (ChgCd == "SISCOMEX")
                            {
                                List<string> decList = new List<string>();
                                decimal total = 0;
                                foreach (DataRow cdr in table.Rows)
                                {
                                    decimal sisFee = Prolink.Math.GetValueAsDecimal(cdr["SIS_FEE"]);
                                    if (sisFee == 0) continue;
                                    string decNo = Prolink.Math.GetValueAsString(cdr["DEC_NO"]);
                                    if (!decList.Contains(decNo))
                                    {
                                        decList.Add(decNo);
                                        total += sisFee;
                                    }
                                }
                                if (!ChgAmt.ContainsKey(ChgCd))
                                {
                                    ChgAmt.Add(ChgCd, total);
                                }
                                setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, total);
                            }
                            else if (ChgCd == "SUFRAMA")
                            {
                                decimal total = 0;
                                foreach (DataRow cdr in table.Rows)
                                {
                                    decimal sufCost = Prolink.Math.GetValueAsDecimal(cdr["SUF_COST"]);
                                    total += sufCost;
                                }
                                if (!ChgAmt.ContainsKey(ChgCd))
                                {
                                    ChgAmt.Add(ChgCd, total);
                                }
                                setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, total);
                            }
                            else if (Punit == "DEC" || Punit == "DECA")
                            {
                                #region 計價單位為Dec算法
                                foreach (DataRow cdr in cdt.Rows)
                                {
                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["DEC_DATE"]);
                                    string ProductType = string.Empty;
                                    string Inspection = Prolink.Math.GetValueAsString(cdr["INSPECTION"]);
                                    string Icdf = Prolink.Math.GetValueAsString(cdr["ICDF"]);
                                    string DecNo = Prolink.Math.GetValueAsString(cdr["DEC_NO"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["CNTRY_CD"]);
                                    //string refNo = Prolink.Math.GetValueAsString(cdr["REF_NO"]);
                                    int CntrQty = 0;
                                    if (TranType == "F" || TranType == "R")
                                    {
                                        CntrQty = Prolink.Math.GetValueAsInt(cdr["CNTR_QTY"]);
                                    }

                                    HsQty = Prolink.Math.GetValueAsInt(cdr["HS_QTY"]);

                                    if (HsQtyFrom > 0)
                                    {
                                        if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                        {
                                            Message.Add(string.Format(ChgDescp + ":" + ShipmentId + " QTY/HS IS NOT between {0} and {1}", HsQtyFrom, HsQtyTo));
                                            continue;
                                        }
                                    }

                                    if (CntrQtyFrom > 0)
                                    {
                                        if (CntrQty < CntrQtyFrom || CntrQty > CntrQtyTo)
                                        {
                                            Message.Add(string.Format(ChgDescp + ":" + ShipmentId + " container quantity is not between {0} and {1}", CntrQtyFrom, CntrQtyTo));
                                            continue;
                                        }
                                    }

                                    if (DecCntrDn.Contains(ChgCd + DecNo))
                                    {
                                        continue;
                                    }

                                    string IsHoliday = GetHoliday(decdate, CntryCd);
                                    decimal amt = 0;
                                    if (ChgCdCheck(TtlCntNum, ChgCd, ChgDescp, cdr))
                                    {
                                        amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                        if (!ChgAmt.ContainsKey(ChgCd))
                                        {
                                            ChgAmt.Add(ChgCd, 0);
                                        }
                                        ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                        if (amt > 0)
                                        {
                                            DecCntrDn.Add(ChgCd + DecNo);
                                        }
                                    }
                                    setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt);
                                }
                                #endregion

                            }
                            else if (Punit == "CTR" || Punit == "DN")
                            {

                                #region 計價單位為container, dn算法
                                string ctrSql = string.Format("SELECT DISTINCT CNTR_NO FROM SMICNTR WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(ShipmentId));
                                DataTable trdt = CommonHelp.getDataTableFromSql(ctrSql);

                                if (TranType == "F" || TranType == "R")
                                {
                                    sql = "SELECT CNTR_NO AS UNI_KEY,DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,SUM(HS_QTY) AS HS_QTY,CNTRY_CD FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY CNTR_NO,DEC_NO,DEC_DATE,DIVISION_DESCP,INSPECTION,ICDF,CNTRY_CD";
                                }
                                else
                                {
                                    sql = "SELECT DN_NO AS UNI_KEY,DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,SUM(HS_QTY) AS HS_QTY,CNTRY_CD FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY DN_NO,DEC_NO,DEC_DATE,DIVISION_DESCP,INSPECTION,ICDF,CNTRY_CD";
                                }
                                cdt = CommonHelp.getDataTableFromSql(sql);

                                F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);
                                DataRow[] fdr = fdt.Select("CHG_CD='ACCF'");
                                Accf = fdr.Length;
                                if (cdt.Rows.Count > 0)
                                {
                                    int k = 0;
                                    foreach (DataRow cdr in cdt.Rows)
                                    {
                                        DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["DEC_DATE"]);
                                        string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                        string Inspection = Prolink.Math.GetValueAsString(cdr["INSPECTION"]);
                                        string Icdf = Prolink.Math.GetValueAsString(cdr["ICDF"]);
                                        string DecNo = Prolink.Math.GetValueAsString(cdr["DEC_NO"]);
                                        string CntryCd = Prolink.Math.GetValueAsString(cdr["CNTRY_CD"]);
                                        string key = Prolink.Math.GetValueAsString(cdr["UNI_KEY"]);
                                        //string refNo = Prolink.Math.GetValueAsString(cdr["REF_NO"]);
                                        HsQty = Prolink.Math.GetValueAsInt(cdr["HS_QTY"]);

                                        if (HsQtyFrom > 0)
                                        {
                                            if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                            {
                                                Message.Add(string.Format(ChgDescp + ":" + key + " QTY/HS IS NOT between {0} and {1}", HsQtyFrom, HsQtyTo));
                                                continue;
                                            }
                                        }

                                        if (DecCntrDn.Contains(ChgCd + key))
                                        {
                                            continue;
                                        }

                                        string IsHoliday = GetHoliday(decdate, CntryCd);
                                        decimal amt = 0;
                                        if (ChgCd == "CISC")
                                        {
                                            if (Inspection == "Y")
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }
                                            else
                                            {
                                                amt = 0;
                                                Message.Add(ChgDescp + ":" + key + " INSPECTION is 'no'");
                                            }
                                        }
                                        else if (ChgCd == "ICDF")
                                        {
                                            if (Icdf == "Y")
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }
                                            else
                                            {
                                                amt = 0;
                                                Message.Add(ChgDescp + ":" + key + " CC-Faster Solution is 'no'");
                                            }
                                        }
                                        else
                                        {
                                            if (Accf == 0 && (ChgCd == "ICD" || ChgCd == "ICDS"))
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                if ("L".Equals(TranType))
                                                {
                                                    ChgAmt[ChgCd] = amt;
                                                }
                                                else
                                                {
                                                    ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                                }
                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }
                                            else if (Accf > 0 && (ChgCd == "ICD" || ChgCd == "ICDS"))
                                            {
                                                if (k == 0)
                                                {
                                                    amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                    if (!ChgAmt.ContainsKey(ChgCd))
                                                    {
                                                        ChgAmt.Add(ChgCd, 0);
                                                    }
                                                    //ChgAmt[ChgCd] = amt * (cdt.Rows.Count - 1);//20170929改為ACCF和ICD並存時，算式為Container 數量-1乘上報價單的費用
                                                    //ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                                    ChgAmt[ChgCd] = amt;//變成當ICD計算的時候要看有沒有ACCF，沒有的話就正常算法，有的話就只算一個ICD的櫃子，其他用ACCF算
                                                    if (amt > 0)
                                                    {
                                                        DecCntrDn.Add(ChgCd + key);
                                                    }
                                                }
                                            }
                                            else if (ChgCd == "ACCF")
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                ChgAmt[ChgCd] = amt * (cdt.Rows.Count - 1);//20170929改為ACCF和ICD並存時，算式為Container 數量-1乘上報價單的費用
                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }
                                            else
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                if ("DTA".Equals(ChgCd) && "CTR".Equals(Punit))
                                                {
                                                    ChgAmt[ChgCd] = amt * (trdt.Rows.Count);
                                                }
                                                else
                                                {
                                                    ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                                }
                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }

                                        }
                                        setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt);

                                        k++;
                                    }
                                }
                                #endregion

                            }
                            else if (Punit == "HS")
                            {
                                #region 計價單位為HS Code Qty
                                if (TranType == "F" || TranType == "R")
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,CNTR_NO AS REF_NO FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                else
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,DN_NO AS REF_NO FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                cdt = CommonHelp.getDataTableFromSql(sql);

                                foreach (DataRow cdr in cdt.Rows)
                                {
                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["DEC_DATE"]);
                                    string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                    string DecNo = Prolink.Math.GetValueAsString(cdr["DEC_NO"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["CNTRY_CD"]);
                                    string refNo = Prolink.Math.GetValueAsString(cdr["REF_NO"]);
                                    HsQty = Prolink.Math.GetValueAsInt(cdr["HS_QTY"]);
                                    if (HsQtyFrom > 0)
                                    {
                                        if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                        {
                                            Message.Add(string.Format(ChgDescp + ":" + refNo + " QTY/HS IS NOT between {0} and {1}", HsQtyFrom, HsQtyTo));
                                            continue;
                                        }
                                    }
                                    if (DecCntrDn.Contains(ChgCd + DecNo))
                                    {
                                        continue;
                                    }

                                    string IsHoliday = GetHoliday(decdate, CntryCd);

                                    decimal amt = 0;
                                    if (ChgCdCheck(TtlCntNum, ChgCd, ChgDescp, cdr))
                                    {
                                        amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                        if (!ChgAmt.ContainsKey(ChgCd))
                                        {
                                            ChgAmt.Add(ChgCd, 0);
                                        }
                                        ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                        if (amt > 0)
                                        {
                                            DecCntrDn.Add(ChgCd + DecNo);
                                        }
                                    }
                                    setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt);
                                }
                                #endregion
                            }
                            else if (Punit == "ITEM")
                            {
                                #region 計價單位為Item
                                if (TranType == "F" || TranType == "R")
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,PD_QTY FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                else
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,PD_QTY FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                cdt = CommonHelp.getDataTableFromSql(sql);

                                foreach (DataRow cdr in cdt.Rows)
                                {
                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["DEC_DATE"]);
                                    string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                    string Inspection = Prolink.Math.GetValueAsString(cdr["INSPECTION"]);
                                    string Icdf = Prolink.Math.GetValueAsString(cdr["ICDF"]);
                                    string DecNo = Prolink.Math.GetValueAsString(cdr["DEC_NO"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["CNTRY_CD"]);
                                    int PdQty = Prolink.Math.GetValueAsInt(cdr["PD_QTY"]);
                                    HsQty = Prolink.Math.GetValueAsInt(cdr["HS_QTY"]);

                                    if (DecCntrDn.Contains(ChgCd + DecNo))
                                    {
                                        continue;
                                    }

                                    string IsHoliday = GetHoliday(decdate, CntryCd);
                                    decimal amt = 0;
                                    if (ChgCd == "ICDD")
                                    {
                                        amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                        if (!ChgAmt.ContainsKey(ChgCd))
                                        {
                                            ChgAmt.Add(ChgCd, 0);
                                        }
                                        ChgAmt[ChgCd] = ChgAmt[ChgCd] + (amt * PdQty);
                                        setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt, PdQty);
                                        if (amt > 0)
                                        {
                                            DecCntrDn.Add(ChgCd + DecNo);
                                        }
                                    }

                                }
                                #endregion
                            }
                            else if (Punit == "INVO")
                            {
                                #region 計價單位為Invioce
                                if (TranType == "F" || TranType == "R")
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,PD_QTY FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                else
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,PD_QTY FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                cdt = CommonHelp.getDataTableFromSql(sql);
                                foreach (DataRow cdr in cdt.Rows)
                                {
                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["DEC_DATE"]);
                                    string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["CNTRY_CD"]);

                                    if (DecCntrDn.Contains(ChgCd))
                                    {
                                        continue;
                                    }
                                    string IsHoliday = GetHoliday(decdate, CntryCd);
                                    decimal amt = 0;
                                    amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                    if (!ChgAmt.ContainsKey(ChgCd))
                                    {
                                        ChgAmt.Add(ChgCd, 0);
                                    }
                                    sql = string.Format("SELECT COUNT(*) FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                                    int sum = CommonHelp.getOneValueAsIntFromSql(sql);
                                    ChgAmt[ChgCd] = ChgAmt[ChgCd] + (amt * sum);
                                    setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt, sum);
                                    if (amt > 0)
                                    {
                                        DecCntrDn.Add(ChgCd);
                                    }
                                }
                                #endregion
                            }
                            else if (Punit == "BL" || Punit == "SHT")
                            {
                                sql = string.Format("SELECT SUM(HS_QTY) AS HS_QTY,INSPECTION,ICDF FROM SMIDN WHERE SHIPMENT_ID={0} GROUP BY INSPECTION,ICDF", SQLUtils.QuotedStr(ShipmentId));
                                if (TranType == "F" || TranType == "R")
                                    sql = string.Format("SELECT SUM(HS_QTY) AS HS_QTY,INSPECTION,ICDF FROM SMICNTR WHERE SHIPMENT_ID={0} GROUP BY INSPECTION,ICDF", SQLUtils.QuotedStr(ShipmentId));
                                cdt = CommonHelp.getDataTableFromSql(sql);
                                HsQty = cdt.AsEnumerable().Sum(row => Prolink.Math.GetValueAsInt(row["HS_QTY"]));
                                if (HsQtyFrom > 0)
                                {
                                    if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                    {
                                        Message.Add(string.Format(ChgDescp + ":" + ShipmentId + " QTY/HS IS NOT between {0} and {1}", HsQtyFrom, HsQtyTo));
                                        continue;
                                    }
                                }
                                decimal amt = 0;
                                if (ChgCdCheck(TtlCntNum, ChgCd, ChgDescp, cdt.Rows[0]))
                                {
                                    string IsHoliday = GetHoliday(smDecDate, country);
                                    amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, product_type, F3);
                                    if (!ChgAmt.ContainsKey(ChgCd))
                                    {
                                        ChgAmt.Add(ChgCd, 0);
                                    }
                                    ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                    setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt, 1);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region 轉關用
                        if (TranType == "F" || TranType == "R")
                        {
                            sql = "SELECT TC_DEC_DATE,DIVISION_DESCP,TC_DEC_NO,TC_INSPECTION,TC_ICDF,SUM(TC_HS_QTY) AS TC_HS_QTY, TC_CNTRY_CD, COUNT(CNTR_NO) CNTR_QTY FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY TC_DEC_NO,TC_DEC_DATE,DIVISION_DESCP,TC_INSPECTION,TC_ICDF,TC_CNTRY_CD";
                        }
                        else
                        {
                            sql = "SELECT TC_DEC_DATE,DIVISION_DESCP,TC_DEC_NO,TC_INSPECTION,TC_ICDF,SUM(TC_HS_QTY) AS TC_HS_QTY, TC_CNTRY_CD FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY TC_DEC_NO,TC_DEC_DATE,DIVISION_DESCP,TC_INSPECTION,TC_ICDF,TC_CNTRY_CD";
                        }
                        DataTable cdt = CommonHelp.getDataTableFromSql(sql);

                        if (cdt.Rows.Count > 0)
                        {
                            if (Punit == "DEC" || Punit == "DECA")
                            {
                                #region 計價單位為Dec, BL, Shipment算法
                                foreach (DataRow cdr in cdt.Rows)
                                {

                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["TC_DEC_DATE"]);
                                    string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                    string TcInspection = Prolink.Math.GetValueAsString(cdr["TC_INSPECTION"]);
                                    string Icdf = Prolink.Math.GetValueAsString(cdr["TC_ICDF"]);
                                    string DecNo = Prolink.Math.GetValueAsString(cdr["TC_DEC_NO"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["TC_CNTRY_CD"]);
                                    //string refNo=Prolink.Math.GetValueAsString(cdr["REF_NO"]);
                                    int CntrQty = 0;

                                    if (TranType == "F" || TranType == "R")
                                    {
                                        CntrQty = Prolink.Math.GetValueAsInt(cdr["CNTR_QTY"]);
                                    }

                                    HsQty = Prolink.Math.GetValueAsInt(cdr["TC_HS_QTY"]);

                                    if (HsQtyFrom > 0)
                                    {
                                        if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                        {
                                            Message.Add(string.Format(ChgDescp + ":" + ShipmentId + " QTY/HS Code is not between {0} and {1}", HsQtyFrom, HsQtyTo));
                                            continue;
                                        }
                                    }

                                    if (CntrQtyFrom > 0)
                                    {
                                        if (CntrQty < CntrQtyFrom || CntrQty > CntrQtyTo)
                                        {
                                            Message.Add(string.Format(ChgDescp + ":" + ShipmentId + " container quantity is not between {0} and {1}", CntrQtyFrom, CntrQtyTo));
                                            continue;
                                        }
                                    }

                                    if (DecCntrDn.Contains(ChgCd + DecNo))
                                    {
                                        continue;
                                    }

                                    string IsHoliday = GetHoliday(decdate, CntryCd);
                                    decimal amt = 0;
                                    if (ChgCd == "T1_CISC")
                                    {
                                        if (TcInspection == "Y")
                                        {
                                            amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                            if (!ChgAmt.ContainsKey(ChgCd))
                                            {
                                                ChgAmt.Add(ChgCd, 0);
                                            }
                                            ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                            if (amt > 0)
                                            {
                                                DecCntrDn.Add(ChgCd + DecNo);
                                            }
                                        }
                                        else
                                        {
                                            Message.Add(ChgDescp + ":" + ShipmentId + " INSPECTION is 'no'");
                                        }
                                    }
                                    else if (ChgCd == "T1_ACCF")
                                    {
                                        if (TtlCntNum > 1)
                                        {
                                            amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                            if (!ChgAmt.ContainsKey(ChgCd))
                                            {
                                                ChgAmt.Add(ChgCd, 0);
                                            }
                                            ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                            if (amt > 0)
                                            {
                                                DecCntrDn.Add(ChgCd + DecNo);
                                            }
                                        }
                                        else
                                        {
                                            Message.Add(ChgDescp + ":" + ShipmentId + " container quantity 0");
                                        }
                                    }
                                    else if (ChgCd == "T1_ICDF")
                                    {
                                        if (Icdf == "Y")
                                        {
                                            amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                            if (!ChgAmt.ContainsKey(ChgCd))
                                            {
                                                ChgAmt.Add(ChgCd, 0);
                                            }
                                            ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                            if (amt > 0)
                                            {
                                                DecCntrDn.Add(ChgCd + DecNo);
                                            }
                                        }
                                        else
                                        {
                                            Message.Add(ChgDescp + ":" + ShipmentId + " CC-Faster Solution is 'no'");
                                        }
                                    }
                                    else
                                    {
                                        amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                        if (!ChgAmt.ContainsKey(ChgCd))
                                        {
                                            ChgAmt.Add(ChgCd, 0);
                                        }
                                        ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                        if (amt > 0)
                                        {
                                            DecCntrDn.Add(ChgCd + DecNo);
                                        }
                                    }
                                    setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt);
                                }
                                #endregion

                            }
                            else if (Punit == "CTR" || Punit == "DN")
                            {
                                #region 計價單位為container, dn算法
                                if (TranType == "F" || TranType == "R")
                                {
                                    sql = "SELECT CNTR_NO AS UNI_KEY,TC_DEC_DATE,DIVISION_DESCP,TC_DEC_NO,TC_INSPECTION,TC_ICDF,SUM(TC_HS_QTY) AS HS_QTY,TC_CNTRY_CD FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY CNTR_NO,TC_DEC_NO,TC_DEC_DATE,DIVISION_DESCP,TC_INSPECTION,TC_ICDF,TC_CNTRY_CD";
                                }
                                else
                                {
                                    sql = "SELECT DN_NO AS UNI_KEY, TC_DEC_DATE,DIVISION_DESCP,TC_DEC_NO,TC_INSPECTION,TC_ICDF,SUM(TC_HS_QTY) AS HS_QTY,TC_CNTRY_CD FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " GROUP BY DN_NO,TC_DEC_NO,TC_DEC_DATE,DIVISION_DESCP,TC_INSPECTION,TC_ICDF,TC_CNTRY_CD";
                                }
                                cdt = CommonHelp.getDataTableFromSql(sql);

                                F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);
                                DataRow[] fdr = fdt.Select("CHG_CD='T1_ACCF'");
                                Accf = fdr.Length;
                                if (cdt.Rows.Count > 0)
                                {
                                    int k = 0;
                                    foreach (DataRow cdr in cdt.Rows)
                                    {
                                        DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["TC_DEC_DATE"]);
                                        string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                        string TcInspection = Prolink.Math.GetValueAsString(cdr["TC_INSPECTION"]);
                                        string Icdf = Prolink.Math.GetValueAsString(cdr["TC_ICDF"]);
                                        string key = Prolink.Math.GetValueAsString(cdr["UNI_KEY"]);
                                        string CntryCd = Prolink.Math.GetValueAsString(cdr["TC_CNTRY_CD"]);
                                        HsQty = Prolink.Math.GetValueAsInt(cdr["HS_QTY"]);

                                        if (Punit == "CTR")
                                        {
                                            if (CntrQtyFrom > 0)
                                            {
                                                if (cdt.Rows.Count < CntrQtyFrom || cdt.Rows.Count > CntrQtyTo)
                                                {
                                                    Message.Add(string.Format(ChgDescp + ":" + key + " container quantity is not between {0} and {1}", CntrQtyFrom, CntrQtyTo));
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (HsQtyFrom > 0)
                                            {
                                                if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                                {
                                                    Message.Add(string.Format(ChgDescp + ":" + key + " QTY/HS IS NOT between {0} and {1}", HsQtyFrom, HsQtyTo));
                                                    continue;
                                                }
                                            }

                                        }

                                        if (DecCntrDn.Contains(ChgCd + key))
                                        {
                                            continue;
                                        }

                                        string IsHoliday = GetHoliday(decdate, CntryCd);
                                        decimal amt = 0;
                                        if (ChgCd == "T1_CISC")
                                        {
                                            if (TcInspection == "Y")
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }
                                            else
                                            {
                                                Message.Add(ChgDescp + ":" + key + " INSPECTION is 'no'");
                                            }
                                        }
                                        else if (ChgCd == "T1_ICDF")
                                        {
                                            if (Icdf == "Y")
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }
                                            else
                                            {
                                                Message.Add(ChgDescp + ":" + key + " CC-Faster Solution is 'no'");
                                            }
                                        }
                                        else
                                        {
                                            if (Accf == 0 && (ChgCd == "T1_ICD" || ChgCd == "T1_ICDS"))
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }
                                            else if (Accf > 0 && (ChgCd == "T1_ICD" || ChgCd == "T1_ICDS"))
                                            {
                                                if (k == 0)
                                                {
                                                    TtlFee = amt;
                                                }
                                            }
                                            else if (ChgCd == "ACCF")
                                            {
                                                //TtlFee = amt * (cdt.Rows.Count - 1);
                                                ChgAmt[ChgCd] = amt * (cdt.Rows.Count - 1);
                                            }
                                            else
                                            {
                                                amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                                if (!ChgAmt.ContainsKey(ChgCd))
                                                {
                                                    ChgAmt.Add(ChgCd, 0);
                                                }
                                                ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;

                                                if (amt > 0)
                                                {
                                                    DecCntrDn.Add(ChgCd + key);
                                                }
                                            }

                                        }
                                        setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt);

                                        k++;
                                    }
                                }
                                #endregion

                            }
                            else if (Punit == "HS")
                            {
                                #region 計價單位為Dec, BL, Shipment算法
                                if (TranType == "F" || TranType == "R")
                                {
                                    sql = "SELECT TC_DEC_DATE,DIVISION_DESCP,TC_DEC_NO,TC_INSPECTION,TC_ICDF,SUM(TC_HS_QTY) AS TC_HS_QTY, TC_CNTRY_CD,CNTR_NO AS REF_NO FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                else
                                {
                                    sql = "SELECT TC_DEC_DATE,DIVISION_DESCP,TC_DEC_NO,TC_INSPECTION,TC_ICDF,SUM(TC_HS_QTY) AS TC_HS_QTY, TC_CNTRY_CD,DN_NO AS REF_NO FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                cdt = CommonHelp.getDataTableFromSql(sql);
                                foreach (DataRow cdr in cdt.Rows)
                                {
                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["TC_DEC_DATE"]);
                                    string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                    string TcInspection = Prolink.Math.GetValueAsString(cdr["TC_INSPECTION"]);
                                    string Icdf = Prolink.Math.GetValueAsString(cdr["TC_ICDF"]);
                                    string DecNo = Prolink.Math.GetValueAsString(cdr["TC_DEC_NO"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["TC_CNTRY_CD"]);
                                    string refNo = Prolink.Math.GetValueAsString(cdr["REF_NO"]);
                                    HsQty = Prolink.Math.GetValueAsInt(cdr["TC_HS_QTY"]);

                                    if (HsQtyFrom > 0)
                                    {
                                        if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                        {
                                            Message.Add(string.Format(ChgDescp + ":" + refNo + " QTY/HS IS NOT between {0} and {1}", HsQtyFrom, HsQtyTo));
                                            continue;
                                        }
                                    }

                                    if (DecCntrDn.Contains(ChgCd + DecNo))
                                    {
                                        continue;
                                    }

                                    string IsHoliday = GetHoliday(decdate, CntryCd);

                                    decimal amt = 0;
                                    if (ChgCd == "T1_CISC")
                                    {
                                        if (TcInspection == "Y")
                                        {
                                            amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                            if (!ChgAmt.ContainsKey(ChgCd))
                                            {
                                                ChgAmt.Add(ChgCd, 0);
                                            }
                                            ChgAmt[ChgCd] = ChgAmt[ChgCd] + (amt * HsQty);

                                            if (amt > 0)
                                            {
                                                DecCntrDn.Add(ChgCd + DecNo);
                                            }
                                        }
                                        else
                                        {
                                            Message.Add(ChgDescp + ":" + refNo + " INSPECTION is 'no'");
                                        }
                                    }
                                    else if (ChgCd == "T1_ACCF")
                                    {
                                        if (TtlCntNum > 1)
                                        {
                                            amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                            if (!ChgAmt.ContainsKey(ChgCd))
                                            {
                                                ChgAmt.Add(ChgCd, 0);
                                            }
                                            ChgAmt[ChgCd] = ChgAmt[ChgCd] + (amt * HsQty);

                                            if (amt > 0)
                                            {
                                                DecCntrDn.Add(ChgCd + DecNo);
                                            }
                                        }
                                        else
                                        {
                                            Message.Add(ChgDescp + ":" + ShipmentId + " container quantity 0");
                                        }
                                    }
                                    else if (ChgCd == "T1_ICDF")
                                    {
                                        if (Icdf == "Y")
                                        {
                                            amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                            if (!ChgAmt.ContainsKey(ChgCd))
                                            {
                                                ChgAmt.Add(ChgCd, 0);
                                            }
                                            ChgAmt[ChgCd] = ChgAmt[ChgCd] + (amt * HsQty);

                                            if (amt > 0)
                                            {
                                                DecCntrDn.Add(ChgCd + DecNo);
                                            }
                                        }
                                        else
                                        {
                                            Message.Add(ChgDescp + ":" + refNo + " CC-Faster Solution is 'no'");
                                        }
                                    }
                                    else
                                    {
                                        amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                        if (!ChgAmt.ContainsKey(ChgCd))
                                        {
                                            ChgAmt.Add(ChgCd, 0);
                                        }
                                        ChgAmt[ChgCd] = ChgAmt[ChgCd] + (amt * HsQty);

                                        if (amt > 0)
                                        {
                                            DecCntrDn.Add(ChgCd + DecNo);
                                        }
                                    }
                                    setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt, HsQty);
                                }
                                #endregion
                            }
                            else if (Punit == "INVO")
                            {
                                #region 計價單位為Invioce
                                if (TranType == "F" || TranType == "R")
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,PD_QTY FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                else
                                {
                                    sql = "SELECT DEC_DATE,DIVISION_DESCP,DEC_NO,INSPECTION,ICDF,HS_QTY,CNTRY_CD,PD_QTY FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                                }
                                cdt = CommonHelp.getDataTableFromSql(sql);
                                foreach (DataRow cdr in cdt.Rows)
                                {
                                    DateTime decdate = Prolink.Math.GetValueAsDateTime(cdr["DEC_DATE"]);
                                    string ProductType = Prolink.Math.GetValueAsString(cdr["DIVISION_DESCP"]);
                                    string CntryCd = Prolink.Math.GetValueAsString(cdr["CNTRY_CD"]);

                                    if (DecCntrDn.Contains(ChgCd))
                                    {
                                        continue;
                                    }
                                    string IsHoliday = GetHoliday(decdate, CntryCd);
                                    decimal amt = 0;
                                    amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, ProductType, F3);
                                    if (!ChgAmt.ContainsKey(ChgCd))
                                    {
                                        ChgAmt.Add(ChgCd, 0);
                                    }
                                    sql = string.Format("SELECT COUNT(*) FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                                    int sum = CommonHelp.getOneValueAsIntFromSql(sql);
                                    ChgAmt[ChgCd] = ChgAmt[ChgCd] + (amt * sum);
                                    setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt, sum);
                                    if (amt > 0)
                                    {
                                        DecCntrDn.Add(ChgCd);
                                    }
                                }
                                #endregion
                            }
                            else if (Punit == "BL" || Punit == "SHT")
                            {
                                sql = string.Format("SELECT SUM(HS_QTY) AS HS_QTY,INSPECTION,ICDF FROM SMIDN WHERE SHIPMENT_ID={0} GROUP BY INSPECTION,ICDF", SQLUtils.QuotedStr(ShipmentId));
                                if (TranType == "F" || TranType == "R")
                                    sql = string.Format("SELECT SUM(HS_QTY) AS HS_QTY,INSPECTION,ICDF FROM SMICNTR WHERE SHIPMENT_ID={0} GROUP BY INSPECTION,ICDF", SQLUtils.QuotedStr(ShipmentId));
                                cdt = CommonHelp.getDataTableFromSql(sql);
                                HsQty = cdt.AsEnumerable().Sum(row => Prolink.Math.GetValueAsInt(row["HS_QTY"]));
                                if (HsQtyFrom > 0)
                                {
                                    if (HsQty < HsQtyFrom || HsQty > HsQtyTo)
                                    {
                                        Message.Add(string.Format(ChgDescp + ":" + ShipmentId + " QTY/HS IS NOT between {0} and {1}", HsQtyFrom, HsQtyTo));
                                        continue;
                                    }
                                }
                                decimal amt = 0;
                                if (ChgCdCheck(TtlCntNum, ChgCd, ChgDescp, cdt.Rows[0]))
                                {
                                    string IsHoliday = GetHoliday(smDecDate, country);
                                    amt = GetBrokerMFee(IsHoliday, qHoliDay, qProdType, product_type, F3);
                                    if (!ChgAmt.ContainsKey(ChgCd))
                                    {
                                        ChgAmt.Add(ChgCd, 0);
                                    }
                                    ChgAmt[ChgCd] = ChgAmt[ChgCd] + amt;
                                }
                                setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt, 1);
                            }
                        }
                        #endregion
                    }
                }

                try
                {
                    decimal TtlFee = 0;
                    List<string> RemarkUnit = new List<string>();
                    if (shipmentList.Count > 0)
                    {
                        sql = string.Format("SELECT * FROM SMSMI M INNER JOIN SMSMIPT D ON D.U_FID=M.U_ID AND PARTY_TYPE IN ('IBBR', 'IBTC') WHERE M.SHIPMENT_ID IN {0}"
                            , SQLUtils.Quoted(shipmentList.ToArray()));
                        allSmDt = CommonHelp.getDataTableFromSql(sql);
                    }
                    foreach (var item in ChgAmt)
                    {
                        string ChgCd = item.Key;
                        TtlFee = item.Value;
                        if (ChgRemarkUnit.ContainsKey(ChgCd))
                            RemarkUnit = ChgRemarkUnit[ChgCd];
                        if ("ICD".Equals(ChgCd) && shipmentDecCount.Count > 0)
                        {
                            foreach (string key in shipmentDecCount.Keys)
                            {
                                List<string> myRemark = new List<string>();
                                string decNo = key.Split(';')[1];
                                if (ChgRemarkUnit.ContainsKey(ChgCd + decNo))
                                    myRemark = ChgRemarkUnit[ChgCd + decNo];
                                string tShipment = key.Split(';')[0];
                                decimal fee = TtlFee;
                                if (decCount[decNo] > 0)
                                {
                                    if (myRemark.Count > 3)
                                        myRemark[3] = (shipmentDecCount[key] / decCount[decNo]).ToString();
                                    fee = (shipmentDecCount[key] / decCount[decNo]) * TtlFee;
                                }
                                DataRow smdr = smsmi.Rows[0];
                                if (!tShipment.Equals(ShipmentId))
                                    smdr = allSmDt.Select(string.Format("SHIPMENT_ID ={0}", SQLUtils.QuotedStr(tShipment)))[0];
                                if (string.IsNullOrEmpty(decNo))
                                    decNo = "NULL";
                                setBrokerFee(smdr, fdt, Message, tShipment, TranType, QuotNo, fee, myRemark, ChgCd, decNo);
                            }
                        } 
                        else
                            setBrokerFee(smsmi.Rows[0], fdt, Message, ShipmentId, TranType, QuotNo, TtlFee, RemarkUnit, ChgCd);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void setBrokerFee(DataRow smsmi, DataTable fdt, List<string> Message, string ShipmentId, string TranType, string QuotNo, decimal TtlFee, List<string> RemarkUnit, string ChgCd, string decNo = "")
        {
            List<string> msg = new List<string>();
            string Location = Prolink.Math.GetValueAsString(smsmi["CMP"]);
            string sql = "SELECT TOP 1 * FROM SMQTD WHERE CHG_CD={0} AND QUOT_NO={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(QuotNo));
            DataTable dt = CommonHelp.getDataTableFromSql(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string ChgDescp = Prolink.Math.GetValueAsString(dr["CHG_DESCP"]);
                string Cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                string smbidUid = string.Empty;
                MixedList mixList = new MixedList();
                string LspNo = Prolink.Math.GetValueAsString(fdt.Rows[0]["LSP_CD"]);
                msg.Add(ChgCd);
                msg.Add(Prolink.Math.GetValueAsString(TtlFee));
                smbidUid = CalCualteFeeHandle.chkFeeExist(ChgCd, ShipmentId, Prolink.Math.GetValueAsString(fdt.Rows[0]["LSP_CD"]), Location, "", decNo);
                EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                string uid = System.Guid.NewGuid().ToString();
                if (!string.IsNullOrEmpty(smbidUid))
                {
                    ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    ei.PutKey("U_ID", smbidUid);
                    msg.Add("update");
                }
                else
                {
                    msg.Add("add");
                    ei.Put("U_ID", uid);
                }

                ei.Put("SHIPMENT_ID", ShipmentId);
                ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(smsmi["GROUP_ID"]));
                ei.Put("SEC_CMP", Prolink.Math.GetValueAsString(smsmi["SEC_CMP"]));
                ei.Put("CMP", Location);
                ei.Put("STN", Prolink.Math.GetValueAsString(smsmi["STN"]));
                //ei.Put("DEP", Prolink.Math.GetValueAsString(smsmi.Rows[0]["DEP"]));
                ei.Put("QUOT_NO", QuotNo);
                ei.Put("RFQ_NO", Prolink.Math.GetValueAsString(dr["RFQ_NO"]));
                ei.Put("QAMT", TtlFee);
                if (!string.IsNullOrEmpty(decNo) && !"NULL".Equals(decNo.ToUpper()))
                    ei.Put("DEC_NO", decNo);
                ei.Put("QTAX", 0);
                ei.Put("CHG_CD", ChgCd);
                ei.Put("CHG_DESCP", ChgDescp);
                ei.Put("QCUR", Cur);
                ei.Put("LSP_NO", Prolink.Math.GetValueAsString(fdt.Rows[0]["LSP_CD"]));
                ei.Put("LSP_NM", Prolink.Math.GetValueAsString(fdt.Rows[0]["LSP_NM"]));
                ei.Put("TRAN_TYPE", TranType);
                ei.Put("CNTR_INFO", Prolink.Math.GetValueAsString(smsmi["CNTR_INFO"]));
                ei.Put("MASTER_NO", Prolink.Math.GetValueAsString(smsmi["MASTER_NO"]));
                ei.Put("BL_NO", Prolink.Math.GetValueAsString(smsmi["MASTER_NO"]));
                ei.Put("POD_CD", Prolink.Math.GetValueAsString(smsmi["POD_CD"]));
                SetCHGInfo(dr, ei, smsmi, RemarkUnit);
                TPV.Financial.InboundBill.SetEstInfo(ei);
                mixList.Add(ei);


                if (mixList.Count > 0)
                {
                    try
                    {
                        CalculateLocalAmt(mixList, smsmi);
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        msg = new List<string>();
                        msg.Add(ex.ToString());
                    }
                    Bill.WriteLog(string.Join(";", msg), ShipmentId);
                }
            }
        }

        public static string getBrokerSql(string shipmentId, string tranType, bool Tc = false)
        {
            string sql = "SELECT DEC_DATE,DEC_NO,INSPECTION,ICDF,SUM(HS_QTY) AS HS_QTY,CNTRY_CD,COUNT(CNTR_NO) AS CNTR_QTY,CNTR_NO AS REF_NO FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentId) + " GROUP BY DEC_NO,DEC_DATE,INSPECTION,ICDF,CNTRY_CD,REF_NO";
            if ("F".Equals(tranType) || "R".Equals(tranType))
                sql = "SELECT DEC_DATE,DEC_NO,INSPECTION,ICDF,SUM(HS_QTY) AS HS_QTY,CNTRY_CD,DN_NO AS REF_NO FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentId) + " GROUP BY DEC_NO,DEC_DATE,INSPECTION,ICDF,CNTRY_CD,REF_NO";
            return sql;
        }

        private static string GetHoliday(DateTime decDate,string country)
        {
            string sql = "SELECT D_DAY FROM BSDATE WHERE D_DAY=" + SQLUtils.QuotedStr(decDate.ToString("yyyy-MM-dd")) + " AND CNTRY_CD=" + SQLUtils.QuotedStr(country);
            return CommonHelp.getOneValueAsStringFromSql(sql);
        }

        private static void setRemarkUnit(Dictionary<string, List<string>> ChgRemarkUnit, string ChgCd, string Punit, string remark, decimal amt, int Quantity = 1)
        {
            List<string> strs = new List<string>();
            if (!ChgRemarkUnit.ContainsKey(ChgCd))
            {
                strs.Add(remark);
                strs.Add(Punit);
                strs.Add(Prolink.Math.GetValueAsString(amt));
                strs.Add(Prolink.Math.GetValueAsString(Quantity));
                ChgRemarkUnit.Add(ChgCd, strs);
            }
            else
            {
                if (amt > 0)
                {
                    if (Quantity == 0)
                        Quantity = 1;
                    strs = ChgRemarkUnit[ChgCd];
                    if (strs.Count > 3)
                    {
                        if ("0".Equals(strs[2]))
                            strs[2] = Prolink.Math.GetValueAsString(amt);
                        int sum = Prolink.Math.GetValueAsInt(strs[3]);
                        sum += Quantity;
                        strs[3] = Prolink.Math.GetValueAsString(sum);
                        ChgRemarkUnit[ChgCd] = strs;
                    }
                }
            }
        }

        public static decimal GetBrokerMFee(string IsHoliday, string qHoliDay, string qProdType, string ProductType, decimal F3)
        {
            decimal TtlFee = 0;
            if (!string.IsNullOrEmpty(qProdType))
                qProdType = qProdType.ToUpper();
            if (!string.IsNullOrEmpty(ProductType))
                ProductType = ProductType.ToUpper();
            if (IsHoliday != "")
            {
                if (qHoliDay == "Y" && (qProdType == "" || qProdType == ProductType))
                {
                    TtlFee = F3;
                }

                if (qHoliDay == "" && (qProdType == "" || qProdType == ProductType))
                {
                    TtlFee = F3;
                }
            }
            else if (IsHoliday == "")
            {
                if (qHoliDay == "N" && (qProdType == "" || qProdType == ProductType))
                {
                    TtlFee = F3;
                }

                if (qHoliDay == "" && (qProdType == "" || qProdType == ProductType))
                {
                    TtlFee = F3;
                }
            }

            return TtlFee;
        }
        #endregion

        #region 將海運公共費寫入帳單
        public void SeaPublicFee2Smbid(DataRow smdr, DataRow qdr, List<string> Message)
        {
            try
            {
                string ShipmentId = Prolink.Math.GetValueAsString(smdr["SHIPMENT_ID"]);
                string TranType = Prolink.Math.GetValueAsString(smdr["TRAN_TYPE"]);
                decimal Cnt20 = Prolink.Math.GetValueAsDecimal(smdr["CNT20"]);
                decimal Cnt40 = Prolink.Math.GetValueAsDecimal(smdr["CNT40"]);
                string Cnt40OtherType = Prolink.Math.GetValueAsString(smdr["OCNT_TYPE"]);
                decimal Cnt40Other = Prolink.Math.GetValueAsDecimal(smdr["OCNT_NUMBER"]);
                decimal Cnt40Other2 = Prolink.Math.GetValueAsDecimal(smdr["CNT_NUMBER"]);
                decimal Cnt40hq = Prolink.Math.GetValueAsDecimal(smdr["CNT40HQ"]);
                decimal Cbm = Prolink.Math.GetValueAsDecimal(smdr["CBM"]);
                decimal Gw = Prolink.Math.GetValueAsDecimal(smdr["GW"]);
                string Gwu = Prolink.Math.GetValueAsString(smdr["GWU"]);
                string PkgUnit = Prolink.Math.GetValueAsString(smdr["PKG_UNIT"]);
                decimal PkgNum = Prolink.Math.GetValueAsDecimal(smdr["PKG_NUM"]);
                decimal Cw = Prolink.Math.GetValueAsDecimal(smdr["CW"]);
                string Punit = Prolink.Math.GetValueAsString(qdr["PUNIT"]);
                decimal F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);
                decimal Fee20 = Prolink.Math.GetValueAsDecimal(qdr["F4"]);
                decimal Fee40 = Prolink.Math.GetValueAsDecimal(qdr["F5"]);
                decimal Fee40hq = Prolink.Math.GetValueAsDecimal(qdr["F6"]);

                decimal amt = 0;
                decimal TtlFee = 0;

                int sum = 1;
                switch (Punit)
                {
                    case "20GP":
                        amt = Fee20;
                        TtlFee = Fee20 * Cnt20;
                        if (Fee20 == 0)
                        {
                            TtlFee = F3 * Cnt20;
                        }
                        sum = Prolink.Math.GetValueAsInt(Cnt20);
                        break;
                    case "40GP":
                        amt = Fee40;
                        TtlFee = Fee40 * Cnt40;
                        if (Fee40 == 0)
                        {
                            TtlFee = F3 * Cnt40;
                        }
                        sum = Prolink.Math.GetValueAsInt(Cnt40);
                        break;
                    case "40HQ":
                        amt = Fee40hq;
                        TtlFee = Fee40hq * Cnt40hq;
                        if (Fee40hq == 0)
                        {
                            TtlFee = F3 * Cnt40hq;
                        }
                        sum = Prolink.Math.GetValueAsInt(Cnt40hq);
                        break;
                    case "CBM":
                        TtlFee = F3 * Cbm;
                        sum = Prolink.Math.GetValueAsInt(Cbm);
                        break;
                    case "KGS":
                        if (Gwu == "G" || Gwu == "g")
                        {
                            TtlFee = F3 * (Gw / 1000);
                        }
                        else
                        {
                            TtlFee = F3 * Gw;
                        }
                        break;
                    case "SHT":
                        TtlFee = F3;
                        break;
                    case "BL":
                        TtlFee = F3;
                        break;
                    case "CTR":
                        TtlFee = SetCntr(smdr, qdr);
                        sum = Prolink.Math.GetValueAsInt(Cnt20 + Cnt40 + Cnt40hq + Cnt40Other + Cnt40Other2);
                        break;
                    case "PHK":
                        TtlFee = F3 * System.Math.Ceiling(Gw / 100);
                        break;
                    case "%":
                        string table = "SMIDN";
                        if (TranType == "F" || TranType == "R")
                        {
                            table = "SMICNTR";
                        }
                        string sql = string.Format("SELECT SUM(FOB_AMT) FROM {0} WHERE SHIPMENT_ID={1}", table, SQLUtils.QuotedStr(ShipmentId));
                        Decimal FobValue = Prolink.Math.GetValueAsDecimal(OperationUtils.GetValueAsFloat(sql, Prolink.Web.WebContext.GetInstance().GetConnection()));

                        TtlFee = F3 * (FobValue / 100);
                        break;
                    case "CW":
                        TtlFee = F3 * Cw;
                        break;
                    case "PLT":
                        if (PkgUnit.ToUpper() == Punit.ToUpper())
                            TtlFee = F3 * PkgNum;
                        break;
                    default:
                        if (Cnt40Other > 0)
                        {
                            string field = getfieldFromOtherMapping(Cnt40OtherType);
                            if (!string.IsNullOrEmpty(field))
                            {
                                Fee40hq = Prolink.Math.GetValueAsDecimal(qdr[field]);
                                TtlFee = Fee40hq * Cnt40Other;
                                sum = Prolink.Math.GetValueAsInt(Cnt40Other);
                            }
                        }
                        break;
                }
                string repay = Prolink.Math.GetValueAsString(qdr["REPAY"]);
                if ("CTR".Equals(Punit) && "M".Equals(repay))
                {
                    string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                    DataTable smicndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow idr in smicndt.Rows)
                    {
                        string cntrno = Prolink.Math.GetValueAsString(idr["CNTR_NO"]);
                        string cntrtype = Prolink.Math.GetValueAsString(idr["CNTR_TYPE"]);
                        sum = 1;
                        TtlFee = GetCntrFee(cntrtype, qdr);
                        SeaPublicFee2SmbidItem(smdr, qdr, Message, amt, TtlFee, sum, cntrno);
                    }
                }
                else
                {
                    SeaPublicFee2SmbidItem(smdr, qdr, Message, amt, TtlFee, sum);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void SeaPublicFee2SmbidItem(DataRow smdr, DataRow qdr, List<string> Message, decimal amt, decimal TtlFee, int sum, string CntrNo = "")
        {
            List<string> msg = new List<string>();
            string ShipmentId = Prolink.Math.GetValueAsString(smdr["SHIPMENT_ID"]);
            string TranType = Prolink.Math.GetValueAsString(smdr["TRAN_TYPE"]);
            string Location = Prolink.Math.GetValueAsString(smdr["CMP"]);
            string LspNo = Prolink.Math.GetValueAsString(smdr["PARTY_NO"]);
            string LspNm = Prolink.Math.GetValueAsString(smdr["PARTY_NAME"]);
            string CreditTo = Prolink.Math.GetValueAsString(qdr["CREDIT_TO"]);
            string CreditNm = Prolink.Math.GetValueAsString(qdr["CREDIT_NM"]);
            string ChgCd = Prolink.Math.GetValueAsString(qdr["CHG_CD"]);
            string ChgDescp = Prolink.Math.GetValueAsString(qdr["CHG_DESCP"]);
            string QuotNo = Prolink.Math.GetValueAsString(qdr["QUOT_NO"]);
            string Cur = Prolink.Math.GetValueAsString(qdr["CUR"]);
            string remark = Prolink.Math.GetValueAsString(qdr["REMARK"]);

            string Punit = Prolink.Math.GetValueAsString(qdr["PUNIT"]);
            decimal F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);
            decimal MinAmt = Prolink.Math.GetValueAsDecimal(qdr["MIN_AMT"]);
            decimal MaxAmt = Prolink.Math.GetValueAsDecimal(qdr["MAX_AMT"]);
            string batNo = Prolink.Math.GetValueAsString(smdr["BAT_NO"]);//运输批次号码

            MixedList mixList = new MixedList();
            if (amt == 0)
                amt = F3;
            Dictionary<string, List<string>> ChgRemarkUnit = new Dictionary<string, List<string>>();
            setRemarkUnit(ChgRemarkUnit, ChgCd, Punit, remark, amt, sum);

            if (MinAmt > TtlFee & MinAmt > 0)
            {
                TtlFee = MinAmt;
            }
            if (TtlFee > MaxAmt & MaxAmt > 0)
            {
                TtlFee = MaxAmt;
            }
            LspNo = string.IsNullOrEmpty(CreditTo) ? LspNo : CreditTo;
            LspNm = string.IsNullOrEmpty(CreditTo) ? LspNm : CreditNm;


            bool IsOk = CalCualteFeeHandle.ChkSmbid(ChgCd, ShipmentId, LspNo, null, CntrNo);
            //尚未開立帳單的才能寫入
            if (!IsOk)
            {
                msg.Add(ShipmentId + "," + LspNo + "," + ChgCd + ":已开立账单");
                Message.Add(ShipmentId + "," + LspNo + ":" + ChgDescp + "," + "It was a bill already");
            }
            if (IsOk == true)
            {
                msg.Add(ChgCd);
                msg.Add(Prolink.Math.GetValueAsString(TtlFee));
                List<string> RemarkUnit = new List<string>();
                if (ChgRemarkUnit.ContainsKey(ChgCd))
                    RemarkUnit = ChgRemarkUnit[ChgCd];
                string smbidUid = CalCualteFeeHandle.chkFeeExist(ChgCd, ShipmentId, LspNo, Location, null, CntrNo);
                if (smbidUid == "")
                {
                    EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                    string uid = System.Guid.NewGuid().ToString(); 
                    ei.Put("U_ID", uid);
                    ei.Put("SHIPMENT_ID", ShipmentId);
                    ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(smdr["GROUP_ID"]));
                    ei.Put("SEC_CMP", Prolink.Math.GetValueAsString(smdr["SEC_CMP"]));
                    ei.Put("CMP", Location);
                    ei.Put("STN", Prolink.Math.GetValueAsString(smdr["STN"])); 
                    ei.Put("QUOT_NO", QuotNo);
                    ei.Put("RFQ_NO", Prolink.Math.GetValueAsString(qdr["RFQ_NO"]));
                    ei.Put("QAMT", TtlFee); 
                    if (!string.IsNullOrEmpty(CntrNo))
                    {
                        ei.Put("DEC_NO", CntrNo);
                        ei.Put("CNTR_NO", CntrNo);
                    }

                    if (TranType.Equals("T") && !string.IsNullOrEmpty(batNo))//内贸 运输批次号码写入至Reference No
                    {
                        ei.Put("DEC_NO", batNo);
                    }

                    ei.Put("QTAX", 0);
                    ei.Put("CHG_CD", ChgCd);
                    ei.Put("CHG_DESCP", ChgDescp);
                    ei.Put("QCUR", Cur);
                    ei.Put("LSP_NO", LspNo);
                    ei.Put("LSP_NM", LspNm);
                    ei.Put("TRAN_TYPE", TranType);
                    ei.Put("CNTR_INFO", Prolink.Math.GetValueAsString(smdr["CNTR_INFO"]));
                    ei.Put("MASTER_NO", Prolink.Math.GetValueAsString(smdr["MASTER_NO"]));
                    ei.Put("BL_NO", Prolink.Math.GetValueAsString(smdr["MASTER_NO"]));
                    ei.Put("POD_CD", Prolink.Math.GetValueAsString(smdr["POD_CD"]));
                    SetCHGInfo(qdr, ei, smdr, RemarkUnit);
                    TPV.Financial.InboundBill.SetEstInfo(ei);
                    msg.Add("Add");
                    mixList.Add(ei);
                }
                else
                {
                    EditInstruct ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                    string uid = System.Guid.NewGuid().ToString(); 
                    ei.PutKey("U_ID", smbidUid);
                    ei.Put("QAMT", TtlFee);
                    ei.Put("QCUR", Cur);
                    UpdateSmbidQuotNo(ei, smbidUid, QuotNo, CntrNo);
                    if (TranType.Equals("T") && !string.IsNullOrEmpty(batNo))//内贸 运输批次号码写入至Reference No
                    {
                        ei.Put("DEC_NO", batNo);
                    } 
                    SetCHGInfo(qdr, ei, smdr);
                    TPV.Financial.InboundBill.SetEstInfo(ei);
                    msg.Add("Update");
                    mixList.Add(ei);
                }
            }
            if (mixList.Count > 0)
            {
                try
                {
                    CalculateLocalAmt(mixList, smdr);
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = new List<string>();
                    msg.Add(ex.ToString());
                }
            }
            Bill.WriteLog(string.Join(";", msg), ShipmentId);
        }

        public decimal GetCntrFee(string cntrtype, DataRow qdr)
        {
            decimal ttlfee = 0m;
            decimal F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);
            decimal Fee20 = Prolink.Math.GetValueAsDecimal(qdr["F4"]);
            decimal Fee40 = Prolink.Math.GetValueAsDecimal(qdr["F5"]);
            decimal Fee40hq = Prolink.Math.GetValueAsDecimal(qdr["F6"]);
            switch (cntrtype)
            {
                case "Cnt20":
                case "20GP":
                    ttlfee = Fee20;
                    break;
                case "Cnt40":
                case "40GP":
                    ttlfee = Fee40;
                    break;
                case "40HQ":
                    ttlfee = Fee40hq;
                    break;
                default:
                    string field = getfieldFromOtherMapping(cntrtype);
                    if (!string.IsNullOrEmpty(field))
                    {
                        ttlfee = Prolink.Math.GetValueAsDecimal(qdr[field]);//40HQ
                    }
                    break;

            }
            if (ttlfee <= 0)
                ttlfee = F3;
            return ttlfee;
        }

        public decimal SetCntr(DataRow smdr, DataRow qdr)
        {
            decimal Cnt20 = Prolink.Math.GetValueAsDecimal(smdr["CNT20"]);
            decimal Cnt40 = Prolink.Math.GetValueAsDecimal(smdr["CNT40"]);
            decimal Cnt40hq = Prolink.Math.GetValueAsDecimal(smdr["CNT40HQ"]);
            string Cnt40OtherType = Prolink.Math.GetValueAsString(smdr["OCNT_TYPE"]);
            decimal Cnt40Other = Prolink.Math.GetValueAsDecimal(smdr["OCNT_NUMBER"]);
            string Cnt40OtherType2 = Prolink.Math.GetValueAsString(smdr["CNT_TYPE"]);
            decimal Cnt40Other2 = Prolink.Math.GetValueAsDecimal(smdr["CNT_NUMBER"]);
            decimal F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);
            decimal Fee20 = Prolink.Math.GetValueAsDecimal(qdr["F4"]);
            decimal Fee40 = Prolink.Math.GetValueAsDecimal(qdr["F5"]);
            decimal Fee40hq = Prolink.Math.GetValueAsDecimal(qdr["F6"]);

            decimal TtlFee = (Fee20 * Cnt20) + (Fee40 * Cnt40) + (Fee40hq * Cnt40hq);
            if (Fee20 == 0 && Fee40 == 0 && Fee40hq == 0)
            {
                TtlFee = (F3 * Cnt20) + (F3 * Cnt40) + (F3 * Cnt40hq);
            }
            string field = getfieldFromOtherMapping(Cnt40OtherType);
            if (!string.IsNullOrEmpty(field))
            {
                decimal Ffiled = Prolink.Math.GetValueAsDecimal(qdr[field]);//40HQ
                TtlFee += Ffiled * Cnt40Other;
            }

            field = getfieldFromOtherMapping(Cnt40OtherType2);
            if (!string.IsNullOrEmpty(field))
            {
                decimal Ffiled = Prolink.Math.GetValueAsDecimal(qdr[field]);//40HQ
                TtlFee += Ffiled * Cnt40Other2;
            }

            return TtlFee;
        }
        #endregion

        #region 檢查tmp檔
        public static bool ChkTmpExist(string ShipmentId, string CntrNo, string ReserveNo, string ChgCd)
        {
            string sql = "SELECT COUNT(*) FROM TMP_AMT WHERE SHIPMENT_ID={0} AND (CNTR_NO={1} OR CNTR_NO IS NULL) AND RESERVE_NO={2} AND CHG_CD={3}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(CntrNo), SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(ChgCd));
            int n = CommonHelp.getOneValueAsIntFromSql(sql);

            if (n > 0)
            {
                return true;
            }

            return false;
        }
        #endregion


        public List<string> CheckBsTerm(string ShipmentId)
        {
            List<string> ChgType = null;
            string sql = string.Format("SELECT FRT_TERM, INCOTERM_CD, INCOTERM_DESCP, POD_NAME, CMP FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = CommonHelp.getDataTableFromSql(sql);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string FrtTerm = Prolink.Math.GetValueAsString(dr["FRT_TERM"]);
                    string IncotermCd = Prolink.Math.GetValueAsString(dr["INCOTERM_CD"]);
                    string IncotermDescp = Prolink.Math.GetValueAsString(dr["INCOTERM_DESCP"]).Trim();
                    string PodName = Prolink.Math.GetValueAsString(dr["POD_NAME"]);
                    string Location = Prolink.Math.GetValueAsString(dr["CMP"]);

                    if (string.IsNullOrEmpty(IncotermDescp))
                        sql =string.Format("SELECT NEED_CHG FROM BSTERM WHERE CMP={0} AND FRT_TERM={1} AND INCOTERM_CD={2} AND (INCOTERM_DESCP IS NULL OR INCOTERM_DESCP='') AND POD_NAME={3}",
                            SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(PodName));
                    else
                        sql =string.Format("SELECT NEED_CHG FROM BSTERM WHERE CMP={0} AND FRT_TERM={1} AND INCOTERM_CD={2} AND INCOTERM_DESCP LIKE '%{3}%' AND POD_NAME={4}",
                             SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(IncotermCd), IncotermDescp, SQLUtils.QuotedStr(PodName));
                    DataTable gdt = CommonHelp.getDataTableFromSql(sql);

                    if (gdt.Rows.Count > 0)
                    {
                        ChgType = new List<string>();
                        foreach (DataRow gdr in gdt.Rows)
                        {
                            string NeedChg = Prolink.Math.GetValueAsString(gdr["NEED_CHG"]);
                            if (!string.IsNullOrEmpty(NeedChg))
                            {
                                NeedChg = NeedChg.Replace(" ", "");
                                ChgType = NeedChg.Split(',').ToList();
                                return ChgType;
                            }
                        }
                    }
                }
            }
            return ChgType;
        }

        #region 本地金额重新计算

        private static void CalculateLocalAmt(MixedList ml, DataRow smsmi = null, string groupId = "", string cmp = "")
        {
            CalCualteFeeHandle.CalculateLocalAmt(ml, smsmi, groupId, cmp);
        }
        #endregion

        /// <summary>
        /// 将报价中的费用信息设置到账单明细中
        /// </summary>
        /// <param name="chg"></param>
        /// <param name="ei"></param>
        private static void SetCHGInfo(DataRow chg, EditInstruct ei, DataRow smsmi = null, List<string> RemarkUnit = null)
        {
            CalCualteFeeHandle.SetCHGInfo(chg, ei, smsmi, RemarkUnit);
        }

        public static DataTable CheckItemIpart(string ChgCd, string ShipmentId, string LspNo)
        {
            string sql = string.Format("SELECT STATUS,U_ID FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND LSP_NO={2}", SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));
            DataTable dt = CommonHelp.getDataTableFromSql(sql);
            return dt;
        }

        public void CalOtherLocal(DataRow smdr, DataRow[] smqtDrs, string ShipmentId, List<string> emptyMessage)
        {
            MixedList mixList = new MixedList();
            List<string> msg = new List<string>();
            decimal Amount = 0M;
            string uid = string.Empty;
            if (smqtDrs.Length <= 0) return;
            string QuotNo = Prolink.Math.GetValueAsString(smqtDrs[0]["QUOT_NO"]);
            Bill.WriteLog(string.Format("Local费用报价:{0},BHC計價", QuotNo), ShipmentId);
            foreach (DataRow smqtDr in smqtDrs)
            {
                string lsp_cd = Prolink.Math.GetValueAsString(smqtDr["LSP_CD"]);
                string rfq_no = Prolink.Math.GetValueAsString(smqtDr["RFQ_NO"]);
                string ChgCd = Prolink.Math.GetValueAsString(smqtDr["CHG_CD"]);
                string ChgDescp = Prolink.Math.GetValueAsString(smqtDr["CHG_DESCP"]);
                string Rlocation = Prolink.Math.GetValueAsString(smdr["CMP"]);
                string Cur = Prolink.Math.GetValueAsString(smqtDr["CUR"]);
                string Punit = Prolink.Math.GetValueAsString(smqtDr["PUNIT"]);
                string sql = string.Empty;

                DateTime podDate = GetStandardDate(DateTime.Now);
                DateTime colseDate = GetStandardDate(DateTime.Now);

                Dictionary<string, string> map = new Dictionary<string, string>();
                DataTable ecreffeeoDt = CommonHelp.getDataTableFromSql(string.Format("SELECT CHG_CD,CHG_DESCP,GW,CBM,REMARK,FEE_WEIGHT,FEE_OP,CAL_TYPE FROM ECREFFEE WHERE FEE_TYPE='I' AND CMP={0}  ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(Rlocation)));
                foreach (DataRow dr in ecreffeeoDt.Rows)
                {
                    string chg_cd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                    string inch = Prolink.Math.GetValueAsString(dr["CHG_DESCP"]);
                    if (inch.Contains("."))
                    {
                        inch = inch.TrimEnd(new char[] { '0', ' ' });
                        inch = inch.TrimEnd(new char[] { '.', ' ' });
                    }
                    inch = inch.TrimEnd(new char[] { '\'' });
                    map[inch] = chg_cd;
                }

                Regex regex = new Regex("(?<inch>[0-9.]+?)[\"|吋]", RegexOptions.IgnoreCase);
                string status = string.Empty;

                DataTable dt = CheckItemIpart(ChgCd, ShipmentId, lsp_cd);
                if (dt.Rows.Count > 0)
                {
                    status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
                    uid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
                }
                switch (status)
                {
                    case "Y":
                        emptyMessage.Add(ShipmentId + "," + lsp_cd + ":Handling Fee." + "It was a bill already");
                        continue;
                }
                switch (ChgCd)
                {
                    //case "ITEM":
                    case "BHC":
                        DataTable cdt = CommonHelp.getDataTableFromSql("SELECT * FROM SMIDNP WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId));
                        foreach (DataRow cdr in cdt.Rows)
                        {
                            string ipartNo = Prolink.Math.GetValueAsString(cdr["IPART_NO"]);
                            string goodsDescp = Prolink.Math.GetValueAsString(cdr["GOODS_DESCP"]);
                            string prodDescp = Prolink.Math.GetValueAsString(cdr["PROD_DESCP"]);

                            string inch = GetInch(regex, goodsDescp);
                            if (string.IsNullOrEmpty(inch))
                                continue;
                            string inch_cd = map.ContainsKey(inch) ? map[inch] : string.Empty;
                            decimal price = (!string.IsNullOrEmpty(inch_cd) && smqtDr.Table.Columns.Contains(inch_cd)) ? Prolink.Math.GetValueAsDecimal(smqtDr[inch_cd]) : 0M;
                            int Qty = Prolink.Math.GetValueAsInt(cdr["QTY"]);
                            Amount += price * Qty;
                        }
                        break;
                        //case "WSF":
                        //    DateTime start_date = podDate.AddDays(30);
                        //    if (start_date.Day != 1)
                        //    {
                        //        start_date = start_date.AddMonths(1);
                        //        start_date = new DateTime(start_date.Year, start_date.Month, 1, 0, 0, 0);
                        //    }
                        //    TimeSpan sp = colseDate.Subtract(start_date);
                        //    int day = sp.Days;
                        //    if (day <= 0)
                        //        return;
                        //    break;
                }
            }
            AddSMBIDByItem(smdr, smqtDrs[0], ShipmentId, mixList, Amount, uid, QuotNo, msg);
            if (mixList.Count > 0)
            {
                try
                {
                    CalculateLocalAmt(mixList, smdr);
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = new List<string>();
                    msg.Add(ex.ToString());
                }
            }
            Bill.WriteLog(string.Join(";", msg), ShipmentId);
        }

        private static DateTime GetStandardDate(DateTime date)
        {
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            return date;
        }

        private static string GetInch(Regex regex, string goodsDescp)
        {
            string inch = string.Empty;
            MatchCollection ms = regex.Matches(goodsDescp);
            if (ms == null || ms.Count <= 0)
                return inch;
            inch = ms[0].Groups["inch"].Value;
            if (string.IsNullOrEmpty(inch))
                return inch;
            if (inch.Contains("."))
            {
                inch = inch.TrimEnd(new char[] { '0', ' ' });
                inch = inch.TrimEnd(new char[] { '.', ' ' });
            }
            return inch;
        }

        private static void AddSMBIDByItem(DataRow smdr, DataRow smqtDr, string ShipmentId, MixedList mixList, Decimal amt, string uid, string QuotNo, List<string> msg)
        {
            List<string> RemarkUnit = new List<string>();
            RemarkUnit.Add(Prolink.Math.GetValueAsString(smqtDr["REMARK"]));
            RemarkUnit.Add(Prolink.Math.GetValueAsString(smqtDr["PUNIT"]));
            string chgCd = Prolink.Math.GetValueAsString(smqtDr["CHG_CD"]);
            string LspNo = Prolink.Math.GetValueAsString(smqtDr["LSP_CD"]);
            string LspNm = Prolink.Math.GetValueAsString(smqtDr["LSP_NM"]);
            string CreditTo = Prolink.Math.GetValueAsString(smqtDr["CREDIT_TO"]);
            string CreditNm = Prolink.Math.GetValueAsString(smqtDr["CREDIT_NM"]);
            msg.Add(chgCd); msg.Add(Prolink.Math.GetValueAsString(amt));
            if (string.IsNullOrEmpty(uid))
            {
                EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                uid = System.Guid.NewGuid().ToString();
                ei.Put("U_ID", uid);
                ei.Put("SHIPMENT_ID", ShipmentId);
                ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(smdr["GROUP_ID"]));
                ei.Put("SEC_CMP", Prolink.Math.GetValueAsString(smdr["SEC_CMP"]));
                ei.Put("CMP", Prolink.Math.GetValueAsString(smdr["CMP"]));
                ei.Put("STN", Prolink.Math.GetValueAsString(smdr["STN"]));
                //ei.Put("DEP", Prolink.Math.GetValueAsString(smsmi.Rows[0]["DEP"]));
                ei.Put("QUOT_NO", smqtDr["QUOT_NO"]);
                ei.Put("RFQ_NO", smqtDr["QUOT_NO"]);

                //ei.Put("QUNIT_PRICE", price);
                ei.Put("QAMT", amt);
                ei.Put("QCUR", smqtDr["CUR"]);
                //ei.Put("IPART_NO", ipartNo);

                ei.Put("QTAX", 0);
                ei.Put("CHG_CD", chgCd);
                ei.Put("CHG_DESCP", smqtDr["CHG_DESCP"]);
                ei.Put("LSP_NO", string.IsNullOrEmpty(CreditTo) ? LspNo : CreditTo);
                ei.Put("LSP_NM", string.IsNullOrEmpty(CreditTo) ? LspNm : CreditNm);
                ei.Put("TRAN_TYPE", smqtDr["TRAN_TYPE"]);
                ei.Put("CNTR_INFO", Prolink.Math.GetValueAsString(smdr["CNTR_INFO"]));
                ei.Put("MASTER_NO", Prolink.Math.GetValueAsString(smdr["MASTER_NO"]));
                ei.Put("BL_NO", Prolink.Math.GetValueAsString(smdr["MASTER_NO"]));
                ei.Put("POD_CD", Prolink.Math.GetValueAsString(smdr["POD_CD"]));
                SetCHGInfo(smqtDr, ei, smdr, RemarkUnit);
                TPV.Financial.InboundBill.SetEstInfo(ei);
                msg.Add("Add");
                mixList.Add(ei);
            }
            else
            {
                EditInstruct ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                //ei.Put("QUNIT_PRICE", price);
                ei.Put("QAMT", amt);
                ei.Put("QCUR", smqtDr["CUR"]);
                UpdateSmbidQuotNo(ei, uid, Prolink.Math.GetValueAsString(smqtDr["QUOT_NO"]), "");
                msg.Add("Update");
                SetCHGInfo(smqtDr, ei, smdr);
                TPV.Financial.InboundBill.SetEstInfo(ei);
                mixList.Add(ei);
            }
        }
        public void CalFreightCalculat(string shipmentid)
        { 
            if (ChgType == null || !ChgType.Contains("FC"))
            { 
                return;
            }
            InboundBill ib = new InboundBill();
            ib.Share(shipmentid);
        }
        /// <summary>
        /// 183092 手动上传可能存在QUOT NO为空，需更新
        /// </summary>
        /// <param name="ei"></param>
        /// <param name="smbidUid"></param>
        /// <param name="QuotNo">quotation No</param>
        /// <param name="decNo">Reference no</param>
        private static void UpdateSmbidQuotNo(EditInstruct ei, string smbidUid, string QuotNo, string decNo)
        {
            string smbidSql = string.Format("SELECT QUOT_NO,DEC_NO FROM SMBID WHERE U_ID={0} ", SQLUtils.QuotedStr(smbidUid));
            DataTable dt = CommonHelp.getDataTableFromSql(smbidSql);
            if (dt.Rows.Count > 0)
            {
                if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_NO"])) && !string.IsNullOrEmpty(QuotNo))
                {
                    ei.Put("QUOT_NO", QuotNo);
                }
                if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dt.Rows[0]["DEC_NO"])) && !string.IsNullOrEmpty(decNo))
                {
                    ei.Put("DEC_NO", decNo);
                }
            }
        }
    }

    internal class CommonHelp
    {
        public static string getOneValueAsStringFromSql(string sql)
        {
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static int getOneValueAsIntFromSql(string sql)
        {
            return OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static double getOneValueAsDoubleFromSql(string sql)
        {
            return OperationUtils.GetValueAsFloat(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static DataTable getDataTableFromSql(string sql)
        {
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        public static void exeSql(string sql)
        {
            if (sql != "")
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }
    }
}
