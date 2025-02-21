using Business.TPV.Utils;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Utils;
using static TrackingEDI.InboundBusiness.InboundHelper; 

namespace Business.TPV.Financial
{
    public class CalCualteFeeHandle
    {
        /// <summary>
        /// Calendar Day
        /// </summary>
        public static string CalendarDay = "C";
        /// <summary>
        /// Working Day
        /// </summary>
        public static string WorkingDay = "W";

        public static void LocalStorageDemurageDetention(string ShipmentId, DataTable dt, DataTable mdt, List<string> emptyMessage, bool ChcekFlag, string headOffice)
        {
            string Location = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
            string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
            DateTime etd = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETD"]);
            DateTime eta = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETA"]);
            DateTime atd = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ATD"]);
            DateTime ata = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ATA"]);
            string LspNo = GetForwarderNo(ShipmentId);
            DateTime depDate = atd > DateTime.MinValue ? atd : etd;
            DateTime arvDate = ata > DateTime.MinValue ? ata : eta;

            DataTable bsDateDt = DayHelper.GetBsdate(Location);

            Func<DataTable, DateTime, DateTime, DateTime, DateTime, Tuple<string, int, int,string>> getFreeTime = (qdt, dischargeDate, emptyTime, pickDate, empPickDate) =>
            {
                DataRow[] freeQdr = qdt.Select(string.Format("FEE_PER_DAY=0"), "S_DAY DESC");
                int freeTime = 0;
                string combineDet = "N";
                int days = 0;
                string chgDayType = "";
                if (freeQdr.Length > 0)
                {
                    freeTime = Prolink.Math.GetValueAsInt(freeQdr[0]["E_DAY"]);
                    string iType = Prolink.Math.GetValueAsString(freeQdr[0]["I_TYPE"]);
                        chgDayType = Prolink.Math.GetValueAsString(freeQdr[0]["CHG_DAY_TYPE"]);

                    Func<DateTime, DateTime, int> getDay = (to, from) =>
                    {
                        if (string.IsNullOrEmpty(chgDayType) || chgDayType.Equals(CalendarDay))
                        {
                            days = (to - from).Days;
                        }
                        else
                        {
                            days = DayHelper.GetWorkAndHolidays(from, to, bsDateDt);
                        }
                        return days;
                    };

                    switch (iType)
                    {
                        case "BOTH":
                            combineDet = "Y";
                            if (emptyTime.Year != 1 && dischargeDate.Year != 1)
                                days = getDay(emptyTime, dischargeDate);
                            break;
                        case "USAGE":
                            combineDet = "U";
                            if (emptyTime.Year != 1 && empPickDate.Year != 1)
                                days = getDay(emptyTime, empPickDate);
                            break;
                        case "DDEM":
                            if (pickDate.Year != 1 && dischargeDate.Year != 1)
                                days = getDay(pickDate, dischargeDate);
                            break;
                        case "DDET":
                            if (emptyTime.Year != 1 && pickDate.Year != 1)
                                days = getDay(emptyTime, pickDate);
                            break;
                        case "DSTF":
                            if (pickDate.Year != 1 && dischargeDate.Year != 1)
                                days = getDay(pickDate, dischargeDate);
                            break;
                    }
                }
                return new Tuple<string, int, int,string>(combineDet, freeTime, days, chgDayType);
            }; 

            string StorageCur = string.Empty, DemurageCur = string.Empty, DetentionCur = string.Empty;

            decimal StorageAmt = 0, DemurageAmt = 0, DetentionAmt = 0;

            string QuotNo = Prolink.Math.GetValueAsString(mdt.Rows[0]["QUOT_NO"]);
            string sql = string.Format("SELECT CHG_CD,SP_CD FROM SMQTD WHERE CHG_CD IN ('DSTF','DDEM','DDET') AND QUOT_NO={0} AND REPAY='A'", SQLUtils.QuotedStr(QuotNo));
            List<string> chgList = new List<string>();
            DataTable qtdDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataRow[] qtdDtRows = CalculateFee.GetSmqtdRows(ChcekFlag, qtdDt, headOffice);
            if (qtdDtRows == null) return; 
            foreach (DataRow dr in qtdDtRows)
            {
                string chgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                string spCd = Prolink.Math.GetValueAsString(dr["SP_CD"]); 
                if (chgList.Contains(chgCd))
                    continue;
                chgList.Add(chgCd);
            }
            if (chgList.Count <= 0)
                return;
            MixedList ml = new MixedList();
            if (TranType == "F" || TranType == "R")
            {
                sql = "SELECT * FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            }
            else
            {
                sql = "SELECT TOP 1 *,NULL AS CNTR_TYPE, NULL AS EMPTY_TIME,NULL AS EMP_PICK_DATE FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " ORDER BY PICKUP_CDATE DESC";
            }

            DataTable detailDt = CommonHelp.getDataTableFromSql(sql);
            if (detailDt.Rows.Count <= 0) return;
            List<PortFeeDate> portFreeDateList = new List<PortFeeDate>();
            
            Tuple<string, int, int, int, string, string, string,Tuple<string>> portFreeItem = new Tuple<string, int, int, int, string, string, string, Tuple<string>>("", 0, 0, 0, "", "", "", new Tuple<string>("")); 
 

            if ("F".Equals(TranType) || "R".Equals(TranType))
            {
                portFreeItem = TrackingEDI.InboundBusiness.InboundHelper.getFreeTime(LspNo,PodCd, Location, Carrier, TranType, arvDate,depDate);
            }
            PortFeeDate shipmentFreeDate = new PortFeeDate(ShipmentId, "");
            shipmentFreeDate.PortFreeTime = portFreeItem.Item4;
            shipmentFreeDate.PortChgDayType = portFreeItem.Rest.Item1;
            shipmentFreeDate.FactFreeTime = portFreeItem.Item3;
            shipmentFreeDate.FactChgDayType = portFreeItem.Item7;
            shipmentFreeDate.ConFreeTime = portFreeItem.Item2;
            shipmentFreeDate.ConChgDayType = portFreeItem.Item6;
            shipmentFreeDate.CombineDet = portFreeItem.Item1;
            shipmentFreeDate.ShowCombineDet = portFreeItem.Item5;
            int portFreeTime = 0, factFreeTime = 0, conFreeTime = 0;
                string portChgType = "", factChgType = "", conChgType = "";
            List<string> ListAmt = new List<string>();
            foreach (DataRow ddr in detailDt.Rows)
            {
                string isCombineDet = "";
                decimal FobAmt = Prolink.Math.GetValueAsDecimal(ddr["FOB_AMT"]);
                decimal CifAmt = Prolink.Math.GetValueAsDecimal(ddr["CIF_AMT"]);

                DateTime EmptyTime = Prolink.Math.GetValueAsDateTime(ddr["EMPTY_TIME"]);
                DateTime PickDate = Prolink.Math.GetValueAsDateTime(ddr["PICKUP_CDATE"]);
                DateTime DischargeDate = Prolink.Math.GetValueAsDateTime(ddr["DISCHARGE_DATE"]);
                DateTime empPickDate = Prolink.Math.GetValueAsDateTime(ddr["EMP_PICK_DATE"]);
                string CntrType = Prolink.Math.GetValueAsString(ddr["CNTR_TYPE"]);
                CntrType = GetCntrType(Location, CntrType);
                string cntrno = Prolink.Math.GetValueAsString(ddr["CNTR_NO"]);
                PortFeeDate portFeeDate = new PortFeeDate(ShipmentId, cntrno);
                portFeeDate.DischargeDate = DischargeDate;
                portFeeDate.EmpPickDate = empPickDate;
                portFeeDate.PickDate = PickDate;
                portFeeDate.Eta = eta;
                //DataTable qdt = GetQtiDataTable(dt, CntrType, LspNo);
                DataTable qdt1 = GetQtiStorageDataTable(dt, CntrType, LspNo);
                DataTable demTable = new DataTable(), detTable = new DataTable();
                int days = 0;
                if (qdt1.Rows.Count > 0)
                {
                    var freeTime = getFreeTime(qdt1, DischargeDate, EmptyTime, PickDate, empPickDate);
                    factFreeTime = freeTime.Item2;
                    factChgType = freeTime.Item4;
                    days = freeTime.Item3;
                    #region Storage(使用qdt1)
                    if (days > 0)
                    {
                        var storageItem = GetAmt(qdt1, ddr, TranType, days, ref ListAmt);
                        StorageAmt = storageItem.Item2;
                        StorageCur = storageItem.Item1;
                    }
                    #endregion
                }
                if (TranType == "F" || TranType == "R")
                {
                    demTable = GetQtiDataTable(dt, CntrType, LspNo, new List<string>() { "BOTH", "USAGE", "DDEM" });
                    days = 0;
                    isCombineDet = "N";
                    if (demTable.Rows.Count > 0)
                    {
                        var freeTime = getFreeTime(demTable, DischargeDate, EmptyTime, PickDate, empPickDate);
                        portFreeTime = freeTime.Item2;
                            portChgType = freeTime.Item4;
                        isCombineDet = freeTime.Item1;
                        days = freeTime.Item3;
                    }
                    if (days > 0)
                    {
                        var demurageItem = GetAmt(demTable, ddr, TranType, days, ref ListAmt);
                        DemurageAmt = demurageItem.Item2;
                        DemurageCur = demurageItem.Item1;
                    }
                    portFeeDate.CombineDet += isCombineDet;
                    detTable = GetQtiDataTable(dt, CntrType, LspNo, new List<string>() { "BOTH", "DDET" });
                    days = 0;
                    isCombineDet = "N";
                    if (detTable.Rows.Count > 0)
                    {
                        var freeTime = getFreeTime(detTable, DischargeDate, EmptyTime, PickDate, empPickDate);
                        conFreeTime = freeTime.Item2;
                        conChgType = freeTime.Item4;
                        isCombineDet = freeTime.Item1;
                        days = freeTime.Item3;
                    }

                    if (days > 0)
                    {
                        var detentionItem = GetAmt(detTable, ddr, TranType, days, ref ListAmt);
                        DetentionAmt = detentionItem.Item2;
                        DetentionCur = detentionItem.Item1;
                    }
                    portFeeDate.CombineDet += isCombineDet;
                    portFeeDate.ShowCombineDet = isCombineDet;

                    portFeeDate.FactFreeTime = factFreeTime;
                        portFeeDate.FactChgDayType = factChgType;
                    portFeeDate.PortFreeTime = portFreeTime;
                        portFeeDate.PortChgDayType = portChgType;
                    portFeeDate.ConFreeTime = conFreeTime;
                        portFeeDate.ConChgDayType = conChgType;
                    if (StorageAmt > 0 && chgList.Contains("DSTF"))
                    {
                        portFeeDate.StoUp = CalCualteFeeHandle.SetAtCostFee(ShipmentId, dt, emptyMessage, StorageCur, ref StorageAmt, QuotNo, "DSTF", null, cntrno);
                        portFeeDate.StoAmt = StorageAmt;
                        portFeeDate.StoCur = StorageCur;
                    }

                    if (portFeeDate.ShowCombineDet == "Y")
                    {
                        portFeeDate.DemUp = DEMtype.empty;
                    }
                    else if (DemurageAmt > 0 && chgList.Contains("DDEM"))
                    {
                        portFeeDate.DemUp = CalCualteFeeHandle.SetAtCostFee(ShipmentId, dt, emptyMessage, DemurageCur, ref DemurageAmt, QuotNo, "DDEM", null, cntrno);
                        portFeeDate.DemAmt = DemurageAmt;
                        portFeeDate.DemCur = DemurageCur;
                    }

                    if (DetentionAmt > 0 && chgList.Contains("DDET"))
                    {
                        portFeeDate.DetUp = CalCualteFeeHandle.SetAtCostFee(ShipmentId, dt, emptyMessage, DetentionCur, ref DetentionAmt, QuotNo, "DDET", null, cntrno);
                        portFeeDate.DetAmt = DetentionAmt;
                        portFeeDate.DetCur = DetentionCur;
                    }

                    StorageAmt = 0; DetentionAmt = 0; DemurageAmt = 0;
                    //UpdateSmordAmt(portFeeDate, ml);
                }



                if ((TranType == "F" || TranType == "R" || TranType == "A") &&
                    qdt1.Rows.Count <= 0 && demTable.Rows.Count <= 0 && detTable.Rows.Count <= 0)
                    emptyMessage.Add(ShipmentId + "," + QuotNo + ":storage,Demurage,Detention cann't find port fee date");

                if (TranType == "F" || TranType == "R")
                    portFreeDateList.Add(portFeeDate);

                if (StorageAmt > 0 && chgList.Contains("DSTF"))
                {
                    CalCualteFeeHandle.SetAtCostFee(ShipmentId, dt, emptyMessage, StorageCur, ref StorageAmt, QuotNo, "DSTF", ListAmt);
                    StorageAmt = 0;
                }
            }
          
            UpdateFreeDueDate(portFreeDateList, shipmentFreeDate, bsDateDt);
        }

        public static void GetLspSmqti(string sql, string lspCon, ref DataTable qtiDt, string LspNo, string TranType)
        {
            if (qtiDt.Rows.Count > 0 || string.IsNullOrEmpty(LspNo) || TranType != "F") return; 
            sql = sql.Replace(lspCon, string.Format(" AND LSP_NO={0}", SQLUtils.QuotedStr(LspNo)));
            qtiDt = CommonHelp.getDataTableFromSql(sql); 
        }

        public static DataTable GetQtiDataTable(DataTable dt, string CntrType,string LspNo, List<string> iTypeList)
        {
            string Location = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
            string TerminalCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TERMINAL_CD"]);
            string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
            DateTime etd = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETD"]);
            DateTime eta = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETA"]);
            DateTime atd = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ATD"]);
            DateTime ata = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ATA"]);
            string condition = string.Format(" CMP={0} AND POD_CD={1} AND TRAN_TYPE={2}",
                SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(TranType));
            if (TranType == "F" || TranType == "R")
                condition += string.Format(" AND {0}", SQLUtils.Like("CNT_TYPE", CntrType));
            string lspCon = " AND (LSP_NO IS NULL OR LSP_NO='')";
            if (!string.IsNullOrEmpty(LspNo))
            {
                if (TranType == "F")
                    lspCon = string.Format("  AND LSP_NO=(SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={0} AND STATUS='U')", SQLUtils.QuotedStr(LspNo));
                else
                    lspCon = string.Format(" AND LSP_NO={0}", SQLUtils.QuotedStr(LspNo));
            }
            condition += lspCon;
            string sql = string.Format("SELECT * FROM SMQTI WHERE {0} AND I_TYPE IN {1}", condition, SQLUtils.Quoted(iTypeList.ToArray()));
            DataTable qtiDt = CommonHelp.getDataTableFromSql(sql);
            GetLspSmqti(sql, lspCon, ref qtiDt, LspNo, TranType);
            DataRow[]  qtiDrs = qtiDt.Select(string.Format("CARRIER_CD={0}", SQLUtils.QuotedStr(Carrier)));
            if(qtiDrs.Length<=0)
                qtiDrs = qtiDt.Select("CARRIER_CD IS NULL");
            DataTable copyDt = qtiDrs.Length > 0 ? qtiDrs.CopyToDataTable() : new DataTable();
            if (copyDt.Rows.Count > 0)
            {
                condition = " TERMINAL_CD=" + SQLUtils.QuotedStr(TerminalCd);
                qtiDrs = copyDt.Select(condition);
                copyDt = qtiDrs.Length > 0 ? qtiDrs.CopyToDataTable() : copyDt;
            }
            if(copyDt.Rows.Count<=0)
                return copyDt;
            return GetQtiDataTableByDate(copyDt, ata > DateTime.MinValue ? ata : eta, atd > DateTime.MinValue ? atd : etd);
        }

        public static DataTable GetQtiStorageDataTable(DataTable dt, string CntrType, string LspNo)
        {
            string Location = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
            string TerminalCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TERMINAL_CD"]);
            string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
            DateTime etd = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETD"]);
            DateTime eta = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETA"]);
            DateTime atd = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ATD"]);
            DateTime ata = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ATA"]);
            string basecondition = string.Format(" CMP={0} AND POD_CD={1} AND TRAN_TYPE={2}", 
                SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(TranType));
            string lspCon = " AND (LSP_NO IS NULL OR LSP_NO='')";
            if (!string.IsNullOrEmpty(LspNo))
            {
                if (TranType == "F") 
                    lspCon = string.Format("  AND LSP_NO=(SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={0} AND STATUS='U')", SQLUtils.QuotedStr(LspNo)); 
                else 
                    lspCon = string.Format(" AND LSP_NO={0}", SQLUtils.QuotedStr(LspNo)); 
            } 
            basecondition += lspCon;
            string sql = string.Format("SELECT * FROM SMQTI WHERE {0} AND I_TYPE='DSTF'", basecondition);
            DataTable qtiDt = CommonHelp.getDataTableFromSql(sql);
            GetLspSmqti(sql, lspCon, ref qtiDt, LspNo, TranType);
            basecondition = " TERMINAL_CD=" + SQLUtils.QuotedStr(TerminalCd);
            DataRow[] qtiDrs = qtiDt.Select(basecondition);
            if(TranType != "F" && TranType != "R")
                return qtiDrs.Length > 0 ? qtiDrs.CopyToDataTable() : qtiDt;
            if (qtiDrs.Length <= 0)
                basecondition = " TERMINAL_CD IS NULL ";
            string condition = basecondition + string.Format(" AND CARRIER_CD={0} AND {1}", SQLUtils.QuotedStr(Carrier), SQLUtils.Like("CNT_TYPE", CntrType));
            qtiDrs = qtiDt.Select(condition);
            if (qtiDrs.Length <= 0)
            {
                condition = basecondition + string.Format(" AND CARRIER_CD IS NULL AND {0}", SQLUtils.Like("CNT_TYPE", CntrType));
                qtiDrs = qtiDt.Select(condition);
            }
            if (qtiDrs.Length <= 0)
            {
                condition = basecondition + string.Format(" AND CARRIER_CD IS NULL AND CNT_TYPE IS NULL ");
                qtiDrs = qtiDt.Select(condition);
            }
            DataTable copyDt = qtiDrs.Length > 0 ? qtiDrs.CopyToDataTable() : new DataTable();
            if ("A".Equals(TranType)|| copyDt.Rows.Count <= 0)
                return copyDt;
            return GetQtiDataTableByDate(copyDt, ata > DateTime.MinValue ? ata : eta, atd > DateTime.MinValue ? atd : etd);
        }

        public static DataTable GetQtiDataTableByDate(DataTable qtiDt ,DateTime arvDate,DateTime depDate) { 
            Func<string, DateTime,DataTable> getQtiDataTable = (calDate,date) => {
                string condition = string.Format("CAL_DATE={0} AND EFFECT_DATE<={1} AND EXPIRAT_DATE>={1}",
 SQLUtils.QuotedStr(calDate), SQLUtils.QuotedStr(date.ToString("yyyy-MM-dd")));
                DataRow[] drs = qtiDt.Select(condition, "EFFECT_DATE DESC,S_DAY ASC");
                if(drs.Length>0)
                    return drs.CopyToDataTable();
                return new DataTable();
            };
            DataTable dt = getQtiDataTable("A", arvDate);
            if (dt.Rows.Count <= 0)
                dt = getQtiDataTable("D", depDate);
            DataTable cloneDt= dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                if (cloneDt.Rows.Count <= 0)
                    cloneDt.ImportRow(dr);
                int Sday = Prolink.Math.GetValueAsInt(dr["S_DAY"]);
                if (Sday == 0)
                    continue;
                DateTime cloneEffectDate = Prolink.Math.GetValueAsDateTime(cloneDt.Rows[0]["EFFECT_DATE"]);
                DateTime effectDate = Prolink.Math.GetValueAsDateTime(dr["EFFECT_DATE"]);
                DateTime cloneExpiratDate = Prolink.Math.GetValueAsDateTime(cloneDt.Rows[0]["EXPIRAT_DATE"]);
                DateTime expiratDate = Prolink.Math.GetValueAsDateTime(dr["EXPIRAT_DATE"]);
                if (cloneEffectDate == effectDate && cloneExpiratDate == expiratDate)
                    cloneDt.ImportRow(dr);
            }
            return cloneDt;
        }

        public static Tuple<string, decimal, bool> GetAmt(DataTable qdt, DataRow ddr, string TranType, int Day, ref List<string> ListAmt)
        {
            decimal amt = 0;
            string cur = "";
            decimal FobAmt = Prolink.Math.GetValueAsDecimal(ddr["FOB_AMT"]);
            decimal CifAmt = Prolink.Math.GetValueAsDecimal(ddr["CIF_AMT"]);
            int CalTypeSday = 0;
            DataRow[] freeQdr = qdt.Select(" I_TYPE='DSTF' AND FEE_PER_DAY=0 AND S_DAY <=" + Day, "S_DAY DESC");
            if (freeQdr.Length > 0)
            {
                CalTypeSday = Prolink.Math.GetValueAsInt(freeQdr[0]["E_DAY"]);
            }
            CalTypeSday = Day - CalTypeSday;
            DataRow[] qdr = qdt.Select(" S_DAY <=" + Day, "S_DAY DESC");  
            for (int i = 0; i < qdr.Length; i++)
            {
                int Sday = Prolink.Math.GetValueAsInt(qdr[i]["S_DAY"]);
                decimal FeePerDay = Prolink.Math.GetValueAsDecimal(qdr[i]["FEE_PER_DAY"]);
                string CalType = Prolink.Math.GetValueAsString(qdr[i]["CAL_TYPE"]);
                decimal Percentage = Prolink.Math.GetValueAsDecimal(qdr[i]["PERCENTAGE"]);
                string FobCif = Prolink.Math.GetValueAsString(qdr[i]["FOB_CIF"]);
                decimal TermAmt = 0;
                Percentage = Percentage == 0 ? 100 : Percentage;

                if (i == 0)
                {
                    cur = Prolink.Math.GetValueAsString(qdr[i]["CUR"]);
                }
                switch (FobCif)
                {
                    case "F":
                        TermAmt = FobAmt;break;
                    case "C":
                        TermAmt = CifAmt;break;
                    case "B":
                        TermAmt = FobAmt + CifAmt;break;
                }
                if (FeePerDay == 0 && TermAmt == 0)
                    break;

                int tmpDay = Day - Sday + 1;
                Day = Day - tmpDay;
                decimal tempAmt = FeePerDay == 0 ? TermAmt : FeePerDay;
                if (CalType == "S")
                {
                    var val = tempAmt * System.Math.Round(Percentage / 100, 2);
                    amt = val * CalTypeSday;
                    ListAmt.Add(CalTypeSday + "#" + val);
                    break;
                }
                if (CalType == "C")
                {
                    tempAmt = tmpDay * tempAmt;
                    if (FeePerDay != 0)
                    {
                        var val = FeePerDay * System.Math.Round(Percentage / 100, 2); 
                        ListAmt.Add(tmpDay + "#" + val);
                    }
                }
                amt = amt + (tempAmt * System.Math.Round(Percentage / 100, 2));
            }

            return new Tuple<string, decimal, bool>(cur, amt, qdr.Length > 0);
        }

        private static string GetCntrType(string Location, string CntrType)
        {
            if (CntrType == "20GP")
            {
                CntrType = "F4";
            }
            else if (CntrType == "40GP")
            {
                CntrType = "F5";
            }
            else if (CntrType == "40HQ")
            {
                CntrType = "F6";
            }
            else
            {
                string sqlecrrf = string.Format("SELECT TOP 1 CHG_CD FROM ECREFFEE WHERE CHG_DESCP={0} AND (CMP='*' OR CMP={1})", SQLUtils.QuotedStr(CntrType), SQLUtils.QuotedStr(Location));
                string cerrfchgcd = CommonHelp.getOneValueAsStringFromSql(sqlecrrf);
                if (!string.IsNullOrEmpty(cerrfchgcd))
                    CntrType = cerrfchgcd;
            }

            return CntrType;
        }


        public static DEMtype SetAtCostFee(string ShipmentId, DataTable dt, List<string> emptyMessage, string StorageCur, ref decimal StorageAmt, string QuotNo, string chgcd, List<string> ListAmt=null, string cntrno = "")
        {
            DEMtype update = DEMtype.undo;
            string sql = string.Format(@"SELECT SMQTD.*,(SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=SMQTD.LSP_CD)AS LSP_NM,
(SELECT TOP 1 CREDIT_TO FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_TO,
(SELECT TOP 1 CREDIT_NM FROM SMQTM WHERE QUOT_NO={0}) AS CREDIT_NM FROM SMQTD WHERE REPAY='A' AND CHG_CD={1} AND QUOT_NO={0}", SQLUtils.QuotedStr(QuotNo), SQLUtils.QuotedStr(chgcd));
            DataTable fdt = CommonHelp.getDataTableFromSql(sql);
            bool phkflg = false;
            string msg = "";
            switch (chgcd)
            {
                case "DSTF":
                    phkflg = true;
                    msg = "Storage";
                    break;
                case "DDEM":
                    msg = "Demurrage";
                    break;
                case "DDET":
                    msg = "Detention";
                    break;
            }
            if (fdt.Rows.Count > 0)
            {
                Bill.WriteLog(string.Format("Local费用报价:{0},{1}", QuotNo, msg), ShipmentId);
                foreach (DataRow fdr in fdt.Rows)
                {
                    decimal amt = StorageAmt;
                    StorageAmt = chkPunit(dt, fdr, StorageAmt, phkflg, ListAmt);
                    fdr["F3"] = StorageAmt;
                    update = QtiFee2Smbid(dt, fdr, emptyMessage, StorageCur, amt, cntrno);
                }
            }
            else
            {
                emptyMessage.Add(string.Format("{0}:cann't find {1} fee", QuotNo, msg));
            }
            return update;
        }

        public static decimal chkPunit(DataTable smdt, DataRow fdr, decimal amt, bool PHKFlag = false, List<string> ListAmt = null)
        {
            string Punit = Prolink.Math.GetValueAsString(fdr["PUNIT"]);
            decimal Cbm = Prolink.Math.GetValueAsDecimal(smdt.Rows[0]["CBM"]);
            decimal Gw = Prolink.Math.GetValueAsDecimal(smdt.Rows[0]["GW"]);
            decimal MinAmt = Prolink.Math.GetValueAsDecimal(fdr["MIN_AMT"]);
            switch (Punit)
            {
                case "CBM":
                    amt = amt * Cbm;
                    break;
                case "KGS":
                    amt = amt * Gw;
                    break;
                case "PHK":
                    if (PHKFlag && MinAmt > 0)//空运DSTF
                    {
                        if (ListAmt == null)
                        {
                            amt = amt * System.Math.Ceiling(Gw / 100);
                            if (amt <= MinAmt)
                                amt = MinAmt;
                        }
                        else
                        {
                            decimal gw = System.Math.Ceiling(Gw / 100);
                            decimal val, cost, sumCost = 0;
                            int day;
                            foreach (string item in ListAmt)
                            {
                                string[] value = item.Split('#');
                                day = Prolink.Math.GetValueAsInt(value[0]);
                                val = Prolink.Math.GetValueAsDecimal(value[1]);
                                cost = gw * val;
                                if (cost <= MinAmt)
                                    cost = MinAmt;
                                sumCost += cost * day;
                            }
                            amt = sumCost;
                        } 
                    }
                    else
                    {
                        amt = amt * System.Math.Ceiling(Gw / 100);
                    }
                    break;
                default:
                    break;
            }
            return amt;
        }

        public static DEMtype QtiFee2Smbid(DataTable smsmi, DataRow qdr, List<string> Message, string Cur, decimal amt, string cntrno = "")
        {
            DEMtype update = DEMtype.empty;
            try
            {
                List<string> msg = new List<string>();
                string sql = string.Empty;
                string ShipmentId = Prolink.Math.GetValueAsString(smsmi.Rows[0]["SHIPMENT_ID"]);
                string TranType = Prolink.Math.GetValueAsString(smsmi.Rows[0]["TRAN_TYPE"]);
                string Carrier = Prolink.Math.GetValueAsString(smsmi.Rows[0]["CARRIER"]);
                string Location = Prolink.Math.GetValueAsString(smsmi.Rows[0]["CMP"]);
                string PodCd = Prolink.Math.GetValueAsString(smsmi.Rows[0]["POD_CD"]);
                decimal Cbm = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["CBM"]);
                decimal Gw = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["GW"]);
                decimal Vw = Prolink.Math.GetValueAsDecimal(smsmi.Rows[0]["VW"]);

                string ChgCd = Prolink.Math.GetValueAsString(qdr["CHG_CD"]);
                string ChgDescp = Prolink.Math.GetValueAsString(qdr["CHG_DESCP"]);
                string QuotNo = Prolink.Math.GetValueAsString(qdr["QUOT_NO"]);
                string Repay = Prolink.Math.GetValueAsString(qdr["REPAY"]);

                string Punit = Prolink.Math.GetValueAsString(qdr["PUNIT"]);
                decimal F3 = Prolink.Math.GetValueAsDecimal(qdr["F3"]);

                string remark = Prolink.Math.GetValueAsString(qdr["REMARK"]);

                decimal TtlFee = F3;

                List<string> RemarkUnit = new List<string>();
                RemarkUnit.Add(remark);
                RemarkUnit.Add(Punit);
                RemarkUnit.Add(Prolink.Math.GetValueAsString(amt));

                string smbidUid = string.Empty;
                MixedList mixList = new MixedList();
                msg.Add(ChgCd);
                msg.Add(Prolink.Math.GetValueAsString(TtlFee));
                string LspNo = Prolink.Math.GetValueAsString(qdr["LSP_CD"]);
                string LspNm = Prolink.Math.GetValueAsString(qdr["LSP_NM"]);
                string CreditTo = Prolink.Math.GetValueAsString(qdr["CREDIT_TO"]);
                string CreditNm = Prolink.Math.GetValueAsString(qdr["CREDIT_NM"]);


                LspNo = string.IsNullOrEmpty(CreditTo) ? LspNo : CreditTo;
                LspNm = string.IsNullOrEmpty(CreditTo) ? LspNm : CreditNm;

                bool IsOk = ChkSmbid(ChgCd, ShipmentId, LspNo, null, cntrno);
                //尚未開立帳單的才能寫入
                if (!IsOk)
                {
                    msg.Add(ShipmentId + "," + LspNo + "," + ChgCd + ":已开立账单");
                    Message.Add(ShipmentId + ":" + ChgDescp + "," + "It was a bill already");
                    update = DEMtype.undo;
                }
                if (IsOk == true)
                {
                    smbidUid = chkFeeExist(ChgCd, ShipmentId, LspNo, Location, null, cntrno);
                    if (smbidUid == "")
                    {
                        EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                        string uid = System.Guid.NewGuid().ToString();
                        ei.Put("U_ID", uid);
                        ei.Put("SHIPMENT_ID", ShipmentId);
                        ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(smsmi.Rows[0]["GROUP_ID"]));
                        ei.Put("SEC_CMP", Prolink.Math.GetValueAsString(smsmi.Rows[0]["SEC_CMP"]));
                        ei.Put("CMP", Location);
                        ei.Put("STN", Prolink.Math.GetValueAsString(smsmi.Rows[0]["STN"]));
                        ei.Put("QUOT_NO", QuotNo);
                        ei.Put("RFQ_NO", Prolink.Math.GetValueAsString(qdr["RFQ_NO"]));
                        ei.Put("QAMT", TtlFee);
                        ei.Put("QTAX", 0);
                        ei.Put("CHG_CD", ChgCd);
                        ei.Put("CHG_DESCP", ChgDescp);
                        ei.Put("QCUR", Cur);
                        ei.Put("LSP_NO", LspNo);
                        ei.Put("LSP_NM", LspNm);
                        ei.Put("TRAN_TYPE", TranType);
                        if (!string.IsNullOrEmpty(cntrno))
                        {
                            ei.Put("CNTR_NO", cntrno);
                            ei.Put("DEC_NO", cntrno);
                        }
                        ei.Put("CNTR_INFO", Prolink.Math.GetValueAsString(smsmi.Rows[0]["CNTR_INFO"]));
                        ei.Put("MASTER_NO", Prolink.Math.GetValueAsString(smsmi.Rows[0]["MASTER_NO"]));
                        ei.Put("BL_NO", Prolink.Math.GetValueAsString(smsmi.Rows[0]["MASTER_NO"]));
                        ei.Put("POD_CD", PodCd);
                        SetCHGInfo(qdr, ei, smsmi.Rows[0], RemarkUnit);
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
                        SetCHGInfo(qdr, ei, smsmi.Rows[0]);
                        msg.Add("Update");
                        mixList.Add(ei);
                    }
                }


                if (mixList.Count > 0)
                {
                    try
                    {
                        CalculateLocalAmt(mixList, smsmi.Rows[0]);
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        update = DEMtype.update;
                    }
                    catch (Exception ex)
                    {
                        msg = new List<string>();
                        msg.Add(ex.ToString());
                    }
                }
                Bill.WriteLog(string.Join(";", msg));
            }
            catch (Exception ex)
            {

            }
            return update;
        }

        public static bool ChkSmbid(string ChgCd, string ShipmentId, string LspNo, string cntrNo = "", string decNo = "")
        {
            string status = string.Empty;
            bool IsOk = true;

            string sql = string.Empty;
            sql = "SELECT STATUS FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND LSP_NO={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));
            if (ChgCd == "DTRF" && !string.IsNullOrEmpty(cntrNo))
            {
                sql = "SELECT STATUS FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND LSP_NO={2} AND CNTR_NO={3}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(cntrNo));
            }
            if (!string.IsNullOrEmpty(decNo))//"ICD".Equals(ChgCd) && 
            {
                sql += " AND DEC_NO IS NULL";
                if (!"NULL".Equals(decNo.ToUpper()))
                {
                    sql = "SELECT STATUS FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND LSP_NO={2} AND DEC_NO ={3}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(decNo));
                }
            }
            status = CommonHelp.getOneValueAsStringFromSql(sql);
            if (status == "Y" || status == "N")
            {
                IsOk = false;
            }
            return IsOk;
        }

        public static string chkFeeExist(string ChgCd, string ShipmentId, string LspNo, string cmp, string cntrNo = "", string decNo = "")
        { 
            string queryUidSql = string.Format("SELECT TOP 1 U_ID FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND CMP={2} AND LSP_NO={3}",
                SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(LspNo));
            string condition = "";
            string sql = string.Empty;
            if (ChgCd == "DTRF")
            {
                sql = "DELETE FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND (BAMT=0 OR BAMT IS NULL) AND LSP_NO={2} AND CMP={3}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(cmp));
                condition = string.Format(" AND LSP_NO={0}", SQLUtils.QuotedStr(LspNo));
                if (!string.IsNullOrEmpty(cntrNo))
                {
                    string lspquery = string.Format("SELECT TRUCKER,CNTR_NO FROM SMIRV WHERE SHIPMENT_INFO LIKE '%{0}%' OR SHIPMENT_ID = '{0}'",
                            ShipmentId);
                    DataTable dt = OperationUtils.GetDataTable(lspquery, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    List<string> lspnolist = new List<string>();
                    foreach(DataRow dr in dt.Rows)
                    {
                        string trucker = Prolink.Math.GetValueAsString(dr["TRUCKER"]);
                        if(!lspnolist.Contains(trucker) && trucker != LspNo)
                            lspnolist.Add(trucker);
                    }


                    sql = "DELETE FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND (BAMT=0 OR BAMT IS NULL) AND CNTR_NO={2} AND CMP={3} AND LSP_NO NOT IN {4}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), 
                        SQLUtils.QuotedStr(cntrNo), SQLUtils.QuotedStr(cmp), SQLUtils.Quoted(lspnolist.ToArray()));
                    condition = string.Format(" AND CNTR_NO={0} AND LSP_NO={1}", SQLUtils.QuotedStr(cntrNo), SQLUtils.QuotedStr(LspNo));
                }
            }
            else
            {
                sql = "DELETE FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND (BAMT=0 OR BAMT IS NULL) AND CMP={2} AND LSP_NO={3}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(cmp));
                if (!string.IsNullOrEmpty(decNo))// && "ICD".Equals(ChgCd)
                {
                    sql = string.Format("DELETE FROM SMBID WHERE CHG_CD={0} AND SHIPMENT_ID={1} AND (BAMT=0 OR BAMT IS NULL) AND CMP={2} AND DEC_NO{3} AND LSP_NO={4}", SQLUtils.QuotedStr(ChgCd),
                        SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(cmp), "NULL".Equals(decNo.ToUpper()) ? " IS NULL" : "=" + SQLUtils.QuotedStr(decNo), SQLUtils.QuotedStr(LspNo));
                    condition = string.Format(" AND DEC_NO{0}", "NULL".Equals(decNo.ToUpper()) ? " IS NULL" : "=" + SQLUtils.QuotedStr(decNo));
                }
            }

            try
            {
                CommonHelp.exeSql(sql);
                string uid = CommonHelp.getOneValueAsStringFromSql(queryUidSql + condition);
                return uid;
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }


        /// <summary>
        /// 将报价中的费用信息设置到账单明细中
        /// </summary>
        /// <param name="chg"></param>
        /// <param name="ei"></param>
        public static void SetCHGInfo(DataRow chg, EditInstruct ei, DataRow smsmi = null, List<string> RemarkUnit = null)
        {
            if (ei == null)
                return;

            if (chg != null)
            {
                ei.Put("CHG_TYPE", chg.Table.Columns.Contains("CHG_TYPE") ? chg["CHG_TYPE"] : string.Empty);
                ei.Put("REPAY", chg.Table.Columns.Contains("REPAY") ? chg["REPAY"] : string.Empty);
            }

            switch (ei.ID)
            {
                case "SMBID":
                    if (ei.OperationType == EditInstruct.INSERT_OPERATION || ei.OperationType == EditInstruct.UPDATE_OPERATION)
                    {

                        if (RemarkUnit != null && RemarkUnit.Count > 0)
                        {
                            ei.Put("REMARK", RemarkUnit[0]);
                            if (RemarkUnit.Count > 3)
                            {
                                ei.Put("QCHG_UNIT", RemarkUnit[1]);
                                ei.Put("QUNIT_PRICE", RemarkUnit[2]);
                                ei.Put("QQTY", RemarkUnit[3]);
                            }
                        }
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                    }

                    if (smsmi != null)
                    {
                        if (smsmi.Table.Columns.Contains("ATA") && smsmi["ATA"] != null && smsmi["ATA"] != DBNull.Value)
                            ei.PutDate("DEBIT_DATE", smsmi["ATA"]);
                        else if (smsmi.Table.Columns.Contains("ETA") && smsmi["ETA"] != null && smsmi["ETA"] != DBNull.Value)
                            ei.PutDate("DEBIT_DATE", smsmi["ETA"]);

                        if (smsmi.Table.Columns.Contains("INVOICE_INFO"))
                            ei.Put("INVOICE_INFO", smsmi["INVOICE_INFO"]);
                    }
                    break;
            }
        }

        public static void CalculateLocalAmt(MixedList ml, DataRow smsmi = null, string groupId = "", string cmp = "")
        {
            if (smsmi != null)
            {
                groupId = smsmi.Table.Columns.Contains("GROUP_ID") ? Prolink.Math.GetValueAsString(smsmi["GROUP_ID"]) : string.Empty;
                cmp = smsmi.Table.Columns.Contains("CMP") ? Prolink.Math.GetValueAsString(smsmi["CMP"]) : string.Empty;
            }

            #region 获取本地币别
            string localCur = Business.TPV.Standard.BillingManager.GetLocalCur(groupId, cmp);
            #endregion

            #region 获取相关币别的全部汇率
            List<string> curs = new List<string>();
            curs.Add(localCur);
            for (int i = 0; i < ml.Count; i++)
            {
                if (!(ml[i] is EditInstruct))
                {
                    continue;
                }
                EditInstruct ei = (EditInstruct)ml[i];
                if (!"SMBID".Equals(ei.ID))
                    continue;

                string qcur = ei.Get("QCUR");
                if (!string.IsNullOrEmpty(qcur) && !curs.Contains(qcur))
                    curs.Add(qcur);
            }

            string sql = string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE FCUR IN {0} UNION SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE TCUR IN {0}", SQLUtils.Quoted(curs.ToArray()));
            DataTable rateDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            for (int i = 0; i < ml.Count; i++)
            {
                if (!(ml[i] is EditInstruct))
                {
                    continue;
                }
                EditInstruct ei = (EditInstruct)ml[i];
                if (!"SMBID".Equals(ei.ID))
                    continue;

                decimal qamt = ei.GetValueAsDecimal("QAMT");
                string qcur = ei.Get("QCUR");
                if (string.IsNullOrEmpty(qcur))
                    continue;
                if (qcur.Equals(localCur))
                {
                    ei.Put("QLRATE", 1);
                    ei.Put("QEX_RATE", 1);
                    ei.Put("QLAMT", qamt);
                    ei.Put("QLCUR", localCur);
                    continue;
                }

                decimal lqamt = 0;
                ei.Put("QLCUR", localCur);
                decimal rate = GetTotal(rateDt, qamt, qcur, ref lqamt, localCur);
                ei.Put("QLRATE", rate);
                ei.Put("QEX_RATE", rate);
                ei.Put("QLAMT", lqamt); ;
            }
        }

        public static decimal GetTotal(DataTable rateDt, decimal val, string cur, ref decimal total, string to_cur = "", List<string> msg = null)
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
                msgStr = string.Format("无{0}对应{1}的费率", cur, to_cur);
                if (msg != null && !msg.Contains(msgStr))
                    msg.Add(msgStr);
            }
            else
            {
                rate = Prolink.Math.GetValueAsDecimal(drs[0]["EX_RATE"]);
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

        public static void UpdateFreeDueDate(List<PortFeeDate> portFeeDateList, PortFeeDate portFeeDate,DataTable bsDateDt)
        {
            DEMtype update = DEMtype.undo;
            MixedList ml = new MixedList();
            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            smsmiei.PutKey("SHIPMENT_ID", portFeeDate.ShipmentId);

            if (portFeeDate.PortFreeTime != 0)
            {
                smsmiei.Put("PORT_FREE_TIME", portFeeDate.PortFreeTime);
            }
            else
            {
                smsmiei.Put("PORT_FREE_TIME", null);
            }            
            smsmiei.Put("FACT_FREE_TIME", portFeeDate.FactFreeTime);
            smsmiei.Put("CON_FREE_TIME", portFeeDate.ConFreeTime);
            smsmiei.Put("COMBINE_DET", portFeeDate.CombineDet);
            smsmiei.Put("SHOW_COMBINE_DET", portFeeDate.ShowCombineDet);
            ml.Add(smsmiei);
            string sql = string.Format("SELECT ORD_NO FROM SMORD WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(portFeeDate.ShipmentId));
            string ordNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (PortFeeDate feeDate in portFeeDateList)
            {
                
                if (feeDate.CombineDet != portFeeDate.CombineDet && (feeDate.CombineDet == "NN" || string.IsNullOrEmpty(feeDate.CombineDet)))
                {
                    feeDate.CombineDet = portFeeDate.CombineDet;
                    feeDate.PortFreeTime = portFeeDate.PortFreeTime;
                    feeDate.PortChgDayType = portFeeDate.PortChgDayType;
                    feeDate.FactFreeTime = portFeeDate.FactFreeTime;
                    feeDate.FactChgDayType = portFeeDate.FactChgDayType;
                    feeDate.ConFreeTime = portFeeDate.ConFreeTime;
                    feeDate.ConChgDayType = portFeeDate.ConChgDayType;
                }
                var dueDateItem = getDueDate(feeDate, bsDateDt);
                EditInstruct icntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                icntrei.PutKey("SHIPMENT_ID", feeDate.ShipmentId);
                icntrei.PutKey("CNTR_NO", feeDate.CntrNo);
                icntrei.PutDate("DETENTION_DUE_DATE", dueDateItem.Item1);
                icntrei.PutDate("STORAGE_DUE_DATE", dueDateItem.Item3);
                icntrei.Put("PORT_CHG_TYPE", feeDate.PortChgDayType);
                icntrei.Put("FACT_CHG_TYPE", feeDate.FactChgDayType);
                icntrei.Put("CON_CHG_TYPE", feeDate.ConChgDayType);

                if (feeDate.ShowCombineDet != "Y")
                {
                    icntrei.PutDate("DEMURRAGE_DUE_DATE", dueDateItem.Item2);
                }
                else
                {
                    icntrei.PutDate("DEMURRAGE_DUE_DATE", null);
                }
                update = SetEivalue("STO_EST", feeDate.StoUp, feeDate.StoAmt, feeDate.StoCur, icntrei, update);
                update = SetEivalue("DEM_EST", feeDate.DemUp, feeDate.DemAmt, feeDate.DemCur, icntrei, update);
                update = SetEivalue("DET_EST", feeDate.DetUp, feeDate.DetAmt, feeDate.DetCur, icntrei, update);

                ml.Add(icntrei);
                if (string.IsNullOrEmpty(ordNo))
                    continue;
                EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", feeDate.ShipmentId);
                ei.PutKey("CNTR_NO", feeDate.CntrNo);
                ei.Put("FACT_FREE_TIME", portFeeDate.FactFreeTime);
                ei.Put("CON_FREE_TIME", portFeeDate.ConFreeTime);
                ei.Put("COMBINE_DET", portFeeDate.CombineDet);
                ei.Put("SHOW_COMBINE_DET", portFeeDate.ShowCombineDet);
                ei.PutDate("DETENTION_DUE_DATE", dueDateItem.Item1);
                ei.PutDate("STORAGE_DUE_DATE", dueDateItem.Item3);

                if (feeDate.ShowCombineDet != "Y")
                {
                    ei.PutDate("DEMURRAGE_DUE_DATE", dueDateItem.Item2);
                }
                else
                {
                    ei.Put("DEMURRAGE_DUE_DATE", null);
                }

                if (feeDate.ShowCombineDet != "Y" && portFeeDate.PortFreeTime != 0)
                {
                    ei.Put("PORT_FREE_TIME", portFeeDate.PortFreeTime);
                }
                else
                {
                    ei.Put("PORT_FREE_TIME", null);
                }

                update = SetEivalue("STO_EST", feeDate.StoUp, feeDate.StoAmt, feeDate.StoCur, ei, update);
                update = SetEivalue("DEM_EST", feeDate.DemUp, feeDate.DemAmt, feeDate.DemCur, ei, update);
                update = SetEivalue("DET_EST", feeDate.DetUp, feeDate.DetAmt, feeDate.DetCur, ei, update);
                
                ml.Add(ei);
            }

            if (ml.Count > 0 && update != DEMtype.undo)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static Func<string, DEMtype, decimal, string, EditInstruct, DEMtype, DEMtype> SetEivalue = (col, demtype, Amt, Cur, ei, updateDEMtype) =>
         {
             switch (demtype)
             {
                 case DEMtype.update:
                     ei.Put(col + "_AMT", Amt);
                     ei.Put(col + "_CUR", Cur);
                     updateDEMtype = DEMtype.update;
                     break;
                 case DEMtype.empty:
                     ei.Put(col + "_AMT", "");
                     ei.Put(col + "_CUR", "");
                     updateDEMtype = DEMtype.empty;
                     break;
             }
             return updateDEMtype;
         };

        public static void UpdateACTAmt(PortFeeDate portFeeDate,MixedList ml)
        {
            DEMtype update = DEMtype.undo;
            if (ml == null)
            {
                ml = new MixedList();
                update = DEMtype.update;
            }
            
            EditInstruct inctrEi = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
            inctrEi.PutKey("SHIPMENT_ID", portFeeDate.ShipmentId);
            inctrEi.PutKey("CNTR_NO", portFeeDate.CntrNo);

            update = SetEivalue("STO_ACT", portFeeDate.StoUp, portFeeDate.StoAmt, portFeeDate.StoCur, inctrEi, update);
            update = SetEivalue("DEM_ACT", portFeeDate.DemUp, portFeeDate.DemAmt, portFeeDate.DemCur, inctrEi, update);
            update = SetEivalue("DET_ACT", portFeeDate.DetUp, portFeeDate.DetAmt, portFeeDate.DetCur, inctrEi, update);

            if (portFeeDate.DetUp != DEMtype.undo || portFeeDate.DemUp != DEMtype.undo || portFeeDate.StoUp != DEMtype.undo) 
                ml.Add(inctrEi);
            string sql = string.Format("SELECT ORD_NO FROM SMRCNTR WHERE CNTR_NO={0} AND SHIPMENT_ID={1}",
                SQLUtils.QuotedStr(portFeeDate.CntrNo), SQLUtils.QuotedStr(portFeeDate.ShipmentId));
            string ordNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(ordNo))
            {
                EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("ORD_NO", ordNo);

                update = SetEivalue("STO_ACT", portFeeDate.StoUp, portFeeDate.StoAmt, portFeeDate.StoCur, ei, update);
                update = SetEivalue("DEM_ACT", portFeeDate.DemUp, portFeeDate.DemAmt, portFeeDate.DemCur, ei, update);
                update = SetEivalue("DET_ACT", portFeeDate.DetUp, portFeeDate.DetAmt, portFeeDate.DetCur, ei, update);

                if (portFeeDate.DetUp != DEMtype.undo || portFeeDate.DemUp != DEMtype.undo || portFeeDate.StoUp != DEMtype.undo)
                    ml.Add(ei);
            }

            if (ml.Count > 0 && update != DEMtype.undo)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {

                }
            }
        }

        
    }

}
