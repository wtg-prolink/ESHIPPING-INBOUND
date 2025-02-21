using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prolink.Data;

namespace ReportGetter
{
    public class ReporterRegister
    {
        public static DelegateConnection DelegateConnection
        {
            get { return Prolink.Web.WebContext.GetInstance().GetConnection(); }
        }

        public static void Register()
        {
            //Prolink.WebReport.Model.ReportList.RegisterReport("SEQUO",
            //"ReportGetter.DefaultDataGetter,ReportGetter", "");
            Prolink.WebReport.Model.ReportList.RegisterReport(Prolink.WebReport.Business.ReportConfiguration.DEFAULT_GATTER,
            "ReportGetter.DefaultDataGetter,ReportGetter", "");

            //Packing List
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ01", "ReportGetter.IPQ01DataGetter,ReportGetter", "");
            //INVOICE
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ02", "ReportGetter.IPQ02DataGetter,ReportGetter", "");
            //INVOICE 2
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ04", "ReportGetter.IPQ04DataGetter,ReportGetter", "");
            //Packing List 2
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ03", "ReportGetter.IPQ03DataGetter,ReportGetter", "");
            //Packing List 3
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ05", "ReportGetter.IPQ05DataGetter,ReportGetter", "");

            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ05C", "ReportGetter.IPQ05CDataGetter,ReportGetter", "");
            //INVOICE 3
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ06", "ReportGetter.IPQ06DataGetter,ReportGetter", "");
            //合约
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ06C", "ReportGetter.IPQ06CDataGetter,ReportGetter", "");
            //Booking From
            Prolink.WebReport.Model.ReportList.RegisterReport("FCL01", "ReportGetter.FCL01DataGetter,ReportGetter", "");
            //Draft B/L
            Prolink.WebReport.Model.ReportList.RegisterReport("FCL02", "ReportGetter.FCL02DataGetter,ReportGetter", "");
            //Booking From 中文
            Prolink.WebReport.Model.ReportList.RegisterReport("FCL03", "ReportGetter.FCL03DataGetter,ReportGetter", "");
        }
    }
}