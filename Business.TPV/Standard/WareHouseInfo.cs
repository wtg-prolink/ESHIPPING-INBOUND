using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    [XmlRoot("Shipment")]
    public class WareHouseInfo
    {
        [XmlElement]
        public string ShipmentId { get; set; }

        [XmlArray(ElementName = "DeliveryNoteList")]
        public List<DeliveryNoteMathed> DeliveryNoteList { get; set; }

        [XmlArray(ElementName = "ContainersList")]
        public List<ContainerInfo> ContainersList { get; set; }
    }

    public class DeliveryNoteMathed
    {
        [XmlElement]
        public string DeliveryNote1 { get; set; }
        [XmlElement]
        public string DeliveryNote2 { get; set; }
        [XmlElement]
        public string DeliveryNote3 { get; set; }

    }

    [XmlType("Container")]
    public class ContainerInfo
    {
        [XmlElement]
        public string ContainerNo { get; set; }
        [XmlElement]
        public string Status { get; set; }
        [XmlElement]
        public string CallTruckStatus { get; set; }

        [XmlElement]
        public string MasterBL { get; set; }
        [XmlElement]
        public string CntrType { get; set; }
        [XmlElement]
        public string HouseDL { get; set; }
        [XmlElement]
        public string TransMode { get; set; }
        [XmlElement]
        public string ProductType { get; set; }
        [XmlElement]
        public string TruckerNm1 { get; set; }
        [XmlElement]
        public string Carrier { get; set; }
        [XmlElement]
        public string DestName { get; set; }
        [XmlElement]
        public string PkgInfo { get; set; }
        [XmlElement]
        public string GrossWeight { get; set; }
        [XmlElement]
        public string WeightUnit { get; set; } 
        [XmlElement]
        public string ArrivalFactDateL { get; set; }
        [XmlElement]
        public string FiCustomerGroup { get; set; } 
        [XmlElement]
        public string Warehouse { get; set; }
        [XmlElement]
        public string ConsigneeName { get; set; }
        [XmlElement]
        public string DestCountry { get; set; }
        [XmlElement]
        public string ExternalModelInfo { get; set; }
        [XmlElement]
        public string ModelName { get; set; }
        [XmlElement]
        public string DeliveryDate { get; set; }
        [XmlElement]
        public string ETD { get; set; }
        [XmlElement]
        public string ATD { get; set; }
        [XmlElement]
        public string ETA { get; set; }
        [XmlElement]
        public string ATA { get; set; }
        [XmlElement]
        public string PickUpDateTime { get; set; }
        [XmlElement]
        public string PalletQty { get; set; }
        [XmlArray(ElementName = "PalletsList")]
        public List<Pallet> PalletsList { get; set; }
    }

    public class Pallet
    {
        [XmlElement]
        public string PalletNo { get; set; }
        [XmlElement]
        public string BoxQty { get; set; }
        [XmlArray(ElementName = "BoxesList")]
        public List<Box> BoxesList { get; set; }
    }

    public class Box
    {
        [XmlElement]
        public string BoxNo { get; set; }
        [XmlArray(ElementName = "MaterialsInBox")]
        public List<Material> MaterialsInBox { get; set; }
    }

    public class Material
    {
        [XmlElement]
        public string PartNumber { get; set; }
        [XmlElement]
        public string Qty { get; set; }
        [XmlElement]
        public string Unit { get; set; }
    }
}
