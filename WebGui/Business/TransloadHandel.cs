using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrackingEDI.Business;

namespace Business
{
    public class TransloadHandel
    {
        public DateTime ATA { get; set; }
        public DateTime TcDecDate { get; set; }
        public string MasterNo { get; set; }
        public string InvNo { get; set; }
        public string CntrNo { get; set; }
        public string CntrSeal { get; set; }
        public string PackList { get; set; }
        public string ImpLiceReq { get; set; }
        public string ImpLiceApv { get; set; }
        public DateTime CiOutDate { get; set; }
        public string TcDecNo { get; set; }
        public string TsTrailer { get; set; }
        public string TsTruck { get; set; }
        public string PlaNo { get; set; }
        public int PlaQty { get; set; }
        public string TsSealNo { get; set; }
        public string CiOutNo { get; set; }
        public string MIGO { get; set; }
        public bool IsPla { get; set; }


        public DateTime CiInDate { get; set; }
        public string DecInfo { get; set; }
        public string DecDateInfo { get; set; }
        public DateTime RelDate { get; set; }
        public DateTime PodUpdateDate { get; set; }
        public string IdnplUID { get; set; }
        public string ShipmentId { get; set; }
        public string CSMUID { get; set; }
        public string CSMNo { get; set; }
        public decimal CcRate { get; set; }
        public decimal FobAmt { get; set; }
        public static DataTable CityDataTable { get; set; }
        //public string CiOutDate { get; set; }
        public static string HandleTransloadDetail(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            TransloadHandel transloadInfo = new TransloadHandel();
            transloadInfo.MasterNo = Prolink.Math.GetValueAsString(ei.Get("MASTER_NO"));
            transloadInfo.InvNo = Prolink.Math.GetValueAsString(ei.Get("INV_NO"));
            transloadInfo.CntrNo = Prolink.Math.GetValueAsString(ei.Get("CNTR_NO"));
            transloadInfo.CntrSeal = Prolink.Math.GetValueAsString(ei.Get("CNTR_SEAL"));
            transloadInfo.PackList = Prolink.Math.GetValueAsString(ei.Get("PACK_LIST"));
            transloadInfo.ImpLiceReq = Prolink.Math.GetValueAsString(ei.Get("IMP_LICE_REQ"));
            transloadInfo.ImpLiceApv = Prolink.Math.GetValueAsString(ei.Get("IMP_LICE_APV"));
            transloadInfo.ATA = Prolink.Math.GetValueAsDateTime(ei.Get("ATA"));
            transloadInfo.TcDecNo = Prolink.Math.GetValueAsString(ei.Get("TC_DEC_NO"));
            transloadInfo.TcDecDate = Prolink.Math.GetValueAsDateTime(ei.Get("TC_DEC_DATE"));
            transloadInfo.TsTrailer = Prolink.Math.GetValueAsString(ei.Get("TS_TRAILER"));
            transloadInfo.TsTruck = Prolink.Math.GetValueAsString(ei.Get("TS_TRUCK"));
            transloadInfo.PlaNo = Prolink.Math.GetValueAsString(ei.Get("HTML_PLA_NO"));
            transloadInfo.PlaQty = Prolink.Math.GetValueAsInt(ei.Get("PALLET_QTY"));
            transloadInfo.TsSealNo = Prolink.Math.GetValueAsString(ei.Get("TS_SEAL_NO"));
            transloadInfo.CiOutNo = Prolink.Math.GetValueAsString(ei.Get("CI_OUT_NO"));
            transloadInfo.CiOutDate = Prolink.Math.GetValueAsDateTime(ei.Get("CI_OUT_DATE"));
            transloadInfo.CiInDate = Prolink.Math.GetValueAsDateTime(ei.Get("CI_IN_DATE"));
            transloadInfo.DecInfo = Prolink.Math.GetValueAsString(ei.Get("DEC_INFO"));
            transloadInfo.DecDateInfo = Prolink.Math.GetValueAsString(ei.Get("DEC_DATE_INFO"));
            transloadInfo.RelDate = Prolink.Math.GetValueAsDateTime(ei.Get("REL_DATE"));
            transloadInfo.PodUpdateDate = Prolink.Math.GetValueAsDateTime(ei.Get("POD_UPDATE_DATE"));
            transloadInfo.MIGO = Prolink.Math.GetValueAsString(ei.Get("MIGO"));
            transloadInfo.CcRate = Prolink.Math.GetValueAsDecimal(ei.Get("CC_RATE"));
            transloadInfo.FobAmt = Prolink.Math.GetValueAsDecimal(ei.Get("FOB_AMT"));
            transloadInfo.IsPla = false;
            if (transloadInfo.PlaNo.StartsWith("Pallet No:"))
                transloadInfo.IsPla = true;
            transloadInfo.PlaNo = transloadInfo.PlaNo.Replace("Pallet No:", "").Replace("Case No:", "");

            string condition = string.Format("WHERE MASTER_NO={0} ", SQLUtils.QuotedStr(transloadInfo.MasterNo));
            if (!string.IsNullOrEmpty(transloadInfo.InvNo))
            {
                condition += " AND INV_NO=" + SQLUtils.QuotedStr(transloadInfo.InvNo);
            }
            else {
                condition += " AND INV_NO IS NULL";
            }

            if (!string.IsNullOrEmpty(transloadInfo.CntrNo))
            {
                condition += " AND CNTR_NO=" + SQLUtils.QuotedStr(transloadInfo.CntrNo);
            }
            else
            {
                condition += " AND CNTR_NO IS NULL";
            }

            if (transloadInfo.IsPla)
            {
                condition += " AND PLA_NO=" + SQLUtils.QuotedStr(transloadInfo.PlaNo);
            } else
            {
                condition += " AND CASE_NO=" + SQLUtils.QuotedStr(transloadInfo.PlaNo);
            }

            string sql = string.Format("SELECT U_ID,SHIPMENT_ID,U_FID FROM SMIDNPL " + condition);
            DataTable idnplDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow idnpldr in idnplDt.Rows)
            {
                transloadInfo.IdnplUID = Prolink.Math.GetValueAsString(idnpldr["U_ID"]);
                transloadInfo.ShipmentId = Prolink.Math.GetValueAsString(idnpldr["SHIPMENT_ID"]);
                transloadInfo.CSMUID = Prolink.Math.GetValueAsString(idnpldr["U_FID"]);
            }
            Dictionary<string, string> csmUidDic = (Dictionary<string, string>)parm["csmInfo"];
            if (!string.IsNullOrEmpty(transloadInfo.CSMUID))
            {
                sql = string.Format("SELECT TS_TRUCK,SHIPMENT_ID FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(transloadInfo.CSMUID));
                DataTable csmDt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (csmDt.Rows.Count > 0)
                {
                    string csmTruck = Prolink.Math.GetValueAsString(csmDt.Rows[0]["TS_TRUCK"]);
                    string csmNo = Prolink.Math.GetValueAsString(csmDt.Rows[0]["SHIPMENT_ID"]);
                    transloadInfo.CSMNo = csmNo;
                    if (csmTruck.Equals(transloadInfo.TsTruck) && !csmUidDic.ContainsKey(transloadInfo.TsTruck))
                        csmUidDic.Add(transloadInfo.TsTruck, transloadInfo.CSMUID);
                }
            }
            
            List<TransloadHandel> transloadList = new List<TransloadHandel>();
            Dictionary<string, List<TransloadHandel>> transloadDic = (Dictionary<string, List<TransloadHandel>>)parm["transloadInfo"];
            if (!transloadDic.ContainsKey(transloadInfo.TsTruck))
                transloadDic.Add(transloadInfo.TsTruck, transloadList);
            transloadList = transloadDic[transloadInfo.TsTruck];
            transloadList.Add(transloadInfo);

            return BaseParser.ERROR;
        }

        public static string CreateConsignment(List<TransloadHandel> transloadList, Dictionary<string, string> csmUidDic, MixedList ml, string userId, Dictionary<string, List<string>> rerunCsm) {
            if (CityDataTable == null || CityDataTable.Rows.Count <= 0) InitCityData();
            TransloadHandel transloadHandel = transloadList[0];
            string truck = transloadHandel.TsTruck;
            string sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(transloadHandel.ShipmentId));
            DataTable smsmiDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string GroupId = Prolink.Math.GetValueAsString(smsmiDt.Rows[0]["GROUP_ID"]);
            string CompanyId = Prolink.Math.GetValueAsString(smsmiDt.Rows[0]["CMP"]);
            string podCd = Prolink.Math.GetValueAsString(smsmiDt.Rows[0]["POD_CD"]);
            string tranType = Prolink.Math.GetValueAsString(smsmiDt.Rows[0]["TRAN_TYPE"]);
            string csmUid = System.Guid.NewGuid().ToString();
            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.INSERT_OPERATION);
            if (csmUidDic.ContainsKey(truck))
            {
                csmUid = csmUidDic[truck];
                smsmiei.OperationType = EditInstruct.UPDATE_OPERATION;
                smsmiei.PutKey("U_ID", csmUid);
            }
            string shipmentid = string.Empty;
            Dictionary<string,string> clearUid =new Dictionary<string, string>();
            if (smsmiei.OperationType == EditInstruct.INSERT_OPERATION)
            {
                Dictionary<string, object> parm = new Dictionary<string, object>();
                parm["mixedlist"] = ml;
                foreach (DataColumn column in smsmiDt.Columns)
                {
                    switch (column.ColumnName.ToUpper())
                    {
                        case "U_ID":
                            smsmiei.Put(column.ColumnName, csmUid);
                            break;
                        case "CSM_NO":
                            shipmentid = TrackingEDI.InboundBusiness.ReserveHelper.getAutoNo("IBCSM_NO", GroupId, CompanyId);
                            smsmiei.Put(column.ColumnName, shipmentid);
                            smsmiei.Put("SHIPMENT_ID", shipmentid);
                            break;
                        case "SHIPMENT_ID":
                        case "BL_NO_INFO":
                        case "TC_DEC_NO":
                        case "TC_DEC_DATE":
                        case "TS_TRAILER":
                        case "TS_TRUCK":
                        case "PALLET_QTY":
                        case "TS_SEAL_NO":
                        case "CI_OUT_NO":
                        case "CI_OUT_DATE":
                        case "CI_IN_DATE":
                        case "REL_DATE":
                        case "POD_UPDATE_DATE":
                        case "TRANSLOAD":
                        case "CNTR_INFO":
                        case "INVOICE_INFO":
                        case "DEC_INFO":
                        case "DEC_DATE_INFO":
                        case "CREATE_DATE_L":
                        case "MODIFY_DATE":
                        case "MODIFY_DATE_L":
                        case "MODIFY_BY":
                        case "ATA":
                            break;
                        case "TRAN_TYPE":
                            smsmiei.Put(column.ColumnName, "F");
                            break;
                        case "BSTATUS":
                        case "STATUS":
                            smsmiei.Put(column.ColumnName, "I");
                            break;
                        case "CSTATUS":
                            smsmiei.Put(column.ColumnName, "Y");
                            break;
                        case "CREATE_DATE":
                            DateTime odt = DateTime.Now;
                            DateTime ndt = TrackingEDI.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            smsmiei.PutDate("CREATE_DATE", odt);
                            smsmiei.PutDate("CREATE_DATE_L", ndt);
                            break;
                        case "CREATE_BY":
                            smsmiei.Put(column.ColumnName, userId);
                            break;
                        default:
                            if (column.DataType == typeof(DateTime) || column.DataType.Name.StartsWith("Date"))
                            {
                                smsmiei.PutDate(column.ColumnName, Prolink.Math.GetValueAsDateTime(smsmiDt.Rows[0][column.ColumnName]));
                            }
                            else
                            {
                                smsmiei.Put(column.ColumnName, Prolink.Math.GetValueAsString(smsmiDt.Rows[0][column.ColumnName]));
                            }
                            break;
                    }
                }
                sql = string.Format("SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(transloadHandel.ShipmentId));
                DataTable ptDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in ptDt.Rows)
                {
                    string partyNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                    string partyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    InboundUploadExcelManager.CreateSmsmiptinfo(csmUid, shipmentid, partyType, partyNo, parm, CompanyId);
                }
            }
            else
            {
                DateTime odt = DateTime.Now;
                DateTime ndt = TrackingEDI.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                smsmiei.PutDate("MODIFY_DATE", odt);
                smsmiei.PutDate("MODIFY_DATE_L", ndt);
                smsmiei.Put("MODIFY_BY", userId);
                sql = string.Format("SELECT U_ID,SHIPMENT_ID FROM SMIDNPL WHERE U_FID={0}", SQLUtils.QuotedStr(csmUid));
                DataTable OidnplDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in OidnplDt.Rows)
                {
                    string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    string plShipment = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    if (!clearUid.ContainsKey(uid) && !string.IsNullOrEmpty(uid))
                        clearUid.Add(uid, plShipment);
                }
                
            }
            smsmiei.Put("TC_DEC_NO", transloadHandel.TcDecNo);
            smsmiei.PutDate("TC_DEC_DATE", transloadHandel.TcDecDate);
            smsmiei.Put("TS_TRAILER", transloadHandel.TsTrailer);
            smsmiei.Put("TS_TRUCK", transloadHandel.TsTruck);
            smsmiei.Put("TS_SEAL_NO", transloadHandel.TsSealNo);
            smsmiei.Put("CI_OUT_NO", transloadHandel.CiOutNo);
            smsmiei.PutDate("CI_OUT_DATE", transloadHandel.CiOutDate);
            smsmiei.PutDate("ATA", transloadHandel.ATA);

            int palletQty = 0;
            List<string> cntrList = new List<string>();
            List<string> blNoList = new List<string>();
            List<string> invoiceList = new List<string>();
            List<string> idnpluidList = new List<string>();
            foreach (TransloadHandel transload in transloadList)
            {
                if (clearUid.ContainsKey(transload.IdnplUID))
                    clearUid.Remove(transload.IdnplUID);

                if (!idnpluidList.Contains(transload.IdnplUID) && !string.IsNullOrEmpty(transload.IdnplUID))
                    idnpluidList.Add(transload.IdnplUID);
                EditInstruct idnplei = new EditInstruct("SMIDNPL", EditInstruct.UPDATE_OPERATION);
                idnplei.PutKey("U_ID", transload.IdnplUID);
                idnplei.Put("U_FID", csmUid);
                ml.Add(idnplei);
                palletQty += transload.PlaQty;
                if (!cntrList.Contains(transload.CntrNo) && !string.IsNullOrEmpty(transload.CntrNo))
                    cntrList.Add(transload.CntrNo);
                if (!blNoList.Contains(transload.MasterNo) && !string.IsNullOrEmpty(transload.MasterNo))
                    blNoList.Add(transload.MasterNo);
                if (!invoiceList.Contains(transload.InvNo) && !string.IsNullOrEmpty(transload.InvNo))
                    invoiceList.Add(transload.InvNo);
                if (!string.IsNullOrEmpty(transload.CSMUID))
                {
                    if (!rerunCsm.ContainsKey(transload.CSMUID))
                        rerunCsm.Add(transload.CSMUID, new List<string>() { transload.IdnplUID });
                    List<string> uidList = rerunCsm[transload.CSMUID];
                    if (!uidList.Contains(transload.IdnplUID))
                        uidList.Add(transload.IdnplUID);
                    rerunCsm[transload.CSMUID] = uidList;
                }
            }
            if (rerunCsm.ContainsKey(csmUid))
                rerunCsm.Remove(csmUid);

            sql = string.Format("SELECT SUM(QTY) AS SUM_QTY,SUM(NW) AS SUM_NW,SUM(GW) AS SUM_GW,SUM(CBM) AS SUM_CBM FROM SMIDNPL WHERE U_ID IN {0}", SQLUtils.Quoted(idnpluidList.ToArray()));
            DataTable idnplDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            decimal sumQty = 0;
            decimal sumNw = 0;
            decimal sumGw = 0;
            decimal sumCbm = 0;
            if (idnplDt.Rows.Count > 0)
            {
                sumQty = Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["SUM_QTY"]);
                sumNw = Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["SUM_NW"]);
                sumGw = Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["SUM_GW"]);
                sumCbm = Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["SUM_CBM"]);
            }


            smsmiei.Put("PALLET_QTY", sumQty);
            smsmiei.Put("GW", sumGw);
            smsmiei.Put("CBM", sumCbm);
            smsmiei.Put("CNTR_INFO", string.Join(";", cntrList.ToArray()));
            smsmiei.Put("INVOICE_INFO", string.Join(";", invoiceList.ToArray()));
            smsmiei.Put("BL_NO_INFO", string.Join(";", blNoList.ToArray()));

            ml.Add(smsmiei);

            sql = string.Format("SELECT SHIPMENT_ID,POD_CD FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(csmUid));
            DataTable csmDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in csmDt.Rows)
            {
                shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                podCd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
            }

            EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.INSERT_OPERATION);
            if (csmDt.Rows.Count > 0)
            {
                smicntrei.OperationType = EditInstruct.UPDATE_OPERATION;
                smicntrei.PutKey("SHIPMENT_ID", shipmentid);
                smicntrei.PutKey("CNTR_NO", truck);
            } 
            else
            {
                smicntrei.Put("U_ID", System.Guid.NewGuid().ToString());
                smicntrei.Put("CMP", CompanyId);
                smicntrei.Put("SHIPMENT_ID", shipmentid);
                //smicntrei.Put("DN_NO", invoiceno);
                smicntrei.Put("CNTR_NO", truck);
                smicntrei.Put("PRIORITY", "2");
            }
            smicntrei.Put("QTY", sumQty);
            smicntrei.Put("NW", sumNw);

            //smicntrei.Put("CNTR_TYPE", CntrType);
            smicntrei.Put("SEAL_NO1", transloadHandel.TsSealNo);
            string podname = GetTableValueByName(CityDataTable, string.Format("CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(podCd.Substring(0, 2)), SQLUtils.QuotedStr(podCd.Substring(2, 3))), "PORT_NM");
            smicntrei.Put("TRAN_TYPE1", "T");
            smicntrei.Put("POL1", podCd);
            smicntrei.Put("POL_NM1", podname);
            smicntrei.Put("PRIORITY", "2");
            smicntrei.Put("BACK_LOCATION", podCd);

            smicntrei.Put("CNTRY_CD", podCd.Substring(0, 2));
            smicntrei.Put("TC_CNTRY_CD", podCd.Substring(0, 2));

            if (tranType == "F" || tranType == "R")
            {
                sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(transloadHandel.ShipmentId));
                DataTable cntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (cntrDt.Rows.Count > 0)
                {
                    DataRow smicntrdr = cntrDt.Rows[0];
                    smicntrei.Put("POD1", Prolink.Math.GetValueAsString(smicntrdr["POD1"]));
                    smicntrei.Put("POD_NM1", Prolink.Math.GetValueAsString(smicntrdr["POD_NM1"]));
                    smicntrei.Put("TRAN_TYPE1", Prolink.Math.GetValueAsString(smicntrdr["TRAN_TYPE1"]));
                    smicntrei.Put("DEP_ADDR1", Prolink.Math.GetValueAsString(smicntrdr["DEP_ADDR1"]));
                    smicntrei.Put("TRUCKER1", Prolink.Math.GetValueAsString(smicntrdr["TRUCKER1"]));
                    smicntrei.Put("TRUCKER_NM1", Prolink.Math.GetValueAsString(smicntrdr["TRUCKER_NM1"]));
                    smicntrei.Put("POL2", Prolink.Math.GetValueAsString(smicntrdr["POL2"]));
                    smicntrei.Put("POL_NM2", Prolink.Math.GetValueAsString(smicntrdr["POL_NM2"]));
                    smicntrei.Put("POL3", Prolink.Math.GetValueAsString(smicntrdr["POL3"]));
                    smicntrei.Put("POL_NM3", Prolink.Math.GetValueAsString(smicntrdr["POL_NM3"]));
                    smicntrei.Put("TRAN_TYPE2", Prolink.Math.GetValueAsString(smicntrdr["TRAN_TYPE2"]));
                    smicntrei.Put("DEP_ADDR2", Prolink.Math.GetValueAsString(smicntrdr["DEP_ADDR2"]));
                    smicntrei.Put("TRUCKER2", Prolink.Math.GetValueAsString(smicntrdr["TRUCKER2"]));
                    smicntrei.Put("TRUCKER_NM2", Prolink.Math.GetValueAsString(smicntrdr["TRUCKER_NM2"]));
                    smicntrei.Put("TRAN_TYPE3", Prolink.Math.GetValueAsString(smicntrdr["TRAN_TYPE3"]));
                    smicntrei.Put("DEP_ADDR3", Prolink.Math.GetValueAsString(smicntrdr["DEP_ADDR3"]));
                    smicntrei.Put("TRUCKER3", Prolink.Math.GetValueAsString(smicntrdr["TRUCKER3"]));
                    smicntrei.Put("TRUCKER_NM3", Prolink.Math.GetValueAsString(smicntrdr["TRUCKER_NM3"]));
                    smicntrei.Put("DIVISION_DESCP", Prolink.Math.GetValueAsString(smicntrdr["DIVISION_DESCP"]));
                    smicntrei.Put("WS_CD", Prolink.Math.GetValueAsString(smicntrdr["WS_CD"]));
                    smicntrei.Put("WS_NM", Prolink.Math.GetValueAsString(smicntrdr["WS_NM"]));
                    smicntrei.Put("ADDR_CODE", Prolink.Math.GetValueAsString(smicntrdr["ADDR_CODE"]));
                    smicntrei.Put("DLV_ADDR", Prolink.Math.GetValueAsString(smicntrdr["DLV_ADDR"]));
                    smicntrei.Put("DLV_AREA", Prolink.Math.GetValueAsString(smicntrdr["DLV_AREA"]));
                    smicntrei.Put("DLV_AREA_NM", Prolink.Math.GetValueAsString(smicntrdr["DLV_AREA_NM"]));
                    smicntrei.Put("FINAL_WH", Prolink.Math.GetValueAsString(smicntrdr["FINAL_WH"]));
                }
            }
            else
            {
                DataRow smDr = smsmiDt.Rows[0];
                smicntrei.Put("POD1", Prolink.Math.GetValueAsString(smDr["POD1"]));
                smicntrei.Put("POD_NM1", Prolink.Math.GetValueAsString(smDr["POD_NM1"]));
                smicntrei.Put("TRAN_TYPE1", Prolink.Math.GetValueAsString(smDr["TRAN_TYPE1"]));
                smicntrei.Put("DEP_ADDR1", Prolink.Math.GetValueAsString(smDr["DEP_ADDR1"]));
                smicntrei.Put("TRUCKER1", Prolink.Math.GetValueAsString(smDr["TRUCKER1"]));
                smicntrei.Put("TRUCKER_NM1", Prolink.Math.GetValueAsString(smDr["TRUCKER_NM1"]));
                smicntrei.Put("POL2", Prolink.Math.GetValueAsString(smDr["POL2"]));
                smicntrei.Put("POL_NM2", Prolink.Math.GetValueAsString(smDr["POL_NM2"]));
                smicntrei.Put("POL3", Prolink.Math.GetValueAsString(smDr["POL3"]));
                smicntrei.Put("POL_NM3", Prolink.Math.GetValueAsString(smDr["POL_NM3"]));
                smicntrei.Put("TRAN_TYPE2", Prolink.Math.GetValueAsString(smDr["TRAN_TYPE2"]));
                smicntrei.Put("DEP_ADDR2", Prolink.Math.GetValueAsString(smDr["DEP_ADDR2"]));
                smicntrei.Put("TRUCKER2", Prolink.Math.GetValueAsString(smDr["TRUCKER2"]));
                smicntrei.Put("TRUCKER_NM2", Prolink.Math.GetValueAsString(smDr["TRUCKER_NM2"]));
                smicntrei.Put("TRAN_TYPE3", Prolink.Math.GetValueAsString(smDr["TRAN_TYPE3"]));
                smicntrei.Put("DEP_ADDR3", Prolink.Math.GetValueAsString(smDr["DEP_ADDR3"]));
                smicntrei.Put("TRUCKER3", Prolink.Math.GetValueAsString(smDr["TRUCKER3"]));
                smicntrei.Put("TRUCKER_NM3", Prolink.Math.GetValueAsString(smDr["TRUCKER_NM3"]));
                smicntrei.Put("DIVISION_DESCP", Prolink.Math.GetValueAsString(smDr["PRODUCT_INFO"]));
                sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(transloadHandel.ShipmentId));
                DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dnDt.Rows.Count > 0)
                {
                    DataRow dnRow = dnDt.Rows[0];
                    smicntrei.Put("WS_CD", Prolink.Math.GetValueAsString(dnRow["WS_CD"]));
                    smicntrei.Put("WS_NM", Prolink.Math.GetValueAsString(dnRow["WS_NM"]));
                    smicntrei.Put("ADDR_CODE", Prolink.Math.GetValueAsString(dnRow["ADDR_CODE"]));
                    smicntrei.Put("DLV_ADDR", Prolink.Math.GetValueAsString(dnRow["DLV_ADDR"]));
                    smicntrei.Put("DLV_AREA", Prolink.Math.GetValueAsString(dnRow["DLV_AREA"]));
                    smicntrei.Put("DLV_AREA_NM", Prolink.Math.GetValueAsString(dnRow["DLV_AREA_NM"]));
                    smicntrei.Put("FINAL_WH", Prolink.Math.GetValueAsString(dnRow["FINAL_WH"]));
                }
            }

            ml.Add(smicntrei);

            List<string> plShipmentList = new List<string>();
            foreach (string uid in clearUid.Keys)
            {
                EditInstruct cidnplei = new EditInstruct("SMIDNPL", EditInstruct.UPDATE_OPERATION);
                cidnplei.PutKey("U_ID", uid);
                cidnplei.Put("U_FID", DBNull.Value);
                ml.Add(cidnplei);
                string val = clearUid[uid];
                if (!plShipmentList.Contains(val))
                {
                    plShipmentList.Add(val);
                    EditInstruct plsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    smsmiei.PutKey("SHIPMENT_ID", val);
                    smsmiei.Put("STATUS", "H");
                    smsmiei.Put("BSTATUS", "H");
                    ml.Add(plsmiei);
                }
            }
            return csmUid;
        }


        private static string GetTableValueByName(DataTable dt, string filter, string filename)
        {
            string value = string.Empty;
            if (!dt.Columns.Contains(filename))
                return value;
            DataRow[] drs = dt.Select(filter);
            if (drs.Length > 0)
                value = Prolink.Math.GetValueAsString(drs[0][filename]);
            return value;
        }

        public static void InitCityData()
        {
            string sql = "SELECT CNTRY_CD, PORT_CD, PORT_NM,PORT_NM2 FROM BSCITY WITH(NOLOCK)";
            CityDataTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void rerunConsignment(Dictionary<string, List<string>> rerunCsm,MixedList ml)
        {
            foreach (string key in rerunCsm.Keys)
            {
                string sql = string.Format(@"SELECT QTY,NW,GW,CBM,
MASTER_NO,CNTR_NO,INV_NO FROM SMIDNPL WHERE U_FID = {0} AND U_ID NOT IN {1}",
                    SQLUtils.QuotedStr(key), SQLUtils.Quoted(rerunCsm[key].ToArray()));
                DataTable idnplDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = string.Format("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(key));
                string shipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (idnplDt.Rows.Count < 0)
                {
                    EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.DELETE_OPERATION);
                    smicntrei.PutKey("SHIPMENT_ID", shipmentId);
                    ml.Add(smicntrei);
                    EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.DELETE_OPERATION);
                    smsmiei.PutKey("U_ID", key);
                    ml.Add(smsmiei);
                    EditInstruct smsmiptei = new EditInstruct("SMSMIPT", EditInstruct.DELETE_OPERATION);
                    smsmiptei.PutKey("U_FID", key);
                    ml.Add(smsmiptei);
                }
                else {
                    List<string> cntrList = new List<string>();
                    List<string> blNoList = new List<string>();
                    List<string> invoiceList = new List<string>();
                    decimal sumQty = 0;
                    decimal sumNw = 0;
                    decimal sumGw = 0;
                    decimal sumCbm = 0;
                    if (idnplDt.Rows.Count > 0)
                    {
                        
                        foreach (DataRow dr in idnplDt.Rows)
                        {
                            sumQty += Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["QTY"]);
                            sumNw += Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["NW"]);
                            sumGw += Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["GW"]);
                            sumCbm += Prolink.Math.GetValueAsDecimal(idnplDt.Rows[0]["CBM"]);
                            string cntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                            string invNo = Prolink.Math.GetValueAsString(dr["INV_NO"]);
                            string masterNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                            if (!cntrList.Contains(cntrNo) && !string.IsNullOrEmpty(cntrNo))
                                cntrList.Add(cntrNo);
                            if (!blNoList.Contains(masterNo) && !string.IsNullOrEmpty(masterNo))
                                blNoList.Add(masterNo);
                            if (!invoiceList.Contains(invNo) && !string.IsNullOrEmpty(invNo))
                                invoiceList.Add(invNo);
                        }
                    }
                    EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    smicntrei.PutKey("SHIPMENT_ID", shipmentId);
                    smicntrei.Put("QTY", sumQty);
                    smicntrei.Put("NW", sumNw);
                    ml.Add(smicntrei);
                    EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    smsmiei.PutKey("U_ID", key);
                    smsmiei.Put("PALLET_QTY", sumQty);
                    smsmiei.Put("GW", sumGw);
                    smsmiei.Put("CBM", sumCbm);
                    smsmiei.Put("CNTR_INFO", string.Join(";", cntrList.ToArray()));
                    smsmiei.Put("INVOICE_INFO", string.Join(";", invoiceList.ToArray()));
                    smsmiei.Put("BL_NO_INFO", string.Join(";", blNoList.ToArray()));
                    ml.Add(smsmiei);
                }
            }
        }

    }

    public class ShipmentInfo { 
        public string ShipmentId { get; set; }
        public string Status { get; set; }
    }
}
