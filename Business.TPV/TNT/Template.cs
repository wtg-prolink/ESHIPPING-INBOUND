using Business.EDI;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.TNT
{
    public abstract class Template : EntityEDITemplate
    {
        protected override System.Xml.XmlDeclaration CreateXmlDeclaration(System.Xml.XmlDocument doc)
        {
            return doc.CreateXmlDeclaration("1.0", "iso-8859-1", "no");
        }

        /// <summary>
        /// 报文唯一标识
        /// </summary>
        [Required]
        [StringLength(20)]
        public string UniqueNumber { get; set; }
    }
}
