using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Models.EDI
{
    public class OutBoundFreight
    {
        public string ShipmentId { get; set; }
        public string Cmp { get; set; }
        public string GroupId { get; set; }
        public string Pod { get; set; }
        public string Pol { get; set; }
        public string TranType { get; set; }
        public string Wms { get; set; }
        public string IscombineBl { get; set; }
        public string ShipMode { get; set; }
        public string CustomsCheck { get; set; }
        public string DeclNum { get; set; }
        public string ContDeclNum { get; set; }
        public string NextNum { get; set; }
        public string SmsmPod { get; set; }
        public string TrackWay { get; set; }
        public decimal Pcnt20 { get; set; }
        public decimal Pcnt40 { get; set; }
        public decimal Pcnt40Hq { get; set; }
        public decimal PocntNumber { get; set; }
        public string PcntType { get; set; }
        public decimal PcntNumber { get; set; } 
        public decimal CntNum { get; set; }
        public decimal DnNum { get; set; }
        public decimal Cbm { get; set; }
        public decimal Cnt { get; set; }
        public decimal Nw { get; set; }
        public string Gwu { get; set; }
        public decimal Gw { get; set; }
        public decimal Gvalue { get; set; }
        public string BillDate { get; set; }
        public string BillDescp { get; set; }
        public string BlNo { get; set; } 
        public string BrPartyNo { get; set; }
        public string BrPartyNm { get; set; }
        public string CrPartyNo { get; set; }
        public string CrPartyNm { get; set; }
        public string SpPartyNo { get; set; }
        public string SpPartyNm { get; set; }
        public string FcPartyNo { get; set; }
        public string FcPartyNm { get; set; }
        public string BoPartyNo { get; set; }
        public string BoPartyNm { get; set; } 
        public string ShPartyNo { get; set; }
        public string ShPartyNm { get; set; }

        public DataTable RateDt { get; set; } 
        public string Cout { get; set; }
        public string IncotermCd { get; set; }
        public string Carrier { get; set; } 
        public List<string> DebitToList { get; set; }
        public string FrtTerm { get; set; } 
        public string TelexRls { get; set; } 
        public string Region { get; set; }
        public string Horn { get; set; }
        public string Battery { get; set; }
        public string IsLand { get; set; } 
        public string OcntType { get; set; }  
        public string IsSplitBill { get; set; } 
        public DataRow Current_smsm { get; set; }   
        public string InvoiceInfo { get; set; }
        public string CntrInfo { get; set; }
    }
}
