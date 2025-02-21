using Business.EDI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.DHL
{
    public class TrackingTemplate : Template
    {
        public override XmlEDINode CreateXmlEDINode()
        {
            XmlEDINode root = new XmlEDINode("req:KnownTrackingRequest");
            var xmlns = "http://www.w3.org/2001/XMLSchema-instance";
            root.Attributes.Add(new XmlAttr("xmlns:req", "http://www.dhl.com"));
            root.Attributes.Add(new XmlAttr("xmlns:xsi", xmlns));
            root.Attributes.Add(new XmlAttr("xsi:schemaLocation", xmlns, "http://www.dhl.com TrackingRequestKnown.xsd"));
            SetRequst(root);
            XmlEDINode node = new XmlEDINode("LanguageCode", "zh");
            root.ChildNodes.Add(node);
            foreach (var item in AWBNumber)
            {
                node = new XmlEDINode("AWBNumber", item);
                root.ChildNodes.Add(node);
            }
            node = new XmlEDINode("LevelOfDetails", LevelOfDetails.ToString());
            root.ChildNodes.Add(node);
            node = new XmlEDINode("PiecesEnabled", PiecesEnabled.ToString());
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CountryCode", "CN");
            root.ChildNodes.Add(node);
            return root;
        }

        public TrackingTemplate()
        {
            AWBNumber = new List<string>();
        }
        /// <summary>
        /// 运单号码(可以有1-10个)
        /// </summary>
        [Required]
        public List<string> AWBNumber { get; set; }
        public LevelOfDetailsModes LevelOfDetails { get; set; }
        public PiecesEnabledModes PiecesEnabled { get; set; }
    }
    public enum LevelOfDetailsModes { LAST_CHECK_POINT_ONLY, ALL_CHECK_POINTS };
    public enum PiecesEnabledModes
    {
        /// <summary>
        /// 只有运单信息及状态
        /// </summary>
        S,
        /// <summary>
        /// 只有PIECE信息及状态
        /// </summary>
        P,
        /// <summary>
        /// 都有
        /// </summary>
        B
    }
}