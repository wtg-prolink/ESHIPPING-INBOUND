using Business;
using Business.TPV.Financial;
using EDOCApi;
using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TrackingEDI;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;
using TrackingEDI.Mail;
using TrackingEDI.Model;
using WebGui.App_Start;
using System.Net;
using static TrackingEDI.InboundBusiness.InboundHelper;

namespace WebGui.Controllers
{
    public class SMSMIController : BaseController
    {
        string bookingCol = @"*,DATEDIFF(DAY, ETD_L, ETA_L) AS ELT_L,DATEDIFF(DAY, ATD, ATA) AS ALT_L,DATEDIFF(DAY, ETA_L, ETA) AS EDD_L,DATEDIFF(DAY, ETA_L, ATA) AS ADD_L,CASE WHEN DATEDIFF(DAY, ETA_L, ATA)>0 THEN 1 WHEN DATEDIFF(DAY, ETA_L, ATA)<=0 THEN 0 WHEN DATEDIFF(DAY, ETA_L, ETA)>0 THEN 1 WHEN DATEDIFF(DAY, ETA_L, ETA) <=0 THEN 0 ELSE NULL END AS LTS,(SELECT TOP 1 PARTY_NAME3 FROM SMSMIPT WHERE SHIPMENT_ID=SMSMI.SHIPMENT_ID AND PARTY_TYPE='SP') AS PARTY_NAME3,(SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO=(SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SHIPMENT_ID=SMSMI.SHIPMENT_ID AND PARTY_TYPE='SP') AND STATUS='U') AS HEAD_OFFICE";
        TrackingEDI.Mail.MailSender Mail = null;
        // GET: /SMSMI/
        #region 订舱View
        public ActionResult BookingQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK010");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            ViewBag.ISFUrl = WebConfigurationManager.AppSettings["ISF_URL"];
            string con = GetBaseCmp();
            string cmp = CompanyId;
            if (IOFlag == "O")
            {
                con = GetBaseCompany();
                cmp = BaseCompanyId;
            }
            ViewBag.ISFAcct = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFACCT' AND " + con)[1];
            ViewBag.ISFKey = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFKey' AND " + con)[1];
            ViewBag.ISFPWD = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFPWD' AND " + con)[1];
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='ISFS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowISFSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='EALS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowEAlertSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD");
            return View();
        }
        public ActionResult FclBookingQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK020");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            ViewBag.ISFUrl = WebConfigurationManager.AppSettings["ISF_URL"];
            string cmp = CompanyId;
            string con = GetBaseCmp();
            if (IOFlag == "O")
            {
                con = GetBaseCompany();
                cmp = BaseCompanyId;
            }
            ViewBag.ISFAcct = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFACCT' AND " + con)[1];
            ViewBag.ISFKey = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFKey' AND " + con)[1];
            ViewBag.ISFPWD = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFPWD' AND " + con)[1];
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='ISFS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowISFSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='EALS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowEAlertSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "F");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "F");
            return View();
        }

        public ActionResult ModifyBookingSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK080");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            string shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string Cmp = getOneValueAsStringFromSql("SELECT CMP FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            int n = 0;
            DataTable smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            if (smicntr.Rows.Count > 0)
            {
                foreach (DataRow dr in smicntr.Rows)
                {
                    string CntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                    sql = "SELECT COUNT(*) FROM SMICNTR WHERE CNTR_NO={0} AND CMP={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(CntrNo), SQLUtils.QuotedStr(Cmp));
                    n = getOneValueAsIntFromSql(sql);
                }
            }
            ViewBag.CntrN = n;
            ViewBag.HasRv = "false";
            sql = "SELECT RESERVE_NO FROM SMIRV WHERE STATUS<>'V' AND (SHIPMENT_INFO LIKE '%" + shipmentid + "%' OR SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid) + ")";
            DataTable dt = getDataTableFromSql(sql);
            if (dt.Rows.Count > 0)
            {
                ViewBag.HasRv = "true";
            }
            ViewBag.TRAN_TYPE = getOneValueAsStringFromSql("SELECT TRAN_TYPE FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            SetPTDnlist(id);
            return View();
        }

        public ActionResult FclBookingSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK020");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            string shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string Cmp = getOneValueAsStringFromSql("SELECT CMP FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            int n = 0;
            DataTable smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            if (smicntr.Rows.Count > 0)
            {
                foreach (DataRow dr in smicntr.Rows)
                {
                    string CntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                    sql = "SELECT COUNT(*) FROM SMICNTR WHERE CNTR_NO={0} AND CMP={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(CntrNo), SQLUtils.QuotedStr(Cmp));
                    n = getOneValueAsIntFromSql(sql);
                }
            }
            ViewBag.CntrN = n;
            ViewBag.PTlist = getPartyType(shipmentid);
            ViewBag.selects = TKBLController.GetSelectsToString("Fcl");
            return View();
        }

        public ActionResult LclBookingQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK030");
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.ISFUrl = WebConfigurationManager.AppSettings["ISF_URL"];
            ViewBag.ISFAcct = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFACCT' AND " + GetBaseCmp())[1];
            ViewBag.ISFKey = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFKey' AND " + GetBaseCmp())[1];
            ViewBag.ISFPWD = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFPWD' AND " + GetBaseCmp())[1];
            string cmp = CompanyId;
            if (IOFlag == "O")
            {
                cmp = BaseCompanyId;
            }
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='ISFS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowISFSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='EALS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowEAlertSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return View();
        }

        public ActionResult LclBookingSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK030");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            string shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            ViewBag.PTlist = getPartyType(shipmentid);
            return View();
        }

        public ActionResult AirBookingQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK040");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            string cmp = CompanyId;
            if (IOFlag == "O")
            {
                cmp = BaseCompanyId;
            }
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='EALS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowEAlertSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "A");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "A");
            return View();
        }

        public ActionResult AirBookingSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK040");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            SetPTDnlist(id);
            ViewBag.selects = TKBLController.GetSelectsToString("Air");
            return View();
        }

        public void SetPTDnlist(string id)
        {
            string sql = string.Format(@"SELECT SHIPMENT_ID,COMBINE_INFO FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(id));
            string shipmentid = "", combineInfo = "";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                combineInfo = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBINE_INFO"]);
            }
            ViewBag.PTlist = getPartyType(shipmentid);
            ViewBag.DnList = CommonHelp.getDnNoList(combineInfo);
        }

        public ActionResult ExBookingQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK050");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            string cmp = CompanyId;
            if (IOFlag == "O")
            {
                cmp = BaseCompanyId;
            }
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='EALS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowEAlertSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return View();
        }

        public ActionResult ExBookingSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK050");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C")); 
            SetPTDnlist(id);
            return View();
        }
         
        public ActionResult RailwayBookingQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK070");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            string cmp = CompanyId;
            if (IOFlag == "O")
            {
                cmp = BaseCompanyId;
            }
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='EALS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowEAlertSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "R");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "R");
            return View();
        }

        public ActionResult RailwayBookingSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK070");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            string shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            ViewBag.selects = TKBLController.GetSelectsToString("Railway");
            ViewBag.PTlist = getPartyType(shipmentid);
            return View();
        }

        public ActionResult CustomsQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK060");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult CustomsSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK060");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            ViewBag.TRAN_TYPE = getOneValueAsStringFromSql("SELECT TRAN_TYPE FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            SetPTDnlist(id);
            return View();
        }

        public ActionResult DTBookingQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BK080");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            string cmp = CompanyId;
            if (IOFlag == "O")
            {
                cmp = BaseCompanyId;
            }
            string sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD={0} AND CD_TYPE='EALS' ", SQLUtils.QuotedStr(cmp));
            ViewBag.SHowEAlertSending = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "R");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "R");
            return View();
        }

        public ActionResult DTBookingSetup(string id = null, string uid = null)
        {
            //string is_ok = TrackingEDI.InboundBusiness.InboundHelper.O2IFunc("BHB2110012102","E");


            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BK080");
            ViewBag.selects = TKBLController.GetSelectsToString("DTBooking", Request["location"], this.GroupId, this.BaseCompanyId, Request["dn"], Request["code"]);
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.shipToParty = uid == null ? "" : CommonHelp.getShipToParty(uid);
            return View();
        }

        #endregion

        #region 订舱确认View
        public ActionResult FclBookingConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC020");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "F");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "F");
            return View();
        }

        public ActionResult FclBookingConfirmSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC020");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            string shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string Cmp = getOneValueAsStringFromSql("SELECT CMP FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            int n = 0;
            DataTable smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            if (smicntr.Rows.Count > 0)
            {
                foreach (DataRow dr in smicntr.Rows)
                {
                    string CntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                    sql = "SELECT COUNT(*) FROM SMICNTR WHERE CNTR_NO={0} AND CMP={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(CntrNo), SQLUtils.QuotedStr(Cmp));
                    n = getOneValueAsIntFromSql(sql);
                }
            }
            ViewBag.CntrN = n;
            ViewBag.selects = TKBLController.GetSelectsToString("Fcl");
            SetPTDnlist(id);
            return View();
        }

        public ActionResult LclBookingConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC030");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult LclBookingConfirmSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC030");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            // ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult AirBookingConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC040");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "A");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "A");
            return View();
        }

        public ActionResult AirBookingConfirmSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC040");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            ViewBag.selects = TKBLController.GetSelectsToString("Air");
            SetPTDnlist(id);
            return View();
        }

        public ActionResult ExBookingConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC050");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult ExBookingConfirmSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC050");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            SetPTDnlist(id);
            return View();
        }

        public ActionResult RailwayBookingConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC070");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "R");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "R");
            return View();
        }

        public ActionResult RailwayBookingConfirmSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC070");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            ViewBag.selects = TKBLController.GetSelectsToString("Railway");
            return View();
        }

        public ActionResult DTBookingConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC080");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.DelayReason = CommonHelp.GetBscodeSelect("DELY", "CD_DESCP", "T");
            ViewBag.DelaySolution = CommonHelp.GetBscodeSelect("DELY", "AR_CD", "T");
            return View();
        }

        public ActionResult DTBookingConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC080");
            ViewBag.selects = TKBLController.GetSelectsToString("DTBooking", Request["location"], this.GroupId, this.BaseCompanyId, Request["dn"], Request["code"]);
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult CustomsConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC060");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult TransloadConfirmQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BKC090");
            return View();
        }

        public ActionResult TransloadSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC090");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            string shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            ViewBag.PTlist = getPartyType(shipmentid);
            return View();
        }

        public ActionResult CustomsConfirmSetupView(string id = null, string uid = null)
        {
            SetSchema("SMSMI");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BKC060");
            ViewBag.TranType = CommonHelp.getBscodeForSelect("TNT", GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForSelect("PRI", GetDataPmsCondition("C"));
            ViewBag.ExtraSrv = CommonHelp.getBscodeForCheckbox("SRV", GetDataPmsCondition("C"), "ExtraSrv");
            ViewBag.Ccchannel = CommonHelp.getBscodeForColModel("CCCH", GetDataPmsCondition("C"));
            string shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string Cmp = getOneValueAsStringFromSql("SELECT CMP FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            int n = 0;
            DataTable smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            if (smicntr.Rows.Count > 0)
            {
                foreach (DataRow dr in smicntr.Rows)
                {
                    string CntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                    sql = "SELECT COUNT(*) FROM SMICNTR WHERE CNTR_NO={0} AND CMP={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(CntrNo), SQLUtils.QuotedStr(Cmp));
                    n = getOneValueAsIntFromSql(sql);
                }
            }
            ViewBag.CntrN = n;
            ViewBag.TRAN_TYPE = getOneValueAsStringFromSql("SELECT TRAN_TYPE FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(id));
            return View();
        }
        #endregion

        #region 各运输方式纯查询
        public ActionResult AllQueryData()
        {
            
            string table = @"(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT "+ bookingCol + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            return BKTable(table, "");
        }

        public ActionResult FclQueryData()
        {
            string table = @"(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT " + bookingCol + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            return BKTable(table, "F");
        }

        public ActionResult LclQueryData()
        {
            string table = @"(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT " + bookingCol + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            return BKTable(table, "L");
        }

        public ActionResult AirQueryData()
        {
            string table = @"(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT " + bookingCol + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            return BKTable(table, "A");

        }

        private string SMSMIQueryVirCondition(string condition)
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            if (virCondition != "")
            {
                string[] subcolumn = Regex.Split(virCondition, "P_L_A_C_E_", RegexOptions.IgnoreCase);
                foreach (string column in subcolumn)
                {
                    if (column.Contains("SMIDN_"))
                    {
                        string sminfiled = column.Replace("SMIDN_", "").Replace("AND", "");
                        condition += " AND SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMIDN WHERE 1=1 AND " + sminfiled + ")";
                    }
                    if (column.Contains("SMIDNP_"))
                    {
                        string smidnpfiled = column.Replace("SMIDNP_", "").Replace("AND", "");
                        condition += " AND SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMIDNP WHERE 1=1 AND " + smidnpfiled + ")";
                    }
                    else if (column.Contains("SMICNTR_"))
                    {
                        string temp = column.ToUpper();
                        if (temp.Contains(" IN ("))
                        {
                            string smicntrfiled = column.Replace("SMICNTR_", "").Replace("AND", "");
                            condition += " AND SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMICNTR WHERE 1=1 AND " + smicntrfiled + ")";
                        }
                        else
                        {
                            condition += " AND " + column.Replace("SMICNTR_CNTR_NO", "CNTR_INFO");
                        }
                    }
                    else if (column.Contains("SMIDNICNTR_"))
                    {
                        string smidnicntrfield = column.Replace("SMIDNICNTR_", "").Replace("AND", "");
                        condition += " AND (SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMIDN WHERE 1=1 AND " + smidnicntrfield + ")";
                        condition += " OR SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMICNTR WHERE 1=1 AND " + smidnicntrfield + "))";
                    }
                }
            }
            return condition;
        }

        public ActionResult ExQueryData()
        {
            string table = "(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            return BKTable(table, "E");
        }

        public ActionResult RailwayQueryData()
        {
            string table = @"(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT " + bookingCol + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            return BKTable(table, "R");
        }

        public ActionResult DTQueryData()
        {
            string table = @"(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT " + bookingCol + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            return BKTable(table, "T");
        }

        public ActionResult CustomsQueryData()
        {
            //string table = "(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBBR', 'IBTC'))) S";
            //table += " UNION ALL SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId) + ") S" + ") M";
            //table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //string table = ;
            string condition = GetBaseGroup() + " AND (TRANSLOAD <>'T' OR TRANSLOAD IS NULL) ";
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBBR;IBTC"), condition);
        }

        private string getBaseStatusCondition(string tranType)
        {
            string cmp = CompanyId;
            if (IOFlag == "O")
            {
                cmp = BaseCompanyId;
            }
            List<string> statusList = new List<string>();
            string sql = string.Format("SELECT CD_TYPE FROM BSCODE WHERE CD={0} AND CD_TYPE IN ('ISFS','EALS')", SQLUtils.QuotedStr(cmp));
            DataTable dt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]);
                
                if ("EALS".Equals(cdType) && !statusList.Contains("E"))
                    statusList.Add("E");
                if ("F".Equals(tranType) || "L".Equals(tranType) || string.IsNullOrEmpty(tranType))
                {
                    if ("ISFS".Equals(cdType) && !statusList.Contains("S"))
                        statusList.Add("S");
                }
            }
            string con = "";
            if (statusList.Count <= 0)
            {
                con = " AND STATUS NOT IN ('E','S')";
            }
            else if (statusList.Contains("S") && statusList.Contains("E"))
            {
                con = "";
            }
            else if (statusList.Contains("S"))
            {
                con = " AND STATUS!='E'";
            }
            else if (statusList.Contains("E"))
            {
                con = " AND STATUS!='S'";
            }
            return con;
        }

        #endregion

        #region 各运输订舱确认纯查询
        public ActionResult ConfirmFclQueryData()
        {
            //string table = "(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP'))) S";
            //table += " UNION ALL SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" + ") M";
            //table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //string table = GetConfirmTable("IBCR;IBSP");
            string condition = GetBaseGroup() + " AND TRAN_TYPE='F' AND STATUS NOT IN ('A','S','E')";
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBCR;IBSP"), condition);
        }

        public ActionResult ConfirmLclQueryData()
        {
            //string table = "(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP'))) S";
            //table += " UNION ALL SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" + ") M";
            //table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //table = GetConfirmTable("IBCR;IBSP");
            string condition = GetBaseGroup() + " AND TRAN_TYPE='L' AND STATUS NOT IN ('A','S','E')";
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBCR;IBSP"), condition);
        }

        public ActionResult ConfirmAirQueryData()
        {
            //string table = "(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP'))) S";
            //table += " UNION ALL SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" + ") M";
            //table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //table = GetConfirmTable("IBCR;IBSP");
            string condition = GetBaseGroup() + " AND TRAN_TYPE='A' AND STATUS NOT IN ('A','S','E')";
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBCR;IBSP"), condition);
        }

        public ActionResult ConfirmExQueryData()
        {
            //string table = "(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP'))) S";
            //table += " UNION ALL SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" + ") M";
            //table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //table = GetConfirmTable("IBCR;IBSP");
            string condition = GetBaseGroup() + " AND TRAN_TYPE='E' AND STATUS NOT IN ('A','S','E')";
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBCR;IBSP"), condition);
        }

        public ActionResult ConfirmRailwayQueryData()
        {
            //string table = "(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP'))) S";
            //table += " UNION ALL SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" + ") M";
            //table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //table = GetConfirmTable("IBCR;IBSP");
            string condition = GetBaseGroup() + " AND TRAN_TYPE='R' AND STATUS NOT IN ('A','S','E')";
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBCR;IBSP"), condition);
        }

        public ActionResult ConfirmDTQueryData()
        {
            string condition = GetBaseGroup() + " AND TRAN_TYPE='T' AND STATUS NOT IN ('A','S','E')";  //CORDER IS NOT NULL AND CORDER !='N' 
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBCR;IBSP"), condition);
        }

        public ActionResult ConfirmCuQueryData()
        {
            //string table = "(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBBR', 'IBTC'))) S";
            //table += " UNION ALL SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId) + ") S" + ") M";
            //table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            //table = GetConfirmTable("IBBR;IBTC");
            string condition = GetBaseGroup() + " AND BSTATUS IN ( 'Y','B','I','H','C','F') AND STATUS NOT IN('E') AND (TRANSLOAD <>'T' OR TRANSLOAD IS NULL) ";
            condition = SMSMIQueryVirCondition(condition);
            condition += FilterSubBgCondition();
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(GetConfirmTable("IBBR;IBTC"), condition);
        }

        public string GetConfirmTable(string partyType)
        {
            string[] partyTypes = partyType.Split(';');
            string sql = string.Format(@"(SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1})) S", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            if ("O".Equals(IOFlag))
            {
                sql = string.Format(@"(SELECT * FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN {2})) S)T",
                    SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.Quoted(partyTypes));
            }
            return sql;
        }

        public ActionResult IDNPLTQueryData()
        {
            string condition = GetCreateDateCondition("SMIDNPL", "");
            return GetBootstrapData("SMIDNPL", condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable subDt = new DataTable();
            DataTable smdndt = new DataTable();
            DataTable subDt2 = new DataTable();
            DataTable smidn = new DataTable();
            DataTable smicntr = new DataTable();
            DataTable subDt4 = new DataTable();
            DataTable subDt5 = new DataTable();
            DataTable shipmentDt = new DataTable();
            string TranType = string.Empty;
            int n = 0;
            if (mainDt.Rows.Count > 0)
            {
                string shipmentid = mainDt.Rows[0]["SHIPMENT_ID"].ToString();
                string csmNo = mainDt.Rows[0]["CSM_NO"].ToString();
                string combine_info = mainDt.Rows[0]["COMBINE_INFO"].ToString();
                string Cmp = mainDt.Rows[0]["CMP"].ToString();
                TranType = mainDt.Rows[0]["TRAN_TYPE"].ToString();
                string[] dns = combine_info.Split(',');

                sql = string.Format("SELECT * FROM SMSMIPT WHERE U_FID={0} ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                #region 判斷IBTC寫入Inbound Customs Declaration
                if (subDt.Rows.Count > 0)
                {
                    for (int i = 0; i < subDt.Rows.Count; i++)
                    {
                        string PartyType = subDt.Rows[i]["PARTY_TYPE"].ToString();
                        if (PartyType == "IBTC" && PartyType != null)
                        {
                            string TcImporter = subDt.Rows[i]["PARTY_NO"].ToString();
                            string TcImporterNm = subDt.Rows[i]["PARTY_NAME"].ToString();
                            string TcImporterAddr = subDt.Rows[i]["PARTY_ADDR1"].ToString();

                            mainDt.Rows[0]["TC_IMPORTER"] = TcImporter;
                            mainDt.Rows[0]["TC_IMPORTER_NM"] = TcImporterNm;
                            mainDt.Rows[0]["TC_IMPORTER_ADDR"] = TcImporterAddr;
                            mainDt.Rows[0]["IS_TRANSIT_BROKER"] = "Y";
                            break;
                        }
                    }
                }
                #endregion

                sql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID={0} ORDER BY DN_NO", SQLUtils.QuotedStr(shipmentid));
                subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smidn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (subDt2.Rows.Count > 0)
                {
                    for (int i = 0; i < subDt2.Rows.Count; i++)
                    {
                        string DN_NO = subDt2.Rows[i]["DN_NO"].ToString();
                        if (i == 0)
                        {
                            sql = string.Format("SELECT * FROM SMICUFT WHERE DN_NO = {0}", SQLUtils.QuotedStr(DN_NO));
                        }
                        sql = sql + " or DN_NO =" + SQLUtils.QuotedStr(DN_NO);
                    }
                    subDt4 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                }


                sql = string.Format("SELECT * FROM SMIDNPL WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                if (!string.IsNullOrEmpty(csmNo))
                {
                    sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMIDNPL WHERE U_FID={0})", SQLUtils.QuotedStr(u_id));
                    shipmentDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = string.Format("SELECT * FROM SMIDNPL WHERE U_FID={0} ", SQLUtils.QuotedStr(u_id));
                }
                subDt5 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmiModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmiptModel");
            data["sub2"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            data["sub3"] = ModelFactory.ToTableJson(subDt2, "SmidnpModel");
            data["sub4"] = ModelFactory.ToTableJson(subDt4, "SmicuftModel");
            data["sub5"] = ModelFactory.ToTableJson(subDt5, "SmidnplModel");
            data["shipment"] = ModelFactory.ToTableJson(shipmentDt, "SmsmiModel");
            data["DnGrid"] = ModelFactory.ToTableJson(smdndt, "SmdnModel");
            if (TranType == "F" || TranType == "R")
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            }
            else
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            }

            data["smicntr"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult BookingUpdateData()
        {
            string changeData = Request.Params["changedData"];
            //string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string shipmentid = Request.Params["ShipmentId"];
            int n = Prolink.Math.GetValueAsInt(Request.Params["n"]);

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            //if (!string.IsNullOrEmpty(u_id))
            //    u_id = HttpUtility.UrlDecode(u_id);
            string u_id = getOneValueAsStringFromSql("SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid));

            bool isdelete = false;
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string su_id = string.Empty;
            string trantype = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        trantype = Prolink.Math.GetValueAsString(ei.Get("TRAN_TYPE"));
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("CSTATUS", "N");
                            ei.Put("STATUS", "A");
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            shipmentid = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(ei.Get("U_ID"))));
                            string del_sql = "DELETE FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                            mixList.Add(del_sql);
                            del_sql = "DELETE FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                            mixList.Add(del_sql);
                            del_sql = "DELETE FROM SMIDNP WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                            mixList.Add(del_sql);
                            if ("T".Equals(trantype))
                            {
                                string sql = string.Format("SELECT * FROM SMIRV WHERE IS_AUTOCREATE='Y' AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                                DataTable smirvdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (smirvdt.Rows.Count > 0)
                                {
                                    return Json(new { message = "The shipment is auto create,can't delete!" });
                                }

                                del_sql = "DELETE FROM SMORD WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                                mixList.Add(del_sql);
                                del_sql = "DELETE FROM SMRDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                                mixList.Add(del_sql);
                                del_sql = "DELETE FROM SMRCNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                                mixList.Add(del_sql);
                                del_sql = "DELETE FROM SMIRV WHERE SHIPMENT_INFO=" + SQLUtils.QuotedStr(shipmentid);
                                mixList.Add(del_sql);
                            }
                            isdelete = true;
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;
                    if (objList == null) continue;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiptModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        RemovePriority(ei);
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub3")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub4")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmicuftModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.PutKey("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", System.Guid.NewGuid().ToString());
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            //ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "cc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;
                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            RemovePriority(ei);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "tc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;
                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            RemovePriority(ei);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "cntr")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmicntrModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        RemovePriority(ei);
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            string newcntrno = Prolink.Math.GetValueAsString(ei.Get("CNTR_NO"));
                            string oldcntr = getOneValueAsStringFromSql("SELECT CNTR_NO FROM SMICNTR WHERE U_ID=" + SQLUtils.QuotedStr(su_id));
                            if (!string.IsNullOrEmpty(newcntrno) && !string.IsNullOrEmpty(oldcntr) && newcntrno != oldcntr)
                            {
                                string updatesmidnp = string.Format("UPDATE SMIDNP SET CNTR_NO={0} WHERE CNTR_NO={1} AND SHIPMENT_ID={2};",
                                    SQLUtils.QuotedStr(newcntrno), SQLUtils.QuotedStr(oldcntr), SQLUtils.QuotedStr(shipmentid));
                                mixList.Add(updatesmidnp);
                            }
                            mixList.Add(ei);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            string status = getOneValueAsStringFromSql("SELECT STATUS FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid));
                            if (string.IsNullOrEmpty(status) || "A".Equals(status))
                                mixList.Add(ei);
                        }
                        else
                        {
                            mixList.Add(ei);
                        }
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    UpdatePriorityToSMSMI(shipmentid, trantype);
                    SetDecInfoToSmsmi(trantype, shipmentid);
                    DataTable mainDt1 = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    InboundHelper.SetPartyToIBCR(mainDt1);

                    if (isdelete)
                    {
                        AfterDeleteSendToSAP(shipmentid);
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            Dictionary<string, object> data = AfterBookingSaveToDo(u_id, shipmentid);
            return ToContent(data);
        }

        public void RemovePriority(EditInstruct ei)
        {
            if (ei.IsExist("PRIORITY"))
            {
                string priority = ei.Get("PRIORITY");
                if (string.IsNullOrEmpty(priority) || string.IsNullOrEmpty(priority.Trim(' ')))
                    ei.Remove("PRIORITY");
            }
        }

        private void AfterDeleteSendToSAP(string shipmentid)
        {
            string sql3 = string.Format("UPDATE SMSM SET IS_OK='T' WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentid));
            exeSql(sql3);
            SendICACargoInfo(shipmentid);
        }

        private void SendICACargoInfo(string shipmentid)
        {
            string sql = string.Format("SELECT COMBINE_INFO,SHIPMENT_ID,CMP,IS_OK FROM SMSM WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                Business.TPV.Helper.SendICACargoInfo(dr);
            }
        }

        public void UpdatePriorityToSMSMI(string shipmentid, string trantype = "")
        {
            MixedList ml = new MixedList();
            string sql = string.Format(@"SELECT MIN(PRIORITY) FROM (SELECT PRIORITY FROM SMICNTR WHERE SHIPMENT_ID = {0} 
                            UNION  SELECT PRIORITY FROM SMIDN WHERE SHIPMENT_ID = {0}) T", SQLUtils.QuotedStr(shipmentid));
            string priority = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct smei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            smei.PutKey("SHIPMENT_ID", shipmentid);
            if (!string.IsNullOrEmpty(priority))
                smei.Put("PRIORITY", priority);
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                string cntrinfo = OperationUtils.GetValueAsString(
                    string.Format("SELECT DISTINCT CNTR_NO+',' FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID={0} AND CNTR_NO IS NOT NULL FOR XML PATH('')",
                    SQLUtils.QuotedStr(shipmentid)), Prolink.Web.WebContext.GetInstance().GetConnection());
                cntrinfo = cntrinfo.Trim(',');
                smei.Put("CNTR_INFO", cntrinfo);
            }
            ml.Add(smei);
            ml.Add(ReserveManage.SetArrivalDateToSMORD(shipmentid, ml));
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex) { }
        }

        public ActionResult CustomsUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string shipmentid = Request.Params["ShipmentId"];
            int n = Prolink.Math.GetValueAsInt(Request.Params["n"]);

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string su_id = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("CSTATUS", "N");
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiptModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];


                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub3")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "cc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;

                    if (n == 1)
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "tc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;

                    if (n == 1)
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }

            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            string sql = string.Format("SELECT * FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable subDt = new DataTable();
            DataTable smdndt = new DataTable();
            DataTable subDt2 = new DataTable();
            DataTable smidn = new DataTable();
            DataTable smicntr = new DataTable();
            DataTable subDt5 = new DataTable();
            DataTable shipmentDt = new DataTable();
            string TranType = string.Empty;
            if (mainDt.Rows.Count > 0)
            {
                string combine_info = mainDt.Rows[0]["COMBINE_INFO"].ToString();
                string[] dns = combine_info.Split(',');
                TranType = mainDt.Rows[0]["TRAN_TYPE"].ToString();
                string csmNo = mainDt.Rows[0]["CSM_NO"].ToString();
                sql = string.Format("SELECT * FROM SMSMIPT WHERE U_FID={0} ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID={0} ORDER BY DN_NO", SQLUtils.QuotedStr(shipmentid));
                subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smidn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                InboundHelper.SetPartyToIBCR(mainDt);

                sql = string.Format("SELECT * FROM SMIDNPL WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                if (!string.IsNullOrEmpty(csmNo))
                {
                    sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMIDNPL WHERE U_FID={0})", SQLUtils.QuotedStr(u_id));
                    shipmentDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = string.Format("SELECT * FROM SMIDNPL WHERE U_FID={0} ", SQLUtils.QuotedStr(u_id));
                }
                subDt5 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            sql = string.Format("SELECT * FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmiModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmiptModel");
            data["sub2"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            data["sub3"] = ModelFactory.ToTableJson(subDt2, "SmidnpModel");
            data["sub5"] = ModelFactory.ToTableJson(subDt5, "SmidnplModel");
            data["shipment"] = ModelFactory.ToTableJson(shipmentDt, "SmsmiModel");
            data["DnGrid"] = ModelFactory.ToTableJson(smdndt, "SmdnModel");
            if (TranType == "F" || TranType == "R")
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            }
            else
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            }

            data["smicntr"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            return ToContent(data);
        }

        public ActionResult ModifyBookingUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string shipmentid = Request.Params["ShipmentId"];
            int n = Prolink.Math.GetValueAsInt(Request.Params["n"]);

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string su_id = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("CSTATUS", "N");
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiptModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];


                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub3")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "cc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;

                    if (n == 1)
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "tc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;

                    if (n == 1)
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "cntr")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmicntrModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            mixList.Add(ei);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            string status = getOneValueAsStringFromSql("SELECT STATUS FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid));
                            if (string.IsNullOrEmpty(status) || "A".Equals(status))
                                mixList.Add(ei);
                        }
                        else
                        {
                            mixList.Add(ei);
                        }
                    }
                }

            }
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message });
                }
            }

            string sql = string.Format("SELECT TRAN_TYPE FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            string TranType = getOneValueAsStringFromSql(sql);

            InboundHelper.InputIBTWParty(TranType, shipmentid, su_id);

            sql = string.Format("SELECT * FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable subDt = new DataTable();
            DataTable smdndt = new DataTable();
            DataTable subDt2 = new DataTable();
            DataTable smidn = new DataTable();
            DataTable smicntr = new DataTable();
            TranType = string.Empty;

            InboundHandel.CreateOrdNew(shipmentid, u_id);
            string arrivalsqlUpdate = ReserveManage.SetArrivalDateToSMORD(shipmentid);
            exeSql(arrivalsqlUpdate); 

            if (mainDt.Rows.Count > 0)
            {
                string combine_info = mainDt.Rows[0]["COMBINE_INFO"].ToString();
                string[] dns = combine_info.Split(',');
                TranType = mainDt.Rows[0]["TRAN_TYPE"].ToString();
                UpdatePriorityToSMSMI(shipmentid, TranType);

                sql = string.Format("SELECT * FROM SMSMIPT WHERE U_FID={0} ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID={0} ORDER BY DN_NO", SQLUtils.QuotedStr(shipmentid));
                subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smidn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                InboundHelper.SetPartyToIBCR(mainDt);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmiModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmiptModel");
            data["sub2"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            data["sub3"] = ModelFactory.ToTableJson(subDt2, "SmidnpModel");
            data["DnGrid"] = ModelFactory.ToTableJson(smdndt, "SmdnModel");
            if (TranType == "F" || TranType == "R")
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            }
            else
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            }

            data["smicntr"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            return ToContent(data);
        }
        #endregion

        #region 訂艙確認保存

        public ActionResult BookingConfirmUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string shipmentid = Request.Params["ShipmentId"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
                return Json(new { message = "No valid Data!" });

            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string su_id = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("CSTATUS", "N");
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
                }

                else if (item.Key == "smicntr")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = "SmicntrModel";
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "smidn")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = "SmidnModel";
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                            mixList.Add(ei);
                        }
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            Dictionary<string, object> data = AfterBookingSaveToDo(u_id, shipmentid);
            return ToContent(data);
        }

        private Dictionary<string, object> AfterBookingSaveToDo(string u_id, string shipmentid)
        {
            string sql = string.Format("SELECT TRAN_TYPE FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            string TranType = getOneValueAsStringFromSql(sql);
            InboundHelper.InputIBTWParty(TranType, shipmentid, u_id);

            sql = string.Format("SELECT * FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable subDt = new DataTable();
            DataTable smdndt = new DataTable();
            DataTable subDt2 = new DataTable();
            DataTable subDt4 = new DataTable();
            DataTable smidn = new DataTable();
            DataTable smicntr = new DataTable();
            DataTable subDt5 = new DataTable();
            DataTable shipmentDt = new DataTable();
            if (mainDt.Rows.Count > 0)
            {
                string csmNo = mainDt.Rows[0]["CSM_NO"].ToString();
                sql = string.Format("SELECT * FROM SMSMIPT WHERE U_FID={0} ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID={0} ORDER BY DN_NO", SQLUtils.QuotedStr(shipmentid));
                subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smidn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (subDt2.Rows.Count > 0)
                {
                    for (int i = 0; i < subDt2.Rows.Count; i++)
                    {
                        string DN_NO = subDt2.Rows[i]["DN_NO"].ToString();
                        if (i == 0)
                        {
                            sql = string.Format("SELECT * FROM SMICUFT WHERE DN_NO = {0}", SQLUtils.QuotedStr(DN_NO));
                        }
                        sql = sql + " or DN_NO =" + SQLUtils.QuotedStr(DN_NO);
                    }
                    subDt4 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                }

                sql = string.Format("SELECT * FROM SMIDNPL WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                if (!string.IsNullOrEmpty(csmNo))
                {
                    sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMIDNPL WHERE U_FID={0})", SQLUtils.QuotedStr(u_id));
                    shipmentDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = string.Format("SELECT * FROM SMIDNPL WHERE U_FID={0} ", SQLUtils.QuotedStr(u_id));
                }
                subDt5 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmiModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmiptModel");
            data["sub2"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            data["sub3"] = ModelFactory.ToTableJson(subDt2, "SmidnpModel");
            data["sub4"] = ModelFactory.ToTableJson(subDt4, "SmicuftModel");
            data["sub5"] = ModelFactory.ToTableJson(subDt5, "SmidnplModel");
            data["shipment"] = ModelFactory.ToTableJson(shipmentDt, "SmsmiModel");
            data["DnGrid"] = ModelFactory.ToTableJson(smdndt, "SmdnModel");
            if (TranType == "F" || TranType == "R")
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            }
            else
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            }

            data["smicntr"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            return data;
        }

        public ActionResult CustomsConfirmUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string shipmentid = Request.Params["ShipmentId"];
            string trantype = Request.Params["trantype"];

            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);


            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string su_id = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("CSTATUS", "N");
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmiptModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];


                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub3")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmidnpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "cc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;

                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "tc")
                {
                    ArrayList objList = item.Value as ArrayList;
                    string model = string.Empty;

                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        model = "SmicntrModel";
                    }
                    else
                    {
                        model = "SmidnModel";
                    }

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, model);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }

                        mixList.Add(ei);
                    }
                }

            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            SetDecInfoToSmsmi(trantype, shipmentid);
            string sql = string.Format("SELECT * FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable subDt = new DataTable();
            DataTable smdndt = new DataTable();
            DataTable subDt2 = new DataTable();
            DataTable smidn = new DataTable();
            DataTable smicntr = new DataTable();
            string TranType = string.Empty;
            if (mainDt.Rows.Count > 0)
            {
                string combine_info = mainDt.Rows[0]["COMBINE_INFO"].ToString();
                string[] dns = combine_info.Split(',');
                TranType = mainDt.Rows[0]["TRAN_TYPE"].ToString();

                sql = string.Format("SELECT * FROM SMSMIPT WHERE U_FID={0} ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID={0} ORDER BY DN_NO", SQLUtils.QuotedStr(shipmentid));
                subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smidn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
                smicntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                InboundHelper.SetPartyToIBCR(mainDt);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmiModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmiptModel");
            data["sub2"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            data["sub3"] = ModelFactory.ToTableJson(subDt2, "SmidnpModel");
            data["DnGrid"] = ModelFactory.ToTableJson(smdndt, "SmdnModel");
            if (TranType == "F" || TranType == "R")
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            }
            else
            {
                data["CcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
                data["TcGrid"] = ModelFactory.ToTableJson(smidn, "SmidnModel");
            }

            data["smicntr"] = ModelFactory.ToTableJson(smicntr, "SmicntrModel");
            return ToContent(data);
        }

        private void SetDecInfoToSmsmi(string shipmentid)
        {
            string sql = string.Format("SELECT TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            string trantype = getOneValueAsStringFromSql(sql);
            SetDecInfoToSmsmi(trantype, shipmentid);
        }

        private void SetDecInfoToSmsmi(string trantype, string shipmentid)
        {
            string sql = "";
            string delist = "";
            string decDateList = "";
            string relesDateList = "";
            try
            {
                if (trantype == "F" || trantype == "R")
                {
                    sql = string.Format("SELECT DISTINCT ','+DEC_NO FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                    delist = getOneValueAsStringFromSql(sql);

                    sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), DEC_DATE, 23) FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                    decDateList = getOneValueAsStringFromSql(sql);

                    sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), REL_DATE, 23) FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                    relesDateList = getOneValueAsStringFromSql(sql);
                }
                else
                {
                    sql = string.Format("SELECT DISTINCT ','+DEC_NO FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                    delist = getOneValueAsStringFromSql(sql);

                    sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), DEC_DATE, 23) FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                    decDateList = getOneValueAsStringFromSql(sql);

                    sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), REL_DATE, 23) FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                    relesDateList = getOneValueAsStringFromSql(sql);
                }
                if (delist.Length > 500)
                {
                    delist = delist.Substring(0, 500);
                }
                EditInstruct smei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                smei.PutKey("SHIPMENT_ID", shipmentid);
                smei.Put("DEC_INFO", delist.Length >= 1 ? delist.Substring(1) : delist);
                smei.Put("DEC_DATE_INFO", decDateList.Length >= 1 ? decDateList.Substring(1) : decDateList);
                smei.Put("TC_REL_DATE", relesDateList.Length >= 1 ? relesDateList.Substring(1) : relesDateList);
                OperationUtils.ExecuteUpdate(smei, Prolink.Web.WebContext.GetInstance().GetConnection());
                SetDecInfoToSmord(trantype, shipmentid);
            } catch (Exception ex)
            {
            }
        }

        private void SetDecInfoToSmord(string trantype, string shipmentid)
        {
            string sql = "";
            DataTable dt = new DataTable();
            MixedList mixList = new MixedList();
            if (trantype == "F" || trantype == "R")
            {
                sql = string.Format("SELECT DEC_NO,DEC_DATE,TC_DEC_NO,TC_DEC_DATE,CNTR_NO FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string cntrNo = Prolink.Math.GetValueAsString(dt.Rows[i]["CNTR_NO"]);
                    if (!string.IsNullOrEmpty(cntrNo))
                    {
                        EditInstruct smordEi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                        smordEi.PutKey("SHIPMENT_ID", shipmentid);
                        smordEi.PutKey("CNTR_NO", cntrNo);
                        smordEi.Put("DEC_NO", dt.Rows[i]["DEC_NO"]);
                        smordEi.PutDate("DEC_DATE", dt.Rows[i]["DEC_DATE"]);
                        smordEi.Put("TC_DEC_NO", dt.Rows[i]["TC_DEC_NO"]);
                        smordEi.PutDate("TC_DEC_DATE", dt.Rows[i]["TC_DEC_DATE"]);
                        mixList.Add(smordEi);
                    }
                }
            }
            else
            {
                sql = string.Format("SELECT TOP 1 DEC_NO,DEC_DATE,TC_DEC_NO,TC_DEC_DATE FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    EditInstruct smordEi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    smordEi.PutKey("SHIPMENT_ID", shipmentid);
                    smordEi.Put("DEC_NO", dt.Rows[0]["DEC_NO"]);
                    smordEi.PutDate("DEC_DATE", dt.Rows[0]["DEC_DATE"]);
                    smordEi.Put("TC_DEC_NO", dt.Rows[0]["TC_DEC_NO"]);
                    smordEi.PutDate("TC_DEC_DATE", dt.Rows[0]["TC_DEC_DATE"]);
                    mixList.Add(smordEi);
                }
            }
            if (mixList.Count > 0)
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }

        private static void SetTcDecInfoToSmord(string shipmentid, MixedList mixList)
        {
            string sql = string.Format("SELECT TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            string trantype = getOneValueAsStringFromSql(sql);
            DataTable dt = new DataTable();
            try
            {
                if (trantype == "F" || trantype == "R")
                {
                    sql = string.Format("SELECT TC_DEC_NO,TC_DEC_DATE,CNTR_NO FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string cntrNo = Prolink.Math.GetValueAsString(dt.Rows[i]["CNTR_NO"]);
                        if (!string.IsNullOrEmpty(cntrNo))
                        {
                            EditInstruct smordEi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                            smordEi.PutKey("SHIPMENT_ID", shipmentid);
                            smordEi.PutKey("CNTR_NO", cntrNo);
                            smordEi.Put("TC_DEC_NO", dt.Rows[i]["TC_DEC_NO"]);
                            smordEi.PutDate("TC_DEC_DATE", dt.Rows[i]["TC_DEC_DATE"]);
                            mixList.Add(smordEi);
                        }
                    }
                }
                else
                {
                    sql = string.Format("SELECT TOP 1 TC_DEC_NO,TC_DEC_DATE FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count > 0)
                    {
                        EditInstruct smordEi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                        smordEi.PutKey("SHIPMENT_ID", shipmentid);
                        smordEi.Put("TC_DEC_NO", dt.Rows[0]["TC_DEC_NO"]);
                        smordEi.PutDate("TC_DEC_DATE", dt.Rows[0]["TC_DEC_DATE"]);
                        mixList.Add(smordEi);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region 设置灯号
        public ActionResult setLight()
        {
            string u_id = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string OUid = Prolink.Math.GetValueAsString(Request.Params["OUid"]);
            string Io = Prolink.Math.GetValueAsString(Request.Params["Io"]);
            string msg = TrackingEDI.InboundBusiness.SMSMIHelper.InboundsetLight(u_id, OUid, Io);
            return Json(new { msg = msg });
        }

        /// <summary>
        /// 设置灯号基本方法
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="OUid"></param>
        /// <param name="Io"></param>
        /// <returns></returns>
        public static string setLight(string u_id, string OUid, string Io)
        {
            string smsmTable = "SMSM", smsmptTable = "SMSMPT";
            string sql = string.Empty;
            string Location = string.Empty, TranType = string.Empty, PartyNo = string.Empty, ShipmentId = string.Empty;
            string msg = "success";

            if (Io == "I")
            {
                smsmTable = "SMSMI";
                smsmptTable = "SMSMIPT";
            }

            try
            {
                sql = "SELECT M.CMP, D.PARTY_NO, M.TRAN_TYPE, M.SHIPMENT_ID FROM " + smsmTable + " M LEFT JOIN " + smsmptTable + " D ON M.SHIPMENT_ID=D.SHIPMENT_ID AND D.PARTY_TYPE='CS' WHERE M.U_ID=" + SQLUtils.QuotedStr(u_id);
                DataTable smdt = getDataTableFromSql(sql);

                if (smdt.Rows.Count > 0)
                {
                    foreach (DataRow dr in smdt.Rows)
                    {
                        Location = Prolink.Math.GetValueAsString(dr["CMP"]);
                        TranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                        PartyNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                        ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    }
                }

                sql = "SELECT D.* FROM BSLIGHTM M, BSLIGHTD D WHERE M.U_ID=D.U_FID AND D.CUST_CD=" + SQLUtils.QuotedStr(PartyNo) + " AND D.TRAN_TYPE=" + SQLUtils.QuotedStr(TranType) + " AND D.IO=" + SQLUtils.QuotedStr(Io) + " AND M.CMP=" + SQLUtils.QuotedStr(Location);
                DataTable lgdt = getDataTableFromSql(sql);
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
                    lgdt = getDataTableFromSql(sql);

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

                DataTable dt = getDataTableFromSql(sql);
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
                    dnCount = getOneValueAsIntFromSql(string.Format("SELECT COUNT(*) FROM SMDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)));
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
                exeSql(sql);
            }
            catch (Exception ex)
            {

                msg = "error";
            }
            return msg;
        }
        #endregion

        public ActionResult GetISF()
        {
            string Cmp = Request.Params["Cmp"];
            string con = GetBaseGroup() + string.Format(" AND CMP={0}", SQLUtils.QuotedStr(Cmp));
            string ISFAcct = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFACCT' AND " + con)[1];
            string ISFKey = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFKey' AND " + con)[1];
            string ISFPWD = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFPWD' AND " + con)[1];

            return Json(new { ISFPWD = ISFPWD, ISFKey = ISFKey, ISFAcct = ISFAcct });
        }

        private static void GetLightByDn(int dnCount, List<string> edocList, string edocType, ref string str, string cdDescp)
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

        #region 基本方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "SMSMI":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMSMI WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }
        public ActionResult BKTable(string table, string trantype)
        {
            string condition = GetBookingQueryConditon(ref table, trantype);
            condition += getBaseStatusCondition(trantype);
            condition = GetCreateDateCondition("SMSMI", condition);
            return GetBootstrapData(table, condition);
        }

        private string GetBookingQueryConditon(ref string table, string trantype)
        {
            string condition = "";
            string col = bookingCol;
            if (!table.Contains(bookingCol))
                col = "*";
            if (!string.IsNullOrEmpty(trantype))
                condition = string.Format(" TRAN_TYPE={0} ", SQLUtils.QuotedStr(trantype));
            if ("O".Equals(IOFlag))
            {
                table += " UNION ALL SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT " + col + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") S" + ") M";
                if (!string.IsNullOrEmpty(condition))
                {
                    condition += " AND " + GetBaseGroup();
                }
                else
                    condition += GetBaseGroup();
            }
            else
            {
                table += " UNION ALL SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT " + col + " FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + ") S" + ") M";
                if (!string.IsNullOrEmpty(condition))
                {
                    condition += " AND " + GetBookingCondition(true);
                }
                else
                    condition += GetBookingCondition(true);
                condition += FilterSubBgCondition();
            }
            table = string.Format(table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            condition = SMSMIQueryVirCondition(condition);
            return condition;
        }

        private ActionResult GetBootstrapData(string table, string condition, string orderBy = "", string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                dt = GetStatusCountData(statusField, table, condition, Request.Params);
                pageSize = 1;
            }
            else
            {
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize, orderBy);
                if (resultType == "excel")
                    return ExportExcelFile(dt);
            }
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        private ActionResult GetBaseData(string table, string condition, string colNames = "*", string orderBy = "", string qType = "")
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 20;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            switch (qType)
            {
                case "1":
                    dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                case "2":
                    dt = ModelFactory.InquiryData("*", table, condition, orderBy, pageIndex, pageSize, ref  recordsCount);
                    break;
                case "3":
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                default:
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
            }

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues)
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "  SELECT '' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
            DataTable dtsum = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dtAll = dt.Copy();
            foreach (DataRow dr in dtsum.Rows)
            {
                dtAll.ImportRow(dr);
            }

            return dtAll;
        }

        #endregion

        #region 测试进口转出口
        public ActionResult o2iTest()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            InboundHelper.O2IFunc(ShipmentId);

            string sql = "UPDATE SMSM SET IS_OK='Y' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            try
            {
                exeSql(sql);
            }
            catch (Exception ex)
            {

            }

            return Json(new { msg = "success"});
        }

        public void o2iTest(string ShipmentId)
        {
            InboundHelper.O2IFunc(ShipmentId);

            string sql = "UPDATE SMSM SET IS_OK='Y' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            try
            {
                exeSql(sql);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region 通知物流
        public ActionResult notifytoLsp()
        {
            string returnMsg = "";
            string isok = "N";
            string requestUid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            string[] uidStr = requestUid.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            MixedList ml = new MixedList();
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            returnMsg = NotifyLspCheck(uidStr, ml, pairs);

            if (!string.IsNullOrEmpty(returnMsg))
            {
                return Json(new { message = returnMsg, IsOk = isok });
            }
            else
            {
                try
                {
                    if (ml.Count > 0)
                    {
                        int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string podcd;
                        foreach (string shipmentid in pairs.Keys)
                        {
                            podcd = pairs[shipmentid];
                            Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "010", Cmp = CompanyId, Sender = UserId, Location = podcd, LocationName = "", StsDescp = "Notify To LSP" });
                        } 
                        SendBookingOrCallMailList(uidStr);
                    }
                } 
                catch (Exception ex)
                {
                    return Json(new { message = ex.ToString(), IsOk = isok });
                }
            }
            return Json(new { message = @Resources.Locale.L_BookingAction_Controllers_131, IsOk = "Y" });
        }

        public string NotifyLspCheck(string[] uidStr, MixedList ml, Dictionary<string, string> pairs)
        {
            string message = "";
            string ShipmentId, trantype;
            string str = "\r\n";
            string sql = string.Format(@"SELECT * FROM SMSMI WHERE U_ID IN{0}", SQLUtils.Quoted(uidStr));
            DataTable dt = getDataTableFromSql(sql);
            if (dt.Rows.Count <= 0) return "No Data";

            sql = string.Format(@"SELECT * FROM SMSMIPT WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND U_FID IN{0}", SQLUtils.Quoted(uidStr));
            DataTable subdt = getDataTableFromSql(sql);

            foreach (DataRow dr in dt.Rows)
            {
                trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string podCd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                if (!pairs.ContainsKey(ShipmentId))
                { 
                    pairs.Add(ShipmentId, podCd);
                }
                if ("F".Equals(trantype) || "R".Equals(trantype))
                {
                    sql = string.Format("SELECT COUNT(*) FROM SMICNTR WHERE (WS_CD IS NULL OR WS_CD ='') AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                }
                else
                {
                    sql = string.Format("SELECT COUNT(*) FROM SMIDN WHERE (WS_CD IS NULL OR WS_CD ='') AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                }

                if (OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection()) > 0)
                {
                    message += ShipmentId + " Warehouse is Empty,Please Check it !" + str;
                }

                if (trantype == "F" || trantype == "R")
                {
                    sql = @"SELECT * FROM SMICNTR WHERE (POL1 IS NULL OR TRAN_TYPE1 IS NULL OR TRUCKER1 IS NULL 
                        OR DLV_AREA IS NULL OR DLV_AREA_NM IS NULL OR ADDR_CODE IS NULL OR DLV_ADDR IS NULL) AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable cntr = getDataTableFromSql(sql);

                    if (cntr.Rows.Count > 0)
                    {
                        message += ShipmentId + " Delivery Area Name,Delivery Address,Address Code,1st POL, 1st Tran Type, 1st Trucker, is required in Container Information!" + str;
                    }
                }
                else
                {
                    string Pol1 = Prolink.Math.GetValueAsString(dr["POL1"]);
                    string TranType1 = Prolink.Math.GetValueAsString(dr["TRAN_TYPE1"]);
                    string Trucker1 = Prolink.Math.GetValueAsString(dr["TRUCKER1"]);
                    if (Pol1 == "" || TranType1 == "" || Trucker1 == "")
                    {
                        message += ShipmentId + " 1st POL, 1st Tran Type, 1st Trucker is required." + str;
                    }

                    sql = @"SELECT * FROM SMIDN WHERE ( DLV_AREA IS NULL OR DLV_AREA_NM IS NULL OR ADDR_CODE IS NULL OR DLV_ADDR IS NULL) AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable dninfo = getDataTableFromSql(sql);

                    if (dninfo.Rows.Count > 0)
                    {
                        message += ShipmentId + " Delivery Area, Delivery Address is required in Import DN/Invoice." + str;
                    }
                }

                DataRow[] brRows = subdt.Select(string.Format(@"SHIPMENT_ID={0} AND PARTY_TYPE='IBBR'", SQLUtils.QuotedStr(ShipmentId)));
                DataRow[] crRows = subdt.Select(string.Format(@"SHIPMENT_ID={0} AND PARTY_TYPE='IBCR'", SQLUtils.QuotedStr(ShipmentId)));
                DataRow[] spRows = subdt.Select(string.Format(@"SHIPMENT_ID={0} AND PARTY_TYPE='IBSP'", SQLUtils.QuotedStr(ShipmentId)));
                if (brRows.Length <= 0 || crRows.Length <= 0 || spRows.Length <= 0)
                {
                    message += @Resources.Locale.L_BookingAction_Controllers_132 + ShipmentId + "Please check Party type has [IBBR], [IBCR], [IBSP]." + str;
                }

                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", dr["U_ID"]);
                ei.Put("MODIFY_BY", UserId);
                DateTime odt = DateTime.Now;
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                ei.PutDate("MODIFY_DATE", odt);
                ei.PutDate("MODIFY_DATE_L", ndt);
                if ("A".Equals(dr["STATUS"].ToString()))
                    ei.Put("STATUS", "B");
                if (string.IsNullOrEmpty(dr["BSTATUS"].ToString()))
                    ei.Put("BSTATUS", InboundHandel.BROKER_STATUS_B_DeclarationWithoutNotice);
                ei.Put("IB_WINDOW", UserId);
                ei.PutDate("IB_DATE", ndt);
                if(InboundHandel.CheckIsTransload(dr))
                    ei.Put("TRANSLOAD", "Y");
                ml.Add(ei);
            }
            return message;
        }
           
        private static string GetSubJectByTranType(string trantype, string term, string cancel = "", int sendtimes = 0)
        {
            string subject = string.Empty;
            string notice = " Booking Notice ";
            if (sendtimes > 0)
                notice = " Revise(" + sendtimes + ") Booking Notice ";
            switch (trantype)
            {
                case "A":
                    subject = "Air (" + term + ")" + cancel + notice;
                    break;
                case "L":
                    subject = "LCL (" + term + ")" + cancel + notice;
                    break;
                case "R":
                    subject = "RailWay (" + term + ")" + cancel + notice;
                    break;
                case "F":
                    subject = "FCL (" + term + ")" + cancel + notice;
                    break;
                case "E":
                    subject = "Express (" + term + ")" + cancel + notice;
                    break;
                case "D":
                    subject = "Express (" + term + ")" + cancel + notice;
                    break;
                case "T":
                    subject = "Domestic (" + term + ")" + cancel + notice;
                    break;
                default:
                    subject = "(" + term + ")" + cancel + notice;
                    break;
            }
            return subject;
        }
        #endregion

        #region 取消通知物流
        public ActionResult notifytoCancelLsp()
        {
            string returnMsg = "";
            string isok = "N";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            string[] uids = uid.Split(',');

            for (int i = 0; i < uids.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(uids[i]))
                {
                    string valueitme = doCancelNotifyLsp(uids[i]);
                    returnMsg += valueitme;
                    if (!string.IsNullOrEmpty(valueitme))
                    {
                        isok = "N";
                    }
                    else
                    {
                        isok = "Y";
                    }
                }
            }
            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingAction_Controllers_131;
            }
            return Json(new { message = returnMsg, IsOk = isok });
        }

        private string doCancelNotifyLsp(string s_uid)
        {
            string returnMsg = string.Empty;
            string shipmentid = "";
            string partyno = "";
            string partytype = "";
            string sql = string.Empty;
            string pol = string.Empty;

            sql = "SELECT * FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(s_uid);
            DataTable maindt = getDataTableFromSql(sql);

            if (maindt.Rows.Count > 0)
            {
                shipmentid = maindt.Rows[0]["SHIPMENT_ID"].ToString();
                string TranType = Prolink.Math.GetValueAsString(maindt.Rows[0]["TRAN_TYPE"]);
                string Status = Prolink.Math.GetValueAsString(maindt.Rows[0]["STATUS"]);
                string ProductionDate = Prolink.Math.GetValueAsString(maindt.Rows[0]["PRODUCTION_DATE"]);
                string Priority = Prolink.Math.GetValueAsString(maindt.Rows[0]["PRIORITY"]);
                string GroupId = Prolink.Math.GetValueAsString(maindt.Rows[0]["GROUP_ID"]);
                string CompanyId = Prolink.Math.GetValueAsString(maindt.Rows[0]["CMP"]);
                string term = maindt.Rows[0]["INCOTERM_CD"].ToString();
                pol = maindt.Rows[0]["POD_CD"].ToString();

                if(Status != "B")
                {
                    return @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + "Status Not At Notify LSP，So You Can't Cancel It！！";
                }

                sql = "SELECT * FROM SMSMIPT WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND U_FID=" + SQLUtils.QuotedStr(s_uid);
                DataTable subdt = getDataTableFromSql(sql);

                #region 发送email
                string mailcc = string.Empty;
                string mailType = string.Empty; 
                mailType = "ICABK";
                foreach (DataRow dr in subdt.Rows)
                {
                    partyno = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                    partytype = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);

                    DataTable mailGroupDt = new DataTable();

                    mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, "I"+TranType);
                    if (mailGroupDt.Rows.Count <= 0) continue;
                    string mailindex = string.Empty;
                    foreach (DataRow mailGroup in mailGroupDt.Rows)
                    {
                        mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                        if (!string.IsNullOrEmpty(mailindex) && mailcc.IndexOf(mailindex) < 0)
                        {
                            mailcc += mailindex + ";";
                        }
                    }

                    EvenFactory.AddEven(mailType + "#" + s_uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString() + "#" + "", s_uid, "IN", null, 1, 0, mailcc, GetSubJectByTranType(TranType, term,  " Cancel") + shipmentid, "");//mailcc
                }

                MixedList ml = new MixedList();
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", s_uid);
                ei.Put("STATUS", "A");
                ei.Put("TRANSLOAD", DBNull.Value);
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
                ei.PutDate("CANCEL_NOTIFY_LSP_TIME", ndt);
                ml.Add(ei);
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "011", Cmp = CompanyId, Sender = UserId, Location = pol, LocationName = "", StsDescp = "Cancel Notify To LSP" });
                }
                catch (Exception ex)
                {
                }
            }

            return "";
        }

        public ActionResult CancelBroker()
        {
            string returnMsg = "";
            string isok = "N";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            string[] uids = uid.Split(',');

            DataTable maindt = GetSMByUids(uids);
            if(maindt.Rows.Count<=0)
                return Json(new { message = "No Valid Data", IsOk = isok });

            string shipmentid = string.Empty;
            string status=string.Empty;
            foreach (DataRow dr in maindt.Rows)
            {
                shipmentid = dr["SHIPMENT_ID"].ToString();
                status = dr["STATUS"].ToString();
                if(!("C".Equals(status)||"H".Equals(status)))
                    returnMsg += @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + "Status Not At Notify Broker or Notify Transit Broker，So You Can't Cancel It！！";
                string sql = "SELECT RESERVE_NO FROM SMIRV WHERE STATUS<>'V' AND (SHIPMENT_INFO LIKE '%" + shipmentid + "%' OR SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid) + ") AND CMP=" + SQLUtils.QuotedStr(CompanyId);
                DataTable dt = getDataTableFromSql(sql);
                if (dt.Rows.Count > 0)
                {
                    returnMsg+=@Resources.Locale.L_BookingAction_Controllers_132 +" "+ shipmentid + " Cann't Cancel Broker, because the shipment have ordered truck. If you do process, please cancel truck calling first ！！";
                }
            }
            if (!string.IsNullOrEmpty(returnMsg))
                return Json(new { message = returnMsg, IsOk = "N" });

            foreach (DataRow dr in maindt.Rows)
            {
                returnMsg+=CancelBrokerByItem(dr);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    isok = "N";
                }
                else
                {
                    isok = "Y";
                }
            }
            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingAction_Controllers_131;
            }
            return Json(new { message = returnMsg, IsOk = isok });
        }

        private string CancelBrokerByItem(DataRow smRow)
        {
            string returnMsg = string.Empty;
            string partyno = "";
            string partytype = "";

            string shipmentid = smRow["SHIPMENT_ID"].ToString();
            string TranType = Prolink.Math.GetValueAsString(smRow["TRAN_TYPE"]);
            string Status = Prolink.Math.GetValueAsString(smRow["STATUS"]);
            string GroupId = Prolink.Math.GetValueAsString(smRow["GROUP_ID"]);
            string CompanyId = Prolink.Math.GetValueAsString(smRow["CMP"]);
            string term = smRow["INCOTERM_CD"].ToString();
            string s_uid = smRow["U_ID"].ToString();
            string pol = smRow["POD_CD"].ToString();

            if (!("C".Equals(Status) || "H".Equals(Status)))
            {
                return @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + "Status not at notify broker or notify transit broker，So You Can't Cancel It！！";
            }

            string sql = "SELECT * FROM SMSMIPT WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND U_FID=" + SQLUtils.QuotedStr(s_uid);
            DataTable subdt = getDataTableFromSql(sql);

            sql = string.Format("SELECT U_FID,(SELECT TOP 1 CSM_NO FROM SMSMI WHERE SMSMI.U_ID=SMIDNPL.U_FID) AS CSM_NO FROM SMIDNPL WHERE SHIPMENT_ID={0} AND U_FID IS NOT NULL", SQLUtils.QuotedStr(shipmentid));
            DataTable idnplDt = getDataTableFromSql(sql);

            string csmNo = "";
            foreach (DataRow dr in idnplDt.Rows)
            {
                csmNo += Prolink.Math.GetValueAsString(dr["CMP"]) + ",";
            }

            if (!string.IsNullOrEmpty(csmNo))
                return csmNo.TrimEnd(',') + " Status not at notify transit broker，So You Can't Cancel It！！";

            MixedList ml = new MixedList();

            string mailcc = string.Empty;
            string mailType = string.Empty;
            mailType = "ICABK";
            foreach (DataRow dr in subdt.Rows)
            {
                partyno = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                partytype = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);

                DataTable mailGroupDt = new DataTable();

                mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, "I" + TranType);
                if (mailGroupDt.Rows.Count <= 0) continue;
                string mailindex = string.Empty;
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailindex) && mailcc.IndexOf(mailindex) < 0)
                    {
                        mailcc += mailindex + ";";
                    }
                }

                EvenFactory.AddEven(mailType + "#" + s_uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString() + "#" + "", s_uid, "IN", ml, 1, 0, mailcc, GetSubJectByTranType(TranType, term, "Cancel Notify Broker And Cancel") + shipmentid, "");//mailcc
            }


            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", s_uid);
            ei.Put("STATUS", "A");
            ei.Put("CSTATUS", "Y");
            ei.Put("BSTATUS", "");
            ei.Put("TRANSLOAD", DBNull.Value);
            ml.Add(ei);

            //删除运输单信息
            ei = new EditInstruct("SMORD", EditInstruct.DELETE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ml.Add(ei);

            ei = new EditInstruct("SMRDN", EditInstruct.DELETE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ml.Add(ei);

            ei = new EditInstruct("SMRCNTR", EditInstruct.DELETE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ml.Add(ei);

            ei = new EditInstruct("SMIDNPL", EditInstruct.DELETE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ml.Add(ei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "011", Cmp = CompanyId, Sender = UserId, Location = pol, LocationName = "", StsDescp = "Cancel Notice To Broker" });
            }
            catch (Exception ex)
            {
                return @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + " Cancel Broker Faild The Reason is " + ex.Message + " ！！";
            }
            return "";
        }


        #region 物流业者分配
        public ActionResult LspDistribution()
        {
            string returnMessage = "success";
            string suids = Prolink.Math.GetValueAsString(Request.Params["suid"]);
            string sql = string.Empty;

            string[] a = suids.Split(',');
            for (int i = 0; i < a.Length; i++)
            {
                returnMessage = InboundHelper.InboudAllcBySuid(a[i]);
            }
            return Json(new { msg = returnMessage });
        }

        #endregion

        #region 執行訂艙確認
        public ActionResult doFclConfirmShipment()
        {
            string msg = "success";
            string suid = Prolink.Math.GetValueAsString(Request.Params["UId"]); 
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            string confirmMsg = InboundHandel.SetConfirm(userinfo, suid);
            if (!string.IsNullOrEmpty(confirmMsg))
                msg = confirmMsg;
            return Json(new {msg = msg });
        }

        #endregion

        

        #region 通知报关
        public ActionResult DECLBookAction()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            string Type = Prolink.Math.GetValueAsString(Request.Params["Type"]);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');
            if (uids.Length <= 0)
                return Json(new { message = "No valid data" });
            DataTable maindt = GetSMByUids(uids);
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            returnMsg = InboundHandel.SendDeclaration(maindt, Type, userinfo, "", true);
            foreach (DataRow dr in maindt.Rows)
            {
                string csmNo = Prolink.Math.GetValueAsString(dr["CSM_NO"]);
                string u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                if (string.IsNullOrEmpty(csmNo))
                    continue;
                InboundHandel.CreateOrdNew(csmNo, u_id);
            }
            if (returnMsg.Contains(@Resources.Locale.L_BookingAction_Controllers_160))
                return Json(new { message = returnMsg, IsOk = "Y" });
            return Json(new { message = returnMsg, IsOk = "N" });
        }
       
        #endregion

        #region 报关确认
        public ActionResult DECLBookConfirm()
        {
            string uid=Request.Params["UId"];
            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string declrlsdate = Request.Params["DeclRlsDate"];
            Result result=DECLBookConfirmFunc(shipmentid, declrlsdate);
            if(result.Success)
                return Json(new { message = result.Message, IsOk = "Y" });
            return Json(new { message = result.Message, IsOk = "N" });
        }

        public Result DECLBookConfirmFunc(string ShipmentId, string declrlsdate)
        {
            MixedList mixedlist = new MixedList();
            Result result = new Result();
            string smsmisql = string.Format(@"SELECT TRAN_TYPE,BSTATUS,U_ID,CMP,POD_CD,SHIPMENT_ID,STATUS,CSM_NO FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = getDataTableFromSql(smsmisql);
            if (dt.Rows.Count <= 0)
            {
                result.Success = false;
                result.Message = "No Data ,So you cann't do C.C Confirm！";
                return result;
            }
            string u_id =  dt.Rows[0]["U_ID"].ToString();
            string TranType = dt.Rows[0]["TRAN_TYPE"].ToString();
            string status = dt.Rows[0]["STATUS"].ToString();
            string smsmiBstatus = dt.Rows[0]["BSTATUS"].ToString();
            string pod=dt.Rows[0]["POD_CD"].ToString();
            string cmp= dt.Rows[0]["CMP"].ToString();
            string csmNo = dt.Rows[0]["CSM_NO"].ToString();
            string sql = string.Empty;
            
            result = CheckBrokerConfirm(dt.Rows[0]);
            if (!result.Success)
            {
                return result;
            }
            string bsctatus =InboundHandel.BROKER_STATUS_C_BrokerConfirm;
            string smsmistatus = "D";
            if (!string.IsNullOrEmpty(declrlsdate)){ //有放行时间，将状态更新为放行
                bsctatus = InboundHandel.BROKER_STATUS_F_Release;
            }

            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", u_id);
            ei.Put("MODIFY_BY", UserId);
            DateTime odt = DateTime.Now;
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("MODIFY_DATE", odt);
            ei.PutDate("MODIFY_DATE_L", ndt);
            ei.Put("BROKER_CONFIRM_BY", UserId);
            ei.PutDate("BROKER_CONFIRM_DATE", ndt);

            if ("Y".Equals(smsmiBstatus))
            {
                ei.Put("BSTATUS", bsctatus);
            }

            if ("C".Equals(status))     //只有notify broker的时候才能将状态更新成报关确认
            { 
                ei.Put("STATUS", smsmistatus);
            }
            mixedlist.Add(ei);

            if (!string.IsNullOrEmpty(csmNo))
                InboundHandel.UpdateShipmentStatusByCsmUId(u_id, "D", mixedlist);

            try
            {
                int[] results = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                CalculateFee cf = new CalculateFee(ShipmentId);
                List<string> emptyMessage = new List<string>();
                Bill.WriteLogTagStart("报关确认计算报关费用", ShipmentId);
                cf.CalBrokerFee(ShipmentId, emptyMessage);
                InboundTransfer.UpdateBillInfoToSMORD(ShipmentId, "", null);
                Bill.WriteLogTagStart(string.Join(";",emptyMessage), ShipmentId);
                Bill.WriteLogTagStart("结束计算", ShipmentId);

                Manager.IBSaveStatus(new Status() { ShipmentId = ShipmentId, StsCd = "046", Cmp = CompanyId, Sender = UserId, Location = pod, LocationName = "", StsDescp = "Broker Confirm" });

                #region 報關確認時更新dec no 到SMORD
                List<string> DecList = new List<string>();
                string delist = string.Empty;
                if (TranType == "F" || TranType == "R")
                {
                    sql = string.Format("SELECT DISTINCT DEC_NO+',' FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(ShipmentId));
                    delist = getOneValueAsStringFromSql(sql);
                }
                else
                {
                    sql = string.Format("SELECT DISTINCT DEC_NO+',' FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(ShipmentId));
                    delist = getOneValueAsStringFromSql(sql);
                }

                sql = "UPDATE SMORD SET DEC_INFO={0},BSTATUS={1},STATUS={2} WHERE SHIPMENT_ID={3}";
                sql = string.Format(sql, SQLUtils.QuotedStr(delist),
                    SQLUtils.QuotedStr(bsctatus), SQLUtils.QuotedStr(smsmistatus),
                    SQLUtils.QuotedStr(ShipmentId));
                exeSql(sql);

                sql = "SELECT RESERVE_NO, SHIPMENT_INFO FROM SMIRV WHERE STATUS <> 'V' AND SHIPMENT_INFO LIKE '%{0}%'";
                sql = string.Format(sql, ShipmentId);
                DataTable smdt = getDataTableFromSql(sql);
                if (smdt.Rows.Count > 0)
                {
                    foreach (DataRow smdr in smdt.Rows)
                    {
                        string ShipmentInfo = Prolink.Math.GetValueAsString(smdr["SHIPMENT_INFO"]);
                        string ReserveNo = Prolink.Math.GetValueAsString(smdr["RESERVE_NO"]);
                        sql = "SELECT DEC_NO FROM SMIDN WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(ShipmentInfo.Split(','));
                        DataTable dndt = getDataTableFromSql(sql);

                        if (dndt.Rows.Count > 0)
                        {
                            foreach (DataRow dndr in dndt.Rows)
                            {
                                string DecNo = Prolink.Math.GetValueAsString(dndr["DEC_NO"]);

                                if (!DecList.Contains(DecNo))
                                {
                                    DecList.Add(DecNo);
                                }
                            }
                        }

                        sql = "SELECT DEC_NO,NEW_SEAL,SHIPMENT_ID FROM SMICNTR WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(ShipmentInfo.Split(','));
                        DataTable cntrdt = getDataTableFromSql(sql);
                        string NewSeal = string.Empty;
                        if (cntrdt.Rows.Count > 0)
                        {
                            foreach (DataRow cntrdr in cntrdt.Rows)
                            {
                                string DecNo = Prolink.Math.GetValueAsString(cntrdr["DEC_NO"]);
                                string shipment_id = Prolink.Math.GetValueAsString(cntrdr["SHIPMENT_ID"]);
                               if(ShipmentId== shipment_id)
                                {
                                    NewSeal = Prolink.Math.GetValueAsString(cntrdr["NEW_SEAL"]);
                                }
                                if (!DecList.Contains(DecNo))
                                {
                                    DecList.Add(DecNo);
                                }
                            }
                        }

                        sql = "UPDATE SMIRV SET DEC_INFO={0},NEW_SEAL={1} WHERE RESERVE_NO={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(String.Join(",", DecList.ToArray())), SQLUtils.QuotedStr(NewSeal), SQLUtils.QuotedStr(ReserveNo));
                        exeSql(sql);

                    }
                }
                #endregion

                #region 報關確認時更dec no 到 SMRDN, SMRCNTR
                sql = "UPDATE SMRDN SET SMRDN.DEC_NO=SMIDN.DEC_NO, SMRDN.REL_DATE=SMIDN.REL_DATE FROM SMIDN WHERE SMIDN.DN_NO=SMRDN.DN_NO AND SMIDN.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                exeSql(sql);

                sql = "UPDATE SMRCNTR SET SMRCNTR.DEC_NO=SMICNTR.DEC_NO,SMRCNTR.REL_DATE=SMICNTR.REL_DATE FROM SMICNTR WHERE SMICNTR.CNTR_NO=SMRCNTR.CNTR_NO AND SMICNTR.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                exeSql(sql);
                #endregion

                if ("BR" == cmp)
                {
                    UserInfo userInfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
                    InboundHandel.CreateDeclaration(u_id, ShipmentId, userInfo, delist);
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.ToString();
                result.Success = false;
                return result;
            }
            result.Message = @Resources.Locale.L_BookingActionController_Controllers_68 + ShipmentId + @Resources.Locale.L_BookingAction_Controllers_170;
            result.Success= true;
            return result;
        }
         
        private Result CheckBrokerConfirm(DataRow smsmidr)
        {
            Result result = new Result();
            string TranType = Prolink.Math.GetValueAsString(smsmidr["TRAN_TYPE"]);
            string bstatus = Prolink.Math.GetValueAsString(smsmidr["BSTATUS"]);
            string pod = Prolink.Math.GetValueAsString(smsmidr["POD_CD"]);
            string shipmentid = Prolink.Math.GetValueAsString(smsmidr["SHIPMENT_ID"]);
            string sql = string.Empty;
            if ("O".Equals(IOFlag))
            {
                sql = string.Format("SELECT COUNT(1) FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_NO={1} AND PARTY_TYPE='IBBR'",
                        SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(CompanyId));
                int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (count <= 0)
                {
                    result.Success = false;
                    result.Message = "No permission to operated!";
                    return result;
                }
            }
            
            string countmessage = " DN";
            string table = "SMIDN";
            if (TranType == "F" || TranType == "R")
            {
                table = "SMICNTR";
                countmessage = " Container";
            }
            sql = string.Format(@"SELECT COUNT(1) FROM {0} WHERE SHIPMENT_ID={1}
            AND (DEC_DATE IS NULL OR  DEC_DATE ='' OR REL_DATE='' OR IMPORT_NO='' OR INSPECTION='' OR
            REL_DATE IS NULL OR IMPORT_NO IS NULL OR INSPECTION IS NULL )", table, SQLUtils.QuotedStr(shipmentid));
            int nodatacount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            int hasdatacount = 0;
            if (nodatacount > 0)
            {
                sql = string.Format(@"SELECT COUNT(1) FROM {0} WHERE SHIPMENT_ID={1}
                AND DEC_DATE IS NOT NULL AND REL_DATE IS NOT NULL AND IMPORT_NO IS NOT NULL AND INSPECTION IS NOT NULL",
                 table, SQLUtils.QuotedStr(shipmentid));
                hasdatacount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (hasdatacount >= 1)
                {
                    result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid +
                        " Has " + hasdatacount.ToString() + countmessage + " Exp. C/C Successful And Also Has " + nodatacount.ToString() + countmessage + " Exp. C/C Failure!";
                    result.Success = false;
                    return result;
                }
            }

            sql = string.Format("SELECT * FROM " + table + " WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (mainDt.Rows.Count > 0)
            {
                foreach (DataRow dr in mainDt.Rows)
                {
                    string DecDate = Prolink.Math.GetValueAsString(dr["DEC_DATE"]);
                    string RelDate = Prolink.Math.GetValueAsString(dr["REL_DATE"]);
                    string ImportNo = Prolink.Math.GetValueAsString(dr["IMPORT_NO"]);
                    string Inspection = Prolink.Math.GetValueAsString(dr["INSPECTION"]);

                    if (DecDate == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "[Declaration Date] not fill";
                        result.Success = false;
                        return result;
                    }

                    if (RelDate == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "[Release Date] not fill";
                        result.Success = false;
                        return result;
                    }

                    if (ImportNo == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "[Import No.] not fill";
                        result.Success = false;
                        return result;
                    }

                    if (Inspection == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "[Inspection] not fill";
                        result.Success = false;
                        return result;
                    }
                }

            }

            if (bstatus.Equals("C"))
            {
                result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + @Resources.Locale.L_BookingAction_Controllers_168;
                result.Success = false;
                return result;
            }
            if ("N".Equals(bstatus))
            {
                result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_67;
                result.Success = false;
                return result;
            }
            result.Success = true;
            return result;
        }


        #endregion

        #region 转关确认
        public ActionResult TcBookConfirm()
        {
            string u_id = Request.Params["UId"];
            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string declrlsdate = Request.Params["DeclRlsDate"];
            string returnMessage = "success";
            MixedList mixedlist = new MixedList();
            List<string> msg = new List<string>();
            string isok = "Y";
            string sql = string.Format("SELECT SHIPMENT_ID,TRAN_TYPE,BSTATUS,STATUS,U_ID FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count<=0)
                return Json(new { message = returnMessage, IsOk = "N" });
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId,IoFlag= IOFlag };
            foreach (DataRow dr in dt.Rows)
            {
                TrackingEDI.Business.Result result = InboundHandel.TCBookConfirmFunc(dr, userinfo);
                if (!result.Success)
                {
                    isok = "N";
                }
                msg.Add(result.Message);
            }
            returnMessage = string.Join("\r\n", msg);
            return Json(new { message = returnMessage, IsOk = isok });
        }
        #endregion

        #region 批量寫入production date and Priority
        public ActionResult InsertPdp()
        {
            string returnMessage = "success";
            string shipments = Prolink.Math.GetValueAsString(Request.Params["shipments"]);
            string pd = Prolink.Math.GetValueAsString(Request.Params["ProductionDate"]);
            string priority = Prolink.Math.GetValueAsString(Request.Params["Priority"]);

            DateTime odt = DateTime.Now;
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            MixedList mlist = new MixedList();
            string[] shipmentids = shipments.Split(',');
            string sql = "UPDATE SMSMI SET PRODUCTION_DATE={0}, PRIORITY={1}, MODIFY_BY={2}, MODIFY_DATE={3}, MODIFY_DATE_L={4} WHERE SHIPMENT_ID IN {5}";
            sql = string.Format(sql, SQLUtils.QuotedStr(pd), SQLUtils.QuotedStr(priority), SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.Quoted(shipmentids));
            mlist.Add(sql);
            if ("1".Equals(priority))
            {
                string cntrsql = String.Format("UPDATE SMICNTR SET PRIORITY={0} WHERE SHIPMENT_ID IN {1}", SQLUtils.QuotedStr(priority), SQLUtils.Quoted(shipmentids));
                mlist.Add(cntrsql);
                string idnsql = String.Format("UPDATE SMIDN SET PRIORITY={0} WHERE SHIPMENT_ID IN {1}", SQLUtils.QuotedStr(priority), SQLUtils.Quoted(shipmentids));
                mlist.Add(idnsql);
            }

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage });
        }

        public ActionResult UpdatePriorityInfo()
        {
            string shipmentIds = Request.Params["shipments"];
            string[] shipmentList = shipmentIds.Split(',');
            string scmRequestDate = Request.Params["ProductionDate"];
            string Priority = Prolink.Math.GetValueAsString(Request.Params["Priority"]);
            MixedList mlist = new MixedList();
            string smCol = "", ordCol = "", idnCol = "";
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
            if (!string.IsNullOrEmpty(scmRequestDate))
            {
                smCol = string.Format(" PRODUCTION_DATE={0},UPLOAD_SCM_DATE={1},UPLOAD_SCM_BY={2} ",
                    SQLUtils.QuotedStr(scmRequestDate), SQLUtils.QuotedStr(ndt.ToString("yyyyMMdd")), SQLUtils.QuotedStr(UserId));
                idnCol = string.Format(" SCMREQUEST_DATE={0} ", SQLUtils.QuotedStr(scmRequestDate));
                ordCol = string.Format(" ARRIVAL_DATE={0} ", SQLUtils.QuotedStr(scmRequestDate));
            }
            if (!string.IsNullOrEmpty(Priority))
            {
                if (!string.IsNullOrEmpty(smCol))
                {
                    smCol += ",";
                    idnCol += ",";
                    ordCol += ",";
                }
                smCol += string.Format(" PRIORITY={0} ", SQLUtils.QuotedStr(Priority));
                idnCol += string.Format(" PRIORITY={0} ", SQLUtils.QuotedStr(Priority));
                ordCol += string.Format(" PRIORITY={0} ", SQLUtils.QuotedStr(Priority));
            }
            if (string.IsNullOrEmpty(scmRequestDate) && string.IsNullOrEmpty(Priority))
                return Json(new { message = "false" });
            foreach (string shipmentId in shipmentList)
            {
                if (string.IsNullOrEmpty(shipmentId))
                    continue;
                string sql = string.Format("UPDATE SMSMI SET {1} WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId), smCol);
                mlist.Add(sql);
                sql = string.Format("UPDATE SMICNTR SET {1} WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId), idnCol);
                mlist.Add(sql);
                sql = string.Format("UPDATE SMIDN SET {1} WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId), idnCol);
                mlist.Add(sql);
                sql = string.Format(@"UPDATE SMORD SET {1} WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId), ordCol);
                mlist.Add(sql);
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                }
            }
            return Json(new { message = "success" });
        }
        #endregion

        #region 基础查询方法
        public DataTable GetSMByUid(string uid)
        {
            string sql = "SELECT DATEDIFF(DAY,ETD,ETA) AS INTERVAL_DAY,* FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetSMByUids(string[] uids)
        {
            string sql = string.Format("SELECT * FROM SMSMI WHERE U_ID IN {0} ", SQLUtils.Quoted(uids));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public ActionResult GetUidBySmId()
        {
            string uid = string.Empty;
            string trantype = string.Empty;
            string shipmentid = Request.Params["shipmentid"];
            string sql = string.Format("SELECT TRAN_TYPE,U_ID FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smdt.Rows.Count > 0)
            {
                uid = smdt.Rows[0]["U_ID"].ToString();
                trantype = smdt.Rows[0]["TRAN_TYPE"].ToString();
            }
            return Json(new { uid = uid, trantype = trantype });
        }


        public DataTable GetSMByShipmentId(string shipmentid,string condition="1=1")
        {
            string sql = string.Format("SELECT DATEDIFF(DAY,ETD,ETA) AS INTERVAL_DAY,* FROM SMSMI WHERE SHIPMENT_ID={0} AND " + condition, SQLUtils.QuotedStr(shipmentid));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetPTBySMUid(string[] uids)
        {
            string sql = string.Format("SELECT * FROM SMSMIPT WHERE SHIPMENT_ID=(SELECT SHIPMENT_ID FROM SMSM WHERE U_ID IN {0}) ORDER BY ORDER_BY ASC", SQLUtils.Quoted(uids));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        #endregion

        #region Excel導入
        [HttpPost]
        public ActionResult UploadSmsmi(FormCollection form)
        {

            string returnMessage = "success";
            string ermsg = "";

            int StartRow = 0; //Excel 從第2排開始讀
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "upload error:doesn't recevie any excel file!" });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                return Json(new { message = "upload error:There's nothing in the excel file!" });
            }
            else
            {
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadSmsmi\\");
                string excelFileName = string.Format("{0}.{1}", dirpath + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);
                string InsertNo = System.Guid.NewGuid().ToString(); //用來判斷同一批資料
                /*
                 * dr[0]: ISF Seller = PARTY_NO(SISF)        NEW 2017/08/10
                 * dr[1]: BookingParty = PARTY_NO(IBBK)
                 * dr[2]: Location = CMP
                 * dr[3]: TranType
                 * dr[4]: MBL MASTER_NO
                 * dr[5]: HBL HOUSE_NO
                 * dr[6]: MARKS  
                 * dr[7]: Commodity GOODS
                 * dr[8]: BL MARKS
                 * dr[9]: YEAR
                 * dr[10]: Month
                 * dr[11]: Weekly
                 * dr[12]: Gvalue
                 * dr[13]: Cur
                 * dr[14]: CNT20
                 * dr[15]: CNT40
                 * dr[16]: CNT40HQ
                 * dr[17]: CNT_TYPE
                 * dr[18]: CNT_NUMBER
                 * dr[19]: PKG_NUM
                 * dr[20]: PKG_UNIT
                 * dr[21]: GW
                 * dr[22]: GWU
                 * dr[23]: VW
                 * dr[24]: CBM
                 * dr[25]: CBM UNIT
                 * dr[26]: CW
                 * dr[27]: INCOTERM_CD ---
                 * dr[28]: INCORTERM DESCPIPTION           NEW 2017/08/10
                 * dr[29]: LOADING_FROM
                 * dr[30]: LOADING_TO
                 * dr[31]: CARRIER ---
                 * dr[32]: HTCCode
                 * dr[33]: Country              NEW
                 * dr[34]: SCAC_CD
                 * dr[35]: FRT_TERM
                 * dr[36]: POR_CD ---
                 * dr[37]: POL_CD ---
                 * dr[38]: POD_CD ---
                 * dr[39]: DEST_CD ---
                 * dr[40]: Dest.Region          NEW
                 * dr[41]: VESSEL1
                 * dr[42]: VOYAGE1
                 * dr[43]: ETD1
                 * dr[44]: ETA1
                 * dr[45]: VESSEL2
                 * dr[46]: VOYAGE2
                 * dr[47]: ETD2
                 * dr[48]: ETA2
                 * dr[49]: VESSEL3
                 * dr[50]: VOYAGE3
                 * dr[51]: ETD3
                 * dr[52]: ETA3
                 * dr[53]: VESSEL4
                 * dr[54]: VOYAGE4
                 * dr[55]: ETD4
                 * dr[56]: ETA4
                 * dr[57]: ATD1
                 * dr[58]: ATA
                 * dr[59]: PARTY_NO(SH)
                 * dr[60]: PARTY_NO(CS)
                 * dr[61]: PARTY_NO(NT)
                 * dr[62]: PARTY_NO(WE)
                 * dr[63]: PARTY_NO(FC)
                 * dr[64]: PARTY_NO(ZT)
                 * dr[65]: PARTY_NO(RE)
                 * dr[66]: PARTY_NO(SP)  
                 * dr[67]: Invoice No = INV_NO      DN/Invoice.主檔
                 * dr[68]: Inv_Unit_Price	          NEW
                 * dr[69]: Inv_Currency               NEW
                 * dr[70]: CargoType = DIVISION       NEW   根據TransType判斷
                 * dr[71]: NW
                 * dr[72]: GW
                 * dr[73]: GWU
                 * dr[74]: CBM 
                 * dr[75]: CBMU
                 * dr[76]: QTY
                 * dr[77]: QTYU
                 * dr[78]: PKG_NUM
                 * dr[79]: PKG_UNIT
                 * dr[80]: PKG_UNIT_DESC
                 * dr[81]: IPART_NO                  DN細檔 
                 * dr[82]: QTY
                 * dr[83]: GW
                 * dr[84]: CBM
                 * dr[85]: CNTR_NO
                 * dr[86]: CNTR_TYPE
                 * dr[87]: SEAL_NO1
                 * dr[88]: OPART_NO
                 * dr[89]: PO_NO
                 */

                List<string> msgs = new List<string>();
                #region 过滤空数据
                DataTable copyDt = dt.Clone();
                foreach (DataRow dr in dt.Rows)
                {
                    bool add = false;
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr[col.ColumnName])))
                        {
                            add = true;
                            break;
                        }
                    }
                    if (add)
                        copyDt.ImportRow(dr);
                }
                dt = copyDt;
                #endregion
                
                CheckSMSMIImport(dt, msgs);
                if (msgs.Count > 0)
                    return Json(new { message = "error:" + string.Join(System.Environment.NewLine, msgs) });

                DataTable carrierDt = OperationUtils.GetDataTable(string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE = 'TCAR' AND (CMP = {0}  OR CMP='*') AND GROUP_ID={1}", SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(GroupId)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable cityDt = OperationUtils.GetDataTable(string.Format("SELECT CNTRY_CD,PORT_CD,PORT_NM FROM BSCITY WITH(NOLOCK)"), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable smptyDt = OperationUtils.GetDataTable(string.Format("SELECT PARTY_NO,PART_ADDR1,PARTY_NAME,PARTY_NAME,DEP FROM SMPTY WITH(NOLOCK)"), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable maDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM DEST_MAP WHERE GROUP_ID={0}", SQLUtils.QuotedStr(GroupId)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable directDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM DIRECT_MAP WHERE GROUP_ID={0}", SQLUtils.QuotedStr(GroupId)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //string secCmp = "";
                try
                {
                    Dictionary<string, List<string>> shipmentDic = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> opartDic = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> poDic = new Dictionary<string, List<string>>();
                    List<string> InvNoList = new List<string>();
                    List<string> shipmentList = new List<string>();
                    string CombineDn = "";
                    string CombineInv = "";
                    string CombineCargo = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string Sql = "";
                        string UId = "", ShipmentId = "", CntyCd = "", PortCd = "";
                        string Dn_UId = "", Dnp_UId = "", Cntr_UId = "";
                        EditInstruct ei;
                        EditInstruct ei2;
                        EditInstruct ei3;
                        EditInstruct ei4;
                        MixedList ml = new MixedList();
                        MixedList ml2 = new MixedList();

                        DataRow dr = dt.Rows[i];

                        #region 判斷必填欄位
                        //必填欄位
                        string IsfSeller = Prolink.Math.GetValueAsString(dr[0]);
                        string BookingParty = Prolink.Math.GetValueAsString(dr[1]);
                        string Location = Prolink.Math.GetValueAsString(dr[2]);
                        string TranType = Prolink.Math.GetValueAsString(dr[3]);
                        string MasterNo = Prolink.Math.GetValueAsString(dr[4]);
                        string HouseNo = Prolink.Math.GetValueAsString(dr[5]);
                        string Marks = Prolink.Math.GetValueAsString(dr[6]);
                        string Commodity = Prolink.Math.GetValueAsString(dr[7]);
                        string Blmark = Prolink.Math.GetValueAsString(dr[8]);
                        decimal Year = Prolink.Math.GetValueAsDecimal(dr[9]);
                        decimal Month = Prolink.Math.GetValueAsDecimal(dr[10]);
                        decimal Weekly = Prolink.Math.GetValueAsDecimal(dr[11]);
                        decimal Gvalue = Prolink.Math.GetValueAsDecimal(dr[12]);
                        string Cur = Prolink.Math.GetValueAsString(dr[13]);
                        decimal Cnt20 = Prolink.Math.GetValueAsDecimal(dr[14]);
                        decimal Cnt40 = Prolink.Math.GetValueAsDecimal(dr[15]);
                        decimal Cnt40HQ = Prolink.Math.GetValueAsDecimal(dr[16]);
                        string CntType = Prolink.Math.GetValueAsString(dr[17]);
                        decimal CntNumber = Prolink.Math.GetValueAsDecimal(dr[18]);
                        string PkgNum_str = Prolink.Math.GetValueAsString(dr[19]);
                        decimal PkgNum = Prolink.Math.GetValueAsDecimal(dr[19]);
                        string PkgUnit = Prolink.Math.GetValueAsString(dr[20]);
                        decimal Gw = Prolink.Math.GetValueAsDecimal(dr[21]);
                        string Gwu = Prolink.Math.GetValueAsString(dr[22]);
                        decimal Vw = Prolink.Math.GetValueAsDecimal(dr[23]);
                        decimal Cbm = Prolink.Math.GetValueAsDecimal(dr[24]);
                        string Cbmu = Prolink.Math.GetValueAsString(dr[25]);
                        decimal Cw = Prolink.Math.GetValueAsDecimal(dr[26]);
                        string Incoterm = Prolink.Math.GetValueAsString(dr[27]);
                        string IncotermDescp = Prolink.Math.GetValueAsString(dr[28]);
                        string LoadingFrom = Prolink.Math.GetValueAsString(dr[29]);
                        string LoadingTo = Prolink.Math.GetValueAsString(dr[30]);
                        string Carrier = Prolink.Math.GetValueAsString(dr[31]);
                        string HtsCode = Prolink.Math.GetValueAsString(dr[32]);
                        string Country = Prolink.Math.GetValueAsString(dr[33]);
                        string ScacCd = Prolink.Math.GetValueAsString(dr[34]);
                        string FreightTerm = Prolink.Math.GetValueAsString(dr[35]);
                        if (FreightTerm != "")
                        {
                            FreightTerm = FreightTerm.Substring(0, 1).ToUpper();
                        }
                        string PorCd = Prolink.Math.GetValueAsString(dr[36]);
                        string PolCd = Prolink.Math.GetValueAsString(dr[37]);
                        string PodCd = Prolink.Math.GetValueAsString(dr[38]);
                        string DestCd = Prolink.Math.GetValueAsString(dr[39]);
                        string DestRegion = Prolink.Math.GetValueAsString(dr[40]);
                        string Vessel1 = Prolink.Math.GetValueAsString(dr[41]);
                        string Voyage1 = Prolink.Math.GetValueAsString(dr[42]);
                        DateTime Etd1 = Prolink.Math.GetValueAsDateTime(dr[43]);
                        DateTime Eta1 = Prolink.Math.GetValueAsDateTime(dr[44]);
                        string Vessel2 = Prolink.Math.GetValueAsString(dr[45]);
                        string Voyage2 = Prolink.Math.GetValueAsString(dr[46]);
                        DateTime Etd2 = Prolink.Math.GetValueAsDateTime(dr[47]);
                        DateTime Eta2 = Prolink.Math.GetValueAsDateTime(dr[48]);
                        string Vessel3 = Prolink.Math.GetValueAsString(dr[49]);
                        string Voyage3 = Prolink.Math.GetValueAsString(dr[50]);
                        DateTime Etd3 = Prolink.Math.GetValueAsDateTime(dr[51]);
                        DateTime Eta3 = Prolink.Math.GetValueAsDateTime(dr[52]);
                        string Vessel4 = Prolink.Math.GetValueAsString(dr[53]);
                        string Voyage4 = Prolink.Math.GetValueAsString(dr[54]);
                        DateTime Etd4 = Prolink.Math.GetValueAsDateTime(dr[55]);
                        DateTime Eta4 = Prolink.Math.GetValueAsDateTime(dr[56]);
                        DateTime Atd1 = Prolink.Math.GetValueAsDateTime(dr[57]);
                        DateTime eta = Eta1;
                        if (!string.IsNullOrEmpty(dr[56].ToString()))
                            eta = Eta4;
                        else if (!string.IsNullOrEmpty(dr[52].ToString()))
                            eta = Eta3;
                        else if (!string.IsNullOrEmpty(dr[48].ToString()))
                            eta = Eta2;
                        if (dr[57].ToString() == "")
                        {
                            Atd1 = Etd1;

                        }
                        DateTime Ata = Prolink.Math.GetValueAsDateTime(dr[58]);
                        string ShipperNo = Prolink.Math.GetValueAsString(dr[59]);
                        string ConsigneeNo = Prolink.Math.GetValueAsString(dr[60]);
                        string NotifyNo = Prolink.Math.GetValueAsString(dr[61]);
                        string ShipToNo = Prolink.Math.GetValueAsString(dr[62]);
                        string FiCustomer = Prolink.Math.GetValueAsString(dr[63]);
                        string SubBG = Prolink.Math.GetValueAsString(dr[64]);
                        string Importer = Prolink.Math.GetValueAsString(dr[65]);
                        string HandlingAgen = Prolink.Math.GetValueAsString(dr[66]);

                        string InvoiceNo = Prolink.Math.GetValueAsString(dr[67]);
                        if (!InvNoList.Contains(InvoiceNo)&&!string.IsNullOrEmpty(InvoiceNo))
                            InvNoList.Add(InvoiceNo);
                        decimal InvPrice = Prolink.Math.GetValueAsDecimal(dr[68]);
                        string InvCur = Prolink.Math.GetValueAsString(dr[69]);
                        string CargoType = Prolink.Math.GetValueAsString(dr[70]);
                        decimal Dn_Nw = Prolink.Math.GetValueAsDecimal(dr[71]);
                        decimal Dn_Gw = Prolink.Math.GetValueAsDecimal(dr[72]);
                        string Dn_Gwu = Prolink.Math.GetValueAsString(dr[73]);
                        decimal Dn_Cbm = Prolink.Math.GetValueAsDecimal(dr[74]);
                        string Dn_Cbmu = Prolink.Math.GetValueAsString(dr[75]);
                        decimal Dn_Qty = Prolink.Math.GetValueAsDecimal(dr[76]);
                        string Dn_Qtyu = Prolink.Math.GetValueAsString(dr[77]);
                        string Dn_PkgNum_str = Prolink.Math.GetValueAsString(dr[78]);
                        decimal Dn_PkgNum = Prolink.Math.GetValueAsDecimal(dr[78]);
                        string Dn_PkgUnit = Prolink.Math.GetValueAsString(dr[79]);
                        string Dn_PkgUnitDesc = Prolink.Math.GetValueAsString(dr[80]);
                        string IpartNo = Prolink.Math.GetValueAsString(dr[81]);
                        decimal Dnp_Qty = Prolink.Math.GetValueAsDecimal(dr[82]);
                        decimal Dnp_Gw = Prolink.Math.GetValueAsDecimal(dr[83]);
                        decimal Dnp_Cbm = Prolink.Math.GetValueAsDecimal(dr[84]);
                        string CntrNo = Prolink.Math.GetValueAsString(dr[85]);
                        if (!string.IsNullOrEmpty(CntrNo))
                        {
                            CntrNo = CntrNo.Replace(Environment.NewLine, "");
                            CntrNo = CntrNo.Trim();
                        }
                        string CntrType = Prolink.Math.GetValueAsString(dr[86]);
                        string SealNo1 = Prolink.Math.GetValueAsString(dr[87]);
                        string OpartNo = Prolink.Math.GetValueAsString(dr[88]);
                        string PoNo = Prolink.Math.GetValueAsString(dr[89]);
                        //string Division = Prolink.Math.GetValueAsString(dr[80]);
                        if (dr[9].ToString() == "" && dr[10].ToString() == "" && dr[11].ToString() == "")
                        {
                            Year = Etd1.Year;
                            Month = Etd1.Month;
                            Weekly = TrackingEDI.Business.DateTimeUtils.GetWeekOfYear(Etd1, DayOfWeek.Monday); 
                        }
                         
                        #endregion

                        #region 抓欄位敘述
                        //Incoterm;
                        //Sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE = 'TD' AND CD = {0} AND CMP = {1} AND GROUP_ID={2}", SQLUtils.QuotedStr(Incoterm), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(GroupId));
                        //string IncotermDescp = getOneValueAsStringFromSql(Sql);
                        //Carrier

                        string CarrierNm = GetTableValueByName(carrierDt, string.Format("CD={0}", SQLUtils.QuotedStr(Carrier)), "CD_DESCP");

                        //POL
                        CntyCd = PolCd.Substring(0, 2);
                        PortCd = PolCd.Substring(2, 3);
                        string PolNm = GetTableValueByName(cityDt, string.Format("CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(CntyCd), SQLUtils.QuotedStr(PortCd)), "PORT_NM");
                        string PolCnty = CntyCd;
                        //POD
                        CntyCd = PodCd.Substring(0, 2);
                        PortCd = PodCd.Substring(2, 3);
                        string PodNm = GetTableValueByName(cityDt, string.Format("CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(CntyCd), SQLUtils.QuotedStr(PortCd)), "PORT_NM");

                        Sql = string.Format("SELECT CNTRY_NM FROM BSCITY WHERE CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(CntyCd), SQLUtils.QuotedStr(PortCd));
                        string PodCnty = CntyCd;
                        //POR
                        CntyCd = PorCd.Substring(0, 2);
                        PortCd = PorCd.Substring(2, 3);
                        string PorNm = GetTableValueByName(cityDt, string.Format("CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(CntyCd), SQLUtils.QuotedStr(PortCd)), "PORT_NM");

                        Sql = string.Format("SELECT CNTRY_NM FROM BSCITY WHERE CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(CntyCd), SQLUtils.QuotedStr(PortCd));
                        string PorCnty = CntyCd;
                        //DEST
                        CntyCd = DestCd.Substring(0, 2);
                        PortCd = DestCd.Substring(2, 3);
                        string DestNm = GetTableValueByName(cityDt, string.Format("CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(CntyCd), SQLUtils.QuotedStr(PortCd)), "PORT_NM");

                        Sql = string.Format("SELECT CNTRY_NM FROM BSCITY WHERE CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(CntyCd), SQLUtils.QuotedStr(PortCd));
                        string DestCnty = CntyCd;
                        //Oexporter
                        //Sql = string.Format("SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(ShipperNo));
                        //string OexporterNm = getOneValueAsStringFromSql(Sql);
                        string OexporterNm = GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(ShipperNo)), "PARTY_NAME");
          
                        //Sql = string.Format("SELECT PART_ADDR1 FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(ShipperNo));
                        //string OexporterAddr = getOneValueAsStringFromSql(Sql);
                        string OexporterAddr = GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(ShipperNo)), "PART_ADDR1");

                        //TcImporter,Oimporter
                        //Sql = string.Format("SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(ConsigneeNo));
                        //string TcImporterNm = getOneValueAsStringFromSql(Sql);
                        string TcImporterNm = GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(ConsigneeNo)), "PARTY_NAME");

                        //Sql = string.Format("SELECT PART_ADDR1 FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(ConsigneeNo));
                        //string TcImporterAddr = getOneValueAsStringFromSql(Sql);
                        string TcImporterAddr = GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(ConsigneeNo)), "PART_ADDR1");

                        
                        //if (!string.IsNullOrEmpty(mapCmp))
                        //    secCmp = mapCmp;
                        #endregion

                        #region 判斷有無重複
                        //if (HouseNo == "")
                        //{
                        //    Sql = string.Format("SELECT * FROM SMSMI WHERE MASTER_NO={0}",SQLUtils.QuotedStr(MasterNo));
                        //}
                        //else
                        //{
                        //    Sql = string.Format("SELECT * FROM SMSMI WHERE MASTER_NO={1} AND HOUSE_NO={2}", SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(MasterNo), SQLUtils.QuotedStr(HouseNo));
                        //}
                        Sql = string.Format("SELECT * FROM SMSMI WHERE MASTER_NO={0}", SQLUtils.QuotedStr(MasterNo));
                        DataTable smsmidt = getDataTableFromSql(Sql);
                        string sql = string.Format("SELECT TOP 1 E_DAY FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND CARRIER_CD={2} AND I_TYPE IN ('DDEM','BOTH') AND FEE_PER_DAY=0", SQLUtils.QuotedStr(Location),
                            SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(Carrier));
                        int PortFreeTime = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //string tran = Prolink.Math.GetValueAsString(smsmdt.Rows[0]["TRAN_TYPE"]);
                        if (!"F".Equals(TranType))
                            PortFreeTime = 0;
                        if (smsmidt.Rows.Count > 0)
                        {
                            UId = smsmidt.Rows[0]["U_ID"].ToString();
                            ShipmentId = smsmidt.Rows[0]["SHIPMENT_ID"].ToString();
                            string Groupid = smsmidt.Rows[0]["GROUP_ID"].ToString();
                            string cmp = smsmidt.Rows[0]["CMP"].ToString();
                            string secCmp = GetTableValueByName(maDt, string.Format("DEST_CODE={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(DestCd), SQLUtils.QuotedStr(cmp)), "SEC_CMP");
                            string partyNo= GetTableValueByName(directDt, string.Format("PARTY_NO={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(ConsigneeNo), SQLUtils.QuotedStr(cmp)), "PARTY_NO");
                            if (!string.IsNullOrEmpty(partyNo))
                                secCmp = "";
                            TranType = smsmidt.Rows[0]["TRAN_TYPE"].ToString();
                            MasterNo = smsmidt.Rows[0]["MASTER_NO"].ToString();
                            HouseNo = smsmidt.Rows[0]["HOUSE_NO"].ToString();
                            CombineDn = smsmidt.Rows[0]["COMBINE_INFO"].ToString();
                            CombineInv = smsmidt.Rows[0]["INVOICE_INFO"].ToString();
                            CombineCargo = smsmidt.Rows[0]["PRODUCT_INFO"].ToString();
                            if (!"A".Equals(smsmidt.Rows[0]["STATUS"].ToString()))
                            {
                                returnMessage = "Master No :" + MasterNo + " is already exist and status is not unreach,So you cannot import it!";
                                return Json(new { message = returnMessage });
                            }
                            string[] CombineDn_A = CombineDn.Split(',');
                            string[] CombineInv_A = CombineInv.Split(',');
                            string[] CombineCargo_A = CombineCargo.Split(',');

                            #region UPDATE SMSMI
                            ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                            ei.PutKey("U_ID", UId);
                            ei.PutKey("GROUP_ID", Groupid);
                            ei.PutKey("CMP", cmp);
                            //ei.Put("STN", "*");
                            //ei.Put("DEP", "*");
                            ei.PutKey("TRAN_TYPE", TranType);
                            string extrasrv = SMSMIHelper.GetExtraSrvInfo(TranType, MasterNo, cmp);
                            if (!string.IsNullOrEmpty(extrasrv))
                            {
                                ei.Put("EXTRA_SRV", extrasrv);
                            }
                            ei.PutKey("SHIPMENT_ID", ShipmentId);
                            ei.PutKey("MASTER_NO", MasterNo);
                            ei.PutKey("HOUSE_NO", HouseNo);
                            foreach (string item in CombineDn_A)
                            {
                                if (item == "")
                                {
                                    ei.Put("COMBINE_INFO", InvoiceNo);
                                }
                                if (item != "" && item != InvoiceNo)
                                {
                                    ei.Put("COMBINE_INFO", CombineDn + "," + InvoiceNo);
                                }
                            }
                            CombineDn = CombineDn + "," + InvoiceNo;
                            string[] dns = CombineDn.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
                            CombineDn = dns.Length > 0 ? string.Join(",", dns) : string.Empty;
                            ei.Put("COMBINE_INFO", CombineDn);

                            CombineInv = CombineInv + "," + InvoiceNo;
                            CombineInv = string.Join(",", CombineInv.Split(',').Distinct().ToArray());
                            CombineCargo = CombineCargo + "," + CargoType;
                            CombineCargo = string.Join(",", CombineCargo.Split(',').Distinct().ToArray());
                            ei.Put("INVOICE_INFO", CombineInv);
                            ei.Put("PRODUCT_INFO", CombineCargo);
                            ei.Put("MARKS", Marks);
                            ei.Put("OUTSOURCING_TO_INBOUND", "N");
                            ei.Put("GOODS", Commodity);
                            ei.Put("BL_RMK", Blmark);
                            ei.Put("YEAR", Year);
                            ei.Put("MONTH", Month);
                            ei.Put("WEEKLY", Weekly);
                            ei.Put("GVALUE", Gvalue);
                            ei.Put("CUR", Cur);
                            ei.Put("CNT20", Cnt20);
                            ei.Put("CNT40", Cnt40);
                            ei.Put("CNT40HQ", Cnt40HQ);
                            ei.Put("CNT_TYPE", CntType);
                            ei.Put("CNT_NUMBER", CntNumber);
                            ei.Put("PKG_NUM", PkgNum);
                            ei.Put("PKG_UNIT", PkgUnit);
                            ei.Put("GW", Gw);
                            ei.Put("GWU", Gwu);
                            ei.Put("VW", Vw);
                            ei.Put("CBM", Cbm);
                            ei.Put("CW", Cw);
                            ei.Put("INCOTERM_CD", Incoterm);
                            ei.Put("TRADE_TERM", Incoterm);
                            ei.Put("INCOTERM_DESCP", IncotermDescp);
                            ei.Put("TRADETERM_DESCP", IncotermDescp);
                            ei.Put("LOADING_FROM", LoadingFrom);
                            ei.Put("LOADING_TO", LoadingTo);
                            ei.Put("CARRIER", Carrier);
                            ei.Put("CARRIER_NM", CarrierNm);
                            ei.Put("HTS_CODE", HtsCode);
                            ei.Put("COUNTRY", Country);
                            ei.Put("SCAC_CD", ScacCd);
                            ei.Put("FRT_TERM", FreightTerm);
                            ei.Put("POR_CD", PorCd);
                            ei.Put("POR_NAME", PorNm);
                            ei.Put("POR_CNTY", PorCnty);
                            ei.Put("POL_CD", PolCd);
                            ei.Put("POL_NAME", PolNm);
                            ei.Put("POL_CNTY", PolCnty);
                            ei.Put("POD_CD", PodCd);
                            ei.Put("POD_NAME", PodNm);
                            ei.Put("POD_CNTY", PodCnty);
                            ei.Put("DEST_CD", DestCd);
                            ei.Put("DEST_NAME", DestNm);
                            ei.Put("DEST_CNTY", DestCnty);
                            ei.Put("DEST_REGION", DestRegion);
                            ei.Put("VESSEL1", Vessel1);
                            ei.Put("VOYAGE1", Voyage1);
                            ei.Put("VESSEL2", Vessel2);
                            ei.Put("VOYAGE2", Voyage2);
                            ei.Put("VESSEL3", Vessel3);
                            ei.Put("VOYAGE3", Voyage3);
                            ei.Put("VESSEL4", Vessel4);
                            ei.Put("VOYAGE4", Voyage4);
                            ei.PutDate("ETD", Etd1);
                            ei.PutDate("ETA", eta);
                            ei.Put("ETA_WK", TrackingEDI.Business.DateTimeUtils.GetWeekOfYear(eta, DayOfWeek.Monday));
                            ei.PutDate("ETD1", Etd1);
                            ei.PutDate("ETA1", Eta1);
                            ei.PutDate("ETD2", Etd2);
                            ei.PutDate("ETA2", Eta2);
                            ei.PutDate("ETD3", Etd3);
                            ei.PutDate("ETA3", Eta3);
                            ei.PutDate("ETD4", Etd4);
                            ei.PutDate("ETA4", Eta4);
                            ei.PutDate("ATD", Atd1);
                            ei.PutDate("ATA", Ata);
                            ei.Put("OEXPORTER", ShipperNo);
                            ei.Put("TC_IMPORTER", ConsigneeNo);
                            ei.Put("OIMPORTER", ConsigneeNo);
                            ei.Put("OEXPORTER_NM", OexporterNm);
                            ei.Put("OEXPORTER_ADDR", OexporterAddr);
                            ei.Put("TC_IMPORTER_NM", TcImporterNm);
                            ei.Put("TC_IMPORTER_ADDR", TcImporterAddr);
                            ei.Put("OIMPORTER_NM", TcImporterNm);
                            ei.Put("OIMPORTER_ADDR", TcImporterAddr);
                            ei.Put("TRAN_TYPE1", "T");
                            //ei.Put("POL1", PodCd);
                            //ei.Put("POL_NM1", PodNm);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, cmp);
                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                            ei.Put("PORT_FREE_TIME", PortFreeTime);
                            ei.Put("SEC_CMP", DBNull.Value);
                            if (!string.IsNullOrEmpty(secCmp))
                                ei.Put("SEC_CMP", secCmp);
                            //ei.Put("CNTR_INFO", CombineValue(CntrNo, Prolink.Math.GetValueAsString(smsmidt.Rows[0]["CNTR_INFO"])));
                            SetPartyToSMSMI(smptyDt, ei, ShipperNo, ConsigneeNo, NotifyNo, ShipToNo, FiCustomer,SubBG);
                            ml.Add(ei);
                            #endregion

                            //Sql = string.Format("DELETE FROM SMSMI WHERE U_ID = {0} AND SHIPMENT_ID = {1}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(ShipmentId));
                            //ml2.Add(Sql);
                            Sql = string.Format("DELETE FROM SMSMIPT WHERE U_FID = {0} AND SHIPMENT_ID = {1}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(ShipmentId));
                            ml2.Add(Sql);
                            //刪除
                            int[] result = OperationUtils.ExecuteUpdate(ml2, Prolink.Web.WebContext.GetInstance().GetConnection());

                        }
                        else
                        {
                            UId = System.Guid.NewGuid().ToString();
                            //string autocmp = this.BaseCompanyId;
                            //if (IOFlag == "I")
                            //{
                            //    autocmp = CompanyId;
                            //}
                            string autocmp = Location.Trim();  //根据汇入的时候的location来创建ShipmentID号码

                            ShipmentId = Business.ReserveManage.getAutoNo("SHIB_NO", GroupId, autocmp);
                            DateTime odt = DateTime.Now;
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, autocmp);
                            #region INSERT SMSMI
                            ei = new EditInstruct("SMSMI", EditInstruct.INSERT_OPERATION);
                            ei.Put("STATUS", "A");
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", Location);
                            string secCmp = GetTableValueByName(maDt, string.Format("DEST_CODE={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(DestCd), SQLUtils.QuotedStr(Location)), "SEC_CMP");
                            string partyNo = GetTableValueByName(directDt, string.Format("PARTY_NO={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(ConsigneeNo), SQLUtils.QuotedStr(Location)), "PARTY_NO");
                            if (!string.IsNullOrEmpty(partyNo))
                                secCmp = "";
                            //ei.Put("STN", "*");
                            //ei.Put("DEP", "*");
                            ei.Put("TRAN_TYPE", TranType);
                            string extrasrv = SMSMIHelper.GetExtraSrvInfo(TranType, MasterNo, Location);
                            if (!string.IsNullOrEmpty(extrasrv))
                            {
                                ei.Put("EXTRA_SRV", extrasrv);
                            }
                            ei.Put("SHIPMENT_ID", ShipmentId);
                            ei.Put("MASTER_NO", MasterNo);
                            ei.Put("HOUSE_NO", HouseNo);
                            ei.Put("COMBINE_INFO", InvoiceNo);
                            ei.Put("INVOICE_INFO", InvoiceNo);
                            ei.Put("PRODUCT_INFO", CargoType);
                            ei.Put("MARKS", Marks);
                            ei.Put("GOODS", Commodity);
                            ei.Put("BL_RMK", Blmark);
                            ei.Put("YEAR", Year);
                            ei.Put("MONTH", Month);
                            ei.Put("WEEKLY", Weekly);
                            ei.Put("GVALUE", Gvalue);
                            ei.Put("OUTSOURCING_TO_INBOUND", "N");
                            ei.Put("CUR", Cur);
                            ei.Put("CNT20", Cnt20);
                            ei.Put("CNT40", Cnt40);
                            ei.Put("CNT40HQ", Cnt40HQ);
                            ei.Put("CNT_TYPE", CntType);
                            ei.Put("CNT_NUMBER", CntNumber);
                            ei.Put("PKG_NUM", PkgNum);
                            ei.Put("PKG_UNIT", PkgUnit);
                            ei.Put("GW", Gw);
                            ei.Put("GWU", Gwu);
                            ei.Put("VW", Vw);
                            ei.Put("CBM", Cbm);
                            ei.Put("CW", Cw);
                            ei.Put("INCOTERM_CD", Incoterm);
                            ei.Put("TRADE_TERM", Incoterm);
                            ei.Put("INCOTERM_DESCP", IncotermDescp);
                            ei.Put("TRADETERM_DESCP", IncotermDescp);
                            ei.Put("LOADING_FROM", LoadingFrom);
                            ei.Put("LOADING_TO", LoadingTo);
                            ei.Put("CARRIER", Carrier);
                            ei.Put("CARRIER_NM", CarrierNm);
                            ei.Put("HTS_CODE", HtsCode);
                            ei.Put("COUNTRY", Country);
                            ei.Put("SCAC_CD", ScacCd);
                            ei.Put("FRT_TERM", FreightTerm);
                            ei.Put("POR_CD", PorCd);
                            ei.Put("POR_NAME", PorNm);
                            ei.Put("POR_CNTY", PorCnty);
                            ei.Put("POL_CD", PolCd);
                            ei.Put("POL_NAME", PolNm);
                            ei.Put("POL_CNTY", PolCnty);
                            ei.Put("POD_CD", PodCd);
                            ei.Put("POD_NAME", PodNm);
                            ei.Put("POD_CNTY", PodCnty);
                            ei.Put("DEST_CD", DestCd);
                            ei.Put("DEST_NAME", DestNm);
                            ei.Put("DEST_CNTY", DestCnty);
                            ei.Put("DEST_REGION", DestRegion);
                            ei.Put("VESSEL1", Vessel1);
                            ei.Put("VOYAGE1", Voyage1);
                            ei.Put("VESSEL2", Vessel2);
                            ei.Put("VOYAGE2", Voyage2);
                            ei.Put("VESSEL3", Vessel3);
                            ei.Put("VOYAGE3", Voyage3);
                            ei.Put("VESSEL4", Vessel4);
                            ei.Put("VOYAGE4", Voyage4);
                            ei.PutDate("ETD", Etd1);
                            ei.PutDate("ETA", eta);
                            ei.Put("ETA_WK", TrackingEDI.Business.DateTimeUtils.GetWeekOfYear(eta, DayOfWeek.Monday));
                            ei.PutDate("ETD1", Etd1);
                            ei.PutDate("ETA1", Eta1);
                            ei.PutDate("ETD2", Etd2);
                            ei.PutDate("ETA2", Eta2);
                            ei.PutDate("ETD3", Etd3);
                            ei.PutDate("ETA3", Eta3);
                            ei.PutDate("ETD4", Etd4);
                            ei.PutDate("ETA4", Eta4);
                            ei.PutDate("ATD", Atd1);
                            ei.PutDate("ATA", Ata);
                            ei.Put("OEXPORTER", ShipperNo);
                            ei.Put("TC_IMPORTER", ConsigneeNo);
                            ei.Put("OIMPORTER", ConsigneeNo);
                            ei.Put("OEXPORTER_NM", OexporterNm);
                            ei.Put("OEXPORTER_ADDR", OexporterAddr);
                            ei.Put("TC_IMPORTER_NM", TcImporterNm);
                            ei.Put("TC_IMPORTER_ADDR", TcImporterAddr);
                            ei.Put("OIMPORTER_NM", TcImporterNm);
                            ei.Put("OIMPORTER_ADDR", TcImporterAddr);
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                            ei.Put("CREATE_BY", UserId);
                            ei.Put("PORT_FREE_TIME", PortFreeTime);
                            if (!string.IsNullOrEmpty(secCmp))
                                ei.Put("SEC_CMP", secCmp);
                            //ei.Put("CNTR_INFO", CntrNo);
                            SetPartyToSMSMI(smptyDt, ei, ShipperNo, ConsigneeNo, NotifyNo, ShipToNo, FiCustomer, SubBG);
                            ml.Add(ei);
                            #endregion
                        }
                        #endregion
                        if (!shipmentList.Contains(ShipmentId))
                        {
                            shipmentList.Add(ShipmentId);
                            ml.Add(string.Format("DELETE FROM SMIDNP WHERE SHIPMENT_ID = {0} ", SQLUtils.QuotedStr(ShipmentId)));
                        }
                        List<string> dnList = new List<string>();
                        if (!shipmentDic.ContainsKey(ShipmentId) && !string.IsNullOrEmpty(InvoiceNo))
                        {
                            dnList.Add(InvoiceNo);
                            shipmentDic.Add(ShipmentId, dnList);
                        }
                        else if (shipmentDic.ContainsKey(ShipmentId))
                        {
                            dnList = shipmentDic[ShipmentId];
                            if (!dnList.Contains(InvoiceNo))
                            {
                                dnList.Add(InvoiceNo);
                                shipmentDic[ShipmentId] = dnList;
                            }
                            //shipmentDic.Add(ShipmentId, dnList);
                        }

                        Sql = string.Format("SELECT * FROM SMIDN WHERE INV_NO = {0}", SQLUtils.QuotedStr(InvoiceNo));
                        DataTable smidndt = getDataTableFromSql(Sql);

                        sql = string.Format(@"SELECT TOP 1 TAX_NO FROM SMPTY WHERE PARTY_NO={0}",
                                SQLUtils.QuotedStr(ConsigneeNo));
                        string DnImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (smidndt.Rows.Count > 0)
                        {
                            string shipment = smidndt.Rows[0]["SHIPMENT_ID"].ToString();
                            string groupid = smidndt.Rows[0]["GROUP_ID"].ToString();
                            string cmp = smidndt.Rows[0]["CMP"].ToString();
                            Sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID = {0} AND MASTER_NO = {1}", SQLUtils.QuotedStr(shipment), SQLUtils.QuotedStr(MasterNo));
                            DataTable checkdt = getDataTableFromSql(Sql);
                            if (checkdt.Rows.Count > 0)
                            {

                                #region UPDATE DN主檔
                                ei2 = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                                Dn_UId = smidndt.Rows[0]["U_ID"].ToString();
                                ei2.PutKey("U_ID", Dn_UId);
                                ei2.PutKey("GROUP_ID", groupid);
                                ei2.PutKey("CMP", cmp);
                                //ei2.Put("STN", "*");
                                //ei2.Put("DEP", "*");
                                ei2.PutKey("SHIPMENT_ID", shipment);
                                ei2.PutKey("INV_NO", InvoiceNo);
                                //ei2.Put("DN_NO", InvoiceNo);
                                ei2.Put("INV_PRICE", InvPrice);
                                ei2.Put("INV_CUR", InvCur);
                                ei2.Put("IMPORT_NO", DnImportNo);
                                ei2.Put("TC_IMPORT_NO", DnImportNo);
                                if (TranType == "F")
                                {
                                    string Insert = smidndt.Rows[0]["INSERT_NO"].ToString();
                                    if (InsertNo == Insert)
                                    {
                                        decimal DnNw = Prolink.Math.GetValueAsDecimal(smidndt.Rows[0]["NW"]);
                                        decimal DnGw = Prolink.Math.GetValueAsDecimal(smidndt.Rows[0]["GW"]);
                                        decimal DnCbm = Prolink.Math.GetValueAsDecimal(smidndt.Rows[0]["CBM"]);
                                        decimal DnQty = Prolink.Math.GetValueAsDecimal(smidndt.Rows[0]["QTY"]);
                                        decimal DnPkgnum = Prolink.Math.GetValueAsDecimal(smidndt.Rows[0]["PKG_NUM"]);
                                        Dn_Nw = DnNw + Dn_Nw;
                                        Dn_Gw = DnGw + Dn_Gw;
                                        Dn_Cbm = DnCbm + Dn_Cbm;
                                        Dn_Qty = DnQty + Dn_Qty;
                                        Dn_PkgNum = DnPkgnum + Dn_PkgNum;
                                    }
                                }
                                ei2.Put("NW", Dn_Nw);
                                ei2.Put("GW", Dn_Gw);
                                ei2.Put("GWU", Dn_Gwu);
                                ei2.Put("CBM", Dn_Cbm);
                                ei2.Put("CBMU", Dn_Cbmu);
                                ei2.Put("QTY", Dn_Qty);
                                ei2.Put("QTYU", Dn_Qtyu);
                                ei2.Put("PKG_NUM", Dn_PkgNum);
                                ei2.Put("PKG_UNIT", Dn_PkgUnit);
                                ei2.Put("PKG_UNIT_DESC", Dn_PkgUnitDesc);
                                if (TranType == "A" || TranType == "L" || TranType == "E")
                                {
                                    ei2.Put("DIVISION_DESCP", CargoType);
                                    ei2.Put("PRIORITY", "2");
                                }
                                ei2.Put("INSERT_NO", InsertNo);
                                ei2.Put("CNTRY_CD", PodCnty);
                                ei2.Put("TC_CNTRY_CD", PodCnty);
                                ml.Add(ei2);
                                #endregion
                            }
                            else
                            {
                                Sql = string.Format("DELETE FROM SMIDN WHERE U_ID = {0} AND SHIPMENT_ID = {1} AND GROUP_ID = {2} AND CMP = {3} AND INV_NO = {4}", SQLUtils.QuotedStr(Dn_UId), SQLUtils.QuotedStr(shipment), SQLUtils.QuotedStr(groupid), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(InvoiceNo));
                                ml2.Add(Sql);
                                int[] results = OperationUtils.ExecuteUpdate(ml2, Prolink.Web.WebContext.GetInstance().GetConnection());
                                returnMessage = "Invoice No :" + InvoiceNo + " is already exist";
                                return Json(new { message = returnMessage });
                            }
                        }
                        else
                        {
                            #region INSERT DN主檔
                            ei2 = new EditInstruct("SMIDN", EditInstruct.INSERT_OPERATION);
                            Dn_UId = System.Guid.NewGuid().ToString();
                            ei2.Put("U_ID", Dn_UId);
                            ei2.Put("GROUP_ID", GroupId);
                            ei2.Put("CMP", Location);
                            ei2.Put("STN", "*");
                            ei2.Put("DEP", "*");
                            ei2.Put("SHIPMENT_ID", ShipmentId);
                            ei2.Put("DN_NO", InvoiceNo);
                            ei2.Put("INV_NO", InvoiceNo);
                            ei2.Put("INV_PRICE", InvPrice);
                            ei2.Put("INV_CUR", InvCur);
                            ei2.Put("NW", Dn_Nw);
                            ei2.Put("GW", Dn_Gw);
                            ei2.Put("GWU", Dn_Gwu);
                            ei2.Put("CBM", Dn_Cbm);
                            ei2.Put("CBMU", Dn_Cbmu);
                            ei2.Put("QTY", Dn_Qty);
                            ei2.Put("QTYU", Dn_Qtyu);
                            ei2.Put("PKG_NUM", Dn_PkgNum);
                            ei2.Put("PKG_UNIT", Dn_PkgUnit);
                            ei2.Put("PKG_UNIT_DESC", Dn_PkgUnitDesc);
                            ei2.Put("IMPORT_NO", DnImportNo);
                            ei2.Put("TC_IMPORT_NO", DnImportNo);
                            if (TranType == "A" || TranType == "L" || TranType == "E")
                            {
                                ei2.Put("DIVISION_DESCP", CargoType);
                                ei2.Put("PRIORITY", "2");
                            }
                            ei2.Put("INSERT_NO", InsertNo);
                            ei2.Put("CNTRY_CD", PodCnty);
                            ei2.Put("TC_CNTRY_CD", PodCnty);
                            ml.Add(ei2);
                            #endregion
                        }


                        #region INSERT DN細檔
                        ei3 = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
                        Dnp_UId = System.Guid.NewGuid().ToString();
                        ei3.Put("U_ID", Dnp_UId);
                        ei3.Put("U_FID", Dn_UId);
                        ei3.Put("SHIPMENT_ID", ShipmentId);
                        ei3.Put("DN_NO", InvoiceNo);
                        ei3.Put("INV_NO", InvoiceNo);
                        if (TranType == "F" || TranType == "R")
                        {
                            ei3.Put("CNTR_NO", CntrNo);
                        }
                        ei3.Put("IPART_NO", IpartNo);
                        if (!string.IsNullOrEmpty(OpartNo))
                        {
                            List<string> opartList = new List<string>();
                            if (!opartDic.ContainsKey(ShipmentId))
                            {
                                opartList.Add(OpartNo);
                                opartDic.Add(ShipmentId, opartList);
                            }
                            else
                            {
                                opartList = opartDic[ShipmentId];
                                if (!opartList.Contains(OpartNo))
                                {
                                    opartList.Add(OpartNo);
                                    opartDic[ShipmentId] = opartList;
                                }
                            }
                            ei3.Put("OPART_NO", OpartNo);
                            ei3.Put("PART_NO", OpartNo);
                        }
                        if (!string.IsNullOrEmpty(PoNo))
                        {
                            List<string> poList = new List<string>();
                            if (!poDic.ContainsKey(ShipmentId))
                            {
                                poList.Add(PoNo);
                                poDic.Add(ShipmentId, poList);
                            }
                            else
                            {
                                poList = poDic[ShipmentId];
                                if (!poList.Contains(PoNo))
                                {
                                    poList.Add(PoNo);
                                    poDic[ShipmentId] = poList;
                                }
                            }
                            ei3.Put("PO_NO", PoNo);
                        }
                        ei3.Put("CNTR_NO", CntrNo);
                        ei3.Put("QTY", Dnp_Qty);
                        ei3.Put("GW", Dnp_Gw);
                        ei3.Put("CBM", Dnp_Cbm);

                        //InboundHelper.SetPartAndAsn(ref asnDate, InvoiceNo, ei3, OpartNo, Prolink.Math.GetValueAsInt(Dnp_Qty));

                        ml.Add(ei3);
                        #endregion


                        Sql = "SELECT * FROM SMICNTR WHERE SHIPMENT_ID = {0} AND CNTR_NO = {1}";
                        Sql = string.Format(Sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(CntrNo));
                        DataTable cntrdt = getDataTableFromSql(Sql);
                        if (cntrdt.Rows.Count > 0)
                        {
                            string InvNos = InvoiceNo;
                            string[] Dns = cntrdt.Rows[0]["DN_NO"].ToString().Split(',');
                            foreach (string dn in Dns)
                            {
                                if (dn != InvoiceNo)
                                {
                                    InvNos = InvNos + "," + dn;
                                }
                            }
                            #region UPDATE 貨櫃檔
                            ei4 = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                            //Cntr_UId = System.Guid.NewGuid().ToString();
                            //ei4.Put("U_ID", Cntr_UId);
                            ei4.PutKey("CMP", CompanyId);
                            ei4.PutKey("SHIPMENT_ID", ShipmentId);
                            ei4.PutKey("CNTR_NO", CntrNo);
                            ei4.Put("DN_NO", InvNos);
                            ei4.Put("CNTR_TYPE", CntrType);
                            ei4.Put("SEAL_NO1", SealNo1);
                            ei4.Put("CMP", Location);
                            ei4.Put("IMPORT_NO", DnImportNo);
                            ei4.Put("TC_IMPORT_NO", DnImportNo);
                            if (TranType == "F" || TranType == "R")
                            {
                                ei4.Put("DIVISION_DESCP", CargoType);
                                ei4.Put("TRAN_TYPE1", "T");
                                ei4.Put("POL1", PodCd);
                                ei4.Put("POL_NM1", PodNm);
                                ei4.Put("PRIORITY", "2");
                                ei4.Put("BACK_LOCATION", PodCd);
                            }
                            ei4.Put("CNTRY_CD", PodCnty);
                            ei4.Put("TC_CNTRY_CD", PodCnty);
                            ml.Add(ei4);
                            #endregion
                        }
                        else
                        {
                            #region INSERT 貨櫃檔
                            ei4 = new EditInstruct("SMICNTR", EditInstruct.INSERT_OPERATION);
                            Cntr_UId = System.Guid.NewGuid().ToString();
                            ei4.Put("U_ID", Cntr_UId);
                            ei4.Put("CMP", CompanyId);
                            ei4.Put("SHIPMENT_ID", ShipmentId);
                            ei4.Put("DN_NO", InvoiceNo);
                            ei4.Put("CNTR_NO", CntrNo);
                            ei4.Put("CNTR_TYPE", CntrType);
                            ei4.Put("SEAL_NO1", SealNo1);
                            ei4.Put("CMP", Location);
                            ei4.Put("IMPORT_NO", DnImportNo);
                            ei4.Put("TC_IMPORT_NO", DnImportNo);
                            if (TranType == "F" || TranType == "R")
                            {
                                ei4.Put("DIVISION_DESCP", CargoType);
                                ei4.Put("TRAN_TYPE1", "T");
                                ei4.Put("POL1", PodCd);
                                ei4.Put("POL_NM1", PodNm);
                                ei4.Put("PRIORITY", "2");
                                ei4.Put("BACK_LOCATION", PodCd);
                            }
                            ei4.Put("CNTRY_CD", PodCnty);
                            ei4.Put("TC_CNTRY_CD", PodCnty);
                            ml.Add(ei4);
                            #endregion
                        }


                        //寫SMSMIPT檔
                        UploadSmsmipt(UId, ShipmentId, "SISF", IsfSeller);
                        UploadSmsmipt(UId, ShipmentId, "IBBK", BookingParty);
                        UploadSmsmipt(UId, ShipmentId, "SH", ShipperNo);
                        UploadSmsmipt(UId, ShipmentId, "CS", ConsigneeNo);
                        UploadSmsmipt(UId, ShipmentId, "NT", NotifyNo);
                        UploadSmsmipt(UId, ShipmentId, "WE", ShipToNo);
                        UploadSmsmipt(UId, ShipmentId, "FC", FiCustomer);
                        UploadSmsmipt(UId, ShipmentId, "ZT", SubBG);
                        UploadSmsmipt(UId, ShipmentId, "RE", Importer);
                        UploadSmsmipt(UId, ShipmentId, "SP", HandlingAgen);

                        if (ml.Count > 0)
                        {
                            int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            DataTable smidnpDt= OperationUtils.GetDataTable(string.Format("SELECT PART_NO,QTY from SMIDNP WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            List<string> partnolist = new List<string>();
                            List<int> qtylist = new List<int>();
                            int sumqty = 0;
                            foreach (DataRow smidnpdr in smidnpDt.Rows)
                            {
                                partnolist.Add(Prolink.Math.GetValueAsString(smidnpdr["PART_NO"]));
                                qtylist.Add(Prolink.Math.GetValueAsInt(smidnpdr["QTY"]));
                                sumqty += Prolink.Math.GetValueAsInt(smidnpdr["QTY"]);
                            }
                            string partnoInfo = string.Join(",", partnolist);
                            if (partnoInfo.Length > 500)
                                partnoInfo = partnoInfo.Substring(0, 500);
                            string modelQty = string.Join(",", qtylist);
                            EditInstruct smei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                            smei.PutKey("SHIPMENT_ID", ShipmentId);
                            smei.Put("PARTNO_INFO", partnoInfo);
                            smei.Put("PART_QTY", modelQty);
                            smei.Put("QTY", sumqty);
                            smei.PutExpress("QTYU", "(SELECT TOP 1 QTYU FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMSMI.SHIPMENT_ID)");
                            results = OperationUtils.ExecuteUpdate(smei, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }

                        SetCntrProd(UId, ShipmentId);
                        InboundHelper.InboundAllcByShipment(ShipmentId);
                        InboundHelper.SetPartyToIBCRByShipID(ShipmentId);

                        TrackingEDI.InboundBusiness.ASNManager.SetAsnByShipmentid(ShipmentId);
                        try
                        {
                            BookingParser bp = new BookingParser();
                            bp.SaveToTrackingByIBShipmentid(new string[] { ShipmentId });
                            Manager.IBSaveStatus(new Status() { ShipmentId = ShipmentId, StsCd = "000", Cmp = CompanyId, Sender = UserId, Location = PodCd, LocationName = "", StsDescp = "Init Booking Info." });
                        }catch(Exception ex){
                        }
                    }

                    string smicuft_sql = string.Format("SELECT * FROM SMICUFT WHERE DN_NO IN {0}", SQLUtils.Quoted(InvNoList.ToArray()));
                    DataTable smicuftDt = OperationUtils.GetDataTable(smicuft_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    MixedList ml3 = new MixedList();
                    foreach (string shipment in shipmentList)
                    {
                        bool isUpdate = false;
                        EditInstruct smei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        smei.PutKey("SHIPMENT_ID", shipment);
                        if (shipmentDic.ContainsKey(shipment))
                        {
                            DataRow[] drs = smicuftDt.Select(string.Format("DN_NO IN {0}", SQLUtils.Quoted(shipmentDic[shipment].ToArray())));
                            List<string> cuftList = new List<string>();
                            foreach (DataRow cuft in drs)
                            {
                                cuftList.Add(string.Format("{0}:{1}*{2}*{3}", cuft["DN_NO"], cuft["L"], cuft["W"], cuft["H"]));
                            }
                            if (cuftList.Count > 0)
                            {
                                isUpdate = true;
                                smei.Put("DIMENSIONS_INFO", string.Join(";", cuftList));
                            }
                        }
                        if (opartDic.ContainsKey(shipment))
                        {
                            isUpdate = true;
                            string opartInfo = string.Join(",", opartDic[shipment]);
                            if (opartInfo.Length > 200)
                                opartInfo=opartInfo.Substring(0, 200);
                            smei.Put("EXRERNAL_INFO", opartInfo);
                        }
                        if (poDic.ContainsKey(shipment))
                        {
                            isUpdate = true;
                            string poInfo = string.Join(",", poDic[shipment]);
                            if (poInfo.Length > 500)
                                poInfo = poInfo.Substring(0, 500);
                            smei.Put("PONO_INFO", poInfo);
                        }
                        if(isUpdate)
                            ml3.Add(smei);
                    }
                    
                    if (ml3.Count > 0)
                    {
                        int[] results = OperationUtils.ExecuteUpdate(ml3, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
                catch (Exception ex)
                {
                    //ermsg += DnNo + ":" + ex.Message + "\n";
                    ermsg = "Import Shipment Error" + ex.ToString();
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog("Import Shipment Error :" + ex.ToString());
                    return Json(new { message = ermsg });
                }
            }
            return Json(new { message = returnMessage });
        }

        [HttpPost]
        public ActionResult UploadSmsmiNew(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "upload error:doesn't recevie any excel file!" });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                return Json(new { message = "upload error:There's nothing in the excel file!" });
            }
            string strExt = System.IO.Path.GetExtension(file.FileName);
            strExt = strExt.Trim('.');
            string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadSmsmi\\");
            string excelFileName = string.Format("{0}{1}.{2}", dirpath, DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
            file.SaveAs(excelFileName);

            int starRow = 0;
            string mapping = InboundUploadExcelManager.InboundSMSMIMapping;
            MixedList ml = new MixedList();
            MixedList mlist = new MixedList();
            Dictionary<string, object> parm = new Dictionary<string, object>();
            MixedList itemlist = new MixedList();
            Dictionary<string, string> masternoDic = new Dictionary<string, string>();
            Dictionary<string, string> invnoDic = new Dictionary<string, string>();
            Dictionary<string, string> cntrDic = new Dictionary<string, string>();
            Dictionary<string, InboundUploadExcelManager.ShipmentFreeInfo> freeDic = new Dictionary<string, InboundUploadExcelManager.ShipmentFreeInfo>();
            List<string> shipmentList = new List<string>();
            parm["mixedlist"] = itemlist;
            parm["masternoDic"] = masternoDic;
            parm["invnoDic"] = invnoDic;
            parm["GroupId"] = GroupId;
            parm["shipmentList"] = shipmentList;
            parm["UserId"] = UserId;
            parm["cntrDic"] = cntrDic;
            parm["freeDic"] = freeDic;
            parm["dnDimensionFlag"] = false;
            try
            {
                ExcelParser.RegisterEditInstructFunc(mapping, InboundUploadExcelManager.HandleCreateSMSMIInfo);
                ExcelParser ep = new ExcelParser();
                ep.Save(mapping, excelFileName, ml, parm, starRow);
                itemlist = (MixedList)parm["mixedlist"];
                for (int i = 0; i < itemlist.Count; i++)
                {
                    ml.Add((EditInstruct)itemlist[i]);
                }
                shipmentList = (List<string>)parm["shipmentList"];

                if (ml.Count <= 0)
                {
                    returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                }

                StringBuilder checklength = new StringBuilder();
                for (int i=0;i<ml.Count;i++)
                {
                    var item = ml[i];
                    if(item.GetType() == typeof(EditInstruct))
                    {
                        EditInstruct eiitem = (EditInstruct)item;
                        string[] allfields = eiitem.getNameSet();
                        string tableid = eiitem.ID;
                        foreach (string fieldname in allfields)
                        {
                            TrackingEDI.Business.XmlParser.GetDbValueChecklength(tableid, fieldname, eiitem.Get(fieldname), checklength);
                        }
                    }
                }
                if (checklength.Length > 0)
                {
                    throw new Exception(string.Format("Import Error: {0} ", checklength.ToString()));
                }
                CreateSmicuft(parm, excelFileName, ml);
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                string smsmisql = string.Format("SELECT M.U_ID,M.SHIPMENT_ID,M.POD_CD,M.POD_NAME FROM SMSMI M WHERE M.SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentList.ToArray()));
                DataTable smsmidatatable = OperationUtils.GetDataTable(smsmisql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                MixedList updatesmsmiml = new MixedList();
                foreach (string shipmentid in shipmentList)
                {
                    DataRow[] drs = smsmidatatable.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)));
                    if (drs.Length > 0)
                    {
                        SetCntrProd(drs[0]["U_ID"].ToString(), shipmentid);
                        InboundHelper.InboundAllcByShipment(shipmentid);
                        InboundHelper.SetPartyToIBCRByShipID(shipmentid);
                        ASNManager.SetAsnMapToSMIDNP(shipmentid);
                        ASNManager.SetAsnByShipmentid(shipmentid);

                        string smindp = string.Format("SELECT DISTINCT PO_NO + ',' FROM SMIDNP WHERE SHIPMENT_ID={0} FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                        string ponoinfo = OperationUtils.GetValueAsString(smindp, Prolink.Web.WebContext.GetInstance().GetConnection());
                        DataTable smidnpDt = OperationUtils.GetDataTable(string.Format("SELECT OPART_NO,PART_NO,QTY from SMIDNP WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        List<string> partnolist = new List<string>();
                        List<int> qtylist = new List<int>();
                        int sumqty = 0;
                        List<string> opartnolist = new List<string>();
                        foreach (DataRow smidnpdr in smidnpDt.Rows)
                        {
                            partnolist.Add(Prolink.Math.GetValueAsString(smidnpdr["PART_NO"]));
                            qtylist.Add(Prolink.Math.GetValueAsInt(smidnpdr["QTY"]));
                            sumqty += Prolink.Math.GetValueAsInt(smidnpdr["QTY"]);
                            string opartno = Prolink.Math.GetValueAsString(smidnpdr["OPART_NO"]);
                            if (!opartnolist.Contains(opartno))
                                opartnolist.Add(opartno);
                        }
                        string partnoInfo = string.Join(",", partnolist);
                        if (partnoInfo.Length > 500)
                            partnoInfo = partnoInfo.Substring(0, 500);
                        string modelQty = string.Join(",", qtylist);
                        if (ponoinfo.Length > 500)
                            ponoinfo = ponoinfo.Substring(0, 500);

                        List<string> combineinfo = new List<string>();
                        List<string> invoiceinfo = new List<string>();
                        List<string> productinfo = new List<string>();
                        DataTable smidninfodt = OperationUtils.GetDataTable(string.Format("SELECT DN_NO,INV_NO,DIVISION_DESCP,QTYU FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string sumqtyu = string.Empty;
                        foreach (DataRow smidndr in smidninfodt.Rows)
                        {
                            combineinfo.Add(Prolink.Math.GetValueAsString(smidndr["DN_NO"]));
                            invoiceinfo.Add(Prolink.Math.GetValueAsString(smidndr["INV_NO"]));
                            string divisiondescp = Prolink.Math.GetValueAsString(smidndr["DIVISION_DESCP"]);
                            if (!productinfo.Contains(divisiondescp))
                                productinfo.Add(divisiondescp);
                            if (string.IsNullOrEmpty(sumqtyu))
                            {
                                sumqtyu = Prolink.Math.GetValueAsString(smidndr["QTYU"]);
                            }
                        }

                        EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        smsmiei.PutKey("SHIPMENT_ID", shipmentid);
                        smsmiei.Put("PARTNO_INFO", partnoInfo);
                        smsmiei.Put("PART_QTY", modelQty);
                        smsmiei.Put("PONO_INFO", ponoinfo);
                        string externale = string.Join(",", opartnolist);
                        if (externale.Length > 200)
                            externale = externale.Substring(0, 200);
                        smsmiei.Put("EXRERNAL_INFO", externale);
                        smsmiei.Put("COMBINE_INFO", string.Join(",", combineinfo));
                        smsmiei.Put("INVOICE_INFO", string.Join(",", invoiceinfo));
                        smsmiei.Put("PRODUCT_INFO", string.Join(",", productinfo));
                        smsmiei.Put("QTYU", sumqtyu);
                        smsmiei.Put("QTY", sumqty);
                        updatesmsmiml.Add(smsmiei);

                        EditInstruct smidnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                        smidnei.PutExpress("QTY", "(SELECT SUM(QTY) FROM SMIDNP WHERE SMIDNP.SHIPMENT_ID = SMIDN.SHIPMENT_ID AND SMIDNP.DN_NO = SMIDN.DN_NO)");
                        smidnei.PutKey("SHIPMENT_ID", shipmentid);
                        updatesmsmiml.Add(smidnei);
                        try
                        {
                            BookingParser bp = new BookingParser();
                            bp.SaveToTrackingByIBShipmentid(new string[] { shipmentid });
                            Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "000", Cmp = CompanyId, Sender = UserId, Location = drs[0]["POD_CD"].ToString(), LocationName = "", StsDescp = "Init Booking Info." });
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                }
                if (updatesmsmiml.Count > 0)
                {
                    try
                    {
                        result = OperationUtils.ExecuteUpdate(updatesmsmiml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex) { }
                }

            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
                return Json(new { message = ex.Message, message1 = returnMessage });
            }
            return Json(new { message = returnMessage });
        }

        private void CreateSmicuft(Dictionary<string, object> parm, string excelFileName, MixedList ml)
        { 
            string mapping = InboundUploadExcelManager.SmicuftMapping;
            MixedList delMl = new MixedList();
            List<string> delList = new List<string>();
            parm["delMl"] = delMl;
            parm["delList"] = delList; 
            bool flag = Prolink.Math.GetValueAsBool(parm["dnDimensionFlag"]);
            ExcelParser.RegisterEditInstructFunc(mapping, HandleCreateSmicuft);
            ExcelParser ep = new ExcelParser();
            ep.Save(mapping, excelFileName, ml, parm, 1, 1);

            if (flag && delMl.Count <= 0)
                throw new Exception("Please maintain DN Dimension for Express shipment");

            for (int i = 0; i < delMl.Count; i++)
            {
                ml.Insert(0, (EditInstruct)delMl[i]);
            }
        }

        public static string HandleCreateSmicuft(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList delMl = (MixedList)parm["delMl"];
            List<string> delList = (List<string>)parm["delList"];
            string dnNo = ei.Get("DN_NO"); 
            if (!delList.Contains(dnNo))
            {
                delList.Add(dnNo);
                EditInstruct delEi = new EditInstruct("SMICUFT", EditInstruct.DELETE_OPERATION);
                delEi.PutKey("DN_NO", dnNo);
                delMl.Add(delEi);
            } 
            ei.Put("U_ID", Guid.NewGuid());
            ei.Put("U_FID", Guid.NewGuid());
            return string.Empty;
        }

        private static void SetPartyToSMSMI(DataTable smptyDt, EditInstruct ei, string ShipperNo, string ConsigneeNo, string NotifyNo, string ShipToNo, string FiCustomer,string subBg)
        {
            if (!string.IsNullOrEmpty(ShipperNo))
            {
                ei.Put("SH_NO", ShipperNo);
                ei.Put("SH_NM", GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(ShipperNo)), "PARTY_NAME"));
            }
            if (!string.IsNullOrEmpty(ConsigneeNo))
            {
                ei.Put("CS_NO", ConsigneeNo);
                ei.Put("CS_NM", GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(ConsigneeNo)), "PARTY_NAME"));
            }
            if (!string.IsNullOrEmpty(ShipToNo))
            {
                ei.Put("WE_NO", ShipToNo);
                ei.Put("WE_NM", GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(ShipToNo)), "PARTY_NAME"));
            }
            if (!string.IsNullOrEmpty(FiCustomer))
            {
                ei.Put("FC_NO", FiCustomer);
                ei.Put("FC_NM", GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(FiCustomer)), "PARTY_NAME"));
            }
            if (!string.IsNullOrEmpty(NotifyNo))
            {
                ei.Put("NT_NO", NotifyNo);
                ei.Put("NT_NM", GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(NotifyNo)), "PARTY_NAME"));
            }
            if (!string.IsNullOrEmpty(subBg))
            {
                ei.Put("ZT_NO", NotifyNo);
                ei.Put("BU", GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(subBg)), "DEP"));
                ei.Put("ZT_NM", GetTableValueByName(smptyDt, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(subBg)), "PARTY_NAME"));
            }
        }

        /// <summary>
        /// 验证SMSMI是否允许导入
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="msgs"></param>
        private void CheckSMSMIImport(DataTable dt, List<string> msgs)
        {
            Dictionary<string, object> mapping = XmlParser.GetMapping("SMSMIImport");
            BaseParser baseParser = new BaseParser();
            MixedList miList = new MixedList();
            baseParser.ParseEditInstruct(dt, "SMSMIImport", miList);
            Dictionary<string, object> fields = XmlParser.GetFields(mapping);
            List<string> invList = new List<string>();
            #region 验证栏位不允许为空
            for (int i = 0; i < miList.Count; i++)
            {
                EditInstruct ei = miList[i] as EditInstruct;
                string inv_no = ei.Get("INV_NO");
                if (!string.IsNullOrEmpty(inv_no) && !invList.Contains(inv_no))
                    invList.Add(inv_no);
                decimal Year = ei.GetValueAsDecimal("YEAR");
                decimal Month = ei.GetValueAsDecimal("MONTH");
                decimal Weekly = ei.GetValueAsDecimal("WEEKLY");
                DateTime Etd = Prolink.Math.GetValueAsDateTime(ei.Get("ETD1"));
                DateTime Eta = Prolink.Math.GetValueAsDateTime(ei.Get("ETA1"));

                if (DateTime.Compare(Etd, Eta) > 0)
                {
                    msgs.Add(string.Format("{0}.{1}: Import failed! ETA is not less than ETD, please check!", (i + 1), ei.Get("MASTER_NO")));
                }
                string date = ei.Get("ETD1");
                //if (string.IsNullOrEmpty(date))
                //    date = ei.Get("ATD1");
                if (Year == 0 && Month == 0 && Weekly == 0)
                {
                    if (!string.IsNullOrEmpty(date))
                    {
                        DateTime dateT = Prolink.Math.GetValueAsDateTime(date);
                        Year = dateT.Year;
                        Month = dateT.Month;
                        Weekly = TrackingEDI.Business.DateTimeUtils.GetWeekOfYear(dateT, DayOfWeek.Monday);
                    }
                }
                ei.Put("YEAR", Year);
                ei.Put("MONTH", Month);
                ei.Put("WEEKLY", Weekly);
                List<string> msg = new List<string>();
                foreach (var kv in fields)
                {
                    var field = kv.Value as Dictionary<string, object>;
                    string fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    string name = Prolink.Math.GetValueAsString(field["name"]);
                    string dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    string isnotnull = string.Empty;
                    if (field.ContainsKey("isnotnull"))
                        isnotnull = Prolink.Math.GetValueAsString(field["isnotnull"]);
                    if (!"Y".Equals(isnotnull))
                        continue;
                    switch (dataType)
                    {
                        case "number":
                            if (ei.GetValueAsDecimal(fieldname) == 0)
                                msg.Add(name);
                            break;
                        default:
                            string value = ei.Get(fieldname);
                            if (string.IsNullOrEmpty(value))
                                msg.Add(name);
                            break;
                    }
                }

                string tranType = ei.Get("TRAN_TYPE");
                if ("F".Equals(tranType) || "R".Equals(tranType))
                {
                    if (string.IsNullOrEmpty(ei.Get("CNTR_NO")))
                    {
                        msg.Add("Container#");
                    }
                    if (string.IsNullOrEmpty(ei.Get("CNTR_TYPE")))
                    {
                        msg.Add("ContainerType");
                    }
                    if (string.IsNullOrEmpty(ei.Get("SEAL_NO1")))
                    {
                        msg.Add("Seal#");
                    }
                }

                if (msg.Count > 0)
                    msgs.Add(string.Format("{0}.{1}: Import failed! {2} is key item, cannot be kept blank! Please check again.", (i + 1), ei.Get("MASTER_NO"), string.Join(",", msg)));
            }
            #endregion

            #region 验证INV_NO是否被使用
            if (invList.Count > 0)
            {
                DataTable invDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT SMIDN.SHIPMENT_ID,SMIDN.INV_NO FROM SMIDN WHERE INV_NO IN {0})T OUTER APPLY (SELECT MASTER_NO,STATUS FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=T.SHIPMENT_ID)M", SQLUtils.Quoted(invList.ToArray())), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                for (int i = 0; i < miList.Count; i++)
                {
                    EditInstruct ei = miList[i] as EditInstruct;
                    string inv_no = ei.Get("INV_NO");
                    string master_no = ei.Get("MASTER_NO");
                    if (string.IsNullOrEmpty(inv_no))
                        continue;
                    if (string.IsNullOrEmpty(master_no))
                        continue;

                    DataRow[] drs = invDt.Select(string.Format("INV_NO={0}", SQLUtils.QuotedStr(inv_no)));
                    if (drs.Length <= 0)
                        continue;
                    DataRow[] drs1 = invDt.Select(string.Format("INV_NO={0} AND MASTER_NO={1}", SQLUtils.QuotedStr(inv_no), SQLUtils.QuotedStr(master_no)));
                    if (drs1.Length > 0)
                        continue;
                    msgs.Add(string.Format("{0} is used by mbl({1}),import faild!", inv_no, drs[0]["MASTER_NO"]));
                }
            }
            #endregion
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


        private static void SetCntrProd(string UId, string ShipmentId)
        {
            string sql = string.Format("SELECT CNTR_NO + ',' FROM SMICNTR WHERE SHIPMENT_ID={0} FOR XML PATH('')", SQLUtils.QuotedStr(ShipmentId));
            string cntrinfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            smsmiei.PutKey("U_ID", UId);
            smsmiei.PutKey("SHIPMENT_ID", ShipmentId);
            bool isupdate = false;

            if (!string.IsNullOrEmpty(cntrinfo))
            {
                smsmiei.Put("CNTR_INFO", cntrinfo.Trim(','));
                isupdate = true;
            }
            sql = string.Format(" SELECT DIVISION_DESCP + ',' FROM SMIDN WHERE SHIPMENT_ID={0} FOR XML PATH('')", SQLUtils.QuotedStr(ShipmentId));
            string productinfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(productinfo))
            {
                smsmiei.Put("PRODUCT_INFO", productinfo.Trim(','));
                isupdate = true;
            }
            if (isupdate)
            {
                OperationUtils.ExecuteUpdate(smsmiei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }



        public ActionResult UploadSmsmipt(string U_ID,string Shipment_Id ,string PARTY_TYPE, string PARTY_NO)
        {
            string returnMessage = "success";
            string errorMsg = "";
            string Sql = "";
            EditInstruct ei;
            MixedList ml = new MixedList();            
            try
            {
                if (PARTY_NO == "")
                {
                    return Json(new { message = returnMessage });
                }
                Sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(PARTY_NO));
                DataTable dt = getDataTableFromSql(Sql);
                if (dt.Rows.Count > 0) 
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Sql = string.Format("DELETE FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE={1}", SQLUtils.QuotedStr(Shipment_Id), SQLUtils.QuotedStr(PARTY_TYPE));
                        ml.Add(Sql);
                        Sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(PARTY_TYPE), SQLUtils.QuotedStr(CompanyId));                   
                        string TypeDescp = getOneValueAsStringFromSql(Sql);
                        Sql = string.Format("SELECT ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND CD={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(PARTY_TYPE), SQLUtils.QuotedStr(CompanyId));
                        string OrderBy = getOneValueAsStringFromSql(Sql);
                        if (PARTY_TYPE == "IBBK") 
                        {
                            TypeDescp = "Outsite Vender";
                        }
                        string UFid = System.Guid.NewGuid().ToString();
                        ei = new EditInstruct("SMSMIPT", EditInstruct.INSERT_OPERATION);
                        ei.Put("U_ID", UFid);
                        ei.Put("U_FID", U_ID);
                        ei.Put("SHIPMENT_ID", Shipment_Id);
                        ei.Put("PARTY_TYPE", PARTY_TYPE);
                        ei.Put("TYPE_DESCP",TypeDescp);
                        ei.Put("ORDER_BY",OrderBy);                        
                        ei.Put("PARTY_NO", PARTY_NO);
                        ei.Put("PARTY_NAME",Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                        ei.Put("PARTY_NAME2",Prolink.Math.GetValueAsString(dr["PARTY_NAME2"]));
                        ei.Put("PARTY_NAME3",Prolink.Math.GetValueAsString(dr["PARTY_NAME3"]));
                        ei.Put("PARTY_NAME4",Prolink.Math.GetValueAsString(dr["PARTY_NAME4"]));
                        ei.Put("PARTY_ADDR1",Prolink.Math.GetValueAsString(dr["PART_ADDR1"]));
                        ei.Put("PARTY_ADDR2",Prolink.Math.GetValueAsString(dr["PART_ADDR2"]));
                        ei.Put("PARTY_ADDR3",Prolink.Math.GetValueAsString(dr["PART_ADDR3"]));
                        ei.Put("PARTY_ADDR4",Prolink.Math.GetValueAsString(dr["PART_ADDR4"]));
                        ei.Put("PARTY_ADDR5",Prolink.Math.GetValueAsString(dr["PART_ADDR5"]));
                        ei.Put("CNTY",Prolink.Math.GetValueAsString(dr["CNTY"]));
                        ei.Put("CNTY_NM",Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                        ei.Put("CITY",Prolink.Math.GetValueAsString(dr["CITY"]));
                        ei.Put("CITY_NM",Prolink.Math.GetValueAsString(dr["CITY_NM"]));
                        ei.Put("STATE",Prolink.Math.GetValueAsString(dr["STATE"]));
                        ei.Put("ZIP",Prolink.Math.GetValueAsString(dr["ZIP"]));
                        ei.Put("PARTY_ATTN",Prolink.Math.GetValueAsString(dr["PARTY_ATTN"]));
                        ei.Put("PARTY_TEL", Prolink.Math.GetValueAsString(dr["PARTY_TEL"]));
                        ei.Put("PARTY_MAIL", Prolink.Math.GetValueAsString(dr["PARTY_MAIL"]));
                        //ei.Put("DEBIT_TO",Prolink.Math.GetValueAsString(dr["STATE"]));
                        ei.Put("FAX_NO",Prolink.Math.GetValueAsString(dr["PARTY_FAX"]));
                        ei.Put("TAX_NO", Prolink.Math.GetValueAsString(dr["TAX_NO"]));
                        ml.Add(ei);
                    }
                }
                if (ml.Count > 0)
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception ex)
            {
                errorMsg = "Import PartyType Error";
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("Import PartyType Error :" + ex.ToString() + " : PartyType = "+ PARTY_TYPE + " : PartyNo = " + PARTY_NO);
                return Json(new { message = errorMsg });
            }
            return Json(new { message = returnMessage });
        }
        #endregion

        #region Import Party
        [HttpPost]
        public ActionResult ImportPartySmsmi(FormCollection form)
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
            string ermsg = "";
            EditInstruct ei;
            int StartRow = 0; //Excel 從第2排開始讀
            if (Request.Files.Count == 0)
            { 
                return Json(new { errorMsg = "error" });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            { 
                return Json(new { errorMsg = "error" });
            }
            else
            {
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "ImportPartySmsmi\\");
                string excelFileName = string.Format("{0}.{1}", dirpath + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        DataRow dr = dt.Rows[i];
                        //必填欄位
                        string ShipmentId = Prolink.Math.GetValueAsString(dr[0]);
                        string IBBR = Prolink.Math.GetValueAsString(dr[1]);
                        string IBSP = Prolink.Math.GetValueAsString(dr[2]);
                        string IBCR = Prolink.Math.GetValueAsString(dr[3]);
                        string IBTC = Prolink.Math.GetValueAsString(dr[4]);
                        string IBTW = Prolink.Math.GetValueAsString(dr[5]);
                        string IBLP = Prolink.Math.GetValueAsString(dr[6]);
                        string IBGVS = Prolink.Math.GetValueAsString(dr[7]);
                        string IBGVX = Prolink.Math.GetValueAsString(dr[8]); 
                        string InBoundRemark = Prolink.Math.GetValueAsString(dr[9]);

                        if (ShipmentId == "")
                            continue;

                        string Sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                        DataTable smsmidt = getDataTableFromSql(Sql);
                        if (smsmidt.Rows.Count <= 0)
                        {
                            ermsg += "Can't find this ShipmentID :" + ShipmentId + "\r\n";
                            break;
                        }
                        string UId = smsmidt.Rows[0]["U_ID"].ToString();
                        ShipmentId = smsmidt.Rows[0]["SHIPMENT_ID"].ToString();
                        string TranType =Prolink.Math.GetValueAsString(smsmidt.Rows[0]["TRAN_TYPE"]);
                        string status = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["STATUS"]);
                        


                        //IBSP, IBTW, IBLP, IBGVS, IBGVX: 未Notify LSP前可以修改
                        //IBCR: 未叫车前可以修改
                        //IBBR: 未Notify Broker前可以修改 
                        //IBTC：未Notify Transit Broker前可以修改 
                        Func<string, string> CheckStatus = (partyType) =>
                        {
                            string result = "";
                            List<string> statusList = new List<string>();
                            switch (partyType)
                            {
                                case "IBSP":
                                case "IBTW":
                                case "IBLP":
                                case "IBGVS":
                                case "IBGVX":
                                    statusList = new List<string> { "A" };
                                    if (!statusList.Contains(status))
                                        result = "ShipmentId:" + ShipmentId + ",the shipment status is not in \"Unreach\", cannot modify " + partyType + ".\r\n";
                                    break;
                                case "IBCR":
                                    string sql = string.Format(@"SELECT COUNT(1) FROM SMORD WHERE SHIPMENT_ID={0} AND CSTATUS NOT IN('X','Y')", SQLUtils.QuotedStr(ShipmentId));
                                    int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    if (count > 0)
                                        result = "ShipmentId:" + ShipmentId + ",the shipment has been truck calling, cannot change IBCR." + "\r\n";
                                    break;
                                case "IBBR":
                                    statusList = new List<string> { "A", "B" };
                                    if (!statusList.Contains(status))
                                        result = "ShipmentId:" + ShipmentId + ",the shipment has been Notify Broker, cannot change IBBR." + "\r\n";
                                    break;
                                case "IBTC":
                                    statusList = new List<string> { "A", "B", "C", "D" };
                                    if (!statusList.Contains(status))
                                        result = "ShipmentId:" + ShipmentId + ",the shipment status has been Notify Transit Broker, cannot change IBTC." + "\r\n";
                                    break;
                            }
                            ermsg += result;
                            return result;
                        };
                         
                        ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("U_ID", UId);
                        ei.PutKey("SHIPMENT_ID", ShipmentId);
                        ei.Put("INSTRUCTION", InBoundRemark); 
                        if (!string.IsNullOrEmpty(IBBR)) 
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBBR")))
                            {
                                ImportPartySmsmipt(UId, ShipmentId, "IBBR", IBBR, ml);
                                SetPartyNameToSMSMI("IBBR", IBBR, ref ei, TranType, ShipmentId, ml);
                            } 
                        }
                        if (!string.IsNullOrEmpty(IBSP))
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBSP")))
                            { 
                                ImportPartySmsmipt(UId, ShipmentId, "IBSP", IBSP, ml);
                                SetPartyNameToSMSMI("IBSP", IBSP, ref ei, TranType, ShipmentId, ml);
                            } 
                        }
                        if (!string.IsNullOrEmpty(IBCR))
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBCR")))
                            { 
                                ImportPartySmsmipt(UId, ShipmentId, "IBCR", IBCR, ml);
                                SetPartyNameToSMSMI("IBCR", IBCR, ref ei, TranType, ShipmentId, ml);
                            } 
                        }
                        if (!string.IsNullOrEmpty(IBTC))
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBTC")))
                            {
                                bool has = ImportPartySmsmipt(UId, ShipmentId, "IBTC", IBTC, ml);
                                if (has)
                                {
                                    ei.Put("IS_TRANSIT_BROKER", "Y");
                                }
                                else
                                {
                                    ei.Put("IS_TRANSIT_BROKER", "");
                                }

                            }  
                        }
                        if (!string.IsNullOrEmpty(IBTW))
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBTW"))) 
                                ImportPartySmsmipt(UId, ShipmentId, "IBTW", IBTW, ml); 
                        }
                        if (!string.IsNullOrEmpty(IBLP))
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBLP"))) 
                                ImportPartySmsmipt(UId, ShipmentId, "IBLP", IBLP, ml); 
                        }
                        if (!string.IsNullOrEmpty(IBGVS))
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBGVS"))) 
                                ImportPartySmsmipt(UId, ShipmentId, "IBGVS", IBGVS, ml);  
                        }
                        if (!string.IsNullOrEmpty(IBGVX))
                        {
                            if (string.IsNullOrEmpty(CheckStatus("IBGVX"))) 
                                ImportPartySmsmipt(UId, ShipmentId, "IBGVX", IBGVX, ml);
                        }
                         
                        ml.Add(ei);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { errorMsg = ex.ToString() });
                    }
                }
                if (!string.IsNullOrEmpty(ermsg))
                {
                    return Json(new { errorMsg = ermsg });
                }

                if (ml.Count > 0)
                {
                    try
                    {
                        int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch(Exception ex)
                    {
                        returnMessage = "error";
                        ermsg = ex.Message.ToString();
                    }
                }
            }
            return Json(new { message = returnMessage, errorMsg = ermsg });
        }

        public bool ImportPartySmsmipt(string UId, string ShipmentId, string PartyType, string PartyNo,MixedList ml)
        {
            try
            {
                string Sql = string.Format("SELECT * FROM SMSMIPT WHERE U_FID = {0} AND SHIPMENT_ID = {1} AND PARTY_TYPE = {2}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(PartyType));
                DataTable Smiptdt = getDataTableFromSql(Sql);
                if ( Smiptdt.Rows.Count > 0 ) 
                {
                    Sql = string.Format("DELETE FROM SMSMIPT WHERE U_FID = {0} AND SHIPMENT_ID = {1} AND PARTY_TYPE = {2}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(PartyType));
                    ml.Add(Sql);
                }
                Sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(PartyNo));
                DataTable dt = getDataTableFromSql(Sql);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];

                    Sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD={0} AND ( CMP={1}  OR CMP='*')", SQLUtils.QuotedStr(PartyType), SQLUtils.QuotedStr(CompanyId));
                    string TypeDescp = getOneValueAsStringFromSql(Sql);
                    Sql = string.Format("SELECT ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND CD={0} AND( CMP={1} OR CMP='*')", SQLUtils.QuotedStr(PartyType), SQLUtils.QuotedStr(CompanyId));
                    string OrderBy = getOneValueAsStringFromSql(Sql);
                    string UFid = System.Guid.NewGuid().ToString();
                    EditInstruct ei = new EditInstruct("SMSMIPT", EditInstruct.INSERT_OPERATION);
                    ei.Put("U_ID", UFid);
                    ei.Put("U_FID", UId);
                    ei.Put("SHIPMENT_ID", ShipmentId);
                    ei.Put("PARTY_TYPE", PartyType);
                    ei.Put("TYPE_DESCP", TypeDescp);
                    ei.Put("ORDER_BY", OrderBy);
                    ei.Put("PARTY_NO", PartyNo);
                    ei.Put("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                    ei.Put("PARTY_NAME2", Prolink.Math.GetValueAsString(dr["PARTY_NAME2"]));
                    ei.Put("PARTY_NAME3", Prolink.Math.GetValueAsString(dr["PARTY_NAME3"]));
                    ei.Put("PARTY_NAME4", Prolink.Math.GetValueAsString(dr["PARTY_NAME4"]));
                    ei.Put("PARTY_ADDR1", Prolink.Math.GetValueAsString(dr["PART_ADDR1"]));
                    ei.Put("PARTY_ADDR2", Prolink.Math.GetValueAsString(dr["PART_ADDR2"]));
                    ei.Put("PARTY_ADDR3", Prolink.Math.GetValueAsString(dr["PART_ADDR3"]));
                    ei.Put("PARTY_ADDR4", Prolink.Math.GetValueAsString(dr["PART_ADDR4"]));
                    ei.Put("PARTY_ADDR5", Prolink.Math.GetValueAsString(dr["PART_ADDR5"]));
                    ei.Put("CNTY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                    ei.Put("CNTY_NM", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                    ei.Put("CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                    ei.Put("CITY_NM", Prolink.Math.GetValueAsString(dr["CITY_NM"]));
                    ei.Put("STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                    ei.Put("ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                    ei.Put("PARTY_ATTN", Prolink.Math.GetValueAsString(dr["PARTY_ATTN"]));
                    ei.Put("PARTY_TEL", Prolink.Math.GetValueAsString(dr["PARTY_TEL"]));
                    ei.Put("PARTY_MAIL", Prolink.Math.GetValueAsString(dr["PARTY_MAIL"]));
                    //ei.Put("DEBIT_TO",Prolink.Math.GetValueAsString(dr["STATE"]));
                    ei.Put("FAX_NO", Prolink.Math.GetValueAsString(dr["PARTY_FAX"]));
                    ei.Put("TAX_NO", Prolink.Math.GetValueAsString(dr["TAX_NO"]));
                    ml.Add(ei);
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public void SetPartyNameToSMSMI(string PartyType, string PartyNo,ref EditInstruct ei, string TranType, string shipmentId,MixedList ml)
        {
            string sql = string.Format("SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(PartyNo));
            string PartyNm = getOneValueAsStringFromSql(sql);
            if (!string.IsNullOrEmpty(PartyNm))
            {
                ei.Put(PartyType + "_NO", PartyNo);
                ei.Put(PartyType + "_NM", PartyNm);
                if ("IBCR".Equals(PartyType) && !string.IsNullOrEmpty(shipmentId))
                {
                    ei.Put("TRUCKER1", PartyNo);
                    ei.Put("TRUCKER_NM1", PartyNm);
                    if ("F".Equals(TranType) || "R".Equals(TranType))
                    {
                        sql = string.Format("UPDATE SMICNTR SET TRUCKER1={0},TRUCKER_NM1={1} WHERE SHIPMENT_ID={2}",
                            SQLUtils.QuotedStr(PartyNo), SQLUtils.QuotedStr(PartyNm), SQLUtils.QuotedStr(shipmentId));
                        ml.Add(sql);
                    }
                }
            }
        }

        #endregion

        #region 报关格式下载
        public ActionResult DownloadExcel() 
        {
            string returnMsg = "success";
            string Sql = "";
            string TransType = "";
            string Division = "";

            string Cmp = Request.Params["Cmp"];
            string suid = Request.Params["suid"];
            string data = Request.Params["postData"];
            string[] shipments = Request.Params["shipments"].Split(',');
            string VenderCd = Prolink.Math.GetValueAsString(Request.Params["VenderCd"]);
            DataTable dtAll = new DataTable();
            DataTable dt = new DataTable();            
            try
            {
                foreach (string item in shipments) 
                {
                    string InvNo = "";
                    DataTable dnDt = new DataTable();
                    DataTable cnDt = new DataTable();
                    Sql = string.Format("SELECT TRAN_TYPE,SHIPMENT_ID,MASTER_NO,DN_NO,POD_CD,DEST_CD FROM SMSMI WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(item));
                    dt = getDataTableFromSql(Sql);
                    TransType = dt.Rows[0]["TRAN_TYPE"].ToString();
                    dt.Columns["TRAN_TYPE"].MaxLength = 5;
                    
                    dt.Columns.Add("CONTAINER_NO", typeof(String));  
                    dt.Columns["CONTAINER_NO"].SetOrdinal(3);
                    dt.Columns.Add("MATERIAL_TYPE", typeof(String));
                    dt.Columns["MATERIAL_TYPE"].MaxLength = 1000;
                    dt.Columns.Add("DEC_NO", typeof(String));
                    dt.Columns.Add("IMPORT_NO", typeof(String));
                    dt.Columns.Add("DEC_DATE", typeof(DateTime));
                    dt.Columns.Add("REL_DATE", typeof(DateTime));
                    dt.Columns.Add("INSPECTION", typeof(String));
                    dt.Columns.Add("CER_NO", typeof(String));
                    dt.Columns.Add("DEC_REPLY", typeof(String));
                    dt.Columns.Add("ICDF", typeof(String));
                    dt.Columns.Add("CC_CHANNEL", typeof(String));
                    dt.Columns["DN_NO"].MaxLength = 2000;
                    dt.Columns.Add("HS_QTY", typeof(int));
                    dt.Columns.Add("TC_HS_QTY", typeof(int));
                    dt.Columns.Add("CNTRY_CD", typeof(string));
                    dt.Columns.Add("TC_CNTRY_CD", typeof(string));
                    dt.Columns.Add("PLI", typeof(string));
                    dt.Columns.Add("LI", typeof(string));
                    dt.Columns.Add("SUF_COST", typeof(double));
                    dt.Columns.Add("CC_RATE", typeof(double));
                    dt.Columns.Add("ADD_QTY", typeof(double));
                    dt.Columns.Add("SIS_FEE", typeof(double));

                    switch (TransType)
                    {
                        case "F":
                        case "R":
                            dt.Rows[0]["TRAN_TYPE"] = getTranType(TransType);
                            Sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(item));
                            cnDt = getDataTableFromSql(Sql);
                            if (cnDt.Rows.Count > 0)
                            {
                                for (int i = 0; i < cnDt.Rows.Count; i++) 
                                {
                                    Sql = string.Format("SELECT * FROM SMICNTR WHERE CNTR_NO = {0} AND SHIPMENT_ID = {1}", SQLUtils.QuotedStr(cnDt.Rows[i]["CNTR_NO"].ToString()), SQLUtils.QuotedStr(item));
                                    DataTable dt1 = getDataTableFromSql(Sql);
                                    if (dt1.Rows.Count == 1) 
                                    {
                                        Sql = string.Format("SELECT MASTER_NO FROM SMSMI WHERE SHIPMENT_ID={0}",SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()));
                                        string MasterNo = getOneValueAsStringFromSql(Sql);
                                        string Dns = dt1.Rows[0]["DN_NO"].ToString();
                                        if (Dns == "")
                                        {
                                            Sql = "SELECT INV_NO FROM SMIDN WHERE SHIPMENT_ID={0}";
                                            Sql = string.Format(Sql, SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()));
                                        }
                                        else
                                        {
                                            Sql = "SELECT INV_NO FROM SMIDN WHERE SHIPMENT_ID={0} AND DN_NO IN {1}";
                                            Sql = string.Format(Sql, SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()), SQLUtils.Quoted(Dns.Split(',')));
                                        }
                                        DataTable InvNos = getDataTableFromSql(Sql);
                                        if (InvNos.Rows.Count > 0) 
                                        {
                                            for (int n = 0; n < InvNos.Rows.Count; n++) 
                                            {
                                                string inv_no = InvNos.Rows[n]["INV_NO"].ToString();
                                                InvNo = InvNo + inv_no + ",";
                                            }
                                            InvNo = InvNo.TrimEnd(',');
                                            string ContainerNo = cnDt.Rows[i]["CNTR_NO"].ToString();
                                            string DnNo = cnDt.Rows[i]["DN_NO"].ToString();
                                            Division = cnDt.Rows[i]["DIVISION_DESCP"].ToString();
                                            dt.Rows[0]["CONTAINER_NO"] = ContainerNo;
                                            dt.Rows[0]["MASTER_NO"] = MasterNo;
                                            dt.Rows[0]["DN_NO"] = InvNo;
                                            dt.Rows[0]["MATERIAL_TYPE"] = Division;
                                            dt.Rows[0]["DEC_NO"] = cnDt.Rows[i]["DEC_NO"].ToString();
                                            dt.Rows[0]["IMPORT_NO"] = cnDt.Rows[i]["IMPORT_NO"].ToString();
                                            dt.Rows[0]["DEC_DATE"] = cnDt.Rows[i]["DEC_DATE"]; 
                                            dt.Rows[0]["REL_DATE"] = cnDt.Rows[i]["REL_DATE"];
                                            dt.Rows[0]["INSPECTION"] = cnDt.Rows[i]["INSPECTION"].ToString();
                                            dt.Rows[0]["CER_NO"] = cnDt.Rows[i]["CER_NO"].ToString();
                                            dt.Rows[0]["DEC_REPLY"] = cnDt.Rows[i]["DEC_REPLY"].ToString();
                                            dt.Rows[0]["ICDF"] = cnDt.Rows[i]["ICDF"].ToString();
                                            dt.Rows[0]["CC_CHANNEL"] = cnDt.Rows[i]["CC_CHANNEL"].ToString();
                                            dt.Rows[0]["HS_QTY"] = Prolink.Math.GetValueAsInt(cnDt.Rows[i]["HS_QTY"]);
                                            dt.Rows[0]["TC_HS_QTY"] = Prolink.Math.GetValueAsInt(cnDt.Rows[i]["TC_HS_QTY"]);
                                            dt.Rows[0]["CNTRY_CD"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["CNTRY_CD"]);
                                            dt.Rows[0]["TC_CNTRY_CD"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["TC_CNTRY_CD"]);

                                            dt.Rows[0]["PLI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["PLI"]);
                                            dt.Rows[0]["LI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["LI"]);
                                            dt.Rows[0]["SUF_COST"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SUF_COST"]);
                                            dt.Rows[0]["CC_RATE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["CC_RATE"]);
                                            dt.Rows[0]["ADD_QTY"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["ADD_QTY"]);
                                            dt.Rows[0]["SIS_FEE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SIS_FEE"]);
                                             
                                            InvNo = "";
                                            dtAll.Merge(dt);
                                        }                                       
                                    }
                                    else if (dt1.Rows.Count >= 2) 
                                    {
                                        for (int a = 0; a < dt1.Rows.Count; a++) 
                                        {
                                            Sql = string.Format("SELECT MASTER_NO FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()));
                                            DataTable MasterList = getDataTableFromSql(Sql);
                                            for (int n = 0; n < MasterList.Rows.Count; n++) 
                                            {
                                                Sql = string.Format("SELECT INV_NO FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()));
                                                DataTable InvNos = getDataTableFromSql(Sql);
                                                
                                                if (InvNos.Rows.Count > 0)
                                                {
                                                    for (int s = 0; s < InvNos.Rows.Count; s++)
                                                    {
                                                        string MasterNo = MasterList.Rows[n]["MASTER_NO"].ToString();
                                                        string ContainerNo = cnDt.Rows[i]["CNTR_NO"].ToString();
                                                        Division = cnDt.Rows[i]["DIVISION_DESCP"].ToString();
                                                        dt.Rows[0]["CONTAINER_NO"] = ContainerNo;
                                                        dt.Rows[0]["MASTER_NO"] = MasterNo;
                                                        dt.Rows[0]["MATERIAL_TYPE"] = Division;
                                                        dt.Rows[0]["DN_NO"] = InvNos.Rows[n]["INV_NO"].ToString();
                                                        dt.Rows[0]["DEC_NO"] = cnDt.Rows[i]["DEC_NO"].ToString();
                                                        dt.Rows[0]["IMPORT_NO"] = cnDt.Rows[i]["IMPORT_NO"].ToString();
                                                        dt.Rows[0]["DEC_DATE"] = cnDt.Rows[i]["DEC_DATE"];
                                                        dt.Rows[0]["REL_DATE"] = cnDt.Rows[i]["REL_DATE"];
                                                        dt.Rows[0]["INSPECTION"] = cnDt.Rows[i]["INSPECTION"].ToString();
                                                        dt.Rows[0]["CER_NO"] = cnDt.Rows[i]["CER_NO"].ToString();
                                                        dt.Rows[0]["DEC_REPLY"] = cnDt.Rows[i]["DEC_REPLY"].ToString();
                                                        dt.Rows[0]["ICDF"] = cnDt.Rows[i]["ICDF"].ToString();
                                                        dt.Rows[0]["CC_CHANNEL"] = cnDt.Rows[i]["CC_CHANNEL"].ToString();
                                                        dt.Rows[0]["HS_QTY"] = Prolink.Math.GetValueAsInt(cnDt.Rows[i]["HS_QTY"]);
                                                        dt.Rows[0]["TC_HS_QTY"] = Prolink.Math.GetValueAsInt(cnDt.Rows[i]["TC_HS_QTY"]);
                                                        dt.Rows[0]["CNTRY_CD"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["CNTRY_CD"]);
                                                        dt.Rows[0]["TC_CNTRY_CD"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["TC_CNTRY_CD"]);

                                                        dt.Rows[0]["PLI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["PLI"]);
                                                        dt.Rows[0]["LI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["LI"]);
                                                        dt.Rows[0]["SUF_COST"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SUF_COST"]);
                                                        dt.Rows[0]["CC_RATE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["CC_RATE"]);
                                                        dt.Rows[0]["ADD_QTY"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["ADD_QTY"]);
                                                        dt.Rows[0]["SIS_FEE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SIS_FEE"]);

                                                        dtAll.Merge(dt);
                                                    }
                                                }        
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "L":
                        case "A":
                        case "E":
                            dt.Rows[0]["TRAN_TYPE"] = getTranType(TransType);
                            Sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(item));
                            dnDt = getDataTableFromSql(Sql);
                            if (dnDt.Rows.Count > 0) 
                            {
                                for (int i = 0; i < dnDt.Rows.Count; i++) 
                                {
                                    string inv_no = dnDt.Rows[i]["INV_NO"].ToString();
                                    InvNo = InvNo + inv_no + ",";
                                }
                                InvNo = InvNo.TrimEnd(',');

                                Division = dnDt.Rows[0]["DIVISION_DESCP"].ToString();
                                dt.Rows[0]["MATERIAL_TYPE"] = Division;
                                dt.Rows[0]["DN_NO"] = InvNo;
                                dt.Rows[0]["DEC_NO"] = dnDt.Rows[0]["DEC_NO"].ToString();
                                dt.Rows[0]["IMPORT_NO"] = dnDt.Rows[0]["IMPORT_NO"].ToString();
                                dt.Rows[0]["DEC_DATE"] = dnDt.Rows[0]["DEC_DATE"];
                                dt.Rows[0]["REL_DATE"] = dnDt.Rows[0]["REL_DATE"];
                                dt.Rows[0]["INSPECTION"] = dnDt.Rows[0]["INSPECTION"].ToString();
                                dt.Rows[0]["CER_NO"] = dnDt.Rows[0]["CER_NO"].ToString();
                                dt.Rows[0]["DEC_REPLY"] = dnDt.Rows[0]["DEC_REPLY"].ToString();
                                dt.Rows[0]["ICDF"] = dnDt.Rows[0]["ICDF"].ToString();
                                dt.Rows[0]["CC_CHANNEL"] = dnDt.Rows[0]["CC_CHANNEL"].ToString();
                                dt.Rows[0]["HS_QTY"] = Prolink.Math.GetValueAsInt(dnDt.Rows[0]["HS_QTY"]);
                                dt.Rows[0]["TC_HS_QTY"] = Prolink.Math.GetValueAsInt(dnDt.Rows[0]["TC_HS_QTY"]);
                                dt.Rows[0]["CNTRY_CD"] = Prolink.Math.GetValueAsString(dnDt.Rows[0]["CNTRY_CD"]);
                                dt.Rows[0]["TC_CNTRY_CD"] = Prolink.Math.GetValueAsString(dnDt.Rows[0]["TC_CNTRY_CD"]);

                                dt.Rows[0]["PLI"] = Prolink.Math.GetValueAsString(dnDt.Rows[0]["PLI"]);
                                dt.Rows[0]["LI"] = Prolink.Math.GetValueAsString(dnDt.Rows[0]["LI"]);
                                dt.Rows[0]["SUF_COST"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["SUF_COST"]);
                                dt.Rows[0]["CC_RATE"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["CC_RATE"]);
                                dt.Rows[0]["ADD_QTY"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["ADD_QTY"]);
                                dt.Rows[0]["SIS_FEE"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["SIS_FEE"]);
                                dtAll.Merge(dt);
                            }                            
                            break;
                    }
                    if (dtAll.Rows.Count > 0)
                    {
                        dtAll.DefaultView.Sort = "TRAN_TYPE DESC";
                    }                  
                }                
            }
            catch (Exception ex) 
            {
                returnMsg = "Fail" + ex.ToString();
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("DownloadExcel Error :" + ex.ToString());
                return Json(new { message = returnMsg });
            } 
            return ExportExcelFile(dtAll);
        }

        public ActionResult NewDownloadExcel()
        {
            bool result = false;
            string msg = string.Empty;
            bool isCsm = Prolink.Math.GetValueAsBool(Request["isCsm"]);
            string file = "";
            if (isCsm)
            {
                file = DownloadCSMCCInformation(ref result, ref msg);
            } else
            {
                file = DownloadCCInformation(ref result, ref msg);
            }
            return Json(new { IsOk = result ? "Y" : "N", file = file, msg = msg });
        }

        protected string DownloadCCInformation(ref bool result, ref string msg)
        { 
            string Sql = "";
            string TransType = "";
            string Division = ""; 
            string[] shipments = Request.Params["shipments"].Split(','); 
            DataTable dtAll = new DataTable();
            DataTable dt = new DataTable();
            try
            {
                foreach (string item in shipments)
                {
                    string InvNo = "";
                    DataTable dnDt = new DataTable();
                    DataTable cnDt = new DataTable();
                    Sql = string.Format("SELECT TRAN_TYPE,SHIPMENT_ID,MASTER_NO,DN_NO,POD_CD,DEST_CD FROM SMSMI WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(item));
                    dt = getDataTableFromSql(Sql);
                    TransType = dt.Rows[0]["TRAN_TYPE"].ToString();
                    dt.Columns["TRAN_TYPE"].MaxLength = 5;
                    dt.Columns.Add("CONTAINER_NO", typeof(String));
                    dt.Columns["CONTAINER_NO"].SetOrdinal(3);
                    dt.Columns.Add("MATERIAL_TYPE", typeof(String));
                    dt.Columns["MATERIAL_TYPE"].MaxLength = 1000;
                    dt.Columns.Add("DEC_NO", typeof(String));
                    dt.Columns.Add("IMPORT_NO", typeof(String));
                    dt.Columns.Add("DEC_DATE", typeof(String));
                    dt.Columns.Add("REL_DATE", typeof(String));
                    dt.Columns.Add("INSPECTION", typeof(String));
                    dt.Columns.Add("CER_NO", typeof(String));
                    dt.Columns.Add("DEC_REPLY", typeof(String));
                    dt.Columns.Add("ICDF", typeof(String));
                    dt.Columns.Add("CC_CHANNEL", typeof(String));
                    dt.Columns["DN_NO"].MaxLength = 2000;
                    dt.Columns.Add("HS_QTY", typeof(int)); 
                    dt.Columns.Add("CNTRY_CD", typeof(string)); 
                    dt.Columns.Add("PLI", typeof(string));
                    dt.Columns.Add("LI", typeof(string));
                    dt.Columns.Add("SUF_COST", typeof(double));
                    dt.Columns.Add("CC_RATE", typeof(double));
                    dt.Columns.Add("ADD_QTY", typeof(double));
                    dt.Columns.Add("SIS_FEE", typeof(double));

                    Func<string, string> getDateFont = (date) =>
                     {
                         if (string.IsNullOrEmpty(date)) return string.Empty; 
                         return Prolink.Math.GetValueAsDateTime(date).ToString("yyyy-MM-dd"); 
                     };

                    switch (TransType)
                    {
                        case "F":
                        case "R":
                            dt.Rows[0]["TRAN_TYPE"] = getTranType(TransType);
                            Sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(item));
                            cnDt = getDataTableFromSql(Sql);
                            if (cnDt.Rows.Count > 0)
                            {
                                for (int i = 0; i < cnDt.Rows.Count; i++)
                                {
                                    Sql = string.Format("SELECT * FROM SMICNTR WHERE CNTR_NO = {0} AND SHIPMENT_ID = {1}", SQLUtils.QuotedStr(cnDt.Rows[i]["CNTR_NO"].ToString()), SQLUtils.QuotedStr(item));
                                    DataTable dt1 = getDataTableFromSql(Sql);
                                    if (dt1.Rows.Count == 1)
                                    {
                                        Sql = string.Format("SELECT MASTER_NO FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()));
                                        string MasterNo = getOneValueAsStringFromSql(Sql);
                                        string ContainerNo = cnDt.Rows[i]["CNTR_NO"].ToString();
                                        string DnNo = cnDt.Rows[i]["DN_NO"].ToString();
                                        Division = cnDt.Rows[i]["DIVISION_DESCP"].ToString();
                                        dt.Rows[0]["CONTAINER_NO"] = ContainerNo;
                                        dt.Rows[0]["MASTER_NO"] = MasterNo;
                                        dt.Rows[0]["DN_NO"] = cnDt.Rows[i]["INV_NO"].ToString();
                                        dt.Rows[0]["MATERIAL_TYPE"] = Division;
                                        dt.Rows[0]["DEC_NO"] = cnDt.Rows[i]["DEC_NO"].ToString();
                                        dt.Rows[0]["IMPORT_NO"] = cnDt.Rows[i]["IMPORT_NO"].ToString(); 
                                        dt.Rows[0]["DEC_DATE"] = getDateFont(Prolink.Math.GetValueAsString(cnDt.Rows[i]["DEC_DATE"]));
                                        dt.Rows[0]["REL_DATE"] = getDateFont(Prolink.Math.GetValueAsString(cnDt.Rows[i]["REL_DATE"]));
                                        dt.Rows[0]["INSPECTION"] = cnDt.Rows[i]["INSPECTION"].ToString();
                                        dt.Rows[0]["CER_NO"] = cnDt.Rows[i]["CER_NO"].ToString();
                                        dt.Rows[0]["DEC_REPLY"] = cnDt.Rows[i]["DEC_REPLY"].ToString();
                                        dt.Rows[0]["ICDF"] = cnDt.Rows[i]["ICDF"].ToString();
                                        dt.Rows[0]["CC_CHANNEL"] = cnDt.Rows[i]["CC_CHANNEL"].ToString();
                                        dt.Rows[0]["HS_QTY"] = Prolink.Math.GetValueAsInt(cnDt.Rows[i]["HS_QTY"]); 
                                        dt.Rows[0]["CNTRY_CD"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["CNTRY_CD"]);  
                                        dt.Rows[0]["PLI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["PLI"]);
                                        dt.Rows[0]["LI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["LI"]);
                                        dt.Rows[0]["SUF_COST"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SUF_COST"]);
                                        dt.Rows[0]["CC_RATE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["CC_RATE"]);
                                        dt.Rows[0]["ADD_QTY"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["ADD_QTY"]);
                                        dt.Rows[0]["SIS_FEE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SIS_FEE"]);
                                        
                                        InvNo = "";
                                        dtAll.Merge(dt); 
                                    }
                                    else if (dt1.Rows.Count >= 2)
                                    {
                                        for (int a = 0; a < dt1.Rows.Count; a++)
                                        {
                                            Sql = string.Format("SELECT MASTER_NO FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()));
                                            DataTable MasterList = getDataTableFromSql(Sql);
                                            for (int n = 0; n < MasterList.Rows.Count; n++)
                                            {
                                                Sql = string.Format("SELECT INV_NO FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(dt1.Rows[0]["SHIPMENT_ID"].ToString()));
                                                DataTable InvNos = getDataTableFromSql(Sql);

                                                if (InvNos.Rows.Count > 0)
                                                {
                                                    for (int s = 0; s < InvNos.Rows.Count; s++)
                                                    {
                                                        string MasterNo = MasterList.Rows[n]["MASTER_NO"].ToString();
                                                        string ContainerNo = cnDt.Rows[i]["CNTR_NO"].ToString();
                                                        Division = cnDt.Rows[i]["DIVISION_DESCP"].ToString();
                                                        dt.Rows[0]["CONTAINER_NO"] = ContainerNo;
                                                        dt.Rows[0]["MASTER_NO"] = MasterNo;
                                                        dt.Rows[0]["MATERIAL_TYPE"] = Division;
                                                        dt.Rows[0]["DN_NO"] = InvNos.Rows[n]["INV_NO"].ToString();
                                                        dt.Rows[0]["DEC_NO"] = cnDt.Rows[i]["DEC_NO"].ToString();
                                                        dt.Rows[0]["IMPORT_NO"] = cnDt.Rows[i]["IMPORT_NO"].ToString(); 
                                                        dt.Rows[0]["DEC_DATE"] = getDateFont(Prolink.Math.GetValueAsString(cnDt.Rows[i]["DEC_DATE"]));
                                                        dt.Rows[0]["REL_DATE"] = getDateFont(Prolink.Math.GetValueAsString(cnDt.Rows[i]["REL_DATE"]));
                                                        dt.Rows[0]["INSPECTION"] = cnDt.Rows[i]["INSPECTION"].ToString();
                                                        dt.Rows[0]["CER_NO"] = cnDt.Rows[i]["CER_NO"].ToString();
                                                        dt.Rows[0]["DEC_REPLY"] = cnDt.Rows[i]["DEC_REPLY"].ToString();
                                                        dt.Rows[0]["ICDF"] = cnDt.Rows[i]["ICDF"].ToString();
                                                        dt.Rows[0]["CC_CHANNEL"] = cnDt.Rows[i]["CC_CHANNEL"].ToString();
                                                        dt.Rows[0]["HS_QTY"] = Prolink.Math.GetValueAsInt(cnDt.Rows[i]["HS_QTY"]); 
                                                        dt.Rows[0]["CNTRY_CD"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["CNTRY_CD"]); 
                                                        dt.Rows[0]["PLI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["PLI"]);
                                                        dt.Rows[0]["LI"] = Prolink.Math.GetValueAsString(cnDt.Rows[i]["LI"]);
                                                        dt.Rows[0]["SUF_COST"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SUF_COST"]);
                                                        dt.Rows[0]["CC_RATE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["CC_RATE"]);
                                                        dt.Rows[0]["ADD_QTY"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["ADD_QTY"]);
                                                        dt.Rows[0]["SIS_FEE"] = Prolink.Math.GetValueAsDouble(cnDt.Rows[i]["SIS_FEE"]);
                                                        
                                                        dtAll.Merge(dt);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "L":
                        case "A":
                        case "E":
                            dt.Rows[0]["TRAN_TYPE"] = getTranType(TransType);
                            Sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(item));
                            dnDt = getDataTableFromSql(Sql);
                            if (dnDt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dnDt.Rows.Count; i++)
                                {
                                    string inv_no = dnDt.Rows[i]["INV_NO"].ToString();
                                    InvNo = InvNo + inv_no + ",";
                                }
                                InvNo = InvNo.TrimEnd(',');

                                Division = dnDt.Rows[0]["DIVISION_DESCP"].ToString();
                                dt.Rows[0]["MATERIAL_TYPE"] = Division;
                                dt.Rows[0]["DN_NO"] = InvNo;
                                dt.Rows[0]["DEC_NO"] = dnDt.Rows[0]["DEC_NO"].ToString();
                                dt.Rows[0]["IMPORT_NO"] = dnDt.Rows[0]["IMPORT_NO"].ToString(); 
                                dt.Rows[0]["DEC_DATE"] = getDateFont(Prolink.Math.GetValueAsString(dnDt.Rows[0]["DEC_DATE"]));
                                dt.Rows[0]["REL_DATE"] = getDateFont(Prolink.Math.GetValueAsString(dnDt.Rows[0]["REL_DATE"]));
                                dt.Rows[0]["INSPECTION"] = dnDt.Rows[0]["INSPECTION"].ToString();
                                dt.Rows[0]["CER_NO"] = dnDt.Rows[0]["CER_NO"].ToString();
                                dt.Rows[0]["DEC_REPLY"] = dnDt.Rows[0]["DEC_REPLY"].ToString();
                                dt.Rows[0]["ICDF"] = dnDt.Rows[0]["ICDF"].ToString();
                                dt.Rows[0]["CC_CHANNEL"] = dnDt.Rows[0]["CC_CHANNEL"].ToString();
                                dt.Rows[0]["HS_QTY"] = Prolink.Math.GetValueAsInt(dnDt.Rows[0]["HS_QTY"]); 
                                dt.Rows[0]["CNTRY_CD"] = Prolink.Math.GetValueAsString(dnDt.Rows[0]["CNTRY_CD"]); 
                                dt.Rows[0]["PLI"] = Prolink.Math.GetValueAsString(dnDt.Rows[0]["PLI"]);
                                dt.Rows[0]["LI"] = Prolink.Math.GetValueAsString(dnDt.Rows[0]["LI"]);
                                dt.Rows[0]["SUF_COST"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["SUF_COST"]);
                                dt.Rows[0]["CC_RATE"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["CC_RATE"]);
                                dt.Rows[0]["ADD_QTY"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["ADD_QTY"]);
                                dt.Rows[0]["SIS_FEE"] = Prolink.Math.GetValueAsDouble(dnDt.Rows[0]["SIS_FEE"]);
                                dtAll.Merge(dt);
                            }
                            break;
                    }
                    if (dtAll.Rows.Count > 0)
                    {
                        dtAll.DefaultView.Sort = "TRAN_TYPE DESC";
                    }
                }
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("DownloadExcel Error :" + ex.ToString());
                return   msg = "Fail" + ex.ToString();
            }

            string strName = "DeclarationConfirm.xlsx"; 
            int startRow = 3;
            string company = CompanyId;
             
            string strPath = Server.MapPath("~/download");

            string strName1 = company + "_" + strName;
            string strFile = string.Format(@"{0}\{1}", strPath, strName1);
            bool exists = System.IO.File.Exists(strFile);
            if (!exists)
            {
                strFile = string.Format(@"{0}\{1}", strPath, strName);
            }
            else
            {
                strName = strName1;
            } 
            if (dtAll.Rows.Count > 500)
            {
                msg = @Resources.Locale.L_DNManageController_Controllers_128;
            }
            else
            {
                Business.NPOIExcelHelp exhelp = new Business.NPOIExcelHelp();
                if (!exhelp.Connect_NOPI(strFile))
                {
                    msg = @Resources.Locale.L_DNManage_Controllers_292;
                }
                else
                {
                    NPOI.SS.UserModel.IWorkbook book = exhelp.DataTableToExcel(dtAll, false, startRow, "Sheet1");
                    string dirpath = GetDirPath(Server.MapPath("~/download/") + "backup\\");
                    string finalpathdate = Business.TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss");
                    string excelfile = string.Format("{0}{1}", dirpath + finalpathdate, strName);
                    string filepath = dirpath.Replace(Server.MapPath("~/download/"), "");
                    strName = filepath + finalpathdate + strName;
                    using (FileStream file = new FileStream(excelfile, FileMode.Create))
                    {
                        book.Write(file);
                        file.Close();
                        result = true;
                    }
                }
            }
            return strName;
        }

        private string getTranType(string type)
        {
            switch (type)
            {
                case "F":
                    return "FCL";
                case "R":
                    return "RAIL";
                case "L":
                    return "LCL";
                case "A":
                    return "AIR";
                case "E":
                    return "EXP";
            }
            return string.Empty;
        }
        #endregion

        #region 报关格式上传
        public ActionResult ImportBrokerExcel() 
        {            
            string returnMessage = "success";
           
            
            string autoChk = Prolink.Math.GetValueAsString(Request.Params["autoChk"]);
            int StartRow = 1;
          
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            List<string> shipmentlist = new List<string>();

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            else
            {
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "ImportBrokerExcel\\");
                string excelFileName = string.Format("{0}.{1}", dirpath + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);

                bool isCsm = false;
                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);
                if (dt.Rows.Count > 0)
                {
                    string val = Prolink.Math.GetValueAsString(dt.Rows[0][0]);
                    isCsm = val == "Current Owner";
                }
                if (isCsm)
                {
                    dt = TrackingEDI.Business.ExcelHelper.ImportExcelToDataTable(excelFileName, 6, 0);
                    DataTable newDt = dt.Clone();
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if (j <= 1)
                            continue;
                        newDt.ImportRow(dt.Rows[j]);
                    }
                    returnMessage = UploadCCConfirmByCSM(newDt, shipmentlist);
                    if (!string.IsNullOrEmpty(returnMessage))
                        return Json(new { message = returnMessage });
                }
                else
                {
                    returnMessage = UploadCCConfirmByShipment(dt, shipmentlist);
                    if (!string.IsNullOrEmpty(returnMessage))
                        return Json(new { message = returnMessage });
                }

                if (autoChk == "Y") 
                {
                    foreach (string shipment in shipmentlist)
                    {
                        Result result = DECLBookConfirmFunc(shipment, null);
                        if (!result.Success)
                        {
                            returnMessage += "\n";
                            returnMessage += shipment + " Import Declaration Success!";
                            Prolink.DataOperation.OperationUtils.Logger.WriteLog("ImportBrokerExcel Error :" + shipment + "报关确认失败:" + result.Message);
                        }
                    }
                }                
            }

            return Json(new { message = returnMessage });
        }
        
        #endregion

        public string UploadCCConfirmByCSM(DataTable dt, List<string> shipmentlist)
        {
            MixedList ml = new MixedList();
            string mapping = "TransloadMapping";
            Dictionary<string, object> parm = new Dictionary<string, object>();
            Dictionary<string, List<TransloadHandel>> transloadDic = new Dictionary<string, List<TransloadHandel>>();
            Dictionary<string, string> csmUidDic = new Dictionary<string, string>();
            Dictionary<string, List<string>> rerunCsm = new Dictionary<string, List<string>>();
            parm["transloadInfo"] = transloadDic;
            parm["csmInfo"] = csmUidDic;
            ExcelParser.RegisterEditInstructFunc(mapping, TransloadHandel.HandleTransloadDetail);
            ExcelParser ep = new ExcelParser();
            ep.Save(dt, mapping, "", ml, parm);
            foreach (string truck in transloadDic.Keys)
            {
                List<TransloadHandel> transloadList = transloadDic[truck];
                foreach (TransloadHandel transload in transloadList)
                {
                    if (shipmentlist.Contains(transload.CSMNo))
                        continue;
                    shipmentlist.Add(transload.CSMNo);
                    EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    smsmiei.PutKey("U_ID", transload.CSMUID);
                    smsmiei.PutDate("CI_IN_DATE", transload.CiInDate);
                    smsmiei.Put("DEC_INFO", transload.DecInfo);
                    smsmiei.Put("DEC_DATE_INFO", transload.DecDateInfo);
                    smsmiei.PutDate("REL_DATE", transload.RelDate);
                    smsmiei.PutDate("POD_UPDATE_DATE", transload.PodUpdateDate);
                    ml.Add(smsmiei);
                    EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    smicntrei.PutKey("SHIPMENT_ID", transload.CSMNo);
                    smicntrei.Put("DEC_NO", transload.DecInfo);
                    smicntrei.PutDate("DEC_DATE", Prolink.Math.GetValueAsDateTime(transload.DecDateInfo));
                    smicntrei.PutDate("REL_DATE", transload.RelDate);
                    smicntrei.Put("INSPECTION", "N");
                    smicntrei.Put("CC_RATE", transload.CcRate);
                    smicntrei.Put("FOB_AMT_USD", transload.FobAmt);
                    //smicntrei.Put("IMPORT_NO", transload.ImpLiceReq);
                    ml.Add(smicntrei);
                }
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception EX)
                {
                    return EX.ToString();
                }
            }
            return "";
        }


        public string UploadCCConfirmByShipment(DataTable dt, List<string> shipmentlist)
        {
            string ShipmentId = "", TransType = "", MasterNo = "", CntrNo = "", DnNo = "";
            string MaterialType = "";
            string ermsg = "";
            MixedList ml = new MixedList();
            try
            {
                /*
                 * dr[0]: TranType = TRAN_TYPE
                 * dr[1]: SHIPMENT ID = SHIPMENT_ID
                 * dr[2]: Master B/L = MASTER_NO
                 * dr[3]: Container no.  = CNTR_NO 
                 * dr[4]: DN/Invoice No. = DN_NO
                 * dr[5]: POD = POD_CD
                 * dr[6]: Dest = DEST_CD
                 * dr[7]: Material type = 
                 * dr[8]: DECL NO = DEC_NO
                 * dr[9]: Import No. = IMPORT_NO
                 * dr[10]: CLEARANCE DATE = DEC_DATE
                 * dr[11]: RELEASE DATE = REL_DATE
                 * dr[12]: Inspection = INSPECTION
                 * dr[13]: Certificate No. = CER_NO
                 * dr[14]: Declaration Reply = DEC_REPLY
                 * dr[15]: Faster Solution = ICDF
                 */
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    MixedList ml2 = new MixedList();
                    EditInstruct ei;
                    EditInstruct ei2;
                    DataTable cnDt = new DataTable();

                    DataRow dr = dt.Rows[i];
                    TransType = Prolink.Math.GetValueAsString(dr[0]);
                    ShipmentId = Prolink.Math.GetValueAsString(dr[1]);
                    if (!shipmentlist.Contains(ShipmentId))
                    {
                        shipmentlist.Add(ShipmentId);
                    }
                    MasterNo = Prolink.Math.GetValueAsString(dr[2]);
                    CntrNo = Prolink.Math.GetValueAsString(dr[3]);
                    DnNo = Prolink.Math.GetValueAsString(dr[4]);
                    MaterialType = Prolink.Math.GetValueAsString(dr[7]);
                    string DecNo = Prolink.Math.GetValueAsString(dr[8]);
                    string ImportNo = Prolink.Math.GetValueAsString(dr[9]);
                    DateTime DecDate = Prolink.Math.GetValueAsDateTime(dr[10]);
                    DateTime RelDate = Prolink.Math.GetValueAsDateTime(dr[11]);
                    string Inspection = Prolink.Math.GetValueAsString(dr[12]);
                    string CerNo = Prolink.Math.GetValueAsString(dr[13]);
                    string DecReply = Prolink.Math.GetValueAsString(dr[14]);
                    string ICDF = Prolink.Math.GetValueAsString(dr[15]);
                    string CcChannel = Prolink.Math.GetValueAsString(dr[16]);
                    string HsQty = Prolink.Math.GetValueAsString(dr[17]);
                    string Country = Prolink.Math.GetValueAsString(dr[18]);
                    string pli = "", li = "", sufCost = "", ccRate = "", addQty = "", sisFee = "";
                    try
                    {
                        pli = Prolink.Math.GetValueAsString(dr[19]);
                        li = Prolink.Math.GetValueAsString(dr[20]);
                        sufCost = Prolink.Math.GetValueAsString(dr[21]);
                        ccRate = Prolink.Math.GetValueAsString(dr[22]);
                        addQty = Prolink.Math.GetValueAsString(dr[23]);
                        sisFee = Prolink.Math.GetValueAsString(dr[24]);
                    }
                    catch (Exception e)
                    {
                        return "Please use the new template to upload.";
                    }


                    string[] Dnlist = DnNo.Split(',');
                    if (Inspection == "Yes" || Inspection == "YES")
                    {
                        Inspection = "Y";
                    }
                    else if (Inspection == "No" || Inspection == "NO")
                    {
                        Inspection = "N";
                    }

                    Func<string[], string[], string> checkIsEmpty = (column, descp) =>
                    {
                        string msg = "";
                        for (int j = 0; j < column.Length; j++)
                        {
                            if (string.IsNullOrEmpty(column[j]))
                            {
                                msg += "," + descp[j];
                            }
                        }
                        if (!string.IsNullOrEmpty(msg))
                        {
                            return "ShipmentId :" + ShipmentId + msg + " is empty" + "\n";
                        }
                        return string.Empty;
                    };

                    #region 判断此笔ShipmentId的报关费用是否已经上传了
                    string sql = string.Format("SELECT CMP FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                    string cmp = getOneValueAsStringFromSql(sql);
                    sql = string.Format("SELECT COUNT(1) FROM SMBID WHERE SHIPMENT_ID={0} AND CMP={1} AND FSTATUS IN ('C','D')AND CHG_CD IN ('ICD','ICDF')", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(cmp));
                    int billcount = getOneValueAsIntFromSql(sql);
                    if (billcount > 0)
                    {
                        return string.Format("The Shipment {0} corresponding bill has been uploaded!", ShipmentId);
                    }
                    #endregion
                    Func<EditInstruct, string> UpdateEi = (eii) => {
                        eii.Put("DEC_NO", DecNo);
                        eii.Put("IMPORT_NO", ImportNo);
                        eii.PutDate("DEC_DATE", DecDate);
                        eii.PutDate("REL_DATE", RelDate);
                        eii.Put("INSPECTION", Inspection);
                        eii.Put("CER_NO", CerNo);
                        eii.Put("DEC_REPLY", DecReply);
                        eii.Put("ICDF", ICDF);
                        eii.Put("CC_CHANNEL", CcChannel);
                        eii.Put("HS_QTY", HsQty);
                        eii.Put("CNTRY_CD", Country);
                        if ("BR" == cmp)
                        {
                            eii.Put("PLI", pli);
                            eii.Put("LI", li);
                            eii.Put("SUF_COST", sufCost);
                            eii.Put("CC_RATE", ccRate);
                            eii.Put("ADD_QTY", addQty);
                            eii.Put("SIS_FEE", sisFee);
                        }
                        return string.Empty;
                    };
                    #region 判斷必填欄位
                    switch (TransType)
                    {
                        case "FCL":
                        case "RAIL":
                            string[] columnList = new string[] { TransType, ShipmentId, CntrNo, DecNo, Inspection, dr[10].ToString(), dr[11].ToString() };
                            string[] descpList = new string[] { "Tran Type", "Shipment ID", "Container no.", "DECL NO", "Inspection(Y/N)", "CLEARANCE DATE", "RELEASE DATE" };
                            string msg = checkIsEmpty(columnList, descpList);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                ermsg += msg;
                                break;
                            }


                            ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                            ei.PutKey("SHIPMENT_ID", ShipmentId);
                            ei.PutKey("CNTR_NO", CntrNo);
                            UpdateEi(ei);
                            ml.Add(ei);

                            if (Dnlist.Length > 0)
                            {
                                for (int n = 0; n < Dnlist.Length; n++)
                                {
                                    ei2 = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                                    ei2.PutKey("SHIPMENT_ID", ShipmentId);
                                    ei2.PutKey("INV_NO", Dnlist[n].ToString());
                                    UpdateEi(ei2);
                                    ml.Add(ei2);
                                }
                            }

                            break;
                        case "LCL":
                        case "AIR":
                        case "EXP":
                            string[] columnList2 = new string[] { TransType, ShipmentId, DecNo, Inspection, dr[10].ToString(), dr[11].ToString() };
                            string[] descpList2 = new string[] { "Tran Type", "Shipment ID", "DECL NO", "Inspection(Y/N)", "CLEARANCE DATE", "RELEASE DATE" };
                            string msg2 = checkIsEmpty(columnList2, descpList2);
                            if (!string.IsNullOrEmpty(msg2))
                            {
                                ermsg += msg2;
                                break;
                            }
                            if (Dnlist.Length > 0)
                            {
                                for (int a = 0; a < Dnlist.Length; a++)
                                {
                                    ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                                    ei.PutKey("SHIPMENT_ID", ShipmentId);
                                    ei.PutKey("INV_NO", Dnlist[a].ToString());
                                    UpdateEi(ei);
                                    ml.Add(ei);
                                }
                            }
                            break;
                    }
                    #endregion
                }
                if (!string.IsNullOrEmpty(ermsg))
                    return ermsg;
                if (ml.Count > 0)
                {
                    int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (string shipmentid in shipmentlist)
                    {
                        SetDecInfoToSmsmi(shipmentid);
                    }
                }
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("ImportBrokerExcel Error :" + ex.ToString());
                return ex.ToString();
            }
            return "";
        }
        public ActionResult ImportTCBrokerExcel()
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
            string msg = string.Empty;
            string sql = string.Empty;
            string autoChk = Prolink.Math.GetValueAsString(Request.Params["autoChk"]);
            int StartRow = 1;

            #region 生成上传的excel数据
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            string path = Server.MapPath("~/FileUploads/");
            string strExt = System.IO.Path.GetExtension(file.FileName);
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, "TCCBrokerInfoExcel");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch { }
            path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMMddHHmmfff") + strExt);
            file.SaveAs(path);
            DataTable dt = ImportExcelToDataTable(path, strExt, StartRow);
            #endregion
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId, IoFlag = IOFlag };
            TCCBrokerInfoParser parser = new TCCBrokerInfoParser();
            return Json(new { message = parser.Save(dt, autoChk, userinfo), excelMsg = msg });
        }


        /*測試MAIL*/
        public ActionResult testMail()
        {
            string returnMessage = "success";
            TrackingEDI.Business.EvenFactory.RegisterSendMail("IN", MailManager.SendInboundBookingMail);
            EvenFactory.ExecuteMailEven("IN");
            return Json(new { message = returnMessage });
        }

        #region 批次計算所有費用
        public ActionResult CalculateFee()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request["ShipmentId"]);
            string returnMessage = "success";
            List<string> emptyMessage = new List<string>();
            string[] s = ShipmentId.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < s.Length; i++)
            {
                string sid = s[i];

                if (sid == "")
                {
                    break;
                }

                string Location = getOneValueAsStringFromSql("SELECT CMP FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sid));

                try
                {
                    Bill.WriteLogTagStart("手动计价", sid);
                    string sql = "DELETE FROM SMBID WHERE SHIPMENT_ID={0} AND CMP={1} AND BAMT IS NULL AND CHG_CD NOT IN ('DF')";
                    sql = string.Format(sql, SQLUtils.QuotedStr(sid), SQLUtils.QuotedStr(Location));
                    exeSql(sql);

                    sql = "DELETE FROM TMP_AMT WHERE SHIPMENT_ID={0}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(sid));
                    exeSql(sql);

                    CalculateFee cf = new CalculateFee(sid);
                    if (cf.CHG_TYPE == null)
                        emptyMessage.Add(sid + ":cann't find term vs charge");
                    Business.TimeZoneHelper tz = new Business.TimeZoneHelper();
                    sql = "SELECT RESERVE_NO FROM SMIRV WHERE SHIPMENT_INFO LIKE '%" + sid + "%' OR SHIPMENT_ID=" + SQLUtils.QuotedStr(sid);
                    DataTable dt = getDataTableFromSql(sql);
                    if(dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string ReserveNo = Prolink.Math.GetValueAsString(dr["RESERVE_NO"]);
                            cf.FindTrailerQuote(ReserveNo, sid, emptyMessage);
                        }
                    }
                    InboundHelper.CalIFAndFob(sid);
                    cf.CalBrokerFee(sid, emptyMessage); 
                    cf.CalFreightCalculat(sid);
                    //cf.CalLocalCFee(sid);
                    //cf.CalLocalYFee(sid);
                    cf.CalLocalFee(sid, emptyMessage);
                    cf.CalOutboundFreight(sid, emptyMessage); 
                    InboundTransfer.UpdateBillInfoToSMORD(sid, "", null);
                    Bill.WriteLogTagStart("结束计算", sid);
                    bool isUpdate= CostStatistics.StatisticsProduce(sid, "SYS");
                    if (!isUpdate) 
                        TrackingEDI.Business.CostStatistics.SetCStask(sid, "", "", "SYS_ERROR");  
                }
                catch (Exception ex)
                {

                    return Json(new { message = "error:" + ex.ToString() });
                }
            }

            return Json(new { message = returnMessage, empMsg = emptyMessage });
        }
        #endregion

        #region 批量上传EDOC
        public ActionResult BatchUploadEdoc()
        {
            var files = Request.Files;
            string smsmis = Prolink.Math.GetValueAsString(Request.Params["uidlist"]);
            string location = Prolink.Math.GetValueAsString(Request.Params["olocation"]);
            string[] smsmiarray = smsmis.Trim(',').Split(',');
            string[] sloction = location.Trim(',').Split(',');
            string edoctype = Prolink.Math.GetValueAsString(Request.Params["EdocType"]);
            string groupId = GroupId;
            string cmp = CompanyId;
            if (sloction.Length > 0)
            {
                cmp = sloction[0];
            }
            string stn ="*";
            if (string.IsNullOrEmpty(edoctype))
            {
                return Json(new { message = "fail", IsOk = "N" });
            }
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            int serverNum = 0;
            Dictionary<int, string> dic = new Dictionary<int, string>();
            EDOCResultUploadFile uploadResult = null;
            EDOCFileItem data = null;
            foreach (string uploadId in files)
            {
                HttpPostedFileBase file = files[uploadId];
                dic = new Dictionary<int, string>();
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName); 
                    var path = Path.Combine(Server.MapPath("~/FileUploads"), fileName);
                    file.SaveAs(path);
                    foreach (string uid in smsmiarray)
                    {
                        serverNum = 0;
                        string folderID = Business.TPV.Utils.EDocHelper.CheckDuplicatedFolder(_api, uid, groupId, cmp, stn, "", ref serverNum);
                        if (!dic.ContainsKey(serverNum))
                        {
                            uploadResult = _api.UploadFile(folderID, path, "", UserId, "D", "8", serverNum);
                            if (uploadResult != null && uploadResult.Status == DBErrors.DB_SUCCESS)
                            {
                                data = uploadResult.FileInfo;
                                List<UpdateFileItem> fileList = new List<UpdateFileItem>();
                                fileList.Add(new UpdateFileItem
                                {
                                    FileID = data.FileID,
                                    EdocType = edoctype,
                                    Remark = "BATCH UPLOAD"+ edoctype
                                });
                                bool isSuccess = _api.UpdateFiles(fileList, serverNum);
                            }
                            dic.Add(serverNum, data.FileID);
                        }
                        else
                            _api.CopyFile(dic[serverNum], folderID, serverNum);
                    }
                }
                else
                {
                    return Json(new { message = "fail", IsOk = "N" });
                }
            }
            //Thread tr = new Thread(() => TrackingEDI.Business.EdocHelper.doPoEdocCopy(smsmis, jobNo, groupId, location, stn, fileId, edoctype, "BATCH UPLOAD" + edoctype));

            //try
            //{
            //    tr.Start();
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { message = "fail", IsOk = "N" });
            //}
            return Json(new { message = "success", IsOk = "Y" });
        }

        #endregion

        #region 判斷inbound shipment是否可為放行(單元測試)
        public ActionResult SetSmStatus()
        {

            DelegateConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection();
            string sql = "";
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);

            DateTime Today = DateTime.Now;
            string StrToday_F = Today.ToString("yyyy-MM-dd 00:00:00");
            string StrToday_T = Today.ToString("yyyy-MM-dd 23:59:59");

            #region 掃進口DN檔是否可放行的shipment
            sql = "SELECT SHIPMENT_ID, REL_DATE FROM SMIDN WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2} GROUP BY SHIPMENT_ID, REL_DATE";
            sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, conn);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    //string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    int n = OperationUtils.GetValueAsInt(sql, conn);

                    if (n > 0)
                    {
                        sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                        int m = OperationUtils.GetValueAsInt(sql, conn);

                        if (n == m)
                        {
                            sql = "UPDATE SMSMI SET STATUS='F' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            catch (Exception ex)
                            {
                                
                            }
                        }
                    }

                }
            }
            #endregion

            #region 掃進口貨櫃檔是否可放行的shipment
            sql = "SELECT SHIPMENT_ID, REL_DATE FROM SMICNTR WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2} GROUP BY SHIPMENT_ID, REL_DATE";
            sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
            dt = OperationUtils.GetDataTable(sql, null, conn);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    //string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    int n = OperationUtils.GetValueAsInt(sql, conn);

                    if (n > 0)
                    {
                        sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                        int m = OperationUtils.GetValueAsInt(sql, conn);

                        if (n == m)
                        {
                            sql = "UPDATE SMSMI SET STATUS='F' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            catch (Exception ex)
                            {
                                
                            }
                        }
                    }

                }
            }
            #endregion

            return Json(new { msg = "success" });
        }
        #endregion

        #region 檢查是否可為放行
        static void ChkSm(string UId)
        {
            string sql = string.Empty;
            DateTime Today = DateTime.Now;
            string StrToday_F = Today.ToString("yyyy-MM-dd 00:00:00");
            string StrToday_T = Today.ToString("yyyy-MM-dd 23:59:59");
            string ShipmentId = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(UId));
            #region 掃進口DN檔是否可放行的shipment
            sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            int n = getOneValueAsIntFromSql(sql);

            if (n > 0)
            {
                sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                int m = getOneValueAsIntFromSql(sql);

                if (n == m)
                {
                    sql = "UPDATE SMSMI SET STATUS='J' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    try
                    {
                        exeSql(sql);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            #endregion

            #region 掃進口貨櫃檔是否可放行的shipment
            sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE SHIPMENT_ID" + SQLUtils.QuotedStr(ShipmentId);
            n = getOneValueAsIntFromSql(sql);

            if (n > 0)
            {
                sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                int m = getOneValueAsIntFromSql(sql);

                if (n == m)
                {
                    sql = "UPDATE SMSMI SET STATUS='J' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    try
                    {
                        exeSql(sql);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            #endregion
        }
        #endregion

        #region 計算due Date
        public ActionResult CalDueDate()
        {
            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            TrackingEDI.InboundBusiness.SMSMIHelper.CalDueDate(shipmentid);
            return Json(new {msg = "success" });
        }
        #endregion

        public ActionResult DownLoadSCMRequest()
        {
            string transtype = Request.Params["TransType"];
            string shipmentids = Request.Params["normalshipmentids"];
            string fclshipmentids = Request.Params["fclshipmentids"];
            string Ismodel = Prolink.Math.GetValueAsString(Request.Params["Ismodel"]);
            string[] shipmentlist = shipmentids.Split(';');
            string[] fclshipmentlist = fclshipmentids.Split(';');

            DataTable dt = null;
            string sql = string.Empty;
            DataTable dtAll = null;
            if (fclshipmentlist.Length > 0 && !string.IsNullOrEmpty(fclshipmentids))
            {
                sql = string.Format(@"SELECT SMSMI.SHIPMENT_ID,SMSMI.TRAN_TYPE,MASTER_NO,SMICNTR.CNTR_NO,SMICNTR.DN_NO, '' AS INV_NO,SMICNTR.SCMREQUEST_DATE,SMICNTR.PRIORITY,SMSMI.INBOUND_PO_NO,
                '' AS OPART_NO,'' AS IPART_NO,'' AS GOODS_DESCP,SMICNTR.ETA_MSL,SMICNTR.ETA_MSL_TIME
                FROM SMSMI,SMICNTR WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMSMI.SHIPMENT_ID IN {0}",
                    SQLUtils.Quoted(fclshipmentlist));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                dt.Columns["OPART_NO"].MaxLength = 1000;
                dt.Columns["IPART_NO"].MaxLength = 1000;
                dt.Columns["GOODS_DESCP"].MaxLength = 1000;
                dt.Columns["INV_NO"].MaxLength = 400;
                dtAll = dt.Clone();
                foreach (DataRow dr in dt.Rows)
                {
                    string dnno = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                    if (!string.IsNullOrEmpty(dnno))
                    {
                        string[] dnnos = dnno.Split(',');
                        string inv_sql = string.Format("SELECT OPART_NO,IPART_NO,GOODS_DESCP,INV_NO FROM SMIDNP WHERE DN_NO IN {0}", SQLUtils.Quoted(dnnos));
                        DataTable smindpDt = OperationUtils.GetDataTable(inv_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        List<string> opno = new List<string>();
                        List<string> ipno = new List<string>();
                        List<string> goods = new List<string>();
                        List<string> invnolist = new List<string>();

                        Action<List<string>, string> onAdd = (items, txt) =>
                       {
                           if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                           items.Add(txt);
                       };

                        foreach (DataRow dpdr in smindpDt.Rows)
                        {
                            onAdd(opno, dpdr["OPART_NO"].ToString());
                            onAdd(ipno, dpdr["IPART_NO"].ToString());
                            onAdd(goods, dpdr["GOODS_DESCP"].ToString());
                            onAdd(invnolist, dpdr["INV_NO"].ToString());
                        }
                        string partno = string.Join(",", opno);
                        if (partno.Length > 1000)
                            partno = partno.Substring(0, 999);
                        dr["OPART_NO"] = partno;

                        string ipartno = string.Join(",", ipno);
                        if (ipartno.Length > 1000)
                            ipartno = ipartno.Substring(0, 999);
                        dr["IPART_NO"] = ipartno;

                        string goodsdescp = string.Join(",", goods);
                        if (goodsdescp.Length > 1000)
                            goodsdescp = goodsdescp.Substring(0, 999);
                        dr["GOODS_DESCP"] = goodsdescp;

                        string invno = string.Join(",", invnolist);
                        if (invno.Length > 1000)
                            invno = invno.Substring(0, 399);
                        dr["INV_NO"] = invno;
                        dtAll.ImportRow(dr);
                    }
                    else
                    {
                        dtAll.ImportRow(dr);
                    }
                }
            }
            if (shipmentlist.Length > 0 && !string.IsNullOrEmpty(shipmentids))
            {
                sql = string.Format(@"SELECT SMSMI.SHIPMENT_ID,SMSMI.TRAN_TYPE,MASTER_NO,'' AS CNTR_NO,SMIDN.DN_NO, INV_NO,SCMREQUEST_DATE,SMIDN.PRIORITY,SMSMI.INBOUND_PO_NO,
                '' AS OPART_NO,'' AS IPART_NO,'' AS GOODS_DESCP,SMIDN.ETA_MSL,SMIDN.ETA_MSL_TIME
                FROM SMSMI,SMIDN WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMSMI.SHIPMENT_ID IN {0}",
                        SQLUtils.Quoted(shipmentlist));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                dt.Columns["OPART_NO"].MaxLength = 1000;
                dt.Columns["IPART_NO"].MaxLength = 1000;
                dt.Columns["GOODS_DESCP"].MaxLength = 1000;
                dt.Columns["INV_NO"].MaxLength = 400;
                if (dtAll == null || dtAll.Rows.Count < 0)
                    dtAll = dt.Clone();
                foreach (DataRow dr in dt.Rows)
                {
                    string invno = dr["INV_NO"].ToString();
                    string inv_sql = string.Format("SELECT OPART_NO,IPART_NO,GOODS_DESCP  FROM SMIDNP WHERE INV_NO = {0}", SQLUtils.QuotedStr(invno));
                    DataTable smindpDt = OperationUtils.GetDataTable(inv_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    List<string> opno = new List<string>();
                    List<string> ipno = new List<string>();
                    List<string> goods = new List<string>();

                    Action<List<string>, string> onAdd = (items, txt) =>
                    {
                        if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                        items.Add(txt);
                    };

                    foreach (DataRow dpdr in smindpDt.Rows)
                    {
                        onAdd(opno, dpdr["OPART_NO"].ToString());
                        onAdd(ipno, dpdr["IPART_NO"].ToString());
                        onAdd(goods, dpdr["GOODS_DESCP"].ToString());
                    }
                    string partno = string.Join(",", opno);
                    if (partno.Length > 1000)
                        partno = partno.Substring(0, 999);
                    dr["OPART_NO"] = partno;

                    string ipartno = string.Join(",", ipno);
                    if (ipartno.Length > 1000)
                        ipartno = ipartno.Substring(0, 999);
                    dr["IPART_NO"] = ipartno;

                    string goodsdescp = string.Join(",", goods);
                    if (goodsdescp.Length > 1000)
                        goodsdescp = goodsdescp.Substring(0, 999);
                    dr["GOODS_DESCP"] = goodsdescp;
                    dtAll.ImportRow(dr);
                }
            }
            SCMInfoToExcel scminfotoexcel = new SCMInfoToExcel();
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            string xlsFile = scminfotoexcel.ResetXls(userinfo, dtAll);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", "SCM%20Format.xlsx");
        }

        public ActionResult DownLoadSCMRequestModel()
        {
            string transtype = Request.Params["TransType"];
            string shipmentids = Request.Params["normalshipmentids"];
            string fclshipmentids = Request.Params["fclshipmentids"];
            string[] shipmentlist = shipmentids.Trim(';').Split(';');
            string[] fclshipmentlist = fclshipmentids.Trim(';').Split(';');
            DataTable dt = null;
            string sql = string.Empty;
            DataTable dtAll = null;
            if (fclshipmentlist.Length > 0 && !string.IsNullOrEmpty(fclshipmentids))
            {
                sql = string.Format(@"SELECT I.SHIPMENT_ID,I.BU,DATENAME(WEEK,I.ETD) AS ETD_WK,I.O_LOCATION,I.SH_NM,P.INV_NO,P.PART_NO,P.QTY,P.CNTR_NO,I.MASTER_NO,
I.TRAN_TYPE,I.CARRIER_NM,I.POL_NAME,I.M_VESSEL,I.DEST_NAME,I.ETA,I.PORT_FREE_TIME,I.ASN_DATE,P.ASN_NO,P.PRIORITY,P.SCMREQUEST_DATE,I.GR_STATUS,P.GR_QTY,P.GR_DATE,P.DN_NO,
(SELECT TOP 1 ETA_MSL FROM SMICNTR WHERE CNTR_NO=P.CNTR_NO AND SHIPMENT_ID=P.SHIPMENT_ID) AS ETA_MSL,
(SELECT TOP 1 ETA_MSL_TIME FROM SMICNTR WHERE CNTR_NO=P.CNTR_NO AND SHIPMENT_ID=P.SHIPMENT_ID) AS ETA_MSL_TIME,
(SELECT TOP 1 RESERVE_DATE FROM SMIRV WHERE RESERVE_NO=(SELECT TOP 1 RESERVE_NO FROM SMRCNTR WHERE CNTR_NO=P.CNTR_NO AND SHIPMENT_ID=I.SHIPMENT_ID)) AS RESERVE_DATE,
I.STATUS,I.ZT_NM,I.MONTH,I.WE_NM,I.PONO_INFO,P.GOODS_DESCP,I.CNT20,I.CNT40,I.CNT40HQ,I.OCNT_NUMBER,I.TRADE_TERM,I.POL_CD,I.ETD,I.ATD,I.ETD1,I.VESSEL1,I.ETA1,I.ETD2,
I.VESSEL2,I.ETA2,I.DEST_CD,I.FACT_FREE_TIME,I.ASN_STATUS,
(SELECT TOP 1 ORDER_DATE_L FROM SMIRV WHERE RESERVE_NO=(SELECT TOP 1 RESERVE_NO FROM SMRCNTR WHERE CNTR_NO=P.CNTR_NO AND SHIPMENT_ID=I.SHIPMENT_ID)) AS APPOINTMENT_TIME,
(SELECT TOP 1 ARRIVAL_FACT_DATE_L FROM SMIRV WHERE RESERVE_NO=(SELECT TOP 1 RESERVE_NO FROM SMRCNTR WHERE CNTR_NO=P.CNTR_NO AND SHIPMENT_ID=I.SHIPMENT_ID)) AS ARRIVAL_FACT_DATE
 FROM SMIDNP P LEFT JOIN SMSMI I ON I.SHIPMENT_ID=P.SHIPMENT_ID WHERE P.SHIPMENT_ID IN {0}", SQLUtils.Quoted(fclshipmentlist));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                dt.Columns["INV_NO"].MaxLength = 400;
                dtAll = dt.Clone();
                dtAll.Merge(dt);
            }
            dtAll.Select("");
            if (shipmentlist.Length > 0 && !string.IsNullOrEmpty(shipmentids))
            {
                //if (!string.IsNullOrEmpty(sql))
                //    sql += " UNION ALL ";
                sql = string.Format(@"SELECT I.SHIPMENT_ID,I.BU,DATENAME(WEEK,I.ETD) AS ETD_WK,I.O_LOCATION,I.SH_NM,P.INV_NO,P.PART_NO,P.QTY,'' AS CNTR_NO,I.MASTER_NO,
I.TRAN_TYPE,I.CARRIER_NM,I.POL_NAME,I.M_VESSEL,I.DEST_NAME,I.ETA,I.PORT_FREE_TIME,I.ASN_DATE,P.ASN_NO,P.PRIORITY,P.SCMREQUEST_DATE,I.GR_STATUS,P.GR_QTY,P.GR_DATE,P.DN_NO,
(SELECT TOP 1 ETA_MSL FROM SMIDN WHERE DN_NO=P.DN_NO AND SHIPMENT_ID=P.SHIPMENT_ID) AS ETA_MSL,
(SELECT TOP 1 ETA_MSL_TIME FROM SMIDN WHERE DN_NO=P.DN_NO AND SHIPMENT_ID=P.SHIPMENT_ID) AS ETA_MSL_TIME,
(SELECT TOP 1 RESERVE_DATE FROM SMIRV WHERE RESERVE_NO=(SELECT TOP 1 RESERVE_NO FROM SMRDN WHERE DN_NO=P.DN_NO AND SHIPMENT_ID=I.SHIPMENT_ID)) AS RESERVE_DATE,
I.STATUS,I.ZT_NM,I.MONTH,I.WE_NM,I.PONO_INFO,P.GOODS_DESCP,I.CNT20,I.CNT40,I.CNT40HQ,I.OCNT_NUMBER,I.TRADE_TERM,I.POL_CD,I.ETD,I.ATD,I.ETD1,I.VESSEL1,I.ETA1,I.ETD2,
I.VESSEL2,I.ETA2,I.DEST_CD,I.FACT_FREE_TIME,I.ASN_STATUS,
(SELECT TOP 1 ORDER_DATE_L FROM SMIRV WHERE RESERVE_NO=(SELECT TOP 1 RESERVE_NO FROM SMRDN WHERE DN_NO=P.DN_NO AND SHIPMENT_ID=I.SHIPMENT_ID)) AS APPOINTMENT_TIME,
(SELECT TOP 1 ARRIVAL_FACT_DATE_L FROM SMIRV WHERE RESERVE_NO=(SELECT TOP 1 RESERVE_NO FROM SMRDN WHERE DN_NO=P.DN_NO AND SHIPMENT_ID=I.SHIPMENT_ID)) AS ARRIVAL_FACT_DATE
FROM SMIDNP P LEFT JOIN SMSMI I ON I.SHIPMENT_ID=P.SHIPMENT_ID WHERE P.SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentlist));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dtAll == null || dtAll.Rows.Count < 0)
                    dtAll = dt.Clone();
                dtAll.Merge(dt); 
            }
            dtAll.DefaultView.Sort = "SHIPMENT_ID ASC,CNTR_NO ASC,DN_NO ASC";
            dtAll = dtAll.DefaultView.ToTable();
            //string dtSql = string.Format("SET DATEFIRST 1;SELECT * FROM ({0}) T ORDER BY T.SHIPMENT_ID,T.CNTR_NO,T.DN_NO", sql);
            //DataTable dtAll = OperationUtils.GetDataTable(dtSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SCMModelInfoToExcel scminfotoexcel = new SCMModelInfoToExcel();
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            string xlsFile = scminfotoexcel.ResetXls(userinfo, dtAll);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", "SCM%20by%20Model%20Format.xlsx");
        }

        public ActionResult DownLoadSCMRequestModelToFTP()
        {
            string table = "(SELECT S.*,(SELECT LIGHT FROM SMSM WHERE SMSM.SHIPMENT_ID=S.SHIPMENT_ID)AS O_LIGHT FROM (SELECT * FROM SMSMI WITH (NOLOCK) WHERE GROUP_ID={0} AND EXISTS (SELECT 1 FROM SMSMIPT WITH (NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_NO={1} AND SMSMIPT.PARTY_TYPE IN ('IBCR', 'IBSP','IBBK'))) S";
            string condition = GetBookingQueryConditon(ref table, "");
            string inquiryConditions = ModelFactory.GetInquiryCondition(table, condition, Request.Params);

            string sql = string.Format("SELECT DISTINCT SHIPMENT_ID FROM {0} WHERE {1}", table, inquiryConditions);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return Json(new { message = "No Data !!!", IsOk = "N" });
            }
            if (dt.Rows.Count > 10000)
            {
                return Json(new { message = "More than 10000 queries cannot be downloaded!!!", IsOk="N" });
            }

            DateTime createdate = DateTime.Now;
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("EXPORT_SCMMODFILE", EditInstruct.INSERT_OPERATION);
            ei.Put("CMP", CompanyId);
            ei.Put("S_TABLE", table);
            ei.Put("CONDITIONS", inquiryConditions);
            ei.Put("CREATE_BY", UserId);
            ei.Put("CREATE_BY", UserId);
            ei.Put("FILE_NAME", "SCM by Model Format" + createdate.ToString("yyyyMMddHHmmss"));
            ei.PutDate("CREATE_DATE", createdate);
            ml.Add(ei);
            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = "!!!", IsOk = "Y" });
        }


        public ActionResult DownLoadVizioAsnformat()
        {
            string shipmentids = Request.Params["normalshipmentids"];
            string[] shipmentlist = shipmentids.Split(';');

            DataTable dt = null;
            string sql = string.Empty;
            DataTable dtAll = null;
            if (shipmentlist.Length > 0 && !string.IsNullOrEmpty(shipmentids))
            {
                sql = string.Format(@"SELECT SHIPMENT_ID,WS_CD, PO_NO,CNTR_NO,(SELECT TOP 1 CONVERT(nvarchar(15), ETA, 111) FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID) AS ETA,
                (SELECT TOP 1 CONVERT(nvarchar(15), ETD, 111) FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID) AS ETD,
                (SELECT TOP 1 CARRIER FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID) AS CARRIER,
                '' AS REMARK,'1' AS LINE_NO,(
                SELECT TOP 1 PART_NO FROM SMIDNP WHERE SMIDNP.CNTR_NO=SMICNTR.CNTR_NO AND SMIDNP.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND
                PART_NO IS NOT NULL
                ) AS MODEL,QTY,'' AS ASN_PEG_ID,(
                SELECT TOP 1 CONVERT(INT,SO_NO) FROM SMIDNP WHERE SMIDNP.CNTR_NO=SMICNTR.CNTR_NO AND SMIDNP.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND
                SO_NO IS NOT NULL
                ) AS EXTERNAL_REF,(
                SELECT TOP 1 OPART_NO FROM SMIDNP WHERE SMIDNP.CNTR_NO=SMICNTR.CNTR_NO AND SMIDNP.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND
                OPART_NO IS NOT NULL
                ) AS OPART_NO
                FROM SMICNTR WHERE  SMICNTR.SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentlist));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                dt.Columns["MODEL"].MaxLength = 1000;
                dt.Columns["LINE_NO"].MaxLength = 1000;
                dt.Columns["REMARK"].MaxLength = 1000;
                dt.Columns["ASN_PEG_ID"].MaxLength = 1000;
                if (dtAll == null || dtAll.Rows.Count < 0)
                    dtAll = dt.Clone();

                sql=string.Format("SELECT * FROM BSCODE WHERE  CD_TYPE='TCAR' AND CMP IN ('*',{0})",SQLUtils.QuotedStr(CompanyId));
                DataTable tcardt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in dt.Rows)
                {
                    StringBuilder pegid = new StringBuilder();
                    pegid.Append(InboundHandel.ApartFromSATU(Prolink.Math.GetValueAsDateTime(dr["ETD"])));
                    string wscd = dr["WS_CD"].ToString();
                    string[] wscds = wscd.Split('_');
                    if (wscds.Length >= 2)
                        wscd = wscds[1];
                    if (!string.IsNullOrEmpty(wscd))
                    {
                        pegid.Append("_");
                        pegid.Append(wscd);
                        pegid.Append("_");
                    }
                    pegid.Append(dr["OPART_NO"].ToString());
                    dr["ASN_PEG_ID"] = pegid.ToString();
                    string carrier = dr["CARRIER"].ToString();
                    DataRow[] tcarrows = tcardt.Select(string.Format("CD={0} AND INTTRA IS NOT NULL AND INTTRA <>''", SQLUtils.QuotedStr(carrier)));
                    if(tcarrows.Length>0)
                        carrier=tcarrows[0]["INTTRA"].ToString();
                    else if(carrier.Length >= 2)
                        carrier = carrier.Substring(0, 2).ToUpper();
                    dr["CARRIER"] = carrier;
                    int qty = Prolink.Math.GetValueAsInt(dr["QTY"].ToString());
                    if (qty <= 0)
                    {
                        string cntr = Prolink.Math.GetValueAsString(dr["CNTR_NO"].ToString());
                        string shipment_id= Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"].ToString());
                        sql = string.Format("SELECT ISNULL(SHIPMENT_INFO,SHIPMENT_ID)SHIPMENT_ID FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id));
                        string shipmentinfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string[] shipmentitems = shipmentinfo.Split(',');
                        sql = string.Format("SELECT top 1 QTY,SHIPMENT_ID FROM SMIRV WHERE CNTR_NO={0} AND SHIPMENT_ID IN {1} AND (RV_TYPE IS NULL OR RV_TYPE='') ", SQLUtils.QuotedStr(cntr), SQLUtils.Quoted(shipmentitems));
                        DataTable smrvdt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (smrvdt.Rows.Count > 0)
                        {
                            qty = Prolink.Math.GetValueAsInt(smrvdt.Rows[0]["QTY"]);
                            if (qty <= 0)
                            {
                                string smid = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["SHIPMENT_ID"]);
                                qty = OperationUtils.GetValueAsInt(string.Format("SELECT QTY FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(smid)), Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            dr["QTY"] = qty;
                        }
                    }
                    dtAll.ImportRow(dr);
                }
            }
            VizioDataExcel viziodataexcel = new VizioDataExcel();
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            string xlsFile = viziodataexcel.ResetXls(userinfo, dtAll);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", regex.Match(xlsFile).Value);
        }

        [HttpPost]
        public ActionResult UploadSCMAction(FormCollection form)
        {
            string returnMessage = string.Empty;
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.Replace(".", "");
                    string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadSCMAction\\");
                    string excelFileName = string.Format("{0}.{1}", dirpath + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    mapping = InboundUploadExcelManager.InboundSCMInfoMapping;

                    MixedList ml = new MixedList();
                    MixedList partyml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();

                    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                    parm["mixedlist"] = partyml;
                    List<string> shipmentids=new List<string>();
                    parm["combineDictionary"] = dictionary;
                    parm["shipmentlist"]=shipmentids;
                    parm["condition"] = GetDataPmsCondition("C");
                    ExcelParser.RegisterEditInstructFunc(mapping, InboundUploadExcelManager.HandleSCMInfoExcel);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm, 0);
                    shipmentids=(List<string>)parm["shipmentlist"];
                    if (partyml.Count <= 0)
                    {
                        returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                    }

                    if (partyml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(partyml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            MixedList mixed = new MixedList();
                            string sql = string.Format("SELECT SHIPMENT_ID,POD_CD FROM SMSMI WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentids.ToArray()));
                            DataTable dt = getDataTableFromSql(sql);
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
                            foreach (DataRow dr in dt.Rows)
                            {
                                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                                string shipmentid = dr["SHIPMENT_ID"].ToString();
                                UpdatePriorityToSMSMI(shipmentid);
                                ei.PutKey("SHIPMENT_ID", shipmentid);
                                ei.PutDate("UPLOAD_SCM_DATE", ndt);
                                ei.Put("UPLOAD_SCM_BY", UserId);
                                mixed.Add(ei);
                                Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "016", Cmp = CompanyId, Sender = UserId, Location = dr["POD_CD"].ToString(), LocationName = "", StsDescp = "SCMRequest Date Uploaded" });
                            }
                            if(mixed.Count>0)
                                result = OperationUtils.ExecuteUpdate(mixed, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            if (string.IsNullOrEmpty(returnMessage))
                returnMessage = "success";
            return Json(new { message = returnMessage });
        }

        [HttpPost]
        public ActionResult UploadSCMActionByModel(FormCollection form)
        {
            string returnMessage = string.Empty;
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.Replace(".", "");
                    string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadSCMAction\\");
                    string excelFileName = string.Format("{0}.{1}", dirpath + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    mapping = InboundUploadExcelManager.IbSCMModelInfoMapping;

                    MixedList ml = new MixedList();
                    MixedList partyml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();

                    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                    parm["mixedlist"] = partyml;
                    List<string> shipmentids = new List<string>();
                    List<string> cntrshipments = new List<string>();
                    List<string> dnshipments = new List<string>();
                    parm["shipmentlist"] = shipmentids;
                    parm["cntrshipment"] = cntrshipments;
                    parm["dnshipment"] = dnshipments;
                    parm["condition"] = GetDataPmsCondition("C");

                    //Dictionary<string, string> smexData = new Dictionary<string, string>();
                    //parm["smexData"] = smexData;

                    ExcelParser.RegisterEditInstructFunc(mapping, InboundUploadExcelManager.HandleSCMInfoByModel);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm, 0);
                    shipmentids = (List<string>)parm["shipmentlist"];
                    cntrshipments=(List<string>)parm["cntrshipment"];
                    dnshipments = (List<string>)parm["dnshipment"];
                    //smexData = (Dictionary<string, string>)parm["smexData"];
                    //foreach (var item in smexData)
                    //{
                    //    EditInstruct msei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    //    msei.PutKey("SHIPMENT_ID", item.Key);
                    //    msei.Put("INBOUND_PO_NO", item.Value);
                    //    ml.Add(msei);
                    //}


                    for (int i = 0; i < partyml.Count; i++)
                    {
                        ml.Add((EditInstruct)partyml[i]);
                    }

                    if (ml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            MixedList mixed = new MixedList();
                            string sql = string.Empty;
                            if (cntrshipments.Count > 0)
                            {
                                sql = string.Format("SELECT MIN(PRIORITY) AS PRIORITY,MAX(SCMREQUEST_DATE) AS SCMREQUEST_DATE,CNTR_NO,SHIPMENT_ID FROM SMIDNP WHERE SHIPMENT_ID IN {0} GROUP BY CNTR_NO,SHIPMENT_ID",
                                SQLUtils.Quoted(cntrshipments.ToArray()));
                                DataTable cntrdt = getDataTableFromSql(sql);
                                foreach (DataRow dr in cntrdt.Rows)
                                {
                                    string priority = Prolink.Math.GetValueAsString(dr["PRIORITY"]);
                                    string scmrequestdate = Prolink.Math.GetValueAsString(dr["SCMREQUEST_DATE"]);
                                    string cntrno = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                                    string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);

                                    if (string.IsNullOrEmpty(priority) && string.IsNullOrEmpty(scmrequestdate))
                                        continue;
                                    EditInstruct ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                                    ei.PutKey("SHIPMENT_ID", shipmentid);
                                    ei.PutKey("CNTR_NO", cntrno);
                                    if (!string.IsNullOrEmpty(scmrequestdate))
                                    {
                                        ei.PutDate("SCMREQUEST_DATE", Prolink.Math.GetValueAsDateTime(dr["SCMREQUEST_DATE"]));
                                    }
                                    if (!string.IsNullOrEmpty(priority))
                                    {
                                        ei.Put("PRIORITY", priority);
                                    }
                                    mixed.Add(ei);
                                    EditInstruct idnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                                    idnei.PutKey("SHIPMENT_ID", shipmentid);
                                    bool uploadsmidn = false;
                                    if (!string.IsNullOrEmpty(scmrequestdate))
                                    {
                                        idnei.PutDate("SCMREQUEST_DATE", Prolink.Math.GetValueAsDateTime(dr["SCMREQUEST_DATE"]));
                                        uploadsmidn = true;
                                    }
                                    if (!string.IsNullOrEmpty(priority))
                                    {
                                        idnei.Put("PRIORITY", priority);
                                        uploadsmidn = true;
                                    }
                                    if (uploadsmidn)
                                    {
                                        mixed.Add(idnei);
                                    }
                                }

                            }
                            if (dnshipments.Count > 0)
                            {
                                sql = string.Format("SELECT MIN(PRIORITY) AS PRIORITY,MAX(SCMREQUEST_DATE) AS SCMREQUEST_DATE,INV_NO,SHIPMENT_ID FROM SMIDNP WHERE SHIPMENT_ID IN {0} GROUP BY INV_NO,SHIPMENT_ID",
                               SQLUtils.Quoted(dnshipments.ToArray()));
                                DataTable dndt = getDataTableFromSql(sql);
                                foreach (DataRow dr in dndt.Rows)
                                {
                                    string priority = Prolink.Math.GetValueAsString(dr["PRIORITY"]);
                                    string scmrequestdate = Prolink.Math.GetValueAsString(dr["SCMREQUEST_DATE"]);
                                    string invno = Prolink.Math.GetValueAsString(dr["INV_NO"]);
                                    string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);

                                    if (string.IsNullOrEmpty(priority) && string.IsNullOrEmpty(scmrequestdate))
                                        continue;
                                    EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                                    ei.PutKey("SHIPMENT_ID", shipmentid);
                                    ei.PutKey("INV_NO", invno);
                                    if (!string.IsNullOrEmpty(scmrequestdate))
                                    {
                                        ei.PutDate("SCMREQUEST_DATE", Prolink.Math.GetValueAsDateTime(dr["SCMREQUEST_DATE"]));
                                    }
                                    if (!string.IsNullOrEmpty(priority))
                                    {
                                        ei.Put("PRIORITY", priority);
                                    }
                                    mixed.Add(ei);
                                }
                            }
                            if (mixed.Count > 0)
                                result = OperationUtils.ExecuteUpdate(mixed, Prolink.Web.WebContext.GetInstance().GetConnection());
                            mixed = new MixedList();
                            sql = string.Format("SELECT SHIPMENT_ID,POD_CD FROM SMSMI WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentids.ToArray()));
                            DataTable dt = getDataTableFromSql(sql);
                            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
                            foreach (DataRow dr in dt.Rows)
                            {
                                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                                string shipmentid = dr["SHIPMENT_ID"].ToString();
                                UpdatePriorityToSMSMI(shipmentid);
                                ei.PutKey("SHIPMENT_ID", shipmentid);
                                ei.PutDate("UPLOAD_SCM_DATE", ndt);
                                ei.Put("UPLOAD_SCM_BY", UserId);
                                mixed.Add(ei);
                                Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "016", Cmp = CompanyId, Sender = UserId, Location = dr["POD_CD"].ToString(), LocationName = "", StsDescp = "SCMRequest Date Uploaded" });
                            }
                            if (mixed.Count > 0)
                                result = OperationUtils.ExecuteUpdate(mixed, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            if (string.IsNullOrEmpty(returnMessage))
                returnMessage = "success";
            return Json(new { message = returnMessage });
        }

        public ActionResult UploadSCMActionASN(FormCollection form)
        {
            string returnMessage = string.Empty;
            string type = Prolink.Math.GetValueAsString(Request.Params["type"]);
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.Replace(".", "");
                    string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadSCMAction\\");
                    string excelFileName = string.Format("{0}.{1}", dirpath + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    DataTable dt = ImportExcelToDataTable(excelFileName, strExt, 0);
                    #region 表头参数映射
                    string[] fileds = new string[] { "Shipment ID", "Container No", "Invoice No", "Model Name", "Qty", "ASN NO", "ASN Date", "GR Date", "GR QTY" };
                    string[] filedNames = new string[] { "SHIPMENT_ID", "CNTR_NO", "INV_NO", "PART_NO", "QTY", "ASN_NO", "ASN_DATE", "GR_DATE","GR_QTY" };
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    Dictionary<string, string> map1 = new Dictionary<string, string>();
                    for (int i = 0; i < fileds.Length; i++)
                    {
                        string key = fileds[i];
                        map[key] = filedNames[i];
                    }

                    foreach (DataColumn col in dt.Columns)
                    {
                        string name = col.ColumnName;
                        string key = name;
                        if (map.ContainsKey(key))
                            col.ColumnName = map[key];
                    }
                    #endregion
                    List<string> smList = new List<string>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        if (string.IsNullOrEmpty(ShipmentId) || smList.Contains(ShipmentId))
                            continue;
                        smList.Add(ShipmentId);
                    }
                    MixedList ml = new MixedList();
                    string sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(smList.ToArray()));
                    DataTable inpDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (string shipmentId in smList)
                    {
                        //DataRow[] mapDrs = dt.Select(string.Format(map1["SHIPMENT_ID"] + "={0}", SQLUtils.QuotedStr(shipmentId)));
                        DataRow[] inpDrs = inpDt.Select("SHIPMENT_ID='" + shipmentId + "'");
                        foreach (DataRow inpDr in inpDrs)
                        {
                            DateTime asnDatetime = new DateTime(2000, 1, 1);
                            string cntrNo = Prolink.Math.GetValueAsString(inpDr["CNTR_NO"]);
                            string partNo = Prolink.Math.GetValueAsString(inpDr["PART_NO"]);
                            int qty = Prolink.Math.GetValueAsInt(inpDr["QTY"]);
                            string invNo = Prolink.Math.GetValueAsString(inpDr["INV_NO"]);
                            string uId = Prolink.Math.GetValueAsString(inpDr["U_ID"]);
                            if (string.IsNullOrEmpty(partNo) || qty <= 0)
                                continue;
                            //string cntrCondition = string.IsNullOrEmpty(cntrNo) ? string.Format(" AND {0}={1}", map1["CNTR_NO"], SQLUtils.QuotedStr(cntrNo)) : string.Empty;
                            string baseCondition = string.Format("SHIPMENT_ID={0} AND PART_NO={1} AND QTY={2}", SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(partNo), SQLUtils.QuotedStr(qty.ToString()));
                            string invCondition = !string.IsNullOrEmpty(invNo) ? string.Format(" AND INV_NO={0}", SQLUtils.QuotedStr(invNo)) : string.Empty;
                            DataRow[] mapDrs = dt.Select(baseCondition + invCondition);
                            //if (mapDrs.Length <= 0 && !string.IsNullOrEmpty(cntrCondition))
                            //    mapDrs = dt.Select(baseCondition + cntrCondition);
                            //if (mapDrs.Length <= 0 && !string.IsNullOrEmpty(invCondition))
                            //    mapDrs = dt.Select(baseCondition + invCondition);
                            //if (mapDrs.Length <= 0)
                            //    mapDrs = dt.Select(baseCondition);
                            if (mapDrs.Length > 0)
                            {
                                List<string> asnList = new List<string>();
                                List<string> grLit = new List<string>();
                                List<string> qtyList = new List<string>();

                                foreach (DataRow mapDr in mapDrs)
                                {
                                    string asnNo = Prolink.Math.GetValueAsString(mapDr["ASN_NO"]);
                                    string asnDate = Prolink.Math.GetValueAsDateTime(mapDr["ASN_DATE"]).ToString("yyyy-MM-dd");
                                    string grDate = Prolink.Math.GetValueAsDateTime(mapDr["GR_DATE"]).ToString("yyyy-MM-dd");
                                    string grQty = Prolink.Math.GetValueAsString(mapDr["GR_QTY"]);
                                    if (!asnList.Contains(asnNo) && !string.IsNullOrEmpty(asnNo))
                                        asnList.Add(asnNo);
                                    if (!qtyList.Contains(grQty) && !string.IsNullOrEmpty(grQty))
                                        qtyList.Add(grQty);
                                    if (!grLit.Contains(grDate) && !string.IsNullOrEmpty(grDate) && !"0001-01-01".Equals(grDate))
                                        grLit.Add(grDate);
                                    //DateTime now = DateTime.Now;
                                    if (asnDatetime <= new DateTime(2000, 1, 1))
                                        DateTime.TryParse(asnDate, out asnDatetime);
                                }
                                EditInstruct ei = new EditInstruct("SMIDNP", EditInstruct.UPDATE_OPERATION);
                                ei.PutKey("U_ID", uId);
                                if ("ASN".Equals(type))
                                {
                                    ei.Put("ASN_NO", string.Join(",", asnList));
                                    if(asnDatetime > new DateTime(2000, 1, 1))
                                        ei.PutDate("ASN_DATE", asnDatetime);
                                    if (asnList.Count == 0)
                                    {
                                        ei.Put("ASN_NO", "");
                                        ei.PutDate("ASN_DATE", DBNull.Value);
                                    }
                                }
                                else
                                {
                                    ei.Put("GR_DATE", string.Join(",", grLit));
                                    ei.Put("GR_QTY", string.Join(",", qtyList));
                                    if (grLit.Count == 0)
                                        ei.Put("GR_DATE", "");
                                    if (qtyList.Count == 0)
                                        ei.Put("GR_QTY", "");
                                }
                                ml.Add(ei);
                            }
                        }
                    }

                    if (ml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            MixedList mixed = new MixedList();
                            sql = string.Format("SELECT SHIPMENT_ID,POD_CD,SHIPMENT_INFO,TRAN_TYPE,ASN_DATE,INVOICE_INFO FROM SMSMI WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(smList.ToArray()));
                            DataTable smidt = getDataTableFromSql(sql);
                            //DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
                            TrackingEDI.InboundBusiness.ASNManager.SetAsn(smidt);
                            foreach (DataRow dr in smidt.Rows)
                            {
                                string shipmentid = dr["SHIPMENT_ID"].ToString();
                                if ("ASN".Equals(type))
                                {
                                    Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "ASN", Cmp = CompanyId, Sender = UserId, Location = dr["POD_CD"].ToString(), LocationName = "", StsDescp = "ASN Uploaded" });
                                }
                                else
                                {
                                    Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "GR", Cmp = CompanyId, Sender = UserId, Location = dr["POD_CD"].ToString(), LocationName = "", StsDescp = "GR Uploaded" });
                                }
                            }
                            if (mixed.Count > 0)
                                result = OperationUtils.ExecuteUpdate(mixed, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            if (string.IsNullOrEmpty(returnMessage))
                returnMessage = "success";
            return Json(new { message = returnMessage });
        }

        #region Excel匯入(NEW_BOOKING_CONFIRM)
        public ActionResult QuaryXls()
        {
            bool result = false;
            string msg = string.Empty;
            string uid = string.Empty;
            string filename = string.Empty;
            string IsCntr = Prolink.Math.GetValueAsString(Request.Params["IsCntr"]);
            string file = GetBatchUp(ref result, ref msg);
            return Json(new { IsOk = result ? "Y" : "N", file = file, msg = msg });
        }
        protected string GetBatchUp(ref bool result, ref string msg)
        {
            string strName = "NEW_BOOKING_CONFIRM_AIR.xlsx";
            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["shipmentid"]);
            if (string.IsNullOrEmpty(shipmentid))
            { return @Resources.Locale.L_TKBLQuery_Select; }
            string[] _shipmentid = shipmentid.Split(',');

            string sql = string.Format(@"SELECT A.SHIPMENT_ID,A.TRAN_TYPE, A.MASTER_NO,A.HOUSE_NO, B.INV_NO,A.ATA FROM SMSMI AS A,SMIDN AS B WHERE
                        A.SHIPMENT_ID=B.SHIPMENT_ID AND A.SHIPMENT_ID IN {0}
                         GROUP BY A.SHIPMENT_ID,A.TRAN_TYPE, A.MASTER_NO,A.HOUSE_NO,A.ATA, B.INV_NO", SQLUtils.Quoted(_shipmentid));
           
            string IsCntr = Prolink.Math.GetValueAsString(Request.Params["IsCntr"]);
            if ("Y".Equals(IsCntr))
            {
                strName = "NEW_BOOKING_CONFIRM.xlsx";
                sql = string.Format("select a.SHIPMENT_ID,a.TRAN_TYPE, a.MASTER_NO, b.CNTR_NO, (select c.INV_NO + ',' from SMIDN as c where a.SHIPMENT_ID=c.SHIPMENT_ID FOR XML PATH('')) as INV_NO, a.ATA from SMSMI as a, SMICNTR as b where a.SHIPMENT_ID=b.SHIPMENT_ID and a.SHIPMENT_ID IN {0} group by a.SHIPMENT_ID,a.TRAN_TYPE, a.MASTER_NO, a.ATA, b.CNTR_NO"
                , SQLUtils.Quoted(_shipmentid));
            }
            string strPath = Server.MapPath("~/download");
            string strFile = string.Format(@"{0}\{1}", strPath, strName);

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 500)
            {
                msg = @Resources.Locale.L_DNManageController_Controllers_128;
            }
            else
            {
                Business.NPOIExcelHelp exhelp = new Business.NPOIExcelHelp();
                if (!exhelp.Connect_NOPI(strFile))
                {
                    msg = @Resources.Locale.L_DNManage_Controllers_292;
                }
                else
                {
                    NPOI.SS.UserModel.IWorkbook book = exhelp.DataTableToExcel1(dt, false, 2, true, 0, 0, "NEW_BOOKING_CONFIRM");
                    string FilePath = strFile.Replace(strName, "backup\\");
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    string _strName = strName;
                    strName = "backup\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + strName;
                    strFile = strFile.Replace(_strName, strName);
                    using (FileStream file = new FileStream(strFile, FileMode.Create))
                    {
                        book.Write(file);
                        file.Close();
                        result = true;
                    }
                }
            }
            return strName;
        }

        public void DownLoadXls()
        {
            string strResult = string.Empty;
            string strPath = Server.MapPath("~/download");//D:\U_Disk\V3Tracking\WebGui\Config\excel\AIRBSMapping.xml
            string filename = Prolink.Math.GetValueAsString(Request.Params["filename"]);
            string strName = "NEW_BOOKING_CONFIRM.xls";
            if (!string.IsNullOrEmpty(filename))
                strName = filename;
            string strFile = string.Format(@"{0}\{1}", strPath, strName);

            using (FileStream fs = new FileStream(strFile, FileMode.Open))
            {
                byte[] bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(strName, System.Text.Encoding.UTF8));
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
            }
        }

        #endregion
        public void DownLoadTemplateXls()
        {
            string strResult = string.Empty;
            string strPath = Server.MapPath("~/download");
            string trantype = Prolink.Math.GetValueAsString(Request.Params["TranType"]);
            string filetype = Prolink.Math.GetValueAsString(Request.Params["FileType"]);
            string strName = "Excel.xls";
            switch (trantype)
            {
                case "SMSMIInput":
                    strName = "e-shipping-IB for Outside_Vendor_Template_V1_20240728.xlsx";
                    break;
                case "ImportPaty":
                    strName = "import party template_V1_20240728.xlsx";
                    break;
                default:
                    strName = "SeaFCLBookingStatus_V1_20240728.xlsx";
                    break;
            }
            string strFile = string.Format(@"{0}\{1}", strPath, strName);



            using (FileStream fs = new FileStream(strFile, FileMode.Open))
            {
                byte[] bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(strName, System.Text.Encoding.UTF8).Replace("+", "%20"));
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
            }
        }
        #endregion

        [HttpPost]
        public ActionResult UploadSMICNTR(FormCollection form)
        {

            string returnMessage = "success";
            string ermsg = "";

            int StartRow = 0; //Excel 從第2排開始讀

            string IsCntr = Prolink.Math.GetValueAsString(Request.Params["IsCntr"]);
            bool IsAutoConfirm = Prolink.Math.GetValueAsBool(Request.Params["IsAuto"]);

            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            string strExt = System.IO.Path.GetExtension(file.FileName);
            strExt = strExt.Replace(".", "");
            string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadSMICNTR\\");
            string excelFileName = string.Format("{0}.{1}", dirpath + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
            file.SaveAs(excelFileName);

            DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);
            try
            {
                List<string> splist = new List<string>();
                List<string> cntrList = new List<string>();
                MixedList ml = new MixedList();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    EditInstruct ei;
                    ml = new MixedList();

                    DataRow dr = dt.Rows[i];

                    string IbpaPartyno = string.Empty;
                    string ShipmentID = Prolink.Math.GetValueAsString(dr[0]);
                    if (string.IsNullOrEmpty(ShipmentID))
                        continue;
                    DateTime Ata;
                    if (!splist.Contains(ShipmentID))
                    {
                        splist.Add(ShipmentID);
                    }
                    if ("Y".Equals(IsCntr))
                    {
                        if(!cntrList.Contains(ShipmentID))
                            cntrList.Add(ShipmentID);
                        Ata = Prolink.Math.GetValueAsDateTime(dr[5]);
                        string ContainerNo = Prolink.Math.GetValueAsString(dr[3]);
                        string ERLocation = Prolink.Math.GetValueAsString(dr[6]);
                        string PinNo = Prolink.Math.GetValueAsString(dr[7]);
                        DateTime DischargeTime = Prolink.Math.GetValueAsDateTime(dr[8]);
                        IbpaPartyno = Prolink.Math.GetValueAsString(dr[9]);

                        ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentID);
                        ei.PutKey("CNTR_NO", ContainerNo);
                        ei.Put("BACK_LOCATION", ERLocation);
                        ei.Put("PIN_NO", PinNo);
                        ei.PutDate("DISCHARGE_DATE", DischargeTime);
                        ml.Add(ei);
                        ei = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentID);
                        ei.PutKey("CNTR_NO", ContainerNo);
                        ei.PutDate("DISCHARGE_DATE",DischargeTime);
                        ml.Add(ei);
                    }
                    else
                    {
                        Ata = Prolink.Math.GetValueAsDateTime(dr[5]);
                        string invno = Prolink.Math.GetValueAsString(dr[4]);
                        DateTime dischargetime = Prolink.Math.GetValueAsDateTime(dr[6]);
                        IbpaPartyno = Prolink.Math.GetValueAsString(dr[7]);

                        ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentID);
                        ei.PutKey("INV_NO", invno);
                        ei.PutDate("DISCHARGE_DATE", dischargetime);
                        ml.Add(ei);
                        ei = new EditInstruct("SMRDN", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentID);
                        ei.PutKey("INV_NO", invno);
                        ei.PutDate("DISCHARGE_DATE", dischargetime);
                        ml.Add(ei);
                    }

                    if (Ata.Year != 1)
                    {
                        ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentID);
                        ei.PutDate("ATA", Ata);
                        ml.Add(ei);
                        ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentID);
                        ei.PutDate("ATA", Ata);
                        ml.Add(ei);
                    }
                    
                    if (ml.Count > 0)
                    {
                        int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }

                    if (!string.IsNullOrEmpty(IbpaPartyno))
                    {
                        string sql1 = string.Format("SELECT * FROM SMPTY WHERE STATUS='U' AND PARTY_NO Like '%{0}%'", IbpaPartyno);
                        DataTable smpty = getDataTableFromSql(sql1);
                        if (smpty.Rows.Count > 0)
                        {
                            string sql = string.Format("SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentID));
                            string UId = getOneValueAsStringFromSql(sql);
                            IbpaPartyno = smpty.Rows[0]["PARTY_NO"].ToString();
                            UploadSmsmipt(UId, ShipmentID, "IBTA", IbpaPartyno);
                        }
                    }
                }
                ml = new MixedList();
                foreach (string shipmentId in splist)
                {
                    string updatesql = string.Format("UPDATE SMSMI SET DISCHARGE_DATE=(SELECT TOP 1 DISCHARGE_DATE FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID={0} ORDER BY DISCHARGE_DATE DESC ) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                    if (!cntrList.Contains(shipmentId))
                        updatesql = string.Format("UPDATE SMSMI SET DISCHARGE_DATE=(SELECT TOP 1 DISCHARGE_DATE FROM SMIDN WHERE SMIDN.SHIPMENT_ID={0} ORDER BY DISCHARGE_DATE DESC ) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                    ml.Add(updatesql);
                }
                if (ml.Count > 0)
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                //订舱确认
                if (IsAutoConfirm)
                {
                    string condition=" STATUS NOT IN ('I','C','D','H')";
                    foreach (string shipmentid in splist)
                    {
                        DataTable maindt = GetSMByShipmentId(shipmentid, condition);
                        if (maindt.Rows.Count <= 0)  
                            continue; 
                        string sql = "SELECT * FROM SMIRV WHERE SHIPMENT_INFO LIKE '%" + shipmentid+"%' AND RV_TYPE='I' AND STATUS !='V'";
                        DataTable Smrvdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (Smrvdt.Rows.Count > 0) 
                            continue; 
                        UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
                        string uid = Prolink.Math.GetValueAsString(maindt.Rows[0]["U_ID"]);
                        string confirmmsg = InboundHandel.SetConfirm(userinfo, uid, maindt);
                        if (!string.IsNullOrEmpty(confirmmsg))
                            ermsg += confirmmsg;
                    }
                }
            }
            catch (Exception ex)
            {
                ermsg = "Import Shipment Error" + ex.ToString();
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("Import Shipment Error :" + ex.ToString());
                return Json(new { errorMsg = ermsg });
            }
            return Json(new { message = returnMessage, errorMsg = ermsg });
        }

        public ActionResult GetExportData(string id = null, string uid = null)
        {
            string shipmentid = Prolink.Math.GetValueAsString(Request["ShipmentId"]);
            string type = Prolink.Math.GetValueAsString(Request["TYPE"]);
            if (String.IsNullOrEmpty(shipmentid)) return ToContent(null);
            shipmentid = shipmentid.Trim(',');
            string[] shipmentids = shipmentid.Split(',');

            string sql = string.Format("SELECT O_UID,O_LOCATION, COMBINE_INFO,SHIPMENT_ID,MASTER_NO,(SELECT TOP 1 ISCOMBINE_BL FROM SMSM WHERE SMSM.SHIPMENT_ID=SMSMI.SHIPMENT_ID) AS ISCOMBINE_BL FROM SMSMI WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentids));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach(DataRow dr in dt.Rows)
            {
                string shipmentd = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string ouid = Prolink.Math.GetValueAsString(dr["O_UID"]);
                string olocation = Prolink.Math.GetValueAsString(dr["O_LOCATION"]);
                string masterno = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                DataTable Dt = ReserveManage.GetEdocData(shipmentd, CompanyId, ouid,true);
                if (Dt.Rows.Count <= 0) continue;
                string jobNo = string.Empty;
                string fileNo = string.Empty;
                string cmp = string.Empty ;
                if (!string.IsNullOrEmpty(ouid))
                {
                    jobNo = ouid;
                    fileNo = masterno;
                    cmp = olocation;
                }
                string dep = "";
                foreach (DataRow fllowdr in Dt.Rows)
                {
                    string flowuid = Prolink.Math.GetValueAsString(fllowdr["U_ID"]);
                    string dnno = Prolink.Math.GetValueAsString(fllowdr["DN_NO"]);
                    string flowcmp = Prolink.Math.GetValueAsString(fllowdr["CMP"]);
                    if (string.IsNullOrEmpty(dnno))
                        dnno = masterno;
                    if (string.IsNullOrEmpty(dnno))
                        continue;
                    jobNo += "," + flowuid;
                    fileNo += "," + dnno;
                    cmp += "," + flowcmp;
                }
                jobNo = jobNo.Trim(',');
                fileNo = fileNo.Trim(',');
                cmp = cmp.Trim(',');

                UserInfo userinfo = new UserInfo
                {
                    UserId = UserId,
                    CompanyId = CompanyId,
                    GroupId = GroupId,
                    Upri = UPri,
                    Dep = Dep
                };
                Thread tr = new Thread(() => Business.DownLoadFile.DownLoadFileToDir(jobNo, fileNo, cmp, dep, type, true, userinfo, shipmentd));
                try
                {
                    tr.Start();
                }
                catch (Exception ex)
                {
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog("DownLoadFileToDir :" + ex.ToString());
                }
            }
            return Json("true");
        }

        public ActionResult GetExportData1(string id = null, string uid = null)
        {
            string shipmentid = Prolink.Math.GetValueAsString(Request["ShipmentId"]);
            string type = Prolink.Math.GetValueAsString(Request["TYPE"]);
            string batchtype = Prolink.Math.GetValueAsString(Request["BatchType"]);
            if (String.IsNullOrEmpty(shipmentid)) return ToContent(null);
            shipmentid = shipmentid.Trim(',');
            string[] shipmentids = shipmentid.Split(',');

            MixedList ml = new MixedList();
            foreach(string shipment in shipmentids)
            {
                if (string.IsNullOrEmpty(shipment)) continue;
                EditInstruct ei = new EditInstruct("EXPORT_FILE", EditInstruct.INSERT_OPERATION);
                ei.Put("SHIPMENT_ID", shipment);
                ei.Put("CMP", CompanyId);
                ei.Put("EDOC_TYPE", type);
                ei.Put("USER_ID", UserId);
                ei.Put("CREATE_BY", UserId);
                ei.Put("FOLDER_NAME", batchtype);
                ei.PutDate("CREATE_DATE", DateTime.Now);
                ml.Add(ei);
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                }
            }
            return Json("true");
        }


        public ActionResult GetDNData(string id = null, string uid = null)
        {
            string shipmentid = Prolink.Math.GetValueAsString(Request["ShipmentId"]);
            string OUid = Prolink.Math.GetValueAsString(Request["OUid"]);
            if(string.IsNullOrEmpty(shipmentid))
                return ToContent(null);

            DataTable Dt = ReserveManage.GetEdocData(shipmentid, CompanyId, OUid);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["dn"] = ModelFactory.ToTableJson(Dt, "SmdnModel");
            return ToContent(data);
        }

        public ActionResult UpdateETAInfo()
        {
            string msg = "success";
            string uids = Request.Params["Uids"];
            string[] uidlist = uids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string eta = Request.Params["Eta"];
            string mVessel = Prolink.Math.GetValueAsString(Request.Params["mVessel"]);
            string mVoyage = Prolink.Math.GetValueAsString(Request.Params["mVoyage"]);

            #region Truck calling 使用
            string smordShipmentId = Prolink.Math.GetValueAsString(Request.Params["SmordShipmentId"]);
            if (!string.IsNullOrEmpty(smordShipmentId) && string.IsNullOrEmpty(uids))
            {
                string[] smordList = smordShipmentId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string smsmiSql= string.Format("SELECT DISTINCT U_ID,SHIPMENT_ID FROM SMSMI WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(smordList));
                DataTable dt = OperationUtils.GetDataTable(smsmiSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> smsmiUids = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    smsmiUids.Add(Prolink.Math.GetValueAsString(dr["U_ID"]));
                }
                uidlist = smsmiUids.ToArray();
            }
            #endregion

            MixedList mlist = new MixedList();
            string etaCol = "", vCol = "", sql = "", RailwayCol = "";
            string remark = "", vRemark = "";
            if (!string.IsNullOrEmpty(eta))
            {
                etaCol = string.Format("ETA={0}", SQLUtils.QuotedStr(eta));
                remark = "ETA:" + eta;
            }
            if (!string.IsNullOrEmpty(mVessel))
            {
                vCol = string.Format("M_VESSEL={0},M_VOYAGE={1}", SQLUtils.QuotedStr(mVessel), string.IsNullOrEmpty(mVoyage) ? "null" : SQLUtils.QuotedStr(mVoyage));
                vRemark = string.Format("Mother Vessel:{0};Mother Voyage:{1}", mVessel, mVoyage);
                RailwayCol = string.Format("VESSEL1={0},VOYAGE1={1}", SQLUtils.QuotedStr(mVessel), string.IsNullOrEmpty(mVoyage) ? "null" : SQLUtils.QuotedStr(mVoyage));
            }
            foreach (string index in uidlist)
            {
                if (string.IsNullOrEmpty(index))
                    continue;
                if (!string.IsNullOrEmpty(etaCol))
                {
                    sql = string.Format("UPDATE SMSM SET {0} WHERE SHIPMENT_ID=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1})", etaCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMSM SET {0} WHERE COMBIN_SHIPMENT=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1})", etaCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMSMI SET {0} WHERE U_ID={1}", etaCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMORD SET {0} WHERE SHIPMENT_ID=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1})", etaCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                }
                if (!string.IsNullOrEmpty(vCol))
                {
                    sql = string.Format("UPDATE SMSM SET {0} WHERE SHIPMENT_ID=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1} AND TRAN_TYPE IN ('F','L','R'))", vCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMSM SET {0} WHERE COMBIN_SHIPMENT=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1} AND TRAN_TYPE IN ('F','L','R'))", vCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMSMI SET {0} WHERE U_ID={1} AND TRAN_TYPE IN ('F','L','R')", vCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMORD SET {0} WHERE SHIPMENT_ID=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1} AND TRAN_TYPE IN ('F','L','R'))", vCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                }
                if (!string.IsNullOrEmpty(RailwayCol))
                {
                    sql = string.Format("UPDATE SMSM SET {0} WHERE SHIPMENT_ID=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1} AND TRAN_TYPE='R')", RailwayCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMSM SET {0} WHERE COMBIN_SHIPMENT=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1} AND TRAN_TYPE='R')", RailwayCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMSMI SET {0} WHERE U_ID={1} AND TRAN_TYPE='R'", RailwayCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                    sql = string.Format("UPDATE SMORD SET {0} WHERE SHIPMENT_ID=(SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={1} AND TRAN_TYPE='R')", RailwayCol, SQLUtils.QuotedStr(index));
                    mlist.Add(sql);
                }
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = string.Format("SELECT POD_CD,CMP,TRAN_TYPE,SHIPMENT_ID FROM SMSMI WHERE U_ID IN {0}", SQLUtils.Quoted(uidlist));
                    DataTable smDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow smDr in smDt.Rows)
                    {
                        string pod = Prolink.Math.GetValueAsString(smDr["POD_CD"]);
                        if (string.IsNullOrEmpty(pod))
                            pod = "CNXMN";
                        string tranType = Prolink.Math.GetValueAsString(smDr["TRAN_TYPE"]);
                        string shipmentId = Prolink.Math.GetValueAsString(smDr["SHIPMENT_ID"]);
                        string cmp = Prolink.Math.GetValueAsString(smDr["CMP"]);
                        if (!string.IsNullOrEmpty(eta))
                        {
                            string mRemark = remark;
                            DateTime etaDate = Business.DateTimeUtils.ParseToDateTime(eta);
                            if (etaDate > DateTime.MinValue)
                                eta = etaDate.ToString("yyyyMMdd");
                            if (eta.Length > 8)
                                eta = eta.Replace("-", "").Replace("/", "");
                            if ("L".Equals(tranType) || "F".Equals(tranType) || "R".Equals(tranType))
                                mRemark += ";" + vRemark;
                            Manager.IBSaveStatus(new Status()
                            {
                                ShipmentId = shipmentId,
                                Cmp = cmp,
                                StsCd = "UEA",
                                Sender = UserId,
                                Location = pod,
                                LocationName = "",
                                EventTime = eta,
                                StsDescp = "Upadte ETA/Mother vessel",
                                Remark = mRemark
                            });
                        }
                        else
                        {
                            if ("L".Equals(tranType) || "F".Equals(tranType) || "R".Equals(tranType))
                            {
                                Manager.IBSaveStatus(new Status()
                                {
                                    ShipmentId = shipmentId,
                                    Cmp= cmp,
                                    StsCd = "UMV",
                                    Sender = UserId,
                                    Location = pod,
                                    LocationName = "",
                                    StsDescp = "Upadte Mother vessel",
                                    Remark = vRemark
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                { 
                    msg = "Update ETA Error:" + ex.ToString();
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog(msg);
                }
            }
            return Json(new { message = msg });
        }

        public ActionResult UpdateDischargeInfo(string id = null, string uid = null)
        {
             string changeData = Request.Params["changedData"];
            if (changeData == null)
                return Json(new { message = "No valid Data!" });
            changeData = changeData.Trim(',');
            string[] changes = changeData.Split(',');
            MixedList mixList = new MixedList();
            string CntrNo = string.Empty;
            string InvNo = string.Empty;
            string ShipmentId =string.Empty;
            string sql = string.Empty;
            List<string> cntrshipmentlist = new List<string>();
            List<string> dnshipmentlist = new List<string>();
            foreach (string change in changes)
            {
                string[] updates = change.Replace("DgDate", ",").Split(',');
                if (updates.Length <= 2) continue;

                string discharge = updates[0];
                string idt = updates[1];
                string trantype = updates[2];
                DataTable datable = new DataTable();
                if ("F".Equals(trantype) || "R".Equals(trantype))
                {
                    sql = string.Format(@"SELECT SHIPMENT_ID,CNTR_NO,DN_NO FROM SMICNTR WHERE U_ID={0}", SQLUtils.QuotedStr(idt));
                    datable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (datable.Rows.Count <= 0) continue;
                    CntrNo = Prolink.Math.GetValueAsString(datable.Rows[0]["CNTR_NO"]);
                    ShipmentId = Prolink.Math.GetValueAsString(datable.Rows[0]["SHIPMENT_ID"]);
                    if (!cntrshipmentlist.Contains(ShipmentId))
                    {
                        cntrshipmentlist.Add(ShipmentId);
                    }
                }
                else
                {
                    sql = string.Format(@"SELECT SHIPMENT_ID,INV_NO FROM SMIDN WHERE U_ID={0}", SQLUtils.QuotedStr(idt));
                    datable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (datable.Rows.Count <= 0) continue;
                    InvNo = Prolink.Math.GetValueAsString(datable.Rows[0]["INV_NO"]);
                    ShipmentId = Prolink.Math.GetValueAsString(datable.Rows[0]["SHIPMENT_ID"]);
                    if (!dnshipmentlist.Contains(ShipmentId))
                    {
                        dnshipmentlist.Add(ShipmentId);
                    }
                }
                string table = "SMIDN";
                if (!string.IsNullOrEmpty(CntrNo))
                    table = "SMICNTR";
                if (discharge == "null" || string.IsNullOrEmpty(discharge)) 
                {
                    sql = string.Format(@"UPDATE SMICNTR SET DISCHARGE_DATE=NULL WHERE U_ID={0}", SQLUtils.QuotedStr(idt));
                    mixList.Add(sql);
                    sql = string.Format(@"UPDATE SMIDN SET DISCHARGE_DATE=NULL WHERE U_ID={0}", SQLUtils.QuotedStr(idt));
                    mixList.Add(sql);
                    if (!string.IsNullOrEmpty(CntrNo))
                        sql = string.Format(@"UPDATE SMRCNTR SET DISCHARGE_DATE=NULL WHERE SHIPMENT_ID={0} AND CNTR_NO={1}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(CntrNo));
                    else
                        sql = string.Format(@"UPDATE SMRDN SET DISCHARGE_DATE=NULL WHERE SHIPMENT_ID={0} AND INV_NO={1}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(InvNo));
                }
                else
                {
                    DateTime dischargeDate = Prolink.Math.GetValueAsDateTime(discharge);
                    discharge = dischargeDate.ToString("yyyy-MM-dd");
                    sql = string.Format(@"UPDATE SMICNTR SET DISCHARGE_DATE={0} WHERE U_ID={1}", SQLUtils.QuotedStr(discharge), SQLUtils.QuotedStr(idt));
                    mixList.Add(sql);
                    sql = string.Format(@"UPDATE SMIDN SET DISCHARGE_DATE={0} WHERE U_ID={1}",SQLUtils.QuotedStr(discharge), SQLUtils.QuotedStr(idt));
                    mixList.Add(sql);
                    if (!string.IsNullOrEmpty(CntrNo))
                        sql = string.Format(@"UPDATE SMRCNTR SET DISCHARGE_DATE={2} WHERE SHIPMENT_ID={0} AND CNTR_NO={1}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(CntrNo), SQLUtils.QuotedStr(discharge));
                    else
                        sql = string.Format(@"UPDATE SMRDN SET DISCHARGE_DATE={2} WHERE SHIPMENT_ID={0} AND INV_NO={1}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(InvNo), SQLUtils.QuotedStr(discharge));
                }
                mixList.Add(sql);
                updateDueDate(ShipmentId, mixList, discharge, idt, table,CntrNo);
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    MixedList updatesmiml = new MixedList();
                    foreach (string shipment in cntrshipmentlist)
                    {
                        string updatesql = string.Format("UPDATE SMSMI SET DISCHARGE_DATE=(SELECT TOP 1 DISCHARGE_DATE FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID={0} ORDER BY DISCHARGE_DATE DESC ) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment));
                        updatesmiml.Add(updatesql);
                    }
                    foreach (string shipment in dnshipmentlist)
                    {
                        string updatesql = string.Format("UPDATE SMSMI SET DISCHARGE_DATE=(SELECT TOP 1 DISCHARGE_DATE FROM SMIDN WHERE SMIDN.SHIPMENT_ID={0} ORDER BY DISCHARGE_DATE DESC ) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment));
                        updatesmiml.Add(updatesql);
                    }
                    if (updatesmiml.Count > 0)
                        result = OperationUtils.ExecuteUpdate(updatesmiml, Prolink.Web.WebContext.GetInstance().GetConnection());

                }
                catch (Exception ex)
                {
                }
            }
            return Json(new { message = "success" });
        }

        public void updateDueDate(string shipmentId, MixedList ml, string discharge, string uid, string table, string cntrNo)
        {
            Tuple<string, int, int, int, string, string, string, Tuple<string>> tuple = TrackingEDI.InboundBusiness.InboundHelper.setFreeTime(shipmentId);
            string sql = string.Format("SELECT PORT_FREE_TIME,FACT_FREE_TIME,CON_FREE_TIME,COMBINE_DET,ETA,CMP FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable smDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT EMP_PICK_DATE,PICKUP_CDATE FROM SMICNTR WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count > 0&& dt.Rows.Count>0)
            {
                int PortFreeTime = Prolink.Math.GetValueAsInt(smDt.Rows[0]["PORT_FREE_TIME"]);
                int FactFreeTime = Prolink.Math.GetValueAsInt(smDt.Rows[0]["FACT_FREE_TIME"]);
                int ConFreeTime = Prolink.Math.GetValueAsInt(smDt.Rows[0]["CON_FREE_TIME"]);
                string isCombineDet = Prolink.Math.GetValueAsString(smDt.Rows[0]["COMBINE_DET"]);
                DateTime eta = Prolink.Math.GetValueAsDateTime(smDt.Rows[0]["ETA"]);
                string cmp = Prolink.Math.GetValueAsString(smDt.Rows[0]["CMP"]);
                DateTime empPickDate = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["EMP_PICK_DATE"]);
                DateTime pickDate = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["PICKUP_CDATE"]);
                string factChgDayType = tuple.Item7;
                string portChgDayType = tuple.Rest.Item1;
                string conChgDayType = tuple.Item6;
                DateTime dischargeDate = Prolink.Math.GetValueAsDateTime(discharge);
                PortFeeDate feeDate = new PortFeeDate(shipmentId,cntrNo, isCombineDet, PortFreeTime, FactFreeTime, ConFreeTime, eta, empPickDate, pickDate, dischargeDate, portChgDayType, factChgDayType, conChgDayType);

                DataTable bsDateDt = TrackingEDI.Utils.DayHelper.GetBsdate(cmp);
                var dueDateItem = getDueDate(feeDate, bsDateDt);
                EditInstruct icntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                icntrei.PutKey("U_ID", uid);
                icntrei.PutDate("DETENTION_DUE_DATE", dueDateItem.Item1);
                icntrei.PutDate("DEMURRAGE_DUE_DATE", dueDateItem.Item2);
                icntrei.PutDate("STORAGE_DUE_DATE", dueDateItem.Item3);
                icntrei.Put("PORT_CHG_TYPE", feeDate.PortChgDayType);
                icntrei.Put("FACT_CHG_TYPE", feeDate.FactChgDayType);
                icntrei.Put("CON_CHG_TYPE", feeDate.ConChgDayType);
                EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", feeDate.ShipmentId);
                ei.PutKey("CNTR_NO", feeDate.CntrNo);
                ei.PutDate("DETENTION_DUE_DATE", dueDateItem.Item1);
                ei.PutDate("DEMURRAGE_DUE_DATE", dueDateItem.Item2);
                ei.PutDate("STORAGE_DUE_DATE", dueDateItem.Item3);
                ml.Add(icntrei);
                ml.Add(ei);
            }
        }

        public ActionResult UpdateAtaInfo(string id = null, string uid = null)
        {
            string changeData = Request.Params["changedData"];
            if (changeData == null)
                return Json(new { message = "No valid Data!" });
            changeData = changeData.Trim(',');
            string[] changes = changeData.Split(',');
            MixedList mixList = new MixedList();
            foreach (string change in changes)
            {
                string sql = string.Empty;
                string[] updates = change.Replace("ATA", ",").Split(',');
                if (updates.Length > 1)
                {
                    string idt = updates[1];
                    string ata = updates[0];
                    string quersql = string.Format("SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(idt));
                    string shipmentid = getOneValueAsStringFromSql(quersql);
                    UpdateATA(mixList, ata, shipmentid, UserId);
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                }
            }
            return Json(new { message = "success" });
        }
        public static void UpdateATA(MixedList mixList, string ata, string shipmentid,string UserId)
        {
            string sql = "";
            if (ata == "null")
            {
                sql = string.Format(@"UPDATE SMSMI SET ATA=NULL WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                mixList.Add(sql);
                sql = string.Format(@"UPDATE SMORD SET ATA=NULL WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                mixList.Add(sql);
                sql = string.Format(@"UPDATE SMIRV SET PORT_DATE=NULL WHERE SHIPMENT_INFO like '%{0}%'", shipmentid);
            }
            else
            {
                EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                smsmiei.PutKey("SHIPMENT_ID", shipmentid);
                smsmiei.Put("ATA", ata);
                smsmiei.Put("ATA_UPLOADED", UserId);
                smsmiei.PutDate("ATA_UPLOADED_TIME", DateTime.Now);
                mixList.Add(smsmiei);

                EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("ATA", ata);
                ei.Put("ATA_UPLOADED", UserId);
                ei.PutDate("ATA_UPLOADED_TIME", DateTime.Now);
                mixList.Add(ei);

                sql = string.Format("SELECT ISCOMBINE_BL,COMBIN_SHIPMENT FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string iscombinebl = Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"]);
                        string combinshipment = Prolink.Math.GetValueAsString(dr["COMBIN_SHIPMENT"]);
                        if ("C".Equals(iscombinebl) && !string.IsNullOrEmpty(combinshipment))
                        {
                            EditInstruct ei1 = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                            ei1.PutKey("COMBIN_SHIPMENT", combinshipment);
                            ei1.Put("ATA", ata);
                            ei1.Put("ATA_UPLOADED", UserId);
                            ei1.PutDate("ATA_UPLOADED_TIME", DateTime.Now);
                            mixList.Add(ei1);
                        }
                    }
                }

                sql = string.Format(@"UPDATE SMORD SET ATA={1} WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(ata));
                mixList.Add(sql);
                sql = string.Format(@"UPDATE SMIRV SET PORT_DATE={1} WHERE SHIPMENT_INFO like '%{0}%'", shipmentid, SQLUtils.QuotedStr(ata));
            }
            mixList.Add(sql);
        }

        public ActionResult UpdatePoNoInfo(string id = null, string uid = null)
        {
            string changeData = Request.Params["changedData"];
            if (changeData == null)
                return Json(new { message = "No valid Data!" });
            changeData = changeData.Trim(',');
            string[] changes = changeData.Split(',');
            MixedList mixList = new MixedList();
            List<string> updatesmidnsmrv = new List<string>();
            foreach (string change in changes)
            {
                string[] updates = change.Replace("PoNo", ",").Split(',');
                string[] woupdates = change.Replace("Wo", ",").Split(',');
                string shipmentid = string.Empty;
                if (updates.Length > 1)
                    shipmentid=UpdateWoORPo(mixList, updates);
                if (woupdates.Length > 1)
                    shipmentid = UpdateWoORPo(mixList, woupdates, "WO");
                if (!string.IsNullOrEmpty(shipmentid))
                {
                    if (!updatesmidnsmrv.Contains(shipmentid))
                        updatesmidnsmrv.Add(shipmentid);
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    MixedList ml = new MixedList();
                    foreach(string shipmentid in updatesmidnsmrv)
                    {
                        string sql = "SELECT DN_NO FROM SMIRV WHERE SHIPMENT_INFO LIKE '%" + shipmentid + "%'";
                        string Dn_No = getOneValueAsStringFromSql(sql);
                        if (Dn_No.Contains(","))
                        {
                            string[] Dn_Nos = Dn_No.Split(',');
                            sql = string.Format("SELECT DISTINCT WO+',' FROM SMIDN WHERE DN_NO IN {0}  FOR XML PATH('')", SQLUtils.Quoted(Dn_Nos));
                            string ponolist = getOneValueAsStringFromSql(sql);
                            string updatesmrvsql = @"UPDATE SMIRV SET WO=" + SQLUtils.QuotedStr(ponolist) +" WHERE SHIPMENT_INFO LIKE '%" + shipmentid + "%'";
                            ml.Add(updatesmrvsql);
                            sql = string.Format("SELECT DISTINCT PO_NO+',' FROM SMIDN WHERE DN_NO IN {0}  FOR XML PATH('')", SQLUtils.Quoted(Dn_Nos));
                            ponolist = getOneValueAsStringFromSql(sql);
                            updatesmrvsql = @"UPDATE SMIRV SET PO_NO=" + SQLUtils.QuotedStr(ponolist) + " WHERE SHIPMENT_INFO LIKE '%" + shipmentid + "%'";
                            ml.Add(updatesmrvsql);
                        }
                    }
                    if (ml.Count > 0)
                    {
                        try
                        {
                            result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception e) { }
                    }

                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message });
                }
            }
            return Json(new { message = "success" });
        }

        private string UpdateWoORPo(MixedList mixList, string[] updates, string type = "PO_NO")
        {
            string returndnshipmentid = string.Empty;
            string idt = updates[1];
            string pono = updates[0];
            if ("NULL".Equals(pono.ToUpper()) || string.IsNullOrEmpty(pono))
                return returndnshipmentid;
            string sql = string.Format(@"UPDATE SMICNTR SET " + type + "={0} WHERE U_ID={1}", SQLUtils.QuotedStr(pono), SQLUtils.QuotedStr(idt));
            exeSql(sql);
            sql = string.Format(@"UPDATE SMIDN SET " + type + "={0} WHERE U_ID={1}", SQLUtils.QuotedStr(pono), SQLUtils.QuotedStr(idt));
            exeSql(sql);

            string shipmentid = string.Empty;
            sql = string.Format("SELECT CNTR_NO,SHIPMENT_ID FROM SMICNTR WHERE U_ID={0}", SQLUtils.QuotedStr(idt));
            DataTable smicntr = getDataTableFromSql(sql);
            if (smicntr.Rows.Count > 0)
            {
                string CntrNo = Prolink.Math.GetValueAsString(smicntr.Rows[0]["CNTR_NO"]);
                if (!string.IsNullOrEmpty(CntrNo))
                {
                    shipmentid = Prolink.Math.GetValueAsString(smicntr.Rows[0]["SHIPMENT_ID"]);
                    sql = @"UPDATE SMIRV SET " + type + "=" + SQLUtils.QuotedStr(pono) +
                                " WHERE SHIPMENT_INFO LIKE '%" + shipmentid + "%' AND CNTR_NO LIKE '%" + CntrNo + "%';";
                    mixList.Add(sql);
                    sql = @"UPDATE SMRCNTR SET SMRCNTR." + type + "=SMICNTR." + type +
                               " FROM SMICNTR WHERE SMICNTR.CNTR_NO=SMRCNTR.CNTR_NO AND SMICNTR.SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                    mixList.Add(sql);
                }
            }
            
            sql = string.Format("SELECT SHIPMENT_ID,INV_NO FROM SMIDN WHERE U_ID={0}", SQLUtils.QuotedStr(idt));
            DataTable smidndt = getDataTableFromSql(sql);
            if (smidndt.Rows.Count > 0)
            {
                shipmentid = Prolink.Math.GetValueAsString(smidndt.Rows[0]["SHIPMENT_ID"]);
                returndnshipmentid = shipmentid;
                sql = @"UPDATE SMRDN SET SMRDN." + type + "=SMIDN." + type +
                                    " FROM SMIDN WHERE SMIDN.DN_NO=SMRDN.DN_NO AND SMIDN.U_ID=" + SQLUtils.QuotedStr(idt);
                mixList.Add(sql);
            }
            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
            string str = type.Replace("_", "");
            ei.PutDate(str + "_INPUT_DATE", ndt);
            mixList.Add(ei);
            return returndnshipmentid;
        }

        /// <summary>
        /// booking 叫车  broker   partyType有值时为报关通知
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="even_type"></param>
        /// <param name="partyType"></param>
        /// <param name="isNotifyBroker">是否是通知报关</param>
        public static void SendBookingOrCallMailList(string[] uids, string even_type = "INS",string partyType="",bool isNotifyBroker=false)
        {
            DataTable smsmipt = null;
            DataTable smrv = null;
            switch (even_type)
            {
                case "ICS":
                case "IICS":
                    smrv = OperationUtils.GetDataTable(string.Format("SELECT U_ID,SHIPMENT_ID,SHIPMENT_INFO FROM SMIRV WITH(NOLOCK) WHERE U_ID IN {0}", SQLUtils.Quoted(uids)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    List<string> smList = new List<string>();
                    foreach (DataRow dr in smrv.Rows)
                    {
                        string shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        if (!string.IsNullOrEmpty(shipment_id) && !smList.Contains(shipment_id))
                            smList.Add(shipment_id);
                        shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
                        if (string.IsNullOrEmpty(shipment_id))
                            continue;
                        string[] temps = shipment_id.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var temp in temps)
                        {
                            if (!smList.Contains(temp))
                                smList.Add(temp);
                        }
                    }

                    smsmipt = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT PARTY_TYPE,PARTY_NO,PARTY_NAME,U_FID FROM SMSMIPT WITH(NOLOCK) WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND U_FID IN (SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID IN {0}))A OUTER APPLY (SELECT TOP 1 TRAN_TYPE,GROUP_ID,CMP,SHIPMENT_ID FROM SMSMI M WITH (NOLOCK) WHERE M.U_ID=A.U_FID)B", SQLUtils.Quoted(smList.ToArray())), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
                case "INS":
                    string insSql = string.Format("SELECT * FROM (SELECT PARTY_TYPE,PARTY_NO,PARTY_NAME,U_FID FROM SMSMIPT WITH(NOLOCK) WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND U_FID IN {0})A OUTER APPLY (SELECT TOP 1 TRAN_TYPE,GROUP_ID,CMP FROM SMSMI M WITH (NOLOCK) WHERE M.U_ID=A.U_FID)B", SQLUtils.Quoted(uids));
                    if (!string.IsNullOrEmpty(partyType))
                    {
                        insSql = string.Format("SELECT * FROM (SELECT PARTY_TYPE,PARTY_NO,PARTY_NAME,U_FID FROM SMSMIPT WITH(NOLOCK) WHERE PARTY_TYPE={1} AND U_FID IN {0})A OUTER APPLY (SELECT TOP 1 TRAN_TYPE,GROUP_ID,CMP FROM SMSMI M WITH (NOLOCK) WHERE M.U_ID=A.U_FID)B", SQLUtils.Quoted(uids), SQLUtils.QuotedStr(partyType));
                    }
                    smsmipt = OperationUtils.GetDataTable(insSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
            }
 
            List<string> tranTypes = new List<string>();
            List<string> lspLsit = new List<string>();
            foreach (DataRow dr in smsmipt.Rows)
            {
                string party_no = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                if (!string.IsNullOrEmpty(party_no) && !lspLsit.Contains(party_no))
                    lspLsit.Add(party_no);
                string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                if (!string.IsNullOrEmpty(tranType) && !tranTypes.Contains(tranType))
                    tranTypes.Add(tranType);
            }

            MixedList ml = new MixedList();
            foreach (var tranType in tranTypes)
            {
                foreach (var lspNo in lspLsit)
                {
                    DataRow[] drs = smsmipt.Select(string.Format("PARTY_NO={0} AND TRAN_TYPE={1}", SQLUtils.QuotedStr(lspNo), SQLUtils.QuotedStr(tranType)));
                    if (drs.Length <= 0)
                        continue;
                    List<string> idList = new List<string>();
                    if (smrv != null)
                    {
                        foreach (var dr in drs)
                        {
                            string smid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                            DataRow[] rvs = smrv.Select(string.Format("SHIPMENT_ID={0} OR SHIPMENT_INFO LIKE {1}", SQLUtils.QuotedStr(smid), SQLUtils.QuotedStr("%" + smid + "%")));
                            foreach (var rv in rvs)
                            {
                                string fid = Prolink.Math.GetValueAsString(rv["U_ID"]);
                                if (idList.Contains(fid))
                                    continue;
                                idList.Add(fid);
                            }
                        }
                    }
                    else
                    {

                        foreach (var dr in drs)
                        {
                            string fid = Prolink.Math.GetValueAsString(dr["U_FID"]);
                            if (idList.Contains(fid))
                                continue;
                            idList.Add(fid);
                        }
                    }

                    string groupId = Prolink.Math.GetValueAsString(drs[0]["GROUP_ID"]);
                    string cmp = Prolink.Math.GetValueAsString(drs[0]["CMP"]);
                    string party_name = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                    string guid = Guid.NewGuid().ToString();
                    string subject =isNotifyBroker? @Resources.Locale.L_BookingAction_Controllers_167:GetSubJectByTranType(tranType, tranType, "");
                    EvenFactory.AddEven(string.Format("{0}#{1}#{2}#{3}#{4}#{5}", tranType, lspNo, groupId, cmp, guid, partyType), guid, even_type, ml, 1, 0, party_name, subject, string.Join(";", idList));//mailcc
                }
            }
            if (ml.Count > 0)
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            } 

        }

        public ActionResult DoDirectlyNB()
        {
            string uids = Prolink.Math.GetValueAsString(Request["Uids"]);
            string directtype =Prolink.Math.GetValueAsString(Request["Type"]);
            bool onlynotifybroker = Prolink.Math.GetValueAsBool(Request["onlynotifybroker"]);
            uids = uids.Trim(',');
            string[] uidlist = uids.Split(',');
            DataTable maindt = GetSMByUids(uidlist);
            bool checkIBCR = false;
            if (!onlynotifybroker)
            {
                checkIBCR = true;
            }

            string ermsg = string.Empty;
            string termtype=string.Empty;
            string shipmentid=string.Empty;
            string trantype = string.Empty;
            string WsCd = string.Empty;
            string WsNm = string.Empty;
            string dlvAddr = string.Empty;
            string dlvarea = string.Empty;
            string dlvareanm = string.Empty;
            string addrcd = string.Empty;
            string suid=string.Empty;
            MixedList mlist = new MixedList();
            foreach (DataRow dr in maindt.Rows)
            {
                termtype = dr["INCOTERM_CD"].ToString();
                shipmentid = dr["SHIPMENT_ID"].ToString();
                suid = dr["U_ID"].ToString();
                trantype = dr["TRAN_TYPE"].ToString();
                //通知报关
                DataTable dtAll = maindt.Clone();
                dtAll.ImportRow(dr);

                bool checkflag = CheckDeliveryInfo(shipmentid, trantype, ref WsCd, ref WsNm, ref dlvAddr, ref dlvarea, ref dlvareanm, ref addrcd);
                if (checkflag)
                {
                    ermsg += shipmentid + " Dlv Area Or Dlv Addr is Empty,Please Check it !";
                    continue;
                }
                if (string.IsNullOrEmpty(WsCd))
                {
                    ermsg += shipmentid + " Warehouse is Empty,Please Check it !";
                    continue;
                }
                string Type = "IBBR";
                string isdirectlynb = "Y";
                if ("IBTC".Equals(directtype))
                {
                    Type = "IBTC";
                    isdirectlynb = "T";
                }
                UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
                ermsg += InboundHandel.SendDeclaration(dtAll, Type, userinfo, isdirectlynb, false, checkIBCR);
                if (!string.IsNullOrEmpty(ermsg))
                {
                    if (!ermsg.Contains(@Resources.Locale.L_BookingActionController_Controllers_64))
                        return Json(new { message = ermsg });
                }

                //产生运输单主档
                string resultStr=InboundHandel.CreateOrdNew(shipmentid, suid, mlist);
                if (!string.IsNullOrEmpty(resultStr))
                {                   
                    EditInstruct eiLog = new EditInstruct("SYS_LOG", EditInstruct.INSERT_OPERATION);
                    eiLog.PutKey("ID", System.Guid.NewGuid().ToString());
                    eiLog.Put("MsgType", "DoDirectlyNB");
                    eiLog.Put("RefNO", shipmentid);
                    eiLog.Put("Remark", "CreateOrdNew is error");
                    eiLog.Put("Data", resultStr);
                    eiLog.Put("CreateBy", UserId);                               
                    eiLog.PutExpress("EventTime", "getdate()");
                    OperationUtils.ExecuteUpdate(eiLog, Prolink.Web.WebContext.GetInstance().GetConnection());
                    return Json(new { message = " Create Truck Calling Data failure!" });
                }
                DateTime odt = DateTime.Now;
                DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", suid);
                if (!onlynotifybroker)
                    ei.Put("IS_DIRECTLYNB", isdirectlynb);
                ei.Put("CSTATUS", "Y");
                ei.Put("IB_WINDOW", UserId);
                ei.PutDate("IB_DATE", ndt);
                mlist.Add(ei);

                ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                if (!onlynotifybroker)
                    ei.Put("IS_DIRECTLYNB", isdirectlynb);
                ei.Put("CSTATUS", "Y");
                ei.Put("IB_WINDOW", UserId);
                mlist.Add(ei);
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    ermsg = ex.ToString();
                    EditInstruct eiLog = new EditInstruct("SYS_LOG", EditInstruct.INSERT_OPERATION);
                    eiLog.PutKey("ID", System.Guid.NewGuid().ToString());
                    eiLog.Put("MsgType", "DoDirectlyNB");
                    eiLog.Put("RefNO", shipmentid);
                    eiLog.Put("Remark", "DoDirectlyNB Execute sql is error");
                    eiLog.Put("Data", ermsg);
                    eiLog.Put("CreateBy", UserId);
                    eiLog.PutExpress("EventTime", "getdate()");
                    OperationUtils.ExecuteUpdate(eiLog, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }

            if (string.IsNullOrEmpty(ermsg))
                return Json(new { message = "success" });
            return Json(new { message = ermsg });
        }

        public ActionResult DoQuerySMSMI()
        {
            string uids = Prolink.Math.GetValueAsString(Request["Uids"]);
            uids = uids.Trim(',');
            string[] uidlist = uids.Split(',');
            string table = string.Format(@"(SELECT U_ID, MASTER_NO, HOUSE_NO, ATA FROM SMSMI WHERE U_ID IN {0}) T",
                    SQLUtils.Quoted(uidlist));
            return GetBootstrapData(table, "1=1");
        }

        public ActionResult DoQueryCntrOrDN()
        {
            string uids = Prolink.Math.GetValueAsString(Request["Uids"]);
            uids = uids.Trim(',');
            string[] uidlist = uids.Split(',');
            string sql = string.Format("SELECT DISTINCT TRAN_TYPE FROM SMSMI WHERE U_ID IN {0}",
                    SQLUtils.Quoted(uidlist));
            string trantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dt = new DataTable();
            string table = string.Empty;
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                table = string.Format(@"(SELECT SMICNTR.U_ID,SMSMI.SHIPMENT_ID,SMICNTR.CNTR_NO,SMICNTR.DN_NO, '' AS INV_NO,SMICNTR.DISCHARGE_DATE,SMSMI.TRAN_TYPE
                FROM SMSMI,SMICNTR WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMSMI.U_ID IN {0}) T",
                   SQLUtils.Quoted(uidlist));
            }
            else
            {
                table = string.Format(@"(SELECT SMIDN.U_ID,SMSMI.SHIPMENT_ID,'' AS CNTR_NO,SMIDN.DN_NO,INV_NO,SMIDN.DISCHARGE_DATE,SMSMI.TRAN_TYPE
                FROM SMSMI,SMIDN WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMSMI.U_ID IN {0}) T",
                      SQLUtils.Quoted(uidlist));
            }
            return GetBootstrapData(table, "1=1");
        }

        public ActionResult DoQueryCntrOrDN_PoNo()
        {
            string uids = Prolink.Math.GetValueAsString(Request["Uids"]);
            uids = uids.Trim(',');
            string[] uidlist = uids.Split(',');
            string sql = string.Format("SELECT DISTINCT TRAN_TYPE FROM SMSMI WHERE U_ID IN {0}",
                    SQLUtils.Quoted(uidlist));
            string trantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dt = new DataTable();
            string table = string.Empty;
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                table = string.Format(@"(SELECT SMICNTR.U_ID,SMSMI.SHIPMENT_ID,SMICNTR.CNTR_NO,SMICNTR.DN_NO, INV_NO,SMICNTR.PO_NO,STATUS,SMICNTR.WO
                FROM SMSMI,SMICNTR WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMSMI.U_ID IN {0}) T",
                   SQLUtils.Quoted(uidlist));
            }
            else
            {
                table = string.Format(@"(SELECT SMIDN.U_ID,SMSMI.SHIPMENT_ID,'' AS CNTR_NO,SMIDN.DN_NO,INV_NO,PO_NO,STATUS,WO
                FROM SMSMI,SMIDN WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMSMI.U_ID IN {0}) T",
                      SQLUtils.Quoted(uidlist));
            }
            return GetBootstrapData(table, "1=1");
        }

        public bool StringISEmpty(string value)
        {
            value=value.Trim();
            if ("NULL".Equals(value.ToUpper()))
                return true;
            return string.IsNullOrEmpty(value);
        }

        #region 过账
        public ActionResult PostingBillSAP()
        {
            string returnMsg = "";
            string uids = Request.Params["uids"];
            uids = uids.Trim(',');
            string[] uiditems = uids.Split(',');
            MixedList mlist = new MixedList();
            string dnindex = string.Empty;
            string uidindex = string.Empty;
            string tsapsql = string.Format("SELECT CD FROM BSCODE WHERE CD_TYPE='TSAP' AND CMP={0}", SQLUtils.QuotedStr(CompanyId));
            string sapid = OperationUtils.GetValueAsString(tsapsql,Prolink.Web.WebContext.GetInstance().GetConnection());
            for (int i = 0; i < uiditems.Length; i++)
            {
                uidindex = uiditems[i];
                try
                {
                    string sql = string.Format(@"SELECT SMSMI.CMP, (SELECT TOP 1 RESERVE_DATE FROM SMIRV WHERE RESERVE_NO=(SELECT TOP 1 RESERVE_NO FROM SMRDN WHERE DN_NO=SMIDN.DN_NO AND SHIPMENT_ID=SMIDN.SHIPMENT_ID)) AS DLV_DATE
                        ,SMSMI.ETA,SMIDN.IS_POSTBILL,SMIDN.DN_NO,SMSMI.SHIPMENT_ID,SMSMI.INCOTERM_CD,SMSMI.INCOTERM_DESCP FROM SMIDN,SMSMI WHERE 
                        SMIDN.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMI.U_ID={0} ", SQLUtils.QuotedStr(uidindex));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count <= 0)
                    {
                        continue;
                    }
                    string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                    string incoterm = Prolink.Math.GetValueAsString(dt.Rows[0]["INCOTERM_CD"]);
                    string incotermDescp = Prolink.Math.GetValueAsString(dt.Rows[0]["INCOTERM_DESCP"]);
                    string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                    string scmpSql = string.Format("SELECT IS_ETA_MSL,IS_ETA FROM SCMPB WHERE INCOTERM={0} AND INCOTERM_DESCP={1} AND CMP={2}",
                         SQLUtils.QuotedStr(incoterm), SQLUtils.QuotedStr(incotermDescp), SQLUtils.QuotedStr(cmp));
                    DataTable scmpbDt = OperationUtils.GetDataTable(scmpSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (scmpbDt.Rows.Count <= 0)
                    {
                        returnMsg += @Resources.Locale.L_DNManageController_Controllers_111 + shipmentid + " does not mapping the postbill setup!";
                        continue;
                    }
                    bool ismsl = true;
                    DateTime eta = new DateTime();
                    if ("Y".Equals(scmpbDt.Rows[0]["IS_ETA"]))
                    {
                        if (dt.Rows[0]["ETA"] != null && dt.Rows[0]["ETA"] != DBNull.Value)
                        {
                            eta = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETA"]);
                        }
                        ismsl = false;
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        dnindex = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                        string postflag = Prolink.Math.GetValueAsString(dr["IS_POSTBILL"]);
                        if (postflag.Equals("Y"))
                        {
                            returnMsg += "The Dn No:" + dnindex + @Resources.Locale.L_DNManage_Controllers_247+"/n";
                            continue;
                        }

                        if(ismsl){
                            if (dr["DLV_DATE"] != null && dr["DLV_DATE"] != DBNull.Value)
                            {
                                eta = Prolink.Math.GetValueAsDateTime(dr["DLV_DATE"]);
                            }
                        }

                        Business.TPV.Import.DeliveryPostingManager dpManager = new Business.TPV.Import.DeliveryPostingManager();
                        Business.TPV.Import.DeliveryPostingInfo dpInfo = new Business.TPV.Import.DeliveryPostingInfo();
                        dpInfo.DNNO = dnindex;
                        dpInfo.GoodsMovementDate = eta;
                        dpInfo.CMP = CompanyId;
                        Business.TPV.RFC.DPResultInfo result = null;
                        bool isSucceed = dpManager.TryPostingDate(sapid, dpInfo, out result, "");
                        if (isSucceed)
                        {
                            EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                            ei.PutKey("DN_NO", dnindex);
                            ei.Put("IS_POSTBILL", "Y");
                            ei.PutDate("POST_BILL_DATE", eta);
                            mlist.Add(ei);
                            try
                            {
                                int[] results = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                                returnMsg = @Resources.Locale.L_DNManageController_Controllers_111 + dnindex + @Resources.Locale.L_DNManage_Controllers_248;
                                SMSMIHelper.UpdatePostBill(shipmentid, dnindex);
                            }
                            catch (Exception ex)
                            {
                                returnMsg = ex.ToString();
                            }
                        }
                        else
                        {
                            returnMsg += @Resources.Locale.L_DNManageController_Controllers_112;
                            if (result != null)
                                returnMsg += result.MsgText + "\n";
                        }
                    }
                }
                catch (Exception ex)
                {
                    returnMsg += dnindex + ":" + @Resources.Locale.L_DNManageController_Controllers_113 + ex.ToString() + "\n";
                }
            }
            return Json(new { message = returnMsg });
        }

        #endregion

        public ActionResult OutsourcingToInbound()
        {
            string returnMsg = "Successful!";
            string uids = Request.Params["uids"];
            uids = uids.Trim(',');
            string[] uiditems = uids.Split(',');

            MixedList mlist = new MixedList();
            foreach (string uidindex in uiditems)
            {
                if (string.IsNullOrEmpty(uidindex))
                    continue;
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uidindex);
                ei.Put("OUTSOURCING_TO_INBOUND", "Y");
                mlist.Add(ei);
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = "fail" });
                }
            }

            string sql = string.Format("SELECT SHIPMENT_ID,CMP,COMBINE_INFO from smsmi WHERE U_ID IN {0}", SQLUtils.Quoted(uiditems));
            DataTable dndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dndt.Rows.Count > 0)
            {
                Business.TPV.Helper.SendICACargoInfoByInbound(dndt.Rows[0]);
            }
            return Json(new { message = returnMsg });
        }

        public ActionResult DoToDoorDeliver()
        {
            string uids = Prolink.Math.GetValueAsString(Request["Uids"]);
            uids = uids.Trim(',');
            string ermsg = SMSMIHelper.ToDoorDelivery(uids, UserId, CompanyId, GroupId);
            if (string.IsNullOrEmpty(ermsg))
                return Json(new { message = "success" });
            return Json(new { message = ermsg });
        }

       

        private bool CheckDeliveryInfo(string shipmentid, string trantype, ref string WsCd, ref string WsNm, ref string dlvAddr, ref string dlvarea, ref string dlvareanm, ref string addrcd)
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
            DataTable checkDt = CommonHelp.getDataTableFromSql(checksql);
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

        public ActionResult Send2ISF()
        {
            string returnMsg = "success";
            string ShipmentId = Request.Params["ShipmentId"];
            string ISFAcct = Request.Params["ISFAcct"];
            string ISFPWD = Request.Params["ISFPWD"];
            string pol = string.Empty;
            string pod = string.Empty;
            string ScacCd = string.Empty;
            string MasterNo = string.Empty;
            string HouseNo = string.Empty;
            string uid = string.Empty;
            string cmp = string.Empty;
            string sql = "SELECT POD_CD,POL_CD,SCAC_CD,MASTER_NO,HOUSE_NO,U_ID,CMP FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                pod = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
                pol = Prolink.Math.GetValueAsString(dt.Rows[0]["POL_CD"]);
                ScacCd = Prolink.Math.GetValueAsString(dt.Rows[0]["SCAC_CD"]);
                MasterNo = Prolink.Math.GetValueAsString(dt.Rows[0]["MASTER_NO"]);
                HouseNo = Prolink.Math.GetValueAsString(dt.Rows[0]["HOUSE_NO"]);
                uid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
                cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            }
            string _act = ISFAcct, _pwd = ISFPWD;

            if (string.IsNullOrEmpty(ScacCd))
            {
                return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_126 });
            }
            if (string.IsNullOrEmpty(MasterNo) && string.IsNullOrEmpty(HouseNo))
            {
                return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_132 });
            }
            if (!string.IsNullOrEmpty(uid))
            {
                sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO IN (SELECT PARTY_NO FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE='RE')", SQLUtils.QuotedStr(uid));
                DataTable PTdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (PTdt != null && PTdt.Rows.Count > 0)
                {
                    string bondtype = Prolink.Math.GetValueAsString(PTdt.Rows[0]["BOND_TYPE"]);
                    string bondact = Prolink.Math.GetValueAsString(PTdt.Rows[0]["BOND_ACT"]);
                    if (string.IsNullOrEmpty(bondtype) || string.IsNullOrEmpty(bondact))
                    {
                        return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_1 });
                    }
                }
                else
                {
                    return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_2 });
                }
            }
            try
            {
                if (pod.Substring(0, 2) != "US")
                {
                    return Json(new { message = "fail" });
                }
                if (pod == "" || pol == "")
                {
                    return Json(new { message = "POL OR POD MUST TO SET VALUE" });
                }
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                }
                catch (Exception)
                {
                }
            using (var client = new WebGui.ISFReference.SFServiceSoapClient())
                {

                    Bussiness.CBPISFPostAssistant data = new Bussiness.CBPISFPostAssistant();
                    string xml = data.GetPostData(ShipmentId, UserId, out _act, out _pwd, GetBaseCmp()).ToXml();

                    if (string.IsNullOrEmpty(_act) || string.IsNullOrEmpty(_pwd))
                    {
                        return Json(new { message = "Importer Account was not set. " });
                    }

                    returnMsg = client.Login(_act, _pwd);
                    returnMsg = client.SendOcean(xml);
                    AfterSendISF(ShipmentId, pod, cmp);
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message });
            }
            return Json(returnMsg);

        }

        public void AfterSendISF(string shipmentid, string pol, string cmp)
        {
            MixedList mlist = new MixedList();
            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("IORDER", "Y");
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
            DateTime odt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, cmp);
            ei.PutDate("ISF_SEND_DATE", ndt);
            ei.PutDate("ISF_SEND_DATE_L", odt);
            string uext = BookingStatusManager.GetUserFxt(UserId);
            if (string.IsNullOrEmpty(uext)) uext = UserId;
            ei.Put("ISF_WIN", uext);
            mlist.Add(ei);

            ei = new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.PutKey("CMP", CompanyId);
            ei.Put("IORDER", "Y");
            ei.PutDate("ISF_SEND_DATE", ndt);
            ei.PutDate("ISF_SEND_DATE_L", odt);
            mlist.Add(ei);
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    Manager.IBSaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "050",Cmp=CompanyId, Sender = UserId, Location = pol, LocationName = "", StsDescp = "ISF Upload" });
                }
                catch (Exception ex)
                {
                }
            }
        }

        public string getPartyType(string shipmentId)
        {
            string sql = string.Format("SELECT DISTINCT PARTY_TYPE FROM SMSMIPT WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable smsmipt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string ptlist = string.Empty;
            if (smsmipt.Rows.Count > 0)
            {
                foreach (DataRow dr in smsmipt.Rows)
                {
                    string PartyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    ptlist += PartyType + "|";
                }
                ptlist = ptlist.Substring(0, ptlist.Length - 1);
            }
            return ptlist;
        }

        public ActionResult UpdateStatusToFinish()
        {
            string uids = Prolink.Math.GetValueAsString(Request["uids"]);
            string Message = "success";
            if (!string.IsNullOrEmpty(uids))
            {
                string[] Uids = uids.TrimEnd(';').Split(';');
                string sql = string.Format("UPDATE SMSMI SET STATUS='Z' WHERE U_ID IN {0} AND STATUS ='A'", SQLUtils.Quoted(Uids));
                try {
                    int result = OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex) { Message = ex.ToString(); }
            }
            return Json(new { message = Message });
        }
        
        class TCCBrokerInfoParser : BaseParser
        {
            static bool _register = false;
            static object _lock = new object();
            private static string CreateEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
            {
                return string.Empty;
            }

            public string Save(DataTable dt, string autoChk,UserInfo userinfo)
            {
                if (!_register)
                {
                    lock (_lock)
                    {
                        RegisterEditInstructFunc("TCCBrokerInfoMapping", CreateEditInstruct);
                        _register = true;
                    }
                }
                List<string> msg = new List<string>();
                List<string> msg1 = new List<string>();
                MixedList ml = new MixedList();
                try
                {
                    ParseEditInstruct(dt, "TCCBrokerInfoMapping", ml);
                }
                catch (Exception e)
                {
                    return e.Message;
                }

                MixedList list = new MixedList();
                List<string> shipmentids = new List<string>();

                string trantype = string.Empty;
                for (int j = 0; j < ml.Count; j++)
                {
                    EditInstruct ei = (EditInstruct)ml[j];
                    string shipmentid = ei.Get("SHIPMENT_ID");
                    if (string.IsNullOrEmpty(shipmentid))
                        continue;
                    if(!shipmentids.Contains(shipmentid)){
                    shipmentids.Add(shipmentid);
                    }

                    if(string.IsNullOrEmpty(ei.Get("POD_CD")))
                        throw new Exception(shipmentid + " POD Is Null");
                    ei.Remove("POD_CD");
                    if(string.IsNullOrEmpty(ei.Get("DEST_CD")))
                        throw new Exception(shipmentid + " Dest. Is Null");
                    ei.Remove("DEST_CD");
                    if(string.IsNullOrEmpty(ei.Get("TC_DEC_NO")))
                        throw new Exception(shipmentid + " DECL NO Is Null");
                    if(string.IsNullOrEmpty(ei.Get("TC_DEC_DATE")))
                        throw new Exception(shipmentid + " CLEARANCE DATE Is Null");
                    if(string.IsNullOrEmpty(ei.Get("TC_REL_DATE")))
                        throw new Exception(shipmentid + " RELEASE DATE Is Null");
                    if(string.IsNullOrEmpty(ei.Get("TC_INSPECTION")))
                        throw new Exception(shipmentid + " Inspection(Y/N) Is Null");
                    string dnno = ei.Get("DN_NO").ToString();
                    string[] Dnlist = dnno.Split(',');

                    trantype = ei.Get("TRAN_TYPE");
                    ei.Remove("TRAN_TYPE");
                    switch (trantype)
                    {
                        case "FCL":
                        case "RAIL":
                            string cntrno = ei.Get("CNTR_NO").ToString();
                            if (string.IsNullOrEmpty(cntrno))
                            {
                                msg1.Add(string.Format("Shipment:{0} Container No. Is Null!", shipmentid));
                                break;
                            }

                            string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentid));
                            DataTable cnDt = getDataTableFromSql(sql);
                            if (cnDt.Rows.Count > 0) 
                            {
                                sql = string.Format("SELECT DISTINCT SHIPMENT_ID FROM SMICNTR WHERE CNTR_NO = {0}", SQLUtils.QuotedStr(cntrno));
                                DataTable dt1 = getDataTableFromSql(sql);
                                if (dt1.Rows.Count == 1)
                                {
                                    ei.ID = "SMICNTR";
                                    ei.OperationType = EditInstruct.UPDATE_OPERATION;
                                    ei.PutKey("CNTR_NO", cntrno);
                                    ei.PutKey("SHIPMENT_ID", shipmentid);
                                    ei.Remove("DN_NO");
                                    list.Add(ei);
                                }
                                else if (dt1.Rows.Count >= 2)
                                {
                                    if (Dnlist.Length > 0)
                                    {
                                        for (int i = 0; i < Dnlist.Length; i++)
                                        {
                                            ei.OperationType = EditInstruct.UPDATE_OPERATION;
                                            ei.PutKey("INV_NO", Dnlist[i].ToString());
                                            ei.PutKey("SHIPMENT_ID", shipmentid);
                                            ei.Remove("DN_NO");
                                            ei.Remove("CNTR_NO");
                                            list.Add(ei);
                                        }
                                    }
                                }
                            }
                            break;
                        case "LCL":
                        case "AIR":
                        case "EXP":
                            if (Dnlist.Length > 0)
                            {
                                for (int i = 0; i < Dnlist.Length; i++)
                                {
                                    ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                                    ei.PutKey("INV_NO", Dnlist[i].ToString());
                                    ei.PutKey("SHIPMENT_ID", shipmentid);
                                    list.Add(ei);
                                }
                            }
                            break;
                    }
                }

                for (int j = 0; j < ml.Count; j++)
                {
                    EditInstruct ei = (EditInstruct)ml[j];
                    string shipmentid = ei.Get("SHIPMENT_ID");
                    if (string.IsNullOrEmpty(shipmentid))
                        continue;
                    SetTcDecInfoToSmord(shipmentid, list);
                }

                try
                {
                    if (list.Count > 0)
                        OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
                    msg.Add("Import T.C.C Info. Update success");
                    if (autoChk == "Y")
                    {
                        string sql=string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID IN {0}",SQLUtils.Quoted(shipmentids.ToArray()));
                        DataTable smsmidt=getDataTableFromSql(sql);

                        foreach (DataRow dr  in smsmidt.Rows)
                        {
                            InboundHandel.TCBookConfirmFunc(dr, userinfo);
                        }
                    }  
                }
                catch (Exception e)
                {
                    msg.Add("update fail:" + e.Message);
                }
                return string.Join("\r\n", msg);
            }
        }

        public ActionResult BatchDelUnReach()
        {
            string returnMsg = "Successful!";
            string shipments = Request.Params["shipments"];
            shipments = shipments.Trim(',');
            string[] shipmentItems = shipments.Split(',');

            MixedList mlist = new MixedList();
            foreach (string shipmentId in shipmentItems)
            {
                if (string.IsNullOrEmpty(shipmentId))
                    continue;
                string sql = string.Format("DELETE FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                mlist.Add(sql);
                sql = string.Format("DELETE FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                mlist.Add(sql);
                sql = string.Format("DELETE FROM SMSMIPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                mlist.Add(sql);
                sql = string.Format("DELETE FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                mlist.Add(sql);
                sql = string.Format("DELETE FROM SMIDNP WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                mlist.Add(sql);

                if (mlist.Count > 0)
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                        AfterDeleteSendToSAP(shipmentId);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { message = "fail" });
                    }
                }
            }
            return Json(new { message = returnMsg });
        }

        public ActionResult ResendToOutbound()
        {
            string returnMsg = "Successful!";
            string shipments = Request.Params["shipments"];
            shipments = shipments.Trim(',');
            string[] shipmentItems = shipments.Split(',');
            string backremark = Request.Params["BackRemark"];
            MixedList mlist = new MixedList();
            foreach (string shipmentId in shipmentItems)
            {
                if (string.IsNullOrEmpty(shipmentId))
                    continue;
                string sql= string.Format("UPDATE SMSM SET IS_OK='R' WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                mlist.Add(sql);
                sql = string.Format("UPDATE SMSMI SET RESEND_FLAG='Y' WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                mlist.Add(sql);

                EditInstruct ei = new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
                string uid = Guid.NewGuid().ToString();
                ei.Put("U_ID", uid);
                ei.Put("U_FID", uid);
                ei.Put("JOB_NO", shipmentId);
                ei.PutExpress("SEQ_NO", @"(SELECT 
   (CASE  WHEN MAX(SEQ_NO) IS NULL THEN 1 WHEN MAX(SEQ_NO) = '' THEN 1 ELSE MAX(SEQ_NO)+1 END)
     FROM TMEXP WHERE
   JOB_NO ='" + shipmentId + "')");
                ei.Put("EXP_TEXT", @Resources.Locale.L_BookingAction_Controllers_152 + ":" + UserId + " Resend to Outbound!"+
                    @Resources.Locale.L_BillApproveHelper_Business_30+ backremark);
                ei.PutDate("CREATE_DATE", DateTime.Now);
                ei.Put("CREATE_BY", UserId);
                ei.PutDate("WR_DATE", DateTime.Now);
                ei.Put("WR_ID", UserId);
                ei.Put("EXP_OBJ", UserId);
                ei.Put("EXP_TYPE", "SM");
                ei.Put("EXP_REASON", "RTO");
                ei.Put("GROUP_ID", GroupId);
                ei.Put("CMP", CompanyId);
                mlist.Add(ei);

            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string sql = string.Format("SELECT COMBINE_INFO,SHIPMENT_ID,CMP,'N' AS IS_OK,POD_CD FROM SMSM WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentItems));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    List<string> shipmentList = new List<string>();
                    foreach (DataRow smDr in dt.Rows)
                    {
                        string shipmentId = Prolink.Math.GetValueAsString(smDr["SHIPMENT_ID"]);
                        string podCd = Prolink.Math.GetValueAsString(smDr["POD_CD"]);
                        string cmp = Prolink.Math.GetValueAsString(smDr["CMP"]);
                        if (shipmentList.Contains(shipmentId))
                            continue;
                        shipmentList.Add(shipmentId);
                        Manager.IBSaveStatus(new Status()
                        {
                            ShipmentId = shipmentId,
                            Cmp=cmp,
                            StsCd = "RTO",
                            Sender = UserId,
                            Location = podCd,
                            LocationName = "",
                            StsDescp = "Resend To Outbound",
                            Remark = backremark
                        });
                        SendICACargoInfo(shipmentId);
                    }
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                }
            }
            return Json(new { message = returnMsg });
        }

        #endregion

        public ActionResult SmicntrExport()
        {
            try
            {
                string uid = Request.Params["conditions"].ToString();
                string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID= {0})", SQLUtils.QuotedStr(uid));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                return Export(dt);
            }
            catch (Exception e)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("SmicntrExport error:" + e.ToString());
                throw;
            }

        }

        public ActionResult SmidnExport()
        {
            try
            {
                string uid = Request.Params["conditions"].ToString();              
                string sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID= {0})", SQLUtils.QuotedStr(uid));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                return Export(dt);
            }
            catch (Exception e)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("SmidnExport error:" + e.ToString());
                throw;
            }

        }

        public ActionResult SmidnpExport()
        {
            try
            {
                string uid = Request.Params["conditions"].ToString();
                string sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMSMI WHERE U_ID= {0})", SQLUtils.QuotedStr(uid));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                return Export(dt);
            }
            catch (Exception e)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("SmidnpExport error:" + e.ToString());
                throw;
            }

        }      
        public ActionResult Export(DataTable dt)
        {  
            JavaScriptSerializer js = new JavaScriptSerializer();
            int recordsCount = 0, pageIndex = 0, pageSize = 0; 
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt); 
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        public ActionResult DownloadEmptyReturn()
        {
            string msg = string.Empty;
            bool result = false;
            string strName = "EmptyReturn.xlsx";
            string shipment = Prolink.Math.GetValueAsString(Request.Params["shipments"]);
            string[] shipments = shipment.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string sql = string.Format("SELECT SHIPMENT_ID,CNTR_NO,TRUCKER_NM1,POL_NM1,BACK_LOCATION,CASE WHEN EMPTY_TIME IS NULL THEN '' ELSE CONVERT(nvarchar,EMPTY_TIME,23)END AS EMPTY_TIME FROM SMICNTR WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipments));
            DataTable icntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string strPath = Server.MapPath("~/download");
            string strFile = string.Format(@"{0}\{1}", strPath, strName);
            Business.NPOIExcelHelp exhelp = new Business.NPOIExcelHelp();
            if (!exhelp.Connect_NOPI(strFile))
            {
                msg = @Resources.Locale.L_DNManage_Controllers_292;
            }
            else
            {
                NPOI.SS.UserModel.IWorkbook book = exhelp.DataTableToExcel1(icntrDt, false, 3, true, 0, 0);
                string dirpath = GetDirPath(Server.MapPath("~/download/") + "backup\\");
                string finalpathdate = Business.TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss");
                string excelfile = string.Format("{0}{1}", dirpath + finalpathdate, strName);
                string filepath = dirpath.Replace(Server.MapPath("~/download/"), "");
                strName = filepath + finalpathdate + strName;
                using (FileStream file = new FileStream(excelfile, FileMode.Create))
                {
                    book.Write(file);
                    file.Close();
                    result = true;
                }
            }
            return Json(new { IsOk = result ? "Y" : "N", file = strName, msg = msg });
        }

        public ActionResult UploadEmptyReturn()
        {
            string returnMessage = string.Empty;
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping = "EmptyReturnMapping";
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.Replace(".", "");
                    string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadEmptyReturn\\");
                    string excelFileName = string.Format("{0}.{1}", dirpath + Business.TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    MixedList ml = new MixedList();
                    ExcelParser.RegisterEditInstructFunc(mapping, HandleFreightQuotDetail);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, null, 1);
                    if (ml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            if (string.IsNullOrEmpty(returnMessage))
                returnMessage = "success";
            return Json(new { message = returnMessage });

        }

        public static string HandleFreightQuotDetail(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.Remove("TRUCKER_NM1");
            ei.Remove("POL_NM1");
            ei.AddKey("SHIPMENT_ID");
            ei.AddKey("CNTR_NO");
            return string.Empty;
        }


        public ActionResult DownloadTransload()
        {
            string msg = string.Empty;
            bool result = false;
            string strName = "TransloadExcel.xlsx";
            string uid = Prolink.Math.GetValueAsString(Request.Params["uids"]);
            string[] uids = uid.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string sql = string.Format(@"SELECT '' as First,MASTER_NO,INV_NO,'' AS PACKING_LIST,CNTR_NO,'' AS CNTR_SEAL,'' AS LICENSE_REQUEST,
'' AS LICENSE_APPROVAL_DATE,ATA,'' AS TC_DEC_NO,'' AS TC_DEC_DATE,'' AS TS_TRAILER,'' AS TS_TRUCK,
CASE WHEN PLA_NO IS NULL THEN 'Case No:'+CASE_NO ELSE 'Pallet No:'+PLA_NO END,QTY  FROM SMIDNPL WHERE U_ID IN {0} AND 
NOT EXISTS (SELECT 1 FROM SMSMI WHERE SMSMI.U_ID=SMIDNPL.U_FID AND BSTATUS='C')", SQLUtils.Quoted(uids));
            DataTable icntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string strPath = Server.MapPath("~/download");
            string strFile = string.Format(@"{0}\{1}", strPath, strName);
            Business.NPOIExcelHelp exhelp = new Business.NPOIExcelHelp();
            if (!exhelp.Connect_NOPI(strFile))
            {
                msg = @Resources.Locale.L_DNManage_Controllers_292;
            }
            else
            {
                NPOI.SS.UserModel.IWorkbook book = exhelp.DataTableToExcel1(icntrDt, false, 10, true, 0, 0);
                string dirpath = GetDirPath(Server.MapPath("~/download/") + "backup\\");
                string finalpathdate = Business.TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss");
                string excelfile = string.Format("{0}{1}", dirpath + finalpathdate, strName);
                string filepath = dirpath.Replace(Server.MapPath("~/download/"), "");
                strName = filepath + finalpathdate + strName;
                using (FileStream file = new FileStream(excelfile, FileMode.Create))
                {
                    book.Write(file);
                    file.Close();
                    result = true;
                }
            }
            return Json(new { IsOk = result ? "Y" : "N", file = strName, msg = msg });
        }

        public ActionResult UploadTransload()
        {
            string returnMessage = string.Empty;
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
                string mapping = "TransloadMapping";
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.Replace(".", "");
                    string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadTransload\\");
                    string excelFileName = string.Format("{0}.{1}", dirpath + Business.TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    DataTable dt = TrackingEDI.Business.ExcelHelper.ImportExcelToDataTable(excelFileName, 6, 0);
                    DataTable newDt = dt.Clone();
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if (j <= 1)
                            continue;
                        newDt.ImportRow(dt.Rows[j]);
                    }
                    string sql = string.Empty;
                    MixedList ml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    Dictionary<string, List<TransloadHandel>> transloadDic = new Dictionary<string, List<TransloadHandel>>();
                    Dictionary<string, string> csmUidDic = new Dictionary<string, string>();
                    Dictionary<string, List<string>> rerunCsm = new Dictionary<string, List<string>>();
                    parm["transloadInfo"] = transloadDic;
                    parm["csmInfo"] = csmUidDic;
                    ExcelParser.RegisterEditInstructFunc(mapping, TransloadHandel.HandleTransloadDetail);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(newDt, mapping, excelFileName, ml, parm);
                    List<string> uidList = new List<string>();
                    foreach (string truck in transloadDic.Keys)
                    {
                        string csmuid = TransloadHandel.CreateConsignment(transloadDic[truck], csmUidDic, ml, UserId, rerunCsm);
                        if (!string.IsNullOrEmpty(csmuid) && !uidList.Contains(csmuid))
                            uidList.Add(csmuid);
                    }
                    TransloadHandel.rerunConsignment(rerunCsm, ml);
                    if (ml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            DataTable maindt = GetSMByUids(uidList.ToArray());
                            foreach (DataRow dr in maindt.Rows)
                            {
                                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                                string csmNo = Prolink.Math.GetValueAsString(dr["CSM_NO"]);
                                BookingParser bp = new BookingParser();
                                bp.SaveToTrackingByIBShipmentid(new string[] { csmNo });
                                InboundHandel.UpdateShipmentStatusByCsmUId(uid, "I");
                            }
                            returnMessage = InboundHandel.SendDeclaration(maindt, "IBBR", userinfo, "");
                            foreach (DataRow dr in maindt.Rows)
                            {
                                string csmNo = Prolink.Math.GetValueAsString(dr["CSM_NO"]);
                                string u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                                if (string.IsNullOrEmpty(csmNo))
                                    continue;
                                InboundHandel.CreateOrdNew(csmNo, u_id);
                            }
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            if (string.IsNullOrEmpty(returnMessage))
                returnMessage = "success";
            return Json(new { message = "success", errorMsg = returnMessage });

        }

        public ActionResult GetIDNPLData()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["uid"]);
            return GetBootstrapData("SMSMI", " U_ID=" + SQLUtils.QuotedStr(uid));
        }


        public string DownloadCSMCCInformation(ref bool result, ref string msg)
        {
            string strName = "TransloadExcel.xlsx";
            string[] shipments = Request.Params["shipments"].Split(',');
            string sql = string.Format(@"SELECT '' as First,L.MASTER_NO,L.INV_NO,'' AS PACKING_LIST,L.CNTR_NO,'' AS CNTR_SEAL,'' AS LICENSE_REQUEST,
'' AS LICENSE_APPROVAL_DATE,
CASE WHEN L.ATA IS NULL THEN NULL ELSE CONVERT(VARCHAR(10), L.ATA, 120) END AS ATA,
I.TC_DEC_NO,
CASE WHEN I.TC_DEC_DATE IS NULL THEN NULL ELSE CONVERT(VARCHAR(10), I.TC_DEC_DATE, 120) END AS TC_DEC_DATE,
I.TS_TRAILER,I.TS_TRUCK,CASE WHEN PLA_NO IS NULL THEN 'Case No:'+CASE_NO ELSE 'Pallet No:'+PLA_NO END,
L.QTY,I.TS_SEAL_NO,I.CI_OUT_NO,
CASE WHEN I.CI_OUT_DATE IS NULL THEN NULL ELSE CONVERT(VARCHAR(10), I.CI_OUT_DATE, 120) END AS CI_OUT_DATE,
I.CI_IN_DATE,I.DEC_INFO,I.DEC_DATE_INFO,I.REL_DATE ,
CASE WHEN I.TRAN_TYPE='F' OR I.TRAN_TYPE='R' THEN 
(SELECT TOP 1 CC_RATE FROM SMICNTR WHERE SHIPMENT_ID=I.SHIPMENT_ID)
ELSE
(SELECT TOP 1 CC_RATE FROM SMIDN WHERE SHIPMENT_ID=I.SHIPMENT_ID)
END CC_RATE,
CASE WHEN I.TRAN_TYPE='F' OR I.TRAN_TYPE='R' THEN 
(SELECT TOP 1 FOB_AMT_USD FROM SMICNTR WHERE SHIPMENT_ID=I.SHIPMENT_ID)
ELSE
(SELECT TOP 1 FOB_AMT_USD FROM SMIDN WHERE SHIPMENT_ID=I.SHIPMENT_ID)
END FOB_AMT,I.POD_UPDATE_DATE
FROM SMIDNPL L LEFT JOIN SMSMI I ON L.U_FID=I.U_ID WHERE I.SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipments));
            DataTable icntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string strPath = Server.MapPath("~/download");
            string strFile = string.Format(@"{0}\{1}", strPath, strName);
            Business.NPOIExcelHelp exhelp = new Business.NPOIExcelHelp();
            if (!exhelp.Connect_NOPI(strFile))
            {
                msg = @Resources.Locale.L_DNManage_Controllers_292;
            }
            else
            {
                NPOI.SS.UserModel.IWorkbook book = exhelp.DataTableToExcel1(icntrDt, false, 10, true, 0, 0);
                string dirpath = GetDirPath(Server.MapPath("~/download/") + "backup\\");
                string finalpathdate = Business.TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss");
                string excelfile = string.Format("{0}{1}", dirpath + finalpathdate, strName);
                string filepath = dirpath.Replace(Server.MapPath("~/download/"), "");
                strName = filepath + finalpathdate + strName;
                using (FileStream file = new FileStream(excelfile, FileMode.Create))
                {
                    book.Write(file);
                    file.Close();
                    result = true;
                }
            }
            return strName;
        }


    }
}
