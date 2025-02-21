using Prolink.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using TrackingEDI.Business;
using WebGui.App_Start;
using ReportGetter;
using System.IO;
using NPOI.XSSF.UserModel;

namespace WebGui
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            DBConfig.Register(this.Server, this.Application);
            //ReporterRegister.DelegateConnection = Prolink.Web.WebContext.GetInstance().GetConnection();
            ReporterRegister.Register();
            PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
            Business.TPV.Context.Bulid();
            //SplitSmbidHelper splitSmbidHelper = new SplitSmbidHelper("F267AFC2-2289-478F-BF19-34C81E19F63C");
            //splitSmbidHelper.DoSplitSMBID();
            //ReadExcel();
            //TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.BILLAp, MailManager.SendBillApNotiyMail);
            //EvenFactory.ExecuteMailEven(MailManager.BILLAp);
        }

        public void ReadExcel()
        {
            string excelFileName = @"C:\Users\Will.Wan\Desktop\Copy of FeedBackInfo.xlsx";

            System.Data.DataTable dt = Business.XExcelHelper.ImportExcelToDataTable(excelFileName);

            try
            {
                File.SetAttributes(excelFileName, System.IO.FileAttributes.Normal);
                FileStream templetefs = new FileStream(excelFileName, FileMode.Open);
                XSSFWorkbook hssfworkbook = new XSSFWorkbook(templetefs);
                XSSFSheet sheet1 = (XSSFSheet)hssfworkbook.GetSheetAt(0);
                for(int i=0; i<= sheet1.LastRowNum;i++){
                    XSSFRow row = (XSSFRow)sheet1.GetRow(i);

                    XSSFCell cell1 = (XSSFCell)row.GetCell(0);
                    string cntrno = cell1.StringCellValue;
                    string sql = string.Format("SELECT U_ID,SHIPMENT_ID FROM SMRV WHERE RV_TYPE='I' AND CNTR_NO='{0}' AND STATUS NOT IN ('O','V')", cntrno);
                    System.Data.DataTable dtdt = Prolink.DataOperation.OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dtdt.Rows.Count <= 0) continue;
                    string uid=dtdt.Rows[0]["U_ID"].ToString();
                    if (string.IsNullOrEmpty(uid)) continue;
                    string updatesql = "UPDATE SMRV SET STATUS='O";
                    updatesql += GetValue(row, 1, "ARRIVAL_DATE");
                    updatesql+=GetValue(row, 3, "PALLET_QTY");
                    updatesql += GetValue(row, 4, "USE_DATE");
                    updatesql += GetValue(row, 5, "RESERVE_DATE");
                    updatesql += GetValue(row, 6, "DRIVER");
                    updatesql += GetValue(row, 6, "LDRIVER");
                    updatesql += GetValue(row, 7, "DRIVER_ID");
                    updatesql += GetValue(row, 7, "LDRIVER_ID");

                    updatesql += GetValue(row, 11, "AT_YARD_TIME");
                    updatesql += GetValue(row, 12, "IN_DATE_L");
                    updatesql += GetValue(row, 13, "POD_UPDATE_DATE");
                    updatesql += GetValue(row, 14, "OUT_DATE_L");
                    XSSFCell sqlcell1 = (XSSFCell)row.CreateCell(22);

                    updatesql += " WHERE U_ID ='" + uid + "'";
                    formatCellValue(sqlcell1, "string", updatesql); //这边siDt.Rows[i][j]用的是索引

                    XSSFCell sqlcell2 = (XSSFCell)row.CreateCell(25);
                    string shipmentid = dtdt.Rows[0]["SHIPMENT_ID"].ToString();
                    string update2 = "UPDATE SMSMI SET STATUS='O' WHERE SHIPMENT_ID='" + shipmentid + "'";
                    formatCellValue(sqlcell2, "string", update2); //这边siDt.Rows[i][j]用的是索引
                }
                string excelFileName1 = @"C:\Users\Will.Wan\Desktop\FeedBackInfo.xlsx";
                using (System.IO.FileStream fs = new System.IO.FileStream(excelFileName1, System.IO.FileMode.Create))
                {
                    hssfworkbook.Write(fs);
                }
                hssfworkbook.Close();
                hssfworkbook = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            foreach (System.Data.DataRow dr in dt.Rows)
            {

            }
        }

        public string GetValue( XSSFRow row,int cellnum,string filedname)
        {
            XSSFCell receivecell = (XSSFCell)row.GetCell(cellnum);
            if (receivecell == null) return "";
            string receivedate=string.Empty;
            try { receivedate = receivecell.StringCellValue;}
            catch(Exception ex){
                try {
                receivedate=receivecell.DateCellValue.ToShortDateString();
                }
                catch (Exception ex1)
                {
                    return "";
                }
            }
            return " AND " + filedname + "='" + receivedate + "'";
        }

        public void formatCellValue(XSSFCell col, string dataType, string value)
        {
            if (string.IsNullOrEmpty(value)) //如果没有值的话，后面就不必再执行了
                return;
            switch (dataType.ToLower())
            {
                case "string": //如果值是字符串
                    col.SetCellValue(value);
                    break;
                case "datetime": //如果是日期
                    col.SetCellValue(Prolink.Math.GetValueAsDateTime(value).ToString("yyyy/MM/dd"));
                    break;
                case "decimal": //如果是数值
                    col.SetCellValue(double.Parse(value));
                    break;
            }
        }
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            
            HttpCookie MyLang = Request.Cookies["plv3.passport.lang"];
            if (MyLang != null)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture =
                    new System.Globalization.CultureInfo(MyLang.Value);
                System.Threading.Thread.CurrentThread.CurrentUICulture =
                    new System.Globalization.CultureInfo(MyLang.Value);
                //Response.Cookies["plv3.passport.lang"].Value = null;
            }
        }
    }
}