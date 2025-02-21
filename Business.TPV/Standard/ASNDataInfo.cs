using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.Standard
{
    public class ASNDataInfo
    {
        public string Invoice_Number { get; set; }
        public string ASN_Number { get; set; }
        public DateTime ASN_Date { get; set; }
        public DateTime Send_Time { get; set; }
        public DateTime GR_Date { get; set; }
        public string Gr_Status { get; set; }
        public string ASN_Date_Update { get; set; }
        public string ChangedBy { get; set; }
    }
}
