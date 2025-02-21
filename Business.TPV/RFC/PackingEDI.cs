using Business.Service;
using Business.TPV.Export;
using Business.TPV.Import;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class PackingEDI : EDIBase
    {
        public ResultInfo TryPostPackingInfo(string sapId, IEnumerable<PackingInfo> infos, string location, out ResultInfo result)
        {
            IRfcTable table = null;
            result = null;
            List<PackingInfo> items = infos.ToList();
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            //parameters.Add("SRCSYSTEM", sapId);
            IRfcFunction function = GetOperatorForPost("ZRFC_ESP_PACKING", parameters, location);
            table = CreatePostDT(function, infos);
            function.Invoke(Destination);
            ResultInfo rf = ParseResult(function);
            if (rf.IsSucceed)
            {
                PackingReceiveEDI prei = new PackingReceiveEDI();
                List<PackingReceiveInfo> prinfo = prei.Parse(function).ToList();
                PackingReceiveManager prm = new PackingReceiveManager();
                result = prm.Import(prinfo);
            }
            return rf;
        }

        IRfcTable CreatePostDT(IRfcFunction function, IEnumerable<PackingInfo> infos)
        {
            IRfcTable table = function.GetTable("IMT_PACKING");
            table.Clear();
            foreach (var item in infos)
            {
                RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_IMPORT_PACKING");
                IRfcStructure row = cargoMetadata.CreateStructure();
                row.SetValue("DN_E", item.DNNO);
                row.SetValue("Pallet", item.Pallet);
                row.SetValue("NumFrom", item.From);
                row.SetValue("NumTo", item.To);
                row.SetValue("MATNR", item.Material);
                row.SetValue("LFIMG", item.Quantity);
                row.SetValue("VRKME", item.Unit);
                row.SetValue("NTGEW", Math.Round(item.NetWeight, 3));
                //row.SetValue("UNIT_WEIGHT", Math.Round(item, 3));
                row.SetValue("BRGEW", Math.Round(item.GrossWeight, 3));
                row.SetValue("VOLUM", Math.Round(item.Volume, 3));
                row.SetValue("SINVOICE", item.SupplierInvoiceNo);
                row.SetValue("Oricountry", item.OriginalCntry);
                row.SetValue("Reclocation", item.ReceiveLocation);
                table.Insert(row);
            }
            return table;
        }
    }
}
