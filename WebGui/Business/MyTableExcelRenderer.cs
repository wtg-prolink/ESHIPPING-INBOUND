using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using Prolink.Model;
using Prolink.V3;
using Prolink.Web;
using Prolink.Web.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Business
{
    public class MyTableExcelRenderer
    {
         public static ITableExcelHelper Helper = null;

        private ColumnList _columns = null;
        private System.Data.DataTable _dt;
        private string _caption;
        private string _reportName;
        private FieldSet _fs = new FieldSet("");
        private ColumnSummaryList _summaryList = null;
        private HSSFWorkbook _hssfWorkbook;

        public MyTableExcelRenderer(System.Data.DataTable dt, ColumnList columns, string[] totalColumns, string caption)
        {
            this._dt = dt;
            this._columns = columns;
            this._caption = caption;
            _summaryList = new ColumnSummaryList(this, columns, totalColumns);
        }

        HSSFPalette _palette = null;

        /// <summary>
        /// 创建excel文件
        /// </summary>
        /// <param name="templateFile"></param>
        /// <param name="xlsFile"></param>
        /// <param name="format"></param>
        /// <returns>文件路径</returns>
        public string CreateExcelFile(string xlsFile)
        {
            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            _palette = hssfworkbook.GetCustomPalette();

            this._hssfWorkbook = hssfworkbook;
            ISheet sheet = hssfworkbook.CreateSheet("Sheet1");
            CellStyleFactory styleFactory = new CellStyleFactory(hssfworkbook);

            IRow titileRow = sheet.CreateRow(0);
            titileRow.HeightInPoints = 37;
            ICell titileCell = titileRow.CreateCell(0);
            titileCell.SetCellValue(this._caption);
            titileCell.CellStyle = styleFactory.TitleStyle;
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, this._columns.Count - 1));

            IRow columnTitleRow = sheet.CreateRow(1);
            columnTitleRow.HeightInPoints = 25;
            
            for (int i = 0; i < _columns.Count; i++)
            {
                ICell columnTitleCell = columnTitleRow.CreateCell(i);
                columnTitleCell.SetCellValue(_columns[i].Caption);
                columnTitleCell.CellStyle = styleFactory.GetStyle(_columns[i]);
                ((HSSFSheet)sheet).Sheet.SetColumn(i, 0, 0, false, false);
                ((HSSFSheet)sheet).Sheet.ColumnInfos.GetColInfo(i).ColumnWidth = _columns[i].Width * 37;
            }

            ICellStyle cellStyle = this._hssfWorkbook.CreateCellStyle();
            for (int i = 0; i < _dt.Rows.Count; i++)
            {
                IRow dataRow = sheet.CreateRow(2 + i);
                for (int j = 0; j < _columns.Count; j++)
                {
                    ICell dataCell = dataRow.CreateCell(j);
                    RenderCell(dataCell, _columns[j], _dt.Rows[i][_columns[j].FieldName], null, styleFactory, cellStyle, _dt.Rows[i]);
                }
            }
            //render data row--end

            _summaryList.Render(0, 2 + 1, 2 + _dt.Rows.Count, sheet, styleFactory, cellStyle);
            System.IO.FileStream file = new System.IO.FileStream(xlsFile, System.IO.FileMode.Create);
            hssfworkbook.Write(file);
            file.Close();
            return xlsFile;
        }

        public string ReportName
        {
            get
            {
                return _reportName;
            }
            set
            {
                this._reportName = value;
            }
        }

        public FieldSet FieldSet
        {
            get
            {
                return _fs;
            }
            set
            {
                this._fs = value;
            }
        }

        private void RenderCell(ICell cell, Column column, object dbValue, string formula, CellStyleFactory styleFactory, ICellStyle cellStyle,DataRow dr=null)
        {
            object value = dbValue;
            if (dbValue is DateTime)
            {
                value = ((DateTime)dbValue).ToString("yyyyMMddHHmmss");
                value = GetValue(value + "", column);

                cell.SetCellValue(value + "");
                cell.CellStyle = styleFactory.GetStyle(column);

            }
            else if (IsIntegerType(column.FieldName))
            {
                cell.SetCellValue(Prolink.Math.GetValueAsInt(value));
                cell.CellStyle = styleFactory.IntegerStyle;
                cell.SetCellType(CellType.Numeric);
            }
            else if (IsNumberType(column.FieldName))
            {
                cell.SetCellValue(Prolink.Math.GetValueAsDouble(value));

                Field f = _fs[column.FieldName];
                string scaleStr = "#,##0";
                int scale = f.Scale;
                if (scale > 0)
                {
                    scaleStr += ".";
                    while (scale > 0)
                    {
                        scaleStr += "0";
                        scale--;
                    }
                }
                else//如果没有设置scale，则预设两位小数显示
                {
                    scaleStr += ".00";
                }

                if (dr != null)
                {
                    string statusName = column.FieldName + "_Status";
                    if (dr.Table.Columns.Contains(statusName))
                    {
                        if ("N".Equals(Prolink.Math.GetValueAsString(dr[statusName])))
                        {
                            cellStyle = styleFactory.CreateCellStyle();
                            cellStyle.SetFont(styleFactory.RedFont);
                        }
                    }
                }

                //styleFactory.NumberStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("#,##0.00");
                //cell.CellStyle = styleFactory.NumberStyle;
                //ICellStyle cellStyle = this._hssfWorkbook.CreateCellStyle();
                cellStyle.Alignment = HorizontalAlignment.Right;
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.WrapText = true;

                //cellStyle.FillBackgroundColor = _palette.FindColor(255, 0, 0).Indexed;
              
                IDataFormat format = this._hssfWorkbook.CreateDataFormat();
                cellStyle.DataFormat = format.GetFormat(scaleStr);                         
                cell.CellStyle = cellStyle;                
                cell.SetCellType(CellType.Numeric);
            }
            else
            {
                cell.SetCellValue(GetValue(value + "", column));
                cell.CellStyle = styleFactory.GetStyle(column);
            }
            if (formula != null && formula != "") cell.SetCellFormula(formula);
        }


        private string GetValue(string value, Column col)
        {
            if (value == null) return value;
            string dec = (string)col.GetExtraData()["decimals"];
            if (dec != null && dec != "")
            {
                string strFormat = "0.";
                try
                {
                    int intDec = int.Parse(dec);
                    for (int i = 0; i < intDec; i++)
                    {
                        strFormat += "0";
                    }
                }
                catch { }
                return Prolink.Utils.FormatUtils.FormatNumberString(value, strFormat);
            }

            if (col.DisplayFormat != null && col.DisplayFormat != "")
            {
                string storeFormat = col.StoreFormat;
                if (storeFormat == null || storeFormat == "" || storeFormat == "undefined") storeFormat = "YYYYMMDD";
                try
                {
                    return DateTimeFormater.ChangeFormat(storeFormat, col.DisplayFormat, value);
                }
                catch { return value; }
            }

            if (col.ValueList == null || col.ValueList == "") return value;
            string[] ary = col.ValueList.Split(new char[] { ';' });
            for (int i = 0; i < ary.Length; i++)
            {
                string[] temp = ary[i].Split(new char[] { '=' });
                if (temp.Length < 2)
                    continue;
                if (temp[0] == value) return temp[1];
            }

            return value;
        }

       

        private bool IsIntegerType(string fieldName)
        {
            Field f = _fs[fieldName];
            if (f == null) return false;
            //return f.Type == Field.INTEGER_TYPE ||
            //    (f.Type == Field.FLOAT_TYPE && f.Scale == 0);
            return f.Type == Field.INTEGER_TYPE;
        }

        private bool IsNumberType(string fieldName)
        {
            Field f = _fs[fieldName];
            if (f == null) return false;
            //return f.Type == Field.INTEGER_TYPE || f.Type == Field.FLOAT_TYPE;
            return f.Type == Field.FLOAT_TYPE;
        }

        private class ColumnSummaryList
        {
            private class ColumnSummary
            {
                public Column Column;
                public double Value = 0;
            }

            private ColumnSummary[] _columns = null;
            private MyTableExcelRenderer _container = null;

            public ColumnSummaryList(MyTableExcelRenderer container, ColumnList columns, string[] totalColumns)
            {
                _container = container;
                _columns = new ColumnSummary[columns.Count];
                for (int i = 0; i < columns.Count; i++)
                {
                    for (int j = 0; j < totalColumns.Length; j++)
                    {
                        if (columns[i].FieldName == ModelFactory.ReplaceFiledToDBName(totalColumns[j]))
                        {
                            _columns[i] = new ColumnSummary();
                            _columns[i].Column = columns[i];
                            break;
                        }
                    }
                }
            }

            public void Inc(int index, double value)
            {
                if (_columns[index] == null) return;
                _columns[index].Value += value;
            }

            public void Render(int offsetColumnIndex, int startRowIndex, int endRowIndex, ISheet sheet, CellStyleFactory styleFactory, ICellStyle cellStyle)
            {
                if (!HasSummaryColumn()) return;
                IRow summaryRow = sheet.CreateRow(endRowIndex);
                for (int i = 0; i < _columns.Length; i++)
                {
                    if (_columns[i] == null)
                    {
                        summaryRow.CreateCell(offsetColumnIndex + i).CellStyle = styleFactory.LeftStyle;
                        continue;
                    }
                    string xlsColumnIndex = GetXlsColumnIndex(offsetColumnIndex + i);
                    string sumFormula = "SUM(" + xlsColumnIndex + startRowIndex + ":" + xlsColumnIndex + endRowIndex + ")";
                    _container.RenderCell(summaryRow.CreateCell(offsetColumnIndex + i), _columns[i].Column, _columns[i].Value, sumFormula, styleFactory, cellStyle);
                }
            }

            private string GetXlsColumnIndex(int columnIndex)
            {
                int beiShu = columnIndex / 26;
                int yuShu = columnIndex % 26;
                string result = (char)(((int)'A') + yuShu) + "";
                if (beiShu > 0) result = (char)(((int)'A') - 1 + beiShu) + result;
                return result;
            }

            private bool HasSummaryColumn()
            {
                for (int i = 0; i < _columns.Length; i++)
                {
                    if (_columns[i] != null) return true;
                }

                return false;
            }
        }

        public static string GetExcelRenderer(DataTable dt, NameValueCollection nameValues, string columnListStr = "")
        {
            if (string.IsNullOrEmpty(columnListStr))
            {
                columnListStr = nameValues["columnList"];
                if (!string.IsNullOrEmpty(columnListStr))
                    columnListStr = System.Web.HttpUtility.UrlDecode(columnListStr);
            }

            string title = Prolink.Math.GetValueAsString(nameValues["reportTitle"]);
            string excelName = Prolink.Math.GetValueAsString(nameValues["excelName"]);
            string totalCloumnStr = nameValues["totalColumns"];
            string[] totalColumns = new string[] { };
            if (!string.IsNullOrEmpty(totalCloumnStr))
                totalColumns = totalCloumnStr.Split(';');

            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Dictionary<string, object>> list = js.DeserializeObject(columnListStr) as List<Dictionary<string, object>>;
            ArrayList arraylist = js.DeserializeObject(columnListStr) as ArrayList;
            object[] items = js.DeserializeObject(columnListStr) as object[];
            ColumnList columnList = new ColumnList();
            FieldSet result = new FieldSet("");
         
            Column col;
            foreach (Dictionary<string, object> item in items)
            {

                if (item == null)
                {
                    continue;
                }
                string fieldName = Prolink.Math.GetValueAsString(item["name"]);
                fieldName = ModelFactory.ReplaceFiledToDBName(fieldName);
                string caption = Prolink.Math.GetValueAsString(item["caption"]);
                int width = 100;
                if (item.ContainsKey("width"))
                {
                    width = Prolink.Math.GetValueAsInt(item["width"]);
                }
                string alignment = "right";
                if (item.ContainsKey("align"))
                {
                    alignment = Prolink.Math.GetValueAsString(item["align"]);
                }
                col = new Column(fieldName, caption, alignment, width);
                int thisType = 0;
                int thisLength = 1000;
                int thisScale = 0;
                //添加日期格式以及代码/描述转换功能
                if (item.ContainsKey("formatter"))
                {
                    string formatter = Prolink.Math.GetValueAsString(item["formatter"]);

                    if (formatter == "date")
                    {
                        Dictionary<string, object> newformat = item["formatoptions"] as Dictionary<string, object>;
                        IEnumerator ie = newformat.GetEnumerator();
                        while (ie.MoveNext())
                        {
                            KeyValuePair<string, object> kv = (KeyValuePair<string, object>)ie.Current;
                            string value = Prolink.Math.GetValueAsString(kv.Value);
                            switch (value)
                            {
                                case "Y-m-d":
                                    {
                                        col.DisplayFormat = "yyyy-MM-dd";
                                        break;
                                    }//目前仅mapping一种格式，此处往下可以继续写case种类
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                    }
                    else if (formatter == "select")
                    {
                        Dictionary<string, object> editoptions = item["editoptions"] as Dictionary<string, object>;
                        IEnumerator ie = editoptions.GetEnumerator();
                        while (ie.MoveNext())
                        {
                            KeyValuePair<string, object> kv = (KeyValuePair<string, object>)ie.Current;
                            string value = Prolink.Math.GetValueAsString(kv.Value);
                            col.ValueList = value.Replace(":", "=");
                        }
                    }
                    else if (formatter == "number" || formatter == "currency" || formatter == "float")
                    {
                        thisType = Field.FLOAT_TYPE;
                        if (item.ContainsKey("formatoptions"))
                        {
                            Dictionary<string, object> formatoptions = item["formatoptions"] as Dictionary<string, object>;
                            IEnumerator ie = formatoptions.GetEnumerator();
                            while (ie.MoveNext())
                            {
                                KeyValuePair<string, object> kv = (KeyValuePair<string, object>)ie.Current;
                                if (kv.Key == "decimalPlaces")
                                {
                                    thisScale = Prolink.Math.GetValueAsInt(kv.Value);
                                }
                            }
                        }
                    }
                    else if (formatter == "int" || formatter == "integer")
                    {
                        thisType = Field.INTEGER_TYPE;
                    }
                }

                columnList.Add(col);

                if (item.ContainsKey("maxLength"))
                {
                    thisLength = Prolink.Math.GetValueAsInt(item["maxLength"]);
                }
                result.Add(new Field(fieldName)
                {
                    Type = thisType,
                    Length = thisLength,
                    Scale = thisScale
                });
            }

            MyTableExcelRenderer tableExcelRenderer = new MyTableExcelRenderer(dt, columnList, totalColumns, title);
            if (excelName == "null")
            {
                tableExcelRenderer.ReportName = System.Guid.NewGuid().ToString();
            }
            else
            {
                tableExcelRenderer.ReportName = excelName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            tableExcelRenderer.FieldSet = result;

            var reportPath = string.Format("{0}{1}.xls", System.IO.Path.GetTempPath(), tableExcelRenderer.ReportName);
            string xlsFile = tableExcelRenderer.CreateExcelFile(reportPath);
            return xlsFile;
        }

        private class CellStyleFactory
        {
            private ICellStyle _titleStyle;
            private ICellStyle _leftStyle;
            private ICellStyle _centerStyle;
            private ICellStyle _rightStyle;
            private ICellStyle _integerStyle;
            private ICellStyle _numberStyle;
            private IFont _redFontStyle;
            HSSFWorkbook _hssfworkbook = null;
            public CellStyleFactory(HSSFWorkbook hssfworkbook)
            {
                _hssfworkbook = hssfworkbook;
                _titleStyle = CreateStyle(hssfworkbook, HorizontalAlignment.Center);
                IFont font = hssfworkbook.CreateFont();
                font.FontHeightInPoints = 20;
                _titleStyle.SetFont(font);
                _leftStyle = CreateStyle(hssfworkbook, HorizontalAlignment.Left);
                _centerStyle = CreateStyle(hssfworkbook, HorizontalAlignment.Center);
                _rightStyle = CreateStyle(hssfworkbook, HorizontalAlignment.Right);
                _integerStyle = CreateStyle(hssfworkbook, HorizontalAlignment.Right);
                _integerStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("#,##0");
                _redFontStyle = setCellStyle(hssfworkbook); 
                //_numberStyle = CreateStyle(hssfworkbook, HorizontalAlignment.Right);
                //_numberStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat(@"\#\,\##0.###########");            
            }

            public ICellStyle CreateCellStyle()
            {
                return _hssfworkbook.CreateCellStyle();
            }

            private IFont setCellStyle(HSSFWorkbook workbook)
            {
                IFont ffont = workbook.CreateFont();
                ffont.Color = HSSFColor.Red.Index;
                return ffont;
            }

            private static ICellStyle CreateStyle(HSSFWorkbook hssfworkbook, HorizontalAlignment align)
            {
                ICellStyle style = hssfworkbook.CreateCellStyle();
                style.Alignment = align;
                style.VerticalAlignment = VerticalAlignment.Center;
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.WrapText = true;
                return style;
            }

            public IFont RedFont
            {
                get
                {
                    return this._redFontStyle;
                }
            }


            public ICellStyle TitleStyle
            {
                get
                {
                    return this._titleStyle;
                }
            }

            public ICellStyle LeftStyle
            {
                get
                {
                    return this._leftStyle;
                }
            }

            public ICellStyle CenterStyle
            {
                get
                {
                    return this._centerStyle;
                }
            }

            public ICellStyle RightStyle
            {
                get
                {
                    return this._rightStyle;
                }
            }

            public ICellStyle IntegerStyle
            {
                get
                {
                    return this._integerStyle;
                }
            }

            public ICellStyle NumberStyle
            {
                get
                {
                    return this._numberStyle;
                }
            }

            public ICellStyle GetStyle(Column column)
            {
                if (column.Alignment == "right")
                {
                    return RightStyle;
                }
                else if (column.Alignment == "center")
                {
                    return CenterStyle;
                }
                else
                {
                    return LeftStyle;
                }
            }
        }
    }

    public interface ITableExcelHelper
    {
        string GetHeaderClassName(string name);
        string GetColumnHeaderClassName(string name, Column col);
        string GetHeaderBelowContent(string name);
        string RegisterStyleContent(string name);
    }
}