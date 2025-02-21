using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.InboundProcess
{
    public class BookingAgent
    {
        public DateTime? ATA { get; set; }
        public string BACK_LOCATION { get; set; }
        public string PIN_NO { get; set; }
        public DateTime? DISCHARGE_DATE { get; set; } 
        public string InboundTerminalAgent { get; set; } 
    }
} 