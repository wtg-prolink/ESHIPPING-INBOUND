using Business.Service;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Business.TPV.RFC
{
    public class ASNDateEDI : EDIBase
    {
        public ResultInfo TryPostASNDateInfo(ASNDateInfo info, string location)
        {
            if (info == null) return ResultInfo.NullDataResult();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("IV_VBELN", info.AsnNo);
            parameters.Add("IV_LFDAT", info.ASN_Date);
            IRfcFunction function = GetOperatorForPost("ZRFC_ESP_ASN_UPDATE", parameters, location);
            function.Invoke(Destination);
            return ParseEVResult(function);
        }
    }

    public class ASNDateInfo
    {
        public string GroupId { get; set; }
        public string CMP { get; set; }
        public string InvNo { get; set; }
        public string ShipmentId { get; set; }
        public string AsnNo { get; set; }
        public DateTime? ASN_Date { get; set; } 
        public DateTime? OriginalAsnDate { get; set; }
        public int WorkDate { get; set; }
        public string CntrNo { get; set; }
        public string Bu { get; set; }
        public string TranType { get; set; }
    }
}
