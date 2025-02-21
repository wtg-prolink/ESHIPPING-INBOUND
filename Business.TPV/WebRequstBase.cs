using Business.EDI;
using Business.Service;
using Business.TPV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV
{
    public abstract class WebRequstBase : XmlWebRequst
    {
        protected abstract RequstModes Mode { get; }

        protected override string Backup(IEnumerable<string> dirConcatenates, System.Xml.XmlDocument doc, string fileName = "")
        {
            List<string> list = new List<string> { Mode.ToString() };
            list.AddRange(dirConcatenates);
            if (string.IsNullOrEmpty(fileName))
            {
                if (Runtime != null && !string.IsNullOrEmpty(Runtime.ShipmentID))
                    fileName = string.Format("{0}_{1}", Runtime.ShipmentID, GetCurrentTimeString());
            }
            return base.Backup(list, doc, fileName);
        }

        protected Runtime Runtime
        {
            get;
            set;
        }

        internal void WriteEDILog(BookingEDILog log, ResultInfo result)
        {
            if (result.IsSucceed)
            {
                Logger.WriteLog(log.CreateSucceed());
            }
            else
            {
                Logger.WriteLog(log.CreateEx(result.Description));
            }
        }
    }

    public enum RequstModes { None, DHL, TNT, CPL, LSP, Cosco }
}