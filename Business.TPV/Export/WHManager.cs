using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Business.TPV.Standard;
using Business.Utils;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Export
{
    public class WHManager : ManagerBase
    {
        private string firstDn;
        private string shipmentId;
        private DataTable smsmiDt;
        private DataTable smirvDt;
        private string secondDn;
        private string thirdDn;
        public WHManager(string dnno)
        {
            firstDn = dnno;
        }

        private void SearchDataByFirstDN()
        {
            //--RefNo 二段DN
            string sql = string.Format("SELECT REF_NO FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(firstDn));
            secondDn = DB.GetValueAsString(sql); 
            CreateDataTable(firstDn);
        }

        private void SearchDataByDN2()
        {
            secondDn = firstDn;
            string sql = string.Format("SELECT TOP 1 REF_NO,DN_NO_CMP_REF FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(secondDn));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count > 0)
            { 
                firstDn = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO_CMP_REF"]);
                thirdDn = Prolink.Math.GetValueAsString(dt.Rows[0]["REF_NO"]);
            }
            string dnNo = !string.IsNullOrEmpty(thirdDn) ? thirdDn : secondDn;
            CreateDataTable(dnNo);    
        }
        private void SearchDataByDN3()
        {
            thirdDn = firstDn;  
            //通过出口找出2,1段DN
            string sql = string.Format("SELECT TOP 1 DN_NO,DN_NO_CMP_REF FROM SMDN WHERE REF_NO={0}", SQLUtils.QuotedStr(thirdDn));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count > 0)
            {
                secondDn = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
                firstDn = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO_CMP_REF"]);
            }       
            CreateDataTable(thirdDn);
        }
        private void CreateDataTable(string dnNo)
        {
            string  sql = string.Format("SELECT * FROM SMIDN WHERE DN_NO={0}", SQLUtils.QuotedStr(dnNo));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count > 0)
            {
                shipmentId = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                string smsmisql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                smsmiDt = DB.GetDataTable(smsmisql, new string[] { });

                string smirvsql = string.Format("SELECT WS_CD,RESERVE_DATE,ARRIVAL_FACT_DATE_L FROM SMIRV WHERE SHIPMENT_INFO={0} AND STATUS!='V'", SQLUtils.QuotedStr(shipmentId));
                smirvDt = DB.GetDataTable(smirvsql, new string[] { });
            }
        }    
        public string GetWareHouseInfoByDn1()
        {
            try
            {
                SearchDataByFirstDN();
                return CreateWareHouseInfo();
            }
            catch (Exception ex)
            {
                return "System Error!";
            }
        }

        public string GetWareHouseInfoByDn2()
        {
            try
            {
                SearchDataByDN2();
                return CreateWareHouseInfo();
            }
            catch (Exception ex)
            {
                return "System Error!";
            }
        }
        public string GetWareHouseInfoByDn3()
        {
            try
            {
                SearchDataByDN3();
                return CreateWareHouseInfo();
            }
            catch (Exception ex)
            {
                return "System Error!";
            }
        }

        private string CreateWareHouseInfo()
        {
            if (string.IsNullOrEmpty(shipmentId))
            {
                return "No data";
            }
            WareHouseInfo warehouseinfo = new WareHouseInfo();
            warehouseinfo.ShipmentId = shipmentId;

            DeliveryNoteMathed deliveryNoteMathed = new DeliveryNoteMathed();
            deliveryNoteMathed.DeliveryNote1 = firstDn;
            deliveryNoteMathed.DeliveryNote2 = secondDn;
            if(!string.IsNullOrEmpty(thirdDn))
                deliveryNoteMathed.DeliveryNote3 = thirdDn;
            List<DeliveryNoteMathed> deliveryNoteMathedlist = new List<DeliveryNoteMathed>();
            deliveryNoteMathedlist.Add(deliveryNoteMathed);
            warehouseinfo.DeliveryNoteList = deliveryNoteMathedlist;



            string smicntsql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} AND DN_NO LIKE '%{1}%'", SQLUtils.QuotedStr(shipmentId), firstDn);        
            DataTable smicntrDt = DB.GetDataTable(smicntsql, new string[] { });
            DataRow smsmidr = smsmiDt.Rows[0];
            DataRow smirvdr = null;
            if (smirvDt.Rows.Count > 0) 
                smirvdr = smirvDt.Rows[0];

            List<ContainerInfo> containerInfolist = new List<ContainerInfo>();
            foreach (DataRow dr in smicntrDt.Rows)
            {
                string cntrno = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                ContainerInfo containerInfo = new ContainerInfo();
                containerInfo.ContainerNo = cntrno;
                containerInfo.Status = GetBookingStatus(Prolink.Math.GetValueAsString(smsmidr["STATUS"]));
                string smordsql = string.Format("SELECT CSTATUS FROM SMORD WHERE CNTR_NO={0} AND SHIPMENT_ID={1}", SQLUtils.QuotedStr(cntrno), SQLUtils.QuotedStr(shipmentId));
                string calltruckstatus = DB.GetValueAsString(smordsql);
                containerInfo.CallTruckStatus = GetWareHouseStatus(calltruckstatus);

                containerInfo.MasterBL= Prolink.Math.GetValueAsString(smsmidr["MASTER_NO"]);
                containerInfo.CntrType = Prolink.Math.GetValueAsString(dr["CNTR_TYPE"]);
                containerInfo.HouseDL = Prolink.Math.GetValueAsString(smsmidr["HOUSE_NO"]);
                containerInfo.TransMode= Prolink.Math.GetValueAsString(smsmidr["TRAN_TYPE"]);
                containerInfo.ProductType = Prolink.Math.GetValueAsString(dr["DIVISION_DESCP"]);
                containerInfo.TruckerNm1= Prolink.Math.GetValueAsString(smsmidr["TRUCKER_NM1"]);
                containerInfo.Carrier = Prolink.Math.GetValueAsString(smsmidr["CARRIER"]);
                containerInfo.DestName = Prolink.Math.GetValueAsString(smsmidr["DEST_NAME"]);
                containerInfo.PkgInfo = Prolink.Math.GetValueAsString(smsmidr["PKG_UNIT_DESC"]);
                containerInfo.GrossWeight = Prolink.Math.GetValueAsString(smsmidr["GW"]);
                containerInfo.WeightUnit = Prolink.Math.GetValueAsString(smsmidr["GWU"]);
                containerInfo.FiCustomerGroup = Prolink.Math.GetValueAsString(smsmidr["FC_NM"]);
                containerInfo.ConsigneeName = Prolink.Math.GetValueAsString(smsmidr["CS_NM"]);
                containerInfo.DestCountry = Prolink.Math.GetValueAsString(smsmidr["DEST_CNTY"]);
                containerInfo.ExternalModelInfo = Prolink.Math.GetValueAsString(smsmidr["EXRERNAL_INFO"]);
                containerInfo.ModelName = Prolink.Math.GetValueAsString(smsmidr["PARTNO_INFO"]);
                if (smirvdr != null)
                {
                    containerInfo.Warehouse = Prolink.Math.GetValueAsString(smirvdr["WS_CD"]); 
                    containerInfo.DeliveryDate = Prolink.Math.GetValueAsDateTime(smirvdr["RESERVE_DATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                    containerInfo.ArrivalFactDateL = Prolink.Math.GetValueAsDateTime(smirvdr["ARRIVAL_FACT_DATE_L"]).ToString("yyyy-MM-dd HH:mm:ss");
                } 

                containerInfo.ETD = Prolink.Math.GetValueAsDateTime(smsmidr["ETD"]).ToString("yyyy-MM-dd HH:mm:ss");
                containerInfo.ATD = Prolink.Math.GetValueAsDateTime(smsmidr["ATD"]).ToString("yyyy-MM-dd HH:mm:ss");
                containerInfo.ETA = Prolink.Math.GetValueAsDateTime(smsmidr["ETA"]).ToString("yyyy-MM-dd HH:mm:ss");
                containerInfo.ATA = Prolink.Math.GetValueAsDateTime(smsmidr["ATA"]).ToString("yyyy-MM-dd HH:mm:ss");
                containerInfo.PickUpDateTime = Prolink.Math.GetValueAsDateTime(dr["PICKUP_CDATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                

                List<Pallet> palletlist = new List<Pallet>();
                string sminpSql = string.Format("SELECT * FROM SMINP WHERE DN_NO={0} AND CNTR_NO={1} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(firstDn), SQLUtils.QuotedStr(cntrno));
                DataTable sminpDt = DB.GetDataTable(sminpSql, new string[] { });
                Dictionary<string, List<Box>> palletDic = new Dictionary<string, List<Box>>();
                Dictionary<string, List<string>> pBoxDic = new Dictionary<string, List<string>>();
                foreach (DataRow sminpdr in sminpDt.Rows)
                {
                    string plaNo = Prolink.Math.GetValueAsString(sminpdr["PLA_NO"]);
                    string caseNo = Prolink.Math.GetValueAsString(sminpdr["CASE_NO"]);
                    List<Box> boxlist = new List<Box>();
                    List<string> caseNolist = new List<string>();
                    if (!palletDic.ContainsKey(plaNo))
                    {
                        palletDic.Add(plaNo, boxlist);
                        pBoxDic.Add(plaNo, caseNolist);
                    }
                    caseNolist = pBoxDic[plaNo];
                    if (!caseNolist.Contains(caseNo))
                        caseNolist.Add(caseNo);
                    boxlist = palletDic[plaNo];
                    Box box = new Box();
                    box.BoxNo = caseNo;
                    List<Material> mteriallist = new List<Material>();
                    Material material = new Material();
                    material.PartNumber= Prolink.Math.GetValueAsString(sminpdr["IPART_NO"]);
                    material.Qty = Prolink.Math.GetValueAsString(sminpdr["TTL_QTY"]);
                    material.Unit = Prolink.Math.GetValueAsString(sminpdr["QTYU"]);
                    mteriallist.Add(material);
                    box.MaterialsInBox = mteriallist;
                    boxlist.Add(box);
                }
                containerInfo.PalletQty = Prolink.Math.GetValueAsString(palletDic.Keys.Count());
                foreach (string key in palletDic.Keys)
                {
                    List<Box> boxlist = palletDic[key];
                    Pallet pallet = new Pallet();
                    pallet.PalletNo = key;
                    pallet.BoxQty = Prolink.Math.GetValueAsString(pBoxDic[key].Count());
                    pallet.BoxesList = boxlist;
                    palletlist.Add(pallet);
                }
                containerInfo.PalletsList = palletlist;
                containerInfolist.Add(containerInfo);
            }
            warehouseinfo.ContainersList = containerInfolist;

            string xml = XmlUtil.SerializerUTF8<WareHouseInfo>(warehouseinfo);

            return xml;
        }


        private string GetBookingStatus(string status)
        {
            switch (status)
            {
                case "S":
                    return "ISF Sending";
                case "A":
                    return "Unreach";
                case "B":
                    return "Notify LSP";
                case "C":
                    return "Notify Broker";
                case "D":
                    return "Broker Confirm";
                case "E":
                    return "E - Alert";
                case "F":
                    return "Release";
                case "G":
                    return "Gate In";
                case "H":
                    return "Notify Transit Broker";
                case "I":
                    return "Transit Confirm";
                case "P":
                    return "POD";
                case "X":
                    return "Cancel";
                case "O":
                    return "Gate Out";
                case "Z":
                    return "Finish";
                case "V":
                    return "Void";
                case "R":
                    return "Archived";
            }
            return status;
        }

        private string GetWareHouseStatus(string status)
        {
            switch (status) {
                case "A": return "Arrival";
                case "T": return "On the way";
                case "Y": return "Call Truck";
                case "D": return "Called Truck";
                case "R": return "SlotTime Booked";
                case "C": return "Confirmed";
                case "I": return "Gate In";
                case "G": return "Berthing in the Dock";
                case "V": return "Cancel";
                case "X": return "Cancel";
                case "O": return "Gate Out";
                case "Z": return "Finished";
                case "U": return "POD / Unloading";
            }
            return status;
        }
    }
}
