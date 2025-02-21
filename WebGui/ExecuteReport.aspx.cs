using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Prolink.WebReport.Business;

namespace Prolink.OEC.WebGui
{
    public partial class ExecuteReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ReportManagerFactory.CreateReport(TaskId, ExportType, this.form1);
        }

        protected string ExportType
        {
            get
            {
                return Request.Params["exportType"];
            }
        }

        private string TaskId
        {
            get
            {
                return Request.Params["id"];
            }
        }
    }
}