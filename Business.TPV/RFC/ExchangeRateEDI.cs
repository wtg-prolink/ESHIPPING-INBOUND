using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class ExchangeRateEDI : EDIBase
    {
        public IEnumerable<ExchangeRateInfo> GetExchangeRateInfo(string sapId, ExchangeRateTypes rateType = ExchangeRateTypes.Standard, string fromCurrency = null, string toCurrency = null,string location="")
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            string rateCode = GetReteTypeCode(rateType);
            if (!string.IsNullOrEmpty(rateCode))
            {
                parameters.Add("KURST", rateCode);
                parameters.Add("FCURR", fromCurrency);
                parameters.Add("TCURR", toCurrency);
            }
            IRfcFunction function = GetOperator("ZRFC_ESP_EXCHANGE_RATE", parameters, location);
            try
            {
                return Parse(function);
            }
            catch
            {
                return null;
            }
        }

        internal string GetReteTypeCode(ExchangeRateTypes type)
        {
            switch (type)
            {
                case ExchangeRateTypes.Standard: return "M";
                case ExchangeRateTypes.EndOfTheMonth: return "R";
            }
            return null;
        }

        IEnumerable<ExchangeRateInfo> Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("EXCHANGE_RATE_TAB");
            foreach (IRfcStructure row in table)
            {
                ExchangeRateInfo info = new ExchangeRateInfo();
                info.ExchangeRateType = row.GetFieldValueAsString("RATE_TYPE");
                info.ValidDate = row.GetFieldValueAsString("VALID_FROM");
                info.DirectQuotedExchangeRate = row.GetFieldValueAsString("EXCH_RATE");
                info.FromCurrencyUnits = row.GetFieldValueAsString("FROM_FACTOR");
                info.ToCurrencyUnits = row.GetFieldValueAsString("TO_FACTOR");
                info.FromCurrency = row.GetFieldValueAsString("FROM_CURR");
                info.IndirectQuotedExchangeRate = row.GetFieldValueAsString("EXCH_RATE_V");
                info.FromCurrencyUnits_V = row.GetFieldValueAsString("FROM_FACTOR_V");
                info.ToCurrencyUnits_V = row.GetFieldValueAsString("TO_FACTOR_V");
                info.ToCurrency = row.GetFieldValueAsString("TO_CURRNCY");
                yield return info;
            }
        }
}

    public enum ExchangeRateTypes
    {
        /// <summary>
        /// 在平均比率下的标准兑换
        /// </summary>
        Standard,
        /// <summary>
        /// 月末评估汇率
        /// </summary>
        EndOfTheMonth
    }

    class ExchangeRateInfo
    {
        public string SAPID { get; set; }

        /// <summary>
        /// Exchange Rate Type
        /// </summary>
        public string ExchangeRateType { get; set; }
        /// <summary>
        /// Date from Which Entry Is Valid
        /// </summary>
        public string ValidDate { get; set; }
        /// <summary>
        /// Direct Quoted Exchange Rate
        /// </summary>
        public string DirectQuotedExchangeRate { get; set; }
        /// <summary>
        /// Ratio for the "From" Currency Units
        /// </summary>
        public string FromCurrencyUnits { get; set; }
        /// <summary>
        /// Ratio for the "To" Currency Units
        /// </summary>
        public string ToCurrencyUnits { get; set; }
        /// <summary>
        /// From currency
        /// </summary>
        public string FromCurrency { get; set; }
        /// <summary>
        /// Indirect Quoted Exchange Rate
        /// </summary>
        public string IndirectQuotedExchangeRate { get; set; }
        /// <summary>
        /// Ratio for the "From" Currency Units
        /// </summary>
        public string FromCurrencyUnits_V { get; set; }
        /// <summary>
        /// Ratio for the "To" Currency Units
        /// </summary>
        public string ToCurrencyUnits_V { get; set; }
        /// <summary>
        /// To currency
        /// </summary>
        public string ToCurrency { get; set; }
    }
}
