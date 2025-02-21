using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.LSP
{
    [XmlRoot(ElementName = "DeclarationInformation")]
    public class DeclarationInfo : ImprotInfo
    {
        /// <summary>
        /// 是否确认报关
        /// </summary>
        [StringLength(1)]
        [Required]
        [XmlElement]
        public string ConfirmDeclaration { get; set; }

        /// <summary>
        /// 报关单号
        /// </summary>
        [StringLength(20)]
        [Required]
        [XmlElement]
        public string DeclarationNumber { get; set; }
        /// <summary>
        /// 批准文号
        /// </summary>
        [StringLength(20)]
        [XmlElement]
        public string ApprovalNumber { get; set; }

        /// <summary>
        /// 统一编号
        /// </summary>
        [StringLength(20)]
        [XmlElement]
        public string UnifiedTaxationCode { get; set; }

        /// <summary>
        /// 报关时间
        /// </summary>
        [XmlIgnore]
        public DateTime DeclarationDate { get; set; }
        /// <summary>
        /// 报关放行时间
        /// </summary>
        [XmlIgnore]
        public DateTime DeclarationReleaseDate { get; set; }
        /// <summary>
        /// 报关说明
        /// </summary>
        [XmlElement]
        [StringLength(30)]
        public string Remark { get; set; }

        /// <summary>
        /// 报关单文档(请将文档转为Base64编码字符写入)
        /// </summary>
        [Required]
        [XmlElement]
        public string FileData { get; set; }
        /// <summary>
        /// 报关文档扩展名
        /// </summary>
        [Required]
        [StringLength(10)]
        [XmlElement]
        public string FileExtension { get; set; }

        //替代屬性
        [XmlAttribute("DeclarationDate")]
        public string XDeclarationDate
        {
            get { return this.DeclarationDate.ToString("yyyyMMddHHmm"); }
            set
            {
                DateTime dt;
                if (DateTime.TryParse(value, out dt))
                    this.DeclarationDate = dt;
            }
        }

        //替代屬性
        [XmlAttribute("DeclarationReleaseDate")]
        public string XDeclarationReleaseDate
        {
            get { return this.DeclarationReleaseDate.ToString("yyyyMMddHHmm"); }
            set
            {
                DateTime dt;
                if (DateTime.TryParse(value, out dt))
                    this.DeclarationReleaseDate = dt;
            }
        }
    }
}
