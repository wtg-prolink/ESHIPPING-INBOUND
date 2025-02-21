using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using Prolink.WebReport.Business;

namespace WebGui.Controllers
{
    public class ReportController : Controller
    {
        public ActionResult CreateNewReport()
        {
            NameValueCollection nameValues = new NameValueCollection();
            int paraCount = Request.Params.Count;
            for (int i = 0; i < paraCount; i++)
                nameValues.Add(Request.Params.Keys[i], Request.Params[i]);
            //Add some global variable here
            //nameValues.Add("NOW", Prolink.Web.Context.GetServerDateTime().ToString("yyyyMMddHHmmss"));
            nameValues.Add("UserId", "PLFOC");
            nameValues.Set("conditions", Request["conditions"]);
            nameValues.Set("basecondition", GetDecodeBase64ToString(Request["basecondition"]));

            Prolink.WebReport.Business.ReportUtils utils = new Prolink.WebReport.Business.ReportUtils(ReportName, ReportView, nameValues, FormatType, ExportFile);
            string taskId = utils.CreateTask();

            //string rptUrl = Prolink.Web.WebContext.GetInstance().GetProperty("exec-newrpt-url");
            //if (rptUrl == null || rptUrl == "") rptUrl = "http://localhost/ReportService";
            //this.Response.Write("/ExecuteReport.aspx?id=" + System.Web.HttpUtility.UrlEncode(taskId) + "&exportType=" + ExportType);
            //this.Response.End();

            //return Content("Report/ExecuteReport?id=" + System.Web.HttpUtility.UrlEncode(taskId) + "&exportType=" + ExportType);
            return Content(Url.Content("~/ExecuteReport.aspx") + "?id=" + System.Web.HttpUtility.UrlEncode(taskId) + "&exportType=" + ExportType);
        }

        public ActionResult CreateNewReport2Edoc()
        {
            NameValueCollection nameValues = new NameValueCollection();
            int paraCount = Request.Params.Count;
            for (int i = 0; i < paraCount; i++)
                nameValues.Add(Request.Params.Keys[i], Request.Params[i]);
            //Add some global variable here
            //nameValues.Add("NOW", Prolink.Web.Context.GetServerDateTime().ToString("yyyyMMddHHmmss"));
            nameValues.Add("UserId", "PLFOC");
            nameValues.Set("conditions", Request["conditions"]);
            nameValues.Set("basecondition", GetDecodeBase64ToString(Request["basecondition"]));

            Prolink.WebReport.Business.ReportUtils utils = new Prolink.WebReport.Business.ReportUtils(ReportName, ReportView, nameValues, FormatType, ExportFile);
            string taskId = utils.CreateTask();

            //string rptUrl = Prolink.Web.WebContext.GetInstance().GetProperty("exec-newrpt-url");
            //if (rptUrl == null || rptUrl == "") rptUrl = "http://localhost/ReportService";
            //this.Response.Write("/ExecuteReport.aspx?id=" + System.Web.HttpUtility.UrlEncode(taskId) + "&exportType=" + ExportType);
            //this.Response.End();

            //return Content("Report/ExecuteReport?id=" + System.Web.HttpUtility.UrlEncode(taskId) + "&exportType=" + ExportType);
            return Json(new { URL = Url.Content("~/ExecuteReport.aspx") + "?id=" + System.Web.HttpUtility.UrlEncode(taskId) + "&exportType=" + ExportType }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExecuteReport()
        {
            return View();
        }

        /// <summary>
        /// 报表名称
        /// </summary>
        private string ReportName
        {
            get
            {
                return Request["rptName"];
            }
        }

        /// <summary>
        /// 报表View
        /// </summary>
        private string ReportView
        {
            get
            {
                return Request["rptView"];
            }
        }

        /// <summary>
        /// 导出文件类型（xls,pdf,word等）
        /// </summary>
        private string FormatType
        {
            get
            {
                return Request["formatType"];
            }
        }

        /// <summary>
        /// 导出的文件名
        /// </summary>
        private string ExportFile
        {
            get
            {
                return Request["exportFile"];
            }
        }

        /// <summary>
        /// 导出方式，(预览=PREVIEW，写到硬盘=IO，下载=DOWNLOAD)
        /// </summary>
        private string ExportType
        {
            get
            {
                //return "PREVIEW";
                return Request["exportType"];
            }
        }

        public string GetDecodeBase64ToString(string baseCondition)
        {
            if (!string.IsNullOrEmpty(baseCondition))
            {
                baseCondition = HttpUtility.UrlDecode(baseCondition);
                baseCondition = Prolink.Utils.SerializeUtils.DecodeBase64ToString(baseCondition);
            }
            return baseCondition;
        }
    }
}
