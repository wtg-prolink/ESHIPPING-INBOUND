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
using Business.Mail;
using Business;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using Business.TPV;
using System.IO;
using TrackingEDI;
using TrackingEDI.Model;
using System.Web.Configuration;
using System.Threading;

namespace WebGui.Controllers
{
    public class DNManageController : BaseController
    {
        //
        // GET: /DNManage/

        #region View
        public ActionResult DNApproveManage()
        {
            ViewBag.MenuBar = false;
            SetApproveSelect();
            GetUseAcct();
            SetRoleSelect();
            SetApproveRole();
            ViewBag.AtranType = GetBscodeByMode("TCT");
            ViewBag.CargoType = GetBscodeByMode("TCGT");
            ViewBag.pmsList = GetBtnPms("DN010");
            return View();
        }

        private void SetRoleSelect()
        {
            ViewBag.SelectRole = "";
            ViewBag.DefaultRole = "";

            #region Approve
            string sql = string.Format(@"SELECT APPROVE_GROUP,GROUP_DESCP,SEQ_NO FROM APPROVE_ATTR_D WHERE U_FID=(select U_ID FROM APPROVE_ATTRIBUTE WHERE APPROVE_ATTR='DN' AND {0}) AND {1}
            ORDER BY SEQ_NO,APPROVE_GROUP ASC", GetBaseCmp(), GetBaseCmp());

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
                    ViewBag.DefaultRole = Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                }
                select += Prolink.Math.GetValueAsString(dr["APPROVE_GROUP"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["GROUP_DESCP"]);
            }


            select += ";Finish:Finish;Finished:Finished/ATD<3months;ARCH:Archived/ATD≥3months";
            ViewBag.SelectRole = select;
            #endregion
        }

        private void GetUseAcct()
        {
            string sql = string.Format("SELECT U_PRI FROM SYS_ACCT WHERE U_ID={0} AND {1}", SQLUtils.QuotedStr(UserId), GetBaseDep());
            string upri = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (Dep.Equals("LST") || Dep.Equals("GLST"))
            {
                ViewBag.Upri = "G";
            }
            else
            {
                ViewBag.Upri = upri;
            }
        }

        private void SetApproveRole()
        {
            ViewBag.ApproveRole = "";
            string approveroles=DNApproveLoop.GetApprove(UserId, CompanyId, GroupId,UPri,Dep);
            ViewBag.ApproveRole = approveroles;
        }

        private void SetApproveSelect()
        {
            ViewBag.SelectApprove = "";
            ViewBag.DefaultApprove = "";

            #region Approve
            string sql = string.Format(@"SELECT APPROVE_CODE,APPROVE_NAME FROM APPROVE_FLOW_M WHERE  GROUP_ID={0} AND CMP_ID={1} AND AU_ID IN(
                SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE  GROUP_ID={0} AND CMP={1} AND APPROVE_ATTR='DN')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
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
                    ViewBag.DefaultApprove = Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
                }
                select += Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["APPROVE_NAME"]);
            }
            ViewBag.SelectApprove = select;
            #endregion
        }

        private string GetBscodeSelect(string Bscode)
        {
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE={0}", SQLUtils.QuotedStr(Bscode));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
            }
            return select;
        }


        public ActionResult DNFlowManage()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("DN020");
            ViewBag.approveGroup = GetApproveGroup("DN");
            SetApproveSelect();
            SetRoleSelect();
            ViewBag.AtranType = GetBscodeByMode("TCT");
            ViewBag.SelectCargoType = GetBscodeByMode("TCGT");
            return View();
        }

        public ActionResult DNDetailVeiw(string id = null, string uid = null)
        {
            SetSchema("DNDetailView");
            SetApproveSelect();
            SetRoleSelect();
            string rcsql = string.Format("SELECT RC FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            string rc = OperationUtils.GetValueAsString(rcsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.RC = rc;
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN020");
            return View();
        }

        public void SetBQuerySelect()
        {
            ViewBag.SelectServiceMode = GetBscodeByMode("PK");
            ViewBag.SelectCargoType = GetBscodeByMode("TCGT");
            ViewBag.SelectTrackWay = GetBscodeByMode("TDTK");
            ViewBag.SelectCarType = GetBscodeByMode("TDT");
        }

        public ActionResult FCLBQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN030");
            ViewBag.ISFUrl = WebConfigurationManager.AppSettings["ISF_URL"];
            ViewBag.ISFAcct = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFACCT' AND "+GetBaseCmp())[1];
            ViewBag.ISFKey = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFKey' AND " + GetBaseCmp())[1];
            ViewBag.ISFPWD = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFPWD' AND " + GetBaseCmp())[1];
            return View();
        }

        public ActionResult LCLBQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN035");
            ViewBag.ISFUrl = WebConfigurationManager.AppSettings["ISF_URL"];
            ViewBag.ISFAcct = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFACCT' AND " + GetBaseCmp())[1];
            ViewBag.ISFKey = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFKey' AND " + GetBaseCmp())[1];
            ViewBag.ISFPWD = GetBsCodeData(" CD_TYPE='SYS' AND CD='ISFPWD' AND " + GetBaseCmp())[1];
            return View();
        }

        public ActionResult AirBooking()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN040");
            return View();
        }

        public ActionResult AirBookingSetup(string id = null, string uid = null)
        {
            SetSchema("AirBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN040");
            return View();
        }

        public ActionResult RailwayBooking()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN050");
            return View();
        }

        public ActionResult RailwayBookingSetup(string id = null, string uid = null)
        {
            SetSchema("RailwayBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN050");
            return View();
        }

        public ActionResult IEBooking()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN060");
            return View();
        }

        public ActionResult DEBooking()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN070");
            return View();
        }

        public ActionResult DEBookingSetup(string id = null, string uid = null)
        {
            SetSchema("DEBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN070");
            return View();
        }

        public ActionResult IEBookingSetup(string id = null, string uid = null)
        {
            SetSchema("IEBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN060");
            ViewBag.ServiceSel = CommonHelp.getBscodeForSelect("SERV", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult DTBooking()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN080");
            return View();
        }

        public ActionResult DTBookingSetup(string id = null, string uid = null)
        {
            SetSchema("DTBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN080");
            return View();
        }

        public ActionResult CustomsNotice()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN090");
            return View();
        }

        public ActionResult FCLBooking(string id = null, string uid = null)
        {
            SetSchema("FCLBooking");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN030");
            return View();
        }

        public ActionResult LCLBooking(string id = null, string uid = null)
        {
            SetSchema("LCLBooking");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN035");
            return View();
        }

        public ActionResult ChangePodView(string id = null, string uid = null)
        {
            SetSchema("FCLBooking");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            //ViewBag.pmsList = GetBtnPms("DN035");
            return View();
        }

        public ActionResult CustomsBooking(string id = null, string uid = null)
        {
            SetSchema("CustomsBooking");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.pmsList = GetBtnPms("DN090");
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult ShipmentDN(string id = null, string shipmentid = null)
        {
            SetSchema("DNDetailView");
            if (shipmentid == null)
            {
                shipmentid = id;
            }
            ViewBag.ShipmentId = shipmentid;
            SetApproveSelect();
            SetRoleSelect();
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult InvPkgQuery()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            ViewBag.MenuBar = false;
            ViewBag.DnNo = DnNo;
            ViewBag.ShipmentId = ShipmentId;
            ViewBag.pmsList = GetBtnPms("DN011");
            SetRoleSelect();
            return View();
        }

        public ActionResult InvPkgSetup(string id=null, string uid=null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM SMINM WHERE 1=0";
            string tir = "", CargoType = "";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            if (uid != null && uid != "")
            {
                sql = "SELECT SMDN.ETD FROM SMDN, SMINM WHERE SMDN.DN_NO=SMINM.DN_NO AND SMINM.U_ID=" + SQLUtils.QuotedStr(id);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count <= 0)
                {
                    sql = "SELECT SMSM.DN_ETD AS ETD FROM SMSM, SMINM WHERE SMSM.SHIPMENT_ID=SMINM.SHIPMENT_ID AND SMINM.U_ID=" + SQLUtils.QuotedStr(id);
                    dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                if (dt.Rows.Count > 0)
                {
                    DateTime year = Prolink.Math.GetValueAsDateTime(dt.Rows[0]["ETD"]);
                    string str_year = year.ToString("yyyy");

                    sql = "SELECT CD_DESCP FROM BSCODE WHERE GROUP_ID='TPV' AND CD_TYPE='TIR' AND CD=" + SQLUtils.QuotedStr(str_year);
                    tir = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }

                sql = "SELECT CARGO_TYPE FROM SMDN, SMINM WHERE SMDN.DN_NO=SMINM.DN_NO AND SMINM.U_ID = {0}";
                sql = string.Format(sql, SQLUtils.QuotedStr(uid));
                CargoType = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            
            ViewBag.pmsList = GetBtnPms("DN011");
            ViewBag.schemas = ToContent(schemas);
            ViewBag.Uid = id;
            ViewBag.tir = tir;
            ViewBag.CargoType = CargoType;
            ViewBag.selects = TKBLController.GetSelectsToString("DNDetailView", CompanyId, this.GroupId, this.BaseCompanyId, "不需要", "");
            return View();
        }
        public ActionResult SMDNDSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            string sql = "SELECT * FROM SMDND WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            sql = string.Format("SELECT DN_NO FROM SMDN WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            string dnno = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.DnNo = dnno;
            return View();
        }

        public ActionResult BookingQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN210");
            return View();
        }

        public ActionResult ErrManage()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN025");
            return View();
        }


        public ActionResult SMBDQuery() 
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("DNI030");
            return View();
        }

        public ActionResult SMBDSetup(string id = null, string uid = null)
        {            
            if (uid == null)
            {
                uid = id;
            }           
            //ViewBag.pmsList = GetBtnPms("DNI030");
            string sql = "SELECT * FROM SMBD WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult SMBDUpdate()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Request.Params["uid"];
            string shipmentid = Request.Params["shipmentid"];
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
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbdModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                            ei.AddKey("U_ID");
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbddModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (IsGuidByError(Prolink.Math.GetValueAsString(ei.Get("U_ID"))))
                                continue;
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
            return Json(new { UId = u_id });
        }

        public ActionResult SMBDQueryData()
        {
            return GetBootstrapData("SMBD", GetBaseCmp());
        }

        public ActionResult GetSMBDItem()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMBD WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (mainDt.Rows.Count <= 0)
            {
                return ToContent(data);
            }
            sql = string.Format("SELECT * FROM SMBDD WHERE U_FID={0} ", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            data["main"] = ModelFactory.ToTableJson(mainDt, "SmbdModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbddModel");
            return ToContent(data);
        }

        public ActionResult VoidSMBD()
        {
            string returnMsg = "";
            string uid = Request.Params["UId"];
            MixedList mlist = new MixedList();
            if (!string.IsNullOrEmpty(uid))
            {
                EditInstruct ei = new EditInstruct("SMBD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.Put("STATUS", "V");
                mlist.Add(ei);
            }
            else
            {
                returnMsg = @Resources.Locale.L_DNManageController_Controllers_103;
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_DNManageController_Controllers_104;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public ActionResult SMDNPQuyer(string id = null, string uid = null)
        {            
            string dnno = Request["DnNo"];
            string sql = string.Format("SELECT * FROM SMDNP WHERE DN_NO={0} ORDER BY DELIVERY_ITEM", SQLUtils.QuotedStr(dnno));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["smdnp"] = ModelFactory.ToTableJson(subDt, "SmdnpModel");
            return ToContent(data);
        } 
        #endregion

        #region 訂艙確認 View
        public ActionResult FCLBConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN110");
            return View();
        }

        public ActionResult LCLBConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN120");
            return View();
        }
        
        public ActionResult AirConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN130");
            return View();
        }

        public ActionResult RailwayConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN140");
            return View();
        }

        public ActionResult IEBookingConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN150");
            return View();
        }

        public ActionResult DEBookingConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN160");
            return View();
        }

        public ActionResult DTBookingConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN170");
            return View();
        }

        public ActionResult CustomsNoticeConfirmQuery()
        {
            ViewBag.MenuBar = false;
            SetBQuerySelect();
            ViewBag.pmsList = GetBtnPms("DN180");
            return View();
        }
        
        //FCL訂艙確認
        public ActionResult FCLBConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("FCLBooking");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN110");
            return View();
        }

        //LCL訂艙確認
        public ActionResult LCLBConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("LCLBooking");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN120");
            return View();
        }
        
        //空運訂艙確認
        public ActionResult AirConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("AirBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN130");
            return View();
        }

        //鐵路訂艙確認
        public ActionResult RailwayConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("RailwayBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN140");
            return View();
        }

        //國際快遞訂艙確認
        public ActionResult IEBookingConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("IEBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN150");
            return View();
        }

        //國內快遞訂艙確認
        public ActionResult DEBookingConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("DEBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN160");
            return View();
        }

        //內貿訂艙確認
        public ActionResult DTBookingConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("DTBookingSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN170");
            return View();
        }

        //出口報關確認
        public ActionResult CustomsNoticeConfirmSetup(string id = null, string uid = null)
        {
            SetSchema("CustomsBooking");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("DN180");
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
                    case "DNDetailView":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMDN WHERE 1=0";
                        break;
                    case "FCLBooking":
                    case "AirBookingSetup":
                    case "RailwayBookingSetup":
                    case "DEBookingSetup":
                    case "DTBookingSetup":
                    case "LCLBooking":
                    case "IEBookingSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMSM WHERE 1=0";
                        break;
                    case "CustomsBooking":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMSM WHERE 1=0";
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
        [ValidateInput(false)]
        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*", string dnapprove="",NameValueCollection namevaluecollection=null)
        {
           int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                string basecondtion=GetDecodeBase64ToString(Request.Params["basecondition"]);
                if (!string.IsNullOrEmpty(basecondtion))
                {
                    if (string.IsNullOrEmpty(condition))
                    {
                        condition = basecondtion;
                    }
                    else
                    {
                        condition += " AND " + basecondtion;
                    }
                }
                dt = GetStatusCountData(statusField, table, condition, Request.Params,dnapprove);
                pageSize = 1;
            }
            else
            {
                if (namevaluecollection == null) namevaluecollection = Request.Params;
                dt = ModelFactory.InquiryData(colNames, table, condition, namevaluecollection, ref recordsCount, ref pageIndex, ref pageSize);
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

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues, string dnapprove="")
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);
            //king filter archived status
            if (defaultTable == "SMSM")
            {
                condition += " AND (" + col + " !='R' )";
            }
            else
            {
                condition += " AND (" + col + " !='R' AND " + col + " !='A'  AND " + col + " !='ARCH' )";
            }
            

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";

            if (!string.IsNullOrEmpty(dnapprove))
            {
                if (dnapprove == "DNAPPROVE")
                {
                    string personsql = "SELECT 'Person' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE APPROVE_USER='" + UserId + "' UNION";
                    string localsql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " AND APPROVE_TO NOT IN('A') GROUP BY " + col + " UNION";
                    //string fiftysql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " AND APPROVE_TO IN('RDMM','CSMM','MPMM','SSMM')AND APPROVE_USER='" + UserId + "' GROUP BY " + col+ " UNION";
                    string asql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " AND APPROVE_TO='A' AND DEP='"+Dep+"' GROUP BY " + col;
                    sql = personsql + localsql + asql;
                }
            }

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

        //权限方法  
        public string GetPMSByUrpi()
        {
            string condtion = GetPMSByUPri();
            string sql=string.Format("SELECT TRAN_TYPE FROM SYS_ACCT WHERE U_ID={0} AND {1}",SQLUtils.QuotedStr(UserId), GetBaseCmp());
            string actrantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string actrancondition=string.Empty;
            if (!string.IsNullOrEmpty(actrantype))
            {
                if (Dep.Equals("SFI")) //财务才区分是否是内销还是外销
                {
                    actrantype = actrantype.Trim(';');
                    string[] actrantypes = actrantype.Split(';');
                    actrancondition = string.Format("AND ATRAN_TYPE IN {0}", SQLUtils.Quoted(actrantypes));
                }
            }
            if (!string.IsNullOrEmpty(actrancondition))
            {
                condtion += actrancondition;
            }
            return condtion;
        }

        public string GetPMSByUPri()
        {
            string condtion = GetBaseGroup();
            switch (UPri)
            {
                case "G":
                    condtion = GetBaseGroup();
                    break;
                case "C":
                    condtion = GetBaseCmp() + getSpecialPriCondition();
                 
                    break;
                case "S":
                    condtion = GetBaseStn() + getSpecialPriCondition();
                    break;
                case "D":
                    condtion = string.Format("GROUP_ID={0} AND CMP={1} AND DEP={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Dep)) + getSpecialPriCondition();

                    break;
                case "U":
                    condtion = string.Format("GROUP_ID={0} AND CMP={1} AND DEP={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(Dep)) + string.Format(" AND CREATE_BY={0}", SQLUtils.QuotedStr(UserId));
                    break;
               default:
                    condtion += getSpecialPriCondition();
                     break;
            }
            return condtion;
        }

        public string getSpecialPriCondition()
        {
            string condition = "";
            if (TCmp != "")
            {
                condition += string.Format(" AND STN in ( {0} ) ", TCmp);
            }
            if (PlantPri != "''")
            {
                condition += string.Format(" AND PLANT in ( {0} ) ", PlantPri);
            }
            return condition;
        }

        public readonly string DN_APPROVE_CONDITON = " AND (DN_NO_CMP_REF IS NULL OR ltrim(DN_NO_CMP_REF)='') AND NOT EXISTS (SELECT DN_NO FROM SMDN AS D WHERE SMDN.DN_NO = D.DN_NO AND (DN_TYPE= N'1' OR  DN_TYPE= N'3') AND SAP_ID= N'SPMS') AND STATUS !='V' ";
        
        public ActionResult DNApproveQueryData()
        {
            //if using Virtual Column ,has get the requst params 'virConditions', and input to ConvParam2SQL function convert to sql string ,and todo to sub condition
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string subSql = "";
            string fiftySql = string.Empty;
            bool IsPerson=false;
            if (virCondition != "")
            {
                subSql = " AND DN_NO IN ( SELECT REF_NO FROM APPROVE_RECORD WHERE APPROVE_CODE <> 'VOID' AND " + virCondition + " )";
            }
            string approveto = Prolink.Math.GetValueAsString(Request.Params["conditions"]);
            if (!string.IsNullOrEmpty(approveto))
            {
                if (approveto.Contains("ApproveTo=Person"))
                {
                    fiftySql = string.Format(" AND APPROVE_USER={0}", SQLUtils.QuotedStr(UserId));
                    IsPerson = true;
                }
                else if (approveto.Contains("ApproveTo=A"))
                {
                    fiftySql = string.Format(" AND DEP={0}", SQLUtils.QuotedStr(Dep));
                    fiftySql +=" AND "+ GetPMSByUrpi();
                }
                else
                {
                    fiftySql += " AND " + GetPMSByUrpi();
                }
            }
            else
            {
                fiftySql += " AND " + GetPMSByUrpi();
            }

            string stnSql = " AND (DN_NO_CMP_REF IS NULL OR ltrim(DN_NO_CMP_REF)='') AND NOT EXISTS (SELECT DN_NO FROM SMDN AS D WHERE SMDN.DN_NO = D.DN_NO AND (DN_TYPE= N'1' OR  DN_TYPE= N'3') AND SAP_ID= N'SPMS') AND STATUS !='V' ";
            string table = "(SELECT V_SMDN.* FROM V_SMDN) SMDN";
            //string table = "(SELECT V_SMDN.*,(SELECT TOP 1 CASE WHEN FCOUNT > 0 THEN 'Y' ELSE '' END  FROM V_COUNT_EDOC WHERE V_COUNT_EDOC.JOB_NO=CONVERT(NVARCHAR(50),V_SMDN.U_ID))AS EDOC FROM V_SMDN)SMDN";
            NameValueCollection namevaluecollection = null;
            if (IsPerson)
            {
                namevaluecollection = new NameValueCollection();
                for (int i = 0; i < Request.Params.Count;i++ )
                {
                    if ("conditions".Equals(Request.Params.GetKey(i))) continue;
                    namevaluecollection.Add(Request.Params.GetKey(i), Request.Params[i]);
                }
            }
            return GetBootstrapData(table, "1=1 " + subSql + fiftySql + stnSql, "*", "DNAPPROVE", namevaluecollection);
        }

        public ActionResult DNFlowQueryData()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string subSql = "";
            string fiftySql = string.Empty;
            if (virCondition != "")
            {
                subSql = " AND EXISTS (SELECT *  FROM SMDNS  WHERE  SMDNS.DN_NO = V_SMDN.DN_NO AND " + virCondition + ")";
            }
            string table = @"(SELECT * FROM V_SMDN WHERE STATUS='D' AND (DN_NO_CMP_REF='' OR DN_NO_CMP_REF IS NULL) UNION
                            SELECT * FROM V_SMDN WHERE  STATUS!='D' UNION SELECT * FROM V_FLOWSMDN ) V_SMDN";
            return GetBootstrapData(table, GetPMSByUPri() + subSql);
        }

        public ActionResult DNSpellQueryData()
        {
            string conditions = " (COMBINE_INFO IS NULL OR COMBINE_INFO='') AND STATUS='D' AND (DN_NO_CMP_REF IS NULL OR DN_NO_CMP_REF='') AND ";
            string basecondtion = GetDecodeBase64ToString(Request.Params["basecondition"]);
            conditions += basecondtion;
            conditions += " AND " + GetBaseCmp();
            return GetBootstrapData("SMDN", conditions);
        }

        public ActionResult DNParterQueryData(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string rcsql = string.Format("SELECT RC FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            string rc = OperationUtils.GetValueAsString(rcsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string basefileds = "'0' AS Value1,'0' AS UNIT_PRICE1,'0' AS COST";

            if (rc.Equals("B"))
            {
                basefileds = "UNIT_PRICE1,COST,VALUE1";
            }
            else if (rc.Equals("R"))    //收入
            {
                basefileds = "'0' AS COST,VALUE1,UNIT_PRICE1";
            }
            else if (rc.Equals("C"))    //成本
            {
                basefileds = "'0' AS UNIT_PRICE1,'0' AS VALUE1,COST";
            }

            basefileds += @",U_ID,U_FID,DN_NO,PLANT,PO_NO,PKG_NUM,CUR1,CUR
                            ,SO_NO,SO_ITEM,VALUE1,CBM
                            ,ONE_TIME,DEL_ITEM,RESOLUTION,BRAND,CATEGORY
                            ,GOODS_DESCP,PROD_DESCP,PROD_SPEC,OHS_CODE,IHS_CODE
                            ,IPART_NO,OPART_NO,PART_NO,SAFETY_MODEL,BRAND_TYPE
                            ,PROD_TYPE,ASSEMBLY_ID,MATERIAL_TYPE,SIZE,PRODUCT_DATE
                            ,QTY,QTYU,CBMU,UL,BATTERY,ADDS,DU,PKG_UNIT
                            ,GW,CNTR_STD_QTY,JOB_NO,JQTY,PRODUCT_LINE
                            ,IQTY,PQTY,WQTY,ESTIMATE_DATE,ACTURE_DATE
                            ,ITEM_NO,ODN_NO,PMATN,REPLACE_PART,BSTKD_E,IHREZ
                            ,IHREZ_E,DELIVERY_ITEM,INSPECT
                            ,DOWN_FLAG,SEND_SFIS_FLAG,REF_PART,PREPARE_RMK,SLOC,INTERFACE_CD";

            return GetBootstrapData("SMDNP", "U_FID=" + SQLUtils.QuotedStr(uid), basefileds);
        }

        public ActionResult DNSeriesQueryData(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            return GetBootstrapData("SMDNS", "U_FID=" + SQLUtils.QuotedStr(uid));
        }

        public ActionResult GetDNDetail()
        {
            return GetBootstrapData("SMDN", GetBaseCmp());
        }

        public ActionResult GetDNDetailItem()
        {
            string u_id = Request["UId"];
            string rcsql = string.Format("SELECT RC FROM SYS_ACCT WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId));
            string rc = OperationUtils.GetValueAsString(rcsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string sql = string.Format("SELECT * FROM V_SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string basefileds = " '0' AS UNIT_PRICE1,'0' AS COST,'0' AS VALUE1";
            DataRow mainrow = mainDt.Rows[0];
            if (rc.Equals("B"))
            {
                basefileds = "UNIT_PRICE1,COST,VALUE1";
            }
            else if (rc.Equals("R"))    //收入
            {
                basefileds = "'*' AS COST,VALUE1,UNIT_PRICE1";
            }
            else if (rc.Equals("C"))    //成本
            {
                basefileds = "'*' AS UNIT_PRICE1,'*' AS VALUE1,COST";
            }

            basefileds += @",U_ID,U_FID,DN_NO,PLANT,PO_NO,PKG_NUM,CBM,CUR1,CUR
                            ,SO_NO,SO_ITEM,VALUE1
                            ,ONE_TIME,DEL_ITEM,RESOLUTION,BRAND,CATEGORY
                            ,GOODS_DESCP,PROD_DESCP,PROD_SPEC,OHS_CODE,IHS_CODE
                            ,IPART_NO,OPART_NO,PART_NO,SAFETY_MODEL,BRAND_TYPE
                            ,PROD_TYPE,ASSEMBLY_ID,MATERIAL_TYPE,SIZE,PRODUCT_DATE
                            ,QTY,QTYU,CBMU,UL,BATTERY,ADDS,DU,PKG_UNIT
                            ,GW,CNTR_STD_QTY,JOB_NO,JQTY,PRODUCT_LINE
                            ,IQTY,PQTY,WQTY,ESTIMATE_DATE,ACTURE_DATE
                            ,ITEM_NO,ODN_NO,PMATN,REPLACE_PART,BSTKD_E,IHREZ
                            ,IHREZ_E,DELIVERY_ITEM,INSPECT
                            ,DOWN_FLAG,SEND_SFIS_FLAG,REF_PART,PREPARE_RMK,SLOC,INTERFACE_CD";

            sql = string.Format("SELECT {0} FROM SMDNP WHERE DN_NO={1} ORDER BY DELIVERY_ITEM", basefileds, SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDNS WHERE DN_NO = {0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDNPT WHERE DN_NO = {0} ORDER BY ORDER_BY ASC", SQLUtils.QuotedStr(u_id));
            DataTable subDt3 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMCUFT WHERE DN_NO = {0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt4 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmdnModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmdnpModel");
            data["sub2"] = ModelFactory.ToTableJson(subDt2, "SmdnsModel");
            data["sub3"] = ModelFactory.ToTableJson(subDt3, "SmdnptModel");
            data["sub4"] = ModelFactory.ToTableJson(subDt4, "SmcuftModel");
            return ToContent(data);
        }

        public ActionResult SaveDN()
        {
            string changeData = Request.Params["changedData"];
            string u_id = Request.Params["uid"];
            string dn_no = Request.Params["dnno"];
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
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        dn_no = ei.Get("DN_NO");
                        string dnetd = ei.Get("ETD");

                        if (TrackingEDI.Business.DateTimeUtils.IsDate(dnetd))
                        {
                            int []ymw =TrackingEDI.Business.DateTimeUtils.DateToYMW(dnetd);
                            if (ymw.Length >= 3)
                            {
                                ei.Put("YEAR", ymw[0]);
                                ei.Put("MONTH", ymw[1]);
                                ei.Put("WEEKLY", ymw[2]);
                            }
                            //ei.Put("WEEKLY", TrackingEDI.Business.DateTimeUtils.WeekOfYear(dnetd));
                            //ei.Put("MONTH", TrackingEDI.Business.DateTimeUtils.MonthOfYear(dnetd));
                            //ei.Put("YEAR", TrackingEDI.Business.DateTimeUtils.YearOfYear(dnetd));
                        }
                        
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.Remove("SHIPMENT_ID");
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                            ei.AddKey("U_ID");
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdnpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("DN_NO", dn_no);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdnsModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("DN_NO", dn_no);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub3")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdnptModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        ei.Put("DN_NO", dn_no);
                        string partytype = Prolink.Math.GetValueAsString(ei.Get("PARTY_TYPE"));
                        bool IsChange = false;
                        switch (partytype)
                        {
                            case "FS":
                            case "SP":
                            case "BO":
                            case "SH":
                            case "WE":
                            case "CR":
                            case "CS":
                                IsChange = true;
                                break;
                        }
                        if (!IsChange)
                            continue;

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", u_id);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION || ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.PutKey("PARTY_TYPE", ei.Get("PARTY_TYPE"));
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub4")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmcuftModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        ei.Put("DN_NO", dn_no);
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("DN_NO", dn_no);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION || ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("U_FID", u_id);
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
            return Json(new { UId = dn_no });
            //return ToContent(data);
        }


        #region Approve签核通过操作
        public ActionResult ApproveDN()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            if (string.IsNullOrEmpty(DnNo))
            {
                returnMsg = @Resources.Locale.L_DNManageController_Controllers_1051;
            }
            else
            {
                string[] uids = uid.Split(',');
                string[] DnNos = DnNo.Split(',');
                for (int i = 0; i < uids.Length; i++)
                {
                    MixedList mixList = new MixedList();
                    uid = uids[i].ToString();
                    DnNo = DnNos[i].ToString();
                    returnMsg += DNApproveLoop.ApproveDnItem(uid, DnNo, UserId, Dep, GetBaseCmp(), UPri) + "\n";
                    //UserInfo userinfo = new UserInfo
                    //{
                    //    CompanyId = CompanyId,
                    //    GroupId = GroupId,
                    //    Upri = UPri,
                    //    Dep = Dep,
                    //    UserId = UserId
                    //};
                    //returnMsg += DNApproveLoop.ApproveDnItem(uid, DnNo, userinfo, GetBaseCmp()) + "\n";
                }
            }
            return Json(new { message = returnMsg });
        }

        #endregion

        #region 签核退回操作
        public ActionResult ApproveBackDN()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string DnType = Prolink.Math.GetValueAsString(Request.Params["DnType"]);
            string ApproveTo = Prolink.Math.GetValueAsString(Request.Params["ApproveTo"]);
            string backremark = Prolink.Math.GetValueAsString(Request.Params["BackRemark"]);

            UserInfo userinfo=new UserInfo{
                UserId=UserId,
                CompanyId=CompanyId,
                GroupId=GroupId,
                Upri=UPri,
                Dep=Dep,
                basecondtions=GetBaseCmp()
            };
            string message=DNApproveLoop.DnApproveBack(uid,backremark,userinfo);
            return Json(new { message = message });
        }
        #endregion

        #region DN管理—— 退回DN
        public ActionResult ReturnDn()
        {
            string returnMsg = "";
            string DnNos = Request.Params["pushdata"];
            DnNos = DnNos.Trim(',');
            string dntypes = Request.Params["dntypes"];
            dntypes = dntypes.Trim(',');
            string[] dnitems = DnNos.Split(',');
            string[] typeitems = dntypes.Split(',');
            MixedList mlist = new MixedList();
            //写一笔资料到异常
            for (int i = 0; i < dnitems.Length; i++)
            {
                string dnitem = dnitems[i];
                EditInstruct ei = new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
                string uid = Guid.NewGuid().ToString();
                ei.Put("U_ID", uid);
                ei.Put("U_FID", uid);
                ei.Put("JOB_NO", dnitem);
                ei.PutExpress("SEQ_NO", string.Format("(select count(*)+1 from TMEXP where JOB_NO ={0})", SQLUtils.QuotedStr(dnitem)));
                ei.Put("EXP_TEXT", @Resources.Locale.L_DNManageController_Controllers_106 + UserId + @Resources.Locale.L_DNManage_Controllers_237);
                ei.PutDate("CREATE_DATE", DateTime.Now);
                ei.Put("CREATE_BY", UserId);
                mlist.Add(ei);

                //发送Mail
                string dntype = Prolink.Math.GetValueAsString(typeitems[i]);
                returnMsg = SendCancelMail(dntype, dnitem);
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_DNManageController_Controllers_107;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg });
                }
            }
            return Json(new { message = returnMsg });
        }
        #endregion

        #region DN管理—修改DN
        public ActionResult CancelDN()
        {
            string returnMsg = "";
            string DnNos = Request.Params["pushdata"];
            string flag = Prolink.Math.GetValueAsString(Request.Params["cancelflag"]);
            DnNos = DnNos.Trim(',');
            string approvetypes = Request.Params["approvetypes"];
            approvetypes = approvetypes.Trim(',');
            string[] dnitems = DnNos.Split(',');
            string[] typeitems = approvetypes.Split(',');
            string statusmessage="";
            for (int i=0;i<dnitems.Length;i++)
            {
                string dnitem=dnitems[i];
                string typeitem = typeitems[i];
                statusmessage=CheckCancelDN(dnitem);
                if (!string.IsNullOrEmpty(statusmessage))
                {
                    returnMsg += statusmessage;
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
                DataTable dt=DNInfoCheck.GetDNDataByDnNo(dnitem);
                if (dt.Rows.Count <= 0)
                {
                    returnMsg += dnitem+" No Valide Data";
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
                statusmessage=DNInfoCheck.CheckDNApproveStatus(Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]));
                if (!string.IsNullOrEmpty(statusmessage))
                {
                    returnMsg += statusmessage;
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
                statusmessage = DNInfoCheck.CheckSMStatus(dnitem, Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]));
                if (!string.IsNullOrEmpty(statusmessage))
                {
                    returnMsg += statusmessage;
                    return Json(new { message = returnMsg, IsOk = "N" });
                }

                typeitem=Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_TYPE"]);

                if (string.IsNullOrEmpty(flag))
                {
                    returnMsg += CancelDnItem(returnMsg, dnitem, typeitem, dt.Rows[0]);
                }
                else if (flag.Equals("A"))//不出货審就此停住(此DN為作廢)
                {
                    returnMsg += CancelDnItemByA(returnMsg, dnitem);
                }
                else if (flag.Equals("B"))
                {
                    returnMsg += CancelDnItemByB(returnMsg, dnitem, typeitem);
                }
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        private string CancelDnItemByA(string returnMsg, string dnitem )
        {
            MixedList mlist = new MixedList();
            int count = OperationUtils.GetValueAsInt(string.Format("select count (*)+1 from TMEXP WHERE JOB_NO LIKE '%{0}%'", dnitem), Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct Tei = new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
            string uid = Guid.NewGuid().ToString();
            Tei.Put("SEQ_NO", count);
            Tei.Put("U_ID", uid);
            Tei.Put("U_FID", uid);
            Tei.Put("JOB_NO", dnitem);
            Tei.Put("EXP_TEXT", @Resources.Locale.L_BookingAction_Controllers_152 + ":" + UserId + @Resources.Locale.L_DNManage_Controllers_PerformsA);
            Tei.PutDate("CREATE_DATE", DateTime.Now);
            Tei.Put("CREATE_BY", UserId);
            Tei.PutDate("WR_DATE", DateTime.Now);
            Tei.Put("WR_ID", UserId);
            Tei.Put("EXP_OBJ", UserId);
            Tei.Put("EXP_TYPE", "DN");
            Tei.Put("EXP_REASON", "DNB");
            Tei.Put("GROUP_ID", GroupId);
            Tei.Put("CMP", CompanyId);
            mlist.Add(Tei);

            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("DN_NO", dnitem);
            ei.Put("STATUS", "V");    //将DN状态修改为取消
            mlist.Add(ei);
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg += @Resources.Locale.L_DNManageController_Controllers_108 + dnitem + @Resources.Locale.L_DNManage_Controllers_238;
                }
                catch (Exception ex)
                {
                    returnMsg += ex.ToString();
                }
            }
            return returnMsg;
        }

        private string CancelDnItemByB(string returnMsg, string dnitem, string typeitem)
        {
            MixedList mlist = new MixedList();
            //发送签核退回Mail
            returnMsg += SendCancelMail(Prolink.Math.GetValueAsString(typeitem), dnitem);
            int count = OperationUtils.GetValueAsInt(string.Format("select count (*)+1 from TMEXP WHERE JOB_NO LIKE '%{0}%'",dnitem), Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
            string uid = Guid.NewGuid().ToString();
            ei.Put("SEQ_NO", count);
            ei.Put("U_ID", uid);
            ei.Put("U_FID", uid);
            ei.Put("JOB_NO", dnitem);
            ei.Put("EXP_TEXT", @Resources.Locale.L_BookingAction_Controllers_152 + ":" + UserId + @Resources.Locale.L_DNManage_Controllers_PerformsB);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("CREATE_BY", UserId);
            ei.PutDate("WR_DATE", DateTime.Now);
            ei.Put("WR_ID", UserId);
            ei.Put("EXP_OBJ", UserId);
            ei.Put("EXP_TYPE", "DN");
            ei.Put("EXP_REASON", "DNT");
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", CompanyId);
            mlist.Add(ei);

            ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("DN_NO", dnitem);
            ei.Put("APPROVE_BACK", "Y");    //启动签核退回标志
            ei.Put("STATUS", DNInfoCheck.STA_H);          //状态修改暂缓出货
            mlist.Add(ei);
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg += @Resources.Locale.L_DNManageController_Controllers_108 + dnitem + @Resources.Locale.L_DNManage_Controllers_240;
                }
                catch (Exception ex)
                {
                    returnMsg += ex.ToString();
                }
            }
            return returnMsg;
        }

        public void WriteToTmexp(string dnitem, MixedList mlist,string type)
        {
            int count = OperationUtils.GetValueAsInt(string.Format("select count (*)+1 from TMEXP WHERE JOB_NO LIKE '%{0}%'", dnitem), Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
            string uid = Guid.NewGuid().ToString();
            ei.Put("SEQ_NO", count);
            ei.Put("U_ID", uid);
            ei.Put("U_FID", uid);
            ei.Put("JOB_NO", dnitem);
            ei.Put("EXP_TEXT", @Resources.Locale.L_BookingAction_Controllers_152 + ":" + UserId + @Resources.Locale.L_DNManage_Controllers_PerformsV);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("CREATE_BY", UserId);
            ei.PutDate("WR_DATE", DateTime.Now);
            ei.Put("WR_ID", UserId);
            ei.Put("EXP_OBJ", UserId);
            ei.Put("EXP_TYPE", "DN");
            ei.Put("EXP_REASON", type);
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", CompanyId);
            mlist.Add(ei);
        }
        private string CancelDnItem(string returnMsg, string dnitem, string typeitem,DataRow dnRow)
        {
            MixedList mlist = new MixedList();
            //发送签核退回Mail
            returnMsg += SendCancelMail(Prolink.Math.GetValueAsString(typeitem), dnitem);
            EditInstruct ei = new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
            int count = OperationUtils.GetValueAsInt(string.Format("select count (*)+1 from TMEXP WHERE JOB_NO LIKE '%{0}%'", dnitem), Prolink.Web.WebContext.GetInstance().GetConnection());
            string uid = Guid.NewGuid().ToString();
            ei.Put("SEQ_NO", count);
            ei.Put("U_ID", uid);
            ei.Put("U_FID", uid);
            ei.Put("JOB_NO", dnitem);
            ei.Put("EXP_TEXT", @Resources.Locale.L_BookingAction_Controllers_152 + ":" + UserId + @Resources.Locale.L_DNManage_Controllers_PerformsM);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("CREATE_BY", UserId);
            ei.PutDate("WR_DATE", DateTime.Now);
            ei.Put("WR_ID", UserId);
            ei.Put("EXP_OBJ",UserId);
            ei.Put("EXP_TYPE", "DN");
            ei.Put("EXP_REASON", "DNC");
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", CompanyId);
            mlist.Add(ei);

            ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("DN_NO", dnitem);
            ei.Put("APPROVE_BACK", "N");    //启动签核退回标志修改
            ei.Put("APPROVE_TO", "A");    //状态修改为申请者
            ei.Put("APPROVE_USER", "");
            ei.Put("STATUS", "D");
            mlist.Add(ei);

            string approveType=Prolink.Math.GetValueAsString(dnRow["APPROVE_TYPE"]);

            mlist.Add(DNInfoCheck.GetApproveRdVoidEI(dnitem, approveType));
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg += @Resources.Locale.L_DNManageController_Controllers_108 + dnitem + @Resources.Locale.L_DNFlowManage_Views_349;
                }
                catch (Exception ex)
                {
                    returnMsg += ex.ToString();
                }
            }
            return returnMsg;
        }

        private string CheckCancelDN(string dnno)
        {
            string sql = string.Format(@"SELECT STATUS FROM SMDN WHERE DN_NO ={0}",SQLUtils.QuotedStr(dnno));
            string status=OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            switch(status)
            {
                case "B":
                     return @Resources.Locale.L_DNManageController_Controllers_109;
            }
            return string.Empty;
        }

        private string  SendCancelMail(string approvecode,string dnno)
        {
            string returnvalue = string.Empty;
            string sql = string.Format("SELECT * FROM APPROVE_RECORD WHERE REF_NO={0} AND APPROVE_CODE={1} AND STATUS='1'", SQLUtils.QuotedStr(dnno), SQLUtils.QuotedStr(approvecode));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string approverole="";
            IMailTemplateParse parse = new DefaultMailParse();
            DataTable maindt = DNInfoCheck.GetDNDataByDnNo(dnno);
            string dnuid = Prolink.Math.GetValueAsString(maindt.Rows[0]["U_ID"]);
            foreach (DataRow dr in dt.Rows)
            {
                approverole = Prolink.Math.GetValueAsString(dr["ROLE"]);
                sql = string.Format(@" SELECT * FROM APPROVE_FLOW_D WHERE  BACK_FLAG='Y' AND APPROVE_CODE={0} AND ROLE={1}", SQLUtils.QuotedStr(approvecode), SQLUtils.QuotedStr(approverole));
                DataTable dtd = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                 if (dtd.Rows.Count == 0)
                {
                    continue ;
                }
                 string uuid = Prolink.Math.GetValueAsString(dtd.Rows[0]["GU_ID"]);
                try
                {
                    string subject = "DN No:" + dnno +  @Resources.Locale.L_GateReserveSetup_Scripts_276;
                    returnvalue += DNApproveLoop.AddToNoticeDN(uuid, GetBaseCmp(), subject, UserId, GroupId, CompanyId, Dep, dnuid);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return returnvalue;
        }

        public bool ApproveBackLoop(string dnno,string approvecode)   //签核回退机制
        {
            //暂停出货，将DN修改为DN_H
            MixedList mlist = new MixedList();
            string updateapproverecode = string.Format("UPDATE APPROVE_RECORD SET REF_NO=REF_NO+'_H' WHERE APPROVE_CODE={0} AND REF_NO={1}", SQLUtils.QuotedStr(dnno),SQLUtils.QuotedStr(approvecode));   
            string updatesmdn=string.Format("UPDATE SMDN SET DN_NO=DN_NO+'_H' WHERE DN_NO={0}",SQLUtils.QuotedStr(dnno));  
            string updatesmdnp=string.Format("UPDATE SMDNP SET DN_NO=DN_NO+'_H' WHERE DN_NO={0}",SQLUtils.QuotedStr(dnno)); 
            string updatesmdnpt=string.Format("UPDATE SMDNPT SET DN_NO=DN_NO+'_H' WHERE DN_NO={0}",SQLUtils.QuotedStr(dnno)); 
            string updatesmdnd=string.Format("UPDATE SMDND SET DN_NO=DN_NO+'_H' WHERE DN_NO={0}",SQLUtils.QuotedStr(dnno));
            string updatesmdns = string.Format("UPDATE SMDNS SET DN_NO=DN_NO+'_H' WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            mlist.Add(updateapproverecode);
            mlist.Add(updatesmdn);
            mlist.Add(updatesmdnp);
            mlist.Add(updatesmdnpt);
            mlist.Add(updatesmdnd);
            mlist.Add(updatesmdns);
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }
        

        public string GetMailBody(string group_id, string notify_format = "STT")
        {
            string sql = string.Format("SELECT MT_NAME,MT_CONTENT FROM TKPMT WHERE GROUP_ID={0} AND MT_TYPE={1}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(notify_format));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return "";
            }
            return Prolink.Math.GetValueAsString(dt.Rows[0]["MT_CONTENT"].ToString());
        }
        #endregion

        #region
        public ActionResult QuadrupleSingle()
        {
            string returnmsg = string.Empty;
            string dnno = Request.Params["dnno"];
            DnHandel dnhandel = new DnHandel();
            if (dnno.EndsWith("_F"))
            {
                returnmsg = @Resources.Locale.L_DNManage_Controllers_242;
                return Json(new { message = returnmsg, DnNo = dnno });
            }
            string dncondition = dnno + "_F%";
            string sql = string.Format("SELECT COUNT(1) FROM SMDN WHERE DN_NO LIKE {0}", SQLUtils.QuotedStr(dncondition));
            int count=OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            int seqno = count + 1;
            string newdnno = dnno + "_F" + seqno;

            returnmsg = dnhandel.Quadruple(dnno, newdnno);
            if (string.IsNullOrEmpty(returnmsg))
            {
                returnmsg = @Resources.Locale.L_DNManage_Controllers_243;
                dnno = newdnno;
            }
            else
            {
                newdnno = dnno;
            }
            return Json(new { message = returnmsg, DnNo = dnno });
        }
        #endregion


        #region 分批出货
        public ActionResult PartialQuery(string id = null, string ufid = null)
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            if (ufid == null)
            {
                ufid = id;
            }
            string conditions = string.Format(" U_FID={0}", SQLUtils.QuotedStr(ufid));

            DataTable dt = ModelFactory.InquiryData("*", "SMDND", conditions, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmdndModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult PartialUpdate()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string returnMessage = "success";
            string changeData = Request.Params["changedData"];
            int sumqty = Prolink.Math.GetValueAsInt(Request.Params["sumqty"]);
            string DnUid = Prolink.Math.GetValueAsString(Request.Params["DnUid"]);
            List<Dictionary<string, object>> smdndData = new List<Dictionary<string, object>>();

            string sql = string.Format("SELECT QTY FROM SMDN WHERE U_ID={0}", SQLUtils.QuotedStr(DnUid));
            int dnqty = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dnqty < sumqty)
            {
                returnMessage = @Resources.Locale.L_DNManageController_Controllers_110;
                return Json(new { message = returnMessage });
            }
            
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdndModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            //修改
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
                    sql = "SELECT * FROM SMDND WHERE U_FID="+SQLUtils.QuotedStr(DnUid) + " ORDER BY CALL_DATE ASC";
                    DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    decimal tqty = 0, oqty=0;
                    if (dnDt.Rows.Count > 0)
                    {
                        foreach (DataRow item in dnDt.Rows)
                        {
                            decimal qty = Prolink.Math.GetValueAsDecimal(item["QTY"]);
                            string UId = Prolink.Math.GetValueAsString(item["U_ID"]);
                            tqty = tqty + qty;
                            sql = "UPDATE SMDND SET AQTY=" + tqty + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                            Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                    }
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    DataTable dt = ModelFactory.InquiryData("*", "SMDND", "U_FID='" + DnUid + "'", "CALL_DATE ASC", pageIndex, pageSize, ref recordsCount);
                    smdndData = ModelFactory.ToTableJson(dt, "SmdndModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, smdndData = smdndData });
        }

        #endregion

        #region 作废
        public ActionResult VoidDN()
        {
            string returnMsg = "";
            string dnno = Request.Params["dnno"];
            MixedList mlist = new MixedList();
            returnMsg = DNInfoCheck.CheckSMStatus(dnno);
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return Json(new { message = returnMsg, IsOk = "N" });
            }
            if (!string.IsNullOrEmpty(dnno))
            {
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("DN_NO", dnno);
                ei.Put("APPROVE_BACK", "Y");//启动签核退回机制
                ei.Put("STATUS", "V");
                ei.Put("MODIFY_BY", UserId);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
                mlist.Add(ei);
            }
            if (mlist.Count > 0)
            {
                try
                {
                    WriteToTmexp(dnno, mlist,"DNV");
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_DNManage_Controllers_233;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
            }
            else
            {
                return Json(new { message = "No Valid Data!", IsOk = "N" });
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public ActionResult DNViewCancelB()
        {
            string returnmsg = string.Empty;
            string dnno = Request.Params["dnno"];
            DataTable dt = DNInfoCheck.GetDNDataByDnNo(dnno);
            if (dt.Rows.Count <= 0)
            {
                return Json(new { message = "No Valid Data" });
            }
            string statusmessage = DNInfoCheck.CheckDNApproveStatus(Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]));
            if (!string.IsNullOrEmpty(statusmessage))
            {
                return Json(new { message = statusmessage });
            }
            statusmessage = DNInfoCheck.CheckSMStatus(dnno, Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]));
            if (!string.IsNullOrEmpty(statusmessage))
            {
                return Json(new { message = statusmessage });
            }

            string approvetype = Prolink.Math.GetValueAsString(dt.Rows[0]["APPROVE_TYPE"]);
            string status=Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            
            returnmsg=CancelDnItemByB(returnmsg, dnno, approvetype);
            return Json(new { message = returnmsg });
        }
        #endregion

        #region 过账
        public ActionResult PostingSAP()
        {
            string returnMsg = "";
            string location = Request["location"];
            string DnNos = Request.Params["pushdata"];
            DnNos = DnNos.Trim(',');
            string[] dnitems = DnNos.Split(',');

            string uids = Request.Params["uids"];
            uids = uids.Trim(',');
            string[] uiditems = uids.Split(',');
            MixedList mlist = new MixedList();
            string dnindex = string.Empty;
            string uidindex = string.Empty;
            for (int i = 0; i < uiditems.Length; i++)
            {
                dnindex = dnitems[i];
                uidindex = uiditems[i];
                try
                {
                    //调用过账Edi
                    string sql = string.Format("SELECT POST_FLAG,SAP_ID,DN_NO FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(dnindex));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count <= 0)
                    {
                        continue;
                    }
                    dnindex = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
                    string sapId = Prolink.Math.GetValueAsString(dt.Rows[0]["SAP_ID"]);
                    string postflag = Prolink.Math.GetValueAsString(dt.Rows[0]["POST_FLAG"]);
                    if (postflag.Equals("Y"))
                    {
                        returnMsg = @Resources.Locale.L_DNManageController_Controllers_111 + dnindex +  @Resources.Locale.L_DNManage_Controllers_247;
                        continue;
                    }

                    Business.TPV.Import.DeliveryPostingManager dpManager = new Business.TPV.Import.DeliveryPostingManager();
                    Business.TPV.Import.DeliveryPostingInfo dpInfo = new Business.TPV.Import.DeliveryPostingInfo();
                    dpInfo.DNNO = dnindex;
                    dpInfo.GoodsMovementDate = DateTime.Now;
                    Business.TPV.RFC.DPResultInfo result = null;
                    bool isSucceed = dpManager.TryPostingDate(sapId, dpInfo, out result, location);
                    if (isSucceed)
                    {
                        EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("DN_NO", dnindex);
                        ei.PutKey("SAP_ID", sapId);
                        ei.Put("POST_FLAG", "Y");
                        mlist.Add(ei);

                        if (mlist.Count > 0)
                        {
                            try
                            {
                                int[] results = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                                returnMsg = @Resources.Locale.L_DNManageController_Controllers_111 + dnindex +  @Resources.Locale.L_DNManage_Controllers_248;
                            }
                            catch (Exception ex)
                            {
                                returnMsg = ex.ToString();
                            }
                        }
                    }
                    else
                    {
                        returnMsg += @Resources.Locale.L_DNManageController_Controllers_112;
                        if (result != null)
                            returnMsg += result.MsgText;
                    }
                }
                catch (Exception ex)
                {
                    returnMsg += dnindex + ":"+@Resources.Locale.L_DNManageController_Controllers_113 + ex.ToString() + "\n";
                }
            }
            return Json(new { message = returnMsg });
        }

        #endregion

        public ActionResult GetUidBySmId()
        {
            string uid = string.Empty;
            string trantype = string.Empty;
            string shipmentid = Request.Params["shipmentid"];
            string sql = string.Format("SELECT TRAN_TYPE,U_ID FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smdt.Rows.Count > 0)
            {
                uid = smdt.Rows[0]["U_ID"].ToString();
                trantype = smdt.Rows[0]["TRAN_TYPE"].ToString();
            }
            return Json(new { uid = uid, trantype = trantype });
        }

        public ActionResult ReloadDN()
        {
            string dnNo = Request.Params["DnNo"];
            string sapId = Request.Params["SapId"];
            string location = Request.Params["location"];
            if (string.IsNullOrEmpty(dnNo) || string.IsNullOrEmpty(sapId))
                return Json(new { errMsg = @Resources.Locale.L_DNManageController_Controllers_114 });
            Business.TPV.Import.DNManager dnManager = new Business.TPV.Import.DNManager();
            Business.Service.ResultInfo result = dnManager.ImportDNForSAP(sapId, dnNo,""); 
            if (!result.IsSucceed)
                return Json(new { errMsg = result.Description });
            return Json(new { });
        }

        public ActionResult ReloadQuantity()
        { 
            string dnNo = Request.Params["DnNo"];
            string sapId = Request.Params["SapId"];
            string location = Request.Params["location"];

            if (string.IsNullOrEmpty(dnNo) || string.IsNullOrEmpty(sapId))
                return Json(new { errMsg = @Resources.Locale.L_DNManageController_Controllers_114 });
            DnHandel dh = new DnHandel();
            DataTable dt = dh.GetVSMDNByDnNo(dnNo);
            if(dt.Rows.Count<=0) return Json(new { errMsg = "No valid Data!" });
            string shipmentid=dt.Rows[0]["SHIPMENT_ID"].ToString();
            string status = dt.Rows[0]["SM_STATUS"].ToString();
            if ("P".Equals(status)) return Json(new { errMsg = @Resources.Locale.L_DNManageController_Controllers_115+ @Resources.Locale.L_DNManage_Controllers_255 });
            if ("O".Equals(status)) return Json(new { errMsg = @Resources.Locale.L_DNManageController_Controllers_116 +@Resources.Locale.L_DNManage_Controllers_255 });
            if ("H".Equals(status)) return Json(new { errMsg = @Resources.Locale.L_DNManageController_Controllers_117 +@Resources.Locale.L_DNManage_Controllers_255 });
            Business.TPV.Import.DNManager m = new Business.TPV.Import.DNManager();
            Business.Service.ResultInfo result = m.ImportDNForSAP(sapId, dnNo, "", Business.TPV.Import.DNImportModes.ReloadQuantity);
            if (!result.IsSucceed)
                return Json(new { errMsg = result.Description });
            UserInfo userinfo = new UserInfo();
            userinfo.Dep = Dep;
            userinfo.CompanyId = CompanyId;
            userinfo.GroupId = GroupId;
            userinfo.UserId = UserId;
            string message = dh.DnCombineSM(dnNo, shipmentid, status, userinfo);
            if(string.IsNullOrEmpty(message))
                return Json(new { errMsg = message });
            return Json(new { });
        }

        #region 并柜与取消并柜
        public ActionResult SpellCTN()
        {
            string returnMsg = "";
            string firstdnno = Request.Params["dnno"];
            string DnNos = Request.Params["pushdata"];
            string firstdnitems = firstdnno;

            if (!string.IsNullOrEmpty(firstdnno))
            {
                string sql = string.Format("SELECT COMBINE_INFO FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(firstdnno));
                firstdnitems = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                firstdnitems = firstdnitems.Trim(',');
                if (string.IsNullOrEmpty(firstdnitems))
                {
                    firstdnitems = firstdnno;
                }
            }
            else
            {
                firstdnitems = firstdnno;
            }
            
            DnNos=DnNos.Trim(',');
            DnNos += "," + firstdnitems;
            string[] dnitems = DnNos.Split(',');

            List<string> list = new List<string>();
            foreach (string item in dnitems)
            {
                if (list.Contains(item))
                    continue;
                list.Add(item);
            }
            DnNos = string.Join(",", list);

            MixedList mlist = new MixedList();

            foreach (string dnitem in dnitems)
            {
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.Put("COMBINE_INFO", DnNos);
                ei.PutDate("COMBINE_DATE", DateTime.Now);
                ei.Put("COMBINE_BY", UserId);
                ei.PutKey("DN_NO", dnitem);
                mlist.Add(ei);
            }

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_DNManage_Controllers_259;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg });
                }
            }
            return Json(new { message = returnMsg, CombineInfo = DnNos });
        }

        public ActionResult CancelSpellCTN()
        {
            string returnMsg = "";
            string DnNos = Request.Params["pushdata"];
            DnNos = DnNos.Trim(',');
            string dnnoitem=string.Empty;
            string[] dnitems = DnNos.Split(',');
            MixedList mlist = new MixedList();
             
            foreach (string dnitem in dnitems)
            {
                string sql = string.Format("SELECT COMBINE_INFO,STATUS FROM SMDN WHERE DN_NO ={0}", SQLUtils.QuotedStr(dnitem));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count <= 0) return Json(new { message = string.Format(@Resources.Locale.L_DNManageController_Controllers_1181, dnitem) });
                string combins = dt.Rows[0]["COMBINE_INFO"].ToString();
                string status = dt.Rows[0]["STATUS"].ToString();
                if ("B".Equals(status)) return Json(new { message = string.Format(@Resources.Locale.L_DNManageController_Controllers_1191, dnitem) }); 
                string[] combin = combins.Split(',');
                for (int i = 0; i < combin.Length; i++)
                {
                    dnnoitem = combin[i];
                    if (!string.IsNullOrEmpty(combin[i]))
                    {
                        EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                        ei.Put("COMBINE_INFO", "");
                        ei.PutDate("COMBINE_DATE", "");
                        ei.Put("COMBINE_BY", "");
                        ei.PutKey("DN_NO", dnnoitem);
                        mlist.Add(ei);
                    }
                }
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_DNManage_Controllers_262;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg });
                }
            }
            else
            {
                return Json(new { message = @Resources.Locale.L_DNManage_Controllers_263 });
            }
            return Json(new { message = returnMsg });
        }

        public ActionResult FLowSpellCTN()
        {
            string DnNos = Request.Params["pushdata"];
            string firstdnitems = string.Empty;
            DnNos = DnNos.Trim(',');
            string[] dnitems = DnNos.Split(',');
            string returnMsg= CombineCTN(dnitems);
            if(string.IsNullOrEmpty(returnMsg))
                return Json(new { message =  @Resources.Locale.L_DNManage_Controllers_259, IsOk = "Y" });
            return Json(new { message = returnMsg, IsOk = "N" });
        }

        private string CombineCTN(string[] dnitems)
        {
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT PORT_DEST,TRAN_TYPE,status,SHIPMENT_ID,dn_no,COMBINE_INFO FROM SMDN WHERE DN_NO IN {0} ",
                SQLUtils.Quoted(dnitems)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string portdest = string.Empty;
            string trantype = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                if (i == 0)
                {
                    portdest = dr["PORT_DEST"].ToString();
                    trantype = dr["TRAN_TYPE"].ToString();
                }
                else
                {
                    if (!portdest.Equals(dr["PORT_DEST"].ToString()) || !trantype.Equals(dr["TRAN_TYPE"].ToString()))
                    {
                        return  @Resources.Locale.L_DNManageController_Controllers_121;
                    }
                }
                if (!string.IsNullOrEmpty(dr["SHIPMENT_ID"].ToString()))
                {
                    return string.Format("{0}{1}{2}", dr["DN_NO"].ToString(),
                    @Resources.Locale.L_BaseBookingSetup_Scripts_164, @Resources.Locale.L_DNManage_Controllers_265);
                }
                if (!"D".Equals(dr["status"].ToString()))
                {
                    return string.Format("{0}{1}{2}", dr["DN_NO"].ToString(),
                    @Resources.Locale.L_RQQuery_Status, @Resources.Locale.L_InsuranceManage_StatusI);
                }
            }
            List<string> list = new List<string>();
            foreach (string item in dnitems)
            {
                if (list.Contains(item))
                    continue;
                list.Add(item);
            }
            string DnNos = string.Join(",", list);

            MixedList mlist = new MixedList();

            foreach (string dnitem in dnitems)
            {
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.Put("COMBINE_INFO", DnNos);
                ei.PutDate("COMBINE_DATE", DateTime.Now);
                ei.Put("COMBINE_BY", UserId);
                ei.PutKey("DN_NO", dnitem);
                mlist.Add(ei);
            }

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            return string.Empty;
        }

        public ActionResult FlowCancelSpellCTN()
        {
            string returnMsg = "";
            string DnNos = Request.Params["pushdata"];
            DnNos = DnNos.Trim(',');
            string dnnoitem = string.Empty;
            string[] dnitems = DnNos.Split(',');
            MixedList mlist = new MixedList();
            foreach (string dnitem in dnitems)
            {
                string sql = string.Format("SELECT COMBINE_INFO,STATUS FROM SMDN WHERE DN_NO ={0}", SQLUtils.QuotedStr(dnitem));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count <= 0) return Json(new { message = string.Format(@Resources.Locale.L_DNManageController_Controllers_1181, dnitem) });
                string combins = dt.Rows[0]["COMBINE_INFO"].ToString();
                string status = dt.Rows[0]["STATUS"].ToString();
                if ("B".Equals(status)) return Json(new { message = string.Format(@Resources.Locale.L_DNManageController_Controllers_1191, dnitem) });
                string[] combin = combins.Split(',');
                for (int i = 0; i < combin.Length; i++)
                {
                    dnnoitem = combin[i];
                    if (!string.IsNullOrEmpty(combin[i]))
                    {
                        EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                        ei.Put("COMBINE_INFO", "");
                        ei.PutDate("COMBINE_DATE", "");
                        ei.Put("COMBINE_BY", "");
                        ei.PutKey("DN_NO", dnnoitem);
                        mlist.Add(ei);
                    }
                }
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_DNManage_Controllers_262;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg,IsOk="N" });
                }
            }
            else
            {
                return Json(new { message = @Resources.Locale.L_DNManage_Controllers_263, IsOk = "N" });
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }
        #endregion

        //签核明细
        public ActionResult GetApproveInfo()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string sql = string.Format("SELECT  APPROVE_RECORD.*, ISNULL((SELECT TOP 1 APPROVE_NAME FROM APPROVE_FLOW_M WHERE APPROVE_FLOW_M.APPROVE_CODE=APPROVE_RECORD.APPROVE_CODE  AND APPROVE_FLOW_M.CMP_ID={1}),APPROVE_RECORD.APPROVE_CODE) AS APPROVE_CODENAME," +
                "ISNULL((SELECT TOP 1 GROUP_DESCP FROM APPROVE_FLOW_D WHERE APPROVE_FLOW_D.APPROVE_CODE=APPROVE_RECORD.APPROVE_CODE AND APPROVE_FLOW_D.ROLE= APPROVE_RECORD.ROLE AND APPROVE_FLOW_D.CMP_ID={1}),APPROVE_RECORD.ROLE) AS APPROVE_ROLE" +
                " FROM APPROVE_RECORD WHERE REF_NO={0} ORDER BY VOID_LOOP DESC, APPROVE_CODE DESC, CAST(APPROVE_LEVEL AS INT) ASC", SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(CompanyId));
            DataTable groupDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(groupDt, "ApproveRecordModel"));
        }

        public ActionResult InitiateBooking()
        {
            string returnMsg = "";
            string DnNos = Request.Params["pushdata"];
            string uids = Request.Params["uids"];

            DnNos = DnNos.Trim(',');
            uids = uids.Trim(',');
            string[] dnitems = DnNos.Split(',');
            string[] uiditems = uids.Split(',');
            TransferBooking tb = new TransferBooking();
            string dnindex = string.Empty;
            string uidindex = string.Empty;
            for (int i = 0; i < uiditems.Length; i++)
            {
                dnindex = dnitems[i];
                uidindex = uiditems[i];
                if (CheckTranType(uidindex))
                {
                    return Json(new { message = @Resources.Locale.L_DNManageController_tip1, IsOk = "N" });
                }
                if (string.IsNullOrEmpty(uidindex))
                {
                    continue;
                }
                try
                {
                    if (CheckBooking(uidindex))
                    {
                        returnMsg = dnindex + @Resources.Locale.L_DNManageController_Controllers_122 ;
                        return Json(new { message = returnMsg, IsOk = "N" });
                    }
                    returnMsg = tb.SaveToBooking(uidindex,UserId);
                    if (!string.IsNullOrEmpty(returnMsg))
                    {
                        return Json(new { message = returnMsg, IsOk = "N" });
                    }
                }
                catch (Exception ex)
                {
                    returnMsg += dnindex + ":"+@Resources.Locale.L_DNManageController_Controllers_123 + ex.ToString() + "\n";
                    return Json(new { message = returnMsg });
                }
            }
            returnMsg += DnNos + ":"+@Resources.Locale.L_DNManageController_Controllers_124;
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public bool CheckTranType(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return false;
            string sql = string.Format(@"SELECT SMDN.* FROM SMDN WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                    if (trantype.Equals("A"))
                    {
                        string _sql = string.Format("SELECT COUNT(*) FROM SMCUFT WHERE U_FID ={0}", SQLUtils.QuotedStr(uid));
                        int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (count > 0)
                            return true;
                    }
                }
            }
            return false;
        }
        public ActionResult DNCheckBooking()
        {
            string returnMsg = string.Empty;
            string uid = Request.Params["uid"];
            string sql = string.Format(@" SELECT SMSM.MODIFY_BY,SMSM.STATUS,SMSM.U_ID,SMSM.TORDER FROM (SELECT SMDN.* FROM SMDN WHERE U_ID={0})SD 
                        LEFT JOIN SMSM ON SMSM.SHIPMENT_ID=SD.SHIPMENT_ID", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return Json(new { message = @Resources.Locale.L_DNManage_Controllers_270, IsOk = "N", }); 
            string torder = string.Empty;
            torder = dt.Rows[0]["TORDER"].ToString();
            if (CheckBooking(uid))
            {
                return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_125, IsOk = "N", Torder = torder });
            }
            return Json(new { message = returnMsg, IsOk = "Y",Torder = torder }); 
        }

        public bool CheckBooking(string smdnuid)
        {
            string sql = string.Format(@" SELECT SMSM.MODIFY_BY,SMSM.STATUS,SMSM.U_ID FROM (SELECT SMDN.* FROM SMDN WHERE U_ID={0})SD 
                        LEFT JOIN SMSM ON SMSM.SHIPMENT_ID=SD.SHIPMENT_ID", SQLUtils.QuotedStr(smdnuid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return false;
            string status = dt.Rows[0]["STATUS"].ToString();
            string modifyby = dt.Rows[0]["MODIFY_BY"].ToString();
            if (!string.IsNullOrEmpty(modifyby)) return true;
            string shipmentuid= dt.Rows[0]["U_ID"].ToString();
            if (string.IsNullOrEmpty(status)) return false;
            if ("A".Equals(status))
            {
                sql = string.Format(@"SELECT TOP 1 CASE WHEN FCOUNT > 0 THEN 'Y' ELSE '' END 
                FROM V_COUNT_EDOC WHERE V_COUNT_EDOC.JOB_NO={0}", SQLUtils.QuotedStr(shipmentuid));
                string isdoc = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if ("Y".Equals(isdoc))
                {
                    return true;
                }
                return false;
            }
            return true;
            
        }

        public ActionResult InitiSMBDBooking()
        {
            string returnMsg = "";
            string DnNos = Request.Params["pushdata"];
            string uids = Request.Params["uids"];
            string dnno = Request.Params["DnNo"];
            TransferBooking tb = new TransferBooking();
            //U_ID,CMP,STN,DEP,U_EXT,U_EMAIL,U_PHONE 
            string sql=string.Format("SELECT * FROM SYS_ACCT WHERE U_ID={0} AND {1}",SQLUtils.QuotedStr(UserId), GetBaseDep());
            DataTable dt=OperationUtils.GetDataTable(sql, null,Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count<=0)return Json(new { message = @Resources.Locale.L_DNManage_Controllers_273, IsOk = "N" });
            string phone=Prolink.Math.GetValueAsString(dt.Rows[0]["U_PHONE"]);
            string uext=Prolink.Math.GetValueAsString(dt.Rows[0]["U_EXT"]);
            string []uexts=uext.Split('-');
            string partytel=phone;
            if(uexts.Length>=2)
                partytel+="-"+uexts[1];
            TransferUser userinfo = new TransferUser
            {
                UId = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]),
                Dep = Prolink.Math.GetValueAsString(dt.Rows[0]["DEP"]),
                Cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]),
                Stn = Prolink.Math.GetValueAsString(dt.Rows[0]["STN"]),
                UExt = uext,
                UPhone = phone,
                UEmail = Prolink.Math.GetValueAsString(dt.Rows[0]["U_EMAIL"]),
                PartyAttn = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]),
                PartyTel = partytel
            };
            returnMsg = tb.SaveSmbdToBook(uids, dnno, UserId, userinfo);
            return Json(new { message = returnMsg, IsOk = "N" });
        }

        #region Booking
        /// <summary>
        /// 订舱查询
        /// </summary>
        /// <returns></returns>
        public ActionResult ShippingBookingQueryData()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string subSql = "";
            string fiftySql = string.Empty;
            if (virCondition != "")
            {
                subSql=" AND EXISTS (SELECT *  FROM SMDNP,SMDN  WHERE  SMDNP.DN_NO = SMDN.DN_NO AND " + virCondition + " AND SMDN.SHIPMENT_ID=SMSM.SHIPMENT_ID )";
            }
            //if ((UPri != "G" && UPri !="U" ) && TCmp != "")
            //{
            //    subSql += string.Format(" AND STN in ( {0} ) ", TCmp);
            //}
            //return GetBootstrapData("SMSM", "1=1 " + subSql );
            return GetBootstrapData("SMSM", GetBookingCondition() + subSql);
        }

        public ActionResult ShippingBookingQueryDataFcl()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string subSql = "";
            string fiftySql = string.Empty;
            if (virCondition != "")
            {
                subSql = " AND EXISTS (SELECT *  FROM SMDNP,SMDN  WHERE  SMDNP.DN_NO = SMDN.DN_NO AND " + virCondition + " AND SMDN.SHIPMENT_ID=SMSM.SHIPMENT_ID )";
            }
            //if ((UPri != "G" && UPri !="U" ) && TCmp != "")
            //{
            //    subSql += string.Format(" AND STN in ( {0} ) ", TCmp);
            //}
            //return GetBootstrapData("SMSM", "1=1 " + subSql );
            return GetBootstrapData(@" (select *,(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO,
(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1,
(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO,
(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO,
(SELECT TOP 1 TARE_WEIGHT FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TARE_WEIGHT,
(SELECT TOP 1 GW FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_GW,
(SELECT TOP 1 TTL_VGM FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TTL_VGM from SMSM  ) SMSM", GetBookingCondition() + subSql, " *");
        }
        /// <summary>
        /// 叫车管理查询
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderCarQueryData()
        {
            string virCondition = ConvParam2SQL(Request.Params["virConditions"]);
            string subSql = "";
            string fiftySql = string.Empty;
            string realseCondition = string.Empty;
            if(virCondition == "")
            {
                if (PlantPri != "''")
                {
                    if (string.IsNullOrEmpty(PlantPri))
                    {
                        virCondition += " SMDN.PLANT in ( '' ) ";
                        realseCondition = " AND PLANT in ( '' )";
                    }
                    else
                    {
                        virCondition += string.Format(" SMDN.PLANT in ( {0} ) ", PlantPri);
                        realseCondition = string.Format(" AND PLANT in ( {0} ) ", PlantPri);
                    }
                }
            }
            else
            {
                if (PlantPri != "''")
                {
                    if (string.IsNullOrEmpty(PlantPri))
                    {
                        virCondition += " AND SMDN.PLANT in ( '' ) ";
                        realseCondition = " AND PLANT in ( '' )";
                    }
                    else
                    {
                        virCondition += string.Format(" AND SMDN.PLANT in ( {0} ) ", PlantPri);
                        realseCondition = string.Format(" AND PLANT in ( {0} ) ", PlantPri);
                    }
                }
            }

            if (virCondition != "")
            {
                subSql = " AND ((SMSM.DN_NO is null "+ realseCondition+") OR EXISTS (SELECT *  FROM SMDNP,SMDN  WHERE  SMDNP.DN_NO = SMDN.DN_NO AND " + virCondition + " AND SMDN.SHIPMENT_ID=SMSM.SHIPMENT_ID ))";
            }

            string fields = @"U_ID,WEEKLY,MONTH,YEAR,GROUP_ID,CMP,STN,DEP,SHIPMENT_ID,SHIPMENT_INFO,NO_DECL,STATUS
      ,BL_CHECK,CARRIER,CARRIER_NM,SCAC_CD,SVC_CONTACT,BL_WIN,SALES_WIN,BRG_TYPE,SIGN_BACK
      ,INCOTERM_CD,INCOTERM_DESCP,SERVICE_MODE,FRT_TERM,GOODS,MARKS
      ,INSTRUCTION,BOOKING_INFO,DIMENSION,HOUSE_NO,MASTER_NO,SO_NO,REF_NO
      ,BL_TYPE,BL_DATE,CLS_DATE,ETD,ETA,PICKUP_WMS,PICKUP_WMS_NM,PICKUP_WMS_DATE,RCV_DATE
      ,DLV_DOC_DATE,RLS_CNTR_DATE,RCV_DOC_DATE,CUT_PORT_DATE,PORT_DATE,CUSTOMS_DATE,PORT_RLS_DATE
      ,VESSEL1,VOYAGE1,ETD1,ETA1,VESSEL2,VOYAGE2,ETD2,ETA2,VESSEL3,VOYAGE3,ETD3,ETA3
      ,VESSEL4,VOYAGE4,ETD4,ETA4,POR_CD,POR_CNTY,POR_NAME,POL_CD,POL_CNTY,POL_NAME,VIA_CD,VIA_CNTY,VIA_NAME
      ,POD_CD,POD_CNTY,POD_NAME,DEST_CD,DEST_CNTY,DEST_NAME,QTY,QTYU,NW,GW,GWU,CBM
      ,PLT_NUM,CNT20,CNT40,CNT40HQ,CNT_TYPE,CNT_NUMBER,CUR,FREIGHT_AMT,INSURANCE_AMT,OPICKUP
      ,ODELIVERY,EXPORT_NO,EDECL_NO,APPROVE_NO,OEXPORTER,OEXPORTER_NM,OEXPORTER_ADDR,OIMPORTER
      ,OIMPORTER_NM,OIMPORTER_ADDR,DECL_DATE,DECL_RLS_DATE,BROKER_INSTR
      ,BROKER_INFO,TRAN_TYPE,CARGO_TYPE,PICKUP_PORT,REGION,STATE,DELIVERY_PORT,TRUCK_TYPE
      ,DRIVER,DRIVER_TEL,TRUCK_NO,LSP_NO,LSP_NM,EXPORT_CUR,OF_COST,OT_COST,CREATE_BY,CREATE_DEP
      ,CREATE_EXT,CREATE_DATE,IPICKUP,IDELIVERY,IMPORT_NO,CC_NO,IAPPROVE_NO,IEXPORTER,IEXPORTER_NM,IEXPORTER_ADDR
      ,IIMPORTER,IIMPORTER_NM,IIMPORTER_ADDR,CC_DATE,CC_RLS_DATE,CC_INSTR,CC_INFO,IMPORT_CUR
      ,IF_COST,IT_COST,ICREATE_BY,ICREATE_DEP,ICREATE_EXT,ICREATE_DATE
      ,DN_NO,ATD,ATA,REMARK,MODIFY_BY,MODIFY_DATE,PPOR_CD,PPOR_CNTY,PPOR_NAME,PPOL_CD
      ,PPOL_CNTY,PPOL_NAME,PPOD_CD,PPOD_CNTY,PPOD_NAME,PDEST_CD
      ,PDEST_CNTY,PDEST_NAME,PCNT20,PCNT40,PCNT40HQ,PCNT_TYPE,PCNT_NUMBER,PICKUP_CDATE,RETURN_CDATE
      ,CNTR_DESCP,COMBINE_INFO,DN_ETD,RECEIVE_DATE
      ,GVALUE,INSPECT,TRACK_WAY,DEEP_PROCESS,BAND_TYPE,PLT_NO,ETT,ATT,DTT,AQTY,DQTY
      ,FQTY,FDQTY,NDQTY,SEPARATE,CAR_TYPE,PORT,PORT_NM,PORT_CD,POST_CARGO_FLAG,INV_FLOW,CORDER,BORDER
      ,LOADING_FROM,LOADING_TO,SH_CD,SH_NM,CS_CD,CS_NM,PT_CD,PT_NM,DT_CD,DT_NM,PAYTERM_CD,PAYTERM_NM,BROKER_USER
      ,PKG_NUM,PKG_UNIT,PKG_UNIT_DESC,BOOKING_USER,PRODUCT_DATE,PROFILE_CD,AG_CD,AG_NM,WE_CD
      ,WE_NM,ZE_CD,ZE_NM,FC_CD,FC_NM,RE_CD,RE_NM,RO_CD,RO_NM,CAR_TYPE1
      ,CAR_TYPE2,CAR_QTY,CAR_QTY1,CAR_QTY2,TRANSACTE_MODE,UNICODE,COMBIN_SHIPMENT,COST_CENTER,PGW
      ,PCBM,BROKER_TIMES,BOOKING_TIMES,ATA_D,CUT_BL_DATE,LGOODS,BL_RMK,TRADE_TERM,TRADETERM_DESCP,BAND_DESCP
      ,BAND_CD,CENT_DECL,DRAFT_BL_WIN,BLNOTIFY_DATE,BR_CD,CR_CD,PARTIAL_FLAG,CR_NM,BR_NM
      ,SEND_BOOK_DATE,INTERNAL_BK_DATE,SEND_BROKER_DATE,ISCOMBINE_BL,TRUCK_COST,LSP_COST,IS_EXPORT,EXTERNAL_WMS
      ,EXTERNAL_WMS_NM,DEBIT_NO,DEBIT_NM,DECL_NUM,NEXT_NUM,ATP,CW,IORDER,TCBM,COMBINE_OTHER
      ,TELEX_RLS,HORN,BATTERY,CUSTOMS_CHECK,ISF_SEND_DATE,ISF_WIN,VIA,SORDER,Sdate
      ,SORDER_BY,DEBIT_TO,POST_CARGO_DATE,COST_CENTERDESCP,HOUSE_NO_FLAG,OPTIONS,SERVICE,CONT_DECL_NUM,VW,PVW,PLANT";

            subSql += " AND (ISCOMBINE_BL IS NULL  OR ISCOMBINE_BL='S' OR ISCOMBINE_BL='')";
            string talbe1 = string.Format(@"(SELECT 'O' AS TORDER,{0} FROM SMSM WHERE TORDER='C' AND STATUS IN ('O','H','F','R') UNION 
            SELECT 'H' AS TORDER,{0} FROM SMSM WHERE TORDER='C' AND STATUS  NOT IN ('O','H','F','R') UNION SELECT TORDER,{0} FROM SMSM
            WHERE TORDER NOT IN('C'))T", fields);
            string table = "(SELECT T.*,(SELECT TOP 1 SP.IPART_NO FROM SMDNP SP WHERE SP.DN_NO=T.DN_NO )AS IPART_NO FROM " + talbe1 + ")SMSM";
            return GetBootstrapData(table, GetBookingCondition() + subSql);
        }

        /// <summary>
        /// 发送报关查询
        /// </summary>
        /// <returns></returns>
        public ActionResult BookingBrokerQueryData()
        {
            string condition = " AND BORDER IS NOT NULL AND BORDER !='N' AND STATUS !='V' ";
            return GetBootstrapData("SMSM", GetBookingCondition() + condition);
        }


        /// <summary>
        /// 出口报关确认查询
        /// </summary>
        /// <returns></returns>
        public ActionResult BookingBrokerConfirmQueryData()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string virtualCol = Prolink.Math.GetValueAsString(Request.Params["virtualCol"]);
            string table = "(SELECT *" + virtualCol + " FROM (SELECT *, "+
                "(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1," +
                "(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO ," +
                "(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO," +
"(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO " +
            "FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM SMSMPT WITH (NOLOCK) WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_NO={0} AND SMSMPT.PARTY_TYPE IN('BR','BM'))", SQLUtils.QuotedStr(this.CompanyId)) + " AND BORDER IS NOT NULL AND BORDER !='N') S";
            if ("O".Equals(ioflag))
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT *," +
                "(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1," +
                "(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO ," +
                "(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO," +
"(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO " +
            " FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + " AND BORDER IS NOT NULL AND BORDER !='N') S" + ") M";
            }
            else
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT *," +
                "(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1," +
                "(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO ," +
                "(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO," +
"(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO" +
            " FROM SMSM WITH (NOLOCK) WHERE " + GetBookingCondition() + " AND BORDER IS NOT NULL AND BORDER !='N') S" + ") M";
            }
            
            string conditions = GetBaseGroup()+" AND STATUS !='V' ";
            return GetBootstrapData(table, conditions);
        }

        /// <summary>
        /// 订舱确认查询针对空海运非内贸的
        /// </summary>
        /// <returns></returns>
        public ActionResult BookingConfirmQueryData()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string virtualCol = Prolink.Math.GetValueAsString(Request.Params["virtualCol"]);
            string table = "(SELECT *" + virtualCol + " FROM (SELECT  *,(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO,"+
"(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1,"+
"(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO,"+
"(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO,"+
"(SELECT TOP 1 TARE_WEIGHT FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TARE_WEIGHT,"+
"(SELECT TOP 1 GW FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_GW,"+
"(SELECT TOP 1 TTL_VGM FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TTL_VGM  FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM SMSMPT WITH (NOLOCK) WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_NO={0} AND SMSMPT.PARTY_TYPE IN('SP','BO','CR'))", SQLUtils.QuotedStr(this.CompanyId)) + " AND CORDER IS NOT NULL AND CORDER !='N') S";
            if ("O".Equals(ioflag))
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT  *,(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO," +
"(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1,"+
"(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO,"+
"(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO,"+
"(SELECT TOP 1 TARE_WEIGHT FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TARE_WEIGHT,"+
"(SELECT TOP 1 GW FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_GW,"+
"(SELECT TOP 1 TTL_VGM FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TTL_VGM  FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + " AND CORDER IS NOT NULL AND CORDER !='N') S" + ") M";
            }
            else
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT  *,(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO,"+
"(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1,"+
"(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO,"+
"(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO,"+
"(SELECT TOP 1 TARE_WEIGHT FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TARE_WEIGHT,"+
"(SELECT TOP 1 GW FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_GW,"+
"(SELECT TOP 1 TTL_VGM FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TTL_VGM  FROM SMSM WITH (NOLOCK) WHERE "+GetBookingCondition()+" AND CORDER IS NOT NULL AND CORDER !='N') S" + ") M";
            }
            string conditions = GetBaseGroup();
            return GetBootstrapData(table, conditions);
        }
        public ActionResult BookingConfirmQueryDataFCL()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string virtualCol = Prolink.Math.GetValueAsString(Request.Params["virtualCol"]);
            string table = "(SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM SMSMPT WITH (NOLOCK) WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_NO={0} AND SMSMPT.PARTY_TYPE IN('SP','BO','CR'))", SQLUtils.QuotedStr(this.CompanyId)) + " AND CORDER IS NOT NULL AND CORDER !='N') S";
            if ("O".Equals(ioflag))
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + " AND CORDER IS NOT NULL AND CORDER !='N') S" + ") M";
            }
            else
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE " + GetBookingCondition() + " AND CORDER IS NOT NULL AND CORDER !='N') S" + ") M";
            }
            string conditions = GetBaseGroup();
            return GetBootstrapData(table, conditions,@" *,(SELECT TOP 1 CNTR_NO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_CNTR_NO,
(SELECT TOP 1 SEAL_NO1 FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_SEAL_NO1,
(SELECT TOP 1 TRUCK_SEALNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_SEALNO,
(SELECT TOP 1 TRUCK_CNTRNO FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TRUCK_CNTRNO,
(SELECT TOP 1 TARE_WEIGHT FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TARE_WEIGHT,
(SELECT TOP 1 GW FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_GW,
(SELECT TOP 1 TTL_VGM FROM SMRV  WHERE SMSM.SHIPMENT_ID=SMRV.SHIPMENT_ID ) AS RV_TTL_VGM " );
        }
        /// <summary>
        /// 订舱确认查询针对国内快递和内贸的来过滤物流业者的数据
        /// </summary>
        /// <returns></returns>
        public ActionResult BookingConfirmQueryDataByDT()
        {
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            string virtualCol = Prolink.Math.GetValueAsString(Request.Params["virtualCol"]);
            string table = "(SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM SMSMPT WITH (NOLOCK) WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_NO={0} AND SMSMPT.PARTY_TYPE ='CR')", SQLUtils.QuotedStr(this.CompanyId)) + " AND CORDER IS NOT NULL AND CORDER !='N') S";
            if ("O".Equals(ioflag))
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + " AND CORDER IS NOT NULL AND CORDER !='N') S" + ") AS SMSM";
            }
            else
            {
                table += " UNION SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE " + GetBookingCondition() + " AND CORDER IS NOT NULL AND CORDER !='N') S" + ") M";
            }
            
            string conditions = GetBaseGroup();
            return GetBootstrapData(table, conditions);
        }
        
        public ActionResult SMDNQueryData()
        {
            string dnno = Prolink.Math.GetValueAsString(Request.Params["Dnno"]);
            return GetBootstrapData("SMDN", "DN_NO=" + SQLUtils.QuotedStr(dnno));
        }

        public ActionResult SaveFCLBookingData()
        {
            string TranType = Request.Params["TranType"];
            string changeData = Request.Params["changedData"];
            string u_id = Request.Params["UId"];
            string returnMessage = "success";
            string shipmentid = Request.Params["ShipmentId"];
            string statussql = string.Format("SELECT STATUS FROM SMSM WHERE SHIPMENT_ID ={0}",SQLUtils.QuotedStr(shipmentid));
            string warning = string.Empty;

            string isconfirm = Prolink.Math.GetValueAsString(Request.Params["IsConfirm"]);
            
            string status = OperationUtils.GetValueAsString(statussql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(!returnMessage.Equals("success"))
            {
                return Json(new { message = returnMessage });
            }
            
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string su_id = string.Empty;
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            string ETD = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if ("Y".Equals(isconfirm))
                        {
                            ei.PutDate("RLS_CNTR_DATE", DateTime.Now);
                        }
                        string dnetd = ei.Get("ETD");
                        if (TrackingEDI.Business.DateTimeUtils.IsDate(dnetd))
                        {
                            int[] ymw = TrackingEDI.Business.DateTimeUtils.DateToYMW(dnetd);
                            if (ymw.Length >= 3)
                            {
                                ei.Put("YEAR", ymw[0]);
                                ei.Put("MONTH", ymw[1]);
                                ei.Put("WEEKLY", ymw[2]);
                                ETD = dnetd;
                            }
                        }
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                            if(TranType != "")
                            {
                                ei.Put("TRAN_TYPE", TranType);
                            }

                            //数据验证
                            /*string combineinfo = Prolink.Math.GetValueAsString(ei.Get("COMBINE_INFO"));
                            Decimal nowqty = Prolink.Math.GetValueAsDecimal(ei.Get("QTY"));
                            Decimal nowpgw= Prolink.Math.GetValueAsDecimal(ei.Get("PGW"));
                            Decimal nowpcbm = Prolink.Math.GetValueAsDecimal(ei.Get("PCBM"));
                            Decimal nownw = Prolink.Math.GetValueAsDecimal(ei.Get("NW"));
                            sql = string.Format("SELECT SUM(NW) AS NW, SUM(QTY) AS QTY,SUM(GW) AS GW,SUM(CBM) AS CBM FROM SMDN WHERE DN_NO IN {0}", SQLUtils.Quoted(combineinfo.Split(',')));
                            DataTable totoldt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            sql = string.Format("SELECT SUM(NW) AS NW, SUM(QTY) AS QTY,SUM(PGW) AS PGW,SUM(PCBM) AS PCBM FROM SMSM WHERE COMBINE_INFO={0}", SQLUtils.QuotedStr(combineinfo));
                            DataTable usedt = OperationUtils.GetDataTable(sql, null,Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (totoldt.Rows.Count > 0 && usedt.Rows.Count > 0)
                            {
                                Decimal totolindex = Prolink.Math.GetValueAsInt(totoldt.Rows[0]["QTY"]);
                                Decimal useindex = Prolink.Math.GetValueAsInt(usedt.Rows[0]["QTY"]);
                                if (nowqty > totolindex - useindex)
                                    return Json(new { message = "订舱数量已经超过了DN中的数量！" });
                                totolindex = Prolink.Math.GetValueAsDecimal(totoldt.Rows[0]["GW"]);
                                useindex = Prolink.Math.GetValueAsDecimal(usedt.Rows[0]["PGW"]);
                                if (nowpgw > totolindex - useindex)
                                    warning += "毛重已经超过DN毛重的总和！\n";
                                totolindex = Prolink.Math.GetValueAsDecimal(totoldt.Rows[0]["CBM"]);
                                useindex = Prolink.Math.GetValueAsDecimal(usedt.Rows[0]["PCBM"]);
                                if (nowpcbm> totolindex - useindex)
                                    warning += "材积已经超过DN材积的总和！\n";
                                totolindex = Prolink.Math.GetValueAsDecimal(totoldt.Rows[0]["NW"]);
                                useindex = Prolink.Math.GetValueAsDecimal(usedt.Rows[0]["NW"]);
                                if(nownw> totolindex - useindex)
                                    warning += "净重已经超过DN材积的总和！";
                            }*/
                            
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
                            shipmentid = ei.Get("SHIPMENT_ID");
                        }
                        mixList.Add(ei);

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei = new EditInstruct("SMSMPT", EditInstruct.DELETE_OPERATION);
                            ei.PutKey("SHIPMENT_ID", shipmentid);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmptModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        continue;
                        //mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (status.Equals("A") || status.Equals("B"))
                    {
                        if (!"Y".Equals(isconfirm))
                        {
                            CommonManager.UpdateSMSMPartys(shipmentid);
                            CommonManager.UpdateSMSM(shipmentid);
                        }
                    }
                    //modify by dean 20161026 问题单：10525  订舱的EDT变更后，系统需要自动计价一次并且将ETD同步更新到账务管理的账单日期ETD栏位等
                    AutoCalculation(status, ETD, u_id);
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}  ORDER BY ORDER_BY ASC ", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDNP WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0}) ORDER BY DELIVERY_ITEM ", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMCUFT WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt3 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            if (mainDt.Rows.Count > 0)
                data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmModel");
            if (mainDt.Rows.Count > 0)
                data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmptModel");
            if (mainDt.Rows.Count > 0)
                data["sub2"] = ModelFactory.ToTableJson(subDt2, "SmdnModel");
            if (subDt3.Rows.Count > 0)
                data["sub3"] = ModelFactory.ToTableJson(subDt3, "SmcuftModel");

            string trantype = mainDt.Rows[0]["TRAN_TYPE"].ToString();
            string partialflag = mainDt.Rows[0]["PARTIAL_FLAG"].ToString();

            sql = "SELECT * FROM SMBDD WHERE 1=0";
            if ("Y".Equals(partialflag))
            {
                sql = string.Format("SELECT * FROM SMBDD WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            DataTable subbdd = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            data["subbdd"] = ModelFactory.ToTableJson(subbdd, "SmbddModel");

            if (!string.IsNullOrEmpty(warning))
                data["warning"] = warning;
            return ToContent(data);
        }
        public void AutoCalculation(string status, string ETD, string u_id)
        {
            //if (status.Equals("C") && ETD != "")  修改：订舱后
            if ((!status.Equals("A") || !status.Equals("B")) && ETD != "")
            {
                //string autosql = string.Format("INSERT INTO AUTO_VALUATION_TASK (U_ID,SMU_ID,DONE,CREATE_BY,CREATE_DATE) VALUES ('{0}','{1}','N','{2}','{3}')", System.Guid.NewGuid().ToString(), u_id, UserId, DateTime.Now);
                EditInstruct AutoValuationTaskEi = new EditInstruct("AUTO_VALUATION_TASK", EditInstruct.INSERT_OPERATION);
                AutoValuationTaskEi.Put("U_ID", System.Guid.NewGuid().ToString());
                AutoValuationTaskEi.Put("SMU_ID", u_id);
                AutoValuationTaskEi.Put("DONE", "N");
                AutoValuationTaskEi.Put("CREATE_BY", UserId);
                AutoValuationTaskEi.PutDate("CREATE_DATE", DateTime.Now);
                OperationUtils.ExecuteUpdate(AutoValuationTaskEi, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }
        public ActionResult GetFCLBookingData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("'*", "SMSM", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetDNData(string id = null, string uid = null)
        {
            string dnno = string.Empty;
            if (!String.IsNullOrEmpty(Request["DnNo"]))
            {
                dnno = SQLUtils.QuotedStr(Request["DnNo"]).Replace(",", SQLUtils.QuotedStr(","));
            }
            else
            {
                return ToContent(null);
            }
            //TODO DN ATA CHECK
            //string sql = string.Format("SELECT GROUP_ID,CMP,U_ID FROM SMDN WHERE DN_NO in ({0}) ", dnno);
            string sql = string.Format(@";WITH REF_DATA AS(
SELECT SMDN.U_ID,SMDN.GROUP_ID,SMDN.CMP,SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF FROM SMDN WHERE DN_NO IN ({0})
UNION ALL
SELECT SMDN.U_ID,SMDN.GROUP_ID,SMDN.CMP,SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF FROM SMDN INNER JOIN REF_DATA R ON   R.REF_NO = SMDN.DN_NO 
)
SELECT U_ID,GROUP_ID,CMP FROM REF_DATA 
UNION select U_ID,GROUP_ID,CMP from SMINM WHERE  SHIPMENT_ID IN (
select SHIPMENT_ID from smdn   WHERE DN_NO IN ({0}))", dnno);
            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["dn"] = ModelFactory.ToTableJson(Dt, "SmdnModel");
            return ToContent(data);
        }
        public ActionResult GetSMAndDnData(string id = null, string uid = null)
        {
            string smno = string.Empty;
            if (!String.IsNullOrEmpty(Request["SmNo"]))
            {
                smno = SQLUtils.QuotedStr(Request["SmNo"]).Replace(",", SQLUtils.QuotedStr(","));
            }
            else
            {
                return ToContent(null);
            }

            string dn_no = Prolink.Math.GetValueAsString(Request["DnNo"]);
            string sql = string.Empty;
            if (string.IsNullOrEmpty(dn_no))
            {
                string conditions = string.Format("SHIPMENT_ID IN ({0})", smno);
                sql = string.Format(@"SELECT GROUP_ID,CMP,U_ID FROM SMSM  WHERE {0}
                UNION SELECT GROUP_ID,CMP,U_ID FROM SMDN WHERE {0} ", conditions);
            }
            else
            {
                string _sql = string.Format("select DN_NO_CMP_REF  from SMDN WHERE DN_NO ={0}",SQLUtils.QuotedStr(dn_no));
                string DN_NO_CMP_REF = OperationUtils.GetValueAsString(_sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (string.IsNullOrEmpty(DN_NO_CMP_REF))
                {
                    string conditions = string.Format("SHIPMENT_ID IN ({0})", smno);
                    sql = string.Format(@"SELECT GROUP_ID,CMP,U_ID FROM SMSM  WHERE {0}
                UNION SELECT GROUP_ID,CMP,U_ID FROM SMDN WHERE {0} ", conditions);
                }
                else
                {
                    string conditions = string.Format("SHIPMENT_ID IN ({0})", smno);
                    string _conditions = string.Format("DN_NO IN ({0})", SQLUtils.QuotedStr(dn_no));
                    sql = string.Format(@"SELECT GROUP_ID,CMP,U_ID FROM SMSM  WHERE {0}
                UNION SELECT GROUP_ID,CMP,U_ID FROM SMDN WHERE {1} ", conditions, _conditions);
                }
            }
            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sm"] = ModelFactory.ToTableJson(Dt, "SmdnModel");
            return ToContent(data);
        }

        public ActionResult GetSMData(string id = null, string uid = null)
        {
            string smno = string.Empty;
            if (!String.IsNullOrEmpty(Request["SmNo"]))
            {
                smno = SQLUtils.QuotedStr(Request["SmNo"]).Replace(",", SQLUtils.QuotedStr(","));
            }
            else
            {
                return ToContent(null);
            }

            string sql = string.Format("SELECT GROUP_ID,CMP,U_ID FROM SMSM WHERE SHIPMENT_ID in ({0}) ", smno);
            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sm"] = ModelFactory.ToTableJson(Dt, "SmsmModel");
            return ToContent(data);
        } 

        public ActionResult GetFCLBookingItem()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (mainDt.Rows.Count <= 0)
            {
                return ToContent(data);
            }
            string shipmentid = mainDt.Rows[0]["SHIPMENT_ID"].ToString();
            sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}  ORDER BY ORDER_BY ASC ", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string combine_info = mainDt.Rows[0]["COMBINE_INFO"].ToString();
            string[] dns = combine_info.Split(',');

            sql = string.Format("SELECT * FROM SMDNP WHERE DN_NO IN {0} ORDER BY DN_NO,DELIVERY_ITEM", SQLUtils.Quoted(dns));
            DataTable subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMCUFT WHERE DN_NO IN {0}", SQLUtils.Quoted(dns));
            DataTable subDt3 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            DataTable smdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string trantype = mainDt.Rows[0]["TRAN_TYPE"].ToString();
            string partialflag = mainDt.Rows[0]["PARTIAL_FLAG"].ToString();

            sql = "SELECT * FROM SMBDD WHERE 1=0";
            if ("Y".Equals(partialflag))
            {
                sql = string.Format("SELECT * FROM SMBDD WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            DataTable subbdd = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            //string rvstatusSQL = string.Format("SELECT STATUS FROM SMRV WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMSM WHERE U_ID={0})", SQLUtils.QuotedStr(u_id));
            //string rvstatus=OperationUtils.GetValueAsString(rvstatusSQL, Prolink.Web.WebContext.GetInstance().GetConnection());

            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmptModel");
            data["sub2"] = ModelFactory.ToTableJson(subDt2, "SmdnpModel");
            data["sub3"] = ModelFactory.ToTableJson(subDt3, "SmcuftModel");
            data["DnGrid"] = ModelFactory.ToTableJson(smdndt, "SmdnModel");
            data["subbdd"] = ModelFactory.ToTableJson(subbdd, "SmbddModel");
            //data["rvstatus"] = rvstatus;
            return ToContent(data);
        }

        public ActionResult DNDetailQuery()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string uid = Request.Params["conditions"].ToString();
            string sql = string.Format("SELECT * FROM SMDNP WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID = (SELECT SHIPMENT_ID FROM SMSM WHERE U_ID= {0}))", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string resultType = Request.Params["resultType"];
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

        public ActionResult SaveCustomsBook()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            string u_id = Request.Params["UId"];
            string shipmentid = Request.Params["ShipmentId"];
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        string declrlsdate = ei.Get("DECL_RLS_DATE");

                        sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
                        DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (dt.Rows.Count > 0)
                        {
                            string border = Prolink.Math.GetValueAsString(dt.Rows[0]["BORDER"]);
                            if (string.IsNullOrEmpty(declrlsdate)) declrlsdate = Prolink.Math.GetValueAsString(dt.Rows[0]["DECL_RLS_DATE"]);
                            if (border.Equals("C"))
                            {
                                if (!string.IsNullOrEmpty(declrlsdate))
                                {
                                    ei.Put("BORDER", "H");  //有放行时间，将状态更新为放行
                                    ei.Put("STATUS", "H");
                                }
                            }
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                            ei.AddKey("U_ID");
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "dngrid")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmdnModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            string dn_no=ei.Get("DN_NO");
                            ei.PutKey("SHIPMENT_ID", shipmentid);
                            ei.PutKey("DN_NO", dn_no);
                            mixList.Add(ei);
                        }
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    GetTTLNextNum(shipmentid);
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}  ORDER BY ORDER_BY ASC ", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmptModel");
            return ToContent(data);
        }

        public void GetTTLNextNum(string shipmentid)
        {
            string nextsql = string.Format(@"SELECT SMDN.BL_LEVEL,SMDN.NEXT_NUM,SMDN.EDECL_NO,SMDN.DECL_DATE,SMDN.EXPORT_NO,SMDN.DECL_RLS_DATE,
                    SMSM.BORDER FROM SMDN,SMSM WHERE SMDN.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMDN.SHIPMENT_ID={0}",
                        SQLUtils.QuotedStr(shipmentid));
            DataTable nexdt = OperationUtils.GetDataTable(nextsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int total = 0;
            string exportno = string.Empty;
            string edeclno = string.Empty;
            string decldate = string.Empty;
            string declrlsdate = string.Empty;
            List<string> decllist=new List<string>();
            string border = string.Empty;
            for (int i = 0; i < nexdt.Rows.Count;i++ )
            {
                DataRow dr = nexdt.Rows[i];
                border = Prolink.Math.GetValueAsString(dr["BORDER"]);
                int indexnum = Prolink.Math.GetValueAsInt(dr["NEXT_NUM"]);
                total += indexnum;
                string declitem=Prolink.Math.GetValueAsString(dr["EDECL_NO"]);
                if (i == 0)
                {
                    exportno = Prolink.Math.GetValueAsString(dr["EXPORT_NO"]);
                    edeclno = declitem;
                    decldate = Prolink.Math.GetValueAsString(dr["DECL_DATE"]);
                    declrlsdate = Prolink.Math.GetValueAsString(dr["DECL_RLS_DATE"]);
                }
                if (string.IsNullOrEmpty(declitem)) continue;
                if (!decllist.Contains(declitem))
                    decllist.Add(declitem);
            }
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("NEXT_NUM", total);
            ei.Put("EXPORT_NO", exportno);
            ei.Put("EDECL_NO", edeclno);
            if (decllist.Count > 0)
            {
                ei.Put("DECL_NUM", decllist.Count().ToString());
                ei.Put("CONT_DECL_NUM", (decllist.Count() - 1).ToString());
            }
            else
            {
                ei.Put("DECL_NUM", "0");
                ei.Put("CONT_DECL_NUM", "0");
            }
            ei.PutDate("DECL_DATE",decldate);
            ei.PutDate("DECL_RLS_DATE", declrlsdate);
            if ("C".Equals(border))
            {
                if (!string.IsNullOrEmpty(declrlsdate))
                {
                    ei.Put("BORDER", "H");  //有放行时间，将状态更新为放行
                    ei.Put("STATUS", "H");
                }
            }
            MixedList ml = new MixedList();
            ml.Add(ei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
            }
        }

        public ActionResult GetCustomsBookData()
        {
            return GetBootstrapData("SMSM", "");
        }

        public ActionResult GetCustomsBookItem()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string u_id = Request["uId"];
            string sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (mainDt.Rows.Count <= 0)
            {
                return ToContent(data);
            }
            string shipmentid = mainDt.Rows[0]["SHIPMENT_ID"].ToString();
            sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}  ORDER BY ORDER_BY ASC ", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDNP WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT DN_NO,BL_LEVEL,EXPORT_NO,UNICODE,APPROVE_NO,ASK_TIM,EDECL_NO,DECL_DATE,RLS_DATE,Decl_Rls_Date,Next_Num,DREMARK FROM SMDN WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentid));
            DataTable subdn = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmptModel");
            data["sub2"] = ModelFactory.ToTableJson(subDt2, "SmdnpModel");
            data["DnGrid"] = ModelFactory.ToTableJson(subdn, "SmdnModel");
            return ToContent(data);

        }
        #endregion

        #region  订舱确认
        public ActionResult SaveFCLBConfirmData()
        {
            string shipmentid = Request.Params["ShipmentId"];
            ConfirmBookingResultInfo cbri=Helper.ConfirmBooking(shipmentid, UserId);
            if (!cbri.IsSucceed)
            {
                return Json(new { message = cbri.Description, IsOk = "N" });
            }
            return Json(new { message = cbri.Description, IsOk = "Y" });
        }
        #endregion

        public ActionResult UpdateExportTag()
        {
            string JOBNO = Request.Params["JOBNO"]+"last";
            string GROUP_ID = Request.Params["GROUP_ID"];
            string CMP = Request.Params["CMP"];
            string STN = Request.Params["STN"];
            JOBNO = "'" + JOBNO.Replace(",", "','") + "'";

            string table = Prolink.Math.GetValueAsString(Request.Params["TbType"]);
            if ("Inbound".Equals(table))
                table = "SMSMI";
            else
                table = "SMSM";
            string sql = "UPDATE " + table + " SET IS_EXPORT = 'Y' WHERE SHIPMENT_ID in (" + JOBNO + ") AND GROUP_ID = " + SQLUtils.QuotedStr(GROUP_ID) + " AND CMP = " + SQLUtils.QuotedStr(CMP);

            try
            {
                OperationUtils.ExecuteUpdate(sql ,Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { message = "fail", IsOk = "N" });
            }

            return Json(new { message = "success", IsOk = "Y" });
        }

        #region 订舱退运
        public ActionResult BackTransport()
        {
            string returnMessage = "";
            string dnno =Prolink.Math.GetValueAsString(Request["Dnno"]);
            string shipmentid=Prolink.Math.GetValueAsString(Request["Shipmentid"]);
            MixedList mixList = new MixedList();
            //删除DN的审核状态
            EditInstruct ei=new EditInstruct("APPROVE_RECORD",EditInstruct.DELETE_OPERATION);
            ei.PutKey("REF_NO", dnno);
            mixList.Add(ei);
            //设置该笔的资料为V，该笔Shipmentid不能使用
            ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("DN_NO", dnno);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.PutKey("STATUS","V");
            mixList.Add(ei);
            //dn可以重新跑签核流程
            ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("DN_NO", dnno);
            ei.PutKey("STATUS", "A");
            mixList.Add(ei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMessage = @Resources.Locale.L_DNManage_Controllers_282;
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
                return Json(new { message = returnMessage });
            }
            return Json(new { message = returnMessage});
        }
        #endregion

        #region Invoice/Packing
        public ActionResult InvQueryData()
        {
            string table = @"(SELECT SMINM.*,(SELECT TOP 1 SM_STATUS FROM V_SMDN WHERE V_SMDN.DN_NO=SMINM.DN_NO)SM_STATUS,
            (SELECT TOP 1 BL_WIN FROM SMSM WHERE SMSM.SHIPMENT_ID=SMINM.SHIPMENT_ID)BL_WIN,
            (SELECT TOP 1 SEND_BROKER_DATE FROM SMSM WHERE SMSM.SHIPMENT_ID=SMINM.SHIPMENT_ID)SEND_BROKER_DATE   FROM SMINM ) SMINM";
            string condition = "((" + GetBaseCmp() + ") OR (UPLOAD_BY=" + SQLUtils.QuotedStr(UserId)+"))";
            condition = GetCreateDateCondition("SMINM", condition);
            return GetBootstrapData(table, condition);
        }
        public JsonResult GetInvDetail()
        {
            string u_id = Request["UId"];
            //string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            //string sql = string.Format("SELECT * FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable dt = new DataTable();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();

            string sql = "SELECT TOP 1 * FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string dn_uid = "";
            if (dt.Rows.Count > 0)
            {
                string DnNo = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
                string Cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            
                sql = "SELECT U_ID FROM SMDN WHERE DN_NO={0} AND CMP={1}";
                sql = string.Format(sql, SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(Cmp));
                dn_uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }


            //DataTable dt = ModelFactory.InquiryData("*", "SMINM", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            string conditions = " WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
            DataTable detailDt = ModelFactory.InquiryData("*", "SMIND", conditions, "", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(detailDt)
            };

            DataTable pkgDt = ModelFactory.InquiryData("*", "SMINP", conditions, " SEQ_NO ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult pkgDetail = null;
            pkgDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(pkgDt)
            };

            DataTable ipoDt = ModelFactory.InquiryData("*", "SMINPO", conditions, "PO_NO ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult ipoDetail = null;
            ipoDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(ipoDt)
            };

            return Json(new { mainTable = result.ToContent(), inData = resultDetail.ToContent(), pkgData = pkgDetail.ToContent(), ipoData = ipoDetail.ToContent(), dn_uid = dn_uid });
        }

        public ActionResult GetIndGridData()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string conditions = " U_FID=" + SQLUtils.QuotedStr(UId);
            int recordsCount = 0, pageIndex = 0, pageSize = 999;
            JavaScriptSerializer js = new JavaScriptSerializer();

            DataTable dt = ModelFactory.InquiryData("*", "SMIND", conditions, "SEQ_NO ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return resultDetail.ToContent();
        }

        public ActionResult GetPkgGridData()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string conditions = " U_FID=" + SQLUtils.QuotedStr(UId);
            int recordsCount = 0, pageIndex = 0, pageSize = 999;
            JavaScriptSerializer js = new JavaScriptSerializer();

            DataTable dt = ModelFactory.InquiryData("*", "SMINP", conditions, "SEQ_NO ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return resultDetail.ToContent();
        }

        public ActionResult GetInpoGridData()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string conditions = " U_FID=" + SQLUtils.QuotedStr(UId);
            int recordsCount = 0, pageIndex = 0, pageSize = 999;
            JavaScriptSerializer js = new JavaScriptSerializer();

            DataTable dt = ModelFactory.InquiryData("*", "SMINPO", conditions, "PO_NO ASC", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return resultDetail.ToContent();
        }

        public ActionResult InvSetupUpdate()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            List<Dictionary<string, object>> invData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> indData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> pkgData = new List<Dictionary<string, object>>();
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmInmModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.PutKey("U_ID", UId);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("U_ID", ei.Get("U_ID"));

                            if (ei.Get("FREIGHT_FEE") != null || ei.Get("ISSUE_FEE") != null || ei.Get("FOB_VALUE") != null)
                            {
                                ei.Put("SEND_FRT", "");
                            }

                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            EditInstruct ei2 = new EditInstruct("SMIND", EditInstruct.DELETE_OPERATION);
                            EditInstruct ei3 = new EditInstruct("SMINP", EditInstruct.DELETE_OPERATION);
                             if (ei.Get("U_ID") == null)
                            {
                                continue;
                            }

                             ei2.PutKey("U_FID", UId);
                             ei3.PutKey("U_FID", UId);
                             mixList.Add(ei2);
                            mixList.Add(ei3);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "st1")
                { 
                    string sql = ""; 
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmIndModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            ei.Put("U_FID", UId);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "st2")
                {
                    string sql = "";
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmInpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            ei.Put("U_FID", UId);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "st3")
                {
                    string sql = "";
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SminpoModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            ei.Put("U_FID", UId);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
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
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    int recordsCount = 0, pageIndex = 1, pageSize = 999;
                    string sql = string.Format("SELECT * FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    invData = ModelFactory.ToTableJson(mainDt, "SmInmModel");

                    //DataTable dt = ModelFactory.InquiryData("*", "SMIND", "U_FID='" + UId + "'", "SEQ_NO ASC", pageIndex, pageSize, ref recordsCount);
                    sql = string.Format("SELECT * FROM SMIND WHERE U_FID={0} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(UId));
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection()); 
                    indData = ModelFactory.ToTableJson(dt, "SmIndModel");

                    //DataTable dt1 = ModelFactory.InquiryData("*", "SMINP", "U_FID='" + UId + "'", "SEQ_NO ASC", pageIndex, pageSize, ref recordsCount);
                    sql = string.Format("SELECT * FROM SMINP WHERE U_FID={0} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(UId));
                    DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection()); 
                    pkgData = ModelFactory.ToTableJson(dt1, "SmPkgModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = invData, subData1 = indData, subData2=pkgData });
        }
        #endregion

        #region 訂艙獲取SI資訊
        public ActionResult getSiData()
        { 
            string returnMessage = "success";
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string sql = string.Format("SELECT PROFILE_CD FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            string profileid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string[] profiles = profileid.Split(';');
            ActionResult result = null;
            result = GetBootstrapData("SMSIM", string.Format(" PROFILE IN {0}",SQLUtils.Quoted(profiles)));
            return Json(new { message = returnMessage, returnData = result });
        }

        public ActionResult getSidData()
        { 
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string PodCd = Prolink.Math.GetValueAsString(Request.Params["PpodCd"]);
            string PolCd = Prolink.Math.GetValueAsString(Request.Params["PpolCd"]);
            string Incoterm = Prolink.Math.GetValueAsString(Request.Params["Incoterm"]);
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            DataTable dt = null, sellerDt = null, buyerDt = null, cneeDt = null, shprDt = null, notify1Dt = null, notify2Dt = null;

            List<Dictionary<string, object>> simData = new List<Dictionary<string, object>>();
            string sql = "SELECT * FROM SMSIM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string Seller = Prolink.Math.GetValueAsString(item["SELLER"]);
                    sql = "SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(Seller);
                    sellerDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                    for (int i = 1; i < 6; i++)
                    {
                        string Buyer = Prolink.Math.GetValueAsString(item["BUYER" + i]);
                        string bIncoterm = Prolink.Math.GetValueAsString(item["INCOTERM" + i]);

                        if (bIncoterm == Incoterm)
                        {
                            sql = "SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(Buyer);
                            buyerDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                            break;
                        }
                    }
                }
            }
            List<Dictionary<string, object>> sellerData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> buyerData = new List<Dictionary<string, object>>();
            if (sellerDt != null)
            { 
                sellerData = ModelFactory.ToTableJson(sellerDt);
            }
            
            if (buyerDt != null)
            {
                buyerData = ModelFactory.ToTableJson(buyerDt);
            }
            

            List<Dictionary<string, object>> sidData = new List<Dictionary<string, object>>();
            sql = "SELECT TOP 1 * FROM SMSID WHERE U_FID=" + SQLUtils.QuotedStr(UId) + " AND POL=" + SQLUtils.QuotedStr(PolCd) + " AND POD=" + SQLUtils.QuotedStr(PodCd);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sidData = ModelFactory.ToTableJson(dt);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string CneeCd = Prolink.Math.GetValueAsString(item["CNEE_CD"]);
                    string ShprCd = Prolink.Math.GetValueAsString(item["SHPR_CD"]);
                    string Notify1 = Prolink.Math.GetValueAsString(item["NOTIFY1"]);
                    string Notify2 = Prolink.Math.GetValueAsString(item["NOTIFY2"]);

                    sql = "SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(CneeCd);
                    cneeDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = "SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(ShprCd);
                    shprDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = "SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(Notify1);
                    notify1Dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = "SELECT TOP 1 * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(Notify2);
                    notify2Dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }

            List<Dictionary<string, object>> cneeData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> shprData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> notify1Data = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> notify2Data = new List<Dictionary<string, object>>();
            if (cneeDt != null)
            {
                cneeData = ModelFactory.ToTableJson(cneeDt);
            }
            if (shprDt != null)
            {
                shprData = ModelFactory.ToTableJson(shprDt);
            }
            if (notify1Dt != null)
            {
                notify1Data = ModelFactory.ToTableJson(notify1Dt);
            }
            if (notify2Dt != null)
            {
                notify2Data = ModelFactory.ToTableJson(notify2Dt);
            }

            try
            {

                sql = "SELECT COUNT(*) FROM SMINM WHERE INVOICE_TYPE='O' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (num == 0)
                {
                    sql = "SELECT TOP 1 * FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    MixedList ml = new MixedList();
                    EditInstruct ei;
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            string DnNo = Prolink.Math.GetValueAsString(item["DN_NO"]);
                            string Goods = Prolink.Math.GetValueAsString(item["GOODS"]);
                            string ruleCode = "SMINM_NO";
                            System.Collections.Hashtable hash = new System.Collections.Hashtable();
                            hash.Add(CompanyId, ruleCode);
                            string InvNo = AutoNo.GetNo(ruleCode, hash, GroupId, CompanyId, "*");

                            ei = new EditInstruct("SMINM", EditInstruct.INSERT_OPERATION);
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("INV_NO", InvNo);
                            ei.Put("DN_NO", DnNo);
                            ei.Put("SHIPMENT_ID", ShipmentId);
                            ei.Put("CMDTY_CD", Goods);
                            ei.Put("INVOICE_TYPE", "O");
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            if(shprDt != null)
                            {
                                if (shprDt.Rows.Count > 0)
                                {
                                    foreach (DataRow item1 in shprDt.Rows)
                                    {
                                        ei.Put("SHPR_CD", Prolink.Math.GetValueAsString(item1["PARTY_NO"]));
                                        ei.Put("SHPR_NM", Prolink.Math.GetValueAsString(item1["PARTY_NAME"]));
                                    }
                                }
                            }

                            if (cneeDt != null)
                            {
                                if (cneeDt.Rows.Count > 0)
                                {
                                    foreach (DataRow item2 in cneeDt.Rows)
                                    {
                                        ei.Put("CNEE_CD", Prolink.Math.GetValueAsString(item2["PARTY_NO"]));
                                        ei.Put("CNEE_NM", Prolink.Math.GetValueAsString(item2["PARTY_NAME"]));
                                    }
                                }
                            }

                            if (notify1Dt != null)
                            {
                                if (notify1Dt.Rows.Count > 0)
                                {
                                    foreach (DataRow item3 in notify1Dt.Rows)
                                    {
                                        ei.Put("NOTIFY_NO", Prolink.Math.GetValueAsString(item3["PARTY_NO"]));
                                        ei.Put("NOTIFY_NM", Prolink.Math.GetValueAsString(item3["PARTY_NAME"]));
                                    }
                                }
                            }

                            ml.Add(ei);
                            try
                            {
                                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch (Exception ex)
                            {
                                returnMessage = ex.Message;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }

            return Json(new { message = returnMessage, sidData = sidData, sellerData = sellerData, buyerData = buyerData, cneeData = cneeData, shprData = shprData, notify1Data= notify1Data, notify2Data = notify2Data });
        }
        #endregion

        [HttpPost]
        public ActionResult Upload(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                string mapping= string.Empty;
                string confirmtype = Request.Params["ConfirmType"].ToString();
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

                    switch (confirmtype)
                    {
                        case "FCL":
                            mapping = BookingStatusManager.FCLStatusMapping;
                            break;
                        case "LCL":
                            mapping = BookingStatusManager.LCLStatusMapping;
                            break;
                        case "EXP":
                            mapping = BookingStatusManager.ExpStatusMapping;
                            break;
                        case "AIR":
                            mapping = BookingStatusManager.AirStatusMapping;
                            break;
                        case "TK":
                            mapping = BookingStatusManager.TruckBKStatusMapping;
                            break;
                        case "RAIL":
                            mapping = BookingStatusManager.RailWayBKStatusMapping;
                            break;
                        case "DECL":
                            mapping = BookingStatusManager.DeclStatusMapping;
                            break;
                    }

                    MixedList ml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    parm["CONFIRM_TYPE"] = confirmtype;
                    ExcelParser.RegisterEditInstructFunc(mapping, BookingStatusManager.HandleBookingStatus);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm);
                    if (ml.Count <= 0)
                    {
                        returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                    }
                    if (ml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if ("DECL".Equals(mapping))
                            {
                                for (int i = 0; i < ml.Count; i++)
                                {
                                    Object obj = ml[i];
                                    if ("EditInstruct".Equals(obj.GetType()))
                                    {
                                        EditInstruct ei = (EditInstruct)obj;
                                        SetStatus(Prolink.Math.GetValueAsString(ei.Get("SHIPMENT_ID")), UserId);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ml.Count; i++)
                                {
                                    EditInstruct ei = (EditInstruct)ml[i];
                                    ConfirmBookingResultInfo cbri = Helper.ConfirmBooking(ei.Get("SHIPMENT_ID").ToString(), UserId);
                                    if (!cbri.IsSucceed)
                                    {
                                        return Json(new { message = cbri.Description, IsOk = "N" });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }catch(Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }

        public JsonResult GetSmdndDetail()
        {
            string u_id = Request["UId"];
            DataTable dt = new DataTable();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();

            string sql = "SELECT TOP 1 * FROM SMDND WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
            dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            return Json(new { mainTable = result.ToContent() });
        }

        public void SetStatus(string shipmentid,string sender)
        {
            Manager.SaveStatus(new Status() { ShipmentId = shipmentid,Sender=sender, StsCd = "020", Location = CompanyId, LocationName = "", StsDescp = "Booking Confirm" });
        }

        public ActionResult SetEdocColumn()
        {
            string jobno = Request.Params["jobNo"];
            string edoctype = Request.Params["edocType"];
            if(string.IsNullOrEmpty(jobno))
                return Json(new { message = "fail", IsOk = "N" });
            MixedList mlist = new MixedList();
            EditInstruct ei=new EditInstruct("SMDN",EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID",jobno);
            ei.Put("EDOC","Y");
            mlist.Add(ei);

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { message = "fail", IsOk = "N" });
                }
            }
            return Json(new { message = "success", IsOk = "Y" });
        }

        public ActionResult PoEdocCopy()
        {
            string selUids = Request.Params["selUids"];
            string jobNo = Request.Params["jobNo"];
            string groupId = Request.Params["GROUP_ID"];
            string cmp = Request.Params["CMP"];
            string stn = Request.Params["STN"];
            string fileId = Request.Params["fileId"];
            if (string.IsNullOrEmpty(selUids))
                return Json(new { message = "fail", IsOk = "N" });

            Thread tr = new Thread(() => doPoEdocCopy(selUids, jobNo, groupId, cmp, stn, fileId));

            try
            {
                tr.Start();
            }
            catch (Exception ex)
            {
                return Json(new { message = "fail", IsOk = "N" });
            }      
            return Json(new { message = "success", IsOk = "Y" });
        }

        public void doPoEdocCopy(string selUids, string jobNo, string groupId, string cmp, string stn, string fileId)
        {
            string[] UIds = selUids.Split(',');
            MixedList mlist = new MixedList();
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            string sourceFileId = fileId;
            string targetGuid = "";

            //update temp type to PO
            List<EDOCApi.UpdateFileItem> fileList = new List<EDOCApi.UpdateFileItem>();
            fileList.Add(new EDOCApi.UpdateFileItem
            {
                FileID = sourceFileId,
                EdocType = "PO",
                Remark = "BATCH PO UPLOAD"
            });
            _api.UpdateFiles(fileList);


            //for loop in selected rows, using uid get GUID to copy
            for (int i = 0; i < UIds.Length; i++)
            {
                if (string.IsNullOrEmpty(UIds[i]))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        targetGuid = _api.GetFolderGUID(UIds[i], groupId, cmp, stn, "");
                        if (string.IsNullOrEmpty(targetGuid))
                        {
                            targetGuid = _api.SetNewFolder(); //沒查到才向EDOC索取並寫進table
                            if (!string.IsNullOrEmpty(targetGuid))
                                _api.SetFolderIDInDB(UIds[i], targetGuid, groupId, cmp, stn, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        if (message.Length > 500) message = message.Substring(0, 500);
                        EditInstruct ei2 = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
                        ei2.Put("U_ID", System.Guid.NewGuid().ToString());
                        ei2.Put("EDI_ID", "GetFolderGuid");
                        ei2.PutExpress("EVENT_DATE", "getdate()");
                        ei2.Put("REMARK", message);
                        ei2.Put("SENDER", "Edoc Server");
                        ei2.Put("RS", "Receive");
                        ei2.Put("STATUS", "Exception");
                        ei2.Put("FROM_CD", "Edoc Server");
                        ei2.Put("TO_CD", "Eshipping");
                        ei2.Put("DATA_FOLDER", "");
                        ei2.Put("REF_NO", UIds[i]);
                        ei2.Put("GROUP_ID", "TPV");
                        ei2.Put("CMP", cmp);
                        ei2.Put("STN", stn);
                        mlist.Add(ei2);
                    }
                    if (string.IsNullOrEmpty(targetGuid))
                    {
                        continue;
                    }
                    //if dn upload po doc, it's will update the EDOC tag to Y
                    EDOCApi.EDOCAgent.Agent.DoCopyFile(sourceFileId, targetGuid);
                    EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", UIds[i]);
                    ei.Put("EDOC", "Y");
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
                  
                }

            }
            //delete temp file
            _api.DeleteFile(sourceFileId);
        }

        public ActionResult Send2ISF()
        {
            string returnMsg = "success";
            string ShipmentId = Request.Params["ShipmentId"];
            string ISFAcct = Request.Params["ISFAcct"];
            string ISFPWD = Request.Params["ISFPWD"];
            string pol=string.Empty;
            string pod=string.Empty;
            string ScacCd = string.Empty;
            string MasterNo = string.Empty;
            string HouseNo = string.Empty;
            string uid = string.Empty;
            string sql = "SELECT POD_CD,POL_CD,SCAC_CD,MASTER_NO,HOUSE_NO,U_ID FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt=OperationUtils.GetDataTable(sql, null,Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count>0){
                pod=Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
                pol=Prolink.Math.GetValueAsString(dt.Rows[0]["POL_CD"]);
                ScacCd = Prolink.Math.GetValueAsString(dt.Rows[0]["SCAC_CD"]);
                MasterNo = Prolink.Math.GetValueAsString(dt.Rows[0]["MASTER_NO"]);
                HouseNo = Prolink.Math.GetValueAsString(dt.Rows[0]["HOUSE_NO"]);
                uid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
            }
            string _act = ISFAcct, _pwd = ISFPWD;

            if (string.IsNullOrEmpty(ScacCd))
            {
                return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_126 }); 
            }
            if (string.IsNullOrEmpty(MasterNo) && string.IsNullOrEmpty(HouseNo))
            {
                return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_132});
            }
            if (!string.IsNullOrEmpty(uid))
            {
                //sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO IN (SELECT PARTY_NO FROM SMSMPT WHERE U_FID={0} AND PARTY_TYPE='RE') AND CMP={1}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(CompanyId));
                sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO IN (SELECT PARTY_NO FROM SMSMPT WHERE U_FID={0} AND PARTY_TYPE='RE')", SQLUtils.QuotedStr(uid));
                DataTable PTdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (PTdt != null && PTdt.Rows.Count > 0)
                {
                    string bondtype = Prolink.Math.GetValueAsString(PTdt.Rows[0]["BOND_TYPE"]);
                    string bondact = Prolink.Math.GetValueAsString(PTdt.Rows[0]["BOND_ACT"]);
                    if (string.IsNullOrEmpty(bondtype) || string.IsNullOrEmpty(bondact))
                    {
                        return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_1 });
                    }
                }
                else
                {
                    return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_2 });
                }
            }
            try
            {
                if (pod.Substring(0, 2) != "US")
                {
                    return Json(new { message = "fail" });
                }
                if (pod == "" || pol == "")
                {
                    return Json(new { message = "POL OR POD MUST TO SET VALUE" });
                }
                using (var client = new WebGui.ISFReference.SFServiceSoapClient())
                {
                    
                    Bussiness.CBPISFPostAssistant data = new Bussiness.CBPISFPostAssistant();
                    string xml = data.GetPostData(ShipmentId, UserId, out _act, out _pwd, GetBaseCmp()).ToXml();

                    if (string.IsNullOrEmpty(_act) || string.IsNullOrEmpty(_pwd))
                    {
                        return Json(new { message = "Importer Account was not set. " });
                    }

                    returnMsg = client.Login(_act, _pwd);
                    returnMsg = client.SendOcean(xml);
                    AfterSendISF(ShipmentId, pol);
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message });
            }
            return Json(returnMsg);

        }

        public ActionResult ExeclToUpdateSmrv()
        {
            string returnMsg = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            if (UId == "")
            {
                returnMsg = "fail";
                return Json(new { message = @Resources.Locale.L_DNManage_Controllers_287 });
            }
            string sql = string.Format("SELECT * FROM SMINM WHERE U_ID='{0}' ", UId);
            DataTable Mdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string invoicetype = string.Empty;
            double ttlnw = 0, ttlgw = 0, ttlcbm = 0;
            if (Mdt.Rows.Count > 0)
            {
                DataRow dr = Mdt.Rows[0];
                invoicetype = Prolink.Math.GetValueAsString(dr["INVOICE_TYPE"]);
                string dn_no = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                string combineinfo = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]);
                sql = string.Format("SELECT * FROM SMINP WHERE U_FID='{0}' ", UId);
                DataTable Pdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = string.Format("SELECT * FROM SMRV WHERE DN_NO LIKE '%{0}%' ", dn_no);
                DataTable Vdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                MixedList ml = new MixedList();
                /*//修改货柜
                foreach (DataRow vdr in Vdt.Rows)
                {
                    string dns = Prolink.Math.GetValueAsString(vdr["DN_NO"]);
                    string cntrno = Prolink.Math.GetValueAsString(vdr["CNTR_NO"]);

                    string[] dnss = combineinfo.Split(',');
                    string dn = SQLUtils.Quoted(dnss);
                    bool check = false;
                    var ttl = GetTTL(Pdt, dn, cntrno, ref check);
                    if (!check) continue;

                    string sql1 = string.Format("SELECT * FROM SMINP WHERE DN_NO IN {0} AND DN_NO !='{1}' AND SHIPMENT_ID='{2}' ORDER BY DN_NO ", dn, DnNo,ShipmentId);
                    DataTable Odt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    var ttl1 = GetOherTtl(Odt, dnss, cntrno);

                    if (ttl.Count > 0)
                    {
                        EditInstruct ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                        ttlnw = Prolink.Math.GetValueAsDouble(ttl["TTL_NW"]);
                        ttlgw = Prolink.Math.GetValueAsDouble(ttl["TTL_GW"]);
                        ttlcbm = Prolink.Math.GetValueAsDouble(ttl["TTL_CBM"]);
                        var TareWeight = Prolink.Math.GetValueAsDouble(vdr["TARE_WEIGHT"]);
                      
                        if (ttl1.Count > 0)
                        {
                            ttlnw += Prolink.Math.GetValueAsDouble(ttl1["TTL_NW"]);
                            ttlgw += Prolink.Math.GetValueAsDouble(ttl1["TTL_GW"]);
                            ttlcbm += Prolink.Math.GetValueAsDouble(ttl1["TTL_CBM"]);
                        }

                        ei.PutKey("U_ID", Prolink.Math.GetValueAsString(vdr["U_ID"]));
                        ei.Put("NW", ttlnw);
                        ei.Put("GW", ttlgw);
                        ei.Put("CBM", ttlcbm);
                        ei.Put("TTL_VGM", ttlgw + TareWeight);
                        ml.Add(ei);
                    }
                }*/


                foreach (DataRow vdr in Vdt.Rows)
                {
                    string dns = Prolink.Math.GetValueAsString(vdr["DN_NO"]);
                    string cntrno = Prolink.Math.GetValueAsString(vdr["CNTR_NO"]);

                    string[] dnss = dns.Split(',');
                    string dn = SQLUtils.Quoted(dnss);

                    string sql1 = string.Format("SELECT * FROM SMINP WHERE DN_NO IN {0} AND SHIPMENT_ID='{1}' ORDER BY DN_NO ", dn, ShipmentId);
                    DataTable Odt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    var ttl = GetOherTtl(Odt, dnss, cntrno);

                    if (ttl.Count > 0)
                    {
                        EditInstruct ei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                        ttlnw = Prolink.Math.GetValueAsDouble(ttl["TTL_NW"]);
                        ttlgw = Prolink.Math.GetValueAsDouble(ttl["TTL_GW"]);
                        ttlcbm = Prolink.Math.GetValueAsDouble(ttl["TTL_CBM"]);
                        var TareWeight = Prolink.Math.GetValueAsDouble(vdr["TARE_WEIGHT"]);
                        ei.PutKey("U_ID", Prolink.Math.GetValueAsString(vdr["U_ID"]));
                        ei.Put("NW", ttlnw);
                        ei.Put("GW", ttlgw);
                        ei.Put("CBM", ttlcbm);
                        ei.Put("TTL_VGM", ttlgw + TareWeight);
                        ml.Add(ei);
                    }
                }
                //修改DN明细
                sql = string.Format("SELECT * FROM SMDN WHERE DN_NO='{0}' ", dn_no);
                DataTable Ndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                foreach (DataRow ndr in Ndt.Rows)
                {
                    double gw = 0, cbm = 0, nw = 0;
                    GetTTL(Pdt, "TTL_NW", ref nw);
                    GetTTL(Pdt, "TTL_GW", ref gw);
                    GetTTL(Pdt, "TTL_CBM", ref cbm);
                    EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", Prolink.Math.GetValueAsString(ndr["U_ID"]));
                    ei.Put("GW", gw);
                    ei.Put("CBM", cbm);
                    ei.Put("NW", nw);
                    ml.Add(ei);
                }

                if (ml.Count > 0)
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                        MixedList ml1 = new MixedList();
                        //修改订舱
                        ttlgw = 0; ttlcbm = 0; ttlnw = 0;
                        sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID='{0}' AND CMP='{1}' AND GROUP_ID='{2}'", ShipmentId, CompanyId, GroupId);
                        DataTable SMdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        foreach (DataRow SMdr in SMdt.Rows)
                        {
                            string [] CombineInfo = Prolink.Math.GetValueAsString(SMdr["COMBINE_INFO"]).Split(',');
                            sql = string.Format("SELECT * FROM SMINM WHERE SHIPMENT_ID='{0}' AND CMP='{1}' AND GROUP_ID='{2}' AND INVOICE_TYPE='{3}' AND DN_NO IN {4}", ShipmentId, CompanyId, GroupId, invoicetype, SQLUtils.Quoted(CombineInfo));
                            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                            foreach (DataRow mdr in dt.Rows)
                            {
                                ttlgw += Prolink.Math.GetValueAsDouble(mdr["TTL_GW"]);
                                ttlcbm += Prolink.Math.GetValueAsDouble(mdr["TTL_CBM"]);
                                ttlnw += Prolink.Math.GetValueAsDouble(mdr["TTL_NW"]);
                            }
                            EditInstruct ei1 = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                            ei1.PutKey("SHIPMENT_ID", ShipmentId);
                            ei1.PutKey("CMP", CompanyId);
                            ei1.PutKey("GROUP_ID", GroupId);
                            ei1.Put("GW", ttlgw);
                            ei1.Put("CBM", ttlcbm);
                            ei1.Put("NW", ttlnw);
                            ml1.Add(ei1);
                        }

                        //string CombinShipment = string.Empty;
                        //if (SMdt.Rows.Count > 0)
                        //{
                        //    CombinShipment = Prolink.Math.GetValueAsString(SMdt.Rows[0]["COMBIN_SHIPMENT"]);
                        //    if (!string.IsNullOrEmpty(CombinShipment))
                        //    {
                        //        sql = string.Format("SELECT * FROM SMSM WHERE COMBIN_SHIPMENT='{0}' AND CMP='{1}' AND GROUP_ID='{2}'", CombinShipment, CompanyId, GroupId);
                        //        DataTable allsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //        foreach (DataRow dr1 in allsmdt.Rows)
                        //        {
                        //            string shipmentid = Prolink.Math.GetValueAsString(dr1["SHIPMENT_ID"]);
                        //            if (shipmentid.Equals(CombinShipment) || shipmentid.Equals(ShipmentId)) continue;
                        //            ttlgw += Prolink.Math.GetValueAsDouble(dr1["GW"]);
                        //            ttlcbm += Prolink.Math.GetValueAsDouble(dr1["CBM"]);
                        //        }
                        //        EditInstruct ei2 = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                        //        ei2.PutKey("SHIPMENT_ID", CombinShipment);
                        //        ei2.PutKey("CMP", CompanyId);
                        //        ei2.PutKey("GROUP_ID", GroupId);
                        //        ei2.Put("GW", ttlgw);
                        //        ei2.Put("CBM", ttlcbm);
                        //        ei2.Put("NW", ttlnw);
                        //        ml1.Add(ei2);
                        //    }
                        //}
                        OperationUtils.ExecuteUpdate(ml1, Prolink.Web.WebContext.GetInstance().GetConnection());

                        TrackingEDI.Business.CaCuLateManager cm = new TrackingEDI.Business.CaCuLateManager();
                        TrackingEDI.Business.TotalQTyCaCuLate tqc = new TrackingEDI.Business.TotalQTyCaCuLate();
                        tqc = cm.CaCulateCombineByDn(dn_no, combineinfo);
                        EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                        cm.CaCulatePutEi(ref ei, tqc, ShipmentId, null);
                        ei.PutKey("SHIPMENT_ID", ShipmentId);
                        ml1 = new MixedList();
                        ml1.Add(ei);
                        OperationUtils.ExecuteUpdate(ml1, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMsg = "fail";
                        return Json(new { message = returnMsg });
                    }
                }
            }

            return Json(new { message = returnMsg });

        }
        public void GetTTL(DataTable dt, string type, ref double ttl)
        {
            
            foreach(DataRow dr in dt.Rows){
                ttl+= Prolink.Math.GetValueAsDouble(dr[type]);
            }
        }
        public Dictionary<string, object> GetOherTtl(DataTable dts, string[] dns, string cntrno)
        {
            bool check = true;
            Dictionary<string, object> ttl = new Dictionary<string, object>();
            Dictionary<string, object> _ttl = new Dictionary<string, object>();
            foreach (string dn in dns)
            {
                var listdt = dts.Select("DN_NO=" + SQLUtils.QuotedStr(dn));
                if (listdt.Length > 0)
                {
                    DataTable dt = listdt.CopyToDataTable();
                    if (dt.Rows.Count > 0)
                    {
                        string Pcntrno = Prolink.Math.GetValueAsString(dt.Rows[0]["CNTR_NO"]);
                        if (Pcntrno != "")
                        {
                            _ttl = GetTTL(dt, "("+SQLUtils.QuotedStr(dn)+")", cntrno, ref  check);
                        }
                        else
                        {
                            _ttl = GetTTL(dt);
                        }
                        if (ttl.Count <= 0) { ttl = _ttl; continue; }
                        foreach (var _num in _ttl)
                        {
                            ttl[_num.Key] = Prolink.Math.GetValueAsDouble(_num.Value) + Prolink.Math.GetValueAsDouble(ttl[_num.Key]);
                        }
                    }
                }
            }
            return ttl;
        }

        public Dictionary<string, object> GetTTL(DataTable dt)
        {
            Dictionary<string, object> ttl = new Dictionary<string, object>();
            string[] column = { "TTL_NW", "TTL_GW", "TTL_CBM" };
            foreach (DataRow dr in dt.Rows)
            {
                if (ttl.Count == 0)
                {
                    foreach (string str in column)
                    {
                        ttl.Add(str, dr[str]);
                    }
                }
                else
                {
                    foreach (string str in column)
                    {
                        object val = new object();
                        ttl[str] = Prolink.Math.GetValueAsDouble(dr[str]) + Prolink.Math.GetValueAsDouble(ttl[str]);
                    }
                }
            }
            return ttl;
        }
        public Dictionary<string, object> GetTTL(DataTable dt, string dn, string cntrno ,ref bool check)
        {
            var data = dt.Select("DN_NO IN " + dn + " AND CNTR_NO= '" + cntrno + "'");
            Dictionary<string, object> ttl = new Dictionary<string, object>();
            if (data.Length <= 0)
            {
                ttl.Add("TTL_NW", 0);
                ttl.Add("TTL_GW", 0);
                ttl.Add("TTL_CBM", 0);
                return ttl;
            }
            DataTable dt1 = data.CopyToDataTable();

            string[] column = { "TTL_NW", "TTL_GW", "TTL_CBM" };
            foreach (DataRow dr in dt1.Rows)
            {
                if (ttl.Count == 0)
                {
                    foreach (string str in column)
                    {
                        ttl.Add(str, dr[str]);
                    }
                }
                else
                {
                    foreach (string str in column)
                    {
                        object val = new object();
                        ttl[str] = Prolink.Math.GetValueAsDouble(dr[str]) + Prolink.Math.GetValueAsDouble(ttl[str]);
                        //ttl.Add(str, Prolink.Math.GetValueAsDouble(dr[str]) + Prolink.Math.GetValueAsDouble(ttl[str]));
                    }
                }
            }
            check = true;
            return ttl;
        }
        public ActionResult GetTransTypeInfo()
        {
            string returnMsg = "success";
            string TransType = Request.Params["TransType"];
            string Cmp = Request.Params["Cmp"];
            if (Cmp == "")
            {
                Cmp = CompanyId;
            }
            string Type = Request.Params["Type"];
            //string sql = "SELECT DISTINCT CHG_CD,CHG_DESCP FROM SMCHG WHERE " + GetBaseGroup() + " AND CMP = " + SQLUtils.QuotedStr(Cmp) + " AND IO_TYPE = 'O' AND TRAN_MODE=" + SQLUtils.QuotedStr(TransType);
            string chgTypeStr = "";
            string chgTypeColsStr = "";
            try
            {
                DataTable dt = Business.TPV.Financial.Bill.GetChargeCodes(Cmp, TransType, Type);//OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count == 0)
                {
                    returnMsg = "fail";
                    return Json(new { message = @Resources.Locale.L_ActManage_Controllers_49 });
                }
                foreach (DataRow dr in dt.Rows)
                {
                    chgTypeStr += Prolink.Math.GetValueAsString(dr["CHG_CD"]) + "-" + Prolink.Math.GetValueAsString(dr["CHG_DESCP"]).Replace("\n", "") + ";";
                    chgTypeColsStr += WebGui.Models.BaseModel.GetModelFiledName(Prolink.Math.GetValueAsString(dr["CHG_CD"])) + ";";
                }
            }
            catch (Exception ex)
            {
                returnMsg = "fail";
                return Json(new { message = returnMsg });
            }
            return Json(new { message = returnMsg, chgTypeStr = chgTypeStr.Substring(0, chgTypeStr.Length - 1), chgTypeColsStr = chgTypeColsStr.Substring(0, chgTypeColsStr.Length - 1), cmp = CompanyId, user = UserId, group = GroupId, createDate = DateTime.Now.ToString("yyyy/MM/dd HH:ss") });

        }

        

        public void AfterSendISF(string shipmentid,string pol)
        {
            MixedList mlist=new MixedList();
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("IORDER", "Y");
            ei.PutDate("ISF_SEND_DATE", DateTime.Now);
            string uext = BookingStatusManager.GetUserFxt(UserId);
            if (string.IsNullOrEmpty(uext)) uext = UserId;
            ei.Put("ISF_WIN", uext); 
            mlist.Add(ei);

            ei = new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("IORDER", "Y");
            ei.PutDate("ISF_SEND_DATE", DateTime.Now);
            mlist.Add(ei);

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    Manager.SaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "050", Sender = UserId, Location = pol, LocationName = "", StsDescp = "ISF Upload" });
                }
                catch (Exception ex)
                {
                }
            }
        }

        #region 取得关联DN DATA
        public ActionResult getRefDnData()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string sql = string.Format(
@";with REF_DATA AS(
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN where DN_NO = {0}
union all
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN inner join REF_DATA r on   r.REF_NO = SMDN.DN_NO 
)
,COM_REF_DATA AS(
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN where DN_NO = {1}
union all
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN inner join COM_REF_DATA r on  r.DN_NO_CMP_REF = SMDN.DN_NO and SMDN.DN_NO <> r.DN_NO
)

select DISTINCT *,(SELECT PARTY_NAME FROM SMDNPT WHERE PARTY_TYPE='FC' AND SMDNPT.DN_NO=DATA.DN_NO ) AS PARTY_NAME from (
select  * from REF_DATA  
union all
select  * from COM_REF_DATA  
)DATA ORDER BY ETD ASC
", SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(DnNo));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmdndModel");
            return ToContent(data);
        }
        #endregion

        public void DownLoadXls()
        {            
            string strResult = string.Empty;
            string strPath = Server.MapPath("~/download");//D:\U_Disk\V3Tracking\WebGui\Config\excel\AIRBSMapping.xml
            string trantype = Prolink.Math.GetValueAsString(Request.Params["TranType"]);
            string filetype = Prolink.Math.GetValueAsString(Request.Params["FileType"]);
            string strName = "Excel.xls";
            switch (trantype)
            {
                case "FCL":
                    strName = "SeaFCLBookingStatus_V1_20240728.xlsx";
                    break;
                case "LCL":
                    strName = "SeaLCLBookingStatus.xlsx";
                    break;
                case "EXP":
                    strName = "ExpressBookingStatus.xlsx";
                    break;
                case "AIR":
                    strName = "AirBookingStatus.xlsx";
                    break;
                case "TK":
                    strName = "TruckingBookingStatus_V1_20240728.xlsx";
                    break;
                case "RAIL":
                    strName = "RailwayBookingStatus.xlsx";
                    break;
                case "DECL":
                    strName = "DECLConfirm.xlsx";
                    break;
                case "FORCAST":
                    strName = "FORECAST_fcst.xlsx";
                    break;
                case "Logistics":
                    strName = "LogisticsImport.xlsx";
                    break;
                case "DnPrice":
                    strName = "DNPriceImport.txt";
                    break;
                case "STS":
                    strName = "ShipmentStatus_byBatch_V1_20240728.xlsx";
                    break;
                case "POD":
                    strName = "BathChCodPOD.xlsx";
                    break;
                case "CustomerProfile":
                    strName = "Customer Profile_V1_20240728.xlsx";
                    break;
                default:
                    strName = "SeaFCLBookingStatus_V1_20240728.xlsx";
                    break;
            }

            switch (filetype)
            {
                case "GwCbm":
                    string name = Prolink.Math.GetValueAsString(Request.Params["filename"]);
                    if (!string.IsNullOrEmpty(name))
                        strName = name;
                    break;
            }
            string strFile = string.Format(@"{0}\{1}", strPath, strName);
            
           
      
            using (FileStream fs = new FileStream(strFile, FileMode.Open))            
            {                
                byte[] bytes = new byte[(int)fs.Length];                
                fs.Read(bytes, 0, bytes.Length);                
                fs.Close();                
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(strName, System.Text.Encoding.UTF8).Replace("+", "%20"));                
                Response.BinaryWrite(bytes);                
                Response.Flush();                
                Response.End();            
            }        
        }

        public ActionResult QuaryXls()
        {
            bool result=false;
            string msg=string.Empty;
            string file = GetBatchUpCbmGw(ref result,ref msg);
            return Json(new { IsOk = result ? "Y" : "N", file = file, msg = msg });
        }
        protected string GetBatchUpCbmGw(ref bool result, ref string msg)
        {
            string strName = "BatchUpCbmGw_V1_20240728.xlsx";
            string strPath = Server.MapPath("~/download");
            string strFile = string.Format(@"{0}\{1}", strPath, strName);
            string virtualCol = Prolink.Math.GetValueAsString(Request.Params["virtualCol"]);
            string table = "(SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + string.Format(" AND EXISTS (SELECT 1 FROM SMSMPT WITH (NOLOCK) WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_NO={0} AND SMSMPT.PARTY_TYPE IN('SP','BO','CR'))", SQLUtils.QuotedStr(this.CompanyId)) + " AND CORDER IS NOT NULL AND CORDER !='N') S";
            table += " UNION SELECT *" + virtualCol + " FROM (SELECT * FROM SMSM WITH (NOLOCK) WHERE GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId) + " AND CMP=" + SQLUtils.QuotedStr(this.CompanyId) + " AND CORDER IS NOT NULL AND CORDER !='N') S" + ") M";
            string trantype = Prolink.Math.GetValueAsString(Request.Params["trantype"]);
            string condition = GetBaseGroup();
            if (trantype != "") condition += " AND TRAN_TYPE=" + SQLUtils.QuotedStr(trantype);
            string conditions = Prolink.Math.GetValueAsString(Request.Params["conditions"]);
            if (conditions != "")
                conditions = HttpUtility.UrlDecode(conditions);
            if (!string.IsNullOrEmpty(conditions))
            {
                string val = ModelFactory.ConvParam2Condition(conditions, "");
                if (condition != "" && val != "") condition +=( " AND " + val);
                else if (condition == "" && val != "") condition = val;
            }
            if (condition == "") condition = " 1=1";
            string sql = string.Format("SELECT SHIPMENT_ID,MASTER_NO,HOUSE_NO FROM {0} WHERE {1}", table, condition);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 500)
            {
                msg= @Resources.Locale.L_DNManageController_Controllers_128;
            }
            else
            {
                NPOIExcelHelp exhelp = new NPOIExcelHelp();
                if (!exhelp.Connect_NOPI(strFile)) {
                    msg = @Resources.Locale.L_DNManage_Controllers_292;
                }
                else{
                    NPOI.SS.UserModel.IWorkbook book = exhelp.DataTableToExcel1(dt, false, 4, true, 0, 0, "Sheet1");
                    exhelp.DeleteRow(null, 0, 2, 3);
                    string FilePath = strFile.Replace(strName, "backup\\");
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    string _strName=strName;
                    strName ="backup\\"+ DateTime.Now.ToString("yyyyMMddHHmmss") + strName;
                    strFile = strFile.Replace(_strName, strName); 
                    using (FileStream file = new FileStream(strFile, FileMode.Create))
                    {
                        book.Write(file);
                        file.Close();
                        result = true;
                    }
                }
            }
            return strName;
        }
        [HttpPost]
        public ActionResult DnPriceUpload(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0) return Json(new { message = @Resources.Locale.L_DNManageController_Controllers_129 });
            List<string> list = new List<string>();
            List<string> exList = new List<string>();
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0) return Json(new { message = "error" });
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string path = Server.MapPath("~/FileUploads/ImportDnPrice/" + UserId + "/");
                bool exists = System.IO.Directory.Exists(path);
                if (!exists)
                    System.IO.Directory.CreateDirectory(path);
                string excelFileName = string.Format("{0}.{1}", path + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                foreach (var dnNo in AnalyzeFilebyPath(excelFileName))
                {
                    try
                    {
                        DnHandel dh = new DnHandel();
                        DataTable dt = dh.GetVSMDNByDnNo(dnNo);
                        if (dt.Rows.Count <= 0) continue;
                        string shipmentid = dt.Rows[0]["SHIPMENT_ID"].ToString();
                        string status = dt.Rows[0]["SM_STATUS"].ToString();
                        string sapId = dt.Rows[0]["SAP_ID"].ToString();
                        string location = dt.Rows[0]["CMP"].ToString();
                        switch (status)
                        {
                            case "P":
                            case "O":
                            case "F":
                                break;
                            default:
                                exList.Add(dnNo + ":"+@Resources.Locale.L_DNManageController_Controllers_130);
                                continue;
                        }
                        Business.TPV.Import.DNManager m = new Business.TPV.Import.DNManager();
                        Business.Service.ResultInfo result = m.ImportDNForSAP(sapId, dnNo, "", Business.TPV.Import.DNImportModes.ReloadQuantity);
                        if (!result.IsSucceed){
                            exList.Add(dnNo+":"+result.Description);
                            continue;
                        }
                        UserInfo userinfo = new UserInfo();
                        userinfo.Dep = Dep;
                        userinfo.CompanyId = CompanyId;
                        userinfo.GroupId = GroupId;
                        userinfo.UserId = UserId;
                        string message = dh.DnCombineSM(dnNo, shipmentid, status, userinfo,true);
                        if (!string.IsNullOrEmpty(message)){
                            exList.Add(dnNo+":"+message);
                            continue;
                        }
                        list.Add(dnNo);
                    }
                    catch
                    {
                        exList.Add(dnNo);
                    }
                }
                if (list.Count > 0)
                {
                    UTF8Encoding utf8 = new UTF8Encoding(false);
                }

            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            returnMessage = @Resources.Locale.L_DNManage_Controllers_296  + list.Count.ToString() + @Resources.Locale.L_DNManageController_Controllers_131  + exList.Count.ToString() +  @Resources.Locale.L_DNManage_Controllers_291;
            if (exList.Count > 0)
            {
                returnMessage += string.Join(Environment.NewLine, exList);
            }

            return Json(new { message = returnMessage });
        }

        IEnumerable<string> AnalyzeFilebyPath(string filepath)
        {
            using (StreamReader reder = new StreamReader(filepath))
            {
                string txt = string.Empty;
                while (!string.IsNullOrEmpty((txt = reder.ReadLine())))
                {
                    yield return txt;
                }
            }

        }

        public ActionResult exportsmdnToExcel()
        {
            string condition = ModelFactory.GetInquiryCondition("SMDN", "", Request.Params);

            string resultType = Request.Params["resultType"];

            DataTable dt = null;
            /*
            string sql = @"SELECT 'U_ID' AS U_ID, 'SHIPMENT_INFO' AS SHIPMENT_INFO, 'DN_NO' AS DN_NO, 'IPART_NO' AS IPART_NO, 'JOB_NO' AS JOB_NO, 'PRODUCT_LINE' AS PRODUCT_LINE, 'OPART_NO' AS OPART_NO 
                                   UNION
                                    SELECT convert(nvarchar(50), U_ID) AS U_ID,  SHIPMENT_INFO, DN_NO, IPART_NO, JOB_NO, PRODUCT_LINE, OPART_NO 
                                   FROM  V_CMSUMMARY WHERE (DOWN_FLAG IS  NULL OR DOWN_FLAG = 'N') ORDER BY DN_NO DESC";
             */
            string sql = string.Format("SELECT top 100 * FROM SMDN WHERE CMP={0} AND STATUS='D' AND APPROVE_TO='A' AND ", SQLUtils.QuotedStr(CompanyId));
            sql += GetPMSByUPri() + DN_APPROVE_CONDITON;
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                sql = "";
                foreach (DataRow item in dt.Rows)
                {
                    item["COMBINE_INFO"] = string.Empty;
                    string UId = Prolink.Math.GetValueAsString(item["U_ID"]);

                    if (UId != "")
                    {
                        //sql += string.Format("UPDATE SMDNP SET DOWN_FLAG='Y', DOWN_USER={0},DOWN_DATE=getdate() WHERE U_ID={1}",
                         //   SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(UId)) + ";";
                        //OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
            }
            string[] d_fileds = new string[] { "ISBOOKING", "ISAPPROVE", "FS", "SP", "CR" };
            foreach (string field in d_fileds)
            {
                if (!dt.Columns.Contains(field))
                {
                    dt.Columns.Add(field, typeof(string));
                }
            }
            BatchDnToExcel batchdntoexcel = new BatchDnToExcel();
            UserInfo userinfo = new UserInfo { UserId = UserId, CompanyId = CompanyId, GroupId = GroupId };
            string xlsFile = batchdntoexcel.ResetXls(userinfo, dt);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^/\\\\]+$");
            return File(xlsFile, "application/vnd.ms-excel", regex.Match(xlsFile).Value);
        }

        public JsonResult chksmdnCount()
        {
            string returnMsg = "success";
           string  condition = ModelFactory.GetInquiryCondition("SMDN", "", Request.Params);
            string Dns = Prolink.Math.GetValueAsString(Request.Params["Dns"]);
            string sql = "SELECT * FROM SMDN WHERE STATUS='D' AND APPROVE_TO='A' AND ";
            sql += GetPMSByUPri();
            sql += DN_APPROVE_CONDITON;
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count == 0)
            {
                returnMsg = "Fail";
            }

            return Json(new { msg = returnMsg });
        }

        public void executeUpdateSql(string sql)
        {

        }

        public ActionResult GetChangeInfo()
        {
            string u_id = Request["uId"];
            Dictionary<string, object> data = GetChangPodInfoByUid(u_id);
            return ToContent(data);
        }

        private Dictionary<string, object> GetChangPodInfoByUid(string u_id)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string shipmentid = mainDt.Rows[0]["SHIPMENT_ID"].ToString();
            sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}  ORDER BY ORDER_BY ASC ", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM CHANGE_INFO WHERE JOB_ID ={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable subDt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            data["main"] = ModelFactory.ToTableJson(mainDt, "SmsmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmsmptModel");
            data["ChangeGrid"] = ModelFactory.ToTableJson(subDt2, "ChangeInfoModel");
            return data;
        }

        
        public ActionResult SaveChangePodBook()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            string u_id = Request.Params["UId"];
            string changereason = Request.Params["changereason"];
            string shipmentid = Request.Params["ShipmentId"];
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        sql = string.Format("SELECT PPOD_CD,PDEST_CD FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
                        DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        EditInstruct changei = new EditInstruct("CHANGE_INFO", EditInstruct.INSERT_OPERATION);
                        changei.Put("U_ID",System.Guid.NewGuid().ToString());
                        changei.Put("JOB_ID", shipmentid);
                        if (dt.Rows.Count > 0)
                        {
                            changei.Put("OLD_INFO", "PPOD_CD:" + Prolink.Math.GetValueAsString(dt.Rows[0]["PPOD_CD"]));
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else
                        {
                            continue;
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "ChangeGrid")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmsmptModel");
                    string su_id = string.Empty;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        string partytype = Prolink.Math.GetValueAsString(ei.Get("PARTY_TYPE"));
                        bool IsChange = false;
                        switch (partytype)
                        {
                            case "FS":
                            case "SP":
                            case "BO":
                            case "BM":
                            case "CR":
                            case "BR":
                                IsChange = true;
                                break;
                        }
                        if (IsChange)
                            continue;
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", u_id);
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                            ei.Put("SHIPMENT_ID", shipmentid);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            su_id = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            if (IsGuidByError(su_id))
                                continue;
                        }
                        mixList.Add(ei);
                        //EditInstruct changei = new EditInstruct("CHANGE_INFO", EditInstruct.INSERT_OPERATION);
                        //changei.Put("U_ID", System.Guid.NewGuid().ToString());
                        //changei.Put("JOB_ID", shipmentid);
                        //sql = "SELECT PARTY_TYPE+':'+PARTY_NO+' ' FROM smsmpt WHERE SHIPMENT_ID='FQB1604000202' FOR XML PATH('') ";
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    SMHandle.SetPartyToSm(shipmentid);

                    UserInfo userinfo = new UserInfo
                    {
                        UserId = UserId,
                        CompanyId = CompanyId,
                        GroupId = GroupId,
                        Dep = Dep
                    };
                    SMHandle.ChangePodAction(u_id, changereason, userinfo);
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            Dictionary<string, object> data = GetChangPodInfoByUid(u_id);
            return ToContent(data);
        }


        [HttpPost]
        public ActionResult UploadDN(FormCollection form)
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
                    mapping = BookingStatusManager.BatchDnInfoMapping;

                    MixedList ml = new MixedList();
                    MixedList partyml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();

                    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                    DataTable baseDt = TrackingEDI.Mail.MailTemplate.GetBaseData("'TCAR'", GroupId, CompanyId);
                    parm["bacodeDt"] = baseDt;
                    parm["mixedlist"] = partyml;
                    parm["combineDictionary"] = dictionary;
                    ExcelParser.RegisterEditInstructFunc(mapping, BookingStatusManager.HandleDnInfoStatus);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm,1);
                    List<string> bookingList = new List<string>();
                    List<string> approveDnList = new List<string>();
                    List<string> approveDnUidList = new List<string>();
                    for (int i = 0; i < ml.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)ml[i];
                        if ("Y".Equals(ei.Get("HTML_IS_BOOKING")))
                        {
                            bookingList.Add(ei.Get("U_ID"));
                        }
                        ei.Remove("HTML_IS_BOOKING");
                        if ("Y".Equals(ei.Get("HTML_IS_APPROVE")))
                        {
                            approveDnList.Add(ei.Get("DN_NO"));
                            approveDnUidList.Add(ei.Get("U_ID"));
                        }
                        ei.Remove("HTML_IS_APPROVE");
                    }
                    
                    partyml = (MixedList)parm["mixedlist"];
                    for (int i = 0; i < partyml.Count; i++)
                    {
                        ml.Add((EditInstruct)partyml[i]);
                    }
                    if (ml.Count <= 0)
                    {
                        returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                    }

                    dictionary = (Dictionary<string, List<string>>)parm["combineDictionary"];
                    if (ml.Count > 0)
                    {
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                            foreach (List<string> dnlist in dictionary.Values)
                            {
                                //dnitems
                                string[] dnitems = dnlist.ToArray();
                                if (dnitems.Length > 1)
                                {
                                   returnMessage += CombineCTN(dnitems);
                                }
                            } 
                            
                            if (bookingList.Count > 0)
                            {
                                TrackingEDI.Business.TransferBooking b = new TrackingEDI.Business.TransferBooking();
                                foreach (string uid in bookingList)
                                {
                                    returnMessage += b.SaveToBooking(uid, UserId);
                                }
                            }
                            if (approveDnList.Count > 0)
                            {
                                for(int i=0;i<approveDnList.Count;i++)
                                {
                                    returnMessage += DNApproveLoop.ApproveDnItem(approveDnUidList[i], approveDnList[i], UserId, Dep, GetBaseCmp(), UPri) + "\n";
                                }
                            }

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
            if(string.IsNullOrEmpty( returnMessage))
                returnMessage = "success";
            return Json(new { message = returnMessage });
        }


    }
}
