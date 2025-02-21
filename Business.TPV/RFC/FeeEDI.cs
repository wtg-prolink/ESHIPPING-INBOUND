using Business.Service;
using Business.TPV.Export;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class FeeEDI : EDIBase
    {
        public ResultInfo TryPostFeeInfo(string sapId, IEnumerable<FeeInfo> infos, string location)
        {
            IRfcTable table = null;
            List<FeeInfo> items = infos.ToList();
            if (items == null || items.Count <= 0) return ResultInfo.NullDataResult();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            IRfcFunction function = GetOperatorForPost("ZRFC_ESP_DN_FEE", parameters, location);
            table = CreatePostDT(function, infos);
            function.Invoke(Destination);
            return ParseResult(function);
        }

        IRfcTable CreatePostDT(IRfcFunction function, IEnumerable<FeeInfo> infos)
        {
            IRfcTable table = function.GetTable("FEE_TAB");
            table.Clear();
            foreach (var item in infos)
            {
                RfcStructureMetadata cargoMetadata = Destination.Repository.GetStructureMetadata("ZESP_FEE");
                IRfcStructure row = cargoMetadata.CreateStructure();
                row.SetValue("VBELN", item.DNNO);
                row.SetValue("FRAMT", item.FreightFee);
                row.SetValue("INSAT", item.InsuranceFee);
                row.SetValue("FOB", item.FOBFee);
                table.Insert(row);
            }
            return table;
        }
    }
}