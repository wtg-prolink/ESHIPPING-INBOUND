using Prolink.DataOperation;
using Prolink.V6.Persistence;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class BaseCodeEDI : EDIBase<BaseCodeInfo>
    {
        public IEnumerable<BaseCodeInfo> GetBaseCode(string sapId, string codeType, string location, string code = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            parameters.Add("MASTER_TYPE", codeType);
            if (!string.IsNullOrEmpty(code))
                parameters.Add("MASTER_CODE", code);
            IRfcFunction function = GetOperator("ZRFC_ESP_MASTER_CODE", parameters, location);
            try
            {
                return Parse(function);
            }
            catch
            {
                return null;
            }
        }

        IEnumerable<BaseCodeInfo> Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("MASTER_TAB");
            foreach (IRfcStructure row in table)
            {
                BaseCodeInfo info = new BaseCodeInfo();
                info.Type = GetSaveTypeCode(row.GetFieldValueAsString("MASTER_TYPE"));
                info.Code = row.GetFieldValueAsString("MASTER_CODE");
                info.LanguageCode = row.GetFieldValueAsString("MASTER_LANG");
                info.Description = row.GetFieldValueAsString("MASTER_TEXT");
                yield return info;
            }
        }

        internal static string GetSaveTypeCode(string type)
        {
            switch (type)
            {
                case "VERP": return "TCNT";
                case "TINC": return "TD";
                default: return type;
            }
        }

        public override List<BaseCodeInfo> Distinct(IEnumerable<BaseCodeInfo> items)
        {
            List<BaseCodeInfo> list = new List<BaseCodeInfo>();
            foreach (var item in items)
            {
                var v = list.Where(p => p.Code == item.Code).FirstOrDefault();
                if (v == null)
                {
                    list.Add(item);
                    continue;
                }
                if (item.LanguageCode == "EN")
                {
                    list.Remove(v);
                    list.Add(item);
                }
            }
            return list;
        }
    }

    class BaseCodeInfo : EDIInfo
    {
        public string LanguageCode { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }
}
