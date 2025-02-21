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
using TrackingEDI.Business;
using TrackingEDI.Mail;
using TrackingEDI.Utils;

namespace TrackingEDI.InboundBusiness
{
    public class InboundASDReportHelper
    {
        string templatefile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "download/InboundASDReport.xls");
        string ASDType = string.Empty;

        public InboundASDReportHelper(string asdtype)
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
                InboundASDReportLogic reportLogic = new InboundASDReportLogic(year, month, userinfo.GroupId, userinfo.CompanyId, ASDType);

                reportLogic.createDnRowExcel(sheet1);
                xlsFile = GetTemplatePath();
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

        public string GetTemplatePath()
        {
            string ext = ".xlsx"; //生成文件的扩展名
            string xlsFile = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], string.Format("UploadFiles/InboundCostReport/{0}{1}", System.Guid.NewGuid().ToString(), ext));
            string directoryPath = Path.GetDirectoryName(xlsFile);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return xlsFile;
        }
    }

    public class InboundASDReportLogic
    {
        string mappingName = "InboundASDReportMapping";
        string smdnpMappingName = "InboundASDReportSMDNPMapping";
        string bsCodeTypes = "'TCGT','TRGN','TNT','TCAR'";
        Dictionary<string, object> mapping = new Dictionary<string, object>();
        Dictionary<string, object> smdnpmapping = new Dictionary<string, object>();
        Dictionary<string, object> _fields = new Dictionary<string, object>();
        Dictionary<string, object> _smdnpfields = new Dictionary<string, object>();
        string sqlfilepath = Path.Combine(WebConfigurationManager.AppSettings["WEB_PATH"], "rpt/template/InboundAsdSql.xml");
        XmlDocument sqldoc = new XmlDocument();

        Dictionary<string, object> field = null;
        DataTable baseCodeDt = new DataTable();
        string[] shipmentids = null;
        string _groupid = string.Empty;
        string _cmp = string.Empty;
        string _asdType = string.Empty;
        int _year = 0;
        string _month = string.Empty;

        public InboundASDReportLogic(int year, string month, string groupid, string cmp, string asdtype)
        {
            mapping = XmlParser.GetMapping(mappingName, true);
            smdnpmapping = XmlParser.GetMapping(smdnpMappingName, true);
            _fields = XmlParser.GetFields(mapping);
            _smdnpfields = XmlParser.GetFields(smdnpmapping);
            field = null;
            string xml = new System.IO.StreamReader(sqlfilepath).ReadToEnd();
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("sqlfilepath:" + sqlfilepath);
            sqldoc.LoadXml(xml);
            _year = year;
            _month = month;
            shipmentids = null;
            _groupid = groupid;
            _cmp = cmp;
            _asdType = asdtype;
            baseCodeDt = MailTemplate.GetBaseData(bsCodeTypes, groupid, cmp);

            SetExcelColCodes(_fields);
            SetExcelColCodes(_smdnpfields);
        }

        public int createDnRowExcel(XSSFSheet sheet1)
        {
            int excelRowIndex = 1;
            string smsmsql = string.Format(@"SELECT DISTINCT SHIPMENT_ID,TRAN_TYPE,ETD FROM SMSMI WHERE YEAR(ETA)={0} AND MONTH(ETA)={1} AND CMP={2} ORDER BY TRAN_TYPE,ETD DESC ", _year, _month, SQLUtils.QuotedStr(_cmp));
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("createDnRowExcel" + smsmsql);
            DataTable smsmDt = Database.GetDataTable(smsmsql, null);
            DataView dv = smsmDt.DefaultView;
            DataTable shipmentDt = dv.ToTable(true, "SHIPMENT_ID");
            DataTable rateDt1 = Database.GetDataTable("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' ORDER BY EDATE", null);
            List<string> shipmentlist = new List<string>();
            foreach (DataRow dr in shipmentDt.Rows)
            {
                shipmentlist.Add(Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
            }
            shipmentids = shipmentlist.ToArray();
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("shipmentlist Count:" + shipmentlist.Count);
            if (shipmentlist.Count <= 0)
                return excelRowIndex;
            string smdnSql = string.Format(GetXmlDocSql("SMDN"), _year, _month, SQLUtils.QuotedStr(_cmp));
            DataTable smdnDt = Database.GetDataTable(smdnSql, null);
            Prolink.DataOperation.OperationUtils.Logger.WriteLog("smdnDt Count:" + smdnDt.Rows.Count);
            string smdnpSql = string.Empty;
            DataTable smdnpDt = new DataTable();
            #region 合并明细获取明细
            try
            {
                smdnpSql = string.Format(GetXmlDocSql("SMIDNP"), _year, _month, SQLUtils.QuotedStr(_cmp));
                smdnpDt = Database.GetDataTable(smdnpSql, null);
            }
            catch (Exception ex)
            {
                smdnpSql = "";
                throw ex;
            }
            #endregion

            DataTable mainDt = null;
            try
            {
                string mainsql = string.Format(@"SELECT *,(CASE WHEN CNT20 <>0 THEN CNT20 END) AS SH_CNT20,(CASE WHEN CNT40 <>0 THEN CNT40 END) AS SH_CNT40,(CASE WHEN CNT40HQ <>0 THEN CNT40HQ END) AS SH_CNT40HQ,(CASE WHEN CNT_NUMBER <>0 THEN CNT_NUMBER END) AS SH_CNT_NUMBER FROM SMSMI WHERE YEAR(ETA)={0} AND MONTH(ETA)={1} AND CMP={2} ORDER BY TRAN_TYPE,ETD DESC ", _year, _month, SQLUtils.QuotedStr(_cmp));
                mainDt = Database.GetDataTable(mainsql, null);
                if (!mainDt.Columns.Contains("MINCBM"))
                    mainDt.Columns.Add("MINCBM", typeof(decimal));
            }
            catch { }

            //if (mainDt != null)
            //{
            //    SetShipmentInfo(smdnDt, mainDt);
            //    SetShipmentInfo(smdnpDt, mainDt);
            //}

            int smdnRowCount = smdnDt.Rows.Count;

            DataTable smbidDt = Database.GetDataTable(string.Format("SELECT * FROM SMBID WHERE SHIPMENT_ID IN {0} AND (CMP={1} OR CMP NOT IN ('FQ','XM','BJ','QD','WH','BH','XY'))", SQLUtils.Quoted(shipmentids), SQLUtils.QuotedStr(_cmp)), null);

            string sql = string.Format(@"SELECT DISTINCT CHG_CD, CHG_CD+' '+CHG_DESCP CHGNAME FROM SMCHG WHERE IO_TYPE='I' AND GROUP_ID ={0} AND (CMP='*' OR CMP={1})", SQLUtils.QuotedStr(_groupid), SQLUtils.QuotedStr(_cmp));
            string constant = string.Format(@"UNION SELECT 'ZSUM' AS CHG_CD, 'Total Cost(USD)' AS CHGNAME
                                              UNION SELECT 'ZSUML' AS CHG_CD, 'Total Cost Local' AS CHGNAME
                                              ORDER BY CHG_CD");
            sql += constant;
            DataTable chgCodeDt = Database.GetDataTable(sql, null);

            string shipmentid = string.Empty;
            int titlecell = 0;
            for (int j = 0; j < _fields.Count; j++)
            {
                List<string> test = new List<string>(_fields.Keys);
                field = _fields[test[j]] as Dictionary<string, object>;
                string cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                int cellnum = ToIndex(cellcode);
                if (titlecell < cellnum)
                    titlecell = cellnum;
            }

            XSSFRow row = (XSSFRow)sheet1.GetRow(0);
            Dictionary<string, object> smbidfields = new Dictionary<string, object>();
            string chgcellcode = string.Empty;
            Dictionary<string, object> indexfields = new Dictionary<string, object>();
            string chgname = string.Empty;
            string chgcode = string.Empty;
            for (int j = 0, s = 0; j < chgCodeDt.Rows.Count; j++)
            {
                chgname = chgCodeDt.Rows[j]["CHGNAME"].ToString();
                chgcode = chgCodeDt.Rows[j]["CHG_CD"].ToString();
                if (smbidfields.ContainsKey(chgcode))
                    continue;
                indexfields = new Dictionary<string, object>();
                indexfields.Add("name", chgname);
                indexfields.Add("fieldname", chgcode);
                indexfields.Add("key", "False");
                indexfields.Add("defalutValue", "");
                indexfields.Add("dataType", "decimal");
                chgcellcode = excelColIndexToStr(s + titlecell + 1);
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

                if (mainDt != null)
                {
                    DataRow[] drs = mainDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)));
                    if (drs.Length > 0)
                    {
                        DataTable smDt = mainDt.Clone();
                        smDt.ImportRow(drs[0]);
                        DateTime billdate = GetBillDate(DateTime.Now, drs[0]);
                        DataTable billRateDT = CommonManager.GetRate(rateDt1, billdate);

                        excelRowIndex = createMasterExcelCellByRows(shipmentid, _fields, sheet1, smDt, smbidDt, chgCodeDt, excelRowIndex, smbidfields, billRateDT, true);
                    }
                }

                DataRow[] smdnrows = smdnDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)), "DN_NO DESC");
                DataTable smdnbySMDt = smdnDt.Clone();
                for (int i = 0; i < smdnrows.Length; i++)
                {
                    smdnbySMDt.ImportRow(smdnrows[i]);
                }
                if (smdnbySMDt.Rows.Count <= 0) continue;

                DateTime evendate = Prolink.Math.GetValueAsDateTime(smdnbySMDt.Rows[0]["ETD"]);
                if (smdnbySMDt.Rows[0]["ATD"] != null && smdnbySMDt.Rows[0]["ATD"] != DBNull.Value)
                {
                    evendate = Prolink.Math.GetValueAsDateTime(smdnbySMDt.Rows[0]["ATD"]);
                }
                Decimal mincbm = Prolink.Math.GetValueAsDecimal(smdnbySMDt.Rows[0]["MINCBM"]);

                DataTable rateDt = CommonManager.GetRate(rateDt1, evendate);
                excelRowIndex = createMasterExcelCellByRows(shipmentid, _fields, sheet1, smdnbySMDt, smbidDt, chgCodeDt, excelRowIndex, smbidfields, rateDt);

                DataRow[] smdnprows = smdnpDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)), "DN_NO DESC");
                DataTable smdnpbySMDt = smdnpDt.Clone();
                for (int i = 0; i < smdnprows.Length; i++)
                {
                    smdnpbySMDt.ImportRow(smdnprows[i]);
                }
                if (smdnpbySMDt.Rows.Count > 0)
                {
                    //设置细档的数据
                    excelRowIndex = createSubExcelCellByRows(shipmentid, _smdnpfields, sheet1, smdnpbySMDt, smbidDt, chgCodeDt, excelRowIndex, smbidfields, rateDt, mincbm);
                    //重新设置主表下一行的位置
                    excelRowIndex++;
                }
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("rowsindex" + s + "excelRowIndex:" + excelRowIndex);
            }
            return smdnRowCount;
        }

        /// <summary>
        /// 将shipment的信息设置到其他table中
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="mainDt"></param>
        private static void SetShipmentInfo(DataTable dt, DataTable mainDt)
        {
            string[] fileds = new string[] { "ISCOMBINE_BL" };
            foreach (string filed in fileds)
            {
                if (dt.Columns.Contains(filed))
                    continue;
                dt.Columns.Add(filed, typeof(string));
                dt.Columns[filed].MaxLength = mainDt.Columns[filed].MaxLength;
            }

            foreach (DataRow dr in dt.Rows)
            {
                DataRow[] drs = mainDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]))));
                if (drs == null || drs.Length <= 0)
                    continue;
                foreach (string filed in fileds)
                {
                    dr[filed] = drs[0][filed];
                }
            }
        }

        /// <summary>
        /// 设置excel的 cellCode
        /// </summary>
        /// <param name="fields"></param>
        private void SetExcelColCodes(Dictionary<string, object> fields)
        {
            int cellnum = 0;
            Dictionary<string, object> item = null;
            string[] keys = fields.Keys.ToArray();
            for (int i = 0; i < fields.Count; i++)
            {
                item = fields[keys[i]] as Dictionary<string, object>;
                string cellcode = excelColIndexToStr(cellnum);
                item["cellCode"] = cellcode;
                cellnum++;
            }
        }

        public string GetXmlDocSql(string xmlnodename)
        {
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
            string cellcode = string.Empty; int cellnum = 0;
            for (int j = 0; j < fieldCount; j++)
            {
                List<string> test = new List<string>(_fields.Keys);
                field = _fields[test[j]] as Dictionary<string, object>;

                fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                if (string.IsNullOrEmpty(fieldname))
                    continue;
                defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                cellnum = ToIndex(cellcode);
                if (cellnum <= 0) continue;
                XSSFCell XSSFcell = (XSSFCell)row.CreateCell(cellnum);

                if (string.IsNullOrEmpty(fieldname))
                    continue;
                try
                {
                    fieldValue = Prolink.Math.GetValueAsString(dt.Rows[dtRowIndex][fieldname]);
                }
                catch (Exception ex)
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
        private int createMasterExcelCellByRows(string shipmentid, Dictionary<string, object> mapfields, XSSFSheet sheet1, DataTable smdndt, DataTable smbidDt, DataTable chgCodeDt, int excelRowIndex, Dictionary<string, object> smindfields, DataTable ratedt, bool isShipment = false)
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
                    if (string.IsNullOrEmpty(fieldname))
                        continue;
                    defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                    dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    cellcode = Prolink.Math.GetValueAsString(field["cellCode"]);
                    cellnum = ToIndex(cellcode);
                    if (cellnum < 0) continue;
                    XSSFCell XSSFcell = (XSSFCell)row.CreateCell(cellnum);

                    try
                    {
                        fieldValue = GetBsCodeDescpByFieldName(fieldname, fieldValue, smdndt.Rows[i], true, isShipment);
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
                Decimal localsum = 0;
                Decimal usdsum = 0;
                string localcur = string.Empty;
                DataRow[] smbidrows = smbidDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)));
                DataTable smbidrowdt = smbidDt.Clone();
                foreach (DataRow dr in smbidrows)
                {
                    smbidrowdt.ImportRow(dr);
                    if (string.IsNullOrEmpty(localcur))
                        localcur = Prolink.Math.GetValueAsString(dr[cur]);
                }
                for (int j = 0; j < smindfields.Count; j++)
                {
                    cellnum++;
                    fieldValue = string.Empty;
                    List<string> test = new List<string>(smindfields.Keys);
                    field = smindfields[test[j]] as Dictionary<string, object>;

                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    if (string.IsNullOrEmpty(fieldname))
                        continue;
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
                        //Decimal percent = GetPerCent(dr);
                        Decimal percent = 1;
                        if (!isShipment)
                        {
                            Decimal mincbm = smdndt.Columns.Contains("MINCBM") ? Prolink.Math.GetValueAsDecimal(smdndt.Rows[i]["MINCBM"]) : 0M;
                            Decimal ttlcbm = Prolink.Math.GetValueAsDecimal(smdndt.Compute("Sum(CBM)", "true"));
                            Decimal cbm = Prolink.Math.GetValueAsDecimal(smdndt.Rows[i]["CBM"]);
                            if (mincbm <= 0)
                            {
                                ttlcbm = Prolink.Math.GetValueAsDecimal(smdndt.Compute("Sum(AMOUNT1)", "true"));
                                cbm = Prolink.Math.GetValueAsDecimal(smdndt.Rows[i]["AMOUNT1"]);
                            }

                            if (ttlcbm > 0)
                            {
                                percent = cbm / ttlcbm;
                            }
                        }

                        fieldValue = System.Math.Round(Prolink.Math.GetValueAsDouble(chgqty * percent), 2).ToString();
                        break;
                    }
                    
                    switch (fieldname)
                    {
                        case "ZSUM":
                            if (localsum > 0)
                            {
                                bool error = false;
                                usdsum = 0;
                                decimal rate = CommonManager.GetTotal(ratedt, null, localsum, localcur, ref usdsum, ref error, "USD");
                                fieldValue = System.Math.Round(Prolink.Math.GetValueAsDouble(usdsum), 2).ToString();
                            }
                            break;
                        case "ZSUML":
                            if (localsum > 0)
                                fieldValue = System.Math.Round(Prolink.Math.GetValueAsDouble(localsum), 2).ToString();
                            break;
                        default:
                            localsum += Prolink.Math.GetValueAsDecimal(fieldValue);
                            break;
                    }
                    formatCellValue(XSSFcell, dataType, fieldValue); //这边siDt.Rows[i][j]用的是索引

                }
                #endregion
                excelRowIndex = excelRowIndex + 1;
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
        private int createSubExcelCellByRows(string shipmentid, Dictionary<string, object> mapfields, XSSFSheet sheet1, DataTable smdnpDt, DataTable smbidDt, DataTable chgCodeDt, int excelRowIndex, Dictionary<string, object> smindfields, DataTable ratedt, Decimal mincbm = 0)
        {
            List<string> keys = XmlParser.GetKeys(mapping);
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            string cellcode = string.Empty; int cellnum = 0;
            int fieldCount = mapfields.Count;
            int dnpcount = smdnpDt.Rows.Count;
            string trantype = Prolink.Math.GetValueAsString(smdnpDt.Rows[0]["TRAN_TYPE"]);
            for (int i = 0; i < dnpcount; i++)
            {
                XSSFRow row = (XSSFRow)sheet1.CreateRow(excelRowIndex);
                for (int j = 0; j < fieldCount; j++)
                {
                    List<string> test = new List<string>(mapfields.Keys);
                    field = mapfields[test[j]] as Dictionary<string, object>;

                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    if (string.IsNullOrEmpty(fieldname))
                        continue;
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
                    if (string.IsNullOrEmpty(fieldname))
                        continue;
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
                        if (mincbm <= 0)
                        {
                            ttlcbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Compute("Sum(VALUE1)", "true"));
                            cbm = Prolink.Math.GetValueAsDecimal(smdnpDt.Rows[i]["VALUE1"]);
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


        public string GetBsCodeDescpByFieldName(string fieldname, string code, DataRow dr, bool IsMaster = true, bool isShipment = false)
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

            string iscombine_bl = dr.Table.Columns.Contains("ISCOMBINE_BL") ? Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"]) : string.Empty;

            switch (fieldname)
            {
                case "CBM":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return code;
                case "QTY":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return code;
                case "AMOUNT1":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return code;
                case "CW":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return code;
                case "GW":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return code;
                case "HTML_SCREEN_CNDNS":
                    if (isShipment)
                        return "BY SHIPMENT";
                    if (IsMaster) return "BY DN";
                    return "BY DN item";
                case "HTML_MONTH":
                    return MonthToEnglish(dr, IsMaster);
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
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT20"]) / 2) * GetPerCent(dr), 2).ToString();
                case "HTML_CNT40":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT40"])) * GetPerCent(dr), 2).ToString();
                case "HTML_CNT40HQ":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT40HQ"])) * GetPerCent(dr), 2).ToString();
                case "HTML_CNT_NUMBER":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    return fieldValue = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT_NUMBER"])) * GetPerCent(dr), 2).ToString();
                case "HTML_FEU":
                    if ("C".Equals(iscombine_bl))
                        return "0";
                    var htmlfeu = System.Math.Round((Prolink.Math.GetValueAsDecimal(dr["CNT20"]) / 2 + Prolink.Math.GetValueAsDecimal(dr["CNT40"]) + Prolink.Math.GetValueAsDecimal(dr["CNT40HQ"])) * GetPerCent(dr), 2);
                    fieldValue = htmlfeu.ToString();
                    break;
                //case "HTML_CNTRY":
                //    string region = Prolink.Math.GetValueAsString(dr["REGION"]);
                //    if ("CN".Equals(region) || "DB".Equals(region) || "HB".Equals(region) || "HD".Equals(region) ||
                //        "HN".Equals(region) || "HZ".Equals(region) || "XB".Equals(region) || "XN".Equals(region))
                //        return fieldValue = "China";
                //    string pod = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                //    if (pod.Length != 5) return string.Empty;
                //    string cntycd = pod.Substring(0, 2);
                //    return fieldValue = Database.GetValueAsString(string.Format("SELECT TOP 1 CNTRY_NM FROM BSCNTY WHERE CNTRY_CD={0}", SQLUtils.QuotedStr(cntycd)));
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

        public string MonthToEnglish(DataRow dr, bool ismaster)
        {
            DateTime etd = Prolink.Math.GetValueAsDateTime(dr["ETD"]);
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
                index += ((int)chars[i] - (int)'A' + 1) * (int)Math.Pow(26, chars.Length - i - 1);
            }
            return index - 1;
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
                XSSFCell XSSFcell = (XSSFCell)excelRow.CreateCell(excelColStartIndex);
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

        /// <summary>
        /// 获取账单日期
        /// </summary>
        /// <param name="billDate"></param>
        /// <param name="drs"></param>
        /// <returns></returns>
        public static DateTime GetBillDate(DateTime billDate, DataRow dr, bool haveATP = true)
        {
            string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            //if (haveATP && ("F".Equals(tranType) || "L".Equals(tranType))//如果Region是NA則用ATP回寫到費用管理
            //    && dr["ATP"] != null && dr["ATP"] != DBNull.Value &&
            //    "NA".Equals(Prolink.Math.GetValueAsString(dr["REGION"])))
            //    billDate = (DateTime)dr["ATP"];
            //else 
            if (dr["ATD"] != null && dr["ATD"] != DBNull.Value)//其他用ATD
                billDate = (DateTime)dr["ATD"];
            else if (dr["ETD"] != null && dr["ETD"] != DBNull.Value)
                billDate = (DateTime)dr["ETD"];
            else if (dr["DN_ETD"] != null && dr["DN_ETD"] != DBNull.Value)
                billDate = (DateTime)dr["DN_ETD"];
            return billDate;
        }
    }
}