using NPOI.XSSF.UserModel;
using NPOI.XSSF.UserModel;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using TrackingEDI.Mail;
using TrackingEDI.Utils;

namespace TrackingEDI.Business
{
    public class ASDReportHelper
    {
        string templatefile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "download/ASDReport.xls");
        string ASDType = string.Empty;

        public ASDReportHelper(string asdtype)
        {
            ASDType = asdtype;
        }

        public string GetTemplateData(int year, string month, ASDUserInfo userinfo)
        {
            string xlsFile = string.Empty;
            try
            {
                File.SetAttributes(templatefile, FileAttributes.Normal);
                FileStream templetefs = new FileStream(templatefile, FileMode.Open);
                XSSFWorkbook hssfworkbook = new XSSFWorkbook(templetefs);
                XSSFSheet sheet1 = (XSSFSheet)hssfworkbook.GetSheetAt(0);
                ASDReportLogic reportLogic = new ASDReportLogic(year, month, userinfo.GroupId, userinfo.CompanyId, ASDType);

                //DataTable expressDt = reportLogic.GetCostFeeData(); //查询非运费-FI费用
                //DataTable tkbilmMainDt = reportLogic.GetCostFeeByFIData(); //查询运费-FI费用
                reportLogic.createDnRowExcel(sheet1);
                string ext = ".xls"; //生成文件的扩展名
                xlsFile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], string.Format("UploadFiles/CostReport/{0}{1}", System.Guid.NewGuid().ToString(), ext));
                string directoryPath = Path.GetDirectoryName(xlsFile);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(xlsFile, System.IO.FileMode.Create))
                {
                    hssfworkbook.Write(fs);
                }
                hssfworkbook.Close();
                hssfworkbook = null;
                reportLogic = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return xlsFile;
        }


    }

    public class ASDUserInfo
    {
        public string UserId { get; set; }
        public string CompanyId { get; set; }
        public string GroupId { get; set; }
    }
    public class ASDReportLogic
    {
        string mappingName = "ASDReportMapping";
        string smdnpMappingName = "ASDReportSMDNPMapping";
        //TCGT 货型
        string bsCodeTypes = "'TCGT','TRGN','TNT','TCAR'";
        Dictionary<string, object> mapping = new Dictionary<string, object>();
        Dictionary<string, object> smdnpmapping = new Dictionary<string, object>();
        Dictionary<string, object> fields = new Dictionary<string, object>();
        Dictionary<string, object> smdnpfields = new Dictionary<string, object>();
        string sqlfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rpt/template/AsdSql.xml");
        XmlDocument sqldoc = new XmlDocument();
        
        Dictionary<string, object> field = null;
        DataTable baseCodeDt = new DataTable();
        string[] shipmentids = null;
        string _groupid=string.Empty;
        string _cmp=string.Empty;
        string _asdType = string.Empty;
        int _year = 0;
        string _month = string.Empty;

        public ASDReportLogic(int year,string month,string groupid,string cmp,string asdtype)
        {
            mapping = XmlParser.GetMapping(mappingName, true);
            smdnpmapping = XmlParser.GetMapping(smdnpMappingName, true);
            fields = XmlParser.GetFields(mapping);
            smdnpfields = XmlParser.GetFields(smdnpmapping);
            field = null;
            string xml = new System.IO.StreamReader(sqlfilepath).ReadToEnd();
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("sqlfilepath:" + sqlfilepath);
            sqldoc.LoadXml(xml);
            _year = year;
            _month = month;
            shipmentids = null;
            _groupid=groupid;
            _cmp=cmp;
            _asdType = asdtype;
            baseCodeDt = MailTemplate.GetBaseData(bsCodeTypes, groupid, cmp);
        }

        public DataTable GetCostFeeData()
        {
            //获取费用分摊数据
            string sql = string.Format(@"SELECT *,(SELECT TOP 1 LSP_NO FROM SMBIM WHERE SMBIM.U_ID=SMBID.U_FID) 
                AS LSP_NO,(SELECT TOP 1 LSP_NM FROM SMBIM WHERE SMBIM.U_ID=SMBID.U_FID) AS LSP_NM FROM SMBID
                 WHERE SHIPMENT_ID IN {0} ORDER BY CHG_CD", SQLUtils.Quoted(shipmentids));
            return Database.GetDataTable(sql, null);
        }

        public DataTable GetCostFeeByFIData()
        {
            //获取FI的费用分摊数据
            string sql = string.Format(@"SELECT *,(SELECT TOP 1 LSP_NO FROM SMBIM WHERE SMBIM.U_ID=SMBID.U_FID) 
                AS LSP_NO,(SELECT TOP 1 LSP_NM FROM SMBIM WHERE SMBIM.U_ID=SMBID.U_FID) AS LSP_NM FROM SMBID
                 WHERE SHIPMENT_ID IN {0} ORDER BY CHG_CD", SQLUtils.Quoted(shipmentids));
            return Database.GetDataTable(sql, null);
        }

        public int createDnRowExcel(XSSFSheet sheet1)
        {
            int excelRowIndex = 1;
            //string smsmsql = string.Format(@"SELECT * FROM TEMP_ASD_MASTER08 WHERE {0} ORDER BY TRAN_TYPE,ETD DESC ", siWhere);
            //string smsmsql = "SELECT DISTINCT SHIPMENT_ID FROM TEMP_ASD_MASTER08 WHERE CMP='" + _cmp + "'";
            string smsmsql = string.Format(@"SELECT DISTINCT SHIPMENT_ID,TRAN_TYPE,SM_ETD FROM TEMP_ASD_MASTER{0}{1} WHERE SM_CMP={2} ORDER BY TRAN_TYPE,SM_ETD DESC ", _year, _month, SQLUtils.QuotedStr(_cmp));
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("createDnRowExcel"+smsmsql);
            DataTable smsmDt = Database.GetDataTable(smsmsql, null);
            DataView dv = smsmDt.DefaultView;
            DataTable shipmentDt = dv.ToTable(true, "SHIPMENT_ID");
            DataTable rateDt1 = Database.GetDataTable("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' ORDER BY EDATE", null);
            List<string> shipmentlist=new List<string>();
            foreach (DataRow dr in shipmentDt.Rows)
            {
                shipmentlist.Add(Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
            }
            shipmentids = shipmentlist.ToArray();
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("shipmentlist Count:" + shipmentlist.Count);
            //string smdnSql = string.Format(GetXmlDocSql("SMDN"), SQLUtils.Quoted(shipmentids));
            string smdnSql = string.Format(GetXmlDocSql("SMDN"), _year, _month, SQLUtils.QuotedStr(_cmp));
            DataTable smdnDt= Database.GetDataTable(smdnSql, null);
            string smdnpSql = string.Empty;
            DataTable smdnpDt = new DataTable();
            try
            {
                smdnpSql = string.Format(GetXmlDocSql("SMDNP"), _year, _month, SQLUtils.QuotedStr(_cmp));
                smdnpDt = Database.GetDataTable(smdnpSql, null);
            }
            catch (Exception ex)
            {
                smdnpSql = "";
                throw ex;
            }
            int smdnRowCount = smdnDt.Rows.Count;

            DataTable smbidDt=Database.GetDataTable(string.Format("SELECT * FROM SMBID WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentids)),null);

            DataTable chgCodeDt = Database.GetDataTable(string.Format(@"SELECT DISTINCT CHG_CD, CHG_CD+' '+CHG_DESCP CHGNAME FROM SMCHG WHERE  GROUP_ID ={0} AND CMP={1}
                            ORDER BY CHG_CD ASC",  SQLUtils.QuotedStr(_groupid), SQLUtils.QuotedStr(_cmp)), null);

            string shipmentid=string.Empty;
            int titlecell = 0;
            for (int j = 0; j < fields.Count; j++)
            {
                List<string> test = new List<string>(fields.Keys);
                field = fields[test[j]] as Dictionary<string, object>;
                string cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                int cellnum = ToIndex(cellcode);
                if (titlecell < cellnum)
                    titlecell = cellnum;
            }

             XSSFRow row = (XSSFRow)sheet1.GetRow(0);
            Dictionary<string, object> smbidfields = new Dictionary<string, object>();
            string chgcellcode = string.Empty;
            Dictionary<string, object> indexfields=new Dictionary<string, object>();
            string chgname=string.Empty;
            string chgcode = string.Empty;
            for (int j = 0,s=0; j < chgCodeDt.Rows.Count; j++){
                chgname= chgCodeDt.Rows[j]["CHGNAME"].ToString();
                chgcode = chgCodeDt.Rows[j]["CHG_CD"].ToString();
                if (smbidfields.ContainsKey(chgcode))
                    continue;
                indexfields = new Dictionary<string, object>();
                indexfields.Add("name",chgname);
                indexfields.Add("fieldname", chgcode);
                indexfields.Add("key", "False");
                indexfields.Add("defalutValue", "");
                indexfields.Add("dataType", "decimal");
                chgcellcode = excelColIndexToStr(s + titlecell+1);
                indexfields.Add("cellCode", chgcellcode);
                smbidfields.Add(chgcode, indexfields);
                XSSFCell XSSFcell = (XSSFCell)row.CreateCell(s + titlecell + 1);
                //XSSFCell XSSFcell = (XSSFCell)row.GetCell(s + titlecell + 1);
                var fieldname = chgname;
                formatCellValue(XSSFcell, "string", fieldname); //这边siDt.Rows[i][j]用的是索引
                s++;
            }
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("smsmDt Rows Count:" + smsmDt.Rows.Count);
            for (int s = 0; s < smsmDt.Rows.Count; s++)
            {
                //masterRow = (XSSFRow)sheet1.CreateRow(excelRowIndex);
                //生成excel中的主表区域
                shipmentid = Prolink.Math.GetValueAsString(smsmDt.Rows[s]["SHIPMENT_ID"]);
                DataRow[] smdnrows = smdnDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)),"DN_NO DESC");
                DataTable smdnbySMDt = smdnDt.Clone();
                for (int i = 0; i < smdnrows.Length; i++)
                {
                    smdnbySMDt.ImportRow(smdnrows[i]);
                }
                if (smdnbySMDt.Rows.Count <= 0) continue;

                DateTime evendate = Prolink.Math.GetValueAsDateTime(smdnbySMDt.Rows[0]["SM_ETD"]);
                if (smdnbySMDt.Rows[0]["ATD"] != null && smdnbySMDt.Rows[0]["ATD"] != DBNull.Value)
                {
                    evendate = Prolink.Math.GetValueAsDateTime(smdnbySMDt.Rows[0]["ATD"]);
                }

                DataTable rateDt = CommonManager.GetRate(rateDt1, evendate);
                excelRowIndex = createMasterExcelCellByRows(shipmentid, fields, sheet1, smdnbySMDt, smbidDt, chgCodeDt, excelRowIndex,smbidfields,rateDt);

                DataRow[] smdnprows = smdnpDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)), "DN_NO DESC");
                DataTable smdnpbySMDt = smdnpDt.Clone();
                for (int i = 0; i < smdnprows.Length; i++)
                {
                    smdnpbySMDt.ImportRow(smdnprows[i]);
                }
                if (smdnpbySMDt.Rows.Count > 0)
                {
                    //设置细档的数据
                    excelRowIndex = createSubExcelCellByRows(shipmentid, smdnpfields, sheet1, smdnpbySMDt, smbidDt, chgCodeDt, excelRowIndex, smbidfields, rateDt);
                    //重新设置主表下一行的位置
                    excelRowIndex++;
                }
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("rowsindex" + s + "excelRowIndex:" + excelRowIndex);
            }
            return smdnRowCount;
        }

        public string GetXmlDocSql(string xmlnodename){
            XmlNode rootNode = sqldoc.SelectSingleNode("root");
            XmlNodeList nodeList = rootNode.ChildNodes;
            foreach (XmlNode node in nodeList)
            {
                if (xmlnodename.Equals(node.Name))
                    return node.InnerText;
            }
            return string.Empty;
        }

        /// <summary>
        /// 生成excel中主表的区域
        /// </summary>
        /// <param name="fieldCount">查查询的字段数</param>
        /// <param name="row">excel中的行</param>
        /// <param name="dt">存放数据的DataTable</param>
        /// <param name="dtRowIndex">DataTable中的行索引</param>
        /// <param name="compareField">用于相邻行需要比较的字段</param>
        /// <param name="siCount">主档记录数</param>
        /// <param name="zeroFieldList">要设置为0的字段的列表</param>
        private void generateMasterExcelCell(int fieldCount, XSSFRow row, DataTable dt, int dtRowIndex, int siCount, string compareField, string zeroField)
        {
            //创建主表单元格列区域
            createMasterExcelCell(fieldCount, row, dt, dtRowIndex);
        }

        /// <summary>
        /// 创建主表单元格列区域
        /// </summary>
        /// <param name="fieldCount">查查询的字段数</param>
        /// <param name="row">excel中的行</param>
        /// <param name="dt">存放数据的DataTable</param>
        /// <param name="dtRowIndex">DataTable中的行索引</param>
        private void createMasterExcelCell(int fieldCount, XSSFRow row, DataTable dt, int dtRowIndex)
        {
            List<string> keys = XmlParser.GetKeys(mapping);
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            string cellcode=string.Empty; int cellnum=0;
            for (int j = 0; j < fieldCount; j++)
            {
                List<string> test = new List<string>(fields.Keys);
                field = fields[test[j]] as Dictionary<string, object>;

                fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                cellnum = ToIndex(cellcode);
                if(cellnum<=0) continue;
                XSSFCell XSSFcell = (XSSFCell)row.CreateCell(cellnum);
                
                try
                {
                    fieldValue = Prolink.Math.GetValueAsString(dt.Rows[dtRowIndex][fieldname]);
                }
                catch(Exception ex)
                {
                    fieldValue = string.Empty;
                }
                formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引
            }
        }

        /// <summary>
        /// 创建主表单元格列区域
        /// </summary>
        /// <param name="fieldCount">查查询的字段数</param>
        /// <param name="row">excel中的行</param>
        /// <param name="dt">存放数据的DataTable</param>
        /// <param name="dtRowIndex">DataTable中的行索引</param>
        private int createMasterExcelCellByRows(string shipmentid,Dictionary<string, object> mapfields, XSSFSheet sheet1, DataTable smdndt, DataTable smbidDt, DataTable chgCodeDt, int excelRowIndex,Dictionary<string, object> smindfields,DataTable ratedt)
        {
            List<string> keys = XmlParser.GetKeys(mapping);
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            string cellcode = string.Empty; int cellnum = 0;
            int fieldCount = mapfields.Count;
            string trantype = Prolink.Math.GetValueAsString(smdndt.Rows[0]["TRAN_TYPE"]);
            for (int i = 0; i < smdndt.Rows.Count; i++)
            {
                XSSFRow row = (XSSFRow)sheet1.CreateRow(excelRowIndex);
                for (int j = 0; j < fieldCount; j++)
                {
                    fieldValue = string.Empty;
                    List<string> test = new List<string>(mapfields.Keys);
                    field = mapfields[test[j]] as Dictionary<string, object>;

                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                    dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                    cellnum = ToIndex(cellcode);
                    if (cellnum < 0) continue;
                    XSSFCell XSSFcell = (XSSFCell)row.CreateCell(cellnum);

                    try
                    {
                        fieldValue=GetBsCodeDescpByFieldName(fieldname, fieldValue, smdndt.Rows[i]);
                    }
                    catch (Exception ex)
                    {
                        fieldValue = string.Empty;
                    }
                    formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引
                }
                string amtfield = "QAMT";
                string cur = "QCUR";
                Decimal chgqty=0;

                #region 计算费用
                DataRow[] smbidrows = smbidDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)));
                DataTable smbidrowdt = smbidDt.Clone();
                foreach (DataRow dr in smbidrows)
                {
                    smbidrowdt.ImportRow(dr);  
                }
                for (int j = 0; j < smindfields.Count; j++)
                {
                    cellnum++;
                    fieldValue = string.Empty;
                    List<string> test = new List<string>(smindfields.Keys);
                    field = smindfields[test[j]] as Dictionary<string, object>;

                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                    dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                    cellnum = ToIndex(cellcode);
                    if (cellnum < 0) continue;
                    XSSFCell XSSFcell = (XSSFCell)row.CreateCell(cellnum);

                    foreach (DataRow dr in smbidrows)
                    {
                        if (fieldname != dr["CHG_CD"].ToString())
                            continue;
                        if ("B".Equals(_asdType))
                        {
                            amtfield = "BAMT";
                            cur = "CUR";
                        }
                        if (smbidrows.Length <= 0) continue;
                        Decimal chgqty1 = Prolink.Math.GetValueAsDecimal(dr[amtfield]);
                        chgqty1=GetSmbidamt(smbidrowdt, amtfield, fieldname);
                        chgqty = chgqty1;
                        string curreny = Prolink.Math.GetValueAsString(dr[cur]);
                        bool error = false;
                        decimal rate = 1;
                        if (("F".Equals(trantype) || "L".Equals(trantype)) && "OF".Equals(fieldname))
                        {
                            if ("USD".Equals(curreny))
                            {
                                chgqty = chgqty1;
                            }
                            else
                            {
                                rate = CommonManager.GetTotal(ratedt, null, chgqty1, curreny, ref chgqty, ref error, "USD");
                            }
                        }
                        else
                        {
                            if (!"B".Equals(_asdType))
                                chgqty = GetSmbidamt(smbidrowdt, "QLAMT", fieldname);
                            //rate = CommonManager.GetTotal(ratedt, null, chgqty1, curreny, ref chgqty, ref error, "CNY");
                        }
                        
                        //获取DN体积所占的比例
                        //Decimal percent = GetPerCent(dr);
                        Decimal ttlcbm = Prolink.Math.GetValueAsDecimal(smdndt.Compute("Sum(CBM)", "true"));
                        Decimal cbm = Prolink.Math.GetValueAsDecimal(smdndt.Rows[i]["CBM"]);
                        if (cbm <= 0)
                        {
                            ttlcbm = Prolink.Math.GetValueAsDecimal(smdndt.Compute("Sum(AMOUNT1)", "true"));
                            cbm = Prolink.Math.GetValueAsDecimal(smdndt.Rows[i]["AMOUNT1"]);
                        }
                        Decimal percent = 1;
                        if (ttlcbm > 0)
                        {
                            percent = cbm / ttlcbm;
                        }


                        fieldValue = System.Math.Round(Prolink.Math.GetValueAsDouble(chgqty * percent), 2).ToString();
                        break;
                    }
                    formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引
                }
                #endregion
                excelRowIndex =excelRowIndex+1;
            }
            return excelRowIndex;
        }

        public decimal GetSmbidamt(DataTable smbiddt, string fieldname, string chgcode)
        {
            return Prolink.Math.GetValueAsDecimal(smbiddt.Compute("Sum(" + fieldname + ")", string.Format("CHG_CD={0}", SQLUtils.QuotedStr(chgcode))));
        }

        /// <summary>
        /// 创建子表单元格列区域
        /// </summary>
        /// <param name="fieldCount">查查询的字段数</param>
        /// <param name="row">excel中的行</param>
        /// <param name="dt">存放数据的DataTable</param>
        /// <param name="dtRowIndex">DataTable中的行索引</param>
        private int createSubExcelCellByRows(string shipmentid, Dictionary<string, object> mapfields, XSSFSheet sheet1, DataTable smdnpDt, DataTable smbidDt, DataTable chgCodeDt, int excelRowIndex, Dictionary<string, object> smindfields, DataTable ratedt)
        {
            List<string> keys = XmlParser.GetKeys(mapping);
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            string cellcode = string.Empty; int cellnum = 0;
            int fieldCount = mapfields.Count;
            int dnpcount=smdnpDt.Rows.Count;
            string trantype = Prolink.Math.GetValueAsString(smdnpDt.Rows[0]["TRAN_TYPE"]);
            for (int i = 0; i < dnpcount; i++)
            {
                XSSFRow row = (XSSFRow)sheet1.CreateRow(excelRowIndex);
                for (int j = 0; j < fieldCount; j++)
                {
                    List<string> test = new List<string>(mapfields.Keys);
                    field = mapfields[test[j]] as Dictionary<string, object>;

                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                    dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                    cellnum = ToIndex(cellcode);
                    if (cellnum < 0) continue;
                    XSSFCell XSSFcell = (XSSFCell)row.CreateCell(cellnum);

                    try
                    {
                        fieldValue = GetBsCodeDescpByFieldName(fieldname, fieldValue, smdnpDt.Rows[i], false);
                    }
                    catch (Exception ex)
                    {
                        fieldValue = string.Empty;
                    }
                    formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引
                }
                string amtfield = "QAMT";
                string cur = "QCUR";
                Decimal chgqty = 0;

                #region 计算费用
                DataRow[] smbidrows = smbidDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)));
                DataTable smbidrowdt = smbidDt.Clone();
                foreach (DataRow dr in smbidrows)
                {
                    smbidrowdt.ImportRow(dr);
                }
                for (int j = 0; j < smindfields.Count; j++)
                {
                    cellnum++;
                    fieldValue = string.Empty;
                    List<string> test = new List<string>(smindfields.Keys);
                    field = smindfields[test[j]] as Dictionary<string, object>;

                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                    dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                    cellnum = ToIndex(cellcode);
                    if (cellnum < 0) continue;
                    XSSFCell XSSFcell = (XSSFCell)row.CreateCell(cellnum);

                    foreach (DataRow dr in smbidrows)
                    {
                        if (fieldname != dr["CHG_CD"].ToString())
                            continue;
                        if ("B".Equals(_asdType))
                        {
                            amtfield = "BAMT";
                            cur = "CUR";
                        }
                        if (smbidrows.Length <= 0) continue;
                        Decimal chgqty1 = Prolink.Math.GetValueAsDecimal(dr[amtfield]);
                        chgqty1 = GetSmbidamt(smbidrowdt, amtfield, fieldname);
                        chgqty = chgqty1;
                        string curreny = Prolink.Math.GetValueAsString(dr[cur]);
                        bool error = false;
                        decimal rate = 1;
                        if (("F".Equals(trantype) || "L".Equals(trantype)) && "OF".Equals(fieldname))
                        {
                            if ("USD".Equals(curreny))
                            {
                                chgqty = chgqty1;
                            }
                            else
                            {
                                rate = CommonManager.GetTotal(ratedt, null, chgqty1, curreny, ref chgqty, ref error, "USD");
                            }
                        }
                        else
                        {
                            if (!"B".Equals(_asdType))
                                chgqty = GetSmbidamt(smbidrowdt, "QLAMT", fieldname);
                            //rate = CommonManager.GetTotal(ratedt, null, chgqty1, curreny, ref chgqty, ref error, "CNY");
                        }
                        
                        //获取DN体积所占的比例
                        Decimal ttlcbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Compute("Sum(CBM)", "true"));
                        Decimal cbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Rows[i]["CBM"]);
                        if (cbm <= 0)
                        {
                            ttlcbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Compute("Sum(VALUE1)", "true"));
                            cbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Rows[i]["VALUE1"]);
                        }
                        if (cbm <= 0)
                        {
                            ttlcbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Compute("Sum(GW)", "true"));
                            cbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Rows[i]["GW"]);
                        }
                        if (cbm <= 0)
                        {
                            ttlcbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Compute("Sum(QTY)", "true"));
                            cbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Rows[i]["QTY"]);
                        }
                        Decimal percent = 1;
                        if (ttlcbm > 0)
                        {
                            percent = cbm / ttlcbm;
                        }
                        fieldValue = System.Math.Round(Prolink.Math.GetValueAsDouble(chgqty * percent), 2).ToString();
                    }
                    formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引
                }
                #endregion
                if (i < dnpcount - 1)
                {
                    excelRowIndex = excelRowIndex + 1;
                }
            }
            return excelRowIndex;
        }


        public string GetBsCodeDescpByFieldName(string fieldname, string code,DataRow dr,bool IsMaster=true)
        {
            string fieldValue = string.Empty;
            
            try
            {
                string newfieldname = fieldname;
                if (newfieldname.StartsWith("HTML_"))
                    newfieldname = newfieldname.Replace("HTML_", "");
                code = Prolink.Math.GetValueAsString(dr[newfieldname]);
            }catch(Exception ex){
                code = string.Empty;
            }
            
            switch (fieldname)
            {
                case "HTML_SCREEN_CNDNS":
                    if (IsMaster) return "BY DN";
                    return "BY DN item";
                case "HTML_MONTH":
                    return MonthToEnglish(dr,IsMaster);
                    //return MonthToEnglish(Prolink.Math.GetValueAsInt(dr["MONTH"]));
                case "HTML_STN":
                    return fieldValue = Database.GetValueAsString(string.Format("SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO={0} AND CMP={1}", SQLUtils.QuotedStr(code), SQLUtils.QuotedStr(_cmp)));
                    break;
                case "CARGO_TYPE":
                    fieldValue = MailTemplate.GetBaseCodeValue(baseCodeDt, "TCGT", code);
                    break;
                case "TRAN_TYPE":
                    fieldValue = MailTemplate.GetBaseCodeValue(baseCodeDt, "TNT", code);
                    break;
                case "CARRIER":
                    fieldValue = MailTemplate.GetBaseCodeApCdValue(baseCodeDt, "TCAR", code);
                    break;

                case "HTML_CNT20":
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT20"]) / 2) * GetPerCent(dr), 2).ToString();
                case "HTML_CNT40":
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT40"])) * GetPerCent(dr), 2).ToString();
                case "HTML_CNT40HQ":
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT40HQ"])) * GetPerCent(dr), 2).ToString();
                case "HTML_CNT_NUMBER":
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT_NUMBER"])) * GetPerCent(dr), 2).ToString();
                case "HTML_FEU":
                    var htmlfeu = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT20"]) / 2 + Prolink.Math.GetValueAsDecimal(dr["CNT40"]) + Prolink.Math.GetValueAsDecimal(dr["CNT40HQ"])) * GetPerCent(dr), 2);
                    fieldValue = htmlfeu.ToString();
                    break;
                case "HTML_CNTRY":
                    string region = Prolink.Math.GetValueAsString(dr["REGION"]);
                    if ("CN".Equals(region) || "DB".Equals(region) || "HB".Equals(region) || "HD".Equals(region) ||
                        "HN".Equals(region) || "HZ".Equals(region) || "XB".Equals(region) || "XN".Equals(region))
                        return fieldValue = "China";
                    string pod = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                    if (pod.Length != 5) return string.Empty;
                    string cntycd = pod.Substring(0, 2);
                    return fieldValue = Database.GetValueAsString(string.Format("SELECT TOP 1 CNTRY_NM FROM BSCNTY WHERE CNTRY_CD={0}", SQLUtils.QuotedStr(cntycd)));
            }
            if (string.IsNullOrEmpty(fieldValue)) return code;
            return fieldValue;
        }

        public decimal GetPerCent(DataRow dr)
        {
            Decimal percent = 1;
            Decimal mincbm = Prolink.Math.GetValueAsDecimal(dr["MINCBM"]);
            Decimal cbm = Prolink.Math.GetValueAsDecimal(dr["CBM"]);
            Decimal ttlcbm = Prolink.Math.GetValueAsDecimal(dr["SM_CBM"]);
            if (mincbm <= 0)
            {
                cbm = Prolink.Math.GetValueAsDecimal(dr["AMOUNT1"]);
                ttlcbm = Prolink.Math.GetValueAsDecimal(dr["GVALUE"]);
            }
            percent = cbm / ttlcbm;
            return percent;

        }

        public string MonthToEnglish(DataRow dr,bool ismaster)
        {
            DateTime etd = Prolink.Math.GetValueAsDateTime(dr["ETD"]);
            if (ismaster)
            {
                etd = Prolink.Math.GetValueAsDateTime(dr["SM_ETD"]);
            }
            int month = etd.Month;
            if (dr["ATD"] != null && dr["ATD"] != DBNull.Value)
            {
                DateTime atd = Prolink.Math.GetValueAsDateTime(dr["ATD"]);
                month = atd.Month;
            }
            switch (month)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
            }
            return "";
        }

        public String excelColIndexToStr(int columnIndex)
        {
            if (columnIndex < 0)
            {
                return null;
            }
            string columnStr = "";
            //columnIndex--;
            do
            {
                if (columnStr.Length > 0)
                {
                    columnIndex--;
                }
                columnStr = ((char)(columnIndex % 26 + (int)'A')) + columnStr;
                columnIndex = (int)((columnIndex - columnIndex % 26) / 26);
            } while (columnIndex > 0);
            return columnStr;
        }


        public int ToIndex(string columnName)
        {
            if (!Regex.IsMatch(columnName.ToUpper(), @"[A-Z]+"))
            {
                return -1;
            }
            int index = 0;
            char[] chars = columnName.ToUpper().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                index+=((int)chars[i]-(int)'A'+1)*(int)Math.Pow(26,chars.Length-i-1);
            }
            return index-1;
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

        private static List<string> GetNameList(Dictionary<string, object> fields, string name)
        {
            List<string> list = new List<string>();
            foreach (var kv in fields)
            {
                Dictionary<string, object> field = kv.Value as Dictionary<string, object>;
                string fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                //string caption = Prolink.Math.GetValueAsString(field["name"]);
                if (name.Equals(fieldname))
                    list.Add(kv.Key);
            }
            return list;
        }

        /// <summary>
        /// 生成excel中子表的区域
        /// </summary>
        /// <param name="masterFieldCount">主表查查询的字段数</param>
        /// <param name="ItemFieldCount">从表要查询的字段数</param>
        /// <param name="excelRow">excel中的子表的行</param>
        /// <param name="dt">子表DataTable</param>
        /// <param name="dtRowIndex">DataTable中的行的索引</param>
        /// <returns></returns>
        private void generateSubRowCell(int masterFieldCount, int ItemFieldCount, XSSFRow excelRow, DataTable dt, int dtRowIndex)
        {
            int excelColStartIndex = masterFieldCount; //excel细档列的起始位置
            int dtColumnIndex = 0; //DataTable中列的索引位置
            for (int j = 0; j < ItemFieldCount; j++)
            {
                dtColumnIndex = j + masterFieldCount; //计算子表对应的DataTable中的位置
                XSSFCell XSSFcell=(XSSFCell)excelRow.CreateCell(excelColStartIndex);
                formatCellValue(XSSFcell, dt.Columns[dtColumnIndex].DataType.Name, dt.Rows[dtRowIndex][dtColumnIndex].ToString()); //这边siDt.Rows[i][j]用的是索引
                excelColStartIndex++;
            }
        }

        /// <summary>
        /// 从DataTable中复制指定列的数据
        /// </summary>
        /// <param name="copyColumns">要复制的列</param>
        /// <param name="sourcedt">复制数据的源数据所在的DataTable</param>
        /// <returns></returns>
        private DataTable getDataByColumn(string copyColumns, DataTable sourcedt, DataTable destinDt)
        {
            //复制数据
            copyData(sourcedt, destinDt);
            return destinDt;
        }

        /// <summary>
        /// 从DataTable中复制指定列的数据
        /// </summary>
        /// <param name="copyColumns">要复制的列</param>
        /// <param name="sourcedt">复制数据的源数据所在的DataTable</param>
        /// <returns></returns>
        private DataTable getDataByColumn(string copyColumns, DataTable sourcedt)
        {
            DataTable destinDt = new DataTable();
            string[] copyColumnsArray = copyColumns.Split(',');
            //给新建的DataTable添加列
            foreach (string element in copyColumnsArray)
            {
                destinDt.Columns.Add(element);
                //将源DataTable中的那个列的数据类型复制到目标DataTable中的对应的列中
                if (sourcedt.Columns.Contains(element))
                {
                    destinDt.Columns[element].DataType = sourcedt.Columns[element].DataType;

                }
            }
            //复制数据
            copyData(sourcedt, destinDt);
            return destinDt;
        }


        private void copyData(DataTable sourcedt, DataTable destinDt)
        {
            //将源数据中的指定列复制到目标数据中
            DataRow resultRow;
            foreach (DataRow sourceRow in sourcedt.Rows)
            {
                resultRow = destinDt.NewRow(); //在目标DataTable上新增行
                foreach (DataColumn destinColumn in destinDt.Columns)
                {
                    //将源数据上的指定列复制到目标数据行中
                    resultRow[destinColumn.ColumnName] = sourceRow[destinColumn.ColumnName];
                }
                destinDt.Rows.Add(resultRow);
            }
        }

        private string getExpressSql(string fieldConfig, string siWhere)
        {
            //注：这边要加入A.EXPRESS='Y',否则会将非快递的数据查出来(已测试)

            //modify by turner 2013-07-10 因上面的sql语句有性能问题，这边做了一下调整
            string sql = string.Format(@"SELECT {0}
    FROM TKBILM M,TKBILD D, 
     (SELECT SHIPMENT_ID,E_I,SHIPPER_ID FROM DECL_SI A {1} AND A.EXPRESS = 'Y') T
     WHERE   D.SHIPMENT_ID IS NOT NULL
    AND   M.DN_NO = D.DN_NO
    AND INSTR (';' || D.SHIPMENT_ID || ';',
               ';' || T.SHIPMENT_ID || ';') > 0
       AND T.SHIPPER_ID = M.BILL_TO
     AND T.E_I = M.EX_IM", fieldConfig, siWhere);
            return sql;

        }

    }
}
