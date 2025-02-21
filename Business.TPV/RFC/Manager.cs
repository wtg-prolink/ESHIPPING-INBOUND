using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.RFC
{
    class Manager
    {
        public static RFCConfig GetRFCConfig(FactoryCode code,string location)
        {
            string name = string.Format("edi/{0}_SAP_RFC.xml", location);
            string filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, name);
            if (!File.Exists(filePath))
            {
                name = "edi/SAP_RFC.xml";
                filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, name);
                if (!File.Exists(filePath))
                    throw new Exception("程式未配置！");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlNodeList nodeList = doc.GetElementsByTagName("Factory");
            if (nodeList == null || nodeList.Count <= 0) throw new Exception("程式未配置！");
            RFCConfig config = new RFCConfig();
            foreach (XmlElement node in nodeList)
            {
                string fCode = node.GetAttribute("code");
                if (fCode != code.ToString()) continue;
                foreach (XmlNode cNode in node.ChildNodes)
                {
                    if (cNode is System.Xml.XmlComment) continue;
                    string destinationName = cNode.Attributes["destinationName"].Value;
                    RfcConfigParameters parameters = new RfcConfigParameters();
                    foreach (XmlNode dItem in cNode.ChildNodes)
                    {
                        if (string.IsNullOrEmpty(dItem.InnerText)) continue;
                        parameters.Add(dItem.Name, dItem.InnerText);
                    }
                    config.Parmeters.Add(destinationName, parameters);
                }
            }
            return config;
        }

        static DestinationConfiguration _config;
        public static RfcDestination CreateRfcDestination(FactoryCode code,string location)
        {
            RFCConfig c = GetRFCConfig(code, location);
            if (c == null) throw new Exception(string.Format("{0} RFC Not Configuration!", code));
            if (_config != null)
                _config.Dispose();
            _config = new DestinationConfiguration(c);
            string destinationName = string.Empty;
            foreach (var item in c.Parmeters)
            {
                destinationName = item.Key;
            }
            return RfcDestinationManager.GetDestination(destinationName);
        }
        public static void DisposeDestinationConfig()
        {
            try
            {
                if (_config != null)
                    _config.Dispose();
            }
            catch (Exception ex) { }
        }
    }

    class RFCConfig
    {
        public RFCConfig()
        {
            Parmeters = new Dictionary<string, RfcConfigParameters>();
        }
        
        public FactoryCode Code { get; set; }
        public Dictionary<string, RfcConfigParameters> Parmeters { get; private set; }
    }
}
