using Business.Import;
using Business.TPV.Utils;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Import
{
    public class ASNManager
    {

        public static bool SetAsnDetail(XmlDocument doc, EdiInfo ediInfo, bool isDirect, string fInvNo)
        {
            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("ns1", "http://www.unece.org/cefact/namespaces/StandardBusinessDocumentHeader");
            mgr.AddNamespace("ns2", "http://www.w3.org/2001/XMLSchema-instance");
            mgr.AddNamespace("ns3", "urn:ean.ucc:2");
            mgr.AddNamespace("ns4", "urn:ean.ucc:align:2");
            mgr.AddNamespace("ns5", "urn:ean.ucc:deliver:2");
            mgr.AddNamespace("prefix0", "http://xml.tradeplace.com/schemas/TradeXML/1.0.0/TradeXML.dtd");
            XmlNodeList ns3List = doc.SelectNodes("ns1:StandardBusinessDocument/ns3:message/ns3:transaction/command/ns3:documentCommand", mgr);
            string asnNo = "";
            MixedList ml = new MixedList();
            List<string> allShipment = new List<string>();
            ediInfo.ID = System.Guid.NewGuid().ToString();
            ediInfo.EdiId = "ASNFTP";
            ediInfo.Remark = "";
            ediInfo.Status = "Succeed";
            DateTime dateTime = new DateTime(2000, 1, 1);

            foreach (XmlNode ns3 in ns3List)
            {
                if (ns3 != null && ns3.ChildNodes.Count > 0)
                {
                    foreach (XmlNode node in ns3.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case "documentCommandHeader":
                                foreach (XmlNode hNode1 in node.ChildNodes)
                                {
                                    if ("entityIdentification".Equals(hNode1.Name))
                                    {
                                        foreach (XmlNode asnNode in hNode1.ChildNodes)
                                        {
                                            if ("uniqueCreatorIdentification".Equals(asnNode.Name))
                                                asnNo = asnNode.InnerText;
                                        }
                                    }
                                }
                                break;
                            case "documentCommandOperand":
                                foreach (XmlNode oNode in node.ChildNodes)
                                {
                                    if ("ns5:despatchAdvice".Equals(oNode.Name))
                                    {
                                        foreach (XmlNode oNodeC in oNode.ChildNodes)
                                        {
                                            switch (oNodeC.Name)
                                            {
                                                case "despatchInformation":
                                                    foreach (XmlNode dNode in oNodeC.ChildNodes)
                                                    {
                                                        if ("estimatedDelivery".Equals(dNode.Name))
                                                        {
                                                            foreach (XmlNode dateNode in dNode.ChildNodes)
                                                            {
                                                                if ("estimatedDeliveryDateTime".Equals(dateNode.Name))
                                                                {
                                                                    DateTime.TryParse(dateNode.InnerText, out dateTime);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            ediInfo.RefNO = asnNo;
            List<string> shipmentList = ASNManager.SetAsnMap(isDirect, asnNo, fInvNo, dateTime,ml);
            if (shipmentList.Count<=0)
            {
                ediInfo.Status = "Exception";
                ediInfo.Remark += string.Format("InvNo:{0} cann't find ShipmentDetail", fInvNo);
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach(string shipmentid in shipmentList)
                        TrackingEDI.InboundBusiness.ASNManager.SetAsnByShipmentid(shipmentid);
                }
                catch (Exception ex)
                {
                    ediInfo.Remark = ex.ToString();
                    ediInfo.Status = "Exception";
                    return false;
                }
            }
            if ("Succeed".Contains(ediInfo.Status))
                ediInfo.Remark = "Successfully";        
            return true;
        }


        public static bool SetGrDetail(XmlDocument doc, EdiInfo ediInfo)
        {
            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("sh", "http://www.unece.org/cefact/namespaces/StandardBusinessDocumentHeader");
            mgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            mgr.AddNamespace("ns", "urn:ean.ucc:2");
            mgr.AddNamespace("eanucc", "urn:ean.ucc:2");
            mgr.AddNamespace("ns0", "urn:ean.ucc:deliver:2");
            XmlNode ns0 = doc.SelectSingleNode("sh:StandardBusinessDocument/ns:message/ns:transaction/command/ns:documentCommand/documentCommandOperand/ns0:receivingAdvice", mgr);
            string asnNo = "", grDate = "", grPartNo = "";
            Dictionary<string, int> grQtyReceivedDic = new Dictionary<string, int>();
            int grQty = 0;
            if (ns0 != null && ns0.ChildNodes.Count > 0)
            {
                foreach (XmlNode node in ns0.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "despatchAdvice":
                            foreach (XmlNode asnNode in node.ChildNodes)
                            {
                                if ("uniqueCreatorIdentification".Equals(asnNode.Name))
                                    asnNo = asnNode.InnerText;
                            }
                            break;
                        case "receiptInformation":
                            foreach (XmlNode gNode in node.ChildNodes)
                            {
                                if ("receivingDateTime".Equals(gNode.Name))
                                    grDate = gNode.InnerText;
                            }
                            break;
                        case "receivingAdviceItemContainmentLineItem":
                            XmlNode qNode = null, qgNode = null, qgNodevalue = null, received = null, receivedvalue = null;
                            if (node != null)
                            {
                                qNode = node.SelectSingleNode("containedItemIdentification");
                                received = node.SelectSingleNode("quantityReceived");
                            }
                            if (qNode != null)
                                qgNode = qNode.SelectSingleNode("additionalTradeItemIdentification");
                            if (qgNode != null)
                            {
                                qgNodevalue = qgNode.SelectSingleNode("additionalTradeItemIdentificationValue");
                                grPartNo = qgNodevalue.InnerText;
                            }
                            if (received != null)
                            {
                                receivedvalue = received.SelectSingleNode("value");
                                grQty = Prolink.Math.GetValueAsInt(receivedvalue.InnerText);
                            }
                            if (!string.IsNullOrEmpty(grPartNo) && !grQtyReceivedDic.ContainsKey(grPartNo))
                            {
                                grQtyReceivedDic.Add(grPartNo, grQty);
                            }
                            break;
                    }
                }
            }
            if (string.IsNullOrEmpty(asnNo))
                return true;

            ediInfo.ID = System.Guid.NewGuid().ToString();
            ediInfo.EdiId = "GRFTP";
            ediInfo.RefNO = asnNo;
            //ediInfo.GroupId = "TPV";
            ediInfo.Remark = "Successfully";
            ediInfo.Status = "Succeed";
            DateTime dateTime = new DateTime(2000, 1, 1);
            DateTime.TryParse(grDate, out dateTime);
            MixedList smidnpMl = new MixedList();
            string sql = string.Format("SELECT * FROM GR_MAP WHERE ASN_NO={0}", asnNo);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (var key in grQtyReceivedDic.Keys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;
                grQty = grQtyReceivedDic[key];
                DataRow[] drs = dt.Select(string.Format("MODEL_NO={0}", SQLUtils.QuotedStr(key)));
                EditInstruct ei = new EditInstruct("GR_MAP", EditInstruct.INSERT_OPERATION);
                ei.Put("ASN_NO", asnNo);
                ei.Put("MODEL_NO", key);
                ei.Put("GROUP_ID", "TPV");
                if (drs.Length > 0)
                {
                    ei = new EditInstruct("GR_MAP", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("ASN_NO", asnNo);
                    ei.PutKey("MODEL_NO", key);
                    ei.PutDate("MODIFY_DATE", DateTime.Now);
                }

                if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                    ei.PutDate("CREATE_DATE", DateTime.Now);
                ei.Put("GR_QTY", grQty);
                if (dateTime.CompareTo(new DateTime(2000, 1, 1)) > 0 || grQty > 0)
                {
                    bool flag = TrackingEDI.InboundBusiness.ASNManager.SetGRToSMINDNP(asnNo, key, grQty, dateTime, smidnpMl);
                    TrackingEDI.InboundBusiness.ASNManager.SetDateTimeToEiGR(ei, dateTime);

                    if (!flag)
                    {
                        ediInfo.Status = "Exception";
                        ediInfo.Remark += string.Format("ASN NO:{0},Model Name:{1} cann't find DN Detail", asnNo, key);
                    }
                } 
                smidnpMl.Add(ei);
            }
            if (smidnpMl.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(smidnpMl, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception e)
                {
                    ediInfo.Status = "Exception";
                    ediInfo.Remark = e.ToString();
                    return false;
                }
                TrackingEDI.InboundBusiness.ASNManager.SetAsnByAsnNo(asnNo);
            }
            return true;
        }

        public static List<string> SetAsnMap(bool isDirect, string asnNo, string invNo, DateTime asnDate, MixedList ml)
        {
            List<string> ShipmentList = new List<string>();
            if (string.IsNullOrEmpty(asnNo)) return ShipmentList;
            string sql = string.Format("SELECT U_ID FROM ASN_MAP WHERE CNTR_NO={0}", SQLUtils.QuotedStr(invNo));
            string uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = new EditInstruct("ASN_MAP", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            if (!string.IsNullOrEmpty(uid))
            {
                ei = new EditInstruct("ASN_MAP", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            ei.Put("CNTR_NO", invNo);
            ei.Put("ASN_NO", asnNo);
            ei.Put("GROUP_ID", "TPV");
            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                ei.PutDate("CREATE_DATE", DateTime.Now);
            if (asnDate.CompareTo(new DateTime(2000, 1, 1)) > 0)
            {
                ei.PutDate("ASN_DATE", asnDate);
                if(isDirect)
                    ei.PutDate("GR_DATE", asnDate);
            }
            ml.Add(ei);

             sql = string.Format(@"SELECT U_ID,SHIPMENT_ID,INV_NO,ASN_DATE,
(SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDNP.SHIPMENT_ID) AS ETA,
(SELECT TOP 1 CMP FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDNP.SHIPMENT_ID) AS CMP,
(SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDNP.SHIPMENT_ID) AS TRAN_TYPE
            FROM SMIDNP WHERE INV_NO={0} AND (NEW_CATEGORY!='TANN' OR NEW_CATEGORY IS NULL)",
                SQLUtils.QuotedStr(invNo));
            DataTable smidnpDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in smidnpDt.Rows)
            {
                string inpUid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                if (!string.IsNullOrEmpty(shipmentId) && !ShipmentList.Contains(shipmentId))
                    ShipmentList.Add(shipmentId);
                EditInstruct smidnpei = new EditInstruct("SMIDNP", EditInstruct.UPDATE_OPERATION);
                smidnpei.PutKey("U_ID", inpUid);
                smidnpei.Put("ASN_NO", asnNo);
                if (asnDate.CompareTo(new DateTime(2000, 1, 1)) > 0)
                {
                    DateTime asnDatetemp = asnDate; 

                    if (!string.IsNullOrEmpty(shipmentId))
                    {
                        TrackingEDI.InboundBusiness.ASNManager.Datetype datetype =TrackingEDI.InboundBusiness.ASNManager.Getasndate(shipmentId, invNo, dr, ref asnDatetemp);
                        TrackingEDI.InboundBusiness.ASNManager.UpdateAsnToSmidn(datetype, ml, asnDatetemp, dr);
                    }
                    smidnpei.PutDate("ASN_DATE", asnDatetemp);
                    if (isDirect)
                    {
                        smidnpei.Put("GR_DATE", asnDate.ToString("yyyy-MM-dd"));
                        smidnpei.PutExpress("GR_QTY", "QTY");
                        smidnpei.Put("GR_STATUS", "Y");
                    }
                }

                ml.Add(smidnpei);
            }
            return ShipmentList;
        }

    }
}
