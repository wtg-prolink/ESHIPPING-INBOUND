using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    public class OceanDeclaration : OceanBooking
    {
        public PartyInfo BookingAgent { get; set; }

        public string MasterNO { get; set; }
        [Required]
        public string HouseNO { get; set; }
        [Required]
        public string Vessel { get; set; }
        [Required]
        public string Voyage { get; set; }
        [XmlArray]
        public Container[] Containers { get; set; }
       
        public string InvoiceFileUrl { get; set; }
        public string PackingFileUrl { get; set; }
    }

    public class Container
    {
        [Required]
        public string ContainerNO { get; set; }
        [DeclContainerRequired]
        public string SealsNO { get; set; }
        [DeclContainerRequired]
        public string ContainerType { get; set; }
        public string QTY { get; set; }
        public string QTYUnit { get; set; }
        public string CBM { get; set; }
        public string GW { get; set; }
        public string GWUnit { get; set; }
        public string NW { get; set; }
        public string TareGW { get; set; }
    }

    class DeclContainerRequiredAttribute : RequiredAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult result = base.IsValid(value, validationContext);
            if (result != null && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                Container p = validationContext.ObjectInstance as Container;
                if (p != null && validationContext.MemberName != "ContainerNO")
                    result.ErrorMessage = string.Format("Container NO:{0} {1}", p.ContainerNO, result.ErrorMessage);
            }
            return result;
        }
    }
}
