using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class CustomerProfileEDI : EDIBase
    {
        public IEnumerable<ProfileInfo> GetCustomerProfileInfo(string sapId, string profileCode, string location)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            if (!string.IsNullOrEmpty(profileCode))
                parameters.Add("PROFILE", profileCode);
            IRfcFunction function = GetOperator("ZRFC_ESP_PROFILE", parameters, location);
            return Parse(function);
        }

        IEnumerable<ProfileInfo> Parse(IRfcFunction function)
        {
            IRfcTable table = function.GetTable("PROFILE_TAB");
            foreach (IRfcStructure row in table)
            {
                ProfileInfo info = new ProfileInfo();
                info.IsUse = row.GetFieldValueAsString("FUSED");
                info.IsDeleted = row.GetFieldValueAsString("FDELE");
                info.Vendor = row.GetFieldValueAsString("LIFNR");
                info.ShiptoOrZPZQ = row.GetFieldValueAsString("KUNWE");
                info.ProfileCode = row.GetFieldValueAsString("PROFILE");
                info.CompanyCode = row.GetFieldValueAsString("BUKRS");
                info.ProfileName = row.GetFieldValueAsString("PRONAME");
                info.InvoiceFlow = row.GetFieldValueAsString("IFLOW");
                info.ModelName = row.GetFieldValueAsString("MODEL");
                info.Customer = row.GetFieldValueAsString("KUNFC");
                info.Seller = row.GetFieldValueAsString("KUNSL");
                info.Buyer1 = row.GetFieldValueAsString("KUNAG1");
                info.Incoterm1 = row.GetFieldValueAsString("INCO11");
                info.Buyer2 = row.GetFieldValueAsString("KUNAG2");
                info.Incoterm2 = row.GetFieldValueAsString("INCO12");
                info.Buyer3 = row.GetFieldValueAsString("KUNAG3");
                info.Incoterm3 = row.GetFieldValueAsString("INCO13");
                info.Buyer4 = row.GetFieldValueAsString("KUNAG4");
                info.Incoterm4 = row.GetFieldValueAsString("INCO14");
                info.Buyer5 = row.GetFieldValueAsString("KUNAG5");
                info.Incoterm5 = row.GetFieldValueAsString("INCO15");
                info.PortOfDischarge = row.GetFieldValueAsString("PODIS");
                info.Consignee = row.GetFieldValueAsString("KUNCS");
                info.ShippingMode = row.GetFieldValueAsString("TRATY");
                info.Notify1 = row.GetFieldValueAsString("KUNNT1");
                info.Notify2 = row.GetFieldValueAsString("KUNNT2");
                info.Notify3 = row.GetFieldValueAsString("KUNNT3");
                info.PKType = row.GetFieldValueAsString("PKTYP");
                yield return info;
            }
        }
    }

    public class ProfileInfo
    {
        public string PKType { get; set; }
        public string IsUse { get; set; }
        public string IsDeleted { get; set; }
        public string Vendor { get; set; }
        public string ShiptoOrZPZQ { get; set; }
        public string ProfileCode { get; set; }
        public string CompanyCode { get; set; }
        public string ProfileName { get; set; }
        public string InvoiceFlow { get; set; }
        public string ModelName { get; set; }
        public string Customer { get; set; }
        public string Seller { get; set; }
        public string Buyer1 { get; set; }
        public string Incoterm1 { get; set; }
        public string Buyer2 { get; set; }
        public string Incoterm2 { get; set; }
        public string Buyer3 { get; set; }
        public string Incoterm3 { get; set; }
        public string Buyer4 { get; set; }
        public string Incoterm4 { get; set; }
        public string Buyer5 { get; set; }
        public string Incoterm5 { get; set; }
        public string PortOfDischarge { get; set; }
        public string Consignee { get; set; }
        public string ShippingMode { get; set; }
        public string Notify1 { get; set; }
        public string Notify2 { get; set; }
        public string Notify3 { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}