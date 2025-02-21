using Business.Service;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    /// <summary>
    /// 货况回抛
    /// </summary>
    class CargoEDI : EDIBase
    {
        public ResultInfo TryPostCargoInfo(IEnumerable<CargoInfo> infos, out PostSAPData data, string location)
        {
            data = null;
            List<CargoInfo> items = infos.ToList();
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", items[0].SAP_ID);
            IRfcFunction function = GetOperatorForPost("ZRFC_ESP_DN_CARGO", parameters, location);
            //IRfcFunction function = GetOperatorForPost("ZRFC_ESP_CARGO", parameters);
            data = CreatePostDT(function, infos);
            function.Invoke(Destination);
            return ParseResult(function);
        }

        PostSAPData CreatePostDT(IRfcFunction function, IEnumerable<CargoInfo> infos)
        {
            IRfcTable table = function.GetTable("CARGO_TAB");
            IRfcTable packingTable = function.GetTable("PACKING_TAB");
            PostSAPData data = new PostSAPData { Tables = new List<IRfcTable> { table, packingTable } };
            packingTable.Clear();
            table.Clear();
            Func<DateTime?, object> getDateV = time =>
                {
                    if (time.HasValue)
                        return time.Value;
                    return null;
                };
            Func<DateTime?, string> getDateStr = date =>
                {
                    if (date.HasValue)
                        return date.Value.ToString("yyyyMMdd");
                    return null;
                };
            Func<DateTime?, string> getTimeStr = date =>
                {
                    if (date.HasValue)
                        return date.Value.ToString("HHmmss");
                    return null;
                };
            foreach (var item in infos)
            {
                RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_CARGO");
                IRfcStructure row = cargoMetadata.CreateStructure();
                row.SetValue("TDDAT", getDateStr(item.InDate));
                row.SetValue("TDUHR", getTimeStr(item.InDate));
                row.SetValue("KODAT", getDateStr(item.SealDate));
                row.SetValue("KOUHR", getTimeStr(item.SealDate));
                row.SetValue("LDDAT", getDateStr(item.OutDate));
                row.SetValue("LDUHR", getTimeStr(item.OutDate));
                row.SetValue("VBELN", item.DNNO);
                
                row.SetValue("TRA1", item.Vessel);
                row.SetValue("KUNBR", item.Broker);
                row.SetValue("KUNCR", item.Trailer);
                row.SetValue("KUNFS", item.Transport);
                row.SetValue("KUNSP", item.Forwarder);
                row.SetValue("KUNBO", item.BookingAgent);
                row.SetValue("TRA2", item.Vessel2);
                row.SetValue("TRA2N", item.VeeselUp2);
                row.SetValue("BOLNR", item.BillNO);
                row.SetValue("TRAPT2", item.LoadingPort2);
                row.SetValue("ETD2", getDateV(item.ETD2));
                row.SetValue("PODPT2", item.UnLoadingPort2);
                row.SetValue("ETA2", getDateV(item.ETA2));
                row.SetValue("ATA2", getDateV(item.ATA2));
                row.SetValue("TRA3", item.Vessel3);
                row.SetValue("TRAPT3", item.LoadingPort3);
                row.SetValue("ETD3", getDateV(item.ETD3));
                row.SetValue("PODPT3", item.UnLoadingPort3);
                row.SetValue("ETA3", getDateV(item.ETA3));
                row.SetValue("ATA3", getDateV(item.ATA3));
                row.SetValue("POL", item.POL);
                row.SetValue("ETAP", getDateV(item.ETA));
                row.SetValue("ATAP", getDateV(item.ATA));
                row.SetValue("TRTIM", item.DaysInTransit);
                row.SetValue("TRGAP", item.GapFor);
                row.SetValue("REMARK", item.Remarks);
                if(!"WH".Equals(item.CMP)){
                    row.SetValue("ATD3", getDateV(item.ATD3));
                    row.SetValue("ATD2", getDateV(item.ATD2));
                    row.SetValue("ATD1", getDateV(item.ATD));
                    if (item.ATA.HasValue || item.ATA3.HasValue || item.ATD.HasValue)
                        row.SetValue("FPOCH", "X");
                }
                table.Insert(row);                
                CreatePacking(function, item, packingTable);
            }
            return data;
        }
        void CreatePacking(IRfcFunction function, CargoInfo info, IRfcTable table)
        {
            if (info.Containers == null || info.Containers.Count <= 0) return;
            foreach (var item in info.Containers)
            {
                RfcStructureMetadata packingMetadata = Destination.Repository.GetStructureMetadata("ZESP_CARGO_PACKING");
                IRfcStructure row = packingMetadata.CreateStructure();
                row.SetValue("VBELN", item.DNNO);
                row.SetValue("SEQNR", item.SEQ);
                row.SetValue("EXIDV2", item.ContainerNO);
                row.SetValue("INHALT", item.SealNO);
                row.SetValue("VEGR1", item.SizeCode);
                row.SetValue("VEGR2", item.SerialNO);
                table.Insert(row);
            }
        }
    }

    

    class CargoInfo
    {
        public string SMID { get; set; }
        public DateTime? InDate { get; set; }
        public DateTime? SealDate { get; set; }
        public DateTime? OutDate { get; set; }
        public string SAP_ID { get; set; }
        public string DNNO { get; set; }
        public DateTime? ATD { get; set; }
        public string Vessel { get; set; }
        public string Broker { get; set; }
        public string Trailer { get; set; }
        public string Transport { get; set; }
        public string Forwarder { get; set; }
        public string BookingAgent { get; set; }
        public string Vessel2 { get; set; }
        public string VeeselUp2 { get; set; }
        public string BillNO { get; set; }
        public string LoadingPort2 { get; set; }
        public string UnLoadingPort2 { get; set; }
        public DateTime? ETA2 { get; set; }
        public DateTime? ATA2 { get; set; }
        public DateTime? ETD2 { get; set; }
        public DateTime? ATD2 { get; set; }
        public string Vessel3 { get; set; }
        public string LoadingPort3 { get; set; }
        public string UnLoadingPort3 { get; set; }
        public DateTime? ETA3 { get; set; }
        public DateTime? ATA3 { get; set; }
        public DateTime? ETD3 { get; set; }
        public DateTime? ATD3 { get; set; }
        public string POL { get; set; }
        public string POD { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? ATA { get; set; }
        public int DaysInTransit { get; set; }
        public int GapFor { get; set; }
        public string Remarks { get; set; }
        public string CMP { get; set; }
        public string STN { get; set; }
        public string GroupId { get; set; }

        public List<CNTRInfo> Containers { get; set; }
    }

    class CNTRInfo
    {
        public string DNNO { get; set; }
        public string ContainerNO { get; set; }
        public string SealNO { get; set; }
        public string SizeCode { get; set; }
        public string SEQ { get; set; }
        public string SerialNO { get; set; }
        public DateTime? InDate { get; set; }
        public DateTime? SealDate { get; set; }
        public DateTime? OutDate { get; set; }
    }
}

