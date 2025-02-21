using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using NPOI.XSSF.UserModel;

namespace Business
{
    public class XExcelHelper
    {
        #region 从excel中将数据导出到datatable
        /// <summary>读取excel
        /// 默认第一行为标头
        /// </summary>
        /// <param name="strFileName">excel文档路径</param>
        /// <returns></returns>
        public static DataTable ImportExcelToDataTable(string strFileName)
        {
            DataTable dt = new DataTable();
            XSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = hssfworkbook.GetSheetAt(0) as XSSFSheet;
            dt = ImportDataTable(sheet, 0, true);
            return dt;
        }
        /// <summary>
        /// 读取EXCEL
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <param name="HeaderRowIndex">读取的起始行</param>
        /// <returns></returns>
        public static DataTable ImportExcelToDataTable(string strFileName, int HeaderRowIndex)
        {
            DataTable dt = new DataTable();
            XSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = hssfworkbook.GetSheetAt(0) as XSSFSheet;
            dt = ImportDataTable(sheet, HeaderRowIndex, true);
            return dt;
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
            XSSFWorkbook workbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = workbook.GetSheet(SheetName) as XSSFSheet;
            DataTable table = new DataTable();
            table = ImportDataTable(sheet, HeaderRowIndex, true);
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
        public static DataTable ImportExcelToDataTable(string strFileName, int SheetIndex, int HeaderRowIndex)
        {
            XSSFWorkbook workbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = workbook.GetSheetAt(SheetIndex) as XSSFSheet;
            DataTable table = new DataTable();
            table = ImportDataTable(sheet, HeaderRowIndex, true);
            //ExcelFileStream.Close();
            workbook = null;
            sheet = null;
            return table;
        }


        public static DataTable ImportExcelToDataTable(int HeaderRowIndex, int HeaderRowEnd, string strFileName)
        {
            DataTable dt = new DataTable();
            XSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = hssfworkbook.GetSheetAt(0) as XSSFSheet;
            dt = ImportDataTable(sheet, HeaderRowIndex, HeaderRowEnd, false);
            return dt;
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
            XSSFWorkbook workbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = workbook.GetSheet(SheetName) as XSSFSheet;
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
            XSSFWorkbook workbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = workbook.GetSheetAt(SheetIndex) as XSSFSheet;
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
        static DataTable ImportDataTable(XSSFSheet sheet, int HeaderRowIndex, bool needHeader)
        {
            DataTable table = new DataTable();
            XSSFRow headerRow;
            int cellCount;
            try
            {
                if (HeaderRowIndex < 0 || !needHeader)
                {
                    headerRow = sheet.GetRow(0) as XSSFRow;
                    cellCount = headerRow.LastCellNum;

                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        DataColumn column = new DataColumn(Convert.ToString(i));
                        table.Columns.Add(column);
                    }
                }
                else
                {
                    headerRow = sheet.GetRow(HeaderRowIndex) as XSSFRow;
                    cellCount = headerRow.LastCellNum;

                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        if (headerRow.GetCell(i) == null)
                        {
                            if (table.Columns.IndexOf(Convert.ToString(i)) > 0)
                            {
                                DataColumn column = new DataColumn(Convert.ToString(@Resources.Locale.L_ExcelHelper_Business_79 + i));
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
                            DataColumn column = new DataColumn(Convert.ToString(@Resources.Locale.L_ExcelHelper_Business_79 + i));
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

                    XSSFRow row;
                    if (sheet.GetRow(i) == null)
                    {
                        row = sheet.CreateRow(i) as XSSFRow;
                    }
                    else
                    {
                        row = sheet.GetRow(i) as XSSFRow;
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

        static DataTable ImportDataTable(XSSFSheet sheet, int HeaderRowIndex, int HeaderRowEnd, bool needHeader)
        {
            DataTable table = new DataTable();
            XSSFRow headerRow;
            int cellCount;
            try
            {
                if (HeaderRowIndex < 0 || !needHeader)
                {
                    headerRow = sheet.GetRow(0) as XSSFRow;
                    cellCount = headerRow.LastCellNum;

                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        DataColumn column = new DataColumn(Convert.ToString(i));
                        table.Columns.Add(column);
                    }
                }
                else
                {
                    headerRow = sheet.GetRow(HeaderRowIndex) as XSSFRow;
                    cellCount = headerRow.LastCellNum;

                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        if (headerRow.GetCell(i) == null)
                        {
                            if (table.Columns.IndexOf(Convert.ToString(i)) > 0)
                            {
                                DataColumn column = new DataColumn(Convert.ToString(@Resources.Locale.L_ExcelHelper_Business_79 + i));
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
                            DataColumn column = new DataColumn(Convert.ToString(@Resources.Locale.L_ExcelHelper_Business_79 + i));
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
                for (int i = (HeaderRowIndex + 1); i <= sheet.LastRowNum && i <= HeaderRowEnd; i++)
                {

                    XSSFRow row;
                    if (sheet.GetRow(i) == null)
                    {
                        row = sheet.CreateRow(i) as XSSFRow;
                    }
                    else
                    {
                        row = sheet.GetRow(i) as XSSFRow;
                    }

                    DataRow dataRow = table.NewRow();

                    for (int j = row.FirstCellNum; j <= cellCount-1; j++)
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
        private static void SwitchCellType(XSSFRow row, DataRow dataRow, int j)
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
                        dataRow[j] = Convert.ToDouble(row.GetCell(j).NumericCellValue);
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
        public static object ImportExcelGetCellValue(int RowIndex, int colindex, string strFileName)
        {
            object cellvalue = null;
            XSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new XSSFWorkbook(file);
            }
            XSSFSheet sheet = hssfworkbook.GetSheetAt(0) as XSSFSheet;
            try
            {
                cellvalue = SwitchCellType(sheet.GetRow(RowIndex - 1) as XSSFRow, colindex);
            }
            catch (Exception e)
            {
                cellvalue = "";
            }
            return cellvalue;
        }
        private static object SwitchCellType(XSSFRow row, int j)
        {
            object val = null;
            switch (row.GetCell(j).CellType)
            {
                case CellType.String:
                    string str = row.GetCell(j).StringCellValue;
                    if (str != null && str.Length > 0)
                    {
                        val = str.ToString();
                    }
                    else
                    {
                        val = null;
                    }
                    break;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(row.GetCell(j)))
                    {
                        val = DateTime.FromOADate(row.GetCell(j).NumericCellValue);
                    }
                    else
                    {
                        val = Convert.ToDouble(row.GetCell(j).NumericCellValue);
                    }
                    break;
                case CellType.Boolean:
                    val = Convert.ToString(row.GetCell(j).BooleanCellValue);
                    break;
                case CellType.Error:
                    val = ErrorEval.GetText(row.GetCell(j).ErrorCellValue);
                    break;
                case CellType.Formula:
                    switch (row.GetCell(j).CachedFormulaResultType)
                    {
                        case CellType.String:
                            string strFORMULA = row.GetCell(j).StringCellValue;
                            if (strFORMULA != null && strFORMULA.Length > 0)
                            {
                                val = strFORMULA.ToString();
                            }
                            else
                            {
                                val = null;
                            }
                            break;
                        case CellType.Numeric:
                            val = Convert.ToString(row.GetCell(j).NumericCellValue);
                            break;
                        case CellType.Boolean:
                            val = Convert.ToString(row.GetCell(j).BooleanCellValue);
                            break;
                        case CellType.Error:
                            val = ErrorEval.GetText(row.GetCell(j).ErrorCellValue);
                            break;
                        default:
                            val = "";
                            break;
                    }
                    break;
                default:
                    val = "";
                    break;
            }
            return val;
        }
    }
}