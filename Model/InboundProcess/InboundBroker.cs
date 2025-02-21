using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.InboundProcess
{  
    public class InboundBroker
    { 
        public string DEC_NO { get; set; }
        public string IMPORT_NO { get; set; }
        public DateTime? DEC_DATE { get; set; }
        public DateTime? REL_DATE { get; set; }
        public string INSPECTION { get; set; }
        public string CER_NO { get; set; }
        public string DEC_REPLY { get; set; }
        public string ICDF { get; set; }
        public string CC_CHANNEL { get; set; }
        public string HS_QTY { get; set; }
        public string CNTRY_CD { get; set; }
        public string PLI { get; set; }
        public string LI { get; set; }
        public string SUF_COST { get; set; }
        public string CC_RATE { get; set; }
        public string ADD_QTY { get; set; }
        public string SIS_FEE { get; set; }
    }
}
