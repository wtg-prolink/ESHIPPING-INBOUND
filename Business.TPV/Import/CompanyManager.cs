using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
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
    public class CompanyManager : CompanyManagerBase
    {
        public override ResultInfo Import(string sapId,string location, string partyNO = null, DateTime? from = null, DateTime? to = null)
        {
            CompanyEDI edi = new CompanyEDI();
            List<CompanyInfo> infos = edi.GetCompanyInfo(sapId,location, partyNO).ToList(); 
            ResultInfo result = Save(infos, location, sapId);
            WriteEDILog(infos, partyNO, result);
            return new ImportCompanyResultInfo() { Description = result.Description, IsSucceed = result.IsSucceed, ResultCode = result.ResultCode, Infos = infos };
        }
    }

    public abstract class CompanyManagerBase : ManagerBase
    {
        protected override List<string> GetCheckTables()
        {
            return new List<string>() { "SMPTY" };
        }
        public virtual ResultInfo Import(string sapId,string location, string partyNO = null, DateTime? from = null, DateTime? to = null)
        {
            PartnerEDI edi = new PartnerEDI();
            List<CompanyInfo> infos = edi.GetPartnerInfo(sapId, location,partyNO, from, to).ToList();
            if (infos == null || infos.Count <= 0)
                return new ImportCompanyResultInfo() { Description = "处理已完成,未获取到任何资料！", IsSucceed = string.IsNullOrEmpty(partyNO), ResultCode = ResultCode.Succeed, Infos = infos };
            infos = FilterPartyNo(infos, location, sapId);
            ResultInfo result = Save(infos,location, sapId);
            WriteEDILog(infos, partyNO, result);
            return new ImportCompanyResultInfo() { Description = result.Description, IsSucceed = result.IsSucceed, ResultCode = result.ResultCode, Infos = infos };
        }

        internal void WriteEDILog(List<CompanyInfo> infoList, string refNO, ResultInfo result)
        {
            Utils.EdiInfo info = null;
            PartnerEDILog log = new PartnerEDILog(infoList, refNO, "System");
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

        DataTable QuerySmptyFilter()
        {
            string sql = "SELECT PARTY_NO,SOURCE_CODE FROM SMPTY_FILTER ";
            return DB.GetDataTable(sql, new string[] { });
        }

        DataTable QueryCountry(List<CompanyInfo> infos)
        {
            List<string> countryCode = infos.Select(info => info.CountryKey).Distinct().ToList();
            if (countryCode == null || countryCode.Count <= 0) return null;
            string sql = string.Format("SELECT CNTRY_CD,CNTRY_NM FROM BSCNTY WHERE CNTRY_CD IN({0})",
                string.Join(",", countryCode.Select(country => SQLUtils.QuotedStr(country))));
            return DB.GetDataTable(sql, new string[] { });
        }

        DataTable QueryCity(List<CompanyInfo> infos)
        {
            List<string> cityName = infos.Select(info => info.City).Distinct().ToList();
            if (cityName == null || cityName.Count <= 0) return null;
            string sql = string.Format("SELECT PORT_CD,PORT_NM FROM BSCITY  WHERE PORT_NM IN({0})",
                string.Join(",", cityName.Select(city => SQLUtils.QuotedStr(city))));
            return DB.GetDataTable(sql, new string[] { });
        }

        internal bool CheckIsSave(CompanyInfo info)
        {
            return true;
            if (string.IsNullOrEmpty(info.VendorAccountNumber)) return true;
            if (string.IsNullOrEmpty(info.PartnerNumber)) return false;
            if (info.PartnerNumber.Length < 6) return false;
            string flag = info.PartnerNumber.Substring(0, 6);
            List<string> items = new List<string> { "000891", "000892", "000893", "000895" };
            return items.Contains(flag);
        }

        internal ResultInfo Save(List<CompanyInfo> infos,string location,string sapId)
        { 
            Func<string, bool> checkHas = code =>
            {
                string sql = string.Format(@"SELECT PARTY_NO,SOURCE_CODE FROM SMPTY WHERE PARTY_NO={0} AND SOURCE_CODE ={1}",
                    SQLUtils.QuotedStr(code), SQLUtils.QuotedStr(sapId));
                DataTable preDT = DB.GetDataTable(sql, new string[] { });
                return preDT != null && preDT.Rows.Count > 0;
            };
            DataTable countryDT = QueryCountry(infos);
            DataTable cityDT = QueryCity(infos);
            Func<string, string> getCountryName = code =>
                {
                    DataRow[] rows = countryDT.Select(string.Format("CNTRY_CD={0}", SQLUtils.QuotedStr(code)));
                    if (rows != null && rows.Length > 0)
                        return Prolink.Math.GetValueAsString(rows[0]["CNTRY_NM"]);
                    return string.Empty;
                };
            Func<string, string> getCityCode = name =>
            {
                DataRow[] rows = cityDT.Select(string.Format("PORT_NM={0}", SQLUtils.QuotedStr(name)));
                if (rows != null && rows.Length > 0)
                    return Prolink.Math.GetValueAsString(rows[0]["PORT_CD"]);
                return string.Empty;
            };
            EditInstructList eiList = new EditInstructList();
            string sapIdsql = string.Format("SELECT TOP 1 AP_CD FROM BSCODE WHERE CD_TYPE='SAPID' AND CD={0}", SQLUtils.QuotedStr(sapId));
            string PartnerNumberflag = OperationUtils.GetValueAsString(sapIdsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (var item in infos)
            {
                SetPartnerNumber(location, PartnerNumberflag, item);

                BackupData(item, string.Format("{0}_{1}", item.PartnerNumber, GetCurrentTimeString())); 
                EditInstruct ei = null;
                if (checkHas(item.PartnerNumber))
                {
                    ei = new EditInstruct("SMPTY", EditInstruct.UPDATE_OPERATION);
                    ei.Condition = string.Format("PARTY_NO={0} AND SOURCE_CODE={1}", SQLUtils.QuotedStr(item.PartnerNumber), SQLUtils.QuotedStr(sapId));
                    ei.PutDate("MODIFY_DATE", DateTime.Now);
                }
                else
                {
                    ei = new EditInstruct("SMPTY", EditInstruct.INSERT_OPERATION);
                    ei.Put("U_ID", System.Guid.NewGuid().ToString().Replace("-", ""));
                    ei.PutDate("CREATE_DATE", DateTime.Now);
                }
                if (string.IsNullOrEmpty(location))
                    location="FQ";
                ei.Put("CMP", location);
                ei.Put("SOURCE_CODE", sapId); 

                if (!string.IsNullOrEmpty(item.Function))
                    ei.Put("PARTY_TYPE", item.Function);
                ei.Put("GROUP_ID", Business.TPV.Context.GroupId);
                ei.Put("PARTY_NO", item.PartnerNumber);
                ei.Put("STATUS ", "U");
                string abbr = item.SearchTerm1;
                if (string.IsNullOrEmpty(abbr))
                    abbr = item.SearchTerm2;
                ei.Put("ABBR", abbr);
                ei.Put("HEAD_OFFICE", item.PartnerNumber);
                ei.Put("PARTY_NAME", item.Name1);
                ei.Put("PARTY_NAME2", item.Name2);
                ei.Put("PARTY_NAME3", item.Name3);
                ei.Put("PARTY_NAME4", item.Name4);
                ei.Put("PART_ADDR1", item.Street);
                ei.Put("PART_ADDR2", item.Street2);
                ei.Put("PART_ADDR3", item.Street3);
                ei.Put("PART_ADDR4", item.Street4);
                ei.Put("PART_ADDR5", item.Street5);
                ei.Put("CNTY", item.CountryKey);
                ei.Put("CNTY_NM", getCountryName(item.CountryKey));
                string citycode = getCityCode(item.City);
                if (!string.IsNullOrEmpty(citycode))
                {
                    ei.Put("CITY", citycode);
                }
                else {
                    ei.Put("CITY", item.CityCode);
                }
                ei.Put("CITY_NM", item.City);
                ei.Put("STATE", item.Region);
                ei.Put("ZIP", item.PostalCode);
                if (!string.IsNullOrEmpty(item.FirstName) || !string.IsNullOrEmpty(item.LastName))
                    ei.Put("PARTY_ATTN", string.Format("{0} {1}", item.FirstName, item.LastName));
                string tel = item.Telephone1;
                if (!string.IsNullOrEmpty(item.Telephone1Extension))
                    tel = string.Join("#", item.Telephone1, item.Telephone1Extension);
                ei.Put("PARTY_TEL", tel);
                ei.Put("PARTY_MAIL", item.EMail);
                string fax = item.Fax;
                if (!string.IsNullOrEmpty(item.FaxExtension))
                    fax = string.Join("#", item.Fax, item.FaxExtension);
                ei.Put("PARTY_FAX", fax);
                ei.Put("TAX_NO", item.VATNumber);
                ei.Put("BILL_TO", item.PartnerNumber);
                ei.Put("REMARK", item.AddressNotes);
                //ei.Put("DUE_DAY", item);
                eiList.Add(ei);
            }
            return Execute(eiList);
        }

        private static void SetPartnerNumber(string location, string PartnerNumberflag, CompanyInfo item)
        {
            if (string.IsNullOrEmpty(item.PartnerNumber)) return;
            if ("BR".Equals(location) && !string.IsNullOrEmpty(item.PartnerNumber))
                PartnerNumberflag = "B";
            if (string.IsNullOrEmpty(PartnerNumberflag)) return; 
            string startCode = item.PartnerNumber.Substring(0, 1);
            if (PartnerNumberflag == startCode) return;
            if (item.PartnerNumber.Length >= 10 && "0".Equals(startCode))
                item.PartnerNumber = PartnerNumberflag + item.PartnerNumber.Substring(1, item.PartnerNumber.Length - 1);
            else
                item.PartnerNumber = PartnerNumberflag + item.PartnerNumber; 
        }
        List<CompanyInfo> FilterPartyNo(List<CompanyInfo> infos, string location, string spaId)
        {
            DataTable filterDT = QuerySmptyFilter();
            if (filterDT == null || filterDT.Rows.Count <= 0) return infos;
            string sql = string.Format("SELECT TOP 1 AP_CD FROM BSCODE WHERE CD_TYPE='SAPID' AND CD={0}", SQLUtils.QuotedStr(spaId));
            string PartnerNumberflag = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<CompanyInfo> listinfo = new List<CompanyInfo>();
            string party_no = string.Empty;
            foreach (var item in infos)
            {
                SetPartnerNumber(location, PartnerNumberflag, item);
                DataRow[] rows = filterDT.Select(string.Format("PARTY_NO={0} AND SOURCE_CODE={1}", SQLUtils.QuotedStr(item.PartnerNumber), SQLUtils.QuotedStr(location)));
                if (rows != null && rows.Length >= 1)
                {
                    listinfo.Add(item);
                }
            }
            if (listinfo.Count > 0)
            {
                foreach (var item in listinfo)
                {
                    infos.Remove(item);
                }
            }
            return infos;
        }
    }

    class ImportCompanyResultInfo : ResultInfo
    {
        internal List<CompanyInfo> Infos { get; set; }
    }
}