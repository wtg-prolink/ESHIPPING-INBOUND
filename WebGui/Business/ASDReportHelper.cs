using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using TrackingEDI.Business;

namespace Business
{
    public class ASDReportHelper
    {
        string templatefile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download/ASDReport.xls");
        string ASDType = string.Empty;

        public ASDReportHelper(string asdtype){
            ASDType = asdtype;
        }

        public string GetTemplateData(string conditions, UserInfo userinfo)
        {
            string xlsFile = string.Empty;
            try
            {
                File.SetAttributes(templatefile, FileAttributes.Normal);
                FileStream templetefs = new FileStream(templatefile, FileMode.Open);
                XSSFWorkbook hssfworkbook = new XSSFWorkbook(templetefs);
                XSSFSheet sheet1 = (XSSFSheet)hssfworkbook.GetSheetAt(0); 
                ASDReportLogic reportLogic = new ASDReportLogic(conditions, userinfo.GroupId, userinfo.CompanyId, ASDType);

                //DataTable expressDt = reportLogic.GetCostFeeData(); //查询非运费-FI费用
                //DataTable tkbilmMainDt = reportLogic.GetCostFeeByFIData(); //查询运费-FI费用
                reportLogic.createDnRowExcel(sheet1);
                string ext = ".xls"; //生成文件的扩展名
                xlsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("../UploadFiles/CostReport/{0}{1}", System.Guid.NewGuid().ToString(), ext));
                string directoryPath = Path.GetDirectoryName(xlsFile);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(xlsFile, System.IO.FileMode.Create))
                {
                    hssfworkbook.Write(fs);
                }
            }
            catch(Exception ex){
                throw ex;
            }
            return xlsFile;
        }
    }
}