using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using TrackingEDI.Business;

namespace Business
{
    public class ChargeToExcel
    {
        string templatefile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "download/ChargeInfomation.xlsx");
        public string ResetXls(DataTable dt)
        {
            File.SetAttributes(templatefile, FileAttributes.Normal);
            FileStream templetefs = new FileStream(templatefile, FileMode.Open);
            XSSFWorkbook workbook = new XSSFWorkbook(templetefs);

            ISheet sheet1 = workbook.GetSheetAt(0);//获得第一个工作表  
            Dictionary<string, object> mapping = XmlParser.GetMapping("ChargeInfoMapping", false);
            CreateRowExcel(sheet1, dt, mapping);
            sheet1.ForceFormulaRecalculation = true;
            string xlsFile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], string.Format("UploadFiles/ChargeInfo/{0}{1}", System.Guid.NewGuid().ToString(), ".xlsx"));
            string directoryPath = Path.GetDirectoryName(xlsFile);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            using (System.IO.FileStream fs = new System.IO.FileStream(xlsFile, System.IO.FileMode.Create))
            {
                workbook.Write(fs);
            }
            workbook.Close();
            workbook = null;
            GC.Collect();
            return xlsFile;
        }
        public void CreateRowExcel(ISheet sheet1, DataTable dt, Dictionary<string, Object> mapping)
        {

            List<string> keys = XmlParser.GetKeys(mapping);
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            string cellcode = string.Empty; int cellnum = 0;
            Dictionary<string, Object> fields = XmlParser.GetFields(mapping);
            int fieldCount = fields.Count;
            Dictionary<string, object> field = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                XSSFRow row = (XSSFRow)sheet1.GetRow(i + 1);
                if (row == null)
                {
                    row = (XSSFRow)sheet1.CreateRow(i + 1);
                }
                for (int j = 0; j < fieldCount; j++)
                {
                    fieldValue = string.Empty;
                    List<string> test = new List<string>(fields.Keys);
                    test[1].ToString();
                    field = fields[test[j]] as Dictionary<string, object>;

                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                    dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                    if (cellnum < 0) continue;
                    XSSFCell XSSFcell = (XSSFCell)row.GetCell(j);
                    if (XSSFcell == null)
                        XSSFcell = (XSSFCell)row.CreateCell(j);
                    try
                    {
                        fieldValue = GetBsCodeDescpByFieldName(fieldname, fieldValue, dt.Rows[i]);
                    }
                    catch (Exception ex)
                    {
                        fieldValue = string.Empty;
                    }
                    formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引
                }
            }
        }

        public string GetBsCodeDescpByFieldName(string fieldname, string code, DataRow dr)
        {
            string fieldValue = string.Empty;

            try
            {
                string newfieldname = fieldname;
                if (newfieldname.StartsWith("HTML_"))
                    newfieldname = newfieldname.Replace("HTML_", "");
                code = Prolink.Math.GetValueAsString(dr[newfieldname]);
            }
            catch (Exception ex)
            {
                code = string.Empty;
            }

            switch (fieldname)
            {
                case "":
                    break;

            }
            if (string.IsNullOrEmpty(fieldValue)) return code;
            return fieldValue;
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

    }
}