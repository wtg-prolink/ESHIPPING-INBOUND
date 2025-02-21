using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    public class ExchangeRateManager : ManagerBase
    {
        public ResultInfo Import(string sapId, string location, ExchangeRateTypes rateType = ExchangeRateTypes.Standard, string fromCurrency = null, string toCurrency = null)
        {
            location = "FQ";
            ExchangeRateEDI edi = new ExchangeRateEDI();
            List<ExchangeRateInfo> infos = edi.GetExchangeRateInfo(sapId, rateType, fromCurrency, toCurrency).ToList();
            string rateCode = edi.GetReteTypeCode(rateType);
            DataTable dt = Query(rateCode, fromCurrency, toCurrency, location);
            var result = Save(infos, dt, location, rateCode);
            WriteEDILog(infos, string.Format("{0}_{1}", fromCurrency, toCurrency), result);
            return result;
        }

        void WriteEDILog(List<ExchangeRateInfo> infoList, string refNO, ResultInfo result)
        {
            Utils.EdiInfo info = null;
            ExchangeRateEDILog log = new ExchangeRateEDILog(infoList, refNO, "System");
            if (result.IsSucceed)
            {
                info = log.CreateSucceed();
            }
            else
            {
                info = log.CreateEx(result.Description);
            }
            Logger.WriteLog(info);
        }

        string GetCondition(string rateCode, string fromCurrency, string toCurrency,string location,string edate)
        {
            List<ConditionItem> conditions = new List<ConditionItem>();
            conditions.Add(new ConditionItem("FCUR", fromCurrency));
            conditions.Add(new ConditionItem("TCUR", toCurrency));
            conditions.Add(new ConditionItem("ETYPE", rateCode));
            conditions.Add(new ConditionItem("EDATE", edate));
            conditions.Add(new ConditionItem("CMP", location));
            return DBManager.CreateCondition(conditions);
        }

        string GetConditionByDt(string rateCode, string fromCurrency, string toCurrency)
        {
            List<ConditionItem> conditions = new List<ConditionItem>();
            conditions.Add(new ConditionItem("FCUR", fromCurrency));
            conditions.Add(new ConditionItem("TCUR", toCurrency));
            conditions.Add(new ConditionItem("ETYPE", rateCode));;
            return DBManager.CreateCondition(conditions);
        }

        DataTable Query(string rateCode, string fromCurrency, string toCurrency,string location)
        {
            string sql = "SELECT * FROM BSERATE";
            string condition = GetConditionByDt(rateCode, fromCurrency, toCurrency);
            if (!string.IsNullOrEmpty(condition))
                sql = string.Format("{0} WHERE {1}", sql, condition);
            return DB.GetDataTable(sql, new string[] { });
        }

        ResultInfo Save(List<ExchangeRateInfo> infos, DataTable preDT,string location,string ratecd)
        {
            Func<ExchangeRateInfo, bool> checkHas = info =>
            {
                string condition = GetCondition(info.ExchangeRateType, info.FromCurrency, info.ToCurrency,location,info.ValidDate);
                DataRow[] rows = preDT.Select(condition);
                return rows != null && rows.Length > 0;
            };
            List<string> autolist = new List<string>();
            EditInstructList eiList = new EditInstructList();
            bool upcny = false;
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}_{2}", item.FromCurrency, item.ToCurrency, GetCurrentTimeString()));
                EditInstruct ei = null;
                if (checkHas(item))
                {
                    ei = new EditInstruct("BSERATE", EditInstruct.UPDATE_OPERATION);
                    
                    ei.Condition = GetCondition(item.ExchangeRateType, item.FromCurrency, item.ToCurrency,location,item.ValidDate);
                    ei.PutDate("MODIFY_DATE", DateTime.Now);
                }
                else
                {
                    ei = new EditInstruct("BSERATE", EditInstruct.INSERT_OPERATION);
                    ei.PutDate("CREATE_DATE", DateTime.Now);                   
                }
                ei.Put("GROUP_ID", Business.TPV.Context.GroupId);
                ei.Put("ETYPE", item.ExchangeRateType);

                if (!string.IsNullOrEmpty(item.ValidDate))
                {
                    DateTime time;
                    if (DateTime.TryParse(item.ValidDate, out time))
                    {
                        ei.PutDate("EDATE", time);
                    }
                }
                ei.Put("CMP", location);
                ei.Put("FCUR ", item.FromCurrency);
                ei.Put("TCUR", item.ToCurrency);
                ei.Put("EX_RATE", item.DirectQuotedExchangeRate);
                eiList.Add(ei);

                if ("CNY".Equals(item.FromCurrency) || "CNY".Equals(item.ToCurrency))
                {
                    upcny = true;
                }
            }
            ResultInfo resultinfo = Execute(eiList);
            if ("M".Equals(ratecd) && resultinfo.IsSucceed && upcny)
            {
                try
                {
                    int year = DateTime.Now.Year;
                    int month = DateTime.Now.Month;
                    int lastmonth = month + 1;
                    int lastyear = year;
                    if (lastmonth > 12)
                    {
                        lastyear = year + 1;
                        lastmonth = 1;
                    }
                    string strmonth = month.ToString();
                    if (strmonth.Length == 1)
                        strmonth = "0" + strmonth;
                    string strlsthmonth = lastmonth.ToString();
                    if (strlsthmonth.Length == 1)
                        strlsthmonth = "0" + strlsthmonth;
                    //DateTime etdfrom = Prolink.Math.GetValueAsDateTime(year + strmonth + "01");
                    //DateTime etdto = Prolink.Math.GetValueAsDateTime(lastyear + strlsthmonth + "01");
                    string etdfrom = year + "-" + strmonth + "-01";
                    string etdto = lastyear + "-" + strlsthmonth + "-01";
                    string sql = string.Format(@"SELECT SHIPMENT_ID,U_ID,ATP FROM SMSM WHERE (TRAN_TYPE='F' OR TRAN_TYPE='L') AND REGION='NA' AND ATP>={0} AND
                    ATP<{1} AND STATUS NOT IN('A','B','V','Z')union
                    SELECT SHIPMENT_ID,U_ID,ATP FROM SMSM WHERE (TRAN_TYPE='F' OR TRAN_TYPE='L') AND REGION='NA' AND ATP IS NULL AND ATD>={0} AND
                    ATD<{1} AND STATUS NOT IN('A','B','V','Z')union
                    SELECT SHIPMENT_ID,U_ID,ATP FROM SMSM WHERE (REGION NOT IN ('NA') OR REGION IS NULL) AND  ETD>={0} AND
                    ETD<{1} AND STATUS NOT IN('A','B','V','Z')", SQLUtils.QuotedStr(etdfrom), SQLUtils.QuotedStr(etdto));
                    DataTable dt = DB.GetDataTable(sql, new string[] { });
                    MixedList ml = new MixedList();
                    foreach (DataRow dr in dt.Rows)
                    {
                        EditInstruct AutoValuationTaskEi = new EditInstruct("AUTO_VALUATION_TASK", EditInstruct.INSERT_OPERATION);
                        AutoValuationTaskEi.Put("U_ID", dr["U_ID"].ToString());
                        AutoValuationTaskEi.Put("SMU_ID", dr["U_ID"].ToString());
                        AutoValuationTaskEi.Put("CREATE_BY", "ExchangeRate");
                        AutoValuationTaskEi.PutDate("CREATE_DATE", DateTime.Now);
                        ml.Add(AutoValuationTaskEi);
                    }
                    if (ml.Count > 0)
                    {
                        DB.ExecuteUpdate(ml);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return resultinfo;
        }
    }
}