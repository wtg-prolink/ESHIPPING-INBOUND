using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class UnloadingPortEDI : EDIBase<UnloadingPortInfo>
    {
        public IEnumerable<UnloadingPortInfo> GetUnloadingPortInfo(string sapId,string location)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            IRfcFunction function = GetOperator("ZRFC_ESP_UNLOADING", parameters, location);
            try
            {
                return Parse(function);
            }
            catch
            {
                return null;
            }
        }

        IEnumerable<UnloadingPortInfo> Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("UNLOADING_TAB");
            foreach (IRfcStructure row in table)
            {
                UnloadingPortInfo info = new UnloadingPortInfo();
                info.CountryKey = row.GetFieldValueAsString("LAND1");
                info.Code = row.GetFieldValueAsString("ZOLLA");
                info.LanguageCode = row.GetFieldValueAsString("LAISO");
                info.Description = row.GetFieldValueAsString("BEZEI");
                yield return info;
            }
        }

        public override List<UnloadingPortInfo> Distinct(IEnumerable<UnloadingPortInfo> items)
        {
            List<UnloadingPortInfo> list = new List<UnloadingPortInfo>();
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

    class UnloadingPortInfo : EDIInfo
    {
        public string CountryKey { get; set; }
        public string LanguageCode { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}