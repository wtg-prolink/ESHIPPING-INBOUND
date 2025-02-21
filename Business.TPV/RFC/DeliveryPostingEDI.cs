using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    /// <summary>
    /// 发货过账EDI
    /// </summary>
    class DeliveryPostingEDI : EDIBase
    {
        public bool TryPostDeliveryPostingInfo(string sapId, Business.TPV.Import.DeliveryPostingInfo info, out DPResultInfo result, string location)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            parameters.Add("VBELN", info.DNNO);
            parameters.Add("WADAT", info.GoodsMovementDate.Value);
            IRfcFunction function = GetOperator("ZRFC_ESP_DN_PGI", parameters, location);
            result = Parse(function);
            switch (result.MsgType)
            {
                case "S": return true;
                default: return false;
            }
        }

        DPResultInfo Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("RETURN");
            DPResultInfo info = new DPResultInfo();
            foreach (var row in table)
            {
                info.MsgType = row.GetFieldValueAsString("TYPE");
                info.MsgKey = row.GetFieldValueAsString("ID");
                info.MsgNumber = row.GetFieldValueAsString("NUMBER");
                info.MsgText = row.GetFieldValueAsString("MESSAGE");
                info.ApplocationLog = row.GetFieldValueAsString("LOG_NO");
                info.InternalMsgSerialNO = row.GetFieldValueAsString("LOG_MSG_NO");
                info.MsgVariable1 = row.GetFieldValueAsString("MESSAGE_V1");
                info.MsgVariable2 = row.GetFieldValueAsString("MESSAGE_V2");
                info.MsgVariable3 = row.GetFieldValueAsString("MESSAGE_V3");
                info.MsgVariable4 = row.GetFieldValueAsString("MESSAGE_V4");
                info.ParameterName = row.GetFieldValueAsString("PARAMETER");
                info.Lines = row.GetFieldValueAsString("ROW");
                info.Field = row.GetFieldValueAsString("FIELD");
                info.LogicalSystem = row.GetFieldValueAsString("SYSTEM");
            }
            return info;
        }
    }

    public class DPResultInfo
    {
        public string MsgType { get; set; }
        public string MsgKey { get; set; }
        public string MsgNumber { get; set; }
        public string MsgText { get; set; }
        public string ApplocationLog { get; set; }
        public string InternalMsgSerialNO { get; set; }
        public string MsgVariable1 { get; set; }
        public string MsgVariable2 { get; set; }
        public string MsgVariable3 { get; set; }
        public string MsgVariable4 { get; set; }
        public string ParameterName { get; set; }
        public string Lines { get; set; }
        public string Field { get; set; }
        public string LogicalSystem { get; set; }
    }
}















