using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using TrackingEDI.Business;

namespace Business
{

    public class SCMInfoToExcel : DownExcelInterface
    {
        protected override DownExcelConfig CreateConfig()
        {
            return new DownExcelConfig
            {
                WebPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath,
                mapping = "InboundSCMInfoMapping",
                //templatefile= Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "download/DNInfomation.xlsx")
                templatefile = Path.Combine(System.Web.HttpContext.Current.Request.PhysicalApplicationPath, "download/InboundSCMInfo.xlsx"),
                isExcel = true
            };
        }
    }

    public class VizioDataExcel : DownExcelInterface
    {
        protected override DownExcelConfig CreateConfig()
        {
            return new DownExcelConfig
            {
                WebPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath,
                mapping = "VizioDataInfoMapping",
                templatefile = Path.Combine(System.Web.HttpContext.Current.Request.PhysicalApplicationPath, "download/VizioDataInfo.xlsx"),
                isExcel = true
            };
        }
    }
}