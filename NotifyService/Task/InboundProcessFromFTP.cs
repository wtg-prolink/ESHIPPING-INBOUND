using Business.Import;
using Business.TPV.Financial;
using Business.TPV.Utils;
using Business.Utils;
using Models.InboundProcess;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using TrackingEDI.Business;

namespace NotifyService.Task
{
    public class InboundProcessFromFTP : FtpImportBase, IPlanTask
    {
        string Location = "";
        string FileName = "InboundProcess";
        private const string _CreateBy = "System AUTO";


        protected string ConfigNodeName
        {
            get
            {
                return "Receive";
            }
        }
        protected string ConfigFileName
        {
            get
            {
                return System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, string.Format("edi/ftp/{0}.xml", FileName));
            }
        }
        protected override Business.Utils.FTPConfig GetFtpConfig()
        { 
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigFileName);
            XmlNodeList nodeList = doc.GetElementsByTagName(ConfigNodeName);
            return Business.Utils.ConfigManager.GetFTPConfig(nodeList, c =>
            {
                if (!string.IsNullOrEmpty(Location))
                {
                    c.Path = string.Format(c.Path, Location);
                    c.LogPath = string.Format(c.LogPath, Location);
                    c.BackupPath = string.Format(c.BackupPath, Location);
                    c.DownloadPath = string.Format(c.DownloadPath, Location);
                    c.FtpBackupPath = string.Format(c.FtpBackupPath, Location);
                }
                c.SearchDir = "Y";
            }).First();
        }

        public void Run(IPlanTaskMessenger messenger)
        {
            this.AnalyzeFile(true);
        }

        protected override bool OperateFile(FtpImportEvertArgs e)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(e.LocalFileName);

            string[] fileList = fileName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (fileList.Length <= 1)
            {
                throw new Exception(string.Format("name error"));
            }
            string type = fileList[1];
            if (!type.Equals("AIP"))
            {
                throw new Exception(string.Format("type error"));
            }

            string remark = string.Empty;
            string shipmentId = string.Empty;
            string cntrNo = string.Empty;
            string cmp = string.Empty;
            string tranType = string.Empty;
            DataTable dt = ExcelHelper.ImportExcelToDataTable(e.LocalFileName, 1);
            MixedList ml = new MixedList();

            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    BookingAgent agent = SetBookingAgent(dr);
                    InboundBroker broker = SetinboundBroker(dr);
                    TruckCompany truck = SetTruckCompany(dr);
                    SMSMIModel smsmiModel = SetSmsmiModel(dr);

                    DataTable smdt = GetSM(smsmiModel.MASTER_NO, smsmiModel.TRAN_TYPE);
                    if (smdt.Rows.Count <= 0) continue;
                    DataRow row = smdt.Rows[0];

                    cmp = Prolink.Math.GetValueAsString(row["CMP"]);
                    shipmentId = Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]);
                    string smsmiUid = Prolink.Math.GetValueAsString(row["U_ID"]);

                    smsmiModel.SHIPMENT_ID = shipmentId;
                    smsmiModel.CMP = cmp;
                    smsmiModel.U_ID = smsmiUid;
                    cntrNo = smsmiModel.CNTR_NO;
                    tranType = smsmiModel.TRAN_TYPE;

                    bool uploadFalg = UploadEdoc(row, e.LocalFileName, cmp, shipmentId);
                    if (!uploadFalg) continue;
                    InboundConfirmHelper helper = new InboundConfirmHelper();
                    InboundConfirmHelper.UserInfo userinfo = new InboundConfirmHelper.UserInfo
                    {
                        UserId = _CreateBy,
                        CompanyId = cmp,
                        GroupId = "TPV",
                        Upri = "G",
                        Dep = "GLST",
                        IoFlag = "O"
                    };
                    remark = helper.NotifyLspCheck(smdt); 
                    if (!string.IsNullOrEmpty(remark))
                    {
                        InsertEdiLog(remark, shipmentId, cmp);
                        continue;
                    }

                    //Booking Agent Confirm
                    if (agent.DISCHARGE_DATE != null)
                    {
                        remark = helper.BookingAgentConfirm(smsmiModel, agent, userinfo);
                        InsertEdiLog(remark, shipmentId, cmp);
                    }
                    //C.C. Confirm
                    if (!string.IsNullOrEmpty(broker.DEC_NO) && !string.IsNullOrEmpty(broker.IMPORT_NO) && broker.DEC_DATE != null)
                    {
                        remark = helper.CCConfirm(smsmiModel, broker, userinfo);
                        InsertEdiLog(remark, shipmentId, cmp);
                    }
                    DataTable orddt = GetSmord(shipmentId, tranType, cntrNo);
                    DataTable cdt = GetSmrcntr(shipmentId, tranType, cntrNo);


                    if (orddt.Rows.Count <= 0) continue;
                    string WsCd = Prolink.Math.GetValueAsString(orddt.Rows[0]["WS_CD"]);

                    List<string> EtaMsl = new List<string>();
                    List<string> idList = new List<string>();
                    List<string> newReserveNo = new List<string>();

                    if (truck.ARRIVAL_DATE != null && truck.USE_DATE != null)
                    {
                        string pickUpDate = Prolink.Math.GetValueAsDateTime(truck.USE_DATE).ToString("yyyy-MM-dd");
                        string arrivalDate = Prolink.Math.GetValueAsDateTime(truck.ARRIVAL_DATE).ToString("yyyy-MM-dd");
                        string status = Prolink.Math.GetValueAsString(orddt.Rows[0]["CSTATUS"]);
                        if (status == "Y")
                        {
                            Dictionary<string, bool> shipmentDic = new Dictionary<string, bool>();
                            switch (tranType)
                            {
                                case "F":
                                case "R":
                                    TrackingEDI.InboundBusiness.IBResultInfo result= TrackingEDI.InboundBusiness.SMSMIHelper.InboundFclOrderTrucker(orddt.Rows[0], cdt.Rows[0], pickUpDate, EtaMsl, arrivalDate, WsCd, _CreateBy, "", "", idList, newReserveNo);
                                    if (result.IsSucceed)
                                        remark = "Order Truck By Container Success!";
                                    else
                                        remark = "Order Truck By Container Error:" + result.Description;
                                    InsertEdiLog(remark, shipmentId, cmp);
                                    break;
                                case "L":
                                case "A":
                                case "E":
                                    DataTable rdnDt = GetSmrdn(shipmentId);
                                    if (rdnDt.Rows.Count > 0)
                                    {
                                        string msg = helper.InboundDnOrderTrucker(truck, orddt.Rows[0], rdnDt.Rows[0], cdt.Rows[0]["CNTR_TYPE"].ToString(), EtaMsl, userinfo, shipmentDic, idList);
                                        if (string.IsNullOrEmpty(msg))
                                            remark = "Order Truck By Dn Success!";
                                        else
                                            remark = "Order Truck By Dn Error:" + msg;
                                        InsertEdiLog(remark, shipmentId, cmp);
                                    } 
                                    break; 
                            }
                        } 
                    }

                    DataTable smirvDt = GetSmirv(shipmentId, tranType, cntrNo);
                    if (smirvDt.Rows.Count <= 0) continue;
                    string Status = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["STATUS"]);
                    string reserve_no = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["RESERVE_NO"]);
                    //SlotTime Booked
                    if (truck.ARRIVAL_DATE != null && truck.USE_DATE != null)
                    {
                        remark = "SlotTime Booked Success!";
                        MixedList list = new MixedList();
                        DateTime useDate = Prolink.Math.GetValueAsDateTime(truck.USE_DATE);
                        try
                        {
                            TrackingEDI.InboundBusiness.InboundHelper.UpdateDetDueDate(reserve_no, useDate, list);
                            helper.SoltTimeBooked(smsmiModel, truck, smirvDt.Rows[0], userinfo, list);
                            if (list.Count > 0)
                                OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex1)
                        {
                            remark = "SlotTime Booked Error:" + ex1.ToString();
                        }
                        InsertEdiLog(remark, shipmentId, cmp);
                    }
                    //Gate In Confirm
                    if (truck.IN_DATE_L != null)
                    {
                        smirvDt = GetSmirv(shipmentId, tranType, cntrNo);
                        if (smirvDt.Rows.Count <= 0) continue;
                        Status = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["STATUS"]);

                        remark = "Gate In Confirm Success!";
                        if (Status == "P" || Status == "O" || (Status != "R" && Status != "C" && Status != "E" && Status != "A"))
                        {
                        }
                        else
                        {
                            string msg = helper.GateInConfirm(smirvDt, truck);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                remark = "Gate In Confirm Error:" + msg;
                            }
                            InsertEdiLog(remark, shipmentId, cmp);
                        }

                    }
                    //POD Confirm
                    if (truck.POD_UPDATE_DATE != null)
                    {
                        smirvDt = GetSmirv(shipmentId, tranType, cntrNo);
                        if (smirvDt.Rows.Count <= 0) continue;
                        remark = "POD Confirm Success!";
                        string msg = helper.BatchUploadPOD(truck, cmp, smirvDt);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            remark = "POD Confirm Error:" + msg;
                        }
                        InsertEdiLog(remark, shipmentId, cmp);
                    }
                    //Gate Out Confirm
                    if (truck.OUT_DATE_L != null)
                    {
                        remark = "Gate Out Confirm Success!";

                        smirvDt = GetSmirv(shipmentId, tranType, cntrNo);
                        if (smirvDt.Rows.Count <= 0) continue;
                        Status = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["STATUS"]);

                        if (Status == "O" || Status == "R" || Status == "C" || Status == "E")
                        { 
                        }
                        else
                        {
                            string msg = helper.GateOutConfirm(smirvDt, truck);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                remark = "Gate Out Confirm Error:" + msg;
                            }
                            InsertEdiLog(remark, shipmentId, cmp);
                        }
                       
                    }
                    //Empty Return Confirm
                    if (truck.EMPTY_TIME != null)
                    {
                        remark = "Empty Return Confirm Success!";
                        truck.RESERVE_NO = reserve_no;
                        string msg = helper.EmptyReturnConfirm(smsmiModel, truck);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            remark = "Empty Return Confirm Error:" + msg;
                        }
                        InsertEdiLog(remark, shipmentId, cmp);
                    }
                }
                catch (Exception ex2)
                {
                    InsertEdiLog(ex2.ToString(), shipmentId, cmp);
                    return false;
                }
            }
            return true;
        }

        public bool UploadEdoc(DataRow row, string filePath, string cmp, string refNo)
        {
            string remark = "Upload Edoc Success";
            EDocInfo info = Business.TPV.Helper.CreateShipmentEDocInfo(row);
            info.DocType = "ISC";
            info.FilePath = filePath;
            info.Remark = "FTP";
            try
            {
                EDOCApi.EDOCResultUploadFile uploadResult = EDocHelper.UploadFile2EDOC(info);
                if (uploadResult == null || uploadResult.Status != EDOCApi.DBErrors.DB_SUCCESS)
                {
                    remark = "Upload Edoc Error:" + uploadResult.Status;
                    return false;
                }
            }
            catch (Exception ex)
            {
                remark = "Upload Edoc Error:" + ex.ToString();
                return false;
            }
            InsertEdiLog(remark, refNo, cmp);
            return true;
        }
        public BookingAgent SetBookingAgent(DataRow dr)
        {
            BookingAgent agent = new BookingAgent();
            agent.ATA = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["ATA"]));
            agent.BACK_LOCATION = Prolink.Math.GetValueAsString(dr["Empty_Return_Location"]).Trim();
            agent.PIN_NO = Prolink.Math.GetValueAsString(dr["PIN No."]).Trim();
            agent.DISCHARGE_DATE = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["Discharge Time"]));
            agent.InboundTerminalAgent = Prolink.Math.GetValueAsString(dr["Inbound Terminal Agent"]).Trim();
            return agent;
        }

        public InboundBroker SetinboundBroker(DataRow dr)
        {
            InboundBroker broker = new InboundBroker();
            broker.DEC_NO = Prolink.Math.GetValueAsString(dr["DECL NO"]).Trim();
            broker.IMPORT_NO = Prolink.Math.GetValueAsString(dr["Import No."]).Trim();
            broker.DEC_DATE = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["CLEARANCE DATE"]));
            broker.REL_DATE = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["RELEASE DATE"]));
            broker.INSPECTION = Prolink.Math.GetValueAsString(dr["Inspection(Y/N)"]).Trim();
            broker.CER_NO = Prolink.Math.GetValueAsString(dr["Certificate No."]).Trim();
            broker.DEC_REPLY = Prolink.Math.GetValueAsString(dr["Declaration Reply"]).Trim();
            broker.ICDF = Prolink.Math.GetValueAsString(dr["Faster Solution(Y/N)"]).Trim();
            broker.CC_CHANNEL = Prolink.Math.GetValueAsString(dr["Cc Channel(GREEN/YELLOW/GRAY/RED)"]).Trim();
            broker.HS_QTY = Prolink.Math.GetValueAsString(dr["QTY/HS Code"]).Trim();
            broker.CNTRY_CD = Prolink.Math.GetValueAsString(dr["Country"]).Trim();
            broker.PLI = Prolink.Math.GetValueAsString(dr["PLI#"]).Trim();
            broker.LI = Prolink.Math.GetValueAsString(dr["LI#"]).Trim();
            broker.SUF_COST = Prolink.Math.GetValueAsString(dr["SUFRAMA"]).Trim();
            broker.CC_RATE = Prolink.Math.GetValueAsString(dr["Declaration Exchange Rate"]).Trim();
            broker.ADD_QTY = Prolink.Math.GetValueAsString(dr["Additions Qty"]).Trim();
            broker.SIS_FEE = Prolink.Math.GetValueAsString(dr["SISCOMEX Fee"]).Trim();
            return broker;
        }

        public TruckCompany SetTruckCompany(DataRow dr)
        {
            TruckCompany truck = new TruckCompany();
            truck.ARRIVAL_DATE = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["Arrival Date"]));
            truck.USE_DATE = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["Pickup Date"]));
            truck.IN_DATE_L = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["Gate In Time"]));
            truck.POD_UPDATE_DATE = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["POD Time"]));
            truck.OUT_DATE_L = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["Gate Out Time"]));
            truck.TRUCK_NO = Prolink.Math.GetValueAsString(dr["Gate in truck No"]).Trim();
            truck.DRIVER = Prolink.Math.GetValueAsString(dr["Gate in Driver"]).Trim();
            truck.DRIVER_ID = Prolink.Math.GetValueAsString(dr["Gate in Driver ID No"]).Trim();
            truck.TEL = Prolink.Math.GetValueAsString(dr["Gate in Driver Mobile"]).Trim();
            truck.LTRUCK_NO = Prolink.Math.GetValueAsString(dr["Gate out truck No"]).Trim();
            truck.LDRIVER = Prolink.Math.GetValueAsString(dr["Gate out Driver"]).Trim();
            truck.LDRIVER_ID = Prolink.Math.GetValueAsString(dr["Gate out Driver ID No"]).Trim();
            truck.LTEL = Prolink.Math.GetValueAsString(dr["Gate out Driver Mobile"]).Trim();
            truck.HEAVY_PICKUP_TIME = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["LFD Heavy Pickup"]));
            truck.EMPTY_RETURN_TIME = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["LFD Empty Return"]));
            truck.AT_YARD_TIME = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["LFD At Yard"]));
            truck.BACK_LOCATION = Prolink.Math.GetValueAsString(dr["Empty Return Location"]).Trim();
            truck.EMPTY_TIME = SetDateTime(Prolink.Math.GetValueAsDateTime(dr["Empty Return Date"]));
            return truck;
        }

        public SMSMIModel SetSmsmiModel(DataRow dr)
        {
            SMSMIModel smsmi = new SMSMIModel();
            smsmi.TRAN_TYPE = SetTranType(dr["Trans Type"]);
            smsmi.MASTER_NO = Prolink.Math.GetValueAsString(dr["BL NO"]).Trim();
            smsmi.CNTR_NO = Prolink.Math.GetValueAsString(dr["Container No"]).Trim();
            return smsmi;
        }

        public string SetTranType(object tranType)
        {
            string type = Prolink.Math.GetValueAsString(tranType).Trim().ToUpper();
            switch (type)
            {
                case "FCL":
                    type = "F";
                    break;
                case "LCL":
                    type = "L";
                    break;
                case "AIR":
                    type = "A";
                    break;
                case "EXPRESS":
                    type = "E";
                    break;
                case "RAILWAY":
                    type = "R";
                    break;
            }
            return type;
        }

        public static DataTable GetSmrcntr(string shipmentId, string tranType, string cntrNo)
        {
            string sql;
            if (tranType == "F" || tranType == "R")
                sql = $"SELECT * FROM SMRCNTR WHERE SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)} AND CNTR_NO={SQLUtils.QuotedStr(cntrNo)}";
            else
                sql = $"SELECT * FROM SMRCNTR WHERE SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)}";
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }
        public static DataTable GetSmrdn(string shipmentId)
        {
            string sql = $"SELECT * FROM SMRDN WHERE SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)}";
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }


        public static DataTable GetSmord(string shipmentId, string tranType, string cntrNo)
        {
            string sql;
            if (tranType == "F" || tranType == "R")
                sql = $"SELECT * FROM SMORD WHERE SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)} AND CNTR_NO={SQLUtils.QuotedStr(cntrNo)}";
            else
                sql = $"SELECT * FROM SMORD WHERE SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)} ";
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public static DataTable GetSM(string blNo, string tranType)
        {
            string sql = $"SELECT * FROM SMSMI WHERE MASTER_NO={SQLUtils.QuotedStr(blNo)} AND TRAN_TYPE={SQLUtils.QuotedStr(tranType)}";
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }
        public static DataTable GetSmirv(string shipmentId, string tranType, string cntrNo)
        {
            string sql = "";
            if (tranType == "F"|| tranType == "R")
                sql = $"SELECT * FROM SMIRV WHERE SHIPMENT_INFO={SQLUtils.QuotedStr(shipmentId)} AND CNTR_NO={SQLUtils.QuotedStr(cntrNo)}";
            else
                sql = $"SELECT * FROM SMIRV WHERE SHIPMENT_INFO={SQLUtils.QuotedStr(shipmentId)}";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public DateTime? SetDateTime(DateTime time)
        {
            if (time == DateTime.MinValue)
                return null;
            return time;
        }

        public void InsertEdiLog(string remark, string refNo, string cmp)
        {
            EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("EDI_ID", "InboundProcess");
            ei.PutExpress("EVENT_DATE", "getdate()");
            ei.Put("SENDER", _CreateBy);
            ei.Put("RS", "Receive");
            if (remark.Length > 500)
                remark = remark.Substring(0, 500);
            ei.Put("REMARK", remark);
            ei.Put("STATUS", remark.Contains("Success") ? "Succeed" : "Exception");
            ei.Put("FROM_CD", "FTP");
            ei.Put("TO_CD", "ESHIPPING");
            ei.Put("DATA_FOLDER", "");
            ei.Put("REF_NO", refNo);
            ei.Put("GROUP_ID", "TPV");
            ei.Put("CMP", cmp);
            ei.Put("STN", "*");
            OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
    }
}
