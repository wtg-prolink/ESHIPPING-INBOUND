using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;
using WebGui.Models;
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json;
using Prolink.Model;
using System.Text;
using System.Collections.Specialized;
using TrackingEDI.Business;
using EDOCApi;
using Business.TPV.Financial;
using Business;

namespace WebGui.Controllers
{
    public class IQTManageController : BaseController
    {
        #region View
        public ActionResult QuotLocalQuery()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult QuotLocalSetup()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            ViewBag.ioflag = ioflag;
            string isa = Prolink.Math.GetValueAsString(Request["isa"]);
            ViewBag.isa = isa;
            if (isa.Equals("Y"))
            {
                ViewBag.pmsList = GetBtnPms("IQT021");
            }
            else
            {
                ViewBag.pmsList = GetBtnPms("IQTX01");
            }
            ViewBag.Uid = Request["UId"];
            SetOpitonForSelect(new string[] { "RN_F" }, "  ORDER BY Min(ORDER_BY) DESC");
            SetSchema("SMQTM");
            return View();
        }

        /// <summary>
        /// 拖车报价
        /// </summary>
        /// <returns></returns>
        public ActionResult QuotTrailerQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("IQTC01");
            string sql = "SELECT CMP, NAME FROM SYS_SITE WHERE TYPE=1";
            DataTable dt = getDataTableFromSql(sql);
            string str = string.Empty;
            string Cmp = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                Cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string Name = Prolink.Math.GetValueAsString(dr["NAME"]);
                if (IOFlag != "O" && CompanyId==Cmp)
                {
                    str += "<option value='" + Cmp + "' SELECTED>" + Cmp + ":" + Name + "</option>";
                }
                else
                {
                    str += "<option value='" + Cmp + "'>" + Cmp + ":" + Name + "</option>";
                }
            }

            ViewBag.LocationSel = str;
            ViewBag.Cmp = CompanyId;
            return View();
        }

        /// <summary>
        /// 报关报价
        /// </summary>
        /// <returns></returns>
        public ActionResult QuotBrokerQuery()
        {
            ViewBag.MenuBar = false;
            //ViewBag.pmsList = GetBtnPms("IQTB01");
            return View();
        }

        /// <summary>
        /// 拖车报价
        /// </summary>
        /// <returns></returns>
        public ActionResult QuotTrailerSetup()
        {
            string isa = Prolink.Math.GetValueAsString(Request["isa"]);
            ViewBag.isa = isa;
            if (isa.Equals("Y"))
            {
                ViewBag.pmsList = GetBtnPms("IQT021");
            }
            else
            {
                ViewBag.pmsList = GetBtnPms("IQTC01");
            }
            Dictionary<string, List<string>> options = GetCodeSelectData("'WH'", GroupId);
            if (options.ContainsKey("WH"))
                ViewBag.SelectWH = string.Join(";", options["WH"]);
            string uid = Prolink.Math.GetValueAsString(Request["UId"]);
            string lspCd = Prolink.Math.GetValueAsString(Request["LspCd"]);
            ViewBag.Uid = uid;
            ViewBag.Location = Request["Location"];
            ViewBag.LspCd = lspCd;
            if (!string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(lspCd))
            {
                string sql = string.Format("SELECT LSP_CD FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
                lspCd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                ViewBag.LspCd = lspCd;
            }
            ViewBag.ioflag = IOFlag;
            SetSchema("SMQTM");
            
            return View();
        }

        /// <summary>
        /// 报关报价
        /// </summary>
        /// <returns></returns>
        public ActionResult QuotBrokerSetup()
        {
            string isa = Prolink.Math.GetValueAsString(Request["isa"]);
            ViewBag.isa = isa;
            if (isa.Equals("Y"))
            {
                ViewBag.pmsList = GetBtnPms("IQT021");
            }
            else
            {
                ViewBag.pmsList = GetBtnPms("IQTB01");
            }
            Dictionary<string, List<string>> options = GetCodeSelectData("'WH'", GroupId);
            if (options.ContainsKey("WH"))
                ViewBag.SelectWH = string.Join(";", options["WH"]);
            ViewBag.Uid = Request["UId"];
            ViewBag.ioflag = IOFlag;
            ViewBag.SelectProductType = CommonHelp.getBscodeForColModelDescp("TSPA", GetDataPmsCondition("C"));
            SetSchema("SMQTM");
            return View();
        }

        public ActionResult QuotApproveQuery()
        {
            string tranType = Request["tranType"];
            if (string.IsNullOrEmpty(tranType)) tranType = string.Empty;
            ViewBag.TranType = tranType;
            ViewBag.pmsList = GetBtnPms("IQT021" + tranType);
            GetBscode("QTA");
            ViewBag.MenuBar = false;
            GetUseAcct();
            SetApproveRole("QUOT");
            SetApproveSelect();
            GetRoleSelect("QUOT");
            return View();
        }

        public void GetBscode(string mode)
        {
            ViewBag.SelectTranMode = "";
            ViewBag.DefaultTranMode = "";
            string sql = string.Format("SELECT CD,CD_DESCP,AR_CD FROM BSCODE WHERE CD_TYPE='{0}' AND GROUP_ID={1} AND( CMP={2} OR CMP ='*')", mode, SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultTranMode = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                string eRemark = Prolink.Math.GetValueAsString(dr["AR_CD"]).Trim();
                if ("en-US".Contains(SiteLang) && !string.IsNullOrEmpty(eRemark))
                {
                    select += ":" + eRemark;
                }
                else
                {
                    select += ":" + Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                }
            }
            ViewBag.SelectTranMode = select;
        }

        private void GetUseAcct()
        {
            string sql = string.Format("SELECT U_PRI FROM SYS_ACCT WHERE U_ID={0} AND {1}", SQLUtils.QuotedStr(UserId), GetBaseDep());
            string upri = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.Upri = upri;
        }

        private void GetRoleSelect(string type)
        {
            ViewBag.SelectRole = "";
            ViewBag.DefaultRole = "";

            #region Approve
            string sql = string.Format(@"SELECT APPROVE_GROUP,GROUP_DESCP,SEQ_NO FROM APPROVE_ATTR_D WHERE U_FID=(select U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR='{2}' AND {0}) AND {1}
            ORDER BY SEQ_NO,APPROVE_GROUP ASC", GetBaseCmp(), GetBaseCmp(), type);


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

        private void SetApproveSelect()
        {
            ViewBag.SelectApprove = "";
            ViewBag.DefaultApprove = "";

            string approveroles = GetApproveBack(UserId, CompanyId, GroupId, UPri, Dep);
            ViewBag.SelectApprove = approveroles;
        }
        public static string GetApproveBack(string UserId, string CompanyId, string GroupId, string upri, string Dep)
        {
            string sql = string.Format(@"SELECT D.APPROVE_ATTR,D.APPROVE_GROUP FROM APPROVE_ATTR_D D LEFT JOIN APPROVE_ATTR_DP DP　
  ON D.U_ID = DP.U_FID WHERE DP.USER_ID={0} AND DP.CMP={1}  AND DP.GROUP_ID={2} AND APPROVE_ATTR='QUOT'",//AND DP.STN={2}
             SQLUtils.QuotedStr(UserId),
             SQLUtils.QuotedStr(CompanyId),
             SQLUtils.QuotedStr(GroupId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string approveroles = "Person;A;";//申请者的状态下也可以出现  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approveroles += Prolink.Math.GetValueAsString(dt.Rows[i]["APPROVE_GROUP"]) + ";";
            }
            approveroles = approveroles.Trim(';');
            return approveroles;
        }


        public ActionResult LCLFreightQuery()
        {
            SetTranTypeSelect();
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult DTFreightQuery()
        {
            SetTranTypeSelect();
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult DEFreightQuery()
        {
            SetTranTypeSelect();
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult FCLFreightQuery()
        {
            SetTranTypeSelect();
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult IEFreightQuery()
        {
            SetTranTypeSelect();
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult AirFreightQuery()
        {
            SetTranTypeSelect();
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            ViewBag.MenuBar = false;
            return View();
        }

        /// <summary>
        /// 运输管理
        /// </summary>
        /// <returns></returns>
        public ActionResult IQT011()
        {
            SetTranTypeSelect();
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult QTQuery()
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            RQManageController.ChangeQuotType();
            SetTranModeSelect();
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult FCLChgQueryView()
        {
            ViewBag.MenuBar = false;
            //ViewBag.pmsList = GetBtnPms("IQT013");
            return View();
        }

        public ActionResult FCLChgSetup(string id = null, string uid = null)
        {
            SetSchema("FCLFSFSetup");
            ViewBag.pmsList = GetBtnPms("IQT013");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            ViewBag.ioflag = IOFlag;
            return View();
        }

        public ActionResult SendMail()
        {
            string uid = Request["UId"];
            string msg = "success";

            string lspcd = Prolink.Math.GetValueAsString(Request["LspCd"]);
            //string sql = string.Format("SELECT * FROM TKPMG WHERE {0} AND CMP={1}", GetBaseGroup(), SQLUtils.QuotedStr(this.BaseCompanyId));
            string sql = string.Format("SELECT * FROM TKPMG WHERE {0} AND CMP={1} And NAME={2}", GetBaseGroup(), SQLUtils.QuotedStr(lspcd), SQLUtils.QuotedStr("IQQT"));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string mailto = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                string mail = Prolink.Math.GetValueAsString(dr["MAIL_ID"]);
                if (!string.IsNullOrEmpty(mail))
                {
                    if (!string.IsNullOrEmpty(mailto))
                    {
                        mailto += ";";
                    }
                    mailto += mail;
                }
            }
            if (string.IsNullOrEmpty(mailto))
            {
                return Json(new { IsOk = false, message = "Please go Mail Group Setup to add this company code into mail and set for \"inbound quotation enquiry\". " });
            }
            TrackingEDI.Business.EvenFactory.AddEven(uid + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), uid, MailManager.Inquery_Quotation, null, 1, 0, mailto, "報價通知");
            //TrackingEDI.Business.EvenFactory.RegisterSendMail("IQQT", MailManager.SendIQQTvoid);
            EvenFactory.ExecuteMailEven("IQQT");
            MixedList mlist = new MixedList() { };
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID",uid);
            ei.Put("QUOT_TYPE","I");
            OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            bool re = true;
            return Json(new { IsOk = re, message = msg });
            //return ToContent(msg);
        }

        public ActionResult FCLFSFSetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("FCLFSFSetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            return View();
        }

        public ActionResult FCLFSetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("FCLFSetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            return View();
        }

        public ActionResult LCLSetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("LCLSetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            return View();
        }

        public ActionResult AirSetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("AirSetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            return View();
        }

        public ActionResult IESetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("IESetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];

            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);
            return View();
        }

        public ActionResult DESetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("DESetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            return View();
        }

        public ActionResult DTSetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("DTSetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            SetTranTypeSelect();

            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN','TDT'", GroupId, " ORDER BY AP_CD ASC");
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);
            if (options.ContainsKey("TDT"))
                ViewBag.QuotType = string.Join(";", options["TDT"]);
            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);

            //BSSTATE
            return View();
        }

        public ActionResult ContractQuery()
        {
            ViewBag.pmsList = GetBtnPms("QT012");
            Dictionary<string, List<string>> options = GetCodeSelectData("'TRGN'", GroupId);
            if (options.ContainsKey("TRGN"))
                ViewBag.SelectTRGN = string.Join(";", options["TRGN"]);
            List<string> options1 = GetStateSelectData(GroupId);
            ViewBag.SelectSTATE = string.Join(";", options1);
            SetTranModeSelect();
            ViewBag.MenuBar = false;
            SetApproveRole();
            ViewBag.Upri = UPri;
            ViewBag.SelectRole = BaseApprove.GetRoleSelect(GetBaseCmp(), "Contract",true);
            return View();
        }

        private void SetApproveRole(string attr="")
        {
            ViewBag.ApproveRole = "";
            string approveroles = ContractApproveHelper.GetApprove(UserId, CompanyId, GroupId, UPri, Dep, attr);
            ViewBag.ApproveRole = approveroles;
        }

        public ActionResult ContractSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            SetSchema("ContractSetup");
            ViewBag.pmsList = GetBtnPms("QT012");

            ViewBag.Uid = id;
            SetTranModeSelect();
            ViewBag.SelectRole = BaseApprove.GetRoleSelect(GetBaseCmp(), "Contract");
            ViewBag.CmpNm = getOneValueAsStringFromSql("SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE STATUS='U' AND PARTY_NO=" + SQLUtils.QuotedStr(CompanyId));
            return View();
        }

        public ActionResult GetQtdData()
        {
            string RfqNo = Prolink.Math.GetValueAsString(Request.Params["RfqNo"]);
            string LspNo = Prolink.Math.GetValueAsString(Request.Params["LspNo"]);
            string sql = string.Empty;

            sql = "SELECT * FROM SMQTD WHERE RFQ_NO={0} AND LSP_CD={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(RfqNo), SQLUtils.QuotedStr(LspNo));

            DataTable dt = getDataTableFromSql(sql);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(dt, "SmqtdModel");

            return ToContent(data);
        }

        /// <summary>
        /// 物流費用匯總
        /// </summary>
        /// <returns></returns>
        public ActionResult ShipFeeQueryView()
        {
            ViewBag.MenuBar = false;
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            //ViewBag.pmsList = GetBtnPms("IQTB01");
            ViewBag.ShipmentId = ShipmentId;
            return View();
        }

        public ActionResult ShipFeeSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            //ViewBag.pmsList = GetBtnPms("QT012");
            ViewBag.SpId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            ViewBag.Uid = id;
            SetSchema("ShipFeeSetup");
            return View();
        }

        public ActionResult FCLEasyViewR(string id = null, string uid = null)
        {
            SetSchema("FCLFSFSetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            return View();
        }

        public ActionResult FCLEasyViewB(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IQT010");
            SetSchema("FCLFSFSetup");
            ViewBag.RQUid = Request["RQUid"];
            ViewBag.Op = Request["Op"];
            ViewBag.Uid = Request["UId"];
            ViewBag.LspCd = Request["LspCd"];
            ViewBag.RfqNo = Request["RfqNo"];
            return View();
        }

        #endregion

        #region 基础方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        /// <summary>
        /// 设置Schema
        /// </summary>
        /// <param name="name"></param>
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "RQSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMRQM WHERE 1=0";
                        break;
                    case "SMQTM":
                    case "FCLFSFSetup":
                    case "FCLFSetup":
                    case "LCLSetup":
                    case "DTSetup":
                    case "DESetup":
                    case "AirSetup":
                    case "IESetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMQTM WHERE 1=0";
                        break;
                    case "ShipFeeSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMBID WHERE 1=0";
                        break;
                    case "ContractSetup":
                         kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMCTM WHERE 1=0";
                        break;

                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
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
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                if (resultType == "excel")
                    return ExportExcelFile(dt);
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

            string sql = " SELECT  '" + statusField + "' col," + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "  SELECT  '" + statusField + "' col, '' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
            DataTable dtsum = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dtAll = dt.Copy();
            foreach (DataRow dr in dtsum.Rows)
            {
                dtAll.ImportRow(dr);
            }

            return dtAll;
        }

        private void SetTranTypeSelect()
        {
            ViewBag.SelectTranType = "";
            ViewBag.DefaultTranType = "";

            #region Tran Mode
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TDTK' AND GROUP_ID={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            //string sql = "SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TTRN'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            //A:有色采购;B:有色销售;C:冷链;D:日化;E:空白
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultTranType = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                select += ":" + Prolink.Math.GetValueAsString(dr["CD"]).Trim() + "." + Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
            }
            ViewBag.SelectTranType = select;
            #endregion
        }
        #endregion

        #region 查询
        public ActionResult QTManageQueryData()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string condition = GetCreateDateCondition("SMQTM", "1=1");
            if (virCondition != "")
            {
                condition += " AND QUOT_NO IN ( SELECT QUOT_NO FROM SMQTD WHERE 1=1 AND " + virCondition + " )";
            }
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string col = "*,(CASE WHEN OUT_IN='O' THEN 'OutBound' WHEN OUT_IN='I' THEN 'INBound' WHEN OUT_IN='O;I' THEN 'OutBound,INBound' ELSE OUT_IN END) OUT_IN_DESC,(SELECT DISTINCT CONTRACT_NO+';' FROM SMQTD WHERE SMQTD.QUOT_NO=SMQTM.QUOT_NO FOR XML PATH('')) AS CONTRACT_INFO";
            string table = string.Format("(SELECT {2} FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE NOT IN ('O','C','B','X'))T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), col);
            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                if ("G".Equals(upri))
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND RLOCATION=" + SQLUtils.QuotedStr(this.CompanyId);
                table = string.Format("(SELECT {3} FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE NOT IN ('O','C','B','X') UNION SELECT {3} FROM SMQTM WHERE {2} AND TRAN_MODE NOT IN ('O','C','B','X'))T",
                    SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), innerCondition, col);
            }

            return GetBootstrapData(table, condition);
        }

        public ActionResult GetQTDataItem()
        {
            string u_id = Request["UId"];
            //string sql = string.Format("SELECT SMQTM.*,(SELECT TOP 1 STATUS FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTM.RFQ_NO) AS RQ_STATUS FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            string sql = string.Format("SELECT *  FROM (SELECT * FROM SMQTM WHERE U_ID={0})A OUTER APPLY (SELECT TOP 1 STATUS AS RQ_STATUS,U_ID AS RQ_UID,GROUP_ID AS RQ_GROUPID,CMP AS RQ_CMP FROM SMRQM WITH (NOLOCK) WHERE SMRQM.RFQ_NO=A.RFQ_NO) B", SQLUtils.QuotedStr(u_id));

            string lspCd = Request["LspCd"], rfqNo = Request["RfqNo"];
            if (string.IsNullOrEmpty(u_id))
                sql = string.Format("SELECT *  FROM (SELECT * FROM SMQTM WHERE LSP_CD={0} AND RFQ_NO={1})A OUTER APPLY (SELECT TOP 1 STATUS AS RQ_STATUS,U_ID AS RQ_UID,GROUP_ID AS RQ_GROUPID,CMP AS RQ_CMP FROM SMRQM WITH (NOLOCK) WHERE SMRQM.RFQ_NO=A.RFQ_NO) B", SQLUtils.QuotedStr(lspCd), SQLUtils.QuotedStr(rfqNo));
            //sql = string.Format("SELECT SMQTM.*,(SELECT TOP 1 STATUS FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTM.RFQ_NO) AS RQ_STATUS FROM SMQTM WHERE LSP_CD={0} AND RFQ_NO={1}", SQLUtils.QuotedStr(lspCd), SQLUtils.QuotedStr(rfqNo));

            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (mainDt.Rows.Count > 0)
                u_id = Prolink.Math.GetValueAsString(mainDt.Rows[0]["U_ID"]);
            else
                u_id = System.Guid.NewGuid().ToString();
            Dictionary<string, object> data = GetQtDatas(u_id, mainDt);
            return ToMyContent(data);
        }

        public ActionResult GetAirData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE='A'");
        }

        public ActionResult GetDEData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE='A'");
        }

        public ActionResult GetDTData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE='A'");
        }

        public ActionResult GetFCLFSData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE='A'");
        }

        public ActionResult GetFCLFSFData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE='A'");
        }

        public ActionResult GetIEData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE='A'");
        }

        public ActionResult GetLCLData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE='L'");
        }

        /// <summary>
        /// FCL  OTH 费用
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFCLCHGData()
        {
            return GetBootstrapData("SMQTM", GetBaseGroup() + " AND TRAN_MODE = 'O'");
        }

        public ActionResult ShipFeeQueryData()
        {
            string condition = GetBaseCmp();
            string table = "V_SHIPFEE";
            return GetBootstrapData(table, condition);
        }
        #endregion

        #region 保存

        public ActionResult SaveQTData()
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
            string rfq_no = Request["rfq_no"];
            string dn_no = string.Empty;
            string u_id = Request["u_id"];
            string term = Request["term"];
            string mode = Request["mode"];
            string dataType = Request["dataType"];
            string contractType = Request["contractType"];
            string ei0 = null;
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            if (!string.IsNullOrEmpty(rfq_no))
                rfq_no = HttpUtility.UrlDecode(rfq_no);
            if (!string.IsNullOrEmpty(term))
                term = HttpUtility.UrlDecode(term);
            if (!string.IsNullOrEmpty(contractType))
                contractType = HttpUtility.UrlDecode(contractType);
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            string lspCd = Request["lspCd"];
            if (!string.IsNullOrEmpty(lspCd)) lspCd = HttpUtility.UrlDecode(lspCd);
            string quot_no = Request["quot_no"];
            if (!string.IsNullOrEmpty(quot_no)) quot_no = HttpUtility.UrlDecode(quot_no);

            if (dict.ContainsKey("ContractList"))
            {
                ArrayList List = dict["ContractList"] as ArrayList;
                List<string> objList = new List<string>();
                for (int j = 0; j < List.Count; j++)
                {
                    string contractNo = Prolink.Math.GetValueAsString(List[j]).Trim();
                    if (!string.IsNullOrEmpty(contractNo) && !objList.Contains(contractNo))
                        objList.Add(contractNo);
                }
                if (objList.Count > 0)
                {
                    sql = string.Format("SELECT DISTINCT CONTRACT_NO FROM SMQTD WHERE CONTRACT_NO IN {0} AND QUOT_NO!={1}", SQLUtils.Quoted(objList.ToArray()), SQLUtils.QuotedStr(quot_no));
                    DataTable tdDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string contractList = "";
                    foreach (DataRow dr in tdDt.Rows)
                    {
                        string contractNo = Prolink.Math.GetValueAsString(dr["CONTRACT_NO"]).Trim();
                        if (!string.IsNullOrEmpty(contractList))
                            contractList += ",";
                        contractList += contractNo;
                    }
                    if (!string.IsNullOrEmpty(contractList))
                        return Json(new { message = contractList + " :" + @Resources.Locale.L_QTManage_Controllers_ContractNo });
                }
            }

            bool isDel = false;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmqtmModel");
                    DateTime odt = DateTime.Now;
                    DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (string.IsNullOrEmpty(rfq_no))
                            rfq_no = ei.Get("RFQ_NO");
                        if (string.IsNullOrEmpty(u_id))
                            u_id = ei.Get("U_ID");
                        if (string.IsNullOrEmpty(lspCd))
                            lspCd = ei.Get("LSP_CD");
                        if (string.IsNullOrEmpty(term))
                            term = ei.Get("INCOTERM");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt); 
                            //数据验证
                            sql = string.Format("SELECT * FROM SMRQM WHERE RFQ_NO={0} "
                              , SQLUtils.QuotedStr(ei.Get("RFQ_NO")));

                            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (dt.Rows.Count > 0)
                                return Json(new { message = @Resources.Locale.L_QTManage_Controllers_391 });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            isDel = true;
                        }
                        mixList.Add(ei);


                        if (ei.OperationType != EditInstruct.DELETE_OPERATION && "L".Equals(dataType))
                        {
                            if (!string.IsNullOrEmpty(u_id))
                                ei0 = string.Format("UPDATE SMQTD SET POL_CD=(SELECT TOP 1 POL_CD FROM SMQTM WHERE U_ID={0}) WHERE TRAN_MODE='X' AND U_FID={0}", SQLUtils.QuotedStr(u_id));
                        }

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei = new EditInstruct("SMQTD", EditInstruct.DELETE_OPERATION);
                            ei.PutKey("U_FID", u_id);
                            mixList.Add(ei);
                        }
                    }
                }
                else if ("sub".Equals(item.Key) || "sub1".Equals(item.Key))
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmqtdModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        string carrier = ei.Get("CARRIER");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            string chg_cd = ei.Get("CHG_CD");
                            if (string.IsNullOrEmpty(chg_cd))
                                ei.Put("CHG_CD", "FRT");
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("RFQ_NO", rfq_no);
                            if (!string.IsNullOrEmpty(lspCd))
                                ei.Put("LSP_CD", lspCd);
                            if (!string.IsNullOrEmpty(quot_no))
                                ei.Put("QUOT_NO", quot_no);
                            if ("1".Equals(mode) && !string.IsNullOrEmpty(term))
                                ei.Put("INCOTERM", term);
                            ei.Put("U_FID", u_id);
                            if (!string.IsNullOrEmpty(contractType))
                                ei.Put("CONTRACT_TYPE", contractType);
                            string contractNo = ei.Get("CONTRACT_NO");
                            if (!string.IsNullOrEmpty(contractNo))
                                ei.Put("CONTRACT_NO", contractNo.Trim());
                            //ei.Put("CMP", CompanyId);
                            //ei.Put("GROUP_ID", GroupId);
                            if (!string.IsNullOrEmpty(carrier))
                                ei.Put("CARRIER", carrier.Replace("&nbsp;", " "));
                            #region 运费报价查询 手动新增明细 OUT_IN 未带出
                            if (string.IsNullOrEmpty(ei.Get("OUT_IN")) && !string.IsNullOrEmpty(u_id))
                            {
                                string outInSql = string.Format("SELECT OUT_IN FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
                                string outIn = OperationUtils.GetValueAsString(outInSql, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (!string.IsNullOrEmpty(outIn))
                                {
                                    ei.Put("OUT_IN", outIn);
                                }
                            }
                            #endregion
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            if (!string.IsNullOrEmpty(contractType))
                                ei.Put("CONTRACT_TYPE", contractType);
                            string contractNo = ei.Get("CONTRACT_NO");
                            if (!string.IsNullOrEmpty(contractNo))
                                ei.Put("CONTRACT_NO", contractNo.Trim());
                            if (!string.IsNullOrEmpty(carrier))
                                ei.Put("CARRIER", carrier.Replace("&nbsp;", " "));
                        }
                        else
                            ei.AddKey("U_ID");
                        System.Guid test_id = System.Guid.NewGuid();
                        if (!System.Guid.TryParse(ei.Get("U_ID"), out test_id))
                            continue;
                        mixList.Add(ei);
                    }
                }
            }

            string seqquiry = Request.Params["seqquery"];
            if (!string.IsNullOrEmpty(seqquiry))
            {
                seqquiry = System.Web.HttpUtility.UrlDecode(seqquiry);
                Dictionary<string, string> seq = js.Deserialize<Dictionary<string, string>>(seqquiry);
                foreach (var item in seq)
                {
                    EditInstruct ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", item.Key);
                    ei.Put("SEQ_NO", item.Value);
                    mixList.Add(ei);
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    if (!isDel && !string.IsNullOrEmpty(u_id))
                    {
                        //EditInstruct ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
                        //ei.Condition = string.Format("U_FID={0}", SQLUtils.QuotedStr(u_id));
                        //ei.PutKey("U_FID", u_id);
                        //ei.Put("LSP_CD", lspCd);
                        mixList.Add(string.Format("UPDATE SMQTD SET LSP_CD=(SELECT TOP 1 LSP_CD FROM SMQTM WHERE SMQTM.U_ID={0}) WHERE U_FID={0}", SQLUtils.QuotedStr(u_id)));
                    }
                    if (!string.IsNullOrEmpty(ei0))
                        mixList.Add(ei0);

                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            sql = string.Format("SELECT * FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = GetQtDatas(u_id, mainDt);
            return ToContent(data);
        }

        private static Dictionary<string, object> GetQtDatas(string u_id, DataTable mainDt)
        {
            string sql = string.Empty;
            SetTableEdit(mainDt);
            string tran_type = string.Empty;
            string period = string.Empty;
            if (mainDt.Rows.Count > 0)
            {
                tran_type = Prolink.Math.GetValueAsString(mainDt.Rows[0]["TRAN_MODE"]);
                period = Prolink.Math.GetValueAsString(mainDt.Rows[0]["PERIOD"]);
            }
            DataTable subDt = new DataTable(), subDt1 = new DataTable();
            if ("D".Equals(tran_type) || "E".Equals(tran_type) || "T".Equals(tran_type))
            {
                sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND CHG_CD='FRT' ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND CHG_CD<>'FRT' ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
                subDt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else if ("F".Equals(tran_type))
            {
                sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //if ("R".Equals(period))
                //{
                //    sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND CHG_CD<>'FRT' ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
                //    subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //}
                //else
                //{
                //    sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND CHG_CD='FRT' ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
                //    subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //}
            }
            else
            {
                sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
                subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmqtmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmqtdModel");
            data["sub1"] = ModelFactory.ToTableJson(subDt1, "SmqtdModel");

            if (mainDt.Rows.Count > 0)
            {
                string rfq_no = Prolink.Math.GetValueAsString(mainDt.Rows[0]["RFQ_NO"]);
                sql = string.Format("SELECT * FROM SMRQM WHERE RFQ_NO={0}", SQLUtils.QuotedStr(rfq_no));
                DataTable qrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if ("R".Equals(period))
                    data["rq"] = ModelFactory.ToTableJson(qrDt, "SmrqmModel");
            }
            return data;
        }

        private static void SetTableEdit(DataTable mainDt)
        {
            mainDt.Columns.Add("SYS_EDIT", typeof(bool));//是否允许编辑
            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            foreach (DataRow dr in mainDt.Rows)
            {
                dr["SYS_EDIT"] = true;
                if (dr["RFQ_TO"] != null && dr["RFQ_TO"] != DBNull.Value)
                {
                    DateTime rfq_to = (DateTime)dr["RFQ_TO"];
                    rfq_to = new DateTime(rfq_to.Year, rfq_to.Month, rfq_to.Day, 0, 0, 0);
                    if (now.CompareTo(rfq_to) > 0)
                        dr["SYS_EDIT"] = false;
                }
            }
        }
        #endregion

        #region 物流费用相关
        public ActionResult GetShipFeeDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMBID WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbidModel");
            return ToContent(data);
        }
        public ActionResult ShipFeeUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
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

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbidModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("CREATE_BY", UserId);
                            DateTime odt = DateTime.Now;                          
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("CREATE_DATE", odt);
                            ei.PutDate("CREATE_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            DateTime odt = DateTime.Now;                         
                            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                            
                            ei.PutDate("MODIFY_DATE", odt);
                            ei.PutDate("MODIFY_DATE_L", ndt);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            UId = ei.Get("U_ID");
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
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
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            string sql = string.Format("SELECT * FROM SMBID WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbidModel");
            return ToContent(data);
        }
        #endregion

        #region 取得下載帳單之費用代碼
        public ActionResult GetTransTypeInfo()
        {
            string returnMsg = "success";
            string TransType = Request.Params["TransType"];
            string Cmp = Request.Params["Cmp"];
            string VenderCd = Prolink.Math.GetValueAsString(Request.Params["VenderCd"]);

            if (Cmp == "")
            {
                Cmp = CompanyId;
            }
            string Type = Request.Params["Type"];
            //string sql = "SELECT DISTINCT CHG_CD,CHG_DESCP FROM SMCHG WHERE " + GetBaseGroup() + " AND CMP = " + SQLUtils.QuotedStr(Cmp) + " AND IO_TYPE = 'O' AND TRAN_MODE=" + SQLUtils.QuotedStr(TransType);
            string chgTypeStr = "";
            string gwTypeStr = "";
            string cbmTypeStr = "";
            string chgTypeColsStr = "";
            try
            {
                DataTable dt = CommonHelp.GetChargeCodesByVender("CMP=" + SQLUtils.QuotedStr(Cmp), VenderCd);//OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable dtAll = dt.Copy();
                DataTable dt1 = null;
                if (dt.Rows.Count == 0)
                {
                    dt1 = CommonHelp.GetChargeCodes(GetDataPmsCondition("C"), 1);
                }
                else
                {
                    dt1 = CommonHelp.GetChargeCodes(GetDataPmsCondition("C"), 2);
                }
                dtAll.Merge(dt1);

                if (dtAll.Rows.Count == 0)
                {
                    returnMsg = "fail";
                    return Json(new { message = "該物流業者無費用報價" });
                }
                foreach (DataRow dr in dtAll.Rows)
                {
                    if (Prolink.Math.GetValueAsString(dr["CHG_DESCP"]) != "")
                    {
                        string chg = Prolink.Math.GetValueAsString(dr["CHG_DESCP"]).Replace("\n", "") + "\n(" + Prolink.Math.GetValueAsString(dr["REMARK"]).Replace("\n", "") + ")";

                        chgTypeStr += chg + ";";
                    }

                    if (Prolink.Math.GetValueAsString(dr["GW"]) != "")
                        gwTypeStr += Prolink.Math.GetValueAsString(dr["GW"]).Replace("\n", "") + ";";
                    if (Prolink.Math.GetValueAsString(dr["CBM"]) != "")
                        cbmTypeStr += Prolink.Math.GetValueAsString(dr["CBM"]).Replace("\n", "") + ";";

                    chgTypeColsStr += WebGui.Models.BaseModel.GetModelFiledName(Prolink.Math.GetValueAsString(dr["CHG_CD"])) + ";";
                }
            }
            catch (Exception ex)
            {
                returnMsg = "fail";
                return Json(new { message = returnMsg });
            }

            if (chgTypeStr != "")
                chgTypeStr = chgTypeStr.Substring(0, chgTypeStr.Length - 1);
            if (gwTypeStr != "")
                gwTypeStr = gwTypeStr.Substring(0, gwTypeStr.Length - 1);
            if (cbmTypeStr != "")
                cbmTypeStr = cbmTypeStr.Substring(0, cbmTypeStr.Length - 1);

            return Json(new { message = returnMsg, chgTypeStr = chgTypeStr, chgTypeColsStr = chgTypeColsStr.Substring(0, chgTypeColsStr.Length - 1), gwTypeStr = gwTypeStr, cbmTypeStr = cbmTypeStr, cmp = CompanyId, stn = Station, user = UserId, group = GroupId, createDate = DateTime.Now.ToString("yyyy/MM/dd HH:ss") });

        }
        #endregion

        public static string HandleQuotDetail(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("RFQ_NO", Prolink.Math.GetValueAsString(parm["RFQ_NO"]));
            ei.Put("QUOT_NO", Prolink.Math.GetValueAsString(parm["QUOT_NO"]));

            if (string.IsNullOrEmpty(ei.Get("INCOTERM")))
                ei.Put("INCOTERM", Prolink.Math.GetValueAsString(parm["INCOTERM"]));
            if (string.IsNullOrEmpty(ei.Get("TRAN_MODE")))
                ei.Put("TRAN_MODE", Prolink.Math.GetValueAsString(parm["TRAN_MODE"]));
            if (string.IsNullOrEmpty(ei.Get("SERVICE_MODE")))
                ei.Put("SERVICE_MODE", Prolink.Math.GetValueAsString(parm["SERVICE_MODE"]));

            if (string.IsNullOrEmpty(ei.Get("CUR")))
                ei.Put("CUR", Prolink.Math.GetValueAsString(parm["CUR"]));

            string tran_mode = Prolink.Math.GetValueAsString(parm["TRAN_MODE"]);
            string mapping = Prolink.Math.GetValueAsString(parm["mapping"]);
            if ("QuotF".Equals(mapping))
            {
                DataTable codeDt = parm["codeDt"] as DataTable;
                foreach (DataColumn col in dr.Table.Columns)
                {
                    string name = col.ColumnName.ToLower().Trim();
                    if ("term".Equals(name))
                    {
                        string val = Prolink.Math.GetValueAsString(dr[col.ColumnName]).Trim().ToUpper();
                        if (string.IsNullOrEmpty(val))
                            break;
                        string[] cp = val.Split(new string[] { "-" }, StringSplitOptions.None);
                        if (cp.Length > 0)
                        {
                            PutTerm(ei, codeDt, "LOADING_FROM", cp[0]);
                        }
                        if (cp.Length > 1)
                        {
                            PutTerm(ei, codeDt, "LOADING_TO", cp[1]);
                        }
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(ei.Get("LOADING_TO")))
                ei.Put("LOADING_TO", Prolink.Math.GetValueAsString(parm["LOADING_TO"]));
            if (string.IsNullOrEmpty(ei.Get("LOADING_FROM")))
                ei.Put("LOADING_FROM", Prolink.Math.GetValueAsString(parm["LOADING_FROM"]));
            CheckQTField(ei, "CUR");
            CheckQTField(ei, "PUNIT");
            CheckQTField(ei, "POL_CD");
            CheckQTField(ei, "POD_CD");
            CheckQTField(ei, "ALL_IN");

            ei.Put("LSP_CD", Prolink.Math.GetValueAsString(parm["LSP_CD"]));
            ei.Put("U_FID", Prolink.Math.GetValueAsString(parm["U_FID"]));
            ei.Put("OUT_IN", Prolink.Math.GetValueAsString(parm["OUT_IN"]));
            if (string.IsNullOrEmpty(ei.Get("CHG_CD")))
                ei.Put("CHG_CD", "FRT");
            int seq_no = Prolink.Math.GetValueAsInt(parm["SEQ_NO"]);
            ei.Put("SEQ_NO", seq_no);
            parm["SEQ_NO"] = seq_no + 1;
            bool error = true;
            if ("F".Equals(tran_mode))
            {
                for (int i = 1; i <= 50; i++)
                {
                    if (ei.GetValueAsDecimal("F" + i) != 0)
                    {
                        error = false;
                        break;
                    }
                }
                if (error)
                    return ExcelParser.ERROR;
            }
            if (string.IsNullOrEmpty(ei.Get("POL_CD")))
                return ExcelParser.ERROR;
            return string.Empty;
        }

        /// <summary>
        /// 根据代码建档设置值
        /// </summary>
        /// <param name="ei"></param>
        /// <param name="codeDt"></param>
        /// <param name="val"></param>
        /// <param name="name"></param>
        private static void PutTerm(EditInstruct ei, DataTable codeDt, string name, string val)
        {
            string v0 = GetCode(null, "PK", codeDt, val);
            if (string.IsNullOrEmpty(v0))
            {
                if ("DR".Equals(val))
                {
                    v0 = GetCode(null, "PK", codeDt, "DOOR");
                    if (string.IsNullOrEmpty(v0))
                        v0 = val;
                }
                else if ("DOOR".Equals(val))
                {
                    v0 = GetCode(null, "PK", codeDt, "DR");
                    if (string.IsNullOrEmpty(v0))
                        v0 = val;
                }
            }
            if (string.IsNullOrEmpty(v0))
                v0 = val;
            ei.Put(name, v0);
        }

        private static void CheckQTField(EditInstruct ei, string name)
        {
            string val = ei.Get(name);
            if (!string.IsNullOrEmpty(val))
            {
                val = val.ToUpper().Trim();
                ei.Put(name, val);
            }
        }

        #region 報價上傳新方法
        public ActionResult QtFileUpload()
        {
            string returnMessage = "success", msg = string.Empty;

            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);
            string u_id = Prolink.Math.GetValueAsString(Request.Params["u_id"]);
            string rfq_no = Prolink.Math.GetValueAsString(Request.Params["rfq_no1"]);
            string quot_no = Prolink.Math.GetValueAsString(Request.Params["quot_no"]);
            string lsp_cd = Prolink.Math.GetValueAsString(Request.Params["lsp_cd"]);
            string sql = string.Empty;
            string cmp = getOneValueAsStringFromSql("SELECT CMP FROM SMQTM WHERE U_ID=" + SQLUtils.QuotedStr(u_id));
            List<Dictionary<string, object>> smdnpData = new List<Dictionary<string, object>>();
            if (u_id == "")
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            if ("undefined".Equals(rfq_no) || "null".Equals(rfq_no))
                rfq_no = "";

            sql = "SELECT TOP 1 FEE_TYPE FROM ECREFFEE WHERE VENDER_CD=" + SQLUtils.QuotedStr(lsp_cd);
            string oFeeType = getOneValueAsStringFromSql(sql);

            //sql = "SELECT TOP 1 FEE_TYPE FROM SMQTM WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            //string qFeeType = getOneValueAsStringFromSql(sql);

            sql = "SELECT TOP 1 CUR FROM SMQTM WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            string Cur = getOneValueAsStringFromSql(sql);
            /*
            if (oFeeType != "")
            {
                if (oFeeType != qFeeType)
                {
                    returnMessage = "匯入格式與報價格式不同，無法匯入";
                    return Json(new { message = returnMessage });
                }
            }
            */

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            else
            {
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);
                EditInstruct ei;

                /*
                 * dr[0]: TRAN_TYPE
                 * dr[1]: POL_NM
                 * dr[2]: POL_CD
                 * dr[3]: POD_NM
                 * dr[4]: POD_CD
                 * dr[5]: STATE
                 * dr[6]: STATE_NM
                 * dr[7]: TT
                 * dr[8~.....]: 一堆費用代碼
                 */
                try
                {
                    ArrayList ChgCdArray = new ArrayList();
                    ArrayList ChgDescpArry = new ArrayList();
                    ArrayList CurArry = new ArrayList();
                    List<string> uidArry = new List<string>();
                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        if (k < 8)
                        {
                            ChgCdArray.Add("xxx");
                            ChgDescpArry.Add("xxx");
                        }
                        else
                        {
                            string chg = Prolink.Math.GetValueAsString(dt.Columns[k]);
                            if (chg == "")
                            {
                                break;
                            }
                            try
                            {
                                /*
                                string[] chgArray = chg.Split('-');
                                string ChgCd = chgArray[0];
                                string ChgDescp = chgArray[1];
                                ChgCdArray.Add(ChgCd);
                                 * */
                                ChgDescpArry.Add(chg);

                            }
                            catch (Exception ex)
                            {
                                continue;
                            }

                        }
                    }

                    #region 查詢報價明細檔，若有則刪除
                    sql = "SELECT U_FID FROM SMQTD WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
                    DataTable ddt = getDataTableFromSql(sql);

                    if (ddt.Rows.Count > 0)
                    {
                        sql = "DELETE FROM SMQTD WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
                        exeSql(sql);
                    }
                    #endregion

                    MixedList mx = new MixedList();
                    int seq = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        //string TruckTypeNm = Prolink.Math.GetValueAsString(dr[0]);
                        string TranType = Prolink.Math.GetValueAsString(dr[0]);
                        string PolNm = Prolink.Math.GetValueAsString(dr[1]);
                        string PolCd = Prolink.Math.GetValueAsString(dr[2]);
                        string PodNm = Prolink.Math.GetValueAsString(dr[3]);
                        string PodCd = Prolink.Math.GetValueAsString(dr[4]);
                        string Tt = Prolink.Math.GetValueAsString(dr[7]);
                        /*
                        decimal PickFee = Prolink.Math.GetValueAsDecimal(dr[4]);
                        decimal DlvFee = Prolink.Math.GetValueAsDecimal(dr[5]);
                        decimal MinAmt = Prolink.Math.GetValueAsDecimal(dr[6]);
                         * */

                        #region 檢查地區
                        //sql = "SELECT DIST_CD FROM BSDIST WHERE DIST_NM=" + SQLUtils.QuotedStr(PolNm);
                        sql = "SELECT DIST_NM FROM BSDIST WHERE DIST_CD=" + SQLUtils.QuotedStr(PolCd);
                        PolNm = getOneValueAsStringFromSql(sql);
                        if (PolNm == "")
                        {
                            returnMessage = "第" + i.ToString() + "筆：啟運地不存在！";
                            continue;
                        }

                        sql = "SELECT DIST_NM FROM BSDIST WHERE DIST_CD=" + SQLUtils.QuotedStr(PodCd);
                        PodNm = getOneValueAsStringFromSql(sql);
                        if (PodNm == "")
                        {
                            returnMessage = "第" + i.ToString() + "筆：目的地不存在！";
                            continue;
                        }
                        #endregion
                        ei = new EditInstruct("SMQTD", EditInstruct.INSERT_OPERATION);
                        ei.Put("U_ID", System.Guid.NewGuid().ToString());
                        ei.Put("U_FID", u_id);
                        ei.Put("QUOT_NO", quot_no);
                        ei.Put("RFQ_NO", rfq_no);
                        ei.Put("CHG_CD", "FRT");
                        ei.Put("SEQ_NO", seq);
                        ei.Put("CHG_DESCP", getOneValueAsStringFromSql("SELECT TOP 1 CHG_DESCP FROM SMCHG WHERE CHG_CD='FRT' AND CMP=" + SQLUtils.QuotedStr(cmp)));
                        //string TruckType = getOneValueAsStringFromSql("SELECT TOP 1 CD FROM BSCODE WHERE CD_TYPE='TCP' AND CD_DESCP=" + SQLUtils.QuotedStr(TruckTypeNm) + " AND STN=" + SQLUtils.QuotedStr(stn));
                        //ei.Put("TRUCK_TYPE", TruckType);
                        ei.Put("TRAN_TYPE", TranType);
                        ei.Put("POL_CD", PolCd);
                        ei.Put("POL_NM", PolNm);
                        ei.Put("POD_CD", PodCd);
                        ei.Put("POD_NM", PodNm);
                        ei.Put("TT", Tt);
                        //ei.Put("PICK_FEE", PickFee);
                        //ei.Put("DLV_FEE", DlvFee);
                        //ei.Put("MIN_AMT", MinAmt);
                        ei.Put("CUR", Cur);

                        int k = 7;
                        for (int j = 7; j < dr.ItemArray.Length - 1; j++)
                        {
                            if (j + 1 >= ChgDescpArry.Count)
                            {
                                break;
                            }

                            string ChgDescp = Prolink.Math.GetValueAsString(ChgDescpArry[j + 1]);
                            string[] tmpArray = ChgDescp.Split('\n');
                            decimal Amt = Prolink.Math.GetValueAsDecimal(dr[j + 1]);

                            sql = "SELECT TOP 1 CHG_CD FROM ECREFFEE WHERE (CMP={0} OR CMP='*') AND CHG_DESCP={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(tmpArray[0]));
                            string ChgCd = getOneValueAsStringFromSql(sql);
                            if (ChgCd == "")
                            {
                                continue;
                            }
                            ei.Put(ChgCd, Amt);
                        }
                        seq++;
                        mx.Add(ei);
                    }

                    if (mx.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            msg += "更新明细发生错误 \n";
                        }
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }
            sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND CHG_CD='FRT' ORDER BY SEQ_NO", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            return Json(new { message = returnMessage, excelMsg = msg, subData = ModelFactory.ToTableJson(subDt, "SmqtdModel") });
        }
        #endregion

        public ActionResult FileUpload()
        {
            string groupId = string.Empty;
            string cmp = string.Empty;
            string stn = "*";
            List<string> msg = new List<string>();
            string returnMessage = "Y";
            bool isSucc = false;
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                string jobNo = Request.Params["jobNo"];
                MixedList ml = new MixedList();
                string excelFileName = string.Empty;
                try
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName).ToLower();
                    string path = Server.MapPath("~/FileUploads/");
                    if (!strExt.EndsWith(".xls") && !strExt.EndsWith(".xlsx"))
                        throw new Exception(@Resources.Locale.L_QTManageController_Controllers_167);

                    excelFileName = System.IO.Path.Combine(path, "MYQUOT" + DateTime.Now.ToString("yyyyMMddHHmmfff") + strExt);
                    file.SaveAs(excelFileName);
                    DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count <= 0)
                        throw new Exception(@Resources.Locale.L_QTManage_Controllers_395);

                    groupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
                    cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                    string quot_type = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_TYPE"]);

                    ml.Add(string.Format("DELETE FROM SMQTD WHERE U_FID={0}", SQLUtils.QuotedStr(jobNo)));

                    string tran_mode = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_MODE"]);
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    string rfq_no = Prolink.Math.GetValueAsString(dt.Rows[0]["RFQ_NO"]);
                    string quot_no = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_NO"]);
                    string lsp_cd = Prolink.Math.GetValueAsString(dt.Rows[0]["LSP_CD"]);
                    parm["RFQ_NO"] = rfq_no;
                    parm["QUOT_NO"] = quot_no;
                    parm["INCOTERM"] = Prolink.Math.GetValueAsString(dt.Rows[0]["INCOTERM"]);
                    parm["LOADING_FROM"] = Prolink.Math.GetValueAsString(dt.Rows[0]["LOADING_FROM"]);
                    parm["LOADING_TO"] = Prolink.Math.GetValueAsString(dt.Rows[0]["LOADING_TO"]);
                    parm["SERVICE_MODE"] = Prolink.Math.GetValueAsString(dt.Rows[0]["SERVICE_MODE"]);
                    parm["TRAN_MODE"] = tran_mode;
                    parm["LSP_CD"] = lsp_cd;
                    string cur = Prolink.Math.GetValueAsString(dt.Rows[0]["CUR"]);
                    parm["CUR"] = cur;
                    parm["OUT_IN"] = Prolink.Math.GetValueAsString(dt.Rows[0]["OUT_IN"]);
                    parm["U_FID"] = jobNo;
                    parm["SEQ_NO"] = 0;
                    parm["QUOT_TYPE"] = quot_type;
                    parm["CMP"] = cmp;
                    string mapping = "Quot" + tran_mode;
                    EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
                    if (!"F".Equals(quot_type))
                    {
                        //ei.PutKey("U_ID", jobNo);
                        //ei.PutKey("QUOT_TYPE", "Q");
                        //ei.PutDate("QUOT_DATE", DateTime.Now);
                        //ei.Put("QUOT_TYPE", "N");
                        //ml.Add(ei);
                    }
                    else
                        mapping = "QuotFC";

                    if ("R".Equals(tran_mode))
                    {
                        mapping = "QuotF";
                    }

                    parm["mapping"] = mapping;

                    string types = "'UB','PK','TRGN','TALN','TCAR'";
                    DataTable chgDt = null;

                    //if ("L".Equals(tran_mode) || "A".Equals(tran_mode) || "F".Equals(tran_mode))
                    //    chgDt = OperationUtils.GetDataTable(string.Format("SELECT TRAN_MODE,IO_TYPE,CHG_TYPE,CHG_CD,CHG_DESCP FROM SMCHG WHERE GROUP_ID={0}", SQLUtils.QuotedStr(groupId)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                    DataTable codeDt = OperationUtils.GetDataTable(string.Format("SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE {0} AND CD_TYPE IN({1})", GetBaseGroup(), types), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    DataTable stateDt = OperationUtils.GetDataTable(string.Format("SELECT STATE_CD,STATE_NM,REGION_CD,REGION_NM FROM BSSTATE WHERE GROUP_ID={0}", SQLUtils.QuotedStr(groupId)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                    DataTable cnDt = null;
                    if ("E".Equals(tran_mode))
                        cnDt = OperationUtils.GetDataTable(string.Format("SELECT CNTRY_CD,CNTRY_NM FROM BSCNTY WHERE {0}", GetBaseGroup(), types), null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    parm["codeDt"] = codeDt;
                    ExcelParser.RegisterEditInstructFunc(mapping, HandleQuotDetail);
                    ExcelParser ep = new ExcelParser();

                    //DataTable srcDt = ExcelHelper.ImportExcelToDataTable(excelFileName);
                    ep.Save(mapping, excelFileName, ml, parm);
                    //if (ep.ErrorMessager.Count > 0)
                    //{
                    //    ViewBag.MenuBar = false;
                    //    Response.Write("<script type=\"text/javascript\">parent.CallBack(\"" + "导入失败,请检查上传格式!" + string.Join("", ep.ErrorMessager.ToArray()) + "\")</script>");
                    //    return View();
                    //}

                    List<string> portC_list = new List<string>();
                    List<string> party_list = new List<string>();
                    List<string> chg_list = new List<string>();
                    for (int i = 0; i < ml.Count; i++)
                    {
                        EditInstruct ei0 = ml[i] as EditInstruct;
                        if (ei0 == null || ei0.OperationType == EditInstruct.DELETE_OPERATION)
                            continue;
                        GetPort(ei0.Get("POL_CD"), portC_list, tran_mode);
                        GetPort(ei0.Get("POD_CD"), portC_list, tran_mode);
                        GetChg(ei0.Get("CHG_CD"), chg_list, tran_mode);
                        GetParty(ei0.Get("CARRIER"), party_list);
                    }
                    DataTable portDt = null, partyDt = null;
                    string sql = "";
                    if (portC_list.Count > 0)
                    {
                        if (portC_list.Count <= 10)
                            portDt = OperationUtils.GetDataTable(string.Join(" UNION ", portC_list), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        else
                        {
                            if ("D".Equals(tran_mode) || "T".Equals(tran_mode))
                                sql = "SELECT PORT_CD,CNTRY_CD,[STATE],REGION,PORT_NM FROM BSTPORT";
                            else
                                sql = "SELECT PORT_CD,CNTRY_CD,[STATE],REGION,PORT_NM FROM BSCITY";
                            portDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                    }
                    if (party_list.Count > 0)
                        partyDt = OperationUtils.GetDataTable(string.Join(" UNION ", party_list), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (chg_list.Count > 0)
                    {
                        if (chg_list.Count <= 10)
                            chgDt = OperationUtils.GetDataTable(string.Join(" UNION ", chg_list), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        else
                            chgDt = OperationUtils.GetDataTable("SELECT TRAN_MODE,IO_TYPE,CHG_TYPE,CHG_CD,CHG_DESCP FROM SMCHG", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }

                    DataTable curDt = OperationUtils.GetDataTable(string.Format("SELECT CUR,CUR_DESCP FROM BSCUR WHERE {0}", GetBaseGroup(), types), null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    for (int i = 0; i < ml.Count; i++)
                    {
                        EditInstruct ei0 = ml[i] as EditInstruct;
                        if (ei0 == null || ei0.OperationType == EditInstruct.DELETE_OPERATION)
                            continue;
                        CheckPort(msg, "POL", ei0, portDt, tran_mode);
                        CheckPort(msg, "POD", ei0, portDt, tran_mode);
                        CheckCur(msg, "CUR", ei0, curDt, cur);
                        CheckChg(msg, "CHG_CD", ei0, chgDt, tran_mode);

                        if ("F".Equals(tran_mode) || "A".Equals(tran_mode) || "L".Equals(tran_mode))
                            CheckCode(msg, "CARRIER", "CARRIER", ei0, codeDt);

                        //if (!"F".Equals(tran_mode))
                        //    CheckParty(msg, "CARRIER", ei0, partyDt);
                        if (!"T".Equals(tran_mode))
                            CheckCode(msg, "UB", "PUNIT", ei0, codeDt);
                        else
                        {
                            if (string.IsNullOrEmpty(ei0.Get("PUNIT")))
                                ei0.Put("PUNIT", @Resources.Locale.L_QTManageController_Controllers_168);

                        }

                        if ("E".Equals(tran_mode))
                        {
                            if (!SetCountry(msg, cnDt, ei0, "REGION_NM"))
                            {
                                if (!SetCountry(msg, cnDt, ei0, "REGION_EN"))
                                {
                                    if (!string.IsNullOrEmpty(ei0.Get("REGION")))
                                    {
                                        string msg0 = string.Format(@Resources.Locale.L_QTManageController_Controllers_240, ei0.Get("REGION_NM"), ei0.Get("REGION_EN"));
                                        if (msg != null && !msg.Contains(msg0))
                                            msg.Add(msg0);
                                    }
                                }
                            }
                        }
                        else
                            SetREGION(msg, codeDt, ei0, "REGION_NM");
                        //if (!SetREGION(msg, codeDt, ei0, "REGION_NM"))
                        //    SetREGION(msg, codeDt, ei0, "REGION_EN");
                        if (!SetState(msg, ei0, stateDt, "STATE"))
                            SetState(msg, ei0, stateDt);
                        ei0.Remove("REGION_NM");
                        ei0.Remove("STATE_NM");
                        ei0.Remove("REGION_EN");
                    }
                    OtherCharge(parm, excelFileName, msg, ml);
                    if (msg.Count <= 0)
                    {
                        int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        isSucc = true;
                        if ("F".Equals(tran_mode) && !string.IsNullOrEmpty(rfq_no) && !string.IsNullOrEmpty(lsp_cd))
                        {
                            string deleteSameData = string.Format("DELETE FROM SMQTD WHERE U_ID NOT IN (SELECT MAX(U_ID) FROM SMQTD WHERE RFQ_NO={0} AND  LSP_CD={1} GROUP BY RFQ_NO,REGION,STATE,POD_CD,POL_CD,LOADING_FROM,LOADING_TO,F2,F3,F4,CARRIER,LSP_CD) AND RFQ_NO={0} AND LSP_CD={1}", SQLUtils.QuotedStr(rfq_no), SQLUtils.QuotedStr(lsp_cd));
                            OperationUtils.ExecuteUpdate(deleteSameData, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                    msg.Clear();
                    //return Json(new { message = ex.Message });
                }
                try
                {
                    if (isSucc)
                        EDOCController.UploadFile2EDOC(jobNo, excelFileName, groupId, cmp, stn, UserId, "BID_INV");
                    if (System.IO.File.Exists(excelFileName))
                        System.IO.File.Delete(excelFileName);
                }
                catch { }

                if (msg.Count > 0)
                    returnMessage = string.Join("", msg);
                if (returnMessage.Length >= 200)
                    returnMessage = returnMessage.Substring(0, 200) + ".......";
                Response.Write("<script type=\"text/javascript\">parent.CallBack(\"" + returnMessage + "\")</script>");
            }

            ViewBag.MenuBar = false;
            return View();
        }


        private void OtherCharge(Dictionary<string, object> parm, string excelFileName, List<string> msg, MixedList ml)
        {
            string tranMode = Prolink.Math.GetValueAsString(parm["TRAN_MODE"]);
            if (!"E".Equals(tranMode)) return;
            string mapping = "OtherQuotE";
            string sql = string.Format(@"SELECT CHG_CD,CHG_DESCP FROM SMCHG WHERE IO_TYPE = 'I' AND TRAN_MODE='E' ");
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            parm["smchgDt"] = dt;
            parm["msg"] = msg;
            ExcelParser.RegisterEditInstructFunc(mapping, HandleOtherQuotDetail);
            ExcelParser ep = new ExcelParser();
            ep.Save(mapping, excelFileName, ml, parm, 0, 1);
        }

        public static string HandleOtherQuotDetail(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string result = string.Empty;
            List<string> msg = (List<string>)parm["msg"];
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("U_FID", Prolink.Math.GetValueAsString(parm["U_FID"]));
            ei.Put("RFQ_NO", Prolink.Math.GetValueAsString(parm["RFQ_NO"]));
            ei.Put("QUOT_NO", Prolink.Math.GetValueAsString(parm["QUOT_NO"]));
            ei.Put("QUOT_TYPE", Prolink.Math.GetValueAsString(parm["QUOT_TYPE"]));
            ei.Put("TRAN_MODE", Prolink.Math.GetValueAsString(parm["TRAN_MODE"]));
            ei.Put("OUT_IN", Prolink.Math.GetValueAsString(parm["OUT_IN"]));
            ei.Put("LSP_CD", Prolink.Math.GetValueAsString(parm["LSP_CD"]));
            ei.Put("CUR", Prolink.Math.GetValueAsString(parm["CUR"]));

            DataTable dt = (DataTable)parm["smchgDt"];
            if (dt.Rows.Count <= 0)
            {
                result = @Resources.Locale.L_QTManageController_OtherChargesNoExists;
                msg.Add(result);
            }
            string chgCd = ei.Get("CHG_CD");
            DataRow[] rows = dt.Select(string.Format(@"CHG_CD={0}", SQLUtils.QuotedStr(chgCd)));
            if (rows.Length <= 0)
            {
                result = string.Format(@Resources.Locale.L_QTManageController_ChargeCdNoExists, chgCd);
                msg.Add(result);
            }
            ei.Put("CHG_DESCP", rows[0]["CHG_DESCP"]);
            string cmp = parm["CMP"].ToString();
            string region = ei.Get("REGION");
            if (!string.IsNullOrEmpty(region))
            {
                string sql = string.Format(@"SELECT * FROM BSCNTY WHERE GROUP_ID='TPV' AND (CMP={0} OR CMP='*') AND CNTRY_CD={1}",
                    SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(region));
                DataTable bscntyDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (bscntyDt.Rows.Count <= 0)
                {
                    result = string.Format(@Resources.Locale.L_QTManageController_CntryCdNoExists, region);
                    msg.Add(result);
                }
                else
                {
                    ei.Put("REGION_NAME", bscntyDt.Rows[0]["CNTRY_NM"]);
                }
            }

            string punit = ei.Get("PUNIT");
            string F1 = ei.Get("F1");
            string addFsc = ei.Get("ADD_FSC");
            string mappingPunit = "";
            switch (punit)
            {
                case "CBM":
                    mappingPunit = "CBM";
                    break;
                case "Shipment":
                    mappingPunit = "SHT";
                    break;
                case "Truck":
                    mappingPunit = "TRK";
                    break;
                case "CARTON":
                    mappingPunit = "CTN";
                    break;
                case "CONTAINER":
                    mappingPunit = "CTR";
                    break;
                case "PALLET":
                    mappingPunit = "PLT";
                    break;
                case "Weight or Measurement":
                    mappingPunit = "WM";
                    break;
                case "At Cost":
                    mappingPunit = "AC";
                    break;
                case "KILOGRAMS":
                    mappingPunit = "KGS";
                    break;
                case "TONNE (1000KG)":
                    mappingPunit = "TNE";
                    break;
                case "BL":
                    mappingPunit = "BL";
                    break;
                case "Document":
                    mappingPunit = "DOC";
                    break;
                case "Handling":
                    mappingPunit = "HAD";
                    break;
                case "Day":
                    mappingPunit = "DAY";
                    break;
                case "Hour":
                    mappingPunit = "HR";
                    break;
                case "SET":
                    mappingPunit = "SET";
                    break;
                case "Pieces":
                    mappingPunit = "PCS";
                    break;
                case "Packages":
                    mappingPunit = "PKG";
                    break;
                case "Bill of Entry":
                    mappingPunit = "BLE";
                    break;
                case "Charge Weight":
                    mappingPunit = "CW";
                    break;
                case "%":
                    mappingPunit = "%";
                    break;
                case "DN":
                    mappingPunit = "DN";
                    break;
                case "17米":
                    mappingPunit = "T17";
                    break;
                case "整车":
                    mappingPunit = "TF";
                    break;
                case "4.2米":
                    mappingPunit = "T4_2";
                    break;
                case "7.2米":
                    mappingPunit = "T7_2";
                    break;
                case "9.6米":
                    mappingPunit = "T9_6";
                    break;
                case "16米":
                    mappingPunit = "T16";
                    break;
                case "Value%":
                    mappingPunit = "Value%";
                    break;
                default:
                    mappingPunit = punit;
                    break;
            }

            ei.Put("PUNIT", mappingPunit);
            if (string.IsNullOrEmpty(punit))
            {
                result = "其他费用代码：" + chgCd + "，计费单位不可为空\r\n";
                msg.Add(result);
            }
            if (string.IsNullOrEmpty(F1))
            {
                result = "其他费用代码：" + chgCd + "，单价不可为空\r\n";
                msg.Add(result);
            }
            if (string.IsNullOrEmpty(addFsc))
            {
                result = "其他费用代码：" + chgCd + "，是否含燃油附加费不可为空\r\n";
                msg.Add(result);
            }
            return string.Empty;
        }


        public ActionResult ToMyContent(object obj)
        {
            JavaScriptSerializer jsSeri = new JavaScriptSerializer();
            jsSeri.MaxJsonLength = 1024 * 1024 * 50;
            string str = jsSeri.Serialize(obj);
            ContentResult result = new ContentResult();
            result.Content = str;
            return result;
        }

        #region 报价上传数据验证
        /// <summary>
        /// 设置地区
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="codeDt"></param>
        /// <param name="ei0"></param>
        private static bool SetREGION(List<string> sb, DataTable codeDt, EditInstruct ei0, string rnField = "REGION_NM")
        {
            DataRow[] drs = null;
            string val = ei0.Get("REGION");
            bool up = false;
            if (!string.IsNullOrEmpty(val))
            {
                val = val.ToUpper();
                drs = codeDt.Select(string.Format("CD={0} AND CD_TYPE={1}", SQLUtils.QuotedStr(val), SQLUtils.QuotedStr("TRGN")));
                if (drs.Length > 0)
                {
                    up = true;
                    ei0.Put("REGION", val);
                    return up;
                }
                else
                {
                    string msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_170, val);
                    if (sb != null && !sb.Contains(msg))
                        sb.Add(msg);
                }

            }

            val = ei0.Get(rnField);
            if (string.IsNullOrEmpty(val))
                return false;
            //val = val.ToUpper();
            drs = codeDt.Select(string.Format("(CD={0} OR CD_DESCP LIKE {1}) AND CD_TYPE={2}", SQLUtils.QuotedStr(val.ToUpper()), SQLUtils.QuotedStr("%" + val + "%"), SQLUtils.QuotedStr("TRGN")));

            foreach (DataRow dr in drs)
            {
                string code = Prolink.Math.GetValueAsString(dr["CD"]);
                string cd_descp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                if (val.Equals(code))
                {
                    up = true;
                    ei0.Put("REGION", code);
                    break;
                }
            }
            if (up) return up;
            foreach (DataRow dr in drs)
            {
                string code = Prolink.Math.GetValueAsString(dr["CD"]);
                string cd_descp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                if (cd_descp.Contains(val))
                {
                    ei0.Put("REGION", code);
                    string msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_171, cd_descp, code);
                    if (sb != null && !sb.Contains(msg))
                        sb.Add(msg);
                    break;
                }
            }
            return up;
        }

        /// <summary>
        /// 港口代码检查
        /// </summary>
        /// <param name="SB"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private static string GetPort(string val, List<string> portC_list, string tran_mode)
        {
            if (string.IsNullOrEmpty(val))
                return val;
            val = val.ToUpper();
            string sql = "";

            if ("D".Equals(tran_mode) || "T".Equals(tran_mode))
                sql = string.Format("SELECT PORT_CD,CNTRY_CD,[STATE],REGION,PORT_NM FROM BSTPORT WHERE PORT_CD={0}", SQLUtils.QuotedStr(val));
            else
            {
                if (val.Length != 5)
                {
                    return val;
                    //sb.Append(string.Format("{0}不是有效的港口代码;", val));
                }
                sql = string.Format("SELECT PORT_CD,CNTRY_CD,[STATE],REGION,PORT_NM FROM BSCITY WHERE CNTRY_CD={0} AND PORT_CD={1}", SQLUtils.QuotedStr(val.Substring(0, 2)), SQLUtils.QuotedStr(val.Substring(2, 3)));
            }

            if (!portC_list.Contains(sql))
                portC_list.Add(sql);
            return val;
        }

        private static string GetChg(string val, List<string> chg_list, string tran_mode)
        {
            if (string.IsNullOrEmpty(val))
                return val;
            val = val.ToUpper();
            string sql = string.Format("SELECT TRAN_MODE,IO_TYPE,CHG_TYPE,CHG_CD,CHG_DESCP FROM SMCHG WHERE CHG_CD={0}", SQLUtils.QuotedStr(val));
            if (!chg_list.Contains(sql))
                chg_list.Add(sql);
            return val;
        }

        /// <summary>
        /// 港口代码检查
        /// </summary>
        /// <param name="SB"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private static string GetParty(string val, List<string> party_list)
        {
            if (string.IsNullOrEmpty(val))
                return val;
            val = val.ToUpper();
            string sql = string.Format("SELECT PARTY_NO FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(val));
            if (!party_list.Contains(sql))
                party_list.Add(sql);
            return val;
        }

        private static string CheckCode(List<string> sb, string type, string name, EditInstruct ei, DataTable codeDt)
        {
            string val = ei.Get(name);
            return CheckCode(sb, type, codeDt, val);
        }

        private static string CheckCode(List<string> sb, string type, DataTable codeDt, string val)
        {
            if (string.IsNullOrEmpty(val))
                return val;
            if (!"CARRIER".Equals(type))
                val = val.ToUpper();
            //CD,CD_DESCP,CD_TYPE
            string filter = string.Format("CD={0} AND CD_TYPE={1}", SQLUtils.QuotedStr(val), SQLUtils.QuotedStr(type));
            if ("CARRIER".Equals(type))
                filter = string.Format("CD={0} AND CD_TYPE IN ('TCAR','TALN')", SQLUtils.QuotedStr(val));
            DataRow[] drs = codeDt.Select(filter);
            if (drs.Length <= 0)
            {
                string msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_172, val, type);
                if ("CARRIER".Equals(type))
                    msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_173, val);

                if (sb != null && !sb.Contains(msg))
                    sb.Add(msg);
            }
            return val;
        }

        private static string GetCode(StringBuilder sb, string type, DataTable codeDt, string val)
        {
            if (string.IsNullOrEmpty(val))
                return val;
            val = val.ToUpper();
            string filter = string.Format("CD={0} AND CD_TYPE={1}", SQLUtils.QuotedStr(val), SQLUtils.QuotedStr(type));
            DataRow[] drs = codeDt.Select(filter);
            if (drs.Length > 0)
            {
                return val;
            }
            return string.Empty;
        }

        private string CheckCur(List<string> sb, string name, EditInstruct ei, DataTable curDt, string cur)
        {
            string val = ei.Get(name);
            if (string.IsNullOrEmpty(val))
                return val;
            if (curDt == null)
                return val;
            val = val.ToUpper();
            if (!string.IsNullOrEmpty(cur) && cur.Equals(val))
            {
                return val;
            }
            //CD,CD_DESCP,CD_TYPE
            string filter = string.Format("CUR={0}", SQLUtils.QuotedStr(val));
            DataRow[] drs = curDt.Select(filter);
            if (drs.Length <= 0)
            {
                string msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_174, val);
                if (sb != null && !sb.Contains(msg))
                    sb.Add(msg);
            }
            return val;
        }

        private string CheckParty(List<string> sb, string name, EditInstruct ei, DataTable partyDt)
        {
            string val = ei.Get(name);
            if (string.IsNullOrEmpty(val))
                return val;
            if (partyDt == null)
                return val;
            val = val.ToUpper();
            //CD,CD_DESCP,CD_TYPE
            string filter = string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(val));
            DataRow[] drs = partyDt.Select(filter);
            if (drs.Length <= 0)
            {
                string msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_175, val);
                if (sb != null && !sb.Contains(msg))
                    sb.Add(msg);
            }
            return val;
        }

        private static string CheckPort(List<string> sb, string name, EditInstruct ei, DataTable portDt, string tran_mode)
        {
            string desp = @Resources.Locale.L_FCLFSetup_PodCd;
            if ("D".Equals(tran_mode) || "T".Equals(tran_mode))
                desp = @Resources.Locale.L_QTManage_Controllers_405;
            string msg = string.Empty;
            string val = ei.Get(name + "_CD");
            if (string.IsNullOrEmpty(val))
                return val;
            if (portDt == null)
                return val;
            val = val.ToUpper();
            if (!"D".Equals(tran_mode) && !"T".Equals(tran_mode))
            {
                if (val.Length != 5)
                {
                    msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_176, val, desp);
                    if (sb != null && !sb.Contains(msg))
                        sb.Add(msg);
                    return val;
                }
            }
            string filter = string.Empty;
            if ("D".Equals(tran_mode) || "T".Equals(tran_mode))
                filter = string.Format("PORT_CD={0}", SQLUtils.QuotedStr(val));
            else
                filter = string.Format("CNTRY_CD={0} AND PORT_CD={1}", SQLUtils.QuotedStr(val.Substring(0, 2)), SQLUtils.QuotedStr(val.Substring(2, 3)));
            DataRow[] drs = portDt.Select(filter);
            if (drs.Length > 0)
            {
                ei.Put(name + "_NM", Prolink.Math.GetValueAsString(drs[0]["PORT_NM"]));
            }
            else
            {
                drs = portDt.Select(string.Format("PORT_NM LIKE {0}", SQLUtils.QuotedStr("%" + val + "%")));
                msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_177, val, desp);
                if (drs.Length > 0)
                {
                    foreach (DataRow dr in drs)
                    {
                        msg += string.Format(@Resources.Locale.L_QTManageController_Controllers_178, Prolink.Math.GetValueAsString(dr["CNTRY_CD"]) + Prolink.Math.GetValueAsString(dr["PORT_CD"]), Prolink.Math.GetValueAsString(dr["PORT_NM"]));
                    }
                }

                if (sb != null && !sb.Contains(msg))
                {
                    sb.Add(msg);
                }
            }
            return val;
        }

        private static string CheckChg(List<string> sb, string name, EditInstruct ei, DataTable chgDt, string tran_mode)
        {
            string msg = string.Empty;
            string val = ei.Get("CHG_CD");
            if (string.IsNullOrEmpty(val))
                return val;
            if (chgDt == null)
                return val;
            if ("FRT".Equals(val.ToUpper()))
                return "FRT";
            string filter = string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(val.ToUpper()), SQLUtils.QuotedStr(tran_mode));
            DataRow[] drs = chgDt.Select(filter);
            if (drs.Length <= 0)
                drs = chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE='O'", SQLUtils.QuotedStr(val.ToUpper())));
            if (drs.Length > 0)
            {
                ei.Put("CHG_DESCP", Prolink.Math.GetValueAsString(drs[0]["CHG_DESCP"]));
            }
            else
            {
                drs = chgDt.Select(string.Format("CHG_DESCP LIKE {0}", SQLUtils.QuotedStr("%" + val + "%")));
                msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_179, val);
                if (drs.Length > 0)
                {
                    foreach (DataRow dr in drs)
                    {
                        msg += string.Format(@Resources.Locale.L_QTManageController_Controllers_178, Prolink.Math.GetValueAsString(dr["CHG_CD"]), Prolink.Math.GetValueAsString(dr["CHG_DESCP"]));
                    }
                }

                if (sb != null && !sb.Contains(msg))
                {
                    sb.Add(msg);
                }
            }
            return val;
        }

        public Dictionary<string, List<string>> GetCodeSelectData(string types, string groupId,string filter="")
        {
            string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE GROUP_ID={0} AND (CMP={1} OR CMP ='*') AND CD_TYPE IN({2}){3}", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(this.BaseCompanyId), types, string.IsNullOrEmpty(filter) ? string.Empty : " " + filter);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string option = null;
            Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();
            List<string> list = null;
            string cd, cdDescp, cdType;
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
                cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]).Trim();
                option = string.Empty;
                if (cd.Equals(cdDescp))
                    option = cd + ":" + cdDescp;
                else
                {
                    option = cd + ":" + cd + "." + cdDescp;
                }
                if (!options.ContainsKey(cdType))
                    options[cdType] = new List<string>();
                list = options[cdType] as List<string>;
                list.Add(option);
            }
            return options;
        }

        public static List<string> GetStateSelectData(string groupId)
        {
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT DISTINCT STATE_CD,STATE_NM,REGION_CD,REGION_NM FROM BSSTATE WHERE GROUP_ID={0} AND CNTRY_CD='CN'", SQLUtils.QuotedStr(groupId)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string option = null;
            Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();
            List<string> list = new List<string>();
            string cd, cdDescp, cdType;
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["STATE_CD"]).Trim();
                cdDescp = Prolink.Math.GetValueAsString(dr["STATE_NM"]).Trim();
                option = string.Empty;
                if (cd.Equals(cdDescp))
                    option = cd + ":" + cdDescp;
                else
                {
                    option = cd + ":" + cd + "." + cdDescp;
                }
                list.Add(option);
            }
            return list;
        }

        public static bool SetState(List<string> sb, EditInstruct ei, DataTable stateDt, string checkName = "STATE_NM")
        {
            string val = ei.Get(checkName);
            if (string.IsNullOrEmpty(val))
            {
                if ("STATE".Equals(checkName))
                    val = ei.Get("STATE_NM");
                if (string.IsNullOrEmpty(val))
                    return true;
                else
                    return false;
            }
            //val = val.ToUpper();
            string filter = string.Format("STATE_CD={0} OR STATE_NM LIKE {1}", SQLUtils.QuotedStr(val.ToUpper()), SQLUtils.QuotedStr("%" + val + "%"));
            bool up = false;
            DataRow[] drs = stateDt.Select(filter);
            foreach (DataRow dr in drs)
            {
                string code = Prolink.Math.GetValueAsString(dr["STATE_CD"]);
                string cd_descp = Prolink.Math.GetValueAsString(dr["STATE_NM"]);
                if (val.ToUpper().Equals(code))
                {
                    up = true;
                    ei.Put("STATE", code);
                    break;
                }
            }
            if (up) return up;
            string msg = string.Empty;
            foreach (DataRow dr in drs)
            {
                string code = Prolink.Math.GetValueAsString(dr["STATE_CD"]);
                string cd_descp = Prolink.Math.GetValueAsString(dr["STATE_NM"]);
                if (cd_descp.Contains(val))
                {
                    ei.Put("STATE", code);
                    msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_171, cd_descp, code);
                    if (sb != null && !sb.Contains(msg))
                        sb.Add(msg);
                    break;
                }
            }
            if (up) return up;
            msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_180, val);
            if (sb != null && !sb.Contains(msg))
                sb.Add(msg);
            return up;
        }

        private static bool SetCountry(List<string> sb, DataTable cnDt, EditInstruct ei0, string rnField = "REGION_NM", string cnField = "REGION")
        {
            DataRow[] drs = null;
            string val = ei0.Get(cnField);
            bool up = false;
            if (!string.IsNullOrEmpty(val))
            {
                val = val.ToUpper();
                drs = cnDt.Select(string.Format("CNTRY_CD={0}", SQLUtils.QuotedStr(val)));
                if (drs.Length > 0)
                {
                    up = true;
                    ei0.Put(cnField, val);
                    return up;
                }
            }

            val = ei0.Get(rnField);
            if (string.IsNullOrEmpty(val))
                return false;
            //val = val.ToUpper();
            drs = cnDt.Select(string.Format("CNTRY_NM LIKE {0}", SQLUtils.QuotedStr("%" + val + "%")));

            foreach (DataRow dr in drs)
            {
                string code = Prolink.Math.GetValueAsString(dr["CNTRY_CD"]);
                string cd_descp = Prolink.Math.GetValueAsString(dr["CNTRY_NM"]);
                if (val.Equals(code))
                {
                    up = true;
                    ei0.Put(cnField, code);
                    break;
                }
            }
            if (up) return up;
            foreach (DataRow dr in drs)
            {
                string code = Prolink.Math.GetValueAsString(dr["CNTRY_CD"]);
                string cd_descp = Prolink.Math.GetValueAsString(dr["CNTRY_NM"]);
                if (cd_descp.Contains(val))
                {
                    ei0.Put(cnField, code);
                    string msg = string.Format(@Resources.Locale.L_QTManageController_Controllers_171, cd_descp, code);
                    if (sb != null && !sb.Contains(msg))
                        sb.Add(msg);
                    break;
                }
            }
            return up;
        }
        #endregion

        #region 运输管理
        public ActionResult GetQTDetailData()
        {
            string condition = "1=1";
            string condition1 = "";
            string rlocation = Request["rlocation"];
            string incoterm = Request["incoterm"];
            if (!string.IsNullOrEmpty(rlocation))
            {
                condition1 += " AND RLOCATION=" + SQLUtils.QuotedStr(rlocation);
            }
            if (!string.IsNullOrEmpty(incoterm))
            {
                condition1 += string.Format(" AND INCOTERM LIKE'%{0}%'", incoterm);
            }
            string tranMode = GetUrlDecodeValue(Request["TranMode"]);
            string pol = GetUrlDecodeValue(Request["pol"]).ToUpper();
            string pod = GetUrlDecodeValue(Request["pod"]).ToUpper();
            string region = GetUrlDecodeValue(Request["region"]);
            string[] regions = region.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string[] pols = pol.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string[] pods = pod.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            pols = GetPols(pols, CompanyId);

            string rateFilter = string.Empty;
            string etd = Request["etd"];
            string bu_condition = string.Empty;
            if (!string.IsNullOrEmpty(etd))
            {
                DateTime weekDay = Prolink.Utils.FormatUtils.ParseDateTime(etd.Replace("/", "").Replace(":", "").Replace("-", "").Replace(" ", ""), "yyyyMMdd");
                condition1 += string.Format(" AND SMRQM.EFFECT_FROM<={0} AND SMRQM.EFFECT_TO>={0}", SQLUtils.QuotedStr(weekDay.ToString("yyyy-MM-dd")));
                rateFilter = string.Format(" WHERE EDATE<={0}", SQLUtils.QuotedStr(weekDay.ToString("yyyy-MM-dd")));
                bu_condition = string.Format("SMFSC.EFFECT_DATE<={0}", SQLUtils.QuotedStr(weekDay.ToString("yyyy-MM-dd")));
            }
            else
            {
                rateFilter = string.Format(" WHERE EDATE<={0}", SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd")));
                bu_condition = string.Format("SMFSC.EFFECT_DATE<={0}", SQLUtils.QuotedStr(DateTime.Now.ToString("yyyy-MM-dd")));
            }

            if (!string.IsNullOrEmpty(condition1))
                condition = "EXISTS (SELECT RFQ_NO FROM SMRQM WHERE SMRQM.RFQ_NO = SMQTD.RFQ_NO" + condition1 + ")";

            condition += " AND QUOT_TYPE='F'";
            if (pols.Length > 0)
                condition += " AND POL_CD IN " + JoinString(pols);
            if ("E".Equals(tranMode))
            {
                if (regions.Length > 0)
                    condition += " AND REGION IN " + JoinString(regions);
            }
            else if (pods.Length > 0)
                condition += " AND POD_CD IN " + JoinString(pods);

            //DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMQTD WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string tranType = GetUrlDecodeValue(Request["TranType"]);
            if ("T".Equals(tranMode))
            {
                if (!string.IsNullOrEmpty(tranType))
                {
                    condition += " AND TRAN_TYPE=" + SQLUtils.QuotedStr(tranType);
                }
            }

            string table = string.Format("(SELECT * FROM (SELECT * FROM SMQTD WHERE {0}) A OUTER APPLY (SELECT TOP 1 QUOT_NO AS QUOT_NO1,LSP_NM AS LSP_NM1,EFFECT_FROM AS EFFECT_FROM1,EFFECT_TO AS EFFECT_TO1 FROM SMQTM M WITH (NOLOCK) WHERE M.U_ID = A.U_FID) B) T", condition);
            if ("L".Equals(tranMode))
                table = string.Format("(SELECT TRAN_MODE,RFQ_NO,QUOT_NO1 AS QUOT_NO,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1,U_FID FROM (SELECT * FROM SMQTD WHERE {0}) A OUTER APPLY (SELECT TOP 1 QUOT_NO AS QUOT_NO1,LSP_NM AS LSP_NM1,EFFECT_FROM AS EFFECT_FROM1,EFFECT_TO AS EFFECT_TO1 FROM SMQTM M WITH (NOLOCK) WHERE M.U_ID = A.U_FID ) B  GROUP BY TRAN_MODE,RFQ_NO,QUOT_NO1,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1,U_FID)T", condition);
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string baseCondition = string.Empty;


            switch (tranMode)
            {
                case "E":
                case "D":
                    baseCondition = "CHG_CD='FRT'";
                    break;
            }

            DataTable buDt = null;
            if ("F".Equals(tranMode))
                buDt = OperationUtils.GetDataTable(string.Format("SELECT SMFSC.EFFECT_DATE,SMFSC.GP20,SMFSC.GP40,SMFSC.HQ40,SMFSC.OTH_RATE,SMFSC.CARRIER_NM,SMFSC.CMP_NM,BSCAA.* FROM SMFSC LEFT JOIN BSCAA ON SMFSC.U_ID=BSCAA.U_FID WHERE {0} ORDER BY  EFFECT_DATE DESC", bu_condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dt = ModelFactory.InquiryData("*", table, baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT * FROM SMQTD WHERE {0}) A OUTER APPLY (SELECT TOP 1 QUOT_NO FROM SMQTM M WITH (NOLOCK) WHERE M.U_ID = A.U_ID) B", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable othDt = null;
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm["Cnt20"] = Request["Cnt20"];
            parm["Cnt40"] = Request["Cnt40"];
            parm["Cnt40hq"] = Request["Cnt40hq"];
            parm["cw"] = Request["cw"];
            parm["gw"] = Request["gw"];
            parm["cbm"] = Request["cbm"];
            parm["carType"] = Request["carType"];
            parm["car_cw"] = Request["car_cw"];
            parm["cnt"] = Request["cnt"];
            parm["TranMode"] = tranMode;
            parm["gwu"] = Request["gwu"];
            parm["DnNum"] = 1;
            parm["TrackWay"] = string.Empty;
            parm["CntNum"] = Helper.GetDecimalValue(parm["Cnt20"]) + Helper.GetDecimalValue(parm["Cnt40"]) + Helper.GetDecimalValue(parm["Cnt40hq"]);

            DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE {0} ORDER BY EDATE", rateFilter), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            Bill bill = new Bill(false);
            bill.FreightCalculat(parm, dt, othDt, rateDt, new List<string> { }, buDt);

            #region 作废
            //List<string> jobNoList = new List<string>();
            //switch (tranMode)
            //{
            //    case "F":
            //        othDt = OperationUtils.GetDataTable("SELECT * FROM SMQTD WHERE TRAN_MODE='O'" + ((pols.Length > 0) ? (" AND POL_CD IN " + JoinString(pols)) : ""), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            //        FclFreight(parm, dt, othDt, rateDt, buDt);
            //        break;
            //    case "T":
            //        TruckFreight(parm, dt, null, rateDt);
            //        break;
            //    case "A":
            //        AirFreight(parm, dt, null, rateDt);
            //        break;
            //    case "D":
            //        InlandExpressFreight(parm, dt, null, rateDt);
            //        break;
            //    case "L":
            //        foreach (DataRow qt in dt.Rows)
            //        {
            //            string temp = Prolink.Math.GetValueAsString(qt["U_FID"]);
            //            if (!jobNoList.Contains(temp))
            //                jobNoList.Add(temp);
            //        }
            //        if (jobNoList.Count > 0)
            //        {
            //            othDt = OperationUtils.GetDataTable("SELECT * FROM SMQTD WHERE U_FID IN " + JoinString(jobNoList.ToArray()), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            //            LCLFreight(parm, dt, othDt, rateDt);
            //        }
            //        break;
            //    case "E":
            //        foreach (DataRow qt in dt.Rows)
            //        {
            //            string temp = Prolink.Math.GetValueAsString(qt["U_ID"]);
            //            if (!jobNoList.Contains(temp))
            //                jobNoList.Add(temp);
            //        }
            //        if (jobNoList.Count > 0)
            //        {
            //            othDt = OperationUtils.GetDataTable("SELECT * FROM SMQTD WHERE CHG_CD<>'FRT' AND U_FID IN " + JoinString(jobNoList.ToArray()), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            //            ExpressFreight(parm, dt, othDt, rateDt);
            //        }
            //        break;
            //}
            #endregion

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

        private void LCLFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            dt.Columns.Add("CHG_REMARK", typeof(string));
            dt.Columns["CHG_REMARK"].MaxLength = 9999;

            dt.Columns.Add("LOCALE_AMT", typeof(decimal));
            string gwu = Prolink.Math.GetValueAsString(parm["gwu"]);
            //decimal cw = GetDecimalValue(parm["cw"]);
            decimal gw = GetDecimalValue(parm["gw"]);
            decimal cbm = GetDecimalValue(parm["cbm"]);
            decimal cnt = GetDecimalValue(parm["cnt"]);
            DataRow[] drs = null;
            foreach (DataRow dr in dt.Rows)
            {
                string pol_cd = Prolink.Math.GetValueAsString(dr["POL_CD"]);
                string pod_cd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                string u_fid = Prolink.Math.GetValueAsString(dr["U_FID"]);

                if (string.IsNullOrEmpty(pol_cd))
                    continue;
                List<string> msg = new List<string>();
                //string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO1"]);//报价号码
                decimal total = 0M;
                bool error = false;
                //string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                drs = othDt.Select(string.Format("POL_CD={0} AND POD_CD={1} AND U_FID={2}", SQLUtils.QuotedStr(pol_cd), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(u_fid)));
                Dictionary<string, decimal> otherChg = new Dictionary<string, decimal>();
                foreach (DataRow chg in drs)
                {
                    string cur1 = Prolink.Math.GetValueAsString(chg["CUR"]).ToUpper();
                    string chg_descp = Prolink.Math.GetValueAsString(chg["CHG_DESCP"]).ToUpper();
                    string punit = Prolink.Math.GetValueAsString(chg["PUNIT"]).ToUpper();
                    decimal min_amt = Prolink.Math.GetValueAsDecimal(chg["MIN_AMT"]);
                    decimal price = Prolink.Math.GetValueAsDecimal(chg["F1"]);

                    decimal qty = 0;
                    switch (punit)
                    {
                        case "BL":
                            qty = 1;
                            break;
                        case "CBM":
                            qty = cbm;
                            break;
                        case "CNT":
                            qty = cnt;
                            break;
                        case "PKG":
                        case "L":
                        case "K"://1千克(kg)=2.2046226磅(lb)
                            qty = gw;
                            if ("L".Equals(punit) || "K".Equals(gwu))
                            {
                                if (!msg.Contains(@Resources.Locale.L_QTManageController_Controllers_181))
                                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_181);
                                qty = gw * 2.2046226M;
                            }
                            else if ("K".Equals(punit) || "L".Equals(gwu))
                            {
                                if (!msg.Contains(@Resources.Locale.L_QTManageController_Controllers_182))
                                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_182);
                                qty = gw * 0.4535924M;
                            }
                            break;
                        default:
                            qty = 1;
                            break;
                    }
                    decimal val = price * qty;
                    if (val < min_amt)
                    {
                        val = min_amt;
                        msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_183, chg_descp, price, qty, min_amt, val, GetLoalCurName(cur1), punit));
                    }
                    else
                        msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_184, chg_descp, price, qty, min_amt, val, GetLoalCurName(cur1), punit));

                    if (!otherChg.ContainsKey(cur1))
                        otherChg[cur1] = val;
                    else
                        otherChg[cur1] = otherChg[cur1] + val;
                }
                if (otherChg.Count > 0)
                {
                    string othMsg = "";
                    foreach (var kv in otherChg)
                    {
                        if (othMsg.Length > 0)
                            othMsg += ",";
                        othMsg += kv.Value + kv.Key;
                        GetTotal(rateDt, msg, kv.Value, kv.Key, ref total, ref error);
                    }
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_185, othMsg));
                }
                if (error)
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_186);
                else
                {
                    dr["LOCALE_AMT"] = Get45CurValue(total);
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_187, Get45CurValue(total), GetLoalCurName("CNY")));
                }
                dr["CHG_REMARK"] = string.Join("；", msg);
            }
        }

        /// <summary>
        /// 空运试算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private void AirFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            decimal cw = GetDecimalValue(parm["gw"]);
            dt.Columns.Add("CHG_REMARK", typeof(string));
            dt.Columns["CHG_REMARK"].MaxLength = 9999;
            dt.Columns.Add("LOCALE_AMT", typeof(decimal));
            Dictionary<string, decimal> map = new Dictionary<string, decimal>();
            map["F1"] = -45M;
            map["F7"] = 2000M;
            map["F6"] = 1000M;
            map["F5"] = 500M;
            map["F4"] = 300M;
            map["F3"] = 100M;
            map["F2"] = 45M;
            foreach (DataRow dr in dt.Rows)
            {
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);//F2:20'  F3:40'  F4:40HQ
                if (string.IsNullOrEmpty(polCd))
                    continue;
                List<string> msg = new List<string>();
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);//报价号码
                decimal total = 0M;
                decimal curTotal = 0M;
                bool error = false;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                decimal min = Prolink.Math.GetValueAsDecimal(dr["MIN_AMT"]);

                decimal price = 0m;
                foreach (var kv in map)
                {
                    decimal val = kv.Value;
                    if (val < 0)
                    {
                        val = System.Math.Abs(val);
                        if (cw < val)
                        {

                            price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                            if (price != 0)
                            {
                                msg.Add(string.Format("{0}<{1}({2}{3})*{4}", cw, val, price, GetLoalCurName(cur), cw));
                                break;
                            }
                        }
                    }
                    else if (cw >= val)
                    {
                        price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                        if (price != 0)
                        {
                            msg.Add(string.Format("{0}>={1}({2}{3})*{4}", cw, val, price, GetLoalCurName(cur), cw));
                            break;
                        }
                    }
                }

                if (price <= 0)
                    msg.Add(@Resources.Locale.L_QTManage_Controllers_416);

                if (min > 0)
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_188, Get45CurValue(min), GetLoalCurName(cur)));

                curTotal = price * cw;
                if (curTotal < min)
                {
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_189, Get45CurValue(curTotal), GetLoalCurName(cur)));
                    curTotal = min;
                }
                GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_190, Get45CurValue(curTotal), GetLoalCurName(cur)));
                if (error)
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_186);
                else
                {
                    dr["LOCALE_AMT"] = Get45CurValue(total);
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_187, Get45CurValue(total), GetLoalCurName("CNY")));
                }
                dr["CHG_REMARK"] = string.Join("；", msg);
            }
        }

        /// <summary>
        /// FLC 运费试算
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="parm"></param>
        private void FclFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt, DataTable buDt = null)
        {
            decimal Cnt20 = GetDecimalValue(parm["Cnt20"]);
            decimal Cnt40 = GetDecimalValue(parm["Cnt40"]);
            decimal Cnt40hq = GetDecimalValue(parm["Cnt40hq"]);

            dt.Columns.Add("CHG_REMARK", typeof(string));
            dt.Columns["CHG_REMARK"].MaxLength = 9999;

            dt.Columns.Add("LOCALE_AMT", typeof(decimal));

            DataRow[] drs = null;
            foreach (DataRow dr in dt.Rows)
            {
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);//F2:20'  F3:40'  F4:40HQ
                if (string.IsNullOrEmpty(polCd))
                    continue;
                List<string> msg = new List<string>();
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);//报价号码
                decimal cnt = 0M;
                decimal total = 0M;
                bool error = false;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();

                decimal F12 = 0m, F13 = 0m, F14 = 0m;
                if (buDt != null && buDt.Rows.Count > 0)
                {
                    DataRow[] bus = buDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(polCd)), "EFFECT_DATE DESC");
                    if (bus.Length > 0)
                    {
                        F12 = Prolink.Math.GetValueAsDecimal(bus[0]["GP20"]);
                        F13 = Prolink.Math.GetValueAsDecimal(bus[0]["GP40"]);
                        F14 = Prolink.Math.GetValueAsDecimal(bus[0]["HQ40"]);
                    }

                }
                if (F12 <= 0)
                    F12 = Prolink.Math.GetValueAsDecimal(dr["F12"]);//20' BUC/BAF/FAF
                if (F13 <= 0)
                    F13 = Prolink.Math.GetValueAsDecimal(dr["F13"]);//40' BUC/BAF/FAF
                if (F14 <= 0)
                    F14 = Prolink.Math.GetValueAsDecimal(dr["F14"]);//40HQ BUC/BAF/FAF

                decimal F2 = Prolink.Math.GetValueAsDecimal(dr["F2"]);//20' 
                if (Cnt20 > 0 && F2 <= 0)
                {
                    error = true;
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_191);
                }
                else if (Cnt20 > 0 && F2 > 0)
                    msg.Add(string.Format("20'({0}+{1}{2})*{3}", F2, F12, GetLoalCurName(cur), Cnt20));

                decimal F3 = Prolink.Math.GetValueAsDecimal(dr["F3"]);//40'
                if (Cnt40 > 0 && F3 <= 0)
                {
                    error = true;
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_192);
                }
                else if (Cnt40 > 0 && F3 > 0)
                    msg.Add(string.Format("40'({0}+{1}{2})*{3}", F3, F13, GetLoalCurName(cur), Cnt40));

                decimal F4 = Prolink.Math.GetValueAsDecimal(dr["F4"]);//40HQ
                if (Cnt40hq > 0 && F4 <= 0)
                {
                    error = true;
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_193);
                }
                else if (Cnt40hq > 0 && F4 > 0)
                    msg.Add(string.Format("40HQ({0}+{1}{2})*{3}", F4, F14, GetLoalCurName(cur), Cnt40hq));

                cnt += Cnt20 * (F2 + F12);
                cnt += Cnt40 * (F3 + F13);
                cnt += Cnt40hq * (F4 + F14);

                drs = othDt.Select(string.Format("POL_CD={0}", SQLUtils.QuotedStr(polCd)));
                Dictionary<string, decimal> otherChg = new Dictionary<string, decimal>();
                foreach (DataRow chg in drs)
                {
                    string cur1 = Prolink.Math.GetValueAsString(chg["CUR"]).ToUpper();
                    decimal val = Prolink.Math.GetValueAsDecimal(chg["F3"]);
                    if (!otherChg.ContainsKey(cur1))
                        otherChg[cur1] = val;
                    else
                        otherChg[cur1] = otherChg[cur1] + val;
                }
                GetTotal(rateDt, msg, cnt, cur, ref total, ref error);
                if (otherChg.Count > 0)
                {
                    string othMsg = "";
                    foreach (var kv in otherChg)
                    {
                        if (othMsg.Length > 0)
                            othMsg += ",";
                        othMsg += kv.Value + kv.Key;
                    }
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_194, othMsg));
                    foreach (var kv in otherChg)
                    {
                        GetTotal(rateDt, msg, kv.Value, kv.Key, ref total, ref error);
                    }
                }
                msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_195, Get45CurValue(cnt), GetLoalCurName(cur)));
                if (error)
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_186);
                else
                {
                    dr["LOCALE_AMT"] = Get45CurValue(total);
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_187, Get45CurValue(total), GetLoalCurName("CNY")));
                }
                dr["CHG_REMARK"] = string.Join("；", msg);
            }
        }

        /// <summary>
        /// 国内快递
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private void InlandExpressFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            dt.Columns.Add("CHG_REMARK", typeof(string));
            dt.Columns["CHG_REMARK"].MaxLength = 9999;
            dt.Columns.Add("LOCALE_AMT", typeof(decimal));
            decimal cw = GetDecimalValue(parm["gw"]);

            foreach (DataRow dr in dt.Rows)
            {
                decimal total = 0m;
                decimal curTotal = 0m;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                bool error = false;

                List<string> msg = new List<string>();
                decimal first = 0.5m;
                decimal pre = 0.5m;
                decimal price = Prolink.Math.GetValueAsDecimal(dr["F1"]);//0.5 首重
                decimal price_more = Prolink.Math.GetValueAsDecimal(dr["F3"]);//0.5 继重
                if (price <= 0)
                {
                    price = Prolink.Math.GetValueAsDecimal(dr["F2"]);//1首重
                    first = 1M;
                }

                if (price_more <= 0)
                {
                    price_more = Prolink.Math.GetValueAsDecimal(dr["F4"]);//1 继重
                    if (price_more > 0)
                    {
                        pre = 1M;
                    }
                    else
                        price_more = price;
                }

                if (price <= 0)
                {
                    error = false;
                    msg.Add(@Resources.Locale.L_QTManage_Controllers_422);
                }
                if (price_more <= 0)
                {
                    error = false;
                    msg.Add(@Resources.Locale.L_QTManage_Controllers_423);
                }
                if (cw <= first)
                {
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_196, price, GetLoalCurName(cur), cw, first));
                    curTotal = price;
                }
                else
                {
                    decimal cw_mode = (cw - first) % pre;
                    if (cw_mode > 0) cw_mode = pre - cw_mode;
                    int cw_0 = (int)((cw - first + cw_mode) / pre);

                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_197, first, price, GetLoalCurName(cur)));// 
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_198, price_more, GetLoalCurName(cur), pre));// 
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_199, (cw - first + cw_mode), cw_0, price_more, GetLoalCurName(cur)));// 
                    curTotal = price + price_more * cw_0;
                }
                GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_190, Get45CurValue(curTotal), GetLoalCurName(cur)));
                if (error)
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_186);
                else if (curTotal != total)
                {
                    dr["LOCALE_AMT"] = Get45CurValue(total);
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_187, Get45CurValue(total), GetLoalCurName("CNY")));
                }
                dr["CHG_REMARK"] = string.Join("；", msg);
            }
        }

        /// <summary>
        /// 内陆运输
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private void TruckFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            dt.Columns.Add("CHG_REMARK", typeof(string));
            dt.Columns["CHG_REMARK"].MaxLength = 9999;
            dt.Columns.Add("LOCALE_AMT", typeof(decimal));
            decimal car_cw = GetDecimalValue(parm["car_cw"]);
            string gwu = Prolink.Math.GetValueAsString(parm["gwu"]);
            string carType = Prolink.Math.GetValueAsString(parm["carType"]);
            decimal gw = GetDecimalValue(parm["gw"]);
            decimal cbm = GetDecimalValue(parm["cbm"]);

            foreach (DataRow dr in dt.Rows)
            {
                List<string> msg = new List<string>();
                decimal price = 0m;
                if (dt.Columns.Contains(carType))
                    price = Prolink.Math.GetValueAsDecimal(dr[carType]);
                decimal total = 0m;
                decimal cbm_total = 0m;
                decimal gw_total = 0m;
                decimal curTotal = 0m;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                bool error = false;
                if (car_cw > 0)
                {
                    if (price <= 0)
                        msg.Add(@Resources.Locale.L_QTManage_Controllers_426);
                    else
                    {
                        curTotal = price * car_cw;
                        msg.Add(string.Format("({0}{1})*{2}", price, GetLoalCurName(cur), car_cw));
                    }
                }
                decimal cbm_price = Prolink.Math.GetValueAsDecimal(dr["F1"]);
                decimal gw_price = Prolink.Math.GetValueAsDecimal(dr["F2"]);
                if (cbm > 0 || gw > 0)
                {
                    cbm_total = cbm_price * cbm;
                    gw_total = gw_price * gw;
                    if (cbm_total > gw_total)
                    {
                        msg.Add(string.Format("CBM({0}*{1}){2}{3}", cbm_price, cbm, cbm_total, GetLoalCurName(cur))
                            + ">" + string.Format("GW({0}*{1}){2}{3}", gw_price, gw, gw_total, GetLoalCurName(cur)));
                        curTotal += cbm_total;
                    }
                    else
                    {
                        curTotal += gw_total;
                        msg.Add(string.Format("CBM({0}*{1}){2}{3}", cbm_price, cbm, cbm_total, GetLoalCurName(cur))
                     + "<=" + string.Format("GW({0}*{1}){2}{3}", gw_price, gw, gw_total, GetLoalCurName(cur)));
                    }
                }
                GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_190, Get45CurValue(curTotal), GetLoalCurName(cur)));
                if (error)
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_186);
                else if (curTotal != total)
                {
                    dr["LOCALE_AMT"] = Get45CurValue(total);
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_187, Get45CurValue(total), GetLoalCurName("CNY")));
                }
                dr["CHG_REMARK"] = string.Join("；", msg);
            }
        }

        /// <summary>
        /// 国际快递 运费试算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private void ExpressFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            decimal cw = GetDecimalValue(parm["gw"]);
            dt.Columns.Add("CHG_REMARK", typeof(string));
            dt.Columns["CHG_REMARK"].MaxLength = 9999;
            dt.Columns.Add("LOCALE_AMT", typeof(decimal));
            Dictionary<string, decimal> map = new Dictionary<string, decimal>();
            for (int i = 11; i <= 50; i++)
            {
                map["F" + i] = -(0.5m + (i - 11) * 0.5m);
            }
            map["F7"] = 300;
            map["F6"] = 200;
            map["F5"] = 100;
            map["F4"] = 50;
            map["F3"] = 40;
            map["F2"] = 30;
            map["F1"] = 20;

            foreach (DataRow dr in dt.Rows)
            {
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);//F2:20'  F3:40'  F4:40HQ
                if (string.IsNullOrEmpty(polCd))
                    continue;
                List<string> msg = new List<string>();
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);//报价号码
                decimal total = 0M;
                decimal curTotal = 0M;
                bool error = false;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                //decimal min = Prolink.Math.GetValueAsDecimal(dr["MIN_AMT"]);
                decimal price = 0m;
                foreach (var kv in map)
                {
                    decimal val = kv.Value;
                    if (val < 0)
                    {
                        val = System.Math.Abs(val);
                        if (cw < val)
                        {

                            price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                            if (price != 0)
                            {
                                msg.Add(string.Format("{0}<{1}({2}{3})*{4}", cw, val, price, GetLoalCurName(cur), cw));
                                break;
                            }
                        }
                    }
                    else if (cw >= val)
                    {
                        price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                        if (price != 0)
                        {
                            msg.Add(string.Format("{0}>={1}({2}{3})*{4}", cw, val, price, GetLoalCurName(cur), cw));
                            break;
                        }
                    }
                }

                if (price <= 0)
                    msg.Add(@Resources.Locale.L_QTManage_Controllers_416);

                curTotal = price * cw;
                GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_190, Get45CurValue(curTotal), GetLoalCurName(cur)));
                if (error)
                    msg.Add(@Resources.Locale.L_QTManageController_Controllers_186);
                else
                {
                    dr["LOCALE_AMT"] = Get45CurValue(total);
                    msg.Add(string.Format(@Resources.Locale.L_QTManageController_Controllers_187, Get45CurValue(total), GetLoalCurName("CNY")));
                }
                dr["CHG_REMARK"] = string.Join("；", msg);
            }
        }

        private static decimal Get45CurValue(decimal val)
        {
            return System.Math.Round(val, 2, MidpointRounding.AwayFromZero);
        }

        private static string GetLoalCurName(string cur)
        {
            if ("RMB".Equals(cur) || "CNY".Equals(cur))
                return "CNY";
            return cur;
        }

        private static void GetTotal(DataTable rateDt, List<string> msg, decimal val, string cur, ref decimal total, ref bool error)
        {
            if ("RMB".Equals(cur) || "CNY".Equals(cur))
            {
                total += val;
                return;
            }
            string msgStr = string.Empty;
            DataRow[] drs = rateDt.Select(string.Format("FCUR={0} AND (TCUR='RMB' OR TCUR='CNY')", SQLUtils.QuotedStr(cur)), "EDATE DESC");
            if (drs.Length <= 0)
            {
                error = true;
                msgStr = string.Format(@Resources.Locale.L_QTManageController_Controllers_200, cur);
                if (!msg.Contains(msgStr))
                    msg.Add(msgStr);
            }
            else
            {
                decimal rate = Prolink.Math.GetValueAsDecimal(drs[0]["EX_RATE"]);
                msgStr = string.Format(@Resources.Locale.L_QTManageController_Controllers_201, cur, Get45CurValue(rate));
                if (!msg.Contains(msgStr))
                    msg.Add(msgStr);
                total += rate * val;
            }
        }

        private static string[] GetPols(string[] pols, string cmp)
        {
            DataTable polCtity = OperationUtils.GetDataTable(string.Format("SELECT POL,POL_DESCP FROM BSLCPOL WHERE CMP={0}", SQLUtils.QuotedStr(cmp)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (polCtity != null && polCtity.Rows.Count > 0)
            {
                List<string> list = new List<string>();
                List<string> list1 = new List<string>();
                foreach (DataRow city in polCtity.Rows)
                {
                    string name = Prolink.Math.GetValueAsString(city["POL"]);
                    if (string.IsNullOrEmpty(name))
                        continue;
                    if (!list.Contains(name))
                        list.Add(name);
                }
                if (pols.Length <= 0)
                {
                    pols = list.ToArray();
                }
                else
                {
                    foreach (string name in pols)
                    {
                        if (string.IsNullOrEmpty(name))
                            continue;
                        if (list.Contains(name))
                            list1.Add(name);
                    }
                    pols = list1.ToArray();
                    if (pols.Length <= 0)
                    {
                        pols = new string[] { "XXXXXX" };
                    }
                }
            }
            return pols;
        }

        private static decimal GetDecimalValue(object val1)
        {
            string val = Prolink.Math.GetValueAsString(val1);
            if (string.IsNullOrEmpty(val))
                return 0M;
            decimal dvalue = 0m;
            if (decimal.TryParse(val, out dvalue))
                return dvalue;
            return 0M;
        }

        private static string JoinString(string[] vals)
        {
            string result = string.Empty;
            foreach (string val in vals)
            {
                if (result.Length > 0)
                    result += ",";
                result += SQLUtils.QuotedStr(val);
            }
            return "(" + result + ")";
        }

        private static string GetUrlDecodeValue(string val)
        {
            if (!string.IsNullOrEmpty(val))
                return HttpUtility.UrlDecode(val);
            else
                return string.Empty;
        }

        /// <summary>
        /// 获取FCL报价其他费用
        /// </summary>
        /// <returns></returns>
        public ActionResult GetQTSubFreightData()
        {
            string pol = Request["pol"];
            string pod = Request["pod"];
            string u_id = Request["UId"];
            string ufid = Request["UFid"];
            string tranMode = Request["tranMode"];
            string sql = string.Empty;
            switch (tranMode)
            {
                case "F":
                    sql = string.Format("SELECT * FROM SMQTD WHERE POL_CD={0} AND TRAN_MODE='O'", SQLUtils.QuotedStr(pol));
                    break;
                case "E":
                    sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND CHG_CD<>'FRT'", SQLUtils.QuotedStr(ufid));
                    break;
                case "L":
                    sql = string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND POL_CD={1} AND POD_CD={2}", SQLUtils.QuotedStr(ufid), SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(pod));
                    break;
            }


            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["chg"] = ModelFactory.ToTableJson(mainDt, "SmqtdModel");
            return ToContent(data);
        }

        public ActionResult GetQTPorts()
        {
            string table = string.Format("(SELECT POL AS PORT_CD,POL_DESCP AS PORT_NM FROM BSLCPOL WHERE CMP={0})T", SQLUtils.QuotedStr(CompanyId));
            string condition = string.Empty;
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            DataTable dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            if (dt.Rows.Count <= 0)
            {
                table = "(SELECT POL_CD AS PORT_CD,POL_NM AS PORT_NM FROM SMQTD WHERE POL_CD IS NOT NULL AND QUOT_TYPE='F' GROUP BY POL_CD,POL_NM UNION SELECT POD_CD AS PORT_CD,POD_NM AS PORT_NM FROM SMQTD WHERE POD_CD IS NOT NULL AND QUOT_TYPE='F' GROUP BY POD_CD,POD_NM)T";
                recordsCount = 0; pageIndex = 0; pageSize = 0;
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetQTPorts1()
        {
            string table = "(SELECT POL_CD AS PORT_CD,POL_NM AS PORT_NM FROM SMQTD WHERE POL_CD IS NOT NULL AND QUOT_TYPE='F' GROUP BY POL_CD,POL_NM UNION SELECT POD_CD AS PORT_CD,POD_NM AS PORT_NM FROM SMQTD WHERE POD_CD IS NOT NULL AND QUOT_TYPE='F' GROUP BY POD_CD,POD_NM)T";
            string condition = string.Empty;
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            DataTable dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetQTCountrys()
        {
            string table = "(SELECT * FROM (SELECT REGION AS CNTRY_CD FROM SMQTD WHERE REGION IS NOT NULL AND TRAN_MODE='E' AND QUOT_TYPE='F' GROUP  BY REGION)A OUTER APPLY (SELECT TOP 1 CNTRY_NM FROM BSCNTY WHERE BSCNTY.CNTRY_CD=A.CNTRY_CD)B)T";
            string condition = string.Empty;
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            DataTable dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        #endregion

        public ActionResult CostTest()
        {
            string uid = Request["uid"];
            Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
            bill.Create(uid, DateTime.Now);
            return Content(@Resources.Locale.L_QTManage_Controllers_429);
        }


        public ActionResult CostDetail()
        {
            string uid = Request["uid"];
            string shipmentId = Request["shipmentId"];
            string location = getOneValueAsStringFromSql("SELECT CMP FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentId));
            string sql = string.Format(@"SELECT U_ID,(CASE WHEN RFQ_NO='undefined' THEN NULL ELSE RFQ_NO END) RFQ_NO,
            QUOT_NO,SHIPMENT_ID,BL_NO,DEBIT_NO,DEBIT_DATE,CHG_CD,CHG_DESCP,CHG_TYPE,REPAY,QCUR,QCHG_UNIT,QUNIT_PRICE,QQTY,QAMT,QEX_RATE,
            QLCUR,QLAMT,QLRATE,QTAX,CHECK_DESCP,CUR,UNIT_PRICE,QTY,BAMT,EX_RATE,LAMT,TAX,REMARK,COST_CENTER,PROFIT_CENTER,INVOICE_INFO,DEC_NO,
            LSP_NO,LSP_NM
            FROM SMBID WHERE SHIPMENT_ID={0} AND (CMP={1} OR CMP={2}) ORDER BY CHG_CD",
            SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(location));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "SmbidModel"));
        }

        public ActionResult CreateBill()
        {
            string uid = Request["uid"];
            string[] uids = uid.Split(';');
            string shipmentId = Request["shipmentId"];
            foreach (string index in uids)
            {
                if (string.IsNullOrEmpty(index))
                    continue;
                Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                bill.Create(index, DateTime.Now);
            }
            //bill.Share(uid);
            return Json(new { message = @Resources.Locale.L_QTManageController_Controllers_202, flag = true });
        }

        public ActionResult QTTrailerQueryData()
        {
            //string condition = GetBaseGroup() + " AND LSP_CD=" + SQLUtils.QuotedStr(this.CompanyId) + " AND TRAN_MODE='C'";
            //string table = "SMQTM";
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string table = string.Format("(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='C')T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));
            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                if ("G".Equals(upri))
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId);
                table = string.Format(@"(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='C' UNION 
SELECT * FROM SMQTM WHERE {2} AND TRAN_MODE='C' AND QUOT_TYPE<>'P' UNION 
SELECT * FROM SMQTM WHERE {2} AND CREATE_BY={3} AND TRAN_MODE='C'  UNION 
SELECT * FROM SMQTM WHERE GROUP_ID={0} AND (RLOCATION={1} OR SHARED_TO={1}) AND TRAN_MODE='C')T", 
                    SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), innerCondition, SQLUtils.QuotedStr(this.UserId));
            }
            string condition = GetCreateDateCondition("SMQTM", "1=1");
            return GetBootstrapData(table, condition);
        }

        public ActionResult QTBrokerQueryData()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string table = string.Format("(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='B')T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));
            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                if ("G".Equals(upri))
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId);
                table = string.Format(@"(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='B' 
UNION SELECT * FROM SMQTM WHERE {2} AND TRAN_MODE='B' AND QUOT_TYPE<>'P' UNION 
SELECT * FROM SMQTM WHERE {2} AND CREATE_BY={3} AND TRAN_MODE='B' UNION 
SELECT * FROM SMQTM WHERE GROUP_ID={0} AND (RLOCATION={1} OR SHARED_TO={1}) AND TRAN_MODE='B')T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), innerCondition, SQLUtils.QuotedStr(this.UserId));
            }

            //string condition = GetBaseGroup() + " AND LSP_CD=" + SQLUtils.QuotedStr(this.CompanyId) + " AND TRAN_MODE='B'";
            //string table = "SMQTM";
            string condition = GetCreateDateCondition("SMQTM", "1=1");
            return GetBootstrapData(table, condition);
        }

        public ActionResult QTCarrierQueryData()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string table = string.Format("(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='O')T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));
            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                if ("G".Equals(upri))
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId);
                table = string.Format(@"(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='O' UNION 
SELECT * FROM SMQTM WHERE {2} AND TRAN_MODE='O' AND QUOT_TYPE<>'P' UNION 
SELECT * FROM SMQTM WHERE {2} AND CREATE_BY={3} AND TRAN_MODE='O' UNION 
SELECT * FROM SMQTM WHERE GROUP_ID={0} AND (RLOCATION={1} OR SHARED_TO={1}) AND TRAN_MODE='O')T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), innerCondition, SQLUtils.QuotedStr(this.UserId));
            }

            //string condition = GetBaseGroup() + " AND LSP_CD=" + SQLUtils.QuotedStr(this.CompanyId) + " AND TRAN_MODE='B'";
            //string table = "SMQTM";
            string condition = GetCreateDateCondition("SMQTM", "1=1");
            return GetBootstrapData(table, condition);
        }

        public ActionResult QTLocalQueryData()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string table = string.Format("(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='X')T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId));
            if ("I".Equals(ioflag))
            {
                string innerCondition = string.Empty;
                string upri = this.UPri;
                if ("G".Equals(upri))
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
                else
                    innerCondition = "GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND (RLOCATION=" + SQLUtils.QuotedStr(this.CompanyId) + " OR CMP=" + SQLUtils.QuotedStr(this.CompanyId)+")";
                table = string.Format(@"(SELECT * FROM SMQTM WHERE GROUP_ID={0} AND LSP_CD={1} AND TRAN_MODE='X' UNION 
SELECT * FROM SMQTM WHERE {2} AND TRAN_MODE='X' AND QUOT_TYPE<>'P' UNION 
SELECT * FROM SMQTM WHERE {2} AND CREATE_BY={3} AND TRAN_MODE='X' UNION 
SELECT * FROM SMQTM WHERE GROUP_ID={0} AND (RLOCATION={1} OR SHARED_TO={1}) AND TRAN_MODE='X')T", SQLUtils.QuotedStr(this.GroupId), SQLUtils.QuotedStr(this.CompanyId), innerCondition, SQLUtils.QuotedStr(this.UserId));
            }
            string condition = GetCreateDateCondition("SMQTM", "1=1");
            return GetBootstrapData(table, condition);
        }

        public ActionResult GetNowDate() 
        {
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            string date = ndt.ToString("yyyy/MM/dd");
            return Json(new { nowDate = date });
        }

        #region 合約管理
        public ActionResult SaveContractData()
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
            string rfq_no = Request["rfq_no"];
            //string dn_no = string.Empty;
            string u_id = Request["u_id"];
            //string mode = Request["mode"];
            string dataType = Request["dataType"];
            string ei0 = null;
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            if (!string.IsNullOrEmpty(rfq_no))
                rfq_no = HttpUtility.UrlDecode(rfq_no);
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SMCTMModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (string.IsNullOrEmpty(rfq_no))
                            rfq_no = ei.Get("RFQ_NO");
                        if (string.IsNullOrEmpty(u_id))
                            u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            //数据验证
                            //sql = string.Format("SELECT * FROM SMRQM WHERE RFQ_NO={0} "
                            //  , SQLUtils.QuotedStr(ei.Get("RFQ_NO")));

                            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //if (dt.Rows.Count > 0)
                            //    return Json(new { message = "已存在该组数据" });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);

                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ei0))
                        mixList.Add(ei0);

                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            sql = string.Format("SELECT * FROM SMCTM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SMCTMModel");

            return ToContent(data);
        }
        public ActionResult SMCTMQueryData()
        {
            return GetBootstrapData("SMCTM", GetBaseGroup());
        }
        public ActionResult GetSMCTMDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMCTM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SMCTMModel");
            return ToContent(data);
        }
        public ActionResult GetQTDetail()
        {
            string flag = "Y";
            string QuotNo = Request["QuotNo"];
            string sql = string.Format("select TOP 1 U_ID,TRAN_MODE,PERIOD from SMQTM WHERE  QUOT_NO ={0}", SQLUtils.QuotedStr(QuotNo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string uid = string.Empty, TranMode = string.Empty, Period = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    TranMode = Prolink.Math.GetValueAsString(dr["TRAN_MODE"]);
                    Period = Prolink.Math.GetValueAsString(dr["PERIOD"]);
                }
            }
            //string uid=OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (string.IsNullOrEmpty(uid))
            {
                flag = "N";
            }
            return Json(new { uid = uid, flag = flag, TranMode = TranMode, Period = Period });
        }
        #region 合约签核通过操作
        public ActionResult ApproveContract()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string DebitNo = Prolink.Math.GetValueAsString(Request.Params["JobNo"]);
            if (string.IsNullOrEmpty(DebitNo))
            {
                returnMsg = @Resources.Locale.L_QTManageController_Controllers_203;
            }
            else
            {
                string[] uids = uid.Split(',');
                string[] DnNos = DebitNo.Split(',');
                for (int i = 0; i < uids.Length; i++)
                {
                    MixedList mixList = new MixedList();
                    uid = uids[i].ToString();
                    DebitNo = DnNos[i].ToString();
                    UserInfo userinfo = new UserInfo
                    {
                        UserId = UserId,
                        CompanyId = CompanyId,
                        GroupId = GroupId,
                        Upri = UPri,
                        Dep = Dep,
                        basecondtions = GetBaseCmp()
                    };

                    returnMsg += Business.ContractApproveHelper.ApproveBillItem(uid, DebitNo, userinfo) + "\n";
                }
            }
            return Json(new { message = returnMsg });
        }
        #endregion

       

        #region 合约签核退回操作
        public ActionResult ApproveBackContract()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DebitNo"]);
            string ApproveType = Prolink.Math.GetValueAsString(Request.Params["ApproveType"]);
            string ApproveTo = Prolink.Math.GetValueAsString(Request.Params["ApproveTo"]);
            string backremark = Prolink.Math.GetValueAsString(Request.Params["BackRemark"]);

            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                Upri = UPri,
                Dep = Dep,
                basecondtions = GetBaseCmp()
            };
            string message = ContractApproveHelper.ContApproveBack(uid, backremark, userinfo);
            return Json(new { message = message });
        }
        #endregion
        #endregion

        /// <summary>
        /// 获取自定义区间表头
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLocalColumInfo()
        {
            string returnMsg = "success";
            string TransType = Request.Params["TransType"];
            string Cmp = Request.Params["Cmp"];
            string VenderCd = Prolink.Math.GetValueAsString(Request.Params["VenderCd"]);

            if (Cmp == "")
            {
                Cmp = CompanyId;
            }
            string Type = Request.Params["Type"];
            string chgTypeStr = "";
            string gwTypeStr = "";
            string cbmTypeStr = "";
            string chgTypeColsStr = "";
            try
            {
                DataTable dt = CommonHelp.GetChargeCodesByVender("CMP=" + SQLUtils.QuotedStr(Cmp), VenderCd);//OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable dtAll = dt.Copy();
                DataTable dt1 = null;
                if (dt.Rows.Count == 0)
                {
                    dt1 = CommonHelp.GetChargeCodes(GetDataPmsCondition("C"), 4);
                }
                dtAll.Merge(dt1);

                if (dtAll.Rows.Count == 0)
                {
                    returnMsg = "fail";
                    return Json(new { message = "該物流業者無費用報價" });
                }
                foreach (DataRow dr in dtAll.Rows)
                {
                    if (Prolink.Math.GetValueAsString(dr["CHG_DESCP"]) != "")
                    {
                        string chg = Prolink.Math.GetValueAsString(dr["CHG_DESCP"]).Replace("\n", "") + "\n(" + Prolink.Math.GetValueAsString(dr["REMARK"]).Replace("\n", "") + ")";

                        chgTypeStr += chg + ";";
                    }

                    if (Prolink.Math.GetValueAsString(dr["GW"]) != "")
                        gwTypeStr += Prolink.Math.GetValueAsString(dr["GW"]).Replace("\n", "") + ";";
                    if (Prolink.Math.GetValueAsString(dr["CBM"]) != "")
                        cbmTypeStr += Prolink.Math.GetValueAsString(dr["CBM"]).Replace("\n", "") + ";";

                    chgTypeColsStr += WebGui.Models.BaseModel.GetModelFiledName(Prolink.Math.GetValueAsString(dr["CHG_CD"])) + ";";
                }
            }
            catch (Exception ex)
            {
                returnMsg = "fail";
                return Json(new { message = returnMsg });
            }

            if (chgTypeStr != "")
                chgTypeStr = chgTypeStr.Substring(0, chgTypeStr.Length - 1);
            if (gwTypeStr != "")
                gwTypeStr = gwTypeStr.Substring(0, gwTypeStr.Length - 1);
            if (cbmTypeStr != "")
                cbmTypeStr = cbmTypeStr.Substring(0, cbmTypeStr.Length - 1);

            return Json(new { message = returnMsg, chgTypeStr = chgTypeStr, chgTypeColsStr = chgTypeColsStr.Substring(0, chgTypeColsStr.Length - 1), gwTypeStr = gwTypeStr, cbmTypeStr = cbmTypeStr, cmp = CompanyId, stn = Station, user = UserId, group = GroupId, createDate = DateTime.Now.ToString("yyyy/MM/dd HH:ss") });

        }

        public ActionResult DownloadQuotDetail()
        {
            string QuotNo = Request.Params["QuotNo"];
            string sql = string.Format("SELECT QUOT_NO,TRAN_MODE,POL_CD,POL_NM,POD_CD,POD_NM,VIA_CD,CARRIER,INCOTERM,CONTRACT_NO,TRAN_TYPE,CHG_TYPE,PUNIT FROM SMQTD WHERE QUOT_NO={0} AND (CHG_TYPE='F' OR CHG_CD='FRT') ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(QuotNo));
            DataTable qtdDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            ChargeToExcel chargeToExcel = new ChargeToExcel();
            string xlsFile = chargeToExcel.ResetXls(qtdDt);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", regex.Match(xlsFile).Value);
        }

        public ActionResult UploadChargeAction(FormCollection form)
        {
            string returnMessage = string.Empty;
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping = "ChargeInfoMapping";
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.Replace(".", "");
                    string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadChargeAction\\");
                    string excelFileName = string.Format("{0}.{1}", dirpath + TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss"), strExt);
                    file.SaveAs(excelFileName);
                    MixedList ml = new MixedList();
                    MixedList partyml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    parm["condition"] = GetDataPmsCondition("C");
                    List<string> contractList = new List<string>();
                    parm["ContractList"] = contractList;
                    parm["QuotNo"] = "";
                    ExcelParser.RegisterEditInstructFunc(mapping, HandleFreightQuotDetail);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm, 0);
                    contractList = parm["ContractList"] as List<string>;
                    string quotNo = Prolink.Math.GetValueAsString(parm["QuotNo"]);
                    string sql = string.Format("SELECT DISTINCT CONTRACT_NO FROM SMQTD WHERE CONTRACT_NO IN {0} AND QUOT_NO !={1}", SQLUtils.Quoted(contractList.ToArray()), SQLUtils.QuotedStr(quotNo));
                    DataTable tdDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string conList = "";
                    foreach (DataRow dr in tdDt.Rows)
                    {
                        string contractNo = Prolink.Math.GetValueAsString(dr["CONTRACT_NO"]).Trim();
                        if (!string.IsNullOrEmpty(conList))
                            conList += ",";
                        conList += contractNo;
                    }
                    if (!string.IsNullOrEmpty(conList))
                        return Json(new { message = conList + " :" + @Resources.Locale.L_QTManage_Controllers_ContractNo });
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

        public static string HandleFreightQuotDetail(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string quotNo = Prolink.Math.GetValueAsString(ei.Get("QUOT_NO"));
            List<string> contractList = parm["ContractList"] as List<string>;
            string polCd = Prolink.Math.GetValueAsString(ei.Get("POL_CD"));
            if (string.IsNullOrEmpty(quotNo) || string.IsNullOrEmpty(polCd))
                return "";
            parm["QuotNo"] = quotNo;
            string condition = string.Empty;
            string contractNo = Prolink.Math.GetValueAsString(ei.Get("CONTRACT_NO")).Trim();
            string carrier = Prolink.Math.GetValueAsString(ei.Get("CARRIER"));
            string podCd = Prolink.Math.GetValueAsString(ei.Get("POD_CD"));
            string via = Prolink.Math.GetValueAsString(ei.Get("VIA_CD"));
            string punit = Prolink.Math.GetValueAsString(ei.Get("PUNIT"));
            ei.PutKey("QUOT_NO", quotNo);
            ei.PutKey("POL_CD", polCd);
            if (string.IsNullOrEmpty(carrier))
            {
                condition = " (CARRIER='' OR CARRIER IS NULL)";
            }
            else
                ei.PutKey("CARRIER", carrier);
            if (string.IsNullOrEmpty(podCd))
            {
                if (!string.IsNullOrEmpty(condition))
                    condition += " AND ";
                condition += " (POD_CD='' OR POD_CD IS NULL)";
            }
            else
                ei.PutKey("POD_CD", podCd);
            if (string.IsNullOrEmpty(via))
            {
                if (!string.IsNullOrEmpty(condition))
                    condition += " AND ";
                condition += " (VIA_CD='' OR VIA_CD IS NULL)";
            }
            else
                ei.PutKey("VIA_CD", via);
            if (string.IsNullOrEmpty(punit))
            {
                if (!string.IsNullOrEmpty(condition))
                    condition += " AND ";
                condition += " (PUNIT='' OR PUNIT IS NULL)";
            }
            else
                ei.PutKey("PUNIT", punit);
            ei.Condition = condition;
            ei.Put("CONTRACT_NO", contractNo);
            if (!contractList.Contains(contractNo) && !string.IsNullOrEmpty(contractNo))
                contractList.Add(contractNo);
            ei.Remove("INCOTERM");
            return string.Empty;
        }


        public ActionResult ExpressAddChargeQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("EXPADD");
            return View();
        }
        public ActionResult ExpressAddSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM SMBFA WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("SMBFA");
            return View();
        }

        public ActionResult GetExpressAddData()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string condition = GetCreateDateCondition("SMBFA", "");
            DataTable dt = ModelFactory.InquiryData("*", "SMBFA", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmbfaModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult ExpressAddSave()
        {
            string changeData = Request.Params["changedData"];
            string uid = Prolink.Math.GetValueAsString(Request.Params["uid"]);
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbfaModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            uid = System.Guid.NewGuid().ToString();
                            ei.PutKey("U_ID", uid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId));
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId));
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (ei.Get("U_ID") == null)
                                continue;
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
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
                    returnMessage = ex.ToString();
                }
            }
            string sql = string.Format("SELECT * FROM SMBFA WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbfaModel");
            return ToContent(data);
        }

        public ActionResult ExpressAddDetail()
        {
            string uId = Request["UId"];
            string sql = string.Format("SELECT * FROM SMBFA WHERE U_ID={0}", SQLUtils.QuotedStr(uId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbfaModel");
            return ToContent(data);
        }

        public ActionResult CWStandardQuery()
        {
            ViewBag.MenuBar = false;
            SetTranModeSelect();
            ViewBag.pmsList = GetBtnPms("CWSET");
            return View();
        }

        public ActionResult CWStandardSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM SMBWS WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("SMBWS");
            return View();
        }

        public ActionResult GetCWStandardData()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string condition = GetCreateDateCondition("SMBWS", "");
            DataTable dt = ModelFactory.InquiryData("*", "SMBWS", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmbwsModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        public ActionResult CWStandardSave()
        {
            string changeData = Request.Params["changedData"];
            string uid = Prolink.Math.GetValueAsString(Request.Params["uid"]);
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbwsModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            uid = System.Guid.NewGuid().ToString();
                            ei.PutKey("U_ID", uid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId));
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId));
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (ei.Get("U_ID") == null)
                                continue;
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        mixList.Add(ei);
                    }
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
                    returnMessage = ex.ToString();
                }
            }
            string sql = string.Format("SELECT * FROM SMBWS WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbwsModel");
            return ToContent(data);
        }

        public ActionResult CWStandardDetail()
        {
            string uId = Request["UId"];
            string sql = string.Format("SELECT * FROM SMBWS WHERE U_ID={0}", SQLUtils.QuotedStr(uId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbwsModel");
            return ToContent(data);
        }
    }
}
