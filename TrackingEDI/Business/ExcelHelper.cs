using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace TrackingEDI.Business
{
    public class ExcelHelper
    {
        #region 从excel中将数据导出到datatable
        /// <summary>读取excel
        /// 默认第一行为标头
        /// </summary>
        /// <param name="strFileName">excel文档路径</param>
        /// <returns></returns>
        public static DataTable ImportExcelToDataTable(string strFileName)
        {
            IWorkbook hssfworkbook = GetWorkbook(strFileName);
            ISheet sheet = hssfworkbook.GetSheetAt(0);
            DataTable dt = ImportDataTable(sheet, 0, true);
            return dt;
        }
        /// <summary>
        /// 读取EXCEL
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <param name="HeaderRowIndex">读取的起始行</param>
        /// <param name="SheetIndex">Sheet号</param>
        /// <returns></returns>
        public static DataTable ImportExcelToDataTable(string strFileName, int HeaderRowIndex, int SheetIndex = 0)
        {
            IWorkbook hssfworkbook = GetWorkbook(strFileName);
            ISheet sheet = hssfworkbook.GetSheetAt(SheetIndex);
            DataTable dt = ImportDataTable(sheet, HeaderRowIndex, true);
            DataTable newdt = dt.Clone();
            int columns=0;
            if(dt!=null)
                columns=dt.Columns.Count;
            bool isnotnull=false;
            foreach (DataRow sdr in dt.Rows)
            {
                isnotnull = false;
                for(int i=0;i<columns;i++){
                    if(!string.IsNullOrEmpty(sdr[i].ToString()))
                    {
                        isnotnull=true;
                        break;
                    }
                }
                if(isnotnull){
                    newdt.Rows.Add(sdr.ItemArray);
                }
            }
            if (newdt.Rows.Count > 0)
                return newdt;
            return dt;
        }

        private static IWorkbook GetWorkbook(string strFileName)
        {
            IWorkbook hssfworkbook = null;
            string filename = strFileName.ToLower();
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                if (filename.EndsWith(".xlsx"))
                    hssfworkbook = new XSSFWorkbook(file);
                else
                {
                    //if (filename.EndsWith(".xls"))
                    hssfworkbook = new HSSFWorkbook(file);
                }
            }
            return hssfworkbook;
        }
        /// <summary>
        /// 读取excel
        /// </summary>
        /// <param name="strFileName">excel文件路径</param>
        /// <param name="sheet">需要导出的sheet</param>
        /// <param name="HeaderRowIndex">列头所在行号，-1表示没有列头</param>
        /// <returns></returns>
        public static DataTable ImportExcelToDataTable(string strFileName, string SheetName, int HeaderRowIndex)
        {
            IWorkbook workbook = GetWorkbook(strFileName);
            ISheet sheet = workbook.GetSheetAt(0);
            DataTable table = ImportDataTable(sheet, HeaderRowIndex, true);
            //ExcelFileStream.Close();
            workbook = null;
            sheet = null;
            return table;
        }
         

        /// <summary>
        /// 读取excel
        /// </summary>
        /// <param name="strFileName">excel文件路径</param>
        /// <param name="sheet">需要导出的sheet</param>
        /// <param name="HeaderRowIndex">列头所在行号，-1表示没有列头</param>
        /// <returns></returns>
        public static DataTable ImportExcelToDataTable(string strFileName, string SheetName, int HeaderRowIndex, bool needHeader)
        {
            IWorkbook workbook = GetWorkbook(strFileName);
            ISheet sheet = workbook.GetSheetAt(0);
            DataTable table = new DataTable();
            table = ImportDataTable(sheet, HeaderRowIndex, needHeader);
            //ExcelFileStream.Close();
            workbook = null;
            sheet = null;
            return table;
        }

        /// <summary>
        /// 读取excel
        /// </summary>
        /// <param name="strFileName">excel文件路径</param>
        /// <param name="sheet">需要导出的sheet序号</param>
        /// <param name="HeaderRowIndex">列头所在行号，-1表示没有列头</param>
        /// <returns></returns>
        public static DataTable ImportExcelToDataTable(string strFileName, int SheetIndex, int HeaderRowIndex, bool needHeader)
        {
            IWorkbook workbook = GetWorkbook(strFileName);
            ISheet sheet = workbook.GetSheetAt(0);
            DataTable table = new DataTable();
            table = ImportDataTable(sheet, HeaderRowIndex, needHeader);
            //ExcelFileStream.Close();
            workbook = null;
            sheet = null;
            return table;
        }

        /// <summary>
        /// 将指定的sheet中的数据导出到datatable中
        /// </summary>
        /// <param name="sheet">需要导出的sheet</param>
        /// <param name="HeaderRowIndex">列头所在行号，-1表示没有列头</param>
        /// <returns></returns>
        static DataTable ImportDataTable(ISheet sheet, int HeaderRowIndex, bool needHeader)
        {
            DataTable table = new DataTable();
            IRow headerRow;
            int cellCount;
            try
            {
                if (HeaderRowIndex < 0 || !needHeader)
                {
                    headerRow = sheet.GetRow(0);
                    cellCount = headerRow.LastCellNum;

                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        DataColumn column = new DataColumn(Convert.ToString(i));
                        table.Columns.Add(column);
                    }
                }
                else
                {
                    headerRow = sheet.GetRow(HeaderRowIndex);
                    cellCount = headerRow.LastCellNum;

                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        if (headerRow.GetCell(i) == null)
                        {
                            if (table.Columns.IndexOf(Convert.ToString(i)) > 0)
                            {
                                DataColumn column = new DataColumn(Convert.ToString("重复列名" + i));
                                table.Columns.Add(column);
                            }
                            else
                            {
                                DataColumn column = new DataColumn(Convert.ToString(i));
                                table.Columns.Add(column);
                            }

                        }
                        else if (table.Columns.IndexOf(headerRow.GetCell(i).ToString()) > 0)
                        {
                            DataColumn column = new DataColumn(Convert.ToString("重复列名" + i));
                            table.Columns.Add(column);
                        }
                        else
                        {
                            DataColumn column = new DataColumn(headerRow.GetCell(i).ToString());
                            table.Columns.Add(column);
                        }
                    }
                }
                int rowCount = sheet.LastRowNum;
                for (int i = (HeaderRowIndex + 1); i <= sheet.LastRowNum; i++)
                {

                    IRow row;
                    if (sheet.GetRow(i) == null)
                    {
                        row = sheet.CreateRow(i);
                    }
                    else
                    {
                        row = sheet.GetRow(i);
                    }

                    DataRow dataRow = table.NewRow();

                    for (int j = row.FirstCellNum; j <= cellCount; j++)
                    {

                        if (row.GetCell(j) != null)
                        {
                            SwitchCellType(row, dataRow, j);
                        }
                    }
                    table.Rows.Add(dataRow);
                }

            }
            catch (Exception exceptionMsg)
            {
                //wLog.Error(exceptionMsg.Message + "\r\n" + exceptionMsg.StackTrace.ToString() + "\r\n");
            }
            return table;
        }

        private static void SwitchCellType(IRow row, DataRow dataRow, int j)
        {
            switch (row.GetCell(j).CellType)
            {
                case CellType.String:
                    string str = row.GetCell(j).StringCellValue;
                    if (str != null && str.Length > 0)
                    {
                        dataRow[j] = str.ToString();
                    }
                    else
                    {
                        dataRow[j] = null;
                    }
                    break;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(row.GetCell(j)))
                    {
                        dataRow[j] = DateTime.FromOADate(row.GetCell(j).NumericCellValue);
                    }
                    else
                    {
                        string val = row.GetCell(j).ToString();
                        dataRow[j] = Prolink.Math.GetValueAsDecimal(val);
                        //dataRow[j] = Convert.ToDouble(row.GetCell(j).NumericCellValue);
                    }
                    break;
                case CellType.Boolean:
                    dataRow[j] = Convert.ToString(row.GetCell(j).BooleanCellValue);
                    break;
                case CellType.Error:
                    dataRow[j] = ErrorEval.GetText(row.GetCell(j).ErrorCellValue);
                    break;
                case CellType.Formula:
                    switch (row.GetCell(j).CachedFormulaResultType)
                    {
                        case CellType.String:
                            string strFORMULA = row.GetCell(j).StringCellValue;
                            if (strFORMULA != null && strFORMULA.Length > 0)
                            {
                                dataRow[j] = strFORMULA.ToString();
                            }
                            else
                            {
                                dataRow[j] = null;
                            }
                            break;
                        case CellType.Numeric:
                            dataRow[j] = Convert.ToString(row.GetCell(j).NumericCellValue);
                            break;
                        case CellType.Boolean:
                            dataRow[j] = Convert.ToString(row.GetCell(j).BooleanCellValue);
                            break;
                        case CellType.Error:
                            dataRow[j] = ErrorEval.GetText(row.GetCell(j).ErrorCellValue);
                            break;
                        default:
                            dataRow[j] = "";
                            break;
                    }
                    break;
                default:
                    dataRow[j] = "";
                    break;
            }
        }
        #endregion 
    }
}