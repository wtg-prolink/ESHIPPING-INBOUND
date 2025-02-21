using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackingEDI.Mail;
using TrackingEDI.Utils;

namespace TrackingEDI.InboundBusiness
{
    public class InboundHelper
    {
        static bool _isout { get; set; }
        #region 出口轉進口
        public static string O2IFunc(string ShipmentId, string elart = "", EditInstruct mei = null)
        {
            DelegateConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection();
            string sql = "SELECT STATUS FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            string s_status = getOneValueAsStringFromSql(sql);
            if ("E".Equals(elart) && s_status.Equals("A"))
            {
                return "The shipment can not send to inbound E-alert!";
            }
            if (s_status != "A" && s_status != "" && s_status != "S" && s_status != "E")
            {
                return "The shipment already existed in inbound!!";
            }
            Tuple<string, Dictionary<string, string>> isok = OToIFunc(ShipmentId, elart);
            if (mei != null && isok.Item2.Count > 0)
            {
                foreach (var datekey in isok.Item2)
                {
                    if (string.IsNullOrEmpty(datekey.Value))
                        mei.PutDate(datekey.Key, null);
                    else
                        mei.PutDate(datekey.Key, Prolink.Math.GetValueAsDateTime(datekey.Value));
                }
            }
            return isok.Item1;
        }

        public static Tuple<string, Dictionary<string, string>> OToIFunc(string ShipmentId, string elart = "", bool isout = false)
        {
            _isout = isout;
            string sql = string.Empty, muid = string.Empty;
            string podcd = string.Empty, carrier = string.Empty;
            string issplitbill = string.Empty;
            string is_ok = "N";
            bool IS_ISF_SENDING = false;
            Dictionary<string, string> smdic = new Dictionary<string, string>();
            try
            {
                DateTime EalertDate = DateTime.MinValue, EalertDateL = DateTime.MinValue, InboundDate = DateTime.MinValue, InboundDateL = DateTime.MinValue;
                DateTime eta = DateTime.MinValue, etd = DateTime.MinValue, atd = DateTime.MinValue, ata = DateTime.MinValue;
                DateTime asnDate = new DateTime(2000, 1, 1);
                string sh_location = string.Empty;
                string mVessel = string.Empty, mVoyage = string.Empty, destCode = string.Empty;
                string tran = string.Empty;
                string isCombineBl = string.Empty;
                sql = string.Format(@"SELECT SOLDTO_MMDHK,DN_NO,ISF_SEND_DATE,ETD,IS_OK_ISF,IORDER,IS_SPLIT_BILL,ISCOMBINE_BL,COMBINE_INFO,BL_CHECK,ATD,
COMBIN_SHIPMENT,SHIPMENT_INFO,U_ID,GROUP_ID,POD1,POD2,POD3,POD4,POD5,POD6,POD7,POD_CD,VESSEL1,VOYAGE1,VESSEL2,VOYAGE2,VESSEL3,VOYAGE3,
VESSEL4,VOYAGE4,VESSEL5,VOYAGE5,VESSEL6,VOYAGE6,VESSEL7,VOYAGE7,DEST_CD,PDEST_CD,CMP,TRAN_TYPE,HOUSE_NO,PARTIAL_FLAG,ETA,CARRIER,M_VESSEL,
M_VOYAGE,CNT20,CNT40,CNT40HQ,OCNT_NUMBER,CNT_NUMBER,IS_OSP,IMPORT_NO,STATUS,ATA,EALERT_DATE,EALERT_DATE_L,INBOUND_DATE,INBOUND_DATE_L FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                DataTable smsmdt = getDataTableFromSql(sql);
                DataTable ptdt = new DataTable();
                string lastdnno = string.Empty;
                string firstdnno = string.Empty;
                if (smsmdt.Rows.Count > 0)
                {
                    DataRow smdr = smsmdt.Rows[0];
                    string dnno = Prolink.Math.GetValueAsString(smdr["DN_NO"]);
                    string soldtommdhk = Prolink.Math.GetValueAsString(smdr["SOLDTO_MMDHK"]);
                    if ("Y".Equals(soldtommdhk))
                        return new Tuple<string, Dictionary<string, string>>(string.Format("The shipment can not send to inbound!", ShipmentId), smdic); 
                     refdninfo refdn= getRefDnData(dnno);
                    lastdnno = getLastDnData(refdn);
                    firstdnno = refdn.DN1;
                    isCombineBl = Prolink.Math.GetValueAsString(smdr["ISCOMBINE_BL"]);
                    if ("S".Equals(isCombineBl))
                        return new Tuple<string, Dictionary<string, string>>(string.Format("This Shipment {0} is Sub BL can't to send it!", ShipmentId), smdic);
                    if ("XMAVA".Equals(smdr["IS_OSP"].ToString()))
                        return new Tuple<string, Dictionary<string, string>>(string.Format("This Shipment {0} is XM AVA DN1 can't to send it!", ShipmentId), smdic);
                    tran = Prolink.Math.GetValueAsString(smdr["TRAN_TYPE"]);
                    carrier = Prolink.Math.GetValueAsString(smdr["CARRIER"]);
                    issplitbill = Prolink.Math.GetValueAsString(smdr["IS_SPLIT_BILL"]);
                    if ("C".Equals(issplitbill))
                        return new Tuple<string, Dictionary<string, string>>(string.Format("This Shipment {0} is Master Split BL, Can't to send it!", ShipmentId), smdic);
                    if (("F".Equals(tran) || "R".Equals(tran)) && string.IsNullOrEmpty(carrier))
                    {
                        return new Tuple<string, Dictionary<string, string>>(string.Format("This Shipment {0} Carrier is empty, Can't to send it!", ShipmentId), smdic);
                    }

                    sql = string.Format(@"SELECT PARTY_NO,(SELECT TOP 1 AP_CD FROM BSCODE WHERE CD_TYPE='CULO' 
            AND CD=SMSMPT.PARTY_NO AND (CMP='*' OR CMP={0})) AS LOCATION,PARTY_TYPE FROM SMSMPT WHERE SHIPMENT_ID={1}
                AND PARTY_TYPE IN ('CS','NT','WE','FC') ORDER BY ORDER_BY DESC", SQLUtils.QuotedStr(sh_location), SQLUtils.QuotedStr(ShipmentId));

                    ptdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    string msg = CheckInfos(ShipmentId, ref podcd, ref sh_location, issplitbill, smsmdt, elart, ptdt);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        string isokisf = Prolink.Math.GetValueAsString(smdr["IS_OK_ISF"]);
                        etd = (DateTime)smdr["ETD"];
                        string isfsendingdateString = "20180601";
                        DateTime isfsendingdate = DateTime.ParseExact(isfsendingdateString, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        if ("Y".Equals(smdr["IORDER"].ToString()))
                        {
                            if (isokisf == "Y" || (DateTime.Compare(etd, isfsendingdate) < 0)) //已经发送过到InboundISF sending状态下的资料下一次发送肯定就是Unreach下的判断了
                            {
                                return new Tuple<string, Dictionary<string, string>>(msg, smdic);
                            }
                            IS_ISF_SENDING = true;
                        }
                        else
                        {
                            if (!"E".Equals(elart))
                                return new Tuple<string, Dictionary<string, string>>(msg, smdic);
                        }
                    }
                    string podCd = Prolink.Math.GetValueAsString(smdr["POD_CD"]);
                    if ("F".Equals(tran) || "L".Equals(tran))
                    {
                        mVessel = Prolink.Math.GetValueAsString(smdr["M_VESSEL"]);
                        mVoyage = Prolink.Math.GetValueAsString(smdr["M_VOYAGE"]);
                        if (string.IsNullOrEmpty(mVessel))
                        {
                            for (int i = 1; i < 5; i++)
                            {
                                string location = Prolink.Math.GetValueAsString(smdr["POD" + i]);
                                if (string.IsNullOrEmpty(location))
                                    continue;
                                if (!string.IsNullOrEmpty(podCd) && (podCd.Equals(location) || podCd.Substring(0, 2).Equals(location.Substring(0, 2))))
                                {
                                    mVessel = Prolink.Math.GetValueAsString(smdr["VESSEL" + i]);
                                    mVoyage = Prolink.Math.GetValueAsString(smdr["VOYAGE" + i]);
                                }
                            }
                        }
                    }
                    if ("F".Equals(tran) || "L".Equals(tran) || "R".Equals(tran))
                        destCode = Prolink.Math.GetValueAsString(smdr["DEST_CD"]);
                    else
                        destCode = Prolink.Math.GetValueAsString(smdr["PDEST_CD"]);
                    eta = Prolink.Math.GetValueAsDateTime(smdr["ETA"]);
                    atd = Prolink.Math.GetValueAsDateTime(smdr["ATD"]);
                    ata = Prolink.Math.GetValueAsDateTime(smdr["ATA"]);
                    InboundDate = Prolink.Math.GetValueAsDateTime(smdr["INBOUND_DATE"]);
                    InboundDateL = Prolink.Math.GetValueAsDateTime(smdr["INBOUND_DATE_L"]);
                    EalertDate = Prolink.Math.GetValueAsDateTime(smdr["EALERT_DATE"]);
                    EalertDateL = Prolink.Math.GetValueAsDateTime(smdr["EALERT_DATE_L"]);
                }
                string Location = string.Empty;



                #region Resend to Outbound,Y：已Resend to Outbound,F:重新send to Inbound,空值：未操作过Resend to Outbound
                sql = string.Format("SELECT RESEND_FLAG FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                string resend = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                string resendFlag = string.Empty;
                if ("Y".Equals(resend.ToUpper()))
                {
                    resendFlag = "F";
                }
                #endregion

                MixedList mixList = new MixedList();


                mixList.Add(string.Format("DELETE FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)));
                mixList.Add(string.Format("DELETE FROM SMSMIPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)));
                mixList.Add(string.Format("DELETE FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)));
                mixList.Add(string.Format("DELETE FROM SMIDNP WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)));
                if ("T".Equals(tran))
                {
                    mixList.Add(string.Format("DELETE FROM SMORD WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(ShipmentId)));
                    mixList.Add(string.Format("DELETE FROM SMRDN WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(ShipmentId)));
                    mixList.Add(string.Format("DELETE FROM SMRCNTR WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(ShipmentId)));
                    mixList.Add(string.Format("DELETE FROM SMIRV WHERE SHIPMENT_INFO ={0}", SQLUtils.QuotedStr(ShipmentId)));
                }
                if ("C".Equals(isCombineBl))
                {
                    string con = string.Format(" IN(SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT =REPLACE({0},'MHK_','') AND STATUS='E')", SQLUtils.QuotedStr(ShipmentId));
                    mixList.Add(string.Format("DELETE FROM SMSMI WHERE SHIPMENT_ID {0}", con));
                    mixList.Add(string.Format("DELETE FROM SMSMIPT WHERE SHIPMENT_ID {0}", con));
                    mixList.Add(string.Format("DELETE FROM SMIDN WHERE SHIPMENT_ID {0}", con));
                    mixList.Add(string.Format("DELETE FROM SMIDNP WHERE SHIPMENT_ID {0}", con));
                    if ("T".Equals(tran))
                    {
                        mixList.Add(string.Format("DELETE FROM SMORD WHERE SHIPMENT_ID {0}", con));
                        mixList.Add(string.Format("DELETE FROM SMRDN WHERE SHIPMENT_ID {0}", con));
                        mixList.Add(string.Format("DELETE FROM SMRCNTR WHERE  SHIPMENT_ID {0}", con));
                        mixList.Add(string.Format("DELETE FROM SMIRV WHERE  SHIPMENT_ID {0}", con));
                    }
                }
                List<string> opartno = new List<string>();
                List<string> partnoList = new List<string>();
                List<string> PoNo = new List<string>();
                List<string> IhsCode = new List<string>();
                List<string> OhsCode = new List<string>();
                List<string> Resolution = new List<string>();
                //List<string> asnList = new List<string>();
                bool isY = true;
                List<int> qtyList = new List<int>();
                string subBgBu = "";
                //Dictionary<string, string> asnDic = new Dictionary<string, string>();
                List<string> asnDetailList = new List<string>();
                if (ptdt.Rows.Count > 0)
                {
                    bool haslocation = false;
                    string partyNos = string.Empty;
                    foreach (DataRow dr in ptdt.Rows)
                    {
                        if ("CS".Equals(Prolink.Math.GetValueAsString(dr["PARTY_TYPE"])) || "NT".Equals(Prolink.Math.GetValueAsString(dr["PARTY_TYPE"])))
                            partyNos += Prolink.Math.GetValueAsString(dr["PARTY_NO"]) + ",";
                        if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["LOCATION"])))
                        {
                            haslocation = true;
                        }
                    }
                    if (!haslocation)
                    {
                        return new Tuple<string, Dictionary<string, string>>(string.Format("{0} doesn't included in the Master code set up!", Prolink.Math.GetValueAsString(ptdt.Rows[0]["PARTY_NO"])), smdic);
                    }


                    foreach (DataRow dr in ptdt.Rows)
                    {
                        string PartyNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                        bool isDirect = false;
                        sql = "SELECT TOP 1 AP_CD FROM BSCODE WHERE CD_TYPE='CULO' AND CD={0} AND (CMP='*' OR CMP={1})";
                        sql = string.Format(sql, SQLUtils.QuotedStr(PartyNo), SQLUtils.QuotedStr(sh_location));
                        Location = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (string.IsNullOrEmpty(Location))
                            continue;
                        string secCmp = "";
                        if (!string.IsNullOrEmpty(partyNos))
                        {
                            string[] partyNo = partyNos.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            sql = string.Format("SELECT COUNT(*) C FROM DIRECT_MAP  WHERE PARTY_NO IN {0} AND (CMP={1} OR CMP='*')",
                                SQLUtils.Quoted(partyNo), SQLUtils.QuotedStr(Location));
                            int counts = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (counts > 0)
                                isDirect = true;
                        }
                        if (!string.IsNullOrEmpty(destCode) && !isDirect)
                        {
                            sql = string.Format("SELECT * FROM DEST_MAP WHERE DEST_CODE={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(destCode), SQLUtils.QuotedStr(Location));
                            DataTable maDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            string mapCmp = string.Empty;
                            foreach (DataRow maDr in maDt.Rows)
                            {
                                mapCmp = Prolink.Math.GetValueAsString(maDr["SEC_CMP"]);
                                if (!string.IsNullOrEmpty(mapCmp))
                                {
                                    secCmp = mapCmp;
                                    break;
                                }
                            }
                        }
                        Tuple<string, int, int, int, string, string, string, Tuple<string>> portFreeItem = new Tuple<string, int, int, int, string, string, string, Tuple<string>>("", 0, 0, 0, "", "", "", new Tuple<string>(""));
                        DateTime arvDate = ata > DateTime.MinValue ? ata : eta;
                        DateTime depDate = atd > DateTime.MinValue ? atd : etd;
                        if ("F".Equals(tran) || "R".Equals(tran))
                        {
                            string lspNo = GetForwarderNoBySm(ShipmentId);
                            portFreeItem = getFreeTime(lspNo, podcd, Location, carrier, tran, arvDate, depDate);
                        }
                        #region 转进口主档sql string
                        string DTColumn = string.Empty;//内贸订舱 新增转到inbound栏位
                        string batNo = "NULL";//内贸订舱 转批次号码到inbound
                        if ("T".Equals(tran))
                        {
                            DTColumn = ",DN_ETD,TRACK_WAY,CARGO_TYPE,PRODUCT_DATE,TRANSACTE_MODE,CAR_TYPE,CAR_QTY,CAR_QTY1,CAR_TYPE1,CAR_QTY2,CAR_TYPE2,STATE,REGION,PPOL_CD,PPOL_NAME,PDEST_CD,PDEST_NAME,BAND_CD,BAND_DESCP,PROFILE_CD,DN_NO_REF ";
                            batNo = "(SELECT TOP 1 BAT_NO FROM SMRV WHERE SMRV.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS BAT_NO";
                        }
                        string col = @"";
                        string POD_NAME = "ISNULL((SELECT TOP 1 PORT_NM2 FROM BSCITY WHERE CNTRY_CD+PORT_CD=SMSM.POD_CD),POD_NAME)";
                        string DEST_NAME = "ISNULL((SELECT TOP 1 PORT_NM2 FROM BSCITY WHERE CNTRY_CD+PORT_CD=SMSM.DEST_CD),DEST_NAME)";
                        if (_isout)
                        {
                            col = @"OCNT_TYPE,
                                    OCNT_NUMBER,";
                        }
                        sql = @"INSERT INTO SMSMI
                    (
                        U_ID,
                        GROUP_ID,
                        CMP,
						MONTH,
                        TRAN_TYPE,
                        SHIPMENT_ID,
                        STATUS,
                        COMBINE_INFO,
                        MARKS,
                        GOODS,
                        LGOODS,
                        BL_RMK,
                        BOOKING_INFO,
                        CUR,
                        GVALUE,
                        CNT20,
                        CNT40,
                        CNT40HQ,
                        CNT_TYPE,
                        CNT_NUMBER,
                        PKG_NUM,
                        PKG_UNIT,
                        PKG_UNIT_DESC,
                        GW,
                        GWU,
                        CBM,
                        WEEKLY,
                        YEAR,
                        TRADE_TERM,
                        TRADETERM_DESCP,
                        INCOTERM_CD,
                        INCOTERM_DESCP,
                        LOADING_FROM,
                        LOADING_TO,
                        CARRIER,
                        CARRIER_NM,
                        SCAC_CD,
                        FRT_TERM,
                        MASTER_NO,
                        HOUSE_NO,
                        SVC_CONTACT,
                        POR_CD,
                        POR_CNTY,
                        POR_NAME,
                        POL_CD,
                        POL_CNTY,
                        POL_NAME,
                        POD_CD,
                        POD_CNTY,
                        POD_NAME,
                        DEST_CD,
                        DEST_CNTY,
                        DEST_NAME,
                        POL_CD1,POD1,VESSEL1,VOYAGE1,ETD1,ETA1,ATD1,ATA1,
                        POL_CD2,POD2,VESSEL2,VOYAGE2,ETD2,ETA2,ATD2,ATA2,
                        POL_CD3,POD3,VESSEL3,VOYAGE3,ETD3,ETA3,ATD3,ATA3,
                        POL_CD4,POD4,VESSEL4,VOYAGE4,ETD4,ETA4,ATD4,ATA4,
                        POL_CD5,POD5,VESSEL5,VOYAGE5,ETD5,ETA5,ATD5,ATA5,
                        POL_CD6,POD6,VESSEL6,VOYAGE6,ETD6,ETA6,ATD6,ATA6,
                        POL_CD7,POD7,VESSEL7,VOYAGE7,ETD7,ETA7,ATD7,ATA7,
                        ATP,
                        ATD,
                        ATA,
                        ETA,ETA_L,
                        ETD,ETD_L,
                        DELAY_REASON,
                        DELAY_SOLUTION,
                        DELAY_REMARK,
                        QTY,
                        QTYU,
                        VW,
                        OF_COST,
                        TRUCK_COST,
                        CW,
                        HORN,
                        BATTERY,
                        OEXPORTER,
                        OEXPORTER_NM,
                        OEXPORTER_ADDR,
                        OIMPORTER,
                        OIMPORTER_NM,
                        OIMPORTER_ADDR,
                        CREATE_DATE,
                        CREATE_DATE_L,
                        CREATE_BY,
                        STN,
                        DEP,
                        DN_NO,
                        O_LOCATION,
                        CSTATUS,
                        INSPECTION,
                        O_UID,
                        TRAN_TYPE1,
                        POL1,
                        POL_NM1,
                        BOOKING_WIN,
                        SALSE_WIN,
                        VIA,
                        BRG_TYPE,
                        UNIQUE_NO,EALERT_DATE,EALERT_DATE_L,INBOUND_DATE,INBOUND_DATE_L,
                        ISF_WIN,ISF_SEND_DATE,IORDER,ISF_ETD,ISF_STATUS,ISF_TRANSMIT_DATE,FI_WIN,M_VESSEL,M_VOYAGE,SEC_CMP,PORT_FREE_TIME,FACT_FREE_TIME,CON_FREE_TIME,COMBINE_DET,RESEND_FLAG,BAT_NO
                        {13}
                    )
                    SELECT
                        {0}, 
                        GROUP_ID,
                        {1},
                        MONTH,
                        TRAN_TYPE,
                        SHIPMENT_ID,
                        'A',
                        COMBINE_INFO,
                        MARKS,
                        GOODS,
                        LGOODS,
                        BL_RMK,
                        BOOKING_INFO,
                        CUR,
                        GVALUE,
                        CNT20,
                        CNT40,
                        CNT40HQ,
                        CNT_TYPE,
                        CNT_NUMBER,
                        PKG_NUM,
                        PKG_UNIT,
                        PKG_UNIT_DESC,
                        GW,
                        GWU,
                        CBM,
                        WEEKLY,
                        YEAR,
                        TRADE_TERM,
                        TRADETERM_DESCP,
                        INCOTERM_CD,
                        INCOTERM_DESCP,
                        LOADING_FROM,
                        LOADING_TO,
                        CARRIER,
                        CARRIER_NM,
                        SCAC_CD,
                        FRT_TERM,
                        (CASE WHEN TRAN_TYPE='E' THEN HOUSE_NO ELSE MASTER_NO END) AS MASTER_NO,
                        HOUSE_NO,
                        SVC_CONTACT,
                        POR_CD,
                        POR_CNTY,
                        POR_NAME,
                        POL_CD,
                        POL_CNTY,
                        POL_NAME,
                        POD_CD,
                        POD_CNTY,
                        {16},
                        DEST_CD,
                        DEST_CNTY,
                        {17},
                        POL1,POD1,VESSEL1,VOYAGE1,ETD1,ETA1,ATD1,ATA1,
                        POL2,POD2,VESSEL2,VOYAGE2,ETD2,ETA2,ATD2,ATA2,
                        POL3,POD3,VESSEL3,VOYAGE3,ETD3,ETA3,ATD3,ATA3,
                        POL4,POD4,VESSEL4,VOYAGE4,ETD4,ETA4,ATD4,ATA4,
                        POL5,POD5,VESSEL5,VOYAGE5,ETD5,ETA5,ATD5,ATA5,
                        POL6,POD6,VESSEL6,VOYAGE6,ETD6,ETA6,ATD6,ATA6,
                        POL7,POD7,VESSEL7,VOYAGE7,ETD7,ETA7,ATD7,ATA7,
                        ATP,
                        ATD,
                        ATA, 
                        ETA,ETA_L,
                        ETD,ETD_L,
                        DELAY_REASON,
                        DELAY_SOLUTION,
                        DELAY_REMARK,
                        QTY,
                        QTYU,
                        VW,
                        OF_COST,
                        TRUCK_COST,
                        CW,
                        HORN,
                        BATTERY,
                        OEXPORTER,
                        OEXPORTER_NM,
                        OEXPORTER_ADDR,
                        OIMPORTER,
                        OIMPORTER_NM,
                        OIMPORTER_ADDR,
                        {2},
                        {3},
                        'Outbound',
                        STN,
                        DEP,
                        DN_NO,
                        CMP,
                        'N',
                        'N',
                        U_ID,
                        'T',
                        POD_CD,
                        (SELECT TOP 1 PORT_NM FROM BSTPORT WHERE PORT_CD=SMSM.POD_CD AND CMP={1}),
                        BL_WIN,
                        SALES_WIN,
                        VIA,
                        BRG_TYPE,
                        REFERENCE_NO,EALERT_DATE,EALERT_DATE_L,INBOUND_DATE,INBOUND_DATE_L,
                        ISF_WIN,ISF_SEND_DATE,IORDER,ISF_ETD,ISF_STATUS,ISF_TRANSMIT_DATE,FI_WIN,{5},{6},{7},{8},{9},{10},{11},{12},{14}{13}
                    FROM SMSM WHERE SHIPMENT_ID={4}";
                        muid = System.Guid.NewGuid().ToString();
                        DateTime odt = DateTime.Now;
                        string CompanyId = Location;
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                        sql = string.Format(sql, SQLUtils.QuotedStr(muid), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")),
                            SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ShipmentId),
                            string.IsNullOrEmpty(mVessel) ? "null" : SQLUtils.QuotedStr(mVessel), string.IsNullOrEmpty(mVoyage) ? "null" : SQLUtils.QuotedStr(mVoyage),
                            string.IsNullOrEmpty(secCmp) ? "null" : SQLUtils.QuotedStr(secCmp), portFreeItem.Item4, portFreeItem.Item3, portFreeItem.Item2,
                            SQLUtils.QuotedStr(portFreeItem.Item1), SQLUtils.QuotedStr(resendFlag), DTColumn, batNo, col, POD_NAME, DEST_NAME);
                        //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        mixList.Add(sql);
                        #endregion

                        #region 转进口Party档

                        string ptcol = @"PARTY_TYPE,
                                        TYPE_DESCP,
                                        ORDER_BY,
                                        PARTY_NO,
                                        PARTY_NAME,
                                        PARTY_NAME2,
                                        PARTY_NAME3,
                                        PARTY_NAME4,
                                        PART_ADDR,
                                        PART_ADDR2,
                                        PART_ADDR3,
                                        PART_ADDR4,
                                        PART_ADDR5,
                                        CNTY,
                                        CNTY_NM,
                                        STATE,
                                        ZIP,
                                        CONTACT,
                                        TEL, 
                                        MAIL,
                                        FAX_NO,
                                        TAX_NO";
                        string smptcol = @"PARTY_TYPE,
                                        TYPE_DESCP,
                                        ORDER_BY,
                                        PARTY_NO,
                                        PARTY_NAME,
                                        PARTY_NAME2,
                                        PARTY_NAME3,
                                        PARTY_NAME4,
                                        PART_ADDR1 AS PART_ADDR,
                                        PART_ADDR2,
                                        PART_ADDR3,
                                        PART_ADDR4,
                                        PART_ADDR5,
                                        CNTY,
                                        CNTY_NM,
                                        STATE,
                                        ZIP,
                                        PARTY_ATTN AS CONTACT, 
                                        PARTY_TEL AS TEL, 
                                        PARTY_MAIL AS MAIL, 
                                        FAX_NO,
                                        TAX_NO";
                        List<string> pttype = new List<string>() { "FS", "SP", "BO", "BR", "CR", "RSP" };
                        sql = string.Format(@"SELECT {3} FROM SMDNPT WHERE DN_NO ={1} AND PARTY_TYPE NOT IN {0}
                            UNION 
                            SELECT {4} FROM SMSMPT WHERE SHIPMENT_ID IN (SELECT TOP 1 SHIPMENT_ID FROM SMDN WHERE DN_NO ={2}) AND PARTY_TYPE IN {0}", SQLUtils.Quoted(pttype.ToArray()), SQLUtils.QuotedStr(lastdnno)
                            , SQLUtils.QuotedStr(firstdnno), ptcol, smptcol);

                        DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


                        if (dt.Rows.Count <= 0)
                        {
                            if ("T".Equals(tran))//内贸资料传送inbound，新增party type为CR资料
                            {
                                sql = $"SELECT {smptcol} FROM SMSMPT WHERE SHIPMENT_ID={SQLUtils.QuotedStr(ShipmentId)} AND PARTY_TYPE IN('FS','BO','AG','BC','CS','FC','HUB','LC','NT','RG','RO','RE','SB','SH','SL','SP','WE','Z1','Z2','Z3','Z4','Z5','Z6','ZC','ZE','ZS','RSP','LF','CR')";
                            }
                            else
                            {
                                sql = $"SELECT {smptcol} FROM SMSMPT WHERE SHIPMENT_ID={SQLUtils.QuotedStr(ShipmentId)} AND PARTY_TYPE IN('FS','BO','AG','BC','CS','FC','HUB','LC','NT','RG','RO','RE','SB','SH','SL','SP','WE','Z1','Z2','Z3','Z4','Z5','Z6','ZC','ZE','ZS','RSP','LF')";
                            }
                            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }

                        foreach (DataRow item in dt.Rows)
                        {
                            string PARTY_TYPE = Prolink.Math.GetValueAsString(item["PARTY_TYPE"]);
                            string TYPE_DESCP = Prolink.Math.GetValueAsString(item["TYPE_DESCP"]);
                            string ORDER_BY = Prolink.Math.GetValueAsString(item["ORDER_BY"]);
                            string PARTY_NO = Prolink.Math.GetValueAsString(item["PARTY_NO"]);
                            string PARTY_NAME = Prolink.Math.GetValueAsString(item["PARTY_NAME"]);
                            string PARTY_NAME2 = Prolink.Math.GetValueAsString(item["PARTY_NAME2"]);
                            string PARTY_NAME3 = Prolink.Math.GetValueAsString(item["PARTY_NAME3"]);
                            string PARTY_NAME4 = Prolink.Math.GetValueAsString(item["PARTY_NAME4"]);
                            string PARTY_ADDR1 = Prolink.Math.GetValueAsString(item["PART_ADDR"]);//Prolink.Math.GetValueAsString(item["PART_ADDR1"]);
                            string PARTY_ADDR2 = Prolink.Math.GetValueAsString(item["PART_ADDR2"]);
                            string PARTY_ADDR3 = Prolink.Math.GetValueAsString(item["PART_ADDR3"]);
                            string PARTY_ADDR4 = Prolink.Math.GetValueAsString(item["PART_ADDR4"]);
                            string PARTY_ADDR5 = Prolink.Math.GetValueAsString(item["PART_ADDR5"]);
                            string CNTY = Prolink.Math.GetValueAsString(item["CNTY"]);
                            string CNTY_NM = Prolink.Math.GetValueAsString(item["CNTY_NM"]);
                            string STATE = Prolink.Math.GetValueAsString(item["STATE"]);
                            string ZIP = Prolink.Math.GetValueAsString(item["ZIP"]);
                            string PARTY_ATTN = Prolink.Math.GetValueAsString(item["CONTACT"]); //Prolink.Math.GetValueAsString(item["PARTY_ATTN"]); 
                            string PARTY_TEL = Prolink.Math.GetValueAsString(item["TEL"]); //Prolink.Math.GetValueAsString(item["PARTY_TEL"]);
                            string PARTY_MAIL = Prolink.Math.GetValueAsString(item["MAIL"]); //Prolink.Math.GetValueAsString(item["PARTY_MAIL"]);
                            string DEBIT_TO = "";//Prolink.Math.GetValueAsString(item["DEBIT_TO"]);
                            string FAX_NO = Prolink.Math.GetValueAsString(item["FAX_NO"]);
                            string TAX_NO = Prolink.Math.GetValueAsString(item["TAX_NO"]);
                            string duid = System.Guid.NewGuid().ToString();
                            string str = SQLUtils.QuotedStr(duid) + "," + SQLUtils.QuotedStr(muid) + "," + SQLUtils.QuotedStr(ShipmentId) + "," + SQLUtils.QuotedStr(PARTY_TYPE) + "," + SQLUtils.QuotedStr(TYPE_DESCP) + "," + SQLUtils.QuotedStr(ORDER_BY) + "," + SQLUtils.QuotedStr(PARTY_NO) + "," + SQLUtils.QuotedStr(PARTY_NAME) + "," + SQLUtils.QuotedStr(PARTY_NAME2) + "," + SQLUtils.QuotedStr(PARTY_NAME3) + "," + SQLUtils.QuotedStr(PARTY_NAME4) + "," + SQLUtils.QuotedStr(PARTY_ADDR1) + "," + SQLUtils.QuotedStr(PARTY_ADDR2) + "," + SQLUtils.QuotedStr(PARTY_ADDR3) + "," + SQLUtils.QuotedStr(PARTY_ADDR4) + "," + SQLUtils.QuotedStr(PARTY_ADDR5) + "," + SQLUtils.QuotedStr(CNTY) + "," + SQLUtils.QuotedStr(CNTY_NM) + "," + SQLUtils.QuotedStr(STATE) + "," + SQLUtils.QuotedStr(ZIP) + "," + SQLUtils.QuotedStr(PARTY_ATTN) + "," + SQLUtils.QuotedStr(PARTY_TEL) + "," + SQLUtils.QuotedStr(PARTY_MAIL) + "," + SQLUtils.QuotedStr(DEBIT_TO) + "," + SQLUtils.QuotedStr(FAX_NO) + "," + SQLUtils.QuotedStr(TAX_NO);
                            sql = @"INSERT INTO SMSMIPT( 
                                    U_ID,
                                    U_FID,
                                    SHIPMENT_ID,
                                    PARTY_TYPE,
                                    TYPE_DESCP,
                                    ORDER_BY,
                                    PARTY_NO,
                                    PARTY_NAME,
                                    PARTY_NAME2,
                                    PARTY_NAME3,
                                    PARTY_NAME4,
                                    PARTY_ADDR1,
                                    PARTY_ADDR2,
                                    PARTY_ADDR3,
                                    PARTY_ADDR4,
                                    PARTY_ADDR5,
                                    CNTY,
                                    CNTY_NM,
                                    STATE,
                                    ZIP,
                                    PARTY_ATTN,
                                    PARTY_TEL,
                                    PARTY_MAIL,
                                    DEBIT_TO,
                                    FAX_NO,
                                    TAX_NO) VALUES ({0})";
                            sql = string.Format(sql, str);
                            //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            mixList.Add(sql);
                        }
                        #endregion

                        #region 轉進口DN
                        //string GetDNSql = "SELECT DN_NO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        //string DnNo = OperationUtils.GetValueAsString(GetDNSql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        //sql = "SELECT * FROM SMDNPT WHERE DN_NO=" + SQLUtils.QuotedStr(lastdnno) + " AND PARTY_TYPE IN('ZT')";
                        //DataTable Dndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                        //if (Dndt.Rows.Count > 0)
                        //{
                        //    foreach (DataRow item in Dndt.Rows)
                        //    {
                        //        string PARTY_TYPE = Prolink.Math.GetValueAsString(item["PARTY_TYPE"]);
                        //        string TYPE_DESCP = Prolink.Math.GetValueAsString(item["TYPE_DESCP"]);
                        //        string ORDER_BY = Prolink.Math.GetValueAsString(item["ORDER_BY"]);
                        //        string PARTY_NO = Prolink.Math.GetValueAsString(item["PARTY_NO"]);
                        //        string PARTY_NAME = Prolink.Math.GetValueAsString(item["PARTY_NAME"]);
                        //        string PARTY_NAME2 = Prolink.Math.GetValueAsString(item["PARTY_NAME2"]);
                        //        string PARTY_NAME3 = Prolink.Math.GetValueAsString(item["PARTY_NAME3"]);
                        //        string PARTY_NAME4 = Prolink.Math.GetValueAsString(item["PARTY_NAME4"]);
                        //        string PARTY_ADDR1 = Prolink.Math.GetValueAsString(item["PART_ADDR"]);
                        //        string PARTY_ADDR2 = Prolink.Math.GetValueAsString(item["PART_ADDR2"]);
                        //        string PARTY_ADDR3 = Prolink.Math.GetValueAsString(item["PART_ADDR3"]);
                        //        string PARTY_ADDR4 = Prolink.Math.GetValueAsString(item["PART_ADDR4"]);
                        //        string PARTY_ADDR5 = Prolink.Math.GetValueAsString(item["PART_ADDR5"]);
                        //        subBgBu = getPartyColByNo("DEP", PARTY_NO);
                        //        string CNTY = Prolink.Math.GetValueAsString(item["CNTY"]);
                        //        string CNTY_NM = Prolink.Math.GetValueAsString(item["CNTY_NM"]);
                        //        string STATE = Prolink.Math.GetValueAsString(item["STATE"]);
                        //        string ZIP = Prolink.Math.GetValueAsString(item["ZIP"]);
                        //        string PARTY_ATTN = Prolink.Math.GetValueAsString(item["PARTY_ATTN"]);
                        //        string PARTY_TEL = Prolink.Math.GetValueAsString(item["TEL"]);
                        //        string PARTY_MAIL = Prolink.Math.GetValueAsString(item["MAIL"]);
                        //        //string DEBIT_TO = Prolink.Math.GetValueAsString(item["DEBIT_TO"]);
                        //        string FAX_NO = Prolink.Math.GetValueAsString(item["FAX_NO"]);
                        //        string TAX_NO = Prolink.Math.GetValueAsString(item["TAX_NO"]);
                        //        string duid = System.Guid.NewGuid().ToString();
                        //        string str = SQLUtils.QuotedStr(duid) + "," + SQLUtils.QuotedStr(muid) + "," + SQLUtils.QuotedStr(ShipmentId) + "," + SQLUtils.QuotedStr(PARTY_TYPE) + "," + SQLUtils.QuotedStr(TYPE_DESCP) + "," + SQLUtils.QuotedStr(ORDER_BY) + "," + SQLUtils.QuotedStr(PARTY_NO) + "," + SQLUtils.QuotedStr(PARTY_NAME) + "," + SQLUtils.QuotedStr(PARTY_NAME2) + "," + SQLUtils.QuotedStr(PARTY_NAME3) + "," + SQLUtils.QuotedStr(PARTY_NAME4) + "," + SQLUtils.QuotedStr(PARTY_ADDR1) + "," + SQLUtils.QuotedStr(PARTY_ADDR2) + "," + SQLUtils.QuotedStr(PARTY_ADDR3) + "," + SQLUtils.QuotedStr(PARTY_ADDR4) + "," + SQLUtils.QuotedStr(PARTY_ADDR5) + "," + SQLUtils.QuotedStr(CNTY) + "," + SQLUtils.QuotedStr(CNTY_NM) + "," + SQLUtils.QuotedStr(STATE) + "," + SQLUtils.QuotedStr(ZIP) + "," + SQLUtils.QuotedStr(PARTY_ATTN) + "," + SQLUtils.QuotedStr(PARTY_TEL) + "," + SQLUtils.QuotedStr(PARTY_MAIL) + "," + SQLUtils.QuotedStr(FAX_NO) + "," + SQLUtils.QuotedStr(TAX_NO);
                        //        sql = @"INSERT INTO SMSMIPT( 
                        //            U_ID,
                        //            U_FID,
                        //            SHIPMENT_ID,
                        //            PARTY_TYPE,
                        //            TYPE_DESCP,
                        //            ORDER_BY,
                        //            PARTY_NO,
                        //            PARTY_NAME,
                        //            PARTY_NAME2,
                        //            PARTY_NAME3,
                        //            PARTY_NAME4,
                        //            PARTY_ADDR1,
                        //            PARTY_ADDR2,
                        //            PARTY_ADDR3,
                        //            PARTY_ADDR4,
                        //            PARTY_ADDR5,
                        //            CNTY,
                        //            CNTY_NM,
                        //            STATE,
                        //            ZIP,
                        //            PARTY_ATTN,
                        //            PARTY_TEL,
                        //            PARTY_MAIL,
                        //            FAX_NO,
                        //            TAX_NO) VALUES ({0})";
                        //        sql = string.Format(sql, str);
                        //        //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //        mixList.Add(sql);
                        //    }
                        //}
                        #endregion

                        #region 轉進口貨況
                        sql = "DELETE FROM TKBL WHERE SHIPMENT_ID={0} AND CMP={1}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(Location));
                        OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        sql = @"INSERT INTO TKBL
                    (
                        U_ID,
                        GROUP_ID,
                        CMP,
                        STN,
                        DEP,
                        SHIPMENT_ID,
                        PSTATUS,
                        CSTATUS,
                        FINAL_STATUS,
                        TRAN_TYPE,
                        CARRIER_CD,
                        CARRIER_NM,
                        HOUSE_NO,
                        MASTER_NO,
                        REF_NO,
                        LOT_NO,
                        BOOKING_NO,
                        BL_TYPE,
                        BL_DATE,
                        CLS_DATE,
                        INCOTERM,
                        INCOTERM_DESCP,
                        ETA,
                        ETD,
                        OBD_DATE,
                        ARR_DATE,
                        SO_NO,
                        VESSEL1,
                        VOYAGE1,
                        VESSEL2,
                        VOYAGE2,
                        VESSEL3,
                        VOYAGE3,
                        VESSEL,
                        VOYAGE,
                        MVESSEL,
                        MVOYAGE,
                        POR_CD,
                        POR_CNTY,
                        POR_NAME,
                        POL_CD,
                        POL_CNTY,
                        POL_NAME,
                        VIA_CD,
                        VIA_CNTY,
                        VIA_NAME,
                        POD_CD,
                        POD_CNTY,
                        POD_NAME,
                        ROUTE_CD,
                        ROUTE_CNTY,
                        ROUTE_NAME,
                        DEST_CD,
                        DEST_CNTY,
                        DEST_NAME,
                        DLV_CD,
                        DLV_CNTY,
                        DLV_NAME,
                        LOADING_FROM,
                        LOADING_TO,
                        QTY,
                        QTYU,
                        PKG_DESCP,
                        GW,
                        GWU,
                        CBM,
                        CNT20,
                        CNT40,
                        CNT40HQ,
                        CNT_TYPE,
                        CNT_NUMBER,{4}
                        CUR,
                        FRT_PC,
                        OTH_PC,
                        GOODS,
                        MARKS,
                        PICKUP_PORT,
                        PICKUP_NM,
                        REGION,
                        STATE,
                        DELIVERY_PORT,
                        DELIVERY_NM,
                        TRUCK_TYPE,
                        DRIVER,
                        DRIVER_TEL,
                        TRUCK_NO,
                        LSP_NO,
                        LSP_NM,
                        CREATE_BY,
                        CREATE_DATE,
                        MODIFY_BY,
                        MODIFY_DATE,
                        REMARK,
                        SCAC_CD,
                        ETD1,
                        ETA1,
                        ETD2,
                        ETA2,
                        ETD3,
                        ETA3,
                        TRACE_FLAG,
                        TRANS_FLAG,
                        VW,
                        CWU,
                        COMMODITY,
                        MPOL,
                        ISF_NO,
                        SHIPPING_DATE,
                        PKG,
                        PKGU,
                        VOYAGE4,
                        VESSEL4,
                        SH_CD,
                        SH_NM,
                        CS_CD,
                        CS_NM,
                        SP_CD,
                        SP_NM,
                        AG_CD,
                        AG_NM,
                        FC_CD,
                        FC_NM,
                        RELEASE_NO,
                        CNTR_NO,
                        C_MASTER_NO,
                        C_HOUSE_NO,
                        ISCOMBINE_BL,
                        COMBIN_SHIPMENT,
                        ISF_SEND_DATE,
                        IORDER,
                        O_UID,
                        O_LOCATION   
                    )
                    SELECT TOP 1
                        {0},
                        GROUP_ID,
                        {1},
                        STN,
                        DEP,
                        SHIPMENT_ID,
                        PSTATUS,
                        CSTATUS,
                        FINAL_STATUS,
                        TRAN_TYPE,
                        CARRIER_CD,
                        CARRIER_NM,
                        HOUSE_NO,
                        MASTER_NO,
                        REF_NO,
                        LOT_NO,
                        BOOKING_NO,
                        BL_TYPE,
                        BL_DATE,
                        CLS_DATE,
                        INCOTERM,
                        INCOTERM_DESCP,
                        ETA,
                        ETD,
                        OBD_DATE,
                        ARR_DATE,
                        SO_NO,
                        VESSEL1,
                        VOYAGE1,
                        VESSEL2,
                        VOYAGE2,
                        VESSEL3,
                        VOYAGE3,
                        VESSEL,
                        VOYAGE,
                        MVESSEL,
                        MVOYAGE,
                        POR_CD,
                        POR_CNTY,
                        POR_NAME,
                        POL_CD,
                        POL_CNTY,
                        POL_NAME,
                        VIA_CD,
                        VIA_CNTY,
                        VIA_NAME,
                        POD_CD,
                        POD_CNTY,
                        POD_NAME,
                        ROUTE_CD,
                        ROUTE_CNTY,
                        ROUTE_NAME,
                        DEST_CD,
                        DEST_CNTY,
                        DEST_NAME,
                        DLV_CD,
                        DLV_CNTY,
                        DLV_NAME,
                        LOADING_FROM,
                        LOADING_TO,
                        QTY,
                        QTYU,
                        PKG_DESCP,
                        GW,
                        GWU,
                        CBM,
                        CNT20,
                        CNT40,
                        CNT40HQ,
                        CNT_TYPE,
                        CNT_NUMBER,{4}
                        CUR,
                        FRT_PC,
                        OTH_PC,
                        GOODS,
                        MARKS,
                        PICKUP_PORT,
                        PICKUP_NM,
                        REGION,
                        STATE,
                        DELIVERY_PORT,
                        DELIVERY_NM,
                        TRUCK_TYPE,
                        DRIVER,
                        DRIVER_TEL,
                        TRUCK_NO,
                        LSP_NO,
                        LSP_NM,
                        CREATE_BY,
                        {2},
                        MODIFY_BY,
                        MODIFY_DATE,
                        REMARK,
                        SCAC_CD,
                        ETD1,
                        ETA1,
                        ETD2,
                        ETA2,
                        ETD3,
                        ETA3,
                        TRACE_FLAG,
                        TRANS_FLAG,
                        VW,
                        CWU,
                        COMMODITY,
                        MPOL,
                        ISF_NO,
                        SHIPPING_DATE,
                        PKG,
                        PKGU,
                        VOYAGE4,
                        VESSEL4,
                        SH_CD,
                        SH_NM,
                        CS_CD,
                        CS_NM,
                        SP_CD,
                        SP_NM,
                        AG_CD,
                        AG_NM,
                        FC_CD,
                        FC_NM,
                        RELEASE_NO,
                        CNTR_NO,
                        C_MASTER_NO,
                        C_HOUSE_NO,
                        ISCOMBINE_BL,
                        COMBIN_SHIPMENT,
                        ISF_SEND_DATE,
                        IORDER,
                        U_ID,
                        CMP   
                    FROM TKBL WHERE SHIPMENT_ID={3}";
                        string tkuid = System.Guid.NewGuid().ToString();
                        string CREATE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        if (_isout)
                        {
                            CREATE_DATE = TimeZoneHelper.GetTimeZoneDate(Location).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        sql = string.Format(sql, SQLUtils.QuotedStr(tkuid), SQLUtils.QuotedStr(Location), SQLUtils.QuotedStr(CREATE_DATE), SQLUtils.QuotedStr(ShipmentId), col);
                        //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        mixList.Add(sql);
                        #endregion

                        #region 轉進口DN
                        string PartialFlag = Prolink.Math.GetValueAsString(smsmdt.Rows[0]["PARTIAL_FLAG"]);
                        //string tran = Prolink.Math.GetValueAsString(smsmdt.Rows[0]["TRAN_TYPE"]);
                        string houseno = Prolink.Math.GetValueAsString(smsmdt.Rows[0]["HOUSE_NO"]);
                        string CntryCd = string.Empty;
                        List<string> productinfo = new List<string>();
                        List<string> invoicelist = new List<string>();
                        List<string> dninfolist = new List<string>();
                        if (PartialFlag == "Y")
                        {
                            string msg = PartialHandel(ShipmentId, ref sql, Location, mixList, opartno, ref CntryCd, productinfo, invoicelist, dninfolist, tran);
                            if (msg != "success")
                                return new Tuple<string, Dictionary<string, string>>(msg, smdic);
                        }
                        else
                        {
                            sql = "SELECT COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            string CombineInfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            string[] DnNos = CombineInfo.Split(',');
                            List<refdninfo> refdnlist = getRefDnDataList(DnNos);
                            //List<string> lastdnlist = getLastDnList(refdnlist);

                            sql = "SELECT TOP 1 TAX_NO FROM SMSMPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            string DnImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (string.IsNullOrEmpty(DnImportNo))
                            {
                                sql = string.Format(@"SELECT TOP 1 TAX_NO FROM SMPTY WHERE PARTY_NO=(
                                        SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID={0})",
                                     SQLUtils.QuotedStr(ShipmentId));
                                DnImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }

                            sql = "SELECT ISNULL(POD_CD,DEST_CD) AS POD_CD FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            string PodCd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (PodCd.Length >= 2)
                            {
                                CntryCd = PodCd.Substring(0, 2);
                            }

							Func<string, string> SetDNInfo = (DNNO) =>
							{
								if (!string.IsNullOrEmpty(DNNO) && !dninfolist.Contains(DNNO))
								{
									dninfolist.Add(DNNO);
								}
								return "";
							};
							for (int i = 0; i < refdnlist.Count; i++)
                            {
                                refdninfo refdnitem = refdnlist[i];
                                string lastdnitem = getLastDnData(refdnitem);
                                //SetDNInfo(refdnitem.DN3);
                                //SetDNInfo(refdnitem.DN2);
                                SetDNInfo(refdnitem.DN1);
                                sql = string.Format(@"SELECT CASE WHEN REF_NO IS NULL THEN AMOUNT1 ELSE (SELECT TOP 1 AMOUNT1 FROM SMDN A WHERE SMDN.REF_NO=A.DN_NO) END AS AMOUNT2
		                        ,CASE WHEN REF_NO IS NULL THEN CUR ELSE (SELECT TOP 1 CUR FROM SMDN A WHERE SMDN.REF_NO=A.DN_NO) END AS CUR2,
                                (SELECT TOP 1 SHIPMENT_ID FROM SMDN WHERE DN_NO={0}) AS DN_SHIPMENT_ID,* 
                                FROM SMDN WHERE DN_NO={1}", SQLUtils.QuotedStr(refdnitem.oldDN), SQLUtils.QuotedStr(lastdnitem));
                                DataTable dndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                sql = "DELETE FROM SMIDN WHERE DN_NO=" + SQLUtils.QuotedStr(lastdnitem);
                                mixList.Add(sql);
                                sql = "DELETE FROM SMIDNP WHERE DN_NO=" + SQLUtils.QuotedStr(lastdnitem);
                                mixList.Add(sql);

                                sql = "SELECT * FROM SMCUFT WHERE DN_NO=" + SQLUtils.QuotedStr(refdnitem.oldDN);
                                DataTable cuftdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                sql = "DELETE FROM SMICUFT WHERE DN_NO=" + SQLUtils.QuotedStr(refdnitem.oldDN);
                                mixList.Add(sql);

                                if (cuftdt.Rows.Count > 0)
                                {
                                    foreach (DataRow cuftdr in cuftdt.Rows)
                                    {
                                        EditInstruct ei;
                                        ei = new EditInstruct("SMICUFT", EditInstruct.INSERT_OPERATION);
                                        string DnUId = System.Guid.NewGuid().ToString();
                                        ei.Put("U_ID", Prolink.Math.GetValueAsString(cuftdr["U_ID"]));
                                        ei.Put("U_FID", Prolink.Math.GetValueAsString(cuftdr["U_FID"]));
                                        ei.Put("DN_NO", refdnitem.oldDN);
                                        ei.Put("L", Prolink.Math.GetValueAsDecimal(cuftdr["L"]));
                                        ei.Put("W", Prolink.Math.GetValueAsDecimal(cuftdr["W"]));
                                        ei.Put("H", Prolink.Math.GetValueAsDecimal(cuftdr["H"]));
                                        ei.Put("PKG", Prolink.Math.GetValueAsDecimal(cuftdr["PKG"]));
                                        ei.Put("PKG_UNIT", Prolink.Math.GetValueAsString(cuftdr["PKG_UNIT"]));
                                        ei.Put("CUFT", Prolink.Math.GetValueAsDecimal(cuftdr["CUFT"]));
                                        ei.Put("VW", Prolink.Math.GetValueAsDecimal(cuftdr["VW"]));
                                        ei.Put("GW", Prolink.Math.GetValueAsDecimal(cuftdr["GW"]));
                                        ei.Put("SBW", Prolink.Math.GetValueAsDecimal(cuftdr["SBW"]));
                                        mixList.Add(ei);
                                    }
                                }
                                Func<string, string> GeInvoice = (InvNo) =>
                                {
                                    if (!string.IsNullOrEmpty(InvNo))
                                    {
                                        InvNo = InvNo.Substring(4, InvNo.Length - 4);
                                        InvNo = InvNo.TrimStart(new char[] { '0' });
                                    }
                                    return InvNo;
                                };
                                Func<string,string> SetInvoice = (InvNo) =>
                                {
                                    InvNo = GeInvoice(InvNo);
                                    if (!string.IsNullOrEmpty(InvNo) && !invoicelist.Contains(InvNo))
                                    {
                                        invoicelist.Add(InvNo);
                                    }
                                    return "";
                                };
                                if (dndt.Rows.Count > 0)
                                {
                                    foreach (DataRow dndr in dndt.Rows)
                                    {
                                        string GroupId = Prolink.Math.GetValueAsString(dndr["GROUP_ID"]);
                                        string Odnuid = Prolink.Math.GetValueAsString(dndr["U_ID"]);
                                        string dnshipmentid = Prolink.Math.GetValueAsString(dndr["SHIPMENT_ID"]);
                                        string refNo = Prolink.Math.GetValueAsString(dndr["REF_NO"]);
                                        if(!string.IsNullOrEmpty(refdnitem.DN3))
                                            SetInvoice(refdnitem.DN3);
                                        else if (!string.IsNullOrEmpty(refdnitem.DN2)) 
                                            SetInvoice(refdnitem.DN2);
                                        else if (!string.IsNullOrEmpty(refdnitem.DN1))
                                            SetInvoice(refdnitem.DN1);
                                        string InvNo = GeInvoice(lastdnitem);

                                        decimal Nw = Prolink.Math.GetValueAsDecimal(dndr["NW"]);
                                        decimal Gw = Prolink.Math.GetValueAsDecimal(dndr["GW"]);
                                        decimal Cbm = Prolink.Math.GetValueAsDecimal(dndr["CBM"]);
                                        string Gwu = Prolink.Math.GetValueAsString(dndr["GWU"]);
                                        //string Cbmu = Prolink.Math.GetValueAsString(dndr["CBMU"]);
                                        decimal Qty = Prolink.Math.GetValueAsDecimal(dndr["QTY"]);
                                        string Qtyu = Prolink.Math.GetValueAsString(dndr["QTYU"]);
                                        decimal PkgNum = Prolink.Math.GetValueAsDecimal(dndr["PKG_NUM"]);
                                        string PkgUnit = Prolink.Math.GetValueAsString(dndr["PKG_UNIT"]);
                                        sql = "SELECT AP_CD FROM BSCODE WHERE CD_TYPE='TCNT' AND (CMP='*' OR CMP=" + SQLUtils.QuotedStr(Location) + ")";
                                        PkgUnit = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        string PkgUnitDesc = Prolink.Math.GetValueAsString(dndr["PKG_UNIT_DESC"]);
                                        decimal Cnt20 = Prolink.Math.GetValueAsDecimal(dndr["CNT20"]);
                                        decimal Cnt40 = Prolink.Math.GetValueAsDecimal(dndr["CNT40"]);
                                        decimal Cnt40hq = Prolink.Math.GetValueAsDecimal(dndr["CNT40HQ"]);
                                        string OthCntType = Prolink.Math.GetValueAsString(dndr["CNT_TYPE"]);
                                        string OthCntNum = Prolink.Math.GetValueAsString(dndr["CNT_NUMBER"]);
                                        string OCntType = Prolink.Math.GetValueAsString(dndr["OCNT_TYPE"]);
                                        string OCntNum = Prolink.Math.GetValueAsString(dndr["OCNT_NUMBER"]);
                                        string DivisionDescp = Prolink.Math.GetValueAsString(dndr["DIVISION_DESCP"]);
                                        string dnShipmentId = Prolink.Math.GetValueAsString(dndr["DN_SHIPMENT_ID"]);
                                        if (!productinfo.Contains(DivisionDescp))
                                        {
                                            if (!string.IsNullOrEmpty(DivisionDescp))
                                                productinfo.Add(DivisionDescp);
                                        }

                                        EditInstruct ei;
                                        ei = new EditInstruct("SMIDN", EditInstruct.INSERT_OPERATION);
                                        if ("T".Equals(tran))
                                        {
                                            string msg = DestInfoUpdate(ShipmentId, Location, ei);
                                            if (msg != "success")
                                                return new Tuple<string, Dictionary<string, string>>(msg, smdic);
                                        }
                                        string DnUId = System.Guid.NewGuid().ToString();
                                        ei.Put("U_ID", DnUId);
                                        ei.Put("GROUP_ID", GroupId);
                                        ei.Put("CMP", Location);
                                        ei.Put("STN", "*");
                                        ei.Put("DEP", "*");
                                        ei.Put("SHIPMENT_ID", ShipmentId);
                                        ei.Put("DN_NO", lastdnitem);
                                        ei.Put("INV_NO", InvNo);
                                        ei.Put("IMPORT_NO", DnImportNo);
                                        ei.Put("TC_IMPORT_NO", DnImportNo);
                                        ei.Put("NW", Nw);
                                        ei.Put("GW", Gw);
                                        ei.Put("GWU", Gwu);
                                        ei.Put("CBM", Cbm);
                                        //ei.Put("CBMU", Cbmu);
                                        ei.Put("QTY", Qty);
                                        ei.Put("QTYU", Qtyu);
                                        ei.Put("PKG_NUM", PkgNum);
                                        ei.Put("PKG_UNIT", PkgUnit);
                                        ei.Put("PKG_UNIT_DESC", PkgUnitDesc);
                                        ei.Put("CNT20", Cnt20);
                                        ei.Put("CNT40", Cnt40);
                                        ei.Put("CNT40HQ", Cnt40hq);
                                        ei.Put("OTH_CNT_TYPE", OthCntType);
                                        ei.Put("OTH_CNT_NUM", OthCntNum);
                                        ei.Put("DIVISION_DESCP", DivisionDescp);
                                        ei.Put("CNTRY_CD", CntryCd);
                                        ei.Put("TC_CNTRY_CD", CntryCd);
                                        if (_isout)
                                        {
                                            ei.Put("OCNT_TYPE", OCntType);
                                            ei.Put("OCNT_NUMBER", OCntNum);
                                        }
                                        SetSmind(dndr, ei);
                                        //ei.Put("PRIORITY", "2");
                                        mixList.Add(ei);

                                        sql = "SELECT * FROM SMDNP WHERE U_FID=" + SQLUtils.QuotedStr(Odnuid);
                                        DataTable dnpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                       
                                        sql = string.Format(@"SELECT CNTR_NO, SEAL_NO1, SEAL_NO2 FROM SMRV WHERE STATUS = 'O' AND CNTR_NO IN(
                                            SELECT CNTR_NO FROM CONTAINER_DNINFO WHERE DN_NO ={0}) AND SHIPMENT_ID = {1}", SQLUtils.QuotedStr(refdnitem.oldDN), SQLUtils.QuotedStr(dnShipmentId));
                                        DataTable cntrdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        if (cntrdt.Rows.Count <= 0)
                                        {
                                            sql = "SELECT CNTR_NO,SEAL_NO1,SEAL_NO2 FROM SMRV WHERE STATUS='O' AND DN_NO LIKE '%{0}%'  AND SHIPMENT_ID = {1}";
                                            sql = string.Format(sql, refdnitem.oldDN, SQLUtils.QuotedStr(dnShipmentId));
                                            cntrdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                            if (cntrdt.Rows.Count <= 0 && dnShipmentId.StartsWith("MHK_B"))
                                            {
                                                sql = "SELECT CNTR_NO,SEAL_NO1,SEAL_NO2 FROM SMRV WHERE STATUS='O' AND DN_NO LIKE '%{0}%'";
                                                sql = string.Format(sql, refdnitem.DN1);
                                                cntrdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                            }
                                        }
                                        foreach (DataRow dnpdr in dnpdt.Rows)
                                        {
                                            string cntrNo = string.Empty;
                                            if (cntrdt.Rows.Count > 0)
                                            {
                                                foreach (DataRow cntrdr in cntrdt.Rows)
                                                {
                                                    ei = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
                                                    //if(!asnDic.ContainsKey())
                                                    InsertIntoSMIDNP(ShipmentId, opartno, PoNo, IhsCode, OhsCode, Resolution, qtyList, refdnitem.DN1, InvNo, ei, DnUId, dnpdr);
                                                    cntrNo = Prolink.Math.GetValueAsString(cntrdr["CNTR_NO"]);
                                                    string sealno1 = Prolink.Math.GetValueAsString(cntrdr["SEAL_NO1"]);
                                                    string sealno2 = Prolink.Math.GetValueAsString(cntrdr["SEAL_NO2"]);
                                                    //SetPartAndAsn(ref asnDate, partnoList, ref isY, qtyList, asnDetailList, InvNo, ei, dnpdr);
                                                    ei.Put("SEAL_NO1", sealno1);
                                                    ei.Put("SEAL_NO2", sealno2);
                                                    if ("E".Equals(tran))
                                                    {
                                                        cntrNo = houseno;
                                                    }
                                                    ei.Put("CNTR_NO", cntrNo);
                                                    ei.Put("U_ID", System.Guid.NewGuid().ToString());
                                                    mixList.Add(ei);
                                                }
                                            }
                                            else
                                            {
                                                ei = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
                                                //SetPartAndAsn(ref asnDate, partnoList, ref isY, qtyList, asnDetailList, InvNo, ei, dnpdr);
                                                InsertIntoSMIDNP(ShipmentId, opartno, PoNo, IhsCode, OhsCode, Resolution, qtyList, refdnitem.DN1, InvNo, ei, DnUId, dnpdr);
                                                if ("E".Equals(tran))
                                                {
                                                    cntrNo = houseno;
                                                }
                                                ei.Put("CNTR_NO", cntrNo);
                                                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                                                mixList.Add(ei);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        #endregion

                        string CntrInfo = string.Empty;
                        #region 轉進口貨櫃
                        sql = "SELECT * FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string ouid = string.Empty, SmGroupId = string.Empty, Stn = string.Empty;
                        string podnm = string.Empty, trantype = string.Empty;
                        string pod = string.Empty;
                        string cmp = string.Empty;
                        string combineinfo = string.Empty;

                        if (smdt.Rows.Count > 0)
                        {
                            ouid = Prolink.Math.GetValueAsString(smdt.Rows[0]["U_ID"]);
                            SmGroupId = Prolink.Math.GetValueAsString(smdt.Rows[0]["GROUP_ID"]);
                            Stn = Prolink.Math.GetValueAsString(smdt.Rows[0]["STN"]);
                            pod = Prolink.Math.GetValueAsString(smdt.Rows[0]["POD_CD"]);
                            podnm = Prolink.Math.GetValueAsString(smdt.Rows[0]["POD_NAME"]);
                            trantype = Prolink.Math.GetValueAsString(smdt.Rows[0]["TRAN_TYPE"]);
                            cmp = Prolink.Math.GetValueAsString(smdt.Rows[0]["CMP"]);
                            combineinfo = Prolink.Math.GetValueAsString(smdt.Rows[0]["COMBINE_INFO"]);
                        }
                        string[] combinedns = combineinfo.Split(',');
                        string condtion = string.Empty;
                        if (!string.IsNullOrEmpty(cmp))
                        {
                            condtion += string.Format(" AND CMP={0}", SQLUtils.QuotedStr(cmp));
                        }
                        sql = string.Format(@"SELECT * FROM SMRV WHERE SHIPMENT_ID IN(
                                            SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT=REPLACE({0},'MHK_',''))
                                            UNION SELECT * FROM SMRV WHERE SHIPMENT_ID={0} ORDER BY RESERVE_NO DESC", SQLUtils.QuotedStr(ShipmentId));
                        DataTable cdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if ("S".Equals(issplitbill))
                        {
                            if (combinedns.Length > 0 && combinedns[0].ToString().Contains('_'))    //上传packing的方式，并不会复制SMRV所以用原来的shipment
                            {
                                string oldshipmentid = ShipmentId.Split('_')[0].ToString();
                                sql = string.Format(@"SELECT * FROM SMRV WHERE SHIPMENT_ID IN(
                                            SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT=REPLACE({0},'MHK_','')){1}
                                            UNION SELECT * FROM SMRV WHERE SHIPMENT_ID={0}{1} ORDER BY RESERVE_NO DESC", SQLUtils.QuotedStr(oldshipmentid), condtion);
                                cdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            else
                            {
                                if (combinedns.Length == 1 && (!string.IsNullOrEmpty(combinedns[0])))
                                {
                                    sql = string.Format("SELECT * FROM SMRV WHERE DN_NO={0}{1}", SQLUtils.QuotedStr(combinedns[0].ToString()), condtion);
                                    cdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    if (cdt.Rows.Count == 0)
                                    {
                                        sql = string.Format("SELECT * FROM SMRV WHERE CNTR_NO IN (SELECT CNTR_NO FROM CONTAINER_DNINFO WHERE DN_NO IN {0}) {1}", SQLUtils.Quoted(combinedns), condtion);
                                        cdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    }
                                }
                                else
                                {
                                    sql = string.Format("SELECT * FROM SMRV WHERE CNTR_NO IN (SELECT CNTR_NO FROM CONTAINER_DNINFO WHERE DN_NO IN {0}) {1}", SQLUtils.Quoted(combinedns), condtion);
                                    cdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                }
                                if (IsChangeToLCL(combinedns) && "F".Equals(trantype))
                                {
                                    trantype = "L";
                                    EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                                    ei.PutKey("SHIPMENT_ID", ShipmentId);
                                    ei.Put("TRAN_TYPE", trantype);
                                    mixList.Add(ei);
                                }
                            }
                        }

                        sql = "SELECT TOP 1 TAX_NO FROM SMSMPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        string CntrImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (string.IsNullOrEmpty(CntrImportNo))
                        {
                            sql = string.Format(@"SELECT TOP 1 TAX_NO FROM SMPTY WHERE PARTY_NO=(
                                        SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID={0})",
                                 SQLUtils.QuotedStr(ShipmentId));
                            CntrImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }

                        if (cdt.Rows.Count > 0)
                        {
                            sql = "DELETE FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            mixList.Add(sql);
                            foreach (DataRow cdr in cdt.Rows)
                            {
                                string CNTR_NO = Prolink.Math.GetValueAsString(cdr["CNTR_NO"]);
                                string CNTR_TYPE = Prolink.Math.GetValueAsString(cdr["CNT_TYPE"]);
                                string SEAL_NO1 = Prolink.Math.GetValueAsString(cdr["SEAL_NO1"]);
                                string SEAL_NO2 = Prolink.Math.GetValueAsString(cdr["SEAL_NO2"]);
                                string BAT_NO = Prolink.Math.GetValueAsString(cdr["BAT_NO"]);
                                string qty = Prolink.Math.GetValueAsString(cdr["QTY"]);
                                string CntrDnNo = string.Empty;
                                string DivisionDescp = string.Empty;
                                string smrvshipment = Prolink.Math.GetValueAsString(cdr["SHIPMENT_ID"]);

                                sql = string.Format(@"SELECT DN_NO,(SELECT TOP 1 REF_NO FROM SMDN WHERE SMDN.DN_NO=CONTAINER_DNINFO.DN_NO)AS REF_NO,
                                    (SELECT TOP 1 DIVISION_DESCP FROM SMDN WHERE SMDN.DN_NO=CONTAINER_DNINFO.DN_NO)AS DIVISION_DESCP,
                                    CNTR_NO FROM CONTAINER_DNINFO WHERE CNTR_NO={0} AND BAT_NO={1}", SQLUtils.QuotedStr(CNTR_NO), SQLUtils.QuotedStr(BAT_NO));
                                DataTable ddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (ddt.Rows.Count == 0)
                                {
                                    sql = "SELECT DN_NO,REF_NO,DIVISION_DESCP FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(smrvshipment);
                                    ddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                }
                                string invinfo = string.Empty;
                                if (ddt.Rows.Count > 0)
                                {
                                    int k = 0;
                                    foreach (DataRow ddr in ddt.Rows)
                                    {
                                        string dnno = Prolink.Math.GetValueAsString(ddr["DN_NO"]);
                                        string refNo = Prolink.Math.GetValueAsString(ddr["REF_NO"]);
                                        refdninfo ddrrefdn = getRefDnData(dnno);
                                        CntrDnNo += ddrrefdn.DN1 + ",";
                                        string InvNo = string.IsNullOrEmpty(refNo) ? dnno.Substring(4, dnno.Length - 4) : refNo.Substring(4, refNo.Length - 4);
                                        InvNo = InvNo.TrimStart(new char[] { '0' });
                                        invinfo += InvNo + ",";

                                        if (k == 0)
                                        {
                                            DivisionDescp = Prolink.Math.GetValueAsString(ddr["DIVISION_DESCP"]);
                                        }
                                        k++;
                                    }
                                    invinfo = invinfo.Trim(',');
                                    CntrDnNo = CntrDnNo.Trim(',');
                                }
                                CntrInfo += CNTR_NO + ",";
                                if ("E".Equals(tran))
                                    CNTR_NO = houseno;
                                EditInstruct ei;
                                ei = new EditInstruct("SMICNTR", EditInstruct.INSERT_OPERATION);
                                string Cuid = System.Guid.NewGuid().ToString();
                                ei.Put("U_ID", Cuid);
                                ei.Put("GROUP_ID", SmGroupId);
                                ei.Put("CMP", Location);
                                ei.Put("STN", Stn);
                                ei.Put("SHIPMENT_ID", ShipmentId);
                                ei.Put("CMP", Location);
                                ei.Put("CNTR_NO", CNTR_NO);
                                ei.Put("CNTR_TYPE", CNTR_TYPE);
                                ei.Put("IMPORT_NO", CntrImportNo);
                                ei.Put("TC_IMPORT_NO", CntrImportNo);
                                ei.Put("NEW_SEAL", SEAL_NO1);
                                ei.Put("TC_NEW_SEAL", SEAL_NO1);
                                ei.Put("SEAL_NO1", SEAL_NO1);
                                ei.Put("SEAL_NO2", SEAL_NO2);
                                ei.Put("DN_NO", CntrDnNo);
                                ei.Put("DIVISION_DESCP", DivisionDescp);
                                ei.Put("BACK_LOCATION", pod);
                                ei.Put("CNTRY_CD", CntryCd);
                                ei.Put("TC_CNTRY_CD", CntryCd);
                                if ("F".Equals(trantype) || "R".Equals(trantype))
                                {
                                    ei.Put("TRAN_TYPE1", "T");
                                    ei.Put("POL1", pod);
                                    ei.Put("POL_NM1", podnm);

                                    DataTable bsDateDt = DayHelper.GetBsdate(Location);

                                    ei.PutDate("STORAGE_DUE_DATE", DayHelper.AddWorkHolidays(eta, portFreeItem.Item3, bsDateDt, portFreeItem.Item7));
                                    ei.Put("CON_CHG_TYPE", portFreeItem.Item7);
                                    ei.PutDate("DEMURRAGE_DUE_DATE", DayHelper.AddWorkHolidays(eta, portFreeItem.Item4, bsDateDt, portFreeItem.Rest.Item1));
                                    ei.Put("PORT_CHG_TYPE", portFreeItem.Rest.Item1);
                                    if ((!string.IsNullOrEmpty(portFreeItem.Item1) || _isout) && portFreeItem.Item1.StartsWith("U"))
                                    {
                                        if (cdr["PICK_DATE"] != null && cdr["PICK_DATE"] != DBNull.Value)
                                        {
                                            DateTime pickDate = Prolink.Math.GetValueAsDateTime(cdr["PICK_DATE"]);
                                            ei.PutDate("DEMURRAGE_DUE_DATE", DayHelper.AddWorkHolidays(pickDate, portFreeItem.Item4, bsDateDt, portFreeItem.Rest.Item1));
                                        }
                                    }
                                }
                                if (cdr["PICK_DATE"] != null && cdr["PICK_DATE"] != DBNull.Value)
                                {
                                    ei.PutDate("EMP_PICK_DATE", Prolink.Math.GetValueAsDateTime(cdr["PICK_DATE"]));
                                }
                                //ei.Put("PRIORITY", "2");
                                ei.Put("INV_NO", invinfo);
                                ei.Put("QTY", qty);
                                mixList.Add(ei);
                            }
                        }
                        #endregion
                        if (invoicelist.Count > 0 || dninfolist.Count > 0 || productinfo.Count > 0)
                        {
                            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                            ei.PutKey("SHIPMENT_ID", ShipmentId);
                            if (invoicelist.Count > 0)
                            {
                                ei.Put("INVOICE_INFO", string.Join(",", invoicelist));
                            }
                            if (productinfo.Count > 0)
                            {
                                ei.Put("PRODUCT_INFO", string.Join(",", productinfo));
                            }
                            if (dninfolist.Count > 0)
                            {
                                ei.Put("COMBINE_INFO", string.Join(",", dninfolist));
                            }
                            mixList.Add(ei);
                        }

                        if (mixList.Count > 0)
                        {
                            try
                            {
                                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                                //DataTable smiDt = OperationUtils.GetDataTable(string.Format("SELECT COMBINE_INFO FROM SMSMI WHERE SHIPMENT_ID={0}",
                                //    SQLUtils.QuotedStr(ShipmentId)), null, Prolink.Web.WebContext.GetInstance().GetConnection());


                                string opartstr = string.Join(",", opartno);
                                if (opartstr.Length > 200)
                                    opartstr = opartstr.Substring(0, 200);
                                string poinfo = string.Join(",", PoNo);
                                if (poinfo.Length > 500)
                                    poinfo = poinfo.Substring(0, 500);
                                string ihsinfo = string.Join(",", IhsCode);
                                if (ihsinfo.Length > 500)
                                    ihsinfo = ihsinfo.Substring(0, 500);
                                string ohsinfo = string.Join(",", OhsCode);
                                if (ohsinfo.Length > 500)
                                    ohsinfo = ohsinfo.Substring(0, 500);
                                string resolutioninfo = string.Join(",", Resolution);
                                if (resolutioninfo.Length > 500)
                                    resolutioninfo = resolutioninfo.Substring(0, 500);
                                string partnoInfo = string.Join(",", partnoList);
                                if (partnoInfo.Length > 500)
                                    partnoInfo = partnoInfo.Substring(0, 500);
                                string modelQty = string.Join(",", qtyList);
                                if (modelQty.Length > 500)
                                    modelQty = modelQty.Substring(0, 500);
                                mixList = new MixedList();
                                string extrasrv = SMSMIHelper.GetExtraSrvInfo(ShipmentId, trantype);
                                EditInstruct smei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                                smei.PutKey("SHIPMENT_ID", ShipmentId);
                                smei.Put("EXRERNAL_INFO", opartstr);
                                smei.Put("PONO_INFO", poinfo);
                                if (!string.IsNullOrEmpty(extrasrv))
                                {
                                    smei.Put("EXTRA_SRV", extrasrv);
                                }
                                smei.Put("IHSCODE_INFO", ihsinfo);
                                smei.Put("OHSCODE_INFO", ohsinfo);
                                smei.Put("RESOLUTION_INFO", resolutioninfo);

                                smei.Put("PART_QTY", modelQty);
                                smei.Put("BU", subBgBu);
                                if (_isout)
                                {
                                    smei.Put("PARTNO_INFO", partnoInfo);
                                    if (asnDate > new DateTime(2000, 1, 1))
                                        smei.PutDate("ASN_DATE", asnDate);
                                }
                                SMSMIHelper.InboundsetLight(muid, ouid, "I");

                                string dnnos = OperationUtils.GetValueAsString(string.Format("SELECT COMBINE_INFO FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)), Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (!string.IsNullOrEmpty(dnnos))
                                {
                                    List<string> dntwoinfo = new List<string>();
                                    List<string> sotwoinfo = new List<string>();
                                    string[] dnnolist = dnnos.Split(',');
                                    if (_isout)
                                    {
                                        string smicuft_sql = string.Format("SELECT * FROM SMICUFT WHERE DN_NO IN {0}", SQLUtils.Quoted(dnnolist));
                                        DataTable smicuftDt = OperationUtils.GetDataTable(smicuft_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        List<string> cuftList = new List<string>();
                                        foreach (DataRow cuft in smicuftDt.Rows)
                                        {
                                            cuftList.Add(string.Format("{0}:{1}*{2}*{3}", cuft["DN_NO"], cuft["L"], cuft["W"], cuft["H"]));
                                        }
                                        if (cuftList.Count > 0)
                                            smei.Put("DIMENSIONS_INFO", string.Join(";", cuftList));
                                    }
                                    string dntwosql = string.Format("SELECT REF_NO FROM SMDN WHERE DN_NO IN {0}", SQLUtils.Quoted(dnnolist));
                                    DataTable dntwodt = OperationUtils.GetDataTable(dntwosql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    foreach (DataRow twodr in dntwodt.Rows)
                                    {
                                        string refno = Prolink.Math.GetValueAsString(twodr["REF_NO"]);
                                        if (string.IsNullOrEmpty(refno))
                                            continue;
                                        dntwoinfo.Add(refno);
                                    }
                                    if (dntwoinfo.Count > 0)
                                    {
                                        dntwosql = string.Format("SELECT PO_NO FROM SMDNP WHERE DN_NO IN {0}", SQLUtils.Quoted(dntwoinfo.ToArray()));
                                        DataTable potwodt = OperationUtils.GetDataTable(dntwosql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        foreach (DataRow twodr in potwodt.Rows)
                                        {
                                            string sono = Prolink.Math.GetValueAsString(twodr["PO_NO"]);
                                            if (string.IsNullOrEmpty(sono) || sotwoinfo.Contains(sono))
                                                continue;
                                            sotwoinfo.Add(sono);
                                        }
                                        smei.Put("DN_TWO_INFO", string.Join(",", dntwoinfo));
                                        smei.Put("SO_TWO_INFO", string.Join(",", sotwoinfo));
                                    }
                                }



                                if (smsmdt.Rows[0]["ISF_SEND_DATE"] != null && smsmdt.Rows[0]["ISF_SEND_DATE"] != DBNull.Value)
                                {
                                    DateTime isfsenddate = Prolink.Math.GetValueAsDateTime(smsmdt.Rows[0]["ISF_SEND_DATE"]);
                                    DateTime isfldate = TimeZoneHelper.GetTimeZoneDate(isfsenddate, sh_location);
                                    smei.PutDate("ISF_SEND_DATE_L", isfldate);
                                }

                                if (IS_ISF_SENDING)
                                    smei.Put("STATUS", "S");
                                if ("E".Equals(elart))
                                {
                                    smei.Put("STATUS", "E");
                                    if (EalertDate <= DateTime.MinValue)
                                    {
                                        EalertDate = DateTime.Now;
                                        EalertDateL = TimeZoneHelper.GetTimeZoneDate(EalertDate, CompanyId);
                                        smei.PutDate("EALERT_DATE", EalertDate);
                                        smei.PutDate("EALERT_DATE_L", EalertDateL);
                                    }
                                }
                                if (IS_ISF_SENDING || "E".Equals(elart))
                                {
                                }
                                else
                                {
                                    if (InboundDate <= DateTime.MinValue)
                                    {
                                        InboundDate = DateTime.Now;
                                        InboundDateL = TimeZoneHelper.GetTimeZoneDate(InboundDate, CompanyId);
                                        smei.PutDate("INBOUND_DATE", InboundDate);
                                        smei.PutDate("INBOUND_DATE_L", InboundDateL);
                                    }
                                }
                                smdic.Add("EALERT_DATE", EalertDate <= DateTime.MinValue ? "" : EalertDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                smdic.Add("EALERT_DATE_L", EalertDateL <= DateTime.MinValue ? "" : EalertDateL.ToString("yyyy-MM-dd HH:mm:ss"));
                                smdic.Add("INBOUND_DATE", InboundDate <= DateTime.MinValue ? "" : InboundDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                smdic.Add("INBOUND_DATE_L", InboundDateL <= DateTime.MinValue ? "" : InboundDateL.ToString("yyyy-MM-dd HH:mm:ss"));
                                string smsmEta = Prolink.Math.GetValueAsString(smsmdt.Rows[0]["ETA"]);
                                if (!string.IsNullOrEmpty(smsmEta) && TrackingEDI.Business.DateTimeUtils.IsDate(smsmEta))
                                {
                                    DateTime ETA = Prolink.Math.GetValueAsDateTime(smsmEta);
                                    int week = TrackingEDI.Business.DateTimeUtils.GetWeekOfYear(ETA, DayOfWeek.Monday);
                                    smei.Put("ETA_WK", week);
                                }

                                if (CntrInfo.Length > 0)
                                {
                                    if (string.IsNullOrEmpty(CntrInfo.Trim(',')))
                                    {
                                        if ("E".Equals(tran))
                                            smei.Put("CNTR_INFO", houseno);
                                    }
                                    else
                                        smei.Put("CNTR_INFO", CntrInfo.Trim(','));
                                }
                                else
                                {
                                    if ("E".Equals(tran))
                                        smei.Put("CNTR_INFO", houseno);
                                }

                                string smsmisql = string.Format("SELECT DEST_CD,(SELECT TOP 1 REGION FROM BSCITY WHERE CNTRY_CD+PORT_CD = SMSMI.DEST_CD) AS REGION FROM SMSMI WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(ShipmentId));
                                DataTable regiondt = OperationUtils.GetDataTable(smsmisql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (regiondt.Rows.Count > 0)
                                {
                                    string destct = Prolink.Math.GetValueAsString(regiondt.Rows[0]["DEST_CD"]);
                                    if (destct.Length > 2)
                                    {
                                        smei.Put("DEST_CNTY", destct.Substring(0, 2));
                                    }
                                    smei.Put("DEST_REGION", Prolink.Math.GetValueAsString(regiondt.Rows[0]["REGION"]));
                                }
                                smei.PutExpress("GVALUE", "(SELECT SUM(AMOUNT) FROM SMIDN WHERE SMIDN.SHIPMENT_ID = SMSMI.SHIPMENT_ID)");
                                smei.PutExpress("CUR", "(SELECT TOP 1 INV_CUR FROM SMIDN WHERE SMIDN.SHIPMENT_ID = SMSMI.SHIPMENT_ID)");
                                mixList.Add(smei);
                                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                                UpdatePartyToSMSMINew(ShipmentId, trantype);
                                InboundAllcByShipment(ShipmentId);
                                SetPartyToIBCRByShipID(ShipmentId);

                                ASNManager.SetAsnMapToSMIDNP(ShipmentId);
                                ASNManager.SetAsnByShipmentid(ShipmentId);

                                is_ok = "Y";
                                if (IS_ISF_SENDING)
                                {
                                    is_ok = "S";
                                    Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = ShipmentId, StsCd = "000", Cmp = CompanyId, Sender = "Outbound", Location = podcd, LocationName = "", StsDescp = "ISF Sending Info." });
                                }
                                else
                                {
                                    if ("E".Equals(elart))
                                    {
                                        is_ok = "N";
                                        Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = ShipmentId, StsCd = "000", Cmp = CompanyId, Sender = "Outbound", Location = podcd, LocationName = "", StsDescp = "E-Alert Info." });
                                    }
                                    else
                                    {
                                        Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = ShipmentId, StsCd = "000", Cmp = CompanyId, Sender = "Outbound", Location = podcd, LocationName = "", StsDescp = "Init Booking Info." });
                                    }
                                }
                                //CalIFAndFob(ShipmentId);
                            }
                            catch (Exception ex)
                            {
                                return new Tuple<string, Dictionary<string, string>>("转Inbound失败，请联系系统管理员!" + ex, smdic);
                            }
                        }
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                return new Tuple<string, Dictionary<string, string>>("转Inbound失败，请联系系统管理员!" + ex, smdic);
            }
            return new Tuple<string, Dictionary<string, string>>(is_ok, smdic);
        }

        public static void SetTruckToAutoInfo(string shipmentid, string UserId, string CompanyId)
        {
            string sql = string.Format("SELECT DATEDIFF(DAY,ETD,ETA) AS INTERVAL_DAY,* FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string suid = maindt.Rows[0]["U_ID"].ToString();
            string groupid = maindt.Rows[0]["GROUP_ID"].ToString();
            string batNo = maindt.Rows[0]["BAT_NO"].ToString();
            string confirmmsg = BookingConfirm.SetConfirm(maindt, UserId, CompanyId);
            if (string.IsNullOrEmpty(confirmmsg))
            {
                //叫车
                string ordsql = string.Format("select ORD_NO  FROM smord WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                string ordno = OperationUtils.GetValueAsString(ordsql, Prolink.Web.WebContext.GetInstance().GetConnection());
                SMSMIHelper.ToDoorDelivery(suid, UserId, CompanyId, groupid, ordno, "R");

                EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("ORD_NO", ordno);
                ei.Put("CSTATUS", "R");
                ei.Put("BAT_NO", batNo);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

        }
        
        private static void InsertIntoSMIDNP(string ShipmentId, List<string> opartno, List<string> PoNo, List<string> IhsCode, List<string> OhsCode, List<string> Resolution, List<int> qtyList, string DnNo, string InvNo, EditInstruct ei, string DnUId, DataRow dnpdr)
        {
            ei.Put("U_FID", DnUId);
            ei.Put("DN_NO", DnNo);
            ei.Put("REF_NO", GetRefNo(DnNo));
            ei.Put("QTY", Prolink.Math.GetValueAsString(dnpdr["QTY"]));
            ei.Put("PART_NO", Prolink.Math.GetValueAsString(dnpdr["PART_NO"]));
            ei.Put("INV_NO", InvNo);
            ei.Put("SHIPMENT_ID", ShipmentId);
            ei.Put("IPART_NO", Prolink.Math.GetValueAsString(dnpdr["IPART_NO"]));
            ei.Put("CATEGORY", Prolink.Math.GetValueAsString(dnpdr["CATEGORY"]));
            ei.Put("NEW_CATEGORY", Prolink.Math.GetValueAsString(dnpdr["NEW_CATEGORY"]));
            string opart = Prolink.Math.GetValueAsString(dnpdr["OPART_NO"]);
            ei.Put("SO_NO", Prolink.Math.GetValueAsString(dnpdr["SO_NO"]));
            ei.Put("SIZE", Prolink.Math.GetValueAsString(dnpdr["SIZE"]));
            ei.Put("OPART_NO", opart);
            if (!opartno.Contains(opart))
                opartno.Add(opart);
            ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(dnpdr["GOODS_DESCP"]));
            ei.Put("PROD_DESCP", Prolink.Math.GetValueAsString(dnpdr["PROD_DESCP"]));
            ei.Put("GW", Prolink.Math.GetValueAsDecimal(dnpdr["GW"]));
            ei.Put("CBM", Prolink.Math.GetValueAsDecimal(dnpdr["CBM"]));
            string po = Prolink.Math.GetValueAsString(dnpdr["PO_NO"]);
            string ohs = Prolink.Math.GetValueAsString(dnpdr["OHS_CODE"]);
            string Ihs = Prolink.Math.GetValueAsString(dnpdr["IHS_CODE"]);
            string resolution = Prolink.Math.GetValueAsString(dnpdr["RESOLUTION"]);
            if (!PoNo.Contains(po) && !string.IsNullOrEmpty(po))
                PoNo.Add(po);
            if (!OhsCode.Contains(ohs) && !string.IsNullOrEmpty(ohs))
                OhsCode.Add(ohs);
            if (!IhsCode.Contains(Ihs) && !string.IsNullOrEmpty(Ihs))
                IhsCode.Add(Ihs);
            if (!Resolution.Contains(resolution) && !string.IsNullOrEmpty(resolution))
                Resolution.Add(resolution);
            ei.Put("PO_NO", po);
            ei.Put("OHS_CODE", ohs);
            ei.Put("IHS_CODE", Ihs);
            ei.Put("RESOLUTION", resolution);
            ei.Put("CNTR_STD_QTY", Prolink.Math.GetValueAsString(dnpdr["CNTR_STD_QTY"]));
            ei.Put("BRAND", dnpdr["BRAND"]);
            ei.Put("VALUE1", dnpdr["VALUE1"]);
        }

        private static string CheckInfos(string ShipmentId, ref string podcd, ref string sh_location, string issplitbill, DataTable smsmdt, string elart, DataTable ptdt)
        {
            string tran = smsmdt.Rows[0]["TRAN_TYPE"].ToString();
            string status = smsmdt.Rows[0]["STATUS"].ToString();
            podcd = smsmdt.Rows[0]["POD_CD"].ToString();
            string atd = smsmdt.Rows[0]["ATD"].ToString();
            sh_location = smsmdt.Rows[0]["CMP"].ToString();

            if ("T".Equals(tran))
            {
                if (!(status == "F" || status == "R" || status == "O" || status == "H") && string.IsNullOrEmpty(elart))
                {
                    return string.Format("This Shipment {0} does not Gate Out, Please Check!", ShipmentId);
                }
            }
            else
            {
                string iscombinbl = smsmdt.Rows[0]["ISCOMBINE_BL"].ToString();
                string blcheck = smsmdt.Rows[0]["BL_CHECK"].ToString();

                if (blcheck != "Y")
                    return string.Format("This Shipment {0} does not Check the bill!", ShipmentId);

                DataRow[] tppt = new DataRow[] { };
                if (ptdt.Rows.Count > 0)
                {
                    tppt = ptdt.Select("LOCATION='TP'");
                }


                if (string.IsNullOrEmpty(elart) && tppt.Length <= 0 && string.IsNullOrEmpty(atd))
                    return string.Format("This Shipment {0} does not have the ATD!", ShipmentId);


                string combininfo = smsmdt.Rows[0]["COMBINE_INFO"].ToString();
                combininfo = combininfo.Trim(',');
                string[] dns = combininfo.Split(',');
                int dncounts = dns.Length;

                string[] edoctypes = { "PACKI", "INVI", "PKINCB", "CINVOICE", "CPACKING" };
                string[] shipmentedoctypes = { "BL_CONFIRM" };
                DataTable edocdt = GetEdocTypeCountsDt(edoctypes, ShipmentId, combininfo, issplitbill);
                string uid = smsmdt.Rows[0]["U_ID"].ToString();
                string shipment1 = smsmdt.Rows[0]["IMPORT_NO"].ToString();
                DataTable edocsmdt = GetEdocTypeCountForShipment(shipmentedoctypes, uid, shipment1);
                if ("F".Equals(tran) || "L".Equals(tran) || "R".Equals(tran))
                {
                    if (!EdocCount("BL_CONFIRM", edocsmdt, dncounts, false))
                    {
                        return string.Format("This Shipment {0} doesn't have the Bill of Lading document !", ShipmentId);
                    }
                }

                if (!EdocCount("PKINCB", edocdt, dncounts, false))
                {
                    if (!EdocCount("CINVOICE", edocdt, dncounts, false))
                    {
                        if (!EdocCount("INVI", edocdt, dncounts, true))
                        {
                            return string.Format("This Shipment {0} doesn't have CombinedPKG_INV_Inbound or CombinedInv_Inbound or Packing_Inbound!", ShipmentId);
                        }
                    }
                    if (!EdocCount("CPACKING", edocdt, dncounts, false))
                    {
                        if (!EdocCount("PACKI", edocdt, dncounts, true))
                        {
                            return string.Format("This Shipment {0} doesn't have CombinedPKG_INV_Inbound or CombinedPkg_Inbound or Packing_Inbound!", ShipmentId);
                        }
                    }
                }

                if ("F".Equals(tran) || "R".Equals(tran))
                {
                    if ("S".Equals(issplitbill))
                        return string.Empty;
                    string cntrsql = string.Format("SELECT CNTR_NO,SHIPMENT_ID FROM SMRV WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                    if ("C".Equals(iscombinbl))
                    {
                        cntrsql = string.Format("SELECT CNTR_NO,SHIPMENT_ID FROM SMRV WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT=REPLACE({0},'MHK_',''))", SQLUtils.QuotedStr(ShipmentId));
                    }
                    DataTable cntrdt = OperationUtils.GetDataTable(cntrsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (cntrdt.Rows.Count <= 0)
                        return string.Format("This Shipment {0} doesn't call truck!", ShipmentId);
                    DataRow[] cntrdrs = cntrdt.Select("CNTR_NO IS NULL");
                    if (cntrdrs.Length > 0)
                        return string.Format("This Shipment {0} container number is null!", cntrdrs[0]["SHIPMENT_ID"]);

                    int cntcounts = Prolink.Math.GetValueAsInt(smsmdt.Rows[0]["CNT20"]) + Prolink.Math.GetValueAsInt(smsmdt.Rows[0]["CNT40"]) + Prolink.Math.GetValueAsInt(smsmdt.Rows[0]["CNT40HQ"])
                        + Prolink.Math.GetValueAsInt(smsmdt.Rows[0]["OCNT_NUMBER"]) + Prolink.Math.GetValueAsInt(smsmdt.Rows[0]["CNT_NUMBER"]);
                    if (cntrdt.Rows.Count < cntcounts)
                    {
                        return string.Format("Shipment {0}: 请确认提单柜量与真实叫柜数要一致!", ShipmentId);
                    }
                }
            }
            return string.Empty;
        }

        private static DataTable GetEdocTypeCountsDt(string[] edoctypes, string shipmentid, string combineinfo, string issplitbill)
        {
            List<string> uidlist = new List<string>();
            DataTable dt = GetDnInfos(shipmentid, combineinfo, issplitbill);
            foreach (DataRow dr in dt.Rows)
            {
                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                if (!string.IsNullOrEmpty(uid) && !uidlist.Contains(uid))
                {
                    uidlist.Add(uid);
                }
            }

            string sql = string.Format(@"SELECT DISTINCT EDOCTYPE,COUNT(1) AS COUNTS FROM FILES WHERE EDOCTYPE in {0} AND FID IN (SELECT FID FROM FOLDERS WHERE GUID IN (SELECT  FOLDER_GUID FROM EDOC2_FOLDER
                    WHERE JOB_NO IN {1})) GROUP BY EDOCTYPE",
                    SQLUtils.Quoted(edoctypes), SQLUtils.Quoted(uidlist.ToArray()));
            DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt1;
        }

        private static DataTable GetEdocTypeCountForShipment(string[] edoctypes, string shipmentuid, string shipment1)
        {
            List<string> uidlist = new List<string>();
            uidlist.Add(shipmentuid);
            if (!string.IsNullOrEmpty(shipment1))
            {
                string sql1 = string.Format("SELECT U_ID FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment1));
                string uid1 = OperationUtils.GetValueAsString(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
                uidlist.Add(uid1);
            }
            string sql = string.Format(@"SELECT DISTINCT EDOCTYPE,COUNT(1) AS COUNTS FROM FILES WHERE EDOCTYPE in {0} AND FID IN (SELECT FID FROM FOLDERS WHERE GUID IN (SELECT  FOLDER_GUID FROM EDOC2_FOLDER
                    WHERE JOB_NO IN {1})) GROUP BY EDOCTYPE",
                    SQLUtils.Quoted(edoctypes), SQLUtils.Quoted(uidlist.ToArray()));
            DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt1;
        }

        private static bool EdocCount(string edoctype, DataTable dt, int dncounts, bool isequaldncounts)
        {
            DataRow[] drs = dt.Select(string.Format("EDOCTYPE={0}", SQLUtils.QuotedStr(edoctype)));
            if (drs.Length <= 0) return false;

            int counts = (int)drs[0]["COUNTS"];

            if (isequaldncounts)
            {
                if (counts >= dncounts)
                    return true;
                else
                    return false;
            }
            if (counts > 0)
                return true;
            else
                return false;
        }

        private static string PartialHandel(string ShipmentId, ref string sql, string Location, MixedList mixList, List<string> opartno, ref string CntryCd, List<string> productinfo, List<string> invoicelist, List<string> dninfolist,string tranType)
        {
            string returnmsg = "success";
            sql = "SELECT COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            string CombineInfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (CombineInfo == "")
            {
                sql = "SELECT RELEASE_NO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                string Releaseno = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                string dnno = "N" + ShipmentId;
                if (!string.IsNullOrEmpty(Releaseno))
                    dnno = Releaseno;
                if (!invoicelist.Contains(dnno))
                {
                    invoicelist.Add(dnno);
                }
                if (!dninfolist.Contains(dnno))
                {
                    dninfolist.Add(dnno);
                }

                sql = "SELECT TOP 1 TAX_NO FROM SMSMPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                string DnImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = "SELECT ISNULL(POD_CD,DEST_CD) AS POD_CD FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                string PodCd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (PodCd.Length >= 2)
                {
                    CntryCd = PodCd.Substring(0, 2);
                }

                sql = "SELECT * FROM SMBD WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND DN_NO is null";
                DataTable bddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string Odnuid = string.Empty;
                string DnUId = string.Empty;

                if (bddt.Rows.Count > 0)
                {
                    foreach (DataRow bddr in bddt.Rows)
                    {
                        string GroupId = Prolink.Math.GetValueAsString(bddr["GROUP_ID"]);
                        Odnuid = Prolink.Math.GetValueAsString(bddr["U_ID"]);
                        decimal Nw = Prolink.Math.GetValueAsDecimal(bddr["NW"]);
                        decimal Gw = Prolink.Math.GetValueAsDecimal(bddr["GW"]);
                        decimal Cbm = Prolink.Math.GetValueAsDecimal(bddr["CBM"]);
                        string Gwu = Prolink.Math.GetValueAsString(bddr["GWU"]);
                        decimal Qty = Prolink.Math.GetValueAsDecimal(bddr["QTY"]);
                        string Qtyu = Prolink.Math.GetValueAsString(bddr["QTYU"]);
                        decimal PkgNum = Prolink.Math.GetValueAsDecimal(bddr["PKG_NUM"]);
                        string PkgUnit = Prolink.Math.GetValueAsString(bddr["PKG_UNIT"]);
                        sql = "SELECT AP_CD FROM BSCODE WHERE CD_TYPE='TCNT' AND (CMP='*' OR CMP=" + SQLUtils.QuotedStr(Location) + ")";
                        PkgUnit = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        string PkgUnitDesc = Prolink.Math.GetValueAsString(bddr["PKG_UNIT_DESC"]);
                        //decimal Cnt20 = Prolink.Math.GetValueAsDecimal(bddr["CNT20"]);
                        //decimal Cnt40 = Prolink.Math.GetValueAsDecimal(bddr["CNT40"]);
                        //decimal Cnt40hq = Prolink.Math.GetValueAsDecimal(bddr["CNT40HQ"]);

                        EditInstruct ei;
                        ei = new EditInstruct("SMIDN", EditInstruct.INSERT_OPERATION);
                        if ("T".Equals(tranType))
                        {
                            string msg = DestInfoUpdate(ShipmentId, Location, ei);
                            if (msg != "success")
                                return msg;
                        }
                        DnUId = System.Guid.NewGuid().ToString();
                        ei.Put("U_ID", DnUId);
                        ei.Put("GROUP_ID", GroupId);
                        ei.Put("CMP", Location);
                        ei.Put("STN", "*");
                        ei.Put("DEP", "*");
                        ei.Put("SHIPMENT_ID", ShipmentId);
                        ei.Put("DN_NO", dnno);
                        ei.Put("INV_NO", dnno);
                        ei.Put("NW", Nw);
                        ei.Put("GW", Gw);
                        ei.Put("GWU", Gwu);
                        ei.Put("CBM", Cbm);
                        ei.Put("QTY", Qty);
                        ei.Put("QTYU", Qtyu);
                        ei.Put("PKG_NUM", PkgNum);
                        ei.Put("PKG_UNIT", PkgUnit);
                        ei.Put("PKG_UNIT_DESC", PkgUnitDesc);
                        //ei.Put("CNT20", Cnt20);
                        //ei.Put("CNT40", Cnt40);
                        //ei.Put("CNT40HQ", Cnt40hq);
                        SetSmindByBD(bddr, ei);
                        mixList.Add(ei);
                    }
                }

                sql = "SELECT * FROM SMBDD WHERE U_FID=" + SQLUtils.QuotedStr(Odnuid);
                DataTable bdddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                foreach (DataRow bdddr in bdddt.Rows)
                {
                    EditInstruct ei;
                    ei = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
                    string DnpUId = System.Guid.NewGuid().ToString();
                    ei.Put("U_ID", DnpUId);
                    ei.Put("U_FID", DnUId);
                    ei.Put("DN_NO", dnno);
                    ei.Put("INV_NO", dnno);
                    ei.Put("SHIPMENT_ID", ShipmentId);
                    ei.Put("IPART_NO", Prolink.Math.GetValueAsString(bdddr["IPART_NO"]));
                    string opart = Prolink.Math.GetValueAsString(bdddr["OPART_NO"]);
                    ei.Put("OPART_NO", opart);
                    if (!opartno.Contains(opart))
                        opartno.Add(opart);
                    ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(bdddr["GOODS_DESCP"]));
                    ei.Put("GW", Prolink.Math.GetValueAsDecimal(bdddr["GW"]));
                    ei.Put("CBM", Prolink.Math.GetValueAsDecimal(bdddr["CBM"]));
                    ei.Put("QTY", Prolink.Math.GetValueAsDecimal(bdddr["QTY"]));

                    mixList.Add(ei);
                }
            }
            else
            {
                string[] DnNos = CombineInfo.Split(',');
                sql = "SELECT TOP 1 TAX_NO FROM SMSMPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                string DnImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (string.IsNullOrEmpty(DnImportNo))
                {
                    sql = string.Format(@"SELECT TOP 1 TAX_NO FROM SMPTY WHERE PARTY_NO=(
                                        SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='CS' AND SHIPMENT_ID={0})",
                         SQLUtils.QuotedStr(ShipmentId));
                    DnImportNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }

                sql = "SELECT ISNULL(POD_CD,DEST_CD) AS POD_CD FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                string PodCd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (PodCd.Length >= 2)
                {
                    CntryCd = PodCd.Substring(0, 2);
                }

                for (int i = 0; i < DnNos.Length; i++)
                {
                    sql = @"SELECT CASE WHEN REF_NO IS NULL THEN AMOUNT1 ELSE (SELECT TOP 1 AMOUNT1 FROM SMDN A WHERE SMDN.REF_NO=A.DN_NO) END AS AMOUNT2
		                        ,CASE WHEN REF_NO IS NULL THEN CUR ELSE (SELECT TOP 1 CUR FROM SMDN A WHERE SMDN.REF_NO=A.DN_NO) END AS CUR2,* 
                                FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNos[i]);
                    DataTable dndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = "DELETE FROM SMIDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNos[i]);
                    mixList.Add(sql);
                    sql = "DELETE FROM SMIDNP WHERE DN_NO=" + SQLUtils.QuotedStr(DnNos[i]);
                    mixList.Add(sql);

                    sql = "SELECT * FROM SMCUFT WHERE DN_NO=" + SQLUtils.QuotedStr(DnNos[i]);
                    DataTable cuftdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = "DELETE FROM SMICUFT WHERE DN_NO=" + SQLUtils.QuotedStr(DnNos[i]);
                    mixList.Add(sql);

                    if (cuftdt.Rows.Count > 0)
                    {
                        foreach (DataRow cuftdr in cuftdt.Rows)
                        {
                            EditInstruct ei;
                            ei = new EditInstruct("SMICUFT", EditInstruct.INSERT_OPERATION);
                            string DnUId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", Prolink.Math.GetValueAsString(cuftdr["U_ID"]));
                            ei.Put("U_FID", Prolink.Math.GetValueAsString(cuftdr["U_FID"]));
                            ei.Put("DN_NO", DnNos[i]);
                            ei.Put("L", Prolink.Math.GetValueAsDecimal(cuftdr["L"]));
                            ei.Put("W", Prolink.Math.GetValueAsDecimal(cuftdr["W"]));
                            ei.Put("H", Prolink.Math.GetValueAsDecimal(cuftdr["H"]));
                            ei.Put("PKG", Prolink.Math.GetValueAsDecimal(cuftdr["PKG"]));
                            ei.Put("PKG_UNIT", Prolink.Math.GetValueAsString(cuftdr["PKG_UNIT"]));
                            ei.Put("CUFT", Prolink.Math.GetValueAsDecimal(cuftdr["CUFT"]));
                            ei.Put("VW", Prolink.Math.GetValueAsDecimal(cuftdr["VW"]));
                            ei.Put("GW", Prolink.Math.GetValueAsDecimal(cuftdr["GW"]));
                            ei.Put("SBW", Prolink.Math.GetValueAsDecimal(cuftdr["SBW"]));
                            mixList.Add(ei);
                        }
                    }


                    if (dndt.Rows.Count > 0)
                    {
                        foreach (DataRow dndr in dndt.Rows)
                        {
                            decimal Nw = 0.0M;
                            decimal Gw = 0.0M;
                            decimal Cbm = 0.0M;
                            string Gwu = string.Empty;
                            decimal Qty = 0.0M;
                            string Qtyu = string.Empty;
                            decimal PkgNum = 0.0M;
                            string PkgUnit = string.Empty;
                            string PkgUnitDesc = string.Empty;
                            string GroupId = Prolink.Math.GetValueAsString(dndr["GROUP_ID"]);
                            string Odnuid = string.Empty;
                            string refNo = Prolink.Math.GetValueAsString(dndr["REF_NO"]);
                            string InvNo = string.IsNullOrEmpty(refNo) ? DnNos[i].Substring(4, DnNos[i].Length - 4) : refNo.Substring(4, refNo.Length - 4);
                            InvNo = InvNo.TrimStart(new char[] { '0' });

                            if (!invoicelist.Contains(InvNo))
                            {
                                if (!string.IsNullOrEmpty(InvNo))
                                    invoicelist.Add(InvNo);
                            }
                            sql = "SELECT * FROM SMBD WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND DN_NO=" + SQLUtils.QuotedStr(DnNos[i]);
                            DataTable bddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (bddt.Rows.Count > 0)
                            {
                                foreach (DataRow bddr in bddt.Rows)
                                {
                                    Nw = Prolink.Math.GetValueAsDecimal(bddr["NW"]);
                                    Gw = Prolink.Math.GetValueAsDecimal(bddr["GW"]);
                                    Cbm = Prolink.Math.GetValueAsDecimal(bddr["CBM"]);
                                    Gwu = Prolink.Math.GetValueAsString(bddr["GWU"]);
                                    Qty = Prolink.Math.GetValueAsDecimal(bddr["QTY"]);
                                    Qtyu = Prolink.Math.GetValueAsString(bddr["QTYU"]);
                                    PkgNum = Prolink.Math.GetValueAsDecimal(bddr["PKG_NUM"]);
                                    PkgUnit = Prolink.Math.GetValueAsString(bddr["PKG_UNIT"]);
                                    sql = "SELECT AP_CD FROM BSCODE WHERE CD_TYPE='TCNT' AND (CMP='*' OR CMP=" + SQLUtils.QuotedStr(Location) + ")";
                                    PkgUnit = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    PkgUnitDesc = Prolink.Math.GetValueAsString(bddr["PKG_UNIT_DESC"]);
                                    Odnuid = Prolink.Math.GetValueAsString(bddr["U_ID"]);
                                }
                            }
                            string OthCntType = Prolink.Math.GetValueAsString(dndr["CNT_TYPE"]);
                            string OthCntNum = Prolink.Math.GetValueAsString(dndr["CNT_NUMBER"]);
                            string OCntType = Prolink.Math.GetValueAsString(dndr["OCNT_TYPE"]);
                            string OCntNum = Prolink.Math.GetValueAsString(dndr["OCNT_NUMBER"]);
                            string DivisionDescp = Prolink.Math.GetValueAsString(dndr["DIVISION_DESCP"]);
                            if (!productinfo.Contains(DivisionDescp))
                            {
                                if (!string.IsNullOrEmpty(DivisionDescp))
                                    productinfo.Add(DivisionDescp);
                            }


                            sql = "SELECT TOP 1 DN_NO FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND DN_NO LIKE '" + DnNos[i] + "_' ORDER BY DN_NO DESC";
                            string dnno = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (dnno == "")
                            {
                                dnno = DnNos[i] + "A";
                            }
                            else
                            {
                                string p_dnno = dnno.Substring(dnno.Length - 1);
                                p_dnno += 1;
                                dnno = DnNos[i] + p_dnno;
                            }

                            EditInstruct ei;
                            ei = new EditInstruct("SMIDN", EditInstruct.INSERT_OPERATION);
                            if ("T".Equals(tranType))
                            {
                                string msg = DestInfoUpdate(ShipmentId, Location, ei);
                                if (msg != "success")
                                    return msg;
                            }
                            string DnUId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", DnUId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", Location);
                            ei.Put("STN", "*");
                            ei.Put("DEP", "*");
                            ei.Put("SHIPMENT_ID", ShipmentId);
                            ei.Put("DN_NO", dnno);
                            ei.Put("INV_NO", InvNo);
                            ei.Put("IMPORT_NO", DnImportNo);
                            ei.Put("TC_IMPORT_NO", DnImportNo);
                            ei.Put("NW", Nw);
                            ei.Put("GW", Gw);
                            ei.Put("GWU", Gwu);
                            ei.Put("CBM", Cbm);
                            ei.Put("QTY", Qty);
                            ei.Put("QTYU", Qtyu);
                            ei.Put("PKG_NUM", PkgNum);
                            ei.Put("PKG_UNIT", PkgUnit);
                            ei.Put("PKG_UNIT_DESC", PkgUnitDesc);
                            ei.Put("OTH_CNT_TYPE", OthCntType);
                            ei.Put("OTH_CNT_NUM", OthCntNum);
                            if (_isout)
                            {
                                ei.Put("OCNT_TYPE", OCntType);
                                ei.Put("OCNT_NUMBER", OCntNum);
                            }
                            ei.Put("DIVISION_DESCP", DivisionDescp);
                            ei.Put("CNTRY_CD", CntryCd);
                            ei.Put("TC_CNTRY_CD", CntryCd);
                            SetSmind(dndr, ei);
                            //ei.Put("PRIORITY", "2");
                            mixList.Add(ei);

                            sql = "SELECT * FROM SMBDD WHERE U_FID=" + SQLUtils.QuotedStr(Odnuid);
                            DataTable bdddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                            sql = "SELECT CNTR_NO,SEAL_NO1,SEAL_NO2 FROM SMRV WHERE STATUS='O' AND DN_NO LIKE '%{0}%'";
                            sql = string.Format(sql, DnNos[i]);
                            DataTable cntrdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                            if (bdddt.Rows.Count > 0 && cntrdt.Rows.Count == 1)
                            {
                                foreach (DataRow bdddr in bdddt.Rows)
                                {
                                    ei = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
                                    string DnpUId = System.Guid.NewGuid().ToString();
                                    ei.Put("U_ID", DnpUId);
                                    ei.Put("U_FID", DnUId);
                                    ei.Put("DN_NO", DnNos[i]);
                                    ei.Put("REF_NO", GetRefNo(DnNos[i]));
                                    ei.Put("INV_NO", InvNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.Put("IPART_NO", Prolink.Math.GetValueAsString(bdddr["IPART_NO"]));
                                    string opart = Prolink.Math.GetValueAsString(bdddr["OPART_NO"]);
                                    ei.Put("OPART_NO", opart);
                                    if (!opartno.Contains(opart))
                                        opartno.Add(opart);
                                    ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(bdddr["GOODS_DESCP"]));
                                    ei.Put("GW", Prolink.Math.GetValueAsDecimal(bdddr["GW"]));
                                    ei.Put("CBM", Prolink.Math.GetValueAsDecimal(bdddr["CBM"]));
                                    ei.Put("QTY", Prolink.Math.GetValueAsDecimal(bdddr["QTY"]));
                                    ei.Put("CNTR_NO", Prolink.Math.GetValueAsString(cntrdt.Rows[0]["CNTR_NO"]));
                                    ei.Put("SEAL_NO1", Prolink.Math.GetValueAsString(cntrdt.Rows[0]["SEAL_NO1"]));
                                    ei.Put("SEAL_NO2", Prolink.Math.GetValueAsString(cntrdt.Rows[0]["SEAL_NO2"]));

                                    mixList.Add(ei);
                                }
                            }
                            else if (bdddt.Rows.Count == 1 && cntrdt.Rows.Count > 0)
                            {
                                foreach (DataRow cntrdr in cntrdt.Rows)
                                {
                                    ei = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
                                    string DnpUId = System.Guid.NewGuid().ToString();
                                    ei.Put("U_ID", DnpUId);
                                    ei.Put("U_FID", DnUId);
                                    ei.Put("DN_NO", DnNos[i]);
                                    ei.Put("REF_NO", GetRefNo(DnNos[i]));
                                    ei.Put("INV_NO", InvNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.Put("IPART_NO", Prolink.Math.GetValueAsString(bdddt.Rows[0]["IPART_NO"]));
                                    string opart = Prolink.Math.GetValueAsString(bdddt.Rows[0]["OPART_NO"]);
                                    ei.Put("OPART_NO", opart);
                                    if (!opartno.Contains(opart))
                                        opartno.Add(opart);
                                    ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(bdddt.Rows[0]["GOODS_DESCP"]));
                                    ei.Put("GW", Prolink.Math.GetValueAsDecimal(bdddt.Rows[0]["GW"]));
                                    ei.Put("CBM", Prolink.Math.GetValueAsDecimal(bdddt.Rows[0]["CBM"]));
                                    ei.Put("QTY", Prolink.Math.GetValueAsDecimal(bdddt.Rows[0]["QTY"]));
                                    ei.Put("CNTR_NO", Prolink.Math.GetValueAsString(cntrdr["CNTR_NO"]));
                                    ei.Put("SEAL_NO1", Prolink.Math.GetValueAsString(cntrdr["SEAL_NO1"]));
                                    ei.Put("SEAL_NO2", Prolink.Math.GetValueAsString(cntrdr["SEAL_NO2"]));


                                    mixList.Add(ei);
                                }
                            }
                            else
                            {
                                foreach (DataRow bdddr in bdddt.Rows)
                                {
                                    ei = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
                                    string DnpUId = System.Guid.NewGuid().ToString();
                                    ei.Put("U_ID", DnpUId);
                                    ei.Put("U_FID", DnUId);
                                    ei.Put("DN_NO", DnNos[i]);
                                    ei.Put("REF_NO", GetRefNo(DnNos[i]));
                                    ei.Put("INV_NO", InvNo);
                                    ei.Put("SHIPMENT_ID", ShipmentId);
                                    ei.Put("IPART_NO", Prolink.Math.GetValueAsString(bdddr["IPART_NO"]));
                                    string opart = Prolink.Math.GetValueAsString(bdddr["OPART_NO"]);
                                    ei.Put("OPART_NO", opart);
                                    if (!opartno.Contains(opart))
                                        opartno.Add(opart);
                                    ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(bdddr["GOODS_DESCP"]));
                                    ei.Put("GW", Prolink.Math.GetValueAsDecimal(bdddr["GW"]));
                                    ei.Put("CBM", Prolink.Math.GetValueAsDecimal(bdddr["CBM"]));
                                    ei.Put("QTY", Prolink.Math.GetValueAsDecimal(bdddr["QTY"]));

                                    mixList.Add(ei);
                                }

                            }
                        }
                    }
                }
            }
            return returnmsg;
        }

        public static bool IsChangeToLCL(string[] combinedns)
        {
            string sql = string.Format("SELECT DISTINCT CNTR_NO FROM CONTAINER_DNINFO WHERE DN_NO IN {0}", SQLUtils.Quoted(combinedns));
            DataTable cntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in cntrDt.Rows)
            {
                sql = string.Format("SELECT COUNT(1) FROM CONTAINER_DNINFO WHERE CNTR_NO={0} AND DN_NO NOT IN {1}",
                    SQLUtils.QuotedStr(dr["CNTR_NO"].ToString()), SQLUtils.Quoted(combinedns));
                int counts = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (counts > 0)
                    return true;
            }
            return false;
        }

        public static void UpdatePartyToSMSMI(string ShipmentId,string tranType, MixedList ml = null)
        {
            List<string> tranTypeList = new List<string>() { "F", "L", "A", "R", "E" };
            string sql = "SELECT PARTY_NO, PARTY_NAME,PARTY_TYPE,(SELECT TOP 1 DEP FROM SMPTY WHERE SMPTY.PARTY_NO=SMSMIPT.PARTY_NO) AS DEP FROM SMSMIPT WHERE PARTY_TYPE IN ('SH','CS','NT','WE','FC','IBSP','IBBR','IBCR','IBLP','IBGV','IBTC','ZT','NT','SP') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable pdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string PartyNo = string.Empty;
            if (pdt.Rows.Count > 0)
            {
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", ShipmentId);
                DataRow[] ibtcrows = pdt.Select("PARTY_TYPE='IBTC'");
                if (ibtcrows.Length > 0)
                {
                    ei.Put("IS_TRANSIT_BROKER", "Y");
                }
                else
                {
                    ei.Put("IS_TRANSIT_BROKER", "");
                }

                foreach (DataRow pdr in pdt.Rows)
                {
                    string PartyType = Prolink.Math.GetValueAsString(pdr["PARTY_TYPE"]);
                    PartyNo = Prolink.Math.GetValueAsString(pdr["PARTY_NO"]);
                    string PartyName = Prolink.Math.GetValueAsString(pdr["PARTY_NAME"]);
                    if (PartyType == "SH")
                    {
                        ei.Put("SH_NO", PartyNo);
                        ei.Put("SH_NM", PartyName);
                    }

                    if (PartyType == "CS")
                    {
                        ei.Put("CS_NO", PartyNo);
                        ei.Put("CS_NM", PartyName);
                    }

                    if (PartyType == "WE")
                    {
                        ei.Put("WE_NO", PartyNo);
                        ei.Put("WE_NM", PartyName);
                    }

                    if (PartyType == "FC")
                    {
                        ei.Put("FC_NO", PartyNo);
                        ei.Put("FC_NM", PartyName);
                    }

                    if (PartyType == "NT")
                    {
                        ei.Put("NT_NO", PartyNo);
                        ei.Put("NT_NM", PartyName);
                    }

                    if (PartyType == "IBSP")
                    {
                        ei.Put("IBSP_NO", PartyNo);
                        ei.Put("IBSP_NM", PartyName);
                    }

                    if (PartyType == "IBBR")
                    {
                        ei.Put("IBBR_NO", PartyNo);
                        ei.Put("IBBR_NM", PartyName);
                    }

                    if (PartyType == "IBCR")
                    {
                        ei.Put("IBCR_NO", PartyNo);
                        ei.Put("IBCR_NM", PartyName);
                    }

                    if (PartyType == "IBLP")
                    {
                        ei.Put("IBLP_NO", PartyNo);
                        ei.Put("IBLP_NM", PartyName);
                    }

                    if (PartyType == "ZT")
                    {
                        ei.Put("ZT_NO", PartyNo);
                        ei.Put("BU", Prolink.Math.GetValueAsString(pdr["DEP"]));
                        ei.Put("ZT_NM", PartyName);
                    }

                    if (PartyType == "IBGV")
                    {
                        ei.Put("IBGV_NO", PartyNo);
                        ei.Put("IBGV_NM", PartyName);
                    }

                    if (PartyType == "SP"&& tranTypeList.Contains(tranType))
                    {
                        ei.Put("LSP_NO", PartyNo);
                        ei.Put("LSP_NM", PartyName);
                    }
                }
                if (ml == null)
                {
                    OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                else
                    ml.Add(ei);
            }
        }

        public static void UpdatePartyToSMSMINew(string ShipmentId, string tranType, MixedList ml = null)
        {
            bool update = false;
            if (ml == null)
            {
                ml = new MixedList();
                update = true;
            }
            List<string> partyTypeList = new List<string>() { "SH", "CS", "NT", "WE", "FC", "IBSP", "IBBR", "IBCR", "IBLP", "IBGV", "IBTC", "ZT", "NT", "SP" };
            List<string> tranTypeList = new List<string>() { "F", "L", "A", "R", "E" };
            string sql = string.Format("SELECT PARTY_NO, PARTY_NAME,PARTY_TYPE FROM SMSMIPT WHERE PARTY_TYPE IN {0} AND SHIPMENT_ID={1}",
                SQLUtils.Quoted(partyTypeList.ToArray()), SQLUtils.QuotedStr(ShipmentId));
            DataTable pdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", ShipmentId);
            EditInstruct ordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            ordei.PutKey("SHIPMENT_ID", ShipmentId);
            ordei.Put("LSP_NO", DBNull.Value);
            ordei.Put("LSP_NO", DBNull.Value);
            ei.Put("IS_TRANSIT_BROKER", DBNull.Value);
            foreach (string type in partyTypeList)
            {
                switch(type)
                {
                    case "IBTC":
                        break;
                    case "SP":
                        ei.Put("LSP_NO", DBNull.Value);
                        ei.Put("LSP_NM", DBNull.Value);
                        break;
                    default:
                        ei.Put(type + "_NO", DBNull.Value);
                        ei.Put(type + "_NM", DBNull.Value);
                        break;
                }
                
            }
            foreach (DataRow pdr in pdt.Rows)
            {
                string PartyType = Prolink.Math.GetValueAsString(pdr["PARTY_TYPE"]).ToUpper();
                string PartyNo = Prolink.Math.GetValueAsString(pdr["PARTY_NO"]);
                string PartyName = Prolink.Math.GetValueAsString(pdr["PARTY_NAME"]);
                switch (PartyType)
                {
                    case "SP":
                        if (!tranTypeList.Contains(tranType))
                            break;
                        ei.Put("LSP_NO", PartyNo);
                        ei.Put("LSP_NM", PartyName);
                        ordei.Put("LSP_NO", PartyNo);
                        ordei.Put("LSP_NM", PartyName);
                        break;
                    case "IBTC":
                        ei.Put("IS_TRANSIT_BROKER", "Y");
                        break;
                    case "ZT": 
                        ei.Put("ZT_NO", PartyNo);
                        ei.Put("BU", getPartyColByNo("DEP", PartyNo));
                        ei.Put("ZT_NM", PartyName);
                        break;
                    default:
                        ei.Put(PartyType + "_NO", PartyNo);
                        ei.Put(PartyType + "_NM", PartyName);
                        break;
                }
            }
            ml.Add(ei);
            ml.Add(ordei);


            if (ml.Count>0&&update)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                { 
                
                }
            }

        }

        public static DataTable GetDnInfos(string shipmentid, string combineinfo, string issplitbill)
        {
            string condition = string.Format("(SELECT SHIPMENT_ID FROM SMSM WHERE SHIPMENT_ID={0} OR COMBIN_SHIPMENT=REPLACE({0},'MHK_',''))", SQLUtils.QuotedStr(shipmentid));
            string sql = string.Format(@";WITH REF_DATA AS(
SELECT SMDN.U_ID,SMDN.GROUP_ID,SMDN.CMP,SMDN.DN_NO,SMDN.REF_NO,SMDN.DN_NO_CMP_REF FROM SMDN WHERE SHIPMENT_ID IN {0}
UNION ALL
SELECT SMDN.U_ID,SMDN.GROUP_ID,SMDN.CMP,SMDN.DN_NO,SMDN.REF_NO,SMDN.DN_NO_CMP_REF FROM SMDN INNER JOIN REF_DATA R ON   R.REF_NO = SMDN.DN_NO AND R.DN_NO <> SMDN.DN_NO
)
SELECT U_ID,GROUP_ID,CMP,SUBSTRING(DN_NO,7,8) AS DN_NO FROM REF_DATA 
UNION select U_ID,GROUP_ID,CMP,SUBSTRING(SMINM.DN_NO, 7,8)AS DN_NO FROM SMINM WHERE  SHIPMENT_ID IN {0}
UNION SELECT U_ID,GROUP_ID,CMP,DN_NO FROM SMSM  WHERE  SHIPMENT_ID IN {0}", condition);

            if ("S".Equals(issplitbill))
            {
                string[] dns = combineinfo.Split(',');
                sql += string.Format("UNION SELECT U_ID,GROUP_ID,CMP,DN_NO FROM SMDN WHERE DN_NO IN {0}", SQLUtils.Quoted(dns));
            }
            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Dt;
        }

        #endregion

        public static int getOneValueAsIntFromSql(string sql)
        {
            return OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static string getOneValueAsStringFromSql(string sql)
        {
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static DataTable getDataTableFromSql(string sql)
        {
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static string insertParty(DataRow[] drs, string suid, string TranType, string ShipmentId, string PodCd, string Podnm, DateTime eta, string cmp)
        {
            string sql = string.Empty;
            string returnMessage = "success";
            MixedList ml = new MixedList();
            Dictionary<string, List<string>> partydictionary = new Dictionary<string, List<string>>();
            foreach (DataRow dr in drs)
            {
                string partyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                string LspCd = Prolink.Math.GetValueAsString(dr["LSP_CD"]);
                int OrderBy = getOneValueAsIntFromSql("SELECT ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND CD=" + SQLUtils.QuotedStr(partyType));
                string DlvArea = Prolink.Math.GetValueAsString(dr["DLV_AREA"]);
                string DlvAreaNm = Prolink.Math.GetValueAsString(dr["DLV_AREA_NM"]);
                string AddrCode = Prolink.Math.GetValueAsString(dr["ADDR_CODE"]);
                string DlvAddr = Prolink.Math.GetValueAsString(dr["DLV_ADDR"]);
                string finalwh = Prolink.Math.GetValueAsString(dr["FINAL_WH"]);

                string TypeDescp = getOneValueAsStringFromSql("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD=" + SQLUtils.QuotedStr(partyType));
                sql = "SELECT COUNT(*) FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE={1}";
                sql = string.Format(sql, SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(partyType));
                int n = getOneValueAsIntFromSql(sql);
                if (n > 0)
                {
                    return returnMessage;// "Duplicated Party Type, Please check this";
                }
                if (!partydictionary.Keys.Contains(partyType))
                {
                    List<string> partynolist = new List<string>();
                    partynolist.Add(LspCd);
                    partydictionary.Add(partyType, partynolist);
                }
                else
                {
                    if (!partydictionary[partyType].Contains(LspCd))
                    {
                        partydictionary[partyType].Add(LspCd);
                    }
                    else
                    {
                        continue;
                    }
                }

                sql = @"INSERT INTO SMSMIPT
                                    (
                                        U_ID,U_FID,SHIPMENT_ID,PARTY_TYPE,
                                        TYPE_DESCP,PARTY_NO,PARTY_NAME,PARTY_NAME2,
                                        PARTY_NAME3,PARTY_NAME4,PARTY_ADDR1,PARTY_ADDR2,
                                        PARTY_ADDR3,PARTY_ADDR4,PARTY_ADDR5,CNTY,
                                        CNTY_NM,CITY,CITY_NM,STATE,
                                        ZIP,PARTY_ATTN,PARTY_TEL,PARTY_MAIL,
                                        DEBIT_TO,FAX_NO,TAX_NO,ORDER_BY
                                    )
                                    SELECT TOP 1
                                    {0},{1},{2},{3},
                                    {4},PARTY_NO,PARTY_NAME,PARTY_NAME2,
                                    PARTY_NAME3,PARTY_NAME4,PART_ADDR1,PART_ADDR2,
                                    PART_ADDR3,PART_ADDR4,PART_ADDR5,CNTY,
                                    CNTY_NM,CITY,CITY_NM,STATE,
                                    ZIP,PARTY_ATTN,PARTY_TEL,PARTY_MAIL,
                                    BILL_TO,PARTY_FAX,TAX_NO,{5}
                                    FROM SMPTY WHERE STATUS='U' AND PARTY_NO={6}";
                string ptuid = System.Guid.NewGuid().ToString();
                sql = string.Format(sql, SQLUtils.QuotedStr(ptuid), SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(partyType), SQLUtils.QuotedStr(TypeDescp), OrderBy, SQLUtils.QuotedStr(LspCd));
                ml.Add(sql);

                if ("IBCR".Equals(partyType))
                {
                    string sql1 = string.Format("SELECT * FROM SMPTY WHERE STATUS='U' AND PARTY_NO={0}", SQLUtils.QuotedStr(LspCd));
                    DataTable smpty = getDataTableFromSql(sql1);
                    if (smpty.Rows.Count > 0)
                    {
                        string lspnm = smpty.Rows[0]["PARTY_NAME"].ToString();
                        EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("U_ID", suid);
                        ei.Put("TRUCKER1", LspCd);
                        ei.Put("TRUCKER_NM1", lspnm);
                        ml.Add(ei);
                        if ("F".Equals(TranType) || "R".Equals(TranType))
                        {
                            EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                            smicntrei.PutKey("SHIPMENT_ID", ShipmentId);
                            smicntrei.Put("TRUCKER1", LspCd);
                            smicntrei.Put("TRUCKER_NM1", lspnm);
                            smicntrei.Put("TRAN_TYPE1", "T");
                            smicntrei.Put("POL1", PodCd);
                            smicntrei.Put("POL_NM1", Podnm);
                            ml.Add(smicntrei);
                        }
                    }
                }

                UpdateSCMReqDate(suid, ShipmentId, PodCd, eta, ml, DlvArea);

                if (!"T".Equals(TranType))
                {
                    sql = string.Format("SELECT TOP 1 WS_CD,WS_NM FROM SMWH WHERE DLV_ADDR={0} AND CMP={1}", SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(cmp));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string wsCd = string.Empty;
                    string wsNm = string.Empty;
                    if (dt.Rows.Count > 0)
                    {
                        wsCd = Prolink.Math.GetValueAsString(dt.Rows[0]["WS_CD"]);
                        wsNm = Prolink.Math.GetValueAsString(dt.Rows[0]["WS_NM"]);
                    }
                    EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentId);
                    ei.Put("DLV_AREA", DlvArea);
                    ei.Put("DLV_AREA_NM", DlvAreaNm);
                    ei.Put("ADDR_CODE", AddrCode);
                    ei.Put("DLV_ADDR", DlvAddr);
                    ei.Put("FINAL_WH", finalwh);
                    ei.Put("WS_CD", wsCd);
                    ei.Put("WS_NM", wsNm);
                    ml.Add(ei);
                     
                    EditInstruct smicntrEi = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    smicntrEi.PutKey("SHIPMENT_ID", ShipmentId);
                    smicntrEi.Put("DLV_AREA", DlvArea);
                    smicntrEi.Put("DLV_AREA_NM", DlvAreaNm);
                    smicntrEi.Put("ADDR_CODE", AddrCode);
                    smicntrEi.Put("DLV_ADDR", DlvAddr);
                    smicntrEi.Put("FINAL_WH", finalwh);
                    smicntrEi.Put("WS_CD", wsCd);
                    smicntrEi.Put("WS_NM", wsNm);
                    ml.Add(smicntrEi);
                }

            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = "error";
                }
            }
            return returnMessage;
        }

        public static void UpdateSCMReqDate(string suid, string ShipmentId, string PodCd, DateTime eta, MixedList ml, string DlvArea)
        {
            string sql = string.Format("SELECT TOP 1 ADD_DATE FROM SCMREF WHERE POD LIKE '%" + PodCd + "%' AND S_POD LIKE '%" + DlvArea + "%' ORDER BY ADD_DATE DESC");
            int days = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (days > 0)
            {
                eta = eta.AddDays(days);
                int d = (int)eta.DayOfWeek;
                if (d == 0 || d == 6)
                {
                    if (d > 0)
                        eta = eta.AddDays(2);
                    else
                        eta = eta.AddDays(1);

                }
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", suid);
                ei.PutDate("PRODUCTION_DATE", eta);
                ml.Add(ei);
                EditInstruct smidnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                smidnei.PutKey("SHIPMENT_ID", ShipmentId);
                smidnei.PutDate("SCMREQUEST_DATE", eta);
                ml.Add(smidnei);
                EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                smicntrei.PutKey("SHIPMENT_ID", ShipmentId);
                smicntrei.PutDate("SCMREQUEST_DATE", eta);
                ml.Add(smicntrei);
            }
        }

        public static string insertPartyIBTW(DataTable dt1, string suid, string ShipmentId)
        {
            string sql = string.Empty;
            string returnMessage = "success";
            MixedList ml = new MixedList();
            int k = 0;
            if (dt1.Rows.Count > 0)
            {
                foreach (DataRow dr in dt1.Rows)
                {
                    string CustomerCode = Prolink.Math.GetValueAsString(dr["CUSTOMER_CODE"]);
                    string TypeDescp = getOneValueAsStringFromSql("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD='IBTW'");

                    sql = "SELECT COUNT(*) FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE='IBTW'";
                    sql = string.Format(sql, SQLUtils.QuotedStr(suid));

                    int n = getOneValueAsIntFromSql(sql);

                    if (n > 0)
                    {
                        sql = "SELECT COUNT(*) FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE='IBTW' AND PARTY_NO!={1}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(CustomerCode));
                        int m = getOneValueAsIntFromSql(sql);
                        if (m > 0)
                        {
                            sql = "DELETE FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE='IBTW' AND PARTY_NO!={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(CustomerCode));
                            ml.Add(sql);
                        }
                    }
                    else
                    {
                        sql = @"INSERT INTO SMSMIPT
                                    (
                                        U_ID,
                                        U_FID,
                                        SHIPMENT_ID,
                                        PARTY_TYPE,
                                        TYPE_DESCP,
                                        PARTY_NO,
                                        PARTY_NAME,
                                        PARTY_NAME2,
                                        PARTY_NAME3,
                                        PARTY_NAME4,
                                        PARTY_ADDR1,
                                        PARTY_ADDR2,
                                        PARTY_ADDR3,
                                        PARTY_ADDR4,
                                        PARTY_ADDR5,
                                        CNTY,
                                        CNTY_NM,
                                        CITY,
                                        CITY_NM,
                                        STATE,
                                        ZIP,
                                        PARTY_ATTN,
                                        PARTY_TEL,
                                        PARTY_MAIL,
                                        DEBIT_TO,
                                        FAX_NO,
                                        TAX_NO
                                    )
                                    SELECT 
                                    {0},
                                    {1},
                                    {2},
                                    'IBTW',
                                    {3},
                                    PARTY_NO,
                                    PARTY_NAME,
                                    PARTY_NAME2,
                                    PARTY_NAME3,
                                    PARTY_NAME4,
                                    PART_ADDR1,
                                    PART_ADDR2,
                                    PART_ADDR3,
                                    PART_ADDR4,
                                    PART_ADDR5,
                                    CNTY,
                                    CNTY_NM,
                                    CITY,
                                    CITY_NM,
                                    STATE,
                                    ZIP,
                                    PARTY_ATTN,
                                    PARTY_TEL,
                                    PARTY_MAIL,
                                    BILL_TO,
                                    PARTY_FAX,
                                    TAX_NO
                                    FROM SMPTY WHERE STATUS='U' AND PARTY_NO={4}";

                        string ptuid = System.Guid.NewGuid().ToString();
                        sql = string.Format(sql, SQLUtils.QuotedStr(ptuid), SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(TypeDescp), SQLUtils.QuotedStr(CustomerCode));
                        ml.Add(sql);

                        k++;
                    }
                }
            }

            if (k > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = "error:" + ex;
                }
            }

            return returnMessage;
        }

        public static string InboundAllcByShipment(string shipmentid)
        {
            DataTable dt = SmsmiTable(string.Empty, shipmentid);
            return InboundAllocation(dt);
        }

        public static DataTable SmsmiTable(string uid, string shipmentId)
        {
            string condition = string.IsNullOrEmpty(uid) ? $"SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)}" : $"U_ID={SQLUtils.QuotedStr(uid)}";
            string sql = @"SELECT CMP,U_ID,ETA,SHIPMENT_ID,TRAN_TYPE,FRT_TERM,INCOTERM_CD,INCOTERM_DESCP,POD_CD,POD_NAME,TERMINAL_CD,CARRIER,
                (SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SHIPMENT_ID=SMSMI.SHIPMENT_ID AND PARTY_TYPE='CS') AS CONN_CD,
                (SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SHIPMENT_ID=SMSMI.SHIPMENT_ID AND PARTY_TYPE='WE') AS SMSM_WE_NO FROM SMSMI WHERE " + condition;
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static string InboudAllcBySuid(string uid)
        {
            string message = string.Empty;
            DataTable dt = SmsmiTable(uid, string.Empty);
            message = InboundAllocation(dt);
            if (dt.Rows.Count > 0)
            {
                string tranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
                string shipmentId = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                TrackingEDI.InboundBusiness.InboundHelper.UpdatePartyToSMSMINew(shipmentId, tranType);
            }
            return message;
        }

        public static void SetPartyToIBCRByShipID(string shipmentid)
        {
            string sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string Pol1 = dt.Rows[0]["POL1"].ToString();
            string cmp = dt.Rows[0]["CMP"].ToString();
            sql = string.Format(@"SELECT BSADDR.ADDR_CODE,BSADDR.ADDR FROM BSADDR,BSTPORT WHERE BSTPORT.Port_Cd=BSADDR.Port_Cd
                AND BSTPORT.Cntry_Cd=BSADDR.Cntry_Cd AND BSTPORT.CMP={0} and BSTPORT.port_cd={1} ORDER BY ADDR_CODE ASC",
                  SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(Pol1));
            DataTable portDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (portDt.Rows.Count > 0)
            {
                string addrcd = portDt.Rows[0]["ADDR_CODE"].ToString();
                string addr = portDt.Rows[0]["ADDR"].ToString();
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("DEP_ADDR_CD1", addrcd);
                ei.Put("DEP_ADDR1", addr);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());

                ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("DEP_ADDR1", addr);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            SetPartyToIBCR(dt);
        }

        public static string InboundAllocation(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
                string FrtTerm = Prolink.Math.GetValueAsString(dt.Rows[0]["FRT_TERM"]);
                string ConnCd = Prolink.Math.GetValueAsString(dt.Rows[0]["CONN_CD"]);
                string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
                string Podnm = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_NAME"]);
                string IncotermCd = Prolink.Math.GetValueAsString(dt.Rows[0]["INCOTERM_CD"]);
                string IncotermDescp = Prolink.Math.GetValueAsString(dt.Rows[0]["INCOTERM_DESCP"]);
                string ShipmentId = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                string TerminalCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TERMINAL_CD"]);
                string suid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
                string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]); 
                DateTime eta = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETA"]);
                string carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
                string weCd = Prolink.Math.GetValueAsString(dt.Rows[0]["SMSM_WE_NO"]);
                 
                string con = string.Format(" AND CMP={0}", SQLUtils.QuotedStr(cmp));
                if (TerminalCd != "")
                {
                    con += " AND TERMINAL_CD=" + SQLUtils.QuotedStr(TerminalCd);
                }


                string returnMessage = string.Empty;


                InputIBISParty(TranType, ShipmentId, suid, cmp);


                string smalSql = string.Format(@"SELECT LSP_CD, PARTY_TYPE, DLV_AREA, DLV_AREA_NM, ADDR_CODE,DLV_ADDR,CONN_CD,INCOTERM_CD,INCOTERM_DESCP,CARRIER,WE_CD,
                (SELECT TOP 1 FINAL_WH FROM BSADDR WHERE BSADDR.CMP={0} AND BSADDR.ADDR_CODE=ADDR_CODE) AS FINAL_WH FROM SMAL WHERE TRAN_TYPE={1} AND FREIGHT_TERM={2} AND POD_CD={3} {4}",
                SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(PodCd), con);

                DataTable dt1 = getDataTableFromSql(smalSql);


                DataRow[] drs = dt1.Select(string.Format(@"CONN_CD={0} AND INCOTERM_CD={1} AND INCOTERM_DESCP={2} AND CARRIER={3} AND WE_CD={4}",
                    SQLUtils.QuotedStr(ConnCd), SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp), SQLUtils.QuotedStr(carrier),SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                } 
                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL)  AND INCOTERM_CD={0} AND INCOTERM_DESCP={1} AND CARRIER={2} AND WE_CD={3}",
                   SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"CONN_CD={0} AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL) AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL) AND CARRIER={1} AND WE_CD={2}",
                   SQLUtils.QuotedStr(ConnCd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"CONN_CD={0} AND INCOTERM_CD={1} AND INCOTERM_DESCP={2} AND (CARRIER='' OR CARRIER IS NULL) AND WE_CD={3}",
                  SQLUtils.QuotedStr(ConnCd), SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp), SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"CONN_CD={0} AND INCOTERM_CD={1} AND INCOTERM_DESCP={2} AND CARRIER={3} AND (WE_CD='' OR WE_CD IS NULL)",
                  SQLUtils.QuotedStr(ConnCd), SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp), SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL) AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL) AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL) AND CARRIER={0} AND WE_CD={1}",
                   SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL) AND INCOTERM_CD={0} AND INCOTERM_DESCP={1} AND (CARRIER='' OR CARRIER IS NULL) AND WE_CD={2}",
                   SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp), SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL) AND INCOTERM_CD={0} AND INCOTERM_DESCP={1} AND CARRIER={2} AND (WE_CD='' OR WE_CD IS NULL)",
                SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp), SQLUtils.QuotedStr(carrier)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"CONN_CD={0} AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL) AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL) AND (CARRIER='' OR CARRIER IS NULL) AND (WE_CD='' OR WE_CD IS NULL)",
                   SQLUtils.QuotedStr(ConnCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL) AND INCOTERM_CD={0} AND INCOTERM_DESCP={1} AND (CARRIER='' OR CARRIER IS NULL) AND (WE_CD='' OR WE_CD IS NULL)",
                SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL) AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL) AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL) AND CARRIER={0} AND (WE_CD='' OR WE_CD IS NULL)",
                SQLUtils.QuotedStr(carrier)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }

                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL) AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL) AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL) AND (CARRIER='' OR CARRIER IS NULL) AND WE_CD={0}",
                SQLUtils.QuotedStr(weCd)));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }

                drs = dt1.Select(string.Format(@"(CONN_CD='' OR CONN_CD IS NULL) AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL) AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL) AND (CARRIER='' OR CARRIER IS NULL) AND (WE_CD='' OR WE_CD IS NULL)"));
                if (drs.Length > 0)
                {
                    returnMessage = insertParty(drs, suid, TranType, ShipmentId, PodCd, Podnm, eta, cmp);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                } 
                InputIBTWParty(TranType, ShipmentId, suid);
            }
            return "success";
        }


        public static void InputIBISParty(string TranType, string ShipmentId, string suid, string cmp)
        {
            string sql = string.Format("SELECT SHIPMENT_ID,IORDER,ISNULL(TRANSACTION_NO,(SELECT TOP 1 TRANSACTION_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMSMI.SHIPMENT_ID)) AS TRANSACTION_NO FROM SMSMI where SHIPMENT_ID={0}",
               SQLUtils.QuotedStr(ShipmentId));
            DataTable smsmidt = getDataTableFromSql(sql);
            if (smsmidt.Rows.Count <= 0)
                return;

            string transactionno = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["TRANSACTION_NO"]);
            string Iorder = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["IORDER"]);

            if ((!"Y".Equals(Iorder)) || string.IsNullOrEmpty(transactionno))
                return;
            sql = string.Format("SELECT DISTINCT AP_CD AS CUSTOMER_CODE FROM BSCODE WHERE CD_TYPE='IBIS' AND CD ={0} and (CMP='*' OR CMP={0})",
                SQLUtils.QuotedStr(cmp));
            DataTable dt = getDataTableFromSql(sql);
            if (dt.Rows.Count > 0)
            {
                insertPartyByPartType(dt, suid, ShipmentId, "IBIS");
            }
        }

        public static string insertPartyByPartType(DataTable partydt, string suid, string ShipmentId, string parttype)
        {
            string sql = string.Empty;
            string returnMessage = "success";
            MixedList ml = new MixedList();
            if (partydt.Rows.Count > 0)
            {
                foreach (DataRow dr in partydt.Rows)
                {
                    string CustomerCode = Prolink.Math.GetValueAsString(dr["CUSTOMER_CODE"]);
                    string TypeDescp = getOneValueAsStringFromSql(string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD={0}", SQLUtils.QuotedStr(parttype)));

                    sql = "SELECT COUNT(*) FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(parttype));

                    int n = getOneValueAsIntFromSql(sql);

                    if (n > 0)
                    {
                        sql = "SELECT COUNT(*) FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE={1} AND PARTY_NO={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(parttype), SQLUtils.QuotedStr(CustomerCode));
                        int m = getOneValueAsIntFromSql(sql);
                        if (m > 0)
                        {
                            sql = "DELETE FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE={1} AND PARTY_NO={2}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(parttype), SQLUtils.QuotedStr(CustomerCode));
                            ml.Add(sql);
                        }
                    }

                    sql = @"INSERT INTO SMSMIPT
                                    (
                                        U_ID,
                                        U_FID,
                                        SHIPMENT_ID,
                                        PARTY_TYPE,
                                        TYPE_DESCP,
                                        PARTY_NO,
                                        PARTY_NAME,
                                        PARTY_NAME2,
                                        PARTY_NAME3,
                                        PARTY_NAME4,
                                        PARTY_ADDR1,
                                        PARTY_ADDR2,
                                        PARTY_ADDR3,
                                        PARTY_ADDR4,
                                        PARTY_ADDR5,
                                        CNTY,
                                        CNTY_NM,
                                        CITY,
                                        CITY_NM,
                                        STATE,
                                        ZIP,
                                        PARTY_ATTN,
                                        PARTY_TEL,
                                        PARTY_MAIL,
                                        DEBIT_TO,
                                        FAX_NO,
                                        TAX_NO
                                    )
                                    SELECT 
                                    {0},
                                    {1},
                                    {2},
                                    {3},
                                    {4},
                                    PARTY_NO,
                                    PARTY_NAME,
                                    PARTY_NAME2,
                                    PARTY_NAME3,
                                    PARTY_NAME4,
                                    PART_ADDR1,
                                    PART_ADDR2,
                                    PART_ADDR3,
                                    PART_ADDR4,
                                    PART_ADDR5,
                                    CNTY,
                                    CNTY_NM,
                                    CITY,
                                    CITY_NM,
                                    STATE,
                                    ZIP,
                                    PARTY_ATTN,
                                    PARTY_TEL,
                                    PARTY_MAIL,
                                    BILL_TO,
                                    PARTY_FAX,
                                    TAX_NO
                                    FROM SMPTY WHERE STATUS='U' AND PARTY_NO={5}";

                    string ptuid = System.Guid.NewGuid().ToString();
                    sql = string.Format(sql, SQLUtils.QuotedStr(ptuid), SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(parttype), SQLUtils.QuotedStr(TypeDescp), SQLUtils.QuotedStr(CustomerCode));
                    ml.Add(sql);
                }
            }

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = "error:" + ex;
                }
            }
            return returnMessage;
        }


        /// <summary>
        /// 根据规则将IBTW 写入到Party中
        /// </summary>
        /// <param name="TranType"></param>
        /// <param name="ShipmentId"></param>
        /// <param name="suid"></param>
        public static void InputIBTWParty(string TranType, string ShipmentId, string suid)
        {
            string sql = string.Empty;
            DataTable dt1 = new DataTable();
            if ("F".Equals(TranType) || "R".Equals(TranType))
            {
                sql = "SELECT DISTINCT b.CUSTOMER_CODE FROM SMICNTR as a, BSADDR as b WHERE a.SHIPMENT_ID={0} AND A.CMP=B.CMP AND A.ADDR_CODE=b.ADDR_CODE AND b.OUTER_FLAG='Y'";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId));
                dt1 = getDataTableFromSql(sql);
                if (dt1.Rows.Count > 0)
                {
                    insertPartyIBTW(dt1, suid, ShipmentId);
                }
            }
            else
            {
                sql = "SELECT DISTINCT b.CUSTOMER_CODE FROM SMIDN as a, BSADDR as b WHERE a.SHIPMENT_ID={0} AND A.CMP=B.CMP AND a.ADDR_CODE=b.ADDR_CODE AND b.OUTER_FLAG='Y'";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId));
                dt1 = getDataTableFromSql(sql);
                if (dt1.Rows.Count > 0)
                {
                    insertPartyIBTW(dt1, suid, ShipmentId);
                }
            }
        }

        public static void SetPartyToIBCR(DataTable mainDt1)
        {
            if (mainDt1.Rows.Count > 0)
            {
                string ShipmentId = mainDt1.Rows[0]["SHIPMENT_ID"].ToString();
                string sql1 = "SELECT PARTY_TYPE, PARTY_NO,PARTY_NAME FROM SMSMIPT WHERE PARTY_TYPE='IBCR' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " ORDER BY PARTY_NO ASC";
                DataTable ptdt = getDataTableFromSql(sql1);
                string trantype = mainDt1.Rows[0]["TRAN_TYPE"].ToString();
                string u_id = mainDt1.Rows[0]["U_ID"].ToString();
                string trucker1 = Prolink.Math.GetValueAsString(mainDt1.Rows[0]["TRUCKER1"]);
                string trucker_nm1 = Prolink.Math.GetValueAsString(mainDt1.Rows[0]["TRUCKER_NM1"]);
                if (ptdt.Rows.Count <= 0)
                {
                    TrackingEDI.InboundBusiness.InboundHelper.UpdatePartyToSMSMINew(ShipmentId, trantype);
                }
                else
                {
                    string PartyNo = Prolink.Math.GetValueAsString(ptdt.Rows[0]["PARTY_NO"]);
                    string partyNm = Prolink.Math.GetValueAsString(ptdt.Rows[0]["PARTY_NAME"]);
                    MixedList mlist = new MixedList();
                    EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", u_id);
                    ei.Put("TRUCK_CD", PartyNo);
                    if (string.IsNullOrEmpty(trucker1))
                    {
                        ei.Put("TRUCKER1", PartyNo);
                        ei.Put("TRUCKER_NM1", partyNm);
                        mlist.Add(ei);
                    }
                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        sql1 = string.Format("UPDATE SMICNTR SET TRUCKER1={0},TRUCKER_NM1={1} WHERE SHIPMENT_ID={2} AND (TRUCKER1 IS NULL OR TRUCKER1='')",
                        SQLUtils.QuotedStr(PartyNo), SQLUtils.QuotedStr(partyNm), SQLUtils.QuotedStr(ShipmentId));
                        mlist.Add(sql1);
                    }

                    try
                    {
                        TrackingEDI.InboundBusiness.InboundHelper.UpdatePartyToSMSMINew(ShipmentId, trantype, mlist);
                        OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception)
                    {
                    }
                }
            }


        }

        #region 計算Insurance Fee和FOB
        public static string CalIFAndFob(string ShipmentId)
        {
            MixedList ml = new MixedList();
            string sql = "SELECT CMP, ETA, GVALUE, CUR, TRAN_TYPE,FREIGHT_AMT FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int k = 0;
            string returnMessage = "success";
            if (sdt.Rows.Count > 0)
            {
                EditInstruct ei;
                ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                EditInstruct smicntrei;
                smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                EditInstruct smidnei;
                smidnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                foreach (DataRow sdr in sdt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(sdr["CMP"]);
                    DateTime Eta = Prolink.Math.GetValueAsDateTime(sdr["ETA"]);
                    Decimal Gvalue = Prolink.Math.GetValueAsDecimal(sdr["GVALUE"]);
                    string cur = Prolink.Math.GetValueAsString(sdr["CUR"]);
                    string tran_type = Prolink.Math.GetValueAsString(sdr["TRAN_TYPE"]);
                    string Eta_y = Eta.Year.ToString();

                    sql = "SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TIR' AND CD=" + SQLUtils.QuotedStr(Eta_y) + " AND CMP=" + SQLUtils.QuotedStr(Cmp);
                    string Fee_s = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    Decimal Fee = 0;

                    if (Fee_s == "")
                    {
                        sql = "SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TIR' AND CMP='*' AND CD=" + SQLUtils.QuotedStr(Eta_y);
                        Fee = Prolink.Math.GetValueAsDecimal(OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection()));
                    }
                    else
                    {
                        Fee = Prolink.Math.GetValueAsDecimal(Fee_s);
                    }

                    sql = "SELECT CD FROM BSCODE WHERE CD_TYPE='LCUR' AND CMP=" + SQLUtils.QuotedStr(Cmp);
                    string Cur_L = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = "SELECT TOP 1 EX_RATE FROM BSERATE WHERE FCUR=" + SQLUtils.QuotedStr(cur) + " AND TCUR=" + SQLUtils.QuotedStr(Cur_L) + " order by EDATE desc";
                    Decimal FX = Prolink.Math.GetValueAsDecimal(OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection()));
                    if (cur != Cur_L)
                    {
                        Gvalue = Gvalue * FX;
                    }
                    Decimal i_amt = Gvalue * Fee;
                    ei.PutKey("SHIPMENT_ID", ShipmentId);
                    ei.Put("INSURANCE_AMT", i_amt);

                    Decimal OSF = Prolink.Math.GetValueAsDecimal(sdr["FREIGHT_AMT"]);
                    if (OSF == 0)
                    {
                        sql = string.Format("SELECT FREIGHT_AMT FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                        OSF = (Decimal)OperationUtils.GetValueAsFloat(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    string Cur_q = string.Empty;

                    if (cur != Cur_L)
                    {
                        OSF = OSF * FX;
                    }
                    Decimal FOB = Gvalue - i_amt - OSF;

                    if (tran_type == "F" || tran_type == "R")
                    {
                        smicntrei.PutKey("SHIPMENT_ID", ShipmentId);
                        smicntrei.Put("FOB_AMT", FOB);
                        ml.Add(smicntrei);
                    }
                    else
                    {
                        smidnei.PutKey("SHIPMENT_ID", ShipmentId);
                        smidnei.Put("FOB_AMT", FOB);
                        ml.Add(smidnei);
                    }
                    ml.Add(ei);
                    k++;
                }
            }

            if (k > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = "error:" + ex;
                }
            }

            return returnMessage;
        }
        #endregion

        public static string getPartyColByNo(string col, string partyNo)
        {
            string sql = string.Format("SELECT {0} FROM SMPTY WHERE PARTY_NO={1}", col, SQLUtils.QuotedStr(partyNo));
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void UpdateDetDueDate(string reserveNo, DateTime pickupDate, MixedList mixList)
        {
            if (pickupDate.CompareTo(new DateTime(2000, 1, 1)) <= 0)
                return;
            string sql = string.Format("SELECT SHIPMENT_ID,CNTR_NO,ORD_NO FROM SMRCNTR WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserveNo));
            if (string.IsNullOrEmpty(reserveNo))
                sql = string.Format("SELECT SHIPMENT_ID,CNTR_NO,ORD_NO FROM SMRCNTR WHERE ORD_NO={0}", SQLUtils.QuotedStr(reserveNo));
            DataTable rcntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, Tuple<string, int, int, int, string, string, string, Tuple<string>>> shipmentList = new Dictionary<string, Tuple<string, int, int, int, string, string, string, Tuple<string>>>();
            sql = string.Format("UPDATE SMRCNTR SET SMRCNTR.PICKUP_DATE={1} FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRCNTR.RESERVE_NO AND SMRCNTR.RESERVE_NO={0}",
                SQLUtils.QuotedStr(reserveNo), SQLUtils.QuotedStr(pickupDate.ToString("yyyy-MM-dd")));
            mixList.Add(sql);
            sql = string.Format("UPDATE SMICNTR SET SMICNTR.PICKUP_CDATE={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={0}"
                , SQLUtils.QuotedStr(reserveNo), SQLUtils.QuotedStr(pickupDate.ToString("yyyy-MM-dd")));
            mixList.Add(sql);

            sql = string.Format("UPDATE SMORD SET SMORD.PICKUP_CDATE={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO = SMORD.CNTR_NO AND SMRCNTR.SHIPMENT_ID = SMORD.SHIPMENT_ID AND SMRCNTR.RESERVE_NO ={0}"
                , SQLUtils.QuotedStr(reserveNo), SQLUtils.QuotedStr(pickupDate.ToString("yyyy-MM-dd")));
            mixList.Add(sql);

            sql = string.Format("UPDATE SMRDN SET SMRDN.PICKUP_DATE={1} FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRDN.RESERVE_NO AND SMRDN.RESERVE_NO={0}", SQLUtils.QuotedStr(reserveNo),
                SQLUtils.QuotedStr(pickupDate.ToString("yyyy-MM-dd")));
            mixList.Add(sql);

            sql = string.Format("UPDATE SMORD SET SMORD.PICKUP_CDATE={1} FROM SMRDN WHERE SMRDN.ORD_NO=SMORD.ORD_NO AND SMRDN.RESERVE_NO={0}", SQLUtils.QuotedStr(reserveNo),
                SQLUtils.QuotedStr(pickupDate.ToString("yyyy-MM-dd")));
            mixList.Add(sql);

            sql = string.Format("UPDATE SMIDN SET SMIDN.PICKUP_CDATE={1} FROM SMRDN WHERE SMIDN.DN_NO=SMRDN.DN_NO AND SMIDN.SHIPMENT_ID=SMRDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={0}", SQLUtils.QuotedStr(reserveNo),
                SQLUtils.QuotedStr(pickupDate.ToString("yyyy-MM-dd")));
            mixList.Add(sql);


            if (rcntrDt.Rows.Count > 0)
            {
                Tuple<string, int, int, int, string, string, string, Tuple<string>> tuple = new Tuple<string, int, int, int, string, string, string, Tuple<string>>("", 0, 0, 0, "", "", "", new Tuple<string>(""));
                foreach (DataRow rcntrDr in rcntrDt.Rows)
                {
                    string shipmentId = Prolink.Math.GetValueAsString(rcntrDr["SHIPMENT_ID"]);
                    string cntrNo = Prolink.Math.GetValueAsString(rcntrDr["CNTR_NO"]);
                    if (!shipmentList.ContainsKey(shipmentId))
                    {
                        tuple = setFreeTime(shipmentId);
                        shipmentList.Add(shipmentId, tuple);
                    }
                    else
                    {
                        tuple = shipmentList[shipmentId];
                    }
                    ModifyDueDate(pickupDate, mixList, rcntrDr, tuple);
                }
            }
        }

        public static void ModifyDueDate(DateTime pickupDate, MixedList mixList, DataRow cdr, Tuple<string, int, int, int, string, string, string, Tuple<string>> tuple)
        {
            string shipmentId = Prolink.Math.GetValueAsString(cdr["SHIPMENT_ID"]);
            string cntrNo = Prolink.Math.GetValueAsString(cdr["CNTR_NO"]);
            string ordNo = Prolink.Math.GetValueAsString(cdr["ORD_NO"]);
            string sql = string.Format("SELECT COMBINE_DET,CON_FREE_TIME,EMP_PICK_DATE,DISCHARGE_DATE,ETA,PICKUP_CDATE,FACT_FREE_TIME,PORT_FREE_TIME,CMP FROM SMORD WHERE ORD_NO={0}", SQLUtils.QuotedStr(ordNo));
            DataTable ordDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (ordDt.Rows.Count > 0&&!string.IsNullOrEmpty(cntrNo))
            {
                int ConFreeTime = Prolink.Math.GetValueAsInt(ordDt.Rows[0]["CON_FREE_TIME"]);
                int FactFreeTime = Prolink.Math.GetValueAsInt(ordDt.Rows[0]["FACT_FREE_TIME"]);
                int PortFreeTime = Prolink.Math.GetValueAsInt(ordDt.Rows[0]["PORT_FREE_TIME"]);
                string isCombineDet = Prolink.Math.GetValueAsString(ordDt.Rows[0]["COMBINE_DET"]);
                DateTime empPickDate = Prolink.Math.GetValueAsDateTime(ordDt.Rows[0]["EMP_PICK_DATE"]);
                DateTime eta = Prolink.Math.GetValueAsDateTime(ordDt.Rows[0]["ETA"]);
                DateTime pickDate = Prolink.Math.GetValueAsDateTime(ordDt.Rows[0]["PICKUP_CDATE"]);
                DateTime dischargeDate = Prolink.Math.GetValueAsDateTime(ordDt.Rows[0]["DISCHARGE_DATE"]);
                string cmp = Prolink.Math.GetValueAsString(ordDt.Rows[0]["CMP"]);

                string factChgDayType = tuple.Item7;
                string portChgDayType = tuple.Rest.Item1;
                string conChgDayType = tuple.Item6;

                PortFeeDate feeDate = new PortFeeDate(shipmentId, cntrNo, isCombineDet, PortFreeTime, FactFreeTime, ConFreeTime, eta, empPickDate, pickupDate, dischargeDate, portChgDayType, factChgDayType, conChgDayType);

                DataTable bsDateDt = DayHelper.GetBsdate(cmp);
                var dueDateItem = getDueDate(feeDate, bsDateDt);
                EditInstruct icntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                icntrei.PutKey("SHIPMENT_ID", feeDate.ShipmentId);
                icntrei.PutKey("CNTR_NO", feeDate.CntrNo);
                icntrei.PutDate("DETENTION_DUE_DATE", dueDateItem.Item1);
                icntrei.PutDate("DEMURRAGE_DUE_DATE", dueDateItem.Item2);
                icntrei.PutDate("STORAGE_DUE_DATE", dueDateItem.Item3);
                icntrei.Put("PORT_CHG_TYPE", feeDate.PortChgDayType);
                icntrei.Put("FACT_CHG_TYPE", feeDate.FactChgDayType);
                icntrei.Put("CON_CHG_TYPE", feeDate.ConChgDayType);

                EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("ORD_NO", ordNo);
                ei.PutDate("DETENTION_DUE_DATE", dueDateItem.Item1);
                ei.PutDate("DEMURRAGE_DUE_DATE", dueDateItem.Item2);
                ei.PutDate("STORAGE_DUE_DATE", dueDateItem.Item3);
                mixList.Add(icntrei);
                mixList.Add(ei);

            }
        }

        public static Tuple<string, int, int, int, string, string, string, Tuple<string>> setFreeTime(string shipmentId)
        {
            string sql = string.Format("SELECT POD_CD,CMP,CARRIER,TRAN_TYPE,ETD,ETA,ATD,ATA FROM SMSMI WHERE SHIPMENT_ID={0} AND TRAN_TYPE IN ('F','R')", SQLUtils.QuotedStr(shipmentId));
            DataTable smDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Tuple<string, int, int, int, string, string, string, Tuple<string>> portFreeItem = new Tuple<string, int, int, int, string, string, string, Tuple<string>>("", 0, 0, 0, "", "", "", new Tuple<string>(""));
            string LspNo = GetForwarderNo(shipmentId);
            foreach (DataRow dr in smDt.Rows)
            {
                string podCd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string carrier = Prolink.Math.GetValueAsString(dr["CARRIER"]);
                string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                DateTime etd= Prolink.Math.GetValueAsDateTime(dr["ETD"]);
                DateTime eta = Prolink.Math.GetValueAsDateTime(dr["ETA"]);
                DateTime atd = Prolink.Math.GetValueAsDateTime(dr["ATD"]);
                DateTime ata = Prolink.Math.GetValueAsDateTime(dr["ATA"]);
                portFreeItem = getFreeTime(LspNo,podCd, cmp, carrier, tranType, ata > DateTime.MinValue ? ata : eta, atd > DateTime.MinValue ? atd : etd);
            }
            MixedList ml = new MixedList();

            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            smsmiei.PutKey("SHIPMENT_ID", shipmentId);
            smsmiei.Put("PORT_FREE_TIME", portFreeItem.Item4);
            smsmiei.Put("FACT_FREE_TIME", portFreeItem.Item3);
            smsmiei.Put("CON_FREE_TIME", portFreeItem.Item2);
            smsmiei.Put("COMBINE_DET", portFreeItem.Item1);
            smsmiei.Put("SHOW_COMBINE_DET", portFreeItem.Item5);
            ml.Add(smsmiei);
            EditInstruct ordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            ordei.PutKey("SHIPMENT_ID", shipmentId);
            ordei.Put("PORT_FREE_TIME", portFreeItem.Item4);
            ordei.Put("FACT_FREE_TIME", portFreeItem.Item3);
            ordei.Put("CON_FREE_TIME", portFreeItem.Item2);
            ordei.Put("COMBINE_DET", portFreeItem.Item1);
            ordei.Put("SHOW_COMBINE_DET", portFreeItem.Item5);
            ml.Add(ordei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex) { }
            return portFreeItem;
        }

        public static Tuple<string, int, int, int, string, string, string, Tuple<string>> getFreeTime(string lspNo, string podCd, string cmp, string carrier, string tranType, DateTime arvDate, DateTime depDate)
        {
            string sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CARRIER_CD,CHG_DAY_TYPE FROM SMQTI WHERE CMP={0} 
                AND POD_CD={1} AND (CARRIER_CD={2} OR CARRIER_CD IS NULL) AND TRAN_TYPE={3} AND I_TYPE IN ('DDEM','BOTH','USAGE','DDET','DSTF') 
                AND FEE_PER_DAY=0 AND (LSP_NO IS NULL OR LSP_NO='')",
                SQLUtils.QuotedStr(cmp),SQLUtils.QuotedStr(podCd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(tranType));

            string lspNotNullSql = "";
            if (!string.IsNullOrEmpty(lspNo))
            {
                lspNotNullSql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CARRIER_CD,CHG_DAY_TYPE FROM SMQTI WHERE CMP={0}
                AND POD_CD={1} AND (CARRIER_CD={2} OR CARRIER_CD IS NULL) AND TRAN_TYPE={3} AND I_TYPE IN ('DDEM','BOTH','USAGE','DDET','DSTF') 
                AND FEE_PER_DAY=0 ", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(podCd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(tranType)); 
                sql = string.Format(@"{0} AND LSP_NO={1}", lspNotNullSql, SQLUtils.QuotedStr(lspNo));
                if ("F".Equals(tranType)) 
                    sql = string.Format(@"{0} AND LSP_NO=(SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={1} AND STATUS='U')",lspNotNullSql, SQLUtils.QuotedStr(lspNo)); 
            }
            DataTable qtiDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (qtiDt.Rows.Count <= 0 && !string.IsNullOrEmpty(lspNo) && "F".Equals(tranType))
                sql = string.Format(@"{0} AND LSP_NO={1}", lspNotNullSql, SQLUtils.QuotedStr(lspNo)); 
            qtiDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Func<string, Tuple<string, int, string>> getFreeItem = freeType =>
            {
                Tuple<string, int, string> freeItem = getFreeTime(qtiDt, freeType, "A", arvDate, carrier);
                if (freeItem.Item2 <= 0)
                    freeItem = getFreeTime(qtiDt, freeType, "D", depDate, carrier);
                return freeItem;
            };
            Tuple<string, int, string> factFreeItem = getFreeItem("STOFEE");
            Tuple<string, int, string> conFreeItem = getFreeItem("DETFEE");
            Tuple<string, int, string> portFreeItem = getFreeItem("DEMFEE");
            string isCombineDet = portFreeItem.Item1 + conFreeItem.Item1;
            string ShowisCombineDet = conFreeItem.Item2 <= 0 ? "" : conFreeItem.Item1;
            int portFreeItemitem2 = "Y".Equals(ShowisCombineDet) ? 0 : portFreeItem.Item2;
            if (_isout)
            {
                isCombineDet = conFreeItem.Item1;
                if (!string.IsNullOrEmpty(portFreeItem.Item1))
                    isCombineDet = portFreeItem.Item1;
                portFreeItemitem2 = portFreeItem.Item2;
            }
            return new Tuple<string, int, int, int, string, string, string, Tuple<string>>(isCombineDet, conFreeItem.Item2, factFreeItem.Item2, portFreeItemitem2, ShowisCombineDet, conFreeItem.Item3, factFreeItem.Item3, new Tuple<string>(portFreeItem.Item3));
        }

        public static Tuple<string, int, string> getFreeTime(DataTable qtiDt, string myType, string calDate, DateTime date, string carrier)
        {
            string[] typeList = new string[] { };
            switch (myType)
            {
                case "DEMFEE": typeList = new string[] { "DDEM", "BOTH", "USAGE" };break;
                case "DETFEE": typeList = new string[] { "DDET", "BOTH" };break;
                case "STOFEE": typeList = new string[] { "DSTF" };break;
            } 
            DataRow[] drs = qtiDt.Select(string.Format("I_TYPE IN{0} AND CAL_DATE={1} AND CARRIER_CD={2}", SQLUtils.Quoted(typeList), 
                SQLUtils.QuotedStr(calDate),SQLUtils.QuotedStr(carrier)), "EFFECT_DATE ASC,EXPIRAT_DATE ASC");
            if (drs.Length <= 0)
            {
                if (_isout)
                {
                    drs = qtiDt.Select(string.Format("I_TYPE IN{0} AND CAL_DATE={1} AND CARRIER_CD IS NULL", SQLUtils.Quoted(typeList),
                    SQLUtils.QuotedStr(calDate), SQLUtils.QuotedStr(carrier)), "EFFECT_DATE ASC");
                }
                else
                {
                    drs = qtiDt.Select(string.Format("I_TYPE IN{0} AND CAL_DATE={1} AND CARRIER_CD IS NULL", SQLUtils.Quoted(typeList),
                    SQLUtils.QuotedStr(calDate)), "EFFECT_DATE ASC,EXPIRAT_DATE ASC");
                }
            }
            int freeDate = 0;
            string isCombineDet = "N";
            string chgdayType = "";
            if (_isout)
            {
                isCombineDet = "";
            }
            foreach (DataRow dr in drs)
            {
                DateTime effectDate = Prolink.Math.GetValueAsDateTime(dr["EFFECT_DATE"]);
                DateTime expriatDate = Prolink.Math.GetValueAsDateTime(dr["EXPIRAT_DATE"]);
                if (date > expriatDate || date < effectDate)
                    continue;
                if (_isout)
                {
                    isCombineDet = "";
                }
                freeDate = Prolink.Math.GetValueAsInt(dr["E_DAY"]);
                string iType = Prolink.Math.GetValueAsString(dr["I_TYPE"]);
                chgdayType = Prolink.Math.GetValueAsString(dr["CHG_DAY_TYPE"]);
                switch (iType)
                {
                    case "BOTH":
                        isCombineDet = "Y"; break;
                    case "USAGE":
                        isCombineDet = "U";
                        break;
                    default:
                        if (_isout && myType == "DETFEE")
                            isCombineDet = "N";
                        break;
                }
            }
            return new Tuple<string, int, string>(isCombineDet, freeDate, chgdayType);
        }

        public static string GetForwarderNo(string shipmentId)
        {
            string sql = string.Format("SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE='SP'", SQLUtils.QuotedStr(shipmentId));
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        public static string GetForwarderNoBySm(string shipmentId)
        {
            string sql = string.Format("SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE='SP'", SQLUtils.QuotedStr(shipmentId));
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// TranType="T"时，根据工厂代码和ship to Party 带出目的地库位资料去更新DN表
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="cmp"></param>
        /// <param name="ei"></param>
        /// <returns></returns>
        public static string DestInfoUpdate(string shipmentId, string cmp, EditInstruct ei)
        {
            string sql = string.Format("SELECT STN,(SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE PARTY_TYPE='WE' AND SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID) AS PARTY_NO" +
                " FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return "The shipment doesn't find";
            string factory = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
            string shipTo = Prolink.Math.GetValueAsString(dt.Rows[0]["PARTY_NO"]);

            sql = string.Format("SELECT PORT_CD,PORT_NM," +
                "(SELECT TOP 1 ADDR_CODE FROM DEST_ADDR WHERE PORT_CD=BSDEST.PORT_CD AND CNTRY_CD=BSDEST.CNTRY_CD AND FACTORY=BSDEST.FACTORY AND SHIP_TO=BSDEST.SHIP_TO AND CMP=BSDEST.CMP) AS ADDR_CODE," +
                "(SELECT TOP 1 ADDR FROM DEST_ADDR WHERE PORT_CD=BSDEST.PORT_CD AND CNTRY_CD=BSDEST.CNTRY_CD AND FACTORY=BSDEST.FACTORY AND SHIP_TO=BSDEST.SHIP_TO AND CMP=BSDEST.CMP) AS ADDR," +
                "(SELECT TOP 1 WH_CODE FROM DEST_ADDR WHERE PORT_CD=BSDEST.PORT_CD AND CNTRY_CD=BSDEST.CNTRY_CD AND FACTORY=BSDEST.FACTORY AND SHIP_TO=BSDEST.SHIP_TO AND CMP=BSDEST.CMP) AS WH_CODE," +
                "(SELECT TOP 1 WH_NAME FROM DEST_ADDR WHERE PORT_CD=BSDEST.PORT_CD AND CNTRY_CD=BSDEST.CNTRY_CD AND FACTORY=BSDEST.FACTORY AND SHIP_TO=BSDEST.SHIP_TO AND CMP=BSDEST.CMP) AS WH_NAME," +
                "(SELECT TOP 1 OUTER_FLAG FROM DEST_ADDR WHERE PORT_CD=BSDEST.PORT_CD AND CNTRY_CD=BSDEST.CNTRY_CD AND FACTORY=BSDEST.FACTORY AND SHIP_TO=BSDEST.SHIP_TO AND CMP=BSDEST.CMP) AS OUTER_FLAG," +
                "(SELECT TOP 1 FINAL_WH FROM DEST_ADDR WHERE PORT_CD=BSDEST.PORT_CD AND CNTRY_CD=BSDEST.CNTRY_CD AND FACTORY=BSDEST.FACTORY AND SHIP_TO=BSDEST.SHIP_TO AND CMP=BSDEST.CMP) AS FINAL_WH" +
                " FROM BSDEST WHERE FACTORY={0} AND SHIP_TO={1} AND CMP={2}", SQLUtils.QuotedStr(factory), SQLUtils.QuotedStr(shipTo), SQLUtils.QuotedStr(cmp));
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return string.Format("Factory:{0},Ship to Party:{1},cmp:{2} doesn't included in the 3PL DC Setup!", factory, shipTo, cmp);

            string portCd = Prolink.Math.GetValueAsString(dt.Rows[0]["PORT_CD"]);
            string portNm = Prolink.Math.GetValueAsString(dt.Rows[0]["PORT_NM"]);
            string addrCode = Prolink.Math.GetValueAsString(dt.Rows[0]["ADDR_CODE"]);
            string addr = Prolink.Math.GetValueAsString(dt.Rows[0]["ADDR"]);
            string whCode = Prolink.Math.GetValueAsString(dt.Rows[0]["WH_CODE"]);
            string whName = Prolink.Math.GetValueAsString(dt.Rows[0]["WH_NAME"]);
            string outerFlag = Prolink.Math.GetValueAsString(dt.Rows[0]["OUTER_FLAG"]);
            string finalWh = Prolink.Math.GetValueAsString(dt.Rows[0]["FINAL_WH"]);

            ei.Put("DLV_AREA", portCd);
            ei.Put("DLV_AREA_NM", portNm);
            ei.Put("ADDR_CODE", addrCode);
            ei.Put("DLV_ADDR", addr);
            ei.Put("WS_CD", whCode);
            ei.Put("WS_NM", whName);
            ei.Put("FINAL_WH", finalWh);

            return "success";
        }
        /// <summary>
        /// 获取二段DN NO
        /// </summary>
        /// <param name="dnNo">DN NO</param>
        /// <returns></returns>
        public static string GetRefNo(string dnNo)
        {
            if (string.IsNullOrEmpty(dnNo)) return "";
            string sql = string.Format("SELECT REF_NO FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(dnNo));
            string refNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return refNo;
        }

        public static Tuple<DateTime, DateTime, DateTime> getDueDate(PortFeeDate feeDate, DataTable bsDateDt)
        {
            DateTime DetDueDate = DateTime.MinValue, DemDueDate = DateTime.MinValue, StoDueDate = DateTime.MinValue;
            if (feeDate.DischargeDate > DateTime.MinValue || feeDate.Eta > DateTime.MinValue)
                StoDueDate = feeDate.DischargeDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.DischargeDate, feeDate.FactFreeTime, bsDateDt, feeDate.FactChgDayType) : DayHelper.AddWorkHolidays(feeDate.Eta, feeDate.FactFreeTime, bsDateDt, feeDate.FactChgDayType);
            switch (feeDate.CombineDet)
            {
                case "NN":
                    if (feeDate.DischargeDate > DateTime.MinValue || feeDate.Eta > DateTime.MinValue)
                        DemDueDate = feeDate.DischargeDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.DischargeDate, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType) : DayHelper.AddWorkHolidays(feeDate.Eta, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType);
                    DetDueDate = feeDate.PickDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.PickDate, feeDate.ConFreeTime, bsDateDt, feeDate.ConChgDayType) : DetDueDate;
                    break;
                case "YY":
                    if (feeDate.DischargeDate > DateTime.MinValue || feeDate.Eta > DateTime.MinValue)
                        DemDueDate = feeDate.DischargeDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.DischargeDate, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType) : DayHelper.AddWorkHolidays(feeDate.Eta, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType);
                    if (feeDate.DischargeDate > DateTime.MinValue || feeDate.Eta > DateTime.MinValue)
                        DetDueDate = feeDate.DischargeDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.DischargeDate, feeDate.ConFreeTime, bsDateDt, feeDate.ConChgDayType) : DayHelper.AddWorkHolidays(feeDate.Eta, feeDate.ConFreeTime, bsDateDt, feeDate.ConChgDayType);
                    break;
                case "UY":
                    if (feeDate.EmpPickDate > DateTime.MinValue || feeDate.Eta > DateTime.MinValue)
                        DemDueDate = feeDate.EmpPickDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.EmpPickDate, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType) : DayHelper.AddWorkHolidays(feeDate.Eta, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType);
                    if (feeDate.DischargeDate > DateTime.MinValue || feeDate.Eta > DateTime.MinValue)
                        DetDueDate = feeDate.DischargeDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.DischargeDate, feeDate.ConFreeTime, bsDateDt, feeDate.ConChgDayType) : DayHelper.AddWorkHolidays(feeDate.Eta, feeDate.ConFreeTime, bsDateDt, feeDate.ConChgDayType);
                    break;
                case "UN":
                    if (feeDate.EmpPickDate > DateTime.MinValue || feeDate.Eta > DateTime.MinValue)
                        DemDueDate = feeDate.EmpPickDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.EmpPickDate, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType) : DayHelper.AddWorkHolidays(feeDate.Eta, feeDate.PortFreeTime, bsDateDt, feeDate.PortChgDayType);
                    DetDueDate = feeDate.PickDate > DateTime.MinValue ? DayHelper.AddWorkHolidays(feeDate.PickDate, feeDate.ConFreeTime, bsDateDt, feeDate.ConChgDayType) : DetDueDate;
                    break;
            }
            DetDueDate = feeDate.ConFreeTime > 0 ? DetDueDate : DateTime.MinValue;
            DemDueDate = feeDate.PortFreeTime > 0 ? DemDueDate : DateTime.MinValue;
            StoDueDate = feeDate.FactFreeTime > 0 ? StoDueDate : DateTime.MinValue;
            return new Tuple<DateTime, DateTime, DateTime>(DetDueDate, DemDueDate, StoDueDate);
        }

        public class PortFeeDate
        {
            public string ShipmentId { get; set; }
            public string CntrNo { get; set; }
            public string StoCur { get; set; }
            public decimal StoAmt { get; set; }
            public DEMtype StoUp { get; set; }
            public string DemCur { get; set; }
            public decimal DemAmt { get; set; }
            public DEMtype DemUp { get; set; }
            public string DetCur { get; set; }
            public decimal DetAmt { get; set; }
            public DEMtype DetUp { get; set; }
            public DateTime Eta { get; set; }
            public DateTime DischargeDate { get; set; }
            public DateTime EmpPickDate { get; set; }
            public DateTime PickDate { get; set; }
            public int PortFreeTime { get; set; }
            public int FactFreeTime { get; set; }
            public int ConFreeTime { get; set; }

            public string PortChgDayType { get; set; }
            public string FactChgDayType { get; set; }
            public string ConChgDayType { get; set; }

            public string CombineDet { get; set; }
            public string ShowCombineDet { get; set; }
            public PortFeeDate(string shipmentId, string cntrNo)
            {
                this.StoUp = DEMtype.undo;
                this.DemUp = DEMtype.undo;
                this.DetUp = DEMtype.undo;
                this.ShipmentId = shipmentId;
                this.CntrNo = cntrNo;
            }
            public PortFeeDate(string shipmentId, string cntrNo, string isCombineDet,int portFreeTime,int factFreeTime,int conFreeTime,DateTime eta,DateTime empPickDate,DateTime pickDate,DateTime dischargeDate, string portChgDayType, string factChgDayType, string conChgDayType)
            {
                this.StoUp = DEMtype.undo; 
                this.DemUp = DEMtype.undo;
                this.DetUp = DEMtype.undo;
                this.ShipmentId = shipmentId;
                this.CntrNo = cntrNo;
                this.CombineDet = isCombineDet;
                this.PortFreeTime = portFreeTime;
                this.FactFreeTime = factFreeTime;
                this.ConFreeTime = conFreeTime;
                this.Eta = eta;
                this.EmpPickDate = empPickDate;
                this.PickDate = pickDate;
                this.DischargeDate = dischargeDate;
                this.PortChgDayType = portChgDayType;
                this.FactChgDayType = factChgDayType;
                this.ConChgDayType = conChgDayType;
            }
        }

        public static void SetSmind(DataRow dr, EditInstruct ei)
        {
            string dep = Prolink.Math.GetValueAsString(dr["DEP"]);
            string cur = Prolink.Math.GetValueAsString(dr["CUR2"]);
            string amount = Prolink.Math.GetValueAsString(dr["AMOUNT2"]);

            ei.Put("DEP", dep);
            ei.Put("INV_CUR", cur);
            ei.Put("AMOUNT", amount);
        }

        public static void SetSmindByBD(DataRow dr, EditInstruct ei)
        {
            string dep = Prolink.Math.GetValueAsString(dr["DEP"]);
            string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
            string amount = Prolink.Math.GetValueAsString(dr["GVALUE"]);

            ei.Put("DEP", dep);
            ei.Put("INV_CUR", cur);
            ei.Put("AMOUNT", amount);
        }

        public enum DEMtype
        {
            update,
            empty,
            undo
        }

        private static string getLastDnData(refdninfo refdn)
        {
            return string.IsNullOrEmpty(refdn.DN3) ? string.IsNullOrEmpty(refdn.DN2) ? refdn.DN1 : refdn.DN2 : refdn.DN3;
        }

        private static List<string> getLastDnList(List<refdninfo> refdnlist)
        {
            List<string> LastDnList = new List<string>();
            foreach (refdninfo refdn in refdnlist)
            {
                string dnno = getLastDnData(refdn);
                if (!LastDnList.Contains(dnno)) 
                    LastDnList.Add(dnno);
            }
            return LastDnList;
        }

        private static List<refdninfo> getRefDnDataList(string[] DnNoList) {
            List<refdninfo> refdnlist = new List<refdninfo>();
            foreach (string dnno in DnNoList) {
                refdnlist.Add(getRefDnData(dnno));
            }
            return refdnlist;
        }

        private static refdninfo getRefDnData(string DnNo) {
            string sql = string.Format(
@"{1},COM_REF_DATA AS(
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN where DN_NO = {0}
union all
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN inner join COM_REF_DATA r on  r.DN_NO_CMP_REF = SMDN.DN_NO and SMDN.DN_NO <> r.DN_NO
)

select DISTINCT *,(SELECT PARTY_NAME FROM SMDNPT WHERE PARTY_TYPE='FC' AND SMDNPT.DN_NO=DATA.DN_NO ) AS PARTY_NAME from (
select  * from REF_DATA  
union all
select  * from COM_REF_DATA  
)DATA ORDER BY ETD ASC
", SQLUtils.QuotedStr(DnNo), GetRefDataSql(string.Format("DN_NO = {0}", SQLUtils.QuotedStr(DnNo)), "SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF"));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            refdninfo refdn = new refdninfo();
            refdn.oldDN = DnNo;
            if (mainDt != null && mainDt.Rows.Count > 0)
            {
                if (mainDt.Rows.Count > 2)
                {
                    refdn.DN3 = mainDt.AsEnumerable().Where(g => string.IsNullOrEmpty(g.Field<string>("REF_NO"))).FirstOrDefault()?.Field<string>("DN_NO");
                    refdn.DN2 = mainDt.AsEnumerable().Where(g => g.Field<string>("REF_NO") == refdn.DN3).FirstOrDefault()?.Field<string>("DN_NO");
                    refdn.DN1 = mainDt.AsEnumerable().Where(g => g.Field<string>("REF_NO") == refdn.DN2).FirstOrDefault()?.Field<string>("DN_NO");
                }
                else if (mainDt.Rows.Count > 1)
                {
                    refdn.DN2 = mainDt.AsEnumerable().Where(g => string.IsNullOrEmpty(g.Field<string>("REF_NO"))).FirstOrDefault()?.Field<string>("DN_NO");
                    refdn.DN1 = mainDt.AsEnumerable().Where(g => g.Field<string>("REF_NO") == refdn.DN2).FirstOrDefault()?.Field<string>("DN_NO");
                }
                else
                {
                    refdn.DN1 = Prolink.Math.GetValueAsString(mainDt.Rows[0]["DN_NO"]);
                }
            }

            return refdn;
        }

        private static string GetRefDataSql(string condition, string col = "")
        {
            if (string.IsNullOrEmpty(col))
                col = "SMDN.U_ID,SMDN.GROUP_ID,SMDN.CMP,SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF";
            return string.Format(@";WITH REF_DATA AS(
                SELECT {1} FROM SMDN WHERE {0}
                UNION ALL
                SELECT {1} FROM SMDN INNER JOIN REF_DATA R ON   R.REF_NO = SMDN.DN_NO AND R.DN_NO <> SMDN.DN_NO
                )", condition, col);
        }

        public class refdninfo
        {
            public string DN1 { get; set; }
            public string DN2 { get; set; }
            public string DN3 { get; set; }
            public string oldDN { get; set; }
        }
    }
}
