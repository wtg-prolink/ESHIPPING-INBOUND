using Business.Service;
using Business.TPV.Export;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;
using TrackingEDI.Mail;

namespace Business.TPV
{
    public class WebApiEdiHandle
    {
        public static ResultInfo SendInvoiceCheckEdi(string InvoiceNo, string userid, string cmp)
        {
            Business.TPV.Runtime runtime = new Business.TPV.Runtime { RefNo = InvoiceNo, OPUser = userid, Location = cmp, PartyNo = "WS-FSSC-INVOICE-01" };
            InvoiceCheckManager sntopts = new InvoiceCheckManager();
            ResultInfo resultinfo = sntopts.ExportEDI(runtime, "INV_C");
            return resultinfo;
        }

        public static ResultInfo SendVoidINVEdi(string refNo, string userid, string cmp, string stn)
        {
            Business.TPV.Runtime runtime = new Business.TPV.Runtime { RefNo = refNo, OPUser = userid, Data = stn, Location = cmp, PartyNo = "WS-IM-003" };
            ViodINVIMManager viodINVFSSP = new ViodINVIMManager();
            ResultInfo resultinfo = viodINVFSSP.ExportEDI(runtime, "INV_V");
            return resultinfo;
        }

        public static ResultInfo SendRejectINVEdi(string refNo, string userid, string cmp, string stn)
        {
            Business.TPV.Runtime runtime = new Business.TPV.Runtime { RefNo = refNo, OPUser = userid, Data = stn, Location = cmp, PartyNo = "BPM-FSSP-002" };
            ViodINVFSSPManager viodINVFSSP = new ViodINVFSSPManager();
            ResultInfo resultinfo = viodINVFSSP.ExportEDI(runtime, "INV_R");
            return resultinfo;
        }

        public static ResultInfo SendSplitSMBIDEdi(string tpvDebitNo, string userid, string cmp, string uid)
        {
            Business.TPV.Runtime runtime = new Business.TPV.Runtime { RefNo = tpvDebitNo, OPUser = userid, Data = uid, Location = cmp, PartyNo = "BPM-FSSP-001" };
            SplitBIDFSSPManage splitBIDFSSPManage = new SplitBIDFSSPManage();
            ResultInfo resultinfo = splitBIDFSSPManage.ExportEDI(runtime, "BID_D");
            if (!resultinfo.IsSucceed)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Add("FSSP_MSG", resultinfo.Description);
                map.Add("USER", "");
                DataTable maindt = Financial.Bill.GetBillDataTableByUId(uid);
                IMailTemplateParse parse = new DefaultMailParse();
                string mailTo = MailTemplate.GetMailToByUID(uid,map);
                string body = MailTemplate.GetMailBody("BPF", "TPV", cmp);
                string mailBody = parse.Parse(maindt, null, body, map);
                string mailSubject = MailTemplate.GetBillingFailMailSubject("BPF", "TPV", cmp);
                EvenFactory.AddEven(Guid.NewGuid().ToString(), uid, MailManager.RouteNotify, null, 1, 0, mailTo,
                mailSubject, mailBody);
            }
            return resultinfo;
        }
    }
}
