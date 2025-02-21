using Business.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    public class ResponseInfoBase : InfoBase
    {
        /// <summary>
        /// 用途
        /// </summary>
        [StringLength(1)]
        [Required]
        [XmlElement]
        public override string Mode { get; set; }
        /// <summary>
        /// 發送者
        /// </summary>
        [StringLength(10)]
        [Required]
        [XmlElement]
        public override string Sender { get; set; }
        /// <summary>
        /// 接收方
        /// </summary>
        [StringLength(10)]
        [Required]
        [XmlElement]
        public override string RecieveCode { get; set; }
        /// <summary>
        /// 運單號
        /// </summary>
        [Required]
        [StringLength(20)]
        [XmlElement]
        public override string ShipmentID { get; set; }

        [XmlElement]
        [StringLength(300)]
        [RequiredIf("Mode", "C")]
        public override string Remark { get; set; }

        /// <summary>
        /// 报文ID（客户Refrerence Number）
        /// </summary>
        [XmlElement]
        [StringLength(32)]
        public override string UniqueNumber { get; set; }

        /// <summary>
        /// 反馈文档(请将文档转为Base64编码字符写入)
        /// </summary>
        [XmlElement]
        public override string FileData { get; set; }
        /// <summary>
        /// 反馈文档扩展名
        /// </summary>
        [StringLength(10)]
        [XmlElement]
        public override string FileExtension { get; set; }
    }

    public class InfoBase
    {
        public virtual string Sender { get; set; }
        public virtual string RecieveCode { get; set; }
        public virtual string Mode { get; set; }
        public virtual string ShipmentID { get; set; }
        public virtual string Remark { get; set; }
        public virtual string UniqueNumber { get; set; }
        public virtual string FileData { get; set; }
        public virtual string FileExtension { get; set; }
        public virtual string NextNum { get; set; }

        [XmlIgnore]
        public object Data { get; set; }
        [XmlIgnore]
        public List<DataRow> SMRows { get; set; }
    }
}