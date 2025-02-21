using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Mail;
using TrackingEDI.Model;

namespace TrackingEDI.Business
{
    /// <summary>
    /// DN信息转订舱
    /// </summary>
    public class TransferBooking : BaseParser
    {
        public static string[] PARTY_TYPES = { "AG", "ZE", "RO", "FC", "RE", "SP", "NT", "Z4", "Z5", "Z6", "Z7", "CS", "Z1", "Z2", "Z3", "FS", "BO", "SL", "CC", "DT", "SH", "FC", "WE", "CR", "RG" };
        public static string[] SET_TYPES = { "SH", "CS", "PT", "DT", "FS", "AG", "ZE", "FC", "RE", "WE", "RO", "CR" };

        static string PartyEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string shimentid = Prolink.Math.GetValueAsString(parm["SYS_SHIPMENTID"]);
            string partytype = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
            string newpartytype = partytype;

            switch (partytype)
            {
                case "NT":
                case "Z4":
                case "Z5":
                case "Z6":
                case "Z7":
                    newpartytype="NT";
                    break;
                case "CS":
                case "Z1":
                case "Z2":
                case "Z3":
                    newpartytype="CS";
                    break;
            }

            string ufid = Prolink.Math.GetValueAsString(parm["U_ID"]);
            string uid = Guid.NewGuid().ToString();
            ei.Put("U_ID", uid);
            ei.Put("SHIPMENT_ID", shimentid);
            ei.Put("U_FID", ufid);
            ei.Put("PARTY_TYPE", newpartytype);
            
            return string.Empty;
        }

        static string BookingEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string shimentid = Prolink.Math.GetValueAsString(parm["SYS_SHIPMENTID"]);
            string tran_type = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            string combineinfo= Prolink.Math.GetValueAsString(parm["COMBINE_INFO"]);
            string pol = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            string dnno = Prolink.Math.GetValueAsString(dr["DN_NO"]);

            string shipmentuid = string.Empty;
            try{
                shipmentuid = Prolink.Math.GetValueAsString(parm["SHIPMENT_UID"]); ;
            }catch(Exception ex){
                shipmentuid = string.Empty;
            }
            if(string.IsNullOrEmpty(shipmentuid))
                shipmentuid = Guid.NewGuid().ToString();
            
            parm["U_ID"] = shipmentuid;
            ei.Put("U_ID", shipmentuid);
            TrackingEDI.Business.CaCuLateManager cm = new TrackingEDI.Business.CaCuLateManager();
            TrackingEDI.Business.TotalQTyCaCuLate tqc = new TrackingEDI.Business.TotalQTyCaCuLate();
            ei.Put("TRAN_TYPE", tran_type);
            ei.Put("SHIPMENT_ID", shimentid);
            ei.Put("STATUS", 'A'); //由DN转入Booking的资料设置初始值为A--未订舱

            //国际快递订舱画面的service栏位要求默认带出“15N:全球快递（包裹）“
            if(tran_type == "E")
            {
                ei.Put("SERVICE", "15N");
            }

            if (string.IsNullOrEmpty(combineinfo)) combineinfo = dnno;
            tqc = cm.CaCulateCombineByDn(dnno, combineinfo);
            cm.CaCulatePutEi(ref ei, tqc, shimentid, null);

            ei.Put("DEST_CD", dr["POD"]);
            ei.Put("DEST_NAME", dr["POD_NM"]);
            ei.Put("PPOD_CD", dr["POD"]);
            ei.Put("PPOD_NAME", dr["POD_NM"]);
            ei.Put("POD_CD", dr["POD"]);
            ei.Put("POD_NAME", dr["POD_NM"]);
            ei.Put("PPOR_CD", dr["POL"]);
            ei.Put("PPOR_NAME", dr["POL_NM"]);
            ei.Put("POR_CD", dr["POL"]);
            ei.Put("POR_NAME", dr["POL_NM"]);
            ei.Put("POL_CD", dr["POL"]);
            ei.Put("POL_NAME", dr["POL_NM"]);

            BatchSetEI(ei,SET_TYPES,parm);

            Func<string, string> setPT = name =>
           {
                if (parm.ContainsKey(name))
                    ei.Put(name, Prolink.Math.GetValueAsString(parm[name]));
                return string.Empty;
            };
            setPT("DEBIT_TO");
            setPT("DEBIT_NM");
            setPT("CARRIER");
            setPT("CARRIER_NM");
            setPT("OEXPORTER");
            setPT("OEXPORTER_NM");
            setPT("OEXPORTER_ADDR");
            setPT("OIMPORTER");
            setPT("OIMPORTER_NM");
            setPT("OIMPORTER_ADDR");

            ei.Put("BORDER", 'N');  
            ei.Put("CORDER", 'N'); 
            ei.Put("TORDER", 'N');
            ei.PutDate("INTERNAL_BK_DATE", DateTime.Now);
            ei.Put("DN_NO_REF", dr["DN_NO_REF"]);

            string uext = BookingStatusManager.GetUserFxt(parm["USER_ID"].ToString());
            if (string.IsNullOrEmpty(uext)) uext = parm["USER_ID"].ToString();
            ei.Put("SALES_WIN", uext); 
            return string.Empty;
        }

        public static void BatchSetEI(EditInstruct ei, string[] types, Dictionary<string, object> parm)
        {
            foreach (string type in types)
            {
                setP(ei, type, parm);
            }
        }

        public static void setP(EditInstruct ei, string type, Dictionary<string, object> parm)
        {
            string name = type + "_CD";
            if (parm.ContainsKey(name))
                ei.Put(name, Prolink.Math.GetValueAsString(parm[name]));
            name = type + "_NM";
            if (parm.ContainsKey(name))
                ei.Put(name, Prolink.Math.GetValueAsString(parm[name]));
        }

        public static string []SplitDn(string dnnos)
        {
            if (string.IsNullOrEmpty(dnnos)) return null;
            dnnos = dnnos.Trim(',').Trim(';');
            return dnnos.Split(new char[] { ',', ';' });
        }

        public static string GetSMAutoNo(string group, string cmp, string stn)
        {
            group = "TPV";
            //cmp="FQ";
            stn="*";
            string ruleCode = "SMRV_NO";
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            //hash.Add("GROUPID",group);
            hash.Add("CMP",cmp);
            //hash.Add("STN",stn);
            return AutoNo.GetNo(ruleCode, hash, group, cmp, stn);
        }

        private void GetTotal(string combineinfo, Dictionary<string, object> parm,DataRow dr)
        {
            
        }

        private string checkStatusinfo(string combineinfo)
        {
            if (string.IsNullOrEmpty(combineinfo)) return string.Empty;
            combineinfo = combineinfo.Trim(',');
            string[] dnitems = combineinfo.Split(',');
            string sql = string.Format("SELECT DN_NO,STATUS FROM SMDN WHERE DN_NO IN {0}", SQLUtils.Quoted(dnitems));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string status=string.Empty;
            string dnno=string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                status=Prolink.Math.GetValueAsString(dr["STATUS"]);
                dnno=Prolink.Math.GetValueAsString(dr["DN_NO"]);
                if ("T".Equals(status))
                    continue;
                if (!"D".Equals(status))
                    return dnno + " 此笔DN不是未处理状态 不允许对外订舱!";
            }
            return string.Empty;
        }

        private void UpdateCombinInfo(MixedList ml,string combineinfo, string shipmentid )
        {
            if (!string.IsNullOrEmpty(combineinfo))
            {
               combineinfo = combineinfo.Trim(',');
                string[] dnitems = combineinfo.Split(',');
                 //A,B,C,O
                string sql=string.Format("SELECT TRANSACTE_MODE,DN_NO FROM SMDN WHERE DN_NO IN {0}",Prolink.Data.SQLUtils.Quoted(dnitems));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string[] blleves = new string[] {"D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V" };
                Dictionary<string, string> blmapping = new Dictionary<string, string>();
                int j = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    //A,B,C,O
                    string transactemode=dr["TRANSACTE_MODE"].ToString();
                    EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("DN_NO", dr["DN_NO"].ToString());
                    ei.Put("SHIPMENT_ID", shipmentid);
                    ei.Put("STATUS", "B");  //更新DN的状态为订舱发起
                    if ("A".Equals(transactemode) || "B".Equals(transactemode) || "C".Equals(transactemode))
                    {
                        ei.Put("BL_LEVEL", transactemode);
                    }
                    else
                    {
                        ei.Put("BL_LEVEL", blleves[j]);
                        j++;
                    }
                    ml.Add(ei);

                    EditInstruct ine = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
                    ine.PutKey("DN_NO", dnitems[i]);
                    ine.Put("SHIPMENT_ID", shipmentid);
                    ml.Add(ine);

                    EditInstruct inde = new EditInstruct("SMIND", EditInstruct.UPDATE_OPERATION);
                    inde.PutKey("DN_NO", dnitems[i]);
                    inde.Put("SHIPMENT_ID", shipmentid);
                    ml.Add(inde);
                }
            }
        }

        public string SaveToBooking(string u_id,string userid)
        {
            MixedList ml = new MixedList();
            Dictionary<string, object> parm = new Dictionary<string, object>();
         
            string sql = string.Format("SELECT * FROM SMDN WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
            string combineinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBINE_INFO"]);
            string group = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string stn = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
            string dnno = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
            string dn_no_cmp_ref = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO_CMP_REF"]);
            string pol = Prolink.Math.GetValueAsString(dt.Rows[0]["POL"]);

            Func<string, string, string> checkEmpty = (column, descp) =>
            {
                string value = Prolink.Math.GetValueAsString(dt.Rows[0][column]);
                if (string.IsNullOrEmpty(value))
                    return string.Format("此笔DN{0}:{1}为空，不能对外订舱！",dnno, descp);
                return string.Empty;
            };
            List<string> checkResults = new List<string>();
            Action<string, string> check = (column, descp) =>
                {
                    var result=checkEmpty(column, descp);
                    if (string.IsNullOrEmpty(result)) return;
                    if (!checkResults.Contains(result))
                        checkResults.Add(result);
                };
            check("ETD", "预计出货日");
            check("PRODUCT_DATE", "生产日期");
            check("POL", "启运地");
            check("POD", "目的地");
            check("FREIGHT_TERM", "Freight Term");

            string co= Prolink.Math.GetValueAsString(dt.Rows[0]["COMBINE_OTHER"]);
            if ("Y".Equals(co))
                check("TCBM", "總材積");

            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            switch (trantype)
            {
                case "T":
                    check("TRACK_WAY", "陆运方式");
                    break;
                case "F":
                case "L":
                    check("LOADING_FROM", "Service Mode");
                    check("LOADING_FROM", "Service Mode");
                    break;
            }
            if (checkResults.Count > 0)
            {
                return string.Join(Environment.NewLine, checkResults);
            }

            string msg = string.Empty;
            if ("1090".Equals(stn))
            {
                switch (trantype)
                {
                    case "T":
                    case "D":
                    case "A":
                        msg = string.Empty;
                        break;
                    default:
                        msg = "此为1090段DN，非空运，内贸，国内快递情况下，无法订舱!";
                        break;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(dn_no_cmp_ref))
                {

                    msg= "此为后段DN,无法订舱!";
                }
            }
            if (!string.IsNullOrEmpty(msg))
            {
                return msg;
            }
            msg=checkStatusinfo(combineinfo);
            if(!string.IsNullOrEmpty(msg)){
                return msg;
            }

            if (string.IsNullOrEmpty(shipmentid))
            {
                shipmentid = GetSMAutoNo(group, cmp, stn);
                dt.Columns.Add("IORDER");
                foreach (DataRow dr in dt.Rows)
                {
                    dr["IORDER"] = "N";
                }
            }
            parm["SYS_SHIPMENTID"] = shipmentid;
            parm["COMBINE_INFO"] = combineinfo;
            parm["DN_NO"] = dnno;
            parm["USER_ID"] = userid;
            GetTotal(combineinfo, parm, dt.Rows[0]);

            sql = string.Format("SELECT SMDNPT.* FROM SMDNPT WHERE DN_NO={0} AND PARTY_TYPE IN {1}", Prolink.Data.SQLUtils.QuotedStr(dnno), Prolink.Data.SQLUtils.Quoted(PARTY_TYPES));
            //sql = string.Format("SELECT SMDNPT.* FROM SMDNPT WHERE DN_NO={0}", Prolink.Data.SQLUtils.QuotedStr(dnno));
            DataTable partyDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            StringBuilder partyLog = new StringBuilder();

            string ref_no = Prolink.Math.GetValueAsString(dt.Rows[0]["REF_NO"]);
            string trade_term = Prolink.Math.GetValueAsString(dt.Rows[0]["TRADE_TERM"]);
            string ref_trade = string.Empty;

             parm["DEBIT_TO"]=string.Empty;
             parm["DEBIT_NM"] = string.Empty;
            if (!string.IsNullOrEmpty(ref_no))
            {
                sql = string.Format("SELECT TRADE_TERM FROM SMDN WHERE DN_NO={0}", Prolink.Data.SQLUtils.QuotedStr(ref_no));
                ref_trade = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                //找出第一段DN 跟  第二段 DN 的 Incoterm 做比較. 
                //如果兩段DN 的 Incoterm 都依樣, 不用處理. 
                //如果不一樣處理方式如下. 
                // DN1     DN2 
                //FOB       DN2 為 CIF/DDP/DIP       請將DN1 的 SOLD_TO ID&Name 放到SMSM.Debit_to 
                if ("FOB".Equals(trade_term))
                { 
                    switch(ref_trade){
                        case "CIF":
                        case "DDP":
                        case "DIP":
                             DataRow[] drs = partyDt.Select("PARTY_TYPE='AG'");
                            if (drs.Length>0)
                            {
                                parm["DEBIT_TO"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                                parm["DEBIT_NM"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                            }
                            break;
                    }
                }
            }

            partyLog.AppendFormat(" {0} ：{1}，发起DN{2}的原始DNParty资料为：", shipmentid, combineinfo, dnno);
            foreach (DataRow dr in partyDt.Rows)
            {
                partyLog.AppendFormat("{0}:{1} \r", dr["PARTY_TYPE"].ToString(), dr["PARTY_NO"].ToString());
            }
            try
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog(partyLog.ToString());
            }
            catch (Exception ex)
            {
            }
            ml.Add(string.Format("DELETE FROM SMSM WHERE SHIPMENT_ID={0}", Prolink.Data.SQLUtils.QuotedStr(shipmentid)));
            ml.Add(string.Format("DELETE FROM SMSMPT WHERE SHIPMENT_ID={0}", Prolink.Data.SQLUtils.QuotedStr(shipmentid)));

            RegisterEditInstructFunc("DNToBookingMapping", BookingEditInstruct);
            RegisterEditInstructFunc("DnPartyMapping", PartyEditInstruct);

            //"SH", "CS", "PT", "DT","FS"
            SetPartyDatas(parm, partyDt, SET_TYPES);

            ParseEditInstruct(dt, "DNToBookingMapping", ml, parm);
            ParseEditInstruct(partyDt, "DnPartyMapping", ml,parm);

            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.Put("SHIPMENT_ID", shipmentid);
            ei.Put("STATUS", "B");  //更新状态为订舱发起
            ei.PutKey("U_ID", u_id);
            ei.Put("BL_LEVEL", "A");
            ml.Add(ei);
            UpdateCombinInfo(ml, Prolink.Math.GetValueAsString(parm["COMBINE_INFO"]), shipmentid);
            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                try
                {
                    CommonManager.UpdateSMSMPartys(shipmentid);
                    BookingParser bp = new BookingParser();
                    bp.SaveToTrackingByShimentID(new string[]{shipmentid});
                    Manager.SaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "000", Sender = userid, Location = pol, LocationName = "", StsDescp = "Internal Booking" });
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                msg += dnno + ":发起订舱失败!";
            }
            return msg;
            //CommonManager.UpdateSMSMPartys(shipmentid);
        }

        public string UpdateSMBDToSMSM(MixedList ml, string smbduid, string shipmentid,DataRow smbdrow,string shipmentuid,int editype,TransferUser tuser)
        {
            ml.Add(GetSMBDEi(smbdrow, editype, shipmentuid));
            string tranmode = Prolink.Math.GetValueAsString(smbdrow["TRAN_MODE"]);
            string shipper = Prolink.Math.GetValueAsString(smbdrow["SHPR_CD"]);
            string shippername = Prolink.Math.GetValueAsString(smbdrow["SHPR_NM"]);
            string consignee = Prolink.Math.GetValueAsString(smbdrow["CNEE_CD"]);
            string consigneename = Prolink.Math.GetValueAsString(smbdrow["CNEE_NM"]);
            string notify = Prolink.Math.GetValueAsString(smbdrow["NOTIFY_CD"]);
            string notifyname = Prolink.Math.GetValueAsString(smbdrow["NOTIFY_NM"]);
            string shipto = Prolink.Math.GetValueAsString(smbdrow["SHIPTO_CD"]);
            string shiptoname = Prolink.Math.GetValueAsString(smbdrow["SHIPTO_NM"]);
            string billto = Prolink.Math.GetValueAsString(smbdrow["BILLTO_CD"]);
            string billtoname = Prolink.Math.GetValueAsString(smbdrow["BILLTO_NM"]);
            string cmp = Prolink.Math.GetValueAsString(smbdrow["CMP"]);

            SmptyInfo shiptoinfo = new SmptyInfo
            {
                PartyName=Prolink.Math.GetValueAsString(smbdrow["SHIPTO_NM"]),
                PartAddr1 = Prolink.Math.GetValueAsString(smbdrow["PART_ADDR1"]),
                PartAddr2 = Prolink.Math.GetValueAsString(smbdrow["PART_ADDR2"]),
                PartAddr3 = Prolink.Math.GetValueAsString(smbdrow["PART_ADDR3"]),
                Cnty = Prolink.Math.GetValueAsString(smbdrow["CNTY"]),
                CntyNm = Prolink.Math.GetValueAsString(smbdrow["CNTY_NM"]),
                City = Prolink.Math.GetValueAsString(smbdrow["CITY"]),
                CityNm = Prolink.Math.GetValueAsString(smbdrow["CITY_NM"]),
                State = Prolink.Math.GetValueAsString(smbdrow["STATE"]),
                Zip = Prolink.Math.GetValueAsString(smbdrow["ZIP"]),
                PartyAttn = Prolink.Math.GetValueAsString(smbdrow["PARTY_ATTN"]),
                PartyTel = Prolink.Math.GetValueAsString(smbdrow["PARTY_TEL"])
            };

            string returnMessage = string.Empty;

            Func<string, string, string, string> SetParty = (field, partyno, partyname) =>
            {
                if (string.IsNullOrEmpty(partyno)) return string.Empty;
                EditInstruct patyei = new EditInstruct("SMSMPT", EditInstruct.DELETE_OPERATION);
                patyei.PutKey("SHIPMENT_ID", shipmentid);
                patyei.PutKey("PARTY_TYPE", field);
                ml.Add(patyei);
                if ("E".Equals(tranmode) || "T".Equals(tranmode))
                {
                    switch (field)
                    {
                        case "WE":
                            return BookingStatusManager.InsetIntoMlBySMPTY(ml, field, partyno, shipmentid, shipmentuid,shiptoinfo,partyname);
                        case "SH":
                            return BookingStatusManager.InsetIntoMlBySMPTY(ml, field, partyno, shipmentid, shipmentuid, null, partyname, tuser);
                    }
                }

                //if ("E".Equals(tranmode) && "WE".Equals(field))
                //    return BookingStatusManager.InsetIntoMlBySMPTY(ml, field, partyno, shipmentid, shipmentuid,shiptoinfo,partyname);
                //if ("E".Equals(tranmode) && "SH".Equals(field)) //如果是Shipper的话，默认带入当前用户的用户名和分机
                //    return BookingStatusManager.InsetIntoMlBySMPTY(ml, field, partyno, shipmentid, shipmentuid, null, partyname,tuser);
                return BookingStatusManager.InsetIntoMlBySMPTY(ml, field, partyno, shipmentid, shipmentuid,null, partyname);
            };
            returnMessage += SetParty("SH", shipper, shippername);
            returnMessage += SetParty("CS", consignee, consigneename);
            returnMessage += SetParty("NT", notify, notifyname);
            returnMessage += SetParty("WE", shipto, shiptoname);
            returnMessage += SetParty("RE", billto, billtoname);
             return returnMessage;
        }

        public EditInstruct GetSMBDEi(DataRow smbdrow, int edittype,string uid)
        {
            //EditInstruct.UPDATE_OPERATION
            string tranmode = Prolink.Math.GetValueAsString(smbdrow["TRAN_MODE"]);
            EditInstruct ei = new EditInstruct("SMSM", edittype);
            string dnetd=Prolink.Math.GetValueAsString(smbdrow["PICKUP_WMS_DATE"]);
            if (TrackingEDI.Business.DateTimeUtils.IsDate(dnetd))
            {
                ei.Put("WEEKLY", TrackingEDI.Business.DateTimeUtils.WeekOfYear(dnetd));
                ei.Put("MONTH", TrackingEDI.Business.DateTimeUtils.MonthOfYear(dnetd));
                ei.Put("YEAR", TrackingEDI.Business.DateTimeUtils.YearOfYear(dnetd));
            }

            //国际快递订舱画面的service栏位要求默认带出“15N:全球快递（包裹）“
            if ("E".Equals(tranmode))
            {
                ei.Put("SERVICE", "15N");
            }
            ei.PutDate("PRODUCT_DATE", dnetd);
            ei.PutDate("DN_ETD", dnetd);
            ei.PutKey("SHIPMENT_ID", Prolink.Math.GetValueAsString(smbdrow["SHIPMENT_ID"]));
            ei.PutKey("U_ID", uid);
            ei.Put("BL_TYPE", "B");
            ei.Put("PARTIAL_FLAG", "Y");    //表示分批出货的情况
            ei.Put("STATUS", "A");
            ei.Put("CUR", Prolink.Math.GetValueAsString(smbdrow["CUR"]));
            ei.Put("GVALUE", Prolink.Math.GetValueAsString(smbdrow["GVALUE"]));
            ei.Put("TRAN_TYPE", Prolink.Math.GetValueAsString(smbdrow["TRAN_MODE"]));
            ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(smbdrow["GROUP_ID"]));
            ei.Put("CMP", Prolink.Math.GetValueAsString(smbdrow["CMP"]));
            ei.Put("STN", Prolink.Math.GetValueAsString(smbdrow["STN"]));
            ei.Put("DEP", Prolink.Math.GetValueAsString(smbdrow["DEP"]));
            ei.Put("CREATE_DEP", Prolink.Math.GetValueAsString(smbdrow["DEP"]));
            ei.Put("CREATE_EXT", Prolink.Math.GetValueAsString(smbdrow["CREATE_EXT"]));
            ei.Put("GOODS", Prolink.Math.GetValueAsString(smbdrow["GOODS"]));
            ei.Put("MARKS", Prolink.Math.GetValueAsString(smbdrow["MARKS"]));
            ei.Put("INSTRUCTION", Prolink.Math.GetValueAsString(smbdrow["INSTRUCTION"]));
            ei.Put("PICKUP_WMS", Prolink.Math.GetValueAsString(smbdrow["PICKUP_WMS"]));
            ei.Put("PICKUP_WMS_NM", Prolink.Math.GetValueAsString(smbdrow["PICKUP_WMS_NM"]));
            ei.Put("PICKUP_WMS_NM", Prolink.Math.GetValueAsString(smbdrow["PICKUP_WMS_NM"]));
            ei.PutDate("PICKUP_WMS_DATE", smbdrow["PICKUP_WMS_DATE"]);
            ei.Put("QTY", Prolink.Math.GetValueAsString(smbdrow["QTY"]));
            ei.Put("QTYU", Prolink.Math.GetValueAsString(smbdrow["QTYU"]));
            ei.Put("NW", Prolink.Math.GetValueAsString(smbdrow["NW"]));
            ei.Put("GW", Prolink.Math.GetValueAsString(smbdrow["GW"]));
            ei.Put("GWU", Prolink.Math.GetValueAsString(smbdrow["GWU"]));
            ei.Put("CBM", Prolink.Math.GetValueAsString(smbdrow["CBM"]));
            ei.Put("PGW", Prolink.Math.GetValueAsString(smbdrow["GW"]));
            ei.Put("PCBM", Prolink.Math.GetValueAsString(smbdrow["CBM"]));
            ei.Put("FRT_TERM", Prolink.Math.GetValueAsString(smbdrow["FRT_TERM"]));

            ei.Put("TRACK_WAY", Prolink.Math.GetValueAsString(smbdrow["TRAN_TYPE"]));
            ei.Put("CARGO_TYPE", Prolink.Math.GetValueAsString(smbdrow["CARGO_TYPE"]));
            ei.Put("POL_CD", Prolink.Math.GetValueAsString(smbdrow["PICKUP_PORT"]));
            ei.Put("PPOL_CD", Prolink.Math.GetValueAsString(smbdrow["PICKUP_PORT"]));
            ei.Put("POR_CD", Prolink.Math.GetValueAsString(smbdrow["PICKUP_PORT"]));
            ei.Put("PPOR_CD", Prolink.Math.GetValueAsString(smbdrow["PICKUP_PORT"]));
            ei.Put("POL_NAME", Prolink.Math.GetValueAsString(smbdrow["PICKUP_NM"]));
            ei.Put("PPOL_NAME", Prolink.Math.GetValueAsString(smbdrow["PICKUP_NM"]));
            ei.Put("POR_NAME", Prolink.Math.GetValueAsString(smbdrow["PICKUP_NM"]));
            ei.Put("PPOR_NAME", Prolink.Math.GetValueAsString(smbdrow["PICKUP_NM"]));

            ei.Put("POD_CD", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_PORT"]));
            ei.Put("PPOD_CD", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_PORT"]));
            ei.Put("DEST_CD", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_PORT"]));
            ei.Put("PDEST_CD", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_PORT"]));
            ei.Put("POD_NAME", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_NM"]));
            ei.Put("PPOD_NAME", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_NM"]));
            ei.Put("PDEST_NAME", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_NM"]));
            ei.Put("DEST_NAME", Prolink.Math.GetValueAsString(smbdrow["DELIVERY_NM"]));
            ei.Put("REGION", Prolink.Math.GetValueAsString(smbdrow["REGION"]));
            ei.Put("STATE", Prolink.Math.GetValueAsString(smbdrow["STATE"]));
            ei.Put("CREATE_BY", Prolink.Math.GetValueAsString(smbdrow["CREATE_BY"]));
            ei.PutDate("CREATE_DATE", Prolink.Math.GetValueAsString(smbdrow["CREATE_DATE"]));
            ei.Put("CENT_DECL", Prolink.Math.GetValueAsString(smbdrow["CENT_DECL"]));
            ei.Put("COST_CENTER", Prolink.Math.GetValueAsString(smbdrow["COST_CENTER"]));
            ei.Put("COST_CENTERDESCP", Prolink.Math.GetValueAsString(smbdrow["COST_CENTERDESCP"]));//add by dean 问题单：109402
            ei.Put("TRADE_TERM", Prolink.Math.GetValueAsString(smbdrow["TRADE_TERM"]));
            ei.Put("TRADETERM_DESCP", Prolink.Math.GetValueAsString(smbdrow["TRADETERM_DESCP"]));
            ei.Put("INCOTERM_CD", Prolink.Math.GetValueAsString(smbdrow["TRADE_TERM"]));
            ei.Put("INCOTERM_DESCP", Prolink.Math.GetValueAsString(smbdrow["TRADETERM_DESCP"]));
            ei.Put("PKG_NUM", Prolink.Math.GetValueAsString(smbdrow["PKG_NUM"]));
            ei.Put("PKG_UNIT", Prolink.Math.GetValueAsString(smbdrow["PKG_UNIT"]));
            ei.Put("PKG_UNIT_DESC", Prolink.Math.GetValueAsString(smbdrow["PKG_UNIT_DESC"]));
            ei.PutDate("INTERNAL_BK_DATE", DateTime.Now);

            ei.Put("COMBINE_OTHER", Prolink.Math.GetValueAsString(smbdrow["COMBINE_OTHER"]));
            ei.Put("TCBM", Prolink.Math.GetValueAsString(smbdrow["TCBM"]));
            ei.Put("HORN", Prolink.Math.GetValueAsString(smbdrow["HORN"]));
            ei.Put("BATTERY", Prolink.Math.GetValueAsString(smbdrow["BATTERY"]));
            ei.Put("RELEASE_NO", Prolink.Math.GetValueAsString(smbdrow["RELEASE_NO"]));
            ei.Put("PLANT", Prolink.Math.GetValueAsString(smbdrow["PLANT"]));
            ei.Put("BORDER", 'N');
            ei.Put("CORDER", 'N');
            ei.Put("TORDER", 'N');

            string uext = BookingStatusManager.GetUserFxt(smbdrow["CREATE_BY"].ToString());
            if (string.IsNullOrEmpty(uext)) uext = smbdrow["CREATE_BY"].ToString();
            ei.Put("SALES_WIN", uext); 
            return ei;
        }

        public string SaveSmbdToBook(string smbduid,string dn_no, string userid,TransferUser tuser)
        {
            //处理没有DN的情况下转入订舱
            if (string.IsNullOrEmpty(dn_no)) return NoDnToBooking(smbduid, tuser);

            MixedList ml = new MixedList();
            Dictionary<string, object> parm = new Dictionary<string, object>();

            string sql = string.Format("SELECT *,'N'IORDER  FROM SMDN WHERE DN_NO={0}", Prolink.Data.SQLUtils.QuotedStr(dn_no));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return "没有数据,无法对内订舱";
            string shipmentid = string.Empty;
            string combineinfo = string.Empty;
            string group = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string stn = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
            string dnno = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
            string dn_no_cmp_ref = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO_CMP_REF"]);
            string pol = Prolink.Math.GetValueAsString(dt.Rows[0]["POL"]);

            string product_date = Prolink.Math.GetValueAsString(dt.Rows[0]["PRODUCT_DATE"]);
            string etd = Prolink.Math.GetValueAsString(dt.Rows[0]["ETD"]);
            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);

            if (string.IsNullOrEmpty(etd) || string.IsNullOrEmpty(product_date)) return "此笔DN预计出货日或生产日期为空，不能对外订舱！";

            string msg = string.Empty;

            if ("1090".Equals(stn))
            {
                switch (trantype)
                {
                    case "T":
                    case "D":
                    case "A":
                        msg = string.Empty;
                        break;
                    default:
                        msg = "此为1090段DN，非空运，内贸，国内快递情况下，无法订舱!";
                        break;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(dn_no_cmp_ref))
                {

                    msg = "此为后段DN,无法订舱!";
                }
            }

            sql = string.Format("SELECT * FROM SMBD WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(smbduid));
            DataTable smbddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            shipmentid = Prolink.Math.GetValueAsString(smbddt.Rows[0]["SHIPMENT_ID"]);
            combineinfo = Prolink.Math.GetValueAsString(smbddt.Rows[0]["COMBINE_INFO"]);
            string status = Prolink.Math.GetValueAsString(smbddt.Rows[0]["STATUS"]);

            if ("B".Equals(status))
            {
                return "该笔资料已经发起过订舱!";
            }

            if (string.IsNullOrEmpty(shipmentid))
            {
                shipmentid = GetSMAutoNo(group, cmp, stn);
                dt.Columns.Add("IORDER");
                foreach (DataRow dr in dt.Rows)
                {
                    dr["IORDER"] = "N";
                }
            }
            parm["SYS_SHIPMENTID"] = shipmentid;
            parm["COMBINE_INFO"] = combineinfo;
            parm["DN_NO"] = dnno;
            parm["USER_ID"] = userid;
            parm["SHIPMENT_UID"] = smbduid;

            sql = string.Format("SELECT SMDNPT.* FROM SMDNPT WHERE DN_NO={0} AND PARTY_TYPE IN {1}", Prolink.Data.SQLUtils.QuotedStr(dnno), Prolink.Data.SQLUtils.Quoted(PARTY_TYPES));
            DataTable partyDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            ml.Add(string.Format("DELETE FROM SMSM WHERE SHIPMENT_ID={0}", Prolink.Data.SQLUtils.QuotedStr(shipmentid)));
            ml.Add(string.Format("DELETE FROM SMSMPT WHERE SHIPMENT_ID={0}", Prolink.Data.SQLUtils.QuotedStr(shipmentid)));

            RegisterEditInstructFunc("DNToBookingMapping", BookingEditInstruct);
            RegisterEditInstructFunc("DnPartyMapping", PartyEditInstruct);

            //"SH", "CS", "PT", "DT","FS"
            SetPartyData(parm, partyDt, "CS");
            SetPartyData(parm, partyDt, "SH");
            SetPartyData(parm, partyDt, "PT");
            SetPartyData(parm, partyDt, "DT");
            SetPartyData(parm, partyDt, "FS");

            SetPartyData(parm, partyDt, "AG");
            SetPartyData(parm, partyDt, "ZE");
            SetPartyData(parm, partyDt, "FC");
            SetPartyData(parm, partyDt, "RE");
            SetPartyData(parm, partyDt, "WE");
            SetPartyData(parm, partyDt, "RO");
            SetPartyData(parm, partyDt, "CR");

            ParseEditInstruct(dt, "DNToBookingMapping", ml, parm);
            ParseEditInstruct(partyDt, "DnPartyMapping", ml, parm);

            ml.Add(UpdateSMBDstatus(shipmentid, smbduid));
            string sipmentuid=Prolink.Math.GetValueAsString(parm["U_ID"]);
            msg = UpdateSMBDToSMSM(ml, smbduid, shipmentid, smbddt.Rows[0], sipmentuid, EditInstruct.UPDATE_OPERATION, tuser);
            if(!string.IsNullOrEmpty(msg)) return msg;
            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                 msg += shipmentid+"发起订舱成功"; 
                try
                {
                    BookingParser bp = new BookingParser();
                    bp.SaveToTrackingByShimentID(new string[] { shipmentid });
                    Manager.SaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "000", Sender = userid, Location = pol, LocationName = "", StsDescp = "Internal Booking" });
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                msg += dnno + ":发起订舱失败," + ex.ToString() + "\n";
            }
            return msg;
        }

        public EditInstruct UpdateSMBDstatus(string shipmentid, string smbduid)
        {
            EditInstruct ei = new EditInstruct("SMBD", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("STATUS", "B");  //更新状态为订舱发起
            ei.PutKey("U_ID", smbduid);
            return ei;
        }

        /// <summary>
        /// 针对没有DN的情况执行转Booking
        /// </summary>
        /// <returns></returns>
        public string NoDnToBooking(string smbduid, TransferUser tuser)
        {
            string sql = string.Format("SELECT * FROM SMBD WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(smbduid));
            DataTable smbddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(smbddt.Rows.Count<=0) return "没有对应的分批出货资料！";
            DataRow smbdrow = smbddt.Rows[0];
            string status = Prolink.Math.GetValueAsString(smbdrow["STATUS"]);
            string shipmentid = Prolink.Math.GetValueAsString(smbdrow["SHIPMENT_ID"]);
            string cmp = Prolink.Math.GetValueAsString(smbdrow["CMP"]);
            string pickupport = Prolink.Math.GetValueAsString(smbdrow["PICKUP_PORT"]);
            switch(status){
                case "B":
                    return "该笔资料已经发起过订舱!";
                case "V":
                    return "该笔资料已经作废!";
            }
            MixedList ml = new MixedList();
            //string shipment_uid = Guid.NewGuid().ToString();
            string shipment_uid = smbduid;
            ml.Add(UpdateSMBDstatus(shipmentid, smbduid));
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.DELETE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ml.Add(ei);
            string msg = UpdateSMBDToSMSM(ml, smbduid, shipmentid, smbddt.Rows[0], shipment_uid, EditInstruct.INSERT_OPERATION, tuser);
            if (!string.IsNullOrEmpty(msg)) return msg;
            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                msg += shipmentid + "发起订舱成功";
                try
                {
                    BookingParser bp = new BookingParser();
                    bp.SaveToTrackingByShimentID(new string[] { shipmentid });
                    Manager.SaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "000", Sender = tuser.UId, Location = pickupport, LocationName = "", StsDescp = "Internal Booking" });
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
                msg += shipmentid + ":发起订舱失败," + ex.Message + "\n";
            }
            return msg;
        }

        public string SmdndToBooking(string smdn_uid)
        {
            MixedList ml = new MixedList();
            Dictionary<string, object> parm = new Dictionary<string, object>();
            string returnMessage=string.Empty;
            string smdnsql = string.Format("SELECT * FROM SMDND WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(smdn_uid));
            DataTable smdndt = OperationUtils.GetDataTable(smdnsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(smdndt.Rows.Count<0)
            {
                return "";
            }
            DataRow dr=smdndt.Rows[0];
            string dnno = Prolink.Math.GetValueAsString(dr["DN_NO"]);
            string partyno = Prolink.Math.GetValueAsString(dr["TRUCK_NO"]);
            string qty = Prolink.Math.GetValueAsString(dr["QTY"]);

            parm["QTY"] = qty;
            string sql = string.Format("SELECT * FROM SMDN WHERE DN_NO={0}", Prolink.Data.SQLUtils.QuotedStr(dnno));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string shipmentid = string.Empty;
            string group=Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            string cmp=Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string stn=Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
            if (string.IsNullOrEmpty(shipmentid))
            {
                shipmentid = GetSMAutoNo(group, cmp, stn);
            }
            parm["SYS_SHIPMENTID"] = shipmentid;
            parm["DN_NO"] = dnno;

            string uid = Guid.NewGuid().ToString();
            ml.Add(string.Format("DELETE FROM SMSM WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(uid)));
            ml.Add(string.Format("DELETE FROM SMSMPT WHERE U_FID={0}", Prolink.Data.SQLUtils.QuotedStr(uid)));

            RegisterEditInstructFunc("SMDNDToBookingMapping", SMDNDBookingEdit);

            ParseEditInstruct(dt, "SMDNDToBookingMapping", ml, parm);

            returnMessage = BookingStatusManager.InsetIntoMlBySMPTY(ml, "CR", partyno, shipmentid, uid,null);
            if (!string.IsNullOrEmpty(returnMessage))
            {
                return "";
            }

            EditInstruct ei = new EditInstruct("SMDND", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", smdn_uid);
            ei.Put("SHIPMENT_ID", shipmentid);  //更新状态为订舱发起
            ml.Add(ei);

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return string.Empty ;
                }
            }
            return shipmentid;
        }
        
        private string SMDNDBookingEdit(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string qty = Prolink.Math.GetValueAsString(parm["QTY"]);
            ei.Put("QTY", qty);
            ei.Put("BORDER", 'N');
            ei.Put("CORDER", 'C');
            ei.Put("TORDER", 'S');
            ei.Put("SHIPMENT_ID", parm["SYS_SHIPMENTID"]);
            ei.Put("DN_NO", parm["DN_NO"]);
            ei.Put("U_ID", Guid.NewGuid().ToString());
            return string.Empty;
        }

        public static void SetPartyDatas(Dictionary<string, object> parm, DataTable partyDt, string[] types)
        {
            foreach (string type in types)
            {
                SetPartyData(parm, partyDt, type);
            }
        }

        public static void SetPartyData(Dictionary<string, object> parm, DataTable partyDt, string type = "SP")
        {
            DataRow[] drs = partyDt.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(type)));
            if (drs.Length > 0)
            {
                if (type.Equals("FS"))
                {
                    parm["CARRIER"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                    parm["CARRIER_NM"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);

                    DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT * FROM BSCODE WHERE CD_TYPE='TCAR' AND AR_CD={0}",
                        SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]))),
                        null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count > 0)
                    {
                        parm["CARRIER"] = Prolink.Math.GetValueAsString(dt.Rows[0]["CD"]);
                        parm["CARRIER_NM"] = Prolink.Math.GetValueAsString(dt.Rows[0]["CD_DESCP"]);
                    }
                }
                else
                {
                    if (type.Equals("SH"))
                    {
                        parm["OEXPORTER"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                        parm["OEXPORTER_NM"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                        parm["OEXPORTER_ADDR"] = Prolink.Math.GetValueAsString(drs[0]["PART_ADDR"]);
                    }
                    else if (type.Equals("CS"))
                    {
                        parm["OIMPORTER"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                        parm["OIMPORTER_NM"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                        parm["OIMPORTER_ADDR"] = Prolink.Math.GetValueAsString(drs[0]["PART_ADDR"]);
                    }
                    parm[type + "_CD"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                    parm[type + "_NM"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                }
            }
        }
    }

    public class TransferUser{
        public string UId { get; set; }
        public string Cmp { get; set; }
        public string Stn { get; set; }
        public string Dep { get; set; }
        public string UExt { get; set; }
        public string UEmail { get; set; }
        public string UPhone { get; set; }
        public string PartyAttn { get; set; }
        public string PartyTel { get; set; }
    }

    public class CaCuLateManager
    {
        public TotalQTyCaCuLate CaCulateCombineByDn(string dnno, string combineinfo="")
        {
            TotalQTyCaCuLate tqc = new TotalQTyCaCuLate();
            if (string.IsNullOrEmpty(combineinfo))
                combineinfo = dnno;
            tqc.CombineInfo=combineinfo;
            combineinfo = combineinfo.Trim(',').Trim(';');
            string[] dnitems = combineinfo.Split(new char[] { ',', ';' });
            string sql = string.Format(@"SELECT SUM(AMOUNT1)AS AMOUNT1,
	                                SUM(CNTR_QTY)AS CNTR_QTY,
	                                SUM(FEU)AS FEU,
	                                SUM(GW)AS GW,
	                                SUM(NW)AS NW,
	                                SUM(CBM)AS CBM,
	                                SUM(COST)AS COST,
	                                SUM(QTY)AS QTY,
                                    SUM(PKG_NUM)AS PKG_NUM,
                                    MAX(CNT20)AS CNT20,
                                    MAX(CNT40)AS CNT40,
                                    MAX(CNT40HQ)AS CNT40HQ,
                                    MAX(CNT_NUMBER)AS CNT_NUMBER
                                FROM SMDN WHERE DN_NO IN {0}", Prolink.Data.SQLUtils.Quoted(dnitems));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dritem = dt.Rows[0];
                tqc.Amount1=Prolink.Math.GetValueAsString(dritem["AMOUNT1"]);
                tqc.Cbm=Prolink.Math.GetValueAsString(dritem["CBM"]);
                tqc.Cnt20=Prolink.Math.GetValueAsString(dritem["CNT20"]);
                tqc.Cnt40=Prolink.Math.GetValueAsString(dritem["CNT40"]);
                tqc.Cnt40hq=Prolink.Math.GetValueAsString(dritem["CNT40HQ"]);
                tqc.CntNumber=Prolink.Math.GetValueAsString(dritem["CNT_NUMBER"]);
                tqc.CntrQty=Prolink.Math.GetValueAsString(dritem["CNTR_QTY"]);
                tqc.Cost=Prolink.Math.GetValueAsString(dritem["COST"]);
                tqc.Feu=Prolink.Math.GetValueAsString(dritem["FEU"]);
                tqc.Gw=Prolink.Math.GetValueAsString(dritem["GW"]);
                tqc.Nw=Prolink.Math.GetValueAsString(dritem["NW"]);
                tqc.Qty=Prolink.Math.GetValueAsString(dritem["QTY"]);
                tqc.PkgNum=Prolink.Math.GetValueAsString(dritem["PKG_NUM"]);
            }

            sql = string.Format("SELECT QTY,PKG_NUM,PKG_UNIT,PKG_UNIT_DESC,GOODS,LGOODS,AC_REMARK,GROUP_ID,CMP,DEP,CATON_NUM,CATON_SUM,PALLET_NUM,PROFILE_CD,QTYU,TRANSACTE_MODE,POD,TRAN_TYPE FROM SMDN WHERE DN_NO IN {0}", Prolink.Data.SQLUtils.Quoted(dnitems));
            DataTable markdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(markdt.Rows.Count<=0)return tqc;
            List<string> goodslist = new List<string>();
            List<string> lgoodslist = new List<string>();
            List<string> instructionlist = new List<string>();
            List<string> transactemodelist = new List<string>();

            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            string pkgunit = string.Empty;
            int cartonscounts = 0;
            string pkgunitdesc = string.Empty;
            DataTable baseDt = MailTemplate.GetBaseData("'TCNT'", markdt.Rows[0]["GROUP_ID"].ToString(), markdt.Rows[0]["CMP"].ToString());
            string profile = string.Empty;
            string qtyu = markdt.Rows[0]["QTYU"].ToString();

            string region = string.Empty;
            string trantype = string.Empty;
            int cartonnum = 0;
            int pltnum = 0;
            for (int i = 0; i < markdt.Rows.Count; i++)
            {
                DataRow markdr = markdt.Rows[i];
                if (string.IsNullOrEmpty(region))
                {
                    region = Prolink.Math.GetValueAsString(markdr["POD"]);
                }
                if (string.IsNullOrEmpty(trantype))
                {
                    trantype = Prolink.Math.GetValueAsString(markdr["TRAN_TYPE"]);
                }
                if (string.IsNullOrEmpty(profile)) profile = Prolink.Math.GetValueAsString(markdr["PROFILE_CD"]);
                string transactemode = Prolink.Math.GetValueAsString(markdr["TRANSACTE_MODE"]);
                if (string.IsNullOrEmpty(transactemode) || "O".Equals(transactemode)) transactemode = "T" + i;
                onAdd(transactemodelist, transactemode);
                onAdd(goodslist, Prolink.Math.GetValueAsString(markdr["GOODS"]));
                onAdd(lgoodslist, Prolink.Math.GetValueAsString(markdr["LGOODS"]));
                onAdd(instructionlist, Prolink.Math.GetValueAsString(markdr["AC_REMARK"]));
                int pkgnum = Prolink.Math.GetValueAsInt(markdr["PKG_NUM"]);
                string pkgunitdescindex =  Prolink.Math.GetValueAsString(markdr["PKG_UNIT_DESC"]);
                pkgunit = MailTemplate.GetBaseCodeApCdValue(baseDt, "TCNT", Prolink.Math.GetValueAsString(markdr["PKG_UNIT"]));
                int cartonCount = Prolink.Math.GetValueAsInt(markdr["CATON_SUM"]);
                if (cartonCount<=0)
                    cartonCount = Prolink.Math.GetValueAsInt(markdr["QTY"]);
                tqc.TotalPackageInfos.Add(new TotalPackageInfo(pkgnum, pkgunit, pkgunitdescindex, Prolink.Math.GetValueAsInt(markdr["PALLET_NUM"])));
                cartonscounts += cartonCount;
                cartonnum += Prolink.Math.GetValueAsInt(markdr["CATON_NUM"]);
                pltnum += Prolink.Math.GetValueAsInt(markdr["PALLET_NUM"]);
            }
            tqc.CartonsNum = cartonnum;
            tqc.CartonsSum = cartonscounts;
            tqc.PalletsNum = pltnum;
            tqc.DeclNum = transactemodelist.Count().ToString();
            DataView dataView = markdt.DefaultView;
            DataTable dataTableDistinct = dataView.ToTable(true,"PKG_UNIT");
            tqc.CalculLateUnitDesc();
            tqc.Goods = string.Join(Environment.NewLine, goodslist);
            tqc.Lgoods = string.Join(Environment.NewLine, lgoodslist);
            tqc.Instruction = string.Join(Environment.NewLine, instructionlist);
            tqc.Marks = CalculateMarks(combineinfo);


            string sminmsql = string.Format(@"SELECT MODEL_HEADER,PKG_DESCP,PO_HEADER,PART_HEADER,CONTACT_NO1,CONTACT_NO2,CONTACT_NO3,
            BL_REMARK1,BL_REMARK2,BL_REMARK3,BL_REMARK4,BL_REMARK5,BL_REMARK6,BL_TYPE FROM SMSIM WHERE PROFILE={0}", SQLUtils.QuotedStr(profile));
            DataTable smsimDt = OperationUtils.GetDataTable(sminmsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string bltype = string.Empty;
            if (smsimDt.Rows.Count > 0)
            {
                bltype = Prolink.Math.GetValueAsString(smsimDt.Rows[0]["BL_TYPE"]);
                if (!string.IsNullOrEmpty(bltype))
                    bltype = bltype.ToUpper();
                if ("TELEX RELEASE BL".Equals(bltype))
                    tqc.TelexRls = "Y";
                tqc.SvcContact = Prolink.Math.GetValueAsString(smsimDt.Rows[0]["CONTACT_NO1"]);
            }
            tqc.BlRmk = CalculateGetBlRmk(smsimDt, dnitems, qtyu, region, trantype, tqc, markdt.Rows[0]["DEP"].ToString());
            return tqc;
        }

        private void CalutePkgInfo(string pkgunit, ref int totalcarton, ref int totalpallet, ref int sumcarton, int pkgnum, string pkgunitdescindex)
        {
            try
            {
                //22  PLT   22PALLET=4400CARTON         
                //8   PKG   4CARTON+4PALLET=2004CARTON
                switch (pkgunit)
                {
                    case "CTN":
                        totalcarton += pkgnum;
                        sumcarton += pkgnum;
                        break;
                    case "PLT":
                        totalpallet += pkgnum;
                        string[] plts = pkgunitdescindex.Split('=');
                        if (plts.Length >= 2)
                        {
                            sumcarton += AfterReplaseToInt(plts[1], "CARTON");
                        }
                        break;
                    case "PKG":
                        string[] pkgs = pkgunitdescindex.Split('=');  //5Package=2Pallet+3CARTON=5CARTON
                        if (pkgs.Length >= 3)
                        {
                            string[] pcs = pkgs[1].Split('+');
                            totalpallet += AfterReplaseToInt(pcs[0].ToUpper(), "PALLET");
                            totalcarton += AfterReplaseToInt(pcs[1].ToUpper(), "CARTON");
                            sumcarton += AfterReplaseToInt(pkgs[2], "CARTON");
                        }
                        break;
                }
            }catch(Exception ex){
            }
        }

        public int AfterReplaseToInt(string oldstr,string replacestr)
        {
            return Prolink.Math.GetValueAsInt(oldstr.Replace(replacestr, ""));
        }

        public TotalQTyCaCuLate CaCulateCombinBySM(string shippmentinfo)
        {
            string[] shippments=shippmentinfo.Split(',');
            return CaCulateCombinBySMArray(shippments);
        }

        public TotalQTyCaCuLate CaCulateCombinBySMArray(string[] shippmentarray)
        {
            string dnno = string.Empty;
            string sql = string.Format("SELECT COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID IN {0}", Prolink.Data.SQLUtils.Quoted(shippmentarray));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> dnlist = new List<string>();
            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            foreach (DataRow dr in dt.Rows)
            {
                string combine = dr["COMBINE_INFO"].ToString();
                string[] combines = combine.Split(',');
                foreach (string dn in combines)
                {
                    onAdd(dnlist, dn);
                }
            }
            if (dnlist.Count > 0)
                dnno = string.Join(",", dnlist);
            return CaCulateCombineByDn(dnno);
        }

        public void CaCulatePutEi(ref EditInstruct ei, TotalQTyCaCuLate tqc,string shipmentid,MixedList ml)
        {
            if (ei == null) ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("GVALUE", tqc.Amount1);
            //ei.Put("CNTR_QTY",tqc.CntrQty);
            //ei.Put("FEU",tqc.Feu);
            ei.Put("GW",tqc.Gw);
            ei.Put("PGW", tqc.Gw);
            ei.Put("NW",tqc.Nw);
            ei.Put("CBM",tqc.Cbm);
            ei.Put("PCBM", tqc.Cbm);
            //ei.Put("COST",tqc.Cost);
            ei.Put("QTY",tqc.Qty);
            ei.Put("CNT20",tqc.Cnt20);
            ei.Put("CNT40",tqc.Cnt40);
            ei.Put("CNT40HQ", tqc.Cnt40hq);
            ei.Put("PCNT20", tqc.Cnt20);
            ei.Put("PCNT40", tqc.Cnt40);
            ei.Put("PCNT40HQ",  tqc.Cnt40hq);
            ei.Put("PCNT_NUMBER", tqc.CntNumber);
            ei.Put("CNT_NUMBER",tqc.CntNumber);
            ei.Put("PKG_NUM", tqc.PkgNum);
            ei.Put("PKG_UNIT", tqc.PkgUnit);
            ei.Put("PKG_UNIT_DESC",tqc.PkgUnitDesc);
            ei.Put("COMBINE_INFO",tqc.CombineInfo);
            ei.Put("LGOODS", tqc.Lgoods);
            ei.Put("GOODS", tqc.Goods);
            ei.Put("INSTRUCTION",tqc.Instruction);
            ei.Put("MARKS", tqc.Marks);
            ei.Put("SVC_CONTACT", tqc.SvcContact);
            ei.Put("BL_RMK", tqc.BlRmk);
            ei.Put("DECL_NUM", tqc.DeclNum);
            //ei.Put("TELEX_RLS", tqc.TelexRls);

            if (ml != null)
            {
                EditInstruct smrvei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                smrvei.PutKey("SHIPMENT_ID", shipmentid);
                smrvei.Put("GW", tqc.Gw);
                smrvei.Put("CBM", tqc.Cbm);
                smrvei.PutExpress("TTL_VGM", "TARE_WEIGHT+" + tqc.Gw);
                ml.Add(smrvei);
            }
        }

        public string CalculateMarks(string combineinfo)
        {
            string[] dnitems = combineinfo.Split(new char[] { ',', ';' });
            string sql = string.Format("SELECT SHIP_MARK,PKG_UNIT FROM SMDN WHERE DN_NO IN {0}", Prolink.Data.SQLUtils.Quoted(dnitems));
            DataTable markdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (markdt.Rows.Count <= 0) return "";
            string itemsmark = string.Empty;
            for (int i = 0; i < markdt.Rows.Count; i++)
            {
                DataRow markdr = markdt.Rows[i];
                string markindex = Prolink.Math.GetValueAsString(markdr["SHIP_MARK"]);
                if (markindex.IndexOf("SHIPPINGMARK:") < 0 && string.IsNullOrEmpty(markindex))
                {
                    if (string.IsNullOrEmpty(markindex)) continue;
                    itemsmark += markindex + "\r\n";
                    continue;
                }
                markindex = markindex.Replace("SHIPPINGMARK:", "");//.Replace("NO:","");

                string[] marksa = markindex.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                string markitem = string.Empty;
                foreach (string mark in marksa)
                {
                    markitem += mark.Trim() + "\r\n";
                }
                if (string.IsNullOrEmpty(markitem)) continue;
                markitem = markitem.Trim();
                itemsmark += markitem + "\r\n";
            }
            itemsmark = itemsmark.Trim();
            return itemsmark;
        }

        public string CalculateGetBlRmk(DataTable dt, string[] dnnos, string qtyu,string region,string trantype,TotalQTyCaCuLate tqc,string dep)
        {
            string qty = Prolink.Math.GetValueAsString(tqc.Qty);
            string pkgnum = tqc.PkgNum.ToString();
            string pkgunit = Prolink.Math.GetValueAsString(tqc.PkgUnit);
            string pkgunitdesc = Prolink.Math.GetValueAsString(tqc.PkgUnitDesc);
            if (dt.Rows.Count <= 0) return string.Empty;
            DataRow dr = dt.Rows[0];
            string poHeader = Prolink.Math.GetValueAsString(dr["PO_HEADER"]);
            string partHeader = Prolink.Math.GetValueAsString(dr["PART_HEADER"]);
            string modelHeader = Prolink.Math.GetValueAsString(dr["MODEL_HEADER"]);
            string pkgDescp = Prolink.Math.GetValueAsString(dr["PKG_DESCP"]);
            StringBuilder sb = new StringBuilder();

            string sql = string.Format("SELECT PO_NO,PART_NO,OPART_NO FROM SMDNP WHERE DN_NO IN {0}", SQLUtils.Quoted(dnnos));
            DataTable smdnpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Func<string, string, string> SetBlRemark = (field, value) =>
            {
                if (string.IsNullOrEmpty(value)) return string.Empty;
                DataView dataView = smdnpdt.DefaultView;
                DataTable smdnpdtDistinct = dataView.ToTable(true, field);
                return value += string.Join("/", smdnpdtDistinct.Rows.Cast<DataRow>().Select(row =>
                   Prolink.Math.GetValueAsString(row[field])).Where(s => !string.IsNullOrEmpty(s)));
            };
            List<string> items = new List<string>();
            Action<string> onAdd = txt =>
            {
                if (string.IsNullOrEmpty(txt)) return;
                //if (items.Contains(txt)) return;
                items.Add(txt);
            };
            onAdd(SetBlRemark("PO_NO", poHeader));
            onAdd(SetBlRemark("OPART_NO", modelHeader));
            onAdd(SetBlRemark("PART_NO", partHeader));
            if ("Y".Equals(pkgDescp)) { 
                onAdd(string.Format("QTY:{0} {1}", qty, qtyu));
                if ("F".Equals(trantype) || "L".Equals(trantype))
                {
                    //if ("CS".Equals(dep))
                    //{ 
                        if ("PLT".Equals(pkgunit))
                        {
                            onAdd(string.Format("{0}PACKAGES={0}PALLETS={1}CARTONS", pkgnum, tqc.CartonsSum));
                        }
                        else if ("PKG".Equals(pkgunit))
                        {
                            onAdd(string.Format("{0}PACKAGES={1}PALLETS+{2}CARTONS={3}CARTONS", pkgnum,tqc.PalletsNum, tqc.CartonsNum,tqc.CartonsSum));
                        }
                        else
                        {
                            onAdd(string.Format("{0}PACKAGES={1}CARTONS", pkgnum, tqc.CartonsSum));
                        }
                }
            }
            if ("PLT".Equals(pkgunit))
            {
                tqc.PkgUnitDesc = string.Format("{0}PALLETS={1}CARTONS", pkgnum, tqc.CartonsSum);
            }
            else if ("PKG".Equals(pkgunit))
            {
                tqc.PkgUnitDesc = string.Format("{0}PALLETS+{1}CARTONS={2}CARTONS", tqc.PalletsNum, tqc.CartonsNum, tqc.CartonsSum);
            }
            else
            {
                tqc.PkgUnitDesc = string.Format("{0}CARTONS", tqc.CartonsSum);
            }
            //onAdd(qty);
            onAdd(Prolink.Math.GetValueAsString(dr["BL_REMARK1"]));
            onAdd(Prolink.Math.GetValueAsString(dr["BL_REMARK2"]));
            onAdd(Prolink.Math.GetValueAsString(dr["BL_REMARK3"]));
            onAdd(Prolink.Math.GetValueAsString(dr["BL_REMARK4"]));
            onAdd(Prolink.Math.GetValueAsString(dr["BL_REMARK5"]));
            onAdd(Prolink.Math.GetValueAsString(dr["BL_REMARK6"]));
            return string.Join(Environment.NewLine, items);
        }

        public CombinBLInfo GetCombinBLInfo(string shippmentinfo)
        {
            string[] shippments = shippmentinfo.Split(',');
            CombinBLInfo Cblinfo = new CombinBLInfo();
            string sql = string.Format(@"SELECT SUM(GVALUE)AS GVALUE,
	                                SUM(GW)AS GW,
	                                SUM(PGW)AS PGW,
	                                SUM(NW)AS NW,
	                                SUM(CBM)AS CBM,
                                    SUM(PCBM)AS PCBM,
	                                SUM(QTY)AS QTY,
                                    SUM(CNT20)AS CNT20,
                                    SUM(CNT40)AS CNT40,
                                    SUM(CNT40HQ)AS CNT40HQ,
                                    SUM(CNT_NUMBER)AS CNT_NUMBER,
                                    SUM(PCNT20)AS PCNT20,
                                    SUM(PCNT40)AS PCNT40,
                                    SUM(PCNT40HQ)AS PCNT40HQ,
                                    SUM(PCNT_NUMBER)AS PCNT_NUMBER
                                FROM SMSM WHERE SHIPMENT_ID IN {0}", Prolink.Data.SQLUtils.Quoted(shippments));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dritem = dt.Rows[0];
                Cblinfo.Gvalue = Prolink.Math.GetValueAsString(dritem["GVALUE"]);
                Cblinfo.Gw = Prolink.Math.GetValueAsString(dritem["GW"]); 
                Cblinfo.Pgw = Prolink.Math.GetValueAsString(dritem["PGW"]);
                Cblinfo.Cnt20 = Prolink.Math.GetValueAsString(dritem["CNT20"]);
                Cblinfo.Cnt40 = Prolink.Math.GetValueAsString(dritem["CNT40"]);
                Cblinfo.Cnt40hq = Prolink.Math.GetValueAsString(dritem["CNT40HQ"]);
                Cblinfo.CntNumber = Prolink.Math.GetValueAsString(dritem["CNT_NUMBER"]);
                Cblinfo.Pcnt20 = Prolink.Math.GetValueAsString(dritem["PCNT20"]);
                Cblinfo.Pcnt40 = Prolink.Math.GetValueAsString(dritem["PCNT40"]);
                Cblinfo.Pcnt40hq = Prolink.Math.GetValueAsString(dritem["PCNT40HQ"]);
                Cblinfo.PcntNumber = Prolink.Math.GetValueAsString(dritem["PCNT_NUMBER"]);
                Cblinfo.Cbm = Prolink.Math.GetValueAsString(dritem["CBM"]);
                Cblinfo.Pcbm = Prolink.Math.GetValueAsString(dritem["PCBM"]);
                Cblinfo.Nw = Prolink.Math.GetValueAsString(dritem["NW"]);
                Cblinfo.Qty = Prolink.Math.GetValueAsString(dritem["QTY"]);
            }
            sql = string.Format("SELECT SO_NO,SHIPMENT_INFO,INSTRUCTION FROM SMSM WHERE SHIPMENT_ID IN {0}", Prolink.Data.SQLUtils.Quoted(shippments));
            DataTable smsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smsmdt.Rows.Count <= 0) return Cblinfo;
            List<string> sonolist = new List<string>();
            List<string> shippmentlist = new List<System.String>(shippments);
            List<string> instructionlist = new List<System.String>(shippments);
            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            for (int i = 0; i < smsmdt.Rows.Count; i++)
            {
                DataRow smsmdr = smsmdt.Rows[i];
                onAdd(sonolist, Prolink.Math.GetValueAsString(smsmdr["SO_NO"]));
                onAdd(shippmentlist, Prolink.Math.GetValueAsString(smsmdr["SHIPMENT_INFO"]));
                //onAdd(instructionlist, Prolink.Math.GetValueAsString(smsmdr["INSTRUCTION"]));
            }
            Cblinfo.SoNo = string.Join(",", sonolist);
            Cblinfo.ShipmentInfo = string.Join(",", shippmentlist);
            string instructionremark = "Booking No  Include ";
            onAdd(instructionlist, instructionremark);
            onAdd(instructionlist, string.Join(";", sonolist));
            Cblinfo.Instruction = string.Join(Environment.NewLine, instructionlist);
            return Cblinfo;
        }

        public void GetCombinBLEI(ref EditInstruct ei, CombinBLInfo tqc)
        {
            if (ei == null) ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Put("GVALUE", tqc.Gvalue);
            ei.Put("GW", tqc.Gw);
            ei.Put("PGW", tqc.Pgw);
            ei.Put("NW", tqc.Nw);
            ei.Put("CBM", tqc.Cbm);
            ei.Put("PCBM", tqc.Pcbm);
            ei.Put("QTY", tqc.Qty);
            ei.Put("CNT20", tqc.Cnt20);
            ei.Put("CNT40", tqc.Cnt40);
            ei.Put("CNT40HQ", tqc.Cnt40hq);
            ei.Put("PCNT20", tqc.Pcnt20);
            ei.Put("PCNT40", tqc.Pcnt40);
            ei.Put("PCNT40HQ", tqc.Pcnt40hq);
            ei.Put("PCNT_NUMBER", tqc.PcntNumber);
            ei.Put("CNT_NUMBER", tqc.CntNumber);
            //ei.Put("SO_NO", tqc.SoNo);
            ei.Put("SO_NO", "");
            //ei.Put("SHIPMENT_INFO", tqc.ShipmentInfo);
            ei.Put("INSTRUCTION", tqc.Instruction);
        }
    }

     public class TotalPackageInfo
    {
        public int Pkg_Num { get; set; }
        public string Unit { get; set; }
        public string Unit_Desc { get; set; }
        public int Pallet_Num { get; set; }
        public TotalPackageInfo(int pkgnum,string unit,string unitdesc,int pallets)
        {
            Pkg_Num = pkgnum;
            Unit = unit;
            Unit_Desc=unitdesc;
            Pallet_Num = pallets;
        }
    }

    public class TotalQTyCaCuLate
    {
        public string Amount1 { get; set; }
        public string CntrQty { get; set; }
        public string Feu { get; set; }
        public string Gw { get; set; }
        public string Nw { get; set; }
        public string Cbm { get; set; }
        public string Cost { get; set; }
        public string Qty { get; set; }
        public string Cnt20 { get; set; }
        public string Cnt40 { get; set; }
        public string Cnt40hq { get; set; }
        public string CntNumber { get; set; }
        public string PkgNum { get; set; }
        public string PkgUnit { get; set; }
        public string PkgUnitDesc { get; set; }
        public string CombineInfo { get; set; }

        public string Lgoods { get; set; }
        public string Goods { get; set; }
        public string Instruction { get; set; }
        public string Marks { get; set; }
        public string SvcContact { get; set; }
        public string BlRmk { get; set; }
        public string TelexRls { get; set; }
        public string DeclNum { get; set; }
        public int CartonsNum { get; set; }
        public int PalletsNum { get; set; }

        public List<TotalPackageInfo> TotalPackageInfos{ get; private set; }

        public TotalQTyCaCuLate()
        {
            TotalPackageInfos = new List<TotalPackageInfo>();
        }

        public void CalculLateUnitDesc()
        {
            var pkgList= TotalPackageInfos.Select(l => l.Unit).Distinct().ToList();
            if (pkgList.Count == 1)
            {
                this.PkgUnit = pkgList.First();
                this.PkgNum = TotalPackageInfos.Select(l => l.Pkg_Num).Sum().ToString();
                this.PkgUnitDesc = string.Join("+", TotalPackageInfos.Select(l => l.Unit_Desc));
            }
            else if (pkgList.Count > 1)
            {
                this.PkgUnit = "PKG";
                this.PkgNum = TotalPackageInfos.Select(l => l.Pkg_Num).Sum().ToString();
                this.PkgUnitDesc = string.Join("+", TotalPackageInfos.Select(l => l.Unit_Desc));
            }
        }

        public int CartonsSum { get; set; }
    }

    public class CombinBLInfo
    {
        public string Gvalue { get; set; }
        public string Gw { get; set; }
        public string Pgw { get; set; }
        public string Nw { get; set; }
        public string Cbm { get; set; }
        public string Pcbm { get; set; }
        public string Qty { get; set; }
        public string Cnt20 { get; set; }
        public string Cnt40 { get; set; }
        public string Cnt40hq { get; set; }
        public string Pcnt20 { get; set; }
        public string Pcnt40 { get; set; }
        public string Pcnt40hq { get; set; }
        public string PcntNumber { get; set; }
        public string CntNumber { get; set; }
        public string SoNo { get; set; }
        public string ShipmentInfo { get; set; }
        public string Instruction { get; set; }
    }
    //public class TotalQTyCaCuLate
}
