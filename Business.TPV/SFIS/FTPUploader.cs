using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.SFIS
{
    class FTPUploader : Business.Export.FTPUploader
    {
        public FTPUploader(ExportModes mode)
        {
            Mode = mode;
        }

        public ExportModes Mode { get; private set; }
        protected override FTPConfig CreateConfig()
        {
            string filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, "edi/ftp/SFIS.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            string nodeName = string.Empty;
            switch (Mode)
            {
                case ExportModes.FQDNStatus: nodeName = "FQDNStatus"; break;
                case ExportModes.DNStatus: nodeName = "FQDNStatus"; break;
                case ExportModes.XMDNStatus: nodeName = "XMDNStatus"; break;
                case ExportModes.BJDNStatus: nodeName = "BJDNStatus"; break;
                case ExportModes.QDDNStatus: nodeName = "QDDNStatus"; break;
                case ExportModes.WHDNStatus: nodeName = "WHDNStatus"; break;
                case ExportModes.BHDNStatus: nodeName = "BHDNStatus"; break;
                case ExportModes.OrderNubmer: nodeName = "SendOrderNO"; break;
                case ExportModes.FQOrderNubmer: nodeName = "FQSendOrderNO"; break;
                case ExportModes.XMOrderNubmer: nodeName = "XMSendOrderNO"; break;
                case ExportModes.BJOrderNubmer: nodeName = "BJSendOrderNO"; break;
                case ExportModes.QDOrderNubmer: nodeName = "QDSendOrderNO"; break;
                case ExportModes.WHOrderNubmer: nodeName = "WHSendOrderNO"; break;
                case ExportModes.BHOrderNubmer: nodeName = "BHSendOrderNO"; break;
            }
            XmlNodeList nodeList = doc.GetElementsByTagName(nodeName);
            return Business.Utils.ConfigManager.GetFTPConfig(nodeList).First();
        }
    }

    enum ExportModes
    {
        OrderNubmer, DNStatus, FQDNStatus, XMDNStatus, BJDNStatus, QDDNStatus, WHDNStatus, BHDNStatus,
        FQOrderNubmer, XMOrderNubmer, BJOrderNubmer, QDOrderNubmer, WHOrderNubmer, BHOrderNubmer
    }
}
