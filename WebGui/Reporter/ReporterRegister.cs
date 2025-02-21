using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prolink.Data;

namespace WebGui.Reporter
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
            //"WebGui.Reporter.DefaultDataGetter,WebGui", "");
            Prolink.WebReport.Model.ReportList.RegisterReport(Prolink.WebReport.Business.ReportConfiguration.DEFAULT_GATTER,
            "WebGui.Reporter.DefaultDataGetter,WebGui", "");

            //Packing List
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ01", "WebGui.Reporter.IPQ01DataGetter,WebGui", "");
            //INVOICE
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ02", "WebGui.Reporter.IPQ02DataGetter,WebGui", "");
            //INVOICE 2
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ04", "WebGui.Reporter.IPQ04DataGetter,WebGui", "");
            //Packing List 2
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ03", "WebGui.Reporter.IPQ03DataGetter,WebGui", "");
            //Packing List 3
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ05", "WebGui.Reporter.IPQ05DataGetter,WebGui", "");

            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ05C", "WebGui.Reporter.IPQ05CDataGetter,WebGui", "");
            //INVOICE 3
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ06", "WebGui.Reporter.IPQ06DataGetter,WebGui", "");
            //合约
            Prolink.WebReport.Model.ReportList.RegisterReport("IPQ06C", "WebGui.Reporter.IPQ06CDataGetter,WebGui", "");
            //Booking From
            Prolink.WebReport.Model.ReportList.RegisterReport("FCL01", "WebGui.Reporter.FCL01DataGetter,WebGui", "");
            //Draft B/L
            Prolink.WebReport.Model.ReportList.RegisterReport("FCL02", "WebGui.Reporter.FCL02DataGetter,WebGui", "");
            //Booking From 中文
            Prolink.WebReport.Model.ReportList.RegisterReport("FCL03", "WebGui.Reporter.FCL03DataGetter,WebGui", "");
        }
    }
}