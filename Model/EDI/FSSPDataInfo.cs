using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Models.EDI
{
    public class FSSPDataInfo
    {
        [Required]
        public InteractionInfo INTERACTION_INFO { get; set; }
        [Required]
        public TaskInfo TASK_INFO { get; set; }
        //[Required]
        //public List<BussinessObjectList> BUSINESS_OBJECT_LIST { get; set; }
        [Required]
        public BussinessObject BUSINESS_OBJECT_LIST { get; set; }
        [Required]
        public OperInfo OPER_INFO { get; set; }
        [Required]
        public List<OperHistory> OPER_HISTORY_LIST { get; set; }

        [Required]
        public string LANGUAGE { get; set; }
    }

    public class InteractionInfo
    {
        [StringLength(64)]
        [Required]
        public string INTERACTION_ID { get; set; }
        [StringLength(8)]
        [Required]
        public string PROCESS_TYPE { get; set; }
        [StringLength(32)]
        [Required]
        public string COMPANY_CODE { get; set; }
        [StringLength(32)]
        [Required]
        public string SYSTEM_TAG { get; set; }
    }

    public class TaskInfo
    {
        [StringLength(64)]
        [Required]
        public string CURRENCY { get; set; }
        [StringLength(128)]
        [Required]
        public string PAYMENT_METHOD { get; set; }
        [StringLength(256)]
        public string VENDOR_CODE { get; set; }
        [Required]
        public decimal AMOUNT { get; set; }

        public string TASK_TITLE { get; set; }

        public string CREATE_TIME { get; set; }

        public string CREATE_USER_NAME { get; set; }

        public string CREATE_USER_ID { get; set; }

        public UrgencyFlag URGENCY_FLAG { get; set; }

        public string REMARKS { get; set; }
    }


    public class BussinessObjectList
    {
        public string OBJECT_TYPE { get; set; }

        public string ATTRIBUTE_KEY { get; set; }

        public string ATTRIBUTE_VALUE { get; set; }

        public string REF_OBJECT_TYPE { get; set; }

        public string REF_ATTRIBUTE_KEY { get; set; }

        public string REF_ATTRIBUTE_VALUE { get; set; }

        public string ATTRIBUTE_ID { get; set; }

        public string REF_ATTRIBUTE_ID { get; set; }

        public string ATTR1 { get; set; }

        public string ATTR2 { get; set; }

        public string ATTR3 { get; set; }

    }

    public class BussinessObject
    {
        public Dictionary<string, string> head { get; set; }
        public List<Dictionary<string, string>> details { get; set; }
    }

    public class OperInfo
    {
        public string EXEC_TYPE { get; set; }
        public string EXEC_DATE { get; set; }
        public string EXEC_USER_ID { get; set; }
        public string EXEC_USER_NAME { get; set; }
    }

    public class OperHistory
    {
        public string DEF_NODE_NAME { get; set; }
        public string DONE_INT_ID { get; set; }
        public string EXEC_USER_ID { get; set; }
        public string EXEC_USER_NAME { get; set; }
        public string EXEC_DIM_ID { get; set; }
        public string EXEC_DIM_NAME { get; set; }
        public string EXEC_DATE { get; set; }
        public string EXEC_TYPE { get; set; }
        public string EXEC_DESC { get; set; }
        public string LANGUAGE { get; set; }
    }

    public enum UrgencyFlag { General, Emergency, ExtraUrgent }

    public class result
    {
        public string code { get; set; }
        public string data { get; set; }
        //public FSSPResultData data { get; set; }
        public string message { get; set; }
        public string state { get; set; }
        public string success { get; set; }
    }

    public class fsspResult
    {
        public string code { get; set; }
        public FSSPResultData data { get; set; }
        public string message { get; set; }
        public string state { get; set; }
        public string success { get; set; }
    }

    public class FSSPResultData
    {
        public string DATA { get; set; }
        public string ERROR_INFO { get; set; }
        public string ERROR_TYPE { get; set; }
        public string RETURN_FLAG { get; set; }
        public string rowSize { get; set; }
        public string taskKeyword { get; set; }
    }
}
