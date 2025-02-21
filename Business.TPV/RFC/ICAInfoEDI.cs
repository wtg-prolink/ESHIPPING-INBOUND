using Business.Service;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class ICAInfoEDI : EDIBase
    {
        public ResultInfo TryPostICAInfo(IEnumerable<ICAInfo> infos, string location,string Ibcompany)
        {
            IRfcTable table = null;
            List<ICAInfo> items = infos.ToList();
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IRfcFunction function = GetOperatorForPost("ZRFC_ESP_ICA_SHIPMENT", parameters, location);
            if (!string.IsNullOrEmpty(Ibcompany) && "BR".Equals(Ibcompany))
            {
                function = GetOperatorForPost("ZRFC_ESP_BZ_ICA_SHIPMENT", parameters, location);
            }
            if (!string.IsNullOrEmpty(Ibcompany) && "MN".Equals(Ibcompany))
            {
                function = GetOperatorForPost("ZRFC_ESP_DN_SHIPMENT_AOCMMD", parameters, location);
            }
            if ("MX".Equals(Ibcompany))
            {
                function = GetOperatorForPost("ZRFC_ESP_ICA_SHIPMENT_MX", parameters, location);
            }
            if ("THI".Equals(Ibcompany))
            {
                function = GetOperatorForPost("ZRFC_ESP_ICA_SHIPMENT_TH", parameters, location);
            }
            if ("AR".Equals(Ibcompany))
            {
                function = GetOperatorForPost("ZRFC_ESP_AR_ICA_SHIPMENT", parameters, location);
            }

            table = CreatePostDT(function, infos, Ibcompany);
            function.Invoke(Destination);
            return ParseResult(function);
        }

        IRfcTable CreatePostDT(IRfcFunction function, IEnumerable<ICAInfo> infos,string Ibcompany)
        {
            Func<DateTime?, string> getTimeStr = date =>
            {
                if (date.HasValue)
                    return date.Value.ToString("HHmmss");
                return null;
            };
            Func<DateTime?, string> getDateStr = date =>
            {
                if (date.HasValue)
                    return date.Value.ToString("yyyyMMdd");
                return null;
            };
            IRfcTable table = null;
            if (!string.IsNullOrEmpty(Ibcompany) && "MN".Equals(Ibcompany))
            {
                table = function.GetTable("IT_SHIPMENT");
            }
            else
            {
                table = function.GetTable("IMT_ICA_SHIPMENT");
            }
            table.Clear();
            foreach (var item in infos)
            {
                if (!string.IsNullOrEmpty(Ibcompany) && "BR".Equals(Ibcompany))
                {
                    RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_BZ_ICA_SHIPMENT");
                    IRfcStructure row = cargoMetadata.CreateStructure();
                    row.SetValue("DN_E", item.DNNO);
                    row.SetValue("ETA", item.ETA);
                    row.SetValue("ETD", item.ETD);
                    row.SetValue("CONT_NO", item.ContainerNo);
                    if (!string.IsNullOrEmpty(item.FreightAmt))
                    {
                        row.SetValue("FREIGHT", item.FreightAmt);
                    }
                    row.SetValue("PACKINFO", item.PackingUnitInfo);
                    row.SetValue("FLAG", item.SendToInbound);
                    table.Insert(row);
                }
                else if (!string.IsNullOrEmpty(Ibcompany) && "MN".Equals(Ibcompany))
                {
                    RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZSDS0018");
                    IRfcStructure row = cargoMetadata.CreateStructure();
                    row.SetValue("ESPDN", item.DNNO);
                    row.SetValue("ETD", item.ETD);
                    row.SetValue("ATD", item.ATD);
                    row.SetValue("ETA", item.ETA);
                    row.SetValue("CONTNO", item.ContainerNo);
                    row.SetValue("BLNO", item.MasterBl);
                    //row.SetValue("POL_CODE", item.POL);
                    row.SetValue("POLNM", item.POL_DESCP);
                    //row.SetValue("POD_CODE", item.POD);
                    row.SetValue("PODNM", item.POD_DESCP);
                    //row.SetValue("CCOUNTRY", item.Cntry);
                    //row.SetValue("LDAT", getDateStr(item.OutDate));
                    //row.SetValue("LDUHR", getTimeStr(item.OutDate));
                    row.SetValue("STINB", item.SendToInbound);
                    table.Insert(row);
                }
                else if ("MX".Equals(Ibcompany))
                {
                    string[] containerNoArry = item.ContainerNo.Split(new char[] { ';', ',' });

                    RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_IMPORT_ICA_SHIPMENT_MX");
                    IRfcStructure row = cargoMetadata.CreateStructure();
                    row.SetValue("DN_E", item.DNNO);
                    row.SetValue("ATD", getDateStr(item.ATD));
                    row.SetValue("BL_NO", item.MasterBl);
                    row.SetValue("CONT1", containerNoArry.Length >= 1 ? containerNoArry[0] : null);
                    row.SetValue("CONT2", containerNoArry.Length >= 2 ? containerNoArry[1] : null);
                    row.SetValue("CONT3", containerNoArry.Length >= 3 ? containerNoArry[2] : null);
                    row.SetValue("CONT4", containerNoArry.Length >= 4 ? containerNoArry[3] : null);
                    row.SetValue("CONT5", containerNoArry.Length >= 5 ? containerNoArry[4] : null);
                    table.Insert(row);
                }
                else if ("THI".Equals(Ibcompany))
                {
                    RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_IMPORT_ICA_SHIPMENT_TH");
                    IRfcStructure row = cargoMetadata.CreateStructure();
                    row.SetValue("DN_E", item.DNNO);
                    row.SetValue("CCOUNTRY", item.Cntry);
                    row.SetValue("ETA", item.ETA);
                    row.SetValue("ETD", item.ETD);
                    row.SetValue("POD_CODE", item.POD);
                    row.SetValue("POD_DESC", item.POD_DESCP);
                    row.SetValue("POL_CODE", item.POL);
                    row.SetValue("POL_DESC", item.POL_DESCP);
                    row.SetValue("BL", item.MasterBl);
                    row.SetValue("CONT_NO", item.ContainerNo);
                    row.SetValue("LDAT", getDateStr(item.OutDate));
                    row.SetValue("LDUHR", getTimeStr(item.OutDate));
                    row.SetValue("FLAG", item.SendToInbound);
                    table.Insert(row);
                }
                else if ("AR".Equals(Ibcompany))
                {
                    RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_AR_ICA_SHIPMENT");
                    IRfcStructure row = cargoMetadata.CreateStructure();
                    row.SetValue("DN_E", item.DNNO);
                    row.SetValue("ETD", item.ETD);
                    row.SetValue("CONT_NO", item.ContainerNo);
                    if (!string.IsNullOrEmpty(item.FreightAmt))
                    {
                        row.SetValue("FREIGHT", item.FreightAmt);
                    }
                    row.SetValue("PACKINFO", item.PackingUnitInfo);
                    row.SetValue("ETA", item.ETA);
                    row.SetValue("FLAG", item.SendToInbound);
                    table.Insert(row);
                }
                else {
                    RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_IMPORT_ICA_SHIPMENT");
                    IRfcStructure row = cargoMetadata.CreateStructure();
                    row.SetValue("DN_E", item.DNNO);
                    row.SetValue("CCOUNTRY", item.Cntry);
                    row.SetValue("ETA", item.ETA);
                    row.SetValue("ETD", item.ETD);
                    row.SetValue("POD_CODE", item.POD);
                    row.SetValue("POD_DESC", item.POD_DESCP);
                    row.SetValue("POL_CODE", item.POL);
                    row.SetValue("POL_DESC", item.POL_DESCP);
                    row.SetValue("BL", item.MasterBl);
                    row.SetValue("CONT_NO", item.ContainerNo);
                    row.SetValue("LDAT", getDateStr(item.OutDate));
                    row.SetValue("LDUHR", getTimeStr(item.OutDate));
                    row.SetValue("FLAG", item.SendToInbound);
                    table.Insert(row);
                }
            }
            return table;
        }
    }

   public class ICAInfo
    {
        public string GroupId { get; set; }
        public string CMP { get; set; }
        public string STN { get; set; }
        public string DNNO { get; set; }
        public string SAP_ID { get; set; }
        public string Cntry { get; set; }
        public DateTime? ETA_MSL { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? ETD { get; set; }
        public string POD { get; set; }
        public string POD_DESCP { get; set; }
        public string POL { get; set; }
        public string POL_DESCP { get; set; }
        public string MasterBl { get; set; }
        public string ContainerNo { get; set; }
        public DateTime? OutDate { get; set; }
        public string SendToInbound { get; set; }
        public string OutSourcingToIB { get; set; }
        public string FreightAmt { get; set; }
        public string PackingUnitInfo { get; set; }
        public DateTime? ATD { get; set; }
    }
}
