using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Data;
using NPOI.SS.Util;

namespace Business
{
    public class NPOIExcelHelp
    {
        private string fileName = null; //文件名
        private string fullpath = string.Empty;
        private XSSFWorkbook workbook = null;
        private FileStream fs = null;
        Dictionary<string, object> sheelname = null;
        private bool disposed;
        XSSFSheet newsheet = null;
        int sheetcount = 0;
        ISheet isheet = null;

        public class mergelist
        {
            public int rowstar { get; set; }
            public int rowend { get; set; }
            public string columnstar { get; set; }
            public string columnend { get; set; }
        }
        //Dictionary<string, int> MergedRegionParam = null;
        public class MergedRegionParam
        {
            public int Upper { get; set; }
            public int Left { get; set; }
            public int Lower { get; set; }
            public int Right { get; set; }
        }
        public bool Connect_NOPI(string strFileName)
        {
            try
            {
                using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
                {
                    fullpath = strFileName;
                    workbook = new XSSFWorkbook(file);
                    file.Close();  
                }
                XSSFSheet sheet = workbook.GetSheetAt(0) as XSSFSheet;
                newsheet = sheet;
                return true;
            }
            catch (Exception e)
            {
                //Writelog("conn连接失败\r\n", DefaultLogger.INFOR);
                //Writelog(e.Message + "\r\n", DefaultLogger.INFOR);
                return false;
            }
        }
        public bool Colse()
        {
            if (fullpath != "")
                try
                {
                    using (FileStream file = new FileStream(fullpath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        workbook.Write(file);
                        file.Close();
                    }
                }
                catch (Exception e)
                {
                    //Writelog("conn连接失败\r\n", DefaultLogger.INFOR);
                    //Writelog(e.Message + "\r\n", DefaultLogger.INFOR);
                    return false;
                }
            return true;
        }
        public bool Colse(XSSFWorkbook work)
        {
            if (fullpath != "")
                try
                {
                    using (FileStream file = new FileStream(fullpath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        work.Write(file);
                        file.Close();
                    }
                }
                catch (Exception e)
                {
                    //Writelog("conn连接失败\r\n", DefaultLogger.INFOR);
                    //Writelog(e.Message + "\r\n", DefaultLogger.INFOR);
                    return false;
                }
            return true;
        }
        public XSSFWorkbook CreateWork()
        {
                XSSFWorkbook wk = new XSSFWorkbook();
                workbook = wk;
                return wk;
        }
        public ISheet CreateSheet(XSSFWorkbook Work,string Sheetname="")
        {
            if (string.IsNullOrEmpty(Sheetname)) {
                if (sheetcount == 0)
                    sheetcount = sheetlist.Count;
                ++sheetcount;
                Sheetname = string.Format("Sheet{0}", sheetcount);
            }
            ISheet sheet = Work.CreateSheet(Sheetname);
            isheet = sheet;
            return sheet;
        }
        public ISheet CreateHead( List<List<object>> Header,ISheet sheet=null, int star = 0)
        {
            sheet = CheckISheet(sheet);
            foreach (var lists in Header)
            {
                IRow row = sheet.CreateRow(star);
                for (int j = 0; j < lists.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(Prolink.Math.GetValueAsString(lists[j]));
                }
                star++;
            }
            isheet = sheet;
            return sheet;
        }

        public ISheet SetCellStyle(ICellStyle cellstyle,int star, ISheet sheet=null)
        {
            sheet = CheckISheet(sheet);
            star=star==0?star:star-1;
            IRow row =sheet.GetRow(star);
            for (int i = 0; i < row.LastCellNum;i++ )
            {
                row.GetCell(i).CellStyle = cellstyle;
            }
            isheet = sheet;
            return sheet;
        }
        public ISheet BatchSetCellStyle(ICellStyle cellstyle, int star,int end, ISheet sheet=null)
        {
            for (; star <= end; star++)
            {
                SetCellStyle(cellstyle, star);
            }
            return sheet;
        }
        private Dictionary<string, object> sheetlist
        {
            get { return SheelName(workbook); }
        }
        public ISheet CheckISheet(ISheet sheet)
        {
            return sheet = sheet == null ? isheet : sheet;
        }
        public string[] Letter = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public int ColumnToNum(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            value = value.ToUpper();
            int ColumnNumber=0;
            for (int j = 0; j < value.Length; j++)
            {
                string cha = value.Substring(0, 1);
                int index = Letter.ToList().IndexOf(cha);
                if (j != 0)
                    index++;
                ColumnNumber += index;
            }
            return ColumnNumber;
        }
        public static string[] letter = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public static int columntonum(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            value = value.ToUpper();
            int ColumnNumber = 0;
            for (int j = 0; j < value.Length; j++)
            {
                string cha = value.Substring(0, 1);
                int index = letter.ToList().IndexOf(cha);
                if (j != 0)
                    index++;
                ColumnNumber += index;
            }
            return ColumnNumber;
        }
        /// <summary>
        /// 合并单元格
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="param">起始行，结束行，起始列，结束列</param>
        /// <returns></returns>
        public ISheet MergedRegion(int[] row, string[] column, ISheet sheet = null)
        {
            sheet = CheckISheet(sheet);
            if (row.Length == 2 && column.Length == 2)
            {
                int rowstar = row[0] == 0 ? 0 : row[0] - 1;
                int rowend = row[1] == 0 ? 0 : row[1] - 1;
                //CellRangeAddress四个参数为：起始行，结束行，起始列，结束列
                sheet.AddMergedRegion(new CellRangeAddress(rowstar, rowend, ColumnToNum(column[0]), ColumnToNum(column[1])));
            }
            isheet = sheet;
            return sheet;
        }
        public ISheet MergedRegion(int[] row, int[] column, ISheet sheet = null)
        {
            sheet = CheckISheet(sheet);
            if (row.Length == 2 && column.Length == 2)
            {
                int rowstar = row[0] == 0 ? 0 : row[0] - 1;
                int rowend = row[1] == 0 ? 0 : row[1] - 1;
                //CellRangeAddress四个参数为：起始行，结束行，起始列，结束列
                sheet.AddMergedRegion(new CellRangeAddress(rowstar, rowend, column[0], column[1]));
            }
            isheet = sheet;
            return sheet;
        }
        public ISheet BatchMergedRegion(List<mergelist> lists,ISheet sheet=null)
        {
            sheet = CheckISheet(sheet);
            foreach (var list in lists)
            {
                int rowstar = list.rowstar == 0 ? 0 : list.rowstar - 1;
                int rowend = list.rowend == 0 ? 0 : list.rowend - 1;
                int columnstar = ColumnToNum(list.columnstar);
                //columnstar = columnstar == 0 ? 0 : columnstar - 1;
                int columnend = ColumnToNum(list.columnend);
                //columnend = columnend == 0 ? 0 : list.rowend - 1;
                if (columnstar <= columnend && rowstar <= rowend)
                {
                    sheet.AddMergedRegion(new CellRangeAddress(rowstar, rowend, columnstar, columnend));
                }
            }
            isheet = sheet;
            return sheet;
        }
        private Dictionary<string, object> SheelName(XSSFWorkbook workbook)
        {
            Dictionary<string, object> sheel = new Dictionary<string, object>();
            int i = 0;
            bool factor = true;
            while (factor)
            {
                try
                {
                    string name = workbook.GetSheetName(i);
                    sheel.Add(i.ToString(), name);
                    i++;
                }
                catch (Exception e)
                {
                    factor = false;
                }
            }
            return sheel;
        }
        /// <summary>
        /// dt转excel
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isColumnWritten">是否写入列名</param>
        /// <param name="start">开始位置</param>
        /// <param name="needremove">是否需要移除</param>
        /// <param name="removebegin"></param>
        /// <param name="removeend"></param>
        /// <param name="sheetName"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IWorkbook DataTableToExcel(DataTable data, bool isColumnWritten, int start, bool needremove, int removebegin=0, int removeend=0, string sheetName = "", int end = 0)
        {
            //if (!Connect_NOPI(fileName))
            //{
            //    //return new ReplyMsg
            //    //{
            //    //    IsSucceed = false,
            //    //    ResultCode = "fail",
            //    //    Description = "文件连接失败"
            //    //};
            //}
            int i = 0;
            int j = 0;
            int count = 0;
            XSSFSheet sheet = null;
            if (start > 0)
                count = start-1;
            try
            {
                if (workbook != null)
                {
                    if (sheetName != "")
                    {
                        sheet = workbook.GetSheet(sheetName) as XSSFSheet;
                        newsheet = sheet;
                    }
                    else
                    {
                        sheet = workbook.GetSheetAt(0) as XSSFSheet;
                        newsheet = sheet;
                    }
                }
                if (isColumnWritten == true) //写入DataTable的列名
                {
                    var dd= sheet.GetRow(0).GetCell(0).CellStyle;
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                        row.GetCell(j).CellStyle = dd;
                    }
                    count = 1;
                }
                for (i = 0; i < data.Rows.Count; ++i)
                {
                    var dd = sheet.GetRow(1);
                    IRow row = sheet.CreateRow(count);
                    sheet.ShiftRows(start + i + 1, start + i + 2, 1, true, true);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                        row.GetCell(j).CellStyle = dd.GetCell(j).CellStyle;
                    }
                    
                    ++count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            if (needremove)
            {
                if (removebegin > 0 && removeend > 0 && removeend > removebegin)
                {
                    DeleteRow(sheet, 0, removebegin, removeend);
                    newsheet = sheet;
                }
            }
            return workbook;// ReplyMsg.SucceedResult();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isColumnWritten">是否写入db列名</param>
        /// <param name="start">开始位置</param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public IWorkbook DataTableToExcel(DataTable data, bool isColumnWritten, int start, string sheetName = "")
        {
            int i = 0;
            int j = 0;
            int count = 0;
            XSSFSheet sheet = null;
            if (start > 0)
                count = start - 1;
            try
            {
                if (workbook != null)
                {
                    if (sheetName != "")
                    {
                        sheet = CheckISheet(sheet) as XSSFSheet;
                        sheet = workbook.GetSheet(sheetName) as XSSFSheet;
                        newsheet = sheet;
                    }
                    else
                    {
                        sheet = workbook.GetSheetAt(0) as XSSFSheet;
                        newsheet = sheet;
                    }
                }
                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    count = 1;
                }
                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }

                    ++count;
                }
                isheet = (ISheet)sheet;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            return workbook;
        }

        public IWorkbook DataTableToExcel1(DataTable data, bool isColumnWritten, int start, bool needremove, int removebegin, int removeend, string sheetName = "", int end = 0, int head = 0)
        {
            //if (!Connect_NOPI(fileName))
            //{
            //    //return new ReplyMsg
            //    //{
            //    //    IsSucceed = false,
            //    //    ResultCode = "fail",
            //    //    Description = "文件连接失败"
            //    //};
            //}
            int i = 0;
            int j = 0;
            int count = 0;
            XSSFSheet sheet = null;
            if (start > 0)
                count = start - 1;
            try
            {
                if (workbook != null)
                {
                    sheet = workbook.GetSheetAt(0) as XSSFSheet;
                    newsheet = sheet;
                }
                if (isColumnWritten == true) //写入DataTable的列名
                {
                    var dd = sheet.GetRow(0).GetCell(0).CellStyle;
                    IRow row = sheet.CreateRow(head);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                        row.GetCell(j).CellStyle = dd;
                    }
                    count = 1;
                }
                for (i = 0; i < data.Rows.Count; ++i)
                {
                    var dd = sheet.GetRow(count-1);
                    insertRow(sheet, count, 1);
                    IRow row = sheet.CreateRow(count);
                    row.RowStyle = dd.RowStyle;
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    for (j = 0; j < dd.LastCellNum; j++)
                    {
                        if(j>=row.LastCellNum)
                            row.CreateCell(j).SetCellValue("");
                        row.GetCell(j).CellStyle = dd.GetCell(j).CellStyle;
                    }
                        ++count;
                }
                //workbook.Write(fs); //写入到excel
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            return workbook;// ReplyMsg.SucceedResult();
        }

        public IWorkbook DataTableToExcel2(DataTable data, bool isColumnWritten, int start, bool needremove, int removebegin, int removeend, string sheetName = "", int end = 0)
        {
            //if (!Connect_NOPI(fileName))
            //{
            //    //return new ReplyMsg
            //    //{
            //    //    IsSucceed = false,
            //    //    ResultCode = "fail",
            //    //    Description = "文件连接失败"
            //    //};
            //}
            int i = 0;
            int j = 0;
            int count = 0;
            XSSFSheet sheet = null;
            if (start > 0)
                count = start - 1;
            try
            {
                if (workbook != null)
                {
                    if (!string.IsNullOrEmpty(sheetName))
                    {
                        sheet = GetSheet(workbook, sheetName) as XSSFSheet;
                    }
                    else
                    {
                        sheet = workbook.GetSheetAt(0) as XSSFSheet;
                    }
                    newsheet = sheet;
                }
                if (isColumnWritten == true) //写入DataTable的列名
                {
                    var dd = sheet.GetRow(0).GetCell(0).CellStyle;
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                        row.GetCell(j).CellStyle = dd;
                    }
                    count = 1;
                }
                for (i = 0; i < data.Rows.Count; ++i)
                {
                    var dd = sheet.GetRow(count - 1);
                    //insertRow(sheet, count, 1);
                    IRow row = sheet.CreateRow(count);
                    row.RowStyle = dd.RowStyle;
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    for (j = 0; j < dd.LastCellNum; j++)
                    {
                        if (j >= row.LastCellNum)
                            row.CreateCell(j).SetCellValue("");
                        //row.GetCell(j).CellStyle = dd.GetCell(j).CellStyle;
                    }
                    ++count;
                }
                //workbook.Write(fs); //写入到excel
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            return workbook;// ReplyMsg.SucceedResult();
        }
        public IWorkbook DataTableToExcel(DataTable data, bool isColumnWritten, int start, int head)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            XSSFSheet sheet = null;
            if (start > 0)
                count = start - 1;
            try
            {
                if (workbook != null)
                {
                    sheet = workbook.GetSheetAt(0) as XSSFSheet;
                    newsheet = sheet;
                }
                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(head);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    if (start == 0) count = 1;
                }
                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    ++count;
                }
                //workbook.Write(fs); //写入到excel
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            return workbook;// ReplyMsg.SucceedResult();
        }
        /// <summary>
        /// 删除指定行内容
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="LineIndex">指定行</param>
        /// <returns></returns>
        public void DeleteRowContent(XSSFSheet sheet, int LineIndex, IWorkbook book = null)
        {
            if (sheet == null&&book==null)
                sheet = newsheet;
            else if (book != null) sheet = workbook.GetSheetAt(0) as XSSFSheet;
            try
            {
                var row = sheet.GetRow(LineIndex);
                sheet.RemoveRow(row);
                //workbook.Write(fs);
            }
            catch (Exception e)
            { 
            }
            //return ReplyMsg.SucceedResult();
        }
        /// <summary>
        ///  删除指定行
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="LineIndex">指定行</param>
        /// <returns></returns>
        public void DeleteRow(XSSFSheet sheet, int LineIndex, int begin = 0, int end = 0)
        {
            if (sheet == null)
                sheet = newsheet;
            if (begin > 0 && end > 0)
            {
                try
                {
                    for (int i = begin; i < end+1; i++)
                        sheet.ShiftRows(begin, sheet.LastRowNum, -1);
                }
                catch (Exception e)
                {
                }
            }
            else
            {
                try
                {
                    sheet.ShiftRows(LineIndex, LineIndex + 1, -1);
                }
                catch (Exception e)
                {
                }
            }
        }

        /// <summary>
        /// 指定行插入内容
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="LineIndex"></param>
        /// <returns></returns>
        //public ReplyMsg InsertRow(XSSFSheet sheet, int LineIndex, HSSFSheet targetRow)
        //{

        //    try
        //    {
        //        sheet.GetRow(0);
        //    }
        //    catch (Exception e)
        //    {
        //    }
        //    return ReplyMsg.SucceedResult();
        //}
        #region Excel复制行
        /// <summary>
        /// Excel复制行
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="sheet"></param>
        /// <param name="starRow"></param>
        /// <param name="rows"></param>
        public void insertRow(XSSFSheet sheet, int starRow, int rows)
        {
            /*
             * ShiftRows(int startRow, int endRow, int n, bool copyRowHeight, bool resetOriginalRowHeight); 
             * 
             * startRow 开始行
             * endRow 结束行
             * n 移动行数
             * copyRowHeight 复制的行是否高度在移
             * resetOriginalRowHeight 是否设置为默认的原始行的高度
             * 
             */

            sheet.ShiftRows(starRow + 1, sheet.LastRowNum, rows, true, true);

            starRow = starRow - 1;

            for (int i = 0; i < rows; i++)
            {

                XSSFRow sourceRow = null;
                XSSFRow targetRow = null;
                XSSFCell sourceCell = null;
                XSSFCell targetCell = null;

                short m;

                starRow = starRow + 1;
                sourceRow = (XSSFRow)sheet.GetRow(starRow);
                targetRow = (XSSFRow)sheet.CreateRow(starRow + 1);
                targetRow.HeightInPoints = sourceRow.HeightInPoints;
                targetRow.RowStyle = sourceRow.RowStyle;
                for (m = (short)sourceRow.FirstCellNum; m < sourceRow.LastCellNum; m++)
                {

                    sourceCell = (XSSFCell)sourceRow.GetCell(m);
                    targetCell = (XSSFCell)targetRow.CreateCell(m);

                    targetCell.CellStyle = sourceCell.CellStyle;
                    targetCell.SetCellType(sourceCell.CellType);

                }
            }

        }

        #endregion
        private Dictionary<string, object> SheelName(IWorkbook workbook)
        {
            int i = 0;

            bool factor = true;
            while (factor)
            {
                try
                {
                    string name = workbook.GetSheetName(i);
                    sheelname.Add(i.ToString(), name);
                }
                catch (Exception e)
                {
                    factor = false;
                }
            }
            return sheelname;
        }
        public ISheet GetSheet(string sheetName)
        { 
            var sheet = workbook.GetSheet(sheetName);
            isheet = sheet;
            return isheet;
        }
        public ISheet GetSheet(XSSFWorkbook Work, string sheetName)
        {
            var sheet = Work.GetSheet(sheetName);
            isheet = sheet;
            return isheet;
        }
        public XSSFWorkbook GetWork()
        {
            return workbook;
        }
    }
}