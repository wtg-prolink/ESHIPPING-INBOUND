using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace TrackingEDI.Business
{
    public class SCMModelInfoToExcel : DownExcelInterface
    {
        protected override DownExcelConfig CreateConfig()
        {
            return new DownExcelConfig
            {
                WebPath = WebConfigurationManager.AppSettings["WEB_PATH"],
                mapping = "IbSCMModelInfoMapping",
                templatefile= Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "download/IbSCMModelInfoMapping.xlsx"),
                //templatefile = Path.Combine(System.Web.HttpContext.Current.Request.PhysicalApplicationPath, "download/IbSCMModelInfoMapping.xlsx"),
                isExcel = false
            };
        }
    }

}
