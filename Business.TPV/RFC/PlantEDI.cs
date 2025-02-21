using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class PlantEDI : EDIBase
    {
        public IEnumerable<PlantInfo> GetPlantInfo(string sapId, string location , string planCode = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            if (!string.IsNullOrEmpty(planCode))
                parameters.Add("WERKS", planCode);
            IRfcFunction function = GetOperator("ZRFC_ESP_PLANT", parameters, location);
            try
            {
                return Parse(function);
            }
            catch
            {
                return null;
            }
        }

        IEnumerable<PlantInfo> Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("PLANT_TAB");
            foreach (IRfcStructure row in table)
            {
                PlantInfo info = new PlantInfo();
                info.CompanyCode = row.GetFieldValueAsString("BUKRS");
                info.PlantCode = row.GetFieldValueAsString("WERKS");
                info.Description = row.GetFieldValueAsString("NAME1");
                yield return info;
            }
        }
    }

    class PlantInfo
    {
        public string CompanyCode { get; set; }
        public string PlantCode { get; set; }
        public string Description { get; set; }
    }
}