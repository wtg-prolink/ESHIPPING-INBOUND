using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TrackingEDI.Serviceface
{
    /// <summary>
    /// 问题单:92554   需求：danny  add by fish 2015-4-27
    /// ItraceService 管理接口
    /// </summary>
    public class ItraceServiceface
    {
        private ItraceWebReference.EDI _service = null;
        public static EventHandler ExchrtMessage = null;

        private Dictionary<string,int> _currencyPointList = null;
        private static System.Net.CookieContainer _cookie = new System.Net.CookieContainer();
        public ItraceServiceface()
        {
            _service = new ItraceWebReference.EDI();
            //_service.Url = V6.SmartClient.Context.Server.Host + "/Services/Utils.asmx";
            _service.CookieContainer = _cookie;
        }

        public string Url
        {
            get {
                return _service.Url;
            }
        }

        /// <summary>
        /// 登入
        /// </summary>
        public string Login(string UserID, string UserPSW)
        {
            string xml=_service.EDILogin(UserID, UserPSW);
            XmlDocument xmldoc = new XmlDocument();
            string result = string.Empty; ;
            try
            {
                xmldoc.LoadXml(xml);
                result = GetXmlNodeValue(xmldoc, "WSData", "result");
                if (result != null && "true".Equals(result.ToLower()))
                    return "Y";
                return GetXmlNodeValue(xmldoc, "WSData", "msgData");
            }
            catch (System.Xml.XmlException e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// 登入
        /// </summary>
        public string Login()
        {
            return Login("HARDCORE", "HARDCORE!@#");
        }

        /// <summary>
        /// 發送EDI给IXX 
        /// </summary>
        public string SendToItrace(string strDataXML)
        {
            string xml = _service.SendToItrace(strDataXML);
            XmlDocument xmldoc = new XmlDocument();
            string result = string.Empty; ;
            try
            {
                xmldoc.LoadXml(xml);
                result=GetXmlNodeValue(xmldoc,"WSData", "result");
                if (result != null && "true".Equals(result.ToLower()))
                    return "Y";
                return GetXmlNodeValue(xmldoc, "WSData", "msgData");
            }
            catch (System.Xml.XmlException e)
            {
                return e.Message;
            }
        }

        public static string GetXmlNodeValue(XmlDocument xmldoc, string RootName, string NodeName)
        {
            System.Xml.XmlNode XnNode = xmldoc.SelectSingleNode(string.Format("//{0}/node()[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{1}']", new object[] { RootName, NodeName.ToLower() }));
            if (XnNode != null)
            {
                return XnNode.InnerText;
            }
            else
                return string.Empty;
        }
    }
}
