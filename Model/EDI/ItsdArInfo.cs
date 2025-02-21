using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.EDI
{ 
    public class ItsdArInfo
    {  
        public string module { get; set; }
        public string functional { get; set; }
        public string appDateFrom { get; set; }
        public string appDateTo { get; set; }
        public string appNo { get; set; }
        public int? state { get; set; }
        public string site { get; set; }
    }

    public class ItsdArResult
    {
        public string statusCode { get; set; }
        public string message { get; set; }
        public ItsdArData[] data { get; set; }
    }
    public class ItsdArData
    {
        public ItsdSubData[] Items{ get; set; }
        /// <summary>
        /// 申请单号
        /// </summary>
        public string AppNO { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 申请日期
        /// </summary>
        public string AppDate { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 申请内容
        /// </summary>
        public string AppContent { get; set; }
    } 
     
    public class ItsdSubData
    { 
        /// <summary>
        /// 模块
        /// </summary>
        public string Module { get; set; }
        /// <summary>
        /// 功能分类
        /// </summary>
        public string Functional { get; set; }
        /// <summary>
        /// 登入账号
        /// </summary>
        public string AD { get; set; }
        /// <summary>
        /// 登入账号.名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 登入账号.联系电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 登入账号.分机号码
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// 登入账号.邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 登入账号.主管
        /// </summary>
        public string Manager { get; set; }
        /// <summary>
        /// SAP ID
        /// </summary>
        public string SAPId { get; set; }
        /// <summary>
        /// 登入账号.卡号
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 申请内容
        /// </summary>
        public string Content { get; set; }
    }

    public class EshppingInfo
    { 
        public string EDI_CODE { get; set; }
        public string GROUP_ID { get; set; }
        public string CMP { get; set; }
        public string CREATE_BY { get; set; }
    }

}

