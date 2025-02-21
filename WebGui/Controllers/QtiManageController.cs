using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Prolink.Persistence;
using Business;
using System.Globalization;
using System.Text.RegularExpressions;
using TrackingEDI.Business;
using System.IO;

namespace WebGui.Controllers
{
    public class QtiManageController : BaseController
    {
        #region View
        public ActionResult QueryView()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("QTI010");
            ViewBag.CntTypeSel = CommonHelp.getReffeeForColSelect(GetDataPmsCondition("C"));
            ViewBag.BscodeSel = CommonHelp.getBscodeForColModel("CTNY", GetDataPmsCondition("C"));
            return View();
        }

        public ActionResult SetupView(string id = null, string uid = null)
        {
            SetSchema("SMQTI");
            ViewBag.pmsList = GetBtnPms("QTI010");
            ViewBag.Uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            ViewBag.CntTypeSel = CommonHelp.getReffeeForColSelect(GetDataPmsCondition("C"));
            ViewBag.BscodeSel = CommonHelp.getBscodeForColModel("CTNY", GetDataPmsCondition("C"));
            return View();
        }
        #endregion

        #region 纯查询
        public ActionResult QueryData()
        {
            string pri = "G".Equals(UPri) ? "G" : "C"; 
            string con = GetCreateDateCondition("IP_LOG", GetDataPmsCondition(pri));
            return GetBootstrapData("SMQTI", con);
        }

        public ActionResult CheckPod()
        {
            string cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string podcd = Prolink.Math.GetValueAsString(Request.Params["PodCD"]);
            string sql = string.Format("SELECT FROM_CMP FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND FROM_CMP IS NOT NULL", 
                SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(podcd));

            string fromcmp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { FromCmp = fromcmp });
        }
        #endregion

        #region 取得資料
        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string[] uids = u_id.Split(';');
            if (uids.Length > 0)
            {
                string sql = string.Format("SELECT * FROM SMQTI WHERE U_ID IN {0} ORDER BY FROM_CMP DESC, ORDER_SEQ ASC", SQLUtils.Quoted(uids));
                DataTable itemDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = string.Format("SELECT TOP 1 * FROM SMQTI WHERE U_ID IN {0} ORDER BY FROM_CMP DESC, ORDER_SEQ ASC"
                    , SQLUtils.Quoted(uids));
                DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, object> data = new Dictionary<string, object>();
                data["main"] = ModelFactory.ToTableJson(mainDt, "SmqtiModel");
                data["items"] = ModelFactory.ToTableJson(itemDt, "SmqtiModel");
                return ToContent(data);
            }
            return Json(new { message = "" });
        }
        #endregion

        #region 保存
        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string PodCd = Prolink.Math.GetValueAsString(Request.Params["PodCd"]);
            string TerminalCd = Prolink.Math.GetValueAsString(Request.Params["TerminalCd"]);
            string tranType= Prolink.Math.GetValueAsString(Request.Params["TranType"]);
            string shareTo = Prolink.Math.GetValueAsString(Request.Params["ShareTo"]);
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(Cmp))
                Cmp = HttpUtility.UrlDecode(Cmp);

            if (!string.IsNullOrEmpty(PodCd))
                PodCd = HttpUtility.UrlDecode(PodCd);

            if (!string.IsNullOrEmpty(TerminalCd))
                TerminalCd = HttpUtility.UrlDecode(TerminalCd);
            if (!string.IsNullOrEmpty(shareTo))
                shareTo = HttpUtility.UrlDecode(shareTo);
            string[] shares = shareTo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string con = string.Empty;
            if(TerminalCd == "")
            {
                con = " AND (TERMINAL_CD='' OR TERMINAL_CD IS NULL)";
            }
            else
            {
                con = " AND TERMINAL_CD = " + SQLUtils.QuotedStr(TerminalCd);
            }
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            List<string> cmpList = new List<string>(shares);
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmqtiModel");

                    string sql = "SELECT U_ID FROM SMQTI WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND POD_CD=" + SQLUtils.QuotedStr(PodCd) + " AND TRAN_TYPE=" + SQLUtils.QuotedStr(tranType) + con + " AND FROM_CMP IS NULL";
                    if (!cmpList.Contains(Cmp))
                        cmpList.Add(Cmp);
                    if (Cmp != "" && PodCd != "" && !string.IsNullOrEmpty(tranType))
                    {
                        sql += string.Format(" UNION SELECT U_ID FROM SMQTI WHERE POD_CD={0} AND TRAN_TYPE={1}{2} AND FROM_CMP={3}", SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(tranType), con,
                            SQLUtils.QuotedStr(Cmp));
                        DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        List<string> uidList = new List<string>();
                        foreach (DataRow dr in dt.Rows)
                        {
                            string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                            if (uidList.Contains(uid))
                                continue;
                            uidList.Add(uid);
                            EditInstruct tei = new EditInstruct("SMQTI", EditInstruct.DELETE_OPERATION);
                            tei.PutKey("U_ID", uid);
                            mixList.Add(tei);
                        }
                    }
                    else
                    {
                        break;
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (string sCmp in cmpList)
                        {
                            if (string.IsNullOrEmpty(sCmp))
                                continue;
                            string tCmp = sCmp;
                            
                            EditInstruct ei = (EditInstruct)list[i];
                            
                            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                            {
                                EditInstruct tei = new EditInstruct("SMQTI", EditInstruct.INSERT_OPERATION);
                                string[] fileds = ei.getNameSet();
                                foreach (string filed in fileds)
                                {
                                    switch (filed.ToUpper())
                                    {
                                        case "EFFECT_DATE":
                                        case "EXPIRAT_DATE":
                                            tei.PutDate(filed, ei.Get(filed));
                                            break;
                                        default:
                                            tei.Put(filed, ei.Get(filed));
                                            break;
                                    }
                                }
                                string u_id = System.Guid.NewGuid().ToString();
                                tei.Put("U_ID", u_id);
                                tei.Put("GROUP_ID", GroupId);
                                tei.Put("CMP", sCmp);
                                tei.Put("STN", Station);
                                tei.Put("DEP", Dep);
                                tei.Put("POD_CD", PodCd);
                                if (sCmp.Equals(Cmp))
                                    tei.Put("SHARE_TO", shareTo);
                                tei.Put("TRAN_TYPE", tranType);
                                tei.Put("TERMINAL_CD", TerminalCd);
                                tei.Put("Order_Seq", i);
                                tei.Put("MODIFY_BY", UserId);
                                tei.PutDate("MODIFY_DATE", DateTime.Now);
                                if (!sCmp.Equals(Cmp))
                                    tei.Put("FROM_CMP", Cmp);
                                mixList.Add(tei);
                            }
                            
                        }
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

            string sql1 = string.Format("SELECT * FROM SMQTI WHERE POD_CD={0}", SQLUtils.QuotedStr(PodCd)) + " AND CMP=" + SQLUtils.QuotedStr(Cmp) + " AND TRAN_TYPE=" + 
                SQLUtils.QuotedStr(tranType) + con + "ORDER BY Order_Seq" ;
            DataTable itemDt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql1 = string.Format("SELECT top 1 * FROM SMQTI WHERE POD_CD={0}", SQLUtils.QuotedStr(PodCd)) + " AND CMP=" + SQLUtils.QuotedStr(Cmp) + " AND TRAN_TYPE=" +
                SQLUtils.QuotedStr(tranType) + con + " ORDER BY FROM_CMP DESC, ORDER_SEQ ASC";
            DataTable mainDt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmqtiModel");
            data["items"] = ModelFactory.ToTableJson(itemDt, "SmqtiModel");
            return ToContent(data);
        }

        public ActionResult UpdateDataNew()
        {
            string changeData = Request.Params["changedData"];
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string PodCd = Prolink.Math.GetValueAsString(Request.Params["PodCd"]);
            string TerminalCd = Prolink.Math.GetValueAsString(Request.Params["TerminalCd"]);
            string tranType = Prolink.Math.GetValueAsString(Request.Params["TranType"]);
            string shareTo = Prolink.Math.GetValueAsString(Request.Params["ShareTo"]);
            string mapUid = Prolink.Math.GetValueAsString(Request.Params["MapUId"]); 
            List<Dictionary<string, object>> smexData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (!string.IsNullOrEmpty(Cmp))
                Cmp = HttpUtility.UrlDecode(Cmp);

            if (!string.IsNullOrEmpty(PodCd))
                PodCd = HttpUtility.UrlDecode(PodCd);

            if (!string.IsNullOrEmpty(TerminalCd))
                TerminalCd = HttpUtility.UrlDecode(TerminalCd);
            if (!string.IsNullOrEmpty(shareTo))
                shareTo = HttpUtility.UrlDecode(shareTo);
            if (!string.IsNullOrEmpty(mapUid))
                mapUid = HttpUtility.UrlDecode(mapUid);

            string[] uids = mapUid.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> uidList = new List<string>(uids);
            string con = " AND (TERMINAL_CD='' OR TERMINAL_CD IS NULL)"; ;
            if(!string.IsNullOrEmpty(TerminalCd))
                con = " AND TERMINAL_CD = " + SQLUtils.QuotedStr(TerminalCd);
            string sql = string.Format("SELECT TOP 1 MAX(ORDER_SEQ) AS MAX_SEQ FROM SMQTI WHERE CMP={0} AND POD_CD={1} AND TRAN_TYPE={2} AND FROM_CMP IS NULL{3}",
                SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(tranType), con);
            int maxOrdSeq = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
             

            MixedList mixedList = new MixedList();
            sql = string.Format("SELECT SHARE_TO,U_ID FROM SMQTI WHERE U_ID IN{0}", SQLUtils.Quoted(uids));
            DataTable oldDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            List<string> queryList = new List<string>();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmqtiModel");
                    int j = maxOrdSeq + 1;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            EditInstruct tei = new EditInstruct("SMQTI", EditInstruct.INSERT_OPERATION);
                            string u_id = ei.Get("U_ID");
                            if (!string.IsNullOrEmpty(u_id))
                            {
                                tei = new EditInstruct("SMQTI", EditInstruct.UPDATE_OPERATION);
                                tei.PutKey("U_ID", u_id);
                            }
                            SetFieldValue(tei, ei, ref u_id, j);
                            tei.Put("CMP", Cmp);
                            tei.Put("POD_CD", PodCd);
                            tei.Put("SHARE_TO", shareTo);
                            tei.Put("TRAN_TYPE", tranType);
                            tei.Put("TERMINAL_CD", TerminalCd); 
                            if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                            {
                                tei.PutDate("CREATE_DATE", DateTime.Now);
                                tei.Put("CREATE_BY", UserId);
                            }
                            mixList.Add(tei);


                           
                            string oldshareTo = shareTo;
                            if (tei.OperationType == EditInstruct.UPDATE_OPERATION)
                            {
                                sql = string.Format("SELECT SHARE_TO FROM SMQTI WHERE U_ID={0} ", SQLUtils.QuotedStr(u_id));
                                oldshareTo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            } 
                            string[] share_tos = oldshareTo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string oldcmp in share_tos)
                            {
                                string id = ei.Get("U_ID");
                                EditInstruct nei = new EditInstruct("SMQTI", EditInstruct.INSERT_OPERATION);
                                if (tei.OperationType == EditInstruct.UPDATE_OPERATION)
                                {
                                    nei = new EditInstruct("SMQTI", EditInstruct.UPDATE_OPERATION);
                                    nei.PutKey("REF_NO", id);
                                    nei.PutKey("CMP", oldcmp);
                                }
                                SetFieldValue(nei, ei, ref id, j);
                                nei.Put("CMP", oldcmp);
                                nei.Put("POD_CD", PodCd);
                                nei.Put("TRAN_TYPE", tranType);
                                nei.Put("TERMINAL_CD", TerminalCd);
                                nei.Put("FROM_CMP", Cmp);
                                nei.Put("REF_NO", u_id); if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                                {
                                    nei.PutDate("CREATE_DATE", DateTime.Now);
                                    nei.Put("CREATE_BY", UserId);
                                }
                                mixList.Add(nei);
                            }

                            if (uidList.Contains(u_id))
                                uidList.Remove(u_id);
                            if (!queryList.Contains(u_id) && !string.IsNullOrEmpty(u_id))
                                queryList.Add(u_id);
                            j++;
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (string.IsNullOrEmpty(ei.Get("U_ID")))
                                continue;
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            mixList.Add(ei);
                            EditInstruct dei = new EditInstruct("SMQTI", EditInstruct.DELETE_OPERATION);
                            dei.PutKey("REF_NO", ei.Get("U_ID"));
                            mixList.Add(dei);
                        }
                    }
                }
            }
            foreach (string uid in uidList)
            {
                if (string.IsNullOrEmpty(uid))
                    continue;
                EditInstruct dei = new EditInstruct("SMQTI", EditInstruct.DELETE_OPERATION);
                dei.PutKey("U_ID", uid);
                mixList.Add(dei);
                dei = new EditInstruct("SMQTI", EditInstruct.DELETE_OPERATION);
                dei.PutKey("REF_NO", uid);
                mixList.Add(dei);
            }
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());

                    string oldshare_to = string.Empty;
                    string thisUid = string.Empty;
                    foreach (DataRow dr in oldDt.Rows)
                    {
                        oldshare_to = Prolink.Math.GetValueAsString(dr["SHARE_TO"]);
                        thisUid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                        sql = string.Format("SELECT * FROM SMQTI WHERE U_ID ={0}", SQLUtils.QuotedStr(thisUid));
                        DataTable updateDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        List<string> AddShareCmp = GetShareCmp(shareTo, oldshare_to);
                        List<string> DelShareCmp = GetShareCmp(oldshare_to, shareTo);
                        if (DelShareCmp.Count > 0 || AddShareCmp.Count > 0)
                        {
                            RemoveShareByCmp(Cmp, DelShareCmp, thisUid, mixedList);
                            AddSharetoCmp(updateDt, AddShareCmp, mixedList);
                            UpdateShareTo(shareTo, oldshare_to, thisUid, mixedList);
                        }
                    }
                    OperationUtils.ExecuteUpdate(mixedList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            string sql1 = string.Format("SELECT * FROM SMQTI WHERE U_ID IN {0} ORDER BY FROM_CMP DESC, ORDER_SEQ ASC", SQLUtils.Quoted(queryList.ToArray()));
            if(queryList.Count<=0)
                sql1 = string.Format("SELECT * FROM SMQTI WHERE 1=0 ORDER BY FROM_CMP DESC, ORDER_SEQ ASC");
            DataTable itemDt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql1 = string.Format("SELECT top 1 * FROM SMQTI WHERE U_ID IN {0} ORDER BY FROM_CMP DESC, ORDER_SEQ ASC", SQLUtils.Quoted(queryList.ToArray()));
            if (queryList.Count <= 0)
                sql1 = string.Format("SELECT top 1 * FROM SMQTI WHERE 1=0 ORDER BY FROM_CMP DESC, ORDER_SEQ ASC");
            DataTable mainDt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmqtiModel");
            data["items"] = ModelFactory.ToTableJson(itemDt, "SmqtiModel");
            return ToContent(data);
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
                    case "SMQTI":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMQTI WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        private ActionResult GetBaseData(string table, string condition, string colNames = "*", string orderBy = "", string qType = "")
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 20;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            switch (qType)
            {
                case "1":
                    dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                case "2":
                    dt = ModelFactory.InquiryData("*", table, condition, orderBy, pageIndex, pageSize, ref  recordsCount);
                    break;
                case "3":
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                default:
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        #endregion


        [HttpPost]
        public ActionResult UploadPortFree(FormCollection form)
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
                    mapping = InboundUploadExcelManager.InboundPortFreeMapping;

                    MixedList ml = new MixedList();
                    MixedList partyml = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();

                    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                    List<string> shipmentids = new List<string>();
                    //parm["groupid"] = GetDataPmsCondition("C");
                    parm["groupid"] = GroupId;
                    parm["companyid"] = CompanyId;
                    parm["station"] = Station;
                    parm["dep"] = Dep;
                    parm["userId"] = UserId; 
                    ExcelParser.RegisterEditInstructFunc(mapping, InboundUploadExcelManager.HandlePortFreeExcel);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm, 0);

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
            string strName = "PortFreeDate_Upload_Format_V2_20241124.xlsx";
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

        public void RemoveShareByCmp(string fromCmp,List<string> removeCmp, string uId,MixedList ml)
        {
            if (string.IsNullOrEmpty(uId))
                return;
            foreach (string cmp in removeCmp)
            {
                string sql = string.Format("DELETE FROM SMQTI WHERE REF_NO= {0} AND FROM_CMP={1} AND CMP={2}",
                    SQLUtils.QuotedStr(uId), SQLUtils.QuotedStr(fromCmp), SQLUtils.QuotedStr(cmp));
                ml.Add(sql);
            }
        }

        public void AddSharetoCmp(DataTable qtiDt, List<string> addCmp,MixedList ml)
        {
            if (addCmp.Count <= 0)
                return;
            foreach (string cmp in addCmp)
            {
                if (string.IsNullOrEmpty(cmp))
                    continue;
                foreach (DataRow dr in qtiDt.Rows)
                {
                    EditInstruct tei = new EditInstruct("SMQTI", EditInstruct.INSERT_OPERATION);
                    tei.Put("U_ID", System.Guid.NewGuid().ToString());
                    tei.Put("GROUP_ID", GroupId);
                    tei.Put("STN", Station);
                    tei.Put("DEP", Dep);
                    tei.Put("MODIFY_BY", UserId);
                    tei.PutDate("MODIFY_DATE", DateTime.Now);
                    tei.PutDate("CREATE_DATE", DateTime.Now);
                    tei.Put("CREATE_BY", UserId);
                    tei.Put("CMP", cmp);
                    tei.Put("FROM_CMP", Prolink.Math.GetValueAsString(dr["CMP"]));
                    tei.Put("REF_NO", Prolink.Math.GetValueAsString(dr["U_ID"]));
                    foreach (DataColumn col in qtiDt.Columns)
                    {
                        switch (col.ColumnName.ToUpper())
                        {
                            case "POD_CD":
                            case "TRAN_TYPE":
                            case "TERMINAL_CD":
                            case "I_TYPE":
                            case "CHG_CD":
                            case "CARRIER_CD":
                            case "CARRIER_NM":
                            case "S_DAY":
                            case "E_DAY":
                            case "CNT_TYPE":
                            case "CNT_DESCP":
                            case "FEE_PER_DAY":
                            case "CUR":
                            case "EMPTY_RETURN":
                            case "CAL_TYPE":
                            case "PERCENTAGE":
                            case "FOB_CIF":
                            case "CAL_DATE":
                            case "LSP_NO":
                            case "LSP_NM":
                            case "CHG_DAY_TYPE":
                                tei.Put(col.ColumnName.ToUpper(), Prolink.Math.GetValueAsString(dr[col.ColumnName.ToUpper()]));
                                break;
                            case "EFFECT_DATE":
                            case "EXPIRAT_DATE":
                                tei.PutDate(col.ColumnName.ToUpper(), Prolink.Math.GetValueAsDateTime(dr[col.ColumnName.ToUpper()]));
                                break;
                        }
                    }
                    ml.Add(tei);
                }
            }
        } 

        public void UpdateShareTo(string shareTo, string oldshareTto,string uId, MixedList ml)
        {
            if (string.IsNullOrEmpty(uId))
                return; 
            string sql = string.Format("UPDATE SMQTI SET SHARE_TO={0} WHERE U_ID={1}", SQLUtils.QuotedStr(shareTo), SQLUtils.QuotedStr(uId));
            ml.Add(sql);
        }

        public List<string> GetShareCmp(string share_to,string shareTo)
        {
            List<string> ShareCmp = new List<string>();
            string[] share_tos = share_to.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string[] shares = shareTo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string share in share_tos)
            {
                if (string.IsNullOrEmpty(share))
                    continue;
                if (!shares.Contains(share))
                    ShareCmp.Add(share);
            }
            return ShareCmp;
        }

        public void SetFieldValue(EditInstruct tei, EditInstruct ei,ref string u_id,int req)
        {
            string[] fileds = ei.getNameSet();
            foreach (string filed in fileds)
            {
                switch (filed.ToUpper())
                {
                    case "U_ID":
                        break;
                    case "EFFECT_DATE":
                    case "EXPIRAT_DATE":
                        tei.PutDate(filed, ei.Get(filed));
                        break;
                    default:
                        tei.Put(filed, ei.Get(filed));
                        break;
                }
            }
            if (string.IsNullOrEmpty(u_id))
            {
                u_id = System.Guid.NewGuid().ToString();
                tei.Put("U_ID", u_id);
                tei.Put("ORDER_SEQ", req);
            }
            tei.Put("GROUP_ID", GroupId);
            tei.Put("STN", Station);
            tei.Put("DEP", Dep);
            tei.Put("MODIFY_BY", UserId);
            tei.PutDate("MODIFY_DATE", DateTime.Now);
        }

    }
}
