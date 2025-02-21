using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
    public class BookingStatusManager
    {
        #region 注册类型
        /// <summary>
        /// 确认Air Booking Status：代码AIRBSMapping
        /// </summary>
        public static string AirStatusMapping = "AIRBSMapping";

        /// <summary>
        /// 确认Sea LCL Booking Status：代码LCLBSMapping
        /// </summary>
        public static string LCLStatusMapping = "LCLBSMapping";

        /// <summary>
        /// 确认Sea FCL Booking Status：代码FCLBSMapping
        /// </summary>
        public static string FCLStatusMapping = "FCLBSMapping";

        /// <summary>
        /// 确认Express Booking Status：代码EXPBSMapping
        /// </summary>
        public static string ExpStatusMapping = "EXPBSMapping";

        /// <summary>
        /// 确认Trucking Booking Status：代码TRKBSMapping
        /// </summary>
        public static string TruckBKStatusMapping = "TRKBSMapping";

        /// <summary>
        /// 确认Railway Booking Status：代码TRKBSMapping
        /// </summary>
        public static string RailWayBKStatusMapping = "RailWayMapping";

        /// <summary>
        /// 确认DECL Status：代码DECLSTSMapping
        /// </summary>
        public static string DeclStatusMapping = "DECLSTSMapping";

        /// <summary>
        /// 确认Tracking Status：代码TRACKSTSMapping
        /// </summary>
        public static string TrackStatusMapping = "TRACKSTSMapping";

        /// <summary>
        /// LNT Excel订舱确认：代码LNTConfirmMapping
        /// </summary>
        public static string LNTConfirmMapping = "LNTConfirmMapping";

        /// <summary>
        /// SMSM partytype导入
        /// </summary>
        public static string LogisticsMapping = "LogisticsMapping";

        public static string ChangePodMapping = "ChangePodMapping";

        /// <summary>
        /// SMSM  订舱确认查询汇总画面实际毛重和体积导入
        /// </summary>
        public static string GwAndCbmMapping = "GwAndCbmMapping";
        public static string TKBatchStatusMapping = "TKBatchStatusMapping";

        public static string BatchDnInfoMapping = "BatchDNInfoMapping";

        public static DataTable PortDataTable=new DataTable();
        public static DataTable TruckPortDataTable = new DataTable();
        #endregion

        public static void InitPortData()
        {
            string sql = "SELECT CNTRY_CD+PORT_CD CNPORT,PORT_CD,CNTRY_CD,PORT_NM FROM BSCITY";
            PortDataTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        public static void InitTruckPortData()
        {
            string sql = "SELECT * FROM BSTPORT";
            TruckPortDataTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        public static string HandleBookingStatus(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string shipmentid = ei.Get("SHIPMENT_ID");
            string confirm_type = Prolink.Math.GetValueAsString(parm["CONFIRM_TYPE"]);
            if (string.IsNullOrEmpty(shipmentid)) return BaseParser.ERROR;
            ei.PutKey("SHIPMENT_ID", shipmentid);
            string sql = string.Format("SELECT STATUS,TRAN_TYPE,BORDER,REGION,BOOKING_INFO,GROUP_ID,CMP,CARRIER,DN_NO,ISCOMBINE_BL FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count<=0) throw new Exception(shipmentid+"无订舱资料！");

            string border = Prolink.Math.GetValueAsString(dt.Rows[0]["BORDER"]);
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            string region = Prolink.Math.GetValueAsString(dt.Rows[0]["REGION"]);
            Func<string, string, string> Getvailidate = (fieldname, filevalue) =>
            {
                if (string.IsNullOrEmpty(fieldname))
                    return string.Format("订舱确认失败! 请输入:{0} \r\n", filevalue);
                return string.Empty;
            };

            Func<string, string, string,string,string> GetDatevailidate = (fieldname1, filevalue1, fieldname2, filevalue2) =>
            {
                if (string.IsNullOrEmpty(fieldname1) || string.IsNullOrEmpty(fieldname2)) return string.Empty;
                try
                {
                    DateTime time1 = Business.DateTimeUtils.SetDateStrToDate(fieldname1);
                    DateTime time2 = Business.DateTimeUtils.SetDateStrToDate(fieldname2);
                    if (time1 > time2)
                        return string.Format("订舱确认失败! 输入的{0}大于{1} \r\n", filevalue1, filevalue2);
                }catch(Exception ex){
                }
                return string.Empty;
            };

            string msg=string.Empty;
            if ("DECL".Equals(confirm_type))
            {
                 if ("H".Equals(status)) throw new Exception("该笔订舱资料"+shipmentid+"已经放行！");
                switch(border){
                    case "H":msg+=shipmentid+"已经放行，不可更新！";
                        break;
                    case "N":msg+=shipmentid+"还没有通知报关，不可更新！";
                        break;
                    case "M":
                    case "C":
                    case "S":msg += Getvailidate(ei.Get("DECL_RLS_DATE"), "报关时间为空");
                        break;
                    default:
                        msg += shipmentid + "还没有通知报关，不可更新！";
                        break;
                }
                if (!string.IsNullOrEmpty(msg))
                {
                    throw new Exception(msg);
                }
                ei.Put("BORDER", "C");
                return string.Empty;
            }

            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            ResultData rd=CheckSMEtd(shipmentid);
            if (!rd.IsSucceed)
            {
                throw new Exception(rd.Description);
            }

            //if (!"B".Equals(status))
            //{
            //    throw new Exception("该笔订舱资料"+shipmentid+"已经订舱或还未发起订舱！");
            //}

            string eta = GetETAate(ei, "ETA");
            string etd = GetETDate(ei, "ETD");
            msg += Getvailidate(eta, "ETA1");
            msg += Getvailidate(etd, "ETD1");
            switch (trantype)
            {
                case "L":
                    msg += Getvailidate(ei.Get("PORT_DATE"), "截进仓时间");
                    msg += GetDatevailidate(ei.Get("PORT_DATE"), "截进仓时间", etd, "ETD");
                    msg += Getvailidate(ei.Get("VESSEL1"), "船名");
                    msg += Getvailidate(ei.Get("VOYAGE1"), "船期");
                    //msg += Getvailidate(ei.Get("SCAC_CD"), "SCAC Code");
                    //msg += Getvailidate(ei.Get("MASTER_NO"), "Master NO");
                    //检查截提单 CUT_BL_DATE，截进仓时间与ETD比对//
                    msg += GetDatevailidate(ei.Get("CUT_BL_DATE"), "截提单时间", etd, "ETD");
                    DataTable baselclDt = TrackingEDI.Mail.MailTemplate.GetBaseData("'TCAR'", dt.Rows[0]["GROUP_ID"].ToString(), dt.Rows[0]["CMP"].ToString());
                    string lclcrcode = Prolink.Math.GetValueAsString(ei.Get("CARRIER"));
                    string lclcrname = TrackingEDI.Mail.MailTemplate.GetBaseCodeValueEmpty(baselclDt, "TCAR", lclcrcode);
                    if (!string.IsNullOrEmpty(lclcrcode))
                    {
                        if (string.IsNullOrEmpty(lclcrname))
                        {
                            msg += string.Format("订舱确认失败,该笔Shipment ID:{0} 的Carrier:{1}和系统的船公司建档不匹配！", shipmentid, lclcrcode);
                        };
                    }
                    break;
                case "F":
                    msg += Getvailidate(ei.Get("CUT_PORT_DATE"), "码头截进场_24hours");
                    msg += Getvailidate(ei.Get("RCV_DATE"), "截AMS/截提单");
                    msg += Getvailidate(ei.Get("PORT_NM"), "挂靠码头");
                    msg += Getvailidate(ei.Get("CUSTOMS_DATE"), "海关截投单");
                    msg += Getvailidate(ei.Get("PORT_RLS_DATE"), "码头截放行时间");
                    msg += Getvailidate(ei.Get("VESSEL1"), "船名");
                    msg += Getvailidate(ei.Get("VOYAGE1"), "船期");
                    DataTable baseDt = TrackingEDI.Mail.MailTemplate.GetBaseData("'TCAR'", dt.Rows[0]["GROUP_ID"].ToString(), dt.Rows[0]["CMP"].ToString());
                    string crcode = Prolink.Math.GetValueAsString(ei.Get("CARRIER"));
                    string crname = TrackingEDI.Mail.MailTemplate.GetBaseCodeValueEmpty(baseDt, "TCAR", crcode);
                    if (string.IsNullOrEmpty(crname))
                    {
                        msg += string.Format("订舱确认失败,该笔Shipment ID:{0} 的Carrier:{1}和系统的船公司建档不匹配！", shipmentid, crcode);
                    };
                    ei.Put("CARRIER_NM", crname);
                    if ("NA".Equals(region) || "SA".Equals(region) || "EU".Equals(region))
                    {
                        msg += Getvailidate(ei.Get("SCAC_CD"), "SCAC Code（因为目的地区域为:EU\\NA\\SA）");
                    }
                    msg += Getvailidate(ei.Get("MASTER_NO"), "Master NO");
                    //检查截提单 CUT_BL_DATE，截进仓时间与ETD比对//
                    msg += GetDatevailidate(ei.Get("CUT_BL_DATE"), "截提单时间", etd, "ETD");
                    break;
                case "A":
                    msg += Getvailidate(ei.Get("MASTER_NO"), "VESSEL1");
                    msg += Getvailidate(ei.Get("HOUSE_NO"), "House NO");
                    msg += Getvailidate(ei.Get("VESSEL1"), "航班");
                    break;
                case "D":
                case "E":
                    msg += Getvailidate(ei.Get("HOUSE_NO"), "快递单号");
                    break;
                case "T":
                    msg += Getvailidate(ei.Get("HOUSE_NO"), "运输号码");
                    break;
                case "R":
                    msg += Getvailidate(ei.Get("HOUSE_NO"), "House B/L");
                    msg += Getvailidate(ei.Get("VESSEL1"), "车次");
                    break;
            }
            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception( msg);
            }
            ei.PutDate("ETD", etd);
            if (TrackingEDI.Business.DateTimeUtils.IsDate(etd))
            {
                int[] ymw = TrackingEDI.Business.DateTimeUtils.DateToYMW(etd);
                if (ymw.Length >= 3)
                {
                    ei.Put("YEAR", ymw[0]);
                    ei.Put("MONTH", ymw[1]);
                    ei.Put("WEEKLY", ymw[2]);
                }
            }
            ei.PutDate("ETA", eta);
            //ei.Put("STATUS", "C");
            ei.Put("CORDER", "C");
            string bookingInfo=string.Empty;
            if(string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dt.Rows[0]["BOOKING_INFO"]))){
                bookingInfo=ei.Get("BOOKING_INFO").ToString();
            }else{
                string[] books=new string[2];
                books[0]="";
                books[1]=ei.Get("BOOKING_INFO").ToString();
                bookingInfo = string.Join(Environment.NewLine, books);
            }
            ei.PutExpress("BOOKING_INFO", "BOOKING_INFO+'" + bookingInfo + "'");
            ei.PutDate("RLS_CNTR_DATE", DateTime.Now);
            //CheckShipment(shipmentid);
            string torder = "N";
            if (IsApproveByFIM(shipmentid, ref border, ref torder, ref status))
            {
                if (torder == "N" || string.IsNullOrEmpty(torder))
                    ei.Put("TORDER", "S");
                if (border == "N" || string.IsNullOrEmpty(border))
                    ei.Put("BORDER", "M");
            }
            if ("A".Equals(status) || "B".Equals(status) || string.IsNullOrEmpty(status))
                ei.Put("STATUS", "C");

            ei.Put("POL_NAME", GetPortNm(ei.Get("POL_CD"), trantype));
            ei.Put("POR_NAME", GetPortNm(ei.Get("POR_CD"), trantype));
            ei.Put("POD_NAME", GetPortNm(ei.Get("POD_CD"), trantype));
            ei.Put("DEST_NAME", GetPortNm(ei.Get("DEST_CD"), trantype));
         
            string dnno = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
            string iscombine_bl = Prolink.Math.GetValueAsString(dt.Rows[0]["ISCOMBINE_BL"]);
            if (dnno.StartsWith("1090"))
            {
                ei.Put("STATUS", "O");
                ei.Put("BORDER", "N");
                ei.Put("TORDER", "N");
            }
            if ("C".Equals(iscombine_bl) || "Y".Equals(iscombine_bl))
            {
                ei.Put("SORDER", "C");  //更改合并提单通知状态为C表示已经回写
                ei.Put("CORDER", "Y");  //更改合并提单通知状态为C表示已经回写
                ei.Put("STATUS", "Y");  //将订舱主状态回写成合并提单确认
            }

            Func<string, bool> GetCnt = (cnt) =>
            {
                if(string.IsNullOrEmpty(ei.Get(cnt))) return true;
                return false;
            };
            bool ishascnt=GetCnt("CNT20") && GetCnt("CNT40") && GetCnt("CNT40HQ");
            if (ishascnt)
            {
                ei.Remove("CNT20");
                ei.Remove("CNT40");
                ei.Remove("CNT40HQ");
            }

            return string.Empty;
        }

        public static string HandleLNTBookingStatus(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string subdnno = ei.Get("SUB_DN_NO");
            ei.PutKey("SUB_DN_NO", subdnno);
            string sql = string.Format("SELECT STATUS,TRAN_TYPE,BORDER FROM SMSM WHERE SUB_DN_NO={0}", SQLUtils.QuotedStr(subdnno));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) throw new Exception(subdnno + "无订舱资料！");
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            string msg = string.Empty;
            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);

            Func<string, string, string> Getvailidate = (fieldname, filevalue) =>
            {
                if (string.IsNullOrEmpty(fieldname))
                    return string.Format("订舱确认失败! 请输入:{0}", filevalue);
                return string.Empty;
            };
            switch (trantype)
            {
                case "L":
                case "F":
                    msg += Getvailidate(ei.Get("VESSEL1"), "船名");
                    msg += Getvailidate(ei.Get("VOYAGE1"), "船期");
                    msg += Getvailidate(ei.Get("SCAC_CD"), "SCAC Code");
                    msg += Getvailidate(ei.Get("MASTER_NO"), "Master NO");
                    break;
                case "A":
                    msg += Getvailidate(ei.Get("MASTER_NO"), "VESSEL1");
                    //msg += Getvailidate(ei.Get("HOUSE_NO"), "House NO");
                    msg += Getvailidate(ei.Get("VESSEL1"), "航班");
                    break;
                case "D":
                case "E":
                    msg += Getvailidate(ei.Get("MASTER_NO"), "快递单号");
                    break;
                case "T":
                    msg += Getvailidate(ei.Get("MASTER_NO"), "运输号码");
                    break;
                case "R":
                    msg += Getvailidate(ei.Get("MASTER_NO"), "House B/L");
                    msg += Getvailidate(ei.Get("VESSEL1"), "车次");
                    break;
            }
            if (!string.IsNullOrEmpty(msg)) throw new Exception(msg);

            ei.Put("STATUS", "C");
            ei.Put("CORDER", "C");
            ei.PutDate("RLS_CNTR_DATE", DateTime.Now);

            ei.Put("POL_NAME",GetPortNm(ei.Get("POL_CD"),trantype));
            ei.Put("POR_NAME", GetPortNm(ei.Get("POR_CD"), trantype));
            ei.Put("POD_NAME", GetPortNm(ei.Get("POD_CD"), trantype));
            ei.Put("DEST_NAME", GetPortNm(ei.Get("DEST_CD"), trantype));
            return string.Empty;
        }

        public static string GetPortNm(string portcd,string trantype)
        {
            if (string.IsNullOrEmpty(portcd)) return string.Empty;
            if ("T".Equals(trantype))
            {
                if (TruckPortDataTable.Rows.Count == 0) InitTruckPortData();
                DataRow[] drs = TruckPortDataTable.Select(string.Format("PORT_CD={0}", SQLUtils.QuotedStr(portcd)));
                if (drs.Length > 0)
                {
                    return Prolink.Math.GetValueAsString(drs[0]["PORT_NM"]);
                }
            }
            else
            {
                if (PortDataTable.Rows.Count == 0) InitPortData();
                DataRow[] drs = PortDataTable.Select(string.Format("CNPORT={0}", SQLUtils.QuotedStr(portcd)));
                if (drs.Length > 0)
                {
                    return Prolink.Math.GetValueAsString(drs[0]["PORT_NM"]);
                }
            }
            return string.Empty;
        }

        public static string HandleLogisctImport(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string shipmentid = ei.Get("SHIPMENT_ID");
            if (string.IsNullOrEmpty(shipmentid)) return BaseParser.ERROR;
            ei.PutKey("SHIPMENT_ID", shipmentid);
            string sql = string.Format("SELECT U_ID,STATUS,TRAN_TYPE,BORDER,REGION,BOOKING_INFO,GROUP_ID,CMP,CARRIER,DN_NO,ISCOMBINE_BL,TORDER FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) throw new Exception(shipmentid + "无订舱资料！");
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            //if (!"A".Equals(status))
            //    throw new Exception(shipmentid + "非未处理状态，不允许导入Partytype");
            string shipmentuid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string Torder = Prolink.Math.GetValueAsString(dt.Rows[0]["TORDER"]);
            string BORDER = Prolink.Math.GetValueAsString(dt.Rows[0]["BORDER"]);
            string bocd = ei.Get("BO_CD");
            string spcd = ei.Get("SP_CD");
            //string rvstatusSQL = string.Format("SELECT STATUS FROM SMRV WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid));
            //string rvstatus = OperationUtils.GetValueAsString(rvstatusSQL, Prolink.Web.WebContext.GetInstance().GetConnection());

            if ((!string.IsNullOrEmpty(bocd)) && (!string.IsNullOrEmpty(spcd)))
                throw new Exception(shipmentid + "BO_Code和SP_Code不能同时填写！");
            string FS_CD = ei.Get("FS_CD");
            string DN_ETD = ei.Get("DN_ETD");
            string PPOD_CD = ei.Get("PPOD_CD");
            string PDEST_CD = ei.Get("PDEST_CD");
            string BM_CD = ei.Get("BM_CD");
            string CR_CD = ei.Get("CR_CD");
            string BR_CD = ei.Get("BR_CD");
            if ((((!string.IsNullOrEmpty(DN_ETD)) || (!string.IsNullOrEmpty(PPOD_CD)) || (!string.IsNullOrEmpty(PDEST_CD))) && !"A".Equals(status)) || (!string.IsNullOrEmpty(CR_CD)&&"T".Equals(trantype) && !"A".Equals(status)))
            {
                throw new Exception(shipmentid + "非未处理状态，不允许导入Partytype");
            }
            bool statusb = false;
            switch (status)//A:未处理;B:订舱;C:订舱确认;D:叫柜;I:入厂
            {
                case "A":
                case "B":
                case "C":
                case "D":
                case "I":
                    statusb = true; break;
            }
            if (((!string.IsNullOrEmpty(BM_CD)) || (!string.IsNullOrEmpty(FS_CD)) || (!string.IsNullOrEmpty(bocd)) || (!string.IsNullOrEmpty(spcd))) && !"A".Equals(status))
            {
                throw new Exception(shipmentid + "非未处理状态，不允许导入Partytype");
                //throw new Exception(shipmentid + ":" + "已封柜，无法修改FS，BO,SP信息");
            }

            if (!string.IsNullOrEmpty(CR_CD) && !string.IsNullOrEmpty(Torder) && (!("A".Equals(status) || "B".Equals(status) || "C".Equals(status)))) 
            {
                throw new Exception(shipmentid + ":" + "已叫车，无法修改CR信息");
            }

            if (!string.IsNullOrEmpty(BR_CD) && !string.IsNullOrEmpty(Torder) && ("H".Equals(BORDER) || "C".Equals(BORDER) || "S".Equals(BORDER)))
            {
                throw new Exception(shipmentid + ":" + "已通知报关，无法修改BR信息");
            }
            DataTable baseDt = (DataTable)parm["bacodeDt"];
            Func<string, string, string> SetPartyInfo = (fieldname, partytype) =>
            {
                string partyno = Prolink.Math.GetValueAsString(ei.Get(fieldname));
                if (string.IsNullOrEmpty(partyno))
                {
                    ei.Remove(fieldname);
                    return string.Empty;
                }
                DataTable smptydt = OperationUtils.GetDataTable("SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO LIKE '%" + partyno + "%'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smptydt.Rows.Count <= 0)
                    return partyno + "客户建档中无对应客户资料; ";
                partyno = Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"].ToString());
                string message = InsetIntoMlSMPTYDt(ml, partytype, partyno, shipmentid, shipmentuid, smptydt);
                switch (fieldname)
                {
                    case "BM_CD":
                        ei.Remove(fieldname);
                        break;
                    case "FS_CD":
                        DataRow bsrow = TrackingEDI.Mail.MailTemplate.GetBCValueByCondtions(baseDt, "TCAR", string.Format("AR_CD={0}", SQLUtils.QuotedStr(partyno)));
                        ei.Remove(fieldname);
                        if (bsrow != null)
                        {
                            ei.Put("CARRIER", Prolink.Math.GetValueAsString(bsrow["CD"]));
                            ei.Put("CARRIER_NM", Prolink.Math.GetValueAsString(bsrow["CD_DESCP"]));
                        }
                        break;
                    case "SP_CD":
                    case "BO_CD":
                        ei.Remove(fieldname);
                        ei.Put("LSP_NO", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"]));
                        ei.Put("LSP_NM", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NAME"]));
                        break;
                    case "CR_CD":
                        if ("T".Equals(trantype) || "D".Equals(trantype))
                        {
                            ei.Put("LSP_NO", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"]));
                            ei.Put("LSP_NM", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NAME"]));
                        }
                        ei.Put(partytype + "_CD", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"]));
                        ei.Put(partytype + "_NM", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NAME"]));
                        break;
                    case "BR_CD":
                        ei.Put(partytype + "_CD", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"]));
                        ei.Put(partytype + "_NM", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NAME"]));
                        break;
                    default:
                        ei.Put(partytype + "_CD", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"]));
                        ei.Put(partytype + "_NM", Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NAME"]));
                        break;
                }

                return string.Empty;
            };

            string msg = string.Empty;
            msg += SetPartyInfo("BR_CD", "BR");
            msg += SetPartyInfo("CR_CD", "CR");
            msg += SetPartyInfo("BO_CD", "BO");
            msg += SetPartyInfo("SP_CD", "SP");
            msg += SetPartyInfo("FS_CD", "FS");
            msg += SetPartyInfo("BM_CD", "BM");
            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception(shipmentid + ":" + msg);
            }

            if (PortDataTable.Rows.Count == 0)
            {
                InitPortData();
            }

            //Func<string, string, string> RemoveEditField = (field, tran) =>
            //{
            //    if (string.IsNullOrEmpty(portcd)) return string.Empty;

            //    DataRow[] drs = PortDataTable.Select(string.Format("CNPORT={0}", SQLUtils.QuotedStr(portcd)));
            //    if (drs.Length > 0)
            //    {
            //        return Prolink.Math.GetValueAsString(drs[0]["PORT_NM"]);
            //    }
            //    return string.Empty;
            //};
            Func<string, string, string> RemoveEditField = (field, tran) =>
             {
                 var cd = ei.Get(field);
                 if (string.IsNullOrEmpty(cd))
                 {
                     ei.Remove(field);
                     return string.Empty;
                 }
                 var name = GetPortNm(cd, tran);
                 switch (field)
                 {
                     case "PPOD_CD":
                         ei.Put("PPOD_NAME", name);
                         ei.Put("POD_CD", cd);
                         ei.Put("POD_NAME", name);
                         break;
                     case "PDEST_CD":
                         ei.Put("PDEST_NAME", name);
                         ei.Put("DEST_CD", cd);
                         ei.Put("DEST_NAME", name);
                         break;
                 }
                 return string.Empty;
             };
            RemoveEditField("PPOD_CD",trantype);
            RemoveEditField("PDEST_CD", trantype);
            RemoveEditField("DN_ETD", trantype);
            return string.Empty;
        }

        public static string HandleGwAndCbmImport(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string shipmentid = ei.Get("SHIPMENT_ID");
            string houseno=ei.Get("HOUSE_NO");
            string masterno=ei.Get("MASTER_NO");
            if ("eg.".Equals(shipmentid))
                return BaseParser.ERROR;
            string conditions = string.Empty;
            if (string.IsNullOrEmpty(shipmentid))
            {
                if (string.IsNullOrEmpty(houseno))
                {
                    if (string.IsNullOrEmpty(masterno))
                        return BaseParser.ERROR;
                    ei.PutKey("MASTER_NO", masterno);
                    conditions = string.Format(" AND MASTER_NO={0}", SQLUtils.QuotedStr(masterno));
                }
                else
                {
                    ei.PutKey("HOUSE_NO", houseno);
                    conditions = string.Format(" AND HOUSE_NO={0}", SQLUtils.QuotedStr(houseno));
                }
            }
            else
            {
                ei.PutKey("SHIPMENT_ID", shipmentid);
                conditions = string.Format(" AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }

            string sql = string.Format("SELECT U_ID,STATUS,TRAN_TYPE,GROUP_ID,CMP,CW,GW,CBM,MASTER_NO,HOUSE_NO FROM SMSM WHERE 1=1 {0}", conditions);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count <= 0) throw new Exception("失败：" + shipmentid + "/" + houseno + "/" + masterno + " 无订舱资料！");
            if (dt.Rows.Count >= 2) throw new Exception("失败：" + shipmentid + "/" + houseno + "/" + masterno + " 对应多笔的订舱资料，无法更新！");
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);

            string shipmentuid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            decimal oldcw = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["CW"]);

            double gw = Prolink.Math.GetValueAsDouble(ei.Get("GW"));
            double cbm = Prolink.Math.GetValueAsDouble(ei.Get("CBM"));
            Func<string, double, double> RemoveEditField = (field, val) =>
            {
                var cd = ei.Get(field);
                if (string.IsNullOrEmpty(cd))
                {
                    ei.Remove(field);
                    val = Prolink.Math.GetValueAsDouble(dt.Rows[0][field]);
                }
                else
                {
                    val = Prolink.Math.GetValueAsDouble(ei.Get(field));
                }
                return val;
            };
            Func<string, string> RemoveEditFields = (field) =>
            {
                var cd = ei.Get(field);
                if (string.IsNullOrEmpty(cd))
                {
                    ei.Remove(field);
                }
                return string.Empty;
            };
            gw = RemoveEditField("GW", gw);
            cbm = RemoveEditField("CBM", cbm);
            RemoveEditFields("MASTER_NO");
            RemoveEditFields("HOUSE_NO");
            RemoveEditFields("SHIPMENT_ID");

            Func<string, string, double, double, double> GetCw = (strvw,fuctran, fucgw, fuccbm) =>
            {
                double ttlcbm = 0;
                switch (fuctran)
                {
                    case "A":
                        ttlcbm = fuccbm * 166.67;
                        if (!string.IsNullOrEmpty(strvw))
                        {
                            ttlcbm = Prolink.Math.GetValueAsDouble(strvw);
                        }
                        ei.Put("VW", ttlcbm);
                        if (fucgw >= ttlcbm)
                        {
                            ttlcbm = fucgw;
                        }
                        ttlcbm = Math.Round(ttlcbm * 1, 1);
                        return ttlcbm;
                        break;
                    case "E":
                        ttlcbm = fuccbm * 200;
                        ttlcbm = Math.Round(ttlcbm * 1, 4);
                        if (!string.IsNullOrEmpty(strvw))
                        {
                            ttlcbm = Prolink.Math.GetValueAsDouble(strvw);
                        }
                        ei.Put("VW", ttlcbm);
                        if (fucgw >= ttlcbm)
                        {
                            ttlcbm = fucgw;
                        }
                        
                        var intcbm =(int)ttlcbm;    //取整
                        double minuite = ttlcbm - intcbm;
                        if (ttlcbm > intcbm)
                        {
                            if (ttlcbm > 20)
                            {
                                if (minuite > 0)
                                    ttlcbm = intcbm + 1;
                            }
                            else
                            {
                                if (minuite < 0.5)
                                    ttlcbm = intcbm + 0.5;
                                else if (minuite > 0.5)
                                    ttlcbm = intcbm + 1;
                            }
                        }
                        break;
                    default:
                        ttlcbm = 0;
                        return ttlcbm;
                }
                return ttlcbm;
            };
            string vw = Prolink.Math.GetValueAsString(ei.Get("VW"));
            double cw=GetCw(vw,trantype, gw, cbm);
            if (trantype == "A" || trantype == "E")
                ei.Put("CW", cw);

            if ("A" != status && "B" != status)
            {
                EditInstruct AutoValuationTaskEi = new EditInstruct("AUTO_VALUATION_TASK", EditInstruct.INSERT_OPERATION);
                AutoValuationTaskEi.Put("U_ID", shipmentuid);
                AutoValuationTaskEi.Put("SMU_ID", shipmentuid);
                AutoValuationTaskEi.Put("CREATE_BY", parm["UserId"].ToString());
                AutoValuationTaskEi.PutDate("CREATE_DATE", DateTime.Now);
                ml.Add(AutoValuationTaskEi);
            }
            return string.Empty;
        }

        private static string GetETAate(EditInstruct ei, string fieldname)
        {
            if (!string.IsNullOrEmpty(ei.Get(fieldname+"4")))
            {
                return ei.Get(fieldname + "4");
            } 
            if (!string.IsNullOrEmpty(ei.Get(fieldname + "3")))
            {
                return ei.Get(fieldname + "3");
            }
            if (!string.IsNullOrEmpty(ei.Get(fieldname + "2")))
            {
                return ei.Get(fieldname + "2");
            }
            if (!string.IsNullOrEmpty(ei.Get(fieldname + "1")))
            {
                return ei.Get(fieldname + "1");
            }
            return ei.Get(fieldname);
        }

        private static string GetETDate(EditInstruct ei, string fieldname)
        {
            if (!string.IsNullOrEmpty(ei.Get(fieldname + "1")))
            {
                return ei.Get(fieldname + "1");
            }
            if (!string.IsNullOrEmpty(ei.Get(fieldname + "2")))
            {
                return ei.Get(fieldname + "2");
            }
            if (!string.IsNullOrEmpty(ei.Get(fieldname + "3")))
            {
                return ei.Get(fieldname + "3");
            }
            if (!string.IsNullOrEmpty(ei.Get(fieldname + "4")))
            {
                return ei.Get(fieldname + "4");
            }
            return ei.Get(fieldname);
        }

        private static string GetPort(string val, List<string> portC_list, string tran_mode)
        {
            if (string.IsNullOrEmpty(val))
                return val;
            val = val.ToUpper();
            string sql = "";

            if ("D".Equals(tran_mode) || "T".Equals(tran_mode))
                sql = string.Format("SELECT PORT_CD,CNTRY_CD,[STATE],REGION,PORT_NM FROM BSTPORT WHERE PORT_CD={0}", SQLUtils.QuotedStr(val));
            else
            {
                if (val.Length != 5)
                {
                    return val;
                    //sb.Append(string.Format("{0}不是有效的港口代码;", val));
                }
                sql = string.Format("SELECT PORT_CD,CNTRY_CD,[STATE],REGION,PORT_NM FROM BSCITY WHERE CNTRY_CD={0} AND PORT_CD={1}", SQLUtils.QuotedStr(val.Substring(0, 2)), SQLUtils.QuotedStr(val.Substring(2, 3)));
            }

            if (!portC_list.Contains(sql))
                portC_list.Add(sql);
            return val;
        }

        public static bool CheckShipment(string shipmentId)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return true;
            DataRow row = dt.Rows[0];
            List<Tuple<string, string, CheckShipmentModes>> items = new List<Tuple<string, string, CheckShipmentModes>>();
            items.Add(new Tuple<string, string, CheckShipmentModes>("PPOL_CD", "POL_CD", CheckShipmentModes.POL));
            items.Add(new Tuple<string, string, CheckShipmentModes>("PPOD_CD", "POD_CD", CheckShipmentModes.POD));
            items.Add(new Tuple<string, string, CheckShipmentModes>("PPOR_CD", "POR_CD", CheckShipmentModes.POR));
            items.Add(new Tuple<string, string, CheckShipmentModes>("PDEST_CD", "DEST_CD", CheckShipmentModes.DEST));
            List<string> checkResult = new List<string>();
            Action<Tuple<string, string, CheckShipmentModes>> check = item =>
            {
                string code1 = Prolink.Math.GetValueAsString(row[item.Item1]);
                string code2 = Prolink.Math.GetValueAsString(row[item.Item2]);
                if (string.IsNullOrEmpty(code1) || string.IsNullOrEmpty(code2)) return;
                if (code1 == code2) return;
                string msg = string.Format("预计{0}:{1}与实际{0}:{2}不一样，烦请验证!", item.Item3.ToString(), code1, code2);
                checkResult.Add(msg);
            };
            items.ForEach(item => check(item));
            if (checkResult.Count <= 0) return true;
            string bookingUser = Prolink.Math.GetValueAsString(row["BOOKING_USER"]);
            sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID={0}", SQLUtils.QuotedStr(bookingUser));
            string mailTo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string uid = Prolink.Math.GetValueAsString(row["U_ID"]);
            TrackingEDI.Business.EvenFactory.AddEven(Guid.NewGuid().ToString(), uid, MailManager.RouteNotify, null, 1, 0, mailTo,
                shipmentId+"：港口不匹配提醒", string.Join(Environment.NewLine, checkResult));
            return false;
        }

        enum CheckShipmentModes { POL, POD, POR, DEST }

        public static bool  IsApproveByFIM(string shipmentid,ref string border,ref string torder,ref string smstatus)
        {
            bool isOK = false;
            if (string.IsNullOrEmpty(shipmentid))
            {
                return false;
            }
            string sql = string.Format("SELECT CORDER,TORDER,BORDER,PARTIAL_FLAG,STATUS FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return false;
            }
            DataRow dr = dt.Rows[0];
            string bltype = Prolink.Math.GetValueAsString(dr["PARTIAL_FLAG"]);
            string corder = Prolink.Math.GetValueAsString(dr["CORDER"]);
            border = Prolink.Math.GetValueAsString(dr["BORDER"]);
            torder = Prolink.Math.GetValueAsString(dr["TORDER"]);
            smstatus = Prolink.Math.GetValueAsString(dr["STATUS"]);
            if ("Y".Equals(bltype)) return true;   //如果
            sql = string.Format(@"SELECT AR.STATUS FROM APPROVE_RECORD AR,SMDN SN 
                        WHERE AR.REF_NO=SN.DN_NO AND AR.APPROVE_CODE=SN.APPROVE_TYPE AND AR.ROLE='FIM'
                        AND SN.SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                isOK = false;
                foreach (DataRow aprow in dt.Rows)
                {
                    isOK = true;
                    string status = Prolink.Math.GetValueAsString(aprow["STATUS"]);
                    if (status != "1")
                    {
                        isOK = false;
                        return isOK;
                    }
                }
                return isOK;
            }
            return isOK;
        }

        public static bool IsApproveByFIM(string shipmentid, ref string border, ref string torder, ref string smstatus,ref string corder)
        {
            bool isOK = false;
            if (string.IsNullOrEmpty(shipmentid))
            {
                return false;
            }
            string sql = string.Format("SELECT CORDER,TORDER,BORDER,PARTIAL_FLAG,STATUS FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return false;
            }
            DataRow dr = dt.Rows[0];
            string bltype = Prolink.Math.GetValueAsString(dr["PARTIAL_FLAG"]);
            corder = Prolink.Math.GetValueAsString(dr["CORDER"]);
            border = Prolink.Math.GetValueAsString(dr["BORDER"]);
            torder = Prolink.Math.GetValueAsString(dr["TORDER"]);
            smstatus = Prolink.Math.GetValueAsString(dr["STATUS"]);
            if ("Y".Equals(bltype)) return true;   //如果
            sql = string.Format(@"SELECT AR.STATUS FROM APPROVE_RECORD AR,SMDN SN 
                        WHERE AR.REF_NO=SN.DN_NO AND AR.APPROVE_CODE=SN.APPROVE_TYPE AND AR.ROLE='FIM'
                        AND SN.SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                isOK = false;
                foreach (DataRow aprow in dt.Rows)
                {
                    isOK = true;
                    string status = Prolink.Math.GetValueAsString(aprow["STATUS"]);
                    if (status != "1")
                    {
                        isOK = false;
                        return isOK;
                    }
                }
                return isOK;
            }
            return isOK;
        }

        public static bool IsApproveByFIM(string shipmentid, ref string border, ref string torder, ref string smstatus, ref string corder,ref List<string>dnlist)
        {
            bool isOK = false;
            if (string.IsNullOrEmpty(shipmentid))
            {
                return false;
            }
            string sql = string.Format("SELECT CORDER,TORDER,BORDER,PARTIAL_FLAG,STATUS,COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return false;
            }
            
            DataRow dr = dt.Rows[0];
            string dn = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]);
            string[] dns = dn.Split(',');
            foreach (string item in dns)
            {
                if (!string.IsNullOrEmpty(item))
                    dnlist.Add(item);
            }
            string bltype = Prolink.Math.GetValueAsString(dr["PARTIAL_FLAG"]);
            corder = Prolink.Math.GetValueAsString(dr["CORDER"]);
            border = Prolink.Math.GetValueAsString(dr["BORDER"]);
            torder = Prolink.Math.GetValueAsString(dr["TORDER"]);
            smstatus = Prolink.Math.GetValueAsString(dr["STATUS"]);
            if ("Y".Equals(bltype)) return true;   //如果
            sql = string.Format(@"SELECT AR.STATUS,SN.DN_NO FROM APPROVE_RECORD AR,SMDN SN 
                        WHERE AR.REF_NO=SN.DN_NO AND AR.APPROVE_CODE=SN.APPROVE_TYPE AND AR.ROLE='FIM'
                        AND SN.SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            bool notconfirm = true;
            if (dt.Rows.Count > 0)
            {
                isOK = false;
                foreach (DataRow aprow in dt.Rows)
                {
                    isOK = true;
                    string status = Prolink.Math.GetValueAsString(aprow["STATUS"]);
                    if (status != "1")
                    {
                        //dnlist.Add(Prolink.Math.GetValueAsString(aprow["DN_NO"]));
                        isOK = false;
                        notconfirm = false;
                    }
                    else
                    {
                        dnlist.Remove(Prolink.Math.GetValueAsString(aprow["DN_NO"]));
                    }
                }
                if (notconfirm)
                {
                    if (dnlist.Count > 0)
                        return false;
                    return isOK;
                }
                else
                    return notconfirm;
            }
            return isOK;
        }


        public static string InsetIntoMlBySMPTY(MixedList ml, string partytype, string partyno, string shipmentid, string uid,SmptyInfo smptyinfo,string partyname="",TransferUser tuser=null)
        {
            string msg = string.Empty;
            EditInstruct ei = new EditInstruct("SMSMPT", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", Guid.NewGuid().ToString());
            ei.Put("U_FID", uid);
            ei.Put("SHIPMENT_ID", shipmentid);
            string sql = "SELECT CD,CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND GROUP_ID='TPV' AND CD=" + SQLUtils.QuotedStr(partytype);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                ei.Put("PARTY_TYPE", dr["CD"]);
                ei.Put("TYPE_DESCP", dr["CD_DESCP"]);
                ei.Put("ORDER_BY", dr["ORDER_BY"]);
            }
            else
            {
                return partytype + "：代码建档中无对应";
            }
            sql = "SELECT * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(partyno);
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                ei.Put("PARTY_NAME", dr["PARTY_NAME"]);
                ei.Put("PARTY_MAIL", dr["PARTY_MAIL"]);
                ei.Put("PART_ADDR1", dr["PART_ADDR1"]);
                ei.Put("PART_ADDR2", dr["PART_ADDR2"]);
                ei.Put("PART_ADDR3", dr["PART_ADDR3"]);
                ei.Put("PARTY_ATTN", dr["PARTY_ATTN"]);
                ei.Put("PARTY_TEL", dr["PARTY_TEL"]);
                ei.Put("PARTY_NO", dr["PARTY_NO"]);
                ei.Put("CNTY", dr["CNTY"]);
                ei.Put("CNTY_NM", dr["CNTY_NM"]);
                ei.Put("CITY", dr["CITY"]);
                ei.Put("CITY_NM", dr["CITY_NM"]);
                ei.Put("STATE", dr["STATE"]);
                ei.Put("ZIP", dr["ZIP"]);

                ei.Put("PART_ADDR4", dr["PART_ADDR4"]);
                ei.Put("PART_ADDR5", dr["PART_ADDR5"]);
                ei.Put("PARTY_NAME2", dr["PARTY_NAME2"]);
                ei.Put("PARTY_NAME3", dr["PARTY_NAME3"]);
                ei.Put("PARTY_NAME4", dr["PARTY_NAME4"]);
                ei.Put("FAX_NO", dr["PARTY_FAX"]);
                ei.Put("TAX_NO", dr["TAX_NO"]);
                if (!string.IsNullOrEmpty(partyname))
                    ei.Put("PARTY_NAME", partyname);
                if (smptyinfo != null)
                {
                    ei.Put("PARTY_NAME", smptyinfo.PartyName);
                    ei.Put("PART_ADDR1", smptyinfo.PartAddr1);
                    ei.Put("PART_ADDR2", smptyinfo.PartAddr2);
                    ei.Put("PART_ADDR3", smptyinfo.PartAddr3);
                    ei.Put("PARTY_ATTN", smptyinfo.PartyAttn);
                    ei.Put("PARTY_TEL", smptyinfo.PartyTel);
                    ei.Put("CNTY", smptyinfo.Cnty);
                    ei.Put("CNTY_NM", smptyinfo.CntyNm);
                    ei.Put("CITY", smptyinfo.City);
                    ei.Put("CITY_NM", smptyinfo.CityNm);
                    ei.Put("STATE", smptyinfo.State);
                    ei.Put("ZIP", smptyinfo.Zip);
                }

                if (tuser != null)
                {
                    ei.Put("PARTY_ATTN", tuser.PartyAttn);
                    ei.Put("PARTY_TEL", tuser.PartyTel);
                }
            }
            else
            {
                return partyno + "：客户建档中无对应";
            }

            ml.Add(ei);
            return msg;
        }

        public static string InsetIntoMlSMPTYDt(MixedList ml, string partytype, string partyno, string shipmentid, string uid,DataTable dt )
        {
            string msg = string.Empty;
            EditInstruct ei = new EditInstruct("SMSMPT", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", Guid.NewGuid().ToString());
            ei.Put("U_FID", uid);
            ei.Put("SHIPMENT_ID", shipmentid);
            string sql = "SELECT CD,CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND GROUP_ID='TPV' AND CD=" + SQLUtils.QuotedStr(partytype);
            DataTable bsdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (bsdt.Rows.Count > 0)
            {
                DataRow dr = bsdt.Rows[0];
                ei.Put("PARTY_TYPE", partytype);
                ei.Put("TYPE_DESCP", dr["CD_DESCP"]);
                ei.Put("ORDER_BY", dr["ORDER_BY"]);
            }
            else
            {
                return partytype + "：代码建档中无对应";
            }
            //sql = "SELECT * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(partyno);
            //dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                ei.Put("PARTY_NAME", dr["PARTY_NAME"]);
                ei.Put("PARTY_MAIL", dr["PARTY_MAIL"]);
                ei.Put("PART_ADDR1", dr["PART_ADDR1"]);
                ei.Put("PART_ADDR2", dr["PART_ADDR2"]);
                ei.Put("PART_ADDR3", dr["PART_ADDR3"]);
                ei.Put("PARTY_ATTN", dr["PARTY_ATTN"]);
                ei.Put("PARTY_TEL", dr["PARTY_TEL"]);
                ei.Put("PARTY_NO", dr["PARTY_NO"]);
                ei.Put("CNTY", dr["CNTY"]);
                ei.Put("CNTY_NM", dr["CNTY_NM"]);
                ei.Put("CITY", dr["CITY"]);
                ei.Put("CITY_NM", dr["CITY_NM"]);
                ei.Put("STATE", dr["STATE"]);
                ei.Put("ZIP", dr["ZIP"]);

                ei.Put("PART_ADDR4", dr["PART_ADDR4"]);
                ei.Put("PART_ADDR5", dr["PART_ADDR5"]);
                ei.Put("PARTY_NAME2", dr["PARTY_NAME2"]);
                ei.Put("PARTY_NAME3", dr["PARTY_NAME3"]);
                ei.Put("PARTY_NAME4", dr["PARTY_NAME4"]);
                ei.Put("FAX_NO", dr["PARTY_FAX"]);
                ei.Put("TAX_NO", dr["TAX_NO"]);
            }
            else
            {
                return partyno + "：客户建档中无对应";
            }
            EditInstruct delei = new EditInstruct("SMSMPT", EditInstruct.DELETE_OPERATION);
            delei.PutKey("SHIPMENT_ID", shipmentid);
            delei.PutKey("PARTY_TYPE", partytype);
            ml.Add(delei);
            ml.Add(ei);
            return msg;
        }

        public static string InsertIntoSMDNPT(MixedList ml, string partytype, string partyno, string dnno, string uid, DataTable dt)
        {
            string msg = string.Empty;
            EditInstruct ei = new EditInstruct("SMDNPT", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", uid);
            //ei.Put("U_FID", uid);
            ei.Put("DN_NO", dnno);
            string sql = "SELECT CD,CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND GROUP_ID='TPV' AND CD=" + SQLUtils.QuotedStr(partytype);
            DataTable bsdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (bsdt.Rows.Count > 0)
            {
                DataRow dr = bsdt.Rows[0];
                ei.Put("PARTY_TYPE", partytype);
                ei.Put("TYPE_DESCP", dr["CD_DESCP"]);
                ei.Put("ORDER_BY", dr["ORDER_BY"]);
            }
            else
            {
                return partytype + "：代码建档中无对应";
            }
            //sql = "SELECT * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(partyno);
            //dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                ei.Put("PARTY_NAME", dr["PARTY_NAME"]);
                ei.Put("MAIL", dr["PARTY_MAIL"]);
                ei.Put("PART_ADDR", dr["PART_ADDR1"]);
                ei.Put("PART_ADDR2", dr["PART_ADDR2"]);
                ei.Put("PART_ADDR3", dr["PART_ADDR3"]);
                ei.Put("PARTY_ATTN", dr["PARTY_ATTN"]);
                ei.Put("TEL", dr["PARTY_TEL"]);
                ei.Put("PARTY_NO", dr["PARTY_NO"]);
                ei.Put("CNTY", dr["CNTY"]);
                ei.Put("CNTY_NM", dr["CNTY_NM"]);
                ei.Put("CITY", dr["CITY"]);
                ei.Put("CITY_NM", dr["CITY_NM"]);
                ei.Put("STATE", dr["STATE"]);
                ei.Put("ZIP", dr["ZIP"]);

                ei.Put("PART_ADDR4", dr["PART_ADDR4"]);
                ei.Put("PART_ADDR5", dr["PART_ADDR5"]);
                ei.Put("PARTY_NAME2", dr["PARTY_NAME2"]);
                ei.Put("PARTY_NAME3", dr["PARTY_NAME3"]);
                ei.Put("PARTY_NAME4", dr["PARTY_NAME4"]);
                ei.Put("FAX_NO", dr["PARTY_FAX"]);
                ei.Put("TAX_NO", dr["TAX_NO"]);
            }
            else
            {
                return partyno + "：客户建档中无对应";
            }
            EditInstruct delei = new EditInstruct("SMDNPT", EditInstruct.DELETE_OPERATION);
            delei.PutKey("DN_NO", dnno);
            delei.PutKey("PARTY_TYPE", partytype);
            ml.Add(delei);
            ml.Add(ei);
            return msg;
        }


        public static string GetUserFxt(string userid)
        {
            string uextsql = string.Format("SELECT U_ID+' '+ U_EXT FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(userid));
            string uext = OperationUtils.GetValueAsString(uextsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return uext;
        }

        public static string InsetMlByDuplicate(MixedList ml, string partytype, string partyno, string shipmentid, string uid)
        {
            string sql = string.Format("SELECT COUNT(1) FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE={1}",
                SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(partytype));
            int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (count >= 1)
            {
                return string.Empty;
            }
            return InsetIntoMlBySMPTY(ml,partytype,partyno,shipmentid,uid,null);
            
        }

        public static ResultData CheckSMEtd(string shipmentid,DataTable dt=null,string checktype="")
        {
            string returnMsg = string.Empty;
            string trantype = string.Empty;
            if (dt == null)
            {
                string sql = string.Format("SELECT DATEDIFF(DAY,GETDATE(), ETD ) AS NO_OF_DAYS,ETD,CORDER,TRAN_TYPE FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            string etd = Prolink.Math.GetValueAsString(dt.Rows[0]["ETD"]);
            string corder = Prolink.Math.GetValueAsString(dt.Rows[0]["CORDER"]);
            if (string.IsNullOrEmpty(etd)) return ResultData.SucceedResult();
            if (!"C".Equals(corder) && !"Q".Equals(corder)) return ResultData.SucceedResult();
            int days = Prolink.Math.GetValueAsInt(dt.Rows[0]["NO_OF_DAYS"]);
            int defaultday = 3;
            trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            switch (trantype)
            {
                case "F":
                case "R":
                    defaultday = 1;
                    break;
                case "E":
                case "A":
                case "T":
                    if ("COD".Equals(checktype))
                        return ResultData.BeforThreeDResult();
                    return ResultData.SucceedResult();
                default:
                    defaultday = -1;
                    break;
            }
            if (days >= defaultday)
            {
                return ResultData.SucceedResult();
            }
            if (defaultday == 3)
            {
                return ResultData.BeforThreeDResult();
            }
            else{
                return ResultData.AfterOneDResult();
            }
        }

        public static string HandleTKBLStatus(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.OperationType = EditInstruct.INSERT_OPERATION;
            string shipmentid = ei.Get("SHIPMENT_ID");
            MixedList ml = (MixedList)parm["mixedlist"];

            if (string.IsNullOrEmpty(shipmentid)) 
                return BaseParser.ERROR;
            if (shipmentid.Contains("Special Note:")) 
                return BaseParser.END;

            ei.PutKey("SHIPMENT_ID", shipmentid);
            string sql = string.Format("SELECT U_ID,SHIPMENT_ID,(SELECT TOP 1 STATUS FROM SMSM WHERE SHIPMENT_ID=TKBL.SHIPMENT_ID)SM_STATUS,MASTER_NO,HOUSE_NO,COMBIN_SHIPMENT FROM TKBL WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) throw new Exception(shipmentid + " No Data！");
            
            if(string.IsNullOrEmpty(Prolink.Math.GetValueAsString(ei.Get("STS_CD"))))
                 throw new Exception(shipmentid + " StatusCode is null！");
            if(string.IsNullOrEmpty(Prolink.Math.GetValueAsString(ei.Get("EVEN_DATE"))))
                throw new Exception(shipmentid + " EventDate is null！");
            if(string.IsNullOrEmpty(Prolink.Math.GetValueAsString(ei.Get("LOCATION"))))
                throw new Exception(shipmentid + " Location is null！");

            string masterno=Prolink.Math.GetValueAsString(dt.Rows[0]["MASTER_NO"]);
            if(string.IsNullOrEmpty(masterno))
                masterno=Prolink.Math.GetValueAsString(dt.Rows[0]["HOUSE_NO"]);
            if(string.IsNullOrEmpty(masterno))
                throw new Exception(shipmentid + " Tracking's Master No is null！");

            string smsts=Prolink.Math.GetValueAsString(dt.Rows[0]["SM_STATUS"]);
            switch (smsts)
            {
                case "H":
                case "Y":
                case "U":
                case "O":
                case "F":
                case "R":
                    break;
                default:
                    throw new Exception(shipmentid + " Haven't left yet！");
            }
            ei.Put("MODIFY_BY", parm["UserId"].ToString());
            ei.PutDate("MODIFY_DATE", DateTime.Now);
            DateTime evendate=Prolink.Math.GetValueAsDateTime(ei.Get("EVEN_DATE"));
            ei.Put("EVEN_DATE", evendate.ToString("yyyyMMddHHmmssfff"));
            ei.Put("SEQ_NO", Guid.NewGuid().ToString());
            ei.Put("U_ID", Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]));
            ei.Put("REMARK", "Upload by " + parm["UserId"].ToString() + " Upload Date:" + DateTime.Now.ToString("yyyy-MM-ddTHH:MM:sszzz"));
            
            if (shipmentid.StartsWith("B")&& !shipmentid.StartsWith("BHB"))
            {
                sql = string.Format("SELECT SHIPMENT_ID,COMBIN_SHIPMENT FROM TKBL WHERE COMBIN_SHIPMENT={0}", SQLUtils.QuotedStr(shipmentid));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow datarow in dt.Rows)
                {
                    if (!shipmentid.Equals(datarow["SHIPMENT_ID"].ToString()))
                    {
                        ei.Put("SHIPMENT_ID", datarow["SHIPMENT_ID"].ToString());
                        ml.Add(ei);
                    }
                }
            }
            
            return string.Empty;
        }

        /// <summary>
        /// 批量改港操作，批量该港作业
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="ei"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        public static string HandleChangePod(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string shipmentid = ei.Get("SHIPMENT_ID");
            if (string.IsNullOrEmpty(shipmentid)) return BaseParser.ERROR;
            ei.PutKey("SHIPMENT_ID", shipmentid);
            string sql = string.Format("SELECT  DATEDIFF(DAY,GETDATE(), ETD ) AS NO_OF_DAYS,ETD,CORDER, U_ID,STATUS,TRAN_TYPE,BORDER,REGION,BOOKING_INFO,GROUP_ID,CMP,CARRIER,DN_NO,ISCOMBINE_BL,ATA FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            
            if (dt.Rows.Count <= 0) throw new Exception(shipmentid + "无订舱资料！");
            ResultData resultdata=CheckSMEtd(shipmentid, dt,"COD");
            if (resultdata.IsSucceed == true)
                throw new Exception(shipmentid + "可直接在订舱确认做更改确认");
            //string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            //if (!"B".Equals(status))
            //    throw new Exception(shipmentid + "非未处理状态，不允许导入Partytype");
            if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dt.Rows[0]["ATA"])))
                throw new Exception(shipmentid + " 已经有ATA不允许导入");
            string shipmentuid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
            ei.PutKey("U_ID", shipmentuid);
            string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string batchcode = ei.Get("BATCH_CODE");
            string[] batchcods = batchcode.Split('@');

            string msg = string.Empty;
            Func<string, string, string> SetPartyInfo = (partyno, partytype) =>
           {
               DataTable smptydt = OperationUtils.GetDataTable("SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO LIKE '%" + partyno + "%'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
               if (smptydt.Rows.Count <= 0)
                   return partyno + "客户建档中无对应客户资料; ";
               partyno = Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"].ToString());
               string message = InsetIntoMlSMPTYDt(ml, partytype, partyno, shipmentid, shipmentuid, smptydt);
               return message;
           };
            foreach (string partycode in batchcods)
            {
                if (string.IsNullOrEmpty(partycode))
                    continue;
                string parytype = partycode.Replace("：", ":").Split(':')[0];
                string partyno = partycode.Replace("：", ":").Split(':')[1];
                if ("SP".Equals(parytype) || "BO".Equals(parytype) || "CR".Equals(parytype) || "BR".Equals(parytype) || "BM".Equals(parytype) || "FS".Equals(parytype))
                    continue;
                //然后根据partyno写入到Ei中
                msg+=SetPartyInfo(partyno, parytype);
            }
            ei.Remove("BATCH_CODE");

            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception(shipmentid + ":" + msg);
            }
            string shipmentreason=Prolink.Math.GetValueAsString(ei.Get("CHANGE_REMARK"));
            Dictionary<string, string> dictionary = (Dictionary<string, string>)parm["reasonDic"];
            dictionary.Add(shipmentuid, shipmentreason);
            ei.Remove("CHANGE_REMARK");

            Func<string,string, string> RemoveEditField = (field,tran) =>
            {
                var cd = ei.Get(field);
                if (string.IsNullOrEmpty(cd))
                {
                    ei.Remove(field);
                    return string.Empty;
                }
                var name = GetPortNm(cd, tran);
                switch (field)
                {
                    case "PPOD_CD":
                        ei.Put("PPOD_NAME", name);
                        ei.Put("POD_CD", cd);
                        ei.Put("POD_NAME", name);
                        break;
                    case "PDEST_CD":
                        ei.Put("PDEST_NAME", name);
                        ei.Put("DEST_CD", cd);
                        ei.Put("DEST_NAME", name);
                        break;
                }
                return string.Empty;
            };
            RemoveEditField("PPOD_CD",trantype);
            RemoveEditField("PDEST_CD", trantype);
            return string.Empty;
        }


        public static string HandleDnInfoStatus(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            Dictionary<string, List<string>> dictionary = (Dictionary<string, List<string>>)parm["combineDictionary"];
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string dnno = ei.Get("DN_NO");
            
            if (string.IsNullOrEmpty(dnno)) return BaseParser.ERROR;
            ei.PutKey("DN_NO", dnno);
            //ei.Condition="":
            if(string.IsNullOrEmpty(ei.Get("HTML_IS_BOOKING").ToString()))
                throw new Exception(dnno+"是否订舱不能为空！");
            if (string.IsNullOrEmpty(ei.Get("HTML_IS_APPROVE").ToString()))
                throw new Exception(dnno + "是否签核不能为空！");
            if (string.IsNullOrEmpty(ei.Get("PRODUCT_DATE").ToString()))
                throw new Exception(dnno + "生产日期不能为空！");
            if (string.IsNullOrEmpty(ei.Get("ETD").ToString()))
                throw new Exception(dnno + "预计出货日不能为空！");

            if (!string.IsNullOrEmpty(ei.Get("ETD").ToString()))
                ei.PutDate("ETD", ei.Get("ETD").ToString());
            if (!string.IsNullOrEmpty(ei.Get("PRODUCT_DATE").ToString()))
                ei.PutDate("PRODUCT_DATE", ei.Get("PRODUCT_DATE").ToString());

            string sql = string.Format("SELECT U_ID,STATUS,APPROVE_TO,CMP,DN_NO,TRAN_TYPE FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) throw new Exception(dnno + "No Data！");
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string dnuid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
            ei.PutKey("U_ID", dnuid);
            string[] fields = ei.getNameSet();// ei.GetDateOffsetFields();
            List<string>unfiledlist =new List<string>();
            unfiledlist.Add("HTML_CAR_TYPE");
            unfiledlist.Add("HTML_TRACK_WAY");
            unfiledlist.Add("HTML_BATTERY");
            unfiledlist.Add("CNT20");
            unfiledlist.Add("CNT40");
            unfiledlist.Add("CNT40HQ");
            unfiledlist.Add("HTML_VIA");
            unfiledlist.Add("STATUS");
            unfiledlist.Add("APPROVE_TO");
            unfiledlist.Add("DN_TYPE");
            unfiledlist.Add("TRAN_TYPE_DESCP");
            unfiledlist.Add("POL");
            unfiledlist.Add("POD");
            unfiledlist.Add("SC_CODE");
            unfiledlist.Add("SPEC_PROCID");
            foreach (string field in fields)
            {
                if (unfiledlist.Contains(field))
                {
                    ei.Remove(field);
                    continue;
                }
                if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(ei.Get(field))))
                    ei.Remove(field);
            }
            string combineinfo = Prolink.Math.GetValueAsString(ei.Get("COMBINE_INFO"));
            if (!string.IsNullOrEmpty(combineinfo))
            {
                if (!dictionary.Keys.Contains(combineinfo))
                {
                    List<string> dnnos = new List<string>();
                    dnnos.Add(dnno);
                    dictionary.Add(combineinfo, dnnos);
                }
                else
                {
                    if (!dictionary[combineinfo].Contains(dnno))
                        dictionary[combineinfo].Add(dnno);
                }
                ei.Remove("COMBINE_INFO");
            }
            string[] partytypes = new string[] { "FS", "SP", "CR" };
            Func<string, string, string> SetPartyInfo = (partyno, partytype) =>
            {
                DataTable smptydt = OperationUtils.GetDataTable("SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO LIKE '%" + partyno + "%'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smptydt.Rows.Count <= 0)
                    return partyno + "客户建档中无对应客户资料; ";
                partyno = Prolink.Math.GetValueAsString(smptydt.Rows[0]["PARTY_NO"].ToString());
                string message = InsertIntoSMDNPT(ml, partytype, partyno, dnno, dnuid, smptydt);
                return message;
            };
            foreach (string partytype in partytypes)
            {
                string partyno = Prolink.Math.GetValueAsString(ei.Get(partytype));
                ei.Remove(partytype);
                if (string.IsNullOrEmpty(partyno))
                    continue;
                SetPartyInfo(partyno, partytype);
            }
            return string.Empty;
        }

    }

    public class SmptyInfo
    {
        public string ShipmentId { get; set; }
        public string PartyType { get; set; }
        public string TypeDescp { get; set; }
        public string OrderBy { get; set; }
        public string PartyNo { get; set; }
        public string PartyName { get; set; }
        public string PartAddr1 { get; set; }
        public string PartAddr2 { get; set; }
        public string PartAddr3 { get; set; }
        public string Cnty { get; set; }
        public string CntyNm { get; set; }
        public string City { get; set; }
        public string CityNm { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string PartyAttn { get; set; }
        public string PartyTel { get; set; }
        public string PartyMail { get; set; }
        public string DebitTo { get; set; }
    }

    public class ResultData
    {
        public bool IsSucceed { get; set; }
        public string ResultCode { get; set; }
        public string Description { get; set; }

        public static Func<ResultData> SucceedResult = () => { return new ResultData { IsSucceed = true, ResultCode = "Succeed", Description = "Successfully!" }; };
        public static Func<ResultData> BeforThreeDResult = () => { return new ResultData { ResultCode = "Three", Description = "距ETD日期3日内，不允许修改订舱资料!" }; };
        public static Func<ResultData> AfterOneDResult = () => { return new ResultData { ResultCode = "BeforeOne", Description = "超过ETD日期1日后，不允许修改订舱资料!" }; };
    }
}
