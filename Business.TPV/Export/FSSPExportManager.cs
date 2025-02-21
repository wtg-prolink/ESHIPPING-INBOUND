using Business.TPV.Web;
using Models.EDI;
using Newtonsoft.Json;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Export
{
    class InvoiceCheckManager : SendToFSSPManager<FSSPDataInfo>
    {
        protected override string OperateData(FSSPDataInfo info, Runtime runtime, EDIConfig config)
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            json = JsonConvert.SerializeObject(info, jSetting);
            try
            {
                returnJson = HttpPost(config.Server, config.User, config.Psw, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            result result = new result();
            try
            {
                result = JsonConvert.DeserializeObject<result>(returnJson);
            }
            catch
            {
                result.success = "false";
                result.message = "JSON解析异常";
            }

            if (result.success.ToUpper() != "TRUE")
                throw new Exception(result.message);

            return result.code + ":" + result.message;
        }

        protected override FSSPDataInfo CreateEdiInfos(Runtime runtime)
        {
            FSSPDataInfo template = new FSSPDataInfo();
            DataTable dt = GetSMBIDDataTable(runtime.RefNo);
            if (dt.Rows.Count <= 0)
                return null;
            FillInteractionInfo(dt, template);
            FillTaskInfo(dt, template);
            template.LANGUAGE = "en";
            return template;
        }

        DataTable GetSMBIDDataTable(string InvoiceNo)
        {
            string sql = string.Format("SELECT * FROM SMBIM WHERE TPV_DEBIT_NO = {0}", SQLUtils.QuotedStr(InvoiceNo));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            return dt;
        }
        void FillInteractionInfo(DataTable dt, FSSPDataInfo template)
        {
            InteractionInfo temp = new InteractionInfo();
            temp.COMPANY_CODE = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
            temp.INTERACTION_ID = Prolink.Math.GetValueAsString(dt.Rows[0]["TPV_DEBIT_NO"]);
            temp.PROCESS_TYPE = "SSC-AP-14";
            temp.SYSTEM_TAG = "ESP";
            template.INTERACTION_INFO = temp;
        }
        void FillTaskInfo(DataTable dt, FSSPDataInfo template)
        {
            TaskInfo temp = new TaskInfo();
            temp.CURRENCY = Prolink.Math.GetValueAsString(dt.Rows[0]["CUR"]);
            temp.PAYMENT_METHOD = Prolink.Math.GetValueAsString(dt.Rows[0]["DEBIT_TYPE"]);
            //temp.PRICE_TAX_AMOUNT = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["AMT"]);
            temp.VENDOR_CODE = Prolink.Math.GetValueAsString(dt.Rows[0]["RECEIVER"]);
            temp.AMOUNT = Prolink.Math.GetValueAsDecimal(dt.Rows[0]["AMT"]);
            template.TASK_INFO = temp;
        }
    }

    class ViodINVFSSPManager : SendToFSSPManager<FSSPDataInfo>
    {
        protected override string OperateData(FSSPDataInfo info, Runtime runtime, EDIConfig config)
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            json = JsonConvert.SerializeObject(info, jSetting);
            try
            {
                returnJson = HttpPost(config.Server, config.User, config.Psw, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            fsspResult result = new fsspResult();
            try
            {
                result = JsonConvert.DeserializeObject<fsspResult>(returnJson);
            }
            catch
            {
                result.success = "false";
                result.message = "JSON解析异常";
            }
            if (result.success.ToUpper() != "TRUE")
                throw new Exception(result.message);

            return result.message;
        }

        protected override FSSPDataInfo CreateEdiInfos(Runtime runtime)
        {
            FSSPDataInfo dataInfo = new FSSPDataInfo();
            InteractionInfo interactionInfo = new InteractionInfo();
            interactionInfo.PROCESS_TYPE = "SSC-AP-14";
            interactionInfo.COMPANY_CODE = runtime.Data.ToString();
            interactionInfo.SYSTEM_TAG = "ESP";
            interactionInfo.INTERACTION_ID = runtime.RefNo;
            dataInfo.INTERACTION_INFO = interactionInfo;
            OperInfo operInfo = new OperInfo();
            operInfo.EXEC_TYPE = "19";
            operInfo.EXEC_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            operInfo.EXEC_USER_ID = runtime.OPUser;
            operInfo.EXEC_USER_NAME = runtime.OPUser;
            dataInfo.OPER_INFO = operInfo;
            return dataInfo;
        }
    }

    class ViodINVIMManager : SendToFSSPManager<InteractionInfo>
    {
        protected override string OperateData(InteractionInfo info, Runtime runtime, EDIConfig config)
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            json = JsonConvert.SerializeObject(info, jSetting);
            try
            {
                returnJson = HttpPost(config.Server, config.User, config.Psw, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            result result = new result();
            try
            {
                result = JsonConvert.DeserializeObject<result>(returnJson);
            }
            catch
            {
                result.success = "false";
                result.message = "JSON解析异常";
            }

            if (result.success.ToUpper() != "TRUE")
                throw new Exception(result.message);

            return result.message;
        }

        protected override InteractionInfo CreateEdiInfos(Runtime runtime)
        {
            InteractionInfo interaction = new InteractionInfo();
            interaction.PROCESS_TYPE = "SSC-AP-14";//请款是SSC-AP-14，预提是SSC-ER-07
            interaction.COMPANY_CODE = runtime.Data.ToString();//账单号码对应的cmp
            interaction.SYSTEM_TAG = "ESP";//固定ESP
            interaction.INTERACTION_ID = runtime.RefNo;//账单号码
            return interaction;
        }

    }

    class SplitBIDFSSPManage : SendToFSSPManager<FSSPDataInfo>
    {
        protected override string OperateData(FSSPDataInfo info, Runtime runtime, EDIConfig config)
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            json = JsonConvert.SerializeObject(info, jSetting);
            //config.Server = "https://fssp-qas.tpv-tech.com/fssp-services/fssp/bpm/processTaskReceiveNew.do";
            try
            {
                returnJson = HttpPost(config.Server, config.User, config.Psw, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            fsspResult result = new fsspResult();
            try
            {
                result = JsonConvert.DeserializeObject<fsspResult>(returnJson);
                if (result != null && result.data != null)
                    UpdateSMBIM(runtime.RefNo, result.data.DATA);
            }
            catch
            {
                result.success = "false";
                result.message = "JSON解析异常";
            }

            if (result.success.ToUpper() != "TRUE")
                throw new Exception(result.message);

            return result.data.DATA + " " + result.message;
        }

        protected override FSSPDataInfo CreateEdiInfos(Runtime runtime)
        {
            FSSPDataInfo template = new FSSPDataInfo();
            DataTable dt = GetSMBIDDNDataTable(runtime.Data.ToString());
            DataTable hDt = GetSMBIMDataTable(runtime.Data.ToString());
            if (dt.Rows.Count <= 0 || hDt.Rows.Count <= 0)
                return null;
            DataTable approveDt = GetApproveDataTable(runtime.Data.ToString(), hDt);
            FillInteractionInfo(hDt, template, runtime.RefNo);
            FillTaskInfo(template);
            FillBussinessObjectListNew(dt, hDt, template);
            FillOperHistoryList(approveDt, template);
            return template;
        }

        private void UpdateSMBIM(string refNo, string ReimburseNo)
        {
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("TPV_DEBIT_NO", refNo);
            ei.Put("REIMBURSE_NO", ReimburseNo);
            ei.PutDate("APPLY_DATE", DateTime.Now);
            int[] result = OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        void FillInteractionInfo(DataTable dt, FSSPDataInfo template, string refNo)
        {
            InteractionInfo interactionInfo = new InteractionInfo();
            interactionInfo.PROCESS_TYPE = "SSC-AP-14";//请款是SSC-AP-14，预提是SSC-ER-07
            interactionInfo.COMPANY_CODE = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]);
            interactionInfo.SYSTEM_TAG = "ESP";//固定ESP
            interactionInfo.INTERACTION_ID = refNo;
            template.INTERACTION_INFO = interactionInfo;
        }
        void FillTaskInfo(FSSPDataInfo template)
        {
            TaskInfo taskInfo = new TaskInfo();
            taskInfo.TASK_TITLE = "Sub-FSSP-ESP-001";
            taskInfo.CREATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            taskInfo.CREATE_USER_NAME = "Eshipping";
            taskInfo.URGENCY_FLAG = UrgencyFlag.General;
            template.TASK_INFO = taskInfo;
        }

        DataTable GetSMBIDDNDataTable(string billUid)
        {
            string sql = string.Format("SELECT * FROM SMBID_DN WHERE U_FID={0}", SQLUtils.QuotedStr(billUid));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            return dt;
        }

        DataTable GetSMBIMDataTable(string billUid)
        {
            string sql = string.Format("SELECT TPV_DEBIT_NO AS DEBIT_NO,CONTRACT_NO,BANK_TYPE,COLLECT_BANK,ACCOUNT_NAME,BANK_INFO,SWIFT_CODE,DEBIT_NO,PAY_DATE,DEBIT_TYPE,STN,APPROVE_TYPE,CMP,REMARK_S,REMARK,PAY_START_DATE,PAY_TERM FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(billUid));
            return DB.GetDataTable(sql, new string[] { });
        }

        DataTable GetApproveDataTable(string billUid, DataTable hDt)
        {
            string approveCode = Prolink.Math.GetValueAsString(hDt.Rows[0]["APPROVE_TYPE"]);
            string cmp = Prolink.Math.GetValueAsString(hDt.Rows[0]["CMP"]);
            string sql = string.Format(@"SELECT APPROVE_BY,APPROVE_LEVEL,APPROVE_DATE,REMARK,APPROVE_REMARK,
ISNULL((SELECT TOP 1 GROUP_DESCP FROM APPROVE_FLOW_D WHERE APPROVE_FLOW_D.APPROVE_CODE=APPROVE_RECORD.APPROVE_CODE AND APPROVE_FLOW_D.ROLE= APPROVE_RECORD.ROLE AND APPROVE_FLOW_D.CMP_ID={2}),APPROVE_RECORD.ROLE) AS APPROVE_ROLE
FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} ORDER BY APPROVE_LEVEL",
SQLUtils.QuotedStr(billUid), SQLUtils.QuotedStr(approveCode), SQLUtils.QuotedStr(cmp));
            return DB.GetDataTable(sql, new string[] { });
        }

        DataTable GetUserDataTable(string userId)
        {
            string sql = string.Format(@"SELECT (SELECT TOP 1 CD_DESCP FROM BSCODE WHERE CD='DE' AND CD_TYPE=SYS_ACCT.DEP) AS DEP_DESCP,
DEP,CARD_NO FROM SYS_ACCT WHERE U_ID={0} AND CARD_NO IS NOT NULL", SQLUtils.QuotedStr(userId));
            return DB.GetDataTable(sql, new string[] { });
        }

        void FillBussinessObjectListNew(DataTable dt, DataTable hDt, FSSPDataInfo template)
        {
            BussinessObject bussinessObject = new BussinessObject();
            Dictionary<string, string> headDic = new Dictionary<string, string>();
            string chgDescp = "";
            foreach (DataRow hdr in hDt.Rows)
            {
                if (!headDic.ContainsKey("id"))
                    headDic.Add("id", template.INTERACTION_INFO.INTERACTION_ID);
                foreach (DataColumn col in hdr.Table.Columns)
                {
                    string colName = col.ColumnName;
                    string value = GetValueAsString(hdr[colName]);
                    switch (colName.ToUpper())
                    {
                        case "STN":
                        case "APPROVE_TYPE":
                        case "CMP":
                            break;
                        case "REMARK":
                            chgDescp = value;
                            break;
                        case "REMARK_S":
                            template.TASK_INFO.REMARKS = value;
                            if (!headDic.ContainsKey(colName))
                                headDic.Add(colName, value);
                            break;
                        default:
                            //string colName = ConvertToPascalCase(col.ColumnName);
                            if (!headDic.ContainsKey(colName))
                                headDic.Add(colName, value);
                            break;
                    }

                }
            }
            bussinessObject.head = headDic;
            List<Dictionary<string, string>> detailList = new List<Dictionary<string, string>>();

            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, string> detail = new Dictionary<string, string>();
                foreach (DataColumn col in dr.Table.Columns)
                {
                    string colName = col.ColumnName;
                    string uid = GetValueAsString(dr["U_ID"]);
                    if (!detail.ContainsKey("id"))
                        detail.Add("id", uid);
                    string attrVal = GetValueAsString(dr[colName]);
                    switch (colName.ToUpper())
                    {
                        case "DEBIT_NO":
                            attrVal = "tpvDebitNo";
                            break;
                        case "DN_NO":
                            int dnLength = attrVal.Length;
                            if (dnLength > 8)
                                attrVal = attrVal.Substring(dnLength - 8, 8);
                            break;
                        case "COST_CENTER":
                            attrVal = attrVal.ToUpper();
                            break;
                        case "CHG_TYPE":
                            attrVal = chgDescp;
                            break;
                        case "EAMT":
                        case "ETAX":
                        case "ECUR":
                        case "PRE_NAAMT":
                        case "PRE_AAMT":
                        case "PRD_SUM":
                        case "NEAMT":
                        case "ELEVEL":
                        case "ETAMT":
                            attrVal = "";
                            break;
                    }
                    //string colName = ConvertToPascalCase(col.ColumnName);
                    if (!detail.ContainsKey(colName))
                        detail.Add(colName, attrVal);
                }
                detailList.Add(detail);
            }

            bussinessObject.details = detailList;
            template.BUSINESS_OBJECT_LIST = bussinessObject;
        }

        void FillOperHistoryList(DataTable approveDt, FSSPDataInfo template)
        {
            List<OperHistory> operHistoryList = new List<OperHistory>();
            int i = 1;
            Dictionary<string, string> dic = new Dictionary<string, string>() { { "Start", "1" }, { "发起", "1" }, { "發起", "1" },
                { "Approve", "2" }, { "通过", "2" }, { "通過", "2" } };
            foreach (DataRow dr in approveDt.Rows)
            {
                OperHistory operHistory = new OperHistory();
                operHistory.LANGUAGE = "en";
                string approveRole = Prolink.Math.GetValueAsString(dr["APPROVE_ROLE"]);
                string approveBy = Prolink.Math.GetValueAsString(dr["APPROVE_BY"]);
                DateTime approveDate = Prolink.Math.GetValueAsDateTime(dr["APPROVE_DATE"]);
                string remark = Prolink.Math.GetValueAsString(dr["REMARK"]);
                string approveRemark = Prolink.Math.GetValueAsString(dr["APPROVE_REMARK"]);
                string execDate = approveDate > DateTime.MinValue ? approveDate.ToString("yyyy-MM-dd HH:mm:ss") : "";
                operHistory.DEF_NODE_NAME = approveRole;
                operHistory.DONE_INT_ID = i.ToString();
                operHistory.EXEC_DATE = execDate;
                operHistory.EXEC_DESC = approveRemark;
                operHistory.EXEC_TYPE = dic.ContainsKey(remark) ? dic[remark] : remark;
                if (string.IsNullOrEmpty(remark))
                    continue;
                DataTable userDt = GetUserDataTable(approveBy);
                if (userDt.Rows.Count > 0)
                {
                    operHistory.EXEC_USER_ID = Prolink.Math.GetValueAsString(userDt.Rows[0]["CARD_NO"]);
                    operHistory.EXEC_USER_NAME = approveBy;
                    operHistory.EXEC_DIM_ID = Prolink.Math.GetValueAsString(userDt.Rows[0]["DEP"]);
                    operHistory.EXEC_DIM_NAME = Prolink.Math.GetValueAsString(userDt.Rows[0]["DEP_DESCP"]);
                    if (i == 1)
                    {
                        template.TASK_INFO.CREATE_USER_NAME = approveBy;
                        template.TASK_INFO.CREATE_USER_ID = operHistory.EXEC_USER_ID;
                    }
                }
                operHistoryList.Add(operHistory);
                i++;

            }
            template.OPER_HISTORY_LIST = operHistoryList;
        }

    }
}
