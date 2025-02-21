using SAP.Middleware.Connector;
using System.Collections.Generic;

namespace Business.TPV.RFC
{
    public class PackingReceiveEDI : EDIBase
    {
        public IEnumerable<PackingReceiveInfo> GetPackingReceiveInfo(string sapId, string location, string dnno = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            //parameters.Add("SRCSYSTEM", sapId);
            IRfcFunction function = GetOperator("ZRFC_ESP_PACKING", parameters, location);
            try
            {
                return Parse(function);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<PackingReceiveInfo> Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("EXT_PACKING");
            foreach (IRfcStructure row in table)
            {
                PackingReceiveInfo info = new PackingReceiveInfo();
                info.DNNO = row.GetFieldValueAsString("DN_E");
                info.Pallet = row.GetFieldValueAsString("Pallet");
                info.From = row.GetFieldValueAsString("NumFrom");
                info.To=row.GetFieldValueAsString("NumTo");
                info.Material = row.GetFieldValueAsString("MATNR");
                //info.Quantity = Prolink.Math.GetValueAsDouble(row.GetFieldValueAsString("LFIMG"));
                //info.Unit = row.GetFieldValueAsString("VRKME");
                //info.GrossWeight = Prolink.Math.GetValueAsDouble(row.GetFieldValueAsString("BRGEW"));
                //info.Volume = Prolink.Math.GetValueAsDouble(row.GetFieldValueAsString("VOLUM"));
                //info.SupplierInvoiceNo = row.GetFieldValueAsString("SINVOICE");
                //info.OriginalCntry = row.GetFieldValueAsString("Oricountry");
                //info.ReceiveLocation = row.GetFieldValueAsString("Reclocation");
                info.NetWeight = Prolink.Math.GetValueAsDouble(row.GetFieldValueAsString("NTGEW"));
                info.UnitWeight = Prolink.Math.GetValueAsDouble(row.GetFieldValueAsString("UNIT_WEIGHT"));
                info.NcmNo = row.GetFieldValueAsString("NCM_NO");
                info.CnCode = row.GetFieldValueAsString("CN_CODE");
                info.ChinaDescp = row.GetFieldValueAsString("CN_DESC");
                info.EngDescp = row.GetFieldValueAsString("EN_DESC");
                //info.SapStatus = row.GetFieldValueAsString("XXXX");
                yield return info;
            }
        }
    }

    public class PackingReceiveInfo
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string DNNO { get; set; }
        public string Pallet { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Material { get; set; }
        //public double Quantity { get; set; }
        public double NetWeight { get; set; }
        public double UnitWeight { get; set; }
        public string NcmNo { get; set; }
        public string CnCode { get; set; }
        public string ChinaDescp { get; set; }
        public string EngDescp { get; set; }
        public string SapStatus { get; set; }
        public string CMP { get; set; }
        public string STN { get; set; }
    }
}
