using Business.Mail;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using WebGui.Models;
using Prolink.V3;
using Prolink;
using System.Globalization;
using Business.Service;
using Business.TPV;
using Business.TPV.Financial;
using TrackingEDI.InboundBusiness;

namespace Business
{
    public class ReserveManage
    {

        /*叫車檢查*/
        public static string ChkTruck(string ShipmentId)
        {
            string msg = "ok";
            string sql = @"SELECT TRAN_TYPE FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            string TranType = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (TranType == "A" || TranType == "L")
            {
                sql = @"SELECT COUNT(*) FROM SMSMPT  WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND PARTY_TYPE='CR' OR PARTY_TYPE='SP'";
            }
            else
            {
                sql = @"SELECT COUNT(*) FROM SMSMPT  WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND PARTY_TYPE='CR'";
            }
            int n = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (n == 0)
            {
                return @Resources.Locale.L_GateManageController_Controllers_146;
            }

            return msg;
        }

        public static string ChkSmrv(string ShipmentId)
        {
            string msg = "success";
            string sql = @"SELECT STATUS FROM SMIRV  WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                //foreach (DataRow item in dt.Rows)
                //{ 
                //    string Status = Prolink.Math.GetValueAsString(item["STATUS"]);
                //    if(Status == "I")
                //    {
                //        msg = "fail";
                //        break;
                //    }
                //}
                msg = "fail";
            }

           
            return msg;
        }

        public static string ChkShipmentStatus(string ShipmentId)
        {
            string msg = "success";
            string sql = @"SELECT STATUS FROM SMSM  WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            string Status = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if(Status == "V")
            {
                return "Shipment ID:"+ShipmentId+@Resources.Locale.L_ReserveManage_Business_90;
            }


            return msg;
        }

        public static DataTable getTruckerByShipment(string ShipmentId)
        {
            string sql = "SELECT PARTY_NO, PARTY_NAME FROM SMSM, SMSMPT WHERE SMSM.SHIPMENT_ID=SMSMPT.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='CR' AND SMSM.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            return dt;
        }

        public static DataTable getIbTruckerByShipment(string ShipmentId)
        {
            string sql = "SELECT PARTY_NO, PARTY_NAME FROM SMSMI, SMSMIPT WHERE SMSMI.SHIPMENT_ID=SMSMIPT.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='IBCR' AND SMSMI.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            return dt;
        }

        public static DataTable getFwByShipment(string ShipmentId)
        {
            string sql = "SELECT PARTY_NO, PARTY_NAME FROM SMSM, SMSMPT WHERE SMSM.U_ID=SMSMPT.U_FID AND SMSMPT.PARTY_TYPE='FS' AND SMSM.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            return dt;
        }

        public static DataTable getShByShipment(string ShipmentId)
        {
            string sql = "SELECT PARTY_NO, PARTY_NAME FROM SMSM, SMSMPT WHERE SMSM.U_ID=SMSMPT.U_FID AND SMSMPT.PARTY_TYPE='FS' AND SMSM.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            return dt;
        }

        public static DataTable getCaBySmId(string ShipmentId)
        {
            string sql = "SELECT CARRIER, CARRIER_NM FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            return dt;
        }

        public static string getAutoNo(string ruleCode, string GroupId, string CompanyId)
        {
            return TrackingEDI.InboundBusiness.ReserveHelper.getAutoNo(ruleCode, GroupId, CompanyId);
        }

        public static string OrderTrucker(string ShipmentId, string GroupId, string CompanyId, string Dep, string Ext, string UserId,List<string> idList=null, int del = 0, decimal CntNumber = 0, string CombineDate="", string IsBatch="N", string WsCd="", string DnNo="",string cnttype="")
        {
            string returnMessage = "success";

            string sql = "";
            MixedList mixList = new MixedList();
            DataTable dt = new DataTable();
            string chk = Business.ReserveManage.ChkTruck(ShipmentId);
            string chkSmrv = Business.ReserveManage.ChkSmrv(ShipmentId);
            string chkSmStatus = ReserveManage.ChkShipmentStatus(ShipmentId);

            if(chkSmStatus != "success")
            {
                return chkSmStatus;
            }

            if (chk != "ok")
            {
                return chk;
            }

            if (del == 1)
            {
                sql = "DELETE FROM SMIRV WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId); //如果有需要刪除，就跑這裡
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            sql = @"SELECT TOP 1 SMSM.GROUP_ID, SMSM.CMP, SMSM.DN_NO,SMSM.COMBINE_INFO, SMSM.SHIPMENT_INFO, SMSM.SHIPMENT_ID, SMSM.CNT_NUMBER, (SELECT TOP 1 PRODUCT_LINE FROM SMDNP WHERE DN_NO={0} AND PRODUCT_LINE IS NOT NULL) AS PRODUCT_LINE, TRAN_TYPE, CUT_PORT_DATE, PORT_DATE, CARGO_TYPE, CNT20, CNT40, CNT40HQ, CNT_TYPE, GW, CBM, CREATE_BY, BL_WIN, CAR_QTY, CAR_QTY1, CAR_QTY2, CAR_TYPE, CAR_TYPE1, CAR_TYPE2 
                             ,SMSM.PLANT FROM SMSM
                             WHERE  SMSM.SHIPMENT_ID={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(ShipmentId));
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DnSealSataue(ShipmentId);
            if (dt.Rows.Count > 0)
            {
                string UId = "";
                foreach (DataRow item in dt.Rows)
                {
                    bool success = false;
                    string ReserveNo = "", BatNo = "";

                    decimal Cnt20 = Prolink.Math.GetValueAsDecimal(item["CNT20"]);
                    decimal Cnt40 = Prolink.Math.GetValueAsDecimal(item["CNT40"]);
                    decimal Cnt40hq = Prolink.Math.GetValueAsDecimal(item["CNT40HQ"]);
                    decimal cntNumber = Prolink.Math.GetValueAsDecimal(item["CNT_NUMBER"]);

                    decimal CarQty  = Prolink.Math.GetValueAsDecimal(item["CAR_QTY"]);
                    decimal CarQty1 = Prolink.Math.GetValueAsDecimal(item["CAR_QTY1"]);
                    decimal CarQty2 = Prolink.Math.GetValueAsDecimal(item["CAR_QTY2"]);

                    decimal Gw = Prolink.Math.GetValueAsDecimal(item["GW"]);
                    decimal Cbm = Prolink.Math.GetValueAsDecimal(item["CBM"]);
                    string CntType = Prolink.Math.GetValueAsString(item["CNT_TYPE"]);
                    if (!string.IsNullOrEmpty(cnttype))
                    {
                        CntType = cnttype;
                    }

                    string CombineInfo = Prolink.Math.GetValueAsString(item["COMBINE_INFO"]);
                    if(string.IsNullOrEmpty(DnNo))
                    {
                        DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                    }
                    string ShipmentInfo = Prolink.Math.GetValueAsString(item["SHIPMENT_INFO"]);
                    string ProductLine = Prolink.Math.GetValueAsString(item["PRODUCT_LINE"]);
                    string TranType = Prolink.Math.GetValueAsString(item["TRAN_TYPE"]);
                    string CargoType = Prolink.Math.GetValueAsString(item["CARGO_TYPE"]);
                    string SmCreateBy = Prolink.Math.GetValueAsString(item["IB_WINDOW"]); //訂艙人員
                    DateTime CutPortDate = Prolink.Math.GetValueAsDateTime(item["CUT_PORT_DATE"]);
                    DateTime PortDate = Prolink.Math.GetValueAsDateTime(item["PORT_DATE"]);
                    string plant = Prolink.Math.GetValueAsString(item["PLANT"]);

                    UId = System.Guid.NewGuid().ToString();
                    string Trucker = "", TruckerNm = "", Carrier = "", CarrierNm = "";

                    if (CombineInfo != "")
                    {
                        DnNo = CombineInfo;
                    }

                    if (TranType == "A" || TranType == "L")
                    {
                        DataTable TruckerDt = Business.ReserveManage.getTruckerByShipment(ShipmentId);
                        if (TruckerDt.Rows.Count > 0)
                        {
                            foreach (DataRow titem in TruckerDt.Rows)
                            {
                                Trucker = Prolink.Math.GetValueAsString(titem["PARTY_NO"]);
                                TruckerNm = Prolink.Math.GetValueAsString(titem["PARTY_NAME"]);
                            }
                        }

                        DataTable CarrierDt = Business.ReserveManage.getCaBySmId(ShipmentId);
                        if (CarrierDt.Rows.Count > 0)
                        {
                            foreach (DataRow citem in CarrierDt.Rows)
                            {
                                Carrier = Prolink.Math.GetValueAsString(citem["CARRIER"]);
                                CarrierNm = Prolink.Math.GetValueAsString(citem["CARRIER_NM"]);
                            }
                        }

                        if(Carrier == "" && CarrierNm == "")
                        {
                            CarrierDt = Business.ReserveManage.getShByShipment(ShipmentId);
                            if (CarrierDt.Rows.Count > 0)
                            {
                                foreach (DataRow citem in CarrierDt.Rows)
                                {
                                    Carrier = Prolink.Math.GetValueAsString(citem["PARTY_NO"]);
                                    CarrierNm = Prolink.Math.GetValueAsString(citem["PARTY_NAME"]);
                                }
                            }
                        }

                        if (Trucker == "")
                        {
                            TruckerDt = Business.ReserveManage.getFwByShipment(ShipmentId);
                            if (TruckerDt.Rows.Count > 0)
                            {
                                foreach (DataRow titem in TruckerDt.Rows)
                                {
                                    Trucker = Prolink.Math.GetValueAsString(titem["PARTY_NO"]);
                                    TruckerNm = Prolink.Math.GetValueAsString(titem["PARTY_NAME"]);
                                }
                            }
                        }
                    }
                    else
                    {
                        DataTable TruckerDt = Business.ReserveManage.getTruckerByShipment(ShipmentId);

                        if (TruckerDt.Rows.Count > 0)
                        {
                            foreach (DataRow titem in TruckerDt.Rows)
                            {
                                Trucker = Prolink.Math.GetValueAsString(titem["PARTY_NO"]);
                                TruckerNm = Prolink.Math.GetValueAsString(titem["PARTY_NAME"]);
                            }
                        }

                        if (TranType == "F")
                        {
                            DataTable CarrierDt = Business.ReserveManage.getCaBySmId(ShipmentId);
                            if (CarrierDt.Rows.Count > 0)
                            {
                                foreach (DataRow citem in CarrierDt.Rows)
                                {
                                    Carrier = Prolink.Math.GetValueAsString(citem["CARRIER"]);
                                    CarrierNm = Prolink.Math.GetValueAsString(citem["CARRIER_NM"]);
                                }
                            }

                            if (Carrier == "" && CarrierNm == "")
                            {
                                CarrierDt = Business.ReserveManage.getShByShipment(ShipmentId);
                                if (CarrierDt.Rows.Count > 0)
                                {
                                    foreach (DataRow citem in CarrierDt.Rows)
                                    {
                                        Carrier = Prolink.Math.GetValueAsString(citem["PARTY_NO"]);
                                        CarrierNm = Prolink.Math.GetValueAsString(citem["PARTY_NAME"]);
                                    }
                                }
                            }

                        }
                    }

                    string MfNo = string.Empty;
                    if (ProductLine != "" && WsCd == "")
                    {
                        sql = "SELECT * FROM SMWH WHERE GROUP_ID={0} AND CMP={1} AND PRODUCT_LINE LIKE '%{2}%' ";
                        sql = string.Format(sql, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), ProductLine);
                        DataTable whdt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        foreach (DataRow dr in whdt.Rows)
                        {
                            string[] pline = Prolink.Math.GetValueAsString(dr["PRODUCT_LINE"]).Split('/');
                            if (pline.Contains(ProductLine)) {
                                WsCd = Prolink.Math.GetValueAsString(dr["WS_CD"]);
                            }
                        }
                    }

                    if (WsCd != "")
                    {
                        sql = "SELECT MF_NO FROM SMWH WHERE GROUP_ID={0} AND CMP={1} AND WS_CD={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(WsCd));
                        MfNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }

                    string Dim = "";
                    if (DnNo != "")
                    {
                        sql = "SELECT TOP 1 (convert(varchar(10),L) + 'X' + convert(varchar(10),W) + 'X' + convert(varchar(10),H) + 'X' + convert(varchar(10),VW) + 'CTN') AS DIM FROM SMCUFT WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                        Dim = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }

                    string CarType = Prolink.Math.GetValueAsString(item["CAR_TYPE"]);
                    //string sqlStringCol = @"INSERT INTO SMIRV (U_ID, RESERVE_NO, STATUS, GROUP_ID, CMP, DN_NO, PRODUCT_LINE, SHIPMENT_INFO, DEP, CREATE_BY, CREATE_DATE,CREATE_CMP, CREATE_DEP, CREATE_EXT, CALL_DATE, TRUCKER, WS_CD, SHIPMENT_ID, USE_DATE, CARRIER, CARRIER_NM,RESERVE_DATE,TEMP_WSCD, TEMP_RDATE, TRUCKER_NM, IS_BATCH, TRAN_TYPE, CARGO_TYPE, CUT_PORT_DATE)"; 
                    //string sqlStringVal = @" VALUES (" + SQLUtils.QuotedStr(UId) + "," + SQLUtils.QuotedStr(ReserveNo) + ",'D'," + SQLUtils.QuotedStr(GroupId) + "," + SQLUtils.QuotedStr(CompanyId) + "," + SQLUtils.QuotedStr(DnNo) + "," + SQLUtils.QuotedStr(ProductLine) + "," + SQLUtils.QuotedStr(ShipmentInfo) + "," + SQLUtils.QuotedStr(Dep) + "," + SQLUtils.QuotedStr(UserId) + "," + SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd HH:mm")) + "," + SQLUtils.QuotedStr(CompanyId) + "," + SQLUtils.QuotedStr(Dep) + "," + SQLUtils.QuotedStr(Ext) + "," + SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd HH:mm")) + "," + SQLUtils.QuotedStr(Trucker) + "," + SQLUtils.QuotedStr(WsCd) + "," + SQLUtils.QuotedStr(ShipmentId) + "," + SQLUtils.QuotedStr(CombineDate) + "," + SQLUtils.QuotedStr(Carrier) + "," + SQLUtils.QuotedStr(CarrierNm) + "," + SQLUtils.QuotedStr(CombineDate) + "," + SQLUtils.QuotedStr(WsCd) + "," + SQLUtils.QuotedStr(CombineDate) + "," + SQLUtils.QuotedStr(TruckerNm) + "," + SQLUtils.QuotedStr(IsBatch) + "," + SQLUtils.QuotedStr(TranType) + "," + SQLUtils.QuotedStr(CargoType) + "," + CutPortDate + ")";   
                    if (Trucker == "")
                    {
                        returnMessage = @Resources.Locale.L_DnHandel_Business_56;
                    }
                    else
                    {
                        if (SMHandle.CheckEDoc(ShipmentId, Trucker, "CR", "*", ""))
                        {
                            
                        }
                        EditInstruct ei;
                        if (CntNumber > 0)
                        {
                            #region 叫车管理有输柜量
                            for (int i = 0; i < CntNumber; i++)
                            {
                                ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                UId = System.Guid.NewGuid().ToString();
                                ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                ei.Put("U_ID", UId);
                                ei.Put("RESERVE_NO", ReserveNo);
                                ei.Put("STATUS", 'D');
                                ei.Put("GROUP_ID", GroupId);
                                ei.Put("CMP", CompanyId);
                                ei.Put("DN_NO", DnNo);
                                ei.Put("PRODUCT_LINE", ProductLine);
                                ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                ei.Put("WS_CD", WsCd);
                                ei.Put("MF_NO", MfNo);
                                ei.Put("SHIPMENT_ID", ShipmentId);
                                ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                ei.Put("CARRIER", Carrier);
                                ei.Put("CARRIER_NM", CarrierNm);
                                ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                ei.Put("TEMP_WSCD", WsCd);
                                ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                ei.Put("TRUCKER_NM", TruckerNm);
                                ei.Put("IS_BATCH", IsBatch);
                                ei.Put("TRAN_TYPE", TranType);
                                ei.Put("CARGO_TYPE", CargoType);
                                ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                ei.PutDate("PORT_DATE", PortDate);
                                ei.Put("CNT_TYPE", CntType);
                                ei.Put("CNT_NUMBER", CntNumber);
                                ei.Put("GW", Gw);
                                ei.Put("CBM", Cbm);
                                ei.Put("DIM", Dim);
                                ei.Put("SMCREATE_BY", SmCreateBy);
                                ei.Put("PLANT", plant);
                                if (idList != null && !idList.Contains(UId))
                                    idList.Add(UId);
                                if ("E".Equals(TranType))
                                {
                                    string ptsql = string.Format("select * from SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE='CR'", SQLUtils.QuotedStr(ShipmentId));
                                    DataTable ptdt = OperationUtils.GetDataTable(ptsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    if (ptdt != null && ptdt.Rows.Count > 0)
                                    {
                                        string partyno = Prolink.Math.GetValueAsString(ptdt.Rows[0]["PARTY_NO"]);
                                        string partycmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                                        if (!string.IsNullOrEmpty(partyno))
                                        {
                                            bool mail = false;
                                            string carsql = string.Format("SELECT TOP 1 * FROM BSTRUCKC WHERE PARTY_NO={0} AND SP_PLANT={1}", SQLUtils.QuotedStr(partyno), SQLUtils.QuotedStr(partycmp));
                                            DataTable cardt = OperationUtils.GetDataTable(carsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                            if (cardt != null && cardt.Rows.Count > 0)
                                            {
                                                string truck = Prolink.Math.GetValueAsString(cardt.Rows[0]["TRUCK_NO"]);
                                                ei.Put("TRUCK_NO", truck);
                                                ei.Put("LTRUCK_NO", truck); 
                                                mail = true;
                                            }
                                            string persql = string.Format("SELECT TOP 1 * FROM BSTRUCKD WHERE PARTY_NO={0} AND SP_PLANT_D={1}", SQLUtils.QuotedStr(partyno), SQLUtils.QuotedStr(partycmp));
                                            DataTable perdt = OperationUtils.GetDataTable(persql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                            if (perdt != null && perdt.Rows.Count > 0)
                                            {
                                                string Driver = Prolink.Math.GetValueAsString(perdt.Rows[0]["DRIVER_NAME"]);
                                                string phone = Prolink.Math.GetValueAsString(perdt.Rows[0]["DRIVER_PHONE"]);
                                                string driverid = Prolink.Math.GetValueAsString(perdt.Rows[0]["DRIVER_ID"]);
                                                ei.Put("DRIVER", Driver);
                                                ei.Put("TEL", phone);
                                                ei.Put("DRIVER_ID", driverid);

                                                ei.Put("LDRIVER", Driver);
                                                ei.Put("LTEL", phone);
                                                ei.Put("LDRIVER_ID", driverid);   
                                                mail = true;
                                            }
                                            if (mail)
                                            {
                                                //发送mail
                                            }
                                        }
                                    }
                                }
                                mixList.Add(ei);
                            }
                            #endregion
                        }
                        else
                        {
                            if (TranType == "F")
                            {
                                #region FCL 需判断柜量叫车
                                for (int i = 0; i < Cnt20; i++)
                                {
                                    ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                    UId = System.Guid.NewGuid().ToString();
                                    ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                    ei.Put("U_ID", UId);
                                    ei.Put("RESERVE_NO", ReserveNo);
                                    ei.Put("STATUS", 'D');
                                    ei.Put("GROUP_ID", GroupId);
                                    ei.Put("CMP", CompanyId);
                                    ei.Put("DN_NO", DnNo);
                                    ei.Put("PRODUCT_LINE", ProductLine);
                                    ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                    ei.Put("WS_CD", WsCd);
                                    ei.Put("MF_NO", MfNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("CARRIER", Carrier);
                                    ei.Put("CARRIER_NM", CarrierNm);
                                    ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TEMP_WSCD", WsCd);
                                    ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TRUCKER_NM", TruckerNm);
                                    ei.Put("IS_BATCH", IsBatch);
                                    ei.Put("TRAN_TYPE", TranType);
                                    ei.Put("CARGO_TYPE", CargoType);
                                    ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                    ei.PutDate("PORT_DATE", PortDate);
                                    ei.Put("CNT_TYPE", "20GP");
                                    ei.Put("CNT_NUMBER", Cnt20);
                                    ei.Put("GW", Gw);
                                    ei.Put("CBM", Cbm);
                                    ei.Put("DIM", Dim);
                                    ei.Put("SMCREATE_BY", SmCreateBy);
                                    ei.Put("PLANT", plant);
                                    mixList.Add(ei);

                                    if (idList != null && !idList.Contains(UId))
                                        idList.Add(UId);
                                }

                                for (int i = 0; i < Cnt40; i++)
                                {
                                    ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                    UId = System.Guid.NewGuid().ToString();
                                    ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                    ei.Put("U_ID", UId);
                                    ei.Put("RESERVE_NO", ReserveNo);
                                    ei.Put("STATUS", 'D');
                                    ei.Put("GROUP_ID", GroupId);
                                    ei.Put("CMP", CompanyId);
                                    ei.Put("DN_NO", DnNo);
                                    ei.Put("PRODUCT_LINE", ProductLine);
                                    ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                    ei.Put("WS_CD", WsCd);
                                    ei.Put("MF_NO", MfNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("CARRIER", Carrier);
                                    ei.Put("CARRIER_NM", CarrierNm);
                                    ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TEMP_WSCD", WsCd);
                                    ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TRUCKER_NM", TruckerNm);
                                    ei.Put("IS_BATCH", IsBatch);
                                    ei.Put("TRAN_TYPE", TranType);
                                    ei.Put("CARGO_TYPE", CargoType);
                                    ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                    ei.PutDate("PORT_DATE", PortDate);
                                    ei.Put("CNT_TYPE", "40GP");
                                    ei.Put("CNT_NUMBER", Cnt40);
                                    ei.Put("GW", Gw);
                                    ei.Put("CBM", Cbm);
                                    ei.Put("DIM", Dim);
                                    ei.Put("SMCREATE_BY", SmCreateBy);
                                    ei.Put("PLANT", plant);
                                    mixList.Add(ei);

                                    if (idList != null && !idList.Contains(UId))
                                        idList.Add(UId);
                                }

                                for (int i = 0; i < Cnt40hq; i++)
                                {
                                    ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                    UId = System.Guid.NewGuid().ToString();
                                    ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);

                                    ei.Put("U_ID", UId);
                                    ei.Put("RESERVE_NO", ReserveNo);
                                    ei.Put("STATUS", 'D');
                                    ei.Put("GROUP_ID", GroupId);
                                    ei.Put("CMP", CompanyId);
                                    ei.Put("DN_NO", DnNo);
                                    ei.Put("PRODUCT_LINE", ProductLine);
                                    ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                    ei.Put("WS_CD", WsCd);
                                    ei.Put("MF_NO", MfNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("CARRIER", Carrier);
                                    ei.Put("CARRIER_NM", CarrierNm);
                                    ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TEMP_WSCD", WsCd);
                                    ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TRUCKER_NM", TruckerNm);
                                    ei.Put("IS_BATCH", IsBatch);
                                    ei.Put("TRAN_TYPE", TranType);
                                    ei.Put("CARGO_TYPE", CargoType);
                                    ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                    ei.PutDate("PORT_DATE", PortDate);
                                    ei.Put("CNT_TYPE", "40HQ");
                                    ei.Put("CNT_NUMBER", Cnt40hq);
                                    ei.Put("GW", Gw);
                                    ei.Put("CBM", Cbm);
                                    ei.Put("DIM", Dim);
                                    ei.Put("SMCREATE_BY", SmCreateBy);
                                    ei.Put("PLANT", plant);
                                    mixList.Add(ei);

                                    if (idList != null && !idList.Contains(UId))
                                        idList.Add(UId);
                                }
                                #endregion

                                if(Cnt20 == 0 && Cnt40 == 0 && Cnt40hq == 0)
                                {
                                    #region SMSM柜量与叫车管理柜量都为0时
                                    ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                    UId = System.Guid.NewGuid().ToString();
                                    ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                    ei.Put("U_ID", UId);
                                    ei.Put("RESERVE_NO", ReserveNo);
                                    ei.Put("STATUS", 'D');
                                    ei.Put("GROUP_ID", GroupId);
                                    ei.Put("CMP", CompanyId);
                                    ei.Put("DN_NO", DnNo);
                                    ei.Put("PRODUCT_LINE", ProductLine);
                                    ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                    ei.Put("WS_CD", WsCd);
                                    ei.Put("MF_NO", MfNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("CARRIER", Carrier);
                                    ei.Put("CARRIER_NM", CarrierNm);
                                    ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TEMP_WSCD", WsCd);
                                    ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TRUCKER_NM", TruckerNm);
                                    ei.Put("IS_BATCH", IsBatch);
                                    ei.Put("TRAN_TYPE", TranType);
                                    ei.Put("CARGO_TYPE", CargoType);
                                    ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                    ei.PutDate("PORT_DATE", PortDate);
                                    ei.Put("CNT_NUMBER", 1);
                                    ei.Put("GW", Gw);
                                    ei.Put("CBM", Cbm);
                                    ei.Put("DIM", Dim);
                                    ei.Put("SMCREATE_BY", SmCreateBy);
                                    ei.Put("PLANT", plant);
                                    mixList.Add(ei);

                                    if (idList != null && !idList.Contains(UId))
                                        idList.Add(UId);
                                    #endregion
                                }
                            }
                            else if (TranType == "T")
                            {
                                if (CarQty != 0 || CarQty1 != 0 || CarQty2 != 0)
                                {
                                    #region 内贸 车数不为零 需判断车数叫车
                                    for (int i = 0; i < CarQty; i++)
                                    {

                                        ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                        UId = System.Guid.NewGuid().ToString();
                                        ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                        ei.Put("U_ID", UId);
                                        ei.Put("RESERVE_NO", ReserveNo);
                                        ei.Put("STATUS", 'D');
                                        ei.Put("GROUP_ID", GroupId);
                                        ei.Put("CMP", CompanyId);
                                        ei.Put("DN_NO", DnNo);
                                        ei.Put("PRODUCT_LINE", ProductLine);
                                        ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                        ei.Put("WS_CD", WsCd);
                                        ei.Put("MF_NO", MfNo);
                                        ei.Put("SHIPMENT_ID", ShipmentId);
                                        ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("CARRIER", Carrier);
                                        ei.Put("CARRIER_NM", CarrierNm);
                                        ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("TEMP_WSCD", WsCd);
                                        ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("TRUCKER_NM", TruckerNm);
                                        ei.Put("IS_BATCH", IsBatch);
                                        ei.Put("TRAN_TYPE", TranType);
                                        ei.Put("CARGO_TYPE", CargoType);
                                        ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                        ei.PutDate("PORT_DATE", PortDate);
                                        ei.Put("CNT_TYPE", CarType);
                                        ei.Put("CNT_NUMBER", Cnt20);
                                        ei.Put("GW", Gw);
                                        ei.Put("CBM", Cbm);
                                        ei.Put("DIM", Dim);
                                        ei.Put("SMCREATE_BY", SmCreateBy);
                                        ei.Put("PLANT", plant);
                                        mixList.Add(ei);

                                        if (idList != null && !idList.Contains(UId))
                                            idList.Add(UId);
                                    }

                                    for (int i = 0; i < CarQty1; i++)
                                    {
                                        ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                        UId = System.Guid.NewGuid().ToString();
                                        ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                        ei.Put("U_ID", UId);
                                        ei.Put("RESERVE_NO", ReserveNo);
                                        ei.Put("STATUS", 'D');
                                        ei.Put("GROUP_ID", GroupId);
                                        ei.Put("CMP", CompanyId);
                                        ei.Put("DN_NO", DnNo);
                                        ei.Put("PRODUCT_LINE", ProductLine);
                                        ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                        ei.Put("WS_CD", WsCd);
                                        ei.Put("MF_NO", MfNo);
                                        ei.Put("SHIPMENT_ID", ShipmentId);
                                        ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("CARRIER", Carrier);
                                        ei.Put("CARRIER_NM", CarrierNm);
                                        ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("TEMP_WSCD", WsCd);
                                        ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("TRUCKER_NM", TruckerNm);
                                        ei.Put("IS_BATCH", IsBatch);
                                        ei.Put("TRAN_TYPE", TranType);
                                        ei.Put("CARGO_TYPE", CargoType);
                                        ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                        ei.PutDate("PORT_DATE", PortDate);
                                        ei.Put("CNT_TYPE", CarType);
                                        ei.Put("CNT_NUMBER", CarQty1);
                                        ei.Put("GW", Gw);
                                        ei.Put("CBM", Cbm);
                                        ei.Put("DIM", Dim);
                                        ei.Put("SMCREATE_BY", SmCreateBy);
                                        ei.Put("PLANT", plant);
                                        mixList.Add(ei);

                                        if (idList != null && !idList.Contains(UId))
                                            idList.Add(UId);
                                    }

                                    for (int i = 0; i < CarQty2; i++)
                                    {
                                        ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                        UId = System.Guid.NewGuid().ToString();
                                        ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);

                                        ei.Put("U_ID", UId);
                                        ei.Put("RESERVE_NO", ReserveNo);
                                        ei.Put("STATUS", 'D');
                                        ei.Put("GROUP_ID", GroupId);
                                        ei.Put("CMP", CompanyId);
                                        ei.Put("DN_NO", DnNo);
                                        ei.Put("PRODUCT_LINE", ProductLine);
                                        ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                        ei.Put("WS_CD", WsCd);
                                        ei.Put("MF_NO", MfNo);
                                        ei.Put("SHIPMENT_ID", ShipmentId);
                                        ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("CARRIER", Carrier);
                                        ei.Put("CARRIER_NM", CarrierNm);
                                        ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("TEMP_WSCD", WsCd);
                                        ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                        ei.Put("TRUCKER_NM", TruckerNm);
                                        ei.Put("IS_BATCH", IsBatch);
                                        ei.Put("TRAN_TYPE", TranType);
                                        ei.Put("CARGO_TYPE", CargoType);
                                        ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                        ei.PutDate("PORT_DATE", PortDate);
                                        ei.Put("CNT_TYPE", CarType);
                                        ei.Put("CNT_NUMBER", CarQty2);
                                        ei.Put("GW", Gw);
                                        ei.Put("CBM", Cbm);
                                        ei.Put("DIM", Dim);
                                        ei.Put("SMCREATE_BY", SmCreateBy);
                                        ei.Put("PLANT", plant);
                                        mixList.Add(ei);

                                        if (idList != null && !idList.Contains(UId))
                                            idList.Add(UId);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region SMSM柜量与叫车管理柜量都为0时
                                    ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                    UId = System.Guid.NewGuid().ToString();
                                    ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                    ei.Put("U_ID", UId);
                                    ei.Put("RESERVE_NO", ReserveNo);
                                    ei.Put("STATUS", 'D');
                                    ei.Put("GROUP_ID", GroupId);
                                    ei.Put("CMP", CompanyId);
                                    ei.Put("DN_NO", DnNo);
                                    ei.Put("PRODUCT_LINE", ProductLine);
                                    ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                    ei.Put("WS_CD", WsCd);
                                    ei.Put("MF_NO", MfNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("CARRIER", Carrier);
                                    ei.Put("CARRIER_NM", CarrierNm);
                                    ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TEMP_WSCD", WsCd);
                                    ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                    ei.Put("TRUCKER_NM", TruckerNm);
                                    ei.Put("IS_BATCH", IsBatch);
                                    ei.Put("TRAN_TYPE", TranType);
                                    ei.Put("CARGO_TYPE", CargoType);
                                    ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                    ei.PutDate("PORT_DATE", PortDate);
                                    ei.Put("CNT_NUMBER", 1);
                                    ei.Put("GW", Gw);
                                    ei.Put("CBM", Cbm);
                                    ei.Put("DIM", Dim);
                                    ei.Put("SMCREATE_BY", SmCreateBy);
                                    ei.Put("PLANT", plant);
                                    mixList.Add(ei);

                                    if (idList != null && !idList.Contains(UId))
                                        idList.Add(UId);
                                    #endregion
                                }
                            }
                            else
                            {
                                #region SMSM柜量与叫车管理柜量都为0时
                                ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                                UId = System.Guid.NewGuid().ToString();
                                ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                                ei.Put("U_ID", UId);
                                ei.Put("RESERVE_NO", ReserveNo);
                                ei.Put("STATUS", 'D');
                                ei.Put("GROUP_ID", GroupId);
                                ei.Put("CMP", CompanyId);
                                ei.Put("DN_NO", DnNo);
                                ei.Put("PRODUCT_LINE", ProductLine);
                                ei.Put("SHIPMENT_INFO", ShipmentInfo);
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
                                ei.Put("WS_CD", WsCd);
                                ei.Put("MF_NO", MfNo);
                                ei.Put("SHIPMENT_ID", ShipmentId);
                                ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                ei.Put("CARRIER", Carrier);
                                ei.Put("CARRIER_NM", CarrierNm);
                                ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                ei.Put("TEMP_WSCD", WsCd);
                                ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                                ei.Put("TRUCKER_NM", TruckerNm);
                                ei.Put("IS_BATCH", IsBatch);
                                ei.Put("TRAN_TYPE", TranType);
                                ei.Put("CARGO_TYPE", CargoType);
                                ei.PutDate("CUT_PORT_DATE", CutPortDate);
                                ei.PutDate("PORT_DATE", PortDate);
                                ei.Put("CNT_NUMBER", 1);
                                ei.Put("GW", Gw);
                                ei.Put("CBM", Cbm);
                                ei.Put("DIM", Dim);
                                ei.Put("SMCREATE_BY", SmCreateBy);
                                ei.Put("PLANT", plant);

                                if (idList != null && !idList.Contains(UId))
                                    idList.Add(UId);
                                if (TranType.Equals("E"))
                                {
                                    string ptsql = string.Format("select * from SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE='CR'", SQLUtils.QuotedStr(ShipmentId));
                                    DataTable ptdt = OperationUtils.GetDataTable(ptsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    if (ptdt != null && ptdt.Rows.Count > 0)
                                    {
                                        string partyno = Prolink.Math.GetValueAsString(ptdt.Rows[0]["PARTY_NO"]);
                                        string partycmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                                        if (!string.IsNullOrEmpty(partyno))
                                        {
                                            bool mail = false;
                                            string carsql = string.Format("SELECT TOP 1 * FROM BSTRUCKC WHERE PARTY_NO={0} AND CMP={1} AND SP_PLANT={1}", SQLUtils.QuotedStr(partyno), SQLUtils.QuotedStr(partycmp));
                                            DataTable cardt = OperationUtils.GetDataTable(carsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                            if (cardt != null && cardt.Rows.Count > 0)
                                            {
                                                string truck = Prolink.Math.GetValueAsString(cardt.Rows[0]["TRUCK_NO"]);
                                                ei.Put("TRUCK_NO", truck);
                                                ei.Put("LTRUCK_NO", truck); 
                                                mail = true;
                                            }
                                            string persql = string.Format("SELECT TOP 1 * FROM BSTRUCKD WHERE PARTY_NO={0} AND CMP={1} AND SP_PLANT_D={1}", SQLUtils.QuotedStr(partyno), SQLUtils.QuotedStr(partycmp));
                                            DataTable perdt = OperationUtils.GetDataTable(persql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                            if (perdt != null && perdt.Rows.Count > 0)
                                            {
                                                string Driver=Prolink.Math.GetValueAsString(perdt.Rows[0]["DRIVER_NAME"]);
                                                string phone=Prolink.Math.GetValueAsString(perdt.Rows[0]["DRIVER_PHONE"]);
                                                string driverid=Prolink.Math.GetValueAsString(perdt.Rows[0]["DRIVER_ID"]);
                                                ei.Put("DRIVER", Driver);
                                                ei.Put("TEL", phone);
                                                ei.Put("DRIVER_ID", driverid);

                                                ei.Put("LDRIVER", Driver);
                                                ei.Put("LTEL", phone);
                                                ei.Put("LDRIVER_ID", driverid);                                                    
                                                mail = true;
                                            }
                                            if (mail)
                                            {
                                                //发送mail
                                            }
                                        }
                                    }
                                }
                                mixList.Add(ei);
                                #endregion
                            }
                        }

                        try
                        {
                            OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            returnMessage = @Resources.Locale.L_ReserveManage_Business_93;
                            break;
                        }

                        if (success == true)
                        {
                            sql = @"UPDATE SMSM SET TORDER='C', PICKUP_CDATE=" + SQLUtils.QuotedStr(CombineDate) + " WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                            sql = @"SELECT U_ID FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            string sm_id = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            Business.ReserveManage.updateShipmentStatus(sm_id, "D");

                            DataTable mailGroupDt = MailTemplate.GetMailGroup(Trucker, GroupId, "C");
                            if (mailGroupDt.Rows.Count > 0)
                            {
                                foreach (DataRow item1 in mailGroupDt.Rows)
                                {
                                    string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                                    if (mailStr != "")
                                    {
                                        EvenFactory.AddEven(UId + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), UId, MailManager.RVNotify, null, 1, 0, mailStr, @Resources.Locale.L_ReserveManage_Business_94 + ReserveNo, "");
                                    }
                                }
                            }
                            
                        }
                    }

                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_150;
            }

            return returnMessage;
        }
        public static void DnSealSataue(string id)
        {
            string sql = string.Format("UPDATE SMDN SET SEAL_SATAUE ='N' WHERE SHIPMENT_ID IN ({0})", SQLUtils.QuotedStr(id));
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        public static string CancelTrucker(string UId,string userid)
        {
            string returnMessage = "success", Trucker = "", GroupId = "", ShipmentId = "", CompanyId = "", ReserveNo = "", TruckerNm="", DnNo = "";

            string sql = "SELECT * FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    Trucker = Prolink.Math.GetValueAsString(item["TRUCKER"]);
                    TruckerNm = Prolink.Math.GetValueAsString(item["TRUCKER_NM"]);
                    GroupId = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                    CompanyId = Prolink.Math.GetValueAsString(item["CMP"]);
                    ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                    ReserveNo = Prolink.Math.GetValueAsString(item["RESERVE_NO"]);
                    DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                }
            }


            DataTable mailGroupDt = MailTemplate.GetMailGroup(Trucker, GroupId,"C");
            if (mailGroupDt.Rows.Count > 0)
            {
                foreach (DataRow item1 in mailGroupDt.Rows)
                {
                    string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                    if (mailStr != "")
                    {
                        //EvenFactory.AddEven(ShipmentId + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), ShipmentId, MailManager.RVNotify, null, 1, 0, mailStr, "取消叫車", "");
                        EvenFactory.AddEven(ReserveNo + "#" + ShipmentId + "#" + DnNo + "#" + TruckerNm + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), UId, MailManager.RVC, null, 1, 0, mailStr, @Resources.Locale.L_ReserveManage_Business_95 + ReserveNo, "");
                    }
                }
            }

            try {
                MixedList ml = new MixedList();
                sql = "DELETE FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                ml.Add(sql);

                TmexpHandler th = new TmexpHandler();
                TmexpInfo tpi = new TmexpInfo();
                tpi.UFid = Guid.NewGuid().ToString();
                tpi.WrId = userid;
                tpi.WrDate = DateTime.Now;
                tpi.Cmp = CompanyId;
                tpi.GroupId = GroupId;
                tpi.JobNo = ShipmentId;
                tpi.ExpType = "SM";
                tpi.ExpReason = "TK_CANCEL";
                tpi.ExpText = @Resources.Locale.L_ReserveManage_Business_0+ userid + @Resources.Locale.L_ReserveManage_Business_97 + ReserveNo + "！";
                tpi.ExpObj = userid;
                ml.Add(th.SetTmexpEi(tpi));

                Prolink.DataOperation.OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = "SELECT COUNT(1) FROM SMIRV WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                int counts = Prolink.DataOperation.OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (counts <= 0)
                {
                    MixedList mixlist = new MixedList();
                    EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentId);
                    ei.Put("TORDER", "S");
                    mixlist.Add(ei);
                    ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentId);
                    ei.PutKey("STATUS", "D");
                    ei.Put("TORDER", "S");
                    ei.Put("STATUS", "C");
                    mixlist.Add(ei);
                    try
                    {
                        OperationUtils.ExecuteUpdate(mixlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage += ex.ToString() + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }
            
            return returnMessage;
        }

        public static bool updateShipmentStatus(string uid, string Status)
        {
            string sql = "SELECT STATUS FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            string oStatus = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (oStatus == "C")
            {
                sql = "UPDATE SMSM SET STATUS=" + SQLUtils.QuotedStr(Status) + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            }
            else if (oStatus == "I")
            {
                if (Status == "P" || Status == "O")
                {
                    sql = "UPDATE SMSM SET STATUS=" + SQLUtils.QuotedStr(Status) + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                }
                else
                {
                    sql = "UPDATE SMSM SET STATUS='I' WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                }
            }
            else if (oStatus == "P")
            {
                if (Status == "O" || Status == "G" || Status == "H")
                {
                    sql = "UPDATE SMSM SET STATUS=" + SQLUtils.QuotedStr(Status) + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                }
                else
                {
                    sql = "UPDATE SMSM SET STATUS='P' WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                }
            }
            else if (oStatus == "O")
            {
                if (Status == "G" || Status == "H")
                {
                    sql = "UPDATE SMSM SET STATUS=" + SQLUtils.QuotedStr(Status) + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                }
                else
                {
                    sql = "UPDATE SMSM SET STATUS='O' WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                }
            }
            else
            {
                sql = "UPDATE SMSM SET STATUS=" + SQLUtils.QuotedStr(Status) + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            }

            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static EditInstruct AddMessage(MessageData md, string UserId, string receiveUser, string GroupId, string CompanyId, string Dep)
        {
            EditInstruct ei = new EditInstruct("GFMESSAGE", EditInstruct.INSERT_OPERATION);
            ei.Put("GROUP_ID", GroupId);
            //string msg_id = AutoNo.GetNo("MESSAGE_JOB_NO", new Hashtable(), GroupId, CompanyId, Station);
            string msg_id = GroupId + CompanyId + Dep + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            ei.Put("MSG_ID", msg_id);
            ei.Put("RCV_CD", receiveUser);
            ei.Put("STATUS", MessageData.HAS_RECEVIE);
            ei.Put("CREATE_BY", UserId);
            ei.Put("MSG_TYPE", md.Type);
            ei.Put("JOB_NO", md.JobNo);
            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            ei.PutClob("CONTENT", md.ToXml());
            //OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ei;
        }

        public static bool syncBatNo(string UId)
        {
            string sql = "";
            sql = "SELECT BAT_NO FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string BatNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (BatNo == "")
            {
                return true;
            }

            MixedList mixlist = new MixedList();
            sql = @"UPDATE SMIRV
                        SET
                        SMIRV.STATUS=S2.STATUS,
                        SMIRV.BAT_NO=S2.BAT_NO,
                        SMIRV.WS_CD = S2.WS_CD,
                        SMIRV.GATE_NO = S2.GATE_NO,
                        SMIRV.RESERVE_DATE = S2.RESERVE_DATE,
                        SMIRV.RESERVE_FROM = S2.RESERVE_FROM,
                        SMIRV.RESERVE_HOUR = S2.RESERVE_HOUR,
                        SMIRV.TRUCK_CNTRNO = S2.TRUCK_CNTRNO,
                        SMIRV.TRUCK_SEALNO = S2.TRUCK_SEALNO,
                        SMIRV.TRUCK_NO = S2.TRUCK_NO,
                        SMIRV.DRIVER_ID = S2.DRIVER_ID,
                        SMIRV.TEL = S2.TEL,
                        SMIRV.DRIVER = S2.DRIVER,
                        SMIRV.TEMP_WSCD = S2.TEMP_WSCD,
                        SMIRV.TEMP_GATENO=S2.TEMP_GATENO,
                        SMIRV.TEMP_RDATE = S2.TEMP_RDATE,
                        SMIRV.TEMP_RFROM = S2.TEMP_RFROM,
                        SMIRV.TEMP_RH = S2.TEMP_RH,
                        SMIRV.LTRUCK_NO = S2.LTRUCK_NO,
                        SMIRV.LDRIVER = S2.LDRIVER,
                        SMIRV.LTEL = S2.LTEL,
                        SMIRV.LDRIVER_ID = S2.LDRIVER_ID,
                        SMIRV.ORDER_DATE = S2.ORDER_DATE,
                        SMIRV.ORDER_BY = S2.ORDER_BY,
                        SMIRV.IN_BY = S2.IN_BY,
                        SMIRV.IN_DATE = S2.IN_DATE,
                        SMIRV.OUT_DATE = S2.OUT_DATE,
                        SMIRV.OUT_BY = S2.OUT_BY,
                        SMIRV.SEAL_DATE = S2.SEAL_DATE 
                    FROM SMIRV, SMIRV S2
                    WHERE SMIRV.BAT_NO=S2.BAT_NO AND S2.U_ID='{0}' AND S2.BAT_NO IS NOT NULL";
            sql = string.Format(sql, UId);
            mixlist.Add(sql);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixlist, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static string updateJobNo(string DnNo, string IpartNo, string JobNo, string ProductLine)
        {
            string returnMessage = "success";
            string sql = "UPDATE SMDNP SET JOB_NO={0}, PRODUCT_LINE={1} WHERE DN_NO={2} AND IPART_NO={3}";
            sql = string.Format(sql, SQLUtils.QuotedStr(JobNo), SQLUtils.QuotedStr(ProductLine), SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(IpartNo));
            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }
            return returnMessage;
        }

        #region 進口一對一叫車
        public static string InboundOrderTrucker(string OrdNo, string GroupId, string CompanyId, string Dep, string Ext, string UserId, string UseDatetime = "", string ArrivalDate = "", List<string> idList = null)
        {
            string returnMessage = "success";
            string sql = @"SELECT SMORD.GROUP_ID, SMORD.CMP, SMORD.DN_NO,SMORD.COMBINE_INFO, SMORD.SHIPMENT_ID, SMORD.CNT_NUMBER, TRAN_TYPE1,IB_WINDOW, 
                    CNT20, CNT40, CNT40HQ, CNT_TYPE, GW, GWU, CBM, CREATE_BY, BL_WIN, TRUCK_CD, POL1,POL_NM1,POD1,POD_NM1, MASTER_NO,
                    TRUCKER1, TRUCKER_NM1,CNTR_NO,DEP_ADDR,IS_DIRECTLYNB  FROM SMORD
                             WHERE  SMORD.ORD_NO={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(OrdNo));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                DataRow item = dt.Rows[0];
                MixedList mixList = new MixedList();
                string ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                decimal Gw = Prolink.Math.GetValueAsDecimal(item["GW"]);
                decimal Cbm = Prolink.Math.GetValueAsDecimal(item["CBM"]);
                string CntType = Prolink.Math.GetValueAsString(item["CNT_TYPE"]);
                string Gwu = Prolink.Math.GetValueAsString(item["GWU"]);
                decimal gu = Convert.ToDecimal(Prolink.Math.unitConvert(Gwu, "KG"));
                Gw = Gw * gu;

                string DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                string CombineInfo = Prolink.Math.GetValueAsString(item["COMBINE_INFO"]);
                if (string.IsNullOrEmpty(CombineInfo))
                {
                    DnNo = CombineInfo;
                }
                string TranType = Prolink.Math.GetValueAsString(item["TRAN_TYPE1"]);
                string SmCreateBy = Prolink.Math.GetValueAsString(item["IB_WINDOW"]); //訂艙人員
                string TruckCd = Prolink.Math.GetValueAsString(item["TRUCKER1"]);
                string Pol1 = Prolink.Math.GetValueAsString(item["POL1"]);
                string PolNm1 = Prolink.Math.GetValueAsString(item["POL_NM1"]);
                string Pod1 = Prolink.Math.GetValueAsString(item["POD1"]);
                string PodNm1 = Prolink.Math.GetValueAsString(item["POD_NM1"]);
                string isdirectlynb = Prolink.Math.GetValueAsString(item["IS_DIRECTLYNB"]);
                string Dim = "";
                if (DnNo != "")
                {
                    sql = "SELECT TOP 1 (convert(varchar(10),L) + 'X' + convert(varchar(10),W) + 'X' + convert(varchar(10),H) + 'X' + convert(varchar(10),VW) + 'CTN') AS DIM FROM SMCUFT WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                    Dim = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }

                if (TruckCd == "" && "Y" != isdirectlynb && "T" != isdirectlynb)
                {
                    returnMessage = @Resources.Locale.L_DnHandel_Business_56;
                }
                else
                {
                    EditInstruct ei;
                    ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                    string UId = System.Guid.NewGuid().ToString();
                    string ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
                    ei.Put("U_ID", UId);
                    ei.Put("RESERVE_NO", ReserveNo);
                    ei.Put("ORD_NO", OrdNo);
                    ei.Put("ORD_INFO", OrdNo);
                    if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                    {
                        ei.Put("STATUS", 'R');      //直接由Directly Notify Broker 执行的，叫车的时候直接转愉悦
                    }
                    else
                    {
                        ei.Put("STATUS", 'D');
                    }
                    ei.Put("GROUP_ID", GroupId);
                    ei.Put("CMP", CompanyId);
                    ei.Put("DN_NO", DnNo);
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
                    ei.Put("TRUCKER", TruckCd);
                    ei.Put("SHIPMENT_ID", ShipmentId);
                    ei.Put("SHIPMENT_INFO", ShipmentId);
                    SetShipperLspCarrierInfo(ShipmentId, ei);

                    ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(UseDatetime));
                    ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(UseDatetime));
                    if (!string.IsNullOrEmpty(ArrivalDate))
                    {
                        DateTime arrivalDate = Business.DateTimeUtils.ParseToDateTime(ArrivalDate);
                        ei.PutDate("RESERVE_DATE", arrivalDate);
                        ei.Put("RESERVE_FROM", arrivalDate.Hour);
                        ei.PutDate("ARRIVAL_DATE", arrivalDate);
                    }
                    ei.Put("TRAN_TYPE", TranType);
                    ei.Put("CNT_TYPE", CntType);
                    ei.Put("GW", Gw);
                    ei.Put("GWU", "KGS");
                    ei.Put("CBM", Cbm);
                    ei.Put("DIM", Dim);
                    ei.Put("SMCREATE_BY", SmCreateBy);
                    ei.Put("PICK_AREA", Pol1);
                    ei.Put("PICK_AREA_NM", PolNm1);
                    ei.Put("DLV_AREA", Pod1);
                    ei.Put("DLV_AREA_NM", PodNm1);
                    ei.Put("LOT_NO", ShipmentId);
                    ei.Put("BAT_NO", ShipmentId);
                    ei.Put("RV_TYPE", "I");
                    ei.Put("CALL_TYPE", "S");
                    ei.Put("CNTR_NO", dt.Rows[0]["CNTR_NO"].ToString());
                    ei.Put("DEP_ADDR", dt.Rows[0]["DEP_ADDR"].ToString());
                    TrackingEDI.InboundBusiness.SMSMIHelper.setReserveValue(ShipmentId, ei);
                    mixList.Add(ei);

                    sql = "UPDATE SMRCNTR SET PICKUP_DATE={0}, ARRIVAL_DATE={1}, RESERVE_NO={2}, LOT_NO={3} WHERE ORD_NO={4}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(UseDatetime), SQLUtils.QuotedStr(ArrivalDate), SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(OrdNo));
                    mixList.Add(sql);
                    try
                    {
                        OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (idList != null && !idList.Contains(UId))
                            idList.Add(UId);
                    }
                    catch (Exception ex)
                    {
                        returnMessage = @Resources.Locale.L_ReserveManage_Business_93;
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_150;
            }
            return returnMessage;
        }
        #endregion

        #region 進口多對一叫車
        public static string MutiSmOrderTrucker(DataTable dt, string UserId, string CombineDate = "", string WsCd = "", string LotNo = "", string DlvArea = "", int CntNumber = 0, string DlvAddr = "", string AddrCode = "",List<string> idList=null)
        {
            string returnMessage = "success";
            
            bool success = false;
            MixedList mixList = new MixedList();
            string sumDn = "";
            string ids = string.Empty;
            List<string> ShipperList = new List<string>();
            List<string> LspList = new List<string>();
            List<string> CarrierList = new List<string>();

            if(dt.Rows.Count > 0)
            {
                string GroupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
                string Cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                string Stn = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
                string Dep = Prolink.Math.GetValueAsString(dt.Rows[0]["DEP"]);
                string TruckCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TRUCK_CD"]);
                string TruckNm = CommonHelp.getOneValueAsStringFromSql("SELECT PARTY_NO FROM SMPTY WHERE STATUS='U' AND PARTY_NO=" + SQLUtils.QuotedStr(TruckCd));
                string ShipmentId = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                string Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
                string CarrierNm = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER_NM"]);
                string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
                string SmCreateBy = Prolink.Math.GetValueAsString(dt.Rows[0]["IB_WINDOW"]);
                string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
                string isdirectlynb = Prolink.Math.GetValueAsString(dt.Rows[0]["IS_DIRECTLYNB"]);
                string WsNm = string.Empty;
                if (WsCd.Length > 0)
                {
                    string wsnmsql = string.Format("SELECT WS_NM FROM SMWH WHERE CMP={0} AND WS_CD IN {1}", SQLUtils.QuotedStr(Cmp),
                      SQLUtils.Quoted(WsCd.Split(',')));
                    DataTable wsdt = OperationUtils.GetDataTable(wsnmsql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    for (int i = 0; i < wsdt.Rows.Count; i++)
                    {
                        WsNm += wsdt.Rows[i]["WS_NM"].ToString() + ",";
                    }
                }
                string MasterNo = string.Empty;

                foreach (DataRow mdr in dt.Rows)
                {
                    MasterNo += Prolink.Math.GetValueAsString(mdr["MASTER_NO"]) + ",";
                    string shno = mdr["SH_NO"].ToString();
                    shno += mdr["SH_NM"].ToString();
                    if (!ShipperList.Contains(shno))
                    {
                        ShipperList.Add(shno);
                    }
                    string lspno = mdr["LSP_NO"].ToString();
                    lspno += mdr["LSP_NM"].ToString();
                    if (!LspList.Contains(lspno))
                    {
                        LspList.Add(lspno);
                    }
                    string carrier = mdr["CARRIER"].ToString();
                    if (!CarrierList.Contains(carrier))
                    {
                        CarrierList.Add(carrier);
                    }
                }
                MasterNo = MasterNo.Remove(MasterNo.Length - 1);

                if (TruckCd == "" && "Y" != isdirectlynb &&"T"!=isdirectlynb)
                {
                    returnMessage = @Resources.Locale.L_DnHandel_Business_56;
                    return returnMessage;

                }

                decimal sumGw = 0, sumCbm = 0;
                
                foreach (DataRow dr in dt.Rows)
                {
                    decimal Gw = Prolink.Math.GetValueAsDecimal(dr["GW"]);
                    decimal Cbm = Prolink.Math.GetValueAsDecimal(dr["CBM"]);
                    string CombineInfo = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]);
                    string Gwu = Prolink.Math.GetValueAsString(dr["GWU"]);
                    decimal gu = Convert.ToDecimal(Prolink.Math.unitConvert(Gwu, "KG"));
                    ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);

                    sumGw = sumGw + (Gw*gu);
                    sumCbm = sumCbm + Cbm;
                    sumDn = CombineInfo + "," + sumDn;
                    ids = ShipmentId + "," + ids;
                }

                sumDn = sumDn.Remove(sumDn.Length - 1);
                ids = ids.Remove(ids.Length - 1);
                EditInstruct ei;
                for (int i = 0; i < CntNumber; i++)
                {
                    ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
                    string UId = System.Guid.NewGuid().ToString();
                    string ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, Cmp);
                    ei.Put("U_ID", UId);
                    ei.Put("RESERVE_NO", ReserveNo);
                    if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                    {
                        ei.Put("STATUS", 'R');
                    }
                    else
                    {
                        ei.Put("STATUS", 'D');
                    }
                    ei.Put("GROUP_ID", GroupId);
                    ei.Put("CMP", Cmp);
                    ei.Put("DN_NO", sumDn);
                    ei.Put("DEP", Dep);
                    ei.Put("CREATE_BY", UserId);
                    DateTime odt = DateTime.Now;                    
                    string CompanyId = Cmp;
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    ei.PutDate("CREATE_DATE", odt.ToString("yyyyMMddHHmmss"));
                    ei.PutDate("CREATE_DATE_L", ndt.ToString("yyyyMMddHHmmss"));
                    ei.Put("CREATE_CMP", Cmp);
                    ei.Put("CREATE_DEP", Dep);
                    ei.PutDate("CALL_DATE", odt.ToString("yyyyMMddHHmmss"));
                    ei.PutDate("CALL_DATE_L", ndt.ToString("yyyyMMddHHmmss"));
                    ei.Put("TRUCKER", TruckCd);
                    ei.Put("TRUCKER_NM", TruckNm);
                    ei.Put("WS_CD", WsCd);
                    ei.Put("WS_NM", WsNm);
                    ei.Put("SHIPMENT_ID", ShipmentId);
                    ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                    ei.Put("CARRIER", Carrier);
                    ei.Put("CARRIER_NM", CarrierNm);
                    ei.PutDate("RESERVE_DATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                    ei.Put("TEMP_WSCD", WsCd);
                    ei.PutDate("TEMP_RDATE", Business.DateTimeUtils.ParseToDateTime(CombineDate));
                    ei.Put("TRAN_TYPE", TranType);
                    ei.Put("CNT_NUMBER", 1);
                    ei.Put("GW", sumGw);
                    ei.Put("GWU", "KGS");
                    ei.Put("CBM", sumCbm);
                    ei.Put("SMCREATE_BY", SmCreateBy);
                    ei.Put("RV_TYPE", "I");
                    ei.Put("LOT_NO", LotNo);
                    ei.Put("BAT_NO", LotNo);
                    ei.Put("PICK_AREA", PodCd);
                    ei.Put("SHIPMENT_INFO", ids);
                    ei.Put("SHIPPER", String.Join(",", ShipperList.ToArray()));
                    ei.Put("FOWARDER", String.Join(",", LspList.ToArray()));
                    ei.Put("CARRIER", String.Join(",", CarrierList.ToArray()));
                    ei.Put("PICK_AREA_NM", CommonHelp.getOneValueAsStringFromSql("SELECT PORT_NM FROM BSCITY WHERE PORT_CD=" + SQLUtils.QuotedStr(PodCd)));
                    ei.Put("DLV_AREA", DlvArea);
                    ei.Put("DLV_AREA_NM", CommonHelp.getOneValueAsStringFromSql("SELECT PORT_NM FROM BSTPORT WHERE PORT_CD=" + SQLUtils.QuotedStr(DlvArea)));
                    ei.Put("CNTR_NO", MasterNo);
                    ei.Put("OUTER_FLAG", CommonHelp.getOneValueAsStringFromSql("SELECT OUTER_FLAG FROM BSADDR WHERE ADDR_CODE=" + SQLUtils.QuotedStr(AddrCode)));
                    ei.Put("DLV_ADDR", DlvAddr);
                    ei.Put("ADDR_CODE", AddrCode);
                    TrackingEDI.InboundBusiness.SMSMIHelper.setReserveValue(ShipmentId, ei);
                    mixList.Add(ei);
                    if (idList != null && !idList.Contains(UId))
                        idList.Add(UId);
                }

                try
                {
                    OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    success = true;
                }
                catch (Exception ex)
                {
                    returnMessage = @Resources.Locale.L_ReserveManage_Business_93;
                    success = false;
                }

                //if (success == true)
                //{

                //    DataTable mailGroupDt = MailTemplate.GetMailGroup(TruckCd, GroupId, "IC");
                //    if (mailGroupDt.Rows.Count > 0)
                //    {
                //        foreach (DataRow item1 in mailGroupDt.Rows)
                //        {
                //            string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                //            if (mailStr != "")
                //            {
                //                EvenFactory.AddEven(LotNo + "#" + GroupId + "#" + Cmp + "#" + Guid.NewGuid().ToString(), LotNo, "IRVTK", null, 1, 0, mailStr, @Resources.Locale.L_ReserveManage_Business_94 + LotNo, "");
                //            }
                //        }
                //    }

                //}
            }


            return returnMessage;
        }
        #endregion

        public static void SetShipperLspCarrierInfo(string ShipmentId,EditInstruct ei)
        {
            TrackingEDI.InboundBusiness.SMSMIHelper.SetShipperLspCarrierInfo(ShipmentId, ei);
        }

        #region 進口FCL叫車
        public static ResultInfo InboundFclOrderTrucker(DataRow odr, DataRow cntrdr, string PickupDate, List<string> EtaMsl, string ArrivalDate, string WsCd, string UserId, string Ext, string QuotNo = "", List<string> idList = null, Dictionary<string, string> parm = null)
        {
            ResultInfo ri = new ResultInfo();
            List<string> newReserveNo = new List<string>();
            TrackingEDI.InboundBusiness.IBResultInfo ibresultinfo =
            TrackingEDI.InboundBusiness.SMSMIHelper.InboundFclOrderTrucker(odr, cntrdr, PickupDate, EtaMsl, ArrivalDate, WsCd, UserId, Ext, QuotNo, idList, newReserveNo, parm);
            if (ibresultinfo.IsSucceed)
            {
                try
                {
                    foreach (string resno in newReserveNo)
                    {
                        string shipment = odr["SHIPMENT_ID"].ToString();
                        CalculateFee cs = new CalculateFee(shipment);
                        Bill.WriteLogTagStart("fcl叫车自动计价", shipment);
                        List<string> emptyMessage = new List<string>();
                        cs.FindTrailerQuote(resno, shipment, emptyMessage);
                        InboundTransfer.UpdateBillInfoToSMORD(shipment, "", null);
                        Bill.WriteLogTagStart("结束计算", shipment);
                    }
                }
                catch (Exception ex)
                {
                    ibresultinfo.Description += "Failure To Calculate Truck Costs！ ";
                }
            }
            ri.IsSucceed = ibresultinfo.IsSucceed;
            ri.ResultCode = ibresultinfo.ResultCode;
            ri.Description = ibresultinfo.Description;
            return ri;
        }

        #endregion

        #region 進口DN叫車
        public static string InboundDnOrderTrucker(System.Collections.ArrayList objList, string CarType, string TrsMode, string PickupDate, string DepAddr, List<string> EtaMsl, TrackingEDI.InboundBusiness.IBUserInfo userinfo, Dictionary<string,bool> shipmentDic, string QuotNo = "", List<string> idList=null)
        {
            string UserId = userinfo.UserId;
            string Ext = userinfo.Ext;
            string GroupId = userinfo.GroupId;
            string CompanyId = userinfo.CompanyId;
            string Dep = userinfo.Dep;
            string LotNo = "S" + Business.ReserveManage.getAutoNo("SHIB_NO", GroupId, CompanyId);
            string returnMessage = "";
            string sql = "";

            string Trucker = string.Empty;
            string TruckerNm = string.Empty;
            string TranType = string.Empty;
            string WsCd = string.Empty;
            string WsNm = string.Empty;
            string PickArea = string.Empty;
            string PickAreaNm = string.Empty;
            string ShipmentId = string.Empty;
            string location = string.Empty;
            string DnNo = string.Empty;
            string InvNo = string.Empty;
            string DivDescp = string.Empty;
            string PoNo = string.Empty;
            string podcd = string.Empty;
            string Wo = string.Empty;
            string SmCreateBy = string.Empty;
            List<string> DnList = new List<string>();
            List<string> DivList = new List<string>();
            List<string> SmList = new List<string>();
            List<string> OrdList = new List<string>();
            List<string> DecList = new List<string>();
            List<string> MasterList = new List<string>();
            List<string> WSList = new List<string>();
            List<string> WNList = new List<string>();
            List<string> ShipperList = new List<string>();
            List<string> LspList = new List<string>();
            List<string> CarrierList = new List<string>();

            int n = 0;
            decimal SumGw = 0, SumCbm = 0;
            MixedList mixList = new MixedList();
            string isdirectlynb = string.Empty;
            string remark = string.Empty;
            string addrcode = string.Empty;string dlvArea = string.Empty;string dlvAreaNm = string.Empty;

            foreach (Dictionary<string, object> json in objList)
            {
                string OrdNo = json["OrdNo"].ToString();
                remark += json["InvNo"].ToString();
                string dUId = json["UId"].ToString();
                DnNo = json["DnNo"].ToString();
                ShipmentId = json["ShipmentId"].ToString();
                addrcode = json["AddrCode"].ToString();
                dlvArea = json["DlvArea"].ToString();
                dlvAreaNm = json["DlvAreaNm"].ToString();

                sql = string.Format("SELECT RESERVE_NO FROM SMRDN WHERE U_ID={0} AND ORD_NO={1}", SQLUtils.QuotedStr(dUId), SQLUtils.QuotedStr(OrdNo));
                string reserveNo= CommonHelp.getOneValueAsStringFromSql(sql);
                if (!string.IsNullOrEmpty(reserveNo))
                {
                    returnMessage += "Shipment ID: " + ShipmentId + ";DN NO" + DnNo + @Resources.Locale.L_GateManageController_Controllers_137 + "\n";
                    return returnMessage;
                }
                remark += ",";
                if (n == 0)
                {
                    WsCd = json["WsCd"].ToString();
                    WsNm = json["WsNm"].ToString();
                    if (!WSList.Contains(WsCd))
                    {
                        WSList.Add(WsCd);
                        WNList.Add(WsNm);
                    }
                    sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                    DataTable dt = CommonHelp.getDataTableFromSql(sql);

                    if (dt.Rows.Count > 0)
                    {
                        Trucker = dt.Rows[0]["TRUCKER1"].ToString();
                        TruckerNm = dt.Rows[0]["TRUCKER_NM1"].ToString();
                        TranType = dt.Rows[0]["TRAN_TYPE1"].ToString();
                        PickArea = dt.Rows[0]["POL1"].ToString();
                        PickAreaNm = dt.Rows[0]["POL_NM1"].ToString();
                        isdirectlynb = dt.Rows[0]["IS_DIRECTLYNB"].ToString();
                        SmCreateBy = Prolink.Math.GetValueAsString(dt.Rows[0]["IB_WINDOW"]);
                    }
                }
                InvNo = json["InvNo"].ToString();
                

                bool hIBCR = ReserveManage.checkIBCR(shipmentDic, ShipmentId);

                if (!hIBCR)
                {
                    returnMessage += "Shipment ID: " + ShipmentId + @Resources.Locale.L_GateManageController_Controllers_146 + "\n";

                    return returnMessage;
                }

                sql = "SELECT DIVISION_DESCP FROM SMIDN WHERE SHIPMENT_ID={0} AND DN_NO={1}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(DnNo));
                DivDescp = CommonHelp.getOneValueAsStringFromSql(sql);
                string DecNo = json["DecNo"].ToString();

                if (!DnList.Contains(InvNo))
                {
                    DnList.Add(InvNo);
                }

                if (!DivList.Contains(DivDescp))
                {
                    DivList.Add(DivDescp);
                }

                if (!SmList.Contains(ShipmentId))
                {
                    SmList.Add(ShipmentId);

                    sql = "SELECT MASTER_NO,SH_NO+SH_NM AS SH_NO,LSP_NO+LSP_NM AS LSP_NO,CARRIER,POD_CD,CMP FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable smsmidt = CommonHelp.getDataTableFromSql(sql);
                    if (smsmidt.Rows.Count > 0)
                    {
                        podcd = smsmidt.Rows[0]["POD_CD"].ToString();
                        string MasterNo = smsmidt.Rows[0]["MASTER_NO"].ToString();
                        if (!MasterList.Contains(MasterNo))
                        {
                            MasterList.Add(MasterNo);
                        }
                        string shno = smsmidt.Rows[0]["SH_NO"].ToString();
                        if (!ShipperList.Contains(shno))
                        {
                            ShipperList.Add(shno);
                        }
                        string lspno = smsmidt.Rows[0]["LSP_NO"].ToString();
                        if (!LspList.Contains(lspno))
                        {
                            LspList.Add(lspno);
                        }
                        string carrier = smsmidt.Rows[0]["CARRIER"].ToString();
                        if (!CarrierList.Contains(carrier))
                        {
                            CarrierList.Add(carrier);
                        }

                        location = smsmidt.Rows[0]["CMP"].ToString();
                    }
                }

                if (!OrdList.Contains(OrdNo))
                {
                    OrdList.Add(OrdNo);
                }
                if (!DecList.Contains(DecNo))
                {
                    DecList.Add(DecNo);
                }
                decimal Gw = Convert.ToDecimal(json["Gw"]);
                string Gwu = json["Gwu"].ToString();
                decimal gu = Convert.ToDecimal(Prolink.Math.unitConvert(Gwu, "KG"));
                Gw = Gw * gu;
                SumGw += Gw;

                decimal Cbm = Convert.ToDecimal(json["Cbm"]);
                SumCbm += Cbm;

                //decimal Vw = Convert.ToDecimal(json["Vw"]);
                //SumCbm += Cbm;


                bool isupdate = false;
                EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                smordei.PutKey("ORD_NO", OrdNo);


                EditInstruct smidnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                smidnei.PutKey("SHIPMENT_ID", ShipmentId);
                smidnei.PutKey("DN_NO", DnNo);
                if (!string.IsNullOrEmpty(PickupDate))
                {
                    smidnei.PutDate("PICKUP_CDATE", Business.DateTimeUtils.ParseToDateTime(PickupDate));
                    smordei.PutDate("PICKUP_CDATE", Business.DateTimeUtils.ParseToDateTime(PickupDate));
                    isupdate = true;
                }
                if (EtaMsl != null)
                {
                    if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                    {
                        smidnei.Put("CALL_TRUCK_STATUS", 'R');      //直接由Directly Notify Broker 执行的，叫车的时候直接转愉悦
                        smordei.Put("CSTATUS", 'R');
                        isupdate = true;
                    }
                    else
                    {
                        smidnei.Put("CALL_TRUCK_STATUS", 'D');
                        //smordei1.Put("CSTATUS", 'D');
                    }
                    if (EtaMsl.Count > 0 && !string.IsNullOrEmpty(EtaMsl[0]))
                    {
                        smordei.PutDate("ETA_MSL", Business.DateTimeUtils.ParseToDateTime(EtaMsl[0]));
                        smidnei.PutDate("ETA_MSL", Business.DateTimeUtils.ParseToDateTime(EtaMsl[0]));
                        isupdate = true;
                    }
                    if (EtaMsl.Count > 1 && !string.IsNullOrEmpty(EtaMsl[1]))
                    {
                        smordei.Put("ETA_MSL_TIME", Convert.ToString(EtaMsl[1]));
                        smidnei.Put("ETA_MSL_TIME", Convert.ToString(EtaMsl[1]));
                        isupdate = true;
                    }

                    if (!isupdate)
                        mixList.Add(smidnei);

                }
                if (isupdate)
                {
                    mixList.Add(smordei);
                    mixList.Add(smidnei);
                }

                //mixList.Add(smordei1);

                //將PO NO從SMIDN寫入SMRDN
                sql = "SELECT PO_NO,WO FROM SMIDN WHERE SHIPMENT_ID={0} AND DN_NO={1} ORDER BY SCMREQUEST_DATE ASC";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(json["DnNo"].ToString()));
                DataTable dt1 = CommonHelp.getDataTableFromSql(sql);
                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt1.Rows)
                    {
                        string PN = Prolink.Math.GetValueAsString(dr["PO_NO"]);
                        PoNo += PN + ",";
                        string wo = Prolink.Math.GetValueAsString(dr["WO"]);
                        Wo += wo + ",";
                    }
                }
            }
             
            if (!string.IsNullOrEmpty(PoNo))
            {

                PoNo = PoNo.Remove(PoNo.Length - 1);
            }
            if (!string.IsNullOrEmpty(Wo))
            {

                Wo = Wo.Remove(Wo.Length - 1);
            }
            EditInstruct ei;
            ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
            string UId = System.Guid.NewGuid().ToString();
            string ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
            ei.Put("U_ID", UId);
            ei.Put("RESERVE_NO", ReserveNo);

            if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
            {
                ei.Put("STATUS", 'R');      //直接由Directly Notify Broker 执行的，叫车的时候直接转愉悦
            }
            else
            {
                ei.Put("STATUS", 'D');
            }
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", location);
            ei.Put("DN_NO", String.Join(",", DnList.ToArray()));
            ei.Put("INVOICE_INFO", String.Join(",", DnList.ToArray()));
            ei.Put("PRODUCT_TYPE", String.Join(",", DivList.ToArray()));
            ei.Put("SHIPMENT_INFO", String.Join(",", SmList.ToArray()));
            ei.Put("SHIPPER", String.Join(",", ShipperList.ToArray()));
            ei.Put("FOWARDER", String.Join(",", LspList.ToArray()));
            ei.Put("CARRIER", String.Join(",", CarrierList.ToArray()));
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
            if (!string.IsNullOrEmpty(PickupDate))
            {
                ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(PickupDate));
            }
            ei.Put("DEP_ADDR", DepAddr);
            ei.Put("TRUCKER", Trucker);
            ei.Put("TRUCKER_NM", TruckerNm);
            ei.Put("TRAN_TYPE", TranType);
            ei.Put("GW", SumGw);
            ei.Put("GWU", "KGS");
            ei.Put("CBM", SumCbm);
            ei.Put("LOT_NO", LotNo);
            ei.Put("BAT_NO", ReserveNo);
            ei.Put("CAR_TYPE", CarType);
            ei.Put("TRS_MODE", TrsMode);
            ei.Put("RV_TYPE", "I");
            ei.Put("CALL_TYPE", "D");
            ei.Put("ORD_INFO", String.Join(",", OrdList.ToArray()));
            ei.Put("WS_CD",string.Join(",", WSList)); 
            ei.Put("WS_NM", string.Join(",", WNList));

            ei.Put("ADDR_CODE", addrcode);
            ei.Put("DLV_AREA", dlvArea);
            ei.Put("DLV_AREA_NM", dlvAreaNm);
            ei.Put("PICK_AREA", PickArea);
            ei.Put("PICK_AREA_NM", PickAreaNm);
            ei.Put("QUOT_NO", QuotNo);
            ei.Put("DEC_INFO", String.Join(",", DecList.ToArray()));
            ei.Put("CNTR_NO", String.Join(",", MasterList.ToArray()));
            ei.Put("PRIORITY", GetPriority(SmList.ToArray()));
            ei.Put("SMCREATE_BY", SmCreateBy);
            string ArrivalDate = string.Empty;
            DateTime arrivaltime = DateTime.MinValue;
            foreach (Dictionary<string, object> json in objList)
            {
                ArrivalDate = json["ArrivalDate"].ToString();
                ArrivalDate = ArrivalDate.Trim();
                if (!string.IsNullOrEmpty(ArrivalDate))
                {
                    if (DateTime.Compare(arrivaltime, DateTime.MinValue) <= 0)
                    {
                        arrivaltime = Business.DateTimeUtils.ParseToDateTime(ArrivalDate);
                    }
                    else if (DateTime.Compare(Business.DateTimeUtils.ParseToDateTime(ArrivalDate), arrivaltime) <= 0)
                    {
                        arrivaltime = Business.DateTimeUtils.ParseToDateTime(ArrivalDate);
                    }
                }
                string dUId = json["UId"].ToString();
                string AddPoint = json["AddPoint"].ToString();
                string OrdNo = json["OrdNo"].ToString();

                if(!OrdList.Contains(OrdNo))
                {
                    OrdList.Add(OrdNo);
                }

                WsCd = json["WsCd"].ToString();
                sql = "UPDATE SMRDN SET PICKUP_DATE={0}, ARRIVAL_DATE={1}, WS_CD={2}, RESERVE_NO={3}, ADD_POINT={4}, LOT_NO={5} WHERE U_ID={6} AND ORD_NO={7}";
                sql = string.Format(sql, SQLUtils.QuotedStr(PickupDate), SQLUtils.QuotedStr(ArrivalDate), SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(AddPoint), SQLUtils.QuotedStr(LotNo), SQLUtils.QuotedStr(dUId), SQLUtils.QuotedStr(OrdNo));
                mixList.Add(sql);
                DnNo = json["DnNo"].ToString(); 
            }
            if (DateTime.Compare(arrivaltime,  DateTime.MinValue) > 0)
            {
                ei.PutDate("RESERVE_DATE", arrivaltime);
                ei.Put("RESERVE_FROM", arrivaltime.Hour);
            }
            ei.Put("PO_NO", PoNo);
            ei.Put("WO", Wo);
            List<string> asnList = new List<string>();
            List<string> partList = new List<string>();
            List<int> qtyList = new List<int>(); 
            string psql = string.Format("SELECT ASN_NO,PART_NO,IPART_NO,QTY FROM SMIDNP WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(SmList.ToArray()));
            DataTable inpDt = OperationUtils.GetDataTable(psql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in inpDt.Rows)
            {
                string asnNo = Prolink.Math.GetValueAsString(dr["ASN_NO"]);
                string partNo = Prolink.Math.GetValueAsString(dr["PART_NO"]);
                int qty = Prolink.Math.GetValueAsInt(dr["QTY"]);
                if (string.IsNullOrEmpty(partNo))
                {
                    partNo = Prolink.Math.GetValueAsString(dr["IPART_NO"]);
                }
                if (!string.IsNullOrEmpty(partNo))
                {
                    partList.Add(partNo);
                    qtyList.Add(qty);
                    asnList.Add(asnNo);
                }
            }
            ei.Put("ASNNO_INFO", getValueByCol(asnList.ToArray()));
            ei.Put("PARTNO_INFO", getValueByCol(partList.ToArray()));
            ei.Put("PART_QTY", getValueByCol(qtyList.ToArray()));
            string csCd = "", csNm = "", csName = "", bu = "";
            List<string> csCdList = new List<string>();
            List<string> csNmList = new List<string>();
            List<string> csNameList = new List<string>();
            List<string> buList = new List<string>();
            string ptSql = string.Format("SELECT Y.ABBR,Y.DEP,Y.PARTY_NAME,T.PARTY_NO,T.PARTY_TYPE FROM SMPTY Y LEFT JOIN SMSMIPT T ON T.PARTY_NO=Y.PARTY_NO WHERE T.SHIPMENT_ID IN {0} AND T.PARTY_TYPE IN ('CS','ZT')", SQLUtils.Quoted(SmList.ToArray()));
            DataTable ptyDt = OperationUtils.GetDataTable(ptSql, null, Prolink.Web.WebContext.GetInstance().GetConnection()); 
            if (ptyDt.Rows.Count > 0)
            {
                foreach (DataRow dr in ptyDt.Rows)
                {
                    string partyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    switch (partyType)
                    {
                        case "CS":
                            csCd = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                            csName = Prolink.Math.GetValueAsString(dr["ABBR"]);
                            csNm = Prolink.Math.GetValueAsString(dr["PARTY_NAME"]);
                            if (!csCdList.Contains(csCd) && !string.IsNullOrEmpty(csCd))
                            {
                                csCdList.Add(csCd);
                                csNameList.Add(csName);
                                csNmList.Add(csNm);
                            }
                            break;
                        case "ZT":
                            bu = Prolink.Math.GetValueAsString(dr["DEP"]);
                            if (!buList.Contains(bu) && !string.IsNullOrEmpty(bu))
                            {
                                buList.Add(bu);
                            }
                            break;
                    }
                }
            }
            csCd = string.Join(",", csCdList);
            if (csCd.Length > 350)
                csCd.Substring(0, 350);
            csName = string.Join(",", csNameList);
            if (csName.Length > 350)
                csName.Substring(0, 350);
            csNm = string.Join(",", csNmList);
            if (csNm.Length > 500)
                csNm.Substring(0, 500);
            bu = string.Join(",", buList);
            if (bu.Length > 100)
                bu.Substring(0, 100);
            ei.Put("CS_CD", csCd);
            ei.Put("CS_NAME", csName);
            ei.Put("CS_NM", csNm);
            ei.Put("BU", bu);
            TrackingEDI.InboundBusiness.SMSMIHelper.setReserveValue(ShipmentId, ei);
            mixList.Add(ei);

            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (idList != null && !idList.Contains(UId))
                    idList.Add(UId);
                TrackingEDI.InboundBusiness.SMSMIHelper.GetDivisonBySMR(ReserveNo, TranType);
                InboundHandel.UpdateSMICUFT(ReserveNo, true);
                foreach (string shipment in SmList)
                {
                    TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = ShipmentId, StsCd = "018", Cmp = CompanyId, Remark=remark, Sender = UserId, Location = podcd, LocationName = "", StsDescp = "Order Truck By DN" });
                }
                for (int i = 0; i < OrdList.Count; i++)
                {
                    int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(OrdList[i]));
                    int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdList[i]));

                    if (r == c)
                    {
                        //更新運輸單的Cstatus
                        sql = "UPDATE SMORD SET CSTATUS='D' WHERE ORD_NO={0}";
                        if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                        {
                            sql = "UPDATE SMORD SET CSTATUS='R' WHERE ORD_NO={0}";
                        }
                        sql = string.Format(sql, SQLUtils.QuotedStr(OrdList[i]));
                        CommonHelp.exeSql(sql);
                    }
                }


                for (int i = 0; i < SmList.Count; i++)
			    {
                    string updateArrivateDatetoSmord=SetArrivalDateToSMORD(SmList[i]);
                    CommonHelp.exeSql(updateArrivateDatetoSmord);
                    CalculateFee cs = new CalculateFee(SmList[i]);
                    Bill.WriteLogTagStart("dn叫车自动计价", SmList[i]);
                    List<string> emptyMessage = new List<string>();
                    cs.FindTrailerQuote(ReserveNo, SmList[i], emptyMessage);
                    InboundTransfer.UpdateBillInfoToSMORD(SmList[i], "", null);
                    Bill.WriteLogTagStart("结束计算", SmList[i]);
                }

                
            }
            catch (Exception ex)
            {
                returnMessage += ex.ToString();
            }
            return returnMessage;
        }

        public static string RebuildDnOrderTrucker(DataRow smrdnRow, string CarType, string TrsMode, string PickupDate, string DepAddr, List<string> EtaMsl, TrackingEDI.InboundBusiness.IBUserInfo userinfo, Dictionary<string, bool> shipmentDic, string QuotNo = "", List<string> idList = null)
        {
            string UserId = userinfo.UserId;
            string Ext = userinfo.Ext;
            string GroupId = userinfo.GroupId;
            string CompanyId = userinfo.CompanyId;
            string Dep = userinfo.Dep;
            string LotNo = "S" + Business.ReserveManage.getAutoNo("SHIB_NO", GroupId, CompanyId);
            string returnMessage = "";
            string sql = "";

            string Trucker = string.Empty;
            string TruckerNm = string.Empty;
            string TranType = string.Empty;
            string WsCd = string.Empty;
            string WsNm = string.Empty;
            string PickArea = string.Empty;
            string PickAreaNm = string.Empty;
            string ShipmentId = string.Empty;
            string location = string.Empty;
            string DnNo = string.Empty;
            string InvNo = string.Empty;
            string DivDescp = string.Empty;
            string PoNo = string.Empty;
            string podcd = string.Empty;
            string Wo = string.Empty; 
            string SmCreateBy = string.Empty;
            List<string> DnList = new List<string>();
            List<string> DivList = new List<string>();
            List<string> SmList = new List<string>();
            List<string> OrdList = new List<string>();
            List<string> DecList = new List<string>();
            List<string> MasterList = new List<string>();
            List<string> WSList = new List<string>();
            List<string> WNList = new List<string>();
            List<string> ShipperList = new List<string>();
            List<string> LspList = new List<string>();
            List<string> CarrierList = new List<string>();

            int n = 0;
            decimal SumGw = 0, SumCbm = 0;
            MixedList mixList = new MixedList();
            string isdirectlynb = string.Empty;
            string remark = string.Empty;
            string addrcode = string.Empty; string dlvArea = string.Empty; string dlvAreaNm = string.Empty;

 
                string OrdNo = smrdnRow["Ord_No"].ToString(); 
                string dUId = smrdnRow["U_Id"].ToString();
                DnNo = smrdnRow["Dn_No"].ToString();
                ShipmentId = smrdnRow["Shipment_Id"].ToString();
                addrcode = smrdnRow["Addr_Code"].ToString();
                dlvArea = smrdnRow["Dlv_Area"].ToString();
                dlvAreaNm = smrdnRow["Dlv_Area_Nm"].ToString();

                sql = string.Format("SELECT RESERVE_NO FROM SMRDN WHERE U_ID={0} AND ORD_NO={1}", SQLUtils.QuotedStr(dUId), SQLUtils.QuotedStr(OrdNo));
                string reserveNo = CommonHelp.getOneValueAsStringFromSql(sql);
                if (!string.IsNullOrEmpty(reserveNo))
                {
                    returnMessage += "Shipment ID: " + ShipmentId + ";DN NO" + DnNo + @Resources.Locale.L_GateManageController_Controllers_137 + "\n";
                    return returnMessage;
                }
                remark += ",";
                if (n == 0)
                {
                    WsCd = smrdnRow["Ws_Cd"].ToString();  
                    string wsnmsql = string.Format("SELECT WS_NM FROM SMWH WHERE CMP={0} AND WS_CD = {1}", SQLUtils.QuotedStr(smrdnRow["CMP"].ToString()),
                   SQLUtils.QuotedStr(WsCd));

                    WsNm = CommonHelp.getOneValueAsStringFromSql(wsnmsql);
                    if (!WSList.Contains(WsCd))
                    {
                        WSList.Add(WsCd);
                        WNList.Add(WsNm);
                    }
                    sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                    DataTable dt = CommonHelp.getDataTableFromSql(sql);

                    if (dt.Rows.Count > 0)
                    {
                        Trucker = dt.Rows[0]["TRUCKER1"].ToString();
                        TruckerNm = dt.Rows[0]["TRUCKER_NM1"].ToString();
                        TranType = dt.Rows[0]["TRAN_TYPE1"].ToString();
                        PickArea = dt.Rows[0]["POL1"].ToString();
                        PickAreaNm = dt.Rows[0]["POL_NM1"].ToString();
                        isdirectlynb = dt.Rows[0]["IS_DIRECTLYNB"].ToString();
                        SmCreateBy = Prolink.Math.GetValueAsString(dt.Rows[0]["IB_WINDOW"]);
                    }
                } 


                bool hIBCR = ReserveManage.checkIBCR(shipmentDic, ShipmentId);

                if (!hIBCR)
                {
                    returnMessage += "Shipment ID: " + ShipmentId + @Resources.Locale.L_GateManageController_Controllers_146 + "\n";

                    return returnMessage;
                }

                sql = "SELECT DIVISION_DESCP,INV_NO,GW,GWU,CBM,PO_NO,WO,DN_NO FROM SMIDN WHERE SHIPMENT_ID={0} AND DN_NO={1}  ORDER BY SCMREQUEST_DATE ASC";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(DnNo));
                DataTable smidnDt = CommonHelp.getDataTableFromSql(sql);
                foreach (DataRow dnRow in smidnDt.Rows)
                {
                    DivDescp = Prolink.Math.GetValueAsString(dnRow["DIVISION_DESCP"]);
                    InvNo = Prolink.Math.GetValueAsString(dnRow["INV_NO"]);
                    string PN = Prolink.Math.GetValueAsString(dnRow["PO_NO"]);
                    PoNo += PN + ",";
                    string wo = Prolink.Math.GetValueAsString(dnRow["WO"]);
                    Wo += wo + ","; 

                    if (!DnList.Contains(InvNo))
                    {
                        DnList.Add(InvNo);
                    }

                    if (!DivList.Contains(DivDescp))
                    {
                        DivList.Add(DivDescp);
                    }

                    decimal Gw = Convert.ToDecimal(dnRow["Gw"]);
                    string Gwu = dnRow["Gwu"].ToString();
                    decimal gu = Convert.ToDecimal(Prolink.Math.unitConvert(Gwu, "KG"));
                    Gw = Gw * gu;
                    SumGw += Gw; 
                    decimal Cbm = Convert.ToDecimal(dnRow["Cbm"]);
                    SumCbm += Cbm; 
                }
                if (!string.IsNullOrEmpty(PoNo))
                {
                    PoNo = PoNo.Remove(PoNo.Length - 1);
                }
                if (!string.IsNullOrEmpty(Wo))
                {
                    Wo = Wo.Remove(Wo.Length - 1);
                } 
                string DecNo = smrdnRow["Dec_No"].ToString(); 
                if (!SmList.Contains(ShipmentId))
                {
                    SmList.Add(ShipmentId);

                    sql = "SELECT MASTER_NO,SH_NO+SH_NM AS SH_NO,LSP_NO+LSP_NM AS LSP_NO,CARRIER,POD_CD,CMP FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable smsmidt = CommonHelp.getDataTableFromSql(sql);
                    if (smsmidt.Rows.Count > 0)
                    {
                        podcd = smsmidt.Rows[0]["POD_CD"].ToString();
                        string MasterNo = smsmidt.Rows[0]["MASTER_NO"].ToString();
                        if (!MasterList.Contains(MasterNo))
                        {
                            MasterList.Add(MasterNo);
                        }
                        string shno = smsmidt.Rows[0]["SH_NO"].ToString();
                        if (!ShipperList.Contains(shno))
                        {
                            ShipperList.Add(shno);
                        }
                        string lspno = smsmidt.Rows[0]["LSP_NO"].ToString();
                        if (!LspList.Contains(lspno))
                        {
                            LspList.Add(lspno);
                        }
                        string carrier = smsmidt.Rows[0]["CARRIER"].ToString();
                        if (!CarrierList.Contains(carrier))
                        {
                            CarrierList.Add(carrier);
                        }

                        location = smsmidt.Rows[0]["CMP"].ToString();
                    }
                }

                if (!OrdList.Contains(OrdNo))
                {
                    OrdList.Add(OrdNo);
                }
                if (!DecList.Contains(DecNo))
                {
                    DecList.Add(DecNo);
                }
                 
                bool isupdate = false;
                EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                smordei.PutKey("ORD_NO", OrdNo);
             
                EditInstruct smidnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                smidnei.PutKey("SHIPMENT_ID", ShipmentId);
                smidnei.PutKey("DN_NO", DnNo);
                if (!string.IsNullOrEmpty(PickupDate))
                {
                    smidnei.PutDate("PICKUP_CDATE", Business.DateTimeUtils.ParseToDateTime(PickupDate));
                    smordei.PutDate("PICKUP_CDATE", Business.DateTimeUtils.ParseToDateTime(PickupDate));
                    isupdate = true;
                }
                if (EtaMsl != null)
                {
                    if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                    {
                        smidnei.Put("CALL_TRUCK_STATUS", 'R');      //直接由Directly Notify Broker 执行的，叫车的时候直接转愉悦
                        smordei.Put("CSTATUS", 'R');
                        isupdate = true;
                    }
                    else
                    {
                        smidnei.Put("CALL_TRUCK_STATUS", 'D');
                        //smordei1.Put("CSTATUS", 'D');
                    }
                    if (EtaMsl.Count > 0 && !string.IsNullOrEmpty(EtaMsl[0]))
                    {
                        smordei.PutDate("ETA_MSL", Business.DateTimeUtils.ParseToDateTime(EtaMsl[0]));
                        smidnei.PutDate("ETA_MSL", Business.DateTimeUtils.ParseToDateTime(EtaMsl[0]));
                        isupdate = true;
                    }
                    if (EtaMsl.Count > 1 && !string.IsNullOrEmpty(EtaMsl[1]))
                    {
                        smordei.Put("ETA_MSL_TIME", Convert.ToString(EtaMsl[1]));
                        smidnei.Put("ETA_MSL_TIME", Convert.ToString(EtaMsl[1]));
                        isupdate = true;
                    }

                    if (!isupdate)
                        mixList.Add(smidnei);

                }
                if (isupdate)
                {
                    mixList.Add(smordei);
                    mixList.Add(smidnei);
                }
                   
            EditInstruct ei;
            ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
            string UId = System.Guid.NewGuid().ToString();
            string ReserveNo = Business.ReserveManage.getAutoNo("RV_NO", GroupId, CompanyId);
            ei.Put("U_ID", UId);
            ei.Put("RESERVE_NO", ReserveNo);

            if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
            {
                ei.Put("STATUS", 'R');      //直接由Directly Notify Broker 执行的，叫车的时候直接转愉悦
            }
            else
            {
                ei.Put("STATUS", 'D');
            }
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", location);
            ei.Put("DN_NO", String.Join(",", DnList.ToArray()));
            ei.Put("INVOICE_INFO", String.Join(",", DnList.ToArray()));
            ei.Put("PRODUCT_TYPE", String.Join(",", DivList.ToArray()));
            ei.Put("SHIPMENT_INFO", String.Join(",", SmList.ToArray()));
            ei.Put("SHIPPER", String.Join(",", ShipperList.ToArray()));
            ei.Put("FOWARDER", String.Join(",", LspList.ToArray()));
            ei.Put("CARRIER", String.Join(",", CarrierList.ToArray()));
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
            if (!string.IsNullOrEmpty(PickupDate))
            {
                ei.PutDate("USE_DATE", Business.DateTimeUtils.ParseToDateTime(PickupDate));
            }
            ei.Put("DEP_ADDR", DepAddr);
            ei.Put("TRUCKER", Trucker);
            ei.Put("TRUCKER_NM", TruckerNm);
            ei.Put("TRAN_TYPE", TranType);
            ei.Put("GW", SumGw);
            ei.Put("GWU", "KGS");
            ei.Put("CBM", SumCbm);
            ei.Put("LOT_NO", LotNo);
            ei.Put("BAT_NO", ReserveNo);
            ei.Put("CAR_TYPE", CarType);
            ei.Put("TRS_MODE", TrsMode);
            ei.Put("RV_TYPE", "I");
            ei.Put("CALL_TYPE", "D");
            ei.Put("ORD_INFO", String.Join(",", OrdList.ToArray()));
            ei.Put("WS_CD", string.Join(",", WSList));
            ei.Put("WS_NM", string.Join(",", WNList));

            ei.Put("ADDR_CODE", addrcode);
            ei.Put("DLV_AREA", dlvArea);
            ei.Put("DLV_AREA_NM", dlvAreaNm);
            ei.Put("PICK_AREA", PickArea);
            ei.Put("PICK_AREA_NM", PickAreaNm);
            ei.Put("QUOT_NO", QuotNo);
            ei.Put("DEC_INFO", String.Join(",", DecList.ToArray()));
            ei.Put("CNTR_NO", String.Join(",", MasterList.ToArray()));
            ei.Put("PRIORITY", GetPriority(SmList.ToArray()));
            ei.Put("SMCREATE_BY", SmCreateBy);
            string ArrivalDate = string.Empty;
            DateTime arrivaltime = DateTime.MinValue;
 
                //ArrivalDate = smrdnRow["Arrival_Date"].ToString();
                ArrivalDate = PickupDate.Trim();
                if (!string.IsNullOrEmpty(ArrivalDate))
                {
                    if (DateTime.Compare(arrivaltime, DateTime.MinValue) <= 0)
                    {
                        arrivaltime = Business.DateTimeUtils.ParseToDateTime(ArrivalDate);
                    }
                    else if (DateTime.Compare(Business.DateTimeUtils.ParseToDateTime(ArrivalDate), arrivaltime) <= 0)
                    {
                        arrivaltime = Business.DateTimeUtils.ParseToDateTime(ArrivalDate);
                    }
                }
                //string AddPoint = smrdnRow["Add_Point"].ToString();  
                WsCd = smrdnRow["Ws_Cd"].ToString();
                sql = "UPDATE SMRDN SET PICKUP_DATE={0}, ARRIVAL_DATE={1}, WS_CD={2}, RESERVE_NO={3}, LOT_NO={4} WHERE U_ID={5} AND ORD_NO={6}";
                sql = string.Format(sql, SQLUtils.QuotedStr(PickupDate), SQLUtils.QuotedStr(ArrivalDate), SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(LotNo), SQLUtils.QuotedStr(dUId), SQLUtils.QuotedStr(OrdNo));
                mixList.Add(sql);
                DnNo = smrdnRow["Dn_No"].ToString(); 
            if (DateTime.Compare(arrivaltime, DateTime.MinValue) > 0)
            {
                ei.PutDate("RESERVE_DATE", arrivaltime);
                ei.Put("RESERVE_FROM", arrivaltime.Hour);
            }
            ei.Put("PO_NO", PoNo);
            ei.Put("WO", Wo);
            List<string> asnList = new List<string>();
            List<string> partList = new List<string>();
            List<int> qtyList = new List<int>();
            string psql = string.Format("SELECT ASN_NO,PART_NO,IPART_NO,QTY FROM SMIDNP WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(SmList.ToArray()));
            DataTable inpDt = OperationUtils.GetDataTable(psql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in inpDt.Rows)
            {
                string asnNo = Prolink.Math.GetValueAsString(dr["ASN_NO"]);
                string partNo = Prolink.Math.GetValueAsString(dr["PART_NO"]);
                int qty = Prolink.Math.GetValueAsInt(dr["QTY"]);
                if (string.IsNullOrEmpty(partNo))
                {
                    partNo = Prolink.Math.GetValueAsString(dr["IPART_NO"]);
                }
                if (!string.IsNullOrEmpty(partNo))
                {
                    partList.Add(partNo);
                    qtyList.Add(qty);
                    asnList.Add(asnNo);
                }
            }
            ei.Put("ASNNO_INFO", getValueByCol(asnList.ToArray()));
            ei.Put("PARTNO_INFO", getValueByCol(partList.ToArray()));
            ei.Put("PART_QTY", getValueByCol(qtyList.ToArray()));
            string csCd = "", csNm = "", csName = "", bu = "";
            List<string> csCdList = new List<string>();
            List<string> csNmList = new List<string>();
            List<string> csNameList = new List<string>();
            List<string> buList = new List<string>();
            string ptSql = string.Format("SELECT Y.ABBR,Y.DEP,Y.PARTY_NAME,T.PARTY_NO,T.PARTY_TYPE FROM SMPTY Y LEFT JOIN SMSMIPT T ON T.PARTY_NO=Y.PARTY_NO WHERE T.SHIPMENT_ID IN {0} AND T.PARTY_TYPE IN ('CS','ZT')", SQLUtils.Quoted(SmList.ToArray()));
            DataTable ptyDt = OperationUtils.GetDataTable(ptSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (ptyDt.Rows.Count > 0)
            {
                foreach (DataRow dr in ptyDt.Rows)
                {
                    string partyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    switch (partyType)
                    {
                        case "CS":
                            csCd = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                            csName = Prolink.Math.GetValueAsString(dr["ABBR"]);
                            csNm = Prolink.Math.GetValueAsString(dr["PARTY_NAME"]);
                            if (!csCdList.Contains(csCd) && !string.IsNullOrEmpty(csCd))
                            {
                                csCdList.Add(csCd);
                                csNameList.Add(csName);
                                csNmList.Add(csNm);
                            }
                            break;
                        case "ZT":
                            bu = Prolink.Math.GetValueAsString(dr["DEP"]);
                            if (!buList.Contains(bu) && !string.IsNullOrEmpty(bu))
                            {
                                buList.Add(bu);
                            }
                            break;
                    }
                }
            }
            csCd = string.Join(",", csCdList);
            if (csCd.Length > 350)
                csCd.Substring(0, 350);
            csName = string.Join(",", csNameList);
            if (csName.Length > 350)
                csName.Substring(0, 350);
            csNm = string.Join(",", csNmList);
            if (csNm.Length > 500)
                csNm.Substring(0, 500);
            bu = string.Join(",", buList);
            if (bu.Length > 100)
                bu.Substring(0, 100);
            ei.Put("CS_CD", csCd);
            ei.Put("CS_NAME", csName);
            ei.Put("CS_NM", csNm);
            ei.Put("BU", bu);
            TrackingEDI.InboundBusiness.SMSMIHelper.setReserveValue(ShipmentId, ei);
            mixList.Add(ei);

            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (idList != null && !idList.Contains(UId))
                    idList.Add(UId);
                TrackingEDI.InboundBusiness.SMSMIHelper.GetDivisonBySMR(ReserveNo, TranType);
                InboundHandel.UpdateSMICUFT(ReserveNo, true);
                foreach (string shipment in SmList)
                {
                    TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = ShipmentId, StsCd = "018", Cmp = CompanyId, Remark = remark, Sender = UserId, Location = podcd, LocationName = "", StsDescp = "Order Truck By DN" });
                }
                for (int i = 0; i < OrdList.Count; i++)
                {
                    int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(OrdList[i]));
                    int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdList[i]));

                    if (r == c)
                    {
                        //更新運輸單的Cstatus
                        sql = "UPDATE SMORD SET CSTATUS='D' WHERE ORD_NO={0}";
                        if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                        {
                            sql = "UPDATE SMORD SET CSTATUS='R' WHERE ORD_NO={0}";
                        }
                        sql = string.Format(sql, SQLUtils.QuotedStr(OrdList[i]));
                        CommonHelp.exeSql(sql);
                    }
                }


                for (int i = 0; i < SmList.Count; i++)
                {
                    string updateArrivateDatetoSmord = SetArrivalDateToSMORD(SmList[i]);
                    CommonHelp.exeSql(updateArrivateDatetoSmord);
                    CalculateFee cs = new CalculateFee(SmList[i]);
                    Bill.WriteLogTagStart("dn叫车自动计价", SmList[i]);
                    List<string> emptyMessage = new List<string>();
                    cs.FindTrailerQuote(ReserveNo, SmList[i], emptyMessage);
                    InboundTransfer.UpdateBillInfoToSMORD(SmList[i], "", null);
                    Bill.WriteLogTagStart("结束计算", SmList[i]);
                }


            }
            catch (Exception ex)
            {
                returnMessage += ex.ToString();
            }
            return returnMessage;
        }

        #endregion

        public static bool checkIBCR(Dictionary<string, bool> shipmentDic, string ShipmentId)
        {
            bool hIBCR=false;
            if (!shipmentDic.ContainsKey(ShipmentId))
            {
                DataTable ptdt = InboundHandel.GetPTByPartyType(ShipmentId, "IBCR");
                foreach (DataRow dr in ptdt.Rows)
                {
                    string partyNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                    if (!string.IsNullOrEmpty(partyNo))
                    {
                        hIBCR = true;
                    }
                }
                shipmentDic.Add(ShipmentId, hIBCR);
            }
            else
            {
                hIBCR = shipmentDic[ShipmentId];
            }
            return hIBCR;
        }

        public static string SetArrivalDateToSMORD(string shipmentid, MixedList ml = null)
        {
            string sql = string.Format("SELECT TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID={0} ",
                SQLUtils.QuotedStr(shipmentid));
            string trantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string scmSql = string.Empty;
            if ("F".Equals(trantype)||"R".Equals(trantype))
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMICNTR WHERE
                 SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMICNTR.CNTR_NO=SMORD.CNTR_NO),PRIORITY=(SELECT MIN(PRIORITY) FROM SMICNTR WHERE
                 SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMICNTR.CNTR_NO=SMORD.CNTR_NO) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));

                scmSql = string.Format(@"UPDATE SMSMI SET PRODUCTION_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMICNTR WHERE  
                SMICNTR.SHIPMENT_ID=SMSMI.SHIPMENT_ID) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            else
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMIDN WHERE
                 SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID),PRIORITY=(SELECT MIN(PRIORITY) FROM SMIDN WHERE
                 SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID) WHERE SHIPMENT_ID={0}",
                SQLUtils.QuotedStr(shipmentid));

                scmSql = string.Format(@"UPDATE SMSMI SET PRODUCTION_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMIDN WHERE  
                SMIDN.SHIPMENT_ID=SMSMI.SHIPMENT_ID) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            if (ml == null)
            {
                OperationUtils.ExecuteUpdate(scmSql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else
                ml.Add(scmSql);
            return sql;
        }

        public static string GetPriority(string shipmentid)
        {
            if (string.IsNullOrEmpty(shipmentid)) return string.Empty;
            string[] shipmentids = { shipmentid };
            return GetPriority(shipmentids);
        }
        public static string GetPriority(string shipmentid, string cntrno)
        {
            return TrackingEDI.InboundBusiness.SMSMIHelper.GetPriority(shipmentid, cntrno);
        }
        public static string GetPriority(string[] shipmentids)
        {
            if (shipmentids.Length <= 0) return string.Empty;
            string sql=string.Format(@"SELECT MIN(PRIORITY) FROM (SELECT PRIORITY FROM SMICNTR WHERE SHIPMENT_ID IN {0} 
                            UNION  SELECT PRIORITY FROM SMIDN WHERE SHIPMENT_ID IN {0}) T", SQLUtils.Quoted(shipmentids)); ;
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static string getValueByCol(string[] colList)
        {
            string col = string.Join(",", colList.ToArray());
            if (col.Length > 500)
                col = col.Substring(0, 500);
            return col;
        }

        public static string getValueByCol(int [] colList)
        {
            string col = string.Join(",", colList.ToArray());
            if (col.Length > 500)
                col = col.Substring(0, 500);
            return col;
        }

        #region 進口取消叫車by lot no
        public static string InboundCancelTruckerByLotNo(string[] rodnos, string userid)
        {
            string returnMessage = "success", Trucker = "", GroupId = "", ShipmentId = "", CompanyId = "", ReserveNo = "", TruckerNm = "", DnNo = "", Status = "", Cmp = "", WsCd = "", GateNo = "", ReserveDate = "";
            //int ReserveFrom = 0, ReserveHour = 0;
            MixedList ml = new MixedList();
            foreach (string rodno in rodnos)
            {
                if (string.IsNullOrEmpty(rodno))
                    continue;
                string sql = string.Format("SELECT * FROM SMIRV WHERE ORD_INFO LIKE '%{0}%' AND STATUS NOT IN ('V')", rodno);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        Trucker = Prolink.Math.GetValueAsString(item["TRUCKER"]);
                        TruckerNm = Prolink.Math.GetValueAsString(item["TRUCKER_NM"]);
                        GroupId = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                        CompanyId = Prolink.Math.GetValueAsString(item["CMP"]);
                        ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                        ReserveNo = Prolink.Math.GetValueAsString(item["RESERVE_NO"]);
                        DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                        Status = Prolink.Math.GetValueAsString(item["STATUS"]);
                        //ReserveFrom = Prolink.Math.GetValueAsInt(item["RESERVE_FROM"]);
                        //ReserveHour = Prolink.Math.GetValueAsInt(item["RESERVE_HOUR"]);

                        Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                        WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                        GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                        ReserveDate = Prolink.Math.GetValueAsString(item["RESERVE_DATE"]);
                    }
                }
                if ("O".Equals(Status))
                {
                    return returnMessage = string.Format("It Can't Be Cancel，Becase This Reserve：{0} Was Gate Out !!", ReserveNo); ;
                }
                if ("I".Equals(Status))
                {
                    return returnMessage = string.Format("It Can't Be Cancel，Becase This Reserve：{0} Was Gate In !!", ReserveNo); ;
                }
                if ("P".Equals(Status))
                {
                    return returnMessage = string.Format("It Can't Be Cancel，Becase This Reserve：{0} Was Container Sealed !!", ReserveNo); ;
                }
                DateTime odt = DateTime.Now;                
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                sql = "UPDATE SMIRV SET STATUS='V', CANCEL_DATE={0}, CANCEL_DATE_L={1} WHERE RESERVE_NO={2}";
                sql = string.Format(sql, SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ReserveNo));
                ml.Add(sql);
                sql = "UPDATE SMORD SET CSTATUS='Y', LOT_NO=NULL WHERE ORD_NO=" + SQLUtils.QuotedStr(rodno);
                ml.Add(sql);

                sql = "UPDATE SMRDN SET PICKUP_DATE=NULL,ARRIVAL_DATE=NULL,RESERVE_NO=NULL,LOT_NO=NULL,CANCEL_NO=" + SQLUtils.QuotedStr(ReserveNo) + " WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                ml.Add(sql);

                sql = "UPDATE SMRCNTR SET PICKUP_DATE=NULL,ARRIVAL_DATE=NULL,RESERVE_NO=NULL,LOT_NO=NULL,CANCEL_NO=" + SQLUtils.QuotedStr(ReserveNo) + " WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                ml.Add(sql);
                DataTable mailGroupDt = MailTemplate.GetMailGroup(Trucker, GroupId, "CIC");
                if (mailGroupDt.Rows.Count > 0)
                {
                    foreach (DataRow item1 in mailGroupDt.Rows)
                    {
                        string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                        if (mailStr != "")
                        {
                            EvenFactory.AddEven(ReserveNo + "#" + ShipmentId + "#" + DnNo + "#" + TruckerNm + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), ReserveNo, "CIRVTK", null, 1, 0, mailStr, @Resources.Locale.L_ReserveManage_Business_95 + ReserveNo, "");
                        }
                    }
                }

                //if (Status == "R" || Status == "C")
                //{
                //    string str = string.Empty;
                //    string cstr = string.Empty;
                //    for (int i = ReserveFrom; i < ReserveFrom + ReserveHour; i++)
                //    {
                //        str += "H_" + i + "=NULL,";
                //        cstr += "C_" + i + "=NULL,";
                //    }
                //    cstr = cstr.Remove(cstr.Length - 1);
                //    str = str + cstr;
                //    sql = "UPDATE SMRVD SET {0} WHERE CMP={1} AND WS_CD={2} AND GATE_NO={3} AND RESERVE_DATE={4}";
                //    sql = string.Format(sql, str, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(GateNo), SQLUtils.QuotedStr(ReserveDate));
                //}
            }
            
            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }

            return returnMessage;
        }
        #endregion

        #region 進口取消叫車by reserve no

        #endregion

        public static bool IsTSALogistic(string CompanyId)
        {
            return TrackingEDI.InboundBusiness.SMSMIHelper.IsTSALogistic(CompanyId);
        }

        public static DataTable GetEdocData(string shipmentid, string companyid, string OUid = "", bool hasinbound = false)
        {
            string dnsql = string.Format("SELECT COMBINE_INFO,O_LOCATION,MASTER_NO FROM SMSMI WHERE SHIPMENT_ID={0}",
                SQLUtils.QuotedStr(shipmentid));
            string dninfo = string.Empty;
            string o_location = string.Empty;
            string masterno = string.Empty;
            DataTable smDt = OperationUtils.GetDataTable(dnsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count > 0)
            {
                dninfo = Prolink.Math.GetValueAsString(smDt.Rows[0]["COMBINE_INFO"]);
                o_location = Prolink.Math.GetValueAsString(smDt.Rows[0]["O_LOCATION"]);
                masterno = Prolink.Math.GetValueAsString(smDt.Rows[0]["MASTER_NO"]);
            }

            string sql = string.Format(@"SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,SUBSTRING(SMDN.DN_NO, 7,8)AS DN_NO FROM SMDN WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMINM' AS D_TYPE,SUBSTRING(SMINM.DN_NO, 7,8)AS DN_NO FROM SMINM WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMIRV' AS D_TYPE,{2} AS DN_NO FROM SMIRV WHERE SHIPMENT_INFO LIKE '%{1}%'",
               SQLUtils.QuotedStr(shipmentid), shipmentid, SQLUtils.QuotedStr(masterno));
            if (string.IsNullOrEmpty(OUid))
            {
                sql += string.Format(@"UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO  FROM SMSM WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO FROM SMSM WHERE COMBIN_SHIPMENT ={0}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
            }else
            {
                sql += string.Format(" UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO FROM SMSM WHERE COMBIN_SHIPMENT ={0} AND COMBIN_SHIPMENT!=SHIPMENT_ID", 
                    SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
                string sql1 = string.Format("SELECT IMPORT_NO FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(OUid));
                string importno = OperationUtils.GetValueAsString(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (!string.IsNullOrEmpty(importno))
                {
                    sql += string.Format(" UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO FROM SMSM WHERE SHIPMENT_ID ={0}",
                    SQLUtils.QuotedStr(importno), SQLUtils.QuotedStr(masterno));
                }
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
    }
}