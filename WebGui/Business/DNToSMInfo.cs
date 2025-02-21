using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Business
{
    public class DNToSMInfo
    {
        private static readonly string[] PARTY_COLUMNS = new string[] { "SHIP_TO","CNEE1_CD","CNEE2_CD","CNEE3_CD","NOTIFY1_NO","NOTIFY2_NO"
            ,"FI_CUST_CD","SALES_CUST_CD","CA_CD","EX_CD","BA_CD","FW_CD","BK_CD","PK_CD"};

        private string _groupId = "";
        private string _cmp = "";
        private string _stn = "";
        private string _dep = "";
        private string _dnno="";

        private Dictionary<String, String> _pCdDict = null;
        private Dictionary<String, String> _PartyTypeDict = null;

        public DNToSMInfo(string GroupId, string Cmp, string Stn, string Dep)
		{
			this._groupId = GroupId;
			this._cmp = Cmp;
            this._stn = Stn;
            this._dep = Dep;
            _pCdDict = new Dictionary<String, String>();
            _PartyTypeDict = new Dictionary<String, String>();
		}

        public EditInstructList GetSMEditIns(string dnno)
        {
            EditInstructList eilist = new EditInstructList();
            //获取SMDN资料
            string sql = "SELECT * FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(dnno);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "SELECT * FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(dnno);

            DataRow maindr = null;
            if (maindt.Rows.Count > 0)
            {
                maindr = maindt.Rows[0];
            }
            //处理SMSM主档
            #region SMSM主档
            EditInstruct eimain = new EditInstruct("SMSM", EditInstruct.INSERT_OPERATION);
            string smsmuid=Guid.NewGuid().ToString();
            string shipmentid=Prolink.Math.GetValueAsString(maindr["DN_NO"]);
            eimain.Put("U_ID", smsmuid);
            eimain.Put("WEEKLY", Prolink.Math.GetValueAsString(maindr["WEEKLY"]));
            eimain.Put("MONTH", Prolink.Math.GetValueAsString(maindr["MONTH"]));
            eimain.Put("YEAR", Prolink.Math.GetValueAsString(maindr["YEAR"]));
            eimain.Put("GROUP_ID", Prolink.Math.GetValueAsString(maindr["GROUP_ID"]));
            eimain.Put("CMP", Prolink.Math.GetValueAsString(maindr["CMP"]));
            eimain.Put("STN", Prolink.Math.GetValueAsString(maindr["STN"]));
            if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(maindr["DEP"])))
            {
                eimain.Put("DEP", _dep);
            }
            else
            {
                eimain.Put("DEP", Prolink.Math.GetValueAsString(maindr["DEP"]));
            }
            eimain.Put("SHIPMENT_ID", shipmentid);
            //eimain.Put("SHIPMENT_INFO", Prolink.Math.GetValueAsString(maindr["SHIPMENT_INFO"]));
            //eimain.Put("NO_DECL", Prolink.Math.GetValueAsString(maindr["NO_DECL"]));
            eimain.Put("STATUS", "A");
            eimain.Put("BL_CHECK", "A");
            eimain.Put("CARRIER", Prolink.Math.GetValueAsString(maindr["CA_CD"]));
            eimain.Put("CARRIER_NM", Prolink.Math.GetValueAsString(maindr["CA_NAME"]));
            //eimain.Put("SCAC_CD", "");
            //eimain.Put("SVC_CONTACT", "");
            //eimain.Put("BL_WIN", "");
            //eimain.Put("SALES_WIN", "");
            //eimain.Put("BRG_TYPE", "");
            //eimain.Put("SIGN_BACK", "");//迟签退原因
            eimain.Put("INCOTERM_CD", Prolink.Math.GetValueAsString(maindr["INCOTERM"]));
            eimain.Put("INCOTERM_DESCP", Prolink.Math.GetValueAsString(maindr["INCOTERM_DESCP"]));
            //eimain.Put("INCOTERM_CD2", Prolink.Math.GetValueAsString(maindr["INCOTERM_CD2"]));
            //eimain.Put("INCOTERM_DESCP2", Prolink.Math.GetValueAsString(maindr["INCOTERM_DESCP2"]));
            eimain.Put("SERVICE_MODE", Prolink.Math.GetValueAsString(maindr["SERVICE_MODE"]));
            eimain.Put("FRT_TERM", Prolink.Math.GetValueAsString(maindr["FREIGHT_TERM"]));
            //eimain.Put("GOODS", "");
            //eimain.Put("MARKS", "");
            //eimain.Put("INSTRUCTION", "");
            eimain.Put("BOOKING_INFO", "");
            eimain.Put("DIMENSION", "");
            eimain.Put("HOUSE_NO", "");
            eimain.Put("MASTER_NO", "");
            eimain.Put("SO_NO", "");
            eimain.Put("REF_NO", Prolink.Math.GetValueAsString(maindr["REF_NO"]));
            eimain.Put("BL_TYPE", "H");
            eimain.PutDate("BL_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            //eimain.Put("CLS_DATE", "");
            eimain.Put("ETD", Prolink.Math.GetValueAsString(maindr["ETD"]));

            /*eimain.Put("ETA", "");
            eimain.Put("PICKUP_WMS", "");
            eimain.Put("PICKUP_WMS_NM", "");
            eimain.Put("PICKUP_WMS_DATE", "");
            eimain.Put("RCV_DATE", "");
            eimain.Put("DLV_DOC_DATE", "");
            eimain.Put("RLS_CNTR_DATE", "");
            eimain.Put("RCV_DOC_DATE", "");
            eimain.Put("CUT_PORT_DATE", "");
            eimain.Put("PORT_DATE", "");
            eimain.Put("CUSTOMS_DATE", "");
            eimain.Put("PORT_RLS_DATE", "");
            eimain.Put("VESSEL1", "");
            eimain.Put("VOYAGE1", "");
            eimain.Put("ETD1", "");
            eimain.Put("ETA1", "");
            eimain.Put("VESSEL2", "");
            eimain.Put("VOYAGE2", "");
            eimain.Put("ETD2", "");
            eimain.Put("ETA2", "");
            eimain.Put("VESSEL3", "");
            eimain.Put("VOYAGE3", "");
            eimain.Put("ETD3", "");
            eimain.Put("ETA3", "");
            eimain.Put("VESSEL4", "");
            eimain.Put("VOYAGE4", "");
            eimain.Put("ETD4", "");
            eimain.Put("ETA4", "");
            eimain.Put("POR_CD", "");
            eimain.Put("POR_CNTY", "");
            eimain.Put("POR_NAME", "");
            eimain.Put("POL_CD", "");
            eimain.Put("POL_CNTY", "");
            eimain.Put("POL_NAME", "");
            eimain.Put("VIA_CD", "");
            eimain.Put("VIA_CNTY", "");
            eimain.Put("VIA_NAME", "");
            eimain.Put("POD_CD", "");
            eimain.Put("POD_CNTY", "");
            eimain.Put("POD_NAME", "");
            eimain.Put("DEST_CD", "");
            eimain.Put("DEST_CNTY", "");
            eimain.Put("DEST_NAME", "");*/
            eimain.Put("QTY", Prolink.Math.GetValueAsString(maindr["QTY"]));
            eimain.Put("QTYU", Prolink.Math.GetValueAsString(maindr["QTYU"]));
            eimain.Put("NW", Prolink.Math.GetValueAsString(maindr["NW"]));
            eimain.Put("GW", Prolink.Math.GetValueAsString(maindr["GW"]));
            eimain.Put("GWU", Prolink.Math.GetValueAsString(maindr["GWU"]));
            eimain.Put("CBM", Prolink.Math.GetValueAsString(maindr["CBM"]));
            //eimain.Put("PLT_NUM", "");
            //eimain.Put("CNT20", "");
            //eimain.Put("CNT40", "");
            //eimain.Put("CNT40HQ", "");
            eimain.Put("CNT_TYPE", Prolink.Math.GetValueAsString(maindr["CNTR_TYPE"]));
            eimain.Put("CNT_NUMBER", Prolink.Math.GetValueAsString(maindr["FEU"]));
            eimain.Put("CUR", Prolink.Math.GetValueAsString(maindr["CUR"]));
            //eimain.Put("FREIGHT_AMT", "");        //运费
            //eimain.Put("INSURANCE_AMT", "");      //保费
            //eimain.Put("OPICKUP", "");
            //eimain.Put("ODELIVERY", "");
            eimain.Put("EXPORT_NO", Prolink.Math.GetValueAsString(maindr["EXPORT_NO"]));
            eimain.Put("EDECL_NO", Prolink.Math.GetValueAsString(maindr["EDECL_NO"]));
            eimain.Put("APPROVE_NO", Prolink.Math.GetValueAsString(maindr["APPROVE_NO"]));
            /*eimain.Put("OEXPORTER", "");
            eimain.Put("OEXPORTER_NM", "");
            eimain.Put("OEXPORTER_ADDR", "");
            eimain.Put("OIMPORTER", "");
            eimain.Put("OIMPORTER_NM", "");
            eimain.Put("OIMPORTER_ADDR", "");
            eimain.Put("DECL_DATE", "");
            eimain.Put("DECL_RLS_DATE", "");
            eimain.Put("BROKER_INSTR", "");
            eimain.Put("BROKER_INFO", "");*/
            eimain.Put("TRAN_TYPE", Prolink.Math.GetValueAsString(maindr["TRAN_MODE"]));
            eimain.Put("CARGO_TYPE", Prolink.Math.GetValueAsString(maindr["CARGO_TYPE"]));
            //eimain.Put("PICKUP_PORT", "");
            eimain.Put("REGION", Prolink.Math.GetValueAsString(maindr["REGION"]));
            eimain.Put("STATE", Prolink.Math.GetValueAsString(maindr["STATE"]));
            /*eimain.Put("DELIVERY_PORT", "");
            eimain.Put("TRUCK_TYPE", "");
            eimain.Put("DRIVER", "");
            eimain.Put("DRIVER_TEL", "");
            eimain.Put("TRUCK_NO", "");
            eimain.Put("LSP_NO", "");
            eimain.Put("LSP_NM", "");
            eimain.Put("EXPORT_CUR", "");
            eimain.Put("OF_COST", "");
            eimain.Put("OT_COST", "");*/
            eimain.Put("CREATE_BY", Prolink.Math.GetValueAsString(maindr["CREATE_BY"]));
            eimain.Put("CREATE_DEP", Prolink.Math.GetValueAsString(maindr["CREATE_DEP"]));
            eimain.Put("CREATE_EXT", Prolink.Math.GetValueAsString(maindr["CREATE_EXT"]));
            eimain.PutDate("CREATE_DATE", Prolink.Math.GetValueAsString(maindr["CREATE_DATE"]));
            /*eimain.Put("IPICKUP", "");
            eimain.Put("IDELIVERY", "");
            eimain.Put("IMPORT_NO", "");
            eimain.Put("CC_NO", "");
            eimain.Put("IAPPROVE_NO", "");
            eimain.Put("IEXPORTER", "");
            eimain.Put("IEXPORTER_NM", "");
            eimain.Put("IEXPORTER_ADDR", "");
            eimain.Put("IIMPORTER", "");
            eimain.Put("IIMPORTER_NM", "");
            eimain.Put("IIMPORTER_ADDR", "");
            eimain.Put("CC_DATE", "");
            eimain.Put("CC_RLS_DATE", "");
            eimain.Put("CC_INSTR", "");
            eimain.Put("CC_INFO", "");
            eimain.Put("IMPORT_CUR", "");
            eimain.Put("IF_COST", "");
            eimain.Put("IT_COST", "");
            eimain.Put("ICREATE_BY", "");
            eimain.Put("ICREATE_DEP", "");
            eimain.Put("ICREATE_EXT", "");
            eimain.Put("ICREATE_DATE", "");*/
            eimain.Put("DN_NO", Prolink.Math.GetValueAsString(maindr["DN_NO"]));
            //eimain.Put("ATD", "");
            //eimain.Put("ATA", "");
            //eimain.Put("REMARK", Prolink.Math.GetValueAsString(maindr["DN_NO"]));
            //eimain.Put("MODIFY_BY", Prolink.Math.GetValueAsString(maindr["MODIFY_BY"]));
            //eimain.Put("MODIFY_DATE", Prolink.Math.GetValueAsString(maindr["MODIFY_DATE"]));
            #endregion
            eilist.Add(eimain);

            foreach (string colName in PARTY_COLUMNS)
            {
                string colValue = "";
                string partyType = "";
                switch (colName)
                {
                    case "SHIP_TO": partyType = "ST";
                        break;
                    case "CNEE1_CD":
                    case "CNEE2_CD":
                    case "CNEE3_CD": partyType = "CN";
                        break;
                    case "NOTIFY1_NO":
                    case "NOTIFY2_NO": partyType = "NF";
                        break;
                    case "FI_CUST_CD": partyType = "CU";
                        break;
                    case "SALES_CUST_CD": partyType = "CU";
                        break;
                    case "CA_CD": partyType = "CA";
                        break;
                    case "EX_CD": partyType = "EX";
                        break;
                    case "BA_CD": partyType = "BA";
                        break;
                    case "FW_CD": partyType = "FW";
                        break;
                    case "BK_CD": partyType = "BK";
                        break;
                    case "PK_CD": partyType = "PT";
                        break;
                }
                colValue = Prolink.Math.GetValueAsString(maindr[colName]);
                if (_pCdDict.ContainsKey(colName) == false)
                {
                    _pCdDict.Add(colName, colValue);
                }
                if (_PartyTypeDict.ContainsKey(colName) == false)
                {
                    _PartyTypeDict.Add(colName, partyType);
                }
            }

            string[] cds = new List<string>(_pCdDict.Values).ToArray();
            sql = string.Format(@"SELECT SMPTY.*,(SELECT TOP 1 CD_DESCP FROM BSCODE WHERE BSCODE.CD_TYPE='PT' 
                AND BSCODE.GROUP_ID=SMPTY.GROUP_ID
                AND BSCODE.STN=SMPTY.STN)AS TYPE_DESCP FROM SMPTY  WHERE GROUP_ID={0} AND CMP={1} AND STN={2} AND PARTY_NO IN {3}",
                    SQLUtils.QuotedStr(_groupId), SQLUtils.QuotedStr(_cmp), SQLUtils.QuotedStr(_stn), SQLUtils.Quoted(cds));
            DataTable dtparty = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<string> cdlist = new List<string>(_pCdDict.Keys);
            List<string> partytypelist = new List<string>(_pCdDict.Keys);
            for (int i = 0; i < cdlist.Count; i++)
            {
                string cdtype = _pCdDict[cdlist[i]];
                if (string.IsNullOrEmpty(cdtype))
                {
                    continue;
                }
                string partytype = _PartyTypeDict[partytypelist[i]];
                string conditions = string.Format("PARTY_TYPE={0} AND PARTY_NO={1}", SQLUtils.QuotedStr(partytype), SQLUtils.QuotedStr(cdtype));
                DataRow[] drs = dtparty.Select(conditions, "CREATE_DATE DESC");
                if (drs.Length >= 1)
                {
                    DataRow dr = drs[0];
                    EditInstruct ei = new EditInstruct("SMSMPT", EditInstruct.INSERT_OPERATION);
                    ei.PutKey("U_ID", Guid.NewGuid().ToString());
                    ei.PutKey("U_FID", smsmuid);
                    ei.PutKey("SHIPMENT_ID",shipmentid);
                    

                    ei.Put("PARTY_TYPE", Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]));
                    ei.Put("TYPE_DESCP", Prolink.Math.GetValueAsString(dr["TYPE_DESCP"]));
                    ei.Put("ORDER_BY", "");
                    ei.Put("PARTY_NO", Prolink.Math.GetValueAsString(dr["PARTY_NO"]));
                    ei.Put("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                    ei.Put("PART_ADDR1", Prolink.Math.GetValueAsString(dr["PART_ADDR1"]));
                    ei.Put("PART_ADDR2", Prolink.Math.GetValueAsString(dr["PART_ADDR2"]));
                    ei.Put("PART_ADDR3", Prolink.Math.GetValueAsString(dr["PART_ADDR3"]));
                    ei.Put("CNTY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                    ei.Put("CNTY_NM", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                    ei.Put("CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                    ei.Put("CITY_NM", Prolink.Math.GetValueAsString(dr["CITY_NM"]));
                    ei.Put("STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                    ei.Put("ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                    ei.Put("PARTY_ATTN", Prolink.Math.GetValueAsString(dr["PARTY_ATTN"]));
                    ei.Put("PARTY_TEL", Prolink.Math.GetValueAsString(dr["PARTY_TEL"]));
                    ei.Put("PARTY_MAIL", Prolink.Math.GetValueAsString(dr["PARTY_MAIL"]));

                    eilist.Add(ei);
                }
            }
            return eilist;
        }
    }
}