using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Prolink.Data;
using Prolink.DataOperation;
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
    public class BatchDnToExcel
    {
        string templatefile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "download/DNInfomation.xlsx");
        public string ResetXls(UserInfo userinfo,DataTable dndt)
        {
            //MemoryStream ms = new MemoryStream(ExcelModelRes.testexcel);//读取一个资源文件中存放的测试Excel文件  
            File.SetAttributes(templatefile, FileAttributes.Normal);
            FileStream templetefs = new FileStream(templatefile, FileMode.Open);
            XSSFWorkbook workbook = new XSSFWorkbook(templetefs);
    
            ISheet sheet1 = workbook.GetSheetAt(0);//获得第一个工作表  
            //SetXlsForValiadition(workbook, sheet1, userinfo);
            //sheet1.ForceFormulaRecalculation = true;
            Dictionary<string, object> mapping = XmlParser.GetMapping("BatchDNInfoMapping", true);
            CreateDnRowExcel(sheet1, dndt, mapping);
            sheet1.ForceFormulaRecalculation = true;
            string xlsFile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], string.Format("UploadFiles/CostReport/{0}{1}", System.Guid.NewGuid().ToString(), ".xlsx"));
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
            //FileStream file = new FileStream(@"test.xlsx", FileMode.Create);//Dump到文件</span></span> 
            //workbook.Write(file);
            //file.Close();
        }

        public void CreateDnRowExcel(ISheet sheet1, DataTable smdndt, Dictionary<string, Object> mapping)
        {

            List<string> keys = XmlParser.GetKeys(mapping);
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            string cellcode = string.Empty; int cellnum = 0;
            Dictionary<string, Object> fields = XmlParser.GetFields(mapping);
            int fieldCount = fields.Count;
            Dictionary<string, object> field = null;
            for (int i = 0; i < smdndt.Rows.Count; i++)
            {
                XSSFRow row = (XSSFRow)sheet1.GetRow(i + 2);
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
                        if ("PRODUCT_DATE".Equals(fieldname) || "ETD".Equals(fieldname))
                        {
                            if (!string.IsNullOrEmpty(fieldValue) && fieldValue.Length > 8)
                                fieldValue = fieldValue.Substring(0, 8);
                            dataType = "string";
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
                case "datetime": //如果是日期
                    col.SetCellValue(Prolink.Math.GetValueAsDateTime(value).ToString("yyyy/MM/dd"));
                    break;
                case "decimal": //如果是数值
                    col.SetCellValue(double.Parse(value));
                    break;
            }
        }


        public string GetBsCodeDescpByFieldName(string fieldname, string code, DataRow dr, bool IsMaster = true)
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


        public void SetXlsForValiadition(XSSFWorkbook workbook,ISheet sheet1,UserInfo userinfo)
        {
            #region 并柜信息栏位验证处理
            ISheet sheetRef = workbook.GetSheet("ref");//名为ref的工作表  
            for (int i = 0; i <= 14; i++)//A1到A4格子里存放0001到0004，这是下拉框可以选择的4个选项  
            {
                sheetRef.CreateRow(i);
                var j = i + 1;
                sheetRef.GetRow(i).GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("CN" + j);
            }
            IName range = workbook.CreateName();//创建一个命名公式  
            range.RefersToFormula = "ref!$A$1:$A$15";//公式内容，就是上面的区域  
            range.NameName = "sectionName";//公式名称，可以在"公式"-->"名称管理器"中看到  
            
            CellRangeAddressList regions = new CellRangeAddressList(0, 65535, 5, 5);//增加第六页中的数据下拉验证   
            XSSFDataValidationHelper helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            IDataValidation validation = helper.CreateValidation(helper.CreateFormulaListConstraint("sectionName"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去
            #endregion

            sheetRef = workbook.GetSheet("ref");//名为ref的工作表  
            sheetRef.GetRow(0).GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("Y");
            sheetRef.GetRow(1).GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("N");
            range = workbook.CreateName();//创建一个命名公式  
            range.RefersToFormula = "ref!$B$1:$B$2";//公式内容，就是上面的区域  
            range.NameName = "Isok";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 1, 2);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("Isok"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去

            regions = new CellRangeAddressList(0, 65535, 13, 13);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("Isok"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去

            regions = new CellRangeAddressList(0, 65535, 16, 17);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("Isok"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去

            regions = new CellRangeAddressList(0, 65535, 20, 20);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("Isok"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去

            regions = new CellRangeAddressList(0, 65535, 22, 23);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("Isok"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去


            string[] types = new string[] { "PK", "TDTK", "TDT", "VIA" };
            string sql = string.Format("SELECT * FROM BSCODE WHERE {0} AND CD_TYPE IN {1}", string.Format("GROUP_ID={0} AND (CMP='*' OR CMP={1}) ", SQLUtils.QuotedStr(userinfo.GroupId), SQLUtils.QuotedStr(userinfo.CompanyId)), SQLUtils.Quoted(types));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sheetRef = workbook.GetSheet("ref");//名为ref的工作表  
            DataRow[] drs = dt.Select("CD_TYPE='TDT'");
            for (int i = 0; i < drs.Length; i++)
            {
                sheetRef.GetRow(i).GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue(Prolink.Math.GetValueAsString(drs[i]["CD"]));
            }
            range = workbook.CreateName();//创建一个命名公式 
            int count = drs.Length;
            range.RefersToFormula = "ref!$C$1:$C$" + count; ;//公式内容，就是上面的区域  
            range.NameName = "TDT";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 11, 11);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("TDT"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去


            drs = dt.Select("CD_TYPE='TDTK'");
            for (int i = 0; i < drs.Length; i++)
            {
                sheetRef.GetRow(i).GetCell(3, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue(Prolink.Math.GetValueAsString(drs[i]["CD"]));
            }
            range = workbook.CreateName();//创建一个命名公式 
            count = drs.Length;
            range.RefersToFormula = "ref!$D$1:$D$" + count; ;//公式内容，就是上面的区域  
            range.NameName = "TrackWay";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 12, 12);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("TrackWay"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去

            drs = dt.Select("CD_TYPE='PK'");
            for (int i = 0; i < drs.Length; i++)
            {
                sheetRef.GetRow(i).GetCell(4, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue(Prolink.Math.GetValueAsString(drs[i]["CD"]));
            }
            range = workbook.CreateName();//创建一个命名公式 
            count = drs.Length;
            range.RefersToFormula = "ref!$E$1:$E$" + count; ;//公式内容，就是上面的区域  
            range.NameName = "PK";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 18, 19);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("PK"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去

            drs = dt.Select("CD_TYPE='VIA'");
            for (int i = 0; i < drs.Length; i++)
            {
                sheetRef.GetRow(i).GetCell(5, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue(Prolink.Math.GetValueAsString(drs[i]["CD"]));
            }
            range = workbook.CreateName();//创建一个命名公式 
            count = drs.Length;
            range.RefersToFormula = "ref!$F$1:$F$" + count; ;//公式内容，就是上面的区域  
            range.NameName = "VIA";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 26, 26);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("VIA"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去


            sheetRef = workbook.GetSheet("ref");//名为ref的工作表  
            sheetRef.GetRow(0).GetCell(6, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("MLB");
            sheetRef.GetRow(1).GetCell(6, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("AWR");
            range = workbook.CreateName();//创建一个命名公式  
            range.RefersToFormula = "ref!$G$1:$G$2";//公式内容，就是上面的区域  
            range.NameName = "BrgType";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 25, 25);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("BrgType"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去

            sheetRef = workbook.GetSheet("ref");//名为ref的工作表  
            sheetRef.GetRow(0).GetCell(7, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("N");
            sheetRef.GetRow(1).GetCell(7, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("Y");
            sheetRef.GetRow(2).GetCell(7, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue("D");
            range = workbook.CreateName();//创建一个命名公式  
            range.RefersToFormula = "ref!$H$1:$H$3";//公式内容，就是上面的区域  
            range.NameName = "Battery";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 24, 24);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("Battery"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去


            sheetRef = workbook.GetSheet("ref");//名为ref的工作表  
            sheetRef.GetRow(0).GetCell(8, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue(@Resources.Locale.L_ActManage_No);
            sheetRef.GetRow(1).GetCell(8, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue(@Resources.Locale.L_BatteryL);
            sheetRef.GetRow(2).GetCell(8, MissingCellPolicy.CREATE_NULL_AS_BLANK).SetCellValue(@Resources.Locale.L_BatteryD);
            range = workbook.CreateName();//创建一个命名公式  
            range.RefersToFormula = "ref!$H$1:$H$3";//公式内容，就是上面的区域  
            range.NameName = "Battery";//公式名称，可以在"公式"-->"名称管理器"中看到  

            regions = new CellRangeAddressList(0, 65535, 24, 24);//增加第六页中的数据下拉验证   
            helper = new XSSFDataValidationHelper((XSSFSheet)sheet1);//获得一个数据验证Helper  
            validation = helper.CreateValidation(helper.CreateFormulaListConstraint("Battery"), regions);//创建一个特定约束范围内的公式列表约束（即第一节里说的"自定义"方式）  
            validation.CreateErrorBox("错误", "请按右侧下拉箭头选择!");//不符合约束时的提示  
            validation.ShowErrorBox = true;//显示上面提示 = True  
            sheet1.AddValidationData(validation);//添加进去
        }

        public string GroupId { get; set; }
    }
}