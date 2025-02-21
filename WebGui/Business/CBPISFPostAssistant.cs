using System;
using System.Collections.Generic;
using System.Text;
using Prolink.V6.Model;
using Prolink.DataOperation;

using Prolink.EDIBridge.Model;
using Prolink.Data;
using System.Data;
using WebGui.Controllers;

namespace Bussiness
{
    public class ISFMode
    {
        public static string ISF10 = "10";
        public static string ISF5 = "5";
    }
    /// <summary>
    /// CBPISF传输助手，用于产生传送到平台ISF XML文件
    /// </summary>
    public class CBPISFPostAssistant : BaseController
    {
        private string _userId = "";

        private OceanISFData _oceanISFData = new OceanISFData();
        private string _isfMode;
        private string sm_id;
        private string _mtSN = "";
        private string _act = "";
        private string _pwd = "";
        private string _tranType = "";
        private string _dlvName = "";
        private string _dlvCode = "";
        private string _billToCode = "";
        private string _cmp = "";
        private string _esrmark = "";
        private string _IbbkNo = "";

        /// <summary>
        /// 获取传送CBPISF数据
        /// </summary>
        /// <returns>用于传送CBPISF的数据</returns>
        public OceanISFData GetPostData(string ShipmentId,string UserId, out string _act, out string _pwd, string CmpSQL)
        {
            sm_id = ShipmentId;
            _userId = UserId;
            CreateData();
            _act = "";
            _pwd = "";

            try
            {
                string getImporterDataSql = string.Format("SELECT TOP 1 CD,CD_DESCP,AP_CD FROM BSCODE WHERE CD_DESCP = {0} AND CD_TYPE='IMT'", SQLUtils.QuotedStr(_cmp + "_" + _IbbkNo));
                DataTable imDt = OperationUtils.GetDataTable(getImporterDataSql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow imDr in imDt.Rows)
                {
                    _act = Prolink.Math.GetValueAsString(imDr["CD_DESCP"]);
                    _pwd = Prolink.Math.GetValueAsString(imDr["AP_CD"]);
                }
            }
            catch (Exception ex)
            {

                return null;
            }

            //TODO UPDATE IMPORTER ID INTO SMSMI NEW COLUM
            UpdateIMT2SMSM(ShipmentId, CmpSQL);
            return _oceanISFData;
        }

        public void UpdateIMT2SMSM(string ShipmentId, string CmpSQL)
        {
            string sql = string.Format(" UPDATE SMSMI SET IM_CD = {0} WHERE SHIPMENT_ID = {1} AND {2} ", SQLUtils.QuotedStr(_billToCode), SQLUtils.QuotedStr(ShipmentId), CmpSQL);

            try
            {
                int result = OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                
            }


        }


        private void CreateData()
        {
            string sql = "SELECT U_ID,CMP FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            string u_id = string.Empty;
            DataTable smsmidt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smsmidt.Rows.Count > 0)
            {
                _cmp = smsmidt.Rows[0]["CMP"].ToString();
                u_id = smsmidt.Rows[0]["U_ID"].ToString();
            }

            sql = @"SELECT *,(SELECT TOP 1 P.IM_RECORD FROM SMSMIPT S LEFT JOIN SMPTY P ON  S.PARTY_NO = P.PARTY_NO WHERE S.U_FID=" + SQLUtils.QuotedStr(u_id) + " AND S.PARTY_TYPE='RE' ) AS BOND_NO,"+
                "(SELECT TOP 1 C.MAPPING_CODE FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD = POD_CD ) AS POD_MAPCODE,(SELECT TOP 1 C.MAPPING_CODE FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD = DEST_CD ) AS DEST_MAPCODE " +
                "FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow dr = dt.Rows[0];
            _tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            
            string columns = "SHIPMENT_ID,PARTY_TYPE,PARTY_NO,PARTY_NAME,CNTY,CNTY_NM,PARTY_ADDR1,PARTY_ADDR2,PARTY_ADDR3,CITY AS CITY_CD,CITY_NM AS CITY,STATE,ZIP";
            string getBondSql = ",CASE WHEN PARTY_TYPE = 'CS' OR  PARTY_TYPE = 'RE' THEN (SELECT TOP 1 P.IM_RECORD FROM SMPTY AS P WHERE  P.PARTY_NO = SMSMIPT.PARTY_NO ) ELSE null END AS BOND_NO";
            getBondSql += ",CASE WHEN PARTY_TYPE = 'CS' OR  PARTY_TYPE = 'RE' THEN (SELECT TOP 1 P.IDENT FROM SMPTY AS P WHERE P.PARTY_NO = SMSMIPT.PARTY_NO ) ELSE null END AS IDENT";
            getBondSql += ",CASE WHEN PARTY_TYPE = 'RE' THEN (SELECT TOP 1 P.BOND_TYPE FROM SMPTY AS P WHERE P.PARTY_NO = SMSMIPT.PARTY_NO ) ELSE null END AS BOND_TYPE";
            getBondSql += ",CASE WHEN PARTY_TYPE = 'RE' THEN (SELECT TOP 1 P.BOND_ACT FROM SMPTY AS P WHERE P.PARTY_NO = SMSMIPT.PARTY_NO ) ELSE null END AS BOND_ACT";

            sql = "SELECT " + columns + getBondSql + " FROM SMSMIPT WHERE U_FID=" + SQLUtils.QuotedStr(u_id) + " AND PARTY_TYPE IN('CS','WE','SH','RE','FC','BO','SP','IBBK','SISF')";
            //sql += @"UNION ALL SELECT '" + sm_id + "' AS SHIPMENT_ID,'SL' AS PARTY_TYPE,ISF_SELLER AS PARTY_NO,ISF_SELLERNM AS PARTY_NAME,ISF_SELLERCNTY AS CNTY,"+
            //"ISF_SELLERCNTY_NM AS CNTY_NM,ISF_SELLERADDR1 AS PART_ADDR1,ISF_SELLERADDR2 AS PART_ADDR2,ISF_SELLERADDR3 AS PART_ADDR3,ISF_SELLERCITY AS CITY_CODE,ISF_SELLERCITY_NM AS CITY,ISF_SELLERSTATE AS STATE," +
            //"ISF_SELLERZIP AS ZIP,IM_RECORD AS BOND_NO,'' AS IDENT,'' AS BOND_TYPE,'' AS BOND_ACT FROM SMSIM WHERE PROFILE = (SELECT TOP 1 PROFILE_CD FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id) + ")";
            
            DataTable ptdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            //根据party档中的consignee的国别来判断，若为US则为ISF 10，否则为ISF 5
            foreach (DataRow ptdr in ptdt.Rows)
            {
                string PartyType = Prolink.Math.GetValueAsString(ptdr["PARTY_TYPE"]);
                string cntyCd = Prolink.Math.GetValueAsString(ptdr["CNTY"]);
                if (PartyType == "CS")
                {
                    if (cntyCd.Substring(0, 2) != "US")
                    {
                        _oceanISFData.PutNode("ISF_TYPE", "5");
                        _isfMode = "5";
                    }
                    else
                    {
                        _oceanISFData.PutNode("ISF_TYPE", "10");
                        _isfMode = "10";
                    }
                    //107550 Type是ISF-5的时候，place of Delivery目前是抓取party档中Consignee的city name和city code。现请修改成抓取party中consignne对应客户建档中的City name、5码code（city三码+国别两码）
                    string getCntySql = "SELECT TOP 1 CITY_NM AS DLV_NAME,CNTY+CITY AS DLV_CODE,CNTY,CITY FROM SMPTY WHERE PARTY_NO = " + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(ptdr["PARTY_NO"]));
                    DataTable cntyDt = OperationUtils.GetDataTable(getCntySql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow cntyDr in cntyDt.Rows)
                    {
                        _dlvName = Prolink.Math.GetValueAsString(cntyDr["DLV_NAME"]);
                        _dlvCode = Prolink.Math.GetValueAsString(cntyDr["DLV_CODE"]);
                        if ("5".Equals(_isfMode))
                        {
                            if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(cntyDr["CNTY"])))
                                throw new Exception("Consignee's Country Code is null!");
                            if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(cntyDr["CITY"])))
                                throw new Exception("Consignee's City Code is null!");
                        }
                    }
                }
                if (PartyType == "RE")
                {
                    _billToCode = Prolink.Math.GetValueAsString(ptdr["PARTY_NO"]);
                }
                if (PartyType == "IBBK")
                    _IbbkNo = Prolink.Math.GetValueAsString(ptdr["PARTY_NO"]);
                if (PartyType == "FC")
                    _esrmark = Prolink.Math.GetValueAsString(ptdr["PARTY_NO"]) +" "+ Prolink.Math.GetValueAsString(ptdr["PARTY_NAME"]);
            }

            if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["POD_MAPCODE"])) && _isfMode != "10")
                throw new Exception("POD AMS Code is null!");

            AddISF(dr);
            AddISFParty(ptdt, _tranType);
            AddISFPart(dr);
            AddISFHBL(dr);
        }

        private void AddISF(DataRow dr)
        {
            _oceanISFData.PutNode("UPLOAD_DATE", Prolink.Utils.FormatUtils.FormatDateTime(System.DateTime.Now, "yyyyMMddHHmmss"));
            _oceanISFData.PutNode("UPLOAD_USER_NAME", _userId);
            string pod = Prolink.Math.GetValueAsString(dr["POD_CD"]);
            string dest = Prolink.Math.GetValueAsString(dr["DEST_CD"]);

            _oceanISFData.PutNode("DATA_TYPE", "06");
            _oceanISFData.PutNode("DATA_NO", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
            _oceanISFData.PutNode("U_ID", Prolink.Math.GetValueAsString(dr["U_ID"]));
            _oceanISFData.PutNode("IF_GROUP_ID", Prolink.Math.GetValueAsString(dr["GROUP_ID"]));
            _oceanISFData.PutNode("IF_CMP", Prolink.Math.GetValueAsString(dr["CMP"]));
            _oceanISFData.PutNode("IF_STN", Prolink.Math.GetValueAsString(dr["STN"]));
            _oceanISFData.PutNode("DATA_FROM", "I");//标识从iFreight上传数据到平台
            _oceanISFData.PutNode("SHIPMENT_TYPE", "01");
            _oceanISFData.PutNode("ETD", Prolink.Utils.FormatUtils.FormatDateTime(Prolink.Math.GetValueAsDateTime(dr["ETD"]), "yyyyMMdd"));//回讯使用，实际并没有传送到美国海关 on 141215 by jam for 88888
            _oceanISFData.PutNode("ETA", Prolink.Utils.FormatUtils.FormatDateTime(Prolink.Math.GetValueAsDateTime(dr["ETA"]), "yyyyMMdd"));//回讯使用，实际并没有传送到美国海关 on 141215 by jam for 88888



            _oceanISFData.PutNode("POD_CD", Prolink.Math.GetValueAsString(dr["POD_MAPCODE"]));
            _oceanISFData.PutNode("POD_CRT", pod.Substring(0,2));
            _oceanISFData.PutNode("POD_NAME", Prolink.Math.GetValueAsString(dr["POD_NAME"]));
            _oceanISFData.PutNode("DLV_TYPE", "");
            _oceanISFData.PutNode("DLV_CD", dest.Substring(3,2));
            _oceanISFData.PutNode("DLV_CRT", dest.Substring(0,2));
           
            //传送ISF 5时，目前ISF 5的Place of Delivery栏位是抓取订舱的DEST栏位,请修改成抓取订舱中（公司类别=CS）Consignee对应客户建档中城市名称和五字代码（国别两码+城市三码）。
            if (_isfMode == "5")
            {
                _oceanISFData.PutNode("DLV_NAME", _dlvName);
                _oceanISFData.PutNode("DLV_CODE", _dlvCode);
            }
            else
            {
                _oceanISFData.PutNode("DLV_NAME", Prolink.Math.GetValueAsString(dr["DEST_NAME"]));
                _oceanISFData.PutNode("DLV_CODE", Prolink.Math.GetValueAsString(dr["DEST_MAPCODE"]));
            }
            
             
            _oceanISFData.PutNode("SURETY_CODE", "");

            string bondNo = Prolink.Math.GetValueAsString(dr["BOND_NO"]);
            _oceanISFData.PutNode("BOND_NO", "");
            _oceanISFData.PutNode("ID_NO", bondNo);
            _oceanISFData.PutNode("BOND_ID", bondNo);
            _oceanISFData.PutNode("SEND_USER", _userId);
            _oceanISFData.PutNode("SEND_DATE", Prolink.Utils.FormatUtils.FormatDateTime(DateTime.Now, "yyyyMMddHHmmss"));
            _oceanISFData.PutNode("CREATE_DATE", Prolink.Utils.FormatUtils.FormatDateTime(Prolink.Math.GetValueAsDateTime(dr["CREATE_DATE"]), "yyyyMMdd"));

            string uext = TrackingEDI.Business.BookingStatusManager.GetUserFxt(_userId);
            if (string.IsNullOrEmpty(uext)) uext = _userId;
            _oceanISFData.PutNode("CREATE_BY", uext);
            _oceanISFData.PutNode("MODIFY_DATE", Prolink.Utils.FormatUtils.FormatDateTime(Prolink.Math.GetValueAsDateTime(dr["MODIFY_DATE"]), "yyyyMMdd"));
            _oceanISFData.PutNode("MODIFY_BY", Prolink.Math.GetValueAsString(dr["MODIFY_BY"]));
            _oceanISFData.PutNode("ISF_MODE", 1);
            _oceanISFData.PutNode("IM_CD", _billToCode);
            _oceanISFData.PutNode("ES_REMARK", _esrmark);
            _oceanISFData.PutNode("DN_NO", Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]));
        }



        private void AddISFPart(DataRow dr)
        {

            OceanISFPartDataList dataList = (OceanISFPartDataList)_oceanISFData.GetChildren(typeof(OceanISFPartData).FullName, true);
            OceanISFPartData data = new OceanISFPartData();
            data.PutNode("PART_NO", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
            data.PutNode("JOB_NO", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
            data.PutNode("SEQ_NO", 1);

            data.PutNode("HTS", Prolink.Math.GetValueAsString(dr["HTS_CODE"], 6));
            data.PutNode("CTR_CODE", "CN");
            data.PutNode("CTR_NAME", "CHINA");
            data.PutNode("PARTY_TYPE", "MF");
            data.PutNode("PARTY_SN", _mtSN);

            dataList.Add(data);
        }

        private string SubStringAddr(string addr, int start, int length)
        {
            if (addr.Length > length + start)
                return addr.Substring(start, length).Trim();
            if (addr.Length > start)
                return addr.Substring(start).Trim();
            else return string.Empty;
        }

        private void AddISFParty(DataTable dt, string TranType)
        {
            OceanISFPartyDataList dataList = (OceanISFPartyDataList)_oceanISFData.GetChildren(typeof(OceanISFPartyData).FullName, true);
            int seq = 0;
            foreach (DataRow dr in dt.Rows)
            {
                
                OceanISFPartyData data = new OceanISFPartyData();
                data.PutNode("DATA_NO", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                string PartyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                Boolean isSend = true;
                data.PutNode("PARTY_SN", seq++);
                if(PartyType == "CS")
                {
                    data.PutNode("PARTY_TYPE", "CN");
                    data.PutNode("ID_TYPE", Prolink.Math.GetValueAsString(dr["IDENT"]));
                }
                else if(PartyType == "WE")
                {
                    data.PutNode("PARTY_TYPE", "ST");
                    data.PutNode("ID_TYPE", "");
                }
                else if(PartyType == "SISF")
                {
                    data.PutNode("PARTY_TYPE", "SE");
                    data.PutNode("ID_TYPE", "");
                }
                else if(PartyType == "SH")
                {
                    data.PutNode("PARTY_TYPE", "MF");
                    data.PutNode("ID_TYPE", "");
                    _mtSN = (seq-1).ToString();
                }
                else if (PartyType == "RE")
                {
                    data.PutNode("PARTY_TYPE", "IM");
                    data.PutNode("ID_TYPE", Prolink.Math.GetValueAsString(dr["IDENT"]));
                }
                else if (PartyType == "SP")
                {
                    isSend = false;
                }
                else if (PartyType == "BO" )
                {
                    isSend = false;
                }
                else
                {
                    continue;
                }

                if (isSend)
                {
                    if (PartyType == "CS" || PartyType == "RE")
                    {
                        data.PutNode("ID_NO", Prolink.Math.GetValueAsString(dr["BOND_NO"]));
                        data.PutNode("BOND_ID", Prolink.Math.GetValueAsString(dr["BOND_NO"]));
                    }
                    else
                    {
                        data.PutNode("ID_NO", Prolink.Math.GetValueAsString(dr["PARTY_NO"]));
                    }
                    data.PutNode("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"], 35));
                    data.PutNode("PARTY_CNTRY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                    data.PutNode("PARTY_CNTRYNAME", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                    string peraddr1 = Prolink.Math.GetValueAsString(dr["PARTY_ADDR1"]) +" "+ Prolink.Math.GetValueAsString(dr["PARTY_ADDR2"])
                        +" "+ Prolink.Math.GetValueAsString(dr["PARTY_ADDR3"]);
                    data.PutNode("PARTY_ADDR1", SubStringAddr(peraddr1, 0, 35));
                    data.PutNode("PARTY_ADDR2", SubStringAddr(peraddr1, 35, 35));
                    data.PutNode("PARTY_ADDR3", SubStringAddr(peraddr1, 70, 35));

                    data.PutNode("PARTY_CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                    data.PutNode("PARTY_STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                    data.PutNode("PARTY_ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                    data.PutNode("PS_COUNTRY", "");
                    data.PutNode("BIRTH_DATE", "");

                    if (PartyType == "RE")
                    {
                        data.PutNode("BOND_TYPE", Prolink.Math.GetValueAsString(dr["BOND_TYPE"]));
                        data.PutNode("BOND_ACT", Prolink.Math.GetValueAsString(dr["BOND_ACT"]));

                    }
                    dataList.Add(data);
                }

                if ((PartyType == "SH" && TranType == "F") || (PartyType == "SP" && TranType == "L") || (PartyType == "BO" && TranType == "L"))
                {
                    OceanISFPartyData data2 = new OceanISFPartyData();
                    data2.PutNode("PARTY_TYPE", "LG");
                    data2.PutNode("ID_TYPE", "");
                    data2.PutNode("PARTY_SN", seq++);
                    data2.PutNode("ID_NO", Prolink.Math.GetValueAsString(dr["PARTY_NO"]));
                    data2.PutNode("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                    data2.PutNode("PARTY_CNTRY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                    data2.PutNode("PARTY_CNTRYNAME", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                    string data2peraddr = Prolink.Math.GetValueAsString(dr["PARTY_ADDR1"]) + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR2"])
                        + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR3"]);
                    data2.PutNode("PARTY_ADDR1", SubStringAddr(data2peraddr, 0, 35));
                    data2.PutNode("PARTY_ADDR2", SubStringAddr(data2peraddr, 35, 35));
                    data2.PutNode("PARTY_ADDR3", SubStringAddr(data2peraddr, 70, 35));
                    data2.PutNode("PARTY_CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                    data2.PutNode("PARTY_STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                    data2.PutNode("PARTY_ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                    data.PutNode("PS_COUNTRY", "");
                    data.PutNode("BIRTH_DATE", "");

                    dataList.Add(data2);

                    OceanISFPartyData data3 = new OceanISFPartyData();
                    data3.PutNode("PARTY_TYPE", "CS");
                    data3.PutNode("ID_TYPE", "");
                    data3.PutNode("PARTY_SN", seq++);
                    data3.PutNode("ID_NO", Prolink.Math.GetValueAsString(dr["PARTY_NO"]));
                    data3.PutNode("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                    data3.PutNode("PARTY_CNTRY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                    data3.PutNode("PARTY_CNTRYNAME", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                    string data3peraddr = Prolink.Math.GetValueAsString(dr["PARTY_ADDR1"]) + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR2"])
                        + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR3"]);
                    data3.PutNode("PARTY_ADDR1", SubStringAddr(data3peraddr, 0, 35));
                    data3.PutNode("PARTY_ADDR2", SubStringAddr(data3peraddr, 35, 35));
                    data3.PutNode("PARTY_ADDR3", SubStringAddr(data3peraddr, 70, 35));
                    data3.PutNode("PARTY_CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                    data3.PutNode("PARTY_STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                    data3.PutNode("PARTY_ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                    data.PutNode("PS_COUNTRY", "");
                    data.PutNode("BIRTH_DATE", "");

                    dataList.Add(data3);

                }

                if (_isfMode.Equals("5") && ((PartyType == "SH" && TranType == "F") || ( TranType == "L" && (PartyType == "SP" || PartyType == "BO"))) )
                {
                    OceanISFPartyData data4 = new OceanISFPartyData();
                    data4.PutNode("PARTY_TYPE", "BKP");
                    data4.PutNode("ID_TYPE", "");
                    data4.PutNode("PARTY_SN", seq++);
                    data4.PutNode("ID_NO", Prolink.Math.GetValueAsString(dr["PARTY_NO"]));
                    data4.PutNode("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                    data4.PutNode("PARTY_CNTRY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                    data4.PutNode("PARTY_CNTRYNAME", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                    string data4peraddr = Prolink.Math.GetValueAsString(dr["PARTY_ADDR1"]) + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR2"])
                        + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR3"]);
                    data4.PutNode("PARTY_ADDR1", SubStringAddr(data4peraddr, 0, 35));
                    data4.PutNode("PARTY_ADDR2", SubStringAddr(data4peraddr, 35, 35));
                    data4.PutNode("PARTY_ADDR3", SubStringAddr(data4peraddr, 70, 35));
                    data4.PutNode("PARTY_CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                    data4.PutNode("PARTY_STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                    data4.PutNode("PARTY_ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                    data.PutNode("PS_COUNTRY", "");
                    data.PutNode("BIRTH_DATE", "");

                    dataList.Add(data4);
                }

                OceanISFPartyData data1 = new OceanISFPartyData();
                if (PartyType == "RE")
                {
                    data1.PutNode("PARTY_TYPE", "BY");
                    data1.PutNode("ID_TYPE", "");
                }
                else
                {
                    continue;
                }

                data1.PutNode("ID_NO", Prolink.Math.GetValueAsString(dr["PARTY_NO"]));
                data1.PutNode("PARTY_SN", seq++);
                data1.PutNode("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                data1.PutNode("PARTY_CNTRY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                data1.PutNode("PARTY_CNTRYNAME", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                string data1peraddr = Prolink.Math.GetValueAsString(dr["PARTY_ADDR1"]) + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR2"])
                        + " " + Prolink.Math.GetValueAsString(dr["PARTY_ADDR3"]);
                data1.PutNode("PARTY_ADDR1", SubStringAddr(data1peraddr, 0, 35));
                data1.PutNode("PARTY_ADDR2", SubStringAddr(data1peraddr, 35, 35));
                data1.PutNode("PARTY_ADDR3", SubStringAddr(data1peraddr, 70, 35));
                data1.PutNode("PARTY_CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                data1.PutNode("PARTY_STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                data1.PutNode("PARTY_ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                data.PutNode("PS_COUNTRY", "");
                data.PutNode("BIRTH_DATE", "");

                dataList.Add(data1);
            }
        }

        /// <summary>
        /// 获取ISF HBL数据
        /// </summary>
        private void AddISFHBL(DataRow dr)
        {

            OceanISFHouseDataList dataList = (OceanISFHouseDataList)_oceanISFData.GetChildren(typeof(OceanISFHouseData).FullName, true);
            OceanISFHouseData data = new OceanISFHouseData();
            data.PutNode("DATA_NO", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
            data.PutNode("BL_SN", 1);
            
            data.PutNode("SCAC_CODE", Prolink.Math.GetValueAsString(dr["SCAC_CD"]));
            string hblNo = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);
            string blType = "BM";
            if (hblNo == "")
            {
                hblNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                blType = "OB";
            }
            data.PutNode("BL_TYPE", blType);

            //检查Master BL的前四码是否与SCAC一致，如一样则Master BL前四码不传，否则就不用过滤直接传送。
            if (hblNo != "" && hblNo.IndexOf(Prolink.Math.GetValueAsString(dr["SCAC_CD"])) == 0)
            {
                hblNo = hblNo.Replace(Prolink.Math.GetValueAsString(dr["SCAC_CD"]), "");
            }
            data.PutNode("AMS_HBLNO", hblNo);

            data.PutNode("HBL_NO", sm_id);//回讯使用，实际并没有传送到美国海关 on 141215 by jam for 88888
            dataList.Add(data);
        }
    }
}