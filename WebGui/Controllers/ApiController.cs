using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrackingEDI.Business;
using WebGui.App_Start;
using Business;
using System.Web.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Web.Script.Serialization;
using Models.EDI;
using Models.EDI;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using System.Security.Cryptography;

namespace WebGui.Controllers
{
    /// <summary>
    /// app调用的相关api
    /// 预约=》入厂=>封柜=?出厂
    /// </summary>
    public class ApiController : BaseController
    {
        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns></returns>
        public ActionResult Version()
        {
            string v = Request["v"];
            if (string.IsNullOrEmpty(v))
                v = string.Empty;
            else
                v = HttpUtility.UrlDecode(v);
            string app_v = Prolink.Web.WebContext.GetInstance().GetProperty("app_v");
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["v"] = app_v;
            result["url"] = string.Empty;
            if (!v.Equals(app_v))
            {
                result["url"] = Prolink.Web.WebContext.GetInstance().GetProperty("app_url");
            }
            return ToContent(result);
        }

        /// <summary>
        /// 仓库系统  检查版本号
        /// </summary>
        /// <returns></returns>
        public ActionResult Version1()
        {
            string v = Request["v"];
            if (string.IsNullOrEmpty(v))
                v = string.Empty;
            else
                v = HttpUtility.UrlDecode(v);
            string app_v = Prolink.Web.WebContext.GetInstance().GetProperty("app_v1");
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["v"] = app_v;
            result["url"] = string.Empty;
            if (!string.IsNullOrEmpty(app_v) && !v.Equals(app_v))
            {
                result["url"] = Prolink.Web.WebContext.GetInstance().GetProperty("app_url1");
            }
            return ToContent(result);
        }

        /// <summary>
        /// 进口仓库版本验证
        /// </summary>
        /// <returns></returns>
        public ActionResult VersionIW()
        {
            string v = Request["v"];
            if (string.IsNullOrEmpty(v))
                v = string.Empty;
            else
                v = HttpUtility.UrlDecode(v);
            string app_v = Prolink.Web.WebContext.GetInstance().GetProperty("app_iw_v");
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["v"] = app_v;
            result["url"] = string.Empty;
            if (!string.IsNullOrEmpty(app_v) && !v.Equals(app_v))
            {
                result["url"] = Prolink.Web.WebContext.GetInstance().GetProperty("app_iw_url");
            }
            return ToContent(result);
        }

        /// <summary>
        /// tracking app
        /// </summary>
        /// <returns></returns>
        public ActionResult Version2()
        {
            string v = Request["v"];
            if (string.IsNullOrEmpty(v))
                v = string.Empty;
            else
                v = HttpUtility.UrlDecode(v);
            string app_v = Prolink.Web.WebContext.GetInstance().GetProperty("app_v2");
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["v"] = app_v;
            result["url"] = string.Empty;
            if (!string.IsNullOrEmpty(app_v) && !v.Equals(app_v))
            {
                result["url"] = Prolink.Web.WebContext.GetInstance().GetProperty("app_url2");
            }
            return ToContent(result);
        }

        #region 数据查询
        /// <summary>
        /// 进场查询
        /// </summary>
        /// <returns></returns>
        public ActionResult InFactory()
        {
            return Gate("I");
        }

        /// <summary>
        /// 获取所有的dn号码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDnGateByDnno()
        {
            string status = "'I','P','C','R','G'";
            string dnNo = Request["dnNo"];
            if (!string.IsNullOrEmpty(dnNo)) dnNo = HttpUtility.UrlDecode(dnNo);

            string cols = string.Empty;
            cols += "(SELECT TOP 1 ISNULL(SMSM.POD_NAME,'')+'('+ISNULL(SMSM.POD_CD,'')+')' FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS POD,";
            cols += "(SELECT TOP 1 ISNULL(SMSM.DEST_NAME,'')+'('+ISNULL(SMSM.DEST_CD,'')+')' FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS DEST,";
            cols += "(SELECT TOP 1 SMSMPT.CNTY_NM FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMIRV.SHIPMENT_ID  AND SMSMPT.PARTY_TYPE='WE') AS DEST_CD,";

            string sql = string.Format("SELECT " + cols + "* FROM SMIRV WHERE GROUP_ID={0} AND CMP={1} AND STATUS IN ({2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), status);
            if (!string.IsNullOrEmpty(dnNo))
                sql += " AND DN_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
            else
                sql += " AND DN_NO = " + SQLUtils.QuotedStr(dnNo);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
        }

        /// <summary>
        /// 查询可封柜的
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPGateByDnno()
        {
            string status = "'I','G'";
            string dnNo = Request["dnNo"];
            if (!string.IsNullOrEmpty(dnNo)) dnNo = HttpUtility.UrlDecode(dnNo);
            string cols = string.Empty;
            cols += "(SELECT TOP 1 ISNULL(SMSM.POD_NAME,'')+'('+ISNULL(SMSM.POD_CD,'')+')' FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS POD,";
            cols += "(SELECT TOP 1 ISNULL(SMSM.DEST_NAME,'')+'('+ISNULL(SMSM.DEST_CD,'')+')' FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS DEST,";
            cols += "(SELECT TOP 1 SMSMPT.CNTY_NM FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='WE') AS DEST_CD,";

            string sql = string.Format("SELECT " + cols + "* FROM SMIRV WHERE GROUP_ID={0} AND CMP={1} AND STATUS IN ({2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), status);
            if (!string.IsNullOrEmpty(dnNo))
                sql += " AND DN_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
            else
                sql += " AND DN_NO = " + SQLUtils.QuotedStr(dnNo);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
        }

        public ActionResult GetInOutGate()
        {
            string status = "'E','P','C','R','G'";
            string condition = string.Empty;
            string tablecondi = GetBookingCondition();
            tablecondi += " AND (RV_TYPE<>'I' OR RV_TYPE IS NULL)";
            string table = string.Format("(SELECT * FROM SMIRV WHERE {0} AND STATUS IN ({1})", tablecondi, status);

            table += string.Format(" UNION SELECT * FROM SMIRV WHERE {0} AND STATUS='I' AND TRAN_TYPE IN ('D','E'))T", tablecondi);
            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            //int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            //ExchangeDT(dt, mode);
            return GetBootstrapData(table, condition);
        }

        public ActionResult GetInOutGateForIb()
        {
            string status = "'E','P','C','R','G','U','I','O','A','Z','M'";
            string sql = string.Format("SELECT WHS FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            string Wh = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SYS_ACCT_WH WHERE USER_ID={0} AND U_CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            DataTable userWhDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string condition = Wh.TrimEnd(';').Replace(";", "','");
            Dictionary<string, string> whDic = new Dictionary<string, string>();
            whDic.Add(CompanyId, condition);
            foreach (DataRow dr in userWhDt.Rows)
            {
                string wsCd = Prolink.Math.GetValueAsString(dr["WS_CD"]);
                string wsCmp = Prolink.Math.GetValueAsString(dr["WS_CMP"]);
                if (whDic.ContainsKey(wsCmp))
                {
                    condition = whDic[wsCmp];
                    if (!string.IsNullOrEmpty(condition) && !condition.Contains(wsCd))
                    {
                        condition += "','" + wsCd;
                    }
                    else if (string.IsNullOrEmpty(condition))
                    {
                        condition += wsCd;
                    }
                }
                else
                {
                    condition = wsCd;
                    whDic.Add(wsCmp, wsCd);
                }
                whDic[wsCmp] = condition;
            }
            string wsCondition = "";
            foreach (string key in whDic.Keys)
            {
                if (!string.IsNullOrEmpty(wsCondition))
                    wsCondition += " OR ";
                wsCondition += string.Format("(WS_CD IN ('{0}',NULL,'') AND CMP='{1}')", whDic[key], key);
            }
            string tablecondi = GetBookingCondition(true);
            tablecondi += " AND RV_TYPE='I' ";
            string table = string.Format(@"(SELECT SMIRV.*
                FROM SMIRV WHERE {0}  AND CALL_TYPE !='S' AND ((STATUS IN ({1})
                AND EXISTS(SELECT 1 FROM (SELECT ADDR_CODE FROM SMRCNTR WHERE SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO AND ({2})
                UNION SELECT ADDR_CODE FROM SMRDN WHERE SMRDN.RESERVE_NO=SMIRV.RESERVE_NO AND ({2})) T )) OR (STATUS='I' AND TRAN_TYPE IN ('D','E'))))T",
                tablecondi, status, wsCondition);

            return GetBootstrapData(table, "");
        }

        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            if (resultType == "excel")
                return ExportExcelFile(dt);

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        /// <summary>
        /// 查询可进厂出厂的
        /// </summary>
        /// <returns></returns>
        public ActionResult GetInOutGateByDnno()
        {
            string status = "'A','E','P','C','R','G','U'";
            string dnNo = Request["dnNo"];
            string ws_cd = Request["ws_cd"];
            if (!string.IsNullOrEmpty(ws_cd)) ws_cd = HttpUtility.UrlDecode(ws_cd);

            if (!string.IsNullOrEmpty(dnNo)) dnNo = HttpUtility.UrlDecode(dnNo);


            string sql = string.Format("SELECT (SELECT TOP 1 SMSM.RELEASE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND RELEASE_NO IS NOT NULL) AS RELEASE_NO,* FROM SMIRV WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN ({2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), status);
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(dnNo))
            {
                condition += " AND (DN_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                condition += " OR RESERVE_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                condition += " OR BAT_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                condition += " OR EXISTS (SELECT 1 FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND RELEASE_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%") + ")";
                condition += " OR CNTR_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                condition += " OR TRUCK_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                condition += " OR LTRUCK_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%") + ")";
            }
            else
                condition += " AND DN_NO = " + SQLUtils.QuotedStr(dnNo);
            sql += condition;

            sql += string.Format(" UNION SELECT (SELECT TOP 1 SMSM.RELEASE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND RELEASE_NO IS NOT NULL) AS RELEASE_NO,* FROM SMIRV WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS='I' AND TRAN_TYPE IN ('D','E')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            sql += condition;

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            ExchangeDT(dt, mode);
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
        }

        /// <summary>
        /// 数据交换
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="mode"></param>
        private static void ExchangeDT(DataTable dt, int mode)
        {
            if (mode == 1)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string truckNo = Prolink.Math.GetValueAsString(dr["TRUCK_NO"]);
                    string driver = Prolink.Math.GetValueAsString(dr["DRIVER"]);
                    string tel = Prolink.Math.GetValueAsString(dr["TEL"]);
                    dr["TRUCK_NO"] = dr["LTRUCK_NO"];
                    dr["DRIVER"] = dr["LDRIVER"];
                    dr["TEL"] = dr["LTEL"];
                    dr["DRIVER_ID"] = dr["LDRIVER_ID"];

                    dr["LTRUCK_NO"] = truckNo;
                    dr["LDRIVER"] = driver;
                    dr["LTEL"] = tel;
                }
            }
        }

        /// <summary>
        /// 获取封柜
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPDnGateByDnno()
        {
            string status = "P";
            string dnNo = Request["dnNo"];
            if (!string.IsNullOrEmpty(dnNo)) dnNo = HttpUtility.UrlDecode(dnNo);
            string sql = string.Format("SELECT * FROM SMIRV WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN {2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(status));
            if (!string.IsNullOrEmpty(dnNo))
            {
                sql += " AND (DN_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                sql += " OR RESERVE_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                sql += " OR CNTR_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                sql += " OR TRUCK_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                sql += " OR LTRUCK_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%") + ")";
            }
            else
                sql += " AND DN_NO=" + SQLUtils.QuotedStr(dnNo);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
        }

        /// <summary>
        /// 出厂查询
        /// </summary>
        /// <returns></returns>
        public ActionResult OutFactory()
        {
            return Gate("O");
        }

        /// <summary>
        /// 预约查询
        /// </summary>
        /// <returns></returns>
        public ActionResult RFactory()
        {
            string sql = string.Format("SELECT (SELECT TOP 1 SMSM.RELEASE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND RELEASE_NO IS NOT NULL) AS RELEASE_NO,* FROM SMIRV WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN ({2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), "'R','E','A'");

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            ExchangeDT(dt, mode);

            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
            //return Gate("R");
        }

        /// <summary>
        /// 预约确认查询
        /// </summary>
        /// <returns></returns>
        public ActionResult CFactory()
        {
            string sql = string.Format("SELECT (SELECT TOP 1 SMSM.RELEASE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND RELEASE_NO IS NOT NULL) AS RELEASE_NO,* FROM SMIRV WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN ({2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), "'A','C','E'");
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            ExchangeDT(dt, mode);
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
            //return Gate("C");
        }

        /// <summary>
        /// 封柜查询  查出来的数据要进行出厂动作   G: In Gate   入月台
        /// </summary>
        /// <returns></returns>
        public ActionResult PGFactory()
        {
            string sql = string.Format("SELECT * FROM SMIRV WHERE GROUP_ID={0} AND CMP={1} AND STATUS IN ({2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), "'G','P','U'");
            sql += string.Format(" UNION SELECT * FROM SMIRV WHERE GROUP_ID={0} AND CMP={1} AND STATUS='I' AND TRAN_TYPE IN ('D','E')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            ExchangeDT(dt, mode);
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
            //return Gate("P");
        }

        /// <summary>
        /// 进厂查询  查出来的数据要进行封柜动作   G: In Gate   入月台
        /// </summary>
        /// <returns></returns>
        public ActionResult IGFactory()
        {
            string cols = string.Empty;
            cols += "(SELECT TOP 1 ISNULL(SMSM.POD_NAME,'')+'('+ISNULL(SMSM.POD_CD,'')+')' FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS POD,";
            cols += "(SELECT TOP 1 ISNULL(SMSM.DEST_NAME,'')+'('+ISNULL(SMSM.DEST_CD,'')+')' FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS DEST,";
            cols += "(SELECT TOP 1 SMSMPT.CNTY_NM FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='WE') AS DEST_CD,";

            string sql = string.Format("SELECT " + cols + "* FROM SMIRV WHERE GROUP_ID={0} AND CMP={1} AND STATUS IN ({2})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), "'G','I'");
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
            //return Gate("P");
        }

        public ActionResult PFactory()
        {
            return Gate("P");
        }
        #endregion

        #region 确认操作
        /// <summary>
        /// 预约确认进厂
        /// </summary>
        /// <returns></returns>
        public ActionResult RFactoryConfirm()
        {
            return UpdateGate(Request["id"], "R", Request["wh"]);
        }

        /// <summary>
        /// 预约进厂
        /// </summary>
        /// <returns></returns>
        public ActionResult CFactoryConfirm()
        {
            return UpdateGate(Request["id"], "C", Request["wh"]);
        }
        /// <summary>
        /// DN封柜确认
        /// </summary>
        /// <returns></returns>
        public ActionResult InFactoryConfirmDN()
        {
            string job_no = Request["id"];//Approve STATUS =Finish
            DataTable smrv = GetSMRVData(job_no);
            string DnList = Prolink.Math.GetValueAsString(Request["dnlist"]);
            string bat_no = string.Empty;
            string status = string.Empty;
            DateTime? putdate = null;
            if (smrv.Rows.Count > 0)
            {
                bat_no = Prolink.Math.GetValueAsString(smrv.Rows[0]["BAT_NO"]);
                status = Prolink.Math.GetValueAsString(smrv.Rows[0]["STATUS"]);
                if (smrv.Rows[0]["PUT_DATE"] != null && smrv.Rows[0]["PUT_DATE"] != DBNull.Value)
                    putdate = Prolink.Math.GetValueAsDateTime(smrv.Rows[0]["PUT_DATE"]);
            }
            string returnMsg = Business.SMHandle.QAHoldBlMessage(bat_no, "QAH", true);
            if (!string.IsNullOrEmpty(returnMsg))
                return ToMessage(returnMsg, false);
            List<string> msg = new List<string>();

            if (string.IsNullOrEmpty(status))
                status = "I";
            if ("O".Equals(status))
                return ToMessage(@Resources.Locale.L_Api_Controllers_82, false);
            if ("P".Equals(status))
                return ToMessage(@Resources.Locale.L_SmrvSetup_Scripts_294, false);
            if (putdate != null)
            {
                if (putdate.Value.CompareTo(DateTime.Now) > 0)
                {
                    return ToMessage(@Resources.Locale.L_PutTime_Tip, false);
                }
            }

            //bool approve_to = IsApproveFinish(job_no, bat_no, msg);
            //bool qtyIsEqual = true;

            List<string> oth_msg = new List<string>();
            bool qtyIsEqual = QtyIsEquals(job_no, bat_no, oth_msg);

            string[] list = DnList.Split(';');
            string approveSQL = string.Format("SELECT DN_NO,APPROVE_TO from SMDN WHERE DN_NO IN {0}", SQLUtils.Quoted(list));
            DataTable approveDt = OperationUtils.GetDataTable(approveSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow at in approveDt.Rows)
            {
                string approve_to0 = Prolink.Math.GetValueAsString(at["APPROVE_TO"]);
                if (string.IsNullOrEmpty(approve_to0) || !"finish".Equals(approve_to0.ToLower()))
                {
                    return ToMessage(string.Format(@Resources.Locale.L_Api_Controllers_132, Prolink.Math.GetValueAsString(at["DN_NO"])), false);
                }
            }

            if (!string.IsNullOrEmpty(DnList))
            {
                MixedList ml = new MixedList();
                foreach (string dn in list)
                {
                    EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("DN_NO", dn);
                    ei.Put("SEAL_SATAUE", "Y");
                    ei.Put("SEAL_USER", UserId);
                    ei.PutDate("SEAL_DATE", DateTime.Now);
                    ml.Add(ei);
                }
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    return ToMessage(@Resources.Locale.L_ApiController_SealSuccess, true);
                }
                catch (Exception e)
                {
                    return ToMessage(e.Message, false);
                }
            }
            return ToMessage(@Resources.Locale.L_ActManage_Select1Data, false);
        }
        /// <summary>
        /// 封柜确认  
        /// </summary>
        /// <returns></returns>
        public ActionResult InFactoryConfirm()
        {
            string job_no = Request["id"];//Approve STATUS =Finish
            DataTable smrv = GetSMRVData(job_no);
            string dn_no = string.Empty;
            string bat_no = string.Empty;
            string is_batch = string.Empty;
            string status = Request["status"];
            string rv_type = string.Empty;
            DateTime? putdate = null;
            if (smrv.Rows.Count > 0)
            {
                rv_type = Prolink.Math.GetValueAsString(smrv.Rows[0]["RV_TYPE"]);
                is_batch = Prolink.Math.GetValueAsString(smrv.Rows[0]["IS_BATCH"]).ToUpper();
                dn_no = Prolink.Math.GetValueAsString(smrv.Rows[0]["DN_NO"]);
                bat_no = Prolink.Math.GetValueAsString(smrv.Rows[0]["BAT_NO"]);
                status = Prolink.Math.GetValueAsString(smrv.Rows[0]["STATUS"]);
                if (smrv.Rows[0]["PUT_DATE"] != null && smrv.Rows[0]["PUT_DATE"] != DBNull.Value)
                    putdate = Prolink.Math.GetValueAsDateTime(smrv.Rows[0]["PUT_DATE"]);
            }
            string returnMsg = Business.SMHandle.QAHoldBlMessage(bat_no, "QAH", true);
            if (!string.IsNullOrEmpty(returnMsg))
                return ToMessage(returnMsg, false);
            List<string> msg = new List<string>();

            if (!"I".Equals(rv_type))
            {
                string _dncountsql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID IN( SELECT SHIPMENT_ID FROM SMIRV WHERE U_ID={0}) AND SEAL_SATAUE !='Y'", SQLUtils.QuotedStr(job_no));
                DataTable _dncountdt = OperationUtils.GetDataTable(_dncountsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string _msg = string.Empty;
                if (_dncountdt != null && _dncountdt.Rows.Count > 0)
                {
                    foreach (DataRow dr in _dncountdt.Rows)
                    {
                        _msg += Prolink.Math.GetValueAsString(dr["DN_NO"]) + "\n";
                    }
                }
                if (!string.IsNullOrEmpty(_msg))
                    return ToMessage(@Resources.Locale.L_ApiController_SealError + "\n" + _msg, false);
            }

            if (string.IsNullOrEmpty(status))
                status = "I";
            if ("O".Equals(status))
                return ToMessage(@Resources.Locale.L_Api_Controllers_82, false);
            if ("P".Equals(status))
                return ToMessage(@Resources.Locale.L_SmrvSetup_Scripts_294, false);
            if (putdate != null)
            {
                if (putdate.Value.CompareTo(DateTime.Now) > 0)
                {
                    return ToMessage(@Resources.Locale.L_PutTime_Tip, false);
                }
            }

            bool approve_to = true;
            bool qtyIsEqual = true;

            List<string> oth_msg = new List<string>();
            if (!"I".Equals(rv_type))
            {
                approve_to = IsApproveFinish(job_no, bat_no, msg);
                qtyIsEqual = QtyIsEquals(job_no, bat_no, oth_msg);
            }
            if ((approve_to) || string.IsNullOrEmpty(dn_no) || "Y".Equals(is_batch))
                return UpdateGate(job_no, status, Request["wh"], true, false, smrv, oth_msg);
            else
            {
                if (msg.Count > 0)
                    return ToMessage(string.Join(",", msg.ToArray()) + @Resources.Locale.L_SmrvSetup_Scripts_291, false);
                else
                    return ToMessage(@Resources.Locale.L_Api_Controllers_130, false);
            }
        }

        /// <summary>
        /// 检查数量
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckQty()
        {
            string job_no = Request["id"];
            DataTable smrv = GetSMRVData(job_no);
            string DnList = Prolink.Math.GetValueAsString(Request["dnlist"]);
            string dn_no = string.Empty;
            string bat_no = string.Empty;
            if (smrv.Rows.Count > 0)
            {
                dn_no = Prolink.Math.GetValueAsString(smrv.Rows[0]["DN_NO"]);
                bat_no = Prolink.Math.GetValueAsString(smrv.Rows[0]["BAT_NO"]);
            }
            if (string.IsNullOrEmpty(DnList))
            {
                List<string> oth_msg = new List<string>();
                bool qtyIsEqual = QtyIsEquals(job_no, bat_no, oth_msg);
                return ToMessage(string.Join(",", oth_msg.ToArray()), qtyIsEqual);
            }
            else
            {
                List<string> msg = new List<string>();
                string[] list = DnList.Split(';');
                string qtySQL = string.Format("SELECT TRAN_TYPE,DN_NO,SEAL_QTY,QTY FROM SMDN WHERE DN_NO IN (SELECT DN_NO FROM SMIRV WHERE U_ID={0}) AND CARGO_TYPE='A' AND DN_NO IN {1} ", SQLUtils.QuotedStr(job_no), SQLUtils.Quoted(list));
                if (!string.IsNullOrEmpty(bat_no))
                    qtySQL = string.Format("SELECT TRAN_TYPE,DN_NO,SEAL_QTY,QTY FROM SMDN WHERE DN_NO IN (SELECT DN_NO FROM SMIRV WHERE BAT_NO={0}) AND CARGO_TYPE='A' AND DN_NO IN {1}", SQLUtils.QuotedStr(bat_no), SQLUtils.Quoted(list));
                DataTable qtyDt = OperationUtils.GetDataTable(qtySQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                bool result = true;
                foreach (DataRow qtyDr in qtyDt.Rows)
                {
                    string tran_type = Prolink.Math.GetValueAsString(qtyDr["TRAN_TYPE"]);
                    //问题单:103574  MODIFY BY FISH  需求:yvonneLee  封柜时，不要检查运输类型是国际/国内快递，货型为成品的序列号
                    if ("E".Equals(tran_type) || "D".Equals(tran_type))
                        continue;
                    decimal seal_qty = Prolink.Math.GetValueAsDecimal(qtyDr["SEAL_QTY"]);
                    decimal qty = Prolink.Math.GetValueAsDecimal(qtyDr["QTY"]);
                    if (seal_qty != qty)
                    {
                        msg.Add(string.Format(@Resources.Locale.L_Api_Controllers_133, Prolink.Math.GetValueAsString(qtyDr["DN_NO"]), qty, seal_qty));
                        result = false;
                    }
                }
                return ToMessage(string.Join(",", msg.ToArray()), result);
            }
        }

        /// <summary>
        /// 入厂确认(Inbound)
        /// </summary>
        /// <returns></returns>
        public ActionResult GateInConfirm()
        {
            string cmp = CompanyId;
            if (string.IsNullOrEmpty(cmp))
            {
                return Json(new { IsOk = false, message = @Resources.Locale.L_CookieMissing });
            }
            DateTime Now = DateTime.Now;
            bool re = false;
            string UId = Request["id"];
            //string Wscd = Request["wscd"];
            string TruckInfo = Request["data"];
            MixedList ml = new MixedList();
            string returnMessage = "";
            List<string> normallist = new List<string>();
            List<string> thirdPartyReserveno = new List<string>();

            string[] u = UId.Split(',');
            string[] t = TruckInfo.Split(',');
            //string[] w = Wscd.Split(',');
            string truckNo = t[0];
            string driver = t[1];
            string driverid = t[2];
            string tel = t[3];
            List<string> AllInsertTrackingSM = new List<string>();
            string podcd = string.Empty;

            for (int i = 0; i < u.Length; i++)
            {
                string uid = u[i];
                //string wscd = w[i];

                if (uid == "")
                {
                    break;
                }
                DataTable smrvDt = GetSMRVData(uid);
                string shippment_info = string.Empty;
                string reserve_no = string.Empty;
                string call_type = string.Empty;
                string ord_no = string.Empty;
                string ord_info = string.Empty;
                if (smrvDt != null && smrvDt.Rows.Count > 0)
                {
                    call_type = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CALL_TYPE"]);
                    reserve_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["RESERVE_NO"]);
                    shippment_info = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["SHIPMENT_INFO"]);
                    ord_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["ORD_NO"]);
                    ord_info = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["ORD_INFO"]);
                }
                string[] shipmentids = shippment_info.Split(',');
                string[] ordnos = ord_info.Split(',');

                string outterflag = GetOuterFlagInfo(reserve_no);
                if ("Y".Equals(outterflag))
                {
                    thirdPartyReserveno.Add(reserve_no);
                    continue;
                }
                normallist.Add(reserve_no);

                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.Put("STATUS", "I");
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, cmp);

                ei.PutDate("IN_DATE", odt);
                ei.PutDate("IN_DATE_L", ndt);
                ei.Put("TRUCK_NO", truckNo);
                ei.Put("DRIVER", driver);
                ei.Put("DRIVER_ID", driverid);
                ei.Put("TEL", tel);
                ei.Put("LTRUCK_NO", truckNo);
                ei.Put("LDRIVER", driver);
                ei.Put("LDRIVER_ID", driverid);
                ei.Put("LTEL", tel);
                ml.Add(ei);

                EditInstruct ei2 = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
                if ("D".Equals(call_type))
                    ei2 = new EditInstruct("SMRDN", EditInstruct.UPDATE_OPERATION);
                ei2.PutKey("RESERVE_NO", reserve_no);
                ei2.PutDate("IDATE", odt);
                ei2.PutDate("IDATE_L", ndt);
                ml.Add(ei2);

                string nowdate = odt.ToString("yyyy-MM-dd hh:mm:ss");
                string newdate = ndt.ToString("yyyy-MM-dd hh:mm:ss");
                string updatesql = string.Format("UPDATE SMICNTR SET SMICNTR.IDATE={0}, SMICNTR.IDATE_L={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={2}",
                    SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
                if ("D".Equals(call_type))
                {
                    updatesql = string.Format("UPDATE SMIDN SET SMIDN.IDATE={0}, SMIDN.IDATE_L={1} FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={2}",
                   SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
                }
                ml.Add(updatesql);

                for (int j = 0; j < shipmentids.Length; j++)
                {
                    string shippment_id = shipmentids[j];
                    if (!AllInsertTrackingSM.Contains(shippment_id))
                    {
                        AllInsertTrackingSM.Add(shippment_id);
                    }
                    if ("D".Equals(call_type))
                    {
                        string smrdnsql = string.Format(@"SELECT COUNT(1) FROM (
                        SELECT DISTINCT DN_NO FROM SMRDN WHERE SHIPMENT_ID LIKE '%{0}%' AND LOT_no IS NOT NULL) SMRDN", shippment_id);
                        int rdncount = OperationUtils.GetValueAsInt(smrdnsql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        string smidnsql = string.Format(@"SELECT COUNT(1) FROM (
                        SELECT DISTINCT DN_NO FROM SMIDN WHERE SHIPMENT_ID LIKE '%{0}%') SMRDN", shippment_id);
                        int idncount = OperationUtils.GetValueAsInt(smidnsql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (rdncount == idncount)
                        {
                            UpdateCstatus(ml, ord_no, ordnos);
                        }
                    }
                    else if ("D" != call_type)
                    {
                        UpdateCstatus(ml, ord_no, ordnos);
                    }

                    string gateSMRVSQL = "SELECT STATUS FROM SMIRV WHERE SHIPMENT_INFO like '%" + shippment_id + "%' and STATUS != 'V' and RESERVE_NO !='" + reserve_no + "'";
                    string gateSMRV2SQL = "SELECT ARRIVAL_FACT_DATE FROM SMIRV WHERE RESERVE_NO ='" + reserve_no + "'";
                    DataTable SMRVDt = OperationUtils.GetDataTable(gateSMRVSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    DataTable SMRV2Dt = OperationUtils.GetDataTable(gateSMRV2SQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string IsGateIn = "Y";
                    EditInstruct ei3 = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                    EditInstruct ei4 = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    if (SMRVDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr2 in SMRV2Dt.Rows)
                        {
                            DateTime Arrival_Fact_Date = Prolink.Math.GetValueAsDateTime(dr2["ARRIVAL_FACT_DATE"]);
                            if (Arrival_Fact_Date.Year == 1)
                            {
                                ei3.PutKey("RESERVE_NO", reserve_no);
                                ei3.PutDate("ARRIVAL_FACT_DATE", odt);
                                ei3.PutDate("ARRIVAL_FACT_DATE_L", ndt);
                                ml.Add(ei3);
                            }
                        }
                        foreach (DataRow dr in SMRVDt.Rows)
                        {
                            string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                            if (status == "A" || status == "D" || status == "R" || status == "C")
                            {
                                IsGateIn = "N";
                            }
                        }
                        if (IsGateIn == "Y")
                        {
                            string gateSMSMISQL = string.Format("SELECT STATUS,POD_CD,CSM_NO,U_ID FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shippment_id));
                            DataTable SMSMIDt = OperationUtils.GetDataTable(gateSMSMISQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (SMSMIDt.Rows.Count > 0)
                            {
                                foreach (DataRow dr in SMSMIDt.Rows)
                                {
                                    string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                                    string csmNo = Prolink.Math.GetValueAsString(dr["CSM_NO"]);
                                    string smuid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                                    if (string.IsNullOrEmpty(podcd))
                                    {
                                        podcd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                                    }
                                    if (status != "X")
                                    {
                                        ei4.PutKey("SHIPMENT_ID", shippment_id);
                                        ei4.Put("STATUS", "G");
                                        ml.Add(ei4);
                                    }
                                    if (!string.IsNullOrEmpty(csmNo))
                                        InboundHandel.UpdateShipmentStatusByCsmUId(smuid, "G", ml);
                                }
                            }
                        }
                    }
                    else
                    {
                        gateSMRVSQL = "SELECT ARRIVAL_FACT_DATE FROM SMIRV WHERE RESERVE_NO ='" + reserve_no + "'";
                        SMRVDt = OperationUtils.GetDataTable(gateSMRVSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (SMRVDt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in SMRVDt.Rows)
                            {
                                DateTime Arrival_Fact_Date = Prolink.Math.GetValueAsDateTime(dr["ARRIVAL_FACT_DATE"]);
                                if (Arrival_Fact_Date.Year == 1)
                                {
                                    ei3.PutKey("RESERVE_NO", reserve_no);
                                    ei3.PutDate("ARRIVAL_FACT_DATE", odt);
                                    ei3.PutDate("ARRIVAL_FACT_DATE_L", ndt);
                                    ml.Add(ei3);
                                }
                            }
                        }
                        string gateSMSMISQL = string.Format("SELECT STATUS,CSM_NO,U_ID FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shippment_id));
                        DataTable SMSMIDt = OperationUtils.GetDataTable(gateSMSMISQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (SMSMIDt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in SMSMIDt.Rows)
                            {
                                string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                                string csmNo = Prolink.Math.GetValueAsString(dr["CSM_NO"]);
                                string smuid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                                if (status != "X")
                                {
                                    ei4.PutKey("SHIPMENT_ID", shippment_id);
                                    ei4.Put("STATUS", "G");
                                    ml.Add(ei4);
                                }
                                if (!string.IsNullOrEmpty(csmNo))
                                    InboundHandel.UpdateShipmentStatusByCsmUId(smuid, "G", ml);
                            }
                        }
                    }
                }
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (string shipment in AllInsertTrackingSM)
                    {
                        TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = shipment, StsCd = "035", Cmp = cmp, Sender = UserId, Location = podcd, LocationName = "", StsDescp = "Order Truck By Container" });
                    }
                    re = true;
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            if (normallist.Count > 0)
            {
                returnMessage += " Appointment No.:" + string.Join(";", normallist) + " successed!";
            }


            if (thirdPartyReserveno.Count > 0)
            {
                returnMessage += " Appointment No.:" + string.Join(";", thirdPartyReserveno) + " is in third party WareHouse!";
            }
            return Json(new { IsOk = re, message = returnMessage });
        }

        private static string GetOuterFlagInfo(string reserve_no)
        {
            string outterflagSql = string.Format(@"SELECT TOP 10 OUTER_FLAG
						FROM BSADDR
						WHERE BSADDR.ADDR_CODE in(SELECT top 1 ADDR_CODE
						FROM SMRCNTR
						WHERE SMRCNTR.RESERVE_NO ={0}
						UNION
						SELECT top 1 ADDR_CODE
						FROM SMRDN
						WHERE SMRDN.RESERVE_NO={0}) order by OUTER_FLAG desc", SQLUtils.QuotedStr(reserve_no));
            string outterflag = OperationUtils.GetValueAsString(outterflagSql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return outterflag;
        }

        private static void UpdateCstatus(MixedList ml, string ord_no, string[] ordnos, string status = "I")
        {
            EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            if (!string.IsNullOrEmpty(ord_no))
            {
                ei.PutKey("ORD_NO", ord_no);
                if ("O".Equals(status) && IsSMORDChangeToFinish(ord_no))
                {
                    ei.Put("CSTATUS", "Z");
                }
                else
                {
                    ei.Put("CSTATUS", status);
                }
                ml.Add(ei);
            }
            else
            {
                for (int k = 0; k < ordnos.Length; k++)
                {
                    if (k > 0)
                        ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("ORD_NO", ordnos[k]);
                    if ("O".Equals(status) && IsSMORDChangeToFinish(ordnos[k]))
                    {
                        ei.Put("CSTATUS", "Z");
                    }
                    else
                    {
                        ei.Put("CSTATUS", status);
                    }
                    ml.Add(ei);
                }
            }
        }

        /// <summary>
        /// 出厂确认(Inbound)
        /// </summary>
        /// <returns></returns>
        public ActionResult GateOutConfirm()
        {
            bool re = false;
            string UId = Request["id"];
            //string Wscd = Request["wscd"];
            MixedList ml = new MixedList();
            string returnMessage = "";
            List<string> normallist = new List<string>();
            List<string> thirdPartyReserveno = new List<string>();

            string[] u = UId.Split(',');
            List<string> AllInsertTrackingSM = new List<string>();
            string podcd = string.Empty;
            //string[] w = Wscd.Split(',');

            UserInfo userInfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            for (int i = 0; i < u.Length; i++)
            {
                string uid = u[i];
                //string wscd = w[i];

                if (uid == "")
                {
                    break;
                }
                DataTable smrvDt = GetSMRVData(uid);
                string shippment_info = string.Empty;
                string reserve_no = string.Empty;
                string call_type = string.Empty;
                string ord_no = string.Empty;
                string ord_info = string.Empty;

                if (smrvDt != null && smrvDt.Rows.Count > 0)
                {
                    call_type = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CALL_TYPE"]);
                    reserve_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["RESERVE_NO"]);
                    shippment_info = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["SHIPMENT_INFO"]);
                    ord_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["ORD_NO"]);
                    ord_info = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["ORD_INFO"]);
                }
                string[] s = shippment_info.Split(',');
                string[] ordnos = ord_info.Split(',');

                string outterflag = GetOuterFlagInfo(reserve_no);
                if ("Y".Equals(outterflag))
                {
                    thirdPartyReserveno.Add(reserve_no);
                    continue;
                }
                normallist.Add(reserve_no);


                string gateOutSQL = string.Format("SELECT POD_MDATE,CNTR_NO,DN_NO FROM SMRCNTR WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
                if ("D".Equals(call_type))
                    gateOutSQL = string.Format("SELECT POD_MDATE,'' AS CNTR_NO,DN_NO FROM SMRDN WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));

                DataTable smDt = OperationUtils.GetDataTable(gateOutSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smDt.Rows.Count > 0)
                {
                    if (smDt.Select(string.Format("POD_MDATE IS NULL")).Length == smDt.Rows.Count)
                        return ToMessage("No POD{edoc Type: POD}, no Abnormal {edoc type: Abnormal pic} for container/DN{" + Prolink.Math.GetValueAsString(smDt.Rows[0]["CNTR_NO"]) + "/" + Prolink.Math.GetValueAsString(smDt.Rows[0]["DN_NO"]) + "} , cannot Gate Out factory. Pls check with WH/Warehouse for help.", false);
                }

                string gateFWHSQL = string.Format("SELECT b.FINAL_WH FROM SMRCNTR as a, BSADDR as b WHERE a.ADDR_CODE=b.ADDR_CODE AND RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
                if ("D".Equals(call_type))
                    gateFWHSQL = string.Format("SELECT b.FINAL_WH FROM SMRDN as a, BSADDR as b WHERE a.ADDR_CODE=b.ADDR_CODE AND RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
                DataTable FWHDt = OperationUtils.GetDataTable(gateFWHSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (FWHDt.Rows.Count > 0)
                {
                    string IsFinal = "Y";
                    for (int j = 0; j < s.Length; j++)
                    {
                        string shippment_id = s[j];
                        if (!AllInsertTrackingSM.Contains(shippment_id)) {
                            AllInsertTrackingSM.Add(shippment_id);
                        }

                        foreach (DataRow dr in FWHDt.Rows)
                        {
                            string FINAL_WH = Prolink.Math.GetValueAsString(dr["FINAL_WH"]);
                            if (FINAL_WH == "Temp")
                            {
                                UpdateCstatus(ml, ord_no, ordnos, "T");
                                IsFinal = "N";
                            }
                        }
                        if (IsFinal == "Y")
                        {
                            UpdateCstatus(ml, ord_no, ordnos, "O");
                        }
                    }
                }

                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                if (IsSMRVChangeToFinish(uid))
                {
                    ei.Put("STATUS", "Z");
                }
                else
                {
                    ei.Put("STATUS", "O");
                }
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                ei.PutDate("OUT_DATE", odt);
                ei.PutDate("OUT_DATE_L", ndt);
                ml.Add(ei);

                EditInstruct ei2 = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
                if ("D".Equals(call_type))
                    ei2 = new EditInstruct("SMRDN", EditInstruct.UPDATE_OPERATION);
                ei2.PutKey("RESERVE_NO", reserve_no);
                ei2.PutDate("LDATE", odt);
                ei2.PutDate("LDATE_L", ndt);
                ml.Add(ei2);

                string nowdate = odt.ToString("yyyy-MM-dd hh:mm:ss");
                string newdate = ndt.ToString("yyyy-MM-dd hh:mm:ss");
                string updatesql = string.Format("UPDATE SMICNTR SET SMICNTR.ODATE={0}, SMICNTR.ODATE_L={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={2}",
                    SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
                if ("D".Equals(call_type))
                {
                    updatesql = string.Format("UPDATE SMIDN SET SMIDN.ODATE={0}, SMIDN.ODATE_L={1} FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={2}",
                   SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
                }
                ml.Add(updatesql);


                for (int j = 0; j < s.Length; j++)
                {
                    string shippment_id = s[j];
                    string gateSMRVSQL = "SELECT STATUS FROM SMIRV WHERE SHIPMENT_INFO like '%" + shippment_id + "%' and STATUS != 'V' and RESERVE_NO !='" + reserve_no + "'";
                    DataTable SMRVDt = OperationUtils.GetDataTable(gateSMRVSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string IsGateOut = "Y";
                    EditInstruct ei3 = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                    if (SMRVDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in SMRVDt.Rows)
                        {
                            string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                            if (status != "O" && status != "Z")
                            {
                                IsGateOut = "N";
                            }
                        }
                        if (IsGateOut == "Y")
                        {
                            UpdateSMSMIStatus(ml, shippment_id, userInfo);
                        }
                    }
                    else
                    {
                        UpdateSMSMIStatus(ml, shippment_id, userInfo);
                    }
                }
                if (smrvDt != null && smrvDt.Rows.Count > 0)
                {
                    DataRow item = smrvDt.Rows[0];
                    string CntrNo = Prolink.Math.GetValueAsString(item["CNTR_NO"]);
                    string TruckCntrno = Prolink.Math.GetValueAsString(item["TRUCK_CNTRNO"]);
                    string TruckNo = Prolink.Math.GetValueAsString(item["TRUCK_NO"]);
                    string GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    string BatNo = Prolink.Math.GetValueAsString(item["BAT_NO"]);

                    if (CntrNo == "" && TruckCntrno == "")
                    {
                        CntrNo = TruckNo;
                    }
                    else if (CntrNo == "")
                    {
                        CntrNo = TruckCntrno;
                    }
                    string sql = "UPDATE SMWHGT SET CNTR_NO=NULL,RESERVE_NO=NULL WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd);
                    ml.Add(sql);
                }

            }

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string sql1 = string.Format("SELECT POD_CD,SHIPMENT_ID FROM SMSMI WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(AllInsertTrackingSM.ToArray()));
                    DataTable dt = getDataTableFromSql(sql1);
                    foreach (DataRow dr in dt.Rows)
                    {
                        TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = dr["SHIPMENT_ID"].ToString(), StsCd = "100", Cmp = CompanyId, Sender = UserId, Location = dr["POD_CD"].ToString(), LocationName = "", StsDescp = "Leave Factory" });
                    }
                    re = true;
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            if (normallist.Count > 0)
            {
                returnMessage += " Appointment No.:" + string.Join(";", normallist) + " successed!";
            }


            if (thirdPartyReserveno.Count > 0)
            {
                returnMessage += " Appointment No.:" + string.Join(";", thirdPartyReserveno) + " is in third party WareHouse!";
            }

            return Json(new { IsOk = re, message = returnMessage });
        }

        private static bool IsSMORDChangeToFinish(string ord_no)
        {
            string sql = string.Format(@"SELECT * FROM (SELECT ORD_NO,TRAN_TYPE,(SELECT COUNT(1) FROM SMRCNTR WHERE EMPTY_TIME IS NULL AND SMRCNTR.ORD_NO=SMORD.ORD_NO)AS EMPTYNULL FROM SMORD )T
                        WHERE EMPTYNULL>0 AND TRAN_TYPE IN ('F','R')  AND ORD_NO={0}", SQLUtils.QuotedStr(ord_no));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                int isnullcount = Prolink.Math.GetValueAsInt(dt.Rows[0]["EMPTYNULL"]);
                if (isnullcount > 0)
                    return false;
            }
            return true;
        }

        private static bool IsSMRVChangeToFinish(string smrvuid)
        {
            string sql = string.Format(@"SELECT * FROM (SELECT (SELECT TOP 1 TRAN_TYPE FROM SMORD WHERE SMORD.ORD_NO=SMIRV.ORD_NO) AS TRAN_TYPE
            ,(SELECT COUNT(1) FROM SMRCNTR WHERE EMPTY_TIME IS NULL AND SMRCNTR.ORD_NO=SMIRV.ORD_NO)AS EMPTYNULL
            FROM SMIRV WHERE U_ID={0})T  WHERE EMPTYNULL>0 AND TRAN_TYPE IN ('F','R') ", SQLUtils.QuotedStr(smrvuid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                int isnullcount = Prolink.Math.GetValueAsInt(dt.Rows[0]["EMPTYNULL"]);
                if (isnullcount > 0)
                    return false;
            }
            return true;
        }

        private static void UpdateSMSMIStatus(MixedList ml, string shippment_id, UserInfo userInfo)
        {
            string gateSMSMISQL = string.Format(@"SELECT STATUS,SHIPMENT_ID ,TRAN_TYPE,U_ID,CSM_NO,MASTER_NO,HOUSE_NO,
                (SELECT COUNT(1) FROM SMICNTR WHERE EMPTY_TIME IS NULL AND SMICNTR.SHIPMENT_ID=SMSMI.SHIPMENT_ID)AS EMPTYNULL
                FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shippment_id));
            DataTable SMSMIDt = OperationUtils.GetDataTable(gateSMSMISQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (SMSMIDt.Rows.Count > 0)
            {
                foreach (DataRow dr in SMSMIDt.Rows)
                {
                    string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                    string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    string csmNo = Prolink.Math.GetValueAsString(dr["CSM_NO"]);
                    string MasterNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                    string HouseNo = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);
                    string trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                    if (status != "X")
                    {
                        EditInstruct ei4 = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        ei4.PutKey("SHIPMENT_ID", shippment_id);
                        int count = Prolink.Math.GetValueAsInt(dr["EMPTYNULL"]);
                        if ("F".Equals(trantype) || "R".Equals(trantype))
                        {
                            if (count == 0)
                                ei4.Put("STATUS", "Z");
                            else
                                ei4.Put("STATUS", "O");
                        }
                        else
                        {
                            ei4.Put("STATUS", "Z");
                        }
                        ml.Add(ei4);
                        if (!string.IsNullOrEmpty(csmNo))
                            InboundHandel.UpdateShipmentStatusByCsmUId(uid, "O", ml);
                    }
                    string delist = string.IsNullOrEmpty(HouseNo) ? MasterNo : HouseNo;
                    if ("E".Equals(trantype))
                    {
                        delist = HouseNo;
                    }
                    InboundHandel.CreateDeclaration(uid, shippment_id, userInfo, delist, "POD", ml);
                }
            }
        }

        /// <summary>
        /// 出厂确认
        /// </summary>
        /// <returns></returns>
        public ActionResult OutFactoryConfirm()
        {
            string status = Request["status"];
            if (string.IsNullOrEmpty(status))
                status = "P";
            //return UpdateGate(Request["id"], status,false,true);

            string job_no = Request["id"];//Approve STATUS =Finish

            DataTable smrvDt = GetSMRVData(job_no);
            string is_batch = string.Empty;
            string old_status = string.Empty;
            string ltruck_no = string.Empty;
            string truckNo = string.Empty;

            string seal_no1 = string.Empty;
            string seal_no2 = string.Empty;

            string bat_no = string.Empty;
            string dn_no = string.Empty;
            string rv_type = string.Empty;
            string cntr_no = string.Empty;
            string reserve_no = string.Empty;
            string call_type = string.Empty;

            if (smrvDt != null && smrvDt.Rows.Count > 0)
            {
                call_type = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CALL_TYPE"]);
                reserve_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["RESERVE_NO"]);

                bat_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["BAT_NO"]);
                dn_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["DN_NO"]);

                is_batch = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["IS_BATCH"]).ToUpper();
                old_status = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["STATUS"]);
                truckNo = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["TRUCK_NO"]);
                ltruck_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["LTRUCK_NO"]);
                seal_no1 = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["SEAL_NO1"]).ToUpper().Trim();
                seal_no2 = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["SEAL_NO2"]).ToUpper().Trim();

                cntr_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CNTR_NO"]);
                rv_type = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["RV_TYPE"]);
            }

            if ("I".Equals(rv_type))//沒上傳POD不能GATE OUT顯示錯誤訊息
            {
                string gateOutSQL = string.Format("SELECT POD_MDATE,CNTR_NO,DN_NO FROM SMRCNTR WHERE SMRCNTR.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
                if ("D".Equals(call_type))
                    gateOutSQL = string.Format("SELECT POD_MDATE,'' AS CNTR_NO,DN_NO FROM SMRDN WHERE SMRDN.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));

                //沒上傳POD不能GATE OUT顯示錯誤訊息
                DataTable smDt = OperationUtils.GetDataTable(gateOutSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smDt.Rows.Count > 0)
                {
                    if (smDt.Select(string.Format("POD_MDATE IS NULL")).Length == smDt.Rows.Count)
                        return ToMessage("No POD{edoc Type: POD}, no Abnormal {edoc type: Abnormal pic} for container/DN{" + Prolink.Math.GetValueAsString(smDt.Rows[0]["CNTR_NO"]) + "/" + Prolink.Math.GetValueAsString(smDt.Rows[0]["DN_NO"]) + "} , cannot Gate Out factory. Pls check with WH/Warehouse for help.", false);
                }
            }

            int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            if (!"I".Equals(rv_type))
            {
                string returnMsg = Business.SMHandle.QAHoldBlMessage(bat_no, "QAH", true);
                if (!string.IsNullOrEmpty(returnMsg))
                    return ToMessage(returnMsg, false);
                if ("O".Equals(old_status))
                    return ToMessage(@Resources.Locale.L_Api_Controllers_82, false);
                if (mode == 0 && !"P".Equals(old_status))
                    return ToMessage(@Resources.Locale.L_ApiController_Controllers_22, false);

                if (mode == 1)
                {
                    if (string.IsNullOrEmpty(truckNo))
                        return ToMessage(@Resources.Locale.L_ApiController_Controllers_23, false);
                }
                else
                {
                    if (string.IsNullOrEmpty(ltruck_no))
                        return ToMessage(@Resources.Locale.L_ApiController_Controllers_24, false);
                }


                if ("P".Equals(old_status))
                {
                    string check_SealNo = Request["check_SealNo"];
                    if (!string.IsNullOrEmpty(check_SealNo))
                    {
                        check_SealNo = HttpUtility.UrlDecode(check_SealNo).ToUpper().Trim();
                    }
                    else
                        check_SealNo = string.Empty;
                    if (!string.IsNullOrEmpty(seal_no1))
                    {
                        if (string.IsNullOrEmpty(check_SealNo))
                            return ToMessage(@Resources.Locale.L_ApiController_Controllers_25, false);
                        if (!check_SealNo.Equals(seal_no1))
                            return ToMessage(@Resources.Locale.L_ApiController_Controllers_26, false);
                    }
                    //return Json(new { message = "您输入的封条号和平台上的不一致,请尽快联络仓库", flag = false });
                }
            }

            //string is_batch = OperationUtils.GetValueAsString(string.Format("SELECT IS_BATCH,[STATUS] FROM SMIRV WHERE U_ID={0}", SQLUtils.QuotedStr(job_no)), Prolink.Web.WebContext.GetInstance().GetConnection());

            //分批出货和dn签核才可出厂
            try
            {
                List<string> msg = new List<string>();
                bool approve_to = true;
                if (!"I".Equals(rv_type))
                {
                    if (!"Y".Equals(is_batch) && mode != 1)//不为分批出货
                        approve_to = IsApproveFinish(job_no, bat_no, msg);
                }

                if ("Y".Equals(is_batch) || mode == 1 || approve_to || string.IsNullOrEmpty(dn_no))
                    return UpdateGate(job_no, old_status, Request["wh"], false, true, smrvDt);
                else
                {
                    if (msg.Count > 0)
                        return ToMessage(string.Join(",", msg.ToArray()) + @Resources.Locale.L_Api_Controllers_92, false);
                    else
                        return ToMessage(@Resources.Locale.L_Api_Controllers_131, false);
                    //return Json(new { message = "DN未签核完成不可出厂", flag = false });
                }
            }
            catch (Exception e)
            {
                return ToMessage(e.Message, false);
            }
        }

        /// <summary>
        /// 判断是否签核完成
        /// </summary>
        /// <param name="job_no"></param>
        /// <param name="bat_no"></param>
        /// <returns></returns>
        private static bool IsApproveFinish(string job_no, string bat_no, List<string> msg)
        {
            string approveSQL = string.Format("SELECT DN_NO,APPROVE_TO FROM SMDN WHERE DN_NO IN (SELECT DN_NO FROM SMIRV WHERE U_ID={0})", SQLUtils.QuotedStr(job_no));
            if (!string.IsNullOrEmpty(bat_no))
                approveSQL = string.Format("SELECT DN_NO,APPROVE_TO FROM SMDN WHERE DN_NO IN (SELECT DN_NO FROM SMIRV WHERE BAT_NO={0})", SQLUtils.QuotedStr(bat_no));
            DataTable approveDt = OperationUtils.GetDataTable(approveSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            bool approve_to = true;
            foreach (DataRow at in approveDt.Rows)
            {
                string approve_to0 = Prolink.Math.GetValueAsString(at["APPROVE_TO"]);
                if (string.IsNullOrEmpty(approve_to0) || !"finish".Equals(approve_to0.ToLower()))
                {
                    msg.Add(string.Format(@Resources.Locale.L_Api_Controllers_132, Prolink.Math.GetValueAsString(at["DN_NO"])));
                    approve_to = false;
                }
            }
            return approve_to;
        }

        /// <summary>
        /// 在封櫃頁面點擊封柜时，需檢核序列號數(DN明細內)要與DN數量一致才可作封櫃,(只需檢查成品)
        /// </summary>
        /// <param name="job_no"></param>
        /// <param name="bat_no"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool QtyIsEquals(string job_no, string bat_no, List<string> msg)
        {
            string qtySQL = string.Format("SELECT TRAN_TYPE,DN_NO,SEAL_QTY,QTY FROM SMDN WHERE DN_NO IN (SELECT DN_NO FROM SMIRV WHERE U_ID={0}) AND CARGO_TYPE='A'", SQLUtils.QuotedStr(job_no));
            if (!string.IsNullOrEmpty(bat_no))
                qtySQL = string.Format("SELECT TRAN_TYPE,DN_NO,SEAL_QTY,QTY FROM SMDN WHERE DN_NO IN (SELECT DN_NO FROM SMIRV WHERE BAT_NO={0}) AND CARGO_TYPE='A'", SQLUtils.QuotedStr(bat_no));
            DataTable qtyDt = OperationUtils.GetDataTable(qtySQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            bool result = true;
            foreach (DataRow qtyDr in qtyDt.Rows)
            {
                string tran_type = Prolink.Math.GetValueAsString(qtyDr["TRAN_TYPE"]);
                //问题单:103574  MODIFY BY FISH  需求:yvonneLee  封柜时，不要检查运输类型是国际/国内快递，货型为成品的序列号
                if ("E".Equals(tran_type) || "D".Equals(tran_type))
                    continue;
                decimal seal_qty = Prolink.Math.GetValueAsDecimal(qtyDr["SEAL_QTY"]);
                decimal qty = Prolink.Math.GetValueAsDecimal(qtyDr["QTY"]);
                if (seal_qty != qty)
                {
                    msg.Add(string.Format(@Resources.Locale.L_Api_Controllers_133, Prolink.Math.GetValueAsString(qtyDr["DN_NO"]), qty, seal_qty));
                    result = false;
                }
            }
            return result;
        }
        #endregion
        /// <summary>
        /// 更新DN封柜
        /// </summary>
        /// <param name="ml"></param>
        /// <param name="uid"></param>
        public void UpdateSealDN(MixedList ml, string uid)
        {
            string dncountsql = string.Format("SELECT SHIPMENT_ID FROM SMIRV WHERE BAT_NO in(select BAT_NO from SMIRV WHERE U_ID={0})", SQLUtils.QuotedStr(uid));
            DataTable dncount = OperationUtils.GetDataTable(dncountsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dncount != null && dncount.Rows.Count > 0)
            {
                foreach (DataRow dr in dncount.Rows)
                {
                    EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", dr["SHIPMENT_ID"]);
                    ei.Put("SEAL_SATAUE", "Y");
                    ei.Put("SEAL_USER", UserId);
                    ei.PutDate("SEAL_DATE", DateTime.Now);
                    ml.Add(ei);
                }
            }
        }
        #region 基础操作
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status">当前状态</param>
        /// <param name="isP">是否封柜</param>
        /// <param name="isO">是否出厂</param>
        /// <returns></returns>
        [NonAction]
        private ActionResult UpdateGate(string id, string status, string wh, bool isP = false, bool isO = false, DataTable smrv = null, List<string> oth_msg = null)
        {
            bool isIOut = false;//是否是进口端离厂
            MixedList ml = new MixedList();
            List<string> allStatus = new List<string>();//允许的状态
            switch (status)
            {
                case "R":
                case "C":
                    allStatus.Add("A");
                    allStatus.Add("E");
                    allStatus.Add("R");
                    allStatus.Add("C");
                    break;
            }
            string irvCmp = string.Empty;
            string shipment_id = string.Empty;
            string shipment_info = string.Empty;
            string bat_no = string.Empty;
            string oldLtruckNo = string.Empty; ;
            string rv_type = string.Empty;
            string oldLdriver = string.Empty, oldCntrNo = string.Empty, oldSealNo1 = string.Empty, twu = string.Empty, ept_odate = string.Empty, in_date = string.Empty, call_type = string.Empty, reserve_no = string.Empty;
            decimal tare_weight = 0m;
            string tranType = Request["tranType"];
            #region 获取预约参数
            if (smrv == null)
            {
                smrv = GetSMRVData(id);
            }
            if (smrv != null && smrv.Rows.Count > 0)
            {
                shipment_info = Prolink.Math.GetValueAsString(smrv.Rows[0]["SHIPMENT_INFO"]);
                reserve_no = Prolink.Math.GetValueAsString(smrv.Rows[0]["RESERVE_NO"]);
                call_type = Prolink.Math.GetValueAsString(smrv.Rows[0]["CALL_TYPE"]);
                rv_type = smrv.Columns.Contains("RV_TYPE") ? Prolink.Math.GetValueAsString(smrv.Rows[0]["RV_TYPE"]) : string.Empty;
                shipment_id = smrv.Columns.Contains("SHIPMENT_ID") ? Prolink.Math.GetValueAsString(smrv.Rows[0]["SHIPMENT_ID"]) : string.Empty;
                irvCmp = Prolink.Math.GetValueAsString(smrv.Rows[0]["CMP"]);
                if (string.IsNullOrEmpty(irvCmp))
                    irvCmp = CompanyId;
                tranType = Prolink.Math.GetValueAsString(smrv.Rows[0]["TRAN_TYPE"]);

                oldCntrNo = Prolink.Math.GetValueAsString(smrv.Rows[0]["CNTR_NO"]);
                oldSealNo1 = Prolink.Math.GetValueAsString(smrv.Rows[0]["SEAL_NO1"]);

                oldLtruckNo = Prolink.Math.GetValueAsString(smrv.Rows[0]["LTRUCK_NO"]);
                oldLdriver = Prolink.Math.GetValueAsString(smrv.Rows[0]["LDRIVER"]);
                bat_no = Prolink.Math.GetValueAsString(smrv.Rows[0]["BAT_NO"]);
                status = Prolink.Math.GetValueAsString(smrv.Rows[0]["STATUS"]);
                tare_weight = Prolink.Math.GetValueAsDecimal(smrv.Rows[0]["TARE_WEIGHT"]);
                twu = Prolink.Math.GetValueAsString(smrv.Rows[0]["TWU"]);
                ept_odate = Prolink.Math.GetValueAsString(smrv.Rows[0]["EPT_ODATE"]);
                in_date = Prolink.Math.GetValueAsString(smrv.Rows[0]["IN_DATE"]);
            }
            #endregion

            if (allStatus.Count > 0 && !allStatus.Contains(status))
            {
                return ToMessage("数据状态已更新请重新查询后再操作", false);
            }

            string message = string.Empty;
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("STATUS", status);
            ei.PutKey("GROUP_ID", GroupId);
            ei.PutKey("CMP", irvCmp);
            if (!string.IsNullOrEmpty(bat_no))
                ei.PutKey("BAT_NO", bat_no);
            else
                ei.PutKey("U_ID", id);

            bool flag = true;
            string newStatus = string.Empty;
            switch (status)
            {
                case "O":
                case "A":
                case "E":
                case "R"://预约转进场
                case "C"://预约确认转进场
                    newStatus = "I";
                    break;
                case "I"://进场转封柜
                    newStatus = "P";
                    break;
                case "U"://POD转出厂
                case "P"://封柜转出厂
                    newStatus = "O";
                    break;
            }
            if (isP)
                newStatus = "P";
            if (isO)
                newStatus = "O";
            if (IsSMRVChangeToFinish(id) && isO)
                newStatus = "Z";
            if (!string.IsNullOrEmpty(newStatus))
                ei.Put("STATUS", newStatus);
            int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            switch (newStatus)
            {
                case "P"://封柜
                    ei.Put("SEAL_BY", UserId);
                    DateTime odt = DateTime.Now;
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                    ei.PutDate("SEAL_DATE", odt);
                    ei.PutDate("SEAL_DATE_L", ndt);
                    message = @Resources.Locale.L_ApiController_Controllers_28;
                    break;
                case "I"://入厂
                    if (mode == 1 && !"E".Equals(status))
                    {
                        //ei.Put("STATUS", "E");
                        DateTime odt1 = DateTime.Now;
                        DateTime ndt1 = TimeZoneHelper.GetTimeZoneDate(odt1, CompanyId);
                        ei.PutDate("EPT_IDATE", odt1);
                        ei.PutDate("EPT_IDATE_L", ndt1);
                    }
                    else
                    {
                        DateTime odt2 = DateTime.Now;
                        DateTime ndt2 = TimeZoneHelper.GetTimeZoneDate(odt2, CompanyId);
                        ei.PutDate("IN_DATE", odt2);
                        ei.PutDate("IN_DATE_L", ndt2);
                    }
                    ei.Put("IN_BY", UserId);
                    message = @Resources.Locale.L_ApiController_Controllers_29;
                    break;
                case "O"://出厂
                case "Z":
                    if (mode == 1)
                    {
                        if ("I".Equals(rv_type))//警衛APP 在出廠時，如果該Shipment ID/預約號是Inbound（RV_TYPE=I），則空車離廠要改為已離場
                        {
                            isIOut = true;
                            //newStatus = "O";
                            ei.Put("STATUS", newStatus);
                            DateTime odt1 = DateTime.Now;
                            DateTime ndt1 = TimeZoneHelper.GetTimeZoneDate(odt1, CompanyId);
                            ei.PutDate("OUT_DATE", odt1);
                            ei.PutDate("OUT_DATE_L", ndt1);
                        }
                        else
                        {
                            DateTime odt1 = DateTime.Now;
                            DateTime ndt1 = TimeZoneHelper.GetTimeZoneDate(odt1, CompanyId);
                            ei.PutDate("EPT_ODATE", odt1);
                            ei.PutDate("EPT_ODATE_L", ndt1);
                            newStatus = "E";
                            ei.Put("STATUS", newStatus);//空车出厂
                        }
                    }
                    else
                    {
                        DateTime odt1 = DateTime.Now;
                        DateTime ndt1 = TimeZoneHelper.GetTimeZoneDate(odt1, CompanyId);
                        ei.PutDate("OUT_DATE", odt1);
                        ei.PutDate("OUT_DATE_L", ndt1);
                    }
                    ei.Put("OUT_BY", UserId);
                    if (smrv.Rows.Count > 0)
                    {
                        string cmp = Prolink.Math.GetValueAsString(smrv.Rows[0]["CMP"]);
                        string wscd = Prolink.Math.GetValueAsString(smrv.Rows[0]["WS_CD"]);
                        string gateno = Prolink.Math.GetValueAsString(smrv.Rows[0]["GATE_NO"]);
                        string sql = "UPDATE SMWHGT SET CNTR_NO='',RESERVE_NO='' WHERE CMP=" + SQLUtils.QuotedStr(cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(wscd);
                        ml.Add(sql);
                    }
                    //ei.PutDate("EPT_ODATE", DateTime.Now);       
                    message = @Resources.Locale.L_ApiController_Controllers_30;
                    break;
                default:
                    return ToMessage(@Resources.Locale.L_Api_Controllers_100, false);
                    //return Json(new { message = "操作失败",flag=false });
            }

            string seal_no1 = Request["seal_no1"];
            if (!string.IsNullOrEmpty(seal_no1))
            {
                seal_no1 = HttpUtility.UrlDecode(seal_no1).Trim();
                ei.Put("SEAL_NO1", seal_no1.ToUpper());
            }

            string cntr_no = Request["cntr_no"];
            if (!string.IsNullOrEmpty(cntr_no))
            {
                cntr_no = HttpUtility.UrlDecode(cntr_no).Trim();
                ei.Put("CNTR_NO", cntr_no.ToUpper());
            }

            string seal_no2 = Request["seal_no2"];
            if (!string.IsNullOrEmpty(seal_no2))
            {
                seal_no2 = HttpUtility.UrlDecode(seal_no2).Trim();
                ei.Put("SEAL_NO2", seal_no2.ToUpper());
            }

            SetEiValue(ei, "LDRIVER", Request["Driver"]);
            SetEiValue(ei, "LTEL", Request["Tel"]);
            SetEiValue(ei, "LDRIVER_ID", Request["DriverId"]);
            SetEiValue(ei, "LTRUCK_NO", Request["TruckNo"]);

            string LtruckNo = SetEiValue(ei, "LTRUCK_NO", Request["LtruckNo"]);
            string Ldriver = SetEiValue(ei, "LDRIVER", Request["Ldriver"]);
            string Ltel = SetEiValue(ei, "LTEL", Request["Ltel"]);
            string LdriverId = SetEiValue(ei, "LDRIVER_ID", Request["LdriverId"]);

            SetEiValue(ei, "DRIVER", Request["Driver"]);
            SetEiValue(ei, "TEL", Request["Tel"]);
            SetEiValue(ei, "DRIVER_ID", Request["DriverId"]);
            SetEiValue(ei, "TRUCK_NO", Request["TruckNo"]);

            if ("I".Equals(newStatus) && !"I".Equals(rv_type))
            {
                //问题单：105739   add by fish 2016/6/24 批次號碼欄位如果為空：1.警衛APP ：不能入廠、出廠2.倉庫封櫃APP：不能封櫃
                if (string.IsNullOrEmpty(bat_no))
                    return ToMessage(string.Format(@Resources.Locale.L_ApiController_Controllers_31), false);
                if ("R".Equals(status))
                    return ToMessage(string.Format(@Resources.Locale.L_ApiController_Controllers_32), false);
                if (mode == 1 && (string.IsNullOrEmpty(in_date) && string.IsNullOrEmpty(ept_odate)))
                    return ToMessage(string.Format(@Resources.Locale.L_ApiController_Controllers_33), false);
            }

            if ("P".Equals(newStatus))
            {
                //问题单：105739   add by fish 2016/6/24 批次號碼欄位如果為空：1.警衛APP ：不能入廠、出廠2.倉庫封櫃APP：不能封櫃
                if (string.IsNullOrEmpty(bat_no))
                    return ToMessage(string.Format(@Resources.Locale.L_ApiController_Controllers_34), false);

                #region 车号  司机必输确认
                //if (string.IsNullOrEmpty(LtruckNo)&&string.IsNullOrEmpty(oldLtruckNo))
                //{
                //    return ToMessage("需要输入出厂车号才能封柜", false);
                //}

                //if (string.IsNullOrEmpty(Ldriver) && string.IsNullOrEmpty(oldLdriver))
                //{
                //    return ToMessage("需要输入出厂司机才能封柜", false);
                //}
                #endregion

                #region 货柜 封条号 必输确认
                if (!string.IsNullOrEmpty(tranType))
                {
                    Dictionary<string, string> tranTypeMap = new Dictionary<string, string>();
                    tranTypeMap["F"] = "FCL";
                    //tranTypeMap["L"] = "LCL";
                    //tranTypeMap["A"] = "空运";
                    //tranTypeMap["T"] = "内贸";
                    tranTypeMap["R"] = @Resources.Locale.L_DNManage_Rail;
                    if (tranTypeMap.ContainsKey(tranType))//一定需要封条号的
                    {
                        if (string.IsNullOrEmpty(seal_no1) && string.IsNullOrEmpty(oldSealNo1))
                        {
                            return ToMessage(string.Format(@Resources.Locale.L_Api_Controllers_134, tranTypeMap[tranType]), false);
                        }
                    }

                    Dictionary<string, string> conMap = new Dictionary<string, string>();
                    conMap["F"] = "FCL";
                    conMap["R"] = @Resources.Locale.L_DNManage_Rail;
                    if (conMap.ContainsKey(tranType))//一定需要封条号的
                    {
                        if (string.IsNullOrEmpty(cntr_no) && string.IsNullOrEmpty(oldCntrNo))
                        {
                            return ToMessage(string.Format(@Resources.Locale.L_Api_Controllers_135, conMap[tranType]), false);
                        }
                    }

                    #region add by fish 2016/6/13 问题单:105138  關於APP封櫃 ：在貨櫃管理頁面如果為FCL，則封櫃時，Tare Weight為必輸項  需求:mina
                    if ("F".Equals(tranType))
                    {
                        string msg = string.Empty;
                        if (tare_weight == 0)
                        {
                            msg += @Resources.Locale.L_Api_Controllers_70;
                        }
                        if (string.IsNullOrEmpty(twu))
                        {
                            msg += @Resources.Locale.L_Api_Controllers_69;
                        }
                        if (!string.IsNullOrEmpty(msg))
                            return ToMessage(msg, false);
                    }
                    #endregion
                }
                #endregion
            }

            //问题单:102344   add by fish 2016.4.8
            if ("O".Equals(newStatus) && !"I".Equals(rv_type))
            {
                //问题单：105739   add by fish 2016/6/24 批次號碼欄位如果為空：1.警衛APP ：不能入廠、出廠2.倉庫封櫃APP：不能封櫃
                if (string.IsNullOrEmpty(bat_no))
                    return ToMessage(string.Format(@Resources.Locale.L_ApiController_Controllers_35), false);

                #region 货柜 封条号 必输确认
                if (!string.IsNullOrEmpty(tranType))
                {
                    Dictionary<string, string> tranTypeMap = new Dictionary<string, string>();
                    tranTypeMap["F"] = "FCL";
                    //tranTypeMap["L"] = "LCL";
                    //tranTypeMap["A"] = "空运";
                    tranTypeMap["T"] = @Resources.Locale.L_DNManage_Domestic;
                    tranTypeMap["R"] = @Resources.Locale.L_DNManage_Rail;
                    if (tranTypeMap.ContainsKey(tranType))//一定需要封条号的
                    {
                        if (string.IsNullOrEmpty(oldSealNo1))
                        {
                            return ToMessage(string.Format(@Resources.Locale.L_Api_Controllers_136, tranTypeMap[tranType]), false);
                        }
                    }

                    Dictionary<string, string> conMap = new Dictionary<string, string>();
                    conMap["F"] = "FCL";
                    conMap["R"] = @Resources.Locale.L_DNManage_Rail;
                    if (conMap.ContainsKey(tranType))//一定需要封条号的
                    {
                        if (string.IsNullOrEmpty(oldCntrNo))
                        {
                            return ToMessage(string.Format(@Resources.Locale.L_Api_Controllers_137, conMap[tranType]), false);
                        }
                    }
                }
                #endregion
            }

            string inOutMsg = UpdateUpdateInOutStatus(wh, ml, rv_type, call_type, reserve_no, newStatus, CompanyId);
            if (!string.IsNullOrEmpty(inOutMsg))
                return ToMessage(inOutMsg, false);

            ml.Add(ei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                try
                {
                    string sm_id = string.Empty, pol_cd = string.Empty, pol_name = string.Empty;
                    if (("O".Equals(newStatus) || "I".Equals(newStatus) || "P".Equals(newStatus)) && rv_type != "I")
                    {
                        DataTable dt = new DataTable();
                        if (bat_no == "")
                        {
                            dt = OperationUtils.GetDataTable(string.Format("SELECT U_ID,POL_CD,POL_NAME FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIRV WHERE U_ID={0})", SQLUtils.QuotedStr(id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        else
                        {
                            dt = OperationUtils.GetDataTable(string.Format("SELECT U_ID,POL_CD,POL_NAME FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIRV WHERE BAT_NO={0})", SQLUtils.QuotedStr(bat_no)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }


                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow item in dt.Rows)
                            {
                                sm_id = Prolink.Math.GetValueAsString(item["U_ID"]);
                                pol_cd = Prolink.Math.GetValueAsString(item["POL_CD"]);
                                pol_name = Prolink.Math.GetValueAsString(item["POL_NAME"]);
                                switch (newStatus)
                                {
                                    case "O"://离场进行转订舱
                                        if (!string.IsNullOrEmpty(sm_id))
                                        {
                                            BookingParser bp = new BookingParser();
                                            bp.SaveToTracking(sm_id);
                                            Business.ReserveManage.updateShipmentStatus(sm_id, "O");
                                            TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ContainerNo = oldCntrNo, JobNo = sm_id, StsCd = "100", StsDescp = "Leave Factory", Location = pol_cd, LocationName = pol_name, Sender = this.UserId });
                                        }
                                        setLeaveGate(id); // 清空該月台
                                        break;
                                    case "P"://
                                        if (!string.IsNullOrEmpty(sm_id))
                                        {
                                            Business.ReserveManage.updateShipmentStatus(sm_id, "P");
                                            TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ContainerNo = (string.IsNullOrEmpty(cntr_no) ? oldCntrNo : cntr_no), JobNo = sm_id, StsCd = "040", StsDescp = "Close Container", Location = pol_cd, LocationName = pol_name, Sender = this.UserId });
                                        }
                                        break;
                                    case "I":
                                        if (!string.IsNullOrEmpty(sm_id))
                                        {
                                            Business.ReserveManage.updateShipmentStatus(sm_id, "I");
                                            TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ContainerNo = (string.IsNullOrEmpty(cntr_no) ? oldCntrNo : cntr_no), JobNo = sm_id, StsCd = "035", StsDescp = "In Factory", Location = pol_cd, LocationName = pol_name, Sender = this.UserId });
                                        }
                                        break;
                                }
                            }

                        }

                    }
                }
                catch (Exception e)
                {
                }

                UpdateInboundSMSMStatus(id, rv_type, "", newStatus);
            }
            catch (Exception ex)
            {
                flag = false;
                message = @Resources.Locale.L_ApiController_Controllers_38;
                //message = ex.ToString();
            }
            if (oth_msg != null && oth_msg.Count > 0)
                message += " (" + @Resources.Locale.L_ApiController_Controllers_39 + string.Join(";", oth_msg) + ")";
            return ToMessage(message, flag);
            //return Json(new { message = message, flag = flag });
        }

        private static DataTable GetSMRVData(string id)
        {
            string sql = string.Format("SELECT * FROM SMIRV WHERE U_ID={0}", SQLUtils.QuotedStr(id));
            DataTable smrv = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return smrv;
        }

        private static string SetEiValue(EditInstruct ei, string name, string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                val = Prolink.Math.GetValueAsString(HttpUtility.UrlDecode(val)).Trim();
                if (!string.IsNullOrEmpty(val))
                    ei.Put(name, val);
            }
            return val;
        }

        [NonAction]
        private ActionResult Gate(string status)
        {
            string sql = string.Format("SELECT (SELECT TOP 1 SMSM.RELEASE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMIRV.SHIPMENT_ID AND RELEASE_NO IS NOT NULL) AS RELEASE_NO,* FROM SMIRV WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(status));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            ExchangeDT(dt, mode);
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
        }

        public ActionResult Lookup()
        {
            //BSTRUCKC  BSTRUCKD
            string type = Request["type"];
            string trucker = Request["Trucker"];
            string table = string.Empty;
            string filter = string.Empty;
            string val = Prolink.Math.GetValueAsString(Request["val"]);
            switch (type)
            {
                case "0"://车牌
                    table = "SELECT TRUCK_NO,DRIVER_NAME FROM BSTRUCKC";
                    filter = string.Format(" AND (TRUCK_NO LIKE {0} OR DRIVER_NAME LIKE {0})", SQLUtils.QuotedStr("%" + val + "%"));
                    break;
                case "1"://司机
                    table = "SELECT DRIVER_NAME,DRIVER_PHONE FROM BSTRUCKD";
                    filter = string.Format(" AND (DRIVER_NAME LIKE {0} OR DRIVER_PHONE LIKE {0})", SQLUtils.QuotedStr("%" + val + "%"));
                    break;
            }
            string sql = string.Format("{0} WHERE GROUP_ID={1} AND PARTY_NO={2} {3}", table, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(trucker), filter);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt));
        }
        #endregion

        public ActionResult ToMessage(string message, bool flag)
        {
            Dictionary<string, object> json = new Dictionary<string, object>();
            json["message"] = message;
            json["flag"] = flag;
            return ToContent(json);
        }
        /// <summary>
        /// 断点续传
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadFile()
        {
            //string path = Prolink.Web.Context.GetConfigValue("tempPath");
            string path = Server.MapPath("~/FileUploads/");
            string fileType = Server.UrlDecode(Request.Headers["fileType"]);//上传的文件名
            string file = Server.UrlDecode(Request.Headers["filePath"]);//上传的文件名
            string docId = Server.UrlDecode(Request.Headers["fileName"]);
            string jobNo = Server.UrlDecode(Request.Headers["job_no"]);
            string pos = Server.UrlDecode(Request.Headers["pos"]);
            string fl = Server.UrlDecode(Request.Headers["fl"]);//文件总长度
            //string index = Server.UrlDecode(Request.Headers["index"]);
            string wh = Server.UrlDecode(Request.Headers["wh"]);
            string status = Server.UrlDecode(Request.Headers["Status"]);
            #region 生成文件路径
            path = System.IO.Path.Combine(path, status);
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    path = Server.MapPath("~/FileUploads/");
                }
            }
            #endregion
            //OperationUtils.Logger.WriteLog("UploadFile Status:" + status);
            string remark = string.Empty;
            switch (status)
            {
                case "C":
                case "R":
                    remark = "IN";
                    break;
                case "P":
                case "U":
                    remark = "OUT";
                    break;
                case "I":
                    remark = "FPW";
                    break;
            }

            Dictionary<string, object> json = new Dictionary<string, object>();
            json["msg"] = "";

            int pos_ln = 0;
            int f_ln = 0;
            int.TryParse(pos, out pos_ln);
            int.TryParse(fl, out f_ln);

            try
            {
                System.IO.Stream sm = Request.InputStream;//获取post正文
                int len = (int)sm.Length;//post数据长度
                byte[] inputByts = new byte[len];//字节数据,用于存储post数据
                sm.Read(inputByts, 0, len);//将post数据写入byte数组中
                sm.Close();//关闭IO流

                string filepath = path;
                if (!Directory.Exists(filepath))
                    Directory.CreateDirectory(filepath);
                bool del = false;
                if (string.IsNullOrEmpty(docId))
                {
                    docId = remark + DateTime.Now.ToString("yyMMddHHmmfff") + System.IO.Path.GetExtension(file);
                    del = true;
                }

                filepath = Path.Combine(new string[] { path, docId });
                if (del && System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);

                using (FileStream fs = new FileStream(filepath, FileMode.Append))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        //byte[] b = { 1, 2, 3, 4 };
                        bw.Write(inputByts);
                    }
                }

                if (pos_ln >= f_ln && pos_ln > 0)//文件上传完毕
                {
                    //FileInfo fi = new FileInfo(filepath);
                    using (FileStream fs = new FileStream(filepath, FileMode.Open))
                    {
                        if (fs.Length == f_ln)//检查文件是否有丢失
                        {
                            json["c"] = "Y";
                        }
                        else
                        {
                            json["c"] = "N";
                            json["msg"] = @Resources.Locale.L_ApiController_Controllers_40;
                        }
                    }
                    if ("Y".Equals(json["c"]))//上传完成
                    {
                        DataTable smrvDt = null;

                        #region 验证是否允许上传pod
                        if ("POD".Equals(fileType) || "POD".Equals(status))//进口 第一次上传POD
                        {
                            string msg = string.Empty;
                            //預約單沒GATE IN就不能上傳POD顯示錯誤訊息：No gate in information for container/DN{container#/DN#} , you cannot upload POD. Pls check with Security for help.
                            DataTable dt = null;
                            if ("IN_WH_APP".Equals(status))
                            {
                                smrvDt = OperationUtils.GetDataTable(string.Format("SELECT RESERVE_NO,SHIPMENT_ID,SHIPMENT_INFO,CALL_TYPE FROM SMIRV WITH(NOLOCK) WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (smrvDt.Rows.Count <= 0)
                                {
                                    msg = "No gate in information for container/DN, you cannot upload POD. Pls check with Security for help";
                                }
                                else
                                {
                                    string reserve_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["RESERVE_NO"]);
                                    string call_type = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CALL_TYPE"]);
                                    //string gateOutSQL = string.Format("SELECT IDATE,CNTR_NO,DN_NO FROM SMRCNTR WHERE WS_CD={0} AND SMRCNTR.RESERVE_NO={1}", SQLUtils.QuotedStr(wh), SQLUtils.QuotedStr(reserve_no));
                                    //if ("D".Equals(call_type))
                                    //    gateOutSQL = string.Format("SELECT IDATE,'' AS CNTR_NO,DN_NO FROM SMRDN WHERE WS_CD={0} AND SMRDN.RESERVE_NO={1}", SQLUtils.QuotedStr(wh), SQLUtils.QuotedStr(reserve_no));

                                    dt = PODUploadHandle.GetSMRVDetail(wh, reserve_no, call_type);
                                    if (dt.Rows.Count <= 0)
                                    {
                                        msg = "No gate in information for container/DN, you cannot upload POD. Pls check with Security for help";
                                    }
                                    else if (dt.Select(string.Format("OUTER_FLAG='Y'")).Length > 0)
                                    {
                                        //2-1-2. 當預約單明細的Delivery Address是外倉(檢查卡車送貨點建檔中的常用地址中的Third Party WH=Y)，可以不用Gate In(不用判斷IGATE是否有料)
                                        msg = string.Empty;
                                        //ADDR_CODE,ADDR,WS_CD
                                        remark = string.Format("{0},{1},{2}", dt.Rows[0]["ADDR_CODE"], dt.Rows[0]["ADDR"], dt.Rows[0]["WS_CD"]);
                                    }
                                    else if (dt.Select(string.Format("IDATE IS NOT NULL")).Length <= 0)
                                    {
                                        msg = "No gate in information for container/DN{" + Prolink.Math.GetValueAsString(dt.Rows[0]["CNTR_NO"]) + "/" + Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]) + "} , you cannot upload POD. Pls check with Security for help";
                                    }
                                }
                            }
                            else
                            {
                                dt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,STATUS,IN_DATE,CNTR_NO,DN_NO FROM SMIRV WITH(NOLOCK) WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM TKBL WITH(NOLOCK) WHERE U_ID={0}) AND RV_TYPE='I' AND SHIPMENT_ID IS NOT NULL", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (dt.Rows.Count <= 0)
                                {
                                    msg = "No gate in information for container/DN, you cannot upload POD. Pls check with Security for help";
                                }
                                else if (dt.Select(string.Format("IN_DATE IS NOT NULL")).Length <= 0)
                                {
                                    msg = "No gate in information for container/DN{" + Prolink.Math.GetValueAsString(dt.Rows[0]["CNTR_NO"]) + "/" + Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]) + "} , you cannot upload POD. Pls check with Security for help";
                                }
                            }

                            if (!string.IsNullOrEmpty(msg))
                            {
                                json["c"] = "N";
                                json["msg"] = msg;
                                json["f"] = docId;
                                try
                                {
                                    System.IO.File.Delete(filepath);
                                }
                                catch { }
                                return ToContent(json);
                            }
                        }
                        #endregion

                        DataTable smDt = null;
                        string group_id = string.Empty, cmp = string.Empty;
                        EDOCApi.EDOCFileItem item = null;
                        switch (status)
                        {
                            case "IN_WH_APP"://inbound仓库app 上传文件
                                json = UploadInboundSMRVStatus(jobNo, fileType, filepath, wh, remark);
                                break;
                            case "EXPP"://问题单:119358     .要多一個功能，上傳異常照片，EDOC TYPE=EXPP 
                                smDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (smDt.Rows.Count > 0)
                                {
                                    group_id = Prolink.Math.GetValueAsString(smDt.Rows[0]["GROUP_ID"]);
                                    cmp = (Prolink.Math.GetValueAsString(smDt.Rows[0]["O_LOCATION"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_LOCATION"]) : string.Empty;
                                    string docJobNo = (Prolink.Math.GetValueAsString(smDt.Rows[0]["O_UID"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_UID"]) : string.Empty;
                                    if (string.IsNullOrEmpty(cmp))
                                        cmp = Prolink.Math.GetValueAsString(smDt.Rows[0]["CMP"]);
                                    if (string.IsNullOrEmpty(docJobNo))
                                        docJobNo = jobNo;

                                    item = EDOCController.UploadFile2EDOC(docJobNo, filepath, group_id, cmp, "*", UserId, "EXPP", "EXPP");
                                    if (item == null)
                                    {
                                        json["c"] = "N";
                                        json["msg"] = @Resources.Locale.L_ApiController_Controllers_40;
                                    }
                                }
                                break;
                            case "POD":
                                smDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM TKBL WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (smDt.Rows.Count > 0)
                                {
                                    group_id = Prolink.Math.GetValueAsString(smDt.Rows[0]["GROUP_ID"]);
                                    cmp = (Prolink.Math.GetValueAsString(smDt.Rows[0]["O_LOCATION"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_LOCATION"]) : string.Empty;
                                    string docJobNo = (Prolink.Math.GetValueAsString(smDt.Rows[0]["O_UID"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_UID"]) : string.Empty;
                                    if (string.IsNullOrEmpty(cmp))
                                        cmp = Prolink.Math.GetValueAsString(smDt.Rows[0]["CMP"]);
                                    if (string.IsNullOrEmpty(docJobNo))
                                        docJobNo = jobNo;

                                    item = EDOCController.UploadFile2EDOC(docJobNo, filepath, group_id, cmp, "*", UserId, "POD", "POD");
                                    if (item == null)
                                    {
                                        json["c"] = "N";
                                        json["msg"] = @Resources.Locale.L_ApiController_Controllers_40;
                                    }
                                    else
                                    {
                                        DateTime podDate = DateTime.Now;
                                        TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status()
                                        {
                                            ShipmentId = Prolink.Math.GetValueAsString(smDt.Rows[0]["SHIPMENT_ID"]),
                                            StsCd = "POD",
                                            Sender = UserId,
                                            EventTime = podDate.ToString("yyyyMMddHHmmss"),
                                            Location = Prolink.Math.GetValueAsString(smDt.Rows[0]["POD_CD"]),
                                            LocationName = Prolink.Math.GetValueAsString(smDt.Rows[0]["POD_NAME"]),
                                            StsDescp = "POD"
                                        });

                                        #region SMSMI.STATUS-這是進口訂艙管理的狀態   SMIRV.STATUS-這是預約單主檔的狀態   然後同時回寫這個SHipment存在的預約單狀態變成U+Shipment的狀態要變成P
                                        //MixedList mli = new MixedList();
                                        //EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                                        //ei.PutKey("SHIPMENT_ID", Prolink.Math.GetValueAsString(smDt.Rows[0]["SHIPMENT_ID"]));
                                        ////ei.PutKey("CMP", cmp);
                                        ////ei.Put("STATUS", "P");
                                        //ei.PutDate("POD_MDATE", podDate);
                                        //mli.Add(ei);

                                        //ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                                        //ei.Condition = string.Format(" SHIPMENT_INFO IS NOT NULL AND SHIPMENT_INFO LIKE {0} AND RV_TYPE='I'", SQLUtils.QuotedStr("%" + Prolink.Math.GetValueAsString(smDt.Rows[0]["SHIPMENT_ID"]) + "%"));

                                        ////ei.PutKey("SHIPMENT_ID", Prolink.Math.GetValueAsString(smDt.Rows[0]["SHIPMENT_ID"]));
                                        //ei.PutKey("RV_TYPE", "I");
                                        //ei.Put("STATUS", "U");
                                        ////ei.PutDate("POD_MDATE", podDate);
                                        //mli.Add(ei);
                                        //OperationUtils.ExecuteUpdate(mli, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        #endregion
                                    }
                                }

                                UpdateInboundSMSMStatus(jobNo, "I", "BL");
                                break;
                            default:
                                #region 警卫仓库系统上传
                                smrvDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,TRUCK_CNTRNO,TRUCK_NO,LTRUCK_NO,BAT_NO,RV_TYPE FROM SMIRV WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (smrvDt.Rows.Count > 0)
                                {
                                    string bat_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["BAT_NO"]);

                                    if (string.IsNullOrEmpty(bat_no))
                                        smDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,U_ID,GROUP_ID,CMP,O_LOCATION,O_UID FROM SMSMI WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIRV WHERE U_ID={0})", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    else
                                    {
                                        smrvDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,TRUCK_CNTRNO,TRUCK_NO,LTRUCK_NO,BAT_NO,RV_TYPE FROM SMIRV WHERE BAT_NO={0}", SQLUtils.QuotedStr(bat_no)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        smDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,U_ID,GROUP_ID,CMP,O_LOCATION,O_UID FROM SMSMI WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIRV WHERE BAT_NO={0})", SQLUtils.QuotedStr(bat_no)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    }
                                    //string sm_id = OperationUtils.GetValueAsString(string.Format("SELECT U_ID FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMIRV WHERE U_ID={0})", SQLUtils.QuotedStr(jobNo)), Prolink.Web.WebContext.GetInstance().GetConnection());
                                    //if (smDt.Rows.Count > 0)
                                    //{
                                    foreach (DataRow smdr in smDt.Rows)
                                    {
                                        //string sm_id = Prolink.Math.GetValueAsString(smdr["U_ID"]);
                                        group_id = Prolink.Math.GetValueAsString(smdr["GROUP_ID"]);
                                        //cmp = Prolink.Math.GetValueAsString(smdr["CMP"]);
                                        cmp = (Prolink.Math.GetValueAsString(smdr["O_LOCATION"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_LOCATION"]) : string.Empty;
                                        string sm_id = (Prolink.Math.GetValueAsString(smdr["O_UID"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_UID"]) : string.Empty;
                                        if (string.IsNullOrEmpty(cmp))
                                            cmp = Prolink.Math.GetValueAsString(smdr["CMP"]);
                                        if (string.IsNullOrEmpty(sm_id))
                                            sm_id = Prolink.Math.GetValueAsString(smdr["U_ID"]);
                                        string shipment_id = Prolink.Math.GetValueAsString(smdr["SHIPMENT_ID"]);
                                        string rv_type = string.Empty;
                                        string truck_cntrno = string.Empty, truck_no = string.Empty, ltruck_no = string.Empty;
                                        DataRow[] drs = smrvDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id)));
                                        if (drs.Length > 0)
                                        {
                                            truck_cntrno = Prolink.Math.GetValueAsString(drs[0]["TRUCK_CNTRNO"]);
                                            truck_no = Prolink.Math.GetValueAsString(drs[0]["TRUCK_NO"]);
                                            ltruck_no = Prolink.Math.GetValueAsString(drs[0]["LTRUCK_NO"]);
                                            rv_type = Prolink.Math.GetValueAsString(drs[0]["RV_TYPE"]);
                                        }
                                        string docType = "SECPIC";
                                        switch (status)
                                        {
                                            case "C":
                                            case "R":
                                            case "E":
                                                if ("I".Equals(rv_type))
                                                    docType = "SECPICI";
                                                remark = string.Format("{0}-{1}-{2}", "IN", bat_no, string.IsNullOrEmpty(truck_cntrno) ? truck_no : truck_cntrno);
                                                break;
                                            case "P":
                                            case "U":
                                                if ("I".Equals(rv_type))
                                                    docType = "SECPICO";
                                                remark = string.Format("{0}-{1}-{2}", "OUT", bat_no, string.IsNullOrEmpty(truck_cntrno) ? ltruck_no : truck_cntrno);
                                                break;
                                        }
                                        if (!string.IsNullOrEmpty(sm_id))
                                        {
                                            item = EDOCController.UploadFile2EDOC(sm_id, filepath, group_id, cmp, "*", UserId, docType, remark);
                                            if (item == null)
                                            {
                                                json["c"] = "N";
                                                json["msg"] = @Resources.Locale.L_ApiController_Controllers_40;
                                            }
                                        }
                                        else
                                        {
                                            json["c"] = "N";
                                            json["msg"] = @Resources.Locale.L_Api_Controllers_126;
                                        }
                                    }
                                }
                                break;
                                #endregion
                        }
                    }
                }
            }
            catch (Exception e)
            {
                json["c"] = "N";
                json["msg"] = e.Message;
            }
            json["f"] = docId;
            return ToContent(json);
        }

        /// <summary>
        /// 清空月台
        /// </summary>
        /// <returns></returns>
        public bool setLeaveGate(string uid)
        {
            string returnMessage = "success";
            string now = DateTime.Now.ToString();
            MixedList mixList = new MixedList();
            EditInstruct ei;
            if (uid == "")
            {
                return false;
            }
            string sql = "SELECT TOP 1 RESERVE_NO FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            string ReserveNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "SELECT TOP 1 * FROM SMIRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string BatNo = Prolink.Math.GetValueAsString(item["BAT_NO"]);

                    sql = "UPDATE SMWHGT SET CNTR_NO='',RESERVE_NO='' WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND RESERVE_NO=" + SQLUtils.QuotedStr(BatNo);
                    mixList.Add(sql);
                    ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("BAT_NO", BatNo);
                    ei.Put("STATUS", "O");
                    mixList.Add(ei);

                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.Message;
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取货况主表信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCargoData()
        {
            string eta = Request["eta"];
            string etd = Request["etd"];
            string no = Request["no"];
            string no1 = Request["no1"];
            string cd = Request["cd"];

            string allEta = Request["allEta"];
            string allEtd = Request["allEtd"];

            string condition = "";
            if (!string.IsNullOrEmpty(no))
            {
                condition += string.Format(" AND (MASTER_NO LIKE {0}", SQLUtils.QuotedStr("%" + no + "%"));
                condition += string.Format(" OR SHIPMENT_ID LIKE {0}", SQLUtils.QuotedStr("%" + no + "%"));
                condition += string.Format(" OR HOUSE_NO LIKE {0}", SQLUtils.QuotedStr("%" + no + "%"));
                condition += string.Format(" OR REF_NO LIKE {0}", SQLUtils.QuotedStr("%" + no + "%"));
                //condition += string.Format(" OR EXISTS(SELECT 1 FROM SMSM WITH (NOLOCK) WHERE SMSM.SHIPMENT_ID=TKBL.SHIPMENT_ID AND DN_NO LIKE {0})", SQLUtils.QuotedStr("%" + no1 + "%"));

                condition += string.Format(" OR SO_NO LIKE {0})", SQLUtils.QuotedStr("%" + no + "%"));
            }

            if (!string.IsNullOrEmpty(no1))
            {
                condition += string.Format(" AND (EXISTS(SELECT 1 FROM TKBLCNTR WITH (NOLOCK) WHERE JOB_NO=TKBL.U_ID AND (CNTR_NO LIKE {0} OR SEAL_NO1 LIKE {0}))", SQLUtils.QuotedStr("%" + no1 + "%"));

                condition += string.Format(" OR VESSEL1 LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += string.Format(" OR VESSEL2 LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += string.Format(" OR VESSEL3 LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += string.Format(" OR VESSEL LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += string.Format(" OR VOYAGE1 LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += string.Format(" OR VOYAGE2 LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += string.Format(" OR VOYAGE3 LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += string.Format(" OR VOYAGE LIKE {0}", SQLUtils.QuotedStr("%" + no1 + "%"));

                condition += string.Format(" OR EXISTS(SELECT 1 FROM TKBLST WITH (NOLOCK) WHERE TKBLST.U_ID=TKBL.U_ID AND TKBLST.STS_CD={0})", SQLUtils.QuotedStr(no1));

                condition += string.Format(" OR EXISTS(SELECT 1 FROM TKBLST WITH (NOLOCK) WHERE TKBLST.U_ID=TKBL.U_ID AND TKBLST.REMARK LIKE {0})", SQLUtils.QuotedStr("%" + no1 + "%"));
                condition += ")";
            }

            if (!string.IsNullOrEmpty(cd))
            {
                string[] cds = cd.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                List<string> cdList = new List<string>();
                foreach (var c in cds)
                {
                    cdList.Add(SQLUtils.QuotedStr(c.Split(new string[] { ":" }, StringSplitOptions.None)[0].Trim()));
                }
                if (cdList.Count > 0)
                    condition += string.Format(" AND CSTATUS IN ({0})", string.Join(",", cdList));
                //condition += string.Format(" AND EXISTS(SELECT 1 FROM TKBLST WITH (NOLOCK) WHERE TKBLST.U_ID=TKBL.U_ID AND TKBLST.STS_CD={0})", SQLUtils.QuotedStr(cd));
            }

            if (!string.IsNullOrEmpty(eta))
            {
                DateTime etaTime = GetDate(eta);
                if (etaTime.CompareTo(new DateTime(2000, 1, 1)) > 0)
                    condition += string.Format(" AND ETA>={0} ", SQLUtils.QuotedStr(etaTime.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            if (!string.IsNullOrEmpty(etd))
            {
                DateTime etdTime = GetDate(etd);
                if (etdTime.CompareTo(new DateTime(2000, 1, 1)) > 0)
                    condition += string.Format(" AND ETD>={0} ", SQLUtils.QuotedStr(etdTime.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            if ("Y".Equals(allEtd))
            {
                condition = " AND CSTATUS>='4'";
            }
            else if ("Y".Equals(allEta))
            {
                condition = " AND CSTATUS>='5'";
            }

            string table = "(SELECT * FROM (SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + condition
                + string.Format(" AND EXISTS (SELECT 1 FROM TKBLPT WITH (NOLOCK) WHERE TKBLPT.U_ID=TKBL.U_ID AND TKBLPT.PARTY_NO={0})", SQLUtils.QuotedStr(this.CompanyId)) + ") S";
            table += " UNION SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + condition + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") M";

            return GetBootstrapData(table, string.Empty);
        }

        private static DateTime GetDate(string eta)
        {
            eta = eta.Replace("/", "").Replace("-", "").Replace(":", "").Replace(" ", "");
            if (eta.Length <= 8)
                eta += "000000";
            DateTime now = Prolink.Utils.FormatUtils.ParseDateTime(eta, "yyyyMMddHHmmss");
            return now;
        }


        /// <summary>
        /// 获取货况汇总明细
        /// </summary>
        void GetStatusInfo()
        {
            string filter = string.Empty;
            //收到的所有货况
            string sql = string.Format("SELECT * FROM (SELECT RQ_CD,CARRIER,COUNT(1) AS _COUNT FROM TKRQM WHERE 1=1{0} GROUP BY RQ_CD,CARRIER)T ORDER BY _COUNT DESC", filter);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            dt.Columns.Add("STATUS_COUNT", typeof(int));
            dt.Columns.Add("SEND_ERROR_COUNT", typeof(int));
            dt.Columns.Add("NOSTATUS_COUNT", typeof(int));
            dt.Columns.Add("UNKNOW_COUNT", typeof(int));
            //有货况的
            sql = string.Format("SELECT RQ_CD,CARRIER,COUNT(1) AS _COUNT FROM TKRQM WHERE UPDATE_STATUS_DATE IS NOT NULL{0} GROUP BY RQ_CD,CARRIER", filter);
            DataTable dt1 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            //回传异常
            sql = string.Format("SELECT RQ_CD,CARRIER,COUNT(1) AS _COUNT FROM TKRQM WHERE SEND_MSG IS NOT NULL{0} GROUP BY RQ_CD,CARRIER", filter);
            DataTable dt2 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            //有开发的无货况
            sql = string.Format("SELECT RQ_CD,CARRIER,COUNT(1) AS _COUNT FROM TKRQM WHERE UPDATE_STATUS_DATE IS NULL AND ERROR_MSG NOT LIKE '%无对应的EDI%'{0} GROUP BY RQ_CD,CARRIER", filter);
            DataTable dt3 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            //无开发的
            sql = string.Format("SELECT RQ_CD,CARRIER,COUNT(1) AS _COUNT FROM TKRQM WHERE ERROR_MSG LIKE '%无对应的EDI%'{0} GROUP BY RQ_CD,CARRIER", filter);
            DataTable dt4 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            SetStatusInfoData(dt, dt1, "STATUS_COUNT");
            SetStatusInfoData(dt, dt2, "SEND_ERROR_COUNT");
            SetStatusInfoData(dt, dt3, "NOSTATUS_COUNT");
            SetStatusInfoData(dt, dt4, "UNKNOW_COUNT");
        }

        private static void SetStatusInfoData(DataTable destDt, DataTable orgDt, string filed)
        {
            foreach (DataRow dr in orgDt.Rows)
            {
                string rq_cd = Prolink.Math.GetValueAsString(dr["RQ_CD"]);
                string carrier = Prolink.Math.GetValueAsString(dr["CARRIER"]);
                string f = string.Format("RQ_CD={0} AND CARRIER={1}", SQLUtils.QuotedStr(rq_cd), SQLUtils.QuotedStr(carrier));
                DataRow[] drs = destDt.Select(f);
                if (drs.Length > 0)
                    drs[0][filed] = dr["_COUNT"];
            }
        }

        public ActionResult GetEtaEtdCount()
        {
            string table = "(SELECT * FROM (SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + "{0}"
                + string.Format(" AND EXISTS (SELECT 1 FROM TKBLPT WITH (NOLOCK) WHERE TKBLPT.U_ID=TKBL.U_ID AND TKBLPT.PARTY_NO={0})", SQLUtils.QuotedStr(this.CompanyId)) + ") S";
            table += " UNION SELECT * FROM TKBL WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + "{0}" + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + ") M";
            string sql = string.Format("SELECT COUNT(1) AS DATA_COUNT,'ATD' AS CD FROM " + table, " AND CSTATUS>='4'");
            sql += string.Format(" UNION SELECT COUNT(1) AS DATA_COUNT,'ATA' AS CD FROM " + table, " AND CSTATUS>='5'");
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "TkblModel"));
        }

        public ActionResult GetStatusType()
        {
            //string sql = "SELECT DISTINCT STS_CD,EDESCP FROM TKSTSCD ORDER BY STS_CD";
            string sql = "SELECT * from (SELECT distinct CD AS STS_CD,CD_DESCP AS EDESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='TKLC') T ORDER BY ORDER_BY";

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "TkstscdModel"));
        }

        public ActionResult GetCntrNoBySMRV()
        {
            string u_id = Request["id"];
            string batNo = Request["batNo"];
            string sql = string.Format("SELECT DN_NO,SEAL_SATAUE FROM SMDN WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMIRV WHERE BAT_NO={0}) AND (SEAL_SATAUE<>'Y' OR SEAL_SATAUE IS NULL)", SQLUtils.QuotedStr(batNo));
            if (string.IsNullOrEmpty(batNo))
                sql = string.Format("SELECT DN_NO,SEAL_SATAUE FROM SMDN WHERE SHIPMENT_ID IN( SELECT SHIPMENT_ID FROM SMIRV WHERE BAT_NO IN(SELECT BAT_NO FROM SMIRV WHERE U_ID={0})) AND (SEAL_SATAUE<>'Y' OR SEAL_SATAUE IS NULL)", SQLUtils.QuotedStr(u_id));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "SmdnModel"));
        }

        public ActionResult GetSWHType()
        {
            string sql = string.Format("SELECT WHS FROM SYS_ACCT WHERE U_ID={0} AND GROUP_ID={1} AND CMP={2}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            string wh = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT WS_CD,WS_NM FROM SYS_ACCT_WH WHERE USER_ID={0} AND GROUP_ID={1} AND U_CMP={2}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            if (string.IsNullOrEmpty(wh))
            {
                sql += string.Format(" UNION SELECT WS_CD,WS_NM FROM SMWH WHERE GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            }
            else
            {
                string[] whs = wh.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                sql += string.Format(" UNION SELECT WS_CD,WS_NM FROM SMWH WHERE WS_CD IN {0} AND GROUP_ID={1} AND CMP={2}", SQLUtils.Quoted(whs), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            }
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "SmwhModel"));
        }

        /// <summary>
        /// 问题单:120616  add by fish 2017/05/25
        /// 當車輛貨車進場時根據SMIRV.CALL_TYPE回寫進場時間到SMRCNTR(SMRDN).IDATE、並回寫到SMSV狀態為進場 
        /// 貨車離場時間時根據SMIRV.CALL_TYPE回寫離場時間到SMRCNTR(SMRDN).LDATE，並回寫到SMSV狀?為離場
        /// </summary>
        /// <param name="wh"></param>
        /// <param name="ml"></param>
        /// <param name="rv_type"></param>
        /// <param name="call_type"></param>
        /// <param name="release_no"></param>
        /// <param name="newStatus"></param>
        private static string UpdateUpdateInOutStatus(string wh, MixedList ml, string rv_type, string call_type, string reserve_no, string newStatus, string CompanyId)
        {
            //if (string.IsNullOrEmpty(wh) || string.IsNullOrEmpty(reserve_no) || !"I".Equals(rv_type))
            if (string.IsNullOrEmpty(reserve_no) || !"I".Equals(rv_type))
                return string.Empty;

            string table = "";
            string field = "";
            string field1 = "";
            switch (call_type)
            {
                case "D":
                    table = "SMRDN";
                    break;
                default:
                    table = "SMRCNTR";
                    break;
            }

            switch (newStatus)
            {
                case "I":
                    field = "IDATE";
                    field1 = "IDATE_L";
                    break;
                case "O":
                case "Z":
                    field = "LDATE";
                    field1 = "LDATE_L";
                    break;
            }
            if (!string.IsNullOrEmpty(field))
            {
                //DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT {0} FROM {1} WHERE RESERVE_NO={2} AND WS_CD={3}", field, table, SQLUtils.QuotedStr(reserve_no), SQLUtils.QuotedStr(wh)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //if (dt.Select(string.Format("{0} IS NOT NULL", field)).Length > 0)
                //{
                //    switch (newStatus)
                //    {
                //        case "I":
                //            return "不能重复入同一仓库";
                //        case "O":
                //            return "不能重复出同一仓库";
                //    }
                //    return "操作失败";
                //}
                EditInstruct ei = new EditInstruct(table, EditInstruct.UPDATE_OPERATION);
                ei.PutKey("RESERVE_NO", reserve_no);
                //ei.PutKey("WS_CD", wh);
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                ei.PutDate(field, odt);
                ei.PutDate(field, ndt);

                ml.Add(ei);
            }
            return string.Empty;
        }

        /// <summary>
        /// 當一個Shipment中所有預約單有1或多個在進廠且其他都在靠月台orPODor出廠時，Shipment的狀態應該要更新成進廠
        /// 當一個Shipment中所有預約單有1個或多個是POD且其他都是出廠時，Shipment的狀態更新成POD
        /// 當一個Shipment中所有預約單所有都出廠，Shpment的狀態會更新成Finish
        /// 更新inbound状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rv_type"></param>
        public static void UpdateInboundSMSMStatus(string id, string rv_type, string type = "", string newStatus = "")
        {
            PODUploadHandle.UpdateInboundSMSMStatus(id, rv_type, type, newStatus);
        }

        /// <summary>
        /// 检查是否可以上传POD
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckPODUpload()
        {
            string reserve_no = Request["id"];
            string call_type = Request["call_type"];
            Dictionary<string, string> json = new Dictionary<string, string>();
            json["flag"] = "Y";

            if (!string.IsNullOrEmpty(reserve_no))
            {
                DataTable dt = PODUploadHandle.GetSMRVDetail(Request["wh"], reserve_no, call_type);

                string msg = string.Empty;
                if (dt.Rows.Count <= 0)
                {
                    msg = "No gate in information for container/DN, you cannot upload POD. Pls check with Security for help";
                }
                else if (dt.Select(string.Format("OUTER_FLAG='Y'")).Length > 0)
                {
                    //2-1-2. 當預約單明細的Delivery Address是外倉(檢查卡車送貨點建檔中的常用地址中的Third Party WH=Y)，可以不用Gate In(不用判斷IGATE是否有料)
                    msg = string.Empty;
                }
                else if (dt.Select(string.Format("IDATE IS NOT NULL")).Length <= 0)
                {
                    msg = "No gate in information for container/DN{" + Prolink.Math.GetValueAsString(dt.Rows[0]["CNTR_NO"]) + "/" + Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]) + "} , you cannot upload POD. Pls check with Security for help";
                }

                if (!string.IsNullOrEmpty(msg))
                {
                    json["flag"] = "N";
                    json["msg"] = msg;
                }
            }
            return ToContent(json);
        }

       

      
        public Dictionary<string, object> UploadInboundSMRVStatus(string jobNo, string doc_type, string filepath, string wh, string remark = "", string PodMdate = "")
        {
            MixedList ml = new MixedList();
            List<string> smlist = new List<string>();
            Dictionary<string, object> json = new Dictionary<string, object>();
            json["c"] = "Y";
            json["msg"] = string.Empty;
            DataTable smrvDt = OperationUtils.GetDataTable(string.Format("SELECT CNTR_NO,RESERVE_NO,SHIPMENT_ID,SHIPMENT_INFO,CALL_TYPE,CMP,GROUP_ID FROM SMIRV WITH(NOLOCK) WHERE U_ID={0} AND RV_TYPE='I'", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string reserve_no = string.Empty;
            string call_type= string.Empty;
            foreach (DataRow dr in smrvDt.Rows)
            {
                if (!string.IsNullOrEmpty(wh))
                {
                    reserve_no = Prolink.Math.GetValueAsString(dr["RESERVE_NO"]);
                    call_type = Prolink.Math.GetValueAsString(dr["CALL_TYPE"]);
                    EditInstruct ei = new EditInstruct("D".Equals(call_type) ? "SMRDN" : "SMRCNTR", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("RESERVE_NO", reserve_no);
                    ei.PutKey("WS_CD", wh);
                    
                    string check_name = string.Empty;

                    EditInstruct rvei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                    rvei.PutKey("RESERVE_NO", reserve_no);
                    rvei.PutKey("RV_TYPE", "I");


                    switch (doc_type)
                    {
                        case "SELP":
                            check_name = "SEAL_CHECK";
                            break;
                        case "FLRP":
                            check_name = "FLOOR_CHECK";
                            break;
                        case "POD":
                            check_name = "POD_CHECK";
                            DateTime odt = DateTime.Now;
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            if (!string.IsNullOrEmpty(PodMdate))
                            {
                                try {
                                    ndt = DateTime.ParseExact(PodMdate, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                                    odt = TimeZoneHelper.GetTimeZoneDate(ndt, "CN", CompanyId);
                                }
                                catch {
                                    odt = DateTime.Now;
                                    ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                                }
                            }
                            ei.PutDate("POD_MDATE", odt);
                            ei.PutDate("POD_MDATE_L", ndt);
                            if (InboundHandel.checkPODstatus(reserve_no, jobNo, 1))
                                rvei.Put("STATUS", "U");
                            string nowdate = odt.ToString("yyyy-MM-dd hh:mm:ss");
                            string newdate = ndt.ToString("yyyy-MM-dd hh:mm:ss");
                            string updatesql = string.Format("UPDATE SMICNTR SET SMICNTR.POD_DATE={0}, SMICNTR.POD_DATE_L={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={2}",
                                SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
                            if ("D".Equals(call_type))
                            {
                                updatesql = string.Format("UPDATE SMIDN SET SMIDN.POD_DATE={0}, SMIDN.POD_DATE_L={1} FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={2}",
                               SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
                            }
                            ml.Add(updatesql);
                            rvei.PutDate("POD_UPDATE_DATE", ndt);
                            PODUploadHandle.UpdateSMORDStatusAtPod(ml, rvei, reserve_no,wh,call_type);
                            break;
                        case "EXPP":
                            check_name = "EXPP_CHECK";
                            break;
                        default:
                            continue;
                    }
                    ei.Put(check_name, "Y");
                    ml.Add(ei);

                    rvei.Put(check_name, "Y");
                    ml.Add(rvei);
                }
                string sm_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string sm_info = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
                string[] sms = sm_info.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (!string.IsNullOrEmpty(sm_id) && !smlist.Contains(sm_id))
                    smlist.Add(sm_id);
                foreach (string str in sms)
                {
                    if (!string.IsNullOrEmpty(str) && !smlist.Contains(str))
                        smlist.Add(str);
                }
            }

            if (smlist.Count > 0)
            {
                string cntr_no = smrvDt.Columns.Contains("CNTR_NO") ? Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CNTR_NO"]) : string.Empty;
                if (!string.IsNullOrEmpty(remark))
                    remark += ";Container No:" + cntr_no;
                else
                    remark = cntr_no;
                if (!"POD".Equals(doc_type))
                {
                    string condition = string.Empty;
                    foreach (var str in smlist)
                    {
                        if (!string.IsNullOrEmpty(condition))
                        {
                            condition += ",";
                        }
                        condition += SQLUtils.QuotedStr(str);
                    }
                    DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,U_ID,GROUP_ID,CMP,O_LOCATION,O_UID FROM SMSMI WITH(NOLOCK) WHERE SHIPMENT_ID IN ({0})", condition), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string group_id = string.Empty, cmp = string.Empty;

                    List<string> uploadKeys = new List<string>();
                    foreach (DataRow smdr in smDt.Rows)
                    {
                        group_id = Prolink.Math.GetValueAsString(smdr["GROUP_ID"]);
                        cmp = (Prolink.Math.GetValueAsString(smdr["O_LOCATION"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_LOCATION"]) : string.Empty;
                        string docJobNo = (Prolink.Math.GetValueAsString(smdr["O_UID"]) != "") ? Prolink.Math.GetValueAsString(smDt.Rows[0]["O_UID"]) : string.Empty;
                        if (string.IsNullOrEmpty(cmp))
                            cmp = Prolink.Math.GetValueAsString(smdr["CMP"]);
                        if (string.IsNullOrEmpty(docJobNo))
                            docJobNo = Prolink.Math.GetValueAsString(smdr["U_ID"]);
                        string key = string.Format("{0}#{1}#{2}#{3}", docJobNo, group_id, cmp, doc_type);
                        if (uploadKeys.Contains(key))
                            continue;
                        uploadKeys.Add(key);
                        EDOCFileItem item = EDOCController.UploadFile2EDOC(docJobNo, filepath, group_id, cmp, "*", UserId, doc_type, remark);
                        if (item == null)
                        {
                            json["c"] = "N";
                            json["msg"] = "EDOC Uplaod Fail";
                            ml = new MixedList();
                            break;
                        }
                    }
                }
                else
                {
                    string cmp = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CMP"]);
                    string groupId = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["GROUP_ID"]);
                    int serverNum = 0;
                    bool isSuccess = false;
                    Prolink.EDOC_API _api = new Prolink.EDOC_API();
                    EDOCFileItem fileInfo = null;
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    EDOCController.BatchUplaodFiles(_api, UserId, jobNo, groupId, cmp, doc_type, remark,ref fileInfo, dic, filepath, ref serverNum, ref isSuccess);
                    if (fileInfo == null)
                    {
                        json["c"] = "N";
                        json["msg"] = "EDOC Uplaod Fail";
                        ml = new MixedList();
                    }
                    EDOCController.BatchUploadPOD(_api, UserId, jobNo, groupId, cmp, doc_type, remark, reserve_no, wh, call_type, fileInfo, dic, filepath, ref serverNum, ref isSuccess);
                }
                if (ml.Count > 0)
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //if ("POD".Equals(doc_type))
                    //{
                    //    UpdateInboundSMSMStatus(jobNo, "I", "", "U");
                    //}
                }
            }
            return json;
        }

        private static void UpdateSMRVPod(MixedList ml, string reserve_no)
        {
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RESERVE_NO", reserve_no);
            ei.PutKey("RV_TYPE", "I");
            ei.Put("STATUS", "U");
            ml.Add(ei);
        }

        private string UrlDecode(string name)
        {
            string val = Request[name];
            if (!string.IsNullOrEmpty(val)) val = HttpUtility.UrlDecode(val);
            return val;
        }

        public ActionResult InboundQuery()
        {
            string dnNo = Request["dnNo"];
            if (!string.IsNullOrEmpty(dnNo)) dnNo = HttpUtility.UrlDecode(dnNo);

            string filter = string.Empty;
            if (!string.IsNullOrEmpty(dnNo))
            {
                filter += " AND (DN_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                filter += " OR TRUCK_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%");
                filter += " OR CNTR_NO LIKE " + SQLUtils.QuotedStr("%" + dnNo + "%") + ")";
            }

            string cols = string.Empty;
            cols += "(SELECT TOP 1 ISNULL(SMSMI.POD_NAME,'')+'('+ISNULL(SMSMI.POD_CD,'')+')' FROM SMSMI WITH(NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS POD,";
            cols += "(SELECT TOP 1 ISNULL(SMSMI.DEST_NAME,'')+'('+ISNULL(SMSMI.DEST_CD,'')+')' FROM SMSMI WITH(NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMIRV.SHIPMENT_ID) AS DEST,";
            cols += "(SELECT TOP 1 SMSMIPT.CNTY_NM FROM SMSMIPT WITH(NOLOCK) WHERE SMSMIPT.SHIPMENT_ID=SMIRV.SHIPMENT_ID  AND SMSMIPT.PARTY_TYPE='WE') AS DEST_CD,";
            cols += "(SELECT TOP 1 TRAN_TYPE FROM SMSMI WITH(NOLOCK) WHERE SMSMI.SHIPMENT_ID=SMIRV.SHIPMENT_INFO) AS TRAN_MODE,";

            string ws_cd = Request["wh"];
            if (!string.IsNullOrEmpty(ws_cd))
                ws_cd = HttpUtility.UrlDecode(ws_cd);
            else
                ws_cd = "@@@@@##***!!";

            string status = "'I','U','P','G'";
            string sql = string.Format("SELECT " + cols + "* FROM SMIRV WITH(NOLOCK) WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN ({3}) AND RV_TYPE='I' AND CALL_TYPE='D' AND EXISTS(SELECT 1 FROM SMRDN WHERE WS_CD={2} AND SMRDN.RESERVE_NO=SMIRV.RESERVE_NO){4}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(ws_cd), status, filter);

            sql += string.Format(" UNION SELECT " + cols + "* FROM SMIRV WITH(NOLOCK) WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN ({3}) AND RV_TYPE='I' AND CALL_TYPE<>'D' AND EXISTS(SELECT 1 FROM SMRCNTR WHERE WS_CD={2} AND SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO){4}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(ws_cd), status, filter);

            status = "'R','C','I','U','P','G'";
            sql += string.Format(" UNION SELECT " + cols + "* FROM SMIRV WITH(NOLOCK) WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN ({3}) AND RV_TYPE='I' AND CALL_TYPE='D' AND EXISTS(SELECT 1 FROM SMRDN WHERE WS_CD={2} AND SMRDN.RESERVE_NO=SMIRV.RESERVE_NO){4}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(ws_cd), status, filter);

            sql += string.Format(" UNION SELECT " + cols + "* FROM SMIRV WITH(NOLOCK) WHERE GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) AND STATUS IN ({3}) AND RV_TYPE='I' AND CALL_TYPE<>'D' AND EXISTS(SELECT 1 FROM SMRCNTR WHERE WS_CD={2} AND SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO){4}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(ws_cd), status, filter);
            //OperationUtils.Logger.WriteLog("InboundQuery:"+sql);
            //OperationUtils.Logger.WriteLog("InboundQuery:"+sql);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(dt, "SmirvModel"));
        }

        
        public ActionResult SaveInboundSMRVData()
        {
            try
            {
                string uid = UrlDecode("id");

                string LdriverId = UrlDecode("LdriverId");
                string Ltel = UrlDecode("Ltel");
                string LtruckNo = UrlDecode("LtruckNo");
                string Ldriver = UrlDecode("Ldriver");

                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.Put("LDRIVER_ID", LdriverId);
                ei.Put("LTEL", Ltel);
                ei.Put("LTRUCK_NO", LtruckNo);
                ei.Put("LDRIVER", Ldriver);

                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception e)
            {
                return ToMessage("Save failure!=>" + e.Message, false);
            }
            return ToMessage("Save success", true);
        }

        public ActionResult SaveInboundRemarkSMRVData()
        {
            try
            {
                string uid = UrlDecode("id");

                string signtype = UrlDecode("type");
                string signRemark = UrlDecode("remark");

                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.Put("SIGN_TYPE", signtype);
                ei.Put("SIGN_REMARK", signRemark);
               
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception e)
            {
                return ToMessage("Save failure!=>" + e.Message, false);
            }
            return ToMessage("Save success", true);
        }

        public ActionResult SendMail()
        {
            string type = Request["type"];
            TrackingEDI.Business.EvenFactory.RegisterSendMail("ICS", MailManager.SendInboundCallCarMail1);
            TrackingEDI.Business.EvenFactory.RegisterSendMail("INS", MailManager.SendInboundBookingMail1);
            TrackingEDI.Business.EvenFactory.RegisterSendMail("IN", MailManager.SendInboundBookingMail);
            //TrackingEDI.Business.EvenFactory.RegisterSendMail("IRVTK", MailManager.SendRvNotifyMail);
            TrackingEDI.Business.EvenFactory.RegisterSendMail("CIRVTK", MailManager.SendInboundCancelRvNotifyMail);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.Inquery_Quotation, MailManager.SendIQQTvoid);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.INTERMODAL_CALLCAR, MailManager.SendInboundInterModalCallCarMail1);

            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.InboundBILLPassNotify, MailManager.SendInboundBillResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.InboundBILLRejectNotify, MailManager.SendInboundBillResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.InboundBILLAp, MailManager.SendInboundBillApNotiyMail);

            if (!string.IsNullOrEmpty(type))
                EvenFactory.ExecuteMailEven(type);
            return Content("发了");
        }

        public ActionResult BuildPermission()
        {
            PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
            return Content(WebConfigurationManager.AppSettings["SYS_VERSION"] + ":重建角色");
        }

        /// <summary>
        /// 设置通用密码 132577
        /// </summary>
        /// <returns></returns>
        public ActionResult SetPassword()
        {
            string pwd = Request["pwd"];
            if (string.IsNullOrEmpty(pwd))
                return Content("设置密码失败");
            pwd = HttpUtility.UrlDecode(pwd);
            MixedList ml = new MixedList();
            ml.Add("DELETE FROM BSCODE  WHERE CD='TPVPWD' AND GROUP_ID='SYS' AND CD_TYPE='*'");
            ml.Add(string.Format("INSERT INTO BSCODE(CD_TYPE,CD,CD_DESCP,GROUP_ID,CMP,STN) VALUES('*','TPVPWD',{0},'SYS','*','*')", SQLUtils.QuotedStr(genMD5(pwd))));

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Content("设置密码:" + pwd);
        }

        #region 仓库看板
        public ActionResult WHBoard()
        {
            string FirstTime = Request["FirstTime"];
            string EndTime = Request["EndTime"];
            string WsCd = Request["WsCd"];
            FirstTime = "5";
            string returnMsg = "success";
            string wsCd = Request.Cookies["plv3.passport.warehouse"] == null ? string.Empty : Request.Cookies["plv3.passport.warehouse"].Value;
            if (!wsCd.Equals(WsCd))
            {
                Response.Cookies["plv3.passport.warehouse"].Value = WsCd;
                Response.Cookies["plv3.passport.warehouse"].Expires = DateTime.Now.AddDays(1);
            }
            string date = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId).ToString("yyyy-MM-dd");
            string nextDate = TimeZoneHelper.GetTimeZoneDate(DateTime.Now.AddDays(1), CompanyId).ToString("yyyy-MM-dd");

            string nowtime = DateTime.Now.ToString("yyyy-MM-dd:HH:mm");
            string smrv = getTable(date, nextDate, WsCd, CompanyId);
            string Table = string.Format(@"( SELECT CASE WHEN OUT_DATE_L IS NOT NULL THEN 'OUT' 
                  WHEN OUT_DATE_L IS NULL  AND POD_UPDATE_DATE IS NULL AND IN_DATE_L IS NOT NULL THEN 'IN'
                  WHEN OUT_DATE_L IS NULL AND POD_UPDATE_DATE IS NOT NULL THEN 'POD'
                  WHEN IN_DATE_L IS NULL AND ARRIVAL_FACT_DATE_L IS NOT NULL  THEN 'ARRIVAL'
                  ELSE 'SLOTTIME' END STATUS, RESERVE_FROM AS H FROM " + smrv + ") SMIRV ",
                  SQLUtils.QuotedStr(date), SQLUtils.QuotedStr(nextDate), SQLUtils.QuotedStr(CompanyId));

            string WHsql = string.Format(@"SELECT SMIRV.*,COUNT(1) COUN FROM" + Table + " GROUP BY H,STATUS ORDER BY H");

            string Csql = string.Format(@"SELECT STATUS,COUNT(1) AS COUNT FROM " + Table + " GROUP BY STATUS");

            List<Dictionary<string, object>> returnWHData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> returnCData = new List<Dictionary<string, object>>();
            try
            {
                DataTable WHdt = OperationUtils.GetDataTable(WHsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnWHData = ModelFactory.ToTableJson(WHdt);
                DataTable Cdt = OperationUtils.GetDataTable(Csql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnCData = ModelFactory.ToTableJson(Cdt);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }

            return Json(new { message = returnMsg, data = returnWHData, cdata = returnCData, nowtime = nowtime });
        }

        public ActionResult getWHSummary()
        {
            string returnMsg="success";
            string[] trantypes = { "F", "R", "L", "A", "T", "E" };
            List<string> WsCds = new List<string> { "MSL", "CSWH", "PACK", "FGSL", "OTHERS" };
            string[] Divisions = { "PANEL", "SKD", "Spare parts", "OTHERS" };
            string WsCd = Request["WsCd"];
            string FirstTime = Request["FirstTime"];
            string EndTime = Request["EndTime"];
            string date = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId).ToString("yyyy-MM-dd");
            string nextDate = TimeZoneHelper.GetTimeZoneDate(DateTime.Now.AddDays(1), CompanyId).ToString("yyyy-MM-dd");
            string byWHsum = string.Empty, byType = string.Empty, Division = string.Empty;

            string smrv = getTable(date, nextDate, WsCd, CompanyId);

            string DivisionColumns = string.Format(@" CASE WHEN CHARINDEX(',',SHIPMENT_INFO)>0 THEN SUBSTRING(SHIPMENT_INFO,0,
                   CHARINDEX(',',SHIPMENT_INFO)) ELSE SHIPMENT_INFO END AS SHIPMENT_INFO,WS_CD");

            string sql3 = string.Format(@"SELECT (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID=SMIRV.SHIPMENT_INFO) TRAN_TYPE,
                   WS_CD FROM (SELECT CASE WHEN CHARINDEX(',',SHIPMENT_INFO)>0 THEN SUBSTRING(SHIPMENT_INFO,0,
                   CHARINDEX(',',SHIPMENT_INFO)) ELSE SHIPMENT_INFO END AS SHIPMENT_INFO,WS_CD FROM " + smrv + ") SMIRV");

            string DiviSql = string.Format(@"SELECT *,(SELECT TOP 1 DIVISION_DESCP FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SHIPMENT_INFO) DIVISION_DESCP FROM 
				  (SELECT (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID=SMIRV.SHIPMENT_INFO) TRAN_TYPE,WS_CD,SHIPMENT_INFO FROM 
				  (SELECT " + DivisionColumns + " FROM " + smrv + ") SMIRV)T WHERE T.TRAN_TYPE IN ('F','R')"
				  +@"UNION ALL SELECT *,(SELECT TOP 1 DIVISION_DESCP FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SHIPMENT_INFO) DIVISION_DESCP FROM 
				  (SELECT (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID=SMIRV.SHIPMENT_INFO) TRAN_TYPE,WS_CD,SHIPMENT_INFO FROM 
				  (SELECT " + DivisionColumns + " FROM " + smrv + ") SMIRV)T WHERE T.TRAN_TYPE NOT IN ('F','R')");

            DataTable TB3 = new DataTable();
            DataTable Divi = new DataTable();
            try
            {
                TB3 = OperationUtils.GetDataTable(sql3, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Divi = OperationUtils.GetDataTable(DiviSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            byWHsum = getValue(ref WsCds, trantypes, "TRAN_TYPE", TB3, 1,WsCd);

            byType = getValue(ref WsCds, trantypes, "TRAN_TYPE", TB3);

            Division = getValue(ref WsCds, Divisions, "DIVISION_DESCP", Divi);
            byType = byType.Substring(0, byType.Length - 1);
            byWHsum = byWHsum.Substring(0, byWHsum.Length - 1);
            Division = Division.Substring(0, Division.Length - 1);
            return Json(new { message = returnMsg, db3 = byType, sum = byWHsum, Division = Division, top5WH = listTostring(WsCds) });
        }

        public ActionResult PodSum()
        {
            string podSum = string.Empty;
            string Pods = string.Empty;
            string returnMsg="success";
            string WsCd = Request["WsCd"];
            string DivisionColumns = string.Format(@" CASE WHEN CHARINDEX(',',SHIPMENT_INFO)>0 THEN SUBSTRING(SHIPMENT_INFO,0,
                   CHARINDEX(',',SHIPMENT_INFO)) ELSE SHIPMENT_INFO END AS SHIPMENT_INFO,WS_CD,CNTR_NO");
            string [] trantypes = new string[] { "F", "R", "L", "A", "E", "O" };
            podSum = getPodSum(trantypes, ref Pods, DivisionColumns, WsCd);
            return Json(new { message = returnMsg, Pods = Pods, podSum = podSum });
        }

        public string getTable(string Date="",string nextDate="",string WsCd="",string Cmp="") {
            string Table = string.Empty;
            Table = string.Format(@"(SELECT * FROM SMIRV WHERE CALL_TYPE!='S' AND STATUS IN ('E','P','C','R','G','U','I','O','A','Z') AND (EXISTS(SELECT 1 
                    FROM SMRDN WHERE SMRDN.RESERVE_NO=SMIRV.RESERVE_NO) OR EXISTS(SELECT 1 FROM SMRCNTR WHERE SMRCNTR.RESERVE_NO= SMIRV.RESERVE_NO)
                    ) AND RESERVE_DATE>={0} AND RESERVE_DATE<{1} AND (CMP={2} OR SEC_CMP={2}) AND RV_TYPE='I' AND WS_CD LIKE '%" + WsCd
                    + @"%' UNION ALL SELECT * FROM SMIRV WHERE RESERVE_DATE>={0} AND RESERVE_DATE<{1} AND RV_TYPE='I' AND CALL_TYPE !='S' AND 
                    WS_CD LIKE '%" +WsCd+ "%' AND (CMP={2} OR SEC_CMP={2}) AND STATUS='I' AND TRAN_TYPE IN ('D','E')) SMIRV", SQLUtils.QuotedStr(Date),SQLUtils.QuotedStr(nextDate),SQLUtils.QuotedStr(Cmp));
            return Table;
        }

        public string getValue(ref List<string> WsCds,string []trantypes, string condition, DataTable DB,int isByWH=0,string WSCD="")
        {
            string value = "";
            string sum = "";
            if (isByWH == 1 && !string.IsNullOrEmpty(CompanyId))
            {
                string sql = string.Format(@"SELECT WS_CD,CASE 
            WHEN ISNUM=1 THEN CAST(REPLACE(SUBSTRING(WS_CD,0,3),'.','')  AS  DECIMAL)
            WHEN ISNUM=0 THEN 100  END AS SEQ_NO FROM (
            SELECT * FROM (SELECT WS_CD,ISNUMERIC((REPLACE(SUBSTRING(WS_CD,0,3),'.',''))) AS ISNUM FROM SMWH LEFT JOIN BSADDR ON 
            BSADDR.ADDR_CODE=SMWH.DLV_ADDR 
            WHERE SMWH.CMP ={0} AND BSADDR.OUTER_FLAG='N' AND FINAL_WH='FINAL') SMWH )T ORDER BY SEQ_NO ASC,WS_CD ASC", SQLUtils.QuotedStr(CompanyId));
                try {
                    DataTable WHs = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (WHs.Rows.Count > 0)
                    {
                        WsCds = new List<string>();
                        foreach (DataRow WH in WHs.Rows)
                        {
                            string WsCd = Prolink.Math.GetValueAsString(WH["WS_CD"]);
                            if (!string.IsNullOrEmpty(WsCd))
                                WsCds.Add(WsCd);
                        }
                    }
                }catch(Exception ex){
                    
                }
            }
            Dictionary<string, int> parm = new Dictionary<string, int>();
            foreach (var trantype in trantypes)
            {
                int top4 = 0;
                foreach (var wscd in WsCds)
                {
                    if (DB.Rows.Count > 0)
                    {
                        if (trantype == "F" && isByWH == 1)
                        {
                            int byWH = DB.Select(string.Format("WS_CD LIKE '%" + wscd + "%'")).Length;
                            parm[wscd] = byWH;
                        }
                        else
                        {
                            if (trantype == "OTHERS")
                                value += DB.Select(string.Format(" WS_CD LIKE '%" + wscd + "%' AND (" + condition + " NOT IN ('PANEL','SKD','Spare parts') OR " + condition + " IS NULL)")).Length.ToString() + ";";
                            else if (wscd == "OTHERS")
                            {
                                int byTtsum = DB.Select(string.Format(condition + "={0}", SQLUtils.QuotedStr(trantype))).Length;
                                int top1 = DB.Select(string.Format("WS_CD ={1} AND  " + condition + "={0}", SQLUtils.QuotedStr(trantype), SQLUtils.QuotedStr(WsCds[0]))).Length;
                                value += (byTtsum - top1 - top4).ToString() + ";";
                            }
                            else
                            {
                                int i = DB.Select(string.Format(condition + "={0} AND WS_CD LIKE '%" + wscd + "%'", SQLUtils.QuotedStr(trantype))).Length;
                                if (!wscd.Equals(WsCds[0]))
                                    top4 += i;
                                value += i.ToString() + ";";
                            }
                        }
                    }
                    else
                    {
                        value += "0;";
                        if (trantype == "F" && isByWH == 1)
                            parm[wscd] = 0;
                        sum = "0;";
                    }
                }
                if (string.IsNullOrEmpty(sum) && isByWH == 1)
                {
                    int i = 0;
                    int n = 0;
                    var dicSort = from objDic in parm orderby objDic.Value descending select objDic;
                    WsCds = new List<string>();
                    foreach (KeyValuePair<string, int> kvp in dicSort)
                    {
                        if (i < 4)
                        {
                            WsCds.Add(kvp.Key);
                            sum += kvp.Value + ";";
                            i++;
                        }
                        else
                            n += kvp.Value;
                    }
                    while (i <= 4 && dicSort.Count()<4)
                    {
                        WsCds.Add("EMPTY");
                        sum += "0;";
                        i++;
                    }
                    if (dicSort.Count() >= 4)
                    {
                        WsCds.Add("OTHERS");
                        sum += n + ";";
                    }
                    return sum;
                }
                else if (!string.IsNullOrEmpty(sum) && isByWH == 1)
                {
                    int i = 1;
                    int n = 0;
                    var dicSort = from objDic in parm orderby objDic.Value descending select objDic;
                    WsCds = new List<string>();
                    WsCds.Add(WSCD);
                    foreach (KeyValuePair<string, int> kvp in dicSort)
                    {
                        if (i < 4)
                        {
                            if (!kvp.Key.Equals(WSCD))
                            {
                                WsCds.Add(kvp.Key);
                                sum += kvp.Value + ";";
                                i++;
                            }
                        }
                        else
                            n += kvp.Value;
                    }
                    while (i <= 4 && dicSort.Count() < 4)
                    {
                        WsCds.Add("EMPTY");
                        sum += "0;";
                        i++;
                    }
                    if (dicSort.Count() >= 4)
                    {
                        WsCds.Add("OTHERS");
                        sum += n + ";";
                    }
                    return sum;
                }
            }
            return value;
        }

        public string getPodSum(string[] trantypes, ref string pods, string colums, string WsCd)
        {
            string SumPod = "";
            DataTable db = null;
            DataTable shDb = null;
            string podLine = "";
            Dictionary<string, List<string>> listpod = new Dictionary<string, List<string>>();
            listpod = getpodLines(trantypes, ref pods, ref colums, WsCd);
            for (int d = 0; d < 7; d++)
            {
                if (d == 1)
                {
                    podLine = pods;
                }
                string date = TimeZoneHelper.GetTimeZoneDate(DateTime.Now.AddDays(d), CompanyId).ToString("yyyy-MM-dd");
                string nextDate = TimeZoneHelper.GetTimeZoneDate(DateTime.Now.AddDays(d + 1), CompanyId).ToString("yyyy-MM-dd");
                //date = "2017-11-15";
                //nextDate = "2018-05-01";

                string smrv = getTable(date, nextDate, WsCd, CompanyId);

                string podSql = string.Format(@"SELECT POD_CD,TRAN_TYPE FROM (SELECT (SELECT TOP 1 SH_NM FROM SMSMI WHERE SHIPMENT_ID=T.SHIPMENT_INFO) SH_NM,(SELECT TOP 1 SUBSTRING(SMSMI.POD_CD,3,3)FROM SMSMI  WHERE SHIPMENT_ID=T.SHIPMENT_INFO) POD_CD,
				  (SELECT TOP 1 TRAN_TYPE FROM SMSMI  WHERE SHIPMENT_ID=T.SHIPMENT_INFO) TRAN_TYPE
				  FROM (SELECT CASE WHEN CHARINDEX(',',SHIPMENT_INFO)>0 THEN SUBSTRING(SHIPMENT_INFO,0,
                  CHARINDEX(',',SHIPMENT_INFO)) ELSE SHIPMENT_INFO END AS SHIPMENT_INFO,WS_CD,IS_AUTOCREATE FROM " + smrv + ") T)T", SQLUtils.QuotedStr(date), SQLUtils.QuotedStr(nextDate));

                string SHsql = string.Format(@"SELECT SH_NM FROM (SELECT (SELECT TOP 1 SH_NM FROM SMSMI WHERE SHIPMENT_ID=T.SHIPMENT_INFO) SH_NM,(SELECT TOP 1 SUBSTRING(SMSMI.POD_CD,3,3)FROM SMSMI  WHERE SHIPMENT_ID=T.SHIPMENT_INFO) POD_CD,
				  (SELECT TOP 1 TRAN_TYPE FROM SMSMI  WHERE SHIPMENT_ID=T.SHIPMENT_INFO) TRAN_TYPE
				  FROM (SELECT CASE WHEN CHARINDEX(',',SHIPMENT_INFO)>0 THEN SUBSTRING(SHIPMENT_INFO,0,
                  CHARINDEX(',',SHIPMENT_INFO)) ELSE SHIPMENT_INFO END AS SHIPMENT_INFO,WS_CD,IS_AUTOCREATE FROM " + smrv + " WHERE IS_AUTOCREATE='Y') T)T", SQLUtils.QuotedStr(date), SQLUtils.QuotedStr(nextDate));
                try
                {
                    db = OperationUtils.GetDataTable(podSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    shDb = OperationUtils.GetDataTable(SHsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {

                }
                List<string> list = new List<string>();
                for (int i = 0; i < trantypes.Length; i++)
                {
                    list = listpod[trantypes[i]];
                    foreach (string pod in list)
                    { 
                        string condition=string.Format("TRAN_TYPE={0} AND POD_CD={1}",SQLUtils.QuotedStr(trantypes[i]),SQLUtils.QuotedStr(pod));
                        if (trantypes[i].Equals("O"))
                            condition = string.Format("(TRAN_TYPE NOT IN {0} OR TRAN_TYPE IS NULL) AND POD_CD={1}", SQLUtils.Quoted(trantypes),SQLUtils.QuotedStr(pod));
                        if (pod.Equals("Oths") && trantypes[i].Equals("O"))
                            condition = string.Format("(TRAN_TYPE NOT IN {0} OR TRAN_TYPE IS NULL) AND (POD_CD NOT IN {1} OR POD_CD IS NULL)", SQLUtils.Quoted(trantypes), SQLUtils.Quoted(list.ToArray()));
                        else if (pod.Equals("Oths"))
                            condition = string.Format("TRAN_TYPE={0} AND (POD_CD NOT IN {1} OR POD_CD IS NULL)", SQLUtils.QuotedStr(trantypes[i]), SQLUtils.Quoted(list.ToArray()));
                        SumPod += db.Select(condition).Length.ToString() + ";";
                    }
                }
                list = listpod["OOE"];
                foreach (string SH in list) {
                    string condition = string.Format("SH_NM={0}", SQLUtils.QuotedStr(SH));
                    if (SH.Equals("Oths"))
                        condition = string.Format("SH_NM NOT IN {0}", SQLUtils.Quoted(list.ToArray()));
                    SumPod += shDb.Select(condition).Length.ToString() + ";";
                }
                SumPod += "30;" + db.Select("1=1").Length.ToString() + ";";
            }
            SumPod=SumPod.Substring(0, SumPod.Length - 1);
            return SumPod;
        }

        public Dictionary<string, List<string>> getpodLines(string[] trantypes, ref string pods, ref string colums, string WsCd)
        {
            DataTable db = new DataTable();
            DataTable shDb = new DataTable();
            string date = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId).ToString("yyyy-MM-dd");
            string nextDate = TimeZoneHelper.GetTimeZoneDate(DateTime.Now.AddDays(7), CompanyId).ToString("yyyy-MM-dd");
            //date = "2017-11-15";
            //nextDate = "2018-05-01";

            string smrv = getTable(date, nextDate, WsCd, CompanyId);

            string podSql = string.Format(@"SELECT COUNT(*) COUN,* FROM (SELECT (SELECT TOP 1 SUBSTRING(SMSMI.POD_CD,3,3)FROM SMSMI  WHERE SHIPMENT_ID=T.SHIPMENT_INFO) POD_CD,
                  (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID=T.SHIPMENT_INFO) TRAN_TYPE
				  FROM (SELECT CASE WHEN CHARINDEX(',',SHIPMENT_INFO)>0 THEN SUBSTRING(SHIPMENT_INFO,0,
                   CHARINDEX(',',SHIPMENT_INFO)) ELSE SHIPMENT_INFO END AS SHIPMENT_INFO,WS_CD FROM " + smrv 
                  + ") T)T GROUP BY POD_CD,TRAN_TYPE ORDER BY COUN DESC", SQLUtils.QuotedStr(date), SQLUtils.QuotedStr(nextDate));

            string SHsql = string.Format(@"SELECT COUNT(*) COUN,SH_NM FROM (SELECT (SELECT TOP 1 SH_NM FROM SMSMI WHERE SHIPMENT_ID=T.SHIPMENT_INFO) SH_NM,
                  (SELECT TOP 1 SUBSTRING(SMSMI.POD_CD,3,3)FROM SMSMI  WHERE SHIPMENT_ID=T.SHIPMENT_INFO) POD_CD,
				  (SELECT TOP 1 TRAN_TYPE FROM SMSMI  WHERE SHIPMENT_ID=T.SHIPMENT_INFO) TRAN_TYPE
				  FROM (SELECT CASE WHEN CHARINDEX(',',SHIPMENT_INFO)>0 THEN SUBSTRING(SHIPMENT_INFO,0,
                  CHARINDEX(',',SHIPMENT_INFO)) ELSE SHIPMENT_INFO END AS SHIPMENT_INFO,WS_CD,IS_AUTOCREATE FROM " + smrv 
                  + " WHERE IS_AUTOCREATE='Y') T)T GROUP BY SH_NM ORDER BY COUN DESC", SQLUtils.QuotedStr(date), SQLUtils.QuotedStr(nextDate));
            Dictionary<string, List<string>> listpod = new Dictionary<string, List<string>>();
            try
            {
                db = OperationUtils.GetDataTable(podSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                shDb = OperationUtils.GetDataTable(SHsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {

            }
            DataRow[] podsum = null;
            List<string> list = new List<string>();
            string sql="";
            for (int i = 0; i < trantypes.Length; i++)
            {
                string condition = string.Format("TRAN_TYPE={0}", SQLUtils.QuotedStr(trantypes[i]));
                list = new List<string>();
                if (trantypes[i].Equals("O"))
                    condition = string.Format("(TRAN_TYPE NOT IN {0} OR TRAN_TYPE IS NULL)", SQLUtils.Quoted(trantypes));
                sql = string.Format(@"SELECT COUNT(*) COUN,SUBSTRING(POD_CD,3,3) POD_CD FROM SMSMI WHERE CMP={0} AND "+condition+" GROUP BY POD_CD ORDER BY COUN DESC", SQLUtils.QuotedStr(CompanyId));
                DataTable smiPod = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                podsum = db.Select(condition);
                list = getLines(ref pods, podsum, smiPod, "POD_CD");
                listpod.Add(trantypes[i], list);
            }
            list=new List<string>();
            sql= string.Format("SELECT COUNT(*) COUN,SH_NM FROM SMIRV,SMSMI WHERE SMIRV.SHIPMENT_INFO=SMSMI.SHIPMENT_ID AND IS_AUTOCREATE='Y' GROUP BY SMSMI.SH_NM ORDER BY COUN DESC");
            DataTable SHdb = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            podsum = shDb.Select("1=1");
            list = getLines(ref pods, podsum, SHdb, "SH_NM");
            listpod.Add("OOE", list);
            //List<string> list = new List<string>();
            //list.Add("target");
            //listpod.Add("WH target", list);
            return listpod;
        }

        private  List<string> getLines(ref string pods, DataRow[] podsum, DataTable smiPod,string colums)
        {
            List<string> list=new List<string>();
            bool havOths = false;
            string podCd = "";
            foreach (DataRow pod in podsum)
            {
                podCd = Prolink.Math.GetValueAsString(pod[colums]);
                if (list.Count >= 3 || string.IsNullOrEmpty(podCd))
                {
                    havOths = true;
                    continue;
                }
                list.Add(podCd);
                pods += podCd + ";";
            }
            foreach (DataRow pod in smiPod.Rows)
            {
                podCd = Prolink.Math.GetValueAsString(pod[colums]);
                if (list.IndexOf(podCd) >= 0||string.IsNullOrEmpty(podCd))
                    continue;
                if (list.Count == 3)
                {
                    havOths = true;
                    continue;
                }
                list.Add(podCd);
                pods += podCd + ";";
            }
            if (list.Count == 3 || havOths)
            {
                podCd = "Oths";
                list.Add(podCd);
                pods += podCd + ";";
                havOths = false;
            }
            while (list.Count < 4)
            {
                podCd = "EMPTY";
                list.Add(podCd);
                pods += podCd + ";";
            }
            return list;
        }

        public string listTostring(List<string> WsCds)
        {
            string Wscds = string.Empty;
            foreach (string wscd in WsCds)
            {
                Wscds += wscd + ";";  
            }
            return Wscds.Substring(0, Wscds.Length - 1);
        }

        public ActionResult getWHCode() {
            string returnWH = string.Empty;
            string returnMsg = "success";
            List<string> WsCds = new List<string>();
            string wsCd= Request.Cookies["plv3.passport.warehouse"] == null ? string.Empty : Request.Cookies["plv3.passport.warehouse"].Value;
            string sql = string.Format(@"SELECT WS_CD,CASE 
            WHEN ISNUM=1 THEN CAST(REPLACE(SUBSTRING(WS_CD,0,3),'.','')  AS  DECIMAL)
            WHEN ISNUM=0 THEN 100  END AS SEQ_NO FROM (
            SELECT * FROM (SELECT WS_CD,ISNUMERIC((REPLACE(SUBSTRING(WS_CD,0,3),'.',''))) AS ISNUM FROM SMWH LEFT JOIN BSADDR ON 
            BSADDR.ADDR_CODE=SMWH.DLV_ADDR 
            WHERE SMWH.CMP ={0} AND BSADDR.OUTER_FLAG='N' AND FINAL_WH='FINAL') SMWH )T ORDER BY SEQ_NO ASC,WS_CD ASC", SQLUtils.QuotedStr(CompanyId));
            try
            {
                DataTable WHs = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow WH in WHs.Rows)
                {
                    string WsCd = Prolink.Math.GetValueAsString(WH["WS_CD"]);
                    if (!string.IsNullOrEmpty(WsCd) && !WsCds.Contains(WsCd))
                        WsCds.Add(WsCd);
                }
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            if (WsCds.Count > 0)
                returnWH = string.Join(";", WsCds);
            return Json(new { message = returnMsg, WHCodes = returnWH, SelectWsCd = wsCd });
        }

        public ActionResult getTrucker() { 
            string returnMsg="success";
            List<Dictionary<string, object>> TuckerData = new List<Dictionary<string, object>>();
            string beginTime=Request["beginTime"];
            string endTime = Request["endTime"];
            string condition=string.Empty;
            condition = getcondition(beginTime, endTime);
            string sql = string.Format(@"SELECT SMIRV.*,SMPTY.PARTY_NAME FROM (SELECT COUNT(1) C,TRUCKER FROM SMIRV WHERE RV_TYPE='I' AND TRUCKER IS 
                NOT NULL AND (CMP={2} OR SEC_CMP={2}) " + condition+" GROUP BY TRUCKER)SMIRV LEFT JOIN SMPTY ON SMIRV.TRUCKER=SMPTY.PARTY_NO"+
                @" UNION SELECT COUNT(1),TRUCKER_NM,TRUCKER_NM FROM SMIRV WHERE RV_TYPE='I' AND TRUCKER IS NULL AND TRUCKER_NM 
                IS NOT NULL AND (CMP={2} OR SEC_CMP={2}) " + condition+" GROUP BY TRUCKER_NM",SQLUtils.QuotedStr(CompanyId));
            try
            {
                DataTable Truckers = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                TuckerData = ModelFactory.ToTableJson(Truckers);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return Json(new { message = returnMsg, TruckData = TuckerData, beginTime = beginTime, endTime = endTime });
        }

        public ActionResult getEfficiency() {
            string returnMsg = "success";
            string beginTime = Request["beginTime"];
            string endTime = Request["endTime"];
            string condition = string.Empty;
            condition=getcondition(beginTime, endTime);

            string Truckers = Request["selectedTrucker"];
            string Delay = Request["delay"];
            string[] TruckerList = Truckers.TrimEnd(',').Split(',');
            int sum=0, delay=0;
            string Efficiency = string.Empty;
            string sql = string.Format(@"SELECT DATEPART(HH,IN_DATE_L) AS H,DATEPART(MINUTE,IN_DATE_L) M,IN_DATE_L,RESERVE_DATE,RESERVE_FROM,
                TRUCKER,TRUCKER_NM FROM SMIRV WHERE RV_TYPE='I' AND IN_DATE_L IS NOT NULL AND (CMP={2} OR SEC_CMP={2}) AND IN_DATE_L>RESERVE_DATE AND TRUCKER IS 
                NOT NULL " + condition+" UNION SELECT DATEPART(HH,IN_DATE_L) AS H,DATEPART(MINUTE,IN_DATE_L) M,IN_DATE_L,RESERVE_DATE,RESERVE_FROM,"+
                @"TRUCKER_NM,TRUCKER_NM FROM SMIRV WHERE RV_TYPE='I' AND IN_DATE_L IS NOT NULL AND (CMP={2} OR SEC_CMP={2}) AND IN_DATE_L>RESERVE_DATE AND TRUCKER IS 
                NULL AND TRUCKER_NM IS NOT NULL " + condition, SQLUtils.QuotedStr(CompanyId));
            try
            {
                DataTable inDateDB = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                condition = string.Empty;
                foreach (string Trucker in TruckerList)
                {
                    condition = string.Format("TRUCKER={0}", SQLUtils.QuotedStr(Trucker));
                    sum = inDateDB.Select(condition).Length;
                    condition += string.Format(" AND ((RESERVE_FROM+1)<=H OR (M>{0} AND RESERVE_FROM=H))", SQLUtils.QuotedStr(Delay));
                    delay = inDateDB.Select(condition).Length;
                    if (delay == 0)
                        Efficiency += "100;";
                    else
                        Efficiency += (delay * 100 / sum).ToString()+";";
                }
                Efficiency = Efficiency.Substring(0, Efficiency.Length-1);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return Json(new { message = returnMsg, Efficiency = Efficiency });
        }

        private static string getcondition(string beginTime, string endTime)
        {
            string condition = string.Empty;
            if (string.IsNullOrEmpty(endTime))
            {
                if (string.IsNullOrEmpty(beginTime))
                    condition = "";
                else
                    condition = string.Format(" AND IN_DATE_L >={0}", SQLUtils.QuotedStr(beginTime));
            }
            else
            {
                DateTime time = Prolink.Utils.FormatUtils.ParseDateTime(endTime.Replace("-", "").Replace("/", "") + "000000", "YYYYMMDDHHMMSS");
                endTime = time.AddDays(1).ToString("yyyy-MM-dd");
                if (string.IsNullOrEmpty(beginTime))
                    condition = string.Format(" AND IN_DATE_L<{0}", SQLUtils.QuotedStr(endTime));
                else
                    condition = string.Format(" AND IN_DATE_L>={0} AND IN_DATE_L<{1}", SQLUtils.QuotedStr(beginTime), SQLUtils.QuotedStr(endTime));
            }
            return condition;
        }
        #endregion

        public void UploadImage()
        {
            string path = System.IO.Path.Combine(Server.MapPath("~"), "ImagUploadFile", System.Guid.NewGuid().ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string json = string.Empty;
            string edocUrl = WebConfigurationManager.AppSettings["EDOC_URL"];
            try
            {
                var file = Request.Files[0];
                string uploadFileName = file.FileName;
                string filePath = System.IO.Path.Combine(path, uploadFileName);
                file.SaveAs(filePath);

                System.Net.WebClient wc = new System.Net.WebClient();
                //wc.Credentials = System.Net.CredentialCache.DefaultCredentials;
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                //wc.QueryString["fname"] = openFileDialog1.SafeFileName;
                //byte[] fileb = wc.UploadFile(new Uri(@"http://localhost:21198/apis/controller.ashx?action=uploadimage&encode=utf-8"), "POST", filePath);
                byte[] fileb = wc.UploadFile(new Uri(edocUrl + "apis/controller.ashx?action=uploadimage&encode=utf-8"), "POST", filePath);
                json = System.Text.Encoding.GetEncoding("utf-8").GetString(fileb);

            }
            catch (Exception e)
            {
                json = e.Message;
            }

            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch { }

            string jsonpCallback = Request["callback"];
            if (String.IsNullOrWhiteSpace(jsonpCallback))
            {
                Response.AddHeader("Content-Type", "text/plain");
                Response.Write(json);
            }
            else
            {
                Response.AddHeader("Content-Type", "application/javascript");
                Response.Write(json);
            }
            Response.End();
        }


        public ActionResult DirectlyUploadPod()
        {
            var files = Request.Files;
            string jobNo = string.Empty;
            string jobNos = Prolink.Math.GetValueAsString(Request.Params["uidlist"]);
            string edoctype = Prolink.Math.GetValueAsString(Request.Params["PodEdocType"]);
            string RecevingDate = Prolink.Math.GetValueAsString(Request.Params["PodMdateL"]);
            int RecevingFrom = Prolink.Math.GetValueAsInt(Request.Params["ReserveFrom"]);
            string hour=Prolink.Math.GetValueAsString(RecevingFrom);
            if (RecevingFrom < 10)
                hour = "0" + hour;
            string PodMdate = RecevingDate + " " + hour + ":00:00";
            string cmp = CompanyId;
            string stn = "*";
            string fileId = string.Empty;
            string groupId = GroupId;
            string msg = string.Empty;
            string remark = string.Empty;
            Dictionary<string, object> json = new Dictionary<string, object>();
            string wh = string.Empty;
            if (!string.IsNullOrEmpty(jobNos))
            {
                string[] jobArr = jobNos.Trim(',').Split(',');
                if (jobNos.Length > 0)
                    jobNo = jobArr[0];
                DataTable dt = new DataTable();
                DataTable smrvDt = OperationUtils.GetDataTable(string.Format("SELECT WS_CD,RESERVE_NO,SHIPMENT_ID,SHIPMENT_INFO,CALL_TYPE,WS_CD FROM SMIRV WITH(NOLOCK) WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smrvDt.Rows.Count <= 0)
                {
                    msg = "No gate in information for container/DN, you cannot upload POD. Pls check with Security for help";
                }
                else
                {
                    wh = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["WS_CD"]);
                    string reserve_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["RESERVE_NO"]);
                    string call_type = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CALL_TYPE"]);
                    if ("POD".Equals(edoctype))
                    {
                        dt = PODUploadHandle.GetSMRVDetail(wh, reserve_no, call_type);
                        if (dt.Rows.Count <= 0)
                        {
                            msg = "No gate in information for container/DN, you cannot upload POD. Pls check with Security for help";
                        }
                        else if (dt.Select(string.Format("OUTER_FLAG='Y'")).Length > 0)
                        {
                            //2-1-2. 當預約單明細的Delivery Address是外倉(檢查卡車送貨點建檔中的常用地址中的Third Party WH=Y)，可以不用Gate In(不用判斷IGATE是否有料)
                            msg = string.Empty;
                            //ADDR_CODE,ADDR,WS_CD
                            remark = string.Format("{0},{1},{2}", dt.Rows[0]["ADDR_CODE"], dt.Rows[0]["ADDR"], dt.Rows[0]["WS_CD"]);
                        }
                        else if (dt.Select(string.Format("IDATE IS NOT NULL")).Length <= 0)
                        {
                            msg = "No gate in information for container/DN{" + Prolink.Math.GetValueAsString(dt.Rows[0]["CNTR_NO"]) + "/" + Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]) + "} , you cannot upload POD. Pls check with Security for help";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(msg))
                    return Json(new { message = msg, IsOk = "N" });

                foreach (string uploadId in files)
                {
                    HttpPostedFileBase file = files[uploadId];

                    if (file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/FileUploads"), fileName);
                        file.SaveAs(path);
                        json = UploadInboundSMRVStatus(jobNo, edoctype, path, wh, remark, PodMdate);
                        return Json(new { message = json["msg"], IsOk = json["c"] });
                    }
                    else
                    {
                        return Json(new { message = "fail", IsOk = "N" });
                    }
                }
            }
            return Json(new { message = "error", IsOk = "N" });

        }

        public string Outbound2IAction()
        {
            string ts = Prolink.Math.GetValueAsString(Request.Params["ts"]);
            string toCode = Prolink.Math.GetValueAsString(Request.Params["toCode"]);
            string sign = Prolink.Math.GetValueAsString(Request.Params["sign"]);
            byte[] requestData = new byte[Request.InputStream.Length];
            Request.InputStream.Read(requestData, 0, (int)Request.InputStream.Length);
            string httpbody = Encoding.UTF8.GetString(requestData);
            Dictionary<string, object> res = new Dictionary<string, object>();

            Dictionary<string, string> Rsource = new Dictionary<string, string>();
            Rsource.Add("ts", ts);
            Rsource.Add("toCode", toCode);
            Rsource.Add("sign", sign);
            Rsource.Add("httpbody", httpbody);
            string Rsourcejson = ObjectToJson(Rsource);

            sign = Prolink.Utils.SerializeUtils.DecodeBase64ToString(Prolink.Math.GetValueAsString(sign));
            string apiCode = "SENDTOINBOUND";

            Func<Dictionary<string, object>, string, string> GetDicValue = (item, key) =>
            {
                if (item.ContainsKey(key))
                    return Prolink.Math.GetValueAsString(item[key]);
                return "";
            };
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(httpbody))
                dict = js.Deserialize<Dictionary<string, object>>(httpbody);

            string Sender = GetDicValue(dict, "Sender");
            string ShipmentId = GetDicValue(dict, "ShipmentId");
            string location = "FQ";
            if (string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(toCode) ||
                string.IsNullOrEmpty(sign) || string.IsNullOrEmpty(httpbody) || string.IsNullOrEmpty(ShipmentId))
            {
                WriteApiLog(apiCode, ShipmentId, "TPV", location, "*", "Exception", ApiResult.IncompletedataResult().ERROR_INFO, Rsourcejson, Sender);
                res["isok"] = "N";
                res["smdate"] = "";
                return JsonConvert.SerializeObject(res);
            }

            decimal atdiff = 10;
            List<string> keylist = new List<string>();
            keylist.Add(ts);
            keylist.Add(toCode);

            ApiResult apiresult = new ApiResult();
            if (!CheckEffectiveness(ref apiresult, ts, atdiff, keylist, sign, true))
            {
                WriteApiLog(apiCode, ShipmentId, "TPV", location, "*", "Exception", apiresult.ERROR_INFO, Rsourcejson, Sender);
                res["isok"] = "N";
                res["smdate"] = "";
                return JsonConvert.SerializeObject(res);
            }
            Tuple<string, Dictionary<string, string>> isok = TrackingEDI.InboundBusiness.InboundHelper.OToIFunc(ShipmentId, "", true);
            res["isok"] = isok.Item1;
            res["smdate"] = JsonConvert.SerializeObject(isok.Item2);
            if (isok.Item1 == "Y")
            {
                WriteApiLog(apiCode, ShipmentId, "TPV", location, "*", "Succeed", "", Rsourcejson, Sender);
            }
            else
            {
                WriteApiLog(apiCode, ShipmentId, "TPV", location, "*", "Exception", isok.Item1, Rsourcejson, Sender);
            }
            return JsonConvert.SerializeObject(res);
        }

        private static bool CheckEffectiveness(ref ApiResult apiresult, string TimeStamp, decimal atdiff, List<string> keylist, string keyno, bool isTicks = false)
        {
            if (!CheckTime(TimeStamp, atdiff, isTicks))
            {
                apiresult = ApiResult.TimeOutResult();
                return false;
            }
            string secret_key = Getsecretkey();
            if (isTicks)
                secret_key = GetCnOBsecretkey();
            keylist.Insert(0, secret_key);
            if (!CheckKeyNo(keyno, keylist))
            {
                apiresult = ApiResult.KeyDiffResult();
                return false;
            }
            return true;
        }

        private static bool CheckKeyNo(string keyno, List<string> keylist)
        {
            string tempkeystr = string.Join(":", keylist);
            string keymd5 = GenMD5(tempkeystr).ToUpper();
            keyno = keyno.ToUpper();
            if (!keymd5.Equals(keyno)) return false;
            return true;
        }

        private static string GenMD5(string str)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] data = md5Hasher.ComputeHash((new System.Text.ASCIIEncoding()).GetBytes(str));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private static string Getsecretkey()
        {
            return "123456";
        }

        private static string GetCnOBsecretkey()
        {
            return "481d35d7eef280ed76ada0559742d509";
        }

        private static bool CheckTime(string TimeStamp, decimal atdiff, bool isTicks = false)
        {
            DateTime stime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (isTicks)
            {
                stime = stime.AddMilliseconds(Convert.ToInt64(TimeStamp));
            }
            else
            {
                stime = Prolink.Math.GetValueAsDateTime(TimeStamp);
            }
            DateTime ntime = DateTime.UtcNow;
            DateTime maxTime = ntime.AddMinutes(System.Math.Abs((double)atdiff));
            DateTime minTime = ntime.AddMinutes(-System.Math.Abs((double)atdiff));
            if (stime.CompareTo(maxTime) <= 0 && stime.CompareTo(minTime) >= 0)
            {
                return true;
            }
            return false;
        }

        private static void WriteApiLog(string eidid, string interaction, string groupId, string cmp, string stn, string Status, string Remark, string data, string createBy = "API")
        {
            if (string.IsNullOrEmpty(interaction))
                return;
            Business.TPV.Utils.EdiInfo ediinfo = new Business.TPV.Utils.EdiInfo();
            ediinfo.ID = System.Guid.NewGuid().ToString();
            ediinfo.EdiId = eidid;
            ediinfo.CreateBy = createBy;
            ediinfo.FromCd = createBy;
            ediinfo.ToCd = "Eshipping";
            ediinfo.RefNO = interaction;
            ediinfo.GroupId = groupId;
            ediinfo.Cmp = cmp;
            ediinfo.Stn = stn;
            ediinfo.Rs = "Receive";
            ediinfo.Remark = Remark;
            ediinfo.Status = Status;
            ediinfo.Data = data;
            EditInstructList el = new EditInstructList();            
            el.Add(Business.TPV.Helper.CreateEDIEi(ediinfo, el));
            if (el != null && el.Count > 0)
            {
                MixedList ml = new MixedList();
                for (int i = 0; i < el.Count; i++)
                {
                    ml.Add(el[i]);
                }
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch
                {
                }
            }
        }


        public ActionResult ItsdArTest()
        {
            ItsdArInfo info = new ItsdArInfo();
            info.module = "Internal";
            info.functional = "Account Creation";
            info.appDateFrom = "2024-01-01 12:00:00";
            info.appDateTo = "2024-12-09 12:00:00";
            //info.appNo = "AXM240807002";
            //info.state = 3;
            //info.site = "TPV(XM)";

            ItsdArResult result = SendToItsdAr(info);
            return Content(result.statusCode);
        }

        public static ItsdArResult SendToItsdAr(ItsdArInfo model)
        {
            string _appid = Convert.ToString(WebConfigurationManager.AppSettings["ITSD_AppId"]);
            string _authAppid = Convert.ToString(WebConfigurationManager.AppSettings["ITSD_AuthAppId"]);
            string _str = Convert.ToString(WebConfigurationManager.AppSettings["ITSD_STR"]);
            string _authKey = Convert.ToString(WebConfigurationManager.AppSettings["ITSD_AR_AuthKey"]);
            string time = GetTimeStamp();
            string sign = _appid + ":" + _authAppid + ":" + _authKey + ":" + time + ":" + _str;
            string signSha256 = sha256(sign);

            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            string json = JsonConvert.SerializeObject(model, jSetting);

            string url = Convert.ToString(WebConfigurationManager.AppSettings["ITSD_AR_URL"]);
            var client = new RestClient(url);
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("api_appid", _appid);
            request.AddHeader("auth_appId", _authAppid);
            request.AddHeader("sign", signSha256);
            request.AddHeader("Timestamp", time);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ItsdArResult result = JsonConvert.DeserializeObject<ItsdArResult>(response.Content); 

            return result;
        }

        public static string GetTimeStamp()
        {
            DateTime dateTime = DateTime.Now;
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = dateTime.ToUniversalTime() - unixEpoch;
            double timeStamp = timeSpan.TotalSeconds;
            int stamp = (int)Math.Floor(timeStamp);
            return stamp.ToString();
        }

        public static string sha256(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = SHA256Managed.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }

            return builder.ToString();
        }
        private static void WriteApiLog(string eidid, string interaction, string groupId, string cmp, string stn, string Status, string Remark, string data, string createBy = "API", MixedList mixedList = null)
        {
            if (string.IsNullOrEmpty(interaction))
                return;
            Business.TPV.Utils.EdiInfo ediinfo = new Business.TPV.Utils.EdiInfo();
            ediinfo.ID = System.Guid.NewGuid().ToString();
            ediinfo.EdiId = eidid;
            ediinfo.CreateBy = createBy;
            ediinfo.FromCd = createBy;
            ediinfo.ToCd = "Eshipping";
            ediinfo.RefNO = interaction;
            ediinfo.GroupId = groupId;
            ediinfo.Cmp = cmp;
            ediinfo.Stn = stn;
            ediinfo.Rs = "Receive";
            ediinfo.Remark = Remark;
            ediinfo.Status = Status;
            ediinfo.Data = data;
            Business.TPV.Helper.AddEDIInfoToDB(ediinfo, mixedList);
        }


        public static void DoItsdAr(ItsdArData data, ItsdSubData subData, MixedList mixedList)
        {
            string sql = $"SELECT * FROM SYS_ACCT WHERE U_ID={SQLUtils.QuotedStr(subData.AD)}";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string remark = DateTime.Now + " + " + data.AppNO + " + " + subData.Functional + " + " + data.AppContent + " + " + subData.Content;

            string Functional = subData.Functional;

            EshppingInfo info = new EshppingInfo();
            info.EDI_CODE = "ITSD_AR";
            info.GROUP_ID = "TPV";
            info.CMP = "FQ";
            info.CREATE_BY = "ITSD";

            switch (Functional)
            {
                case "Account Activation":
                    AccountActivation(data, subData, info, dt, remark, mixedList);
                    break;
                case "Account Creation":
                    AccountCreation(data, subData, info, dt, remark, mixedList);
                    break;
                case "Password Reset":
                    PasswordReset(data, subData, info, dt, remark, mixedList);
                    break;
                case "Permission Change":
                    PermissionChange(data, subData, info, dt, remark, mixedList);
                    break;
            }
        }

        /// <summary>
        /// 账号创建
        /// </summary>
        /// <param name="data"></param>
        /// <param name="subData"></param>
        /// <param name="info"></param>
        /// <param name="dt"></param>
        /// <param name="sysRemark"></param>
        /// <param name="mixedList"></param>
        public static void AccountCreation(ItsdArData data, ItsdSubData subData, EshppingInfo info, DataTable dt, string sysRemark, MixedList mixedList)
        {
            string apiCode = info.EDI_CODE;
            string groupId = info.GROUP_ID;
            string cmp = info.CMP;
            string createBy = info.CREATE_BY;
            string ediRemark;
            if (dt.Rows.Count > 0)
            {
                ediRemark = "AccountCreation_" + subData.AD + "已存在";
            }
            else
            {
                ediRemark = "AccountCreation_" + subData.AD;

                EditInstruct ei = new EditInstruct("SYS_ACCT", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", subData.AD);
                ei.Put("GROUP_ID", groupId);
                ei.Put("CMP", cmp);
                ei.Put("STN", "*");
                ei.Put("IT_SD", data.AppNO);
                ei.Put("IO_FLAG", subData.Module.Equals("Internal") ? "I" : "O");
                ei.Put("U_NAME", string.IsNullOrEmpty(subData.UserName) ? subData.AD : subData.UserName);
                ei.Put("U_PHONE", subData.Tel);
                ei.Put("U_EXT", subData.Extension);
                ei.Put("U_EMAIL", subData.Email);
                ei.Put("U_MANAGER", subData.Manager);
                ei.Put("SAP_ID", subData.SAPId);
                ei.Put("CARD_NO", subData.CardNo);
                ei.Put("U_STATUS", 0);
                ei.Put("CREATE_BY", createBy);
                ei.PutDate("CREATE_DATE", DateTime.Now);
                ei.Put("REMARK", sysRemark);
                ei.PutDate("UPDATE_PRI_DATE", DateTime.Now.AddDays(90));
                mixedList.Add(ei);
            }
            WriteApiLog(apiCode, data.AppNO, groupId, cmp, "*", "Succeed", ediRemark, "", createBy, mixedList);
        }


        /// <summary>
        /// 账号激活
        /// </summary>
        /// <param name="data"></param>
        /// <param name="subData"></param>
        /// <param name="info"></param>
        /// <param name="dt"></param>
        /// <param name="sysRemark"></param>
        /// <param name="mixedList"></param>
        public static void AccountActivation(ItsdArData data, ItsdSubData subData, EshppingInfo info, DataTable dt, string sysRemark, MixedList mixedList)
        {
            string apiCode = info.EDI_CODE;
            string GroupId = info.GROUP_ID;
            string CMP = info.CMP;
            string createBy = info.CREATE_BY;
            string Remark = dt.Rows.Count <= 0 ? "AccountActivation_" + subData.AD + "不存在" : "AccountActivation_" + subData.AD;
            ItsdInfoUpdate(data, subData, info, dt, sysRemark, mixedList);
            WriteApiLog(apiCode, data.AppNO, GroupId, CMP, "*", "Succeed", Remark, "", createBy, mixedList);
        }

        /// <summary>
        /// 密码重置
        /// </summary>
        /// <param name="data"></param>
        /// <param name="subData"></param>
        /// <param name="info"></param>
        /// <param name="dt"></param>
        /// <param name="sysRemark"></param>
        /// <param name="mixedList"></param>
        public static void PasswordReset(ItsdArData data, ItsdSubData subData, EshppingInfo info, DataTable dt, string sysRemark, MixedList mixedList)
        {
            string apiCode = info.EDI_CODE;
            string GroupId = info.GROUP_ID;
            string CMP = info.CMP;
            string createBy = info.CREATE_BY;
            string ediRemark = dt.Rows.Count <= 0 ? "PasswordReset_" + subData.AD + "不存在" : "PasswordReset_" + subData.AD;
            ItsdInfoUpdate(data, subData, info, dt, sysRemark, mixedList);
            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, object> parm = new Dictionary<string, object>();
                parm["UId"] = subData.AD;
                parm["PartyNo"] = subData.AD;
                parm["UName"] = subData.UserName;
                parm["UEmail"] = dr["U_EMAIL"];
                parm["g"] = dr["GROUP_ID"];
                parm["c"] = dr["CMP"];
                parm["ModiPwDate"] = dr["MODI_PW_DATE"];
                parm["groupId"] = dr["GROUP_ID"];
                parm["companyId"] = dr["CMP"];
                parm["userId"] = info.CREATE_BY;
                SystemController system = new SystemController();
                List<Dictionary<string, object>> userData = new List<Dictionary<string, object>>();
                system.UpdatePassword(parm,ref userData);
            }
            WriteApiLog(apiCode, data.AppNO, GroupId, CMP, "*", "Succeed", ediRemark, "", createBy, mixedList);
        }

        /// <summary>
        /// 权限变更
        /// </summary>
        /// <param name="data"></param>
        /// <param name="subData"></param>
        /// <param name="info"></param>
        /// <param name="dt"></param>
        /// <param name="sysRemark"></param>
        /// <param name="mixedList"></param>
        public static void PermissionChange(ItsdArData data, ItsdSubData subData, EshppingInfo info, DataTable dt, string sysRemark, MixedList mixedList)
        {
            string Remark = dt.Rows.Count <= 0 ? "PermissionChange_" + subData.AD + "不存在" : "PermissionChange_" + subData.AD;
            MixedList ml = new MixedList();
            ItsdInfoUpdate(data, subData, info, dt, sysRemark, mixedList);
            WriteApiLog(info.EDI_CODE, data.AppNO, info.GROUP_ID, info.CMP, "*", "Succeed", Remark, "", info.CREATE_BY, mixedList);
        }

        public static void ItsdInfoUpdate(ItsdArData data, ItsdSubData subData, EshppingInfo info, DataTable dt, string sysRemark, MixedList mixedList)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string remark = Prolink.Math.GetValueAsString(dr["REMARK"]);

                if (!string.IsNullOrEmpty(remark))
                    remark += "\r\n" + sysRemark;
                else
                    remark = sysRemark;

                EditInstruct ei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("GROUP_ID", info.GROUP_ID);
                ei.PutKey("CMP", cmp);
                ei.PutKey("U_ID", subData.AD);
                ei.Put("IT_SD", data.AppNO);
                ei.Put("U_STATUS", 0);
                ei.Put("REMARK", remark);
                ei.Put("MODIFY_BY", info.CREATE_BY);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
                mixedList.Add(ei);

                BaseController controller = new BaseController();
                controller.WriteDBLog(mixedList, ei, data.AppNO, info.CREATE_BY, info.GROUP_ID, info.CMP);
            }
        }

    }
}
