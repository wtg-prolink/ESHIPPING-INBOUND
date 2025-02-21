using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Prolink.Data;
using Prolink.DataOperation;

namespace TrackingEDI.Business
{
    /// <summary>
    /// 订舱信息转到traking
    /// </summary>
    public class BookingParser : BaseParser
    {
        static string PartyEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string u_id = Prolink.Math.GetValueAsString(parm["JOB_NO"]);
            ei.Put("U_ID", u_id);
            if (string.IsNullOrEmpty(ei.Get("PARTY_NO")))
                return BaseParser.ERROR;
            return string.Empty;
        }

        static string BookingEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            SetPort("POR", dr, ei);
            SetPort("POL", dr, ei);
            SetPort("VIA", dr, ei);
            SetPort("POD", dr, ei);
            SetPort("DEST", dr, ei);
            string u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
            string tran_type = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            //switch (tran_type)
            //{
            //    case "A":
            //        tran_type = "A"; break;//Air 
            //    case "L":
            //        tran_type = "S"; break;//
            //    case "F":
            //        tran_type = "S"; break;//Sea 
            //    case "E":
            //        tran_type = "E"; break;//Express
            //    case "D":
            //        tran_type = "R"; break;//Railroad
            //    case "S":
            //        tran_type = "6"; break;
            //}
            string dcd = "0";
            if (parm != null && parm.ContainsKey("CD"))
            {
                dcd = Prolink.Math.GetValueAsString(parm["CD"]);
                if (string.IsNullOrEmpty(dcd)) dcd = "0";
            }
            ei.PutDate("SHIPPING_DATE", System.DateTime.Now);
            ei.Put("CSTATUS", dcd);
            ei.Put("TRAN_TYPE", tran_type);

            Func<string, string> setP = type =>
            {
                string name=type+"_CD";
                if (parm.ContainsKey(name))
                    ei.Put(name, Prolink.Math.GetValueAsString(parm[name]));
                name = type + "_NM";
                if (parm.ContainsKey(name))
                    ei.Put(name, Prolink.Math.GetValueAsString(parm[name]));
                return string.Empty;
            };
            setP("FC");
            setP("CS");
            setP("SH");
            setP("SP");
            setP("AG");
            string sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
                ei.OperationType = EditInstruct.UPDATE_OPERATION;
            return string.Empty;
        }

        private static void SetPort(string name,DataRow dr, EditInstruct ei)
        {
            try
            {
                string cd = Prolink.Math.GetValueAsString(dr[name + "_CD"]);
                string cnty = Prolink.Math.GetValueAsString(dr[name + "_CNTY"]);
                if (cd.Length > 3)
                {
                    ei.Put(name + "_CD", cd);
                }
                else
                {
                    ei.Put(name + "_CD", cnty + cd);
                }
            }
            catch (Exception ex)
            {
            }
            
        }

        public void SaveToTrackingByShimentID(IEnumerable<string> shipmentIDList)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID IN({0})",
                string.Join(",", shipmentIDList.Select(s => SQLUtils.QuotedStr(s))));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return;
            foreach (DataRow row in dt.Rows)
            {
                DataTable clDT = dt.Clone();
                if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(row["C_MASTER_NO"])))
                {
                    row["C_MASTER_NO"] = row["MASTER_NO"].ToString();
                }
                clDT.ImportRow(row);
                SaveToTracking(clDT);
            }
        }

        public void SaveToTracking(string u_id)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return;
            foreach (DataRow row in dt.Rows)
            {
                DataTable clDT = dt.Clone();
                if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(row["C_MASTER_NO"])))
                {
                    row["C_MASTER_NO"] = row["MASTER_NO"].ToString();
                }
                clDT.ImportRow(row);
                SaveToTracking(clDT);
            }
        }

        void SaveToTracking(DataTable dt)
        {
            if (dt == null || dt.Rows.Count <= 0) return;
            Dictionary<string, object> parm = new Dictionary<string, object>();
            MixedList ml = new MixedList();
            DataRow row = dt.Rows[0];
            string u_id = Prolink.Math.GetValueAsString(row["U_ID"]);
            string shipment_id = Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]);
            parm["CD"] = OperationUtils.GetValueAsString(string.Format("SELECT TOP 1 CD FROM BSCODE WHERE CD_TYPE='TKLC' AND GROUP_ID={0} ORDER BY ORDER_BY",
                SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(row["GROUP_ID"]))), Prolink.Web.WebContext.GetInstance().GetConnection());
            parm["JOB_NO"] = u_id;

            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
            //string sql = string.Format("SELECT * FROM SMSMPT WHERE U_FID={0} AND SHIPMENT_ID={1}", Prolink.Data.SQLUtils.QuotedStr(u_id), Prolink.Data.SQLUtils.QuotedStr(shipment_id));
            DataTable partyDt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable partyDt = partyDt1.Clone();
            List<string> partys = new List<string>();
            foreach (DataRow dr in partyDt1.Rows)
            {
                string key = Prolink.Math.GetValueAsString(dr["PARTY_NO"]) + "#" + Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]) + "#" + Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (!partys.Contains(key))
                {
                    partyDt.ImportRow(dr);
                    partys.Add(key);
                }
            }
            sql = string.Format("SELECT * FROM (SELECT * FROM SMRV WHERE SHIPMENT_ID={0} AND CNTR_NO IS NOT NULL) A OUTER APPLY (SELECT TOP 1 LOADING_FROM,LOADING_TO FROM SMSM WHERE SMSM.SHIPMENT_ID=A.SHIPMENT_ID) B", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
            //sql = string.Format("SELECT * FROM SMRV WHERE SHIPMENT_ID={0} AND CNTR_NO IS NOT NULL", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
            DataTable containerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (containerDt == null || containerDt.Rows.Count <= 0)
            {
                sql = string.Format("SELECT * FROM (SELECT * FROM SMRV WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT={0}) AND CNTR_NO IS NOT NULL) A OUTER APPLY (SELECT TOP 1 LOADING_FROM,LOADING_TO FROM SMSM WHERE SMSM.SHIPMENT_ID=A.SHIPMENT_ID) B", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
                containerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            ml.Add(string.Format("DELETE FROM TKBLPT WHERE SHIPMENT_ID={0} OR U_ID={1}", Prolink.Data.SQLUtils.QuotedStr(shipment_id), Prolink.Data.SQLUtils.QuotedStr(u_id)));
            //ml.Add(string.Format("DELETE FROM TKBLPT WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id)));
            ml.Add(string.Format("DELETE FROM TKBLCNTR WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(u_id)));

            RegisterEditInstructFunc("BookingMapping", BookingEditInstruct);
            RegisterEditInstructFunc("BookingPartyMapping", PartyEditInstruct);
            RegisterEditInstructFunc("BookingContainerMapping", ContainerEditInstruct);

            SetPartyData(parm, partyDt, "FC");
            SetPartyData(parm, partyDt, "CS");
            SetPartyData(parm, partyDt, "SH");
            SetPartyData(parm, partyDt, "SP");
            SetPartyData(parm, partyDt, "AG");

            ParseEditInstruct(dt, "BookingMapping", ml, parm);
            ParseEditInstruct(partyDt, "BookingPartyMapping", ml, parm);

            //DataTable dnDt = OperationUtils.GetDataTable(string.Format("SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0}"), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            ParseEditInstruct(containerDt, "BookingContainerMapping", ml, parm);
            //新增货况通知的栏位
            EvenFactory.AddOnceEven(u_id, u_id, EvenManager.TrackingEven, ml);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

            try
            {
                //更新主表货柜
                OperationUtils.ExecuteUpdate(string.Format("UPDATE TKBL SET CNTR_NO=(SELECT DISTINCT CNTR_NO+';' FROM TKBLCNTR WHERE JOB_NO={0} AND CNTR_NO IS NOT NULL FOR XML PATH('')) WHERE U_ID={0}", SQLUtils.QuotedStr(u_id)), Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { }
            //EvenNotify.Notify(u_id);
            //SendIport(u_id);
        }

        public void SaveToTrackingByIBShipmentid(IEnumerable<string> shipmentIDList)
        {
            string sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID IN({0})",
                string.Join(",", shipmentIDList.Select(s => SQLUtils.QuotedStr(s))));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return;
            foreach (DataRow row in dt.Rows)
            {
                DataTable clDT = dt.Clone();
                clDT.ImportRow(row);
                IBSaveToTracking(clDT);
            }
        }

        void IBSaveToTracking(DataTable dt)
        {
            if (dt == null || dt.Rows.Count <= 0) return;
            Dictionary<string, object> parm = new Dictionary<string, object>();
            MixedList ml = new MixedList();
            DataRow row = dt.Rows[0];
            string u_id = Prolink.Math.GetValueAsString(row["U_ID"]);
            string shipment_id = Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]);
            parm["CD"] = OperationUtils.GetValueAsString(string.Format("SELECT TOP 1 CD FROM BSCODE WHERE CD_TYPE='TKLC' AND GROUP_ID={0} ORDER BY ORDER_BY",
                SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(row["GROUP_ID"]))), Prolink.Web.WebContext.GetInstance().GetConnection());
            parm["JOB_NO"] = u_id;

            string sql = string.Format("SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0}", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
            DataTable partyDt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable partyDt = partyDt1.Clone();
            List<string> partys = new List<string>();
            foreach (DataRow dr in partyDt1.Rows)
            {
                string key = Prolink.Math.GetValueAsString(dr["PARTY_NO"]) + "#" + Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]) + "#" + Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (!partys.Contains(key))
                {
                    partyDt.ImportRow(dr);
                    partys.Add(key);
                }
            }
            sql = string.Format("SELECT * FROM (SELECT * FROM SMIRV WHERE SHIPMENT_ID={0} AND CNTR_NO IS NOT NULL) A OUTER APPLY (SELECT TOP 1 LOADING_FROM,LOADING_TO FROM SMSM WHERE SMSM.SHIPMENT_ID=A.SHIPMENT_ID) B", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
            DataTable containerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (containerDt == null || containerDt.Rows.Count <= 0)
            {
                sql = string.Format(@"SELECT * FROM (SELECT * FROM SMIRV WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMSMI WHERE COMBIN_SHIPMENT={0}) AND CNTR_NO IS NOT NULL) A OUTER APPLY (SELECT TOP 1 LOADING_FROM,LOADING_TO FROM SMSM WHERE SMSM.SHIPMENT_ID=A.SHIPMENT_ID) B", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
                containerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            ml.Add(string.Format("DELETE FROM TKBLPT WHERE SHIPMENT_ID={0} OR U_ID={1}", Prolink.Data.SQLUtils.QuotedStr(shipment_id), Prolink.Data.SQLUtils.QuotedStr(u_id)));
            ml.Add(string.Format("DELETE FROM TKBLCNTR WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(u_id)));

            RegisterEditInstructFunc("BookingMapping", BookingEditInstruct);
            RegisterEditInstructFunc("BookingPartyMapping", PartyEditInstruct);
            RegisterEditInstructFunc("BookingContainerMapping", ContainerEditInstruct);

            SetPartyData(parm, partyDt, "FC");
            SetPartyData(parm, partyDt, "CS");
            SetPartyData(parm, partyDt, "SH");
            SetPartyData(parm, partyDt, "SP");
            SetPartyData(parm, partyDt, "AG");

            ParseEditInstruct(dt, "BookingMapping", ml, parm);
            ParseEditInstruct(partyDt, "BookingPartyMapping", ml, parm);

            ParseEditInstruct(containerDt, "BookingContainerMapping", ml, parm);
            EvenFactory.AddOnceEven(u_id, u_id, EvenManager.TrackingEven, ml);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

            try
            {
                OperationUtils.ExecuteUpdate(string.Format("UPDATE TKBL SET CNTR_NO=(SELECT DISTINCT CNTR_NO+';' FROM TKBLCNTR WHERE JOB_NO={0} AND CNTR_NO IS NOT NULL FOR XML PATH('')) WHERE U_ID={0}", SQLUtils.QuotedStr(u_id)), Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { }
        }


        private static void SetPartyData(Dictionary<string, object> parm, DataTable partyDt, string type = "SP")
        {
            DataRow[] drs = partyDt.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(type)));
            parm[type + "_CD"] = "";
            parm[type + "_NM"] = "";
            if (drs.Length > 0)
            {
                parm[type + "_CD"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                parm[type + "_NM"] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
            }
        }

        private string ContainerEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            //int seq_no = 0;
            //if (parm != null && parm.ContainsKey("SEQ_NO"))
            //{
            //    seq_no = Prolink.Math.GetValueAsInt(parm["SEQ_NO"]);
            //}
            //ei.Put("SEQ_NO", seq_no);
            //seq_no++;
            //parm["SEQ_NO"] = seq_no;

            ei.Put("JOB_NO", parm["JOB_NO"]);
            return string.Empty;
        }

        /// <summary>
        /// 通知EDI HUB要求货况
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="bl"></param>
        public void SendIport(string u_id, DataRow bl = null)
        {
            TrackingEDI.Utils.TraceStatusHelper.SendIport(u_id, bl);
            if (true)
                return;

            StringBuilder sb = new StringBuilder();
            XmlDocument xmldoc = new XmlDocument();
            string houseNo = string.Empty;
            string sql = string.Empty;
            try
            {
                string path = Prolink.Web.WebContext.GetInstance().GetProperty("itracePath");
                xmldoc.Load(path);
                if (bl == null)
                {
                    sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count <= 0)
                        return;
                    bl = dt.Rows[0];
                }
                houseNo = Prolink.Math.GetValueAsString(bl["HOUSE_NO"]);
                XmlNode node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "MessageSender");
                node.InnerText = "TPV";

                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "MessageReceiver");
                node.InnerText = "itrace";

                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "MessageCode");
                string tran_type = Prolink.Math.GetValueAsString(bl["TRAN_TYPE"]);
                if ("A".Equals(tran_type))
                    node.InnerText = "ITRACE_RQ_AIR";
                else if ("F".Equals(tran_type) || "L".Equals(tran_type) || "S".Equals(tran_type))
                    node.InnerText = "ITRACE_RQ_SEA";
                else
                    sb.Append(string.Format("TRAN_TYPE：{0};",tran_type));

                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "IdentifyNo");
                node.InnerText = Guid.NewGuid().ToString("N");

                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "Group");
                node.InnerText = Prolink.Math.GetValueAsString(bl["GROUP_ID"]);
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("GROUP_ID节点为空;");

                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "Company");
                node.InnerText = Prolink.Math.GetValueAsString(bl["CMP"]);
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("CMP节点为空;");

                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "Station");
                node.InnerText = Prolink.Math.GetValueAsString(bl["STN"]);
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("STN节点为空;");

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "CarrierCode");
                string master_no = Prolink.Math.GetValueAsString(bl["MASTER_NO"]);
                string scac = Prolink.Math.GetValueAsString(bl["SCAC_CD"]);
                if (string.IsNullOrEmpty(scac))
                {
                    if ("A".Equals(tran_type))
                    {
                        if (master_no != null && master_no.Length > 3)
                            scac = master_no.Substring(0, 3);
                        else
                            scac = master_no;
                    }
                    else
                    {
                        if (master_no != null && master_no.Length > 4)
                            scac = master_no.Substring(0, 4);
                        else
                            scac = master_no;
                    }
                }
                node.InnerText = scac;

                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("CarrierCode(空运MasterNo前3码,海运取Carrier_SCAC节点)节点为空;");
                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "PortCode");

                node.InnerText = Prolink.Math.GetValueAsString(bl["POL_CNTY"]) + Prolink.Math.GetValueAsString(bl["POL_CD"]);
                if (("F".Equals(tran_type) || "L".Equals(tran_type) || "S".Equals(tran_type)) && string.IsNullOrEmpty(node.InnerText))//(海運必填)PortCode 和 CarriderCode 這兩個字段不能同時為空
                    sb.Append("海運PortCode节点为空;");

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "DataNo");
                node.InnerText = master_no;
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("MASTER_NO节点为空;");

                if ("A".Equals(tran_type))
                {
                    node = GetXmlNode(xmldoc, "Itrace/MessageBody", "FlightNo");
                    node.InnerText = Prolink.Math.GetValueAsString(bl["VESSEL1"]);
                    if (string.IsNullOrEmpty(node.InnerText))
                        sb.Append("空运FlightNo1节点为空;");
                    node = GetXmlNode(xmldoc, "Itrace/MessageBody", "FlightDt");
                    node.InnerText = Prolink.Math.GetValueAsString(bl["ETD1"]);
                    if (string.IsNullOrEmpty(node.InnerText))
                        sb.Append("空运FlightDt节点为空;");
                }

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "POLCode");
                #region 截取3码
                string pol_cd = Prolink.Math.GetValueAsString(bl["POL_CD"]);
                if (!string.IsNullOrEmpty(pol_cd) && pol_cd.Length > 3)
                {
                    pol_cd = pol_cd.Trim();
                    if (pol_cd.Length >= 5)
                    {
                        pol_cd = pol_cd.Substring(pol_cd.Length - 3, 3);
                    }
                    else
                        pol_cd = pol_cd.Substring(0, 3);
                }
                node.InnerText = pol_cd;
                #endregion

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "MessageType");
                if ("A".Equals(tran_type))
                    node.InnerText = "A";
                else if ("F".Equals(tran_type) || "L".Equals(tran_type) || "S".Equals(tran_type))
                    node.InnerText = "O";
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("MessageType节点为空;");
                //发送
                if (sb.Length > 0)
                    EvenFactory.SaveLog(Serviceface.Serviceface.GetItrace().Url, xmldoc.InnerXml, sb.ToString() + "=>取消发送EDI HUB", houseNo, "IP", u_id);
                if ("A".Equals(tran_type) || "F".Equals(tran_type) || "L".Equals(tran_type) || "S".Equals(tran_type))
                {
                    //XmlParser.SaveFileLog(xmldoc.InnerXml,"ToEDIHub",master_no);
                    string result = Serviceface.Serviceface.GetItrace().Login();
                    result = Serviceface.Serviceface.GetItrace().SendToItrace(xmldoc.InnerXml);
                    EvenFactory.SaveLog(Serviceface.Serviceface.GetItrace().Url, xmldoc.InnerXml, "EDI HUB传送成功", houseNo, "IP", u_id);
                }
            }
            catch (Exception e)
            {
                sb.Append(e.Message + ";");
                EvenFactory.SaveLog(Serviceface.Serviceface.GetItrace().Url, xmldoc.InnerXml, sb.ToString() + "=>发送EDI HUB异常:" + sb.ToString(), houseNo, "IP", u_id, "N");
            }
        }

        public static string GetScac(string CarrierCd)
        {
            string sql1 = string.Format("SELECT AP_CD FROM BSCODE WHERE CD={0} AND CD_TYPE='TCAR' AND AP_CD IS NOT NULL", Prolink.Data.SQLUtils.QuotedStr(CarrierCd));
            string myscac = OperationUtils.GetValueAsString(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
            return myscac;
        }
    }
}
