using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAP.Middleware.Connector;

namespace Business.TPV.RFC
{
    class AsusEDI:EDIBase
    {
        public IEnumerable<AsusInfo> GetAsusInfo(string SoNo, string location,string dnNo)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("S_PONUM", SoNo);
            IRfcFunction function = GetOperator("ZRFC_ASUS_ESHIPPING", parameters, location);
            try
            {
                return Parse(function,dnNo);
            }
            catch
            {
                return null;
            }
        }
        IEnumerable<AsusInfo> Parse(IRfcFunction function,string dnNo)
        {
            IRfcTable table = function.GetTable("IT_ALV");
            foreach (IRfcStructure row in table)
            {
                AsusInfo info = new AsusInfo();
                info.SOURCE_NO = row.GetFieldValueAsString("ZASUS_PONUM");
                info.ITEM_NUM = row.GetFieldValueAsString("ZASUS_ITEMNUM");
                info.VERSION = row.GetFieldValueAsString("ZASUS_VERSION");
                info.PO = row.GetFieldValueAsString("ZASUS_PO");
                info.POLINE = row.GetFieldValueAsString("ZASUS_POLINE");
                info.SO = row.GetFieldValueAsString("ZASUS_SO");
                info.SOLINE = row.GetFieldValueAsString("ZASUS_SOLINE");
                info.DN = row.GetFieldValueAsString("ZASUS_DN");
                info.DNLINE = row.GetFieldValueAsString("ZASUS_DNLINE");
                info.POCUS = row.GetFieldValueAsString("ZASUS_POCUS");
                info.DEST = row.GetFieldValueAsString("ZASUS_REFDEST");
                info.DN_NO = dnNo;
                yield return info;
            }
        }
    }

    class AsusInfo
    {
        public string SOURCE_NO { get; set; }
        public string ITEM_NUM { get; set; }
        public string VERSION { get; set; }
        public string PO { get; set; }
        public string POLINE { get; set; }
        public string SO { get; set; }
        public string SOLINE { get; set; }
        public string DN { get; set; }
        public string DNLINE { get; set; }
        public string POCUS { get; set; }
        public string DEST { get; set; }
        public string DN_NO { get; set; }
    }
}
