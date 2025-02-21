using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.Standard
{
    public class AirBookingResponse : ShippingBookingResponse
    {
        /// <summary>
        /// 飛機啟運點
        /// </summary>
        [Required]
        [StringLength(5)]
        public string Original { get; set; }
        /// <summary>
        /// 飛機目的地
        /// </summary>
        [Required]
        [StringLength(5)]
        public string DEST { get; set; }
        /// <summary>
        /// 最終目的地
        /// </summary>
        [Required]
        [StringLength(5)]
        public string LastDEST { get; set; }
        /// <summary>
        /// 飛機班次(第一段)
        /// </summary>
        [Required]
        [StringLength(30)]
        public string Flight1 { get; set; }
        /// <summary>
        /// 飛機班次(第二段)
        /// </summary>
        public string Flight2 { get; set; }
        /// <summary>
        /// 飛機班次(第三段)
        /// </summary>
        public string Flight3 { get; set; }
    }
}
