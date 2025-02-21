using Business;
using Business.Service;
using Business.TPV.Financial;
using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;
using TrackingEDI.Mail;
using WebGui.App_Start;

namespace WebGui.Controllers
{
    public class IbGateManageController : BaseController
    {
        //
        // GET: /IbGateManage/

        #region View
        public ActionResult InOutManage()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IBRV080");
            return View();
        }

        public ActionResult GateReserve()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IBRV030");
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            return View();
        }
        public ActionResult GateReserveSetup(string id = null, string uid = null)
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            SetSchema("GateReserveSetup");
            if (uid == null)
            {
                uid = id;
            }

            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("IBRV030");
            ViewBag.RelationId = ids;
            ViewBag.CarType = CommonHelp.getReffeeForSelect(GetDataPmsCondition("C"));
            ViewBag.WsCol = GetWarehouseCol();
            ViewBag.IoFlag = IOFlag;
            return View();
        }

        private string GetWarehouseCol(bool isSelect=false)
        {
            string WsSelect = "";
            string sql = string.Format("SELECT * FROM DEST_MAP WHERE SEC_CMP={0}", SQLUtils.QuotedStr(CompanyId));
            DataTable secCmpDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> cmpList = new List<string>();
            cmpList.Add(CompanyId);
            foreach (DataRow secDr in secCmpDt.Rows)
            {
                string cmp = Prolink.Math.GetValueAsString(secDr["CMP"]);
                if (!cmpList.Contains(cmp))
                    cmpList.Add(cmp);
            }
            sql = string.Format("SELECT WS_CD, WS_NM FROM SMWH WHERE CMP IN {0} ORDER BY WS_CD ASC", SQLUtils.Quoted(cmpList.ToArray()));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sel = ":;";
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    string WsNm = Prolink.Math.GetValueAsString(item["WS_NM"]);
                    WsSelect += WsCd + ")*(" + WsNm + "(*)";
                    sel += WsCd + ":" + WsNm + ";";
                }
            }
            sel = sel.Remove(sel.Length - 1);
            if (isSelect)
                return WsSelect;
            return sel;
        }

        public ActionResult GateReserveTSA()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IBRVTSA");
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            return View();
        }
        public ActionResult GateReserveTSASetup(string id = null, string uid = null)
        {
            string ids = Prolink.Math.GetValueAsString(Request.Params["ids"]);
            SetSchema("GateReserveSetup");
            if (uid == null)
            {
                uid = id;
            }

            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("IBRVTSA");
            ViewBag.RelationId = ids;
            ViewBag.CarType = CommonHelp.getReffeeForSelect(GetDataPmsCondition("C"));
            ViewBag.WsCol = GetWarehouseCol();
            ViewBag.IoFlag = IOFlag;
            return View();
        }

        public ActionResult GateAnalysis()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM040");
            return View();
        }

        public ActionResult ConfirmReserve()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IBRV040");
            ViewBag.CarType = CommonHelp.getReffeeForSelect(GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult ConfirmReserveSetup(string id = null, string uid = null)
        {
            SetSchema("ConfirmReserveSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("IBRV040");
            ViewBag.CarType = CommonHelp.getReffeeForSelect(GetDataPmsCondition("C"));
            ViewBag.WsCol = GetWarehouseCol();
            ViewBag.IoFlag = IOFlag;
            return View();
        }

        public ActionResult GateSetup(string id = null, string uid = null)
        {
            SetSchema("GateSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("IBRV070");
            return View();
        }

        public ActionResult ContainerManage()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM070");
            return View();
        }

        public ActionResult SmrvSetup(string id = null, string uid = null)
        {
            SetSchema("ConfirmReserveSetup");
            if (uid == null)
            {
                uid = id;
            }
            if (string.IsNullOrEmpty(id))
                id = Request["id"];
            string flag = Request["flag"];

            ViewBag.Uid = id;
            ViewBag.Flag = flag;
            ViewBag.pmsList = GetBtnPms("IBRV080");
            ViewBag.WsCol = GetWarehouseCol();
            return View();
        }

        public ActionResult GateStatus()
        {
            string sql = "SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            string CmpNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.MenuBar = false;
            ViewBag.Cmp = CompanyId;
            ViewBag.CmpNm = CmpNm;
            ViewBag.pmsList = GetBtnPms("IBRV050");
            return View();
        }

        public ActionResult ReserveQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("SM080");
            return View();
        }

        public ActionResult OrderCarQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IBRV010");
            ViewBag.WsSelect = GetWarehouseCol(true);
            ViewBag.WsCol = GetWarehouseCol();
            ViewBag.CargoType = GetBscodeByMode("TCGT");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.CarType = CommonHelp.getReffeeForSelect(GetDataPmsCondition("C"));
            ViewBag.Priority = CommonHelp.getBscodeForColModel("PRI", GetDataPmsCondition("C"));
            ViewBag.CompanyId = CompanyId;
            SetRoleSelect();
            return View();
        }

        private void SetRoleSelect()
        {
            ViewBag.SelectRole = "";
            ViewBag.DefaultRole = "";

            #region Approve
            string sql = string.Format(@"SELECT APPROVE_GROUP,GROUP_DESCP FROM APPROVE_ATTR_D WHERE U_FID=(select U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR='BILLING' AND {0}) AND {1}
            ORDER BY SEQ_NO ASC", GetBaseCmp(), GetBaseCmp());


            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            string approvegroup = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                approvegroup = Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                if ("AM".Equals(approvegroup) || "AMM".Equals(approvegroup) || "AMMM".Equals(approvegroup))
                    continue;
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultRole = approvegroup;
                }
                select += approvegroup;
                select += ":" + Prolink.Math.GetValueAsString(dr["GROUP_DESCP"]);
            }
            select += ";Finish:Finish";
            ViewBag.SelectRole = select;
            #endregion
        }

        public ActionResult InternalTranQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IBRV090");
            ViewBag.WsSelect = GetWarehouseCol(true);
            ViewBag.CargoType = GetBscodeByMode("TCGT");
            ViewBag.TranType = CommonHelp.getBscodeForColModel("TNT", GetDataPmsCondition("C"));
            ViewBag.WsCol = GetWarehouseCol() ;
            return View();
        }

        public ActionResult GateQueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IBRV070");
            return View();
        }
        #endregion

        #region 基本方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "GateReserveSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMIRV WHERE 1=0";
                        break;
                    case "ConfirmReserveSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMIRV WHERE 1=0";
                        break;
                    case "GateSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMWH WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        private ActionResult GetBootstrapData(string table, string condition, string orderBy = "", string format = "yyyy-MM-dd HH:mm:ss")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                dt = GetStatusCountData(statusField, table, condition, Request.Params);
                pageSize = 1;
            }
            else
            {
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize, orderBy);
                if (resultType == "excel")
                    return ExportExcelFile(dt);
            }
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "", format)
            };
            return result.ToContent();
        }

        private ActionResult GetBaseData(string table, string condition, string colNames = "*", string orderBy = "", string qType = "")
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 20;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            switch (qType)
            {
                case "1":
                    dt = ModelFactory.InquiryData(colNames, table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                case "2":
                    dt = ModelFactory.InquiryData(colNames, table, condition, orderBy, pageIndex, pageSize, ref  recordsCount);
                    break;
                case "3":
                    dt = ModelFactory.InquiryData(colNames, table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                default:
                    dt = ModelFactory.InquiryData(colNames, table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
            }

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

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues)
        { 

            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "  SELECT '' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
            DataTable dtsum = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dtAll = dt.Copy();
            foreach (DataRow dr in dtsum.Rows)
            {
                dtAll.ImportRow(dr);
            }

            return dtAll;
        }

        #endregion

        #region 純查詢
        public ActionResult GetSmRvByDnNo()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string condition = " DN_NO=" + SQLUtils.QuotedStr(DnNo);
            return GetBaseData("SMIRV", condition, "*", "RESERVE_NO", "2");
        }
        public ActionResult GetSmRvByShipmentId()
        {
            string LotNo = Prolink.Math.GetValueAsString(Request.Params["LotNo"]);
            int pagesize = Prolink.Math.GetValueAsInt(Request.Params["Pagesize"]);
            if (pagesize <= 0)
            {
                pagesize = 20;
            }
            string condition = " LOT_NO=" + SQLUtils.QuotedStr(LotNo);

            //return GetBaseData("SMIRV", condition, "*", "RESERVE_NO", "2", pagesize);
            return GetBootstrapData("SMIRV", GetDataPmsCondition("C",true) + " AND" + condition);
        }

        public ActionResult GetSmRvByOrdNo()
        {
            string ordnos = Prolink.Math.GetValueAsString(Request.Params["OrdNo"]);
            string[] ordnoarray = ordnos.Split(';');
            string conditions = "(";
            for (int i=0;i<ordnoarray.Length;i++)
            {
                string ordno = ordnoarray[i];
                if (string.IsNullOrEmpty(ordno)) continue;
                if (i > 0)
                {
                    conditions += " OR ";
                }
                conditions += " ORD_INFO LIKE '%" + ordno + "%'";
            }
            conditions += ")";
            return GetBootstrapData("SMIRV", GetDataPmsCondition("C",true) + " AND" + conditions);
        }

        public ActionResult InternalTranQueryData()
        {
            string table = @"SMORD";
            string condition = GetDataPmsCondition("C", true) + " AND CSTATUS<>'N' AND CSTATUS IS NOT NULL AND TRAN_TYPE1 IN('R','S','A','I')";
            condition = GetCreateDateCondition("SMORD", condition);
            return GetBootstrapData(table, condition);
        }

        public ActionResult OrderCarQueryData()
        {
            string conditions =ModelFactory.ConvParam2Condition(HttpUtility.UrlDecode(Request.Params["conditions"]), "");
            string condition = GetDataPmsCondition("C", true) + " AND CSTATUS<>'N' AND CSTATUS IS NOT NULL AND TRAN_TYPE1='T'";
            //if (string.IsNullOrEmpty(condition))
            //    condition = GetDataPmsCondition("C",true);
            //else
            //    condition = GetDataPmsCondition("C", true) + " AND " + conditions;
            string table = @"(SELECT SMORD.* ,(SELECT TOP 1 ZT_NO FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMORD.SHIPMENT_ID) AS ZT_NO, 
            (SELECT TOP 1 ZT_NM FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMORD.SHIPMENT_ID) AS ZT_NM,
            (SELECT TOP 1 PARTY_NAME3 FROM SMSMIPT WHERE SHIPMENT_ID=SMORD.SHIPMENT_ID AND PARTY_TYPE='SP') AS PARTY_NAME3,
            (SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO=(SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SHIPMENT_ID=SMORD.SHIPMENT_ID AND PARTY_TYPE='SP') AND STATUS='U') AS HEAD_OFFICE
            FROM SMORD) SMORD";
            condition = GetCreateDateCondition("SMORD", condition);
            return GetBootstrapData(table, condition); 
        }

        public ActionResult GetGateData()
        {
            string condition = "";

            if ("O"== IOFlag)
            {
                condition = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND TRUCKER=" + SQLUtils.QuotedStr(CompanyId);
            }
            else
            {
                condition = string.Format(" GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) ",SQLUtils.QuotedStr(GroupId),SQLUtils.QuotedStr(BaseCompanyId));
            }

            condition += " AND RV_TYPE='I'";
            string table = @"(SELECT SMIRV.*,(SELECT TOP 1 ETD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIRV.SHIPMENT_ID)AS ETD FROM SMIRV where 1=1 AND CMP NOT IN (SELECT CD FROM BSCODE WHERE CD_TYPE='TSA')) SMIRV ";
            condition = GetCreateDateCondition("SMIRV", condition);
            return GetBootstrapData(table, condition);
        }

        public ActionResult GetGateDataTSA()
        {
            string condition = "";

            if ("O" == IOFlag) 
            {
                condition = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND TRUCKER=" + SQLUtils.QuotedStr(CompanyId);
            }
            else
            {
                condition = string.Format(" GROUP_ID={0} AND (CMP={1} OR SEC_CMP={1}) ", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(BaseCompanyId));
            }

            condition += " AND RV_TYPE='I'";
            string table = @"(SELECT SMIRV.*,(SELECT TOP 1 ETD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIRV.SHIPMENT_ID)AS ETD,
            (SELECT TOP 1 FINAL_WH FROM SMRCNTR WHERE SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO)AS FINAL_WH FROM SMIRV WHERE CMP IN (SELECT CD FROM BSCODE WHERE CD_TYPE='TSA')) SMIRV ";
            return GetBootstrapData(table, condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetGateItem()//GetDRuleItem
        {
            string u_id = Request["UId"];
            string sql = string.Empty;
            sql = string.Format("SELECT SMIRV.*,(SELECT COUNT(1) FROM SMRCNTR WHERE SMIRV.RESERVE_NO=SMRCNTR.RESERVE_NO)CNTR_COUNT,(SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMIRV.SHIPMENT_ID=SMSMI.SHIPMENT_ID) AS SMTYPE FROM SMIRV WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string s_uid = "", gp_id = "", cmp = "", BatNo = string.Empty;

            string InUrl = "";
            string OutUrl = "";

            Dictionary<string, object> data = new Dictionary<string, object>();
            if (groupDt.Rows.Count > 0)
            {
                BatNo = Prolink.Math.GetValueAsString(groupDt.Rows[0]["BAT_NO"]);
                sql = "SELECT B.U_ID,B.SHIPMENT_ID,B.GROUP_ID,B.CMP FROM SMIRV A, SMSM B WHERE A.SHIPMENT_ID=B.SHIPMENT_ID AND A.BAT_NO=" + SQLUtils.QuotedStr(BatNo);
                DataTable bDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                data["SmData"] = ModelFactory.ToTableJson(bDt, "SmsmModel");

                foreach (DataRow item in bDt.Rows)
                {
                    string sm_id = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                    sql = "SELECT U_ID, GROUP_ID, CMP FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (sdt.Rows.Count > 0)
                    {
                        foreach (DataRow item1 in sdt.Rows)
                        {
                            s_uid = Prolink.Math.GetValueAsString(item1["U_ID"]);
                            gp_id = Prolink.Math.GetValueAsString(item1["GROUP_ID"]);
                            cmp = Prolink.Math.GetValueAsString(item1["CMP"]);

                            #region 獲取照片
                            try
                            {
                                List<Dictionary<string, string>> list = MailTemplate.FileQueryDownlodInfo(s_uid, gp_id, cmp, "*", "", "SECPIC");
                                string urls = string.Empty;
                                if (list.Count > 0)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        string url = list[i]["FileUrl"];
                                        string Remark = list[i]["Remark"];
                                        string[] str = Remark.Split('-');

                                        if (str.Length >= 2)
                                        {
                                            if (str[1] == BatNo && str[0] == "IN")
                                            {
                                                InUrl = url;
                                            }
                                            else if (str[0] == "OUT" && str[1] == BatNo)
                                            {
                                                OutUrl = url;
                                            }
                                        }
                                        else
                                        {
                                            //king 20160705 增加相容舊的模式
                                            if (str[0] == "IN" && InUrl == "")
                                            {
                                                InUrl = url;
                                            }
                                            else if (str[0] == "OUT" && OutUrl == "")
                                            {
                                                OutUrl = url;
                                            }
                                        }


                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string s = ex.ToString();
                            }
                            #endregion
                        }
                    }

                }
            }

            string ReserveNo = groupDt.Rows[0]["RESERVE_NO"].ToString();
            sql = "SELECT * FROM SMRDN WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable ddt = getDataTableFromSql(sql);

            sql = "SELECT * FROM SMRCNTR WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable cdt = getDataTableFromSql(sql);


            data["main"] = ModelFactory.ToTableJson(groupDt, "SmirvModel");
            data["sub1"] = ModelFactory.ToTableJson(ddt, "SmrdnModel");
            data["sub2"] = ModelFactory.ToTableJson(cdt, "SmrcntrModel");
            data["InImg"] = InUrl;
            data["OutImg"] = OutUrl;

            return ToContent(data);

        }
        #endregion

        #region 取得預約狀況
        public JsonResult GetRVD()
        {
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["SearchCmp"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["SearchWsCd"]);
            string ReserveDate = Prolink.Math.GetValueAsString(Request.Params["SearchRDate"]);
            string returnMsg = "success";

            string sql = @"SELECT SMWHGT.CMP, 
                                                SMWHGT.WS_CD, 
                                                SMWHGT.GATE_NO,
                                                SMWHGT.LIFT,
                                                SMRVD.H_0, 
                                                SMRVD.H_1, 
                                                SMRVD.H_2, 
                                                SMRVD.H_3, 
                                                SMRVD.H_4, 
                                                SMRVD.H_5, 
                                                SMRVD.H_6,
                                                SMRVD.H_7, 
                                                SMRVD.H_8, 
                                                SMRVD.H_9, 
                                                SMRVD.H_10, 
                                                SMRVD.H_11, 
                                                SMRVD.H_12, 
                                                SMRVD.H_13,
                                                SMRVD.H_14,
                                                SMRVD.H_15, 
                                                SMRVD.H_16, 
                                                SMRVD.H_17, 
                                                SMRVD.H_18, 
                                                SMRVD.H_19, 
                                                SMRVD.H_20, 
                                                SMRVD.H_21,
                                                SMRVD.H_22, 
                                                SMRVD.H_23,
                                                SMRVD.C_0, 
                                                SMRVD.C_1, 
                                                SMRVD.C_2, 
                                                SMRVD.C_3, 
                                                SMRVD.C_4, 
                                                SMRVD.C_5, 
                                                SMRVD.C_6,
                                                SMRVD.C_7, 
                                                SMRVD.C_8, 
                                                SMRVD.C_9, 
                                                SMRVD.C_10, 
                                                SMRVD.C_11, 
                                                SMRVD.C_12, 
                                                SMRVD.C_13,
                                                SMRVD.C_14,
                                                SMRVD.C_15, 
                                                SMRVD.C_16, 
                                                SMRVD.C_17, 
                                                SMRVD.C_18, 
                                                SMRVD.C_19, 
                                                SMRVD.C_20, 
                                                SMRVD.C_21,
                                                SMRVD.C_22,
                                                SMRVD.C_23
                                  FROM   SMWHGT 
                                  LEFT JOIN SMRVD  ON SMRVD.WS_CD = SMWHGT.WS_CD  AND SMRVD.GATE_NO = SMWHGT.GATE_NO AND SMRVD.RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate)
                                  + "WHERE  SMWHGT.CMP = " + SQLUtils.QuotedStr(Cmp) + "  AND SMWHGT.WS_CD = " + SQLUtils.QuotedStr(WsCd) + " ORDER BY GATE_NO ASC";

            string sql1 = "SELECT * FROM SMWHT WHERE CMP={0} AND WS_CD={1}";
            sql1 = string.Format(sql1, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd));

            List<Dictionary<string, object>> returnData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> returnData1 = new List<Dictionary<string, object>>();
            try
            {
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable dt1 = OperationUtils.GetDataTable(sql1, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnData = ModelFactory.ToTableJson(dt);
                returnData1 = ModelFactory.ToTableJson(dt1);
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }

            return Json(new { message = returnMsg, data = returnData, data1 = returnData1 });
        }
        #endregion

        #region 保存
        public ActionResult SaveGateReseve()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = string.Empty;
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string reserveDate = Prolink.Math.GetValueAsString(Request.Params["ReserveDate"]);
            int reserveFrom = Prolink.Math.GetValueAsInt(Request.Params["ReserveFrom"]);
            string shipmentInfo = Prolink.Math.GetValueAsString(Request.Params["ShipmentInfo"]);
            string[] shipmentStr = new string[] { shipmentInfo };
            if (shipmentInfo.Contains(","))
                shipmentStr = shipmentInfo.Split(',');

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            DateTime pickupDate = new DateTime();
            string reserve_no = string.Empty;
            string irvReserveNo = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmirvModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;                        
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            string GateNo = Prolink.Math.GetValueAsString(ei.Get("GATE_NO"));
                            string TempWscd = Prolink.Math.GetValueAsString(ei.Get("WS_CD"));
                            string TempRdate = Prolink.Math.GetValueAsString(ei.Get("RESERVE_DATE"));
                            string TempRfrom = Prolink.Math.GetValueAsString(ei.Get("RESERVE_FROM"));
                            string TempRh = Prolink.Math.GetValueAsString(ei.Get("RESERVE_HOUR"));
                            string TruckNo = Prolink.Math.GetValueAsString(ei.Get("TRUCK_NO"));
                            string Driver = Prolink.Math.GetValueAsString(ei.Get("DRIVER"));
                            string Tel = Prolink.Math.GetValueAsString(ei.Get("TEL"));
                            string DriverId = Prolink.Math.GetValueAsString(ei.Get("DRIVER_ID"));
                            pickupDate = Prolink.Math.GetValueAsDateTime(ei.Get("USE_DATE"));
                            irvReserveNo = Prolink.Math.GetValueAsString(ei.Get("RESERVE_NO"));
                            string wsRmk =Prolink.Math.GetValueAsString(ei.Get("WS_RMK"));  
                            if (GateNo != "")
                            {
                                ei.Put("TEMP_GATENO", GateNo);
                            }
                            if (TempWscd != "")
                            {
                                ei.Put("TEMP_WSCD", TempWscd);
                            }
                            if (TempRdate != "")
                            {
                                ei.PutDate("TEMP_RDATE", TempRdate);
                            }
                            if (TempRfrom != "")
                            {
                                ei.Put("TEMP_RFROM", TempRfrom);
                            }
                            if (TempRh != "")
                            {
                                ei.Put("TEMP_RH", TempRh);
                            }

                            string Status = Prolink.Math.GetValueAsString(ei.Get("STATUS"));

                            if (Status == "I")
                            {
                                DateTime odt = DateTime.Now;                                
                                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                                
                                ei.PutDate("IN_DATE", odt);
                                ei.PutDate("IN_DATE_L", ndt);
                            }

                            sql = @"SELECT TOP 1 SMSMI.TRAN_TYPE FROM SMSMI INNER JOIN SMIRV ON SMSMI.SHIPMENT_ID=SMIRV.SHIPMENT_ID WHERE SMIRV.U_ID=" + SQLUtils.QuotedStr(UId);
                            string TranType = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                            if (TranType != "F")
                            {
                                if (TruckNo != "")
                                {
                                    ei.Put("LTRUCK_NO", TruckNo);
                                }

                                if (Driver != "")
                                {
                                    ei.Put("LDRIVER", Driver);
                                }

                                if (Tel != "")
                                {
                                    ei.Put("LTEL", Tel);
                                }

                                if (DriverId != "")
                                {
                                    ei.Put("LDRIVER_ID", DriverId);
                                }
                            }
                            DateTime result = DateTime.Now;
                            if ((DateTime.TryParseExact(reserveDate, new string[] { "yyyy-MM-dd", "yyyyMMdd", "dd MMM yyyy","yyyy/MM/dd" }, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out result))
                                ||DateTime.TryParse(reserveDate, out result))
                            {
                                result = result.AddHours(reserveFrom);
                                string arrivalDate = result.ToString("yyyy-MM-dd HH:mm");
                                sql = string.Format("UPDATE SMRDN SET ARRIVAL_DATE={0} WHERE RESERVE_NO={1}", SQLUtils.QuotedStr(arrivalDate), SQLUtils.QuotedStr(irvReserveNo));
                                mixList.Add(sql);
                                sql = string.Format("UPDATE SMRCNTR SET ARRIVAL_DATE={0} WHERE RESERVE_NO={1}", SQLUtils.QuotedStr(arrivalDate), SQLUtils.QuotedStr(irvReserveNo));
                                mixList.Add(sql);
                            }

                            if (ei.Get("WS_RMK") != null)
                            {
                                sql = string.Format(@"SELECT CNTR_NO FROM SMRCNTR WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(irvReserveNo));
                                string cntrNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (!string.IsNullOrEmpty(cntrNo))
                                    sql = string.Format("UPDATE SMORD SET WS_RMK={0} WHERE SHIPMENT_ID IN{1} AND CNTR_NO={2}", SQLUtils.QuotedStr(wsRmk), SQLUtils.Quoted(shipmentStr), SQLUtils.QuotedStr(cntrNo));
                                else
                                    sql = string.Format("UPDATE SMORD SET WS_RMK={0} WHERE SHIPMENT_ID IN{1}", SQLUtils.QuotedStr(wsRmk), SQLUtils.Quoted(shipmentStr));
                                mixList.Add(sql);
                            }
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                            ei.AddKey("U_ID");

                       
                        if (ei.OperationType != EditInstruct.DELETE_OPERATION)
                        {
                            List<string> names = new List<string>() { };
                            names.AddRange(ei.getNameSet());
                            if (names.Contains("EMPTY_TIME"))
                                ei.Remove("EMPTY_TIME");
                        }
                        ei.Remove("Smtype"); 
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmrcntrModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            string su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            string rid = Prolink.Math.GetValueAsString(ei.Get("RESERVE_NO"));
                            ei.PutKey("RESERVE_NO", rid);
                            ei.PutKey("U_ID", su_id);
                        }

                        if (ei.OperationType != EditInstruct.DELETE_OPERATION && !string.IsNullOrEmpty(ei.Get("EMPTY_TIME")) && !string.IsNullOrEmpty(ei.Get("RESERVE_NO")))
                        {
                            reserve_no = ei.Get("RESERVE_NO");
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmrdnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            string su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            string rid = Prolink.Math.GetValueAsString(ei.Get("RESERVE_NO"));
                            ei.PutKey("RESERVE_NO", rid);
                            ei.PutKey("U_ID", su_id);
                        }

                        mixList.Add(ei);
                    }
                }
            }

            UpdateEmptyDate(reserve_no, UserId, "", mixList);
            TrackingEDI.InboundBusiness.InboundHelper.UpdateDetDueDate(irvReserveNo, pickupDate,mixList);
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            List<Dictionary<string, object>> rvData = new List<Dictionary<string, object>>();
            sql = string.Format("SELECT * FROM SMIRV WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            rvData = ModelFactory.ToTableJson(groupDt, "SmirvModel");


            return Json(new { message = returnMessage, mainData = rvData });
        }

        
        #endregion

        #region 卡車公司預約
        public ActionResult ReverseGate()
        {
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = true;
            string BatNo = string.Empty;
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    string GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    //string ReserveDate = Prolink.Math.GetValueAsString(item["RESERVE_DATE"]);
                    string ReserveDate = ((DateTime)item["RESERVE_DATE"]).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    int ReserveFrom = Prolink.Math.GetValueAsInt(item["RESERVE_FROM"]);
                    int Hour = Prolink.Math.GetValueAsInt(item["RESERVE_HOUR"]);
                    string LotNo = Prolink.Math.GetValueAsString(item["LOT_NO"]);
                    string ShipmentInfo = Prolink.Math.GetValueAsString(item["SHIPMENT_INFO"]);
                    string ordinfo = Prolink.Math.GetValueAsString(item["ORD_INFO"]);
                    string[] ordnos = ordinfo.Split(',');

                    if (ReserveDate != "")
                    {
                        //chk = InsertRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour);
                        //if (chk == true)
                        //{
                        DateTime odt = DateTime.Now;                  
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        sql = "UPDATE SMIRV SET STATUS='R', ORDER_BY=" + SQLUtils.QuotedStr(UserId) + ", ORDER_DATE=" + SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")) + ", ORDER_DATE_L=" + SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")) + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);

                            try
                            {
                                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                chk = true;

                                foreach (string ordno in ordnos)
                                {
                                    int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(ordno));
                                    int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(ordno));

                                    if (r == c)
                                    {
                                        //更新運輸單的Cstatus
                                        sql = "UPDATE SMORD SET CSTATUS='R' WHERE ORD_NO={0}";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(ordno));
                                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    }
                                }

                                sql = "UPDATE SMSMI SET PICKUP_CDATE={0} WHERE SHIPMENT_ID IN {1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(ReserveDate + " " + ReserveFrom.ToString() + ":00:00"), SQLUtils.Quoted(ShipmentInfo.Split(',')));
                                exeSql(sql);

                                sql = "SELECT RESERVE_NO FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                                string ReserveNo = getOneValueAsStringFromSql(sql);
                                
                                sql = "UPDATE SMRDN SET SMRDN.PICKUP_DATE=SMIRV.USE_DATE FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRDN.RESERVE_NO AND SMRDN.RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                exeSql(sql);

                                sql = "UPDATE SMRCNTR SET SMRCNTR.PICKUP_DATE=SMIRV.USE_DATE FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRCNTR.RESERVE_NO AND SMRCNTR.RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                exeSql(sql);

                                sql = "UPDATE SMIDN SET SMIDN.DISCHARGE_DATE=SMRDN.DISCHARGE_DATE,SMIDN.PICKUP_CDATE=SMRDN.PICKUP_DATE FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                exeSql(sql);

                            sql = @"UPDATE SMORD SET SMORD.PICKUP_CDATE=SMRDN.PICKUP_DATE FROM SMRDN WHERE SMRDN.ORD_NO=SMORD.ORD_NO AND SMRDN.RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                            exeSql(sql);

                            string sql2 = "SELECT * FROM SMRDN WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                DataTable dt2 = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (dt2.Rows.Count > 0)
                                {
                                    foreach (DataRow item2 in dt2.Rows)
                                    {
                                        string ShipmentID = Prolink.Math.GetValueAsString(item2["SHIPMENT_ID"]);
                                        string DnNo = Prolink.Math.GetValueAsString(item2["DN_NO"]);
                                        sql = "UPDATE SMIDN SET CALL_TRUCK_STATUS='R' WHERE SHIPMENT_ID={0} AND DN_NO={1}";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentID), SQLUtils.QuotedStr(DnNo));
                                        exeSql(sql);
                                    }
                                }
                                
                                sql = @"UPDATE SMICNTR SET SMICNTR.PICKUP_CDATE=SMRCNTR.PICKUP_DATE,SMICNTR.EMPTY_TIME=SMRCNTR.EMPTY_TIME,SMICNTR.DISCHARGE_DATE=SMRCNTR.DISCHARGE_DATE FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                exeSql(sql);

                            sql = @"UPDATE SMORD SET SMORD.PICKUP_CDATE=SMRCNTR.PICKUP_DATE FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMORD.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMRCNTR.RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                            exeSql(sql);



                            sql2 = "SELECT * FROM SMRCNTR WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                                dt2 = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (dt2.Rows.Count > 0)
                                {
                                    foreach (DataRow item2 in dt2.Rows)
                                    {
                                        string ShipmentID = Prolink.Math.GetValueAsString(item2["SHIPMENT_ID"]);
                                        string CntrNo = Prolink.Math.GetValueAsString(item2["CNTR_NO"]);
                                        sql = "UPDATE SMICNTR SET CALL_TRUCK_STATUS='R' WHERE SHIPMENT_ID={0} AND CNTR_NO={1}";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentID), SQLUtils.QuotedStr(CntrNo));
                                        exeSql(sql);
                                    }
                                }

                                sql=string.Format("SELECT MIN(ARRIVAL_DATE) FROM SMRCNTR WHERE RESERVE_NO={0}",SQLUtils.QuotedStr(ReserveNo));
                                string arrivaldate = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if(string.IsNullOrEmpty(arrivaldate)){
                                    sql = string.Format("UPDATE SMIRV SET ARRIVAL_DATE=(SELECT MIN(ARRIVAL_DATE) FROM SMRDN WHERE SMRDN.RESERVE_NO=SMIRV.RESERVE_NO) WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(ReserveNo));
                                }else{
                                    sql = string.Format("UPDATE SMIRV SET ARRIVAL_DATE=(SELECT MIN(ARRIVAL_DATE) FROM SMRCNTR WHERE SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO) WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(ReserveNo));
                                }
                                exeSql(sql);
                            }
                            catch (Exception ex)
                            {
                                chk = false;
                            }
                        //}

                    }
                    else
                    {
                        returnMessage = @Resources.Locale.L_GateManageController_Controllers_143;
                    }

                }
            }

            if (chk == false)
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_144;
            }

            return Json(new { message = returnMessage, BatNo = BatNo });
        }
        #endregion

        #region 檢查預約明細是否被預約
        private Boolean ChkRVD(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour, string UId)
        {
            bool IsOk = true;




            return IsOk;
        }
        #endregion

        #region 將預約單寫入明細
        private Boolean InsertRVDRecode(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour)
        {
            string sql = "";
            bool success = false;
            string con = "CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo) + " AND RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate);
            sql = "SELECT COUNT(*) FROM SMRVD WHERE " + con;
            string comma = ",";
            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dt = new DataTable();
            if (num > 0)
            {
                string sql1 = "SELECT TOP 1 * FROM SMRVD WHERE " + con;
                dt = OperationUtils.GetDataTable(sql1, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        int h = Prolink.Math.GetValueAsInt(item["H_" + Hour.ToString()]);

                        if (h == 1)
                        {
                            return false;
                        }
                        else
                        {
                            bool tag = true;

                            /*從預約的時間往後加，檢查位子是都沒人預約*/
                            for (int i = 1; i <= Hour; i++)
                            {
                                int after = ReserveFrom + 1;
                                h = Prolink.Math.GetValueAsInt(item["H_" + after.ToString()]);
                                if (h == 1)
                                {
                                    tag = false;
                                    break;
                                }
                            }

                            if (tag == true)
                            {
                                string str_h = "", str_c = "";
                                for (int i = 0; i < Hour; i++)
                                {
                                    if (ReserveFrom + i > 23)
                                    {
                                        break; //超過23要換天
                                    }
                                    if (i + 1 == Hour)
                                    {
                                        str_h += " H_" + (ReserveFrom + i).ToString() + "=1 ";
                                        str_c += " C_" + (ReserveFrom + i).ToString() + "=" + SQLUtils.QuotedStr(CompanyId);
                                    }
                                    else
                                    {
                                        if (ReserveFrom + i == 23)
                                        {
                                            comma = "";
                                        }
                                        str_h += " H_" + (ReserveFrom + i).ToString() + "=1" + comma;
                                        str_c += " C_" + (ReserveFrom + i).ToString() + "=" + SQLUtils.QuotedStr(CompanyId) + comma;
                                    }
                                }
                                sql = @"UPDATE SMRVD SET " + str_h + "," + str_c + " WHERE " + con;
                            }
                            else
                            {
                                return false;
                            }

                        }
                    }
                }
            }
            else
            {
                string str_h = "", str_c = "", str_hv = "", str_cv = "";
                for (int i = 0; i < Hour; i++)
                {
                    if (ReserveFrom + i > 23)
                    {
                        break;//超過23要換天
                    }
                    if (i + 1 == Hour)
                    {
                        str_h += " H_" + (ReserveFrom + i).ToString() + " ";
                        str_c += " C_" + (ReserveFrom + i).ToString() + " ";
                        str_hv += " 1 ";
                        str_cv += SQLUtils.QuotedStr(CompanyId) + " ";
                    }
                    else
                    {
                        if (ReserveFrom + i == 23)
                        {
                            comma = "";
                        }
                        str_h += " H_" + (ReserveFrom + i).ToString() + comma;
                        str_c += " C_" + (ReserveFrom + i).ToString() + comma;
                        str_hv += " 1" + comma;
                        str_cv += SQLUtils.QuotedStr(CompanyId) + comma;
                    }
                }
                sql = "INSERT INTO SMRVD(CMP, WS_CD, GATE_NO, RESERVE_DATE, " + str_h + "," + str_c + ") VALUES(" + SQLUtils.QuotedStr(Cmp) + "," + SQLUtils.QuotedStr(WsCd) + "," + SQLUtils.QuotedStr(GateNo) + "," + SQLUtils.QuotedStr(ReserveDate) + "," + str_hv + "," + str_cv + ")";
            }

            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }
        #endregion

        #region 更新预约明细
        private Boolean UpdateRVDRecode(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour)
        {
            bool success = false;
            string str_h = "", str_c = "";
            string comma = ",";
            string con = "CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo) + " AND RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate);
            for (int i = 0; i < Hour; i++)
            {
                if (ReserveFrom + i > 23)
                {
                    break;//超過23要換天
                }
                if (i + 1 == Hour)
                {
                    str_h += " H_" + (ReserveFrom + i).ToString() + "=null ";
                    str_c += " C_" + (ReserveFrom + i).ToString() + "=null";
                }
                else
                {
                    if (ReserveFrom + i == 23)
                    {
                        comma = "";
                    }
                    str_h += " H_" + (ReserveFrom + i).ToString() + "=null" + comma;
                    str_c += " C_" + (ReserveFrom + i).ToString() + "=null" + comma;
                }
            }
            string sql = "UPDATE  SMRVD SET " + str_h + "," + str_c + " WHERE " + con;

            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }
        #endregion

        #region 预约确认
        public ActionResult ConfirmReverseGate()
        { 
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = true;
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string LotNo = string.Empty;
            string ReserveNo = string.Empty;
            string ordinfo = string.Empty;
            MixedList ml = new MixedList();
            if (dt.Rows.Count > 0)
            {
                DateTime pickupdate = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["USE_DATE"]);
                string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                string shipmentidinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);
                ReserveNo = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
                string Status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
                ordinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["ORD_INFO"]);
                List<string> list = new List<string>();
                if (!string.IsNullOrEmpty(shipmentid))
                    list.Add(shipmentid);
                string []shipments=shipmentidinfo.Split(new char[] {';',',' });
                foreach (string s in shipments)
                {
                    if (string.IsNullOrEmpty(s))
                        continue;
                    if (list.Contains(s))
                        continue;
                    list.Add(s);
                }
                foreach (string shipmentindex in list)
                {
                    EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", shipmentindex);
                    ei.PutDate("PICKUP_CDATE", pickupdate);
                    ml.Add(ei);

                    EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    smicntrei.PutKey("SHIPMENT_ID", shipmentindex);
                    smicntrei.PutDate("PICKUP_CDATE", pickupdate);
                    ml.Add(smicntrei);

                    EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    smordei.PutKey("SHIPMENT_ID", shipmentindex);
                    smordei.PutDate("PICKUP_CDATE", pickupdate);
                    ml.Add(smordei);
                }
            }
            if(dt.Rows.Count > 0)
            {
                ReserveNo = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
            }

            if (chk == true)
            {
                EditInstruct ei;
                int[] resulst;
                ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", UId);
                ei.Put("CONFIRM_BY", UserId);
                DateTime odt = DateTime.Now;                
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("CONFIRM_DATE", odt);
                ei.PutDate("CONFIRM_DATE_L", ndt);
                ei.Put("STATUS", "C");
                ml.Add(ei);

                try
                {
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string[] ordnos = ordinfo.Split(',');
                    foreach (string ordno in ordnos)
                    {
                        int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(ordno));
                        int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(ordno));
                        if (r == c)
                        {
                            sql = "UPDATE SMORD SET CSTATUS='C' WHERE ORD_NO={0}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ordno));
                            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }                      
                    }

                    string sql2 = "SELECT * FROM SMRDN WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    DataTable dt2 = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt2.Rows.Count > 0)
                    {
                        foreach (DataRow item2 in dt2.Rows)
                        {
                            string ShipmentID = Prolink.Math.GetValueAsString(item2["SHIPMENT_ID"]);
                            string DnNo = Prolink.Math.GetValueAsString(item2["DN_NO"]);
                            sql = "UPDATE SMIDN SET CALL_TRUCK_STATUS='C' WHERE SHIPMENT_ID={0} AND DN_NO={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentID), SQLUtils.QuotedStr(DnNo));
                            exeSql(sql);
                        }
                    }

                    sql2 = "SELECT * FROM SMRCNTR WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    dt2 = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt2.Rows.Count > 0)
                    {
                        foreach (DataRow item2 in dt2.Rows)
                        {
                            string ShipmentID = Prolink.Math.GetValueAsString(item2["SHIPMENT_ID"]);
                            string CntrNo = Prolink.Math.GetValueAsString(item2["CNTR_NO"]);
                            sql = "UPDATE SMICNTR SET CALL_TRUCK_STATUS='C' WHERE SHIPMENT_ID={0} AND CNTR_NO={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentID), SQLUtils.QuotedStr(CntrNo));
                            exeSql(sql);
                        }
                    }
                }
                catch (Exception ex)
                {
                    chk = false;
                    returnMessage = ex.Message;
                }
            }

            return Json(new { message = returnMessage });
        }
        #endregion

        public ActionResult SetStatus()
        {
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string type = Prolink.Math.GetValueAsString(Request.Params["type"]);
            string sql = "SELECT TOP 1 * FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string LotNo = string.Empty;
            string ReserveNo = string.Empty;
            string idcombinedp = string.Empty;
            string ordinfo = string.Empty;
            MixedList ml = new MixedList();
            if (dt.Rows.Count > 0)
            {
                ReserveNo = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
                idcombinedp = Prolink.Math.GetValueAsString(dt.Rows[0]["IS_COMBINE_DP"]);
            }
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", UId);
            if ("void".Equals(type))
            {
                ei.Put("STATUS", "K");
            }
            if ("restore".Equals(type))
            {
                if("D".Equals(idcombinedp))
                    ei.Put("STATUS", "W");
                else
                    ei.Put("STATUS", "T");
            }
            ml.Add(ei);

            try
            {
                int[] resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }

        public ActionResult SetAutoCreateCancel()
        {
            string returnMessage = "success";
            string Reserveno = Prolink.Math.GetValueAsString(Request.Params["Reserveno"]);
            string[] reservenos = Reserveno.Split(',');
            MixedList ml = new MixedList();
            foreach(string reserve in reservenos){
                if (string.IsNullOrEmpty(reserve))
                    continue;
                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("RESERVE_NO", reserve);
                ei.PutKey("IS_AUTOCREATE", "Y");
                ei.Put("STATUS", "V");
                ml.Add(ei);

                ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.Condition = string.Format("SHIPMENT_ID=(SELECT SHIPMENT_INFO FROM SMIRV WHERE RESERVE_NO={0})", SQLUtils.QuotedStr(reserve));
                ei.Put("STATUS", "V");
                ml.Add(ei);
            }

            try
            {
                int[] resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }


        #region 月台動態action
        /*離開月台*/
        public JsonResult setLeaveGate()
        {
            string returnMessage = "success";
            string ReserveNo = Prolink.Math.GetValueAsString(Request.Params["ReserveNo"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string GateNo = Prolink.Math.GetValueAsString(Request.Params["GateNo"]);
            string now = DateTime.Now.ToString();
            MixedList mixList = new MixedList();
            EditInstruct ei;
            EditInstruct ei2;
            if (ReserveNo == "")
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_310;
                return Json(new { msg = returnMessage });
            }

            string sql = "SELECT TOP 1 * FROM SMIRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string UId = Prolink.Math.GetValueAsString(item["U_ID"]);
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string ordinfo = Prolink.Math.GetValueAsString(item["ORD_INFO"]);
                    string[] ordnos = ordinfo.Split(',');

                    sql = "UPDATE SMWHGT SET CNTR_NO='',RESERVE_NO='' WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo);
                    mixList.Add(sql);
                    ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", UId);
                    ei.Put("STATUS", "G");
                    //ei.PutDate("OUT_DATE", DateTime.Now);
                    mixList.Add(ei);

                    //sql = "SELECT DN_NO FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                    //string DnNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //ei2 = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    //ei2.PutKey("DN_NO", DnNo);
                    //ei2.Put("STATUS", "O");
                    //ei2.PutDate("OUT_DATE", DateTime.Now);
                    //mixList.Add(ei2);

                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        foreach (string ordno in ordnos)
                        {
                            int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(ordno));
                            int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(ordno));

                            if (r == c)
                            {
                                sql = "UPDATE SMORD SET CSTATUS='G' WHERE ORD_NO={0}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(ordno));
                                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.Message;
                        return Json(new { msg = returnMessage });
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_140;
            }

            return Json(new { msg = returnMessage });
        }
        /*移動貨櫃*/
        public JsonResult setMoveGate()
        {
            string returnMessage = "success";
            string ReserveNo = Prolink.Math.GetValueAsString(Request.Params["ReserveNo"]);
            string oldWsCd = Prolink.Math.GetValueAsString(Request.Params["oldWsCd"]);
            string newWsCd = Prolink.Math.GetValueAsString(Request.Params["newWsCd"]);
            string oldGateNo = Prolink.Math.GetValueAsString(Request.Params["oldGateNo"]);
            string newGateNo = Prolink.Math.GetValueAsString(Request.Params["newGateNo"]);
            MixedList mixList = new MixedList();
            string sql = "SELECT TOP 1 * FROM SMIRV WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei;

            if (ReserveNo == "")
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_310;
                return Json(new { msg = returnMessage });
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string UFid = Prolink.Math.GetValueAsString(item["U_ID"]);
                    string CntrNo = Prolink.Math.GetValueAsString(item["CNTR_NO"]);
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string Trucker = Prolink.Math.GetValueAsString(item["TRUCKER"]);

                    string TruckCntrno = Prolink.Math.GetValueAsString(item["TRUCK_CNTRNO"]);
                    string TruckNo = Prolink.Math.GetValueAsString(item["TRUCK_NO"]);

                    if (CntrNo == "" && TruckCntrno == "")
                    {
                        CntrNo = TruckNo;
                    }
                    else if (CntrNo == "")
                    {
                        CntrNo = TruckCntrno;
                    }

                    sql = "UPDATE SMWHGT SET CNTR_NO=" + SQLUtils.QuotedStr(CntrNo) + ",RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo) + " WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(newWsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(newGateNo);
                    mixList.Add(sql);
                    sql = "UPDATE SMWHGT SET CNTR_NO='',RESERVE_NO='' WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(oldWsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(oldGateNo);
                    mixList.Add(sql);
                    sql = "UPDATE SMIRV SET WS_CD=" + SQLUtils.QuotedStr(newWsCd) + ", GATE_DATE=GETDATE(), GATE_NO=" + SQLUtils.QuotedStr(newGateNo) + " WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    mixList.Add(sql);
                    sql = "SELECT COUNT(*) AS SEQ_NO FROM SMRVM WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    int SeqNo = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    ei = new EditInstruct("SMRVM", EditInstruct.INSERT_OPERATION);
                    ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    ei.Put("U_FID", UFid);
                    ei.Put("SEQ_NO", SeqNo + 1);
                    ei.Put("CNTR_NO", CntrNo);
                    ei.Put("TRUCKER", Trucker);
                    ei.Put("TRUCK_NO", TruckNo);
                    DateTime odt = DateTime.Now;                  
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    ei.PutDate("MOVING_DATE", odt);
                    ei.PutDate("MOVING_DATE_L", ndt);
                    ei.Put("OWS_CD", oldWsCd);
                    ei.Put("OGATE_NO", oldGateNo);
                    ei.Put("NWS_CD", newWsCd);
                    ei.Put("NGATE_NO", newGateNo);
                    ei.Put("RESERVE_NO", ReserveNo);
                    ei.Put("MODIFY_BY", UserId);
                    ei.PutDate("MODIFY_DATE", odt);
                    ei.PutDate("MODIFY_DATE_L", ndt);
                    ei.Put("BAT_NO", ReserveNo);

                    mixList.Add(ei);

                    sql = "UPDATE SMIRV SET MOVE_NUMBER=" + (SeqNo + 1) + " WHERE BAT_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    mixList.Add(sql);
                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //Business.ReserveManage.syncBatNo(UFid);
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.Message;
                        return Json(new { msg = returnMessage });
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_141;
            }



            return Json(new { msg = returnMessage });
        }
        /*進入月台*/
        public JsonResult setEnterToGate()
        {
            string returnMessage = "success";
            string ReserveNo = Prolink.Math.GetValueAsString(Request.Params["ReserveNo"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string GateNo = Prolink.Math.GetValueAsString(Request.Params["GateNo"]);
            string sql = "SELECT TOP 1 * FROM SMIRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (ReserveNo == "")
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_141;
                return Json(new { msg = returnMessage });
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string CntrNo = Prolink.Math.GetValueAsString(item["CNTR_NO"]);
                    string TruckCntrno = Prolink.Math.GetValueAsString(item["TRUCK_CNTRNO"]);
                    string TruckNo = Prolink.Math.GetValueAsString(item["TRUCK_NO"]);
                    //string GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    string UId = Prolink.Math.GetValueAsString(item["U_ID"]);
                    string BatNo = Prolink.Math.GetValueAsString(item["BAT_NO"]);
                    string is_autocreate=Prolink.Math.GetValueAsString(item["IS_AUTOCREATE"]);
                    string DnNo=Prolink.Math.GetValueAsString(item["DN_NO"]);
                    string ordinfo = Prolink.Math.GetValueAsString(item["ORD_INFO"]);
                    string[] ordnos = ordinfo.Split(',');

                    if (CntrNo == "" && TruckCntrno == "")
                    {
                        CntrNo = TruckNo;
                    }
                    else if (CntrNo == "")
                    {
                        CntrNo = TruckCntrno;
                    }

                    if ("Y".Equals(is_autocreate))
                    {
                        if (string.IsNullOrEmpty(CntrNo))
                            CntrNo = DnNo;
                    }

                    sql = "UPDATE SMWHGT SET CNTR_NO=" + SQLUtils.QuotedStr(CntrNo) + ",RESERVE_NO=" + SQLUtils.QuotedStr(BatNo) + " WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo);
                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        sql = "UPDATE SMIRV SET STATUS='G', WS_CD={0}, GATE_DATE=GETDATE(), GATE_NO={1}, TEMP_WSCD={0},TEMP_GATENO={1} WHERE BAT_NO={2}";
                        if ("Y".Equals(is_autocreate) || string.IsNullOrEmpty(BatNo))
                        {
                            BatNo = ReserveNo;
                            sql = "UPDATE SMIRV SET STATUS='G', WS_CD={0}, GATE_DATE=GETDATE(), GATE_NO={1}, TEMP_WSCD={0},TEMP_GATENO={1} WHERE RESERVE_NO={2}";
                        }
                        sql = string.Format(sql, SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(GateNo), SQLUtils.QuotedStr(BatNo));
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        foreach(string ordno in ordnos)
                        {
                            sql = "UPDATE SMORD SET CSTATUS='G' WHERE ORD_NO={0}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ordno));
                            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
              
                        //Business.ReserveManage.syncBatNo(UId);
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.Message;
                        return Json(new { msg = returnMessage });
                    }
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_141;
            }

            return Json(new { msg = returnMessage });
        }

        /*取得今日入廠車輛by gate no*/
        public ActionResult getTodayWaitTrucker()
        {
            string Today = Prolink.Math.GetValueAsString(Request.Params["Today"]);

            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["Shipmentid"]);
            string dnno = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);

            string condition = " STATUS='I'" + " AND " + GetBaseSecCmp();
            if (!string.IsNullOrEmpty(shipmentid))
            {
                condition += string.Format(" AND (SHIPMENT_ID LIKE '%{0}%' OR RESERVE_NO LIKE '%{0}%')", shipmentid);
            }
            if (!string.IsNullOrEmpty(dnno))
            {
                condition += string.Format(" AND DN_NO LIKE '%{0}%'", dnno);
            }
            return GetBaseData("SMIRV", condition + " AND RV_TYPE='I'", "*", " RESERVE_NO ASC", "2");
        }
        #endregion

        #region 月台管理分析
        /*月台管理分析*/
        public ActionResult gateAnalysisQuery()
        {
            string condition = GetBaseSecCmp();

            string table = @"(SELECT 
(SELECT TOP 1 U_ID FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS U_ID,
SMSMI.GROUP_ID,SMSMI.CMP,SMSMI.STN,SMSMI.DEP,SMSMI.CREATE_DATE,
SMSMI.POL_NAME,SMSMI.POD_CD,SMSMI.POD_NAME,SMSMI.DEST_CD,SMSMI.DEST_NAME,smsmi.CARRIER,
SMSMI.SHIPMENT_ID,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='CS') AS CONSIGNEE,INV_NO,MASTER_NO,HOUSE_NO,
(SELECT TOP 1 OPART_NO FROM SMIDNP WHERE SMIDNP.INV_NO=SMIDN.INV_NO AND (NEW_CATEGORY !='TANN' OR NEW_CATEGORY IS NULL)) AS OPART_NO,
(SELECT TOP 1 IPART_NO FROM SMIDNP WHERE SMIDNP.INV_NO=SMIDN.INV_NO AND (NEW_CATEGORY !='TANN' OR NEW_CATEGORY IS NULL)) AS IPART_NO,
(SELECT TOP 1 PART_NO FROM SMIDNP WHERE SMIDNP.INV_NO=SMIDN.INV_NO AND (NEW_CATEGORY !='TANN' OR NEW_CATEGORY IS NULL)) AS PART_NO,
(SELECT TOP 1 SO_NO FROM SMIDNP WHERE SMIDNP.INV_NO=SMIDN.INV_NO AND (NEW_CATEGORY !='TANN' OR NEW_CATEGORY IS NULL)) AS SO_NO,
(SELECT TOP 1 SEAL_NO1 FROM SMIDNP WHERE SMIDNP.INV_NO=SMIDN.INV_NO AND (NEW_CATEGORY !='TANN' OR NEW_CATEGORY IS NULL)) AS SEAL_NO1,
(SELECT TOP 1 SEAL_NO2 FROM SMIDNP WHERE SMIDNP.INV_NO=SMIDN.INV_NO AND (NEW_CATEGORY !='TANN' OR NEW_CATEGORY IS NULL)) AS SEAL_NO2,
(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='SH') AS SHIPPER,Pol_cd,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE in ('SP','BO')) AS SP,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='FS') AS FS,(SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='IBCR') AS IBCR_CODE,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='IBCR') AS IBCR_NAME,(SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='IBBR') AS IBBR_CODE,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='IBBR') AS IBBR_NAME,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='FC') AS FC_NAME,(SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=
SMIDN.SHIPMENT_ID AND PARTY_TYPE='FC') AS FC_CODE,SMSMI.TRAN_TYPE,Incoterm_Cd,Incoterm_Descp,SMIDN.QTY AS INV_QTY,frt_term,
Pol1,Pol_Nm1,Vessel1,Voyage1,Vessel2,Voyage2,Vessel3,Voyage3,Vessel4,Voyage4,ETD1,ETA1,ETD2,ETA2,ETD3,ETA3,ETD4,ETA4,
ETD,ETA,ATP,ATD,ATA,
---Gate In Factory
---T/T{ETA(D)-ETD(O)}
PONO_INPUT_DATE,
CASE WHEN PO_NO IS NULL THEN (SELECT TOP 1 PO_NO FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMIDN.SHIPMENT_ID
 AND SMICNTR.Dn_No LIKE '%'+SMIDN.DN_NO+'%') else PO_NO END AS PO_NO,
SMSMI.GW,SMSMI.CBM,--SMSM.NW
(SELECT TOP 1 CNTR_NO FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMICNTR.Dn_No LIKE '%'+SMIDN.DN_NO+'%') AS CNTR_NO,
(SELECT TOP 1 CNT_TYPE FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMICNTR.Dn_No LIKE '%'+SMIDN.DN_NO+'%') AS CNT_TYPE,
SMIDN.Dec_no,SMIDN.Dec_Date,SMIDN.Cc_Channel,SMIDN.Rel_Date,SMIDN.CNTRY_CD,
--POD_DATE,
Lsp_Abnormal_Rmk,(SELECT TOP 1 Truck_RMK FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS Truck_RMK,
(SELECT TOP 1 Ws_Rmk FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS Ws_Rmk,
(SELECT TOP 1 Reserve_Date FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS Reserve_Date,
(SELECT TOP 1 Use_Date FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS Use_Date,
(SELECT TOP 1 Eta_Railramp_Date FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS Eta_Railramp_Date,
(SELECT TOP 1 Truck_No FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS Truck_No,
Dlv_Area_Nm,Dlv_Addr,WS_CD,(SELECT TOP 1 DLV_AREA_NM FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO ORDER BY ORD_NO DESC) AS DLV_AREA_NM_F,
(SELECT TOP 1 DLV_ADDR FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO ORDER BY ORD_NO DESC) AS DLV_ADDR_F,
(SELECT TOP 1 WS_CD FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO ORDER BY ORD_NO DESC) AS WS_CD_F,
SMSMI.IB_DATE,LSP_CONFIRM_BY,LSP_CONFIRM_DATE,SMSMI.SEC_CMP,
(SELECT TOP 1 CALL_DATE_L FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS CALL_DATE_L,
(SELECT TOP 1 ORDER_DATE_L FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS ORDER_DATE,
(SELECT TOP 1 CONFIRM_DATE_L FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS CONFIRM_DATE,
(SELECT TOP 1 RESERVE_NO FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS RESERVE_NO,
(SELECT TOP 1 ARRIVAL_FACT_DATE_L FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS ARRIVAL_FACT_DATE,
(SELECT TOP 1 IN_DATE_L FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS IN_DATE,
(SELECT TOP 1 POD_MDATE FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO) AS POD_DATE,
(SELECT TOP 1 OUT_DATE_L FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMIRV.STATUS !='V' AND RV_TYPE='I') AS OUT_DATE,
Back_Location,Empty_Time,SMSMI.Goods,SMSMI.Product_Info  
FROM SMIDN left join SMSMI ON SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID ) SMIRV ";
            condition = GetCreateDateCondition("SMIRV", condition);
            return GetBootstrapData(table, condition);
        }

        public ActionResult getMoveInfo()
        {
            string BatNo = Prolink.Math.GetValueAsString(Request.Params["BatNo"]);
            string condition = " BAT_NO=" + SQLUtils.QuotedStr(BatNo);
            return GetBaseData("SMRVM", condition, "*", " SEQ_NO ASC", "2");
        }

        /*取得倉庫月台所有預約狀況*/
        public JsonResult getWhGateReserve()
        {
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string ReserveDate = Prolink.Math.GetValueAsString(Request.Params["ReserveDate"]);
            string sql = "SELECT WS_CD FROM SMWH WHERE GROUP_ID='TPV' AND CMP=" + SQLUtils.QuotedStr(Cmp);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            smwhTable p = new smwhTable();

            List<object> returnData = new List<object>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    sql = "SELECT * FROM SMWHGT WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " ORDER BY GATE_NO ASC";
                    DataTable dt1 = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    whItem g = new whItem();
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow item1 in dt1.Rows)
                        {
                            string GateNo = Prolink.Math.GetValueAsString(item1["GATE_NO"]);
                            string CntrNo = Prolink.Math.GetValueAsString(item1["CNTR_NO"]);
                            string gStatus = Prolink.Math.GetValueAsString(item1["STATUS"]);
                            string ReserveNo = Prolink.Math.GetValueAsString(item1["RESERVE_NO"]);
                            string status = "0";
                            sql = "SELECT COUNT(*) FROM SMRVD WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd) + " AND GATE_NO=" + SQLUtils.QuotedStr(GateNo) + " AND RESERVE_DATE=" + SQLUtils.QuotedStr(ReserveDate);
                            int n = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (CntrNo != "")
                            {
                                status = "1";
                            }
                            else if (n > 0)
                            {
                                status = "2";
                            }
                            else if (gStatus != "Y")
                            {
                                status = "0";
                            }
                            else
                            {
                                status = "3";
                            }

                            g.Gates.Add(new gtItem
                            {
                                GateNo = GateNo,
                                CntrNo = CntrNo,
                                GateStatus = status,
                                ReserveNo = ReserveNo
                            });

                        }
                    }

                    p.wh.Add(new whItem
                    {
                        WsCd = WsCd,
                        Gates = g.Gates
                    });

                }
            }

            return Json(new { returnData = p });
        }
        #endregion

        #region 月台設定
        #region 纯查询
        public ActionResult QueryData()
        {
            string condition = GetCreateDateCondition("SMWH", GetBaseCmp());
            return GetBootstrapData("SMWH", condition);
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMWH WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMWHGT WHERE U_FID={0} ORDER BY GATE_NO ASC", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMWHT WHERE U_FID={0} ORDER BY T_FROM ASC", SQLUtils.QuotedStr(u_id));
            DataTable subDt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmwhModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmwhgtModel");
            data["sub1"] = ModelFactory.ToTableJson(subDt1, "SmwhtModel");
            return ToContent(data);
        }
        #endregion

        #region 保存
        public ActionResult GateSetupUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(UId))
                UId = HttpUtility.UrlDecode(UId);
            if (!string.IsNullOrEmpty(WsCd))
                WsCd = HttpUtility.UrlDecode(WsCd);
            if (!string.IsNullOrEmpty(Cmp))
                Cmp = HttpUtility.UrlDecode(Cmp);
            bool checkDoubleWh = false;

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmwhModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            checkDoubleWh = CheckWs(WsCd, Cmp);
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            string wsCd = ei.Get("WS_CD");
                            string uid = ei.Get("U_ID");
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);

                            if (!string.IsNullOrEmpty(wsCd) && !string.IsNullOrEmpty(uid))
                            {
                                EditInstruct smwhgtEi = new EditInstruct("SMWHGT", EditInstruct.UPDATE_OPERATION);
                                smwhgtEi.PutKey("U_FID", uid);
                                smwhgtEi.Put("WS_CD", wsCd);
                                mixList.Add(smwhgtEi);

                                EditInstruct smwhtEi = new EditInstruct("SMWHT", EditInstruct.UPDATE_OPERATION);
                                smwhtEi.PutKey("U_FID", uid);
                                smwhtEi.Put("WS_CD", wsCd);
                                mixList.Add(smwhtEi);
                            } 
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            EditInstruct ei2 = new EditInstruct("SMWHGT", EditInstruct.DELETE_OPERATION);
                            if (ei.Get("U_ID") == null)
                            {
                                continue;
                            }

                            ei2.PutKey("U_FID", UId);
                            mixList.Add(ei2);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmwhgtModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("CMP", Cmp);
                            ei.Put("WS_CD", WsCd);
                            ei.Put("U_FID", UId);
                        }
                        else
                            ei.AddKey("U_ID");
                        //string test_id = ei.Get("U_ID");
                        System.Guid test_id = System.Guid.NewGuid();
                        if (!System.Guid.TryParse(ei.Get("U_ID"), out test_id))
                            continue;

                        ei.Put("CMP", Cmp);
                        ei.Put("WS_CD", WsCd);
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub1")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmwhtModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("CMP", Cmp);
                            ei.Put("WS_CD", WsCd);
                            ei.Put("CMP", Cmp);
                            ei.Put("U_FID", UId);
                        }
                        else
                            ei.AddKey("U_ID");
                        System.Guid test_id = System.Guid.NewGuid();
                        if (!System.Guid.TryParse(ei.Get("U_ID"), out test_id))
                            continue;

                        ei.Put("CMP", Cmp);
                        ei.Put("WS_CD", WsCd);
                        mixList.Add(ei);
                    }
                }

            }
            if (checkDoubleWh)
            {
                return Json(new { message = "The Warehouse code has been repeated!" });
            }
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            string sql = string.Format("SELECT * FROM SMWH WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMWHGT WHERE U_FID={0} ORDER BY GATE_NO ASC", SQLUtils.QuotedStr(UId));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMWHT WHERE U_FID={0} ORDER BY T_FROM ASC", SQLUtils.QuotedStr(UId));
            DataTable subDt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmwhModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmwhgtModel");
            data["sub1"] = ModelFactory.ToTableJson(subDt1, "SmwhtModel");
            return ToContent(data);
        }
        #endregion

        private bool CheckWs(string WsCd, string Cmp)
        {
            string sql = string.Format("SELECT COUNT(1) FROM SMWH WHERE WS_CD={0} AND CMP={1}",
                    SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(Cmp));
            int counts = getOneValueAsIntFromSql(sql);
            if (counts > 0)
                return true;
            return false;
        }
        #endregion

        #region 叫单车
        public JsonResult OneByOneOrderTruck()
        {
            string returnMessage = "success";
            string OrdNo = Prolink.Math.GetValueAsString(Request.Params["OrdNos"]);
            string UseDatetime = Prolink.Math.GetValueAsString(Request.Params["UseDatetime"]);
            string ArrivalDate = Prolink.Math.GetValueAsString(Request.Params["ArrivalDate"]);

            string sql = "";

            if (OrdNo == "")
            {
                return Json(new { message = "error" });
            }

            sql = @"SELECT * FROM SMORD WHERE ORD_NO IN " + SQLUtils.Quoted(OrdNo.Split(','));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<string> idList = new List<string>();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                    string ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                    string Cstatus = Prolink.Math.GetValueAsString(item["CSTATUS"]);
                    string g = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                    string c = Prolink.Math.GetValueAsString(item["CMP"]);
                    string d = Prolink.Math.GetValueAsString(item["DEP"]);
                    string frtterm = Prolink.Math.GetValueAsString(item["FRT_TERM"]);
                    string exsit = Business.ReserveManage.ChkSmrv(ShipmentId);
                    string trantype= Prolink.Math.GetValueAsString(item["TRAN_TYPE"]);
                    OrdNo = Prolink.Math.GetValueAsString(item["ORD_NO"]);

                    if (Cstatus == "N" || Cstatus == "")
                    {
                        returnMessage = "The Order can not call.";
                        return Json(new { message = returnMessage });
                    }

                    if (Cstatus == "C")
                    {
                        returnMessage = "The Order already called";
                        return Json(new { message = returnMessage });
                    }


                    string result = string.Empty;

                    result = Business.ReserveManage.InboundOrderTrucker(OrdNo, g, c, d, Ext, UserId, UseDatetime, ArrivalDate, idList);

                    if (result == "success")
                    {
                        //更新shipment主档的lot no跟cstatus
                        sql = "UPDATE SMORD SET LOT_NO={0}, CSTATUS='D' WHERE ORD_NO={1}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(OrdNo));

                        try
                        {
                            exeSql(sql);
                        }
                        catch (Exception ex)
                        {

                        }
                            
                        returnMessage += "Shipment ID: " + ShipmentId + "," + result + "\n";
                    }
                    else
                    {
                        return Json(new { message = result });
                    }
                }
            }

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), MailManager.INTERMODAL_CALLCAR);
            return Json(new { message = returnMessage });
        }
        #endregion

        #region 叫多车
        public JsonResult MutilOrderTruck()
        {
            List<string> idList = new List<string>();
            string returnMessage = "success";
            string Uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string WsCd = Prolink.Math.GetValueAsString(Request.Params["WsCd"]);
            string CallType = Prolink.Math.GetValueAsString(Request.Params["CallType"]);
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string UseDatetime = Prolink.Math.GetValueAsString(Request.Params["UseDatetime"]);
            string DlvArea = Prolink.Math.GetValueAsString(Request.Params["DlvArea"]);
            string DlvAddr = Prolink.Math.GetValueAsString(Request.Params["DlvAddr"]);
            string AddrCode = Prolink.Math.GetValueAsString(Request.Params["AddrCode"]);
            int CntNumber = Prolink.Math.GetValueAsInt(Request.Params["CntNumber"]);
            string sql = "";
            string LotNo = string.Empty;

            LotNo = "S" + Business.ReserveManage.getAutoNo("SHIB_NO", GroupId, CompanyId);

            sql = @"SELECT * FROM SMSMI WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(ShipmentId.Split(','));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0) 
            {
                foreach (DataRow item in dt.Rows) 
                {
                    string Cstatus = Prolink.Math.GetValueAsString(item["CSTATUS"]);
                    if (Cstatus == "C")
                    {
                        returnMessage = "此笔shipment已叫车";
                        return Json(new { message = returnMessage });
                    }
                }
            }

            string result = Business.ReserveManage.MutiSmOrderTrucker(dt, UserId, UseDatetime, WsCd, LotNo, DlvArea, CntNumber, DlvAddr, AddrCode, idList);

            if (result == "success")
            {
                sql = "UPDATE SMSMI SET LOT_NO={0}, CSTATUS='C' WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(ShipmentId.Split(','));
                sql = string.Format(sql, SQLUtils.QuotedStr(LotNo));
                try
                {
                    exeSql(sql);
                    returnMessage = "";
                }
                catch (Exception ex)
                {

                }
                returnMessage += "Shipment ID: " + ShipmentId + "," + result + "\n";
            }
            else
            {
                return Json(new { message = result });
            }

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            return Json(new { message = returnMessage });
        }
        #endregion

        #region FCL 叫車
        public ActionResult FclOrderTruck()
        {
            string returnMessage = "";
            string CallData = Request.Params["CallData"];
            string PickupDate = Request.Params["PickupDate"];
            string EtaMsl1 = Request.Params["EtaMsl"];
            string EtaMslTime1 = Request.Params["EtaMslTime"];
            if (!string.IsNullOrEmpty(PickupDate))
            {
                DateTime? s = Business.DateTimeUtils.ParseToDateTimeForNullValue(PickupDate);
                if (s == null)
                {
                    return Json(new { message = "Pickup Date input error" });
                }
            }
            if (!string.IsNullOrEmpty(EtaMsl1))
            {
                DateTime? s = Business.DateTimeUtils.ParseToDateTimeForNullValue(EtaMsl1);
                if (s == null)
                {
                    return Json(new { message = "ETA MSL Date input Error" });
                }
            }

            List<string> EtaMsl = new List<string>();
            EtaMsl.Add(EtaMsl1);
            EtaMsl.Add(EtaMslTime1);
            string QuotNo = Prolink.Math.GetValueAsString(Request.Params["QuotNo"]);
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(CallData);
            string sql = string.Empty;
            if(dict.Count<0)
                return Json(new { message = "No Data" });
            List<string> idList = new List<string>();
            List<string> shipmentList = new List<string>();
            Dictionary<string, bool> shipmentDic = new Dictionary<string, bool>();
            Dictionary<string, string> parm = new Dictionary<string, string>();
            bool isOk = true;
            foreach(var item in dict)
            {
                ArrayList objList = item.Value as ArrayList; 
                foreach (Dictionary<string, object> json in objList)
                {
                    string ShipmentId = json["ShipmentId"].ToString();
                    string OrdNo = json["OrdNo"].ToString();
                    string ArrivalDate = json["ArrivalDate"].ToString();
                    string WsCd = json["WsCd"].ToString();
                    string CntrNo = json["CntrNo"].ToString(); 
                    parm["WsNm"] = json["WsNm"].ToString();
                    parm["AddrCode"] = json["AddrCode"].ToString();
                    parm["DlvArea"] = json["DlvArea"].ToString();
                    parm["DlvAreaNm"] = json["DlvAreaNm"].ToString(); 

                    if (!string.IsNullOrEmpty(ArrivalDate))
                    {
                        DateTime? s = Business.DateTimeUtils.ParseToDateTimeForNullValue(ArrivalDate);
                        if (s == null)
                        {
                            return Json(new { message = "Arrival Date input Error" });
                        }
                    }
                    


                    bool hIBCR = ReserveManage.checkIBCR(shipmentDic, ShipmentId);
                    if (!hIBCR)
                    {
                        returnMessage += "Shipment ID: " + ShipmentId + @Resources.Locale.L_GateManageController_Controllers_146 + "\n";
                        continue;
                    }

                    sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                    DataTable dt = getDataTableFromSql(sql);

                    sql = "SELECT * FROM SMRCNTR WHERE ORD_NO={0} AND CNTR_NO={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(CntrNo));
                    DataTable cdt = getDataTableFromSql(sql);
                    if (cdt.Rows.Count <= 0)
                        continue;
                    string reserveNo = Prolink.Math.GetValueAsString(cdt.Rows[0]["RESERVE_NO"]);
                    if (!string.IsNullOrEmpty(reserveNo))
                    {
                        returnMessage += "Container No: " + CntrNo + ", " + @Resources.Locale.L_GateManageController_Controllers_137 + "\n";
                        isOk = false;
                        continue;
                    }
                    ResultInfo resultinfo = Business.ReserveManage.InboundFclOrderTrucker(dt.Rows[0], cdt.Rows[0], PickupDate, EtaMsl, ArrivalDate, WsCd, UserId, Ext, QuotNo, idList, parm);

                    if (resultinfo.IsSucceed)
                    {
                        returnMessage += "Shipment ID: " + ShipmentId + "," + resultinfo.Description + "\n";
                    }
                    else
                    {
                        returnMessage += "Shipment ID: " + ShipmentId + ", Order truck failure：the reason is：" + resultinfo.Description + "\n";
                        isOk = false;
                    }
                }
            }

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            return Json(new { message = returnMessage, IsOk = isOk ? "Y" : "N" });
        }

        
        #endregion

        #region by DN叫車
        public JsonResult OrderTruckByDn()
        {
            string returnMessage = "success";
            string CallData = Request.Params["CallData"];
            string CarType = Request.Params["CarType"];
            string TrsMode = Request.Params["TrsMode"];
            string PickupDate = Request.Params["PickupDate"];
            string DepAddr = Request.Params["DepAddr"];
            string EtaMsl1 = Request.Params["EtaMsl"];
            string EtaMslTime1 = Request.Params["EtaMslTime"];
            //string DnRemark = Request.Params["DnRemark"]; 
            if (!string.IsNullOrEmpty(PickupDate))
            {
                DateTime? s = Business.DateTimeUtils.ParseToDateTimeForNullValue(PickupDate);
                if (s == null)
                {
                    return Json(new { message = "Pickup Date input error" });
                }
            }
            if (!string.IsNullOrEmpty(EtaMsl1))
            {
                DateTime? s = Business.DateTimeUtils.ParseToDateTimeForNullValue(EtaMsl1);
                if (s == null)
                {
                    return Json(new { message = "ETA MSL Date input Error" });
                }
            }
            List<string> EtaMsl = new List<string>();
            EtaMsl.Add(EtaMsl1);
            EtaMsl.Add(EtaMslTime1);
            string QuotNo = Prolink.Math.GetValueAsString(Request.Params["QuotNo"]);
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(CallData);
            string sql = string.Empty;
            string result = string.Empty;
            List<string> idList = new List<string>();
            Dictionary<string, bool> shipmentDic = new Dictionary<string, bool>();

            TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                IoFlag = "I",
                Dep = Dep,
                Ext = Ext
            };

            foreach (var item in dict)
            {
                ArrayList objList = item.Value as ArrayList;
                foreach (Dictionary<string, object> json in objList)
                {
                    string ArrivalDate = json["ArrivalDate"].ToString();
                    if (!string.IsNullOrEmpty(ArrivalDate))
                    {
                        DateTime? s = Business.DateTimeUtils.ParseToDateTimeForNullValue(ArrivalDate);
                        if (s == null)
                        {
                            return Json(new { message = "Arrival Date input Error" });
                        }
                    }
                }
                    
                result = Business.ReserveManage.InboundDnOrderTrucker(objList, CarType, TrsMode, PickupDate, DepAddr, EtaMsl, userinfo, shipmentDic, QuotNo, idList);
            }

            returnMessage = !string.IsNullOrEmpty(result) ? result : returnMessage;
            if (idList.Count > 0 && string.IsNullOrEmpty(result))
            {
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            }

            
            return Json(new { message = returnMessage });
        }
        #endregion

        #region 修改確認
        public ActionResult ModifyReverseGate()
        { 
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = false, updateData=false;
            //string TempGateno = "";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string TempWscd = "", TempGateno = "", TempRdate = "";
            string WsCd = "", GateNo = "", ReserveDate = "";
            int TempRfrom = 0, TempRh = 0;
            int ReserveFrom = 0, Hour = 0;
            string LotNo = string.Empty;
            if (dt.Rows.Count > 0)
            { 
                foreach(DataRow item in dt.Rows)
                {
                    string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                    TempWscd = Prolink.Math.GetValueAsString(item["TEMP_WSCD"]);
                    TempGateno = Prolink.Math.GetValueAsString(item["TEMP_GATENO"]);
                    TempRdate = ((DateTime)item["TEMP_RDATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    TempRfrom = Prolink.Math.GetValueAsInt(item["TEMP_RFROM"]);
                    TempRh = Prolink.Math.GetValueAsInt(item["TEMP_RH"]);

                    WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                    GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                    ReserveDate = ((DateTime)item["RESERVE_DATE"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    ReserveFrom = Prolink.Math.GetValueAsInt(item["RESERVE_FROM"]);
                    Hour = Prolink.Math.GetValueAsInt(item["RESERVE_HOUR"]);

                    LotNo = Prolink.Math.GetValueAsString(item["LOT_NO"]);

                    updateData = UpdateRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour);

                    if (updateData == true)
                    {
                        chk = InsertRVDRecode(Cmp, TempWscd, TempGateno, TempRdate, TempRfrom, TempRh);
                    }
                    else
                    {
                        chk = false;
                    }
                }
            }

            if (chk == false)
            {
                returnMessage = @Resources.Locale.L_GateManageController_Controllers_145;
            }

            if (chk == true)
            {
                //sql = "UPDATE SMIRV SET STATUS='C', GATE_NO=" + SQLUtils.QuotedStr(TempGateno) + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                MixedList ml = new MixedList();
                EditInstruct ei;
                int[] resulst;
                ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", UId);
                if (!string.IsNullOrEmpty(TempWscd))
                    ei.Put("WS_CD", TempWscd);
                if (!string.IsNullOrEmpty(TempGateno))
                    ei.Put("GATE_NO", TempGateno);
                if (!string.IsNullOrEmpty(TempRdate))
                {
                    ei.PutDate("RESERVE_DATE", TempRdate);
                    ei.Put("RESERVE_FROM", TempRfrom);
                }
                if (0 != TempRh)
                    ei.Put("RESERVE_HOUR", TempRh);
                ei.Put("CONFIRM_BY", UserId);
                DateTime odt = DateTime.Now;               
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                
                ei.PutDate("CONFIRM_DATE", odt);
                ei.PutDate("CONFIRM_DATE_L", ndt);
                ei.Put("STATUS", "C");
                ml.Add(ei);
                try
                {
                    //Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    chk = true;
                    chk = Business.ReserveManage.syncBatNo(UId);

                    #region 計算費用

                    if(dt.Rows.Count > 0)
                    {
                        string ReserveNo = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
                        string ShipmentInfo = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);
                        
                        if (ShipmentInfo != "")
                        {
                            string[] shipment_id = ShipmentInfo.Split(',');
                            for (int i = 0; i < shipment_id.Length; i++)
                            {
                                Bill.WriteLogTagStart("修改预约确认计算拖卡车费用", shipment_id[i]);
                                List<string> emptyMessage = new List<string>();
                                CalculateFee cf = new CalculateFee(shipment_id[i]);
                                cf.FindTrailerQuote(ReserveNo, shipment_id[i], emptyMessage);
                                InboundTransfer.UpdateBillInfoToSMORD(shipment_id[i], "", null);
                                Bill.WriteLogTagStart("结束计算", shipment_id[i]);
                            }
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    chk = false;
                    returnMessage = ex.Message;
                }
            }

            return Json(new { message = returnMessage });
        }
        #endregion

        #region 取消叫車
        public JsonResult CancelOrderTruck()
        {
            string returnMessage = "success";
            string ordnos = Prolink.Math.GetValueAsString(Request.Params["shipments"]);
            string[] ordno = ordnos.Split(',');

            string result = Business.ReserveManage.InboundCancelTruckerByLotNo(ordno, UserId);
            if (result != "success")
            {
                returnMessage = result;
            }
            return Json(new { message = returnMessage });
        }        
        #endregion

        #region Input ETA MSL
        public ActionResult InputEtaMsl()
        {
            string returnMessage = "success";
            string OrdNos = Request.Params["OrdNos"];
            string EM = Request.Params["EM"];
            string EMTime = Request.Params["EMTime"];
            string[] Ord_No = OrdNos.Split(',');
            string sql = "";
            string sql2 = "";
            string sql3 = "";
            for (int i = 0; i < Ord_No.Length; i++)
            {
                string oid = Ord_No[i];
                if (oid != "")
                {
                    string ShipmentId = getOneValueAsStringFromSql("SELECT SHIPMENT_ID FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(oid));
                    string CntrNo = getOneValueAsStringFromSql("SELECT CNTR_NO FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(oid));
                    //string DnNo = getOneValueAsStringFromSql("SELECT DN_NO FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(oid));
                    if (CntrNo != "")
                    {
                        sql2 = string.Format("UPDATE SMICNTR SET ETA_MSL={0},ETA_MSL_TIME={1} WHERE SHIPMENT_ID={2} AND CNTR_NO={3}",
                        SQLUtils.QuotedStr(EM), SQLUtils.QuotedStr(EMTime), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(CntrNo));
                    }

                    sql3 = string.Format("UPDATE SMIDN SET ETA_MSL={0},ETA_MSL_TIME={1} WHERE SHIPMENT_ID={2}",
                    SQLUtils.QuotedStr(EM), SQLUtils.QuotedStr(EMTime), SQLUtils.QuotedStr(ShipmentId));
                    sql = string.Format("UPDATE SMORD SET ETA_MSL={0},ETA_MSL_TIME={1} WHERE ORD_NO={2} ",
                        SQLUtils.QuotedStr(EM), SQLUtils.QuotedStr(EMTime), SQLUtils.QuotedStr(oid));
                }
                try
                {
                    exeSql(sql);
                    exeSql(sql2);
                    exeSql(sql3);
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.ToString() });
                }
            }
            return Json(new { message = returnMessage });
        }
        #endregion

        public ActionResult InputTrucker()
        {
            string returnMessage = "success";
            string OrdNos = Request.Params["OrdNos"];
            string trucker = Request.Params["trucker"];
            string trukcername = Request.Params["truckerNm"];
            MixedList mlist = new MixedList();

            string[] Ord_No = OrdNos.Trim(',').Split(',');
            string sql = string.Format("SELECT TRAN_TYPE,CNTR_NO,SHIPMENT_ID,ORD_NO,TRUCKER1,TRUCKER_NM1 FROM SMORD WHERE ORD_NO IN {0} ", SQLUtils.Quoted(Ord_No));
            DataTable dt = getDataTableFromSql(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string ordno = Prolink.Math.GetValueAsString(dr["ORD_NO"]);
                string trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string oldTrucker = Prolink.Math.GetValueAsString(dr["TRUCKER1"]);
                string oldTruckerNm = Prolink.Math.GetValueAsString(dr["TRUCKER_NM1"]);

                TmexpHandler th1 = new TmexpHandler();
                TmexpInfo tpi1 = new TmexpInfo();
                string expText = ""; 
                if ("F".Equals(trantype) || "R".Equals(trantype))
                {
                    string cntrno = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                    EditInstruct ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("CNTR_NO", cntrno);
                    ei.PutKey("SHIPMENT_ID", shipmentid);
                    ei.Put("TRUCKER1", trucker);
                    ei.Put("TRUCKER_NM1", trukcername);
                    mlist.Add(ei);
                    if (!string.IsNullOrEmpty(cntrno))
                    {
                        expText += "Container No:" + cntrno + ",";
                    }
                }
                EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                smordei.PutKey("ORD_NO", ordno);
                smordei.PutKey("SHIPMENT_ID", shipmentid);
                smordei.Put("TRUCK_CD", trucker);
                smordei.Put("TRUCKER1", trucker);
                smordei.Put("TRUCKER_NM1", trukcername);
                mlist.Add(smordei);
                
                tpi1.UId = Guid.NewGuid().ToString();
                tpi1.UFid = Guid.NewGuid().ToString();
                tpi1.WrId = UserId;
                tpi1.WrDate = DateTime.Now;
                tpi1.Cmp = CompanyId;
                tpi1.GroupId = GroupId;
                tpi1.JobNo = shipmentid;
                tpi1.ExpType = "SM";
                tpi1.ExpReason = "BK";
                tpi1.ExpText = "Change Trucker:"+ expText + "Old Value:Trucker1:" + oldTrucker + ",TruckerNm1:" + oldTruckerNm + ",Update Value:Trucker1:" + trucker + ",TruckerNm1:" + trukcername;
                tpi1.ExpObj = UserId;
                mlist.Add(th1.SetTmexpEi(tpi1));

                string uid = getOneValueAsStringFromSql(string.Format("SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid)));
                SetIBCRToSMSMIPT(mlist, uid, shipmentid, trucker, trukcername);
            }
            if (mlist.Count > 0)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {

                    return Json(new { msg = ex.ToString() });
                }
            }
            return Json(new { message = returnMessage });
        }

        public ActionResult InputUseDateMsl()
        {
            string returnMessage = "success";
            string ReserveNos = Request.Params["ReserveNos"];
            string UseDate = Request.Params["UseDate"];
            string[] ReserveNo = ReserveNos.Split(',');
            MixedList ml=new MixedList();
            for (int i = 0; i < ReserveNo.Length; i++)
            {
                string item = ReserveNo[i];
                if (string.IsNullOrEmpty(item))
                    continue;
                string sql = string.Format("UPDATE SMIRV SET USE_DATE={0} WHERE RESERVE_NO={1}",
                       SQLUtils.QuotedStr(UseDate), SQLUtils.QuotedStr(item));
                ml.Add(sql);
                DateTime pickupDate = Prolink.Math.GetValueAsDateTime(UseDate);
                TrackingEDI.InboundBusiness.InboundHelper.UpdateDetDueDate(item, pickupDate, ml);
            }
            if (ml.Count > 0)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {

                    return Json(new { msg = ex.ToString() });
                }
            }
            return Json(new { message = returnMessage });
        }

        #region 找尋相近位子
        List<GateInfo> gi = new List<GateInfo>();
        class GateInfo
        {
            public string Cmp { get; set; }
            public string WsCd { get; set; }
            public string GateNo { get; set; }
            public string UId { get; set; }
            public int ReserveFrom { get; set; }
            public int Hour { get; set; }
        }

        public string FindTheGate(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour, string LotNo, string UId)
        {
            string msg = "success";
            string sql = string.Empty;
            

            GateInfo gid = new GateInfo();

            gid.Cmp = Cmp;
            gid.WsCd = WsCd;
            gid.GateNo = GateNo;
            gid.ReserveFrom = ReserveFrom;
            gid.Hour = Hour;
            gid.UId = UId;
            gi.Add(gid);

            sql = "SELECT * FROM SMIRV WHERE LOT_NO=" + SQLUtils.QuotedStr(LotNo) + " AND U_ID <>" + SQLUtils.QuotedStr(UId);
            DataTable dt = getDataTableFromSql(sql);

            sql = "SELECT SEQ_NO FROM SMWHGT WHERE CMP={0} AND WS_CD={1} AND GATE_NO={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(GateNo));
            int SeqNo = getOneValueAsIntFromSql(sql);
            int NewSeqNo = 0;
            int NewFrom = 0;

            string chk = string.Empty;
            int k = 0;
            //InsertRVDRecode(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if(k == 0)
                    {
                        NewSeqNo = SeqNo + 1;
                        sql = "SELECT GATE_NO FROM SMWHGT WHERE CMP={0} AND WS_CD={1} AND SEQ_NO={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd), NewSeqNo);
                        string NewGateNo = getOneValueAsStringFromSql(sql);

                        chk = ChkGate(Cmp, WsCd, NewGateNo, ReserveDate, ReserveFrom, Hour);
                    }

                    if(k == 1)
                    {
                        NewSeqNo = SeqNo - 1;
                        sql = "SELECT GATE_NO FROM SMWHGT WHERE CMP={0} AND WS_CD={1} AND SEQ_NO={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd), NewSeqNo);
                        string NewGateNo = getOneValueAsStringFromSql(sql);

                        chk = ChkGate(Cmp, WsCd, NewGateNo, ReserveDate, ReserveFrom, Hour);
                    }

                    if(k == 2)
                    {
                        NewFrom = ReserveFrom + Hour;
                        chk = ChkGate(Cmp, WsCd, GateNo, ReserveDate, NewFrom, Hour);
                    }

                    if(k == 3)
                    {
                        NewFrom = ReserveFrom - Hour;
                        chk = ChkGate(Cmp, WsCd, GateNo, ReserveDate, NewFrom, Hour);
                    }

                    if(chk != "")
                    {
                        return chk;
                    }

                    if(k > 3)
                    {
                        return "最多預約五台車";
                    }
                    k++;
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if(i == 0)
                {
                    NewSeqNo = SeqNo + 1;
                    sql = "SELECT GATE_NO FROM SMWHGT WHERE CMP={0} AND WS_CD={1} AND SEQ_NO={2}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd), NewSeqNo);
                    string NewGateNo = getOneValueAsStringFromSql(sql);

                    gid = new GateInfo();
                    gid.Cmp = Cmp;
                    gid.WsCd = WsCd;
                    gid.GateNo = NewGateNo;
                    gid.ReserveFrom = ReserveFrom;
                    gid.Hour = Hour;
                    gid.UId = dt.Rows[i]["U_ID"].ToString();
                    gi.Add(gid);
                }

                if(i == 1)
                {
                    NewSeqNo = SeqNo - 1;
                    sql = "SELECT GATE_NO FROM SMWHGT WHERE CMP={0} AND WS_CD={1} AND SEQ_NO={2}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd), NewSeqNo);
                    string NewGateNo = getOneValueAsStringFromSql(sql);

                    gid = new GateInfo();
                    gid.Cmp = Cmp;
                    gid.WsCd = WsCd;
                    gid.GateNo = NewGateNo;
                    gid.ReserveFrom = ReserveFrom;
                    gid.Hour = Hour;
                    gid.UId = dt.Rows[i]["U_ID"].ToString();
                    gi.Add(gid);
                }

                if(i == 2)
                {
                    gid = new GateInfo();
                    gid.Cmp = Cmp;
                    gid.WsCd = WsCd;
                    gid.GateNo = GateNo;
                    gid.ReserveFrom = ReserveFrom + Hour;
                    gid.Hour = Hour;
                    gid.UId = dt.Rows[i]["U_ID"].ToString();
                    gi.Add(gid);
                }

                if(i == 3)
                {
                    gid = new GateInfo();
                    gid.Cmp = Cmp;
                    gid.WsCd = WsCd;
                    gid.GateNo = GateNo;
                    gid.ReserveFrom = ReserveFrom - Hour;
                    gid.Hour = Hour;
                    gid.UId = dt.Rows[i]["U_ID"].ToString();
                    gi.Add(gid);
                }
            }

            foreach (GateInfo gateInfo in gi)
            {
                InsertRVDRecode(gateInfo.Cmp, gateInfo.WsCd, gateInfo.GateNo, ReserveDate, gateInfo.ReserveFrom, gateInfo.Hour);
            }

            return msg;
        }

        public string ChkGate(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour)
        {
            bool isok = ChkGateTime(WsCd, Cmp, ReserveFrom);
            if (isok == false)
            {
                return "月台非營業時間無法預約";
            }

            isok = ChkGateEmpty(Cmp, WsCd, GateNo, ReserveDate, ReserveFrom, Hour);

            if (isok == false)
            {
                return "月台已被預約";
            }

            return "";
        }
        #endregion

        #region 檢查月台時間是否為營業時間
        public bool ChkGateTime(string WsCd, string Cmp, int Hour)
        {
            string sql = string.Empty;

            sql = "SELECT D.* FROM SMWH M, SMWHT D WHERE M.U_ID=D.U_FID AND M.WS_CD={0} AND M.CMP={1} ORDER BY SEQ_NO ASC";
            sql = string.Format(sql, SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(Cmp));
            DataTable dt = getDataTableFromSql(sql);

            if(dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int f = Prolink.Math.GetValueAsInt(dr["T_FROM"]);
                    int t = Prolink.Math.GetValueAsInt(dr["T_TO"]);

                    if(Hour >= f && Hour <= t)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region 檢查月台是否可預約
        public bool ChkGateEmpty(string Cmp, string WsCd, string GateNo, string ReserveDate, int ReserveFrom, int Hour)
        {
            string sql = string.Empty;
            sql = "SELECT * FROM SMRVD WHERE CMP={0} AND WS_CD={1} AND GATE_NO={2} AND RESERVE_DATE={3}";
            sql = string.Format(sql, SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(GateNo), SQLUtils.QuotedStr(ReserveDate));
            DataTable dt = getDataTableFromSql(sql);
            int h = 0;

            if(dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    /*從預約的時間往後加，檢查位子是都沒人預約*/
                    for (int i = 1; i <= Hour; i++)
                    {
                        int after = ReserveFrom + 1;
                        h = Prolink.Math.GetValueAsInt(dr["H_" + after.ToString()]);
                        if (h == 1)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        #endregion

        #region 取得貨櫃資訊
        public ActionResult GetContainerByShipment()
        {
            string OrdNo = Prolink.Math.GetValueAsString(Request.Params["OrdNos"]);
            OrdNo = OrdNo.Remove(OrdNo.Length - 1);
            string[] o_id = OrdNo.Split(';');
            return GetBaseData("(SELECT (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMSMI.SHIPMENT_ID=B.SHIPMENT_ID) SMTRAN_TYPE,A.ETA_MSL,A.ETA_MSL_TIME,A.SCMREQUEST_DATE,A.PRIORITY,A.WS_NM,(select TOP 1 BU FROM SMSMI WHERE SMSMI.SHIPMENT_ID=B.SHIPMENT_ID)BU,B.* FROM SMRCNTR B LEFT JOIN SMICNTR A ON  A.SHIPMENT_ID =B.SHIPMENT_ID AND A.CNTR_NO=B.CNTR_NO)SMRCNTR", " ORD_NO IN" + SQLUtils.Quoted(o_id) + " ORDER BY CNTR_NO ASC", "RESERVE_NO,ORD_NO,PICKUP_DATE,WS_CD,SHIPMENT_ID,CNTR_NO,DEC_NO,CNTR_TYPE,DLV_AREA,DLV_AREA_NM,ADDR_CODE,DLV_ADDR,SEAL_NO1,SEAL_NO2,QTY,GW,GWU,CBM,ARRIVAL_DATE,WO,(SELECT TOP 1 PIN_NO FROM SMICNTR WHERE SMICNTR.DN_NO=SMRCNTR.DN_NO AND SMICNTR.SHIPMENT_ID=SMRCNTR.SHIPMENT_ID) AS PIN_NO,ETA_MSL,BU,SCMREQUEST_DATE,ETA_MSL_TIME,PRIORITY,WS_NM,REMARK,SMTRAN_TYPE,DIVISION_DESCP", "", "1");
        }
        #endregion

        #region 取得DN資訊
        public ActionResult GetDnByShipment()
        {
            string OrdNo = Prolink.Math.GetValueAsString(Request.Params["OrdNos"]);
            OrdNo = OrdNo.Trim(';');
            string[] o_id = OrdNo.Split(';');
            //return GetBaseData("SMRDN", " ORD_NO IN " + SQLUtils.Quoted(o_id) + " ORDER BY ADDR_CODE ASC", "U_ID,RESERVE_NO,ORD_NO,PICKUP_DATE,WS_CD,ADD_POINT,SHIPMENT_ID,DN_NO,DEC_NO,DLV_AREA,DLV_AREA_NM,ADDR_CODE,DLV_ADDR,QTY,GW,GWU,CBM,INV_NO, (SELECT TOP 1 SCMREQUEST_DATE FROM SMIDN WHERE SMIDN.DN_NO=SMRDN.DN_NO AND SMIDN.SHIPMENT_ID=SMRDN.SHIPMENT_ID) AS ARRIVAL_DATE", "", "1");
            return GetBaseData("(SELECT A.DIVISION_DESCP,(SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMSMI.SHIPMENT_ID=B.SHIPMENT_ID) SMTRAN_TYPE,A.ETA_MSL,A.ETA_MSL_TIME,A.SCMREQUEST_DATE,A.PRIORITY,A.WS_NM,(select TOP 1 BU FROM SMSMI WHERE SMSMI.SHIPMENT_ID=B.SHIPMENT_ID)BU,B.* FROM SMRDN B LEFT JOIN SMIDN A ON  A.SHIPMENT_ID =B.SHIPMENT_ID AND A.DN_NO=B.DN_NO) SMRDN", " ORD_NO IN " + SQLUtils.Quoted(o_id) + " ORDER BY ADDR_CODE ASC", "U_ID,RESERVE_NO,ORD_NO,PICKUP_DATE,WS_CD,ADD_POINT,SHIPMENT_ID,DN_NO,DEC_NO,DLV_AREA,DLV_AREA_NM,ADDR_CODE,DLV_ADDR,QTY,WO,GW,GWU,CBM,INV_NO,ARRIVAL_DATE,ETA_MSL,BU,SCMREQUEST_DATE,ETA_MSL_TIME,PRIORITY,WS_NM,REMARK,SMTRAN_TYPE,DIVISION_DESCP", "", "1");
        }
        #endregion

        public ActionResult CancelTruckByReserve()
        {
            string returnMessage = "success";
            string reservenos = Request.Params["ReserveNo"];
            reservenos = reservenos.Trim(';');
            string[] reservearray = reservenos.Split(';');

            string sql = string.Format("SELECT * FROM SMIRV WHERE RESERVE_NO IN {0}", SQLUtils.Quoted(reservearray));
            DataTable dt = getDataTableFromSql(sql);
            
            MixedList mixList = new MixedList();
            if(dt.Rows.Count > 0)
            {
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                foreach (DataRow dr in dt.Rows)
                {
                    string msg = CancelTruckItem(dr, mixList);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        return Json(new { msg = msg });
                    }
                }
            }
            return Json(new {msg = returnMessage });
        }

        private string CancelTruckItem(DataRow dr,MixedList mixList)
        {
            string Status = dr["STATUS"].ToString();
            
            if (Status == "I" || Status == "P" || Status == "G" || Status == "O" || Status == "E")
            {
                return dr["SHIPMENT_INFO"].ToString() +":"+ @Resources.Locale.L_DNManage_HasIbCanotCancel;
            }
            string OrdInfo = dr["ORD_INFO"].ToString();
            string ReserveNo = dr["RESERVE_NO"].ToString();
            if (Status == "V")
            {
                return string.Empty;
            }

            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

            string[] OrdList = OrdInfo.Split(',');
            string sql = "UPDATE SMORD SET CSTATUS='Y' WHERE ORD_NO IN " + SQLUtils.Quoted(OrdList);
            mixList.Add(sql);

            string Trucker = Prolink.Math.GetValueAsString(dr["TRUCKER"]);
            string TruckerNm = Prolink.Math.GetValueAsString(dr["TRUCKER_NM"]);
            string shipmentinfo = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
            string DnNo = Prolink.Math.GetValueAsString(dr["DN_NO"]);
            string[] shipments = shipmentinfo.Split(',');
            foreach (string ShipmentId in shipments)
            {
                DataTable mailGroupDt = MailTemplate.GetMailGroup(Trucker, GroupId, "CIC");
                if (mailGroupDt.Rows.Count > 0)
                {
                    foreach (DataRow item1 in mailGroupDt.Rows)
                    {
                        string mailStr = Prolink.Math.GetValueAsString(item1["MAIL_ID"]);
                        if (mailStr != "")
                        {
                            EvenFactory.AddEven(ReserveNo + "#" + ShipmentId + "#" + DnNo + "#" + TruckerNm + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), ReserveNo, "CIRVTK", null, 1, 0, mailStr, @Resources.Locale.L_ReserveManage_Business_95 + ReserveNo, "");
                        }
                    }
                }
            }

            sql = string.Format("SELECT SHIPMENT_ID,COMBIN_SHIPMENT,SHIPMENT_INFO FROM SMSMI WHERE SHIPMENT_ID IN {0} AND ISCOMBINE_CALLTRUCK IS NOT NULL", SQLUtils.Quoted(shipments));
            DataTable smsmidt = getDataTableFromSql(sql);
            List<string> combineshipment = new List<string>();
            foreach (DataRow smsmidr in smsmidt.Rows)
            {
                string shipment = Prolink.Math.GetValueAsString(smsmidr["SHIPMENT_ID"]);

                string combine_shipment = Prolink.Math.GetValueAsString(smsmidr["COMBIN_SHIPMENT"]);
                if (!combineshipment.Contains(combine_shipment))
                {
                    string delsql = "DELETE FROM SMSMI WHERE SHIPMENT_ID = " + SQLUtils.QuotedStr(combine_shipment);
                    mixList.Add(delsql);
                    delsql = "DELETE FROM SMSMIPT WHERE SHIPMENT_ID = " + SQLUtils.QuotedStr(combine_shipment);
                    mixList.Add(delsql);
                    delsql = "DELETE FROM SMBID WHERE CHG_CD='DTRF' AND SHIPMENT_ID = " + SQLUtils.QuotedStr(combine_shipment);
                    mixList.Add(delsql);
                }
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipment);
                ei.Put("SHIPMENT_INFO", null);
                ei.Put("COMBIN_SHIPMENT", null);
                ei.Put("ISCOMBINE_CALLTRUCK", null);
                mixList.Add(ei);
            }

            sql = "UPDATE SMIRV SET STATUS='V', CANCEL_DATE={0}, CANCEL_DATE_L={1} WHERE RESERVE_NO={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ReserveNo));
            mixList.Add(sql);

                    sql = "UPDATE SMRDN SET PICKUP_DATE=NULL,ARRIVAL_DATE=NULL,RESERVE_NO=NULL,LOT_NO=NULL,CANCEL_NO=" + SQLUtils.QuotedStr(ReserveNo) + " WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    mixList.Add(sql);

            sql = "UPDATE SMRCNTR SET PICKUP_DATE=NULL,ARRIVAL_DATE=NULL,RESERVE_NO=NULL,LOT_NO=NULL,CANCEL_NO=" + SQLUtils.QuotedStr(ReserveNo) + " WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
            mixList.Add(sql);

                    sql = "DELETE FROM TMP_AMT WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    mixList.Add(sql);

            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        public ActionResult CancelTruckByOrdNo()
        {
            string ordnos = Prolink.Math.GetValueAsString(Request.Params["OrdNo"]);
            string[] ordnoarray = ordnos.Split(';');
            string conditions = "(";
            for (int i = 0; i < ordnoarray.Length; i++)
            {
                string ordno = ordnoarray[i];
                if (string.IsNullOrEmpty(ordno)) continue;
                if (i > 0)
                {
                    conditions += " OR ";
                }
                conditions += " ORD_INFO LIKE '%" + ordno + "%'";
            }
            conditions += ")";
            string sql = string.Format("SELECT * FROM SMIRV WHERE {0} AND {1}", GetDataPmsCondition("C", true), conditions);
            DataTable dt= getDataTableFromSql(sql);
            MixedList mixList = new MixedList();
            foreach (DataRow dr in dt.Rows)
            {
                string msg = CancelTruckItem(dr, mixList);
                if (!string.IsNullOrEmpty(msg))
                {
                    return Json(new { message = msg });
                }
            }
            return Json(new { message = "success" });
        }


        #region 取得edoc需要的資訊
        public ActionResult GetSmData()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string sql = "SELECT O_UID, O_LOCATION, GROUP_ID,CMP,U_ID FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = getDataTableFromSql(sql);
            string Ouid = string.Empty, OLcation = string.Empty, GroupId = string.Empty;
            string IbCmp = string.Empty;
            if (dt.Rows.Count > 0)
            {
                Ouid = Prolink.Math.GetValueAsString(dt.Rows[0]["O_UID"]);
                OLcation = Prolink.Math.GetValueAsString(dt.Rows[0]["O_LOCATION"]);
                GroupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
                if (string.IsNullOrEmpty(Ouid))
                    Ouid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
                if (string.IsNullOrEmpty(OLcation))
                    OLcation = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                IbCmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            }

            return Json(new {Ouid = Ouid, Cmp = OLcation, GroupId = GroupId, IbCmp= IbCmp });
        }
        #endregion

        #region 重啟訂單
        public ActionResult ReGenOrd()
        {
            string OrdNo = Prolink.Math.GetValueAsString(Request.Params["OrdNo"]);
            string PodCd = Prolink.Math.GetValueAsString(Request.Params["PodCd"]);
            string PodNm = Prolink.Math.GetValueAsString(Request.Params["PodNm"]);
            string AddrCode = Prolink.Math.GetValueAsString(Request.Params["AddrCode"]);
            string Addr = Prolink.Math.GetValueAsString(Request.Params["DlvAddr"]);
            string Trucker1 = Prolink.Math.GetValueAsString(Request.Params["Trucker1"]);
            string TruckerNm1 = Prolink.Math.GetValueAsString(Request.Params["TruckerNm1"]);

            string sql = string.Empty;

            sql = "SELECT GROUP_ID, CMP, SHIPMENT_ID FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
            DataTable dt = getDataTableFromSql(sql);

            try
            {
                if (dt.Rows.Count > 0)
                {
                    string Gid = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
                    string Cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                    string Pol1 = string.Empty;
                    string PolNm1 = string.Empty;
                    string ShipmentId = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);

                    sql = "SELECT DLV_AREA, DLV_AREA_NM FROM SMRCNTR WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                    DataTable sdt = getDataTableFromSql(sql);
                    if(sdt.Rows.Count == 0)
                    {
                        sql = "SELECT DLV_AREA, DLV_AREA_NM FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                        sdt = getDataTableFromSql(sql);
                    }

                    if(sdt.Rows.Count > 0)
                    {
                        Pol1 = Prolink.Math.GetValueAsString(sdt.Rows[0]["DLV_AREA"]);
                        PolNm1 = Prolink.Math.GetValueAsString(sdt.Rows[0]["DLV_AREA_NM"]);
                    }

                    MixedList mixList = new MixedList();

                    string NewOrdNo = Business.ReserveManage.getAutoNo("ORD_NO", Gid, Cmp);
                    #region 複制訂單
                    sql = @"
                    INSERT INTO SMORD 
                    (LIGHT,
                    MONTH,
                    GROUP_ID,
                    CMP,
                    STN,
                    DEP,
                    SHIPMENT_ID,
                    CNTR_NO,
                    TRAN_TYPE,
                    STATUS,
                    DN_ETD,
                    COMBINE_INFO,
                    MARKS,
                    GOODS,
                    LGOODS,
                    BL_RMK,
                    INSTRUCTION,
                    BOOKING_INFO,
                    ABNORMAL_RMK,
                    LSP_ABNORMAL_RMK,
                    PRODUCTION_DATE,
                    PRIORITY,
                    PORT_FREE_TIME,
                    FACT_FREE_TIME,
                    FREIGHT_CUR,
                    FREIGHT_AMT,
                    CUR,
                    GVALUE,
                    CNT20,
                    CNT40,
                    CNT40HQ,
                    CNT_TYPE,
                    CNT_NUMBER,
                    INSURANCE_AMT,
                    PKG_NUM,
                    PKG_UNIT,
                    PKG_UNIT_DESC,
                    GW,
                    GWU,
                    CBM,
                    WEEKLY,
                    YEAR,
                    TRADE_TERM,
                    TRADETERM_DESCP,
                    INCOTERM_CD,
                    INCOTERM_DESCP,
                    LOADING_FROM,
                    LOADING_TO,
                    PICKUP_CDATE,
                    CARRIER,
                    CARRIER_NM,
                    SCAC_CD,
                    FRT_TERM,
                    CON_FREE_TIME,
                    MASTER_NO,
                    HOUSE_NO,
                    SVC_CONTACT,
                    WH_TIME,
                    POR_CD,
                    POR_CNTY,
                    POR_NAME,
                    IB_WINDOW,
                    FACT_TIME,
                    POL_CD,
                    POL_CNTY,
                    POL_NAME,
                    PORT_ETA,
                    EMPTY_TIME,
                    POD_CD,
                    POD_CNTY,
                    POD_NAME,
                    PORT_ATA,
                    BACK_LOCATION,
                    DEST_CD,
                    DEST_CNTY,
                    DEST_NAME,
                    PORT_RLS_DATE,
                    WM_CUT_DATE,
                    VESSEL1,
                    VOYAGE1,
                    ETD1,
                    ETA1,
                    VESSEL2,
                    VOYAGE2,
                    ETD2,
                    ETA2,
                    VESSEL3,
                    VOYAGE3,
                    ETD3,
                    ETA3,
                    VESSEL4,
                    VOYAGE4,
                    ETD4,
                    ETA4,
                    ETD,
                    ETA,
                    ATP,
                    ATD,
                    ATA,
                    OEXPORTER,
                    OEXPORTER_NM,
                    OEXPORTER_ADDR,
                    OIMPORTER,
                    OIMPORTER_NM,
                    OIMPORTER_ADDR,
                    IMPORT_NO,
                    DEC_NO,
                    CER_NO,
                    DEC_DATE,
                    REL_DATE,
                    INSPECTION,
                    DEC_CUST,
                    DEC_REPLY,
                    TC_IMPORTER,
                    TC_IMPORTER_NM,
                    TC_IMPORTER_ADDR,
                    TC_IMPORT_NO,
                    TC_DEC_NO,
                    TC_CER_NO,
                    TC_DEC_DATE,
                    TC_REL_DATE,
                    TC_INSPECTION,
                    TC_DEC_CUST,
                    TC_DEC_REPLY,
                    CREATE_DATE,
                    CREATE_BY,
                    MODIFY_DATE,
                    MODIFY_BY,
                    AIRLINE_CD,
                    AIRLINE_NM,
                    QTY,
                    QTYU,
                    VW,
                    OF_COST,
                    TRUCK_COST,
                    CW,
                    HORN,
                    BATTERY,
                    TRUCK_CD,
                    O_LOCATION,
                    CSTATUS,
                    BSTATUS,
                    DN_NO,
                    LOT_NO,
                    BL_WIN,
                    TERMINAL_CD,
                    TERMINAL_NM,
                    HS_CODE,
                    HS_CODE_NO,
                    TC_HS_CODE,
                    TC_HS_CODE_NO,
                    PRODUCT_TYPE,
                    CONTAINER_YARD_CD,
                    CONTAINER_YARD_NM,
                    EXTRA_SRV,
                    EXTRA_MEMO,
                    FORK,
                    O_UID,
                    O_ORDNO,
                    POL1,
                    POL_NM1,
                    POD1,
                    POD_NM1,
                    TRAN_TYPE1,
                    TRUCKER1,
                    TRUCKER_NM1,
                    ORD_NO,
                    TRAN_NO, ETA_MSL, ARRIVAL_DATE, ETA_MSL_TIME,EMP_PICK_DATE )
                SELECT 
                    LIGHT,
                    MONTH,
                    GROUP_ID,
                    CMP,
                    STN,
                    DEP,
                    SHIPMENT_ID,
                    CNTR_NO,
                    TRAN_TYPE,
                    STATUS,
                    DN_ETD,
                    COMBINE_INFO,
                    MARKS,
                    GOODS,
                    LGOODS,
                    BL_RMK,
                    INSTRUCTION,
                    BOOKING_INFO,
                    ABNORMAL_RMK,
                    LSP_ABNORMAL_RMK,
                    PRODUCTION_DATE,
                    PRIORITY,
                    PORT_FREE_TIME,
                    FACT_FREE_TIME,
                    FREIGHT_CUR,
                    FREIGHT_AMT,
                    CUR,
                    GVALUE,
                    CNT20,
                    CNT40,
                    CNT40HQ,
                    CNT_TYPE,
                    CNT_NUMBER,
                    INSURANCE_AMT,
                    PKG_NUM,
                    PKG_UNIT,
                    PKG_UNIT_DESC,
                    GW,
                    GWU,
                    CBM,
                    WEEKLY,
                    YEAR,
                    TRADE_TERM,
                    TRADETERM_DESCP,
                    INCOTERM_CD,
                    INCOTERM_DESCP,
                    LOADING_FROM,
                    LOADING_TO,
                    PICKUP_CDATE,
                    CARRIER,
                    CARRIER_NM,
                    SCAC_CD,
                    FRT_TERM,
                    CON_FREE_TIME,
                    MASTER_NO,
                    HOUSE_NO,
                    SVC_CONTACT,
                    WH_TIME,
                    POR_CD,
                    POR_CNTY,
                    POR_NAME,
                    IB_WINDOW,
                    FACT_TIME,
                    POL_CD,
                    POL_CNTY,
                    POL_NAME,
                    PORT_ETA,
                    EMPTY_TIME,
                    POD_CD,
                    POD_CNTY,
                    POD_NAME,
                    PORT_ATA,
                    BACK_LOCATION,
                    DEST_CD,
                    DEST_CNTY,
                    DEST_NAME,
                    PORT_RLS_DATE,
                    WM_CUT_DATE,
                    VESSEL1,
                    VOYAGE1,
                    ETD1,
                    ETA1,
                    VESSEL2,
                    VOYAGE2,
                    ETD2,
                    ETA2,
                    VESSEL3,
                    VOYAGE3,
                    ETD3,
                    ETA3,
                    VESSEL4,
                    VOYAGE4,
                    ETD4,
                    ETA4,
                    ETD,
                    ETA,
                    ATP,
                    ATD,
                    ATA,
                    OEXPORTER,
                    OEXPORTER_NM,
                    OEXPORTER_ADDR,
                    OIMPORTER,
                    OIMPORTER_NM,
                    OIMPORTER_ADDR,
                    IMPORT_NO,
                    DEC_NO,
                    CER_NO,
                    DEC_DATE,
                    REL_DATE,
                    INSPECTION,
                    DEC_CUST,
                    DEC_REPLY,
                    TC_IMPORTER,
                    TC_IMPORTER_NM,
                    TC_IMPORTER_ADDR,
                    TC_IMPORT_NO,
                    TC_DEC_NO,
                    TC_CER_NO,
                    TC_DEC_DATE,
                    TC_REL_DATE,
                    TC_INSPECTION,
                    TC_DEC_CUST,
                    TC_DEC_REPLY,
                    {6},
                    {7},
                    MODIFY_DATE,
                    MODIFY_BY,
                    AIRLINE_CD,
                    AIRLINE_NM,
                    QTY,
                    QTYU,
                    VW,
                    OF_COST,
                    TRUCK_COST,
                    CW,
                    HORN,
                    BATTERY,
                    TRUCK_CD,
                    O_LOCATION,
                    'Y',
                    BSTATUS,
                    DN_NO,
                    LOT_NO,
                    BL_WIN,
                    TERMINAL_CD,
                    TERMINAL_NM,
                    HS_CODE,
                    HS_CODE_NO,
                    TC_HS_CODE,
                    TC_HS_CODE_NO,
                    PRODUCT_TYPE,
                    CONTAINER_YARD_CD,
                    CONTAINER_YARD_NM,
                    EXTRA_SRV,
                    EXTRA_MEMO,
                    FORK,
                    O_UID,
                    ORD_NO,
                    {4},
                    {5},
                    {2},
                    {3},
                    'T',
                    {8},
                    {9},
                    {0},
                    1, ETA_MSL, ARRIVAL_DATE, ETA_MSL_TIME,EMP_PICK_DATE FROM SMORD WHERE ORD_NO={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(NewOrdNo), SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(PodNm), SQLUtils.QuotedStr(Pol1), SQLUtils.QuotedStr(PolNm1), SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")), SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(Trucker1), SQLUtils.QuotedStr(TruckerNm1));
                    mixList.Add(sql);
                    #endregion
                    #region 卡車公司回寫SMSMIPT的IBCR
                    sql = @"INSERT INTO SMSMIPT 
                    (U_ID
                    ,U_FID
                    ,SHIPMENT_ID
                    ,PARTY_TYPE
                    ,TYPE_DESCP
                    ,ORDER_BY
                    ,PARTY_NO
                    ,PARTY_NAME
                    ,PARTY_NAME2
                    ,PARTY_NAME3
                    ,PARTY_NAME4
                    ,PARTY_ADDR1
                    ,PARTY_ADDR2
                    ,PARTY_ADDR3
                    ,PARTY_ADDR4
                    ,PARTY_ADDR5
                    ,CNTY
                    ,CNTY_NM
                    ,CITY
                    ,CITY_NM
                    ,STATE
                    ,ZIP
                    ,PARTY_ATTN
                    ,PARTY_TEL
                    ,PARTY_MAIL
                    ,DEBIT_TO
                    ,FAX_NO
                    ,TAX_NO )
                SELECT 
                    {3}
                    ,U_FID
                    ,SHIPMENT_ID
                    ,PARTY_TYPE
                    ,TYPE_DESCP
                    ,ORDER_BY
                    ,{0}
                    ,{1}
                    ,PARTY_NAME2
                    ,PARTY_NAME3
                    ,PARTY_NAME4
                    ,PARTY_ADDR1
                    ,PARTY_ADDR2
                    ,PARTY_ADDR3
                    ,PARTY_ADDR4
                    ,PARTY_ADDR5
                    ,CNTY
                    ,CNTY_NM
                    ,CITY
                    ,CITY_NM
                    ,STATE
                    ,ZIP
                    ,PARTY_ATTN
                    ,PARTY_TEL
                    ,PARTY_MAIL
                    ,DEBIT_TO
                    ,FAX_NO
                    ,TAX_NO FROM SMSMIPT WHERE SHIPMENT_ID={2} AND PARTY_TYPE = 'IBCR'";
                    string ptuid = System.Guid.NewGuid().ToString();
                    sql = string.Format(sql, SQLUtils.QuotedStr(Trucker1), SQLUtils.QuotedStr(TruckerNm1), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ptuid));
                    mixList.Add(sql);
                    #endregion
                    string wscdsql = string.Format("SELECT TOP 1 WS_CD FROM SMWH WHERE DLV_ADDR={0} AND CMP={1} ", SQLUtils.QuotedStr(AddrCode),
                            SQLUtils.QuotedStr(CompanyId));
                    string wscd=getOneValueAsStringFromSql(wscdsql);
                    #region 將dn轉至叫車dn
                    sql = @"INSERT INTO SMRDN (
                            U_ID,
                            GROUP_ID,
                            CMP,
                            STN,
                            DEP,
                            SHIPMENT_ID,
                            DN_NO,
                            INV_NO,
                            NW,
                            GW,
                            GWU,
                            CBM,
                            CBMU,
                            QTY,
                            QTYU,
                            PKG_NUM,
                            PKG_UNIT,
                            PKG_UNIT_DESC,
                            CNT20,
                            CNT40,
                            CNT40HQ,
                            OTH_CNT_TYPE,
                            OTH_CNT_NUM,
                            DLV_AREA,
                            DLV_AREA_NM,
                            PICK_AREA,
                            PICK_AREA_NM,
                            ADDR_CODE,
                            DLV_ADDR,
                            ORD_NO,
                            WS_CD)
                        SELECT 
                            U_ID,
                            GROUP_ID,
                            CMP,
                            STN,
                            DEP,
                            SHIPMENT_ID,
                            DN_NO,
                            INV_NO,
                            NW,
                            GW,
                            GWU,
                            CBM,
                            CBMU,
                            QTY,
                            QTYU,
                            PKG_NUM,
                            PKG_UNIT,
                            PKG_UNIT_DESC,
                            CNT20,
                            CNT40,
                            CNT40HQ,
                            OTH_CNT_TYPE,
                            OTH_CNT_NUM,
                            {4},
                            {5},
                            {0},
                            {1},
                            {6},
                            {7},
                            {2},
                            {8}
                            FROM SMRDN WHERE SHIPMENT_ID={3} AND ORD_NO={9}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(Pol1), SQLUtils.QuotedStr(PolNm1), SQLUtils.QuotedStr(NewOrdNo),
                        SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(PodNm),
                        SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(Addr), SQLUtils.QuotedStr(wscd), SQLUtils.QuotedStr(OrdNo));
                    mixList.Add(sql);
                    #endregion

                     wscdsql= string.Format("SELECT TOP 1 FINAL_WH FROM BSADDR WHERE DLV_ADDR={0} AND CMP={1}  ", SQLUtils.QuotedStr(AddrCode),
                            SQLUtils.QuotedStr(CompanyId));
                     string finalwh = getOneValueAsStringFromSql(wscdsql);

                    #region 將進口貨櫃寫入叫車貨櫃
                    sql = @"INSERT INTO SMRCNTR (
                            U_ID,
                            SHIPMENT_ID,
                            GROUP_ID,
                            STN,
                            DEP,
                            CMP,
                            CNTR_NO,
                            CNTR_TYPE,
                            SEAL_NO1,
                            SEAL_NO2,
                            DN_NO,
                            DIVISION_DESCP,
                            QTY,
                            QTYU,
                            GW,
                            GWU,
                            CBM,
                            DLV_AREA,
                            DLV_AREA_NM,
                            PICK_AREA,
                            PICK_AREA_NM,
                            ADDR_CODE,
                            DLV_ADDR,
                            ORD_NO,
                            WS_CD,FINAL_WH)
                        SELECT 
                            U_ID,
                            SHIPMENT_ID,
                            GROUP_ID,
                            STN,
                            DEP,
                            CMP,
                            CNTR_NO,
                            CNTR_TYPE,
                            SEAL_NO1,
                            SEAL_NO2,
                            DN_NO,
                            DIVISION_DESCP,
                            QTY,
                            QTYU,
                            GW,
                            GWU,
                            CBM,
                            {4},
                            {5},
                            {0},
                            {1},
                            {6},
                            {7},
                            {2},
                            {8},{10}
                            FROM SMRCNTR WHERE SHIPMENT_ID={3} AND ORD_NO={9}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(Pol1), SQLUtils.QuotedStr(PolNm1), SQLUtils.QuotedStr(NewOrdNo), 
                        SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(PodNm),
                        SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(Addr), SQLUtils.QuotedStr(wscd), SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(finalwh));
                    mixList.Add(sql);
                    #endregion

                    #region 修改狀態
                    sql = @"UPDATE SMORD SET CSTATUS='O' WHERE ORD_NO={0}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(OrdNo));
                    mixList.Add(sql);
                    #endregion

                    OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception ex)
            {
                return Json(new {msg = "error", log = ex.ToString()});
            }

            return Json(new { msg = "success" });
        }
        #endregion

        #region Rebuild Order New Function
        public ActionResult ReGenOrdNew()
        {
            string CallData = Request.Params["CallData"];
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(CallData);
            string sql = string.Empty;
            string result = string.Empty;
            List<string> returnMsg = new List<string>();
            MixedList mixList = new MixedList();
            List<string> ordnos = new List<string>();
            foreach (var item in dict)
            {
                ArrayList objList = item.Value as ArrayList;
                foreach (Dictionary<string, object> json in objList)
                {
                    ReBuildOrderIndex(json, mixList, returnMsg, ordnos);
                }
            }
            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { msg = "error", log = ex.ToString() });
            }
            return Json(new { msg = "success", ordnos = string.Join(",", ordnos) });
        }

        private void ReBuildOrderIndex(Dictionary<string, object> json, MixedList mixList, List<string> returnMsg, List<string> ordnos)
        {
             TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = "TPV",
                IoFlag = "I",
                BaseCompanyId = BaseCompanyId
            };
            TrackingEDI.InboundBusiness.SMSMIHelper.ReBuildOrderIndex(json, mixList, returnMsg, ordnos, userinfo);
        }

        public ActionResult AutoCallTruckByFCL()
        {
            string ordno = Prolink.Math.GetValueAsString(Request.Params["OrdNos"]);
            string[] ordnos = ordno.Split(','); 
            string sql = string.Empty;
            string ordnoindex = string.Empty;
            StringBuilder returnMessage = new StringBuilder();
            List<string> idList = new List<string>();
            for (int i = 0; i < ordnos.Length; i++)
            {
                ordnoindex = ordnos[i];
                sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(ordnoindex);
                DataTable dt = getDataTableFromSql(sql);

                sql = "SELECT * FROM SMRCNTR WHERE ORD_NO={0}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ordnoindex));
                DataTable cdt = getDataTableFromSql(sql);

                sql = string.Format(@"SELECT * FROM SMRDN WHERE ORD_NO={0}", SQLUtils.QuotedStr(ordnoindex));
                DataTable smrdnDt = getDataTableFromSql(sql);

                string ShipmentId = dt.Rows[0]["SHIPMENT_ID"].ToString();
                string cntrno = dt.Rows[0]["CNTR_NO"].ToString();
                string DepAddr = dt.Rows[0]["DEP_ADDR"].ToString();
                string delivery = Prolink.Math.GetValueAsString(dt.Rows[0]["POD1"]);
                string tranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
                string CntType = Prolink.Math.GetValueAsString(cdt.Rows[0]["CNTR_TYPE"]);
                string ArrivalDate=string.Empty;
                string CarType = string.Empty;

                string WsCd=cdt.Rows[0]["WS_CD"].ToString();
                List<string> EtaMsl = new List<string>();

                if ("F".Equals(tranType) || "R".Equals(tranType))
                {
                    if ((!string.IsNullOrEmpty(cdt.Rows[0]["ARRIVAL_DATE"].ToString())) && cdt.Rows[0]["ARRIVAL_DATE"] != DBNull.Value)
                    {
                        ArrivalDate = ((DateTime)cdt.Rows[0]["ARRIVAL_DATE"]).ToString("yyyy-MM-dd HH:mm");
                    }
                    ResultInfo resultinfo = Business.ReserveManage.InboundFclOrderTrucker(dt.Rows[0], cdt.Rows[0], ArrivalDate, EtaMsl, ArrivalDate, WsCd, UserId, Ext, "", idList);


                    if (resultinfo == null || resultinfo.IsSucceed)
                    {
                        returnMessage.Append("Container No: " + cntrno + "; Delivery Area" + delivery + "," + resultinfo.Description + "\n");
                        returnMessage.Append("Call Truck Success!");
                    }
                    else
                    {
                        returnMessage.Append("Container No: " + cntrno + " Rebuild Order Success,Call Truck Failed!");
                    }
                }
                else
                {
                    Dictionary<string, bool> shipmentDic = new Dictionary<string, bool>();
                    TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
                    {
                        UserId = UserId,
                        CompanyId = CompanyId,
                        GroupId = GroupId,
                        IoFlag = "I",
                        Dep = Dep,
                        Ext = Ext
                    }; 
                    if (CntType == "20GP")
                    {
                        CarType = "F4";
                    }
                    else if (CntType == "40GP")
                    {
                        CarType = "F5";
                    }
                    else if (CntType == "40HQ")
                    {
                        CarType = "F6";
                    }
                    if (string.IsNullOrEmpty(CarType))
                    {
                        string rnfsql = string.Format("SELECT CHG_CD FROM ECREFFEE WHERE (CMP ={0} OR CMP = '*') AND FEE_TYPE = 'O' AND CHG_CD NOT IN('F1','F2','F3') AND CHG_DESCP={1}",
                             SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(CntType));
                        CarType = OperationUtils.GetValueAsString(rnfsql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    if ((!string.IsNullOrEmpty(smrdnDt.Rows[0]["ARRIVAL_DATE"].ToString())) && smrdnDt.Rows[0]["ARRIVAL_DATE"] != DBNull.Value)
                    {
                        ArrivalDate = ((DateTime)smrdnDt.Rows[0]["ARRIVAL_DATE"]).ToString("yyyy-MM-dd HH:mm");
                    }
                    string result = Business.ReserveManage.RebuildDnOrderTrucker(smrdnDt.Rows[0], CarType, "", ArrivalDate, DepAddr, EtaMsl, userinfo, shipmentDic, "", idList);

                    if (!string.IsNullOrEmpty(result))
                    { 
                        returnMessage.Append("Container No: " + cntrno + " Rebuild Order Success,Call Truck Failed!");
                    }
                    else
                    {
                        returnMessage.Append("Container No: " + cntrno + "; Delivery Area" + delivery + "\n");
                        returnMessage.Append("Call Truck Success!");
                    } 
                } 
            }
            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            if (string.IsNullOrEmpty(returnMessage.ToString()))
                returnMessage.Append("No Data！");
            return Json(new { message = returnMessage.ToString() });
        }

        public void SetIBCRToSMSMIPT(MixedList mlist, string uid, string shipmentid, string truckno, string truckname)
        {
            string sql = string.Format(@"SELECT COUNT(1) FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE = 'IBCR' AND
             PARTY_NO={1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(truckno));
            int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (count > 0) return;
            sql = string.Format(@"SELECT COUNT(1) FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE = 'IBCR'", SQLUtils.QuotedStr(shipmentid));
            count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (count > 0)
            {
                EditInstruct ei = new EditInstruct("SMSMIPT", EditInstruct.DELETE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.PutKey("PARTY_TYPE", "IBCR");
                mlist.Add(ei);
            }
            InboundHandel.SetPartyToSMSMIPT(uid, shipmentid, mlist, truckno, "IBCR");
            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            smsmiei.PutKey("SHIPMENT_ID", shipmentid);
            smsmiei.Put("TRUCK_CD", truckno);
            smsmiei.Put("TRUCKER1", truckno);
            smsmiei.Put("TRUCKER_NM1", truckname);
            smsmiei.Put("IBCR_NO", truckno);
            smsmiei.Put("IBCR_NM", truckname);
            mlist.Add(smsmiei);
        }

        #endregion

        #region Set Arrival Date
        public ActionResult SetArrival()
        {
            string returnMsg = "success";
            DateTime odt = DateTime.Now;
            
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);            
            string reserves = Prolink.Math.GetValueAsString(Request.Params["reserves"]);
            string reserveNo = string.Empty;
            string[] reserveNos = reserves.Split(',');
            for (int i = 0; i < reserveNos.Length; i++)
            {
                reserveNo = reserveNos[i];
                if (string.IsNullOrEmpty(reserveNo))
                    continue;
                string smirvSql = string.Format("SELECT STATUS FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserveNo));
                string status = OperationUtils.GetValueAsString(smirvSql, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> statusList = new List<string> { "I", "V", "O", "Z", "U", "M" };
                if (statusList.Contains(status.ToUpper()))
                {
                    return Json(new { msg = "Appointment No:"+reserveNo+ " Can not proceed Arrival Confirm!" });
                }
            }

            string sql = "UPDATE SMIRV SET ARRIVAL_FACT_DATE={0},ARRIVAL_FACT_DATE_L={1},STATUS='A' WHERE RESERVE_NO IN {2}";
            string str_today = odt.ToString("yyyy-MM-dd HH:mm:ss");
            string str_today1 = ndt.ToString("yyyy-MM-dd HH:mm:ss");
            sql = string.Format(sql, SQLUtils.QuotedStr(str_today), SQLUtils.QuotedStr(str_today1), SQLUtils.Quoted(reserves.Split(',')));

            try
            {
                exeSql(sql);
            }
            catch (Exception ex)
            {
                
                return Json(new { msg = ex.ToString() });
            }

            string sql1 = string.Format(@"SELECT TOP 1 * FROM SMIRV WHERE RESERVE_NO IN {0}", SQLUtils.Quoted(reserves.Split(',')));
            DataTable dt = getDataTableFromSql(sql1);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string ordinfo = Prolink.Math.GetValueAsString(item["ORD_INFO"]);
                    string[] ordnos = ordinfo.Split(',');
                    foreach (string ordno in ordnos)
                    {
                        int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(ordno));
                        int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(ordno));

                        if (r == c)
                        {
                            sql = "UPDATE SMORD SET CSTATUS='A' WHERE ORD_NO={0}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ordno));
                            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }                       
                    }
                }
            }

            return Json(new { msg = returnMsg });
        }
        #endregion

        #region Set Cancel 取消入廠
        public ActionResult SetCancel()
        {
            string returnMsg = "success";
            DateTime today = DateTime.Now;
            MixedList mixList = new MixedList();
            string reserves = Prolink.Math.GetValueAsString(Request.Params["reserves"]);

            string sql = "UPDATE SMIRV SET ARRIVAL_FACT_DATE=NULL, STATUS='C', IN_DATE=NULL WHERE RESERVE_NO IN {0}";
            sql = string.Format(sql, SQLUtils.Quoted(reserves.Split(',')));
            mixList.Add(sql);

            sql = "UPDATE SMRDN SET IDATE=NULL WHERE RESERVE_NO IN {0}";
            sql = string.Format(sql, SQLUtils.Quoted(reserves.Split(',')));
            mixList.Add(sql);

            sql = "UPDATE SMRCNTR SET IDATE=NULL WHERE RESERVE_NO IN {0}";
            sql = string.Format(sql, SQLUtils.Quoted(reserves.Split(',')));
            mixList.Add(sql);

            string smsmisql = string.Format(@"SELECT SHIPMENT_INFO FROM SMIRV WHERE CMP={0}
                AND RESERVE_NO IN {1}", SQLUtils.QuotedStr(CompanyId), SQLUtils.Quoted(reserves.Split(',')));
            DataTable dt = getDataTableFromSql(smsmisql);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string shipmentinfo = dr["SHIPMENT_INFO"].ToString();
                    string[] shipmentid = shipmentinfo.Split(',');
                    foreach (string shipment in shipmentid)
                    {
                        if (string.IsNullOrEmpty(shipment))
                            continue;
                        sql = "UPDATE SMSMI SET STATUS='F' WHERE SHIPMENT_ID={0} ";
                        sql = string.Format(sql, SQLUtils.QuotedStr(shipment));
                        mixList.Add(sql);
                    }
                }
            }
            //sql = "UPDATE SMSMI SET STATUS='F' WHERE RESERVE_NO IN {0}";
            //sql = string.Format(sql, SQLUtils.Quoted(reserves.Split(',')));
            //mixList.Add(sql);
            //F


            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());

                string sql1 = string.Format(@"SELECT TOP 1 * FROM SMIRV WHERE RESERVE_NO IN {0}", SQLUtils.Quoted(reserves.Split(',')));
                DataTable dt1 = getDataTableFromSql(sql1);

                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow item in dt1.Rows)
                    {
                        string ordinfo = Prolink.Math.GetValueAsString(item["ORD_INFO"]);
                        string[] ordnos = ordinfo.Split(',');
                        foreach (string ordno in ordnos)
                        {
                            int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(ordno));
                            int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(ordno));

                            if (r == c)
                            {
                                //更新運輸單的Cstatus
                                sql = "UPDATE SMORD SET CSTATUS='C' WHERE ORD_NO={0}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(ordno));
                                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }                              
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                return Json(new { msg = ex.ToString() });
            }


            return Json(new { msg = returnMsg });
        }
        #endregion

        //批量上传POD
        public ActionResult BatchUploadEdoc()
        {
            var files = Request.Files;
            string smsmis = Prolink.Math.GetValueAsString(Request.Params["uidlist"]);
            string istsa = Prolink.Math.GetValueAsString(Request.Params["istsa"]);
            string[] smsmiarray = smsmis.Trim(',').Split(',');
            string edoctype = Prolink.Math.GetValueAsString(Request.Params["EdocType"]);
            string fileId = string.Empty;
            string jobNo = string.Empty;
            string groupId = GroupId;
            if (smsmiarray.Length > 0)
            {
                jobNo = smsmiarray[0];
            }
            string cmp = CompanyId;
            string stn = "*";
            if (string.IsNullOrEmpty(edoctype))
            {
                return Json(new { message = "fail", IsOk = "N" });
            }
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            int serverNum = 0;
            Dictionary<int, string> dic = new Dictionary<int, string>();
            EDOCResultUploadFile uploadResult = null;
            EDOCFileItem data = null;
            foreach (string uploadId in files)
            {
                HttpPostedFileBase file = files[uploadId];
                dic = new Dictionary<int, string>();
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/FileUploads"), fileName);
                    file.SaveAs(path);
                    foreach (string uid in smsmiarray)
                    {
                        serverNum = 0;
                        string folderID = Business.TPV.Utils.EDocHelper.CheckDuplicatedFolder(_api, uid, groupId, cmp, stn, "", ref serverNum);
                        if (!dic.ContainsKey(serverNum))
                        {
                            uploadResult = _api.UploadFile(folderID, path, "", UserId, "D", "8", serverNum);
                            if (uploadResult != null && uploadResult.Status == DBErrors.DB_SUCCESS)
                            {
                                data = uploadResult.FileInfo;
                                List<UpdateFileItem> fileList = new List<UpdateFileItem>();
                                fileList.Add(new UpdateFileItem
                                {
                                    FileID = data.FileID,
                                    EdocType = edoctype,
                                    Remark = "BATCH UPLOAD" + edoctype
                                });
                                bool isSuccess = _api.UpdateFiles(fileList, serverNum);
                            }
                            dic.Add(serverNum, data.FileID);
                        }
                        else
                            _api.CopyFile(dic[serverNum], folderID, serverNum);
                    }
                }
                else
                {
                    return Json(new { message = "fail", IsOk = "N" });
                }
            }
            //更改状态
            if ("TSA".Equals(istsa) && "POD".Equals(edoctype))
            {
                Business.TPV.Standard.ChageSmRVStatus cs = new Business.TPV.Standard.ChageSmRVStatus();
                cs.UpdateSMRV(istsa, smsmiarray,CompanyId);
            }
            if ("ITMDPOD".Equals(edoctype) && (!("TSA".Equals(istsa))))
            {
                Business.TPV.Standard.ChageSmRVStatus cs = new Business.TPV.Standard.ChageSmRVStatus();
                cs.UpdateSMRV(istsa, smsmiarray, CompanyId);
            }

            //Thread tr = new Thread(() => TrackingEDI.Business.EdocHelper.doPoEdocCopy(smsmis, jobNo, groupId, cmp, stn, fileId, edoctype, "BATCH UPLOAD" + edoctype));

            //try
            //{
            //    tr.Start();
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { message = "fail", IsOk = "N" });
            //}
            return Json(new { message = "success", IsOk = "Y" });
        }

        public ActionResult CreateNewAppointMent()
        {
            string changeData = Request.Params["changedData"];
            string trantype = Request.Params["TranType"];
            string shipmentid = Request.Params["ShipmentId"];
            string reserveno = Request.Params["ReserveNo"];
            string CntrNo = Prolink.Math.GetValueAsString(Request.Params["CntrNo"]);
            if(shipmentid!=null)
                shipmentid = System.Web.HttpUtility.UrlDecode(shipmentid);
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string u_id = string.Empty;
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmirvModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        string invoiceno=ei.Get("InvoiceNo");
                        string Shipper=ei.Get("Shipper");
                        string ShipperNm=ei.Get("ShipperNm");
                        //DLV_AREA,DLV_AREA_NM,DLV_ADDR 
                        string dlv_area=ei.Get("DLV_AREA");
                        string dlv_area_nm=ei.Get("DLV_AREA_NM");
                        string dlv_addr = ei.Get("ADDR_CODE");
                        string ws_cd=ei.Get("WS_CD");
                        string cntr_no=ei.Get("CNTR_NO");
                        string addrname = string.Empty;
                        string pono = ei.Get("PO_NO");
                        //string dnno=ei.Get("DN_NO");

                        ei.OperationType = EditInstruct.INSERT_OPERATION;
                        ei.Put("U_ID", Guid.NewGuid().ToString());
                        ei.Put("GROUP_ID", GroupId);
                        ei.Put("CMP", CompanyId);
                        ei.Put("CREATE_BY", UserId);
                        DateTime odt = DateTime.Now;                       
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        ei.PutDate("CREATE_DATE", odt);
                        ei.PutDate("CREATE_DATE_L", ndt);
                        ei.Put("SHIPMENT_INFO", shipmentid);
                        ei.Put("STATUS","R");
                        ei.Put("RV_TYPE", "I");
                        ei.Put("CNTR_NO", cntr_no);
                        ei.Put("DN_NO", invoiceno);
                        ei.Put("IS_AUTOCREATE", "Y");   //是否是自动创建的Appointment  Y为是
                        if ("F".Equals(trantype) || "R".Equals(trantype))
                        {
                            ei.Put("CALL_TYPE", "C");
                        }
                        else
                        {
                            ei.Put("CALL_TYPE", "D");
                        }

                        //移除没有产生的EI栏位
                        ei.Put("SHIPPER", Shipper + ShipperNm);
                        ei.Put("PO_NO", pono);
                        TrackingEDI.InboundBusiness.SMSMIHelper.setReserveValue(shipmentid, ei);
                        ei.Remove("InvoiceNo");
                        ei.Remove("ShipperNm");
                        mixList.Add(ei);

                        TrackingEDI.InboundBusiness.SMSMIHelper.CreateSubData(trantype, shipmentid, reserveno, mixList, invoiceno, ws_cd, cntr_no, GroupId, CompanyId, ShipperNm, Shipper, UserId);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.ToString(), IsOk = "Y" });
                }
            }
            return Json(new { message = "success", IsOk = "Y" });
        }

        [HttpPost]
        public ActionResult BatchCreateNewAppoint(FormCollection form)
        {
            string returnMessage = string.Empty;
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    mapping = InboundUploadExcelManager.InboundCreateAppointMapping;

                    MixedList ml = new MixedList();
                    MixedList partyml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();

                    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                    parm["mixedlist"] = partyml;
                    List<string> shipmentids = new List<string>();
                    //parm["condition"] = GetDataPmsCondition("C");
                    parm["CompanyId"] = CompanyId;
                    parm["GroupId"] = GroupId;
                    parm["UserId"] = UserId;
                    ExcelParser.RegisterEditInstructFunc(mapping, InboundUploadExcelManager.HandleCreateAppoint);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm, 0);
                    partyml = (MixedList)parm["mixedlist"];
                    for (int i = 0; i < partyml.Count; i++)
                    {
                        ml.Add((EditInstruct)partyml[i]);
                    }

                    if (ml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            if (string.IsNullOrEmpty(returnMessage))
                returnMessage = "success";
            return Json(new { message = returnMessage });
        }

        public void DownLoadXls()
        {
            string strResult = string.Empty;
            string strPath = Server.MapPath("~/download");//D:\U_Disk\V3Tracking\WebGui\Config\excel\AIRBSMapping.xml
            string trantype = Prolink.Math.GetValueAsString(Request.Params["TranType"]);
            string filetype = Prolink.Math.GetValueAsString(Request.Params["FileType"]);
            string strName = "CREATE_APPOINT_V1_20240728.xlsx";
            string strFile = string.Format(@"{0}\{1}", strPath, strName);

            using (FileStream fs = new FileStream(strFile, FileMode.Open))
            {
                byte[] bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(strName, System.Text.Encoding.UTF8));
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
            }
        }

        public ActionResult UpdateTrucker()
        {
            string truckernm = Request.Params["truckerNm"];
            string wscd = Request.Params["WsCd"];
            string dlvarea = Request.Params["DlvArea"];
            string dlvareanm = Request.Params["DlvAreaNm"];
            string addrcode = Request.Params["AddrCode"];
            string reserveno = Request.Params["ReserveNo"];
            string batchid = Request.Params["BatchId"];
            if(batchid!=null)
                batchid = System.Web.HttpUtility.UrlDecode(batchid);

            string pono = Prolink.Math.GetValueAsString(Request.Params["PoNo"]);
            string[] reservenos = reserveno.Split(',');
            MixedList mlist = new MixedList();
            foreach (string value in reservenos)
            {
                if (string.IsNullOrEmpty(value))
                    continue;
                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("RESERVE_NO", value);
                ei.Put("TRUCKER_NM", truckernm);
                ei.Put("WS_CD", wscd);
                ei.Put("DLV_AREA", dlvarea);
                ei.Put("DLV_AREA_NM", dlvareanm);
                ei.Put("ADDR_CODE", addrcode);
                ei.Put("PO_NO", pono);
                if (!string.IsNullOrEmpty(batchid))
                    ei.Put("BATCH_ID", batchid);
                mlist.Add(ei);

                string sql = "SELECT CALL_TYPE FROM SMIRV WHERE RESERVE_NO=" + SQLUtils.QuotedStr(value);
                string calltype = CommonHelp.getOneValueAsStringFromSql(sql);

                sql = "SELECT DLV_ADDR_NM FROM SMWH WHERE WS_CD=" + SQLUtils.QuotedStr(wscd);
                string dlvaddr = CommonHelp.getOneValueAsStringFromSql(sql);

                if (calltype == "C")
                {
                    ei = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("RESERVE_NO", value);
                    ei.Put("WS_CD", wscd);
                    ei.Put("DLV_AREA", dlvarea);
                    ei.Put("DLV_AREA_NM", dlvareanm);
                    ei.Put("ADDR_CODE", addrcode);
                    ei.Put("DLV_ADDR", dlvaddr);
                    mlist.Add(ei);
                }

                if (calltype == "D")
                {
                    ei = new EditInstruct("SMRDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("RESERVE_NO", value);
                    ei.Put("WS_CD", wscd);
                    ei.Put("DLV_AREA", dlvarea);
                    ei.Put("DLV_AREA_NM", dlvareanm);
                    ei.Put("ADDR_CODE", addrcode);
                    ei.Put("DLV_ADDR", dlvaddr);
                    mlist.Add(ei);
                }
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message, message1 = ex.ToString() });
                }
            }
            return Json(new { message = "success", message1 = "success" });
        }
        public ActionResult CheckTrucker()
        {
            string truckernm = Request.Params["truckerNm"];
            string reserveno = Request.Params["ReserveNo"];
            string wsCd = Request.Params["WsCd"];
            string[] reservenos = reserveno.Split(',');

            //IS_AUTOCREATE
            string sql = string.Format("SELECT DISTINCT RESERVE_NO+';' FROM SMIRV WHERE RESERVE_NO IN {0} AND (IS_AUTOCREATE='' OR IS_AUTOCREATE IS NULL) FOR XML PATH('')", SQLUtils.Quoted(reservenos));
            string sqlxml = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT DLV_ADDR,DLV_AREA,DLV_AREA_NM FROM SMWH WHERE WS_CD={0}", SQLUtils.QuotedStr(wsCd));
            DataTable whDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<Dictionary<string, object>> WhData = new List<Dictionary<string, object>>();
            WhData = ModelFactory.ToTableJson(whDt);
            if (string.IsNullOrEmpty(sqlxml))
            {
                return Json(new { IsOk = "Y", message = "success", WHData = WhData });
            }
            sqlxml = sqlxml.Trim(';');
            return Json(new { IsOk = "N", message = sqlxml, WHData = WhData });
        }

        public ActionResult TodayDev()
        {
            string status = "'E','P','C','R','G','U','I','O','A'";
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
            string today = ndt.ToShortDateString();
            string condition = GetWsCdCondition();
            string tablecondi = GetBookingCondition(true);
            tablecondi += " AND RV_TYPE='I' ";
            //DLV_ADDR,ADDR_CODE    根据ADDR CODE 查询
            string table = string.Format(@"(SELECT * FROM SMIRV WHERE {0} AND RESERVE_DATE = {2} AND CALL_TYPE !='S' AND STATUS IN ({1})
                AND EXISTS(SELECT 1 FROM SMRDN WHERE 1=1 AND SMRDN.RESERVE_NO=SMIRV.RESERVE_NO
                    AND EXISTS(SELECT 1 FROM BSADDR WHERE BSADDR.ADDR_CODE=SMRDN.ADDR_CODE AND OUTER_FLAG='N'))", tablecondi, status, SQLUtils.QuotedStr(today));
            table += string.Format(@"UNION SELECT * FROM SMIRV WHERE {0} AND RESERVE_DATE = {2} AND CALL_TYPE !='S' AND STATUS IN ({1})
                AND EXISTS(SELECT 1 FROM SMRCNTR WHERE 1=1 AND SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO
                    AND EXISTS(SELECT 1 FROM BSADDR WHERE BSADDR.ADDR_CODE=SMRCNTR.ADDR_CODE AND OUTER_FLAG='N'))", tablecondi, status, SQLUtils.QuotedStr(today));
            table += string.Format(" UNION SELECT * FROM SMIRV WHERE {0} AND RESERVE_DATE = {1} AND CALL_TYPE !='S' AND STATUS='I' AND TRAN_TYPE IN ('D','E'))T", tablecondi, SQLUtils.QuotedStr(today));
            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            //int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            //ExchangeDT(dt, mode);
            return GetBootstrapData(table, condition);
        }

        public ActionResult TomorrowDev()
        {
            string status = "'E','P','C','R','G','U','I','O','A'";
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
            string tomorrow = ndt.AddDays(1).ToShortDateString();
            string condition = GetWsCdCondition();
            string tablecondi = GetBookingCondition(true);
            tablecondi += " AND RV_TYPE='I' ";
            //DLV_ADDR,ADDR_CODE    根据ADDR CODE 查询
            string table = string.Format(@"(SELECT * FROM SMIRV WHERE {0} AND RESERVE_DATE = {2} AND CALL_TYPE !='S' AND STATUS IN ({1})
                AND EXISTS(SELECT 1 FROM SMRDN WHERE 1=1 AND SMRDN.RESERVE_NO=SMIRV.RESERVE_NO
                    AND EXISTS(SELECT 1 FROM BSADDR WHERE BSADDR.ADDR_CODE=SMRDN.ADDR_CODE AND OUTER_FLAG='N'))", tablecondi, status, SQLUtils.QuotedStr(tomorrow));
            table += string.Format(@"UNION SELECT * FROM SMIRV WHERE {0} AND RESERVE_DATE = {2} AND CALL_TYPE !='S' AND STATUS IN ({1})
                AND EXISTS(SELECT 1 FROM SMRCNTR WHERE 1=1 AND SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO
                    AND EXISTS(SELECT 1 FROM BSADDR WHERE BSADDR.ADDR_CODE=SMRCNTR.ADDR_CODE AND OUTER_FLAG='N'))", tablecondi, status, SQLUtils.QuotedStr(tomorrow));
            table += string.Format(" UNION SELECT * FROM SMIRV WHERE {0} AND RESERVE_DATE = {1} AND CALL_TYPE !='S' AND STATUS='I' AND TRAN_TYPE IN ('D','E'))T", tablecondi, SQLUtils.QuotedStr(tomorrow));
            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            //int mode = Prolink.Math.GetValueAsInt(Request["mode"]);
            //ExchangeDT(dt, mode);
            return GetBootstrapData(table, condition);
        }

        /// <summary>
        /// 获取登录用户visible warehouse
        /// </summary>
        /// <returns></returns>
        public string GetWsCdCondition()
        {
            string condition = "1=0";
            string sql = string.Format("SELECT TOP 1 WHS FROM SYS_ACCT WHERE GROUP_ID={0} AND CMP={1} AND U_ID={2}",
                SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UserId));
            string wsCd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            string[] wsCds = wsCd.Split(new string[] { ";"}, StringSplitOptions.RemoveEmptyEntries);
            if (wsCds.Length > 0)
                condition = string.Format(" WS_CD IN {0}", SQLUtils.Quoted(wsCds));

            return condition;
        }

        public static void UpdateEmptyDate(string reserve_no,string userId, string back_location="", MixedList mixList = null)
        {
            if (string.IsNullOrEmpty(reserve_no))
                return;
            bool update = false;
            if (mixList == null)
            {
                update = true;
                mixList = new MixedList();
            }
            if (back_location == null)
                back_location = string.Empty;
            else if (!string.IsNullOrEmpty(back_location))
                back_location = string.Format(",BACK_LOCATION={0}", SQLUtils.QuotedStr(back_location));
            else
                back_location = string.Format(",BACK_LOCATION=(SELECT TOP 1 BACK_LOCATION FROM SMIRV WHERE RESERVE_NO={0})", SQLUtils.QuotedStr(reserve_no));

            string sql = string.Format("SELECT STATUS,SHIPMENT_INFO FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
            DataTable dt=OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            string shipmentinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);//XMB1706036451,XMB1706035598

            string[] shipments = shipmentinfo.Split(',');
            foreach (string shipment in shipments)
            {
                sql=string.Format(@"SELECT STATUS,SHIPMENT_ID ,TRAN_TYPE,
                (SELECT COUNT(1) FROM SMICNTR WHERE EMPTY_TIME IS NULL AND SMICNTR.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMICNTR.CNTR_NO not in (
				SELECT CNTR_NO FROM SMRCNTR WHERE SMRCNTR.RESERVE_NO={0}))AS EMPTYNULL
                FROM SMSMI WHERE SHIPMENT_ID={1} AND TRAN_TYPE IN ('F','R')", SQLUtils.QuotedStr(reserve_no), SQLUtils.QuotedStr(shipment));
                DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                bool change = true;
                if (smdt.Rows.Count > 0)
                {
                    foreach (DataRow dr in smdt.Rows)
                    {
                        string status1 = Prolink.Math.GetValueAsString(dr["STATUS"]);
                        if (status1 != "X")
                        {
                            EditInstruct ei4 = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                            ei4.PutKey("SHIPMENT_ID", shipment);
                            int count = Prolink.Math.GetValueAsInt(dr["EMPTYNULL"]);
                            string trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                            if ("F".Equals(trantype) || "R".Equals(trantype))
                            {
                                if (count > 0)
                                    change = false;
                            }
                            ei4.Put("STATUS", "Z");
                            if (change)
                            {
                                mixList.Add(ei4);
                            }
                        }
                    }
                }
                EditInstruct AutoCalculTaskEi = new EditInstruct("AUTO_IBCALCUL_TASK", EditInstruct.INSERT_OPERATION);
                AutoCalculTaskEi.Put("U_ID", System.Guid.NewGuid().ToString());
                AutoCalculTaskEi.Put("SHIPMENT_ID", shipment);
                AutoCalculTaskEi.Put("RESERVE_NO", reserve_no);
                AutoCalculTaskEi.Put("DONE", "N");
                AutoCalculTaskEi.Put("CREATE_BY", userId);
                AutoCalculTaskEi.PutDate("CREATE_DATE", DateTime.Now);
                mixList.Add(AutoCalculTaskEi);
            }

            if ("O".Equals(status) || "Z".Equals(status))
            {
                mixList.Add(string.Format("UPDATE SMIRV SET STATUS='Z',EMPTY_TIME=(SELECT TOP 1 EMPTY_TIME FROM SMRCNTR WHERE RESERVE_NO={0}) WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no)));
                mixList.Add(string.Format("UPDATE SMORD SET CSTATUS='Z' WHERE SMORD.ORD_NO IN (SELECT ORD_NO FROM SMIRV WHERE RESERVE_NO={0})", SQLUtils.QuotedStr(reserve_no)));
            }
            else
            {
                mixList.Add(string.Format("UPDATE SMIRV SET EMPTY_TIME=(SELECT TOP 1 EMPTY_TIME FROM SMRCNTR WHERE RESERVE_NO={0}) WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no)));
            }
            mixList.Add(string.Format("UPDATE SMICNTR SET SMICNTR.DISCHARGE_DATE=SMRCNTR.DISCHARGE_DATE,SMICNTR.EMPTY_TIME=SMRCNTR.EMPTY_TIME{1} FROM SMRCNTR WHERE SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no), back_location));
            mixList.Add(string.Format("UPDATE SMIDN SET SMIDN.DISCHARGE_DATE=SMRDN.DISCHARGE_DATE FROM SMRDN WHERE SMRDN.INV_NO=SMIDN.INV_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no)));
            if (update)
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public ActionResult ExportCntrExcel()
        {
            string virtualCol = Request["virtualCol"];
            string filter = "0=1";
            if (virtualCol != null)
            {
                virtualCol = System.Web.HttpUtility.UrlDecode(virtualCol);
                string[] ids = virtualCol.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                if (ids.Length > 0)
                    filter = string.Format("RESERVE_NO IN {0}", SQLUtils.Quoted(ids));
            }
            string sql = string.Format("SELECT * FROM (SELECT * FROM SMRCNTR WHERE {0})A OUTER APPLY (SELECT TOP 1 * FROM SMIRV WITH (NOLOCK) WHERE SMIRV.RESERVE_NO=A.RESERVE_NO)B ORDER BY A.RESERVE_NO", filter);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            dt.Columns.Add("EMPTY_TIME_STR", typeof(string));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr=dt.Rows[i];
                if (dr["EMPTY_TIME"] == null || dr["EMPTY_TIME"] == DBNull.Value)
                    continue;
                dr["EMPTY_TIME_STR"] = ((DateTime)dr["EMPTY_TIME"]).ToString("yyyy-MM-dd HH:mm");
            }
            return ExportExcelFile(dt);
        }

        public ActionResult ExportTruckInfoExcel()
        {
            string virtualCol = Request["virtualCol"];
            string filter = "0=1";
            if (virtualCol != null)
            {
                virtualCol = System.Web.HttpUtility.UrlDecode(virtualCol);
                string[] ids = virtualCol.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                if (ids.Length > 0)
                    filter = string.Format("RESERVE_NO IN {0}", SQLUtils.Quoted(ids));
            }
            string sql = string.Format("SELECT * FROM SMIRV WHERE {0} ORDER BY RESERVE_NO", filter);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string[] columns = { "RESERVE_DATE", "ARRIVAL_DATE", "USE_DATE", "HEAVY_PICKUP_TIME", "EMPTY_RETURN_TIME", "AT_YARD_TIME","ETA_RAILRAMP_DATE" };
            foreach (string column in columns)
            {
                dt.Columns.Add(column+"_STR", typeof(string));
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                foreach (string column in columns)
                {
                    if (dr[column] != null && dr[column] != DBNull.Value)
                    {
                        dr[column + "_STR"] = ((DateTime)dr[column]).ToString("yyyy-MM-dd HH:mm");
                    }
                }
            }
            return ExportExcelFile(dt);
        }

        public ActionResult ImportEmptyDateExcel()
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
            string msg = string.Empty;
            string sql = string.Empty;
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

            #region 生成上传的excel数据
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            string path = Server.MapPath("~/FileUploads/");
            string strExt = System.IO.Path.GetExtension(file.FileName);
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, "EmptyDateExcel");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch { }
            path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMMddHHmmfff") + strExt);
            file.SaveAs(path);
            DataTable dt = ImportExcelToDataTable(path, strExt, StartRow);
            #endregion

            EmptyDateParser parser = new EmptyDateParser();
            return Json(new { message = parser.Save(dt,UserId), excelMsg = msg });
        }

        public ActionResult ImportTruckInfoExcel()
        {
            MixedList ml = new MixedList();
            string returnMessage = "success";
            string msg = string.Empty;
            string sql = string.Empty;
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

            #region 生成上传的excel数据
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            string path = Server.MapPath("~/FileUploads/");
            string strExt = System.IO.Path.GetExtension(file.FileName);
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, "TruckInfoExcel");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch { }
            path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMMddHHmmfff") + strExt);
            file.SaveAs(path);
            DataTable dt = ImportExcelToDataTable(path, strExt, StartRow);
            #endregion

            TruckInfoParser parser = new TruckInfoParser();
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            return Json(new { message = parser.Save(dt, UserId, ndt), excelMsg = msg });
        }

        public ActionResult getSmrTable()
        {
            string OrdNo = Prolink.Math.GetValueAsString(Request.Params["OrdNo"]);
            string OrdNoC = Prolink.Math.GetValueAsString(Request.Params["OrdNoC"]);
            string sql = "SELECT * FROM SMICNTR WHERE 1=0";
            if (!string.IsNullOrEmpty(OrdNoC))
            {
                string[] ordnos = OrdNoC.Trim(',').Split(',');
                sql = string.Format(@"SELECT DISTINCT DLV_AREA,SMRCNTR.CNTR_NO,DLV_AREA_NM,DLV_ADDR,SMORD.ADDR_CODE,U_ID,
                SMORD.FINAL_WH,SMORD.CMP,
                SMORD.WS_CD,SMORD.WS_NM
                FROM SMRCNTR,SMORD WHERE SMRCNTR.ORD_NO=SMORD.ORD_NO AND SMORD.ORD_NO IN {0}", SQLUtils.Quoted(ordnos));   
            }
            DataTable smrcntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "SELECT * FROM SMRDN WHERE 1=0";
            if (!string.IsNullOrEmpty(OrdNo))
            {
                string[] o_id = OrdNo.Trim(',').Split(',');
                sql = string.Format(@"SELECT DISTINCT DLV_AREA,SMRDN.DN_NO,INV_NO,DLV_AREA_NM,DLV_ADDR,SMORD.ADDR_CODE,U_ID,
                    SMORD.FINAL_WH,SMORD.CMP,
                   SMORD. WS_CD,SMORD.WS_NM
                    FROM SMRDN,SMORD WHERE SMRDN.ORD_NO=SMORD.ORD_NO AND SMORD.ORD_NO IN {0}", SQLUtils.Quoted(o_id));
            }
            DataTable smrdn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["smrdn"] = ModelFactory.ToTableJson(smrdn, "SmrdnModel");
            data["smrcntr"] = ModelFactory.ToTableJson(smrcntr, "SmicntrModel");
            return ToContent(data);
        }

        public ActionResult GetRebuildModelInfo()
        {

            string OrdNo = Prolink.Math.GetValueAsString(Request.Params["OrdNos"]);

            OrdNo = OrdNo.Trim(';');
            string[] ordnos = OrdNo.Split(';');
            string sql = string.Format("SELECT DISTINCT PLANT,SHIPMENT_ID FROM SMORD WHERE ORD_NO IN {0}", SQLUtils.Quoted(ordnos));
            DataTable orddt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList mixList=new MixedList();
            if (orddt.Rows.Count > 0)
            {
                foreach (DataRow ord in orddt.Rows)
                {
                    string plant =Prolink.Math.GetValueAsString(ord["PLANT"]);
                    string shipmentId = Prolink.Math.GetValueAsString(ord["SHIPMENT_ID"]);
                    sql = string.Empty;
                    if (string.IsNullOrEmpty(plant))
                        InboundHandel.UpdatePlant(shipmentId, mixList);
                }
            }
            if (mixList.Count > 0)
            {
                try {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                { }
            }
            string table = @"(SELECT (SELECT TOP 1 ARRIVAL_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO=SMORD.ORD_NO ORDER BY ARRIVAL_DATE DESC)AS ARRIVAL_DATE1,
                 (SELECT TOP 1 WS_CD FROM SMRCNTR WHERE SMRCNTR.ORD_NO=SMORD.ORD_NO ) AS TEMP_WS_CD,         
                 SMORD.*, CASE WHEN CUSTERM_CODE IS NULL THEN (SELECT TOP 1 PARTY_NO FROM SMSMIPT WHERE 
                 SMSMIPT.SHIPMENT_ID=SMORD.SHIPMENT_ID AND PARTY_TYPE='IBTW') ELSE CUSTERM_CODE END AS CUSTERM_CODE1,
                 CASE WHEN CUSTERM_CODE IS NULL THEN (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE 
                 SMSMIPT.SHIPMENT_ID=SMORD.SHIPMENT_ID AND PARTY_TYPE='IBTW') ELSE CUSTERM_NAME END AS CUSTERM_NAME1,PLANT AS WS_CD FROM
                 (SELECT SMORD.ORD_NO,SMORD.SHIPMENT_ID,SMORD.CNTR_TYPE,SMORD.CNTR_NO,SMORD.PLANT,SMORD.CMP,
                 (SELECT TOP 1 DLV_AREA FROM SMWH WHERE WS_CD=SMORD.PLANT) AS DEST_CD,
                 (SELECT TOP 1 DLV_AREA_NM FROM SMWH WHERE WS_CD=SMORD.PLANT) AS DEST_NAME,
                 (SELECT TOP 1 DLV_ADDR FROM SMWH WHERE WS_CD=SMORD.PLANT) AS ADDR_CODE,
                 (SELECT TOP 1 DLV_ADDR_NM FROM SMWH WHERE WS_CD=SMORD.PLANT) AS ADDR,
                 (SELECT TOP 1 CUSTOMER_CODE FROM BSADDR WHERE ADDR_CODE=(SELECT TOP 1 DLV_ADDR FROM SMWH WHERE SMWH.WS_CD=SMORD.PLANT)) AS CUSTERM_CODE,
                 (SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO=
                 (SELECT TOP 1 CUSTOMER_CODE FROM BSADDR WHERE ADDR_CODE=(SELECT TOP 1 DLV_ADDR FROM SMWH WHERE SMWH.WS_CD=SMORD.PLANT))) AS CUSTERM_NAME
                 FROM SMORD )SMORD)SMORD ";
            string condition=string.Format("ORD_NO IN {0}", SQLUtils.Quoted(ordnos));
            return GetBootstrapData(table, condition);
        }

        public ActionResult UpdateDlvAddr()
        {
            string changeData = Request.Params["changedData"];
            if (changeData == null)
                return Json(new { message = "No valid Data!" });
            string[] changes = changeData.Split('$');
            MixedList mixList = new MixedList();
            string[] UIds = { }, DlvAreas = { }, DlvAreaNms = { }, DlvAddrs = { }, AddrCodes = { }, WsCds = { }, WsNms = { }, FinalWhs = { };
            UIds = changes[0].Replace("UId", "$").Trim('$').Split('$');
            DlvAreas = changes[1].Replace("DlvArea", "$").Trim('$').Split('$');
            DlvAreaNms = changes[2].Replace("DlvAreaNm", "$").Trim('$').Split('$');
            DlvAddrs = changes[4].Replace("DlvAddr", "$").Trim('$').Split('$');
            AddrCodes = changes[3].Replace("AddrCode", "$").Trim('$').Split('$');
            WsCds = changes[5].Replace("WsCd", "$").Trim('$').Split('$');
            WsNms = changes[6].Replace("WsNm", "$").Trim('$').Split('$');
            FinalWhs = changes[7].Replace("FinalWh", "$").Trim('$').Split('$');

            for (int i = 0; i < UIds.Length; i++)
            {
                string DlvArea = "", DlvAreaNm = "", DlvAddr = "", AddrCode = "", WsCd = "", WsNm = "", FinalWh = "";
                DlvArea = isnull(DlvAreas[i]);
                DlvAreaNm = isnull(DlvAreaNms[i]);
                DlvAddr = isnull(DlvAddrs[i]);
                AddrCode = isnull(AddrCodes[i]);
                WsCd = isnull(WsCds[i]);
                WsNm = isnull(WsNms[i]);
                FinalWh = isnull(FinalWhs[i]);

                if (string.IsNullOrEmpty(DlvArea) || string.IsNullOrEmpty(AddrCode))
                    continue;
                string cntrno = string.Empty, shipmentid = string.Empty, invno = string.Empty;

                TmexpHandler th1 = new TmexpHandler();
                TmexpInfo tpi1 = new TmexpInfo();
                tpi1.UId = Guid.NewGuid().ToString();
                tpi1.UFid = Guid.NewGuid().ToString();
                tpi1.WrId = UserId;
                tpi1.WrDate = DateTime.Now;
                tpi1.Cmp = CompanyId;
                tpi1.GroupId = GroupId;
               
                tpi1.ExpType = "SM";
                tpi1.ExpReason = "BK"; 
                tpi1.ExpObj = UserId;
                 
                string sql = string.Format("SELECT SHIPMENT_ID,CNTR_NO FROM SMRCNTR WHERE U_ID = {0}", SQLUtils.QuotedStr(UIds[i]));
                DataTable smrcntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smrcntr.Rows.Count > 0)
                {
                    shipmentid = Prolink.Math.GetValueAsString(smrcntr.Rows[0]["SHIPMENT_ID"]);
                    cntrno = Prolink.Math.GetValueAsString(smrcntr.Rows[0]["CNTR_NO"]);
                    sql = string.Format("UPDATE SMRCNTR SET DLV_AREA={0},DLV_AREA_NM={1},DLV_ADDR={2},ADDR_CODE={3},WS_CD={5} WHERE U_ID={4}",
                        SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(DlvAreaNm), SQLUtils.QuotedStr(DlvAddr),
                        SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(UIds[i]), SQLUtils.QuotedStr(WsCd));
                    exeSql(sql);
                    sql = string.Format(@"UPDATE SMICNTR SET
                            SMICNTR.DLV_AREA=SMRCNTR.DLV_AREA,
                            SMICNTR.DLV_AREA_NM=SMRCNTR.DLV_AREA_NM,
                            SMICNTR.ADDR_CODE=SMRCNTR.ADDR_CODE,
                            SMICNTR.DLV_ADDR=SMRCNTR.DLV_ADDR,
                            SMICNTR.WS_CD=SMRCNTR.WS_CD,
                            SMICNTR.WS_NM={2},
                            SMICNTR.FINAL_WH={3} 
                            FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID={0} AND SMRCNTR.CNTR_NO={1}",
                            SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntrno), SQLUtils.QuotedStr(WsNm), SQLUtils.QuotedStr(FinalWh));
                    exeSql(sql);

                    sql = string.Format(@"SELECT CNTR_NO,SHIPMENT_ID,ADDR_CODE,WS_CD,WS_NM,FINAL_WH 
                        FROM SMORD WHERE ORD_NO IN(SELECT ORD_NO FROM SMRCNTR WHERE SHIPMENT_ID={0} AND CNTR_NO={1})",
                        SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntrno));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = string.Format(@"UPDATE SMORD SET
                            SMORD.ADDR_CODE=SMRCNTR.ADDR_CODE,
                            SMORD.WS_CD=SMRCNTR.WS_CD,
                            SMORD.WS_NM={2},
                            SMORD.FINAL_WH={3} 
                            FROM SMRCNTR WHERE SMRCNTR.ORD_NO=SMORD.ORD_NO AND SMRCNTR.SHIPMENT_ID={0} AND SMRCNTR.CNTR_NO={1}",
                            SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntrno), SQLUtils.QuotedStr(WsNm), SQLUtils.QuotedStr(FinalWh));
                    exeSql(sql);

                    sql = string.Format(@"UPDATE SMIRV SET ADDR_CODE=SMRCNTR.ADDR_CODE,WS_CD=SMRCNTR.WS_CD,WS_NM={2} FROM 
                                SMRCNTR WHERE SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO AND SMRCNTR.SHIPMENT_ID={0} AND SMRCNTR.CNTR_NO={1}",
                                 SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntrno), SQLUtils.QuotedStr(WsNm));
                    exeSql(sql);
                     
                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        tpi1.JobNo = shipmentid;
                        string expText = "";
                        if (!string.IsNullOrEmpty(dr["CNTR_NO"].ToString()))
                        {
                            expText = "CntrNo:" + dr["CNTR_NO"] + ",";
                        }
                        tpi1.ExpText = "Change Delivery Address:" + expText + "Old Value:AddrCode:" + dr["ADDR_CODE"] + ",WH Code:" + dr["WS_CD"] + ",WH Name:"
                            + dr["WS_NM"] + ",WH Status:" + dr["FINAL_WH"] + ",Update Value:AddrCode:" + AddrCode + ",WH Code:" + WsCd + ",WH Name:"
                            + WsNm + ",WH Status:" + FinalWh;
                        OperationUtils.ExecuteUpdate(th1.SetTmexpEi(tpi1), Prolink.Web.WebContext.GetInstance().GetConnection());
                    } 
                    continue;
                }
                sql = string.Format("SELECT SHIPMENT_ID,DN_NO,INV_NO FROM SMRDN WHERE U_ID = {0}", SQLUtils.QuotedStr(UIds[i]));
                DataTable smrdn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smrdn.Rows.Count > 0)
                {
                    shipmentid = Prolink.Math.GetValueAsString(smrdn.Rows[0]["SHIPMENT_ID"]);
                    invno = Prolink.Math.GetValueAsString(smrdn.Rows[0]["INV_NO"]);
                    sql = string.Format("UPDATE SMRDN SET DLV_AREA={0},DLV_AREA_NM={1},DLV_ADDR={2},ADDR_CODE={3},WS_CD={5} WHERE U_ID={4}",
                        SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(DlvAreaNm), SQLUtils.QuotedStr(DlvAddr),
                        SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(UIds[i]), SQLUtils.QuotedStr(WsCd));
                    exeSql(sql);
                    sql = string.Format(@"UPDATE SMIDN SET
                            SMIDN.DLV_AREA=SMRDN.DLV_AREA,
                            SMIDN.DLV_AREA_NM=SMRDN.DLV_AREA_NM,
                            SMIDN.ADDR_CODE=SMRDN.ADDR_CODE,
                            SMIDN.DLV_ADDR=SMRDN.DLV_ADDR,
                            SMIDN.FINAL_WH={2},
                            SMIDN.WS_CD=SMRDN.WS_CD,
                            SMIDN.WS_NM={3} 
                            FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID={0} AND SMRDN.INV_NO={1}",
                            SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(invno), SQLUtils.QuotedStr(FinalWh), SQLUtils.QuotedStr(WsNm));
                    exeSql(sql);

                    sql = string.Format(@"SELECT CNTR_NO,SHIPMENT_ID,ADDR_CODE,WS_CD,WS_NM,FINAL_WH 
                        FROM SMORD WHERE ORD_NO IN(SELECT ORD_NO FROM SMRDN WHERE SHIPMENT_ID={0} AND INV_NO={1})",
                        SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(invno));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


                    sql = string.Format(@"UPDATE SMORD SET
                            SMORD.ADDR_CODE=SMRDN.ADDR_CODE,
                            SMORD.FINAL_WH={2},
                            SMORD.WS_CD=SMRDN.WS_CD,
                            SMORD.WS_NM={3} 
                            FROM SMRDN WHERE SMRDN.ORD_NO=SMORD.ORD_NO AND SMRDN.SHIPMENT_ID={0} AND SMRDN.INV_NO={1}",
                           SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(invno), SQLUtils.QuotedStr(FinalWh), SQLUtils.QuotedStr(WsNm));
                    exeSql(sql);

                    sql = string.Format(@"UPDATE SMIRV SET ADDR_CODE=SMRDN.ADDR_CODE,WS_CD=SMRDN.WS_CD,WS_NM={2} FROM 
                                SMRDN WHERE SMRDN.RESERVE_NO=SMIRV.RESERVE_NO AND SMRDN.SHIPMENT_ID={0}",
                                 SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntrno), SQLUtils.QuotedStr(WsNm));
                    exeSql(sql);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        tpi1.JobNo = shipmentid;
                        string expText = "";
                        if (!string.IsNullOrEmpty(dr["CNTR_NO"].ToString()))
                        {
                            expText = "CntrNo:" + dr["CNTR_NO"] + ",";
                        }
                        tpi1.ExpText = "Change Delivery Address:" + expText + "Old Value:AddrCode:" + dr["ADDR_CODE"] + ",WH Code:" + dr["WS_CD"] + ",WH Name:"
                            + dr["WS_NM"] + ",WH Status:" + dr["FINAL_WH"] + ",Update Value:AddrCode:" + AddrCode + ",WH Code:" + WsCd + ",WH Name:"
                            + WsNm + ",WH Status:" + FinalWh;
                        OperationUtils.ExecuteUpdate(th1.SetTmexpEi(tpi1), Prolink.Web.WebContext.GetInstance().GetConnection());
                    } 
                }
            }
            return Json(new { message = "success" });
        }
        public string isnull(string strs) { 
            string str="";
            if (!"null".Equals(strs))
                str = strs;
            return str;
        }

        public ActionResult GetSmrcntrInfo()
        {
            string conditions = " RESERVE_NO IS NOT NULL AND ORD_NO IS NOT NULL AND (OLD_RESERVE_NO IS NULL OR OLD_RESERVE_NO='')";
            string basecondtion = GetDecodeBase64ToString(Request.Params["basecondition"]);
            if (!string.IsNullOrEmpty(basecondtion))
            {
                conditions += " AND " + basecondtion;
            }

            string dtconditions = string.Empty;
            if (IOFlag == "O")
            {
                dtconditions = " AND SMIRV.GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND SMIRV.TRUCKER=" + SQLUtils.QuotedStr(CompanyId);
            }
            else
            {
                dtconditions = " AND SMIRV.GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND SMIRV.CMP=" + SQLUtils.QuotedStr(BaseCompanyId);
            }
            StringBuilder table=new StringBuilder();
            table.Append("( SELECT SMRCNTR.* FROM SMRCNTR,SMIRV WHERE SMIRV.RESERVE_NO=SMRCNTR.RESERVE_NO ");
            table.Append(dtconditions);
            table.Append(") SMRCNTR");

            return GetBootstrapData(table.ToString(), conditions);
        }

        public ActionResult SpellCTN()
        {
            string returnMsg = "";
            string reserveno = Request.Params["reserveno"];
            string reservenos = Request.Params["pushdata"];
            string firstdnitems = reserveno;

            string[] reserveArry = reservenos.Split(',');
            TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = "TPV",
                IoFlag = "I",
                BaseCompanyId = BaseCompanyId
            };
            MixedList mlist = TrackingEDI.InboundBusiness.SMSMIHelper.TSAAddContainerInRV(reserveno, reservenos, out reserveno, userinfo);
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = "Transloading has completed";
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg });
                }
            }
            return Json(new { message = returnMsg, CombineInfo = reservenos });
        }

        public ActionResult CombineCTN()
        {
            string returnMsg = "success";
            string reserveno = Request.Params["reserveno"];
            string reservenos = Request.Params["pushdata"];
            string[] reserveArry = reservenos.Split(',');

            MixedList mlist ;
            string combineReserveno;
            TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                IoFlag=IOFlag,
                BaseCompanyId=BaseCompanyId
            };
            string msg = TrackingEDI.InboundBusiness.SMSMIHelper.TSATransloading(reserveno, reserveArry, out mlist, out combineReserveno,userinfo);
            if (!string.IsNullOrEmpty(msg))
                return Json(new { message = msg });
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg });
                }
            }
            return Json(new { message = returnMsg, CombineInfo = combineReserveno });
        }

        public ActionResult TSAMakeAppoint()
        {
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT TOP 1 * FROM SMIRV WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            bool chk = true;
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string LotNo = string.Empty;
            string ReserveNo = string.Empty;
            string ordinfo = string.Empty;
            MixedList ml = new MixedList();
            if (dt.Rows.Count > 0)
            {
                DateTime pickupdate = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["USE_DATE"]);
                string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                string shipmentidinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);
                ReserveNo = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
                string Status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
                ordinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["ORD_INFO"]);
                List<string> list = new List<string>();
                if (!string.IsNullOrEmpty(shipmentid))
                    list.Add(shipmentid);
                string[] shipments = shipmentidinfo.Split(new char[] { ';', ',' });
                foreach (string s in shipments)
                {
                    if (string.IsNullOrEmpty(s))
                        continue;
                    if (list.Contains(s))
                        continue;
                    list.Add(s);
                }
                foreach (string shipmentindex in list)
                {
                    EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", shipmentindex);
                    ei.PutDate("PICKUP_CDATE", pickupdate);
                    ml.Add(ei);

                    EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    smicntrei.PutKey("SHIPMENT_ID", shipmentindex);
                    smicntrei.PutDate("PICKUP_CDATE", pickupdate);
                    ml.Add(smicntrei);

                    EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    smordei.PutKey("SHIPMENT_ID", shipmentindex);
                    smordei.PutDate("PICKUP_CDATE", pickupdate);
                    ml.Add(smordei);
                }
            }
            if (dt.Rows.Count > 0)
            {
                ReserveNo = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
            }

            if (chk == true)
            {
                EditInstruct ei;
                int[] resulst;
                ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", UId);
                ei.Put("CONFIRM_BY", UserId);
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

                ei.PutDate("CONFIRM_DATE", odt);
                ei.PutDate("CONFIRM_DATE_L", ndt);
                ei.Put("STATUS", "R");
                ml.Add(ei);

                try
                {
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string[] ordnos = ordinfo.Split(',');
                    foreach (string ordno in ordnos)
                    {
                        int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(ordno));
                        int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(ordno));
                        if (r == c)
                        {
                            sql = "UPDATE SMORD SET CSTATUS='R' WHERE ORD_NO={0}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ordno));
                            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                    }

                    string sql2 = "SELECT * FROM SMRDN WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    DataTable dt2 = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt2.Rows.Count > 0)
                    {
                        foreach (DataRow item2 in dt2.Rows)
                        {
                            string ShipmentID = Prolink.Math.GetValueAsString(item2["SHIPMENT_ID"]);
                            string DnNo = Prolink.Math.GetValueAsString(item2["DN_NO"]);
                            sql = "UPDATE SMIDN SET CALL_TRUCK_STATUS='C' WHERE SHIPMENT_ID={0} AND DN_NO={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentID), SQLUtils.QuotedStr(DnNo));
                            exeSql(sql);
                        }
                    }

                    sql2 = "SELECT * FROM SMRCNTR WHERE RESERVE_NO=" + SQLUtils.QuotedStr(ReserveNo);
                    dt2 = OperationUtils.GetDataTable(sql2, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt2.Rows.Count > 0)
                    {
                        foreach (DataRow item2 in dt2.Rows)
                        {
                            string ShipmentID = Prolink.Math.GetValueAsString(item2["SHIPMENT_ID"]);
                            string CntrNo = Prolink.Math.GetValueAsString(item2["CNTR_NO"]);
                            sql = "UPDATE SMICNTR SET CALL_TRUCK_STATUS='C' WHERE SHIPMENT_ID={0} AND CNTR_NO={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentID), SQLUtils.QuotedStr(CntrNo));
                            exeSql(sql);
                        }
                    }
                }
                catch (Exception ex)
                {
                    chk = false;
                    returnMessage = ex.Message;
                }
            }

            return Json(new { message = returnMessage });
        }

        
        private void TSADeliverying(string reserveno, out MixedList mlist)
        {
            mlist = new MixedList();
            EditInstruct smrvei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            smrvei.PutKey("RESERVE_NO", reserveno);
            smrvei.Put("CREATE_BY", UserId);
            smrvei.Put("IS_COMBINE_DP", "D");
            smrvei.Put("STATUS", "W");
            smrvei.PutExpress("TRUCK_NO", "CNTR_NO");
            mlist.Add(smrvei);
        }

        public ActionResult DirectDelivery()
        {
            string returnMsg = "success";
            string reserveno = Request.Params["reserveno"];
            string reservenos = Request.Params["pushdata"];
            string[] reserveArry = reservenos.Split(',');

            MixedList mlist;
            MixedList mixedlist = new MixedList(); ;
            string msg = string.Empty;
            foreach (string index in reserveArry)
            {
                if (string.IsNullOrEmpty(index)) continue;
                TSADeliverying(index,  out mlist);
                if (!string.IsNullOrEmpty(msg))
                {
                    return Json(new { message = msg });
                }
                for (int i = 0; i < mlist.Count; i++)
                {
                    mixedlist.Add((EditInstruct)mlist[i]);
                }
            }
            if (mixedlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg });
                }
            }
            return Json(new { message = returnMsg });
        }

        public ActionResult SPellCntrno()
        {
            string reserveno = Request.Params["Reserveno"];
            string removecntr = Request.Params["RemoveCntrno"];
            string[] removeArry = removecntr.Split(',');
            string sql = string.Format("SELECT * FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserveno));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string cntrno = dt.Rows[0]["CNTR_NO"].ToString();
            cntrno = cntrno.Replace(",,",",").Trim(',');
            string[] cntrarray = cntrno.Split(',');
            if (removeArry.Length >= cntrarray.Length)
                return Json(new { message = "You must keep a container of information " });
            MixedList mlist=new MixedList();
            foreach (string cntr in removeArry)
            {
                string delsql = string.Format("DELETE FROM SMRCNTR WHERE RESERVE_NO={0} AND CNTR_NO={1} AND OLD_RESERVE_NO IS NOT NULL",
                    SQLUtils.QuotedStr(reserveno), SQLUtils.QuotedStr(cntr));
                mlist.Add(delsql);
            }
            sql = string.Format(@"SELECT RESERVE_NO,OLD_RESERVE_NO, COUNT(1) as COUNTS FROM SMRCNTR WHERE OLD_RESERVE_NO IN (
                SELECT OLD_RESERVE_NO FROM SMRCNTR WHERE RESERVE_NO={0} AND CNTR_NO IN {1})
                GROUP BY RESERVE_NO,OLD_RESERVE_NO", SQLUtils.QuotedStr(reserveno), SQLUtils.Quoted(removeArry));
            DataTable olddt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow[] drs = olddt.Select("COUNTS='1'");
            if (drs.Length > 0)
            {
                foreach (DataRow dr in drs)
                {
                    string updatesql = string.Format("UPDATE SMIRV SET IS_COMBINE_DP=NULL WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(dr["OLD_RESERVE_NO"].ToString()));
                    mlist.Add(updatesql);
                }
            }
            try
            {
                OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                mlist = new MixedList();
                sql = string.Format("SELECT * FROM SMRCNTR WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserveno));
                DataTable smcntrdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> cntrlist = new List<string>();
                List<string> dnlist = new List<string>();
                List<string> shipmentlist = new List<string>();
                foreach (DataRow smrdr in smcntrdt.Rows)
                {
                    string cntrindex = smrdr["CNTR_NO"].ToString();
                    string dnindex = smrdr["DN_NO"].ToString();
                    string shipmentdex = smrdr["SHIPMENT_ID"].ToString();
                    if (!cntrlist.Contains(cntrindex) && !string.IsNullOrEmpty(cntrindex))
                        cntrlist.Add(cntrindex);
                    if (!dnlist.Contains(dnindex) && !string.IsNullOrEmpty(dnindex))
                        dnlist.Add(dnindex);
                    if (!shipmentlist.Contains(shipmentdex) && !string.IsNullOrEmpty(shipmentdex))
                        shipmentlist.Add(shipmentdex);
                }
                EditInstruct smrvei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                smrvei.PutKey("RESERVE_NO", reserveno);
                smrvei.Put("CNTR_NO", string.Join(",", cntrlist));
                smrvei.Put("DN_NO", string.Join(",", dnlist));
                smrvei.Put("SHIPMENT_INFO", string.Join(",", shipmentlist));
                mlist.Add(smrvei);
                OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                return Json(new { message = "success", IsOk = "Y" });
            }
            catch (Exception ex)
            {
                return Json(new { message = "Operation failed ", IsOk = "N" });
            }
        }

        public ActionResult DownLoadInfoToVizio()
        {
            string transtype = Request.Params["TransType"];
            string reserveno = Request.Params["reserveno"];
            string[] reservenolist = reserveno.Split(';');

            DataTable dt = null;
            string sql = string.Empty;
            DataTable dtAll = null;
            if (reservenolist.Length > 0 && !string.IsNullOrEmpty(reserveno))
            {
                sql = string.Format(@"SELECT WS_CD, PO_NO,CNTR_NO,(SELECT ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMRCNTR.SHIPMENT_ID) AS ETA,
                (SELECT ETD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMRCNTR.SHIPMENT_ID) AS ETD,
                (SELECT CARRIER FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMRCNTR.SHIPMENT_ID) AS CARRIER,
                '' AS REMARK,'' AS LINE_NO,'' AS MODEL,QTY,'' AS ASN_PEG_ID,'' AS EXTERNAL_REF,(
                SELECT TOP 1 OPART_NO FROM SMIDNP WHERE SMIDNP.CNTR_NO=SMRCNTR.CNTR_NO AND SMIDNP.SHIPMENT_ID=SMRCNTR.SHIPMENT_ID AND
                OPART_NO IS NOT NULL
                ) AS OPART_NO
                FROM SMRCNTR WHERE  SMRCNTR.RESERVE_NO IN {0}",
                        SQLUtils.Quoted(reservenolist));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                dt.Columns["MODEL"].MaxLength = 1000;
                dt.Columns["LINE_NO"].MaxLength = 1000;
                dt.Columns["REMARK"].MaxLength = 1000;
                dt.Columns["ASN_PEG_ID"].MaxLength = 1000;
                if (dtAll == null || dtAll.Rows.Count < 0)
                    dtAll = dt.Clone();
                sql = string.Format("SELECT * FROM BSCODE WHERE  CD_TYPE='TCAR' AND CMP IN ('*',{0})", SQLUtils.QuotedStr(CompanyId));
                DataTable tcardt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in dt.Rows)
                {
                    StringBuilder pegid = new StringBuilder();
                    pegid.Append(InboundHandel.ApartFromSATU((DateTime)dr["ETD"]));
                    string wscd = dr["WS_CD"].ToString();
                    string[] wscds = wscd.Split('_');
                    if (wscds.Length >= 2)
                        wscd = wscds[1];
                    if (!string.IsNullOrEmpty(wscd))
                    {
                        pegid.Append("_");
                        pegid.Append(wscd);
                    }
                    pegid.Append(dr["OPART_NO"].ToString());
                    dr["ASN_PEG_ID"] = pegid.ToString();
                    string carrier = dr["CARRIER"].ToString();
                    DataRow[] tcarrows = tcardt.Select(string.Format("CD={0} AND INTTRA IS NOT NULL AND INTTRA <>''", SQLUtils.QuotedStr(carrier)));
                    if (tcarrows.Length > 0)
                        carrier = tcarrows[0]["INTTRA"].ToString();
                    else if (carrier.Length >= 2)
                        carrier = carrier.Substring(0, 2).ToUpper();
                    dr["CARRIER"] = carrier;
                    dtAll.ImportRow(dr);
                }
            }
            VizioDataExcel viziodataexcel = new VizioDataExcel();
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            string xlsFile = viziodataexcel.ResetXls(userinfo, dtAll);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", regex.Match(xlsFile).Value);
        }

        public ActionResult GetSMAndDnData(string id = null, string uid = null)
        {
            string smno = string.Empty;
            string shipmentinfo = Prolink.Math.GetValueAsString(Request["ShipmentInfo"]);
            if (string.IsNullOrEmpty(shipmentinfo))
                return null;
            shipmentinfo = shipmentinfo.Trim(',');
            string[] shipmentids = shipmentinfo.Split(',');
            string sql = string.Format(@"SELECT GROUP_ID,CMP,U_ID,'SMSMI' AS D_TYPE FROM SMSMI WHERE SHIPMENT_ID IN {0}
                UNION SELECT GROUP_ID,O_LOCATION AS CMP,O_UID AS U_ID,'SMSM' AS D_TYPE FROM SMSMI WHERE SHIPMENT_ID IN {0} AND O_UID IS NOT NULL",
                    SQLUtils.Quoted(shipmentids));
            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable combineDt = Dt.Copy();
            foreach (string shipment_id in shipmentids)
            {
                DataTable outdt = ReserveManage.GetEdocData(shipment_id, CompanyId);
                foreach (DataRow dr in outdt.Rows)
                {
                    combineDt.ImportRow(dr);
                }
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sm"] = ModelFactory.ToTableJson(combineDt, "SmdnModel");
            return ToContent(data);
        }

        public ActionResult SetReamrk()
        {
            string result = "success";
            string OrdNo = Prolink.Math.GetValueAsString(Request.Params["OrdNos"]);
            OrdNo = OrdNo.Remove(OrdNo.Length - 1);
            string[] o_id = OrdNo.Split(';');
            string remark = Prolink.Math.GetValueAsString(Request.Params["remark"]);
            //ORD_NO

            MixedList ml = new MixedList();
            string sql = string.Format("UPDATE SMRCNTR SET REMARK={0} WHERE ORD_NO IN {1}", SQLUtils.QuotedStr(remark), SQLUtils.Quoted(o_id));
            ml.Add(sql);
            sql = string.Format("UPDATE SMRDN SET REMARK={0} WHERE ORD_NO IN {1}", SQLUtils.QuotedStr(remark), SQLUtils.Quoted(o_id));
            ml.Add(sql);
            sql = string.Format("UPDATE SMORD SET REMARK={0} WHERE ORD_NO IN {1}", SQLUtils.QuotedStr(remark), SQLUtils.Quoted(o_id));
            ml.Add(sql);
            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                result = ex.Message;
                OperationUtils.Logger.WriteLog(ex);
            }

            return Json(new { message = result });
        }

        public ActionResult DoQueryCntrOrDN()
        {
            string shpFRs = Prolink.Math.GetValueAsString(Request["shpFRlist"]).Trim(',');
            string[] shpFRlist = shpFRs.Split(',');
            string shpOTHs = Prolink.Math.GetValueAsString(Request["shpOTHlist"]).Trim(',');
            string[] shpOTHlist = shpOTHs.Split(',');

            string sql = "";
            if (!string.IsNullOrEmpty(shpFRs))
            {
                sql += string.Format(@"SELECT SMICNTR.U_ID,SMSMI.SHIPMENT_ID,SMICNTR.CNTR_NO,SMICNTR.DN_NO, '' AS INV_NO,SMICNTR.DISCHARGE_DATE,
                SMSMI.TRAN_TYPE FROM SMSMI,SMICNTR WHERE SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMSMI.U_ID IN 
                (SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID IN {0})", SQLUtils.Quoted(shpFRlist));
            }
            if (!string.IsNullOrEmpty(shpOTHs))
            {
                if (!string.IsNullOrEmpty(sql))
                    sql += " UNION ";
                sql += string.Format(@"SELECT SMIDN.U_ID,SMSMI.SHIPMENT_ID,'' AS CNTR_NO,SMIDN.DN_NO,INV_NO,SMIDN.DISCHARGE_DATE,SMSMI.TRAN_TYPE
                FROM SMSMI,SMIDN WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMSMI.U_ID IN (SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID IN {0})",
                SQLUtils.Quoted(shpOTHlist));
            }
            return GetBootstrapData("(" + sql + ") T", "1=1", "", "yyyy-MM-dd");
        }

        public ActionResult UpdateDischargeDate()
        {
            string msg = "success";
            string changeData = Request.Params["changedData"];
            if (changeData == null)
                return Json(new { message = "No valid Data!" });
            changeData = changeData.Trim(',');
            string[] changes = changeData.Split(',');
            MixedList mixList = new MixedList();
            foreach (string change in changes)
            {
                string[] updates = change.Replace("DgDate", ",").Split(',');
                if (updates.Length <= 3) continue;

                string discharge = updates[0];
                string cntrNo = updates[1];
                string dnNo = updates[2];
                string tranType = updates[3];

                if ("F".Equals(tranType) || "R".Equals(tranType))
                {
                    string.Format("UPDATE SMORD SET DISCHARGE_DATE=");
                }
                else
                {

                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception e)
                {
                    return Json(new { message = e.Message });
                }
            }
            return Json(new { message = "success" });
        }

        public ActionResult DoQuerySMORD()
        {
            string shmlist = Prolink.Math.GetValueAsString(Request["shmlist"]).Trim(',');
            string[] shmlistlist = shmlist.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            string table = string.Format(@"(SELECT DISTINCT MASTER_NO, HOUSE_NO, ATA,SHIPMENT_ID FROM SMORD WHERE SHIPMENT_ID IN {0}) T",
                    SQLUtils.Quoted(shmlistlist));
            return GetBootstrapData(table, "1=1", "", "yyyy-MM-dd");
        }

        public ActionResult UpdateAtaInfo(string id = null, string uid = null)
        {
            string changeData = Request.Params["changedData"];
            if (changeData == null)
                return Json(new { message = "No valid Data!" });
            changeData = changeData.Trim(',');
            string[] changes = changeData.Split(',');
            MixedList mixList = new MixedList();
            foreach (string change in changes)
            {
                string sql = string.Empty;
                string[] updates = change.Replace("ATA", ",").Split(',');
                if (updates.Length > 1)
                {
                    string shipmentid = updates[1];
                    string ata = updates[0];
                    SMSMIController.UpdateATA(mixList, ata, shipmentid, UserId);
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                }
            }
            return Json(new { message = "success" });
        }

        public ActionResult UpdateWsRmkInfo()
        {
            string cntrNos = Request.Params["cntrList"];
            string[] cntrlist = cntrNos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string shipids = Request.Params["shipmentIdlist"];
            string[] shiplist = shipids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string wsRmk = Prolink.Math.GetValueAsString(Request.Params["wsRmk"]);

            MixedList mlist = new MixedList();
            string con= "";
            if (!string.IsNullOrEmpty(cntrNos))
            { 
                foreach (var item in cntrlist)
                {
                    string[] strArray = item.Split(new string[] { "CntrNo" }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray.Length > 1) 
                    {
                        con = string.Format(" SET WS_RMK={0} WHERE SHIPMENT_ID={1} AND CNTR_NO ={2}", SQLUtils.QuotedStr(wsRmk), SQLUtils.QuotedStr(strArray[0]), SQLUtils.QuotedStr(strArray[1]));
                        mlist.Add(string.Format("UPDATE SMIRV {0}",con));
                        mlist.Add(string.Format("UPDATE SMORD {0}", con));
                    }
                } 

            }
            if (!string.IsNullOrEmpty(shipids))
            {
                con = string.Format(" SET WS_RMK={0} WHERE SHIPMENT_ID IN{1}", SQLUtils.QuotedStr(wsRmk), SQLUtils.Quoted(shiplist)); 
                mlist.Add(string.Format("UPDATE SMIRV SET WS_RMK={0} WHERE SHIPMENT_INFO IN{1}", SQLUtils.QuotedStr(wsRmk), SQLUtils.Quoted(shiplist)));
                mlist.Add(string.Format("UPDATE SMORD {0}", con));
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message });
                }
            }
            return Json(new { message = "success" });
        }


        public ActionResult CreateEstimation()
        {
            string keys = Prolink.Math.GetValueAsString(Request["keys"]);
            string returnMessage = "success";
            List<string> emptyMessage = new List<string>();
            string[] keyList = keys.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            string newUids = string.Empty;
            ForecastParser forecast = new ForecastParser();
            forecast.CreateForecast(keyList, ref newUids);

            return Json(new { message = returnMessage, empMsg = emptyMessage, uidList = newUids });
        }


        public ActionResult EstimationQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.BidUid = Prolink.Math.GetValueAsString(Request.Params["BidUid"]);
            return View();
        }

        public ActionResult EstimationQueryData()
        { 
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string resultType = Request.Params["resultType"];

            string condition = Request.Params["condition"];

            DataTable dt = ModelFactory.InquiryData("*", "SMORD_FORECAST", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmordForecastModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult UpdateEstimation()
        {
            string keys = Prolink.Math.GetValueAsString(Request["keys"]);
            string pickUpCdate = Prolink.Math.GetValueAsString(Request["pickUpCdate"]);
            string emptyTime = Prolink.Math.GetValueAsString(Request["emptyTime"]);

            string[] newUidList = keys.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            MixedList ml = new MixedList();

            foreach (string newUid in newUidList)
            {
                EditInstruct ei = new EditInstruct("SMORD_FORECAST", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("NEW_UID", newUid);
                if (!string.IsNullOrEmpty(pickUpCdate)) 
                    ei.PutDate("PICKUP_CDATE", Prolink.Math.GetValueAsDateTime(pickUpCdate));
                else
                    ei.PutDate("PICKUP_CDATE", null);
                 
                if (!string.IsNullOrEmpty(emptyTime)) 
                    ei.PutDate("EMPTY_TIME", Prolink.Math.GetValueAsDateTime(emptyTime));
                else
                    ei.PutDate("EMPTY_TIME", null);
                ml.Add(ei);
            }

            try
            { 
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message });
            }

            return Json(new { message = "success" });
        }
    }

    class EmptyDateParser : BaseParser{
        static bool _register = false;
        static object _lock = new object();
        private static string CreateEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            return string.Empty;
        }

        public string Save(DataTable dt,string userId)
        {
            if (!_register)
            {
                lock (_lock)
                {
                    RegisterEditInstructFunc("EmptyDateSMRCNTRMapping", CreateEditInstruct);
                    _register = true;
                }
            }

            List<string> msg = new List<string>();
            List<string> msg1 = new List<string>();
            MixedList ml = new MixedList();
            ParseEditInstruct(dt, "EmptyDateSMRCNTRMapping", ml);
            DataTable portDt = null;

            MixedList list = new MixedList();
            List<string> names = new List<string>() { "RESERVE_NO", "CNTR_NO" };
            for (int i = 0; i < ml.Count; i++)
            {
                EditInstruct ei = (EditInstruct)ml[i];
                string reserve_no = ei.Get("RESERVE_NO");
                if (string.IsNullOrEmpty(ei.Get("CNTR_NO")) || string.IsNullOrEmpty(reserve_no))
                    continue;
                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                ei.AddKey("RESERVE_NO");
                ei.AddKey("CNTR_NO");
                string back_location = ei.Get("BACK_LOCATION");
                string empty_time = ei.Get("EMPTY_TIME");

                #region 过滤多余栏位
                List<string> ei_names = new List<string>() { };
                ei_names.AddRange(ei.getNameSet());
                foreach (var name in ei_names)
                {
                    if (!names.Contains(name))
                        ei.Remove(name);
                }
                #endregion
                if (!string.IsNullOrEmpty(back_location) && !string.IsNullOrEmpty(empty_time))
                {
                    back_location = back_location.ToUpper();
                    if (portDt == null)
                        portDt = OperationUtils.GetDataTable("SELECT PORT_CD FROM BSTPORT", new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (portDt.Select(string.Format("PORT_CD={0}", SQLUtils.QuotedStr(back_location))).Length > 0)
                    {
                        list.Add(string.Format("UPDATE SMIRV SET BACK_LOCATION={0} WHERE RESERVE_NO={1}", SQLUtils.QuotedStr(back_location), SQLUtils.QuotedStr(reserve_no)));
                        list.Add(string.Format("UPDATE SMICNTR SET SMICNTR.BACK_LOCATION={1} FROM SMRCNTR WHERE SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no), SQLUtils.QuotedStr(back_location)));
                    }
                    else
                    {
                        msg1.Add(string.Format("【{0}】 is not in truck port,update fail", back_location));
                    }

                    if (msg1.Count == 0 || msg1 == null)
                    {
                        ei.PutDate("EMPTY_TIME", GetDateTime(empty_time));
                        list.Add(ei);
                        IbGateManageController.UpdateEmptyDate(reserve_no, userId, null, list);
                    }
                }
                else
                    msg1.Add("Empty Return Location or Empty Return Date is null");

            }
            if (msg1.Count>0)
                msg.Add(string.Join(";", msg1));
            try
            {
                if (list.Count > 0 && (msg.Count == 0 || msg == null))
                {
                    OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
                    msg.Add("Empty Return Date update success");
                }
            }
            catch (Exception e)
            {
                msg.Add("update fail:" + e.Message);
            }
            return string.Join("\r\n", msg);
        }

        private static DateTime GetDateTime(string empty_time)
        {
            string[] temps = empty_time.Trim().Split(new string[] { "-", "/", ":", " " }, StringSplitOptions.None);
            empty_time = string.Empty;
            foreach (var v in temps)
            {
                if (v.Length == 0)
                    empty_time += "00";
                else if (v.Length == 1)
                    empty_time += "0";
                empty_time += v;
            }
            empty_time += "00000000000000";
            empty_time = empty_time.Substring(0, 14);
            return Prolink.Utils.FormatUtils.ParseDateTime(empty_time, "YYYYMMDDHHMMSS");
        }
    }

    class TruckInfoParser : BaseParser
    {
        static bool _register = false;
        static object _lock = new object();
        private static string CreateEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            return string.Empty;
        }

        public string Save(DataTable dt,string userId,DateTime ndt)
        {
            if (!_register)
            {
                lock (_lock)
                {
                    RegisterEditInstructFunc("TruckInfoSMRVMapping", CreateEditInstruct);
                    _register = true;
                }
            }

            List<string> msg = new List<string>(); 
            MixedList ml = new MixedList();
            ParseEditInstruct(dt, "TruckInfoSMRVMapping", ml);

            MixedList list = new MixedList();
            List<string> reservenos = new List<string>();

            for (int i = 0; i < ml.Count; i++)
            {
                EditInstruct ei = (EditInstruct)ml[i];
                string reserve_no = ei.Get("RESERVE_NO");
                if (string.IsNullOrEmpty(reserve_no))
                    continue;
                reservenos.Add(reserve_no);
            }
            string sql = string.Format("SELECT U_ID,STATUS,RESERVE_NO,USE_DATE,CMP,TRAN_TYPE,CALL_TYPE FROM SMIRV WHERE RESERVE_NO IN {0}", SQLUtils.Quoted(reservenos.ToArray()));
            DataTable smrvDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string[] columns = {"HEAVY_PICKUP_TIME", "EMPTY_RETURN_TIME", "AT_YARD_TIME","ETA_RAILRAMP_DATE" };
            for (int i = 0; i < ml.Count; i++)
            {
                EditInstruct ei = (EditInstruct)ml[i];
                string reserve_no = ei.Get("RESERVE_NO");
                string autoontheway = ei.Get("HTML_AUTO_ONTHEWAY");
                if (string.IsNullOrEmpty(reserve_no))
                    continue;
                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                ei.AddKey("RESERVE_NO");
                DataRow []drs=smrvDt.Select(string.Format("RESERVE_NO={0}",SQLUtils.QuotedStr(reserve_no)));
                if (drs.Length <= 0)
                    continue;
                DataRow dr = drs[0];
                ei.Remove("HTML_AUTO_ONTHEWAY");

                bool isTsalog=false;
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                bool isTsa=ReserveManage.IsTSALogistic(cmp);
                if (isTsa)
                {
                    isTsalog = "T".Equals(dr["STATUS"]) || "W".Equals(dr["STATUS"]);
                }
                string usedate = ei.Get("USE_DATE");
                if (!string.IsNullOrEmpty(usedate))
                {
                    ei.PutDate("USE_DATE", GetDateTime(usedate));
                }
                else
                {
                    return "update fail:" + reserve_no + " Pickup Date Can't be empty!";
                } 
                DateTime pickupDate = GetDateTime(usedate);                

                if ("D".Equals(dr["STATUS"]) || "R".Equals(dr["STATUS"]) || isTsalog)
                {
                    ei.Put("ORDER_BY", userId);
                    ei.PutDate("ORDER_DATE_L", ndt);
                  
                    if ((dr["USE_DATE"] != null && dr["USE_DATE"] != DBNull.Value) || !string.IsNullOrEmpty(usedate))
                        ei.Put("STATUS", "R"); 
                    foreach (string column in columns){
                        usedate = ei.Get(column);
                        if (!string.IsNullOrEmpty(usedate))
                        {
                            ei.PutDate(column, GetDateTime(usedate));
                        }
                    }

                    if ("Y".Equals(autoontheway) && ("T".Equals(dr["TRAN_TYPE"].ToString())))
                    {
                        string calltype = dr["CALL_TYPE"].ToString();
                        GetSMRVDetail(list,reserve_no, calltype);
                        ei.Put("STATUS", "O");
                    }                   
                }               
                list.Add(ei);
                TrackingEDI.InboundBusiness.InboundHelper.UpdateDetDueDate(reserve_no, pickupDate, list);
            }
            
            try
            {
                if (list.Count > 0)
                    OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
                msg.Add("Truck Info update success");
            }
            catch (Exception e)
            {
                msg.Add("update fail:" + e.Message);
            }
            return string.Join("\r\n", msg);
        }

        private static DateTime GetDateTime(string empty_time)
        {
            string[] temps = empty_time.Trim().Split(new string[] { "-", "/", ":", " " }, StringSplitOptions.None);
            empty_time = string.Empty;
            foreach (var v in temps)
            {
                if (v.Length == 0)
                    empty_time += "00";
                else if (v.Length == 1)
                    empty_time += "0";
                empty_time += v;
            }
            empty_time += "00000000000000";
            empty_time = empty_time.Substring(0, 14);
            return Prolink.Utils.FormatUtils.ParseDateTime(empty_time, "YYYYMMDDHHMMSS");
        }

        public static void GetSMRVDetail(MixedList ml, string reserve_no, string call_type)
        {
            string gateOutSQL = string.Format("SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD FROM (SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,CMP FROM SMRCNTR WITH (NOLOCK) WHERE SMRCNTR.RESERVE_NO={1})A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B",  SQLUtils.QuotedStr(reserve_no));
            if ("D".Equals(call_type))
                gateOutSQL = string.Format("SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD FROM (SELECT ORD_NO,IDATE,'' AS CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,CMP FROM SMRDN WITH (NOLOCK) WHERE SMRDN.RESERVE_NO={1})A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B",  SQLUtils.QuotedStr(reserve_no));

            DataTable dt = OperationUtils.GetDataTable(gateOutSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count == 0)
                return;
            List<string> ordList = new List<string>();
            foreach (DataRow ord in dt.Rows)
            {
                string ord_no = Prolink.Math.GetValueAsString(ord["ORD_NO"]);
                if (ordList.Contains(ord_no))
                    continue;
                EditInstruct ordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ordei.PutKey("ORD_NO", ord_no);
                 if ("Temp".Equals(Prolink.Math.GetValueAsString(ord["FINAL_WH"])) && "Y".Equals(Prolink.Math.GetValueAsString(ord["OUTER_FLAG"])))
                {
                    ordei.Put("CSTATUS", "T");
                    ordList.Add(ord_no);
                    ml.Add(ordei);
                }
            }
        } 
    }

    
}
