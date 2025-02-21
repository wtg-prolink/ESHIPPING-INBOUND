using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.InboundProcess
{
    public class TruckCompany
    { 
        public DateTime? ARRIVAL_DATE { get; set; }
        public DateTime? USE_DATE { get; set; }
        public DateTime? IN_DATE_L { get; set; }
        public DateTime? POD_UPDATE_DATE { get; set; }
        public DateTime? OUT_DATE_L { get; set; } 
        public string TRUCK_NO { get; set; }
        public string DRIVER { get; set; }
        public string DRIVER_ID { get; set; }
        public string TEL { get; set; }
        public string LTRUCK_NO { get; set; }
        public string LDRIVER { get; set; }
        public string LDRIVER_ID { get; set; }
        public string LTEL { get; set; }
        public DateTime? HEAVY_PICKUP_TIME { get; set; }
        public DateTime? EMPTY_RETURN_TIME { get; set; }
        public DateTime? AT_YARD_TIME { get; set; }
        public string BACK_LOCATION { get; set; }
        public DateTime? EMPTY_TIME { get; set; }

        public string RESERVE_NO { get; set; }

    }
}
