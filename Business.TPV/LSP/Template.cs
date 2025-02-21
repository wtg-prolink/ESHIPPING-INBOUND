using Business.EDI;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.LSP
{
    abstract class Template : EntityEDITemplate
    {
        #region edi
        public override XmlEDINode CreateXmlEDINode()
        {
            XmlEDINode root = new XmlEDINode("BookingInformation");
            XmlEDINode node = new XmlEDINode("MODE", GetOperationModeCode());
            root.ChildNodes.Add(node);
            node = new XmlEDINode("EDI_TYPE", GetEdiTypeCode());
            root.ChildNodes.Add(node);
            node = new XmlEDINode("SENDER", Sender);
            root.ChildNodes.Add(node);
            //node = new XmlEDINode("Recieve_Code", RecieveCode);
            node = new XmlEDINode("RECIEVE_CODE", RecieveCode);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("MSGCODE", MsgCode);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("SHIP_TYPE", ShipType);
            root.ChildNodes.Add(node);
            if (ETD.HasValue)
            {
                node = new XmlEDINode("ETD", ETD.Value.ToString("yyyyMMdd"));
                root.ChildNodes.Add(node);
            }

            node = new XmlEDINode("NOTIFY_NM1", NotifyName1);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("NOTIFY_ADD1", NotifyAddress1);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("NOTIFY_NM2", NotifyName2);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("NOTIFY_ADD2", NotifyAddress2);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("NOTIFY_NM3", NotifyName3);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("NOTIFY_ADD3", NotifyAddress3);
            root.ChildNodes.Add(node);

            node = new XmlEDINode("SHIPMENT_ID", ShipmentID);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CARRIER", Carrier);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("PO_NO", PONO);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("POR", POR);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("POR_NAME", PORName);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("POL", POL);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("POL_NAME", POL_Name);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("POD", POD);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("POD_NAME", POD_Name);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("DEST", DEST);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("DEST_NAME", DEST_Name);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("SERVICE_MODE", ServiceMode);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("TRADE_TYPE", TradeType);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("BILL_PORT_NAME", BillPortName);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("PRODUCT_NO_CN", ProductNO_CN);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("PRODUCT_NO", ProductNO);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("SHIPMARK", ShipMark);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("SHIPPER_ID", ShipperID);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("SHIPPER_NAME", ShipperName);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("SHIPPER_ADD", ShipperAddress);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CONSIGNEE_ID", CneeID);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CONSIGNEE_NAME", CneeName);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CONSIGNEE_ADD", CneeAddress);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CARGO_TYPE", CargoType);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CTN", CTN);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("GW", GW);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CBM", CBM);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("BOOKING_USER", BookingUser);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("QTY", QTY);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("PKG_MATERIAL", PackagerUnit);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("PKG_MATERIAL_DESC", PackagerUnitDesc);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("REMARK", Remark);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("GP20", GP20);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("GP40", GP40);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("HQ40", HQ40);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("OTHER_CONTAINER_TYPE", OtherCNTType);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("OTHER_CTNR_QTY", OtherCNTCount);
            root.ChildNodes.Add(node);
            SetDNInfos(root);
            return root;
        }
        protected void SetDNInfos(XmlEDINode root)
        {
            XmlEDINode mNode = new XmlEDINode("DN_INFO");
            foreach (var item in DNInfos)
            {
                XmlEDINode node = new XmlEDINode("DN_Detail");
                SetDNInfo(node, item);
                mNode.ChildNodes.Add(node);
            }
            root.ChildNodes.Add(mNode);
        }
        protected abstract void SetDNInfo(XmlEDINode dnDetail, DNInfo info);
        #endregion
        public OperationModes OperationMode { get; set; }
        [Required]
        public string Sender { get; set; }
        [Required]
        public string RecieveCode { get; set; }
        [Required]
        public string MsgCode { get; set; }
        [Required]
        public string ShipType { get; set; }
        [Required]
        public string ShipmentID { get; set; }
        [Required]
        public string TradeType { get; set; }
        public string BillPortName { get; set; }
        public abstract EdiTypes EdiType { get; }
        [Required(ErrorMessage="未找到对应的DN NO!")]
        public List<DNInfo> DNInfos { get; set; }
        public string PONO { get; set; }
        [Required]
        public DateTime? ETD { get; set; }
        public string POR { get; set; }
        public string PORName { get; set; }
        public string POL { get; set; }
        public string POL_Name { get; set; }
        public string POD { get; set; }
        public string POD_Name { get; set; }
        public string DEST { get; set; }
        public string DEST_Name { get; set; }
        [Required]
        public string ServiceMode { get; set; }
        [Required]
        public string ProductNO { get; set; }
        public string ShipMark { get; set; }
        public string ShipperID { get; set; }
        public string ShipperName { get; set; }
        public string ShipperAddress { get; set; }
        public string CneeID { get; set; }
        public string CneeName { get; set; }
        public string CneeAddress { get; set; }
        public string CargoType { get; set; }
        [Required]
        public string CTN { get; set; }
        [Required]
        public string GW { get; set; }
        [Required]
        public string CBM { get; set; }
        public string Remark { get; set; }
        public string GP20 { get; set; }
        public string GP40 { get; set; }
        public string HQ40 { get; set; }
        public string OtherCNTType { get; set; }
        public string OtherCNTCount { get; set; }        
        public string ProductNO_CN { get; set; }
        [Required]
        public string Carrier { get; set; }
        [Required]
        public string BookingUser { get; set; }
        [Required]
        public string QTY { get; set; }
        public string PackagerUnit { get; set; }
        public string PackagerUnitDesc { get; set; }

        public string NotifyName1 { get; set; }
        public string NotifyAddress1 { get; set; }
        public string NotifyName2 { get; set; }
        public string NotifyAddress2 { get; set; }
        public string NotifyName3 { get; set; }
        public string NotifyAddress3 { get; set; }

        string GetOperationModeCode()
        {
            switch (OperationMode)
            {
                case OperationModes.Add: return "A";
                case OperationModes.Cancel: return "C";
                case OperationModes.Modify: return "M";
            }
            return null;
        }

        string GetEdiTypeCode()
        {
            switch (EdiType)
            {
                case EdiTypes.Booking: return "B";
                case EdiTypes.Declaration: return "D";
            }
            return null;
        }
    }

    class DNInfo
    {
        [Required]
        public string DNNO { get; set; }
        [DeclDNRequired(ErrorMessage = "出口号码不可为空")]
        public string ExportNO { get; set; }
        public string UniCode { get; set; }
        public string PONO { get; set; }
    }

    public enum EdiTypes
    {
        /// <summary>
        /// 订舱
        /// </summary>
        Booking,
        /// <summary>
        /// 预申报
        /// </summary>
        Declaration
    }

    public enum PaymentTypes
    {
        /// <summary>
        /// 到付(C)
        /// </summary>
        Collect,
        /// <summary>
        /// 预付(P)
        /// </summary>
        Prepay,
        /// <summary>
        /// 其他地付款(A)
        /// </summary>
        Other
    }
    public enum TransTypes { FullTruck, Rail, Rail_Truck }
    public enum CargoTypes
    {
        /// <summary>
        /// 整柜
        /// </summary>
        FCL,
        /// <summary>
        /// 拼柜
        /// </summary>
        LCL,
        /// <summary>
        /// 空柜
        /// </summary>
        Empty
    }
}
