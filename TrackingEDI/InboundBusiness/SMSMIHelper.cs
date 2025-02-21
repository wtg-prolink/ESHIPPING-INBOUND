using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Utils;

namespace TrackingEDI.InboundBusiness
{
    public static class SMSMIHelper
    {
        public const string CALL_STATUS_Y_CallTruck = "Y";
        public const string CALL_STATUS_D_CalledTruck = "D";
        public const string CALL_STATUS_R_SlotTimeBooked = "R";
        public const string CALL_STATUS_C_Confirmed = "C";
        public const string CALL_STATUS_A_Arrival = "A";
        public const string CALL_STATUS_I_GateIn = "I";
        public const string CALL_STATUS_G_BerthingInDock = "G";
        public const string CALL_STATUS_U_PODUnloading = "U";
        public const string CALL_STATUS_O_GateOut = "O";
        public const string CALL_STATUS_Z_Finished = "Z";
        public const string CALL_STATUS_X_Cancel = "X";
        public const string CALL_STATUS_T_OntheWay = "T";

        public static void CalDueDate(string ShipmentId)
        {
            DelegateConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection();
            string sql = "SELECT U_ID, SHIPMENT_ID, CNTR_TYPE, DISCHARGE_DATE,PICKUP_CDATE FROM SMICNTR WHERE  SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, conn);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string UId = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    string CntrType = Prolink.Math.GetValueAsString(dr["CNTR_TYPE"]);
                    DateTime DisChargeDate = Prolink.Math.GetValueAsDateTime(dr["DISCHARGE_DATE"]);
                    DateTime PickupDate = Prolink.Math.GetValueAsDateTime(dr["PICKUP_CDATE"]);
                    string Carrier = string.Empty, Location = string.Empty, PodCd = string.Empty, TerminalCd = string.Empty;
                    DateTime Ata = new DateTime();

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

                    sql = "SELECT CARRIER, CMP, POD_CD, TERMINAL_CD, ATA FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, conn);

                    if (sdt.Rows.Count > 0)
                    {
                        Carrier = Prolink.Math.GetValueAsString(sdt.Rows[0]["CARRIER"]);
                        Location = Prolink.Math.GetValueAsString(sdt.Rows[0]["CMP"]);
                        PodCd = Prolink.Math.GetValueAsString(sdt.Rows[0]["POD_CD"]);
                        TerminalCd = Prolink.Math.GetValueAsString(sdt.Rows[0]["TERMINAL_CD"]);
                        Ata = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ATA"]);

                    }
                    DataTable bsDateDt = DayHelper.GetBsdate(Location);

                    sql = "SELECT * FROM SMQTI WHERE CARRIER_CD={0} AND CMP={1} AND POD_CD={2} AND CNT_TYPE={3} AND TERMINAL_CD={4} AND FEE_PER_DAY=0 ORDER BY S_DAY DESC";
                    sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(CntrType), SQLUtils.QuotedStr(TerminalCd));
                    DataTable qdt = getDataTableFromSql(sql);

                    if (qdt.Rows.Count == 0)
                    {
                        sql = "SELECT * FROM SMQTI WHERE CARRIER_CD={0} AND CMP={1} AND POD_CD={2} AND CNT_TYPE={3} AND FEE_PER_DAY=0 ORDER BY S_DAY DESC";
                        sql = string.Format(sql, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(CntrType));
                        qdt = getDataTableFromSql(sql);
                    }

                    if (qdt.Rows.Count > 0)
                    {
                        foreach (DataRow qdr in qdt.Rows)
                        {
                            int EndDay = Prolink.Math.GetValueAsInt(qdr["E_DAY"]);
                            string Itype = Prolink.Math.GetValueAsString(qdr["I_TYPE"]);
                            string chgDayType = Prolink.Math.GetValueAsString(qdr["CHG_DAY_TYPE"]);
                            DateTime DemurrageDueDate = new DateTime();
                            DateTime DetentionDueDate = new DateTime();
                            DateTime StorageDueDate = new DateTime();
                            if (Itype == "DDET")
                            {
                                if (PickupDate.ToString("yyyy-MM-dd") == "0001-01-01")
                                {
                                    continue;
                                }
                                DetentionDueDate = DayHelper.AddWorkHolidays(PickupDate, EndDay, bsDateDt, chgDayType);

                                sql = "UPDATE SMICNTR SET DETENTION_DUE_DATE={0},FACT_CHG_TYPE={2} WHERE U_ID={1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(DetentionDueDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(chgDayType));
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            else if (Itype == "DDEM")
                            {
                                if (DisChargeDate.ToString("yyyy-MM-dd") == "0001-01-01")
                                {
                                    continue;
                                }

                                DemurrageDueDate = DayHelper.AddWorkHolidays(DisChargeDate, EndDay, bsDateDt, chgDayType);

                                sql = "UPDATE SMICNTR SET DEMURRAGE_DUE_DATE={0},PORT_CHG_TYPE={2} WHERE U_ID={1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(DemurrageDueDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(chgDayType));
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            else if (Itype == "BOTH")
                            {
                                if (Ata.ToString("yyyy-MM-dd") == "0001-01-01")
                                {
                                    continue;
                                }
                                DetentionDueDate = DayHelper.AddWorkHolidays(Ata, EndDay, bsDateDt, chgDayType);
                                sql = "UPDATE SMICNTR SET DETENTION_DUE_DATE={0},FACT_CHG_TYPE={2} WHERE U_ID={1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(DetentionDueDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(chgDayType));
                                OperationUtils.ExecuteUpdate(sql, conn);

                                if (DisChargeDate.ToString("yyyy-MM-dd") == "0001-01-01")
                                {
                                    continue;
                                }

                                DemurrageDueDate = DayHelper.AddWorkHolidays(DisChargeDate, EndDay, bsDateDt, chgDayType);

                                sql = "UPDATE SMICNTR SET DEMURRAGE_DUE_DATE={0},PORT_CHG_TYPE={2} WHERE U_ID={1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(DemurrageDueDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(chgDayType));
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            else if (Itype == "DSTF")
                            {
                                if (DisChargeDate.ToString("yyyy-MM-dd") == "0001-01-01")
                                {
                                    continue;
                                }

                                StorageDueDate = DayHelper.AddWorkHolidays(DisChargeDate, EndDay, bsDateDt, chgDayType);

                                sql = "UPDATE SMICNTR SET STORAGE_DUE_DATE={0},CON_CHG_TYPE={2} WHERE U_ID={1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(StorageDueDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(UId),SQLUtils.QuotedStr(chgDayType));
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                        }
                    }
                }
            }


            sql = "SELECT U_ID, SHIPMENT_ID, DISCHARGE_DATE FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            dt = OperationUtils.GetDataTable(sql, null, conn);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string UId = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    string CntrType = string.Empty;
                    DateTime DisChargeDate = Prolink.Math.GetValueAsDateTime(dr["DISCHARGE_DATE"]);
                    string Carrier = string.Empty, Location = string.Empty, PodCd = string.Empty, CntType = string.Empty, TerminalCd = string.Empty;
                    DateTime Ata = new DateTime();

                    sql = "SELECT CARRIER, CMP, POD_CD, TERMINAL_CD, ATA FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, conn);

                    if (sdt.Rows.Count > 0)
                    {
                        Carrier = Prolink.Math.GetValueAsString(sdt.Rows[0]["CARRIER"]);
                        Location = Prolink.Math.GetValueAsString(sdt.Rows[0]["CMP"]);
                        PodCd = Prolink.Math.GetValueAsString(sdt.Rows[0]["POD_CD"]);
                        TerminalCd = Prolink.Math.GetValueAsString(sdt.Rows[0]["TERMINAL_CD"]);
                        Ata = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ATA"]);
                    }

                    sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TERMINAL_CD={2} AND FEE_PER_DAY=0 AND I_TYPE='DSTF' ORDER BY S_DAY DESC";
                    sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(TerminalCd));
                    DataTable qdt = getDataTableFromSql(sql);

                    if (qdt.Rows.Count == 0)
                    {
                        sql = "SELECT * FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND FEE_PER_DAY=0 AND I_TYPE='DSTF' ORDER BY S_DAY DESC";
                        sql = string.Format(sql, SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(PodCd));
                        qdt = getDataTableFromSql(sql);
                    }

                    if (qdt.Rows.Count > 0)
                    {
                        foreach (DataRow qdr in qdt.Rows)
                        {
                            int EndDay = Prolink.Math.GetValueAsInt(qdr["E_DAY"]);
                            string Itype = Prolink.Math.GetValueAsString(qdr["I_TYPE"]);
                            string chgDayType = Prolink.Math.GetValueAsString(qdr["CHG_DAY_TYPE"]);
                            DateTime StorageDueDate = new DateTime();
                            if (Itype == "DSTF")
                            {
                                if (DisChargeDate.ToString("yyyy-MM-dd") == "0001-01-01")
                                {
                                    continue;
                                }
                                DataTable bsDateDt = DayHelper.GetBsdate(Location);
                                StorageDueDate = DayHelper.AddWorkHolidays(DisChargeDate, EndDay, bsDateDt, chgDayType);

                                sql = "UPDATE SMIDN SET STORAGE_DUE_DATE={0} WHERE U_ID={1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(StorageDueDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(UId));
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                        }
                    }
                }
            }
        }

        public static DataTable getDataTableFromSql(string sql)
        {
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void CreateSubData(string trantype, string shipmentid, string reserveno, MixedList mixList, string invoiceno, string ws_cd, string cntr_no, string GroupId, string CompanyId, string ShipperNm, string Shipper, string UserId)
        {
            string dlv_area = string.Empty;
            string dlv_area_nm = string.Empty;
            string dlv_addr = string.Empty;
            string addrname = string.Empty; 
            string sql = string.Format("SELECT DLV_AREA,DLV_AREA_NM,DLV_ADDR,WS_CD,DLV_ADDR_NM FROM SMWH WHERE WS_CD={0} AND CMP={1}",
                            SQLUtils.QuotedStr(ws_cd), SQLUtils.QuotedStr(CompanyId));
            DataTable dt = getDataTableFromSql(sql);
            if (dt.Rows.Count > 0)
            {
                dlv_area = dt.Rows[0]["DLV_AREA"].ToString();
                dlv_area_nm = dt.Rows[0]["DLV_AREA_NM"].ToString();
                dlv_addr = dt.Rows[0]["DLV_ADDR"].ToString();
                addrname = dt.Rows[0]["DLV_ADDR_NM"].ToString();
            }
            if (!string.IsNullOrEmpty(invoiceno))
            {
                invoiceno = invoiceno.Replace(';', ',').Replace('，', ',');
                string[] invoices = invoiceno.Split(',');
                foreach (string invoice in invoices)
                {
                    if (string.IsNullOrEmpty(invoice))
                        continue;
                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        EditInstruct invei = new EditInstruct("SMICNTR", EditInstruct.INSERT_OPERATION);
                        invei.Put("U_ID", Guid.NewGuid().ToString());
                        invei.Put("SHIPMENT_ID", shipmentid);
                        invei.Put("CNTR_NO", cntr_no);
                        invei.Put("DN_NO", invoice);
                        invei.Put("GROUP_ID", GroupId);
                        invei.Put("DLV_AREA", dlv_area);
                        invei.Put("DLV_AREA_NM", dlv_area_nm);
                        invei.Put("ADDR_CODE", dlv_addr);
                        invei.Put("DLV_ADDR", addrname);
                        invei.Put("CMP", CompanyId);
                        mixList.Add(invei);

                        EditInstruct rdnei = new EditInstruct("SMRCNTR", EditInstruct.INSERT_OPERATION);
                        rdnei.Put("U_ID", Guid.NewGuid().ToString());
                        rdnei.Put("SHIPMENT_ID", shipmentid);
                        rdnei.Put("RESERVE_NO", reserveno);
                        rdnei.Put("ORD_NO", reserveno);
                        rdnei.Put("CNTR_NO", cntr_no);
                        rdnei.Put("WS_CD", ws_cd);
                        rdnei.Put("DLV_AREA", dlv_area);
                        rdnei.Put("DLV_AREA_NM", dlv_area_nm);
                        rdnei.Put("ADDR_CODE", dlv_addr);
                        rdnei.Put("DLV_ADDR", addrname);
                        rdnei.Put("DN_NO", invoice);
                        rdnei.Put("GROUP_ID", GroupId);
                        rdnei.Put("CMP", CompanyId);
                        mixList.Add(rdnei);
                    }
                    else
                    {
                        EditInstruct invei = new EditInstruct("SMIDN", EditInstruct.INSERT_OPERATION);
                        invei.Put("U_ID", Guid.NewGuid().ToString());
                        invei.Put("SHIPMENT_ID", shipmentid);
                        invei.Put("INV_NO", invoice);
                        invei.Put("DN_NO", invoice);
                        invei.Put("WS_CD", ws_cd);
                        invei.Put("DLV_AREA", dlv_area);
                        invei.Put("DLV_AREA_NM", dlv_area_nm);
                        invei.Put("ADDR_CODE", dlv_addr);
                        invei.Put("DLV_ADDR", addrname);
                        invei.Put("GROUP_ID", GroupId);
                        invei.Put("CMP", CompanyId);
                        mixList.Add(invei);

                        EditInstruct rdnei = new EditInstruct("SMRDN", EditInstruct.INSERT_OPERATION);
                        rdnei.Put("U_ID", Guid.NewGuid().ToString());
                        rdnei.Put("SHIPMENT_ID", shipmentid);
                        rdnei.Put("RESERVE_NO", reserveno);
                        rdnei.Put("WS_CD", ws_cd);
                        rdnei.Put("DLV_AREA", dlv_area);
                        rdnei.Put("DLV_AREA_NM", dlv_area_nm);
                        rdnei.Put("ADDR_CODE", dlv_addr);
                        rdnei.Put("DLV_ADDR", addrname);
                        rdnei.Put("INV_NO", invoice);
                        rdnei.Put("GROUP_ID", GroupId);
                        rdnei.Put("CMP", CompanyId);
                        mixList.Add(rdnei);
                    }
                }
            }


            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.INSERT_OPERATION);
            smsmiei.Put("U_ID", Guid.NewGuid().ToString());
            smsmiei.Put("SHIPMENT_ID", shipmentid);
            smsmiei.Put("GROUP_ID", GroupId);
            smsmiei.Put("CMP", CompanyId);
            smsmiei.Put("INVOICE_INFO", invoiceno);
            //smsmiei.Put("COMBINE_INFO",dnno);
            smsmiei.Put("CNTR_INFO", cntr_no);
            smsmiei.Put("STATUS", "Z");
            smsmiei.Put("SH_NO", Shipper);
            smsmiei.Put("SH_NM", ShipperNm);
            smsmiei.Put("TRAN_TYPE", trantype);
            smsmiei.PutDate("CREATE_DATE", DateTime.Now);
            smsmiei.PutDate("CREATE_DATE_L", TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId));
            smsmiei.Put("CREATE_BY",UserId);
            mixList.Add(smsmiei);
        }

        public static string TSATransloading(string reserveno, string[] reserveArry, out MixedList mlist, out string combineReserveno, IBUserInfo userinfo)
        {
            mlist = new MixedList();
            EditInstruct smrvei;
            List<string> cntrlist = new List<string>();
            List<string> dnlist = new List<string>();
            List<string> shipmentlist = new List<string>();
            combineReserveno = string.Empty;
            string sql = string.Format("SELECT DN_NO,SHIPMENT_ID,WS_CD,DLV_AREA,DLV_AREA_NM,ADDR_CODE,DLV_ADDR,DISCHARGE_DATE,REL_DATE,EMPTY_TIME,CNTR_NO FROM SMRCNTR WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserveno));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return "This Appointment " + reserveno + " doesn't have container!";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string cntrindex = dt.Rows[i]["CNTR_NO"].ToString();
                string dnindex = dt.Rows[i]["DN_NO"].ToString();
                string shipmentdex = dt.Rows[i]["SHIPMENT_ID"].ToString();
                if (!cntrlist.Contains(cntrindex))
                    cntrlist.Add(cntrindex);
                if (!dnlist.Contains(dnindex))
                    dnlist.Add(dnindex);
                if (!shipmentlist.Contains(shipmentdex))
                    shipmentlist.Add(shipmentdex);
            }

            sql = string.Format("SELECT * FROM SMRCNTR WHERE RESERVE_NO IN {0} ", SQLUtils.Quoted(reserveArry));
            DataTable smcntrdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMIRV WHERE RESERVE_NO ={0}", SQLUtils.QuotedStr(reserveno));
            DataTable smrvdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow dr = dt.Rows[0];
            string mainuid = System.Guid.NewGuid().ToString();
            if (userinfo.IoFlag == "O")
            {
                combineReserveno = "C" + ReserveHelper.getAutoNo("RV_NO", userinfo.GroupId, userinfo.BaseCompanyId);
            }
            else
            {
                combineReserveno = "C" + ReserveHelper.getAutoNo("RV_NO", userinfo.GroupId, userinfo.CompanyId);
            }
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm.Add("CREATE_BY", userinfo.UserId);
            parm.Add("RESERVE_NO", combineReserveno);
            parm.Add("ORD_NO", combineReserveno);
            parm.Add("IS_COMBINE_DP", "C");
            parm.Add("STATUS", "T");
            Dictionary<string, string> fielsparm = new Dictionary<string, string>();
            fielsparm.Add("OLD_RESERVE_NO", "RESERVE_NO");
            fielsparm.Add("OLD_ORD_NO", "ORD_NO");

            ReserveHelper.ToEi(smcntrdt, "SMRCNTR", mlist, mainuid, parm, fielsparm);
            ReserveHelper.ToEi(smrvdt, "SMIRV", mlist, mainuid, parm, fielsparm);
            foreach (DataRow smrdr in smcntrdt.Rows)
            {
                if (!cntrlist.Contains(smrdr["CNTR_NO"].ToString()) && !string.IsNullOrEmpty(smrdr["CNTR_NO"].ToString()))
                    cntrlist.Add(smrdr["CNTR_NO"].ToString());
                if (!dnlist.Contains(smrdr["DN_NO"].ToString()) && !string.IsNullOrEmpty(smrdr["DN_NO"].ToString()))
                    dnlist.Add(smrdr["DN_NO"].ToString());
                if (!shipmentlist.Contains(smrdr["SHIPMENT_ID"].ToString()) && !string.IsNullOrEmpty(smrdr["SHIPMENT_ID"].ToString()))
                    shipmentlist.Add(smrdr["SHIPMENT_ID"].ToString());
            }
            smrvei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            smrvei.PutKey("RESERVE_NO", combineReserveno);
            smrvei.Put("CNTR_NO", string.Join(",", cntrlist));
            smrvei.Put("DN_NO", string.Join(",", dnlist));
            smrvei.Put("SHIPMENT_INFO", string.Join(",", shipmentlist));
            mlist.Add(smrvei);

            EditInstruct upei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            upei.Condition = string.Format("RESERVE_NO IN {0} OR RESERVE_NO={1}", SQLUtils.Quoted(reserveArry), SQLUtils.QuotedStr(reserveno));
            upei.Put("IS_COMBINE_DP", "S");
            mlist.Add(upei);
            return string.Empty;
        }

        public static MixedList TSAAddContainerInRV(string reserveno, string reservenos, out string combineReserveno,   IBUserInfo userinfo)
        {
            combineReserveno = string.Empty;
            string[] reserveArry = reservenos.Split(',');
            MixedList mlist = new MixedList();
            EditInstruct smrvei;
            List<string> cntrlist = new List<string>();
            List<string> dnlist = new List<string>();
            List<string> shipmentlist = new List<string>();
            string sql = string.Format("SELECT DN_NO,SHIPMENT_ID,WS_CD,DLV_AREA,DLV_AREA_NM,ADDR_CODE,DLV_ADDR,DISCHARGE_DATE,REL_DATE,EMPTY_TIME,CNTR_NO FROM SMRCNTR WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserveno));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return mlist;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string cntrindex = dt.Rows[i]["CNTR_NO"].ToString();
                string dnindex = dt.Rows[i]["DN_NO"].ToString();
                string shipmentdex = dt.Rows[i]["SHIPMENT_ID"].ToString();
                if (!cntrlist.Contains(cntrindex))
                    cntrlist.Add(cntrindex);
                if (!dnlist.Contains(dnindex))
                    dnlist.Add(dnindex);
                if (!shipmentlist.Contains(shipmentdex))
                    shipmentlist.Add(shipmentdex);
            }

            sql = string.Format("SELECT * FROM SMRCNTR WHERE RESERVE_NO IN {0} AND CNTR_NO NOT IN {1}", SQLUtils.Quoted(reserveArry), SQLUtils.Quoted(cntrlist.ToArray()));
            DataTable smcntrdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMIRV WHERE RESERVE_NO ={0}", SQLUtils.QuotedStr(reserveno));
            DataTable smrvdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow dr = dt.Rows[0];
            string mainuid = System.Guid.NewGuid().ToString();

            string iscombine = smrvdt.Rows[0]["IS_COMBINE_DP"].ToString();
            combineReserveno = reserveno;
            if ("!C".Equals(iscombine))
                combineReserveno = "C" + ReserveHelper.getAutoNo("RV_NO", userinfo.GroupId, userinfo.CompanyId);
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm.Add("CREATE_BY", userinfo.UserId);
            parm.Add("ORD_NO", combineReserveno);
            parm.Add("RESERVE_NO", combineReserveno);
            parm.Add("WS_CD", dr["WS_CD"].ToString());
            parm.Add("DLV_AREA", dr["DLV_AREA"].ToString());
            parm.Add("DLV_AREA_NM", dr["DLV_AREA_NM"].ToString());
            parm.Add("ADDR_CODE", dr["ADDR_CODE"].ToString());
            parm.Add("DLV_ADDR", dr["DLV_ADDR"].ToString());
            parm.Add("IS_COMBINE_DP", "C");
            parm.Add("STATUS", "R");

            Dictionary<string, string> fielsparm = new Dictionary<string, string>();
            fielsparm.Add("OLD_RESERVE_NO", "RESERVE_NO");
            fielsparm.Add("OLD_ORD_NO", "ORD_NO");

            if (dr["DISCHARGE_DATE"] != null && dr["DISCHARGE_DATE"] != DBNull.Value)
            {
                parm.Add("DISCHARGE_DATE", (DateTime)dr["DISCHARGE_DATE"]);
            }
            if (dr["REL_DATE"] != null && dr["REL_DATE"] != DBNull.Value)
            {
                parm.Add("REL_DATE", (DateTime)dr["REL_DATE"]);
            }
            if (dr["EMPTY_TIME"] != null && dr["EMPTY_TIME"] != DBNull.Value)
            {
                parm.Add("EMPTY_TIME", (DateTime)dr["EMPTY_TIME"]);
            }

            ReserveHelper.ToEi(smcntrdt, "SMRCNTR", mlist, mainuid, parm, fielsparm);
            if ("!C".Equals(iscombine))
                ReserveHelper.ToEi(smrvdt, "SMIRV", mlist, mainuid, parm, fielsparm);
            foreach (DataRow smrdr in smcntrdt.Rows)
            {
                if (!cntrlist.Contains(smrdr["CNTR_NO"].ToString()) && !string.IsNullOrEmpty(smrdr["CNTR_NO"].ToString()))
                    cntrlist.Add(smrdr["CNTR_NO"].ToString());
                if (!dnlist.Contains(smrdr["DN_NO"].ToString()) && !string.IsNullOrEmpty(smrdr["DN_NO"].ToString()))
                    dnlist.Add(smrdr["DN_NO"].ToString());
                if (!shipmentlist.Contains(smrdr["SHIPMENT_ID"].ToString()) && !string.IsNullOrEmpty(smrdr["SHIPMENT_ID"].ToString()))
                    shipmentlist.Add(smrdr["SHIPMENT_ID"].ToString());
            }
            smrvei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            smrvei.PutKey("RESERVE_NO", combineReserveno);
            smrvei.Put("CNTR_NO", string.Join(",", cntrlist));
            smrvei.Put("DN_NO", string.Join(",", dnlist));
            smrvei.Put("SHIPMENT_INFO", string.Join(",", shipmentlist));
            smrvei.Put("IS_COMBINE_DP", "C");
            mlist.Add(smrvei);

            EditInstruct upei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            upei.Condition = string.Format("RESERVE_NO IN {0}", SQLUtils.Quoted(reserveArry));
            upei.Put("IS_COMBINE_DP", "S");
            mlist.Add(upei);
            return mlist;
        }

        public static void ReBuildOrderIndex(Dictionary<string, object> json, MixedList mixList, List<string> returnMsg, List<string> ordnos, IBUserInfo userinfo)
        {
            string OrdNo = json["OrdNo"].ToString();
            string PodCd = json["DestCd"].ToString();
            string PodNm = json["DestName"].ToString();
            string AddrCode = json["AddrCode"].ToString();
            string Addr = json["Addr"].ToString();
            string Trucker1 = json["CustermCode1"].ToString();
            string TruckerNm1 = json["CustermName1"].ToString();
            string ArrivalDate = json["ArrivalDate1"].ToString();
            string sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
            DataTable dt = getDataTableFromSql(sql);
            if (dt.Rows.Count <= 0)
            {
                returnMsg.Add("OrdNo:" + OrdNo + " Not data");
                return;
            }
            string cstatus=Prolink.Math.GetValueAsString(dt.Rows[0]["CSTATUS"]);
            if (!"T".Equals(cstatus))
            {
                returnMsg.Add("OrdNo:" + OrdNo + " Status is not On the way!!!");
                return;
            }
            string groupid = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            string Cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string Pol1 = string.Empty;
            string PolNm1 = string.Empty;
            string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
            string shipmentuid = OperationUtils.GetValueAsString("SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid), Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "SELECT * FROM SMRCNTR WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
            DataTable cntrdt = getDataTableFromSql(sql);
            sql = "SELECT * FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
            DataTable smdndt = getDataTableFromSql(sql);
            if (cntrdt.Rows.Count > 0)
            {
                Pol1 = Prolink.Math.GetValueAsString(cntrdt.Rows[0]["DLV_AREA"]);
                PolNm1 = Prolink.Math.GetValueAsString(cntrdt.Rows[0]["DLV_AREA_NM"]);
            }
            else
            {
                if (smdndt.Rows.Count > 0)
                {
                    Pol1 = Prolink.Math.GetValueAsString(smdndt.Rows[0]["DLV_AREA"]);
                    PolNm1 = Prolink.Math.GetValueAsString(smdndt.Rows[0]["DLV_AREA_NM"]);
                }
            }
            string NewOrdNo = ReserveHelper.getAutoNo("ORD_NO", groupid, Cmp);
            ordnos.Add(NewOrdNo);
            string mainuid = System.Guid.NewGuid().ToString();
            Dictionary<string, object> parm = new Dictionary<string, object>();
            //parm.Add("CREATE_DATE", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            parm.Add("CREATE_BY",userinfo.UserId);
            parm.Add("POL1", Pol1);
            parm.Add("POL_NM1", PolNm1);
            parm.Add("POD1", PodCd);
            parm.Add("POD_NM1", PodNm);
            parm.Add("TRAN_TYPE1", "T");
            parm.Add("TRUCKER1", Trucker1);
            parm.Add("TRUCKER_NM1", TruckerNm1);
            parm.Add("ORD_NO", NewOrdNo);
            parm.Add("TRAN_NO", "1");
            parm.Add("CSTATUS", "Y");
            parm.Add("PLANT", "");
            ReserveHelper.ToEi(dt, "SMORD", mixList, mainuid, parm);
            //SetIBCRToSMSMIPTNoHave(mixList, shipmentuid, shipmentid, Trucker1);
            string sql1 = string.Format(@"SELECT COUNT(1) FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE = 'IBCR' AND
             PARTY_NO={1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(Trucker1));
            int count = OperationUtils.GetValueAsInt(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (count <= 0)
            {
                SetPartyToSMSMIPT(shipmentuid, shipmentid, mixList, Trucker1, "IBCR");
            }
            string wscdsql = string.Format("SELECT TOP 1 WS_CD FROM SMWH WHERE DLV_ADDR={0} AND CMP={1} ", SQLUtils.QuotedStr(AddrCode),
                           SQLUtils.QuotedStr(userinfo.CompanyId));
            string wscd = OperationUtils.GetValueAsString(wscdsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> smrdnParm = new Dictionary<string, object>();
            smrdnParm.Add("DLV_AREA", PodCd);
            smrdnParm.Add("DLV_AREA_NM", PodNm);
            smrdnParm.Add("PICK_AREA", Pol1);
            smrdnParm.Add("PICK_AREA_NM", PolNm1);
            smrdnParm.Add("ADDR_CODE", AddrCode);
            smrdnParm.Add("DLV_ADDR", Addr);
            smrdnParm.Add("ORD_NO", NewOrdNo);
            smrdnParm.Add("WS_CD", wscd);
            smrdnParm.Add("RESERVE_NO", "");
            smrdnParm.Add("POD_MDATE_L", DBNull.Value); //清空对应的日期
            smrdnParm.Add("POD_MDATE", DBNull.Value);
            smrdnParm.Add("PICKUP_DATE", DBNull.Value);
            smrdnParm.Add("FINAL_WH", "Final");
            ArrivalDate = ArrivalDate.Trim();
            if (!string.IsNullOrEmpty(ArrivalDate))
            {
                DateTime arrivalDate = Prolink.Math.GetValueAsDateTime(ArrivalDate);
                smrdnParm.Add("ARRIVAL_DATE", arrivalDate.ToString("yyyyMMdd"));
            }
            else
            {
                smrdnParm.Add("ARRIVAL_DATE", DBNull.Value);
            }
            ReserveHelper.ToEi(smdndt, "SMRDN", mixList, mainuid, smrdnParm);
            ReserveHelper.ToEi(cntrdt, "SMRCNTR", mixList, mainuid, smrdnParm);
            sql = @"UPDATE SMORD SET CSTATUS='O' WHERE ORD_NO={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(OrdNo));
            mixList.Add(sql);
        }

        /// <summary>
        /// 将对应的party Type以及Party No生成EditInstruct 加入到MixedList中
        /// </summary>
        /// <param name="shipmentuid">shipmentuid</param>
        /// <param name="ShipmentId">ShipmentId</param>
        /// <param name="ml">MixedList</param>
        /// <param name="PartyNo">PartyNo</param>
        /// <param name="PartyType">PartyType</param>
        public static void SetPartyToSMSMIPT(string shipmentuid, string ShipmentId, MixedList ml, string PartyNo, string PartyType)
        {
            string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD={0}", SQLUtils.QuotedStr(PartyType));
            string TypeDescp=OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = @"INSERT INTO SMSMIPT
                                    (
                                        U_ID,
                                        U_FID,
                                        SHIPMENT_ID,
                                        PARTY_TYPE,
                                        TYPE_DESCP,
                                        PARTY_NO,
                                        PARTY_NAME,
                                        PARTY_NAME2,
                                        PARTY_NAME3,
                                        PARTY_NAME4,
                                        PARTY_ADDR1,
                                        PARTY_ADDR2,
                                        PARTY_ADDR3,
                                        PARTY_ADDR4,
                                        PARTY_ADDR5,
                                        CNTY,
                                        CNTY_NM,
                                        CITY,
                                        CITY_NM,
                                        STATE,
                                        ZIP,
                                        PARTY_ATTN,
                                        PARTY_TEL,
                                        PARTY_MAIL,
                                        DEBIT_TO,
                                        FAX_NO,
                                        TAX_NO
                                    )
                                    SELECT 
                                    {0},
                                    {1},
                                    {2},
                                    {3},
                                    {4},
                                    PARTY_NO,
                                    PARTY_NAME,
                                    PARTY_NAME2,
                                    PARTY_NAME3,
                                    PARTY_NAME4,
                                    PART_ADDR1,
                                    PART_ADDR2,
                                    PART_ADDR3,
                                    PART_ADDR4,
                                    PART_ADDR5,
                                    CNTY,
                                    CNTY_NM,
                                    CITY,
                                    CITY_NM,
                                    STATE,
                                    ZIP,
                                    PARTY_ATTN,
                                    PARTY_TEL,
                                    PARTY_MAIL,
                                    BILL_TO,
                                    PARTY_FAX,
                                    TAX_NO
                                    FROM SMPTY WHERE STATUS='U' AND PARTY_NO={5}";
            string ptuid = System.Guid.NewGuid().ToString();
            sql = string.Format(sql, SQLUtils.QuotedStr(ptuid), SQLUtils.QuotedStr(shipmentuid),
                SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(PartyType), SQLUtils.QuotedStr(TypeDescp), SQLUtils.QuotedStr(PartyNo));
            ml.Add(sql);
        }


        public static IBResultInfo InboundFclOrderTrucker(DataRow odr, DataRow cdr, string PickupDate, List<string> EtaMsl, string ArrivalDate, string WsCd, string UserId, string Ext, string QuotNo = "", List<string> idList = null, List<string> newReserveNo = null, Dictionary<string, string> parm = null)
        {
            IBResultInfo resultinfo = new IBResultInfo();
            string sql = "";

            string GroupId = odr["GROUP_ID"].ToString();
            string CompanyId = odr["CMP"].ToString();
            string Dep = odr["DEP"].ToString();
            string DepAddr = odr["DEP_ADDR"].ToString();
            string Trucker = odr["TRUCKER1"].ToString();
            string TruckerNm = odr["TRUCKER_NM1"].ToString();
            string ShipmentId = odr["SHIPMENT_ID"].ToString();
            string TranType = odr["TRAN_TYPE1"].ToString();
            string SmCreateBy = odr["IB_WINDOW"].ToString();
            string secCmp = odr["SEC_CMP"].ToString();
            int ConFreeTime = Prolink.Math.GetValueAsInt(odr["CON_FREE_TIME"]);
            string isCombineDet = Prolink.Math.GetValueAsString(odr["COMBINE_DET"]);
            string InvoiceInfo = Prolink.Math.GetValueAsString(odr["INVOICE_INFO"]);
            

            string OrdNo = cdr["ORD_NO"].ToString();
            string DnNo = cdr["DN_NO"].ToString();
            string CntType = cdr["CNTR_TYPE"].ToString();
            string Pol1 = odr["POL1"].ToString();
            string PolNm1 = odr["POL_NM1"].ToString();
            string Pod1 = odr["POD1"].ToString();
            string PodNm1 = odr["POD_NM1"].ToString();
            string CntrNo = cdr["CNTR_NO"].ToString();
            string isdirectlynb = odr["IS_DIRECTLYNB"].ToString();
            string addrcode = cdr["ADDR_CODE"].ToString();
            string CarType = string.Empty;
            string PoNo = string.Empty;
            string Wo = string.Empty;
            string DivDescp = string.Empty;
            DateTime ata = Prolink.Math.GetValueAsDateTime(odr["ATA"]);
            string partInfo = Prolink.Math.GetValueAsString(odr["PARTNO_INFO"]);
            string partQty = Prolink.Math.GetValueAsString(odr["PART_QTY"]);
            string asnInfo = Prolink.Math.GetValueAsString(odr["ASNNO_INFO"]);
            string csCd = Prolink.Math.GetValueAsString(odr["CS_CD"]);
            string csNm = Prolink.Math.GetValueAsString(odr["CS_NM"]);
            string csName = Prolink.Math.GetValueAsString(odr["CS_NAME"]);
            string bu = Prolink.Math.GetValueAsString(odr["BU"]);
            string DecInfo = string.Empty, NewSeal = string.Empty, backlc = string.Empty, scmdate = string.Empty, SealNo1 = string.Empty;
            sql = "SELECT NEW_SEAL,DEC_NO,BACK_LOCATION,SCMREQUEST_DATE,SEAL_NO1,PO_NO,DN_NO,DIVISION_DESCP,WO,INV_NO FROM SMICNTR WHERE SHIPMENT_ID={0} AND CNTR_NO={1} ORDER BY SCMREQUEST_DATE ASC";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(CntrNo));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string invNos = string.Empty;
            if (dt.Rows.Count > 0)
            {
                DecInfo = dt.Rows[0]["DEC_NO"].ToString();
                NewSeal = dt.Rows[0]["NEW_SEAL"].ToString();
                backlc = dt.Rows[0]["BACK_LOCATION"].ToString();
                scmdate = dt.Rows[0]["SCMREQUEST_DATE"].ToString();
                SealNo1 = dt.Rows[0]["SEAL_NO1"].ToString();
                DivDescp = dt.Rows[0]["DIVISION_DESCP"].ToString();
                foreach (DataRow dr in dt.Rows)
                {
                    string PN = Prolink.Math.GetValueAsString(dr["PO_NO"]);
                    PoNo += PN + ",";
                    string invNo_i = Prolink.Math.GetValueAsString(dr["INV_NO"]);
                    invNos += invNo_i + ",";
                    string wo = Prolink.Math.GetValueAsString(dr["WO"]);
                    Wo += wo + ",";
                }
                PoNo = PoNo.Remove(PoNo.Length - 1);
                invNos = invNos.Remove(invNos.Length - 1);
                Wo = Wo.Remove(Wo.Length - 1);
            }

            if (CntType == "20GP")
            {
                CarType = "F4";
            }
            else if (CntType == "40GP")
            {
                CarType = "F5";
            }
            else if (CntType == "40HQ")
            {
                CarType = "F6";
            }
            if (string.IsNullOrEmpty(CarType))
            {
                string rnfsql = string.Format("SELECT CHG_CD FROM ECREFFEE WHERE (CMP ={0} OR CMP = '*') AND FEE_TYPE = 'O' AND CHG_CD NOT IN('F1','F2','F3') AND CHG_DESCP={1}",
                     SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(CntType));
                CarType = OperationUtils.GetValueAsString(rnfsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            decimal Gw = Prolink.Math.GetValueAsDecimal(cdr["GW"]);
            string Gwu = cdr["GWU"].ToString();
            decimal gu = Convert.ToDecimal(Prolink.Math.unitConvert(Gwu, "KG"));
            Gw = Gw * gu;
            decimal Cbm = Prolink.Math.GetValueAsDecimal(cdr["CBM"]);

            
            MixedList mixList = new MixedList();
            bool isupdate = false;

            EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            smordei.PutKey("ORD_NO", OrdNo);

            EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
            smicntrei.PutKey("SHIPMENT_ID", ShipmentId);
            smicntrei.PutKey("CNTR_NO", CntrNo);
            if (!string.IsNullOrEmpty(PickupDate))
            {
                smicntrei.PutDate("PICKUP_CDATE", Prolink.Math.GetValueAsDateTime(PickupDate));
            }

            if (EtaMsl.Count > 0 && !string.IsNullOrEmpty(EtaMsl[0]))
            {
                smordei.PutDate("ETA_MSL", Prolink.Math.GetValueAsDateTime(EtaMsl[0]));
                smicntrei.PutDate("ETA_MSL", Prolink.Math.GetValueAsDateTime(EtaMsl[0]));
                isupdate = true;
            }
            if (EtaMsl.Count > 1 && !string.IsNullOrEmpty(EtaMsl[1]))
            {
                smordei.Put("ETA_MSL_TIME", Prolink.Math.GetValueAsString(EtaMsl[1]));
                smicntrei.Put("ETA_MSL_TIME", Prolink.Math.GetValueAsString(EtaMsl[1]));
                isupdate = true;
            }

            if (isupdate)
            {
                mixList.Add(smordei);
            }

            EditInstruct smordei1 = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            smordei1.PutKey("ORD_NO", OrdNo);
            if (!string.IsNullOrEmpty(PickupDate))
            {
                smordei1.PutDate("PICKUP_CDATE", Prolink.Math.GetValueAsDateTime(PickupDate));
            }

            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
            string UId = System.Guid.NewGuid().ToString();
            string ReserveNo = ReserveHelper.getAutoNo("RV_NO", GroupId, CompanyId);
            ei.Put("U_ID", UId);
            ei.Put("RESERVE_NO", ReserveNo);
            ei.Put("ORD_NO", OrdNo);
            //ei.PutExpress("ATD", string.Format("(SELECT TOP 1 ATD FROM SMSMI WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(ShipmentId)));

            if (IsTSALogistic(CompanyId))
            {
                string bsaddrspl = string.Format("SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WHERE CMP={0} AND BSADDR.ADDR_CODE = {1}", SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(addrcode));
                DataTable addrdt = OperationUtils.GetDataTable(bsaddrspl, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (addrdt.Rows.Count > 0)
                {
                    string finawh = Prolink.Math.GetValueAsString(addrdt.Rows[0]["FINAL_WH"]);
                    string outerflag = Prolink.Math.GetValueAsString(addrdt.Rows[0]["OUTER_FLAG"]);
                    if ("Temp".Equals(finawh) && "Y".Equals(outerflag))
                    {
                        ei.Put("STATUS", 'R');
                        smicntrei.Put("CALL_TRUCK_STATUS", 'R');
                        smordei1.Put("CSTATUS", 'R');
                    }
                    else
                    {
                        ei.Put("STATUS", 'D');
                        smicntrei.Put("CALL_TRUCK_STATUS", 'D');
                        smordei1.Put("CSTATUS", 'D');
                    }
                }
                else
                {
                    ei.Put("STATUS", 'D');
                    smicntrei.Put("CALL_TRUCK_STATUS", 'D');
                    smordei1.Put("CSTATUS", 'D');
                }
            }
            else if ("Y".Equals(isdirectlynb))
            {
                ei.Put("STATUS", 'R');
                smicntrei.Put("CALL_TRUCK_STATUS", 'R');
                smordei1.Put("CSTATUS", 'R');
            }
            else
            {
                ei.Put("STATUS", 'D');
                smicntrei.Put("CALL_TRUCK_STATUS", 'D');
                smordei1.Put("CSTATUS", 'D');
            }
            mixList.Add(smicntrei);
            mixList.Add(smordei1); 

            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", CompanyId);
            ei.Put("DEP_ADDR", DepAddr);
            ei.Put("DN_NO", invNos);
            ei.Put("PRODUCT_TYPE", DivDescp);
            ei.Put("DEP", Dep);
            ei.Put("CREATE_BY", UserId);
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

            ei.PutDate("CREATE_DATE", odt.ToString("yyyyMMddHHmmss"));
            ei.PutDate("CREATE_DATE_L", ndt.ToString("yyyyMMddHHmmss"));
            ei.Put("CREATE_CMP", CompanyId);
            ei.Put("CREATE_DEP", Dep);
            ei.Put("CREATE_EXT", Ext);
            ei.PutDate("CALL_DATE", odt.ToString("yyyyMMddHHmmss"));
            ei.PutDate("CALL_DATE_L", ndt.ToString("yyyyMMddHHmmss"));
            ei.Put("TRUCKER", Trucker);
            ei.Put("SHIPMENT_ID", ShipmentId);
            if (!string.IsNullOrEmpty(PickupDate))
            {
                ei.PutDate("USE_DATE", Prolink.Math.GetValueAsDateTime(PickupDate));
            }
            if (!string.IsNullOrEmpty(ArrivalDate))
            {
                DateTime arrivalDate = Prolink.Math.GetValueAsDateTime(ArrivalDate);
                ei.PutDate("RESERVE_DATE", arrivalDate);
                ei.Put("RESERVE_FROM", arrivalDate.Hour);
                ei.PutDate("ARRIVAL_DATE", arrivalDate);
            }
            if (!string.IsNullOrEmpty(scmdate))
            {
                ei.PutDate("ARRIVAL_DATE",Prolink.Math.GetValueAsDateTime(scmdate));
            }
            ei.Put("PARTNO_INFO", partInfo);
            ei.Put("PART_QTY", partQty);
            ei.Put("ASNNO_INFO", asnInfo);
            ei.Put("CS_CD", csCd);
            ei.Put("CS_NM", csNm);
            ei.Put("CS_NAME", csName);
            ei.Put("BU", bu);
            ei.Put("TRUCKER_NM", TruckerNm);
            ei.Put("TRAN_TYPE", TranType);
            ei.Put("CNT_TYPE", CntType);
            ei.Put("GW", Gw);
            ei.Put("GWU", "KGS");
            ei.Put("CBM", Cbm);
            ei.Put("SMCREATE_BY", SmCreateBy);
            ei.Put("PICK_AREA", Pol1);
            ei.Put("PICK_AREA_NM", PolNm1);
            ei.Put("DLV_AREA", Pod1);
            ei.Put("DLV_AREA_NM", PodNm1);
            ei.Put("LOT_NO", ShipmentId);
            ei.Put("BAT_NO", ReserveNo);
            ei.Put("RV_TYPE", "I");
            ei.Put("CALL_TYPE", "C");
            ei.Put("ORD_INFO", OrdNo);
            ei.Put("TRS_MODE", "Y");
            ei.Put("CAR_TYPE", CarType);
            ei.Put("QUOT_NO", QuotNo);
            ei.Put("SHIPMENT_INFO", ShipmentId);
            SetShipperLspCarrierInfo(ShipmentId, ei);
            ei.Put("DEC_INFO", DecInfo);
            ei.Put("CNTR_NO", CntrNo);
            ei.Put("NEW_CNTRNO", CntrNo);
            ei.Put("SEAL_NO1", SealNo1);
            ei.Put("NEW_SEAL", NewSeal);
            ei.Put("WS_CD", WsCd);

            string wsname = string.Empty;
            string dlvArea = string.Empty;
            string dlvAreaNm = string.Empty;
            if (parm != null)
            {
                if (parm.ContainsKey("WsNm"))
                    wsname = parm["WsNm"];
                if (parm.ContainsKey("AddrCode"))
                    addrcode = parm["AddrCode"];
                if (parm.ContainsKey("DlvArea"))
                    dlvArea = parm["DlvArea"];
                if (parm.ContainsKey("DlvAreaNm"))
                    dlvAreaNm = parm["DlvAreaNm"]; 
            }
            else
            {
                string wsnmsql = string.Format("SELECT WS_NM,DLV_ADDR,DLV_AREA,DLV_AREA_NM FROM SMWH WHERE CMP={0} AND WS_CD = {1}", SQLUtils.QuotedStr(CompanyId),
                SQLUtils.QuotedStr(WsCd));
                DataTable wsdt = OperationUtils.GetDataTable(wsnmsql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (wsdt.Rows.Count <= 0)
                {
                    resultinfo.IsSucceed = false;
                    resultinfo.Description = " ShipmentId:" + ShipmentId + " warehouse is empty";
                    return resultinfo;
                }

                for (int i = 0; i < wsdt.Rows.Count; i++)
                {
                    wsname += wsdt.Rows[i]["WS_NM"].ToString() + ",";
                }
                wsname = wsname.Trim(',');
                addrcode = Prolink.Math.GetValueAsString(wsdt.Rows[0]["DLV_ADDR"]);
                dlvArea = Prolink.Math.GetValueAsString(wsdt.Rows[0]["DLV_AREA"]);
                dlvAreaNm = Prolink.Math.GetValueAsString(wsdt.Rows[0]["DLV_AREA_NM"]);
            }
            ei.Put("WS_NM", wsname);
            ei.Put("ADDR_CODE", addrcode);
            ei.Put("DLV_AREA", dlvArea);
            ei.Put("DLV_AREA_NM", dlvAreaNm);
            ei.Put("BACK_LOCATION", backlc);
            ei.Put("PRIORITY", GetPriority(ShipmentId, CntrNo));
            ei.Put("PO_NO", PoNo);
            ei.Put("WO", Wo);
            ei.PutDate("PORT_DATE", ata);
            ei.Put("INVOICE_INFO", InvoiceInfo);
            setReserveValue(ShipmentId, ei);
            mixList.Add(ei);

            EditInstruct smrei = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
            smrei.PutKey("ORD_NO",OrdNo);
            smrei.PutKey("CNTR_NO", CntrNo);
            if (string.IsNullOrEmpty(PickupDate))
            {
                smrei.PutDate("ARRIVAL_DATE", Prolink.Math.GetValueAsDateTime(ArrivalDate));
            }
            else
            {
                smrei.PutDate("PICKUP_DATE", Prolink.Math.GetValueAsDateTime(PickupDate));
                smrei.PutDate("ARRIVAL_DATE", Prolink.Math.GetValueAsDateTime(ArrivalDate));
            }
            smrei.Put("WS_CD", WsCd);
            smrei.Put("RESERVE_NO", ReserveNo);
            smrei.Put("LOT_NO", ShipmentId);
            mixList.Add(smrei);
            Tuple<string, int, int, int, string, string, string, Tuple<string>> tuple = InboundHelper.setFreeTime(ShipmentId);
            DateTime pickupDate = Prolink.Math.GetValueAsDateTime(PickupDate);
            InboundHelper.ModifyDueDate(pickupDate, mixList, cdr, tuple); 

            IBEDIConfig config = GetEDIConfig(Trucker, CompanyId);
            if (config != null)
            {
                //0008021497
                switch (config.FunctionCode)
                {
                    case "UNIS":
                        resultinfo = SendUnisService(config, ShipmentId, CntrNo, PickupDate, ArrivalDate, OrdNo);
                        if (!resultinfo.IsSucceed)
                        {
                            return resultinfo;
                        }
                        break;
                }
            }
            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (idList != null && !idList.Contains(UId))
                    idList.Add(UId);
                resultinfo.IsSucceed = true;

                TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = ShipmentId, StsCd = "017", Cmp = CompanyId, Sender = UserId, Location = Pol1, LocationName = "", ContainerNo = CntrNo, Remark = CntrNo, StsDescp = "Order Truck By Container" });

                GetDivisonBySMR(ReserveNo, TranType);
                UpdateSMICUFT(ReserveNo, true);
                newReserveNo.Add(ReserveNo);
            }
            catch (Exception ex)
            {
                resultinfo.IsSucceed = false;
                resultinfo.Description = "Calling Truck Error";
            }
            return resultinfo;
        }

        public static DataTable GetSMByUids(string[] uids)
        {
            string sql = string.Format("SELECT * FROM SMSMI WHERE U_ID IN {0} ", SQLUtils.Quoted(uids));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public static bool StringISEmpty(string value)
        {
            value = value.Trim();
            if ("NULL".Equals(value.ToUpper()))
                return true;
            return string.IsNullOrEmpty(value);
        }

        public static bool CheckDeliveryInfo(string shipmentid, string trantype, ref string WsCd, ref string WsNm, ref string dlvAddr, ref string dlvarea, ref string dlvareanm, ref string addrcd)
        {
            string checksql = string.Empty;
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                checksql = string.Format("SELECT WS_CD,WS_NM,DLV_AREA, DLV_AREA_NM, ADDR_CODE,DLV_ADDR FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                //check
            }
            else
            {
                checksql = string.Format("SELECT WS_CD,WS_NM,DLV_AREA, DLV_AREA_NM, ADDR_CODE,DLV_ADDR FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            DataTable checkDt = OperationUtils.GetDataTable(checksql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            bool checkflag = false;
            for (int i = 0; i < checkDt.Rows.Count; i++)
            {
                if (StringISEmpty(checkDt.Rows[i]["DLV_AREA"].ToString()) || StringISEmpty(checkDt.Rows[i]["DLV_AREA_NM"].ToString()))
                {
                    checkflag = true;
                    break;
                }
                if (StringISEmpty(checkDt.Rows[i]["ADDR_CODE"].ToString()) || StringISEmpty(checkDt.Rows[i]["DLV_ADDR"].ToString()))
                {
                    checkflag = true;
                    break;
                }
                WsCd = Prolink.Math.GetValueAsString(checkDt.Rows[i]["WS_CD"]);
                WsNm = Prolink.Math.GetValueAsString(checkDt.Rows[i]["WS_NM"]);
                dlvAddr = Prolink.Math.GetValueAsString(checkDt.Rows[i]["DLV_ADDR"]);
                dlvarea = Prolink.Math.GetValueAsString(checkDt.Rows[i]["DLV_AREA"]);
                dlvareanm = Prolink.Math.GetValueAsString(checkDt.Rows[i]["DLV_AREA_NM"]);
                addrcd = Prolink.Math.GetValueAsString(checkDt.Rows[i]["ADDR_CODE"]);
            }
            return checkflag;
        }


        public static string ToDoorDelivery(string uids, string userid, string companyid, string groupid, string ordno = "",string smrvstatus="")
        {
            string[] uidlist = uids.Split(',');
            DataTable maindt = GetSMByUids(uidlist);
            //DLV TERM=DDP,CIP,DAP的才可以案;只能在Unreach才能案
            string ermsg = string.Empty;
            string termtype = string.Empty;
            string shipmentid = string.Empty;
            string suid = string.Empty;
            string status = string.Empty;
            string trantype = string.Empty;
            string PickAreaNm = string.Empty;
            string DepAddr = string.Empty;
            string Trucker = string.Empty;
            string TruckerNm = string.Empty;
            string CntrNo = string.Empty;
            string SealNo = string.Empty;
            string NewSeal = string.Empty;
            string Cbm = string.Empty;
            string GW = string.Empty;
            string GWu = string.Empty;
            string WsCd = string.Empty;
            string WsNm = string.Empty;
            string dlvAddr = string.Empty;
            string dlvarea = string.Empty;
            string dlvareanm = string.Empty;
            string addrcd = string.Empty;
            string cmp = string.Empty;
            string batNo = string.Empty;
            //string CallDateL = string.Empty;
            MixedList mlist = new MixedList();
            string cntrinfo = string.Empty;

            foreach (DataRow dr in maindt.Rows)
            {
                termtype = dr["INCOTERM_CD"].ToString();
                shipmentid = dr["SHIPMENT_ID"].ToString();
                suid = dr["U_ID"].ToString();
                status = dr["STATUS"].ToString();
                trantype = dr["TRAN_TYPE"].ToString();
                GW = Prolink.Math.GetValueAsString(dr["GW"]);
                GWu = Prolink.Math.GetValueAsString(dr["GWU"]);
                Cbm = Prolink.Math.GetValueAsString(dr["CBM"]);
                PickAreaNm = Prolink.Math.GetValueAsString(dr["POL_NM1"]);
                DepAddr = Prolink.Math.GetValueAsString(dr["DEP_ADDR1"]);
                Trucker = Prolink.Math.GetValueAsString(dr["TRUCKER1"]);
                TruckerNm = Prolink.Math.GetValueAsString(dr["TRUCKER_NM1"]);
                cntrinfo = Prolink.Math.GetValueAsString(dr["CNTR_INFO"]);
                cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                batNo = Prolink.Math.GetValueAsString(dr["BAT_NO"]);
                if (!"A".Equals(status))
                {
                    if (!"T".Equals(trantype))
                    {
                        ermsg += shipmentid + "Status is not in Unreach!";
                        continue;
                    }
                }
                //if (!("CIP".Equals(termtype) || "DAP".Equals(termtype) || "DDP".Equals(termtype)))
                //{
                //    ermsg += shipmentid + "Trade Term is not equal CIP, DAP OR DDP!";
                //    continue;
                //}
                bool checkflag = CheckDeliveryInfo(shipmentid, trantype, ref WsCd, ref WsNm, ref dlvAddr, ref dlvarea, ref dlvareanm, ref addrcd);
                if (checkflag)
                {
                    ermsg += shipmentid + " Dlv Area Or Dlv Addr is Empty,Please Check it !";
                    continue;
                }

                string sql = string.Empty;
                if (string.IsNullOrEmpty(WsCd))
                {
                    sql = string.Format("SELECT TOP 1 T.WS_CD,T.WS_NM FROM (SELECT BSADDR.*,SMWH.WS_CD,SMWH.WS_NM FROM BSADDR LEFT JOIN SMWH ON BSADDR.ADDR_CODE=SMWH.DLV_ADDR)T WHERE T.ADDR_CODE={0}", SQLUtils.QuotedStr(addrcd));
                    DataTable tdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (tdt.Rows.Count > 0)
                    {
                        WsCd = Prolink.Math.GetValueAsString(tdt.Rows[0]["WS_CD"]);
                        WsNm = Prolink.Math.GetValueAsString(tdt.Rows[0]["WS_NM"]);
                    }
                }
                sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentid));
                DataTable smicntrdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow sminctr in smicntrdt.Rows)
                {
                    CntrNo = Prolink.Math.GetValueAsString(sminctr["CNTR_NO"]);
                    SealNo = Prolink.Math.GetValueAsString(sminctr["SEAL_NO1"]);
                    NewSeal = Prolink.Math.GetValueAsString(sminctr["NEW_SEAL"]);
                }
                //1. 點”To Door Delivery”按鈕時，Shipment的狀態變成Gate In，跳過Notify LSP, Booking Agent Confirm, Notify Broker, Broker Confirm.
                //2. 點這個按鈕後，系統要自動創建預約單，倉庫就直接根據Delivery帶出倉庫，狀態是Gate In，進廠時間寫點這個按鈕的時間(要寫入SMRV.IN_DATE和SMRDN/SMRCNTR中的IDATE)

                EditInstruct smiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                smiei.PutKey("U_ID", suid);
                smiei.Put("STATUS", "G");
                smiei.Put("IB_WINDOW", userid);
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, companyid);
                smiei.PutDate("IB_DATE", ndt);
                mlist.Add(smiei);

                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                System.Collections.Hashtable hash = new System.Collections.Hashtable();
                hash.Add("CMP", companyid);
                string ReserveNo = ReserveHelper.getAutoNoByHash("RV_NO", groupid, companyid, hash); 
                string datetimenow = odt.ToString("yyyy-MM-dd HH:mm:ss");
                string datetimenow1 = ndt.ToString("yyyy-MM-dd HH:mm:ss");
                string arrivaldate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string arrivalstring = arrivaldate.Substring(0, 8);
                int time = Prolink.Math.GetValueAsInt(arrivaldate.Substring(8, 2));

                ei.PutDate("RESERVE_DATE", arrivalstring);//日期部分
                ei.Put("RESERVE_FROM", time);//小时部分
                if (!string.IsNullOrEmpty(ordno))
                {
                    ei.Put("ORD_NO", ordno);
                }
                string shippernm = ei.Get("SHIPPER_NM");
                string shipper = string.Empty;

                ei.OperationType = EditInstruct.INSERT_OPERATION;

                string LotNo = LotNo = "S" + ReserveHelper.getAutoNo("RV_NO", groupid, companyid);
                ei.Put("LOT_NO", LotNo);
                ei.Put("TRS_MODE", "Y");
                ei.Put("U_ID", Guid.NewGuid().ToString());
                ei.Put("GROUP_ID", groupid);
                ei.Put("CMP", cmp);
                ei.Put("CREATE_BY", userid);
                ei.PutDate("CREATE_DATE", odt);
                ei.PutDate("CREATE_DATE_L", ndt);
                ei.Put("SHIPMENT_INFO", shipmentid);
                if (!string.IsNullOrEmpty(smrvstatus))
                {
                    ei.Put("STATUS", smrvstatus);
                }
                else
                {
                    ei.Put("STATUS", "I");
                }
                
                ei.Put("RV_TYPE", "I");
                ei.Put("DN_NO", dr["COMBINE_INFO"].ToString());
                ei.PutDate("ARRIVAL_FACT_DATE", odt);
                ei.PutDate("ARRIVAL_FACT_DATE_L", ndt);
                ei.Put("RESERVE_NO", ReserveNo);
                ei.PutDate("IN_DATE", odt);
                ei.PutDate("IN_DATE_L", ndt);
                ei.PutDate("CALL_DATE_L", ndt.ToString("yyyyMMddHHmmss"));
                ei.Put("NEW_SEAL", NewSeal);
                ei.Put("GW", GW);
                ei.Put("GWU", GWu);
                ei.Put("CBM", Cbm);
                ei.Put("PICK_AREA_NM", PickAreaNm);
                ei.Put("DEP_ADDR", DepAddr);
                ei.Put("TRUCKER", Trucker);
                ei.Put("TRUCKER_NM", TruckerNm);
                if (string.IsNullOrEmpty(CntrNo))
                {
                    ei.Put("CNTR_NO", cntrinfo);
                }
                else
                {
                    ei.Put("CNTR_NO", CntrNo);
                }
                ei.Put("SEAL_NO1", SealNo);
                //ei.Put("IS_AUTOCREATE", "Y");   //是否是自动创建的Appointment  Y为是
                if ("F".Equals(trantype) || "R".Equals(trantype))
                {
                    ei.Put("CALL_TYPE", "C");
                }
                else
                {
                    ei.Put("CALL_TYPE", "D");
                }
                if (!string.IsNullOrEmpty(WsCd))
                    ei.Put("WS_CD", WsCd);
                if (!string.IsNullOrEmpty(WsNm))
                    ei.Put("WS_NM", WsNm);
                if (!string.IsNullOrEmpty(dlvAddr))
                    ei.Put("DLV_ADDR", dlvAddr);
                ei.Put("TRAN_TYPE", "T");
                ei.Put("ADDR_CODE", addrcd);
                ei.Put("DLV_AREA", dlvarea);
                ei.Put("DLV_AREA_NM", dlvareanm);
                ei.Put("SMCREATE_BY", userid);
                ei.Put("IBAT_NO", batNo);
                SMSMIHelper.setReserveValue(shipmentid, ei);
                mlist.Add(ei);
                //产生ShipmentId
                //if (string.IsNullOrEmpty(ReserveNo)) return ;
                SMSMIHelper.CreateSMRSubInfo(mlist, dr, datetimenow, datetimenow1, ReserveNo);

            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    ermsg = ex.Message;
                }
            }

            return ermsg;
        }

        public static void setReserveValue(string ShipmentId, EditInstruct ei)
        {
            string sql = string.Format("SELECT ATD,INCOTERM_CD,INCOTERM_DESCP,CS_NM,SEC_CMP FROM SMSMI WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable smidt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smidt.Rows.Count > 0)
            {
                DateTime atd = Prolink.Math.GetValueAsDateTime(smidt.Rows[0]["ATD"]);
                string IncotermCd = Prolink.Math.GetValueAsString(smidt.Rows[0]["INCOTERM_CD"]);
                string IncotermDescp = Prolink.Math.GetValueAsString(smidt.Rows[0]["INCOTERM_DESCP"]);
                string CsNm = Prolink.Math.GetValueAsString(smidt.Rows[0]["CS_NM"]);
                string secCmp = Prolink.Math.GetValueAsString(smidt.Rows[0]["SEC_CMP"]);
                ei.PutDate("ATD", atd);
                ei.Put("SEC_CMP", secCmp);
                ei.Put("INCOTERM_CODE", IncotermCd);
                ei.Put("INCOTERM_DESCP", IncotermDescp);
                ei.Put("CS_NM", CsNm);
                ei.PutExpress("SLIP_SHEET_INFO", string.Format("(SELECT DISTINCT PKG_UNIT_DESC+';' FROM SMIDN WHERE SMIDN.SHIPMENT_ID={0} AND SMIDN.PKG_UNIT_DESC like '%PLASTIC%' FOR XML PATH(''))", SQLUtils.QuotedStr(ShipmentId)));
            }
        }

        public static void CreateSMRSubInfo(MixedList mixList, DataRow smirow, string indate, string indate1, string reserveno)
        {
            if (smirow == null) return;
            string Pol1 = Prolink.Math.GetValueAsString(smirow["POL1"]);
            string PolNm1 = Prolink.Math.GetValueAsString(smirow["POL_NM1"]);
            string OrdNo = string.Empty;
            string ShipmentId = Prolink.Math.GetValueAsString(smirow["SHIPMENT_ID"]);
            #region 將dn轉至叫車dn
            string sql = @"INSERT INTO SMRDN (
                                            U_ID,
                                            DISCHARGE_DATE,
                                            GROUP_ID,
                                            CMP,
                                            STN,
                                            DEP,
                                            SHIPMENT_ID,
                                            DN_NO,
                                            DEC_NO,
                                            INV_NO,
                                            NW,
                                            GW,
                                            GWU,
                                            CBM,
                                            CBMU,
                                            QTY,
                                            QTYU,
                                            PKG_NUM,
                                            PKG_UNIT,
                                            PKG_UNIT_DESC,
                                            CNT20,
                                            CNT40,
                                            CNT40HQ,
                                            OTH_CNT_TYPE,
                                            OTH_CNT_NUM,
                                            DLV_AREA,
                                            DLV_AREA_NM,
                                            PICK_AREA,
                                            PICK_AREA_NM,
                                            ADDR_CODE,
                                            DLV_ADDR,
                                            WS_CD,
                                            ORD_NO,
                                            IDATE,
                                            IDATE_L,
                                            RESERVE_NO)
                                        SELECT 
                                            a.U_ID,
                                            a.DISCHARGE_DATE,
                                            a.GROUP_ID,
                                            a.CMP,
                                            a.STN,
                                            a.DEP,
                                            a.SHIPMENT_ID,
                                            a.DN_NO,
                                            a.DEC_NO,
                                            a.INV_NO,
                                            a.NW,
                                            a.GW,
                                            a.GWU,
                                            a.CBM,
                                            a.CBMU,
                                            a.QTY,
                                            a.QTYU,
                                            a.PKG_NUM,
                                            a.PKG_UNIT,
                                            a.PKG_UNIT_DESC,
                                            a.CNT20,
                                            a.CNT40,
                                            a.CNT40HQ,
                                            a.OTH_CNT_TYPE,
                                            a.OTH_CNT_NUM,
                                            a.DLV_AREA,
                                            a.DLV_AREA_NM,
                                            {0},
                                            {1},
                                            a.ADDR_CODE,
                                            a.DLV_ADDR,
                                            CASE WHEN WS_CD IS NULL THEN (SELECT TOP 1 WS_CD FROM SMWH WHERE SMWH.DLV_ADDR=a.ADDR_CODE AND SMWH.CMP=a.CMP) ELSE
	                                        WS_CD END AS WS_CD,
                                            {2},
                                            {4},
                                            {5},
                                            {6}
                                            FROM (SELECT * from SMIDN WHERE SHIPMENT_ID={3}) as a";
            sql = string.Format(sql, SQLUtils.QuotedStr(Pol1), SQLUtils.QuotedStr(PolNm1), SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(indate), SQLUtils.QuotedStr(indate1), SQLUtils.QuotedStr(reserveno));
            mixList.Add(sql);
            #endregion

            #region 將進口貨櫃寫入叫車貨櫃
            sql = @"INSERT INTO SMRCNTR (
                                            U_ID,
                                            DISCHARGE_DATE,
                                            SHIPMENT_ID,
                                            GROUP_ID,
                                            STN,
                                            DEP,
                                            CMP,
                                            CNTR_NO,
                                            DEC_NO,
                                            CNTR_TYPE,
                                            SEAL_NO1,
                                            SEAL_NO2,
                                            DN_NO,
                                            DIVISION_DESCP,
                                            QTY,
                                            QTYU,
                                            GW,
                                            GWU,
                                            CBM,
                                            DLV_AREA,
                                            DLV_AREA_NM,
                                            PICK_AREA,
                                            PICK_AREA_NM,
                                            ADDR_CODE,
                                            DLV_ADDR,
                                            WS_CD,
                                            ORD_NO,
                                            IDATE,
                                            IDATE_L,
                                            RESERVE_NO,FINAL_WH)
                                        SELECT 
                                            a.U_ID,
                                            a.DISCHARGE_DATE,
                                            a.SHIPMENT_ID,
                                            a.GROUP_ID,
                                            a.STN,
                                            a.DEP,
                                            a.CMP,
                                            a.CNTR_NO,
                                            a.DEC_NO,
                                            a.CNTR_TYPE,
                                            a.SEAL_NO1,
                                            a.SEAL_NO2,
                                            a.DN_NO,
                                            a.DIVISION_DESCP,
                                            a.QTY,
                                            a.QTYU,
                                            a.GW,
                                            a.GWU,
                                            a.CBM,
                                            a.DLV_AREA,
                                            a.DLV_AREA_NM,
                                            {0},
                                            {1},
                                            a.ADDR_CODE,
                                            a.DLV_ADDR,
                                            CASE WHEN WS_CD IS NULL THEN (SELECT TOP 1 WS_CD FROM SMWH WHERE SMWH.DLV_ADDR=a.ADDR_CODE AND SMWH.DLV_ADDR=a.ADDR_CODE) ELSE
	                                        WS_CD END AS WS_CD,
                                            {2},
                                            {4},
                                            {5},
                                            {6},FINAL_WH
                                            FROM (SELECT * from SMICNTR WHERE SHIPMENT_ID={3}) as a";
            sql = string.Format(sql, SQLUtils.QuotedStr(Pol1), SQLUtils.QuotedStr(PolNm1), SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(indate), SQLUtils.QuotedStr(indate1), SQLUtils.QuotedStr(reserveno));
            mixList.Add(sql);
            #endregion
        }





        /// <summary>
        /// 更新订舱相关的 SMIRV 和 SMIRV的CUFT栏位
        /// </summary>
        /// <param name="no"></param>
        /// <param name="is_reserve">是否是预约单</param>
        public static void UpdateSMICUFT(string no, bool is_reserve = false)
        {
            try
            {
                string field = is_reserve ? "RESERVE_NO" : "SHIPMENT_ID";
                string sql = string.Format("SELECT SHIPMENT_ID,ORD_NO,RESERVE_NO,DN_NO FROM SMRCNTR WHERE {0}={1} UNION SELECT SHIPMENT_ID,ORD_NO,RESERVE_NO,DN_NO FROM SMRDN WHERE {0}={1}", field, SQLUtils.QuotedStr(no));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> dnList = new List<string>();
                foreach (DataRow cuft in dt.Rows)
                {
                    string dnNo = Prolink.Math.GetValueAsString(cuft["DN_NO"]);
                    if (string.IsNullOrEmpty(dnNo))
                        continue;

                    string[] temps = dnNo.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    dnList.AddRange(temps);
                }

                string smicuft_sql = string.Format("SELECT * FROM SMICUFT WHERE DN_NO IN {0}", SQLUtils.Quoted(dnList.ToArray()));
                DataTable smicuftDt = OperationUtils.GetDataTable(smicuft_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                List<string> reserveList = new List<string>();
                List<string> ordList = new List<string>();
                foreach (DataRow cuft in dt.Rows)
                {
                    string reserve_no = Prolink.Math.GetValueAsString(cuft["RESERVE_NO"]);
                    string ord_no = Prolink.Math.GetValueAsString(cuft["ORD_NO"]);
                    if (!string.IsNullOrEmpty(reserve_no) && !dnList.Contains(reserve_no))
                        reserveList.Add(reserve_no);
                    if (!string.IsNullOrEmpty(ord_no) && !dnList.Contains(ord_no))
                        ordList.Add(ord_no);
                }

                MixedList ml = new MixedList();
                if (is_reserve)
                {
                    foreach (string reserveNo in reserveList)
                    {
                        AddUpdateCUFT(dt, smicuftDt, ml, reserveNo, true);
                    }
                }
                else
                {
                    foreach (string ordNo in ordList)
                    {
                        AddUpdateCUFT(dt, smicuftDt, ml, ordNo);
                    }
                }

                if (ml.Count > 0)
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { }
        }

        public static void AddUpdateCUFT(DataTable dt, DataTable smicuftDt, MixedList ml, string no, bool is_reserve = false)
        {
            string table = is_reserve ? "SMIRV" : "SMORD";
            string key = is_reserve ? "RESERVE_NO" : "ORD_NO";
            DataRow[] drs = dt.Select(string.Format("{0}={1}", key, SQLUtils.QuotedStr(no)));
            List<string> test = new List<string>();
            foreach (DataRow dr in drs)
            {
                string dnNo = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                if (string.IsNullOrEmpty(dnNo))
                    continue;
                test.Add(dnNo);
            }
            if (test.Count <= 0)
                return;

            drs = smicuftDt.Select(string.Format("DN_NO IN {0}", SQLUtils.Quoted(test.ToArray())));
            EditInstruct ei = new EditInstruct(table, EditInstruct.UPDATE_OPERATION);
            ei.PutKey(key, no);
            string cuft = GetCuft(drs);
            if (string.IsNullOrEmpty(cuft))
                return;
            ei.Put("DIMENSIONS_INFO", cuft);
            ml.Add(ei);
        }

        public static string GetCuft(DataRow[] drs)
        {
            string cuftStr = string.Empty;
            List<string> cuftList = new List<string>();
            foreach (DataRow cuft in drs)
            {
                cuftList.Add(string.Format("{0}:{1}*{2}*{3}", cuft["DN_NO"], cuft["L"], cuft["W"], cuft["H"]));
            }
            if (cuftList.Count > 0)
                cuftStr = string.Join(";", cuftList);
            return cuftStr;
        }

        public static void GetDivisonBySMR(string reserveno, string trantype = "")
        {
            string sql = string.Empty;

            if ("F".Equals(trantype) || "R".Equals(trantype))
                sql = string.Format("SELECT DISTINCT DIVISION_DESCP FROM SMICNTR WHERE CNTR_NO IN (SELECT CNTR_NO FROM SMRCNTR WHERE RESERVE_NO={0})",
                    SQLUtils.QuotedStr(reserveno));
            else
            {
                sql = string.Format("SELECT DISTINCT DIVISION_DESCP FROM SMIDN WHERE DN_NO IN (SELECT DN_NO FROM SMRDN WHERE RESERVE_NO={0})",
                SQLUtils.QuotedStr(reserveno));
            }
            if (string.IsNullOrEmpty(trantype))
            {
                sql = string.Format(@"SELECT DISTINCT DIVISION_DESCP FROM SMICNTR WHERE CNTR_NO IN (SELECT CNTR_NO FROM SMRCNTR WHERE RESERVE_NO={0})
                UNION SELECT DISTINCT DIVISION_DESCP FROM SMIDN WHERE DN_NO IN (SELECT DN_NO FROM SMRDN WHERE RESERVE_NO={0})",
                    SQLUtils.QuotedStr(reserveno));
            }
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> divitions = new List<string>();
            string divition = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                divition = dr["DIVISION_DESCP"].ToString();
                if (string.IsNullOrEmpty(divition))
                    continue;
                if (!divitions.Contains(divition))
                    divitions.Add(divition);
            }
            if (divitions.Count > 0)
            {
                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("RESERVE_NO", reserveno);
                divition = string.Join(",", divitions);
                if (divition.Length > 300)
                    divition = divition.Substring(0, 300);
                ei.Put("PRODUCT_TYPE_INFO", divition);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

        }

        private static IBResultInfo SendUnisService(IBEDIConfig config, string shipmentid, string cntr_no, string pickupdate = null, string arrvialdate = null,string ordno=null)
        {
            IBResultInfo rf = new IBResultInfo();
            rf.IsSucceed = false;
            string sql = string.Format(@"SELECT SHIPMENT_ID +'|'+CNTR_NO AS SHIPMENT_ID,'D' AS STATUS,(SELECT TOP 1 VESSEL1 FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID)
                AS VESSEL,(SELECT TOP 1 CARRIER FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID) AS CARRIER,
                DLV_AREA AS DEST_CD,
                (SELECT TOP 1 ETD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID) AS ETD,
                (SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID) AS ETA,
                (SELECT TOP 1 ATA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID) AS ATA,SEAL_NO1,CNTR_NO FROM SMICNTR WHERE
                    SHIPMENT_ID={0} AND CNTR_NO={1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntr_no));
            if (!string.IsNullOrEmpty(ordno))
            {
                sql=string.Format(@"SELECT SHIPMENT_ID +'|'+CNTR_NO AS SHIPMENT_ID,'D' AS STATUS,(SELECT TOP 1 VESSEL1 FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMrCNTR.SHIPMENT_ID)
                AS VESSEL,(SELECT TOP 1 CARRIER FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMrCNTR.SHIPMENT_ID) AS CARRIER,
                DLV_AREA AS DEST_CD,
                (SELECT TOP 1 ETD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMrCNTR.SHIPMENT_ID) AS ETD,
                (SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMrCNTR.SHIPMENT_ID) AS ETA,
                (SELECT TOP 1 ATA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMrCNTR.SHIPMENT_ID) AS ATA,
				SEAL_NO1,CNTR_NO FROM SMrCNTR WHERE
                    SHIPMENT_ID={0} AND CNTR_NO={1} AND ORD_NO={2}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntr_no), SQLUtils.QuotedStr(ordno));
            }
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                rf.Description = "No Data";
                return rf;
            }
            sql = string.Format("SELECT OPART_NO,QTY FROM SMIDNP WHERE SHIPMENT_ID={0} AND CNTR_NO={1} order by OPART_NO desc", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntr_no));
            DataTable dnpdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    using (var client = new TransferOperation.InboundTPVServiceClient())
                    {
                        // EndpointAddress 
                        client.Endpoint.Address = new System.ServiceModel.EndpointAddress(config.Server);
                        TransferOperation.TPVInboundOrderRequest tor = new TransferOperation.TPVInboundOrderRequest();
                        tor.AtaPort = Prolink.Math.GetValueAsDateTime(dr["ATA"]).ToString("yyyyMMddHHmm");
                        tor.Carrier = Prolink.Math.GetValueAsString(dr["CARRIER"]);
                        tor.ContainerNo = cntr_no;
                        tor.Destionation = Prolink.Math.GetValueAsString(dr["DEST_CD"]);
                        if (!string.IsNullOrEmpty(pickupdate))
                        {
                            tor.EtaHub = Prolink.Math.GetValueAsDateTime(pickupdate).ToString("yyyyMMddHHmm");
                        }
                        tor.EtaPort = Prolink.Math.GetValueAsDateTime(dr["ETA"]).ToString("yyyyMMddHHmm");
                        if (!string.IsNullOrEmpty(arrvialdate))
                        {
                            tor.EtaTerminal = Prolink.Math.GetValueAsDateTime(arrvialdate).ToString("yyyyMMddHHmm");
                        }
                        tor.EtdChina = Prolink.Math.GetValueAsDateTime(dr["ETD"]).ToString("yyyyMMddHHmm");

                        sql = string.Format("SELECT DISTINCT OPART_NO,COUNT(1) FROM SMIDNP WHERE SHIPMENT_ID={0} AND CNTR_NO={1} GROUP BY  OPART_NO", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntr_no));
                        DataTable distinctdnp = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        List<TransferOperation.Item> items = new List<TransferOperation.Item>();

                        foreach (DataRow dnpdr in distinctdnp.Rows)
                        {
                            string opartno = Prolink.Math.GetValueAsString(dnpdr["OPART_NO"]);
                            if (string.IsNullOrEmpty(opartno))
                                continue;
                            DataRow[] opartrows = dnpdt.Select("OPART_NO=" + SQLUtils.QuotedStr(opartno));
                            int count = opartrows.Length;
                            int i = 0;
                            foreach (DataRow opartdr in opartrows)
                            {
                                TransferOperation.Item item = new TransferOperation.Item();
                                if (i==0)
                                {
                                    item.Model = opartno;
                                }
                                else
                                {
                                    item.Model = opartno + "_" + i;
                                }
                                item.Qty = Prolink.Math.GetValueAsString(opartdr["QTY"]);
                                items.Add(item);
                                i++;
                            }
                        }

                        tor.Items = items.ToArray();
                        tor.SealNo = Prolink.Math.GetValueAsString(dr["SEAL_NO1"]);
                        tor.ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        tor.Status = "D";
                        tor.Vessel = Prolink.Math.GetValueAsString(dr["VESSEL"]);
                        System.Web.Script.Serialization.JavaScriptSerializer Serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string log = Serializer.Serialize(tor);
                        Prolink.DataOperation.OperationUtils.Logger.WriteLog("Send info:" + log);
                        TransferOperation.Response rp = client.TPVInboundOrderWISE(tor);

                        if (!"Success".Equals(rp.Status) && !string.IsNullOrEmpty(rp.Status))
                        {
                            Prolink.DataOperation.OperationUtils.Logger.WriteLog("Unise Edi Send Error :" + Prolink.Math.GetValueAsString(rp.ErrMessage));
                            rf.IsSucceed = false;
                            rf.Description = "Unise Edi Send Error:" + rp.ErrMessage;
                            WriteUniseEdiLog(tor.ShipmentId,log, false, rp.ErrMessage);
                            return rf;
                        }
                    }
                }
                catch (Exception ex)
                {
                    rf.IsSucceed = false;
                    rf.Description = "Unise Edi Send Error:" + ex.Message;
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog("Unise Edi Send Error :" + ex.ToString());
                    WriteUniseEdiLog(shipmentid, "", false, ex.ToString());
                    return rf;
                }
                //rp.CompanyID;   //WISE仓库
                //rp.CustomerID;  //TPV
                //rp.WISEPOID;    //WISE成后订单号
                //rp.ReferenceNo; //ShipmentId
                //rp.PoNo;        //ShipmentId
                //rp.Status;      //Fail  或者  Success
                //rp.ErrMessage;  //错误信息
            }

            rf.IsSucceed = true;
            rf.Description = "Unise Edi Send Success";
            WriteUniseEdiLog(shipmentid, "", true, "");
            return rf;
        }

        public static void WriteUniseEdiLog(string shipmentid, string info,bool successfunl,string remark)
        {
            string str = Prolink.Math.GetValueAsString(info);
            string uid = System.Guid.NewGuid().ToString();
            EdiInfo ediInfo = new EdiInfo();
            ediInfo.ID = "Eshipping";
            ediInfo.EdiId = "SendUnis";
            ediInfo.Remark = remark;
            ediInfo.CreateBy = "Eshipping Call Car";
            ediInfo.Rs = "Send";
            if (successfunl) {
                ediInfo.Status = "Succeed";
            }
            else { ediInfo.Status = "Failed"; }
            ediInfo.FromCd = "eShipping";
            ediInfo.ToCd = "Unise";
            ediInfo.DataFolder = uid;
            ediInfo.RefNO = shipmentid;
            ediInfo.GroupId = "TPV";
            ediInfo.Cmp = "";
            ediInfo.Stn = "";

            MixedList ml = new MixedList();
            ml.Add(CreateEDIEi(ediInfo));
            if (!string.IsNullOrEmpty(str))
            {
                EditInstruct ei = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", uid);
                ei.Put("EDI_DATE", str);
                ei.PutExpress("CREATE_DATE", "getdate()");
                ml.Add(ei);
            }
            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch
            {
            }
        }

        public static EditInstruct CreateEDIEi(EdiInfo ediInfo)
        {
            EditInstruct ei = new EditInstruct("EDI_LOG_UNIS", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", ediInfo.ID);
            ei.Put("EDI_ID", ediInfo.EdiId);
            ei.PutExpress("EVENT_DATE", "getdate()");
            if (!string.IsNullOrEmpty(ediInfo.Remark) && ediInfo.Remark.Length > 500)
                ediInfo.Remark = ediInfo.Remark.Substring(0, 500);
            ei.Put("REMARK", ediInfo.Remark);
            ei.Put("SENDER", ediInfo.CreateBy);
            ei.Put("RS", ediInfo.Rs);
            ei.Put("STATUS", ediInfo.Status);
            ei.Put("FROM_CD", ediInfo.FromCd);
            ei.Put("TO_CD", ediInfo.ToCd);
            ei.Put("DATA_FOLDER", ediInfo.DataFolder);
            ei.Put("REF_NO", ediInfo.RefNO);
            ei.Put("GROUP_ID", ediInfo.GroupId);
            ei.Put("CMP", ediInfo.Cmp);
            ei.Put("STN", ediInfo.Stn);
            return ei;
        }
        

        public static IBEDIConfig GetEDIConfig(string partyNo, string location)
        {
            string sql = string.Format("SELECT * FROM SMEXM WHERE EXPRESS={0}", SQLUtils.QuotedStr(partyNo));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return null;
            DataRow row = dt.Rows[0];
            return new IBEDIConfig
            {
                MsgCode = Prolink.Math.GetValueAsString(row["MSG_CODE"]),
                RecieveCode = Prolink.Math.GetValueAsString(row["RCV_ID"]),
                SenderCode = Prolink.Math.GetValueAsString(row["SEND_ID"]),
                Cmp = Prolink.Math.GetValueAsString(row["CMP"]),
                CmpName = Prolink.Math.GetValueAsString(row["CMP_NM"]),
                PartyNO = Prolink.Math.GetValueAsString(row["EXPRESS"]),
                Psw = Prolink.Math.GetValueAsString(row["PW_ID"]),
                Remark = Prolink.Math.GetValueAsString(row["REMARK"]),
                Server = Prolink.Math.GetValueAsString(row["WEB_URL"]),
                User = Prolink.Math.GetValueAsString(row["EX_NO"]),
                FunctionCode = Prolink.Math.GetValueAsString(row["EDI_MODE"]),
                Authorization = Prolink.Math.GetValueAsString(row["AUTHORI_ZATION"])
            };
        }

        public static string GetPriority(string shipmentid, string cntrno)
        {
            string sql = string.Format(@"SELECT MIN(PRIORITY) FROM SMICNTR WHERE SHIPMENT_ID={0} AND
                        CNTR_NO={1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntrno));
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static bool IsTSALogistic(string CompanyId)
        {
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD_TYPE='TSA' and CD={0}", SQLUtils.QuotedStr(CompanyId));
            int count =  OperationUtils.GetValueAsInt(sql,  Prolink.Web.WebContext.GetInstance().GetConnection());
            if (count > 0)
                return true;
            return false;
        }

        public static void SetShipperLspCarrierInfo(string ShipmentId, EditInstruct ei)
        {
            string sql = "SELECT MASTER_NO,SH_NO+SH_NM AS SH_NO,LSP_NO+LSP_NM AS LSP_NO,CARRIER FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable smsmidt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smsmidt.Rows.Count > 0)
            {
                ei.Put("SHIPPER", smsmidt.Rows[0]["SH_NO"].ToString());
                ei.Put("FOWARDER", smsmidt.Rows[0]["LSP_NO"].ToString());
                ei.Put("CARRIER", smsmidt.Rows[0]["CARRIER"].ToString());
            }
        }

        public static void UpdatePostBill(string shipmentid,string dnno)
        {
            try
            {
                string sqlcout = string.Format("SELECT COUNT(1) FROM SMIDN WHERE SHIPMENT_ID={0} AND DN_NO !={1} AND (IS_POSTBILL!='Y' OR IS_POSTBILL IS NULL)", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(dnno));
                int count = OperationUtils.GetValueAsInt(sqlcout, Prolink.Web.WebContext.GetInstance().GetConnection());
                string flagstring = string.Empty;
                if (count == 0)
                {
                    flagstring = ",IS_POSTBILL='Y'";
                }
                string updatesql = string.Format(@"UPDATE SMSMI SET POST_BILL_DATE_INFO=(SELECT DN_NO+':'+CONVERT(VARCHAR(100), SMIDN.POST_BILL_DATE, 23)+',' FROM 
                                    SMIDN WHERE SMIDN.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND POST_BILL_DATE IS NOT NULL FOR XML PATH('')){0} WHERE SHIPMENT_ID={1}", flagstring, SQLUtils.QuotedStr(shipmentid));
                OperationUtils.ExecuteUpdate(updatesql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception e) { }
        }

        public static string InboundsetLight(string u_id, string OUid, string Io)
        {
            string smsmTable = "SMSM", smsmptTable = "SMSMPT";
            string sql = string.Empty;
            string Location = string.Empty, TranType = string.Empty, PartyNo = string.Empty, ShipmentId = string.Empty;
            string msg = "success";
            List<string> jobList = new List<string>();
            if (Io == "I")
            {
                smsmTable = "SMSMI";
                smsmptTable = "SMSMIPT";
            }
            if (!string.IsNullOrEmpty(OUid))
            {
                string obsql = "SELECT SHIPMENT_ID,IMPORT_NO,CMP,(SELECT U_ID FROM SMSM A WHERE A.SHIPMENT_ID=B.IMPORT_NO) AS FIRST_UID FROM SMSM B WHERE B.U_ID=" + SQLUtils.QuotedStr(OUid);
                DataTable obsmdt = OperationUtils.GetDataTable(obsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (obsmdt.Rows.Count > 0)
                {
                    DataRow obdr = obsmdt.Rows[0];
                    string importNo = Prolink.Math.GetValueAsString(obdr["IMPORT_NO"]);
                    string ocmp = Prolink.Math.GetValueAsString(obdr["CMP"]);
                    string oshipmentid = Prolink.Math.GetValueAsString(obdr["SHIPMENT_ID"]);
                    string firstuid = Prolink.Math.GetValueAsString(obdr["FIRST_UID"]);
                    if (!string.IsNullOrEmpty(importNo) && "MMDHK".Equals(ocmp) && oshipmentid.Contains(importNo))
                    {
                        if (!string.IsNullOrEmpty(firstuid) && !jobList.Contains(firstuid))
                            jobList.Add(firstuid);
                    }
                }
                jobList.Add(OUid);
            }
            try
            {
                sql = "SELECT M.CMP, D.PARTY_NO, M.TRAN_TYPE, M.SHIPMENT_ID FROM " + smsmTable + " M LEFT JOIN " + smsmptTable + " D ON M.SHIPMENT_ID=D.SHIPMENT_ID AND D.PARTY_TYPE='CS' WHERE M.U_ID=" + SQLUtils.QuotedStr(u_id);
                DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smdt.Rows.Count <= 0)
                    return msg;

                foreach (DataRow dr in smdt.Rows)
                {
                    Location = Prolink.Math.GetValueAsString(dr["CMP"]);
                    TranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                    PartyNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                    ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                }

                DataTable jobDt = GetEdocData(ShipmentId, Location, OUid, true);
                if (jobDt != null && jobDt.Rows.Count > 0)
                {
                    foreach (DataRow dr in jobDt.Rows)
                    {
                        string jobNo = Prolink.Math.GetValueAsString(dr["U_ID"]);
                        if (!string.IsNullOrEmpty(jobNo) && !jobList.Contains(jobNo))
                            jobList.Add(jobNo);
                    }
                }
                    
                sql = "SELECT D.* FROM BSLIGHTM M, BSLIGHTD D WHERE M.U_ID=D.U_FID AND D.CUST_CD=" + SQLUtils.QuotedStr(PartyNo) + " AND D.TRAN_TYPE=" + SQLUtils.QuotedStr(TranType) + " AND D.IO=" + SQLUtils.QuotedStr(Io) + " AND M.CMP=" + SQLUtils.QuotedStr(Location);
                DataTable lgdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> lightList = new List<string>();
                if (lgdt.Rows.Count > 0)
                {
                    foreach (DataRow dr in lgdt.Rows)
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            string L = Prolink.Math.GetValueAsString(dr["L" + i.ToString()]);
                            if (L != "")
                            {
                                lightList.Add(L);
                            }
                        }

                    }
                }
                else
                {
                    sql = "SELECT D.* FROM BSLIGHTM M, BSLIGHTD D WHERE M.U_ID=D.U_FID AND D.CUST_CD=" + SQLUtils.QuotedStr(Location) + " AND D.TRAN_TYPE=" + SQLUtils.QuotedStr(TranType) + " AND D.IO=" + SQLUtils.QuotedStr(Io) + " AND M.CMP=" + SQLUtils.QuotedStr(Location);
                    lgdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (lgdt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in lgdt.Rows)
                        {
                            for (int i = 1; i <= 10; i++)
                            {
                                string L = Prolink.Math.GetValueAsString(dr["L" + i.ToString()]);
                                if (L != "")
                                {
                                    lightList.Add(L);
                                }
                            }

                        }
                    }
                }

                if (Io == "I")
                {
                    if (string.IsNullOrEmpty(OUid))
                    {
                        sql = "SELECT F.EdocType FROM [EDOC2_FOLDER] E LEFT JOIN Folders D ON E.FOLDER_GUID = D.GUID LEFT JOIN Files F ON D.FID = F.FID WHERE JOB_NO=" + SQLUtils.QuotedStr(u_id);
                    }
                    else
                    {
                        sql = "SELECT F.EdocType FROM [EDOC2_FOLDER] E LEFT JOIN Folders D ON E.FOLDER_GUID = D.GUID LEFT JOIN Files F ON D.FID = F.FID WHERE JOB_NO=" + SQLUtils.QuotedStr(OUid);
                    }
                }
                else
                {
                    sql = "SELECT F.EdocType FROM [EDOC2_FOLDER] E LEFT JOIN Folders D ON E.FOLDER_GUID = D.GUID LEFT JOIN Files F ON D.FID = F.FID WHERE JOB_NO=" + SQLUtils.QuotedStr(u_id);
                }

                if (jobList.Count > 1)
                {
                    sql = string.Format("SELECT F.EdocType FROM [EDOC2_FOLDER] E LEFT JOIN Folders D ON E.FOLDER_GUID = D.GUID LEFT JOIN Files F ON D.FID = F.FID WHERE JOB_NO IN {0}", SQLUtils.Quoted(jobList.ToArray()));
                }

                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> edocList = new List<string>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        string EdocType = Prolink.Math.GetValueAsString(item["EdocType"]);
                        edocList.Add(EdocType);
                    }
                }

                string str = "";
                int dnCount = 0;
                if (lightList.Count > 0)
                {
                    dnCount = OperationUtils.GetValueAsInt(string.Format("SELECT COUNT(*) FROM SMDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)), Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                for (int i = 0; i < lightList.Count; i++)
                {
                    sql = "SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='EDT' AND (CMP=" + SQLUtils.QuotedStr(Location) + " OR CMP='*') AND CD=" + SQLUtils.QuotedStr(lightList[i]);
                    string CdDescp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (edocList.Contains(lightList[i]))
                    {
                        switch (lightList[i])
                        {
                            case "INVO":
                            case "PACKI":
                            case "INVI":
                            case "PACKO":
                            case "CONTRACT":
                            case "PO":
                            case "POD":
                                GetLightByDn(dnCount, edocList, lightList[i], ref str, CdDescp);
                                break;
                            default://其它type不用判断
                                str += lightList[i] + "(*)" + CdDescp + "(*)1)*(";
                                break;
                        }
                    }
                    else
                    {
                        str += lightList[i] + "(*)" + CdDescp + "(*)0)*(";
                    }
                }

                str = str.Remove(str.Length - 3);

                sql = "UPDATE {0} SET LIGHT={1} WHERE U_ID={2}";
                sql = string.Format(sql, smsmTable, SQLUtils.QuotedStr(str), SQLUtils.QuotedStr(u_id));
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {

                msg = "error";
            }
            return msg;
        }

        public static DataTable GetEdocData(string shipmentid, string companyid, string OUid = "", bool hasinbound = false)
        {
            string dnsql = string.Format("SELECT COMBINE_INFO,O_LOCATION,MASTER_NO FROM SMSMI WHERE SHIPMENT_ID={0}",
               SQLUtils.QuotedStr(shipmentid));
            DataTable smDt = OperationUtils.GetDataTable(dnsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count <= 0)
                return null;
            if (string.IsNullOrEmpty(shipmentid))
            {
                return null;
            }

            string dninfo = Prolink.Math.GetValueAsString(smDt.Rows[0]["COMBINE_INFO"]);
            string masterno = Prolink.Math.GetValueAsString(smDt.Rows[0]["MASTER_NO"]);

            string sql = string.Format(@"SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,SUBSTRING(SMDN.DN_NO, 7,8)AS DN_NO FROM SMDN WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMINM' AS D_TYPE,SUBSTRING(SMINM.DN_NO, 7,8)AS DN_NO FROM SMINM WHERE SHIPMENT_ID !='' AND SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMIRV' AS D_TYPE,{3} AS DN_NO FROM SMIRV WHERE (SHIPMENT_INFO LIKE '%{1}%' OR SHIPMENT_ID={0}) AND CMP={2}",
               SQLUtils.QuotedStr(shipmentid), shipmentid, SQLUtils.QuotedStr(companyid), SQLUtils.QuotedStr(masterno));
            if (!string.IsNullOrEmpty(OUid))
            {
                sql += string.Format(@"UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO  FROM SMSM WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO FROM SMSM WHERE COMBIN_SHIPMENT ={0}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
            }
            else
            {
                sql += string.Format("UNION SELECT U_ID,GROUP_ID,CMP,'SMSMI' AS D_TYPE,{1} AS DN_NO FROM SMSMI WHERE COMBIN_SHIPMENT ={0} AND COMBIN_SHIPMENT!=SHIPMENT_ID",
                    SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
            }
            string[] dninfos = dninfo.Split(',');
            if (dninfos.Length > 0)
            {
                foreach (string dnno in dninfos)
                {
                    sql += string.Format(" UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,SUBSTRING(SMDN.DN_NO, 7,8)AS DN_NO FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
                    sql += string.Format(" UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,SUBSTRING(SMINM.DN_NO, 7,8)AS DN_NO FROM SMINM WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
                }
            }

            if (hasinbound)
            {
                sql += string.Format(" UNION select U_ID,GROUP_ID,CMP,'SMSMI' AS D_TYPE,{1} AS DN_NO  FROM SMSMI WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
            }

            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Dt;
        }
        public static void GetLightByDn(int dnCount, List<string> edocList, string edocType, ref string str, string cdDescp)
        {
            int count = 0;
            for (int i = 0; i < edocList.Count; i++)
            {
                if (edocType.Equals(edocList[i]))
                {
                    count++;
                }
            }
            str += edocType + "(*)" + cdDescp + "(*)" + (count > 0 && count >= dnCount ? "1" : "0") + ")*(";
        }

        public static string GetExtraSrvInfo(string ShipmentId, string trantype)
        {
            string extrasrv = string.Empty;
            if ("A".Equals(trantype))
            {
                string smsmisql = string.Format("SELECT MASTER_NO,CMP FROM SMSMI  WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                DataTable smsmidt = OperationUtils.GetDataTable(smsmisql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (smsmidt.Rows.Count > 0)
                {
                    string masterno = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["MASTER_NO"]);
                    string smsicmp = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["CMP"]);
                    if (masterno.Length >= 3)
                    {
                        string aircompany = masterno.Substring(0, 3);

                        string bssql = string.Format("select distinct cd+';' from bscode where cd_type='SRV' and cmp={0} AND AP_CD like '%{1}%'  FOR XML PATH('')", SQLUtils.QuotedStr(smsicmp),
                            aircompany);
                        extrasrv = OperationUtils.GetValueAsString(bssql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (!string.IsNullOrEmpty(extrasrv))
                            extrasrv = extrasrv.Trim(';');
                    }
                }
            }
            return extrasrv;
        }

        public static string GetExtraSrvInfo(string trantype, string masterno,string smsicmp)
        {
            string extrasrv = string.Empty;
            if ("A".Equals(trantype))
            {
                if (masterno.Length >= 3)
                {
                    string aircompany = masterno.Substring(0, 3);

                    string bssql = string.Format("select distinct cd+';' from bscode where cd_type='SRV' and cmp={0} AND AP_CD like '%{1}%'  FOR XML PATH('')", SQLUtils.QuotedStr(smsicmp),
                        aircompany);
                    extrasrv = OperationUtils.GetValueAsString(bssql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (!string.IsNullOrEmpty(extrasrv))
                        extrasrv = extrasrv.Trim(';');
                }
            }
            return extrasrv;
        }

    }

    public class IBUserInfo
    {
        public string UserId { get; set; }
        public string CompanyId { get; set; }
        public string GroupId { get; set; }
        public string Upri { get; set; }
        public string Dep { get; set; }
        public string basecondtions { get; set; }
        public string BaseCompanyId { get; set; }
        public string IoFlag { get; set; }
        public string Ext { get; set; }
    }

    public class IBEDIConfig
    {
        public string Cmp { get; set; }
        public string CmpName { get; set; }
        public string PartyNO { get; set; }
        public string Server { get; set; }
        public string User { get; set; }
        public string Psw { get; set; }
        public string SenderCode { get; set; }
        public string RecieveCode { get; set; }
        public string MsgCode { get; set; }
        public string Remark { get; set; }
        public string FunctionCode { get; set; }
        public string Authorization { get; set; }
    }

    public class IBResultInfo
    {
        public bool IsSucceed { get; set; }
        public string ResultCode { get; set; }
        public string Description { get; set; }
    }

    public class EdiInfo
    {
        public string EdiId { get; set; }
        public string Rs { get; set; }
        public string Status { get; set; }
        public string FromCd { get; set; }
        public string ToCd { get; set; }
        public string DataFolder { get; set; }
        public string GroupId { get; set; }
        public object Cmp { get; set; }
        public object Stn { get; set; }
        public string ID { get; set; }
        public string RefNO { get; set; }
        public string MsgInfo { get; set; }
        public string MsgInfo2 { get; set; }
        public string MsgType { get; set; }
        public string Remark { get; set; }
        public string MsgLevel { get; set; }
        public string CreateBy { get; set; }
        public string TCode { get; set; }
        public string IP { get; set; }
        public object Data { get; set; }
    }
}
