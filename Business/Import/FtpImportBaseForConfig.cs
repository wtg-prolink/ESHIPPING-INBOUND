using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.Import
{
    public abstract class FtpImportBaseForConfig : FtpImportBase
    {
        protected abstract string ConfigFileName { get; }
        protected abstract string ConfigNodeName { get; }

        protected override FTPConfig GetFtpConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigFileName);
            XmlNodeList nodeList = doc.GetElementsByTagName(ConfigNodeName);
            return Business.Utils.ConfigManager.GetFTPConfig(nodeList).First();
        }
    }
}
