using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace TrackingEDI.Business
{
    public abstract class DownExcelInterface
    {
        //string templatefile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "download/DNInfomation.xlsx");

        public string ResetXls(BaseUserInfo userinfo, DataTable dndt,string filename="")
        {
            DownExcelConfig dconfig = CreateConfig();
            string templatefile = dconfig.templatefile;
            File.SetAttributes(templatefile, FileAttributes.Normal);
            FileStream templetefs = new FileStream(templatefile, FileMode.Open);
            XSSFWorkbook workbook = new XSSFWorkbook(templetefs);

            ISheet sheet1 = workbook.GetSheetAt(0);//获得第一个工作表  
            Dictionary<string, object> mapping = XmlParser.GetMapping(dconfig.mapping, dconfig.isExcel);
            CreateDnRowExcel(sheet1, dndt, mapping);
            sheet1.ForceFormulaRecalculation = true;

            if (string.IsNullOrEmpty(filename))
                filename = System.Guid.NewGuid().ToString();

            string xlsFile = Path.Combine(dconfig.WebPath, string.Format("FileUploads/{0}{1}", filename, ".xlsx"));
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

        protected abstract DownExcelConfig CreateConfig();

        public virtual void CreateDnRowExcel(ISheet sheet1, DataTable smdndt, Dictionary<string, Object> mapping)
        {

            List<string> keys = XmlParser.GetKeys(mapping);
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            string cellcode = string.Empty; int cellnum = 0;
            Dictionary<string, Object> fields = XmlParser.GetFields(mapping);
            int fieldCount = fields.Count;
            Dictionary<string, object> field = null;
            for (int i = 0; i < smdndt.Rows.Count; i++)
            {
                XSSFRow row = (XSSFRow)sheet1.GetRow(i + 1);
                if (row == null)
                    row = (XSSFRow)sheet1.CreateRow(i + 1);
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
                    else
                    {
                    }
                    if ("HTML_CAR_TYPE".Equals(fieldname) || "HTML_TRACK_WAY".Equals(fieldname) || "HTML_BATTERY".Equals(fieldname) || "HTML_VIA".Equals(fieldname))
                        continue;
                    try
                    {
                        fieldValue = GetBsCodeDescpByFieldName(fieldname, fieldValue, smdndt.Rows[i]);
                        if ("PRODUCT_DATE".Equals(fieldname) || "SCMREQUEST_DATE".Equals(fieldname))
                        {
                            if (!string.IsNullOrEmpty(fieldValue) && fieldValue.Length > 8)
                                fieldValue = fieldValue.Substring(0, 8);
                            dataType = "string";
                        }
                        if ("HTML_TRAN_TYPE".Equals(fieldname))
                        {
                            switch (fieldValue)
                            {
                                case "F":
                                    fieldValue = "FCL";
                                    break;
                                case "R":
                                    fieldValue = "RailWay";
                                    break;
                                case "L":
                                    fieldValue = "LCL";
                                    break;
                                case "A":
                                    fieldValue = "AIR";
                                    break;
                                case "T":
                                    fieldValue = "Truck";
                                    break;
                                case "E":
                                    fieldValue = "Express";
                                    break;
                            }
                        }
                        if ("HTML_STATUS".Equals(fieldname))
                        {
                            switch (fieldValue)
                            {
                                case "S":
                                    fieldValue = "ISF Sending";
                                    break;
                                case "A":
                                    fieldValue = "Unreach";
                                    break;
                                case "B":
                                    fieldValue = "Notify LSP";
                                    break;
                                case "C":
                                    fieldValue = "Notify Broker";
                                    break;
                                case "D":
                                    fieldValue = "Broker Confirm";
                                    break;
                                case "E":
                                    fieldValue = "E-Alert";
                                    break;
                                case "F":
                                    fieldValue = "Release";
                                    break;
                                case "G":
                                    fieldValue = "Gate In";
                                    break;
                                case "H":
                                    fieldValue = "Notify Transit Broker";
                                    break;
                                case "I":
                                    fieldValue = "Transit Confirm";
                                    break;
                                case "P":
                                    fieldValue = "POD";
                                    break;
                                case "X":
                                    fieldValue = "Cancel";
                                    break;
                                case "O":
                                    fieldValue = "Gate Out";
                                    break;
                                case "Z":
                                    fieldValue = "Finish";
                                    break;
                                case "V":
                                    fieldValue = "Void";
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        fieldValue = string.Empty;
                    }
                    formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引
                }
            }
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
                case "date":
                case "datetime": //如果是日期
                    col.SetCellValue(Prolink.Math.GetValueAsDateTime(value).ToString("yyyy/MM/dd"));
                    break;
                case "decimal": //如果是数值
                    col.SetCellValue(double.Parse(value));
                    break;
                case "time":
                    string myValue = "";
                    int time = Prolink.Math.GetValueAsInt(value);
                    if (time >= 10)
                        myValue = time + ":00";
                    else
                        myValue = "0" + time + ":00";
                    col.SetCellValue(myValue);
                    break;
            }
        }


        public virtual string GetBsCodeDescpByFieldName(string fieldname, string code, DataRow dr, bool IsMaster = true)
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
    }

    public class DownExcelConfig
    {
        public string WebPath { get; set; }
        public string templatefile { get; set; }
        public string mapping { get; set; }
        public bool isExcel { get; set; }
    }

    public class BaseUserInfo
    {
        public string UserId { get; set; }
        public string CompanyId { get; set; }
        public string GroupId { get; set; }
    }
}
