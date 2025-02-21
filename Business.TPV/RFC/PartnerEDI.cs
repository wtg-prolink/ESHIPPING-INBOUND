using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class PartnerEDI : EDIBase
    {
        public IEnumerable<CompanyInfo> GetPartnerInfo(string sapId, string location, string partnerCode = null, DateTime? from = null, DateTime? to = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            if (!string.IsNullOrEmpty(partnerCode))
                parameters.Add("PARTNER", partnerCode);
            if (from.HasValue)
                parameters.Add("DATE_FROM", from.Value);
            if (to.HasValue)
                parameters.Add("DATE_TO", to.Value);
            IRfcFunction function = GetOperator("ZRFC_ESP_PARTNER", parameters, location);
            try
            {
                return Parse(function);
            }
            catch
            {
                return null;
            }
        }

        IEnumerable<CompanyInfo> Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("PARTNER_TAB");
            foreach (IRfcStructure row in table)
            {
                CompanyInfo info = new CompanyInfo();
                info.VendorAccountNumber = row.GetFieldValueAsString("LIFNR");
                info.CustomerNumber = row.GetFieldValueAsString("KUNNR");
                info.PartnerNumber = row.GetFieldValueAsString("PARTNER");
                info.Indicator = row.GetFieldValueAsString("XCPDK");
                info.CreateDate = row.GetFieldValueAsString("ERDAT");
                info.EntryTime = row.GetFieldValueAsString("ERZET");
                info.CreateBy = row.GetFieldValueAsString("ERNAM");
                info.ChangedDate = row.GetFieldValueAsString("AEDAT");
                info.LastChangedTime = row.GetFieldValueAsString("AEZET");
                info.ChangedBy = row.GetFieldValueAsString("AENAM");
                info.AddressKey = row.GetFieldValueAsString("TITLE");
                info.Name1 = row.GetFieldValueAsString("NAME1");
                info.Name2 = row.GetFieldValueAsString("NAME2");
                info.Name3 = row.GetFieldValueAsString("NAME3");
                info.Name4 = row.GetFieldValueAsString("NAME4");
                info.ConvertedName = row.GetFieldValueAsString("NAME_TXT");
                info.NameCO = row.GetFieldValueAsString("NAME_CO");
                info.City = row.GetFieldValueAsString("CITY1");
                info.District = row.GetFieldValueAsString("CITY2");
                info.CityCode = row.GetFieldValueAsString("CITY_CODE");
                info.DistrictCode = row.GetFieldValueAsString("CITYP_CODE");
                info.PostalCity = row.GetFieldValueAsString("HOME_CITY");
                info.StreetD = row.GetFieldValueAsString("CITYH_CODE");
                info.CityStatus = row.GetFieldValueAsString("CHCKSTATUS");
                info.RegionalStructureGrouping = row.GetFieldValueAsString("REGIOGROUP");
                info.PostalCode = row.GetFieldValueAsString("POST_CODE1");
                info.POBoxPostalCode = row.GetFieldValueAsString("POST_CODE2");
                info.CompanyPostalCode = row.GetFieldValueAsString("POST_CODE3");
                info.PostalCodeExtension = row.GetFieldValueAsString("PCODE1_EXT");
                info.POBoxPostalCodeExtension = row.GetFieldValueAsString("PCODE2_EXT");
                info.MajorCustomerPostalCodeExtension = row.GetFieldValueAsString("PCODE3_EXT");
                info.POBox = row.GetFieldValueAsString("PO_BOX");
                info.POBoxAddressUndeliverableFlag = row.GetFieldValueAsString("DONT_USE_P");
                info.POBoxWithoutNumber = row.GetFieldValueAsString("PO_BOX_NUM");
                info.POBoxCity = row.GetFieldValueAsString("PO_BOX_LOC");
                info.POBoxCityCode = row.GetFieldValueAsString("CITY_CODE2");
                info.POBoxRegion = row.GetFieldValueAsString("PO_BOX_REG");
                info.POBoxCountry = row.GetFieldValueAsString("PO_BOX_CTY");
                info.PostDeliveryDistrict = row.GetFieldValueAsString("POSTALAREA");
                info.TransportationZone = row.GetFieldValueAsString("TRANSPZONE");
                info.Street = row.GetFieldValueAsString("STREET");
                info.StreetAddressUndeliverableFlag = row.GetFieldValueAsString("DONT_USE_S");
                info.StreetNumber = row.GetFieldValueAsString("STREETCODE");
                info.AbbreviationOfStreetName = row.GetFieldValueAsString("STREETABBR");
                info.HouseNumber = row.GetFieldValueAsString("HOUSE_NUM1");
                info.HouseNumberSupplement = row.GetFieldValueAsString("HOUSE_NUM2");
                info.HouseNumberRange = row.GetFieldValueAsString("HOUSE_NUM3");
                info.Street2 = row.GetFieldValueAsString("STR_SUPPL1");
                info.Street3 = row.GetFieldValueAsString("STR_SUPPL2");
                info.Street4 = row.GetFieldValueAsString("STR_SUPPL3");
                info.Street5 = row.GetFieldValueAsString("LOCATION");
                info.Building = row.GetFieldValueAsString("BUILDING");
                info.Floor = row.GetFieldValueAsString("FLOOR");
                info.Room = row.GetFieldValueAsString("ROOMNUMBER");
                info.CountryKey = row.GetFieldValueAsString("COUNTRY");
                info.LanguageKey = row.GetFieldValueAsString("LANGU");
                info.Region = row.GetFieldValueAsString("REGION");
                info.SearchTerm1 = row.GetFieldValueAsString("SORT1");
                info.SearchTerm2 = row.GetFieldValueAsString("SORT2");
                info.PhoneticSearchSortField = row.GetFieldValueAsString("SORT_PHN");
                info.AddressDataSourceKey = row.GetFieldValueAsString("ADDRORIGIN");
                info.Extension = row.GetFieldValueAsString("EXTENSION1");
                info.Extension2 = row.GetFieldValueAsString("EXTENSION2");
                info.TimeZone = row.GetFieldValueAsString("TIME_ZONE");
                info.TaxJurisdiction = row.GetFieldValueAsString("TAXJURCODE");
                info.PhysicalAdressID = row.GetFieldValueAsString("ADDRESS_ID");
                info.AddressNotes = row.GetFieldValueAsString("REMARK");
                info.OriginalLanguage = row.GetFieldValueAsString("LANGU_CREA");
                info.POBoxLobby = row.GetFieldValueAsString("PO_BOX_LOBBY");
                info.DeliveryServiceType = row.GetFieldValueAsString("DELI_SERV_TYPE");
                info.DeliveryServiceNumber = row.GetFieldValueAsString("DELI_SERV_NUMBER");
                info.PrefectureCode = row.GetFieldValueAsString("COUNTY_CODE");
                info.Prefecture = row.GetFieldValueAsString("COUNTY");
                info.TownshipCode = row.GetFieldValueAsString("TOWNSHIP_CODE");
                info.Township = row.GetFieldValueAsString("TOWNSHIP");
                info.Title = row.GetFieldValueAsString("TITLE_MEDI");
                info.POBoxPostalCode = row.GetFieldValueAsString("POSTCODE2");
                info.POBoxPostalCode2 = row.GetFieldValueAsString("POSTCODE2C");
                info.Telephone1 = row.GetFieldValueAsString("TEL_NUMBER");
                info.Telephone1Extension = row.GetFieldValueAsString("TEL_EXTENS");
                info.Fax = row.GetFieldValueAsString("FAX_NUMBER");
                info.FaxExtension = row.GetFieldValueAsString("FAX_EXTENS");
                info.EMail = row.GetFieldValueAsString("SMTP_ADDR");
                info.MobileTelephone = row.GetFieldValueAsString("MOB_NUMBER");
                info.MobileTelephoneExtension = row.GetFieldValueAsString("MOB_EXTENS");
                info.SAPOfficeKey = row.GetFieldValueAsString("SO_KEY");
                info.Department = row.GetFieldValueAsString("DEPARTMENT");
                info.Function = row.GetFieldValueAsString("FUNCTION");
                info.Building_C = row.GetFieldValueAsString("BUILDING_C");
                info.Floor_C = row.GetFieldValueAsString("FLOOR_C");
                info.Room_C = row.GetFieldValueAsString("ROOMNUM_C");
                info.ShortName = row.GetFieldValueAsString("ID_CODE");
                info.IntMail = row.GetFieldValueAsString("IH_MAIL");
                info.SearchTerm1_C = row.GetFieldValueAsString("SORT1_C");
                info.SearchTerm2_C = row.GetFieldValueAsString("SORT2_C");
                info.PhoneticSearchSortField_C = row.GetFieldValueAsString("SORT_PHN_C");
                info.AddressKey_P = row.GetFieldValueAsString("TITLE_P");
                info.FirstName = row.GetFieldValueAsString("NAME_FIRST");
                info.LastName = row.GetFieldValueAsString("NAME_LAST");
                info.NameOfPersonAtBirth = row.GetFieldValueAsString("NAME2_P");
                info.SecondName = row.GetFieldValueAsString("NAMEMIDDLE");
                info.SecondLastName = row.GetFieldValueAsString("NAME_LAST2");
                info.FullName = row.GetFieldValueAsString("NAME_TEXT");
                info.FullNameStatus = row.GetFieldValueAsString("CONVERTED");
                info.AcademicTitle = row.GetFieldValueAsString("TITLE_ACA1");
                info.AcademicTitle2 = row.GetFieldValueAsString("TITLE_ACA2");
                info.Prefix = row.GetFieldValueAsString("PREFIX1");
                info.Prefix2 = row.GetFieldValueAsString("PREFIX2");
                info.NameSupplement = row.GetFieldValueAsString("TITLE_SPPL");
                info.Nickname = row.GetFieldValueAsString("NICKNAME");
                info.Initial = row.GetFieldValueAsString("INITIALS");
                info.NameFormat = row.GetFieldValueAsString("NAMEFORMAT");
                info.CountryFormat = row.GetFieldValueAsString("NAMCOUNTRY");
                info.Profession = row.GetFieldValueAsString("PROFESSION");
                info.Gender = row.GetFieldValueAsString("SEX");
                info.LanguageKey_P = row.GetFieldValueAsString("LANGU_P");
                info.SearchTerm1_P = row.GetFieldValueAsString("SORT1_P");
                info.SearchTerm2_P = row.GetFieldValueAsString("SORT2_P");
                info.PhoneticSearchSortField_P = row.GetFieldValueAsString("SORT_PHN_P");
                info.PersonalDataSource = row.GetFieldValueAsString("PERSORIGIN");
                info.OriginalLanguage_P = row.GetFieldValueAsString("LANGU_CR_P");
                info.Group = row.GetFieldValueAsString("KONZS");
                info.VendorAccountGroup = row.GetFieldValueAsString("KTOKK");
                info.CompanyIDOfTradingPartner = row.GetFieldValueAsString("VBUND");
                info.VATNumber = row.GetFieldValueAsString("STCEG");
                yield return info;
            }
        }
    }

    class CompanyInfo : Models.EDI.PartnerInfo
    {

    }
}