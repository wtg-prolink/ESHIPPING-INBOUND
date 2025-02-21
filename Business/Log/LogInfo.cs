using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Log
{
    public class LogInfo
    {
        public string ID { get; set; }
        public string RefNO { get; set; }
        public string MsgInfo { get; set; }
        public string MsgInfo2 { get; set; }
        public string MsgType { get; set; }
        public string Remark { get; set; }
        public string MsgLevel { get; set; }
        public string CreateBy { get; set; }
        public string TCode { get; set; }
        public string IP { get; set; }
        public object Data { get; set; }
    }
}
