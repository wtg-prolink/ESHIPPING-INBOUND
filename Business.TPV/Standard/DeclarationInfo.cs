using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    [XmlRoot(ElementName = "DeclarationInformation")]
    public class DeclarationInfo : ResponseInfoBase
    {
        [XmlArray(ElementName="Items")]
        public DeclarationItem[] Items { get; set; }
    }

    [XmlType("Item")]
    public class DeclarationItem
    {
        [StringLength(14)]
        [Required]
        [XmlElement]
        public string DNNO { get; set; }
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
        public DateTime? DeclarationDate { get; set; }
        /// <summary>
        /// 报关放行时间
        /// </summary>
        [XmlIgnore]
        public DateTime? DeclarationReleaseDate { get; set; }

        //替代节点
        [XmlElement("DeclarationDate")]
        public string XDeclarationDate
        {
            get
            {
                if (DeclarationDate.HasValue)
                    return DeclarationDate.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                DeclarationDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        //替代节点
        [XmlElement("DeclarationReleaseDate")]
        public string XDeclarationReleaseDate
        {
            get
            {
                if (DeclarationReleaseDate.HasValue)
                    return this.DeclarationReleaseDate.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                DeclarationReleaseDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        /// <summary>
        /// 反馈文档(请将文档转为Base64编码字符写入)
        /// </summary>
        [XmlElement]
        public string FileData { get; set; }
        /// <summary>
        /// 反馈文档扩展名
        /// </summary>
        [StringLength(10)]
        [XmlElement]
        public string FileExtension { get; set; }

        [StringLength(10)]
        [XmlElement]
        public string NextNum { get; set; }
    }
}
