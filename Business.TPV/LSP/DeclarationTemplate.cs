using Business.EDI;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.LSP
{
    /// <summary>
    /// 报关模板
    /// </summary>
    class DeclarationTemplate : Template
    {
        public override EdiTypes EdiType
        {
            get { return EdiTypes.Declaration; }
        }

        public override XmlEDINode CreateXmlEDINode()
        {
            XmlEDINode root = base.CreateXmlEDINode();
            XmlEDINode node = new XmlEDINode("HBL_NO", HouseNO);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("VESSEL", Vessel);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("VOYAGE", Voyage);
            root.ChildNodes.Add(node);
            SetContainers(root);
            return root;
        }

        void SetContainers(XmlEDINode root)
        {
            if (Containers == null || Containers.Count <= 0) return;
            foreach (var item in Containers)
            {
                XmlEDINode node = new XmlEDINode("CONTAINER_INFO");
                SetContainer(node, item);
                root.ChildNodes.Add(node);
            }
        }

        void SetContainer(XmlEDINode mNode, ContainerInfo info)
        {
            XmlEDINode node = new XmlEDINode("CONTAINER_NO", info.ContainerNO);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("SEALS_NO", info.SealsNO);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CONTAINER_TYPE", info.ContainerType);
            mNode.ChildNodes.Add(node);
        }

        protected override void SetDNInfo(XmlEDINode dnDetail, DNInfo info)
        {
            XmlEDINode node = new XmlEDINode("DN_NO", info.DNNO);
            dnDetail.ChildNodes.Add(node);
            node = new XmlEDINode("EDI_NO", info.UniCode);
            dnDetail.ChildNodes.Add(node);
            node = new XmlEDINode("ASD_NO", info.ExportNO);
            dnDetail.ChildNodes.Add(node);
        }

        public override bool Check(out EntityValidationResult result)
        {
            base.Check(out result);
            if (result.HasError) return false;
            foreach (var item in DNInfos)
            {
                result = ValidationHelper.ValidateEntity(item);
                if (result.HasError) return false;
            }
            if (Containers != null && Containers.Count > 0)
            {
                foreach (var c in Containers)
                {
                    result = ValidationHelper.ValidateEntity(c);
                    if (result.HasError) return false;

                }
            }
            return true;
        }

        public List<ContainerInfo> Containers { get; set; }
        [Required]
        public string HouseNO { get; set; }
        [Required]
        public string Vessel { get; set; }
        [Required]
        public string Voyage { get; set; }
    }

    class DeclDNRequiredAttribute : RequiredAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult result = base.IsValid(value, validationContext);
            if (result != null && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                DNInfo p = validationContext.ObjectInstance as DNInfo;
                if (p != null)
                    result.ErrorMessage = string.Format("DN NO:{0} {1}", p.DNNO, result.ErrorMessage);
            }
            return result;
        }
    }
    class DeclContainerRequiredAttribute : RequiredAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult result = base.IsValid(value, validationContext);
            if (result != null && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                ContainerInfo p = validationContext.ObjectInstance as ContainerInfo;
                if (p != null&&validationContext.MemberName!="ContainerNO")
                    result.ErrorMessage = string.Format("Container NO:{0} {1}", p.ContainerNO, result.ErrorMessage);
            }
            return result;
        }
    }

    class ContainerInfo
    {
        [Required]
        public string ContainerNO { get; set; }
        [DeclContainerRequired]
        public string SealsNO { get; set; }
        [DeclContainerRequired]
        public string ContainerType { get; set; }
    }
}